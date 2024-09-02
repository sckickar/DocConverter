namespace DocGen.Chart;

internal interface INiceRangeMaker
{
	int DesiredIntervals { get; set; }

	ChartAxisRangePaddingType RangePaddingType { get; set; }

	bool PreferZero { get; set; }

	bool ForceZero { get; set; }

	MinMaxInfo MakeNiceRange(double min, double max, ChartAxisRangePaddingType rangePaddingType);
}
