using System;
using HeadlessTerrariaClient;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.Chat;
using HeadlessTerrariaClient.Client;
using HeadlessTerrariaClient.Utility;
using System.Threading;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Terraria.ID;

namespace HeadlessTerrariaClient.Examples
{
    /// <summary>
    /// Very simple example client that connects to a server and prints chat messages to the console
    /// </summary>
    public class SimpleChatClient
    {
        const string ServerIP = "127.0.0.1";
        const int ServerPort = 7777;
        
        public async Task Start()
        {
            // Create a new client
            HeadlessClient HeadlessClient = new HeadlessClient();

            // Random client UUID
            HeadlessClient.clientUUID = Guid.NewGuid().ToString();

            // Assaign world reference
            HeadlessClient.World = new ClientWorld();

            // Name the player
            HeadlessClient.LocalPlayer.name = "ExampleChatClient";

            // Softcore player, Default appearence, and Default inventory
            HeadlessClient.LocalPlayer.LoadDefaultPlayer();

            // Run code when a chat message is recived
            HeadlessClient.ChatMessageRecieved += (HeadlessClient client, ChatMessage message) =>
            {
                // Messages of id 255 are not from another player
                if (message.AuthorIndex != 255)
                {
                    Player sender = client.World.Players[message.AuthorIndex];
                    Console.Write($"<{sender.name}>");
                    message.WriteToConsole();
                    Console.Write("\n");
                }
                else
                {
                    message.WriteToConsole();
                    Console.Write("\n");
                }
            };

            // Connect to a server
            await HeadlessClient.Connect(ServerIP, ServerPort);

            await Task.Delay(Timeout.Infinite);
        }
    }
}
