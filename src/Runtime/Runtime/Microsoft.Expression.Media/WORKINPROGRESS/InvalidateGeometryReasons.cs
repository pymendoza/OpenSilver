using System;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Specifies the reason of <see cref="M:Microsoft.Expression.Media.InvalidateGeometry" /> being called.
	/// </summary>
	[Flags]
	public enum InvalidateGeometryReasons
	{
		/// <summary>
		/// Geometry has been invalidated because a property has been changed.
		/// </summary>
		PropertyChanged = 1,
		/// <summary>
		/// Geometry has been invalidated because a property is being animated.
		/// </summary>
		IsAnimated = 2,
		/// <summary>
		/// Geometry has been invalidated because a child has been invalidated.
		/// </summary>
		ChildInvalidated = 4,
		/// <summary>
		/// Geometry has been invalidated because a parent has been invalidated.
		/// </summary>
		ParentInvalidated = 8,
		/// <summary>
		/// Geometry has been invalidated because a new template has been applied.
		/// </summary>
		TemplateChanged = 0x10
	}
}
#endif