using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

internal class PdfCmsSigner
{
	private enum DistinguishedNameSeparator
	{
		InitialSeparator,
		QuoteSpearator
	}

	private const string rfc_FilterType = "ETSI.RFC3161";

	private int m_version = 1;

	private int m_signerVersion = 1;

	private string m_digestAlgorithmOid;

	private IMessageDigest m_messageDigest;

	private Dictionary<string, object> m_digestOid;

	private string m_encryptionAlgorithmOid;

	private byte[] m_signedData;

	private byte[] m_signedRSAData;

	private ISigner m_signer;

	private byte[] m_digest;

	private byte[] m_rsaData;

	private List<X509Certificate> m_certificates;

	private X509Certificate m_signCert;

	private string m_encryptionAlgorithm;

	private bool m_isTimeStamp;

	private byte[] m_digestAttributeData;

	private byte[] m_sigAttribute;

	private byte[] m_sigAttributeDer;

	private byte[] m_timestampCertificates;

	private IMessageDigest m_encodedMessageDigest;

	private MessageDigestAlgorithms m_digestAlgorithm = new MessageDigestAlgorithms();

	private MessageDigestFinder m_digestFinder = new MessageDigestFinder();

	private X509RevocationResponse m_basicOcspResponse;

	private TimeStampToken m_timeStampToken;

	private readonly string m_ocspID = "1.3.6.1.5.5.7.48.1.1";

	private string m_hashAlgorithm;

	private List<X509RevocationResponse> ocspResponseCollection = new List<X509RevocationResponse>();

	internal bool m_timeStampDocument;

	private string m_storeFilterType = "adbe.pkcs7.sha1";

	private PdfSignature m_signature;

	private List<X509RevocationResponse> m_embbedOCSPs;

	private List<X509RevocationResponse> m_dssOCSPs;

	internal Asn1Sequence m_crlSequence;

	internal List<byte[]> m_crlByteCollection;

	internal List<byte[]> m_ocspByteCollection;

	internal LtvVerificationInfo m_ltvVerificationInfo;

	internal X509CertificateCollection rootCertificates;

	private List<string> m_foundOCSPs;

	private List<string> m_foundCRLs;

	private Dictionary<string, List<Dictionary<string, List<string>>>> m_revocationCertURIs;

	private List<string> intermediateIssueNames;

	private bool isLtvEnabled = true;

	private List<string> m_certSerialNumbers;

	private X509Certificate2 m_certificate2;

	private List<X509Certificate> endCertificates = new List<X509Certificate>();

	internal List<X509Certificate> CertificateList => m_certificates;

	internal X509Certificate SignerCertificate => m_signCert;

	internal PdfSignature Signature
	{
		get
		{
			return m_signature;
		}
		set
		{
			m_signature = value;
		}
	}

	internal IMessageDigest MessageDigest => m_digestFinder.GetDigest(HashAlgorithm);

	internal string HashAlgorithm
	{
		get
		{
			if (m_hashAlgorithm == null)
			{
				MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
				m_hashAlgorithm = messageDigestAlgorithms.GetDigest(m_digestAlgorithmOid);
			}
			return m_hashAlgorithm;
		}
	}

	internal string MessageDigestAlgorithm => HashAlgorithm + "with" + EncryptionAlgorithm;

	public string EncryptionAlgorithm
	{
		get
		{
			if (m_encryptionAlgorithm == null)
			{
				EncryptionAlgorithms encryptionAlgorithms = new EncryptionAlgorithms();
				m_encryptionAlgorithm = encryptionAlgorithms.GetAlgorithm(m_encryptionAlgorithmOid);
			}
			return m_encryptionAlgorithm;
		}
	}

	internal PdfCmsSigner(ICipherParam privateKey, ICollection<X509Certificate> certChain, string hashAlgorithm, bool hasRSAdata)
	{
		m_digestAlgorithmOid = m_digestAlgorithm.GetAllowedDigests(hashAlgorithm);
		if (m_digestAlgorithmOid == null)
		{
			throw new ArgumentException("Unknown Hash Algorithm", hashAlgorithm);
		}
		m_version = (m_signerVersion = 1);
		m_certificates = new List<X509Certificate>(certChain);
		m_digestOid = new Dictionary<string, object>();
		m_digestOid[m_digestAlgorithmOid] = null;
		if (m_certificates.Count > 0)
		{
			m_signCert = m_certificates[0];
		}
		if (privateKey != null)
		{
			if (!(privateKey is RsaKeyParam))
			{
				throw new ArgumentException("Unknown key algorithm ", privateKey.ToString());
			}
			m_encryptionAlgorithmOid = "1.2.840.113549.1.1.1";
		}
		if (hasRSAdata)
		{
			m_rsaData = new byte[0];
			m_messageDigest = MessageDigest;
		}
	}

	internal PdfCmsSigner(string hashAlgorithm, bool hasRSAdata)
	{
		MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
		m_digestAlgorithmOid = messageDigestAlgorithms.GetAllowedDigests(hashAlgorithm);
		if (m_digestAlgorithmOid == null)
		{
			throw new ArgumentException("Unknown Hash Algorithm", hashAlgorithm);
		}
		m_version = (m_signerVersion = 1);
		m_digestOid = new Dictionary<string, object>();
		m_digestOid[m_digestAlgorithmOid] = null;
		m_encryptionAlgorithmOid = "1.2.840.113549.1.1.1";
		if (hasRSAdata)
		{
			m_rsaData = new byte[0];
			m_messageDigest = MessageDigest;
		}
	}

	internal PdfCmsSigner(byte[] contentByte, byte[] certBytes)
	{
		ICollection collection = new X509CertificateParser().ReadCertificates(certBytes);
		Asn1 asn = new Asn1Stream(contentByte).ReadAsn1();
		IList<X509Certificate> list = new List<X509Certificate>();
		if (asn != null && asn is Asn1Octet)
		{
			foreach (X509Certificate item in collection)
			{
				if (m_signCert == null)
				{
					m_signCert = item;
				}
				list.Add(item);
			}
		}
		if (m_signCert != null)
		{
			m_certificates = GetCertificateChain(m_signCert, list);
			ISigner signer = new SignerUtilities().GetSigner("SHA-1withRSA");
			signer.Initialize(isSigning: false, m_signCert.GetPublicKey());
			m_digestAlgorithmOid = "1.2.840.10040.4.3";
			m_encryptionAlgorithmOid = "1.3.36.3.3.1.2";
			m_digest = (asn as Asn1Octet).GetOctets();
			m_signer = signer;
			list.Clear();
		}
	}

	internal PdfCmsSigner(byte[] bytes, string subFilter, PdfSignatureValidationOptions options)
	{
		m_digestAlgorithm = new MessageDigestAlgorithms();
		m_digestFinder = new MessageDigestFinder();
		m_isTimeStamp = subFilter == "ETSI.RFC3161";
		_ = subFilter == "ETSI.CAdES.detached";
		Asn1 asn = new Asn1Stream(bytes).ReadAsn1();
		if (asn == null || !(asn is Asn1Sequence))
		{
			return;
		}
		Asn1Sequence asn1Sequence = asn as Asn1Sequence;
		if (!(((DerObjectID)asn1Sequence[0]).ID == "1.2.840.113549.1.7.2"))
		{
			return;
		}
		Asn1Sequence asn1Sequence2 = (Asn1Sequence)((Asn1Tag)asn1Sequence[1]).GetObject();
		_ = ((DerInteger)asn1Sequence2[0]).Value.IntValue;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		IEnumerator enumerator = ((Asn1Set)asn1Sequence2[1]).GetEnumerator();
		while (enumerator.MoveNext())
		{
			DerObjectID derObjectID = (DerObjectID)((Asn1Sequence)enumerator.Current)[0];
			dictionary[derObjectID.ID] = null;
		}
		X509CertificateParser x509CertificateParser = new X509CertificateParser();
		IList<X509Certificate> list = new List<X509Certificate>();
		foreach (X509Certificate item in x509CertificateParser.ReadCertificates(bytes))
		{
			list.Add(item);
		}
		Asn1Sequence asn1Sequence3 = (Asn1Sequence)asn1Sequence2[2];
		if (asn1Sequence3.Count > 1)
		{
			Asn1Octet asn1Octet = (Asn1Octet)((Asn1Tag)asn1Sequence3[1]).GetObject();
			m_rsaData = asn1Octet.GetOctets();
		}
		int i;
		for (i = 3; asn1Sequence2[i] is Asn1Tag; i++)
		{
		}
		Asn1Sequence asn1Sequence4 = (Asn1Sequence)((Asn1Set)asn1Sequence2[i])[0];
		m_signerVersion = ((DerInteger)asn1Sequence4[0]).Value.IntValue;
		Asn1Sequence obj = (Asn1Sequence)asn1Sequence4[1];
		X509Name name = X509Name.GetName(obj[0]);
		Number value = ((DerInteger)obj[1]).Value;
		foreach (X509Certificate item2 in list)
		{
			if (name.Equivalent(item2.IssuerDN) && value.Equals(item2.SerialNumber))
			{
				m_signCert = item2;
				break;
			}
		}
		if (m_signCert == null)
		{
			return;
		}
		m_certificates = GetCertificateChain(m_signCert, list);
		m_digestAlgorithmOid = ((DerObjectID)((Asn1Sequence)asn1Sequence4[2])[0]).ID;
		i = 3;
		if (asn1Sequence4[i] is Asn1Tag)
		{
			Asn1Set asn1Set = Asn1Set.GetAsn1Set((Asn1Tag)asn1Sequence4[i], isExplicit: false);
			m_sigAttribute = asn1Set.GetEncoded();
			m_sigAttributeDer = asn1Set.GetEncoded("DER");
			for (int j = 0; j < asn1Set.Count; j++)
			{
				Asn1Sequence asn1Sequence5 = (Asn1Sequence)asn1Set[j];
				string iD = ((DerObjectID)asn1Sequence5[0]).ID;
				if (iD.Equals(PKCSOIDs.Pkcs9AtMessageDigest.ID))
				{
					Asn1Set asn1Set2 = (Asn1Set)asn1Sequence5[1];
					m_digestAttributeData = ((DerOctet)asn1Set2[0]).GetOctets();
				}
				else
				{
					if (!iD.Equals(PKCSOIDs.AdobeRevocation.ID))
					{
						continue;
					}
					if (options != null && options.OCSPResponseData != null)
					{
						foreach (byte[] oCSPResponseDatum in options.OCSPResponseData)
						{
							Asn1Stream asn1Stream = new Asn1Stream(oCSPResponseDatum);
							OcspHelper ocspHelper = new OcspHelper();
							ocspResponseCollection.Add(new X509RevocationResponse(ocspHelper.GetOcspStructure(asn1Stream.ReadAsn1())));
						}
						continue;
					}
					Asn1Sequence asn1Sequence6 = (Asn1Sequence)((Asn1Set)asn1Sequence5[1])[0];
					for (int k = 0; k < asn1Sequence6.Count; k++)
					{
						Asn1Tag asn1Tag = (Asn1Tag)asn1Sequence6[k];
						if (asn1Tag.TagNumber == 1)
						{
							if (m_embbedOCSPs == null)
							{
								m_embbedOCSPs = new List<X509RevocationResponse>();
							}
							GetOcsp((Asn1Sequence)asn1Tag.GetObject(), dss: false);
						}
						if (asn1Tag.TagNumber == 0)
						{
							m_crlSequence = (Asn1Sequence)asn1Tag.GetObject();
						}
					}
				}
			}
			i++;
		}
		m_encryptionAlgorithmOid = ((DerObjectID)((Asn1Sequence)asn1Sequence4[i++])[0]).ID;
		m_digest = ((Asn1Octet)asn1Sequence4[i++]).GetOctets();
		if (i < asn1Sequence4.Count && asn1Sequence4[i] is DerTag)
		{
			TimeStampElement timeStampElement = new TimeStampElements(Asn1Set.GetAsn1Set((Asn1Tag)asn1Sequence4[i], isExplicit: false))[PKCSOIDs.Pkcs9SignatureTimeStamp];
			if (timeStampElement != null && timeStampElement.Values.Count > 0)
			{
				Asn1Sequence sequence = Asn1Sequence.GetSequence(timeStampElement.Values[0]);
				m_timestampCertificates = sequence.GetEncoded();
				ContentInformation information = ContentInformation.GetInformation(sequence);
				m_timeStampToken = new TimeStampToken(new CmsSignedDetails(information));
			}
		}
		if (m_isTimeStamp)
		{
			ContentInformation information2 = ContentInformation.GetInformation(asn1Sequence);
			m_timeStampToken = new TimeStampToken(new CmsSignedDetails(information2));
			string messageImprintAlgOid = m_timeStampToken.TimeStampInformation.MessageImprintAlgOid;
			m_messageDigest = m_digestFinder.GetDigest(messageImprintAlgOid);
			return;
		}
		if (m_rsaData != null || m_digestAttributeData != null)
		{
			if (subFilter == m_storeFilterType)
			{
				m_messageDigest = m_digestFinder.GetDigest("SHA1");
			}
			else
			{
				m_messageDigest = m_digestFinder.GetDigest(HashAlgorithm);
			}
			m_encodedMessageDigest = MessageDigest;
		}
		m_signer = InitializeSignature();
	}

