namespace DocGen.Pdf.Security;

internal class Asn1SequenceCollection : Asn1Encode
{
	private DerObjectID m_id;

	private Asn1 m_value;

	private Asn1Set m_attributes;

	internal DerObjectID ID => m_id;

	internal Asn1 Value => m_value;

	internal Asn1Set Attributes => m_attributes;

	internal Asn1SequenceCollection(Asn1Sequence sequence)
	{
		m_id = DerObjectID.GetID(sequence[0]);
		m_value = (sequence[1] as Asn1Tag).GetObject();
		if (sequence.Count == 3)
		{
			m_attributes = (DerSet)sequence[2];
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_id, new DerTag(0, m_value));
		if (m_attributes != null)
		{
			asn1EncodeCollection.Add(m_attributes);
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
