using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class SectionPropertiesConverter
{
	public static void Export(WordReader reader, WSection destination, bool parseAll)
	{
		SectionProperties sectionProperties = reader.SectionProperties;
		List<SinglePropertyModifierRecord> modifiers = sectionProperties.Sprms.Modifiers;
		WPageSetup pageSetup = destination.PageSetup;
		List<SinglePropertyModifierRecord> list = new List<SinglePropertyModifierRecord>();
		UpdatePageOrientation(sectionProperties.Sprms, pageSetup);
		if (sectionProperties.Sprms.Contain(12857))
		{
			destination.SectionFormat.IsFormattingChange = true;
			destination.PageSetup.IsFormattingChange = true;
			destination.PageSetup.PageNumbers.IsFormattingChange = true;
			destination.PageSetup.Margins.IsFormattingChange = true;
			destination.PageSetup.Borders.IsFormattingChange = true;
			destination.Document.SetDefaultSectionFormatting(destination);
			for (int num = sectionProperties.Sprms.Modifiers.Count - 1; num > 0; num--)
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord = sectionProperties.Sprms.Modifiers[num];
				_ = singlePropertyModifierRecord.TypedOptions;
				if (singlePropertyModifierRecord.TypedOptions == 12857)
				{
					break;
				}
				list.Add(singlePropertyModifierRecord);
				sectionProperties.Sprms.Modifiers.Remove(singlePropertyModifierRecord);
			}
			destination.PageSetup.PageNumberStyle = (PageNumberStyle)sectionProperties.PageNfc;
			destination.PageSetup.RestartPageNumbering = sectionProperties.PageRestart;
			destination.PageSetup.PageNumbers.ChapterPageSeparator = (ChapterPageSeparatorType)sectionProperties.ChapterPageSeparator;
			destination.PageSetup.PageNumbers.HeadingLevelForChapter = (HeadingLevel)sectionProperties.HeadingLevelForChapter;
			if (sectionProperties.PageRestart)
			{
				destination.PageSetup.PageStartingNumber = sectionProperties.PageStartAt;
			}
		}
		ImportSecSprmToFormat(sectionProperties, modifiers, destination, reader.SttbfRMarkAuthorNames, pageSetup);
		if (list.Count > 0)
		{
			sectionProperties.Sprms.Modifiers.AddRange(list);
			destination.Document.SetDefaultSectionFormatting(destination);
			ImportSecSprmToFormat(sectionProperties, list, destination, reader.SttbfRMarkAuthorNames, pageSetup);
		}
		destination.PageSetup.EqualColumnWidth = sectionProperties.Columns.ColumnsEvenlySpaced;
		if (sectionProperties.Sprms.Contain(12857))
		{
			destination.PageSetup.IsFormattingChange = true;
			destination.PageSetup.EqualColumnWidth = sectionProperties.OldColumns.ColumnsEvenlySpaced;
			destination.PageSetup.IsFormattingChange = false;
		}
		destination.PageSetup.PageNumberStyle = (PageNumberStyle)sectionProperties.PageNfc;
		destination.PageSetup.RestartPageNumbering = sectionProperties.PageRestart;
		destination.PageSetup.PageNumbers.ChapterPageSeparator = (ChapterPageSeparatorType)sectionProperties.ChapterPageSeparator;
		destination.PageSetup.PageNumbers.HeadingLevelForChapter = (HeadingLevel)sectionProperties.HeadingLevelForChapter;
		if (sectionProperties.PageRestart)
		{
			destination.PageSetup.PageStartingNumber = sectionProperties.PageStartAt;
		}
		ColumnDescriptor columnDescriptor = null;
		int i = 0;
		for (int count = sectionProperties.Columns.Count; i < count; i++)
		{
			columnDescriptor = sectionProperties.Columns[i];
			if (columnDescriptor.Width != 0)
			{
				Column column = destination.AddColumn(0f, 0f, isOpening: true);
				column.Width = (float)(int)columnDescriptor.Width / 20f;
				column.Space = (float)(int)columnDescriptor.Space / 20f;
			}
		}
		destination.SectionFormat.SectFormattingColumnCollection = new ColumnCollection(destination);
		int j = 0;
		for (int count2 = sectionProperties.OldColumns.Count; j < count2; j++)
		{
			columnDescriptor = sectionProperties.OldColumns[j];
			if (columnDescriptor.Width != 0)
			{
				Column column2 = new Column(destination.Document);
				column2.IsFormattingChange = true;
				column2.Width = 0f;
				column2.Space = 0f;
				destination.SectionFormat.SectFormattingColumnCollection.Add(column2, isOpening: true);
				column2.Width = (float)(int)columnDescriptor.Width / 20f;
				column2.Space = (float)(int)columnDescriptor.Space / 20f;
				column2.IsFormattingChange = false;
			}
		}
		if (parseAll)
		{
			SinglePropertyModifierArray copiableSprm = sectionProperties.GetCopiableSprm();
			destination.DataArray = new byte[copiableSprm.Length];
			copiableSprm.Save(destination.DataArray, 0);
		}
	}

	private static void ImportSecSprmToFormat(SectionProperties source, List<SinglePropertyModifierRecord> modifiers, WSection destination, Dictionary<int, string> authorNames, WPageSetup pageSetup)
	{
		Borders borders = null;
		int count = modifiers.Count;
		for (int i = 0; i < count; i++)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = modifiers[i];
			switch (singlePropertyModifierRecord.TypedOptions)
			{
			case 45087:
				pageSetup.PageSize = new SizeF((float)(int)source.PageWidth / 20f, (!pageSetup.IsFormattingChange) ? pageSetup.PageSize.Height : (pageSetup.OldPropertiesHash.ContainsKey(5) ? ((SizeF)pageSetup.OldPropertiesHash[5]).Height : pageSetup.PageSize.Height));
				break;
			case 45088:
				pageSetup.PageSize = new SizeF((!pageSetup.IsFormattingChange) ? pageSetup.PageSize.Width : (pageSetup.OldPropertiesHash.ContainsKey(5) ? ((SizeF)pageSetup.OldPropertiesHash[5]).Width : pageSetup.PageSize.Width), (float)(int)source.PageHeight / 20f);
				break;
			case 45079:
				pageSetup.SetPageSetupProperty("HeaderDistance", (float)source.HeaderHeight / 20f);
				break;
			case 45080:
				pageSetup.SetPageSetupProperty("FooterDistance", (float)source.FooterHeight / 20f);
				break;
			case 12314:
				pageSetup.VerticalAlignment = (PageAlignment)source.VerticalAlignment;
				break;
			case 12317:
				pageSetup.Orientation = (PageOrientation)source.Orientation;
				break;
			case 36900:
				pageSetup.Margins.Bottom = (float)source.BottomMargin / 20f;
				break;
			case 36899:
				pageSetup.Margins.Top = (float)source.TopMargin / 20f;
				break;
			case 45089:
				pageSetup.Margins.Left = (float)source.LeftMargin / 20f;
				break;
			case 45090:
				pageSetup.Margins.Right = (float)source.RightMargin / 20f;
				break;
			case 45093:
				pageSetup.Margins.Gutter = (float)source.Gutter / 20f;
				break;
			case 12297:
				destination.BreakCode = (SectionBreakCode)source.BreakCode;
				break;
			case 12298:
				pageSetup.DifferentFirstPage = source.TitlePage;
				break;
			case 20487:
				pageSetup.FirstPageTray = (PrinterPaperTray)source.FirstPageTray;
				break;
			case 20488:
				pageSetup.OtherPagesTray = (PrinterPaperTray)source.OtherPagesTray;
				break;
			case 20501:
				pageSetup.SetPageSetupProperty("LineNumberingStep", (int)source.LineNumberingStep);
				break;
			case 12307:
			{
				bool flag = true;
				if (source.HasOptions(20501) && source.LineNumberingMode != LineNumberingMode.None)
				{
					pageSetup.SetPageSetupProperty("LineNumberingMode", source.LineNumberingMode);
					if (source.Sprms.Contain(12857))
					{
						if (source.LineNumberingStep != 0)
						{
							pageSetup.SetPageSetupProperty("LineNumberingStep", (int)source.LineNumberingStep);
						}
						if (source.LineNumberingStartValue != 0)
						{
							pageSetup.SetPageSetupProperty("LineNumberingStartValue", (int)source.LineNumberingStartValue);
						}
						if (source.LineNumberingDistanceFromText != 0)
						{
							pageSetup.SetPageSetupProperty("LineNumberingDistanceFromText", (float)source.LineNumberingDistanceFromText / 20f);
						}
						flag = false;
					}
				}
				if (flag)
				{
					if (source.LineNumberingStep != 0)
					{
						pageSetup.SetPageSetupProperty("LineNumberingStep", (int)source.LineNumberingStep);
					}
					if (source.LineNumberingStartValue != 0)
					{
						pageSetup.SetPageSetupProperty("LineNumberingStartValue", (int)source.LineNumberingStartValue);
					}
					if (source.LineNumberingDistanceFromText != 0)
					{
						pageSetup.SetPageSetupProperty("LineNumberingDistanceFromText", (float)source.LineNumberingDistanceFromText / 20f);
					}
				}
				break;
			}
			case 21039:
				pageSetup.PageBordersApplyType = source.PageBorderApply;
				pageSetup.IsFrontPageBorder = source.PageBorderIsInFront;
				pageSetup.PageBorderOffsetFrom = source.PageBorderOffsetFrom;
				break;
			case 12840:
				pageSetup.Bidi = source.Bidi;
				break;
			case 28715:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Top);
				break;
			case 53812:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Top);
				break;
			case 28717:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Bottom);
				break;
			case 53814:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Bottom);
				break;
			case 28716:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Left);
				break;
			case 53813:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Left);
				break;
			case 28718:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Right);
				break;
			case 53815:
				borders = GetDestBorders(borders, pageSetup);
				ExportBorder(source.GetBorder(singlePropertyModifierRecord), borders.Right);
				break;
			case 36913:
				pageSetup.LinePitch = (float)(int)source.LinePitch / 20f;
				break;
			case 20530:
				pageSetup.PitchType = (GridPitchType)source.PitchType;
				break;
			case 12313:
				pageSetup.DrawLinesBetweenCols = source.DrawLinesBetweenCols;
				break;
			case 12294:
				destination.ProtectForm = !source.ProtectForm;
				break;
			case 20531:
				destination.TextDirection = (DocTextDirection)source.TextDirection;
				break;
			case 12350:
				destination.PageSetup.RestartIndexForEndnote = (EndnoteRestartIndex)source.RestartIndexForEndnote;
				break;
			case 12348:
				destination.PageSetup.RestartIndexForFootnotes = (FootnoteRestartIndex)source.RestartIndexForFootnotes;
				break;
			case 20543:
				destination.PageSetup.InitialFootnoteNumber = source.InitialFootnoteNumber;
				break;
			case 20545:
				destination.PageSetup.InitialEndnoteNumber = source.InitialEndnoteNumber;
				break;
			case 20544:
				if (destination.Document.WordVersion <= 217)
				{
					destination.PageSetup.FootnoteNumberFormat = destination.Document.FootnoteNumberFormat;
				}
				else
				{
					destination.PageSetup.FootnoteNumberFormat = (FootEndNoteNumberFormat)source.FootnoteNumberFormat;
				}
				break;
			case 20546:
				if (destination.Document.WordVersion <= 217)
				{
					destination.PageSetup.EndnoteNumberFormat = destination.Document.EndnoteNumberFormat;
				}
				else
				{
					destination.PageSetup.EndnoteNumberFormat = (FootEndNoteNumberFormat)source.EndnoteNumberFormat;
				}
				break;
			case 12347:
				destination.PageSetup.FootnotePosition = (FootnotePosition)source.FootnotePosition;
				break;
			case 12857:
				destination.SectionFormat.IsFormattingChange = false;
				destination.PageSetup.IsFormattingChange = false;
				destination.PageSetup.PageNumbers.IsFormattingChange = false;
				destination.PageSetup.Margins.IsFormattingChange = false;
				destination.PageSetup.Borders.IsFormattingChange = false;
				destination.SectionFormat.IsChangedFormat = true;
				break;
			case 53827:
			{
				short key = BitConverter.ToInt16(singlePropertyModifierRecord.ByteArray, 1);
				if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(key))
				{
					destination.SectionFormat.FormatChangeAuthorName = authorNames[key];
				}
				DateTime formatChangeDateTime = destination.SectionFormat.ParseDTTM(BitConverter.ToInt32(singlePropertyModifierRecord.ByteArray, 3));
				if (formatChangeDateTime.Year < 1900)
				{
					formatChangeDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
				}
				destination.SectionFormat.FormatChangeDateTime = formatChangeDateTime;
				break;
			}
			}
		}
	}

	private static void UpdatePageOrientation(SinglePropertyModifierArray sprms, WPageSetup pageSetup)
	{
		if (sprms != null && !sprms.Contain(12317) && pageSetup.Orientation != 0)
		{
			pageSetup.Orientation = PageOrientation.Portrait;
		}
	}

	public static void Import(SectionProperties destination, WSection source)
	{
		List<SinglePropertyModifierRecord> list = new List<SinglePropertyModifierRecord>();
		if (source.SectionFormat.IsChangedFormat)
		{
			OldFormatToSprms(destination, source);
			int num;
			for (num = 0; num < destination.Sprms.Modifiers.Count - 1; num++)
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord = destination.Sprms.Modifiers[num];
				_ = singlePropertyModifierRecord.TypedOptions;
				if (singlePropertyModifierRecord.TypedOptions == 12857)
				{
					list.Add(singlePropertyModifierRecord);
					destination.Sprms.Modifiers.Remove(singlePropertyModifierRecord);
					break;
				}
				list.Add(singlePropertyModifierRecord);
				destination.Sprms.Modifiers.Remove(singlePropertyModifierRecord);
				num--;
			}
		}
		Import(destination, source, source.SectionFormat.IsChangedFormat);
		if (list.Count > 0)
		{
			list.AddRange(destination.Sprms.Modifiers);
			destination.Sprms.Modifiers.Clear();
			destination.Sprms.Modifiers.AddRange(list);
		}
	}

	private static void OldFormatToSprms(SectionProperties destination, WSection source)
	{
		Dictionary<int, object> dictionary = new Dictionary<int, object>(source.SectionFormat.OldPropertiesHash);
		Dictionary<int, object> dictionary2 = new Dictionary<int, object>(source.PageSetup.OldPropertiesHash);
		Dictionary<int, object> dictionary3 = new Dictionary<int, object>(source.PageSetup.PageNumbers.OldPropertiesHash);
		Dictionary<int, object> dictionary4 = new Dictionary<int, object>(source.PageSetup.Margins.OldPropertiesHash);
		Dictionary<int, object> dictionary5 = new Dictionary<int, object>(source.SectionFormat.PropertiesHash);
		Dictionary<int, object> dictionary6 = new Dictionary<int, object>(source.PageSetup.PropertiesHash);
		Dictionary<int, object> dictionary7 = new Dictionary<int, object>(source.PageSetup.PageNumbers.PropertiesHash);
		Dictionary<int, object> dictionary8 = new Dictionary<int, object>(source.PageSetup.Margins.PropertiesHash);
		ColumnCollection columnCollection = new ColumnCollection(source);
		if (source.SectionFormat.SectFormattingColumnCollection != null)
		{
			for (int i = 0; i < source.Columns.Count; i++)
			{
				columnCollection.Add(source.Columns[i]);
			}
			if (source.Columns.Count > 0)
			{
				source.Columns.InnerList.Clear();
			}
			for (int j = 0; j < source.SectionFormat.SectFormattingColumnCollection.Count; j++)
			{
				Column column = new Column(source.Document);
				source.Columns.Add(column);
				foreach (KeyValuePair<int, object> item in source.SectionFormat.SectFormattingColumnCollection[j].OldPropertiesHash)
				{
					source.Columns[j].PropertiesHash[item.Key] = item.Value;
				}
			}
		}
		source.SectionFormat.PropertiesHash.Clear();
		source.SectionFormat.OldPropertiesHash.Clear();
		source.PageSetup.OldPropertiesHash.Clear();
		source.PageSetup.PropertiesHash.Clear();
		source.PageSetup.PageNumbers.OldPropertiesHash.Clear();
		source.PageSetup.PageNumbers.PropertiesHash.Clear();
		source.PageSetup.Margins.OldPropertiesHash.Clear();
		source.PageSetup.Margins.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item2 in dictionary)
		{
			source.SectionFormat.PropertiesHash[item2.Key] = item2.Value;
		}
		foreach (KeyValuePair<int, object> item3 in dictionary2)
		{
			source.PageSetup.PropertiesHash[item3.Key] = item3.Value;
		}
		foreach (KeyValuePair<int, object> item4 in dictionary3)
		{
			source.PageSetup.PageNumbers.PropertiesHash[item4.Key] = item4.Value;
		}
		foreach (KeyValuePair<int, object> item5 in dictionary4)
		{
			source.PageSetup.Margins.PropertiesHash[item5.Key] = item5.Value;
		}
		source.SectionFormat.IsChangedFormat = true;
		Import(destination, source, source.SectionFormat.IsChangedFormat);
		if (source.Columns.Count > 0)
		{
			source.Columns.InnerList.Clear();
		}
		for (int k = 0; k < columnCollection.Count; k++)
		{
			source.Columns.Add(columnCollection[k]);
		}
		columnCollection.Close();
		source.SectionFormat.PropertiesHash.Clear();
		source.PageSetup.PropertiesHash.Clear();
		source.PageSetup.PageNumbers.PropertiesHash.Clear();
		source.PageSetup.Margins.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item6 in dictionary)
		{
			source.SectionFormat.OldPropertiesHash[item6.Key] = item6.Value;
		}
		foreach (KeyValuePair<int, object> item7 in dictionary5)
		{
			source.SectionFormat.PropertiesHash[item7.Key] = item7.Value;
		}
		foreach (KeyValuePair<int, object> item8 in dictionary2)
		{
			source.PageSetup.OldPropertiesHash[item8.Key] = item8.Value;
		}
		foreach (KeyValuePair<int, object> item9 in dictionary6)
		{
			source.PageSetup.PropertiesHash[item9.Key] = item9.Value;
		}
		foreach (KeyValuePair<int, object> item10 in dictionary3)
		{
			source.PageSetup.PageNumbers.OldPropertiesHash[item10.Key] = item10.Value;
		}
		foreach (KeyValuePair<int, object> item11 in dictionary7)
		{
			source.PageSetup.PageNumbers.PropertiesHash[item11.Key] = item11.Value;
		}
		foreach (KeyValuePair<int, object> item12 in dictionary4)
		{
			source.PageSetup.Margins.OldPropertiesHash[item12.Key] = item12.Value;
		}
		foreach (KeyValuePair<int, object> item13 in dictionary8)
		{
			source.PageSetup.Margins.PropertiesHash[item13.Key] = item13.Value;
		}
		destination.IsChangedFormat = true;
		source.SectionFormat.PropertiesHash.Remove(4);
	}

	internal static void Import(SectionProperties destination, WSection source, bool format)
	{
		if (source.PageSetup.PageSize.Width != 0f)
		{
			destination.PageWidth = (ushort)Math.Round(source.PageSetup.PageSize.Width * 20f);
		}
		if (source.PageSetup.PageSize.Height != 0f)
		{
			destination.PageHeight = (ushort)Math.Round(source.PageSetup.PageSize.Height * 20f);
		}
		if (source.PageSetup.VerticalAlignment != 0)
		{
			destination.VerticalAlignment = (byte)source.PageSetup.VerticalAlignment;
		}
		if (source.PageSetup.Orientation != 0)
		{
			destination.Orientation = (byte)source.PageSetup.Orientation;
		}
		destination.TextDirection = (byte)source.TextDirection;
		destination.LeftMargin = (short)Math.Round(source.PageSetup.Margins.Left * 20f);
		destination.RightMargin = (short)Math.Round(source.PageSetup.Margins.Right * 20f);
		destination.TopMargin = (short)Math.Round(source.PageSetup.Margins.Top * 20f);
		destination.BottomMargin = (short)Math.Round(source.PageSetup.Margins.Bottom * 20f);
		destination.Gutter = (short)Math.Round(source.PageSetup.Margins.Gutter * 20f);
		destination.HeaderHeight = (short)Math.Round(source.PageSetup.HeaderDistance * 20f);
		destination.FooterHeight = (short)Math.Round(source.PageSetup.FooterDistance * 20f);
		if (source.PageSetup.FirstPageTray > PrinterPaperTray.DefaultBin)
		{
			destination.FirstPageTray = (ushort)source.PageSetup.FirstPageTray;
		}
		if (source.PageSetup.OtherPagesTray > PrinterPaperTray.DefaultBin)
		{
			destination.OtherPagesTray = (ushort)source.PageSetup.OtherPagesTray;
		}
		if (source.SectionFormat.IsChangedFormat)
		{
			byte[] array = new byte[7] { 1, 0, 0, 0, 0, 0, 0 };
			if (source.SectionFormat.HasKey(5))
			{
				CharacterPropertiesConverter.AuthorNames.Add(source.SectionFormat.FormatChangeAuthorName);
			}
			byte[] bytes = BitConverter.GetBytes((short)CharacterPropertiesConverter.AuthorNames.IndexOf(source.SectionFormat.FormatChangeAuthorName));
			byte[] src = new byte[4];
			if (source.SectionFormat.HasKey(6))
			{
				src = BitConverter.GetBytes(source.SectionFormat.GetDTTMIntValue(source.SectionFormat.FormatChangeDateTime));
			}
			Buffer.BlockCopy(bytes, 0, array, 1, 2);
			Buffer.BlockCopy(src, 0, array, 3, 4);
			destination.Sprms.SetByteArrayValue(53827, array);
		}
		if (source.PageSetup.DifferentFirstPage)
		{
			destination.TitlePage = source.PageSetup.DifferentFirstPage;
		}
		if (source.PageSetup.LineNumberingStep != 0)
		{
			bool flag = true;
			if (source.PageSetup.LineNumberingMode != LineNumberingMode.None)
			{
				destination.LineNumberingMode = source.PageSetup.LineNumberingMode;
				if (destination.Sprms.Contain(12857))
				{
					if (source.PageSetup.LineNumberingStep != 0)
					{
						destination.LineNumberingStep = (ushort)source.PageSetup.LineNumberingStep;
					}
					if (source.PageSetup.LineNumberingStartValue != 0)
					{
						destination.LineNumberingStartValue = (short)source.PageSetup.LineNumberingStartValue;
					}
					if (source.PageSetup.LineNumberingDistanceFromText != 0f)
					{
						destination.LineNumberingDistanceFromText = (short)Math.Round(source.PageSetup.LineNumberingDistanceFromText * 20f);
					}
					flag = false;
				}
			}
			if (flag)
			{
				if (source.PageSetup.LineNumberingStep != 0)
				{
					destination.LineNumberingStep = (ushort)source.PageSetup.LineNumberingStep;
				}
				if (source.PageSetup.LineNumberingStartValue != 0)
				{
					destination.LineNumberingStartValue = (short)source.PageSetup.LineNumberingStartValue;
				}
				if (source.PageSetup.LineNumberingDistanceFromText != 0f)
				{
					destination.LineNumberingDistanceFromText = (short)Math.Round(source.PageSetup.LineNumberingDistanceFromText * 20f);
				}
			}
		}
		if (source.PageSetup.Bidi)
		{
			destination.Bidi = source.PageSetup.Bidi;
		}
		if (!source.PageSetup.Borders.IsDefault)
		{
			Borders borders = source.PageSetup.Borders;
			BorderCode borderCode = new BorderCode();
			if (borders.Top.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Top);
				destination.TopBorder = borderCode;
				destination.TopBorderNew = borderCode;
			}
			if (borders.Left.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Left);
				destination.LeftBorder = borderCode;
				destination.LeftBorderNew = borderCode;
			}
			if (borders.Bottom.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Bottom);
				destination.BottomBorder = borderCode;
				destination.BottomBorderNew = borderCode;
			}
			if (borders.Right.IsBorderDefined)
			{
				ImportBorder(borderCode, borders.Right);
				destination.RightBorder = borderCode;
				destination.RightBorderNew = borderCode;
			}
		}
		destination.PageBorderApply = source.PageSetup.PageBordersApplyType;
		destination.PageBorderIsInFront = source.PageSetup.IsFrontPageBorder;
		destination.PageBorderOffsetFrom = source.PageSetup.PageBorderOffsetFrom;
		destination.PageNfc = (byte)source.PageSetup.PageNumberStyle;
		destination.PageRestart = source.PageSetup.RestartPageNumbering;
		destination.PageStartAt = (ushort)source.PageSetup.PageStartingNumber;
		if (source.PageSetup.PageNumbers.HeadingLevelForChapter != 0)
		{
			destination.HeadingLevelForChapter = (byte)source.PageSetup.PageNumbers.HeadingLevelForChapter;
			destination.ChapterPageSeparator = (byte)source.PageSetup.PageNumbers.ChapterPageSeparator;
		}
		else
		{
			destination.Sprms.RemoveValue(12288);
			destination.Sprms.RemoveValue(12289);
		}
		destination.Columns.Clear();
		destination.OldColumns.Clear();
		if (source.PageSetup.FootnoteNumberFormat != 0)
		{
			destination.FootnoteNumberFormat = (byte)source.PageSetup.FootnoteNumberFormat;
		}
		if (source.PageSetup.FootnotePosition == FootnotePosition.PrintImmediatelyBeneathText)
		{
			destination.FootnotePosition = (byte)source.PageSetup.FootnotePosition;
		}
		if (source.PageSetup.EndnoteNumberFormat != FootEndNoteNumberFormat.LowerCaseRoman)
		{
			destination.EndnoteNumberFormat = (byte)source.PageSetup.EndnoteNumberFormat;
		}
		if (source.PageSetup.RestartIndexForFootnotes != 0)
		{
			destination.RestartIndexForFootnotes = (byte)source.PageSetup.RestartIndexForFootnotes;
		}
		if (source.PageSetup.RestartIndexForEndnote != 0)
		{
			destination.RestartIndexForEndnote = (byte)source.PageSetup.RestartIndexForEndnote;
		}
		if (source.PageSetup.InitialFootnoteNumber != 1)
		{
			destination.InitialFootnoteNumber = (ushort)source.PageSetup.InitialFootnoteNumber;
		}
		if (source.PageSetup.InitialEndnoteNumber != 1)
		{
			destination.InitialEndnoteNumber = (ushort)source.PageSetup.InitialEndnoteNumber;
		}
		if (source.Columns.Count > 0)
		{
			Column column = null;
			int i = 0;
			for (int count = source.Columns.Count; i < count; i++)
			{
				column = source.Columns[i];
				ColumnDescriptor columnDescriptor = destination.Columns.AddColumn();
				columnDescriptor.Width = (ushort)Math.Round(column.Width * 20f);
				columnDescriptor.Space = (ushort)Math.Round(column.Space * 20f);
			}
			if (source.Columns.Count > 1 && !source.PageSetup.EqualColumnWidth)
			{
				destination.Columns.ColumnsEvenlySpaced = false;
			}
		}
		else
		{
			destination.Sprms.SetShortValue(36876, 720);
		}
		if (source.DataArray != null && source.DataArray.Length < 300 && source.DataArray.Length != 0)
		{
			SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray(source.DataArray, 0);
			SinglePropertyModifierRecord singlePropertyModifierRecord = null;
			int j = 0;
			for (int count2 = singlePropertyModifierArray.Count; j < count2; j++)
			{
				singlePropertyModifierRecord = singlePropertyModifierArray.GetSprmByIndex(j);
				if (!destination.HasOptions(singlePropertyModifierRecord.TypedOptions) || destination.HasOptions(21039) || destination.HasOptions(53827))
				{
					destination.Sprms.Modifiers.Add(singlePropertyModifierRecord);
				}
			}
		}
		destination.Sprms.SortSprms();
		destination.DrawLinesBetweenCols = source.PageSetup.DrawLinesBetweenCols;
		destination.BreakCode = (byte)source.BreakCode;
		destination.ProtectForm = source.ProtectForm;
	}

	private static void ExportBorder(BorderCode srcBorder, Border destBorder)
	{
		if (!srcBorder.IsDefault)
		{
			destBorder.Color = srcBorder.LineColorExt;
			destBorder.BorderType = (BorderStyle)srcBorder.BorderType;
			destBorder.LineWidth = (float)(int)srcBorder.LineWidth / 8f;
			destBorder.Space = (int)srcBorder.Space;
			destBorder.Shadow = srcBorder.Shadow;
		}
	}

	private static void ImportBorder(BorderCode destBorder, Border srcBorder)
	{
		if (!srcBorder.IsDefault)
		{
			destBorder.LineColor = (byte)WordColor.ConvertColorToId(srcBorder.Color);
			destBorder.LineColorExt = srcBorder.Color;
			destBorder.BorderType = (byte)srcBorder.BorderType;
			destBorder.LineWidth = (byte)Math.Round(srcBorder.LineWidth * 8f);
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

	private static Borders GetDestBorders(Borders destBorders, WPageSetup destination)
	{
		if (destBorders == null)
		{
			destBorders = destination.Borders;
		}
		return destBorders;
	}
}
