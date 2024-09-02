namespace DocGen.Pdf.Security;

internal class PfxData : Asn1Encode
{
	private ContentInformation m_contentInformation;

	private MacInformation m_macInformation;

	internal ContentInformation ContentInformation => m_contentInformation;

	internal PfxData(Asn1Sequence sequence)
	{
		_ = ((DerInteger)sequence[0]).Value;
		m_contentInformation = DocGen.Pdf.Security.ContentInformation.GetInformation(sequence[1]);
		if (sequence.Count == 3)
		{
			m_macInformation = MacInformation.GetInformation(sequence[2]);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(new DerInteger(3), m_contentInformation);
		if (m_macInformation != null)
		{
			asn1EncodeCollection.Add(m_macInformation);
		}
		return new BerSequence(asn1EncodeCollection);
	}
}
