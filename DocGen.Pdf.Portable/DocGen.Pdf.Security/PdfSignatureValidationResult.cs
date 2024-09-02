using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf.Security;

public class PdfSignatureValidationResult
{
	private SignatureStatus m_signatureStatus;

	private bool m_isDocumentModified;

	private bool m_isCertificated;

	private bool m_isValidAtSignedTime;

	private bool m_isValidAtCurrentTime;

	private bool m_isValidAtTimeStampTime;

	private bool m_signerIdentity;

	private RevocationResult m_revocationResult;

	private CryptographicStandard m_cryptographicStandard;

	private DigestAlgorithm m_digestAlgorithm;

	private string m_signatureAlgorithm;

	private PAdESSignatureLevel m_signatureLevel;

	private X509Certificate2Collection m_certificates;

	private TimeStampInformation m_timeStampInfo;

	private List<PdfSignatureValidationException> m_signatureValidationErrors;

	private string m_signatureName;

	internal bool m_isValidOCSPorCRLtimeValidation;

	internal PdfSignatureValidationOptions signatureOptions;

	private LtvVerificationInfo m_ltvVerificationInfo;

	private List<PdfSignerCertificate> m_signerCertificates;

	private PdfCmsSigner m_signer;

	public string SignatureName
	{
		get
		{
			return m_signatureName;
		}
		internal set
		{
			m_signatureName = value;
		}
	}

	public SignatureStatus SignatureStatus
	{
		get
		{
			return m_signatureStatus;
		}
		internal set
		{
			m_signatureStatus = value;
		}
	}

	public RevocationResult RevocationResult
	{
		get
		{
			return m_revocationResult;
		}
		internal set
		{
			m_revocationResult = value;
		}
	}

	public bool IsDocumentModified
	{
		get
		{
			return m_isDocumentModified;
		}
		internal set
		{
			m_isDocumentModified = value;
		}
	}

	public bool IsCertificated
	{
		get
		{
			return m_isCertificated;
		}
		internal set
		{
			m_isCertificated = value;
		}
	}

	public bool IsValidAtSignedTime
	{
		get
		{
			return m_isValidAtSignedTime;
		}
		internal set
		{
			m_isValidAtSignedTime = value;
		}
	}

	public bool IsValidAtCurrentTime
	{
		get
		{
			return m_isValidAtCurrentTime;
		}
		internal set
		{
			m_isValidAtCurrentTime = value;
		}
	}

	public bool IsValidAtTimeStampTime
	{
		get
		{
			return m_isValidAtTimeStampTime;
		}
		internal set
		{
			m_isValidAtTimeStampTime = value;
		}
	}

	public bool IsSignatureValid
	{
		get
		{
			return m_signerIdentity;
		}
		internal set
		{
			m_signerIdentity = value;
		}
	}

	public CryptographicStandard CryptographicStandard
	{
		get
		{
			return m_cryptographicStandard;
		}
		internal set
		{
			m_cryptographicStandard = value;
		}
	}

	public string SignatureAlgorithm
	{
		get
		{
			return m_signatureAlgorithm;
		}
		internal set
		{
			m_signatureAlgorithm = value;
		}
	}

	public DigestAlgorithm DigestAlgorithm
	{
		get
		{
			return m_digestAlgorithm;
		}
		internal set
		{
			m_digestAlgorithm = value;
		}
	}

	internal PAdESSignatureLevel PAdESSignatureLevel
	{
		get
		{
			return m_signatureLevel;
		}
		set
		{
			m_signatureLevel = value;
		}
	}

	public X509Certificate2Collection Certificates
	{
		get
		{
			if (m_certificates == null)
			{
				m_certificates = new X509Certificate2Collection();
			}
			return m_certificates;
		}
		internal set
		{
			m_certificates = value;
		}
	}

	public TimeStampInformation TimeStampInformation
	{
		get
		{
			return m_timeStampInfo;
		}
		internal set
		{
			m_timeStampInfo = value;
		}
	}

	public List<PdfSignatureValidationException> SignatureValidationErrors
	{
		get
		{
			if (m_signatureValidationErrors == null)
			{
				m_signatureValidationErrors = new List<PdfSignatureValidationException>();
			}
			return m_signatureValidationErrors;
		}
		internal set
		{
			m_signatureValidationErrors = value;
		}
	}

	public LtvVerificationInfo LtvVerificationInfo
	{
		get
		{
			return m_ltvVerificationInfo;
		}
		internal set
		{
			m_ltvVerificationInfo = value;
		}
	}

	public PdfSignerCertificate[] SignerCertificates
	{
		get
		{
			if (m_signerCertificates == null)
			{
				m_signerCertificates = new List<PdfSignerCertificate>();
				if (m_signer != null)
				{
					m_signer.UpdateSignerDetails(m_signerCertificates, this);
				}
			}
			return m_signerCertificates.ToArray();
		}
	}

	internal PdfCmsSigner Signer
	{
		get
		{
			return m_signer;
		}
		set
		{
			m_signer = value;
		}
	}
}
