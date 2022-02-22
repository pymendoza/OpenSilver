#if MIGRATION
using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Expression.Drawing.Core;

namespace Microsoft.Expression.Media
{
	internal class PolygonGeometrySource : GeometrySource<IPolygonGeometrySourceParameters>
	{
		private List<Point> cachedPoints = new List<Point>();

		/// <summary>
		/// Polygon recognizes Stretch.None as the same as Stretch.Fill.
		/// </summary>
		protected override Rect ComputeLogicalBounds(Rect layoutBounds, IGeometrySourceParameters parameters)
		{
			Rect logicalBound = base.ComputeLogicalBounds(layoutBounds, parameters);
			return GeometryHelper.GetStretchBound(logicalBound, parameters.Stretch, new Size(1.0, 1.0));
		}

		protected override bool UpdateCachedGeometry(IPolygonGeometrySourceParameters parameters)
		{
			bool flag = false;
			int num = Math.Max(3, Math.Min(100, (int)Math.Round(parameters.PointCount)));
			double num2 = 360.0 / (double)num;
			double num3 = Math.Max(0.0, Math.Min(1.0, parameters.InnerRadius));
			if (num3 < 1.0)
			{
				double num4 = Math.Cos(Math.PI / (double)num);
				double ratio = num3 * num4;
				double num5 = num2 / 2.0;
				cachedPoints.EnsureListCount(num * 2);
				Rect bound = base.LogicalBounds.Resize(ratio);
				for (int i = 0; i < num; i++)
				{
					double num6 = num2 * (double)i;
					cachedPoints[i * 2] = GeometryHelper.GetArcPoint(num6, base.LogicalBounds);
					cachedPoints[i * 2 + 1] = GeometryHelper.GetArcPoint(num6 + num5, bound);
				}
			}
			else
			{
				cachedPoints.EnsureListCount(num);
				for (int j = 0; j < num; j++)
				{
					double degree = num2 * (double)j;
					cachedPoints[j] = GeometryHelper.GetArcPoint(degree, base.LogicalBounds);
				}
			}
			return flag | PathGeometryHelper.SyncPolylineGeometry(ref cachedGeometry, cachedPoints, isClosed: true);
		}
	}
}
#endif