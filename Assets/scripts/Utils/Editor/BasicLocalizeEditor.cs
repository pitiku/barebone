using I2.Loc;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BasicLocalize))]
public class BasicLocalizeEditor : Editor
{
    #region static
    static List<string> ms_asCategories = new();
    static Dictionary<string, List<string>> ms_dTerms = new();

    [RuntimeInitializeOnLoadMethod]
    static void initializeStatic()
    {
        ms_asCategories = new();
        ms_dTerms = new();

        LocalizationManager.OnLocalizeEvent -= loadLocalization;
        LocalizationManager.OnLocalizeEvent += loadLocalization;
        LocalizationEditor.OnLocalizationImportedEvent -= loadLocalization;
        LocalizationEditor.OnLocalizationImportedEvent += loadLocalization;
    }

    static void checkLocalization()
    {
        if (ms_asCategories.Count == 0)
        {
            loadLocalization();
        }
    }

    static void loadLocalization()
    {
        ms_asCategories.Clear();
        ms_dTerms.Clear();

        LanguageSourceData oCurrentSource = getLocalizationDataSource();
        if (oCurrentSource == null) return;

        ms_asCategories = oCurrentSource.GetCategories();

        for (int iCategory = 0; iCategory < ms_asCategories.Count; iCategory++)
        {
            string sCategory = ms_asCategories[iCategory];
            int iTrim = sCategory.Length + 1;
            List<string> aoTerms = oCurrentSource.GetTermsList(sCategory);
            for (int iTerm = 0; iTerm < aoTerms.Count; ++iTerm)
            {
                aoTerms[iTerm] = aoTerms[iTerm].Substring(iTrim);
            }
            aoTerms.Sort();
            ms_dTerms[sCategory] = aoTerms;
        }
    }

    static LanguageSourceData getLocalizationDataSource()
    {
        LocalizationManager.UpdateSources();

        if (LocalizationManager.Sources.Count > 0)
        {
            return LocalizationManager.Sources[0];
        }
        return null;
    }
    #endregion

    private SerializedProperty m_oParams;
    private SerializedProperty m_oSplitInLines;

    public virtual void OnEnable()
    {
        m_oParams = serializedObject.FindProperty("m_params");
        m_oSplitInLines = serializedObject.FindProperty("m_bSplitInLines");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        checkLocalization();

        if (getLocalizationDataSource() == null) return;

        BasicLocalize oBL = (BasicLocalize)target;

        EditorGUILayout.BeginHorizontal();

        if (!ms_asCategories.Contains(oBL.m_sCategory))
        {
            oBL.m_sCategory = ms_asCategories[0];
        }

        int iCategoryIndex = ms_asCategories.IndexOf(oBL.m_sCategory);
        
        List<string> asTerms = ms_dTerms[oBL.m_sCategory];
        int iTermIndex = asTerms.IndexOf(oBL.m_sTerm);

        int iNewCategoryIndex = EditorGUILayout.Popup(iCategoryIndex, ms_asCategories.ToArray());
        if (iNewCategoryIndex != iCategoryIndex)
        {
            Undo.RecordObject(oBL, "BasicLocalize");
            iCategoryIndex = iNewCategoryIndex;
            oBL.m_sCategory = ms_asCategories[iCategoryIndex];
            asTerms = ms_dTerms[ms_asCategories[iCategoryIndex]];
            iTermIndex = 0;
        }

        if (asTerms != null)
        {
            iTermIndex = EditorGUILayout.Popup(iTermIndex, asTerms.ToArray());

            if (iTermIndex >= 0 && iTermIndex < asTerms.Count)
            {
                string sNewTerm = asTerms[iTermIndex];
                if (sNewTerm != oBL.m_sTerm)
                {
                    Undo.RecordObject(oBL, "BasicLocalize");
                    oBL.m_sTerm = sNewTerm;
                    EditorUtility.SetDirty(oBL);
                }
            }
        }

        m_oParams.boolValue = EditorGUILayout.ToggleLeft("Params", m_oParams.boolValue, GUILayout.Width(60));
        m_oSplitInLines.boolValue = EditorGUILayout.ToggleLeft("SplitLines", m_oSplitInLines.boolValue, GUILayout.Width(60));

        EditorGUILayout.EndHorizontal();

        //Check term
        string sTerm = oBL.m_sCategory + "/" + oBL.m_sTerm;
        string s = Utils.getTranslationWithParams(sTerm);
        if (s == null)
        {
            //oBL.setText("<<term error>>");
        }
        else
        {
            oBL.updateText();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
