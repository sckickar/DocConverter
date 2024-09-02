namespace DocGen.Pdf;

internal class CBlkInfo
{
	public int ulx;

	public int uly;

	public int w;

	public int h;

	public int msbSkipped;

	public int[] len;

	public int[] off;

	public int[] ntp;

	public int ctp;

	public int[][] segLen;

	public int[] pktIdx;

	public CBlkInfo(int ulx, int uly, int w, int h, int nl)
	{
		this.ulx = ulx;
		this.uly = uly;
		this.w = w;
		this.h = h;
		off = new int[nl];
		len = new int[nl];
		ntp = new int[nl];
		segLen = new int[nl][];
		pktIdx = new int[nl];
		for (int num = nl - 1; num >= 0; num--)
		{
			pktIdx[num] = -1;
		}
	}

	public virtual void addNTP(int l, int newtp)
	{
		ntp[l] = newtp;
		ctp = 0;
		for (int i = 0; i <= l; i++)
		{
			ctp += ntp[i];
		}
	}

	public override string ToString()
	{
		string text = "(ulx,uly,w,h)= (" + ulx + "," + uly + "," + w + "," + h;
		text = text + ") " + msbSkipped + " MSB bit(s) skipped\n";
		if (len != null)
		{
			for (int i = 0; i < len.Length; i++)
			{
				text = text + "\tl:" + i + ", start:" + off[i] + ", len:" + len[i] + ", ntp:" + ntp[i] + ", pktIdx=" + pktIdx[i];
				if (segLen != null && segLen[i] != null)
				{
					text += " { ";
					for (int j = 0; j < segLen[i].Length; j++)
					{
						text = text + segLen[i][j] + " ";
					}
					text += "}";
				}
				text += "\n";
			}
		}
		return text + "\tctp=" + ctp;
	}
}
