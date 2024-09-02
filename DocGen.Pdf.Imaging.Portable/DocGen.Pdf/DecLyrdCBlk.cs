namespace DocGen.Pdf;

internal class DecLyrdCBlk : CodedCBlk
{
	public int ulx;

	public int uly;

	public int w;

	public int h;

	public int dl;

	public bool prog;

	public int nl;

	public int ftpIdx;

	public int nTrunc;

	public int[] tsLengths;

	public override string ToString()
	{
		string text = "Coded code-block (" + m + "," + n + "): " + skipMSBP + " MSB skipped, " + dl + " bytes, " + nTrunc + " truncation points, " + nl + " layers, progressive=" + prog + ", ulx=" + ulx + ", uly=" + uly + ", w=" + w + ", h=" + h + ", ftpIdx=" + ftpIdx;
		if (tsLengths != null)
		{
			text += " {";
			for (int i = 0; i < tsLengths.Length; i++)
			{
				text = text + " " + tsLengths[i];
			}
			text += " }";
		}
		return text;
	}
}
