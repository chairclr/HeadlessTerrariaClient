# Headless Terraria Client

Is a 3rd party Terraria client meant to be used programmatically, similar to a discord bot.

## Main Features
Connecting to TShock and Vanilla servers

## Fully Supported Packets
[3] PlayerInfo \
[4] SyncPlayer \
[14] PlayerActive \
[16] PlayerLife \
[42] PlayerMana \
[49] CompleteConnectionAndSpawn \
[68] ClientUUID \
[129] FinishedConnectingToServer 

## Partially Supported Packets
[7] WorldData \
[9] StatusText \
[13] PlayerControls \
[39] ReleaseItemOwnership \
[82] NetModules 

## Planned Features and Packets
Making world file be synced accross multiple clients so that each instance of HeadlessClient doesnt have its own world \
Support for [17] TileManipulation

###### Still no head :(