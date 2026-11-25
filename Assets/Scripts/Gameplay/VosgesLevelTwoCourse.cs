using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>The Hohneck expert course. Builds ordinary editable objects in the level-two scene.</summary>
public class VosgesLevelTwoCourse : MonoBehaviour
{
    [SerializeField, HideInInspector] private int layoutVersion;
    private const int CurrentVersion = 1;
    private const string Folder = "Assets/Materials/VosgesLevelTwo";
    private readonly List<UnityEngine.Object> resources = new List<UnityEngine.Object>();

    private void Start()
    {
        if (EnsureForScene(gameObject.scene)) VosgesGameplaySetup.EnsureForScene(gameObject.scene);
    }

    public static bool EnsureForScene(Scene scene)
    {
        if (scene.name != "Level_2_test") return false;
        Transform board = null;
        TargetController template = null;
        VosgesLevelTwoCourse course = null;
        var items = new List<Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            items.AddRange(root.GetComponentsInChildren<Transform>(true));
            if (root.GetComponent<VosgesLevelTwoCourse>() != null) course = root.GetComponent<VosgesLevelTwoCourse>();
        }
        foreach (Transform item in items)
        {
            if (item.name == "BaseLvl_1") board = item;
            if (item.name == "Bumper WaluigiPinball") template = item.GetComponent<TargetController>();
        }
        if (board == null || template == null) return false;
        if (course != null && course.layoutVersion == CurrentVersion
            && course.transform.Find("Rampe du Col") != null && course.transform.Find("Rampe du Lac Blanc") != null
            && course.transform.Find("Lac Blanc target") != null && course.transform.Find("Route des Cretes target") != null
            && course.transform.Find("Expert slalom and spinner") != null) return false;
        if (course == null)
        {
            var go = new GameObject("Niveau 2 - Le Hohneck", typeof(VosgesLevelTwoCourse));
            SceneManager.MoveGameObjectToScene(go, scene);
            course = go.GetComponent<VosgesLevelTwoCourse>();
        }
        else
            for (int i = course.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = course.transform.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
        course.transform.SetPositionAndRotation(board.position, board.rotation);
        foreach (Transform item in items)
        {
            if (item == null || item.IsChildOf(course.transform)) continue;
            switch (item.name)
            {
                case "Bumper WaluigiPinball": Move(item, board, -2.3f, 2.9f); break;
                case "Bumper WaluigiPinball (3)": Move(item, board, 0, 4.2f); break;
                case "Bumper WaluigiPinball (4)": Move(item, board, 2.15f, 2.7f); break;
                case "Arbre1": Move(item, board, -4.7f, -.65f); break;
                case "Arbre2": Move(item, board, 3.5f, -2.45f); break;
                case "Arbre3": Move(item, board, -4.55f, 4.2f); break;
                case "Chair": case "Chair (1)": case "miroir_chair": item.gameObject.SetActive(false); break;
            }
            if (item.name.StartsWith("Barriere")) item.gameObject.SetActive(false);
            if (item.name.StartsWith("Bumper WaluigiPinball"))
                foreach (TargetController target in item.GetComponentsInChildren<TargetController>(true))
                { target.scoreValue = 100; target.bounceForce = 8; }
        }
        course.Build(template);
        course.layoutVersion = CurrentVersion;
        return true;
    }

    private static void Move(Transform item, Transform board, float x, float worldZ)
    {
        Vector3 local = Quaternion.Inverse(board.rotation) * (item.position - board.position);
        local.x = x;
        local.z = TrackZ(worldZ);
        item.position = board.position + board.rotation * local;
    }

    private static float TrackZ(float worldZ) { return (worldZ - .73f) / Mathf.Cos(20 * Mathf.Deg2Rad); }
    private static Vector2 Point(float x, float worldZ) { return new Vector2(x, TrackZ(worldZ)); }

