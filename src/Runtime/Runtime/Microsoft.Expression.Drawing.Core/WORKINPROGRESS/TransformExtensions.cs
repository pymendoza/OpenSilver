using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

#if MIGRATION
namespace Microsoft.Expression.Drawing.Core
{
	internal static class TransformExtensions
	{
		public static Point TransformPoint(this IEnumerable<GeneralTransform> transforms, Point point)
		{
			if (transforms == null)
			{
				return point;
			}
			foreach (GeneralTransform transform in transforms)
			{
				point = GeometryHelper.SafeTransform(transform, point);
			}
			return point;
		}

		public static Transform CloneTransform(this Transform transform)
		{
			if (transform == null)
			{
				return null;
			}
			if (transform is TranslateTransform translateTransform)
			{
				TranslateTransform translateTransform2 = new TranslateTransform();
				translateTransform2.X = translateTransform.X;
				translateTransform2.Y = translateTransform.Y;
				return translateTransform2;
			}
			if (transform is RotateTransform rotateTransform)
			{
				RotateTransform rotateTransform2 = new RotateTransform();
				rotateTransform2.Angle = rotateTransform.Angle;
				rotateTransform2.CenterX = rotateTransform.CenterX;
				rotateTransform2.CenterY = rotateTransform.CenterY;
				return rotateTransform2;
			}
			if (transform is ScaleTransform scaleTransform)
			{
				ScaleTransform scaleTransform2 = new ScaleTransform();
				scaleTransform2.ScaleX = scaleTransform.ScaleX;
				scaleTransform2.ScaleY = scaleTransform.ScaleY;
				scaleTransform2.CenterX = scaleTransform.CenterX;
				scaleTransform2.CenterY = scaleTransform.CenterY;
				return scaleTransform2;
			}
			if (transform is SkewTransform skewTransform)
			{
				SkewTransform skewTransform2 = new SkewTransform();
				skewTransform2.AngleX = skewTransform.AngleX;
				skewTransform2.AngleY = skewTransform.AngleY;
				skewTransform2.CenterX = skewTransform.CenterX;
				skewTransform2.CenterY = skewTransform.CenterY;
				return skewTransform2;
			}
			if (transform is CompositeTransform compositeTransform)
			{
				CompositeTransform compositeTransform2 = new CompositeTransform();
				compositeTransform2.CenterX = compositeTransform.CenterX;
				compositeTransform2.CenterY = compositeTransform.CenterY;
				compositeTransform2.Rotation = compositeTransform.Rotation;
				compositeTransform2.ScaleX = compositeTransform.ScaleX;
				compositeTransform2.ScaleY = compositeTransform.ScaleY;
				compositeTransform2.SkewX = compositeTransform.SkewX;
				compositeTransform2.SkewY = compositeTransform.SkewY;
				compositeTransform2.TranslateX = compositeTransform.TranslateX;
				compositeTransform2.TranslateY = compositeTransform.TranslateY;
				return compositeTransform2;
			}
			if (transform is MatrixTransform matrixTransform)
			{
				MatrixTransform matrixTransform2 = new MatrixTransform();
				matrixTransform2.Matrix = matrixTransform.Matrix;
				return matrixTransform2;
			}
			if (transform is TransformGroup transformGroup)
			{
				TransformGroup transformGroup2 = new TransformGroup();
				{
					foreach (Transform child in transformGroup.Children)
					{
						transformGroup2.Children.Add(child.CloneTransform());
					}
					return transformGroup2;
				}
			}
			return transform.DeepCopy();
		}

