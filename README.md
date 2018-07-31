# Ascension [NETWORKING] [HIGH]
========

## Homepage

http://rottenvisions.com

## What is Ascension?

![Assets](https://raw.githubusercontent.com/RottenVisions/ascension/master/readme-src/assets.png)

![Editor](https://raw.githubusercontent.com/RottenVisions/ascension/master/readme-src/editor.png)

![Remotes](https://raw.githubusercontent.com/RottenVisions/ascension/master/readme-src/remotes.png)

![Scenes](https://raw.githubusercontent.com/RottenVisions/ascension/master/readme-src/scenes.png)

![Settings](https://raw.githubusercontent.com/RottenVisions/ascension/master/readme-src/settings.png)

Ascension is the high level networking layer built for DESYSIA. It allows for very easy synchronization of entities as well
as support for many high class techniques seen in FPS games.

## Features

**Powerful Event System** - Ascension has a built-in event system which automatically distributes the events to the correct receivers. It is highly configurable of course and events can also be targeted at a specific game object or globally. Three delivery modes are available: Unreliable, Unreliable Synced and Reliable.

**Authoritative Movement** - Built-in support for input and player controller state synchronization allows easy implementation of custom authoritative movement.

**Both Dedicated & Listen Server** - Ascension supports the classic dedicated server and also listen servers where the server is just another player.

**Prioritization & Scoping** - Ascension supports both scoping of entities (which entities should a player be aware of) and prioritization of scoped entities (of two entities A and B that are scoped, which one is the most important for a player).

**Hit-Box Recording** - Ascension has built-in support for recording hit-boxes and rewinding and ray-casting against them, allowing implementation of complex techniques like lag compensation in FPS titles.

**Synchronized Map Loading** - Ascension supports loading maps/scenes from the server, have all the clients load the same map/scene and then provide custom hooks telling the server when the client is ready and vice-versa.

**Supports All Major Unity Platforms** - Ascension runs on Windows, OSX, iOS, Android, GNU/Linux, and Xbox One. PS4 and Nintendo Switch in the future.

Ascension was developed to support any high paced networking projects Rotten Visions makes, including project DESYSIA.
