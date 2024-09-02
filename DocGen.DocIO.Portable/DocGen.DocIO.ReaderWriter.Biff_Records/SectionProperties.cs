using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class SectionProperties
{
	private SectionPropertyException m_sepx;

	private ColumnArray m_columnsArray;

	private ColumnArray m_oldColumnsArray;

	private bool m_stickProperties = true;

	internal bool StickProperties
	{
		get
		{
			return m_stickProperties;
		}
		set
		{
			m_stickProperties = value;
		}
	}

	internal SinglePropertyModifierArray Sprms => m_sepx.Properties;

	internal short HeaderHeight
	{
		get
		{
			return Sprms.GetShort(45079, -1);
		}
		set
		{
			if (value != -1)
			{
				Sprms.SetShortValue(45079, value);
			}
			else
			{
				Sprms.SetShortValue(45079, 720);
			}
		}
	}

	internal short FooterHeight
	{
		get
		{
			return Sprms.GetShort(45080, -1);
		}
		set
		{
			if (value != -1)
			{
				Sprms.SetShortValue(45080, value);
			}
			else
			{
				Sprms.SetShortValue(45080, 720);
			}
		}
	}

	internal bool TitlePage
	{
		get
		{
			return Sprms.GetBoolean(12298, defValue: false);
		}
		set
		{
			if (value)
			{
				Sprms.SetBoolValue(12298, flag: true);
			}
		}
	}

	internal byte BreakCode
	{
		get
		{
			return Sprms.GetByte(12297, 2);
		}
		set
		{
			Sprms.SetByteValue(12297, value);
		}
	}

	internal byte TextDirection
	{
		get
		{
			return Sprms.GetByte(20531, 0);
		}
		set
		{
			Sprms.SetByteValue(20531, value);
		}
	}

	internal ColumnArray Columns => m_columnsArray;

	internal ColumnArray OldColumns => m_oldColumnsArray;

	internal short BottomMargin
	{
		get
		{
			return Sprms.GetShort(36900, -1);
		}
		set
		{
			if (value != -1)
			{
				Sprms.SetShortValue(36900, value);
			}
		}
	}

	internal short TopMargin
	{
		get
		{
			return Sprms.GetShort(36899, -1);
		}
		set
		{
			if (value != -1)
			{
				Sprms.SetShortValue(36899, value);
			}
		}
	}

	internal short LeftMargin
	{
		get
		{
			return Sprms.GetShort(45089, -1);
		}
		set
		{
			if (value != -1)
			{
				Sprms.SetShortValue(45089, value);
			}
		}
	}

	internal short RightMargin
	{
		get
		{
			return Sprms.GetShort(45090, -1);
		}
		set
		{
			if (value != -1)
			{
				Sprms.SetShortValue(45090, value);
			}
		}
	}

	internal byte Orientation
	{
		get
		{
			return Sprms.GetByte(12317, 0);
		}
		set
		{
			Sprms.SetByteValue(12317, value);
		}
	}

	internal ushort PageHeight
	{
		get
		{
			return Sprms.GetUShort(45088, 0);
		}
		set
		{
			if (value > 0)
			{
				Sprms.SetUShortValue(45088, value);
			}
		}
	}

	internal ushort PageWidth
	{
		get
		{
			return Sprms.GetUShort(45087, 0);
		}
		set
		{
			if (value > 0)
			{
				Sprms.SetUShortValue(45087, value);
			}
		}
	}

	internal ushort FirstPageTray
	{
		get
		{
			return Sprms.GetUShort(20487, 0);
		}
		set
		{
			Sprms.SetUShortValue(20487, value);
		}
	}

	internal ushort OtherPagesTray
	{
		get
		{
			return Sprms.GetUShort(20488, 0);
		}
		set
		{
			Sprms.SetUShortValue(20488, value);
		}
	}

	internal byte VerticalAlignment
	{
		get
		{
			return Sprms.GetByte(12314, 0);
		}
		set
		{
			Sprms.SetByteValue(12314, value);
		}
	}

	internal short Gutter
	{
		get
		{
			return Sprms.GetShort(45093, 0);
		}
		set
		{
			Sprms.SetShortValue(45093, value);
		}
	}

	internal bool Bidi
	{
		get
		{
			return Sprms.GetBoolean(12840, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(12840, value);
		}
	}

	internal byte PageNfc
	{
		get
		{
			return Sprms.GetByte(12302, 0);
		}
		set
		{
			Sprms.SetByteValue(12302, value);
		}
	}

	internal ushort PageStartAt
	{
		get
		{
			return Sprms.GetUShort(20508, 0);
		}
		set
		{
			Sprms.SetUShortValue(20508, value);
		}
	}

	internal bool PageRestart
	{
		get
		{
			return Sprms.GetBoolean(12305, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(12305, value);
		}
	}

	internal ushort LinePitch
	{
		get
		{
			return Sprms.GetUShort(36913, 0);
		}
		set
		{
			Sprms.SetUShortValue(36913, value);
		}
	}

	internal ushort PitchType
	{
		get
		{
			return Sprms.GetUShort(20530, 0);
		}
		set
		{
			if (value > 0 && value < 4)
			{
				Sprms.SetUShortValue(20530, value);
			}
		}
	}

	internal bool DrawLinesBetweenCols
	{
		get
		{
			return Sprms.GetBoolean(12313, defValue: false);
		}
		set
		{
			if (value)
			{
				Sprms.SetBoolValue(12313, flag: true);
			}
		}
	}

	internal bool ProtectForm
	{
		get
		{
			return Sprms.GetBoolean(12294, defValue: false);
		}
		set
		{
			if (value)
			{
				Sprms.SetBoolValue(12294, flag: false);
			}
			else
			{
				Sprms.SetBoolValue(12294, flag: true);
			}
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			return Sprms.GetBoolean(12857, defValue: false);
		}
		set
		{
			if (value)
			{
				Sprms.SetBoolValue(12857, flag: true);
			}
			else
			{
				Sprms.SetBoolValue(12857, flag: false);
			}
		}
	}

	internal byte ChapterPageSeparator
	{
		get
		{
			return Sprms.GetByte(12288, 0);
		}
		set
		{
			Sprms.SetByteValue(12288, value);
		}
	}

	internal byte HeadingLevelForChapter
	{
		get
		{
			return Sprms.GetByte(12289, 0);
		}
		set
		{
			Sprms.SetByteValue(12289, value);
		}
	}

	internal LineNumberingMode LineNumberingMode
	{
		get
		{
			return (LineNumberingMode)Sprms.GetByte(12307, 0);
		}
		set
		{
			Sprms.SetByteValue(12307, (byte)value);
		}
	}

	internal ushort LineNumberingStep
	{
		get
		{
			return Sprms.GetUShort(20501, 0);
		}
		set
		{
			Sprms.SetUShortValue(20501, value);
		}
	}

	internal short LineNumberingStartValue
	{
		get
		{
			return (short)(Sprms.GetShort(20507, 0) + 1);
		}
		set
		{
			short value2 = (short)(value - 1);
			Sprms.SetShortValue(20507, value2);
		}
	}

	internal short LineNumberingDistanceFromText
	{
		get
		{
			return Sprms.GetShort(36886, 0);
		}
		set
		{
			Sprms.SetShortValue(36886, value);
		}
	}

	internal BorderCode LeftBorder
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(28716);
			if (byteArray != null)
			{
				return new BorderCode(byteArray, 0);
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[4];
			value.SaveBytes(array, 0);
			Sprms.SetByteArrayValue(28716, array);
		}
	}

	internal BorderCode TopBorder
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(28715);
			if (byteArray != null)
			{
				return new BorderCode(byteArray, 0);
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[4];
			value.SaveBytes(array, 0);
			Sprms.SetByteArrayValue(28715, array);
		}
	}

	internal BorderCode RightBorder
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(28718);
			if (byteArray != null)
			{
				return new BorderCode(byteArray, 0);
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[4];
			value.SaveBytes(array, 0);
			Sprms.SetByteArrayValue(28718, array);
		}
	}

	internal BorderCode BottomBorder
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(28717);
			if (byteArray != null)
			{
				return new BorderCode(byteArray, 0);
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[4];
			value.SaveBytes(array, 0);
			Sprms.SetByteArrayValue(28717, array);
		}
	}

	internal BorderCode LeftBorderNew
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(53813);
			if (byteArray != null)
			{
				BorderCode borderCode = new BorderCode();
				borderCode.ParseNewBrc(byteArray, 0);
				return borderCode;
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[8];
			value.SaveNewBrc(array, 0);
			Sprms.SetByteArrayValue(53813, array);
		}
	}

	internal BorderCode TopBorderNew
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(53812);
			if (byteArray != null)
			{
				BorderCode borderCode = new BorderCode();
				borderCode.ParseNewBrc(byteArray, 0);
				return borderCode;
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[8];
			value.SaveNewBrc(array, 0);
			Sprms.SetByteArrayValue(53812, array);
		}
	}

	internal BorderCode RightBorderNew
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(53815);
			if (byteArray != null)
			{
				BorderCode borderCode = new BorderCode();
				borderCode.ParseNewBrc(byteArray, 0);
				return borderCode;
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[8];
			value.SaveNewBrc(array, 0);
			Sprms.SetByteArrayValue(53815, array);
		}
	}

	internal BorderCode BottomBorderNew
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(53814);
			if (byteArray != null)
			{
				BorderCode borderCode = new BorderCode();
				borderCode.ParseNewBrc(byteArray, 0);
				return borderCode;
			}
			return new BorderCode();
		}
		set
		{
			byte[] array = new byte[8];
			value.SaveNewBrc(array, 0);
			Sprms.SetByteArrayValue(53814, array);
		}
	}

	internal PageBordersApplyType PageBorderApply
	{
		get
		{
			return (PageBordersApplyType)(Sprms.GetShort(21039, 0) & 7);
		}
		set
		{
			short @short = Sprms.GetShort(21039, 0);
			short num = (short)value;
			@short = (short)((@short & 0xFFF8) + num);
			if (@short != 0)
			{
				Sprms.SetShortValue(21039, @short);
			}
		}
	}

	internal bool PageBorderIsInFront
	{
		get
		{
			return (Sprms.GetShort(21039, 0) & 0x18) >> 3 != 1;
		}
		set
		{
			short @short = Sprms.GetShort(21039, 0);
			short num = (short)(((!value) ? 1u : 0u) << 3);
			@short = (short)((@short & 0xFFE7) + num);
			if (@short != 0)
			{
				Sprms.SetShortValue(21039, @short);
			}
		}
	}

	internal PageBorderOffsetFrom PageBorderOffsetFrom
	{
		get
		{
			return (PageBorderOffsetFrom)((Sprms.GetShort(21039, 0) & 0xE0) >> 5);
		}
		set
		{
			short @short = Sprms.GetShort(21039, 0);
			short num = (short)((short)value << 5);
			@short = (short)((@short & 0xFF1F) + num);
			if (@short != 0)
			{
				Sprms.SetShortValue(21039, @short);
			}
		}
	}

	internal ushort ColumnsCount
	{
		get
		{
			return (ushort)(Sprms.GetUShort(20491, 0) + 1);
		}
		set
		{
			Sprms.SetUShortValue(20491, (ushort)(value - 1));
		}
	}

	internal ushort EndnoteNumberFormat
	{
		get
		{
			return Sprms.GetUShort(20546, 2);
		}
		set
		{
			Sprms.SetUShortValue(20546, value);
		}
	}

	internal ushort FootnoteNumberFormat
	{
		get
		{
			return Sprms.GetUShort(20544, 0);
		}
		set
		{
			Sprms.SetUShortValue(20544, value);
		}
	}

	internal byte RestartIndexForEndnote
	{
		get
		{
			return Sprms.GetByte(12350, 0);
		}
		set
		{
			Sprms.SetByteValue(12350, value);
		}
	}

	internal byte RestartIndexForFootnotes
	{
		get
		{
			return Sprms.GetByte(12348, 0);
		}
		set
		{
			Sprms.SetByteValue(12348, value);
		}
	}

	internal byte FootnotePosition
	{
		get
		{
			return Sprms.GetByte(12347, 1);
		}
		set
		{
			Sprms.SetByteValue(12347, value);
		}
	}

	internal ushort InitialFootnoteNumber
	{
		get
		{
			return (ushort)(Sprms.GetUShort(20543, 0) + 1);
		}
		set
		{
			Sprms.SetUShortValue(20543, (ushort)(value - 1));
		}
	}

	internal ushort InitialEndnoteNumber
	{
		get
		{
			return (ushort)(Sprms.GetUShort(20545, 0) + 1);
		}
		set
		{
			Sprms.SetUShortValue(20545, (ushort)(value - 1));
		}
	}

	internal SectionProperties()
	{
		m_sepx = new SectionPropertyException();
		m_columnsArray = new ColumnArray(Sprms);
		m_oldColumnsArray = new ColumnArray(Sprms);
	}

	internal SectionProperties(SectionPropertyException sepx)
	{
		m_sepx = sepx;
		m_columnsArray = new ColumnArray(Sprms);
		m_columnsArray.ReadColumnsProperties(isFromPropertyHash: true);
		m_oldColumnsArray = new ColumnArray(Sprms);
		m_oldColumnsArray.ReadColumnsProperties(isFromPropertyHash: false);
	}

	internal SectionPropertyException CloneSepx()
	{
		SectionPropertyException sepx = m_sepx;
		m_sepx = new SectionPropertyException();
		if (StickProperties)
		{
			int i = 0;
			for (int count = sepx.Properties.Count; i < count; i++)
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord = sepx.Properties.GetSprmByIndex(i).Clone();
				if (singlePropertyModifierRecord != null)
				{
					m_sepx.Properties.Add(singlePropertyModifierRecord);
				}
			}
		}
		return sepx;
	}

	internal bool HasOptions(int options)
	{
		return Sprms[options] != null;
	}

	internal SinglePropertyModifierArray GetCopiableSprm()
	{
		SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray();
		int count = Sprms.Modifiers.Count;
		for (int i = 0; i < count; i++)
		{
			SinglePropertyModifierRecord sprmByIndex = Sprms.GetSprmByIndex(i);
			int typedOptions = sprmByIndex.TypedOptions;
			if (typedOptions != 20491 && typedOptions != 45087 && typedOptions != 45089 && typedOptions != 45090 && typedOptions != 61955 && typedOptions != 45079 && typedOptions != 45080 && typedOptions != 12314 && typedOptions != 12317 && typedOptions != 36900 && typedOptions != 36899 && typedOptions != 0)
			{
				singlePropertyModifierArray.Modifiers.Add(sprmByIndex);
			}
		}
		return singlePropertyModifierArray;
	}

	internal BorderCode GetBorder(SinglePropertyModifierRecord record)
	{
		byte[] byteArray = record.ByteArray;
		BorderCode borderCode;
		if (byteArray.Length == 4)
		{
			borderCode = new BorderCode(record.ByteArray, 0);
		}
		else
		{
			borderCode = new BorderCode();
			borderCode.ParseNewBrc(byteArray, 0);
		}
		return borderCode;
	}
}