		/// <summary>
		/// Compares two transforms for an exact match. Transforms with the same value but different structure (e.g. Translate(0,0) and Rotate(0) are not considered equivalent).
		/// </summary>
		/// <param name="firstTransform">The first transform.</param>
		/// <param name="secondTransform">The second transform.</param>
		/// <returns></returns>
		public static bool TransformEquals(this Transform firstTransform, Transform secondTransform)
		{
			if (firstTransform == null && secondTransform == null)
			{
				return true;
			}
			if (firstTransform == null || secondTransform == null)
			{
				return false;
			}
			if (firstTransform == secondTransform)
			{
				return true;
			}
			TranslateTransform translateTransform = firstTransform as TranslateTransform;
			TranslateTransform translateTransform2 = secondTransform as TranslateTransform;
			if (translateTransform != null && translateTransform2 != null)
			{
				return TranslateTransformEquals(translateTransform, translateTransform2);
			}
			RotateTransform rotateTransform = firstTransform as RotateTransform;
			RotateTransform rotateTransform2 = secondTransform as RotateTransform;
			if (rotateTransform != null && rotateTransform2 != null)
			{
				return RotateTransformEquals(rotateTransform, rotateTransform2);
			}
			ScaleTransform scaleTransform = firstTransform as ScaleTransform;
			ScaleTransform scaleTransform2 = secondTransform as ScaleTransform;
			if (scaleTransform != null && scaleTransform2 != null)
			{
				return ScaleTransformEquals(scaleTransform, scaleTransform2);
			}
			SkewTransform skewTransform = firstTransform as SkewTransform;
			SkewTransform skewTransform2 = secondTransform as SkewTransform;
			if (skewTransform != null && skewTransform2 != null)
			{
				return SkewTransformEquals(skewTransform, skewTransform2);
			}
			MatrixTransform matrixTransform = firstTransform as MatrixTransform;
			MatrixTransform matrixTransform2 = secondTransform as MatrixTransform;
			if (matrixTransform != null && matrixTransform2 != null)
			{
				return MatrixTransformEquals(matrixTransform, matrixTransform2);
			}
			TransformGroup transformGroup = firstTransform as TransformGroup;
			TransformGroup transformGroup2 = secondTransform as TransformGroup;
			if (transformGroup != null && transformGroup2 != null)
			{
				return TransformGroupEquals(transformGroup, transformGroup2);
			}
			CompositeTransform compositeTransform = firstTransform as CompositeTransform;
			CompositeTransform compositeTransform2 = secondTransform as CompositeTransform;
			if (compositeTransform != null && compositeTransform2 != null)
			{
				return CompositeTransformEquals(compositeTransform, compositeTransform2);
			}
			TransformGroup transformGroup3 = new TransformGroup();
			transformGroup3.Children.Add(firstTransform);
			TransformGroup transformGroup4 = new TransformGroup();
			transformGroup4.Children.Add(secondTransform);
			return transformGroup3.ValueInternal == transformGroup4.ValueInternal;
		}

		private static bool TranslateTransformEquals(TranslateTransform firstTransform, TranslateTransform secondTransform)
		{
			if (firstTransform.X == secondTransform.X)
			{
				return firstTransform.Y == secondTransform.Y;
			}
			return false;
		}

		private static bool RotateTransformEquals(RotateTransform firstTransform, RotateTransform secondTransform)
		{
			if (firstTransform.Angle == secondTransform.Angle && firstTransform.CenterX == secondTransform.CenterX)
			{
				return firstTransform.CenterY == secondTransform.CenterY;
			}
			return false;
		}

		private static bool ScaleTransformEquals(ScaleTransform firstTransform, ScaleTransform secondTransform)
		{
			if (firstTransform.ScaleX == secondTransform.ScaleX && firstTransform.ScaleY == secondTransform.ScaleY && firstTransform.CenterX == secondTransform.CenterX)
			{
				return firstTransform.CenterY == secondTransform.CenterY;
			}
			return false;
		}

		private static bool SkewTransformEquals(SkewTransform firstTransform, SkewTransform secondTransform)
		{
			if (firstTransform.AngleX == secondTransform.AngleX && firstTransform.AngleY == secondTransform.AngleY && firstTransform.CenterX == secondTransform.CenterX)
			{
				return firstTransform.CenterY == secondTransform.CenterY;
			}
			return false;
		}

		private static bool CompositeTransformEquals(CompositeTransform firstTransform, CompositeTransform secondTransform)
		{
			if (firstTransform.CenterX == secondTransform.CenterX && firstTransform.CenterY == secondTransform.CenterY && firstTransform.Rotation == secondTransform.Rotation && firstTransform.ScaleX == secondTransform.ScaleX && firstTransform.ScaleY == secondTransform.ScaleY && firstTransform.SkewX == secondTransform.SkewX && firstTransform.SkewY == secondTransform.SkewY && firstTransform.TranslateX == secondTransform.TranslateX)
			{
				return firstTransform.TranslateY == secondTransform.TranslateY;
			}
			return false;
		}

		private static bool TransformGroupEquals(TransformGroup firstTransform, TransformGroup secondTransform)
		{
			if (firstTransform.Children.Count != secondTransform.Children.Count)
			{
				return false;
			}
			for (int i = 0; i < firstTransform.Children.Count; i++)
			{
				if (!firstTransform.Children[i].TransformEquals(secondTransform.Children[i]))
				{
					return false;
				}
			}
			return true;
		}

		private static bool MatrixTransformEquals(MatrixTransform firstTransform, MatrixTransform secondTransform)
		{
			return firstTransform.Matrix == secondTransform.Matrix;
		}
	}

}
#endif