using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Editable cabinet dressing around the existing physical playfield.</summary>
public class VosgesCabinet : MonoBehaviour
{
    private readonly List<UnityEngine.Object> ownedResources = new List<UnityEngine.Object>();
    private const float CenterZ = -4.04f;
    private const float HalfWidth = 6.25f;
    private const float HalfLength = 10.41f;
    [SerializeField, HideInInspector] private int layoutVersion;
    private const int CurrentLayoutVersion = 2;

    public static bool EnsureForScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            VosgesCabinet cabinet = root.GetComponent<VosgesCabinet>();
            if (cabinet == null) continue;
            if (cabinet.layoutVersion != CurrentLayoutVersion)
            {
                for (int i = cabinet.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject child = cabinet.transform.GetChild(i).gameObject;
                    child.SetActive(false);
                    if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
                }
                cabinet.Build();
                HideLegacyOutlines(scene);
                return true;
            }
            return HideLegacyOutlines(scene);
        }

        Transform table = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == "BaseLvl_1") table = item;
        if (table == null) return false;

        var go = new GameObject("Vosges - Pinball cabinet", typeof(VosgesCabinet));
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.SetPositionAndRotation(table.position, table.rotation);
        go.GetComponent<VosgesCabinet>().Build();
        HideLegacyOutlines(scene);
        return true;
    }

    private void Build()
    {
        Material wood = Finish("Walnut cabinet", new Color32(65, 68, 70, 255), .05f, .28f);
        Material forest = Finish("Forest green enamel", new Color32(14, 48, 51, 255), .25f, .5f);
        Material steel = Finish("Brushed steel", new Color32(185, 185, 185, 255), .8f, .6f);
        Material black = Finish("Rubber and seams", new Color32(10, 17, 21, 255), 0, .25f);
        Material red = Finish("Piste red inlay", new Color32(172, 39, 35, 255), .2f, .5f);
        Material snow = Finish("Playfield snow", new Color32(237, 237, 237, 255), .05f, .2f);
        Material gold = Finish("Champagne details", new Color32(197, 204, 207, 255), .65f, .5f);

        Box("Fitted snow playfield", new Vector3(0, -.055f, -3.715f), new Vector3(12.5f, .1f, 20.33f), snow);
        Rim("Deep walnut cabinet", HalfWidth, HalfLength, .72f, .65f, -1.25f, .60f, wood);
        Rim("Lower forest green band", HalfWidth + .02f, HalfLength + .02f, .72f, .67f, -1.03f, -.48f, forest);
        Rim("Lower metal trim", HalfWidth + .70f, HalfLength + .70f, .045f, 1.35f, -1.08f, -1.02f, steel);
        Rim("Inner rubber liner", HalfWidth - .035f, HalfLength - .035f, .08f, .615f, .025f, .60f, black);
        Rim("Brushed steel glass surround", HalfWidth, HalfLength, .70f, .65f, .60f, .70f, steel);
        Rim("Red perimeter inlay", HalfWidth + .28f, HalfLength + .28f, .035f, .93f, .701f, .716f, red);
        Rim("Outer polished edge", HalfWidth + .66f, HalfLength + .66f, .065f, 1.31f, .70f, .77f, steel);

        // The apron covers the drain visually; the existing death triggers keep working below it.
        Box("Drain shadow", new Vector3(0, -.12f, -14.5f), new Vector3(12.35f, .02f, 1.2f), black);
        Box("Front apron", new Vector3(0, -.08f, -14.5f), new Vector3(12.34f, .12f, 1.05f), forest);
        Box("Apron rear metal seam", new Vector3(0, -.015f, -13.99f), new Vector3(12.2f, .018f, .055f), steel);
        Label("Apron crest", "V O S G E S", new Vector3(-.15f, .005f, -14.45f), new Vector2(.9f, 5.8f), 3.3f, new Color32(229, 233, 234, 255));
        Label("Apron signature", "SKI  •  PINBALL", new Vector3(1.0f, .005f, -14.45f), new Vector2(.9f, 2.4f), 1.5f, new Color32(216, 236, 239, 255));
        Box("Front lockdown bar", new Vector3(0, -.16f, -14.91f), new Vector3(13.85f, .23f, .30f), steel);
        Box("Lockdown rubber seam", new Vector3(0, -.29f, -14.91f), new Vector3(13.9f, .055f, .34f), black);

        foreach (int side in new[] { -1, 1 })
        {
            // Small repeated hardware reads as a continuous, manufactured metal frame.
            for (int i = 0; i < 9; i++)
            {
                float z = -13.45f + i * 2.45f;
                Cylinder("Recessed frame screw", new Vector3(side * 6.78f, FrontHeight(.712f, z), z), new Vector3(.105f, .012f, .105f), black);
                Box("Screw slot", new Vector3(side * 6.78f, FrontHeight(.727f, z), z), new Vector3(.055f, .006f, .012f), steel);
            }
            for (int i = 0; i < 5; i++)
                Box("Walnut grain", new Vector3(side * 6.977f, -.4f + i * .1f, CenterZ), new Vector3(.009f, .012f, 20.9f), black);
            var bezel = Cylinder("Flipper button bezel", new Vector3(side * 7.0f, -.3f, -12.6f), new Vector3(.48f, .075f, .48f), steel);
            bezel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            var button = Cylinder("Flipper button", new Vector3(side * 7.09f, -.3f, -12.6f), new Vector3(.33f, .055f, .33f), red);
            button.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        // Plunger and coin hardware finish the front silhouette from the fixed camera.
        Box("Coin door bezel", new Vector3(0, -1.0f, -15.19f), new Vector3(2.9f, .95f, .09f), steel);
        Box("Coin door", new Vector3(0, -1.0f, -15.25f), new Vector3(2.68f, .76f, .055f), black);
        Box("Coin slot", new Vector3(.62f, -.78f, -15.29f), new Vector3(.42f, .075f, .025f), gold);
        Box("Service door handle", new Vector3(-.85f, -.97f, -15.30f), new Vector3(.12f, .28f, .05f), steel);
        var plunger = Cylinder("Ball plunger shaft", new Vector3(5.6f, -.65f, -15.34f), new Vector3(.10f, .20f, .10f), steel);
        plunger.transform.localRotation = Quaternion.Euler(90, 0, 0);
        var knob = Cylinder("Ball plunger knob", new Vector3(5.6f, -.65f, -15.55f), new Vector3(.42f, .08f, .42f), forest);
        knob.transform.localRotation = Quaternion.Euler(90, 0, 0);
        Box("Rear walnut end cap", new Vector3(0, .28f, 6.89f), new Vector3(13.2f, .85f, .20f), wood);
        Box("Rear brass detail", new Vector3(0, .73f, 6.89f), new Vector3(13.2f, .055f, .21f), gold);
        layoutVersion = CurrentLayoutVersion;
    }

    private void Rim(string title, float halfX, float halfZ, float width, float radius, float bottom, float top, Material material)
    {
        const int cornerSteps = 12;
        int count = 4 * (cornerSteps + 1);
        var vertices = new Vector3[count * 4];
        var triangles = new List<int>(count * 24);
        int row = 0;
        for (int corner = 0; corner < 4; corner++)
        {
            float signX = corner == 0 || corner == 3 ? 1 : -1;
            float signZ = corner < 2 ? 1 : -1;
            Vector3 center = new Vector3(signX * (halfX - radius), 0, CenterZ + signZ * (halfZ - radius));
            for (int step = 0; step <= cornerSteps; step++, row++)
            {
                float angle = (corner * 90 + step * 90f / cornerSteps) * Mathf.Deg2Rad;
                Vector3 radial = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                int index = row * 4;
                Vector3 inner = center + radial * radius;
                Vector3 outer = center + radial * (radius + width);
                vertices[index] = inner + Vector3.up * FrontHeight(bottom, inner.z);
                vertices[index + 1] = outer + Vector3.up * FrontHeight(bottom, outer.z);
                vertices[index + 2] = outer + Vector3.up * FrontHeight(top, outer.z);
                vertices[index + 3] = inner + Vector3.up * FrontHeight(top, inner.z);
                for (int side = 0; side < 4; side++)
                {
                    int a = index + side, b = index + (side + 1) % 4;
                    int nextA = ((row + 1) % count) * 4 + side;
                    int nextB = ((row + 1) % count) * 4 + (side + 1) % 4;
                    triangles.Add(a); triangles.Add(b); triangles.Add(nextB);
                    triangles.Add(a); triangles.Add(nextB); triangles.Add(nextA);
                }
            }
        }
        var mesh = new Mesh { name = title };
        mesh.vertices = vertices;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            const string folder = "Assets/Materials/VosgesCabinet";
            EnsureAssetFolder();
            string path = folder + "/" + title.Replace(" ", "_") + "_View_v2.asset";
            Mesh saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved != null) { DestroyImmediate(mesh); mesh = saved; }
            else UnityEditor.AssetDatabase.CreateAsset(mesh, path);
        }
        else
