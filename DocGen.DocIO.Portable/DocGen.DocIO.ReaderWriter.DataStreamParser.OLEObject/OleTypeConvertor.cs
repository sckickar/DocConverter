using System;
using DocGen.DocIO.DLS;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;

internal class OleTypeConvertor
{
	internal static OleObjectType ToOleType(string oleTypeStr)
	{
		oleTypeStr = oleTypeStr.TrimEnd('\0');
		OleObjectType result = OleObjectType.Undefined;
		if (StartsWithExt(oleTypeStr, "Acrobat Document") || StartsWithExt(oleTypeStr, "AcroExch.Document.7") || StartsWithExt(oleTypeStr, "AcroExch.Document.11") || StartsWithExt(oleTypeStr, "AcroExch.Document.DC"))
		{
			result = OleObjectType.AdobeAcrobatDocument;
		}
		else if (StartsWithExt(oleTypeStr, "Package"))
		{
			result = OleObjectType.Package;
		}
		else if (StartsWithExt(oleTypeStr, "PBrush"))
		{
			result = OleObjectType.BitmapImage;
		}
		else if (StartsWithExt(oleTypeStr, "Media Clip") || StartsWithExt(oleTypeStr, "MPlayer"))
		{
			result = OleObjectType.MediaClip;
		}
		else if (StartsWithExt(oleTypeStr, "Microsoft Equation 3.0") || StartsWithExt(oleTypeStr, "Equation.3"))
		{
			result = OleObjectType.Equation;
		}
		else if (StartsWithExt(oleTypeStr, "Microsoft Graph Chart") || StartsWithExt(oleTypeStr, "MSGraph.Chart.8"))
		{
			result = OleObjectType.GraphChart;
		}
		else if (oleTypeStr.Contains("Excel 2003 Worksheet") || StartsWithExt(oleTypeStr, "Excel.Sheet.8"))
		{
			result = OleObjectType.Excel_97_2003_Worksheet;
		}
		else if (oleTypeStr.Contains("Excel Binary Worksheet") || StartsWithExt(oleTypeStr, "Excel.SheetBinaryMacroEnabled.12"))
		{
			result = OleObjectType.ExcelBinaryWorksheet;
		}
		else if (oleTypeStr.Contains("Excel Chart") || StartsWithExt(oleTypeStr, "Excel.Chart.8"))
		{
			result = OleObjectType.ExcelChart;
		}
		else if (oleTypeStr.Contains("Excel Worksheet (code)") || StartsWithExt(oleTypeStr, "Excel.SheetMacroEnabled.12"))
		{
			result = OleObjectType.ExcelMacroWorksheet;
		}
		else if (oleTypeStr.Contains("Excel Worksheet") || StartsWithExt(oleTypeStr, "Excel.Sheet.12"))
		{
			result = OleObjectType.ExcelWorksheet;
		}
		else if (oleTypeStr.Contains("PowerPoint 97-2003 Presentation") || StartsWithExt(oleTypeStr, "PowerPoint.Show.8"))
		{
			result = OleObjectType.PowerPoint_97_2003_Presentation;
		}
		else if (oleTypeStr.Contains("PowerPoint 97-2003 Slide") || StartsWithExt(oleTypeStr, "PowerPoint.Slide.8"))
		{
			result = OleObjectType.PowerPoint_97_2003_Slide;
		}
		else if (oleTypeStr.Contains("PowerPoint Macro-Enabled Presentation") || StartsWithExt(oleTypeStr, "PowerPoint.ShowMacroEnabled.12"))
		{
			result = OleObjectType.PowerPointMacroPresentation;
		}
		else if (oleTypeStr.Contains("PowerPoint Macro-Enabled Slide") || StartsWithExt(oleTypeStr, "PowerPoint.SlideMacroEnabled.12"))
		{
			result = OleObjectType.PowerPointMacroSlide;
		}
		else if (oleTypeStr.Contains("PowerPoint Presentation") || StartsWithExt(oleTypeStr, "PowerPoint.Show.12"))
		{
			result = OleObjectType.PowerPointPresentation;
		}
		else if (oleTypeStr.Contains("PowerPoint Slide") || StartsWithExt(oleTypeStr, "PowerPoint.Slide.12"))
		{
			result = OleObjectType.PowerPointSlide;
		}
		else if (oleTypeStr.Contains("Word 97-2003 Document") || StartsWithExt(oleTypeStr, "Word.Document.8"))
		{
			result = OleObjectType.Word_97_2003_Document;
		}
		else if (oleTypeStr.Contains("Word Document") || StartsWithExt(oleTypeStr, "Word.Document.12"))
		{
			result = OleObjectType.WordDocument;
		}
		else if (oleTypeStr.Contains("Word Macro-Enabled Document") || StartsWithExt(oleTypeStr, "Word.DocumentMacroEnabled.12"))
		{
			result = OleObjectType.WordMacroDocument;
		}
		else if (StartsWithExt(oleTypeStr, "Microsoft Visio Drawing") || StartsWithExt(oleTypeStr, "Visio.Drawing.11"))
		{
			result = OleObjectType.VisioDrawing;
		}
		else if (StartsWithExt(oleTypeStr, "OpenDocument Presentation") || StartsWithExt(oleTypeStr, "PowerPoint.OpenDocumentPresentation.12"))
		{
			result = OleObjectType.OpenDocumentPresentation;
		}
		else if (StartsWithExt(oleTypeStr, "OpenDocument Spreadsheet") || StartsWithExt(oleTypeStr, "Excel.OpenDocumentSpreadsheet.12"))
		{
			result = OleObjectType.OpenDocumentSpreadsheet;
		}
		else if (StartsWithExt(oleTypeStr, "opendocument.CalcDocument.1"))
		{
			result = OleObjectType.OpenOfficeSpreadsheet;
		}
		else if (StartsWithExt(oleTypeStr, "opendocument.WriterDocument.1"))
		{
			result = OleObjectType.OpenOfficeText;
		}
		else if (StartsWithExt(oleTypeStr, "soffice.StarCalcDocument.6"))
		{
			result = OleObjectType.OpenOfficeSpreadsheet1_1;
		}
		else if (StartsWithExt(oleTypeStr, "soffice.StarWriterDocument.6"))
		{
			result = OleObjectType.OpenOfficeText_1_1;
		}
		else if (StartsWithExt(oleTypeStr, "Video Clip") || StartsWithExt(oleTypeStr, "AVIFile"))
		{
			result = OleObjectType.VideoClip;
		}
		else if (StartsWithExt(oleTypeStr, "WaveSound") || StartsWithExt(oleTypeStr, "SoundRec"))
		{
			result = OleObjectType.WaveSound;
		}
		else if (StartsWithExt(oleTypeStr, "WordPad Document") || StartsWithExt(oleTypeStr, "WordPad.Document.1"))
		{
			result = OleObjectType.WordPadDocument;
		}
		return result;
	}

