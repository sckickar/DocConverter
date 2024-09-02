namespace DocGen.Pdf.Parsing;

internal class DataReader
{
	private byte[] source;

	private int bits;

	private int mask;

	private int sourcePointer;

	private int bitPointer;

	public DataReader(byte[] source, int intSizeInBits)
	{
		this.source = source;
		bits = intSizeInBits;
		mask = (1 << bits) - 1;
	}

	internal int Read()
	{
		if (sourcePointer >= source.Length)
		{
			return -1;
		}
		switch (bits)
		{
		case 8:
			return source[sourcePointer++];
		default:
		{
			int result = (source[sourcePointer] >> 8 - bitPointer - bits) & mask;
			bitPointer += bits;
			if (bitPointer == 8)
			{
				sourcePointer++;
				bitPointer = 0;
			}
			return result;
		}
		case 16:
			return (source[sourcePointer++] << 8) | source[sourcePointer++];
		}
	}

	public bool MoveToNextRow()
	{
		if (sourcePointer >= source.Length)
		{
			return false;
		}
		if (bitPointer != 0)
		{
			sourcePointer++;
			bitPointer = 0;
		}
		return true;
	}
}
