using System.Collections.Generic;
using UnityEngine;

public enum eAvatarPart
{
    HEAD = 0,
    WEAPON = 1,
    ROBES = 2
}

public class Avatar : VisualEntity
{
    public SpriteRenderer m_oEmitterSprite;

    public Transform m_oHeadRoot;
    public Transform m_oBodyRoot;
    public Transform m_oRobesRoot;
    public Transform m_oWeaponsRoot;
    public Transform m_oUnflipRoot;
    public Transform m_oFXsWithMovementRoot;

    public int iLayer;
    public uint iLightLayer;

    bool m_bInitialized;

    public override void updateVisuals(GameEntity _oEntity)
    {
        base.updateVisuals(_oEntity);
    }

    Transform getRoot(eAvatarPart _ePart)
    {
        switch (_ePart)
        {
            case eAvatarPart.WEAPON: return m_oWeaponsRoot;
            case eAvatarPart.ROBES: return m_oRobesRoot;
        }

        return null;
    }

    public Bounds? getBounds()
    {
        Bounds? oBounds = null;
        SpriteRenderer[] aoSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(false);
        foreach (SpriteRenderer oSprite in aoSpriteRenderers)
        {
            if (oSprite.enabled)
            {
                Bounds oSpriteBounds = oSprite.bounds;
                oBounds = oBounds?.getUnion(oSpriteBounds) ?? oSpriteBounds;
            }
        }
        return oBounds;
    }

    public List<Animator> stopAnimation()
    {
        List<Animator> aoDisabledAnimators = new();

        if (gameObject.tryGetComponentInChildren(out Animator oAnimator))
        {
            if (oAnimator.enabled)
            {
                oAnimator.enabled = false;
                aoDisabledAnimators.Add(oAnimator);
            }
        }

        return aoDisabledAnimators;
    }

    public List<ParticleSystem> stopParticles()
    {
        List<ParticleSystem> aoDisabledParticleSystems = new();

        ParticleSystem[] aoParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(false);
        foreach (ParticleSystem oParticleSystem in aoParticleSystems)
        {
            if (oParticleSystem.isPlaying)
            {
                oParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                aoDisabledParticleSystems.Add(oParticleSystem);
            }
        }

        return aoDisabledParticleSystems;
    }

    public List<UnityEngine.VFX.VisualEffect> stopVisualEffects()
    {
        List<UnityEngine.VFX.VisualEffect> aoDisabledVisualEffects = new();

        UnityEngine.VFX.VisualEffect[] aoVisualEffects = gameObject.GetComponentsInChildren<UnityEngine.VFX.VisualEffect>(false);
        foreach (UnityEngine.VFX.VisualEffect oVisualEffect in aoVisualEffects)
        {
            if (oVisualEffect.enabled)
            {
                oVisualEffect.enabled = false;
                aoDisabledVisualEffects.Add(oVisualEffect);
            }
        }

        return aoDisabledVisualEffects;
    }

    //public void equipItem(eAvatarPart _oPart, Equipment _oItem, PartyCharacterData _oCharacter)
    //{
    //    //UnityEngine.Debug.Log("equipItem: " + _oPart + ", " + _oItem + ", " + _oCharacter);

    //    Transform oRoot = getRoot(_oPart);
    //    if (_oItem.m_oVisual != null && oRoot != null)
    //    {
    //        var oGameEntity = GetComponentInParent<GameEntity>(true);
    //        if (oGameEntity != null && oGameEntity.m_oVisual != null)
    //        {
    //            oGameEntity.m_oVisual.removeGameObject(oRoot.gameObject, false);
    //            onDestroyItem(oRoot.gameObject);
    //        }
    //        // remove previous items
    //        oRoot.destroyChildren();

    //        GameObject oInstance = Instantiate(_oItem.m_oVisual, oRoot);

    //        if (oGameEntity != null && oGameEntity.m_oVisual != null)
    //        {
    //            oGameEntity.m_oVisual.addGameObject(oInstance, false);
    //            // We need to set all the Visual Parts
    //            foreach(VisualPart oPart in oInstance.GetComponentsInChildren<VisualPart>())
    //            {
    //                if(oPart != null)
    //                {
    //                    addItemVisualData(oPart, oGameEntity);
    //                }
    //            }
    //        }
    //        updateVisuals(oGameEntity);
    //    }
    //}

    public void setLayers(int _iLayer, uint _iLightLayer)
    {
        m_bInitialized = true;
        iLayer = _iLayer;
        iLightLayer = _iLightLayer;
        updateLayers();
    }

    public void updateLayers()
    {
        if (m_bInitialized)
        {
            transform.setLayer(iLayer);
            transform.setLightLayer(iLightLayer);
            GameEntity oEntity = GetComponentInParent<GameEntity>(true);
            //if (oEntity != null && oEntity.m_oVisual != null)
            //{
            //    oEntity.m_oVisual.findRenders(false);
            //}
        }
    }
}