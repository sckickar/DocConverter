namespace DocGen.OfficeChart;

public interface IOfficeChartLayout : IParentApplication
{
	IOfficeChartManualLayout ManualLayout { get; set; }

	LayoutTargets LayoutTarget { get; set; }

	LayoutModes LeftMode { get; set; }

	LayoutModes TopMode { get; set; }

	double Left { get; set; }

	double Top { get; set; }

	LayoutModes WidthMode { get; set; }

	LayoutModes HeightMode { get; set; }

	double Width { get; set; }

	double Height { get; set; }
}
