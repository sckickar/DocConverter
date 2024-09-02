using System.Collections;

namespace DocGen.Chart.Renderers;

internal class AccumulationChartsLabelComparer : IComparer
{
	public int Compare(object x, object y)
	{
		int result = 0;
		AccumulationChartsLabel accumulationChartsLabel = x as AccumulationChartsLabel;
		AccumulationChartsLabel accumulationChartsLabel2 = y as AccumulationChartsLabel;
		if (accumulationChartsLabel != null && accumulationChartsLabel2 != null && accumulationChartsLabel.Value != accumulationChartsLabel2.Value)
		{
			result = ((accumulationChartsLabel.Value > accumulationChartsLabel2.Value) ? 1 : (-1));
		}
		return result;
	}
}
