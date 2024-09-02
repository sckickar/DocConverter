using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class CharacterPropertiesConverter
{
	private static List<int> m_incorrectOptions;

	private static readonly object m_threadLocker = new object();

	private static List<string> m_authorNames;

	private static List<int> IncorrectOptions
	{
		get
		{
			if (m_incorrectOptions == null)
			{
				m_incorrectOptions = new List<int>();
				m_incorrectOptions.Add(0);
				m_incorrectOptions.Add(10);
				m_incorrectOptions.Add(15);
				m_incorrectOptions.Add(16);
				m_incorrectOptions.Add(17);
				m_incorrectOptions.Add(30);
				m_incorrectOptions.Add(31);
				m_incorrectOptions.Add(32);
				m_incorrectOptions.Add(33);
				m_incorrectOptions.Add(35);
				m_incorrectOptions.Add(36);
				m_incorrectOptions.Add(40);
				m_incorrectOptions.Add(42);
				m_incorrectOptions.Add(43);
				m_incorrectOptions.Add(44);
				m_incorrectOptions.Add(45);
				m_incorrectOptions.Add(46);
				m_incorrectOptions.Add(50);
				m_incorrectOptions.Add(256);
				m_incorrectOptions.Add(512);
				m_incorrectOptions.Add(2304);
				m_incorrectOptions.Add(2560);
				m_incorrectOptions.Add(3328);
				m_incorrectOptions.Add(3968);
				m_incorrectOptions.Add(4096);
				m_incorrectOptions.Add(4104);
				m_incorrectOptions.Add(4185);
				m_incorrectOptions.Add(4460);
				m_incorrectOptions.Add(4468);
				m_incorrectOptions.Add(6912);
				m_incorrectOptions.Add(7914);
				m_incorrectOptions.Add(8163);
				m_incorrectOptions.Add(8169);
				m_incorrectOptions.Add(8207);
				m_incorrectOptions.Add(8325);
				m_incorrectOptions.Add(8482);
				m_incorrectOptions.Add(8520);
				m_incorrectOptions.Add(8933);
				m_incorrectOptions.Add(8939);
				m_incorrectOptions.Add(8960);
				m_incorrectOptions.Add(9080);
				m_incorrectOptions.Add(9088);
				m_incorrectOptions.Add(9212);
				m_incorrectOptions.Add(9253);
				m_incorrectOptions.Add(9367);
				m_incorrectOptions.Add(9728);
				m_incorrectOptions.Add(9984);
				m_incorrectOptions.Add(10446);
				m_incorrectOptions.Add(10752);
				m_incorrectOptions.Add(10789);
				m_incorrectOptions.Add(10876);
				m_incorrectOptions.Add(11015);
				m_incorrectOptions.Add(11061);
				m_incorrectOptions.Add(11176);
				m_incorrectOptions.Add(11493);
				m_incorrectOptions.Add(11603);
				m_incorrectOptions.Add(11776);
				m_incorrectOptions.Add(12897);
				m_incorrectOptions.Add(12929);
				m_incorrectOptions.Add(13028);
				m_incorrectOptions.Add(13036);
				m_incorrectOptions.Add(13063);
				m_incorrectOptions.Add(13287);
				m_incorrectOptions.Add(13298);
				m_incorrectOptions.Add(13328);
				m_incorrectOptions.Add(13824);
				m_incorrectOptions.Add(17408);
				m_incorrectOptions.Add(19968);
				m_incorrectOptions.Add(20224);
				m_incorrectOptions.Add(21504);
				m_incorrectOptions.Add(21760);
				m_incorrectOptions.Add(22016);
				m_incorrectOptions.Add(24064);
				m_incorrectOptions.Add(24320);
				m_incorrectOptions.Add(26624);
				m_incorrectOptions.Add(27136);
				m_incorrectOptions.Add(27904);
				m_incorrectOptions.Add(29952);
				m_incorrectOptions.Add(30976);
				m_incorrectOptions.Add(31488);
				m_incorrectOptions.Add(32000);
				m_incorrectOptions.Add(32280);
				m_incorrectOptions.Add(32768);
				m_incorrectOptions.Add(33024);
				m_incorrectOptions.Add(33536);
				m_incorrectOptions.Add(34816);
				m_incorrectOptions.Add(35328);
				m_incorrectOptions.Add(35584);
				m_incorrectOptions.Add(35840);
				m_incorrectOptions.Add(38656);
				m_incorrectOptions.Add(40192);
				m_incorrectOptions.Add(40960);
				m_incorrectOptions.Add(41984);
				m_incorrectOptions.Add(42240);
				m_incorrectOptions.Add(42496);
				m_incorrectOptions.Add(43008);
				m_incorrectOptions.Add(43520);
				m_incorrectOptions.Add(43776);
				m_incorrectOptions.Add(44032);
				m_incorrectOptions.Add(44288);
				m_incorrectOptions.Add(45568);
				m_incorrectOptions.Add(45824);
				m_incorrectOptions.Add(46080);
				m_incorrectOptions.Add(46336);
				m_incorrectOptions.Add(47104);
				m_incorrectOptions.Add(47360);
				m_incorrectOptions.Add(47616);
				m_incorrectOptions.Add(47872);
				m_incorrectOptions.Add(48128);
				m_incorrectOptions.Add(49408);
				m_incorrectOptions.Add(52992);
				m_incorrectOptions.Add(53504);
				m_incorrectOptions.Add(58112);
				m_incorrectOptions.Add(58368);
				m_incorrectOptions.Add(58880);
				m_incorrectOptions.Add(59904);
				m_incorrectOptions.Add(60160);
				m_incorrectOptions.Add(60672);
				m_incorrectOptions.Add(61696);
				m_incorrectOptions.Add(64000);
				m_incorrectOptions.Add(64256);
			}
			return m_incorrectOptions;
		}
	}

	internal static List<string> AuthorNames
	{
		get
		{
			if (m_authorNames == null)
			{
				m_authorNames = new List<string>();
				m_authorNames.Add("Unknown");
			}
			return m_authorNames;
		}
	}

	public static void SprmsToFormat(IWordReaderBase reader, WCharacterFormat format)
	{
		lock (m_threadLocker)
		{
			SprmsToFormat(reader.CHPX.PropertyModifiers, format, reader.StyleSheet, reader.SttbfRMarkAuthorNames, isNewPropertyHash: true);
		}
	}

	internal static void SprmsToFormat(SinglePropertyModifierArray CHPModifierArray, WCharacterFormat characterFormat, WordStyleSheet styleSheet, Dictionary<int, string> authorNames, bool isNewPropertyHash)
	{
		lock (m_threadLocker)
		{
			characterFormat.IsDocReading = true;
			if (CHPModifierArray == null)
			{
				return;
			}
			if (isNewPropertyHash)
			{
				characterFormat.PropertiesHash.Clear();
				characterFormat.OldPropertiesHash.Clear();
			}
			if (CHPModifierArray.Contain(10883))
			{
				characterFormat.IsFormattingChange = true;
			}
			bool flag = false;
			foreach (SinglePropertyModifierRecord item in CHPModifierArray)
			{
				switch (item.Options)
				{
				case 2048:
					characterFormat.SetPropertyValue(104, item.ByteValue);
					break;
				case 2049:
					characterFormat.SetPropertyValue(103, item.ByteValue);
					break;
				case 2050:
					characterFormat.SetPropertyValue(109, item.ByteValue);
					break;
				case 18436:
				{
					short @short = CHPModifierArray.GetShort(18436, 0);
					if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(@short))
					{
						characterFormat.AuthorName = authorNames[@short];
					}
					break;
				}
				case 26629:
				case 26724:
				{
					DateTime revDateTime = characterFormat.ParseDTTM(item.IntValue);
					if (revDateTime.Year < 1900)
					{
						revDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
					}
					characterFormat.RevDateTime = revDateTime;
					break;
				}
				case 10764:
					characterFormat.HighlightColor = WordColor.ColorsArray[item.ByteValue];
					break;
				case 2065:
					characterFormat.SetPropertyValue(92, item.ByteValue);
					break;
				case 2072:
					characterFormat.SetPropertyValue(24, item.ByteValue);
					break;
				case 18992:
					if (!flag && !(characterFormat.OwnerBase is Style) && styleSheet != null)
					{
						if (styleSheet.StyleNames.ContainsKey(item.UshortValue))
						{
							characterFormat.CharStyleName = styleSheet.StyleNames[item.UshortValue];
						}
						else if (styleSheet.StyleNames.Count > 0)
						{
							characterFormat.CharStyleName = styleSheet.StyleNames[0];
						}
					}
					break;
				case 51761:
				{
					if (item.ByteArray.Length == 0 || item.ByteArray[0] != 0 || !CHPModifierArray.Contain(18992) || characterFormat.OwnerBase is Style)
					{
						break;
					}
					ushort num = CHPModifierArray[18992].UshortValue;
					ushort num2 = BitConverter.ToUInt16(item.ByteArray, 1);
					ushort num3 = BitConverter.ToUInt16(item.ByteArray, 3);
					if (num3 < num2 || num < num2 || num > num3)
					{
						break;
					}
					int num4 = 5;
					ushort num5 = (ushort)(num3 - num2 + 1);
					ushort[] array = new ushort[num5];
					for (int i = 0; i < num5; i++)
					{
						if (num4 + 1 >= item.ByteArray.Length)
						{
							break;
						}
						array[i] = BitConverter.ToUInt16(item.ByteArray, num4);
						num4 += 2;
					}
					if (num - num2 < array.Length)
					{
						num = array[num - num2];
					}
					if (styleSheet != null)
					{
						if (styleSheet.StyleNames.ContainsKey(num))
						{
							characterFormat.CharStyleName = styleSheet.StyleNames[num];
						}
						else if (styleSheet.StyleNames.Count > 0)
						{
							characterFormat.CharStyleName = styleSheet.StyleNames[0];
						}
					}
					flag = true;
					break;
				}
				case 10804:
					characterFormat.EmphasisType = (EmphasisType)item.ByteValue;
					break;
				case 2101:
					characterFormat.SetPropertyValue(4, item.ByteValue);
					break;
				case 2102:
					characterFormat.SetPropertyValue(5, item.ByteValue);
					break;
				case 2103:
					characterFormat.SetPropertyValue(6, item.ByteValue);
					break;
				case 2104:
					characterFormat.SetPropertyValue(71, item.ByteValue);
					break;
				case 2105:
					characterFormat.SetPropertyValue(50, item.ByteValue);
					break;
				case 2106:
					characterFormat.SetPropertyValue(55, item.ByteValue);
					break;
				case 2107:
					characterFormat.SetPropertyValue(54, item.ByteValue);
					break;
				case 2108:
					characterFormat.SetPropertyValue(53, item.ByteValue);
					break;
				case 10814:
					characterFormat.UnderlineStyle = (UnderlineStyle)item.ByteValue;
					break;
				case 34880:
					characterFormat.SetPropertyValue(18, (float)CHPModifierArray.GetShort(34880, 0) / 20f);
					break;
				case 10818:
				case 19040:
					if (CHPModifierArray.GetUInt(26736, uint.MaxValue) == uint.MaxValue)
					{
						characterFormat.TextColor = WordColor.ConvertIdToColor(CHPModifierArray.GetByte(10818, 0));
					}
					break;
				case 19011:
					characterFormat.SetPropertyValue(3, (float)(int)item.UshortValue / 2f);
					break;
				case 18501:
					characterFormat.SetPropertyValue(17, (float)item.ShortValue / 2f);
					break;
				case 10824:
					characterFormat.SubSuperScript = (SubSuperScript)item.ByteValue;
					break;
				case 18507:
					characterFormat.Kern = (float)(int)item.UshortValue / 2f;
					break;
				case 19023:
				{
					int ushortValue3 = item.UshortValue;
					if (styleSheet != null && ushortValue3 < styleSheet.FontNamesList.Count)
					{
						string value = styleSheet.FontNamesList[ushortValue3];
						characterFormat.SetPropertyValue(68, value);
						characterFormat.SetPropertyValue(2, value);
					}
					break;
				}
				case 19024:
				{
					int ushortValue2 = item.UshortValue;
					if (styleSheet != null && ushortValue2 < styleSheet.FontNamesList.Count)
					{
						characterFormat.SetPropertyValue(69, styleSheet.FontNamesList[ushortValue2]);
					}
					break;
				}
				case 19025:
				{
					int ushortValue = item.UshortValue;
					if (styleSheet != null && ushortValue < styleSheet.FontNamesList.Count)
					{
						characterFormat.SetPropertyValue(70, styleSheet.FontNamesList[ushortValue]);
					}
					break;
				}
				case 18514:
					characterFormat.Scaling = (int)CHPModifierArray.GetUShort(item.Options, 0);
					break;
				case 10835:
					characterFormat.SetPropertyValue(14, item.ByteValue);
					break;
				case 2132:
					characterFormat.SetPropertyValue(52, item.ByteValue);
					break;
				case 2133:
					characterFormat.SetPropertyValue(106, item.ByteValue);
					break;
				case 51799:
					if (!CHPModifierArray.Contain(51849))
					{
						short key2 = BitConverter.ToInt16(item.ByteArray, 1);
						if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(key2))
						{
							characterFormat.FormatChangeAuthorName = authorNames[key2];
						}
						DateTime formatChangeDateTime2 = characterFormat.ParseDTTM(BitConverter.ToInt32(item.ByteArray, 3));
						if (formatChangeDateTime2.Year < 1900)
						{
							formatChangeDateTime2 = new DateTime(1900, 1, 1, 0, 0, 0);
						}
						characterFormat.FormatChangeDateTime = formatChangeDateTime2;
					}
					break;
				case 2136:
					characterFormat.SetPropertyValue(51, item.ByteValue);
					break;
				case 10329:
					characterFormat.TextEffect = (TextEffect)CHPModifierArray.GetByte(10329, 0);
					break;
				case 2138:
					characterFormat.SetPropertyValue(58, item.ByteValue);
					break;
				case 2140:
					characterFormat.SetPropertyValue(59, item.ByteValue);
					break;
				case 2141:
					characterFormat.SetPropertyValue(60, item.ByteValue);
					break;
				case 19038:
					if (styleSheet != null)
					{
						characterFormat.FontNameBidi = ((styleSheet.FontNamesList.Count == 0) ? string.Empty : ((item.UshortValue < styleSheet.FontNamesList.Count) ? styleSheet.FontNamesList[item.UshortValue] : string.Empty));
					}
					break;
				case 18527:
				{
					short short3 = CHPModifierArray.GetShort(18527, 0);
					characterFormat.LocaleIdBidi = (short)((short3 == 1024) ? 1025 : short3);
					break;
				}
				case 19041:
					characterFormat.SetPropertyValue(62, (float)(item.UshortValue / 2));
					break;
				case 18531:
				{
					short short2 = CHPModifierArray.GetShort(18531, 0);
					if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(short2))
					{
						characterFormat.AuthorName = authorNames[short2];
					}
					break;
				}
				case 26725:
					if (!CHPModifierArray.Contain(51826))
					{
						ParagraphPropertiesConverter.ExportBorder(new BorderCode(CHPModifierArray.GetByteArray(26725), 0), (Border)characterFormat.GetPropertyValue(67));
					}
					break;
				case 18534:
					if (!CHPModifierArray.Contain(51825))
					{
						ShadingDescriptor shading2 = GetShading(item);
						characterFormat.TextBackgroundColor = shading2.BackColor;
						characterFormat.ForeColor = shading2.ForeColor;
						characterFormat.TextureStyle = shading2.Pattern;
					}
					break;
				case 2152:
					characterFormat.SetPropertyValue(81, item.ByteValue);
					break;
				case 18541:
					characterFormat.LocaleIdASCII = item.ShortValue;
					break;
				case 18542:
					characterFormat.LocaleIdFarEast = item.ShortValue;
					break;
				case 10351:
					characterFormat.IdctHint = (FontHintType)item.ByteValue;
					break;
				case 26736:
				{
					uint uIntValue = item.UIntValue;
					if (uIntValue == uint.MaxValue)
					{
						characterFormat.TextColor = WordColor.ConvertIdToColor(CHPModifierArray.GetByte(10818, 0));
					}
					else
					{
						characterFormat.TextColor = WordColor.ConvertRGBToColor(uIntValue);
					}
					break;
				}
				case 51825:
				{
					ShadingDescriptor shading = GetShading(item);
					characterFormat.TextBackgroundColor = shading.BackColor;
					characterFormat.ForeColor = shading.ForeColor;
					characterFormat.TextureStyle = shading.Pattern;
					break;
				}
				case 51826:
				{
					byte[] byteArray = CHPModifierArray.GetByteArray(51826);
					BorderCode borderCode = new BorderCode();
					borderCode.ParseNewBrc(byteArray, 0);
					ParagraphPropertiesConverter.ExportBorder(borderCode, (Border)characterFormat.GetPropertyValue(67));
					break;
				}
				case 18547:
					characterFormat.LocaleIdASCII = item.ShortValue;
					break;
				case 18548:
					characterFormat.LocaleIdFarEast = item.ShortValue;
					break;
				case 2165:
					characterFormat.NoProof = item.BoolValue;
					break;
				case 51830:
					if (IsValidByteArray(item.ByteArray, 0))
					{
						characterFormat.FitTextWidth = BitConverter.ToInt32(item.ByteArray, 0) / 20;
					}
					if (item.ByteArray.Length > 7)
					{
						characterFormat.FitTextID = BitConverter.ToInt32(item.ByteArray, 4);
					}
					break;
				case 26743:
					characterFormat.UnderlineColor = WordColor.ConvertRGBToColor(item.UIntValue);
					break;
				case 51832:
				{
					CFELayout cFELayout = new CFELayout();
					if (IsValidByteArray(item.ByteArray, 0))
					{
						cFELayout.UpdateCFELayout((ushort)BitConverter.ToInt16(item.ByteArray, 0), BitConverter.ToInt32(item.ByteArray, 2));
					}
					characterFormat.CFELayout = cFELayout;
					break;
				}
				case 10361:
					characterFormat.BreakClear = (BreakClearType)item.ByteValue;
					break;
				case 2178:
					characterFormat.SetPropertyValue(99, item.ByteValue);
					break;
				case 10883:
					characterFormat.IsFormattingChange = false;
					characterFormat.IsChangedFormat = true;
					characterFormat.SetPropertyValue(105, true);
					break;
				case 26759:
				{
					SinglePropertyModifierRecord singlePropertyModifierRecord2 = (characterFormat.IsFormattingChange ? CHPModifierArray.GetOldSprm(18568, 10883) : CHPModifierArray.GetNewSprm(18568, 10883));
					if (singlePropertyModifierRecord2 != null && singlePropertyModifierRecord2.BoolValue)
					{
						characterFormat.ListPictureIndex = item.IntValue;
					}
					break;
				}
				case 18568:
					characterFormat.ListHasPicture = item.BoolValue;
					break;
				case 51849:
				{
					short key = BitConverter.ToInt16(item.ByteArray, 1);
					if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(key))
					{
						characterFormat.FormatChangeAuthorName = authorNames[key];
					}
					if (3 < item.ByteArray.Length - 3 || 3 > item.ByteArray.Length - 1)
					{
						DateTime formatChangeDateTime = characterFormat.ParseDTTM(BitConverter.ToInt32(item.ByteArray, 3));
						if (formatChangeDateTime.Year < 1900)
						{
							formatChangeDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
						}
						characterFormat.FormatChangeDateTime = formatChangeDateTime;
					}
					break;
				}
				}
			}
			if ((!CHPModifierArray.Contain(10883) || CHPModifierArray.GetNewSprm(18992, 10883) == null) && characterFormat.OldPropertiesHash.Count > 0 && characterFormat.PropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item2 in characterFormat.OldPropertiesHash)
				{
					if (!characterFormat.PropertiesHash.ContainsKey(item2.Key))
					{
						characterFormat.PropertiesHash.Add(item2.Key, item2.Value);
					}
				}
			}
			if (characterFormat.HasValue(103) || characterFormat.HasValue(104))
			{
				characterFormat.Document.SetTriggerElement(ref characterFormat.Document.m_supportedElementFlag_1, 30);
			}
		}
	}

	private static bool IsValidByteArray(byte[] sprmByteArray, int startIndex)
	{
		if (sprmByteArray.Length >= startIndex && startIndex <= sprmByteArray.Length - 1)
		{
			if (startIndex >= sprmByteArray.Length - 3)
			{
				return startIndex > sprmByteArray.Length - 1;
			}
			return true;
		}
		return false;
	}

	internal static void FormatToSprms(WCharacterFormat characterFormat, SinglePropertyModifierArray sprms, WordStyleSheet styleSheet)
	{
		lock (m_threadLocker)
		{
			sprms.Clear();
			Dictionary<int, object> dictionary = new Dictionary<int, object>();
			if (characterFormat.PropertiesHash.Count > 0)
			{
				dictionary = new Dictionary<int, object>(characterFormat.PropertiesHash);
			}
			if (characterFormat.OldPropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item in new Dictionary<int, object>(characterFormat.OldPropertiesHash))
				{
					FormatToSprms(item.Key, item.Value, sprms, characterFormat, styleSheet);
					if (dictionary.ContainsKey(item.Key) && dictionary[item.Key] == item.Value)
					{
						dictionary.Remove(item.Key);
					}
				}
			}
			if (dictionary.Count > 0)
			{
				UpdateFontSprms(dictionary, sprms, characterFormat, styleSheet);
			}
			if (dictionary.Count > 0 && dictionary.ContainsKey(105))
			{
				FormatToSprms(105, dictionary[105], sprms, characterFormat, styleSheet);
				dictionary.Remove(105);
			}
			if (dictionary.Count > 0)
			{
				SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray();
				foreach (KeyValuePair<int, object> item2 in dictionary)
				{
					FormatToSprms(item2.Key, item2.Value, singlePropertyModifierArray, characterFormat, styleSheet);
				}
				UpdateFontSprms(dictionary, singlePropertyModifierArray, characterFormat, styleSheet);
				for (int i = 0; i < singlePropertyModifierArray.Count; i++)
				{
					sprms.Add(singlePropertyModifierArray.GetSprmByIndex(i).Clone());
				}
				singlePropertyModifierArray.Clear();
			}
			sprms.SortSprms();
		}
	}

	internal static void FormatToSprms(int propKey, object value, SinglePropertyModifierArray sprms, WCharacterFormat charFormat, WordStyleSheet styleSheet)
	{
		lock (m_threadLocker)
		{
			int sprmOption = charFormat.GetSprmOption(propKey);
			switch (propKey)
			{
			case 4:
			case 5:
			case 6:
			case 24:
			case 50:
			case 51:
			case 52:
			case 53:
			case 54:
			case 55:
			case 58:
			case 59:
			case 60:
			case 71:
			case 76:
			case 81:
			case 92:
			case 99:
			case 103:
			case 104:
			case 106:
			case 109:
			{
				byte b = 0;
				if (charFormat.IsDocReading)
				{
					try
					{
						b = GetByteValue(value);
					}
					catch
					{
						b = charFormat.GetBoolComplexValue(propKey, charFormat.GetBoolPropertyValue((short)propKey));
					}
				}
				else
				{
					b = charFormat.GetBoolComplexValue(propKey, charFormat.GetBoolPropertyValue((short)propKey));
				}
				sprms.SetByteValue(sprmOption, b);
				break;
			}
			case 1:
			{
				uint num = WordColor.ConvertColorToRGB((Color)value);
				sprms.SetByteValue(sprmOption, (byte)WordColor.ConvertRGBToId(num));
				sprms.SetUIntValue(26736, num);
				break;
			}
			case 3:
				sprms.SetUShortValue(sprmOption, (ushort)((float)value * 2f));
				break;
			case 7:
				sprms.SetByteValue(sprmOption, (byte)(UnderlineStyle)value);
				break;
			case 91:
				if (value != null)
				{
					short num2 = (short)styleSheet.StyleNameToIndex(value.ToString());
					if (num2 > -1)
					{
						byte[] bytes4 = BitConverter.GetBytes(num2);
						sprms.SetByteArrayValue(18992, bytes4);
					}
				}
				break;
			case 90:
			{
				uint value2 = WordColor.ConvertColorToRGB((Color)value);
				sprms.SetUIntValue(26743, value2);
				break;
			}
			case 79:
				sprms.SetByteValue(sprmOption, (byte)(EmphasisType)value);
				break;
			case 80:
				sprms.SetByteValue(sprmOption, (byte)(TextEffect)value);
				break;
			case 72:
				sprms.SetByteValue(sprmOption, (byte)(FontHintType)value);
				break;
			case 9:
			case 77:
			case 78:
				if (!sprms.Contain(18534))
				{
					ShadingDescriptor shadingDescriptor = new ShadingDescriptor();
					if (charFormat.HasKey(9))
					{
						shadingDescriptor.BackColor = charFormat.TextBackgroundColor;
					}
					if (charFormat.HasKey(77))
					{
						shadingDescriptor.ForeColor = charFormat.ForeColor;
					}
					if (charFormat.HasKey(78))
					{
						shadingDescriptor.Pattern = charFormat.TextureStyle;
					}
					sprms.SetShortValue(18534, shadingDescriptor.Save());
					sprms.SetByteArrayValue(51825, shadingDescriptor.SaveNewShd());
				}
				break;
			case 10:
				sprms.SetByteValue(sprmOption, (byte)(SubSuperScript)value);
				break;
			case 14:
			{
				if (charFormat.DoubleStrike)
				{
					sprms.SetBoolValue(charFormat.GetSprmOption(6), flag: false);
				}
				byte b2 = 0;
				if (charFormat.IsDocReading)
				{
					try
					{
						b2 = GetByteValue(value);
					}
					catch
					{
						b2 = charFormat.GetBoolComplexValue(propKey, charFormat.GetBoolPropertyValue(14));
					}
				}
				else
				{
					b2 = charFormat.GetBoolComplexValue(propKey, charFormat.GetBoolPropertyValue(14));
				}
				sprms.SetByteValue(sprmOption, b2);
				break;
			}
			case 17:
				sprms.SetShortValue(sprmOption, (short)((float)value * 2f));
				break;
			case 18:
				sprms.SetShortValue(sprmOption, (short)((float)value * 20f));
				break;
			case 127:
				sprms.SetUShortValue(sprmOption, (ushort)(float)value);
				break;
			case 125:
				sprms.SetUShortValue(sprmOption, (ushort)((float)value * 2f));
				break;
			case 62:
				sprms.SetUShortValue(sprmOption, (ushort)((float)value * 2f));
				break;
			case 63:
				sprms.SetByteValue(sprmOption, (byte)WordColor.ConvertColorToId((Color)value));
				break;
			case 67:
				if (charFormat.Border.IsBorderDefined)
				{
					BorderCode borderCode = new BorderCode();
					ParagraphPropertiesConverter.ImportBorder(borderCode, (Border)value);
					byte[] array3 = new byte[4];
					borderCode.SaveBytes(array3, 0);
					sprms.SetByteArrayValue(sprmOption, array3);
					BorderCode borderCode2 = new BorderCode();
					ParagraphPropertiesConverter.ImportBorder(borderCode2, (Border)value);
					array3 = new byte[8];
					borderCode2.SaveNewBrc(array3, 0);
					sprms.SetByteArrayValue(51826, array3);
				}
				break;
			case 13:
				sprms.SetByteArrayValue(sprmOption, (value as CFELayout).GetCFELayoutBytes());
				break;
			case 73:
				sprms.SetShortValue(sprmOption, (short)value);
				sprms.SetShortValue(18547, (short)value);
				break;
			case 74:
				sprms.SetShortValue(sprmOption, (short)value);
				sprms.SetShortValue(18548, (short)value);
				break;
			case 75:
				sprms.SetShortValue(sprmOption, (short)value);
				break;
			case 107:
				sprms.SetIntValue(sprmOption, (int)value);
				break;
			case 108:
				sprms.SetBoolValue(sprmOption, (bool)value);
				break;
			case 126:
				sprms.SetByteValue(sprmOption, (byte)(BreakClearType)value);
				break;
			case 11:
			{
				Entity entity = charFormat.OwnerBase as Entity;
				List<Revision> list = ((charFormat.OwnerBase is WParagraph || charFormat.OwnerBase is WTableCell) ? charFormat.Revisions : ((charFormat.OwnerBase is WTableRow) ? (charFormat.OwnerBase as WTableRow).RowFormat.Revisions : entity?.RevisionsInternal));
				if (list == null || list.Count <= 0)
				{
					break;
				}
				{
					foreach (Revision item in list)
					{
						if (item.RevisionType == RevisionType.Insertions || item.RevisionType == RevisionType.MoveTo)
						{
							sprms.SetIntValue(26629, charFormat.GetDTTMIntValue(item.Date));
						}
						else if (item.RevisionType == RevisionType.Deletions || item.RevisionType == RevisionType.MoveFrom)
						{
							sprms.SetIntValue(26724, charFormat.GetDTTMIntValue(item.Date));
						}
					}
					break;
				}
			}
			case 8:
			{
				Entity entity = charFormat.OwnerBase as Entity;
				List<Revision> list = ((charFormat.OwnerBase is WParagraph || charFormat.OwnerBase is WTableCell) ? charFormat.Revisions : ((charFormat.OwnerBase is WTableRow) ? (charFormat.OwnerBase as WTableRow).RowFormat.Revisions : entity?.RevisionsInternal));
				if (list == null || list.Count <= 0)
				{
					break;
				}
				{
					foreach (Revision item2 in list)
					{
						if (!AuthorNames.Contains(item2.Author))
						{
							AuthorNames.Add(item2.Author);
						}
						if (item2.RevisionType == RevisionType.Insertions || item2.RevisionType == RevisionType.MoveTo)
						{
							sprms.SetShortValue(18436, (short)AuthorNames.IndexOf(item2.Author));
						}
						else if (item2.RevisionType == RevisionType.Deletions || item2.RevisionType == RevisionType.MoveFrom)
						{
							sprms.SetShortValue(18531, (short)AuthorNames.IndexOf(item2.Author));
						}
					}
					break;
				}
			}
			case 12:
			case 15:
				if (!sprms.Contain(51849) && charFormat.IsChangedFormat)
				{
					byte[] array2 = new byte[7] { 1, 0, 0, 0, 0, 0, 0 };
					if (!AuthorNames.Contains(charFormat.FormatChangeAuthorName))
					{
						AuthorNames.Add(charFormat.FormatChangeAuthorName);
					}
					byte[] bytes3 = BitConverter.GetBytes((short)AuthorNames.IndexOf(charFormat.FormatChangeAuthorName));
					byte[] src = new byte[4];
					if (charFormat.HasValue(15))
					{
						src = BitConverter.GetBytes(charFormat.GetDTTMIntValue(charFormat.FormatChangeDateTime));
					}
					Buffer.BlockCopy(bytes3, 0, array2, 1, 2);
					Buffer.BlockCopy(src, 0, array2, 3, 4);
					sprms.SetByteArrayValue(51799, array2);
					sprms.SetByteArrayValue(51849, array2);
				}
				break;
			case 105:
				sprms.SetBoolValue(10883, (bool)value);
				break;
			case 16:
			case 19:
				if (!sprms.Contain(51830))
				{
					byte[] array = new byte[8];
					byte[] bytes = BitConverter.GetBytes(charFormat.FitTextWidth * 20);
					byte[] bytes2 = BitConverter.GetBytes(charFormat.FitTextID);
					Buffer.BlockCopy(bytes, 0, array, 0, 4);
					Buffer.BlockCopy(bytes2, 0, array, 4, 4);
					sprms.SetByteArrayValue(51830, array);
				}
				break;
			}
		}
	}

	private static byte GetByteValue(object value)
	{
		if (value is ToggleOperand)
		{
			return (byte)value;
		}
		return 0;
	}

	internal static void UpdateFontSprms(Dictionary<int, object> propertyHash, SinglePropertyModifierArray sprms, WCharacterFormat charFormat, WordStyleSheet styleSheet)
	{
		string text = charFormat.FontName;
		if (propertyHash.ContainsKey(2))
		{
			text = propertyHash[2] as string;
			UpdateFontSprms(2, propertyHash[2], sprms, charFormat, styleSheet);
		}
		if (propertyHash.ContainsKey(68))
		{
			string text2 = propertyHash[68] as string;
			if (charFormat.IsThemeFont(text2))
			{
				text2 = text;
			}
			UpdateFontSprms(68, text2, sprms, charFormat, styleSheet);
		}
		if (propertyHash.ContainsKey(69))
		{
			string text2 = propertyHash[69] as string;
			if (charFormat.IsThemeFont(text2))
			{
				text2 = text;
			}
			UpdateFontSprms(69, text2, sprms, charFormat, styleSheet);
		}
		if (propertyHash.ContainsKey(70))
		{
			string text2 = propertyHash[70] as string;
			if (charFormat.IsThemeFont(text2))
			{
				text2 = text;
			}
			UpdateFontSprms(70, text2, sprms, charFormat, styleSheet);
		}
		if (propertyHash.ContainsKey(61))
		{
			string text2 = propertyHash[61] as string;
			if (charFormat.IsThemeFont(text2))
			{
				text2 = text;
			}
			UpdateFontSprms(61, text2, sprms, charFormat, styleSheet);
		}
	}

	internal static void UpdateFontSprms(int propKey, object value, SinglePropertyModifierArray sprms, WCharacterFormat charFormat, WordStyleSheet styleSheet)
	{
		lock (m_threadLocker)
		{
			int num = 0;
			charFormat.GetSprmOption(propKey);
			switch (propKey)
			{
			case 2:
				num = styleSheet.FontNameToIndex((string)value);
				if (num >= 0)
				{
					sprms.SetUShortValue(19023, (ushort)num);
					sprms.SetUShortValue(19024, (ushort)num);
					sprms.SetUShortValue(19025, (ushort)num);
				}
				else
				{
					ushort value2 = (ushort)styleSheet.FontNamesList.Count;
					sprms.SetUShortValue(19023, value2);
					sprms.SetUShortValue(19024, value2);
					sprms.SetUShortValue(19025, value2);
					styleSheet.UpdateFontName((string)value);
				}
				break;
			case 61:
				num = styleSheet.FontNameToIndex((string)value);
				if (num >= 0)
				{
					sprms.SetUShortValue(19038, (ushort)num);
					break;
				}
				sprms.SetUShortValue(19038, (ushort)styleSheet.FontNamesList.Count);
				styleSheet.UpdateFontName((string)value);
				break;
			case 69:
				num = styleSheet.FontNameToIndex((string)value);
				if (num >= 0)
				{
					sprms.SetUShortValue(19024, (ushort)num);
					break;
				}
				sprms.SetUShortValue(19024, (ushort)styleSheet.FontNamesList.Count);
				styleSheet.UpdateFontName((string)value);
				break;
			case 70:
				num = styleSheet.FontNameToIndex((string)value);
				if (num >= 0)
				{
					sprms.SetUShortValue(19025, (ushort)num);
					break;
				}
				sprms.SetUShortValue(19025, (ushort)styleSheet.FontNamesList.Count);
				styleSheet.UpdateFontName((string)value);
				break;
			case 68:
				num = styleSheet.FontNameToIndex((string)value);
				if (num >= 0)
				{
					sprms.SetUShortValue(19023, (ushort)num);
					break;
				}
				sprms.SetUShortValue(19023, (ushort)styleSheet.FontNamesList.Count);
				styleSheet.UpdateFontName((string)value);
				break;
			}
		}
	}

	internal static ShadingDescriptor GetShading(SinglePropertyModifierRecord record)
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

	internal static void Close()
	{
		if (m_incorrectOptions != null)
		{
			m_incorrectOptions.Clear();
			m_incorrectOptions = null;
		}
		if (m_authorNames != null)
		{
			m_authorNames.Clear();
			m_authorNames = null;
		}
	}
}
