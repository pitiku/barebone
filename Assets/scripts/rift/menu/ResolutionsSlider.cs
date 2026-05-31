using System;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionsSlider : SliderPlus
{
//#if UNITY_STANDALONE || UNITY_EDITOR
//    public override void Awake()
//    {
//        fillResolutionList(false);
//        base.Awake();
//    }

//    public override void OnEnable()
//    {
//        fillResolutionList(false);
//        base.OnEnable();
//        slider.value = GameOptionsManager.Instance.m_oResolutionManager.getResolutionIndex();
//        GameOptionsManager.Instance.m_oResolutionManager.m_oExternalScreenResolutionChangeEvent += onExternalResolutionChanged;
//    }

//    public void OnDisable()
//    {
//        GameOptionsManager.Instance.m_oResolutionManager.m_oExternalScreenResolutionChangeEvent -= onExternalResolutionChanged;
//    }

//    private void fillResolutionList(bool _bForceUpdateResolutionLabel)
//    {
//        List<Resolution> aoResolutions = GameOptionsManager.Instance.m_oResolutionManager.getScreenResolutions();
//        int iResoultionsCount = aoResolutions.Count;
//        texts = new string[iResoultionsCount];
//        for (int iResolutionIndex = 0; iResolutionIndex < iResoultionsCount; ++iResolutionIndex)
//        {
//            texts[iResolutionIndex] = $"{aoResolutions[iResolutionIndex].width}x{aoResolutions[iResolutionIndex].height}";
//        }
//        slider.minValue = 0;
//        slider.maxValue = texts.Length - 1;
//        if(_bForceUpdateResolutionLabel)
//        {
//            slider.onValueChanged.Invoke(slider.value);
//        }
//    }

//    private void onExternalResolutionChanged(object sender, EventArgs e)
//    {
//        fillResolutionList(true);
//        slider.minValue = 0;
//        slider.maxValue = texts.Length - 1;
//        slider.value = GameOptionsManager.Instance.m_oResolutionManager.getResolutionIndex();
//    }
//#endif
}
