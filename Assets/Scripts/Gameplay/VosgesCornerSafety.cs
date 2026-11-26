using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Coins arrondis continus et guides à faible frottement, sans poches fermées dans les couloirs extérieurs.</summary>
public class VosgesCornerSafety : MonoBehaviour
{
    private readonly List<UnityEngine.Object> resources = new List<UnityEngine.Object>();
    private const string Folder = "Assets/Materials/VosgesGuides";
    [SerializeField, HideInInspector] private int guideVersion;
    private const int CurrentVersion = 4;

    public static bool EnsureForScene(Scene scene)
    {
        Transform board = null;
        GameManager manager = null;
        LifeManager lives = null;
        Rigidbody ball = null;
        VosgesCornerSafety existing = null;
        var items = new List<Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            items.AddRange(root.GetComponentsInChildren<Transform>(true));
            if (existing == null) existing = root.GetComponentInChildren<VosgesCornerSafety>(true);
            foreach (GameManager candidate in root.GetComponentsInChildren<GameManager>(true)) manager = candidate;
            foreach (LifeManager candidate in root.GetComponentsInChildren<LifeManager>(true)) lives = candidate;
            foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
                if (body.CompareTag("Ball")) ball = body;
        }
        foreach (Transform item in items) if (item.name == "BaseLvl_1") board = item;
        if (board == null || ball == null || manager == null) return false;
        bool changed = false;
        var release = ball.GetComponent<VosgesBallRelease>();
        if (release == null) { release = ball.gameObject.AddComponent<VosgesBallRelease>(); changed = true; }
        release.Configure(board, manager, lives);
        if (!ball.isKinematic && ball.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
        { ball.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; changed = true; }
        if (ball.interpolation != RigidbodyInterpolation.Interpolate)
        { ball.interpolation = RigidbodyInterpolation.Interpolate; changed = true; }
        if (existing != null && existing.guideVersion == CurrentVersion && existing.HasContainment()) return changed;
        VosgesCornerSafety safety = existing;
        if (safety == null)
        {
            var go = new GameObject("Vosges - Rounded physical guides", typeof(VosgesCornerSafety));
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetPositionAndRotation(board.position, board.rotation);
            safety = go.GetComponent<VosgesCornerSafety>();
        }
        else
            for (int i = safety.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = safety.transform.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
        PhysicsMaterial smooth = safety.Rolling();
        foreach (Transform item in items)
        {
            if (item == null) continue;
            foreach (DeathZone legacy in item.GetComponents<DeathZone>())
            {
                legacy.enabled = false;
                foreach (Collider collider in item.GetComponents<Collider>())
                    if (collider.isTrigger) collider.enabled = false;
            }
            if (item.name == "BordGauche" || item.name == "BordDroit" || item.name == "BordHaut")
                foreach (Collider collider in item.GetComponents<Collider>())
                    if (!collider.isTrigger) collider.enabled = false;
            if (item.name == "CoinHautGauche" || item.name == "CoinEntrer")
            {
                foreach (Collider collider in item.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
                foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            }
            if (item.name.StartsWith("Bord") || item.name == "MurArriveeGauche"
                || item.name == "MurArriveeDroit" || item.name == "MurPropulseurDroit")
                foreach (Collider collider in item.GetComponents<Collider>())
                    if (!collider.isTrigger) collider.sharedMaterial = smooth;
            if (item.name == "MurArriveeGauche" || item.name == "MurArriveeDroit")
            {
                BoxCollider collider = item.GetComponent<BoxCollider>();
                if (collider != null) ClipArrival(collider, board);
            }
            if (item.name == "MurPropulseurDroit")
            {
                BoxCollider collider = item.GetComponent<BoxCollider>();
                if (collider == null) continue;
                Vector3 center = Quaternion.Inverse(board.rotation) * (item.position - board.position);
                Vector3 a = item.InverseTransformPoint(board.position + board.rotation * new Vector3(center.x, center.y, -12.7f));
                Vector3 b = item.InverseTransformPoint(board.position + board.rotation * new Vector3(center.x, center.y, 5.21f));
                SetSegment(collider, a, b);
                safety.LauncherSkin(item, collider, board);
            }
        }
        Material surface = safety.Finish();
        safety.Wall("Continuous left wall", new Vector3(-6.375f, .45f, -4.6f), new Vector3(.5f, 1.3f, 24f), smooth, surface);
        safety.Wall("Continuous right wall", new Vector3(6.447f, .45f, -4.6f), new Vector3(.5f, 1.3f, 24f), smooth, surface);
        safety.Wall("Continuous back wall", new Vector3(.036f, .45f, 7.15f), new Vector3(13.322f, 1.3f, .5f), smooth, surface);
        safety.Wall("Solid backup playfield", new Vector3(.036f, -.185f, -4.7f), new Vector3(12.822f, .35f, 23.6f), smooth, null);
        safety.Wall("Invisible ball containment ceiling", new Vector3(.036f, 1.12f, -4.6f), new Vector3(13.322f, .25f, 24f), smooth, null);
        if (lives != null)
        {
            GameObject drain = safety.Wall("Drain below the flippers", new Vector3(.036f, .25f, -16.15f),
                new Vector3(12.822f, 1.5f, .6f), smooth, null);
            drain.AddComponent<DeathZone>().Configure(lives, board);
        }
        safety.Corner("Rounded left corner", -6.125f, 6.90f, 1.1f, false, smooth, surface);
        safety.Corner("Rounded launcher exit", 6.197f, 6.90f, 1.45f, true, smooth, surface);
        safety.guideVersion = CurrentVersion;
        return true;
    }

    private bool HasContainment()
    {
        foreach (string title in new[] { "Continuous left wall", "Continuous right wall", "Continuous back wall",
            "Solid backup playfield", "Invisible ball containment ceiling", "Drain below the flippers" })
        {
            Transform part = transform.Find(title);
            if (part == null || !part.gameObject.activeInHierarchy) return false;
            BoxCollider collider = part.GetComponent<BoxCollider>();
            if (collider == null || !collider.enabled || collider.isTrigger != (title == "Drain below the flippers")) return false;
        }
        return true;
    }

    private GameObject Wall(string title, Vector3 center, Vector3 size, PhysicsMaterial physics, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = title;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = center;
        go.transform.localScale = size;
        go.GetComponent<Collider>().sharedMaterial = physics;
        var renderer = go.GetComponent<Renderer>();
        if (material == null) renderer.enabled = false;
        else renderer.sharedMaterial = material;
        return go;
    }

    private void LauncherSkin(Transform original, BoxCollider physical, Transform board)
    {
        Renderer old = original.GetComponent<Renderer>();
        if (old == null) return;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Shortened launcher lane wall";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Quaternion.Inverse(board.rotation)
            * (original.TransformPoint(physical.center) - board.position);
        go.transform.localRotation = Quaternion.Inverse(board.rotation) * original.rotation;
        go.transform.localScale = new Vector3(
            original.TransformVector(Vector3.right * physical.size.x).magnitude,
            original.TransformVector(Vector3.up * physical.size.y).magnitude,
            original.TransformVector(Vector3.forward * physical.size.z).magnitude);
        Collider duplicate = go.GetComponent<Collider>();
        duplicate.enabled = false;
        if (Application.isPlaying) Destroy(duplicate); else DestroyImmediate(duplicate);
        go.GetComponent<Renderer>().sharedMaterials = old.sharedMaterials;
        old.enabled = false;
    }

    private static void ClipArrival(BoxCollider collider, Transform board)
    {
        Vector3 a = collider.transform.TransformPoint(collider.center - Vector3.forward * collider.size.z * .5f);
        Vector3 b = collider.transform.TransformPoint(collider.center + Vector3.forward * collider.size.z * .5f);
        Quaternion inverse = Quaternion.Inverse(board.rotation);
        Vector3 localA = inverse * (a - board.position), localB = inverse * (b - board.position);
        float delta = localB.x - localA.x;
        if (Mathf.Abs(delta) < .001f) return;
        float first = (-6.35f - localA.x) / delta, last = (6.42f - localA.x) / delta;
        float start = Mathf.Max(0, Mathf.Min(first, last)), end = Mathf.Min(1, Mathf.Max(first, last));
        if (end <= start) return;
        SetSegment(collider, collider.transform.InverseTransformPoint(Vector3.Lerp(a, b, start)),
            collider.transform.InverseTransformPoint(Vector3.Lerp(a, b, end)));
    }

    private static void SetSegment(BoxCollider collider, Vector3 a, Vector3 b)
    {
        collider.center = (a + b) * .5f;
        Vector3 size = collider.size;
        size.z = Mathf.Abs(b.z - a.z);
        collider.size = size;
    }

    private void Corner(string title, float x, float z, float radius, bool right, PhysicsMaterial physics, Material material)
    {
        var outline = new List<Vector2> { new Vector2(x, z) };
        float centerX = x + (right ? -radius : radius), centerZ = z - radius;
        for (int i = 0; i <= 24; i++)
        {
            float angle = (right ? i * 90f / 24 : 180 - i * 90f / 24) * Mathf.Deg2Rad;
            outline.Add(new Vector2(centerX + Mathf.Cos(angle) * radius, centerZ + Mathf.Sin(angle) * radius));
        }
        // Les deux contours suivent le même ordre de sommets, avec des faces supérieures orientées vers le haut.
        if (!right) outline.Reverse();
        int count = outline.Count;
        var vertices = new Vector3[count * 2];
        for (int i = 0; i < count; i++)
        { vertices[i] = new Vector3(outline[i].x, -.12f, outline[i].y); vertices[i + count] = new Vector3(outline[i].x, .72f, outline[i].y); }
        var triangles = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            triangles.AddRange(new[] { i, next, next + count, i, next + count, i + count });
        }
        // Le coin extérieur d'origine convient comme centre de triangulation pour ce raccord concave en quart de cercle.
        int fan = right ? 0 : count - 1;
        for (int i = 1; i < count - 1; i++)
        {
            int a = (fan + i) % count, b = (fan + i + 1) % count;
            triangles.AddRange(new[] { fan, b, a, fan + count, a + count, b + count });
        }
        var mesh = new Mesh { name = title };
        mesh.vertices = vertices;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureFolder();
            string path = Folder + "/" + title.Replace(" ", "_") + "_Closed_v2.asset";
            Mesh saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved != null) { DestroyImmediate(mesh); mesh = saved; }
            else UnityEditor.AssetDatabase.CreateAsset(mesh, path);
        }
        else
#endif
            resources.Add(mesh);
        var go = new GameObject(title, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        go.transform.SetParent(transform, false);
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        var collider = go.GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.sharedMaterial = physics;
        collider.convex = false;
    }

