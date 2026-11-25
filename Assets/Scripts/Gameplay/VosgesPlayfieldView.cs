using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

/// <summary>Full-window playfield and scoreboard, without exposing the surrounding scene.</summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class VosgesPlayfieldView : MonoBehaviour
{
    private const float SidebarFraction = 280f / 1920f;
    private const float PresentationAspect = 16f / 9f;
    private static readonly Color Background = new Color32(9, 24, 33, 255);
    [SerializeField] private Transform table;
    [SerializeField] private VosgesHUDController hud;
    private Camera gameCamera;
    private UniversalAdditionalCameraData pipelineCamera;

    public static bool EnsureForScene(Scene scene)
    {
        Camera camera = null;
        Transform board = null;
        VosgesHUDController scoreboard = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera candidate in root.GetComponentsInChildren<Camera>(true))
                if (candidate.CompareTag("MainCamera")) camera = candidate;
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == "BaseLvl_1") board = item;
            VosgesHUDController found = root.GetComponentInChildren<VosgesHUDController>(true);
            if (found != null) scoreboard = found;
        }
        if (camera == null || board == null || scoreboard == null) return false;
        var view = camera.GetComponent<VosgesPlayfieldView>();
        bool changed = view == null;
        if (view == null) view = camera.gameObject.AddComponent<VosgesPlayfieldView>();
        if (view.table != board || view.hud != scoreboard) changed = true;
        view.table = board;
        view.hud = scoreboard;
        if (SceneManager.GetActiveScene() == scene)
        {
            Color ambient = new Color(.23f, .23f, .23f, 1);
            if (RenderSettings.ambientMode != UnityEngine.Rendering.AmbientMode.Flat
                || RenderSettings.ambientLight != ambient || RenderSettings.reflectionIntensity != .18f)
            {
                // Neutral indoor fill avoids the blue sky washing over the snow and steel.
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = ambient;
                RenderSettings.reflectionIntensity = .18f;
                changed = true;
            }
        }
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Light light in root.GetComponentsInChildren<Light>(true))
            {
                if (light.type != LightType.Directional) continue;
                if (light.color != Color.white || light.useColorTemperature || !Mathf.Approximately(light.intensity, 1.7f))
                {
                    light.color = Color.white;
                    light.useColorTemperature = false;
                    light.intensity = 1.7f;
                    changed = true;
                }
            }
        changed |= view.HideExterior(scene);
        changed |= view.Apply();
        return changed;
    }

    private void OnEnable() { gameCamera = GetComponent<Camera>(); Apply(); }
    private void LateUpdate() { Apply(); }

    private bool Apply()
    {
        if (table == null || hud == null) return false;
        if (gameCamera == null) gameCamera = GetComponent<Camera>();
        bool changed = false;
        float screenAspect = Screen.width / (float)Mathf.Max(1, Screen.height);
        Rect frame = new Rect(0, 0, 1, 1);
        if (screenAspect > PresentationAspect)
        {
            frame.width = PresentationAspect / screenAspect;
            frame.x = (1 - frame.width) * .5f;
        }
        else
        {
            frame.height = screenAspect / PresentationAspect;
            frame.y = (1 - frame.height) * .5f;
        }
        Rect playArea = new Rect(frame.x, frame.y, frame.width * (1 - SidebarFraction), frame.height);
        if (gameCamera.rect != playArea) { gameCamera.rect = playArea; changed = true; }
        if (!gameCamera.orthographic) { gameCamera.orthographic = true; changed = true; }
        if (gameCamera.clearFlags != CameraClearFlags.SolidColor)
        { gameCamera.clearFlags = CameraClearFlags.SolidColor; changed = true; }
        if (gameCamera.backgroundColor != Background) { gameCamera.backgroundColor = Background; changed = true; }
        if (RenderSettings.fog) { RenderSettings.fog = false; changed = true; }
        if (gameCamera.allowDynamicResolution) { gameCamera.allowDynamicResolution = false; changed = true; }
        if (!gameCamera.allowMSAA) { gameCamera.allowMSAA = true; changed = true; }
        if (pipelineCamera == null) pipelineCamera = GetComponent<UniversalAdditionalCameraData>();
        if (pipelineCamera != null)
        {
            // Preserve the original volume's tone mapping and lighting balance.
            if (!pipelineCamera.renderPostProcessing) { pipelineCamera.renderPostProcessing = true; changed = true; }
            if (pipelineCamera.antialiasing != AntialiasingMode.None)
            { pipelineCamera.antialiasing = AntialiasingMode.None; changed = true; }
        }

        // The existing camera looks perpendicular to this inclined playfield. Fit its
        // full playable rectangle without stretching the ball or the obstacles.
        // The board's transform has a stretched model scale, so use its rotation only.
        Matrix4x4 view = gameCamera.worldToCameraMatrix;
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
        for (int horizontal = 0; horizontal < 2; horizontal++)
            for (int longitudinal = 0; longitudinal < 2; longitudinal++)
            {
                float x = horizontal == 0 ? -6.46f : 6.46f;
                float z = longitudinal == 0 ? -15.05f : 7.15f;
                Vector3 point = view.MultiplyPoint3x4(table.position + table.rotation * new Vector3(x, 0, z));
                minX = Mathf.Min(minX, point.x); maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y); maxY = Mathf.Max(maxY, point.y);
            }
        float centerX = (minX + maxX) * .5f, centerY = (minY + maxY) * .5f;
        float playAspect = PresentationAspect * (1 - SidebarFraction);
        float viewHeight = Mathf.Max(maxY - minY, (maxX - minX) / playAspect);
        float viewWidth = viewHeight * playAspect;
        minX = centerX - viewWidth * .5f; maxX = centerX + viewWidth * .5f;
        minY = centerY - viewHeight * .5f; maxY = centerY + viewHeight * .5f;
        gameCamera.orthographicSize = viewHeight * .5f;
        gameCamera.projectionMatrix = Matrix4x4.Ortho(minX, maxX, minY, maxY,
            gameCamera.nearClipPlane, gameCamera.farClipPlane);
        // Keep visibility culling consistent with the off-centre projection.
        gameCamera.cullingMatrix = gameCamera.projectionMatrix * gameCamera.worldToCameraMatrix;

        var canvas = hud.transform as RectTransform;
        if (canvas == null) return changed;
        RectTransform backdrop = hud.transform.Find("GameSidebarBackground") as RectTransform;
        if (backdrop == null)
        {
            var go = new GameObject("GameSidebarBackground", typeof(RectTransform), typeof(Image));
            backdrop = go.GetComponent<RectTransform>();
            backdrop.SetParent(hud.transform, false);
            backdrop.SetAsFirstSibling();
            Image image = go.GetComponent<Image>();
            image.color = Background;
            image.raycastTarget = false;
            changed = true;
        }
        backdrop.anchorMin = new Vector2(frame.x + frame.width * (1 - SidebarFraction), frame.y);
        backdrop.anchorMax = new Vector2(frame.xMax, frame.yMax);
        backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
        changed |= Mask("Frame top", new Vector2(0, frame.yMax), Vector2.one);
        changed |= Mask("Frame bottom", Vector2.zero, new Vector2(1, frame.y));
        changed |= Mask("Frame left", new Vector2(0, frame.y), new Vector2(frame.x, frame.yMax));
        changed |= Mask("Frame right", new Vector2(frame.xMax, frame.y), new Vector2(1, frame.yMax));
        RectTransform panel = hud.transform.Find("Vosges_Cabinet") as RectTransform;
        if (panel != null && canvas.rect.width > 0 && canvas.rect.height > 0)
        {
            float sidebarWidth = canvas.rect.width * frame.width * SidebarFraction;
            float scale = Mathf.Min(canvas.rect.height * frame.height / 1040f, sidebarWidth / 280f);
            panel.anchorMin = panel.anchorMax = new Vector2(frame.xMax, frame.center.y);
            panel.anchoredPosition = new Vector2(-sidebarWidth * .5f, 0);
            panel.localScale = Vector3.one * scale;
        }
        return changed;
    }

    private bool Mask(string title, Vector2 min, Vector2 max)
    {
        RectTransform rect = hud.transform.Find(title) as RectTransform;
        bool created = rect == null;
        if (created)
        {
            var go = new GameObject(title, typeof(RectTransform), typeof(Image));
            rect = go.GetComponent<RectTransform>();
            rect.SetParent(hud.transform, false);
            rect.SetAsFirstSibling();
            var image = go.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return created;
    }

    private bool HideExterior(Scene scene)
    {
        bool changed = false;
        Quaternion inverse = Quaternion.Inverse(table.rotation);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<VosgesCabinet>() != null)
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    string title = renderer.gameObject.name;
                    if (!title.StartsWith("Flipper button") && !title.StartsWith("Coin ")
                        && !title.StartsWith("Service door") && !title.StartsWith("Ball plunger")) continue;
                    if (renderer.enabled) { renderer.enabled = false; changed = true; }
                }
                continue;
            }
            if (!root.name.EndsWith("Winter resort")) continue;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Vector3 local = inverse * (renderer.bounds.center - table.position);
                if (Mathf.Abs(local.x) <= 6.25f && local.z >= -14.45f && local.z <= 6.37f) continue;
                if (renderer.enabled) { renderer.enabled = false; changed = true; }
            }
        }
        return changed;
    }

    private void OnDisable()
    {
        if (gameCamera == null) return;
        gameCamera.ResetProjectionMatrix();
        gameCamera.ResetCullingMatrix();
    }
}
