using System;
using System.Windows;
using Microsoft.Expression.Media;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Provides helper extension methods to work with IGeometrySource and parameters.
	/// </summary>
	internal static class IGeometrySourceExtensions
	{
		public static double GetHalfStrokeThickness(this IGeometrySourceParameters parameter)
		{
			if (parameter.Stroke != null)
			{
				double strokeThickness = parameter.StrokeThickness;
				if (!double.IsNaN(strokeThickness) && !double.IsInfinity(strokeThickness))
				{
					return Math.Abs(strokeThickness) / 2.0;
				}
			}
			return 0.0;
		}

		public static GeometryEffect GetGeometryEffect(this IGeometrySourceParameters parameters)
		{
			if (parameters is DependencyObject dependencyObject)
			{
				GeometryEffect geometryEffect = GeometryEffect.GetGeometryEffect(dependencyObject);
				if (geometryEffect == null || !dependencyObject.Equals(geometryEffect.Parent))
				{
					return null;
				}
				return geometryEffect;
			}
			return null;
		}
	}
}
#endif