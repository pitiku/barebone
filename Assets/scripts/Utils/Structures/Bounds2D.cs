using UnityEngine;

public struct Bounds2D
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public bool cheapCheckCircle(Vector2 _vCenter, float _fRadius)
    {
        return _vCenter.x - _fRadius < maxX
            && _vCenter.x + _fRadius > minX
            && _vCenter.y - _fRadius < maxY
            && _vCenter.y + _fRadius > minY;
    }

    public bool cheapCheckSquare(Bounds2D _oBounds)
    {
        return _oBounds.maxX < maxX
            && _oBounds.minX > minX
            && _oBounds.maxY < maxY
            && _oBounds.minY > minY;
    }

    public void updateBounds(Vector2[] _avPoints)
    {
        minX = float.PositiveInfinity;
        minY = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        maxY = float.NegativeInfinity;
        for (int i = 0; i < _avPoints.Length; ++i)
        {
            updateBounds(_avPoints[i]);
        }
    }
    public void updateBounds(Vector2 _vNewPoint)
    {
        if (_vNewPoint.x < minX)
        {
            minX = _vNewPoint.x;
        }

        if (_vNewPoint.x > maxX)
        {
            maxX = _vNewPoint.x;
        }

        if (_vNewPoint.y < minY)
        {
            minY = _vNewPoint.y;
        }

        if (_vNewPoint.y > maxY)
        {
            maxY = _vNewPoint.y;
        }
    }
}
