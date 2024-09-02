using System.IO;

namespace DocGen.DocIO.DLS;

internal class Utf8Checker
{
	internal static bool IsUtf8(Stream stream)
	{
		byte[] array = new byte[(stream.Length > 4) ? 4 : stream.Length];
		stream.Position = 0L;
		stream.Read(array, 0, array.Length);
		stream.Position = 0L;
		if (array.Length >= 2 && ((array[0] == byte.MaxValue && array[1] == 254) || (array[0] == 254 && array[1] == byte.MaxValue)))
		{
			return false;
		}
		if (array.Length >= 3)
		{
			if (array[0] == 239 && array[1] == 187 && array[2] == 191)
			{
				return true;
			}
			if (array[0] == 43 && array[1] == 47 && array[2] == 118)
			{
				return false;
			}
		}
		if (array.Length == 4 && ((array[0] == byte.MaxValue && array[1] == 254 && array[2] == 0 && array[3] == 0) || (array[0] == 0 && array[1] == 0 && array[2] == 254 && array[3] == byte.MaxValue)))
		{
			return false;
		}
		bool result = !HasExtendedASCIICharacter(stream);
		stream.Position = 0L;
		return result;
	}

	private static bool HasExtendedASCIICharacter(Stream stream)
	{
		stream.Position = 0L;
		while (stream.Position < stream.Length)
		{
			int num = stream.ReadByte();
			if (num == -1)
			{
				break;
			}
			if (num <= 127)
			{
				continue;
			}
			if (num >= 192 && num <= 223)
			{
				int num2 = stream.ReadByte();
				if (num2 < 128 || num2 > 191)
				{
					return true;
				}
				continue;
			}
			if (num >= 224 && num <= 239)
			{
				int num3 = 0;
				while (num3 < 2)
				{
					int num4 = stream.ReadByte();
					num3++;
					if (num4 < 128 || num4 > 191)
					{
						return true;
					}
				}
				continue;
			}
			if (num >= 240 && num <= 247)
			{
				int num5 = 0;
				while (num5 < 3)
				{
					int num6 = stream.ReadByte();
					num5++;
					if (num6 < 128 || num6 > 191)
					{
						return true;
					}
				}
				continue;
			}
			return true;
		}
		return false;
	}
}
