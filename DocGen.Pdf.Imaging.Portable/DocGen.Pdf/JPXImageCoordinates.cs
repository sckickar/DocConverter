namespace DocGen.Pdf;

internal class JPXImageCoordinates
{
	public int x;

	public int y;

	public JPXImageCoordinates()
	{
	}

	public JPXImageCoordinates(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public JPXImageCoordinates(JPXImageCoordinates c)
	{
		x = c.x;
		y = c.y;
	}

	public override string ToString()
	{
		return "(" + x + "," + y + ")";
	}
}
