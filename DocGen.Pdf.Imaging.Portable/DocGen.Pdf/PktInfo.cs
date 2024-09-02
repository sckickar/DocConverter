namespace DocGen.Pdf;

internal class PktInfo
{
	public int packetIdx;

	public int layerIdx;

	public int cbOff;

	public int cbLength;

	public int[] segLengths;

	public int numTruncPnts;

	public PktInfo(int lyIdx, int pckIdx)
	{
		layerIdx = lyIdx;
		packetIdx = pckIdx;
	}

	public override string ToString()
	{
		return "packet " + packetIdx + " (lay:" + layerIdx + ", off:" + cbOff + ", len:" + cbLength + ", numTruncPnts:" + numTruncPnts + ")\n";
	}
}
