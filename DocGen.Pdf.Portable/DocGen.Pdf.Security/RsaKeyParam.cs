namespace DocGen.Pdf.Security;

internal class RsaKeyParam : CipherParameter
{
	private Number m_modulus;

	private Number m_exponent;

	internal Number Modulus => m_modulus;

	internal Number Exponent => m_exponent;

	internal RsaKeyParam(bool isPrivate, Number modulus, Number exponent)
		: base(isPrivate)
	{
		m_modulus = modulus;
		m_exponent = exponent;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RsaKeyParam rsaKeyParam))
		{
			return false;
		}
		if (rsaKeyParam.IsPrivate == base.IsPrivate && rsaKeyParam.Modulus.Equals(m_modulus))
		{
			return rsaKeyParam.Exponent.Equals(m_exponent);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_modulus.GetHashCode() ^ m_exponent.GetHashCode() ^ base.IsPrivate.GetHashCode();
	}
}
