using System;
using System.Text;

internal class Windows1252Encoding : Encoding
{
	private char[] map = new char[256]
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
		'x', 'y', 'z', '{', '|', '}', '~', '\u007f', '€', '\u0081',
		'‚', 'ƒ', '„', '…', '†', '‡', 'ˆ', '‰', 'Š', '‹',
		'Œ', '\u008d', 'Ž', '\u008f', '\u0090', '‘', '’', '“', '”', '•',
		'–', '—', '\u02dc', '™', 'š', '›', 'œ', '\u009d', 'ž', 'Ÿ',
		'\u00a0', '¡', '¢', '£', '¤', '¥', '¦', '§', '\u00a8', '©',
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

	public override string WebName => "windows-1252";

	public override int GetMaxByteCount(int charCount)
	{
		return charCount;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}

	public override int GetByteCount(char[] chars, int index, int count)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		if (index < 0 || index > chars.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || index + count > chars.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return count;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (index < 0 || index > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || index + count > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		return count;
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		if (chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (charIndex < 0)
		{
			throw new ArgumentOutOfRangeException("charIndex");
		}
		if (charCount < 0)
		{
			throw new ArgumentOutOfRangeException("charCount");
		}
		if (byteIndex < 0)
		{
			throw new ArgumentOutOfRangeException("byteIndex");
		}
		if (charIndex + charCount > chars.Length)
		{
			throw new ArgumentOutOfRangeException("charCount");
		}
		if (byteIndex + charCount > bytes.Length)
		{
			throw new ArgumentException("bytes");
		}
		int num = charIndex + charCount;
		int num2 = charIndex;
		int num3 = byteIndex;
		while (num2 < num)
		{
			char c = chars[num2];
			bytes[num3] = c switch
			{
				'€' => 128, 
				'‚' => 130, 
				'ƒ' => 131, 
				'„' => 132, 
				'…' => 133, 
				'†' => 134, 
				'‡' => 135, 
				'ˆ' => 136, 
				'‰' => 137, 
				'Š' => 138, 
				'‹' => 139, 
				'Œ' => 140, 
				'Ž' => 142, 
				'‘' => 145, 
				'’' => 146, 
				'“' => 147, 
				'”' => 148, 
				'•' => 149, 
				'–' => 150, 
				'—' => 151, 
				'\u02dc' => 152, 
				'™' => 153, 
				'š' => 154, 
				'›' => 155, 
				'œ' => 156, 
				'ž' => 158, 
				'Ÿ' => 159, 
				_ => (byte)((c > 'ÿ') ? 26 : ((byte)c)), 
			};
			num2++;
			num3++;
		}
		return charCount;
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (chars == null)
		{
			throw new ArgumentNullException("chars");
		}
		if (byteIndex < 0)
		{
			throw new ArgumentOutOfRangeException("byteIndex");
		}
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount");
		}
		if (charIndex < 0)
		{
			throw new ArgumentOutOfRangeException("charIndex");
		}
		if (byteIndex + byteCount > bytes.Length)
		{
			throw new ArgumentOutOfRangeException("byteCount");
		}
		if (charIndex + byteCount > chars.Length)
		{
			throw new ArgumentException("chars");
		}
		int num = byteIndex + byteCount;
		int num2 = byteIndex;
		int num3 = charIndex;
		while (num2 < num)
		{
			byte b = bytes[num2];
			char c = map[b];
			chars[num3] = c;
			num2++;
			num3++;
		}
		return byteCount;
	}
}
