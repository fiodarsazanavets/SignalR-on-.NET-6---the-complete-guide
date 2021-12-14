using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs
{
    public class LearningHub : Hub<ILearningHubClient>
    {
        public async Task BroadcastMessage(string message)
        {
            await Clients.All.ReceiveMessage(GetMessageToSend(message));
        }
        
        public async Task SendToOthers(string message)
        {
            await Clients.Others.ReceiveMessage(GetMessageToSend(message));
        }

        public async Task SendToCaller(string message)
        {
            await Clients.Caller.ReceiveMessage(GetMessageToSend(message));
        }

        public async Task SendToIndividual(string connectionId, string message)
        {
            await Clients.Client(connectionId).ReceiveMessage(GetMessageToSend(message));
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        private string GetMessageToSend(string originalMessage)
        {
            return $"User connection id: {Context.ConnectionId}. Message: {originalMessage}";
        }
    }
}
