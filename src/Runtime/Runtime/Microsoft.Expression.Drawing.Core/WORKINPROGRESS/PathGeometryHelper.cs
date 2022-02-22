using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// Helper class to work with PathGeometry.
	/// </summary>
	internal static class PathGeometryHelper
	{
		/// <summary>
		/// Parses abbreviated geometry sytax.
		/// </summary>
		private class AbbreviatedGeometryParser
		{
			private PathGeometry geometry;

			private PathFigure figure;

			private Point lastPoint;

			private Point secondLastPoint;

			private string buffer;

			private int index;

			private int length;

			private char token;

			public AbbreviatedGeometryParser(PathGeometry geometry)
			{
				this.geometry = geometry;
			}

			public void Parse(string data, int startIndex)
			{
				buffer = data;
				length = data.Length;
				index = startIndex;
				bool flag = true;
				char c = ' ';
				while (ReadToken())
				{
					char c2 = token;
					if (flag)
					{
						if (c2 != 'M' && c2 != 'm')
						{
							throw new FormatException();
						}
						flag = false;
					}
					switch (c2)
					{
						case 'M':
						case 'm':
							lastPoint = ReadPoint(c2, allowComma: false);
							BeginFigure(lastPoint);
							c = 'M';
							while (IsNumber(allowComma: true))
							{
								lastPoint = ReadPoint(c, allowComma: false);
								LineTo(lastPoint);
								c = 'L';
							}
							break;
						case 'L':
						case 'l':
							EnsureFigure();
							do
							{
								lastPoint = ReadPoint(c2, allowComma: false);
								LineTo(lastPoint);
							}
							while (IsNumber(allowComma: true));
							break;
						case 'H':
						case 'h':
							EnsureFigure();
							do
							{
								double num2 = ReadDouble(allowComma: false);
								if (c2 == 'h')
								{
									num2 += lastPoint.X;
								}
								lastPoint.X = num2;
								LineTo(lastPoint);
							}
							while (IsNumber(allowComma: true));
							break;
						case 'V':
						case 'v':
							EnsureFigure();
							do
							{
								double num = ReadDouble(allowComma: false);
								if (c2 == 'v')
								{
									num += lastPoint.Y;
								}
								lastPoint.Y = num;
								LineTo(lastPoint);
							}
							while (IsNumber(allowComma: true));
							break;
						case 'C':
						case 'c':
							EnsureFigure();
							do
							{
								Point point3 = ReadPoint(c2, allowComma: false);
								secondLastPoint = ReadPoint(c2, allowComma: true);
								lastPoint = ReadPoint(c2, allowComma: true);
								BezierTo(point3, secondLastPoint, lastPoint);
								c = 'C';
							}
							while (IsNumber(allowComma: true));
							break;
						case 'S':
						case 's':
							EnsureFigure();
							do
							{
								Point smoothBeizerFirstPoint = GetSmoothBeizerFirstPoint();
								Point point2 = ReadPoint(c2, allowComma: false);
								lastPoint = ReadPoint(c2, allowComma: true);
								BezierTo(smoothBeizerFirstPoint, point2, lastPoint);
								c = 'S';
							}
							while (IsNumber(allowComma: true));
							break;
						case 'Q':
						case 'q':
							EnsureFigure();
							do
							{
								Point point = ReadPoint(c2, allowComma: false);
								lastPoint = ReadPoint(c2, allowComma: true);
								QuadraticBezierTo(point, lastPoint);
								c = 'Q';
							}
							while (IsNumber(allowComma: true));
							break;
						case 'A':
						case 'a':
							do
							{
								Size size = ReadSize(allowComma: false);
								double rotationAngle = ReadDouble(allowComma: true);
								bool isLargeArc = ReadBool01(allowComma: true);
								SweepDirection sweepDirection = (ReadBool01(allowComma: true) ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
								lastPoint = ReadPoint(c2, allowComma: true);
								ArcTo(size, rotationAngle, isLargeArc, sweepDirection, lastPoint);
								c = 'A';
							}
							while (IsNumber(allowComma: true));
							EnsureFigure();
							break;
						case 'Z':
						case 'z':
							FinishFigure(figureExplicitlyClosed: true);
							break;
						default:
							throw new NotSupportedException();
					}
				}
				FinishFigure(figureExplicitlyClosed: false);
			}

			private bool ReadToken()
			{
				SkipWhitespace(allowComma: false);
				if (index < length)
				{
					token = buffer[index++];
					return true;
				}
				return false;
			}

			private Point ReadPoint(char command, bool allowComma)
			{
				double num = ReadDouble(allowComma);
				double num2 = ReadDouble(allowComma: true);
				if (command >= 'a')
				{
					num += lastPoint.X;
					num2 += lastPoint.Y;
				}
				return new Point(num, num2);
			}

			private Size ReadSize(bool allowComma)
			{
				double width = ReadDouble(allowComma);
				double height = ReadDouble(allowComma: true);
				return new Size(width, height);
			}

			private bool ReadBool01(bool allowComma)
			{
				double num = ReadDouble(allowComma);
				if (num == 0.0)
				{
					return false;
				}
				if (num == 1.0)
				{
					return true;
				}
				throw new FormatException();
			}

			private double ReadDouble(bool allowComma)
			{
				if (!IsNumber(allowComma))
				{
					throw new FormatException();
				}
				bool flag = true;
				int i = index;
				if (index < length && (buffer[index] == '-' || buffer[index] == '+'))
				{
					index++;
				}
				if (index < length && buffer[index] == 'I')
				{
					index = Math.Min(index + 8, length);
					flag = false;
				}
				else if (index < length && buffer[index] == 'N')
				{
					index = Math.Min(index + 3, length);
					flag = false;
				}
				else
				{
					SkipDigits(signAllowed: false);
					if (index < length && buffer[index] == '.')
					{
						flag = false;
						index++;
						SkipDigits(signAllowed: false);
					}
					if (index < length && (buffer[index] == 'E' || buffer[index] == 'e'))
					{
						flag = false;
						index++;
						SkipDigits(signAllowed: true);
					}
				}
				if (flag && index <= i + 8)
				{
					int num = 1;
					if (buffer[i] == '+')
					{
						i++;
					}
					else if (buffer[i] == '-')
					{
						i++;
						num = -1;
					}
					int num2 = 0;
					for (; i < index; i++)
					{
						num2 = num2 * 10 + (buffer[i] - 48);
					}
					return num2 * num;
				}
				string value = buffer.Substring(i, index - i);
				try
				{
					return Convert.ToDouble(value, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					throw new FormatException();
				}
			}

			private void SkipDigits(bool signAllowed)
			{
				if (signAllowed && index < length && (buffer[index] == '-' || buffer[index] == '+'))
				{
					index++;
				}
				while (index < length && buffer[index] >= '0' && buffer[index] <= '9')
				{
					index++;
				}
			}

			private bool IsNumber(bool allowComma)
			{
				bool flag = SkipWhitespace(allowComma);
				if (index < length)
				{
					token = buffer[index];
					if (token == '.' || token == '-' || token == '+' || (token >= '0' && token <= '9') || token == 'I' || token == 'N')
					{
						return true;
					}
				}
				if (flag)
				{
					throw new FormatException();
				}
				return false;
			}

			private bool SkipWhitespace(bool allowComma)
			{
				bool result = false;
				while (index < length)
				{
					char c = buffer[index];
					switch (c)
					{
						case ',':
							if (allowComma)
							{
								result = true;
								allowComma = false;
								break;
							}
							throw new FormatException();
						default:
							if (c > ' ' && c <= 'z')
							{
								return result;
							}
							if (!char.IsWhiteSpace(c))
							{
								return result;
							}
							break;
						case '\t':
						case '\n':
						case '\r':
						case ' ':
							break;
					}
					index++;
				}
				return false;
			}

			private void BeginFigure(Point startPoint)
			{
				FinishFigure(figureExplicitlyClosed: false);
				EnsureFigure();
				figure.StartPoint = startPoint;
				figure.IsFilled = true;
			}

			private void EnsureFigure()
			{
				if (figure == null)
				{
					figure = new PathFigure();
					figure.Segments = new PathSegmentCollection();
				}
			}

			private void FinishFigure(bool figureExplicitlyClosed)
			{
				if (figure != null)
				{
					if (figureExplicitlyClosed)
					{
						figure.IsClosed = true;
					}
					geometry.Figures.Add(figure);
					figure = null;
				}
			}

			private void LineTo(Point point)
			{
				LineSegment lineSegment = new LineSegment();
				lineSegment.Point = point;
				figure.Segments.Add(lineSegment);
			}

			private void BezierTo(Point point1, Point point2, Point point3)
			{
				BezierSegment bezierSegment = new BezierSegment();
				bezierSegment.Point1 = point1;
				bezierSegment.Point2 = point2;
				bezierSegment.Point3 = point3;
				figure.Segments.Add(bezierSegment);
			}

			private void QuadraticBezierTo(Point point1, Point point2)
			{
				QuadraticBezierSegment quadraticBezierSegment = new QuadraticBezierSegment();
				quadraticBezierSegment.Point1 = point1;
				quadraticBezierSegment.Point2 = point2;
				figure.Segments.Add(quadraticBezierSegment);
			}

			private void ArcTo(Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, Point point)
			{
				ArcSegment arcSegment = new ArcSegment();
				arcSegment.Size = size;
				arcSegment.RotationAngle = rotationAngle;
				arcSegment.IsLargeArc = isLargeArc;
				arcSegment.SweepDirection = sweepDirection;
				arcSegment.Point = point;
				figure.Segments.Add(arcSegment);
			}

			private Point GetSmoothBeizerFirstPoint()
			{
				Point result = lastPoint;
				if (figure.Segments.Count > 0 && figure.Segments[figure.Segments.Count - 1] is BezierSegment bezierSegment)
				{
					Point point = bezierSegment.Point2;
					result.X += lastPoint.X - point.X;
					result.Y += lastPoint.Y - point.Y;
				}
				return result;
			}
		}

		/// <summary>
		/// Converts a string in the path mini-language into a PathGeometry.
		/// </summary>
		/// <param name="abbreviatedGeometry">A string in the path mini-language.</param>
		internal static PathGeometry ConvertToPathGeometry(string abbreviatedGeometry)
		{
			if (abbreviatedGeometry == null)
			{
				throw new ArgumentNullException("abbreviatedGeometry");
			}
			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Figures = new PathFigureCollection();
			int i;
			for (i = 0; i < abbreviatedGeometry.Length && char.IsWhiteSpace(abbreviatedGeometry, i); i++)
			{
			}
			if (i < abbreviatedGeometry.Length && abbreviatedGeometry[i] == 'F')
			{
				for (i++; i < abbreviatedGeometry.Length && char.IsWhiteSpace(abbreviatedGeometry, i); i++)
				{
				}
				if (i == abbreviatedGeometry.Length || (abbreviatedGeometry[i] != '0' && abbreviatedGeometry[i] != '1'))
				{
					throw new FormatException();
				}
				pathGeometry.FillRule = ((abbreviatedGeometry[i] != '0') ? FillRule.Nonzero : FillRule.EvenOdd);
				i++;
			}
			new AbbreviatedGeometryParser(pathGeometry).Parse(abbreviatedGeometry, i);
			return pathGeometry;
		}

		/// <summary>
		/// Converts the given geometry into a single PathGeometry.
		/// </summary>
		public static PathGeometry AsPathGeometry(this Geometry original)
		{
			PathGeometry pathGeometry = original as PathGeometry;
			if (pathGeometry == null && (pathGeometry = ConvertToPathGeometry(original as RectangleGeometry)) == null && (pathGeometry = ConvertToPathGeometry(original as EllipseGeometry)) == null && (pathGeometry = ConvertToPathGeometry(original as LineGeometry)) == null && (pathGeometry = ConvertToPathGeometry(original as GeometryGroup)) == null)
			{
				return null;
			}
			return pathGeometry;
		}

		public static bool IsStroked(this PathSegment pathSegment)
		{
			return true;
		}

		public static bool IsSmoothJoin(this PathSegment pathSegment)
		{
			return false;
		}

		public static bool IsFrozen(this Geometry geometry)
		{
			return true;
		}

		/// <summary>
		/// Updates the given geometry as PathGeometry with a polyline matching a given point list.
		/// </summary>
		public static bool SyncPolylineGeometry(ref Geometry geometry, IList<Point> points, bool isClosed)
		{
			bool flag = false;
			PathFigure figure;
			if (!(geometry is PathGeometry pathGeometry) || pathGeometry.Figures.Count != 1 || (figure = pathGeometry.Figures[0]) == null)
			{
				((PathGeometry)(geometry = new PathGeometry())).Figures.Add(figure = new PathFigure());
				flag = true;
			}
			return flag | PathFigureHelper.SyncPolylineFigure(figure, points, isClosed);
		}

		internal static Geometry FixPathGeometryBoundary(Geometry geometry)
		{
			if (geometry is PathGeometry pathGeometry)
			{
				PathFigureCollection figures = pathGeometry.Figures;
				pathGeometry.Figures = null;
				PathGeometry pathGeometry2 = ClonePathGeometry(pathGeometry);
				pathGeometry2.Figures = figures;
				geometry = pathGeometry2;
			}
			return geometry;
		}

		public static Geometry CloneCurrentValue(this Geometry geometry)
		{
			if (geometry == null)
			{
				return null;
			}
			if (geometry is PathGeometry pathGeometry)
			{
				return ClonePathGeometry(pathGeometry);
			}
			if (geometry is LineGeometry lineGeometry)
			{
				return CloneLineGeometry(lineGeometry);
			}
			if (geometry is EllipseGeometry ellipseGeometry)
			{
				return CloneEllipseGeometry(ellipseGeometry);
			}
			if (geometry is RectangleGeometry rectangleGeometry)
			{
				return CloneRectangleGeometry(rectangleGeometry);
			}
			if (geometry is GeometryGroup geometryGroup)
			{
				return CloneGeometryGroup(geometryGroup);
			}
			return geometry.DeepCopy();
		}

		private static PathGeometry ClonePathGeometry(PathGeometry pathGeometry)
		{
			PathGeometry pathGeometry2 = new PathGeometry();
			pathGeometry2.FillRule = pathGeometry.FillRule;
			pathGeometry2.Transform = pathGeometry.Transform.CloneTransform();
			PathGeometry pathGeometry3 = pathGeometry2;
			foreach (PathFigure figure in pathGeometry.Figures)
			{
				pathGeometry3.Figures.Add(ClonePathFigure(figure));
			}
			return pathGeometry3;
		}

		private static PathFigure ClonePathFigure(PathFigure pathFigure)
		{
			PathFigure pathFigure2 = new PathFigure();
			pathFigure2.IsClosed = pathFigure.IsClosed;
			pathFigure2.IsFilled = pathFigure.IsFilled;
			pathFigure2.StartPoint = pathFigure.StartPoint;
			PathFigure pathFigure3 = pathFigure2;
			foreach (PathSegment segment in pathFigure.Segments)
			{
				pathFigure3.Segments.Add(ClonePathSegment(segment));
			}
			return pathFigure3;
		}

		private static PathSegment ClonePathSegment(PathSegment pathSegment)
		{
			if (pathSegment == null)
			{
				return null;
			}
			if (pathSegment is LineSegment lineSegment)
			{
				return CloneLineSegment(lineSegment);
			}
			if (pathSegment is BezierSegment bezierSegment)
			{
				return CloneBezierSegment(bezierSegment);
			}
			if (pathSegment is QuadraticBezierSegment quadraticBezierSegment)
			{
				return CloneQuadraticBezierSegment(quadraticBezierSegment);
			}
			if (pathSegment is ArcSegment arcSegment)
			{
				return CloneArcSegment(arcSegment);
			}
			if (pathSegment is PolyLineSegment polyLineSegment)
			{
				return ClonePolyLineSegment(polyLineSegment);
			}
			if (pathSegment is PolyBezierSegment polyBezierSegment)
			{
				return ClonePolyBezierSegment(polyBezierSegment);
			}
			if (pathSegment is PolyQuadraticBezierSegment polyQuadraticBezierSegment)
			{
				return ClonePolyQuadraticBezierSegment(polyQuadraticBezierSegment);
			}
			return pathSegment.DeepCopy();
		}

		private static LineSegment CloneLineSegment(LineSegment lineSegment)
		{
			LineSegment lineSegment2 = new LineSegment();
			lineSegment2.Point = lineSegment.Point;
			return lineSegment2;
		}

		private static BezierSegment CloneBezierSegment(BezierSegment bezierSegment)
		{
			BezierSegment bezierSegment2 = new BezierSegment();
			bezierSegment2.Point1 = bezierSegment.Point1;
			bezierSegment2.Point2 = bezierSegment.Point2;
			bezierSegment2.Point3 = bezierSegment.Point3;
			return bezierSegment2;
		}

		private static QuadraticBezierSegment CloneQuadraticBezierSegment(QuadraticBezierSegment quadraticBezierSegment)
		{
			QuadraticBezierSegment quadraticBezierSegment2 = new QuadraticBezierSegment();
			quadraticBezierSegment2.Point1 = quadraticBezierSegment.Point1;
			quadraticBezierSegment2.Point2 = quadraticBezierSegment.Point2;
			return quadraticBezierSegment2;
		}

		private static ArcSegment CloneArcSegment(ArcSegment arcSegment)
		{
			ArcSegment arcSegment2 = new ArcSegment();
			arcSegment2.IsLargeArc = arcSegment.IsLargeArc;
			arcSegment2.Point = arcSegment.Point;
			arcSegment2.RotationAngle = arcSegment.RotationAngle;
			arcSegment2.Size = arcSegment.Size;
			arcSegment2.SweepDirection = arcSegment.SweepDirection;
			return arcSegment2;
		}

		private static PolyLineSegment ClonePolyLineSegment(PolyLineSegment polyLineSegment)
		{
			PolyLineSegment polyLineSegment2 = new PolyLineSegment();
			foreach (Point point in polyLineSegment.Points)
			{
				polyLineSegment2.Points.Add(point);
			}
			return polyLineSegment2;
		}

		private static PolyBezierSegment ClonePolyBezierSegment(PolyBezierSegment polyBezierSegment)
		{
			PolyBezierSegment polyBezierSegment2 = new PolyBezierSegment();
			foreach (Point point in polyBezierSegment.Points)
			{
				polyBezierSegment2.Points.Add(point);
			}
			return polyBezierSegment2;
		}

		private static PolyQuadraticBezierSegment ClonePolyQuadraticBezierSegment(PolyQuadraticBezierSegment polyQuadraticBezierSegment)
		{
			PolyQuadraticBezierSegment polyQuadraticBezierSegment2 = new PolyQuadraticBezierSegment();
			foreach (Point point in polyQuadraticBezierSegment.Points)
			{
				polyQuadraticBezierSegment2.Points.Add(point);
			}
			return polyQuadraticBezierSegment2;
		}

		private static EllipseGeometry CloneEllipseGeometry(EllipseGeometry ellipseGeometry)
		{
			EllipseGeometry ellipseGeometry2 = new EllipseGeometry();
			ellipseGeometry2.Center = ellipseGeometry.Center;
			ellipseGeometry2.RadiusX = ellipseGeometry.RadiusX;
			ellipseGeometry2.RadiusY = ellipseGeometry.RadiusY;
			ellipseGeometry2.Transform = ellipseGeometry.Transform.CloneTransform();
			return ellipseGeometry2;
		}

		private static RectangleGeometry CloneRectangleGeometry(RectangleGeometry rectangleGeometry)
		{
			RectangleGeometry rectangleGeometry2 = new RectangleGeometry();
			rectangleGeometry2.Rect = rectangleGeometry.Rect;
			rectangleGeometry2.RadiusX = rectangleGeometry.RadiusX;
			rectangleGeometry2.RadiusY = rectangleGeometry.RadiusY;
			rectangleGeometry2.Transform = rectangleGeometry.Transform.CloneTransform();
			return rectangleGeometry2;
		}

		private static LineGeometry CloneLineGeometry(LineGeometry lineGeometry)
		{
			LineGeometry lineGeometry2 = new LineGeometry();
			lineGeometry2.StartPoint = lineGeometry.StartPoint;
			lineGeometry2.EndPoint = lineGeometry.EndPoint;
			lineGeometry2.Transform = lineGeometry.Transform.CloneTransform();
			return lineGeometry2;
		}

		private static GeometryGroup CloneGeometryGroup(GeometryGroup geometryGroup)
		{
			GeometryGroup geometryGroup2 = new GeometryGroup();
			geometryGroup2.FillRule = geometryGroup.FillRule;
			geometryGroup2.Transform = geometryGroup.Transform.CloneTransform();
			GeometryGroup geometryGroup3 = geometryGroup2;
			foreach (Geometry child in geometryGroup.Children)
			{
				geometryGroup3.Children.Add(child.CloneCurrentValue());
			}
			return geometryGroup3;
		}

		private static PathGeometry ConvertToPathGeometry(EllipseGeometry ellipseGeometry)
		{
			if (ellipseGeometry != null)
			{
				Rect bounds = ellipseGeometry.Bounds;
				if (bounds.Size().HasValidArea())
				{
					Point point = GeometryHelper.Lerp(bounds.TopLeft(), bounds.TopRight(), 0.5);
					Point point2 = GeometryHelper.Lerp(bounds.BottomLeft(), bounds.BottomRight(), 0.5);
					Size size = new Size(ellipseGeometry.RadiusX, ellipseGeometry.RadiusY);
					PathGeometry pathGeometry = new PathGeometry();
					pathGeometry.Transform = ellipseGeometry.Transform;
					pathGeometry.Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = point,
						IsClosed = true,
						IsFilled = true,
						Segments = new PathSegmentCollection
						{
							new ArcSegment
							{
								Point = point2,
								IsLargeArc = true,
								Size = size,
								SweepDirection = SweepDirection.Clockwise
							},
							new ArcSegment
							{
								Point = point,
								IsLargeArc = true,
								Size = size,
								SweepDirection = SweepDirection.Clockwise
							}
						}
					}
				};
					return pathGeometry;
				}
			}
			return null;
		}

		private static PathGeometry ConvertToPathGeometry(RectangleGeometry rectangleGeometry)
		{
			if (rectangleGeometry == null)
			{
				return null;
			}
			Rect bounds = rectangleGeometry.Bounds;
			if (!bounds.Size().HasValidArea())
			{
				return null;
			}
			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Transform = rectangleGeometry.Transform;
			PathFigure pathFigure = new PathFigure();
			pathFigure.IsClosed = true;
			pathFigure.IsFilled = true;
			PathFigure pathFigure2 = pathFigure;
			pathGeometry.Figures.Add(pathFigure2);
			if (rectangleGeometry.RadiusX * rectangleGeometry.RadiusY == 0.0)
			{
				pathFigure2.StartPoint = bounds.TopLeft();
				pathFigure2.Segments.Add(new PolyLineSegment
				{
					Points = new PointCollection
				{
					bounds.TopRight(),
					bounds.BottomRight(),
					bounds.BottomLeft()
				}
				});
				return pathGeometry;
			}
			bool flag = Math.Abs(rectangleGeometry.RadiusX) < bounds.Width / 2.0;
			bool flag2 = Math.Abs(rectangleGeometry.RadiusY) < bounds.Height / 2.0;
			double num = Math.Min(Math.Abs(rectangleGeometry.RadiusX), bounds.Width / 2.0);
			double num2 = Math.Min(Math.Abs(rectangleGeometry.RadiusY), bounds.Height / 2.0);
			Size cornerSize = new Size(num, num2);
			pathFigure2.StartPoint = new Point(bounds.Left, bounds.Top + num2);
			pathFigure2.Segments.Add(CreateCorner(new Point(bounds.Left + num, bounds.Top), cornerSize));
			if (flag)
			{
				pathFigure2.Segments.Add(new LineSegment
				{
					Point = new Point(bounds.Right - num, bounds.Top)
				});
			}
			pathFigure2.Segments.Add(CreateCorner(new Point(bounds.Right, bounds.Top + num2), cornerSize));
			if (flag2)
			{
				pathFigure2.Segments.Add(new LineSegment
				{
					Point = new Point(bounds.Right, bounds.Bottom - num2)
				});
			}
			pathFigure2.Segments.Add(CreateCorner(new Point(bounds.Right - num, bounds.Bottom), cornerSize));
			if (flag)
			{
				pathFigure2.Segments.Add(new LineSegment
				{
					Point = new Point(bounds.Left + num, bounds.Bottom)
				});
			}
			pathFigure2.Segments.Add(CreateCorner(new Point(bounds.Left, bounds.Bottom - num2), cornerSize));
			return pathGeometry;
		}

		private static ArcSegment CreateCorner(Point endPoint, Size cornerSize)
		{
			ArcSegment arcSegment = new ArcSegment();
			arcSegment.IsLargeArc = false;
			arcSegment.SweepDirection = SweepDirection.Clockwise;
			arcSegment.Point = endPoint;
			arcSegment.Size = cornerSize;
			return arcSegment;
		}

		private static PathGeometry ConvertToPathGeometry(LineGeometry lineGeometry)
		{
			if (lineGeometry != null)
			{
				Rect bounds = lineGeometry.Bounds;
				if (bounds.Size().HasValidArea())
				{
					PathGeometry pathGeometry = new PathGeometry();
					pathGeometry.Transform = lineGeometry.Transform;
					pathGeometry.Figures = new PathFigureCollection
				{
					new PathFigure
					{
						StartPoint = lineGeometry.StartPoint,
						IsClosed = false,
						IsFilled = false,
						Segments = new PathSegmentCollection
						{
							new LineSegment
							{
								Point = lineGeometry.EndPoint
							}
						}
					}
				};
					return pathGeometry;
				}
			}
			return null;
		}

		private static PathGeometry ConvertToPathGeometry(GeometryGroup geometryGroup)
		{
			if (geometryGroup != null)
			{
				Rect bounds = geometryGroup.Bounds;
				if (bounds.Size().HasValidArea())
				{
					PathGeometry pathGeometry = new PathGeometry();
					pathGeometry.SetIfDifferent(PathGeometry.FillRuleProperty, geometryGroup.FillRule);
					LinkedList<Geometry> linkedList = new LinkedList<Geometry>();
					linkedList.AddLast(geometryGroup);
					while (linkedList.Count > 0)
					{
						LinkedListNode<Geometry> first = linkedList.First;
						Geometry value = first.Value;
						if (value is GeometryGroup geometryGroup2)
						{
							foreach (Geometry child in geometryGroup2.Children)
							{
								linkedList.AddAfter(first, child);
							}
						}
						else
						{
							PathGeometry pathGeometry2 = value.AsPathGeometry();
							if (pathGeometry2 != null)
							{
								for (int num = pathGeometry2.Figures.Count - 1; num >= 0; num--)
								{
									PathFigure value2 = pathGeometry2.Figures[num];
									pathGeometry2.Figures.RemoveAt(num);
									pathGeometry.Figures.Add(value2);
								}
							}
						}
						linkedList.RemoveFirst();
					}
					return pathGeometry;
				}
			}
			return null;
		}
	}
}
#endif