#endif
            ownedResources.Add(mesh);
        var go = new GameObject(title, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private GameObject Box(string title, Vector3 position, Vector3 size, Material material)
    { return Shape(title, PrimitiveType.Cube, position, size, material); }

    private static float FrontHeight(float height, float z)
    {
        // Lower the front lip below the palettes instead of masking them in this camera view.
        return height - .8f * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-13.1f, -14.45f, z));
    }

    private GameObject Cylinder(string title, Vector3 position, Vector3 size, Material material)
    { return Shape(title, PrimitiveType.Cylinder, position, size, material); }

    private GameObject Shape(string title, PrimitiveType primitive, Vector3 position, Vector3 size, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = title;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        Collider collider = go.GetComponent<Collider>();
        collider.enabled = false;
        if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider);
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private void Label(string title, string caption, Vector3 position, Vector2 size, float fontSize, Color color)
    {
        var go = new GameObject(title, typeof(TextMeshPro));
        go.transform.SetParent(transform, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(90, 90, 0);
        var text = go.GetComponent<TextMeshPro>();
        text.text = caption;
        text.color = color;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.rectTransform.sizeDelta = size;
        text.enableAutoSizing = true;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.fontSizeMin = 1;
        text.fontSizeMax = fontSize;
    }

    private Material Finish(string title, Color color, float metallic, float smoothness)
    {
#if UNITY_EDITOR
        string path = "Assets/Materials/VosgesCabinet/" + title.Replace(" ", "_") + ".mat";
        if (!Application.isPlaying)
        {
            Material saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (saved != null) return saved;
        }
#endif
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = title, color = color };
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureAssetFolder();
            UnityEditor.AssetDatabase.CreateAsset(material, path);
            return material;
        }
#endif
        ownedResources.Add(material);
        return material;
    }

#if UNITY_EDITOR
    private static void EnsureAssetFolder()
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Materials/VosgesCabinet"))
            UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesCabinet");
    }
#endif

    private static bool HideLegacyOutlines(Scene scene)
    {
        bool changed = false;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<VosgesCabinet>() != null) continue;
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                string title = item.name;
                if (title == "BaseLvl_1" || title == "BordBas" || title == "BordHaut"
                    || title == "BordGauche" || title == "BordDroit" || title == "Verre protecteur"
                    || title == "Timber cabinet rail" || title == "Snow on rail" || title == "Rail lamp"
                    || title == "MurArriveeGauche" || title == "MurArriveeDroit")
                {
                    Renderer renderer = item.GetComponent<Renderer>();
                    if (renderer != null && renderer.enabled) { renderer.enabled = false; changed = true; }
                }
            }
        }
        return changed;
    }

    private void OnDestroy()
    {
        foreach (UnityEngine.Object resource in ownedResources)
            if (resource != null)
            { if (Application.isPlaying) Destroy(resource); else DestroyImmediate(resource); }
    }
}
