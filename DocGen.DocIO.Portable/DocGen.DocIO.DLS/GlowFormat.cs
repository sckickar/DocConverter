using System.Text;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class GlowFormat
{
	private Color m_color;

	private float m_radius;

	private float m_transparency;

	private ShapeBase m_shape;

	private byte m_flags;

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

	internal float Radius
	{
		get
		{
			return m_radius;
		}
		set
		{
			m_radius = value;
		}
	}

	internal float Transparency
	{
		get
		{
			return m_transparency;
		}
		set
		{
			m_transparency = value;
		}
	}

	internal bool IsInlineColor
	{
		get
		{
			return (m_flags & 1) != 0;
		}
		set
		{
			m_flags = (byte)((m_flags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsInlineRadius
	{
		get
		{
			return (m_flags & 2) >> 1 != 0;
		}
		set
		{
			m_flags = (byte)((m_flags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsInlineTransparency
	{
		get
		{
			return (m_flags & 4) >> 2 != 0;
		}
		set
		{
			m_flags = (byte)((m_flags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal GlowFormat(ShapeBase shape)
	{
		m_shape = shape;
	}

	internal bool Compare(GlowFormat glowFormat)
	{
		if (IsInlineColor != glowFormat.IsInlineColor || IsInlineRadius != glowFormat.IsInlineRadius || IsInlineTransparency != glowFormat.IsInlineTransparency || Radius != glowFormat.Radius || Transparency != glowFormat.Transparency || Color.ToArgb() != glowFormat.Color.ToArgb())
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(IsInlineColor ? "1" : "0;");
		stringBuilder.Append(IsInlineRadius ? "1" : "0;");
		stringBuilder.Append(IsInlineTransparency ? "1" : "0;");
		stringBuilder.Append(Radius + ";");
		stringBuilder.Append(Transparency + ";");
		stringBuilder.Append(Color.ToArgb() + ";");
		return stringBuilder;
	}

	internal void Close()
	{
		if (m_shape != null)
		{
			m_shape = null;
		}
	}

	internal GlowFormat Clone()
	{
		return (GlowFormat)MemberwiseClone();
	}
}
