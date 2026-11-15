# Game Flow Integration

## Level objective

The player controls the existing top-down spark. Heat drains over time. The level author manually assigns the mandatory `FlammableHeatSource` objects; decorative flammable props are not counted unless explicitly assigned.

1. The first ignition of each assigned source increments objective progress.
2. Each source is counted only once. Its progress remains credited even after its temporary heat-restoration window ends.
3. The HUD displays `Зажжено: X/Y`.
4. The final bonfire is visible from the start but remains locked until every assigned source is ignited.
5. Touching the locked bonfire displays the number of mandatory sources still unlit.
6. Touching the unlocked bonfire wins the level.
7. Heat depletion loses the level. The result screen can restart the current scene.

This is a route/objective mechanic, not an inventory or fire-spread simulation.

## Runtime responsibilities

Namespace: `Game.GameFlow`.

- `Game.Gameplay.Heat.FlammableHeatSource` owns one-time ignition and temporary heat restoration. Its `Ignited` event fires once when it first ignites.
- `GameFlowBootstrap` subscribes to the Player heat events and the explicitly assigned mandatory-source events. It owns progress, bonfire availability, win/loss state and restart.
- `FinalBonfireTrigger` forwards Player contact to `GameFlowBootstrap.TryCompleteLevel()`.
- `GameFlowUI` displays heat, objective progress, locked-bonfire feedback and result/restart UI.
- Movement and level placement remain outside Game Flow.

## Unity setup

1. Open the gameplay scene.
2. Select **Tools > Game Flow > Setup**.
3. Assign the final bonfire GameObject. It must already have a suitable `Collider2D`; the tool enables **Is Trigger** without changing collider size or position.
4. Add every mandatory `FlammableHeatSource` to the **Обязательные горючие объекты** list. Do not add decorative props.
5. Click **Создать / проверить Game Flow** and save when prompted.
6. Ensure the Player has its existing `Rigidbody2D`, collider, and `PlayerHeatBootstrap`.
7. Enter Play Mode. Check that the HUD shows heat and `Зажжено: 0/Y`. Ignite all mandatory objects, then touch the final bonfire.

The tool does not place, resize or reposition level objects. The level author chooses the mandatory objects and their layout.

## Mandatory source visuals

Each `FlammableHeatSource` can use optional `Unlit Visual` and `Lit Visual` child objects. Assign both to switch the appearance on first ignition. If neither is assigned, the source's `SpriteRenderer` is tinted using the unlit and lit colors. The lit state remains for the attempt even after heat restoration expires.

## Final bonfire visuals

The bonfire can use optional `Locked Visual` and `Ready Visual` child objects on `FinalBonfireTrigger`. Assign both to switch between separate extinguished/ready visuals. If neither is assigned, the script tints the bonfire's `SpriteRenderer` using the locked and ready colors. The visual references are optional; collision and objective logic do not depend on them.

## Console diagnostics

- `[FIRE DEBUG]` reports the first ignition and temporary heat restoration.
- `[Game Flow]` reports mandatory-source progress and when the final bonfire unlocks.
- `[BONFIRE]` reports bonfire contact and the number of remaining mandatory sources when touched too early.

## Boundaries

Do not add inventory, automatic fire spread, wind/material simulation, multiple elements, or multi-level progression for this MVP. Gameplay owns heat and one-time source ignition; Game Flow owns the objective and run completion; Level Design manually assigns mandatory sources.
