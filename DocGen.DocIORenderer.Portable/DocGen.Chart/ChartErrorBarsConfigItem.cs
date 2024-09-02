using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart;

namespace DocGen.Chart;

internal class ChartErrorBarsConfigItem : ChartConfigItem
{
	private bool m_enabled;

	private SizeF m_symbolSize = new SizeF(10f, 10f);

	private ChartSymbolShape m_symbolShape = ChartSymbolShape.Diamond;

	private ChartOrientation m_orientation = ChartOrientation.Vertical;

	internal List<double> MinusValues { get; set; }

	internal List<double> PlusValues { get; set; }

	internal double FixedValue { get; set; }

	internal double Mean { get; set; }

	internal OfficeErrorBarType ValueType { get; set; }

	internal OfficeErrorBarInclude Type { get; set; }

	internal Color Color { get; set; }

	internal float Width { get; set; }

	internal DashStyle DashStyle { get; set; }

	public SizeF SymbolSize
	{
		get
		{
			return m_symbolSize;
		}
		set
		{
			_ = m_symbolSize;
			m_symbolSize = value;
			RaisePropertyChanged("SymbolSize");
		}
	}

	public ChartSymbolShape SymbolShape
	{
		get
		{
			return m_symbolShape;
		}
		set
		{
			if (m_symbolShape != value)
			{
				m_symbolShape = value;
				RaisePropertyChanged("SymbolShape");
			}
		}
	}

	public ChartOrientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value)
			{
				m_orientation = value;
				RaisePropertyChanged("Orientation");
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			if (m_enabled != value)
			{
				m_enabled = value;
				RaisePropertyChanged("Enabled");
			}
		}
	}
}
