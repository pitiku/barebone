using System.Collections;
using UnityEngine;

public class AddToDisableList : MonoBehaviour
{
    private void OnEnable()
    {
        // need to wait a frame to avoid disabling objects that are enabled in the same frame the event is triggered
        StartCoroutine(addToDisableListCoroutine());
    }

    private void OnDisable()
    {
        if (GameplayManager.isNull()) { return; }

        GameplayManager.Instance.removeGameobjectToDisable(gameObject);
    }

    IEnumerator addToDisableListCoroutine()
    {
        yield return null;
        GameplayManager.Instance.addGameobjectToDisable(gameObject);
    }
}
