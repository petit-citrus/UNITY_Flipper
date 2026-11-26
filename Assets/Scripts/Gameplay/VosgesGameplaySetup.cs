using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Relie les cibles animées, configure la partie à deux niveaux et ajoute le lanceur auxiliaire.</summary>
public class VosgesGameplaySetup : MonoBehaviour
{
    [SerializeField, HideInInspector] private bool progressionConfigured;
    private readonly List<UnityEngine.Object> resources = new List<UnityEngine.Object>();
    private const string Folder = "Assets/Materials/VosgesLauncher";

    public static bool EnsureForScene(Scene scene)
    {
        Transform board = null;
        Rigidbody ball = null;
        LevelManager levels = null;
        VosgesGameplaySetup setup = null;
        var animators = new List<Animator>();
        var launchers = new List<BallLauncher>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (setup == null) setup = root.GetComponentInChildren<VosgesGameplaySetup>(true);
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == "BaseLvl_1") board = item;
            foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
                if (body.CompareTag("Ball")) ball = body;
            if (levels == null) levels = root.GetComponentInChildren<LevelManager>(true);
            animators.AddRange(root.GetComponentsInChildren<Animator>(true));
            launchers.AddRange(root.GetComponentsInChildren<BallLauncher>(true));
        }
        if (board == null || ball == null || levels == null) return false;
        bool changed = false;
        if (setup == null)
        {
            var go = new GameObject("Vosges - Gameplay configuration", typeof(VosgesGameplaySetup));
            SceneManager.MoveGameObjectToScene(go, scene);
            setup = go.GetComponent<VosgesGameplaySetup>();
            changed = true;
        }
        if (!setup.progressionConfigured)
        {
            levels.ConfigureProgression(scene.name == "Level_2_test" ? 2 : 1, 1000);
            setup.progressionConfigured = true;
            changed = true;
        }
        foreach (BallLauncher launcher in launchers)
        {
            if (launcher.automatic) continue;
            if (launcher.balle == null) { launcher.balle = ball; changed = true; }
            Transform point = launcher.transform.Find("Vosges launch position");
            if (point == null)
            {
                point = new GameObject("Vosges launch position").transform;
                point.SetParent(launcher.transform, false);
                point.SetPositionAndRotation(launcher.balle.worldCenterOfMass,
                    launcher.pointDeLancement != null ? launcher.pointDeLancement.rotation : board.rotation);
                changed = true;
            }
            if (launcher.pointDeLancement != point) { launcher.pointDeLancement = point; changed = true; }
            if (!Mathf.Approximately(launcher.rayonDeLancement, .65f))
            { launcher.rayonDeLancement = .65f; changed = true; }
        }
        foreach (Animator animator in animators)
        {
            if (animator.runtimeAnimatorController == null) continue;
            bool impact = false;
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.name == "Impact" && parameter.type == AnimatorControllerParameterType.Trigger)
                { impact = true; break; }
            if (!impact) continue;
            Collider[] colliders = animator.GetComponentsInChildren<Collider>(true);
            bool physical = false;
            foreach (Collider collider in colliders) if (collider.enabled) physical = true;
            if (!physical) continue;
            TargetController canonical = animator.GetComponent<TargetController>();
            if (canonical == null) canonical = animator.GetComponentInChildren<TargetController>(true);
            if (canonical == null)
            { canonical = animator.gameObject.AddComponent<TargetController>(); changed = true; }
            if (!canonical.enabled) { canonical.enabled = true; changed = true; }
            changed |= canonical.Configure(animator, board, 100);
            foreach (TargetController duplicate in animator.GetComponentsInChildren<TargetController>(true))
                if (duplicate != canonical && duplicate.enabled) { duplicate.enabled = false; changed = true; }
            foreach (BumperController bumper in animator.GetComponentsInChildren<BumperController>(true))
                if (bumper.enabled) { bumper.enabled = false; changed = true; }
            foreach (Collider collider in colliders)
            {
                if (!collider.enabled) continue;
                TargetHitRelay relay = collider.GetComponent<TargetHitRelay>();
                if (relay == null) { relay = collider.gameObject.AddComponent<TargetHitRelay>(); changed = true; }
                if (relay.target != canonical) { relay.target = canonical; changed = true; }
            }
        }
        if (scene.name == "Level_1_recup" && setup.transform.Find("Auxiliary avalanche launcher") == null)
        {
            setup.BuildLauncher(board, ball);
            changed = true;
        }
        return changed;
    }

    private void BuildLauncher(Transform board, Rigidbody ball)
    {
        var go = new GameObject("Auxiliary avalanche launcher", typeof(BallLauncher));
        go.transform.SetParent(transform, false);
        go.transform.SetPositionAndRotation(board.position + board.rotation * new Vector3(-.2f, 0, (5.2f - .73f) / Mathf.Cos(20 * Mathf.Deg2Rad)),
            Quaternion.LookRotation(board.rotation * new Vector3(-.15f, 0, -1).normalized, board.up));
        var point = new GameObject("Launch direction").transform;
        point.SetParent(go.transform, false);
        point.localPosition = Vector3.up * .25f;
        var launcher = go.GetComponent<BallLauncher>();
        launcher.balle = ball;
        launcher.pointDeLancement = point;
        launcher.automatic = true;
        launcher.impulsion = 16;
        launcher.rayonDeLancement = .6f;
        launcher.cooldown = 1.3f;
        Material dark = Finish("Launcher enamel", new Color32(22, 51, 50, 255));
        Material steel = Finish("Launcher steel", new Color32(176, 183, 185, 255));
        Material white = Finish("Launcher markings", new Color32(235, 238, 238, 255));
        Shape(go.transform, "Receiver cup", PrimitiveType.Cylinder, new Vector3(0, -.025f, 0), new Vector3(1.2f, .035f, 1.2f), dark);
        foreach (int side in new[] { -1, 1 })
        {
            GameObject rail = Shape(go.transform, "Receiver guide", PrimitiveType.Cube,
                new Vector3(side * .53f, .15f, 0), new Vector3(.1f, .3f, 1.1f), steel, true);
            // Un guide passif et lisse ; le lanceur fournit l'impulsion contrôlée vers l'avant.
            var material = Rolling();
            rail.GetComponent<Collider>().sharedMaterial = material;
        }
        Shape(go.transform, "Launch arrow stem", PrimitiveType.Cube, new Vector3(0, .025f, .12f), new Vector3(.045f, .012f, .46f), white);
        foreach (int side in new[] { -1, 1 })
        {
            var wing = Shape(go.transform, "Launch arrow wing", PrimitiveType.Cube,
                new Vector3(side * .085f, .025f, .31f), new Vector3(.035f, .012f, .23f), white);
            wing.transform.localRotation = Quaternion.Euler(0, side * 45, 0);
        }
        var title = new GameObject("Launcher label", typeof(TextMeshPro));
        title.transform.SetParent(go.transform, false);
        title.transform.localPosition = new Vector3(0, .03f, -.38f);
        title.transform.localRotation = Quaternion.Euler(90, 0, 0);
        var label = title.GetComponent<TextMeshPro>();
        label.text = "RELANCEUR";
        label.fontSize = .85f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.rectTransform.sizeDelta = new Vector2(.9f, .2f);
    }

    private GameObject Shape(Transform parent, string title, PrimitiveType primitive, Vector3 position, Vector3 size, Material material, bool physical = false)
    {
        GameObject go = GameObject.CreatePrimitive(primitive);
        go.name = title;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = size;
        if (!physical)
        {
            Collider collider = go.GetComponent<Collider>();
            collider.enabled = false;
            if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider);
        }
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private Material Finish(string title, Color color)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Material saved = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(Folder + "/" + title.Replace(" ", "_") + ".mat");
            if (saved != null) return saved;
        }
#endif
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = title, color = color };
        material.SetFloat("_Metallic", .2f);
        material.SetFloat("_Smoothness", .28f);
        Keep(material, title.Replace(" ", "_") + ".mat");
        return material;
    }

    private PhysicsMaterial Rolling()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var saved = UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Folder + "/LauncherGuide.physicMaterial");
            if (saved != null) return saved;
        }
#endif
        var material = new PhysicsMaterial("Auxiliary launcher guide")
        {
            dynamicFriction = 0, staticFriction = 0, bounciness = 0,
            frictionCombine = PhysicsMaterialCombine.Minimum, bounceCombine = PhysicsMaterialCombine.Minimum
        };
        Keep(material, "LauncherGuide.physicMaterial");
        return material;
    }

    private void Keep(UnityEngine.Object resource, string file)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!UnityEditor.AssetDatabase.IsValidFolder(Folder)) UnityEditor.AssetDatabase.CreateFolder("Assets/Materials", "VosgesLauncher");
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
