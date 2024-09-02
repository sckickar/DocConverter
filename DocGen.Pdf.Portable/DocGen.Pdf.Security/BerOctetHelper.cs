using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class BerOctetHelper : IAsn1Octet, IAsn1
{
	private Asn1Parser m_helper;

	internal BerOctetHelper(Asn1Parser helper)
	{
		m_helper = helper;
	}

	public Stream GetOctetStream()
	{
		return new OctetStream(m_helper);
	}

	public Asn1 GetAsn1()
	{
		try
		{
			return new BerOctet(ReadAll(GetOctetStream()));
		}
		catch (IOException ex)
		{
			throw new Exception(ex.ToString());
		}
	}

	internal byte[] ReadAll(Stream stream)
	{
		MemoryStream memoryStream = new MemoryStream();
		PipeAll(stream, memoryStream);
		return memoryStream.ToArray();
	}

	private void PipeAll(Stream input, Stream output)
	{
		byte[] array = new byte[512];
		int count;
		while ((count = input.Read(array, 0, array.Length)) > 0)
		{
			output.Write(array, 0, count);
		}
	}
}
