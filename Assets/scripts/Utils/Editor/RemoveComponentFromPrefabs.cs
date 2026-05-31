using UnityEditor;
using UnityEngine;

public class RemoveComponentFromPrefabs : EditorWindow
{
    private string componentToRemove;

    [MenuItem("UPTools/Remove Component From Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<RemoveComponentFromPrefabs>("Remove Component");
    }

    private void OnGUI()
    {
        GUILayout.Label("Eliminar Componente de Prefabs", EditorStyles.boldLabel);
        componentToRemove = EditorGUILayout.TextField("Nombre del Componente", componentToRemove);

        if (GUILayout.Button("Eliminar Componente"))
        {
            RemoveComponent();
        }
    }

    private void RemoveComponent()
    {
        if (string.IsNullOrEmpty(componentToRemove))
        {
            Deb.logError("Por favor, especifica el nombre del componente.");
            return;
        }

        // Encuentra todos los prefabs en el proyecto
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
        for (int i = 0; i<prefabPaths.Length; i++)
        {
            string prefabPath = prefabPaths[i];
            string path = AssetDatabase.GUIDToAssetPath(prefabPath);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                bool bSave = false;
                // Busca todos los componentes con el nombre especificado en el prefab y sus hijos
                Component[] components = prefab.GetComponentsInChildren(typeof(Component), true);
                for (int i1 = 0; i1<components.Length; i1++)
                {
                    Component component = components[i1];
                    if (component && component.GetType().Name == componentToRemove)
                    {
                        bSave = true;
                        DestroyImmediate(component, true);
                        Debug.Log($"Componente {componentToRemove} eliminado del prefab: {prefab.name}");
                    }
                }

                // Guarda los cambios en el prefab
                if (bSave) PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        Debug.Log("Proceso completado.");
    }
}
