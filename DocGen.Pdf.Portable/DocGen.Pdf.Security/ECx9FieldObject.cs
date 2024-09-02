namespace DocGen.Pdf.Security;

internal class ECx9FieldObject : Asn1Encode
{
	private readonly DerObjectID m_identifier;

	private readonly Asn1 m_param;

	public DerObjectID Identifier => m_identifier;

	public Asn1 Parameters => m_param;

	public ECx9FieldObject(Number primePNum)
	{
		m_identifier = ECDSAOIDs.X90UniqueID;
		m_param = new DerInteger(primePNum);
	}

	public ECx9FieldObject(int num, int num1, int num2, int num3)
	{
		m_identifier = ECDSAOIDs.X90RecordID;
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(new DerInteger(num));
		if (num2 == 0)
		{
			asn1EncodeCollection.Add(ECDSAOIDs.X90TNObjID, new DerInteger(num1));
		}
		else
		{
			asn1EncodeCollection.Add(ECDSAOIDs.X90PPObjID, new DerSequence(new DerInteger(num1), new DerInteger(num2), new DerInteger(num3)));
		}
		m_param = new DerSequence(asn1EncodeCollection);
	}

	internal ECx9FieldObject(Asn1Sequence sequence)
	{
		m_identifier = (DerObjectID)sequence[0];
		m_param = (Asn1)sequence[1];
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_identifier, m_param);
	}
}
