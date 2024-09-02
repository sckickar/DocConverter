namespace DocGen.Pdf.Security;

internal class OcspRequestCollection : Asn1Encode
{
	private static readonly DerInteger m_integer = new DerInteger(0);

	private readonly DerInteger m_version;

	private readonly OcspTag m_requestorName;

	private readonly Asn1Sequence m_requestList;

	private readonly X509Extensions m_requestExtensions;

	private bool m_versionSet;

	public OcspRequestCollection(OcspTag requestorName, Asn1Sequence requestList, X509Extensions requestExtensions)
	{
		m_version = m_integer;
		m_requestorName = requestorName;
		m_requestList = requestList;
		m_requestExtensions = requestExtensions;
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (!m_version.Equals(m_integer) || m_versionSet)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_version));
		}
		if (m_requestorName != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 1, m_requestorName));
		}
		asn1EncodeCollection.Add(m_requestList);
		if (m_requestExtensions != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 2, m_requestExtensions));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
