using System;

namespace DocGen.Pdf.Security;

internal class ECPublicKeyParam : EllipticKeyParam
{
	private readonly EllipticPoint pointQ;

	public EllipticPoint PointQ => pointQ;

	public ECPublicKeyParam(EllipticPoint pointQ, EllipticCurveParams parameters)
		: this("EC", pointQ, parameters)
	{
	}

	public ECPublicKeyParam(string algorithm, EllipticPoint pointQ, EllipticCurveParams parameters)
		: base(algorithm, isPrivate: false, parameters)
	{
		if (pointQ == null)
		{
			throw new ArgumentNullException("pointQ");
		}
		this.pointQ = pointQ;
	}

	public ECPublicKeyParam(string algorithm, EllipticPoint pointQ, DerObjectID publicKeyParamSet)
		: base(algorithm, isPrivate: false, publicKeyParamSet)
	{
		if (pointQ == null)
		{
			throw new ArgumentNullException("pointQ");
		}
		this.pointQ = pointQ;
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is ECPublicKeyParam element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(ECPublicKeyParam element)
	{
		if (pointQ.Equals(element.pointQ))
		{
			return Equals((EllipticKeyParam)element);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return pointQ.GetHashCode() ^ base.GetHashCode();
	}
}
