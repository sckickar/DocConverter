namespace DocGen.Pdf.Security;

internal class ECX962Params : Asn1Encode, IAsn1Choice
{
	private readonly Asn1 m_parameters;

	public bool IsNamedCurve => m_parameters is DerObjectID;

	public Asn1 Parameters => m_parameters;

	public ECX962Params(ECX9Field ecParameters)
	{
		m_parameters = ecParameters.GetAsn1();
	}

	public ECX962Params(DerObjectID namedCurve)
	{
		m_parameters = namedCurve;
	}

	public ECX962Params(Asn1 obj)
	{
		m_parameters = obj;
	}

	public override Asn1 GetAsn1()
	{
		return m_parameters;
	}
}
