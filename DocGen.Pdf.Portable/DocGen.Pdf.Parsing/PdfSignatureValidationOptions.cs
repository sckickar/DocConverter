using System.Collections.Generic;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Parsing;

public class PdfSignatureValidationOptions
{
	private bool m_revocation = true;

	private RevocationValidationType m_revocationValidationType;

	private List<byte[]> ocspExternalData;

	private List<byte[]> crlExternalData;

	public bool ValidateRevocationStatus
	{
		get
		{
			return m_revocation;
		}
		set
		{
			m_revocation = value;
		}
	}

	public RevocationValidationType RevocationValidationType
	{
		get
		{
			return m_revocationValidationType;
		}
		set
		{
			m_revocationValidationType = value;
		}
	}

	internal List<byte[]> OCSPResponseData
	{
		get
		{
			return ocspExternalData;
		}
		set
		{
			ocspExternalData = value;
		}
	}

	internal List<byte[]> CRLResponseData
	{
		get
		{
			return crlExternalData;
		}
		set
		{
			crlExternalData = value;
		}
	}
}
