namespace DocGen.OfficeChart;

public interface IOfficeChartTextArea : IParentApplication, IOfficeFont, IOptimizedUpdate
{
	string Text { get; set; }

	int TextRotationAngle { get; set; }

	IOfficeChartFrameFormat FrameFormat { get; }

	IOfficeChartLayout Layout { get; set; }
}
