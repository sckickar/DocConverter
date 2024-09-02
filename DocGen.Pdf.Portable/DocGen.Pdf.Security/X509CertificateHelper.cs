namespace DocGen.Pdf.Security;

internal class X509CertificateHelper
{
	private byte[] m_id;

	private X509Name m_issuer;

	private Number m_serialNumber;

	internal byte[] KeyIdentifier
	{
		get
		{
			if (m_id != null)
			{
				return (byte[])m_id.Clone();
			}
			return null;
		}
		set
		{
			m_id = ((value == null) ? null : ((byte[])value.Clone()));
		}
	}

	internal X509Name Issuer
	{
		get
		{
			return m_issuer;
		}
		set
		{
			m_issuer = value;
		}
	}

	internal Number SerialNumber
	{
		get
		{
			return m_serialNumber;
		}
		set
		{
			m_serialNumber = value;
		}
	}
}
