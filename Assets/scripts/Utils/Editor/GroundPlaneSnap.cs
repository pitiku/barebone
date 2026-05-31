using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GroundPlaneSnap
{
    private static bool isEnabled = true;

    static GroundPlaneSnap()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!isEnabled) return;

        Event e = Event.current;

        // Check if we're dragging from project to scene
        if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
        {
            // Create a horizontal plane at Y=0
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            // Raycast from mouse position to the plane
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            float hitDistance;

            if (groundPlane.Raycast(ray, out hitDistance))
            {
                Vector3 hitPoint = ray.GetPoint(hitDistance);

                // Force Y to 0
                hitPoint.y = 0f;

                // You can also round to grid if needed
                // hitPoint.x = Mathf.Round(hitPoint.x);
                // hitPoint.z = Mathf.Round(hitPoint.z);

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        GameObject prefab = draggedObject as GameObject;
                        if (prefab != null)
                        {
                            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                            if (instance != null)
                            {
                                instance.transform.position = hitPoint;
                                Undo.RegisterCreatedObjectUndo(instance, "Place " + instance.name);
                                Selection.activeGameObject = instance;
                            }
                        }
                    }

                    e.Use();
                }
            }
        }
    }
}