using System;

namespace DocGen.Pdf.Security;

internal class EllipticCurveParams
{
	internal EllipticCurves ecCurve;

	internal byte[] data;

	internal EllipticPoint pointG;

	internal Number numberX;

	internal Number numberY;

	internal EllipticCurves Curve => ecCurve;

	internal EllipticPoint PointG => pointG;

	internal Number NumberX => numberX;

	internal Number NumberY => numberY;

	internal EllipticCurveParams(EllipticCurves ecCurve, EllipticPoint pointG, Number numberX)
		: this(ecCurve, pointG, numberX, Number.One)
	{
	}

	internal EllipticCurveParams(EllipticCurves ecCurve, EllipticPoint pointG, Number numberX, Number numberY)
		: this(ecCurve, pointG, numberX, numberY, null)
	{
	}

	internal EllipticCurveParams(EllipticCurves ecCurve, EllipticPoint pointG, Number numberX, Number numberY, byte[] data)
	{
		if (ecCurve == null)
		{
			throw new ArgumentNullException("ecCurve");
		}
		if (pointG == null)
		{
			throw new ArgumentNullException("pointG");
		}
		if (numberX == null)
		{
			throw new ArgumentNullException("numberX");
		}
		if (numberY == null)
		{
			throw new ArgumentNullException("numberY");
		}
		this.ecCurve = ecCurve;
		this.pointG = pointG;
		this.numberX = numberX;
		this.numberY = numberY;
		this.data = Asn1Constants.Clone(data);
	}

	internal byte[] ECSeed()
	{
		return Asn1Constants.Clone(data);
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is EllipticCurveParams element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(EllipticCurveParams element)
	{
		if (ecCurve.Equals(element.ecCurve) && pointG.Equals(element.pointG) && numberX.Equals(element.numberX) && numberY.Equals(element.numberY))
		{
			return Asn1Constants.AreEqual(data, element.data);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ecCurve.GetHashCode() ^ pointG.GetHashCode() ^ numberX.GetHashCode() ^ numberY.GetHashCode() ^ Asn1Constants.GetHashCode(data);
	}
}