	internal static string ToString(OleObjectType oleType, bool isWord2003)
	{
		string result = string.Empty;
		switch (oleType)
		{
		case OleObjectType.AdobeAcrobatDocument:
			result = (isWord2003 ? "Acrobat Document" : "AcroExch.Document.7");
			break;
		case OleObjectType.Package:
			result = "Package";
			break;
		case OleObjectType.BitmapImage:
			result = "PBrush";
			break;
		case OleObjectType.MediaClip:
			result = (isWord2003 ? "Media Clip" : "MPlayer");
			break;
		case OleObjectType.Equation:
			result = (isWord2003 ? "Microsoft Equation 3.0" : "Equation.3");
			break;
		case OleObjectType.GraphChart:
			result = (isWord2003 ? "Microsoft Graph Chart" : "MSGraph.Chart.8");
			break;
		case OleObjectType.Excel_97_2003_Worksheet:
			result = (isWord2003 ? "Microsoft Office Excel 2003 Worksheet" : "Excel.Sheet.8");
			break;
		case OleObjectType.ExcelBinaryWorksheet:
			result = (isWord2003 ? "Microsoft Office Excel Binary Worksheet" : "Excel.SheetBinaryMacroEnabled.12");
			break;
		case OleObjectType.ExcelChart:
			result = (isWord2003 ? "Microsoft Office Excel Chart" : "Excel.Chart.8");
			break;
		case OleObjectType.ExcelMacroWorksheet:
			result = (isWord2003 ? "Microsoft Office Excel Worksheet (code)" : "Excel.SheetMacroEnabled.12");
			break;
		case OleObjectType.ExcelWorksheet:
			result = (isWord2003 ? "Microsoft Office Excel Worksheet" : "Excel.Sheet.12");
			break;
		case OleObjectType.PowerPoint_97_2003_Presentation:
			result = (isWord2003 ? "Microsoft Office PowerPoint 97-2003 Presentation" : "PowerPoint.Show.8");
			break;
		case OleObjectType.PowerPoint_97_2003_Slide:
			result = (isWord2003 ? "Microsoft Office PowerPoint 97-2003 Slide" : "PowerPoint.Slide.8");
			break;
		case OleObjectType.PowerPointMacroPresentation:
			result = (isWord2003 ? "Microsoft Office PowerPoint Macro-Enabled Presentation" : "PowerPoint.ShowMacroEnabled.12");
			break;
		case OleObjectType.PowerPointMacroSlide:
			result = (isWord2003 ? "Microsoft Office PowerPoint Macro-Enabled Slide" : "PowerPoint.SlideMacroEnabled.12");
			break;
		case OleObjectType.PowerPointPresentation:
			result = (isWord2003 ? "Microsoft Office PowerPoint Presentation" : "PowerPoint.Show.12");
			break;
		case OleObjectType.PowerPointSlide:
			result = (isWord2003 ? "Microsoft Office PowerPoint Slide" : "PowerPoint.Slide.12");
			break;
		case OleObjectType.Word_97_2003_Document:
			result = (isWord2003 ? "Microsoft Office Word 97-2003 Document" : "Word.Document.8");
			break;
		case OleObjectType.WordDocument:
			result = (isWord2003 ? "Microsoft Office Word Document" : "Word.Document.12");
			break;
		case OleObjectType.WordMacroDocument:
			result = (isWord2003 ? "Microsoft Office Word Macro-Enabled Document" : "Word.DocumentMacroEnabled.12");
			break;
		case OleObjectType.VisioDrawing:
			result = (isWord2003 ? "Microsoft Visio Drawing" : "Visio.Drawing.11");
			break;
		case OleObjectType.OpenDocumentPresentation:
			result = (isWord2003 ? "OpenDocument Presentation" : "PowerPoint.OpenDocumentPresentation.12");
			break;
		case OleObjectType.OpenDocumentSpreadsheet:
			result = (isWord2003 ? "OpenDocument Spreadsheet" : "Excel.OpenDocumentSpreadsheet.12");
			break;
		case OleObjectType.OpenOfficeSpreadsheet:
			result = "opendocument.CalcDocument.1";
			break;
		case OleObjectType.OpenOfficeText:
			result = "opendocument.WriterDocument.1";
			break;
		case OleObjectType.OpenOfficeSpreadsheet1_1:
			result = "soffice.StarCalcDocument.6";
			break;
		case OleObjectType.OpenOfficeText_1_1:
			result = "soffice.StarWriterDocument.6";
			break;
		case OleObjectType.VideoClip:
			result = (isWord2003 ? "Video Clip" : "AVIFile");
			break;
		case OleObjectType.WaveSound:
			result = (isWord2003 ? "WaveSound" : "SoundRec");
			break;
		case OleObjectType.WordPadDocument:
			result = (isWord2003 ? "WordPad Document" : "WordPad.Document.1");
			break;
		case OleObjectType.MIDISequence:
			result = "MIDI Sequence";
			break;
		}
		return result;
	}

