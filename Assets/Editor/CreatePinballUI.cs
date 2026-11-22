#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu Tools > Créer l’interface Pinball
/// Crée le Canvas (HUD, StartPanel, PausePanel, GameOverPanel), l’EventSystem,
/// les objets de gestion (GameManager, LifeManager, GameOverManager, ScoreManager),
/// une DeathZone et relie toutes les références de l’Inspector.
/// Chaque composant (textes, boutons, icônes) est tourné individuellement de 90° vers la droite,
/// pour un écran 16:9 posé de côté. Le Canvas et les fonds des panneaux ne sont PAS tournés.
/// À placer dans Assets/Editor.
/// </summary>
public static class CreatePinballUI
{
    // Rotation de chaque composant : -90 = 90° vers la droite (sens horaire).
    // Mettre 90 pour tourner vers la gauche.
    private const float RotationZ = -90f;

    // Taille « du point de vue du joueur » (portrait) : 16:9 posé de côté → 1080×1920.
    // Les positions ci-dessous sont définies dans cet espace, puis converties vers l’écran.
    private static readonly Vector2 ReaderSize = new Vector2(1080f, 1920f);

    private static readonly Color AccentColor = new Color(0.16f, 0.52f, 0.9f, 1f);

    private static Sprite uiSprite;
    private static Sprite knobSprite;

    [MenuItem("Tools/Créer l’interface Pinball")]
    public static void Create()
    {
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog(
                "TextMeshPro",
                "Importez d’abord les TMP Essentials : Window > TextMeshPro > Import TMP Essential Resources.",
                "OK");
            return;
        }

