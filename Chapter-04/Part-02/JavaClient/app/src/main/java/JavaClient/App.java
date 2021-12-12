import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;

import java.util.Scanner;

public class App {
    public static void main(String[] args) throws Exception {
        System.out.println("Please specify the URL of SignalR Hub");
        Scanner reader = new Scanner(System.in);
        String input = reader.nextLine();

        HubConnection hubConnection = HubConnectionBuilder.create(input)
                .build();

        hubConnection.on("ReceiveMessage", (message) -> {
            System.out.println(message);
        }, String.class);
		
        hubConnection.start().blockingAwait();

        while (!input.equals("exit")){
            input = reader.nextLine();
            hubConnection.send("BroadcastMessage", input);
        }

        hubConnection.stop();
    }
}
