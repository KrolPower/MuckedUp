# Project Overview
- Game Title: MuckedUp (Muck Multi-Player Survival)
- High-Level Concept: A multiplayer survival roguelike where players gather in an interactive 3D hub (MarketSquare) before venturing into procedurally generated wilderness runs, and return to the hub upon victory or defeat.
- Players: 1-8 players (Online Co-op & Versus via Steam P2P / Classic TCP/UDP)
- Inspiration / Reference Games: Muck, Risk of Rain 2, Valheim
- Tone / Art Direction: Stylized low-poly fantasy adventure
- Target Platform: Standalone Windows PC (Steam)
- Screen Orientation / Resolution: Landscape 1920x1080 (Configurable PC resolutions)
- Render Pipeline: Built-in Render Pipeline

# Game Mechanics
## Core Gameplay Loop
1. **Lobby Creation & Staging**: Host creates a lobby in the Main Menu. When ready, the host clicks Play to transition all connected players into the 3D **MarketSquare** hub scene.
2. **Interactive Hub Exploration & Late Joining**: Players spawn at `MarketSquareWorldSpawn`. In MarketSquare, players can move around the village/tavern, socialize, and explore. The lobby remains open and joinable so additional players can join, spawn into MarketSquare, or leave.
3. **World Start Readying & 5-Second Countdown**: All connected players gather and stand on the `WorldStartPlatform`. Once 100% of connected players are on the platform, a synchronized 5-second countdown timer displays at the top center of the screen. If any player leaves the platform during the countdown, the timer cancels and resets.
4. **Game Run Initiation**: When the countdown completes, the lobby becomes non-joinable (locked for the run) and all players transition to `GameAfterLobby` where procedural world generation, resource spawning, mobs, and day-night survival loops execute.
5. **Run Completion & Loop Back**: When the run ends (all players die, players escape via Boat, or defeat the Dragon Boss), the Game Over / Victory sequence transitions the party back into the **MarketSquare** hub instead of kicking players back to the main menu. The lobby is re-opened for new players to join before starting the next run.

## Controls and Input Methods
- **Movement**: WASD keys
- **Jump & Sprint**: Space to jump, Shift to sprint
- **Mouse Look**: Mouse movement for camera pitch and yaw
- **Interact**: 'E' key (Interacting with objects/doors/chests)
- **Chat**: Enter to open chat box, type, and press Enter to send

# UI
- **MarketSquare Countdown HUD**:
  - Located at the Top Center of the screen (`Canvas` with anchored `TextMeshProUGUI` element).
  - Styling: Large stylized font, displaying "Game starting in 5...", "4...", "3...", "2...", "1..." with smooth pulse/scale animation.
  - Sub-text indicator displaying player ready status (e.g. "Players ready: 2/3 on platform").
  - Fades out / disappears immediately if a player exits the platform area.
- **Game Over / Victory Return Option**:
  - On the Game Over / Victory screen, provide a primary "Return to Hub" / "Play Again" button (alongside "Leave Game") that returns the group back to `MarketSquare`.
- **MarketSquare HUD Elements**:
  - Chat box (`ChatBox`) and Player Nameplates active for social interaction in the hub.

# Key Asset & Context
- **Scenes**:
  - `Assets/Scene/Scenes/MarketSquare.unity`: Hub scene containing `MarketSquareWorldSpawn`, `WorldStartPlatform`, `MarketGround`, and village structures. Added to `EditorBuildSettings`.
  - `Assets/Scene/Scenes/GameAfterLobby.unity`: Procedural survival gameplay scene.
  - `Assets/Scene/Scenes/Menu.unity`: Main menu and lobby configuration scene.
- **Key Scripts to Create / Modify**:
  - `Assets/Scripts/Assembly-CSharp/MarketSquareManager.cs` (New): Manages hub state, late joining spawn logic at `MarketSquareWorldSpawn`, detects player presence on `WorldStartPlatform`, coordinates the 5-second countdown, and requests the server to start the match.
  - `Assets/Scripts/Assembly-CSharp/MarketSquarePlatformTrigger.cs` (New): Trigger component on `WorldStartPlatform` tracking player colliders entering and leaving the platform bounds.
  - `Assets/Scripts/Assembly-CSharp/MarketSquareUI.cs` (New): Handles countdown UI presentation, ready count text, and visual feedback at top-center screen.
  - `Assets/Scripts/Assembly-CSharp/NetworkController.cs`: Updated to support loading `MarketSquare` first upon host starting the lobby, and routing between Hub and Game scenes.
  - `Assets/Scripts/Assembly-CSharp/GameManager.cs`: Updated `StartLoadingScene` / `GenerateWorldRoutine` to differentiate between MarketSquare hub mode (no procedural world gen, instant spawn at spawn point) and GameAfterLobby mode (procedural world gen). Updated GameOver / Victory / Return flows to transition back to MarketSquare without closing the Steam lobby.
  - `Assets/Scripts/Assembly-CSharp/SteamLobby.cs` & `SteamManager.cs`: Keep lobby joinable during MarketSquare hub state, allow dynamic player spawning upon connection in MarketSquare, lock lobby when starting GameAfterLobby, and re-enable joinable state when returning to MarketSquare.
  - `Assets/Scripts/Assembly-CSharp/ServerSend.cs` / `ServerHandle.cs` / `ClientSend.cs` / `ClientHandle.cs`: Networking packets for countdown sync (`StartCountdown`, `CancelCountdown`) and hub player spawning.

