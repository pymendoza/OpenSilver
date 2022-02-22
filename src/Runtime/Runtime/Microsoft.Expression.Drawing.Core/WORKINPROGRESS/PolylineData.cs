using System;
using System.Collections.Generic;
using System.Windows;

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// Represents a polyline with a list of connecting points.
	/// A closed polygon is represented by repeating the first point at the end.
	/// The differences, normals, angles, and lengths are computed on demand.
	/// </summary>
	internal class PolylineData
	{
		private IList<Point> points;

		private IList<Vector> normals;

		private IList<double> angles;

		private IList<double> lengths;

		private IList<double> accumulates;

		private double? totalLength;

		/// <summary>
		/// The polyline is closed when the first and last points are repeated.
		/// </summary>
		public bool IsClosed => points[0] == points.Last();

		/// <summary>
		/// The count of points in this polyline.
		/// </summary>
		public int Count => points.Count;

		/// <summary>
		/// The total arc length of this polyline.
		/// </summary>
		public double TotalLength => totalLength ?? ComputeTotalLength();

		/// <summary>
		/// The point array of this polyline.
		/// </summary>
		public IList<Point> Points => points;

		/// <summary>
		/// The length between line segments, Points[i] to Points[i+1].
		/// </summary>
		public IList<double> Lengths => lengths ?? ComputeLengths();

		/// <summary>
		/// The list of normal vectors for each segment.
		/// Normals[i] is the normal of segment p[i] to p[i + 1].
		/// Normals[N-1] == Normals[N-2].
		/// </summary>
		public IList<Vector> Normals => normals ?? ComputeNormals();

		/// <summary>
		/// The list of Cos(angle) between two line segments on point p[i].
		/// Note: The value is cos(angle) = Dot(u, v). Not in degrees.
		/// </summary>
		public IList<double> Angles => angles ?? ComputeAngles();

		/// <summary>
		/// The list of accumulated length from points[i] to points[0].
		/// </summary>
		public IList<double> AccumulatedLength => accumulates ?? ComputeAccumulatedLength();

		/// <summary>
		/// Constructs a polyline with two or more points.
		/// </summary>
		/// <param name="points"></param>
		public PolylineData(IList<Point> points)
		{
			if (points == null)
			{
				throw new ArgumentNullException("points");
			}
			if (points.Count <= 1)
			{
				throw new ArgumentOutOfRangeException("points");
			}
			this.points = points;
		}

		/// The forward difference vector of polyline.
		/// Points[i] + Differences[i] = Points[i+1]
		public Vector Difference(int index)
		{
			int index2 = (index + 1) % Count;
			return points[index2].Subtract(points[index]);
		}

		/// <summary>
		/// Compute the normal vector of given location (lerp(index, index+1, fraction).
		/// If the location is within range of cornerRadius, interpolate the normal direction.
		/// </summary>
		/// <param name="cornerRadius">The range of normal smoothless.  If zero, no smoothness and return the exact normal on index.</param>
		public Vector SmoothNormal(int index, double fraction, double cornerRadius)
		{
			if (cornerRadius > 0.0)
			{
				double num = Lengths[index];
				if (MathHelper.IsVerySmall(num))
				{
					int num2 = index - 1;
					if (num2 < 0 && IsClosed)
					{
						num2 = Count - 1;
					}
					int num3 = index + 1;
					if (IsClosed && num3 >= Count - 1)
					{
						num3 = 0;
					}
					if (num2 < 0 || num3 >= Count)
					{
						return Normals[index];
					}
					return GeometryHelper.Lerp(Normals[num3], Normals[num2], 0.5).Normalized();
				}
				double num4 = Math.Min(cornerRadius / num, 0.5);
				if (fraction <= num4)
				{
					int num5 = index - 1;
					if (IsClosed && num5 == -1)
					{
						num5 = Count - 1;
					}
					if (num5 >= 0)
					{
						double alpha = (num4 - fraction) / (2.0 * num4);
						return GeometryHelper.Lerp(Normals[index], Normals[num5], alpha).Normalized();
					}
				}
				else if (fraction >= 1.0 - num4)
				{
					int num6 = index + 1;
					if (IsClosed && num6 >= Count - 1)
					{
						num6 = 0;
					}
					if (num6 < Count)
					{
						double alpha2 = (fraction + num4 - 1.0) / (2.0 * num4);
						return GeometryHelper.Lerp(Normals[index], Normals[num6], alpha2).Normalized();
					}
				}
			}
			return Normals[index];
		}

		private IList<double> ComputeLengths()
		{
			lengths = new double[Count];
			for (int i = 0; i < Count; i++)
			{
				lengths[i] = Difference(i).Length;
			}
			return lengths;
		}

		private IList<Vector> ComputeNormals()
		{
			normals = new Vector[points.Count];
			for (int i = 0; i < Count - 1; i++)
			{
				normals[i] = GeometryHelper.Normal(points[i], points[i + 1]);
			}
			normals[Count - 1] = normals[Count - 2];
			return normals;
		}

		private IList<double> ComputeAngles()
		{
			angles = new double[Count];
			for (int i = 1; i < Count - 1; i++)
			{
				angles[i] = 0.0 - GeometryHelper.Dot(Normals[i - 1], Normals[i]);
			}
			if (!IsClosed)
			{
				double num3 = (angles[0] = (angles[Count - 1] = 1.0));
			}
			else
			{
				double num4 = 0.0 - GeometryHelper.Dot(Normals[0], Normals[Count - 2]);
				double num7 = (angles[0] = (angles[Count - 1] = num4));
			}
			return angles;
		}

		private IList<double> ComputeAccumulatedLength()
		{
			accumulates = new double[Count];
			accumulates[0] = 0.0;
			for (int i = 1; i < Count; i++)
			{
				accumulates[i] = accumulates[i - 1] + Lengths[i - 1];
			}
			totalLength = accumulates.Last();
			return accumulates;
		}

		private double ComputeTotalLength()
		{
			ComputeAccumulatedLength();
			return totalLength.Value;
		}
	}
}
#endif