using System;

namespace DocGen.Pdf.Security;

internal class ECGostAlgorithm : Asn1Encode
{
	private DerObjectID m_publicKey;

	private DerObjectID m_digestParam;

	private DerObjectID m_encryptParam;

	public ECGostAlgorithm(DerObjectID m_publicKey, DerObjectID m_digestParam)
		: this(m_publicKey, m_digestParam, null)
	{
	}

	public ECGostAlgorithm(DerObjectID m_publicKey, DerObjectID m_digestParam, DerObjectID m_encryptParam)
	{
		if (m_publicKey == null)
		{
			throw new ArgumentNullException("publicKey");
		}
		if (m_digestParam == null)
		{
			throw new ArgumentNullException("digestParam");
		}
		this.m_publicKey = m_publicKey;
		this.m_digestParam = m_digestParam;
		this.m_encryptParam = m_encryptParam;
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_publicKey, m_digestParam);
		if (m_encryptParam != null)
		{
			asn1EncodeCollection.Add(m_encryptParam);
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
