using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameJam.Art.Editor
{
    /// <summary>
    /// Extracts the approved source archives into Assets/Art/Imported and applies
    /// safe pixel-art texture settings. It deliberately does not modify scenes.
    /// </summary>
    public static class ArtAssetImportTool
    {
        private const string SourceFolder = "Assets/Art/sources";
        private const string ImportedFolder = "Assets/Art/Imported";
        private const string FlameArchive = "firespritesheet.zip";
        private const string CampfireArchive = "idylwilds_chromatic_campfires.zip";

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

            Debug.Log($"[Art Import] Complete. New PNG files extracted/copied: {extracted}. " +
                      "Texture import settings were applied. Review the Imported folders and slice sprite sheets in Sprite Editor before creating clips.");
            EditorUtility.DisplayDialog(
                "Art assets imported",
                $"New PNG files: {extracted}\n\n" +
                "Pixel-art texture settings have been applied. Open Assets/Art/Imported and inspect the sprite sheets. " +
                "No scene or gameplay objects were changed.",
                "OK");
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

                        // Flatten archive subfolders while retaining the original PNG filename.
                        string safeName = Path.GetFileName(entry.Name);
                        if (string.IsNullOrEmpty(safeName))
                            continue;

                        string destination = Path.Combine(destinationFolder, safeName);
                        if (File.Exists(destination))
                        {
                            Debug.Log($"[Art Import] Keeping existing file, not overwriting: {destination}");
                            continue;
                        }

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
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
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
