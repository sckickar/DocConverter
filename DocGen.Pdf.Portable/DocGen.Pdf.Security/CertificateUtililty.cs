using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Security;

internal class CertificateUtililty
{
	internal PdfSignatureValidationResult result;

	internal string GetCrlUrl(X509Certificate certificate)
	{
		try
		{
			Asn1 extensionValue = GetExtensionValue(certificate, X509Extensions.CrlDistributionPoints.ID);
			if (extensionValue == null)
			{
				return null;
			}
			RevocationDistribution[] distributionPoints = new RevocationPointList().GetCrlPointList(extensionValue).GetDistributionPoints();
			for (int i = 0; i < distributionPoints.Length; i++)
			{
				RevocationDistributionType distributionPointName = distributionPoints[i].DistributionPointName;
				if (distributionPointName.PointType != 0)
				{
					continue;
				}
				OcspTag[] names = ((RevocationName)distributionPointName.Name).Names;
				foreach (OcspTag ocspTag in names)
				{
					if (ocspTag.TagNumber == 6)
					{
						return DerAsciiString.GetAsciiString((Asn1Tag)ocspTag.GetAsn1(), isExplicit: false).GetString();
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (result != null)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
				ex2.ExceptionType = PdfSignatureValidationExceptionType.CRL;
				result.SignatureValidationErrors.Add(ex2);
			}
		}
		return null;
	}

	internal string GetOcspUrl(X509Certificate certificate)
	{
		try
		{
			Asn1 extensionValue = GetExtensionValue(certificate, X509Extensions.AuthorityInfoAccess.ID);
			if (extensionValue == null)
			{
				return null;
			}
			Asn1Sequence asn1Sequence = (Asn1Sequence)extensionValue;
			for (int i = 0; i < asn1Sequence.Count; i++)
			{
				Asn1Sequence asn1Sequence2 = (Asn1Sequence)asn1Sequence[i];
				if (asn1Sequence2.Count == 2 && asn1Sequence2[0] is DerObjectID && ((DerObjectID)asn1Sequence2[0]).ID.Equals("1.3.6.1.5.5.7.48.1"))
				{
					string stringFromGeneralName = GetStringFromGeneralName((Asn1)asn1Sequence2[1]);
					if (stringFromGeneralName == null)
					{
						return "";
					}
					return stringFromGeneralName;
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

	private Asn1 GetExtensionValue(X509Certificate certificate, string id)
	{
		byte[] array = null;
		Asn1Octet extension = certificate.GetExtension(new DerObjectID(id));
		if (extension != null)
		{
			array = extension.GetDerEncoded();
		}
		if (array == null)
		{
			return null;
		}
		return new Asn1Stream(new MemoryStream(((Asn1Octet)new Asn1Stream(new MemoryStream(array)).ReadAsn1()).GetOctets())).ReadAsn1();
	}

	private string GetStringFromGeneralName(Asn1 names)
	{
		byte[] octets = Asn1Octet.GetOctetString((Asn1Tag)names, isExplicit: false).GetOctets();
		return Encoding.UTF8.GetString(octets, 0, octets.Length);
	}

	internal List<string> GetCrlUrls(X509Certificate certificate)
	{
		List<string> list = new List<string>();
		try
		{
			Asn1 extensionValue = GetExtensionValue(certificate, X509Extensions.CrlDistributionPoints.ID);
			if (extensionValue == null)
			{
				return null;
			}
			RevocationDistribution[] distributionPoints = new RevocationPointList().GetCrlPointList(extensionValue).GetDistributionPoints();
			for (int i = 0; i < distributionPoints.Length; i++)
			{
				RevocationDistributionType distributionPointName = distributionPoints[i].DistributionPointName;
				if (distributionPointName.PointType != 0)
				{
					continue;
				}
				OcspTag[] names = ((RevocationName)distributionPointName.Name).Names;
				foreach (OcspTag ocspTag in names)
				{
					if (ocspTag.TagNumber == 6)
					{
						string @string = DerAsciiString.GetAsciiString((Asn1Tag)ocspTag.GetAsn1(), isExplicit: false).GetString();
						if (IsValidUrl(@string) && (@string.ToLower().EndsWith(".crl") || !string.IsNullOrEmpty(Path.GetExtension(@string))))
						{
							list.Add(@string);
						}
					}
				}
			}
			return list;
		}
		catch (Exception ex)
		{
			if (result != null)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(ex.Message);
				ex2.ExceptionType = PdfSignatureValidationExceptionType.CRL;
				result.SignatureValidationErrors.Add(ex2);
			}
		}
		return null;
	}

	private bool IsValidUrl(string url)
	{
		if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
		{
			if (!(uri.Scheme == Uri.UriSchemeHttp))
			{
				return uri.Scheme == Uri.UriSchemeHttps;
			}
			return true;
		}
		return false;
	}
}
