using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class OcspResponseHelper
{
	private OcspResponse m_response;

	internal PdfSignatureValidationResult result;

	internal int Status
	{
		get
		{
			if (m_response != null)
			{
				return m_response.ResponseStatus.Value.IntValue;
			}
			return -1;
		}
	}

	public OcspResponseHelper(Stream stream)
		: this(new Asn1Stream(stream))
	{
	}

	private OcspResponseHelper(Asn1Stream stream)
	{
		try
		{
			OcspResponse ocspResponse = new OcspResponse();
			m_response = ocspResponse.GetOcspResponse(stream.ReadAsn1());
		}
		catch (Exception)
		{
			throw new IOException("Invalid response");
		}
	}

	internal object GetResponseObject()
	{
		RevocationResponseBytes responseBytes = m_response.ResponseBytes;
		if (responseBytes == null)
		{
			return null;
		}
		if (responseBytes.ResponseType.Equals(OcspConstants.OcspBasic))
		{
			try
			{
				return new X509RevocationResponse(new OcspHelper().GetOcspStructure(Asn1.FromByteArray(responseBytes.Response.GetOctets())));
			}
			catch (Exception ex)
			{
				if (result != null)
				{
					PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
					ex2.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
					result.SignatureValidationErrors.Add(ex2);
				}
				throw new Exception("Invalid response detected");
			}
		}
		return responseBytes.Response;
	}
}
