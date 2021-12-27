namespace SignalRHubs
{
    public interface ILearningHubClient
    {
        Task ReceiveMessage(string message);
    }
}
