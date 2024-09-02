using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Security;

public class PdfSignatureSettings
{
	private DigestAlgorithm m_digestAlgorithm = DigestAlgorithm.SHA256;

	private CryptographicStandard m_cryptoStandard;

	private bool m_hasChanged;

	private PdfLoadedSignatureField m_field;

	private bool m_digestUpdated;

	internal PdfLoadedSignatureField SignatureField
	{
		get
		{
			return m_field;
		}
		set
		{
			m_field = value;
		}
	}

	public DigestAlgorithm DigestAlgorithm
	{
		get
		{
			if (!m_digestUpdated && m_field != null)
			{
				m_digestAlgorithm = GetDigestAlgorithm();
				m_digestUpdated = true;
			}
			return m_digestAlgorithm;
		}
		set
		{
			m_digestAlgorithm = value;
			m_hasChanged = true;
		}
	}

	public CryptographicStandard CryptographicStandard
	{
		get
		{
			return m_cryptoStandard;
		}
		set
		{
			m_cryptoStandard = value;
			m_hasChanged = true;
		}
	}

	internal bool HasChanged => m_hasChanged;

	internal PdfSignatureSettings()
	{
	}

	private DigestAlgorithm GetDigestAlgorithm()
	{
		PdfCmsSigner cmsSigner = m_field.CmsSigner;
		if (cmsSigner != null)
		{
			string hashAlgorithm = cmsSigner.HashAlgorithm;
			if (!string.IsNullOrEmpty(hashAlgorithm))
			{
				switch (hashAlgorithm)
				{
				case "SHA1":
					return DigestAlgorithm.SHA1;
				case "SHA256":
					return DigestAlgorithm.SHA256;
				case "SHA384":
					return DigestAlgorithm.SHA384;
				case "SHA512":
					return DigestAlgorithm.SHA512;
				case "RIPEMD160":
					return DigestAlgorithm.RIPEMD160;
				}
			}
		}
		return DigestAlgorithm.SHA256;
	}
}
