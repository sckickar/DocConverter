using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerNames : Asn1Encode
{
	private readonly DerName[] m_names;

	internal static DerNames GetDerNames(object obj)
	{
		if (obj == null || obj is DerNames)
		{
			return (DerNames)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new DerNames((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence " + obj.GetType().Name, "obj");
	}

	private DerNames(Asn1Sequence sequence)
	{
		m_names = new DerName[sequence.Count];
		for (int i = 0; i != sequence.Count; i++)
		{
			m_names[i] = DerName.GetDerName(sequence[i]);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1Encode[] names = m_names;
		return new DerSequence(names);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DerNames:");
		stringBuilder.Append(Environment.NewLine);
		DerName[] names = m_names;
		foreach (DerName value in names)
		{
			stringBuilder.Append("    ");
			stringBuilder.Append(value);
			stringBuilder.Append(Environment.NewLine);
		}
		return stringBuilder.ToString();
	}
}
