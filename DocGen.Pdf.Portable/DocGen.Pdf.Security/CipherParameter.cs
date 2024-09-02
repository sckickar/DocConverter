namespace DocGen.Pdf.Security;

internal abstract class CipherParameter : ICipherParam
{
	private readonly bool m_privateKey;

	internal bool IsPrivate => m_privateKey;

	protected CipherParameter(bool privateKey)
	{
		m_privateKey = privateKey;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CipherParameter other))
		{
			return false;
		}
		return Equals(other);
	}

	protected bool Equals(CipherParameter other)
	{
		return m_privateKey == other.m_privateKey;
	}

	public override int GetHashCode()
	{
		return m_privateKey.GetHashCode();
	}
}
