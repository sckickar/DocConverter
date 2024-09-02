namespace DocGen.Pdf;

internal abstract class TrueTypeTableBase : TableBase
{
	internal abstract uint Tag { get; }

	public TrueTypeTableBase(FontFile2 fontSource)
		: base(fontSource)
	{
	}
}
