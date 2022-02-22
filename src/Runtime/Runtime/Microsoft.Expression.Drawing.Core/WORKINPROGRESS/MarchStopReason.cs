using System;
using System.Collections.Generic;
using System.Text;
#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	internal enum MarchStopReason
	{
		CompleteStep,
		CompletePolyline,
		CornerPoint
	}
}
#endif