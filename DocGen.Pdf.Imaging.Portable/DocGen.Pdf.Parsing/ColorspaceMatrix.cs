using System;
using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class ColorspaceMatrix
{
	private double xa;

	private double ya;

	private double za;

	private double xb;

	private double yb;

	private double zb;

	private double xc;

	private double yc;

	private double zc;

	internal double Xa => xa;

	internal double Ya => ya;

	internal double Za => za;

	internal double Xb => xb;

	internal double Yb => yb;

	internal double Zb => zb;

	internal double Xc => xc;

	internal double Yc => yc;

	internal double Zc => zc;

	internal bool IsIdentity
	{
		get
		{
			if (xa == 1.0 && ya == 0.0 && za == 0.0 && xb == 0.0 && yb == 1.0 && zb == 0.0 && xc == 0.0 && yc == 0.0)
			{
				return zc == 1.0;
			}
			return false;
		}
	}

	internal ColorspaceMatrix(PdfArray array)
	{
		if (array.Count != 9)
		{
			throw new InvalidOperationException();
		}
		xa = (array[0] as PdfNumber).FloatValue;
		ya = (array[1] as PdfNumber).FloatValue;
		za = (array[2] as PdfNumber).FloatValue;
		xb = (array[3] as PdfNumber).FloatValue;
		yb = (array[4] as PdfNumber).FloatValue;
		zb = (array[5] as PdfNumber).FloatValue;
		xc = (array[6] as PdfNumber).FloatValue;
		yc = (array[7] as PdfNumber).FloatValue;
		zc = (array[8] as PdfNumber).FloatValue;
	}

	internal IList<object> ToArray()
	{
		return new object[9] { xa, ya, za, xb, yb, zb, xc, yc, zc };
	}

	internal double[] Multiply(double x, double y, double z)
	{
		return new double[3]
		{
			xa * x + ya * y + za * z,
			xb * x + yb * y + zb * z,
			xc * x + yc * y + zc * z
		};
	}
}
