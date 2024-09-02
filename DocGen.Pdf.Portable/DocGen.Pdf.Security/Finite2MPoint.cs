using System;

namespace DocGen.Pdf.Security;

internal class Finite2MPoint : ECPointBase
{
	protected internal override bool BasePoint
	{
		get
		{
			if (base.PointX.ToIntValue().SignValue != 0)
			{
				return base.PointY.Multiply(base.PointX.Invert()).ToIntValue().TestBit(0);
			}
			return false;
		}
	}

	internal Finite2MPoint(EllipticCurves curve, EllipticCurveElements pointX, EllipticCurveElements pointY)
		: this(curve, pointX, pointY, isCompress: false)
	{
	}

	internal Finite2MPoint(EllipticCurves curve, EllipticCurveElements pointX, EllipticCurveElements pointY, bool isCompress)
		: base(curve, pointX, pointY, isCompress)
	{
		if ((pointX != null && pointY == null) || (pointX == null && pointY != null))
		{
			throw new ArgumentException("elements is null");
		}
		if (pointX != null)
		{
			Finite2MFieldObject.ValidateElements(base.pointX, base.pointY);
			Finite2MFieldObject.ValidateElements(base.pointX, base.curve.ElementA);
		}
	}

	private static void ValidatePoints(EllipticPoint bitA, EllipticPoint value)
	{
		if (!bitA.curve.Equals(value.curve))
		{
			throw new ArgumentException("Only points on the same curve can be added or subtracted");
		}
	}

	internal override EllipticPoint SumValue(EllipticPoint value)
	{
		ValidatePoints(this, value);
		return AddSimple((Finite2MPoint)value);
	}

	internal Finite2MPoint AddSimple(Finite2MPoint value)
	{
		if (base.IsInfinity)
		{
			return value;
		}
		if (value.IsInfinity)
		{
			return this;
		}
		Finite2MFieldObject finite2MFieldObject = (Finite2MFieldObject)value.PointX;
		Finite2MFieldObject finite2MFieldObject2 = (Finite2MFieldObject)value.PointY;
		if (pointX.Equals(finite2MFieldObject))
		{
			if (pointY.Equals(finite2MFieldObject2))
			{
				return (Finite2MPoint)Twice();
			}
			return (Finite2MPoint)curve.IsInfinity;
		}
		EllipticCurveElements value2 = pointX.SumValue(finite2MFieldObject);
		Finite2MFieldObject finite2MFieldObject3 = (Finite2MFieldObject)pointY.SumValue(finite2MFieldObject2).Divide(value2);
		Finite2MFieldObject value3 = (Finite2MFieldObject)finite2MFieldObject3.Square().SumValue(finite2MFieldObject3).SumValue(value2)
			.SumValue(curve.ElementA);
		Finite2MFieldObject finite2MFieldObject4 = (Finite2MFieldObject)finite2MFieldObject3.Multiply(pointX.SumValue(value3)).SumValue(value3).SumValue(pointY);
		return new Finite2MPoint(curve, value3, finite2MFieldObject4, isCompress);
	}

	internal override EllipticPoint Subtract(EllipticPoint value)
	{
		ValidatePoints(this, value);
		return SubtractSimple((Finite2MPoint)value);
	}

	internal Finite2MPoint SubtractSimple(Finite2MPoint value)
	{
		if (value.IsInfinity)
		{
			return this;
		}
		return AddSimple((Finite2MPoint)value.Negate());
	}

	internal override EllipticPoint Twice()
	{
		if (base.IsInfinity)
		{
			return this;
		}
		if (pointX.ToIntValue().SignValue == 0)
		{
			return curve.IsInfinity;
		}
		Finite2MFieldObject finite2MFieldObject = (Finite2MFieldObject)pointX.SumValue(pointY.Divide(pointX));
		Finite2MFieldObject finite2MFieldObject2 = (Finite2MFieldObject)finite2MFieldObject.Square().SumValue(finite2MFieldObject).SumValue(curve.ElementA);
		EllipticCurveElements value = curve.ECNumber(Number.One);
		Finite2MFieldObject finite2MFieldObject3 = (Finite2MFieldObject)pointX.Square().SumValue(finite2MFieldObject2.Multiply(finite2MFieldObject.SumValue(value)));
		return new Finite2MPoint(curve, finite2MFieldObject2, finite2MFieldObject3, isCompress);
	}

	internal override EllipticPoint Negate()
	{
		return new Finite2MPoint(curve, pointX, pointX.SumValue(pointY), isCompress);
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
				if (((Field2MCurves)curve).IsKOBLITZ)
				{
					multiplier = new EllipticTNMuliplier();
				}
				else
				{
					multiplier = new ECWMultiplier();
				}
			}
		}
	}
}
