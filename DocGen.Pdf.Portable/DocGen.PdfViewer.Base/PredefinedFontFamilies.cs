using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal static class PredefinedFontFamilies
{
	private static readonly Dictionary<string, string> mapping;

	static PredefinedFontFamilies()
	{
		mapping = new Dictionary<string, string>();
		RegisterFontFamily("Iskoola Pota");
		RegisterFontFamily("Aparajita");
		RegisterFontFamily("Kokila");
		RegisterFontFamily("Utsaah");
		RegisterFontFamily("Agency FB");
		RegisterFontFamily("Aharoni");
		RegisterFontFamily("Algerian");
		RegisterFontFamily("Andalus");
		RegisterFontFamily("Andy");
		RegisterFontFamily("Angsana New");
		RegisterFontFamily("AngsanaUPC");
		RegisterFontFamily("Book Antiqua");
		RegisterFontFamily("Arabic Typesetting");
		RegisterFontFamily("Arial");
		RegisterFontFamily("Arial Narrow");
		RegisterFontFamily("Arial Black");
		RegisterFontFamily("Arial Unicode MS");
		RegisterFontFamily("Arial Rounded MT Bold");
		RegisterFontFamily("Baskerville Old Face");
		RegisterFontFamily("Batang");
		RegisterFontFamily("BatangChe");
		RegisterFontFamily("Gungsuh");
		RegisterFontFamily("GungsuhChe");
		RegisterFontFamily("Bauhaus 93");
		RegisterFontFamily("Bell MT");
		RegisterFontFamily("Bernard MT Condensed");
		RegisterFontFamily("Bodoni MT");
		RegisterFontFamily("Bodoni MT Condensed");
		RegisterFontFamily("Bodoni MT Black");
		RegisterFontFamily("Bodoni MT Poster Compressed");
		RegisterFontFamily("Bookman Old Style");
		RegisterFontFamily("Bradley Hand ITC");
		RegisterFontFamily("Britannic Bold");
		RegisterFontFamily("Berlin Sans FB");
		RegisterFontFamily("Berlin Sans FB Demi");
		RegisterFontFamily("Broadway");
		RegisterFontFamily("Browallia New");
		RegisterFontFamily("BrowalliaUPC");
		RegisterFontFamily("Brush Script MT");
		RegisterFontFamily("Bookshelf Symbol 7");
		RegisterFontFamily("Buxton Sketch");
		RegisterFontFamily("Calibri");
		RegisterFontFamily("Californian FB");
		RegisterFontFamily("Calisto MT");
		RegisterFontFamily("Cambria");
		RegisterFontFamily("Cambria Math");
		RegisterFontFamily("Candara");
		RegisterFontFamily("Castellar");
		RegisterFontFamily("Century Schoolbook");
		RegisterFontFamily("Centaur");
		RegisterFontFamily("Century");
		RegisterFontFamily("Chiller");
		RegisterFontFamily("Colonna MT");
		RegisterFontFamily("Comic Sans MS");
		RegisterFontFamily("Consolas");
		RegisterFontFamily("Constantia");
		RegisterFontFamily("Cooper Black");
		RegisterFontFamily("Copperplate Gothic Light");
		RegisterFontFamily("Copperplate Gothic Bold");
		RegisterFontFamily("Corbel");
		RegisterFontFamily("Cordia New");
		RegisterFontFamily("CordiaUPC");
		RegisterFontFamily("Courier New");
		RegisterFontFamily("Curlz MT");
		RegisterFontFamily("DaunPenh");
		RegisterFontFamily("David");
		RegisterFontFamily("DejaVu Sans Light");
		RegisterFontFamily("DejaVu Sans");
		RegisterFontFamily("DejaVu Sans Condensed");
		RegisterFontFamily("DejaVu Sans Mono");
		RegisterFontFamily("DejaVu Serif");
		RegisterFontFamily("DejaVu Serif Condensed");
		RegisterFontFamily("DokChampa");
		RegisterFontFamily("Ebrima");
		RegisterFontFamily("Elephant");
		RegisterFontFamily("Engravers MT");
		RegisterFontFamily("Eras Light ITC");
		RegisterFontFamily("Eras Medium ITC");
		RegisterFontFamily("Eras Demi ITC");
		RegisterFontFamily("Eras Bold ITC");
		RegisterFontFamily("Estrangelo Edessa");
		RegisterFontFamily("Euphemia");
		RegisterFontFamily("Felix Titling");
		RegisterFontFamily("Forte");
		RegisterFontFamily("Franklin Gothic Book");
		RegisterFontFamily("Franklin Gothic Medium");
		RegisterFontFamily("Franklin Gothic Medium Cond");
		RegisterFontFamily("Franklin Gothic Demi");
		RegisterFontFamily("Franklin Gothic Demi Cond");
		RegisterFontFamily("Franklin Gothic Heavy");
		RegisterFontFamily("FrankRuehl");
		RegisterFontFamily("Freestyle Script");
		RegisterFontFamily("French Script MT");
		RegisterFontFamily("Footlight MT Light");
		RegisterFontFamily("Garamond");
		RegisterFontFamily("Gautami");
		RegisterFontFamily("Gentium Basic");
		RegisterFontFamily("Gentium Book Basic");
		RegisterFontFamily("Georgia");
		RegisterFontFamily("Gigi");
		RegisterFontFamily("Gill Sans MT");
		RegisterFontFamily("Gill Sans MT Condensed");
		RegisterFontFamily("Gill Sans MT Ext Condensed Bold");
		RegisterFontFamily("Gill Sans Ultra Bold");
		RegisterFontFamily("Gill Sans Ultra Bold Condensed");
		RegisterFontFamily("Gisha");
		RegisterFontFamily("Gloucester MT Extra Condensed");
		RegisterFontFamily("Century Gothic");
		RegisterFontFamily("Goudy Old Style");
		RegisterFontFamily("Goudy Stout");
		RegisterFontFamily("Gulim");
		RegisterFontFamily("GulimChe");
		RegisterFontFamily("Dotum");
		RegisterFontFamily("DotumChe");
		RegisterFontFamily("Harlow Solid Italic");
		RegisterFontFamily("Harrington");
		RegisterFontFamily("Haettenschweiler");
		RegisterFontFamily("Microsoft Himalaya");
		RegisterFontFamily("High Tower Text");
		RegisterFontFamily("Impact");
		RegisterFontFamily("Imprint MT Shadow");
		RegisterFontFamily("Informal Roman");
		RegisterFontFamily("Blackadder ITC");
		RegisterFontFamily("Edwardian Script ITC");
		RegisterFontFamily("Kristen ITC");
		RegisterFontFamily("Jing Jing");
		RegisterFontFamily("Jokerman");
		RegisterFontFamily("Juice ITC");
		RegisterFontFamily("DFKai-SB");
		RegisterFontFamily("Kalinga");
		RegisterFontFamily("Kartika");
		RegisterFontFamily("Khmer UI");
		RegisterFontFamily("Kunstler Script");
		RegisterFontFamily("Lucida Sans Unicode");
		RegisterFontFamily("Lao UI");
		RegisterFontFamily("Latha");
		RegisterFontFamily("Wide Latin");
		RegisterFontFamily("Lucida Bright");
		RegisterFontFamily("Lucida Calligraphy");
		RegisterFontFamily("Leelawadee");
		RegisterFontFamily("Lucida Fax");
		RegisterFontFamily("Lucida Handwriting");
		RegisterFontFamily("Liberation Sans Narrow");
		RegisterFontFamily("Lucida Sans");
		RegisterFontFamily("Lucida Sans Typewriter");
		RegisterFontFamily("Lucida Console");
		RegisterFontFamily("Levenim MT");
		RegisterFontFamily("Magneto");
		RegisterFontFamily("Maiandra GD");
		RegisterFontFamily("Malgun Gothic");
		RegisterFontFamily("Mangal");
		RegisterFontFamily("Marlett");
		RegisterFontFamily("Matura MT Script Capitals");
		RegisterFontFamily("Meiryo");
		RegisterFontFamily("Meiryo UI");
		RegisterFontFamily("Microsoft Sans Serif");
		RegisterFontFamily("MingLiU");
		RegisterFontFamily("PMingLiU");
		RegisterFontFamily("MingLiU_HKSCS");
		RegisterFontFamily("MingLiU-ExtB");
		RegisterFontFamily("PMingLiU-ExtB");
		RegisterFontFamily("MingLiU_HKSCS-ExtB");
		RegisterFontFamily("Mistral");
		RegisterFontFamily("Modern No. 20");
		RegisterFontFamily("Moire Light");
		RegisterFontFamily("Moire");
		RegisterFontFamily("Moire ExtraBold");
		RegisterFontFamily("Mongolian Baiti");
		RegisterFontFamily("MoolBoran");
		RegisterFontFamily("Motorwerk");
		RegisterFontFamily("Miriam");
		RegisterFontFamily("Miriam Fixed");
		RegisterFontFamily("MS Gothic");
		RegisterFontFamily("MS PGothic");
		RegisterFontFamily("MS UI Gothic");
		RegisterFontFamily("Microsoft JhengHei");
		RegisterFontFamily("Microsoft MHei");
		RegisterFontFamily("MS Mincho");
		RegisterFontFamily("MS PMincho");
		RegisterFontFamily("Microsoft NeoGothic");
		RegisterFontFamily("Microsoft Uighur");
		RegisterFontFamily("Microsoft YaHei");
		RegisterFontFamily("Microsoft Yi Baiti");
		RegisterFontFamily("Monotype Corsiva");
		RegisterFontFamily("MV Boli");
		RegisterFontFamily("News Gothic");
		RegisterFontFamily("Niagara Engraved");
		RegisterFontFamily("Niagara Solid");
		RegisterFontFamily("Narkisim");
		RegisterFontFamily("Microsoft New Tai Lue");
		RegisterFontFamily("OCR A Extended");
		RegisterFontFamily("Old English Text MT");
		RegisterFontFamily("Onyx");
		RegisterFontFamily("OpenSymbol");
		RegisterFontFamily("MS Outlook");
		RegisterFontFamily("Palatino Linotype");
		RegisterFontFamily("Palace Script MT");
		RegisterFontFamily("Papyrus");
		RegisterFontFamily("Parchment");
		RegisterFontFamily("Perpetua");
		RegisterFontFamily("Perpetua Titling MT");
		RegisterFontFamily("Microsoft PhagsPa");
		RegisterFontFamily("Plantagenet Cherokee");
		RegisterFontFamily("Playbill");
		RegisterFontFamily("Poor Richard");
		RegisterFontFamily("Pristina");
		RegisterFontFamily("Quartz MS");
		RegisterFontFamily("Raavi");
		RegisterFontFamily("Rage Italic");
		RegisterFontFamily("Ravie");
		RegisterFontFamily("MS Reference Sans Serif");
		RegisterFontFamily("MS Reference Specialty");
		RegisterFontFamily("Rockwell");
		RegisterFontFamily("Rockwell Condensed");
		RegisterFontFamily("Rockwell Extra Bold");
		RegisterFontFamily("Rod");
		RegisterFontFamily("Script MT Bold");
		RegisterFontFamily("Segoe Keycaps");
		RegisterFontFamily("Segoe Marker");
		RegisterFontFamily("Segoe Print");
		RegisterFontFamily("Segoe Script");
		RegisterFontFamily("Segoe UI Light");
		RegisterFontFamily("Segoe UI");
		RegisterFontFamily("Segoe UI Semibold");
		RegisterFontFamily("Segoe UI Mono");
		RegisterFontFamily("Segoe UI Symbol");
		RegisterFontFamily("Shonar Bangla");
		RegisterFontFamily("Showcard Gothic");
		RegisterFontFamily("Shruti");
		RegisterFontFamily("FangSong");
		RegisterFontFamily("SimHei");
		RegisterFontFamily("KaiTi");
		RegisterFontFamily("Simplified Arabic");
		RegisterFontFamily("Simplified Arabic Fixed");
		RegisterFontFamily("SimSun");
		RegisterFontFamily("NSimSun");
		RegisterFontFamily("SimSun-ExtB");
		RegisterFontFamily("Snap ITC");
		RegisterFontFamily("Stencil");
		RegisterFontFamily("Sylfaen");
		RegisterFontFamily("Symbol");
		RegisterFontFamily("Tahoma");
		RegisterFontFamily("Microsoft Tai Le");
		RegisterFontFamily("Tw Cen MT");
		RegisterFontFamily("Tw Cen MT Condensed");
		RegisterFontFamily("Tw Cen MT Condensed Extra Bold");
		RegisterFontFamily("Tempus Sans ITC");
		RegisterFontFamily("Times New Roman");
		RegisterFontFamily("Traditional Arabic");
		RegisterFontFamily("Trebuchet MS");
		RegisterFontFamily("Tunga");
		RegisterFontFamily("DilleniaUPC");
		RegisterFontFamily("EucrosiaUPC");
		RegisterFontFamily("FreesiaUPC");
		RegisterFontFamily("IrisUPC");
		RegisterFontFamily("JasmineUPC");
		RegisterFontFamily("KodchiangUPC");
		RegisterFontFamily("LilyUPC");
		RegisterFontFamily("Vani");
		RegisterFontFamily("Verdana");
		RegisterFontFamily("Vijaya");
		RegisterFontFamily("Viner Hand ITC");
		RegisterFontFamily("Vivaldi");
		RegisterFontFamily("Vladimir Script");
		RegisterFontFamily("Vrinda");
		RegisterFontFamily("Webdings");
		RegisterFontFamily("Wingdings");
		RegisterFontFamily("Wingdings 2");
		RegisterFontFamily("Wingdings 3");
		RegisterFontFamily("Wasco Sans");
		RegisterFontFamily("Helvetica", "Arial");
		RegisterFontFamily("Courier", "Courier New");
	}

	public static string CreateFontKey(string font)
	{
		return font.Replace(" ", string.Empty).ToLower();
	}

	public static bool TryGetFontFamily(string font, out string fontFamily)
	{
		string text = CreateFontKey(StdFontsAssistant.StripFontName(font));
		if (mapping.TryGetValue(text, out fontFamily))
		{
			return true;
		}
		if (text.Contains("arial"))
		{
			fontFamily = "Arial";
			return true;
		}
		if (text.Contains("times"))
		{
			fontFamily = "Times New Roman";
			return true;
		}
		return false;
	}

	private static void RegisterFontFamily(string fontFamily)
	{
		string key = CreateFontKey(fontFamily);
		mapping[key] = fontFamily;
	}

	private static void RegisterFontFamily(string fontFamily, string realFontFamily)
	{
		string key = CreateFontKey(fontFamily);
		mapping[key] = realFontFamily;
	}
}
