using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System;
using AutoMapper;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _config = config;
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDTO user)
        {
            if (!string.IsNullOrEmpty(user.UserName))
                user.UserName = user.UserName.ToLower();

            if (await _repo.UserExists(user.UserName))
                ModelState.AddModelError("UserName", "username already exists.");

            // validate request
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userToCreate = new User
            {
                UserName = user.UserName
            };

            var createdUser = await _repo.Register(userToCreate, user.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UserForLoginDTO user)
        {
            var userFromRepo = await _repo.Login(user.UserName.ToLower(), user.Password);

            if (userFromRepo == null)
                ModelState.AddModelError("Credentials", "Incorrect login credentials");

            if (!ModelState.IsValid)
                return Unauthorized();

            // generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userFromRepo.UserName),
                }),
                Expires = System.DateTime.Now.AddDays(1),
                NotBefore = System.DateTime.Now,
                IssuedAt = System.DateTime.Now,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            // transfer to smaller UserForListDTO
            var userForNav = _mapper.Map<UserForListDTO>(userFromRepo);

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var jwtToken = new JsonResult(tokenString);

            //return Ok(jwtToken);
            return Ok(new { jwtToken, userForNav });
        }
    }
}