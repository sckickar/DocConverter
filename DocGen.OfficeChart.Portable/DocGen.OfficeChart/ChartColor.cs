using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.XmlReaders;

namespace DocGen.OfficeChart;

public class ChartColor : IDisposable
{
	public delegate void AfterChangeHandler();

	private ColorType m_colorType;

	private int m_color;

	private double m_tintAndShade;

	private double m_satMod;

	private double m_lumOff;

	private double m_sat;

	private double m_lumMod;

	private bool m_bIsSchemeColor;

	private string m_schemaName;

	private string m_hexColor;

	public int Value => m_color;

	public double Tint
	{
		get
		{
			return m_tintAndShade;
		}
		set
		{
			m_tintAndShade = value;
		}
	}

	internal double Saturation
	{
		get
		{
			return m_satMod;
		}
		set
		{
			m_satMod = value;
		}
	}

	internal double Luminance
	{
		get
		{
			return m_lumMod;
		}
		set
		{
			m_lumMod = value;
		}
	}

	internal double LuminanceOffSet
	{
		get
		{
			return m_lumOff;
		}
		set
		{
			m_lumOff = value;
		}
	}

	internal bool IsSchemeColor
	{
		get
		{
			return m_bIsSchemeColor;
		}
		set
		{
			m_bIsSchemeColor = value;
		}
	}

	internal string SchemaName
	{
		get
		{
			return m_schemaName;
		}
		set
		{
			m_schemaName = value;
		}
	}

	internal string HexColor
	{
		get
		{
			return m_hexColor;
		}
		set
		{
			m_hexColor = value;
		}
	}

	public ColorType ColorType
	{
		get
		{
			return m_colorType;
		}
		set
		{
			m_colorType = value;
		}
	}

	public event AfterChangeHandler AfterChange;

	public ChartColor(Color color)
		: this(ColorType.RGB, color.ToArgb())
	{
	}

	public ChartColor(OfficeKnownColors color)
		: this(ColorType.Indexed, (int)color)
	{
	}

	public ChartColor(ColorType colorType, int colorValue)
		: this(colorType, colorValue, 0.0)
	{
	}

	public ChartColor(ColorType colorType, int colorValue, double tint)
	{
		m_colorType = colorType;
		m_color = colorValue;
		m_tintAndShade = tint;
	}

	internal OfficeKnownColors GetIndexed(IWorkbook book)
	{
		if (m_colorType == ColorType.Indexed)
		{
			return (OfficeKnownColors)m_color;
		}
		return (book as WorkbookImpl).GetNearestColor(GetRGB(book), 8);
	}

	public void SetIndexed(OfficeKnownColors value)
	{
		SetIndexed(value, raiseEvent: true);
	}

	public void SetIndexed(OfficeKnownColors value, bool raiseEvent)
	{
		if (m_colorType != ColorType.Indexed || m_color != (int)value)
		{
			m_colorType = ColorType.Indexed;
			m_color = (int)value;
			Normalize(raiseEvent: false);
			m_tintAndShade = 0.0;
			if (raiseEvent && this.AfterChange != null)
			{
				this.AfterChange();
			}
		}
	}

	internal void SetIndexed(OfficeKnownColors value, bool raiseEvent, WorkbookImpl book)
	{
		if (m_colorType != ColorType.Indexed || m_color != (int)value)
		{
			m_colorType = ColorType.Indexed;
			m_color = (int)value;
			if (!book.IsEqualColor)
			{
				Normalize(raiseEvent: false, book);
			}
			m_tintAndShade = 0.0;
			if (raiseEvent && this.AfterChange != null)
			{
				this.AfterChange();
			}
		}
	}

	internal Color GetRGB(IWorkbook book)
	{
		Color color = m_colorType switch
		{
			ColorType.RGB => ColorExtension.FromArgb(m_color), 
			ColorType.Indexed => book.GetPaletteColor((OfficeKnownColors)m_color), 
			ColorType.Theme => (book as WorkbookImpl).GetThemeColor(m_color), 
			_ => throw new InvalidOperationException(), 
		};
		if (m_tintAndShade != 0.0)
		{
			int num = 0;
			int num2 = 0;
			int num3 = WorkbookImpl.ThemeColorPalette.Length;
			int num4 = WorkbookImpl.DefaultTints.Length;
			double[] defaultTints = WorkbookImpl.DefaultTints;
			for (int i = 0; i < defaultTints.Length && defaultTints[i] != m_tintAndShade; i++)
			{
				num++;
			}
			Color[] defaultThemeColors = WorkbookImpl.DefaultThemeColors;
			for (int i = 0; i < defaultThemeColors.Length; i++)
			{
				Color color2 = defaultThemeColors[i];
				if (color2.Equals(color))
				{
					break;
				}
				num2++;
			}
			if (num2 < num3 && num < num4 && WorkbookImpl.ThemeColorPalette[num2].Length > num)
			{
				color = WorkbookImpl.ThemeColorPalette[num2][num];
			}
			else
			{
				double num5 = m_tintAndShade;
				if (num5 > 100.0)
				{
					num5 = m_tintAndShade / 100000.0;
				}
				color = Excel2007Parser.ConvertColorByTint(color, num5);
			}
		}
		return color;
	}

