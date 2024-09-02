namespace DocGen.Pdf;

internal class BitOperation
{
	public const int LEFT_SHIFT = 0;

	public const int RIGHT_SHIFT = 1;

	public int GetInt32(short[] number)
	{
		return (number[0] << 24) | (number[1] << 16) | (number[2] << 8) | number[3];
	}

	public int GetInt16(short[] number)
	{
		return (number[0] << 8) | number[1];
	}

	public long Bit32Shift(long number, int shift, int direction)
	{
		number = ((direction != 0) ? (number >> shift) : (number << shift));
		long num = 4294967295L;
		return number & num;
	}

	public int Bit8Shift(int number, int shift, int direction)
	{
		number = ((direction != 0) ? (number >> shift) : (number << shift));
		int num = 255;
		return number & num;
	}
}
