﻿using System;
#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// Helper class that provides static properties and methods related to floating point arithmetic.
	/// </summary>
	internal static class MathHelper
	{
		/// <summary>
		/// The minimum distance to consider that two values are same.
		/// Note: internal floating point in MIL/SL is float, not double.
		/// </summary>
		public const double Epsilon = 1E-06;

		/// <summary>
		/// The value of the angle of a full circle.
		/// </summary>
		public const double TwoPI = Math.PI * 2.0;

		/// <summary>
		/// The inner radius for a pentagram polygon shape, at precision of three digits in percentage.
		/// (1 - Sin36 * Sin72 / Sin54) / (Cos36) ^ 2, which is 0.47210998990512996761913067272407
		/// </summary>
		public const double PentagramInnerRadius = 0.47211;

		/// <summary>
		/// Determines whether a <c>System.Double</c> value is small enough to be considered
		/// equivalent to zero.
		/// </summary>
		/// <param name="value"></param>
		/// <returns><c>True</c> if value is smaller than <c>DoubleTolerance</c>;
		/// otherwise, <c>False</c>.</returns>
		public static bool IsVerySmall(double value)
		{
			return Math.Abs(value) < 1E-06;
		}

		public static bool AreClose(double value1, double value2)
		{
			if (value1 == value2)
			{
				return true;
			}
			return IsVerySmall(value1 - value2);
		}

		public static bool GreaterThan(double value1, double value2)
		{
			if (value1 > value2)
			{
				return !AreClose(value1, value2);
			}
			return false;
		}

		public static bool GreaterThanOrClose(double value1, double value2)
		{
			if (value1 <= value2)
			{
				return AreClose(value1, value2);
			}
			return true;
		}

		public static bool LessThan(double value1, double value2)
		{
			if (value1 < value2)
			{
				return !AreClose(value1, value2);
			}
			return false;
		}

		public static bool LessThanOrClose(double value1, double value2)
		{
			if (value1 >= value2)
			{
				return AreClose(value1, value2);
			}
			return true;
		}

		public static double SafeDivide(double lhs, double rhs, double fallback)
		{
			if (!IsVerySmall(rhs))
			{
				return lhs / rhs;
			}
			return fallback;
		}

		public static double Lerp(double x, double y, double alpha)
		{
			return x * (1.0 - alpha) + y * alpha;
		}

		/// <summary>
		/// Returns the value that's within the given range.
		/// A given min/max that is null equals no limit.
		/// </summary>
		public static double EnsureRange(double value, double? min, double? max)
		{
			if (min.HasValue && value < min.Value)
			{
				return min.Value;
			}
			if (max.HasValue && value > max.Value)
			{
				return max.Value;
			}
			return value;
		}

		/// <summary>
		/// Computes the Euclidean norm of the vector (x, y).
		/// </summary>
		/// <param name="x">The first component.</param>
		/// <param name="y">The second component.</param>
		/// <returns>The Euclidean norm of the vector (x, y).</returns>
		public static double Hypotenuse(double x, double y)
		{
			return Math.Sqrt(x * x + y * y);
		}

		/// <summary>
		/// Computes a real number from the mantissa and exponent.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="exp"></param>
		/// <returns>The value of x * 2^exp if successful.</returns>
		public static double DoubleFromMantissaAndExponent(double x, int exp)
		{
			return x * Math.Pow(2.0, exp);
		}

		/// <summary>
		/// Tests a double.
		/// </summary>
		/// <param name="x">The double to test.</param>
		/// <returns><c>True</c> if x is not a NaN and is not equal to plus or minus infinity;
		/// otherwise, <c>False</c>.</returns>
		public static bool IsFiniteDouble(double x)
		{
			if (!double.IsInfinity(x))
			{
				return !double.IsNaN(x);
			}
			return false;
		}
	}
}
#endif