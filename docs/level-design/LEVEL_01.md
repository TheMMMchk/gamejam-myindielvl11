# Level 01 — Level Design

## Concept
One compact top-down level for **Искра**. The player starts in the south-west and must reach the final bonfire in the north-east.

No inventory and no collectible fuel. Dry woodpiles are fixed in the world and use the Gameplay `FlammableHeatSource` component. The player gets heat by entering and staying in the source trigger. No pickup objects are placed.

## Route layout
- **Start clearing:** south-west, with the first woodpile nearby so the player can learn the heat-source rule safely.
- **Central rock formation:** blocks the direct diagonal route and creates two readable ways around it.
- **Left route:** longer route with two fixed woodpiles; it offers more opportunities to recover heat.
- **Right route:** more direct route with one woodpile; it trades fewer turns for less opportunity to recover heat.
- **Final clearing:** north-east. The large, bright bonfire is the visual destination.

## Runtime integration
The scene generator adds `PlayerHeatBootstrap` to Player and `FlammableHeatSource` to the three woodpiles when those Gameplay types are available. It does not add `HeatPickup` anywhere.

Gameplay defaults from the current integration contract are left unchanged:
- heat drain: 5 per second;
- source burn duration: 4 seconds;
- source restoration: 12 per second while Player remains in the trigger.

## Generate the scene
1. Merge the Gameplay branch so the heat components exist in the project.
2. Open the Unity project.
3. Run **Tools → Level Design → Generate Level 01**.
4. Open `Assets/Scenes/Level01.unity`.
5. Verify the Player's Move action reference and test the source triggers in Play Mode.

The generator copies the existing SampleScene, preserves its camera/light/player setup, adds level geometry, and saves a separate Level01 scene. It does not overwrite SampleScene.

## Scope boundaries
This workstream owns the level layout and scene composition. It does not implement heat rules, fire-trail behavior, goal completion, game-over, UI, or restart. The final bonfire is currently a goal visual; Game Flow must connect its win condition separately.
