using System;
using Terraria;
using HeadlessTerrariaClient;

namespace HeadlessTerrariaClient.Examples
{
    public class SimpleChatClient
    {
        const string ServerIP = "terraformserver.com";
        const int ServerPort = 7777;
        static void Main(string[] args)
        {
            Start();

            System.Threading.Thread.Sleep(-1);
        }

        public static void Start()
        {
            HeadlessClient HeadlessClient = new HeadlessClient();
            HeadlessClient.clientUUID = Guid.NewGuid().ToString();
            HeadlessClient.PlayerFile.name = $"ExampleChatClient";  
            HeadlessClient.PlayerFile.difficulty = PlayerDifficultyID.SoftCore;

            // This can bypass some anti-cheats that attempt to block headless clients
            HeadlessClient.Settings.AutoSyncPlayerZoneAndControl = true;

            HeadlessClient.ChatMessageRecieved += (HeadlessClient client, ChatMessage message) =>
            {
                // Messages of id 255 are not from another player
                if (message.author != 255)
                {
                    Player sender = client.player[message.author];
                    Console.WriteLine($"<{sender.name}> {message.message}");
                }
                else
                {
                    Console.WriteLine(message.message);
                }
            };
            HeadlessClient.Connect(ServerIP, ServerPort);
        }

    }
}
