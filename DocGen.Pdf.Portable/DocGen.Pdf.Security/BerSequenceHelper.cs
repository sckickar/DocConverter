namespace DocGen.Pdf.Security;

internal class BerSequenceHelper : IAsn1Collection, IAsn1
{
	private Asn1Parser m_helper;

	internal BerSequenceHelper(Asn1Parser helper)
	{
		m_helper = helper;
	}

	public IAsn1 ReadObject()
	{
		return m_helper.ReadObject();
	}

	public Asn1 GetAsn1()
	{
		return new BerSequence(m_helper.ReadCollection());
	}
}
