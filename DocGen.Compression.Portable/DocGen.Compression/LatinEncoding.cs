using System.Text;

namespace DocGen.Compression;

internal class LatinEncoding : Encoding
{
	public override int GetByteCount(char[] chars, int index, int count)
	{
		return count;
	}

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
	{
		int num = 0;
		while (num < charCount)
		{
			bytes[byteIndex] = (byte)(chars[charIndex] & 0xFFu);
			num++;
			charIndex++;
			byteIndex++;
		}
		return charCount;
	}

	public override int GetCharCount(byte[] bytes, int index, int count)
	{
		return count;
	}

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
	{
		int num = byteCount * 2;
		byte[] array = new byte[num];
		int num2 = byteIndex;
		int num3 = byteIndex + byteCount;
		int num4 = 0;
		while (num2 < num3)
		{
			array[num4] = bytes[num2];
			array[num4 + 1] = 0;
			num2++;
			num4 += 2;
		}
		return Encoding.Unicode.GetChars(array, 0, num, chars, 0);
	}

	public override int GetMaxByteCount(int charCount)
	{
		return charCount;
	}

	public override int GetMaxCharCount(int byteCount)
	{
		return byteCount;
	}
}
