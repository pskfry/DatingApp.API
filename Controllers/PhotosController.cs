using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : Controller
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly Cloudinary _cloudinary;
        public PhotosController(IDatingRepository repo, 
            IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account cloudinaryAccount = new Account(_cloudinaryConfig.Value.CloudName, 
                _cloudinaryConfig.Value.ApiKey, 
                _cloudinaryConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(cloudinaryAccount);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDTO>(photoFromRepo);

            if(photoFromRepo == null)
                return BadRequest();

            return Ok(photo);
        }

        [HttpDelete("{id}", Name = "DeletePhoto")]
        public async Task<IActionResult> DeletePhoto(int id, int userId)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            if(photoFromRepo == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != currentUserId)
                return Unauthorized();

            if(photoFromRepo.isMainPhoto)
                return BadRequest("You cannot delete your main photo!");

            if(photoFromRepo.publicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.publicId);
                var deleteAttempt = _cloudinary.Destroy(deleteParams);

                if(deleteAttempt.StatusCode == HttpStatusCode.OK)
                    _repo.Delete<Photo>(photoFromRepo);
            }

            if(photoFromRepo.publicId == null)
            {
                _repo.Delete<Photo>(photoFromRepo);
            }

            if(await _repo.SaveAll())
                return Ok();

            return BadRequest("Could not delete photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> UpdateMainPhoto(int userId, [FromBody]PhotoForUserDTO photoFromBody)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if(userId != currentUserId)
                return Unauthorized();

            var newMainPhoto = await _repo.GetPhoto(photoFromBody.Id);

            if(newMainPhoto == null)
                return NotFound();

            if(newMainPhoto.isMainPhoto)
                return BadRequest("This is already your main photo");

            var currentMainPhoto = await _repo.GetMainPhoto(userId);

            currentMainPhoto.isMainPhoto = false;
            newMainPhoto.isMainPhoto = true;

            if(await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not make this your main photo");
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto(PhotoForCreationDTO photoDTO, int userId)
        {
            var user = await _repo.GetUser(userId);

            if (user == null)
                return BadRequest("Could not find user");

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (currentUserId != userId)
                return Unauthorized();

            var file = photoDTO.File;
            var uploadResult = new ImageUploadResult();

            if(file.Length > 0)
            {
                using(var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(1000).Height(1000).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoDTO.Url = uploadResult.Uri.ToString();
            photoDTO.publicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoDTO);
            photo.User = user;

            if(!user.Photos.Any(p => p.isMainPhoto = true))
                photo.isMainPhoto = true;

            user.Photos.Add(photo);

            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDTO>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Could not add photo");
        }
    }
}