using System;
using System.Collections.Generic;
using System.Text;
#if MIGRATION
namespace Microsoft.Expression.Media
{
	internal class DrawingPropertyChangedEventArgs : EventArgs
	{
		public DrawingPropertyMetadata Metadata { get; set; }

		public bool IsAnimated { get; set; }
	}
}
#endif