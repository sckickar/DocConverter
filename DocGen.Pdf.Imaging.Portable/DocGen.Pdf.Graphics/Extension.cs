using SkiaSharp;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal static class Extension
{
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

	internal static SKColor GetSKColor(Color color)
	{
		return new SKColor(color.R, color.G, color.B, color.A);
	}

	internal static Color GetColor(SKColor color)
	{
		return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
	}
}
