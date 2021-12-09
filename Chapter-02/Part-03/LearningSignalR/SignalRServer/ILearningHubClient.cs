namespace SignalRServer
{
    public interface ILearningHubClient
    {
        Task ReceiveMessage(string message);
    }
}
