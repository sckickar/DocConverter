using System;

namespace DocGen.Pdf.Security;

internal class TimeStampRequestCreator : Asn1
{
	private const string c_IdSHA256 = "2.16.840.1.101.3.4.2.1";

	private const string c_IdTimeStampToken = "1.2.840.113549.1.9.16.2.14";

	private Asn1Integer m_version;

	private MessageStamp m_messageImprint;

	private Asn1Boolean m_certReq;

	public TimeStampRequestCreator(bool certReq)
	{
		m_certReq = new Asn1Boolean(certReq);
		m_version = new Asn1Integer(1L);
	}

	public byte[] GetAsnEncodedTimestampRequest(byte[] hash)
	{
		Algorithms algorithms = new Algorithms(new Asn1Identifier("2.16.840.1.101.3.4.2.1"), DerNull.Value);
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		asn1EncodeCollection.Add(new Asn1Integer(1L));
		asn1EncodeCollection.Add(new Asn1Sequence(algorithms, new Asn1Octet(hash)));
		asn1EncodeCollection.Add(new Asn1Integer(100L));
		asn1EncodeCollection.Add(new Asn1Boolean(value: true));
		return new Asn1Sequence(asn1EncodeCollection).AsnEncode();
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		throw new NotImplementedException();
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}

	internal override void Encode(DerStream derOut)
	{
		throw new NotImplementedException();
	}
}
