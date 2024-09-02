using System;

namespace DocGen.Pdf.Security;

internal class ContentInformation : Asn1Encode
{
	private DerObjectID m_contentType;

	private Asn1Encode m_content;

	internal DerObjectID ContentType => m_contentType;

	internal Asn1Encode Content => m_content;

	internal static ContentInformation GetInformation(object obj)
	{
		if (obj == null || obj is ContentInformation)
		{
			return (ContentInformation)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new ContentInformation((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry");
	}

	private ContentInformation(Asn1Sequence sequence)
	{
		if (sequence.Count < 1 || sequence.Count > 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_contentType = (DerObjectID)sequence[0];
		if (sequence.Count > 1)
		{
			Asn1Tag asn1Tag = (Asn1Tag)sequence[1];
			if (!asn1Tag.IsExplicit || asn1Tag.TagNumber != 0)
			{
				throw new ArgumentException("Invalid tag");
			}
			m_content = asn1Tag.GetObject();
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_contentType);
		if (m_content != null)
		{
			asn1EncodeCollection.Add(new BerTag(0, m_content));
		}
		return new BerSequence(asn1EncodeCollection);
	}
}
