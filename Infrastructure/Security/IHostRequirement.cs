using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Security
{
    public class IsHostRequirement : IAuthorizationRequirement //for custom auth policy,
    {
    }

    public class IsHostRequirementHandler : AuthorizationHandler<IsHostRequirement>
    {
        private readonly DataContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IsHostRequirementHandler(DataContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        // to check if the one updating activities is the host
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsHostRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Task.CompletedTask;

            var activityId = Guid.Parse(_httpContextAccessor.HttpContext?.Request.RouteValues
                .SingleOrDefault(x => x.Key == "id").Value?.ToString());

            var attendee = _dbContext.ActivityAttendees
                .AsNoTracking() //AsNoTracking cannot be used with FindAsync
                .FirstOrDefaultAsync(x => x.AppUserId == userId && x.ActivityId == activityId) //FindAsync is always going to track an activity or an entity, and we cannot use as no tracking with it
                .Result; //As this is no tracking, this object will not be kept in memory

            if (attendee == null) return Task.CompletedTask; //will not meet authorization requirement

            if (attendee.IsHost) context.Succeed(requirement);

            return Task.CompletedTask; // if we return at this point, then the user would be authorized to go ahead and edit the activity.
        }
    }
}