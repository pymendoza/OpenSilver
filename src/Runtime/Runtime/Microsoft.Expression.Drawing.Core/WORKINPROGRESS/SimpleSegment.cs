using System.Collections.Generic;
using System.Windows;

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	internal class SimpleSegment
	{
		public enum SegmentType
		{
			Line,
			CubicBeizer
		}

		public SegmentType Type { get; private set; }

		/// <summary>
		/// Control points of path segment.  Length is variant.
		/// Line segment has 2 points, Cubic bezier has 4 points.
		/// </summary>
		public Point[] Points { get; private set; }

		public void Flatten(IList<Point> resultPolyline, double tolerance, IList<double> resultParameters)
		{
			switch (Type)
			{
				case SegmentType.Line:
					resultPolyline.Add(Points[1]);
					resultParameters?.Add(1.0);
					break;
				case SegmentType.CubicBeizer:
					BezierCurveFlattener.FlattenCubic(Points, tolerance, resultPolyline, skipFirstPoint: true, resultParameters);
					break;
			}
		}

		/// <summary>
		/// Private constructor. Force to use factory methods.
		/// </summary>
		private SimpleSegment()
		{
		}

		/// <summary>
		///  Creates a line segment
		/// </summary>
		public static SimpleSegment Create(Point point0, Point point1)
		{
			SimpleSegment simpleSegment = new SimpleSegment();
			simpleSegment.Type = SegmentType.Line;
			simpleSegment.Points = new Point[2] { point0, point1 };
			return simpleSegment;
		}

		/// <summary>
		///  Creates a cubic bezier segment from quatratic curve (3 control points)
		/// </summary>
		public static SimpleSegment Create(Point point0, Point point1, Point point2)
		{
			Point point3 = GeometryHelper.Lerp(point0, point1, 2.0 / 3.0);
			Point point4 = GeometryHelper.Lerp(point1, point2, 1.0 / 3.0);
			SimpleSegment simpleSegment = new SimpleSegment();
			simpleSegment.Type = SegmentType.CubicBeizer;
			simpleSegment.Points = new Point[4] { point0, point3, point4, point2 };
			return simpleSegment;
		}

		/// <summary>
		///  Creates a cubic bezier segment with 4 control points.
		/// </summary>
		public static SimpleSegment Create(Point point0, Point point1, Point point2, Point point3)
		{
			SimpleSegment simpleSegment = new SimpleSegment();
			simpleSegment.Type = SegmentType.CubicBeizer;
			simpleSegment.Points = new Point[4] { point0, point1, point2, point3 };
			return simpleSegment;
		}
	}
}
#endif