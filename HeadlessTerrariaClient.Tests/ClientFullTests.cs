using System.Diagnostics;
using HeadlessTerrariaClient.Game;
using HeadlessTerrariaClient.Network;

namespace HeadlessTerrariaClient.Tests;

public class ClientFullTests
{
    [Test]
    public async Task TestConnection()
    {
        using HeadlessClient client = new HeadlessClient("127.0.0.1", 7777);

        await client.ConnectAsync();

        Assert.That(client.Connected, Is.True);

        await client.DisconnectAsync();

        Assert.That(client.Connected, Is.Not.True);
    }

    [Test]
    public async Task TestJoinWorld()
    {
        using HeadlessClient client = new HeadlessClient("127.0.0.1", 7777);

        await client.ConnectAsync();

        Assert.That(client.Connected, Is.True);

        Assert.That(client.ConnectionState, Is.EqualTo(ConnectionState.None));

        await client.JoinWorldAsync();

        Assert.That(client.Connected, Is.True);

        Assert.That(client.ConnectionState, Is.EqualTo(ConnectionState.FinishedConnecting));

        await Task.Delay(1000);

        await client.DisconnectAsync();

        Assert.That(client.Connected, Is.Not.True);

        Assert.That(client.ConnectionState, Is.EqualTo(ConnectionState.None));
    }

    [Test]
    public async Task TestChat()
    {
        using HeadlessClient client = new HeadlessClient("127.0.0.1", 7777);

        await client.ConnectAsync();

        await client.JoinWorldAsync();

        await Task.Delay(1000);

        bool receivedOurMessage = false;

        client.ChatMessageReceived += (author, message) =>
        {
            if (!receivedOurMessage)
            {
                Assert.That(message.ToString(), Is.EquivalentTo("<unnamed player>: <test message>"));
                receivedOurMessage = true;
            }
        };

        await client.SendChatMessageAsync("<test message>");

        Stopwatch timeoutWatch = Stopwatch.StartNew();
        while (!receivedOurMessage)
        {
            await Task.Delay(1);

            if (timeoutWatch.Elapsed.TotalSeconds > 10f)
            {
                Assert.Fail("Timed out");
            }
        }

        await client.DisconnectAsync();
    }

    [Test]
    public async Task TestTileManipulation()
    {
        using HeadlessClient client = new HeadlessClient("127.0.0.1", 7777);

        await client.ConnectAsync();

        await client.JoinWorldAsync();

        await Task.Delay(1000);

        bool receivedOurMessage = false;

        client.TileManipulationReceived += (x) =>
        {
            if (!receivedOurMessage)
            {
                if (x.Type is TileManipulationType.KillTile or TileManipulationType.KillTileNoItem or TileManipulationType.TryKillTile)
                {
                    receivedOurMessage = true;
                }
            }
        };

        await client.SendChatMessageAsync("<test TestTileManipulation> Waiting for player in world to break tile.");

        Stopwatch timeoutWatch = Stopwatch.StartNew();
        while (!receivedOurMessage)
        {
            await Task.Delay(1);

            if (timeoutWatch.Elapsed.TotalSeconds > 10f)
            {
                Assert.Fail("Timed out");
            }
        }

        await client.DisconnectAsync();
    }
}
