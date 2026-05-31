using UnityEngine;

public static class MaterialUtils
{
    //Sets the material instance for the renderer and destroys the previous one. Use _bSetSharedMaterial when _oMaterial must not be instantiated (for example, swaping it back)
    public static void replaceMaterialInstance(this Renderer _oRenderer, Material _oMaterial, bool _bSetSharedMaterial = false)
    {
        UnityEngine.Object.Destroy(_oRenderer.material);
        if (_bSetSharedMaterial)
        {
            _oRenderer.sharedMaterial = _oMaterial;
        }
        else
        {
            _oRenderer.material = _oMaterial;
        }
    }
}