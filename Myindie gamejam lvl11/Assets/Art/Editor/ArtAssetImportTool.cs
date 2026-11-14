using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameJam.Art.Editor
{
    /// <summary>
    /// Extracts approved assets, preserves existing sprite-sheet slicing,
    /// and creates a reusable player flame animation. Does not edit scenes.
    /// </summary>
    public static class ArtAssetImportTool
    {
        private const string SourceFolder = "Assets/Art/sources";
        private const string ImportedFolder = "Assets/Art/Imported";
        private const string FlameArchive = "firespritesheet.zip";
        private const string CampfireArchive = "idylwilds_chromatic_campfires.zip";
        private const string AnimationFolder = "Assets/Art/Animations";
        private const string PlayerFlameClip = AnimationFolder + "/PlayerFlame.anim";
        private const float PlayerFlameFramesPerSecond = 12f;

        [MenuItem("Tools/Art/Import Approved Art Assets")]
        public static void ImportApprovedAssets()
        {
            EnsureFolder(ImportedFolder);

            int extracted = 0;
            extracted += ExtractPngs(Path.Combine(SourceFolder, FlameArchive), Path.Combine(ImportedFolder, "Player"));
            extracted += ExtractPngs(Path.Combine(SourceFolder, CampfireArchive), Path.Combine(ImportedFolder, "Campfires"));

            string woodpileSource = Path.Combine(SourceFolder, "pixor-library-woodpile-selout-48px-8-sides.png");
            if (File.Exists(woodpileSource))
            {
                string destinationFolder = Path.Combine(ImportedFolder, "Environment");
                EnsureFolder(destinationFolder);
                string destination = Path.Combine(destinationFolder, Path.GetFileName(woodpileSource));
                if (!File.Exists(destination))
                {
                    File.Copy(woodpileSource, destination);
                    extracted++;
                }
            }
            else
            {
                Debug.LogWarning($"[Art Import] Woodpile source not found: {woodpileSource}");
            }

            AssetDatabase.Refresh();
            ConfigureImportedTextures();
            AssetDatabase.SaveAssets();

            int flameFrames = CreatePlayerFlameAnimation();
            int campfireSprites = CountSprites(Path.Combine(ImportedFolder, "Campfires"));
            int woodpileSprites = CountSprites(Path.Combine(ImportedFolder, "Environment"));

            Debug.Log($"[Art Import] Complete. New PNG files: {extracted}. " +
                      $"Player flame frames: {flameFrames}; campfire sprites: {campfireSprites}; woodpile directions: {woodpileSprites}. " +
                      "No scene or gameplay objects were changed.");

            EditorUtility.DisplayDialog(
                "Art assets ready",
                $"New PNG files: {extracted}\nPlayer flame frames: {flameFrames}\nCampfire sprites: {campfireSprites}\nWoodpile sprites: {woodpileSprites}\n\n" +
                "PlayerFlame.anim is in Assets/Art/Animations. No scene or gameplay objects were changed.",
                "OK");
        }

        [MenuItem("Tools/Art/Rebuild Player Flame Animation")]
        public static void RebuildPlayerFlameAnimation()
        {
            int frames = CreatePlayerFlameAnimation();
            EditorUtility.DisplayDialog("Player flame animation", $"Animation created from {frames} frames.", "OK");
        }

        private static int ExtractPngs(string archivePath, string destinationFolder)
        {
            if (!File.Exists(archivePath))
            {
                Debug.LogWarning($"[Art Import] Archive not found: {archivePath}");
                return 0;
            }

            EnsureFolder(destinationFolder);
            int extracted = 0;

            try
            {
                using (FileStream stream = File.OpenRead(archivePath))
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name) ||
                            !entry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string safeName = Path.GetFileName(entry.Name);
                        if (string.IsNullOrEmpty(safeName))
                            continue;

                        string destination = Path.Combine(destinationFolder, safeName);
                        if (File.Exists(destination))
                            continue;

                        using (Stream input = entry.Open())
                        using (FileStream output = File.Create(destination))
                            input.CopyTo(output);

                        extracted++;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Art Import] Failed to extract '{archivePath}': {exception}");
            }

            return extracted;
        }

        private static void ConfigureImportedTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ImportedFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                    continue;

                importer.textureType = TextureImporterType.Sprite;

                // Preserve Multiple slicing already stored in the committed .meta files.
                // Only switch to Single for textures that have no existing sprite-sheet data.
                if (importer.spriteImportMode != SpriteImportMode.Multiple)
                    importer.spriteImportMode = SpriteImportMode.Single;

                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
        }

        private static int CreatePlayerFlameAnimation()
        {
            string playerFolder = Path.Combine(ImportedFolder, "Player").Replace('\\', '/');
            string[] guids = AssetDatabase.FindAssets("fireB t:Sprite", new[] { playerFolder });

            Sprite[] frames = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(sprite => sprite != null && sprite.name.StartsWith("fireB", StringComparison.Ordinal))
                .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
                .ToArray();

            if (frames.Length != 25)
            {
                Debug.LogWarning($"[Art Import] Expected 25 individual fireB frames, found {frames.Length}. Animation was not changed.");
                return frames.Length;
            }

            EnsureFolder(AnimationFolder);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PlayerFlameClip);
            if (clip == null)
            {
                clip = new AnimationClip { frameRate = PlayerFlameFramesPerSecond };
                AssetDatabase.CreateAsset(clip, PlayerFlameClip);
            }
            else
            {
                clip.ClearCurves();
                clip.frameRate = PlayerFlameFramesPerSecond;
            }

            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / PlayerFlameFramesPerSecond,
                    value = frames[i]
                };
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Art Import] Created {PlayerFlameClip} ({frames.Length} frames at {PlayerFlameFramesPerSecond} FPS, looping).");
            return frames.Length;
        }

        private static int CountSprites(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return 0;

            return AssetDatabase.FindAssets("t:Sprite", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<Sprite>()
                .Count();
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
                return;

            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
                throw new InvalidOperationException($"Invalid asset folder path: {assetPath}");

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
