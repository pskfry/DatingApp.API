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
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            
            // filter out current user
            users = users.Where(u => u.Id != userParams.UsersId);

            // filter out same-gender users
            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                // filter by age
                users = users.Where(u => Extensions.CalculateAge(u.BirthDate) >= userParams.MinAge);
                users = users.Where(u => Extensions.CalculateAge(u.BirthDate) <= userParams.MaxAge);
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

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int Id)
        {
            return await _context.Photos.FirstOrDefaultAsync(p => p.Id == Id);
        }

        public async Task<Photo> GetMainPhoto(int Id)
        {
            return await _context.Photos.Where(o => o.UserId == Id && o.isMainPhoto == true).FirstOrDefaultAsync();
        }
    }
}