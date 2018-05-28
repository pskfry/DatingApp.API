using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using DatingApp.API.Models;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        private readonly DataContext _context;
        public Seed(DataContext context)
        {
            _context = context;
        }

        public void SeedData(){
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(userData);

            foreach (User user in users)
            {
                // create pw hash
                byte[] passwordHash, passwordSalt;

                using (var hmac = new System.Security.Cryptography.HMACSHA512()){
                    passwordSalt = hmac.Key;
                    passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password"));;

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }

                // add user to db
                _context.Users.Add(user);
            }

            var userDataM = System.IO.File.ReadAllText("Data/UserSeedDataMen.json");
            var usersM = JsonConvert.DeserializeObject<List<User>>(userDataM);

            foreach (User user in usersM)
            {
                // create pw hash
                byte[] passwordHash, passwordSalt;

                using (var hmac = new System.Security.Cryptography.HMACSHA512()){
                    passwordSalt = hmac.Key;
                    passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password"));;

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }

                // add user to db
                _context.Users.Add(user);
            }

            _context.SaveChanges();
        }
    }
}