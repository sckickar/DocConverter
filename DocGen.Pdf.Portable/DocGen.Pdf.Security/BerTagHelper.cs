using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class BerTagHelper : IAsn1Tag, IAsn1
{
	private bool m_isConstructed;

	private int m_tagNumber;

	private Asn1Parser m_helper;

	internal bool Constructed => m_isConstructed;

	public int TagNumber => m_tagNumber;

	internal BerTagHelper(bool isConstructed, int tagNumber, Asn1Parser helper)
	{
		m_isConstructed = isConstructed;
		m_tagNumber = tagNumber;
		m_helper = helper;
	}

	public IAsn1 GetParser(int tagNumber, bool isExplicit)
	{
		if (isExplicit)
		{
			if (!m_isConstructed)
			{
				throw new IOException("Implicit tags identified");
			}
			return m_helper.ReadObject();
		}
		return m_helper.ReadImplicit(m_isConstructed, tagNumber);
	}

	public Asn1 GetAsn1()
	{
		try
		{
			return m_helper.ReadTaggedObject(m_isConstructed, m_tagNumber);
		}
		catch (IOException ex)
		{
			throw new Exception(ex.Message);
		}
	}
}
