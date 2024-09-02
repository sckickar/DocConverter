using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using DocGen.DocIO.DLS.Convertors;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

internal class HTMLConverterImpl : IHtmlConverter
{
	internal class TableGrid
	{
		private Stack<Dictionary<int, List<KeyValuePair<int, float>>>> m_tblGridStack = new Stack<Dictionary<int, List<KeyValuePair<int, float>>>>();

		internal Stack<Dictionary<int, List<KeyValuePair<int, float>>>> TableGridStack
		{
			get
			{
				return m_tblGridStack;
			}
			set
			{
				m_tblGridStack = value;
			}
		}
	}

	internal class SpanHelper
	{
		private int m_curCol;

		internal List<KeyValuePair<int, float>> m_tblGrid = new List<KeyValuePair<int, float>>();

		internal Dictionary<WTableCell, int> m_rowspans = new Dictionary<WTableCell, int>();

		internal float m_tableWidth;

		private Dictionary<int, List<KeyValuePair<int, float>>> m_tblGridCollection = new Dictionary<int, List<KeyValuePair<int, float>>>();

		internal Dictionary<int, List<KeyValuePair<int, float>>> TableGridCollection
		{
			get
			{
				return m_tblGridCollection;
			}
			set
			{
				m_tblGridCollection = value;
			}
		}

		internal void ResetCurrColumn()
		{
			m_curCol = 0;
		}

		internal void AddCellToGrid(WTableCell cell, int colSpan)
		{
			m_tblGrid.Add(new KeyValuePair<int, float>(colSpan, cell.Width));
		}

		internal void NextColumn()
		{
			m_curCol++;
		}

		private List<object> GetTableGrid(Dictionary<int, List<KeyValuePair<int, float>>> tableGridCollection, WTable table, float clientWidth)
		{
			int num = 0;
			for (int i = 0; i < tableGridCollection.Count; i++)
			{
				List<KeyValuePair<int, float>> list = tableGridCollection[i];
				int num2 = 0;
				for (int j = 0; j < list.Count; j++)
				{
					num2 += list[j].Key;
				}
				if (num < num2)
				{
					num = num2;
				}
			}
			float[] array = new float[num + 1];
			for (int k = 0; k < tableGridCollection.Count; k++)
			{
				List<KeyValuePair<int, float>> list2 = tableGridCollection[k];
				float num3 = 0f;
				int num4 = 0;
				for (int l = 0; l < list2.Count; l++)
				{
					num3 = (float)Math.Round(num3 + list2[l].Value, 2);
					num4 += list2[l].Key;
					if (array[num4] < num3)
					{
						array[num4] = num3;
					}
				}
			}
			int num5 = array.Length - 1;
			float num6 = array[num5];
			for (int num7 = array.Length - 1; num7 >= 1; num7--)
			{
				if (array[num7 - 1] >= num6)
				{
					num5 = num7 - 1;
				}
				num6 = array[num7 - 1];
			}
			float num8 = ((m_tableWidth != 0f) ? m_tableWidth : clientWidth);
			float num9 = array[num5];
			if (num9 == 0f && num5 > 0)
			{
				num5--;
			}
			if (num5 < array.Length - 1)
			{
				float num10 = (float)Math.Round((num8 - num9) / (float)(array.Length - 1 - num5), 2);
				if (num8 <= num9)
				{
					num10 = 3f;
				}
				for (int m = num5 + 1; m < array.Length; m++)
				{
					num9 = (array[m] = (float)Math.Round(num9 + num10, 2));
				}
			}
			if (array[^1] != num8 && m_tableWidth != 0f)
			{
				float num11 = num8 / array[^1];
				for (int n = 1; n < array.Length; n++)
				{
					array[n] = (float)Math.Round(array[n] * num11, 2);
				}
				if (array[^1] > num8)
				{
					array[^1] = num8;
				}
			}
			List<object> list3 = new List<object>();
			for (int num12 = 1; num12 < array.Length; num12++)
			{
				list3.Add((float)Math.Round(array[num12] - array[num12 - 1], 2));
			}
			return list3;
		}

		internal void UpdateTable(WTable table, Stack<Dictionary<int, List<KeyValuePair<int, float>>>> tableGridStack, float clientWidth)
		{
			m_tblGridCollection = tableGridStack.Pop();
			UpdateRowSpan(table);
			List<object> tableGrid = GetTableGrid(m_tblGridCollection, table, clientWidth);
			int num = 0;
			for (int i = 0; i < table.Rows.Count; i++)
			{
				WTableRow wTableRow = table.Rows[i];
				List<KeyValuePair<int, float>> list = m_tblGridCollection[i];
				int num2 = 0;
				for (int j = 0; j < wTableRow.Cells.Count; j++)
				{
					WTableCell wTableCell = wTableRow.Cells[j];
					int num3 = 1;
					if (j <= list.Count - 1)
					{
						num3 = list[j].Key;
					}
					wTableCell.Width = GetWidth(tableGrid, num2, num3);
					num2 += num3;
					if (j + 1 == wTableRow.Cells.Count && i > 0 && num2 < num)
					{
						wTableRow.AddCell(isCopyFormat: false);
						continue;
					}
					if (num < num2)
					{
						num = num2;
					}
					if (j + 1 == wTableRow.Cells.Count && num2 < tableGrid.Count)
					{
						num3 = tableGrid.Count - num;
						wTableRow.RowFormat.AfterWidth = GetWidth(tableGrid, num2, num3);
					}
				}
			}
		}

		private float GetWidth(List<object> tblGrid, int columnIndex, int columnSpan)
		{
			float num = 0f;
			for (int i = columnIndex; i < columnIndex + columnSpan; i++)
			{
				num += (float)tblGrid[i];
			}
			return num;
		}

		private void UpdateRowSpan(WTable table)
		{
			int num = 0;
			int num2 = 1;
			for (int i = 1; i <= num2; i++)
			{
				int num3 = 0;
				for (int j = 0; j < table.Rows.Count; j++)
				{
					WTableRow wTableRow = table.Rows[j];
					if (num < wTableRow.Cells.Count)
					{
						if (table.Rows[j].Cells.Count > num2)
						{
							num2 = table.Rows[j].Cells.Count;
						}
						WTableCell wTableCell = wTableRow.Cells[num];
						m_tblGrid = m_tblGridCollection[num3];
						if (wTableCell.CellFormat.VerticalMerge == CellMerge.Start)
						{
							int num4 = 1;
							if (m_rowspans.ContainsKey(wTableCell))
							{
								foreach (WTableCell key2 in m_rowspans.Keys)
								{
									if (key2 == wTableCell)
									{
										m_rowspans.TryGetValue(key2, out var value);
										num4 = value;
										break;
									}
								}
							}
							int cellIndex = wTableCell.GetCellIndex();
							int num5 = wTableCell.OwnerRow.GetRowIndex() + 1;
							for (int k = 1; k < num4; k++)
							{
								if (table.Rows.Count <= num5)
								{
									break;
								}
								int num6 = 0;
								for (int l = 0; l < num; l++)
								{
									int key = m_tblGrid[l].Key;
									num6 += key;
								}
								int num7 = 0;
								int num8 = 0;
								int num9 = 0;
								bool flag = false;
								for (int m = 0; m < m_tblGridCollection[num5].Count; m++)
								{
									KeyValuePair<int, float> keyValuePair = m_tblGridCollection[num5][m];
									if (num6 < num8 + keyValuePair.Key)
									{
										flag = true;
									}
									else if (!flag)
									{
										num8 += keyValuePair.Key;
										num7 = m + 1;
									}
									num9 += keyValuePair.Key;
								}
								if (num9 < num6)
								{
									int num10 = num6 - num9;
									for (int n = 0; n < num10; n++)
									{
										table.Rows[num5].AddCell(isCopyFormat: false);
										table.Rows[num5].Cells[table.Rows[num5].Cells.Count - 1].CellFormat.Borders.BorderType = BorderStyle.Cleared;
										_ = table.Rows[num5].Cells.Count;
										m_tblGridCollection[num5].Add(new KeyValuePair<int, float>(1, 0f));
									}
									num8 = num6;
									num7 += num10;
								}
								WTableCell wTableCell2 = (WTableCell)wTableCell.Clone();
								wTableCell2.Items.Clear();
								KeyValuePair<int, float> keyValuePair2 = m_tblGrid[cellIndex];
								if (num7 < table.Rows[num5].Cells.Count)
								{
									table.Rows[num5].Cells.Insert(num7, wTableCell2);
									m_tblGridCollection[num5].Insert(num7, new KeyValuePair<int, float>(keyValuePair2.Key, keyValuePair2.Value));
								}
								else
								{
									table.Rows[num5].Cells.Add(wTableCell2);
									m_tblGridCollection[num5].Add(new KeyValuePair<int, float>(keyValuePair2.Key, keyValuePair2.Value));
								}
								wTableCell2.CellFormat.VerticalMerge = CellMerge.Continue;
								num5++;
							}
						}
					}
					num3++;
				}
				num++;
			}
		}
	}

	internal class TextFormat
	{
		internal const short FontSizeKey = 0;

		internal const short FontFamilyKey = 1;

		internal const short BoldKey = 2;

		internal const short UnderlineKey = 3;

		internal const short ItalicKey = 4;

		internal const short StrikeKey = 5;

		internal const short FontColorKey = 6;

		internal const short BackColorKey = 7;

		internal const short LineHeightKey = 8;

		internal const short LineHeightNormalKey = 9;

		internal const short TextAlignKey = 10;

		internal const short TopMarginKey = 11;

		internal const short LeftMarginKey = 12;

		internal const short BottomMarginKey = 13;

		internal const short RightMarginKey = 14;

		internal const short TextIndentKey = 15;

		internal const short SubSuperScriptKey = 16;

		internal const short PageBreakBeforeKey = 17;

		internal const short PageBreakAfterKey = 18;

		internal const short CharacterSpacingKey = 19;

		internal const short AllCapsKey = 20;

		internal const short WhiteSpaceKey = 21;

		internal const short WordWrapKey = 22;

		internal const short HiddenKey = 23;

		internal const short SmallCapsKey = 24;

		internal const short Keeplinestogetherkey = 25;

		internal const short keepwithnextKey = 26;

		internal const short AfterSpaceAutoKey = 27;

		internal const short BeforeSpaceAutoKey = 28;

		internal const short LocaleIdASCIIKey = 29;

		internal const short ScalingKey = 30;

		internal const short BidiKey = 31;

		internal Dictionary<int, object> m_propertiesHash;

		private LineSpacingRule m_lineSpacingRule;

		private float m_tabPos;

		private TabLeader m_tabLeader;

		private TabJustification m_tabJustification;

		private float m_tabWidth;

		private bool m_isInlineBlock;

		private bool m_isSpaceRun;

		private bool m_isListTab;

		private float m_listNumberWidth;

		private float m_paddingLeft;

		public bool NumBulleted;

		public bool DefBulleted;

		public BuiltinStyle Style;

		public TableBorders Borders = new TableBorders();

		public bool WordWrap
		{
			get
			{
				if (HasKey(22))
				{
					return (bool)m_propertiesHash[22];
				}
				return true;
			}
			set
			{
				SetPropertyValue(22, value);
			}
		}

		public bool IsPreserveWhiteSpace
		{
			get
			{
				if (HasKey(21))
				{
					return (bool)m_propertiesHash[21];
				}
				return false;
			}
			set
			{
				SetPropertyValue(21, value);
			}
		}

		internal short LocaleIdASCII
		{
			get
			{
				return (short)m_propertiesHash[29];
			}
			set
			{
				SetPropertyValue(29, value);
			}
		}

		internal bool Hidden
		{
			get
			{
				if (HasKey(23))
				{
					return (bool)m_propertiesHash[23];
				}
				return false;
			}
			set
			{
				SetPropertyValue(23, value);
			}
		}

		internal bool KeepLinesTogether
		{
			get
			{
				if (HasKey(25))
				{
					return (bool)m_propertiesHash[25];
				}
				return false;
			}
			set
			{
				SetPropertyValue(25, value);
			}
		}

		internal bool KeepWithNext
		{
			get
			{
				if (HasKey(26))
				{
					return (bool)m_propertiesHash[26];
				}
				return false;
			}
			set
			{
				SetPropertyValue(26, value);
			}
		}

		internal bool AfterSpaceAuto
		{
			get
			{
				if (HasKey(27))
				{
					return (bool)m_propertiesHash[27];
				}
				return false;
			}
			set
			{
				SetPropertyValue(27, value);
			}
		}

		internal bool BeforeSpaceAuto
		{
			get
			{
				if (HasKey(28))
				{
					return (bool)m_propertiesHash[28];
				}
				return false;
			}
			set
			{
				SetPropertyValue(28, value);
			}
		}

		public bool AllCaps
		{
			get
			{
				if (HasKey(20))
				{
					return (bool)m_propertiesHash[20];
				}
				return false;
			}
			set
			{
				SetPropertyValue(20, value);
			}
		}

		internal bool SmallCaps
		{
			get
			{
				if (HasKey(24))
				{
					return (bool)m_propertiesHash[24];
				}
				return false;
			}
			set
			{
				SetPropertyValue(24, value);
			}
		}

		public float CharacterSpacing
		{
			get
			{
				if (HasKey(19))
				{
					return (float)m_propertiesHash[19];
				}
				return 0f;
			}
			set
			{
				SetPropertyValue(19, value);
			}
		}

		public bool PageBreakBefore
		{
			get
			{
				if (HasKey(17))
				{
					return (bool)m_propertiesHash[17];
				}
				return false;
			}
			set
			{
				SetPropertyValue(17, value);
			}
		}

		public bool PageBreakAfter
		{
			get
			{
				if (HasKey(18))
				{
					return (bool)m_propertiesHash[18];
				}
				return false;
			}
			set
			{
				SetPropertyValue(18, value);
			}
		}

		public LineSpacingRule LineSpacingRule
		{
			get
			{
				return m_lineSpacingRule;
			}
			set
			{
				m_lineSpacingRule = value;
			}
		}

		internal float TabPosition
		{
			get
			{
				return m_tabPos;
			}
			set
			{
				m_tabPos = value;
			}
		}

		internal TabLeader TabLeader
		{
			get
			{
				return m_tabLeader;
			}
			set
			{
				m_tabLeader = value;
			}
		}

		internal TabJustification TabJustification
		{
			get
			{
				return m_tabJustification;
			}
			set
			{
				m_tabJustification = value;
			}
		}

		internal float TabWidth
		{
			get
			{
				return m_tabWidth;
			}
			set
			{
				m_tabWidth = value;
			}
		}

		internal bool IsInlineBlock
		{
			get
			{
				return m_isInlineBlock;
			}
			set
			{
				m_isInlineBlock = value;
			}
		}

		internal bool IsNonBreakingSpace
		{
			get
			{
				return m_isSpaceRun;
			}
			set
			{
				m_isSpaceRun = value;
			}
		}

		internal bool IsListTab
		{
			get
			{
				return m_isListTab;
			}
			set
			{
				m_isListTab = value;
			}
		}

		internal float ListNumberWidth
		{
			get
			{
				return m_listNumberWidth;
			}
			set
			{
				m_listNumberWidth = value;
			}
		}

		internal float PaddingLeft
		{
			get
			{
				return m_paddingLeft;
			}
			set
			{
				m_paddingLeft = value;
			}
		}

		public bool Bold
		{
			get
			{
				if (HasKey(2))
				{
					return (bool)m_propertiesHash[2];
				}
				return false;
			}
			set
			{
				SetPropertyValue(2, value);
			}
		}

		public bool Italic
		{
			get
			{
				if (HasKey(4))
				{
					return (bool)m_propertiesHash[4];
				}
				return false;
			}
			set
			{
				SetPropertyValue(4, value);
			}
		}

		public bool Underline
		{
			get
			{
				if (HasKey(3))
				{
					return (bool)m_propertiesHash[3];
				}
				return false;
			}
			set
			{
				SetPropertyValue(3, value);
			}
		}

		public bool Strike
		{
			get
			{
				if (HasKey(5))
				{
					return (bool)m_propertiesHash[5];
				}
				return false;
			}
			set
			{
				SetPropertyValue(5, value);
			}
		}

		public Color FontColor
		{
			get
			{
				if (HasKey(6))
				{
					return (Color)m_propertiesHash[6];
				}
				return Color.Empty;
			}
			set
			{
				SetPropertyValue(6, value);
			}
		}

		public Color BackColor
		{
			get
			{
				if (HasKey(7))
				{
					return (Color)m_propertiesHash[7];
				}
				return Color.Empty;
			}
			set
			{
				SetPropertyValue(7, value);
			}
		}

		public string FontFamily
		{
			get
			{
				if (HasKey(1))
				{
					return (string)m_propertiesHash[1];
				}
				return string.Empty;
			}
			set
			{
				SetPropertyValue(1, value);
			}
		}

		public float FontSize
		{
			get
			{
				if (HasKey(0))
				{
					return (float)m_propertiesHash[0];
				}
				return 12f;
			}
			set
			{
				SetPropertyValue(0, value);
			}
		}

		public float LineHeight
		{
			get
			{
				if (HasKey(8))
				{
					return (float)m_propertiesHash[8];
				}
				return -1f;
			}
			set
			{
				SetPropertyValue(8, value);
			}
		}

		public bool IsLineHeightNormal
		{
			get
			{
				if (HasKey(9))
				{
					return (bool)m_propertiesHash[9];
				}
				return false;
			}
			set
			{
				SetPropertyValue(9, value);
			}
		}

		public HorizontalAlignment TextAlign
		{
			get
			{
				if (HasKey(10))
				{
					return (HorizontalAlignment)m_propertiesHash[10];
				}
				return HorizontalAlignment.Left;
			}
			set
			{
				SetPropertyValue(10, value);
			}
		}

		public float LeftMargin
		{
			get
			{
				if (HasKey(12))
				{
					return (float)m_propertiesHash[12];
				}
				return 0f;
			}
			set
			{
				SetPropertyValue(12, value);
			}
		}

		public float TextIndent
		{
			get
			{
				if (HasKey(15))
				{
					return (float)m_propertiesHash[15];
				}
				return 0f;
			}
			set
			{
				SetPropertyValue(15, value);
			}
		}

		public float RightMargin
		{
			get
			{
				if (HasKey(14))
				{
					return (float)m_propertiesHash[14];
				}
				return 0f;
			}
			set
			{
				SetPropertyValue(14, value);
			}
		}

		public float TopMargin
		{
			get
			{
				if (HasKey(11))
				{
					return (float)m_propertiesHash[11];
				}
				return 0f;
			}
			set
			{
				SetPropertyValue(11, value);
			}
		}

		public float BottomMargin
		{
			get
			{
				if (HasKey(13))
				{
					return (float)m_propertiesHash[13];
				}
				return -1f;
			}
			set
			{
				SetPropertyValue(13, value);
			}
		}

		public SubSuperScript SubSuperScript
		{
			get
			{
				if (HasKey(16))
				{
					return (SubSuperScript)m_propertiesHash[16];
				}
				return SubSuperScript.None;
			}
			set
			{
				SetPropertyValue(16, value);
			}
		}

		internal float Scaling
		{
			get
			{
				if (HasKey(30))
				{
					return (float)m_propertiesHash[30];
				}
				return 100f;
			}
			set
			{
				SetPropertyValue(30, value);
			}
		}

		internal bool Bidi
		{
			get
			{
				if (HasKey(31))
				{
					return (bool)m_propertiesHash[31];
				}
				return false;
			}
			set
			{
				SetPropertyValue(31, value);
			}
		}

		internal TextFormat()
		{
			m_propertiesHash = new Dictionary<int, object>();
		}

		public TextFormat Clone()
		{
			return new TextFormat
			{
				m_propertiesHash = new Dictionary<int, object>(m_propertiesHash),
				NumBulleted = NumBulleted,
				DefBulleted = DefBulleted,
				Borders = Borders,
				LineSpacingRule = LineSpacingRule
			};
		}

		private void SetPropertyValue(int Key, bool value)
		{
			if (!m_propertiesHash.ContainsKey(Key))
			{
				m_propertiesHash.Add(Key, value);
			}
			else
			{
				m_propertiesHash[Key] = value;
			}
		}

		internal bool HasKey(int Key)
		{
			if (m_propertiesHash.ContainsKey(Key))
			{
				return true;
			}
			return false;
		}

		internal bool HasValue(int key)
		{
			if (m_propertiesHash.ContainsKey(key))
			{
				return true;
			}
			return false;
		}

		private void SetPropertyValue(int Key, object value)
		{
			if (!m_propertiesHash.ContainsKey(Key))
			{
				m_propertiesHash.Add(Key, value);
			}
			else
			{
				m_propertiesHash[Key] = value;
			}
		}
	}

	internal class TableBorders
	{
		public Color AllColor = Color.Empty;

		public float AllWidth = -1f;

		public BorderStyle AllStyle;

		public Color TopColor = Color.Empty;

		public Color BottomColor = Color.Empty;

		public Color LeftColor = Color.Empty;

		public Color RightColor = Color.Empty;

		public BorderStyle TopStyle;

		public BorderStyle BottomStyle;

		public BorderStyle LeftStyle;

		public BorderStyle RightStyle;

		public float TopWidth = -1f;

		public float BottomWidth = -1f;

		public float LeftWidth = -1f;

		public float RightWidth = -1f;

		public float BottomSpace;

		public float TopSpace;

		public float LeftSpace;

		public float RightSpace;
	}

	internal enum ThreeState
	{
		False,
		True,
		Unknown
	}

	private const string DEF_WHITESPACE = " ";

	private const float DEF_LH_INDENT = 35f;

	private const float DEF_MEDIUMVALUE = 3f;

	private const float DEF_THICKVALUE = 4.5f;

	private const float DEF_THINVALUE = 0.75f;

	private const float DEF_INDENT = 36f;

	private const string c_Xhtml1ScrictDocType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n";

	private const string c_Xhtml1TRansitionalDocType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n";

	private byte m_bFlags;

	private static readonly Regex m_removeSpaces = new Regex("\\s+");

	private const float c_DefCellWidth = 3f;

	private Stack<WField> m_fieldStack;

	private XmlDocument m_xmlDoc;

	private Stack<TextFormat> m_styleStack = new Stack<TextFormat>();

	private BodyItemCollection m_bodyItems;

	private WTextBody m_textBody;

	private Stack<BodyItemCollection> m_nestedBodyItems = new Stack<BodyItemCollection>();

	private Stack<WTable> m_nestedTable = new Stack<WTable>();

	private WParagraph m_currParagraph;

	private WTable m_currTable;

	private float cellSpacing;

	private string m_basePath;

	private int m_curListLevel = -1;

	private bool isPreTag;

	private List<int> m_listLevelNo = new List<int>();

	private bool checkFirstElement;

	private Stack<ListStyle> m_listStack;

	private Stack<string> m_lfoStack;

	[ThreadStatic]
	private static TextFormat s_defFormat;

	internal float childTableWidth;

	private HorizontalAlignment m_horizontalAlignmentDefinedInCellNode;

	private HorizontalAlignment m_horizontalAlignmentDefinedInRowNode;

	private bool m_bBorderStyle;

	private int m_currTableFooterRowIndex = -1;

	private TextFormat currDivFormat;

	private bool m_bIsInDiv;

	private Stack<bool> m_stackTableStyle = new Stack<bool>();

	private Stack<bool> m_stackRowStyle = new Stack<bool>();

	private Stack<bool> m_stackCellStyle = new Stack<bool>();

	public IWParagraphStyle m_userStyle;

	private TableGrid tableGrid;

	private bool m_bIsInBlockquote;

	private int m_blockquoteLevel;

	private ListStyle m_userListStyle;

	private int m_divCount;

	private bool m_bIsAlignAttrDefinedInRowNode;

	private bool m_bIsAlignAttriDefinedInCellNode;

	private bool m_bIsVAlignAttriDefinedInRowNode;

	private VerticalAlignment m_verticalAlignmentDefinedInRowNode = VerticalAlignment.Middle;

	private bool m_bIsBorderCollapse;

	private Stack<float> m_listLeftIndentStack = new Stack<float>();

	private bool m_bIsWithinList;

	private Color m_hyperlinkcolor = Color.Empty;

	internal WSection m_currentSection;

	internal HTMLImportSettings HtmlImportSettings;

	private CSSStyle m_CSSStyle;

	private bool isPreserveBreakForInvalidStyles;

	private bool isLastLevelSkipped;

	private int lastSkippedLevelNo = -1;

	private int lastUsedLevelNo = -1;

	private int listCount;

	private bool IsPreviousItemFieldStart
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

	private bool IsStyleFieldCode
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	private bool IsTableStyle
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	private bool IsRowStyle
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	private bool IsCellStyle
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal CSSStyle CSSStyle
	{
		get
		{
			if (m_CSSStyle == null)
			{
				m_CSSStyle = new CSSStyle();
			}
			return m_CSSStyle;
		}
	}

	internal float ClientWidth
	{
		get
		{
			float result = m_textBody.Document.LastSection.PageSetup.ClientWidth;
			if (m_textBody is WTableCell)
			{
				result = (m_textBody as WTableCell).Width;
			}
			else if (m_currentSection != null)
			{
				result = m_currentSection.PageSetup.ClientWidth;
			}
			return result;
		}
	}

	protected string BasePath
	{
		get
		{
			return m_basePath;
		}
		set
		{
			m_basePath = value;
		}
	}

	private Stack<WField> FieldStack
	{
		get
		{
			if (m_fieldStack == null)
			{
				m_fieldStack = new Stack<WField>();
			}
			return m_fieldStack;
		}
	}

	private WField CurrentField
	{
		get
		{
			if (m_fieldStack == null || m_fieldStack.Count <= 0)
			{
				return null;
			}
			return m_fieldStack.Peek();
		}
	}

	protected TextFormat CurrentFormat
	{
		get
		{
			if (m_styleStack.Count > 0)
			{
				return m_styleStack.Peek();
			}
			if (s_defFormat == null)
			{
				s_defFormat = new TextFormat();
			}
			return s_defFormat;
		}
	}

	protected WParagraph CurrentPara
	{
		get
		{
			if (m_currParagraph == null)
			{
				m_currParagraph = new WParagraph(m_bodyItems.Document);
				m_bodyItems.Add(m_currParagraph);
				if (m_userStyle != null)
				{
					m_currParagraph.ApplyStyle(m_userStyle, isDomChanges: false);
				}
			}
			return m_currParagraph;
		}
	}

	private Stack<string> LfoStack
	{
		get
		{
			if (m_lfoStack == null)
			{
				m_lfoStack = new Stack<string>();
			}
			return m_lfoStack;
		}
	}

	private Stack<ListStyle> ListStack
	{
		get
		{
			if (m_listStack == null)
			{
				m_listStack = new Stack<ListStyle>();
			}
			return m_listStack;
		}
	}

	private ListStyle CurrentListStyle
	{
		get
		{
			if (m_listStack == null || m_listStack.Count == 0)
			{
				return null;
			}
			return m_listStack.Peek();
		}
	}

	public void AppendToTextBody(ITextBody textBody, string html, int paragraphIndex, int paragraphItemIndex, IWParagraphStyle style, ListStyle listStyle)
	{
		if (style != null)
		{
			m_userStyle = style;
		}
		if (listStyle != null)
		{
			m_userListStyle = listStyle;
		}
		AppendToTextBody(textBody, html, paragraphIndex, paragraphItemIndex);
		m_userStyle = null;
		m_userListStyle = null;
	}

