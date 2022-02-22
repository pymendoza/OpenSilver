#if MIGRATION
using System.Windows;
using System.Windows.Media;
using Microsoft.Expression.Drawing.Core;

namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Provides the base class of a source of geometry.
	/// Generates and caches the geometry based on the input parameters and the layout bounds.
	/// </summary>
	/// <remarks>
	/// A typical implementation will extend the UpdateCachedGeometry() to update this.cachedGeometry.
	/// This base class will then handle the invalidation, pipeline to the geometry effects, and then cache relative to the layout bounds.
	/// An implementation should try to reuse the cached geometry as much as possible to avoid reconstruction in the rendering thread.
	/// An implementation can extend the ComputeLogicalBounds to handle Stretch differently.
	/// </remarks>
	/// <typeparam name="TParameters">The type of geometry source parameter on which the base class is working on.</typeparam>
	public abstract class GeometrySource<TParameters> : IGeometrySource where TParameters : IGeometrySourceParameters
	{
		private bool geometryInvalidated;

		/// <summary>
		/// Specifics the geometry from the previous geometry effect process.
		/// </summary>
		protected Geometry cachedGeometry;

		/// <summary>
		/// Gets or sets the resulting geometry after the latest UpdateGeometry().
		/// </summary>
		public Geometry Geometry { get; private set; }

		/// <summary>
		/// Gets the bounding box that the geometry should stretch to.
		/// The actual geometry might be smaller or larger than this.
		/// <see cref="P:Microsoft.Expression.Media.GeometrySource`1.LogicalBounds" /> should already take stroke thickness and stretch into consideration.
		/// </summary>
		/// <value></value>
		public Rect LogicalBounds { get; private set; }

		/// <summary>
		/// Gets the actual bounds of FrameworkElement.
		/// <see cref="P:Microsoft.Expression.Media.GeometrySource`1.LayoutBounds" /> includes logical bounds, stretch and stroke thickness.
		/// </summary>
		/// <value></value>
		public Rect LayoutBounds { get; private set; }

		/// <summary>
		/// Notifies that the geometry has been invalidated because of external changes.
		/// </summary>
		/// <remarks>
		/// The geometry is typically invalidated when parameters are changed.
		/// If any geometry has been invalidated externally, the geometry will be recomputed regardless if the layout bounds change.
		/// </remarks>
		public bool InvalidateGeometry(InvalidateGeometryReasons reasons)
		{
			if ((reasons & InvalidateGeometryReasons.TemplateChanged) != 0)
			{
				cachedGeometry = null;
			}
			if (!geometryInvalidated)
			{
				geometryInvalidated = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Update the geometry based on the given parameters and layoutBounds.
		/// Returns false if the geometry hasn't been changed.
		/// </summary>
		public bool UpdateGeometry(IGeometrySourceParameters parameters, Rect layoutBounds)
		{
			bool flag = false;
			if (parameters is TParameters)
			{
				Rect rect = ComputeLogicalBounds(layoutBounds, parameters);
				flag |= LayoutBounds != layoutBounds || LogicalBounds != rect;
				if (geometryInvalidated || flag)
				{
					LayoutBounds = layoutBounds;
					LogicalBounds = rect;
					flag |= UpdateCachedGeometry((TParameters)parameters);
					bool num = flag;
					bool force = flag;
					flag = num | ApplyGeometryEffect(parameters, force);
				}
			}
			geometryInvalidated = false;
			return flag;
		}

		/// <summary>
		/// Extends the way to provide geometry by implementing this function.
		/// Returns true when any of the geometry is changed.
		/// </summary>
		protected abstract bool UpdateCachedGeometry(TParameters parameters);

		/// <summary>
		/// Extends the way to handle stretch mode.
		/// The default is to always use Stretch.Fill and center stroke.
		/// </summary>
		protected virtual Rect ComputeLogicalBounds(Rect layoutBounds, IGeometrySourceParameters parameters)
		{
			return GeometryHelper.Inflate(layoutBounds, 0.0 - parameters.GetHalfStrokeThickness());
		}

		/// <summary>
		/// Apply the geometry effect when dirty or forced and update this.Geometry.
		/// Otherwise, keep this.Geometry as this.cachedGeometry.
		/// </summary>
		private bool ApplyGeometryEffect(IGeometrySourceParameters parameters, bool force)
		{
			bool result = false;
			Geometry outputGeometry = cachedGeometry;
			GeometryEffect geometryEffect = parameters.GetGeometryEffect();
			if (geometryEffect != null)
			{
				if (force)
				{
					result = true;
					geometryEffect.InvalidateGeometry(InvalidateGeometryReasons.ParentInvalidated);
				}
				if (geometryEffect.ProcessGeometry(cachedGeometry))
				{
					result = true;
					outputGeometry = geometryEffect.OutputGeometry;
				}
			}
			if (Geometry != outputGeometry)
			{
				result = true;
				Geometry = outputGeometry;
			}
			return result;
		}
	}
}
#endif