	internal void SetSignedData(byte[] digest, byte[] RSAdata, string digestEncryptionAlgorithm)
	{
		m_signedData = digest;
		m_signedRSAData = RSAdata;
		if (digestEncryptionAlgorithm == null)
		{
			return;
		}
		if (digestEncryptionAlgorithm.Equals("RSA"))
		{
			m_encryptionAlgorithmOid = "1.2.840.113549.1.1.1";
			return;
		}
		if (digestEncryptionAlgorithm.Equals("DSA"))
		{
			m_encryptionAlgorithmOid = "1.2.840.10040.4.1";
			return;
		}
		if (digestEncryptionAlgorithm.Equals("ECDSA"))
		{
			string text = ((HashAlgorithm != null) ? HashAlgorithm : "SHA1").ToLower();
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			default:
				return;
			case 6:
				switch (text[3])
				{
				default:
					return;
				case '2':
					break;
				case '3':
					goto IL_0105;
				case '5':
					goto IL_0113;
				case '4':
					return;
				}
				if (!(text == "sha256"))
				{
					return;
				}
				goto IL_0157;
			case 7:
				switch (text[4])
				{
				default:
					return;
				case '2':
					break;
				case '3':
					goto IL_012f;
				case '5':
					goto IL_013d;
				case '4':
					return;
				}
				if (!(text == "sha-256"))
				{
					return;
				}
				goto IL_0157;
			case 4:
				if (!(text == "sha1"))
				{
					return;
				}
				goto IL_014b;
			case 5:
				{
					if (!(text == "sha-1"))
					{
						return;
					}
					goto IL_014b;
				}
				IL_014b:
				m_encryptionAlgorithmOid = "1.2.840.10045.4.1";
				return;
				IL_013d:
				if (!(text == "sha-512"))
				{
					return;
				}
				break;
				IL_012f:
				if (!(text == "sha-384"))
				{
					return;
				}
				goto IL_0163;
				IL_0157:
				m_encryptionAlgorithmOid = "1.2.840.10045.4.3.2";
				return;
				IL_0113:
				if (!(text == "sha512"))
				{
					return;
				}
				break;
				IL_0105:
				if (!(text == "sha384"))
				{
					return;
				}
				goto IL_0163;
				IL_0163:
				m_encryptionAlgorithmOid = "1.2.840.10045.4.3.3";
				return;
			}
			m_encryptionAlgorithmOid = "1.2.840.10045.4.3.4";
			return;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal byte[] Sign(byte[] secondDigest, TimeStampServer server, byte[] timeStampResponse, byte[] ocsp, ICollection<byte[]> crls, CryptographicStandard sigtype, string hashAlgorithm)
	{
		if (m_signedData != null)
		{
			m_digest = m_signedData;
			if (m_rsaData != null)
			{
				m_rsaData = m_signedRSAData;
			}
		}
		else if (m_signedRSAData != null && m_rsaData != null)
		{
			m_rsaData = m_signedRSAData;
			m_signer.BlockUpdate(m_rsaData, 0, m_rsaData.Length);
			m_digest = m_signer.GenerateSignature();
		}
		else
		{
			if (m_rsaData != null)
			{
				m_rsaData = new byte[m_messageDigest.MessageDigestSize];
				m_messageDigest.DoFinal(m_rsaData, 0);
				m_signer.BlockUpdate(m_rsaData, 0, m_rsaData.Length);
			}
			m_digest = m_signer.GenerateSignature();
		}
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		foreach (string key in m_digestOid.Keys)
		{
			Asn1EncodeCollection asn1EncodeCollection2 = new Asn1EncodeCollection();
			asn1EncodeCollection2.Add(new DerObjectID(key));
			asn1EncodeCollection2.Add(DerNull.Value);
			asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		}
		Asn1EncodeCollection asn1EncodeCollection3 = new Asn1EncodeCollection();
		asn1EncodeCollection3.Add(new DerObjectID("1.2.840.113549.1.7.1"));
		if (m_rsaData != null)
		{
			asn1EncodeCollection3.Add(new DerTag(0, new DerOctet(m_rsaData)));
		}
		DerSequence derSequence = new DerSequence(asn1EncodeCollection3);
		asn1EncodeCollection3 = new Asn1EncodeCollection();
		foreach (X509Certificate certificate in m_certificates)
		{
			Asn1Stream asn1Stream = new Asn1Stream(new MemoryStream(certificate.GetEncoded()));
			asn1EncodeCollection3.Add(asn1Stream.ReadAsn1());
		}
		DerSet asn = new DerSet(asn1EncodeCollection3);
		Asn1EncodeCollection asn1EncodeCollection4 = new Asn1EncodeCollection();
		asn1EncodeCollection4.Add(new DerInteger(m_signerVersion));
		asn1EncodeCollection3 = new Asn1EncodeCollection();
		asn1EncodeCollection3.Add(GetIssuer(m_signCert.GetTbsCertificate()));
		asn1EncodeCollection3.Add(new DerInteger(m_signCert.SerialNumber));
		asn1EncodeCollection4.Add(new DerSequence(asn1EncodeCollection3));
		asn1EncodeCollection3 = new Asn1EncodeCollection();
		asn1EncodeCollection3.Add(new DerObjectID(m_digestAlgorithmOid));
		asn1EncodeCollection3.Add(DerNull.Value);
		asn1EncodeCollection4.Add(new DerSequence(asn1EncodeCollection3));
		if (secondDigest != null)
		{
			asn1EncodeCollection4.Add(new DerTag(isExplicit: false, 0, GetSequenceDataSet(secondDigest, ocsp, crls, sigtype)));
		}
		asn1EncodeCollection3 = new Asn1EncodeCollection();
		asn1EncodeCollection3.Add(new DerObjectID(m_encryptionAlgorithmOid));
		asn1EncodeCollection3.Add(DerNull.Value);
		asn1EncodeCollection4.Add(new DerSequence(asn1EncodeCollection3));
		asn1EncodeCollection4.Add(new DerOctet(m_digest));
		if (timeStampResponse == null && server != null)
		{
			byte[] hash = new MessageDigestAlgorithms().Digest(new MemoryStream(m_digest), "SHA256");
			byte[] asnEncodedTimestampRequest = new TimeStampRequestCreator(certReq: true).GetAsnEncodedTimestampRequest(hash);
			timeStampResponse = server.GetTimeStampResponse(asnEncodedTimestampRequest);
			Asn1 asn2 = (new Asn1Stream(timeStampResponse).ReadAsn1() as Asn1Sequence)[1] as Asn1;
			MemoryStream memoryStream = new MemoryStream();
			DerStream derStream = new DerStream(memoryStream);
			asn2.Encode(derStream);
			timeStampResponse = memoryStream.ToArray();
			derStream.m_stream.Dispose();
			memoryStream.Dispose();
		}
		if (timeStampResponse != null)
		{
			Asn1EncodeCollection attributes = GetAttributes(timeStampResponse);
			if (attributes != null)
			{
				asn1EncodeCollection4.Add(new DerTag(isExplicit: false, 1, new DerSet(attributes)));
			}
		}
		Asn1EncodeCollection asn1EncodeCollection5 = new Asn1EncodeCollection();
		asn1EncodeCollection5.Add(new DerInteger(m_version));
		asn1EncodeCollection5.Add(new DerSet(asn1EncodeCollection));
		asn1EncodeCollection5.Add(derSequence);
		asn1EncodeCollection5.Add(new DerTag(isExplicit: false, 0, asn));
		asn1EncodeCollection5.Add(new DerSet(new DerSequence(asn1EncodeCollection4)));
		Asn1EncodeCollection asn1EncodeCollection6 = new Asn1EncodeCollection();
		asn1EncodeCollection6.Add(new DerObjectID("1.2.840.113549.1.7.2"));
		asn1EncodeCollection6.Add(new DerTag(0, new DerSequence(asn1EncodeCollection5)));
		MemoryStream memoryStream2 = new MemoryStream();
		Asn1DerStream asn1DerStream = new Asn1DerStream(memoryStream2);
		asn1DerStream.WriteObject(new DerSequence(asn1EncodeCollection6));
		asn1DerStream.m_stream.Dispose();
		return memoryStream2.ToArray();
	}

	internal byte[] GetEncodedTimestamp(byte[] secondDigest, TimeStampServer server)
	{
		byte[] result = null;
		if (server != null)
		{
			byte[] asnEncodedTimestampRequest = new TimeStampRequestCreator(certReq: true).GetAsnEncodedTimestampRequest(secondDigest);
			Asn1 asn = (new Asn1Stream(server.GetTimeStampResponse(asnEncodedTimestampRequest)).ReadAsn1() as Asn1Sequence)[1] as Asn1;
			MemoryStream memoryStream = new MemoryStream();
			DerStream derStream = new DerStream(memoryStream);
			derStream.WriteObject(asn);
			asn.Encode(derStream);
			result = memoryStream.ToArray();
			derStream.m_stream.Dispose();
			memoryStream.Dispose();
		}
		return result;
	}

	private Asn1EncodeCollection GetAttributes(byte[] timeStampToken)
	{
		if (timeStampToken == null)
		{
			return null;
		}
		Asn1Stream asn1Stream = new Asn1Stream(new MemoryStream(timeStampToken));
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		Asn1EncodeCollection asn1EncodeCollection2 = new Asn1EncodeCollection();
		asn1EncodeCollection2.Add(new DerObjectID("1.2.840.113549.1.9.16.2.14"));
		Asn1Sequence asn1Sequence = (Asn1Sequence)asn1Stream.ReadAsn1();
		asn1EncodeCollection2.Add(new DerSet(asn1Sequence));
		asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		return asn1EncodeCollection;
	}

	private Asn1 GetIssuer(byte[] data)
	{
		Asn1Sequence obj = (Asn1Sequence)new Asn1Stream(new MemoryStream(data)).ReadAsn1();
		return (Asn1)obj[(obj[0] is Asn1Tag) ? 3 : 2];
	}

	internal byte[] GetSequenceData(byte[] secondDigest, byte[] ocsp, ICollection<byte[]> crlBytes, CryptographicStandard sigtype)
	{
		return GetSequenceDataSet(secondDigest, ocsp, crlBytes, sigtype).GetEncoded("DER");
	}

	private DerSet GetSequenceDataSet(byte[] secondDigest, byte[] ocsp, ICollection<byte[]> crlBytes, CryptographicStandard sigtype)
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		Asn1EncodeCollection asn1EncodeCollection2 = new Asn1EncodeCollection();
		asn1EncodeCollection2.Add(new DerObjectID("1.2.840.113549.1.9.3"));
		asn1EncodeCollection2.Add(new DerSet(new DerObjectID("1.2.840.113549.1.7.1")));
		asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		asn1EncodeCollection2 = new Asn1EncodeCollection();
		asn1EncodeCollection2.Add(new DerObjectID("1.2.840.113549.1.9.4"));
		asn1EncodeCollection2.Add(new DerSet(new DerOctet(secondDigest)));
		asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		bool flag = false;
		if (crlBytes != null)
		{
			foreach (byte[] crlByte in crlBytes)
			{
				if (crlByte != null)
				{
					flag = true;
					break;
				}
			}
		}
		if (ocsp != null || flag)
		{
			asn1EncodeCollection2 = new Asn1EncodeCollection();
			asn1EncodeCollection2.Add(new DerObjectID("1.2.840.113583.1.1.8"));
			Asn1EncodeCollection asn1EncodeCollection3 = new Asn1EncodeCollection();
			if (flag)
			{
				Asn1EncodeCollection asn1EncodeCollection4 = new Asn1EncodeCollection();
				foreach (byte[] crlByte2 in crlBytes)
				{
					if (crlByte2 != null)
					{
						Asn1Stream asn1Stream = new Asn1Stream(crlByte2);
						asn1EncodeCollection4.Add(asn1Stream.ReadAsn1());
					}
				}
				asn1EncodeCollection3.Add(new DerTag(isExplicit: true, 0, new DerSequence(asn1EncodeCollection4)));
			}
			if (ocsp != null)
			{
				DerOctet derOctet = new DerOctet(ocsp);
				Asn1EncodeCollection asn1EncodeCollection5 = new Asn1EncodeCollection();
				Asn1EncodeCollection asn1EncodeCollection6 = new Asn1EncodeCollection();
				asn1EncodeCollection6.Add(OcspConstants.OcspBasic);
				asn1EncodeCollection6.Add(derOctet);
				DerCatalogue derCatalogue = new DerCatalogue(0);
				Asn1EncodeCollection asn1EncodeCollection7 = new Asn1EncodeCollection();
				asn1EncodeCollection7.Add(derCatalogue);
				asn1EncodeCollection7.Add(new DerTag(isExplicit: true, 0, new DerSequence(asn1EncodeCollection6)));
				asn1EncodeCollection5.Add(new DerSequence(asn1EncodeCollection7));
				asn1EncodeCollection3.Add(new DerTag(isExplicit: true, 1, new DerSequence(asn1EncodeCollection5)));
			}
			asn1EncodeCollection2.Add(new DerSet(new DerSequence(asn1EncodeCollection3)));
			asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		}
		if (m_signCert != null && sigtype == CryptographicStandard.CADES)
		{
			asn1EncodeCollection2 = new Asn1EncodeCollection();
			asn1EncodeCollection2.Add(new DerObjectID("1.2.840.113549.1.9.16.2.47"));
			Asn1EncodeCollection asn1EncodeCollection8 = new Asn1EncodeCollection();
			MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
			if (!messageDigestAlgorithms.GetAllowedDigests("SHA-256").Equals(m_digestAlgorithmOid))
			{
				Algorithms algorithms = new Algorithms(new DerObjectID(m_digestAlgorithmOid));
				asn1EncodeCollection8.Add(algorithms);
			}
			byte[] bytes = messageDigestAlgorithms.Digest(HashAlgorithm, m_signCert.GetEncoded());
			asn1EncodeCollection8.Add(new DerOctet(bytes));
			asn1EncodeCollection2.Add(new DerSet(new DerSequence(new DerSequence(new DerSequence(asn1EncodeCollection8)))));
			asn1EncodeCollection.Add(new DerSequence(asn1EncodeCollection2));
		}
		return new DerSet(asn1EncodeCollection);
	}

