using System;
using System.Collections;

namespace DocGen.Pdf.Security;

internal class BerTag : DerTag
{
	internal BerTag(int tagNumber, Asn1Encode asn1)
		: base(tagNumber, asn1)
	{
	}

	internal BerTag(bool IsExplicit, int tagNumber, Asn1Encode asn1)
		: base(IsExplicit, tagNumber, asn1)
	{
	}

	internal BerTag(int tagNumber)
		: base(isExplicit: false, tagNumber, BerSequence.Empty)
	{
	}

	internal new void Encode(DerStream stream)
	{
		if (stream is Asn1DerStream)
		{
			stream.WriteTag(160, m_tagNumber);
			stream.m_stream.WriteByte(128);
			if (!base.IsEmpty)
			{
				if (!m_isExplicit)
				{
					IEnumerable enumerable;
					if (m_object is Asn1Octet)
					{
						enumerable = ((!(m_object is BerOctet)) ? new BerOctet(((Asn1Octet)m_object).GetOctets()) : ((BerOctet)m_object));
					}
					else if (m_object is Asn1Sequence)
					{
						enumerable = (Asn1Sequence)m_object;
					}
					else
					{
						if (!(m_object is Asn1Set))
						{
							throw new Exception(m_object.GetType().Name);
						}
						enumerable = (Asn1Set)m_object;
					}
					foreach (Asn1Encode item in enumerable)
					{
						stream.WriteObject(item);
					}
				}
				else
				{
					stream.WriteObject(m_object);
				}
			}
			stream.m_stream.WriteByte(0);
			stream.m_stream.WriteByte(0);
		}
		else
		{
			base.Encode(stream);
		}
	}
}
