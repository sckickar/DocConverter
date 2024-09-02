using System;

namespace DocGen.Pdf.Security;

internal class Field2MCurves : EllipticCurves
{
	private readonly int pointM;

	private readonly int elementX;

	private readonly int elementY;

	private readonly int elementZ;

	private readonly Number numberX;

	private readonly Number numberY;

	private readonly Finite2MPoint infinityPoint;

	private sbyte mPoint;

	private Number[] collection;

	public override EllipticPoint IsInfinity => infinityPoint;

	public override int Size => pointM;

	public bool IsKOBLITZ
	{
		get
		{
			if (numberX != null && numberY != null && (elementA.ToIntValue().Equals(Number.Zero) || elementA.ToIntValue().Equals(Number.One)))
			{
				return elementB.ToIntValue().Equals(Number.One);
			}
			return false;
		}
	}

	public int PointM => pointM;

	public int ElementX => elementX;

	public int ElementY => elementY;

	public int ElementZ => elementZ;

	public Number NumberY => numberY;

	public Field2MCurves(int pointM, int pointX, Number elementA, Number elementB, Number numberX, Number numberY)
		: this(pointM, pointX, 0, 0, elementA, elementB, numberX, numberY)
	{
	}

	public Field2MCurves(int pointM, int elementX, int elementY, int elementZ, Number elementA, Number elementB)
		: this(pointM, elementX, elementY, elementZ, elementA, elementB, null, null)
	{
	}

	public Field2MCurves(int pointM, int elementX, int elementY, int elementZ, Number elementA, Number elementB, Number numberX, Number numberY)
	{
		this.pointM = pointM;
		this.elementX = elementX;
		this.elementY = elementY;
		this.elementZ = elementZ;
		this.numberX = numberX;
		this.numberY = numberY;
		infinityPoint = new Finite2MPoint(this, null, null);
		if (elementX == 0)
		{
			throw new ArgumentException("elementX must be > 0");
		}
		if (elementY == 0)
		{
			if (elementZ != 0)
			{
				throw new ArgumentException("elementZ must be 0 if elementY == 0");
			}
		}
		else
		{
			if (elementY <= elementX)
			{
				throw new ArgumentException("elementY must be > elementX");
			}
			if (elementZ <= elementY)
			{
				throw new ArgumentException("elementZ must be > elementY");
			}
		}
		base.elementA = ECNumber(elementA);
		base.elementB = ECNumber(elementB);
	}

	public override EllipticCurveElements ECNumber(Number number)
	{
		return new Finite2MFieldObject(pointM, elementX, elementY, elementZ, number);
	}

	internal sbyte MU()
	{
		if (mPoint == 0)
		{
			lock (this)
			{
				if (mPoint == 0)
				{
					mPoint = ECTanFunction.FindMU(this);
				}
			}
		}
		return mPoint;
	}

	internal Number[] SI()
	{
		if (collection == null)
		{
			lock (this)
			{
				if (collection == null)
				{
					collection = ECTanFunction.FindSI(this);
				}
			}
		}
		return collection;
	}

	public override EllipticPoint ECPoints(Number numberX1, Number numberY1, bool isCompress)
	{
		return new Finite2MPoint(this, ECNumber(numberX1), ECNumber(numberY1), isCompress);
	}

	protected override EllipticPoint GetDecompressECPoint(int point, Number numberX1)
	{
		EllipticCurveElements ellipticCurveElements = ECNumber(numberX1);
		EllipticCurveElements ellipticCurveElements2 = null;
		if (ellipticCurveElements.ToIntValue().SignValue == 0)
		{
			ellipticCurveElements2 = (Finite2MFieldObject)elementB;
			for (int i = 0; i < pointM - 1; i++)
			{
				ellipticCurveElements2 = ellipticCurveElements2.Square();
			}
		}
		else
		{
			EllipticCurveElements betaPoint = ellipticCurveElements.SumValue(elementA).SumValue(elementB.Multiply(ellipticCurveElements.Square().Invert()));
			EllipticCurveElements ellipticCurveElements3 = ECEquation(betaPoint);
			if (ellipticCurveElements3 == null)
			{
				throw new ArithmeticException("Incorrect point");
			}
			if ((ellipticCurveElements3.ToIntValue().TestBit(0) ? 1 : 0) != point)
			{
				ellipticCurveElements3 = ellipticCurveElements3.SumValue(ECNumber(Number.One));
			}
			ellipticCurveElements2 = ellipticCurveElements.Multiply(ellipticCurveElements3);
		}
		return new Finite2MPoint(this, ellipticCurveElements, ellipticCurveElements2, isCompress: true);
	}

	private EllipticCurveElements ECEquation(EllipticCurveElements betaPoint)
	{
		if (betaPoint.ToIntValue().SignValue == 0)
		{
			return ECNumber(Number.Zero);
		}
		EllipticCurveElements ellipticCurveElements = null;
		EllipticCurveElements ellipticCurveElements2 = ECNumber(Number.Zero);
		while (ellipticCurveElements2.ToIntValue().SignValue == 0)
		{
			EllipticCurveElements value = ECNumber(new Number(pointM, new SecureRandomAlgorithm()));
			ellipticCurveElements = ECNumber(Number.Zero);
			EllipticCurveElements ellipticCurveElements3 = betaPoint;
			for (int i = 1; i <= pointM - 1; i++)
			{
				EllipticCurveElements ellipticCurveElements4 = ellipticCurveElements3.Square();
				ellipticCurveElements = ellipticCurveElements.Square().SumValue(ellipticCurveElements4.Multiply(value));
				ellipticCurveElements3 = ellipticCurveElements4.SumValue(betaPoint);
			}
			if (ellipticCurveElements3.ToIntValue().SignValue != 0)
			{
				return null;
			}
			ellipticCurveElements2 = ellipticCurveElements.Square().SumValue(ellipticCurveElements);
		}
		return ellipticCurveElements;
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is Field2MCurves element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(Field2MCurves element)
	{
		if (pointM == element.pointM && elementX == element.elementX && elementY == element.elementY && elementZ == element.elementZ)
		{
			return Equals((EllipticCurves)element);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ pointM ^ elementX ^ elementY ^ elementZ;
	}
}
