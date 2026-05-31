using UnityEngine;

class ColorHSV
{
    float m_fHue;
    float m_fSaturation;
    float m_fValue;

    public ColorHSV(float _fHue, float _fSaturation, float _fBrightness)
    {
        m_fHue = _fHue;
        m_fSaturation = _fSaturation;
        m_fValue = _fBrightness;
    }

    public ColorHSV(Color _oColor)
    {
        Color.RGBToHSV(_oColor, out m_fHue, out m_fSaturation, out m_fValue);
    }

    public float Hue { get { return m_fHue; } }
    public float Saturation { get { return m_fSaturation; } set { m_fSaturation = value; } }
    public float Value { get { return m_fValue; } set { m_fValue = value; } }

    public static ColorHSV operator *(ColorHSV _v, float _f)
    {
        return new ColorHSV(_f * _v.m_fHue, _f * _v.m_fSaturation, _f * _v.m_fValue);
    }

    public static ColorHSV operator *(float _f, ColorHSV _v)
    {
        return new ColorHSV(_f * _v.m_fHue, _f * _v.m_fSaturation, _f * _v.m_fValue);
    }

    public static ColorHSV operator +(ColorHSV _v, ColorHSV _b)
    {
        // since the resultant hue is the middle point of both hues and the colors are represented in a circle,
        // there are 2 middle points

        float fHue_1 = (_v.m_fHue + _b.m_fHue) * 0.5f;
        float fHue_2 = fHue_1 + 0.5f; // add half a circumference to find the opposite point

        if (fHue_2 > 1) { fHue_2 -= 1; } // due to adding 0.5, the resultant hue may be greater than 1, in that case, since hues are expressed in 0-1 range, by substracting one we transform it into the right coordinates

        // distance is defined as the shortest length between 2 points and since the points are in a circumference, we need to compare distance in both directions and select the shortest one
        float fDistTo_v_1 = Mathf.Abs(_v.m_fHue - fHue_1);
        float fTemp = 1 - fDistTo_v_1; // calculate both dis
        if (fTemp < fDistTo_v_1) { fDistTo_v_1 = fTemp; }

        float fDistTo_v_2 = Mathf.Abs(_v.m_fHue - fHue_2);
        fTemp = 1 - fDistTo_v_2;
        if (fTemp < fDistTo_v_2) { fDistTo_v_2 = fTemp; }

        // prioritize first distance to match real world results
        float fHue = fDistTo_v_1 <= fDistTo_v_2 ? fHue_1 : fHue_2;
        float fSaturation = (_v.m_fSaturation + _b.m_fSaturation) * 0.5f;
        float fBrightness = (_v.m_fValue + _b.m_fValue) * 0.5f;

        return new ColorHSV(fHue, fSaturation, fBrightness);
    }

    public Color toRGB()
    {
        return Color.HSVToRGB(m_fHue, m_fSaturation, m_fValue);
    }
}