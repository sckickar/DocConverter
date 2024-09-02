using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerName : Asn1Encode
{
	private Asn1Encode m_name;

	private int m_tagNumber;

	internal DerName(int tagNumber, Asn1Encode name)
	{
		m_name = name;
		m_tagNumber = tagNumber;
	}

	internal static DerName GetDerName(object name)
	{
		if (name == null || name is DerName)
		{
			return (DerName)name;
		}
		if (name is Asn1Tag)
		{
			Asn1Tag asn1Tag = (Asn1Tag)name;
			int tagNumber = asn1Tag.TagNumber;
			switch (tagNumber)
			{
			case 0:
				return new DerName(tagNumber, Asn1Sequence.GetSequence(asn1Tag, explicitly: false));
			case 1:
				return new DerName(tagNumber, DerAsciiString.GetAsciiString(asn1Tag, isExplicit: false));
			case 2:
				return new DerName(tagNumber, DerAsciiString.GetAsciiString(asn1Tag, isExplicit: false));
			case 3:
				throw new ArgumentException("Invalid entry in sequence" + tagNumber);
			case 4:
				return new DerName(tagNumber, X509Name.GetName(asn1Tag, isExplicit: true));
			case 5:
				return new DerName(tagNumber, Asn1Sequence.GetSequence(asn1Tag, explicitly: false));
			case 6:
				return new DerName(tagNumber, DerAsciiString.GetAsciiString(asn1Tag, isExplicit: false));
			case 7:
				return new DerName(tagNumber, Asn1Octet.GetOctetString(asn1Tag, isExplicit: false));
			case 8:
				return new DerName(tagNumber, DerObjectID.GetID(asn1Tag, isExplicit: false));
			}
		}
		if (name is byte[])
		{
			try
			{
				return GetDerName(Asn1.FromByteArray((byte[])name));
			}
			catch (Exception ex)
			{
				throw new ArgumentException(ex.Message);
			}
		}
		throw new ArgumentException("Invalid entry in sequence " + name.GetType().FullName, "obj");
	}

	internal static DerName GetDerName(Asn1Tag tag, bool isExplicit)
	{
		return GetDerName(Asn1Tag.GetTag(tag, isExplicit: true));
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_tagNumber);
		stringBuilder.Append(": ");
		switch (m_tagNumber)
		{
		case 1:
		case 2:
		case 6:
			stringBuilder.Append(DerAsciiString.GetAsciiString(m_name).GetString());
			break;
		case 4:
			stringBuilder.Append(X509Name.GetName(m_name).ToString());
			break;
		default:
			stringBuilder.Append(m_name.ToString());
			break;
		}
		return stringBuilder.ToString();
	}

	public override Asn1 GetAsn1()
	{
		return new DerTag(m_tagNumber == 4, m_tagNumber, m_name);
	}
}
