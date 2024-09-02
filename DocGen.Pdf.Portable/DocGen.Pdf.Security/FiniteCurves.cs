using System;

namespace DocGen.Pdf.Security;

internal class FiniteCurves : EllipticCurves
{
	private readonly Number number;

	private readonly FinitePoint infinityPoint;

	public Number PointQ => number;

	public override EllipticPoint IsInfinity => infinityPoint;

	public override int Size => number.BitLength;

	public FiniteCurves(Number number, Number elementA, Number elementB)
	{
		this.number = number;
		base.elementA = ECNumber(elementA);
		base.elementB = ECNumber(elementB);
		infinityPoint = new FinitePoint(this, null, null);
	}

	public override EllipticCurveElements ECNumber(Number num)
	{
		return new FinitePFieldObject(number, num);
	}

	public override EllipticPoint ECPoints(Number numberX1, Number numberY1, bool isCompress)
	{
		return new FinitePoint(this, ECNumber(numberX1), ECNumber(numberY1), isCompress);
	}

	protected override EllipticPoint GetDecompressECPoint(int point, Number numberX1)
	{
		EllipticCurveElements ellipticCurveElements = ECNumber(numberX1);
		EllipticCurveElements ellipticCurveElements2 = ellipticCurveElements.Multiply(ellipticCurveElements.Square().SumValue(elementA)).SumValue(elementB).SquareRoot();
		if (ellipticCurveElements2 == null)
		{
			throw new ArithmeticException("point is empty");
		}
		Number number = ellipticCurveElements2.ToIntValue();
		if ((number.TestBit(0) ? 1 : 0) != point)
		{
			ellipticCurveElements2 = ECNumber(this.number.Subtract(number));
		}
		return new FinitePoint(this, ellipticCurveElements, ellipticCurveElements2, isCompress: true);
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is FiniteCurves element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(FiniteCurves element)
	{
		if (Equals((EllipticCurves)element))
		{
			return number.Equals(element.number);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ number.GetHashCode();
	}
}