	public void AppendToTextBody(ITextBody textBody, string html, int paragraphIndex, int paragraphItemIndex)
	{
		textBody.Document.IsOpening = true;
		Init();
		m_basePath = textBody.Document.HtmlBaseUrl;
		m_textBody = textBody as WTextBody;
		m_currentSection = m_textBody.GetOwnerSection(m_textBody) as WSection;
		TextBodyPart textBodyPart = new TextBodyPart(textBody.Document);
		m_bodyItems = textBodyPart.BodyItems;
		ApplyBidiToParaItems();
		m_currParagraph = null;
		tableGrid = new TableGrid();
		LoadXhtml(html);
		XmlNode xmlNode = m_xmlDoc.DocumentElement;
		_ = m_xmlDoc.DocumentElement;
		if (xmlNode.LocalName.Equals("html", StringComparison.OrdinalIgnoreCase))
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				if (childNode.LocalName.Equals("head", StringComparison.OrdinalIgnoreCase) && childNode.NodeType == XmlNodeType.Element)
				{
					foreach (XmlNode childNode2 in childNode.ChildNodes)
					{
						if (childNode2.LocalName.Equals("title", StringComparison.OrdinalIgnoreCase) && childNode2.NodeType == XmlNodeType.Element && m_bodyItems.Document.IsHTMLImport)
						{
							m_bodyItems.Document.BuiltinDocumentProperties.Title = childNode2.InnerText;
						}
						if (childNode2.LocalName.Equals("base", StringComparison.OrdinalIgnoreCase) && childNode2.NodeType == XmlNodeType.Element)
						{
							BasePath = GetAttributeValue(childNode2, "href");
						}
						else if (childNode2.LocalName.Equals("style", StringComparison.OrdinalIgnoreCase) && childNode2.NodeType == XmlNodeType.Element)
						{
							ParseCssStyle(childNode2);
						}
					}
				}
				if (childNode.LocalName.Equals("body", StringComparison.OrdinalIgnoreCase))
				{
					ParseBodyAttributes(childNode);
					xmlNode = childNode;
					break;
				}
			}
		}
		bool stylePresent = ParseBodyStyle(xmlNode, textBody, paragraphIndex);
		TraverseChildNodes(xmlNode.ChildNodes);
		LeaveStyle(stylePresent);
		if (m_currParagraph != null)
		{
			ApplyTextFormatting(m_currParagraph.BreakCharacterFormat);
		}
		RemoveLastLineBreakFromParagraph(textBodyPart.BodyItems);
		textBody.Document.IsOpening = false;
		textBody.Document.IsHTMLImport = true;
		textBodyPart.PasteAt(textBody, paragraphIndex, paragraphItemIndex);
		textBody.Document.IsHTMLImport = false;
		if (textBodyPart.BodyItems.Count > 0 && textBodyPart.BodyItems[0].EntityType == EntityType.Paragraph)
		{
			WParagraph wParagraph = textBodyPart.BodyItems[0] as WParagraph;
			WParagraphFormat paragraphFormat = wParagraph.ParagraphFormat;
			(textBody.ChildEntities[paragraphIndex] as WParagraph).ParagraphFormat.ImportContainer(paragraphFormat);
			(textBody.ChildEntities[paragraphIndex] as WParagraph).ParagraphFormat.CopyProperties(paragraphFormat);
			if (!string.IsNullOrEmpty(wParagraph.StyleName))
			{
				(textBody.ChildEntities[paragraphIndex] as WParagraph).ApplyStyle(wParagraph.StyleName);
			}
			(textBody.ChildEntities[paragraphIndex] as WParagraph).BreakCharacterFormat.ImportContainer(wParagraph.BreakCharacterFormat);
			(textBody.ChildEntities[paragraphIndex] as WParagraph).IsCreatedUsingHtmlSpanTag = wParagraph.IsCreatedUsingHtmlSpanTag;
		}
		if (textBodyPart.BodyItems.Count > 1 && textBodyPart.BodyItems[textBodyPart.BodyItems.Count - 1].EntityType == EntityType.Paragraph)
		{
			WParagraph wParagraph2 = textBodyPart.BodyItems[textBodyPart.BodyItems.Count - 1] as WParagraph;
			int num = paragraphIndex + textBodyPart.BodyItems.Count - 1;
			if (textBodyPart.BodyItems.Count > 0 && textBodyPart.BodyItems[0].EntityType == EntityType.Table)
			{
				num++;
			}
			if (textBody.ChildEntities[num].EntityType == EntityType.Paragraph && wParagraph2.StyleName != string.Empty)
			{
				(textBody.ChildEntities[num] as WParagraph).ApplyStyle(wParagraph2.StyleName);
			}
		}
		SetNextStyleForParagraphStyle(m_bodyItems.Document);
		CSSStyle.Close();
	}

	private bool ParseBodyStyle(XmlNode node, ITextBody textBody, int paragraphIndex)
	{
		string attributeValue = GetAttributeValue(node, "style");
		_ = new string[9] { "dashed", "dotted", "double", "groove", "inset", "outset", "ridge", "solid", "hidden" };
		if (attributeValue.Length != 0)
		{
			TextFormat textFormat = AddStyle();
			textFormat.Borders = new TableBorders();
			string[] array = attributeValue.Split(';', ':');
			int i = 0;
			for (int num = array.Length; i < num - 1; i += 2)
			{
				char[] trimChars = new char[2] { '\'', '"' };
				string paramName = array[i].ToLower().Trim();
				string text = array[i + 1].Trim();
				text = text.Trim(trimChars);
				GetFormat(textFormat, paramName, text, node);
			}
			if ((node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase)) && textFormat.ListNumberWidth != 0f && textFormat.LeftMargin != 0f && textFormat.PaddingLeft != 0f)
			{
				textFormat.TextIndent = 0f - (textFormat.PaddingLeft + textFormat.ListNumberWidth);
				textFormat.LeftMargin = textFormat.LeftMargin - textFormat.TextIndent - textFormat.ListNumberWidth;
			}
			if (textBody.ChildEntities.Count == 1 && textBody.ChildEntities[0] is WParagraph && paragraphIndex == 0)
			{
				ApplyPageBorder(textFormat);
				ApplyPageFormat(textFormat);
			}
			if (textFormat.HasKey(7))
			{
				textFormat.BackColor = Color.Empty;
			}
			if (textFormat.HasKey(13))
			{
				textFormat.BottomMargin = 0f;
			}
			if (textFormat.HasKey(11))
			{
				textFormat.TopMargin = 0f;
			}
			if (textFormat.HasKey(12))
			{
				textFormat.LeftMargin = 0f;
			}
			if (textFormat.HasKey(14))
			{
				textFormat.RightMargin = 0f;
			}
			textFormat.Borders = new TableBorders();
			return true;
		}
		return false;
	}

	private void ApplyPageFormat(TextFormat format)
	{
		m_currentSection.PageSetup.Margins.Left += format.LeftMargin + (m_currentSection.Document.DOP.GutterAtTop ? 0f : m_currentSection.PageSetup.Margins.Gutter);
		m_currentSection.PageSetup.Margins.Right += format.RightMargin;
		m_currentSection.PageSetup.Margins.Top += format.TopMargin + (m_currentSection.Document.DOP.GutterAtTop ? m_currentSection.PageSetup.Margins.Gutter : 0f);
		m_currentSection.PageSetup.Margins.Bottom += format.BottomMargin;
		if (format.HasKey(7))
		{
			m_bodyItems.Document.Background.Type = BackgroundType.Color;
			m_bodyItems.Document.Background.SetBackgroundColor(format.BackColor);
		}
	}

	private void ApplyPageBorder(TextFormat format)
	{
		if (format.Borders.AllStyle != 0)
		{
			Border bottom = m_currentSection.PageSetup.Borders.Bottom;
			Border top = m_currentSection.PageSetup.Borders.Top;
			Border left = m_currentSection.PageSetup.Borders.Left;
			BorderStyle borderStyle = (m_currentSection.PageSetup.Borders.Right.BorderType = format.Borders.AllStyle);
			BorderStyle borderStyle3 = (left.BorderType = borderStyle);
			BorderStyle borderType = (top.BorderType = borderStyle3);
			bottom.BorderType = borderType;
			if (format.Borders.AllColor != Color.Empty)
			{
				Border bottom2 = m_currentSection.PageSetup.Borders.Bottom;
				Border top2 = m_currentSection.PageSetup.Borders.Top;
				Border left2 = m_currentSection.PageSetup.Borders.Left;
				Color color = (m_currentSection.PageSetup.Borders.Right.Color = format.Borders.AllColor);
				Color color3 = (left2.Color = color);
				Color color5 = (top2.Color = color3);
				bottom2.Color = color5;
			}
			if (format.Borders.AllWidth != -1f)
			{
				Border bottom3 = m_currentSection.PageSetup.Borders.Bottom;
				Border top3 = m_currentSection.PageSetup.Borders.Top;
				Border left3 = m_currentSection.PageSetup.Borders.Left;
				float num = (m_currentSection.PageSetup.Borders.Right.LineWidth = format.Borders.AllWidth);
				float num3 = (left3.LineWidth = num);
				float lineWidth = (top3.LineWidth = num3);
				bottom3.LineWidth = lineWidth;
			}
		}
		if (format.Borders.BottomStyle != 0)
		{
			m_currentSection.PageSetup.Borders.Bottom.BorderType = format.Borders.BottomStyle;
			m_currentSection.PageSetup.Borders.Bottom.LineWidth = format.Borders.BottomWidth;
			m_currentSection.PageSetup.Borders.Bottom.Color = format.Borders.BottomColor;
		}
		if (format.Borders.TopStyle != 0)
		{
			m_currentSection.PageSetup.Borders.Top.BorderType = format.Borders.TopStyle;
			m_currentSection.PageSetup.Borders.Top.LineWidth = format.Borders.TopWidth;
			m_currentSection.PageSetup.Borders.Top.Color = format.Borders.TopColor;
		}
		if (format.Borders.LeftStyle != 0)
		{
			m_currentSection.PageSetup.Borders.Left.BorderType = format.Borders.LeftStyle;
			m_currentSection.PageSetup.Borders.Left.LineWidth = format.Borders.LeftWidth;
			m_currentSection.PageSetup.Borders.Left.Color = format.Borders.LeftColor;
		}
		if (format.Borders.RightStyle != 0)
		{
			m_currentSection.PageSetup.Borders.Right.BorderType = format.Borders.RightStyle;
			m_currentSection.PageSetup.Borders.Right.LineWidth = format.Borders.RightWidth;
			m_currentSection.PageSetup.Borders.Right.Color = format.Borders.RightColor;
		}
		if (format.Borders.TopWidth > 0f)
		{
			m_currentSection.PageSetup.Borders.Top.LineWidth = format.Borders.TopWidth;
		}
		if (format.Borders.RightWidth > 0f)
		{
			m_currentSection.PageSetup.Borders.Right.LineWidth = format.Borders.RightWidth;
		}
		if (format.Borders.LeftWidth > 0f)
		{
			m_currentSection.PageSetup.Borders.Left.LineWidth = format.Borders.LeftWidth;
		}
		if (format.Borders.BottomWidth > 0f)
		{
			m_currentSection.PageSetup.Borders.Bottom.LineWidth = format.Borders.BottomWidth;
		}
	}

	private void SetNextStyleForParagraphStyle(WordDocument document)
	{
		foreach (Style style in document.Styles)
		{
			if (style.StyleType == StyleType.ParagraphStyle)
			{
				style.NextStyle = style.Name.Replace(" ", string.Empty);
			}
		}
	}

	private void ParseBodyAttributes(XmlNode node)
	{
		string empty = string.Empty;
		foreach (XmlAttribute attribute in node.Attributes)
		{
			string text = attribute.Name.ToLower();
			if (!(text == "link"))
			{
				if (!(text == "vlink"))
				{
					continue;
				}
				empty = GetAttributeValue(node, "vlink");
				if (empty != string.Empty)
				{
					IStyle style = m_bodyItems.Document.Styles.FindByName(BuiltinStyle.FollowedHyperlink.ToString());
					if (style == null)
					{
						style = Style.CreateBuiltinStyle(BuiltinStyle.FollowedHyperlink, StyleType.CharacterStyle, m_bodyItems.Document);
						m_bodyItems.Document.Styles.Add(style);
					}
					(style as Style).CharacterFormat.TextColor = GetColor(empty);
				}
				continue;
			}
			empty = GetAttributeValue(node, "link");
			if (empty != string.Empty)
			{
				IStyle style2 = m_bodyItems.Document.Styles.FindByName(BuiltinStyle.Hyperlink.ToString());
				if (style2 == null)
				{
					style2 = Style.CreateBuiltinStyle(BuiltinStyle.Hyperlink, StyleType.CharacterStyle, m_bodyItems.Document);
					m_bodyItems.Document.Styles.Add(style2);
				}
				(style2 as Style).CharacterFormat.TextColor = GetColor(empty);
				m_hyperlinkcolor = GetColor(empty);
			}
		}
	}

	private void RemoveLastLineBreakFromParagraph(BodyItemCollection itemCollection)
	{
		foreach (TextBodyItem item in itemCollection)
		{
			if (item.EntityType == EntityType.Paragraph)
			{
				WParagraph wParagraph = item as WParagraph;
				if (wParagraph.Items.Count > 0 && wParagraph.Items[wParagraph.Items.Count - 1].EntityType == EntityType.Break && ((wParagraph.Items[wParagraph.Items.Count - 1] as Break).BreakType == BreakType.LineBreak || (wParagraph.Items[wParagraph.Items.Count - 1] as Break).BreakType == BreakType.TextWrappingBreak) && (wParagraph.Items[wParagraph.Items.Count - 1] as Break).HtmlToDocLayoutInfo.RemoveLineBreak)
				{
					wParagraph.Items.RemoveAt(wParagraph.Items.Count - 1);
				}
				if (wParagraph.Items.Count > 0 && wParagraph.Items[wParagraph.Items.Count - 1].EntityType == EntityType.Break && ((wParagraph.Items[wParagraph.Items.Count - 1] as Break).BreakType == BreakType.LineBreak || (wParagraph.Items[wParagraph.Items.Count - 1] as Break).BreakType == BreakType.TextWrappingBreak) && (wParagraph.Items[wParagraph.Items.Count - 1] as Break).HtmlToDocLayoutInfo.RemoveLineBreak)
				{
					wParagraph.Items.RemoveAt(wParagraph.Items.Count - 1);
				}
			}
			else
			{
				if (item.EntityType != EntityType.Table)
				{
					continue;
				}
				foreach (WTableRow row in (item as WTable).Rows)
				{
					foreach (WTableCell cell in row.Cells)
					{
						RemoveLastLineBreakFromParagraph(cell.Items);
					}
				}
			}
		}
	}

	public bool IsValid(string html, XHTMLValidationType type)
	{
		string exceptionMessage;
		return IsValid(html, type, out exceptionMessage);
	}

	public bool IsValid(string html, XHTMLValidationType type, out string exceptionMessage)
	{
		exceptionMessage = string.Empty;
		Assembly.GetExecutingAssembly();
		XmlSchema xmlSchema = null;
		html = ReplaceHtmlConstantByUnicodeChar(html);
		switch (type)
		{
		case XHTMLValidationType.Strict:
			xmlSchema = XmlSchema.Read(GetManifestResourceStream("xhtml1-strict.xsd"), OnValidation);
			break;
		case XHTMLValidationType.Transitional:
			xmlSchema = XmlSchema.Read(GetManifestResourceStream("xhtml1-transitional.xsd"), OnValidation);
			break;
		}
		m_xmlDoc = new XmlDocument();
		m_xmlDoc.PreserveWhitespace = true;
		html = PrepareHtml(html, xmlSchema);
		try
		{
			if (type == XHTMLValidationType.None)
			{
				m_xmlDoc.LoadXml(html);
				return true;
			}
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.ValidationType = ValidationType.Schema;
			if (xmlSchema != null)
			{
				xmlReaderSettings.Schemas.Add(xmlSchema);
			}
			XmlReader xmlReader = XmlReader.Create(new StringReader(html), xmlReaderSettings);
			m_xmlDoc.Load(xmlReader);
			xmlReader.Close();
		}
		catch (Exception ex)
		{
			exceptionMessage = ex.Message;
			return false;
		}
		return true;
	}

	private string ReplaceHtmlConstantByUnicodeChar(string html)
	{
		html = ReplaceHtmlSpecialCharacters(html);
		html = ReplaceHtmlSymbols(html);
		html = ReplaceHtmlCharacters(html);
		html = ReplaceHtmlMathSymbols(html);
		html = ReplaceHtmlGreekLetters(html);
		html = ReplaceHtmlOtherEntities(html);
		html = ReplaceAmpersand(html);
		return html;
	}

	private string ReplaceHtmlSpecialCharacters(string html)
	{
		html = html.Replace("&quot;", "&#34;");
		html = html.Replace("&apos;", "&#39;");
		html = html.Replace("&amp;", "&#38;");
		html = html.Replace("&lt;", "&#60;");
		html = html.Replace("&gt;", "&#62;");
		html = html.Replace('Â'.ToString(), "&#194;");
		html = html.Replace('«'.ToString(), "&#171;");
		html = html.Replace('»'.ToString(), "&#187;");
		return html;
	}

	private string ReplaceHtmlSymbols(string html)
	{
		html = html.Replace("&nbsp;", "&#160;");
		html = html.Replace("&iexcl;", "&#161;");
		html = html.Replace("&cent;", "&#162;");
		html = html.Replace("&pound;", "&#163;");
		html = html.Replace("&curren;", "&#164;");
		html = html.Replace("&yen;", "&#165;");
		html = html.Replace("&brvbar;", "&#166;");
		html = html.Replace("&sect;", "&#167;");
		html = html.Replace("&uml;", "&#168;");
		html = html.Replace("&copy;", "&#169;");
		html = html.Replace("&ordf;", "&#170;");
		html = html.Replace("&laquo;", "&#171;");
		html = html.Replace("&not;", "&#172;");
		html = html.Replace("&shy;", "&#173;");
		html = html.Replace("&reg;", "&#174;");
		html = html.Replace("&macr;", "&#175;");
		html = html.Replace("&deg;", "&#176;");
		html = html.Replace("&plusmn;", "&#177;");
		html = html.Replace("&sup2;", "&#178;");
		html = html.Replace("&sup3;", "&#179;");
		html = html.Replace("&acute;", "&#180;");
		html = html.Replace("&micro;", "&#181;");
		html = html.Replace("&para;", "&#182;");
		html = html.Replace("&middot;", "&#183;");
		html = html.Replace("&cedil;", "&#184;");
		html = html.Replace("&sup1;", "&#185;");
		html = html.Replace("&ordm;", "&#186;");
		html = html.Replace("&raquo;", "&#187;");
		html = html.Replace("&frac14;", "&#188;");
		html = html.Replace("&frac12;", "&#189;");
		html = html.Replace("&frac34;", "&#190;");
		html = html.Replace("&iquest;", "&#191;");
		html = html.Replace("&times;", "&#215;");
		html = html.Replace("&divide;", "&#247;");
		return html;
	}

	private string ReplaceHtmlCharacters(string html)
	{
		html = html.Replace("&Agrave;", "&#192;");
		html = html.Replace("&Aacute;", "&#193;");
		html = html.Replace("&Acirc;", "&#194;");
		html = html.Replace("&Atilde;", "&#195;");
		html = html.Replace("&Auml;", "&#196;");
		html = html.Replace("&Aring;", "&#197;");
		html = html.Replace("&AElig;", "&#198;");
		html = html.Replace("&Ccedil;", "&#199;");
		html = html.Replace("&Egrave;", "&#200;");
		html = html.Replace("&Eacute;", "&#201;");
		html = html.Replace("&Ecirc;", "&#202;");
		html = html.Replace("&Euml;", "&#203;");
		html = html.Replace("&Igrave;", "&#204;");
		html = html.Replace("&Iacute;", "&#205;");
		html = html.Replace("&Icirc;", "&#206;");
		html = html.Replace("&Iuml;", "&#207;");
		html = html.Replace("&ETH;", "&#208;");
		html = html.Replace("&Ntilde;", "&#209;");
		html = html.Replace("&Ograve;", "&#210;");
		html = html.Replace("&Oacute;", "&#211;");
		html = html.Replace("&Ocirc;", "&#212;");
		html = html.Replace("&Otilde;", "&#213;");
		html = html.Replace("&Ouml;", "&#214;");
		html = html.Replace("&Oslash;", "&#216;");
		html = html.Replace("&Ugrave;", "&#217;");
		html = html.Replace("&Uacute;", "&#218;");
		html = html.Replace("&Ucirc;", "&#219;");
		html = html.Replace("&Uuml;", "&#220;");
		html = html.Replace("&Yacute;", "&#221;");
		html = html.Replace("&THORN;", "&#222;");
		html = html.Replace("&szlig;", "&#223;");
		html = html.Replace("&agrave;", "&#224;");
		html = html.Replace("&aacute;", "&#225;");
		html = html.Replace("&acirc;", "&#226;");
		html = html.Replace("&atilde;", "&#227;");
		html = html.Replace("&auml;", "&#228;");
		html = html.Replace("&aring;", "&#229;");
		html = html.Replace("&aelig;", "&#230;");
		html = html.Replace("&ccedil;", "&#231;");
		html = html.Replace("&egrave;", "&#232;");
		html = html.Replace("&eacute;", "&#233;");
		html = html.Replace("&ecirc;", "&#234;");
		html = html.Replace("&euml;", "&#235;");
		html = html.Replace("&igrave;", "&#236;");
		html = html.Replace("&iacute;", "&#237;");
		html = html.Replace("&icirc;", "&#238;");
		html = html.Replace("&iuml;", "&#239;");
		html = html.Replace("&eth;", "&#240;");
		html = html.Replace("&ntilde;", "&#241;");
		html = html.Replace("&ograve;", "&#242;");
		html = html.Replace("&oacute;", "&#243;");
		html = html.Replace("&ocirc;", "&#244;");
		html = html.Replace("&otilde;", "&#245;");
		html = html.Replace("&ouml;", "&#246;");
		html = html.Replace("&oslash;", "&#248;");
		html = html.Replace("&ugrave;", "&#249;");
		html = html.Replace("&uacute;", "&#250;");
		html = html.Replace("&ucirc;", "&#251;");
		html = html.Replace("&uuml;", "&#252;");
		html = html.Replace("&yacute;", "&#253;");
		html = html.Replace("&thorn;", "&#254;");
		html = html.Replace("&yuml;", "&#255;");
		return html;
	}

	private string ReplaceHtmlMathSymbols(string html)
	{
		html = html.Replace("&forall;", "&#8704;");
		html = html.Replace("&part;", "&#8706;");
		html = html.Replace("&exist;", "&#8707;");
		html = html.Replace("&empty;", "&#8709;");
		html = html.Replace("&nabla;", "&#8711;");
		html = html.Replace("&isin;", "&#8712;");
		html = html.Replace("&notin;", "&#8713;");
		html = html.Replace("&ni;", "&#8715;");
		html = html.Replace("&prod;", "&#8719;");
		html = html.Replace("&sum;", "&#8721;");
		html = html.Replace("&minus;", "&#8722;");
		html = html.Replace("&lowast;", "&#8727;");
		html = html.Replace("&radic;", "&#8730;");
		html = html.Replace("&prop;", "&#8733;");
		html = html.Replace("&infin;", "&#8734;");
		html = html.Replace("&ang;", "&#8736;");
		html = html.Replace("&and;", "&#8743;");
		html = html.Replace("&or;", "&#8744;");
		html = html.Replace("&cap;", "&#8745;");
		html = html.Replace("&cup;", "&#8746;");
		html = html.Replace("&int;", "&#8747;");
		html = html.Replace("&there4;", "&#8756;");
		html = html.Replace("&sim;", "&#8764;");
		html = html.Replace("&cong;", "&#8773;");
		html = html.Replace("&asymp;", "&#8776;");
		html = html.Replace("&ne;", "&#8800;");
		html = html.Replace("&equiv;", "&#8801;");
		html = html.Replace("&le;", "&#8804;");
		html = html.Replace("&ge;", "&#8805;");
		html = html.Replace("&sub;", "&#8834;");
		html = html.Replace("&sup;", "&#8835;");
		html = html.Replace("&nsub;", "&#8836;");
		html = html.Replace("&sube;", "&#8838;");
		html = html.Replace("&supe;", "&#8839;");
		html = html.Replace("&oplus;", "&#8853;");
		html = html.Replace("&otimes;", "&#8855;");
		html = html.Replace("&perp;", "&#8869;");
		html = html.Replace("&sdot;", "&#8901;");
		html = html.Replace("&frasl;", "&#8260;");
		return html;
	}

	private string ReplaceHtmlGreekLetters(string html)
	{
		html = html.Replace("&Alpha;", "&#913;");
		html = html.Replace("&Beta;", "&#914;");
		html = html.Replace("&Gamma;", "&#915;");
		html = html.Replace("&Delta;", "&#916;");
		html = html.Replace("&Epsilon;", "&#917;");
		html = html.Replace("&Zeta;", "&#918;");
		html = html.Replace("&Eta;", "&#919;");
		html = html.Replace("&Theta;", "&#920;");
		html = html.Replace("&Iota;", "&#921;");
		html = html.Replace("&Kappa;", "&#922;");
		html = html.Replace("&Lambda;", "&#923;");
		html = html.Replace("&Mu;", "&#924;");
		html = html.Replace("&Nu;", "&#925;");
		html = html.Replace("&Xi;", "&#926;");
		html = html.Replace("&Omicron;", "&#927;");
		html = html.Replace("&Pi;", "&#928;");
		html = html.Replace("&Rho;", "&#929;");
		html = html.Replace("&Sigma;", "&#931;");
		html = html.Replace("&Tau;", "&#932;");
		html = html.Replace("&Upsilon;", "&#933;");
		html = html.Replace("&Phi;", "&#934;");
		html = html.Replace("&Chi;", "&#935;");
		html = html.Replace("&Psi;", "&#936;");
		html = html.Replace("&Omega;", "&#937;");
		html = html.Replace("&alpha;", "&#945;");
		html = html.Replace("&beta;", "&#946;");
		html = html.Replace("&gamma;", "&#947;");
		html = html.Replace("&delta;", "&#948;");
		html = html.Replace("&epsilon;", "&#949;");
		html = html.Replace("&zeta;", "&#950;");
		html = html.Replace("&eta;", "&#951;");
		html = html.Replace("&theta;", "&#952;");
		html = html.Replace("&iota;", "&#953;");
		html = html.Replace("&kappa;", "&#954;");
		html = html.Replace("&lambda;", "&#955;");
		html = html.Replace("&mu;", "&#956;");
		html = html.Replace("&nu;", "&#957;");
		html = html.Replace("&xi;", "&#958;");
		html = html.Replace("&omicron;", "&#959;");
		html = html.Replace("&pi;", "&#960;");
		html = html.Replace("&rho;", "&#961;");
		html = html.Replace("&sigmaf;", "&#962;");
		html = html.Replace("&sigma;", "&#963;");
		html = html.Replace("&tau;", "&#964;");
		html = html.Replace("&upsilon;", "&#965;");
		html = html.Replace("&phi;", "&#966;");
		html = html.Replace("&chi;", "&#967;");
		html = html.Replace("&psi;", "&#968;");
		html = html.Replace("&omega;", "&#969;");
		html = html.Replace("&thetasym;", "&#977;");
		html = html.Replace("&upsih;", "&#978;");
		html = html.Replace("&piv;", "&#982;");
		return html;
	}

	private string ReplaceHtmlOtherEntities(string html)
	{
		html = html.Replace("&OElig;", "&#338;");
		html = html.Replace("&oelig;", "&#339;");
		html = html.Replace("&Scaron;", "&#352;");
		html = html.Replace("&scaron;", "&#353;");
		html = html.Replace("&Yuml;", "&#376;");
		html = html.Replace("&fnof;", "&#402;");
		html = html.Replace("&circ;", "&#710;");
		html = html.Replace("&tilde;", "&#732;");
		html = html.Replace("&ensp;", "&#8194;");
		html = html.Replace("&emsp;", "&#8195;");
		html = html.Replace("&thinsp;", "&#8201;");
		html = html.Replace("&zwnj;", "&#8204;");
		html = html.Replace("&zwj;", "&#8205;");
		html = html.Replace("&lrm;", "&#8206;");
		html = html.Replace("&rlm;", "&#8207;");
		html = html.Replace("&ndash;", "&#8211;");
		html = html.Replace("&mdash;", "&#8212;");
		html = html.Replace("&lsquo;", "&#8216;");
		html = html.Replace("&rsquo;", "&#8217;");
		html = html.Replace("&sbquo;", "&#8218;");
		html = html.Replace("&ldquo;", "&#8220;");
		html = html.Replace("&rdquo;", "&#8221;");
		html = html.Replace("&bdquo;", "&#8222;");
		html = html.Replace("&dagger;", "&#8224;");
		html = html.Replace("&Dagger;", "&#8225;");
		html = html.Replace("&bull;", "&#8226;");
		html = html.Replace("&hellip;", "&#8230;");
		html = html.Replace("&permil;", "&#8240;");
		html = html.Replace("&prime;", "&#8242;");
		html = html.Replace("&Prime;", "&#8243;");
		html = html.Replace("&lsaquo;", "&#8249;");
		html = html.Replace("&rsaquo;", "&#8250;");
		html = html.Replace("&oline;", "&#8254;");
		html = html.Replace("&euro;", "&#8364;");
		html = html.Replace("&trade;", "&#8482;");
		html = html.Replace("&larr;", "&#8592;");
		html = html.Replace("&uarr;", "&#8593;");
		html = html.Replace("&rarr;", "&#8594;");
		html = html.Replace("&darr;", "&#8595;");
		html = html.Replace("&harr;", "&#8596;");
		html = html.Replace("&crarr;", "&#8629;");
		html = html.Replace("&lArr;", "&#8656;");
		html = html.Replace("&uArr;", "&#8657;");
		html = html.Replace("&rArr;", "&#8658;");
		html = html.Replace("&dArr;", "&#8659;");
		html = html.Replace("&hArr;", "&#8660;");
		html = html.Replace("&lceil;", "&#8968;");
		html = html.Replace("&rceil;", "&#8969;");
		html = html.Replace("&lfloor;", "&#8970;");
		html = html.Replace("&rfloor;", "&#8971;");
		html = html.Replace("&loz;", "&#9674;");
		html = html.Replace("&spades;", "&#9824;");
		html = html.Replace("&clubs;", "&#9827;");
		html = html.Replace("&hearts;", "&#9829;");
		html = html.Replace("&diams;", "&#9830;");
		html = html.Replace("&lang;", "&#9001;");
		html = html.Replace("&rang;", "&#9002;");
		return html;
	}

	private string ReplaceAmpersand(string html)
	{
		List<int> positions = GetPositions(html, "&");
		int num = 0;
		for (int i = 0; i < positions.Count; i++)
		{
			int num2 = positions[i] + num;
			if (html.Length > num2 + 1 && html[num2 + 1] != '#')
			{
				html = html.Remove(num2, 1);
				html = html.Insert(num2, "&#38;");
				num += 4;
			}
		}
		return html;
	}

	private List<int> GetPositions(string source, string searchString)
	{
		List<int> list = new List<int>();
		_ = searchString.Length;
		int num = -1;
		while (true)
		{
			num = source.IndexOf(searchString, num + 1, StringComparison.Ordinal);
			if (num == -1)
			{
				break;
			}
			list.Add(num);
		}
		return list;
	}

	private void LoadXhtml(string html)
	{
		XmlSchema schema = null;
		html = html.Replace("&nbsp;", "nbsp;");
		html = html.Replace("&#160;", "nbsp;");
		try
		{
			m_xmlDoc = new XmlDocument();
			m_xmlDoc.PreserveWhitespace = true;
			LoadXhtml(html, schema);
		}
		catch (XmlException ex)
		{
			throw new NotSupportedException("DocIO support only welformatted xhtml \nDetails:\n" + ex.Message, ex);
		}
	}

	private void LoadXhtml(string html, XmlSchema schema)
	{
		html = PrepareHtml(html, schema);
		if (schema != null)
		{
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.ValidationType = ValidationType.Schema;
			xmlReaderSettings.Schemas.Add(schema);
			xmlReaderSettings.ValidationEventHandler += readerSettings_ValidationEventHandler;
			XmlReader xmlReader = XmlReader.Create(new StringReader(html), xmlReaderSettings);
			m_xmlDoc.Load(xmlReader);
			xmlReader.Close();
		}
		else
		{
			m_xmlDoc.LoadXml(html);
		}
	}

	private void readerSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
	{
		throw new NotSupportedException("DocIO support only welformatted xhtml \nDetails:\n" + e.Exception.Message, e.Exception);
	}

	private string PrepareHtml(string html, XmlSchema schema)
	{
		html = ReplaceHtmlConstantByUnicodeChar(html);
		html = RemoveXmlAndDocTypeElement(html);
		html = InsertHtmlElement(html, schema);
		return html;
	}

	private string RemoveXmlAndDocTypeElement(string html)
	{
		int num = 0;
		while (true)
		{
			html = html.TrimStart();
			if (html.StartsWith("<?xml version", StringComparison.OrdinalIgnoreCase) || html.StartsWith("<?xmlversion", StringComparison.OrdinalIgnoreCase) || html.StartsWith("<xml", StringComparison.OrdinalIgnoreCase))
			{
				num = html.IndexOf(">");
				html = html.Remove(0, num + 1);
				continue;
			}
			if (html.StartsWith("<!doctype", StringComparison.OrdinalIgnoreCase))
			{
				num = html.IndexOf(">");
				html = html.Remove(0, num + 1);
				continue;
			}
			if (!html.StartsWith(ControlChar.CarriegeReturn) && !html.StartsWith(ControlChar.LineFeed))
			{
				break;
			}
			html = html.Remove(0, 1);
		}
		return html;
	}

	private string InsertHtmlElement(string html, XmlSchema schema)
	{
		string text = "<html>";
		if (schema != null)
		{
			text = "<html xmlns=\"" + schema.TargetNamespace + "\">";
		}
		if (!html.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
		{
			html = (html.StartsWith("<head", StringComparison.OrdinalIgnoreCase) ? (text + html + "</html>") : ((!html.StartsWith("<body", StringComparison.OrdinalIgnoreCase)) ? (text + $"<head><title>{GetDocumentTitle()}</title></head><body>" + html + "</body></html>") : (text + $"<head><title>{GetDocumentTitle()}</title></head>" + html + "</html>")));
		}
		else if (schema != null)
		{
			int num = html.ToLower().IndexOf("<body");
			if (num != -1)
			{
				html = text + $"<head><title>{GetDocumentTitle()}</title></head>" + html.Remove(0, num);
			}
		}
		return html;
	}

	private string GetDocumentTitle()
	{
		if (m_bodyItems != null && m_bodyItems.Document != null && m_bodyItems.Document.BuiltinDocumentProperties != null)
		{
			return m_bodyItems.Document.BuiltinDocumentProperties.Title;
		}
		return string.Empty;
	}

	private void TraverseChildNodes(XmlNodeList nodes)
	{
		XmlNode prevNode = null;
		foreach (XmlNode node in nodes)
		{
			if (node.NodeType == XmlNodeType.Text)
			{
				TraverseTextWithinTag(node, prevNode);
			}
			else if (node.NodeType == XmlNodeType.Comment)
			{
				TraverseComments(node);
			}
			else if (node.NodeType == XmlNodeType.Element)
			{
				ParseTags(node);
			}
			else if (node.NodeType == XmlNodeType.Whitespace || node.NodeType == XmlNodeType.SignificantWhitespace)
			{
				if ((StartsWithExt(node.Value, " ") || StartsWithExt(node.Value, ControlChar.Tab)) && m_currParagraph != null)
				{
					if (isPreTag)
					{
						continue;
					}
					TraverseTextWithinTag(node, prevNode);
				}
				if ((isPreTag || CurrentFormat.IsPreserveWhiteSpace) && (StartsWithExt(node.Value, ControlChar.CrLf) || StartsWithExt(node.Value, ControlChar.LineFeed)))
				{
					TraverseTextWithinTag(node, prevNode);
				}
			}
			prevNode = node;
		}
		if (nodes.Count == 0 && CurrentFormat.TabWidth != 0f && CurrentFormat.IsInlineBlock)
		{
			IWTextRange iWTextRange = CurrentPara.AppendText("\t");
			ApplyTextFormatting(iWTextRange.CharacterFormat);
		}
	}

	private void TraverseTextWithinTag(XmlNode node, XmlNode prevNode)
	{
		if (CurrentFormat != null && CurrentFormat.IsNonBreakingSpace && node.InnerText.Contains(ControlChar.NonBreakingSpace))
		{
			node.InnerText = node.InnerText.Replace(ControlChar.NonBreakingSpace, ControlChar.Space);
		}
		if (IsPargraphNeedToBeAdded(prevNode) && !isPreTag)
		{
			AddNewParagraphToTextBody(node);
		}
		else if (m_currParagraph == null && ((node.ParentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) && IsFirstNode(node)) || node.ParentNode.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase) || node.ParentNode.LocalName.Equals("blockquote", StringComparison.OrdinalIgnoreCase)))
		{
			AddNewParagraphToTextBody(node);
			if (node.ParentNode.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase))
			{
				m_currParagraph.IsCreatedUsingHtmlSpanTag = true;
			}
		}
		if (CurrentFormat != null && CurrentFormat.IsListTab)
		{
			if (CurrentFormat.TabPosition != 0f && m_currParagraph != null)
			{
				m_currParagraph.ParagraphFormat.Tabs.AddTab(CurrentFormat.TabPosition, CurrentFormat.TabJustification, CurrentFormat.TabLeader);
			}
			return;
		}
		string attributeValue = GetAttributeValue(node.ParentNode, "id");
		if (attributeValue != string.Empty)
		{
			CurrentPara.AppendBookmarkStart(attributeValue);
		}
		if (IsPreviousItemFieldStart || CurrentField != null)
		{
			ParseField(node);
			return;
		}
		if (isPreTag)
		{
			if (node.NodeType == XmlNodeType.Whitespace && node.Value.Equals(ControlChar.CrLf))
			{
				if (IsPargraphNeedToBeAdded(node.PreviousSibling))
				{
					AddNewParagraph(node);
					ApplyTextFormatting(CurrentPara.BreakCharacterFormat);
					ApplyParagraphFormat(node.ParentNode);
				}
			}
			else
			{
				string innerText = node.InnerText;
				string[] array = Regex.Split(innerText, ControlChar.CrLf);
				if (innerText.EndsWith(ControlChar.CrLf))
				{
					Array.Resize(ref array, array.Length - 1);
				}
				if (!innerText.Contains(ControlChar.CrLf))
				{
					IWTextRange iWTextRange = CurrentPara.AppendText(node.InnerText);
					ApplyTextFormatting(iWTextRange.CharacterFormat);
					ApplyTextFormatting(CurrentPara.BreakCharacterFormat);
					ApplyParagraphFormat(node.ParentNode);
				}
				else
				{
					string text = array[0];
					if (IsTabText(text))
					{
						text = "\t";
					}
					IWTextRange iWTextRange2 = CurrentPara.AppendText(text);
					ApplyTextFormatting(iWTextRange2.CharacterFormat);
					ApplyTextFormatting(CurrentPara.BreakCharacterFormat);
					ApplyParagraphFormat(node.ParentNode);
					for (int i = 1; i < array.Length; i++)
					{
						AddNewParagraph(node);
						ApplyParagraphStyle();
						text = array[i];
						if (IsTabText(text))
						{
							text = "\t";
						}
						iWTextRange2 = CurrentPara.AppendText(text);
						ApplyTextFormatting(iWTextRange2.CharacterFormat);
						ApplyTextFormatting(CurrentPara.BreakCharacterFormat);
						ApplyParagraphFormat(node.ParentNode);
					}
				}
			}
		}
		else
		{
			string text2 = (node.ParentNode.LocalName.Equals("title", StringComparison.OrdinalIgnoreCase) ? node.InnerText : RemoveNewLineCharacter(node.InnerText));
			if (node.ParentNode.LocalName.Equals("title", StringComparison.OrdinalIgnoreCase))
			{
				m_bodyItems.Document.BuiltinDocumentProperties.Title = text2;
			}
			else if (!(node.ParentNode.LocalName == "body") || node.PreviousSibling != null)
			{
				if (node.ParentNode.LocalName == "p" && checkFirstElement)
				{
					AddNewParagraph(node);
					checkFirstElement = true;
				}
				if (node.ParentNode.LocalName == "body")
				{
					bool spaceAfterAuto = m_currParagraph.ParagraphFormat.SpaceAfterAuto;
					bool spaceBeforeAuto = m_currParagraph.ParagraphFormat.SpaceBeforeAuto;
					ApplyParagraphStyle();
					if (!(spaceBeforeAuto && spaceAfterAuto) && m_userStyle == null && m_currParagraph.StyleName == "Normal (Web)")
					{
						m_currParagraph.ApplyStyle("Normal");
					}
				}
				if (IsTabText(node.InnerText))
				{
					IWTextRange iWTextRange3 = CurrentPara.AppendText("\t");
					ApplyTextFormatting(iWTextRange3.CharacterFormat);
				}
				else if (text2 != string.Empty && (!(text2 == " ") || !(node.ParentNode.LocalName == "br")))
				{
					string text3 = text2;
					if (IsTabText(node.InnerText))
					{
						text3 = "\t";
					}
					IWTextRange iWTextRange4 = CurrentPara.AppendText(text3);
					ApplyTextFormatting(iWTextRange4.CharacterFormat);
				}
				if (node.ParentNode.LocalName == "span")
				{
					ApplySpanParagraphFormat();
				}
				if (node.ParentNode.LocalName == "div" && currDivFormat.HasValue(31))
				{
					m_currParagraph.ParagraphFormat.Bidi = currDivFormat.Bidi;
				}
			}
			else
			{
				string text4 = text2;
				if (IsTabText(node.InnerText))
				{
					text4 = "\t";
				}
				IWTextRange iWTextRange5 = CurrentPara.AppendText(text4);
				ApplyTextFormatting(iWTextRange5.CharacterFormat);
			}
		}
		if (attributeValue != string.Empty)
		{
			CurrentPara.AppendBookmarkEnd(attributeValue);
		}
	}

	private bool IsTabText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			char[] obj = new char[5] { '\0', '\0', '.', '_', '-' };
			obj[0] = ControlChar.NonBreakingSpaceChar;
			obj[1] = ControlChar.SpaceChar;
			if (text.Trim(obj) == string.Empty && CurrentFormat != null && CurrentFormat.TabWidth != 0f && CurrentFormat.IsInlineBlock)
			{
				return true;
			}
		}
		return false;
	}

	private void AddNewParagraphToTextBody(XmlNode node)
	{
		AddNewParagraph(node);
		if (m_bIsInBlockquote)
		{
			CurrentPara.ParagraphFormat.SetPropertyValue(2, CurrentPara.ParagraphFormat.LeftIndent + (float)(m_blockquoteLevel * 36));
		}
	}

	private void ApplySpanParagraphFormat()
	{
		if (CurrentPara != null)
		{
			WParagraphFormat paragraphFormat = CurrentPara.ParagraphFormat;
			TextFormat currentFormat = CurrentFormat;
			if (currentFormat.HasValue(10))
			{
				paragraphFormat.HorizontalAlignment = currentFormat.TextAlign;
			}
			if (currentFormat.HasValue(8))
			{
				paragraphFormat.LineSpacingRule = currentFormat.LineSpacingRule;
				paragraphFormat.SetPropertyValue(52, currentFormat.LineHeight);
			}
			if (m_bIsWithinList && (!m_currParagraph.IsInCell || m_currParagraph.ListFormat.CurrentListStyle != null))
			{
				paragraphFormat.SetPropertyValue(2, currentFormat.LeftMargin + ((m_listLeftIndentStack.Count > 0) ? m_listLeftIndentStack.Peek() : 0f));
			}
			else if (currentFormat.HasValue(12))
			{
				paragraphFormat.SetPropertyValue(2, currentFormat.LeftMargin);
			}
			if (currentFormat.HasValue(15))
			{
				paragraphFormat.SetPropertyValue(5, currentFormat.TextIndent);
			}
			if (currentFormat.HasValue(14))
			{
				paragraphFormat.SetPropertyValue(3, currentFormat.RightMargin);
			}
			if (currentFormat.HasValue(17))
			{
				paragraphFormat.PageBreakBefore = currentFormat.PageBreakBefore;
				paragraphFormat.KeepFollow = currentFormat.PageBreakBefore;
			}
			if (currentFormat.HasValue(18))
			{
				paragraphFormat.PageBreakAfter = currentFormat.PageBreakAfter;
			}
			if (currentFormat.HasValue(22))
			{
				paragraphFormat.WordWrap = currentFormat.WordWrap;
			}
			if (currentFormat.TabPosition != 0f)
			{
				paragraphFormat.Tabs.AddTab(currentFormat.TabPosition, currentFormat.TabJustification, currentFormat.TabLeader);
			}
		}
	}

	private bool IsPargraphNeedToBeAdded(XmlNode node)
	{
		if (node != null && (node.LocalName.Equals("dt", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("dd", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("h1", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("h2", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("h3", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("h4", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("h5", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("h6", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("p", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("table", StringComparison.OrdinalIgnoreCase)))
		{
			return true;
		}
		return false;
	}

	private string RemoveWhiteSpacesAtParagraphBegin(string text, WParagraph CurrentPara)
	{
		if (StartsWithExt(text, " "))
		{
			if (CurrentPara.ChildEntities.LastItem != null && CurrentPara.ChildEntities.LastItem.EntityType == EntityType.Break)
			{
				text = text.TrimStart();
			}
			else if (CurrentPara.Text == "" || CurrentPara.Text == null)
			{
				text = text.TrimStart();
			}
		}
		return text;
	}

	private void AddNewParagraph(XmlNode node)
	{
		m_currParagraph = new WParagraph(m_bodyItems.Document);
		m_bodyItems.Add(m_currParagraph);
		if (isPreTag && !node.LocalName.Equals("p", StringComparison.OrdinalIgnoreCase))
		{
			m_currParagraph.ParagraphFormat.SetPropertyValue(8, 0f);
		}
		if (currDivFormat != null && (!m_bIsInDiv || !m_currParagraph.IsInCell || NodeIsInDiv(node)))
		{
			ApplyDivParagraphFormat(node);
		}
	}

	private bool NodeIsInDiv(XmlNode node)
	{
		XmlNode parentNode = node.ParentNode;
		while (parentNode != null && !parentNode.LocalName.Equals("td", StringComparison.OrdinalIgnoreCase) && !parentNode.LocalName.Equals("th", StringComparison.OrdinalIgnoreCase))
		{
			if (parentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			parentNode = parentNode.ParentNode;
		}
		return false;
	}

	private void TraverseParagraphTag(XmlNode node)
	{
		ApplyParagraphStyle();
		ApplyParagraphFormat(node);
		if (m_currParagraph != null)
		{
			ApplyTextFormatting(m_currParagraph.BreakCharacterFormat);
		}
		TraverseChildNodes(node.ChildNodes);
	}

	private bool IsFirstNode(XmlNode node)
	{
		while (true)
		{
			if (node.PreviousSibling == null)
			{
				return true;
			}
			if (node.PreviousSibling.NodeType == XmlNodeType.Whitespace || node.PreviousSibling.NodeType == XmlNodeType.SignificantWhitespace)
			{
				node = node.PreviousSibling;
				continue;
			}
			if (node.PreviousSibling.NodeType == XmlNodeType.Text || !IsEmptyNode(node.PreviousSibling))
			{
				break;
			}
			node = node.PreviousSibling;
		}
		return false;
	}

	private void ParseTags(XmlNode node)
	{
		string text = node.Name.ToLower();
		switch (text)
		{
		case "dir":
		case "body":
			TraverseChildNodes(node.ChildNodes);
			break;
		case "p":
			ParseParagraphTag(node);
			break;
		case "li":
		case "dt":
		case "dd":
		case "lh":
		{
			if (text == "li")
			{
				m_bIsWithinList = true;
				listCount++;
			}
			bool stylePresent2 = ParseStyle(node);
			WriteParagraph(node);
			LeaveStyle(stylePresent2);
			if (text == "li")
			{
				listCount--;
				if (listCount == 0)
				{
					m_bIsWithinList = false;
				}
			}
			ApplyBidiToParaItems();
			m_currParagraph = null;
			break;
		}
		case "div":
			OnDivBegin(node);
			TraverseChildNodes(node.ChildNodes);
			OnDivEnd();
			break;
		case "h1":
		case "h2":
		case "h3":
		case "h4":
		case "h5":
		case "h6":
		case "h7":
			if (!IsEmptyNode(node))
			{
				TextFormat tf = EnsureStyle(node);
				ParseHeadingTag(tf, node);
			}
			break;
		case "table":
			OnTableBegin();
			ParseTable(node);
			OnTableEnd();
			break;
		case "img":
		{
			TextFormat tf = EnsureStyle(node);
			WriteImage(node);
			LeaveStyle(stylePresent: true);
			if (m_currParagraph != null)
			{
				ApplyTextFormatting(m_currParagraph.BreakCharacterFormat);
			}
			break;
		}
		case "a":
		{
			string text2 = string.Empty;
			TextFormat tf = EnsureStyle(node);
			if (IsDefinedInline(node, "id"))
			{
				text2 = GetAttributeValue(node, "id");
			}
			else if (IsDefinedInline(node, "name"))
			{
				text2 = GetAttributeValue(node, "name");
			}
			if (GetAttributeValue(node, "href") == string.Empty && GetAttributeValue(node, "target") == string.Empty && text2 != string.Empty)
			{
				CurrentPara.AppendBookmarkStart(text2);
				TraverseChildNodes(node.ChildNodes);
				CurrentPara.AppendBookmarkEnd(text2);
			}
			else
			{
				WriteHyperlink(node);
			}
			LeaveStyle(stylePresent: true);
			if (m_currParagraph != null)
			{
				ApplyTextFormatting(m_currParagraph.BreakCharacterFormat);
			}
			break;
		}
		case "br":
		{
			bool stylePresent = ParseStyle(node);
			if (!IsDefinedInline(node, "page-break-before") && !IsDefinedInline(node, "page-break-after") && !IsDefinedInline(node, "page-break-inside"))
			{
				Break @break = CurrentPara.AppendBreak(BreakType.LineBreak);
				TraverseChildNodes(node.ChildNodes);
				switch (GetAttributeValue(node, "clear"))
				{
				default:
					if (!node.ParentNode.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase) || !isPreserveBreakForInvalidStyles)
					{
						break;
					}
					goto case "all";
				case "all":
				case "left":
				case "right":
					@break.HtmlToDocLayoutInfo.RemoveLineBreak = false;
					break;
				}
			}
			LeaveStyle(stylePresent);
			break;
		}
		case "blockquote":
			OnBlockquoteBegin(node);
			TraverseChildNodes(node.ChildNodes);
			OnBlockquoteEnd();
			break;
		case "title":
			TraverseChildNodes(node.ChildNodes);
			break;
		case "style":
			ParseCssStyle(node);
			break;
		case "input":
		case "select":
			ParseFormField(node);
			break;
		default:
			ParseFormattingTags(node);
			break;
		case "form":
			break;
		case "script":
			break;
		}
	}

	private void ParseFormField(XmlNode node)
	{
		if (m_currParagraph == null)
		{
			AddNewParagraphToTextBody(node);
		}
		int fieldType = -1;
		if (node.Name.Equals("select", StringComparison.OrdinalIgnoreCase))
		{
			fieldType = 2;
		}
		else if (node.Name.Equals("input", StringComparison.OrdinalIgnoreCase))
		{
			if (GetAttributeValue(node, "type").Equals("checkbox", StringComparison.OrdinalIgnoreCase))
			{
				fieldType = 1;
			}
			else if (GetAttributeValue(node, "type").Equals("text", StringComparison.OrdinalIgnoreCase))
			{
				fieldType = 0;
			}
		}
		WFormField wFormField = InsertFormField(node, fieldType);
		if (wFormField == null)
		{
			return;
		}
		switch (wFormField.FormFieldType)
		{
		case FormFieldType.DropDown:
			ParseDropDownField(node, wFormField as WDropDownFormField);
			break;
		case FormFieldType.CheckBox:
		{
			WCheckBox wCheckBox = wFormField as WCheckBox;
			bool stylePresent = ParseCheckBoxProperties(node, wCheckBox);
			ApplyTextFormatting(wCheckBox.CharacterFormat);
			LeaveStyle(stylePresent);
			break;
		}
		default:
		{
			string attributeValue = GetAttributeValue(node, "value");
			if (attributeValue != string.Empty)
			{
				(wFormField as WTextFormField).Text = attributeValue;
			}
			else
			{
				(wFormField as WTextFormField).Text = "\u2002\u2002\u2002\u2002\u2002";
			}
			break;
		}
		}
		CurrentPara.Items.OnInsertFormFieldComplete(CurrentPara.Items.Count - 1, wFormField);
	}

	private bool ParseCheckBoxProperties(XmlNode node, WCheckBox checkbox)
	{
		checkbox.Checked = IsNodeContainAttribute(node, "checked");
		string attributeValue = GetAttributeValue(node, "style");
		if (attributeValue.Length != 0 || CSSStyle != null)
		{
			TextFormat textFormat = AddStyle();
			string[] array = attributeValue.Split(';', ':');
			int i = 0;
			for (int num = array.Length; i < num - 1; i += 2)
			{
				string text = array[i].ToLower().Trim();
				string text2 = array[i + 1].ToLower().Trim();
				switch (text)
				{
				case "width":
				case "height":
					if (attributeValue.Contains("-sf-size-type"))
					{
						string styleAttributeValue = GetStyleAttributeValue(attributeValue, "-sf-size-type");
						checkbox.SizeType = ((!(styleAttributeValue == "auto")) ? CheckBoxSizeType.Exactly : CheckBoxSizeType.Auto);
					}
					if (checkbox.SizeType == CheckBoxSizeType.Exactly)
					{
						checkbox.CheckBoxSize = (int)Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				default:
					GetFormat(textFormat, text, text2, node);
					break;
				case "-sf-size-type":
					break;
				}
			}
			FindCSSstyleItem(node, textFormat);
			return true;
		}
		return false;
	}

	private WFormField InsertFormField(XmlNode node, int fieldType)
	{
		string attributeValue = GetAttributeValue(node, "name");
		WFormField wFormField = null;
		switch (fieldType)
		{
		case 2:
			wFormField = CurrentPara.AppendField(attributeValue, FieldType.FieldFormDropDown) as WFormField;
			break;
		case 1:
			wFormField = CurrentPara.AppendField(attributeValue, FieldType.FieldFormCheckBox) as WFormField;
			break;
		case 0:
			wFormField = CurrentPara.AppendField(attributeValue, FieldType.FieldFormTextInput) as WFormField;
			break;
		}
		if (wFormField != null)
		{
			OnInsertFormField(wFormField);
		}
		return wFormField;
	}

	internal void OnInsertFormField(WFormField formField)
	{
		switch (formField.FormFieldType)
		{
		case FormFieldType.CheckBox:
		{
			WCheckBox wCheckBox = formField as WCheckBox;
			if (wCheckBox.Name == null || wCheckBox.Name == string.Empty)
			{
				string text3 = "Check_" + Guid.NewGuid().ToString().Replace("-", "_");
				wCheckBox.Name = text3.Substring(0, 20);
			}
			break;
		}
		case FormFieldType.DropDown:
		{
			WDropDownFormField wDropDownFormField = formField as WDropDownFormField;
			if (wDropDownFormField.Name == null || wDropDownFormField.Name == string.Empty)
			{
				string text2 = "Drop_" + Guid.NewGuid().ToString().Replace("-", "_");
				wDropDownFormField.Name = text2.Substring(0, 20);
			}
			break;
		}
		case FormFieldType.TextInput:
		{
			WTextFormField wTextFormField = formField as WTextFormField;
			if (wTextFormField.Name == null || wTextFormField.Name == string.Empty)
			{
				string text = "Text_" + Guid.NewGuid().ToString().Replace("-", "_");
				wTextFormField.Name = text.Substring(0, 20);
			}
			if (wTextFormField.DefaultText == null || wTextFormField.DefaultText == string.Empty)
			{
				wTextFormField.DefaultText = "\u2002\u2002\u2002\u2002\u2002";
			}
			break;
		}
		}
		if (CurrentPara.Document != null)
		{
			CurrentPara.Items.Insert(CurrentPara.ChildEntities.Count - 1, new BookmarkStart(CurrentPara.Document, formField.Name));
		}
	}

	private void ParseDropDownField(XmlNode node, WDropDownFormField dropDownFormField)
	{
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name.ToLower() == "option")
			{
				if (childNode.InnerText == string.Empty)
				{
					dropDownFormField.DropDownItems.Add(" ");
				}
				else
				{
					dropDownFormField.DropDownItems.Add(childNode.InnerText);
				}
				if (IsNodeContainAttribute(childNode, "selected"))
				{
					dropDownFormField.DropDownSelectedIndex = dropDownFormField.DropDownItems.Count - 1;
				}
			}
			else
			{
				ParseDropDownField(childNode, dropDownFormField);
			}
		}
	}

	private bool IsNodeContainAttribute(XmlNode node, string attrName)
	{
		attrName = attrName.ToLower();
		for (int i = 0; i < node.Attributes.Count; i++)
		{
			if (node.Attributes[i].LocalName.Equals(attrName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void ParseParagraphTag(XmlNode node)
	{
		if (!IsEmptyNode(node))
		{
			if ((!node.ParentNode.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase) || !IsFirstNode(node)) && (!node.ParentNode.LocalName.Equals("td", StringComparison.OrdinalIgnoreCase) || !IsFirstNode(node)) && !isPreTag)
			{
				AddNewParagraph(node);
			}
			bool stylePresent = ParseStyle(node);
			TraverseParagraphTag(node);
			LeaveStyle(stylePresent);
		}
		ApplyBidiToParaItems();
		m_currParagraph = null;
	}

	private void ApplyBidiToParaItems()
	{
		if (m_currParagraph == null || m_currParagraph.Items.Count == 0)
		{
			return;
		}
		TextSplitter splitter = new TextSplitter();
		m_currParagraph.SplitLtrAndRtlText(m_currParagraph.Items, splitter);
		m_currParagraph.CombineconsecutiveRTL(m_currParagraph.Items);
		foreach (Entity item in m_currParagraph.Items)
		{
			if (item.EntityType == EntityType.TextRange)
			{
				WTextRange wTextRange = item as WTextRange;
				if (wTextRange.CharacterRange == CharacterRangeType.RTL)
				{
					wTextRange.CharacterFormat.Bidi = true;
				}
			}
		}
	}

	private bool IsEmptyNode(XmlNode node)
	{
		bool result = true;
		if (node.LocalName.Equals("img", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("br", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("a", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Whitespace && childNode.NodeType != XmlNodeType.SignificantWhitespace)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void ParseHeadingTag(TextFormat tf, XmlNode node)
	{
		switch (node.LocalName.ToLower())
		{
		case "h1":
			tf.Style = BuiltinStyle.Heading1;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 24f;
			}
			if (!tf.HasKey(1))
			{
				tf.FontFamily = (isPreTag ? "Courier New" : "Times New Roman");
			}
			break;
		case "h2":
			tf.Style = BuiltinStyle.Heading2;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 18f;
			}
			if (!tf.HasKey(1))
			{
				tf.FontFamily = (isPreTag ? "Courier New" : "Times New Roman");
			}
			break;
		case "h3":
			tf.Style = BuiltinStyle.Heading3;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 13f;
			}
			if (!tf.HasKey(1))
			{
				tf.FontFamily = (isPreTag ? "Courier New" : "Times New Roman");
			}
			break;
		case "h4":
			tf.Style = BuiltinStyle.Heading4;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 12f;
			}
			if (!tf.HasKey(1) && isPreTag)
			{
				tf.FontFamily = "Courier New";
			}
			break;
		case "h5":
			tf.Style = BuiltinStyle.Heading5;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 10f;
			}
			if (!tf.HasKey(1) && isPreTag)
			{
				tf.FontFamily = "Courier New";
			}
			break;
		case "h6":
			tf.Style = BuiltinStyle.Heading6;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 7f;
			}
			if (!tf.HasKey(1) && isPreTag)
			{
				tf.FontFamily = "Courier New";
			}
			break;
		case "h7":
			tf.Style = BuiltinStyle.Heading7;
			if (!tf.HasKey(0))
			{
				tf.FontSize = 12f;
			}
			if (!tf.HasKey(1) && isPreTag)
			{
				tf.FontFamily = "Courier New";
			}
			break;
		}
		WriteParagraph(node);
		LeaveStyle(stylePresent: true);
		ApplyBidiToParaItems();
		m_currParagraph = null;
	}

	private void OnBlockquoteBegin(XmlNode node)
	{
		ApplyBidiToParaItems();
		m_currParagraph = null;
		m_bIsInBlockquote = true;
		m_blockquoteLevel++;
		OnDivBegin(node);
	}

	private void OnBlockquoteEnd()
	{
		m_blockquoteLevel--;
		if (m_blockquoteLevel == 0)
		{
			m_bIsInBlockquote = false;
		}
		OnDivEnd();
		ApplyBidiToParaItems();
		m_currParagraph = null;
	}

	private void OnDivBegin(XmlNode node)
	{
		currDivFormat = null;
		m_divCount++;
		if (m_bIsInDiv)
		{
			ApplyBidiToParaItems();
			m_currParagraph = null;
		}
		m_bIsInDiv = true;
		HorizontalAlignment horizontalAlignment = GetHorizontalAlignment(GetAttributeValue(node, "align"));
		if (ParseStyle(node))
		{
			currDivFormat = m_styleStack.Peek();
		}
		else if (m_bIsInDiv && m_styleStack.Count > 0)
		{
			TextFormat item = m_styleStack.Peek().Clone();
			m_styleStack.Push(item);
			currDivFormat = m_styleStack.Peek();
		}
		if (currDivFormat != null && !string.IsNullOrEmpty(GetAttributeValue(node, "align")))
		{
			currDivFormat.TextAlign = horizontalAlignment;
		}
		else if (currDivFormat == null && horizontalAlignment != 0)
		{
			currDivFormat = new TextFormat();
			if (!currDivFormat.HasKey(10))
			{
				currDivFormat.TextAlign = horizontalAlignment;
			}
		}
	}

	private void OnDivEnd()
	{
		m_divCount--;
		m_bIsInDiv = m_divCount != 0;
		if (m_styleStack.Count > 0)
		{
			m_styleStack.Pop();
		}
		if (m_styleStack.Count > 0 && m_bIsInDiv)
		{
			currDivFormat = m_styleStack.Peek();
		}
		else
		{
			currDivFormat = null;
		}
		ApplyBidiToParaItems();
		m_currParagraph = null;
	}

	private void OnTableEnd()
	{
		if (m_currTable != null)
		{
			if (m_currTable.IsInCell)
			{
				WTable ownerTable = m_currTable.GetOwnerTable();
				while (ownerTable.IsInCell)
				{
					ownerTable = ownerTable.GetOwnerTable();
				}
				ownerTable.RecalculateTables.Insert(0, m_currTable);
			}
			else
			{
				m_currTable.UpdateGridSpan();
			}
		}
		if (m_currTable.DocxTableFormat.StyleName == null)
		{
			m_currTable.DocxTableFormat.StyleName = string.Empty;
		}
		if (m_currTable.RecalculateTables.Count > 0)
		{
			foreach (WTable recalculateTable in m_currTable.RecalculateTables)
			{
				float ownerWidth = recalculateTable.GetOwnerWidth();
				recalculateTable.IsTableGridUpdated = false;
				SetNestedTableCellWidthBasedOnPreferredWidth(recalculateTable, ownerWidth);
				recalculateTable.UpdateGridSpan();
			}
			if (m_currTable.m_recalculateTables != null)
			{
				m_currTable.m_recalculateTables.Clear();
				m_currTable.m_recalculateTables = null;
			}
		}
		m_currTable = m_nestedTable.Pop();
		m_bodyItems = m_nestedBodyItems.Pop();
		IsCellStyle = m_stackCellStyle.Pop();
		IsRowStyle = m_stackRowStyle.Pop();
		IsTableStyle = m_stackTableStyle.Pop();
		ApplyBidiToParaItems();
		m_currParagraph = null;
	}

	private void OnTableBegin()
	{
		m_nestedBodyItems.Push(m_bodyItems);
		m_nestedTable.Push(m_currTable);
		m_stackCellStyle.Push(IsCellStyle);
		m_stackRowStyle.Push(IsRowStyle);
		m_stackTableStyle.Push(IsTableStyle);
	}

	private void SetNestedTableCellWidthBasedOnPreferredWidth(WTable table, float clientWidth)
	{
		float totalCellPreferredWidth = 0f;
		List<float> maxPrefCellWidthOfColumns = table.GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, table.MaximumCellCount());
		if (table.HasPercentPreferredCellWidth && !table.HasPointPreferredCellWidth && !table.HasAutoPreferredCellWidth && !maxPrefCellWidthOfColumns.Contains(0f))
		{
			foreach (WTableRow row in table.Rows)
			{
				for (int i = 0; i < row.Cells.Count; i++)
				{
					row.Cells[i].Width = clientWidth * maxPrefCellWidthOfColumns[i] / 100f;
				}
			}
		}
		if (table.GetOwnerTableCell().PreferredWidth.WidthType != FtsWidth.Percentage || !table.HasPointPreferredCellWidth || table.HasAutoPreferredCellWidth || table.HasPercentPreferredCellWidth || !maxPrefCellWidthOfColumns.Contains(0f) || !(totalCellPreferredWidth <= clientWidth))
		{
			return;
		}
		List<int> list = new List<int>();
		for (int j = 0; j < maxPrefCellWidthOfColumns.Count; j++)
		{
			if (maxPrefCellWidthOfColumns[j] == 0f)
			{
				list.Add(j);
			}
		}
		if (list.Count != 0)
		{
			float value = (clientWidth - totalCellPreferredWidth) / (float)list.Count;
			foreach (int item in list)
			{
				maxPrefCellWidthOfColumns[item] = value;
			}
		}
		foreach (WTableRow row2 in table.Rows)
		{
			for (int k = 0; k < row2.Cells.Count; k++)
			{
				row2.Cells[k].Width = maxPrefCellWidthOfColumns[k];
			}
		}
	}

	private void WriteHyperlink(XmlNode node)
	{
		HyperlinkType hyperlinkType = HyperlinkType.None;
		string text = GetAttributeValue(node, "href");
		IWField iWField = TraverseHyperlinkField(node);
		if (!StartsWithExt(text, "#"))
		{
			hyperlinkType = (StartsWithExt(text, "mailto:") ? HyperlinkType.EMailLink : ((!StartsWithExt(text, "http") && !StartsWithExt(text, "www")) ? HyperlinkType.FileLink : HyperlinkType.WebLink));
		}
		else
		{
			hyperlinkType = HyperlinkType.Bookmark;
			text = text.Replace("#", string.Empty);
		}
		Hyperlink hyperlink = new Hyperlink(iWField as WField);
		hyperlink.Type = hyperlinkType;
		if (hyperlinkType == HyperlinkType.WebLink || hyperlinkType == HyperlinkType.EMailLink)
		{
			hyperlink.Uri = text;
		}
		else if (hyperlink.Type == HyperlinkType.Bookmark)
		{
			hyperlink.BookmarkName = text;
		}
		else if (hyperlink.Type == HyperlinkType.FileLink)
		{
			hyperlink.FilePath = text;
		}
		ApplyHyperlinkStyle(iWField as WField);
	}

	private IWField TraverseHyperlinkField(XmlNode node)
	{
		WField wField = new WField(CurrentPara.Document);
		wField.FieldType = FieldType.FieldHyperlink;
		CurrentPara.Items.Add(wField);
		CurrentPara.AppendText(string.Empty);
		CurrentPara.AppendFieldMark(FieldMarkType.FieldSeparator);
		wField.FieldSeparator = CurrentPara.LastItem as WFieldMark;
		TraverseChildNodes(node.ChildNodes);
		WFieldMark wFieldMark = new WFieldMark(CurrentPara.Document, FieldMarkType.FieldEnd);
		CurrentPara.Items.Add(wFieldMark);
		wField.FieldEnd = wFieldMark;
		return wField;
	}

	private void ApplyHyperlinkStyle(WField field)
	{
		WParagraph ownerParagraph = field.OwnerParagraph;
		int num = ownerParagraph.Items.IndexOf(field);
		bool flag = false;
		for (int i = num; i < ownerParagraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = ownerParagraph.Items[i];
			if (paragraphItem.EntityType == EntityType.FieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldSeparator)
			{
				flag = true;
			}
			else if (paragraphItem.EntityType == EntityType.TextRange && flag)
			{
				if (m_hyperlinkcolor != Color.Empty)
				{
					(paragraphItem as WTextRange).CharacterFormat.TextColor = m_hyperlinkcolor;
				}
				if (m_bodyItems.Document.Styles.FindByName(BuiltinStyle.Hyperlink.ToString()) == null)
				{
					IStyle style = Style.CreateBuiltinStyle(BuiltinStyle.Hyperlink, StyleType.CharacterStyle, m_bodyItems.Document);
					m_bodyItems.Document.Styles.Add(style);
				}
				(paragraphItem as WTextRange).CharacterFormat.CharStyleName = BuiltinStyle.Hyperlink.ToString();
				ApplyTextFormatting((paragraphItem as WTextRange).CharacterFormat);
			}
			else if (paragraphItem.EntityType == EntityType.FieldMark && (paragraphItem as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				flag = false;
				break;
			}
		}
	}

	private void ParseImageAttribute(XmlNode node, IWPicture pic, ref bool isHeightSpecified, ref bool isWidthSpecified)
	{
		foreach (XmlAttribute attribute in node.Attributes)
		{
			switch (attribute.Name.ToLower())
			{
			case "height":
				if (attribute.Value != "auto" && attribute.Value != "inherit" && attribute.Value != "initial")
				{
					pic.Height = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
					isHeightSpecified = true;
				}
				break;
			case "width":
				if (attribute.Value != "auto" && attribute.Value != "inherit" && attribute.Value != "initial")
				{
					pic.Width = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
					isWidthSpecified = true;
				}
				break;
			case "style":
				ParseImageStyleAttribute(attribute, pic, ref isHeightSpecified, ref isWidthSpecified);
				break;
			case "align":
				switch (attribute.Value)
				{
				case "top":
					pic.VerticalAlignment = ShapeVerticalAlignment.Top;
					break;
				case "bottom":
					pic.VerticalAlignment = ShapeVerticalAlignment.Bottom;
					break;
				case "middle":
					pic.HorizontalAlignment = ShapeHorizontalAlignment.Center;
					break;
				case "left":
					pic.HorizontalAlignment = ShapeHorizontalAlignment.Left;
					break;
				case "right":
					pic.HorizontalAlignment = ShapeHorizontalAlignment.Right;
					(pic as WPicture).SetTextWrappingStyleValue(TextWrappingStyle.Square);
					break;
				}
				break;
			case "alt":
				if (attribute.Value != null && attribute.Value != string.Empty)
				{
					pic.AlternativeText = attribute.Value.Trim();
				}
				break;
			}
		}
	}

	private void ParseImageStyleAttribute(XmlAttribute attr, IWPicture pic, ref bool isHeightSpecified, ref bool isWidthSpecified)
	{
		if (!attr.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		string[] array = attr.Value.Split(';', ':');
		int i = 0;
		for (int num = array.Length; i < num - 1; i += 2)
		{
			string text = array[i].ToLower().Trim();
			string text2 = array[i + 1].ToLower().Trim();
			if (!(text == "height"))
			{
				if (text == "width" && text2 != "auto" && text2 != "inherit" && text2 != "initial")
				{
					pic.Width = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					isWidthSpecified = true;
				}
			}
			else if (text2 != "auto" && text2 != "inherit" && text2 != "initial")
			{
				pic.Height = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				isHeightSpecified = true;
			}
		}
	}

	private void WriteImage(XmlNode node)
	{
		if (m_currParagraph == null && m_bIsInDiv && ((node.ParentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) && IsFirstNode(node)) || node.ParentNode.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase)))
		{
			AddNewParagraph(node);
			if (node.ParentNode.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase))
			{
				m_currParagraph.IsCreatedUsingHtmlSpanTag = true;
			}
		}
		string attributeValue = GetAttributeValue(node, "src");
		bool isHeightSpecified = false;
		bool isWidthSpecified = false;
		IWPicture iWPicture = new WPicture(m_bodyItems.Document);
		GetImage(attributeValue, iWPicture);
		if (iWPicture.ImageBytes != null)
		{
			CurrentPara.Items.Add(iWPicture);
			ParseImageAttribute(node, iWPicture, ref isHeightSpecified, ref isWidthSpecified);
		}
		UpdateHeightAndWidth(iWPicture as WPicture, isHeightSpecified, isWidthSpecified);
		ApplyTextFormatting((iWPicture as ParagraphItem).ParaItemCharFormat);
	}

	private async Task GetImage(string src, IWPicture pic)
	{
		try
		{
			if (StartsWithExt(src, "data:image/"))
			{
				if (HtmlImportSettings != null)
				{
					ImageNodeVisitedEventArgs imageNodeVisitedEventArgs = HtmlImportSettings.ExecuteImageNodeVisitedEvent(null, src);
					if (imageNodeVisitedEventArgs.ImageStream != null)
					{
						pic.LoadImage(imageNodeVisitedEventArgs.ImageStream);
					}
					else
					{
						int num = src.IndexOf(",");
						src = src.Substring(num + 1);
						byte[] imageBytes = Convert.FromBase64String(src);
						pic.LoadImage(imageBytes);
					}
				}
			}
			else if (StartsWithExt(src, "http") || StartsWithExt(src, "ftp"))
			{
				if (HtmlImportSettings != null)
				{
					ImageNodeVisitedEventArgs imageNodeVisitedEventArgs2 = HtmlImportSettings.ExecuteImageNodeVisitedEvent(null, src);
					pic.LoadImage(imageNodeVisitedEventArgs2.ImageStream);
				}
				else
				{
					Stream manifestResourceStream = GetManifestResourceStream("ImageNotFound.jpg");
					pic.LoadImage(manifestResourceStream);
					manifestResourceStream.Dispose();
				}
			}
			else if (HtmlImportSettings != null)
			{
				ImageNodeVisitedEventArgs imageNodeVisitedEventArgs3 = HtmlImportSettings.ExecuteImageNodeVisitedEvent(null, src);
				pic.LoadImage(imageNodeVisitedEventArgs3.ImageStream);
			}
			else
			{
				Stream manifestResourceStream2 = GetManifestResourceStream("ImageNotFound.jpg");
				pic.LoadImage(manifestResourceStream2);
				manifestResourceStream2.Dispose();
			}
		}
		catch
		{
			Stream manifestResourceStream3 = GetManifestResourceStream("ImageNotFound.jpg");
			pic.LoadImage(manifestResourceStream3);
			manifestResourceStream3.Dispose();
		}
		if (pic.ImageBytes == null)
		{
			Stream manifestResourceStream4 = GetManifestResourceStream("ImageNotFound.jpg");
			pic.LoadImage(manifestResourceStream4);
			manifestResourceStream4.Dispose();
		}
	}

	private Stream GetManifestResourceStream(string fileName)
	{
		Assembly assembly = typeof(HTMLConverterImpl).GetTypeInfo().Assembly;
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.EndsWith("." + fileName))
			{
				fileName = text;
				break;
			}
		}
		return assembly.GetManifestResourceStream(fileName);
	}

	private void ParseFormattingTags(XmlNode tag)
	{
		if (m_curListLevel < 0)
		{
			m_listLeftIndentStack.Clear();
		}
		TextFormat textFormat = EnsureStyle(tag);
		switch (tag.Name.ToLower())
		{
		case "b":
		case "strong":
			textFormat.Bold = true;
			break;
		case "i":
		case "em":
		case "cite":
		case "dfn":
		case "var":
			textFormat.Italic = true;
			break;
		case "u":
			textFormat.Underline = true;
			break;
		case "s":
		case "strike":
			textFormat.Strike = true;
			break;
		case "small":
			if (textFormat.FontSize < 0f)
			{
				textFormat.FontSize = 10f;
			}
			textFormat.FontSize -= 2f;
			break;
		case "big":
			textFormat.FontSize += 2f;
			break;
		case "tt":
		case "code":
		case "samp":
			textFormat.FontFamily = "Courier New";
			textFormat.FontSize = 10f;
			break;
		case "pre":
			isPreTag = true;
			textFormat.Style = BuiltinStyle.HtmlPreformatted;
			AddNewParagraph(tag);
			ApplyParagraphStyle();
			break;
		case "font":
		{
			string attributeValue = GetAttributeValue(tag, "color");
			string attributeValue2 = GetAttributeValue(tag, "face");
			if (attributeValue.Length > 0)
			{
				textFormat.FontColor = GetColor(attributeValue);
			}
			if (attributeValue2.Length > 0)
			{
				textFormat.FontFamily = GetFontName(attributeValue2);
			}
			string attributeValue3 = GetAttributeValue(tag, "size");
			if (attributeValue3.Length > 0)
			{
				ApplyFontSize(attributeValue3, textFormat);
			}
			break;
		}
		case "ul":
			m_curListLevel++;
			SetListMode(isBulleted: true, tag, textFormat);
			if (!IsDefinedInline(tag, "margin-left") && !IsDefinedInline(tag, "margin") && m_curListLevel >= 0)
			{
				UpdateListLeftIndentStack(0f, isInlineLeftIndent: false);
			}
			if (m_curListLevel == 0 || (isLastLevelSkipped && lastSkippedLevelNo != -1 && lastSkippedLevelNo <= lastUsedLevelNo && m_curListLevel <= lastUsedLevelNo))
			{
				CreateListStyle(tag);
			}
			break;
		case "ol":
			m_curListLevel++;
			SetListMode(isBulleted: false, tag, textFormat);
			if (!IsDefinedInline(tag, "margin-left") && !IsDefinedInline(tag, "margin") && m_curListLevel >= 0)
			{
				UpdateListLeftIndentStack(0f, isInlineLeftIndent: false);
			}
			if (m_curListLevel == 0 || (isLastLevelSkipped && lastSkippedLevelNo != -1 && lastSkippedLevelNo <= lastUsedLevelNo && m_curListLevel <= lastUsedLevelNo))
			{
				CreateListStyle(tag);
			}
			break;
		case "a":
			textFormat.FontColor = Color.Blue;
			textFormat.Underline = true;
			break;
		case "sup":
			textFormat.SubSuperScript = SubSuperScript.SuperScript;
			break;
		case "sub":
			textFormat.SubSuperScript = SubSuperScript.SubScript;
			break;
		case "del":
			textFormat.Strike = true;
			break;
		}
		TraverseChildNodes(tag.ChildNodes);
		if (isPreserveBreakForInvalidStyles)
		{
			isPreserveBreakForInvalidStyles = false;
		}
		if (currDivFormat != null && tag.LocalName.Equals("label", StringComparison.OrdinalIgnoreCase) && m_currParagraph != null && (!m_bIsInDiv || !m_currParagraph.IsInCell || NodeIsInDiv(tag)))
		{
			ApplyDivParagraphFormat(tag);
		}
		if (tag.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase) || tag.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase))
		{
			m_curListLevel--;
			if (m_listLeftIndentStack.Count > 0)
			{
				m_listLeftIndentStack.Pop();
			}
			if (m_curListLevel < 0 && ListStack.Count > 0)
			{
				ListStack.Pop();
			}
			if (LfoStack.Count > 0)
			{
				LfoStack.Pop();
			}
			if (m_curListLevel < 0)
			{
				m_listLevelNo.Clear();
			}
		}
		if (tag.LocalName.Equals("pre", StringComparison.OrdinalIgnoreCase))
		{
			isPreTag = false;
		}
		LeaveStyle(stylePresent: true);
	}

	private void UpdateListLeftIndentStack(float leftIndent, bool isInlineLeftIndent)
	{
		if (m_listLeftIndentStack.Count > 0)
		{
			if (isInlineLeftIndent)
			{
				m_listLeftIndentStack.Push(m_listLeftIndentStack.Peek() + leftIndent);
			}
			else
			{
				m_listLeftIndentStack.Push(m_listLeftIndentStack.Peek() + 36f);
			}
		}
		else if (isInlineLeftIndent)
		{
			m_listLeftIndentStack.Push(leftIndent);
		}
		else
		{
			m_listLeftIndentStack.Push(36f);
		}
	}

	private void ApplyFontSize(string value, TextFormat format)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (StartsWithExt(value, "+"))
		{
			flag = true;
			value = value.Substring(1, value.Length - 1);
		}
		else if (StartsWithExt(value, "-"))
		{
			flag2 = true;
			value = value.Substring(1, value.Length - 1);
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (char.IsDigit(value[i]))
			{
				flag3 = false;
				continue;
			}
			flag3 = true;
			break;
		}
		if (flag3)
		{
			format.FontSize = 12f;
			return;
		}
		int num = Convert.ToInt32(value);
		if (flag)
		{
			num = 3 + num;
		}
		else if (flag2)
		{
			num = 3 - num;
		}
		if (num <= 1)
		{
			format.FontSize = 7.5f;
			return;
		}
		if (num >= 7)
		{
			format.FontSize = 36f;
			return;
		}
		switch (num)
		{
		case 2:
			format.FontSize = 10f;
			break;
		case 3:
			format.FontSize = 12f;
			break;
		case 4:
			format.FontSize = 13.5f;
			break;
		case 5:
			format.FontSize = 18f;
			break;
		case 6:
			format.FontSize = 24f;
			break;
		}
	}

	private void SetListMode(bool isBulleted, XmlNode node, TextFormat format)
	{
		if (isBulleted)
		{
			format.DefBulleted = true;
		}
		else
		{
			format.NumBulleted = true;
		}
	}

	private void WriteParagraph(XmlNode node)
	{
		if ((node.ParentNode.LocalName != "li" && (!node.ParentNode.LocalName.Equals("td", StringComparison.OrdinalIgnoreCase) || !IsFirstNode(node))) || (IsHeadingStyle() && m_currParagraph == null))
		{
			AddNewParagraph(node);
		}
		TraverseParagraphTag(node);
	}

	private bool IsHeadingStyle()
	{
		TextFormat currentFormat = CurrentFormat;
		bool result = false;
		BuiltinStyle style = currentFormat.Style;
		if ((uint)(style - 1) <= 6u)
		{
			result = true;
		}
		return result;
	}

	private void ApplyParagraphStyle()
	{
		TextFormat currentFormat = CurrentFormat;
		if (m_currParagraph == null)
		{
			return;
		}
		if (currentFormat.Style != 0)
		{
			int count = m_currParagraph.Document.Styles.Count;
			m_currParagraph.ApplyStyle(currentFormat.Style, isDomChanges: false);
			IWParagraphStyle paraStyle = m_currParagraph.ParaStyle;
			if (paraStyle.ParagraphFormat.KeepFollow)
			{
				paraStyle.ParagraphFormat.KeepFollow = false;
			}
			if (!IsHeadingStyle())
			{
				return;
			}
			if (count == m_currParagraph.Document.Styles.Count)
			{
				if (!currentFormat.HasKey(2))
				{
					currentFormat.Bold = currentFormat.Style != BuiltinStyle.Heading7;
				}
				if (!currentFormat.HasKey(4))
				{
					currentFormat.Italic = false;
				}
			}
			else
			{
				bool flag = currentFormat.Style != BuiltinStyle.Heading7;
				paraStyle.CharacterFormat.Bold = flag;
				paraStyle.CharacterFormat.BoldBidi = flag;
				paraStyle.CharacterFormat.Italic = false;
				paraStyle.CharacterFormat.ItalicBidi = false;
			}
		}
		else if (m_userStyle != null)
		{
			m_currParagraph.ApplyStyle(m_userStyle, isDomChanges: false);
		}
		else
		{
			m_currParagraph.ApplyStyle(BuiltinStyle.NormalWeb, isDomChanges: false);
		}
	}

	private void ApplyParagraphFormat(XmlNode node)
	{
		if (m_currParagraph == null)
		{
			return;
		}
		if (!m_bIsInDiv || !m_currParagraph.IsInCell)
		{
			ApplyDivParagraphFormat(node);
		}
		WParagraphFormat paragraphFormat = m_currParagraph.ParagraphFormat;
		TextFormat currentFormat = CurrentFormat;
		if (!node.Name.Equals("th", StringComparison.OrdinalIgnoreCase) && !node.Name.Equals("td", StringComparison.OrdinalIgnoreCase))
		{
			ApplyParagraphBorder(paragraphFormat, currentFormat);
		}
		currentFormat.Borders = new TableBorders();
		if (node.LocalName.Equals("dd", StringComparison.OrdinalIgnoreCase))
		{
			m_currParagraph.ParagraphFormat.SetPropertyValue(2, 36f);
		}
		if (currentFormat.HasValue(10))
		{
			paragraphFormat.HorizontalAlignment = currentFormat.TextAlign;
		}
		if (m_currParagraph.IsInCell && currentFormat.HasValue(10))
		{
			m_currParagraph.ParagraphFormat.HorizontalAlignment = currentFormat.TextAlign;
		}
		ApplyListFormatting(paragraphFormat, currentFormat, node);
		if (currentFormat.HasValue(8))
		{
			paragraphFormat.LineSpacingRule = currentFormat.LineSpacingRule;
			paragraphFormat.SetPropertyValue(52, currentFormat.LineHeight);
		}
		if (currentFormat.HasValue(25))
		{
			paragraphFormat.Keep = currentFormat.KeepLinesTogether;
		}
		if (currentFormat.HasValue(26))
		{
			paragraphFormat.KeepFollow = currentFormat.KeepWithNext;
		}
		if (m_bIsWithinList)
		{
			if (!m_currParagraph.IsInCell || m_currParagraph.ListFormat.CurrentListStyle != null)
			{
				paragraphFormat.SetPropertyValue(2, AdjustLeftIndentForList(node, currentFormat));
			}
		}
		else if (currentFormat.HasValue(12) && currentFormat.LeftMargin > 0f)
		{
			paragraphFormat.SetPropertyValue(2, currentFormat.LeftMargin);
		}
		if (currentFormat.HasValue(15))
		{
			paragraphFormat.SetPropertyValue(5, currentFormat.TextIndent);
		}
		if (currentFormat.HasValue(14))
		{
			paragraphFormat.SetPropertyValue(3, currentFormat.RightMargin);
		}
		if (IsBottomMarginNeedToBePreserved(node, currentFormat))
		{
			if (currentFormat.HasValue(27))
			{
				paragraphFormat.SpaceAfterAuto = currentFormat.AfterSpaceAuto;
			}
			else
			{
				paragraphFormat.SetPropertyValue(9, currentFormat.BottomMargin);
				paragraphFormat.SpaceAfterAuto = false;
			}
		}
		else if (!node.LocalName.Equals("td", StringComparison.OrdinalIgnoreCase) && !isPreTag)
		{
			paragraphFormat.SpaceAfterAuto = true;
		}
		if (IsTopMarginNeedToBePreserved(node, currentFormat))
		{
			if (currentFormat.HasValue(28))
			{
				paragraphFormat.SpaceBeforeAuto = currentFormat.BeforeSpaceAuto;
			}
			else
			{
				paragraphFormat.SetPropertyValue(8, currentFormat.TopMargin);
				paragraphFormat.SpaceBeforeAuto = false;
			}
		}
		else if ((!node.LocalName.Equals("td", StringComparison.OrdinalIgnoreCase) && !isPreTag) || node.LocalName.Equals("p", StringComparison.OrdinalIgnoreCase))
		{
			paragraphFormat.SpaceBeforeAuto = true;
		}
		if (currentFormat.HasValue(17))
		{
			paragraphFormat.PageBreakBefore = currentFormat.PageBreakBefore;
			paragraphFormat.KeepFollow = currentFormat.PageBreakBefore;
		}
		if (currentFormat.HasValue(18))
		{
			paragraphFormat.PageBreakAfter = currentFormat.PageBreakAfter;
		}
		if (currentFormat.HasValue(22))
		{
			paragraphFormat.WordWrap = currentFormat.WordWrap;
		}
		if (currentFormat.HasValue(7))
		{
			paragraphFormat.BackColor = currentFormat.BackColor;
		}
		if (currentFormat.Bidi || (!currentFormat.HasValue(31) && m_currParagraph.IsInCell && (m_currParagraph.GetOwnerTable(m_currParagraph) as WTable).TableFormat.Bidi))
		{
			paragraphFormat.Bidi = true;
		}
		UpdateParaFormat(node, paragraphFormat);
		if (m_bIsInBlockquote && !node.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase))
		{
			paragraphFormat.SetPropertyValue(2, CurrentPara.ParagraphFormat.LeftIndent + (float)m_blockquoteLevel * 36f);
		}
		if (!node.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.LocalName.Equals("#text", StringComparison.OrdinalIgnoreCase) || childNode.LocalName.Equals("strong", StringComparison.OrdinalIgnoreCase))
			{
				break;
			}
			if (childNode.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase))
			{
				paragraphFormat.SetPropertyValue(2, CurrentPara.ParagraphFormat.LeftIndent + 36f);
				break;
			}
		}
	}

	private float AdjustLeftIndentForList(XmlNode node, TextFormat format)
	{
		if (!IsDefinedInline(node, "margin-left"))
		{
			format.LeftMargin = 0f;
		}
		return format.LeftMargin + ((m_listLeftIndentStack.Count > 0) ? m_listLeftIndentStack.Peek() : 0f);
	}

	private bool IsBottomMarginNeedToBePreserved(XmlNode node, TextFormat format)
	{
		if (format.HasKey(13))
		{
			if (IsDefinedInline(node, "margin-bottom") || IsDefinedInline(node, "margin") || IsDefinedInline(node, "padding") || IsDefinedInline(node, "padding-bottom"))
			{
				return true;
			}
			if (!node.ParentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) || !IsLastNode(node))
			{
				return false;
			}
			XmlNode parentNode = node.ParentNode;
			while (parentNode != null && parentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase))
			{
				if (IsDefinedInline(parentNode, "margin-bottom") || IsDefinedInline(parentNode, "margin") || IsDefinedInline(parentNode, "padding") || IsDefinedInline(parentNode, "padding-bottom"))
				{
					return true;
				}
				if (parentNode.ParentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) && IsLastNode(parentNode))
				{
					parentNode = parentNode.ParentNode;
					continue;
				}
				return false;
			}
		}
		return false;
	}

	private bool IsTopMarginNeedToBePreserved(XmlNode node, TextFormat format)
	{
		if (format.HasKey(11))
		{
			if (IsDefinedInline(node, "margin-top") || IsDefinedInline(node, "margin") || IsDefinedInline(node, "padding") || IsDefinedInline(node, "padding-top"))
			{
				return true;
			}
			if (!node.ParentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) || !IsFirstNode(node))
			{
				return false;
			}
			XmlNode parentNode = node.ParentNode;
			while (parentNode != null && parentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase))
			{
				if (IsDefinedInline(parentNode, "margin-top") || IsDefinedInline(parentNode, "margin") || IsDefinedInline(parentNode, "padding") || IsDefinedInline(parentNode, "padding-top"))
				{
					return true;
				}
				if (parentNode.ParentNode.LocalName.Equals("div", StringComparison.OrdinalIgnoreCase) && IsFirstNode(parentNode))
				{
					parentNode = parentNode.ParentNode;
					continue;
				}
				return false;
			}
		}
		return false;
	}

	private bool IsLastNode(XmlNode node)
	{
		while (true)
		{
			if (node.NextSibling == null)
			{
				return true;
			}
			if (node.NextSibling.NodeType == XmlNodeType.Whitespace || node.NextSibling.NodeType == XmlNodeType.SignificantWhitespace)
			{
				node = node.NextSibling;
				continue;
			}
			if (node.NextSibling.NodeType == XmlNodeType.Text || !IsEmptyNode(node.NextSibling))
			{
				break;
			}
			node = node.NextSibling;
		}
		return false;
	}

	private bool IsDefinedInline(XmlNode node, string attName)
	{
		_ = string.Empty;
		if (node.Attributes != null && node.Attributes.Count > 0)
		{
			foreach (XmlAttribute attribute in node.Attributes)
			{
				if (attribute.LocalName.Equals("style", StringComparison.OrdinalIgnoreCase))
				{
					string[] array = GetAttributeValue(node, "style").Split(';', ':');
					int i = 0;
					for (int num = array.Length; i < num - 1; i += 2)
					{
						if (array[i].ToLower().Trim() == attName)
						{
							return true;
						}
					}
				}
				else if (attribute.LocalName.Equals(attName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ApplyListFormatting(WParagraphFormat pformat, TextFormat format, XmlNode node)
	{
		if (format.NumBulleted && node.Name.ToUpper() != "LH" && node.Name.Equals("li", StringComparison.OrdinalIgnoreCase) && node.ParentNode.Name.Equals("ol", StringComparison.OrdinalIgnoreCase))
		{
			bool flag = false;
			foreach (XmlAttribute attribute in node.ParentNode.Attributes)
			{
				if (attribute.Name.Equals("type", StringComparison.OrdinalIgnoreCase))
				{
					BuildListStyle(GetListPatternType(attribute.Value), node);
					flag = true;
				}
				else if (attribute.Name.Equals("style", StringComparison.OrdinalIgnoreCase) && IsDefinedInline(node.ParentNode, "list-style-type"))
				{
					string styleAttributeValue = GetStyleAttributeValue(attribute.Value.ToLower(), "list-style-type");
					ListPatternType listPatternType = GetListPatternType(styleAttributeValue);
					if (listPatternType != ListPatternType.None)
					{
						BuildListStyle(listPatternType, node);
					}
					flag = true;
				}
				else if (pformat.Document.HTMLImportSettings != null && pformat.Document.HTMLImportSettings.IsConsiderListStyleAttribute && attribute.Name.Equals("style", StringComparison.OrdinalIgnoreCase) && attribute.Value.Equals("list-style:none", StringComparison.OrdinalIgnoreCase))
				{
					lastSkippedLevelNo = m_curListLevel;
					isLastLevelSkipped = true;
					flag = true;
				}
			}
			if (!flag)
			{
				BuildListStyle(ListPatternType.Arabic, node);
			}
		}
		else if (node.ParentNode.Name.Equals("ul", StringComparison.OrdinalIgnoreCase) && format.DefBulleted && node.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase) && !node.LocalName.Equals("lh", StringComparison.OrdinalIgnoreCase))
		{
			bool flag2 = false;
			foreach (XmlAttribute attribute2 in node.ParentNode.Attributes)
			{
				if (attribute2.Name.Equals("style", StringComparison.OrdinalIgnoreCase) && IsDefinedInline(node.ParentNode, "list-style-image"))
				{
					BuildListStyle(ListPatternType.Bullet, node);
					string styleAttributeValue2 = GetStyleAttributeValue(attribute2.Value.ToLower(), "list-style-image");
					string src = styleAttributeValue2.Replace("url('", string.Empty).Replace("')", string.Empty);
					if (!string.IsNullOrEmpty(styleAttributeValue2))
					{
						WPicture wPicture = new WPicture(m_bodyItems.Document);
						GetImage(src, wPicture);
						m_currParagraph.ListFormat.CurrentListLevel.PicBullet = wPicture;
					}
					flag2 = true;
				}
				else if (pformat.Document.HTMLImportSettings != null && pformat.Document.HTMLImportSettings.IsConsiderListStyleAttribute && attribute2.Name.Equals("style", StringComparison.OrdinalIgnoreCase) && (attribute2.Value.Equals("list-style:none", StringComparison.OrdinalIgnoreCase) || attribute2.Value.Equals("list-style-type:none", StringComparison.OrdinalIgnoreCase)))
				{
					lastSkippedLevelNo = m_curListLevel;
					isLastLevelSkipped = true;
					flag2 = true;
				}
			}
			if (!flag2)
			{
				BuildListStyle(ListPatternType.Bullet, node);
			}
		}
		else if (node.Name.ToUpper() == "LH")
		{
			pformat.SetPropertyValue(2, 35f);
		}
	}

	private ListPatternType GetListPatternType(string attrValue)
	{
		if (attrValue != null)
		{
			int length = attrValue.Length;
			if (length <= 4)
			{
				if (length != 1)
				{
					if (length == 4 && attrValue == "none")
					{
						return ListPatternType.None;
					}
				}
				else
				{
					char c = attrValue[0];
					if ((uint)c <= 73u)
					{
						if (c == 'A')
						{
							goto IL_00dd;
						}
						if (c == 'I')
						{
							goto IL_00e1;
						}
					}
					else
					{
						if (c == 'a')
						{
							goto IL_00db;
						}
						if (c == 'i')
						{
							goto IL_00df;
						}
					}
				}
			}
			else if (length != 7)
			{
				if (length != 11)
				{
					if (length == 20 && attrValue == "decimal-leading-zero")
					{
						goto IL_00e3;
					}
				}
				else
				{
					char c = attrValue[0];
					if (c != 'l')
					{
						if (c == 'u')
						{
							if (attrValue == "upper-alpha")
							{
								goto IL_00dd;
							}
							if (attrValue == "upper-roman")
							{
								goto IL_00e1;
							}
						}
					}
					else
					{
						if (attrValue == "lower-alpha")
						{
							goto IL_00db;
						}
						if (attrValue == "lower-roman")
						{
							goto IL_00df;
						}
					}
				}
			}
			else if (attrValue == "decimal")
			{
				goto IL_00e3;
			}
		}
		return ListPatternType.Arabic;
		IL_00e3:
		return ListPatternType.Arabic;
		IL_00e1:
		return ListPatternType.UpRoman;
		IL_00df:
		return ListPatternType.LowRoman;
		IL_00db:
		return ListPatternType.LowLetter;
		IL_00dd:
		return ListPatternType.UpLetter;
	}

	private void ApplyParagraphBorder(WParagraphFormat pformat, TextFormat format)
	{
		if (format.Borders.AllStyle != 0)
		{
			Border bottom = pformat.Borders.Bottom;
			Border top = pformat.Borders.Top;
			Border left = pformat.Borders.Left;
			BorderStyle borderStyle = (pformat.Borders.Right.BorderType = format.Borders.AllStyle);
			BorderStyle borderStyle3 = (left.BorderType = borderStyle);
			BorderStyle borderType = (top.BorderType = borderStyle3);
			bottom.BorderType = borderType;
			pformat.Borders.Left.Space = format.Borders.LeftSpace;
			pformat.Borders.Right.Space = format.Borders.RightSpace;
			pformat.Borders.Bottom.Space = format.Borders.BottomSpace;
			pformat.Borders.Top.Space = format.Borders.TopSpace;
			if (format.Borders.AllColor != Color.Empty)
			{
				Border bottom2 = pformat.Borders.Bottom;
				Border top2 = pformat.Borders.Top;
				Border left2 = pformat.Borders.Left;
				Color color = (pformat.Borders.Right.Color = format.Borders.AllColor);
				Color color3 = (left2.Color = color);
				Color color5 = (top2.Color = color3);
				bottom2.Color = color5;
			}
			if (format.Borders.AllWidth != -1f)
			{
				Border bottom3 = pformat.Borders.Bottom;
				Border top3 = pformat.Borders.Top;
				Border left3 = pformat.Borders.Left;
				float num = (pformat.Borders.Right.LineWidth = format.Borders.AllWidth);
				float num3 = (left3.LineWidth = num);
				float lineWidth = (top3.LineWidth = num3);
				bottom3.LineWidth = lineWidth;
			}
		}
		if (format.Borders.BottomStyle != 0)
		{
			pformat.Borders.Bottom.BorderType = format.Borders.BottomStyle;
			pformat.Borders.Bottom.LineWidth = format.Borders.BottomWidth;
			pformat.Borders.Bottom.Color = format.Borders.BottomColor;
			pformat.Borders.Bottom.Space = format.Borders.BottomSpace;
		}
		if (format.Borders.TopStyle != 0)
		{
			pformat.Borders.Top.BorderType = format.Borders.TopStyle;
			pformat.Borders.Top.LineWidth = format.Borders.TopWidth;
			pformat.Borders.Top.Color = format.Borders.TopColor;
			pformat.Borders.Top.Space = format.Borders.TopSpace;
		}
		if (format.Borders.LeftStyle != 0)
		{
			pformat.Borders.Left.BorderType = format.Borders.LeftStyle;
			pformat.Borders.Left.LineWidth = format.Borders.LeftWidth;
			pformat.Borders.Left.Color = format.Borders.LeftColor;
			pformat.Borders.Left.Space = format.Borders.LeftSpace;
		}
		if (format.Borders.RightStyle != 0)
		{
			pformat.Borders.Right.BorderType = format.Borders.RightStyle;
			pformat.Borders.Right.LineWidth = format.Borders.RightWidth;
			pformat.Borders.Right.Color = format.Borders.RightColor;
			pformat.Borders.Right.Space = format.Borders.RightSpace;
		}
		if (format.Borders.TopWidth > 0f)
		{
			pformat.Borders.Top.LineWidth = format.Borders.TopWidth;
		}
		if (format.Borders.RightWidth > 0f)
		{
			pformat.Borders.Right.LineWidth = format.Borders.RightWidth;
		}
		if (format.Borders.LeftWidth > 0f)
		{
			pformat.Borders.Left.LineWidth = format.Borders.LeftWidth;
		}
		if (format.Borders.BottomWidth > 0f)
		{
			pformat.Borders.Bottom.LineWidth = format.Borders.BottomWidth;
		}
	}

	private void ApplyDivParagraphFormat(XmlNode node)
	{
		if (currDivFormat == null)
		{
			return;
		}
		WParagraphFormat paragraphFormat = m_currParagraph.ParagraphFormat;
		if (currDivFormat.HasValue(7))
		{
			paragraphFormat.BackColor = currDivFormat.BackColor;
		}
		if (currDivFormat.HasValue(12))
		{
			paragraphFormat.SetPropertyValue(2, currDivFormat.LeftMargin);
		}
		if (currDivFormat.HasValue(14))
		{
			paragraphFormat.SetPropertyValue(3, currDivFormat.RightMargin);
		}
		if (currDivFormat.HasValue(10))
		{
			paragraphFormat.HorizontalAlignment = currDivFormat.TextAlign;
		}
		if (IsBottomMarginNeedToBePreserved(node, currDivFormat))
		{
			paragraphFormat.SetPropertyValue(9, currDivFormat.BottomMargin);
		}
		if (IsTopMarginNeedToBePreserved(node, currDivFormat))
		{
			paragraphFormat.SetPropertyValue(8, currDivFormat.TopMargin);
		}
		if (currDivFormat.HasValue(8))
		{
			paragraphFormat.LineSpacingRule = currDivFormat.LineSpacingRule;
			paragraphFormat.SetPropertyValue(52, currDivFormat.LineHeight);
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		foreach (TextFormat item in m_styleStack)
		{
			if (item == null)
			{
				continue;
			}
			if (item.Borders.AllStyle != 0)
			{
				flag = true;
				Border bottom = paragraphFormat.Borders.Bottom;
				Border top = paragraphFormat.Borders.Top;
				Border left = paragraphFormat.Borders.Left;
				BorderStyle borderStyle = (paragraphFormat.Borders.Right.BorderType = item.Borders.AllStyle);
				BorderStyle borderStyle3 = (left.BorderType = borderStyle);
				BorderStyle borderType = (top.BorderType = borderStyle3);
				bottom.BorderType = borderType;
				if (item.Borders.AllColor != Color.Empty)
				{
					Border bottom2 = paragraphFormat.Borders.Bottom;
					Border top2 = paragraphFormat.Borders.Top;
					Border left2 = paragraphFormat.Borders.Left;
					Color color = (paragraphFormat.Borders.Right.Color = item.Borders.AllColor);
					Color color3 = (left2.Color = color);
					Color color5 = (top2.Color = color3);
					bottom2.Color = color5;
				}
				if (item.Borders.AllWidth != -1f)
				{
					Border bottom3 = paragraphFormat.Borders.Bottom;
					Border top3 = paragraphFormat.Borders.Top;
					Border left3 = paragraphFormat.Borders.Left;
					float num = (paragraphFormat.Borders.Right.LineWidth = item.Borders.AllWidth);
					float num3 = (left3.LineWidth = num);
					float lineWidth = (top3.LineWidth = num3);
					bottom3.LineWidth = lineWidth;
				}
			}
			if (item.Borders.BottomStyle != 0 && !flag2)
			{
				flag2 = true;
				paragraphFormat.Borders.Bottom.BorderType = item.Borders.BottomStyle;
				paragraphFormat.Borders.Bottom.LineWidth = item.Borders.BottomWidth;
				paragraphFormat.Borders.Bottom.Color = item.Borders.BottomColor;
			}
			if (item.Borders.TopStyle != 0 && !flag3)
			{
				flag3 = true;
				paragraphFormat.Borders.Top.BorderType = item.Borders.TopStyle;
				paragraphFormat.Borders.Top.LineWidth = item.Borders.TopWidth;
				paragraphFormat.Borders.Top.Color = item.Borders.TopColor;
			}
			if (item.Borders.LeftStyle != 0 && !flag4)
			{
				flag4 = true;
				paragraphFormat.Borders.Left.BorderType = item.Borders.LeftStyle;
				paragraphFormat.Borders.Left.LineWidth = item.Borders.LeftWidth;
				paragraphFormat.Borders.Left.Color = item.Borders.LeftColor;
			}
			if (item.Borders.RightStyle != 0 && !flag5)
			{
				flag5 = true;
				paragraphFormat.Borders.Right.BorderType = item.Borders.RightStyle;
				paragraphFormat.Borders.Right.LineWidth = item.Borders.RightWidth;
				paragraphFormat.Borders.Right.Color = item.Borders.RightColor;
			}
			if (flag || (flag2 && flag3 && flag4 && flag5))
			{
				break;
			}
		}
	}

	private void ApplyTextFormatting(WCharacterFormat charFormat)
	{
		if (!m_bIsInDiv || !m_currParagraph.IsInCell)
		{
			ApplyDivCharacterFormat(charFormat);
		}
		TextFormat currentFormat = CurrentFormat;
		if (currentFormat.HasValue(2))
		{
			charFormat.Bold = currentFormat.Bold;
			charFormat.BoldBidi = currentFormat.Bold;
		}
		if (currentFormat.HasValue(29))
		{
			charFormat.LocaleIdASCII = currentFormat.LocaleIdASCII;
		}
		if (currentFormat.HasValue(4))
		{
			charFormat.Italic = currentFormat.Italic;
			charFormat.ItalicBidi = currentFormat.Italic;
		}
		if (currentFormat.HasValue(3) && currentFormat.Underline)
		{
			charFormat.UnderlineStyle = UnderlineStyle.Single;
		}
		if (currentFormat.HasValue(5))
		{
			charFormat.Strikeout = currentFormat.Strike;
		}
		if (currentFormat.HasValue(6) && currentFormat.FontColor != Color.Empty)
		{
			charFormat.TextColor = currentFormat.FontColor;
		}
		if (currentFormat.HasValue(1) && currentFormat.FontFamily.Length > 0)
		{
			char[] trimChars = new char[2] { '\'', '"' };
			charFormat.FontName = currentFormat.FontFamily.Trim(trimChars);
			charFormat.FontNameAscii = charFormat.FontName;
			charFormat.FontNameBidi = charFormat.FontName;
			charFormat.FontNameFarEast = charFormat.FontName;
			charFormat.FontNameNonFarEast = charFormat.FontName;
		}
		if (currentFormat.HasValue(0))
		{
			charFormat.SetPropertyValue(3, currentFormat.FontSize);
		}
		else if (CurrentPara.ParaStyle != null && CurrentPara.ParaStyle.CharacterFormat.HasValue(3))
		{
			charFormat.SetPropertyValue(3, CurrentPara.ParaStyle.CharacterFormat.FontSize);
		}
		else if (!charFormat.HasValue(3) && m_bodyItems.Document.Styles.FindByName("Normal (Web)") is WParagraphStyle wParagraphStyle)
		{
			charFormat.SetPropertyValue(3, (wParagraphStyle.CharacterFormat.FontSize != 12f) ? wParagraphStyle.CharacterFormat.FontSize : 12f);
		}
		if (currentFormat.HasValue(7) && currentFormat.BackColor != Color.Empty)
		{
			charFormat.TextBackgroundColor = currentFormat.BackColor;
		}
		if (currentFormat.SubSuperScript != 0)
		{
			charFormat.SubSuperScript = currentFormat.SubSuperScript;
		}
		if (currentFormat.HasValue(19))
		{
			charFormat.SetPropertyValue(18, currentFormat.CharacterSpacing);
		}
		if (currentFormat.HasValue(20))
		{
			charFormat.AllCaps = currentFormat.AllCaps;
		}
		if (currentFormat.HasValue(24))
		{
			charFormat.SmallCaps = currentFormat.SmallCaps;
		}
		if (currentFormat.HasValue(23))
		{
			charFormat.Hidden = currentFormat.Hidden;
		}
		else if (CurrentPara.IsInCell)
		{
			WTableCell wTableCell = CurrentPara.GetOwnerEntity() as WTableCell;
			if (wTableCell.CellFormat.Hidden)
			{
				charFormat.Hidden = true;
			}
			else if (wTableCell.OwnerRow.RowFormat.HasValue(121))
			{
				charFormat.Hidden = wTableCell.OwnerRow.RowFormat.Hidden;
			}
			else if (wTableCell.OwnerRow.OwnerTable.TableFormat.HasValue(121))
			{
				charFormat.Hidden = wTableCell.OwnerRow.OwnerTable.TableFormat.Hidden;
			}
		}
		if (currentFormat.Borders.TopStyle != 0)
		{
			charFormat.Border.BorderType = currentFormat.Borders.TopStyle;
			charFormat.Border.Color = currentFormat.Borders.TopColor;
			charFormat.Border.LineWidth = currentFormat.Borders.TopWidth;
			charFormat.Border.Space = currentFormat.Borders.TopSpace;
		}
		if (charFormat.Document != null && charFormat.Document.HTMLImportSettings != null && charFormat.Document.HTMLImportSettings.AllowUnsupportedCSSProperties && currentFormat.HasValue(30))
		{
			charFormat.SetScalingValue(currentFormat.Scaling);
		}
	}

	private void ApplyDivCharacterFormat(WCharacterFormat charFormat)
	{
		if (currDivFormat != null)
		{
			if (currDivFormat.FontSize > 0f)
			{
				charFormat.SetPropertyValue(3, currDivFormat.FontSize);
			}
			if (currDivFormat.FontFamily.Length > 0)
			{
				charFormat.FontName = currDivFormat.FontFamily;
			}
			if (currDivFormat.HasValue(6) && currDivFormat.FontColor != Color.Empty)
			{
				charFormat.TextColor = currDivFormat.FontColor;
			}
			if (currDivFormat.HasValue(2))
			{
				charFormat.Bold = currDivFormat.Bold;
				charFormat.BoldBidi = currDivFormat.Bold;
			}
			if (currDivFormat.HasValue(3))
			{
				charFormat.UnderlineStyle = (currDivFormat.Underline ? UnderlineStyle.Single : UnderlineStyle.None);
			}
			if (currDivFormat.HasValue(5))
			{
				charFormat.Strikeout = currDivFormat.Strike;
			}
			if (currDivFormat.HasValue(4))
			{
				charFormat.Italic = currDivFormat.Italic;
				charFormat.ItalicBidi = currDivFormat.Italic;
			}
			if (currDivFormat.HasValue(16))
			{
				charFormat.SubSuperScript = currDivFormat.SubSuperScript;
			}
			if (currDivFormat.HasValue(31))
			{
				charFormat.Bidi = currDivFormat.Bidi;
			}
		}
	}

	private TextFormat EnsureStyle(XmlNode node)
	{
		bool flag = ParseStyle(node);
		if (!flag && !node.Name.Equals("pre", StringComparison.OrdinalIgnoreCase))
		{
			return AddStyle();
		}
		if (!flag && node.Name.Equals("pre", StringComparison.OrdinalIgnoreCase))
		{
			TextFormat textFormat = ((m_styleStack.Count > 0) ? m_styleStack.Peek().Clone() : new TextFormat());
			if (textFormat.HasKey(0))
			{
				textFormat.m_propertiesHash.Remove(0);
			}
			if (textFormat.HasKey(1))
			{
				textFormat.m_propertiesHash.Remove(1);
			}
			m_styleStack.Push(textFormat);
			return textFormat;
		}
		return CurrentFormat;
	}

	internal string ExtractValue(string value)
	{
		float num = float.MinValue;
		float result = 0f;
		if (value.EndsWith("pt"))
		{
			return value.Replace("pt", string.Empty);
		}
		if (value.EndsWith("%"))
		{
			float.TryParse(value.Replace("%", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			float num2 = ((m_currTable != null) ? m_currTable.GetTableClientWidth(m_currTable.GetOwnerWidth()) : ((m_currParagraph == null) ? 0f : m_currParagraph.Document.Sections[0].PageSetup.ClientWidth));
			return (result / 100f * num2).ToString(CultureInfo.InvariantCulture);
		}
		if (value.EndsWith("em"))
		{
			float.TryParse(value.Replace("em", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			num = result * 12f;
		}
		else if (value.EndsWith("in"))
		{
			float.TryParse(value.Replace("in", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			num = PointsConverter.FromInch(result);
		}
		else if (value.EndsWith("cm"))
		{
			float.TryParse(value.Replace("cm", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			num = PointsConverter.FromCm(result);
		}
		else if (value.EndsWith("pc"))
		{
			float.TryParse(value.Replace("pc", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			num = result * 12f;
		}
		else if (value.EndsWith("mm"))
		{
			float.TryParse(value.Replace("mm", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			num = (float)UnitsConvertor.Instance.ConvertUnits(result, PrintUnits.Millimeter, PrintUnits.Point);
		}
		else
		{
			float.TryParse(value.Replace("px", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			num = (float)((double)result * 0.75);
		}
		return num.ToString(CultureInfo.InvariantCulture);
	}

	private bool ParseStyle(XmlNode node)
	{
		string attributeValue = GetAttributeValue(node, "style");
		string value = GetAttributeValue(node, "lang");
		string attributeValue2 = GetAttributeValue(node, "dir");
		bool flag = !string.IsNullOrEmpty(value) && DocxParser.IsEnumDefined(ref value);
		_ = new string[9] { "dashed", "dotted", "double", "groove", "inset", "outset", "ridge", "solid", "hidden" };
		if (attributeValue.Length != 0 || CSSStyle != null || flag || !string.IsNullOrEmpty(attributeValue2))
		{
			TextFormat textFormat = AddStyle();
			TableBorders borders = textFormat.Borders;
			textFormat.Borders = new TableBorders();
			string[] array = attributeValue.Split(';', ':');
			int i = 0;
			for (int num = array.Length; i < num - 1; i += 2)
			{
				char[] trimChars = new char[2] { '\'', '"' };
				string paramName = array[i].ToLower().Trim();
				string text = array[i + 1].Trim();
				text = text.Trim(trimChars);
				GetFormat(textFormat, paramName, text, node);
			}
			if (attributeValue == string.Empty && node.Name == "b" && IsDefaultBorderFormat(textFormat.Borders) && !IsDefaultBorderFormat(borders))
			{
				textFormat.Borders = borders;
			}
			if (flag)
			{
				textFormat.LocaleIdASCII = (short)(LocaleIDs)Enum.Parse(typeof(LocaleIDs), value, ignoreCase: true);
			}
			if (attributeValue2.Equals("rtl", StringComparison.OrdinalIgnoreCase))
			{
				textFormat.Bidi = true;
			}
			else if (attributeValue2.Equals("ltr", StringComparison.OrdinalIgnoreCase))
			{
				textFormat.Bidi = false;
			}
			if (node.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase) && textFormat.ListNumberWidth != 0f && textFormat.LeftMargin != 0f && textFormat.PaddingLeft != 0f)
			{
				textFormat.TextIndent = 0f - (textFormat.PaddingLeft + textFormat.ListNumberWidth);
				textFormat.LeftMargin = textFormat.LeftMargin - textFormat.TextIndent - textFormat.ListNumberWidth;
			}
			FindCSSstyleItem(node, textFormat);
			return true;
		}
		return false;
	}

	private bool IsDefaultBorderFormat(TableBorders borders)
	{
		if (borders.AllColor == Color.Empty && borders.AllStyle == BorderStyle.None && borders.AllWidth == -1f && borders.BottomColor == Color.Empty && borders.BottomSpace == 0f && borders.BottomStyle == BorderStyle.None && borders.BottomWidth == -1f && borders.LeftColor == Color.Empty && borders.LeftSpace == 0f && borders.LeftStyle == BorderStyle.None && borders.LeftWidth == -1f && borders.RightColor == Color.Empty && borders.RightSpace == 0f && borders.RightStyle == BorderStyle.None && borders.RightWidth == -1f && borders.TopColor == Color.Empty && borders.TopSpace == 0f && borders.TopStyle == BorderStyle.None && borders.TopWidth == -1f)
		{
			return true;
		}
		return false;
	}

	private void GetFormat(TextFormat format, string paramName, string paramValue, XmlNode node)
	{
		switch (paramName)
		{
		default:
			paramValue = paramValue.ToLower();
			break;
		case "mso-field-code":
		case "font-family":
		case "transform":
			break;
		}
		char[] separator = new char[1] { ' ' };
		if (paramValue.ToLower().Contains("inherit") && !paramName.Equals("page-break-before", StringComparison.OrdinalIgnoreCase) && !paramName.Equals("page-break-after", StringComparison.OrdinalIgnoreCase) && paramName != "page-break-inside")
		{
			return;
		}
		if (node.LocalName.Equals("span", StringComparison.OrdinalIgnoreCase) && isPreserveBreakForInvalidStyles)
		{
			isPreserveBreakForInvalidStyles = false;
		}
		try
		{
			if (paramName == null)
			{
				return;
			}
			string[] array;
			string[] array3;
			string name;
			string[] array4;
			switch (paramName.Length)
			{
			case 7:
				switch (paramName[0])
				{
				case 'd':
					if (!(paramName == "display") || !(node.LocalName != "label"))
					{
						break;
					}
					if (!(paramValue == "none"))
					{
						if (paramValue == "inline-block")
						{
							format.IsInlineBlock = true;
							break;
						}
						format.Hidden = false;
						isPreserveBreakForInvalidStyles = true;
					}
					else
					{
						format.Hidden = true;
					}
					break;
				case 'p':
				{
					if (!(paramName == "padding"))
					{
						break;
					}
					string[] array2 = paramValue.Split(separator);
					switch (array2.Length)
					{
					case 1:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							format.Borders.LeftSpace = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						format.Borders.TopSpace = (format.Borders.BottomSpace = (format.Borders.RightSpace = format.Borders.LeftSpace));
						break;
					case 2:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							format.Borders.TopSpace = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						format.Borders.BottomSpace = format.Borders.TopSpace;
						if (array2[1] != "initial" && array2[1] != "inherit")
						{
							format.Borders.LeftSpace = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						format.Borders.RightSpace = format.Borders.LeftSpace;
						break;
					case 3:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							format.Borders.TopSpace = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "initial" && array2[1] != "inherit")
						{
							format.Borders.LeftSpace = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						format.Borders.RightSpace = format.Borders.LeftSpace;
						if (array2[2] != "initial" && array2[2] != "inherit")
						{
							format.Borders.BottomSpace = Convert.ToSingle(ExtractValue(array2[2]), CultureInfo.InvariantCulture);
						}
						break;
					case 4:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							format.Borders.TopSpace = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "initial" && array2[1] != "inherit")
						{
							format.Borders.RightSpace = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						if (array2[2] != "initial" && array2[2] != "inherit")
						{
							format.Borders.BottomSpace = Convert.ToSingle(ExtractValue(array2[2]), CultureInfo.InvariantCulture);
						}
						if (array2[3] != "initial" && array2[3] != "inherit")
						{
							format.Borders.LeftSpace = Convert.ToSingle(ExtractValue(array2[3]), CultureInfo.InvariantCulture);
						}
						break;
					}
					break;
				}
				}
				break;
			case 11:
				switch (paramName[0])
				{
				case 'w':
					if (paramName == "white-space")
					{
						switch (paramValue)
						{
						case "pre-wrap":
						case "pre":
							format.IsPreserveWhiteSpace = true;
							break;
						case "pre-line":
						case "no-wrap":
						case "inherit":
						case "initial":
						case "normal":
							format.IsPreserveWhiteSpace = false;
							break;
						default:
							format.IsPreserveWhiteSpace = false;
							isPreserveBreakForInvalidStyles = true;
							break;
						}
					}
					break;
				case 'f':
					if (!(paramName == "font-family"))
					{
						if (paramName == "font-weight")
						{
							int result2;
							bool flag = int.TryParse(paramValue, out result2);
							if (paramValue == "normal" || paramValue == "lighter" || (flag && (result2 < 550 || result2 > 1000)))
							{
								format.Bold = false;
								break;
							}
							format.Bold = true;
							isPreserveBreakForInvalidStyles = true;
						}
					}
					else
					{
						format.FontFamily = GetFontName(paramValue);
					}
					break;
				case 'l':
					if (paramName == "line-height")
					{
						ParseLineHeight(paramValue, format, ref isPreserveBreakForInvalidStyles);
					}
					break;
				case '-':
					if (paramName == "-sf-listtab" && paramValue == "yes")
					{
						format.IsListTab = true;
					}
					break;
				case 'm':
					if (!(paramName == "margin-left"))
					{
						if (paramName == "mso-element")
						{
							switch (paramValue)
							{
							case "field-begin":
								IsPreviousItemFieldStart = true;
								break;
							case "field-separator":
								ParseFieldSeparator();
								break;
							case "field-end":
								ParseFieldEnd();
								break;
							}
						}
					}
					else
					{
						if (!paramValue.Equals("auto", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("initial", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
						{
							format.LeftMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
						}
						if (node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase))
						{
							UpdateListLeftIndentStack(format.LeftMargin, isInlineLeftIndent: true);
						}
					}
					break;
				case 't':
					if (paramName == "text-indent" && !paramValue.Equals("auto", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("initial", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("inherit", StringComparison.OrdinalIgnoreCase) && !node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase) && !node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase))
					{
						format.TextIndent = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					break;
				case 'b':
					if (paramName == "border-left")
					{
						ParseParagraphBorder(paramValue, ref format.Borders.LeftColor, ref format.Borders.LeftWidth, ref format.Borders.LeftStyle);
					}
					break;
				case 'p':
					if (paramName == "padding-top" && paramValue != "initial" && paramValue != "inherit")
					{
						format.TopMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					break;
				}
				break;
			case 14:
				switch (paramName[0])
				{
				case 't':
					if (!(paramName == "text-transform"))
					{
						break;
					}
					if (!(paramValue == "uppercase"))
					{
						if (paramValue == "none")
						{
							format.AllCaps = false;
						}
						else
						{
							isPreserveBreakForInvalidStyles = true;
						}
					}
					else
					{
						format.AllCaps = true;
					}
					break;
				case 'l':
					if (paramName == "letter-spacing")
					{
						if (paramValue == "normal")
						{
							format.CharacterSpacing = 0f;
						}
						else
						{
							format.CharacterSpacing = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
						}
					}
					break;
				case 'p':
					if (paramName == "padding-bottom" && paramValue != "initial" && paramValue != "inherit")
					{
						format.BottomMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					break;
				case 'm':
					if (paramName == "mso-field-code")
					{
						string fieldCode = paramValue.Trim().Replace("\"", string.Empty);
						FieldType fieldType = FieldTypeDefiner.GetFieldType(fieldCode);
						ParseFieldCode(fieldType, fieldCode);
					}
					break;
				case 'v':
					if (paramName == "vertical-align")
					{
						if (paramValue == "super")
						{
							format.SubSuperScript = SubSuperScript.SuperScript;
						}
						else if (paramValue == "sub")
						{
							format.SubSuperScript = SubSuperScript.SubScript;
						}
						else
						{
							format.SubSuperScript = SubSuperScript.None;
						}
					}
					break;
				}
				break;
			case 10:
				switch (paramName[0])
				{
				default:
					return;
				case 'f':
					if (paramName == "font-style")
					{
						switch (paramValue)
						{
						case "italic":
						case "oblique":
							format.Italic = true;
							break;
						case "strike":
							format.Strike = true;
							break;
						case "normal":
							format.Italic = false;
							break;
						default:
							isPreserveBreakForInvalidStyles = true;
							break;
						}
					}
					return;
				case 't':
					if (paramName == "text-align")
					{
						format.TextAlign = GetHorizontalAlignment(paramValue);
					}
					return;
				case 'b':
					break;
				case 'm':
					if (paramName == "margin-top")
					{
						if (!paramValue.Equals("auto", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("initial", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
						{
							format.TopMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
						}
						else
						{
							format.TopMargin = -1f;
						}
					}
					return;
				case 'w':
					if (paramName == "word-break" && paramValue == "break-all")
					{
						format.WordWrap = false;
					}
					return;
				}
				if (!(paramName == "background"))
				{
					if (paramName == "border-top")
					{
						ParseParagraphBorder(paramValue, ref format.Borders.TopColor, ref format.Borders.TopWidth, ref format.Borders.TopStyle);
					}
					break;
				}
				goto IL_1191;
			case 9:
				switch (paramName[1])
				{
				case 'o':
					if (paramName == "font-size")
					{
						if (paramValue == "smaller")
						{
							format.FontSize = 10f;
						}
						else
						{
							format.FontSize = (float)ConvertSize(paramValue, format.FontSize);
						}
					}
					break;
				case 'a':
				{
					if (!(paramName == "tab-stops"))
					{
						break;
					}
					string[] separator2 = new string[1] { "pt" };
					string[] array2 = paramValue.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
					for (int j = 0; j < array2.Length; j++)
					{
						bool flag2 = false;
						string[] array5 = array2[j].TrimStart().Split(separator, StringSplitOptions.None);
						float result4 = 0f;
						TabJustification justification = TabJustification.Left;
						TabLeader leader = TabLeader.NoLeader;
						for (int k = 0; k < array5.Length; k++)
						{
							if (array5[k] == "center" || array5[k] == "right" || array5[k] == "decimal" || array5[k] == "bar")
							{
								justification = GetTabjustification(array5[k]);
								continue;
							}
							if (array5[k] == "dotted" || array5[k] == "dashed" || array5[k] == "heavy")
							{
								leader = GetTabLeader(array5[k]);
								continue;
							}
							if (k != array5.Length - 1 || !float.TryParse(array5[k], NumberStyles.Number, CultureInfo.InvariantCulture, out result4))
							{
								break;
							}
							flag2 = true;
						}
						if (!flag2)
						{
							CurrentPara.ParagraphFormat.Tabs.Clear();
							break;
						}
						CurrentPara.ParagraphFormat.Tabs.RemoveByTabPosition(result4);
						CurrentPara.ParagraphFormat.Tabs.AddTab(result4, justification, leader);
					}
					break;
				}
				case 'r':
				{
					if (!(paramName == "transform") || !StartsWithExt(paramValue, "scaleX("))
					{
						break;
					}
					int num5 = paramValue.IndexOf('(');
					int num6 = paramValue.IndexOf(')');
					if (num5 != -1 && num6 != -1)
					{
						string s = paramValue.Substring(num5 + 1, num6 - num5 - 1);
						float result3 = 0f;
						if (float.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out result3))
						{
							format.Scaling = result3 * 100f;
						}
					}
					break;
				}
				}
				break;
			case 17:
				switch (paramName[12])
				{
				case 'a':
					if (paramName == "-sf-tabstop-align")
					{
						switch (paramValue.ToLower())
						{
						case "left":
							format.TabJustification = TabJustification.Left;
							break;
						case "centered":
							format.TabJustification = TabJustification.Centered;
							break;
						case "right":
							format.TabJustification = TabJustification.Right;
							break;
						case "decimal":
							format.TabJustification = TabJustification.Decimal;
							break;
						case "bar":
							format.TabJustification = TabJustification.Bar;
							break;
						case "list":
							format.TabJustification = TabJustification.List;
							break;
						}
					}
					break;
				case 'c':
					if (paramName == "border-left-color")
					{
						format.Borders.LeftColor = GetColor(paramValue);
					}
					break;
				case 'w':
					if (paramName == "border-left-width")
					{
						format.Borders.LeftWidth = CalculateBorderWidth(paramValue);
					}
					break;
				case 's':
					if (paramName == "border-left-style")
					{
						format.Borders.LeftStyle = ToBorderType(paramValue);
					}
					break;
				case 'e':
					if (!(paramName == "page-break-before"))
					{
						break;
					}
					if (node.LocalName.Equals("br", StringComparison.OrdinalIgnoreCase))
					{
						Break break3 = null;
						if (paramValue == "always")
						{
							break3 = CurrentPara.AppendBreak(BreakType.PageBreak);
							break;
						}
						break3 = CurrentPara.AppendBreak(BreakType.LineBreak);
						if (paramValue == "avoid" || paramValue == "inherit")
						{
							switch (GetAttributeValue(node, "clear"))
							{
							case "all":
							case "left":
							case "right":
								break3.HtmlToDocLayoutInfo.RemoveLineBreak = false;
								break;
							}
						}
					}
					else if (paramValue == "always")
					{
						format.PageBreakBefore = true;
					}
					else if (paramValue == "auto")
					{
						format.PageBreakBefore = false;
					}
					break;
				case 'n':
					if (!(paramName == "page-break-inside"))
					{
						break;
					}
					if (node.LocalName.Equals("br", StringComparison.OrdinalIgnoreCase))
					{
						Break break2 = CurrentPara.AppendBreak(BreakType.LineBreak);
						switch (GetAttributeValue(node, "clear"))
						{
						case "all":
						case "left":
						case "right":
							break2.HtmlToDocLayoutInfo.RemoveLineBreak = false;
							break;
						}
					}
					else if (paramValue == "avoid")
					{
						format.KeepLinesTogether = true;
					}
					break;
				}
				break;
			case 18:
				switch (paramName[13])
				{
				case 'e':
					if (paramName == "-sf-tabstop-leader")
					{
						switch (paramValue.ToLower())
						{
						case "noLeader":
							format.TabLeader = TabLeader.NoLeader;
							break;
						case "dotted":
							format.TabLeader = TabLeader.Dotted;
							break;
						case "hyphenated":
							format.TabLeader = TabLeader.Hyphenated;
							break;
						case "single":
							format.TabLeader = TabLeader.Single;
							break;
						case "heavy":
							format.TabLeader = TabLeader.Heavy;
							break;
						}
					}
					break;
				case 'c':
					if (paramName == "border-right-color")
					{
						format.Borders.RightColor = GetColor(paramValue);
					}
					break;
				case 'w':
					if (paramName == "border-right-width")
					{
						format.Borders.RightWidth = CalculateBorderWidth(paramValue);
					}
					break;
				case 's':
					if (paramName == "border-right-style")
					{
						format.Borders.RightStyle = ToBorderType(paramValue);
					}
					break;
				}
				break;
			case 15:
				switch (paramName[0])
				{
				default:
					return;
				case '-':
					if (paramName == "-sf-tabstop-pos" && (paramValue.EndsWith("pt") || paramValue.EndsWith("px") || paramValue.EndsWith("em") || paramValue.EndsWith("cm") || paramValue.EndsWith("pc")))
					{
						format.TabPosition = float.Parse(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					return;
				case 't':
					break;
				}
				if (!(paramName == "text-decoration"))
				{
					break;
				}
				goto IL_0f70;
			case 5:
				switch (paramName[0])
				{
				case 'w':
					if (paramName == "width" && (paramValue.EndsWith("pt") || paramValue.EndsWith("px") || paramValue.EndsWith("em") || paramValue.EndsWith("cm") || paramValue.EndsWith("pc")))
					{
						format.TabWidth = float.Parse(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					break;
				case 'c':
					if (paramName == "color")
					{
						format.FontColor = GetColor(paramValue);
					}
					break;
				}
				break;
			case 12:
				switch (paramName[9])
				{
				case 'r':
					if (paramName == "mso-spacerun" && paramValue == "yes")
					{
						format.IsNonBreakingSpace = true;
					}
					return;
				case 'g':
					break;
				case 'l':
					goto IL_07c7;
				case 'd':
					goto IL_07dc;
				case 'y':
					goto IL_07f1;
				case 'e':
					goto IL_0806;
				default:
					return;
				}
				if (!(paramName == "margin-right"))
				{
					if (paramName == "border-right")
					{
						ParseParagraphBorder(paramValue, ref format.Borders.RightColor, ref format.Borders.RightWidth, ref format.Borders.RightStyle);
					}
				}
				else if (!paramValue.Equals("auto", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("initial", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
				{
					format.RightMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
				}
				break;
			case 16:
				switch (paramName[2])
				{
				case 'f':
					if (paramName == "-sf-number-width" && (paramValue.EndsWith("pt") || paramValue.EndsWith("px") || paramValue.EndsWith("em") || paramValue.EndsWith("cm") || paramValue.EndsWith("pc")))
					{
						format.ListNumberWidth = float.Parse(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					return;
				case 'c':
					break;
				case 'r':
					switch (paramName)
					{
					case "border-top-color":
						format.Borders.TopColor = GetColor(paramValue);
						break;
					case "border-top-width":
						format.Borders.TopWidth = CalculateBorderWidth(paramValue);
						break;
					case "border-top-style":
						format.Borders.TopStyle = ToBorderType(paramValue);
						break;
					}
					return;
				case 'g':
					if (!(paramName == "page-break-after"))
					{
						return;
					}
					if (node.LocalName.Equals("br", StringComparison.OrdinalIgnoreCase))
					{
						Break @break = null;
						if (paramValue == "always")
						{
							@break = CurrentPara.AppendBreak(BreakType.PageBreak);
							return;
						}
						@break = CurrentPara.AppendBreak(BreakType.LineBreak);
						if (paramValue == "avoid" || paramValue == "inherit")
						{
							switch (GetAttributeValue(node, "clear"))
							{
							case "all":
							case "left":
							case "right":
								@break.HtmlToDocLayoutInfo.RemoveLineBreak = false;
								break;
							}
						}
					}
					else if (paramValue == "auto")
					{
						format.PageBreakAfter = false;
					}
					else if (paramValue == "avoid")
					{
						format.KeepWithNext = true;
					}
					return;
				default:
					return;
				}
				if (!(paramName == "background-color"))
				{
					break;
				}
				goto IL_1191;
			case 20:
				switch (paramName[0])
				{
				default:
					return;
				case 't':
					break;
				case '-':
					if (paramName == "-sf-after-space-auto" && paramValue == "yes")
					{
						format.AfterSpaceAuto = true;
					}
					return;
				}
				if (!(paramName == "text-decoration-line"))
				{
					break;
				}
				goto IL_0f70;
			case 13:
			{
				char c = paramName[10];
				if ((uint)c <= 108u)
				{
					if (c != 'd')
					{
						switch (c)
						{
						default:
							return;
						case 'l':
							break;
						case 'g':
							if (paramName == "padding-right" && paramValue != "initial" && paramValue != "inherit")
							{
								format.RightMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
							}
							return;
						}
						if (!(paramName == "outline-color"))
						{
							break;
						}
						goto IL_18cd;
					}
					if (!(paramName == "outline-width"))
					{
						break;
					}
					goto IL_1a6f;
				}
				switch (c)
				{
				default:
					return;
				case 'u':
				{
					if (paramName == "mso-tab-count" && int.TryParse(paramValue, out var result))
					{
						for (int i = 0; i < result; i++)
						{
							CurrentPara.AppendText("\t");
						}
					}
					return;
				}
				case 't':
					if (!(paramName == "margin-bottom"))
					{
						if (paramName == "border-bottom")
						{
							ParseParagraphBorder(paramValue, ref format.Borders.BottomColor, ref format.Borders.BottomWidth, ref format.Borders.BottomStyle);
						}
					}
					else if (!paramValue.Equals("auto", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("initial", StringComparison.OrdinalIgnoreCase) && !paramValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
					{
						format.BottomMargin = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
					else
					{
						format.BottomMargin = -1f;
					}
					return;
				case 'y':
					break;
				}
				if (!(paramName == "outline-style"))
				{
					break;
				}
				goto IL_1c11;
			}
			case 6:
				switch (paramName[0])
				{
				case 'm':
				{
					if (!(paramName == "margin"))
					{
						break;
					}
					string[] array2 = paramValue.Split(separator);
					switch (array2.Length)
					{
					case 1:
						if (array2[0] != "auto" && array2[0] != "initial" && array2[0] != "inherit")
						{
							float leftMargin = (format.BottomMargin = (format.RightMargin = (format.TopMargin = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture))));
							format.LeftMargin = leftMargin;
						}
						if (node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase))
						{
							UpdateListLeftIndentStack(format.LeftMargin, isInlineLeftIndent: true);
						}
						break;
					case 2:
						if (array2[0] != "auto" && array2[0] != "initial" && array2[0] != "inherit")
						{
							format.TopMargin = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "auto" && array2[1] != "initial" && array2[1] != "inherit")
						{
							format.RightMargin = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						if (node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase))
						{
							UpdateListLeftIndentStack(format.LeftMargin, isInlineLeftIndent: false);
						}
						break;
					case 3:
						if (array2[0] != "auto" && array2[0] != "initial" && array2[0] != "inherit")
						{
							format.TopMargin = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "auto" && array2[1] != "initial" && array2[1] != "inherit")
						{
							format.RightMargin = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						if (array2[2] != "auto" && array2[2] != "initial" && array2[2] != "inherit")
						{
							format.BottomMargin = Convert.ToSingle(ExtractValue(array2[2]), CultureInfo.InvariantCulture);
						}
						if (node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase))
						{
							UpdateListLeftIndentStack(format.LeftMargin, isInlineLeftIndent: false);
						}
						break;
					case 4:
						if (array2[0] != "auto" && array2[0] != "initial" && array2[0] != "inherit")
						{
							format.TopMargin = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "auto" && array2[1] != "initial" && array2[1] != "inherit")
						{
							format.RightMargin = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						if (array2[2] != "auto" && array2[2] != "initial" && array2[2] != "inherit")
						{
							format.BottomMargin = Convert.ToSingle(ExtractValue(array2[2]), CultureInfo.InvariantCulture);
						}
						if (array2[3] != "auto" && array2[3] != "initial" && array2[3] != "inherit")
						{
							format.LeftMargin = Convert.ToSingle(ExtractValue(array2[3]), CultureInfo.InvariantCulture);
						}
						if (node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase) || node.LocalName.Equals("ol", StringComparison.OrdinalIgnoreCase))
						{
							UpdateListLeftIndentStack(format.LeftMargin, isInlineLeftIndent: true);
						}
						break;
					}
					break;
				}
				case 'b':
					if (paramName == "border")
					{
						ParseParagraphBorder(paramValue, ref format.Borders.RightColor, ref format.Borders.RightWidth, ref format.Borders.RightStyle);
						ParseParagraphBorder(paramValue, ref format.Borders.LeftColor, ref format.Borders.LeftWidth, ref format.Borders.LeftStyle);
						ParseParagraphBorder(paramValue, ref format.Borders.BottomColor, ref format.Borders.BottomWidth, ref format.Borders.BottomStyle);
						ParseParagraphBorder(paramValue, ref format.Borders.TopColor, ref format.Borders.TopWidth, ref format.Borders.TopStyle);
					}
					break;
				}
				break;
			case 19:
				switch (paramName[14])
				{
				case 'c':
					if (paramName == "border-bottom-color")
					{
						format.Borders.BottomColor = GetColor(paramValue);
					}
					break;
				case 'w':
					if (paramName == "border-bottom-width")
					{
						format.Borders.BottomWidth = CalculateBorderWidth(paramValue);
					}
					break;
				case 's':
					if (paramName == "border-bottom-style")
					{
						format.Borders.BottomStyle = ToBorderType(paramValue);
					}
					break;
				}
				break;
			case 4:
				if (paramName == "font" && !string.IsNullOrEmpty(paramValue))
				{
					GetFont(paramValue, format);
				}
				break;
			case 21:
				if (paramName == "-sf-before-space-auto" && paramValue == "yes")
				{
					format.BeforeSpaceAuto = true;
				}
				break;
			case 8:
				break;
				IL_07f1:
				if (!(paramName == "border-style"))
				{
					break;
				}
				goto IL_1c11;
				IL_1c11:
				array = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 1)
				{
					format.Borders.AllStyle = ToBorderType(paramValue);
				}
				else if (array.Length == 2)
				{
					format.Borders.TopStyle = (format.Borders.BottomStyle = ToBorderType(array[0]));
					format.Borders.LeftStyle = (format.Borders.RightStyle = ToBorderType(array[1]));
				}
				else if (array.Length == 3)
				{
					format.Borders.TopStyle = ToBorderType(array[0]);
					format.Borders.LeftStyle = (format.Borders.RightStyle = ToBorderType(array[1]));
					format.Borders.BottomStyle = ToBorderType(array[2]);
				}
				else if (array.Length == 4)
				{
					format.Borders.TopStyle = ToBorderType(array[0]);
					format.Borders.RightStyle = ToBorderType(array[1]);
					format.Borders.BottomStyle = ToBorderType(array[2]);
					format.Borders.LeftStyle = ToBorderType(array[3]);
				}
				break;
				IL_07c7:
				if (!(paramName == "border-color"))
				{
					break;
				}
				goto IL_18cd;
				IL_0f70:
				switch (paramValue)
				{
				case "underline":
					format.Underline = true;
					break;
				case "line-through":
					format.Strike = true;
					break;
				case "none":
					format.Underline = false;
					format.Strike = false;
					break;
				default:
					isPreserveBreakForInvalidStyles = true;
					break;
				}
				break;
				IL_07dc:
				if (!(paramName == "border-width"))
				{
					break;
				}
				goto IL_1a6f;
				IL_1a6f:
				array3 = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array3.Length == 1)
				{
					format.Borders.AllWidth = CalculateBorderWidth(paramValue);
				}
				else if (array3.Length == 2)
				{
					format.Borders.TopWidth = (format.Borders.BottomWidth = CalculateBorderWidth(array3[0]));
					format.Borders.LeftWidth = (format.Borders.RightWidth = CalculateBorderWidth(array3[1]));
				}
				else if (array3.Length == 3)
				{
					format.Borders.TopWidth = CalculateBorderWidth(array3[0]);
					format.Borders.LeftWidth = (format.Borders.RightWidth = CalculateBorderWidth(array3[1]));
					format.Borders.BottomWidth = CalculateBorderWidth(array3[2]);
				}
				else if (array3.Length == 4)
				{
					format.Borders.TopWidth = CalculateBorderWidth(array3[0]);
					format.Borders.RightWidth = CalculateBorderWidth(array3[1]);
					format.Borders.BottomWidth = CalculateBorderWidth(array3[2]);
					format.Borders.LeftWidth = CalculateBorderWidth(array3[3]);
				}
				break;
				IL_1191:
				name = node.Name;
				if (!name.Equals("table", StringComparison.OrdinalIgnoreCase) && !name.Equals("th", StringComparison.OrdinalIgnoreCase) && !name.Equals("td", StringComparison.OrdinalIgnoreCase))
				{
					format.BackColor = GetColor(paramValue);
				}
				if (paramValue == "transparent")
				{
					format.BackColor = Color.Empty;
				}
				break;
				IL_0806:
				if (paramName == "padding-left" && paramValue != "initial" && paramValue != "inherit")
				{
					float num4 = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					if (node.LocalName.Equals("li", StringComparison.OrdinalIgnoreCase))
					{
						format.PaddingLeft = num4;
					}
					else
					{
						format.LeftMargin = num4;
					}
				}
				break;
				IL_18cd:
				array4 = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array4.Length == 1)
				{
					format.Borders.AllColor = GetColor(paramValue);
				}
				else if (array4.Length == 2)
				{
					format.Borders.TopColor = (format.Borders.BottomColor = GetColor(array4[0]));
					format.Borders.LeftColor = (format.Borders.RightColor = GetColor(array4[1]));
				}
				else if (array4.Length == 3)
				{
					format.Borders.TopColor = GetColor(array4[0]);
					format.Borders.LeftColor = (format.Borders.RightColor = GetColor(array4[1]));
					format.Borders.BottomColor = GetColor(array4[2]);
				}
				else if (array4.Length == 4)
				{
					format.Borders.TopColor = GetColor(array4[0]);
					format.Borders.RightColor = GetColor(array4[1]);
					format.Borders.BottomColor = GetColor(array4[2]);
					format.Borders.LeftColor = GetColor(array4[3]);
				}
				break;
			}
		}
		catch
		{
			isPreserveBreakForInvalidStyles = true;
		}
	}

	private TabJustification GetTabjustification(string value)
	{
		return value.ToLower() switch
		{
			"center" => TabJustification.Centered, 
			"right" => TabJustification.Right, 
			"decimal" => TabJustification.Decimal, 
			"bar" => TabJustification.Bar, 
			_ => TabJustification.Left, 
		};
	}

	private TabLeader GetTabLeader(string value)
	{
		return value.ToLower() switch
		{
			"dotted" => TabLeader.Dotted, 
			"dashed" => TabLeader.Hyphenated, 
			"heavy" => TabLeader.Heavy, 
			_ => TabLeader.NoLeader, 
		};
	}

	private void GetTextFormat(TextFormat format, string paramName, string paramValue)
	{
		if (paramName == null)
		{
			return;
		}
		switch (paramName.Length)
		{
		case 14:
			switch (paramName[0])
			{
			case 't':
				if (!(paramName == "text-transform"))
				{
					break;
				}
				if (!(paramValue == "uppercase"))
				{
					if (paramValue == "none")
					{
						format.AllCaps = false;
					}
				}
				else
				{
					format.AllCaps = true;
				}
				break;
			case 'l':
				if (paramName == "letter-spacing")
				{
					if (paramValue == "normal")
					{
						format.CharacterSpacing = 0f;
					}
					else
					{
						format.CharacterSpacing = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
					}
				}
				break;
			}
			break;
		case 11:
			switch (paramName[5])
			{
			case 'f':
				if (paramName == "font-family")
				{
					format.FontFamily = GetFontName(paramValue);
				}
				break;
			case 'w':
				if (paramName == "font-weight")
				{
					int result;
					bool flag = int.TryParse(paramValue, out result);
					if (paramValue == "normal" || paramValue == "lighter" || (flag && (result < 550 || result > 1000)))
					{
						format.Bold = false;
					}
					else
					{
						format.Bold = true;
					}
				}
				break;
			}
			break;
		case 10:
			switch (paramName[0])
			{
			case 'f':
				if (paramName == "font-style")
				{
					switch (paramValue)
					{
					case "italic":
					case "oblique":
						format.Italic = true;
						break;
					case "strike":
						format.Strike = true;
						break;
					case "normal":
						format.Italic = false;
						break;
					}
				}
				break;
			case 't':
				if (paramName == "text-align")
				{
					format.TextAlign = GetHorizontalAlignment(paramValue);
				}
				break;
			}
			break;
		case 9:
			if (paramName == "font-size")
			{
				if (paramValue == "smaller")
				{
					format.FontSize = 10f;
				}
				else
				{
					format.FontSize = (float)ConvertSize(paramValue, format.FontSize);
				}
			}
			break;
		case 15:
			if (!(paramName == "text-decoration"))
			{
				break;
			}
			goto IL_0266;
		case 20:
			if (!(paramName == "text-decoration-line"))
			{
				break;
			}
			goto IL_0266;
		case 5:
			{
				if (paramName == "color")
				{
					format.FontColor = GetColor(paramValue);
				}
				break;
			}
			IL_0266:
			switch (paramValue)
			{
			case "underline":
				format.Underline = true;
				break;
			case "line-through":
				format.Strike = true;
				break;
			case "none":
				format.Underline = false;
				format.Strike = false;
				break;
			}
			break;
		}
	}

	private void ParseLineHeight(string paramValue, TextFormat format, ref bool isPreserveBreakForInvalidStyles)
	{
		if (paramValue != "normal")
		{
			int result3;
			if (paramValue.EndsWith("pt") || paramValue.EndsWith("px") || paramValue.EndsWith("em") || paramValue.EndsWith("cm") || paramValue.EndsWith("pc"))
			{
				if (float.TryParse(ExtractValue(paramValue), NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
				{
					format.LineSpacingRule = LineSpacingRule.AtLeast;
					format.LineHeight = result;
				}
			}
			else if (paramValue.EndsWith("%"))
			{
				paramValue = paramValue.Replace("%", string.Empty);
				if (float.TryParse(paramValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result2))
				{
					format.LineSpacingRule = LineSpacingRule.Multiple;
					format.LineHeight = result2 / 100f * 12f;
				}
			}
			else if (paramValue != "initial" && paramValue != "inherit" && int.TryParse(paramValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result3))
			{
				isPreserveBreakForInvalidStyles = true;
				format.LineSpacingRule = LineSpacingRule.Multiple;
				format.LineHeight = result3 * 12;
			}
		}
		else
		{
			format.LineSpacingRule = LineSpacingRule.Multiple;
			format.LineHeight = 12f;
		}
	}

	private Color GetRGBColor(string[] value, ref int j)
	{
		Color result = Color.Empty;
		string text = value[j];
		for (int i = j + 1; i < value.Length; i++)
		{
			if (value[i].Contains(")"))
			{
				text += value[i];
				result = GetColor(text);
				j += i - j;
				break;
			}
			text += value[i];
		}
		return result;
	}

	private Color GetColor(string attValue)
	{
		if (StartsWithExt(attValue, "rgb"))
		{
			string[] array = attValue.Replace("rgb", string.Empty).Trim('(', ')', ' ').Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 3)
			{
				int.TryParse(array[0], out var result);
				int.TryParse(array[1], out var result2);
				int.TryParse(array[2], out var result3);
				return Color.FromArgb(result, result2, result3);
			}
			return Color.Empty;
		}
		return ColorTranslator.FromHtml(attValue);
	}

	private string GetFontName(string paramValue)
	{
		string result = paramValue;
		if (paramValue.Trim().Contains(","))
		{
			int length = paramValue.Trim().IndexOf(',');
			result = paramValue.Trim().Substring(0, length);
		}
		return result;
	}

	private void GetFont(string paramValue, TextFormat format)
	{
		char[] separator = new char[1] { ' ' };
		string[] array = paramValue.Split(separator);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < array.Length; i++)
		{
			switch (array[i])
			{
			case "italic":
			case "oblique":
				if (dictionary.Count == 0)
				{
					dictionary.Add("font-style", array[i]);
				}
				else if (dictionary.Count == 1 && (dictionary.ContainsKey("font-weight") || dictionary.ContainsKey("font-variant")))
				{
					dictionary.Add("font-style", array[i]);
				}
				else if (dictionary.Count == 2 && dictionary.ContainsKey("font-weight") && dictionary.ContainsKey("font-variant"))
				{
					dictionary.Add("font-style", array[i]);
				}
				continue;
			case "small-caps":
				if (dictionary.Count == 0)
				{
					dictionary.Add("font-variant", array[i]);
				}
				else if (dictionary.Count == 1 && (dictionary.ContainsKey("font-weight") || dictionary.ContainsKey("font-style")))
				{
					dictionary.Add("font-variant", array[i]);
				}
				else if (dictionary.Count == 2 && dictionary.ContainsKey("font-weight") && dictionary.ContainsKey("font-style"))
				{
					dictionary.Add("font-variant", array[i]);
				}
				continue;
			case "bold":
				if (dictionary.Count == 0)
				{
					dictionary.Add("font-weight", array[i]);
				}
				else if (dictionary.Count == 1 && (dictionary.ContainsKey("font-variant") || dictionary.ContainsKey("font-style")))
				{
					dictionary.Add("font-weight", array[i]);
				}
				else if (dictionary.Count == 2 && dictionary.ContainsKey("font-variant") && dictionary.ContainsKey("font-style"))
				{
					dictionary.Add("font-weight", array[i]);
				}
				continue;
			}
			if (IsFontSize(array[i]))
			{
				char[] separator2 = new char[1] { '/' };
				string[] array2 = array[i].Split(separator2);
				dictionary.Add("font-size", ConvertSize(array2[0], format.FontSize).ToString());
				if (array2.Length == 2)
				{
					dictionary.Add("line-height", array2[1].ToString());
				}
				i++;
				string text = null;
				for (; i < array.Length; i++)
				{
					text = text + array[i] + " ";
				}
				if (!string.IsNullOrEmpty(text))
				{
					dictionary.Add("font-name", text);
				}
			}
			else if (i < array.Length && !dictionary.ContainsKey("font-size"))
			{
				dictionary.Clear();
			}
		}
		if (dictionary.Count <= 0)
		{
			return;
		}
		foreach (string key in dictionary.Keys)
		{
			switch (key)
			{
			case "font-style":
				format.Italic = true;
				break;
			case "font-weight":
				format.Bold = true;
				break;
			case "font-variant":
				format.SmallCaps = true;
				break;
			case "font-size":
			{
				float.TryParse(dictionary[key], NumberStyles.Number, CultureInfo.InvariantCulture, out var result);
				format.FontSize = result;
				break;
			}
			case "line-height":
			{
				string paramValue2 = dictionary[key];
				ParseLineHeight(paramValue2, format, ref isPreserveBreakForInvalidStyles);
				break;
			}
			case "font-name":
				format.FontFamily = dictionary[key];
				break;
			}
		}
	}

	private float CalculateFontSize(string value)
	{
		float result = 12f;
		if (value.EndsWith("pt") || value.EndsWith("px") || value.EndsWith("em") || value.EndsWith("cm") || value.EndsWith("pc") || value.EndsWith("in"))
		{
			result = Convert.ToSingle(ExtractValue(value), CultureInfo.InvariantCulture);
		}
		return result;
	}

	private bool IsFontSize(string value)
	{
		if (value.EndsWith("pt") || value.EndsWith("px") || value.EndsWith("in") || value.EndsWith("em") || value.EndsWith("cm") || value.EndsWith("pc") || value.Contains("/"))
		{
			return true;
		}
		return false;
	}

	private void ParseParagraphBorder(string paramValue, ref Color borderColor, ref float borderWidth, ref BorderStyle style)
	{
		string[] array = new string[9] { "dashed", "dotted", "double", "groove", "inset", "outset", "ridge", "solid", "hidden" };
		string[] array2 = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (paramValue == "none" || paramValue == "medium none")
		{
			borderColor = Color.Empty;
			style = BorderStyle.Cleared;
			borderWidth = 0f;
			return;
		}
		for (int i = 0; i < array2.Length; i++)
		{
			if (StartsWithExt(array2[i], "#"))
			{
				array2[i] = array2[i].Replace("#", string.Empty);
				array2[i] = GetValidRGBHexedecimal(array2[i]);
				int red = int.Parse(array2[i].Substring(0, 2), NumberStyles.AllowHexSpecifier);
				int green = int.Parse(array2[i].Substring(2, 2), NumberStyles.AllowHexSpecifier);
				int blue = int.Parse(array2[i].Substring(4, 2), NumberStyles.AllowHexSpecifier);
				borderColor = Color.FromArgb(red, green, blue);
				continue;
			}
			if (IsBorderWidth(array2[i]))
			{
				borderWidth = CalculateBorderWidth(array2[i]);
				continue;
			}
			string[] array3 = array;
			foreach (string text in array3)
			{
				if (array2[i] == text)
				{
					m_bBorderStyle = true;
				}
			}
			if (m_bBorderStyle)
			{
				style = ToBorderType(array2[i]);
				m_bBorderStyle = false;
				continue;
			}
			borderColor = GetColor(array2[i]);
			if (borderColor.IsEmpty && StartsWithExt(array2[i], "rgb("))
			{
				borderColor = GetRGBColor(array2, ref i);
			}
		}
		if (borderWidth < 0f)
		{
			borderWidth = 3f;
		}
	}

	private void ParseBorder(string paramValue, Border border)
	{
		string[] array = new string[9] { "dashed", "dotted", "double", "groove", "inset", "outset", "ridge", "solid", "hidden" };
		string[] array2 = paramValue.Split(' ');
		if (paramValue == "none" || paramValue == "medium none")
		{
			border.Color = Color.Empty;
			border.BorderType = BorderStyle.None;
			border.LineWidth = 0f;
			return;
		}
		for (int i = 0; i < array2.Length; i++)
		{
			if (StartsWithExt(array2[i], "#"))
			{
				array2[i] = array2[i].Replace("#", string.Empty);
				array2[i] = GetValidRGBHexedecimal(array2[i]);
				int red = int.Parse(array2[i].Substring(0, 2), NumberStyles.AllowHexSpecifier);
				int green = int.Parse(array2[i].Substring(2, 2), NumberStyles.AllowHexSpecifier);
				int blue = int.Parse(array2[i].Substring(4, 2), NumberStyles.AllowHexSpecifier);
				border.Color = Color.FromArgb(red, green, blue);
				continue;
			}
			if (IsBorderWidth(array2[i]))
			{
				border.LineWidth = CalculateBorderWidth(array2[i]);
				continue;
			}
			string[] array3 = array;
			foreach (string text in array3)
			{
				if (array2[i] == text)
				{
					m_bBorderStyle = true;
				}
			}
			if (m_bBorderStyle)
			{
				border.BorderType = ToBorderType(array2[i]);
				m_bBorderStyle = false;
				continue;
			}
			border.Color = GetColor(array2[i]);
			if (border.Color.IsEmpty && StartsWithExt(array2[i], "rgb("))
			{
				border.Color = GetRGBColor(array2, ref i);
			}
		}
	}

	private bool IsBorderWidth(string value)
	{
		if (value.EndsWith("pt") || value.EndsWith("px") || value.EndsWith("in") || value.EndsWith("em") || value.EndsWith("cm") || value.EndsWith("pc"))
		{
			return true;
		}
		switch (value)
		{
		case "medium":
		case "thick":
		case "thin":
			return true;
		default:
			return false;
		}
	}

	private float CalculateBorderWidth(string value)
	{
		float num = 0f;
		if (value.EndsWith("pt") || value.EndsWith("px") || value.EndsWith("em") || value.EndsWith("cm") || value.EndsWith("pc"))
		{
			num = Convert.ToSingle(ExtractValue(value), CultureInfo.InvariantCulture);
		}
		else if (value.EndsWith("in"))
		{
			num = ((SeperateParamValue(value)[1] != null) ? Convert.ToSingle(ExtractValue(value), CultureInfo.InvariantCulture) : 0.75f);
		}
		else if (value == "medium")
		{
			num = 3f;
		}
		else if (value == "thick")
		{
			num = 4.5f;
		}
		return (float)(int)(byte)Math.Round(num * 8f) / 8f;
	}

	private string GetValidRGBHexedecimal(string value)
	{
		if (value.Length == 3)
		{
			string text = string.Empty;
			string text2 = value;
			foreach (char c in text2)
			{
				text += new string(c, 2);
			}
			value = text;
		}
		return value;
	}

	private string[] SeperateParamValue(string paramValue)
	{
		string[] array = new string[2];
		for (int i = 0; i < paramValue.Length; i++)
		{
			char c = paramValue[i];
			if (char.IsDigit(c))
			{
				array[1] += c;
			}
			else
			{
				array[0] += c;
			}
		}
		return array;
	}

	private BorderStyle ToBorderType(string type)
	{
		switch (type.ToLower())
		{
		case "dashed":
			return BorderStyle.DashLargeGap;
		case "dotted":
			return BorderStyle.Dot;
		case "double":
			return BorderStyle.Double;
		case "groove":
			return BorderStyle.Engrave3D;
		case "inset":
			return BorderStyle.Inset;
		case "outset":
			return BorderStyle.Outset;
		case "ridge":
			return BorderStyle.Emboss3D;
		case "solid":
			return BorderStyle.Single;
		case "hidden":
		case "none":
			return BorderStyle.None;
		default:
			return BorderStyle.None;
		}
	}

	private void LeaveStyle(bool stylePresent)
	{
		if (stylePresent)
		{
			m_styleStack.Pop();
		}
	}

	private void UpdateParaFormat(XmlNode node, WParagraphFormat pformat)
	{
		if (m_currParagraph == null)
		{
			return;
		}
		string text = GetAttributeValue(node, "align");
		string attributeValue = GetAttributeValue(node, "style");
		if (attributeValue.Length != 0)
		{
			string[] array = attributeValue.Split(';', ':');
			int i = 0;
			for (int num = array.Length; i < num - 1; i += 2)
			{
				char[] trimChars = new char[2] { '\'', '"' };
				string text2 = array[i].ToLower().Trim();
				string text3 = array[i + 1].ToLower().Trim();
				text3 = text3.Trim(trimChars);
				if (text2 == "text-align")
				{
					text = text3;
				}
			}
		}
		if (text != string.Empty && !node.Name.Equals("table", StringComparison.OrdinalIgnoreCase))
		{
			pformat.HorizontalAlignment = GetHorizontalAlignment(text);
		}
		else if (m_currParagraph.IsInCell)
		{
			if (m_bIsAlignAttrDefinedInRowNode)
			{
				pformat.HorizontalAlignment = m_horizontalAlignmentDefinedInRowNode;
			}
			if (m_bIsAlignAttriDefinedInCellNode)
			{
				pformat.HorizontalAlignment = m_horizontalAlignmentDefinedInCellNode;
			}
		}
	}

	private TextFormat AddStyle()
	{
		TextFormat textFormat = ((m_styleStack.Count > 0) ? m_styleStack.Peek().Clone() : new TextFormat());
		m_styleStack.Push(textFormat);
		return textFormat;
	}

	private void UpdateHeightAndWidth(WPicture pic, bool isHeightSpecified, bool isWidthSpecified)
	{
		if (isHeightSpecified && !isWidthSpecified)
		{
			float num = pic.Height / (float)pic.Image.Height * 100f;
			pic.Width = (float)pic.Image.Width / (100f / num);
		}
		else if (!isHeightSpecified && isWidthSpecified)
		{
			float num2 = pic.Width / (float)pic.Image.Width * 100f;
			pic.Height = (float)pic.Image.Height / (100f / num2);
		}
	}

	private string GetAttributeValue(XmlNode node, string attrName)
	{
		attrName = attrName.ToLower();
		XmlAttribute xmlAttribute = null;
		for (int i = 0; i < node.Attributes.Count; i++)
		{
			xmlAttribute = node.Attributes[i];
			if (xmlAttribute.LocalName.Equals(attrName, StringComparison.OrdinalIgnoreCase))
			{
				return xmlAttribute.Value;
			}
		}
		return string.Empty;
	}

	private string GetStyleAttributeValue(string styleAttr, string styleAttrName)
	{
		string[] array = styleAttr.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (styleAttrName == "list-style-image")
			{
				if (array[i].Contains("list-style-image") && array[i].Contains("url"))
				{
					return array[i].Substring(array[i].IndexOf("url"));
				}
				continue;
			}
			string[] array2 = array[i].Split(':');
			char[] trimChars = new char[2] { '\'', '"' };
			string text = array2[0].ToLower().Trim();
			string text2 = array2[1].ToLower().Trim();
			text2 = text2.Trim(trimChars);
			if (text == styleAttrName)
			{
				return text2;
			}
		}
		return string.Empty;
	}

	private bool ConvertToBoolValue(string paramvalue)
	{
		switch (paramvalue)
		{
		case "always":
		case "auto":
		case "left":
		case "right":
		case "initial":
			return true;
		default:
			return false;
		}
	}

	private double ConvertSize(string paramValue, float baseSize)
	{
		float result = 0f;
		if (baseSize < 0f)
		{
			baseSize = 3f;
		}
		switch (paramValue)
		{
		case "xx-small":
			return 7.5;
		case "x-small":
			return 10.0;
		case "small":
			return 12.0;
		case "medium":
			return 13.5;
		case "large":
			return 18.0;
		case "x-large":
			return 24.0;
		case "xx-large":
			return 36.0;
		case "smaller":
			return 10.0;
		case "bigger":
			return 12.0;
		case "larger":
			return 13.5;
		default:
			if (paramValue.EndsWith("%"))
			{
				return baseSize * GetNumberBefore(paramValue, "%") / 100f;
			}
			if (paramValue.EndsWith("em"))
			{
				return baseSize * GetNumberBefore(paramValue, "em");
			}
			if (paramValue.EndsWith("ex"))
			{
				return baseSize / 2f * GetNumberBefore(paramValue, "ex");
			}
			if (paramValue.EndsWith("pt"))
			{
				return GetNumberBefore(paramValue, "pt");
			}
			if (paramValue.EndsWith("in"))
			{
				float.TryParse(paramValue.Replace("in", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				return PointsConverter.FromInch(result);
			}
			if (paramValue.EndsWith("cm"))
			{
				float.TryParse(paramValue.Replace("cm", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				return PointsConverter.FromCm(result);
			}
			if (paramValue.EndsWith("mm"))
			{
				float.TryParse(paramValue.Replace("mm", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				return (float)UnitsConvertor.Instance.ConvertUnits(result, PrintUnits.Millimeter, PrintUnits.Point);
			}
			if (paramValue.EndsWith("pc"))
			{
				float.TryParse(paramValue.Replace("pc", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				return result * 12f;
			}
			if (paramValue.EndsWith("px"))
			{
				float.TryParse(paramValue.Replace("px", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
				return (float)((double)result / 1.33);
			}
			float.TryParse(paramValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			return (float)((double)result / 1.33);
		}
	}

	private float GetNumberBefore(string val, string end)
	{
		val = val.Substring(0, val.IndexOf(end));
		return float.Parse(val, CultureInfo.InvariantCulture);
	}

	private void OnValidation(object sender, ValidationEventArgs args)
	{
		throw new NotSupportedException(args.Message, args.Exception);
	}

	private void BuildListStyle(ListPatternType type, XmlNode node)
	{
		ListStyle listStyle = null;
		if (ListStack.Count > 0)
		{
			listStyle = ListStack.Peek();
		}
		else
		{
			CreateListStyle(node);
			listStyle = ListStack.Peek();
		}
		if (IsListNodeStart(node))
		{
			CreateListLevel(listStyle, type, node);
		}
		m_currParagraph.ListFormat.ApplyStyle(listStyle.Name);
		if (LfoStack.Count > 0)
		{
			m_currParagraph.ListFormat.LFOStyleName = LfoStack.Peek();
		}
		m_currParagraph.ListFormat.ListLevelNumber = m_curListLevel;
		lastUsedLevelNo = m_curListLevel;
		isLastLevelSkipped = false;
	}

	public void CreateListLevel(ListStyle style, ListPatternType type, XmlNode node)
	{
		WListLevel wListLevel = style.Levels[m_curListLevel];
		if (m_listLevelNo.Contains(m_curListLevel) && wListLevel.PatternType != type && !isLastLevelSkipped)
		{
			wListLevel = CreateListOverrideStyle(m_curListLevel, node);
		}
		if (!m_listLevelNo.Contains(m_curListLevel))
		{
			m_listLevelNo.Add(m_curListLevel);
		}
		wListLevel.PatternType = type;
		wListLevel.NumberPosition = -18f;
		wListLevel.FollowCharacter = FollowCharacterType.Tab;
		wListLevel.CharacterFormat.RemoveFontNames();
		if (type == ListPatternType.Bullet)
		{
			UpdateBulletChar(m_curListLevel, node.ParentNode, wListLevel);
		}
		else
		{
			wListLevel.NumberPrefix = string.Empty;
			wListLevel.NumberSuffix = ".";
		}
		string attributeValue = GetAttributeValue(node, "VALUE");
		string attributeValue2 = GetAttributeValue(node.ParentNode, "START");
		if (!string.IsNullOrEmpty(attributeValue))
		{
			try
			{
				wListLevel.StartAt = Convert.ToInt32(attributeValue);
				return;
			}
			catch
			{
				wListLevel.StartAt = PrepareListStart(attributeValue, GetAttributeValue(node.ParentNode, "TYPE"));
				return;
			}
		}
		if (!string.IsNullOrEmpty(attributeValue2))
		{
			try
			{
				wListLevel.StartAt = Convert.ToInt32(attributeValue2);
			}
			catch
			{
				wListLevel.StartAt = PrepareListStart(attributeValue2, GetAttributeValue(node.ParentNode, "TYPE"));
			}
		}
	}

	private string GetListStyleType(XmlNode node)
	{
		foreach (XmlAttribute attribute in node.Attributes)
		{
			string text = attribute.LocalName.ToLower();
			if (!(text == "type"))
			{
				if (!(text == "style"))
				{
					continue;
				}
				string text2 = attribute.Value.ToLower();
				if (text2.Contains("list-style-type"))
				{
					int startIndex = text2.LastIndexOf("list-style-type");
					startIndex = text2.IndexOf(":", startIndex) + 1;
					int num = text2.Length;
					if (text2.Contains(";"))
					{
						num = text2.IndexOf(";", startIndex);
					}
					return text2.Substring(startIndex, num - startIndex).Trim();
				}
				if (text2.Contains("list-style"))
				{
					int startIndex2 = text2.LastIndexOf("list-style");
					startIndex2 = text2.IndexOf(":", startIndex2) + 1;
					int num2 = text2.Length;
					if (text2.Contains(";"))
					{
						num2 = text2.IndexOf(";", startIndex2);
					}
					if (num2 < 0)
					{
						num2 = text2.Length;
					}
					text2 = text2.Substring(startIndex2, num2 - startIndex2).Trim();
					if (text2.Contains(" "))
					{
						return text2[..text2.IndexOf(" ")].Trim();
					}
					return text2;
				}
				return null;
			}
			if (!node.LocalName.Equals("ul", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			return attribute.Value.ToLower();
		}
		return null;
	}

	private string GetListStyleTypeFromContainer(XmlNode node)
	{
		string result = null;
		while (node != null && !node.LocalName.Equals("html", StringComparison.OrdinalIgnoreCase))
		{
			switch (node.LocalName.ToLower())
			{
			case "body":
			case "table":
			case "tr":
			case "td":
			case "div":
			{
				string listStyleType = GetListStyleType(node);
				if (!string.IsNullOrEmpty(listStyleType))
				{
					result = listStyleType;
				}
				break;
			}
			}
			node = node.ParentNode;
		}
		return result;
	}

	private void UpdateBulletChar(int listLevelNo, XmlNode node, WListLevel listLevel)
	{
		string text = GetListStyleType(node);
		if (string.IsNullOrEmpty(text))
		{
			text = GetListStyleTypeFromContainer(node);
		}
		switch (text)
		{
		case "disc":
			listLevelNo = 0;
			break;
		case "circle":
			listLevelNo = 1;
			break;
		case "square":
			listLevelNo = 2;
			break;
		}
		switch (listLevelNo % 3)
		{
		case 0:
			listLevel.BulletCharacter = "\uf0b7";
			listLevel.CharacterFormat.FontName = "Symbol";
			break;
		case 1:
			listLevel.BulletCharacter = "o";
			listLevel.CharacterFormat.FontName = "Courier New";
			break;
		default:
			listLevel.BulletCharacter = "\uf0a7";
			listLevel.CharacterFormat.FontName = "Wingdings";
			break;
		}
		listLevel.CharacterFormat.SetPropertyValue(3, 10f);
	}

	private void CreateListStyle(XmlNode node)
	{
		string text = null;
		ListStyle listStyle = null;
		if (m_userListStyle != null)
		{
			ListStack.Push(m_userListStyle);
		}
		else
		{
			text = "ListStyle" + Guid.NewGuid();
			listStyle = ((!(node.Name == "ol")) ? m_bodyItems.Document.AddListStyle(ListType.Bulleted, text) : m_bodyItems.Document.AddListStyle(ListType.Numbered, text));
			listStyle.IsHybrid = true;
		}
		ListStack.Push(listStyle);
	}

	private WListLevel CreateListOverrideStyle(int levelNumber, XmlNode node)
	{
		ListOverrideStyle listOverrideStyle = new ListOverrideStyle(m_bodyItems.Document);
		listOverrideStyle.Name = "LfoStyle_" + Guid.NewGuid();
		m_bodyItems.Document.ListOverrides.Add(listOverrideStyle);
		OverrideLevelFormat overrideLevelFormat = new OverrideLevelFormat(m_bodyItems.Document);
		listOverrideStyle.OverrideLevels.Add(levelNumber, overrideLevelFormat);
		overrideLevelFormat.OverrideFormatting = true;
		LfoStack.Push(listOverrideStyle.Name);
		return overrideLevelFormat.OverrideListLevel;
	}

	private int PrepareListStart(string start, string type)
	{
		if (type == "i" || type == "I")
		{
			return RomanToArabic(start);
		}
		byte b = (byte)start.ToCharArray()[0];
		if (b >= 65 && b <= 90)
		{
			return b - 64;
		}
		if (b >= 97 && b <= 122)
		{
			return b - 96;
		}
		return 1;
	}

	private bool IsListNodeEnd(XmlNode node)
	{
		while (true)
		{
			if (node.NextSibling == null)
			{
				return true;
			}
			if (node.NextSibling.NodeType != XmlNodeType.Whitespace)
			{
				break;
			}
			node = node.NextSibling;
		}
		return false;
	}

	private bool IsListNodeStart(XmlNode node)
	{
		while (true)
		{
			if (node.PreviousSibling == null)
			{
				return true;
			}
			if (node.PreviousSibling.NodeType != XmlNodeType.Whitespace)
			{
				break;
			}
			node = node.PreviousSibling;
		}
		return false;
	}

	private bool IsInnerList(XmlNode node)
	{
		if (node.ParentNode != null && (node.ParentNode.Name.ToUpper() == "OL" || node.ParentNode.Name.ToUpper() == "UL") && node.ParentNode.ParentNode != null && (node.ParentNode.ParentNode.Name.ToUpper() == "BODY" || node.ParentNode.ParentNode.Name.ToUpper() == "HTML"))
		{
			return false;
		}
		return true;
	}

	private void ParseTable(XmlNode node)
	{
		m_bIsBorderCollapse = false;
		TableBorders tableBorders = new TableBorders();
		SpanHelper spanHelper = new SpanHelper();
		spanHelper.TableGridCollection = new Dictionary<int, List<KeyValuePair<int, float>>>();
		tableGrid.TableGridStack.Push(spanHelper.TableGridCollection);
		m_currTable = new WTable(m_bodyItems.Document, showBorder: false);
		if (m_bodyItems.Count > 0 && m_bodyItems.LastItem.EntityType == EntityType.Table)
		{
			WParagraph wParagraph = new WParagraph(m_bodyItems.Document);
			wParagraph.BreakCharacterFormat.Hidden = true;
			m_bodyItems.Add(wParagraph);
		}
		m_bodyItems.Add(m_currTable);
		IsTableStyle = false;
		m_currTable.TableFormat.IsAutoResized = true;
		BodyItemCollection bodyItems = m_bodyItems;
		TextFormat textFormat = currDivFormat;
		if (m_bIsInDiv && currDivFormat != null && (!m_currTable.IsInCell || NodeIsInDiv(node)))
		{
			ApplyDivTableFormat(node);
			if (m_styleStack.Count > 0)
			{
				textFormat = m_styleStack.Pop();
				m_styleStack.Push(new TextFormat());
			}
		}
		if (node.ParentNode.LocalName == "td")
		{
			(m_currTable.Owner as WTableCell).OwnerRow.OwnerTable.HasOnlyParagraphs = false;
		}
		m_currTable.HasOnlyParagraphs = true;
		m_currTable.IsFromHTML = true;
		ParseTableAttrs(node, spanHelper, tableBorders);
		ParseTableRows(node, spanHelper, tableBorders);
		LeaveStyle(IsTableStyle);
		if (m_bIsInDiv && textFormat != null && m_styleStack.Count > 0 && (!m_currTable.IsInCell || NodeIsInDiv(node)))
		{
			m_styleStack.Pop();
			m_styleStack.Push(textFormat);
		}
		m_bodyItems = bodyItems;
		spanHelper.UpdateTable(m_bodyItems.LastItem as WTable, tableGrid.TableGridStack, ClientWidth);
		if (m_currTableFooterRowIndex != -1)
		{
			m_currTable.Rows.Add(m_currTable.Rows[m_currTableFooterRowIndex]);
			m_currTableFooterRowIndex = -1;
		}
		if (!m_bIsBorderCollapse)
		{
			m_currTable.TableFormat.CellSpacing = cellSpacing;
			for (int i = 0; i < m_currTable.Rows.Count; i++)
			{
				m_currTable.Rows[i].RowFormat.CellSpacing = cellSpacing;
			}
		}
	}

	private void ApplyDivTableFormat(XmlNode node)
	{
		if (currDivFormat == null)
		{
			return;
		}
		RowFormat tableFormat = m_currTable.TableFormat;
		if (currDivFormat.HasValue(7))
		{
			tableFormat.BackColor = currDivFormat.BackColor;
		}
		if (currDivFormat.HasValue(12))
		{
			tableFormat.LeftIndent = currDivFormat.LeftMargin;
		}
		if (currDivFormat.Bidi)
		{
			tableFormat.Bidi = currDivFormat.Bidi;
		}
		if (currDivFormat.HasValue(10))
		{
			switch (currDivFormat.TextAlign)
			{
			case HorizontalAlignment.Center:
				tableFormat.HorizontalAlignment = RowAlignment.Center;
				break;
			case HorizontalAlignment.Right:
				tableFormat.HorizontalAlignment = RowAlignment.Right;
				break;
			default:
				tableFormat.HorizontalAlignment = RowAlignment.Left;
				break;
			}
		}
		if (currDivFormat.Borders.AllStyle != 0)
		{
			Border bottom = tableFormat.Borders.Bottom;
			Border top = tableFormat.Borders.Top;
			Border left = tableFormat.Borders.Left;
			BorderStyle borderStyle = (tableFormat.Borders.Right.BorderType = currDivFormat.Borders.AllStyle);
			BorderStyle borderStyle3 = (left.BorderType = borderStyle);
			BorderStyle borderType = (top.BorderType = borderStyle3);
			bottom.BorderType = borderType;
			if (currDivFormat.Borders.AllColor != Color.Empty)
			{
				Border bottom2 = tableFormat.Borders.Bottom;
				Border top2 = tableFormat.Borders.Top;
				Border left2 = tableFormat.Borders.Left;
				Color color = (tableFormat.Borders.Right.Color = currDivFormat.Borders.AllColor);
				Color color3 = (left2.Color = color);
				Color color5 = (top2.Color = color3);
				bottom2.Color = color5;
			}
			if (currDivFormat.Borders.AllWidth != -1f)
			{
				Border bottom3 = tableFormat.Borders.Bottom;
				Border top3 = tableFormat.Borders.Top;
				Border left3 = tableFormat.Borders.Left;
				float num = (tableFormat.Borders.Right.LineWidth = currDivFormat.Borders.AllWidth);
				float num3 = (left3.LineWidth = num);
				float lineWidth = (top3.LineWidth = num3);
				bottom3.LineWidth = lineWidth;
			}
		}
		if (currDivFormat.Borders.BottomStyle != 0)
		{
			tableFormat.Borders.Bottom.BorderType = currDivFormat.Borders.BottomStyle;
			tableFormat.Borders.Bottom.LineWidth = currDivFormat.Borders.BottomWidth;
			tableFormat.Borders.Bottom.Color = currDivFormat.Borders.BottomColor;
		}
		if (currDivFormat.Borders.TopStyle != 0)
		{
			tableFormat.Borders.Top.BorderType = currDivFormat.Borders.TopStyle;
			tableFormat.Borders.Top.LineWidth = currDivFormat.Borders.TopWidth;
			tableFormat.Borders.Top.Color = currDivFormat.Borders.TopColor;
		}
	}

	private void ParseTableRows(XmlNode node, SpanHelper spanHelper, TableBorders tblBorders)
	{
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			spanHelper.m_tblGrid = new List<KeyValuePair<int, float>>();
			m_bIsAlignAttrDefinedInRowNode = false;
			m_bIsVAlignAttriDefinedInRowNode = false;
			if (childNode.NodeType == XmlNodeType.Whitespace || childNode.NodeType == XmlNodeType.Comment)
			{
				continue;
			}
			if (childNode.Name.Equals("tbody", StringComparison.OrdinalIgnoreCase) || childNode.Name.Equals("thead", StringComparison.OrdinalIgnoreCase) || childNode.Name.Equals("tfoot", StringComparison.OrdinalIgnoreCase))
			{
				ParseTableRows(childNode, spanHelper, tblBorders);
			}
			else if (childNode.Name.Equals("caption", StringComparison.OrdinalIgnoreCase))
			{
				WTableRow wTableRow = m_currTable.AddRow(isCopyFormat: false, autoPopulateCells: false);
				wTableRow.HeightType = TableRowHeightType.AtLeast;
				wTableRow.IsHeader = true;
				WTableCell wTableCell = wTableRow.AddCell(isCopyFormat: false);
				m_currParagraph = (WParagraph)wTableCell.AddParagraph();
				m_currParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
				TraverseChildNodes(childNode.ChildNodes);
			}
			else
			{
				if (childNode.Name.Equals("col", StringComparison.OrdinalIgnoreCase) || childNode.Name.Equals("colgroup", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (!childNode.Name.Equals("tr", StringComparison.OrdinalIgnoreCase))
				{
					throw new NotSupportedException("Html contains not wellformatted table");
				}
				WTableRow wTableRow2 = m_currTable.AddRow(isCopyFormat: false, autoPopulateCells: false);
				IsRowStyle = false;
				wTableRow2.RowFormat.ImportContainer(m_currTable.TableFormat);
				if (childNode.ParentNode.Name == "thead")
				{
					wTableRow2.IsHeader = true;
				}
				else if (childNode.ParentNode.Name == "tfoot")
				{
					m_currTableFooterRowIndex = m_currTable.Rows.Count - 1;
				}
				ParseRowAttrs(childNode, wTableRow2);
				UpdateHiddenPropertyBasedOnParentNode(childNode, wTableRow2);
				spanHelper.ResetCurrColumn();
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					m_bIsAlignAttriDefinedInCellNode = false;
					if (childNode2.NodeType == XmlNodeType.Whitespace || childNode2.NodeType == XmlNodeType.Comment || childNode2.Name.Equals("col", StringComparison.OrdinalIgnoreCase) || childNode2.Name.Equals("colgroup", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					if (!childNode2.Name.Equals("td", StringComparison.OrdinalIgnoreCase) && !childNode2.Name.Equals("th", StringComparison.OrdinalIgnoreCase))
					{
						throw new NotSupportedException("Html contains not wellformatted table");
					}
					WTableCell wTableCell2 = wTableRow2.AddCell(isCopyFormat: false);
					IsCellStyle = false;
					m_bodyItems = wTableCell2.Items;
					m_currParagraph = (WParagraph)wTableCell2.AddParagraph();
					if (m_userStyle != null)
					{
						m_currParagraph.ApplyStyle(m_userStyle, isDomChanges: false);
					}
					bool flag = childNode2.Name.Equals("th", StringComparison.OrdinalIgnoreCase);
					if (flag)
					{
						m_bIsAlignAttriDefinedInCellNode = true;
						TextFormat textFormat = AddStyle();
						textFormat.TextAlign = HorizontalAlignment.Center;
						m_horizontalAlignmentDefinedInCellNode = HorizontalAlignment.Center;
						textFormat.Bold = true;
					}
					ParseCellAttrs(childNode2, wTableCell2, spanHelper, tblBorders);
					TraverseChildNodes(childNode2.ChildNodes);
					LeaveStyle(IsCellStyle);
					if (m_currParagraph != null)
					{
						ApplyTextFormatting(m_currParagraph.BreakCharacterFormat);
					}
					if (wTableCell2.Items.Count > 1 && wTableCell2.Items[0] is WParagraph && (wTableCell2.Items[0] as WParagraph).Items.Count == 0)
					{
						wTableCell2.Items.RemoveAt(0);
					}
					ApplyParagraphFormat(childNode2);
					LeaveStyle(flag);
					m_horizontalAlignmentDefinedInCellNode = HorizontalAlignment.Left;
					if (!wTableCell2.CellFormat.HasValue(4) && !wTableCell2.CellFormat.HasValue(5) && !wTableCell2.CellFormat.HasValue(7))
					{
						WTable ownerTable = wTableCell2.OwnerRow.OwnerTable;
						WTableRow ownerRow = wTableCell2.OwnerRow;
						if (ownerRow.RowFormat.HasValue(108))
						{
							wTableCell2.CellFormat.BackColor = ownerRow.RowFormat.BackColor;
						}
						else if (ownerTable.TableFormat.HasValue(108))
						{
							wTableCell2.CellFormat.BackColor = ownerTable.TableFormat.BackColor;
						}
					}
					wTableCell2.SetHasPreferredWidth();
				}
				LeaveStyle(IsRowStyle);
				spanHelper.TableGridCollection = tableGrid.TableGridStack.Pop();
				List<KeyValuePair<int, float>> list = new List<KeyValuePair<int, float>>();
				foreach (KeyValuePair<int, float> item in spanHelper.m_tblGrid)
				{
					list.Add(item);
				}
				if (m_currTable.Rows.Count - 1 != spanHelper.TableGridCollection.Count)
				{
					int num2 = 0;
					float num3 = 0f;
					List<KeyValuePair<int, float>> list2 = new List<KeyValuePair<int, float>>();
					for (int i = 0; i < spanHelper.m_tblGrid.Count; i++)
					{
						num2 += spanHelper.m_tblGrid[i].Key;
						num3 += spanHelper.m_tblGrid[i].Value;
					}
					list2.Add(new KeyValuePair<int, float>(num2, num3));
					m_currTable.FirstRow.Cells[0].Width = num3;
					spanHelper.AddCellToGrid(m_currTable.FirstRow.Cells[0], num2);
					spanHelper.TableGridCollection.Add(spanHelper.TableGridCollection.Count, list2);
				}
				spanHelper.m_tblGrid.Clear();
				spanHelper.TableGridCollection.Add(spanHelper.TableGridCollection.Count, list);
				tableGrid.TableGridStack.Push(spanHelper.TableGridCollection);
				m_horizontalAlignmentDefinedInRowNode = HorizontalAlignment.Left;
				if (m_bIsVAlignAttriDefinedInRowNode)
				{
					foreach (WTableCell cell in wTableRow2.Cells)
					{
						if (!cell.CellFormat.HasValue(2))
						{
							cell.CellFormat.VerticalAlignment = m_verticalAlignmentDefinedInRowNode;
						}
					}
				}
				if (wTableRow2.Index == 0)
				{
					num = wTableRow2.Cells.Count;
				}
				else if (wTableRow2.OwnerTable.IsAllRowsHaveSameCellCount && num != wTableRow2.Cells.Count)
				{
					wTableRow2.OwnerTable.IsAllRowsHaveSameCellCount = false;
				}
			}
		}
	}

	private XmlNode GetOwnerTable(XmlNode node)
	{
		while (node != null)
		{
			if (node.NodeType == XmlNodeType.Element && node.LocalName.Equals("table", StringComparison.OrdinalIgnoreCase))
			{
				return node;
			}
			node = node.ParentNode;
		}
		return node;
	}

	private void ParseCellAttrs(XmlNode node, WTableCell cell, SpanHelper spanHelper, TableBorders tblBrdrs)
	{
		int num = 1;
		CellFormat cellFormat = cell.CellFormat;
		cellFormat.VerticalAlignment = VerticalAlignment.Middle;
		List<XmlAttribute> list = new List<XmlAttribute>();
		cellFormat.Borders.IsHTMLRead = true;
		cellFormat.Borders.IsRead = true;
		foreach (XmlAttribute attribute in node.Attributes)
		{
			switch (attribute.Name.ToLower())
			{
			case "width":
				if (attribute.Value.Equals("auto", StringComparison.OrdinalIgnoreCase))
				{
					cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Auto;
				}
				else if (attribute.Value.EndsWith("%"))
				{
					cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Percentage;
					cell.CellFormat.PreferredWidth.Width = Convert.ToSingle(attribute.Value.Replace("%", string.Empty), CultureInfo.InvariantCulture);
					float width = spanHelper.m_tableWidth * cell.CellFormat.PreferredWidth.Width / 100f;
					cell.Width = width;
				}
				else if (attribute.Value != "initial" && attribute.Value != "inherit")
				{
					cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Point;
					float width2 = (cell.CellFormat.PreferredWidth.Width = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture));
					cell.Width = width2;
				}
				if (cell.PreferredWidth.WidthType == FtsWidth.Point)
				{
					m_currTable.HasPointPreferredCellWidth = true;
				}
				else if (cell.PreferredWidth.WidthType == FtsWidth.Percentage)
				{
					m_currTable.HasPercentPreferredCellWidth = true;
				}
				else if (cell.PreferredWidth.WidthType == FtsWidth.Auto)
				{
					m_currTable.HasAutoPreferredCellWidth = true;
				}
				else
				{
					m_currTable.HasNonePreferredCellWidth = true;
				}
				break;
			case "border":
			{
				float num3 = 0f;
				if (attribute.Value != "initial" && attribute.Value != "inherit")
				{
					num3 = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
				}
				if (num3 == 0f)
				{
					cellFormat.Borders.LineWidth = num3;
					cellFormat.Borders.BorderType = BorderStyle.None;
					cellFormat.Borders.Color = Color.Empty;
				}
				else
				{
					cellFormat.Borders.LineWidth = num3;
					cellFormat.Borders.BorderType = BorderStyle.Outset;
					cellFormat.Borders.Color = Color.Empty;
				}
				break;
			}
			case "bordercolor":
				cell.CellFormat.Borders.Color = GetColor(attribute.Value);
				break;
			case "style":
				IsCellStyle = true;
				ParseCellStyle(attribute, cell, node, spanHelper);
				break;
			case "colspan":
				list.Add(attribute);
				break;
			case "rowspan":
				list.Add(attribute);
				break;
			case "align":
				m_bIsAlignAttriDefinedInCellNode = true;
				m_horizontalAlignmentDefinedInCellNode = GetHorizontalAlignment(attribute.Value);
				break;
			case "bgcolor":
				cell.CellFormat.BackColor = GetColor(attribute.Value);
				break;
			case "valign":
				cell.CellFormat.VerticalAlignment = GetVerticalAlignment(attribute.Value);
				break;
			case "border-collapse":
				if (attribute.Value.Equals("collapse", StringComparison.OrdinalIgnoreCase))
				{
					m_bIsBorderCollapse = true;
				}
				break;
			case "class":
				FindClassSelector(null, cell.CellFormat, node);
				break;
			}
		}
		ApplyCellBorder(cellFormat, tblBrdrs);
		cellFormat.Borders.IsHTMLRead = false;
		cellFormat.Borders.IsRead = false;
		int colSpan = 1;
		foreach (XmlAttribute item in list)
		{
			string text = item.Name.ToLower();
			if (!(text == "colspan"))
			{
				if (text == "rowspan")
				{
					num = Convert.ToInt32(item.Value);
					if (num != 1)
					{
						cell.CellFormat.VerticalMerge = CellMerge.Start;
						spanHelper.m_rowspans.Add(cell, num);
					}
				}
			}
			else
			{
				short num4 = Convert.ToInt16(item.Value);
				if (num4 > 1)
				{
					colSpan = num4;
				}
			}
		}
		spanHelper.AddCellToGrid(cell, colSpan);
		spanHelper.NextColumn();
	}

	private void ParseCellStyle(XmlAttribute attr, WTableCell cell, XmlNode node, SpanHelper spanHelper)
	{
		if (!attr.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		TextFormat textFormat = AddStyle();
		textFormat.Borders = new TableBorders();
		CellFormat cellFormat = cell.CellFormat;
		string[] array = attr.Value.Split(';', ':');
		char[] separator = new char[1] { ' ' };
		int i = 0;
		for (int num = array.Length; i < num - 1; i += 2)
		{
			string text = array[i].ToLower().Trim();
			string text2 = array[i + 1].ToLower().Trim();
			if (text2.ToLower().Contains("inherit"))
			{
				continue;
			}
			try
			{
				switch (text)
				{
				case "background":
				case "background-color":
					cellFormat.BackColor = GetColor(text2);
					if (text2 == "transparent")
					{
						cellFormat.BackColor = Color.Empty;
					}
					break;
				case "width":
					if (text2.Equals("auto", StringComparison.OrdinalIgnoreCase))
					{
						cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Auto;
					}
					else if (text2.EndsWith("%"))
					{
						cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Percentage;
						cell.CellFormat.PreferredWidth.Width = Convert.ToSingle(text2.Replace("%", string.Empty));
						float width = spanHelper.m_tableWidth * cell.CellFormat.PreferredWidth.Width / 100f;
						cell.Width = width;
					}
					else if (text2 != "initial" && text2 != "inherit")
					{
						cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Point;
						float width2 = (cell.CellFormat.PreferredWidth.Width = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture));
						cell.Width = width2;
					}
					break;
				case "valign":
				case "vertical-align":
					cell.CellFormat.VerticalAlignment = GetVerticalAlignment(text2);
					break;
				case "display":
					if (text2.Equals("none", StringComparison.OrdinalIgnoreCase))
					{
						cell.CellFormat.Hidden = true;
					}
					else
					{
						cell.CellFormat.Hidden = false;
					}
					break;
				case "text-align":
					m_bIsAlignAttriDefinedInCellNode = true;
					m_horizontalAlignmentDefinedInCellNode = GetHorizontalAlignment(text2);
					break;
				case "border-bottom":
					ParseBorder(text2, cellFormat.Borders.Bottom);
					break;
				case "border-top":
					ParseBorder(text2, cellFormat.Borders.Top);
					break;
				case "border-left":
					ParseBorder(text2, cellFormat.Borders.Left);
					break;
				case "border-right":
					ParseBorder(text2, cellFormat.Borders.Right);
					break;
				case "border-color":
					ParseBorderColor(cellFormat.Borders, text2);
					break;
				case "border-left-color":
					cellFormat.Borders.Left.Color = GetColor(text2);
					break;
				case "border-right-color":
					cellFormat.Borders.Right.Color = GetColor(text2);
					break;
				case "border-top-color":
					cellFormat.Borders.Top.Color = GetColor(text2);
					break;
				case "border-bottom-color":
					cellFormat.Borders.Bottom.Color = GetColor(text2);
					break;
				case "border-width":
					ParseBorderLineWidth(cellFormat.Borders, text2);
					break;
				case "border-left-width":
					cellFormat.Borders.Left.LineWidth = CalculateBorderWidth(text2);
					break;
				case "border-right-width":
					cellFormat.Borders.Right.LineWidth = CalculateBorderWidth(text2);
					break;
				case "border-top-width":
					cellFormat.Borders.Top.LineWidth = CalculateBorderWidth(text2);
					break;
				case "border-bottom-width":
					cellFormat.Borders.Bottom.LineWidth = CalculateBorderWidth(text2);
					break;
				case "border-style":
					ParseBorderStyle(cellFormat.Borders, text2);
					break;
				case "border-left-style":
					cellFormat.Borders.Left.BorderType = ToBorderType(text2);
					break;
				case "border-right-style":
					cellFormat.Borders.Right.BorderType = ToBorderType(text2);
					break;
				case "border-top-style":
					cellFormat.Borders.Top.BorderType = ToBorderType(text2);
					break;
				case "border-bottom-style":
					cellFormat.Borders.Bottom.BorderType = ToBorderType(text2);
					break;
				case "border":
					ParseBorder(text2, cellFormat.Borders.Bottom);
					ParseBorder(text2, cellFormat.Borders.Top);
					ParseBorder(text2, cellFormat.Borders.Left);
					ParseBorder(text2, cellFormat.Borders.Right);
					break;
				case "border-collapse":
					if (text2.Equals("collapse", StringComparison.OrdinalIgnoreCase))
					{
						m_bIsBorderCollapse = true;
					}
					break;
				case "padding":
				{
					string[] array2 = text2.Split(separator);
					int num3 = array2.Length;
					cellFormat.SamePaddingsAsTable = false;
					switch (num3)
					{
					case 1:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							cellFormat.Paddings.All = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						break;
					case 2:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							cellFormat.Paddings.Top = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						cellFormat.Paddings.Bottom = cellFormat.Paddings.Top;
						if (array2[1] != "initial" && array2[1] != "inherit")
						{
							cellFormat.Paddings.Right = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						cellFormat.Paddings.Left = cellFormat.Paddings.Right;
						break;
					case 3:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							cellFormat.Paddings.Top = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "initial" && array2[1] != "inherit")
						{
							cellFormat.Paddings.Right = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						cellFormat.Paddings.Left = cellFormat.Paddings.Right;
						if (array2[2] != "initial" && array2[2] != "inherit")
						{
							cellFormat.Paddings.Bottom = Convert.ToSingle(ExtractValue(array2[2]), CultureInfo.InvariantCulture);
						}
						break;
					case 4:
						if (array2[0] != "initial" && array2[0] != "inherit")
						{
							cellFormat.Paddings.Top = Convert.ToSingle(ExtractValue(array2[0]), CultureInfo.InvariantCulture);
						}
						if (array2[1] != "initial" && array2[1] != "inherit")
						{
							cellFormat.Paddings.Right = Convert.ToSingle(ExtractValue(array2[1]), CultureInfo.InvariantCulture);
						}
						if (array2[2] != "initial" && array2[2] != "inherit")
						{
							cellFormat.Paddings.Bottom = Convert.ToSingle(ExtractValue(array2[2]), CultureInfo.InvariantCulture);
						}
						if (array2[3] != "initial" && array2[3] != "inherit")
						{
							cellFormat.Paddings.Left = Convert.ToSingle(ExtractValue(array2[3]), CultureInfo.InvariantCulture);
						}
						break;
					}
					break;
				}
				case "padding-left":
					if (text2 != "initial" && text2 != "inherit")
					{
						if (cellFormat.SamePaddingsAsTable)
						{
							cellFormat.SamePaddingsAsTable = false;
						}
						cellFormat.Paddings.Left = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				case "padding-right":
					if (text2 != "initial" && text2 != "inherit")
					{
						if (cellFormat.SamePaddingsAsTable)
						{
							cellFormat.SamePaddingsAsTable = false;
						}
						cellFormat.Paddings.Right = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				case "padding-top":
					if (text2 != "initial" && text2 != "inherit")
					{
						if (cellFormat.SamePaddingsAsTable)
						{
							cellFormat.SamePaddingsAsTable = false;
						}
						cellFormat.Paddings.Top = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				case "padding-bottom":
					if (text2 != "initial" && text2 != "inherit")
					{
						if (cellFormat.SamePaddingsAsTable)
						{
							cellFormat.SamePaddingsAsTable = false;
						}
						cellFormat.Paddings.Bottom = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				case "height":
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						cell.OwnerRow.Height = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				default:
					GetFormat(textFormat, text, text2, node);
					break;
				}
			}
			catch
			{
			}
		}
	}

	private void ApplyBorders(Borders borders, bool isTable)
	{
		ApplyBorder(borders.Top, isTable);
		ApplyBorder(borders.Bottom, isTable);
		ApplyBorder(borders.Left, isTable);
		ApplyBorder(borders.Right, isTable);
	}

	private void ApplyBorder(Border border, bool isTable)
	{
		if (border.BorderType != 0 && !border.HasNoneStyle && border.BorderType != BorderStyle.Cleared && border.LineWidth == 0f)
		{
			border.LineWidth = 1f;
		}
		if (border.LineWidth != 0f && border.BorderType == BorderStyle.None && !border.HasNoneStyle)
		{
			border.BorderType = BorderStyle.Outset;
		}
	}

	private void ApplyTableBorder(RowFormat format)
	{
		ApplyBorders(format.Borders, isTable: true);
		if (format.Borders.Horizontal.HasKey(2))
		{
			format.Borders.Horizontal.BorderType = BorderStyle.Cleared;
		}
		if (format.Borders.Vertical.HasKey(2))
		{
			format.Borders.Vertical.BorderType = BorderStyle.Cleared;
		}
	}

	private void ApplyCellBorder(CellFormat format, TableBorders ownerTableFormat)
	{
		ApplyBorders(format.Borders, isTable: false);
		if (format.Borders.Left.LineWidth == 0f && format.Borders.Left.BorderType == BorderStyle.None && !format.Borders.Left.HasNoneStyle && ownerTableFormat.AllWidth != -1f)
		{
			format.Borders.Left.LineWidth = ownerTableFormat.AllWidth;
			format.Borders.Left.BorderType = ownerTableFormat.AllStyle;
		}
		if (format.Borders.Left.Color == Color.Empty && ownerTableFormat.AllColor != Color.Empty)
		{
			format.Borders.Left.Color = ownerTableFormat.AllColor;
		}
		if (format.Borders.Right.LineWidth == 0f && format.Borders.Right.BorderType == BorderStyle.None && !format.Borders.Right.HasNoneStyle && ownerTableFormat.AllWidth != -1f)
		{
			format.Borders.Right.LineWidth = ownerTableFormat.AllWidth;
			format.Borders.Right.BorderType = ownerTableFormat.AllStyle;
		}
		if (format.Borders.Right.Color == Color.Empty && ownerTableFormat.AllColor != Color.Empty)
		{
			format.Borders.Right.Color = ownerTableFormat.AllColor;
		}
		if (format.Borders.Top.LineWidth == 0f && format.Borders.Top.BorderType == BorderStyle.None && !format.Borders.Top.HasNoneStyle && ownerTableFormat.AllWidth != -1f)
		{
			format.Borders.Top.LineWidth = ownerTableFormat.AllWidth;
			format.Borders.Top.BorderType = ownerTableFormat.AllStyle;
		}
		if (format.Borders.Top.Color == Color.Empty && ownerTableFormat.AllColor != Color.Empty)
		{
			format.Borders.Top.Color = ownerTableFormat.AllColor;
		}
		if (format.Borders.Bottom.LineWidth == 0f && format.Borders.Bottom.BorderType == BorderStyle.None && !format.Borders.Bottom.HasNoneStyle && ownerTableFormat.AllWidth != -1f)
		{
			format.Borders.Bottom.LineWidth = ownerTableFormat.AllWidth;
			format.Borders.Bottom.BorderType = ownerTableFormat.AllStyle;
		}
		if (format.Borders.Bottom.Color == Color.Empty && ownerTableFormat.AllColor != Color.Empty)
		{
			format.Borders.Bottom.Color = ownerTableFormat.AllColor;
		}
	}

	private void ParseBorderLineWidth(Borders borders, string paramValue)
	{
		string[] array = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			borders.LineWidth = CalculateBorderWidth(paramValue);
		}
		else if (array.Length == 2)
		{
			Border top = borders.Top;
			float lineWidth = (borders.Bottom.LineWidth = CalculateBorderWidth(array[0]));
			top.LineWidth = lineWidth;
			Border left = borders.Left;
			lineWidth = (borders.Right.LineWidth = CalculateBorderWidth(array[1]));
			left.LineWidth = lineWidth;
		}
		else if (array.Length == 3)
		{
			borders.Top.LineWidth = CalculateBorderWidth(array[0]);
			Border left2 = borders.Left;
			float lineWidth = (borders.Right.LineWidth = CalculateBorderWidth(array[1]));
			left2.LineWidth = lineWidth;
			borders.Bottom.LineWidth = CalculateBorderWidth(array[2]);
		}
		else if (array.Length == 4)
		{
			borders.Top.LineWidth = CalculateBorderWidth(array[0]);
			borders.Right.LineWidth = CalculateBorderWidth(array[1]);
			borders.Bottom.LineWidth = CalculateBorderWidth(array[2]);
			borders.Left.LineWidth = CalculateBorderWidth(array[3]);
		}
	}

	private void ParseBorderStyle(Borders borders, string paramValue)
	{
		string[] array = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			borders.BorderType = ToBorderType(paramValue);
		}
		else if (array.Length == 2)
		{
			Border top = borders.Top;
			BorderStyle borderType = (borders.Bottom.BorderType = ToBorderType(array[0]));
			top.BorderType = borderType;
			Border left = borders.Left;
			borderType = (borders.Right.BorderType = ToBorderType(array[1]));
			left.BorderType = borderType;
		}
		else if (array.Length == 3)
		{
			borders.Top.BorderType = ToBorderType(array[0]);
			Border left2 = borders.Left;
			BorderStyle borderType = (borders.Right.BorderType = ToBorderType(array[1]));
			left2.BorderType = borderType;
			borders.Bottom.BorderType = ToBorderType(array[2]);
		}
		else if (array.Length == 4)
		{
			borders.Top.BorderType = ToBorderType(array[0]);
			borders.Right.BorderType = ToBorderType(array[1]);
			borders.Bottom.BorderType = ToBorderType(array[2]);
			borders.Left.BorderType = ToBorderType(array[3]);
		}
	}

	private void ParseBorderColor(Borders borders, string paramValue)
	{
		string[] array = paramValue.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			borders.Color = GetColor(paramValue);
		}
		else if (array.Length == 2)
		{
			Border top = borders.Top;
			Color color2 = (borders.Bottom.Color = GetColor(array[0]));
			top.Color = color2;
			Border left = borders.Left;
			color2 = (borders.Right.Color = GetColor(array[1]));
			left.Color = color2;
		}
		else if (array.Length == 3)
		{
			borders.Top.Color = GetColor(array[0]);
			Border left2 = borders.Left;
			Color color2 = (borders.Right.Color = GetColor(array[1]));
			left2.Color = color2;
			borders.Bottom.Color = GetColor(array[2]);
		}
		else if (array.Length == 4)
		{
			borders.Top.Color = GetColor(array[0]);
			borders.Right.Color = GetColor(array[1]);
			borders.Bottom.Color = GetColor(array[2]);
			borders.Left.Color = GetColor(array[3]);
		}
	}

	private void ParseRowAttrs(XmlNode rowNode, WTableRow row)
	{
		foreach (XmlAttribute attribute in rowNode.Attributes)
		{
			switch (attribute.Name.ToLower())
			{
			case "height":
				if (attribute.Value != "auto" && attribute.Value != "initial" && attribute.Value != "inherit")
				{
					row.Height = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
				}
				break;
			case "align":
				m_bIsAlignAttrDefinedInRowNode = true;
				m_horizontalAlignmentDefinedInRowNode = GetHorizontalAlignment(attribute.Value);
				break;
			case "style":
				IsRowStyle = true;
				ParseRowStyle(attribute, row);
				break;
			case "valign":
			case "vertical-align":
				m_bIsVAlignAttriDefinedInRowNode = true;
				m_verticalAlignmentDefinedInRowNode = GetVerticalAlignment(attribute.Value.ToLower());
				break;
			case "bgcolor":
				row.RowFormat.BackColor = GetColor(attribute.Value);
				break;
			case "border-collapse":
				if (attribute.Value.Equals("collapse", StringComparison.OrdinalIgnoreCase))
				{
					m_bIsBorderCollapse = true;
				}
				break;
			}
		}
		row.HeightType = TableRowHeightType.AtLeast;
	}

	private void UpdateHiddenPropertyBasedOnParentNode(XmlNode rowNode, WTableRow row)
	{
		if (row.RowFormat.HasKey(121))
		{
			return;
		}
		WTableRow wTableRow = new WTableRow(m_currTable.Document);
		XmlNode parentNode = rowNode.ParentNode;
		while (parentNode.LocalName == "thead" || parentNode.LocalName == "tbody" || parentNode.LocalName == "tfoot")
		{
			ParseRowAttrs(parentNode, wTableRow);
			if (wTableRow.RowFormat.HasKey(121))
			{
				row.RowFormat.Hidden = wTableRow.RowFormat.Hidden;
				break;
			}
			parentNode = parentNode.ParentNode;
		}
	}

	private void ParseTableAttrs(XmlNode node, SpanHelper spanHelper, TableBorders brdrs)
	{
		if (m_currTable == null)
		{
			return;
		}
		RowFormat tableFormat = m_currTable.TableFormat;
		cellSpacing = PointsConverter.FromCm(0.05f) / 2f;
		tableFormat.Paddings.All = PointsConverter.FromCm(0.03f);
		tableFormat.Borders.IsHTMLRead = true;
		tableFormat.Borders.IsRead = true;
		foreach (XmlAttribute attribute in node.Attributes)
		{
			switch (attribute.Name.ToLower())
			{
			case "border":
				if (attribute.Value != "initial" && attribute.Value != "inherit")
				{
					float allWidth = (tableFormat.Borders.LineWidth = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture));
					brdrs.AllWidth = allWidth;
				}
				if (brdrs.AllWidth != -1f && brdrs.AllWidth != 0f)
				{
					brdrs.AllStyle = BorderStyle.Outset;
				}
				break;
			case "bordercolor":
			{
				Color allColor = (tableFormat.Borders.Color = GetColor(attribute.Value));
				brdrs.AllColor = allColor;
				break;
			}
			case "border-collapse":
				if (attribute.Value.Equals("collapse", StringComparison.OrdinalIgnoreCase))
				{
					m_bIsBorderCollapse = true;
				}
				break;
			case "cellpadding":
			{
				float px = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
				tableFormat.Paddings.All = PointsConverter.FromPixel(px);
				break;
			}
			case "cellspacing":
			{
				float px2 = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
				cellSpacing = PointsConverter.FromPixel(px2) / 2f;
				break;
			}
			case "title":
				m_currTable.Title = attribute.Value;
				break;
			case "style":
				IsTableStyle = true;
				ParseTableStyle(attribute, spanHelper);
				break;
			case "background":
			case "background-color":
			case "bgcolor":
				tableFormat.BackColor = GetColor(attribute.Value);
				if (attribute.Value == "transparent")
				{
					tableFormat.BackColor = Color.Empty;
				}
				break;
			case "align":
			{
				string value = attribute.Value;
				if (!(value == "center"))
				{
					if (value == "right")
					{
						m_currTable.TableFormat.HorizontalAlignment = RowAlignment.Right;
					}
					else
					{
						m_currTable.TableFormat.HorizontalAlignment = RowAlignment.Left;
					}
				}
				else
				{
					m_currTable.TableFormat.HorizontalAlignment = RowAlignment.Center;
				}
				break;
			}
			case "width":
				if (attribute.Value.Equals("auto", StringComparison.OrdinalIgnoreCase))
				{
					m_currTable.PreferredTableWidth.WidthType = FtsWidth.Auto;
				}
				else if (attribute.Value.EndsWith("%"))
				{
					m_currTable.PreferredTableWidth.WidthType = FtsWidth.Percentage;
					m_currTable.PreferredTableWidth.Width = Convert.ToSingle(attribute.Value.Replace("%", string.Empty), CultureInfo.InvariantCulture);
					float tableWidth = m_currTable.GetOwnerWidth() * m_currTable.PreferredTableWidth.Width / 100f;
					spanHelper.m_tableWidth = tableWidth;
				}
				else if (!attribute.Value.Equals("initial", StringComparison.OrdinalIgnoreCase) && !attribute.Value.Equals("inherit", StringComparison.OrdinalIgnoreCase))
				{
					m_currTable.PreferredTableWidth.WidthType = FtsWidth.Point;
					spanHelper.m_tableWidth = Convert.ToSingle(ExtractValue(attribute.Value), CultureInfo.InvariantCulture);
					m_currTable.PreferredTableWidth.Width = spanHelper.m_tableWidth;
				}
				break;
			}
		}
		ApplyTableBorder(tableFormat);
		tableFormat.Borders.IsHTMLRead = false;
		tableFormat.Borders.IsRead = false;
	}

	private void ParseTableStyle(XmlAttribute attr, SpanHelper spanHelper)
	{
		if (!attr.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		TextFormat textFormat = AddStyle();
		textFormat.Borders = new TableBorders();
		RowFormat tableFormat = m_currTable.TableFormat;
		string[] array = attr.Value.Split(';', ':');
		string text = string.Empty;
		string text2 = string.Empty;
		int i = 0;
		for (int num = array.Length; i < num - 1; i += 2)
		{
			string text3 = array[i].ToLower().Trim();
			string text4 = array[i + 1].ToLower().Trim();
			try
			{
				switch (text3)
				{
				case "display":
					if (text4.Equals("none", StringComparison.OrdinalIgnoreCase))
					{
						tableFormat.Hidden = true;
					}
					else
					{
						tableFormat.Hidden = false;
					}
					break;
				case "bgcolor":
				case "background":
				case "background-color":
					tableFormat.BackColor = GetColor(text4);
					if (text4 == "transparent")
					{
						tableFormat.BackColor = Color.Empty;
					}
					break;
				case "border-collapse":
					if (text4.Equals("collapse", StringComparison.OrdinalIgnoreCase))
					{
						m_bIsBorderCollapse = true;
					}
					break;
				case "width":
					text = text4;
					break;
				case "max-width":
					text2 = text4;
					break;
				case "margin-left":
					if (!text4.Equals("auto", StringComparison.OrdinalIgnoreCase) && !text4.Equals("initial", StringComparison.OrdinalIgnoreCase) && !text4.Equals("inherit", StringComparison.OrdinalIgnoreCase))
					{
						m_currTable.TableFormat.LeftIndent = Convert.ToSingle(ExtractValue(text4), CultureInfo.InvariantCulture);
					}
					break;
				default:
					ParseTableProperties(text3, text4, textFormat);
					break;
				}
			}
			catch
			{
			}
		}
		if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
		{
			SetTableWidthFromTableStyle(text, text2, spanHelper);
		}
	}

	private void SetTableWidthFromTableStyle(string widthValue, string maxWidthValue, SpanHelper spanHelper)
	{
		widthValue = ((widthValue.EndsWith("%") && !string.IsNullOrEmpty(maxWidthValue) && (maxWidthValue.EndsWith("px") || maxWidthValue.EndsWith("pt"))) ? maxWidthValue : widthValue);
		if (widthValue.Equals("auto", StringComparison.OrdinalIgnoreCase))
		{
			m_currTable.PreferredTableWidth.WidthType = FtsWidth.Auto;
		}
		else if (widthValue.EndsWith("%"))
		{
			m_currTable.PreferredTableWidth.WidthType = FtsWidth.Percentage;
			m_currTable.PreferredTableWidth.Width = Convert.ToSingle(widthValue.Replace("%", string.Empty), CultureInfo.InvariantCulture);
			float tableWidth = m_currTable.GetOwnerWidth() * m_currTable.PreferredTableWidth.Width / 100f;
			spanHelper.m_tableWidth = tableWidth;
		}
		else if (!widthValue.Equals("initial", StringComparison.OrdinalIgnoreCase) && !widthValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
		{
			m_currTable.PreferredTableWidth.WidthType = FtsWidth.Point;
			spanHelper.m_tableWidth = Convert.ToSingle(ExtractValue(widthValue), CultureInfo.InvariantCulture);
			m_currTable.PreferredTableWidth.Width = spanHelper.m_tableWidth;
		}
	}

	private void ParseTableProperties(string paramName, string paramValue, TextFormat textFormat)
	{
		RowFormat tableFormat = m_currTable.TableFormat;
		switch (paramName)
		{
		case "border-color":
			ParseBorderColor(tableFormat.Borders, paramValue);
			break;
		case "border-left-color":
			tableFormat.Borders.Left.Color = GetColor(paramValue);
			break;
		case "border-right-color":
			tableFormat.Borders.Right.Color = GetColor(paramValue);
			break;
		case "border-top-color":
			tableFormat.Borders.Right.Color = GetColor(paramValue);
			break;
		case "border-bottom-color":
			tableFormat.Borders.Bottom.Color = GetColor(paramValue);
			break;
		case "border-width":
			ParseBorderLineWidth(tableFormat.Borders, paramValue);
			break;
		case "border-left-width":
			tableFormat.Borders.Left.LineWidth = CalculateBorderWidth(paramValue);
			break;
		case "border-right-width":
			tableFormat.Borders.Right.LineWidth = CalculateBorderWidth(paramValue);
			break;
		case "border-top-width":
			tableFormat.Borders.Top.LineWidth = CalculateBorderWidth(paramValue);
			break;
		case "border-bottom-width":
			tableFormat.Borders.Bottom.LineWidth = CalculateBorderWidth(paramValue);
			break;
		case "border-style":
			ParseBorderStyle(tableFormat.Borders, paramValue);
			break;
		case "border-left-style":
			tableFormat.Borders.Left.BorderType = ToBorderType(paramValue);
			break;
		case "border-right-style":
			tableFormat.Borders.Right.BorderType = ToBorderType(paramValue);
			break;
		case "border-top-style":
			tableFormat.Borders.Top.BorderType = ToBorderType(paramValue);
			break;
		case "border-bottom-style":
			tableFormat.Borders.Bottom.BorderType = ToBorderType(paramValue);
			break;
		case "border-bottom":
			ParseTableBorder(paramValue, tableFormat.Borders.Bottom);
			break;
		case "border-top":
			ParseTableBorder(paramValue, tableFormat.Borders.Top);
			break;
		case "border-left":
			ParseTableBorder(paramValue, tableFormat.Borders.Left);
			break;
		case "border-right":
			ParseTableBorder(paramValue, tableFormat.Borders.Right);
			break;
		case "border":
			ParseTableBorder(paramValue, tableFormat.Borders.Bottom);
			ParseTableBorder(paramValue, tableFormat.Borders.Top);
			ParseTableBorder(paramValue, tableFormat.Borders.Left);
			ParseTableBorder(paramValue, tableFormat.Borders.Right);
			break;
		case "padding":
		{
			char[] separator = new char[1] { ' ' };
			string[] array = paramValue.Split(separator);
			switch (array.Length)
			{
			case 1:
				if (array[0] != "initial" && array[0] != "inherit")
				{
					tableFormat.Paddings.All = Convert.ToSingle(ExtractValue(array[0]), CultureInfo.InvariantCulture);
				}
				break;
			case 2:
				if (array[0] != "initial" && array[0] != "inherit")
				{
					tableFormat.Paddings.Top = Convert.ToSingle(ExtractValue(array[0]), CultureInfo.InvariantCulture);
				}
				tableFormat.Paddings.Bottom = tableFormat.Paddings.Top;
				if (array[1] != "initial" && array[1] != "inherit")
				{
					tableFormat.Paddings.Right = Convert.ToSingle(ExtractValue(array[1]), CultureInfo.InvariantCulture);
				}
				tableFormat.Paddings.Left = tableFormat.Paddings.Right;
				break;
			case 3:
				if (array[0] != "initial" && array[0] != "inherit")
				{
					tableFormat.Paddings.Top = Convert.ToSingle(ExtractValue(array[0]), CultureInfo.InvariantCulture);
				}
				if (array[1] != "initial" && array[1] != "inherit")
				{
					tableFormat.Paddings.Right = Convert.ToSingle(ExtractValue(array[1]), CultureInfo.InvariantCulture);
				}
				tableFormat.Paddings.Left = tableFormat.Paddings.Right;
				if (array[2] != "initial" && array[2] != "inherit")
				{
					tableFormat.Paddings.Bottom = Convert.ToSingle(ExtractValue(array[2]), CultureInfo.InvariantCulture);
				}
				break;
			case 4:
				if (array[0] != "initial" && array[0] != "inherit")
				{
					tableFormat.Paddings.Top = Convert.ToSingle(ExtractValue(array[0]), CultureInfo.InvariantCulture);
				}
				if (array[1] != "initial" && array[1] != "inherit")
				{
					tableFormat.Paddings.Right = Convert.ToSingle(ExtractValue(array[1]), CultureInfo.InvariantCulture);
				}
				if (array[2] != "initial" && array[2] != "inherit")
				{
					tableFormat.Paddings.Bottom = Convert.ToSingle(ExtractValue(array[2]), CultureInfo.InvariantCulture);
				}
				if (array[3] != "initial" && array[3] != "inherit")
				{
					tableFormat.Paddings.Left = Convert.ToSingle(ExtractValue(array[3]), CultureInfo.InvariantCulture);
				}
				break;
			}
			break;
		}
		case "padding-left":
			if (paramValue != "initial" && paramValue != "inherit")
			{
				tableFormat.Paddings.Left = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
			}
			break;
		case "padding-right":
			if (paramValue != "initial" && paramValue != "inherit")
			{
				tableFormat.Paddings.Right = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
			}
			break;
		case "padding-top":
			if (paramValue != "initial" && paramValue != "inherit")
			{
				tableFormat.Paddings.Top = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
			}
			break;
		case "padding-bottom":
			if (paramValue != "initial" && paramValue != "inherit")
			{
				tableFormat.Paddings.Bottom = Convert.ToSingle(ExtractValue(paramValue), CultureInfo.InvariantCulture);
			}
			break;
		default:
			GetTextFormat(textFormat, paramName, paramValue);
			break;
		}
	}

	private void ParseTableBorder(string paramValue, Border border)
	{
		string[] array = new string[10] { "dashed", "dotted", "double", "groove", "inset", "outset", "ridge", "solid", "hidden", "none" };
		string[] array2 = paramValue.Split(' ');
		if (paramValue == "none" || paramValue == "medium none")
		{
			border.BorderType = BorderStyle.None;
			border.Color = Color.Empty;
			border.LineWidth = 0f;
			return;
		}
		for (int i = 0; i < array2.Length; i++)
		{
			if (StartsWithExt(array2[i], "#"))
			{
				array2[i] = array2[i].Replace("#", string.Empty);
				array2[i] = GetValidRGBHexedecimal(array2[i]);
				int red = int.Parse(array2[i].Substring(0, 2), NumberStyles.AllowHexSpecifier);
				int green = int.Parse(array2[i].Substring(2, 2), NumberStyles.AllowHexSpecifier);
				int blue = int.Parse(array2[i].Substring(4, 2), NumberStyles.AllowHexSpecifier);
				border.Color = Color.FromArgb(red, green, blue);
				continue;
			}
			if (IsBorderWidth(array2[i]))
			{
				border.LineWidth = CalculateBorderWidth(array2[i]);
				continue;
			}
			string[] array3 = array;
			foreach (string text in array3)
			{
				if (array2[i] == text)
				{
					m_bBorderStyle = true;
				}
			}
			if (m_bBorderStyle)
			{
				border.BorderType = ToBorderType(array2[i]);
				m_bBorderStyle = false;
				continue;
			}
			border.Color = GetColor(array2[i]);
			if (border.Color.IsEmpty && StartsWithExt(array2[i], "rgb("))
			{
				border.Color = GetRGBColor(array2, ref i);
			}
		}
	}

	private void ParseRowStyle(XmlAttribute attr, WTableRow row)
	{
		if (!attr.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		TextFormat textFormat = AddStyle();
		textFormat.Borders = new TableBorders();
		RowFormat rowFormat = row.RowFormat;
		string[] array = attr.Value.Split(';', ':');
		int i = 0;
		for (int num = array.Length; i < num - 1; i += 2)
		{
			string text = array[i].ToLower().Trim();
			string text2 = array[i + 1].ToLower().Trim();
			try
			{
				switch (text)
				{
				case "display":
					if (text2.Equals("none", StringComparison.OrdinalIgnoreCase))
					{
						rowFormat.Hidden = true;
					}
					else
					{
						rowFormat.Hidden = false;
					}
					break;
				case "height":
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						row.Height = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
					break;
				case "text-align":
					m_bIsAlignAttrDefinedInRowNode = true;
					m_horizontalAlignmentDefinedInRowNode = GetHorizontalAlignment(text2);
					break;
				case "background":
				case "background-color":
					row.RowFormat.BackColor = GetColor(text2);
					break;
				default:
					GetTextFormat(textFormat, text, text2);
					break;
				}
			}
			catch
			{
			}
		}
	}

	private void ApplyTableBorder(string paramName, string paramValue, Border border)
	{
		string[] array = new string[10] { "dashed", "dotted", "double", "groove", "inset", "outset", "ridge", "solid", "hidden", "none" };
		char[] separator = new char[1] { ' ' };
		string[] array2 = paramValue.Split(separator);
		for (int i = 0; i < array2.Length; i++)
		{
			if (StartsWithExt(array2[i], "#"))
			{
				array2[i] = array2[i].Replace("#", string.Empty);
				array2[i] = GetValidRGBHexedecimal(array2[i]);
				int red = int.Parse(array2[i].Substring(0, 2), NumberStyles.AllowHexSpecifier);
				int green = int.Parse(array2[i].Substring(2, 2), NumberStyles.AllowHexSpecifier);
				int blue = int.Parse(array2[i].Substring(4, 2), NumberStyles.AllowHexSpecifier);
				border.Color = Color.FromArgb(red, green, blue);
				continue;
			}
			if (IsBorderWidth(array2[i]))
			{
				border.LineWidth = CalculateBorderWidth(array2[i]);
				continue;
			}
			string[] array3 = array;
			foreach (string text in array3)
			{
				if (array2[i] == text)
				{
					m_bBorderStyle = true;
				}
			}
			if (m_bBorderStyle)
			{
				border.BorderType = ToBorderType(array2[i]);
				m_bBorderStyle = false;
				continue;
			}
			border.Color = GetColor(array2[i]);
			if (border.Color.IsEmpty && StartsWithExt(array2[i], "rgb("))
			{
				border.Color = GetRGBColor(array2, ref i);
			}
		}
	}

	private float ToPoints(string val)
	{
		if (val.EndsWith("px"))
		{
			val = val.Substring(0, val.Length - 2);
		}
		return PointsConverter.FromPixel(Convert.ToSingle(val));
	}

	private VerticalAlignment GetVerticalAlignment(string val)
	{
		return val.ToLower() switch
		{
			"top" => VerticalAlignment.Top, 
			"middle" => VerticalAlignment.Middle, 
			"bottom" => VerticalAlignment.Bottom, 
			_ => VerticalAlignment.Top, 
		};
	}

	private HorizontalAlignment GetHorizontalAlignment(string val)
	{
		switch (val.ToLower())
		{
		case "center":
			return HorizontalAlignment.Center;
		case "right":
			return HorizontalAlignment.Right;
		case "justify":
			return HorizontalAlignment.Justify;
		default:
			if (!val.Equals("left", StringComparison.OrdinalIgnoreCase))
			{
				isPreserveBreakForInvalidStyles = true;
			}
			return HorizontalAlignment.Left;
		}
	}

	private int RomanToArabic(string numberStr)
	{
		numberStr = numberStr.ToUpper();
		char[] array = numberStr.ToCharArray();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int num4 = array.Length - 1; num4 >= 0; num4--)
		{
			num = ((array[num4] != 'M') ? ((array[num4] != 'D') ? ((array[num4] != 'C') ? ((array[num4] != 'L') ? ((array[num4] != 'X') ? ((array[num4] != 'V') ? ((array[num4] == 'I') ? 1 : 0) : 5) : 10) : 50) : 100) : 500) : 1000);
			num3 = ((num2 <= num) ? (num3 + num) : (num2 - num));
			num2 = num;
		}
		return num3;
	}

	private void Init()
	{
		m_curListLevel = -1;
		lastUsedLevelNo = -1;
		lastSkippedLevelNo = -1;
		listCount = 0;
		m_listStack = null;
		m_listLeftIndentStack.Clear();
	}

	private void ParseCssStyle(XmlNode node)
	{
		string innerText = node.InnerText;
		char[] separator = new char[1] { '}' };
		if (!(innerText != string.Empty))
		{
			return;
		}
		innerText = innerText.Trim();
		string[] array = innerText.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('{');
			if (array2.Length < 2)
			{
				break;
			}
			array2[0] = array2[0].Trim().ToLower();
			array2[1] = array2[1].Trim().ToLower();
			if (string.IsNullOrEmpty(array2[0]))
			{
				continue;
			}
			CSSStyleItem.CssStyleType cssStyleType = FindCSSselectorType(array2[0]);
			if (cssStyleType == CSSStyleItem.CssStyleType.None)
			{
				continue;
			}
			array2[0] = ((cssStyleType == CSSStyleItem.CssStyleType.ClassSelector || cssStyleType == CSSStyleItem.CssStyleType.IdSelector) ? array2[0].Substring(1) : array2[0]);
			if (cssStyleType == CSSStyleItem.CssStyleType.GroupingSelector)
			{
				string[] array3 = array2[0].Split(',');
				for (int j = 0; j < array3.Length; j++)
				{
					CSSStyleItem cSSStyleItem = CSSStyle.GetCSSStyleItem(array3[j], CSSStyleItem.CssStyleType.ElementSelector);
					if (cSSStyleItem == null)
					{
						cSSStyleItem = new CSSStyleItem();
						CSSStyle.StyleCollection.Add(cSSStyleItem);
					}
					cSSStyleItem.StyleName = array3[j];
					cSSStyleItem.StyleType = CSSStyleItem.CssStyleType.ElementSelector;
					ParseCSSTextFormatValue(array2[1], cSSStyleItem);
				}
			}
			else
			{
				CSSStyleItem cSSStyleItem2 = CSSStyle.GetCSSStyleItem(array2[0], cssStyleType);
				if (cSSStyleItem2 == null)
				{
					cSSStyleItem2 = new CSSStyleItem();
					CSSStyle.StyleCollection.Add(cSSStyleItem2);
				}
				cSSStyleItem2.StyleName = array2[0];
				cSSStyleItem2.StyleType = cssStyleType;
				ParseCSSTextFormatValue(array2[1], cSSStyleItem2);
			}
		}
	}

	private CSSStyleItem.CssStyleType FindCSSselectorType(string selectorName)
	{
		CSSStyleItem.CssStyleType result = CSSStyleItem.CssStyleType.None;
		string[] array = selectorName.Split(' ');
		if (StartsWithExt(selectorName, "#"))
		{
			result = CSSStyleItem.CssStyleType.IdSelector;
		}
		else if (StartsWithExt(selectorName, "."))
		{
			result = CSSStyleItem.CssStyleType.ClassSelector;
		}
		else if (selectorName.Contains(">"))
		{
			result = CSSStyleItem.CssStyleType.ChildSelector;
		}
		else if (selectorName.Contains("~"))
		{
			result = CSSStyleItem.CssStyleType.GeneralSiblingSelector;
		}
		else if (selectorName.Contains("+"))
		{
			result = CSSStyleItem.CssStyleType.AdjacentSiblingSelector;
		}
		else if (selectorName.Contains(","))
		{
			result = CSSStyleItem.CssStyleType.GroupingSelector;
		}
		else if (selectorName.Contains(" "))
		{
			result = CSSStyleItem.CssStyleType.DescendantSelector;
		}
		else if (array.Length == 1)
		{
			result = CSSStyleItem.CssStyleType.ElementSelector;
		}
		return result;
	}

	private void FindCSSstyleItem(XmlNode node, TextFormat textFormat)
	{
		if (CSSStyle != null)
		{
			FindIDSelector(textFormat, node);
			FindClassSelector(textFormat, null, node);
			FindDescendantSelector(textFormat, node);
			FindElementSelector(textFormat, node);
			FindChildSelector(textFormat, node);
		}
	}

	private void FindIDSelector(TextFormat textFormat, XmlNode node)
	{
		string attributeValue = GetAttributeValue(node, "id");
		if (!string.IsNullOrEmpty(attributeValue))
		{
			CSSStyleItem cSSStyleItem = CSSStyle.GetCSSStyleItem(attributeValue.ToLower(), CSSStyleItem.CssStyleType.IdSelector);
			if (cSSStyleItem != null)
			{
				ApplyCSSStyle(textFormat, node, cSSStyleItem);
				ApplyImportantCSSStyle(textFormat, node, cSSStyleItem);
			}
		}
	}

	private void FindClassSelector(TextFormat textFormat, CellFormat cellFormat, XmlNode node)
	{
		if (textFormat != null)
		{
			string attributeValue = GetAttributeValue(node, "class");
			if (!string.IsNullOrEmpty(attributeValue))
			{
				CSSStyleItem cSSStyleItem = CSSStyle.GetCSSStyleItem(attributeValue.ToLower(), CSSStyleItem.CssStyleType.ClassSelector);
				if (cSSStyleItem != null)
				{
					ApplyCSSStyle(textFormat, node, cSSStyleItem);
					ApplyImportantCSSStyle(textFormat, node, cSSStyleItem);
				}
			}
		}
		else
		{
			if (cellFormat == null)
			{
				return;
			}
			string attributeValue2 = GetAttributeValue(node, "class");
			if (!string.IsNullOrEmpty(attributeValue2))
			{
				CSSStyleItem cSSStyleItem2 = CSSStyle.GetCSSStyleItem(attributeValue2.ToLower(), CSSStyleItem.CssStyleType.ClassSelector);
				if (cSSStyleItem2 != null)
				{
					ApplyCSSStyleForCell(cellFormat, node, cSSStyleItem2);
				}
			}
		}
	}

	private void ApplyCSSStyleForCell(CellFormat cellFormat, XmlNode node, CSSStyleItem styleItem)
	{
		foreach (KeyValuePair<CSSStyleItem.TextFormatKey, object> item in styleItem.PropertiesHash)
		{
			if (item.Key == CSSStyleItem.TextFormatKey.BackColor && !cellFormat.HasValue(7))
			{
				cellFormat.BackColor = (Color)item.Value;
			}
		}
	}

	private void FindDescendantSelector(TextFormat textFormat, XmlNode node)
	{
		XmlNode xmlNode = node;
		for (int i = 0; i < CSSStyle.StyleCollection.Count; i++)
		{
			CSSStyleItem cSSStyleItem = CSSStyle.StyleCollection[i];
			if (cSSStyleItem.StyleType != CSSStyleItem.CssStyleType.DescendantSelector)
			{
				continue;
			}
			string[] array = cSSStyleItem.StyleName.Split(' ');
			for (int j = 0; j < array.Length && array[array.Length - j - 1].Equals(xmlNode.Name, StringComparison.OrdinalIgnoreCase); j++)
			{
				if (xmlNode.ParentNode != null)
				{
					xmlNode = xmlNode.ParentNode;
				}
				if (array.Length - 1 == j)
				{
					ApplyCSSStyle(textFormat, node, cSSStyleItem);
					ApplyImportantCSSStyle(textFormat, node, cSSStyleItem);
				}
			}
		}
	}

	private void FindElementSelector(TextFormat textFormat, XmlNode node)
	{
		CSSStyleItem cSSStyleItem = CSSStyle.GetCSSStyleItem(node.Name.ToLower(), CSSStyleItem.CssStyleType.ElementSelector);
		if (cSSStyleItem != null)
		{
			ApplyCSSStyle(textFormat, node, cSSStyleItem);
			ApplyImportantCSSStyle(textFormat, node, cSSStyleItem);
		}
	}

	private void FindChildSelector(TextFormat textFormat, XmlNode node)
	{
		if (node.ParentNode != null)
		{
			CSSStyleItem cSSStyleItem = CSSStyle.GetCSSStyleItem(node.ParentNode.Name.ToLower() + node.Name.ToLower(), CSSStyleItem.CssStyleType.ChildSelector);
			if (cSSStyleItem != null)
			{
				ApplyCSSStyle(textFormat, node, cSSStyleItem);
				ApplyImportantCSSStyle(textFormat, node, cSSStyleItem);
			}
		}
	}

	private void ApplyCSSStyle(TextFormat textFormat, XmlNode node, CSSStyleItem styleItem)
	{
		foreach (KeyValuePair<CSSStyleItem.TextFormatKey, object> item in styleItem.PropertiesHash)
		{
			switch (item.Key)
			{
			case CSSStyleItem.TextFormatKey.FontSize:
				if (!textFormat.HasValue(0))
				{
					textFormat.FontSize = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.FontFamily:
				if (!textFormat.HasValue(1))
				{
					textFormat.FontFamily = Convert.ToString(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.FontWeight:
				if (!textFormat.HasValue(2))
				{
					textFormat.Bold = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.Display:
				if (!textFormat.HasValue(23))
				{
					textFormat.Hidden = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.WhiteSpace:
				if (!textFormat.HasValue(21))
				{
					textFormat.IsPreserveWhiteSpace = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.AllCaps:
				if (!textFormat.HasValue(20))
				{
					textFormat.AllCaps = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.LetterSpacing:
				if (!textFormat.HasValue(19))
				{
					textFormat.CharacterSpacing = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.Italic:
				if (!textFormat.HasValue(4))
				{
					textFormat.Italic = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.Strike:
				if (!textFormat.HasValue(5))
				{
					textFormat.Strike = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.Underline:
				if (!textFormat.HasValue(3))
				{
					textFormat.Underline = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.TextAlign:
				if (!textFormat.HasValue(10))
				{
					textFormat.TextAlign = (HorizontalAlignment)item.Value;
				}
				break;
			case CSSStyleItem.TextFormatKey.FontColor:
				if (!textFormat.HasValue(6))
				{
					textFormat.FontColor = (Color)item.Value;
				}
				break;
			case CSSStyleItem.TextFormatKey.BackColor:
				if (!textFormat.HasValue(7))
				{
					textFormat.BackColor = (Color)item.Value;
				}
				break;
			case CSSStyleItem.TextFormatKey.TextIndent:
				if (!textFormat.HasValue(15))
				{
					textFormat.TextIndent = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.LeftMargin:
				if (!textFormat.HasValue(12))
				{
					textFormat.LeftMargin = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.RightMargin:
				if (!textFormat.HasValue(14))
				{
					textFormat.RightMargin = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.TopMargin:
				if (!textFormat.HasValue(11))
				{
					textFormat.TopMargin = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.BottomMargin:
				if (!textFormat.HasValue(13))
				{
					textFormat.BottomMargin = Convert.ToSingle(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.PageBreakBefore:
				if (!textFormat.HasValue(17))
				{
					textFormat.PageBreakBefore = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatKey.PageBreakAfter:
				if (!textFormat.HasValue(18))
				{
					textFormat.PageBreakAfter = Convert.ToBoolean(item.Value);
				}
				break;
			}
		}
	}

	private void ApplyImportantCSSStyle(TextFormat textFormat, XmlNode node, CSSStyleItem styleItem)
	{
		foreach (KeyValuePair<CSSStyleItem.TextFormatImportantKey, object> item in styleItem.ImportantPropertiesHash)
		{
			switch (item.Key)
			{
			case CSSStyleItem.TextFormatImportantKey.FontSize:
				textFormat.FontSize = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.FontFamily:
				textFormat.FontFamily = Convert.ToString(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.FontWeight:
				textFormat.Bold = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.Display:
				textFormat.Hidden = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.WhiteSpace:
				textFormat.IsPreserveWhiteSpace = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.AllCaps:
				if (!textFormat.HasValue(20))
				{
					textFormat.AllCaps = Convert.ToBoolean(item.Value);
				}
				break;
			case CSSStyleItem.TextFormatImportantKey.LetterSpacing:
				textFormat.CharacterSpacing = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.Italic:
				textFormat.Italic = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.Strike:
				textFormat.Strike = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.Underline:
				textFormat.Underline = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.TextAlign:
				textFormat.TextAlign = (HorizontalAlignment)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.FontColor:
				textFormat.FontColor = (Color)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BackColor:
				textFormat.BackColor = (Color)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.TextIndent:
				textFormat.TextIndent = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.LeftMargin:
				textFormat.LeftMargin = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.RightMargin:
				textFormat.RightMargin = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.TopMargin:
				textFormat.TopMargin = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.BottomMargin:
				textFormat.BottomMargin = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.PaddingLeft:
				textFormat.Borders.LeftSpace = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.PaddingRight:
				textFormat.Borders.RightSpace = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.PaddingTop:
				textFormat.Borders.TopSpace = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.PaddingBottom:
				textFormat.Borders.BottomSpace = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.PageBreakBefore:
				textFormat.PageBreakBefore = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.PageBreakAfter:
				textFormat.PageBreakAfter = Convert.ToBoolean(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderBottomColor:
				textFormat.Borders.BottomColor = (Color)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderBottomWidth:
				textFormat.Borders.BottomWidth = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderBottomStyle:
				textFormat.Borders.BottomStyle = (BorderStyle)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderTopColor:
				textFormat.Borders.TopColor = (Color)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderTopWidth:
				textFormat.Borders.TopWidth = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderTopStyle:
				textFormat.Borders.TopStyle = (BorderStyle)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderLeftColor:
				textFormat.Borders.LeftColor = (Color)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderLeftWidth:
				textFormat.Borders.LeftWidth = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderLeftStyle:
				textFormat.Borders.LeftStyle = (BorderStyle)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderRightColor:
				textFormat.Borders.RightColor = (Color)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderRightWidth:
				textFormat.Borders.RightWidth = Convert.ToSingle(item.Value);
				break;
			case CSSStyleItem.TextFormatImportantKey.BorderRightStyle:
				textFormat.Borders.RightStyle = (BorderStyle)item.Value;
				break;
			case CSSStyleItem.TextFormatImportantKey.LineHeight:
				textFormat.LineHeight = Convert.ToSingle(item.Value);
				break;
			}
		}
	}

	private void ParseCSSTextFormatValue(string textFormat, CSSStyleItem CSSItem)
	{
		char[] separator = new char[1] { ' ' };
		string[] array = textFormat.Split(':', ';');
		TextFormat textFormat2 = new TextFormat();
		int i = 0;
		for (int num = array.Length; i < num - 1; i += 2)
		{
			string text = array[i].Trim().ToLower();
			string text2 = array[i + 1].Trim().ToLower();
			if (string.IsNullOrEmpty(text2))
			{
				continue;
			}
			switch (text)
			{
			case "font-size":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.FontSize] = (float)ConvertSize(text2, 12f);
					}
				}
				else if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.FontSize] = (float)ConvertSize(text2, 12f);
				}
				break;
			case "font-family":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.FontFamily] = Convert.ToString(GetFontName(text2));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.FontFamily] = Convert.ToString(GetFontName(text2));
				}
				break;
			case "font-weight":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.FontWeight] = ((!(text2 == "normal") && !(text2 == "lighter")) ? true : false);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.FontWeight] = ((!(text2 == "normal") && !(text2 == "lighter")) ? true : false);
				}
				break;
			case "display":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.Display] = text2 == "none";
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.Display] = ((text2 == "none") ? Convert.ToBoolean(value: true) : Convert.ToBoolean(value: false));
				}
				break;
			case "white-space":
				if (text2.Contains("!important"))
				{
					int num2;
					switch (text2.Substring(0, text2.IndexOf('!')).Trim())
					{
					default:
						num2 = 0;
						break;
					case "pre":
					case "normal":
					case "nowrap":
					case "pre - line":
					case "pre - wrap":
					case "initial":
					case "inherit":
						num2 = 1;
						break;
					}
					CSSItem[CSSStyleItem.TextFormatImportantKey.WhiteSpace] = (byte)num2 != 0;
				}
				else
				{
					int num3;
					switch (text2)
					{
					default:
						num3 = 0;
						break;
					case "pre":
					case "normal":
					case "nowrap":
					case "pre - line":
					case "pre - wrap":
					case "initial":
					case "inherit":
						num3 = 1;
						break;
					}
					CSSItem[CSSStyleItem.TextFormatKey.WhiteSpace] = (byte)num3 != 0;
				}
				break;
			case "text-transform":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.AllCaps] = text2 == "uppercase";
					break;
				}
				switch (text2)
				{
				case "uppercase":
					CSSItem[CSSStyleItem.TextFormatKey.AllCaps] = true;
					break;
				case "capitalize":
					CSSItem[CSSStyleItem.TextFormatKey.Capitalize] = true;
					break;
				case "lowercase":
					CSSItem[CSSStyleItem.TextFormatKey.Lowercase] = true;
					break;
				case "none":
				case "initial":
					CSSItem[CSSStyleItem.TextFormatKey.AllCaps] = false;
					CSSItem[CSSStyleItem.TextFormatKey.Capitalize] = false;
					CSSItem[CSSStyleItem.TextFormatKey.Lowercase] = false;
					break;
				}
				break;
			case "letter-spacing":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					switch (text2)
					{
					case "normal":
					case "initial":
					case "inherit":
						CSSItem[CSSStyleItem.TextFormatImportantKey.LetterSpacing] = false;
						break;
					default:
						CSSItem[CSSStyleItem.TextFormatImportantKey.LetterSpacing] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
						break;
					}
				}
				else
				{
					switch (text2)
					{
					case "normal":
					case "initial":
					case "inherit":
						CSSItem[CSSStyleItem.TextFormatKey.LetterSpacing] = false;
						break;
					default:
						CSSItem[CSSStyleItem.TextFormatKey.LetterSpacing] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
						break;
					}
				}
				break;
			case "font-style":
				if (text2.Contains("!important"))
				{
					switch (text2.Substring(0, text2.IndexOf('!')).Trim())
					{
					case "italic":
					case "oblique":
						CSSItem[CSSStyleItem.TextFormatImportantKey.Italic] = true;
						break;
					case "strike":
						CSSItem[CSSStyleItem.TextFormatImportantKey.Strike] = true;
						break;
					}
				}
				else
				{
					switch (text2)
					{
					case "italic":
					case "oblique":
						CSSItem[CSSStyleItem.TextFormatKey.Italic] = true;
						break;
					case "strike":
						CSSItem[CSSStyleItem.TextFormatKey.Strike] = true;
						break;
					}
				}
				break;
			case "text-align":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.TextAlign] = GetHorizontalAlignment(text2);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.TextAlign] = GetHorizontalAlignment(text2);
				}
				break;
			case "text-decoration":
				if (text2.Contains("!important"))
				{
					switch (text2.Substring(0, text2.IndexOf('!')).Trim())
					{
					case "underline":
						CSSItem[CSSStyleItem.TextFormatImportantKey.Underline] = true;
						break;
					case "line-through":
						CSSItem[CSSStyleItem.TextFormatImportantKey.Strike] = true;
						break;
					case "none":
						CSSItem[CSSStyleItem.TextFormatImportantKey.Strike] = false;
						CSSItem[CSSStyleItem.TextFormatImportantKey.Underline] = false;
						break;
					}
				}
				else
				{
					switch (text2)
					{
					case "underline":
						CSSItem[CSSStyleItem.TextFormatKey.Underline] = true;
						break;
					case "line-through":
						CSSItem[CSSStyleItem.TextFormatKey.Strike] = true;
						break;
					case "none":
						CSSItem[CSSStyleItem.TextFormatKey.Strike] = false;
						CSSItem[CSSStyleItem.TextFormatKey.Underline] = false;
						break;
					}
				}
				break;
			case "color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.FontColor] = GetColor(text2);
					}
				}
				else if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.FontColor] = GetColor(text2);
				}
				break;
			case "background":
			{
				string[] array2 = text2.Split(separator);
				if (array2.Length == 5)
				{
					CSSItem[CSSStyleItem.TextFormatKey.BackColor] = GetColor(array2[0]);
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundImage] = array2[1];
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundRepeat] = array2[2];
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundAttachment] = array2[3];
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundPosition] = array2[4];
				}
				break;
			}
			case "background-color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (CSSItem.StyleName != "table" && CSSItem.StyleName != "th" && CSSItem.StyleName != "td")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BackColor] = GetColor(text2);
					}
					if (text2 == "transparent")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BackColor] = Color.Empty;
					}
				}
				else
				{
					if (CSSItem.StyleName != "table" && CSSItem.StyleName != "th" && CSSItem.StyleName != "td")
					{
						CSSItem[CSSStyleItem.TextFormatKey.BackColor] = GetColor(text2);
					}
					if (text2 == "transparent")
					{
						CSSItem[CSSStyleItem.TextFormatKey.BackColor] = Color.Empty;
					}
				}
				break;
			case "text-indent":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit" && CSSItem.StyleName != "ul" && CSSItem.StyleName != "ol")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.TextIndent] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "initial" && text2 != "inherit" && CSSItem.StyleName != "ul" && CSSItem.StyleName != "ol")
				{
					CSSItem[CSSStyleItem.TextFormatKey.TextIndent] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "margin-left":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.LeftMargin] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.LeftMargin] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "margin-right":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.RightMargin] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.RightMargin] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "margin-top":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.TopMargin] = ((text2 != "auto" && text2 != "initial" && text2 != "inherit") ? Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture) : (-1f));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.TopMargin] = ((text2 != "auto" && text2 != "initial" && text2 != "inherit") ? Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture) : (-1f));
				}
				break;
			case "margin-bottom":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BottomMargin] = ((text2 != "auto" && text2 != "initial" && text2 != "inherit") ? Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture) : (-1f));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BottomMargin] = ((text2 != "auto" && text2 != "initial" && text2 != "inherit") ? Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture) : (-1f));
				}
				break;
			case "margin":
			{
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					string[] array13 = text2.Split(separator);
					switch (array13.Length)
					{
					case 1:
						if (array13[0] != "auto" && array13[0] != "initial" && array13[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.TopMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BottomMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.LeftMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.RightMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
						}
						break;
					case 2:
						if (array13[0] != "auto" && array13[0] != "initial" && array13[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.TopMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BottomMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
						}
						if (array13[1] != "auto" && array13[1] != "initial" && array13[1] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.LeftMargin] = Convert.ToSingle(ExtractValue(array13[1]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.RightMargin] = Convert.ToSingle(ExtractValue(array13[1]), CultureInfo.InvariantCulture);
						}
						break;
					case 3:
						if (array13[0] != "auto" && array13[0] != "initial" && array13[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.TopMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
						}
						if (array13[1] != "auto" && array13[1] != "initial" && array13[1] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.RightMargin] = Convert.ToSingle(ExtractValue(array13[1]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.LeftMargin] = Convert.ToSingle(ExtractValue(array13[1]), CultureInfo.InvariantCulture);
						}
						if (array13[2] != "auto" && array13[2] != "initial" && array13[2] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.BottomMargin] = Convert.ToSingle(ExtractValue(array13[2]), CultureInfo.InvariantCulture);
						}
						break;
					case 4:
						if (array13[0] != "auto" && array13[0] != "initial" && array13[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.TopMargin] = Convert.ToSingle(ExtractValue(array13[0]), CultureInfo.InvariantCulture);
						}
						if (array13[1] != "auto" && array13[1] != "initial" && array13[1] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.RightMargin] = Convert.ToSingle(ExtractValue(array13[1]), CultureInfo.InvariantCulture);
						}
						if (array13[2] != "auto" && array13[2] != "initial" && array13[2] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.BottomMargin] = Convert.ToSingle(ExtractValue(array13[2]), CultureInfo.InvariantCulture);
						}
						if (array13[3] != "auto" && array13[3] != "initial" && array13[3] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.LeftMargin] = Convert.ToSingle(ExtractValue(array13[3]), CultureInfo.InvariantCulture);
						}
						break;
					}
					break;
				}
				string[] array14 = text2.Split(separator);
				switch (array14.Length)
				{
				case 1:
					if (array14[0] != "auto" && array14[0] != "initial" && array14[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.TopMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.BottomMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.LeftMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.RightMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
					}
					break;
				case 2:
					if (array14[0] != "auto" && array14[0] != "initial" && array14[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.TopMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.BottomMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
					}
					if (array14[1] != "auto" && array14[1] != "initial" && array14[1] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.LeftMargin] = Convert.ToSingle(ExtractValue(array14[1]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.RightMargin] = Convert.ToSingle(ExtractValue(array14[1]), CultureInfo.InvariantCulture);
					}
					break;
				case 3:
					if (array14[0] != "auto" && array14[0] != "initial" && array14[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.TopMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
					}
					if (array14[1] != "auto" && array14[1] != "initial" && array14[1] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.RightMargin] = Convert.ToSingle(ExtractValue(array14[1]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.LeftMargin] = Convert.ToSingle(ExtractValue(array14[1]), CultureInfo.InvariantCulture);
					}
					if (array14[2] != "auto" && array14[2] != "initial" && array14[2] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.BottomMargin] = Convert.ToSingle(ExtractValue(array14[2]), CultureInfo.InvariantCulture);
					}
					break;
				case 4:
					if (array14[0] != "auto" && array14[0] != "initial" && array14[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.TopMargin] = Convert.ToSingle(ExtractValue(array14[0]), CultureInfo.InvariantCulture);
					}
					if (array14[1] != "auto" && array14[1] != "initial" && array14[1] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.RightMargin] = Convert.ToSingle(ExtractValue(array14[1]), CultureInfo.InvariantCulture);
					}
					if (array14[2] != "auto" && array14[2] != "initial" && array14[2] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.BottomMargin] = Convert.ToSingle(ExtractValue(array14[2]), CultureInfo.InvariantCulture);
					}
					if (array14[3] != "auto" && array14[3] != "initial" && array14[3] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.LeftMargin] = Convert.ToSingle(ExtractValue(array14[3]), CultureInfo.InvariantCulture);
					}
					break;
				}
				break;
			}
			case "padding":
			{
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					string[] array9 = text2.Split(separator);
					switch (array9.Length)
					{
					case 1:
						if (array9[0] != "auto" && array9[0] != "initial" && array9[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingRight] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingTop] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
						}
						break;
					case 2:
						if (array9[0] != "auto" && array9[0] != "initial" && array9[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingTop] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
						}
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
						if (array9[1] != "auto" && array9[1] != "initial" && array9[1] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array9[1]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingRight] = Convert.ToSingle(ExtractValue(array9[1]), CultureInfo.InvariantCulture);
						}
						break;
					case 3:
						if (array9[0] != "auto" && array9[0] != "initial" && array9[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingTop] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
						}
						if (array9[1] != "auto" && array9[1] != "initial" && array9[1] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array9[1]), CultureInfo.InvariantCulture);
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingRight] = Convert.ToSingle(ExtractValue(array9[1]), CultureInfo.InvariantCulture);
						}
						if (array9[2] != "auto" && array9[2] != "initial" && array9[2] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array9[2]), CultureInfo.InvariantCulture);
						}
						break;
					case 4:
						if (array9[0] != "auto" && array9[0] != "initial" && array9[0] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingTop] = Convert.ToSingle(ExtractValue(array9[0]), CultureInfo.InvariantCulture);
						}
						if (array9[1] != "auto" && array9[1] != "initial" && array9[1] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingRight] = Convert.ToSingle(ExtractValue(array9[1]), CultureInfo.InvariantCulture);
						}
						if (array9[2] != "auto" && array9[2] != "initial" && array9[2] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array9[2]), CultureInfo.InvariantCulture);
						}
						if (array9[3] != "auto" && array9[3] != "initial" && array9[3] != "inherit")
						{
							CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array9[3]), CultureInfo.InvariantCulture);
						}
						break;
					}
					break;
				}
				string[] array10 = text2.Split(separator);
				switch (array10.Length)
				{
				case 1:
					if (array10[0] != "auto" && array10[0] != "initial" && array10[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.PaddingRight] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.PaddingTop] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
					}
					break;
				case 2:
					if (array10[0] != "auto" && array10[0] != "initial" && array10[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingTop] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
					}
					if (array10[1] != "auto" && array10[1] != "initial" && array10[1] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array10[1]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.PaddingRight] = Convert.ToSingle(ExtractValue(array10[1]), CultureInfo.InvariantCulture);
					}
					break;
				case 3:
					if (array10[0] != "auto" && array10[0] != "initial" && array10[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingTop] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
					}
					if (array10[1] != "auto" && array10[1] != "initial" && array10[1] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array10[1]), CultureInfo.InvariantCulture);
						CSSItem[CSSStyleItem.TextFormatKey.PaddingRight] = Convert.ToSingle(ExtractValue(array10[1]), CultureInfo.InvariantCulture);
					}
					if (array10[2] != "auto" && array10[2] != "initial" && array10[2] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array10[2]), CultureInfo.InvariantCulture);
					}
					break;
				case 4:
					if (array10[0] != "auto" && array10[0] != "initial" && array10[0] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingTop] = Convert.ToSingle(ExtractValue(array10[0]), CultureInfo.InvariantCulture);
					}
					if (array10[1] != "auto" && array10[1] != "initial" && array10[1] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingRight] = Convert.ToSingle(ExtractValue(array10[1]), CultureInfo.InvariantCulture);
					}
					if (array10[2] != "auto" && array10[2] != "initial" && array10[2] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingBottom] = Convert.ToSingle(ExtractValue(array10[2]), CultureInfo.InvariantCulture);
					}
					if (array10[3] != "auto" && array10[3] != "initial" && array10[3] != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatKey.PaddingLeft] = Convert.ToSingle(ExtractValue(array10[3]), CultureInfo.InvariantCulture);
					}
					break;
				}
				break;
			}
			case "padding-left":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingLeft] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.PaddingLeft] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "padding-top":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingTop] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.PaddingTop] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "padding-right":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingRight] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.PaddingRight] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "padding-bottom":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.PaddingBottom] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.PaddingBottom] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "word-break":
				CSSItem[CSSStyleItem.TextFormatKey.WordBreak] = text2;
				break;
			case "page-break-before":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.PageBreakBefore] = ConvertToBoolValue(text2);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatImportantKey.PageBreakBefore] = ConvertToBoolValue(text2);
				}
				break;
			case "page-break-inside":
				CSSItem[CSSStyleItem.TextFormatKey.PageBreakInside] = ConvertToBoolValue(text2);
				break;
			case "border-bottom":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					text2.Split(separator);
					ParseParagraphBorder(text2, ref textFormat2.Borders.BottomColor, ref textFormat2.Borders.BottomWidth, ref textFormat2.Borders.BottomStyle);
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomWidth] = textFormat2.Borders.BottomWidth;
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomStyle] = textFormat2.Borders.BottomStyle;
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = textFormat2.Borders.BottomColor;
				}
				else
				{
					text2.Split(separator);
					ParseParagraphBorder(text2, ref textFormat2.Borders.BottomColor, ref textFormat2.Borders.BottomWidth, ref textFormat2.Borders.BottomStyle);
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomWidth] = textFormat2.Borders.BottomWidth;
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = textFormat2.Borders.BottomStyle;
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = textFormat2.Borders.BottomColor;
				}
				break;
			case "border-top":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					text2.Split(separator);
					ParseParagraphBorder(text2, ref textFormat2.Borders.TopColor, ref textFormat2.Borders.TopWidth, ref textFormat2.Borders.TopStyle);
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopWidth] = textFormat2.Borders.TopWidth;
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopStyle] = textFormat2.Borders.TopStyle;
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopColor] = textFormat2.Borders.TopColor;
				}
				else
				{
					text2.Split(separator);
					ParseParagraphBorder(text2, ref textFormat2.Borders.TopColor, ref textFormat2.Borders.TopWidth, ref textFormat2.Borders.TopStyle);
					CSSItem[CSSStyleItem.TextFormatKey.BorderTopWidth] = textFormat2.Borders.TopWidth;
					CSSItem[CSSStyleItem.TextFormatKey.BorderTopStyle] = textFormat2.Borders.TopStyle;
					CSSItem[CSSStyleItem.TextFormatKey.BorderTopColor] = textFormat2.Borders.TopColor;
				}
				break;
			case "border-left":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					string[] array11 = text2.Split(separator);
					if (array11.Length == 3)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomWidth] = CalculateBorderWidth(array11[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomStyle] = array11[1];
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = array11[2];
					}
				}
				else
				{
					string[] array12 = text2.Split(separator);
					if (array12.Length == 3)
					{
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomWidth] = CalculateBorderWidth(array12[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = array12[1];
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = array12[2];
					}
				}
				break;
			case "border-right":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					string[] array5 = text2.Split(separator);
					if (array5.Length == 3)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomWidth] = CalculateBorderWidth(array5[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomStyle] = array5[1];
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = array5[2];
					}
				}
				else
				{
					string[] array6 = text2.Split(separator);
					if (array6.Length == 3)
					{
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomWidth] = CalculateBorderWidth(array6[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = array6[1];
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = array6[2];
					}
				}
				break;
			case "outline-color":
			case "border-color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
					{
						string[] array3 = text2.Split(separator);
						switch (array3.Length)
						{
						case 1:
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightColor] = GetColor(array3[0]);
							break;
						case 2:
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftColor] = GetColor(array3[1]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightColor] = GetColor(array3[1]);
							break;
						case 3:
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftColor] = GetColor(array3[1]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightColor] = GetColor(array3[1]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = GetColor(array3[2]);
							break;
						case 4:
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopColor] = GetColor(array3[0]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightColor] = GetColor(array3[1]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = GetColor(array3[2]);
							CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightColor] = GetColor(array3[3]);
							break;
						}
					}
				}
				else if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
				{
					string[] array4 = text2.Split(separator);
					switch (array4.Length)
					{
					case 1:
						CSSItem[CSSStyleItem.TextFormatKey.BorderTopColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderLeftColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightColor] = GetColor(array4[0]);
						break;
					case 2:
						CSSItem[CSSStyleItem.TextFormatKey.BorderTopColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderLeftColor] = GetColor(array4[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightColor] = GetColor(array4[1]);
						break;
					case 3:
						CSSItem[CSSStyleItem.TextFormatKey.BorderTopColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderLeftColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightColor] = GetColor(array4[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = GetColor(array4[2]);
						break;
					case 4:
						CSSItem[CSSStyleItem.TextFormatKey.BorderTopColor] = GetColor(array4[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightColor] = GetColor(array4[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = GetColor(array4[2]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightColor] = GetColor(array4[3]);
						break;
					}
				}
				break;
			case "border-left-color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftColor] = GetColor(text2);
					}
				}
				else if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderLeftColor] = GetColor(text2);
				}
				break;
			case "border-right-color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightColor] = GetColor(text2);
					}
				}
				else if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderRightColor] = GetColor(text2);
				}
				break;
			case "border-top-color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopColor] = GetColor(text2);
					}
				}
				if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderTopColor] = GetColor(text2);
				}
				break;
			case "border-bottom-color":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomColor] = GetColor(text2);
					}
				}
				if (text2 != "initial" && text2 != "inherit" && text2 != "transparent")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomColor] = GetColor(text2);
				}
				break;
			case "outline-width":
			case "border-width":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				break;
			case "border-left-width":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderLeftWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				break;
			case "border-right-width":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderRightWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				break;
			case "border-top-width":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderTopWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				break;
			case "border-bottom-width":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomWidth] = Convert.ToSingle(CalculateBorderWidth(text2));
				}
				break;
			case "outline-style":
			case "border-style":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					string[] array7 = text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (array7.Length == 1)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderStyle] = ToBorderType(text2);
					}
					else if (array7.Length == 2)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopStyle] = ToBorderType(array7[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = ToBorderType(array7[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftStyle] = ToBorderType(array7[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightStyle] = ToBorderType(array7[1]);
					}
					else if (array7.Length == 3)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopStyle] = ToBorderType(array7[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftStyle] = ToBorderType(array7[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightStyle] = ToBorderType(array7[1]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomStyle] = ToBorderType(array7[2]);
					}
					else if (array7.Length == 4)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopStyle] = ToBorderType(array7[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightStyle] = ToBorderType(array7[1]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomStyle] = ToBorderType(array7[2]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftStyle] = ToBorderType(array7[3]);
					}
				}
				else
				{
					string[] array8 = text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (array8.Length == 1)
					{
						CSSItem[CSSStyleItem.TextFormatKey.BorderStyle] = ToBorderType(text2);
					}
					else if (array8.Length == 2)
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopStyle] = ToBorderType(array8[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = ToBorderType(array8[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftStyle] = ToBorderType(array8[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightStyle] = ToBorderType(array8[1]);
					}
					else if (array8.Length == 3)
					{
						CSSItem[CSSStyleItem.TextFormatKey.BorderTopStyle] = ToBorderType(array8[0]);
						CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftStyle] = ToBorderType(array8[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightStyle] = ToBorderType(array8[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = ToBorderType(array8[2]);
					}
					else if (array8.Length == 4)
					{
						CSSItem[CSSStyleItem.TextFormatKey.BorderTopStyle] = ToBorderType(array8[0]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderRightStyle] = ToBorderType(array8[1]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = ToBorderType(array8[2]);
						CSSItem[CSSStyleItem.TextFormatKey.BorderLeftStyle] = ToBorderType(array8[3]);
					}
				}
				break;
			case "border-left-style":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderLeftStyle] = ToBorderType(text2);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderLeftStyle] = ToBorderType(text2);
				}
				break;
			case "border-right-style":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderRightStyle] = ToBorderType(text2);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderRightStyle] = ToBorderType(text2);
				}
				break;
			case "border-top-style":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderTopStyle] = ToBorderType(text2);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderTopStyle] = ToBorderType(text2);
				}
				break;
			case "border-bottom-style":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BorderBottomStyle] = ToBorderType(text2);
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomStyle] = ToBorderType(text2);
				}
				break;
			case "border":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.Border] = text2;
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.Border] = text2;
				}
				break;
			case "line-height":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 == "normal")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.LineHeight] = 12f;
					}
					else if (text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.LineHeight] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 == "normal")
				{
					CSSItem[CSSStyleItem.TextFormatKey.LineHeight] = 12f;
				}
				else if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.LineHeight] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "align-content":
				CSSItem[CSSStyleItem.TextFormatKey.ContentAlign] = text2;
				break;
			case "align-items":
				CSSItem[CSSStyleItem.TextFormatKey.ItemsAlign] = text2;
				break;
			case "align-self":
				CSSItem[CSSStyleItem.TextFormatKey.SelfAlign] = text2;
				break;
			case "animation":
				CSSItem[CSSStyleItem.TextFormatKey.Animation] = text2;
				break;
			case "animation-delay":
				if (text2 != "initial" && text2 != "inherit" && (text2.Contains("ms") || text2.Contains("s")))
				{
					if (text2.EndsWith("ms"))
					{
						text2 = text2.TrimEnd('s').TrimEnd('m');
						CSSItem[CSSStyleItem.TextFormatKey.TransitionDuration] = TimeSpan.FromMilliseconds(Convert.ToDouble(text2));
					}
					else if (text2.EndsWith("s"))
					{
						text2 = text2.TrimEnd('s');
						CSSItem[CSSStyleItem.TextFormatKey.TransitionDuration] = TimeSpan.FromSeconds(Convert.ToDouble(text2));
					}
				}
				break;
			case "animation-direction":
				CSSItem[CSSStyleItem.TextFormatKey.AnimationDirection] = text2;
				break;
			case "animation-duration":
				if (text2 != "initial" && text2 != "inherit" && (text2.Contains("ms") || text2.Contains("s")))
				{
					if (text2.EndsWith("ms"))
					{
						text2 = text2.TrimEnd('s').TrimEnd('m');
						CSSItem[CSSStyleItem.TextFormatKey.TransitionDuration] = TimeSpan.FromMilliseconds(Convert.ToDouble(text2));
					}
					else if (text2.EndsWith("s"))
					{
						text2 = text2.TrimEnd('s');
						CSSItem[CSSStyleItem.TextFormatKey.TransitionDuration] = TimeSpan.FromSeconds(Convert.ToDouble(text2));
					}
				}
				break;
			case "animation-fill-mode":
				CSSItem[CSSStyleItem.TextFormatKey.AnimationFillMode] = text2;
				break;
			case "animation-iteration-count":
				if (text2 != "initial" && text2 != "inherit" && text2 != "infinite")
				{
					CSSItem[CSSStyleItem.TextFormatKey.AnimationIterationCount] = Convert.ToInt32(text2);
				}
				break;
			case "animation-name":
				if (text2 != "initial" && text2 != "inherit" && text2 != "none")
				{
					CSSItem[CSSStyleItem.TextFormatKey.AnimationName] = text2;
				}
				break;
			case "animation-play-state":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.AnimationPlayState] = text2;
				}
				break;
			case "animation-timing-function":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.AnimationTimingFunction] = text2;
				}
				break;
			case "backface-visibility":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BackfaceVisibility] = text2;
				}
				break;
			case "background-attachment":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BackgroundAttachment] = text2;
				}
				else if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundAttachment] = text2;
				}
				break;
			case "background-clip":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundClip] = text2;
				}
				break;
			case "background-image":
				if (text2 != "none" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundImage] = text2;
				}
				break;
			case "background-origin":
				CSSItem[CSSStyleItem.TextFormatKey.BackgroundOrigin] = text2;
				break;
			case "background-position":
				CSSItem[CSSStyleItem.TextFormatKey.BackgroundPosition] = text2;
				break;
			case "background-repeat":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					CSSItem[CSSStyleItem.TextFormatImportantKey.BackgroundRepeat] = text2;
				}
				else if (text2 != "no-repeat")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BackgroundRepeat] = text2;
				}
				break;
			case "background-size":
				CSSItem[CSSStyleItem.TextFormatKey.BackgroundSize] = text2;
				break;
			case "border-bottom-left-radius":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomLeftRadius] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "border-bottom-right-radius":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderBottomRightRadius] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "border-collapse":
				CSSItem[CSSStyleItem.TextFormatKey.BorderCollapse] = text2;
				break;
			case "border-image":
				CSSItem[CSSStyleItem.TextFormatKey.BorderImage] = text2;
				break;
			case "border-image-outset":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderImageOutset] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "border-image-repeat":
				CSSItem[CSSStyleItem.TextFormatKey.BorderImageRepeat] = text2;
				break;
			case "border-image-slice":
				CSSItem[CSSStyleItem.TextFormatKey.BorderImageSlice] = text2;
				break;
			case "border-image-source":
				CSSItem[CSSStyleItem.TextFormatKey.BorderImageSource] = text2;
				break;
			case "border-image-width":
				if (text2 != "initial" && text2 != "inherit" && text2 != "auto")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderImageWidth] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "border-radius":
				CSSItem[CSSStyleItem.TextFormatKey.BorderRadius] = text2;
				break;
			case "border-spacing":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.BorderSpacing] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "border-top-left-radius":
				CSSItem[CSSStyleItem.TextFormatKey.BorderTopLeftRadius] = text2;
				break;
			case "border-top-right-radius":
				CSSItem[CSSStyleItem.TextFormatKey.BorderTopRightRadius] = text2;
				break;
			case "bottom":
				CSSItem[CSSStyleItem.TextFormatKey.Bottom] = text2;
				break;
			case "box-shadow":
				CSSItem[CSSStyleItem.TextFormatKey.BoxShadow] = text2;
				break;
			case "box-sizing":
				CSSItem[CSSStyleItem.TextFormatKey.BoxSizing] = text2;
				break;
			case "caption-side":
				CSSItem[CSSStyleItem.TextFormatKey.CaptionSide] = text2;
				break;
			case "clear":
				CSSItem[CSSStyleItem.TextFormatKey.Clear] = text2;
				break;
			case "clip":
				CSSItem[CSSStyleItem.TextFormatKey.Clip] = text2;
				break;
			case "column-count":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.ColumnCount] = Convert.ToInt32(text2);
				}
				break;
			case "column-fill":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnFill] = text2;
				break;
			case "column-gap":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnGap] = text2;
				break;
			case "column-rule":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnRule] = text2;
				break;
			case "column-rule-color":
				if (text2 != "initial" && text2 != " inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.ColumnRuleColor] = GetColor(text2);
				}
				break;
			case "column-rule-style":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnRuleStyle] = text2;
				break;
			case "column-rule-width":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnRuleWidth] = text2;
				break;
			case "column-span":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnSpan] = text2;
				break;
			case "column-width":
				CSSItem[CSSStyleItem.TextFormatKey.ColumnWidth] = text2;
				break;
			case "columns":
				CSSItem[CSSStyleItem.TextFormatKey.Columns] = text2;
				break;
			case "content":
				CSSItem[CSSStyleItem.TextFormatKey.Content] = text2;
				break;
			case "counter-increment":
				CSSItem[CSSStyleItem.TextFormatKey.CounterIncrement] = text2;
				break;
			case "counter-reset":
				CSSItem[CSSStyleItem.TextFormatKey.CounterReset] = text2;
				break;
			case "cursor":
				CSSItem[CSSStyleItem.TextFormatKey.Cursor] = text2;
				break;
			case "direction":
				CSSItem[CSSStyleItem.TextFormatKey.Direction] = text2;
				break;
			case "empty-cells":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "hide" || text2 != "initial" || text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.Display] = true;
					}
				}
				else if (text2 != "hide" || text2 != "initial" || text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.Display] = true;
				}
				break;
			case "flex":
				CSSItem[CSSStyleItem.TextFormatKey.Flex] = text2;
				break;
			case "flex-basis":
				CSSItem[CSSStyleItem.TextFormatKey.FlexBasis] = text2;
				break;
			case "flex-direction":
				CSSItem[CSSStyleItem.TextFormatKey.FlexDirection] = text2;
				break;
			case "flex-flow":
				CSSItem[CSSStyleItem.TextFormatKey.FlexFlow] = text2;
				break;
			case "flex-grow":
				CSSItem[CSSStyleItem.TextFormatKey.FlexGrow] = text2;
				break;
			case "flex-shrink":
				CSSItem[CSSStyleItem.TextFormatKey.FlexShrink] = text2;
				break;
			case "flex-wrap":
				CSSItem[CSSStyleItem.TextFormatKey.FlexWrap] = text2;
				break;
			case "float":
				CSSItem[CSSStyleItem.TextFormatKey.Float] = text2;
				break;
			case "font":
				CSSItem[CSSStyleItem.TextFormatKey.Display] = text2;
				break;
			case "@font-face":
				CSSItem[CSSStyleItem.TextFormatKey.FontFace] = text2;
				break;
			case "font-size-adjust":
				CSSItem[CSSStyleItem.TextFormatKey.FontSizeAdjust] = text2;
				break;
			case "font-stretch":
				CSSItem[CSSStyleItem.TextFormatKey.FontStretch] = text2;
				break;
			case "font-variant":
				CSSItem[CSSStyleItem.TextFormatKey.FontVariant] = text2;
				break;
			case "hanging-punctuation":
				CSSItem[CSSStyleItem.TextFormatKey.HangingPunctuation] = text2;
				break;
			case "height":
				CSSItem[CSSStyleItem.TextFormatKey.Height] = text2;
				break;
			case "icon":
				CSSItem[CSSStyleItem.TextFormatKey.Icon] = text2;
				break;
			case "justify-content":
				CSSItem[CSSStyleItem.TextFormatKey.JustifyContent] = text2;
				break;
			case "@keyframes":
				CSSItem[CSSStyleItem.TextFormatKey.KeyFrames] = text2;
				break;
			case "left":
				CSSItem[CSSStyleItem.TextFormatKey.Left] = text2;
				break;
			case "list-style":
				CSSItem[CSSStyleItem.TextFormatKey.ListStyle] = text2;
				break;
			case "list-style-image":
				CSSItem[CSSStyleItem.TextFormatKey.listStyleImage] = text2;
				break;
			case "list-style-position":
				CSSItem[CSSStyleItem.TextFormatKey.listStylePosition] = text2;
				break;
			case "list-style-type":
				CSSItem[CSSStyleItem.TextFormatKey.ListStyleType] = text2;
				break;
			case "max-height":
				CSSItem[CSSStyleItem.TextFormatKey.MaxHeight] = text2;
				break;
			case "max-width":
				CSSItem[CSSStyleItem.TextFormatKey.MaxWidth] = text2;
				break;
			case "min-height":
				CSSItem[CSSStyleItem.TextFormatKey.MinHeight] = text2;
				break;
			case "min-width":
				CSSItem[CSSStyleItem.TextFormatKey.MinWidth] = text2;
				break;
			case "nav-down":
				CSSItem[CSSStyleItem.TextFormatKey.NavDown] = text2;
				break;
			case "nav-index":
				CSSItem[CSSStyleItem.TextFormatKey.NavIndex] = text2;
				break;
			case "nav-left":
				CSSItem[CSSStyleItem.TextFormatKey.NavLeft] = text2;
				break;
			case "nav-right":
				CSSItem[CSSStyleItem.TextFormatKey.NavRight] = text2;
				break;
			case "nav-up":
				CSSItem[CSSStyleItem.TextFormatKey.NavUp] = text2;
				break;
			case "opacity":
				if (text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.Opacity] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "order":
				CSSItem[CSSStyleItem.TextFormatKey.Order] = text2;
				break;
			case "outline":
				CSSItem[CSSStyleItem.TextFormatKey.Outline] = text2;
				break;
			case "outline-offset":
				CSSItem[CSSStyleItem.TextFormatKey.OutlineOffset] = text2;
				break;
			case "overflow":
				CSSItem[CSSStyleItem.TextFormatKey.OverFlow] = text2;
				break;
			case "overflow-x":
				CSSItem[CSSStyleItem.TextFormatKey.Overflow_X] = text2;
				break;
			case "overflow-y":
				CSSItem[CSSStyleItem.TextFormatKey.Overflow_Y] = text2;
				break;
			case "perspective":
				CSSItem[CSSStyleItem.TextFormatKey.Perspective] = text2;
				break;
			case "perspective-origin":
				CSSItem[CSSStyleItem.TextFormatKey.PerspectiveOrigin] = text2;
				break;
			case "position":
				CSSItem[CSSStyleItem.TextFormatKey.Position] = text2;
				break;
			case "quotes":
				CSSItem[CSSStyleItem.TextFormatKey.Quotes] = text2;
				break;
			case "resize":
				CSSItem[CSSStyleItem.TextFormatKey.Resize] = text2;
				break;
			case "right":
				CSSItem[CSSStyleItem.TextFormatKey.Right] = text2;
				break;
			case "table-layout":
				CSSItem[CSSStyleItem.TextFormatKey.TableLayout] = text2;
				break;
			case "text-align-last":
				CSSItem[CSSStyleItem.TextFormatKey.TextAlignLast] = text2;
				break;
			case "text-decoration-color":
				CSSItem[CSSStyleItem.TextFormatKey.TextDecorationColor] = text2;
				break;
			case "text-decoration-line":
				CSSItem[CSSStyleItem.TextFormatKey.TextDecorationLine] = text2;
				break;
			case "text-justify":
				CSSItem[CSSStyleItem.TextFormatKey.TextJustify] = text2;
				break;
			case "text-overflow":
				CSSItem[CSSStyleItem.TextFormatKey.TextOverflow] = text2;
				break;
			case "text-shadow":
				CSSItem[CSSStyleItem.TextFormatKey.TextShadow] = text2;
				break;
			case "top":
				CSSItem[CSSStyleItem.TextFormatKey.Top] = text2;
				break;
			case "transform":
				CSSItem[CSSStyleItem.TextFormatKey.Transform] = text2;
				break;
			case "transform-origin":
				CSSItem[CSSStyleItem.TextFormatKey.TransformOrigin] = text2;
				break;
			case "transform-style":
				CSSItem[CSSStyleItem.TextFormatKey.TransformStyle] = text2;
				break;
			case "transition":
				CSSItem[CSSStyleItem.TextFormatKey.Transition] = text2;
				break;
			case "transition-delay":
				CSSItem[CSSStyleItem.TextFormatKey.TransitionDelay] = text2;
				break;
			case "transition-duration":
				if (text2 != "initial" && text2 != "inherit" && (text2.Contains("ms") || text2.Contains("s")))
				{
					if (text2.EndsWith("ms"))
					{
						text2 = text2.TrimEnd('s').TrimEnd('m');
						CSSItem[CSSStyleItem.TextFormatKey.TransitionDuration] = TimeSpan.FromMilliseconds(Convert.ToDouble(text2));
					}
					else if (text2.EndsWith("s"))
					{
						text2 = text2.TrimEnd('s');
						CSSItem[CSSStyleItem.TextFormatKey.TransitionDuration] = TimeSpan.FromSeconds(Convert.ToDouble(text2));
					}
				}
				break;
			case "transition-property":
				CSSItem[CSSStyleItem.TextFormatKey.TransitionProperty] = text2;
				break;
			case "transition-timing-function":
				CSSItem[CSSStyleItem.TextFormatKey.TransitionTimingFunction] = text2;
				break;
			case "unicode-bidi":
				CSSItem[CSSStyleItem.TextFormatKey.UnicodeBidi] = text2;
				break;
			case "vertical-align":
				CSSItem[CSSStyleItem.TextFormatKey.VerticalAlign] = text2;
				break;
			case "visibility":
				if (text2 == "visible")
				{
					CSSItem[CSSStyleItem.TextFormatKey.Visibility] = true;
				}
				else
				{
					CSSItem[CSSStyleItem.TextFormatKey.Visibility] = false;
				}
				break;
			case "width":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.Width] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.Width] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "word-spacing":
				if (text2.Contains("!important"))
				{
					text2 = text2.Substring(0, text2.IndexOf('!')).Trim();
					if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
					{
						CSSItem[CSSStyleItem.TextFormatImportantKey.WordSpacing] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
					}
				}
				else if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.WordSpacing] = Convert.ToSingle(ExtractValue(text2), CultureInfo.InvariantCulture);
				}
				break;
			case "word-wrap":
				CSSItem[CSSStyleItem.TextFormatKey.WordWrap] = text2;
				break;
			case "z-index":
				if (text2 != "auto" && text2 != "initial" && text2 != "inherit")
				{
					CSSItem[CSSStyleItem.TextFormatKey.Zindex] = Convert.ToInt32(text2);
				}
				break;
			}
		}
	}

	private void TraverseComments(XmlNode node)
	{
		string value = node.Value;
		if (StartsWithExt(value, "[if supportfields]") || StartsWithExt(value, "[if]"))
		{
			int num = 0;
			int count = value.IndexOf("<");
			value = value.Remove(0, count);
			if (value.EndsWith("![endif]"))
			{
				num = value.LastIndexOf(">");
			}
			value = value.Remove(num + 1);
			value = "<comment>" + value + "</comment>";
			value = ReplaceHtmlConstantByUnicodeChar(value);
			XmlNode xmlNode = null;
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				xmlNode = xmlDocument.DocumentElement;
			}
			catch
			{
				return;
			}
			if (xmlNode.LocalName.Equals("comment", StringComparison.OrdinalIgnoreCase))
			{
				TraverseChildNodes(xmlNode.ChildNodes);
			}
		}
	}

	private void InsertFieldBegin(string fieldCode)
	{
		WField wField = null;
		FieldType fieldType = FieldTypeDefiner.GetFieldType(fieldCode);
		wField = fieldType switch
		{
			FieldType.FieldMergeField => new WMergeField(m_currParagraph.Document), 
			FieldType.FieldIf => new WIfField(m_currParagraph.Document), 
			_ => new WField(m_currParagraph.Document), 
		};
		wField.FieldType = fieldType;
		m_currParagraph.Items.Add(wField);
		ApplyTextFormatting(wField.CharacterFormat);
		WTextRange wTextRange = new WTextRange(m_currParagraph.Document);
		wTextRange.Text = fieldCode;
		wTextRange.ApplyCharacterFormat(wField.CharacterFormat);
		m_currParagraph.Items.Add(wTextRange);
		FieldStack.Push(wField);
	}

	private void ParseFieldSeparator()
	{
		WFieldMark wFieldMark = new WFieldMark(m_currParagraph.Document, FieldMarkType.FieldSeparator);
		m_currParagraph.Items.Add(wFieldMark);
		CurrentField.FieldSeparator = wFieldMark;
	}

	private void ParseFieldEnd()
	{
		WFieldMark wFieldMark = new WFieldMark(m_currParagraph.Document, FieldMarkType.FieldEnd);
		m_currParagraph.Items.Add(wFieldMark);
		CurrentField.FieldEnd = wFieldMark;
		WField wField = FieldStack.Pop();
		wField.UpdateFieldCode(wField.GetFieldCodeForUnknownFieldType());
	}

	private void ParseFieldCode(FieldType fieldType, string fieldCode)
	{
		WField wField = null;
		WTextRange wTextRange = new WTextRange(m_currParagraph.Document);
		wTextRange.Text = fieldCode;
		wField = fieldType switch
		{
			FieldType.FieldMergeField => new WMergeField(m_currParagraph.Document), 
			FieldType.FieldIf => new WIfField(m_currParagraph.Document), 
			_ => new WField(m_currParagraph.Document), 
		};
		m_currParagraph.Items.Add(wField);
		m_currParagraph.Items.Add(wTextRange);
		FieldStack.Push(wField);
		wField.FieldType = fieldType;
		ParseFieldSeparator();
		ApplyTextFormatting(wField.CharacterFormat);
		wTextRange.ApplyCharacterFormat(wField.CharacterFormat);
		IsStyleFieldCode = true;
	}

	private void ParseField(XmlNode node)
	{
		string text = RemoveNewLineCharacter(node.InnerText);
		if (IsPreviousItemFieldStart)
		{
			if (text.Trim() != string.Empty)
			{
				InsertFieldBegin(text);
				IsPreviousItemFieldStart = false;
			}
		}
		else
		{
			if (CurrentField == null)
			{
				return;
			}
			if (IsStyleFieldCode)
			{
				IWTextRange iWTextRange = CurrentPara.AppendText(text);
				ApplyTextFormatting(iWTextRange.CharacterFormat);
				if (node.NextSibling == null)
				{
					ParseFieldEnd();
					IsStyleFieldCode = false;
				}
			}
			else
			{
				IWTextRange iWTextRange2 = CurrentPara.AppendText(text);
				ApplyTextFormatting(iWTextRange2.CharacterFormat);
			}
		}
	}

	private string RemoveNewLineCharacter(string text)
	{
		if (!CurrentFormat.IsPreserveWhiteSpace)
		{
			text = text.Replace(ControlChar.LineFeedChar, ' ').Replace('\r', ' ');
			if (text != " " && (m_styleStack.Count <= 0 || !m_styleStack.Peek().IsPreserveWhiteSpace) && !CurrentFormat.IsNonBreakingSpace)
			{
				text = m_removeSpaces.Replace(text, " ");
			}
		}
		text = text.Replace("nbsp;", '\u00a0'.ToString());
		if ((m_styleStack.Count <= 0 || !m_styleStack.Peek().IsPreserveWhiteSpace) && !CurrentFormat.IsNonBreakingSpace)
		{
			text = RemoveWhiteSpacesAtParagraphBegin(text, CurrentPara);
		}
		return text;
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}
}
