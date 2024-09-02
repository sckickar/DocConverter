using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

public class PdfDocumentSecureStore
{
	private PdfCatalog m_catalog;

	private X509Certificate2[] certificates;

	public X509Certificate2[] Certificates
	{
		get
		{
			if (certificates == null)
			{
				ReadCertificates();
			}
			return certificates;
		}
	}

	internal PdfDocumentSecureStore(PdfCatalog catalog)
	{
		if (catalog == null)
		{
			throw new ArgumentNullException("catalog");
		}
		m_catalog = catalog;
	}

	private void ReadCertificates()
	{
		List<X509Certificate2> list = new List<X509Certificate2>();
		if (m_catalog != null && m_catalog["DSS"] != null && PdfCrossTable.Dereference(m_catalog["DSS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Certs") && PdfCrossTable.Dereference(pdfDictionary["Certs"]) is PdfArray pdfArray)
		{
			X509CertificateParser x509CertificateParser = new X509CertificateParser();
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfStream pdfStream)
				{
					byte[] decompressedData = pdfStream.GetDecompressedData();
					X509Certificate x509Certificate = x509CertificateParser.ReadCertificate(decompressedData);
					if (x509Certificate != null)
					{
						list.Add(new X509Certificate2(x509Certificate.GetEncoded()));
					}
				}
			}
		}
		if (list.Count > 0)
		{
			certificates = list.ToArray();
		}
	}
}
