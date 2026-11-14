using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.EditorTools
{
    public static class Level01SceneCreator
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string LevelScenePath = "Assets/Scenes/Level01.unity";
        private const string MoveReferencePath = "Assets/Settings/PlayerMoveActionReference.asset";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        [MenuItem("Tools/Level Design/Generate Level 01")]
        public static void Generate()
        {
            if (!File.Exists(SampleScenePath))
            {
                Debug.LogError($"Level Design: source scene not found: {SampleScenePath}");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            GameObject player = GameObject.Find("Player");
            Camera camera = Camera.main;

            if (player == null || camera == null)
            {
                Debug.LogError("Level Design: SampleScene must contain Player and Main Camera.");
                return;
            }

            SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer == null || playerRenderer.sprite == null)
            {
                Debug.LogError("Level Design: Player needs a SpriteRenderer with a sprite.");
                return;
            }

            RemoveGeneratedRoot("Level01");
            GameObject levelRoot = new GameObject("Level01");
            Transform root = levelRoot.transform;

            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.075f);

            player.transform.position = new Vector3(-4.1f, -2.75f, 0f);
            player.transform.localScale = Vector3.one * 0.72f;
            ConfigureMovement(player);
            ConfigureHeat(player);

            CreateBoundary(root, "North Boundary", new Vector2(0f, 3.65f), new Vector2(10.4f, 0.35f));
            CreateBoundary(root, "South Boundary", new Vector2(0f, -3.65f), new Vector2(10.4f, 0.35f));
            CreateBoundary(root, "West Boundary", new Vector2(-5.05f, 0f), new Vector2(0.35f, 7.3f));
            CreateBoundary(root, "East Boundary", new Vector2(5.05f, 0f), new Vector2(0.35f, 7.3f));

            // A central rock formation blocks the direct diagonal route and creates two readable paths.
            CreateRock(root, playerRenderer.sprite, "Rock A", new Vector2(-0.45f, 0.35f), new Vector2(1.15f, 1.15f));
            CreateRock(root, playerRenderer.sprite, "Rock B", new Vector2(0.55f, 0.9f), new Vector2(0.9f, 0.9f));
            CreateRock(root, playerRenderer.sprite, "Rock C", new Vector2(0.55f, -0.15f), new Vector2(1.0f, 1.0f));
            CreateRock(root, playerRenderer.sprite, "Rock D", new Vector2(-0.35f, -0.8f), new Vector2(0.85f, 0.85f));

            // Left path: longer, but has two heat sources.
            CreateHeatSource(root, playerRenderer.sprite, "Dry Woodpile 01", new Vector2(-2.75f, -1.55f), 0.85f);
            CreateHeatSource(root, playerRenderer.sprite, "Dry Woodpile 02", new Vector2(-2.65f, 1.35f), 0.85f);

            // Right path: more direct, with one source before the final clearing.
            CreateHeatSource(root, playerRenderer.sprite, "Dry Woodpile 03", new Vector2(2.65f, 0.05f), 0.9f);

            CreateRock(root, playerRenderer.sprite, "Left Path Stone 01", new Vector2(-4.0f, -0.25f), new Vector2(0.55f, 0.55f));
            CreateRock(root, playerRenderer.sprite, "Left Path Stone 02", new Vector2(-3.8f, 2.45f), new Vector2(0.65f, 0.65f));
            CreateRock(root, playerRenderer.sprite, "Right Path Stone 01", new Vector2(3.9f, -1.8f), new Vector2(0.7f, 0.7f));
            CreateRock(root, playerRenderer.sprite, "Right Path Stone 02", new Vector2(3.8f, 1.55f), new Vector2(0.55f, 0.55f));

            CreateFinalBonfire(root, playerRenderer.sprite, new Vector2(4.05f, 2.65f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LevelScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Level Design: generated {LevelScenePath}. Check Player movement input and Gameplay components before Play Mode.");
        }

        private static void ConfigureMovement(GameObject player)
        {
            PlayerMovementBootstrap movement = player.GetComponent<PlayerMovementBootstrap>();
            if (movement == null)
            {
                movement = player.AddComponent<PlayerMovementBootstrap>();
            }

            InputActionReference moveReference = AssetDatabase.LoadAssetAtPath<InputActionReference>(MoveReferencePath);
            if (moveReference == null)
            {
                InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
                InputAction moveAction = actions != null ? actions.FindAction("Player/Move") : null;
                if (moveAction == null)
                {
                    Debug.LogError("Level Design: could not find Player/Move in InputSystem_Actions.inputactions.");
                    return;
                }

                moveReference = InputActionReference.Create(moveAction);
                AssetDatabase.CreateAsset(moveReference, MoveReferencePath);
                AssetDatabase.SaveAssets();
            }

            SerializedObject serializedMovement = new SerializedObject(movement);
            serializedMovement.FindProperty("_moveAction").objectReferenceValue = moveReference;
            serializedMovement.ApplyModifiedPropertiesWithoutUndo();
            movement.enabled = true;
        }

        private static void ConfigureHeat(GameObject player)
        {
            Type heatType = FindRuntimeType("Game.Gameplay.Heat.PlayerHeatBootstrap");
            if (heatType == null)
            {
                Debug.LogWarning("Level Design: PlayerHeatBootstrap is not available yet. Merge Gameplay before testing heat.");
                return;
            }

            if (player.GetComponent(heatType) == null)
            {
                player.AddComponent(heatType);
            }
        }

        private static void CreateBoundary(Transform parent, string objectName, Vector2 position, Vector2 size)
        {
            GameObject boundary = new GameObject(objectName);
            boundary.transform.SetParent(parent);
            boundary.transform.position = position;
            BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void CreateRock(Transform parent, Sprite sprite, string objectName, Vector2 position, Vector2 size)
        {
            GameObject rock = new GameObject(objectName);
            rock.transform.SetParent(parent);
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = rock.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.17f, 0.2f, 0.25f, 1f);
            renderer.sortingOrder = 2;

            CircleCollider2D collider = rock.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
        }

        private static void CreateHeatSource(Transform parent, Sprite sprite, string objectName, Vector2 position, float triggerRadius)
        {
            GameObject source = new GameObject(objectName);
            source.transform.SetParent(parent);
            source.transform.position = position;

            CircleCollider2D trigger = source.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = triggerRadius;

            Type sourceType = FindRuntimeType("Game.Gameplay.Heat.FlammableHeatSource");
            if (sourceType == null)
            {
                Debug.LogWarning($"{objectName}: FlammableHeatSource is missing until the Gameplay branch is merged.");
            }
            else
            {
                source.AddComponent(sourceType);
            }

            CreateLogVisual(source.transform, sprite, new Vector2(-0.12f, 0f), new Vector2(0.95f, 0.28f), new Color(0.35f, 0.19f, 0.1f, 1f), -1f);
            CreateLogVisual(source.transform, sprite, new Vector2(0.12f, 0.16f), new Vector2(0.85f, 0.25f), new Color(0.48f, 0.25f, 0.11f, 1f), 0.35f);
            CreateLogVisual(source.transform, sprite, new Vector2(0.05f, -0.16f), new Vector2(0.75f, 0.22f), new Color(0.28f, 0.14f, 0.08f, 1f), -0.3f);
        }

        private static void CreateLogVisual(Transform parent, Sprite sprite, Vector2 offset, Vector2 size, Color color, float rotation)
        {
            GameObject log = new GameObject("Log Visual");
            log.transform.SetParent(parent);
            log.transform.localPosition = offset;
            log.transform.localRotation = Quaternion.Euler(0f, 0f, rotation * Mathf.Rad2Deg);
            log.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = log.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = 3;
        }

        private static void CreateFinalBonfire(Transform parent, Sprite sprite, Vector2 position)
        {
            GameObject bonfire = new GameObject("Final Bonfire - Goal Visual");
            bonfire.transform.SetParent(parent);
            bonfire.transform.position = position;

            CreateLogVisual(bonfire.transform, sprite, new Vector2(-0.12f, -0.08f), new Vector2(1.0f, 0.24f), new Color(0.28f, 0.13f, 0.07f, 1f), 0.55f);
            CreateLogVisual(bonfire.transform, sprite, new Vector2(0.12f, -0.08f), new Vector2(1.0f, 0.24f), new Color(0.42f, 0.2f, 0.08f, 1f), -0.55f);
            CreateLogVisual(bonfire.transform, sprite, new Vector2(0f, 0.3f), new Vector2(0.52f, 0.82f), new Color(1f, 0.42f, 0.08f, 1f), 0f);
            CreateLogVisual(bonfire.transform, sprite, new Vector2(0.02f, 0.24f), new Vector2(0.28f, 0.52f), new Color(1f, 0.78f, 0.2f, 1f), 0f);
        }

        private static Type FindRuntimeType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void RemoveGeneratedRoot(string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }
        }
    }
}