	internal void Update(byte[] bytes, int offset, int length)
	{
		if (m_rsaData != null || m_digestAttributeData != null || m_isTimeStamp)
		{
			m_messageDigest.BlockUpdate(bytes, offset, length);
		}
		else
		{
			m_signer.BlockUpdate(bytes, offset, length);
		}
	}

	private List<X509Certificate> GetCertificateChain(X509Certificate signCert, IList<X509Certificate> certs)
	{
		List<X509Certificate> list = new List<X509Certificate>();
		list.Add(signCert);
		List<X509Certificate> list2 = new List<X509Certificate>(certs);
		for (int i = 0; i < list2.Count; i++)
		{
			if (signCert.Equals(list2[i]))
			{
				list2.RemoveAt(i);
				i--;
			}
		}
		bool flag = true;
		while (flag)
		{
			X509Certificate x509Certificate = list[list.Count - 1];
			flag = false;
			for (int j = 0; j < list2.Count; j++)
			{
				X509Certificate x509Certificate2 = list2[j];
				try
				{
					x509Certificate.Verify(x509Certificate2.GetPublicKey());
					flag = true;
					list.Add(x509Certificate2);
					list2.RemoveAt(j);
				}
				catch
				{
					continue;
				}
				break;
			}
		}
		return list;
	}

	internal bool ValidateCertificateWithCollection(List<X509Certificate> collection, DateTime signDate, PdfSignatureValidationResult signatureResult)
	{
		new List<PdfSignatureValidationException>();
		if (m_certificates != null && m_certificates.Count > 0)
		{
			for (int i = 0; i < m_certificates.Count; i++)
			{
				X509Certificate x509Certificate = m_certificates[i];
				foreach (X509Certificate item in collection)
				{
					if (ValidateCertificates(item, signDate))
					{
						try
						{
							x509Certificate.Verify(item.GetPublicKey());
							return true;
						}
						catch (Exception)
						{
						}
					}
				}
				int j;
				for (j = 0; j < m_certificates.Count; j++)
				{
					if (j != i)
					{
						X509Certificate x509Certificate2 = m_certificates[j];
						try
						{
							x509Certificate.Verify(x509Certificate2.GetPublicKey());
						}
						catch
						{
							continue;
						}
						break;
					}
				}
				if (j == m_certificates.Count)
				{
					signatureResult.SignatureValidationErrors.Add(new PdfSignatureValidationException("Cannot be verified against the KeyStore or the certificate chain"));
				}
			}
		}
		return false;
	}

	internal List<string> GetSupportedOids()
	{
		return new List<string>
		{
			X509Extensions.KeyUsage.ID,
			X509Extensions.CertificatePolicies.ID,
			X509Extensions.PolicyMappings.ID,
			X509Extensions.InhibitAnyPolicy.ID,
			X509Extensions.CrlDistributionPoints.ID,
			X509Extensions.IssuingDistributionPoint.ID,
			X509Extensions.DeltaCrlIndicator.ID,
			X509Extensions.PolicyConstraints.ID,
			X509Extensions.BasicConstraints.ID,
			X509Extensions.SubjectAlternativeName.ID,
			X509Extensions.NameConstraints.ID
		};
	}

