using System;

namespace DocGen.Pdf.Security;

internal class TimeStampElement : Asn1Encode
{
	private DerObjectID m_type;

	private Asn1Set m_values;

	internal DerObjectID Type => m_type;

	internal Asn1Set Values => m_values;

	internal static TimeStampElement GetTimeStampElement(object obj)
	{
		if (obj == null || obj is TimeStampElement)
		{
			return (TimeStampElement)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new TimeStampElement((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence " + obj.GetType().Name, "obj");
	}

	internal TimeStampElement(Asn1Sequence sequence)
	{
		m_type = (DerObjectID)sequence[0];
		m_values = (Asn1Set)sequence[1];
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_type, m_values);
	}
}