        if (GameObject.Find("PinballCanvas") != null)
        {
            EditorUtility.DisplayDialog(
                "Créer l’interface Pinball",
                "Un objet « PinballCanvas » existe déjà dans la scène. Supprimez-le (ainsi que les objets de gestion que vous ne voulez pas conserver) avant de relancer la commande.",
                "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Créer l’interface Pinball");

        uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        EnsureEventSystem();
        Transform canvas = CreateCanvas();

        // ---------------- HUD ----------------
        // Le conteneur reste plein écran et non tourné ; seuls ses composants sont tournés.
        RectTransform hud = CreateRect("HUD", canvas);
        Stretch(hud);

        TextMeshProUGUI scoreText = CreateText("ScoreText", hud, "SCORE : 0", 56, Color.white, TextAlignmentOptions.TopRight);
        PlaceRotated(scoreText.rectTransform, FromTopRight(new Vector2(-40f, -140f), new Vector2(600f, 80f)), new Vector2(600f, 80f));
        Undo.AddComponent<ScoreUI>(scoreText.gameObject);

        TextMeshProUGUI livesText = CreateText("LivesText", hud, "VIES : 3", 48, Color.white, TextAlignmentOptions.TopLeft);
        PlaceRotated(livesText.rectTransform, FromTopLeft(new Vector2(40f, -30f), new Vector2(450f, 70f)), new Vector2(450f, 70f));

        RectTransform livesIcons = CreateRect("LivesIcons", hud);
        PlaceRotated(livesIcons, FromTopLeft(new Vector2(40f, -105f), new Vector2(300f, 60f)), new Vector2(300f, 60f));
        HorizontalLayoutGroup layout = Undo.AddComponent<HorizontalLayoutGroup>(livesIcons.gameObject);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        RectTransform lifeIcon = CreateRect("LifeIcon_Template", livesIcons);
        lifeIcon.sizeDelta = new Vector2(50f, 50f);
        Image lifeImage = Undo.AddComponent<Image>(lifeIcon.gameObject);
        lifeImage.sprite = knobSprite;
        lifeImage.color = new Color(0.82f, 0.84f, 0.88f, 1f);
        lifeImage.raycastTarget = false;

        Button pauseButton = CreateButton("PauseButton", hud, "II", new Vector2(90f, 90f));
        PlaceRotated((RectTransform)pauseButton.transform, FromTopRight(new Vector2(-40f, -30f), new Vector2(90f, 90f)), new Vector2(90f, 90f));

        // ---------------- Menu de départ ----------------
        GameObject startPanel = CreatePanel("StartPanel", canvas, new Color(0f, 0f, 0f, 0.65f));
        TextMeshProUGUI startTitle = CreateText("Title", startPanel.transform, "PINBALL", 120, Color.white, TextAlignmentOptions.Center);
        PlaceRotated(startTitle.rectTransform, new Vector2(0f, 200f), new Vector2(1000f, 160f));
        Button startButton = CreateButton("StartButton", startPanel.transform, "JOUER", new Vector2(380f, 100f));
        PlaceRotated((RectTransform)startButton.transform, new Vector2(0f, -40f), new Vector2(380f, 100f));

        // ---------------- Menu de pause ----------------
        GameObject pausePanel = CreatePanel("PausePanel", canvas, new Color(0f, 0f, 0f, 0.75f));
        TextMeshProUGUI pauseTitle = CreateText("Title", pausePanel.transform, "PAUSE", 110, Color.white, TextAlignmentOptions.Center);
        PlaceRotated(pauseTitle.rectTransform, new Vector2(0f, 200f), new Vector2(1000f, 150f));
        Button resumeButton = CreateButton("ResumeButton", pausePanel.transform, "REPRENDRE", new Vector2(380f, 100f));
        PlaceRotated((RectTransform)resumeButton.transform, new Vector2(0f, 30f), new Vector2(380f, 100f));
        Button pauseRestartButton = CreateButton("RestartButton", pausePanel.transform, "RECOMMENCER", new Vector2(380f, 100f));
        PlaceRotated((RectTransform)pauseRestartButton.transform, new Vector2(0f, -100f), new Vector2(380f, 100f));

        // ---------------- Panneau Game Over ----------------
        GameObject gameOverPanel = CreatePanel("GameOverPanel", canvas, new Color(0f, 0f, 0f, 0.8f));
        TextMeshProUGUI gameOverTitle = CreateText("GameOverTitle", gameOverPanel.transform, "GAME OVER", 130, new Color(1f, 0.3f, 0.3f, 1f), TextAlignmentOptions.Center);
        PlaceRotated(gameOverTitle.rectTransform, new Vector2(0f, 230f), new Vector2(1000f, 170f));
        TextMeshProUGUI finalScoreText = CreateText("FinalScoreText", gameOverPanel.transform, "SCORE FINAL : 0", 60, Color.white, TextAlignmentOptions.Center);
        PlaceRotated(finalScoreText.rectTransform, new Vector2(0f, 80f), new Vector2(1000f, 90f));
        Button gameOverRestart = CreateButton("RestartButton", gameOverPanel.transform, "RECOMMENCER", new Vector2(420f, 100f));
        PlaceRotated((RectTransform)gameOverRestart.transform, new Vector2(0f, -80f), new Vector2(420f, 100f));
        Button mainMenuButton = CreateButton("MainMenuButton", gameOverPanel.transform, "MENU PRINCIPAL", new Vector2(420f, 100f));
        PlaceRotated((RectTransform)mainMenuButton.transform, new Vector2(0f, -210f), new Vector2(420f, 100f));

        // ---------------- Objets de gestion ----------------
        ScoreManager scoreManager = FindOrCreate<ScoreManager>("ScoreManager");
        LifeManager lifeManager = FindOrCreate<LifeManager>("LifeManager");
        GameOverManager gameOverManager = FindOrCreate<GameOverManager>("GameOverManager");
        GameManager gameManager = FindOrCreate<GameManager>("GameManager");

        // Balle
        Rigidbody ball = FindBall();

        // Scripts qui ne doivent fonctionner qu’à l’état Playing
        List<Object> gameplay = new List<Object>();
        foreach (MonoBehaviour mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            string typeName = mb.GetType().Name;
            if (typeName == "BallLauncher" || typeName == "FlipperController" || typeName == "NewMonoBehaviourScript")
                gameplay.Add(mb);
        }

        // ---------------- Liaisons de l’Inspector ----------------
        SetRef(lifeManager, "ball", ball);
        SetRef(lifeManager, "livesText", livesText);
        SetRef(lifeManager, "lifeIconTemplate", lifeIcon.gameObject);

        SetRef(gameOverManager, "gameOverPanel", gameOverPanel);
        SetRef(gameOverManager, "titleText", gameOverTitle);
        SetRef(gameOverManager, "finalScoreText", finalScoreText);
        SetRef(gameOverManager, "restartButton", gameOverRestart);
        SetRef(gameOverManager, "mainMenuButton", mainMenuButton);

        SetRef(gameManager, "lifeManager", lifeManager);
        SetRef(gameManager, "scoreManager", scoreManager);
        SetRef(gameManager, "gameOverManager", gameOverManager);
        SetRefArray(gameManager, "gameplayBehaviours", gameplay);
        SetRef(gameManager, "startPanel", startPanel);
        SetRef(gameManager, "startButton", startButton);
        SetRef(gameManager, "pausePanel", pausePanel);
        SetRef(gameManager, "resumeButton", resumeButton);
        SetRef(gameManager, "pauseRestartButton", pauseRestartButton);
        SetRef(gameManager, "pauseButton", pauseButton);

        // DeathZone
        DeathZone deathZone = Object.FindFirstObjectByType<DeathZone>();
        if (deathZone == null)
        {
            GameObject dz = new GameObject("DeathZone");
            Undo.RegisterCreatedObjectUndo(dz, "Créer l’interface Pinball");

            if (SceneView.lastActiveSceneView != null)
                dz.transform.position = SceneView.lastActiveSceneView.pivot;

            BoxCollider box = Undo.AddComponent<BoxCollider>(dz);
            box.isTrigger = true;
            box.size = new Vector3(10f, 1f, 2f);

            deathZone = Undo.AddComponent<DeathZone>(dz);
        }
        SetRef(deathZone, "lifeManager", lifeManager);

        // ---------------- Fin ----------------
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = gameManager.gameObject;

        Debug.Log(
            "[Pinball UI] Terminé (composants tournés de " + Mathf.Abs(RotationZ) + "°).\n" +
            "- Balle : " + (ball != null ? ball.name : "INTROUVABLE (à assigner dans le LifeManager)") + "\n" +
            "- Scripts de jeu reliés au GameManager : " + gameplay.Count + "\n" +
            "- Reste à faire : déplacer la DeathZone sous les palettes et (facultatif) assigner l’action de pause dans le GameManager.\n" +
            "- Vérifiez dans la Game view réglée sur 16:9 (1920×1080).");
    }

