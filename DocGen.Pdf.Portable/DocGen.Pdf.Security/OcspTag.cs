using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class OcspTag : Asn1Encode
{
	internal Asn1Encode m_encode;

	internal int m_tagNumber;

	internal int TagNumber => m_tagNumber;

	internal OcspTag(int tag, Asn1Encode encode)
	{
		m_encode = encode;
		m_tagNumber = tag;
	}

	internal OcspTag()
	{
	}

	internal OcspTag GetOcspName(object obj)
	{
		if (obj == null || obj is OcspTag)
		{
			return (OcspTag)obj;
		}
		if (obj is Asn1Tag)
		{
			Asn1Tag asn1Tag = (Asn1Tag)obj;
			int tagNumber = asn1Tag.TagNumber;
			switch (tagNumber)
			{
			case 0:
				return new OcspTag(tagNumber, Asn1Sequence.GetSequence(asn1Tag, explicitly: false));
			case 1:
				return new OcspTag(tagNumber, DerAsciiString.GetAsciiString(asn1Tag, isExplicit: false));
			case 2:
				return new OcspTag(tagNumber, DerAsciiString.GetAsciiString(asn1Tag, isExplicit: false));
			case 3:
				throw new ArgumentException("Invalid tag number specified" + tagNumber);
			case 4:
				return new OcspTag(tagNumber, X509Name.GetName(asn1Tag, isExplicit: true));
			case 5:
				return new OcspTag(tagNumber, Asn1Sequence.GetSequence(asn1Tag, explicitly: false));
			case 6:
				return new OcspTag(tagNumber, DerAsciiString.GetAsciiString(asn1Tag, isExplicit: false));
			case 7:
				return new OcspTag(tagNumber, Asn1Octet.GetOctetString(asn1Tag, isExplicit: false));
			case 8:
				return new OcspTag(tagNumber, DerObjectID.GetID(asn1Tag, isExplicit: false));
			}
		}
		if (obj is byte[])
		{
			try
			{
				return GetOcspName(Asn1.FromByteArray((byte[])obj));
			}
			catch (IOException)
			{
				throw new ArgumentException("Invalid OCSP name to parse");
			}
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		return new DerTag(m_tagNumber == 4, m_tagNumber, m_encode);
	}
}
