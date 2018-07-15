using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
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
        public async Task<IActionResult> GetUsers(UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);
            userParams.UsersId = currentUserId;

            if(String.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = (userFromRepo.Gender == "male") ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);

            // map returned users to DTO
            var mappedUsers = _mapper.Map<IEnumerable<UserForListDTO>>(users);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            
            return Ok(mappedUsers);
        }

        [HttpGet("{Id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int Id)
        {
            var user = await _repo.GetUser(Id);

            // map returned user to DTO
            var mappedUser = _mapper.Map<UserForDetailedDTO>(user);

            return Ok(mappedUser);
        }

        // api/users/1 PUT
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateUser(int Id, [FromBody] UserForUpdateDTO userForUpdateDTO)
        {
            if(!ModelState.IsValid) 
                return BadRequest(ModelState);

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _repo.GetUser(Id);

            if(userFromRepo == null)
                return NotFound($"Could not find user with an ID of {Id}");

            if(currentUserId != userFromRepo.Id)
                return Unauthorized();

            _mapper.Map(userForUpdateDTO, userFromRepo);

            if (await _repo.SaveAll())
                return NoContent();

            throw new Exception($"Updates to user {Id} failed on save.");
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser(User user)
        {
            _repo.Add(user);
            bool changes = await _repo.SaveAll();

            return Ok(changes);
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = this.GetUser(id);
            _repo.Delete(user);
            var changes = await _repo.SaveAll();

            return Ok(changes);
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _repo.GetLike(id, recipientId);

            if (like != null)
                return BadRequest("You already liked this user!");

            if (await _repo.GetUser(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like
            {
                LikeeId = id,
                LikerId = recipientId
            };

            _repo.Add<Like>(like);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");
        }
    }
}