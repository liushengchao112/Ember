/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: IdGenerator.cs
// description: 
// 
// created time：09/29/2016
//
//----------------------------------------------------------------*/


using UnityEngine;
using System.Collections.Generic;

public class IdGenerator
{
    const string DEFAULT_COUNTER = "null";  // "null" is the default id counter for generally using.
    static Dictionary<string, int> counters;
    static Dictionary<string, long> longCounters; // this is just for matcher's id when there is no server.

    static IdGenerator()
    {
        counters = new Dictionary<string, int>();
        counters.Add( "null", 1 ); // count from 1.
        longCounters = new Dictionary<string, long>();
        longCounters.Add( "null", 11111111 );
    }

    public static int GenerateIntId( string mark = null )
    {
        if( mark == null )
        {
            mark = DEFAULT_COUNTER;
        }

        int value;
        if( counters.TryGetValue( mark, out value ) )
        {
            counters[mark] = value + 1;
            return value;
        }
        else
        {
            counters.Add( mark, 2 );
            return 1; // count from 1.
        }
    }

    public static long GenerateLongId( string mark = null )
    {
        if( mark == null )
        {
            mark = DEFAULT_COUNTER;
        }

        long value;
        if( longCounters.TryGetValue( mark, out value ) )
        {
            longCounters[mark] = value + 1;
            return value;
        }
        else
        {
            longCounters.Add( mark, 2 );
            return 111111111; // count from 1.
        }
    }

    public static void Reset( string mark = null )
    {
        if( mark == null )
        {
            mark = DEFAULT_COUNTER;
        }

        int value;
        if( counters.ContainsKey( mark ) )
        {
            counters[mark] = 0;
        }
        else
        {
            Debug.LogError( "there isn't such id generator " + mark );
        }
    }

    public static void ResetAll()
    {
        counters.Clear();
        counters = new Dictionary<string, int>();
        counters.Add( "null", 1 ); // count from 1.
    }
	
}
