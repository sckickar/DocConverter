using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart;

internal interface IOfficeChartRichTextString : IParentApplication, IOptimizedUpdate
{
	string Text { get; }

	ChartAlrunsRecord.TRuns[] FormattingRuns { get; }

	void SetFont(int iStartPos, int iEndPos, IOfficeFont font);

	IOfficeFont GetFont(ChartAlrunsRecord.TRuns tRuns);
}
