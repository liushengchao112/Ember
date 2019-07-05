using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
    [Serializable, StructLayout( LayoutKind.Sequential )]
    public struct FixVector3
    {
        public const int Precision = 0x3e8;
        public const float FloatPrecision = 1000f;
        public const float PrecisionFactor = 0.001f;
        public int x;
        public int y;
        public int z;
        public static readonly FixVector3 zero;
        public static readonly FixVector3 one;
        public static readonly FixVector3 half;
        public static readonly FixVector3 forward;
        public static readonly FixVector3 up;
        public static readonly FixVector3 right;

        public FixVector3 normalized
        {
            get
            {
               this.Normalize();
               return this;
            }
        }

        public FixVector3( Vector3 position )
        {
            this.x = (int)Math.Round( (double)( position.x * FloatPrecision ) );
            this.y = (int)Math.Round( (double)( position.y * FloatPrecision ) );
            this.z = (int)Math.Round( (double)( position.z * FloatPrecision ) );
        }

        public FixVector3( int x, int y, int z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public FixVector3( float px, float py, float pz )
        {
            this.x = (int)Math.Round( (double)( px * FloatPrecision ) );
            this.y = (int)Math.Round( (double)( py * FloatPrecision ) );
            this.z = (int)Math.Round( (double)( pz * FloatPrecision ) );
        }

        static FixVector3()
        {
            zero = new FixVector3( 0, 0, 0 );
            one = new FixVector3( Precision, Precision, Precision );
            half = new FixVector3( FloatPrecision * 0.5f, FloatPrecision * 0.5f, FloatPrecision * 0.5f );
            forward = new FixVector3( 0, 0, Precision );
            up = new FixVector3( 0, Precision, 0 );
            right = new FixVector3( Precision, 0, 0 );
        }

        public FixVector3 DivBy2()
        {
            this.x = this.x >> 1;
            this.y = this.y >> 1;
            this.z = this.z >> 1;
            return this;
        }

        public int this[int i]
        {
            get
            {
                return ( ( i != 0 ) ? ( ( i != 1 ) ? this.z : this.y ) : this.x );
            }
            set
            {
                if( i == 0 )
                {
                    this.x = value;
                }
                else if( i == 1 )
                {
                    this.y = value;
                }
                else
                {
                    this.z = value;
                }
            }
        }

        public static float Angle( FixVector3 lhs, FixVector3 rhs )
        {
            double d = ( (double)Dot( lhs, rhs ) ) / ( lhs.magnitude * rhs.magnitude );
            d = ( d >= -1.0 ) ? ( ( d <= 1.0 ) ? d : 1.0 ) : -1.0;
            return (float)Math.Acos( d );
        }

        public static FixFactor AngleInt( FixVector3 lhs, FixVector3 rhs )
        {
            long den = lhs.magnitude * rhs.magnitude;
            return Fix32Math.acos( (long)Dot( ref lhs, ref rhs ), den );
        }

        public static int Dot( ref FixVector3 lhs, ref FixVector3 rhs )
        {
            return ( ( ( lhs.x * rhs.x ) + ( lhs.y * rhs.y ) ) + ( lhs.z * rhs.z ) );
        }

        public static int Dot( FixVector3 lhs, FixVector3 rhs )
        {
            return ( ( ( lhs.x * rhs.x ) + ( lhs.y * rhs.y ) ) + ( lhs.z * rhs.z ) );
        }

        public static long DotLong( FixVector3 lhs, FixVector3 rhs )
        {
            return ( ( ( lhs.x * rhs.x ) + ( lhs.y * rhs.y ) ) + ( lhs.z * rhs.z ) );
        }

        public static long DotLong( ref FixVector3 lhs, ref FixVector3 rhs )
        {
            return ( ( ( lhs.x * rhs.x ) + ( lhs.y * rhs.y ) ) + ( lhs.z * rhs.z ) );
        }

        public static long DotXZLong( ref FixVector3 lhs, ref FixVector3 rhs )
        {
            return ( ( lhs.x * rhs.x ) + ( lhs.z * rhs.z ) );
        }

        public static long DotXZLong( FixVector3 lhs, FixVector3 rhs )
        {
            return ( ( lhs.x * rhs.x ) + ( lhs.z * rhs.z ) );
        }

        public static FixVector3 Cross( ref FixVector3 lhs, ref FixVector3 rhs )
        {
            return new FixVector3( Fix32Math.Divide( (int)( ( lhs.y * rhs.z ) - ( lhs.z * rhs.y ) ), Precision ), Fix32Math.Divide( (int)( ( lhs.z * rhs.x ) - ( lhs.x * rhs.z ) ), Precision ), Fix32Math.Divide( (int)( ( lhs.x * rhs.y ) - ( lhs.y * rhs.x ) ), Precision ) );
        }

        public static FixVector3 Cross( FixVector3 lhs, FixVector3 rhs )
        {
            return new FixVector3( Fix32Math.Divide( (int)( ( lhs.y * rhs.z ) - ( lhs.z * rhs.y ) ), Precision ), Fix32Math.Divide( (int)( ( lhs.z * rhs.x ) - ( lhs.x * rhs.z ) ), Precision ), Fix32Math.Divide( (int)( ( lhs.x * rhs.y ) - ( lhs.y * rhs.x ) ), Precision ) );
        }

        public static FixVector3 MoveTowards( FixVector3 from, FixVector3 to, int dt )
        {
            FixVector3 num2 = to - from;
            if( num2.sqrMagnitudeLong <= ( dt * dt ) )
            {
                return to;
            }
            FixVector3 num = to - from;
            return ( from + num.NormalizeTo( dt ) );
        }

        //The length of two point is square root of ( x * x + y * y + z * z ).
        public static int SqrDistance( FixVector3 f1, FixVector3 f2 )
        {
            long x = f1.x - f2.x;
            long y = f1.y - f2.y;
            long z = f1.z - f2.z;

            return Fix32Math.Sqrt( ( x * x ) + ( y * y ) + ( z * z ) );
        }

        public FixVector3 Normal2D()
        {
            return new FixVector3( this.z, this.y, -this.x );
        }

        public FixVector3 NormalizeTo( int newMagn = 1 )
        {
            long num = this.x;
            long num2 = this.y;
            long num3 = this.z;
            long a = ( ( num * num ) + ( num2 * num2 ) ) + ( num3 * num3 );
            if( a != 0 )
            {
                long b = Fix32Math.Sqrt( a );
                long num6 = newMagn;

                this.x = (int)Fix32Math.Divide( (long)( num * num6 ), b );
                this.y = (int)Fix32Math.Divide( (long)( num2 * num6 ), b );
                this.z = (int)Fix32Math.Divide( (long)( num3 * num6 ), b );
            }
            return this;
        }

        public long Normalize()
        {
            long num = this.x << 7;
            long num2 = this.y << 7;
            long num3 = this.z << 7;
            long a = ( ( num * num ) + ( num2 * num2 ) ) + ( num3 * num3 );
            if( a == 0 )
            {
                return 0L;
            }
            long b = Fix32Math.Sqrt( a );
            long num6 = 0x3e8L;
            this.x = (int)Fix32Math.Divide( (long)( num * num6 ), b );
            this.y = (int)Fix32Math.Divide( (long)( num2 * num6 ), b );
            this.z = (int)Fix32Math.Divide( (long)( num3 * num6 ), b );
            return ( b >> 7 );
        }

        public Vector3 vector3
        {
            get
            {
                return new Vector3( this.x * PrecisionFactor, this.y * PrecisionFactor, this.z * PrecisionFactor );
            }
        }

        public FixVector2 xz
        {
            get
            {
                return new FixVector2( this.x, this.z );
            }
        }

        public int magnitude
        {
            get
            {
                long x = this.x;
                long y = this.y;
                long z = this.z;
                return Fix32Math.Sqrt( ( ( x * x ) + ( y * y ) ) + ( z * z ) );
            }
        }

        public int magnitude2D
        {
            get
            {
                long x = this.x;
                long z = this.z;
                return Fix32Math.Sqrt( ( x * x ) + ( z * z ) );
            }
        }

        public FixVector3 RotateY( ref FixFactor radians )
        {
            FixVector3 num;
            FixFactor factor;
            FixFactor factor2;
            Fix32Math.sincos( out factor, out factor2, radians.nom, radians.den );
            long num2 = factor2.nom * factor.den;
            long num3 = factor2.den * factor.nom;
            long b = factor2.den * factor.den;
            num.x = (int)Fix32Math.Divide( (long)( ( this.x * num2 ) + ( this.z * num3 ) ), b );
            num.z = (int)Fix32Math.Divide( (long)( ( -this.x * num3 ) + ( this.z * num2 ) ), b );
            num.y = 0;
            return num.NormalizeTo( 0x3e8 );
        }

        public FixVector3 RotateY( int degree )
        {
            FixVector3 num;
            FixFactor factor;
            FixFactor factor2;
            Fix32Math.sincos( out factor, out factor2, (long)( 0x7ab8 * degree ), 0x1b7740L );
            long num2 = factor2.nom * factor.den;
            long num3 = factor2.den * factor.nom;
            long b = factor2.den * factor.den;
            num.x = (int)Fix32Math.Divide( (long)( ( this.x * num2 ) + ( this.z * num3 ) ), b );
            num.z = (int)Fix32Math.Divide( (long)( ( -this.x * num3 ) + ( this.z * num2 ) ), b );
            num.y = 0;
            return num.NormalizeTo( Precision );
        }

        public int costMagnitude
        {
            get
            {
                return this.magnitude;
            }
        }

        public float worldMagnitude
        {
            get
            {
                double x = this.x;
                double y = this.y;
                double z = this.z;
                return ( ( (float)Math.Sqrt( ( ( x * x ) + ( y * y ) ) + ( z * z ) ) ) * PrecisionFactor );
            }
        }

        public double sqrMagnitude
        {
            get
            {
                double x = this.x;
                double y = this.y;
                double z = this.z;
                return ( ( ( x * x ) + ( y * y ) ) + ( z * z ) );
            }
        }

        public long sqrMagnitudeLong
        {
            get
            {
                long x = this.x;
                long y = this.y;
                long z = this.z;
                return ( ( ( x * x ) + ( y * y ) ) + ( z * z ) );
            }
        }

        public long sqrMagnitudeLong2D
        {
            get
            {
                long x = this.x;
                long z = this.z;
                return ( ( x * x ) + ( z * z ) );
            }
        }

        public int unsafeSqrMagnitude
        {
            get
            {
                return ( ( ( this.x * this.x ) + ( this.y * this.y ) ) + ( this.z * this.z ) );
            }
        }

        public FixVector3 abs
        {
            get
            {
                return new FixVector3( Math.Abs( this.x ), Math.Abs( this.y ), Math.Abs( this.z ) );
            }
        }

        [Obsolete( "Same implementation as .magnitude" )]
        public float safeMagnitude
        {
            get
            {
                double x = this.x;
                double y = this.y;
                double z = this.z;
                return (float)Math.Sqrt( ( ( x * x ) + ( y * y ) ) + ( z * z ) );
            }
        }

        [Obsolete( ".sqrMagnitude is now per default safe (.unsafeSqrMagnitude can be used for unsafe operations)" )]
        public float safeSqrMagnitude
        {
            get
            {
                float num = this.x * PrecisionFactor;
                float num2 = this.y * PrecisionFactor;
                float num3 = this.z * PrecisionFactor;
                return ( ( ( num * num ) + ( num2 * num2 ) ) + ( num3 * num3 ) );
            }
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "( ", this.x, ", ", this.y, ", ", this.z, ")" };
            return string.Concat( objArray1 );
        }

        public override bool Equals( object o )
        {
            if( o == null )
            {
                return false;
            }
            FixVector3 num = (FixVector3)o;
            return ( ( ( this.x == num.x ) && ( this.y == num.y ) ) && ( this.z == num.z ) );
        }

        public override int GetHashCode()
        {
            return ( ( ( this.x * 0x466f45d ) ^ ( this.y * 0x127409f ) ) ^ ( this.z * 0x4f9ffb7 ) );
        }

        public static FixVector3 Lerp( FixVector3 a, FixVector3 b, float f )
        {
            return new FixVector3( Mathf.RoundToInt( a.x * ( 1f - f ) ) + Mathf.RoundToInt( b.x * f ), Mathf.RoundToInt( a.y * ( 1f - f ) ) + Mathf.RoundToInt( b.y * f ), Mathf.RoundToInt( a.z * ( 1f - f ) ) + Mathf.RoundToInt( b.z * f ) );
        }

        public static FixVector3 Lerp( FixVector3 a, FixVector3 b, FixFactor f )
        {
            return new FixVector3( ( (int)Fix32Math.Divide( (long)( ( b.x - a.x ) * f.nom ), f.den ) ) + a.x, ( (int)Fix32Math.Divide( (long)( ( b.y - a.y ) * f.nom ), f.den ) ) + a.y, ( (int)Fix32Math.Divide( (long)( ( b.z - a.z ) * f.nom ), f.den ) ) + a.z );
        }

        public static FixVector3 Lerp( FixVector3 a, FixVector3 b, int factorNom, int factorDen )
        {
            return new FixVector3( Fix32Math.Divide( (int)( ( b.x - a.x ) * factorNom ), factorDen ) + a.x, Fix32Math.Divide( (int)( ( b.y - a.y ) * factorNom ), factorDen ) + a.y, Fix32Math.Divide( (int)( ( b.z - a.z ) * factorNom ), factorDen ) + a.z );
        }

        public long XZSqrMagnitude( FixVector3 rhs )
        {
            long num = this.x - rhs.x;
            long num2 = this.z - rhs.z;
            return ( ( num * num ) + ( num2 * num2 ) );
        }

        public long XZSqrMagnitude( ref FixVector3 rhs )
        {
            long num = this.x - rhs.x;
            long num2 = this.z - rhs.z;
            return ( ( num * num ) + ( num2 * num2 ) );
        }

        public bool IsEqualXZ( FixVector3 rhs )
        {
            return ( ( this.x == rhs.x ) && ( this.z == rhs.z ) );
        }

        public bool IsEqualXZ( ref FixVector3 rhs )
        {
            return ( ( this.x == rhs.x ) && ( this.z == rhs.z ) );
        }

        public static bool operator ==( FixVector3 lhs, FixVector3 rhs )
        {
            return ( ( ( lhs.x == rhs.x ) && ( lhs.y == rhs.y ) ) && ( lhs.z == rhs.z ) );
        }

        public static bool operator !=( FixVector3 lhs, FixVector3 rhs )
        {
            return ( ( ( lhs.x != rhs.x ) || ( lhs.y != rhs.y ) ) || ( lhs.z != rhs.z ) );
        }

        public static explicit operator FixVector3( Vector3 ob )
        {
            return new FixVector3( (int)Math.Round( (double)( ob.x * FloatPrecision ) ), (int)Math.Round( (double)( ob.y * FloatPrecision ) ), (int)Math.Round( (double)( ob.z * FloatPrecision ) ) );
        }

        public static explicit operator Vector3( FixVector3 ob )
        {
            return new Vector3( ob.x * PrecisionFactor, ob.y * PrecisionFactor, ob.z * PrecisionFactor );
        }

        public static FixVector3 operator -( FixVector3 lhs, FixVector3 rhs )
        {
            lhs.x -= rhs.x;
            lhs.y -= rhs.y;
            lhs.z -= rhs.z;
            return lhs;
        }

        public static FixVector3 operator -( FixVector3 lhs )
        {
            lhs.x = -lhs.x;
            lhs.y = -lhs.y;
            lhs.z = -lhs.z;
            return lhs;
        }

        public static FixVector3 operator +( FixVector3 lhs, FixVector3 rhs )
        {
            lhs.x = (int)Math.Round( (double)( lhs.x + rhs.x ) );
            lhs.y = (int)Math.Round( (double)( lhs.y + rhs.y ) );
            lhs.z = (int)Math.Round( (double)( lhs.z + rhs.z ) );

            return lhs;
        }

        public static FixVector3 operator *( FixVector3 lhs, int rhs )
        {
            lhs.x = (int)Math.Round( (double)( lhs.x * rhs ) );
            lhs.y = (int)Math.Round( (double)( lhs.y * rhs ) );
            lhs.z = (int)Math.Round( (double)( lhs.z * rhs ) );
            return lhs;
        }

        public static FixVector3 operator *( FixVector3 lhs, float rhs )
        {
            lhs.x = (int)Math.Round( (double)( lhs.x * rhs ) );
            lhs.y = (int)Math.Round( (double)( lhs.y * rhs ) );
            lhs.z = (int)Math.Round( (double)( lhs.z * rhs ) );
            return lhs;
        }

        public static FixVector3 operator *( FixVector3 lhs, double rhs )
        {
            lhs.x = (int)Math.Round( (double)( lhs.x * rhs ) );
            lhs.y = (int)Math.Round( (double)( lhs.y * rhs ) );
            lhs.z = (int)Math.Round( (double)( lhs.z * rhs ) );
            return lhs;
        }

        public static FixVector3 operator *( FixVector3 lhs, Vector3 rhs )
        {
            lhs.x = (int)Math.Round( (double)( lhs.x * rhs.x ) );
            lhs.y = (int)Math.Round( (double)( lhs.y * rhs.y ) );
            lhs.z = (int)Math.Round( (double)( lhs.z * rhs.z ) );
            return lhs;
        }

        public static FixVector3 operator *( FixVector3 lhs, FixVector3 rhs )
        {
            lhs.x = (int)Math.Round( (double)( lhs.x * rhs.x ) );
            lhs.y = (int)Math.Round( (double)( lhs.y * rhs.y ) );
            lhs.z = (int)Math.Round( (double)( lhs.z * rhs.z ) );
            return lhs;
        }

        public static FixVector3 operator /( FixVector3 lhs, float rhs )
        {
            lhs.x = (int)Math.Round( (double)( ( (float)lhs.x ) / rhs ) );
            lhs.y = (int)Math.Round( (double)( ( (float)lhs.y ) / rhs ) );
            lhs.z = (int)Math.Round( (double)( ( (float)lhs.z ) / rhs ) );
            return lhs;
        }

        public static implicit operator string( FixVector3 ob )
        {
            return ob.ToString();
        }
    }
}

