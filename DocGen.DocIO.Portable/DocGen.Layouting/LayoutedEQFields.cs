using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LayoutedEQFields
{
	internal enum EQSwitchType
	{
		Array = 1,
		Bracket,
		Displace,
		Fraction,
		Integral,
		List,
		Overstrike,
		Radical,
		Superscript,
		Subscript,
		Box
	}

	private List<LayoutedEQFields> m_childEQFileds;

	private RectangleF m_bounds;

	private EQSwitchType m_switchType;

	private StringAlignment m_alignment;

	internal RectangleF Bounds
	{
		get
		{
			_ = m_bounds;
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	internal List<LayoutedEQFields> ChildEQFileds
	{
		get
		{
			if (m_childEQFileds == null)
			{
				m_childEQFileds = new List<LayoutedEQFields>();
			}
			return m_childEQFileds;
		}
		set
		{
			m_childEQFileds = value;
		}
	}

	internal EQSwitchType SwitchType
	{
		get
		{
			return m_switchType;
		}
		set
		{
			m_switchType = value;
		}
	}

	internal StringAlignment Alignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			m_alignment = value;
		}
	}
}