	internal void SetRGB(Color value, IWorkbook book)
	{
		SetRGB(value, book, 0.0);
	}

	internal void SetRGB(Color value)
	{
		int num = value.ToArgb();
		int num2 = Color.Black.ToArgb();
		if (m_colorType != ColorType.RGB || m_color != num || m_color == num2 || m_color == 0)
		{
			m_colorType = ColorType.RGB;
			m_color = num;
			m_tintAndShade = 0.0;
			if (this.AfterChange != null)
			{
				this.AfterChange();
			}
		}
	}

	public static implicit operator ChartColor(Color color)
	{
		return new ChartColor(color);
	}

	public static bool operator ==(ChartColor first, ChartColor second)
	{
		if ((object)first == null && (object)second == null)
		{
			return true;
		}
		if (((object)first == null && (object)second != null) || ((object)first != null && (object)second == null))
		{
			return false;
		}
		if (first.m_colorType == second.m_colorType && first.m_color == second.m_color)
		{
			return first.m_tintAndShade == second.m_tintAndShade;
		}
		return false;
	}

	public static bool operator !=(ChartColor first, ChartColor second)
	{
		if ((object)first == null && (object)second == null)
		{
			return false;
		}
		if (((object)first == null && (object)second != null) || ((object)first != null && (object)second == null))
		{
			return true;
		}
		if (first.m_colorType == second.m_colorType && first.m_color == second.m_color)
		{
			return first.m_tintAndShade != second.m_tintAndShade;
		}
		return true;
	}

	internal void CopyFrom(ChartColor colorObject, bool callEvent)
	{
		if (colorObject == null)
		{
			throw new ArgumentNullException("colorObject");
		}
		m_color = colorObject.m_color;
		m_colorType = colorObject.m_colorType;
		m_tintAndShade = colorObject.m_tintAndShade;
		if (callEvent && this.AfterChange != null)
		{
			this.AfterChange();
		}
	}

	internal void ConvertToIndexed(IWorkbook book)
	{
		if (m_colorType != ColorType.Indexed)
		{
			SetIndexed(GetIndexed(book));
		}
	}

	public override int GetHashCode()
	{
		return m_color.GetHashCode() ^ m_colorType.GetHashCode();
	}

	public void SetIndexedNoEvent(OfficeKnownColors value)
	{
		m_colorType = ColorType.Indexed;
		m_color = (int)value;
	}

	internal ChartColor Clone()
	{
		return (ChartColor)MemberwiseClone();
	}

	internal void Normalize()
	{
		Normalize(raiseEvent: true);
	}

	internal void Normalize(bool raiseEvent)
	{
		if (m_colorType == ColorType.Indexed)
		{
			int color = m_color;
			if (m_color == 0)
			{
				m_color += 64;
			}
			else if (m_color < 8)
			{
				m_color += 8;
			}
			if (color != m_color)
			{
				SetIndexed((OfficeKnownColors)m_color, raiseEvent);
			}
		}
	}

	internal void Normalize(bool raiseEvent, WorkbookImpl book)
	{
		if (m_colorType == ColorType.Indexed)
		{
			int color = m_color;
			if (m_color == 0)
			{
				m_color += 64;
			}
			if (color != m_color)
			{
				SetIndexed((OfficeKnownColors)m_color, raiseEvent);
			}
		}
	}

	public override bool Equals(object obj)
	{
		ChartColor chartColor = obj as ChartColor;
		if (obj == null)
		{
			return false;
		}
		return chartColor == this;
	}

	internal void SetTheme(int themeIndex, IWorkbook book)
	{
		SetTheme(themeIndex, book, 0.0);
	}

	internal void SetTheme(int themeIndex, IWorkbook book, double dTintValue)
	{
		if (m_colorType != ColorType.Theme || m_color != themeIndex)
		{
			m_colorType = ColorType.Theme;
			m_color = themeIndex;
			m_tintAndShade = dTintValue;
			if (this.AfterChange != null)
			{
				this.AfterChange();
			}
		}
	}

	internal void SetRGB(Color rgb, IWorkbook book, double dTintValue)
	{
		int num = rgb.ToArgb();
		int num2 = Color.Black.ToArgb();
		if (m_colorType != ColorType.RGB || m_color != num || m_color == num2)
		{
			m_colorType = ColorType.RGB;
			m_color = num;
			m_tintAndShade = 0.0;
			if (this.AfterChange != null)
			{
				this.AfterChange();
			}
		}
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}
}
