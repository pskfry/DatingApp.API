using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();

            // map returned users to DTO
            var mappedUsers = _mapper.Map<IEnumerable<UserForListDTO>>(users);
            
            return Ok(mappedUsers);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetUser(int Id)
        {
            var user = await _repo.GetUser(Id);

            // map returned user to DTO
            var mappedUser = _mapper.Map<UserForDetailedDTO>(user);

            return Ok(mappedUser);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser(User user)
        {
            _repo.Add(user);
            bool changes = await _repo.SaveAll();

            return Ok(changes);
        }

        [HttpPost("delete/{Id}")]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            var user = this.GetUser(Id);
            _repo.Delete(user);
            var changes = await _repo.SaveAll();

            return Ok(changes);
        }
    }
}