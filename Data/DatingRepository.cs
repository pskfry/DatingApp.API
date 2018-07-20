using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int Id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Id == Id);

            if(user == null)
                return null;

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users
                .Include(p => p.Photos)
                .OrderByDescending(u => u.LastActive)
                .AsQueryable();
            
            if(!userParams.Likees && !userParams.Likers)
            {
                // filter out current user
                users = users.Where(u => u.Id != userParams.UsersId);

                // filter out same-gender users
                users = users.Where(u => u.Gender == userParams.Gender);

                if (userParams.MinAge != 18 || userParams.MaxAge != 99)
                {
                    // filter by age
                    var min = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                    var max = DateTime.Today.AddYears(-userParams.MinAge);
                    users = users.Where(u => u.BirthDate >= min && u.BirthDate <= max);
                }
            }

            // filter likes
            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UsersId, userParams.Likers);
                users = users.Where(u => userLikers.Any(liker => liker.LikerId == u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UsersId, userParams.Likers);
                users = users.Where(u => userLikees.Any(likee => likee.LikeeId == u.Id));
            }

            if (userParams.OrderBy != null)
            {
                switch (userParams.OrderBy.ToUpper())
                {
                    case "CREATED":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            
            if (users == null)
                return null;
            
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<Like>> GetUserLikes(int id, bool likers)
        {
            // get the user requested, include the list of liker/likees from the user object
            var user = await _context.Users
                .Include(x => x.Likee)
                .Include(x => x.Liker)
                .FirstOrDefaultAsync(u => u.Id == id);

            // if likers is true, we want list of people user liked
            // else get list of people who liked user (user is the likee)
            if (likers)
            {
                return user.Likee.Where(u => u.LikeeId == id);
            }
            else
            {
                return user.Liker.Where(u => u.LikerId == id);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int Id)
        {
            return await _context.Photos
                .FirstOrDefaultAsync(p => p.Id == Id);
        }

        public async Task<Photo> GetMainPhoto(int Id)
        {
            return await _context.Photos
                .Where(o => o.UserId == Id)
                .FirstOrDefaultAsync(o => o.isMainPhoto == true);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.LikeeId == userId && l.LikerId == recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(m => m.Sender)
                .ThenInclude(p => p.Photos)
                .Include(m => m.Recipient)
                .ThenInclude(p => p.Photos).AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "inbox":
                    messages = messages.Where(r => r.RecipientId == messageParams.UsersId && r.RecipientDeleted == false);
                    break;
                case "outbox":
                    messages = messages.Where(s => s.SenderId == messageParams.UsersId && s.SenderDeleted == false);
                    break;
                case "unread":
                    messages = messages.Where(r => r.RecipientId == messageParams.UsersId && r.IsRead == false && r.RecipientDeleted == false);
                    break;
            }

            messages = messages.OrderByDescending(m => m.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(p => p.Photos)
                .Include(m => m.Recipient).ThenInclude(p => p.Photos)
                .Where(m => (m.RecipientId == userId && m.RecipientDeleted == false && m.SenderId == recipientId) 
                    || (m.RecipientId == recipientId && m.SenderDeleted == false && m.SenderId == userId))
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            return messages;
        }
    }
}