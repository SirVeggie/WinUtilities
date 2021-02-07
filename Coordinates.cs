using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace WinUtilities {

    #region additional structs
    /// <summary>Specifies the type of window coordinates used</summary>
    public enum CoordType {
        /// <summary>Matches the visible area of the window</summary>
        Normal,
        /// <summary>Matches the real area of the window</summary>
        Raw,
        /// <summary>Matches the client area of the window</summary>
        Client
    }

    /// <summary>Specifies what the coordinates are relative to</summary>
    public enum CoordRelation {
        /// <summary>Position [0,0] is the primary screen's left upper corner</summary>
        Screen,
        /// <summary>Position [0,0] is the active window's upper left corner</summary>
        ActiveWindow,
        /// <summary>Position [0,0] is the current mouse position</summary>
        Mouse
    }

    /// <summary>Specifies which edge of an area to target</summary>
    [Flags]
    public enum EdgeType {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        None,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>A struct combining a target edge and a point</summary>
    public struct Edge {
        /// <summary>Edges to target</summary>
        public EdgeType Type { get; set; }
        /// <summary>Corner position</summary>
        public Coord Pos { get; set; }

        /// <summary>Create a new <see cref="Edge"/></summary>
        public Edge(EdgeType type, Coord pos) {
            Type = type;
            Pos = pos;
        }
    }
    #endregion

    /// <summary>Specifies a rectangular area in [x, y] coordinates.</summary>
    [DataContract]
    public struct Area {

        [DataMember]
        private Coord point;
        [DataMember]
        private Coord size;

        #region properties
        /// <summary>Location of the upper left corner of the area</summary>
        public Coord Point { get => point; set => point = value; }
        /// <summary>Size of the area</summary>
        public Coord Size { get => size; set => size = value; }

        /// <summary>Left edge of the area</summary>
        public double X { get => point.X; set => point.X = value; }
        /// <summary>Top edge of the area</summary>
        public double Y { get => point.Y; set => point.Y = value; }
        /// <summary>Width of the area</summary>
        public double W { get => size.X; set => size.X = value; }
        /// <summary>Height of the area</summary>
        public double H { get => size.Y; set => size.Y = value; }

        /// <summary>Left edge of the area as an int</summary>
        public int IntX => point.IntX;
        /// <summary>Right edge of the area as an int</summary>
        public int IntY => point.IntY;
        /// <summary>Width of the area as an int</summary>
        public int IntW => size.IntX;
        /// <summary>Height of the area as an int</summary>
        public int IntH => size.IntY;

        /// <summary>Check if all the components are not NaN.</summary>
        public bool IsValid => point.IsValid && size.IsValid;
        /// <summary>Check if all the components are NaN.</summary>
        public bool IsNaN => point.IsNaN && size.IsNaN;

        /// <summary>All components are 0.</summary>
        public static Area Zero => new Area(0, 0, 0, 0);
        /// <summary>All components are NaN.</summary>
        public static Area NaN => new Area(Coord.NaN, Coord.NaN);

        /// <summary>Get or set the center of the area.</summary>
        public Coord Center {
            get => new Coord(X + W / 2, Y + H / 2);
            set => point = value - size / 2;
        }

        /// <summary>Returns a list of the corners' Coords in order of [TopLeft, TopRight, BottomLeft, BottomRight].</summary>
        public Edge[] Corners {
            get => new Edge[] {
                new Edge(EdgeType.TopLeft, TopLeft),
                new Edge(EdgeType.TopRight, TopRight),
                new Edge(EdgeType.BottomLeft, BottomLeft),
                new Edge(EdgeType.BottomRight, BottomRight)
            };
        }

        #region corners and sides
        /// <summary>Get or set the Top Left corner of the area.</summary>
        public Coord TopLeft {
            get => Point;
            set => point = value;
        }
        /// <summary>Get or set the Top Right corner of the area.</summary>
        public Coord TopRight {
            get => new Coord(Right, Top);
            set => point = value - new Coord(W, 0);
        }
        /// <summary>Get or set the Bottom Left corner of the area.</summary>
        public Coord BottomLeft {
            get => new Coord(Left, Bottom);
            set => point = value - new Coord(0, H);
        }
        /// <summary>Get or set the Bottom Right corner of the area.</summary>
        public Coord BottomRight {
            get => new Coord(Right, Bottom);
            set => point = value - new Coord(W, H);
        }

        /// <summary>Get or set the Left edge of the area.</summary>
        public double Left {
            get => X;
            set => X = value;
        }
        /// <summary>Get or set the Right edge of the area.</summary>
        public double Right {
            get => X + W;
            set => X = value - W;
        }
        /// <summary>Get or set the Top edge of the area.</summary>
        public double Top {
            get => Y;
            set => Y = value;
        }
        /// <summary>Get or set the Bottom edge of the area.</summary>
        public double Bottom {
            get => Y + H;
            set => Y = value - H;
        }

        // Resizeable versions

        /// <summary>Resize from Top Left corner of the area.</summary>
        public Coord TopLeftR {
            get => TopLeft;
            set {
                LeftR = value.X;
                TopR = value.Y;
            }
        }
        /// <summary>Resize from Top Right corner of the area.</summary>
        public Coord TopRightR {
            get => TopRight;
            set {
                RightR = value.X;
                TopR = value.Y;
            }
        }
        /// <summary>Resize from Bottom Left corner of the area.</summary>
        public Coord BottomLeftR {
            get => BottomLeft;
            set {
                LeftR = value.X;
                BottomR = value.Y;
            }
        }
        /// <summary>Resize from Bottom Right corner of the area.</summary>
        public Coord BottomRightR {
            get => BottomRight;
            set {
                RightR = value.X;
                BottomR = value.Y;
            }
        }

        /// <summary>Resize from Left edge of the area.</summary>
        public double LeftR {
            get => Left;
            set {
                W += X - value;
                X = value;
            }
        }
        /// <summary>Resize from Right edge of the area.</summary>
        public double RightR {
            get => Right;
            set {
                W = value - X;
            }
        }
        /// <summary>Resize from Top edge of the area.</summary>
        public double TopR {
            get => Top;
            set {
                H += Y - value;
                Y = value;
            }
        }
        /// <summary>Resize from Bottom edge of the area.</summary>
        public double BottomR {
            get => Bottom;
            set {
                H = value - Y;
            }
        }
        #endregion

        #endregion

        #region constructors
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Area(double? x = null, double? y = null, double? w = null, double? h = null) {
            point = new Coord(x, y);
            size = new Coord(w, h);
        }

        public Area(Coord? point = null, Coord? size = null) {
            this.point = point ?? Coord.NaN;
            this.size = size ?? Coord.NaN;
        }

        public Area(Area other) {
            point = other.point;
            size = other.size;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Area(WinAPI.RECT r) => new Area(r.X, r.Y, r.Width, r.Height);
        public static implicit operator WinAPI.RECT(Area a) {
            a.Round();
            return new WinAPI.RECT((int) a.Left, (int) a.Top, (int) a.Right, (int) a.Bottom);
        }

        public static implicit operator Area(Rectangle r) => new Area(r.X, r.Y, r.Width, r.Height);
        public static implicit operator Rectangle(Area a) {
            a.Round();
            return new Rectangle((int) a.Left, (int) a.Top, (int) a.Right, (int) a.Bottom);
        }

        public static implicit operator Point(Area a) => new Point((int) a.X, (int) a.Y);
        public static implicit operator Size(Area a) => new Size((int) a.W, (int) a.H);

        public static bool operator ==(Area a, Area b) => a.Point == b.Point && a.Size == b.Size;
        public static bool operator !=(Area a, Area b) => !(a == b);
        public static Area operator +(Area a, Area b) => new Area(a.Point + b.Point, a.Size + b.Size);
        public static Area operator -(Area a, Area b) => new Area(a.Point - b.Point, a.Size - b.Size);
        public static Area operator -(Area a) => new Area(-a.Point, -a.Size);
        public static Area operator *(Area a, Area b) => new Area(a.Point * b.Point, a.Size * b.Size);
        public static Area operator /(Area a, Area b) => new Area(a.Point / b.Point, a.Size / b.Size);
        public static Area operator *(Area a, double b) => new Area(a.X * b, a.Y * b, a.W * b, a.H * b);
        public static Area operator *(double a, Area b) => new Area(b.X * a, b.Y * a, b.W * a, b.H * a);
        public static Area operator /(Area a, double b) => new Area(a.X / b, a.Y / b, a.W / b, a.H / b);
        public override bool Equals(object obj) => obj is Area && this == (Area) obj;
        public override string ToString() => "{" + X + ", " + Y + ", " + W + ", " + H + "}";
        public override int GetHashCode() {
            int hashCode = 1392910933;
            hashCode = hashCode * -1521134295 + Point.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region methods
        /// <summary>Create a copy while modifying the size using percentages.</summary>
        public Area Copy(double wStart, double wEnd, double hStart, double hEnd) {
            if (wEnd < wStart || hEnd < hStart)
                throw new Exception("Values cannot overlap");
            var copy = this;
            copy.LeftR = X + W * wStart;
            copy.RightR = X + W * wEnd;
            copy.TopR = Y + W * hStart;
            copy.BottomR = Y + W * hEnd;
            return copy;
        }

        /// <summary>Rounds all components to closest integer.</summary>
        /// <returns>A copy of itself.</returns>
        public Area Round() {
            point.Round();
            size.Round();
            return this;
        }

        /// <summary>Fills the current NaN values with the new ones.</summary>
        /// <returns>A copy of itself.</returns>
        public Area FillNaN(Area p) {
            point.Fill(p.Point);
            size.Fill(p.Size);
            return this;
        }

        /// <summary>Returns the Coord of the closest corner.</summary>
        public Edge ClosestCorner(Coord point) => ClosestCorner(point, out var d);
        /// <summary>Returns the Coord of the closest corner.</summary>
        public Edge ClosestCorner(Coord point, out double distance) {
            double min = double.MaxValue;
            Edge res = default;

            foreach (var corner in Corners) {
                var dist = point.Distance(corner.Pos);
                if (dist < min) {
                    min = dist;
                    res = corner;
                }
            }

            distance = min;
            return res;
        }

        /// <summary>Moves the area's edges using dynamic selection.</summary>
        public Area SetEdge(Edge edge, bool resize = false, bool relative = false) => SetEdge(edge.Type, edge.Pos, resize, relative);
        /// <summary>Moves the area's edges using dynamic selection. X and Y are the same.</summary>
        public Area SetEdge(EdgeType type, double pos, bool resize = false, bool relative = false) => SetEdge(type, new Coord(pos, pos), resize, relative);
        /// <summary>Moves the area's edges using dynamic selection.</summary>
        public Area SetEdge(EdgeType type, Coord pos, bool resize = false, bool relative = false) {
            if (type.HasFlag(EdgeType.Left)) {
                if (resize) {
                    LeftR = relative ? Left + pos.X : pos.X;
                } else {
                    Left = relative ? Left + pos.X : pos.X;
                }
            }

            if (type.HasFlag(EdgeType.Right)) {
                if (resize) {
                    RightR = relative ? Right + pos.X : pos.X;
                } else {
                    Right = relative ? Right + pos.X : pos.X;
                }
            }

            if (type.HasFlag(EdgeType.Top)) {
                if (resize) {
                    TopR = relative ? Top + pos.Y : pos.Y;
                } else {
                    Top = relative ? Top + pos.Y : pos.Y;
                }
            }

            if (type.HasFlag(EdgeType.Bottom)) {
                if (resize) {
                    BottomR = relative ? Bottom + pos.Y : pos.Y;
                } else {
                    Bottom = relative ? Bottom + pos.Y : pos.Y;
                }
            }

            return this;
        }

        /// <summary>Add another area's point to this area's point.</summary>
        public Area AddPoint(Area other) => SetPoint(Point + other.Point);
        /// <summary>Add to the area's point.</summary>
        public Area AddPoint(Coord point) => SetPoint(Point + point);
        /// <summary>Set another area's point as this area's point.</summary>
        public Area SetPoint(Area other) => SetPoint(other.Point);
        /// <summary>Set the area's point. Helps with dotting into code.</summary>
        public Area SetPoint(Coord point) {
            Area n = this;
            n.Point = point;
            return n;
        }

        /// <summary>Add another area's size to this area's size.</summary>
        public Area AddSize(Area other) => SetSize(Size + other.Size);
        /// <summary>Add to the area's size.</summary>
        public Area AddSize(Coord size) => SetSize(Size + size);
        /// <summary>Set another area's size as this area's size.</summary>
        public Area SetSize(Area other) => SetSize(other.Size);
        /// <summary>Set the area's size. Helps with dotting into code.</summary>
        public Area SetSize(Coord size) {
            Area n = this;
            n.Size = size;
            return n;
        }

        /// <summary>Turns screen coordinates relative to parent area.</summary>
        public Coord Relative(double x, double y) => Relative(new Coord(x, y));
        /// <summary>Turns screen coordinates relative to parent area.</summary>
        public Coord Relative(Coord point) => point - Point;
        /// <summary>Turns screen coordinates relative to parent area.</summary>
        public Area Relative(Area area) => area.SetPoint(Relative(area.Point));

        /// <summary>Checks if the given point is within the area.</summary>
        public bool Contains(Coord point) => Contains(point.X, point.Y);

        /// <summary>Checks if the given point is within the area.</summary>
        public bool Contains(double x, double y) {
            return x >= Left && x < Right
            && y >= Top && y < Bottom;
        }

        /// <summary>Returns true if the parent fully contains the given position.</summary>
        public bool Contains(Area pos) {
            return pos.Left >= Left && pos.Right <= Right
            && pos.Top >= Top && pos.Bottom <= Bottom;
        }

        /// <summary>Returns true if the two positions overlap.</summary>
        public bool Overlaps(Area pos) {
            return pos.Left < Right && pos.Right > Left
            && pos.Top < Bottom && pos.Bottom > Top;
        }

        /// <summary>Returns true if the two positions overlap or their edges touch.</summary>
        public bool Touches(Area pos) {
            return pos.Left <= Right && pos.Right >= Left
            && pos.Top <= Bottom && pos.Bottom >= Top;
        }

        /// <summary>Grows the area outwards by the specified value. Shrinks if negative.</summary>
        public Area Grow(double value) {
            var temp = new Area(this);
            temp.X -= value;
            temp.Y -= value;
            temp.W += value * 2;
            temp.H += value * 2;
            return temp;
        }

        /// <summary>Clamp the given <paramref name="area"/> to the parent area.</summary>
        public Area Clamp(Area area) => Clamp(area, this);
        /// <summary>Moves the given area to within the clamp area. Area is resized if necessary.</summary>
        public static Area Clamp(Area pos, Area clamp) {
            if (pos.Left < clamp.Left) pos.Left = clamp.Left;
            if (pos.Right > clamp.Right) pos.RightR = clamp.Right;
            if (pos.Top < clamp.Top) pos.Top = clamp.Top;
            if (pos.Bottom > clamp.Bottom) pos.BottomR = clamp.Bottom;
            return pos;
        }

        /// <summary>Clamp the given <paramref name="point"/> to the parent area.</summary>
        public Coord Clamp(Coord point) => Clamp(point, this);
        /// <summary>Moves the given point within the clamped area.</summary>
        public static Coord Clamp(Coord point, Area clamp) {
            if (point.X < clamp.Left) point.X = clamp.Left;
            else if (point.X >= clamp.Right) point.X = clamp.Right - 1;

            if (point.Y < clamp.Top) point.Y = clamp.Top;
            else if (point.Y >= clamp.Bottom) point.Y = clamp.Bottom - 1;

            return point;
        }

        /// <summary>Takes the mutual area between two areas.</summary>
        public static Area Mutual(Area a, Area b) {
            Area res = default;
            res.Left = Math.Max(a.Left, b.Left);
            res.RightR = Math.Min(a.Right, b.Right);
            res.Top = Math.Max(a.Top, b.Top);
            res.BottomR = Math.Min(a.Bottom, b.Bottom);
            return res;
        }
        #endregion
    }

    /// <summary>
    /// Has coordinate (point), vector and rectangle (size) properties.
    /// A hybrid of those.
    /// </summary>
    [DataContract]
    public struct Coord {

        /// <summary>X value of the coordinate</summary>
        [DataMember]
        public double X { get; set; }
        /// <summary>Y value of the coordinate</summary>
        [DataMember]
        public double Y { get; set; }

        #region properties
        /// <summary>X value of the coordinate as an int</summary>
        public int IntX => (int) Math.Round(X);
        /// <summary>Y value of the coordinate as an int</summary>
        public int IntY => (int) Math.Round(Y);

        /// <summary>All components are 0.</summary>
        public static Coord Zero => new Coord(0, 0);
        /// <summary>All components are NaN.</summary>
        public static Coord NaN => new Coord(double.NaN, double.NaN);

        /// <summary>Check if all the components are not NaN.</summary>
        public bool IsValid => !double.IsNaN(X) && !double.IsNaN(Y);
        /// <summary>Check if all the components are NaN.</summary>
        public bool IsNaN => double.IsNaN(X) && double.IsNaN(Y);

        /// <summary>Gives the current Coord as a single integer.</summary>
        public int AsValue => (IntY << 16) | (IntX & 0xFFFF);

        /// <summary>
        /// Point's distance from origin. Naming comes from vectors.
        /// </summary>
        public double Magnitude {
            get => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
            set {
                var mult = value / Magnitude;
                X *= mult;
                Y *= mult;
                var a = (this.Magnitude = 1);
            }
        }

        /// <summary>Gets a copy whose distance to the origin is set to 1</summary>
        public Coord Normalized {
            get {
                Coord copy = this;
                copy.Magnitude = 1;
                return copy;
            }
        }

        /// <summary>Gets the surface area of the area object</summary>
        public double SurfaceArea => X * Y;
        #endregion

        #region constructors
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Coord(double? x = null, double? y = null) {
            X = x ?? double.NaN;
            Y = y ?? double.NaN;
        }

        public Coord(Coord other) {
            X = other.X;
            Y = other.Y;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>Get coordinate from an int where the first 16 bits are the x value and the last 16 are the y value</summary>
        public static Coord FromInt(int value) {
            var x = value & 0x0000FFFF;
            var y = value >> 16;
            return new Coord(x, y);
        }
        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Coord(WinAPI.POINT p) => new Coord(p.X, p.Y);
        public static implicit operator WinAPI.POINT(Coord c) => new WinAPI.POINT(c.IntX, c.IntY);
        public static implicit operator Coord(Point p) => new Coord(p.X, p.Y);
        public static implicit operator Point(Coord c) => new Point(c.IntX, c.IntY);
        public static implicit operator Coord(Size s) => new Coord(s.Width, s.Height);
        public static implicit operator Size(Coord c) => new Size(c.IntX, c.IntY);

        public static bool operator ==(Coord a, Coord b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Coord a, Coord b) => !(a == b);
        public static Coord operator +(Coord a, Coord b) => new Coord(double.IsNaN(a.X + b.X) ? throw new Exception("Operator fail on value that contains NaN") : a.X + b.X, double.IsNaN(a.Y + b.Y) ? throw new Exception("Operator fail on value that contains NaN") : a.Y + b.Y);
        public static Coord operator -(Coord a, Coord b) => new Coord(double.IsNaN(a.X - b.X) ? throw new Exception("Operator fail on value that contains NaN") : a.X - b.X, double.IsNaN(a.Y - b.Y) ? throw new Exception("Operator fail on value that contains NaN") : a.Y - b.Y);
        public static Coord operator -(Coord a) => new Coord(-a.X, -a.Y);
        public static Coord operator *(Coord a, Coord b) => new Coord(double.IsNaN(a.X * b.X) ? throw new Exception("Operator fail on value that contains NaN") : a.X * b.X, double.IsNaN(a.Y * b.Y) ? throw new Exception("Operator fail on value that contains NaN") : a.Y * b.Y);
        public static Coord operator /(Coord a, Coord b) => new Coord(double.IsNaN(a.X / b.X) ? throw new Exception("Operator fail on value that contains NaN") : a.X / b.X, double.IsNaN(a.Y / b.Y) ? throw new Exception("Operator fail on value that contains NaN") : a.Y / b.Y);
        public static Coord operator *(Coord a, double b) => new Coord(a.X * b, a.Y * b);
        public static Coord operator *(double a, Coord b) => new Coord(b.X * a, b.Y * a);
        public static Coord operator /(Coord a, double b) => new Coord(a.X / b, a.Y / b);
        public override bool Equals(object obj) => obj is Coord && this == (Coord) obj;
        public override string ToString() => "{" + X + ", " + Y + "}";
        public override int GetHashCode() {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        private static double DefNaN(double value, double def) {
            if (double.IsNaN(value))
                return def;
            return value;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region methods
        /// <summary>Fills the current Coord's NaN values with the other one.</summary>
        /// <returns>A copy of itself.</returns>
        public Coord Fill(Coord c) {
            X = double.IsNaN(X) ? c.X : X;
            Y = double.IsNaN(Y) ? c.Y : Y;
            return this;
        }

        /// <summary>Rounds all components to closest integer.</summary>
        /// <returns>A copy of itself.</returns>
        public Coord Round() {
            X = Math.Round(X);
            Y = Math.Round(Y);
            return this;
        }

        /// <summary>Turns screen coordinates relative to parent coordinates.</summary>
        public Coord Relative(double x, double y) => Relative(new Coord(x, y));

        /// <summary>Turns screen coordinates relative to parent coordinates.</summary>
        public Coord Relative(Coord point) => point - this;

        /// <summary>Distance to the given point.</summary>
        public double Distance(double x, double y) => Distance(new Coord(x, y));

        /// <summary>Distance to the given point.</summary>
        public double Distance(Coord other) => (this - other).Magnitude;

        /// <summary>Returns the square distance to the point, so Max(dx, dy).</summary>
        public double SqDistance(double x, double y) => SqDistance(new Coord(x, y));

        /// <summary>Returns the square distance to the point, so Max(dx, dy).</summary>
        public double SqDistance(Coord other) {
            var dx = Math.Abs(X - other.X);
            var dy = Math.Abs(Y - other.Y);
            return Math.Max(dx, dy);
        }

        /// <summary>Rotate as a vector.</summary>
        public void Rotate(double degrees) {
            var old = this;
            var rad = Math.PI / 180 * degrees;

            X = old.X * Math.Cos(rad) - old.Y * Math.Sin(rad);
            Y = old.X * Math.Sin(rad) + old.Y * Math.Cos(rad);
        }
        #endregion
    }
}
