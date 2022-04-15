using System;
using System.Threading.Tasks;
using HeadlessTerrariaClient.Client;
using HeadlessTerrariaClient.Util;
using HeadlessTerrariaClient.Terraria;
using HeadlessTerrariaClient.Terraria.Chat;
using System.Threading;

namespace HeadlessTerrariaClient.Examples
{
    public class Program
    {
        static void Main(string[] args)
        {
            SimpleChatClient chatClient = new SimpleChatClient();
            chatClient.Start().Wait();
        }
    }
    public class SimpleChatClient
    {
        const string ServerIP = "127.0.0.1";
        const int ServerPort = 7777;

        public async Task Start()
        {
            // Create a new client
            HeadlessClient HeadlessClient = new HeadlessClient();

            // Random client UUID
            HeadlessClient.SetRandomUUID();

            // Assaign world reference
            HeadlessClient.World = new ClientWorld();

            // Name the player
            HeadlessClient.PlayerFile.name = $"ExampleChatClient";

            // Softcore player, Defualt inventory, Default appearence 
            HeadlessClient.PlayerFile.LoadDefaultPlayer();

            // This can bypass some anti-cheats that attempt to block headless clients
            HeadlessClient.Settings.AutoSyncPlayerZone = true;

            // Run code when a chat message is recived
            HeadlessClient.ChatMessageRecievedAsync += async (HeadlessClient client, ChatMessage message) =>
            {
                // Messages of id 255 are not from another player
                if (message.author != 255)
                {
                    Player sender = client.World.player[message.author];
                    Console.WriteLine($"<{sender.name}> {message.message}");
                }
                else
                {
                    Console.WriteLine(message.message);
                }
            };

            // Connect to a server
            await HeadlessClient.Connect(ServerIP, ServerPort);

            // Stop the console from closing
            await Task.Delay(Timeout.Infinite);
        }
    }
}
