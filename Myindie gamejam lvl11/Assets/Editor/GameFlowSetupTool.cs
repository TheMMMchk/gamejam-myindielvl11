using System.Collections.Generic;
using Game.GameFlow;
using Game.Gameplay.Heat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class GameFlowSetupTool : EditorWindow
{
    private GameObject _finalBonfire;
    [SerializeField] private List<FlammableHeatSource> _requiredSources = new List<FlammableHeatSource>();

    [MenuItem("Tools/Game Flow/Setup")]
    private static void Open() => GetWindow<GameFlowSetupTool>("Game Flow Setup");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Game Flow setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Назначьте финальный костёр и все обязательные горючие объекты уровня. Объекты размещаются вручную; инструмент только связывает системы и создаёт HUD.",
            MessageType.Info);

        _finalBonfire = (GameObject)EditorGUILayout.ObjectField(
            "Финальный костёр", _finalBonfire, typeof(GameObject), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Обязательные горючие объекты", EditorStyles.boldLabel);

        for (int i = 0; i < _requiredSources.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _requiredSources[i] = (FlammableHeatSource)EditorGUILayout.ObjectField(
                "Объект " + (i + 1), _requiredSources[i], typeof(FlammableHeatSource), true);

            if (GUILayout.Button("−", GUILayout.Width(28)))
            {
                _requiredSources.RemoveAt(i);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Добавить обязательный объект"))
            _requiredSources.Add(null);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(_finalBonfire == null))
        {
            if (GUILayout.Button("Создать / проверить Game Flow"))
                Setup();
        }
    }

    private void Setup()
    {
        if (_finalBonfire == null)
        {
            EditorUtility.DisplayDialog("Game Flow", "Укажите объект финального костра.", "OK");
            return;
        }

        Collider2D goalCollider = _finalBonfire.GetComponent<Collider2D>();
        if (goalCollider == null)
        {
            EditorUtility.DisplayDialog("Game Flow", "На финальном костре нет Collider2D. Добавьте подходящий Collider2D вручную. Инструмент не меняет геометрию уровня.", "OK");
            return;
        }

        if (_requiredSources.Count == 0)
        {
            EditorUtility.DisplayDialog("Game Flow", "Добавьте хотя бы один обязательный горючий объект.", "OK");
            return;
        }

        HashSet<FlammableHeatSource> uniqueSources = new HashSet<FlammableHeatSource>();
        for (int i = 0; i < _requiredSources.Count; i++)
        {
            FlammableHeatSource source = _requiredSources[i];
            if (source == null)
            {
                EditorUtility.DisplayDialog("Game Flow", $"В списке обязательных объектов не заполнен элемент {i + 1}.", "OK");
                return;
            }

            if (!uniqueSources.Add(source))
            {
                EditorUtility.DisplayDialog("Game Flow", $"Объект {source.name} добавлен несколько раз. Удалите повтор.", "OK");
                return;
            }
        }

        PlayerHeatBootstrap[] heatComponents = Object.FindObjectsByType<PlayerHeatBootstrap>(FindObjectsSortMode.None);
        if (heatComponents.Length != 1)
        {
            EditorUtility.DisplayDialog("Game Flow", $"Ожидался ровно один PlayerHeatBootstrap, найдено: {heatComponents.Length}. Проверьте Player.", "OK");
            return;
        }

        GameObject flowObject = GameObject.Find("Game Flow");
        if (flowObject == null)
        {
            flowObject = new GameObject("Game Flow");
            Undo.RegisterCreatedObjectUndo(flowObject, "Create Game Flow");
        }

        Undo.RecordObject(goalCollider, "Configure final bonfire trigger");
        goalCollider.isTrigger = true;

        GameFlowBootstrap flow = flowObject.GetComponent<GameFlowBootstrap>();
        if (flow == null)
            flow = Undo.AddComponent<GameFlowBootstrap>(flowObject);

        GameObject canvasObject = GameObject.Find("Game Flow Canvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("Game Flow Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Game Flow Canvas");
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(eventObject, "Create EventSystem");
            eventObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        Transform hud = EnsureUIObject("Heat HUD", canvasObject.transform);
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.5f, 1f);
        hudRect.anchorMax = new Vector2(0.5f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.anchoredPosition = new Vector2(0, -24);
        hudRect.sizeDelta = new Vector2(700, 190);

        Text heatLabel = EnsureText("Heat Label", hud, "Жар: 100/100", 38);
        SetRect(heatLabel.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(660, 52));

        Text progressLabel = EnsureText("Progress Label", hud, "Зажжено: 0/0", 32);
        SetRect(progressLabel.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(660, 44));

        Text statusLabel = EnsureText("Status Label", hud, "Зажгите все обязательные объекты.", 24);
        SetRect(statusLabel.rectTransform, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(660, 38));

        Transform sliderTransform = EnsureUIObject("Heat Slider", hud);
        RectTransform heatSliderRect = sliderTransform.GetComponent<RectTransform>();
        SetRect(heatSliderRect, new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), Vector2.zero, new Vector2(620, 20));
        Slider slider = EnsureComponent<Slider>(sliderTransform.gameObject);
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;

        Transform background = EnsureUIObject("Background", sliderTransform);
        SetStretch(background.GetComponent<RectTransform>());
        Image bgImage = EnsureComponent<Image>(background.gameObject);
        bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        Transform fillArea = EnsureUIObject("Fill Area", sliderTransform);
        SetStretch(fillArea.GetComponent<RectTransform>());
        Transform fill = EnsureUIObject("Fill", fillArea);
        SetStretch(fill.GetComponent<RectTransform>());
        Image fillImage = EnsureComponent<Image>(fill.gameObject);
        fillImage.color = new Color(1f, 0.48f, 0.12f);
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.targetGraphic = fillImage;

        Transform panel = EnsureUIObject("Result Panel", canvasObject.transform);
        Image panelImage = EnsureComponent<Image>(panel.gameObject);
        panelImage.color = new Color(0.035f, 0.025f, 0.02f, 0.92f);
        SetStretch(panel.GetComponent<RectTransform>());

        Text resultLabel = EnsureText("Result Label", panel, "КОСТЁР ЗАЖЖЁН", 56);
        SetRect(resultLabel.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(1000, 120));

        Transform restartButtonTransform = EnsureUIObject("Restart Button", panel);
        SetRect(restartButtonTransform.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(320, 76));
        Image buttonImage = EnsureComponent<Image>(restartButtonTransform.gameObject);
        buttonImage.color = new Color(0.85f, 0.35f, 0.12f, 1f);
        Button button = EnsureComponent<Button>(restartButtonTransform.gameObject);
        button.targetGraphic = buttonImage;
        Text buttonText = EnsureText("Button Label", restartButtonTransform, "Заново", 32);
        SetStretch(buttonText.rectTransform);
        panel.gameObject.SetActive(false);

        GameFlowUI ui = canvasObject.GetComponent<GameFlowUI>();
        if (ui == null)
            ui = Undo.AddComponent<GameFlowUI>(canvasObject);

        SerializedObject uiSerialized = new SerializedObject(ui);
        uiSerialized.FindProperty("_heatSlider").objectReferenceValue = slider;
        uiSerialized.FindProperty("_heatLabel").objectReferenceValue = heatLabel;
        uiSerialized.FindProperty("_progressLabel").objectReferenceValue = progressLabel;
        uiSerialized.FindProperty("_statusLabel").objectReferenceValue = statusLabel;
        uiSerialized.FindProperty("_resultPanel").objectReferenceValue = panel.gameObject;
        uiSerialized.FindProperty("_resultLabel").objectReferenceValue = resultLabel;
        uiSerialized.FindProperty("_restartButton").objectReferenceValue = button;
        uiSerialized.ApplyModifiedProperties();

        SerializedObject flowSerialized = new SerializedObject(flow);
        flowSerialized.FindProperty("_playerHeat").objectReferenceValue = heatComponents[0];
        flowSerialized.FindProperty("_ui").objectReferenceValue = ui;
        SerializedProperty requiredSourcesProperty = flowSerialized.FindProperty("_requiredSources");
        requiredSourcesProperty.arraySize = _requiredSources.Count;
        for (int i = 0; i < _requiredSources.Count; i++)
            requiredSourcesProperty.GetArrayElementAtIndex(i).objectReferenceValue = _requiredSources[i];
        flowSerialized.ApplyModifiedProperties();

        FinalBonfireTrigger goal = _finalBonfire.GetComponent<FinalBonfireTrigger>();
        if (goal == null)
            goal = Undo.AddComponent<FinalBonfireTrigger>(_finalBonfire);
        goal.SetGameFlow(flow);

        EditorUtility.SetDirty(flow);
        EditorUtility.SetDirty(ui);
        EditorUtility.SetDirty(goal);
        EditorSceneManager.MarkSceneDirty(flowObject.scene);
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorUtility.DisplayDialog("Game Flow", "Сохранение отменено. Изменения остались в открытой сцене.", "OK");
            return;
        }

        EditorUtility.DisplayDialog("Game Flow", "Game Flow настроен. Проверьте прогресс и финальный костёр в Play Mode.", "OK");
    }

    private static Transform EnsureUIObject(string name, Transform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static T EnsureComponent<T>(GameObject owner) where T : Component
    {
        T component = owner.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(owner);
    }

    private static Text EnsureText(string name, Transform parent, string value, int size)
    {
        Transform textTransform = EnsureUIObject(name, parent);
        Text text = EnsureComponent<Text>(textTransform.gameObject);
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return text;
    }

    private static void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
