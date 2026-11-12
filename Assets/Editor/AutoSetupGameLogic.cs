using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AutoSetupGameLogic
{
    [MenuItem("Tools/Flappy Bird/Auto Setup Game Logic", false, 3)]
    public static void SetupGameLogic()
    {
        // 1. Pastikan EventSystem ada di Scene (agar Button UI bisa diklik)
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            eventSystem = esGo.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            esGo.AddComponent<StandaloneInputModule>();
#endif
        }

        // 2. Dapatkan font bawaan untuk Legacy UI Text (LegacyRuntime.ttf atau Arial.ttf)
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        if (defaultFont == null)
        {
            string[] fontGuids = AssetDatabase.FindAssets("t:Font");
            if (fontGuids.Length > 0)
            {
                defaultFont = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));
            }
        }

        // 3. Cari atau buat Canvas GameUI
        GameObject canvasGo = GameObject.Find("GameUI");
        if (canvasGo != null)
        {
            Undo.DestroyObjectImmediate(canvasGo);
        }

        canvasGo = new GameObject("GameUI");
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create GameUI Canvas");

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // 4. Buat ScoreText (posisi atas tengah layar) menggunakan Legacy UI Text
        GameObject scoreTextGo = new GameObject("ScoreText");
        scoreTextGo.transform.SetParent(canvasGo.transform, false);

        RectTransform scoreRect = scoreTextGo.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0f, -40f);
        scoreRect.sizeDelta = new Vector2(400f, 120f);

        Text scoreText = scoreTextGo.AddComponent<Text>();
        scoreText.font = defaultFont;
        scoreText.text = "0";
        scoreText.fontSize = 80;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.color = Color.white;

        // 5. Buat GameOverPanel (tengah layar, tersembunyi di awal)
        GameObject panelGo = new GameObject("GameOverPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600f, 440f);

        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

        // 5A. Teks Judul "GAME OVER"
        GameObject titleGo = new GameObject("GameOverTitle");
        titleGo.transform.SetParent(panelGo.transform, false);

        RectTransform titleRect = titleGo.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 130f);
        titleRect.sizeDelta = new Vector2(500f, 80f);

        Text titleText = titleGo.AddComponent<Text>();
        titleText.font = defaultFont;
        titleText.text = "GAME OVER";
        titleText.fontSize = 62;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(1f, 0.35f, 0.35f);

        // 5B. Teks Skor Akhir "Score: 0"
        GameObject finalScoreGo = new GameObject("FinalScoreText");
        finalScoreGo.transform.SetParent(panelGo.transform, false);

        RectTransform finalScoreRect = finalScoreGo.AddComponent<RectTransform>();
        finalScoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalScoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalScoreRect.pivot = new Vector2(0.5f, 0.5f);
        finalScoreRect.anchoredPosition = new Vector2(0f, 30f);
        finalScoreRect.sizeDelta = new Vector2(500f, 60f);

        Text finalScoreText = finalScoreGo.AddComponent<Text>();
        finalScoreText.font = defaultFont;
        finalScoreText.text = "Score: 0";
        finalScoreText.fontSize = 46;
        finalScoreText.alignment = TextAnchor.MiddleCenter;
        finalScoreText.color = Color.white;

        // 5C. Tombol "Restart"
        GameObject buttonGo = new GameObject("RestartButton");
        buttonGo.transform.SetParent(panelGo.transform, false);

        RectTransform btnRect = buttonGo.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0f, -100f);
        btnRect.sizeDelta = new Vector2(260f, 75f);

        Image btnImg = buttonGo.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.75f, 0.35f);

        Button restartBtn = buttonGo.AddComponent<Button>();

        GameObject btnTextGo = new GameObject("Text");
        btnTextGo.transform.SetParent(buttonGo.transform, false);

        RectTransform btnTextRect = btnTextGo.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        Text btnText = btnTextGo.AddComponent<Text>();
        btnText.font = defaultFont;
        btnText.text = "Restart";
        btnText.fontSize = 34;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;

        // Sembunyikan panel di awal permainan
        panelGo.SetActive(false);

        // 6. Setup dan Hubungkan ke GameObject GameManager
        GameObject gmGo = GameObject.Find("GameManager");
        if (gmGo == null)
        {
            gmGo = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmGo, "Create GameManager");
        }

        GameManager gm = gmGo.GetComponent<GameManager>();
        if (gm == null)
        {
            gm = gmGo.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGo, "Add GameManager Component");
        }

        // Hubungkan referensi UI ke GameManager via SerializedObject
        SerializedObject gmSo = new SerializedObject(gm);
        gmSo.FindProperty("scoreText").objectReferenceValue = scoreText;
        gmSo.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
        gmSo.FindProperty("gameOverPanel").objectReferenceValue = panelGo;
        gmSo.FindProperty("restartButton").objectReferenceValue = restartBtn;
        gmSo.ApplyModifiedProperties();

        EditorUtility.SetDirty(gm);
        EditorUtility.SetDirty(gmGo);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = gmGo;

        Debug.Log("[AutoSetupGameLogic] Sukses! Canvas GameUI, ScoreText, GameOverPanel, dan GameManager berhasil dikonfigurasi dengan Legacy UI.");
        EditorUtility.DisplayDialog(
            "Game Logic Setup Selesai",
            "Canvas GameUI, ScoreText, GameOverPanel, dan GameManager berhasil dikonfigurasi dengan UI Legacy (bebas error TMP)!\n\nTekan Play untuk menguji skor dan Game Over.",
            "OK"
        );
    }
}
