// Based on http://www.tallior.com/find-missing-references-unity/
// It fixes deprecations and checks for missing references every time a new scene is loaded
// Moreover, it inspects missing references in animators and animation frames

using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class LatestScenes
{
    static string currentScene = null;

    static LatestScenes()
    {
        EditorApplication.hierarchyChanged += hierarchyWindowChanged;
    }

    static void hierarchyWindowChanged()
    {
        if (currentScene != EditorSceneManager.GetSceneAt(EditorSceneManager.sceneCount - 1).name)
        {
            CheckMissingReferences.FindMissingReferencesInCurrentScene();
            currentScene = EditorSceneManager.GetSceneAt(EditorSceneManager.sceneCount - 1).name;
        }
    }
}

public static class CheckMissingReferences
{

    [MenuItem("UPTools/Show Missing Object References in all scenes", false, 82)]
    public static void MissingSpritesInAllScenes()
    {
        foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            var objects = Object.FindObjectsByType<GameObject>(0);
            FindMissingReferences(scene.path, objects);
        }
    }

    [MenuItem("UPTools/Show Missing Object References in scene", false, 84)]
    public static void FindMissingReferencesInCurrentScene()
    {
        var objects = Object.FindObjectsByType<GameObject>(0);
        FindMissingReferences(EditorSceneManager.GetSceneAt(EditorSceneManager.sceneCount - 1).name, objects);
    }

    [MenuItem("UPTools/Show Missing Object References in assets", false, 83)]
    public static void MissingSpritesInAssets()
    {
        var allAssets = AssetDatabase.GetAllAssetPaths();
        var objs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

        FindMissingReferences("Project", objs);
    }

    public static void FindMissingReferences(string sceneName, GameObject[] objects)
    {
        foreach (var go in objects)
        {
            var components = go.GetComponents<Component>();

            foreach (var c in components)
            {
                if (c == null)
                {
                    Deb.logError("null component in " + go.name + ", parent: " + (go.transform.parent ? go.transform.parent.name : ""), go);

                    continue;
                }

                try
                {
                    var so = new SerializedObject(c);
                    var sp = so.GetIterator();

                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                            {
                                ShowError(FullObjectPath(go), sp.name, sceneName);

                                if (!sp.propertyPath.isNullOrEmpty())
                                {
                                    Deb.logWarning("Check the following reference: " + sp.propertyPath);
                                }
                            }
                        }
                    }
                    var animator = c as Animator;
                    if (animator != null)
                    {
                        CheckAnimatorReferences(animator);
                    }
                }
                catch (System.Exception)
                {
                    Deb.log(go.name);
                }
            }
        }
    }

    public static void CheckAnimatorReferences(Animator component)
    {
        if (component.runtimeAnimatorController == null)
        {
            return;
        }
        foreach (AnimationClip ac in component.runtimeAnimatorController.animationClips)
        {
            var so = new SerializedObject(ac);
            var sp = so.GetIterator();

            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                    {
                        Deb.logWarning("Missing reference found in: " + FullObjectPath(component.gameObject) + ", Animation: " + ac.name + ", Property : " + sp.name + ", Scene: " + EditorSceneManager.GetSceneAt(EditorSceneManager.sceneCount - 1).name);
                    }
                }
            }
        }
    }

    static void ShowError(string objectName, string propertyName, string sceneName)
    {
        Deb.logWarning("Missing reference found in: " + objectName + ", Property : " + propertyName + ", Scene: " + sceneName);
    }

    static string FullObjectPath(GameObject go)
    {
        return go.transform.parent == null ? go.name : FullObjectPath(go.transform.parent.gameObject) + "/" + go.name;
    }
}