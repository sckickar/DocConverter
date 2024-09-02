using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

internal class PdfSignatureDictionary : IPdfWrapper
{
	private const string c_Type = "Sig";

	private const string ts_Type = "DocTimeStamp";

	private const string c_cmsFilterType = "adbe.pkcs7.detached";

	internal const string CadasFilterType = "ETSI.CAdES.detached";

	private const string rfc_FilterType = "ETSI.RFC3161";

	internal const string StoreFilterType = "adbe.pkcs7.sha1";

	private const string c_DocMdp = "DocMDP";

	private const string c_TransParam = "TransformParams";

	private uint c_EstimatedSize = 8192u;

	private PdfDocumentBase m_doc;

	private PdfSignature m_sig;

	private PdfCertificate m_cert;

	private int m_firstRangeLength;

	private int m_secondRangeIndex;

	private int m_startPositionByteRange;

	private int m_docDigestPosition;

	private int m_fieldsDigestPosition;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private bool m_isEndCertOnly = true;

	private Stream m_stream;

	private long[] m_range = new long[4];

	public bool Archive
	{
		get
		{
			return m_dictionary.Archive;
		}
		set
		{
			m_dictionary.Archive = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfSignatureDictionary(PdfDocumentBase doc, PdfSignature sig, PdfCertificate cert)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		if (sig == null)
		{
			throw new ArgumentNullException("sig");
		}
		m_doc = doc;
		m_sig = sig;
		m_cert = cert;
		doc.DocumentSaved += DocumentSaved;
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	internal PdfSignatureDictionary(PdfDocumentBase doc, PdfDictionary dic)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		m_doc = doc;
		m_dictionary = dic;
		doc.DocumentSaved += DocumentSaved;
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	internal PdfSignatureDictionary(PdfDocumentBase doc, PdfSignature sig)
	{
		if (doc == null)
		{
			throw new ArgumentNullException("doc");
		}
		if (sig == null)
		{
			throw new ArgumentNullException("sig");
		}
		m_doc = doc;
		m_sig = sig;
		doc.DocumentSaved += DocumentSaved;
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	private void AddRequiredItems()
	{
		if (m_sig.Certificated && AllowMDP())
		{
			AddReference();
		}
		AddType();
		AddDate();
		AddFilter();
		AddSubFilter();
	}

	private void AddOptionalItems()
	{
		AddReason();
		AddLocation();
		AddContactInfo();
		AddSignedName();
	}

	private void AddLocation()
	{
		if (m_sig.LocationInfo != null)
		{
			m_dictionary.SetProperty("Location", new PdfString(m_sig.LocationInfo));
		}
	}

	private void AddContactInfo()
	{
		if (m_sig.ContactInfo != null)
		{
			m_dictionary.SetProperty("ContactInfo", new PdfString(m_sig.ContactInfo));
		}
	}

	private void AddType()
	{
		if (m_cert != null)
		{
			m_dictionary.SetName("Type", "Sig");
		}
		else if (m_sig.TimeStampServer != null && m_sig.isTimeStampOnly)
		{
			m_dictionary.SetName("Type", "DocTimeStamp");
		}
		else
		{
			m_dictionary.SetName("Type", "Sig");
		}
	}

	private void AddSignedName()
	{
		if (m_sig != null && m_sig.m_signedName != null)
		{
			m_dictionary.SetString("Name", m_sig.m_signedName);
			PdfDictionary pdfDictionary = new PdfDictionary();
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary.SetName("Name", m_sig.m_signedName);
			pdfDictionary2.SetProperty("App", new PdfReferenceHolder(pdfDictionary));
			m_dictionary.SetProperty(new PdfName("Prop_Build"), new PdfReferenceHolder(pdfDictionary2));
		}
	}

	private void AddDate()
	{
		string text = $"D:{DateTime.Now:yyyyMMddHHmmss}";
		string text2 = DateTimeOffset.Now.ToString();
		char[] array = text2.Substring(text2.Length - 6).ToCharArray();
		text += array[0];
		text += array[1];
		text += array[2];
		text += "'";
		text += array[4];
		text += array[5];
		text += "'";
		m_dictionary.SetProperty("M", new PdfString(text));
	}

	private void AddReason()
	{
		if (m_sig.Reason != null)
		{
			m_dictionary.SetProperty("Reason", new PdfString(m_sig.Reason));
		}
	}

	private void AddFilter()
	{
		m_dictionary.SetName("Filter", "Adobe.PPKLite");
	}

	private void AddSubFilter()
	{
		if (m_cert != null)
		{
			if (m_cert.isStore)
			{
				if (!m_sig.Settings.HasChanged)
				{
					m_sig.Settings.DigestAlgorithm = DigestAlgorithm.SHA1;
				}
				m_dictionary.SetName("SubFilter", (m_sig.Settings.CryptographicStandard == CryptographicStandard.CADES) ? "ETSI.CAdES.detached" : "adbe.pkcs7.detached");
			}
			else
			{
				m_dictionary.SetName("SubFilter", (m_sig.Settings.CryptographicStandard == CryptographicStandard.CADES) ? "ETSI.CAdES.detached" : "adbe.pkcs7.detached");
			}
		}
		else if (m_sig.TimeStampServer != null && m_sig.isTimeStampOnly)
		{
			m_dictionary.SetName("SubFilter", "ETSI.RFC3161");
		}
		else
		{
			m_dictionary.SetName("SubFilter", (m_sig.Settings.CryptographicStandard == CryptographicStandard.CADES) ? "ETSI.CAdES.detached" : "adbe.pkcs7.detached");
		}
	}

	private void AddContents(IPdfWriter writer)
	{
		writer.Write("/Contents ");
		m_firstRangeLength = (int)writer.Position;
		uint num = 0u;
		if (m_sig != null && m_sig.EstimatedSignatureSize != 0)
		{
			num = m_sig.EstimatedSignatureSize;
		}
		else
		{
			num = c_EstimatedSize;
			if (m_sig != null && (m_sig.TimeStampServer != null || m_sig.ExternalSigner != null))
			{
				num = c_EstimatedSize + 4192;
			}
		}
		byte[] data = new byte[num * 2 + 2];
		writer.Write(data);
		m_secondRangeIndex = (int)writer.Position;
		writer.Write("\r\n");
	}

	private void AddRange(IPdfWriter writer)
	{
		writer.Write("/ByteRange [");
		m_startPositionByteRange = (int)writer.Position;
		_ = new byte[32];
		for (int i = 0; i < 32; i++)
		{
			writer.Write(" ");
		}
		writer.Write("]\r\n");
	}

	private bool AllowMDP()
	{
		IPdfPrimitive obj = PdfCrossTable.Dereference((PdfCrossTable.Dereference(m_doc.Catalog["Perms"]) as PdfDictionary)["DocMDP"]);
		return m_dictionary.Equals(obj);
	}

	private void AddDigest(IPdfWriter writer)
	{
		if (m_sig.Certificated && AllowMDP())
		{
			PdfDictionary catalog = writer.Document.Catalog;
			writer.Write(new PdfName("Reference"));
			writer.Write("[");
			writer.Write("<<");
			writer.Write("/TransformParams");
			PdfDictionary pdfDictionary = new PdfDictionary();
			int documentPermissions = (int)m_sig.DocumentPermissions;
			pdfDictionary["V"] = new PdfName("1.2");
			pdfDictionary["P"] = new PdfNumber(documentPermissions);
			pdfDictionary["Type"] = new PdfName("TransformParams");
			writer.Write(pdfDictionary);
			writer.Write(new PdfName("TransformMethod"));
			writer.Write(new PdfName("DocMDP"));
			writer.Write(new PdfName("Type"));
			writer.Write(new PdfName("SigRef"));
			writer.Write(new PdfName("DigestValue"));
			int value = (m_docDigestPosition = (int)writer.Position);
			writer.Write(new PdfString(new byte[16]));
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfNumber(value));
			pdfArray.Add(new PdfNumber(34));
			writer.Write(new PdfName("DigestLocation"));
			writer.Write(pdfArray);
			writer.Write(new PdfName("DigestMethod"));
			writer.Write(new PdfName("MD5"));
			writer.Write(new PdfName("Data"));
			PdfReferenceHolder pdfObject = new PdfReferenceHolder(catalog);
			writer.Write(" ");
			writer.Write(pdfObject);
			writer.Write(">>");
			writer.Write("<<");
			writer.Write(new PdfName("TransformParams"));
			pdfDictionary = new PdfDictionary();
			pdfDictionary["V"] = new PdfName("1.2");
			PdfArray pdfArray2 = new PdfArray();
			pdfArray2.Add(new PdfString(m_sig.Field.Name));
			pdfDictionary["Fields"] = pdfArray2;
			pdfDictionary["Type"] = new PdfName("TransformParams");
			pdfDictionary["Action"] = new PdfName("Include");
			writer.Write(pdfDictionary);
			writer.Write(new PdfName("TransformMethod"));
			writer.Write(new PdfName("FieldMDP"));
			writer.Write(new PdfName("Type"));
			writer.Write(new PdfName("SigRef"));
			writer.Write(new PdfName("DigestValue"));
			value = (m_fieldsDigestPosition = (int)writer.Position);
			writer.Write(new PdfString(new byte[16]));
			pdfArray = new PdfArray();
			pdfArray.Add(new PdfNumber(value));
			pdfArray.Add(new PdfNumber(34));
			writer.Write(new PdfName("DigestLocation"));
			writer.Write(pdfArray);
			writer.Write(new PdfName("DigestMethod"));
			writer.Write(new PdfName("MD5"));
			writer.Write(new PdfName("Data"));
			writer.Write(" ");
			writer.Write(new PdfReferenceHolder(catalog));
			writer.Write(">>");
			writer.Write("]");
			writer.Write(" ");
		}
		else if (m_sig != null && m_sig.Field != null && m_sig.Field.Dictionary.ContainsKey("Lock"))
		{
			PdfDictionary catalog2 = writer.Document.Catalog;
			writer.Write(new PdfName("Reference"));
			writer.Write("[");
			writer.Write("<<");
			writer.Write("/TransformParams");
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary2["V"] = new PdfName("1.2");
			if (((m_sig.Field.Dictionary["Lock"] as PdfReferenceHolder).Object as PdfDictionary)["Fields"] is PdfArray value2)
			{
				pdfDictionary2["Fields"] = value2;
			}
			pdfDictionary2["Type"] = new PdfName("TransformParams");
			pdfDictionary2["Action"] = new PdfName("Include");
			writer.Write(pdfDictionary2);
			writer.Write(new PdfName("TransformMethod"));
			writer.Write(new PdfName("FieldMDP"));
			writer.Write(new PdfName("Type"));
			writer.Write(new PdfName("SigRef"));
			writer.Write(new PdfName("DigestValue"));
			int value3 = (m_fieldsDigestPosition = (int)writer.Position);
			writer.Write(new PdfString(new byte[16]));
			PdfArray pdfArray3 = new PdfArray();
			pdfArray3.Add(new PdfNumber(value3));
			pdfArray3.Add(new PdfNumber(34));
			writer.Write(new PdfName("DigestLocation"));
			writer.Write(pdfArray3);
			writer.Write(new PdfName("DigestMethod"));
			writer.Write(new PdfName("MD5"));
			writer.Write(new PdfName("Data"));
			writer.Write(" ");
			writer.Write(new PdfReferenceHolder(catalog2));
			writer.Write(">>");
			writer.Write("]");
			writer.Write(" ");
		}
	}

	private void DocumentSaved(object sender, DocumentSavedEventArgs e)
	{
		if (sender == null)
		{
			throw new ArgumentNullException("sender");
		}
		if (e == null)
		{
			throw new ArgumentNullException("e");
		}
		bool enabled = m_doc.Security.Enabled;
		m_doc.Security.Enabled = false;
		PdfWriter writer = e.Writer;
		byte[] array = new byte[m_firstRangeLength];
		int num = (int)e.Writer.Length - m_secondRangeIndex;
		byte[] array2 = new byte[num];
		string str = "0 ";
		string text = m_firstRangeLength + " ";
		string text2 = m_secondRangeIndex + " ";
		string text3 = num.ToString();
		int startPosition = SaveRangeItem(writer, str, m_startPositionByteRange);
		startPosition = SaveRangeItem(writer, text, startPosition);
		startPosition = SaveRangeItem(writer, text2, startPosition);
		SaveRangeItem(e.Writer, text3, startPosition);
		m_range = new long[4]
		{
			0L,
			int.Parse(text),
			int.Parse(text2),
			int.Parse(text3)
		};
		m_stream = writer.ObtainStream();
		writer.Position = 0L;
		m_stream.Read(array, 0, array.Length);
		writer.Position = m_secondRangeIndex;
		m_stream.Read(array2, 0, array2.Length);
		_ = new byte[2][] { array, array2 };
		PdfSignatureEventArgs pdfSignatureEventArgs = null;
		if (m_sig != null && m_sig.ExternalSigner != null)
		{
			string text4 = PdfString.BytesToHex(GetPKCS7Content());
			e.Writer.Position = m_firstRangeLength;
			e.Writer.Write("<");
			e.Writer.Write(text4);
			int num2 = (m_secondRangeIndex - (int)e.Writer.Position) / 2;
			e.Writer.Write(PdfString.BytesToHex(new byte[num2]));
			e.Writer.Write(">");
			byte[] array3 = new byte[array.Length + array2.Length];
			array.CopyTo(array3, 0);
			array2.CopyTo(array3, array.Length);
		}
		else if (m_sig != null && m_sig.Certificate == null)
		{
			byte[] array4 = new byte[array.Length + array2.Length];
			array.CopyTo(array4, 0);
			array2.CopyTo(array4, array.Length);
			_ = new int[4]
			{
				0,
				int.Parse(text),
				int.Parse(text2),
				int.Parse(text3)
			};
			pdfSignatureEventArgs = new PdfSignatureEventArgs(array4);
			m_sig.OnComputeHash(pdfSignatureEventArgs);
		}
		if (pdfSignatureEventArgs != null && pdfSignatureEventArgs.SignedData != null)
		{
			string text5 = PdfString.BytesToHex(pdfSignatureEventArgs.SignedData);
			e.Writer.Position = m_firstRangeLength;
			e.Writer.Write("<");
			e.Writer.Write(text5);
			int num3 = (m_secondRangeIndex - (int)e.Writer.Position) / 2;
			e.Writer.Write(PdfString.BytesToHex(new byte[num3]));
			e.Writer.Write(">");
			byte[] array5 = new byte[array.Length + array2.Length];
			array.CopyTo(array5, 0);
			array2.CopyTo(array5, array.Length);
		}
		else if (m_sig != null && m_sig.ExternalSigner == null)
		{
			if (m_cert != null)
			{
				string text6 = string.Empty;
				if (m_cert.isStore)
				{
					text6 = GetPortableStoreCertificate();
				}
				else if (m_cert.isPkcs7Certificate)
				{
					text6 = PdfString.BytesToHex(GetPKCS7Content());
				}
				e.Writer.Position = m_firstRangeLength;
				e.Writer.Write("<");
				e.Writer.Write(text6);
				int num4 = (m_secondRangeIndex - (int)e.Writer.Position) / 2;
				e.Writer.Write(PdfString.BytesToHex(new byte[num4]));
				e.Writer.Write(">");
			}
			else if (m_cert == null && m_sig != null)
			{
				if (m_sig.TimeStampServer != null)
				{
					string empty = string.Empty;
					empty = PdfString.BytesToHex(GetPKCS7TimeStampContent());
					e.Writer.Position = m_firstRangeLength;
					e.Writer.Write("<");
					e.Writer.Write(empty);
					int num5 = (m_secondRangeIndex - (int)e.Writer.Position) / 2;
					e.Writer.Write(PdfString.BytesToHex(new byte[num5]));
					e.Writer.Write(">");
				}
			}
			else if (m_dictionary.ContainsKey("Contents"))
			{
				PdfString pdfString = m_dictionary["Contents"] as PdfString;
				e.Writer.Position = m_firstRangeLength;
				e.Writer.Write(pdfString.PdfEncode(writer.Document));
			}
			byte[] array6 = new byte[array.Length + array2.Length];
			array.CopyTo(array6, 0);
			array2.CopyTo(array6, array.Length);
		}
		m_doc.Security.Enabled = enabled;
	}

	private string GetPortableStoreCertificate()
	{
		X509CertificateParser x509CertificateParser = new X509CertificateParser();
		X509Certificate[] certChain = new X509Certificate[1] { x509CertificateParser.ReadCertificate(m_cert.X509Certificate.RawData) };
		string hashAlgorithm = "SHA-1";
		if (m_sig != null && m_sig.Settings != null && m_sig.Settings.DigestAlgorithm != DigestAlgorithm.RIPEMD160)
		{
			hashAlgorithm = GetDigestAlgorithm(m_sig.Settings.DigestAlgorithm);
		}
		X509Certificate2Signature x509Certificate2Signature = new X509Certificate2Signature(m_cert.X509Certificate, hashAlgorithm);
		string hashAlgorithm2 = x509Certificate2Signature.GetHashAlgorithm();
		PdfCmsSigner pdfCmsSigner = new PdfCmsSigner(null, certChain, hashAlgorithm2, hasRSAdata: false);
		IRandom underlyingSource = GetUnderlyingSource();
		IRandom[] array = new IRandom[m_range.Length / 2];
		for (int i = 0; i < m_range.Length; i += 2)
		{
			array[i / 2] = new WindowRandom(underlyingSource, m_range[i], m_range[i + 1]);
		}
		Stream data = new RandomStream(new RandomGroup(array));
		byte[] secondDigest = new MessageDigestAlgorithms().Digest(data, hashAlgorithm2);
		byte[] sequenceData = pdfCmsSigner.GetSequenceData(secondDigest, null, null, m_sig.Settings.CryptographicStandard);
		byte[] digest = x509Certificate2Signature.Sign(sequenceData);
		pdfCmsSigner.SetSignedData(digest, null, x509Certificate2Signature.GetEncryptionAlgorithm());
		byte[] array2 = null;
		array2 = pdfCmsSigner.Sign(secondDigest, m_sig.TimeStampServer, null, null, null, m_sig.Settings.CryptographicStandard, hashAlgorithm2);
		byte[] array3 = new byte[array2.Length];
		Array.Copy(array2, 0, array3, 0, array2.Length);
		return PdfString.BytesToHex(array3);
	}

	private byte[] GetPKCS7Content()
	{
		string hashAlgorithm = string.Empty;
		SignaturePrivateKey signaturePrivateKey = null;
		ICollection<byte[]> collection = null;
		byte[] ocsp = null;
		List<X509Certificate2> list = null;
		IPdfExternalSigner externalSigner = m_sig.ExternalSigner;
		if (externalSigner != null)
		{
			list = m_sig.ExternalCertificates;
		}
		ICollection<X509Certificate> collection2 = new List<X509Certificate>();
		if (externalSigner != null && list != null)
		{
			X509CertificateParser x509CertificateParser = new X509CertificateParser();
			foreach (X509Certificate2 item in list)
			{
				collection2.Add(x509CertificateParser.ReadCertificate(item.RawData));
			}
			List<X509Certificate> list2 = new List<X509Certificate>(collection2);
			EncryptionAlgorithms encryptionAlgorithms = new EncryptionAlgorithms();
			string encryptionAlgorithm = null;
			for (int i = 0; i < list2.Count; i++)
			{
				if (encryptionAlgorithms.GetAlgorithm(list2[i].SigAlgOid) == "ECDSA" || (list2[i].CertificateStructure != null && list2[i].CertificateStructure.SubjectPublicKeyInfo != null && list2[i].CertificateStructure.SubjectPublicKeyInfo.Algorithm != null && list2[i].CertificateStructure.SubjectPublicKeyInfo.Algorithm.ObjectID != null && encryptionAlgorithms.GetAlgorithm(list2[i].CertificateStructure.SubjectPublicKeyInfo.Algorithm.ObjectID.ID) == "ECDSA"))
				{
					encryptionAlgorithm = "ECDSA";
					break;
				}
			}
			list2 = null;
			string text = null;
			text = ((externalSigner.HashAlgorithm == null || !string.IsNullOrEmpty(externalSigner.HashAlgorithm)) ? externalSigner.HashAlgorithm : ((m_sig.Settings != null) ? GetDigestAlgorithm(m_sig.Settings.DigestAlgorithm) : "SHA-256"));
			SignaturePrivateKey signaturePrivateKey2 = new SignaturePrivateKey(text, encryptionAlgorithm);
			hashAlgorithm = signaturePrivateKey2.GetHashAlgorithm();
			signaturePrivateKey = signaturePrivateKey2;
		}
		else if (!m_cert.isStore)
		{
			string key = "";
			foreach (string item2 in m_cert.m_pkcs7Certificate.KeyEnumerable)
			{
				if (m_cert.m_pkcs7Certificate.IsKey(item2) && m_cert.m_pkcs7Certificate.GetKey(item2).Key.IsPrivate)
				{
					key = item2;
					break;
				}
			}
			KeyEntry key2 = m_cert.m_pkcs7Certificate.GetKey(key);
			X509Certificates[] certificateChain = m_cert.m_pkcs7Certificate.GetCertificateChain(key);
			foreach (X509Certificates x509Certificates in certificateChain)
			{
				collection2.Add(x509Certificates.Certificate);
			}
			if (collection2.Count > 1)
			{
				m_isEndCertOnly = false;
			}
			string hashAlgorithm2 = ((m_sig.Settings != null) ? GetDigestAlgorithm(m_sig.Settings.DigestAlgorithm) : "SHA-256");
			RsaPrivateKeyParam obj = key2.Key as RsaPrivateKeyParam;
			SignaturePrivateKey signaturePrivateKey3 = null;
			signaturePrivateKey3 = ((obj == null) ? new SignaturePrivateKey(key2.Key as ECPrivateKey, hashAlgorithm2) : new SignaturePrivateKey(key2.Key as RsaPrivateKeyParam, hashAlgorithm2));
			hashAlgorithm = signaturePrivateKey3.GetHashAlgorithm();
			signaturePrivateKey = signaturePrivateKey3;
		}
		PdfCmsSigner pdfCmsSigner = new PdfCmsSigner(null, collection2, hashAlgorithm, hasRSAdata: false);
		IRandom underlyingSource = GetUnderlyingSource();
		IRandom[] array = new IRandom[m_range.Length / 2];
		for (int k = 0; k < m_range.Length; k += 2)
		{
			array[k / 2] = new WindowRandom(underlyingSource, m_range[k], m_range[k + 1]);
		}
		Stream data = new RandomStream(new RandomGroup(array));
		byte[] secondDigest = new MessageDigestAlgorithms().Digest(data, hashAlgorithm);
		byte[] array2 = null;
		byte[] timeStampResponse = null;
		array2 = pdfCmsSigner.GetSequenceData(secondDigest, ocsp, collection, m_sig.Settings.CryptographicStandard);
		if (externalSigner != null)
		{
			byte[] array3 = externalSigner.Sign(array2, out timeStampResponse);
			if (array3 == null)
			{
				byte[] array4 = new byte[c_EstimatedSize];
				array3 = new byte[0];
				Array.Copy(array3, 0, array4, 0, array3.Length);
				return array4;
			}
			pdfCmsSigner.SetSignedData(array3, null, signaturePrivateKey.GetEncryptionAlgorithm());
		}
		else if (!m_cert.isStore)
		{
			byte[] digest = signaturePrivateKey.Sign(array2);
			pdfCmsSigner.SetSignedData(digest, null, signaturePrivateKey.GetEncryptionAlgorithm());
		}
		byte[] array5 = null;
		array5 = pdfCmsSigner.Sign(secondDigest, m_sig.TimeStampServer, timeStampResponse, ocsp, collection, m_sig.Settings.CryptographicStandard, hashAlgorithm);
		byte[] array6 = new byte[array5.Length];
		Array.Copy(array5, 0, array6, 0, array5.Length);
		return array6;
	}

	private string GetDigestAlgorithm(DigestAlgorithm digest)
	{
		return digest switch
		{
			DigestAlgorithm.SHA1 => "SHA-1", 
			DigestAlgorithm.SHA384 => "SHA-384", 
			DigestAlgorithm.SHA512 => "SHA-512", 
			DigestAlgorithm.RIPEMD160 => "RIPEMD160", 
			_ => "SHA-256", 
		};
	}

	private byte[] GetPKCS7TimeStampContent()
	{
		SignaturePrivateKey signaturePrivateKey = new SignaturePrivateKey((m_sig.Settings != null) ? GetDigestAlgorithm(m_sig.Settings.DigestAlgorithm) : "SHA-256", null);
		string hashAlgorithm = signaturePrivateKey.GetHashAlgorithm();
		PdfCmsSigner pdfCmsSigner = new PdfCmsSigner(hashAlgorithm, hasRSAdata: false);
		IRandom underlyingSource = GetUnderlyingSource();
		IRandom[] array = new IRandom[m_range.Length / 2];
		for (int i = 0; i < m_range.Length; i += 2)
		{
			array[i / 2] = new WindowRandom(underlyingSource, m_range[i], m_range[i + 1]);
		}
		Stream data = new RandomStream(new RandomGroup(array));
		byte[] array2 = new MessageDigestAlgorithms().Digest(data, hashAlgorithm);
		pdfCmsSigner.SetSignedData(array2, null, signaturePrivateKey.GetEncryptionAlgorithm());
		byte[] array3 = null;
		array3 = pdfCmsSigner.GetEncodedTimestamp(array2, m_sig.TimeStampServer);
		byte[] array4 = new byte[array3.Length];
		Array.Copy(array3, 0, array4, 0, array3.Length);
		return array4;
	}

	private IRandom GetUnderlyingSource()
	{
		byte[] array = new byte[m_stream.Length];
		m_stream.Position = 0L;
		m_stream.Read(array, 0, (int)m_stream.Length);
		return new RandomArray(array);
	}

	private int SaveRangeItem(PdfWriter writer, string str, int startPosition)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		writer.Position = startPosition;
		writer.ObtainStream().Write(bytes, 0, bytes.Length);
		return startPosition + str.Length;
	}

	private void AddReference()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		PdfArray pdfArray = new PdfArray();
		int documentPermissions = (int)m_sig.DocumentPermissions;
		pdfDictionary["V"] = new PdfName("1.2");
		pdfDictionary["P"] = new PdfNumber(documentPermissions);
		pdfDictionary["Type"] = new PdfName("TransformParams");
		pdfDictionary2["TransformMethod"] = new PdfName("DocMDP");
		pdfDictionary2["Type"] = new PdfName("SigRef");
		pdfDictionary2["TransformParams"] = pdfDictionary;
		pdfArray.Add(pdfDictionary2);
		m_dictionary.SetProperty("Reference", pdfArray);
	}

