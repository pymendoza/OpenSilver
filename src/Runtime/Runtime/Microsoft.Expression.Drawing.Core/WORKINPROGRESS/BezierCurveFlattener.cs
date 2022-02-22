using System;
using System.Collections.Generic;

#if MIGRATION
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// A utility class to flatten Bezier curves.
	/// </summary>
	internal static class BezierCurveFlattener
	{
		private class AdaptiveForwardDifferencingCubicFlattener
		{
			private double aX;

			private double aY;

			private double bX;

			private double bY;

			private double cX;

			private double cY;

			private double dX;

			private double dY;

			private int numSteps = 1;

			private double flatnessTolerance;

			private double distanceTolerance;

			private bool doParameters;

			private double parameter;

			private double dParameter = 1.0;

			internal AdaptiveForwardDifferencingCubicFlattener(Point[] controlPoints, double flatnessTolerance, double distanceTolerance, bool doParameters)
			{
				this.flatnessTolerance = 3.0 * flatnessTolerance;
				this.distanceTolerance = distanceTolerance;
				this.doParameters = doParameters;
				aX = 0.0 - controlPoints[0].X + 3.0 * (controlPoints[1].X - controlPoints[2].X) + controlPoints[3].X;
				aY = 0.0 - controlPoints[0].Y + 3.0 * (controlPoints[1].Y - controlPoints[2].Y) + controlPoints[3].Y;
				bX = 3.0 * (controlPoints[0].X - 2.0 * controlPoints[1].X + controlPoints[2].X);
				bY = 3.0 * (controlPoints[0].Y - 2.0 * controlPoints[1].Y + controlPoints[2].Y);
				cX = 3.0 * (0.0 - controlPoints[0].X + controlPoints[1].X);
				cY = 3.0 * (0.0 - controlPoints[0].Y + controlPoints[1].Y);
				dX = controlPoints[0].X;
				dY = controlPoints[0].Y;
			}

			private AdaptiveForwardDifferencingCubicFlattener()
			{
			}

			internal bool Next(ref Point p, ref double u)
			{
				while (MustSubdivide(flatnessTolerance))
				{
					HalveStepSize();
				}
				if ((numSteps & 1) == 0)
				{
					while (numSteps > 1 && !MustSubdivide(flatnessTolerance * 0.25))
					{
						DoubleStepSize();
					}
				}
				IncrementDifferencesAndParameter();
				p.X = dX;
				p.Y = dY;
				u = parameter;
				return numSteps != 0;
			}

			private void DoubleStepSize()
			{
				aX *= 8.0;
				aY *= 8.0;
				bX *= 4.0;
				bY *= 4.0;
				cX += cX;
				cY += cY;
				if (doParameters)
				{
					dParameter *= 2.0;
				}
				numSteps >>= 1;
			}

			private void HalveStepSize()
			{
				aX *= 0.125;
				aY *= 0.125;
				bX *= 0.25;
				bY *= 0.25;
				cX *= 0.5;
				cY *= 0.5;
				if (doParameters)
				{
					dParameter *= 0.5;
				}
				numSteps <<= 1;
			}

			private void IncrementDifferencesAndParameter()
			{
				dX = aX + bX + cX + dX;
				dY = aY + bY + cY + dY;
				cX = aX + aX + aX + bX + bX + cX;
				cY = aY + aY + aY + bY + bY + cY;
				bX = aX + aX + aX + bX;
				bY = aY + aY + aY + bY;
				numSteps--;
				parameter += dParameter;
			}

			private bool MustSubdivide(double flatnessTolerance)
			{
				double num = 0.0 - (aY + bY + cY);
				double num2 = aX + bX + cX;
				double num3 = Math.Abs(num) + Math.Abs(num2);
				if (num3 > distanceTolerance)
				{
					num3 *= flatnessTolerance;
					if (Math.Abs(cX * num + cY * num2) > num3)
					{
						return true;
					}
					if (!(Math.Abs((bX + cX + cX) * num + (bY + cY + cY) * num2) > num3))
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public const double StandardFlatteningTolerance = 0.25;

		/// <summary>
		/// Flattens a Bezier cubic curve and adds the resulting polyline to the third parameter.
		/// </summary>
		/// <param name="controlPoints">The four Bezier cubic control points.</param>
		/// <param name="errorTolerance">The maximum distance between two corresponding points on the true curve 
		/// and on the flattened polyline. Must be strictly positive.</param>
		/// <param name="resultPolyline">Where to add the flattened polyline.</param>
		/// <param name="skipFirstPoint">True to skip the first control point when adding the flattened polyline.
		/// <param name="resultParameters">Where to add the value of the Bezier curve parameter associated with 
		/// each of the polyline vertices.</param> 
		/// If <paramref name="resultPolyline" /> is empty, the first control point 
		/// and its associated parameter are always added.</param>
		public static void FlattenCubic(Point[] controlPoints, double errorTolerance, ICollection<Point> resultPolyline, bool skipFirstPoint, ICollection<double> resultParameters = null)
		{
			if (resultPolyline == null)
			{
				throw new ArgumentNullException("resultPolyline");
			}
			if (controlPoints == null)
			{
				throw new ArgumentNullException("controlPoints");
			}
			if (controlPoints.Length != 4)
			{
				throw new ArgumentOutOfRangeException("controlPoints");
			}
			EnsureErrorTolerance(ref errorTolerance);
			if (!skipFirstPoint)
			{
				resultPolyline.Add(controlPoints[0]);
				resultParameters?.Add(0.0);
			}
			if (IsCubicChordMonotone(controlPoints, errorTolerance * errorTolerance))
			{
				AdaptiveForwardDifferencingCubicFlattener adaptiveForwardDifferencingCubicFlattener = new AdaptiveForwardDifferencingCubicFlattener(controlPoints, errorTolerance, errorTolerance, doParameters: true);
				Point p = default(Point);
				double u = 0.0;
				while (adaptiveForwardDifferencingCubicFlattener.Next(ref p, ref u))
				{
					resultPolyline.Add(p);
					resultParameters?.Add(u);
				}
			}
			else
			{
				double x = controlPoints[3].X - controlPoints[2].X + controlPoints[1].X - controlPoints[0].X;
				double y = controlPoints[3].Y - controlPoints[2].Y + controlPoints[1].Y - controlPoints[0].Y;
				double num = 1.0 / errorTolerance;
				uint num2 = Log8UnsignedInt32((uint)(MathHelper.Hypotenuse(x, y) * num + 0.5));
				if (num2 != 0)
				{
					num2--;
				}
				if (num2 != 0)
				{
					DoCubicMidpointSubdivision(controlPoints, num2, 0.0, 1.0, 0.75 * num, resultPolyline, resultParameters);
				}
				else
				{
					DoCubicForwardDifferencing(controlPoints, 0.0, 1.0, 0.75 * num, resultPolyline, resultParameters);
				}
			}
			resultPolyline.Add(controlPoints[3]);
			resultParameters?.Add(1.0);
		}

		/// <summary>
		/// Flattens a Bezier quadratic curve and adds the resulting polyline to the third parameter.
		/// Uses degree elevation for Bezier curves to reuse the code for the cubic case.
		/// </summary>
		/// <param name="controlPoints">The three Bezier quadratic control points.</param>
		/// <param name="errorTolerance">The maximum distance between two corresponding points on the true curve 
		/// and on the flattened polyline. Must be strictly positive.</param>
		/// <param name="resultPolyline">Where to add the flattened polyline.</param>
		/// <param name="skipFirstPoint">Whether to skip the first control point when adding the flattened polyline. 
		/// <param name="resultParameters">Where to add the value of the Bezier curve parameter associated with
		/// each of the polyline vertices.</param>
		/// If <paramref name="resultPolyline" /> is empty, the first control point and 
		/// its associated parameter are always added.</param>
		public static void FlattenQuadratic(Point[] controlPoints, double errorTolerance, ICollection<Point> resultPolyline, bool skipFirstPoint, ICollection<double> resultParameters = null)
		{
			if (resultPolyline == null)
			{
				throw new ArgumentNullException("resultPolyline");
			}
			if (controlPoints == null)
			{
				throw new ArgumentNullException("controlPoints");
			}
			if (controlPoints.Length != 3)
			{
				throw new ArgumentOutOfRangeException("controlPoints");
			}
			EnsureErrorTolerance(ref errorTolerance);
			FlattenCubic(new Point[4]
			{
			controlPoints[0],
			GeometryHelper.Lerp(controlPoints[0], controlPoints[1], 2.0 / 3.0),
			GeometryHelper.Lerp(controlPoints[1], controlPoints[2], 1.0 / 3.0),
			controlPoints[2]
			}, errorTolerance, resultPolyline, skipFirstPoint, resultParameters);
		}

		private static void EnsureErrorTolerance(ref double errorTolerance)
		{
			if (errorTolerance <= 0.0)
			{
				errorTolerance = 0.25;
			}
		}

		private static uint Log8UnsignedInt32(uint i)
		{
			uint num = 0u;
			while (i != 0)
			{
				i >>= 3;
				num++;
			}
			return num;
		}

		private static uint Log4UnsignedInt32(uint i)
		{
			uint num = 0u;
			while (i != 0)
			{
				i >>= 2;
				num++;
			}
			return num;
		}

		private static uint Log4Double(double d)
		{
			uint num = 0u;
			while (d > 1.0)
			{
				d *= 0.25;
				num++;
			}
			return num;
		}

		private static void DoCubicMidpointSubdivision(Point[] controlPoints, uint depth, double leftParameter, double rightParameter, double inverseErrorTolerance, ICollection<Point> resultPolyline, ICollection<double> resultParameters)
		{
			Point[] array = new Point[4]
			{
			controlPoints[0],
			controlPoints[1],
			controlPoints[2],
			controlPoints[3]
			};
			Point[] array2 = new Point[4]
			{
			default(Point),
			default(Point),
			default(Point),
			array[3]
			};
			ref Point reference = ref array[3];
			reference = GeometryHelper.Midpoint(array[3], array[2]);
			ref Point reference2 = ref array[2];
			reference2 = GeometryHelper.Midpoint(array[2], array[1]);
			ref Point reference3 = ref array[1];
			reference3 = GeometryHelper.Midpoint(array[1], array[0]);
			ref Point reference4 = ref array2[2];
			reference4 = array[3];
			ref Point reference5 = ref array[3];
			reference5 = GeometryHelper.Midpoint(array[3], array[2]);
			ref Point reference6 = ref array[2];
			reference6 = GeometryHelper.Midpoint(array[2], array[1]);
			ref Point reference7 = ref array2[1];
			reference7 = array[3];
			ref Point reference8 = ref array[3];
			reference8 = GeometryHelper.Midpoint(array[3], array[2]);
			ref Point reference9 = ref array2[0];
			reference9 = array[3];
			depth--;
			double num = (leftParameter + rightParameter) * 0.5;
			if (depth != 0)
			{
				DoCubicMidpointSubdivision(array, depth, leftParameter, num, inverseErrorTolerance, resultPolyline, resultParameters);
				resultPolyline.Add(array2[0]);
				resultParameters?.Add(num);
				DoCubicMidpointSubdivision(array2, depth, num, rightParameter, inverseErrorTolerance, resultPolyline, resultParameters);
			}
			else
			{
				DoCubicForwardDifferencing(array, leftParameter, num, inverseErrorTolerance, resultPolyline, resultParameters);
				resultPolyline.Add(array2[0]);
				resultParameters?.Add(num);
				DoCubicForwardDifferencing(array2, num, rightParameter, inverseErrorTolerance, resultPolyline, resultParameters);
			}
		}

		private static void DoCubicForwardDifferencing(Point[] controlPoints, double leftParameter, double rightParameter, double inverseErrorTolerance, ICollection<Point> resultPolyline, ICollection<double> resultParameters)
		{
			double num = controlPoints[1].X - controlPoints[0].X;
			double num2 = controlPoints[1].Y - controlPoints[0].Y;
			double num3 = controlPoints[2].X - controlPoints[1].X;
			double num4 = controlPoints[2].Y - controlPoints[1].Y;
			double num5 = controlPoints[3].X - controlPoints[2].X;
			double num6 = controlPoints[3].Y - controlPoints[2].Y;
			double num7 = num3 - num;
			double num8 = num4 - num2;
			double num9 = num5 - num3;
			double num10 = num6 - num4;
			double num11 = num9 - num7;
			double num12 = num10 - num8;
			Vector vector = controlPoints[3].Subtract(controlPoints[0]);
			double length = vector.Length;
			double num13 = (MathHelper.IsVerySmall(length) ? Math.Max(0.0, Math.Max(GeometryHelper.Distance(controlPoints[1], controlPoints[0]), GeometryHelper.Distance(controlPoints[2], controlPoints[0]))) : Math.Max(0.0, Math.Max(Math.Abs((num7 * vector.Y - num8 * vector.X) / length), Math.Abs((num9 * vector.Y - num10 * vector.X) / length))));
			uint num14 = 0u;
			if (num13 > 0.0)
			{
				double num15 = num13 * inverseErrorTolerance;
				num14 = ((num15 < 2147483647.0) ? Log4UnsignedInt32((uint)(num15 + 0.5)) : Log4Double(num15));
			}
			int num16 = (int)(0 - num14);
			int num17 = num16 + num16;
			int exp = num17 + num16;
			double num18 = MathHelper.DoubleFromMantissaAndExponent(3.0 * num7, num17);
			double num19 = MathHelper.DoubleFromMantissaAndExponent(3.0 * num8, num17);
			double num20 = MathHelper.DoubleFromMantissaAndExponent(6.0 * num11, exp);
			double num21 = MathHelper.DoubleFromMantissaAndExponent(6.0 * num12, exp);
			double num22 = MathHelper.DoubleFromMantissaAndExponent(3.0 * num, num16) + num18 + 1.0 / 6.0 * num20;
			double num23 = MathHelper.DoubleFromMantissaAndExponent(3.0 * num2, num16) + num19 + 1.0 / 6.0 * num21;
			double num24 = 2.0 * num18 + num20;
			double num25 = 2.0 * num19 + num21;
			double num26 = controlPoints[0].X;
			double num27 = controlPoints[0].Y;
			Point item = new Point(0.0, 0.0);
			int num28 = 1 << (int)num14;
			double num29 = ((num28 > 0) ? ((rightParameter - leftParameter) / (double)num28) : 0.0);
			double num30 = leftParameter;
			for (int i = 1; i < num28; i++)
			{
				num26 += num22;
				num27 += num23;
				item.X = num26;
				item.Y = num27;
				resultPolyline.Add(item);
				num30 += num29;
				resultParameters?.Add(num30);
				num22 += num24;
				num23 += num25;
				num24 += num20;
				num25 += num21;
			}
		}

		private static bool IsCubicChordMonotone(Point[] controlPoints, double squaredTolerance)
		{
			double num = GeometryHelper.SquaredDistance(controlPoints[0], controlPoints[3]);
			if (num > squaredTolerance)
			{
				Vector lhs = controlPoints[3].Subtract(controlPoints[0]);
				Vector rhs = controlPoints[1].Subtract(controlPoints[0]);
				double num2 = GeometryHelper.Dot(lhs, rhs);
				if (num2 < 0.0 || num2 > num)
				{
					return false;
				}
				Vector rhs2 = controlPoints[2].Subtract(controlPoints[0]);
				double num3 = GeometryHelper.Dot(lhs, rhs2);
				if (num3 < 0.0 || num3 > num)
				{
					return false;
				}
				if (!(num2 <= num3))
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}
}
#endif