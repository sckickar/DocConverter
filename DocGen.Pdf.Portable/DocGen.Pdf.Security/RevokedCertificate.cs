using System;

namespace DocGen.Pdf.Security;

public class RevokedCertificate
{
	private string m_serialNumber;

	private DateTime m_validTo;

	public string SerialNumber
	{
		get
		{
			return m_serialNumber;
		}
		internal set
		{
			m_serialNumber = value;
		}
	}

	public DateTime ValidTo
	{
		get
		{
			return m_validTo;
		}
		internal set
		{
			m_validTo = value;
		}
	}
}
