using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class SnowScreenEffect : MonoBehaviour
{
    private const int FlakeCount = 34;
    private const int CanvasOrder = 10000;
    private static readonly string[] GameplayScenes = { "Level_1_recup", "Level_2_test" };
    private static GameObject instance;
    private static Texture2D flakeTexture;

    private Snowflake[] flakes;
    private RectTransform canvasRect;
    private bool flakesPlaced;

    private struct Snowflake
    {
        public RectTransform rect;
        public float fallSpeed;
        public float drift;
        public float frequency;
        public float phase;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
        flakeTexture = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallbacks()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsGameplayScene(scene.name))
        {
            if (instance != null)
                instance.SetActive(false);
            return;
        }

        if (instance == null)
            CreateOverlay();

        instance.SetActive(true);
    }

    private static bool IsGameplayScene(string sceneName)
    {
        for (int i = 0; i < GameplayScenes.Length; i++)
        {
            if (sceneName == GameplayScenes[i])
                return true;
        }

        return false;
    }

    private static void CreateOverlay()
    {
        GameObject root = new GameObject("Snow Screen Effect");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasOrder;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<SnowScreenEffect>();
        DontDestroyOnLoad(root);
        instance = root;
    }

    private void Awake()
    {
        canvasRect = (RectTransform)transform;
        if (flakeTexture == null)
            flakeTexture = CreateFlakeTexture();

        flakes = new Snowflake[FlakeCount];
        for (int i = 0; i < flakes.Length; i++)
        {
            GameObject flakeObject = new GameObject(
                "Snowflake",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            RectTransform rect = (RectTransform)flakeObject.transform;
            rect.SetParent(transform, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            RawImage image = flakeObject.GetComponent<RawImage>();
            image.texture = flakeTexture;
            image.color = new Color(0.91f, 0.97f, 1f, Random.Range(0.16f, 0.38f));
            image.raycastTarget = false;

            float size = Random.Range(3f, 6f);
            rect.sizeDelta = new Vector2(size, size);
            flakes[i] = new Snowflake
            {
                rect = rect,
                fallSpeed = Random.Range(28f, 58f),
                drift = Random.Range(5f, 13f),
                frequency = Random.Range(0.5f, 1.3f),
                phase = Random.Range(0f, Mathf.PI * 2f)
            };
        }

        PlaceFlakesOnScreen();
    }

    private void Update()
    {
        Vector2 bounds = canvasRect.rect.size;
        if (bounds.x <= 0f || bounds.y <= 0f)
            return;

        if (!flakesPlaced)
        {
            PlaceFlakesOnScreen();
            if (!flakesPlaced)
                return;
        }

        float deltaTime = Time.unscaledDeltaTime;
        float halfWidth = bounds.x * 0.5f;
        float halfHeight = bounds.y * 0.5f;

        for (int i = 0; i < flakes.Length; i++)
        {
            Snowflake flake = flakes[i];
            Vector2 position = flake.rect.anchoredPosition;
            position.y -= flake.fallSpeed * deltaTime;
            position.x += Mathf.Sin(Time.unscaledTime * flake.frequency + flake.phase)
                * flake.drift * deltaTime;

            if (position.y < -halfHeight - flake.rect.rect.height)
            {
                position.y = halfHeight + Random.Range(8f, bounds.y * 0.25f);
                position.x = Random.Range(-halfWidth, halfWidth);
            }
            else if (position.x < -halfWidth - flake.rect.rect.width)
            {
                position.x = halfWidth;
            }
            else if (position.x > halfWidth + flake.rect.rect.width)
            {
                position.x = -halfWidth;
            }

            flake.rect.anchoredPosition = position;
        }
    }

    private void PlaceFlakesOnScreen()
    {
        Vector2 bounds = canvasRect.rect.size;
        if (bounds.x <= 0f || bounds.y <= 0f)
            return;

        for (int i = 0; i < flakes.Length; i++)
        {
            flakes[i].rect.anchoredPosition = new Vector2(
                Random.Range(-bounds.x * 0.5f, bounds.x * 0.5f),
                Random.Range(-bounds.y * 0.5f, bounds.y * 0.5f));
        }

        flakesPlaced = true;
    }

    private static Texture2D CreateFlakeTexture()
    {
        const int textureSize = 16;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "Snow Screen Flake",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color[] pixels = new Color[textureSize * textureSize];
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - Mathf.SmoothStep(5f, 7.5f, distance);
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private void OnDestroy()
    {
        if (flakeTexture != null)
            Destroy(flakeTexture);

        flakeTexture = null;
        instance = null;
    }
}
