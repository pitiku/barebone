using UnityEngine;

public class Layers
{
    public const string LAYER_selectable = "selectable";
    public const string LAYER_menu3D = "menu3D";
    public const string LAYER_charactersStatic = "charactersStaticMesh";
    public const string LAYER_x_ray = "x_ray";
    public const string LAYER_x_ray_ui = "x_ray_UI";
    public const string LAYER_Default = "Default";
    public const string LAYER_UI = "UI";
    public const string LAYER_NavMeshScene = "NavMeshScene";
    public const string LAYER_NavMeshObstacles = "NavMeshObstacles";
    public const string LAYER_NavMeshObstaclesEntities = "NavMeshObstaclesEntities";
    public const string LAYER_colliderProjectiles = "colliderProjectiles";
    public const string LAYER_colliders = "colliders";
    public const string LAYER_collidersEntities = "collidersEntities";
    public const string LAYER_raycast = "raycast";
    public const string LAYER_triggers = "triggers";
    public const string LAYER_colliders2D = "colliders2D";
    public const string LAYER_UI_dont_overlap = "UI_dont_overlap";
    public const string LAYER_Hidden = "hidden";
    public const string LAYER_SpriteMergeCache = "spriteMergeCache";
    public const string LAYER_LayerMask = "layerMask";
    public const string LAYER_compendium = "compendiumCharacters";
  

    public const uint LIGHT_LAYER_menuCharacterss = 1<<7;
    public static uint LIGHT_LAYER_comepndiumChracters;

    public static LayerMask ms_maskSelectable;
    public static LayerMask ms_maskScene;
    public static LayerMask ms_maskCombatCollidersWithTriggers;
    public static LayerMask ms_maskCombatColliders;
    public static LayerMask ms_maskCombatCollidersProjectiles;
    public static LayerMask ms_maskCombat2DColliders;
    public static LayerMask ms_maskRaycast;
    public static LayerMask ms_maskLayerMask;
    public static LayerMask ms_maskUI_dont_overlap;

    public static int ms_iLayerXRay;
    public static int ms_iLayerXRayUI;
    public static int ms_iLayerMenu3D;
    public static int ms_iLayerUI;
    public static int ms_iHidden;
    public static int ms_iLayerTrigger;
    public static int ms_iLayer2DColliders;
    public static int ms_iCombatCollidersProjectiles;
    public static int ms_iLayerMask;
    public static int ms_iLayerCharactersStatic;
    public static int ms_iLayerUI_dont_overlap;
    public static int ms_iLayerUI_SpriteMergeCache;
    

    public static void initialize()
    {
        ms_iLayerUI = LayerMask.NameToLayer(LAYER_UI);
        ms_iLayerXRay = LayerMask.NameToLayer(LAYER_x_ray);
        ms_iLayerXRayUI = LayerMask.NameToLayer(LAYER_x_ray_ui);
        ms_iLayerTrigger = LayerMask.NameToLayer(LAYER_triggers);
        ms_iLayer2DColliders = LayerMask.NameToLayer(LAYER_colliders2D);
        ms_iLayerCharactersStatic = LayerMask.NameToLayer(LAYER_charactersStatic);
        ms_iCombatCollidersProjectiles = LayerMask.NameToLayer(LAYER_colliderProjectiles);
        ms_iLayerMask = LayerMask.NameToLayer(LAYER_LayerMask);
        ms_iLayerMenu3D = LayerMask.NameToLayer(LAYER_menu3D);
        ms_iLayerUI_dont_overlap = LayerMask.NameToLayer(LAYER_UI_dont_overlap);
        ms_iHidden = LayerMask.NameToLayer(LAYER_Hidden);
        ms_iLayerUI_SpriteMergeCache = LayerMask.NameToLayer(LAYER_SpriteMergeCache);

        ms_maskSelectable = LayerMask.GetMask(LAYER_selectable) | LayerMask.GetMask(LAYER_menu3D) | LayerMask.GetMask(LAYER_charactersStatic);
        ms_maskScene = LayerMask.GetMask(LAYER_NavMeshScene);
        ms_maskCombatCollidersWithTriggers = LayerMask.GetMask(LAYER_colliders, LAYER_NavMeshObstacles, LAYER_triggers, LAYER_collidersEntities, LAYER_NavMeshObstaclesEntities);
        ms_maskCombatColliders = LayerMask.GetMask(LAYER_colliders, LAYER_collidersEntities, LAYER_NavMeshObstacles, LAYER_NavMeshObstaclesEntities);
        ms_maskCombatCollidersProjectiles = LayerMask.GetMask(LAYER_colliders, LAYER_collidersEntities, LAYER_NavMeshObstacles, LAYER_NavMeshObstaclesEntities, LAYER_colliderProjectiles, LAYER_triggers);
        ms_maskCombat2DColliders = LayerMask.GetMask(LAYER_colliders2D);
        ms_maskRaycast = LayerMask.GetMask(LAYER_raycast) | LayerMask.GetMask(LAYER_charactersStatic);
        ms_maskUI_dont_overlap = LayerMask.GetMask(LAYER_UI_dont_overlap);
        ms_maskLayerMask = LayerMask.GetMask(LAYER_LayerMask);
  
        int layerIndex = RenderingLayerMask.NameToRenderingLayer(LAYER_compendium);
        LIGHT_LAYER_comepndiumChracters = 1u << layerIndex;

    }

    public static bool isInMask(int _iLayer, LayerMask _oMask)
    {
        return ((1<<_iLayer) & _oMask) != 0;
    }
}
