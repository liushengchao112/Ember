using System;
using System.Runtime.InteropServices;

namespace Utils
{
    [StructLayout( LayoutKind.Sequential )]
    public struct FixRect
    {
        private int l;
        private int t;
        private int w;
        private int h;

        public FixRect( int left, int top, int width, int height )
        {
            this.l = left;
            this.t = top;
            this.w = width;
            this.h = height;
        }

        public FixRect( FixRect source )
        {
            this.l = source.l;
            this.t = source.t;
            this.w = source.w;
            this.h = source.h;
        }

        public static FixRect MinMaxRect( int left, int top, int right, int bottom )
        {
            return new FixRect( left, top, right - left, bottom - top );
        }

        public void Set( int left, int top, int width, int height )
        {
            this.l = left;
            this.t = top;
            this.w = width;
            this.h = height;
        }

        public int x
        {
            get
            {
                return this.l;
            }
            set
            {
                this.l = value;
            }
        }

        public int y
        {
            get
            {
                return this.t;
            }
            set
            {
                this.t = value;
            }
        }

        public FixVector2 position
        {
            get
            {
                return new FixVector2( this.l, this.t );
            }
            set
            {
                this.l = value.x;
                this.t = value.y;
            }
        }

        public FixVector2 center
        {
            get
            {
                return new FixVector2( this.x + ( this.w >> 1 ), this.y + ( this.h >> 1 ) );
            }
            set
            {
                this.l = value.x - ( this.w >> 1 );
                this.t = value.y - ( this.h >> 1 );
            }
        }

        public FixVector2 min
        {
            get
            {
                return new FixVector2( this.xMin, this.yMin );
            }
            set
            {
                this.xMin = value.x;
                this.yMin = value.y;
            }
        }

        public FixVector2 max
        {
            get
            {
                return new FixVector2( this.xMax, this.yMax );
            }
            set
            {
                this.xMax = value.x;
                this.yMax = value.y;
            }
        }

        public int width
        {
            get
            {
                return this.w;
            }
            set
            {
                this.w = value;
            }
        }

        public int height
        {
            get
            {
                return this.h;
            }
            set
            {
                this.h = value;
            }
        }

        public FixVector2 size
        {
            get
            {
                return new FixVector2( this.w, this.h );
            }
            set
            {
                this.w = value.x;
                this.h = value.y;
            }
        }

        public int xMin
        {
            get
            {
                return this.l;
            }
            set
            {
                int xMax = this.xMax;
                this.l = value;
                this.w = xMax - this.l;
            }
        }

        public int yMin
        {
            get
            {
                return this.t;
            }
            set
            {
                int yMax = this.yMax;
                this.t = value;
                this.h = yMax - this.t;
            }
        }

        public int xMax
        {
            get
            {
                return ( this.w + this.l );
            }
            set
            {
                this.w = value - this.l;
            }
        }

        public int yMax
        {
            get
            {
                return ( this.h + this.t );
            }
            set
            {
                this.h = value - this.t;
            }
        }

        public override string ToString()
        {
            object[] args = new object[] { this.x, this.y, this.width, this.height };
            return string.Format( "(x:{0:F2}, y:{1:F2}, width:{2:F2}, height:{3:F2})", args );
        }

        public string ToString( string format )
        {
            object[] args = new object[] {
                this.x.ToString( format ),
                this.y.ToString( format ),
                this.width.ToString( format ),
                this.height.ToString( format )
            };
            return string.Format( "(x:{0}, y:{1}, width:{2}, height:{3})", args );
        }

        public bool Contains( FixVector2 point )
        {
            return ( ( ( ( point.x >= this.xMin ) && ( point.x < this.xMax ) ) && ( point.y >= this.yMin ) ) && ( point.y < this.yMax ) );
        }

        public bool Contains( FixVector3 point )
        {
            return ( ( ( ( point.x >= this.xMin ) && ( point.x < this.xMax ) ) && ( point.y >= this.yMin ) ) && ( point.y < this.yMax ) );
        }

        public bool Contains( FixVector3 point, bool allowInverse )
        {
            if( !allowInverse )
            {
                return this.Contains( point );
            }
            bool flag = false;
            if( ( ( ( this.width < 0f ) && ( point.x <= this.xMin ) ) && ( point.x > this.xMax ) ) || ( ( ( this.width >= 0f ) && ( point.x >= this.xMin ) ) && ( point.x < this.xMax ) ) )
            {
                flag = true;
            }
            return ( flag && ( ( ( ( this.height < 0f ) && ( point.y <= this.yMin ) ) && ( point.y > this.yMax ) ) || ( ( ( this.height >= 0f ) && ( point.y >= this.yMin ) ) && ( point.y < this.yMax ) ) ) );
        }

        private static FixRect OrderMinMax( FixRect rect )
        {
            if( rect.xMin > rect.xMax )
            {
                int xMin = rect.xMin;
                rect.xMin = rect.xMax;
                rect.xMax = xMin;
            }
            if( rect.yMin > rect.yMax )
            {
                int yMin = rect.yMin;
                rect.yMin = rect.yMax;
                rect.yMax = yMin;
            }
            return rect;
        }

        public bool Overlaps( FixRect other )
        {
            return ( ( ( ( other.xMax > this.xMin ) && ( other.xMin < this.xMax ) ) && ( other.yMax > this.yMin ) ) && ( other.yMin < this.yMax ) );
        }

        public bool Overlaps( FixRect other, bool allowInverse )
        {
            FixRect rect = this;
            if( allowInverse )
            {
                rect = OrderMinMax( rect );
                other = OrderMinMax( other );
            }
            return rect.Overlaps( other );
        }

        public override int GetHashCode()
        {
            return ( ( ( this.x.GetHashCode() ^ ( this.width.GetHashCode() << 2 ) ) ^ ( this.y.GetHashCode() >> 2 ) ) ^ ( this.height.GetHashCode() >> 1 ) );
        }

        public override bool Equals( object other )
        {
            if( !( other is FixRect ) )
            {
                return false;
            }
            FixRect rect = (FixRect)other;
            return ( ( ( this.x.Equals( rect.x ) && this.y.Equals( rect.y ) ) && this.width.Equals( rect.width ) ) && this.height.Equals( rect.height ) );
        }

        public static bool operator !=( FixRect lhs, FixRect rhs )
        {
            return ( ( ( ( lhs.x != rhs.x ) || ( lhs.y != rhs.y ) ) || ( lhs.width != rhs.width ) ) || ( lhs.height != rhs.height ) );
        }

        public static bool operator ==( FixRect lhs, FixRect rhs )
        {
            return ( ( ( ( lhs.x == rhs.x ) && ( lhs.y == rhs.y ) ) && ( lhs.width == rhs.width ) ) && ( lhs.height == rhs.height ) );
        }
    }
}

