using UnityEngine;
using System.Collections;

namespace Utils
{
    // This is the class that calculate relation between points and the geometric area.
    public class AreaDecisionUtils
    {
        public static bool WithInCircleArea( FixVector3 orgin, long radius, FixVector3 position )
        {
            if ( FixVector3.SqrDistance( orgin, position ) <= radius )
            {
                return true;
            }

            return false;
        }

        public static bool WithInFrontSectorArea( FixVector3 orgin, FixVector3 direction, long radius, FixVector3 position )
        {
            // check distance
            if ( FixVector3.SqrDistance( orgin, position ) > radius )
            {
                return false;
            }

            FixVector3 targetDirection = position - orgin;

            float a = FixVector3.Angle( targetDirection, direction );

            //Debug.LogError( a );
            //Debug.DrawLine( orgin, position, Color.red );
            //Debug.DrawLine( orgin, direction, Color.red );

            if ( Mathf.Abs( a ) < 30 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool WithInFrontRectArea( FixVector3 orgin, Quaternion r, long width, long height, FixVector3 position )
        {
            FixVector3 lb = orgin - ( new FixVector3( width >> 1, 0, 0 ) ) * r.eulerAngles;
            FixVector3 rb = orgin + ( new FixVector3( width >> 1, 0, 0 ) ) * r.eulerAngles;
            FixVector3 lt = orgin + ( new FixVector3( -width >> 1, 0, height ) ) * r.eulerAngles;
            FixVector3 rt = orgin + ( new FixVector3( width >> 1, 0, height ) ) * r.eulerAngles;

            return isINRect( position, lt, rt, rb, lb );
        }

        #region Private tools
        private static long Multiply( long p1x, long p1y, long p2x, long p2y, long p0x, long p0y )
        {
            return ( ( p1x - p0x ) * ( p2y - p0y ) - ( p2x - p0x ) * ( p1y - p0y ) );
        }

        private static bool isINRect( FixVector3 point, FixVector3 v0, FixVector3 v1, FixVector3 v2, FixVector3 v3 )
        {
            long x = point.x;
            long y = point.z;

            long v0x = v0.x;
            long v0y = v0.z;

            long v1x = v1.x;
            long v1y = v1.z;

            long v2x = v2.x;
            long v2y = v2.z;

            long v3x = v3.x;
            long v3y = v3.z;

            if ( Multiply( x, y, v0x, v0y, v1x, v1y ) * Multiply( x, y, v3x, v3y, v2x, v2y ) <= 0 && Multiply( x, y, v3x, v3y, v0x, v0y ) * Multiply( x, y, v2x, v2y, v1x, v1y ) <= 0 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
    #endregion
}
