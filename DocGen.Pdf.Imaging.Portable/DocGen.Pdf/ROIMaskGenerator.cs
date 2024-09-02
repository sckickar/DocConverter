namespace DocGen.Pdf;

internal abstract class ROIMaskGenerator
{
	internal ROI[] roi_array;

	internal int nrc;

	internal bool[] tileMaskMade;

	internal bool roiInTile;

	internal virtual ROI[] ROIs => roi_array;

	internal ROIMaskGenerator(ROI[] rois, int nrc)
	{
		roi_array = rois;
		this.nrc = nrc;
		tileMaskMade = new bool[nrc];
	}

	internal abstract bool getROIMask(DataBlockInt db, Subband sb, int magbits, int c);

	internal abstract void makeMask(Subband sb, int magbits, int n);

	public virtual void tileChanged()
	{
		for (int i = 0; i < nrc; i++)
		{
			tileMaskMade[i] = false;
		}
	}
}
