using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Affichage d'origine du flipper, dans la bande et l'orientation qui lui sont réservées.</summary>
public static class VosgesScoreboard
{
    private static readonly Color Ink = new Color32(9, 24, 33, 255);
    private static readonly Color Ice = new Color32(152, 225, 232, 255);
    private static readonly Color Amber = new Color32(255, 184, 73, 255);
    private static readonly Color Snow = new Color32(234, 245, 242, 255);

    public static bool Build(VosgesHUDController hud)
    {
        RectTransform previous = hud.transform.Find("Vosges_Cabinet") as RectTransform;
        Quaternion rotation = Quaternion.Euler(0, 0, -90);
        if (previous != null && (previous.sizeDelta - new Vector2(1040, 280)).sqrMagnitude < .01f
            && Quaternion.Angle(previous.localRotation, rotation) < .1f) return false;
        Sprite progressSprite = hud.progress != null ? hud.progress.sprite : null;
        if (previous != null)
        {
            previous.name = "Obsolete scoreboard";
            previous.gameObject.SetActive(false);
            if (Application.isPlaying) Object.Destroy(previous.gameObject);
            else Object.DestroyImmediate(previous.gameObject);
        }
        Transform old = hud.transform.Find("Marqueur_Vosges");
        if (old != null) old.gameObject.SetActive(false);
        RectTransform panel = Box(hud.transform, "Vosges_Cabinet", Vector2.zero, new Vector2(1040, 280), new Color32(75, 43, 30, 255));
        panel.anchorMin = panel.anchorMax = new Vector2(1, .5f);
        panel.anchoredPosition = new Vector2(-140, 0);
        panel.localRotation = rotation;
        Box(panel, "Brass surround", Vector2.zero, new Vector2(1020, 260), new Color32(181, 142, 83, 255));
        Box(panel, "Enamel face", Vector2.zero, new Vector2(1012, 252), Ink);
        Label(panel, "Massif", "M A S S I F   D E S   V O S G E S", new Vector2(0, 104), new Vector2(650, 23), 17, Ice);
        Label(panel, "Title", "VOSGES  /  SKI PINBALL", new Vector2(0, 73), new Vector2(730, 42), 32, Snow);
        Label(panel, "Edition", "WINTER CLUB     •     DEPUIS 2026", new Vector2(0, 46), new Vector2(650, 19), 13, Amber);
        Box(panel, "Top rule", new Vector2(0, 31), new Vector2(942, 2), new Color32(62, 94, 102, 255));
        hud.scoreText = Cell(panel, "SCORE", -331, 270, 52, Amber);
        hud.bestText = Cell(panel, "RECORD", -43, 270, 52, Ice);
        hud.levelText = Cell(panel, "NIVEAU", 205, 190, 48, Snow);
        hud.livesText = Cell(panel, "VIES", 409, 170, 48, Amber);
        hud.scoreText.text = "000000";
        hud.bestText.text = "000000";
        hud.levelText.text = hud.gameObject.scene.name == "Level_2_test" ? "02" : "01";
        hud.livesText.text = "3";
        hud.statusText = Label(panel, "Status", "PRÊT À JOUER", new Vector2(0, -64), new Vector2(936, 28), 23, Amber);
        hud.hintText = Label(panel, "Instructions", "APPUYEZ SUR UN FLIPPER", new Vector2(0, -90), new Vector2(944, 23), 16, Snow);
        hud.rankingText = Label(panel, "Records", "TOP 5", new Vector2(0, -115), new Vector2(920, 20), 15, Ice);
        hud.animatedStatus = hud.statusText.rectTransform;
        hud.progress = Box(panel, "Transition", new Vector2(0, -115), new Vector2(920, 5), Amber).GetComponent<Image>();
        hud.progress.sprite = progressSprite;
        hud.progress.type = Image.Type.Filled;
        hud.progress.fillMethod = Image.FillMethod.Horizontal;
        hud.progress.gameObject.SetActive(false);
        foreach (int x in new[] { -491, 491 })
            foreach (int y in new[] { -111, 111 })
            {
                Box(panel, "Screw", new Vector2(x, y), new Vector2(8, 8), new Color32(145, 163, 165, 255));
                Box(panel, "Screw slot", new Vector2(x, y), new Vector2(5, 2), Ink);
            }
        return true;
    }

    private static TMP_Text Cell(Transform parent, string caption, float x, float width, int size, Color color)
    {
        RectTransform cell = Box(parent, caption + " display", new Vector2(x, -12), new Vector2(width, 78), new Color32(3, 12, 18, 255));
        for (int row = 0; row < 13; row++)
            Box(cell, "Display grille", new Vector2(0, -35 + row * 6), new Vector2(width - 8, 1), new Color32(19, 36, 39, 255));
        Label(cell, "Label", caption, new Vector2(0, 25), new Vector2(width - 12, 18), 14, Ice);
        TMP_Text value = Label(cell, "Digits", "0", new Vector2(0, -9), new Vector2(width - 14, 53), size, color);
        value.characterSpacing = 5;
        return value;
    }

    private static RectTransform Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * .5f;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var graphic = go.GetComponent<Image>();
        graphic.color = color;
        graphic.raycastTarget = false;
        return rect;
    }

    private static TMP_Text Label(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = fontSize * .6f;
        label.fontSizeMax = fontSize;
        label.color = color;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }
}
