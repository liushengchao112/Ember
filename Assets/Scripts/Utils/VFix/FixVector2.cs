using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
    [Serializable, StructLayout( LayoutKind.Sequential )]
    public struct FixVector2
    {
        public int x;
        public int y;
        public static FixVector2 zero;
        private static readonly int[] Rotations;

        public FixVector2( int x, int y )
        {
            this.x = x;
            this.y = y;
        }

        static FixVector2()
        {
            zero = new FixVector2();
            Rotations = new int[] { 1, 0, 0, 1, 0, 1, -1, 0, -1, 0, 0, -1, 0, -1, 1, 0 };
        }

        public int sqrMagnitude
        {
            get
            {
                return ( ( this.x * this.x ) + ( this.y * this.y ) );
            }
        }

        public long sqrMagnitudeLong
        {
            get
            {
                long x = this.x;
                long y = this.y;
                return ( ( x * x ) + ( y * y ) );
            }
        }

        public int magnitude
        {
            get
            {
                long x = this.x;
                long y = this.y;
                return Fix32Math.Sqrt( ( x * x ) + ( y * y ) );
            }
        }

        public static int Dot( FixVector2 a, FixVector2 b )
        {
            return ( ( a.x * b.x ) + ( a.y * b.y ) );
        }

        public static long DotLong( ref FixVector2 a, ref FixVector2 b )
        {
            return ( ( a.x * b.x ) + ( a.y * b.y ) );
        }

        public static long DotLong( FixVector2 a, FixVector2 b )
        {
            return ( ( a.x * b.x ) + ( a.y * b.y ) );
        }

        public static long DetLong( ref FixVector2 a, ref FixVector2 b )
        {
            return ( ( a.x * b.y ) - ( a.y * b.x ) );
        }

        public static long DetLong( FixVector2 a, FixVector2 b )
        {
            return ( ( a.x * b.y ) - ( a.y * b.x ) );
        }

        public override bool Equals( object o )
        {
            if ( o == null )
            {
                return false;
            }
            FixVector2 num = (FixVector2)o;
            return ( ( this.x == num.x ) && ( this.y == num.y ) );
        }

        public override int GetHashCode()
        {
            return ( ( this.x * 0xc005 ) + ( this.y * 0x1800d ) );
        }

        public static FixVector2 Rotate( FixVector2 v, int r )
        {
            r = r % 4;
            return new FixVector2( ( v.x * Rotations[r * 4] ) + ( v.y * Rotations[( r * 4 ) + 1] ), ( v.x * Rotations[( r * 4 ) + 2] ) + ( v.y * Rotations[( r * 4 ) + 3] ) );
        }

        public static FixVector2 Min( FixVector2 a, FixVector2 b )
        {
            return new FixVector2( Math.Min( a.x, b.x ), Math.Min( a.y, b.y ) );
        }

        public static FixVector2 Max( FixVector2 a, FixVector2 b )
        {
            return new FixVector2( Math.Max( a.x, b.x ), Math.Max( a.y, b.y ) );
        }

        public static FixVector2 FromInt3XZ( FixVector3 o )
        {
            return new FixVector2( o.x, o.z );
        }

        public static FixVector3 ToInt3XZ( FixVector2 o )
        {
            return new FixVector3( o.x, 0, o.y );
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "(", this.x, ", ", this.y, ")" };
            return string.Concat( objArray1 );
        }

        public void Min( ref FixVector2 r )
        {
            this.x = Mathf.Min( this.x, r.x );
            this.y = Mathf.Min( this.y, r.y );
        }

        public void Max( ref FixVector2 r )
        {
            this.x = Mathf.Max( this.x, r.x );
            this.y = Mathf.Max( this.y, r.y );
        }

        public void Normalize()
        {
            long num = this.x * 100;
            long num2 = this.y * 100;
            long a = ( num * num ) + ( num2 * num2 );
            if ( a != 0 )
            {
                long b = Fix32Math.Sqrt( a );
                this.x = (int)Fix32Math.Divide( (long)( num * 0x3e8L ), b );
                this.y = (int)Fix32Math.Divide( (long)( num2 * 0x3e8L ), b );
            }
        }

        public FixVector2 normalized
        {
            get
            {
                FixVector2 num = new FixVector2( this.x, this.y );
                num.Normalize();
                return num;
            }
        }
        public static FixVector2 ClampMagnitude( FixVector2 v, int maxLength )
        {
            long sqrMagnitudeLong = v.sqrMagnitudeLong;
            long num2 = maxLength;
            if ( sqrMagnitudeLong > ( num2 * num2 ) )
            {
                long b = Fix32Math.Sqrt( sqrMagnitudeLong );
                int x = (int)Fix32Math.Divide( (long)( v.x * maxLength ), b );
                return new FixVector2( x, (int)Fix32Math.Divide( (long)( v.x * maxLength ), b ) );
            }
            return v;
        }

        public static explicit operator Vector2( FixVector2 ob )
        {
            return new Vector2( ob.x * 0.001f, ob.y * 0.001f );
        }

        public static explicit operator FixVector2( Vector2 ob )
        {
            return new FixVector2( (int)Math.Round( (double)( ob.x * 1000f ) ), (int)Math.Round( (double)( ob.y * 1000f ) ) );
        }

        public static FixVector2 operator +( FixVector2 a, FixVector2 b )
        {
            return new FixVector2( a.x + b.x, a.y + b.y );
        }

        public static FixVector2 operator -( FixVector2 a, FixVector2 b )
        {
            return new FixVector2( a.x - b.x, a.y - b.y );
        }

        public static bool operator ==( FixVector2 a, FixVector2 b )
        {
            return ( ( a.x == b.x ) && ( a.y == b.y ) );
        }

        public static bool operator !=( FixVector2 a, FixVector2 b )
        {
            return ( ( a.x != b.x ) || ( a.y != b.y ) );
        }

        public static FixVector2 operator -( FixVector2 lhs )
        {
            lhs.x = -lhs.x;
            lhs.y = -lhs.y;
            return lhs;
        }

        public static FixVector2 operator *( FixVector2 lhs, int rhs )
        {
            lhs.x *= rhs;
            lhs.y *= rhs;
            return lhs;
        }
    }
}
