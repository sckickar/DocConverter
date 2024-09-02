using System.Text;

namespace DocGen.DocIO.DLS;

internal class TileRectangle
{
	private float m_bottomOffset;

	private float m_leftOffset;

	private float m_rightOffset;

	private float m_topOffset;

	private byte m_flag;

	internal float BottomOffset
	{
		get
		{
			return m_bottomOffset;
		}
		set
		{
			m_bottomOffset = value;
		}
	}

	internal float LeftOffset
	{
		get
		{
			return m_leftOffset;
		}
		set
		{
			m_leftOffset = value;
		}
	}

	internal float RightOffset
	{
		get
		{
			return m_rightOffset;
		}
		set
		{
			m_rightOffset = value;
		}
	}

	internal float TopOffset
	{
		get
		{
			return m_topOffset;
		}
		set
		{
			m_topOffset = value;
		}
	}

	internal bool HasAttributes
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag = (byte)((m_flag & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal TileRectangle()
	{
	}

	internal TileRectangle Clone()
	{
		return (TileRectangle)MemberwiseClone();
	}

	internal bool Compare(TileRectangle fillRectangle)
	{
		if (BottomOffset != fillRectangle.BottomOffset || LeftOffset != fillRectangle.LeftOffset || RightOffset != fillRectangle.RightOffset || TopOffset != fillRectangle.TopOffset)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(BottomOffset + ";");
		stringBuilder.Append(LeftOffset + ";");
		stringBuilder.Append(RightOffset + ";");
		stringBuilder.Append(TopOffset + ";");
		return stringBuilder;
	}
}
