using System;
using System.Collections.Generic;
using System.Text;
#if MIGRATION
namespace Microsoft.Expression.Media
{
    internal interface IPolygonGeometrySourceParameters : IGeometrySourceParameters
    {
        double PointCount { get; }
        double InnerRadius { get; }
    }
}
#endif