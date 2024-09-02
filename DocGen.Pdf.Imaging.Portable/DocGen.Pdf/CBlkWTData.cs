namespace DocGen.Pdf;

internal abstract class CBlkWTData
{
	public int ulx;

	public int uly;

	public int n;

	public int m;

	internal SubbandAn sb;

	public int w;

	public int h;

	public int offset;

	public int scanw;

	public int magbits;

	public float wmseScaling = 1f;

	public double convertFactor = 1.0;

	public double stepSize = 1.0;

	public int nROIcoeff;

	public int nROIbp;

	public abstract int DataType { get; }

	public abstract object Data { get; set; }

	public override string ToString()
	{
		string text = "";
		switch (DataType)
		{
		case 0:
			text = "Unsigned Byte";
			break;
		case 1:
			text = "Short";
			break;
		case 3:
			text = "Integer";
			break;
		case 4:
			text = "Float";
			break;
		}
		return "ulx=" + ulx + ", uly=" + uly + ", idx=(" + m + "," + n + "), w=" + w + ", h=" + h + ", off=" + offset + ", scanw=" + scanw + ", wmseScaling=" + wmseScaling + ", convertFactor=" + convertFactor + ", stepSize=" + stepSize + ", type=" + text + ", magbits=" + magbits + ", nROIcoeff=" + nROIcoeff + ", nROIbp=" + nROIbp;
	}
}
