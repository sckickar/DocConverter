using System;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class LinkInfoStream
{
	private const int DEF_STRUCT_SIZE = 0;

	private const int DEF_UNICODE_MARKER = -858997829;

	private const int DEF_UNICODE_MARKER_SIZE = 4;

	private byte[] m_filePathDataASCII;

	private byte[] m_filePathDataUNICOD;

	private string m_filePath;

	internal int Length
	{
		get
		{
			if (m_filePathDataASCII != null && m_filePathDataUNICOD != null)
			{
				return m_filePathDataASCII.Length + m_filePathDataUNICOD.Length + 4 + 8;
			}
			return 0;
		}
	}

	internal LinkInfoStream(Stream stream)
	{
		Parse((stream as MemoryStream).ToArray(), 0);
	}

	internal LinkInfoStream(string filePath)
	{
		m_filePath = filePath;
		Encoding encoding = new UTF8Encoding();
		UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
		m_filePathDataASCII = encoding.GetBytes(m_filePath);
		m_filePathDataUNICOD = unicodeEncoding.GetBytes(m_filePath);
	}

	internal void Parse(byte[] arrData, int iOffset)
	{
		int iOffset2 = 0;
		int i = 0;
		for (int num = arrData.Length; i < num; i++)
		{
			if (iOffset2 > 0)
			{
				iOffset2 -= 3;
			}
			if (ByteConverter.ReadInt32(arrData, ref iOffset2) == -858997829)
			{
				break;
			}
		}
		byte[] array = new byte[iOffset2 - 4];
		int iOffset3 = 0;
		m_filePathDataASCII = ByteConverter.ReadBytes(arrData, iOffset2 - 4, ref iOffset3);
		iOffset3 += 4;
		int num2 = arrData.Length - array.Length - 4;
		_ = new byte[num2];
		m_filePathDataUNICOD = ByteConverter.ReadBytes(arrData, num2, ref iOffset3);
	}

	internal int Save(byte[] arrData, int iOffset)
	{
		throw new NotImplementedException("Not implemented");
	}

	internal void SaveTo(Stream stream)
	{
		byte[] array = new byte[Length];
		int num = 0;
		array[num] = (byte)m_filePathDataASCII.Length;
		num += 2;
		ByteConverter.WriteBytes(array, ref num, m_filePathDataASCII);
		num += 2;
		ByteConverter.WriteBytes(array, ref num, BitConverter.GetBytes(-858997829));
		array[num] = (byte)m_filePathDataASCII.Length;
		num += 2;
		ByteConverter.WriteBytes(array, ref num, m_filePathDataUNICOD);
		stream.Write(array, 0, array.Length);
	}
}
