using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal class PdfCidFont : PdfDictionary
{
	public PdfCidFont(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, PdfFontMetrics fontMetrics)
	{
		base["Type"] = new PdfName("Font");
		base["Subtype"] = new PdfName("CIDFontType2");
		base["BaseFont"] = new PdfName(fontMetrics.PostScriptName);
		base["DW"] = new PdfNumber((fontMetrics.WidthTable as CjkWidthTable).DefaultWidth);
		base["W"] = fontMetrics.WidthTable.ToArray();
		base["FontDescriptor"] = PdfCjkFontDescryptorFactory.GetFontDescryptor(fontFamily, fontStyle, fontMetrics);
		base["CIDSystemInfo"] = GetSystemInfo(fontFamily);
	}

	private PdfDictionary GetSystemInfo(PdfCjkFontFamily fontFamily)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary["Registry"] = new PdfString("Adobe");
		switch (fontFamily)
		{
		case PdfCjkFontFamily.HanyangSystemsGothicMedium:
		case PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium:
			pdfDictionary["Ordering"] = new PdfString("Korea1");
			pdfDictionary["Supplement"] = new PdfNumber(1);
			break;
		case PdfCjkFontFamily.HeiseiKakuGothicW5:
		case PdfCjkFontFamily.HeiseiMinchoW3:
			pdfDictionary["Ordering"] = new PdfString("Japan1");
			pdfDictionary["Supplement"] = new PdfNumber(2);
			break;
		case PdfCjkFontFamily.MonotypeHeiMedium:
		case PdfCjkFontFamily.MonotypeSungLight:
			pdfDictionary["Ordering"] = new PdfString("CNS1");
			pdfDictionary["Supplement"] = new PdfNumber(0);
			break;
		case PdfCjkFontFamily.SinoTypeSongLight:
			pdfDictionary["Ordering"] = new PdfString("GB1");
			pdfDictionary["Supplement"] = new PdfNumber(2);
			break;
		default:
			throw new ArgumentException("Unsupported font family: " + fontFamily, "fontFamily");
		}
		return pdfDictionary;
	}
}
