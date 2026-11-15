# Gameplay Integration Contract

This document describes the heat mechanic implemented in `feature/gameplay` and the public integration points for the other workstreams.

## Runtime components

All scripts are in namespace `Game.Gameplay.Heat`:

- `PlayerHeatBootstrap` — attach to the Player GameObject. Creates and updates the heat model.
- `HeatModel` — plain C# gameplay logic. Owns the current heat value and depletion rules.
- `HeatPickup` — restores heat instantly on Player contact, then destroys its GameObject.
- `FlammableHeatSource` — ignites once on Player contact and provides heat for a limited time. Its `Ignited` event is raised exactly once on first ignition so Game Flow can count mandatory level objectives.

The existing movement scripts and Player Rigidbody2D/collider are not modified.

## Default tuning

| Setting | Default |
| --- | ---: |
| Maximum heat | 100 |
| Initial heat | 100 |
| Passive drain | 5 heat/second |
| Fading threshold | 25% |
| Pickup restore | 35 heat instantly |
| Flammable source duration | 4 seconds |
| Flammable source restore | 12 heat/second while touching |

Values are Inspector-serialized on the corresponding MonoBehaviour and can be tuned without changing code.

## Game Flow contract

Get a reference to `PlayerHeatBootstrap` from the Player and subscribe to:

```csharp
playerHeat.HeatDepleted += HandlePlayerHeatDepleted;
```

The event is raised once when heat reaches zero. Game Flow owns the resulting game-over / failure transition; Gameplay does not load scenes, show result screens, or restart the run.

Unsubscribe when the listener is destroyed or disabled:

```csharp
playerHeat.HeatDepleted -= HandlePlayerHeatDepleted;
```

## UI contract

The Player heat component exposes:

- `CurrentHeat` — current absolute value.
- `MaxHeat` — maximum value.
- `NormalizedHeat` — normalized value from 0 to 1.
- `IsFading` — true at or below the configured fading threshold.
- `IsDepleted` — true when heat has reached zero.
- `HeatChanged(float current, float max)` — raised whenever the heat value changes.

UI should subscribe to `HeatChanged` and update its presentation. The Gameplay branch does not create or own UI.

## Level Design contract

### Player

- Keep the existing Player Rigidbody2D and Collider2D.
- Add `PlayerHeatBootstrap` to the same Player GameObject as its Collider2D.
- Do not change the movement component to support heat.

### Fuel pickup

- Add `HeatPickup` and a `Collider2D` to the pickup GameObject.
- Enable `Is Trigger` on the collider.
- Set `Heat Amount` in the Inspector if the default 35 is not suitable.
- The pickup is consumed on first Player contact, even if the Player is already at maximum heat.

### Flammable object

- Add `FlammableHeatSource` and a `Collider2D` to the object.
- Enable `Is Trigger` on the collider. Its shape defines the contact area in which the Player receives heat.
- On first Player contact, it ignites once. It then provides heat only while the Player remains inside the trigger and only for its configured burn duration.
- Leaving and re-entering does not restart or extend its burn time. After the duration ends, the object is spent and cannot ignite again.
- Tune `Burn Duration` and `Heat Per Second` in the Inspector.

## Scope boundaries

Gameplay owns heat values, passive drain, pickup restoration, one-time ignition, and limited heat-source behavior.

Gameplay does not own movement, level layout, fire-trail visuals, UI, win/lose transitions, scene loading, or restart logic. No inventory or complex fire simulation is introduced.
