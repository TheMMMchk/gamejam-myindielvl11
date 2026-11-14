# Art asset sources and integration

All selected source assets are stored under `Assets/Art/sources/`. Keep the original archives/files there as provenance and as reproducible import inputs.

## Approved assets

| Game use | Asset | License | Notes |
|---|---|---|---|
| Player flame | [Animated flame / Fire sprite sheet — highwizard](https://opengameart.org/content/animated-flame-fire-sprite-sheet) | CC0 | 25 frames; source archive is stored as `firespritesheet.zip`. |
| One-time flammable object | [Woodpile — Pixor](https://pixor.art/de/library/village/woodpile/) | CC0 | 48×48 sprite sheet with 8 viewing directions; source PNG is stored as `pixor-library-woodpile-selout-48px-8-sides.png`. |
| Final bonfire | [Idylwild's Chromatic Campfires](https://opengameart.org/content/idylwilds-chromatic-campfires) | CC0 | 10 animated torches/campfires in 5 colors, 4-frame animations, smoke and unlit/burnt variants; source archive is stored as `idylwilds_chromatic_campfires.zip`. |

## Import workflow

1. Keep the three source files in `Assets/Art/sources/`.
2. In Unity, run **Tools → Art → Import Approved Art Assets**.
3. The tool extracts PNG files from the two ZIP archives into:
   - `Assets/Art/Imported/Player/`
   - `Assets/Art/Imported/Campfires/`
   - `Assets/Art/Imported/Environment/`
4. It applies Sprite texture type, Point filtering, no compression, no mipmaps, alpha transparency, and Clamp wrapping.
5. Inspect the extracted images and use Sprite Editor to slice sprite sheets according to their actual frame layout. Create clips only for the chosen visual variants.

The importer is intentionally non-destructive: it does not overwrite existing extracted PNG files and does not modify scenes, prefabs, gameplay scripts, or level layout.

## Scope boundaries

- Keep the existing Player GameObject, Rigidbody2D, CircleCollider2D and movement scripts unchanged.
- Do not change the URP 2D Global Light or add scene lights as part of this asset import.
- Scene assembly and level design remain outside the Art & VFX branch.
- The Editor tool is a repeatable extraction/import helper, not a scene generator.

## Current status

- [x] Original source files committed under `Assets/Art/sources/`.
- [x] Unity Editor extraction/import helper added.
- [ ] Run the helper in Unity Editor.
- [ ] Inspect actual extracted PNG dimensions and choose sprite-sheet slicing layouts.
- [ ] Create the selected flame and campfire animation clips.
- [ ] Validate visual scale and sorting in the target scene without editing the scene from this branch.

All three sources are identified as CC0 by their respective source pages. Keep this manifest with the project if assets are moved or replaced.
