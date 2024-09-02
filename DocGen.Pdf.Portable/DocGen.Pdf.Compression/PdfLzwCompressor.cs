using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Compression;

internal class PdfLzwCompressor : IPdfCompressor
{
	private const int c_eod = 257;

	private const int c_clearTable = 256;

	private const int c_startCode = 258;

	private const int c_10BitsCode = 511;

	private const int c_11BitsCode = 1023;

	private const int c_12BitsCode = 2047;

	private byte[][] m_codeTable;

	private byte[] m_inputData;

	private Stream m_outputData;

	private int m_tableIndex;

	private int m_bitsToGet;

	private int m_byteRead;

	private int m_nextData;

	private int m_nextBits;

	private int[] m_sizeTable;

	private bool m_isEarlyChanged = true;

	public CompressionType Type => CompressionType.LZW;

	public string Name => null;

	public PdfLzwCompressor()
	{
		m_sizeTable = new int[4] { 511, 1023, 2047, 4095 };
	}

	public void Decompress(byte[] inputData, Stream outputData)
	{
		if (outputData == null)
		{
			throw new ArgumentNullException("outputData");
		}
		if (inputData == null)
		{
			throw new ArgumentNullException("inputData");
		}
		InitializeDataTable();
		m_inputData = inputData;
		m_outputData = outputData;
		m_byteRead = 0;
		m_nextData = 0;
		m_nextBits = 0;
		m_bitsToGet = 9;
		int num = 0;
		int num2 = 0;
		while ((num = NewCode()) != 257)
		{
			byte[] array;
			switch (num)
			{
			case 256:
				InitializeDataTable();
				num = NewCode();
				if (num != 257 && num != -1)
				{
					array = m_codeTable[num];
					if (array != null)
					{
						WriteCode(m_codeTable[num]);
					}
					num2 = num;
					continue;
				}
				return;
			case -1:
				return;
			}
			if (num < m_tableIndex)
			{
				array = m_codeTable[num];
				if (array != null && m_codeTable[num2] != null)
				{
					WriteCode(array);
					AddCodeToTable(m_codeTable[num2], array[0]);
					num2 = num;
				}
				continue;
			}
			array = m_codeTable[num2];
			if (array != null)
			{
				array = UniteBytes(array, array[0]);
				WriteCode(array);
				AddCodeToTable(array);
				num2 = num;
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

	public byte[] Decompress(byte[] value, bool isearlychange)
	{
		m_isEarlyChanged = isearlychange;
		return Decompress(value);
	}

	public byte[] Decompress(byte[] value)
	{
		MemoryStream memoryStream = new MemoryStream();
		Decompress(value, memoryStream);
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Position = 0L;
		if (memoryStream.Length > 0)
		{
			memoryStream.Read(array, 0, (int)memoryStream.Length);
		}
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

	private void InitializeDataTable()
	{
		m_codeTable = new byte[8192][];
		for (int i = 0; i < 256; i++)
		{
			m_codeTable[i] = new byte[1];
			m_codeTable[i][0] = (byte)i;
		}
		m_tableIndex = 258;
		m_bitsToGet = 9;
	}

	private void WriteCode(byte[] code)
	{
		m_outputData.Write(code, 0, code.Length);
	}

	private void AddCodeToTable(byte[] oldBytes, byte newByte)
	{
		int num = oldBytes.Length;
		byte[] array = new byte[num + 1];
		Array.Copy(oldBytes, 0, array, 0, num);
		array[num] = newByte;
		AddCodeToTable(array);
	}

	private void AddCodeToTable(byte[] data)
	{
		if (m_isEarlyChanged)
		{
			m_codeTable[m_tableIndex++] = data;
		}
		if (m_tableIndex == 511)
		{
			m_bitsToGet = 10;
		}
		else if (m_tableIndex == 1023)
		{
			m_bitsToGet = 11;
		}
		else if (m_tableIndex == 2047)
		{
			m_bitsToGet = 12;
		}
		if (!m_isEarlyChanged && m_tableIndex <= m_codeTable.Length)
		{
			m_codeTable[m_tableIndex++] = data;
		}
	}

	private byte[] UniteBytes(byte[] oldData, byte newData)
	{
		int num = oldData.Length;
		byte[] array = new byte[num + 1];
		Array.Copy(oldData, 0, array, 0, num);
		array[num] = newData;
		return array;
	}

	private int NewCode()
	{
		if (m_byteRead < m_inputData.Length)
		{
			m_nextData = (m_nextData << 8) | m_inputData[m_byteRead++];
			m_nextBits += 8;
			if (m_nextBits < m_bitsToGet && m_byteRead < m_inputData.Length)
			{
				m_nextData = (m_nextData << 8) | m_inputData[m_byteRead++];
				m_nextBits += 8;
			}
			int result = (m_nextData >> m_nextBits - m_bitsToGet) & m_sizeTable[m_bitsToGet - 9];
			m_nextBits -= m_bitsToGet;
			return result;
		}
		return -1;
	}
}
