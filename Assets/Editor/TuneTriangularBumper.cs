using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TuneTriangularBumper
{
    private const string LevelOneScenePath = "Assets/Scenes/Level_1_recup.unity";
    private const string LevelTwoScenePath = "Assets/Scenes/Level_2_test.unity";
    private const string BumperName = "triangular_bumper (1)";
    private const float ReducedBounceForce = 1f;

    [MenuItem("Tools/Flipper/Adoucir triangular_bumper (1)")]
    public static void Apply()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded
            || (activeScene.path != LevelOneScenePath && activeScene.path != LevelTwoScenePath))
            throw new InvalidOperationException("Ouvre Level_1_recup ou Level_2_test avant d'appliquer le changement.");

        string otherPath = activeScene.path == LevelOneScenePath ? LevelTwoScenePath : LevelOneScenePath;
        Scene otherScene = SceneManager.GetSceneByPath(otherPath);
        bool openedOtherScene = false;
        try
        {
            if (!otherScene.IsValid() || !otherScene.isLoaded)
            {
                otherScene = EditorSceneManager.OpenScene(otherPath, OpenSceneMode.Additive);
                openedOtherScene = true;
            }

            ValidateScene(activeScene);
            ValidateScene(otherScene);
            UpdateScene(activeScene);
            UpdateScene(otherScene);

            if (!EditorSceneManager.SaveScene(activeScene))
                throw new InvalidOperationException("Unity n'a pas pu enregistrer " + activeScene.path + ".");
            if (!EditorSceneManager.SaveScene(otherScene))
                throw new InvalidOperationException("Unity n'a pas pu enregistrer " + otherScene.path + ".");
        }
        finally
        {
            if (openedOtherScene && otherScene.IsValid() && otherScene.isLoaded)
                EditorSceneManager.CloseScene(otherScene, false);
        }
    }

    private static void ValidateScene(Scene scene)
    {
        Transform bumper = FindUnique(scene);
        if (bumper.GetComponentsInChildren<BumperController>(true).Length == 0)
            throw new InvalidOperationException("BumperController introuvable sous " + BumperName + " dans " + scene.name + ".");
    }

    private static void UpdateScene(Scene scene)
    {
        Transform bumper = FindUnique(scene);
        BumperController[] controllers = bumper.GetComponentsInChildren<BumperController>(true);
        Undo.RegisterFullObjectHierarchyUndo(bumper.gameObject, "Adoucir le rebond du bumper triangulaire");
        foreach (BumperController controller in controllers)
        {
            Undo.RecordObject(controller, "Adoucir le rebond du bumper triangulaire");
            controller.bounceForce = ReducedBounceForce;
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            EditorUtility.SetDirty(controller);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log(
            BumperName + " dans " + scene.name + ": rebond reduit a "
            + ReducedBounceForce + " (" + controllers.Length + " BumperController).",
            bumper);
    }

    private static Transform FindUnique(Scene scene)
    {
        Transform result = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                if (item.name != BumperName) continue;
                if (result != null)
                    throw new InvalidOperationException("Nom ambigu dans " + scene.name + ": " + BumperName);
                result = item;
            }

        if (result == null)
            throw new InvalidOperationException("Objet introuvable dans " + scene.name + ": " + BumperName);
        return result;
    }
}
