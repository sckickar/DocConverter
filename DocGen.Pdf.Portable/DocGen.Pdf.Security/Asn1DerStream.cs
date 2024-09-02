using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1DerStream : DerStream
{
	private new MemoryStream m_stream;

	internal Asn1DerStream()
	{
		m_stream = new MemoryStream();
	}

	public Asn1DerStream(Stream stream)
		: base(stream)
	{
	}

	internal byte[] ParseTimeStamp(Asn1 encodedObject)
	{
		Asn1DerStream asn1DerStream = new Asn1DerStream();
		m_stream.WriteByte(48);
		m_stream.WriteByte(128);
		if ((encodedObject as Asn1Sequence)[0] is Asn1Identifier)
		{
			byte[] array = ((encodedObject as Asn1Sequence)[0] as Asn1Identifier).Asn1Encode();
			m_stream.Write(array, 0, array.Length);
		}
		if ((encodedObject as Asn1Sequence)[1] is Asn1Tag)
		{
			m_stream.WriteByte((byte)(0xA0u | (uint)(encodedObject as Asn1Tag).TagNumber));
			m_stream.WriteByte(128);
			Asn1Sequence sequence = Asn1Sequence.GetSequence(new List<Asn1> { (encodedObject as Asn1Tag).GetObject() });
			byte[] array = asn1DerStream.ParseTimeStampToken(sequence);
			m_stream.Write(array, 0, array.Length);
		}
		m_stream.WriteByte(0);
		m_stream.WriteByte(0);
		m_stream.WriteByte(0);
		m_stream.WriteByte(0);
		m_stream.Dispose();
		return m_stream.ToArray();
	}

	internal byte[] ParseTimeStampToken(Asn1 encodedObject)
	{
		Asn1Sequence asn1Sequence = null;
		if (encodedObject is Asn1Sequence)
		{
			asn1Sequence = encodedObject as Asn1Sequence;
		}
		else if (encodedObject is Asn1Set)
		{
			asn1Sequence = Asn1Sequence.GetSequence(encodedObject);
		}
		foreach (Asn1 item in asn1Sequence)
		{
			byte[] array = null;
			if (item is Asn1Integer)
			{
				array = (item as Asn1Integer).AsnEncode();
			}
			else if (item is Asn1Boolean)
			{
				array = (item as Asn1Boolean).AsnEncode();
			}
			else if (item is Asn1Null)
			{
				array = (item as Asn1Null).AsnEncode();
			}
			else if (item is Asn1Identifier)
			{
				array = (item as Asn1Identifier).Asn1Encode();
			}
			else if (item is Asn1Tag)
			{
				if ((item as Asn1Tag).GetObject() is Asn1Sequence)
				{
					Asn1Sequence sequence = Asn1Sequence.GetSequence(new List<Asn1> { (item as Asn1Tag).GetObject() });
					array = new Asn1DerStream().ParseTimeStampToken(sequence);
				}
				else if ((item as Asn1Tag).GetObject() is Asn1Tag)
				{
					array = new Asn1DerStream().ParseTimeStampToken((item as Asn1Tag).GetObject());
				}
				else if ((item as Asn1Tag).GetObject() is Asn1Octet)
				{
					new Asn1DerStream();
					array = ((item as Asn1Tag).GetObject() as Asn1Octet).AsnEncode();
				}
				else if ((item as Asn1Tag).GetObject() is Asn1Integer)
				{
					new Asn1DerStream();
					array = ((item as Asn1Tag).GetObject() as Asn1Integer).AsnEncode();
				}
				if ((item as Asn1Tag).IsExplicit)
				{
					m_stream.WriteByte((byte)(0xA0u | (uint)(item as Asn1Tag).TagNumber));
					Write(array);
				}
				else
				{
					array[0] &= 32;
					array[0] |= (byte)(0x80 | (item as Asn1Tag).TagNumber);
				}
			}
			else if (item is Asn1Set)
			{
				array = new Asn1DerStream().ParseTimeStampToken(item);
				m_stream.WriteByte(49);
				Write(array);
			}
			else if (item is Asn1Sequence)
			{
				Asn1DerStream asn1DerStream = new Asn1DerStream();
				asn1DerStream.ParseTimeStampToken(item);
				array = asn1DerStream.m_stream.ToArray();
				m_stream.WriteByte(48);
				Write(array);
			}
			else if (item is Asn1Octet)
			{
				array = (item as Asn1Octet).AsnEncode();
			}
			else if (item is DerUtcTime)
			{
				array = (item as DerUtcTime).GetDerEncoded();
			}
			else if (item is DerPrintableString)
			{
				array = (item as DerPrintableString).Asn1Encode();
			}
			else if (item is DerAsciiString)
			{
				array = (item as DerAsciiString).AsnEncode();
			}
			m_stream.Write(array, 0, array.Length);
		}
		m_stream.Dispose();
		return m_stream.ToArray();
	}

	private void Write(byte[] buffer)
	{
		if (buffer.Length > 127)
		{
			int num = 1;
			uint num2 = (uint)buffer.Length;
			while ((num2 >>= 8) != 0)
			{
				num++;
			}
			m_stream.WriteByte((byte)((uint)num | 0x80u));
			for (int num3 = (num - 1) * 8; num3 >= 0; num3 -= 8)
			{
				m_stream.WriteByte((byte)(buffer.Length >> num3));
			}
		}
		else
		{
			m_stream.WriteByte((byte)buffer.Length);
		}
	}
}
