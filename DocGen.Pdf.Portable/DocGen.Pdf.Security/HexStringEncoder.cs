using System.IO;

namespace DocGen.Pdf.Security;

internal class HexStringEncoder : IHexEncoder
{
	private static readonly byte[] table = new byte[16]
	{
		48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
		97, 98, 99, 100, 101, 102
	};

	private static readonly byte[] tableDecode = CreateDecodeTable(table);

	private static byte[] CreateDecodeTable(byte[] value)
	{
		byte[] array = new byte[128];
		for (int i = 0; i < value.Length; i++)
		{
			array[value[i]] = (byte)i;
		}
		array[65] = array[97];
		array[66] = array[98];
		array[67] = array[99];
		array[68] = array[100];
		array[69] = array[101];
		array[70] = array[102];
		return array;
	}

	public int Encode(byte[] data, int offset, int length, Stream outputStream)
	{
		for (int i = offset; i < offset + length; i++)
		{
			int num = data[i];
			outputStream.WriteByte(table[num >> 4]);
			outputStream.WriteByte(table[num & 0xF]);
		}
		return length * 2;
	}

	private static bool Ignore(char character)
	{
		if (character != '\n' && character != '\r' && character != '\t')
		{
			return character == ' ';
		}
		return true;
	}

	public int Decode(byte[] data, int offset, int length, Stream outputStream)
	{
		int num = 0;
		int num2 = offset + length;
		while (num2 > offset && Ignore((char)data[num2 - 1]))
		{
			num2--;
		}
		int i = offset;
		while (i < num2)
		{
			for (; i < num2 && Ignore((char)data[i]); i++)
			{
			}
			byte b = tableDecode[data[i++]];
			for (; i < num2 && Ignore((char)data[i]); i++)
			{
			}
			byte b2 = tableDecode[data[i++]];
			outputStream.WriteByte((byte)((b << 4) | b2));
			num++;
		}
		return num;
	}

	public int DecodeString(string data, Stream outputStream)
	{
		int num = 0;
		int num2 = data.Length;
		while (num2 > 0 && Ignore(data[num2 - 1]))
		{
			num2--;
		}
		int i = 0;
		while (i < num2)
		{
			for (; i < num2 && Ignore(data[i]); i++)
			{
			}
			byte b = tableDecode[(uint)data[i++]];
			for (; i < num2 && Ignore(data[i]); i++)
			{
			}
			byte b2 = tableDecode[(uint)data[i++]];
			outputStream.WriteByte((byte)((b << 4) | b2));
			num++;
		}
		return num;
	}
}
