using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class VisitsRepository : IVisitsRepository
    {
        private readonly DataContext _context;
        public VisitsRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<UserView> GetUserLike(int  sourceUserId, int targetUserId)
        {
             return await _context.Visits.FindAsync(sourceUserId,targetUserId);
        }

        public async Task<PagedList<VisitDto>> GetUserLikes(VisitsParams visitsParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var visits = _context.Likes.AsQueryable();

            if (visitsParams.Predicate == "Viewed")
            {
                visits = visits.Where(visit => visit.SourceUserId == visitsParams.UserId);
                users = visits.Select(visit => visit.TargetUser);
            }

            if (visitsParams.Predicate == "ViewBy")
            {
                visits = visits.Where(visit => visit.TargetUserId == visitsParams.UserId);
                users = visits.Select(visit => visit.SourceUser);
            }

            var viewedUsers = users.Select(user => new VisitDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalcuateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            return await PagedList<VisitDto>.CreateAsync(viewedUsers, visitsParams.PageNumber, visitsParams.PageSize);
        }

      

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.ViewedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}