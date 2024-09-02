using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAttachedLabel)]
[CLSCompliant(false)]
internal class ChartAttachedLabelRecord : BiffRecordRaw
{
	[Flags]
	private enum OptionFlags
	{
		None = 0,
		ActiveValue = 1,
		PiePercents = 2,
		PieCategoryLabel = 4,
		SmoothLine = 8,
		CategoryLabel = 0x10,
		Bubble = 0x20
	}

	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private OptionFlags m_options;

	public ushort Options => (ushort)m_options;

	public bool ShowActiveValue
	{
		get
		{
			return (m_options & OptionFlags.ActiveValue) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.ActiveValue;
			}
			else
			{
				m_options &= ~OptionFlags.ActiveValue;
			}
		}
	}

	public bool ShowPieInPercents
	{
		get
		{
			return (m_options & OptionFlags.PiePercents) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.PiePercents;
			}
			else
			{
				m_options &= ~OptionFlags.PiePercents;
			}
		}
	}

	public bool ShowPieCategoryLabel
	{
		get
		{
			return (m_options & OptionFlags.PieCategoryLabel) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.PieCategoryLabel;
			}
			else
			{
				m_options &= ~OptionFlags.PieCategoryLabel;
			}
		}
	}

	public bool SmoothLine
	{
		get
		{
			return (m_options & OptionFlags.SmoothLine) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.SmoothLine;
			}
			else
			{
				m_options &= ~OptionFlags.SmoothLine;
			}
		}
	}

	public bool ShowCategoryLabel
	{
		get
		{
			return (m_options & OptionFlags.CategoryLabel) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.CategoryLabel;
			}
			else
			{
				m_options &= ~OptionFlags.CategoryLabel;
			}
		}
	}

	public bool ShowBubble
	{
		get
		{
			return (m_options & OptionFlags.Bubble) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Bubble;
			}
			else
			{
				m_options &= ~OptionFlags.Bubble;
			}
		}
	}

	public ChartAttachedLabelRecord()
	{
	}

	public ChartAttachedLabelRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAttachedLabelRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_options = (OptionFlags)provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, (ushort)m_options);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2;
	}
}
