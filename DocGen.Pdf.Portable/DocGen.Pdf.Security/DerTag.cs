namespace DocGen.Pdf.Security;

internal class DerTag : Asn1Tag
{
	internal DerTag(int tagNumber, Asn1Encode asn1)
		: base(tagNumber, asn1)
	{
	}

	internal DerTag(bool isExplicit, int tagNumber, Asn1Encode asn1)
		: base(isExplicit, tagNumber, asn1)
	{
	}

	internal DerTag(int tagNumber)
		: base(isExplicit: false, tagNumber, DerSequence.Empty)
	{
	}

	internal override void Encode(DerStream stream)
	{
		if (!base.IsEmpty)
		{
			byte[] derEncoded = m_object.GetDerEncoded();
			if (m_isExplicit)
			{
				stream.WriteEncoded(160, m_tagNumber, derEncoded);
				return;
			}
			int flag = (derEncoded[0] & 0x20) | 0x80;
			stream.WriteTag(flag, m_tagNumber);
			stream.m_stream.Write(derEncoded, 1, derEncoded.Length - 1);
		}
		else
		{
			stream.WriteEncoded(160, m_tagNumber, new byte[0]);
		}
	}
}
