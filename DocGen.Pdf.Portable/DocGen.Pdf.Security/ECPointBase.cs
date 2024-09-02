using System;

namespace DocGen.Pdf.Security;

internal abstract class ECPointBase : EllipticPoint
{
	protected internal abstract bool BasePoint { get; }

	protected internal ECPointBase(EllipticCurves curve, EllipticCurveElements pointX, EllipticCurveElements pointY, bool isCompress)
		: base(curve, pointX, pointY, isCompress)
	{
	}

	internal override byte[] Encoded(bool compressed)
	{
		if (base.IsInfinity)
		{
			return new byte[1];
		}
		int byteLength = ECConvertPoint.GetByteLength(pointX);
		byte[] array = ECConvertPoint.ConvetByte(base.PointX.ToIntValue(), byteLength);
		byte[] array2;
		if (compressed)
		{
			array2 = new byte[1 + array.Length];
			array2[0] = (byte)(BasePoint ? 3u : 2u);
		}
		else
		{
			byte[] array3 = ECConvertPoint.ConvetByte(base.PointY.ToIntValue(), byteLength);
			array2 = new byte[1 + array.Length + array3.Length];
			array2[0] = 4;
			array3.CopyTo(array2, 1 + array.Length);
		}
		array.CopyTo(array2, 1);
		return array2;
	}

	internal override EllipticPoint Multiply(Number number)
	{
		if (number.SignValue < 0)
		{
			throw new ArgumentException("number cannot be negative");
		}
		if (base.IsInfinity)
		{
			return this;
		}
		if (number.SignValue == 0)
		{
			return curve.IsInfinity;
		}
		CheckMultiplier();
		return multiplier.Multiply(this, number, preInfo);
	}
}
