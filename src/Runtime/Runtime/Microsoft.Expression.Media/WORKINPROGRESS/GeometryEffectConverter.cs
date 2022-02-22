using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Expression.Media;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	/// <summary>
	/// Provides the conversion between string and geometry effects.
	/// </summary>
	/// <remarks>
	/// This class enables the brief syntax in XAML like <code>GeometryEffect="Sketch"</code>.
	/// Creates a clone of the instance of the geometry effect so it can be used as a resource.
	/// </remarks>
	public sealed class GeometryEffectConverter : TypeConverter
	{
		/// <summary>
		/// Builds a preset list of supported geometry effects.
		/// </summary>
		private static Dictionary<string, GeometryEffect> registeredEffects = new Dictionary<string, GeometryEffect>
		{
			{
				"None",
				GeometryEffect.DefaultGeometryEffect
			},
			{
				"Sketch",
				new SketchGeometryEffect()
			}
		};

		/// <summary>
		/// A GeometryEffect that can be converted from a string type.
		/// </summary>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return typeof(string).IsAssignableFrom(sourceType);
		}

		/// <summary>
		/// A GeometryEffect that can be converted to a string type.
		/// </summary>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return typeof(string).IsAssignableFrom(destinationType);
		}

		/// <summary>
		/// Converts a string to a geometry effect. The fallback value is null.
		/// </summary>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string key && registeredEffects.TryGetValue(key, out var value2))
			{
				return value2.CloneCurrentValue();
			}
			return null;
		}

		/// <summary>
		/// Converts a geometry effect into a string.  The fallback value is null.
		/// </summary>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (typeof(string).IsAssignableFrom(destinationType))
			{
				foreach (KeyValuePair<string, GeometryEffect> registeredEffect in registeredEffects)
				{
					if ((registeredEffect.Value == null) ? (value == null) : registeredEffect.Value.Equals(value as GeometryEffect))
					{
						return registeredEffect.Key;
					}
				}
			}
			return null;
		}
	}
}
#endif