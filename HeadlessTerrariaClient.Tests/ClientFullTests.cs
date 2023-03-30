﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        await Task.Delay(1000);

        Assert.That(client.ConnectionState, Is.EqualTo(ConnectionState.RequestingWorldData));

        await client.DisconnectAsync();

        Assert.That(client.Connected, Is.Not.True);

        Assert.That(client.ConnectionState, Is.EqualTo(ConnectionState.None));
    }
}