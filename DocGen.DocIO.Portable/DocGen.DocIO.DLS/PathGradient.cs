using System.Text;

namespace DocGen.DocIO.DLS;

internal class PathGradient
{
	private GradientShadeType m_pathShade;

	private short m_bottomOffset;

	private short m_leftOffset;

	private short m_rightOffset;

	private short m_topOffset;

	internal GradientShadeType PathShade
	{
		get
		{
			return m_pathShade;
		}
		set
		{
			m_pathShade = value;
		}
	}

	internal short BottomOffset
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

	internal short LeftOffset
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

	internal short RightOffset
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

	internal short TopOffset
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

	internal PathGradient()
	{
	}

	internal PathGradient Clone()
	{
		return (PathGradient)MemberwiseClone();
	}

	internal bool Compare(PathGradient pathGradient)
	{
		if (PathShade != pathGradient.PathShade || BottomOffset != pathGradient.BottomOffset || LeftOffset != pathGradient.LeftOffset || RightOffset != pathGradient.RightOffset || TopOffset != pathGradient.TopOffset)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append((int)PathShade + ";");
		stringBuilder.Append(BottomOffset + ";");
		stringBuilder.Append(LeftOffset + ";");
		stringBuilder.Append(RightOffset + ";");
		stringBuilder.Append(TopOffset + ";");
		return stringBuilder;
	}
}
