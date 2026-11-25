using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>A tapered pinball paddle skin, fitted to the original collider and moving with it.</summary>
public class VosgesFlipperSkin : MonoBehaviour
{
    private readonly List<UnityEngine.Object> resources = new List<UnityEngine.Object>();
    private Transform flipper;
    private Vector3 origin, forward, sideways, up;
    private float length, radius, tipRadius, thickness;
    private string assetPrefix;
    [SerializeField, HideInInspector] private int skinVersion;
    private const int CurrentSkinVersion = 1;

    public static bool EnsureForScene(Scene scene)
    {
        bool changed = false;
        var visited = new HashSet<Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (NewMonoBehaviourScript controller in root.GetComponentsInChildren<NewMonoBehaviourScript>(true))
            {
                if (controller.table == null) continue;
                if (controller.leftFlipper != null && visited.Add(controller.leftFlipper))
                    changed |= Prepare(controller.leftFlipper, controller.rightFlipper, controller.table, "Left");
                if (controller.rightFlipper != null && visited.Add(controller.rightFlipper))
                    changed |= Prepare(controller.rightFlipper, controller.leftFlipper, controller.table, "Right");
            }
        return changed;
    }

    private static bool Prepare(Transform paddle, Transform other, Transform board, string side)
    {
        Transform existing = paddle.Find("Vosges flipper skin");
        if (existing != null)
        {
            VosgesFlipperSkin previous = existing.GetComponent<VosgesFlipperSkin>();
            if (previous == null || previous.skinVersion == CurrentSkinVersion) return HideOriginal(paddle, existing);
            existing.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(existing.gameObject); else DestroyImmediate(existing.gameObject);
        }
        BoxCollider collider = paddle.GetComponentInChildren<BoxCollider>();
        if (collider == null) return false;
        var go = new GameObject("Vosges flipper skin", typeof(VosgesFlipperSkin));
        go.transform.SetParent(paddle, false);
        go.GetComponent<VosgesFlipperSkin>().Build(paddle, other, board, collider, side);
        HideOriginal(paddle, go.transform);
        return true;
    }

