using System;
using System.Runtime.InteropServices;

namespace Utils
{
    [Serializable, StructLayout( LayoutKind.Sequential )]
    public struct Fix32
    {
        public static readonly Fix32 one = new Fix32(1f);
        public static readonly Fix32 zero = new Fix32();
        public static readonly Fix32 hundred = new Fix32(100f);

        public int mainValue;
        public Fix32( int value )
        {
            this.mainValue = value;
        }

        public Fix32( float f )
        {
            this.mainValue = (int)Math.Round( (double)( f * 1000f ) );
        }

        public override bool Equals( object o )
        {
            if ( o == null )
            {
                return false;
            }
            Fix32 num = (Fix32)o;
            return ( this.mainValue == num.mainValue );
        }

        public override int GetHashCode()
        {
            return this.mainValue.GetHashCode();
        }

        public static Fix32 Min( Fix32 a, Fix32 b )
        {
            return new Fix32( Math.Min( a.mainValue, b.mainValue ) );
        }

        public static Fix32 Max( Fix32 a, Fix32 b )
        {
            return new Fix32( Math.Max( a.mainValue, b.mainValue ) );
        }

        public override string ToString()
        {
            return this.scalar.ToString();
        }

        public float scalar
        {
            get
            {
                return ( this.mainValue * 0.001f );
            }
        }
        public static explicit operator Fix32( float f )
        {
            return new Fix32( (int)Math.Round( (double)( f * 1000f ) ) );
        }

        public static implicit operator Fix32( int i )
        {
            return new Fix32( i );
        }

        public static explicit operator float( Fix32 ob )
        {
            return ( ob.mainValue * 0.001f );
        }

        public static explicit operator long( Fix32 ob )
        {
            return (long)ob.mainValue;
        }

        public static Fix32 operator +( Fix32 a, Fix32 b )
        {
            return new Fix32( a.mainValue + b.mainValue );
        }

        public static Fix32 operator -( Fix32 a, Fix32 b )
        {
            return new Fix32( a.mainValue - b.mainValue );
        }

        public static bool operator ==( Fix32 a, Fix32 b )
        {
            return ( a.mainValue == b.mainValue );
        }

        public static bool operator !=( Fix32 a, Fix32 b )
        {
            return ( a.mainValue != b.mainValue );
        }

        public static bool operator <( Fix32 a, Fix32 b )
        {
            return ( a.mainValue < b.mainValue );
        }

        public static bool operator <=( Fix32 a, Fix32 b )
        {
            return ( a.mainValue <= b.mainValue );
        }

        public static bool operator >( Fix32 a, Fix32 b )
        {
            return ( a.mainValue > b.mainValue );
        }

        public static bool operator >=( Fix32 a, Fix32 b )
        {
            return ( a.mainValue >= b.mainValue );
        }

        public static Fix32 operator *( Fix32 a, Fix32 b )
        {
            return new Fix32( a.mainValue * b.mainValue );
        }
    }
}

