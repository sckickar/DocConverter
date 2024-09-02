namespace DocGen.Pdf;

internal class RectangularArrays
{
	internal int[][] ReturnRectangularIntArray(int Size1, int Size2)
	{
		int[][] array = new int[Size1][];
		for (int i = 0; i < Size1; i++)
		{
			array[i] = new int[Size2];
		}
		return array;
	}

	internal string[][] ReturnRectangularStringArray(int Size1, int Size2)
	{
		string[][] array = new string[Size1][];
		for (int i = 0; i < Size1; i++)
		{
			array[i] = new string[Size2];
		}
		return array;
	}

	internal float[][] ReturnRectangularFloatArray(int Size1, int Size2)
	{
		float[][] array = new float[Size1][];
		for (int i = 0; i < Size1; i++)
		{
			array[i] = new float[Size2];
		}
		return array;
	}

	internal short[][] ReturnRectangularShortArray(int Size1, int Size2)
	{
		short[][] array = new short[Size1][];
		for (int i = 0; i < Size1; i++)
		{
			array[i] = new short[Size2];
		}
		return array;
	}

	internal byte[][] ReturnRectangularSbyteArray(int Size1, int Size2)
	{
		byte[][] array = new byte[Size1][];
		for (int i = 0; i < Size1; i++)
		{
			array[i] = new byte[Size2];
		}
		return array;
	}
}
