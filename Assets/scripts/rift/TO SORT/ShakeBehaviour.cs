using UnityEngine;

public class ShakeBehaviour : MonoBehaviour
{
    public enum eShakeType
    {
        POSITION,
        SCALE
    }
    public enum eShakeMethod
    {
        UNIFORM,
        RANDOM
    }

    float m_fDelay = 0.0f;
    float m_fMagnitude = 0;
    float m_fDuration = 0;
    bool m_bYCoord = false;
    eShakeType m_eShakeType;
    eShakeMethod m_eShakeMethod;

    Vector3 m_vOriginalTransformPos;
    float m_fElapsedTime = 0f;
    float m_fDecrementMagnitude;
    float m_fOriginalMagnitude;
    bool _bActive = false;

    public void start(Vector3 _vInitialPos, Vector3 _vInitialScale)
    {
        // used for transform.localPosition and transform.localScale
        m_vOriginalTransformPos = (m_eShakeType == eShakeType.POSITION) ? _vInitialPos : _vInitialScale;

        m_fDecrementMagnitude = m_fMagnitude / m_fDuration;
        m_fOriginalMagnitude = m_fMagnitude;
        _bActive = true;
    }

    void Update()
    {
        if (!_bActive)
        {
            return;
        }

        if (m_fDelay == 0)
        {
            if (m_eShakeType == eShakeType.POSITION)
            {
                if (m_eShakeMethod == eShakeMethod.UNIFORM)
                {
                    float fOffset = getRandomOffset() * m_fMagnitude;
                    transform.localPosition = m_vOriginalTransformPos + new Vector3(fOffset, 0, fOffset);
                }
                else
                {
                    float fXOffset = getRandomOffset() * m_fMagnitude;
                    if (m_bYCoord)
                    {
                        float fyOffset = getRandomOffset() * m_fMagnitude;
                        transform.localPosition = m_vOriginalTransformPos + new Vector3(fXOffset, fyOffset, 0);
                    }
                    else
                    {
                        float fzOffset = getRandomOffset() * m_fMagnitude;
                        transform.localPosition = m_vOriginalTransformPos + new Vector3(fXOffset, 0, fzOffset);
                    }
                }
            }
            else
            {
                if (m_eShakeMethod == eShakeMethod.UNIFORM)
                {
                    float oRandom = getRandomOffset();
                    transform.localScale = m_vOriginalTransformPos + new Vector3(oRandom, oRandom, 0);
                }
                else
                {
                    transform.localScale = m_vOriginalTransformPos + new Vector3(getRandomOffset(), getRandomOffset(), 0);
                }
            }

            m_fElapsedTime += Time.deltaTime;
            m_fMagnitude = m_fOriginalMagnitude - (m_fDecrementMagnitude * m_fElapsedTime);

            if (m_fElapsedTime > m_fDuration)
            {
                end();
            }
        }
        else
        {
            m_fElapsedTime += Time.deltaTime;
            if (m_fElapsedTime >= m_fDelay)
            {
                m_fDelay = 0;
                m_fElapsedTime = 0;
            }
        }
    }

    void end()
    {
        if (_bActive)
        {
            _bActive = false;
            if (m_eShakeType == eShakeType.POSITION)
            {
                transform.localPosition = m_vOriginalTransformPos;
            }
            else
            {
                transform.localScale = m_vOriginalTransformPos;
            }
            Destroy(this);
        }
    }

    public void cancelShake()
    {
        end();
    }

    float getRandomOffset()
    {
        float oRandom = 0;

        switch (m_eShakeType)
        {
            case eShakeType.POSITION:
                oRandom = RandomUtils.range(null, -0.1f, 0.1f, "");
                break;
            case eShakeType.SCALE:
                oRandom = RandomUtils.range(null, -m_fMagnitude, m_fMagnitude, "");
                break;
            default:
                break;
        }
        return oRandom;
    }

    public void setShake(float _fMagnitude, float _fDuration, bool _bYCoord, eShakeType _eShakeType = eShakeType.POSITION, eShakeMethod _eShakeMethod = eShakeMethod.RANDOM, float _fDelay = 0)
    {
        m_fMagnitude = _fMagnitude;
        m_fDuration = _fDuration;
        m_bYCoord = _bYCoord;
        m_eShakeType = _eShakeType;
        m_eShakeMethod = _eShakeMethod;
        m_fDelay = _fDelay;
    }
}
