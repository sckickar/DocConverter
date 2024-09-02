using System.Collections;

namespace DocGen.Pdf.Security;

internal class KeyEntry
{
	private readonly CipherParameter m_key;

	private readonly IDictionary m_attributes;

	internal CipherParameter Key => m_key;

	internal KeyEntry(CipherParameter key, IDictionary attributes)
	{
		m_key = key;
		m_attributes = attributes;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is KeyEntry keyEntry))
		{
			return false;
		}
		return m_key.Equals(keyEntry.m_key);
	}

	public override int GetHashCode()
	{
		return ~m_key.GetHashCode();
	}
}
