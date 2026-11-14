using Game.GameFlow;
using Game.Gameplay.Heat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GameFlowSetupTool : EditorWindow
{
    private GameObject _finalBonfire;

    [MenuItem("Tools/Game Flow/Setup")]
    private static void Open() => GetWindow<GameFlowSetupTool>("Game Flow Setup");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Game Flow setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Выберите объект финального костра. На нём должен быть Collider2D. Инструмент добавит к нему триггер победы и создаст HUD/экран результата, не перемещая объекты уровня.",
            MessageType.Info);

        _finalBonfire = (GameObject)EditorGUILayout.ObjectField(
            "Финальный костёр", _finalBonfire, typeof(GameObject), true);

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
            EditorUtility.DisplayDialog("Game Flow", "На финальном костре нет Collider2D. Добавьте подходящий Collider2D вручную, затем повторите запуск. Инструмент не меняет геометрию уровня.", "OK");
            return;
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
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventObject, "Create EventSystem");
        }

        Transform hud = canvasObject.transform.Find("Heat HUD");
        if (hud == null)
        {
            GameObject hudObject = CreateUIObject("Heat HUD", canvasObject.transform);
            RectTransform rect = hudObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -28);
            rect.sizeDelta = new Vector2(360, 76);

            Text label = CreateText("Heat Label", hudObject.transform, "Тепло: 100/100", 26);
            SetRect(label.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(340, 36));

            GameObject sliderObject = CreateUIObject("Heat Slider", hudObject.transform);
            SetRect(sliderObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(330, 18));
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            GameObject background = CreateUIObject("Background", sliderObject.transform);
            SetStretch(background.GetComponent<RectTransform>());
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
            GameObject fillArea = CreateUIObject("Fill Area", sliderObject.transform);
            SetStretch(fillArea.GetComponent<RectTransform>());
            GameObject fill = CreateUIObject("Fill", fillArea.transform);
            SetStretch(fill.GetComponent<RectTransform>());
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(1f, 0.48f, 0.12f);
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.targetGraphic = fillImage;
        }

        Transform panel = canvasObject.transform.Find("Result Panel");
        if (panel == null)
        {
            GameObject panelObject = CreateUIObject("Result Panel", canvasObject.transform);
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.035f, 0.025f, 0.02f, 0.92f);
            SetStretch(panelObject.GetComponent<RectTransform>());

            Text result = CreateText("Result Label", panelObject.transform, "КОСТЁР НАЙДЕН", 42);
            SetRect(result.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(800, 90));

            GameObject buttonObject = CreateUIObject("Restart Button", panelObject.transform);
            SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(260, 64));
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.85f, 0.35f, 0.12f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            Text buttonText = CreateText("Button Label", buttonObject.transform, "Заново", 26);
            SetStretch(buttonText.rectTransform);
            panelObject.SetActive(false);
        }

        GameFlowUI ui = canvasObject.GetComponent<GameFlowUI>();
        if (ui == null)
            ui = Undo.AddComponent<GameFlowUI>(canvasObject);

        SerializedObject uiSerialized = new SerializedObject(ui);
        uiSerialized.FindProperty("_heatSlider").objectReferenceValue = canvasObject.transform.Find("Heat HUD/Heat Slider").GetComponent<Slider>();
        uiSerialized.FindProperty("_heatLabel").objectReferenceValue = canvasObject.transform.Find("Heat HUD/Heat Label").GetComponent<Text>();
        uiSerialized.FindProperty("_resultPanel").objectReferenceValue = canvasObject.transform.Find("Result Panel").gameObject;
        uiSerialized.FindProperty("_resultLabel").objectReferenceValue = canvasObject.transform.Find("Result Panel/Result Label").GetComponent<Text>();
        uiSerialized.FindProperty("_restartButton").objectReferenceValue = canvasObject.transform.Find("Result Panel/Restart Button").GetComponent<Button>();
        uiSerialized.ApplyModifiedProperties();

        SerializedObject flowSerialized = new SerializedObject(flow);
        flowSerialized.FindProperty("_playerHeat").objectReferenceValue = heatComponents[0];
        flowSerialized.FindProperty("_ui").objectReferenceValue = ui;
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

        EditorUtility.DisplayDialog("Game Flow", "Game Flow и UI настроены. Проверьте финальный костёр и запустите Play Mode.", "OK");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Text CreateText(string name, Transform parent, string value, int size)
    {
        GameObject go = CreateUIObject(name, parent);
        Text text = go.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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
