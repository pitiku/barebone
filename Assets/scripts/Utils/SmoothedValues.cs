using System;
using UnityEngine;

[Serializable]
public struct SmoothedFloat
{
    [HideInInspector] public float current;
    public float smoothTime;
    float velocity;

    public void reset(float _vTarget)
    {
        current = _vTarget;
        velocity = 0.0f;
    }

    public float smooth(float _fTarget)
    {
        current = Mathf.SmoothDamp(current, _fTarget, ref velocity, smoothTime);
        return current;
    }
}

[Serializable]
public struct SmoothedVector2
{
    [HideInInspector] public Vector2 current;
    public float smoothTime;
    Vector2 velocity;

    public void reset(Vector2 _vTarget)
    {
        current = _vTarget;
        velocity = Vector2.zero;
    }

    public Vector2 smooth(Vector2 _vTarget)
    {
        current = Vector2.SmoothDamp(current, _vTarget, ref velocity, smoothTime);
        return current;
    }
}

[Serializable]
public struct SmoothedVector3
{
    [HideInInspector] public Vector3 current;
    public float smoothTime;
    Vector3 velocity;

    public void reset(Vector3 _vTarget)
    {
        current = _vTarget;
        velocity = Vector3.zero;
    }

    public Vector3 smooth(Vector3 _vTarget)
    {
        current = Vector3.SmoothDamp(current, _vTarget, ref velocity, smoothTime);
        return current;
    }
}