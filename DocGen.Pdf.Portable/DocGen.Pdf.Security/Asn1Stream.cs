using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1Stream
{
	private int m_limit;

	private byte[][] m_buffers;

	private Stream m_stream;

	internal Asn1Stream(Stream stream)
		: this(stream, GetLimit(stream))
	{
		m_stream = stream;
	}

	public Asn1Stream(Stream stream, int limit)
	{
		m_stream = stream;
		m_limit = limit;
		m_buffers = new byte[16][];
	}

	public Asn1Stream(byte[] input)
		: this(new MemoryStream(input, writable: false), input.Length)
	{
	}

	private Asn1 BuildObject(int tag, int tagNumber, int length)
	{
		bool flag = (tag & 0x20) != 0;
		Asn1StreamHelper stream = new Asn1StreamHelper(m_stream, length);
		if (((uint)tag & 0x80u) != 0)
		{
			return new Asn1Parser(stream).ReadTaggedObject(flag, tagNumber);
		}
		if (flag)
		{
			switch (tagNumber)
			{
			case 4:
				return new BerOctet(GetDerEncodableCollection(stream));
			case 16:
				return CreateDerSequence(stream);
			case 17:
				return CreateDerSet(stream);
			}
		}
		return GetPrimitiveObject(tagNumber, stream, m_buffers);
	}

	internal Asn1EncodeCollection GetEncodableCollection()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		Asn1 asn;
		while ((asn = ReadAsn1()) != null)
		{
			asn1EncodeCollection.Add(asn);
		}
		return asn1EncodeCollection;
	}

	internal virtual Asn1EncodeCollection GetDerEncodableCollection(Asn1StreamHelper stream)
	{
		return new Asn1Stream(stream).GetEncodableCollection();
	}

	internal virtual DerSequence CreateDerSequence(Asn1StreamHelper stream)
	{
		return DerSequence.FromCollection(GetDerEncodableCollection(stream));
	}

	internal virtual DerSet CreateDerSet(Asn1StreamHelper stream)
	{
		return DerSet.FromCollection(GetDerEncodableCollection(stream), isSort: false);
	}

	internal Asn1 ReadAsn1()
	{
		int num = m_stream.ReadByte();
		if (num <= 0)
		{
			if (num == 0)
			{
				throw new IOException("End of contents is invalid");
			}
			return null;
		}
		int num2 = ReadTagNumber(m_stream, num);
		bool flag = (num & 0x20) != 0;
		int length = GetLength(m_stream, m_limit);
		if (length < 0)
		{
			if (!flag)
			{
				throw new IOException("Encodeing length is invalid");
			}
			Asn1Parser helper = new Asn1Parser(new Asn1LengthStream(m_stream, m_limit), m_limit);
			if (((uint)num & 0x80u) != 0)
			{
				return new BerTagHelper(isConstructed: true, num2, helper).GetAsn1();
			}
			return num2 switch
			{
				4 => new BerOctetHelper(helper).GetAsn1(), 
				16 => new BerSequenceHelper(helper).GetAsn1(), 
				_ => throw new IOException("Invalid object in the sequence"), 
			};
		}
		try
		{
			return BuildObject(num, num2, length);
		}
		catch (ArgumentException ex)
		{
			throw new Exception(ex.Message);
		}
	}

	internal static int GetLimit(Stream input)
	{
		if (input is BaseStream)
		{
			return ((BaseStream)input).Remaining;
		}
		if (input is MemoryStream)
		{
			MemoryStream memoryStream = (MemoryStream)input;
			return (int)(memoryStream.Length - memoryStream.Position);
		}
		return int.MaxValue;
	}

	internal static int ReadTagNumber(Stream stream, int tagNumber)
	{
		int num = tagNumber & 0x1F;
		if (num == 31)
		{
			num = 0;
			int num2 = stream.ReadByte();
			if ((num2 & 0x7F) == 0)
			{
				throw new IOException("Invalid tag number specified");
			}
			while (num2 >= 0 && ((uint)num2 & 0x80u) != 0)
			{
				num |= num2 & 0x7F;
				num <<= 7;
				num2 = stream.ReadByte();
			}
			if (num2 < 0)
			{
				throw new EndOfStreamException("End of file detected");
			}
			num |= num2 & 0x7F;
		}
		return num;
	}

	internal static int GetLength(Stream stream, int limit)
	{
		int num = stream.ReadByte();
		if (num < 0)
		{
			throw new EndOfStreamException("End of file detected");
		}
		if (num == 128)
		{
			return -1;
		}
		if (num > 127)
		{
			int num2 = num & 0x7F;
			if (num2 > 4)
			{
				throw new IOException("Invalid length detected " + num2);
			}
			num = 0;
			for (int i = 0; i < num2; i++)
			{
				int num3 = stream.ReadByte();
				if (num3 < 0)
				{
					throw new EndOfStreamException("End of file detected");
				}
				num = (num << 8) + num3;
			}
			if (num < 0)
			{
				throw new IOException("Invalid length or corrupted input stream detected");
			}
			if (num >= limit)
			{
				throw new IOException("Out of bound or corrupted stream detected");
			}
		}
		return num;
	}

	internal static byte[] GetBytes(Asn1StreamHelper stream, byte[][] buffers)
	{
		int remaining = stream.Remaining;
		if (remaining >= buffers.Length)
		{
			return stream.ToArray();
		}
		byte[] array = buffers[remaining];
		if (array == null)
		{
			array = (buffers[remaining] = new byte[remaining]);
		}
		stream.ReadAll(array);
		return array;
	}

	internal static Asn1 GetPrimitiveObject(int tagNumber, Asn1StreamHelper stream, byte[][] buffers)
	{
		switch (tagNumber)
		{
		case 1:
			return new DerBoolean(GetBytes(stream, buffers));
		case 10:
			return new DerCatalogue(GetBytes(stream, buffers));
		case 6:
			return DerObjectID.FromOctetString(GetBytes(stream, buffers));
		default:
		{
			byte[] bytes = stream.ToArray();
			return tagNumber switch
			{
				3 => DerBitString.FromAsn1Octets(bytes), 
				30 => new DerBmpString(bytes), 
				24 => new GeneralizedTime(bytes), 
				22 => new DerAsciiString(bytes), 
				2 => new DerInteger(bytes), 
				5 => DerNull.Value, 
				4 => new DerOctet(bytes), 
				19 => new DerPrintableString(bytes), 
				20 => new DerTeleText(bytes), 
				23 => new DerUtcTime(bytes), 
				12 => new DerUtf8String(bytes), 
				26 => new DerVisibleString(bytes), 
				_ => null, 
			};
		}
		}
	}
}
