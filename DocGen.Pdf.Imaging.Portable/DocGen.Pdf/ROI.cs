namespace DocGen.Pdf;

internal class ROI
{
	public ImgReaderPGM maskPGM;

	public bool arbShape;

	public bool rect;

	public int comp;

	public int ulx;

	public int uly;

	public int w;

	public int h;

	public int x;

	public int y;

	public int r;

	public ROI(int comp, ImgReaderPGM maskPGM)
	{
		arbShape = true;
		rect = false;
		this.comp = comp;
		this.maskPGM = maskPGM;
	}

	public ROI(int comp, int ulx, int uly, int w, int h)
	{
		arbShape = false;
		this.comp = comp;
		this.ulx = ulx;
		this.uly = uly;
		this.w = w;
		this.h = h;
		rect = true;
	}

	public ROI(int comp, int x, int y, int rad)
	{
		arbShape = false;
		this.comp = comp;
		this.x = x;
		this.y = y;
		r = rad;
	}

	public override string ToString()
	{
		if (arbShape)
		{
			return "ROI with arbitrary shape, PGM file= " + maskPGM;
		}
		if (rect)
		{
			return "Rectangular ROI, comp=" + comp + " ulx=" + ulx + " uly=" + uly + " w=" + w + " h=" + h;
		}
		return "Circular ROI,  comp=" + comp + " x=" + x + " y=" + y + " radius=" + r;
	}
}
