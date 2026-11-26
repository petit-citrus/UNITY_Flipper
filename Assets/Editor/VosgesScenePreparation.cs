using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Matérialise la présentation une seule fois sous forme d'objets de scène modifiables.</summary>
[InitializeOnLoad]
public static class VosgesScenePreparation
{
    static VosgesScenePreparation()
    {
        EditorApplication.delayCall += PrepareLoadedScenes;
        EditorSceneManager.sceneOpened += SceneOpened;
        EditorApplication.playModeStateChanged += PlayModeChanged;
    }

    private static void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += PrepareLoadedScenes;
    }

    private static void SceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += PrepareLoadedScenes;
    }

    [MenuItem("Tools/Ski Pinball/Preparar visual no editor")]
    public static void PrepareLoadedScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall -= PrepareLoadedScenes;
            EditorApplication.delayCall += PrepareLoadedScenes;
            return;
        }
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || (scene.path != "Assets/Scenes/Level_1_recup.unity"
                && scene.path != "Assets/Scenes/Level_2_test.unity")) continue;
            bool changed = false;
            changed |= VosgesLevelTwoCourse.EnsureForScene(scene);
            changed |= VosgesGameplaySetup.EnsureForScene(scene);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (VosgesHUDController hud in root.GetComponentsInChildren<VosgesHUDController>(true))
                {
                    if (!VosgesScoreboard.Build(hud)) continue;
                    EditorUtility.SetDirty(hud);
                    changed = true;
                }
                foreach (VosgesScenery scenery in root.GetComponentsInChildren<VosgesScenery>(true))
                {
                    if (!scenery.EnsureScenery()) continue;
                    EditorUtility.SetDirty(scenery);
                    changed = true;
                }
            }
            changed |= VosgesLevelOneObstacles.EnsureForScene(scene);
            changed |= VosgesCornerSafety.EnsureForScene(scene);
            changed |= VosgesCabinet.EnsureForScene(scene);
            changed |= VosgesFlipperSkin.EnsureForScene(scene);
            changed |= VosgesPlayfieldView.EnsureForScene(scene);
            if (changed) EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
