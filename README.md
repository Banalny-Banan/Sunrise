# Sunrise

SCP:SL anti-cheat that actually works.

Created to prevent common exploits and add server authority in places where it lacks.

**Defeated cheats:** Silent aim, Spinbot, Wallhack (partially), Item pickup through walls, Tesla Gate bypass, Door Manipulator, and [many smaller exploits](#-common-exploit-patches).

---

[![Downloads](https://img.shields.io/github/downloads/Banalny-Banan/Sunrise/total?label=Downloads&color=333333&style=for-the-badge)](https://github.com/Banalny-Banan/Sunrise/releases/latest)
[![Discord](https://img.shields.io/discord/1277301350828478525?style=for-the-badge&logo=discord&logoColor=f9f9f9&label=Discord&color=5865f2)](https://discord.gg/9nAaRVNCq3)

---

# Features

## 🔫 Server-Side Backtrack

> Silent aim doesn't aim, spinbots fire in random directions, fake rotation prevents aiming.
> Cheaters are forced to use aim lock, which is much easier to notice.

The default latency compensation system (Backtrack) in SCP:SL
allows shooters to send their own and the target's position and rotation at the moment of firing.
The server does minimal position validation and no rotation validation, opening the door for exploits like silent aim and shooting through walls.

We replace this default system with a more secure server-side solution, effectively eliminating the possibility of such exploits.

The system works by recording a precise history of each player's position and rotation. When a player fires,
the server no longer trusts client-provided data. Instead, it locates the closest valid entry in the player’s actual historical data,
ensuring that only legitimate shots, based on real past positions and rotations, are processed.

## 🧱 Anti-Wallhack

> Significantly reduces wallhack effective range inside the facility.

In the base game, the server sends data about players from 36 meters away, regardless of walls or room boundaries.
Sunrise significantly limits this by introducing a visibility system based on room layouts.

The server now only shares player data if the observer's current room can "see" the target's room (e.g., through connected doors or open spaces).
This effectively prevents wallhack from spotting players across the map.

Edge cases like SCP abilities (e.g., SCP-049’s sense or SCP-939’s hearing) are respected to preserve vanilla gameplay mechanics.

### Raycast Anti-Wallhack

![Preview](https://github.com/Banalny-Banan/Sunrise/blob/master/Previews/RaycastWallhackPreview.gif)

Raycast Anti-Wallhack is an additional feature that complements the system by using raycasts to confirm visibility.
It yields almost perfect results, but can be computationally expensive.
If it's the case for you, it is always possible to disable it in the configuration file.
It is recommended to use `benchmark` option in the config to test the performance.

## 📦 Pickup Validation

> Prevents item pickups through walls.

By default, the server does not perform any line-of-sight checks when a player attempts to pick up an item.
This allows cheaters to pick up items through walls, doors, and other obstacles.

The pickup validation module checks if the item is genuinely reachable from the player’s point of view using raycasts.
Multiple raycasts may be performed to avoid false positives.
As a last resort, the system simulates slight vertical movement to handle edge cases where a player jumped before starting the pickup action.
If all checks fail, the pickup attempt is denied.

## ⚡ Server-Side Tesla Damage

> Prevents cheaters from bypassing Tesla Gate damage.

In the base game, Tesla Gates rely on clients to report themselves getting hit.
Cheaters can exploit this, making them fully immune to Teslas.

Sunrise adds a hybrid server-side system. The plugin simulates Tesla Gate bursts on the server for a decreased duration to account for latency,
tracking players in the shock area while the server-side burst is active. After a short delay, it verifies if players reported the damage as expected.
If a player did not report the damage, the server forces Tesla damage to be applied.

## 🚪 Door Interaction Validation

> Prevents cheaters from interacting with doors without looking at them.

By default, the server only performs a distance and a line-of-sight check when a player interacts with a door.
Sunrise adds a field-of-view (FOV) check to ensure that the player is looking at the door, preventing that cheat feature from working.

## 🩹 Common Exploit Patches

- SCP-939 lunge exploit: Prevents the exploit that allows SCP-939 to speed-hack using the lunge ability.
- SCP-3114 Item use exploit: Prevents the exploit that allows SCP-3114 to use items, including firearms.
- SCP-3114 Cooldown exploit: Prevents the exploit that allows cheaters to ignore the cooldowns of SCP-3114 disguise and strangle abilities.
- Handcuffed inventory exploit: Prevents the exploit that allows cheaters to use inventory while handcuffed.

## 🗺️ Planned Features

| Feature                    | Status         |
|----------------------------|----------------|
| Pickup spoofing (anti-ESP) | In development |

---

# 🛠️ Installation

The only dependency is [EXILED](https://github.com/ExMod-Team/EXILED).
To install the plugin, simply download the latest release from the [releases page](https://github.com/Banalny-Banan/Sunrise/releases)
and put it in the `Plugins` folder.

---

# 🤝 Community & Contributions

If you want to contribute or just like the project, feel free to join the [Discord server](https://discord.gg/9nAaRVNCq3).
See [CONTRIBUTING.md](./CONTRIBUTING.md) for more information.

---

# 📄 License

Sunrise is licensed under the MIT License.
You are free to use, modify, and include this project (or parts of it) in private or commercial projects.

See [LICENSE](./LICENSE) for full legal terms.
