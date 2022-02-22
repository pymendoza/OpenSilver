using System.Windows;
using System.Windows.Media;
#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// A Tuple data structure for PathSegment and the corresponding StartPoint.
	/// </summary>
	internal sealed class PathSegmentData
	{
		public Point StartPoint { get; private set; }

		public PathSegment PathSegment { get; private set; }

		public PathSegmentData(Point startPoint, PathSegment pathSegment)
		{
			PathSegment = pathSegment;
			StartPoint = startPoint;
		}
	}
}
#endif