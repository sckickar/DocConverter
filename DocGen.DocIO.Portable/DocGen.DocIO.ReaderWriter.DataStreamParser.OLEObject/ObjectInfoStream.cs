using System;
using System.IO;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class ObjectInfoStream
{
	private const int DEF_STRUCT_SIZE = 6;

	private byte[] m_dataBytes;

	internal int Length => 6;

	internal ObjectInfoStream(Stream stream)
	{
		Parse((stream as MemoryStream).ToArray(), 0);
	}

	internal ObjectInfoStream()
	{
	}

	internal void Parse(byte[] arrData, int iOffset)
	{
		m_dataBytes = arrData;
	}

	internal int Save(byte[] arrData, int iOffset)
	{
		throw new NotImplementedException("Not implemented");
	}

	internal void SaveTo(Stream stream, OleLinkType linkType, OleObjectType oleType)
	{
		switch (oleType)
		{
		case OleObjectType.PowerPoint_97_2003_Slide:
		case OleObjectType.WordDocument:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 0, 0, 3, 0, 1, 0 };
			}
			else
			{
				m_dataBytes = new byte[6] { 16, 0, 3, 0, 13, 0 };
			}
			break;
		case OleObjectType.Equation:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 0, 0, 3, 0, 4, 0 };
			}
			break;
		case OleObjectType.GraphChart:
		case OleObjectType.ExcelChart:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 0, 2, 3, 0, 13, 0 };
			}
			else
			{
				m_dataBytes = new byte[6] { 16, 0, 3, 0, 13, 0 };
			}
			break;
		case OleObjectType.AdobeAcrobatDocument:
		case OleObjectType.Excel_97_2003_Worksheet:
		case OleObjectType.ExcelBinaryWorksheet:
		case OleObjectType.ExcelMacroWorksheet:
		case OleObjectType.ExcelWorksheet:
		case OleObjectType.PowerPoint_97_2003_Presentation:
		case OleObjectType.PowerPointMacroPresentation:
		case OleObjectType.PowerPointMacroSlide:
		case OleObjectType.PowerPointPresentation:
		case OleObjectType.PowerPointSlide:
		case OleObjectType.WordMacroDocument:
		case OleObjectType.VisioDrawing:
		case OleObjectType.OpenDocumentPresentation:
		case OleObjectType.OpenDocumentSpreadsheet:
		case OleObjectType.OpenOfficeSpreadsheet1_1:
		case OleObjectType.OpenOfficeText_1_1:
		case OleObjectType.OpenOfficeSpreadsheet:
		case OleObjectType.OpenOfficeText:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 64, 0, 3, 0, 4, 0 };
			}
			else
			{
				m_dataBytes = new byte[6] { 16, 0, 3, 0, 13, 0 };
			}
			break;
		case OleObjectType.BitmapImage:
		case OleObjectType.MIDISequence:
		case OleObjectType.VideoClip:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 0, 0, 3, 0, 4, 0 };
			}
			else
			{
				m_dataBytes = new byte[6] { 16, 0, 3, 0, 4, 0 };
			}
			break;
		case OleObjectType.MediaClip:
		case OleObjectType.Package:
		case OleObjectType.WaveSound:
			m_dataBytes = new byte[6] { 64, 0, 3, 0, 4, 0 };
			break;
		case OleObjectType.Undefined:
		case OleObjectType.WordPadDocument:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 0, 0, 3, 0, 4, 0 };
			}
			else
			{
				m_dataBytes = new byte[6] { 16, 2, 3, 0, 13, 0 };
			}
			break;
		case OleObjectType.Word_97_2003_Document:
			if (linkType == OleLinkType.Embed)
			{
				m_dataBytes = new byte[6] { 0, 2, 3, 0, 1, 0 };
			}
			else
			{
				m_dataBytes = new byte[6] { 16, 2, 3, 0, 13, 0 };
			}
			break;
		}
		stream.Write(m_dataBytes, 0, m_dataBytes.Length);
	}
}
