namespace DocGen.OfficeChart.Implementation.Charts;

internal class HistogramAxisFormat
{
	private byte m_flagOptions;

	private int m_numberOfBins;

	private double m_binWidth;

	private double m_overflowBinValue;

	private double m_underflowBinValue;

	private bool m_isNotAutomaticUnderFlowValue;

	internal bool HasAutomaticBins
	{
		get
		{
			if ((m_flagOptions & 0xF) == 0)
			{
				return true;
			}
			return (m_flagOptions & 1) == 1;
		}
		set
		{
			if (value)
			{
				ResetValues(0);
			}
			else
			{
				m_flagOptions &= 254;
			}
		}
	}

	internal bool IsBinningByCategory
	{
		get
		{
			return (m_flagOptions & 2) == 2;
		}
		set
		{
			if (value)
			{
				ResetValues(1);
			}
			else
			{
				m_flagOptions &= 253;
			}
		}
	}

	internal double BinWidth
	{
		get
		{
			return m_binWidth;
		}
		set
		{
			if (value > 0.0)
			{
				ResetValues(2);
				m_binWidth = value;
			}
		}
	}

	internal int NumberOfBins
	{
		get
		{
			return m_numberOfBins;
		}
		set
		{
			if (value > 0)
			{
				ResetValues(3);
				m_numberOfBins = value;
			}
		}
	}

	internal double OverflowBinValue
	{
		get
		{
			return m_overflowBinValue;
		}
		set
		{
			m_flagOptions |= 16;
			m_overflowBinValue = value;
			IsAutomaticFlowValue = false;
		}
	}

	internal double UnderflowBinValue
	{
		get
		{
			return m_underflowBinValue;
		}
		set
		{
			m_flagOptions |= 32;
			m_underflowBinValue = value;
			IsAutomaticFlowValue = false;
		}
	}

	internal bool IsIntervalClosedinLeft
	{
		get
		{
			return (m_flagOptions & 0x40) == 64;
		}
		set
		{
			if (value)
			{
				m_flagOptions |= 64;
			}
			else
			{
				m_flagOptions &= 191;
			}
		}
	}

	internal byte FlagOptions => m_flagOptions;

	internal bool IsAutomaticFlowValue
	{
		get
		{
			return (m_flagOptions & 0x80) == 128;
		}
		set
		{
			if (value)
			{
				m_flagOptions |= 128;
			}
			else
			{
				m_flagOptions &= 127;
			}
		}
	}

	internal bool IsNotAutomaticUnderFlowValue
	{
		get
		{
			return m_isNotAutomaticUnderFlowValue;
		}
		set
		{
			m_isNotAutomaticUnderFlowValue = value;
		}
	}

	internal bool IsNotAutomaticOverFlowValue
	{
		get
		{
			return (m_flagOptions & 0x80) == 128;
		}
		set
		{
			if (value)
			{
				m_flagOptions |= 128;
			}
			else
			{
				m_flagOptions &= 127;
			}
		}
	}

	private void ResetValues(byte bitPosition)
	{
		m_flagOptions &= 240;
		m_flagOptions |= (byte)(1 << (int)bitPosition);
		if (bitPosition != 2)
		{
			m_binWidth = 0.0;
		}
		else if (bitPosition != 3)
		{
			m_numberOfBins = 0;
		}
	}

	internal void Clone(HistogramAxisFormat inputFormat)
	{
		m_flagOptions = inputFormat.m_flagOptions;
		m_binWidth = inputFormat.m_binWidth;
		m_numberOfBins = inputFormat.m_numberOfBins;
		m_overflowBinValue = inputFormat.m_overflowBinValue;
		m_underflowBinValue = inputFormat.m_underflowBinValue;
	}

	public override bool Equals(object obj)
	{
		HistogramAxisFormat histogramAxisFormat = obj as HistogramAxisFormat;
		if (m_flagOptions == histogramAxisFormat.m_flagOptions && m_binWidth == histogramAxisFormat.m_binWidth && m_numberOfBins == histogramAxisFormat.m_numberOfBins && m_overflowBinValue == histogramAxisFormat.m_overflowBinValue)
		{
			return m_underflowBinValue == histogramAxisFormat.m_underflowBinValue;
		}
		return false;
	}
}