	internal static Guid GetGUID(OleObjectType type)
	{
		Guid result = Guid.NewGuid();
		string text = null;
		switch (type)
		{
		case OleObjectType.AdobeAcrobatDocument:
			text = "b801ca65-a1fc-11d0-85ad-444553540000";
			break;
		case OleObjectType.Equation:
			text = "0002ce02-0000-0000-c000-000000000046";
			break;
		case OleObjectType.GraphChart:
			text = "00020803-0000-0000-c000-000000000046";
			break;
		case OleObjectType.Excel_97_2003_Worksheet:
			text = "00020820-0000-0000-c000-000000000046";
			break;
		case OleObjectType.ExcelChart:
			text = "00020821-0000-0000-c000-000000000046";
			break;
		case OleObjectType.ExcelWorksheet:
			text = "00020830-0000-0000-c000-000000000046";
			break;
		case OleObjectType.ExcelMacroWorksheet:
			text = "00020832-0000-0000-c000-000000000046";
			break;
		case OleObjectType.ExcelBinaryWorksheet:
			text = "00020833-0000-0000-c000-000000000046";
			break;
		case OleObjectType.PowerPoint_97_2003_Presentation:
			text = "64818d10-4f9b-11cf-86ea-00aa00b929e8";
			break;
		case OleObjectType.PowerPoint_97_2003_Slide:
			text = "64818d11-4f9b-11cf-86ea-00aa00b929e8";
			break;
		case OleObjectType.PowerPointMacroPresentation:
			text = "dc020317-e6e2-4a62-b9fa-b3efe16626f4";
			break;
		case OleObjectType.PowerPointMacroSlide:
			text = "3c18eae4-bc25-4134-b7df-1eca1337dddc";
			break;
		case OleObjectType.PowerPointPresentation:
			text = "cf4f55f4-8f87-4d47-80bb-5808164bb3f8";
			break;
		case OleObjectType.PowerPointSlide:
			text = "048eb43e-2059-422f-95e0-557da96038af";
			break;
		case OleObjectType.WordDocument:
			text = "f4754c9b-64f5-4b40-8af4-679732ac0607";
			break;
		case OleObjectType.Word_97_2003_Document:
			text = "00020906-0000-0000-c000-000000000046";
			break;
		case OleObjectType.WordMacroDocument:
			text = "18a06b6b-2f3f-4e2b-a611-52be631b2d22";
			break;
		case OleObjectType.VisioDrawing:
			text = "00021a14-0000-0000-c000-000000000046";
			break;
		case OleObjectType.OpenDocumentPresentation:
			text = "c282417b-2662-44b8-8a94-3bff61c50900";
			break;
		case OleObjectType.OpenDocumentSpreadsheet:
			text = "eabcecdb-cc1c-4a6f-b4e3-7f888a5adfc8";
			break;
		case OleObjectType.OpenOfficeSpreadsheet:
			text = "7fa8ae11-b3e3-4d88-aabf-255526cd1ce8";
			break;
		case OleObjectType.OpenOfficeText:
			text = "f616b81f-7bb8-4f22-b8a5-47428d59f8ad";
			break;
		case OleObjectType.OpenOfficeSpreadsheet1_1:
			text = "7b342dc4-139a-4a46-8a93-db0827ccee9c";
			break;
		case OleObjectType.OpenOfficeText_1_1:
			text = "30a2652a-ddf7-45e7-aca6-3eab26fc8a4e";
			break;
		case OleObjectType.WordPadDocument:
			text = "73fddc80-aea9-101a-98a7-00aa00374959";
			break;
		case OleObjectType.BitmapImage:
			text = "0003000a-0000-0000-c000-000000000046";
			break;
		case OleObjectType.Package:
			text = "0003000c-0000-0000-c000-000000000046";
			break;
		case OleObjectType.MIDISequence:
			text = "00022603-0000-0000-c000-000000000046";
			break;
		}
		if (text != null)
		{
			result = new Guid(text);
		}
		return result;
	}

	private static bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}
}
