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

            var attendee = _dbContext.ActivityAttendees //when we're getting our attendee objects from entity framework, this is tracking the entity that we're getting.
                .FindAsync(userId, activityId).Result; //And this stays in memory, even though our handler will have been disposed of because it's a transient,
//it doesn't mean that the entity that we've obtained from entity framework is also going to be disposed.
//This is staying in memory and it's causing a problem when we're editing an activity because we're only sending up the activity object.

            if (attendee == null) return Task.CompletedTask; //will not meet authorization requirement

            if (attendee.IsHost) context.Succeed(requirement);

            return Task.CompletedTask; // if we return at this point, then the user would be authorized to go ahead and edit the activity.
        }
    }
}