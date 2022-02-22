using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Expression.Media;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Provides the base class for GeometryEffect that transforms a geometry into another geometry.
	/// </summary>
	/// <remarks>
	/// This class provides the basic implementation of processing the rendered geometry of a IShape before it's passed to rendering.
	/// A typical implementation will extend the virtual function <see cref="F:ProcessGeometry" /> to transform the input geometry.
	/// <see cref="T:GeometryEffect" /> is typically attached to <see cref="T:IShape" /> as an attached property and activated when <see cref="T:IShape" /> geometry is updated.
	/// The <see cref="P:OutputGeometry" /> of a <see cref="T:GeometryEffect" /> will replace the rendered geometry in <see cref="T:IShape" />.
	/// </remarks>
	[TypeConverter(typeof(GeometryEffectConverter))]
	public abstract class GeometryEffect : DependencyObject
	{
		private class NoGeometryEffect : GeometryEffect
		{
			protected override bool UpdateCachedGeometry(Geometry input)
			{
				cachedGeometry = input;
				return false;
			}

			protected override GeometryEffect DeepCopy()
			{
				return new NoGeometryEffect();
			}

			public override bool Equals(GeometryEffect geometryEffect)
			{
				if (geometryEffect != null)
				{
					return geometryEffect is NoGeometryEffect;
				}
				return true;
			}
		}

		public static readonly DependencyProperty GeometryEffectProperty;

		private static GeometryEffect defaultGeometryEffect;

		/// <summary>
		/// Specifics the geometry from the previous geometry effect process.
		/// </summary>
		protected Geometry cachedGeometry;

		private bool effectInvalidated;

		/// <summary>
		/// The default geometry effect that only passes through the input geometry.
		/// </summary>
		public static GeometryEffect DefaultGeometryEffect => defaultGeometryEffect ?? (defaultGeometryEffect = new NoGeometryEffect());

		/// <summary>
		/// Gets the output geometry of this geometry effect.
		/// </summary>
		public Geometry OutputGeometry => cachedGeometry;

		/// <summary>
		/// Parent can be either IShape or GeometryEffectGroup.
		/// </summary>
		protected internal DependencyObject Parent { get; private set; }

		/// <summary>
		/// Gets the geometry effect as an attached property on a given dependency object.
		/// </summary>
		public static GeometryEffect GetGeometryEffect(DependencyObject obj)
		{
			return (GeometryEffect)obj.GetValue(GeometryEffectProperty);
		}

		/// <summary>
		/// Sets the geometry effect as an attached property on a given dependency object.
		/// </summary>
		public static void SetGeometryEffect(DependencyObject obj, GeometryEffect value)
		{
			obj.SetValue(GeometryEffectProperty, value);
		}

		private static void OnGeometryEffectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GeometryEffect geometryEffect = e.OldValue as GeometryEffect;
			GeometryEffect geometryEffect2 = e.NewValue as GeometryEffect;
			if (geometryEffect == geometryEffect2)
			{
				return;
			}
			if (geometryEffect != null && obj.Equals(geometryEffect.Parent))
			{
				geometryEffect.Detach();
			}
			if (geometryEffect2 != null)
			{
				if (geometryEffect2.Parent != null)
				{
					GeometryEffect value = geometryEffect2.CloneCurrentValue();
					obj.SetValue(GeometryEffectProperty, value);
				}
				else
				{
					geometryEffect2.Attach(obj);
				}
			}
		}

		/// <summary>
		/// Makes a deep copy of the <see cref="T:GeometryEffect" /> using its current values.
		/// </summary>
		public GeometryEffect CloneCurrentValue()
		{
			return DeepCopy();
		}

		/// <summary>
		/// Makes a deep copy of the geometry effect. Implements CloneCurrentValue in Silverlight.
		/// </summary>
		/// <returns>A clone of the current instance of the geometry effect.</returns>
		protected abstract GeometryEffect DeepCopy();

		/// <summary>
		/// Tests if the given geometry effect is equivalent to the current instance.
		/// </summary>
		/// <param name="geometryEffect">A geometry effect to compare with.</param>
		/// <returns>Returns true when two effects render with the same appearance.</returns>
		public abstract bool Equals(GeometryEffect geometryEffect);

		static GeometryEffect()
		{
			GeometryEffectProperty = DependencyProperty.RegisterAttached("GeometryEffect", typeof(GeometryEffect), typeof(GeometryEffect), new DrawingPropertyMetadata(DefaultGeometryEffect, DrawingPropertyMetadataOptions.AffectsRender, OnGeometryEffectChanged));
			DrawingPropertyMetadata.DrawingPropertyChanged += delegate (object sender, DrawingPropertyChangedEventArgs args)
			{
				if (sender is GeometryEffect geometryEffect && args.Metadata.AffectsRender)
				{
					geometryEffect.InvalidateGeometry(InvalidateGeometryReasons.PropertyChanged);
				}
			};
		}

		/// <summary>
		/// Invalidates the geometry effect without actually computing the geometry.
		/// Notifies all parent shapes or effects to invalidate accordingly.
		/// </summary>
		public bool InvalidateGeometry(InvalidateGeometryReasons reasons)
		{
			if (!effectInvalidated)
			{
				effectInvalidated = true;
				if (reasons != InvalidateGeometryReasons.ParentInvalidated)
				{
					InvalidateParent(Parent);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Processes the geometry effect on a given input geometry.
		/// Stores the result in GeometryEffect.OutputGeometry.
		/// </summary>
		/// <returns>Returns false if nothing has been changed.</returns>
		public bool ProcessGeometry(Geometry input)
		{
			bool flag = false;
			if (effectInvalidated)
			{
				flag |= UpdateCachedGeometry(input);
				effectInvalidated = false;
			}
			return flag;
		}

		/// <summary>
		/// Extends the way of updating cachedGeometry based on a given input geometry.
		/// </summary>
		protected abstract bool UpdateCachedGeometry(Geometry input);

		/// <summary>
		/// Notified when detached from a parent chain.
		/// </summary>
		protected internal virtual void Detach()
		{
			effectInvalidated = true;
			cachedGeometry = null;
			if (Parent != null)
			{
				InvalidateParent(Parent);
				Parent = null;
			}
		}

		/// <summary>
		/// Notified when attached to a parent chain.
		/// </summary>
		protected internal virtual void Attach(DependencyObject obj)
		{
			if (Parent != null)
			{
				Detach();
			}
			effectInvalidated = true;
			cachedGeometry = null;
			if (InvalidateParent(obj))
			{
				Parent = obj;
			}
		}

		/// <summary>
		/// Invalidates the geometry on a given dependency object when
		/// the object is a valid parent type (IShape or GeometryEffect).
		/// </summary>
		private static bool InvalidateParent(DependencyObject parent)
		{
			if (parent is IShape shape)
			{
				shape.InvalidateGeometry(InvalidateGeometryReasons.ChildInvalidated);
				return true;
			}
			if (parent is GeometryEffect geometryEffect)
			{
				geometryEffect.InvalidateGeometry(InvalidateGeometryReasons.ChildInvalidated);
				return true;
			}
			return false;
		}
	}
}
#endif