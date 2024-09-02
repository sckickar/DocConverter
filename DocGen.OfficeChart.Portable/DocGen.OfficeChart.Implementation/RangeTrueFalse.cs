using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation;

internal class RangeTrueFalse
{
	private RangesOperations m_trueValues = new RangesOperations();

	private RangesOperations m_falseValues = new RangesOperations();

	public bool? GetRangeValue(ICombinedRange range)
	{
		Rectangle[] rectangles = range.GetRectangles();
		bool? result = null;
		if (m_trueValues.Contains(rectangles))
		{
			result = true;
		}
		else if (m_falseValues.Contains(rectangles))
		{
			result = false;
		}
		return result;
	}

	public void SetRange(ICombinedRange range, bool? value)
	{
		Rectangle[] rectangles = range.GetRectangles();
		if (!value.HasValue)
		{
			m_trueValues.Remove(rectangles);
			m_falseValues.Remove(rectangles);
		}
		else if (value == true)
		{
			m_trueValues.AddRectangles(rectangles);
			m_falseValues.Remove(rectangles);
		}
		else
		{
			m_trueValues.Remove(rectangles);
			m_falseValues.AddRectangles(rectangles);
		}
	}

	public void Clear()
	{
		m_trueValues.Clear();
		m_falseValues.Clear();
	}
}
