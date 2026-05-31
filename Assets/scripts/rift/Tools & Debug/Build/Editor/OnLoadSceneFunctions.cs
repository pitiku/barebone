using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class OnLoadSceneFunctions
{
    static OnLoadSceneFunctions()
    {
        EditorSceneManager.sceneOpened += onSceneOpened;
    }

    static void onSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (BuildPipeline.isBuildingPlayer)
        {
            buildOptimizations(scene);
            onBuildStuff();
        }
    }

    public static void onBuildStuff()
    {
        BuildVersionController[] aoBuildVersion = GameObject.FindObjectsByType<BuildVersionController>(FindObjectsInactive.Include, 0);
        for (int i = 0; i < aoBuildVersion.Length; i++)
        {
            aoBuildVersion[i].checkVersion(true);
        }
    }

    static void buildOptimizations(Scene scene)
    {
        if (scene.buildIndex != 0)
        {
            List<GameObject> aoRoots = new List<GameObject>();
            scene.GetRootGameObjects(aoRoots);

            GameObject oGame = null;
            GameObject oBase = null;
            foreach (GameObject item in aoRoots)
            {
                if (item.tag == "baseSystems")
                {
                    oBase = item.gameObject;
                }
                if (item.tag == "gameSystems")
                {
                    oGame = item.gameObject;
                }
            }

            if (oGame != null)
            {
                GameObject.DestroyImmediate(oGame);
            }
            if (oBase != null)
            {
                GameObject.DestroyImmediate(oBase);
            }
        }
        //Debug.Log("Build done for scene: " + scene.name);
    }
}