
<h1 align="center">
Headless Client
</h1>
<p align="center">
Headless Client is a 3rd party client for Terraria, meant to be similar to a Discord bot.
</p>
<br>

<h2>
Examples
</h2>

---


```cs
HeadlessClient HeadlessClient = new HeadlessClient();

// Random client UUID
HeadlessClient.clientUUID = Guid.NewGuid().ToString();

// World
HeadlessClient.World = new ClientWorld();

// Player name
HeadlessClient.LocalPlayer.name = "Example Client";

// Softcore player, Default appearence, and Default inventory
HeadlessClient.LocalPlayer.LoadDefaultPlayer();

// Connect to the server
await HeadlessClient.Connect("127.0.0.1", 7777);
```

<h2>
Main features
</h2>

---

- Connecting to TShock and Vanilla Terraria servers
- Sending and receiving chat messages
- Sending and receiving tile manipulation, such as placing and breaking blocks
- Events
  - World Data Recieved
  - Finished Connecting to Server
  - Client Connection Completed
  - Chat Message Recieved
  - Tile Manipulation Message Recieved
  - Net Message Received
  - Net Message Sent

<h2>
Planned features
</h2>

---

- NPC data
- Projectile data
- Writing to signs
- Using chests
- Mass wiring operations
- Improving async/await experience

<h2>
Contributing
</h2>

---

To contribute, open a pull request and I will review it and accept the PR if it suitable.


###### Still no head :(
