using UnityEngine;

public class InitDisableObjectsTags : MonoBehaviour
{
    private void Awake()
    {
        initTags();
    }

    private void initTags()
    {
        Tag[] aoTags = FindObjectsByType<Tag>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Tag itTag in aoTags)
        {
            GameplayManager.Instance.addTag(itTag);
        }
    }
}
