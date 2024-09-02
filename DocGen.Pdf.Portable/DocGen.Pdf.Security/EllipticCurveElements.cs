namespace DocGen.Pdf.Security;

internal abstract class EllipticCurveElements
{
	public abstract string ECElementName { get; }

	public abstract int ElementSize { get; }

	public abstract Number ToIntValue();

	public abstract EllipticCurveElements SumValue(EllipticCurveElements value);

	public abstract EllipticCurveElements Subtract(EllipticCurveElements value);

	public abstract EllipticCurveElements Multiply(EllipticCurveElements value);

	public abstract EllipticCurveElements Divide(EllipticCurveElements value);

	public abstract EllipticCurveElements Negate();

	public abstract EllipticCurveElements Square();

	public abstract EllipticCurveElements Invert();

	public abstract EllipticCurveElements SquareRoot();

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is EllipticCurveElements element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(EllipticCurveElements element)
	{
		return ToIntValue().Equals(element.ToIntValue());
	}

	public override int GetHashCode()
	{
		return ToIntValue().GetHashCode();
	}

	public override string ToString()
	{
		return ToIntValue().ToString(2);
	}
}