	private int CreateAsn1TspRequest(byte[] sha1Hash, Stream input)
	{
		byte[] array = new byte[18]
		{
			48, 39, 2, 1, 1, 48, 31, 48, 7, 6,
			5, 43, 14, 3, 2, 26, 4, 20
		};
		byte[] array2 = new byte[3] { 1, 1, 255 };
		input.Write(array, 0, array.Length);
		input.Write(sha1Hash, 0, sha1Hash.Length);
		input.Write(array2, 0, array2.Length);
		return array.Length + sha1Hash.Length + array2.Length;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs args)
	{
		bool enabled = m_doc.Security.Enabled;
		m_dictionary.Encrypt = enabled;
		if (m_cert != null)
		{
			AddRequiredItems();
			AddOptionalItems();
		}
		else if (m_sig != null)
		{
			if (m_sig.TimeStampServer != null && m_sig.isTimeStampOnly)
			{
				AddRequiredItems();
				AddOptionalItems();
			}
			else
			{
				AddRequiredItems();
				AddOptionalItems();
			}
		}
		m_doc.Security.Enabled = false;
		AddContents(args.Writer);
		AddRange(args.Writer);
		if (m_sig != null)
		{
			PdfDictionary pdfDictionary = null;
			PdfArray pdfArray = null;
			if (m_sig != null && m_sig.Field != null && m_sig.Field.Dictionary.ContainsKey("Lock") && PdfCrossTable.Dereference(m_sig.Field.Dictionary["Lock"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Fields"))
			{
				pdfArray = PdfCrossTable.Dereference(pdfDictionary2["Fields"]) as PdfArray;
			}
			if (m_sig.Certificated || (pdfArray != null && pdfArray.Count > 0))
			{
				AddDigest(args.Writer);
			}
		}
		m_doc.Security.Enabled = enabled;
	}
}