    private void Build(Transform paddle, Transform other, Transform board, BoxCollider collider, string side)
    {
        flipper = paddle;
        assetPrefix = paddle.gameObject.scene.name + "_" + side;
        up = board.up.normalized;
        Vector3 halfX = collider.transform.TransformVector(Vector3.right * collider.size.x * .5f);
        Vector3 halfZ = collider.transform.TransformVector(Vector3.forward * collider.size.z * .5f);
        Vector3 axis = halfX.sqrMagnitude >= halfZ.sqrMagnitude ? halfX : halfZ;
        forward = Vector3.ProjectOnPlane(axis, up).normalized;
        sideways = Vector3.Cross(up, forward).normalized;
        Vector3 center = collider.transform.TransformPoint(collider.center);
        float minLong = float.PositiveInfinity, maxLong = float.NegativeInfinity;
        float minWide = float.PositiveInfinity, maxWide = float.NegativeInfinity;
        float minHigh = float.PositiveInfinity, maxHigh = float.NegativeInfinity;
        for (int corner = 0; corner < 8; corner++)
        {
            Vector3 sign = new Vector3((corner & 1) == 0 ? -1 : 1,
                (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1);
            Vector3 local = collider.center + Vector3.Scale(collider.size * .5f, sign);
            Vector3 offset = collider.transform.TransformPoint(local) - center;
            float along = Vector3.Dot(offset, forward), across = Vector3.Dot(offset, sideways), height = Vector3.Dot(offset, up);
            minLong = Mathf.Min(minLong, along); maxLong = Mathf.Max(maxLong, along);
            minWide = Mathf.Min(minWide, across); maxWide = Mathf.Max(maxWide, across);
            minHigh = Mathf.Min(minHigh, height); maxHigh = Mathf.Max(maxHigh, height);
        }
        length = maxLong - minLong;
        radius = (maxWide - minWide) * .5f;
        thickness = maxHigh - minHigh;
        Vector3 a = center + forward * minLong, b = center + forward * maxLong;
        Vector3 otherPosition = other != null ? other.position : board.position;
        if ((b - otherPosition).sqrMagnitude > (a - otherPosition).sqrMagnitude)
        { forward = -forward; sideways = -sideways; origin = b; }
        else origin = a;
        tipRadius = radius * .64f;

        Material rubber = Finish("Red rubber", new Color32(157, 26, 25, 255), 0, .23f);
        Material ivory = Finish("Warm ivory face", new Color32(235, 235, 235, 255), .08f, .35f);
        Material green = Finish("Vosges green inlay", new Color32(18, 69, 57, 255), .18f, .4f);
        Material steel = Finish("Pivot stainless steel", new Color32(180, 180, 180, 255), .8f, .5f);
        Material dark = Finish("Screw recess", new Color32(22, 25, 23, 255), 0, .2f);
        Extrude("Rounded rubber body", Profile(0), -thickness * .48f, thickness * .48f, rubber);
        float inset = Mathf.Min(.04f, radius * .2f);
        Extrude("Ivory top plate", Profile(inset), thickness * .29f, thickness * .5f, ivory);
        var stripe = new List<Vector2>
        {
            new Vector2(length * .27f, -radius * .22f), new Vector2(length * .77f, -tipRadius * .22f),
            new Vector2(length * .88f, 0), new Vector2(length * .77f, tipRadius * .22f),
            new Vector2(length * .27f, radius * .22f)
        };
        Extrude("Ski arrow inlay", stripe, thickness * .5f, thickness * .52f, green);
        var bolt = new List<Vector2>();
        float boltRadius = radius * .32f;
        for (int i = 0; i < 24; i++)
        {
            float angle = i * Mathf.PI * 2 / 24;
            bolt.Add(new Vector2(radius + Mathf.Cos(angle) * boltRadius, Mathf.Sin(angle) * boltRadius));
        }
        Extrude("Pivot bolt", bolt, thickness * .5f, thickness * .57f, steel);
        var slot = new List<Vector2>
        {
            new Vector2(radius - boltRadius * .65f, -boltRadius * .1f),
            new Vector2(radius + boltRadius * .65f, -boltRadius * .1f),
            new Vector2(radius + boltRadius * .65f, boltRadius * .1f),
            new Vector2(radius - boltRadius * .65f, boltRadius * .1f)
        };
        Extrude("Pivot screw slot", slot, thickness * .57f, thickness * .58f, dark);
        skinVersion = CurrentSkinVersion;
    }

    private List<Vector2> Profile(float inset)
    {
        var result = new List<Vector2>();
        float baseR = radius - inset, noseR = tipRadius - inset;
        for (int i = 0; i <= 18; i++)
        {
            float angle = (-90 + i * 10) * Mathf.Deg2Rad;
            result.Add(new Vector2(length - tipRadius + Mathf.Cos(angle) * noseR, Mathf.Sin(angle) * noseR));
        }
        for (int i = 0; i <= 18; i++)
        {
            float angle = (90 + i * 10) * Mathf.Deg2Rad;
            result.Add(new Vector2(radius + Mathf.Cos(angle) * baseR, Mathf.Sin(angle) * baseR));
        }
        return result;
    }

    private void Extrude(string title, List<Vector2> outline, float bottom, float top, Material material)
    {
        int count = outline.Count;
        var vertices = new Vector3[count * 2 + 2];
        Vector2 center = Vector2.zero;
        for (int i = 0; i < count; i++) center += outline[i];
        center /= count;
        for (int i = 0; i < count; i++)
        {
            vertices[i] = Local(outline[i], bottom);
            vertices[i + count] = Local(outline[i], top);
        }
        vertices[count * 2] = Local(center, bottom);
        vertices[count * 2 + 1] = Local(center, top);
        var indices = new List<int>(count * 12);
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            indices.Add(i); indices.Add(next); indices.Add(next + count);
            indices.Add(i); indices.Add(next + count); indices.Add(i + count);
            indices.Add(count * 2); indices.Add(next); indices.Add(i);
            indices.Add(count * 2 + 1); indices.Add(i + count); indices.Add(next + count);
        }
        var mesh = new Mesh { name = title };
        // The imported left paddle is mirrored. Correct its local winding so both
        // paddles retain outward normals and visible top faces under backface culling.
        if (flipper.localToWorldMatrix.determinant < 0)
            for (int i = 0; i < indices.Count; i += 3)
            {
                int swap = indices[i + 1];
                indices[i + 1] = indices[i + 2];
                indices[i + 2] = swap;
            }
        mesh.vertices = vertices;
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureFolder();
            string path = Folder + "/" + assetPrefix + "_" + title.Replace(" ", "_") + "_Surface_v1.asset";
            Mesh saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved != null) { DestroyImmediate(mesh); mesh = saved; }
            else UnityEditor.AssetDatabase.CreateAsset(mesh, path);
        }
        else
#endif
            resources.Add(mesh);
        var go = new GameObject(title, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private Vector3 Local(Vector2 point, float height)
    {
        // Baking into the paddle's frame preserves its imported scale and existing pivot motion.
        return flipper.InverseTransformPoint(origin + forward * point.x + sideways * point.y + up * height);
    }

    private Material Finish(string title, Color color, float metallic, float smoothness)
    {
#if UNITY_EDITOR
        string path = Folder + "/" + title.Replace(" ", "_") + ".mat";
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
            EnsureFolder();
            UnityEditor.AssetDatabase.CreateAsset(material, path);
            return material;
        }
#endif
        resources.Add(material);
        return material;
    }

    private const string Folder = "Assets/Materials/VosgesFlippers";
#if UNITY_EDITOR
    private static void EnsureFolder()
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(Folder))
            UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesFlippers");
    }
#endif

    private static bool HideOriginal(Transform paddle, Transform skin)
    {
        bool changed = false;
        foreach (Renderer renderer in paddle.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.transform.IsChildOf(skin)) continue;
            if (renderer.enabled) { renderer.enabled = false; changed = true; }
        }
        return changed;
    }

    private void OnDestroy()
    {
        foreach (UnityEngine.Object resource in resources)
            if (resource != null)
            { if (Application.isPlaying) Destroy(resource); else DestroyImmediate(resource); }
    }
}
