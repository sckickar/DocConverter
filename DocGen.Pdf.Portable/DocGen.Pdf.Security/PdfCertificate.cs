using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DocGen.Pdf.Native;

namespace DocGen.Pdf.Security;

public class PdfCertificate
{
	private enum DistinguishedNameSeparator
	{
		InitialSeparator,
		QuoteSpearator
	}

	private const uint CryptographicUserKeyset = 4096u;

	private const uint SystemStoreCertificateCurrentUser = 65536u;

	private const uint CertificateStoreReadonlyFlag = 32768u;

	private const uint CertificateStoreOpenExistingFlag = 16384u;

	private const uint X509AsnEncoding = 1u;

	private const uint PKCS7AsnEncoding = 65536u;

	private const uint EncodingType = 65537u;

	private const int X509Name = 7;

	private const string RelativeDistinguishedName = "2.5.4.3";

	private CryptoSignMessageParamerter m_signParameters;

	private int m_version;

	private byte[] m_serialNumber;

	private string m_issuerName;

	private string m_subjectName;

	private nint m_certificate;

	private uint m_signatureLength;

	private DateTime m_validTo;

	private DateTime m_validFrom;

	private X509Certificate2 m_x509Certificate;

	internal PdfPKCSCertificate m_pkcs7Certificate;

	internal bool isPkcs7Certificate;

	internal bool isStore;

	internal X509Certificate[] Chains;

	private Dictionary<string, Dictionary<string, string>> m_distinguishedNameCollection = new Dictionary<string, Dictionary<string, string>>();

	public int Version => m_version;

	public byte[] SerialNumber => m_serialNumber;

	public string IssuerName => m_issuerName;

	public string SubjectName => m_subjectName;

	public DateTime ValidTo => m_validTo;

	public DateTime ValidFrom => m_validFrom;

	internal nint SysCertificate => m_certificate;

	internal X509Certificate2 X509Certificate => m_x509Certificate;

