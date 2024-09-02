namespace DocGen.OfficeChart.Interfaces.Charts;

internal interface IInternalOfficeChartTextArea : IOfficeChartTextArea, IParentApplication, IOfficeFont, IOptimizedUpdate, IInternalFont
{
	ChartColor ColorObject { get; }

	bool HasTextRotation { get; }

	ChartParagraphType ParagraphType { get; set; }
}
