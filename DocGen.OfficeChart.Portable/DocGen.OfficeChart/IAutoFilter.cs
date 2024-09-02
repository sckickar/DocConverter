namespace DocGen.OfficeChart;

internal interface IAutoFilter
{
	IAutoFilterCondition FirstCondition { get; }

	IAutoFilterCondition SecondCondition { get; }

	bool IsFiltered { get; }

	bool IsAnd { get; set; }

	bool IsPercent { get; }

	bool IsSimple1 { get; }

	bool IsSimple2 { get; }

	bool IsTop { get; set; }

	bool IsTop10 { get; set; }

	int Top10Number { get; set; }
}
