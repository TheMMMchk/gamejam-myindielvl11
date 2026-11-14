# Game Flow Integration

## Runtime contract

Namespace: `Game.GameFlow`.

- `GameFlowBootstrap` owns the run state: `Playing`, `Won`, or `Lost`.
- It subscribes to the existing `PlayerHeatBootstrap.HeatChanged` and `HeatDepleted` events. Gameplay is not modified.
- Heat depletion transitions to `Lost`; `FinalBonfireTrigger` transitions to `Won`.
- Only the first transition from `Playing` is accepted. Finishing pauses gameplay with `Time.timeScale = 0`; restart restores time scale and reloads the active scene.
- `GameFlowUI` presents the heat value, result message, and restart button.

## Unity setup

1. Open the gameplay scene.
2. Select **Tools > Game Flow > Setup**.
3. Assign the final bonfire GameObject. It must already have a suitable `Collider2D`. The tool does not resize, reposition, or replace the level collider.
4. Click **Create / Check Game Flow** and save when prompted.
5. Ensure the final bonfire collider is configured as a trigger and that the Player has its existing `Rigidbody2D`, collider, and `PlayerHeatBootstrap`.
6. Enter Play Mode. Heat is shown at the top; touching the final bonfire wins; heat reaching zero loses; the result overlay offers restart.

The setup command is idempotent for its named Game Flow objects: running it again reuses the existing objects and components rather than duplicating them. Existing level objects are not moved or replaced.

## Boundaries

Game Flow does not change heat rules, player movement, art, VFX, or level layout. It uses only the public Gameplay heat API. The final goal is explicitly assigned in the Editor tool rather than discovered through global scene searches at runtime.
