using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.Office;

internal class TrueTypeFontStream
{
	private const string Prefix = "stream";

	private const string Suffix = "endstream";

	private MemoryStream m_dataStream;

	private bool m_blockEncryption;

	private bool m_bDecrypted;

	private bool m_bCompress;

	private bool m_bEncrypted;

	private TrueTypeFontStream m_clonedObject;

	internal bool isCustomQuality;

	internal bool isImageDualFilter;

	internal MemoryStream InternalStream
	{
		get
		{
			return m_dataStream;
		}
		set
		{
			m_dataStream = value;
		}
	}

	internal byte[] Data
	{
		get
		{
			return m_dataStream.ToArray();
		}
		set
		{
			m_dataStream.SetLength(0L);
			m_dataStream.Write(value, 0, value.Length);
		}
	}

	public bool WasEncrypted
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool Decrypted => m_bDecrypted;

	internal TrueTypeFontStream()
	{
		m_dataStream = new MemoryStream(100);
		m_bCompress = true;
	}

	public static byte[] StreamToBytes(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return StreamToBytes(stream, writeWholeStream: false);
	}

	public static byte[] StreamToBytes(Stream stream, bool writeWholeStream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		long num = ((stream.Position != 0L) ? stream.Position : stream.Length);
		num = (writeWholeStream ? stream.Length : num);
		byte[] array = new byte[num];
		stream.Position = 0L;
		stream.Read(array, 0, (int)num);
		stream.Position = position;
		return array;
	}

	public static byte[] StreamToBigEndian(Stream stream)
	{
		byte[] bytes = StreamToBytes(stream);
		return Encoding.Convert(Encoding.Unicode, Encoding.BigEndianUnicode, bytes);
	}

	internal void Write(char symbol)
	{
		Write(symbol.ToString());
	}

	internal void Write(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length <= 0)
		{
			throw new ArgumentException("Can't write an empty string.", "text");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		Write(bytes);
	}

	internal void Write(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			throw new ArgumentException("Can't write an empty array.", "data");
		}
		m_dataStream.Write(data, 0, data.Length);
	}

	internal void BlockEncryption()
	{
		m_blockEncryption = true;
	}

	private byte HexToDecimalConversion(char hex)
	{
		if (hex >= '0' && hex <= '9')
		{
			return (byte)(hex - 48);
		}
		if (hex >= 'a' && hex <= 'f')
		{
			return (byte)(hex - 97 + 10);
		}
		return (byte)(hex - 65 + 10);
	}

	private byte[] Decode(byte[] inputData)
	{
		inputData = RemoveWhiteSpace(inputData);
		List<byte> list = new List<byte>(inputData.Length);
		int num = 0;
		while (num < inputData.Length)
		{
			int num2 = num + 1;
			byte hex;
			if ((hex = inputData[num]) == 62)
			{
				break;
			}
			byte b = inputData[num2];
			num = num2 + 1;
			if (b == 62)
			{
				b = 48;
			}
			byte item = (byte)((HexToDecimalConversion((char)hex) << 4) | HexToDecimalConversion((char)b));
			list.Add(item);
		}
		byte[] result = list.ToArray();
		list.Clear();
		list = null;
		return result;
	}

	private byte[] RemoveWhiteSpace(byte[] data)
	{
		List<byte> list = new List<byte>(data.Length);
		for (int i = 0; i < data.Length; i++)
		{
			if (!char.IsWhiteSpace((char)data[i]))
			{
				list.Add(data[i]);
			}
		}
		byte[] result = list.ToArray();
		list.Clear();
		list = null;
		return result;
	}

	internal void Clear()
	{
		if (InternalStream != null && InternalStream.CanWrite)
		{
			InternalStream.SetLength(0L);
			InternalStream.Position = 0L;
		}
		m_bCompress = true;
	}

	internal void Dispose()
	{
		if (InternalStream != null)
		{
			InternalStream.Dispose();
			InternalStream = null;
			m_bCompress = true;
		}
	}
}