    private void Build(TargetController template)
    {
        Material ice = Finish("Expert ice track", new Color32(86, 152, 166, 255), .32f);
        Material rails = Finish("Expert stainless rails", new Color32(150, 158, 160, 255), .4f);
        PhysicsMaterial rolling = Rolling();
        Ramp("Rampe du Col", Point(-2.7f, -9.2f), Point(-3.6f, -7.4f), Point(-3.8f, -4.4f), Point(-1.75f, -2.5f), .30f, ice, rails, rolling);
        Ramp("Rampe du Lac Blanc", Point(1.1f, -9.3f), Point(2.2f, -7f), Point(1.3f, -4.5f), Point(.1f, -2.55f), .38f, ice, rails, rolling);
        Target(template, "Lac Blanc target", "Bumper WaluigiPinball - Lac Blanc", -.95f, .2f);
        Target(template, "Route des Cretes target", "Bumper WaluigiPinball - Route des Cretes", 1.2f, .05f);
        var interactives = new GameObject("Expert slalom and spinner", typeof(VosgesLevelOneObstacles));
        interactives.transform.SetParent(transform, false);
        interactives.GetComponent<VosgesLevelOneObstacles>().BuildExpertSet();
        var title = new GameObject("Piste Noire playfield marking", typeof(TextMeshPro));
        title.transform.SetParent(transform, false);
        title.transform.localPosition = new Vector3(-.45f, .012f, TrackZ(-10.3f));
        title.transform.localRotation = Quaternion.Euler(90, 90, 0);
        var text = title.GetComponent<TextMeshPro>();
        text.text = "LE HOHNECK\nPISTE NOIRE";
        text.fontSize = 1.7f;
        text.color = new Color32(40, 65, 66, 255);
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = new Vector2(2.2f, .6f);
    }

    private void Ramp(string title, Vector2 start, Vector2 bendOne, Vector2 bendTwo, Vector2 finish, float height,
        Material surface, Material rails, PhysicsMaterial physics)
    {
        var go = new GameObject(title, typeof(VosgesRamp));
        go.transform.SetParent(transform, false);
        go.GetComponent<VosgesRamp>().Configure(start, bendOne, bendTwo, finish, 1.05f, height, surface, rails, physics);
    }

    private void Target(TargetController template, string anchorName, string title, float x, float worldZ)
    {
        var anchor = new GameObject(anchorName).transform;
        anchor.SetParent(transform, false);
        anchor.localPosition = new Vector3(x, -.03f, TrackZ(worldZ));
        GameObject copy = Instantiate(template.gameObject, anchor);
        copy.name = title;
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.identity;
        copy.transform.localScale = template.transform.lossyScale;
        foreach (TargetController target in copy.GetComponentsInChildren<TargetController>(true))
        { target.scoreValue = 100; target.bounceForce = 8; }
    }

    private Material Finish(string title, Color color, float smoothness)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(Folder + "/" + title.Replace(" ", "_") + ".mat");
            if (saved != null) return saved;
        }
#endif
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = title, color = color };
        material.SetFloat("_Metallic", .2f);
        material.SetFloat("_Smoothness", smoothness);
        Keep(material, title.Replace(" ", "_") + ".mat");
        return material;
    }

    private PhysicsMaterial Rolling()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var saved = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/ExpertRolling.physicMaterial");
            if (saved != null) return saved;
        }
#endif
        var material = new PhysicsMaterial("Expert ramp rolling")
        {
            dynamicFriction = .02f, staticFriction = .02f, bounciness = 0,
            frictionCombine = PhysicsMaterialCombine.Minimum, bounceCombine = PhysicsMaterialCombine.Minimum
        };
        Keep(material, "ExpertRolling.physicMaterial");
        return material;
    }

    private void Keep(UnityEngine.Object resource, string file)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder(Folder)) UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesLevelTwo");
            UnityEditor.AssetDatabase.CreateAsset(resource, Folder + "/" + file);
            return;
        }
#endif
        resources.Add(resource);
    }

    private void OnDestroy()
    {
        foreach (UnityEngine.Object resource in resources)
            if (resource != null) { if (Application.isPlaying) Destroy(resource); else DestroyImmediate(resource); }
    }
}
