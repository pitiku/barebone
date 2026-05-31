using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class VisualPart : MonoBehaviour
{
    public List<SpritePart> m_aoSpriteParts = new List<SpritePart>();
    public SpritePart m_oFixedSpritePart;

    public int m_iPriority = 1;

    [System.NonSerialized, ShowInInspector] string m_sSaveId = "";

    [System.NonSerialized] public int m_iSelectedIndex = 0;
    [System.NonSerialized] public bool m_bIsFixed;
    [System.NonSerialized] public SpritePart m_oSelected = null;

    public void calculateFixed()
    {
        if (m_oFixedSpritePart != null)
        {
            m_bIsFixed = true;
            m_iSelectedIndex = -1;
            m_oSelected = m_oFixedSpritePart;

            initializeVisualPart();
            m_oSelected.calculateFixed();
        }
        else
        {
            calculateRandoms(true);
        }
    }

    public void calculateRandoms(bool _bTryFixed)
    {
        m_bIsFixed = false;

        string sVisualPartID = getSaveId();

        if (m_iSelectedIndex + 1 > m_aoSpriteParts.Count)
        {
            Deb.logWarning(GetComponentInParent<GameEntity>().name + " has error in visualPart: " + name);
            m_iSelectedIndex = m_aoSpriteParts.Count - 1; // fallback to last index
        }
        m_oSelected = m_aoSpriteParts[m_iSelectedIndex];

        initializeVisualPart();

        if (_bTryFixed) { m_oSelected.calculateFixed(); }
        else { m_oSelected.calculateRandoms(); }
    }

    public void selectSpritePart(eVisualType _eType)
    {
        if (m_aoSpriteParts.Count == 0) return;

        if (_eType == eVisualType.Primary || m_aoSpriteParts.Count == 1) 
        { 
            m_iSelectedIndex = 0; 
        }
        else
        {
                //int iStartingIndex = _eType == eVisualType.Secondary ? 1 : 0;
                int iStartingIndex = 0;
                AvatarManager oAvatarManager = AvatarManager.Instance;
                if (oAvatarManager != null)
                {
                    string sSpritePartKey = getSpritePartRandomKey();
                    int iChosenFromBag = oAvatarManager.consumeNextSpritePartVariantIndex(sSpritePartKey, m_aoSpriteParts.Count);
                    if (iChosenFromBag >= 0)
                    {
                        m_iSelectedIndex = Mathf.Min(m_aoSpriteParts.Count - 1, iChosenFromBag + iStartingIndex);
                    }
                    else
                    {
                        m_iSelectedIndex = RandomUtils.rangeInt(new Vector2(iStartingIndex, m_aoSpriteParts.Count - 1), GameplayManager.COLOR_SEED, "[VisualPart] Select sprite part index");
                    }
                }
                else
                {
                    m_iSelectedIndex = RandomUtils.rangeInt(new Vector2(iStartingIndex, m_aoSpriteParts.Count - 1), GameplayManager.COLOR_SEED, "[VisualPart] Select sprite part index");
                }
        }

        calculateRandoms(false);
    }

    string getSpritePartRandomKey()
    {
        GameEntity oEntity = GetComponentInParent<GameEntity>(true);
        string sEntityKey = "";// oEntity == null ? "NO_ENTITY" : oEntity.ID;
        return $"{sEntityKey}_{name}";
    }

    public void setData(VisualEntity.SaveData _oSaveData, eVisualType _eVisualType)
    {
        //Debug.Log("setData: " + name + " -> " + _eVisualType + " - " + getSaveId() + " <> " + _oSaveData.m_aoSpriteColors[0].m_aoColors[0]);

        int iIndexInList = _oSaveData.m_asSpriteIndexId.IndexOf(getSaveId());

        if(m_sSaveId.isNullOrEmpty()) { updateID(); }

        if(iIndexInList == -1)
        {
            selectSpritePart(_eVisualType);
        }

        if (iIndexInList >= 0)
        {
            m_iSelectedIndex = _oSaveData.m_aiSpriteIndex[iIndexInList];
            m_oSelected = m_iSelectedIndex == -1 ? m_oFixedSpritePart : m_aoSpriteParts[m_iSelectedIndex];
        }

        initializeVisualPart();

        iIndexInList = _oSaveData.m_asSpriteColorsId.IndexOf(getSaveId());
        if (iIndexInList >= 0 && m_iSelectedIndex != -1)
        {
            try
            {
                List<Color> aoColors = _oSaveData.m_aoSpriteColors[iIndexInList].m_aoColors;
                m_oSelected.setColors(aoColors);
            }
            catch (System.Exception e)
            {
                Deb.logWarning("Error in saving empty colors (" + name + "): " + e);
            }
        }
        else if(m_oSelected != null)
        {
            m_oSelected.setFixedColors();
        }
    }

    void initializeVisualPart()
    {
        foreach (SpritePart oSpritePart in m_aoSpriteParts)
        {
            oSpritePart.gameObject.SetActive(oSpritePart == m_oSelected);
        }

        if (m_oFixedSpritePart != null)
        {
            m_oFixedSpritePart.gameObject.SetActive(m_oSelected == m_oFixedSpritePart);
        }
    }

    public void updateColors()
    {
        if (m_oSelected != null)
        {
            m_oSelected.updateColors();
        }
    }

    [Button("ID")]
    public void updateID()
    {
        if (m_aoSpriteParts == null)
        {
            Deb.logWarning("missing spriteParts in " + gameObject.name);
            return;
        }

        GameEntity oEntity = GetComponentInParent<GameEntity>(true);

        if(oEntity == null)
        {
            if (m_aoSpriteParts.Count > 0)
            {
                m_sSaveId = name + "_" + m_aoSpriteParts[0].name + "_" + Random.Range(0, 100000).ToString();
            }
            else
            {
                m_sSaveId = name + "_" + Random.Range(0, 100000).ToString();
            }
        }
        else
        {
            updateID(oEntity);
        }
    }

    public void updateID(GameEntity _oEntity)
    {
        if (m_aoSpriteParts == null)
        {
            Deb.logWarning("missing spriteParts in " + gameObject.name);
            return;
        }

        if (m_aoSpriteParts.Count > 0)
        {
            string sName = name.Replace("(Clone)", "");
            string sSpritePartName = m_aoSpriteParts[0].name.Replace("(Clone)", "");

            m_sSaveId = $"{sName}_{sSpritePartName}";
        }
        else
        {
            //m_sSaveId = $"{name}_{_oEntity.getName()}";
        }
    }

    public string getSaveId()
    {
        if (m_sSaveId.isNullOrEmpty())
        {
            updateID();
        }
        return m_sSaveId;
    }
}
