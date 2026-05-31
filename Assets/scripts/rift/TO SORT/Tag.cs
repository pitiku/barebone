using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
class CustomTag
{
    [ValueDropdown("getEvents")]
    public string m_sPredifinedTag = "Custom";

    [ShowIf("@isCustomSelected()")]
    public string m_sCustomTag = "";

    public string Value
    {
        get
        {
            return m_sPredifinedTag == "Custom" ? m_sCustomTag : m_sPredifinedTag;
        }
    }

    IEnumerable<string> getEvents() { return new List<string>(TagTypes.Tags); }
    private bool isCustomSelected() { return m_sPredifinedTag == "Custom"; }
}

public class Tag : MonoBehaviour
{
    enum eAvailability { WhileExists, WhileActive }

    [SerializeField] CustomTag m_oTag;
    [SerializeField] eAvailability m_eAvailability;

    private void Awake()
    {
        if (m_eAvailability == eAvailability.WhileExists) { GameplayManager.Instance.addTag(this); }
    }

    private void OnDestroy()
    {
        if (!GameplayManager.isNull())
        {
            if (m_eAvailability == eAvailability.WhileExists)
            {
                GameplayManager.Instance.removeTag(this);
            }
        }
    }

    private void OnEnable()
    {
        if (m_eAvailability == eAvailability.WhileActive)
        {
            GameplayManager.Instance.addTag(this);
        }
    }

    private void OnDisable()
    {
        if (!GameplayManager.isNull())
        {
            if (m_eAvailability == eAvailability.WhileActive)
            {
                GameplayManager.Instance.removeTag(this);
            }
        }
    }

    public string getValue() { return m_oTag.Value; }
}

public static class TagTypes
{
    public const string TeamScreen = "team_screen";
    public const string CrystalRunesScreen = "crystal_runes_screen";
    public const string SelectSquadScreen = "select_squad_screen";
    public const string ManagementButtons = "management_buttons";

    public const string TimeOfDay = "time_of_day";

    public const string MainBackground = "main_background";
    public const string EquipJewelScreen = "equip_jewel_screen";
    public const string ReplayButton = "replay_button";
    public const string EndTurnButton = "end_turn_button";
    public const string CombatCameraTransform = "combat_camera_transform";
    public const string OnOpenStoreHOCEvents = "on_open_store_hoc_events";

    public const string MapState = "map_state";

    public const string StoreUI = "store_ui";
    public const string ManagementUI = "management_ui";
    public const string PopupManagerUI = "popupmanager";
    public const string Portraits = "portraits";
    public const string Resources = "resources";
    public const string Map = "map";
    public const string EndCombatExperienceScreen = "end_combat_experience_screen";

    public const string TeamScreenCloseButton = "team_screen_close_button";

    public const string CampfireSleepButton = "campfire_sleep_button";
    public const string CampfireRiskBar = "campfire_risk_bar";
    public const string CampfireEventScreen = "campfire_event_screen";

    public const string TeamScreenButton = "team_screen_button";
    public const string ExitStoreButton = "exit_store_button";
    public const string CompendiumButton = "compendium_button";

    public static string[] Tags
    {
        get
        {
            return new string[] {
            "Custom",
            TeamScreen,
            CrystalRunesScreen,
            SelectSquadScreen,
            ManagementButtons,
            ManagementUI,
            Portraits,
            Resources,
            TimeOfDay,
            PopupManagerUI,
            Map,
            MainBackground,
            EquipJewelScreen,
            ReplayButton,
            EndTurnButton,
            CombatCameraTransform,
            OnOpenStoreHOCEvents,
            MapState,
            StoreUI,
            TeamScreenCloseButton,
            EndCombatExperienceScreen,
            CampfireSleepButton,
            CampfireRiskBar,
            CampfireEventScreen,
            TeamScreenButton,
            ExitStoreButton,
            CompendiumButton
        };
        }
    }
}