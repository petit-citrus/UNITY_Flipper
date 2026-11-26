using System.Collections.Generic;
using UnityEngine;

/// <summary>Rampe fixe et lisse pour le flipper, reconstruite dans l'éditeur et dans les builds.</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class VosgesRamp : MonoBehaviour
{
    [Header("Path in the playfield's local frame")]
    [SerializeField] private Vector2 entry;
    [SerializeField] private Vector2 controlOne;
    [SerializeField] private Vector2 controlTwo;
    [SerializeField] private Vector2 exit;
    [SerializeField, Min(.8f)] private float width = 1.15f;
    [SerializeField, Range(.1f, 1.5f)] private float rise = .65f;
    [SerializeField, Range(.3f, .8f)] private float railHeight = .48f;
    [SerializeField, Range(24, 128)] private int segments = 64;

    [Header("Winter finish and rolling surface")]
    [SerializeField] private Material surfaceMaterial;
    [SerializeField] private Material railMaterial;
    [SerializeField] private PhysicsMaterial rollingMaterial;

    private GameObject generated;
    private readonly List<Mesh> meshes = new List<Mesh>();
    private bool needsRebuild;

    public void Configure(Vector2 start, Vector2 firstBend, Vector2 secondBend, Vector2 finish,
        float trackWidth, float height, Material surface, Material rails, PhysicsMaterial physics)
    {
        entry = start;
        controlOne = firstBend;
        controlTwo = secondBend;
        exit = finish;
        width = trackWidth;
        rise = height;
        surfaceMaterial = surface;
        railMaterial = rails;
        rollingMaterial = physics;
        segments = 80;
        Rebuild();
    }

    private void OnEnable() { Rebuild(); }
    private void OnValidate() { needsRebuild = true; }

    private void Update()
    {
        // La création du maillage et du collider est différée après le rappel de sérialisation d'Unity.
        if (needsRebuild) Rebuild();
    }

    [ContextMenu("Rebuild ramp")]
    public void Rebuild()
    {
        needsRebuild = false;
        Clear();
        if (surfaceMaterial == null || railMaterial == null) return;
        segments = Mathf.Clamp(segments, 24, 128);
        width = Mathf.Max(.8f, width);
        generated = new GameObject("Ramp geometry (generated)") { hideFlags = HideFlags.DontSave };
        generated.transform.SetParent(transform, false);
        // Un tablier fermé donne une épaisseur réelle au collider sans ajouter de rebord à l'entrée.
        Part("Ice track", Sweep(0, width, false), surfaceMaterial);
        const float railThickness = .08f;
        float railOffset = (width + railThickness) * .5f;
        Part("Left safety rail", Sweep(-railOffset, railThickness, true), railMaterial);
        Part("Right safety rail", Sweep(railOffset, railThickness, true), railMaterial);
    }

    private Vector3 Center(float t)
    {
        float u = 1 - t;
        Vector2 point = u * u * u * entry + 3 * u * u * t * controlOne
            + 3 * u * t * t * controlTwo + t * t * t * exit;
        // Les deux extrémités rejoignent la piste sans pente supplémentaire : aucune marche ne peut retenir la balle.
        float height = 16 * rise * t * t * u * u;
        return new Vector3(point.x, .003f + height, point.y);
    }

    private Vector3 Right(float t)
    {
        float u = 1 - t;
        Vector2 tangent = 3 * u * u * (controlOne - entry)
            + 6 * u * t * (controlTwo - controlOne) + 3 * t * t * (exit - controlTwo);
        Vector3 right = new Vector3(tangent.y, 0, -tangent.x);
        return right.sqrMagnitude > .000001f ? right.normalized : Vector3.right;
    }

    private Mesh Sweep(float offset, float sectionWidth, bool rail)
    {
        var vertices = new Vector3[(segments + 1) * 4];
        var uv = new Vector2[vertices.Length];
        var triangles = new List<int>(segments * 24 + 12);
        for (int row = 0; row <= segments; row++)
        {
            float t = row / (float)segments;
            Vector3 right = Right(t);
            Vector3 center = Center(t) + right * offset;
            // Les rails s'effilent aux deux entrées pour guider les tirs rasants dans le couloir.
            float railTop = rail ? railHeight * Mathf.SmoothStep(0, 1, Mathf.Min(t, 1 - t) * 12) : 0;
            int index = row * 4;
            vertices[index] = center - right * sectionWidth * .5f - Vector3.up * .08f;
            vertices[index + 1] = center + right * sectionWidth * .5f - Vector3.up * .08f;
            vertices[index + 2] = center + right * sectionWidth * .5f + Vector3.up * railTop;
            vertices[index + 3] = center - right * sectionWidth * .5f + Vector3.up * railTop;
            for (int side = 0; side < 4; side++) uv[index + side] = new Vector2(side / 3f, t * 5);
            if (row == segments) continue;
            for (int side = 0; side < 4; side++)
            {
                int a = index + side;
                int b = index + (side + 1) % 4;
                triangles.Add(a); triangles.Add(b); triangles.Add(b + 4);
                triangles.Add(a); triangles.Add(b + 4); triangles.Add(a + 4);
            }
        }
        triangles.AddRange(new[] { 0, 2, 1, 0, 3, 2 });
        int end = segments * 4;
        triangles.AddRange(new[] { end, end + 1, end + 2, end, end + 2, end + 3 });
        var mesh = new Mesh { name = name + (rail ? " safety rail" : " smooth deck"), hideFlags = HideFlags.DontSave };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshes.Add(mesh);
        return mesh;
    }

    private void Part(string title, Mesh mesh, Material material)
    {
        var part = new GameObject(title, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))
            { hideFlags = HideFlags.DontSave };
        part.transform.SetParent(generated.transform, false);
        part.GetComponent<MeshFilter>().sharedMesh = mesh;
        part.GetComponent<MeshRenderer>().sharedMaterial = material;
        var collider = part.GetComponent<MeshCollider>();
        collider.sharedMaterial = rollingMaterial;
        collider.sharedMesh = mesh;
        // Ces surfaces de piste sont fixes et concaves. Aucun Rigidbody n'est ajouté à la rampe.
        collider.convex = false;
    }

    private void OnDisable() { Clear(); }

    private void Clear()
    {
        if (generated != null)
        {
            generated.SetActive(false);
            Release(generated);
            generated = null;
        }
        foreach (Mesh mesh in meshes) if (mesh != null) Release(mesh);
        meshes.Clear();
    }

    private static void Release(UnityEngine.Object resource)
    {
        if (Application.isPlaying) Destroy(resource);
        else DestroyImmediate(resource);
    }
}
