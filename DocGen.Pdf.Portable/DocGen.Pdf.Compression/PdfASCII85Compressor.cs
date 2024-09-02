using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Compression;

internal class PdfASCII85Compressor : IPdfCompressor
{
	private const int m_asciiOffset = 33;

	private byte[] m_encodedBlock = new byte[5];

	private byte[] m_decodedBlock = new byte[4];

	private uint m_tuple;

	private uint[] m_codeTable;

	public CompressionType Type => CompressionType.ASCII85;

	public string Name => null;

	public PdfASCII85Compressor()
	{
		m_codeTable = new uint[5] { 52200625u, 614125u, 7225u, 85u, 1u };
	}

	public void Decompress(byte[] inputData, Stream outputData)
	{
		int num = 0;
		bool flag = false;
		for (int i = 0; i < inputData.Length; i++)
		{
			char c = Convert.ToChar(inputData[i]);
			switch (c)
			{
			case 'z':
				if (num != 0)
				{
					throw new PdfException("The character 'z' is invalid inside an ASCII85 block.");
				}
				m_decodedBlock[0] = 0;
				m_decodedBlock[1] = 0;
				m_decodedBlock[2] = 0;
				m_decodedBlock[3] = 0;
				outputData.Write(m_decodedBlock, 0, m_decodedBlock.Length);
				flag = false;
				break;
			case '\0':
			case '\b':
			case '\t':
			case '\n':
			case '\f':
			case '\r':
				flag = false;
				break;
			default:
				flag = true;
				break;
			}
			if (flag)
			{
				m_tuple += (uint)((c - 33) * (int)m_codeTable[num]);
				num++;
				if (num == m_encodedBlock.Length)
				{
					DecodeBlock();
					outputData.Write(m_decodedBlock, 0, m_decodedBlock.Length);
					m_tuple = 0u;
					num = 0;
				}
			}
		}
		if (num != 0)
		{
			num--;
			m_tuple += m_codeTable[num];
			DecodeBlock(num);
			for (int j = 0; j < num; j++)
			{
				outputData.WriteByte(m_decodedBlock[j]);
			}
		}
	}

	public byte[] Compress(byte[] data)
	{
		return null;
	}

	public byte[] Compress(string data)
	{
		return null;
	}

	public Stream Compress(Stream inputStream)
	{
		return null;
	}

	public byte[] Decompress(string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		MemoryStream memoryStream = new MemoryStream();
		Decompress(bytes, memoryStream);
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Position = 0L;
		memoryStream.Read(array, 0, (int)memoryStream.Length - 1);
		return array;
	}

	public byte[] Decompress(byte[] value)
	{
		MemoryStream memoryStream = new MemoryStream();
		Decompress(value, memoryStream);
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Position = 0L;
		memoryStream.Read(array, 0, (int)memoryStream.Length - 1);
		return array;
	}

	public Stream Decompress(Stream inputStream)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[inputStream.Length];
		inputStream.Position = 0L;
		inputStream.Read(array, 0, (int)inputStream.Length - 1);
		Decompress(array, memoryStream);
		return memoryStream;
	}

	private void DecodeBlock()
	{
		DecodeBlock(m_decodedBlock.Length);
	}

	private void DecodeBlock(int bytes)
	{
		for (int i = 0; i < bytes; i++)
		{
			m_decodedBlock[i] = (byte)(m_tuple >> 24 - i * 8);
		}
	}
}
