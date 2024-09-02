using System;

namespace DocGen.Pdf.Security;

internal class FinitePFieldObject : EllipticCurveElements
{
	private readonly Number numberPQ;

	private readonly Number numberPX;

	public override string ECElementName => "Fp";

	public override int ElementSize => numberPQ.BitLength;

	public FinitePFieldObject(Number numberPQ, Number numberPX)
	{
		if (numberPX.CompareTo(numberPQ) >= 0)
		{
			throw new ArgumentException("value too large in field element");
		}
		this.numberPQ = numberPQ;
		this.numberPX = numberPX;
	}

	public override Number ToIntValue()
	{
		return numberPX;
	}

	public override EllipticCurveElements SumValue(EllipticCurveElements value)
	{
		return new FinitePFieldObject(numberPQ, numberPX.Add(value.ToIntValue()).Mod(numberPQ));
	}

	public override EllipticCurveElements Subtract(EllipticCurveElements value)
	{
		return new FinitePFieldObject(numberPQ, numberPX.Subtract(value.ToIntValue()).Mod(numberPQ));
	}

	public override EllipticCurveElements Multiply(EllipticCurveElements value)
	{
		return new FinitePFieldObject(numberPQ, numberPX.Multiply(value.ToIntValue()).Mod(numberPQ));
	}

	public override EllipticCurveElements Divide(EllipticCurveElements value)
	{
		return new FinitePFieldObject(numberPQ, numberPX.Multiply(value.ToIntValue().ModInverse(numberPQ)).Mod(numberPQ));
	}

	public override EllipticCurveElements Negate()
	{
		return new FinitePFieldObject(numberPQ, numberPX.Negate().Mod(numberPQ));
	}

	public override EllipticCurveElements Square()
	{
		return new FinitePFieldObject(numberPQ, numberPX.Multiply(numberPX).Mod(numberPQ));
	}

	public override EllipticCurveElements Invert()
	{
		return new FinitePFieldObject(numberPQ, numberPX.ModInverse(numberPQ));
	}

	public override EllipticCurveElements SquareRoot()
	{
		if (!numberPQ.TestBit(0))
		{
			throw new PdfException("even value");
		}
		if (numberPQ.TestBit(1))
		{
			EllipticCurveElements ellipticCurveElements = new FinitePFieldObject(numberPQ, numberPX.ModPow(numberPQ.ShiftRight(2).Add(Number.One), numberPQ));
			if (!Equals(ellipticCurveElements.Square()))
			{
				return null;
			}
			return ellipticCurveElements;
		}
		Number number = numberPQ.Subtract(Number.One);
		Number e = number.ShiftRight(1);
		if (!numberPX.ModPow(e, numberPQ).Equals(Number.One))
		{
			return null;
		}
		Number numberK = number.ShiftRight(2).ShiftLeft(1).Add(Number.One);
		Number number2 = numberPX;
		Number number3 = number2.ShiftLeft(2).Mod(numberPQ);
		Number number5;
		do
		{
			SecureRandomAlgorithm random = new SecureRandomAlgorithm();
			Number number4;
			do
			{
				number4 = new Number(numberPQ.BitLength, random);
			}
			while (number4.CompareTo(numberPQ) >= 0 || !number4.Multiply(number4).Subtract(number3).ModPow(e, numberPQ)
				.Equals(number));
			Number[] array = FLSequence(numberPQ, number4, number2, numberK);
			number5 = array[0];
			Number number6 = array[1];
			if (number6.Multiply(number6).Mod(numberPQ).Equals(number3))
			{
				if (number6.TestBit(0))
				{
					number6 = number6.Add(numberPQ);
				}
				number6 = number6.ShiftRight(1);
				return new FinitePFieldObject(numberPQ, number6);
			}
		}
		while (number5.Equals(Number.One) || number5.Equals(number));
		return null;
	}

	private static Number[] FLSequence(Number valueP, Number numberP, Number numberQ, Number numberK1)
	{
		int bitLength = numberK1.BitLength;
		int lowestSetBit = numberK1.GetLowestSetBit();
		Number number = Number.One;
		Number number2 = Number.Two;
		Number number3 = numberP;
		Number number4 = Number.One;
		Number number5 = Number.One;
		for (int num = bitLength - 1; num >= lowestSetBit + 1; num--)
		{
			number4 = number4.Multiply(number5).Mod(valueP);
			if (numberK1.TestBit(num))
			{
				number5 = number4.Multiply(numberQ).Mod(valueP);
				number = number.Multiply(number3).Mod(valueP);
				number2 = number3.Multiply(number2).Subtract(numberP.Multiply(number4)).Mod(valueP);
				number3 = number3.Multiply(number3).Subtract(number5.ShiftLeft(1)).Mod(valueP);
			}
			else
			{
				number5 = number4;
				number = number.Multiply(number2).Subtract(number4).Mod(valueP);
				number3 = number3.Multiply(number2).Subtract(numberP.Multiply(number4)).Mod(valueP);
				number2 = number2.Multiply(number2).Subtract(number4.ShiftLeft(1)).Mod(valueP);
			}
		}
		number4 = number4.Multiply(number5).Mod(valueP);
		number5 = number4.Multiply(numberQ).Mod(valueP);
		number = number.Multiply(number2).Subtract(number4).Mod(valueP);
		number2 = number3.Multiply(number2).Subtract(numberP.Multiply(number4)).Mod(valueP);
		number4 = number4.Multiply(number5).Mod(valueP);
		for (int i = 1; i <= lowestSetBit; i++)
		{
			number = number.Multiply(number2).Mod(valueP);
			number2 = number2.Multiply(number2).Subtract(number4.ShiftLeft(1)).Mod(valueP);
			number4 = number4.Multiply(number4).Mod(valueP);
		}
		return new Number[2] { number, number2 };
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is FinitePFieldObject element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(FinitePFieldObject element)
	{
		if (numberPQ.Equals(element.numberPQ))
		{
			return Equals((EllipticCurveElements)element);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return numberPQ.GetHashCode() ^ base.GetHashCode();
	}
}
