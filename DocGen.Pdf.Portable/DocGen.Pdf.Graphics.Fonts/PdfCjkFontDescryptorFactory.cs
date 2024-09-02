using System;
using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal sealed class PdfCjkFontDescryptorFactory
{
	internal static PdfDictionary GetFontDescryptor(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, PdfFontMetrics fontMetrics)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		switch (fontFamily)
		{
		case PdfCjkFontFamily.HanyangSystemsGothicMedium:
			FillHanyangSystemsGothicMedium(pdfDictionary, fontFamily, fontMetrics);
			break;
		case PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium:
			FillHanyangSystemsShinMyeongJoMedium(pdfDictionary, fontFamily, fontMetrics);
			break;
		case PdfCjkFontFamily.HeiseiKakuGothicW5:
			FillHeiseiKakuGothicW5(pdfDictionary, fontStyle, fontFamily, fontMetrics);
			break;
		case PdfCjkFontFamily.HeiseiMinchoW3:
			FillHeiseiMinchoW3(pdfDictionary, fontFamily, fontMetrics);
			break;
		case PdfCjkFontFamily.MonotypeHeiMedium:
			FillMonotypeHeiMedium(pdfDictionary, fontFamily, fontMetrics);
			break;
		case PdfCjkFontFamily.MonotypeSungLight:
			FillMonotypeSungLight(pdfDictionary, fontFamily, fontMetrics);
			break;
		case PdfCjkFontFamily.SinoTypeSongLight:
			FillSinoTypeSongLight(pdfDictionary, fontFamily, fontMetrics);
			break;
		}
		return pdfDictionary;
	}

	private static void FillMonotypeSungLight(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(-160, -249, 1175, 1137);
		FillFontBBox(fontDescryptor, fontBBox);
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = (PdfNumber)(fontDescryptor["AvgWidth"] = new PdfNumber(1000));
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(880);
		fontDescryptor["XHeight"] = new PdfNumber(616);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillHeiseiKakuGothicW5(PdfDictionary fontDescryptor, PdfFontStyle fontStyle, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(-92, -250, 1102, 1172);
		Rectangle fontBBox2 = new Rectangle(-92, -250, 1102, 1932);
		if ((fontStyle & (PdfFontStyle.Bold | PdfFontStyle.Italic)) != PdfFontStyle.Italic)
		{
			FillFontBBox(fontDescryptor, fontBBox);
		}
		else
		{
			FillFontBBox(fontDescryptor, fontBBox2);
		}
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = new PdfNumber(1000);
		fontDescryptor["AvgWidth"] = new PdfNumber(689);
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(718);
		fontDescryptor["XHeight"] = new PdfNumber(500);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillHanyangSystemsShinMyeongJoMedium(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(0, -148, 1001, 1028);
		FillFontBBox(fontDescryptor, fontBBox);
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = (PdfNumber)(fontDescryptor["AvgWidth"] = new PdfNumber(1000));
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(880);
		fontDescryptor["XHeight"] = new PdfNumber(616);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillHeiseiMinchoW3(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(-123, -257, 1124, 1167);
		FillFontBBox(fontDescryptor, fontBBox);
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = new PdfNumber(1000);
		fontDescryptor["AvgWidth"] = new PdfNumber(702);
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(718);
		fontDescryptor["XHeight"] = new PdfNumber(500);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillSinoTypeSongLight(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(-25, -254, 1025, 1134);
		FillFontBBox(fontDescryptor, fontBBox);
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = (PdfNumber)(fontDescryptor["AvgWidth"] = new PdfNumber(1000));
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(880);
		fontDescryptor["XHeight"] = new PdfNumber(616);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillMonotypeHeiMedium(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(-45, -250, 1060, 1137);
		FillFontBBox(fontDescryptor, fontBBox);
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = (PdfNumber)(fontDescryptor["AvgWidth"] = new PdfNumber(1000));
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(880);
		fontDescryptor["XHeight"] = new PdfNumber(616);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillHanyangSystemsGothicMedium(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		Rectangle fontBBox = new Rectangle(-6, -145, 1009, 1025);
		FillFontBBox(fontDescryptor, fontBBox);
		FillKnownInfo(fontDescryptor, fontFamily, fontMetrics);
		fontDescryptor["Flags"] = new PdfNumber(4);
		PdfNumber value = (PdfNumber)(fontDescryptor["StemV"] = new PdfNumber(93));
		fontDescryptor["StemH"] = value;
		PdfNumber value2 = (PdfNumber)(fontDescryptor["AvgWidth"] = new PdfNumber(1000));
		fontDescryptor["MaxWidth"] = value2;
		fontDescryptor["CapHeight"] = new PdfNumber(880);
		fontDescryptor["XHeight"] = new PdfNumber(616);
		fontDescryptor["Leading"] = new PdfNumber(250);
	}

	private static void FillKnownInfo(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily, PdfFontMetrics fontMetrics)
	{
		fontDescryptor["FontName"] = new PdfName(fontMetrics.PostScriptName);
		fontDescryptor["Type"] = new PdfName("FontDescriptor");
		fontDescryptor["ItalicAngle"] = new PdfNumber(0);
		fontDescryptor["MissingWidth"] = new PdfNumber((fontMetrics.WidthTable as CjkWidthTable).DefaultWidth);
		fontDescryptor["Ascent"] = new PdfNumber(fontMetrics.Ascent);
		fontDescryptor["Descent"] = new PdfNumber(fontMetrics.Descent);
		FillFlags(fontDescryptor, fontFamily);
	}

	private static void FillFlags(PdfDictionary fontDescryptor, PdfCjkFontFamily fontFamily)
	{
		switch (fontFamily)
		{
		case PdfCjkFontFamily.HeiseiKakuGothicW5:
		case PdfCjkFontFamily.HanyangSystemsGothicMedium:
		case PdfCjkFontFamily.MonotypeHeiMedium:
			fontDescryptor["Flags"] = new PdfNumber(4);
			break;
		case PdfCjkFontFamily.HeiseiMinchoW3:
		case PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium:
		case PdfCjkFontFamily.MonotypeSungLight:
		case PdfCjkFontFamily.SinoTypeSongLight:
			fontDescryptor["Flags"] = new PdfNumber(6);
			break;
		default:
			throw new ArgumentException("Unsupported font family: " + fontFamily, "fontFamily");
		}
	}

	private static void FillFontBBox(PdfDictionary fontDescryptor, Rectangle fontBBox)
	{
		fontDescryptor["FontBBox"] = PdfArray.FromRectangle(fontBBox);
	}
}