# Implementation Steps
### Step 1: Add MarketSquare to Build Settings & Scene Inspection
- **Description**: Ensure `Assets/Scene/Scenes/MarketSquare.unity` is registered in `EditorBuildSettings.scenes` so it loads smoothly in editor play mode and standalone builds.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Implement Platform Trigger & MarketSquare Manager
- **Description**: 
  1. Create `MarketSquarePlatformTrigger.cs` attached to `WorldStartPlatform` with a BoxCollider trigger to detect when `PlayerManager` / `PlayerMovement` enter or exit the platform.
  2. Create `MarketSquareManager.cs` to query `MarketSquareWorldSpawn`, manage hub player spawning, check if all connected players are on the platform, and run the 5-second server-authoritative countdown.
  3. Create `MarketSquareUI.cs` with a top-center TMP overlay to show the 5s countdown timer and player ready counter.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Network Packets & Synchronization for MarketSquare Countdown and Late Joining
- **Description**:
  1. Add network packets (`countdownUpdate`, `startMatchFromHub`, etc.) in `ClientPackets.cs` and `ServerPackets.cs` (or reuse packet protocol) to synchronize countdown state across all connected clients.
  2. In `SteamLobby.cs` and `ServerHandle.cs`, keep lobby joinable while in MarketSquare; when a player connects while in MarketSquare, spawn their player character at `MarketSquareWorldSpawn`.
  3. When a player disconnects in MarketSquare, despawn them cleanly and update the required platform ready count.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: Update Game Start Flow (Menu -> MarketSquare -> GameAfterLobby)
- **Description**:
  1. Update `SteamLobby.StartGame()` and `NetworkController.cs` so host clicking Play in the Main Menu lobby loads `MarketSquare` first.
  2. In `GameManager.cs`, distinguish between MarketSquare hub (static scene, instant spawn, no world generator routine) and `GameAfterLobby` (procedural generation, navmesh baking, mob spawner setup).
  3. When the countdown completes in `MarketSquare`, invoke `NetworkController.Instance.LoadGame()` to transition all players to `GameAfterLobby` and set `Lobby.SetJoinable(false)`.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: No

### Step 5: Update Game Over / Victory Loop Back to MarketSquare
- **Description**:
  1. In `GameManager.cs`, `GameoverUI.cs`, `Boat.cs`, and `Dragon.cs`, update the end-of-run flow so winning (escaping on boat / slaying dragon) or wiping (all players dead) gives the option or automatically returns all connected clients to the `MarketSquare` scene instead of disconnecting to Menu.
  2. On returning to `MarketSquare`, reset player states/inventories, re-enable `Lobby.SetJoinable(true)`, and allow new players to join before stepping on the platform again.
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

### Step 6: MarketSquare Scene Hierarchy & Prefabs Setup
- **Description**: Ensure `MarketSquare.unity` has the required UI Canvas (`UI (1)` prefab/components with Chat, Hotbar, Countdown UI), `MarketSquarePlatformTrigger` on `WorldStartPlatform`, and `MarketSquareWorldSpawn` transform references connected.
- **Assigned role**: developer
- **Dependencies**: Step 2, Step 4
- **Parallelizable**: Yes

# Verification & Testing
- **Singleplayer / Host Verification**:
  1. Start game, create a lobby in Menu, click Start. Confirm `MarketSquare` scene loads.
  2. Confirm player spawns at `MarketSquareWorldSpawn` with full movement, camera, and jumping functional.
  3. Walk onto `WorldStartPlatform`. Confirm the 5-second countdown appears at top center and counts down 5 -> 4 -> 3 -> 2 -> 1.
  4. Step off platform at 3 seconds: confirm countdown immediately cancels and disappears.
  5. Step back onto platform and let countdown reach 0: confirm smooth transition to `GameAfterLobby`, procedural map generation runs, and player spawns into survival world.
- **Multiplayer / Lobby Sync Verification**:
  1. Host enters `MarketSquare`. Second client joins via Steam/lobby ID.
  2. Confirm second client spawns at `MarketSquareWorldSpawn` in `MarketSquare`.
  3. One player stands on `WorldStartPlatform` (1/2 ready): countdown does NOT start, ready counter shows 1/2.
  4. Both players stand on `WorldStartPlatform` (2/2 ready): 5-second countdown starts on both screens simultaneously.
  5. One player jumps off at 2s: countdown aborts for both players.
  6. Both players stand on platform until 0: both clients load into `GameAfterLobby`.
- **Game Over & Victory Loop Back Verification**:
  1. In `GameAfterLobby`, trigger GameOver (kill players or defeat boss / leave on boat).
  2. Confirm game transitions party back to `MarketSquare` with lobby connection intact.
  3. Confirm third player can join the lobby while in `MarketSquare` after the run.
