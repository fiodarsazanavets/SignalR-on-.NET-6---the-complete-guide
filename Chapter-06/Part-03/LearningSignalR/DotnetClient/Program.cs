using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Channels;

Console.WriteLine("Please specify the URL of SignalR Hub");

var url = Console.ReadLine();

var hubConnection = new HubConnectionBuilder()
                         .WithUrl(url)
                         .Build();

hubConnection.On<string>("ReceiveMessage", 
    message => Console.WriteLine($"SignalR Hub Message: {message}"));

try
{
    await hubConnection.StartAsync();

    var running = true;

    while (running)
    {
        var message = string.Empty;
        var groupName = string.Empty;

        Console.WriteLine("Please specify the action:");
        Console.WriteLine("0 - broadcast to all");
        Console.WriteLine("1 - send to others");
        Console.WriteLine("2 - send to self");
        Console.WriteLine("3 - send to individual");
        Console.WriteLine("4 - send to a group");
        Console.WriteLine("5 - add user to a group");
        Console.WriteLine("6 - remove user from a group");
        Console.WriteLine("7 - trigger a server stream");
        Console.WriteLine("exit - Exit the program");

        var action = Console.ReadLine();

        if (action != "5" && action != "6" && action != "7")
        {
            Console.WriteLine("Please specify the message:");
            message = Console.ReadLine();
        }

        if (action == "4" || action == "5" || action == "6")
        {
            Console.WriteLine("Please specify the group name:");
            groupName = Console.ReadLine();
        }

        switch (action)
        {
            case "0":
                if (message?.Contains(';') ?? false)
                {
                    var channel = Channel.CreateBounded<string>(10);
                    await hubConnection.SendAsync("BroadcastStream", channel.Reader);

                    foreach (var item in message.Split(';'))
                    {
                        await channel.Writer.WriteAsync(item);
                    }

                    channel.Writer.Complete();
                }
                else
                {
                    hubConnection.SendAsync("BroadcastMessage", message).Wait();
                }
                break;
            case "1":
                await hubConnection.SendAsync("SendToOthers", message);
                break;
            case "2":
                await hubConnection.SendAsync("SendToCaller", message);
                break;
            case "3":
                Console.WriteLine("Please specify the connection id:");
                var connectionId = Console.ReadLine();
                await hubConnection.SendAsync("SendToIndividual", connectionId, message);
                break;
            case "4":
                hubConnection.SendAsync("SendToGroup", groupName, message).Wait();
                break;
            case "5":
                hubConnection.SendAsync("AddUserToGroup", groupName).Wait();
                break;
            case "6":
                hubConnection.SendAsync("RemoveUserFromGroup", groupName).Wait();
                break;
            case "7":
                Console.WriteLine("Please specify the number of jobs to execute.");
                var numberOfJobs = int.Parse(Console.ReadLine() ?? "0");
                var cancellationTokenSource = new CancellationTokenSource();
                var stream = hubConnection.StreamAsync<string>(
                    "TriggerStream", numberOfJobs, cancellationTokenSource.Token);

                await foreach (var reply in stream)
                {
                    Console.WriteLine(reply);
                }
                break;
            case "exit":
                running = false;
                break;
            default:
                Console.WriteLine("Invalid action specified");
                break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}
