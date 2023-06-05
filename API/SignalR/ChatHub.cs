using Microsoft.AspNetCore.SignalR;
using Application.Comments;
using MediatR;

namespace API.SignalR
{
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;
        public ChatHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        //our client will make a connection to this hub
        public async Task SendComment(Create.Command command)
        {
            var comment = await _mediator.Send(command);
        
            await Clients.Group(command.ActivityId.ToString()) //send it to group instead of the caller alone
                .SendAsync("ReceiveComment", comment.Value);
        }
        //when a client connects to our hub, we want them to join a group.
        public override async Task OnConnectedAsync() // automatically going to remove this client (connectionID) from any groups when a client disconnects from signalR
        {
            var httpContext = Context.GetHttpContext();
            var activityId = httpContext.Request.Query["activityId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, activityId);//activityId as a group name
            var result = await _mediator.Send(new List.Query{ActivityId = Guid.Parse(activityId)});
            await Clients.Caller.SendAsync("LoadComments", result.Value);
        }
    }
}