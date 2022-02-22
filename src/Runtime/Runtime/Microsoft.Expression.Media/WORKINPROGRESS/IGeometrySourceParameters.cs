using System;
using System.Windows.Media;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Provides an interface to describe the parameters of a Shape.
	/// </summary>
	/// <remarks>
	/// This interface is the data for communication between Shape and GeometrySource.
	/// Typically, a concrete implementation of IShape will implement this interface and pass it into
	/// GeometrySource.UpdateGeometry(), which will then consume the shape as a read-only data provider.
	/// </remarks>
	public interface IGeometrySourceParameters
	{
		Stretch Stretch { get; }

		Brush Stroke { get; }

		double StrokeThickness { get; }
	}
}
#endif