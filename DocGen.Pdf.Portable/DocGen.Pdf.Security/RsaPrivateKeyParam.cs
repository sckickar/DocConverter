using System;

namespace DocGen.Pdf.Security;

internal class RsaPrivateKeyParam : RsaKeyParam
{
	private Number m_publicExponent;

	private Number m_p;

	private Number m_q;

	private Number m_dP;

	private Number m_dQ;

	private Number m_inverse;

	internal Number PublicExponent => m_publicExponent;

	internal Number P => m_p;

	internal Number Q => m_q;

	internal Number DP => m_dP;

	internal Number DQ => m_dQ;

	internal Number QInv => m_inverse;

	internal RsaPrivateKeyParam(Number modulus, Number publicExponent, Number privateExponent, Number p, Number q, Number dP, Number dQ, Number inverse)
		: base(isPrivate: true, modulus, privateExponent)
	{
		ValidateValue(publicExponent);
		ValidateValue(p);
		ValidateValue(q);
		ValidateValue(dP);
		ValidateValue(dQ);
		ValidateValue(inverse);
		m_publicExponent = publicExponent;
		m_p = p;
		m_q = q;
		m_dP = dP;
		m_dQ = dQ;
		m_inverse = inverse;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is RsaPrivateKeyParam rsaPrivateKeyParam))
		{
			return false;
		}
		if (rsaPrivateKeyParam.DP.Equals(m_dP) && rsaPrivateKeyParam.DQ.Equals(m_dQ) && rsaPrivateKeyParam.Exponent.Equals(base.Exponent) && rsaPrivateKeyParam.Modulus.Equals(base.Modulus) && rsaPrivateKeyParam.P.Equals(m_p) && rsaPrivateKeyParam.Q.Equals(m_q) && rsaPrivateKeyParam.PublicExponent.Equals(m_publicExponent))
		{
			return rsaPrivateKeyParam.QInv.Equals(m_inverse);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return DP.GetHashCode() ^ DQ.GetHashCode() ^ base.Exponent.GetHashCode() ^ base.Modulus.GetHashCode() ^ P.GetHashCode() ^ Q.GetHashCode() ^ PublicExponent.GetHashCode() ^ QInv.GetHashCode();
	}

	private static void ValidateValue(Number number)
	{
		if (number == null)
		{
			throw new ArgumentNullException("number");
		}
		if (number.SignValue <= 0)
		{
			throw new ArgumentException("Invalid RSA entry");
		}
	}
}
