using System;
using System.Collections.Generic;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class CharacterProperties
{
	private CharacterPropertyException m_chpx;

	private WordStyleSheet m_styleSheet;

	private byte m_bFlags = 1;

	internal SinglePropertyModifierArray Sprms
	{
		get
		{
			if (m_chpx == null)
			{
				return null;
			}
			return m_chpx.PropertyModifiers;
		}
	}

	internal CharacterPropertyException CharacterPropertyException => m_chpx;

	internal bool ComplexScript
	{
		get
		{
			return Sprms.GetBoolean(2178, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2178, value);
		}
	}

	internal bool Bold
	{
		get
		{
			return Sprms.GetBoolean(2101, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2101, value);
		}
	}

	internal byte BoldComplex
	{
		get
		{
			return Sprms.GetByte(2101, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2101, value);
			}
		}
	}

	internal bool Italic
	{
		get
		{
			return Sprms.GetBoolean(2102, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2102, value);
		}
	}

	internal byte ItalicComplex
	{
		get
		{
			return Sprms.GetByte(2102, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2102, value);
			}
		}
	}

	internal bool Strike
	{
		get
		{
			return Sprms.GetBoolean(2103, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2103, value);
		}
	}

	internal byte ShadowComplex
	{
		get
		{
			return Sprms.GetByte(2105, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2105, value);
			}
		}
	}

	internal byte StrikeComplex
	{
		get
		{
			return Sprms.GetByte(2103, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2103, value);
			}
		}
	}

	internal bool DoubleStrike
	{
		get
		{
			return Sprms.GetBoolean(10835, defValue: false);
		}
		set
		{
			if (value)
			{
				Strike = false;
			}
			Sprms.SetBoolValue(10835, value);
		}
	}

	internal byte UnderlineCode
	{
		get
		{
			return Sprms.GetByte(10814, 0);
		}
		set
		{
			Sprms.SetByteValue(10814, value);
		}
	}

	internal string FontName
	{
		get
		{
			if (m_styleSheet.FontNamesList.Count == 0)
			{
				return string.Empty;
			}
			return m_styleSheet.FontNamesList[FontAscii];
		}
		set
		{
			int num = m_styleSheet.FontNameToIndex(value);
			if (num >= 0)
			{
				ushort num3 = (FontNonFarEast = (ushort)num);
				ushort fontAscii = (FontFarEast = num3);
				FontAscii = fontAscii;
			}
			else
			{
				ushort num3 = (FontNonFarEast = (ushort)m_styleSheet.FontNamesList.Count);
				ushort fontAscii = (FontFarEast = num3);
				FontAscii = fontAscii;
				m_styleSheet.UpdateFontName(value);
			}
		}
	}

	internal string FontNameAscii
	{
		get
		{
			if (m_styleSheet.FontNamesList.Count == 0)
			{
				return string.Empty;
			}
			return m_styleSheet.FontNamesList[FontAscii];
		}
		set
		{
			int num = m_styleSheet.FontNameToIndex(value);
			if (num >= 0)
			{
				FontAscii = (ushort)num;
				return;
			}
			FontAscii = (ushort)m_styleSheet.FontNamesList.Count;
			m_styleSheet.UpdateFontName(value);
		}
	}

	internal string FontNameFarEast
	{
		get
		{
			if (m_styleSheet.FontNamesList.Count == 0)
			{
				return string.Empty;
			}
			return m_styleSheet.FontNamesList[FontFarEast];
		}
		set
		{
			int num = m_styleSheet.FontNameToIndex(value);
			if (num >= 0)
			{
				FontFarEast = (ushort)num;
				return;
			}
			FontFarEast = (ushort)m_styleSheet.FontNamesList.Count;
			m_styleSheet.UpdateFontName(value);
		}
	}

	internal string FontNameNonFarEast
	{
		get
		{
			if (m_styleSheet.FontNamesList.Count == 0)
			{
				return string.Empty;
			}
			return m_styleSheet.FontNamesList[FontNonFarEast];
		}
		set
		{
			int num = m_styleSheet.FontNameToIndex(value);
			if (num >= 0)
			{
				FontNonFarEast = (ushort)num;
				return;
			}
			FontNonFarEast = (ushort)m_styleSheet.FontNamesList.Count;
			m_styleSheet.UpdateFontName(value);
		}
	}

	internal string FontNameBi
	{
		get
		{
			if (m_styleSheet.FontNamesList.Count == 0)
			{
				return string.Empty;
			}
			return m_styleSheet.FontNamesList[FontBi];
		}
		set
		{
			int num = m_styleSheet.FontNameToIndex(value);
			if (num >= 0)
			{
				FontBi = (ushort)num;
				return;
			}
			FontBi = (ushort)m_styleSheet.FontNamesList.Count;
			m_styleSheet.UpdateFontName(value);
		}
	}

	internal ushort FontAscii
	{
		get
		{
			return Sprms.GetUShort(19023, 0);
		}
		set
		{
			Sprms.SetUShortValue(19023, value);
		}
	}

	internal ushort FontFarEast
	{
		get
		{
			return Sprms.GetUShort(19024, 0);
		}
		set
		{
			Sprms.SetUShortValue(19024, value);
		}
	}

	internal ushort FontNonFarEast
	{
		get
		{
			return Sprms.GetUShort(19025, 0);
		}
		set
		{
			Sprms.SetUShortValue(19025, value);
		}
	}

	internal ushort FontBi
	{
		get
		{
			return Sprms.GetUShort(19038, 0);
		}
		set
		{
			Sprms.SetUShortValue(19038, value);
		}
	}

	internal float FontSize
	{
		get
		{
			return (float)(int)FontSizeHP / 2f;
		}
		set
		{
			FontSizeHP = (ushort)(value * 2f);
		}
	}

	internal ushort FontSizeHP
	{
		get
		{
			return Sprms.GetUShort(19011, 20);
		}
		set
		{
			Sprms.SetUShortValue(19011, value);
		}
	}

	internal byte FontColor
	{
		get
		{
			return Sprms.GetByte(10818, 0);
		}
		set
		{
			Sprms.SetByteValue(10818, value);
		}
	}

	internal Color FontColorExt
	{
		get
		{
			uint uInt = Sprms.GetUInt(26736, uint.MaxValue);
			if (uInt == uint.MaxValue)
			{
				return WordColor.ConvertIdToColor(FontColor);
			}
			return WordColor.ConvertRGBToColor(uInt);
		}
		set
		{
			uint value2 = WordColor.ConvertColorToRGB(value);
			Sprms.SetUIntValue(26736, value2);
		}
	}

	internal uint FontColorRGB
	{
		get
		{
			uint uInt = Sprms.GetUInt(26736, uint.MaxValue);
			if (uInt == uint.MaxValue)
			{
				return WordColor.ConvertIdToRGB(FontColor);
			}
			return uInt;
		}
		set
		{
			Sprms.SetUIntValue(26736, value);
		}
	}

	internal byte HighlightColor
	{
		get
		{
			return Sprms.GetByte(10764, 0);
		}
		set
		{
			Sprms.SetByteValue(10764, value);
		}
	}

	internal byte SubSuperScript
	{
		get
		{
			return Sprms.GetByte(10824, 0);
		}
		set
		{
			byte b = value;
			if (b <= 2)
			{
				Sprms.SetByteValue(10824, b);
			}
		}
	}

	internal byte Clear
	{
		get
		{
			return Sprms.GetByte(10361, 0);
		}
		set
		{
			byte b = value;
			if (b <= 3)
			{
				Sprms.SetByteValue(10361, b);
			}
		}
	}

	internal int PicLocation
	{
		get
		{
			int result = 0;
			if (Sprms[27139] != null)
			{
				result = Sprms.GetInt(27139, 0);
			}
			return result;
		}
		set
		{
			Sprms.SetIntValue(27139, value);
		}
	}

	internal bool Outline
	{
		get
		{
			return Sprms.GetBoolean(2104, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2104, value);
		}
	}

	internal bool Shadow
	{
		get
		{
			return Sprms.GetBoolean(2105, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2105, value);
		}
	}

	internal bool Emboss
	{
		get
		{
			return Sprms.GetBoolean(2136, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2136, value);
		}
	}

	internal byte EmbossComplex
	{
		get
		{
			return Sprms.GetByte(2136, byte.MaxValue);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2136, value);
			}
		}
	}

	internal bool Engrave
	{
		get
		{
			return Sprms.GetBoolean(2132, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2132, value);
		}
	}

	internal byte EngraveComplex
	{
		get
		{
			return Sprms.GetByte(2132, byte.MaxValue);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2132, value);
			}
		}
	}

	internal bool Hidden
	{
		get
		{
			return Sprms.GetBoolean(2108, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2108, value);
		}
	}

	internal bool SpecVanish
	{
		get
		{
			return Sprms.GetBoolean(2072, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2072, value);
		}
	}

	internal bool SmallCaps
	{
		get
		{
			return Sprms.GetBoolean(2106, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2106, value);
		}
	}

	internal bool AllCaps
	{
		get
		{
			return Sprms.GetBoolean(2107, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2107, value);
		}
	}

	internal byte AllCapsComplex
	{
		get
		{
			return Sprms.GetByte(2107, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2107, value);
			}
		}
	}

	internal short Position
	{
		get
		{
			return Sprms.GetShort(18501, 0);
		}
		set
		{
			Sprms.SetShortValue(18501, value);
		}
	}

	internal short LineSpacing
	{
		get
		{
			return Sprms.GetShort(34880, 0);
		}
		set
		{
			Sprms.SetShortValue(34880, value);
		}
	}

	internal ushort Scaling
	{
		get
		{
			return Sprms.GetUShort(18514, 0);
		}
		set
		{
			Sprms.SetUShortValue(18514, value);
		}
	}

	internal ushort Kern
	{
		get
		{
			return Sprms.GetUShort(18507, 0);
		}
		set
		{
			Sprms.SetUShortValue(18507, value);
		}
	}

	internal ShadingDescriptor Shading
	{
		get
		{
			return new ShadingDescriptor(Sprms.GetShort(18534, 0));
		}
		set
		{
			short value2 = value.Save();
			Sprms.SetShortValue(18534, value2);
		}
	}

	internal ShadingDescriptor ShadingNew
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(51825);
			ShadingDescriptor shadingDescriptor = new ShadingDescriptor();
			shadingDescriptor.ReadNewShd(byteArray, 0);
			return shadingDescriptor;
		}
		set
		{
			byte[] value2 = value.SaveNewShd();
			Sprms.SetByteArrayValue(51825, value2);
		}
	}

	internal BorderCode Border
	{
		get
		{
			return new BorderCode(Sprms.GetByteArray(26725), 0);
		}
		set
		{
			byte[] array = new byte[4];
			value.SaveBytes(array, 0);
			Sprms.SetByteArrayValue(26725, array);
		}
	}

	internal bool StickProperties
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool Special
	{
		get
		{
			return Sprms.GetBoolean(2133, defValue: false);
		}
		set
		{
			if (value)
			{
				Sprms.SetBoolValue(2133, flag: true);
			}
			else
			{
				Sprms.RemoveValue(2133);
			}
		}
	}

	internal SymbolDescriptor Symbol
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(27145);
			SymbolDescriptor symbolDescriptor = new SymbolDescriptor();
			if (byteArray != null)
			{
				symbolDescriptor.Parse(byteArray);
			}
			return symbolDescriptor;
		}
		set
		{
			byte[] value2 = value.Save();
			Sprms.SetByteArrayValue(27145, value2);
		}
	}

	internal byte HiddenComplex
	{
		get
		{
			return Sprms.GetByte(2108, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2108, value);
			}
		}
	}

	internal byte DoubleStrikeComplex
	{
		get
		{
			return Sprms.GetByte(10835, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(10835, value);
			}
		}
	}

	internal byte SmallCapsComplex
	{
		get
		{
			return Sprms.GetByte(2106, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2106, value);
			}
		}
	}

	internal bool FldVanish
	{
		get
		{
			return Sprms.GetBoolean(2050, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2050, value);
		}
	}

	internal byte FldVanishComplex
	{
		get
		{
			return Sprms.GetByte(2050, 0);
		}
		set
		{
			if (value != byte.MaxValue)
			{
				Sprms.SetByteValue(2050, value);
			}
		}
	}

	internal bool NoProof
	{
		get
		{
			return Sprms.GetBoolean(2165, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2165, value);
		}
	}

	internal byte IdctHint
	{
		get
		{
			return Sprms.GetByte(10351, 0);
		}
		set
		{
			Sprms.SetByteValue(10351, value);
		}
	}

	internal bool IsInsertRevision
	{
		get
		{
			return Sprms.GetBoolean(2049, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2049, value);
		}
	}

	internal bool IsDeleteRevision
	{
		get
		{
			return Sprms.GetBoolean(2048, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2048, value);
		}
	}

	internal bool IsChangedFormat
	{
		get
		{
			byte[] byteArray = Sprms.GetByteArray(51799);
			if (byteArray == null)
			{
				byteArray = Sprms.GetByteArray(51849);
				if (byteArray != null)
				{
					return byteArray[0] == 1;
				}
			}
			return false;
		}
		set
		{
			byte[] value2 = new byte[7] { 1, 0, 0, 0, 0, 0, 0 };
			Sprms.SetByteArrayValue(51849, value2);
		}
	}

	internal int ListPictureIndex
	{
		get
		{
			bool listHasImage = ListHasImage;
			int result = int.MaxValue;
			if (listHasImage)
			{
				result = Sprms.GetInt(26759, int.MaxValue);
			}
			return result;
		}
		set
		{
			if (value != int.MaxValue)
			{
				Sprms.SetIntValue(26759, value);
			}
		}
	}

	internal bool ListHasImage
	{
		get
		{
			return Sprms.GetBoolean(18568, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(18568, value);
		}
	}

	internal bool BoldBi
	{
		get
		{
			return Sprms.GetBoolean(2140, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2140, value);
		}
	}

	internal bool ItalicBi
	{
		get
		{
			return Sprms.GetBoolean(2141, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2141, value);
		}
	}

	internal bool Bidi
	{
		get
		{
			return Sprms.GetBoolean(2138, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2138, value);
		}
	}

	internal ushort FontSizeBi
	{
		get
		{
			return (ushort)(Sprms.GetUShort(19041, 1) / 2);
		}
		set
		{
			Sprms.SetUShortValue(19041, (ushort)(value * 2));
		}
	}

	internal WordStyleSheet StyleSheet
	{
		get
		{
			return m_styleSheet;
		}
		set
		{
			m_styleSheet = value;
		}
	}

	internal bool IsOle2
	{
		get
		{
			return Sprms[2058]?.BoolValue ?? false;
		}
		set
		{
			Sprms.SetBoolValue(2058, value);
		}
	}

	internal bool IsData
	{
		get
		{
			return Sprms.GetBoolean(2054, defValue: false);
		}
		set
		{
			Sprms.SetBoolValue(2054, value);
		}
	}

	internal ushort CharacterStyleId
	{
		get
		{
			return Sprms.GetUShort(18992, 0);
		}
		set
		{
			if (value != 0)
			{
				Sprms.SetUShortValue(18992, value);
			}
		}
	}

	internal short LocaleIdASCII
	{
		get
		{
			return Sprms.GetShort(18541, 1033);
		}
		set
		{
			if (value != short.MaxValue)
			{
				Sprms.SetShortValue(18541, value);
			}
		}
	}

	internal short LocaleIdASCII1
	{
		get
		{
			return Sprms.GetShort(18547, 0);
		}
		set
		{
			if (value != short.MaxValue)
			{
				Sprms.SetShortValue(18547, value);
			}
		}
	}

	internal short LocaleIdFarEast
	{
		get
		{
			return Sprms.GetShort(18542, 1033);
		}
		set
		{
			if (value != short.MaxValue)
			{
				Sprms.SetShortValue(18542, value);
			}
		}
	}

	internal short LocaleIdFarEast1
	{
		get
		{
			return Sprms.GetShort(18548, 0);
		}
		set
		{
			if (value != short.MaxValue)
			{
				Sprms.SetShortValue(18548, value);
			}
		}
	}

	internal short LidBi
	{
		get
		{
			return Sprms.GetShort(18527, 0);
		}
		set
		{
			if (value != short.MaxValue)
			{
				Sprms.SetShortValue(18527, value);
			}
		}
	}

	internal CharacterProperties(WordStyleSheet styleSheet)
	{
		m_chpx = new CharacterPropertyException();
		m_styleSheet = styleSheet;
	}

	internal CharacterProperties(CharacterPropertyException chpx, WordStyleSheet styleSheet)
	{
		m_chpx = chpx;
		m_styleSheet = styleSheet;
	}

	internal bool HasOptions(int option)
	{
		return Sprms[option] != null;
	}

	internal SinglePropertyModifierArray GetCopiableSprm()
	{
		SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray();
		int count = Sprms.Modifiers.Count;
		for (int i = 0; i < count; i++)
		{
			SinglePropertyModifierRecord sprmByIndex = Sprms.GetSprmByIndex(i);
			int typedOptions = sprmByIndex.TypedOptions;
			if (typedOptions != 18992 && typedOptions != 2101 && typedOptions != 2102 && typedOptions != 2103 && typedOptions != 2108 && typedOptions != 2105 && typedOptions != 2136 && typedOptions != 2132 && typedOptions != 10835 && typedOptions != 2107 && typedOptions != 2106 && typedOptions != 2104 && typedOptions != 2165 && typedOptions != 2138 && typedOptions != 2140 && typedOptions != 2141 && typedOptions != 10814 && typedOptions != 10824 && typedOptions != 19011 && typedOptions != 19041 && typedOptions != 18501 && typedOptions != 34880 && typedOptions != 10818 && typedOptions != 26736 && typedOptions != 10764 && typedOptions != 18534 && typedOptions != 51825 && typedOptions != 19023 && typedOptions != 19024 && typedOptions != 19025 && typedOptions != 19038 && typedOptions != 26725 && typedOptions != 18541 && typedOptions != 18542 && typedOptions != 18547 && typedOptions != 18548 && typedOptions != 18527 && typedOptions != 10351 && typedOptions != 2133 && typedOptions != 27139 && typedOptions != 27145 && typedOptions != 0 && typedOptions != 26645 && typedOptions != 26646 && typedOptions != 26624 && typedOptions != 2560 && typedOptions != 10752 && typedOptions != 26880 && typedOptions != 51200 && typedOptions != 27136 && typedOptions != 18944 && typedOptions != 43264 && typedOptions != 43520 && typedOptions != 43776)
			{
				singlePropertyModifierArray.Modifiers.Add(sprmByIndex);
			}
		}
		return singlePropertyModifierArray;
	}

	internal bool GetBoolean(SinglePropertyModifierRecord record)
	{
		return record.BoolValue;
	}

	internal string GetFontName(SinglePropertyModifierRecord record)
	{
		if (record.UshortValue >= m_styleSheet.FontNamesList.Count)
		{
			return "Times New Roman";
		}
		return m_styleSheet.FontNamesList[record.UshortValue];
	}

	internal BorderCode GetBorder(SinglePropertyModifierRecord record)
	{
		return new BorderCode(record.ByteArray, 0);
	}

	internal ShadingDescriptor GetShading(SinglePropertyModifierRecord record)
	{
		byte[] byteArray = record.ByteArray;
		ShadingDescriptor shadingDescriptor;
		if (byteArray.Length == 2)
		{
			shadingDescriptor = new ShadingDescriptor(record.ShortValue);
		}
		else
		{
			shadingDescriptor = new ShadingDescriptor();
			shadingDescriptor.ReadNewShd(byteArray, 0);
		}
		return shadingDescriptor;
	}

	internal SymbolDescriptor GetSymbol(SinglePropertyModifierRecord record)
	{
		SymbolDescriptor symbolDescriptor = new SymbolDescriptor();
		byte[] byteArray = record.ByteArray;
		if (byteArray != null)
		{
			symbolDescriptor.Parse(byteArray);
		}
		return symbolDescriptor;
	}

	internal Color GetColor(SinglePropertyModifierRecord record)
	{
		uint uIntValue = record.UIntValue;
		if (uIntValue == uint.MaxValue)
		{
			return WordColor.ConvertIdToColor(FontColor);
		}
		return WordColor.ConvertRGBToColor(uIntValue);
	}

	internal void SetAllFontNames(string fontName)
	{
		int num = m_styleSheet.FontNameToIndex(fontName);
		if (num >= 0)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(19023);
			singlePropertyModifierRecord.UshortValue = (ushort)num;
			Sprms.Add(singlePropertyModifierRecord);
			singlePropertyModifierRecord = new SinglePropertyModifierRecord(19024);
			singlePropertyModifierRecord.UshortValue = (ushort)num;
			Sprms.Add(singlePropertyModifierRecord);
			singlePropertyModifierRecord = new SinglePropertyModifierRecord(19025);
			singlePropertyModifierRecord.UshortValue = (ushort)num;
			Sprms.Add(singlePropertyModifierRecord);
		}
		else
		{
			num = (ushort)m_styleSheet.FontNamesList.Count;
			SinglePropertyModifierRecord singlePropertyModifierRecord2 = new SinglePropertyModifierRecord(19023);
			singlePropertyModifierRecord2.UshortValue = (ushort)num;
			Sprms.Add(singlePropertyModifierRecord2);
			SinglePropertyModifierRecord singlePropertyModifierRecord3 = singlePropertyModifierRecord2.Clone();
			singlePropertyModifierRecord3.TypedOptions = 19024;
			Sprms.Add(singlePropertyModifierRecord3);
			SinglePropertyModifierRecord singlePropertyModifierRecord4 = singlePropertyModifierRecord2.Clone();
			singlePropertyModifierRecord4.TypedOptions = 19025;
			Sprms.Add(singlePropertyModifierRecord4);
			m_styleSheet.UpdateFontName(fontName);
		}
	}

	internal void SetFontName(string fontName, int option)
	{
		int num = m_styleSheet.FontNameToIndex(fontName);
		if (num >= 0)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(option);
			singlePropertyModifierRecord.UshortValue = (ushort)num;
			Sprms.Add(singlePropertyModifierRecord);
		}
		else
		{
			num = (ushort)m_styleSheet.FontNamesList.Count;
			SinglePropertyModifierRecord singlePropertyModifierRecord2 = new SinglePropertyModifierRecord(option);
			singlePropertyModifierRecord2.UshortValue = (ushort)num;
			Sprms.Add(singlePropertyModifierRecord2);
			m_styleSheet.UpdateFontName(fontName);
		}
	}

	internal void AddSprmWithBoolValue(int option, bool value)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(option);
		singlePropertyModifierRecord.BoolValue = value;
		Sprms.Add(singlePropertyModifierRecord);
	}

	internal void AddSprmWithByteValue(int option, byte value)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(option);
		singlePropertyModifierRecord.ByteValue = value;
		Sprms.Add(singlePropertyModifierRecord);
	}

	internal void AddSprmWithUShortValue(int option, ushort value)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(option);
		singlePropertyModifierRecord.UshortValue = value;
		Sprms.Add(singlePropertyModifierRecord);
	}

	internal void AddSprmWithShortValue(int option, short value)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(option);
		singlePropertyModifierRecord.ShortValue = value;
		Sprms.Add(singlePropertyModifierRecord);
	}

	internal void AddSprmWithIntValue(int option, int value)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = new SinglePropertyModifierRecord(option);
		singlePropertyModifierRecord.IntValue = value;
		Sprms.Add(singlePropertyModifierRecord);
	}

	internal CharacterPropertyException CloneChpx()
	{
		CharacterPropertyException chpx = m_chpx;
		m_chpx = new CharacterPropertyException();
		if (StickProperties && chpx != null)
		{
			int i = 0;
			for (int modifiersCount = chpx.ModifiersCount; i < modifiersCount; i++)
			{
				if (chpx.PropertyModifiers.GetSprmByIndex(i).Operand != null)
				{
					SinglePropertyModifierRecord singlePropertyModifierRecord = chpx.PropertyModifiers.GetSprmByIndex(i).Clone();
					if (singlePropertyModifierRecord != null)
					{
						m_chpx.PropertyModifiers.Add(singlePropertyModifierRecord);
					}
				}
			}
		}
		return chpx;
	}

	internal void RemoveSprm(int option)
	{
		List<SinglePropertyModifierRecord> modifiers = Sprms.Modifiers;
		int i = 0;
		for (int count = modifiers.Count; i < count; i++)
		{
			if (modifiers[i].TypedOptions == option)
			{
				modifiers.RemoveAt(i);
				break;
			}
		}
	}

	internal bool HasSprms()
	{
		if (m_chpx != null)
		{
			return m_chpx.HasSprms();
		}
		return false;
	}

	internal SinglePropertyModifierRecord GetNewSprm(int option)
	{
		SinglePropertyModifierRecord result = null;
		int newPropsStartIndex = GetNewPropsStartIndex();
		if (newPropsStartIndex == -1)
		{
			return result;
		}
		int i = newPropsStartIndex;
		for (int modifiersCount = m_chpx.ModifiersCount; i < modifiersCount; i++)
		{
			result = m_chpx.PropertyModifiers.GetSprmByIndex(i);
			if (result.OptionType == (WordSprmOptionType)option)
			{
				return result;
			}
		}
		return null;
	}

	private int GetNewPropsStartIndex()
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = m_chpx.PropertyModifiers[10883];
		if (singlePropertyModifierRecord != null)
		{
			return m_chpx.PropertyModifiers.Modifiers.IndexOf(singlePropertyModifierRecord) + 1;
		}
		return -1;
	}

	private int ConvertColor(int brg)
	{
		byte[] bytes = BitConverter.GetBytes(brg);
		byte b = bytes[0];
		bytes[0] = bytes[2];
		bytes[2] = b;
		return BitConverter.ToInt32(bytes, 0);
	}

	private static bool GetComplexBoolean(byte sprmValue, bool styleSheetValue)
	{
		if (sprmValue < 128)
		{
			return sprmValue == 1;
		}
		return sprmValue switch
		{
			128 => styleSheetValue, 
			129 => !styleSheetValue, 
			_ => throw new Exception("Complex boolean value is expected."), 
		};
	}

	public override string ToString()
	{
		return base.ToString();
	}

	internal void Close()
	{
		if (m_chpx != null)
		{
			if (m_chpx.PropertyModifiers != null)
			{
				m_chpx.PropertyModifiers.Close();
				m_chpx.PropertyModifiers = null;
			}
			m_chpx = null;
		}
		m_styleSheet = null;
	}
}
