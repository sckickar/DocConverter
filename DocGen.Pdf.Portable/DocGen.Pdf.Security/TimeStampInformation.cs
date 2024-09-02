using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace DocGen.Pdf.Security;

public class TimeStampInformation
{
	private bool m_isValid = true;

	private X509Certificate2 m_certificate;

	private string m_messageImprintAlgorithmId;

	private string m_timeStampPolicyId;

	private DateTime m_time;

	private object m_signerInformation;

	private bool m_documentTimeStamp;

	private List<PdfSignerCertificate> m_signerCertificates;

	internal PdfCmsSigner m_signer;

	internal string MessageImprintAlgorithmId
	{
		get
		{
			return m_messageImprintAlgorithmId;
		}
		set
		{
			m_messageImprintAlgorithmId = value;
		}
	}

	public string TimeStampPolicyId
	{
		get
		{
			return m_timeStampPolicyId;
		}
		internal set
		{
			m_timeStampPolicyId = value;
		}
	}

	public DateTime Time
	{
		get
		{
			return m_time;
		}
		internal set
		{
			m_time = value;
		}
	}

	internal object SignerInformation
	{
		get
		{
			return m_signerInformation;
		}
		set
		{
			m_signerInformation = value;
		}
	}

	public bool IsValid
	{
		get
		{
			return m_isValid;
		}
		internal set
		{
			m_isValid = value;
		}
	}

	public bool IsDocumentTimeStamp
	{
		get
		{
			return m_documentTimeStamp;
		}
		internal set
		{
			m_documentTimeStamp = value;
		}
	}

	public X509Certificate2 Certificate
	{
		get
		{
			return m_certificate;
		}
		internal set
		{
			m_certificate = value;
		}
	}

	public PdfSignerCertificate[] SignerCertificates
	{
		get
		{
			if (m_signerCertificates == null)
			{
				m_signerCertificates = new List<PdfSignerCertificate>();
				if (m_signer != null && !IsDocumentTimeStamp)
				{
					m_signer.UpdateTimeStampSignerDetails(m_signerCertificates);
				}
			}
			return m_signerCertificates.ToArray();
		}
	}
}
