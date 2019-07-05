using System;
using System.Runtime.InteropServices;

namespace Utils
{
    [Serializable, StructLayout( LayoutKind.Sequential )]
    public struct FixFactor
    {
        public long nom;
        public long den;
        [NonSerialized]
        public static FixFactor zero;
        [NonSerialized]
        public static FixFactor one;
        [NonSerialized]
        public static FixFactor pi;
        [NonSerialized]
        public static FixFactor twoPi;
        private static long mask_;
        private static long upper_;

        public FixFactor( long n, long d )
        {
            this.nom = n;
            this.den = d;
        }

        static FixFactor()
        {
            zero = new FixFactor( 0L, 1L );
            one = new FixFactor( 1L, 1L );
            pi = new FixFactor( 0x7ab8L, 0x2710L );
            twoPi = new FixFactor( 0xf570L, 0x2710L );
            mask_ = 0x7fffffffffffffffL;
            upper_ = 0xffffffL;
        }

        public int roundInt
        {
            get
            {
                return (int)Fix32Math.Divide( this.nom, this.den );
            }
        }

        public int integer
        {
            get
            {
                return (int)( this.nom / this.den );
            }
        }

        public float single
        {
            get
            {
                double num = ( (double)this.nom ) / ( (double)this.den );
                return (float)num;
            }
        }

        public bool IsPositive
        {
            get
            {
                DebugUtils.Assert( this.den != 0L, "VFactor: denominator is zero !" );
                if( this.nom == 0 )
                {
                    return false;
                }
                bool flag = this.nom > 0L;
                bool flag2 = this.den > 0L;
                return !( flag ^ flag2 );
            }
        }

        public bool IsNegative
        {
            get
            {
                DebugUtils.Assert( this.den != 0L, "VFactor: denominator is zero !" );
                if( this.nom == 0 )
                {
                    return false;
                }
                bool flag = this.nom > 0L;
                bool flag2 = this.den > 0L;
                return ( flag ^ flag2 );
            }
        }

        public bool IsZero
        {
            get
            {
                return ( this.nom == 0L );
            }
        }

        public override bool Equals( object obj )
        {
            return ( ( ( obj != null ) && ( base.GetType() == obj.GetType() ) ) && ( this == ( (FixFactor)obj ) ) );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public FixFactor Inverse
        {
            get
            {
                return new FixFactor( this.den, this.nom );
            }
        }

        public override string ToString()
        {
            return this.single.ToString();
        }

        public void strip()
        {
            while( ( ( this.nom & mask_ ) > upper_ ) && ( ( this.den & mask_ ) > upper_ ) )
            {
                this.nom = this.nom >> 1;
                this.den = this.den >> 1;
            }
        }

        public static bool operator <( FixFactor a, FixFactor b )
        {
            long num = a.nom * b.den;
            long num2 = b.nom * a.den;
            return ( !( ( b.den > 0L ) ^ ( a.den > 0L ) ) ? ( num < num2 ) : ( num > num2 ) );
        }

        public static bool operator >( FixFactor a, FixFactor b )
        {
            long num = a.nom * b.den;
            long num2 = b.nom * a.den;
            return ( !( ( b.den > 0L ) ^ ( a.den > 0L ) ) ? ( num > num2 ) : ( num < num2 ) );
        }

        public static bool operator <=( FixFactor a, FixFactor b )
        {
            long num = a.nom * b.den;
            long num2 = b.nom * a.den;
            return ( !( ( b.den > 0L ) ^ ( a.den > 0L ) ) ? ( num <= num2 ) : ( num >= num2 ) );
        }

        public static bool operator >=( FixFactor a, FixFactor b )
        {
            long num = a.nom * b.den;
            long num2 = b.nom * a.den;
            return ( !( ( b.den > 0L ) ^ ( a.den > 0L ) ) ? ( num >= num2 ) : ( num <= num2 ) );
        }

        public static bool operator ==( FixFactor a, FixFactor b )
        {
            return ( ( a.nom * b.den ) == ( b.nom * a.den ) );
        }

        public static bool operator !=( FixFactor a, FixFactor b )
        {
            return ( ( a.nom * b.den ) != ( b.nom * a.den ) );
        }

        public static bool operator <( FixFactor a, long b )
        {
            long nom = a.nom;
            long num2 = b * a.den;
            return ( ( a.den <= 0L ) ? ( nom > num2 ) : ( nom < num2 ) );
        }

        public static bool operator >( FixFactor a, long b )
        {
            long nom = a.nom;
            long num2 = b * a.den;
            return ( ( a.den <= 0L ) ? ( nom < num2 ) : ( nom > num2 ) );
        }

        public static bool operator <=( FixFactor a, long b )
        {
            long nom = a.nom;
            long num2 = b * a.den;
            return ( ( a.den <= 0L ) ? ( nom >= num2 ) : ( nom <= num2 ) );
        }

        public static bool operator >=( FixFactor a, long b )
        {
            long nom = a.nom;
            long num2 = b * a.den;
            return ( ( a.den <= 0L ) ? ( nom <= num2 ) : ( nom >= num2 ) );
        }

        public static bool operator ==( FixFactor a, long b )
        {
            return ( a.nom == ( b * a.den ) );
        }

        public static bool operator !=( FixFactor a, long b )
        {
            return ( a.nom != ( b * a.den ) );
        }

        public static FixFactor operator +( FixFactor a, FixFactor b )
        {
            FixFactor factor = new FixFactor();
            factor.nom = ( a.nom * b.den ) + ( b.nom * a.den );
            factor.den = a.den * b.den;
            return factor;
        }

        public static FixFactor operator +( FixFactor a, long b )
        {
            a.nom += b * a.den;
            return a;
        }

        public static FixFactor operator -( FixFactor a, FixFactor b )
        {
            FixFactor factor = new FixFactor();
            factor.nom = ( a.nom * b.den ) - ( b.nom * a.den );
            factor.den = a.den * b.den;
            return factor;
        }

        public static FixFactor operator -( FixFactor a, long b )
        {
            a.nom -= b * a.den;
            return a;
        }

        public static FixFactor operator *( FixFactor a, long b )
        {
            a.nom *= b;
            return a;
        }

        public static FixFactor operator /( FixFactor a, long b )
        {
            a.den *= b;
            return a;
        }

        public static FixVector3 operator *( FixVector3 v, FixFactor f )
        {
            return Fix32Math.Divide( v, f.nom, f.den );
        }

        public static FixVector2 operator *( FixVector2 v, FixFactor f )
        {
            return Fix32Math.Divide( v, f.nom, f.den );
        }

        public static FixVector3 operator /( FixVector3 v, FixFactor f )
        {
            return Fix32Math.Divide( v, f.den, f.nom );
        }

        public static FixVector2 operator /( FixVector2 v, FixFactor f )
        {
            return Fix32Math.Divide( v, f.den, f.nom );
        }

        public static int operator *( int i, FixFactor f )
        {
            return (int)Fix32Math.Divide( (long)( i * f.nom ), f.den );
        }

        public static FixFactor operator -( FixFactor a )
        {
            a.nom = -a.nom;
            return a;
        }
    }
}

