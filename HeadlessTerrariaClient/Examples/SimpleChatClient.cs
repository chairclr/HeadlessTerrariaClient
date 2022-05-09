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
    public static class Program
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
            HeadlessClient.clientUUID = Guid.NewGuid().ToString();

            // Assaign world reference
            HeadlessClient.World = new ClientWorld();

            // Name the player
            HeadlessClient.PlayerFile.name = $"ExampleChatClient";

            // Softcore player, Default appearence, and Default inventory
            HeadlessClient.PlayerFile.LoadDefaultPlayer();

            // This can bypass some anti-cheats that attempt to block headless clients
            HeadlessClient.Settings.AutoSyncPlayerZone = true;

            // Run code when a chat message is recived
            HeadlessClient.ChatMessageRecieved += (HeadlessClient client, ChatMessage message) =>
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

            await Task.Delay(Timeout.Infinite);
        }
    }
}
