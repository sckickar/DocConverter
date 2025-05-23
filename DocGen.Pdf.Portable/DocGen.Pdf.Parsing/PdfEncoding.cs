using System.Text.RegularExpressions;

namespace DocGen.Pdf.Parsing;

internal class PdfEncoding
{
	private Regex regex = new Regex("[\u0080-ÿ]");

	private char[] pdfEncodingByteToChar = new char[256]
	{
		'\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
		'\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013',
		'\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d',
		'\u001e', '\u001f', ' ', '!', '"', '#', '$', '%', '&', '\'',
		'(', ')', '*', '+', ',', '-', '.', '/', '0', '1',
		'2', '3', '4', '5', '6', '7', '8', '9', ':', ';',
		'<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E',
		'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
		'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y',
		'Z', '[', '\\', ']', '^', '_', '`', 'a', 'b', 'c',
		'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
		'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
		'x', 'y', 'z', '{', '|', '}', '~', '\u007f', '•', '†',
		'‡', '…', '—', '–', 'ƒ', '⁄', '‹', '›', '−', '‰',
		'„', '“', '”', '‘', '’', '‚', '™', 'ﬁ', 'ﬂ', 'Ł',
		'Œ', 'Š', 'Ÿ', 'Ž', 'ı', 'ł', 'œ', 'š', 'ž', '\ufffd',
		'€', '¡', '¢', '£', '¤', '¥', '¦', '§', '\u00a8', '©',
		'ª', '«', '¬', '\u00ad', '®', '\u00af', '°', '±', '²', '³',
		'\u00b4', 'µ', '¶', '·', '\u00b8', '¹', 'º', '»', '¼', '½',
		'¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç',
		'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð', 'Ñ',
		'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú', 'Û',
		'Ü', 'Ý', 'Þ', 'ß', 'à', 'á', 'â', 'ã', 'ä', 'å',
		'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï',
		'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', '÷', 'ø', 'ù',
		'ú', 'û', 'ü', 'ý', 'þ', 'ÿ'
	};

	internal PdfEncoding()
	{
	}

	internal string ConvertUnicodeToString(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '\u00a0' && regex.IsMatch(text[i].ToString()))
			{
				char newChar = pdfEncodingByteToChar[(byte)text[i] & 0xFF];
				text = text.Replace(text[i], newChar);
			}
		}
		return text;
	}
}
