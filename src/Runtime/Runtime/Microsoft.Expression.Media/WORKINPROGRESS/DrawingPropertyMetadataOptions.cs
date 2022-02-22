using System;
using System.Collections.Generic;
using System.Text;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	[Flags]
	internal enum DrawingPropertyMetadataOptions
	{
		None = 0,
		AffectsMeasure = 1,
		AffectsRender = 0x10
	}
}
#endif