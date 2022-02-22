using Microsoft.Expression.Media;
using CSHTML5.Internal;

#if MIGRATION
using System.Windows;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#endif

#if MIGRATION
namespace Microsoft.Expression.Shapes
{
	/// <summary>
	/// Renders a regular polygon shape or corresponding star shape with variable number of points.
	/// </summary>
	public sealed class RegularPolygon : Shape,/*PrimitiveShape,*/ IPolygonGeometrySourceParameters, IGeometrySourceParameters
	{
		public static readonly DependencyProperty PointCountProperty = DependencyProperty.Register("PointCount", typeof(double), typeof(RegularPolygon), new DrawingPropertyMetadata(6.0, DrawingPropertyMetadataOptions.AffectsRender));

		public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register("InnerRadius", typeof(double), typeof(RegularPolygon), new DrawingPropertyMetadata(1.0, DrawingPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gets or sets the number of points of the <see cref="T:Microsoft.Expression.Shapes.RegularPolygon" />.
		/// </summary>
		public double PointCount
		{
			get
			{
				return (double)GetValue(PointCountProperty);
			}
			set
			{
				SetValue(PointCountProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the the distance between the center and the innermost point.
		/// </summary>
		/// <value>The distance between the center and the innermost point.</value>
		public double InnerRadius
		{
			get
			{
				return (double)GetValue(InnerRadiusProperty);
			}
			set
			{
				SetValue(InnerRadiusProperty, value);
			}
		}

		//protected override IGeometrySource CreateGeometrySource()
		//{
		//	return new PolygonGeometrySource();
		//}

  //      public override object CreateDomElement(object parentRef, out object domElementWhereToPlaceChildren)
  //      {
		//	return INTERNAL_ShapesDrawHelpers.CreateDomElementForPathAndSimilar(this, parentRef, out _canvasDomElement, out domElementWhereToPlaceChildren);
		//}

        //[SpecialName]
        //Stretch IGeometrySourceParameters.get_Stretch()
        //{
        //	return base.Stretch;
        //}

        //[SpecialName]
        //Brush IGeometrySourceParameters.get_Stroke()
        //{
        //	return base.Stroke;
        //}

        //[SpecialName]
        //double IGeometrySourceParameters.get_StrokeThickness()
        //{
        //	return base.StrokeThickness;
        //}
    }
}
#endif