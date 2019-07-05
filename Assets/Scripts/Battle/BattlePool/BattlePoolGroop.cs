using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class BattlePoolGroop
{
    private Dictionary<int, BattlePool> battlePools;

    public BattlePoolGroop()
    {
        battlePools = new Dictionary<int, BattlePool>();
    }

    public void AddUsedUnit( int key, IBattlePoolUnit unit )
    {
        if ( !battlePools.ContainsKey( key ) )
        {
            battlePools.Add( key, new BattlePool() );
        }

        battlePools[key].AddUsedUnit( unit );
    }

    public IBattlePoolUnit GetUnit( int key )
    {
        if ( !battlePools.ContainsKey( key ) )
        {
            return null;
        }

        return battlePools[key].GetUnit();
    }

    public void Release()
    {
        foreach ( KeyValuePair<int, BattlePool> item in battlePools )
        {
            item.Value.Release();
        }

        battlePools.Clear();
    }
}
