using System.Collections;
using System.Collections.Generic;
using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class GradientStop
{
	private byte m_position = byte.MaxValue;

	private Color m_color;

	private byte m_opacity = byte.MaxValue;

	private List<DictionaryEntry> m_fillSchemeColor;

	internal byte Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	internal byte Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			m_opacity = value;
		}
	}

	internal List<DictionaryEntry> FillSchemeColorTransforms
	{
		get
		{
			if (m_fillSchemeColor == null)
			{
				m_fillSchemeColor = new List<DictionaryEntry>();
			}
			return m_fillSchemeColor;
		}
		set
		{
			m_fillSchemeColor = value;
		}
	}

	internal GradientStop()
	{
	}

	internal GradientStop Clone()
	{
		return (GradientStop)MemberwiseClone();
	}

	internal bool Compare(GradientStop gradientStop)
	{
		if (Position != gradientStop.Position || Opacity != gradientStop.Opacity || Color.ToArgb() != gradientStop.Color.ToArgb())
		{
			return false;
		}
		if ((FillSchemeColorTransforms != null && gradientStop.FillSchemeColorTransforms == null) || (FillSchemeColorTransforms == null && gradientStop.FillSchemeColorTransforms != null))
		{
			return false;
		}
		if (FillSchemeColorTransforms != null && gradientStop.FillSchemeColorTransforms != null)
		{
			if (FillSchemeColorTransforms.Count != gradientStop.FillSchemeColorTransforms.Count)
			{
				return false;
			}
			for (int i = 0; i < FillSchemeColorTransforms.Count; i++)
			{
				if (FillSchemeColorTransforms[i].Key != gradientStop.FillSchemeColorTransforms[i].Key || FillSchemeColorTransforms[i].Value != gradientStop.FillSchemeColorTransforms[i].Value)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Position + ";");
		stringBuilder.Append(Opacity + ";");
		stringBuilder.Append(Color.ToArgb() + ";");
		if (FillSchemeColorTransforms != null)
		{
			foreach (DictionaryEntry fillSchemeColorTransform in FillSchemeColorTransforms)
			{
				stringBuilder.Append(fillSchemeColorTransform.Key.ToString() + ";");
				stringBuilder.Append(fillSchemeColorTransform.Value.ToString() + ";");
			}
		}
		return stringBuilder;
	}
}