	public PdfCertificate(Stream certificate, string password)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("pfxPath");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		certificate.Position = 0L;
		try
		{
			InitializePkcs7Certificate(StreamToByteArray(certificate), password.ToCharArray());
		}
		catch (Exception)
		{
			isPkcs7Certificate = false;
			certificate.Position = 0L;
			Initialize(certificate, password);
		}
	}

	internal PdfCertificate(PdfCmsSigner signer)
	{
		LoadDetails(signer.SignerCertificate);
	}

	private byte[] StreamToByteArray(Stream stream)
	{
		stream.Position = 0L;
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		return array;
	}

	private void InitializePkcs7Certificate(byte[] data, char[] password)
	{
		m_pkcs7Certificate = new PdfPKCSCertificate(new MemoryStream(data), password);
		string key = "";
		foreach (string item in m_pkcs7Certificate.KeyEnumerable)
		{
			if (m_pkcs7Certificate.IsKey(item) && m_pkcs7Certificate.GetKey(item).Key.IsPrivate)
			{
				key = item;
				break;
			}
		}
		X509Certificates certificate = m_pkcs7Certificate.GetCertificate(key);
		LoadDetails(certificate.Certificate);
	}

	private void LoadDetails(X509Certificate certificate)
	{
		m_issuerName = GetDistinguishedAttributes(certificate.CertificateStructure.Issuer.ToString(), "CN");
		if (m_issuerName == string.Empty)
		{
			m_issuerName = GetDistinguishedAttributes(certificate.CertificateStructure.Issuer.ToString(), "OU");
		}
		m_subjectName = GetDistinguishedAttributes(certificate.CertificateStructure.Subject.ToString(), "CN");
		m_validFrom = certificate.CertificateStructure.StartDate.ToDateTime();
		m_validTo = certificate.CertificateStructure.EndDate.ToDateTime();
		m_version = certificate.CertificateStructure.Version;
		byte[] array = new byte[certificate.CertificateStructure.SerialNumber.m_value.Length];
		array = (byte[])certificate.CertificateStructure.SerialNumber.m_value.Clone();
		Array.Reverse(array, 0, array.Length);
		m_serialNumber = array;
		isPkcs7Certificate = true;
	}

	private string GetDistinguishedAttributes(string name, string key)
	{
		string result = string.Empty;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (key.Contains("="))
		{
			key = key.Replace("=", "");
		}
		if (!m_distinguishedNameCollection.ContainsKey(name))
		{
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
			m_distinguishedNameCollection.Add(name, dictionary);
			if (dictionary.ContainsKey(key))
			{
				result = dictionary[key];
			}
		}
		else
		{
			dictionary = m_distinguishedNameCollection[name];
			if (dictionary.ContainsKey(key))
			{
				result = dictionary[key];
			}
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

	private string GetName(string name, string key)
	{
		string[] array = name.Split(',');
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].Contains(key))
			{
				continue;
			}
			text = array[i].Replace(key, "").TrimStart(' ').TrimEnd('\\');
			if (array.Length > i + 1 && !array[i + 1].Contains("="))
			{
				text = text + "," + array[i + 1].ToString();
				if (text.Contains("+"))
				{
					text = text.Substring(0, text.IndexOf('+')).TrimEnd();
				}
			}
			else if (text.Contains("+"))
			{
				text = text.Substring(0, text.IndexOf('+')).TrimEnd();
			}
			break;
		}
		return text;
	}

	public PdfCertificate(Stream certificate, string password, KeyStorageFlags storageFlag)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException("pfxPath");
		}
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		certificate.Position = 0L;
		try
		{
			InitializePkcs7Certificate(StreamToByteArray(certificate), password.ToCharArray());
		}
		catch (Exception)
		{
			isPkcs7Certificate = false;
			certificate.Position = 0L;
			Initialize(certificate, password);
		}
	}

	internal PdfCertificate(nint certificate)
	{
		if (certificate == IntPtr.Zero)
		{
			throw new ArgumentNullException("certificate");
		}
		Initialize(certificate);
	}

	public PdfCertificate(X509Certificate2 x509Certificate2)
		: this(x509Certificate2, buildChain: true)
	{
	}

	internal PdfCertificate(X509Certificate2 x509Certificate2, bool buildChain)
	{
		isStore = true;
		m_x509Certificate = x509Certificate2;
		m_issuerName = GetName(x509Certificate2.IssuerName.Name, "CN=");
		if (m_issuerName == string.Empty)
		{
			m_issuerName = GetName(x509Certificate2.IssuerName.Name, "OU=");
		}
		m_subjectName = GetName(x509Certificate2.SubjectName.Name, "CN=");
		m_validFrom = x509Certificate2.NotBefore;
		m_validTo = x509Certificate2.NotAfter;
		m_version = x509Certificate2.Version;
		m_serialNumber = x509Certificate2.GetSerialNumber();
		if (buildChain)
		{
			X509CertificateParser x509CertificateParser = new X509CertificateParser();
			X509Chain x509Chain = new X509Chain();
			x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
			x509Chain.Build(x509Certificate2);
			Chains = new X509Certificate[x509Chain.ChainElements.Count];
			for (int i = 0; i < x509Chain.ChainElements.Count; i++)
			{
				Chains[i] = x509CertificateParser.ReadCertificate(x509Chain.ChainElements[i].Certificate.RawData);
			}
		}
	}

	private void Initialize(Stream certificate, string password)
	{
	}

	private void Initialize(nint certificate)
	{
	}

	internal uint GetSignatureLength()
	{
		return m_signatureLength;
	}

	private static bool Equals(byte[] arr1, byte[] arr2)
	{
		if (arr1 == null)
		{
			throw new ArgumentNullException("arr1");
		}
		if (arr2 == null)
		{
			throw new ArgumentNullException("arr2");
		}
		bool flag = arr1.Length == arr2.Length;
		if (flag)
		{
			for (int i = 0; i < arr1.Length; i++)
			{
				if (arr1[i] != arr2[i])
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}
}
