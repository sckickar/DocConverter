using System;

namespace DocGen.Pdf.Security;

internal class OcspHelper : Asn1Encode
{
	private ResponseInformation m_responseInformation;

	private Algorithms m_algorithms;

	private DerBitString m_signature;

	private Asn1Sequence m_sequence;

	internal DerBitString Signature => m_signature;

	internal Algorithms Algorithm => m_algorithms;

	internal Asn1Sequence Sequence => m_sequence;

	public ResponseInformation ResponseInformation => m_responseInformation;

	private OcspHelper(Asn1Sequence sequence)
	{
		ResponseInformation responseInformation = new ResponseInformation();
		m_responseInformation = responseInformation.GetInformation(sequence[0]);
		m_algorithms = Algorithms.GetAlgorithms(sequence[1]);
		m_signature = (DerBitString)sequence[2];
		if (sequence.Count > 3)
		{
			m_sequence = Asn1Sequence.GetSequence((Asn1Tag)sequence[3], explicitly: true);
		}
	}

	internal OcspHelper()
	{
	}

	public OcspHelper GetOcspStructure(object obj)
	{
		if (obj == null || obj is OcspHelper)
		{
			return (OcspHelper)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OcspHelper((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_responseInformation, m_algorithms, m_signature);
		if (m_sequence != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_sequence));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
