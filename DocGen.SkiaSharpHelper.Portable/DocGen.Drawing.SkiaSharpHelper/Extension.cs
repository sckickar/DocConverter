using System;
using System.Globalization;
using System.IO;
using SkiaSharp;
using DocGen.Office;

namespace DocGen.Drawing.SkiaSharpHelper;

internal static class Extension
{
	internal static double DegreesToAngleConversion(double degrees)
	{
		return Math.PI * degrees / 180.0;
	}

	internal static double GetAscent(this Font font, string text)
	{
		FontExtension fontExtension = new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Pixel);
		SKRect bounds = default(SKRect);
		fontExtension.MeasureText(text + "lg", ref bounds);
		return (double)(0f - bounds.Top) + (double)(fontExtension.FontSpacing + bounds.Top) / 2.0;
	}

	internal static void Multiply(this Matrix matrix, Matrix target, MatrixOrder order)
	{
		SKMatrix target2 = target.GetSKMatrix();
		SKMatrix sKMatrix = matrix.GetSKMatrix();
		if (order == MatrixOrder.Append)
		{
			SKMatrix.PreConcat(ref target2, sKMatrix);
		}
		else
		{
			SKMatrix.PostConcat(ref target2, sKMatrix);
		}
		matrix.SkMatrix = target2;
		matrix.Elements = new float[6] { target2.ScaleX, target2.SkewY, target2.SkewX, target2.ScaleY, target2.TransX, target2.TransY };
	}

	public static void RotateAt(this Matrix matrix, int angle, PointF pointF)
	{
		matrix.RotateAt((float)angle, pointF);
	}

	internal static void RotateAt(this Matrix matrix, float angle, PointF pointF)
	{
		matrix.RotateAt(angle, pointF, MatrixOrder.Append);
	}

	internal static void RotateAt(this Matrix matrix, float angle, PointF pointF, MatrixOrder order)
	{
		SKMatrix tempmatrix = SKMatrix.MakeRotationDegrees(angle, pointF.X, pointF.Y);
		SetSkMatrix(matrix, tempmatrix, order);
	}

	internal static void Rotate(this Matrix matrix, int angle)
	{
		matrix.Rotate((float)angle);
	}

	internal static void Rotate(this Matrix matrix, int angle, MatrixOrder order)
	{
		matrix.Rotate((float)angle, order);
	}

	internal static Matrix MakeRotationDegrees(float angle, float x, float y)
	{
		return new Matrix(SKMatrix.MakeRotationDegrees(angle, x, y).Values);
	}

	internal static float[] GetMatrixValues(float[] elements, float rotationAngle, PointF translatePoints, float scaleFactor)
	{
		SKMatrix matrix = SKMatrix.MakeIdentity();
		matrix.Values = GetSkiaMatrix(elements);
		SKMatrix.RotateDegrees(ref matrix, rotationAngle);
		matrix.SetScaleTranslate(scaleFactor, 1f, translatePoints.X, translatePoints.Y);
		return matrix.Values;
	}

	private static float[] GetSkiaMatrix(float[] drawingMatrix)
	{
		return new float[9]
		{
			drawingMatrix[0],
			drawingMatrix[2],
			drawingMatrix[4],
			drawingMatrix[1],
			drawingMatrix[3],
			drawingMatrix[5],
			0f,
			0f,
			1f
		};
	}

	internal static void Rotate(this Matrix matrix, float angle)
	{
		matrix.Rotate(angle, MatrixOrder.Append);
	}

	internal static void Rotate(this Matrix matrix, float angle, MatrixOrder order)
	{
		SKMatrix tempmatrix = SKMatrix.MakeRotationDegrees(angle);
		SetSkMatrix(matrix, tempmatrix, order);
	}

	internal static void SetSkMatrix(Matrix matrix, SKMatrix tempmatrix, MatrixOrder matrixOrder)
	{
		SKMatrix sKMatrix = matrix.GetSKMatrix();
		if (matrixOrder == MatrixOrder.Append)
		{
			SKMatrix.PreConcat(ref tempmatrix, sKMatrix);
		}
		else
		{
			SKMatrix.PostConcat(ref tempmatrix, sKMatrix);
		}
		matrix.SkMatrix = tempmatrix;
		matrix.Elements = new float[6] { tempmatrix.ScaleX, tempmatrix.SkewY, tempmatrix.SkewX, tempmatrix.ScaleY, tempmatrix.TransX, tempmatrix.TransY };
	}

	internal static void Translate(this Matrix matrix, float x, float y, MatrixOrder matrixOrder)
	{
		SKMatrix tempmatrix = SKMatrix.MakeTranslation(x, y);
		SetSkMatrix(matrix, tempmatrix, matrixOrder);
	}

	internal static void Scale(this Matrix matrix, float x, float y)
	{
		matrix.Scale(x, y, MatrixOrder.Prepend);
	}

	internal static void Scale(this Matrix matrix, float x, float y, MatrixOrder order)
	{
		SKMatrix tempmatrix = SKMatrix.MakeScale(x, y);
		SetSkMatrix(matrix, tempmatrix, order);
	}

	internal static SKMatrix GetSKMatrix()
	{
		return new RenderHelper().GetGraphics().SkSurface.Canvas.TotalMatrix;
	}

	internal static SKMatrix GetSKMatrix(this Matrix matrix)
	{
		if (matrix.SkMatrix == null)
		{
			SKMatrix sKMatrix = default(SKMatrix);
			sKMatrix.ScaleX = matrix.Elements[0];
			sKMatrix.SkewY = matrix.Elements[1];
			sKMatrix.SkewX = matrix.Elements[2];
			sKMatrix.ScaleY = matrix.Elements[3];
			sKMatrix.TransX = matrix.Elements[4];
			sKMatrix.TransY = matrix.Elements[5];
			sKMatrix.Persp2 = 1f;
			matrix.SkMatrix = sKMatrix;
		}
		return (SKMatrix)matrix.SkMatrix;
	}

	internal static string ToShortTimeString(this DateTime dateTime)
	{
		return dateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
	}

	internal static string ToShortDateString(this DateTime dateTime)
	{
		return dateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
	}

	internal static SKColor GetSKColor(Color color)
	{
		return new SKColor(color.R, color.G, color.B, color.A);
	}

	internal static SKColor[] GetArrayOfSKColors(Color[] colors)
	{
		SKColor[] array = new SKColor[colors.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			Color color = colors[i];
			array[i] = new SKColor(color.R, color.G, color.B, color.A);
		}
		return array;
	}

	internal static Color[] GetReversedColors(Color[] colors)
	{
		Color[] array = new Color[colors.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			array[i] = colors[^(i + 1)];
		}
		return array;
	}

	internal static Color GetColor(SKColor color)
	{
		return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
	}

	internal static float GetHeight(this Font font)
	{
		return new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Pixel).FontSpacing;
	}

	internal static float GetHeight(this Font font, Graphics g)
	{
		return new FontExtension(font.Name, font.Size, font.Style, GraphicsUnit.Pixel).FontSpacing;
	}

	internal static SKPoint[] GetArrayOfSKPoints(this PointF[] points)
	{
		SKPoint[] array = new SKPoint[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = new SKPoint(points[i].X, points[i].Y);
		}
		return array;
	}

	internal static SKPoint[] GetArrayOfSKPoints(this Point[] points)
	{
		SKPoint[] array = new SKPoint[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = new SKPoint(points[i].X, points[i].Y);
		}
		return array;
	}

	internal static PointF GetPointF(this SKPoint point)
	{
		return new PointF(point.X, point.Y);
	}

	internal static Point GetPoint(this SKPoint point)
	{
		return new Point((int)point.X, (int)point.Y);
	}

	internal static SKPoint GetSKPoint(this PointF point)
	{
		return new SKPoint(point.X, point.Y);
	}

	internal static SKPoint GetSKPoint(this Point point)
	{
		return new SKPoint(point.X, point.Y);
	}

	internal static PointF[] GetArrayOfPoints(this Point[] points)
	{
		PointF[] array = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = points[i];
		}
		return array;
	}

	internal static string GetUnicodeFamilyName(string text, string fontName)
	{
		return SKFontManager.Default.MatchCharacter(fontName, text[0])?.FamilyName;
	}

	internal static Stream GetUnicodeFontStream(string text, string fontName)
	{
		SKFontManager @default = SKFontManager.Default;
		@default.MatchCharacter(fontName, text[0]);
		using SKTypeface sKTypeface = @default.MatchCharacter(text[0]);
		if (sKTypeface != null)
		{
			using SKStreamAsset sKStreamAsset = sKTypeface.OpenStream();
			if (sKStreamAsset != null && sKStreamAsset.Length > 0)
			{
				byte[] buffer = new byte[sKStreamAsset.Length - 1];
				sKStreamAsset.Read(buffer, sKStreamAsset.Length);
				return new MemoryStream(buffer);
			}
		}
		return null;
	}

	internal static Stream GetBitmapStream(SKBitmap sKBitmap)
	{
		return SKImage.FromBitmap(sKBitmap).Encode(SKEncodedImageFormat.Png, 100).AsStream();
	}

	internal static bool IsHarfBuzzSupportedScript(FontScriptType scriptType)
	{
		if (scriptType != 0)
		{
			if (!FontScriptType.Arabic.HasFlag(scriptType) && !FontScriptType.Chinese.HasFlag(scriptType) && !FontScriptType.Hebrew.HasFlag(scriptType) && !FontScriptType.Hindi.HasFlag(scriptType) && !FontScriptType.Japanese.HasFlag(scriptType))
			{
				return FontScriptType.Korean.HasFlag(scriptType);
			}
			return true;
		}
		return false;
	}
}
