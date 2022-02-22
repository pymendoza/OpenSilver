using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// This is ported from the WPF implementation of Vector.
	/// </summary>
	[DebuggerDisplay("({X}, {Y})")]
	internal struct Vector
	{
		public double X { get; set; }

		public double Y { get; set; }

		public double Length => Math.Sqrt(X * X + Y * Y);

		public double LengthSquared => X * X + Y * Y;

		public static bool operator ==(Vector vector1, Vector vector2)
		{
			if (vector1.X == vector2.X)
			{
				return vector1.Y == vector2.Y;
			}
			return false;
		}

		public static bool operator !=(Vector vector1, Vector vector2)
		{
			return !(vector1 == vector2);
		}

		public static bool Equals(Vector vector1, Vector vector2)
		{
			if (vector1.X.Equals(vector2.X))
			{
				return vector1.Y.Equals(vector2.Y);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Vector vector))
			{
				return false;
			}
			return Equals(this, vector);
		}

		public bool Equals(Vector value)
		{
			return Equals(this, value);
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public Vector(double x, double y)
		{
			this = default(Vector);
			X = x;
			Y = y;
		}

		public Vector(Point point)
		{
			this = default(Vector);
			X = point.X;
			Y = point.Y;
		}

		public void Normalize()
		{
			this /= Math.Max(Math.Abs(X), Math.Abs(Y));
			this /= Length;
		}

		public static double CrossProduct(Vector vector1, Vector vector2)
		{
			return vector1.X * vector2.Y - vector1.Y * vector2.X;
		}

		public static double AngleBetween(Vector vector1, Vector vector2)
		{
			double y = vector1.X * vector2.Y - vector2.X * vector1.Y;
			double x = vector1.X * vector2.X + vector1.Y * vector2.Y;
			return Math.Atan2(y, x) * (180.0 / Math.PI);
		}

		public static Vector operator -(Vector vector)
		{
			return new Vector(0.0 - vector.X, 0.0 - vector.Y);
		}

		public void Negate()
		{
			X = 0.0 - X;
			Y = 0.0 - Y;
		}

		public Size ToSize()
		{
			return new Size(Math.Abs(X), Math.Abs(Y));
		}

		public Point ToPoint()
		{
			return new Point(X, Y);
		}

		public static Vector operator +(Vector vector1, Vector vector2)
		{
			return new Vector(vector1.X + vector2.X, vector1.Y + vector2.Y);
		}

		public static Vector Add(Vector vector1, Vector vector2)
		{
			return new Vector(vector1.X + vector2.X, vector1.Y + vector2.Y);
		}

		public static Vector operator -(Vector vector1, Vector vector2)
		{
			return new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y);
		}

		public static Vector Subtract(Vector vector1, Vector vector2)
		{
			return new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y);
		}

		public static Point operator +(Vector vector, Point point)
		{
			return new Point(point.X + vector.X, point.Y + vector.Y);
		}

		public static Point Add(Vector vector, Point point)
		{
			return new Point(point.X + vector.X, point.Y + vector.Y);
		}

		public static Vector operator *(Vector vector, double scalar)
		{
			return new Vector(vector.X * scalar, vector.Y * scalar);
		}

		public static Vector Multiply(Vector vector, double scalar)
		{
			return new Vector(vector.X * scalar, vector.Y * scalar);
		}

		public static Vector operator *(double scalar, Vector vector)
		{
			return new Vector(vector.X * scalar, vector.Y * scalar);
		}

		public static Vector Multiply(double scalar, Vector vector)
		{
			return new Vector(vector.X * scalar, vector.Y * scalar);
		}

		public static Vector operator /(Vector vector, double scalar)
		{
			return vector * (1.0 / scalar);
		}

		public static Vector Divide(Vector vector, double scalar)
		{
			return vector * (1.0 / scalar);
		}

		public static Vector operator *(Vector vector, Matrix matrix)
		{
			Point point = matrix.Transform(new Point(vector.X, vector.Y));
			point.X -= matrix.OffsetX;
			point.Y -= matrix.OffsetY;
			return new Vector(point);
		}

		public static Vector Multiply(Vector vector, Matrix matrix)
		{
			Point point = matrix.Transform(new Point(vector.X, vector.Y));
			point.X -= matrix.OffsetX;
			point.Y -= matrix.OffsetY;
			return new Vector(point);
		}

		public static double operator *(Vector vector1, Vector vector2)
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		public static double Multiply(Vector vector1, Vector vector2)
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		public static double Determinant(Vector vector1, Vector vector2)
		{
			return vector1.X * vector2.Y - vector1.Y * vector2.X;
		}

		public static explicit operator Size(Vector vector)
		{
			return vector.ToSize();
		}

		public static explicit operator Point(Vector vector)
		{
			return vector.ToPoint();
		}

		public static Point operator +(Point point, Vector vector)
		{
			return new Point(point.X + vector.X, point.Y + vector.Y);
		}

		public static Point operator -(Point point, Vector vector)
		{
			return new Point(point.X - vector.X, point.Y - vector.Y);
		}
	}
}
#endif