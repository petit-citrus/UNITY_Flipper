using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Editable, physical winter obstacles for level one only.</summary>
public class VosgesLevelOneObstacles : MonoBehaviour
{
    private readonly List<UnityEngine.Object> resources = new List<UnityEngine.Object>();
    private Material snow, forest, red, wood, metal;
    private PhysicsMaterial rolling;

    public static bool EnsureForScene(Scene scene)
    {
        if (scene.name != "Level_1_recup") return false;
        Transform board = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<VosgesLevelOneObstacles>(true) != null) return false;
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                if (candidate.name == "BaseLvl_1") board = candidate;
        }
        if (board == null) return false;
        var go = new GameObject("Vosges - Level 1 interactive obstacles", typeof(VosgesLevelOneObstacles));
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.SetPositionAndRotation(board.position, board.rotation);
        go.GetComponent<VosgesLevelOneObstacles>().Build();
        return true;
    }

    private void Build()
    {
        CreateMaterials();
        SnowBumper("Bumper du lac - 50", -4.05f, -.75f, forest);
        SnowBumper("Bumper du refuge - 50", 3.2f, -3.3f, red);
        Spinner();
        SlalomGate();
    }

    public void BuildExpertSet()
    {
        CreateMaterials();
        Spinner(0, 2f, 42, "Tourniquet du Hohneck - 25");
        SlalomGate(-3.65f, 1.2f, "Slalom du col - 80");
        SlalomGate(2.9f, 1f, "Slalom du lac - 80");
    }

    private void CreateMaterials()
    {
        snow = Finish("Neutral snow", new Color32(236, 236, 236, 255));
        forest = Finish("Forest enamel", new Color32(21, 72, 61, 255), true);
        red = Finish("Piste red", new Color32(165, 42, 34, 255), true);
        wood = Finish("Vosges timber", new Color32(96, 90, 84, 255));
        metal = Finish("Neutral steel", new Color32(180, 180, 180, 255));
        rolling = RollingMaterial();
    }

    private GameObject Obstacle(string title, float x, float worldZ, int points, float impulse)
    {
        var go = new GameObject(title, typeof(Rigidbody), typeof(VosgesInteractiveObstacle));
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(x, 0, (worldZ - .73f) / Mathf.Cos(20 * Mathf.Deg2Rad));
        var body = go.GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        var interactive = go.GetComponent<VosgesInteractiveObstacle>();
        interactive.points = points;
        interactive.kick = impulse;
        return go;
    }

    private void SnowBumper(string title, float x, float z, Material color)
    {
        GameObject go = Obstacle(title, x, z, 50, 3.8f);
        SphereCollider collider = go.AddComponent<SphereCollider>();
        collider.radius = .46f;
        collider.center = Vector3.zero;
        collider.sharedMaterial = rolling;
        Shape(go.transform, "Metal foot", PrimitiveType.Cylinder, new Vector3(0, .045f, 0), new Vector3(1.12f, .045f, 1.12f), metal);
        Shape(go.transform, "Timber drum", PrimitiveType.Cylinder, new Vector3(0, .17f, 0), new Vector3(1.02f, .09f, 1.02f), wood);
        var ring = Shape(go.transform, "Scoring band", PrimitiveType.Cylinder, new Vector3(0, .24f, 0), new Vector3(1.06f, .045f, 1.06f), color);
        Shape(go.transform, "Snow cap", PrimitiveType.Sphere, new Vector3(0, .37f, 0), new Vector3(1.04f, .15f, 1.04f), snow);
        go.GetComponent<VosgesInteractiveObstacle>().accent = ring.GetComponent<Renderer>();
        Label(go.transform, "50", new Vector3(0, .455f, 0), new Vector2(.6f, .4f), 2.5f);
    }

    private void Spinner(float x = .35f, float z = -2.4f, float speed = 28, string title = "Tourniquet du teleski - 25")
    {
        GameObject go = Obstacle(title, x, z, 25, 2.4f);
        var hit = go.GetComponent<VosgesInteractiveObstacle>();
        hit.rotationSpeed = speed;
        hit.cooldown = .35f;
        var collider = go.AddComponent<BoxCollider>();
        collider.center = new Vector3(0, .22f, 0);
        collider.size = new Vector3(1.76f, .22f, .20f);
        collider.sharedMaterial = rolling;
        Shape(go.transform, "Pivot foot", PrimitiveType.Cylinder, new Vector3(0, .035f, 0), new Vector3(.42f, .035f, .42f), metal);
        Shape(go.transform, "Timber pivot", PrimitiveType.Cylinder, new Vector3(0, .17f, 0), new Vector3(.19f, .14f, .19f), wood);
        GameObject beam = Shape(go.transform, "Rotating ski tow beam", PrimitiveType.Cube, new Vector3(0, .22f, 0), new Vector3(1.76f, .20f, .20f), red);
        Shape(go.transform, "Snow on beam", PrimitiveType.Cube, new Vector3(0, .335f, 0), new Vector3(1.80f, .035f, .24f), snow);
        hit.accent = beam.GetComponent<Renderer>();
        Label(go.transform, "25", new Vector3(0, .36f, 0), new Vector2(.45f, .3f), 1.8f);
    }

    private void SlalomGate(float x = 2.15f, float z = .8f, string title = "Porte de slalom - montee 80")
    {
        GameObject go = Obstacle(title, x, z, 80, 0);
        var hit = go.GetComponent<VosgesInteractiveObstacle>();
        hit.passageOnly = true;
        hit.cooldown = 1;
        var trigger = go.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, .25f, 0);
        trigger.size = new Vector3(1.66f, .5f, .12f);
        foreach (int side in new[] { -1, 1 })
        {
            GameObject post = Shape(go.transform, "Slalom pole", PrimitiveType.Cylinder,
                new Vector3(side * .95f, .24f, 0), new Vector3(.12f, .24f, .12f), red, true);
            post.GetComponent<Collider>().sharedMaterial = rolling;
            Shape(go.transform, "Pole snow cap", PrimitiveType.Sphere, new Vector3(side * .95f, .49f, 0), Vector3.one * .14f, snow);
        }
        GameObject banner = Shape(go.transform, "Bonus flag", PrimitiveType.Cube, new Vector3(0, .65f, 0), new Vector3(1.85f, .08f, .18f), forest);
        hit.accent = banner.GetComponent<Renderer>();
        Label(go.transform, "80", new Vector3(0, .70f, 0), new Vector2(.4f, .3f), 1.5f);
    }

    private GameObject Shape(Transform parent, string title, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material, bool physical = false)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = title;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        if (!physical)
        {
            Collider collider = go.GetComponent<Collider>();
            collider.enabled = false;
            if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider);
        }
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private void Label(Transform parent, string caption, Vector3 position, Vector2 size, float fontSize)
    {
        var go = new GameObject("Points " + caption, typeof(TextMeshPro));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.Euler(90, 90, 0);
        var text = go.GetComponent<TextMeshPro>();
        text.text = caption;
        text.color = new Color32(41, 42, 33, 255);
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = size;
    }

    private Material Finish(string title, Color color, bool feedback = false)
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
        material.SetFloat("_Smoothness", .22f);
        if (feedback) material.EnableKeyword("_EMISSION");
        Keep(material, title.Replace(" ", "_") + ".mat");
        return material;
    }

    private PhysicsMaterial RollingMaterial()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var saved = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/ObstacleRolling.physicMaterial");
            if (saved != null) return saved;
        }
#endif
        var material = new PhysicsMaterial("Winter obstacle contact")
        {
            dynamicFriction = 0, staticFriction = 0, bounciness = .05f,
            frictionCombine = PhysicsMaterialCombine.Minimum, bounceCombine = PhysicsMaterialCombine.Minimum
        };
        Keep(material, "ObstacleRolling.physicMaterial");
        return material;
    }

    private const string Folder = "Assets/Materials/VosgesObstacles";

    private void Keep(UnityEngine.Object resource, string file)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder(Folder))
                UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesObstacles");
            UnityEditor.AssetDatabase.CreateAsset(resource, Folder + "/" + file);
            return;
        }
#endif
        resources.Add(resource);
    }

    private void OnDestroy()
    {
        foreach (UnityEngine.Object resource in resources)
            if (resource != null)
            { if (Application.isPlaying) Destroy(resource); else DestroyImmediate(resource); }
    }
}
