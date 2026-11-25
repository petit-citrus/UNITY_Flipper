using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Additive, collision-free winter dressing. Never changes the camera or game geometry.</summary>
public class VosgesScenery : MonoBehaviour
{
    [SerializeField] private GameObject pineTree;
    [SerializeField] private GameObject chalet;
    [SerializeField] private GameObject snowman;
    [SerializeField] private GameObject lampPost;

    [SerializeField, HideInInspector] private Transform decor;
    private readonly List<Material> materials = new List<Material>();
    private Material snow, wood, darkWood, red, ice;
    [SerializeField, HideInInspector] private Material glow;
    private bool runtimeGenerated;
    private ScoreManager scoreManager;
    private float flashTime = -10;

    private void Start()
    {
        VosgesLevelTwoCourse.EnsureForScene(gameObject.scene);
        VosgesGameplaySetup.EnsureForScene(gameObject.scene);
        VosgesLevelOneObstacles.EnsureForScene(gameObject.scene);
        VosgesCornerSafety.EnsureForScene(gameObject.scene);
        EnsureScenery();
        VosgesCabinet.EnsureForScene(gameObject.scene);
        VosgesFlipperSkin.EnsureForScene(gameObject.scene);
        VosgesPlayfieldView.EnsureForScene(gameObject.scene);
        // Animate an instance during Play, keeping saved scene materials unchanged.
        if (decor != null && glow != null && !runtimeGenerated)
        {
            Material original = glow;
            glow = Instantiate(original);
            materials.Add(glow);
            foreach (Renderer renderer in decor.GetComponentsInChildren<Renderer>(true))
            {
                Material[] assigned = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < assigned.Length; i++)
                    if (assigned[i] == original) { assigned[i] = glow; changed = true; }
                if (changed) renderer.sharedMaterials = assigned;
            }
        }
        scoreManager = ScoreManager.Instance;
        if (scoreManager != null) scoreManager.onScoreAdded.AddListener(Flash);
    }

    public bool EnsureScenery()
    {
        if (decor != null) return RefreshTargetRings();
        Transform table = null;
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsSortMode.None))
            if (candidate.name == "BaseLvl_1" && candidate.gameObject.scene == gameObject.scene)
            { table = candidate; break; }
        if (table == null) return false;

        decor = new GameObject("Vosges • Winter resort").transform;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(decor.gameObject, gameObject.scene);
        runtimeGenerated = Application.isPlaying;
        decor.SetPositionAndRotation(table.position + table.up * .035f, table.rotation);
        // World-unit coordinates, without inheriting the stretched scale of the playfield.
        snow = Material("Fresh snow", new Color32(236, 236, 236, 255));
        wood = Material("Larch timber", new Color32(103, 98, 90, 255));
        darkWood = Material("Forest enamel", new Color32(24, 64, 66, 255));
        red = Material("Piste red", new Color32(193, 50, 49, 255));
        ice = Material("Glacier blue", new Color32(79, 163, 188, 255));
        glow = Material("Warm cabinet lamps", new Color32(228, 234, 236, 255), true);

        for (int side = -1; side <= 1; side += 2)
        {
            float edge = side * 6.65f;
            Piece("Timber cabinet rail", PrimitiveType.Cube, new Vector3(edge, .35f, 0), new Vector3(.24f, .65f, 31), darkWood);
            Piece("Snow on rail", PrimitiveType.Cube, new Vector3(edge, .71f, 0), new Vector3(.31f, .12f, 31), snow);
            for (int i = 0; i < 14; i++)
                Piece("Rail lamp", PrimitiveType.Sphere, new Vector3(edge - side * .16f, .56f, -14.2f + i * 2.15f), Vector3.one * .13f, glow);
            for (int i = 0; i < 6; i++)
            {
                float z = -12 + i * 5;
                float x = side * (7.7f + (i % 2) * .6f);
                Piece("Snowbank", PrimitiveType.Sphere, new Vector3(x, -.12f, z), new Vector3(3.1f, .65f, 5.2f), snow);
                Prop(pineTree, "Snow-covered fir", new Vector3(x, .08f, z), 2.1f + (i % 3) * .45f, i * 53);
                Fence(side * 7.1f, z + 1.5f);
            }
            for (int i = 0; i < 3; i++)
                Prop(lampPost, "Resort lantern", new Vector3(side * 7.0f, .1f, -8 + i * 9), 1.9f, side < 0 ? 90 : -90);
        }

        Prop(chalet, "Refuge des Vosges", new Vector3(-8.8f, .1f, 9), 3.1f, 90);
        Prop(chalet, "Chalet du lac", new Vector3(8.8f, .1f, -4), 2.5f, -90);
        Prop(snowman, "Welcome snowman", new Vector3(-7.3f, .1f, -9), 1.3f, 110);
        Prop(snowman, "Summit snowman", new Vector3(7.5f, .1f, 12), 1.4f, -80);
        Sign(new Vector3(-5.7f, .05f, 10.8f), "LA BRESSE", red);
        Sign(new Vector3(5.8f, .05f, 5.8f), "GÉRARDMER", darkWood);
        Sign(new Vector3(-5.7f, .05f, -5.2f), "LE HOHNECK", ice);
        // Piste edge markers remain close to the borders, clear of the ball lanes.
        for (int i = 0; i < 10; i++)
        {
            float z = -10 + i * 2.5f;
            foreach (int side in new[] { -1, 1 })
            {
                Piece("Piste marker", PrimitiveType.Cylinder, new Vector3(side * 6.05f, .3f, z), new Vector3(.06f, .3f, .06f), red);
                Piece("Marker snow cap", PrimitiveType.Sphere, new Vector3(side * 6.05f, .63f, z), Vector3.one * .11f, snow);
            }
        }
        RefreshTargetRings();
        return true;
    }

    private bool RefreshTargetRings()
    {
        var targets = new List<TargetController>(FindObjectsByType<TargetController>(FindObjectsSortMode.None));
        targets.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        var positions = new List<Vector4>();
        foreach (TargetController target in targets)
        {
            if (!target.isActiveAndEnabled || target.gameObject.scene != gameObject.scene) continue;
            Collider shape = target.GetComponent<Collider>();
            Vector3 center = shape != null ? shape.bounds.center : target.transform.position;
            Vector3 local = decor.InverseTransformPoint(center);
            if (Mathf.Abs(local.x) > 5.6f || Mathf.Abs(local.z) > 14.5f) continue;
            float radius = shape != null ? Mathf.Clamp(Mathf.Max(shape.bounds.extents.x, shape.bounds.extents.z), .5f, 1.5f) : .75f;
            positions.Add(new Vector4(local.x, .04f, local.z, radius + .15f));
        }
        var rings = new List<LineRenderer>();
        foreach (LineRenderer line in decor.GetComponentsInChildren<LineRenderer>(true))
            if (line.name == "Target illuminated insert") rings.Add(line);
        bool changed = false;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector4 point = positions[i];
            Vector3 center = new Vector3(point.x, point.y, point.z);
            LineRenderer line;
            if (i >= rings.Count) { line = Ring(center, point.w); changed = true; }
            else line = rings[i];
            for (int vertex = 0; vertex < line.positionCount; vertex++)
            {
                float angle = vertex * Mathf.PI * 2 / line.positionCount;
                Vector3 position = center + new Vector3(Mathf.Cos(angle) * point.w, 0, Mathf.Sin(angle) * point.w);
                if ((line.GetPosition(vertex) - position).sqrMagnitude < .000001f) continue;
                line.SetPosition(vertex, position);
                changed = true;
            }
        }
        for (int i = positions.Count; i < rings.Count; i++)
        {
            rings[i].gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(rings[i].gameObject); else DestroyImmediate(rings[i].gameObject);
            changed = true;
        }
        return changed;
    }

    private void Update()
    {
        if (glow == null) return;
        float pulse = Mathf.Clamp01(1 - (Time.unscaledTime - flashTime) * 3);
        glow.SetColor("_EmissionColor", new Color(.9f, .95f, 1f) * (.6f + pulse * .8f));
    }

    private void Flash(int points) { if (points > 0) flashTime = Time.unscaledTime; }

    private void Fence(float x, float z)
    {
        for (int i = -1; i <= 1; i++)
            Piece("Fence post", PrimitiveType.Cube, new Vector3(x, .48f, z + i * .85f), new Vector3(.12f, .95f, .12f), wood);
        foreach (float y in new[] { .35f, .72f })
            Piece("Fence beam", PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(.1f, .1f, 2), wood);
        Piece("Fence snow", PrimitiveType.Cube, new Vector3(x, .81f, z), new Vector3(.17f, .08f, 2.04f), snow);
    }

    private void Sign(Vector3 position, string title, Material enamel)
    {
        Piece("Trail sign post", PrimitiveType.Cube, position + Vector3.up * .6f, new Vector3(.1f, 1.2f, .1f), wood);
        // Signs lie parallel to the playfield so they stay legible from the fixed overhead view.
        Piece(title, PrimitiveType.Cube, position + Vector3.up * 1.2f, new Vector3(.65f, .12f, 1.9f), enamel);
        Piece("Trail sign snow", PrimitiveType.Cube, position + new Vector3(-.28f, 1.28f, 0), new Vector3(.1f, .08f, 1.94f), snow);
        var go = new GameObject("Trail name", typeof(TextMeshPro));
        go.transform.SetParent(decor, false);
        go.transform.localPosition = position + Vector3.up * 1.28f;
        go.transform.localRotation = Quaternion.Euler(90, 90, 0);
        var text = go.GetComponent<TextMeshPro>();
        text.text = title + "  >";
        text.fontSize = 1.5f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.rectTransform.sizeDelta = new Vector2(1.8f, .5f);
        text.enableAutoSizing = true;
        text.fontSizeMin = .5f;
        text.fontSizeMax = 1.5f;
    }

    private LineRenderer Ring(Vector3 center, float radius)
    {
        const int count = 40;
        var go = new GameObject("Target illuminated insert", typeof(LineRenderer));
        go.transform.SetParent(decor, false);
        var line = go.GetComponent<LineRenderer>();
        line.sharedMaterial = glow;
        line.useWorldSpace = false;
        line.loop = true;
        line.widthMultiplier = .055f;
        line.positionCount = count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        }
        return line;
    }

    private void Prop(GameObject prefab, string title, Vector3 position, float height, float yaw)
    {
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab, decor);
        instance.name = title;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        // Disable all physical components before the next physics step.
        foreach (Collider shape in instance.GetComponentsInChildren<Collider>(true)) shape.enabled = false;
        foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
        { body.isKinematic = true; body.detectCollisions = false; }
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        Bounds bounds = LocalBounds(renderers);
        instance.transform.localScale *= height / Mathf.Max(.01f, bounds.size.y);
        bounds = LocalBounds(renderers);
        // Anchor using renderer bounds, accommodating the source models' different origins.
        Vector3 foot = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        instance.transform.localPosition += position - foot;
    }

    private Bounds LocalBounds(Renderer[] renderers)
    {
        // Measure in the table frame so every model rests on the inclined snow surface.
        Bounds result = new Bounds(decor.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            Bounds bounds = renderer.localBounds;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 offset = Vector3.Scale(bounds.extents, new Vector3(
                    (corner & 1) == 0 ? -1 : 1,
                    (corner & 2) == 0 ? -1 : 1,
                    (corner & 4) == 0 ? -1 : 1));
                result.Encapsulate(decor.InverseTransformPoint(renderer.transform.TransformPoint(bounds.center + offset)));
            }
        }
        return result;
    }

    private GameObject Piece(string title, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = title;
        go.transform.SetParent(decor, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        Collider shape = go.GetComponent<Collider>();
        shape.enabled = false;
        if (Application.isPlaying) Destroy(shape);
        else DestroyImmediate(shape);
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private Material Material(string title, Color color, bool emissive = false)
    {
#if UNITY_EDITOR
        const string folder = "Assets/Materials/VosgesDecor";
        string path = folder + "/" + title.Replace(" ", "_") + ".mat";
        if (!Application.isPlaying)
        {
            Material existing = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
        }
#endif
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var material = new Material(shader) { name = title, color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", emissive ? .55f : .15f);
        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.5f);
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesDecor");
            UnityEditor.AssetDatabase.CreateAsset(material, path);
            return material;
        }
#endif
        materials.Add(material);
        return material;
    }

    private void OnDestroy()
    {
        if (scoreManager != null) scoreManager.onScoreAdded.RemoveListener(Flash);
        if (decor != null && runtimeGenerated) Destroy(decor.gameObject);
        foreach (Material material in materials) if (material != null) Destroy(material);
    }
}
