using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class ParagraphPropertiesConverter
{
	private static readonly object m_threadLocker = new object();

	internal static void ExportBorder(BorderCode srcBorder, Border destBorder)
	{
		destBorder.BorderType = (BorderStyle)srcBorder.BorderType;
		destBorder.Color = srcBorder.LineColorExt;
		destBorder.LineWidth = (float)(int)srcBorder.LineWidth / 8f;
		destBorder.Space = (int)srcBorder.Space;
		destBorder.Shadow = srcBorder.Shadow;
	}

	internal static void ExportBorder(SinglePropertyModifierArray sprms, SinglePropertyModifierRecord oldSprm, int newSprmOption, Border border, WParagraphFormat paragraphFormat)
	{
		if (paragraphFormat.IsFormattingChange)
		{
			if (sprms.GetOldSprm(newSprmOption, 9828) == null)
			{
				ExportBorder(paragraphFormat.GetBorder(oldSprm), border);
			}
		}
		else if (sprms.GetNewSprm(newSprmOption, 9828) == null)
		{
			ExportBorder(paragraphFormat.GetBorder(oldSprm), border);
		}
	}

	internal static void ExportBorder(SinglePropertyModifierRecord sprm, Border border, WParagraphFormat paragraphFormat)
	{
		ExportBorder(paragraphFormat.GetBorder(sprm), border);
	}

	internal static void ExportBorder(Border source, Border destBorder)
	{
		source.BorderType = destBorder.BorderType;
		source.Color = destBorder.Color;
		source.LineWidth = destBorder.LineWidth;
		source.Space = destBorder.Space;
		source.Shadow = destBorder.Shadow;
	}

	internal static void ImportBorder(BorderCode destBorder, Border srcBorder)
	{
		if (!srcBorder.IsDefault)
		{
			if (srcBorder.BorderType == BorderStyle.Cleared)
			{
				destBorder.LineColor = 0;
				destBorder.LineColorExt = Color.FromArgb(0, 255, 255, 255);
				destBorder.BorderType = byte.MaxValue;
				destBorder.LineWidth = byte.MaxValue;
			}
			else
			{
				destBorder.LineColor = (byte)WordColor.ConvertColorToId(srcBorder.Color);
				destBorder.LineColorExt = srcBorder.Color;
				destBorder.BorderType = (byte)srcBorder.BorderType;
				destBorder.LineWidth = (byte)Math.Round(srcBorder.LineWidth * 8f);
			}
			destBorder.Space = (byte)Math.Round(srcBorder.Space);
			destBorder.Shadow = srcBorder.Shadow;
		}
		else
		{
			destBorder.LineColor = 0;
			destBorder.LineColorExt = Color.Empty;
			destBorder.BorderType = 0;
			destBorder.LineWidth = 0;
			destBorder.Space = 0;
			destBorder.Shadow = false;
		}
	}

	internal static void ExportTabs(TabsInfo info, TabCollection tabs)
	{
		if (info.TabDeletePositions != null)
		{
			for (int i = 0; i < info.TabDeletePositions.Length; i++)
			{
				tabs.AddTab().DeletePosition = info.TabDeletePositions[i];
			}
		}
		for (int j = 0; j < info.TabCount; j++)
		{
			TabDescriptor tabDescriptor = info.Descriptors[j];
			if (tabDescriptor == null)
			{
				tabDescriptor = new TabDescriptor(0);
			}
			Tab tab = tabs.AddTab();
			tab.Position = (float)info.TabPositions[j] / 20f;
			tab.Justification = tabDescriptor.Justification;
			tab.TabLeader = tabDescriptor.TabLeader;
		}
	}

	internal static void SprmsToFormat(SinglePropertyModifierArray sprms, WParagraphFormat paragraphFormat, Dictionary<int, string> authorNames, WordStyleSheet styleSheet)
	{
		if (sprms == null)
		{
			return;
		}
		lock (m_threadLocker)
		{
			if (sprms.Contain(9828))
			{
				paragraphFormat.IsFormattingChange = true;
			}
			Borders borders = null;
			foreach (SinglePropertyModifierRecord sprm in sprms)
			{
				switch (sprm.Options)
				{
				case 17920:
					if (paragraphFormat.IsFormattingChange)
					{
						ushort ushortValue = sprm.UshortValue;
						if (styleSheet != null && styleSheet.StyleNames.ContainsKey(ushortValue))
						{
							paragraphFormat.SetPropertyValue(47, styleSheet.StyleNames[ushortValue]);
						}
					}
					break;
				case 9221:
				case 9730:
					paragraphFormat.SetPropertyValue(6, sprm.BoolValue);
					break;
				case 9222:
					paragraphFormat.SetPropertyValue(10, sprm.BoolValue);
					break;
				case 9223:
					paragraphFormat.SetPropertyValue(12, sprm.BoolValue);
					break;
				case 9313:
					paragraphFormat.LogicalJustification = (HorizontalAlignment)sprms[9313].ByteValue;
					break;
				case 9219:
					if (sprms[9313] == null)
					{
						paragraphFormat.UpdateJustification(sprms, sprm);
					}
					break;
				case 9228:
					paragraphFormat.SuppressLineNumbers = sprm.BoolValue;
					break;
				case 50701:
				case 50709:
					if (!paragraphFormat.HasValue(30))
					{
						paragraphFormat.UpdateTabs(sprms);
					}
					break;
				case 33885:
					paragraphFormat.SetPropertyValue(3, (float)sprm.ShortValue / 20f);
					break;
				case 33806:
					if (paragraphFormat.Document.WordVersion <= 193)
					{
						paragraphFormat.SetPropertyValue(3, (float)sprm.ShortValue / 20f);
					}
					else if (paragraphFormat.Document.WordVersion <= 217 && !sprms.Contain(33885))
					{
						if (paragraphFormat.Bidi || (sprms.Contain(9281) && sprms[9281].BoolValue))
						{
							paragraphFormat.SetPropertyValue(2, (float)sprm.ShortValue / 20f);
						}
						else
						{
							paragraphFormat.SetPropertyValue(3, (float)sprm.ShortValue / 20f);
						}
					}
					break;
				case 33807:
					if (paragraphFormat.Document.WordVersion <= 193)
					{
						paragraphFormat.SetPropertyValue(2, (float)sprm.ShortValue / 20f);
					}
					else if (paragraphFormat.Document.WordVersion <= 217 && !sprms.Contain(33886))
					{
						if (paragraphFormat.Bidi || (sprms.Contain(9281) && sprms[9281].BoolValue))
						{
							paragraphFormat.SetPropertyValue(3, (float)sprm.ShortValue / 20f);
						}
						else
						{
							paragraphFormat.SetPropertyValue(2, (float)sprm.ShortValue / 20f);
						}
					}
					break;
				case 33886:
					paragraphFormat.SetPropertyValue(2, (float)sprm.ShortValue / 20f);
					break;
				case 17936:
				case 33809:
				case 33888:
					paragraphFormat.SetPropertyValue(5, (float)sprm.ShortValue / 20f);
					break;
				case 25618:
				{
					LineSpacingDescriptor lineSpacingDescriptor = new LineSpacingDescriptor();
					byte[] byteArray = sprm.ByteArray;
					if (byteArray != null)
					{
						lineSpacingDescriptor.Parse(byteArray);
					}
					paragraphFormat.SetPropertyValue(52, (float)lineSpacingDescriptor.LineSpacing / 20f);
					paragraphFormat.LineSpacingRule = lineSpacingDescriptor.LineSpacingRule;
					break;
				}
				case 42003:
					if (sprm.ShortValue == -1)
					{
						paragraphFormat.SetPropertyValue(8, (float)sprm.ShortValue / 20f);
					}
					else
					{
						paragraphFormat.SetPropertyValue(8, (float)(int)sprm.UshortValue / 20f);
					}
					break;
				case 42004:
					if (sprm.ShortValue == -1)
					{
						paragraphFormat.SetPropertyValue(9, (float)sprm.ShortValue / 20f);
					}
					else
					{
						paragraphFormat.SetPropertyValue(9, (float)(int)sprm.UshortValue / 20f);
					}
					break;
				case 33816:
					paragraphFormat.FrameX = (paragraphFormat.IsFrameXAlign(sprm.ShortValue) ? ((float)sprm.ShortValue) : ((float)sprm.ShortValue / 20f));
					break;
				case 33817:
					paragraphFormat.FrameY = (paragraphFormat.IsFrameYAlign(sprm.ShortValue) ? ((float)sprm.ShortValue) : ((float)sprm.ShortValue / 20f));
					break;
				case 33818:
					paragraphFormat.FrameWidth = (float)sprm.ShortValue / 20f;
					break;
				case 9755:
				{
					byte byteValue2 = sprm.ByteValue;
					byte frameVerticalPos = (byte)((uint)(byteValue2 >> 4) & 3u);
					byte frameHorizontalPos = (byte)(byteValue2 >> 6);
					paragraphFormat.FrameVerticalPos = frameVerticalPos;
					paragraphFormat.FrameHorizontalPos = frameHorizontalPos;
					break;
				}
				case 9251:
					paragraphFormat.WrapFrameAround = (FrameWrapMode)sprm.ByteValue;
					break;
				case 9258:
					paragraphFormat.SuppressAutoHyphens = sprm.BoolValue;
					break;
				case 17451:
					paragraphFormat.FrameHeight = (float)sprm.ShortValue / 20f;
					break;
				case 17452:
				{
					byte byteValue = sprm.ByteValue;
					paragraphFormat.DropCap = (DropCapType)(byteValue & 7);
					paragraphFormat.DropCapLines = byteValue >> 3;
					break;
				}
				case 17453:
					if (!sprms.Contain(50765))
					{
						ShadingDescriptor shading2 = CharacterPropertiesConverter.GetShading(sprm);
						paragraphFormat.BackColor = shading2.BackColor;
						paragraphFormat.TextureStyle = shading2.Pattern;
						paragraphFormat.ForeColor = shading2.ForeColor;
					}
					break;
				case 33838:
					paragraphFormat.FrameVerticalDistanceFromText = (float)sprm.ShortValue / 20f;
					break;
				case 33839:
					paragraphFormat.FrameHorizontalDistanceFromText = (float)sprm.ShortValue / 20f;
					break;
				case 9264:
					paragraphFormat.LockFrameAnchor = sprm.BoolValue;
					break;
				case 9265:
					paragraphFormat.WidowControl = sprm.BoolValue;
					break;
				case 9267:
					paragraphFormat.Kinsoku = sprm.BoolValue;
					break;
				case 9268:
					paragraphFormat.WordWrap = sprm.BoolValue;
					break;
				case 9269:
					paragraphFormat.OverflowPunctuation = sprm.BoolValue;
					break;
				case 9270:
					paragraphFormat.TopLinePunctuation = sprm.BoolValue;
					break;
				case 9271:
					paragraphFormat.AutoSpaceDE = sprm.BoolValue;
					break;
				case 9272:
					paragraphFormat.AutoSpaceDN = sprm.BoolValue;
					break;
				case 17465:
					paragraphFormat.BaseLineAlignment = (BaseLineAlignment)sprm.ByteValue;
					break;
				case 17466:
					paragraphFormat.TextDirection = (byte)(sprm.ByteValue & 7u);
					break;
				case 9792:
					paragraphFormat.OutlineLevel = (OutlineLevel)sprm.ByteValue;
					break;
				case 9281:
					paragraphFormat.UpdateBiDi(sprm.BoolValue);
					UpdateDirectParagraphFormatting(paragraphFormat, sprms);
					break;
				case 9283:
					if (paragraphFormat.m_unParsedSprms == null)
					{
						paragraphFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					paragraphFormat.m_unParsedSprms.Add(sprm);
					break;
				case 50757:
					if (paragraphFormat.m_unParsedSprms == null)
					{
						paragraphFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					paragraphFormat.m_unParsedSprms.Add(sprm);
					break;
				case 9287:
					paragraphFormat.SnapToGrid = sprm.BoolValue;
					break;
				case 9288:
					paragraphFormat.AdjustRightIndent = sprm.BoolValue;
					break;
				case 50765:
				{
					ShadingDescriptor shading = CharacterPropertiesConverter.GetShading(sprm);
					paragraphFormat.BackColor = shading.BackColor;
					paragraphFormat.TextureStyle = shading.Pattern;
					paragraphFormat.ForeColor = shading.ForeColor;
					break;
				}
				case 17493:
					paragraphFormat.RightIndentChars = (float)sprm.ShortValue / 100f;
					break;
				case 17494:
					paragraphFormat.LeftIndentChars = (float)sprm.ShortValue / 100f;
					break;
				case 17495:
					paragraphFormat.FirstLineIndentChars = (float)sprm.ShortValue / 100f;
					break;
				case 17496:
					paragraphFormat.BeforeLines = (float)sprm.ShortValue / 100f;
					break;
				case 17497:
					paragraphFormat.AfterLines = (float)sprm.ShortValue / 100f;
					break;
				case 9306:
				case 9307:
					paragraphFormat.SpaceBeforeAuto = sprm.ByteValue == 1;
					break;
				case 9308:
					paragraphFormat.SpaceAfterAuto = sprm.ByteValue == 1;
					break;
				case 9314:
					paragraphFormat.SuppressOverlap = sprm.ByteValue != 1;
					break;
				case 9828:
					paragraphFormat.IsFormattingChange = false;
					paragraphFormat.IsChangedFormat = true;
					break;
				case 9325:
					paragraphFormat.ContextualSpacing = sprm.BoolValue;
					break;
				case 50799:
				{
					short key = BitConverter.ToInt16(sprm.ByteArray, 1);
					if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(key))
					{
						paragraphFormat.FormatChangeAuthorName = authorNames[key];
					}
					DateTime formatChangeDateTime = paragraphFormat.ParseDTTM(BitConverter.ToInt32(sprm.ByteArray, 3));
					if (formatChangeDateTime.Year < 1900)
					{
						formatChangeDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
					}
					paragraphFormat.FormatChangeDateTime = formatChangeDateTime;
					break;
				}
				case 9328:
					paragraphFormat.MirrorIndents = sprm.BoolValue;
					break;
				case 9329:
					paragraphFormat.TextboxTightWrap = (TextboxTightWrapOptions)sprm.ByteValue;
					break;
				case 25636:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprms, sprm, 50766, borders.Top, paragraphFormat);
					break;
				case 50766:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprm, borders.Top, paragraphFormat);
					if (borders.Top.BorderType == BorderStyle.Cleared)
					{
						borders.Top.PropertiesHash.Clear();
					}
					break;
				case 25637:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprms, sprm, 50767, borders.Left, paragraphFormat);
					break;
				case 50767:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprm, borders.Left, paragraphFormat);
					if (borders.Left.BorderType == BorderStyle.Cleared)
					{
						borders.Left.PropertiesHash.Clear();
					}
					break;
				case 25639:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprms, sprm, 50769, borders.Right, paragraphFormat);
					break;
				case 50769:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprm, borders.Right, paragraphFormat);
					if (borders.Right.BorderType == BorderStyle.Cleared)
					{
						borders.Right.PropertiesHash.Clear();
					}
					break;
				case 25638:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprms, sprm, 50768, borders.Bottom, paragraphFormat);
					break;
				case 50768:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprm, borders.Bottom, paragraphFormat);
					if (borders.Bottom.BorderType == BorderStyle.Cleared)
					{
						borders.Bottom.PropertiesHash.Clear();
					}
					break;
				case 25640:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprms, sprm, 50770, borders.Horizontal, paragraphFormat);
					break;
				case 50770:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprm, borders.Horizontal, paragraphFormat);
					if (borders.Horizontal.BorderType == BorderStyle.Cleared)
					{
						borders.Horizontal.PropertiesHash.Clear();
					}
					break;
				case 26153:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprms, sprm, 50771, borders.Vertical, paragraphFormat);
					break;
				case 50771:
					paragraphFormat.UpdateOldFormatBorders(ref borders);
					borders = (paragraphFormat.IsFormattingChange ? borders : paragraphFormat.Borders);
					ExportBorder(sprm, borders.Vertical, paragraphFormat);
					if (borders.Vertical.BorderType == BorderStyle.Cleared)
					{
						borders.Vertical.PropertiesHash.Clear();
					}
					break;
				}
			}
			if ((sprms.Contain(9828) && sprms.GetNewSprm(17920, 9828) != null) || paragraphFormat.OldPropertiesHash.Count <= 0 || paragraphFormat.PropertiesHash.Count <= 0)
			{
				return;
			}
			foreach (KeyValuePair<int, object> item in paragraphFormat.OldPropertiesHash)
			{
				if (item.Key == 20 && paragraphFormat.PropertiesHash.ContainsKey(20))
				{
					CopyBorders((Borders)item.Value, paragraphFormat);
				}
				if (!paragraphFormat.PropertiesHash.ContainsKey(item.Key))
				{
					paragraphFormat.PropertiesHash.Add(item.Key, item.Value);
				}
			}
		}
	}

	internal static void UpdateDirectParagraphFormatting(WParagraphFormat paragraphFormat, SinglePropertyModifierArray sprms)
	{
		if (sprms[9313] != null)
		{
			paragraphFormat.LogicalJustification = (HorizontalAlignment)sprms[9313].ByteValue;
		}
		else if (sprms[9219] != null)
		{
			paragraphFormat.UpdateJustification(sprms, sprms[9219]);
		}
	}

	internal static void CopyBorders(Borders borders, WParagraphFormat paragraphFormat)
	{
		Borders borders2 = paragraphFormat.Borders;
		if (borders.Top.IsBorderDefined && !borders2.Top.IsBorderDefined)
		{
			ExportBorder(borders2.Top, borders.Top);
		}
		if (borders.Bottom.IsBorderDefined && !borders2.Bottom.IsBorderDefined)
		{
			ExportBorder(borders2.Bottom, borders.Bottom);
		}
		if (borders.Right.IsBorderDefined && !borders2.Right.IsBorderDefined)
		{
			ExportBorder(borders2.Right, borders.Right);
		}
		if (borders.Left.IsBorderDefined && !borders2.Left.IsBorderDefined)
		{
			ExportBorder(borders2.Left, borders.Left);
		}
		if (borders.Horizontal.IsBorderDefined && !borders2.Horizontal.IsBorderDefined)
		{
			ExportBorder(borders2.Horizontal, borders.Horizontal);
		}
		if (borders.Vertical.IsBorderDefined && !borders2.Vertical.IsBorderDefined)
		{
			ExportBorder(borders2.Vertical, borders.Vertical);
		}
	}

	internal static void FormatToSprms(WParagraphFormat paragraphFormat, SinglePropertyModifierArray sprms, WordStyleSheet styleSheet)
	{
		lock (m_threadLocker)
		{
			sprms.Clear();
			Dictionary<int, object> dictionary = new Dictionary<int, object>();
			if (paragraphFormat.PropertiesHash.Count > 0)
			{
				dictionary = new Dictionary<int, object>(paragraphFormat.PropertiesHash);
			}
			if (paragraphFormat.OldPropertiesHash.Count > 0)
			{
				Dictionary<int, object> dictionary2 = new Dictionary<int, object>(paragraphFormat.OldPropertiesHash);
				foreach (KeyValuePair<int, object> item in dictionary2)
				{
					FormatToSprms(item.Key, item.Value, sprms, paragraphFormat, styleSheet, dictionary2);
					if (dictionary.ContainsKey(item.Key) && dictionary[item.Key] == item.Value)
					{
						dictionary.Remove(item.Key);
					}
				}
			}
			if (dictionary.ContainsKey(65))
			{
				FormatToSprms(65, dictionary[65], sprms, paragraphFormat, styleSheet, dictionary);
				dictionary.Remove(65);
			}
			if (dictionary.ContainsKey(47))
			{
				dictionary.Remove(47);
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray();
			foreach (KeyValuePair<int, object> item2 in dictionary)
			{
				FormatToSprms(item2.Key, item2.Value, singlePropertyModifierArray, paragraphFormat, styleSheet, dictionary);
			}
			for (int i = 0; i < singlePropertyModifierArray.Count; i++)
			{
				sprms.Add(singlePropertyModifierArray.GetSprmByIndex(i).Clone());
			}
			singlePropertyModifierArray.Clear();
		}
	}

	internal static void FormatToSprms(int propKey, object value, SinglePropertyModifierArray sprms, WParagraphFormat paragraphFormat, WordStyleSheet styleSheet, Dictionary<int, object> propertyHash)
	{
		lock (m_threadLocker)
		{
			ushort num = 0;
			int sprmOption = paragraphFormat.GetSprmOption(propKey);
			switch (propKey)
			{
			case 6:
			case 10:
			case 11:
			case 12:
			case 31:
			case 35:
			case 38:
			case 39:
			case 40:
			case 41:
			case 42:
			case 65:
			case 75:
			case 78:
			case 80:
			case 81:
			case 82:
			case 89:
			case 92:
				sprms.SetBoolValue(sprmOption, (bool)value);
				break;
			case 54:
			{
				byte b = (((bool)value) ? ((byte)1) : ((byte)0));
				sprms.SetByteValue(sprmOption, b);
				if (b == 1)
				{
					sprms.SetUShortValue(42003, 100);
				}
				break;
			}
			case 55:
			{
				byte b4 = (((bool)value) ? ((byte)1) : ((byte)0));
				sprms.SetByteValue(sprmOption, b4);
				if (b4 == 1)
				{
					sprms.SetUShortValue(42004, 100);
				}
				break;
			}
			case 36:
			{
				byte value4 = ((!(bool)value) ? ((byte)1) : ((byte)0));
				sprms.SetByteValue(sprmOption, value4);
				break;
			}
			case 8:
			case 9:
				num = (ushort)Math.Round((float)value * 20f);
				sprms.SetUShortValue(sprmOption, num);
				break;
			case 76:
			case 77:
			case 83:
			case 84:
				sprms.SetShortValue(sprmOption, (short)Math.Round((float)value * 20f));
				break;
			case 2:
			{
				short value2 = (short)Math.Round((float)value * 20f);
				sprms.SetShortValue(33886, value2);
				if (!paragraphFormat.Bidi)
				{
					sprms.SetShortValue(33807, value2);
				}
				else
				{
					sprms.SetShortValue(33806, value2);
				}
				break;
			}
			case 3:
			{
				short value3 = (short)Math.Round((float)value * 20f);
				sprms.SetShortValue(33885, value3);
				if (!paragraphFormat.Bidi)
				{
					sprms.SetShortValue(33806, value3);
				}
				else
				{
					sprms.SetShortValue(33807, value3);
				}
				break;
			}
			case 5:
			{
				short value5 = (short)Math.Round((float)value * 20f);
				sprms.SetShortValue(33809, value5);
				sprms.SetShortValue(33888, value5);
				break;
			}
			case 71:
			case 72:
			{
				byte b2 = 3;
				if (paragraphFormat.HasValue(71))
				{
					b2 = paragraphFormat.FrameHorizontalPos;
				}
				byte b3 = 3;
				if (paragraphFormat.HasValue(72))
				{
					b3 = paragraphFormat.FrameVerticalPos;
				}
				if (sprms[9755] == null)
				{
					sprms.SetByteValue(9755, (byte)(((b2 << 2) | b3) << 4));
				}
				break;
			}
			case 73:
			case 74:
				if ((double)(float)value == Math.Floor((float)value) && ((propKey == 73) ? paragraphFormat.IsFrameXAlign((float)value) : paragraphFormat.IsFrameYAlign((float)value)))
				{
					sprms.SetShortValue(sprmOption, Convert.ToInt16(value));
				}
				else
				{
					sprms.SetShortValue(sprmOption, (short)(float)Math.Round((float)value * 20f));
				}
				break;
			case 88:
				sprms.SetByteValue(sprmOption, (byte)(FrameWrapMode)value);
				break;
			case 86:
				sprms.SetShortValue(sprmOption, (short)Math.Round((float)value * 100f));
				break;
			case 85:
				sprms.SetShortValue(sprmOption, (short)Math.Round((float)value * 100f));
				break;
			case 87:
				sprms.SetShortValue(sprmOption, (short)Math.Round((float)value * 100f));
				break;
			case 91:
				sprms.SetShortValue(sprmOption, (short)Math.Round((float)value * 100f));
				break;
			case 90:
				sprms.SetShortValue(sprmOption, (short)Math.Round((float)value * 100f));
				break;
			case 52:
			{
				LineSpacingDescriptor lineSpacingDescriptor = new LineSpacingDescriptor();
				lineSpacingDescriptor.LineSpacing = (short)Math.Round((float)value * 20f);
				lineSpacingDescriptor.LineSpacingRule = paragraphFormat.LineSpacingRule;
				sprms.SetByteArrayValue(25618, lineSpacingDescriptor.Save());
				break;
			}
			case 30:
				ImportTabs((TabCollection)value, sprms);
				break;
			case 0:
				sprms.SetByteValue(9313, (byte)value);
				break;
			case 21:
			case 32:
			case 33:
				ImportShading(propertyHash, sprms);
				break;
			case 20:
				ImportBorders((Borders)value, sprms);
				break;
			case 56:
				sprms.SetByteValue(sprmOption, (byte)value);
				break;
			case 37:
				sprms.SetByteValue(sprmOption, (byte)(TextboxTightWrapOptions)value);
				break;
			case 34:
				sprms.SetUShortValue(sprmOption, (ushort)(BaseLineAlignment)value);
				break;
			case 43:
			case 44:
				sprms.SetShortValue(sprmOption, (short)(((byte)paragraphFormat.DropCapLines << 3) | (byte)paragraphFormat.DropCap));
				break;
			case 48:
			{
				byte[] bytes3 = BitConverter.GetBytes((short)(0 | (byte)value));
				sprms.SetByteArrayValue(sprmOption, bytes3);
				break;
			}
			case 45:
			case 46:
				if (!sprms.Contain(50799) && paragraphFormat.IsChangedFormat)
				{
					byte[] array = new byte[7] { 1, 0, 0, 0, 0, 0, 0 };
					if (!CharacterPropertiesConverter.AuthorNames.Contains(paragraphFormat.FormatChangeAuthorName))
					{
						CharacterPropertiesConverter.AuthorNames.Add(paragraphFormat.FormatChangeAuthorName);
					}
					byte[] bytes = BitConverter.GetBytes((short)CharacterPropertiesConverter.AuthorNames.IndexOf(paragraphFormat.FormatChangeAuthorName));
					byte[] bytes2 = BitConverter.GetBytes(paragraphFormat.GetDTTMIntValue(paragraphFormat.FormatChangeDateTime));
					Buffer.BlockCopy(bytes, 0, array, 1, 2);
					Buffer.BlockCopy(bytes2, 0, array, 3, 4);
					sprms.SetByteArrayValue(50799, array);
				}
				break;
			case 47:
			{
				int num2 = styleSheet.StyleNameToIndex((string)value, isCharacter: false);
				if (num2 > -1)
				{
					sprms.SetByteArrayValue(17920, BitConverter.GetBytes((ushort)num2));
				}
				break;
			}
			case 1:
			case 4:
			case 7:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 22:
			case 23:
			case 24:
			case 25:
			case 26:
			case 27:
			case 28:
			case 29:
			case 49:
			case 50:
			case 51:
			case 53:
			case 57:
			case 58:
			case 59:
			case 60:
			case 61:
			case 62:
			case 63:
			case 64:
			case 66:
			case 67:
			case 68:
			case 69:
			case 70:
			case 79:
				break;
			}
		}
	}

	private static void ImportTabs(TabCollection tabCollection, SinglePropertyModifierArray sprms)
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < tabCollection.Count; i++)
		{
			if (tabCollection[i].DeletePosition == 0f)
			{
				num++;
			}
		}
		TabsInfo tabsInfo = new TabsInfo((byte)num);
		short[] array = new short[(byte)tabCollection.Count - num];
		int j = 0;
		for (int count = tabCollection.Count; j < count; j++)
		{
			Tab tab = tabCollection[j];
			if (tab.DeletePosition != 0f)
			{
				flag = true;
				array[num2] = (short)tab.DeletePosition;
				num2++;
			}
			else if (num > 0)
			{
				tabsInfo.TabPositions[num3] = (short)Math.Round(tab.Position * 20f);
				tabsInfo.Descriptors[num3] = new TabDescriptor(tab.Justification, tab.TabLeader);
				num3++;
			}
		}
		if (flag)
		{
			tabsInfo.TabDeletePositions = array;
		}
		tabsInfo.Save(sprms, 50701);
	}

	private static void ImportShading(Dictionary<int, object> propertyHash, SinglePropertyModifierArray sprms)
	{
		ShadingDescriptor shadingDescriptor = new ShadingDescriptor();
		if (propertyHash.ContainsKey(21))
		{
			shadingDescriptor.BackColor = (Color)propertyHash[21];
		}
		if (propertyHash.ContainsKey(32))
		{
			shadingDescriptor.ForeColor = (Color)propertyHash[32];
		}
		if (propertyHash.ContainsKey(33))
		{
			shadingDescriptor.Pattern = (TextureStyle)propertyHash[33];
		}
		sprms.SetShortValue(17453, shadingDescriptor.Save());
		sprms.SetByteArrayValue(50765, shadingDescriptor.SaveNewShd());
	}

	private static void ImportBorders(Borders borders, SinglePropertyModifierArray sprms)
	{
		if (!borders.IsDefault)
		{
			BorderCode borderCode = new BorderCode();
			if (!borders.Top.IsDefault && borders.Top.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Top);
				SetBorderSprms(25636, 50766, sprms, borderCode);
			}
			if (!borders.Left.IsDefault && borders.Left.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Left);
				SetBorderSprms(25637, 50767, sprms, borderCode);
			}
			if (!borders.Bottom.IsDefault && borders.Bottom.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Bottom);
				SetBorderSprms(25638, 50768, sprms, borderCode);
			}
			if (!borders.Right.IsDefault && borders.Right.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Right);
				SetBorderSprms(25639, 50769, sprms, borderCode);
			}
			if (!borders.Horizontal.IsDefault && borders.Horizontal.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Horizontal);
				SetBorderSprms(25640, 50770, sprms, borderCode);
			}
			if (!borders.Vertical.IsDefault && borders.Vertical.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Vertical);
				SetBorderSprms(26153, 50771, sprms, borderCode);
			}
		}
	}

	private static void SetBorderSprms(int oldSprmOPtion, int newSprmOption, SinglePropertyModifierArray sprms, BorderCode brc)
	{
		byte[] array = new byte[4];
		brc.SaveBytes(array, 0);
		sprms.SetByteArrayValue(oldSprmOPtion, array);
		array = new byte[8];
		brc.SaveNewBrc(array, 0);
		sprms.SetByteArrayValue(newSprmOption, array);
	}
}
