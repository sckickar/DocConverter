using System;

namespace DocGen.Pdf.Security;

internal class ECPrivateKey : EllipticKeyParam
{
	private readonly Number number;

	public Number Key => number;

	public ECPrivateKey(Number number, EllipticCurveParams parameters)
		: this("EC", number, parameters)
	{
	}

	public ECPrivateKey(string algorithm, Number number, EllipticCurveParams parameters)
		: base(algorithm, isPrivate: true, parameters)
	{
		if (number == null)
		{
			throw new ArgumentNullException("number");
		}
		this.number = number;
	}

	public ECPrivateKey(string algorithm, Number number, DerObjectID publicKeySet)
		: base(algorithm, isPrivate: true, publicKeySet)
	{
		if (number == null)
		{
			throw new ArgumentNullException("number");
		}
		this.number = number;
	}

	public override bool Equals(object element)
	{
		if (element == this)
		{
			return true;
		}
		if (!(element is ECPrivateKey element2))
		{
			return false;
		}
		return Equals(element2);
	}

	protected bool Equals(ECPrivateKey element)
	{
		if (number.Equals(element.number))
		{
			return Equals((EllipticKeyParam)element);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return number.GetHashCode() ^ base.GetHashCode();
	}
}
