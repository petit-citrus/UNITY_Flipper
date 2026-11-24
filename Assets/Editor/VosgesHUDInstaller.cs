using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Installe un tableau de scores lisible dans la zone opposée aux palettes.
/// </summary>
public static class VosgesHUDInstaller
{
    private const string RootName = "Vosges_HUD_2026";
    private static readonly Color Wood = new Color(0.13f, 0.19f, 0.18f, 0.97f);
    private static readonly Color Snow = new Color(0.94f, 0.96f, 0.92f, 1f);
    private static readonly Color Gold = new Color(0.93f, 0.74f, 0.42f, 1f);

    [MenuItem("Tools/Ski Pinball/Installer le HUD Vosges (côté opposé aux flippers)")]
    public static void Install()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(root, "Installer le HUD Vosges");
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        root.GetComponent<GraphicRaycaster>().enabled = false;

        // Dans la vue Game actuelle, l'extrémité haute de la piste est à droite.
        // Un groupe pivoté garde un tableau horizontal dans le repère de la piste.
        RectTransform panel = Rect(root.transform, "Marqueur_Vosges", new Vector2(1040, 280),
            new Vector2(1f, 0.5f), new Vector2(-140, 0), -90f);
        Image bg = panel.gameObject.AddComponent<Image>();
        bg.color = Color.white;
        bg.raycastTarget = false;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Scripts/UI/VosgesHUD/VosgesWood.png");
        // Un seul composant graphique par objet : la texture est portée par un enfant.
        bg.enabled = false;
        var art = Rect(panel, "Bois et neige", new Vector2(1040,280), new Vector2(.5f,.5f), Vector2.zero);
        var raw = art.gameObject.AddComponent<RawImage>(); raw.texture = texture;
        raw.color = texture != null ? Color.white : new Color(0.35f, 0.19f, 0.09f, 1f);
        raw.raycastTarget = false;
        if (texture == null) Debug.LogWarning("VosgesWood.png introuvable : fond brun de secours. Vérifiez Assets/Scripts/UI/VosgesHUD/VosgesWood.png.");
        Label(panel, "Titre", "VOSGES SKI PINBALL", new Vector2(0, 82), new Vector2(850, 56), 48, Snow);
        VosgesHUDController controller = root.AddComponent<VosgesHUDController>();
        string[] names = {"SCORE", "RECORD", "NIVEAU", "VIES"};
        TMP_Text[] values = new TMP_Text[4];
        for (int i=0;i<4;i++) {
            float x = -350 + 230*i;
            Label(panel, names[i], names[i], new Vector2(x, 22), new Vector2(210,35), 27, Gold);
            values[i] = Label(panel, names[i]+"Valeur", "0", new Vector2(x,-19), new Vector2(210,55), 48, Snow);
        }
        controller.scoreText=values[0]; controller.bestText=values[1]; controller.levelText=values[2]; controller.livesText=values[3];
        controller.statusText=Label(panel,"Etat","PRÊT",new Vector2(0,-65),new Vector2(950,42),34,Gold);
        controller.hintText=Label(panel,"Consigne","APPUYEZ SUR UN FLIPPER",new Vector2(0,-96),new Vector2(960,38),26,Snow);
        controller.animatedStatus = controller.statusText.rectTransform;
        controller.rankingText = Label(panel,"Classement","TOP 5",new Vector2(0,-124),new Vector2(1000,25),22,Snow);
        var bar = Rect(panel,"Progression transition",new Vector2(850,8),new Vector2(.5f,.5f),new Vector2(0,-124));
        controller.progress = bar.gameObject.AddComponent<Image>();
        controller.progress.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        controller.progress.type=Image.Type.Filled; controller.progress.fillMethod=Image.FillMethod.Horizontal; controller.progress.color=Gold; controller.progress.raycastTarget=false; bar.gameObject.SetActive(false);
        // Réserve une zone distincte sans modifier la rotation de la caméra.
        var cam = Camera.main;
        if(cam != null) { Undo.RecordObject(cam,"Réserver le marqueur"); cam.rect = new Rect(0,0,1640f/1920f,1);
            // Repart de la projection originale, sans cumul lors des réinstallations.
            cam.ResetAspect();
            cam.ResetProjectionMatrix();
            float ratio = 1640f/1920f;
            if (cam.orthographic) cam.orthographicSize = 5f / ratio;
            else cam.fieldOfView = 2f * Mathf.Atan(Mathf.Tan(30f * Mathf.Deg2Rad) / ratio) * Mathf.Rad2Deg;
            // Petite marge supplémentaire pour les palettes.
            if (!cam.orthographic) cam.fieldOfView += 3f; }
        // Masque uniquement le rendu des anciens Canvas, sans désactiver leurs scripts.
        foreach(var old in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None)) {
            if(old == canvas || (old.gameObject.name != "PinballCanvas" && old.gameObject.name != "Canvas")) continue;
            Undo.RecordObject(old,"Masquer ancienne interface"); old.enabled=false;
            foreach(var ray in old.GetComponentsInChildren<GraphicRaycaster>(true)) { Undo.RecordObject(ray,"Masquer commandes UI"); ray.enabled=false; }
        }
        // La copie de test doit pouvoir déclencher la transition elle aussi.
        var transition = Object.FindFirstObjectByType<LevelTransitionUI>(FindObjectsInactive.Include);
        if (transition != null && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.StartsWith("Level_1_recup")) {
            Undo.RecordObject(transition,"Source transition HUD");
            var so = new SerializedObject(transition); var source = so.FindProperty("sourceScene");
            if(source != null) { source.stringValue=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; so.ApplyModifiedProperties(); }
        }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        Debug.Log("HUD Vosges créé à droite dans Game. Vérifiez en 16:9 avant de masquer l'ancienne interface.");
    }

    [MenuItem("Tools/Ski Pinball/Supprimer le HUD Vosges (Undo possible)")]
    public static void Remove()
    {
        GameObject root = GameObject.Find(RootName);
        if (root == null) return;
        Undo.DestroyObjectImmediate(root);
        Debug.LogWarning("Utilisez Undo pour restaurer également la caméra et les anciens Canvas, ou restaurez la scène sauvegardée.");
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 anchor,
        Vector2 position, float rotation = 0)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        rt.localRotation = Quaternion.Euler(0, 0, rotation);
        return rt;
    }

    private static TMP_Text Label(Transform parent, string name, string value, Vector2 position,
        Vector2 size, int fontSize, Color color)
    {
        RectTransform rt = Rect(parent, name, size, new Vector2(0.5f, 0.5f), position);
        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableAutoSizing = true;
        text.fontSizeMin = fontSize * 0.65f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static void ThinRule(Transform parent, Vector2 position, float width)
    {
        RectTransform rt = Rect(parent, "Filet_doré", new Vector2(width, 2), new Vector2(0.5f, 0.5f), position);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = Gold;
        image.raycastTarget = false;
    }
}
