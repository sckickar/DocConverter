using System;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class CompObjectStream
{
	private class CompObjHeader
	{
		internal const int DEF_STRUCT_LEN = 28;

		internal const int DEF_RESERVED2_ARR_LEN = 20;

		internal int m_reserved1;

		internal int m_version;

		internal byte[] m_reserved2;

		internal int Length => 28;

		internal CompObjHeader()
		{
			m_reserved1 = -131071;
			m_version = 2563;
			m_reserved2 = new byte[20];
		}

		internal void Parse(byte[] arrData, int iOffset)
		{
			m_reserved1 = ByteConverter.ReadInt32(arrData, ref iOffset);
			m_version = ByteConverter.ReadInt32(arrData, ref iOffset);
			if (arrData.Length > iOffset)
			{
				m_reserved2 = ByteConverter.ReadBytes(arrData, 20, ref iOffset);
			}
		}

		internal int Save(byte[] arrData, int iOffset)
		{
			ByteConverter.WriteInt32(arrData, ref iOffset, -131071);
			ByteConverter.WriteInt32(arrData, ref iOffset, 2563);
			m_reserved2 = new byte[20]
			{
				255, 255, 255, 255, 101, 202, 1, 184, 252, 161,
				208, 17, 133, 173, 68, 69, 83, 84, 0, 0
			};
			ByteConverter.WriteBytes(arrData, ref iOffset, m_reserved2);
			return iOffset;
		}

		internal void SaveTo(Stream stream)
		{
			int count = 4;
			byte[] bytes = BitConverter.GetBytes(m_reserved1);
			stream.Write(bytes, 0, count);
			bytes = BitConverter.GetBytes(m_version);
			stream.Write(bytes, 0, count);
			if (m_reserved2 == null)
			{
				stream.Write(new byte[20], 0, 20);
			}
			else
			{
				stream.Write(m_reserved2, 0, 20);
			}
		}
	}

	private const int DEF_STREAM_SIZE = 93;

	private const int DEF_MARKER_OR_LENGTH4 = 400;

	private const int DEF_MARKER_OR_LENGTH5 = 40;

	private const uint DEF_UNICODE_MARKER = 1907505652u;

	private int m_streamLength;

	private CompObjHeader m_header;

	private string m_ansiUserTypeData = string.Empty;

	private string m_ansiClipboardFormatData = string.Empty;

	private string m_reserved1Data = string.Empty;

	private uint m_unicodeMarker = 1907505652u;

	private string m_unicodeUserTypeData = string.Empty;

	private string m_unicodeClipboardFormatData = string.Empty;

	private string m_reserved2Data = string.Empty;

	internal int Length
	{
		get
		{
			if (m_streamLength == 0)
			{
				m_streamLength = 93;
			}
			return m_streamLength;
		}
	}

	internal string ObjectType => m_ansiUserTypeData;

	internal string ObjectTypeReserved => m_reserved1Data;

	internal CompObjectStream(Stream stream)
	{
		Parse((stream as MemoryStream).ToArray(), 0);
	}

	internal CompObjectStream(OleObjectType oleType)
	{
		m_header = new CompObjHeader();
		switch (oleType)
		{
		case OleObjectType.AdobeAcrobatDocument:
			m_ansiUserTypeData = "Acrobat Document\0";
			m_reserved1Data = "AcroExch.Document.7\0";
			break;
		case OleObjectType.WaveSound:
			m_ansiUserTypeData = "Wave Sound\0";
			m_reserved1Data = "SoundRec\0";
			break;
		case OleObjectType.BitmapImage:
		case OleObjectType.MediaClip:
		case OleObjectType.Equation:
		case OleObjectType.GraphChart:
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelChart:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPoint_97_2003_Slide:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.Word_97_2003_Document:
		case OleObjectType.WordDocument:
		case OleObjectType.WordMacroDocument:
		case OleObjectType.MIDISequence:
		case OleObjectType.Package:
		case OleObjectType.VideoClip:
			m_ansiUserTypeData = OleTypeConvertor.ToString(oleType, isWord2003: true) + "\0";
			break;
		case OleObjectType.VisioDrawing:
		case OleObjectType.OpenDocumentPresentation:
		case OleObjectType.OpenDocumentSpreadsheet:
		case OleObjectType.OpenDocumentText:
		case OleObjectType.OpenOfficeSpreadsheet1_1:
		case OleObjectType.OpenOfficeText_1_1:
			break;
		}
	}

	internal void Parse(byte[] arrData, int iOffset)
	{
		m_streamLength = arrData.Length;
		int num = 0;
		Encoding encoding = new UTF8Encoding();
		UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
		m_header = new CompObjHeader();
		m_header.Parse(arrData, iOffset);
		iOffset += m_header.Length;
		if (arrData.Length > iOffset + 4)
		{
			num = ByteConverter.ReadInt32(arrData, ref iOffset);
		}
		if (num > 0)
		{
			byte[] array = ByteConverter.ReadBytes(arrData, num, ref iOffset);
			m_ansiUserTypeData = encoding.GetString(array, 0, array.Length);
		}
		uint num2 = 0u;
		if (arrData.Length > iOffset + 4)
		{
			num2 = ByteConverter.ReadUInt32(arrData, ref iOffset);
		}
		if (num > 0)
		{
			if (num2 == uint.MaxValue || num2 == 4294967294u)
			{
				byte[] array2 = ByteConverter.ReadBytes(arrData, 4, ref iOffset);
				m_ansiUserTypeData = encoding.GetString(array2, 0, array2.Length);
			}
			else if (num2 > 400)
			{
				throw new Exception("OLE stream is not valid");
			}
		}
		if (arrData.Length > iOffset + 4)
		{
			num = ByteConverter.ReadInt32(arrData, ref iOffset);
		}
		if (num > 0 && num <= 40)
		{
			byte[] array3 = ByteConverter.ReadBytes(arrData, num, ref iOffset);
			m_reserved1Data = encoding.GetString(array3, 0, array3.Length);
		}
		if (arrData.Length > iOffset + 4)
		{
			m_unicodeMarker = ByteConverter.ReadUInt32(arrData, ref iOffset);
		}
		if (m_unicodeMarker != 1907505652)
		{
			return;
		}
		if (arrData.Length > iOffset + 4)
		{
			num = ByteConverter.ReadInt32(arrData, ref iOffset);
		}
		if (num > 0)
		{
			byte[] array4 = ByteConverter.ReadBytes(arrData, num, ref iOffset);
			m_unicodeUserTypeData = unicodeEncoding.GetString(array4, 0, array4.Length);
		}
		if (arrData.Length > iOffset + 4)
		{
			num2 = ByteConverter.ReadUInt32(arrData, ref iOffset);
		}
		if (num2 != 0)
		{
			if (num2 == uint.MaxValue || num2 == 4294967294u)
			{
				byte[] array5 = ByteConverter.ReadBytes(arrData, 4, ref iOffset);
				m_unicodeClipboardFormatData = unicodeEncoding.GetString(array5, 0, array5.Length);
			}
			else if (num2 > 400)
			{
				throw new Exception("OLE stream is not valid");
			}
		}
		if (arrData.Length > iOffset + 4)
		{
			num = ByteConverter.ReadInt32(arrData, ref iOffset);
		}
		if (num > 0 && num <= 40)
		{
			byte[] array6 = ByteConverter.ReadBytes(arrData, num, ref iOffset);
			m_reserved2Data = unicodeEncoding.GetString(array6, 0, array6.Length);
		}
	}

	internal int Save(byte[] arrData, int iOffset)
	{
		throw new NotImplementedException("Not implemented");
	}

	internal void SaveTo(Stream stream)
	{
		int byteLength = 4;
		m_header.SaveTo(stream);
		WriteLengthPrefixedString(stream, m_ansiUserTypeData);
		WriteLengthPrefixedString(stream, m_ansiClipboardFormatData);
		WriteLengthPrefixedString(stream, m_reserved1Data);
		WriteZeroByteArr(stream, byteLength);
		WriteZeroByteArr(stream, byteLength);
		WriteZeroByteArr(stream, byteLength);
		WriteZeroByteArr(stream, byteLength);
	}

	private void WriteZeroByteArr(Stream stream, int byteLength)
	{
		byte[] buffer = new byte[byteLength];
		stream.Write(buffer, 0, byteLength);
	}

	private void WriteLengthPrefixedString(Stream stream, string data)
	{
		byte[] array = new byte[4];
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		int iOffset = 0;
		byte[] bytes = uTF8Encoding.GetBytes(data);
		ByteConverter.WriteInt32(array, ref iOffset, bytes.Length);
		stream.Write(array, 0, array.Length);
		if (bytes.Length != 0)
		{
			stream.Write(bytes, 0, bytes.Length);
		}
	}
}
