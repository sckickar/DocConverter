using System;

namespace DocGen.Pdf.Security;

public class PdfSignatureEventArgs : EventArgs
{
	private byte[] m_data;

	private byte[] m_signedData;

	public byte[] Data => m_data;

	public byte[] SignedData
	{
		get
		{
			return m_signedData;
		}
		set
		{
			if (value != null)
			{
				m_signedData = value;
			}
		}
	}

	internal PdfSignatureEventArgs(byte[] documentData)
	{
		m_data = documentData;
	}
}
