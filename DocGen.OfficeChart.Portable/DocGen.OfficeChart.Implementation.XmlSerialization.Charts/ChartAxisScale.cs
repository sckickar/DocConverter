using DocGen.OfficeChart.Implementation.Charts;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartAxisScale
{
	public bool? LogScale;

	public bool? Reversed;

	public double? MaximumValue;

	public double? MinimumValue;

	internal double? LogBase;

	public void CopyTo(IScalable axis)
	{
		if (LogScale.HasValue)
		{
			axis.IsLogScale = LogScale.Value;
		}
		if (Reversed.HasValue)
		{
			axis.ReversePlotOrder = Reversed.Value;
		}
		if (MaximumValue.HasValue)
		{
			axis.MaximumValue = MaximumValue.Value;
		}
		if (MinimumValue.HasValue)
		{
			axis.MinimumValue = MinimumValue.Value;
		}
		if (LogBase.HasValue)
		{
			axis.LogBase = LogBase.Value;
		}
	}
}
