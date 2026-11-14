#if UNITY_EDITOR
using Game.Gameplay.Heat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.EditorTools
{
    /// <summary>
    /// One-click setup for heat mechanics in the currently open scene.
    /// Adds heat to Player and creates simple test pickup / heat-source objects.
    /// </summary>
    public sealed class GameplayHeatSetupTool : EditorWindow
    {
        private bool _createDemoObjects = true;
        private bool _saveScene = true;

        [MenuItem("Tools/Gameplay/Heat Setup")]
        private static void Open()
        {
            GetWindow<GameplayHeatSetupTool>("Heat Setup");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Gameplay — Heat Mechanics", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Adds PlayerHeatBootstrap to the Player object in the active scene. " +
                "Optionally creates a test fuel pickup and a limited-time heat source.",
                MessageType.Info);

            EditorGUILayout.LabelField("Active scene", SceneManager.GetActiveScene().name);
            _createDemoObjects = EditorGUILayout.ToggleLeft("Create demo pickup and flammable object", _createDemoObjects);
            _saveScene = EditorGUILayout.ToggleLeft("Save scene after setup", _saveScene);

            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Set Up Heat Mechanics", GUILayout.Height(32f)))
                {
                    ApplySetup();
                }
            }

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Stop Play Mode before changing the scene.", MessageType.Warning);
            }
        }

        private void ApplySetup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Heat Setup", "No valid active scene is loaded.", "OK");
                return;
            }

            GameObject player = FindInScene(scene, "Player");
            if (player == null)
            {
                EditorUtility.DisplayDialog(
                    "Heat Setup",
                    "Could not find a root GameObject named 'Player' in the active scene.",
                    "OK");
                return;
            }

            if (player.GetComponent<Rigidbody2D>() == null || player.GetComponent<Collider2D>() == null)
            {
                EditorUtility.DisplayDialog(
                    "Heat Setup",
                    "Player needs both Rigidbody2D and Collider2D for 2D trigger interactions.",
                    "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set Up Heat Mechanics");

            if (player.GetComponent<PlayerHeatBootstrap>() == null)
            {
                Undo.AddComponent<PlayerHeatBootstrap>(player);
            }

            int createdCount = 0;
            if (_createDemoObjects)
            {
                SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
                Sprite playerSprite = playerRenderer != null ? playerRenderer.sprite : null;
                int sortingOrder = playerRenderer != null ? playerRenderer.sortingOrder + 1 : 1;

                GameObject pickup = GetOrCreateDemoObject(
                    scene, "Heat Pickup (Demo)", player, playerSprite,
                    new Vector3(-2.0f, 0.2f, 0f), new Color(0.25f, 0.9f, 0.45f, 1f),
                    sortingOrder, 0.55f, out bool pickupCreated);

                EnsurePickup(pickup);
                if (pickupCreated) createdCount++;

                GameObject heatSource = GetOrCreateDemoObject(
                    scene, "Flammable Heat Source (Demo)", player, playerSprite,
                    new Vector3(2.0f, 0.2f, 0f), new Color(1f, 0.42f, 0.12f, 1f),
                    sortingOrder, 0.85f, out bool sourceCreated);

                EnsureHeatSource(heatSource);
                if (sourceCreated) createdCount++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);

            bool saved = _saveScene && !string.IsNullOrEmpty(scene.path)
                && EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);

            string message = "Player heat component is ready.";
            if (_createDemoObjects)
            {
                message += "\nDemo pickup and flammable source are ready. Newly created objects: " + createdCount + ".";
            }

            message += saved
                ? "\nScene saved."
                : "\nScene changes are in memory; save the scene manually.";

            EditorUtility.DisplayDialog("Heat Setup Complete", message, "OK");
            Debug.Log("[Heat Setup] " + message, player);
        }

        private static GameObject FindInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    return root;
                }
            }

            return null;
        }

        private static GameObject GetOrCreateDemoObject(
            Scene scene,
            string objectName,
            GameObject player,
            Sprite sprite,
            Vector3 offset,
            Color color,
            int sortingOrder,
            float colliderRadius,
            out bool created)
        {
            GameObject target = FindInScene(scene, objectName);
            created = target == null;

            if (created)
            {
                target = new GameObject(objectName);
                Undo.RegisterCreatedObjectUndo(target, "Create " + objectName);
                target.transform.position = player.transform.position + offset;
                target.layer = player.layer;
            }

            SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = Undo.AddComponent<SpriteRenderer>(target);
            }

            Undo.RecordObject(renderer, "Configure " + objectName);
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            Undo.RecordObject(target.transform, "Set " + objectName + " Scale");
            target.transform.localScale = Vector3.one * 0.7f;

            CircleCollider2D trigger = target.GetComponent<CircleCollider2D>();
            if (trigger == null)
            {
                trigger = Undo.AddComponent<CircleCollider2D>(target);
            }

            Undo.RecordObject(trigger, "Configure " + objectName + " Trigger");
            trigger.isTrigger = true;
            trigger.radius = colliderRadius;

            return target;
        }

        private static void EnsurePickup(GameObject target)
        {
            HeatPickup pickup = target.GetComponent<HeatPickup>();
            if (pickup == null)
            {
                pickup = Undo.AddComponent<HeatPickup>(target);
            }

            SerializedObject serializedPickup = new SerializedObject(pickup);
            serializedPickup.FindProperty("_heatAmount").floatValue = 35f;
            serializedPickup.ApplyModifiedProperties();
        }

        private static void EnsureHeatSource(GameObject target)
        {
            FlammableHeatSource source = target.GetComponent<FlammableHeatSource>();
            if (source == null)
            {
                source = Undo.AddComponent<FlammableHeatSource>(target);
            }

            SerializedObject serializedSource = new SerializedObject(source);
            serializedSource.FindProperty("_burnDuration").floatValue = 4f;
            serializedSource.FindProperty("_heatPerSecond").floatValue = 12f;
            serializedSource.ApplyModifiedProperties();
        }
    }
}
#endif
