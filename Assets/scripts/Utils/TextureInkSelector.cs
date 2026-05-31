using UnityEngine;

[ExecuteInEditMode]
public class TextureInkSelector : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Texture2D m_oInkTexture = null;

    //public enum Channel { R, G, B, A }

    //public Channel m_oChannel;
    private Texture2D lastTexture;

    private void Update()
    {
        if (m_oInkTexture != null)
        {
            if (GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Main_Texture") == null)
            {
                lastTexture = m_oInkTexture;
                GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Main_Texture", m_oInkTexture);
            }
            if (lastTexture != m_oInkTexture)
            {
                GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Main_Texture", null);
            }
        }
    }

#endif
}