using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class VisitsController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IVisitsRepository _visitsRepository;
        public VisitsController(IUserRepository userRepository, IVisitsRepository visitsRepository)
        {
            _visitsRepository = visitsRepository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddVisit(string username)
        {
            var sourceUserId = User.GetUserId();
            var viewedUser = await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _visitsRepository.GetUserWithLikes(sourceUserId);

            if (viewedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot View yourself");

            var userView = await _visitsRepository.GetUserLike(sourceUserId, viewedUser.Id);

            if (userView != null) return BadRequest("You already View this user");

          userView = new UserView
            {
               SourceUserId = sourceUserId,
                TargetUserId = viewedUser.Id
            };

            sourceUser.ViewedUsers.Add(userView);

           if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to view user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<VisitDto>>> GetUserLikes([FromQuery]VisitsParams visitsParams)
        {
            visitsParams.UserId = User.GetUserId();

            var users = await _visitsRepository.GetUserLikes(visitsParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, 
                users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}