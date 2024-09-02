using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartLegendItemStyle : ICloneable
{
	internal enum ChartLegendStyleKeys
	{
		Font,
		ImageIndex,
		ImageList,
		Interior,
		RepresentationSize,
		ShowSymbol,
		Spacing,
		Symbol,
		Border,
		TextColor,
		Type,
		ShowIcon,
		IconAlignment,
		TextAlignment,
		VisibleCheckBox,
		ShowShadow,
		ShadowOffset,
		ShadowColor,
		BorderColor,
		Url
	}

	private static ChartLegendItemStyle s_default = CreateDefault();

	private string m_url = string.Empty;

	private Hashtable m_store;

	private ChartLegendItemStyle m_parensStyle = s_default;

	private bool m_isStyleChanges;

	protected ChartSymbolInfo m_Symbol = CreateDefaultSymbol();

	public string Url
	{
		get
		{
			m_url = (string)GetObject(ChartLegendStyleKeys.Url);
			if (m_url == null)
			{
				return m_url;
			}
			if (m_url.StartsWith("www."))
			{
				return m_url = m_url.Insert(0, "http://");
			}
			return m_url;
		}
		set
		{
			SetObject(ChartLegendStyleKeys.Url, value);
		}
	}

	public static ChartLegendItemStyle Default => s_default;

	public bool IsEmpty => m_store.Count == 0;

	internal bool IsStyleChanged
	{
		get
		{
			return m_isStyleChanges;
		}
		set
		{
			m_isStyleChanges = value;
		}
	}

	public ChartLegendItemStyle BaseStyle
	{
		get
		{
			return m_parensStyle;
		}
		set
		{
			m_parensStyle = value;
		}
	}

	public Font Font
	{
		get
		{
			return GetObject(ChartLegendStyleKeys.Font) as Font;
		}
		set
		{
			SetObject(ChartLegendStyleKeys.Font, value);
		}
	}

	public int ImageIndex
	{
		get
		{
			return (int)GetObject(ChartLegendStyleKeys.ImageIndex);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ImageIndex, value);
		}
	}

	public ChartImageCollection ImageList
	{
		get
		{
			return GetObject(ChartLegendStyleKeys.ImageList) as ChartImageCollection;
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ImageList, value);
		}
	}

	public BrushInfo Interior
	{
		get
		{
			return GetObject(ChartLegendStyleKeys.Interior) as BrushInfo;
		}
		set
		{
			SetObject(ChartLegendStyleKeys.Interior, value);
		}
	}

	public Size RepresentationSize
	{
		get
		{
			return (Size)GetObject(ChartLegendStyleKeys.RepresentationSize);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.RepresentationSize, value);
		}
	}

	public bool ShowSymbol
	{
		get
		{
			return (bool)GetObject(ChartLegendStyleKeys.ShowSymbol);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ShowSymbol, value);
		}
	}

	public int Spacing
	{
		get
		{
			return (int)GetObject(ChartLegendStyleKeys.Spacing);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.Spacing, value);
		}
	}

	public ChartSymbolInfo Symbol
	{
		get
		{
			return m_Symbol;
		}
		set
		{
			if (m_Symbol != value)
			{
				m_Symbol = value;
			}
		}
	}

	public ChartLineInfo Border
	{
		get
		{
			return (ChartLineInfo)GetObject(ChartLegendStyleKeys.Border);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.Border, value);
		}
	}

	public Color TextColor
	{
		get
		{
			return (Color)GetObject(ChartLegendStyleKeys.TextColor);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.TextColor, value);
		}
	}

	public Color BorderColor
	{
		get
		{
			return (Color)GetObject(ChartLegendStyleKeys.BorderColor);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.BorderColor, value);
		}
	}

	public ChartLegendItemType Type
	{
		get
		{
			return (ChartLegendItemType)GetObject(ChartLegendStyleKeys.Type);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.Type, value);
		}
	}

	public bool ShowIcon
	{
		get
		{
			return (bool)GetObject(ChartLegendStyleKeys.ShowIcon);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ShowIcon, value);
		}
	}

	public LeftRightAlignment IconAlignment
	{
		get
		{
			return (LeftRightAlignment)GetObject(ChartLegendStyleKeys.IconAlignment);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.IconAlignment, value);
		}
	}

	public VerticalAlignment TextAlignment
	{
		get
		{
			return (VerticalAlignment)GetObject(ChartLegendStyleKeys.TextAlignment);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.TextAlignment, value);
		}
	}

	public bool VisibleCheckBox
	{
		get
		{
			return (bool)GetObject(ChartLegendStyleKeys.VisibleCheckBox);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.VisibleCheckBox, value);
		}
	}

	public bool ShowShadow
	{
		get
		{
			return (bool)GetObject(ChartLegendStyleKeys.ShowShadow);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ShowShadow, value);
		}
	}

	public Size ShadowOffset
	{
		get
		{
			return (Size)GetObject(ChartLegendStyleKeys.ShadowOffset);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ShadowOffset, value);
		}
	}

	public Color ShadowColor
	{
		get
		{
			return (Color)GetObject(ChartLegendStyleKeys.ShadowColor);
		}
		set
		{
			SetObject(ChartLegendStyleKeys.ShadowColor, value);
		}
	}

	public ChartLegendItemStyle()
	{
		m_store = new Hashtable();
	}

	private ChartLegendItemStyle(Hashtable store)
	{
		m_store = store;
	}

	public object Clone()
	{
		return new ChartLegendItemStyle(m_store.Clone() as Hashtable);
	}

	public void Reset(ChartLegendStyleKeys key)
	{
		m_store.Remove(key);
	}

	public void Clear()
	{
		m_store.Clear();
	}

	public static ChartSymbolInfo CreateDefaultSymbol()
	{
		return new ChartSymbolInfo
		{
			Color = SystemColors.HighlightText,
			HighlightColor = Color.Transparent,
			DimmedColor = Color.Transparent,
			ImageIndex = -1,
			Size = new Size(10, 10),
			Shape = ChartSymbolShape.None,
			Offset = new Size(0, 0),
			Marker = new ChartMarker(),
			Border = ChartLineInfo.CreateDefault()
		};
	}

	public static ChartLegendItemStyle CreateDefault()
	{
		return new ChartLegendItemStyle
		{
			BorderColor = Color.Black,
			Font = null,
			IconAlignment = LeftRightAlignment.Left,
			TextAlignment = VerticalAlignment.Center,
			TextColor = Color.Empty,
			ImageIndex = -1,
			ImageList = null,
			Interior = new BrushInfo(Color.White),
			RepresentationSize = new Size(20, 20),
			ShadowColor = Color.Gray,
			ShadowOffset = new Size(2, 2),
			ShowIcon = true,
			ShowSymbol = false,
			ShowShadow = false,
			Spacing = 0,
			Symbol = ChartSymbolInfo.Default,
			Border = ChartLineInfo.Default,
			Type = ChartLegendItemType.Rectangle,
			VisibleCheckBox = false,
			Url = string.Empty
		};
	}

	public void SetToLowerLevel(ChartLegendItemStyle style)
	{
		ChartLegendItemStyle chartLegendItemStyle = this;
		while (chartLegendItemStyle.BaseStyle != s_default && chartLegendItemStyle.BaseStyle != style)
		{
			chartLegendItemStyle = chartLegendItemStyle.BaseStyle;
		}
		chartLegendItemStyle.BaseStyle = style;
	}

	public void Dispose()
	{
		Border.Dispose();
		m_Symbol.Dispose();
		s_default = null;
		m_store = null;
		m_parensStyle = null;
		m_Symbol = null;
	}

	private void SetObject(object key, object value)
	{
		m_store[key] = value;
	}

	private object GetObject(object key)
	{
		object obj = null;
		if (m_store.ContainsKey(key))
		{
			return m_store[key];
		}
		if (m_parensStyle != null)
		{
			return m_parensStyle.GetObject(key);
		}
		if (s_default == null)
		{
			s_default = CreateDefault();
		}
		return s_default.GetObject(key);
	}
}
