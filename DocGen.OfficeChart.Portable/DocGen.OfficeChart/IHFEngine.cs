namespace DocGen.OfficeChart;

internal interface IHFEngine : IRichTextString, IParentApplication, IOptimizedUpdate
{
	void Parse(string strText);

	string GetHeaderFooterString();
}
