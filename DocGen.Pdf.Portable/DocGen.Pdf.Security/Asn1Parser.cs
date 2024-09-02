using System;
using System.IO;

namespace DocGen.Pdf.Security;

internal class Asn1Parser
{
	private Stream m_stream;

	private int m_limit;

	private byte[][] m_buffers;

	internal Asn1Parser(Stream stream)
		: this(stream, Asn1Stream.GetLimit(stream))
	{
	}

	public Asn1Parser(Stream stream, int limit)
	{
		if (!stream.CanRead)
		{
			throw new ArgumentException("Invalid stream");
		}
		m_stream = stream;
		m_limit = limit;
		m_buffers = new byte[16][];
	}

	internal Asn1Parser(byte[] encoding)
		: this(new MemoryStream(encoding, writable: false), encoding.Length)
	{
	}

	internal IAsn1 ReadIndefinite(int tagValue)
	{
		return tagValue switch
		{
			4 => new BerOctetHelper(this), 
			16 => new BerSequenceHelper(this), 
			_ => throw new Exception("Invalid entry in sequence"), 
		};
	}

	internal IAsn1 ReadImplicit(bool constructed, int tagNumber)
	{
		if (m_stream is Asn1LengthStream)
		{
			if (!constructed)
			{
				throw new IOException("Invalid length specified");
			}
			return ReadIndefinite(tagNumber);
		}
		if (constructed)
		{
			switch (tagNumber)
			{
			case 17:
				return new DerSetHelper(this);
			case 16:
				return new DerSequenceHelper(this);
			case 4:
				return new BerOctetHelper(this);
			}
		}
		else
		{
			switch (tagNumber)
			{
			case 17:
				throw new Exception("Constructed encoding is not used in the set");
			case 16:
				throw new Exception("Constructed encoding is not used in the sequence");
			case 4:
				return new DerOctetHelper((Asn1StreamHelper)m_stream);
			}
		}
		throw new Exception("Implicit tagging is not supported");
	}

	internal Asn1 ReadTaggedObject(bool constructed, int tagNumber)
	{
		if (!constructed)
		{
			Asn1StreamHelper asn1StreamHelper = (Asn1StreamHelper)m_stream;
			return new DerTag(isExplicit: false, tagNumber, new DerOctet(asn1StreamHelper.ToArray()));
		}
		Asn1EncodeCollection asn1EncodeCollection = ReadCollection();
		if (m_stream is Asn1LengthStream)
		{
			if (asn1EncodeCollection.Count != 1)
			{
				return new BerTag(IsExplicit: false, tagNumber, BerSequence.FromCollection(asn1EncodeCollection));
			}
			return new BerTag(IsExplicit: true, tagNumber, asn1EncodeCollection[0]);
		}
		if (asn1EncodeCollection.Count != 1)
		{
			return new DerTag(isExplicit: false, tagNumber, DerSequence.FromCollection(asn1EncodeCollection));
		}
		return new DerTag(isExplicit: true, tagNumber, asn1EncodeCollection[0]);
	}

	public virtual IAsn1 ReadObject()
	{
		int num = m_stream.ReadByte();
		if (num == -1)
		{
			return null;
		}
		SetEndOfFile(enabled: false);
		int num2 = Asn1Stream.ReadTagNumber(m_stream, num);
		bool flag = (num & 0x20) != 0;
		int length = Asn1Stream.GetLength(m_stream, m_limit);
		if (length < 0)
		{
			if (!flag)
			{
				throw new IOException("Invalid length specified");
			}
			Asn1Parser asn1Parser = new Asn1Parser(new Asn1LengthStream(m_stream, m_limit), m_limit);
			if (((uint)num & 0x80u) != 0)
			{
				return new BerTagHelper(isConstructed: true, num2, asn1Parser);
			}
			return asn1Parser.ReadIndefinite(num2);
		}
		Asn1StreamHelper stream = new Asn1StreamHelper(m_stream, length);
		if (((uint)num & 0x80u) != 0)
		{
			return new BerTagHelper(flag, num2, new Asn1Parser(stream));
		}
		if (flag)
		{
			return num2 switch
			{
				4 => new BerOctetHelper(new Asn1Parser(stream)), 
				16 => new DerSequenceHelper(new Asn1Parser(stream)), 
				17 => new DerSetHelper(new Asn1Parser(stream)), 
				_ => null, 
			};
		}
		if (num2 == 4)
		{
			return new DerOctetHelper(stream);
		}
		try
		{
			return Asn1Stream.GetPrimitiveObject(num2, stream, m_buffers);
		}
		catch (ArgumentException innerException)
		{
			throw new Exception("Invalid or corrupted stream", innerException);
		}
	}

	private void SetEndOfFile(bool enabled)
	{
		if (m_stream is Asn1LengthStream)
		{
			((Asn1LengthStream)m_stream).SetEndOfFileOnStart(enabled);
		}
	}

	internal Asn1EncodeCollection ReadCollection()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		IAsn1 asn;
		while ((asn = ReadObject()) != null)
		{
			asn1EncodeCollection.Add(asn.GetAsn1());
		}
		return asn1EncodeCollection;
	}
}
