using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class DerOctetHelper : IAsn1Octet, IAsn1
{
	private readonly Asn1StreamHelper m_stream;

	internal DerOctetHelper(Asn1StreamHelper stream)
	{
		m_stream = stream;
	}

	public Stream GetOctetStream()
	{
		return m_stream;
	}

	public Asn1 GetAsn1()
	{
		try
		{
			return new DerOctet(m_stream.ToArray());
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message);
		}
	}
}