    // ------------------------------------------------------------------
    // Scène
    // ------------------------------------------------------------------

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            GameObject go = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(go, "Créer l’interface Pinball");
            eventSystem = Undo.AddComponent<EventSystem>(go);
        }

        // Le projet utilise l’Input System : l’ancien module ne fonctionne pas.
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
            Undo.DestroyObjectImmediate(oldModule);

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
    }

    private static Transform CreateCanvas()
    {
        GameObject go = new GameObject("PinballCanvas", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Créer l’interface Pinball");

        Canvas canvas = Undo.AddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = Undo.AddComponent<CanvasScaler>(go);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Undo.AddComponent<GraphicRaycaster>(go);

        return go.transform;
    }

    private static T FindOrCreate<T>(string objectName) where T : Component
    {
        T existing = Object.FindFirstObjectByType<T>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(go, "Créer l’interface Pinball");
        return Undo.AddComponent<T>(go);
    }

    private static Rigidbody FindBall()
    {
        try
        {
            foreach (GameObject candidate in GameObject.FindGameObjectsWithTag("Ball"))
            {
                Rigidbody body = candidate.GetComponent<Rigidbody>();
                if (body != null)
                    return body;
            }
        }
        catch (UnityException)
        {
            // Le tag « Ball » n’existe pas.
        }

        return null;
    }

    // ------------------------------------------------------------------
    // Construction de l’interface
    // ------------------------------------------------------------------

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Créer l’interface Pinball");
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // Centre d’un élément ancré en haut à gauche (point de vue du joueur, en portrait).
    // offset : décalage depuis le coin (y négatif = vers le bas).
    private static Vector2 FromTopLeft(Vector2 offset, Vector2 size)
    {
        return new Vector2(
            -ReaderSize.x * 0.5f + offset.x + size.x * 0.5f,
            ReaderSize.y * 0.5f + offset.y - size.y * 0.5f);
    }

    // Centre d’un élément ancré en haut à droite (point de vue du joueur, en portrait).
    private static Vector2 FromTopRight(Vector2 offset, Vector2 size)
    {
        return new Vector2(
            ReaderSize.x * 0.5f + offset.x - size.x * 0.5f,
            ReaderSize.y * 0.5f + offset.y - size.y * 0.5f);
    }

    // Place un composant au centre de l’écran + position, et le tourne sur lui-même.
    // readerPosition : position du centre du composant du point de vue du joueur (avant rotation).
    // size : taille du composant avant rotation (largeur de lecture, hauteur de lecture).
    private static void PlaceRotated(RectTransform rect, Vector2 readerPosition, Vector2 size)
    {
        // La position suit la même rotation que le composant, pour que la mise en page reste cohérente.
        Vector3 rotated = Quaternion.Euler(0f, 0f, RotationZ) * new Vector3(readerPosition.x, readerPosition.y, 0f);

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f); // rotation autour du centre
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(Mathf.Round(rotated.x), Mathf.Round(rotated.y));
        rect.localRotation = Quaternion.Euler(0f, 0f, RotationZ);
    }

    private static GameObject CreatePanel(string objectName, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(objectName, parent);
        Stretch(rect);

        Image image = Undo.AddComponent<Image>(rect.gameObject);
        image.color = color;
        image.raycastTarget = true; // bloque les clics sur ce qui se trouve dessous

        return rect.gameObject;
    }

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string content,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        RectTransform rect = CreateRect(objectName, parent);

        TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        return text;
    }

    private static Button CreateButton(string objectName, Transform parent, string label, Vector2 size)
    {
        RectTransform rect = CreateRect(objectName, parent);
        rect.sizeDelta = size;

        Image image = Undo.AddComponent<Image>(rect.gameObject);
        image.sprite = uiSprite;
        image.type = Image.Type.Sliced;
        image.color = AccentColor;

        Button button = Undo.AddComponent<Button>(rect.gameObject);
        button.targetGraphic = image;

        // Le texte du bouton est enfant du bouton : il tourne avec lui.
        TextMeshProUGUI text = CreateText("Label", rect, label, 44, Color.white, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);

        return button;
    }

    // ------------------------------------------------------------------
    // Liaisons de l’Inspector
    // ------------------------------------------------------------------

    private static void SetRef(Object target, string field, Object value)
    {
        if (target == null)
            return;

        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);

        if (property == null)
        {
            Debug.LogWarning("[Pinball UI] Champ « " + field + " » introuvable dans " + target.GetType().Name);
            return;
        }

        property.objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    private static void SetRefArray(Object target, string field, List<Object> values)
    {
        if (target == null)
            return;

        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(field);

        if (property == null)
        {
            Debug.LogWarning("[Pinball UI] Champ « " + field + " » introuvable dans " + target.GetType().Name);
            return;
        }

        property.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

        so.ApplyModifiedProperties();
    }
}
#endif