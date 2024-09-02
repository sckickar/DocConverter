using System;

namespace DocGen.Pdf.Security;

internal class Finite2MFieldObject : EllipticCurveElements
{
	public const int intergerX = 1;

	public const int intergerY = 2;

	public const int intergerZ = 3;

	private int valueA;

	private int valueB;

	private int valueC;

	private int valueD;

	private int valueE;

	private PdfIntArray numberPX;

	private readonly int valueF;

	public override string ECElementName => "F2m";

	public override int ElementSize => valueB;

	public Finite2MFieldObject(int valueB, int valueC, int valueD, int valueE, Number numberPX)
	{
		valueF = valueB + 31 >> 5;
		this.numberPX = new PdfIntArray(numberPX, valueF);
		if (valueD == 0 && valueE == 0)
		{
			valueA = 2;
		}
		else
		{
			if (valueD >= valueE)
			{
				throw new ArgumentException("value must be smaller");
			}
			if (valueD <= 0)
			{
				throw new ArgumentException("value must be larger than 0");
			}
			valueA = 3;
		}
		if (numberPX.SignValue < 0)
		{
			throw new ArgumentException("value cannot be negative");
		}
		this.valueB = valueB;
		this.valueC = valueC;
		this.valueD = valueD;
		this.valueE = valueE;
	}

	private Finite2MFieldObject(int valueB, int valueC, int valueD, int valueE, PdfIntArray numberPX)
	{
		valueF = valueB + 31 >> 5;
		this.numberPX = numberPX;
		this.valueB = valueB;
		this.valueC = valueC;
		this.valueD = valueD;
		this.valueE = valueE;
		if (valueD == 0 && valueE == 0)
		{
			valueA = 2;
		}
		else
		{
			valueA = 3;
		}
	}

	public override Number ToIntValue()
	{
		return numberPX.ToBigInteger();
	}

	public static void ValidateElements(EllipticCurveElements curveA, EllipticCurveElements value)
	{
		if (!(curveA is Finite2MFieldObject) || !(value is Finite2MFieldObject))
		{
			throw new ArgumentException("Finite2MFieldObject");
		}
		Finite2MFieldObject finite2MFieldObject = (Finite2MFieldObject)curveA;
		Finite2MFieldObject finite2MFieldObject2 = (Finite2MFieldObject)value;
		if (finite2MFieldObject.valueB != finite2MFieldObject2.valueB || finite2MFieldObject.valueC != finite2MFieldObject2.valueC || finite2MFieldObject.valueD != finite2MFieldObject2.valueD || finite2MFieldObject.valueE != finite2MFieldObject2.valueE)
		{
			throw new ArgumentException("F2M field");
		}
		if (finite2MFieldObject.valueA != finite2MFieldObject2.valueA)
		{
			throw new ArgumentException("elements has incorrect");
		}
	}

	public override EllipticCurveElements SumValue(EllipticCurveElements value)
	{
		PdfIntArray pdfIntArray = numberPX.Copy();
		Finite2MFieldObject finite2MFieldObject = (Finite2MFieldObject)value;
		pdfIntArray.AddShifted(finite2MFieldObject.numberPX, 0);
		return new Finite2MFieldObject(valueB, valueC, valueD, valueE, pdfIntArray);
	}

	public override EllipticCurveElements Subtract(EllipticCurveElements value)
	{
		return SumValue(value);
	}

	public override EllipticCurveElements Multiply(EllipticCurveElements value)
	{
		Finite2MFieldObject finite2MFieldObject = (Finite2MFieldObject)value;
		PdfIntArray pdfIntArray = numberPX.Multiply(finite2MFieldObject.numberPX, valueB);
		pdfIntArray.Reduce(valueB, new int[3] { valueC, valueD, valueE });
		return new Finite2MFieldObject(valueB, valueC, valueD, valueE, pdfIntArray);
	}

	public override EllipticCurveElements Divide(EllipticCurveElements value)
	{
		EllipticCurveElements value2 = value.Invert();
		return Multiply(value2);
	}

	public override EllipticCurveElements Negate()
	{
		return this;
	}

	public override EllipticCurveElements Square()
	{
		PdfIntArray pdfIntArray = numberPX.Square(valueB);
		pdfIntArray.Reduce(valueB, new int[3] { valueC, valueD, valueE });
		return new Finite2MFieldObject(valueB, valueC, valueD, valueE, pdfIntArray);
	}

	public override EllipticCurveElements Invert()
	{
		PdfIntArray pdfIntArray = numberPX.Copy();
		PdfIntArray pdfIntArray2 = new PdfIntArray(valueF);
		pdfIntArray2.SetBit(valueB);
		pdfIntArray2.SetBit(0);
		pdfIntArray2.SetBit(valueC);
		if (valueA == 3)
		{
			pdfIntArray2.SetBit(valueD);
			pdfIntArray2.SetBit(valueE);
		}
		PdfIntArray pdfIntArray3 = new PdfIntArray(valueF);
		pdfIntArray3.SetBit(0);
		PdfIntArray pdfIntArray4 = new PdfIntArray(valueF);
		while (pdfIntArray.GetLength() > 0)
		{
			int num = pdfIntArray.BitLength - pdfIntArray2.BitLength;
			if (num < 0)
			{
				PdfIntArray pdfIntArray5 = pdfIntArray;
				pdfIntArray = pdfIntArray2;
				pdfIntArray2 = pdfIntArray5;
				PdfIntArray pdfIntArray6 = pdfIntArray3;
				pdfIntArray3 = pdfIntArray4;
				pdfIntArray4 = pdfIntArray6;
				num = -num;
			}
			int shift = num >> 5;
			int number = num & 0x1F;
			PdfIntArray values = pdfIntArray2.ShiftLeft(number);
			pdfIntArray.AddShifted(values, shift);
			PdfIntArray values2 = pdfIntArray4.ShiftLeft(number);
			pdfIntArray3.AddShifted(values2, shift);
		}
		return new Finite2MFieldObject(valueB, valueC, valueD, valueE, pdfIntArray4);
	}

	public override EllipticCurveElements SquareRoot()
	{
		throw new ArithmeticException("Function not implemented");
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is Finite2MFieldObject element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(Finite2MFieldObject element)
	{
		if (valueB == element.valueB && valueC == element.valueC && valueD == element.valueD && valueE == element.valueE && valueA == element.valueA)
		{
			return Equals((EllipticCurveElements)element);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return valueB.GetHashCode() ^ valueC.GetHashCode() ^ valueD.GetHashCode() ^ valueE.GetHashCode() ^ valueA.GetHashCode() ^ base.GetHashCode();
	}
}
