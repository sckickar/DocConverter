namespace DocGen.Pdf.Security;

internal class DerSequenceHelper : IAsn1Collection, IAsn1
{
	private Asn1Parser m_helper;

	internal DerSequenceHelper(Asn1Parser helper)
	{
		m_helper = helper;
	}

	public IAsn1 ReadObject()
	{
		return m_helper.ReadObject();
	}

	public Asn1 GetAsn1()
	{
		return new DerSequence(m_helper.ReadCollection());
	}
}
