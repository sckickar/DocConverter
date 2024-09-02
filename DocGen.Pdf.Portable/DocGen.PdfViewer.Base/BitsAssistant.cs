namespace DocGen.PdfViewer.Base;

internal static class BitsAssistant
{
	internal static bool GetBit(int n, byte bit)
	{
		return (n & (1 << (int)bit)) != 0;
	}

	internal static byte[] GetBits(byte bt)
	{
		byte[] array = new byte[8];
		for (int i = 0; i < 8; i++)
		{
			array[i] = (byte)(bt % 2);
			bt /= 2;
		}
		return array;
	}

	internal static byte ToByte(byte[] bits, int offset, int count)
	{
		byte b = 0;
		int num = 1;
		for (int i = 0; i < count; i++)
		{
			b += (byte)(bits[offset + i] * num);
			num *= 2;
		}
		return b;
	}
}
