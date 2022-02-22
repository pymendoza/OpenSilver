using System;
#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	/// <summary>
	/// A random generator that supports uniform and Gaussian distributions.
	/// </summary>
	internal class RandomEngine
	{
		private Random random;

		private double? anotherSample;

		public RandomEngine(long seed)
		{
			Initialize(seed);
		}

		public double NextGaussian(double mean, double variance)
		{
			return Gaussian() * variance + mean;
		}

		public double NextUniform(double min, double max)
		{
			return Uniform() * (max - min) + min;
		}

		private void Initialize(long seed)
		{
			random = new Random((int)seed);
		}

		private double Uniform()
		{
			return random.NextDouble();
		}

		/// <summary>
		/// Generates a pair of independent, standard, normally distributed random numbers,
		/// zero expectation, unit variance, using polar form of the Box-Muller transformation.
		/// </summary>
		private double Gaussian()
		{
			if (anotherSample.HasValue)
			{
				double value = anotherSample.Value;
				anotherSample = null;
				return value;
			}
			double num;
			double num2;
			double num3;
			do
			{
				num = 2.0 * Uniform() - 1.0;
				num2 = 2.0 * Uniform() - 1.0;
				num3 = num * num + num2 * num2;
			}
			while (num3 >= 1.0);
			double num4 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
			anotherSample = num * num4;
			return num2 * num4;
		}
	}
}
#endif