	private bool IsSupportedOid(List<string> oids, X509Certificate certificate)
	{
		string value = "1.3.6.1.5.5.7.3.8";
		List<string> supportedOids = GetSupportedOids();
		if (oids != null && supportedOids != null)
		{
			foreach (string oid in oids)
			{
				if (!supportedOids.Contains(oid) && (oid != X509Extensions.ExtendedKeyUsage.ID || !certificate.GetExtendedKeyUsage().Contains(value)))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool ValidateCertificates(X509Certificate certificate, DateTime signDate)
	{
		List<string> oids = certificate.GetOids(critical: true);
		if (IsSupportedOid(oids, certificate))
		{
			if (!certificate.IsValid(signDate.ToUniversalTime()))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	internal X509Certificate2Collection GetCertificates()
	{
		X509Certificate2Collection x509Certificate2Collection = new X509Certificate2Collection();
		if (m_certificates != null && m_certificates.Count > 0)
		{
			foreach (X509Certificate certificate in m_certificates)
			{
				x509Certificate2Collection.Add(new X509Certificate2(certificate.GetEncoded()));
			}
		}
		return x509Certificate2Collection;
	}

	internal bool ValidateChecksum()
	{
		bool flag = false;
		if (!m_isTimeStamp)
		{
			if (m_sigAttribute != null || m_sigAttributeDer != null)
			{
				byte[] array = new byte[m_messageDigest.MessageDigestSize];
				m_messageDigest.DoFinal(array, 0);
				bool flag2 = true;
				bool flag3 = false;
				if (m_rsaData != null)
				{
					flag2 = object.Equals(array, m_rsaData);
					m_encodedMessageDigest.BlockUpdate(m_rsaData, 0, m_rsaData.Length);
					byte[] array2 = new byte[m_encodedMessageDigest.MessageDigestSize];
					m_encodedMessageDigest.DoFinal(array2, 0);
					flag3 = object.Equals(array2, m_digestAttributeData);
				}
				int i = 0;
				bool flag4 = true;
				if (array.Length == m_digestAttributeData.Length)
				{
					for (; i < array.Length; i++)
					{
						if (array[i] != m_digestAttributeData[i])
						{
							flag4 = false;
							break;
						}
					}
				}
				else
				{
					flag4 = false;
				}
				return (flag4 || flag3) && (ValidateAttributes(m_sigAttribute) || ValidateAttributes(m_sigAttributeDer)) && flag2;
			}
			if (m_rsaData != null)
			{
				byte[] array3 = new byte[m_messageDigest.MessageDigestSize];
				m_messageDigest.DoFinal(array3, 0);
				m_signer.BlockUpdate(array3, 0, array3.Length);
			}
			return m_signer.ValidateSignature(m_digest);
		}
		return true;
	}

	private X509Certificate CheckCertificateValidation(DateTime signDate, PdfSignatureValidationResult result)
	{
		X509Certificate x509Certificate = null;
		List<X509Certificate> list = new List<X509Certificate>();
		X509CertificateParser x509CertificateParser = new X509CertificateParser();
		if (rootCertificates != null)
		{
			foreach (X509Certificate2 rootCertificate in rootCertificates)
			{
				list.Add(x509CertificateParser.ReadCertificate(rootCertificate.RawData));
			}
			if (m_certificates != null && m_certificates.Count > 0)
			{
				endCertificates = m_certificates;
				for (int i = 0; i < endCertificates.Count; i++)
				{
					X509Certificate x509Certificate3 = m_certificates[i];
					foreach (X509Certificate item in list)
					{
						if (!ValidateCertificates(item, signDate))
						{
							continue;
						}
						try
						{
							x509Certificate3.Verify(item.GetPublicKey());
							if (!endCertificates.Contains(item))
							{
								if (x509Certificate == null)
								{
									x509Certificate = item;
								}
								endCertificates.Add(item);
							}
						}
						catch (Exception)
						{
						}
					}
				}
			}
		}
		return x509Certificate;
	}

	private bool ValidateAttributes(byte[] attr)
	{
		ISigner signer = InitializeSignature();
		signer.BlockUpdate(attr, 0, attr.Length);
		return signer.ValidateSignature(m_digest);
	}

	internal TimeStampInformation ValidateTimestamp()
	{
		if (m_timeStampToken == null)
		{
			return null;
		}
		TimeStampInformation timeStampInformation = new TimeStampInformation();
		TimeStampTokenInformation timeStampInformation2 = m_timeStampToken.TimeStampInformation;
		timeStampInformation.MessageImprintAlgorithmId = timeStampInformation2.MessageImprintAlgOid;
		timeStampInformation.TimeStampPolicyId = timeStampInformation2.Policy;
		timeStampInformation.Time = timeStampInformation2.GeneralizedTime;
		timeStampInformation.SignerInformation = timeStampInformation2.TimeStampData;
		MessageStamp messageImprint = timeStampInformation2.TimeStampData.MessageImprint;
		string messageImprintAlgOid = timeStampInformation2.MessageImprintAlgOid;
		byte[] array = m_digestAlgorithm.Digest(messageImprintAlgOid, m_digest);
		byte[] hashedMessage = messageImprint.HashedMessage;
		timeStampInformation.IsDocumentTimeStamp = m_timeStampDocument;
		timeStampInformation.m_signer = this;
		timeStampInformation.Certificate = TimeStampCertificates();
		if (array == hashedMessage)
		{
			timeStampInformation.IsValid = true;
			return timeStampInformation;
		}
		if (array == null || hashedMessage == null)
		{
			timeStampInformation.IsValid = false;
			return timeStampInformation;
		}
		int num = array.Length;
		if (num != hashedMessage.Length)
		{
			timeStampInformation.IsValid = false;
			return timeStampInformation;
		}
		while (num != 0)
		{
			num--;
			if (array[num] != hashedMessage[num])
			{
				timeStampInformation.IsValid = false;
				return timeStampInformation;
			}
		}
		return timeStampInformation;
	}

	private ISigner InitializeSignature()
	{
		ISigner signer = new SignerUtilities().GetSigner(MessageDigestAlgorithm);
		signer.Initialize(isSigning: false, m_signCert.GetPublicKey());
		return signer;
	}

	private void GetOcsp(Asn1Sequence sequenceObj, bool dss)
	{
		X509RevocationResponse x509RevocationResponse = null;
		List<Asn1Sequence> list = new List<Asn1Sequence>();
		for (int i = 0; i < sequenceObj.Count; i++)
		{
			Asn1Sequence asn1Sequence = sequenceObj[i] as Asn1Sequence;
			bool flag = false;
			while (!(asn1Sequence[0] is DerObjectID) || !(((DerObjectID)asn1Sequence[0]).ID == m_ocspID))
			{
				for (int j = 0; j < asn1Sequence.Count; j++)
				{
					if (asn1Sequence[j] is Asn1Sequence)
					{
						asn1Sequence = (Asn1Sequence)asn1Sequence[0];
						flag = false;
						break;
					}
					if (asn1Sequence[j] is Asn1Tag)
					{
						Asn1Tag asn1Tag = (Asn1Tag)asn1Sequence[j];
						if (asn1Tag.GetObject() is Asn1Sequence)
						{
							asn1Sequence = (Asn1Sequence)asn1Tag.GetObject();
							flag = false;
							break;
						}
						return;
					}
				}
				if (flag)
				{
					return;
				}
			}
			list.Add(asn1Sequence);
		}
		for (int k = 0; k < list.Count; k++)
		{
			Asn1Stream asn1Stream = new Asn1Stream(((Asn1Octet)list[k][1]).GetOctets());
			x509RevocationResponse = new X509RevocationResponse(new OcspHelper().GetOcspStructure(asn1Stream.ReadAsn1()));
			if (x509RevocationResponse == null)
			{
				continue;
			}
			if (!dss)
			{
				if (m_embbedOCSPs == null)
				{
					m_embbedOCSPs = new List<X509RevocationResponse>();
				}
				m_embbedOCSPs.Add(x509RevocationResponse);
			}
			else
			{
				if (m_dssOCSPs == null)
				{
					m_dssOCSPs = new List<X509RevocationResponse>();
				}
				m_dssOCSPs.Add(x509RevocationResponse);
			}
		}
	}

	private X509Certificate2 TimeStampCertificates()
	{
		if (m_timestampCertificates != null)
		{
			List<X509Certificate> list = new X509CertificateParser().ReadCertificates(m_timestampCertificates) as List<X509Certificate>;
			m_certificate2 = new X509Certificate2(list[0].CertificateStructure.GetDerEncoded());
		}
		return m_certificate2;
	}

	internal RevocationResult CheckRevocation(DateTime signedDate, PdfSignatureValidationResult validationResult)
	{
		if (m_certificates != null)
		{
			RevocationResult revocationResult = new RevocationResult();
			m_ltvVerificationInfo = new LtvVerificationInfo();
			X509Certificate x509Certificate = CheckCertificateValidation(signedDate, validationResult);
			X509Certificate x509Certificate2 = m_certificates[0];
			X509Certificate x509Certificate3 = ((m_certificates.Count > 1) ? m_certificates[1] : null);
			if (x509Certificate3 == null && x509Certificate != null)
			{
				x509Certificate3 = x509Certificate;
			}
			if (x509Certificate3 == null && m_certificates.Count == 1 && x509Certificate2 != null)
			{
				string distinguishedAttributes = GetDistinguishedAttributes(x509Certificate2.IssuerDN.ToString(), "CN");
				if (distinguishedAttributes == string.Empty)
				{
					distinguishedAttributes = GetDistinguishedAttributes(x509Certificate2.IssuerDN.ToString(), "OU");
				}
				string distinguishedAttributes2 = GetDistinguishedAttributes(x509Certificate2.SubjectDN.ToString(), "CN");
				if (distinguishedAttributes == distinguishedAttributes2)
				{
					return revocationResult;
				}
				UpdateCertCollection();
				x509Certificate2 = m_certificates[0];
				x509Certificate3 = ((m_certificates.Count > 1) ? m_certificates[1] : null);
			}
			List<X509RevocationResponse> list = new List<X509RevocationResponse>();
			List<CertificateCollection> list2 = new List<CertificateCollection>();
			Dictionary<string, X509RevocationResponse> dictionary = new Dictionary<string, X509RevocationResponse>();
			Dictionary<string, CertificateCollection> dictionary2 = new Dictionary<string, CertificateCollection>();
			UpdateIntermediateURIs();
			if (ocspResponseCollection.Count > 0)
			{
				list = ocspResponseCollection;
				for (int i = 0; i < list.Count; i++)
				{
					OneTimeResponse[] responses = list[i].Responses;
					foreach (OneTimeResponse oneTimeResponse in responses)
					{
						if (m_certSerialNumbers != null && m_certSerialNumbers.Count > 0 && m_certSerialNumbers.Contains(oneTimeResponse.CertificateID.SerialNumber.ToString()))
						{
							int num = m_certSerialNumbers.IndexOf(oneTimeResponse.CertificateID.SerialNumber.ToString());
							m_foundOCSPs.Add(m_certSerialNumbers[num + 1]);
							break;
						}
					}
				}
			}
			else if (m_embbedOCSPs != null && m_embbedOCSPs.Count > 0)
			{
				UpdateEmbbedOCSP(dictionary);
			}
			if (m_ocspByteCollection != null && m_ocspByteCollection.Count > 0)
			{
				foreach (byte[] item in m_ocspByteCollection)
				{
					UpdateFromDSS(item, dictionary);
				}
				UpdateDSSOCSP(dictionary);
			}
			if (dictionary.Count > 0)
			{
				foreach (KeyValuePair<string, X509RevocationResponse> item2 in dictionary)
				{
					list.Add(item2.Value);
				}
			}
			dictionary.Clear();
			if (list.Count > 0)
			{
				m_ltvVerificationInfo.IsOcspEmbedded = true;
			}
			if (validationResult.signatureOptions.CRLResponseData != null && validationResult.signatureOptions.CRLResponseData.Count > 0)
			{
				foreach (byte[] cRLResponseDatum in validationResult.signatureOptions.CRLResponseData)
				{
					CertificateCollection certificateList = null;
					UpdateCertificateCollection(cRLResponseDatum, out certificateList);
					if (certificateList != null)
					{
						UpdateEmbbedCRL(dictionary2, certificateList);
					}
				}
			}
			else if (m_crlSequence != null && m_crlSequence.Count > 0)
			{
				for (int k = 0; k < m_crlSequence.Count; k++)
				{
					Asn1Sequence asn1Sequence = (Asn1Sequence)m_crlSequence[k];
					if (asn1Sequence != null)
					{
						CertificateCollection certificateList2 = CertificateCollection.GetCertificateList(asn1Sequence);
						if (certificateList2 != null)
						{
							UpdateEmbbedCRL(dictionary2, certificateList2);
						}
					}
				}
			}
			if (m_crlByteCollection != null)
			{
				Dictionary<string, PdfSignerCertificate> dictionary3 = new Dictionary<string, PdfSignerCertificate>();
				Dictionary<string, X509Certificate> dictionary4 = new Dictionary<string, X509Certificate>();
				for (int num2 = CertificateList.Count - 1; num2 >= 0; num2--)
				{
					X509Certificate x509Certificate4 = CertificateList[num2];
					PdfSignerCertificate pdfSignerCertificate = new PdfSignerCertificate();
					X509Certificate2 certificate = new X509Certificate2(x509Certificate4.GetEncoded());
					pdfSignerCertificate.Certificate = certificate;
					dictionary3[x509Certificate4.SerialNumber.ToString()] = pdfSignerCertificate;
					string distinguishedAttributes3 = GetDistinguishedAttributes(x509Certificate4.IssuerDN.ToString(), "CN");
					if (distinguishedAttributes3 == string.Empty)
					{
						distinguishedAttributes3 = GetDistinguishedAttributes(x509Certificate4.IssuerDN.ToString(), "OU");
					}
					string distinguishedAttributes4 = GetDistinguishedAttributes(x509Certificate4.SubjectDN.ToString(), "CN");
					if (distinguishedAttributes3 != distinguishedAttributes4)
					{
						dictionary4[distinguishedAttributes3] = x509Certificate4;
					}
				}
				foreach (byte[] item3 in m_crlByteCollection)
				{
					CertificateCollection certificateList3 = null;
					UpdateCertificateCollection(item3, out certificateList3);
					if (certificateList3 == null)
					{
						continue;
					}
					string distinguishedAttributes5 = GetDistinguishedAttributes(certificateList3.Issuer.ToString(), "CN");
					if (distinguishedAttributes5 == string.Empty)
					{
						distinguishedAttributes5 = GetDistinguishedAttributes(certificateList3.Issuer.ToString(), "OU");
					}
					X509Extension x509Extension = null;
					List<string> list3 = null;
					List<string> crlUrls = null;
					if (dictionary4.ContainsKey(distinguishedAttributes5))
					{
						X509Certificate certificate2 = dictionary4[distinguishedAttributes5];
						list3 = new CertificateUtililty().GetCrlUrls(certificate2);
						X509Extensions extensions = certificateList3.CertificateList.GetExtensions();
						if (extensions != null)
						{
							x509Extension = extensions.GetExtension(new DerObjectID("2.5.29.28"));
						}
					}
					if (x509Extension != null)
					{
						GetExtensionUrls(x509Extension, out crlUrls);
						bool flag = false;
						if (crlUrls.Count == list3.Count)
						{
							for (int l = 0; l < list3.Count; l++)
							{
								flag = crlUrls[l] == list3[l];
							}
						}
						if (flag)
						{
							UpdateEmbbedCRL(dictionary2, certificateList3);
						}
					}
					else
					{
						UpdateEmbbedCRL(dictionary2, certificateList3);
					}
				}
			}
			if (dictionary2.Count > 0)
			{
				foreach (KeyValuePair<string, CertificateCollection> item4 in dictionary2)
				{
					list2.Add(item4.Value);
				}
			}
			dictionary2.Clear();
			if (list2.Count > 0)
			{
				m_ltvVerificationInfo.IsCrlEmbedded = true;
			}
			OcspValidator ocspValidator = new OcspValidator(list);
			ocspValidator.IsOcspEmbedded = m_ltvVerificationInfo.IsOcspEmbedded;
			ocspValidator.result = validationResult;
			RevocationValidationType revocationValidationType = validationResult.signatureOptions.RevocationValidationType;
			if (revocationValidationType == RevocationValidationType.Both || revocationValidationType == RevocationValidationType.Ocsp)
			{
				revocationResult.OcspRevocationStatus = ocspValidator.Validate(x509Certificate2, x509Certificate3, signedDate, m_ltvVerificationInfo.IsCrlEmbedded);
			}
			if (revocationResult.OcspRevocationStatus == RevocationStatus.Revoked)
			{
				PdfSignatureValidationException ex = new PdfSignatureValidationException(m_ltvVerificationInfo.IsOcspEmbedded ? "The certificate is considered invalid because it has been revoked as verified using Online Certificate Status Protocol (OCSP) that was embedded in the document." : "The certificate is considered invalid because it has been revoked as verified using Online Certificate Status Protocol (OCSP) obtained online.");
				ex.ExceptionType = PdfSignatureValidationExceptionType.OCSP;
				validationResult.SignatureValidationErrors.Add(ex);
			}
			RevocationListValidator revocationListValidator = new RevocationListValidator(x509Certificate2, revocationResult);
			revocationListValidator.result = validationResult;
			if (revocationValidationType == RevocationValidationType.Both || revocationValidationType == RevocationValidationType.Crl)
			{
				if (list2.Count > 0)
				{
					revocationResult.IsRevokedCRL = revocationListValidator.Validate(x509Certificate2, x509Certificate3, signedDate, list2);
				}
				else
				{
					revocationResult.IsRevokedCRL = revocationListValidator.OnlineValidate(x509Certificate2, x509Certificate3, signedDate);
				}
			}
			if (revocationResult.IsRevokedCRL)
			{
				PdfSignatureValidationException ex2 = new PdfSignatureValidationException(m_ltvVerificationInfo.IsCrlEmbedded ? "The certificate is considered invalid because it has been revoked as verified using CRL that was embedded in the document." : "The certificate is considered invalid because it has been revoked as verified using CRL obtained online.");
				ex2.ExceptionType = PdfSignatureValidationExceptionType.CRL;
				validationResult.SignatureValidationErrors.Add(ex2);
			}
			if (revocationResult.OcspRevocationStatus == RevocationStatus.Good || ocspValidator.m_ocspResposne || (!ocspValidator.m_ocspResposne && !revocationResult.IsRevokedCRL))
			{
				validationResult.m_isValidOCSPorCRLtimeValidation = true;
			}
			foreach (string intermediateIssueName in intermediateIssueNames)
			{
				if (!m_foundCRLs.Contains(intermediateIssueName) && !m_foundOCSPs.Contains(intermediateIssueName))
				{
					isLtvEnabled = false;
					break;
				}
			}
			if (isLtvEnabled && (revocationResult.OcspRevocationStatus == RevocationStatus.Good || !revocationResult.IsRevokedCRL))
			{
				m_ltvVerificationInfo.IsLtvEnabled = true;
			}
			Close();
			return revocationResult;
		}
		return null;
	}

	private void UpdateIntermediateURIs()
	{
		if (m_embbedOCSPs == null)
		{
			m_embbedOCSPs = new List<X509RevocationResponse>();
		}
		m_foundOCSPs = new List<string>();
		m_foundCRLs = new List<string>();
		m_certSerialNumbers = new List<string>();
		intermediateIssueNames = new List<string>();
		CertificateUtililty certificateUtililty = new CertificateUtililty();
		X509Certificate[] array = m_certificates.ToArray();
		m_revocationCertURIs = new Dictionary<string, List<Dictionary<string, List<string>>>>();
		X509Certificate[] array2 = array;
		foreach (X509Certificate x509Certificate in array2)
		{
			string distinguishedAttributes = GetDistinguishedAttributes(x509Certificate.IssuerDN.ToString(), "CN");
			if (distinguishedAttributes == string.Empty)
			{
				distinguishedAttributes = GetDistinguishedAttributes(x509Certificate.IssuerDN.ToString(), "OU");
			}
			string distinguishedAttributes2 = GetDistinguishedAttributes(x509Certificate.SubjectDN.ToString(), "CN");
			if (!(distinguishedAttributes == distinguishedAttributes2))
			{
				m_certSerialNumbers.Add(x509Certificate.SerialNumber.ToString());
				m_certSerialNumbers.Add(distinguishedAttributes);
				intermediateIssueNames.Add(distinguishedAttributes);
				List<string> crlUrls = certificateUtililty.GetCrlUrls(x509Certificate);
				Dictionary<string, List<string>> dictionary = null;
				Dictionary<string, List<string>> dictionary2 = null;
				List<Dictionary<string, List<string>>> list = new List<Dictionary<string, List<string>>>();
				if (crlUrls != null && crlUrls.Count > 0)
				{
					dictionary = new Dictionary<string, List<string>>();
					dictionary["CRL"] = crlUrls;
				}
				string ocspUrl = certificateUtililty.GetOcspUrl(x509Certificate);
				if (ocspUrl != null)
				{
					crlUrls = new List<string>();
					dictionary2 = new Dictionary<string, List<string>>();
					crlUrls.Add(ocspUrl);
					dictionary2["OCSP"] = crlUrls;
				}
				if (dictionary != null)
				{
					list.Add(dictionary);
				}
				if (dictionary2 != null)
				{
					list.Add(dictionary2);
				}
				if (list.Count > 0)
				{
					m_revocationCertURIs[distinguishedAttributes] = list;
				}
				else
				{
					isLtvEnabled = false;
				}
			}
		}
		if (m_revocationCertURIs.Count == 0)
		{
			isLtvEnabled = false;
		}
	}

	private void UpdateEmbbedOCSP(Dictionary<string, X509RevocationResponse> ocsps)
	{
		if (m_embbedOCSPs != null && m_embbedOCSPs.Count > 0)
		{
			for (int i = 0; i < m_embbedOCSPs.Count; i++)
			{
				X509RevocationResponse basicOcspResponse = m_embbedOCSPs[i];
				UpdateOCSPCollection(ocsps, basicOcspResponse);
			}
		}
	}

	private void UpdateDSSOCSP(Dictionary<string, X509RevocationResponse> ocsps)
	{
		if (m_dssOCSPs != null && m_dssOCSPs.Count > 0)
		{
			for (int i = 0; i < m_dssOCSPs.Count; i++)
			{
				X509RevocationResponse basicOcspResponse = m_dssOCSPs[i];
				UpdateOCSPCollection(ocsps, basicOcspResponse);
			}
		}
	}

	private void UpdateOCSPCollection(Dictionary<string, X509RevocationResponse> ocsps, X509RevocationResponse m_basicOcspResponse)
	{
		OneTimeResponse[] responses = m_basicOcspResponse.Responses;
		foreach (OneTimeResponse oneTimeResponse in responses)
		{
			if (m_certSerialNumbers != null && m_certSerialNumbers.Count > 0 && m_certSerialNumbers.Contains(oneTimeResponse.CertificateID.SerialNumber.ToString()))
			{
				int num = m_certSerialNumbers.IndexOf(oneTimeResponse.CertificateID.SerialNumber.ToString());
				string text = m_certSerialNumbers[num + 1];
				if (!m_foundOCSPs.Contains(text))
				{
					m_foundOCSPs.Add(text);
				}
				ocsps[text] = m_basicOcspResponse;
				break;
			}
		}
	}

	private void UpdateFromDSS(byte[] bytes, Dictionary<string, X509RevocationResponse> ocsps)
	{
		Asn1Stream asn1Stream = new Asn1Stream(bytes);
		OcspHelper ocspHelper = new OcspHelper();
		X509RevocationResponse x509RevocationResponse = null;
		try
		{
			x509RevocationResponse = new X509RevocationResponse(ocspHelper.GetOcspStructure(asn1Stream.ReadAsn1()));
			if (x509RevocationResponse != null)
			{
				UpdateOCSPCollection(ocsps, x509RevocationResponse);
			}
		}
		catch (Exception)
		{
			asn1Stream = new Asn1Stream(bytes);
			DerSequence sequenceObj = new DerSequence(asn1Stream.GetEncodableCollection());
			GetOcsp(sequenceObj, dss: true);
		}
	}

	private void UpdateEmbbedCRL(Dictionary<string, CertificateCollection> crls, CertificateCollection certificateList)
	{
		if (m_certSerialNumbers == null || m_certSerialNumbers.Count <= 0)
		{
			return;
		}
		string distinguishedAttributes = GetDistinguishedAttributes(certificateList.Issuer.ToString(), "CN");
		if (distinguishedAttributes == string.Empty)
		{
			distinguishedAttributes = GetDistinguishedAttributes(certificateList.Issuer.ToString(), "OU");
		}
		if (m_certSerialNumbers.Contains(distinguishedAttributes))
		{
			crls[distinguishedAttributes] = certificateList;
			if (!m_foundCRLs.Contains(distinguishedAttributes))
			{
				m_foundCRLs.Add(distinguishedAttributes);
			}
		}
	}

	private void UpdateCertificateCollection(byte[] bytes, out CertificateCollection certificateList)
	{
		certificateList = null;
		try
		{
			byte[] input = bytes;
			if (bytes.Length != 0 && bytes[0] != 48)
			{
				MemoryStream memoryStream = new MemoryStream(bytes);
				input = ReadPemCRL(memoryStream);
				memoryStream.Dispose();
				memoryStream = null;
			}
			Asn1Sequence obj = (Asn1Sequence)new Asn1Stream(input).ReadAsn1();
			certificateList = CertificateCollection.GetCertificateList(obj);
		}
		catch
		{
		}
	}

	private string GetDistinguishedAttributes(string name, string key)
	{
		string result = string.Empty;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (key.Contains("="))
		{
			key = key.Replace("=", "");
		}
		StringBuilder stringBuilder = new StringBuilder();
		DistinguishedNameSeparator distinguishedNameSeparator = DistinguishedNameSeparator.InitialSeparator;
		for (int i = 0; i < name.Length; i++)
		{
			switch (distinguishedNameSeparator)
			{
			case DistinguishedNameSeparator.InitialSeparator:
				if (name[i] == ',' || name[i] == ';' || name[i] == '+')
				{
					AddStringToDictionary(stringBuilder.ToString(), dictionary);
					stringBuilder.Length = 0;
					break;
				}
				stringBuilder.Append(name[i]);
				if (name[i] == '\\')
				{
					stringBuilder.Append(name[++i]);
				}
				else if (name[i] == '"')
				{
					distinguishedNameSeparator = DistinguishedNameSeparator.QuoteSpearator;
				}
				break;
			case DistinguishedNameSeparator.QuoteSpearator:
				stringBuilder.Append(name[i]);
				if (name[i] == '\\')
				{
					stringBuilder.Append(name[++i]);
				}
				else if (name[i] == '"')
				{
					distinguishedNameSeparator = DistinguishedNameSeparator.InitialSeparator;
				}
				break;
			}
		}
		AddStringToDictionary(stringBuilder.ToString(), dictionary);
		stringBuilder.Length = 0;
		if (dictionary.ContainsKey(key))
		{
			result = dictionary[key];
		}
		return result;
	}

	private void AddStringToDictionary(string name, Dictionary<string, string> dictionary)
	{
		int num = name.IndexOf("=");
		if (num > 0)
		{
			string[] array = new string[2]
			{
				name.Substring(0, num).TrimStart().TrimEnd(),
				null
			};
			num++;
			array[1] = name.Substring(num, name.Length - num).TrimStart().TrimEnd();
			if (!string.IsNullOrEmpty(array[0]) && !string.IsNullOrEmpty(array[1]) && !dictionary.ContainsKey(array[0]))
			{
				dictionary.Add(array[0], array[1]);
			}
		}
	}

	private byte[] ReadPemCRL(Stream stream)
	{
		string value = "-----BEGIN CRL-----";
		string value2 = "-----BEGIN X509 CRL-----";
		string value3 = "-----END CRL-----";
		string value4 = "-----END X509 CRL-----";
		StringBuilder stringBuilder = new StringBuilder();
		StreamReader streamReader = new StreamReader(stream);
		string text;
		while ((text = streamReader.ReadLine()) != null && !text.StartsWith(value) && !text.StartsWith(value2))
		{
		}
		while ((text = streamReader.ReadLine()) != null && !text.StartsWith(value3) && !text.StartsWith(value4))
		{
			stringBuilder.Append(text);
		}
		streamReader.Dispose();
		streamReader = null;
		return Convert.FromBase64String(stringBuilder.ToString());
	}

	private void Close()
	{
		if (m_certSerialNumbers != null)
		{
			m_certSerialNumbers.Clear();
		}
		if (m_crlByteCollection != null)
		{
			m_crlByteCollection.Clear();
		}
		if (m_ocspByteCollection != null)
		{
			m_ocspByteCollection.Clear();
		}
		if (m_foundOCSPs != null)
		{
			m_foundOCSPs.Clear();
		}
		if (m_foundCRLs != null)
		{
			m_foundCRLs.Clear();
		}
		if (m_revocationCertURIs != null)
		{
			m_revocationCertURIs.Clear();
		}
		if (intermediateIssueNames != null)
		{
			intermediateIssueNames.Clear();
		}
	}

	private void UpdateDSSCollection()
	{
		List<byte[]> crlByteCollection = null;
		List<byte[]> ocspByteCollection = null;
		if (Signature.m_document != null && Signature.m_document.Catalog != null && Signature.m_document.Catalog.ContainsKey("DSS") && PdfCrossTable.Dereference(Signature.m_document.Catalog["DSS"]) is PdfDictionary pdfDictionary)
		{
			if (pdfDictionary.ContainsKey("CRLs"))
			{
				PdfArray array = PdfCrossTable.Dereference(pdfDictionary["CRLs"]) as PdfArray;
				crlByteCollection = GetByteCollection(array);
			}
			if (pdfDictionary.ContainsKey("OCSPs"))
			{
				PdfArray array2 = PdfCrossTable.Dereference(pdfDictionary["OCSPs"]) as PdfArray;
				ocspByteCollection = GetByteCollection(array2);
			}
		}
		m_crlByteCollection = crlByteCollection;
		m_ocspByteCollection = ocspByteCollection;
	}

	private List<byte[]> GetByteCollection(PdfArray array)
	{
		if (array != null)
		{
			List<byte[]> list = new List<byte[]>();
			for (int i = 0; i < array.Count; i++)
			{
				if (PdfCrossTable.Dereference(array[i]) is PdfStream pdfStream)
				{
					list.Add(pdfStream.GetDecompressedData());
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}
		return null;
	}

	internal void UpdateSignerDetails(List<PdfSignerCertificate> signerCertificates, PdfSignatureValidationResult validationResult)
	{
		Dictionary<string, PdfSignerCertificate> dictionary = new Dictionary<string, PdfSignerCertificate>();
		Dictionary<string, X509Certificate2> dictionary2 = new Dictionary<string, X509Certificate2>();
		Dictionary<string, X509RevocationResponse> dictionary3 = new Dictionary<string, X509RevocationResponse>();
		Dictionary<string, CertificateCollection> dictionary4 = new Dictionary<string, CertificateCollection>();
		Dictionary<string, X509Certificate> dictionary5 = new Dictionary<string, X509Certificate>();
		UpdateDSSCollection();
		bool flag = IsDssContainsCertCollection();
		bool isRevokedCRL = false;
		if (CertificateList.Count == 1)
		{
			string distinguishedAttributes = GetDistinguishedAttributes(CertificateList[0].IssuerDN.ToString(), "CN");
			if (distinguishedAttributes == string.Empty)
			{
				distinguishedAttributes = GetDistinguishedAttributes(CertificateList[0].IssuerDN.ToString(), "OU");
			}
			string distinguishedAttributes2 = GetDistinguishedAttributes(CertificateList[0].SubjectDN.ToString(), "CN");
			if (distinguishedAttributes != distinguishedAttributes2 && flag)
			{
				UpdateCertCollection();
			}
		}
		else if (flag)
		{
			bool rootFound = false;
			CheckRootCertficate(out rootFound);
			if (!rootFound)
			{
				UpdateCertCollection();
			}
		}
		X509Certificate2 value = null;
		for (int num = CertificateList.Count - 1; num >= 0; num--)
		{
			X509Certificate x509Certificate = CertificateList[num];
			PdfSignerCertificate pdfSignerCertificate = new PdfSignerCertificate();
			X509Certificate2 x509Certificate3 = (pdfSignerCertificate.Certificate = new X509Certificate2(x509Certificate.GetEncoded()));
			signerCertificates.Add(pdfSignerCertificate);
			dictionary[x509Certificate.SerialNumber.ToString()] = pdfSignerCertificate;
			string distinguishedAttributes3 = GetDistinguishedAttributes(x509Certificate.IssuerDN.ToString(), "CN");
			if (distinguishedAttributes3 == string.Empty)
			{
				distinguishedAttributes3 = GetDistinguishedAttributes(x509Certificate.IssuerDN.ToString(), "OU");
			}
			string distinguishedAttributes4 = GetDistinguishedAttributes(x509Certificate.SubjectDN.ToString(), "CN");
			if (distinguishedAttributes3 != distinguishedAttributes4)
			{
				dictionary5[distinguishedAttributes3] = x509Certificate;
				dictionary2[distinguishedAttributes3] = value;
			}
			value = x509Certificate3;
		}
		List<X509RevocationResponse> list = new List<X509RevocationResponse>();
		List<CertificateCollection> list2 = new List<CertificateCollection>();
		UpdateIntermediateURIs();
		if (ocspResponseCollection.Count > 0)
		{
			list = ocspResponseCollection;
			for (int i = 0; i < list.Count; i++)
			{
				OneTimeResponse[] responses = list[i].Responses;
				foreach (OneTimeResponse oneTimeResponse in responses)
				{
					if (m_certSerialNumbers != null && m_certSerialNumbers.Count > 0 && m_certSerialNumbers.Contains(oneTimeResponse.CertificateID.SerialNumber.ToString()))
					{
						int num2 = m_certSerialNumbers.IndexOf(oneTimeResponse.CertificateID.SerialNumber.ToString());
						m_foundOCSPs.Add(m_certSerialNumbers[num2 + 1]);
						break;
					}
				}
			}
		}
		else if (m_embbedOCSPs != null && m_embbedOCSPs.Count > 0)
		{
			UpdateEmbbedOCSP(dictionary3);
		}
		if (m_ocspByteCollection != null && m_ocspByteCollection.Count > 0)
		{
			foreach (byte[] item in m_ocspByteCollection)
			{
				UpdateFromDSS(item, dictionary3);
			}
			UpdateDSSOCSP(dictionary3);
		}
		if (dictionary3.Count > 0)
		{
			foreach (KeyValuePair<string, X509RevocationResponse> item2 in dictionary3)
			{
				list.Add(item2.Value);
			}
		}
		dictionary3.Clear();
		if (list.Count > 0)
		{
			for (int k = 0; k < list.Count; k++)
			{
				OneTimeResponse[] responses = list[k].Responses;
				foreach (OneTimeResponse oneTimeResponse2 in responses)
				{
					if (m_certSerialNumbers == null || m_certSerialNumbers.Count <= 0)
					{
						continue;
					}
					PdfSignerCertificate value2 = null;
					dictionary.TryGetValue(oneTimeResponse2.CertificateID.SerialNumber.ToString(), out value2);
					if (value2 != null)
					{
						PdfRevocationCertificate pdfRevocationCertificate2 = (value2.OcspCertificate = new PdfRevocationCertificate());
						pdfRevocationCertificate2.IsEmbedded = true;
						if (oneTimeResponse2.CurrentUpdate != null)
						{
							pdfRevocationCertificate2.ValidFrom = oneTimeResponse2.CurrentUpdate.ToDateTime();
						}
						if (oneTimeResponse2.NextUpdate != null)
						{
							pdfRevocationCertificate2.ValidTo = oneTimeResponse2.NextUpdate.ToDateTime();
						}
						X509Certificate[] certificates = list[k].Certificates;
						new List<X509Certificate2>();
						pdfRevocationCertificate2.Certificates = new X509Certificate2[certificates.Length];
						for (int l = 0; l < certificates.Length; l++)
						{
							pdfRevocationCertificate2.Certificates[l] = new X509Certificate2(certificates[l].GetEncoded());
						}
					}
				}
			}
		}
		if (validationResult != null && validationResult.signatureOptions != null && validationResult.signatureOptions.CRLResponseData != null && validationResult.signatureOptions.CRLResponseData.Count > 0)
		{
			foreach (byte[] cRLResponseDatum in validationResult.signatureOptions.CRLResponseData)
			{
				CertificateCollection certificateList = null;
				UpdateCertificateCollection(cRLResponseDatum, out certificateList);
				if (certificateList != null)
				{
					UpdateEmbbedCRL(dictionary4, certificateList);
				}
			}
		}
		else if (m_crlSequence != null && m_crlSequence.Count > 0)
		{
			for (int m = 0; m < m_crlSequence.Count; m++)
			{
				Asn1Sequence asn1Sequence = (Asn1Sequence)m_crlSequence[m];
				if (asn1Sequence != null)
				{
					CertificateCollection certificateList2 = CertificateCollection.GetCertificateList(asn1Sequence);
					if (certificateList2 != null)
					{
						UpdateEmbbedCRL(dictionary4, certificateList2);
					}
				}
			}
		}
		if (m_crlByteCollection != null)
		{
			for (int n = 0; n < m_crlByteCollection.Count; n++)
			{
				byte[] bytes = m_crlByteCollection[n];
				CertificateCollection certificateList3 = null;
				UpdateCertificateCollection(bytes, out certificateList3);
				if (certificateList3 == null)
				{
					continue;
				}
				if (flag)
				{
					string distinguishedAttributes5 = GetDistinguishedAttributes(certificateList3.Issuer.ToString(), "CN");
					X509Certificate certificate = dictionary5[distinguishedAttributes5];
					List<string> crlUrls = new CertificateUtililty().GetCrlUrls(certificate);
					X509Extensions extensions = certificateList3.CertificateList.GetExtensions();
					if (extensions == null)
					{
						continue;
					}
					X509Extension extension = extensions.GetExtension(new DerObjectID("2.5.29.28"));
					List<string> crlUrls2 = null;
					if (extension != null)
					{
						GetExtensionUrls(extension, out crlUrls2);
						bool flag2 = false;
						if (crlUrls2.Count == crlUrls.Count)
						{
							for (int num3 = 0; num3 < crlUrls.Count; num3++)
							{
								flag2 = crlUrls2[num3] == crlUrls[num3];
							}
						}
						if (flag2)
						{
							UpdateEmbbedCRL(dictionary4, certificateList3);
						}
					}
					else
					{
						UpdateEmbbedCRL(dictionary4, certificateList3);
					}
				}
				else
				{
					UpdateEmbbedCRL(dictionary4, certificateList3);
				}
			}
		}
		if (dictionary4.Count > 0)
		{
			foreach (KeyValuePair<string, CertificateCollection> item3 in dictionary4)
			{
				list2.Add(item3.Value);
			}
		}
		dictionary4.Clear();
		if (list2.Count > 0)
		{
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				CertificateCollection certificateCollection = list2[num4];
				string distinguishedAttributes6 = GetDistinguishedAttributes(certificateCollection.Issuer.ToString(), "CN");
				string key = string.Empty;
				if (m_certSerialNumbers.Contains(distinguishedAttributes6))
				{
					int num5 = m_certSerialNumbers.IndexOf(distinguishedAttributes6);
					key = m_certSerialNumbers[num5 - 1];
				}
				X509Certificate value3 = null;
				dictionary5.TryGetValue(distinguishedAttributes6, out value3);
				if (value3 != null && certificateCollection.Issuer.Equivalent(value3.IssuerDN) && Signature.SignedDate.CompareTo(certificateCollection.NextUpdate.ToDateTime()) < 0)
				{
					isRevokedCRL = certificateCollection.IsRevoked(value3);
				}
				PdfSignerCertificate value4 = null;
				dictionary.TryGetValue(key, out value4);
				if (value4 == null)
				{
					continue;
				}
				PdfRevocationCertificate pdfRevocationCertificate3 = new PdfRevocationCertificate();
				pdfRevocationCertificate3.Certificates = new X509Certificate2[1];
				X509Certificate2 value5 = null;
				dictionary2.TryGetValue(distinguishedAttributes6, out value5);
				pdfRevocationCertificate3.Certificates[0] = value5;
				if (value5 == null)
				{
					continue;
				}
				value4.CrlCertificate = pdfRevocationCertificate3;
				pdfRevocationCertificate3.IsEmbedded = true;
				if (certificateCollection.CurrentUpdate != null)
				{
					pdfRevocationCertificate3.ValidFrom = certificateCollection.CurrentUpdate.ToDateTime();
				}
				if (certificateCollection.NextUpdate != null)
				{
					pdfRevocationCertificate3.ValidTo = certificateCollection.NextUpdate.ToDateTime();
				}
				pdfRevocationCertificate3.IsRevokedCRL = isRevokedCRL;
				if (certificateCollection.RevokedCertificates != null && certificateCollection.RevokedCertificates.Length != 0)
				{
					pdfRevocationCertificate3.RevokedCertificates = new RevokedCertificate[certificateCollection.RevokedCertificates.Length];
					for (int num6 = 0; num6 < certificateCollection.RevokedCertificates.Length; num6++)
					{
						RevokedCertificate revokedCertificate = new RevokedCertificate();
						revokedCertificate.ValidTo = certificateCollection.RevokedCertificates[num6].m_revocationDate.ToDateTime();
						revokedCertificate.SerialNumber = certificateCollection.RevokedCertificates[num6].m_serialNumber;
						pdfRevocationCertificate3.RevokedCertificates[num6] = revokedCertificate;
					}
				}
			}
		}
		Close();
		dictionary.Clear();
		dictionary2.Clear();
		m_embbedOCSPs.Clear();
		m_embbedOCSPs = null;
		m_crlSequence = null;
		if (m_dssOCSPs != null)
		{
			m_dssOCSPs.Clear();
		}
		m_dssOCSPs = null;
	}

	internal void UpdateTimeStampSignerDetails(List<PdfSignerCertificate> signerCertificates)
	{
		Dictionary<string, PdfSignerCertificate> dictionary = new Dictionary<string, PdfSignerCertificate>();
		Dictionary<string, X509Certificate2> dictionary2 = new Dictionary<string, X509Certificate2>();
		Dictionary<string, X509RevocationResponse> dictionary3 = new Dictionary<string, X509RevocationResponse>();
		Dictionary<string, CertificateCollection> dictionary4 = new Dictionary<string, CertificateCollection>();
		UpdateDSSCollection();
		List<X509Certificate> certificates = m_certificates;
		List<X509Certificate2> list = TimeStampChainCertificates();
		bool flag = IsDssContainsCertCollection();
		X509CertificateParser x509CertificateParser = new X509CertificateParser();
		List<X509Certificate> list2 = new List<X509Certificate>();
		for (int i = 0; i < list.Count; i++)
		{
			X509Certificate x509Certificate = x509CertificateParser.ReadCertificate(list[i].RawData);
			if (x509Certificate != null)
			{
				list2.Add(x509Certificate);
			}
		}
		m_certificates = list2;
		bool rootFound = false;
		if (flag)
		{
			CheckRootCertficate(out rootFound);
			if (!rootFound)
			{
				UpdateCertCollection();
			}
		}
		X509Certificate2 value = null;
		for (int num = CertificateList.Count - 1; num >= 0; num--)
		{
			X509Certificate x509Certificate2 = CertificateList[num];
			PdfSignerCertificate pdfSignerCertificate = new PdfSignerCertificate();
			X509Certificate2 x509Certificate4 = (pdfSignerCertificate.Certificate = new X509Certificate2(x509Certificate2.GetEncoded()));
			signerCertificates.Add(pdfSignerCertificate);
			dictionary[x509Certificate2.SerialNumber.ToString()] = pdfSignerCertificate;
			string distinguishedAttributes = GetDistinguishedAttributes(x509Certificate2.IssuerDN.ToString(), "CN");
			if (distinguishedAttributes == string.Empty)
			{
				distinguishedAttributes = GetDistinguishedAttributes(x509Certificate2.IssuerDN.ToString(), "OU");
			}
			string distinguishedAttributes2 = GetDistinguishedAttributes(x509Certificate2.SubjectDN.ToString(), "CN");
			if (distinguishedAttributes != distinguishedAttributes2)
			{
				dictionary2[distinguishedAttributes] = value;
			}
			value = x509Certificate4;
		}
		List<X509RevocationResponse> list3 = new List<X509RevocationResponse>();
		List<CertificateCollection> list4 = new List<CertificateCollection>();
		UpdateIntermediateURIs();
		if (m_embbedOCSPs != null && m_embbedOCSPs.Count > 0)
		{
			UpdateEmbbedOCSP(dictionary3);
		}
		if (m_ocspByteCollection != null && m_ocspByteCollection.Count > 0)
		{
			foreach (byte[] item in m_ocspByteCollection)
			{
				UpdateFromDSS(item, dictionary3);
			}
			UpdateDSSOCSP(dictionary3);
		}
		if (dictionary3.Count > 0)
		{
			foreach (KeyValuePair<string, X509RevocationResponse> item2 in dictionary3)
			{
				list3.Add(item2.Value);
			}
		}
		dictionary3.Clear();
		if (list3.Count > 0)
		{
			for (int j = 0; j < list3.Count; j++)
			{
				OneTimeResponse[] responses = list3[j].Responses;
				foreach (OneTimeResponse oneTimeResponse in responses)
				{
					if (m_certSerialNumbers == null || m_certSerialNumbers.Count <= 0)
					{
						continue;
					}
					PdfSignerCertificate value2 = null;
					dictionary.TryGetValue(oneTimeResponse.CertificateID.SerialNumber.ToString(), out value2);
					if (value2 == null)
					{
						continue;
					}
					PdfRevocationCertificate pdfRevocationCertificate2 = (value2.OcspCertificate = new PdfRevocationCertificate());
					pdfRevocationCertificate2.IsEmbedded = true;
					if (oneTimeResponse.CurrentUpdate != null)
					{
						pdfRevocationCertificate2.ValidFrom = oneTimeResponse.CurrentUpdate.ToDateTime();
					}
					if (oneTimeResponse.NextUpdate != null)
					{
						pdfRevocationCertificate2.ValidTo = oneTimeResponse.NextUpdate.ToDateTime();
					}
					X509Certificate[] certificates2 = list3[j].Certificates;
					List<X509Certificate2> list5 = new List<X509Certificate2>();
					if (certificates2.Length > 1)
					{
						for (int l = 0; l < certificates2.Length; l++)
						{
							list5.Add(new X509Certificate2(certificates2[l].GetEncoded()));
						}
					}
					else
					{
						string text = string.Empty;
						for (int m = 0; m < certificates2.Length; m++)
						{
							text = GetDistinguishedAttributes(certificates2[m].IssuerDN.ToString(), "CN");
							list5.Add(new X509Certificate2(certificates2[m].GetEncoded()));
						}
						string text2 = text;
						while (text2 != null)
						{
							X509Certificate2 value3 = null;
							dictionary2.TryGetValue(text2, out value3);
							if (value3 != null && !list5.Contains(value3))
							{
								list5.Add(value3);
								text2 = GetDistinguishedAttributes(value3.IssuerName.Name, "CN");
								string distinguishedAttributes3 = GetDistinguishedAttributes(value3.SubjectName.Name, "CN");
								if (text2 == distinguishedAttributes3)
								{
									text2 = null;
								}
							}
							else
							{
								text2 = null;
							}
						}
					}
					list5.Reverse();
					pdfRevocationCertificate2.Certificates = list5.ToArray();
				}
			}
		}
		if (m_crlSequence != null && m_crlSequence.Count > 0)
		{
			for (int n = 0; n < m_crlSequence.Count; n++)
			{
				Asn1Sequence asn1Sequence = (Asn1Sequence)m_crlSequence[n];
				if (asn1Sequence != null)
				{
					CertificateCollection certificateList = CertificateCollection.GetCertificateList(asn1Sequence);
					if (certificateList != null)
					{
						UpdateEmbbedCRL(dictionary4, certificateList);
					}
				}
			}
		}
		if (m_crlByteCollection != null)
		{
			foreach (byte[] item3 in m_crlByteCollection)
			{
				CertificateCollection certificateList2 = null;
				UpdateCertificateCollection(item3, out certificateList2);
				if (certificateList2 != null)
				{
					UpdateEmbbedCRL(dictionary4, certificateList2);
				}
			}
		}
		if (dictionary4.Count > 0)
		{
			foreach (KeyValuePair<string, CertificateCollection> item4 in dictionary4)
			{
				list4.Add(item4.Value);
			}
		}
		dictionary4.Clear();
		if (list4.Count > 0)
		{
			for (int num2 = 0; num2 < list4.Count; num2++)
			{
				CertificateCollection certificateCollection = list4[num2];
				string distinguishedAttributes4 = GetDistinguishedAttributes(certificateCollection.Issuer.ToString(), "CN");
				string key = string.Empty;
				if (m_certSerialNumbers.Contains(distinguishedAttributes4))
				{
					int num3 = m_certSerialNumbers.IndexOf(distinguishedAttributes4);
					key = m_certSerialNumbers[num3 - 1];
				}
				PdfSignerCertificate value4 = null;
				dictionary.TryGetValue(key, out value4);
				if (value4 != null)
				{
					PdfRevocationCertificate pdfRevocationCertificate4 = (value4.CrlCertificate = new PdfRevocationCertificate());
					pdfRevocationCertificate4.IsEmbedded = true;
					if (certificateCollection.CurrentUpdate != null)
					{
						pdfRevocationCertificate4.ValidFrom = certificateCollection.CurrentUpdate.ToDateTime();
					}
					if (certificateCollection.NextUpdate != null)
					{
						pdfRevocationCertificate4.ValidTo = certificateCollection.NextUpdate.ToDateTime();
					}
					pdfRevocationCertificate4.Certificates = new X509Certificate2[1];
					X509Certificate2 value5 = null;
					dictionary2.TryGetValue(distinguishedAttributes4, out value5);
					pdfRevocationCertificate4.Certificates[0] = value5;
				}
			}
		}
		Close();
		dictionary.Clear();
		dictionary2.Clear();
		if (m_dssOCSPs != null)
		{
			m_dssOCSPs.Clear();
		}
		m_dssOCSPs = null;
		m_certificates = certificates;
	}

	private List<X509Certificate2> TimeStampChainCertificates()
	{
		if (m_timestampCertificates != null)
		{
			List<X509Certificate> list = new X509CertificateParser().ReadCertificates(m_timestampCertificates) as List<X509Certificate>;
			List<X509Certificate2> list2 = new List<X509Certificate2>();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list2.Add(new X509Certificate2(list[num].CertificateStructure.GetDerEncoded()));
			}
			if (list2.Count > 1 && list2[0].IssuerName.Name == list2[0].SubjectName.Name)
			{
				list2.Reverse();
			}
			return list2;
		}
		return null;
	}

	internal List<X509Certificate2> GetTimeStampCertificates(List<X509Certificate2> certificates)
	{
		if (m_timestampCertificates != null)
		{
			List<X509Certificate> list = new X509CertificateParser().ReadCertificates(m_timestampCertificates) as List<X509Certificate>;
			for (int num = list.Count - 1; num >= 0; num--)
			{
				certificates.Add(new X509Certificate2(list[num].CertificateStructure.GetDerEncoded()));
			}
		}
		return certificates;
	}

	private bool IsDssContainsCertCollection()
	{
		if (Signature.m_document != null && Signature.m_document.Catalog != null && Signature.m_document.Catalog.ContainsKey("DSS") && PdfCrossTable.Dereference(Signature.m_document.Catalog["DSS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Certs") && PdfCrossTable.Dereference(pdfDictionary["Certs"]) is PdfArray { Count: >0 })
		{
			return true;
		}
		return false;
	}

	private void GetExtensionUrls(X509Extension extension, out List<string> crlUrls)
	{
		crlUrls = new List<string>();
		try
		{
			if (!(X509Extension.ConvertValueToObject(extension) is Asn1Sequence asn1Sequence))
			{
				return;
			}
			for (int i = 0; i != asn1Sequence.Count; i++)
			{
				Asn1Tag tag = Asn1Tag.GetTag(asn1Sequence[i]);
				if (tag == null || tag.TagNumber != 0)
				{
					continue;
				}
				Asn1Sequence sequence = Asn1Sequence.GetSequence(tag, explicitly: false);
				if (sequence != null)
				{
					sequence = Asn1Sequence.GetSequence(Asn1Tag.GetTag(sequence[0]), explicitly: false);
				}
				if (sequence == null || !(sequence.GetAsn1() is Asn1Sequence asn1Sequence2))
				{
					continue;
				}
				foreach (Asn1Tag item in asn1Sequence2)
				{
					if (item.TagNumber == 6)
					{
						string @string = DerAsciiString.GetAsciiString((Asn1Tag)item.GetAsn1(), isExplicit: false).GetString();
						if (@string.ToLower().EndsWith(".crl") || !string.IsNullOrEmpty(Path.GetExtension(@string)))
						{
							crlUrls.Add(@string);
						}
					}
				}
			}
		}
		catch
		{
		}
	}

	private void CheckRootCertficate(out bool rootFound)
	{
		rootFound = false;
		if (m_certificates == null || m_certificates.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_certificates.Count; i++)
		{
			X509Certificate x509Certificate = m_certificates[i];
			if (x509Certificate != null)
			{
				string distinguishedAttributes = GetDistinguishedAttributes(x509Certificate.IssuerDN.ToString(), "CN");
				if (distinguishedAttributes == string.Empty)
				{
					distinguishedAttributes = GetDistinguishedAttributes(x509Certificate.IssuerDN.ToString(), "OU");
				}
				string distinguishedAttributes2 = GetDistinguishedAttributes(x509Certificate.SubjectDN.ToString(), "CN");
				if (distinguishedAttributes == distinguishedAttributes2)
				{
					rootFound = true;
					break;
				}
			}
		}
	}

	private void UpdateCertCollection()
	{
		List<byte[]> list = new List<byte[]>();
		try
		{
			if (Signature.m_document != null && Signature.m_document.Catalog != null && Signature.m_document.Catalog.ContainsKey("DSS") && PdfCrossTable.Dereference(Signature.m_document.Catalog["DSS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Certs"))
			{
				PdfArray array = PdfCrossTable.Dereference(pdfDictionary["Certs"]) as PdfArray;
				list = GetByteCollection(array);
			}
			X509CertificateParser x509CertificateParser = new X509CertificateParser();
			IList<X509Certificate> list2 = new List<X509Certificate>();
			foreach (byte[] item in list)
			{
				list2.Add(x509CertificateParser.ReadCertificate(item));
			}
			Dictionary<string, List<X509Certificate>> dictionary = new Dictionary<string, List<X509Certificate>>();
			if (list2.Count > 0)
			{
				foreach (X509Certificate item2 in list2)
				{
					if (GetDistinguishedAttributes(item2.IssuerDN.ToString(), "CN") == string.Empty)
					{
						GetDistinguishedAttributes(item2.IssuerDN.ToString(), "OU");
					}
					string distinguishedAttributes = GetDistinguishedAttributes(item2.SubjectDN.ToString(), "CN");
					if (!dictionary.ContainsKey(distinguishedAttributes))
					{
						List<X509Certificate> list3 = new List<X509Certificate>();
						list3.Add(item2);
						dictionary.Add(distinguishedAttributes, list3);
					}
					else
					{
						List<X509Certificate> value = null;
						dictionary.TryGetValue(distinguishedAttributes, out value);
						value?.Add(item2);
					}
				}
			}
			string text = GetDistinguishedAttributes(CertificateList[CertificateList.Count - 1].IssuerDN.ToString(), "CN");
			if (text == string.Empty)
			{
				text = GetDistinguishedAttributes(CertificateList[CertificateList.Count - 1].IssuerDN.ToString(), "OU");
			}
			string distinguishedAttributes2 = GetDistinguishedAttributes(CertificateList[CertificateList.Count - 1].SubjectDN.ToString(), "CN");
			List<X509Certificate> list4 = new List<X509Certificate>();
			list4.Add(CertificateList[CertificateList.Count - 1]);
			while (text != null && dictionary.ContainsKey(text))
			{
				foreach (X509Certificate item3 in dictionary[text])
				{
					X509Certificate x509Certificate = list4[list4.Count - 1];
					try
					{
						x509Certificate.Verify(item3.GetPublicKey());
						list4.Add(item3);
						text = GetDistinguishedAttributes(item3.IssuerDN.ToString(), "CN");
						if (text == string.Empty)
						{
							text = GetDistinguishedAttributes(item3.IssuerDN.ToString(), "OU");
						}
						distinguishedAttributes2 = GetDistinguishedAttributes(item3.SubjectDN.ToString(), "CN");
						if (text == distinguishedAttributes2)
						{
							text = null;
						}
					}
					catch
					{
						text = null;
						continue;
					}
					break;
				}
			}
			if (list4.Count > 1)
			{
				m_certificates = list4;
			}
		}
		catch
		{
		}
	}
}
