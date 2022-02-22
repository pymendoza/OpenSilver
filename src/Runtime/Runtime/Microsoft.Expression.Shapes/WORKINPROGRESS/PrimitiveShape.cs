#if MIGRATION
using Microsoft.Expression.Media;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif

#if MIGRATION
namespace Microsoft.Expression.Shapes
{
    public abstract class PrimitiveShape : Path, IGeometrySourceParameters, IShape
    {
        protected PrimitiveShape() { }

        //
        // Summary:
        //     Gets or sets the rendered geometry of the primitive shape, mapping to System.Windows.Shapes.Path.Data
        //     in Silverlight.
        //
        // Remarks:
        //     The concept corresponds to Shape.RenderedGeometry in WPF.
        public Geometry RenderedGeometry { get; }
        //
        // Summary:
        //     Gets the margin between logical bounds and actual geometry bounds. This can be
        //     either positive (as in Microsoft.Expression.Shapes.Arc) or negative (as in Microsoft.Expression.Controls.Callout).
        public Thickness GeometryMargin { get; }
        //
        // Summary:
        //     Gets or sets a System.Windows.Media.Geometry that specifies the shape to be drawn.
        //
        // Returns:
        //     A description of the shape to be drawn.
        //
        // Remarks:
        //     A new Data property to hide Path.Data property, to avoid being serialized to
        //     XAML. This property will carry DesignerSerializationVisibilityAttribute.
        //public Geometry Data { get; set; }

        //
        // Summary:
        //     Occurs when RenderedGeometry is changed.
        public event EventHandler RenderedGeometryChanged;

        //
        // Summary:
        //     Invalidates the geometry for a Microsoft.Expression.Media.IShape. After the invalidation,
        //     the Microsoft.Expression.Media.IShape will recompute the geometry, which will
        //     occur asynchronously.
        public void InvalidateGeometry(InvalidateGeometryReasons reasons) { }
        //
        // Summary:
        //     Provides the behavior for the Arrange portion of Silverlight layout pass. Classes
        //     can override this method to define their own Arrange pass behavior.
        //
        // Parameters:
        //   finalSize:
        //     The final area within the parent that this object should use to arrange itself
        //     and its children.
        //
        // Returns:
        //     The actual size used once the element is arranged in layout.
        //
        // Remarks:
        //     Microsoft.Expression.Shapes.PrimitiveShape will recompute the Geometry when it's
        //     invalidated and update the RenderedGeometry and GeometryMargin.
        protected override Size ArrangeOverride(Size finalSize) { return new Size(); }
        //
        // Summary:
        //     Extends how the shape is drawn with creating geometry source.
        protected abstract IGeometrySource CreateGeometrySource();
        //
        // Summary:
        //     Provides the behavior for the Measure portion of Silverlight layout pass. Classes
        //     can override this method to define their own Measure pass behavior.
        //
        // Parameters:
        //   availableSize:
        //     The available size that this object can provide to child objects. Infinity (System.Double.PositiveInfinity)
        //     can be specified as a value to indicate that the object will size to whatever
        //     content is available.
        //
        // Returns:
        //     The size that this object determines it requires during layout, based on its
        //     calculations of child object allotted sizes, or possibly on other considerations
        //     such as fixed container size.
        //
        // Remarks:
        //     In WPF, measure override works from Shape.DefiningGeometry which is not always
        //     as expected see bug 99497 for details where WPF is not having correct measure
        //     by default. In Silverlight, measure override on Path does not work the same as
        //     primitive shape works. We should return the smallest size this shape can correctly
        //     render without clipping. By default a shape can render as small as a dot, therefore
        //     returning the strokethickness.
        protected override Size MeasureOverride(Size availableSize) { return new Size(); }
    }
}
#endif