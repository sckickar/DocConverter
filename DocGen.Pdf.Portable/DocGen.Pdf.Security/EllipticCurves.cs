using System;

namespace DocGen.Pdf.Security;

internal abstract class EllipticCurves
{
	internal EllipticCurveElements elementA;

	internal EllipticCurveElements elementB;

	public abstract int Size { get; }

	public abstract EllipticPoint IsInfinity { get; }

	public EllipticCurveElements ElementA => elementA;

	public EllipticCurveElements ElementB => elementB;

	public abstract EllipticCurveElements ECNumber(Number number);

	public abstract EllipticPoint ECPoints(Number pointX, Number pointY, bool isCompress);

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is EllipticCurves element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(EllipticCurves element)
	{
		if (elementA.Equals(element.elementA))
		{
			return elementB.Equals(element.elementB);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return elementA.GetHashCode() ^ elementB.GetHashCode();
	}

	protected abstract EllipticPoint GetDecompressECPoint(int point, Number number);

	public virtual EllipticPoint GetDecodedECPoint(byte[] encodedPoints)
	{
		EllipticPoint ellipticPoint = null;
		int num = (Size + 7) / 8;
		switch (encodedPoints[0])
		{
		case 0:
			if (encodedPoints.Length != 1)
			{
				throw new ArgumentException("Invalid range for encodedPoints");
			}
			return IsInfinity;
		case 2:
		case 3:
		{
			if (encodedPoints.Length != num + 1)
			{
				throw new ArgumentException("Invalid range for compressed encodedPoints");
			}
			int point = encodedPoints[0] & 1;
			Number number = new Number(1, encodedPoints, 1, num);
			return GetDecompressECPoint(point, number);
		}
		case 4:
		case 6:
		case 7:
		{
			if (encodedPoints.Length != 2 * num + 1)
			{
				throw new ArgumentException("Invalid range for uncompressed encodedPoints");
			}
			Number pointX = new Number(1, encodedPoints, 1, num);
			Number pointY = new Number(1, encodedPoints, 1 + num, num);
			return ECPoints(pointX, pointY, isCompress: false);
		}
		default:
			throw new FormatException("Invalid encoding " + encodedPoints[0]);
		}
	}
}
