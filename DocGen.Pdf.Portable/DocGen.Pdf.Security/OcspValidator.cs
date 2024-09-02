using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class OcspValidator
{
	private List<X509RevocationResponse> m_responseCollection;

	internal RevocationStatus RevocationStatus;

	internal bool m_ocspResposne;

	internal bool m_ocspFromDSS;

	private bool m_isOcspEmbedded;

	internal PdfSignatureValidationResult result;

	internal bool IsOcspEmbedded
	{
		get
		{
			return m_isOcspEmbedded;
		}
		set
		{
			m_isOcspEmbedded = value;
		}
	}

	internal OcspValidator(List<X509RevocationResponse> responses)
	{
		m_responseCollection = responses;
	}

	internal RevocationStatus Validate(X509Certificate signerCertificate, X509Certificate issuerCertificate, DateTime signDate, bool isCrlEmbbed)
	{
		DateTime now = DateTime.Now;
		RevocationStatus status = RevocationStatus.None;
		if (m_responseCollection != null && m_responseCollection.Count > 0)
		{
			try
			{
				foreach (X509RevocationResponse item in m_responseCollection)
				{
					status = Validate(item, signerCertificate, issuerCertificate, signDate, isOcspEmbbed: true);
					SetStatus(status);
					status = Validate(item, signerCertificate, issuerCertificate, now, isOcspEmbbed: true);
					SetStatus(status);
				}
			}
			catch (Exception ex)
			{
				if (!m_ocspFromDSS)
				{
					throw ex;
				}
				m_ocspFromDSS = false;
			}
		}
		else if (!isCrlEmbbed && RevocationStatus != RevocationStatus.Good)
		{
			X509RevocationResponse ocspResponse = GetOcspResponse(signerCertificate, issuerCertificate);
			RevocationStatus = Validate(ocspResponse, signerCertificate, issuerCertificate, signDate, isOcspEmbbed: false);
			SetStatus(status);
			RevocationStatus = Validate(ocspResponse, signerCertificate, issuerCertificate, now, isOcspEmbbed: false);
			SetStatus(status);
		}
		return RevocationStatus;
	}

	private void SetStatus(RevocationStatus status)
	{
		switch (status)
		{
		case RevocationStatus.Good:
			RevocationStatus = status;
			return;
		case RevocationStatus.Unknown:
			if (RevocationStatus != RevocationStatus.Good)
			{
				RevocationStatus = status;
				return;
			}
			break;
		case RevocationStatus.None:
			return;
		}
		if (status == RevocationStatus.Revoked && RevocationStatus != RevocationStatus.Good && RevocationStatus != RevocationStatus.Unknown)
		{
			RevocationStatus = status;
		}
	}

	private RevocationStatus Validate(X509RevocationResponse response, X509Certificate signCertificate, X509Certificate issuerCertificate, DateTime signDate, bool isOcspEmbbed)
	{
		if (response == null)
		{
			return RevocationStatus.None;
		}
		OneTimeResponse[] responses = response.Responses;
		for (int i = 0; i < responses.Length; i++)
		{
			if (!isOcspEmbbed)
			{
				if (signCertificate.SerialNumber.IntValue != responses[i].CertificateID.SerialNumber.Value.IntValue)
				{
					continue;
				}
				DateTime dateTime = responses[i].NextUpdate?.ToDateTime() ?? responses[i].CurrentUpdate.ToDateTime().AddSeconds(180.0);
				if (signDate > dateTime)
				{
					if (i == responses.Length - 1)
					{
						m_ocspResposne = true;
					}
					continue;
				}
			}
			object certificateStatus = responses[i].CertificateStatus;
			if (issuerCertificate != null && (certificateStatus == null || (certificateStatus != null && (certificateStatus as OcspStatus).TagNumber == 0)))
			{
				CheckResponse(response, issuerCertificate);
				return RevocationStatus.Good;
			}
			if (certificateStatus != null)
			{
				if ((certificateStatus as OcspStatus).TagNumber == 1)
				{
					return RevocationStatus.Revoked;
				}
				return RevocationStatus.Unknown;
			}
		}
		return RevocationStatus.None;
	}

	private void CheckResponse(X509RevocationResponse ocspResp, X509Certificate issuerCert)
	{
		X509Certificate x509Certificate = null;
		if (CheckSignatureValidity(ocspResp, issuerCert))
		{
			x509Certificate = issuerCert;
			try
			{
				x509Certificate.Verify(issuerCert.GetPublicKey());
			}
			catch
			{
				if (ocspResp.Certificates.Length > 1)
				{
					issuerCert = ocspResp.Certificates[1];
				}
			}
		}
		if (x509Certificate == null)
		{
			X509Certificate[] certificates = ocspResp.Certificates;
			foreach (X509Certificate x509Certificate2 in certificates)
			{
				X509Certificate x509Certificate3;
				try
				{
					x509Certificate3 = x509Certificate2;
				}
				catch (Exception ex)
				{
					if (result != null)
					{
						PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
						ex2.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
						result.SignatureValidationErrors.Add(ex2);
					}
					continue;
				}
				IList list = null;
				try
				{
					list = x509Certificate3.GetExtendedKeyUsage();
					if (list != null && list.Contains("1.3.6.1.5.5.7.3.9") && CheckSignatureValidity(ocspResp, x509Certificate3))
					{
						x509Certificate = x509Certificate3;
						break;
					}
				}
				catch (Exception ex3)
				{
					if (result != null)
					{
						PdfSignatureValidationException ex4 = new PdfSignatureValidationException(ex3.Message);
						ex4.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
						result.SignatureValidationErrors.Add(ex4);
					}
				}
			}
			if (x509Certificate == null)
			{
				throw new Exception("OCSP response could not be verified");
			}
		}
		Asn1Octet extension = x509Certificate.GetExtension(new DerObjectID("1.3.6.1.5.5.7.48.1.5"));
		if (extension == null || extension == null || !IsOcspEmbedded)
		{
			x509Certificate.Verify(issuerCert.GetPublicKey());
			x509Certificate.CheckValidity();
		}
	}

	private bool CheckSignatureValidity(X509RevocationResponse ocspResp, X509Certificate responderCert)
	{
		try
		{
			return ocspResp.Verify(responderCert.GetPublicKey());
		}
		catch (Exception ex)
		{
			if (result != null)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
				ex2.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
				result.SignatureValidationErrors.Add(ex2);
			}
			return false;
		}
	}

	private X509RevocationResponse GetOcspResponse(X509Certificate signCert, X509Certificate issuerCert)
	{
		if (signCert == null && issuerCert == null)
		{
			return null;
		}
		X509RevocationResponse basicOCSPResponse = new Ocsp
		{
			result = result
		}.GetBasicOCSPResponse(signCert, issuerCert, null);
		if (basicOCSPResponse == null)
		{
			return null;
		}
		OneTimeResponse[] responses = basicOCSPResponse.Responses;
		for (int i = 0; i < responses.Length; i++)
		{
			if (responses[i].CertificateStatus == null)
			{
				return basicOCSPResponse;
			}
		}
		return null;
	}
}
