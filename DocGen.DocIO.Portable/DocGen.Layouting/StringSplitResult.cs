using DocGen.Drawing;

namespace DocGen.Layouting;

internal class StringSplitResult
{
	public TextLineInfo[] Lines;

	public string Remainder;

	public SizeF ActualSize;

	public float LineHeight;

	public bool Empty
	{
		get
		{
			if (Lines != null)
			{
				return Lines.Length == 0;
			}
			return true;
		}
	}

	public int Count
	{
		get
		{
			if (Empty)
			{
				return 0;
			}
			return Lines.Length;
		}
	}
}
