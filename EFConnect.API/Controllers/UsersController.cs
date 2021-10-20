using EFConnect.Contracts;
using EFConnect.Data.Entities;
using EFConnect.Helpers;
using EFConnect.Models;
using EFConnect.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EFConnect.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUser(id);

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserForUpdate userForUpdate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var userFromRepo = await _userService.GetUser(id);

            if (userFromRepo == null)
                return NotFound($"User could not be found.");

            if (currentUserId != userFromRepo.Id)
                return Unauthorized();

            if (await _userService.UpdateUser(id, userForUpdate))
                return NoContent();

            throw new Exception($"Updating user failed on save.");
        }

        [HttpPost("{id}/follow/{recipientId}")]
        public async Task<IActionResult> FollowUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))               //  1.
                return Unauthorized();

            var follow = await _userService.GetFollow(id, recipientId);                         //  2.

            if (follow != null)                                                                 //  3.
                return BadRequest("You already followed this user.");

            if (await _userService.GetUser(recipientId) == null)                                //  4.
                return NotFound();

            follow = new Follow                                                                 //  5.
            {
                FollowerId = id,
                FolloweeId = recipientId
            };

            _userService.Add<Follow>(follow);                                                   //  6.

            if (await _userService.SaveAll())                                                   //  7.
                return Ok();

            return BadRequest("Failed to add user.");                                           //  8.
        }

        [HttpGet("PG")]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            userParams.UserId = currentUserId;

            var users = await _userService.GetUsers(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("Follow")]
        public async Task<IActionResult> GetFollow([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            userParams.UserId = currentUserId;

            var users = await _userService.GetFollowUsers(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}
