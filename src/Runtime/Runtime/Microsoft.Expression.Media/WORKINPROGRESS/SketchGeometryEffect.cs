
using System;
using System.Collections.Generic;

#if MIGRATION
using Microsoft.Expression.Drawing.Core;
using System.Windows;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// A geometry effect that transforms any geometry into a Sketch style as in SketchFlow.
	/// </summary>
	public sealed class SketchGeometryEffect : GeometryEffect
	{
		private const double expectedLengthMean = 8.0;

		private const double normalDisturbVariance = 0.5;

		private const double tangentDisturbVariance = 1.0;

		private const double bsplineWeight = 0.05;

		/// <summary>
		/// Use the same random seed on creation to keep visual flickering to a minimum.
		/// </summary>
		private readonly long randomSeed = DateTime.Now.Ticks;

		/// <summary>
		/// Makes a deep copy of the geometry effect.
		/// </summary>
		/// <returns>A clone of the current instance of the geometry effect.</returns>
		protected override GeometryEffect DeepCopy()
		{
			return new SketchGeometryEffect();
		}

		/// <summary>
		/// Tests if the given geometry effect is equivalent to the current instance.
		/// </summary>
		/// <param name="geometryEffect">A geometry effect to compare with.</param>
		/// <returns>Returns true when two effects render with the same appearance.</returns>
		public override bool Equals(GeometryEffect geometryEffect)
		{
			return geometryEffect is SketchGeometryEffect;
		}

		/// <summary>
		/// Updating cachedGeometry based on the given input geometry.
		/// </summary>
		/// <param name="input">An input geometry.</param>
		/// <returns>Returns true when anything on cachedGeometry has been updated.</returns>
		protected override bool UpdateCachedGeometry(Geometry input)
		{
			bool flag = false;
			PathGeometry pathGeometry = input.AsPathGeometry();
			if (pathGeometry != null)
			{
				flag |= UpdateSketchGeometry(pathGeometry);
			}
			else
			{
				cachedGeometry = input;
			}
			return flag;
		}

		private bool UpdateSketchGeometry(PathGeometry inputPath)
		{
			bool flag = false;
			flag |= GeometryHelper.EnsureGeometryType(out var result, ref cachedGeometry, () => new PathGeometry());
			flag |= result.Figures.EnsureListCount(inputPath.Figures.Count, () => new PathFigure());
			RandomEngine random = new RandomEngine(randomSeed);
			for (int i = 0; i < inputPath.Figures.Count; i++)
			{
				PathFigure pathFigure = inputPath.Figures[i];
				bool isClosed = pathFigure.IsClosed;
				bool isFilled = pathFigure.IsFilled;
				if (pathFigure.Segments.Count == 0)
				{
					flag |= result.Figures[i].SetIfDifferent(PathFigure.StartPointProperty, pathFigure.StartPoint);
					flag |= result.Figures[i].Segments.EnsureListCount(0);
					continue;
				}
				List<Point> list = new List<Point>(pathFigure.Segments.Count * 3);
				foreach (SimpleSegment effectiveSegment in GetEffectiveSegments(pathFigure))
				{
					List<Point> list2 = new List<Point>();
					list2.Add(effectiveSegment.Points[0]);
					double tolerance = 0.0;
					IList<double> resultParameters = null;
					effectiveSegment.Flatten(list2, tolerance, resultParameters);
					PolylineData polyline = new PolylineData(list2);
					if (list2.Count > 1 && polyline.TotalLength > 4.0)
					{
						double a = polyline.TotalLength / 8.0;
						int sampleCount = (int)Math.Max(2.0, Math.Ceiling(a));
						double interval = polyline.TotalLength / (double)sampleCount;
						double scale = interval / 8.0;
						List<Point> samplePoints = new List<Point>(sampleCount);
						List<Vector> sampleNormals = new List<Vector>(sampleCount);
						int sampleIndex = 0;
						PolylineHelper.PathMarch(polyline, 0.0, 0.0, delegate (MarchLocation location)
						{
							if (location.Reason == MarchStopReason.CompletePolyline)
							{
								return double.NaN;
							}
							if (location.Reason == MarchStopReason.CompleteStep)
							{
								if (sampleIndex++ == sampleCount)
								{
									return double.NaN;
								}
								samplePoints.Add(location.GetPoint(polyline.Points));
								sampleNormals.Add(location.GetNormal(polyline));
								return interval;
							}
							return location.Remain;
						});
						DisturbPoints(random, scale, samplePoints, sampleNormals);
						list.AddRange(samplePoints);
					}
					else
					{
						list.AddRange(list2);
						list.RemoveLast();
					}
				}
				if (!isClosed)
				{
					list.Add(pathFigure.Segments.Last().GetLastPoint());
				}
				flag |= PathFigureHelper.SyncPolylineFigure(result.Figures[i], list, isClosed, isFilled);
			}
			if (flag)
			{
				cachedGeometry = PathGeometryHelper.FixPathGeometryBoundary(cachedGeometry);
			}
			return flag;
		}

		private static void DisturbPoints(RandomEngine random, double scale, IList<Point> points, IList<Vector> normals)
		{
			int count = points.Count;
			for (int i = 1; i < count; i++)
			{
				double num = random.NextGaussian(0.0, 1.0 * scale);
				double num2 = random.NextUniform(-0.5, 0.5) * scale;
				points[i] = new Point(points[i].X + normals[i].X * num2 - normals[i].Y * num, points[i].Y + normals[i].X * num + normals[i].Y * num2);
			}
		}

		/// <summary>
		/// Iterates all simple segments in given path figure including the closing chord.
		/// </summary>
		private IEnumerable<SimpleSegment> GetEffectiveSegments(PathFigure pathFigure)
		{
			Point lastPoint = pathFigure.StartPoint;
			foreach (PathSegmentData data in pathFigure.AllSegments())
			{
				foreach (SimpleSegment segment in data.PathSegment.GetSimpleSegments(data.StartPoint))
				{
					yield return segment;
					lastPoint = segment.Points.Last();
				}
			}
			if (pathFigure.IsClosed)
			{
				yield return SimpleSegment.Create(lastPoint, pathFigure.StartPoint);
			}
		}
	}
}
#endif