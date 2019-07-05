using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Constants;

namespace Utils
{
    public class ConvertUtils
    {
        public static int ToLogicInt( float f )
        {
            return Mathf.RoundToInt( f * GameConstants.LOGIC_FIXPOINT_PRECISION );
        }

        public static float ToRealFloat( int f )
        {
            return f * GameConstants.LOGIC_FIXPOINT_PRECISION_FACTOR;
        }

        public static List<FixVector3> ToFixVector3List( List<Vector3> list )
        {
            DebugUtils.Assert( list != null, "Can't convert a null vector3 list to fixVector3 list" );

            List<FixVector3> fixList = new List<FixVector3>();

            for ( int i = 0; i < list.Count; i++ )
            {
                fixList.Add( new FixVector3( list[i] ) );
            }

            return fixList;
        }
    }
}