    private PhysicsMaterial Rolling()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var saved = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/SmoothGuides.physicMaterial");
            if (saved != null) return saved;
        }
#endif
        var material = new PhysicsMaterial("Low friction boundary guides")
        {
            dynamicFriction = 0, staticFriction = 0, bounciness = .05f,
            frictionCombine = PhysicsMaterialCombine.Minimum, bounceCombine = PhysicsMaterialCombine.Minimum
        };
        Keep(material, "SmoothGuides.physicMaterial");
        return material;
    }

    private Material Finish()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Material saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(Folder + "/GuideEnamel.mat");
            if (saved != null) return saved;
        }
#endif
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        { name = "Forest enamel corner guides", color = new Color32(25, 58, 50, 255) };
        material.SetFloat("_Smoothness", .25f);
        Keep(material, "GuideEnamel.mat");
        return material;
    }

    private void Keep(UnityEngine.Object resource, string file)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        { EnsureFolder(); UnityEditor.AssetDatabase.CreateAsset(resource, Folder + "/" + file); return; }
#endif
        resources.Add(resource);
    }
#if UNITY_EDITOR
    private static void EnsureFolder()
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(Folder)) UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesGuides");
    }
#endif
    private void OnDestroy()
    {
        foreach (UnityEngine.Object resource in resources)
            if (resource != null) { if (Application.isPlaying) Destroy(resource); else DestroyImmediate(resource); }
    }
}
