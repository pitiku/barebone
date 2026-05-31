using System.Collections.Generic;
using UnityEngine;

public class SpritePart : MonoBehaviour
{
    // Variations ( Tags )
    [System.Serializable]
    public class VisualVariation
    {
        public string m_sTag;
        public GameObject m_oVisual;
        public string m_sTagApplied;
    }

    public List<VisualVariation> m_aoVariations = new List<VisualVariation>();

    // Colors
    public List<string> m_aoMaterialParams = new List<string>();
    public List<ColorList> m_aoColorCombinations = new List<ColorList>();
    public ColorList m_oFixedColorCombination;

    public SpritePart m_oColorsFromSpritePart = null;

    List<Color> m_aoColorsChosen = new List<Color>();

    string m_sSaveId = null;

    GameEntity m_oEntity;

    string getSaveId()
    {
        if (m_sSaveId.isNullOrEmpty())
        {
            m_sSaveId = GetComponentInParent<VisualPart>(true).getSaveId();
        }
        return m_sSaveId;
    }

    List<string> getParams()
    {
        if (m_oColorsFromSpritePart != null)
        {
            return m_oColorsFromSpritePart.m_aoMaterialParams;
        }
        else
        {
            return m_aoMaterialParams;
        }
    }

    public void calculateFixed()
    {
        if (m_oFixedColorCombination != null && m_oFixedColorCombination.m_aoColors.Count > 0)
        {
            setColors(m_oFixedColorCombination.m_aoColors);
        }
        else if (m_oColorsFromSpritePart != null)
        {
            setColors(m_oColorsFromSpritePart.m_aoColorsChosen);
        }
        else
        {
            calculateRandoms();
        }
    }

    public void calculateRandoms()
    {
        GameEntity oEntity = getGameEntity();
        // Colors
        if (m_oColorsFromSpritePart != null)
        {
            setColors(m_oColorsFromSpritePart.m_aoColorsChosen);
        }
        else if (m_aoColorCombinations.Count > 0)
        {
            bool bLoadFromSaveData;

            if (GameUtils.isInPreGameScenes())
            {
                bLoadFromSaveData = false;
            }else if (oEntity == null)
            {
                bLoadFromSaveData = true;
            }
            else { bLoadFromSaveData = false; }

            if (bLoadFromSaveData) { chooseColorFromSaveData(); }
            else { chooseRandomColor(); }
        }
    }

    private void chooseRandomColor()
    {
        AvatarManager oAvatarManager = AvatarManager.Instance;

        if (oAvatarManager != null)
        {
            //string sSpritePartID = getGameEntity()?.ID + "_" + name;
            //int iChosen = oAvatarManager.consumeNextColorVariantIndex(sSpritePartID, m_aoColorCombinations.Count);

            //if (iChosen >= 0 && iChosen < m_aoColorCombinations.Count)
            //{
            //    setColors(m_aoColorCombinations[iChosen].m_aoColors);
            //    return;
            //}
        }

        setColors(m_aoColorCombinations.getRandomElement(GameplayManager.COLOR_SEED, "[SpritePart] Select colors").m_aoColors);
    }

    private void chooseColorFromSaveData()
    {
        //if (!DataManager.SaveDataRun.ms_dSharedColorList.ContainsKey(getSaveId()))
        //{
        //    chooseRandomColor();

        //    ColorList oColorList = new ColorList();
        //    oColorList.setColors(m_aoColorsChosen);

        //    DataManager.SaveDataRun.ms_dSharedColorList.Add(getSaveId(), oColorList);
        //    return;
        //}

        //setColors(DataManager.SaveDataRun.ms_dSharedColorList[getSaveId()].m_aoColors);
    }

    public List<Color> getColors()
    {
        return m_aoColorsChosen;
    }

    public void setColors(List<Color> _aoColors)
    {
        m_aoColorsChosen = _aoColors;
        applyColors();
    }

    public void updateColors()
    {
        applyColors();
    }

    void applyColors()
    {
        //if(m_aoColorsChosen.Count > 0) Debug.Log("applyColors: " + name + " -> " + m_aoColorsChosen[0]);

        applyColorsToRenderer(GetComponent<SpriteRenderer>());

        foreach (SpriteRenderer sRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            applyColorsToRenderer(sRenderer);
        }
    }

    public void setFixedColors()
    {
        calculateFixed();
        applyColors();
    }

    void applyColorsToRenderer(SpriteRenderer _oRenderer)
    {
        if(m_aoColorsChosen.Count == 0) { return; }

        string[] asParams = getParams().ToArray();
        int iLength = Mathf.Min(asParams.Length, m_aoColorsChosen.Count);

        for (int iIndex = 0; iIndex < iLength; ++iIndex)
        {
            if(_oRenderer == null) { continue; }

            string sParam = asParams[iIndex];
            Color oColor = m_aoColorsChosen[iIndex];

            if (Application.isPlaying)
            {
                _oRenderer.material.SetColor(sParam, oColor);
            }
            else if (_oRenderer.sharedMaterial != null)
            {
                _oRenderer.sharedMaterial.SetColor(sParam, oColor);
            }
        }
    }

    void OnValidate()
    {
        foreach (ColorList oColors in m_aoColorCombinations)
        {
            while (oColors.m_aoColors.Count < getParams().Count)
            {
                oColors.m_aoColors.Add(Color.yellow);
            }

            if (oColors.m_aoColors.Count > getParams().Count)
            {
                oColors.m_aoColors.RemoveRange(getParams().Count, oColors.m_aoColors.Count - getParams().Count);
            }
        }
    }

    private GameEntity getGameEntity()
    {
        if(m_oEntity == null) { m_oEntity = GetComponentInParent<GameEntity>(true); }
        return m_oEntity;
    }
}
