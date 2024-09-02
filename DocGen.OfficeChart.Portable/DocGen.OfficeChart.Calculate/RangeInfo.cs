namespace DocGen.OfficeChart.Calculate;

internal class RangeInfo
{
	private int top;

	private int left;

	private int right;

	private int bottom;

	public int Bottom
	{
		get
		{
			return bottom;
		}
		set
		{
			bottom = value;
		}
	}

	public int Left
	{
		get
		{
			return left;
		}
		set
		{
			left = value;
		}
	}

	public int Right
	{
		get
		{
			return right;
		}
		set
		{
			right = value;
		}
	}

	public int Top
	{
		get
		{
			return top;
		}
		set
		{
			top = value;
		}
	}

	public RangeInfo(int top, int left, int bottom, int right)
	{
		this.top = top;
		this.bottom = bottom;
		this.left = left;
		this.right = right;
	}

	public static RangeInfo Cells(int top, int left, int bottom, int right)
	{
		return new RangeInfo(top, left, bottom, right);
	}

	public static string GetAlphaLabel(int col)
	{
		char[] array = new char[10];
		int num = 0;
		while (col > 0 && num < 9)
		{
			col--;
			array[num] = (char)(col % 26 + 65);
			col /= 26;
			num++;
		}
		char[] array2 = new char[num];
		for (int i = 0; i < num; i++)
		{
			array2[num - i - 1] = array[i];
		}
		return new string(array2);
	}
}
