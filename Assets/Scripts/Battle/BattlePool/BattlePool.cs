using System.Collections;
using System.Collections.Generic;

using Utils;

public class BattlePool
{

    private List<IBattlePoolUnit> usedUnits;

    private Queue<IBattlePoolUnit> unusedUnits;


    public BattlePool()
    {
        usedUnits = new List<IBattlePoolUnit>();
        unusedUnits = new Queue<IBattlePoolUnit>();
    }

    public BattlePool( int size )
    {
        size = size > 3 ? size / 2 : 2;
        usedUnits = new List<IBattlePoolUnit>( size );
        unusedUnits = new Queue<IBattlePoolUnit>( size );
    }

    public void AddUsedUnit( IBattlePoolUnit unit )
    {
        unit.pool = this;
        usedUnits.Add( unit );
    }

    public IBattlePoolUnit GetUnit()
    {
        if( unusedUnits.Count > 0 )
        {
            IBattlePoolUnit unit = unusedUnits.Dequeue();
            usedUnits.Add( unit );
            return unit;
        }
        else
        {
            return null;
        }
    }

    public void Recycle( IBattlePoolUnit unit )
    {
        if( usedUnits.Remove( unit ) )
        {
            unusedUnits.Enqueue( unit );
        }
        else
        {
            DebugUtils.Assert( false, "Battle Pool: can't recycle a battle unit " + unit.ToString() );
        }
    }

    public void Release()
    {
        List<IBattlePoolUnit>.Enumerator enumator1 = usedUnits.GetEnumerator();
        while( enumator1.MoveNext() )
        {
            enumator1.Current.Clear();
        }
        usedUnits.Clear();
        usedUnits = null;

        Queue<IBattlePoolUnit>.Enumerator enumator2 = unusedUnits.GetEnumerator();
        while( enumator2.MoveNext() )
        {
            enumator2.Current.Clear();
        }
        unusedUnits.Clear();
        unusedUnits = null;
    }

}
