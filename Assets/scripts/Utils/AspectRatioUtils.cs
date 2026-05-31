using UnityEngine;

public static class AspectRatioUtils
{
    private const float TARGET_ASPECT_RATIO = 16f / 9f;
    private const float ASPECT_RATIO_TOLERANCE = 0.001f;

    public static bool isTargetAspectRatio(in Resolution _oResolution) => isTargetAspectRatio(_oResolution.width, _oResolution.height);
    public static bool isTargetAspectRatio(int _iScreenWidth, int _iScreenHeight) => isTargetAspectRatio((float)_iScreenWidth / (float)_iScreenHeight);
    public static bool isTargetAspectRatio(float _fAspectRatio) => _fAspectRatio.approximately(TARGET_ASPECT_RATIO, ASPECT_RATIO_TOLERANCE);

    public static Rect getCameraRectToTargetAspectRatio(in Resolution _oMaximumResolution) => 
        getBoxingType((float)_oMaximumResolution.width / (float)_oMaximumResolution.height, out float fScaleHeight, out float fScaleWidth) switch
        {
            BoxingType.None => new Rect(0f, 0f, 1f, 1f),
            BoxingType.Letterboxing => new Rect(
                0f, (1f - fScaleHeight) * 0.5f,
                1f, fScaleHeight),
            BoxingType.Pillarboxing => new Rect(
                (1f - fScaleWidth) * 0.5f, 0f,
                fScaleWidth, 1f),
            _ => new Rect(0f, 0f, 1f, 1f)
        };

    public static Vector2Int getResolutionAdjustedToTargetAspectRatio(int _iScreenWidth, int _iScreenHeight) =>
        getResolutionAdjustedToTargetAspectRatio(_iScreenWidth, _iScreenHeight, (float)_iScreenWidth / (float)_iScreenHeight);

    public static Vector2Int getResolutionAdjustedToTargetAspectRatio(int _iScreenWidth, int _iScreenHeight, float _fAspectRatio)
    {
        Vector2 v2AdjustedToAspectRatioResolutionScale = getResolutionScaleAdjustedToTargetAspectRatio(_iScreenWidth, _iScreenHeight, _fAspectRatio);
        return new Vector2Int(
            (int)(v2AdjustedToAspectRatioResolutionScale.x * _iScreenWidth),
            (int)(v2AdjustedToAspectRatioResolutionScale.y * _iScreenHeight));
    }

    private static Vector2 getResolutionScaleAdjustedToTargetAspectRatio(int _iScreenWidth, int _iScreenHeight, float _fAspectRatio)
    {
        return getBoxingType(_fAspectRatio, out float fScaleHeight, out float fScaleWidth) switch
        {
            BoxingType.None => Vector2.one,
            BoxingType.Letterboxing => new Vector2(1f, fScaleHeight),
            BoxingType.Pillarboxing => new Vector2(fScaleWidth, 1f),
            _ => Vector2.one
        };
    }

    private static BoxingType getBoxingType(float _fAspectRatio, out float _fScaleHeight, out float _fScaleWidth)
    {
        BoxingType eBoxingType;
        if (isTargetAspectRatio(_fAspectRatio))
        {
            eBoxingType = BoxingType.None;
            _fScaleHeight = _fScaleWidth = 0f;
        }
        else
        {
            _fScaleHeight = _fAspectRatio / TARGET_ASPECT_RATIO;
            _fScaleWidth = TARGET_ASPECT_RATIO / _fAspectRatio;
            eBoxingType = _fScaleHeight < 1f ? BoxingType.Letterboxing : BoxingType.Pillarboxing;
        }
        return eBoxingType;
    }

    private enum BoxingType
    {
        None, Letterboxing, Pillarboxing
    }
}