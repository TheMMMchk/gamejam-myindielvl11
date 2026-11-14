# Art asset sources and integration

All selected source assets are stored under `Assets/Art/sources/`. Keep the original archives/files there as provenance and as reproducible import inputs.

## Approved assets

| Game use | Asset | License | Notes |
|---|---|---|---|
| Player flame | [Animated flame / Fire sprite sheet — highwizard](https://opengameart.org/content/animated-flame-fire-sprite-sheet) | CC0 | 25 individual animation frames plus sprite sheets; source archive is stored as `firespritesheet.zip`. |
| One-time flammable object | [Woodpile — Pixor](https://pixor.art/de/library/village/woodpile/) | CC0 | 8 directional sprites; source PNG is stored as `pixor-library-woodpile-selout-48px-8-sides.png`. |
| Final bonfire | [Idylwild's Chromatic Campfires](https://opengameart.org/content/idylwilds-chromatic-campfires) | CC0 | 10 animated torches/campfires in 5 colors, 4-frame animations, smoke and unlit/burnt variants; source archive is stored as `idylwilds_chromatic_campfires.zip`. |

## Import and animation workflow

1. Keep the three source files in `Assets/Art/sources/`.
2. In Unity, run **Tools → Art → Import Approved Art Assets**.
3. The tool extracts PNG files from the two ZIP archives into:
   - `Assets/Art/Imported/Player/`
   - `Assets/Art/Imported/Campfires/`
   - `Assets/Art/Imported/Environment/`
4. It applies pixel-art texture settings while preserving existing Multiple-sprite slicing metadata.
5. It creates/rebuilds `Assets/Art/Animations/PlayerFlame.anim` from the 25 individual `fireB####` frames at 12 FPS, looping.
6. The campfire sheet already has 109 sprite regions in its committed Unity importer metadata; the woodpile sheet already has 8 directional sprite regions. These are preserved rather than blindly re-sliced.

The importer is repeatable and non-destructive: it does not overwrite extracted PNG files, preserves Multiple slicing, and does not modify scenes, prefabs, gameplay scripts, or level layout. The flame animation clip can be rebuilt separately via **Tools → Art → Rebuild Player Flame Animation**.

## Scope boundaries

- Keep the existing Player GameObject, Rigidbody2D, CircleCollider2D and movement scripts unchanged.
- Do not change the URP 2D Global Light or add scene lights as part of this asset import.
- Scene assembly and level design remain outside the Art & VFX branch.
- The Editor tool creates reusable art assets only; it does not place objects into scenes.

## Current status

- [x] Original source files committed under `Assets/Art/sources/`.
- [x] Unity Editor extraction/import helper added.
- [x] Existing campfire and woodpile sprite slicing metadata preserved.
- [x] Editor helper extended to create a looping 25-frame player flame Animation Clip.
- [ ] Run the updated helper in Unity Editor and confirm there are no import/compiler errors.
- [ ] Validate visual scale and sorting in the target scene without editing the scene from this branch.

All three sources are identified as CC0 by their respective source pages. Keep this manifest with the project if assets are moved or replaced.
