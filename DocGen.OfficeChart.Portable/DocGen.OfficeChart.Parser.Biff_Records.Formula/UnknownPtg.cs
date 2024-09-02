namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
internal class UnknownPtg : Ptg
{
	[Preserve]
	public UnknownPtg()
	{
	}

	[Preserve]
	public UnknownPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
		offset++;
	}

	public override int GetSize(OfficeVersion version)
	{
		return 1;
	}

	public override string ToString()
	{
		return "( not implemented or UNKNOWN " + TokenCode.ToString() + ")";
	}
}
