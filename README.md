# Doofus : The Runaway Ball 🏏

Welcome to my submission for the Hitwicket Game Developer Challenge! I have fully completed the assignment up to Level 3, implementing a modular architecture, event-driven scoring, and a complete UI state machine. 

To align with Hitwicket's mission of delighting a billion cricket fans, I spent the final hour adding thematic "juice" to the game, transforming Doofus into a cricket ball navigating a disappearing pitch!

## 🎥 Gameplay Showcase

Watch Gameplay Video here : https://drive.google.com/file/d/1sqKtB41CReisdxA0H4_DVL0Wdn4E-GuK/view?usp=sharing

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/2e5d2363-3ac2-458b-b371-4b72dcbe1b6e" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/86916c97-61c1-4eeb-ad24-f3a72c86a09f" />

<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/ea451ee4-cea0-446b-bfd8-52284194a5bd" />


---

## ✨ Assignment Progression & Features

### Level 1: Core Mechanics & JSON Configuration
*   **Dynamic Data:** Game parameters (player speed, spawn times, destroy times) are dynamically deserialized at runtime from `doofus_diary.json` using a `GameConfig` Singleton.
*   **Crisp Movement:** Built using Unity 6, player movement utilizes `Rigidbody.linearVelocity` (with backward compatibility fallbacks) to ensure snappy, non-floaty controls.
*   **Pulpit Lifecycle:** Platforms are managed via Coroutines, triggering precise spawns and visual countdown warnings (color shifting and scaling) during their final second.

### Level 2: Scoring System
*   **Event-Driven:** The scoring system relies on C# `Action` delegates to keep scripts decoupled.
*   **Instance ID Tracking:** The `ScoreManager` tracks the `GetInstanceID()` of visited platforms to guarantee players are only rewarded for exploring new platforms, effectively preventing duplicate scoring.

### Level 3: UI Flow & State Management
*   **Seamless Restarts:** Game Over and Restart logic is handled seamlessly without expensive scene reloads. Physics states, velocities, and UI CanvasGroups are reset instantly.
*   **Persistent High Scores:** The game tracks and saves the player's Best Score utilizing `PlayerPrefs`.

### Bonus / "Juice" (The Hitwicket Theme)
*   **Visual Overhaul:** Doofus is rendered as a red cricket ball equipped with a dynamic `TrailRenderer` for enhanced game feel.
*   **Platform Animations:** The 9x9 platforms feature a grass-green "pitch" material. They dynamically ease-in from below when spawned and simulate gravity by falling into the abyss when destroyed.

---

## ⚙️ Technical Architecture
*   **Engine:** Unity 6000.3 
*   **Patterns Used:** Singleton (Managers), Observer/Event-Driven (Actions)
*   **Hierarchy:** Strict separation between `UIManager`, `ScoreManager`, and core gameplay scripts to ensure scalability for potential live-ops or multiplayer integration.

## 🕹️ Controls
*   **W, A, S, D** or **Arrow Keys** to move.
*   Keep the ball on the pitch and don't look down!
