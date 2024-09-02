using System.Collections;

namespace DocGen.Chart;

internal class ChartAxisLabelByDoubleValueComparer : IComparer
{
	private bool m_inversed;

	public ChartAxisLabelByDoubleValueComparer(bool inversed)
	{
		m_inversed = inversed;
	}

	int IComparer.Compare(object x, object y)
	{
		ChartAxisLabel chartAxisLabel = (ChartAxisLabel)x;
		ChartAxisLabel chartAxisLabel2 = (ChartAxisLabel)y;
		if (chartAxisLabel.DoubleValue == chartAxisLabel2.DoubleValue)
		{
			return 0;
		}
		if (chartAxisLabel.DoubleValue < chartAxisLabel2.DoubleValue)
		{
			if (!m_inversed)
			{
				return -1;
			}
			return 1;
		}
		if (!m_inversed)
		{
			return 1;
		}
		return -1;
	}
}
