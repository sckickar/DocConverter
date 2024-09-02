using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class TablePropertiesConverter
{
	private static readonly object m_threadLocker = new object();

	private static byte[] m_cellShadings = null;

	private static byte[] m_cellShadings_1st = null;

	private static byte[] m_cellShadings_2nd = null;

	private static byte[] m_cellShadings_3rd = null;

	private static Dictionary<short, byte[]> m_cellBorders = null;

	private static Dictionary<short, byte[]> m_cellTCGRF = null;

	private static Dictionary<short, byte[]> m_cellWidth = null;

	private static byte[] m_topBorderCV = null;

	private static byte[] m_leftBorderCV = null;

	private static byte[] m_rightBorderCV = null;

	private static byte[] m_bottomBorderCV = null;

	private static byte[] m_cellBorderType = null;

	internal static void SprmsToFormat(IWordReaderBase reader, RowFormat rowFormat)
	{
		lock (m_threadLocker)
		{
			SprmsToFormat(reader.PAPX.PropertyModifiers, rowFormat, reader.StyleSheet, reader.SttbfRMarkAuthorNames, reader, isNewPropertyHash: true);
		}
	}

	internal static void SprmsToFormat(SinglePropertyModifierArray sprms, RowFormat rowFormat, WordStyleSheet styleSheet, Dictionary<int, string> authorNames, IWordReaderBase reader, bool isNewPropertyHash)
	{
		lock (m_threadLocker)
		{
			if (sprms == null)
			{
				return;
			}
			RowFormat.TablePositioning tablePositioning = null;
			if (sprms.Contain(13928))
			{
				rowFormat.IsFormattingChange = true;
				tablePositioning = new RowFormat.TablePositioning(rowFormat);
			}
			WTableRow ownerRow = rowFormat.OwnerRow;
			TableBorders tableBorders = null;
			Borders borders = null;
			short[] xCenterArray = null;
			int cellCount = 0;
			int num = 22;
			int num2 = 0;
			int num3 = 0;
			byte[] array = null;
			WTableCell wTableCell = null;
			ShadingDescriptor shadingDescriptor = null;
			RowFormat.TablePositioning positioning = rowFormat.Positioning;
			RowFormat.TablePositioning tablePositioning2 = null;
			Spacings spacings = null;
			Spacings spacings2 = null;
			Dictionary<int, Spacings> dictionary = null;
			bool flag = false;
			foreach (SinglePropertyModifierRecord sprm in sprms)
			{
				switch (sprm.Options)
				{
				case 25707:
					if (rowFormat.m_unParsedSprms == null)
					{
						rowFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					rowFormat.m_unParsedSprms.Add(sprm);
					break;
				case 21504:
					if (rowFormat.Document.WordVersion <= 217)
					{
						rowFormat.SetPropertyValue(105, (ParagraphJustify)sprm.ShortValue);
					}
					break;
				case 21642:
					if (sprms.HasSprm(22116) && sprms[22116].BoolValue && sprm.ShortValue == 2 && (!sprms.HasSprm(22027) || !sprms[22027].BoolValue))
					{
						rowFormat.SetPropertyValue(105, ParagraphJustify.Left);
					}
					else
					{
						rowFormat.SetPropertyValue(105, (ParagraphJustify)sprm.ShortValue);
					}
					break;
				case 38401:
					if (rowFormat.m_unParsedSprms == null)
					{
						rowFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					rowFormat.m_unParsedSprms.Add(sprm);
					break;
				case 13315:
					if (rowFormat.Document.WordVersion <= 217)
					{
						rowFormat.SetPropertyValue(106, !sprm.BoolValue);
					}
					break;
				case 13316:
					rowFormat.SetPropertyValue(107, sprm.BoolValue);
					break;
				case 54789:
					ExportTableRowBorders(sprms, rowFormat, sprm);
					break;
				case 37895:
				{
					float num5 = (float)sprm.ShortValue / 20f;
					if (num5 < 0f)
					{
						ownerRow.HeightType = TableRowHeightType.Exactly;
						num5 = Math.Abs(num5);
					}
					rowFormat.Height = num5;
					break;
				}
				case 54792:
					UpdateTableCellDefinition(sprm, ownerRow, ref cellCount, ref xCenterArray, reader);
					break;
				case 54793:
					array = sprm.ByteArray;
					num2 = 0;
					num3 = 0;
					for (; num2 < ownerRow.Cells.Count; num2++)
					{
						if (array.Length <= num3)
						{
							break;
						}
						wTableCell = ownerRow.Cells[num2];
						shadingDescriptor = new ShadingDescriptor(BitConverter.ToInt16(array, num3));
						wTableCell.CellFormat.IsFormattingChange = ownerRow.RowFormat.IsFormattingChange;
						SetShadingValues(wTableCell.CellFormat, shadingDescriptor);
						num3 += 2;
					}
					break;
				case 29706:
					if (rowFormat.m_unParsedSprms == null)
					{
						rowFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					rowFormat.m_unParsedSprms.Add(sprm);
					break;
				case 22027:
					ownerRow.RowFormat.SetPropertyValue(104, sprm.BoolValue);
					break;
				case 54796:
					UpdateCellShading(num * 2, 63, sprm.ByteArray, ownerRow);
					break;
				case 13837:
				{
					tablePositioning2 = tablePositioning ?? positioning;
					HorizontalRelation horizRelationTo = HorizontalRelation.Column;
					switch ((sprm.ByteValue >> 6) & 3)
					{
					case 1:
						horizRelationTo = HorizontalRelation.Margin;
						break;
					case 2:
						horizRelationTo = HorizontalRelation.Page;
						break;
					}
					tablePositioning2.HorizRelationTo = horizRelationTo;
					VerticalRelation vertRelationTo = VerticalRelation.Margin;
					switch ((sprm.ByteValue >> 4) & 3)
					{
					case 1:
						vertRelationTo = VerticalRelation.Page;
						break;
					case 2:
						vertRelationTo = VerticalRelation.Paragraph;
						break;
					}
					tablePositioning2.VertRelationTo = vertRelationTo;
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				}
				case 37902:
				{
					tablePositioning2 = tablePositioning ?? positioning;
					short shortValue = sprm.ShortValue;
					tablePositioning2.HorizPosition = ((shortValue == -4 || shortValue == -8 || shortValue == -12 || shortValue == -16) ? ((float)shortValue) : ((float)shortValue / 20f));
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				}
				case 37903:
				{
					tablePositioning2 = tablePositioning ?? positioning;
					short shortValue = sprm.ShortValue;
					tablePositioning2.VertPosition = ((shortValue == -4 || shortValue == -8 || shortValue == -12 || shortValue == -16 || shortValue == -20) ? ((float)shortValue) : ((float)shortValue / 20f));
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				}
				case 37904:
					tablePositioning2 = tablePositioning ?? positioning;
					tablePositioning2.DistanceFromLeft = (float)sprm.ShortValue / 20f;
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				case 37905:
					tablePositioning2 = tablePositioning ?? positioning;
					tablePositioning2.DistanceFromTop = (float)sprm.ShortValue / 20f;
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				case 54802:
					UpdateCellShading(0, num, sprm.ByteArray, ownerRow);
					break;
				case 54803:
					tableBorders = new TableBorders();
					ExportTableRowBorders(sprm.ByteArray, tableBorders, isOldSprm: false);
					if (rowFormat.IsFormattingChange)
					{
						borders = new Borders();
						ExportBorders(tableBorders, borders);
						rowFormat.SetPropertyValue(1, borders);
					}
					else
					{
						ExportBorders(tableBorders, rowFormat.Borders);
					}
					break;
				case 62996:
					UpdatePreferredWidthInfo(sprm, rowFormat, 11);
					break;
				case 13845:
					rowFormat.SetPropertyValue(103, sprm.BoolValue);
					break;
				case 54806:
					UpdateCellShading(num, num * 2, sprm.ByteArray, ownerRow);
					break;
				case 62999:
					UpdatePreferredWidthInfo(sprm, rowFormat, 13);
					break;
				case 63000:
					UpdatePreferredWidthInfo(sprm, rowFormat, 15);
					break;
				case 54810:
				{
					uint[] colors3 = GetColors(sprm, ownerRow.Cells.Count);
					if (colors3 == null || colors3.Length != ownerRow.Cells.Count)
					{
						break;
					}
					for (num2 = 0; num2 < ownerRow.Cells.Count; num2++)
					{
						wTableCell = ownerRow.Cells[num2];
						borders = GetCellBorders(wTableCell, rowFormat.IsFormattingChange);
						if (colors3[num2] != 4278190080u)
						{
							borders.Top.Color = WordColor.ConvertRGBToColor(colors3[num2]);
						}
					}
					break;
				}
				case 54811:
				{
					uint[] colors2 = GetColors(sprm, ownerRow.Cells.Count);
					if (colors2 == null || colors2.Length != ownerRow.Cells.Count)
					{
						break;
					}
					for (num2 = 0; num2 < ownerRow.Cells.Count; num2++)
					{
						wTableCell = ownerRow.Cells[num2];
						borders = GetCellBorders(wTableCell, rowFormat.IsFormattingChange);
						if (colors2[num2] != 4278190080u)
						{
							borders.Left.Color = WordColor.ConvertRGBToColor(colors2[num2]);
						}
					}
					break;
				}
				case 54812:
				{
					uint[] colors = GetColors(sprm, ownerRow.Cells.Count);
					if (colors == null || colors.Length != ownerRow.Cells.Count)
					{
						break;
					}
					for (num2 = 0; num2 < ownerRow.Cells.Count; num2++)
					{
						wTableCell = ownerRow.Cells[num2];
						borders = GetCellBorders(wTableCell, rowFormat.IsFormattingChange);
						if (colors[num2] != 4278190080u)
						{
							borders.Bottom.Color = WordColor.ConvertRGBToColor(colors[num2]);
						}
					}
					break;
				}
				case 54813:
				{
					uint[] colors4 = GetColors(sprm, ownerRow.Cells.Count);
					if (colors4 == null || colors4.Length != ownerRow.Cells.Count)
					{
						break;
					}
					for (num2 = 0; num2 < ownerRow.Cells.Count; num2++)
					{
						wTableCell = ownerRow.Cells[num2];
						borders = GetCellBorders(wTableCell, rowFormat.IsFormattingChange);
						if (colors4[num2] != 4278190080u)
						{
							borders.Right.Color = WordColor.ConvertRGBToColor(colors4[num2]);
						}
					}
					break;
				}
				case 37918:
					tablePositioning2 = tablePositioning ?? positioning;
					tablePositioning2.DistanceFromRight = (float)sprm.ShortValue / 20f;
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				case 37919:
					tablePositioning2 = tablePositioning ?? positioning;
					tablePositioning2.DistanceFromBottom = (float)sprm.ShortValue / 20f;
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				case 54834:
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<int, Spacings>();
					}
					byte b = sprm.ByteArray[0];
					byte b2 = sprm.ByteArray[1];
					if (b >= ownerRow.Cells.Count || b2 < b || b2 > ownerRow.Cells.Count)
					{
						break;
					}
					if (dictionary.ContainsKey(b))
					{
						dictionary[b].Parse(sprm);
					}
					else
					{
						spacings2 = new Spacings(sprm);
						dictionary.Add(b, spacings2.Clone());
					}
					if (b + 1 != b2)
					{
						for (num2 = b + 1; num2 < b2; num2++)
						{
							dictionary[num2] = dictionary[b].Clone();
						}
					}
					break;
				}
				case 54835:
					array = sprm.ByteArray;
					if (array != null && array.Length == 6 && (array[3] == 3 || array[3] == 19) && array[2] == 15)
					{
						float num4 = (float)(int)BitConverter.ToUInt16(array, 4) / 20f;
						if (num4 >= 0f)
						{
							rowFormat.SetPropertyValue(52, num4);
						}
					}
					break;
				case 54836:
					if (spacings == null)
					{
						spacings = new Spacings(sprm);
					}
					else
					{
						spacings.Parse(sprm);
					}
					break;
				case 22074:
					if (rowFormat.m_unParsedSprms == null)
					{
						rowFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					rowFormat.m_unParsedSprms.Add(sprm);
					break;
				case 54880:
					shadingDescriptor = new ShadingDescriptor();
					shadingDescriptor.ReadNewShd(sprm.ByteArray, 0);
					rowFormat.SetPropertyValue(111, shadingDescriptor.ForeColor);
					rowFormat.SetPropertyValue(108, shadingDescriptor.BackColor);
					rowFormat.SetPropertyValue(110, shadingDescriptor.Pattern);
					break;
				case 63073:
					rowFormat.SetPropertyValue(53, (float)BitConverter.ToInt16(sprm.ByteArray, 1) / 20f);
					break;
				case 54882:
					array = sprm.ByteArray;
					if (array.Length % 4 != 0)
					{
						break;
					}
					num3 = 0;
					for (num2 = 0; num2 < ownerRow.Cells.Count; num2++)
					{
						if (num3 + 3 >= array.Length)
						{
							break;
						}
						wTableCell = ownerRow.Cells[num2];
						borders = (rowFormat.IsFormattingChange ? ((Borders)wTableCell.CellFormat.OldPropertiesHash[1]) : ((Borders)wTableCell.CellFormat.PropertiesHash[1]));
						if (borders != null)
						{
							ApplyBorderStyle(borders.Top, array[num3]);
							ApplyBorderStyle(borders.Left, array[num3 + 1]);
							ApplyBorderStyle(borders.Bottom, array[num3 + 2]);
							ApplyBorderStyle(borders.Right, array[num3 + 3]);
						}
						num3 += 4;
					}
					break;
				case 22116:
					if (rowFormat.m_unParsedSprms == null)
					{
						rowFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					rowFormat.m_unParsedSprms.Add(sprm);
					break;
				case 13413:
					tablePositioning2 = tablePositioning ?? positioning;
					tablePositioning2.AllowOverlap = !sprm.BoolValue;
					if (rowFormat.IsFormattingChange && !flag)
					{
						rowFormat.SetPropertyValue(120, tablePositioning2);
						flag = true;
					}
					break;
				case 13414:
					if (rowFormat.Document.WordVersion > 217)
					{
						rowFormat.SetPropertyValue(106, !sprm.BoolValue);
					}
					break;
				case 54887:
				{
					rowFormat.IsChangedFormat = true;
					short key = BitConverter.ToInt16(sprm.ByteArray, 1);
					if (authorNames != null && authorNames.Count > 0 && authorNames.ContainsKey(key))
					{
						rowFormat.FormatChangeAuthorName = authorNames[key];
					}
					DateTime formatChangeDateTime = rowFormat.ParseDTTM(BitConverter.ToInt32(sprm.ByteArray, 3));
					if (formatChangeDateTime.Year < 1900)
					{
						formatChangeDateTime = new DateTime(1900, 1, 1, 0, 0, 0);
					}
					rowFormat.FormatChangeDateTime = formatChangeDateTime;
					break;
				}
				case 13928:
					if (spacings != null)
					{
						Paddings paddings = new Paddings();
						ExportPaddings(spacings, paddings);
						rowFormat.SetPropertyValue(3, paddings);
					}
					spacings = null;
					if (dictionary != null && dictionary.Count > 0)
					{
						UpdateCellPaddings(dictionary, ownerRow, rowFormat.IsFormattingChange);
					}
					dictionary = null;
					if (!rowFormat.OldPropertiesHash.ContainsKey(53) && xCenterArray != null)
					{
						UpdateLeftIndent(sprms.GetOldSprm(26185, 13928), ownerRow, xCenterArray[0], rowFormat.IsFormattingChange);
					}
					tablePositioning = null;
					flag = false;
					rowFormat.IsFormattingChange = false;
					break;
				case 29801:
					if (rowFormat.m_unParsedSprms == null)
					{
						rowFormat.m_unParsedSprms = new SinglePropertyModifierArray();
					}
					rowFormat.m_unParsedSprms.Add(sprm);
					break;
				case 54896:
					if ((rowFormat.IsFormattingChange ? sprms.GetOldSprm(54802, 13928) : sprms.GetNewSprm(54802, 13928)) == null)
					{
						UpdateCellShading(0, num, sprm.ByteArray, ownerRow);
					}
					break;
				case 54897:
					if ((rowFormat.IsFormattingChange ? sprms.GetOldSprm(54806, 13928) : sprms.GetNewSprm(54806, 13928)) == null)
					{
						UpdateCellShading(num, num * 2, sprm.ByteArray, ownerRow);
					}
					break;
				case 54898:
					if ((rowFormat.IsFormattingChange ? sprms.GetOldSprm(54796, 13928) : sprms.GetNewSprm(54796, 13928)) == null)
					{
						UpdateCellShading(num * 2, 63, sprm.ByteArray, ownerRow);
					}
					break;
				}
			}
			if (!sprms.Contain(54803) && !sprms.Contain(54789))
			{
				tableBorders = new TableBorders();
				ExportBorders(tableBorders, rowFormat.Borders);
			}
			if (spacings != null && !spacings.IsEmpty)
			{
				ExportPaddings(spacings, rowFormat.Paddings);
			}
			else if (sprms.GetNewSprm(54836, 13928) != null)
			{
				ExportDefaultPaddings(rowFormat.Paddings);
			}
			rowFormat.SkipDefaultPadding = true;
			if (dictionary != null && dictionary.Count > 0)
			{
				UpdateCellPaddings(dictionary, ownerRow);
			}
			spacings = null;
			UpdateRowFormatPropertyHash(rowFormat);
			if (!rowFormat.PropertiesHash.ContainsKey(53) && xCenterArray != null)
			{
				UpdateLeftIndent(sprms.GetNewSprm(26185, 13928), ownerRow, xCenterArray[0], rowFormat.IsFormattingChange);
			}
			foreach (WTableCell cell in ownerRow.Cells)
			{
				UpdateCellFormatPropertyHash(cell.CellFormat);
			}
		}
	}

	private static void ApplyBorderStyle(Border border, byte borderTypeValue)
	{
		if (border.LineWidth != 0f || !(border.Color == Color.Empty) || border.BorderType != 0)
		{
			border.BorderType = (BorderStyle)borderTypeValue;
		}
	}

	private static void UpdateRowFormatPropertyHash(RowFormat rowFormat)
	{
		if (rowFormat.OldPropertiesHash.Count <= 0 || rowFormat.PropertiesHash.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, object> item in rowFormat.OldPropertiesHash)
		{
			if (item.Key == 1 && rowFormat.PropertiesHash.ContainsKey(1))
			{
				Borders borders = (Borders)item.Value;
				Borders borders2 = rowFormat.Borders;
				if (borders.Top.IsBorderDefined && !borders2.Top.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Top, borders.Top);
				}
				if (borders.Bottom.IsBorderDefined && !borders2.Bottom.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Bottom, borders.Bottom);
				}
				if (borders.Right.IsBorderDefined && !borders2.Right.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Right, borders.Right);
				}
				if (borders.Left.IsBorderDefined && !borders2.Left.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Left, borders.Left);
				}
				if (borders.Horizontal.IsBorderDefined && !borders2.Horizontal.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Horizontal, borders.Horizontal);
				}
				if (borders.Vertical.IsBorderDefined && !borders2.Vertical.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Vertical, borders.Vertical);
				}
			}
			if (item.Key == 120 && rowFormat.PropertiesHash.ContainsKey(120))
			{
				UpdatePositioning((RowFormat.TablePositioning)item.Value, rowFormat.Positioning);
			}
			if (item.Key == 3 && rowFormat.PropertiesHash.ContainsKey(3))
			{
				UpdatePaddings((Paddings)item.Value, rowFormat.Paddings);
			}
			if (!rowFormat.PropertiesHash.ContainsKey(item.Key))
			{
				rowFormat.PropertiesHash.Add(item.Key, item.Value);
			}
		}
	}

	private static void UpdateCellFormatPropertyHash(CellFormat cellFormat)
	{
		if (cellFormat.OldPropertiesHash.Count <= 0 || cellFormat.PropertiesHash.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, object> item in cellFormat.OldPropertiesHash)
		{
			if (item.Key == 1 && cellFormat.PropertiesHash.ContainsKey(1))
			{
				Borders borders = (Borders)item.Value;
				Borders borders2 = cellFormat.Borders;
				if (borders.Top.IsBorderDefined && !borders2.Top.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Top, borders.Top);
				}
				if (borders.Bottom.IsBorderDefined && !borders2.Bottom.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Bottom, borders.Bottom);
				}
				if (borders.Right.IsBorderDefined && !borders2.Right.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Right, borders.Right);
				}
				if (borders.Left.IsBorderDefined && !borders2.Left.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Left, borders.Left);
				}
				if (borders.Horizontal.IsBorderDefined && !borders2.Horizontal.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Horizontal, borders.Horizontal);
				}
				if (borders.Vertical.IsBorderDefined && !borders2.Vertical.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(borders2.Vertical, borders.Vertical);
				}
			}
			if (item.Key == 3 && cellFormat.PropertiesHash.ContainsKey(3))
			{
				UpdatePaddings((Paddings)item.Value, cellFormat.Paddings);
			}
			if (!cellFormat.PropertiesHash.ContainsKey(item.Key))
			{
				cellFormat.PropertiesHash.Add(item.Key, item.Value);
			}
		}
	}

	private static void UpdatePositioning(RowFormat.TablePositioning positioningOld, RowFormat.TablePositioning positioning)
	{
		if (positioningOld.HasKey(70) && !positioning.HasKey(70))
		{
			positioning.AllowOverlap = positioningOld.AllowOverlap;
		}
		if (positioningOld.HasKey(67) && !positioning.HasKey(67))
		{
			positioning.DistanceFromBottom = positioningOld.DistanceFromBottom;
		}
		if (positioningOld.HasKey(68) && !positioning.HasKey(68))
		{
			positioning.DistanceFromLeft = positioningOld.DistanceFromLeft;
		}
		if (positioningOld.HasKey(69) && !positioning.HasKey(69))
		{
			positioning.DistanceFromRight = positioningOld.DistanceFromRight;
		}
		if (positioningOld.HasKey(66) && !positioning.HasKey(66))
		{
			positioning.DistanceFromTop = positioningOld.DistanceFromTop;
		}
		if (positioningOld.HasKey(62) && !positioning.HasKey(62))
		{
			positioning.HorizPositionAbs = positioningOld.HorizPositionAbs;
		}
		if (positioningOld.HasKey(64) && !positioning.HasKey(64))
		{
			positioning.HorizRelationTo = positioningOld.HorizRelationTo;
		}
		if (positioningOld.HasKey(63) && !positioning.HasKey(63))
		{
			positioning.VertPositionAbs = positioningOld.VertPositionAbs;
		}
		if (positioningOld.HasKey(65) && !positioning.HasKey(65))
		{
			positioning.VertRelationTo = positioningOld.VertRelationTo;
		}
	}

	private static void UpdatePaddings(Paddings paddingsOld, Paddings paddings)
	{
		if (paddingsOld.HasKey(3) && !paddings.HasKey(3))
		{
			paddings.Bottom = paddingsOld.Bottom;
		}
		if (paddingsOld.HasKey(1) && !paddings.HasKey(1))
		{
			paddings.Left = paddingsOld.Left;
		}
		if (paddingsOld.HasKey(4) && !paddings.HasKey(4))
		{
			paddings.Right = paddingsOld.Right;
		}
		if (paddingsOld.HasKey(2) && !paddings.HasKey(2))
		{
			paddings.Top = paddingsOld.Top;
		}
	}

	private static void UpdateCellPaddings(Dictionary<int, Spacings> cellSpacings, WTableRow tableRow, bool isChangedFormat)
	{
		foreach (KeyValuePair<int, Spacings> cellSpacing in cellSpacings)
		{
			Paddings paddings = new Paddings();
			ExportPaddings(cellSpacing.Value, paddings);
			WTableCell wTableCell = tableRow.Cells[cellSpacing.Key];
			wTableCell.CellFormat.IsFormattingChange = isChangedFormat;
			wTableCell.CellFormat.SetPropertyValue(3, paddings);
		}
		cellSpacings.Clear();
	}

	private static void UpdateCellPaddings(Dictionary<int, Spacings> cellSpacings, WTableRow tableRow)
	{
		foreach (KeyValuePair<int, Spacings> cellSpacing in cellSpacings)
		{
			WTableCell wTableCell = tableRow.Cells[cellSpacing.Key];
			wTableCell.CellFormat.SamePaddingsAsTable = false;
			ExportPaddings(cellSpacing.Value, wTableCell.CellFormat.Paddings);
		}
		cellSpacings.Clear();
	}

	private static void UpdateLeftIndent(SinglePropertyModifierRecord sprmTNestingLevel, WTableRow tableRow, short xCenterArrayStart, bool isChangedFormat)
	{
		short num = 0;
		Dictionary<int, object> dictionary = (isChangedFormat ? tableRow.RowFormat.OldPropertiesHash : tableRow.RowFormat.PropertiesHash);
		if (sprmTNestingLevel != null && sprmTNestingLevel.IntValue == 1)
		{
			Dictionary<int, object> dictionary2 = (isChangedFormat ? tableRow.Cells[0].CellFormat.OldPropertiesHash : tableRow.Cells[0].CellFormat.PropertiesHash);
			Paddings paddings = (dictionary2.ContainsKey(3) ? (dictionary2[3] as Paddings) : null);
			if (paddings != null && paddings.Left != -1f && paddings.Right != -1f && paddings.Top != -1f && paddings.Bottom != -1f)
			{
				num = (short)(paddings.Left * 20f);
			}
			else
			{
				paddings = (dictionary.ContainsKey(3) ? (dictionary[3] as Paddings) : null);
				if (paddings != null && paddings.Left != -1f && paddings.Right != -1f && paddings.Top != -1f && paddings.Bottom != -1f)
				{
					num = (short)(paddings.Left * 20f);
				}
			}
		}
		short num2 = 0;
		float num3 = (dictionary.ContainsKey(52) ? ((float)dictionary[52]) : 0f);
		num3 *= 20f;
		if (num3 > 0f)
		{
			num2 += (short)(num3 * 2f);
			Borders borders = (dictionary.ContainsKey(1) ? ((Borders)dictionary[1]) : null);
			if (borders != null)
			{
				num2 += (short)(float)Math.Round(borders.Left.LineWidth / 8f * 20f);
			}
		}
		short num4 = (short)(xCenterArrayStart + num + num2);
		tableRow.RowFormat.SetPropertyValue(53, (float)num4 / 20f);
	}

	private static uint[] GetColors(SinglePropertyModifierRecord sprm, int cellCount)
	{
		uint[] array = null;
		byte[] byteArray = sprm.ByteArray;
		if (byteArray != null && byteArray.Length == cellCount * 4 && cellCount > 0)
		{
			array = new uint[cellCount];
			for (int i = 0; i < cellCount; i++)
			{
				uint num = BitConverter.ToUInt32(byteArray, i * 4);
				array[i] = num;
			}
		}
		return array;
	}

	private static Borders GetCellBorders(WTableCell cell, bool isFormattingChange)
	{
		Borders result = null;
		if (isFormattingChange && cell.CellFormat.OldPropertiesHash.ContainsKey(1))
		{
			result = cell.CellFormat.OldPropertiesHash[1] as Borders;
		}
		else if (cell.CellFormat.PropertiesHash.ContainsKey(1))
		{
			result = cell.CellFormat.PropertiesHash[1] as Borders;
		}
		return result;
	}

	private static void UpdateCellShading(int startCellIndex, int endCellIndex, byte[] buf, WTableRow tableRow)
	{
		int num = 0;
		for (int i = startCellIndex; i < endCellIndex && i < tableRow.Cells.Count; i++)
		{
			if (buf.Length <= num)
			{
				break;
			}
			WTableCell wTableCell = tableRow.Cells[i];
			ShadingDescriptor shadingDescriptor = new ShadingDescriptor();
			shadingDescriptor.ReadNewShd(buf, num);
			wTableCell.CellFormat.IsFormattingChange = tableRow.RowFormat.IsFormattingChange;
			SetShadingValues(wTableCell.CellFormat, shadingDescriptor);
			num += 10;
		}
	}

	private static void UpdatePreferredWidthInfo(SinglePropertyModifierRecord sprm, RowFormat rowFormat, int formatKey)
	{
		byte[] byteArray = sprm.ByteArray;
		rowFormat.SetPropertyValue(formatKey, (FtsWidth)byteArray[0]);
		if (byteArray[0] == 2)
		{
			rowFormat.SetPropertyValue(formatKey + 1, (float)BitConverter.ToInt16(byteArray, 1) / 50f);
		}
		else
		{
			rowFormat.SetPropertyValue(formatKey + 1, (float)BitConverter.ToInt16(byteArray, 1) / 20f);
		}
	}

	private static void SetShadingValues(CellFormat cellFormat, ShadingDescriptor shdDesc)
	{
		cellFormat.SetPropertyValue(5, shdDesc.ForeColor);
		cellFormat.SetPropertyValue(4, shdDesc.BackColor);
		cellFormat.SetPropertyValue(7, shdDesc.Pattern);
	}

	private static void UpdateTableCellDefinition(SinglePropertyModifierRecord sprm, WTableRow tableRow, ref int cellCount, ref short[] xCenterArray, IWordReaderBase reader)
	{
		int num = 0;
		byte[] byteArray = sprm.ByteArray;
		cellCount = byteArray[0];
		if (cellCount < 1)
		{
			throw new ArgumentException(" Number of cells " + cellCount + " must be greater than 1");
		}
		byte[] array = new byte[(cellCount + 1) * 2];
		int num2 = 1 + 2 * (cellCount + 1);
		int num3 = 1 + 2 * (cellCount + 1);
		for (num = 1; num < num3; num++)
		{
			array[num - 1] = byteArray[num];
		}
		xCenterArray = new short[cellCount + 1];
		Buffer.BlockCopy(array, 0, xCenterArray, 0, array.Length);
		if (reader != null && reader.TableRowWidthStack.Count > 0)
		{
			Dictionary<WTableRow, short> dictionary = reader.TableRowWidthStack.Peek();
			short num4 = xCenterArray[xCenterArray.Length - 1];
			if (!dictionary.ContainsKey(tableRow))
			{
				dictionary.Add(tableRow, num4);
			}
			if (reader.MaximumTableRowWidth != null && reader.MaximumTableRowWidth.Count > 0 && reader.MaximumTableRowWidth[reader.MaximumTableRowWidth.Count - 1] < num4)
			{
				reader.MaximumTableRowWidth[reader.MaximumTableRowWidth.Count - 1] = num4;
			}
		}
		num = 0;
		int num5 = 20;
		for (; num < cellCount && num < tableRow.Cells.Count; num++)
		{
			WTableCell wTableCell = tableRow.Cells[num];
			wTableCell.CellFormat.IsFormattingChange = tableRow.RowFormat.IsFormattingChange;
			if (num > xCenterArray.Length - 2)
			{
				throw new ArgumentOutOfRangeException("cellIndex");
			}
			wTableCell.CellFormat.SetPropertyValue(12, (float)(xCenterArray[num + 1] - xCenterArray[num]) / 20f);
			if (num2 + num5 > byteArray.Length)
			{
				UpdateTCGRF(wTableCell.CellFormat, 0);
			}
			else
			{
				UpdateTC80(wTableCell, byteArray, num2);
			}
			num2 += num5;
		}
	}

	private static void UpdateTC80(WTableCell tableCell, byte[] sprmData, int startPos)
	{
		short num = BitConverter.ToInt16(sprmData, startPos);
		UpdateTCGRF(tableCell.CellFormat, num);
		startPos += 2;
		ushort num2 = BitConverter.ToUInt16(sprmData, startPos);
		switch ((FtsWidth)((num >> 9) & 7))
		{
		case FtsWidth.Percentage:
			tableCell.CellFormat.SetPropertyValue(14, (float)(int)num2 / 50f);
			break;
		case FtsWidth.Point:
			tableCell.CellFormat.SetPropertyValue(14, (float)(int)num2 / 20f);
			break;
		}
		tableCell.SetHasPreferredWidth();
		startPos += 2;
		Borders borders = new Borders();
		borders = (tableCell.CellFormat.IsFormattingChange ? borders : tableCell.CellFormat.Borders);
		BRCToBorder(new BorderStructure(sprmData, startPos), borders.Top);
		startPos += 4;
		BRCToBorder(new BorderStructure(sprmData, startPos), borders.Left);
		startPos += 4;
		BRCToBorder(new BorderStructure(sprmData, startPos), borders.Bottom);
		startPos += 4;
		BRCToBorder(new BorderStructure(sprmData, startPos), borders.Right);
		if (tableCell.CellFormat.IsFormattingChange)
		{
			tableCell.CellFormat.SetPropertyValue(1, borders);
		}
	}

	private static void UpdateTCGRF(CellFormat cellFormat, short TCGRF)
	{
		cellFormat.SetPropertyValue(8, GetCellMerge(TCGRF & 3, isHorizontalCellMerge: true));
		TextDirection textDirection = TextDirection.Horizontal;
		switch ((TCGRF >> 2) & 7)
		{
		case 1:
			textDirection = TextDirection.VerticalTopToBottom;
			break;
		case 3:
			textDirection = TextDirection.VerticalBottomToTop;
			break;
		case 4:
			textDirection = TextDirection.HorizontalFarEast;
			break;
		case 5:
			textDirection = TextDirection.VerticalFarEast;
			break;
		case 7:
			textDirection = TextDirection.Vertical;
			break;
		}
		cellFormat.SetPropertyValue(11, textDirection);
		cellFormat.SetPropertyValue(6, GetCellMerge((TCGRF >> 5) & 3, isHorizontalCellMerge: false));
		cellFormat.SetPropertyValue(2, (VerticalAlignment)((TCGRF >> 7) & 3));
		cellFormat.SetPropertyValue(13, (FtsWidth)((TCGRF >> 9) & 7));
		cellFormat.SetPropertyValue(10, ((TCGRF >> 12) & 1) == 1);
		cellFormat.SetPropertyValue(9, ((TCGRF >> 13) & 1) == 0);
	}

	private static CellMerge GetCellMerge(int val, bool isHorizontalCellMerge)
	{
		CellMerge result = CellMerge.None;
		switch (val)
		{
		case 1:
			result = (isHorizontalCellMerge ? CellMerge.Start : CellMerge.Continue);
			break;
		case 2:
		case 3:
			result = ((!isHorizontalCellMerge) ? CellMerge.Start : CellMerge.Continue);
			break;
		}
		return result;
	}

	private static void ExportTableRowBorders(SinglePropertyModifierArray sprms, RowFormat rowFormat, SinglePropertyModifierRecord sprm)
	{
		TableBorders tableBorders = null;
		if (rowFormat.IsFormattingChange)
		{
			if (sprms.GetOldSprm(54803, 13928) == null)
			{
				byte[] byteArray = sprm.ByteArray;
				tableBorders = new TableBorders();
				ExportTableRowBorders(byteArray, tableBorders, isOldSprm: true);
				Borders borders = new Borders();
				borders.IsFormattingChange = true;
				ExportBorders(tableBorders, borders);
				rowFormat.SetPropertyValue(1, borders);
			}
		}
		else if (sprms.GetNewSprm(54803, 13928) == null)
		{
			byte[] byteArray2 = sprm.ByteArray;
			tableBorders = new TableBorders();
			ExportTableRowBorders(byteArray2, tableBorders, isOldSprm: true);
			ExportBorders(tableBorders, rowFormat.Borders);
		}
	}

	private static void ExportBorders(TableBorders srcBorders, Borders destBorders)
	{
		BRCToBorder(srcBorders.LeftBorder, destBorders.Left);
		BRCToBorder(srcBorders.RightBorder, destBorders.Right);
		BRCToBorder(srcBorders.TopBorder, destBorders.Top);
		BRCToBorder(srcBorders.BottomBorder, destBorders.Bottom);
		BRCToBorder(srcBorders.HorizontalBorder, destBorders.Horizontal);
		BRCToBorder(srcBorders.VerticalBorder, destBorders.Vertical);
	}

	private static void ExportTableRowBorders(byte[] buf, TableBorders tableBorders, bool isOldSprm)
	{
		if (isOldSprm)
		{
			for (int i = 0; i < 6; i++)
			{
				tableBorders[i] = new BorderCode();
				tableBorders[i].Parse(buf, i * 4);
			}
		}
		else
		{
			for (int j = 0; j < 6; j++)
			{
				tableBorders[j] = new BorderCode();
				tableBorders[j].ParseNewBrc(buf, j * 8);
			}
		}
	}

	internal static void FormatToSprms(WTableRow tableRow, SinglePropertyModifierArray sprms, WordStyleSheet styleSheet)
	{
		lock (m_threadLocker)
		{
			Dictionary<int, object> dictionary = new Dictionary<int, object>();
			RowFormat rowFormat = tableRow.RowFormat;
			if (!rowFormat.PropertiesHash.ContainsKey(1))
			{
				_ = rowFormat.Borders;
			}
			if (!rowFormat.SkipDefaultPadding)
			{
				rowFormat.CheckDefPadding();
			}
			if (rowFormat.PropertiesHash.Count > 0)
			{
				dictionary = new Dictionary<int, object>(rowFormat.PropertiesHash);
			}
			if (rowFormat.OldPropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item in new Dictionary<int, object>(rowFormat.OldPropertiesHash))
				{
					FormatToSprms(item.Key, item.Value, sprms, rowFormat, styleSheet, isOldFormat: true);
					if (dictionary.ContainsKey(item.Key) && dictionary[item.Key] == item.Value)
					{
						dictionary.Remove(item.Key);
					}
				}
			}
			int count = tableRow.Cells.Count;
			if (count > 0)
			{
				InitCollection(count);
			}
			for (int i = 0; i < count; i++)
			{
				CellFormat cellFormat = tableRow.Cells[i].CellFormat;
				if (cellFormat.OldPropertiesHash.Count <= 0)
				{
					continue;
				}
				Dictionary<int, object> dictionary2 = new Dictionary<int, object>(cellFormat.OldPropertiesHash);
				ushort tcgrf = 0;
				foreach (KeyValuePair<int, object> item2 in dictionary2)
				{
					FormatToSprms(item2.Key, item2.Value, sprms, cellFormat, isOldFormat: true, i, ref tcgrf);
					if (cellFormat.PropertiesHash.ContainsKey(item2.Key) && cellFormat.PropertiesHash[item2.Key] == item2.Value)
					{
						cellFormat.PropertiesHash.Remove(item2.Key);
					}
				}
				SetShading(dictionary2, i, isRowFormat: false);
				m_cellTCGRF[(short)i] = BitConverter.GetBytes(tcgrf);
			}
			UpdateCellsInformation(sprms);
			if (rowFormat.OldPropertiesHash.Count > 0)
			{
				WriteTableCellProps(sprms, tableRow, isOldFormat: true);
				WriteDxaGapHalf(sprms, tableRow, isOldFormat: true);
				sprms.SetBoolValue(13928, flag: true);
			}
			SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray();
			if (dictionary.Count > 0)
			{
				foreach (KeyValuePair<int, object> item3 in dictionary)
				{
					FormatToSprms(item3.Key, item3.Value, singlePropertyModifierArray, rowFormat, styleSheet, isOldFormat: false);
				}
				if (singlePropertyModifierArray[63073] == null)
				{
					byte[] array = new byte[3] { 3, 0, 0 };
					Buffer.BlockCopy(BitConverter.GetBytes((short)Math.Round(rowFormat.LeftIndent * 20f)), 0, array, 1, 2);
					singlePropertyModifierArray.SetByteArrayValue(63073, array);
				}
				if (singlePropertyModifierArray[62996] == null)
				{
					byte[] array2 = new byte[3];
					FtsWidth widthType = rowFormat.OwnerRow.OwnerTable.TableFormat.PreferredWidth.WidthType;
					float width = rowFormat.OwnerRow.OwnerTable.TableFormat.PreferredWidth.Width;
					array2[0] = (byte)widthType;
					short value = 0;
					switch (widthType)
					{
					case FtsWidth.Percentage:
						value = (short)Math.Round(width * 50f);
						break;
					case FtsWidth.Point:
						value = (short)Math.Round(width * 20f);
						break;
					}
					if (widthType > FtsWidth.Auto)
					{
						Buffer.BlockCopy(BitConverter.GetBytes(value), 0, array2, 1, 2);
					}
					singlePropertyModifierArray.SetByteArrayValue(62996, array2);
				}
				if (rowFormat.m_unParsedSprms != null && rowFormat.m_unParsedSprms.Count > 0)
				{
					foreach (SinglePropertyModifierRecord unParsedSprm in rowFormat.m_unParsedSprms)
					{
						if (unParsedSprm.OptionType != WordSprmOptionType.sprmPTableProps)
						{
							singlePropertyModifierArray.Add(unParsedSprm);
						}
					}
				}
			}
			if (count > 0)
			{
				InitCollection(count);
			}
			for (int j = 0; j < count; j++)
			{
				CellFormat cellFormat2 = tableRow.Cells[j].CellFormat;
				Dictionary<int, object> dictionary3 = new Dictionary<int, object>(cellFormat2.PropertiesHash);
				if (dictionary3.Count > 0)
				{
					ushort tcgrf2 = 0;
					foreach (KeyValuePair<int, object> item4 in dictionary3)
					{
						FormatToSprms(item4.Key, item4.Value, singlePropertyModifierArray, cellFormat2, isOldFormat: false, j, ref tcgrf2);
					}
					m_cellTCGRF[(short)j] = BitConverter.GetBytes(tcgrf2);
				}
				SetShading(dictionary3, j, isRowFormat: false);
			}
			if (tableRow.Cells.Count > 0)
			{
				UpdateCellsInformation(sprms);
				WriteTableCellProps(singlePropertyModifierArray, tableRow, isOldFormat: false);
				WriteDxaGapHalf(sprms, tableRow, isOldFormat: false);
			}
			for (int k = 0; k < singlePropertyModifierArray.Count; k++)
			{
				sprms.Add(singlePropertyModifierArray.GetSprmByIndex(k).Clone());
			}
			singlePropertyModifierArray.Clear();
			sprms.SortSprms();
		}
	}

	private static void WriteDxaGapHalf(SinglePropertyModifierArray sprms, WTableRow tableRow, bool isOldFormat)
	{
		short num = 0;
		short num2 = 0;
		WTableCell wTableCell = tableRow.Cells[0];
		Paddings paddings = (isOldFormat ? ((Paddings)wTableCell.CellFormat.GetKeyValue(wTableCell.CellFormat.OldPropertiesHash, 3)) : ((Paddings)wTableCell.CellFormat.GetKeyValue(wTableCell.CellFormat.PropertiesHash, 3)));
		Paddings paddings2 = (isOldFormat ? ((Paddings)tableRow.RowFormat.GetKeyValue(tableRow.RowFormat.OldPropertiesHash, 3)) : ((Paddings)tableRow.RowFormat.GetKeyValue(tableRow.RowFormat.PropertiesHash, 3)));
		if (paddings != null && !paddings.IsEmpty)
		{
			num = (short)(paddings.Left * 20f);
			num2 = (short)(paddings.Right * 20f);
		}
		else if (paddings2 != null && !paddings2.IsEmpty)
		{
			num = (short)(paddings2.Left * 20f);
			num2 = (short)(paddings2.Right * 20f);
		}
		byte[] array = new byte[2];
		array = BitConverter.GetBytes((short)((num + num2) / 2));
		sprms.SetByteArrayValue(38402, array);
	}

	private static void WriteTableCellProps(SinglePropertyModifierArray sprms, WTableRow row, bool isOldFormat)
	{
		int count = row.Cells.Count;
		byte[] array = new byte[count * 22 + 3];
		short[] array2 = new short[count + 1];
		UpdateXCenterArray(array2, row, isOldFormat);
		byte[] array3 = new byte[array2.Length * 2 + 2];
		int num = 1 + 2 * (count + 1);
		UpdateCenterArray(sprms, array2, row, isOldFormat);
		Buffer.BlockCopy(array2, 0, array3, 0, array2.Length * 2);
		array[0] = (byte)count;
		array3.CopyTo(array, 1);
		for (short num2 = 0; num2 < count; num2++)
		{
			if (m_cellTCGRF.ContainsKey(num2))
			{
				Buffer.BlockCopy(m_cellTCGRF[num2], 0, array, num, 2);
			}
			num += 2;
			if (m_cellWidth.ContainsKey(num2))
			{
				Buffer.BlockCopy(m_cellWidth[num2], 0, array, num, 2);
			}
			num += 2;
			if (m_cellBorders.ContainsKey(num2))
			{
				Buffer.BlockCopy(m_cellBorders[num2], 0, array, num, 16);
			}
			num += 16;
		}
		sprms.SetByteArrayValue(54792, array);
		m_cellTCGRF.Clear();
		m_cellTCGRF = null;
		m_cellWidth.Clear();
		m_cellWidth = null;
		m_cellBorders.Clear();
		m_cellBorders = null;
	}

	private static void UpdateXCenterArray(short[] m_xCenterArray, WTableRow row, bool isOldFormat)
	{
		bool flag = false;
		WTableColumnCollection tableGrid = row.OwnerTable.TableGrid;
		for (int i = 0; i < row.Cells.Count; i++)
		{
			if (i > m_xCenterArray.Length - 2)
			{
				throw new ArgumentOutOfRangeException("cellIndex");
			}
			WTableCell wTableCell = row.Cells[i];
			Dictionary<int, object> dictionary = (isOldFormat ? wTableCell.CellFormat.OldPropertiesHash : wTableCell.CellFormat.PropertiesHash);
			float num = (dictionary.ContainsKey(12) ? ((float)dictionary[12]) : 0f);
			if (num > 1638f)
			{
				short num2 = (short)Math.Round(num);
				m_xCenterArray[i + 1] = (short)(num2 + m_xCenterArray[i]);
			}
			else if ((num == 0f || row.Cells[i].GridSpan > 1) && tableGrid.Count != 0)
			{
				short num3 = 0;
				short num4 = (short)(row.Cells[i].GridColumnStartIndex + row.Cells[i].GridSpan);
				short gridColumnStartIndex = row.Cells[i].GridColumnStartIndex;
				if (gridColumnStartIndex > 0 && gridColumnStartIndex < tableGrid.Count && num4 > 0 && num4 < tableGrid.Count)
				{
					if (row.Cells[i].GridSpan != 1)
					{
						if (tableGrid.Count > num4)
						{
							num3 = (short)Math.Round((gridColumnStartIndex == 0) ? tableGrid[num4 - 1].EndOffset : (tableGrid[num4 - 1].EndOffset - tableGrid[gridColumnStartIndex - 1].EndOffset));
							flag = true;
						}
					}
					else if (flag)
					{
						if (tableGrid.Count > num4)
						{
							num3 = (short)Math.Round(tableGrid[num4 - 1].EndOffset - tableGrid[gridColumnStartIndex - 1].EndOffset);
						}
					}
					else if (tableGrid.Count > num4)
					{
						num3 = (short)Math.Round(tableGrid[num4 - 1].EndOffset - tableGrid[gridColumnStartIndex - 1].EndOffset);
					}
				}
				else
				{
					num3 = (short)Math.Round(num * 20f);
				}
				m_xCenterArray[i + 1] = (short)(num3 + m_xCenterArray[i]);
			}
			else
			{
				short num5 = (short)Math.Round(num * 20f);
				m_xCenterArray[i + 1] = (short)(num5 + m_xCenterArray[i]);
			}
		}
	}

	private static void UpdateCenterArray(SinglePropertyModifierArray sprms, short[] m_xCenterArray, WTableRow tableRow, bool isOldFormat)
	{
		WTable ownerTable = tableRow.OwnerTable;
		short num = m_xCenterArray[0];
		short num2 = 0;
		byte[] byteArray = sprms.GetByteArray(63073);
		if (byteArray != null && byteArray.Length == 3)
		{
			num2 = BitConverter.ToInt16(byteArray, 1);
		}
		short num3 = num2;
		if (sprms.GetInt(26185, 1) == 1)
		{
			short num4 = 0;
			if (sprms.TryGetSprm(62999) != null)
			{
				byteArray = sprms.GetByteArray(62999);
				if (byteArray != null && byteArray.Length == 3)
				{
					num4 = BitConverter.ToInt16(byteArray, 1);
				}
			}
			Dictionary<int, object> dictionary = null;
			Paddings paddings = null;
			Dictionary<int, object> dictionary2 = null;
			Paddings paddings2 = null;
			if (ownerTable != null && ownerTable.Rows.Count > 0)
			{
				if (ownerTable.Rows[0].Cells.Count > 0)
				{
					dictionary = (isOldFormat ? ownerTable.Rows[0].Cells[0].CellFormat.OldPropertiesHash : ownerTable.Rows[0].Cells[0].CellFormat.PropertiesHash);
					if (dictionary != null && dictionary.ContainsKey(3))
					{
						paddings = dictionary[3] as Paddings;
					}
				}
				dictionary2 = (isOldFormat ? ownerTable.Rows[0].RowFormat.OldPropertiesHash : ownerTable.Rows[0].RowFormat.PropertiesHash);
				if (dictionary2 != null && dictionary2.ContainsKey(3))
				{
					paddings2 = dictionary2[3] as Paddings;
				}
			}
			if (paddings != null && paddings.HasKey(1))
			{
				num3 -= (short)(paddings.Left * 20f);
				num3 += num4;
			}
			else if (paddings2 != null && paddings2.HasKey(1))
			{
				num3 -= (short)(paddings2.Left * 20f);
				num3 += num4;
			}
			else
			{
				num3 += num4;
			}
		}
		byte[] byteArray2 = sprms.GetByteArray(54835);
		ushort num5 = 0;
		if (byteArray2 != null && byteArray2.Length == 6 && (byteArray2[3] == 3 || byteArray2[3] == 19) && byteArray2[2] == 15)
		{
			num5 = BitConverter.ToUInt16(byteArray2, 4);
		}
		if (num5 > 0)
		{
			num3 -= (short)(num5 * 2);
			byte[] byteArray3 = sprms.GetByteArray(54803);
			bool flag = false;
			if (byteArray3 == null)
			{
				byteArray3 = sprms.GetByteArray(54789);
				flag = true;
			}
			if (byteArray3 != null)
			{
				TableBorders tableBorders = new TableBorders();
				for (int i = 0; i < 2; i++)
				{
					tableBorders[i] = new BorderCode();
					if (flag)
					{
						tableBorders[i].Parse(byteArray3, i * 4);
					}
					else
					{
						tableBorders[i].ParseNewBrc(byteArray3, i * 8);
					}
				}
				num3 -= (short)Math.Round((float)(int)tableBorders.LeftBorder.LineWidth / 8f * 20f);
			}
		}
		if (num != num3)
		{
			m_xCenterArray[0] = num3;
			for (int j = 1; j < m_xCenterArray.Length; j++)
			{
				m_xCenterArray[j] += (short)(num3 - num);
			}
		}
	}

	private static void InitCollection(int cellCount)
	{
		m_cellShadings = new byte[cellCount * 2];
		m_cellTCGRF = new Dictionary<short, byte[]>(cellCount);
		m_cellWidth = new Dictionary<short, byte[]>(cellCount);
		m_cellBorders = new Dictionary<short, byte[]>(cellCount);
		m_topBorderCV = new byte[cellCount * 4];
		m_leftBorderCV = new byte[cellCount * 4];
		m_rightBorderCV = new byte[cellCount * 4];
		m_bottomBorderCV = new byte[cellCount * 4];
		UpdateDefaultBorderCV(cellCount);
		if (cellCount < 23)
		{
			m_cellShadings_1st = new byte[cellCount * 10];
			UpdateDefaultShadingBytes(m_cellShadings_1st, cellCount);
		}
		else if (cellCount < 45)
		{
			m_cellShadings_1st = new byte[220];
			UpdateDefaultShadingBytes(m_cellShadings_1st, 22);
			m_cellShadings_2nd = new byte[(cellCount - 22) * 10];
			UpdateDefaultShadingBytes(m_cellShadings_2nd, cellCount - 22);
		}
		else
		{
			m_cellShadings_1st = new byte[220];
			UpdateDefaultShadingBytes(m_cellShadings_1st, 22);
			m_cellShadings_2nd = new byte[220];
			UpdateDefaultShadingBytes(m_cellShadings_2nd, 22);
			m_cellShadings_3rd = new byte[(cellCount - 44) * 10];
			UpdateDefaultShadingBytes(m_cellShadings_3rd, cellCount - 44);
		}
	}

	private static void UpdateDefaultBorderCV(int cellCount)
	{
		byte[] src = new byte[4] { 0, 0, 0, 255 };
		int i = 0;
		int num = 0;
		for (; i < cellCount; i++)
		{
			Buffer.BlockCopy(src, 0, m_topBorderCV, num, 4);
			Buffer.BlockCopy(src, 0, m_bottomBorderCV, num, 4);
			Buffer.BlockCopy(src, 0, m_leftBorderCV, num, 4);
			Buffer.BlockCopy(src, 0, m_rightBorderCV, num, 4);
			num += 4;
		}
	}

	private static void UpdateDefaultShadingBytes(byte[] operand, int cellCount)
	{
		byte[] src = new byte[4] { 0, 0, 0, 255 };
		int i = 0;
		int num = 0;
		for (; i < cellCount; i++)
		{
			Buffer.BlockCopy(src, 0, operand, num, 4);
			num += 4;
			Buffer.BlockCopy(src, 0, operand, num, 4);
			num += 4;
			num += 2;
		}
	}

	private static void UpdateCellsInformation(SinglePropertyModifierArray sprms)
	{
		if (m_cellShadings != null)
		{
			sprms.SetByteArrayValue(54793, m_cellShadings.Clone() as byte[]);
			Array.Clear(m_cellShadings, 0, m_cellShadings.Length);
			m_cellShadings = null;
		}
		if (m_cellShadings_1st != null)
		{
			sprms.SetByteArrayValue(54896, m_cellShadings_1st.Clone() as byte[]);
			sprms.SetByteArrayValue(54802, m_cellShadings_1st.Clone() as byte[]);
			Array.Clear(m_cellShadings_1st, 0, m_cellShadings_1st.Length);
			m_cellShadings_1st = null;
		}
		if (m_cellShadings_2nd != null)
		{
			sprms.SetByteArrayValue(54897, m_cellShadings_2nd.Clone() as byte[]);
			sprms.SetByteArrayValue(54806, m_cellShadings_2nd.Clone() as byte[]);
			Array.Clear(m_cellShadings_2nd, 0, m_cellShadings_2nd.Length);
			m_cellShadings_2nd = null;
		}
		if (m_cellShadings_3rd != null)
		{
			sprms.SetByteArrayValue(54898, m_cellShadings_3rd.Clone() as byte[]);
			sprms.SetByteArrayValue(54796, m_cellShadings_3rd.Clone() as byte[]);
			Array.Clear(m_cellShadings_3rd, 0, m_cellShadings_3rd.Length);
			m_cellShadings_3rd = null;
		}
		if (m_topBorderCV != null)
		{
			sprms.SetByteArrayValue(54810, m_topBorderCV.Clone() as byte[]);
			Array.Clear(m_topBorderCV, 0, m_topBorderCV.Length);
			m_topBorderCV = null;
		}
		if (m_leftBorderCV != null)
		{
			sprms.SetByteArrayValue(54811, m_leftBorderCV.Clone() as byte[]);
			Array.Clear(m_leftBorderCV, 0, m_leftBorderCV.Length);
			m_leftBorderCV = null;
		}
		if (m_bottomBorderCV != null)
		{
			sprms.SetByteArrayValue(54812, m_bottomBorderCV.Clone() as byte[]);
			Array.Clear(m_bottomBorderCV, 0, m_bottomBorderCV.Length);
			m_bottomBorderCV = null;
		}
		if (m_rightBorderCV != null)
		{
			sprms.SetByteArrayValue(54813, m_rightBorderCV.Clone() as byte[]);
			Array.Clear(m_rightBorderCV, 0, m_rightBorderCV.Length);
			m_rightBorderCV = null;
		}
		if (m_cellBorderType != null)
		{
			sprms.SetByteArrayValue(54882, m_cellBorderType.Clone() as byte[]);
			Array.Clear(m_cellBorderType, 0, m_cellBorderType.Length);
			m_cellBorderType = null;
		}
	}

	internal static void FormatToSprms(int key, object value, SinglePropertyModifierArray sprms, RowFormat rowFormat, WordStyleSheet styleSheet, bool isOldFormat)
	{
		lock (m_threadLocker)
		{
			byte[] array = null;
			switch (key)
			{
			case 105:
				sprms.SetShortValue(21642, (short)(ParagraphJustify)value);
				sprms.SetShortValue(21504, (short)(ParagraphJustify)value);
				break;
			case 106:
				sprms.SetBoolValue(13315, !(bool)value);
				sprms.SetBoolValue(13414, !(bool)value);
				break;
			case 107:
				sprms.SetBoolValue(13316, (bool)value);
				break;
			case 1:
				SetTableBorders(sprms, (Borders)value);
				break;
			case 2:
			{
				short num = (short)Math.Round((float)value * 20f);
				num = (short)((rowFormat.OwnerRow.HeightType == TableRowHeightType.AtLeast) ? num : (-num));
				sprms.SetShortValue(37895, num);
				break;
			}
			case 104:
				sprms.SetBoolValue(22027, (bool)value);
				break;
			case 120:
				SetTablePositioning((RowFormat.TablePositioning)value, sprms);
				break;
			case 11:
			case 12:
				SetPreferredWidthInfo(11, 12, 62996, sprms, rowFormat, isOldFormat);
				break;
			case 13:
			case 14:
				SetPreferredWidthInfo(13, 14, 62999, sprms, rowFormat, isOldFormat);
				break;
			case 15:
			case 16:
				SetPreferredWidthInfo(15, 16, 63000, sprms, rowFormat, isOldFormat);
				break;
			case 103:
				sprms.SetBoolValue(13845, (bool)value);
				break;
			case 52:
				if ((float)value >= 0f)
				{
					array = new byte[6] { 0, 1, 15, 3, 0, 0 };
					Buffer.BlockCopy(BitConverter.GetBytes((short)((float)value * 20f)), 0, array, 4, 2);
					sprms.SetByteArrayValue(54835, array);
				}
				break;
			case 3:
				SetPaddings(sprms, 54836, (Paddings)value, 0);
				break;
			case 108:
			case 110:
			case 111:
				SetTableShading(sprms, 54880, rowFormat, rowFormat.IsFormattingChange);
				break;
			case 53:
				array = new byte[3] { 3, 0, 0 };
				Buffer.BlockCopy(BitConverter.GetBytes((short)Math.Round((float)value * 20f)), 0, array, 1, 2);
				sprms.SetByteArrayValue(63073, array);
				break;
			case 123:
			case 124:
				if (!sprms.Contain(51849) && rowFormat.IsChangedFormat)
				{
					byte[] array2 = new byte[7] { 1, 0, 0, 0, 0, 0, 0 };
					if (!CharacterPropertiesConverter.AuthorNames.Contains(rowFormat.FormatChangeAuthorName))
					{
						CharacterPropertiesConverter.AuthorNames.Add(rowFormat.FormatChangeAuthorName);
					}
					byte[] bytes = BitConverter.GetBytes((short)CharacterPropertiesConverter.AuthorNames.IndexOf(rowFormat.FormatChangeAuthorName));
					byte[] src = new byte[4];
					if (rowFormat.HasValue(15))
					{
						src = BitConverter.GetBytes(rowFormat.GetDTTMIntValue(rowFormat.FormatChangeDateTime));
					}
					Buffer.BlockCopy(bytes, 0, array2, 1, 2);
					Buffer.BlockCopy(src, 0, array2, 3, 4);
					sprms.SetByteArrayValue(54887, array2);
				}
				break;
			}
		}
	}

	private static void FormatToSprms(int key, object value, SinglePropertyModifierArray sprms, CellFormat cellFormat, bool isOldFormat, int cellIndex, ref ushort tcgrf)
	{
		Dictionary<int, object> dictionary = (isOldFormat ? cellFormat.OldPropertiesHash : cellFormat.PropertiesHash);
		lock (m_threadLocker)
		{
			switch (key)
			{
			case 3:
				if (!cellFormat.SamePaddingsAsTable)
				{
					SetPaddings(sprms, 54834, (Paddings)value, cellIndex);
				}
				break;
			case 1:
				SetCellBorders((Borders)value, cellIndex, cellFormat);
				break;
			case 8:
				switch ((CellMerge)value)
				{
				case CellMerge.Start:
					tcgrf = (ushort)((tcgrf & 0xFFFCu) | 1u);
					break;
				case CellMerge.Continue:
					tcgrf = (ushort)((tcgrf & 0xFFFCu) | 2u);
					break;
				}
				break;
			case 11:
				switch ((TextDirection)value)
				{
				case TextDirection.VerticalTopToBottom:
					tcgrf = (ushort)((tcgrf & 0xFFE3u) | 4u);
					break;
				case TextDirection.VerticalBottomToTop:
					tcgrf = (ushort)((tcgrf & 0xFFE3u) | 0xCu);
					break;
				case TextDirection.HorizontalFarEast:
					tcgrf = (ushort)((tcgrf & 0xFFE3u) | 0x10u);
					break;
				case TextDirection.VerticalFarEast:
					tcgrf = (ushort)((tcgrf & 0xFFE3u) | 0x14u);
					break;
				case TextDirection.Vertical:
					tcgrf = (ushort)((tcgrf & 0xFFE3u) | 0x1Cu);
					break;
				}
				break;
			case 6:
				switch ((CellMerge)value)
				{
				case CellMerge.Start:
					tcgrf = (ushort)((tcgrf & 0xFF9Fu) | 0x60u);
					break;
				case CellMerge.Continue:
					tcgrf = (ushort)((tcgrf & 0xFF9Fu) | 0x20u);
					break;
				}
				break;
			case 2:
				tcgrf = (ushort)((tcgrf & 0xFE7Fu) | (uint)((byte)(VerticalAlignment)value << 7));
				break;
			case 13:
				tcgrf = (ushort)((tcgrf & 0xF1FFu) | (uint)((byte)(FtsWidth)value << 9));
				break;
			case 14:
				if (dictionary.ContainsKey(13))
				{
					ushort value2 = 0;
					switch ((FtsWidth)dictionary[13])
					{
					case FtsWidth.Percentage:
						value2 = (ushort)Math.Round((float)value * 50f);
						break;
					case FtsWidth.Point:
						value2 = (ushort)Math.Round((float)value * 20f);
						break;
					}
					m_cellWidth[(short)cellIndex] = BitConverter.GetBytes(value2);
				}
				break;
			case 10:
				tcgrf = (ushort)((tcgrf & 0xEFFFu) | (((bool)value) ? 1u : 0u));
				break;
			case 9:
				tcgrf = (ushort)((tcgrf & 0xDFFFu) | ((!(bool)value) ? 8192u : 0u));
				break;
			case 4:
			case 5:
			case 7:
			case 12:
				break;
			}
		}
	}

	private static void SetShading(Dictionary<int, object> propertyHash, int cellIndex, bool isRowFormat)
	{
		int key;
		int key2;
		int key3;
		if (isRowFormat)
		{
			key = 108;
			key2 = 111;
			key3 = 110;
		}
		else
		{
			key = 4;
			key2 = 5;
			key3 = 7;
		}
		ShadingDescriptor shadingDescriptor = new ShadingDescriptor();
		if (propertyHash.ContainsKey(key))
		{
			shadingDescriptor.BackColor = (Color)propertyHash[key];
		}
		if (propertyHash.ContainsKey(key2))
		{
			shadingDescriptor.ForeColor = (Color)propertyHash[key2];
		}
		if (propertyHash.ContainsKey(key3))
		{
			shadingDescriptor.Pattern = (TextureStyle)propertyHash[key3];
		}
		Buffer.BlockCopy(BitConverter.GetBytes(shadingDescriptor.Save()), 0, m_cellShadings, cellIndex * 2, 2);
		if (cellIndex < 22)
		{
			Buffer.BlockCopy(shadingDescriptor.SaveNewShd(), 0, m_cellShadings_1st, cellIndex * 10, 10);
		}
		else if (cellIndex < 44)
		{
			Buffer.BlockCopy(shadingDescriptor.SaveNewShd(), 0, m_cellShadings_2nd, (cellIndex - 22) * 10, 10);
		}
		else if (cellIndex < 63)
		{
			Buffer.BlockCopy(shadingDescriptor.SaveNewShd(), 0, m_cellShadings_3rd, (cellIndex - 44) * 10, 10);
		}
	}

	private static void SetTableBorders(SinglePropertyModifierArray sprms, Borders borders)
	{
		TableBorders tableBorders = new TableBorders();
		BorderToBRC(borders.Top, tableBorders.TopBorder);
		BorderToBRC(borders.Left, tableBorders.LeftBorder);
		BorderToBRC(borders.Bottom, tableBorders.BottomBorder);
		BorderToBRC(borders.Right, tableBorders.RightBorder);
		BorderToBRC(borders.Horizontal, tableBorders.HorizontalBorder);
		BorderToBRC(borders.Vertical, tableBorders.VerticalBorder);
		byte[] array = new byte[24];
		for (int i = 0; i < 6; i++)
		{
			tableBorders[i].SaveBytes(array, i * 4);
		}
		sprms.SetByteArrayValue(54789, array);
		array = new byte[48];
		for (int j = 0; j < 6; j++)
		{
			tableBorders[j].SaveNewBrc(array, j * 8);
		}
		sprms.SetByteArrayValue(54803, array);
	}

	private static void SetCellBorders(Borders borders, int cellIndex, CellFormat cellFormat)
	{
		int num = cellIndex * 4;
		BorderStructure[] array = new BorderStructure[4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new BorderStructure();
		}
		if (borders.Left.BorderType == BorderStyle.Thick)
		{
			borders.Left.BorderType = BorderStyle.Single;
		}
		if (borders.Right.BorderType == BorderStyle.Thick)
		{
			borders.Right.BorderType = BorderStyle.Single;
		}
		if (borders.Top.BorderType == BorderStyle.Thick)
		{
			borders.Top.BorderType = BorderStyle.Single;
		}
		if (borders.Bottom.BorderType == BorderStyle.Thick)
		{
			borders.Bottom.BorderType = BorderStyle.Single;
		}
		if (borders.Left.BorderType != 0 || borders.Right.BorderType != 0 || borders.Top.BorderType != 0 || borders.Bottom.BorderType != 0)
		{
			if (borders.Top.BorderType != 0 || borders.Top.HasNoneStyle)
			{
				BorderToBRC(borders.Top, array[0]);
			}
			if (borders.Left.BorderType != 0 || borders.Left.HasNoneStyle)
			{
				BorderToBRC(borders.Left, array[1]);
			}
			if (borders.Bottom.BorderType != 0 || borders.Bottom.HasNoneStyle)
			{
				BorderToBRC(borders.Bottom, array[2]);
			}
			if (borders.Right.BorderType != 0 || borders.Right.HasNoneStyle)
			{
				BorderToBRC(borders.Right, array[3]);
			}
			SetCellBorderColors(borders, num);
		}
		else
		{
			if (borders.Left.HasNoneStyle && borders.Left.BorderType == BorderStyle.None && !borders.Left.Color.IsEmpty && borders.Left.Color != Color.Black)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(Color.White)), 0, m_leftBorderCV, num, 4);
			}
			if (borders.Right.HasNoneStyle && borders.Right.BorderType == BorderStyle.None && !borders.Right.Color.IsEmpty && borders.Right.Color != Color.Black)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(Color.White)), 0, m_rightBorderCV, num, 4);
			}
			if (borders.Top.HasNoneStyle && borders.Top.BorderType == BorderStyle.None && !borders.Top.Color.IsEmpty && borders.Top.Color != Color.Black)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(Color.White)), 0, m_topBorderCV, num, 4);
			}
			if (borders.Bottom.HasNoneStyle && borders.Bottom.BorderType == BorderStyle.None && !borders.Bottom.Color.IsEmpty && borders.Bottom.Color != Color.Black)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(Color.White)), 0, m_bottomBorderCV, num, 4);
			}
		}
		byte[] array2 = new byte[16];
		for (int j = 0; j < 4; j++)
		{
			array[j].Save(array2, j * 4);
		}
		m_cellBorders[(short)cellIndex] = array2;
		if (borders.Left.BorderType != 0 || borders.Left.HasNoneStyle || borders.Top.BorderType != 0 || borders.Top.HasNoneStyle || borders.Right.BorderType != 0 || borders.Right.HasNoneStyle || borders.Bottom.BorderType != 0 || borders.Bottom.HasNoneStyle)
		{
			if (m_cellBorderType == null)
			{
				m_cellBorderType = new byte[(cellFormat.OwnerBase.OwnerBase as WTableRow).Cells.Count * 4];
			}
			m_cellBorderType[num] = (byte)borders.Top.BorderType;
			m_cellBorderType[num + 1] = (byte)borders.Left.BorderType;
			m_cellBorderType[num + 2] = (byte)borders.Bottom.BorderType;
			m_cellBorderType[num + 3] = (byte)borders.Right.BorderType;
		}
	}

	private static void SetCellBorderColors(Borders borders, int startPos)
	{
		if (borders.Top.HasKey(1))
		{
			Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(borders.Top.Color)), 0, m_topBorderCV, startPos, 4);
		}
		if (borders.Left.HasKey(1))
		{
			Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(borders.Left.Color)), 0, m_leftBorderCV, startPos, 4);
		}
		if (borders.Bottom.HasKey(1))
		{
			Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(borders.Bottom.Color)), 0, m_bottomBorderCV, startPos, 4);
		}
		if (borders.Right.HasKey(1))
		{
			Buffer.BlockCopy(BitConverter.GetBytes(WordColor.ConvertColorToRGB(borders.Right.Color)), 0, m_rightBorderCV, startPos, 4);
		}
	}

	private static void SetPreferredWidthInfo(int widthTypeKey, int widthKey, int sprmOption, SinglePropertyModifierArray sprms, RowFormat rowFormat, bool isOldFormat)
	{
		if (sprms[sprmOption] == null)
		{
			byte[] array = new byte[3];
			FtsWidth ftsWidth = (isOldFormat ? ((FtsWidth)rowFormat.OldPropertiesHash[widthTypeKey]) : ((FtsWidth)rowFormat.PropertiesHash[widthTypeKey]));
			array[0] = (byte)ftsWidth;
			float num = (isOldFormat ? ((float)rowFormat.GetKeyValue(rowFormat.OldPropertiesHash, widthKey)) : ((float)rowFormat.GetKeyValue(rowFormat.PropertiesHash, widthKey)));
			short value = 0;
			switch (ftsWidth)
			{
			case FtsWidth.Percentage:
				value = (short)Math.Round(num * 50f);
				break;
			case FtsWidth.Point:
				value = (short)Math.Round(num * 20f);
				break;
			}
			if (ftsWidth > FtsWidth.Auto)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(value), 0, array, 1, 2);
			}
			sprms.SetByteArrayValue(sprmOption, array);
		}
	}

	internal static void SetTablePositioning(RowFormat.TablePositioning tablePositioning, SinglePropertyModifierArray sprms)
	{
		if (tablePositioning.m_ownerRowFormat.WrapTextAround)
		{
			sprms.SetByteValue(13837, (byte)((((byte)tablePositioning.HorizRelationTo << 2) | (byte)tablePositioning.VertRelationTo) << 4));
			short value = ((tablePositioning.HorizPositionAbs != 0) ? ((short)tablePositioning.HorizPositionAbs) : ((short)(tablePositioning.HorizPosition * 20f)));
			sprms.SetShortValue(37902, value);
			value = ((tablePositioning.VertPositionAbs != 0) ? ((short)tablePositioning.VertPositionAbs) : ((short)(tablePositioning.VertPosition * 20f)));
			sprms.SetShortValue(37903, value);
			if (tablePositioning.DistanceFromLeft != 0f)
			{
				sprms.SetShortValue(37904, (short)(tablePositioning.DistanceFromLeft * 20f));
			}
			if (tablePositioning.DistanceFromTop != 0f)
			{
				sprms.SetShortValue(37905, (short)(tablePositioning.DistanceFromTop * 20f));
			}
			if (tablePositioning.DistanceFromRight != 0f)
			{
				sprms.SetShortValue(37918, (short)(tablePositioning.DistanceFromRight * 20f));
			}
			if (tablePositioning.DistanceFromBottom != 0f)
			{
				sprms.SetShortValue(37919, (short)(tablePositioning.DistanceFromBottom * 20f));
			}
			if (!tablePositioning.AllowOverlap)
			{
				sprms.SetBoolValue(13413, !tablePositioning.AllowOverlap);
			}
		}
	}

	private static void SetPaddings(SinglePropertyModifierArray sprms, int options, Paddings paddings, int cellIndex)
	{
		Spacings spacings = new Spacings();
		ImportPaddings(spacings, paddings);
		if (!spacings.IsEmpty)
		{
			spacings.CellNumber = cellIndex;
			spacings.Save(sprms, options, cellIndex);
		}
	}

	private static void SetTableShading(SinglePropertyModifierArray sprms, int options, RowFormat rowFormat, bool isChangedFormat)
	{
		if (!sprms.Contain(54880))
		{
			Dictionary<int, object> dictionary = (isChangedFormat ? rowFormat.OldPropertiesHash : rowFormat.PropertiesHash);
			ShadingDescriptor shadingDescriptor = new ShadingDescriptor();
			if (dictionary.ContainsKey(108))
			{
				shadingDescriptor.BackColor = (Color)dictionary[108];
			}
			if (dictionary.ContainsKey(111))
			{
				shadingDescriptor.ForeColor = (Color)dictionary[111];
			}
			if (dictionary.ContainsKey(110))
			{
				shadingDescriptor.Pattern = (TextureStyle)dictionary[110];
			}
			sprms.SetByteArrayValue(options, shadingDescriptor.SaveNewShd());
		}
	}

	private static void BorderToBRC(Border border, BorderCode brc)
	{
		if (border.BorderType == BorderStyle.Cleared)
		{
			border.Color = Color.Empty;
			border.LineWidth = 0f;
		}
		else if (border.BorderType == BorderStyle.None && !border.HasNoneStyle)
		{
			border.BorderType = BorderStyle.Single;
		}
		else if (border.BorderType == BorderStyle.Hairline)
		{
			border.BorderType = BorderStyle.Single;
		}
		if (!border.IsDefault)
		{
			if (border.BorderType == BorderStyle.Cleared)
			{
				brc.BorderType = byte.MaxValue;
				brc.LineColor = 0;
				brc.LineColorExt = Color.FromArgb(0, 255, 255, 255);
				brc.LineWidth = byte.MaxValue;
			}
			else if (border.BorderType != 0)
			{
				brc.BorderType = (byte)border.BorderType;
				brc.LineColor = (byte)WordColor.ConvertColorToId(border.Color);
				brc.LineColorExt = border.Color;
				brc.LineWidth = (byte)(border.LineWidth * 8f);
			}
			brc.Shadow = border.Shadow;
		}
	}

	private static void BorderToBRC(Border border, BorderStructure brc)
	{
		if (border.BorderType == BorderStyle.Hairline)
		{
			border.BorderType = BorderStyle.Single;
		}
		if (!border.IsDefault && border.BorderType == BorderStyle.Cleared)
		{
			brc.BorderType = byte.MaxValue;
			brc.LineColor = byte.MaxValue;
			brc.LineWidth = byte.MaxValue;
			brc.Props = byte.MaxValue;
		}
		else if (!border.IsDefault && (border.BorderType != 0 || border.HasNoneStyle))
		{
			if (brc.BorderType == byte.MaxValue && (byte)border.BorderType != byte.MaxValue)
			{
				brc.Props = 0;
			}
			brc.BorderType = (byte)border.BorderType;
			brc.LineWidth = (byte)(border.LineWidth * 8f);
			brc.Shadow = border.Shadow;
			brc.LineColor = (byte)WordColor.ColorToId(border.Color);
		}
	}

	private static void BRCToBorder(BorderCode brc, Border border)
	{
		Color lineColorExt = brc.LineColorExt;
		float lineWidth = (float)(int)brc.LineWidth / 8f;
		BorderStyle borderType = (BorderStyle)brc.BorderType;
		bool shadow = brc.Shadow;
		border.InitFormatting(lineColorExt, lineWidth, borderType, shadow);
	}

	private static void BRCToBorder(BorderStructure brc, Border border)
	{
		if (IsEmpty(brc))
		{
			return;
		}
		if (!brc.IsClear)
		{
			Color color = WordColor.IdToColor(brc.LineColor);
			float lineWidth = (float)(int)brc.LineWidth / 8f;
			BorderStyle borderType = (BorderStyle)brc.BorderType;
			bool shadow = brc.Shadow;
			border.InitFormatting(color, lineWidth, borderType, shadow);
			if (border.BorderType == BorderStyle.None)
			{
				border.HasNoneStyle = true;
			}
		}
		else
		{
			border.BorderType = BorderStyle.Cleared;
			border.HasNoneStyle = false;
		}
	}

	private static bool IsEmpty(BorderStructure brc)
	{
		if (brc.LineColor == 0 && brc.LineWidth == 0 && brc.BorderType == 0)
		{
			return true;
		}
		return false;
	}

	public static void ImportPaddings(Spacings destination, Paddings source)
	{
		destination.Left = (short)Math.Round(source.Left * 20f);
		destination.Right = (short)Math.Round(source.Right * 20f);
		destination.Top = (short)Math.Round(source.Top * 20f);
		destination.Bottom = (short)Math.Round(source.Bottom * 20f);
	}

	public static void ExportPaddings(Spacings source, Paddings destination)
	{
		if (source != null)
		{
			if (source.Left >= 0)
			{
				destination.Left = (float)source.Left / 20f;
			}
			if (source.Right >= 0)
			{
				destination.Right = (float)source.Right / 20f;
			}
			if (source.Top >= 0)
			{
				destination.Top = (float)source.Top / 20f;
			}
			if (source.Bottom >= 0)
			{
				destination.Bottom = (float)source.Bottom / 20f;
			}
		}
	}

	internal static void ExportDefaultPaddings(Paddings destination)
	{
		destination.Left = 0f;
		destination.Right = 0f;
		destination.Top = 0f;
		destination.Bottom = 0f;
	}
}
