using System.Collections.Generic;
using UnityEngine;

public class AvatarManager : SceneSingleton<AvatarManager>
{
    readonly Dictionary<string, List<int>> m_dStatusShuffleBagByKey = new();
    readonly Dictionary<string, List<int>> m_dColorShuffleBagByKey = new();
    readonly Dictionary<string, List<int>> m_dSpritePartShuffleBagByKey = new();

    public int consumeNextStatusVariantIndex(string _sKey, int _iCapacity)
    {
        string sKey = $"STATUS::{buildEntityKey(_sKey)}";
        return consumeFromShuffleBag(m_dStatusShuffleBagByKey, sKey, _iCapacity);
    }

    public int consumeNextColorVariantIndex(string _sKey, int _iCapacity)
    {
        string sKey = $"COLOR::{buildEntityKey(_sKey)}";
        return consumeFromShuffleBag(m_dColorShuffleBagByKey, sKey, _iCapacity);
    }

    public int consumeNextSpritePartVariantIndex(string _sKey, int _iCapacity)
    {
        string sKey = $"SPRITE_PART::{buildEntityKey(_sKey)}";
        return consumeFromShuffleBag(m_dSpritePartShuffleBagByKey, sKey, _iCapacity);
    }

    static int consumeFromShuffleBag(Dictionary<string, List<int>> _dBagByKey, string _sKey, int _iCapacity)
    {
        if (_iCapacity <= 0) { return -1; }

        if (!_dBagByKey.TryGetValue(_sKey, out List<int> aoBag) || aoBag == null || aoBag.Count == 0)
        {
            aoBag = createShuffledBag(_iCapacity);
            _dBagByKey[_sKey] = aoBag;
        }

        int iLastIndex = aoBag.Count - 1;
        int iChosen = aoBag[iLastIndex];
        aoBag.RemoveAt(iLastIndex);
        return iChosen;
    }

    static List<int> createShuffledBag(int _iCapacity)
    {
        List<int> aoIndices = new List<int>(_iCapacity);
        for (int i = 0; i < _iCapacity; i++) { aoIndices.Add(i); }

        // Fisher-Yates
        for (int i = aoIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (aoIndices[i], aoIndices[j]) = (aoIndices[j], aoIndices[i]);
        }

        return aoIndices;
    }

    static string buildEntityKey(string _sEntityInstanceID)
    {
        return string.IsNullOrEmpty(_sEntityInstanceID) ? "NO_ENTITY" : _sEntityInstanceID;
    }

    public void clearRuntimeCache()
    {
        m_dStatusShuffleBagByKey.Clear();
        m_dColorShuffleBagByKey.Clear();
        m_dSpritePartShuffleBagByKey.Clear();
    }
}