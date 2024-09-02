using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
[Biff(TBIFFRecord.WSBool)]
internal class WSBoolRecord : BiffRecordRaw
{
	[Flags]
	private enum OptionFlags : ushort
	{
		AutoBreaks = 1,
		Dialog = 0x10,
		ApplyStyles = 0x20,
		RowSumsBelow = 0x40,
		RowSumsRight = 0x80,
		FitToPage = 0x100,
		AlternateExpression = 0x4000,
		AlternateFormula = 0x8000
	}

	public const ushort DisplayGutsMask = 3072;

	public const int DisplayGutsStartBit = 10;

	private const int DEF_RECORD_SIZE = 2;

	[BiffRecordPos(0, 2)]
	private OptionFlags m_options = (OptionFlags)1217;

	public bool IsAutoBreaks
	{
		get
		{
			return (m_options & OptionFlags.AutoBreaks) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.AutoBreaks;
			}
			else
			{
				m_options &= ~OptionFlags.AutoBreaks;
			}
		}
	}

	public bool IsDialog
	{
		get
		{
			return (m_options & OptionFlags.Dialog) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Dialog;
			}
			else
			{
				m_options &= ~OptionFlags.Dialog;
			}
		}
	}

	public bool IsApplyStyles
	{
		get
		{
			return (m_options & OptionFlags.ApplyStyles) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.ApplyStyles;
			}
			else
			{
				m_options &= ~OptionFlags.ApplyStyles;
			}
		}
	}

	public bool IsRowSumsBelow
	{
		get
		{
			return (m_options & OptionFlags.RowSumsBelow) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.RowSumsBelow;
			}
			else
			{
				m_options &= ~OptionFlags.RowSumsBelow;
			}
		}
	}

	public bool IsRowSumsRight
	{
		get
		{
			return (m_options & OptionFlags.RowSumsRight) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.RowSumsRight;
			}
			else
			{
				m_options &= ~OptionFlags.RowSumsRight;
			}
		}
	}

	public bool IsFitToPage
	{
		get
		{
			return (m_options & OptionFlags.FitToPage) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.FitToPage;
			}
			else
			{
				m_options &= ~OptionFlags.FitToPage;
			}
		}
	}

	public ushort DisplayGuts
	{
		get
		{
			return (ushort)(BiffRecordRaw.GetUInt16BitsByMask((ushort)m_options, 3072) >> 10);
		}
		set
		{
			if (value > 3)
			{
				throw new ArgumentOutOfRangeException();
			}
			ushort destination = (ushort)m_options;
			BiffRecordRaw.SetUInt16BitsByMask(ref destination, 3072, (ushort)(value << 10));
			m_options = (OptionFlags)destination;
		}
	}

	public bool IsAlternateExpression
	{
		get
		{
			return (m_options & OptionFlags.AlternateExpression) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.AlternateExpression;
			}
			else
			{
				m_options &= ~OptionFlags.AlternateExpression;
			}
		}
	}

	public bool IsAlternateFormula
	{
		get
		{
			return (m_options & OptionFlags.AlternateFormula) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.AlternateFormula;
			}
			else
			{
				m_options &= ~OptionFlags.AlternateFormula;
			}
		}
	}

	public override int MinimumRecordSize => 2;

	public override int MaximumRecordSize => 2;

	public override int MaximumMemorySize => 2;

	public WSBoolRecord()
	{
	}

	public WSBoolRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WSBoolRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_options = (OptionFlags)provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = MinimumRecordSize;
		provider.WriteUInt16(iOffset, (ushort)m_options);
	}
}
