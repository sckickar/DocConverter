using System;
using System.IO;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class GradientStopImpl
{
	internal const int Size = 12;

	private ChartColor m_color;

	private int m_iPosiiton;

	private int m_iTransparency;

	private int m_iTint = -1;

	private int m_iShade = -1;

	public ChartColor ColorObject => m_color;

	public int Position
	{
		get
		{
			return m_iPosiiton;
		}
		set
		{
			m_iPosiiton = value;
		}
	}

	public int Transparency
	{
		get
		{
			return m_iTransparency;
		}
		set
		{
			m_iTransparency = value;
		}
	}

	public int Tint
	{
		get
		{
			return m_iTint;
		}
		set
		{
			m_iTint = value;
		}
	}

	public int Shade
	{
		get
		{
			return m_iShade;
		}
		set
		{
			m_iShade = value;
		}
	}

	public GradientStopImpl(ChartColor color, int position, int transparency)
		: this(color, position, transparency, -1, -1)
	{
	}

	public GradientStopImpl(ChartColor color, int position, int transparency, int tint, int shade)
	{
		m_color = color;
		m_iPosiiton = position;
		m_iTransparency = transparency;
		m_iTint = tint;
		m_iShade = shade;
	}

	public GradientStopImpl(byte[] data, int offset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_iPosiiton = BitConverter.ToInt32(data, offset);
		offset += 4;
		int value = BitConverter.ToInt32(data, offset);
		m_color = new ChartColor(ColorExtension.FromArgb(value));
		offset += 4;
		m_iTransparency = BitConverter.ToInt32(data, offset);
	}

	internal void Serialize(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] bytes = BitConverter.GetBytes(m_iPosiiton);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_color.Value);
		stream.Write(bytes, 0, bytes.Length);
		bytes = BitConverter.GetBytes(m_iTransparency);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal GradientStopImpl Clone()
	{
		GradientStopImpl obj = (GradientStopImpl)MemberwiseClone();
		obj.m_color = new ChartColor(OfficeKnownColors.Black);
		obj.m_color.CopyFrom(m_color, callEvent: false);
		return obj;
	}

	internal bool EqualsWithoutTransparency(GradientStopImpl stop)
	{
		if (stop == null)
		{
			return false;
		}
		if (stop.m_color == m_color && stop.m_iPosiiton == m_iPosiiton && stop.m_iShade == m_iShade)
		{
			return stop.m_iTint == m_iTint;
		}
		return false;
	}

	internal void Dispose()
	{
		m_color.Dispose();
		m_color = null;
	}
}
