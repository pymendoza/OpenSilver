using System;
using System.Windows;
using Microsoft.Expression.Media;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Unifies the interface of PropertyMetadata in WPF and Silverlight.
	/// Provides the necessary notification about render, arrange, or measure.
	/// </summary>
	internal class DrawingPropertyMetadata : PropertyMetadata
	{
		private DrawingPropertyMetadataOptions options;

		private PropertyChangedCallback propertyChangedCallback;

		public bool AffectsRender => (options & DrawingPropertyMetadataOptions.AffectsRender) != 0;

		public bool AffectsMeasure => (options & DrawingPropertyMetadataOptions.AffectsMeasure) != 0;

		public static event EventHandler<DrawingPropertyChangedEventArgs> DrawingPropertyChanged;

		public DrawingPropertyMetadata(object defaultValue)
			: this(defaultValue, DrawingPropertyMetadataOptions.None, null)
		{
		}

		public DrawingPropertyMetadata(PropertyChangedCallback propertyChangedCallback)
			: this(DependencyProperty.UnsetValue, DrawingPropertyMetadataOptions.None, propertyChangedCallback)
		{
		}

		public DrawingPropertyMetadata(object defaultValue, DrawingPropertyMetadataOptions options)
			: this(defaultValue, options, null)
		{
		}

		public DrawingPropertyMetadata(object defaultValue, DrawingPropertyMetadataOptions options, PropertyChangedCallback propertyChangedCallback)
			: base(defaultValue, AttachCallback(defaultValue, options, propertyChangedCallback))
		{
		}

		/// <summary>
		/// This private Ctor should only be used by AttachCallback.
		/// </summary>
		private DrawingPropertyMetadata(DrawingPropertyMetadataOptions options, object defaultValue)
			: base(defaultValue)
		{
		}

		/// <summary>
		/// Chain InternalCallback() to attach the instance of DrawingPropertyMetadata on property callback.
		/// In Silverlight, the property metadata is thrown away after setting. Use callback to remember it.
		/// </summary>
		private static PropertyChangedCallback AttachCallback(object defaultValue, DrawingPropertyMetadataOptions options, PropertyChangedCallback propertyChangedCallback)
		{
			DrawingPropertyMetadata drawingPropertyMetadata = new DrawingPropertyMetadata(options, defaultValue);
			drawingPropertyMetadata.options = options;
			drawingPropertyMetadata.propertyChangedCallback = propertyChangedCallback;
			return drawingPropertyMetadata.InternalCallback;
		}

		/// <summary>
		/// Before chaining the original callback, trigger DrawingPropertyChangedEvent.
		/// </summary>
		private void InternalCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (DrawingPropertyMetadata.DrawingPropertyChanged != null)
			{
				DrawingPropertyMetadata.DrawingPropertyChanged(sender, new DrawingPropertyChangedEventArgs
				{
					Metadata = this,
					IsAnimated = (sender.GetAnimationBaseValue(e.Property) != e.NewValue)
				});
			}
			if (propertyChangedCallback != null)
			{
				propertyChangedCallback(sender, e);
			}
		}

		static DrawingPropertyMetadata()
		{
			DrawingPropertyChanged += delegate (object sender, DrawingPropertyChangedEventArgs args)
			{
				if (sender is IShape shape && args.Metadata.AffectsRender)
				{
					InvalidateGeometryReasons invalidateGeometryReasons = InvalidateGeometryReasons.PropertyChanged;
					if (args.IsAnimated)
					{
						invalidateGeometryReasons |= InvalidateGeometryReasons.IsAnimated;
					}
					shape.InvalidateGeometry(invalidateGeometryReasons);
				}
			};
		}
	}
}
#endif