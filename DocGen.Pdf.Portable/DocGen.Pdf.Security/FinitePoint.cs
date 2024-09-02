using System;

namespace DocGen.Pdf.Security;

internal class FinitePoint : ECPointBase
{
	protected internal override bool BasePoint => base.PointY.ToIntValue().TestBit(0);

	internal FinitePoint(EllipticCurves curve, EllipticCurveElements pointX, EllipticCurveElements pointY)
		: this(curve, pointX, pointY, isCompress: false)
	{
	}

	internal FinitePoint(EllipticCurves curve, EllipticCurveElements pointX, EllipticCurveElements pointY, bool isCompress)
		: base(curve, pointX, pointY, isCompress)
	{
		if (pointX == null != (pointY == null))
		{
			throw new ArgumentException("field elements is null");
		}
	}

	internal override EllipticPoint SumValue(EllipticPoint value)
	{
		if (base.IsInfinity)
		{
			return value;
		}
		if (value.IsInfinity)
		{
			return this;
		}
		if (pointX.Equals(value.pointX))
		{
			if (pointY.Equals(value.pointY))
			{
				return Twice();
			}
			return curve.IsInfinity;
		}
		EllipticCurveElements ellipticCurveElements = value.pointY.Subtract(pointY).Divide(value.pointX.Subtract(pointX));
		EllipticCurveElements value2 = ellipticCurveElements.Square().Subtract(pointX).Subtract(value.pointX);
		EllipticCurveElements ellipticCurveElements2 = ellipticCurveElements.Multiply(pointX.Subtract(value2)).Subtract(pointY);
		return new FinitePoint(curve, value2, ellipticCurveElements2, isCompress);
	}

	internal override EllipticPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		if (pointY.ToIntValue().SignValue == 0)
		{
			return curve.IsInfinity;
		}
		EllipticCurveElements value = curve.ECNumber(Number.Two);
		EllipticCurveElements value2 = curve.ECNumber(Number.Three);
		EllipticCurveElements ellipticCurveElements = pointX.Square().Multiply(value2).SumValue(curve.elementA)
			.Divide(pointY.Multiply(value));
		EllipticCurveElements value3 = ellipticCurveElements.Square().Subtract(pointX.Multiply(value));
		EllipticCurveElements ellipticCurveElements2 = ellipticCurveElements.Multiply(pointX.Subtract(value3)).Subtract(pointY);
		return new FinitePoint(curve, value3, ellipticCurveElements2, isCompress);
	}

	internal override EllipticPoint Subtract(EllipticPoint value)
	{
		if (value.IsInfinity)
		{
			return this;
		}
		return SumValue(value.Negate());
	}

	internal override EllipticPoint Negate()
	{
		return new FinitePoint(curve, pointX, pointY.Negate(), isCompress);
	}

	internal override void CheckMultiplier()
	{
		if (multiplier != null)
		{
			return;
		}
		lock (this)
		{
			if (multiplier == null)
			{
				multiplier = new ECWMultiplier();
			}
		}
	}
}
