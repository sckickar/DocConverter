namespace DocGen.Pdf;

internal class CodedCBlk
{
	public int n;

	public int m;

	public int skipMSBP;

	public byte[] data;

	public CodedCBlk()
	{
	}

	public CodedCBlk(int m, int n, int skipMSBP, byte[] data)
	{
		this.m = m;
		this.n = n;
		this.skipMSBP = skipMSBP;
		this.data = data;
	}

	public override string ToString()
	{
		return "m=" + m + ", n=" + n + ", skipMSBP=" + skipMSBP + ", data.length=" + ((data != null) ? (data.Length.ToString() ?? "") : "(null)");
	}
}
