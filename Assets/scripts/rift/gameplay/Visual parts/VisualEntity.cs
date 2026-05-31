using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum eVisualType
{
    Primary = 0,
    Secondary = 1,
    Tertiary = 2
}

public class VisualEntity : MonoBehaviour
{
    [System.Serializable]
    public class StatusObjects
    {
        public GameObject m_oObject;
        public List<GameObject> m_aoRandomVariantObjects = new();
        public List<string> m_aoMaterialParams = new List<string>();
        public List<ColorList> m_aoColorCombinations = new List<ColorList>();
        public int m_iChosenCombination = -1;

        public Color? getColor(string _sParam)
        {
            if(m_iChosenCombination == -1)
            {
                m_iChosenCombination = RandomUtils.rangeInt(new Vector2(0, m_aoColorCombinations.Count - 1), GameplayManager.COLOR_SEED, "[StatusObjects] Select color combination index");
            }
            int iIndex = m_aoMaterialParams.IndexOf(_sParam);
            if(iIndex > -1 && m_aoColorCombinations[m_iChosenCombination].m_aoColors.Count > iIndex)
            {
                return m_aoColorCombinations[m_iChosenCombination].m_aoColors[iIndex];
            }
            return null;
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public List<string> m_asSpriteIndexId = new List<string>();
        public List<int> m_aiSpriteIndex = new List<int>();

        public List<string> m_asSpriteColorsId = new List<string>();
        public List<ColorList> m_aoSpriteColors = new List<ColorList>();
    }

    public SaveData m_oLastVisualSaveData;

    public List<SpriteRenderer> m_aoExcludeFacingSpriteRenderers = new List<SpriteRenderer>();
    public List<MeshRenderer> m_aoExcludeFacingShadows = new List<MeshRenderer>();

    public List<StatusObjects> m_oaStatusObjects = new();
    private StatusObjects m_oCurrentStatus = null;

    public SaveData calculateRandoms(eVisualType _eType)
    {
        foreach (VisualPart oVisualPart in getVisualParts())
        {
            oVisualPart.selectSpritePart(_eType);
        }

        return createSaveData();
    }

    public SaveData calculateFixed()
    {
        foreach (VisualPart oVisualPart in getVisualParts())
        {
            oVisualPart.calculateFixed();
        }

        return createSaveData();
    }

    private SaveData createSaveData()
    {
        m_oLastVisualSaveData = new SaveData();

        foreach (VisualPart oVisualPart in getVisualParts())
        {
            m_oLastVisualSaveData.m_asSpriteIndexId.Add(oVisualPart.getSaveId());
            m_oLastVisualSaveData.m_aiSpriteIndex.Add(oVisualPart.m_iSelectedIndex);

            if (oVisualPart.m_oSelected.getColors().Count > 0)
            {
                m_oLastVisualSaveData.m_asSpriteColorsId.Add(oVisualPart.getSaveId());
                ColorList oColorList = new ColorList();
                oColorList.setColors(oVisualPart.m_oSelected.getColors());
                m_oLastVisualSaveData.m_aoSpriteColors.Add(oColorList);
            }
        }

        return m_oLastVisualSaveData;
    }

    public void addItemVisualData(VisualPart _oVisualPart, GameEntity _oEntity)
    {
        eVisualType eVisualType = _oEntity.getVisualType();

        if (eVisualType == eVisualType.Primary)
        {
            _oVisualPart.calculateFixed();
        }
        else
        {
            _oVisualPart.selectSpritePart(eVisualType);
        }
    }

    public void setData(SaveData _oSaveData, GameEntity _oEntity)
    {
        if (_oSaveData == null) { return; }

        m_oLastVisualSaveData = _oSaveData;

        foreach (VisualPart oVisualPart in getVisualParts())
        {
            oVisualPart.setData(_oSaveData, _oEntity.getVisualType());
        }

        updateVisuals(_oEntity);
    }

    public void setIDs(GameEntity _oEntity)
    {
        foreach (VisualPart oVisualPart in getVisualParts())
        {
            oVisualPart.updateID(_oEntity);
        }
    }

    public List<VisualPart> getVisualParts()
    {
        List<VisualPart> aoVisualParts = GetComponentsInChildren<VisualPart>().toList();
        aoVisualParts.Sort((a, b) => b.m_iPriority.CompareTo(a.m_iPriority));
        return aoVisualParts;
    }

    public virtual void updateVisuals(GameEntity _oEntity)
    {
        m_oCurrentStatus = null;

        foreach(VisualPart visualPart in getVisualParts())
        {
            visualPart.updateColors();
        }
    }

    public Color? getOverrideColor(string _sParam)
    {
        if (m_oCurrentStatus != null)
        {
            return m_oCurrentStatus.getColor(_sParam);
        }
        return null;
    }

    public void calculate(GameEntity _oEntity)
    {
        eVisualType _eVisualType = _oEntity.getVisualType();

        if (_eVisualType == eVisualType.Primary)
        {
            calculateFixed();
        }
        else
        {
            calculateRandoms(_eVisualType);
        }

        updateVisuals(_oEntity);
    }

    public void updateFacing(bool _bFaceRight)
    {
        SpriteRenderer[] aoSpritesR = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < aoSpritesR.Length; i++)
        {
            if (!m_aoExcludeFacingSpriteRenderers.Contains(aoSpritesR[i]))
            {
                if (aoSpritesR[i].flipX != !_bFaceRight)
                {
                    aoSpritesR[i].flipX = !_bFaceRight;
                    aoSpritesR[i].transform.localPosition = new Vector3(aoSpritesR[i].transform.localPosition.x * -1, aoSpritesR[i].transform.localPosition.y, aoSpritesR[i].transform.localPosition.z);
                }
            }
        }        
        void tryFlip(Transform _oTransform)
        {
            bool bFacingRight = _oTransform.localScale.x > 0;
            if (bFacingRight != _bFaceRight)
            {
                Vector3 v3FlipFactor = new Vector3(-1f, 1f, 1f);
                _oTransform.localScale = Vector3.Scale(_oTransform.localScale, v3FlipFactor);
                _oTransform.localPosition = Vector3.Scale(_oTransform.localPosition, v3FlipFactor);
            }
        }

        MeshRenderer[] aoShadows = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < aoShadows.Length; i++)
        {
            if (!m_aoExcludeFacingShadows.Contains(aoShadows[i]))
            {
                tryFlip(aoShadows[i].transform);
            }
        }

        VisualEffect[] aoVisualEffects = GetComponentsInChildren<VisualEffect>();
        foreach(VisualEffect oVisualEffect in aoVisualEffects)
        {
            tryFlip(oVisualEffect.transform);
        }
    }
}
