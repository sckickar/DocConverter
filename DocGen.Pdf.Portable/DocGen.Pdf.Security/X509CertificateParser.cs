using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Security;

internal class X509CertificateParser
{
	private Asn1Set m_sData;

	private int m_sDataObjectCount;

	private Stream m_currentStream;

	private X509Certificate ReadDerCertificate(Asn1Stream dIn)
	{
		Asn1Sequence asn1Sequence = (Asn1Sequence)dIn.ReadAsn1();
		if (asn1Sequence.Count > 1 && asn1Sequence[0] is DerObjectID && asn1Sequence[0].Equals(PKCSOIDs.SignedData))
		{
			if (asn1Sequence.Count >= 2)
			{
				IEnumerator enumerator = Asn1Sequence.GetSequence((Asn1Tag)asn1Sequence[1], explicitly: true).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Asn1 asn = (Asn1)enumerator.Current;
					if (asn is Asn1Tag)
					{
						Asn1Tag asn1Tag = (Asn1Tag)asn;
						if (asn1Tag.TagNumber == 0)
						{
							m_sData = Asn1Set.GetAsn1Set(asn1Tag, isExplicit: false);
							break;
						}
					}
				}
			}
			return GetCertificate();
		}
		return CreateX509Certificate(X509CertificateStructure.GetInstance(asn1Sequence));
	}

	private X509Certificate GetCertificate()
	{
		if (m_sData != null)
		{
			while (m_sDataObjectCount < m_sData.Count)
			{
				object obj = m_sData[m_sDataObjectCount++];
				if (obj is Asn1Sequence)
				{
					return CreateX509Certificate(X509CertificateStructure.GetInstance(obj));
				}
			}
		}
		return null;
	}

	protected virtual X509Certificate CreateX509Certificate(X509CertificateStructure c)
	{
		return new X509Certificate(c);
	}

	internal X509Certificate ReadCertificate(byte[] input)
	{
		return ReadCertificate(new MemoryStream(input, writable: false));
	}

	internal ICollection ReadCertificates(byte[] input)
	{
		return ReadCertificates(new MemoryStream(input, writable: false));
	}

	internal X509Certificate ReadCertificate(Stream inStream)
	{
		if (inStream == null)
		{
			throw new ArgumentNullException("inStream");
		}
		if (!inStream.CanRead)
		{
			throw new ArgumentException("inStream must be read-able", "inStream");
		}
		if (m_currentStream == null)
		{
			m_currentStream = inStream;
			m_sData = null;
			m_sDataObjectCount = 0;
		}
		else if (m_currentStream != inStream)
		{
			m_currentStream = inStream;
			m_sData = null;
			m_sDataObjectCount = 0;
		}
		try
		{
			if (m_sData != null)
			{
				if (m_sDataObjectCount != m_sData.Count)
				{
					return GetCertificate();
				}
				m_sData = null;
				m_sDataObjectCount = 0;
				return null;
			}
			PushStream pushStream = new PushStream(inStream);
			int num = pushStream.ReadByte();
			if (num < 0)
			{
				return null;
			}
			pushStream.Unread(num);
			return ReadDerCertificate(new Asn1Stream(pushStream));
		}
		catch (Exception innerException)
		{
			throw new Exception("Failed to read certificate", innerException);
		}
	}

	internal ICollection ReadCertificates(Stream inStream)
	{
		List<X509Certificate> list = new List<X509Certificate>();
		X509Certificate item;
		while ((item = ReadCertificate(inStream)) != null)
		{
			list.Add(item);
		}
		return list;
	}
}
