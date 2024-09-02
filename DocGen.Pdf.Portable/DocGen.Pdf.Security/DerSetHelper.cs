namespace DocGen.Pdf.Security;

internal class DerSetHelper : IAsn1SetHelper, IAsn1
{
	private Asn1Parser m_helper;

	internal DerSetHelper(Asn1Parser helper)
	{
		m_helper = helper;
	}

	public IAsn1 ReadObject()
	{
		return m_helper.ReadObject();
	}

	public Asn1 GetAsn1()
	{
		return new DerSet(m_helper.ReadCollection(), isSort: false);
	}
}
