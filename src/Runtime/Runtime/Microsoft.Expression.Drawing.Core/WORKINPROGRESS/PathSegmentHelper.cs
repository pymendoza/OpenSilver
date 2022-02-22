using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// Helper class to convert an ArcSegment to BezierSegment(s).
	/// </summary>
	/// <summary>
	/// Helper class to work with PathSegment and all variations.
	/// </summary>
	/// <summary>
	/// Strategy classes to handle different types of PathSegment.
	/// </summary>
	internal static class PathSegmentHelper
	{
		private static class ArcToBezierHelper
		{
			/// <summary>
			/// ArcToBezier, computes the Bezier approximation of an arc.
			/// </summary>
			/// <remarks>
			/// This utility computes the Bezier approximation for an elliptical arc as
			/// it is defined in the SVG arc spec. The ellipse from which the arc is
			/// carved is axis-aligned in its own coordinates, and defined there by its
			/// x and y radii. The rotation angle defines how the ellipse's axes are
			/// rotated relative to the x axis. The start and end points define one of 4
			/// possible arcs; the sweep and large-arc flags determine which one of
			/// these arcs will be chosen.
			///
			/// Returning cPieces = 0 indicates a line instead of an arc
			///           cPieces = -1 indicates that the arc degenerates to a point 
			/// </remarks>
			public static void ArcToBezier(double xStart, double yStart, double xRadius, double yRadius, double rRotation, bool fLargeArc, bool fSweepUp, double xEnd, double yEnd, out Point[] pPt, out int cPieces)
			{
				double num = 1E-06;
				pPt = new Point[12];
				double num2 = num * num;
				bool flag = false;
				cPieces = -1;
				double num3 = 0.5 * (xEnd - xStart);
				double num4 = 0.5 * (yEnd - yStart);
				double num5 = num3 * num3 + num4 * num4;
				if (num5 < num2)
				{
					return;
				}
				if (!AcceptRadius(num5, num2, ref xRadius) || !AcceptRadius(num5, num2, ref yRadius))
				{
					cPieces = 0;
					return;
				}
				double num6;
				double num7;
				if (Math.Abs(rRotation) < num)
				{
					num6 = 1.0;
					num7 = 0.0;
				}
				else
				{
					rRotation = (0.0 - rRotation) * Math.PI / 180.0;
					num6 = Math.Cos(rRotation);
					num7 = Math.Sin(rRotation);
					double num8 = num3 * num6 - num4 * num7;
					num4 = num3 * num7 + num4 * num6;
					num3 = num8;
				}
				num3 /= xRadius;
				num4 /= yRadius;
				num5 = num3 * num3 + num4 * num4;
				double num11;
				double num10;
				if (num5 > 1.0)
				{
					double num9 = Math.Sqrt(num5);
					xRadius *= num9;
					yRadius *= num9;
					num11 = (num10 = 0.0);
					flag = true;
					num3 /= num9;
					num4 /= num9;
				}
				else
				{
					double num12 = Math.Sqrt((1.0 - num5) / num5);
					if (fLargeArc != fSweepUp)
					{
						num11 = (0.0 - num12) * num4;
						num10 = num12 * num3;
					}
					else
					{
						num11 = num12 * num4;
						num10 = (0.0 - num12) * num3;
					}
				}
				Point point = new Point(0.0 - num3 - num11, 0.0 - num4 - num10);
				Point point2 = new Point(num3 - num11, num4 - num10);
				Matrix matrix = new Matrix(num6 * xRadius, (0.0 - num7) * xRadius, num7 * yRadius, num6 * yRadius, 0.5 * (xEnd + xStart), 0.5 * (yEnd + yStart));
				if (!flag)
				{
					matrix.OffsetX += matrix.M11 * num11 + matrix.M21 * num10;
					matrix.OffsetY += matrix.M12 * num11 + matrix.M22 * num10;
				}
				GetArcAngle(point, point2, fLargeArc, fSweepUp, out var rCosArcAngle, out var rSinArcAngle, out cPieces);
				double num13 = GetBezierDistance(rCosArcAngle);
				if (!fSweepUp)
				{
					num13 = 0.0 - num13;
				}
				Point rhs = new Point((0.0 - num13) * point.Y, num13 * point.X);
				int num14 = 0;
				pPt = new Point[cPieces * 3];
				Point point4;
				for (int i = 1; i < cPieces; i++)
				{
					Point point3 = new Point(point.X * rCosArcAngle - point.Y * rSinArcAngle, point.X * rSinArcAngle + point.Y * rCosArcAngle);
					point4 = new Point((0.0 - num13) * point3.Y, num13 * point3.X);
					ref Point reference = ref pPt[num14++];
					reference = matrix.Transform(point.Plus(rhs));
					ref Point reference2 = ref pPt[num14++];
					reference2 = matrix.Transform(point3.Minus(point4));
					ref Point reference3 = ref pPt[num14++];
					reference3 = matrix.Transform(point3);
					point = point3;
					rhs = point4;
				}
				point4 = new Point((0.0 - num13) * point2.Y, num13 * point2.X);
				ref Point reference4 = ref pPt[num14++];
				reference4 = matrix.Transform(point.Plus(rhs));
				ref Point reference5 = ref pPt[num14++];
				reference5 = matrix.Transform(point2.Minus(point4));
				ref Point reference6 = ref pPt[num14];
				reference6 = new Point(xEnd, yEnd);
			}

			/// <summary>
			/// Gets the number of Bezier arcs, and sine/cosine of each.
			/// </summary>
			/// <remarks>
			/// This is a private utility used by ArcToBezier. Breaks the arc into
			/// pieces so that no piece will span more than 90 degrees. The input
			/// points are on the unit circle.
			/// </remarks>
			private static void GetArcAngle(Point ptStart, Point ptEnd, bool fLargeArc, bool fSweepUp, out double rCosArcAngle, out double rSinArcAngle, out int cPieces)
			{
				rCosArcAngle = GeometryHelper.Dot(ptStart, ptEnd);
				rSinArcAngle = GeometryHelper.Determinant(ptStart, ptEnd);
				if (rCosArcAngle >= 0.0)
				{
					if (!fLargeArc)
					{
						cPieces = 1;
						return;
					}
					cPieces = 4;
				}
				else if (fLargeArc)
				{
					cPieces = 3;
				}
				else
				{
					cPieces = 2;
				}
				double num = Math.Atan2(rSinArcAngle, rCosArcAngle);
				if (fSweepUp)
				{
					if (num < 0.0)
					{
						num += Math.PI * 2.0;
					}
				}
				else if (num > 0.0)
				{
					num -= Math.PI * 2.0;
				}
				num /= (double)cPieces;
				rCosArcAngle = Math.Cos(num);
				rSinArcAngle = Math.Sin(num);
			}

			/// <summary>
			/// GetBezierDistance returns the distance as a fraction of the radius.
			/// </summary>
			///             <remarks>
			///      Get the distance from a circular arc's end points to the control points
			///      of the Bezier arc that approximates it, as a fraction of the arc's
			///      radius.
			///
			///      Since the result is relative to the arc's radius, it depends strictly on
			///      the arc's angle. The arc is assumed to be of 90 degrees or less, so the
			///      angle is determined by the cosine of that angle, which is derived from
			///      rDot = the dot product of two radius vectors. We need the Bezier curve
			///      that agrees with the arc's points and tangents at the ends and midpoint. 
			///      Here we compute the distance from the curve's endpoints to its control
			///      points.
			///
			///      Since we are looking for the relative distance, we can work on the unit
			///      circle. Place the center of the circle at the origin, and put the X axis
			///      as the bisector between the 2 vectors.  Let a be the angle between the
			///      vectors.  Then the X coordinates of the 1st and last points are cos(a/2). 
			///      Let x be the X coordinate of the 2nd and 3rd points.  At t=1/2 we have a
			///      point at (1,0). But the terms of the polynomial there are all equal:
			///
			///                (1-t)^3 = t*(1-t)^2 = t^2*(1-t) = t^3 = 1/8,
			///
			///      so from the Bezier formula there we have:
			///
			///                1 = (1/8) * (cos(a/2) + 3x + 3x + cos(a/2)), 
			///
			///      hence
			///
			///                x = (4 - cos(a/2)) / 3
			///
			///      The X difference between that and the 1st point is:
			///
			///                DX = x - cos(a/2) = 4(1 - cos(a/2)) / 3.
			///
			///      But DX = distance / sin(a/2), hence the distance is
			///
			///                dist = (4/3)*(1 - cos(a/2)) / sin(a/2).
			///
			///      Rather than the angle a, we are given rDot = R^2 * cos(a), so we
			///      multiply top and bottom by R:
			///
			///                dist = (4/3)*(R - Rcos(a/2)) / Rsin(a/2)
			///
			///      and use some trig:
			///                               ________________
			///                cos(a/2)   = \/(1 + cos(a)) / 2
			///                               ______________________
			///                R*cos(a/2) = \/(R^2 + R^2 cos(a)) / 2 
			///                               ________________
			///                           = \/(R^2 + rDot) / 2
			///
			///      Let A = (R^2 + rDot)/2.
			///                               ____________________
			///                R*sin(a/2) = \/R^2 - R^2 cos^2(a/2)
			///                               _______
			///                           = \/R^2 - A
			///
			///      so:
			///                                          _
			///                             4      R - \/A
			///                      dist = - * ------------
			///                             3      _______
			///                                  \/R^2 - A
			///
			///  History:
			///      5/29/2001 MichKa
			///          Created it.
			///             </remarks>
			private static double GetBezierDistance(double rDot, double rRadius = 1.0)
			{
				double num = rRadius * rRadius;
				double result = 0.0;
				double num2 = 0.5 * (num + rDot);
				if (!(num2 < 0.0))
				{
					double num3 = num - num2;
					if (!(num3 <= 0.0))
					{
						double num4 = Math.Sqrt(num3);
						double num5 = 4.0 * (rRadius - Math.Sqrt(num2)) / 3.0;
						result = ((!(num5 <= num4 * 1E-06)) ? (num5 / num4) : 0.0);
					}
				}
				return result;
			}

			/// <summary>
			/// Returns false if the radius is too small compared to the chord length (returns true on NaNs)
			/// radius is modified to the value that is accepted.
			/// </summary>
			private static bool AcceptRadius(double rHalfChord2, double rFuzz2, ref double rRadius)
			{
				bool flag = !(rRadius * rRadius <= rHalfChord2 * rFuzz2);
				if (flag && rRadius < 0.0)
				{
					rRadius = 0.0 - rRadius;
				}
				return flag;
			}
		}

		private abstract class PathSegmentImplementation
		{
			public Point Start { get; private set; }

			public abstract void Flatten(IList<Point> points, double tolerance);

			public abstract Point GetPoint(int index);

			public abstract IEnumerable<SimpleSegment> GetSimpleSegments();

			public static PathSegmentImplementation Create(PathSegment segment, Point start)
			{
				PathSegmentImplementation pathSegmentImplementation = Create(segment);
				pathSegmentImplementation.Start = start;
				return pathSegmentImplementation;
			}

			public static PathSegmentImplementation Create(PathSegment segment)
			{
				PathSegmentImplementation result;
				if ((result = BezierSegmentImplementation.Create(segment as BezierSegment)) == null && (result = LineSegmentImplementation.Create(segment as LineSegment)) == null && (result = ArcSegmentImplementation.Create(segment as ArcSegment)) == null && (result = PolyLineSegmentImplementation.Create(segment as PolyLineSegment)) == null && (result = PolyBezierSegmentImplementation.Create(segment as PolyBezierSegment)) == null && (result = QuadraticBezierSegmentImplementation.Create(segment as QuadraticBezierSegment)) == null && (result = PolyQuadraticBezierSegmentImplementation.Create(segment as PolyQuadraticBezierSegment)) == null)
				{
					throw new NotImplementedException();
				}
				return result;
			}
		}

		private class BezierSegmentImplementation : PathSegmentImplementation
		{
			private BezierSegment segment;

			public static PathSegmentImplementation Create(BezierSegment source)
			{
				if (source != null)
				{
					BezierSegmentImplementation bezierSegmentImplementation = new BezierSegmentImplementation();
					bezierSegmentImplementation.segment = source;
					return bezierSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				Point[] controlPoints = new Point[4] { base.Start, segment.Point1, segment.Point2, segment.Point3 };
				List<Point> list = new List<Point>();
				BezierCurveFlattener.FlattenCubic(controlPoints, tolerance, list, skipFirstPoint: true);
				points.AddRange(list);
			}

			public override Point GetPoint(int index)
			{
				switch (index)
				{
					default:
						throw new ArgumentOutOfRangeException("index");
					case 0:
						return segment.Point1;
					case 1:
						return segment.Point2;
					case -1:
					case 2:
						return segment.Point3;
				}
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				yield return SimpleSegment.Create(base.Start, segment.Point1, segment.Point2, segment.Point3);
			}
		}

		private class QuadraticBezierSegmentImplementation : PathSegmentImplementation
		{
			private QuadraticBezierSegment segment;

			public static PathSegmentImplementation Create(QuadraticBezierSegment source)
			{
				if (source != null)
				{
					QuadraticBezierSegmentImplementation quadraticBezierSegmentImplementation = new QuadraticBezierSegmentImplementation();
					quadraticBezierSegmentImplementation.segment = source;
					return quadraticBezierSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				Point[] controlPoints = new Point[3] { base.Start, segment.Point1, segment.Point2 };
				List<Point> list = new List<Point>();
				BezierCurveFlattener.FlattenQuadratic(controlPoints, tolerance, list, skipFirstPoint: true);
				points.AddRange(list);
			}

			public override Point GetPoint(int index)
			{
				switch (index)
				{
					default:
						throw new ArgumentOutOfRangeException("index");
					case 0:
						return segment.Point1;
					case -1:
					case 1:
						return segment.Point2;
				}
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				yield return SimpleSegment.Create(base.Start, segment.Point1, segment.Point2);
			}
		}

		private class PolyBezierSegmentImplementation : PathSegmentImplementation
		{
			private PolyBezierSegment segment;

			public static PathSegmentImplementation Create(PolyBezierSegment source)
			{
				if (source != null)
				{
					PolyBezierSegmentImplementation polyBezierSegmentImplementation = new PolyBezierSegmentImplementation();
					polyBezierSegmentImplementation.segment = source;
					return polyBezierSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				Point point = base.Start;
				int num = segment.Points.Count / 3 * 3;
				for (int i = 0; i < num; i += 3)
				{
					Point[] controlPoints = new Point[4]
					{
					point,
					segment.Points[i],
					segment.Points[i + 1],
					segment.Points[i + 2]
					};
					List<Point> list = new List<Point>();
					BezierCurveFlattener.FlattenCubic(controlPoints, tolerance, list, skipFirstPoint: true);
					points.AddRange(list);
					point = segment.Points[i + 2];
				}
			}

			public override Point GetPoint(int index)
			{
				int num = segment.Points.Count / 3 * 3;
				if (index < -1 || index > num - 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (index != -1)
				{
					return segment.Points[index];
				}
				return segment.Points[num - 1];
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				Point point0 = base.Start;
				IList<Point> points = segment.Points;
				int count = segment.Points.Count / 3;
				for (int i = 0; i < count; i++)
				{
					int k3 = i * 3;
					yield return SimpleSegment.Create(point0, points[k3], points[k3 + 1], points[k3 + 2]);
					point0 = points[k3 + 2];
				}
			}
		}

		private class PolyQuadraticBezierSegmentImplementation : PathSegmentImplementation
		{
			private PolyQuadraticBezierSegment segment;

			public static PathSegmentImplementation Create(PolyQuadraticBezierSegment source)
			{
				if (source != null)
				{
					PolyQuadraticBezierSegmentImplementation polyQuadraticBezierSegmentImplementation = new PolyQuadraticBezierSegmentImplementation();
					polyQuadraticBezierSegmentImplementation.segment = source;
					return polyQuadraticBezierSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				Point point = base.Start;
				int num = segment.Points.Count / 2 * 2;
				for (int i = 0; i < num; i += 2)
				{
					Point[] controlPoints = new Point[3]
					{
					point,
					segment.Points[i],
					segment.Points[i + 1]
					};
					List<Point> list = new List<Point>();
					BezierCurveFlattener.FlattenQuadratic(controlPoints, tolerance, list, skipFirstPoint: true);
					points.AddRange(list);
					point = segment.Points[i + 1];
				}
			}

			public override Point GetPoint(int index)
			{
				int num = segment.Points.Count / 2 * 2;
				if (index < -1 || index > num - 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (index != -1)
				{
					return segment.Points[index];
				}
				return segment.Points[num - 1];
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				Point point0 = base.Start;
				IList<Point> points = segment.Points;
				int count = segment.Points.Count / 2;
				for (int i = 0; i < count; i++)
				{
					int k2 = i * 2;
					yield return SimpleSegment.Create(point0, points[k2], points[k2 + 1]);
					point0 = points[k2 + 1];
				}
			}
		}

		private class ArcSegmentImplementation : PathSegmentImplementation
		{
			private ArcSegment segment;

			public static PathSegmentImplementation Create(ArcSegment source)
			{
				if (source != null)
				{
					ArcSegmentImplementation arcSegmentImplementation = new ArcSegmentImplementation();
					arcSegmentImplementation.segment = source;
					return arcSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				ArcToBezierSegments(segment, base.Start)?.FlattenSegment(points, base.Start, tolerance);
			}

			public override Point GetPoint(int index)
			{
				if (index < -1 || index > 0)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return segment.Point;
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				PathSegment pathSegment = ArcToBezierSegments(segment, base.Start);
				if (pathSegment != null)
				{
					return pathSegment.GetSimpleSegments(base.Start);
				}
				return Enumerable.Empty<SimpleSegment>();
			}
		}

		private class LineSegmentImplementation : PathSegmentImplementation
		{
			private LineSegment segment;

			public static PathSegmentImplementation Create(LineSegment source)
			{
				if (source != null)
				{
					LineSegmentImplementation lineSegmentImplementation = new LineSegmentImplementation();
					lineSegmentImplementation.segment = source;
					return lineSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				points.Add(segment.Point);
			}

			public override Point GetPoint(int index)
			{
				if (index < -1 || index > 0)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				return segment.Point;
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				yield return SimpleSegment.Create(base.Start, segment.Point);
			}
		}

		private class PolyLineSegmentImplementation : PathSegmentImplementation
		{
			private PolyLineSegment segment;

			public static PathSegmentImplementation Create(PolyLineSegment source)
			{
				if (source != null)
				{
					PolyLineSegmentImplementation polyLineSegmentImplementation = new PolyLineSegmentImplementation();
					polyLineSegmentImplementation.segment = source;
					return polyLineSegmentImplementation;
				}
				return null;
			}

			public override void Flatten(IList<Point> points, double tolerance)
			{
				points.AddRange(segment.Points);
			}

			public override Point GetPoint(int index)
			{
				if (index < -1 || index > segment.Points.Count - 1)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (index != -1)
				{
					return segment.Points[index];
				}
				return segment.Points.Last();
			}

			public override IEnumerable<SimpleSegment> GetSimpleSegments()
			{
				Point point2 = base.Start;
				foreach (Point point in segment.Points)
				{
					yield return SimpleSegment.Create(point2, point);
					point2 = point;
				}
			}
		}

        /// <summary>
        /// Converts an arc segment into Bezier format.
        /// Returns BezierSegment, PolyBezierSegment, LineSegment, or null.
        /// When returning null, the arc degenerates into the start point.
        /// </summary>
        public static PathSegment ArcToBezierSegments(ArcSegment arcSegment, Point startPoint)
        {
            bool isStroked = arcSegment.IsStroked();
            ArcToBezierHelper.ArcToBezier(startPoint.X, startPoint.Y, arcSegment.Size.Width, arcSegment.Size.Height, arcSegment.RotationAngle, arcSegment.IsLargeArc, arcSegment.SweepDirection == SweepDirection.Clockwise, arcSegment.Point.X, arcSegment.Point.Y, out var pPt, out var cPieces);
            switch (cPieces)
            {
				case -1:
					return null;
				case 0:
					return CreateLineSegment(arcSegment.Point, isStroked);
				case 1:
					return CreateBezierSegment(pPt[0], pPt[1], pPt[2], isStroked);
				default:
					return CreatePolyBezierSegment(pPt, 0, cPieces * 3, isStroked);
            }
        }

        /// <summary>
        /// Avoid calling the three-parameter constructor, since it always sets a local value for IsStroked.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="isStroked"></param>
        private static void SetIsStroked(this PathSegment segment, bool isStroked)
		{
		}

		public static LineSegment CreateLineSegment(Point point, bool isStroked = true)
		{
			LineSegment lineSegment = new LineSegment();
			lineSegment.Point = point;
			lineSegment.SetIsStroked(isStroked);
			return lineSegment;
		}

		public static QuadraticBezierSegment CreateQuadraticBezierSegment(Point point1, Point point2, bool isStroked = true)
		{
			QuadraticBezierSegment quadraticBezierSegment = new QuadraticBezierSegment();
			quadraticBezierSegment.Point1 = point1;
			quadraticBezierSegment.Point2 = point2;
			quadraticBezierSegment.SetIsStroked(isStroked);
			return quadraticBezierSegment;
		}

		public static BezierSegment CreateBezierSegment(Point point1, Point point2, Point point3, bool isStroked = true)
		{
			BezierSegment bezierSegment = new BezierSegment();
			bezierSegment.Point1 = point1;
			bezierSegment.Point2 = point2;
			bezierSegment.Point3 = point3;
			bezierSegment.SetIsStroked(isStroked);
			return bezierSegment;
		}

		public static PolyBezierSegment CreatePolyBezierSegment(IList<Point> points, int start, int count, bool isStroked = true)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			count = count / 3 * 3;
			if (count < 0 || points.Count < start + count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
			polyBezierSegment.Points = new PointCollection();
			for (int i = 0; i < count; i++)
			{
				polyBezierSegment.Points.Add(points[start + i]);
			}
			polyBezierSegment.SetIsStroked(isStroked);
			return polyBezierSegment;
		}

		public static PolyQuadraticBezierSegment CreatePolyQuadraticBezierSegment(IList<Point> points, int start, int count, bool isStroked = true)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			count = count / 2 * 2;
			if (count < 0 || points.Count < start + count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			PolyQuadraticBezierSegment polyQuadraticBezierSegment = new PolyQuadraticBezierSegment();
			polyQuadraticBezierSegment.Points = new PointCollection();
			for (int i = 0; i < count; i++)
			{
				polyQuadraticBezierSegment.Points.Add(points[start + i]);
			}
			polyQuadraticBezierSegment.SetIsStroked(isStroked);
			return polyQuadraticBezierSegment;
		}

		public static PolyLineSegment CreatePolylineSegment(IList<Point> points, int start, int count, bool isStroked = true)
		{
			if (count < 0 || points.Count < start + count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			PolyLineSegment polyLineSegment = new PolyLineSegment();
			polyLineSegment.Points = new PointCollection();
			for (int i = 0; i < count; i++)
			{
				polyLineSegment.Points.Add(points[start + i]);
			}
			polyLineSegment.SetIsStroked(isStroked);
			return polyLineSegment;
		}

		public static ArcSegment CreateArcSegment(Point point, Size size, bool isLargeArc, bool clockwise, double rotationAngle = 0.0, bool isStroked = true)
		{
			ArcSegment arcSegment = new ArcSegment();
			arcSegment.SetIfDifferent(ArcSegment.PointProperty, point);
			arcSegment.SetIfDifferent(ArcSegment.SizeProperty, size);
			arcSegment.SetIfDifferent(ArcSegment.IsLargeArcProperty, isLargeArc);
			arcSegment.SetIfDifferent(ArcSegment.SweepDirectionProperty, clockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
			arcSegment.SetIfDifferent(ArcSegment.RotationAngleProperty, rotationAngle);
			arcSegment.SetIsStroked(isStroked);
			return arcSegment;
		}

		/// <summary>
		/// Updates the SegmentCollection with a given polyline matching a given point list.
		/// Tries to keep changes minimum and returns false if nothing has been changed.
		/// </summary>
		public static bool SyncPolylineSegment(PathSegmentCollection collection, int index, IList<Point> points, int start, int count)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			if (index < 0 || index >= collection.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			if (start < 0)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (points.Count < start + count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			bool flag = false;
			PolyLineSegment polyLineSegment;
			if ((polyLineSegment = collection[index] as PolyLineSegment) == null)
			{
				polyLineSegment = (PolyLineSegment)(collection[index] = new PolyLineSegment());
				flag = true;
			}
			flag |= polyLineSegment.Points.EnsureListCount(count);
			for (int i = 0; i < count; i++)
			{
				if (polyLineSegment.Points[i] != points[i + start])
				{
					polyLineSegment.Points[i] = points[i + start];
					flag = true;
				}
			}
			return flag;
		}

		/// <summary>
		/// Updates the collection[index] segment with a poly-Bezier segment matching a given point list.
		/// A given point list must contain 3*N points for N Bezier segments.
		/// </summary>
		public static bool SyncPolyBezierSegment(PathSegmentCollection collection, int index, IList<Point> points, int start, int count)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			if (index < 0 || index >= collection.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			if (start < 0)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			if (points.Count < start + count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			bool result = false;
			count = count / 3 * 3;
			PolyBezierSegment polyBezierSegment;
			if ((polyBezierSegment = collection[index] as PolyBezierSegment) == null)
			{
				polyBezierSegment = (PolyBezierSegment)(collection[index] = new PolyBezierSegment());
				result = true;
			}
			polyBezierSegment.Points.EnsureListCount(count);
			for (int i = 0; i < count; i++)
			{
				if (polyBezierSegment.Points[i] != points[i + start])
				{
					polyBezierSegment.Points[i] = points[i + start];
					result = true;
				}
			}
			return result;
		}

		/// <summary>
		/// Tests if a given path segment is empty.
		/// </summary>
		public static bool IsEmpty(this PathSegment segment)
		{
			return segment.GetPointCount() == 0;
		}

		/// <summary>
		/// Gets the point count in a given path segment.
		/// </summary>
		public static int GetPointCount(this PathSegment segment)
		{
			if (segment is ArcSegment)
			{
				return 1;
			}
			if (segment is LineSegment)
			{
				return 1;
			}
			if (segment is QuadraticBezierSegment)
			{
				return 2;
			}
			if (segment is BezierSegment)
			{
				return 3;
			}
			if (segment is PolyLineSegment polyLineSegment)
			{
				return polyLineSegment.Points.Count;
			}
			if (segment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
			{
				return polyQuadraticBezierSegment.Points.Count / 2 * 2;
			}
			if (segment is PolyBezierSegment polyBezierSegment)
			{
				return polyBezierSegment.Points.Count / 3 * 3;
			}
			return 0;
		}

		/// <summary>
		/// Gets the last point of a given path segment.
		/// </summary>
		public static Point GetLastPoint(this PathSegment segment)
		{
			return segment.GetPoint(-1);
		}

		/// <summary>
		/// Gets the point of a given index in a given segment.
		/// If input is (-1), returns the last point.
		/// </summary>
		public static Point GetPoint(this PathSegment segment, int index)
		{
			return PathSegmentImplementation.Create(segment).GetPoint(index);
		}

		/// <summary>
		/// Flattens a given segment and adds resulting points into a given point list.
		/// </summary>
		/// <param name="segment">The segment to be flatten.</param>
		/// <param name="points">The resulting points list.</param>
		/// <param name="start">The start point of the segment.</param>
		/// <param name="tolerance">The error tolerance. Must be positive. Can be zero. Fallback to default tolerance.</param>
		public static void FlattenSegment(this PathSegment segment, IList<Point> points, Point start, double tolerance)
		{
			PathSegmentImplementation.Create(segment, start).Flatten(points, tolerance);
		}

		public static IEnumerable<SimpleSegment> GetSimpleSegments(this PathSegment segment, Point start)
		{
			PathSegmentImplementation implementation = PathSegmentImplementation.Create(segment, start);
			foreach (SimpleSegment simpleSegment in implementation.GetSimpleSegments())
			{
				yield return simpleSegment;
			}
		}
	}
}
#endif