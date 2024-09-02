using System;

namespace DocGen.Pdf.Graphics.Fonts;

internal sealed class PdfCjkStandardFontMetricsFactory
{
	private const float c_subSuperScriptFactor = 1.52f;

	private PdfCjkStandardFontMetricsFactory()
	{
	}

	public static PdfFontMetrics GetMetrics(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = null;
		pdfFontMetrics = fontFamily switch
		{
			PdfCjkFontFamily.HanyangSystemsGothicMedium => GetHanyangSystemsGothicMediumMetrix(fontFamily, fontStyle, size), 
			PdfCjkFontFamily.HanyangSystemsShinMyeongJoMedium => GetHanyangSystemsShinMyeongJoMediumMetrix(fontFamily, fontStyle, size), 
			PdfCjkFontFamily.HeiseiKakuGothicW5 => GetHeiseiKakuGothicW5Metrix(fontFamily, fontStyle, size), 
			PdfCjkFontFamily.HeiseiMinchoW3 => GetHeiseiMinchoW3(fontFamily, fontStyle, size), 
			PdfCjkFontFamily.MonotypeHeiMedium => GetMonotypeHeiMedium(fontFamily, fontStyle, size), 
			PdfCjkFontFamily.MonotypeSungLight => GetMonotypeSungLightMetrix(fontFamily, fontStyle, size), 
			PdfCjkFontFamily.SinoTypeSongLight => GetSinoTypeSongLight(fontFamily, fontStyle, size), 
			_ => throw new ArgumentException("Unsupported font family", "fontFamily"), 
		};
		pdfFontMetrics.Name = fontFamily.ToString();
		pdfFontMetrics.SubScriptSizeFactor = 1.52f;
		pdfFontMetrics.SuperscriptSizeFactor = 1.52f;
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetHanyangSystemsGothicMediumMetrix(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 127, 500));
		cjkWidthTable.Add(new CjkSameWidth(8094, 8190, 500));
		pdfFontMetrics.Ascent = 880f;
		pdfFontMetrics.Descent = -120f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HYGoThic-Medium,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "HYGoThic-Medium,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HYGoThic-Medium,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "HYGoThic-Medium";
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetMonotypeHeiMedium(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 95, 500));
		cjkWidthTable.Add(new CjkSameWidth(13648, 13742, 500));
		pdfFontMetrics.Ascent = 880f;
		pdfFontMetrics.Descent = -120f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "MHei-Medium,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "MHei-Medium,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "MHei-Medium,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "MHei-Medium";
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetMonotypeSungLightMetrix(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 95, 500));
		cjkWidthTable.Add(new CjkSameWidth(13648, 13742, 500));
		pdfFontMetrics.Ascent = 880f;
		pdfFontMetrics.Descent = -120f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "MSung-Light,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "MSung-Light,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "MSung-Light,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "MSung-Light";
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetSinoTypeSongLight(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 95, 500));
		cjkWidthTable.Add(new CjkSameWidth(814, 939, 500));
		cjkWidthTable.Add(new CjkDifferentWidth(7712, new int[1] { 500 }));
		cjkWidthTable.Add(new CjkDifferentWidth(7716, new int[1] { 500 }));
		pdfFontMetrics.Ascent = 880f;
		pdfFontMetrics.Descent = -120f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "STSong-Light,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "STSong-Light,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "STSong-Light,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "STSong-Light";
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetHeiseiMinchoW3(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 95, 500));
		cjkWidthTable.Add(new CjkSameWidth(231, 632, 500));
		pdfFontMetrics.Ascent = 857f;
		pdfFontMetrics.Descent = -143f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HeiseiMin-W3,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "HeiseiMin-W3,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HeiseiMin-W3,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "HeiseiMin-W3";
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetHeiseiKakuGothicW5Metrix(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 95, 500));
		cjkWidthTable.Add(new CjkSameWidth(231, 632, 500));
		pdfFontMetrics.Ascent = 857f;
		pdfFontMetrics.Descent = -125f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HeiseiKakuGo-W5,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "HeiseiKakuGo-W5,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HeiseiKakuGo-W5,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "HeiseiKakuGo-W5";
		}
		return pdfFontMetrics;
	}

	private static PdfFontMetrics GetHanyangSystemsShinMyeongJoMediumMetrix(PdfCjkFontFamily fontFamily, PdfFontStyle fontStyle, float size)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		CjkWidthTable cjkWidthTable = (CjkWidthTable)(pdfFontMetrics.WidthTable = new CjkWidthTable(1000));
		cjkWidthTable.Add(new CjkSameWidth(1, 95, 500));
		cjkWidthTable.Add(new CjkSameWidth(8094, 8190, 500));
		pdfFontMetrics.Ascent = 880f;
		pdfFontMetrics.Descent = -120f;
		pdfFontMetrics.Size = size;
		pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
		if ((fontStyle & PdfFontStyle.Bold) != 0 && (fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HYSMyeongJo-Medium,BoldItalic";
		}
		else if ((fontStyle & PdfFontStyle.Bold) != 0)
		{
			pdfFontMetrics.PostScriptName = "HYSMyeongJo-Medium,Bold";
		}
		else if ((fontStyle & PdfFontStyle.Italic) != 0)
		{
			pdfFontMetrics.PostScriptName = "HYSMyeongJo-Medium,Italic";
		}
		else
		{
			pdfFontMetrics.PostScriptName = "HYSMyeongJo-Medium";
		}
		return pdfFontMetrics;
	}
}
