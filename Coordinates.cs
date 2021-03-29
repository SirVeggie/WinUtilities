using System;
using System.Drawing;
using System.Linq;
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
        /// <summary>Position [0,0] is the target window's upper left corner</summary>
        Window,
        /// <summary>Position [0,0] is the current mouse position</summary>
        Mouse
    }

    /// <summary>Specifies which edge of an area to target</summary>
    [Flags]
    public enum EdgeType {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>Extension methods for <see cref="EdgeType"/></summary>
    public static class EdgeTypeExtensions {

        /// <summary>Reverse the <see cref="EdgeType"/> to point at the opposite edge/corner</summary>
        public static EdgeType Reverse(this EdgeType edge) => edge.ReverseVertical().ReverseHorizontal();

        /// <summary>Reverse the <see cref="EdgeType"/> to point at the opposite horizontal edge</summary>
        public static EdgeType ReverseHorizontal(this EdgeType edge) {
            if (edge.HasFlag(EdgeType.Left)) {
                edge = edge ^ EdgeType.Left | EdgeType.Right;
            } else if (edge.HasFlag(EdgeType.Right)) {
                edge = edge ^ EdgeType.Right | EdgeType.Left;
            }

            return edge;
        }

        /// <summary>Reverse the <see cref="EdgeType"/> to point at the opposite vertical edge</summary>
        public static EdgeType ReverseVertical(this EdgeType edge) {
            if (edge.HasFlag(EdgeType.Top)) {
                edge = edge ^ EdgeType.Top | EdgeType.Bottom;
            } else if (edge.HasFlag(EdgeType.Bottom)) {
                edge = edge ^ EdgeType.Bottom | EdgeType.Top;
            }

            return edge;
        }

        /// <summary>Check if the edge is <see cref="EdgeType.None"/></summary>
        public static bool IsNone(this EdgeType edge) => edge == 0;
        /// <summary>Check if the edge is the left or the top edge</summary>
        public static bool IsTopOrLeft(this EdgeType edge) => edge.HasFlag(EdgeType.Left) || edge.HasFlag(EdgeType.Top) && !edge.IsCorner();
        /// <summary>Check if the edge has horizontal component</summary>
        public static bool IsHorizontal(this EdgeType edge) => edge.HasFlag(EdgeType.Top) || edge.HasFlag(EdgeType.Bottom);
        /// <summary>Check if the edge has vertical component</summary>
        public static bool IsVertical(this EdgeType edge) => edge.HasFlag(EdgeType.Left) || edge.HasFlag(EdgeType.Right);
        /// <summary>Check if the edge has both vertical and horizontal components</summary>
        public static bool IsCorner(this EdgeType edge) => edge.IsHorizontal() && edge.IsVertical();
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

        /// <summary></summary>
        public override string ToString() => $"{{Edge: {Type}, {Pos}}}";
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
        public Coord Size { get => size; set => size = value.AsPositive(); }

        /// <summary>Left edge of the area</summary>
        public double X { get => point.X; set => point.X = value; }
        /// <summary>Top edge of the area</summary>
        public double Y { get => point.Y; set => point.Y = value; }
        /// <summary>Width of the area</summary>
        public double W { get => size.X; set => size.X = Math.Max(0, value); }
        /// <summary>Height of the area</summary>
        public double H { get => size.Y; set => size.Y = Math.Max(0, value); }

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

        /// <summary>Returns a list of the corners' Coords in order of [TopLeft, TopRight, BottomLeft, BottomRight]</summary>
        public Edge[] Corners {
            get => new Edge[] {
                new Edge(EdgeType.TopLeft, TopLeft),
                new Edge(EdgeType.TopRight, TopRight),
                new Edge(EdgeType.BottomLeft, BottomLeft),
                new Edge(EdgeType.BottomRight, BottomRight)
            };
        }

        /// <summary>Returns a list of the edges' center positions in order of [Left, Right, Top, Bottom]</summary>
        public Edge[] Edges {
            get {
                var center = Center;

                return new Edge[] {
                    new Edge(EdgeType.Left, new Coord(Left, center.Y)),
                    new Edge(EdgeType.Right, new Coord(Right, center.Y)),
                    new Edge(EdgeType.Top, new Coord(center.X, Top)),
                    new Edge(EdgeType.Bottom, new Coord(center.X, Bottom))
                };
            }
        }

        /// <summary>A 4D vector-like magnitude</summary>
        public double Magnitude {
            get => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(W, 2) + Math.Pow(W, 2) + Math.Pow(H, 2));
            set {
                var mult = value / Magnitude;
                X *= mult;
                Y *= mult;
                W *= mult;
                H *= mult;
            }
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

        // Edges

        /// <summary>Get the top left edge object</summary>
        public Edge TopLeftEdge => new Edge(EdgeType.TopLeft, TopLeft);

        /// <summary>Get the top right edge object</summary>
        public Edge TopRightEdge => new Edge(EdgeType.TopRight, TopRight);

        /// <summary>Get the bottom left edge object</summary>
        public Edge BottomLeftEdge => new Edge(EdgeType.BottomLeft, BottomLeft);

        /// <summary>Get the bottom right edge object</summary>
        public Edge BottomRightEdge => new Edge(EdgeType.BottomRight, BottomRight);

        /// <summary>Get the left center edge object</summary>
        public Edge LeftEdge => new Edge(EdgeType.Left, new Coord(Left, Center.Y));

        /// <summary>Get the right center edge object</summary>
        public Edge RightEdge => new Edge(EdgeType.Right, new Coord(Right, Center.Y));

        /// <summary>Get the top center edge object</summary>
        public Edge TopEdge => new Edge(EdgeType.Top, new Coord(Center.X, Top));

        /// <summary>Get the bottom center edge object</summary>
        public Edge BottomEdge => new Edge(EdgeType.Bottom, new Coord(Center.X, Bottom));
        #endregion

        #endregion

        #region constructors
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public Area(double? x = null, double? y = null, double? w = null, double? h = null) {
            point = new Coord(x, y);
            size = new Coord(w, h).AsPositive();
        }

        public Area(Coord? point = null, Coord? size = null) {
            this.point = point ?? Coord.NaN;
            this.size = (size ?? Coord.NaN).AsPositive();
        }

        public Area(Area other) {
            point = other.point;
            size = other.size.AsPositive();
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion

        #region methods
        /// <summary>Create a copy while modifying the size using percentages</summary>
        public Area Slice(double wStart, double wEnd, double hStart, double hEnd) {
            if (wEnd < wStart || hEnd < hStart)
                throw new Exception("Values cannot overlap");
            var copy = this;
            copy.LeftR = X + W * wStart;
            copy.RightR = X + W * wEnd;
            copy.TopR = Y + W * hStart;
            copy.BottomR = Y + W * hEnd;
            return copy;
        }

        /// <summary>Rounds all components to closest integer</summary>
        public Area Round() => new Area(point.Round(), size.Round());

        /// <summary>Fills the current NaN values with the new ones</summary>
        public Area FillNaN(Area p) => new Area(point.Fill(p.Point), size.Fill(p.Size));

        /// <summary>Center this area in another area so that both area's center points match</summary>
        public Area CenterOn(Area other) {
            var area = this;
            area.Center = other.Center;
            return area;
        }

        /// <summary>Gets a point relative to a area's location and size. Formula is roughly [location + size * var].</summary>
        /// <param name="x">Between 0 and 1. Giving 0 targets the area's left edge and 1 targets the right edge. Giving 0.5 would target the center.</param>
        /// <param name="y">Between 0 and 1. Giving 0 targets the area's top edge and 1 targets the bottom edge. Giving 0.5 would target the center.</param>
        public Coord GetRelativePoint(double x, double y) => Point + new Coord(W * x, H * y);

        /// <summary>Get the <see cref="Edge"/> of a given <see cref="EdgeType"/></summary>
        public Edge GetEdge(EdgeType type) {
            if (type == EdgeType.Left) {
                return LeftEdge;
            } else if (type == EdgeType.Right) {
                return RightEdge;
            } else if (type == EdgeType.Top) {
                return TopEdge;
            } else if (type == EdgeType.Bottom) {
                return BottomEdge;
            } else if (type == EdgeType.TopLeft) {
                return TopLeftEdge;
            } else if (type == EdgeType.TopRight) {
                return TopRightEdge;
            } else if (type == EdgeType.BottomLeft) {
                return BottomLeftEdge;
            } else if (type == EdgeType.BottomRight) {
                return BottomRightEdge;
            }

            throw new ArgumentException("Illegal edge type, must be one of the main types");
        }

        /// <summary>Returns the <see cref="Edge"/> of the closest corner</summary>
        public Edge ClosestCorner(Coord point) => ClosestCorner(point, out _);
        /// <summary>Returns the <see cref="Edge"/> of the closest corner</summary>
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

        /// <summary>Returns the <see cref="Edge"/> of the closest center of an edge</summary>
        public Edge ClosestEdge(Coord point) => ClosestEdge(point, out _);
        /// <summary>Returns the <see cref="Edge"/> of the closest center of an edge</summary>
        public Edge ClosestEdge(Coord point, out double distance) {
            double min = double.MaxValue;
            Edge res = default;

            foreach (var edge in Edges) {
                var dist = point.Distance(edge.Pos);
                if (dist < min) {
                    min = dist;
                    res = edge;
                }
            }

            distance = min;
            return res;
        }

        /// <summary>Returns the <see cref="Edge"/> of the closest corner or center of an edge</summary>
        public Edge ClosestCornerOrEdge(Coord point) => ClosestCornerOrEdge(point, out _);
        /// <summary>Returns the <see cref="Edge"/> of the closest corner or center of an edge</summary>
        public Edge ClosestCornerOrEdge(Coord point, out double distance) {
            double min = double.MaxValue;
            Edge res = default;

            foreach (var corner in Corners) {
                var dist = point.Distance(corner.Pos);
                if (dist < min) {
                    min = dist;
                    res = corner;
                }
            }

            foreach (var edge in Edges) {
                var dist = point.Distance(edge.Pos);
                if (dist < min) {
                    min = dist;
                    res = edge;
                }
            }

            distance = min;
            return res;
        }

        /// <summary>Get the closest border to a point. Returns either the left, right, top or bottom border.</summary>
        /// <remarks>This method differs from the ClosestEdge since this method calculates the distance using the entire edge instead of just the center point of an edge.</remarks>
        public EdgeType ClosestBorder(Coord point) {
            var left = Math.Abs(point.X - Left);
            var right = Math.Abs(point.X - Right);
            var top = Math.Abs(point.Y - Top);
            var bottom = Math.Abs(point.Y - Bottom);
            var min = Math.Min(Math.Min(Math.Min(left, right), top), bottom);

            if (left == min)
                return EdgeType.Left;
            if (right == min)
                return EdgeType.Right;
            if (top == min)
                return EdgeType.Top;
            if (bottom == min)
                return EdgeType.Bottom;
            throw new Exception($"Something unexpected happened in {nameof(ClosestBorder)}");
        }

        /// <summary>Moves the area's edges using dynamic selection.</summary>
        public Area SetEdge(Edge edge, bool resize = false, bool relative = false) => SetEdge(edge.Type, edge.Pos, resize, relative);
        /// <summary>Moves the area's edges using dynamic selection. X and Y are the same.</summary>
        public Area SetEdge(EdgeType type, double pos, bool resize = false, bool relative = false) => SetEdge(type, new Coord(pos, pos), resize, relative);
        /// <summary>Moves the area's edges using dynamic selection.</summary>
        public Area SetEdge(EdgeType type, Coord pos, bool resize = false, bool relative = false) {
            var area = this;

            if (type.HasFlag(EdgeType.Left)) {
                if (resize) {
                    area.LeftR = relative ? Left + pos.X : pos.X;
                } else {
                    area.Left = relative ? Left + pos.X : pos.X;
                }
            }

            if (type.HasFlag(EdgeType.Right)) {
                if (resize) {
                    area.RightR = relative ? Right + pos.X : pos.X;
                } else {
                    area.Right = relative ? Right + pos.X : pos.X;
                }
            }

            if (type.HasFlag(EdgeType.Top)) {
                if (resize) {
                    area.TopR = relative ? Top + pos.Y : pos.Y;
                } else {
                    area.Top = relative ? Top + pos.Y : pos.Y;
                }
            }

            if (type.HasFlag(EdgeType.Bottom)) {
                if (resize) {
                    area.BottomR = relative ? Bottom + pos.Y : pos.Y;
                } else {
                    area.Bottom = relative ? Bottom + pos.Y : pos.Y;
                }
            }

            return area;
        }

        /// <summary>Add another area's point to this area's point.</summary>
        public Area AddPoint(Area other) => SetPoint(Point + other.Point);
        /// <summary>Add to the area's point.</summary>
        public Area AddPoint(double x, double y) => SetPoint(Point + new Coord(x, y));
        /// <summary>Add to the area's point.</summary>
        public Area AddPoint(Coord point) => SetPoint(Point + point);
        /// <summary>Set another area's point as this area's point.</summary>
        public Area SetPoint(Area other) => SetPoint(other.Point);
        /// <summary>Set the area's point. Helps with dotting into code.</summary>
        public Area SetPoint(double x, double y) => SetPoint(new Coord(x, y));
        /// <summary>Set the area's point. Helps with dotting into code.</summary>
        public Area SetPoint(Coord point) {
            Area area = this;
            area.Point = point;
            return area;
        }

        /// <summary>Add another area's size to this area's size.</summary>
        public Area AddSize(Area other) => SetSize(Size + other.Size);
        /// <summary>Add to the area's size.</summary>
        public Area AddSize(double x, double y) => SetSize(Size + new Coord(x, y));
        /// <summary>Add to the area's size.</summary>
        public Area AddSize(Coord size) => SetSize(Size + size);
        /// <summary>Set another area's size as this area's size.</summary>
        public Area SetSize(Area other) => SetSize(other.Size);
        /// <summary>Set the area's size. Helps with dotting into code.</summary>
        public Area SetSize(double x, double y) => SetSize(new Coord(x, y));
        /// <summary>Set the area's size. Helps with dotting into code.</summary>
        public Area SetSize(Coord size) {
            Area area = this;
            area.Size = size;
            return area;
        }

        /// <summary>Turns this area relative to the given area</summary>
        public Area SetRelative(Area area) => new Area(this).AddPoint(-area.Point);

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
            var area = new Area(this);
            area.X -= Math.Max(value, -area.W / 2);
            area.Y -= Math.Max(value, -area.H / 2);
            area.W += value * 2;
            area.H += value * 2;
            return area;
        }

        /// <summary>Return a new area that has been adjusted to fit within the clamp area</summary>
        public Area ClampWithin(Area clamp, bool resize = false) {
            var area = this;

            if (!resize) {
                if (area.W > clamp.W) {
                    area.W = clamp.W;
                }

                if (area.H > clamp.H) {
                    area.H = clamp.H;
                }
            }

            if (Left < clamp.Left) {
                if (resize) {
                    area.LeftR = clamp.Left;
                } else {
                    area.Left = clamp.Left;
                }
            }

            if (Right > clamp.Right) {
                if (resize) {
                    area.RightR = clamp.Right;
                } else {
                    area.Right = clamp.Right;
                }
            }

            if (Top < clamp.Top) {
                if (resize) {
                    area.TopR = clamp.Top;
                } else {
                    area.Top = clamp.Top;
                }
            }

            if (Bottom > clamp.Bottom) {
                if (resize) {
                    area.BottomR = clamp.Bottom;
                } else {
                    area.Bottom = clamp.Bottom;
                }
            }

            return area;
        }

        /// <summary>Return a new area that has been adjusted to exclude the given area</summary>
        public Area ClampExclude(Area areaToExclude, bool resize = false) {
            if (!Overlaps(areaToExclude))
                return this;
            var area = this;

            var left = areaToExclude.Right - Left;
            var right = Right - areaToExclude.Left;
            var top = areaToExclude.Bottom - Top;
            var bottom = Bottom - areaToExclude.Top;
            var min = Math.Min(Math.Min(Math.Min(left, right), top), bottom);

            if (left == min) {
                if (resize) {
                    area.LeftR = areaToExclude.Right;
                    if (area.Overlaps(areaToExclude)) {
                        area.Left = areaToExclude.Right;
                    }
                } else {
                    area.Left = areaToExclude.Right;
                }
            } else if (right == min) {
                if (resize) {
                    area.RightR = areaToExclude.Left;
                    if (area.Overlaps(areaToExclude)) {
                        area.Right = areaToExclude.Left;
                    }
                } else {
                    area.Right = areaToExclude.Left;
                }
            } else if (top == min) {
                if (resize) {
                    area.TopR = areaToExclude.Bottom;
                    if (area.Overlaps(areaToExclude)) {
                        area.Top = areaToExclude.Bottom;
                    }
                } else {
                    area.Top = areaToExclude.Bottom;
                }
            } else if (bottom == min) {
                if (resize) {
                    area.BottomR = areaToExclude.Top;
                    if (area.Overlaps(areaToExclude)) {
                        area.Bottom = areaToExclude.Top;
                    }
                } else {
                    area.Bottom = areaToExclude.Top;
                }
            } else {
                throw new Exception($"Something unexpected happened in {nameof(ClampExclude)}");
            }

            return area;
        }

        /// <summary>Return a new area that has been adjusted to exclude the given point</summary>
        public Area ClampExclude(Coord pointToExclude, bool resize = false) {
            if (!Contains(pointToExclude))
                return this;
            var area = this;
            var type = ClosestBorder(pointToExclude);
            if (type == EdgeType.Left || type == EdgeType.Top)
                pointToExclude += new Coord(1, 1);
            area = area.SetEdge(type, pointToExclude, resize);
            return area;
        }

        /// <summary>Return a new area that has been adjusted to include the given area</summary>
        public Area ClampInclude(Area areaToContain, bool resize = false) {
            var area = this;

            if (!resize) {
                if (area.W < areaToContain.W) {
                    area.W = areaToContain.W;
                }

                if (area.H < areaToContain.H) {
                    area.H = areaToContain.H;
                }
            }

            if (Left > areaToContain.Left) {
                if (resize) {
                    area.LeftR = areaToContain.Left;
                } else {
                    area.Left = areaToContain.Left;
                }
            }

            if (Right < areaToContain.Right) {
                if (resize) {
                    area.RightR = areaToContain.Right;
                } else {
                    area.Right = areaToContain.Right;
                }
            }

            if (Top > areaToContain.Top) {
                if (resize) {
                    area.TopR = areaToContain.Top;
                } else {
                    area.Top = areaToContain.Top;
                }
            }

            if (Bottom < areaToContain.Bottom) {
                if (resize) {
                    area.BottomR = areaToContain.Bottom;
                } else {
                    area.Bottom = areaToContain.Bottom;
                }
            }

            return area;
        }

        /// <summary>Return a new area that has been adjusted to include the given point</summary>
        public Area ClampInclude(Coord pointToContain, bool resize = false) {
            var area = this;
            if (Left > pointToContain.X) {
                if (resize) {
                    area.LeftR = pointToContain.X;
                } else {
                    area.Left = pointToContain.X;
                }
            }

            if (Right <= pointToContain.X) {
                if (resize) {
                    area.RightR = pointToContain.X + 1;
                } else {
                    area.Right = pointToContain.X + 1;
                }
            }

            if (Top > pointToContain.Y) {
                if (resize) {
                    area.TopR = pointToContain.Y;
                } else {
                    area.Top = pointToContain.Y;
                }
            }

            if (Bottom <= pointToContain.Y) {
                if (resize) {
                    area.BottomR = pointToContain.Y + 1;
                } else {
                    area.Bottom = pointToContain.Y + 1;
                }
            }
            return area;
        }

        /// <summary>Return a new area whose values have been restricted between the given areas</summary>
        public Area Clamp(Area min, Area max) {
            var x = ClampBidirection(X, min.X, max.X);
            var y = ClampBidirection(Y, min.Y, max.Y);
            var w = ClampBidirection(W, min.W, max.W);
            var h = ClampBidirection(H, min.H, max.H);
            return new Area(x, y, w, h);
        }

        private double ClampBidirection(double value, double bound1, double bound2) {
            if (bound1 <= bound2)
                return Math.Min(Math.Max(bound1, value), bound2);
            return Math.Min(Math.Max(bound2, value), bound1);
        }

        /// <summary>Return a new area that has been clamped linearly between the given areas</summary>
        public Area ClampLinear(Area start, Area end) {
            throw new NotImplementedException();
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

        /// <summary>Lerp to target area</summary>
        public Area Lerp(Area target, double t) => Lerp(this, target, t);
        /// <summary>Lerp between two areas</summary>
        public static Area Lerp(Area from, Area to, double t) => new Area(from.Point.Lerp(to.Point, t), from.Size.Lerp(to.Size, t));
        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Area(WinAPI.RECT r) => new Area(r.X, r.Y, r.Width, r.Height);
        public static implicit operator WinAPI.RECT(Area a) {
            a = a.Round();
            return new WinAPI.RECT((int) a.Left, (int) a.Top, (int) a.Right, (int) a.Bottom);
        }

        public static implicit operator Area(Rectangle r) => new Area(r.X, r.Y, r.Width, r.Height);
        public static implicit operator Rectangle(Area a) {
            a = a.Round();
            return new Rectangle((int) a.X, (int) a.Y, (int) a.W, (int) a.H);
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

        #region methods
        /// <summary>Fills the current Coord's NaN values with the other one.</summary>
        /// <returns>A copy of itself.</returns>
        public Coord Fill(Coord c) {
            var x = double.IsNaN(X) ? c.X : X;
            var y = double.IsNaN(Y) ? c.Y : Y;
            return new Coord(x, y);
        }

        /// <summary>Rounds all components to closest integer.</summary>
        /// <returns>A copy of itself.</returns>
        public Coord Round() {
            var x = Math.Round(X);
            var y = Math.Round(Y);
            return new Coord(x, y);
        }

        /// <summary>Return a new coordinate whose values were clamped to the positive range</summary>
        public Coord AsPositive() {
            var coord = this;
            coord.X = Math.Max(0, X);
            coord.Y = Math.Max(0, Y);
            return coord;
        }

        /// <summary>Turns these coordinates relative to the given coordinates</summary>
        public Coord SetRelative(double x, double y) => SetRelative(new Coord(x, y));

        /// <summary>Turns these coordinates relative to the given coordinates</summary>
        public Coord SetRelative(Coord point) => this - point;

        /// <summary>Turns these coordinates relative to the given coordinates</summary>
        public Coord SetRelative(Area area) => this - area.Point;

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
        public Coord Rotate(double degrees) {
            var rad = Math.PI / 180 * degrees;
            var x = X * Math.Cos(rad) - Y * Math.Sin(rad);
            var y = X * Math.Sin(rad) + Y * Math.Cos(rad);
            return new Coord(x, y);
        }

        /// <summary>Clamp this point inside the specified area</summary>
        public Coord Clamp(Area clamp) {
            var x = Math.Min(Math.Max(clamp.X, X), clamp.X + clamp.W);
            var y = Math.Min(Math.Max(clamp.Y, Y), clamp.Y + clamp.H);
            return new Coord(x, y);
        }

        /// <summary>Clamp this point to a line between the given points</summary>
        public Coord ClampLinear(Coord point1, Coord point2) {
            throw new NotImplementedException();

            var proj = ProjectToLine(point1, point2);

            if (proj.X < Math.Min(point1.X, point2.X)) {

            } else if (proj.X > Math.Max(point1.X, point2.X)) {

            } else {
                return proj;
            }
        }

        /// <summary>Lerp to the target coordinate</summary>
        public Coord Lerp(Coord target, double t) => Lerp(this, target, t);

        /// <summary>Lerp between two coordinates</summary>
        public static Coord Lerp(Coord a, Coord b, double t) => new Coord(a.X * (1 - t) + b.X * t, a.Y * (1 - t) + b.Y * t);

        /// <summary>Not implemented yet</summary>
        public Coord ProjectToLine(Coord point1, Coord point2) {
            throw new NotImplementedException();
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
    }
}
