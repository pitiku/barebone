public static class EventsType
{
    public const string ResetCombatCameraPosition = "reset_combat_camera_position";
    public const string FadeToBlackFinished = "fade_to_black_finished";
    public const string FadeFromBlackFinished = "fade_from_black_finished";

    public const string GoToMainMenu = "go_to_main_menu";
    public const string GoToInventoryScreen = "go_to_inventory_screen";
    public const string GoToEndRunScreen = "go_to_end_run_screen";

    public const string ExitRun = "exit_run";

    public const string GameEntitySetSelected = "GameEntitySelected";

    public const string EndDialogue = "endDialogue";

    public const string SetCrystalColor = "set_crystal_color";

    public const string DisableGameobjects = "disable_gameobjects";

    public const string KillBossFirstMap = "kill_boss_first_map";

    public const string Defeat = "defeat";
    public const string FinalBossKilled = "final_boss_killed";
    public const string BossKilled = "boss_killed";

    public const string StartTreasureMapEvent = "start_treasure_map_event";
    public const string FinishTreasureMapEvent = "finish_treasure_map_event";

    public const string AdventurerToRemoveStart = "adventurer_to_remove_start";
    public const string AdventurerToRemoveFinish = "adventurer_to_remove_finish";

    public const string OpenDialogue = "open_dialogue";
    public const string CloseDialogue = "close_dialogue";

    public const string AdventurerRecruited = "adventurer_recruited";

    public const string EnterNode = "enter_node";
    public const string EnterCampfire = "enter_camp";

    public const string ItemVisualChecked = "item_visual_checked";
    public const string ItemVisualUpgraded = "item_visual_checked";
    public const string NewJewelChecked = "new_jewel_checked";

    public const string StartInventoryTargetSelection = "start_inventory_target_selection";
    public const string StartInventoryEquipPhase = "start_inventory_equip_phase";

    public const string Reroll = "reroll";
    public const string Pick3PickedEquipment = "pick3pickedEquipment";

    public const string DevPanelAdventurerRemoved = "dev_panel_adventurer_removed";
    public const string EndNode = "end_node";

    public const string SpecialLootAnimEnded = "special_loot_next_phase";

    public const string UpdateStats = "update_stats";

    public const string ChangeSaveSlot = "change_save_slot";
    public const string LoadDone = "load_done";

    public const string ActChanged = "act_changed";

    public const string UpdateRelics = "update_relics";

    public static string[] Events
    {
        get
        {
            return new string[] {
           FadeToBlackFinished,
           "deleted",
           "deleted",
           "deleted",
           "deleted",
           "deleted",
           GameEntitySetSelected,
           EndDialogue,
           "Custom",
           "deleted",
           ExitRun,
           "deleted",
           "deleted",
           SetCrystalColor,
           "deleted",
           GoToMainMenu,
           FadeFromBlackFinished,
           "deleted",
           GoToInventoryScreen,
           "deleted",
           GoToEndRunScreen,
           DisableGameobjects,
           "deleted",
           KillBossFirstMap,
           "deleted",
           "deleted",
           Defeat,
           FinalBossKilled,
           "deleted",
           StartTreasureMapEvent,
           FinishTreasureMapEvent,
           "deleted",
           ResetCombatCameraPosition,
           AdventurerToRemoveStart,
           AdventurerToRemoveFinish,
           OpenDialogue,
           CloseDialogue,
           AdventurerRecruited,
           "deleted",
           SpecialLootAnimEnded,
           "deleted",
           EnterNode,
           EnterCampfire,
           "deleted",
           "deleted",
           ItemVisualChecked,
           ItemVisualUpgraded,
           StartInventoryTargetSelection,
           StartInventoryEquipPhase,
           "deleted",
           Reroll,
           DevPanelAdventurerRemoved,
           BossKilled,
           EndNode,
           Pick3PickedEquipment,
           ChangeSaveSlot,
           LoadDone,
           UpdateRelics
       };
        }
    }
}
