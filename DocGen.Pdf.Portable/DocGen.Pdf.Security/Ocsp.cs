using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace DocGen.Pdf.Security;

internal class Ocsp
{
	private byte[] bytes;

	private Stream m_stream;

	private ManualResetEvent allDone = new ManualResetEvent(initialState: false);

	internal PdfSignatureValidationResult result;

	internal Ocsp()
	{
	}

	internal X509RevocationResponse GetBasicOCSPResponse(X509Certificate checkCertificate, X509Certificate rootCertificate, string url)
	{
		try
		{
			OcspResponseHelper ocspResponse = GetOcspResponse(checkCertificate, rootCertificate, url);
			if (ocspResponse != null)
			{
				ocspResponse.result = result;
			}
			if (ocspResponse == null)
			{
				return null;
			}
			if (ocspResponse.Status != 0)
			{
				return null;
			}
			return (X509RevocationResponse)ocspResponse.GetResponseObject();
		}
		catch (Exception ex)
		{
			if (result != null)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
				ex2.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
				result.SignatureValidationErrors.Add(ex2);
			}
		}
		return null;
	}

	internal byte[] GetEncodedOcspRspnose(X509Certificate checkCert, X509Certificate rootCert, string url)
	{
		try
		{
			X509RevocationResponse basicOCSPResponse = GetBasicOCSPResponse(checkCert, rootCert, url);
			if (basicOCSPResponse != null)
			{
				OneTimeResponse[] responses = basicOCSPResponse.Responses;
				if (responses.Length == 1 && responses[0].CertificateStatus == CerificateStatus.Good)
				{
					return basicOCSPResponse.EncodedBytes;
				}
			}
		}
		catch (Exception ex)
		{
			if (result != null)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
				ex2.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
				result.SignatureValidationErrors.Add(ex2);
			}
		}
		return null;
	}

	private OcspRequestHelper GenerateOCSPRequest(X509Certificate issuerCertificate, Number serialNumber)
	{
		CertificateIdentity id = new CertificateIdentity("1.3.14.3.2.26", issuerCertificate, serialNumber);
		OcspRequestCreator ocspRequestCreator = new OcspRequestCreator();
		ocspRequestCreator.AddRequest(id);
		ocspRequestCreator.SetRequestExtensions(new X509Extensions(new Dictionary<DerObjectID, X509Extension> { [OcspConstants.OcspNonce] = new X509Extension(critical: false, new DerOctet(new DerOctet(PdfEncryption.CreateDocumentId()).GetEncoded())) }));
		return ocspRequestCreator.Generate();
	}

	private OcspResponseHelper GetOcspResponse(X509Certificate checkCertificate, X509Certificate rootCertificate, string url)
	{
		if (checkCertificate == null || rootCertificate == null)
		{
			return null;
		}
		if (url == null)
		{
			url = new CertificateUtililty
			{
				result = result
			}.GetOcspUrl(checkCertificate);
		}
		if (url == null)
		{
			return null;
		}
		byte[] encoded = GenerateOCSPRequest(rootCertificate, checkCertificate.SerialNumber).GetEncoded();
		bytes = encoded;
		Stream timeStampResponse = GetTimeStampResponse(url);
		timeStampResponse.Position = 0L;
		return new OcspResponseHelper(timeStampResponse);
	}

	internal Stream GetTimeStampResponse(string url)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
		httpWebRequest.ContentType = "application/ocsp-request";
		httpWebRequest.Method = "POST";
		if (result == null)
		{
			httpWebRequest.Timeout = 5000;
		}
		httpWebRequest.BeginGetRequestStream(GetRequestStreamCallback, httpWebRequest);
		allDone.WaitOne();
		allDone.Reset();
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[1024];
		int num = 0;
		while ((num = m_stream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream.Write(array, 0, num);
		}
		return memoryStream;
	}

	private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
		if (result == null)
		{
			httpWebRequest.Timeout = 5000;
		}
		Stream stream = httpWebRequest.EndGetRequestStream(asynchronousResult);
		stream.Write(bytes, 0, bytes.Length);
		stream.Close();
		httpWebRequest.BeginGetResponse(GetResponseCallback, httpWebRequest);
		allDone.WaitOne();
	}

	private void GetResponseCallback(IAsyncResult asynchronousResult)
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)asynchronousResult.AsyncState;
		if (result == null)
		{
			httpWebRequest.Timeout = 5000;
		}
		try
		{
			Stream responseStream = ((HttpWebResponse)httpWebRequest.EndGetResponse(asynchronousResult)).GetResponseStream();
			m_stream = responseStream;
		}
		catch (Exception ex)
		{
			if (result != null)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
				ex2.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
				result.SignatureValidationErrors.Add(ex2);
			}
		}
		allDone.Set();
	}
}
