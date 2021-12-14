using Microsoft.AspNetCore.SignalR.Client;

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

        Console.WriteLine("Please specify the action:");
        Console.WriteLine("0 - broadcast to all");
        Console.WriteLine("1 - send to others");
        Console.WriteLine("2 - send to self");
        Console.WriteLine("3 - send to individual");
        Console.WriteLine("exit - Exit the program");

        var action = Console.ReadLine();

        Console.WriteLine("Please specify the message:");
        message = Console.ReadLine();

        switch (action)
        {
            case "0":
                await hubConnection.SendAsync("BroadcastMessage", message);
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
