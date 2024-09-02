using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class ShapeStyleReference
{
	private int m_styleRefIndex;

	private Color m_styleRefColor;

	private float m_styleRefOpacity;

	internal int StyleRefIndex
	{
		get
		{
			return m_styleRefIndex;
		}
		set
		{
			m_styleRefIndex = value;
		}
	}

	internal Color StyleRefColor
	{
		get
		{
			return m_styleRefColor;
		}
		set
		{
			m_styleRefColor = value;
		}
	}

	internal float StyleRefOpacity
	{
		get
		{
			return m_styleRefOpacity;
		}
		set
		{
			m_styleRefOpacity = value;
		}
	}

	internal bool Compare(ShapeStyleReference ShapeStyleReference)
	{
		if (StyleRefIndex != ShapeStyleReference.StyleRefIndex || StyleRefOpacity != ShapeStyleReference.StyleRefOpacity || StyleRefColor != ShapeStyleReference.StyleRefColor)
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StyleRefIndex + ";");
		stringBuilder.Append(StyleRefOpacity + ";");
		stringBuilder.Append(StyleRefColor.ToArgb() + ";");
		return stringBuilder;
	}
}
