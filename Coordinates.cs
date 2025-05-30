﻿using System;
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

        /// <summary>Rotate the <see cref="EdgeType"/> in either direction</summary>
        /// <remarks>If the <see cref="EdgeType"/> is a corner, then rotating is done between corners, otherwise rotating is done between non-corners</remarks>
        public static EdgeType Rotate(this EdgeType dir, bool clockwise = true) {
            EdgeType result;

            if (!dir.IsCorner()) {
                switch (dir) {
                case EdgeType.None:
                    result = EdgeType.None;
                    break;
                case EdgeType.Left:
                    result = EdgeType.Top;
                    break;
                case EdgeType.Right:
                    result = EdgeType.Bottom;
                    break;
                case EdgeType.Top:
                    result = EdgeType.Right;
                    break;
                case EdgeType.Bottom:
                    result = EdgeType.Left;
                    break;
                default:
                    result = dir;
                    break;
                }
            } else {
                switch (dir) {
                case EdgeType.None:
                    result = EdgeType.None;
                    break;
                case EdgeType.TopLeft:
                    result = EdgeType.TopRight;
                    break;
                case EdgeType.TopRight:
                    result = EdgeType.BottomRight;
                    break;
                case EdgeType.BottomLeft:
                    result = EdgeType.TopLeft;
                    break;
                case EdgeType.BottomRight:
                    result = EdgeType.BottomLeft;
                    break;
                default:
                    result = dir;
                    break;
                }
            }

            if (clockwise)
                return result;
            return result.Reverse();
        }

        /// <summary>Rotate the <see cref="EdgeType"/> certain amount of steps in either direction</summary>
        /// <remarks>If the <see cref="EdgeType"/> is a corner, then rotating is done between corners, otherwise rotating is done between non-corners</remarks>
        public static EdgeType Rotate(this EdgeType type, int steps) {
            if (steps == 0)
                return type;
            steps = ((Math.Abs(steps) - 1) % 4 + 1) * (steps < 0 ? -1 : 1);
            for (int i = 0; i < steps; i++)
                type = type.Rotate(steps > 0);
            return type;
        }

        /// <summary>Check if the EdgeType includes the Left flag</summary>
        public static bool HasLeft(this EdgeType edge) => edge.HasFlag(EdgeType.Left);
        /// <summary>Check if the EdgeType includes the Right flag</summary>
        public static bool HasRight(this EdgeType edge) => edge.HasFlag(EdgeType.Right);
        /// <summary>Check if the EdgeType includes the Top flag</summary>
        public static bool HasTop(this EdgeType edge) => edge.HasFlag(EdgeType.Top);
        /// <summary>Check if the EdgeType includes the Bottom flag</summary>
        public static bool HasBottom(this EdgeType edge) => edge.HasFlag(EdgeType.Bottom);

        /// <summary>Check if the edge is <see cref="EdgeType.None"/></summary>
        public static bool IsNone(this EdgeType edge) => edge == 0;
        /// <summary>Check if the edge is the left or the top edge (not a corner)</summary>
        public static bool IsTopOrLeft(this EdgeType edge) => edge.HasFlag(EdgeType.Left) || edge.HasFlag(EdgeType.Top) && !edge.IsCorner();
        /// <summary>Check if the edge has horizontal component</summary>
        public static bool IsHorizontal(this EdgeType edge) => edge.HasFlag(EdgeType.Left) || edge.HasFlag(EdgeType.Right);
        /// <summary>Check if the edge has vertical component</summary>
        public static bool IsVertical(this EdgeType edge) => edge.HasFlag(EdgeType.Top) || edge.HasFlag(EdgeType.Bottom);
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

        /// <summary>Area is not valid if size is negative</summary>
        public bool IsValid => W >= 0 && H >= 0;
        /// <summary>True if all components are 0</summary>
        public bool IsZero => point.IsZero && size.IsZero;
        /// <summary>True if any of the values is NaN</summary>
        public bool HasNan => Point.HasNan || Size.HasNan;

        /// <summary>All components are 0</summary>
        public static Area Zero => new Area(0, 0, 0, 0);

        /// <summary>Get or set the center of the area</summary>
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

        /// <summary>Surface area of the <see cref="Area"/></summary>
        public double Surface => W * H;

        /// <summary>Diameter of the <see cref="Area"/></summary>
        public double Diameter => W + W + H + H;

        /// <summary>Area is in landscape form</summary>
        /// <remarks>Square areas are not counted</remarks>
        public bool IsLandscape => Math.Abs(W) > Math.Abs(H);

        /// <summary>Area is in portrait form</summary>
        /// <remarks>Square areas are not counted</remarks>
        public bool IsPortrait => Math.Abs(W) < Math.Abs(H);

        /// <summary>Area is a square</summary>
        public bool IsSquare => Math.Abs(W) == Math.Abs(H);

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
        public Area(double x, double y, double w, double h) {
            point = new Coord(x, y);
            size = new Coord(w, h);
        }

        public Area(Coord point, Coord size) {
            this.point = point;
            this.size = size;
        }

        public Area(Coord point, double w, double h) {
            this.point = point;
            this.size = new Coord(w, h);
        }

        public Area(double x, double y, Coord size) {
            this.point = new Coord(x, y);
            this.size = size;
        }

        public Area(Area other) {
            point = other.point;
            size = other.size;
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

        /// <summary>Sets negative size values to 0</summary>
        public Area Validate() {
            var area = this;
            area.Size = area.Size.AsPositive();
            return area;
        }

        /// <summary>Center this area in another area so that both area's center points match</summary>
        public Area SetCenter(Area other) {
            var area = this;
            area.Center = other.Center;
            return area;
        }

        /// <summary>Center this area to the specified point</summary>
        public Area SetCenter(Coord point) {
            var area = this;
            area.Center = point;
            return area;
        }

        /// <summary>Center the x axis of this area to the given value</summary>
        public Area SetCenterX(double x) {
            var area = this;
            area.Center = new Coord(x, area.Center.Y);
            return area;
        }

        /// <summary>Center the y axis of this area to the given value</summary>
        public Area SetCenterY(double y) {
            var area = this;
            area.Center = new Coord(area.Center.X, y);
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

        /// <summary>Get the [Left, Right, Top, Bottom] of the area as a double</summary>
        public double GetSide(EdgeType type) {
            if (type == EdgeType.Left) return Left;
            if (type == EdgeType.Right) return Right;
            if (type == EdgeType.Top) return Top;
            if (type == EdgeType.Bottom) return Bottom;
            throw new ArgumentException("Invalid edge type, must be left, right, top or bottom");
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

        /// <summary>Set X value</summary>
        public Area SetX(double value) {
            Area a = this;
            a.X = value;
            return a;
        }
        /// <summary>Set Y value</summary>
        public Area SetY(double value) {
            Area a = this;
            a.Y = value;
            return a;
        }
        /// <summary>Set W value</summary>
        public Area SetW(double value) {
            Area a = this;
            a.W = value;
            return a;
        }
        /// <summary>Set H value</summary>
        public Area SetH(double value) {
            Area a = this;
            a.H = value;
            return a;
        }
        /// <summary>Add to X value</summary>
        public Area AddX(double value) {
            Area a = this;
            a.X += value;
            return a;
        }
        /// <summary>Add to Y value</summary>
        public Area AddY(double value) {
            Area a = this;
            a.Y += value;
            return a;
        }
        /// <summary>Add to W value</summary>
        public Area AddW(double value) {
            Area a = this;
            a.W += value;
            return a;
        }
        /// <summary>Add to H value</summary>
        public Area AddH(double value) {
            Area a = this;
            a.H += value;
            return a;
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
        /// <remarks>Regarding special case of 0-sized areas (W or H == 0), they count as overlapping if touching the other area (same as contains)</remarks>
        public bool Overlaps(Area pos) {
            return (pos.Left == Left || pos.Right == Right || (pos.Left < Right && pos.Right > Left))
                && (pos.Top == Top || pos.Bottom == Bottom || (pos.Top < Bottom && pos.Bottom > Top));
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
        public Area ClampValues(Area min, Area max) {
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

        /// <summary>Return a new area that has been mapped from one area to another while maintaining its relative area</summary>
        /// <param name="from">Original area</param>
        /// <param name="to">Target area</param>
        /// <param name="resize">Set if the area is allowed to resize according to the relative difference between the areas</param>
        public Area Map(Area from, Area to, bool resize = false) {
            Area copy = this;

            if (resize) {
                copy.Point = Point.Map(from, to);
                copy.W *= to.W / from.W;
                copy.H *= to.H / from.H;
            } else {
                Area from2 = from.AddSize(-Size).AddPoint(Size / 2);
                Area to2 = to.AddSize(-Size).AddPoint(Size / 2);

                if (from2.W < 0)
                    from2 = from2.SetW(0).SetX(from.Center.X);
                if (from2.H < 0)
                    from2 = from2.SetH(0).SetY(from.Center.Y);
                if (to2.W < 0)
                    to2 = to2.SetW(0).SetX(to.Center.X);
                if (to2.H < 0)
                    to2 = to2.SetH(0).SetY(to.Center.Y);

                Coord center = Center;
                if (center.X >= from2.Left && center.X <= from2.Right) {
                    from = from.SetX(from2.X).SetW(from2.W);
                    to = to.SetX(to2.X).SetW(to2.W);
                }
                if (center.Y >= from2.Top && center.Y <= from2.Bottom) {
                    from = from.SetY(from2.Y).SetH(from2.H);
                    to = to.SetY(to2.Y).SetH(to2.H);
                }

                copy = copy.SetCenter(Center.Map(from, to));
            }

            return copy;
        }

        /// <summary>Takes the mutual area between two areas. Hint: use IsValid for area validation.</summary>
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

        /// <summary>Return a new <see cref="Area"/> that is mirrored on the given point on the X axis</summary>
        public Area MirrorX(Coord point) => MirrorX(point.X);
        /// <summary>Return a new <see cref="Area"/> that is mirrored on the given value on the X axis</summary>
        public Area MirrorX(double x) {
            double mirrored = Center.MirrorX(x).X;
            return SetCenterX(mirrored);
        }

        /// <summary>Return a new <see cref="Area"/> that is mirrored on the given point on the Y axis</summary>
        public Area MirrorY(Coord point) => MirrorY(point.Y);
        /// <summary>Return a new <see cref="Area"/> that is mirrored on the given value on the Y axis</summary>
        public Area MirrorY(double y) {
            double mirrored = Center.MirrorY(y).Y;
            return SetCenterY(mirrored);
        }

        /// <summary>Return a new <see cref="Area"/> that is mirrored on the given point on the X and Y axes</summary>
        public Area Mirror(Coord point) => Mirror(point.X, point.Y);
        /// <summary>Return a new <see cref="Area"/> that is mirrored on the given values on the X and Y axes</summary>
        public Area Mirror(double x, double y) {
            return MirrorX(x).MirrorY(y);
        }
        #endregion

        #region operators
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static implicit operator Area(WinAPI.RECT r) => new Area(r.X, r.Y, r.Width, r.Height);
        public static implicit operator WinAPI.RECT(Area a) {
            a = a.Round();
            return new WinAPI.RECT((int)a.Left, (int)a.Top, (int)a.Right, (int)a.Bottom);
        }

        public static implicit operator Area(Rectangle r) => new Area(r.X, r.Y, r.Width, r.Height);
        public static implicit operator Rectangle(Area a) {
            a = a.Round();
            return new Rectangle((int)a.X, (int)a.Y, (int)a.W, (int)a.H);
        }

        public static implicit operator Point(Area a) => new Point((int)a.X, (int)a.Y);
        public static implicit operator Size(Area a) => new Size((int)a.W, (int)a.H);

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
        public override bool Equals(object obj) => obj is Area && this == (Area)obj;
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
        public int IntX => (int)Math.Round(X);
        /// <summary>Y value of the coordinate as an int</summary>
        public int IntY => (int)Math.Round(Y);

        /// <summary>All components are 0</summary>
        public static Coord Zero => new Coord(0, 0);
        /// <summary>All components are <see cref="double.MaxValue"/></summary>
        public static Coord Max => new Coord(double.MaxValue, double.MaxValue);
        /// <summary>True if all components are 0</summary>
        public bool IsZero => X == 0 && Y == 0;
        /// <summary>True if all components are <see cref="double.MaxValue"/></summary>
        public bool IsMax => X == double.MaxValue && Y == double.MaxValue;
        /// <summary>True if X or Y is NaN</summary>
        public bool HasNan => double.IsNaN(X) || double.IsNaN(Y);

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
        public Coord(double x, double y) {
            if (double.IsNaN(x) || double.IsNaN(y))
                throw new ArgumentException("Cannot initialize Coord with NaN values");
            X = x;
            Y = y;
        }

        public Coord(Coord other) : this(other.X, other.Y) { }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>Get coordinate from an int where the first 16 bits are the x value and the last 16 are the y value</summary>
        public static Coord From(int value) {
            var x = value & 0x0000FFFF;
            var y = value >> 16;
            return new Coord(x, y);
        }

        /// <summary>Create a direction Coord from an EdgeType</summary>
        /// <remarks>BottomRight would be (1, 1) while TopLeft would be (-1, -1)</remarks>
        public static Coord From(EdgeType dir) {
            return new Coord(dir.HasLeft() ? -1 : dir.HasRight() ? 1 : 0, dir.HasTop() ? -1 : dir.HasBottom() ? 1 : 0);
        }
        #endregion

        #region methods
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

        /// <summary>Set X value</summary>
        public Coord SetX(double value) {
            Coord c = this;
            c.X = value;
            return c;
        }
        /// <summary>Set Y value</summary>
        public Coord SetY(double value) {
            Coord c = this;
            c.Y = value;
            return c;
        }
        /// <summary>Add to X value</summary>
        public Coord AddX(double value) {
            Coord c = this;
            c.X += value;
            return c;
        }
        /// <summary>Add to Y value</summary>
        public Coord AddY(double value) {
            Coord c = this;
            c.Y += value;
            return c;
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

        /// <summary>Calculate angle between this and another vector</summary>
        /// <returns>Angle as degrees</returns>
        public double Angle(Coord vector) {
            return Matht.Degrees(Math.Acos(Dot(vector) / (Magnitude * vector.Magnitude)));
        }

        /// <summary>Calculate dot product between this and another vector</summary>
        public double Dot(Coord vector) {
            return X * vector.X + Y * vector.Y;
        }

        /// <summary>Clamp this point inside the specified area</summary>
        public Coord Clamp(Area clamp) {
            var x = Math.Min(Math.Max(clamp.X, X), clamp.X + clamp.W);
            var y = Math.Min(Math.Max(clamp.Y, Y), clamp.Y + clamp.H);
            return new Coord(x, y);
        }

        /// <summary>Clamp this point to a line between the given points</summary>
        public Coord ClampLinear(Coord point1, Coord point2) {
            Coord proj = ProjectToLine(point1, point2);
            Coord remap = SpatialRemap(point1, point2, new Coord(0, 0), new Coord(0, 1));
            if (remap.X < point1.X)
                return point1;
            if (remap.X > point2.X)
                return point2;
            return proj;
        }

        /// <summary>Map this point from one area to another while maintaining its relative position</summary>
        public Coord Map(Area from, Area to) {
            if (!from.IsValid || !to.IsValid)
                throw new ArgumentException($"argument {(!from.IsValid ? nameof(from) : nameof(to))} was not valid (has negative size)");
            if ((from.W == 0 || from.H == 0) && !from.AddSize(1, 1).Contains(this))
                throw new ArgumentException($"Area {nameof(from)}'s size is 0 and does not contain the map point");
            Coord copy = this;
            copy.X = from.W != 0 && to.W != 0 ? ToRange(X, from.Left, from.Right, to.Left, to.Right) : to.Center.X;
            copy.Y = from.H != 0 && to.H != 0 ? ToRange(Y, from.Top, from.Bottom, to.Top, to.Bottom) : to.Center.Y;
            return copy;

            double ToRange(double value, double low, double high, double newLow, double newHigh) => newLow + (newHigh - newLow) * ((value - low) / (high - low));
        }

        /// <summary>Lerp to the target coordinate</summary>
        public Coord Lerp(Coord target, double t) => Lerp(this, target, t);

        /// <summary>Lerp between two coordinates</summary>
        public static Coord Lerp(Coord a, Coord b, double t) => new Coord(a.X * (1 - t) + b.X * t, a.Y * (1 - t) + b.Y * t);

        /// <summary>Remap the point in 2D space by specifying two lines/vectors as guides</summary>
        public Coord SpatialRemap(Coord A, Coord B, Coord C, Coord D, bool mirror = false) {
            Coord offsetAB = A;
            Coord offsetCD = C;
            Coord AB = B - offsetAB;
            Coord CD = D - offsetCD;
            double scale = CD.Magnitude / AB.Magnitude;

            Coord result = (this - offsetAB).Rotate(AB.Angle(CD)) * scale + offsetCD;
            if (mirror) {
                Coord proj = result.ProjectToLine(C, D);
                result = proj + (proj - result);
            }
            return result;
        }

        /// <summary>Project a point to the closest possible position on a line</summary>
        public Coord ProjectToLine(Coord point1, Coord point2) => ProjectToLine(point1, point2, ProjectionMode.closest);
        /// <summary>Project a point to a line</summary>
        public Coord ProjectToLine(Coord point1, Coord point2, ProjectionMode mode) {
            if (mode == ProjectionMode.vertical) {
                double newX = Matht.Clamp(X, point1.X, point2.X);
                double newY = Matht.ToRange(X, point1.X, point2.X, point1.Y, point2.Y);
                return new Coord(newX, newY);

            } else if (mode == ProjectionMode.horizontal) {
                double newY = Matht.Clamp(Y, point1.Y, point2.Y);
                double newX = Matht.ToRange(Y, point1.Y, point2.Y, point1.X, point2.X);
                return new Coord(newX, newY);

            } else {
                Coord orth = point2 - point1;
                orth = new Coord(orth.Y, -orth.X);
                orth = this + orth;
                return Matht.LineIntersection(point1, point2, this, orth);
            }
        }

        /// <summary>Return a new coord that is mirrored on the given point on the X axis</summary>
        public Coord MirrorX(Coord point) => MirrorX(point.X);
        /// <summary>Return a new coord that is mirrored on the given value on the X axis</summary>
        public Coord MirrorX(double x) {
            double mirrored = x + (x - X);
            return new Coord(mirrored, Y);
        }

        /// <summary>Return a new coord that is mirrored on the given point on the Y axis</summary>
        public Coord MirrorY(Coord point) => MirrorY(point.Y);
        /// <summary>Return a new coord that is mirrored on the given value on the Y axis</summary>
        public Coord MirrorY(double y) {
            double mirrored = y + (y - Y);
            return new Coord(X, mirrored);
        }

        /// <summary>Return a new coord that is mirrored on the given point on the X and Y axes</summary>
        public Coord Mirror(Coord point) => Mirror(point.X, point.Y);
        /// <summary>Return a new coord that is mirrored on the given values on the X and Y axes</summary>
        public Coord Mirror(double x, double y) {
            return MirrorX(x).MirrorY(y);
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
        public static Coord operator +(Coord a, Coord b) => new Coord(ValidateNaN(a.X + b.X), ValidateNaN(a.Y + b.Y));
        public static Coord operator -(Coord a, Coord b) => new Coord(ValidateNaN(a.X - b.X), ValidateNaN(a.Y - b.Y));
        public static Coord operator *(Coord a, Coord b) => new Coord(ValidateNaN(a.X * b.X), ValidateNaN(a.Y * b.Y));
        public static Coord operator /(Coord a, Coord b) => new Coord(ValidateNaN(a.X / b.X), ValidateNaN(a.Y / b.Y));
        public static Coord operator *(Coord a, double b) => new Coord(ValidateNaN(a.X * b), ValidateNaN(a.Y * b));
        public static Coord operator *(double a, Coord b) => new Coord(ValidateNaN(b.X * a), ValidateNaN(b.Y * a));
        public static Coord operator /(Coord a, double b) => new Coord(ValidateNaN(a.X / b), ValidateNaN(a.Y / b));
        public static Coord operator -(Coord a) => new Coord(ValidateNaN(-a.X), ValidateNaN(-a.Y));
        public override bool Equals(object obj) => obj is Coord && this == (Coord)obj;
        public override string ToString() => "(" + X + ", " + Y + ")";
        public override int GetHashCode() {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        private static double ValidateNaN(double value) {
            if (double.IsNaN(value))
                throw new InvalidOperationException("Coord object contained NaN values during operation");
            return value;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        #endregion
    }

    /// <summary>Projection mode. Used by <see cref="Coord.ProjectToLine(Coord, Coord, ProjectionMode)"/></summary>
    public enum ProjectionMode {
        /// <summary>Project to the closest point</summary>
        closest,
        /// <summary>Project to the closest point above or below</summary>
        vertical,
        /// <summary>Project to the closest point left or right</summary>
        horizontal
    }
}
