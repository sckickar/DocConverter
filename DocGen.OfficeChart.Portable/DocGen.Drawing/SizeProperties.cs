using DocGen.OfficeChart;

namespace DocGen.Drawing;

internal class SizeProperties
{
	internal static float FULL_COLUMN_OFFSET = 1024f;

	internal static float FULL_ROW_OFFSET = 256f;

	private int m_left;

	private int m_top;

	private int m_bottom;

	private int m_right;

	private int m_leftColumn;

	private int m_rightColumn;

	private int m_topRow;

	private int m_bottomRow;

	private PlacementType m_placementType;

	internal int Bottom
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = value;
		}
	}

	internal int Left
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	internal int Right
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	internal int Top
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	internal SizeProperties()
	{
		m_placementType = PlacementType.MoveAndSize;
	}

	internal void SetBottomRow(int bottomRow)
	{
		m_bottomRow = bottomRow;
	}

	internal int GetRightColumn()
	{
		return m_rightColumn;
	}

	internal void SetRightColumn(int rightColumn)
	{
		m_rightColumn = rightColumn;
	}

	internal PlacementType GetPlacementType()
	{
		return m_placementType;
	}

	internal void SetPlacementType(PlacementType placementType)
	{
		if (m_placementType != placementType)
		{
			m_placementType = placementType;
		}
	}

	internal int GetTopRow()
	{
		return m_topRow;
	}

	internal void SetTopRow(int topRow)
	{
		m_topRow = topRow;
	}

	internal int GetLeftColumn()
	{
		return m_leftColumn;
	}

	internal void SetLeftColumn(int leftColumn)
	{
		m_leftColumn = leftColumn;
	}

	internal int GetBottomRow()
	{
		return m_bottomRow;
	}
}
