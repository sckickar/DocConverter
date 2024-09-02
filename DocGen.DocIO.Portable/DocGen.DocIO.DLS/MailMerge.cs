using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS.Entities;

namespace DocGen.DocIO.DLS;

public class MailMerge
{
	internal class GroupSelector
	{
		internal delegate void GroupFound(IRowsEnumerator rowsEnum);

		private TextBodySelection m_groupSelection;

		private TableRowSelection m_rowSelection;

		private WTextBody m_groupTextBody;

		private WTextBody m_body;

		private WMergeField m_beginGroupField;

		private WMergeField m_endGroupField;

		private int m_bodyItemEndIndex;

		private int m_bodyItemStartIndex = -1;

		private int m_paragraphItemEndIndex = -1;

		private int m_paragraphItemStartIndex = -1;

		private int m_rowIndex = -1;

		private int m_startRowIndex = -1;

		private string m_groupName;

		private GroupFound SendGroupFound;

		private IRowsEnumerator m_rowsEnum;

		private int m_selBodyItemsCnt = -1;

		private byte m_bFlags;

		private List<string> m_fieldNames;

		internal bool InsertAsNewRow
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

		internal TextBodySelection GroupSelection => m_groupSelection;

		internal TableRowSelection RowSelection => m_rowSelection;

		internal WMergeField BeginGroupField => m_beginGroupField;

		internal WMergeField EndGroupField
		{
			get
			{
				return m_endGroupField;
			}
			set
			{
				m_endGroupField = value;
			}
		}

		internal int BodyItemIndex
		{
			get
			{
				return m_bodyItemEndIndex;
			}
			set
			{
				m_bodyItemEndIndex = value;
			}
		}

		internal bool IsGroupFound => m_endGroupField != null;

		internal string GroupName => m_groupName;

		internal int SelectedBodyItemsCount => m_selBodyItemsCnt;

		internal List<string> FieldNames
		{
			get
			{
				if (m_fieldNames == null)
				{
					m_fieldNames = new List<string>();
				}
				return m_fieldNames;
			}
		}

		internal GroupSelector(GroupFound onGroupFound, bool insertAsNewRow)
		{
			InsertAsNewRow = insertAsNewRow;
			SendGroupFound = (GroupFound)Delegate.Combine(SendGroupFound, onGroupFound);
		}

		private void InitProcess(IRowsEnumerator rowsEnum)
		{
			m_groupSelection = null;
			m_rowSelection = null;
			m_beginGroupField = null;
			m_endGroupField = null;
			m_bodyItemEndIndex = 0;
			m_bodyItemStartIndex = -1;
			m_paragraphItemEndIndex = -1;
			m_paragraphItemStartIndex = -1;
			m_rowIndex = -1;
			m_selBodyItemsCnt = -1;
			m_rowsEnum = rowsEnum;
			m_groupName = m_rowsEnum.TableName;
			if (m_fieldNames != null)
			{
				m_fieldNames.Clear();
				m_fieldNames = null;
			}
		}

		internal void ProcessGroupsInNested(WTextBody body, IRowsEnumerator rowsEnum, int itemStart, int itemEnd)
		{
			InitProcess(rowsEnum);
			m_groupTextBody = (m_body = body);
			FindInBodyItemsInNested(m_body.Items, itemStart, itemEnd);
		}

		internal void ProcessGroups(WTextBody body, IRowsEnumerator rowsEnum)
		{
			InitProcess(rowsEnum);
			m_groupTextBody = (m_body = body);
			FindInBodyItems(m_body.Items);
		}

		internal void ProcessGroups(WTable table, int startRow, int endRow, IRowsEnumerator rowsEnum)
		{
			InitProcess(rowsEnum);
			FindInTable(table, startRow, endRow);
		}

		private void FindInBodyItemsInNested(BodyItemCollection bodyItems, int itemStart, int itemEnd)
		{
			int count = bodyItems.Count;
			int i = itemStart;
			for (int num = itemEnd; i <= num; i++)
			{
				bool flag = false;
				TextBodyItem textBodyItem = bodyItems[i];
				m_bodyItemEndIndex = i;
				switch (textBodyItem.EntityType)
				{
				case EntityType.Paragraph:
				{
					WParagraph wParagraph = (WParagraph)textBodyItem;
					for (int j = 0; j < wParagraph.Items.Count; j++)
					{
						ParagraphItem paragraphItem = wParagraph.Items[j];
						m_paragraphItemEndIndex = j;
						if (paragraphItem is WTextBox)
						{
							m_bodyItemEndIndex = 0;
							FindInBodyItems((paragraphItem as WTextBox).TextBoxBody.Items);
							m_bodyItemEndIndex = i;
						}
						else if (paragraphItem is Shape)
						{
							m_bodyItemEndIndex = 0;
							FindInBodyItems((paragraphItem as Shape).TextBody.Items);
							m_bodyItemEndIndex = i;
						}
						else
						{
							CheckItem(paragraphItem);
						}
						if (!IsGroupFound)
						{
							continue;
						}
						if (m_groupSelection != null)
						{
							i = m_groupSelection.ItemEndIndex;
							if (m_groupSelection.ItemStartIndex == m_groupSelection.ItemEndIndex)
							{
								j = m_groupSelection.ParagraphItemEndIndex;
							}
							num += bodyItems.Count - count;
							flag = true;
							ClearSelection();
							continue;
						}
						if (m_rowSelection.StartRowIndex == m_rowSelection.EndRowIndex)
						{
							num += bodyItems.Count - count;
						}
						break;
					}
					break;
				}
				case EntityType.Table:
				{
					WTable wTable = (WTable)textBodyItem;
					FindInTable(wTable, 0, wTable.Rows.Count - 1);
					break;
				}
				case EntityType.BlockContentControl:
				{
					WTextBody textBody = ((BlockContentControl)textBodyItem).TextBody;
					FindInBodyItemsInNested(textBody.Items, textBody.Items.FirstItem.Index, textBody.Items.LastItem.Index);
					break;
				}
				default:
					throw new Exception();
				case EntityType.AlternateChunk:
					break;
				}
				if (flag)
				{
					break;
				}
			}
		}

		private void FindInBodyItems(BodyItemCollection bodyItems)
		{
			int i = 0;
			for (int count = bodyItems.Count; i < count; i++)
			{
				TextBodyItem textBodyItem = bodyItems[i];
				m_bodyItemEndIndex = i;
				switch (textBodyItem.EntityType)
				{
				case EntityType.Paragraph:
				{
					WParagraph wParagraph = (WParagraph)textBodyItem;
					for (int j = 0; j < wParagraph.Items.Count; j++)
					{
						ParagraphItem paragraphItem = wParagraph.Items[j];
						m_paragraphItemEndIndex = j;
						if (paragraphItem is WTextBox)
						{
							m_bodyItemEndIndex = 0;
							FindInBodyItems((paragraphItem as WTextBox).TextBoxBody.Items);
							m_bodyItemEndIndex = i;
						}
						else if (paragraphItem is Shape)
						{
							m_bodyItemEndIndex = 0;
							FindInBodyItems((paragraphItem as Shape).TextBody.Items);
							m_bodyItemEndIndex = i;
						}
						else
						{
							CheckItem(paragraphItem);
						}
						if (!IsGroupFound)
						{
							continue;
						}
						if (m_groupSelection != null)
						{
							i = m_groupSelection.ItemEndIndex;
							if (m_groupSelection.ItemStartIndex == m_groupSelection.ItemEndIndex)
							{
								j = m_groupSelection.ParagraphItemEndIndex;
							}
							count = bodyItems.Count;
							ClearSelection();
							continue;
						}
						if (m_rowSelection.StartRowIndex == m_rowSelection.EndRowIndex)
						{
							count = bodyItems.Count;
						}
						break;
					}
					break;
				}
				case EntityType.Table:
				{
					WTable wTable = (WTable)textBodyItem;
					FindInTable(wTable, 0, wTable.Rows.Count - 1);
					break;
				}
				case EntityType.BlockContentControl:
				{
					BlockContentControl blockContentControl = (BlockContentControl)textBodyItem;
					FindInBodyItems(blockContentControl.TextBody.Items);
					break;
				}
				default:
					throw new Exception();
				case EntityType.AlternateChunk:
					break;
				}
			}
		}

		private void FindInTable(WTable table, int startRow, int endRow)
		{
			int count = table.Rows.Count;
			for (int i = startRow; i <= endRow; i++)
			{
				WTableRow wTableRow = table.Rows[i];
				m_rowIndex = i;
				int j = 0;
				for (int count2 = wTableRow.Cells.Count; j < count2; j++)
				{
					WTableCell wTableCell = wTableRow.Cells[j];
					FindInBodyItems(wTableCell.Items);
					if (IsGroupFound)
					{
						endRow += table.Rows.Count - count;
						count = table.Rows.Count;
						i = m_rowSelection.StartRowIndex;
						ClearSelection();
						break;
					}
				}
			}
		}

		private void ClearSelection()
		{
			m_groupSelection = null;
			m_rowSelection = null;
			m_beginGroupField = null;
			m_endGroupField = null;
			if (m_fieldNames != null)
			{
				m_fieldNames.Clear();
				m_fieldNames = null;
			}
		}

		private void CheckItem(ParagraphItem item)
		{
			if (item.EntityType != EntityType.MergeField)
			{
				return;
			}
			WMergeField wMergeField = item as WMergeField;
			if (m_beginGroupField != null)
			{
				if (wMergeField.Prefix != string.Empty)
				{
					if (wMergeField.FieldName != m_groupName)
					{
						FieldNames.Add(wMergeField.Prefix + ":" + wMergeField.FieldName);
					}
				}
				else
				{
					FieldNames.Add(wMergeField.FieldName);
				}
			}
			if (!(wMergeField.FieldName == m_groupName))
			{
				return;
			}
			if (m_beginGroupField == null)
			{
				if (IsBeginGroup(wMergeField))
				{
					StartSelection(wMergeField);
				}
			}
			else if (IsEndGroup(wMergeField))
			{
				EndSelection(wMergeField);
				if (SendGroupFound != null)
				{
					SendGroupFound(m_rowsEnum);
				}
			}
		}

		private void StartSelection(WMergeField field)
		{
			m_beginGroupField = field;
			m_groupTextBody = field.OwnerParagraph.OwnerTextBody;
			m_bodyItemStartIndex = m_bodyItemEndIndex;
			m_paragraphItemStartIndex = m_paragraphItemEndIndex;
			m_startRowIndex = m_rowIndex;
		}

		private bool IsNeedToInsertAsNewRow(WTextBody textBody)
		{
			if (InsertAsNewRow && textBody.EntityType == EntityType.TableCell && textBody == m_groupTextBody && IsRowContainsSingleCell(textBody as WTableCell, (textBody as WTableCell).OwnerRow))
			{
				return true;
			}
			return false;
		}

		private bool IsRowContainsSingleCell(WTableCell cell, WTableRow ownerRow)
		{
			if (ownerRow.Cells.Count == 1)
			{
				return true;
			}
			if (cell.Index != 0 || cell.CellFormat.HorizontalMerge != CellMerge.Start)
			{
				return false;
			}
			for (int i = 1; i < ownerRow.Cells.Count; i++)
			{
				if (ownerRow.Cells[i].CellFormat.HorizontalMerge != CellMerge.Continue)
				{
					return false;
				}
			}
			return true;
		}

		private void EndSelection(WMergeField field)
		{
			m_endGroupField = field;
			WTextBody ownerTextBody = field.OwnerParagraph.OwnerTextBody;
			if (field.FieldEnd != null)
			{
				m_bodyItemEndIndex = field.FieldEnd.OwnerParagraph.GetIndexInOwnerCollection();
				m_paragraphItemEndIndex = field.FieldEnd.GetIndexInOwnerCollection();
			}
			m_selBodyItemsCnt = m_bodyItemEndIndex - m_bodyItemStartIndex + 1;
			if (IsNeedToInsertAsNewRow(ownerTextBody))
			{
				m_rowIndex = (ownerTextBody as WTableCell).OwnerRow.Index;
				m_rowSelection = new TableRowSelection(ownerTextBody.Owner.Owner as WTable, m_startRowIndex, m_rowIndex);
				return;
			}
			if (ownerTextBody == m_groupTextBody)
			{
				m_groupSelection = new TextBodySelection(ownerTextBody, m_bodyItemStartIndex, m_bodyItemEndIndex, m_paragraphItemStartIndex, m_paragraphItemEndIndex);
				return;
			}
			if (ownerTextBody.EntityType == EntityType.TableCell && m_groupTextBody.EntityType == EntityType.TableCell && (m_groupTextBody.Owner as WTableRow).OwnerTable == (ownerTextBody.Owner as WTableRow).OwnerTable)
			{
				m_rowIndex = (ownerTextBody as WTableCell).OwnerRow.GetRowIndex();
				UpdateEndSelection(ownerTextBody as WTableCell);
				m_rowSelection = new TableRowSelection(ownerTextBody.Owner.Owner as WTable, m_startRowIndex, m_rowIndex);
				return;
			}
			throw new MailMergeException();
		}

		private void UpdateEndSelection(WTableCell cell)
		{
			WTableRow wTableRow = cell.OwnerRow;
			bool flag = false;
			foreach (WTableCell cell2 in wTableRow.Cells)
			{
				_ = cell2;
				if (cell.CellFormat.VerticalMerge != 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			while (wTableRow.NextSibling != null)
			{
				wTableRow = wTableRow.NextSibling as WTableRow;
				flag = false;
				foreach (WTableCell cell3 in wTableRow.Cells)
				{
					_ = cell3;
					if (cell.CellFormat.VerticalMerge != 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					m_rowIndex++;
					continue;
				}
				break;
			}
		}
	}

	internal class TableRowSelection
	{
		internal WTable Table;

		internal int StartRowIndex;

		internal int EndRowIndex;

		internal TableRowSelection(WTable table, int startRowIndex, int endRowIndex)
		{
			Table = table;
			StartRowIndex = startRowIndex;
			EndRowIndex = endRowIndex;
			ValidateIndexes();
		}

		private void ValidateIndexes()
		{
			if (StartRowIndex < 0 || StartRowIndex >= Table.Rows.Count)
			{
				throw new ArgumentOutOfRangeException("StartRowIndex");
			}
			if (EndRowIndex < 0 || EndRowIndex >= Table.Rows.Count)
			{
				throw new ArgumentOutOfRangeException("EndRowIndex");
			}
		}
	}

	private WordDocument m_doc;

	private GroupSelector m_groupSelector;

	private WSectionCollection m_contentSections;

	private string[] m_names;

	private string[] m_values;

	private byte m_bFlags = 1;

	private MailMergeSettings m_settings;

	private bool m_startAtNewPage;

	private DbConnection m_conn;

	private DataSet m_curDataSet;

	private ArrayList m_commands;

	private DataSet m_dataSet;

	private Dictionary<string, IRowsEnumerator> m_nestedEnums;

	private Regex m_varCmdRegex;

	private Stack<GroupSelector> m_groupSelectors;

	private Dictionary<string, string> m_mappedFields;

	private MailMergeDataSet m_dataSetDocIO;

	private List<DictionaryEntry> m_commandsDocIO;

	private MailMergeDataSet m_curDataSetDocIO;

	private int m_mergedRecordCount;

	private Dictionary<string, bool> m_clearFieldsState;

	private Stack<WIfField> m_IfFieldCollections;

	private WMergeField _previousMergeField;

	private bool _isInValidNextField;

	private bool IsSqlConnection
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

	private bool IsBeginGroupFound
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

	private bool IsEndGroupFound
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

	private bool IsNested
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	public bool ClearFields
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

	public bool StartAtNewPage
	{
		get
		{
			return m_startAtNewPage;
		}
		set
		{
			m_startAtNewPage = value;
		}
	}

	protected WordDocument Document => m_doc;

	public bool RemoveEmptyParagraphs
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

	public bool RemoveEmptyGroup
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public bool InsertAsNewRow
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFF7Fu) | ((value ? 1u : 0u) << 7));
			if (m_groupSelector != null)
			{
				m_groupSelector.InsertAsNewRow = InsertAsNewRow;
			}
		}
	}

	private Dictionary<string, IRowsEnumerator> NestedEnums
	{
		get
		{
			if (m_nestedEnums == null)
			{
				m_nestedEnums = new Dictionary<string, IRowsEnumerator>();
			}
			return m_nestedEnums;
		}
	}

	private DataSet CurrentDataSet
	{
		get
		{
			if (m_curDataSet == null)
			{
				m_curDataSet = new DataSet();
			}
			return m_curDataSet;
		}
	}

	private Regex VariableCommandRegex
	{
		get
		{
			if (m_varCmdRegex == null)
			{
				m_varCmdRegex = new Regex("%([^\"%]+)%");
			}
			return m_varCmdRegex;
		}
	}

	private Stack<GroupSelector> GroupSelectors
	{
		get
		{
			if (m_groupSelectors == null)
			{
				m_groupSelectors = new Stack<GroupSelector>();
			}
			return m_groupSelectors;
		}
	}

	public Dictionary<string, string> MappedFields
	{
		get
		{
			if (m_mappedFields == null)
			{
				m_mappedFields = new Dictionary<string, string>();
			}
			return m_mappedFields;
		}
	}

	private MailMergeDataSet CurrentDataSetDocIO
	{
		get
		{
			if (m_curDataSetDocIO == null)
			{
				m_curDataSetDocIO = new MailMergeDataSet();
			}
			return m_curDataSetDocIO;
		}
	}

	private Dictionary<string, bool> ClearFieldsState
	{
		get
		{
			if (m_clearFieldsState == null)
			{
				m_clearFieldsState = new Dictionary<string, bool>();
			}
			return m_clearFieldsState;
		}
	}

	public MailMergeSettings Settings
	{
		get
		{
			if (m_settings == null)
			{
				m_settings = new MailMergeSettings();
			}
			return m_settings;
		}
	}

	public event MergeFieldEventHandler MergeField;

	public event MergeImageFieldEventHandler MergeImageField;

	public event BeforeClearFieldEventHandler BeforeClearField;

	public event BeforeClearGroupFieldEventHandler BeforeClearGroupField;

	internal MailMerge(WordDocument document)
	{
		m_doc = document;
		m_contentSections = new WSectionCollection();
		m_groupSelector = new GroupSelector(OnGroupFound, InsertAsNewRow);
	}

	public void Execute(string[] fieldNames, string[] fieldValues)
	{
		Document.IsMailMerge = true;
		if (fieldNames == null)
		{
			throw new ArgumentNullException("fieldNames");
		}
		if (fieldValues == null)
		{
			throw new ArgumentNullException("fieldValues");
		}
		m_names = fieldNames;
		m_values = fieldValues;
		if (m_names.Length != 0)
		{
			IWSection iWSection = null;
			for (int i = 0; i < Document.Sections.Count; i++)
			{
				iWSection = Document.Sections[i];
				ExecuteForSection(iWSection, null);
			}
		}
		Document.IsMailMerge = false;
	}

	public void Execute(IEnumerable dataSource)
	{
		if (dataSource == null)
		{
			throw new ArgumentNullException("datasource");
		}
		MailMergeDataTable dataSource2 = new MailMergeDataTable(string.Empty, dataSource);
		ExecuteGroup(dataSource2);
	}

	public void ExecuteGroup(MailMergeDataTable dataSource)
	{
		if (dataSource == null)
		{
			throw new ArgumentNullException("datasource");
		}
		if (dataSource.GroupName == string.Empty)
		{
			Execute(new DataTableEnumerator(dataSource));
		}
		else
		{
			ExecuteGroup(new DataTableEnumerator(dataSource));
		}
	}

	public void Execute(DataRow row)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		Execute(new DataTableEnumerator(row));
	}

	public void Execute(DataTable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		Execute(new DataTableEnumerator(table));
	}

	public void Execute(DataView dataView)
	{
		if (dataView == null)
		{
			throw new ArgumentNullException("dataView");
		}
		Execute(new DataViewEnumerator(dataView));
	}

	public void ExecuteGroup(DataTable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		ExecuteGroup(new DataTableEnumerator(table));
	}

	public void ExecuteGroup(DataView dataView)
	{
		if (dataView == null)
		{
			throw new ArgumentNullException("dataView");
		}
		ExecuteGroup(new DataViewEnumerator(dataView));
	}

	public void ExecuteNestedGroup(DataSet dataSet, ArrayList commands)
	{
		if (dataSet == null)
		{
			throw new ArgumentException("dataSet");
		}
		if (commands == null)
		{
			throw new ArgumentException("commands");
		}
		RemoveSpellChecking();
		m_dataSet = dataSet.Copy();
		m_commands = commands;
		DictionaryEntry dictionaryEntry = (DictionaryEntry)commands[0];
		Document.IsMailMerge = true;
		IsNested = true;
		ExecuteNestedGroup((string)dictionaryEntry.Key);
		if (m_nestedEnums != null)
		{
			m_nestedEnums.Clear();
			m_nestedEnums = null;
		}
		if (m_dataSet != null)
		{
			m_dataSet.Clear();
			m_dataSet = null;
		}
		Document.IsMailMerge = false;
		IsNested = false;
	}

	public void ExecuteNestedGroup(MailMergeDataTable dataTable)
	{
		if (dataTable == null)
		{
			throw new ArgumentException("dataset");
		}
		RemoveSpellChecking();
		m_dataSetDocIO = new MailMergeDataSet();
		m_dataSetDocIO.Add(dataTable);
		Document.IsMailMerge = true;
		IsNested = true;
		ExecuteNestedGroup(dataTable.GroupName);
		if (m_nestedEnums != null)
		{
			m_nestedEnums.Clear();
			m_nestedEnums = null;
		}
		if (m_dataSetDocIO != null)
		{
			m_dataSetDocIO.Clear();
			m_dataSetDocIO = null;
		}
		Document.IsMailMerge = false;
		IsNested = false;
	}

	public void ExecuteNestedGroup(MailMergeDataSet dataSource, List<DictionaryEntry> commands)
	{
		if (dataSource == null || dataSource.DataSet.Count == 0)
		{
			throw new ArgumentException("dataSet is empty");
		}
		if (commands == null || commands.Count == 0)
		{
			throw new ArgumentException("commands list is empty");
		}
		RemoveSpellChecking();
		m_dataSetDocIO = dataSource;
		m_commandsDocIO = commands;
		DictionaryEntry dictionaryEntry = commands[0];
		Document.IsMailMerge = true;
		IsNested = true;
		ExecuteNestedGroup((string)dictionaryEntry.Key);
		if (m_nestedEnums != null)
		{
			m_nestedEnums.Clear();
			m_nestedEnums = null;
		}
		if (m_dataSetDocIO != null)
		{
			m_dataSetDocIO.Clear();
			m_dataSetDocIO = null;
		}
		m_commandsDocIO.Clear();
		m_commandsDocIO = null;
		Document.IsMailMerge = false;
		IsNested = false;
	}

	public string[] GetMergeFieldNames()
	{
		List<string> list = new List<string>();
		GetMergeFieldNamesImpl(list, null);
		return list.ToArray();
	}

	public string[] GetMergeFieldNames(string groupName)
	{
		List<string> list = new List<string>();
		GetMergeFieldNamesImpl(list, groupName);
		return list.ToArray();
	}

	public string[] GetMergeGroupNames()
	{
		List<string> list = null;
		List<string> list2 = null;
		List<string> list3 = new List<string>();
		Stack<EntityEntry> stack = new Stack<EntityEntry>();
		stack.Push(new EntityEntry(Document));
		do
		{
			EntityEntry entityEntry = stack.Peek();
			if (entityEntry.Current != null && entityEntry.Current.IsComposite)
			{
				ICompositeEntity compositeEntity = entityEntry.Current as ICompositeEntity;
				if (compositeEntity.ChildEntities.Count > 0)
				{
					stack.Push(new EntityEntry(compositeEntity.ChildEntities[0]));
					continue;
				}
			}
			if (entityEntry.Current != null && entityEntry.Current.EntityType == EntityType.MergeField)
			{
				WMergeField wMergeField = entityEntry.Current as WMergeField;
				if (IsBeginGroup(wMergeField))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(wMergeField.FieldName);
				}
				else if (IsEndGroup(wMergeField))
				{
					if (list2 == null)
					{
						list2 = new List<string>();
					}
					list2.Add(wMergeField.FieldName);
				}
			}
			while (!entityEntry.Fetch())
			{
				stack.Pop();
				if (stack.Count == 0)
				{
					break;
				}
				entityEntry = stack.Peek();
			}
		}
		while (stack.Count > 0);
		if (list != null && list2 != null)
		{
			foreach (string item in list)
			{
				foreach (string item2 in list2)
				{
					if (item == item2)
					{
						list3.Add(item);
						list2.Remove(item2);
						break;
					}
				}
			}
		}
		return list3.ToArray();
	}

	private void GetMergeFieldNamesImpl(List<string> fieldsArray, string groupName)
	{
		WSection wSection = null;
		TextBodyItem textBodyItem = null;
		int i = 0;
		for (int count = Document.Sections.Count; i < count; i++)
		{
			wSection = Document.Sections[i];
			int j = 0;
			for (int count2 = wSection.Body.Items.Count; j < count2; j++)
			{
				textBodyItem = wSection.Body.Items[j];
				GetFieldNamesForParagraph(fieldsArray, textBodyItem, groupName);
			}
			for (int k = 0; k < 6; k++)
			{
				int l = 0;
				for (int count3 = wSection.HeadersFooters[k].Items.Count; l < count3; l++)
				{
					textBodyItem = wSection.HeadersFooters[k].Items[l];
					GetFieldNamesForParagraph(fieldsArray, textBodyItem, groupName);
				}
			}
		}
	}

	internal void Close()
	{
		m_doc = null;
		if (m_contentSections != null)
		{
			m_contentSections.Close();
			m_contentSections = null;
		}
		if (m_names != null)
		{
			m_names = null;
		}
		if (m_values != null)
		{
			m_values = null;
		}
		if (m_conn != null)
		{
			m_conn = null;
		}
		if (m_curDataSet != null)
		{
			m_curDataSet = null;
		}
		if (m_commands != null)
		{
			m_commands = null;
		}
		if (m_dataSet != null)
		{
			m_dataSet = null;
		}
		if (m_nestedEnums != null)
		{
			m_nestedEnums.Clear();
			m_nestedEnums = null;
		}
		if (m_varCmdRegex != null)
		{
			m_varCmdRegex = null;
		}
		if (m_groupSelectors != null)
		{
			m_groupSelectors.Clear();
			m_groupSelectors = null;
		}
		if (m_mappedFields != null)
		{
			m_mappedFields.Clear();
			m_mappedFields = null;
		}
		if (m_dataSetDocIO != null)
		{
			m_dataSetDocIO = null;
		}
		if (m_commandsDocIO != null)
		{
			m_commandsDocIO = null;
		}
		if (m_curDataSetDocIO != null)
		{
			m_curDataSetDocIO = null;
		}
		if (m_clearFieldsState != null)
		{
			m_clearFieldsState.Clear();
			m_clearFieldsState = null;
		}
		if (m_settings != null)
		{
			m_settings.Close();
			m_settings = null;
		}
	}

	private void OnGroupFound(IRowsEnumerator rowsEnum)
	{
		bool flag = false;
		GroupSelector groupSelector = m_groupSelector;
		if (CheckRecordsCount(rowsEnum) && RemoveEmptyGroup)
		{
			flag = true;
			EmptyGroup(groupSelector);
		}
		else if (IsNested)
		{
			m_groupSelector.BeginGroupField.FieldName = string.Empty;
			m_groupSelector.EndGroupField.FieldName = string.Empty;
		}
		if ((flag || CheckSelection(rowsEnum)) && (!CheckRecordsCount(rowsEnum) || !RemoveEmptyGroup))
		{
			if (groupSelector.GroupSelection != null)
			{
				OnBodyGroupFound(rowsEnum);
			}
			else if (groupSelector.RowSelection != null)
			{
				OnRowGroupFound(rowsEnum);
			}
		}
	}

	private bool CheckRecordsCount(IRowsEnumerator rowsEnum)
	{
		if (rowsEnum is DataTableEnumerator)
		{
			if ((rowsEnum as DataTableEnumerator).m_MMtable != null)
			{
				if (!string.IsNullOrEmpty((rowsEnum as DataTableEnumerator).Command) || (rowsEnum as DataTableEnumerator).RowsCount <= 0)
				{
					if (!string.IsNullOrEmpty((rowsEnum as DataTableEnumerator).Command))
					{
						return (rowsEnum as DataTableEnumerator).MatchingRecordsCount <= 0;
					}
					return true;
				}
				return false;
			}
			return rowsEnum.RowsCount == 0;
		}
		return rowsEnum.RowsCount == 0;
	}

	private void EmptyGroup(GroupSelector gs)
	{
		if (gs.BeginGroupField.OwnerParagraph.IsInCell || gs.EndGroupField.OwnerParagraph.IsInCell)
		{
			if (gs.BeginGroupField.Prefix == "TableStart")
			{
				EmptyGroupInTable(gs);
			}
			else
			{
				EmptyGroupInTableCell(gs);
			}
		}
		else
		{
			EmptyGroupInTextbody(gs);
		}
		if (gs.GroupSelection != null)
		{
			gs.GroupSelection.ItemEndIndex = gs.GroupSelection.ItemStartIndex;
		}
		if (gs.RowSelection != null)
		{
			gs.RowSelection.EndRowIndex = gs.RowSelection.StartRowIndex;
		}
	}

	private void EmptyGroupInTextbody(GroupSelector gs)
	{
		if (!(gs.BeginGroupField.Owner is WParagraph) || gs.BeginGroupField.OwnerParagraph.OwnerTextBody == null || gs.EndGroupField.FieldEnd == null || !(gs.EndGroupField.FieldEnd.Owner is WParagraph) || gs.EndGroupField.FieldEnd.OwnerParagraph.OwnerTextBody == null)
		{
			return;
		}
		int indexInOwnerCollection = gs.BeginGroupField.GetIndexInOwnerCollection();
		WParagraph ownerParagraph = gs.BeginGroupField.OwnerParagraph;
		BookmarkStart bookmarkStart = new BookmarkStart(m_doc, "_fieldBookmark");
		BookmarkEnd bookmarkEnd = new BookmarkEnd(m_doc, "_fieldBookmark");
		ownerParagraph.Items.Insert(indexInOwnerCollection, bookmarkStart);
		gs.BeginGroupField.EnsureBookmarkStart(bookmarkStart);
		WParagraph ownerParagraph2 = gs.EndGroupField.FieldEnd.OwnerParagraph;
		int indexInOwnerCollection2 = gs.EndGroupField.FieldEnd.GetIndexInOwnerCollection();
		ownerParagraph2.Items.Insert(indexInOwnerCollection2 + 1, bookmarkEnd);
		gs.EndGroupField.EnsureBookmarkStart(bookmarkEnd);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(m_doc);
		bookmarksNavigator.MoveToBookmark("_fieldBookmark");
		bookmarksNavigator.RemoveEmptyParagraph = false;
		Document.IsSkipFieldDetach = true;
		bookmarksNavigator.DeleteBookmarkContent(saveFormatting: false);
		Document.IsSkipFieldDetach = false;
		if (ownerParagraph.Items.Contains(bookmarkStart))
		{
			ownerParagraph.Items.Remove(bookmarkStart);
		}
		if (ownerParagraph2.Items.Contains(bookmarkEnd))
		{
			ownerParagraph2.Items.Remove(bookmarkEnd);
		}
		if (ownerParagraph.ChildEntities.Count == 0 && ownerParagraph.Equals(ownerParagraph2))
		{
			ownerParagraph.OwnerTextBody.ChildEntities.Remove(ownerParagraph);
			return;
		}
		if (ownerParagraph.ChildEntities.Count == 0)
		{
			ownerParagraph.OwnerTextBody.ChildEntities.Remove(ownerParagraph);
		}
		if (ownerParagraph2.ChildEntities.Count == 0)
		{
			ownerParagraph2.OwnerTextBody.ChildEntities.Remove(ownerParagraph2);
		}
	}

	private void EmptyGroupInTable(GroupSelector gs)
	{
		if (!(gs.BeginGroupField.Owner is WParagraph) || gs.BeginGroupField.OwnerParagraph.OwnerTextBody == null || gs.EndGroupField.FieldEnd == null || !(gs.EndGroupField.FieldEnd.Owner is WParagraph) || gs.EndGroupField.FieldEnd.OwnerParagraph.OwnerTextBody == null)
		{
			return;
		}
		RemoveGoBackBookmark(gs);
		int indexInOwnerCollection = gs.BeginGroupField.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = gs.EndGroupField.FieldEnd.GetIndexInOwnerCollection();
		WTableCell obj = (gs.BeginGroupField.Owner as WParagraph).GetOwnerEntity() as WTableCell;
		WTableCell wTableCell = (gs.EndGroupField.FieldEnd.Owner as WParagraph).GetOwnerEntity() as WTableCell;
		int indexInOwnerCollection3 = gs.BeginGroupField.OwnerParagraph.GetIndexInOwnerCollection();
		int cellIndex = obj.GetCellIndex();
		int endCellIndex = wTableCell.GetCellIndex();
		int rowIndex = (obj.Owner as WTableRow).GetRowIndex();
		int endRowIndex = (wTableCell.Owner as WTableRow).GetRowIndex();
		int indexInOwnerCollection4 = gs.EndGroupField.FieldEnd.OwnerParagraph.GetIndexInOwnerCollection();
		bool num = rowIndex == endRowIndex;
		WTable wTable = gs.BeginGroupField.Owner.Owner.Owner.Owner as WTable;
		Document.IsSkipFieldDetach = true;
		RemoveItemsAfterTableStart(gs, wTable, indexInOwnerCollection, indexInOwnerCollection3, cellIndex, ref endCellIndex, rowIndex, ref endRowIndex, indexInOwnerCollection4, indexInOwnerCollection2);
		if (!num)
		{
			RemoveItemsAtTableEnd(gs, wTable, indexInOwnerCollection2, indexInOwnerCollection4, cellIndex, endCellIndex, rowIndex, endRowIndex);
		}
		else if (indexInOwnerCollection == 0 && indexInOwnerCollection3 == 0)
		{
			if (indexInOwnerCollection4 == wTable.Rows[rowIndex].Cells[cellIndex].Items.Count - 1 && indexInOwnerCollection2 == (wTable.Rows[rowIndex].Cells[cellIndex].Items[indexInOwnerCollection4] as WParagraph).Items.Count - 1)
			{
				wTable.Rows.RemoveAt(rowIndex);
			}
			else
			{
				RemoveItems(wTable.Rows[rowIndex].Cells[cellIndex].Items[indexInOwnerCollection4], 0, indexInOwnerCollection2);
				RemoveItems(wTable.Rows[rowIndex].Cells[cellIndex], 0, indexInOwnerCollection4);
			}
		}
		Document.IsSkipFieldDetach = false;
	}

	private void RemoveGoBackBookmark(GroupSelector gs)
	{
		for (int i = 0; i < gs.BeginGroupField.OwnerParagraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = gs.BeginGroupField.OwnerParagraph.Items[i];
			if ((paragraphItem is BookmarkStart && (paragraphItem as BookmarkStart).Name.ToLower() == "_goback") || (paragraphItem is BookmarkEnd && (paragraphItem as BookmarkEnd).Name.ToLower() == "_goback"))
			{
				paragraphItem.RemoveSelf();
				i--;
			}
		}
		for (int j = 0; j < gs.EndGroupField.OwnerParagraph.Items.Count; j++)
		{
			ParagraphItem paragraphItem2 = gs.EndGroupField.OwnerParagraph.Items[j];
			if ((paragraphItem2 is BookmarkStart && (paragraphItem2 as BookmarkStart).Name.ToLower() == "_goback") || (paragraphItem2 is BookmarkEnd && (paragraphItem2 as BookmarkEnd).Name.ToLower() == "_goback"))
			{
				paragraphItem2.RemoveSelf();
				j--;
			}
		}
	}

	private void RemoveItemsAfterTableStart(GroupSelector gs, WTable table, int startIndex, int startParaIndex, int startCellIndex, ref int endCellIndex, int startRowIndex, ref int endRowIndex, int endParaIndex, int endIndex)
	{
		if (startIndex == 0)
		{
			if (startParaIndex == 0)
			{
				if (startCellIndex == 0)
				{
					if (startRowIndex != endRowIndex)
					{
						RemoveItems(table, startRowIndex, endRowIndex);
						endRowIndex = startRowIndex;
					}
					else if (startCellIndex != endCellIndex)
					{
						RemoveItems(table.Rows[startRowIndex], startCellIndex, endCellIndex);
						endCellIndex = startCellIndex;
					}
				}
				else if (startRowIndex != endRowIndex)
				{
					if (startCellIndex == table.Rows[startRowIndex].Cells.Count - 1)
					{
						table.Rows[startRowIndex].Cells.RemoveAt(startCellIndex);
					}
					else
					{
						RemoveItems(table.Rows[startRowIndex], startCellIndex, table.Rows[startRowIndex].Cells.Count - 1);
					}
					if (startRowIndex + 1 < endRowIndex)
					{
						RemoveItems(table, startRowIndex + 1, endRowIndex);
						endRowIndex = startRowIndex + 1;
					}
				}
				else if (startCellIndex == endCellIndex)
				{
					table.Rows[startRowIndex].Cells.RemoveAt(startCellIndex);
				}
				else
				{
					RemoveItems(table.Rows[startRowIndex], startCellIndex, endCellIndex);
					endCellIndex = startCellIndex;
				}
			}
			else if (startRowIndex == endRowIndex && startCellIndex == endCellIndex)
			{
				WParagraph wParagraph = table.Rows[startRowIndex].Cells[startCellIndex].Paragraphs[endParaIndex];
				if (wParagraph.Items.Count - 1 == endIndex)
				{
					RemoveItems(table.Rows[startRowIndex].Cells[startCellIndex], startParaIndex, endParaIndex + 1);
					return;
				}
				RemoveItems(table.Rows[startRowIndex].Cells[startCellIndex], startParaIndex, endParaIndex);
				RemoveItems(wParagraph, 0, endIndex);
			}
			else
			{
				RemoveItems(table.Rows[startRowIndex].Cells[startCellIndex], startParaIndex, table.Rows[startRowIndex].Cells[startCellIndex].Items.Count);
			}
		}
		else
		{
			RemoveItems(table.Rows[startRowIndex].Cells[startCellIndex].Items[startParaIndex] as WParagraph, startIndex, (table.Rows[startRowIndex].Cells[startCellIndex].Items[startParaIndex] as WParagraph).Items.Count);
			RemoveItems(table.Rows[startRowIndex].Cells[startCellIndex], startParaIndex + 1, table.Rows[startRowIndex].Cells[startCellIndex].Items.Count);
			if (startRowIndex == endRowIndex)
			{
				RemoveItems(table.Rows[startRowIndex], startCellIndex + 1, endCellIndex);
				endCellIndex = startCellIndex + 1;
			}
			else
			{
				RemoveItems(table.Rows[startRowIndex], startCellIndex + 1, table.Rows[startRowIndex].Cells.Count);
			}
			RemoveItems(table, startRowIndex + 1, endRowIndex);
			endRowIndex = startRowIndex + 1;
		}
	}

	private void RemoveItemsAtTableEnd(GroupSelector gs, WTable table, int endIndex, int endParaIndex, int startCellIndex, int endCellIndex, int startRowIndex, int endRowIndex)
	{
		if (startRowIndex != endRowIndex && endIndex == (table.Rows[startRowIndex + 1].Cells[endCellIndex].Items[endParaIndex] as WParagraph).Items.Count - 1)
		{
			if (endParaIndex == table.Rows[startRowIndex + 1].Cells[endCellIndex].Items.Count - 1)
			{
				if (endCellIndex == table.Rows[startRowIndex + 1].Cells.Count - 1)
				{
					table.Rows.RemoveAt(startRowIndex + 1);
				}
				else
				{
					RemoveItems(table.Rows[startRowIndex + 1], 0, endCellIndex + 1);
				}
			}
			else
			{
				RemoveItems(table.Rows[startRowIndex + 1].Cells[endCellIndex], 0, endParaIndex + 1);
				RemoveItems(table.Rows[startRowIndex + 1], 0, endCellIndex);
			}
		}
		else
		{
			RemoveItems(table.Rows[endRowIndex].Cells[endCellIndex].Items[endParaIndex] as WParagraph, 0, endIndex + 1);
			RemoveItems(table.Rows[endRowIndex].Cells[endCellIndex], 0, endParaIndex);
			RemoveItems(table.Rows[endRowIndex], 0, endCellIndex);
		}
	}

	private void EmptyGroupInTableCell(GroupSelector gs)
	{
		if (!(gs.BeginGroupField.Owner is WParagraph) || gs.BeginGroupField.OwnerParagraph.OwnerTextBody == null || gs.EndGroupField.FieldEnd == null || !(gs.EndGroupField.FieldEnd.Owner is WParagraph) || gs.EndGroupField.FieldEnd.OwnerParagraph.OwnerTextBody == null)
		{
			return;
		}
		int indexInOwnerCollection = gs.BeginGroupField.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = gs.BeginGroupField.OwnerParagraph.GetIndexInOwnerCollection();
		int indexInOwnerCollection3 = gs.EndGroupField.FieldEnd.GetIndexInOwnerCollection();
		int indexInOwnerCollection4 = gs.EndGroupField.FieldEnd.OwnerParagraph.GetIndexInOwnerCollection();
		WTableCell wTableCell = gs.BeginGroupField.OwnerParagraph.GetOwnerEntity() as WTableCell;
		Document.IsSkipFieldDetach = true;
		if ((wTableCell.Items[indexInOwnerCollection2] as WParagraph).Items.Count > 1 && indexInOwnerCollection > 0)
		{
			RemoveItems(wTableCell.Items[indexInOwnerCollection2] as WParagraph, indexInOwnerCollection, (wTableCell.Items[indexInOwnerCollection2] as WParagraph).Items.Count);
			RemoveItems(wTableCell, indexInOwnerCollection2 + 1, indexInOwnerCollection4);
			if (indexInOwnerCollection3 == (wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph).Items.Count - 1)
			{
				wTableCell.Items.RemoveAt(indexInOwnerCollection2 + 1);
			}
			else if ((wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph).Items.Count > 0)
			{
				RemoveItems(wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph, 0, indexInOwnerCollection3);
			}
		}
		else
		{
			RemoveItems(wTableCell, indexInOwnerCollection2, indexInOwnerCollection4);
			indexInOwnerCollection4 = gs.EndGroupField.FieldEnd.OwnerParagraph.GetIndexInOwnerCollection();
			if (indexInOwnerCollection2 != indexInOwnerCollection4 && wTableCell.Items.Count > indexInOwnerCollection2 + 1)
			{
				if (indexInOwnerCollection3 == (wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph).Items.Count - 1)
				{
					wTableCell.Items.RemoveAt(indexInOwnerCollection2 + 1);
				}
				else if ((wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph).Items.Count > 0)
				{
					RemoveItems(wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph, 0, (wTableCell.Items[indexInOwnerCollection2 + 1] as WParagraph).Items.Count + 1);
				}
			}
			else if (indexInOwnerCollection3 == (wTableCell.Items[indexInOwnerCollection2] as WParagraph).Items.Count - 1)
			{
				wTableCell.Items.RemoveAt(indexInOwnerCollection2);
			}
			else if ((wTableCell.Items[indexInOwnerCollection2] as WParagraph).Items.Count > 0)
			{
				RemoveItems(wTableCell.Items[indexInOwnerCollection2] as WParagraph, 0, indexInOwnerCollection3 + 1);
			}
		}
		Document.IsSkipFieldDetach = false;
	}

	private void RemoveItems(Entity ent, int startIndex, int endIndex)
	{
		switch (ent.EntityType)
		{
		case EntityType.Paragraph:
		{
			for (int j = startIndex; j < endIndex; j++)
			{
				(ent as WParagraph).Items.RemoveAt(startIndex);
			}
			break;
		}
		case EntityType.TableCell:
		{
			for (int l = startIndex; l < endIndex; l++)
			{
				(ent as WTableCell).Items.RemoveAt(startIndex);
			}
			break;
		}
		case EntityType.TableRow:
		{
			for (int m = startIndex; m < endIndex; m++)
			{
				(ent as WTableRow).Cells.RemoveAt(startIndex);
			}
			break;
		}
		case EntityType.Table:
		{
			for (int k = startIndex; k < endIndex; k++)
			{
				(ent as WTable).Rows.RemoveAt(startIndex);
			}
			break;
		}
		case EntityType.TextBody:
		{
			for (int i = startIndex; i < endIndex; i++)
			{
				(ent as WTextBody).Items.RemoveAt(startIndex);
			}
			break;
		}
		}
	}

	private string[] GetTableCommand(string command)
	{
		return command?.Split(new char[1] { ' ' });
	}

	private void OnBodyGroupFound(IRowsEnumerator rowsEnum)
	{
		GroupSelector groupSelector = m_groupSelector;
		TextBodyPart textBodyPart = new TextBodyPart();
		TextBodySelection groupSelection = groupSelector.GroupSelection;
		textBodyPart.Copy(groupSelection);
		rowsEnum.Reset();
		int num = 0;
		RemoveBookMarks(textBodyPart);
		while (CheckNextRow(rowsEnum))
		{
			if (m_conn != null || m_dataSet != null || m_dataSetDocIO != null)
			{
				UpdateEnum(groupSelector.GroupName, rowsEnum);
			}
			int count = groupSelection.TextBody.Items.Count;
			WParagraph wParagraph = groupSelection.TextBody.Items[groupSelection.ItemEndIndex] as WParagraph;
			int num2 = ((groupSelection.TextBody.Items.Count - 1 >= groupSelection.ItemEndIndex && wParagraph != null) ? wParagraph.Items.Count : 0);
			ExecuteGroupForSelection(groupSelection.TextBody, groupSelection.ItemStartIndex, groupSelection.ItemEndIndex, groupSelection.ParagraphItemStartIndex, groupSelection.ParagraphItemEndIndex, rowsEnum);
			m_mergedRecordCount++;
			num++;
			groupSelection.ItemEndIndex += groupSelection.TextBody.Items.Count - count;
			if (rowsEnum.IsLast || (rowsEnum is DataTableEnumerator && num == (rowsEnum as DataTableEnumerator).MatchingRecordsCount))
			{
				if (IsNested)
				{
					NestedEnums.Remove(groupSelector.GroupName);
				}
				break;
			}
			WParagraph wParagraph2 = ((groupSelection.TextBody.Items.Count > groupSelection.ItemEndIndex) ? groupSelection.TextBody.Items[groupSelection.ItemEndIndex] : null) as WParagraph;
			if (wParagraph2 != null)
			{
				if (wParagraph2.Items.Count > 0)
				{
					groupSelection.ParagraphItemEndIndex = groupSelection.ParagraphItemEndIndex + 1 - (num2 - wParagraph2.Items.Count);
				}
				else
				{
					groupSelection.ParagraphItemEndIndex = wParagraph2.Items.Count;
				}
			}
			if (groupSelection.ItemStartIndex == groupSelection.ItemEndIndex)
			{
				InsertPageBreak(groupSelection);
				textBodyPart.PasteAt(groupSelection.TextBody, groupSelection.ItemEndIndex, groupSelection.ParagraphItemEndIndex);
				groupSelection.ParagraphItemStartIndex = groupSelection.ParagraphItemEndIndex;
				for (int num3 = wParagraph2.Items.Count - 1; num3 >= 0; num3--)
				{
					if (wParagraph2.Items[num3] is WFieldMark && (wParagraph2.Items[num3] as WFieldMark).Type == FieldMarkType.FieldEnd && (wParagraph2.Items[num3] as WFieldMark).ParentField is WMergeField)
					{
						string prefix = string.Empty;
						string nameOfField = string.Empty;
						WMergeField wMergeField = (wParagraph2.Items[num3] as WFieldMark).ParentField as WMergeField;
						string fieldvalue = ((!string.IsNullOrEmpty(wMergeField.FormattingString)) ? wMergeField.FieldCode.Replace(wMergeField.FormattingString, string.Empty) : wMergeField.FieldCode);
						string fieldName = wMergeField.GetFieldValues(fieldvalue)[0];
						wMergeField.ParseFieldName(fieldName, ref prefix, ref nameOfField);
						if (nameOfField == m_groupSelector.GroupName)
						{
							groupSelection.ParagraphItemEndIndex = num3;
							break;
						}
					}
				}
			}
			else
			{
				InsertPageBreak(groupSelection);
				textBodyPart.PasteAt(groupSelection.TextBody, groupSelection.ItemEndIndex, groupSelection.ParagraphItemEndIndex);
				groupSelection.ItemStartIndex = groupSelection.ItemEndIndex;
				groupSelection.ItemEndIndex += textBodyPart.BodyItems.Count - 1;
				groupSelection.ParagraphItemStartIndex = groupSelection.ParagraphItemEndIndex;
				wParagraph2 = ((textBodyPart.BodyItems[textBodyPart.BodyItems.Count - 1] is WParagraph) ? (textBodyPart.BodyItems[textBodyPart.BodyItems.Count - 1] as WParagraph) : null);
				if (wParagraph2 != null)
				{
					groupSelection.ParagraphItemEndIndex = wParagraph2.Items.Count - 1;
				}
			}
			Document.Settings.DuplicateListStyleNames = string.Empty;
		}
	}

	private void InsertPageBreak(TextBodySelection bodySel)
	{
		if (StartAtNewPage)
		{
			WParagraph obj = bodySel.TextBody.Items[bodySel.ItemEndIndex] as WParagraph;
			obj.RemoveEmpty = false;
			Break entity = new Break(Document, BreakType.PageBreak);
			obj.ChildEntities.Insert(bodySel.ParagraphItemEndIndex, entity);
			bodySel.ParagraphItemEndIndex++;
		}
	}

	private void RemoveBookMarks(TextBodyPart txtBodyPart)
	{
		for (int i = 0; i < txtBodyPart.BodyItems.Count; i++)
		{
			DeleteBoookmarks(txtBodyPart.BodyItems[i]);
		}
	}

	private void DeleteBoookmarks(IEntity entity)
	{
		if (entity.IsComposite)
		{
			for (int num = (entity as ICompositeEntity).ChildEntities.Count - 1; num >= 0; num--)
			{
				DeleteBoookmarks((entity as ICompositeEntity).ChildEntities[num]);
			}
		}
		else if (entity.EntityType == EntityType.BookmarkStart || entity.EntityType == EntityType.BookmarkEnd)
		{
			(entity.Owner as WParagraph).Items.Remove(entity);
		}
	}

	private void OnRowGroupFound(IRowsEnumerator rowsEnum)
	{
		GroupSelector groupSelector = m_groupSelector;
		WTable table = groupSelector.RowSelection.Table;
		int startRowIndex = groupSelector.RowSelection.StartRowIndex;
		int endRowIndex = groupSelector.RowSelection.EndRowIndex;
		_ = table.Rows.Count;
		int num = startRowIndex;
		int num2 = 0;
		if (IsNested)
		{
			VerifyNestedGroups(startRowIndex, endRowIndex, table);
		}
		int num3 = endRowIndex - startRowIndex + 1;
		WTableRow[] array = new WTableRow[num3];
		int num4 = 0;
		for (int i = startRowIndex; i <= endRowIndex; i++)
		{
			array[num4] = table.Rows[i].Clone();
			num4++;
		}
		rowsEnum.Reset();
		int num5 = 0;
		while (CheckNextRow(rowsEnum))
		{
			if (m_conn != null || m_dataSet != null || m_dataSetDocIO != null)
			{
				UpdateEnum(groupSelector.GroupName, rowsEnum);
			}
			num2 = ExecuteGroupForRowSelection(table, num, num3, rowsEnum);
			num5++;
			m_mergedRecordCount++;
			if (rowsEnum.IsLast || (rowsEnum is DataTableEnumerator && num5 == (rowsEnum as DataTableEnumerator).MatchingRecordsCount))
			{
				if (IsNested)
				{
					NestedEnums.Remove(groupSelector.GroupName);
				}
				num += num2 - 1;
				break;
			}
			num += num2;
			for (int j = 0; j < num3; j++)
			{
				table.Rows.Insert(num + j, array[j].Clone());
			}
		}
		groupSelector.RowSelection.StartRowIndex = num;
	}

	private void ExecuteGroup(IRowsEnumerator rowsEnum)
	{
		Document.IsMailMerge = true;
		RemoveSpellChecking();
		WSection wSection = null;
		int i = 0;
		for (int count = Document.Sections.Count; i < count; i++)
		{
			wSection = Document.Sections[i];
			ExecuteGroup(wSection, rowsEnum);
		}
		Document.IsMailMerge = false;
	}

	private void ExecuteGroup(WSection section, IRowsEnumerator rowsEnum)
	{
		m_groupSelector.ProcessGroups(section.Body, rowsEnum);
		for (int i = 0; i < 6; i++)
		{
			WTextBody wTextBody = section.HeadersFooters[i];
			if (wTextBody.Items.Count > 0)
			{
				m_groupSelector.ProcessGroups(wTextBody, rowsEnum);
			}
		}
	}

	private void ExecuteGroupForSelection(WTextBody textBody, int itemStart, int itemEnd, int pItemStart, int pItemEnd, IRowsEnumerator rowsEnum)
	{
		if (itemEnd < 0)
		{
			itemEnd = textBody.Items.Count - 1;
		}
		int i = itemStart;
		for (int num = itemEnd; i <= num && i < textBody.Items.Count; i++)
		{
			TextBodyItem textBodyItem = textBody.Items[i];
			switch (textBodyItem.EntityType)
			{
			case EntityType.Table:
			{
				WTable wTable = textBodyItem as WTable;
				for (int k = 0; k < wTable.Rows.Count; k++)
				{
					ExecuteGroupForRowSelection(wTable, k, 1, rowsEnum);
				}
				break;
			}
			case EntityType.BlockContentControl:
			{
				WTextBody textBody2 = ((BlockContentControl)textBodyItem).TextBody;
				ExecuteGroupForSelection(textBody2, textBody2.Items.FirstItem.Index, textBody2.Items.LastItem.Index, 0, -1, rowsEnum);
				break;
			}
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				int num2 = ((i == itemStart) ? pItemStart : 0);
				int num3 = ((i == num && pItemEnd > -1) ? pItemEnd : (wParagraph.Items.Count - 1));
				int count = wParagraph.Items.Count;
				for (int j = num2; j <= num3; j++)
				{
					if (wParagraph.DeepDetached)
					{
						break;
					}
					if (wParagraph.Items.Count > j && wParagraph.Items[j].EntityType == EntityType.TextBox)
					{
						WTextBox wTextBox = wParagraph.Items[j] as WTextBox;
						ExecuteGroupForSelection(wTextBox.TextBoxBody, 0, -1, 0, -1, rowsEnum);
						continue;
					}
					if (wParagraph.Items.Count > j && wParagraph.Items[j].EntityType == EntityType.AutoShape)
					{
						Shape shape = wParagraph.Items[j] as Shape;
						ExecuteGroupForSelection(shape.TextBody, 0, -1, 0, -1, rowsEnum);
						continue;
					}
					WField wField = ((wParagraph.Items.Count > j) ? (wParagraph.Items[j] as WField) : null);
					if (wField == null)
					{
						continue;
					}
					if (wField is WMergeField)
					{
						(wField as WMergeField).UpdateFieldMarks();
						if (!(wField.Owner is WParagraph) || wField.FieldEnd == null || !(wField.FieldEnd.Owner is WParagraph))
						{
							continue;
						}
						WMergeField wMergeField = wField as WMergeField;
						if (!IsBeginGroup(wMergeField) && !IsEndGroup(wMergeField))
						{
							if (!((!wMergeField.Prefix.StartsWith("Image")) ? UpdateMergeFieldValue(wMergeField, rowsEnum) : UpdateImageMergeFieldValue(wMergeField, rowsEnum)))
							{
								continue;
							}
							j--;
							num3 = UpdateEndIndex(ref count, wParagraph, num3);
							if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
							{
								if (wParagraph.Owner is WTableCell wTableCell && (wTableCell.ChildEntities.Count == 1 || (wParagraph == wTableCell.LastParagraph && IsCellHasValidContent(wTableCell))))
								{
									wParagraph.ChildEntities.Clear();
								}
								else
								{
									wParagraph.RemoveEmpty = true;
								}
							}
						}
						else if (IsNested)
						{
							if (IsBeginGroup(wMergeField) && !NestedEnums.ContainsKey(wMergeField.FieldName) && wMergeField.FieldName != string.Empty)
							{
								string fieldName = wMergeField.FieldName;
								IRowsEnumerator @enum = GetEnum(fieldName);
								if (@enum != null)
								{
									int count2 = textBody.Items.Count;
									GroupSelectors.Push(m_groupSelector);
									m_groupSelector = new GroupSelector(OnGroupFound, InsertAsNewRow);
									m_groupSelector.ProcessGroupsInNested(textBody, @enum, itemStart, itemEnd);
									int selectedBodyItemsCount = m_groupSelector.SelectedBodyItemsCount;
									if ((wMergeField.Prefix == "TableStart" || wMergeField.Prefix == "BeginGroup") && selectedBodyItemsCount == -1)
									{
										Entity tableEntity = GetTableEntity(wMergeField);
										if (!(tableEntity is WTable))
										{
											throw new Exception("Group \"" + fieldName + "\" is missing in the source document.");
										}
										m_groupSelector.ProcessGroups(tableEntity as WTable, wMergeField.OwnerParagraph.Owner.Owner.GetIndexInOwnerCollection(), (tableEntity as WTable).Rows.Count - 1, @enum);
									}
									else
									{
										if (selectedBodyItemsCount == -1)
										{
											throw new Exception("Group \"" + fieldName + "\" is missing in the source document.");
										}
										if (selectedBodyItemsCount > 0)
										{
											int num4 = textBody.Items.Count - count2;
											i += num4 + selectedBodyItemsCount - 1;
											num += num4;
											itemEnd = num;
											if (i == num)
											{
												pItemEnd = UpdateEndIndex(ref count, wParagraph, num3);
											}
											if (-num4 != selectedBodyItemsCount)
											{
												i--;
											}
										}
										else if (wMergeField.Owner is WParagraph && HideField(wMergeField, hide: true))
										{
											j--;
											num3 = UpdateEndIndex(ref count, wParagraph, num3);
											if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
											{
												if (wParagraph.Owner is WTableCell wTableCell2 && (wTableCell2.ChildEntities.Count == 1 || (wParagraph == wTableCell2.LastParagraph && IsCellHasValidContent(wTableCell2))))
												{
													wParagraph.ChildEntities.Clear();
												}
												else
												{
													wParagraph.RemoveEmpty = true;
												}
											}
										}
									}
									if (m_curDataSet != null)
									{
										CurrentDataSet.Tables.Remove(fieldName);
									}
									else if (m_curDataSetDocIO != null)
									{
										CurrentDataSetDocIO.RemoveDataTable(fieldName);
									}
									m_groupSelector = GroupSelectors.Pop();
									if (i == num)
									{
										pItemEnd = UpdateEndIndex(ref count, wParagraph, num3);
									}
									break;
								}
								if (RemoveField(wField, isMergeStartAndEndPara: false) != -1)
								{
									j--;
								}
								num3 = UpdateEndIndex(ref count, wParagraph, num3);
								if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
								{
									if (wParagraph.Owner is WTableCell wTableCell3 && (wTableCell3.ChildEntities.Count == 1 || (wParagraph == wTableCell3.LastParagraph && IsCellHasValidContent(wTableCell3))))
									{
										wParagraph.ChildEntities.Clear();
									}
									else
									{
										wParagraph.RemoveEmpty = true;
									}
								}
								if (RemoveEmptyGroup && wParagraph.Items.Count == 0)
								{
									wParagraph.RemoveEmpty = true;
								}
							}
							else
							{
								if (!IsBeginGroup(wMergeField) && !IsEndGroup(wMergeField))
								{
									continue;
								}
								if (IsEndGroup(wMergeField) && (this.BeforeClearField != null || !ClearFields) && !IsNeedToRemoveGroupEnd(wMergeField))
								{
									break;
								}
								if (RemoveField(wField, isMergeStartAndEndPara: false) != -1)
								{
									j--;
								}
								num3 = UpdateEndIndex(ref count, wParagraph, num3);
								if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
								{
									if (wParagraph.Owner is WTableCell wTableCell4 && (wTableCell4.ChildEntities.Count == 1 || (wParagraph == wTableCell4.LastParagraph && IsCellHasValidContent(wTableCell4))))
									{
										wParagraph.ChildEntities.Clear();
									}
									else
									{
										wParagraph.RemoveEmpty = true;
									}
								}
							}
						}
						else
						{
							if (!IsBeginGroup(wMergeField) && !IsEndGroup(wMergeField))
							{
								continue;
							}
							if (IsEndGroup(wMergeField) && (this.BeforeClearField != null || !ClearFields) && !IsNeedToRemoveGroupEnd(wMergeField))
							{
								break;
							}
							if (RemoveField(wField, isMergeStartAndEndPara: false) != -1)
							{
								j--;
							}
							num3 = UpdateEndIndex(ref count, wParagraph, num3);
							if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
							{
								if (wParagraph.Owner is WTableCell wTableCell5 && (wTableCell5.ChildEntities.Count == 1 || (wParagraph == wTableCell5.LastParagraph && IsCellHasValidContent(wTableCell5))))
								{
									wParagraph.ChildEntities.Clear();
								}
								else
								{
									wParagraph.RemoveEmpty = true;
								}
							}
						}
					}
					else if (wField is WIfField)
					{
						UpdateIfFieldValue(wField as WIfField, rowsEnum);
					}
					else if (wField.FieldType == FieldType.FieldNext)
					{
						if (rowsEnum != null && !rowsEnum.IsEnd)
						{
							CheckNextRow(rowsEnum);
						}
						if (RemoveField(wField, isMergeStartAndEndPara: false) != -1)
						{
							j--;
						}
						num3 = UpdateEndIndex(ref count, wParagraph, num3);
						if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
						{
							if (wParagraph.Owner is WTableCell wTableCell6 && (wTableCell6.ChildEntities.Count == 1 || (wParagraph == wTableCell6.LastParagraph && IsCellHasValidContent(wTableCell6))))
							{
								wParagraph.ChildEntities.Clear();
							}
							else
							{
								wParagraph.RemoveEmpty = true;
							}
						}
					}
					else if (wField.FieldType == FieldType.FieldNextIf)
					{
						if (wField.UpdateNextIfField() && rowsEnum != null && !rowsEnum.IsEnd)
						{
							CheckNextRow(rowsEnum);
						}
						if (RemoveField(wField, isMergeStartAndEndPara: false) != -1)
						{
							j--;
						}
						num3 = wParagraph.Items.Count - 1;
						if (RemoveEmptyParagraphs && wParagraph.Items.Count == 0)
						{
							if (wParagraph.Owner is WTableCell wTableCell7 && (wTableCell7.ChildEntities.Count == 1 || (wParagraph == wTableCell7.LastParagraph && IsCellHasValidContent(wTableCell7))))
							{
								wParagraph.ChildEntities.Clear();
							}
							else
							{
								wParagraph.RemoveEmpty = true;
							}
						}
					}
					else if (wField.FieldType == FieldType.FieldMergeRec || wField.FieldType == FieldType.FieldMergeSeq)
					{
						int num5 = 1;
						if (rowsEnum != null)
						{
							num5 += rowsEnum.CurrentRowIndex;
						}
						if (!wField.OwnerParagraph.IsInCell || ClearFieldsState.Count <= 0 || ClearFieldsState.ContainsKey(rowsEnum.TableName))
						{
							ConvertToText(wField, num5.ToString());
						}
					}
				}
				break;
			}
			}
		}
	}

	private bool IsNeedToRemoveGroupEnd(WMergeField mergeField)
	{
		int count = Document.Fields.Count;
		for (int i = 0; i < count; i++)
		{
			if (Document.Fields[i] is WMergeField wMergeField)
			{
				List<string> list = new List<string>(wMergeField.FieldCode.Split(':', ' '));
				int num = list.IndexOf(wMergeField.Prefix);
				string text = string.Empty;
				if (!string.IsNullOrEmpty(wMergeField.Prefix))
				{
					text = list[num + 1];
				}
				if (IsBeginGroup(wMergeField) && mergeField.FieldCode.Contains(text) && m_groupSelector.GroupSelection != null && m_groupSelector.GroupSelection.ItemStartIndex <= wMergeField.OwnerParagraph.Index && m_groupSelector.GroupSelection.ItemEndIndex >= wMergeField.OwnerParagraph.Index && text != string.Empty && wMergeField.OwnerParagraph.ChildEntities.Count != 0 && mergeField.GetOwnerTextBody(mergeField) == wMergeField.GetOwnerTextBody(wMergeField))
				{
					return false;
				}
			}
		}
		return true;
	}

	private int UpdateEndIndex(ref int count, WParagraph para, int endIndex)
	{
		int num = count - para.Items.Count;
		count = para.Items.Count;
		return endIndex - num;
	}

	private Entity GetTableEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WTable));
		return entity2;
	}

	private int ExecuteGroupForRowSelection(WTable table, int startRowIndex, int count, IRowsEnumerator rowsEnum)
	{
		int count2 = table.Rows.Count;
		int num = startRowIndex + count - 1;
		WTableRow wTableRow = null;
		WTableCell wTableCell = null;
		int count3 = table.Rows.Count;
		for (int i = startRowIndex; i <= ((table.Rows.Count > num) ? num : (table.Rows.Count - 1)); i++)
		{
			wTableRow = table.Rows[i];
			int count4 = wTableRow.Cells.Count;
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				wTableCell = wTableRow.Cells[j];
				int count5 = table.Rows.Count;
				ExecuteGroupForSelection(wTableCell, 0, -1, 0, -1, rowsEnum);
				if (count5 < table.Rows.Count)
				{
					num += table.Rows.Count - count5;
				}
				if (count4 > wTableRow.Cells.Count)
				{
					j--;
					count4 = wTableRow.Cells.Count;
				}
			}
			if (count3 > table.Rows.Count)
			{
				i--;
				count3 = table.Rows.Count;
			}
		}
		num = ((table.Rows.Count > num) ? num : (table.Rows.Count - 1));
		int num2 = 0;
		if (IsNested)
		{
			string text = FindNestedGroup(startRowIndex, num, table);
			int count6 = table.Rows.Count;
			if (text != null && ClearFieldsState.ContainsKey(text))
			{
				ClearFields = ClearFieldsState[text];
				ClearFieldsState.Remove(text);
			}
			while (text != null)
			{
				num2 = table.Rows.Count - count6;
				num += num2;
				startRowIndex += num2;
				count6 = table.Rows.Count;
				IRowsEnumerator @enum = GetEnum(text);
				if (@enum != null)
				{
					GroupSelectors.Push(m_groupSelector);
					m_groupSelector = new GroupSelector(OnGroupFound, InsertAsNewRow);
					m_groupSelector.ProcessGroups(table, startRowIndex, num, @enum);
					if (m_curDataSet != null)
					{
						CurrentDataSet.Tables.Remove(text);
					}
					else if (m_curDataSetDocIO != null)
					{
						CurrentDataSetDocIO.RemoveDataTable(text);
					}
					m_groupSelector = GroupSelectors.Pop();
				}
				string text2 = text;
				text = FindNestedGroup(startRowIndex, num, table);
				if (text2 == text)
				{
					break;
				}
			}
		}
		num2 = table.Rows.Count - count2;
		return count + num2;
	}

	private void ExecuteNestedGroup(string tableName)
	{
		IRowsEnumerator @enum = GetEnum(tableName);
		if (@enum != null)
		{
			WSection wSection = null;
			int i = 0;
			for (int count = Document.Sections.Count; i < count; i++)
			{
				wSection = Document.Sections[i];
				ExecuteGroup(wSection, @enum);
			}
			if (m_curDataSet != null)
			{
				CurrentDataSet.Tables.Remove(tableName);
			}
			else if (m_curDataSetDocIO != null)
			{
				CurrentDataSetDocIO.RemoveDataTable(tableName);
			}
		}
	}

	private IRowsEnumerator GetEnum(string tableName)
	{
		IRowsEnumerator rowsEnumerator = null;
		object dataTable = GetDataTable(tableName);
		if (dataTable is DataTable)
		{
			DataTable table = dataTable as DataTable;
			CurrentDataSet.Tables.Add(table);
			rowsEnumerator = new DataTableEnumerator(table);
			rowsEnumerator.Reset();
		}
		else if (dataTable is MailMergeDataTable)
		{
			MailMergeDataTable mailMergeDataTable = dataTable as MailMergeDataTable;
			CurrentDataSetDocIO.Add(mailMergeDataTable);
			rowsEnumerator = new DataTableEnumerator(mailMergeDataTable);
			rowsEnumerator.Reset();
		}
		return rowsEnumerator;
	}

	private void UpdateEnum(string tableName, IRowsEnumerator rowsEnum)
	{
		if (!NestedEnums.ContainsKey(tableName))
		{
			NestedEnums.Add(tableName, rowsEnum);
		}
		else
		{
			NestedEnums[tableName] = rowsEnum;
		}
	}

	private object GetDataTable(string tableName)
	{
		if (m_conn != null)
		{
			return GetDataTableConn(tableName);
		}
		if (m_dataSetDocIO != null)
		{
			return GetDataTable(tableName, m_dataSetDocIO);
		}
		return GetDataTableDSet(tableName);
	}

	private MailMergeDataTable GetDataTable(string tableName, MailMergeDataSet dataSet)
	{
		MailMergeDataTable mailMergeDataTable = null;
		if (dataSet != null)
		{
			mailMergeDataTable = dataSet.GetDataTable(tableName);
		}
		if (mailMergeDataTable == null)
		{
			if (m_commandsDocIO == null)
			{
				object cellValue = NestedEnums[m_groupSelector.GroupName].GetCellValue(tableName);
				if (cellValue is IEnumerable && !(cellValue is IDictionary<string, object>))
				{
					IEnumerable enumerable = cellValue as IEnumerable;
					return new MailMergeDataTable(tableName, enumerable);
				}
				List<object> list = new List<object>();
				if (cellValue != null)
				{
					list.Add(cellValue);
				}
				return new MailMergeDataTable(tableName, list);
			}
			return null;
		}
		string command = GetCommand(tableName);
		if (command == string.Empty || m_commandsDocIO == null)
		{
			mailMergeDataTable.Command = string.Empty;
			return mailMergeDataTable;
		}
		if (command == null)
		{
			List<object> enumerable2 = new List<object>();
			return new MailMergeDataTable(tableName, enumerable2);
		}
		mailMergeDataTable.Command = command;
		mailMergeDataTable.Select(command);
		return mailMergeDataTable;
	}

	private string GetCommand(string tableName)
	{
		DictionaryEntry dictionaryEntry = new DictionaryEntry(string.Empty, string.Empty);
		bool flag = false;
		if (m_commands != null)
		{
			int i = 0;
			for (int count = m_commands.Count; i < count; i++)
			{
				dictionaryEntry = (DictionaryEntry)m_commands[i];
				if (tableName == (string)dictionaryEntry.Key)
				{
					flag = true;
					break;
				}
			}
		}
		else if (m_commandsDocIO != null)
		{
			int j = 0;
			for (int count2 = m_commandsDocIO.Count; j < count2; j++)
			{
				dictionaryEntry = m_commandsDocIO[j];
				if (tableName == (string)dictionaryEntry.Key)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			string text = (string)dictionaryEntry.Value;
			if (text.IndexOf("%") == -1)
			{
				return text;
			}
			return UpdateVarCmd(text);
		}
		return null;
	}

	private string UpdateVarCmd(string command)
	{
		string input = Regex.Replace(command, "(?i)\\b(LIKE|NOT\\s+LIKE)\\s+['\"][^'\"]*['\"]", "");
		MatchCollection matchCollection = VariableCommandRegex.Matches(input);
		if (matchCollection.Count == 0)
		{
			return null;
		}
		char[] separator = new char[1] { '.' };
		string text = null;
		string text2 = null;
		string[] array = null;
		int i = 0;
		for (int count = matchCollection.Count; i < count; i++)
		{
			text2 = matchCollection[i].Value.Replace("%", string.Empty);
			array = text2.Split(separator);
			if (array.Length != 2)
			{
				throw new ArgumentException("String value between '%' symbols (variable command) is not valid.");
			}
			IRowsEnumerator rowsEnumerator = null;
			if (NestedEnums.ContainsKey(array[0]))
			{
				rowsEnumerator = NestedEnums[array[0]];
			}
			if (rowsEnumerator == null)
			{
				return string.Empty;
			}
			text = rowsEnumerator.GetCellValue(array[1]).ToString();
			if (m_dataSet != null && m_dataSet.Tables.Contains(array[0]))
			{
				DataTable dataTable = m_dataSet.Tables[array[0]];
				if (dataTable.Columns.Contains(array[1]) && dataTable.Columns[array[1]].DataType.Name == "String")
				{
					text = text.Replace("'", "''");
					text = "'" + text + "'";
				}
			}
			command = command.Replace("%" + text2 + "%", text);
		}
		return command;
	}

	private void VerifyNestedGroups(int startRow, int endRow, WTable table)
	{
		Dictionary<string, WMergeField> dictionary = new Dictionary<string, WMergeField>();
		Dictionary<string, WMergeField> dictionary2 = new Dictionary<string, WMergeField>();
		WMergeField wMergeField = null;
		for (int i = startRow; i <= endRow; i++)
		{
			foreach (WTableCell cell in table.Rows[i].Cells)
			{
				foreach (WParagraph paragraph in cell.Paragraphs)
				{
					foreach (ParagraphItem item in paragraph.Items)
					{
						if (item is WMergeField)
						{
							wMergeField = item as WMergeField;
							if (IsBeginGroup(wMergeField) && !string.IsNullOrEmpty(wMergeField.FieldName) && !dictionary.ContainsKey(wMergeField.FieldName))
							{
								dictionary.Add(wMergeField.FieldName, wMergeField);
							}
							else if (IsEndGroup(wMergeField) && !string.IsNullOrEmpty(wMergeField.FieldName) && !dictionary2.ContainsKey(wMergeField.FieldName))
							{
								dictionary2.Add(wMergeField.FieldName, wMergeField);
							}
						}
					}
				}
			}
		}
		if (dictionary.Count == 0)
		{
			if (dictionary2.Count > 0)
			{
				using Dictionary<string, WMergeField>.KeyCollection.Enumerator enumerator4 = dictionary2.Keys.GetEnumerator();
				if (enumerator4.MoveNext())
				{
					string current = enumerator4.Current;
					throw new Exception("GroupEnd field \"" + current + "\" doesn't have GroupStart field equivalent.");
				}
			}
		}
		else if (dictionary2.Count == 0 && dictionary.Count > 0)
		{
			using Dictionary<string, WMergeField>.KeyCollection.Enumerator enumerator4 = dictionary.Keys.GetEnumerator();
			if (enumerator4.MoveNext())
			{
				string current2 = enumerator4.Current;
				throw new Exception("GroupStart field \"" + current2 + "\" doesn't have GroupEnd field equivalent.");
			}
		}
		foreach (string key in dictionary.Keys)
		{
			if (!dictionary2.ContainsKey(key))
			{
				throw new Exception("GroupStart field \"" + key + "\" doesn't have GroupEnd field equivalent.");
			}
			dictionary2.Remove(key);
		}
		if (dictionary2.Count > 0)
		{
			using Dictionary<string, WMergeField>.KeyCollection.Enumerator enumerator4 = dictionary2.Keys.GetEnumerator();
			if (enumerator4.MoveNext())
			{
				string current4 = enumerator4.Current;
				throw new Exception("GroupEnd field \"" + current4 + "\" doesn't have GroupStart field equivalent.");
			}
		}
		dictionary.Clear();
		dictionary2.Clear();
	}

	private string FindNestedGroup(int startRow, int endRow, WTable table)
	{
		for (int i = startRow; i <= endRow; i++)
		{
			foreach (WTableCell cell in table.Rows[i].Cells)
			{
				foreach (WParagraph paragraph in cell.Paragraphs)
				{
					foreach (ParagraphItem item in paragraph.Items)
					{
						if (item is WMergeField)
						{
							WMergeField wMergeField = item as WMergeField;
							if (IsBeginGroup(wMergeField) && wMergeField.FieldName != string.Empty)
							{
								string fieldName = (item as WMergeField).FieldName;
								return (fieldName == string.Empty) ? null : fieldName;
							}
						}
					}
				}
			}
		}
		return null;
	}

	private DataTable GetDataTableConn(string tableName)
	{
		string command = GetCommand(tableName);
		if (command == null)
		{
			command = "Select * from " + tableName;
		}
		else if (command == string.Empty)
		{
			return null;
		}
		return new DataTable(tableName);
	}

	private DataTable GetDataTableDSet(string tableName)
	{
		DataTable dataTable = m_dataSet.Tables[tableName];
		if (dataTable == null)
		{
			return new DataTable(tableName);
		}
		string command = GetCommand(tableName);
		if (command == null)
		{
			return null;
		}
		DataRow[] array = null;
		try
		{
			array = dataTable.Select(command);
		}
		catch (Exception)
		{
		}
		DataTable dataTable2 = new DataTable(tableName);
		foreach (DataColumn column in dataTable.Columns)
		{
			dataTable2.Columns.Add(column.ColumnName).DataType = column.DataType;
		}
		if (array == null)
		{
			return dataTable2;
		}
		DataRow[] array2 = array;
		foreach (DataRow dataRow in array2)
		{
			DataRow dataRow2 = dataTable2.NewRow();
			dataRow2.ItemArray = dataRow.ItemArray;
			dataRow2.RowError = dataRow.RowError;
			dataTable2.Rows.Add(dataRow2);
		}
		return dataTable2;
	}

	protected MergeFieldEventArgs SendMergeField(IWMergeField field, object value, IRowsEnumerator rowsEnum)
	{
		string groupName = GetGroupName((rowsEnum != null) ? rowsEnum.TableName : string.Empty);
		MergeFieldEventArgs mergeFieldEventArgs = new MergeFieldEventArgs(Document, rowsEnum.TableName, rowsEnum.CurrentRowIndex, field, value, groupName);
		if (this.MergeField != null)
		{
			this.MergeField(this, mergeFieldEventArgs);
		}
		return mergeFieldEventArgs;
	}

	internal MergeFieldEventArgs SendMergeField(IWMergeField field, object value, IRowsEnumerator rowsEnum, int valIndex)
	{
		MergeFieldEventArgs mergeFieldEventArgs = null;
		string groupName = GetGroupName((rowsEnum != null) ? rowsEnum.TableName : string.Empty);
		if (rowsEnum != null)
		{
			mergeFieldEventArgs = new MergeFieldEventArgs(Document, rowsEnum.TableName, rowsEnum.CurrentRowIndex, field, value, groupName);
		}
		else if (valIndex != -1)
		{
			mergeFieldEventArgs = new MergeFieldEventArgs(Document, "", valIndex, field, m_values[valIndex], groupName);
		}
		if (this.MergeField != null && mergeFieldEventArgs != null)
		{
			this.MergeField(this, mergeFieldEventArgs);
		}
		return mergeFieldEventArgs;
	}

	protected MergeImageFieldEventArgs SendMergeImageField(IWMergeField field, object bmp, IRowsEnumerator rowsEnum, MemoryStream imageByteStream)
	{
		MergeImageFieldEventArgs mergeImageFieldEventArgs = null;
		mergeImageFieldEventArgs = ((rowsEnum == null) ? new MergeImageFieldEventArgs(Document, null, int.MaxValue, field, bmp) : new MergeImageFieldEventArgs(Document, rowsEnum.TableName, rowsEnum.CurrentRowIndex, field, bmp));
		if (imageByteStream != null)
		{
			mergeImageFieldEventArgs.ImageStream = imageByteStream;
		}
		if (this.MergeImageField != null)
		{
			this.MergeImageField(this, mergeImageFieldEventArgs);
		}
		return mergeImageFieldEventArgs;
	}

	internal BeforeClearFieldEventArgs SendBeforeClearField(IRowsEnumerator rowsEnum, IWMergeField mergeField, object value)
	{
		BeforeClearFieldEventArgs beforeClearFieldEventArgs = null;
		bool flag = false;
		string groupName = GetGroupName((rowsEnum != null) ? rowsEnum.TableName : string.Empty);
		if (rowsEnum == null)
		{
			beforeClearFieldEventArgs = new BeforeClearFieldEventArgs(Document, groupName, -1, mergeField, fieldHasMappedInDataSource: false, value);
		}
		else
		{
			List<string> list = new List<string>(rowsEnum.ColumnNames);
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].ToLower();
			}
			beforeClearFieldEventArgs = new BeforeClearFieldEventArgs(fieldHasMappedInDataSource: list.Contains(mergeField.FieldName.ToLower()) ? true : false, doc: Document, groupName: groupName, rowIndex: rowsEnum.CurrentRowIndex, field: mergeField, value: value);
		}
		if (this.BeforeClearField != null)
		{
			this.BeforeClearField(this, beforeClearFieldEventArgs);
		}
		return beforeClearFieldEventArgs;
	}

	internal BeforeClearGroupFieldEventArgs SendBeforeClearGroup(IRowsEnumerator rowsEnum, GroupSelector groupSelector)
	{
		for (int i = 0; i < Document.Fields.Count; i++)
		{
			if (Document.Fields.InnerList[i] is WMergeField wMergeField && wMergeField.FieldName == string.Empty && wMergeField.FieldCode.Contains(rowsEnum.TableName))
			{
				wMergeField.FieldName = rowsEnum.TableName;
			}
		}
		string groupName = GetGroupName((rowsEnum != null) ? rowsEnum.TableName : string.Empty);
		string[] fieldNames = groupSelector.FieldNames.ToArray();
		BeforeClearGroupFieldEventArgs beforeClearGroupFieldEventArgs = new BeforeClearGroupFieldEventArgs(Document, groupName, this as IWMergeField, fieldHasMappedInDataSource: false, fieldNames);
		if (this.BeforeClearGroupField != null)
		{
			this.BeforeClearGroupField(this, beforeClearGroupFieldEventArgs);
		}
		return beforeClearGroupFieldEventArgs;
	}

	private string GetGroupName(string currGroupName)
	{
		string text = string.Empty;
		if (NestedEnums != null)
		{
			if (NestedEnums.Count > 0)
			{
				List<string> list = new List<string>(NestedEnums.Keys);
				for (int i = 0; i < list.Count; i++)
				{
					text = text + list[i] + ":";
				}
				text = text.TrimEnd(':');
			}
			else if (!string.IsNullOrEmpty(currGroupName))
			{
				text = currGroupName;
			}
		}
		if (!string.IsNullOrEmpty(currGroupName) && !text.Contains(currGroupName))
		{
			text = text + ":" + currGroupName;
		}
		return text;
	}

	private void Execute(IRowsEnumerator rowsEnum)
	{
		Document.IsMailMerge = true;
		RemoveSpellChecking();
		if (rowsEnum == null)
		{
			throw new ArgumentNullException("rowsEnum");
		}
		int rowsCount = rowsEnum.RowsCount;
		int num = 0;
		if (rowsCount > 1)
		{
			CopyContent(Document);
		}
		IWSectionCollection sections = Document.Sections;
		rowsEnum.Reset();
		if (rowsEnum.RowsCount == 0 && ClearFields)
		{
			int i = 0;
			for (int count = sections.Count; i < count; i++)
			{
				ExecuteForSection(sections[i], null);
			}
		}
		else
		{
			while (CheckNextRow(rowsEnum))
			{
				int j = num;
				for (int count2 = sections.Count; j < count2; j++)
				{
					ExecuteForSection(sections[j], rowsEnum);
				}
				num = sections.Count;
				if (!rowsEnum.IsLast)
				{
					AppendCopiedContent(Document);
				}
			}
		}
		Document.IsMailMerge = false;
	}

	private bool CheckNextRow(IRowsEnumerator rowsEnum)
	{
		if (IsNested && rowsEnum is DataTableEnumerator)
		{
			if ((rowsEnum as DataTableEnumerator).m_MMtable != null)
			{
				string[] tableCommand = GetTableCommand((rowsEnum as DataTableEnumerator).Command);
				if (tableCommand == null)
				{
					return rowsEnum.NextRow();
				}
				return (rowsEnum as DataTableEnumerator).NextRow(tableCommand);
			}
			return rowsEnum.NextRow();
		}
		return rowsEnum.NextRow();
	}

	private void ExecuteForSection(IWSection sec, IRowsEnumerator rowsEnum)
	{
		ExecuteForTextBody(sec.Body.Items, rowsEnum);
		for (int i = 0; i < 6; i++)
		{
			BodyItemCollection bodyItemCollection = (BodyItemCollection)sec.HeadersFooters[i].ChildEntities;
			if (bodyItemCollection.Count > 0)
			{
				ExecuteForTextBody(bodyItemCollection, rowsEnum);
			}
		}
	}

	private void ExecuteForTextBody(BodyItemCollection bodyItems, IRowsEnumerator rowsEnum)
	{
		for (int i = 0; i < bodyItems.Count; i++)
		{
			ITextBodyItem textBodyItem = bodyItems[i];
			if (textBodyItem != null)
			{
				ExecuteForTextBodyItem(textBodyItem, rowsEnum);
			}
		}
	}

	private void ExecuteForTextBodyItem(ITextBodyItem item, IRowsEnumerator rowsEnum)
	{
		if (item is IWParagraph)
		{
			WParagraph paragraph = item as WParagraph;
			ExecuteForParagraph(paragraph, rowsEnum);
		}
		else if (item is IWTable)
		{
			IWTable table = item as IWTable;
			ExecuteForTable(table, rowsEnum);
		}
		else
		{
			if (!(item is BlockContentControl))
			{
				return;
			}
			BodyItemCollection bodyItemCollection = (item as BlockContentControl).TextBody.ChildEntities as BodyItemCollection;
			if (bodyItemCollection.Count > 0)
			{
				ITextBodyItem textBodyItem = bodyItemCollection[0];
				while (textBodyItem != null)
				{
					ITextBodyItem obj = textBodyItem.NextSibling as ITextBodyItem;
					ExecuteForTextBodyItem(textBodyItem, rowsEnum);
					textBodyItem = obj;
				}
			}
		}
	}

	private void ExecuteForParagraph(WParagraph paragraph, IRowsEnumerator rowsEnum)
	{
		bool paraItemCollectionChanged = false;
		_ = paragraph.Items.Count;
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (paragraph.DeepDetached)
			{
				break;
			}
			ParagraphItem paragraphItem = paragraph[i];
			if (paragraphItem.IsDeleteRevision)
			{
				continue;
			}
			ExecuteForParagraphItems(paragraphItem, paragraph, rowsEnum, ref paraItemCollectionChanged);
			if (!paraItemCollectionChanged)
			{
				continue;
			}
			_ = paragraph.Items.Count;
			i--;
			paraItemCollectionChanged = false;
			if (RemoveEmptyParagraphs && !paragraph.RemoveEmpty && (paragraph.Items.Count == 0 || paragraph.IsOnlyHasSpaces()))
			{
				if (paragraph.Owner is WTableCell wTableCell && (wTableCell.ChildEntities.Count == 1 || (paragraph == wTableCell.LastParagraph && IsCellHasValidContent(wTableCell))))
				{
					paragraph.ChildEntities.Clear();
				}
				else
				{
					paragraph.RemoveEmpty = true;
				}
			}
		}
	}

	private bool IsCellHasValidContent(WTableCell parentCell)
	{
		if (parentCell.LastParagraph.PreviousSibling != null && parentCell.LastParagraph.PreviousSibling is WTable)
		{
			return true;
		}
		bool result = false;
		foreach (Entity childEntity in parentCell.ChildEntities)
		{
			if (childEntity is WParagraph)
			{
				WParagraph wParagraph = childEntity as WParagraph;
				if (wParagraph != parentCell.LastParagraph)
				{
					if (!wParagraph.RemoveEmpty)
					{
						result = false;
						break;
					}
					result = true;
				}
			}
			else if (childEntity is WTable || childEntity is BlockContentControl)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void ExecuteForParagraphItems(ParagraphItem pItem, WParagraph paragraph, IRowsEnumerator rowsEnum, ref bool paraItemCollectionChanged)
	{
		if (pItem is WMergeField wMergeField)
		{
			wMergeField.UpdateFieldMarks();
			if (_isInValidNextField && _previousMergeField != null && _previousMergeField.FieldName == wMergeField.FieldName)
			{
				if (ClearFields)
				{
					paraItemCollectionChanged = RemoveField(wMergeField, isMergeStartAndEndPara: true) != -1;
				}
				return;
			}
			if (wMergeField.Prefix.StartsWith("Image"))
			{
				paraItemCollectionChanged = UpdateImageMergeFieldValue(wMergeField, rowsEnum);
			}
			else
			{
				paraItemCollectionChanged = UpdateMergeFieldValue(wMergeField, rowsEnum);
			}
			_previousMergeField = wMergeField;
			return;
		}
		if (pItem is WField)
		{
			WField wField = pItem as WField;
			if (wField.FieldType == FieldType.FieldNext)
			{
				if (rowsEnum != null && !rowsEnum.IsEnd)
				{
					CheckNextRow(rowsEnum);
				}
				else
				{
					_isInValidNextField = true;
				}
				paraItemCollectionChanged = RemoveField(wField, isMergeStartAndEndPara: false) != -1;
			}
			else if (wField.FieldType == FieldType.FieldNextIf)
			{
				if (wField.UpdateNextIfField() && rowsEnum != null && !rowsEnum.IsEnd)
				{
					CheckNextRow(rowsEnum);
				}
				paraItemCollectionChanged = RemoveField(wField, isMergeStartAndEndPara: false) != -1;
			}
			else if (wField.FieldType == FieldType.FieldIf)
			{
				if (m_IfFieldCollections == null)
				{
					m_IfFieldCollections = new Stack<WIfField>();
				}
				m_IfFieldCollections.Push(wField as WIfField);
				UpdateIfFieldValue(wField as WIfField, rowsEnum);
			}
			else if (wField.FieldType == FieldType.FieldMergeRec || wField.FieldType == FieldType.FieldMergeSeq)
			{
				int num = 1;
				if (rowsEnum != null)
				{
					num += rowsEnum.CurrentRowIndex;
				}
				ConvertToText(wField, num.ToString());
			}
			return;
		}
		if (pItem is WTextBox)
		{
			WTextBox wTextBox = pItem as WTextBox;
			ExecuteForTextBody((BodyItemCollection)wTextBox.TextBoxBody.ChildEntities, rowsEnum);
			return;
		}
		if (pItem is Shape)
		{
			Shape shape = pItem as Shape;
			ExecuteForTextBody((BodyItemCollection)shape.TextBody.ChildEntities, rowsEnum);
			return;
		}
		if (pItem is InlineContentControl)
		{
			foreach (ParagraphItem paragraphItem in (pItem as InlineContentControl).ParagraphItems)
			{
				ExecuteForParagraphItems(paragraphItem, paragraph, rowsEnum, ref paraItemCollectionChanged);
			}
			return;
		}
		if (pItem is WComment)
		{
			foreach (WParagraph childEntity in (pItem as WComment).ChildEntities)
			{
				for (int i = 0; i < childEntity.Items.Count; i++)
				{
					ExecuteForParagraphItems(childEntity.Items[i], childEntity, rowsEnum, ref paraItemCollectionChanged);
				}
			}
			return;
		}
		if (pItem is WFootnote)
		{
			WTextBody textBody = (pItem as WFootnote).TextBody;
			ExecuteForTextBody((BodyItemCollection)textBody.ChildEntities, rowsEnum);
		}
		else if (pItem is WFieldMark && ((WFieldMark)pItem).Type == FieldMarkType.FieldEnd && ((WFieldMark)pItem).ParentField is WIfField && m_IfFieldCollections != null)
		{
			if (m_IfFieldCollections.Count == 1)
			{
				m_IfFieldCollections.Clear();
				m_IfFieldCollections = null;
			}
			else
			{
				m_IfFieldCollections.Pop();
			}
		}
	}

	private void ExecuteForTable(IWTable table, IRowsEnumerator rowsEnum)
	{
		if (table.Rows.Count <= 0)
		{
			return;
		}
		WTableRow wTableRow = table.Rows[0];
		while (wTableRow != null)
		{
			WTableRow wTableRow2 = wTableRow.NextSibling as WTableRow;
			if (wTableRow.Cells.Count > 0)
			{
				WTableCell wTableCell = wTableRow.Cells[0];
				while (wTableCell != null)
				{
					WTableCell obj = wTableCell.NextSibling as WTableCell;
					ExecuteForTextBody((BodyItemCollection)wTableCell.ChildEntities, rowsEnum);
					wTableCell = obj;
				}
			}
			wTableRow = wTableRow2;
		}
	}

	private void ConvertToText(WField field, string text)
	{
		WTextRange wTextRange = new WTextRange(Document);
		WParagraph ownerParagraph = field.OwnerParagraph;
		int indexInOwnerCollection = field.GetIndexInOwnerCollection();
		ownerParagraph.Items.Remove(field);
		ownerParagraph.Items.Insert(indexInOwnerCollection, wTextRange);
		wTextRange.CharacterFormat.ImportContainer(field.CharacterFormat);
		wTextRange.CharacterFormat.CopyProperties(field.CharacterFormat);
		if (wTextRange.CharacterFormat.HasValue(106))
		{
			wTextRange.CharacterFormat.PropertiesHash.Remove(106);
		}
		wTextRange.Text = text;
	}

	private bool UpdateMergeFieldValue(WMergeField mergeField, IRowsEnumerator rowsEnum)
	{
		bool result = false;
		bool clearFields = ClearFields;
		if (rowsEnum == null)
		{
			result = UpdateMergeFieldValue(mergeField);
		}
		else
		{
			object fieldValue = GetFieldValue(mergeField, rowsEnum);
			fieldValue = TriggerSendBeforeClearFieldEvent(fieldValue, rowsEnum, mergeField);
			if (fieldValue != null)
			{
				result = UpdateMergeFieldResult(mergeField, fieldValue, rowsEnum, -1);
			}
			else if (ClearFields)
			{
				result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
			}
		}
		ClearFields = clearFields;
		return result;
	}

	private object TriggerSendBeforeClearFieldEvent(object value, IRowsEnumerator rowsEnum, WMergeField mergeField)
	{
		if ((value == null || value.ToString() == string.Empty) && this.BeforeClearField != null)
		{
			BeforeClearFieldEventArgs beforeClearFieldEventArgs = SendBeforeClearField(rowsEnum, mergeField, value);
			if (beforeClearFieldEventArgs != null)
			{
				ClearFields = beforeClearFieldEventArgs.ClearField;
				if (!beforeClearFieldEventArgs.ClearField && beforeClearFieldEventArgs.FieldValue != null && !string.IsNullOrEmpty(beforeClearFieldEventArgs.FieldValue.ToString()))
				{
					value = beforeClearFieldEventArgs.FieldValue;
				}
			}
		}
		return value;
	}

	private bool UpdateMergeFieldResult(WMergeField mergeField, object value, IRowsEnumerator rowsEnum, int valIndex)
	{
		bool result = false;
		bool clearFields = ClearFields;
		MergeFieldEventArgs mergeFieldEventArgs = SendMergeField(mergeField, value, rowsEnum, valIndex);
		if (mergeFieldEventArgs != null)
		{
			if (mergeFieldEventArgs.TextRange.Text == mergeFieldEventArgs.Text)
			{
				mergeField.FieldResult = mergeFieldEventArgs.Text;
			}
			else
			{
				mergeField.FieldResult = mergeFieldEventArgs.TextRange.Text;
			}
		}
		if (m_IfFieldCollections != null && m_IfFieldCollections.Count > 0)
		{
			EnusreDoubelQuotesForResult(m_IfFieldCollections.Peek(), mergeField);
		}
		value = TriggerSendBeforeClearFieldEvent(value, rowsEnum, mergeField);
		if (mergeField.Owner is WParagraph && mergeField.FieldEnd != null && mergeField.FieldEnd.Owner is WParagraph)
		{
			if (value != null && value.ToString() != string.Empty)
			{
				InsertMergeFieldResultAsTextRange(mergeField, mergeFieldEventArgs);
				result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
			}
			else if (ClearFields)
			{
				result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
			}
		}
		ClearFields = clearFields;
		return result;
	}

	private void EnusreDoubelQuotesForResult(WIfField ifField, WMergeField mergeField)
	{
		string text = string.Empty;
		string text2 = string.Empty;
		if (!string.IsNullOrEmpty(mergeField.TextBefore))
		{
			text = mergeField.TextBefore;
		}
		if (!string.IsNullOrEmpty(mergeField.TextAfter))
		{
			text2 = mergeField.TextAfter;
		}
		string text3 = text + mergeField.FieldResult + text2;
		if (string.IsNullOrEmpty(text3) || text3.Trim().Contains(" "))
		{
			int doubleQuotesCount = 0;
			WParagraph ownerParagraph = ifField.OwnerParagraph;
			WParagraph ownerParagraph2 = mergeField.OwnerParagraph;
			WParagraph wParagraph = ((ifField.FieldSeparator != null) ? ifField.FieldSeparator.OwnerParagraph : null);
			int indexInOwnerCollection = ifField.GetIndexInOwnerCollection();
			int num = ((ifField.FieldSeparator != null) ? ((ownerParagraph2 == wParagraph) ? ifField.FieldSeparator.GetIndexInOwnerCollection() : 0) : 0);
			int indexInOwnerCollection2 = mergeField.GetIndexInOwnerCollection();
			if (wParagraph == null || ownerParagraph2.Index < wParagraph.Index || (ownerParagraph2.Index == wParagraph.Index && indexInOwnerCollection2 < num))
			{
				bool flag = true;
				if (ownerParagraph == ownerParagraph2)
				{
					CountDoubleQuotes(ownerParagraph, indexInOwnerCollection + 1, indexInOwnerCollection2, ref doubleQuotesCount);
				}
				else if (ownerParagraph.OwnerTextBody == ownerParagraph2.OwnerTextBody)
				{
					int indexInOwnerCollection3 = ownerParagraph.GetIndexInOwnerCollection();
					int indexInOwnerCollection4 = ownerParagraph2.GetIndexInOwnerCollection();
					WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
					for (int i = indexInOwnerCollection3; i <= indexInOwnerCollection4 && i < ownerTextBody.ChildEntities.Count; i++)
					{
						if (ownerTextBody.ChildEntities[i] is WParagraph wParagraph2)
						{
							CountDoubleQuotes(wParagraph2, (i == indexInOwnerCollection3) ? (indexInOwnerCollection + 1) : 0, (i == indexInOwnerCollection4) ? indexInOwnerCollection2 : wParagraph2.ChildEntities.Count, ref doubleQuotesCount);
						}
					}
				}
				else
				{
					flag = false;
				}
				if (ifField.FieldValue.Contains("\""))
				{
					doubleQuotesCount += ifField.FieldValue.Length - ifField.FieldValue.Replace("\"", "").Length;
				}
				if (flag && doubleQuotesCount % 2 == 0)
				{
					text = (mergeField.TextBefore = "\"" + text);
					text2 = (mergeField.TextAfter = text2 + "\"");
					text3 = text + mergeField.FieldResult + text2;
				}
			}
		}
		ifField.EnusreSpaceInResultText(mergeField, text3, text, text2);
	}

	private void CountDoubleQuotes(WParagraph paragraph, int startIindex, int endIndex, ref int doubleQuotesCount)
	{
		for (int i = startIindex; i < endIndex && i < paragraph.ChildEntities.Count; i++)
		{
			if (paragraph.ChildEntities[i] is WTextRange wTextRange && wTextRange.Text.Contains("\""))
			{
				doubleQuotesCount += wTextRange.Text.Length - wTextRange.Text.Replace("\"", "").Length;
			}
		}
	}

	private bool IsDeepDetached(WField field)
	{
		if ((field.Owner is WParagraph && (field.Owner as WParagraph).DeepDetached) || (field.FieldEnd.Owner is WParagraph && (field.FieldEnd.Owner as WParagraph).DeepDetached))
		{
			return true;
		}
		return false;
	}

	private int RemoveField(WField field, bool isMergeStartAndEndPara)
	{
		if (!(field.Owner is WParagraph) || field.FieldEnd == null || !(field.FieldEnd.Owner is WParagraph) || field.OwnerParagraph.OwnerTextBody == null || field.FieldEnd.OwnerParagraph.OwnerTextBody == null || IsDeepDetached(field))
		{
			return -1;
		}
		int indexInOwnerCollection = field.GetIndexInOwnerCollection();
		WParagraph ownerParagraph = field.OwnerParagraph;
		string text = "_fieldBookmark";
		if (m_doc.Bookmarks.FindByName(text) != null)
		{
			text = "_fieldBookmark" + Guid.NewGuid();
		}
		BookmarkStart bookmarkStart = new BookmarkStart(m_doc, text);
		BookmarkEnd bookmarkEnd = new BookmarkEnd(m_doc, text);
		ownerParagraph.Items.Insert(indexInOwnerCollection, bookmarkStart);
		field.EnsureBookmarkStart(bookmarkStart);
		WParagraph ownerParagraph2 = field.FieldEnd.OwnerParagraph;
		int indexInOwnerCollection2 = field.FieldEnd.GetIndexInOwnerCollection();
		ownerParagraph2.Items.Insert(indexInOwnerCollection2 + 1, bookmarkEnd);
		field.EnsureBookmarkStart(bookmarkEnd);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(m_doc);
		bookmarksNavigator.MoveToBookmark(text);
		bookmarksNavigator.RemoveEmptyParagraph = false;
		Document.IsSkipFieldDetach = true;
		bookmarksNavigator.DeleteBookmarkContent(saveFormatting: false);
		Document.IsSkipFieldDetach = false;
		if (ownerParagraph.Items.Contains(bookmarkStart))
		{
			ownerParagraph.Items.Remove(bookmarkStart);
		}
		if (ownerParagraph2.Items.Contains(bookmarkEnd))
		{
			ownerParagraph2.Items.Remove(bookmarkEnd);
		}
		if (isMergeStartAndEndPara && ownerParagraph != ownerParagraph2)
		{
			int num = 0;
			while (ownerParagraph.ChildEntities.Count > 0)
			{
				ownerParagraph2.ChildEntities.Insert(num, ownerParagraph.ChildEntities[0]);
				num++;
			}
			ownerParagraph.RemoveEmpty = true;
		}
		return indexInOwnerCollection;
	}

	private void InsertMergeFieldResultAsTextRange(WMergeField mergeField, MergeFieldEventArgs args)
	{
		List<WCharacterFormat> resultCharacterFormatting = mergeField.GetResultCharacterFormatting();
		int num = mergeField.GetIndexInOwnerCollection();
		string text = string.Empty;
		if (!string.IsNullOrEmpty(mergeField.TextBefore))
		{
			text += mergeField.TextBefore;
		}
		text += mergeField.FieldResult;
		if (!string.IsNullOrEmpty(mergeField.TextAfter))
		{
			text += mergeField.TextAfter;
		}
		if (resultCharacterFormatting.Count <= 1)
		{
			WTextRange textRange = args.TextRange;
			textRange.Text = text;
			if (resultCharacterFormatting.Count == 1 && textRange.CharacterFormat.HasValue(106))
			{
				textRange.CharacterFormat.PropertiesHash.Remove(106);
			}
			mergeField.OwnerParagraph.Items.Insert(num, textRange);
			return;
		}
		List<string> list = new List<string>(text.Split(' ', ',', '.', '/', '-', ':', '\t', '\ufffd', '\ufffd'));
		WTextRange wTextRange = null;
		WCharacterFormat wCharacterFormat = null;
		foreach (string item in list)
		{
			WCharacterFormat wCharacterFormat2 = ((resultCharacterFormatting.Count > 0) ? resultCharacterFormatting[0] : null);
			if (wCharacterFormat != null && wCharacterFormat2 != null && wCharacterFormat.Compare(wCharacterFormat2))
			{
				text = text.Remove(0, item.Length);
				string text2 = item;
				if (text.Length > 0)
				{
					text2 += text[0];
					text = text.Remove(0, 1);
				}
				wTextRange.Text += text2;
			}
			else
			{
				wTextRange = new WTextRange(m_doc);
				text = text.Remove(0, item.Length);
				string text2 = item;
				if (text.Length > 0)
				{
					text2 += text[0];
					text = text.Remove(0, 1);
				}
				wTextRange.Text = text2;
				if (wCharacterFormat2 != null)
				{
					if (wCharacterFormat2.HasValue(106))
					{
						wCharacterFormat2.PropertiesHash.Remove(106);
					}
					wTextRange.ApplyCharacterFormat(wCharacterFormat2);
				}
				else if (mergeField.FieldSeparator != null && mergeField.FieldSeparator.NextSibling != null)
				{
					if ((mergeField.FieldSeparator.NextSibling as ParagraphItem).ParaItemCharFormat.HasValue(106))
					{
						wCharacterFormat2.PropertiesHash.Remove(106);
					}
					wTextRange.ApplyCharacterFormat((mergeField.FieldSeparator.NextSibling as ParagraphItem).ParaItemCharFormat);
				}
				mergeField.OwnerParagraph.Items.Insert(num, wTextRange);
				num++;
			}
			wCharacterFormat = wCharacterFormat2;
			if (resultCharacterFormatting.Count > 0)
			{
				resultCharacterFormatting.RemoveAt(0);
			}
		}
	}

	private object GetFieldValue(IWMergeField field, IRowsEnumerator rowsEnum)
	{
		string text = field.FieldName;
		string text2 = "";
		int num = 0;
		string[] array = null;
		if (IsNested && m_nestedEnums != null && m_nestedEnums.Count > 0)
		{
			if (text.Contains(":"))
			{
				text2 = text.Substring(0, text.IndexOf(":"));
			}
			if (!string.IsNullOrEmpty(text2) && m_nestedEnums.ContainsKey(text2))
			{
				rowsEnum = m_nestedEnums[text2];
				text = text.Remove(0, text2.Length + 1);
			}
			else
			{
				array = new string[m_nestedEnums.Count];
				m_nestedEnums.Keys.CopyTo(array, 0);
				if (m_nestedEnums.ContainsKey(rowsEnum.TableName))
				{
					using Dictionary<string, IRowsEnumerator>.Enumerator enumerator = m_nestedEnums.GetEnumerator();
					while (enumerator.MoveNext() && enumerator.Current.Value != rowsEnum)
					{
						num++;
					}
				}
				else
				{
					num = m_nestedEnums.Count;
				}
			}
		}
		string mappedColName = GetMappedColName(text);
		text = ((mappedColName == null) ? text.ToUpper() : mappedColName.ToUpper());
		object obj = null;
		while (obj == null && num >= 0)
		{
			obj = GetFieldValue(text, rowsEnum);
			if (obj == null && num > 0)
			{
				rowsEnum = m_nestedEnums[array[num - 1]];
				obj = GetFieldValue(text, rowsEnum);
				num--;
			}
			if (num == 0)
			{
				break;
			}
		}
		return obj;
	}

	private object GetFieldValue(string fieldName, IRowsEnumerator rowsEnum)
	{
		int i = 0;
		for (int num = rowsEnum.ColumnNames.Length; i < num; i++)
		{
			string text = rowsEnum.ColumnNames[i];
			string text2 = text.ToUpper();
			if (fieldName == text2 || fieldName == "\"" + text2 + "\"")
			{
				return rowsEnum.GetCellValue(text);
			}
		}
		return null;
	}

	private void UpdateIfFieldValue(WIfField field, IRowsEnumerator rowsEnum)
	{
		if (field.MergeFields.Count == 0 || rowsEnum == null)
		{
			return;
		}
		string text = null;
		int i = 0;
		for (int num = rowsEnum.ColumnNames.Length; i < num; i++)
		{
			text = rowsEnum.ColumnNames[i];
			string text2 = text.ToUpper();
			_ = string.Empty;
			PseudoMergeField pseudoMergeField = null;
			int j = 0;
			for (int count = field.MergeFields.Count; j < count; j++)
			{
				pseudoMergeField = field.MergeFields[j];
				if (pseudoMergeField.Name != null && pseudoMergeField.Name.ToUpper() == text2)
				{
					object cellValue = rowsEnum.GetCellValue(text);
					pseudoMergeField.Value = cellValue.ToString();
				}
			}
		}
	}

	private bool UpdateImageMergeFieldValue(WMergeField mergeField, IRowsEnumerator rowsEnum)
	{
		bool result = false;
		bool clearFields = ClearFields;
		if (rowsEnum == null && this.MergeImageField == null)
		{
			result = UpdateMergeFieldValue(mergeField);
		}
		else
		{
			MergeImageFieldEventArgs mergeImageFieldEventArgs = null;
			if (rowsEnum == null && this.MergeImageField != null)
			{
				string fieldName = mergeField.FieldName;
				object value = null;
				for (int i = 0; i < m_names.Length; i++)
				{
					if (fieldName != null && m_names[i].ToUpper() == fieldName.ToUpper())
					{
						value = m_values[i];
						break;
					}
				}
				value = TriggerSendBeforeClearFieldEvent(value, rowsEnum, mergeField);
				if (value != null)
				{
					MemoryStream imageByteStream = null;
					if (value.GetType() == typeof(byte[]))
					{
						imageByteStream = new MemoryStream((byte[])value);
					}
					Image image = GetImage(value);
					if (image != null)
					{
						value = image;
					}
					mergeImageFieldEventArgs = SendMergeImageField(mergeField, value, rowsEnum, imageByteStream);
					result = UpdateMergedPicture(mergeField, mergeImageFieldEventArgs);
				}
				else if (ClearFields)
				{
					result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
				}
			}
			else
			{
				object fieldValue = GetFieldValue(mergeField, rowsEnum);
				fieldValue = TriggerSendBeforeClearFieldEvent(fieldValue, rowsEnum, mergeField);
				if (fieldValue != null)
				{
					MemoryStream imageByteStream2 = null;
					Image image2 = GetImage(fieldValue);
					if (image2 != null)
					{
						if (fieldValue.GetType() == typeof(byte[]))
						{
							imageByteStream2 = new MemoryStream((byte[])fieldValue);
						}
						fieldValue = image2;
					}
					mergeImageFieldEventArgs = SendMergeImageField(mergeField, fieldValue, rowsEnum, imageByteStream2);
					result = UpdateMergedPicture(mergeField, mergeImageFieldEventArgs);
				}
				else if (ClearFields)
				{
					result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
				}
			}
		}
		ClearFields = clearFields;
		return result;
	}

	private bool UpdateMergedPicture(WMergeField mergeField, MergeImageFieldEventArgs args)
	{
		bool result = false;
		if (args.UseText)
		{
			if (mergeField.Owner is WParagraph && mergeField.FieldEnd != null && mergeField.FieldEnd.Owner is WParagraph)
			{
				if (!string.IsNullOrEmpty(args.Text))
				{
					InsertMergeFieldResultAsTextRange(mergeField, args);
					RemoveField(mergeField, isMergeStartAndEndPara: true);
					result = true;
				}
				else if (ClearFields)
				{
					RemoveField(mergeField, isMergeStartAndEndPara: true);
					result = true;
				}
			}
		}
		else if (!args.Skip && args.Image != null)
		{
			WParagraph ownerParagraph = mergeField.OwnerParagraph;
			int num = mergeField.GetIndexInOwnerCollection();
			if (!string.IsNullOrEmpty(mergeField.TextBefore))
			{
				WTextRange wTextRange = new WTextRange(m_doc);
				wTextRange.Text = mergeField.TextBefore;
				wTextRange.ApplyCharacterFormat(mergeField.CharacterFormat);
				ownerParagraph.Items.Insert(num, wTextRange);
				num++;
			}
			IWPicture picture = args.Picture;
			ownerParagraph.Items.Insert(num, picture);
			num++;
			picture.LoadImage(args.Image.ImageData);
			if (!string.IsNullOrEmpty(mergeField.TextAfter))
			{
				WTextRange wTextRange2 = new WTextRange(m_doc);
				wTextRange2.Text = mergeField.TextAfter;
				wTextRange2.ApplyCharacterFormat(mergeField.CharacterFormat);
				ownerParagraph.Items.Insert(num, wTextRange2);
			}
			result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
		}
		else if (ClearFields)
		{
			result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
		}
		return result;
	}

	private bool UpdateMergeFieldValue(WMergeField mergeField)
	{
		bool result = false;
		bool clearFields = ClearFields;
		if (ClearFields && m_values == null)
		{
			result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
		}
		else
		{
			int num = -1;
			string mappedColName = GetMappedColName(mergeField.FieldName);
			if (mappedColName != null)
			{
				for (int i = 0; i < m_names.Length; i++)
				{
					if (m_names[i].ToUpper() == mappedColName.ToUpper())
					{
						num = i;
						break;
					}
				}
			}
			if (num == -1)
			{
				for (int j = 0; j < m_names.Length; j++)
				{
					if (m_names[j].ToUpper() == mergeField.FieldName.ToUpper())
					{
						num = j;
						break;
					}
				}
			}
			if (num == -1 && this.BeforeClearField != null)
			{
				BeforeClearFieldEventArgs beforeClearFieldEventArgs = SendBeforeClearField(null, mergeField, null);
				if (beforeClearFieldEventArgs != null)
				{
					ClearFields = beforeClearFieldEventArgs.ClearField;
					if (!beforeClearFieldEventArgs.ClearField && beforeClearFieldEventArgs.FieldValue != null && !string.IsNullOrEmpty(beforeClearFieldEventArgs.FieldValue.ToString()))
					{
						num = m_values.Length;
						List<string> list = new List<string>(m_values);
						list.Add(beforeClearFieldEventArgs.FieldValue.ToString());
						m_values = list.ToArray();
					}
				}
			}
			if (num != -1 && num < m_values.Length)
			{
				MergeFieldEventArgs mergeFieldEventArgs = new MergeFieldEventArgs(Document, "", num, mergeField, m_values[num]);
				if (this.MergeField != null)
				{
					this.MergeField(this, mergeFieldEventArgs);
				}
				if (mergeFieldEventArgs.FieldValue != null)
				{
					if (mergeFieldEventArgs.TextRange.Text == mergeFieldEventArgs.Text)
					{
						mergeField.FieldResult = mergeFieldEventArgs.Text;
					}
					else
					{
						mergeField.FieldResult = mergeFieldEventArgs.TextRange.Text;
					}
					if (m_IfFieldCollections != null && m_IfFieldCollections.Count > 0)
					{
						EnusreDoubelQuotesForResult(m_IfFieldCollections.Peek(), mergeField);
					}
					if (mergeField.FieldResult == string.Empty)
					{
						if (ClearFields)
						{
							result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
						}
					}
					else if (mergeField.Owner is WParagraph && mergeField.FieldEnd != null && mergeField.FieldEnd.Owner is WParagraph)
					{
						InsertMergeFieldResultAsTextRange(mergeField, mergeFieldEventArgs);
						result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
					}
				}
				else if (ClearFields)
				{
					result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
				}
			}
			else if (ClearFields)
			{
				result = RemoveField(mergeField, isMergeStartAndEndPara: true) != -1;
			}
		}
		ClearFields = clearFields;
		return result;
	}

	private void CopyContent(WordDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		m_contentSections.Clear();
		document.Sections.CloneTo(m_contentSections);
	}

	private void AppendCopiedContent(WordDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		IWSection iWSection = null;
		int i = 0;
		for (int count = m_contentSections.Count; i < count; i++)
		{
			iWSection = m_contentSections[i];
			document.Sections.Add(iWSection.Clone());
			Document.Settings.DuplicateListStyleNames = string.Empty;
		}
	}

	private Image GetImage(object data)
	{
		Type type = data.GetType();
		if (type == typeof(byte[]))
		{
			MemoryStream stream = new MemoryStream((byte[])data);
			try
			{
				return new Image(stream);
			}
			catch
			{
				return null;
			}
		}
		if (type == typeof(WPicture))
		{
			return ((WPicture)data).Image;
		}
		if (type == typeof(Image))
		{
			return data as Image;
		}
		return null;
	}

	private void GetFieldNamesForParagraph(List<string> fieldsArray, TextBodyItem paragraph, string groupName)
	{
		if (paragraph is BlockContentControl)
		{
			GetFiledNamesForSDTBlockItems(fieldsArray, paragraph as BlockContentControl, groupName);
		}
		else if (paragraph is IWTable)
		{
			WTable wTable = paragraph as WTable;
			WTableRow wTableRow = null;
			WTableCell wTableCell = null;
			TextBodyItem textBodyItem = null;
			int i = 0;
			for (int count = wTable.Rows.Count; i < count; i++)
			{
				wTableRow = wTable.Rows[i];
				int j = 0;
				for (int count2 = wTableRow.Cells.Count; j < count2; j++)
				{
					wTableCell = wTableRow.Cells[j];
					int k = 0;
					for (int count3 = wTableCell.ChildEntities.Count; k < count3; k++)
					{
						textBodyItem = wTableCell.Items[k];
						GetFieldNamesForParagraph(fieldsArray, textBodyItem, groupName);
					}
				}
			}
		}
		else
		{
			int l = 0;
			for (int count4 = (paragraph as WParagraph).Items.Count; l < count4; l++)
			{
				ParagraphItem item = (paragraph as WParagraph)[l];
				GetFieldNamesForParagraphItems(fieldsArray, item, groupName);
			}
		}
	}

	private void GetFieldNamesForParagraphItems(List<string> fieldsArray, ParagraphItem item, string groupName)
	{
		if (item is WMergeField { FieldType: FieldType.FieldMergeField } wMergeField)
		{
			if (wMergeField.FieldName == groupName)
			{
				if (!IsBeginGroupFound && IsBeginGroup(wMergeField))
				{
					IsBeginGroupFound = true;
					IsEndGroupFound = false;
				}
				if (!IsEndGroupFound && IsEndGroup(wMergeField))
				{
					IsEndGroupFound = true;
					IsBeginGroupFound = false;
				}
			}
			else if (groupName == null || (IsBeginGroupFound && !IsEndGroupFound))
			{
				fieldsArray.Add(wMergeField.FieldName);
			}
		}
		else if (item is WTextBox)
		{
			WTextBox wTextBox = item as WTextBox;
			int count = wTextBox.TextBoxBody.Items.Count;
			for (int i = 0; i < count; i++)
			{
				GetFieldNamesForParagraph(fieldsArray, wTextBox.TextBoxBody.Items[i], groupName);
			}
		}
		else if (item is Shape)
		{
			Shape shape = item as Shape;
			int count2 = shape.TextBody.Items.Count;
			for (int j = 0; j < count2; j++)
			{
				GetFieldNamesForParagraph(fieldsArray, shape.TextBody.Items[j], groupName);
			}
		}
		else
		{
			if (!(item is InlineContentControl))
			{
				return;
			}
			foreach (ParagraphItem paragraphItem in (item as InlineContentControl).ParagraphItems)
			{
				GetFieldNamesForParagraphItems(fieldsArray, paragraphItem, groupName);
			}
		}
	}

	private void GetFiledNamesForSDTBlockItems(List<string> fieldsArray, BlockContentControl structureDocumentTagBlocklockContent, string groupName)
	{
		for (int i = 0; i < structureDocumentTagBlocklockContent.TextBody.ChildEntities.Count; i++)
		{
			GetFieldNamesForParagraph(fieldsArray, structureDocumentTagBlocklockContent.TextBody.ChildEntities[i] as TextBodyItem, groupName);
		}
	}

	private static bool IsBeginGroup(WMergeField field)
	{
		string prefix = field.Prefix;
		if (!(prefix == "TableStart"))
		{
			return prefix == "BeginGroup";
		}
		return true;
	}

	private static bool IsEndGroup(WMergeField field)
	{
		string prefix = field.Prefix;
		if (!(prefix == "TableEnd"))
		{
			return prefix == "EndGroup";
		}
		return true;
	}

	private bool CheckSelection(IRowsEnumerator rowsEnum)
	{
		if (rowsEnum.RowsCount > 0)
		{
			return true;
		}
		GroupSelector groupSelector = m_groupSelector;
		bool clearFields = ClearFields;
		if (this.BeforeClearGroupField != null && rowsEnum.RowsCount == 0)
		{
			BeforeClearGroupFieldEventArgs beforeClearGroupFieldEventArgs = SendBeforeClearGroup(rowsEnum, groupSelector);
			if (beforeClearGroupFieldEventArgs != null)
			{
				ClearFields = beforeClearGroupFieldEventArgs.ClearGroup;
			}
		}
		if (!ClearFields && !RemoveEmptyParagraphs)
		{
			return true;
		}
		if (groupSelector.GroupSelection != null && ClearFields)
		{
			RemoveEmptyMergeFieldsInBodyItems(groupSelector.GroupSelection);
		}
		else if (groupSelector.RowSelection != null && ClearFields)
		{
			int startRowIndex = groupSelector.RowSelection.StartRowIndex;
			int endRowIndex = groupSelector.RowSelection.EndRowIndex;
			for (int i = startRowIndex; i <= endRowIndex; i++)
			{
				if (groupSelector.RowSelection.Table.Rows.Count > startRowIndex)
				{
					RemoveEmptyMergeFieldsInTableRow(groupSelector.RowSelection.Table.Rows[i]);
				}
			}
		}
		ClearFields = clearFields;
		return false;
	}

	private void HideFields(IWSectionCollection sections)
	{
		int i = 0;
		for (int count = sections.Count; i < count; i++)
		{
			ExecuteForSection(sections[i], null);
		}
	}

	private void RemoveEmptyMergeFieldsInTableRow(WTableRow row)
	{
		WTableCell wTableCell = null;
		int i = 0;
		for (int count = row.Cells.Count; i < count; i++)
		{
			wTableCell = row.Cells[i];
			RemoveEmptyMergeFieldsInBodyItems(wTableCell.Items);
		}
	}

	private void RemoveEmptyMergeFieldsInBodyItems(TextBodySelection selection)
	{
		TextBodyItem textBodyItem = null;
		if (selection == null)
		{
			return;
		}
		int num = selection.ItemEndIndex + 1;
		for (int i = selection.ItemStartIndex; i < num; i++)
		{
			textBodyItem = selection.TextBody.Items[i];
			if (textBodyItem is WParagraph)
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				if (RemoveEmptyMergeFieldsInParagraph(wParagraph) && RemoveEmptyParagraphs && num > 1)
				{
					selection.TextBody.Items.Remove(wParagraph);
					i--;
					num--;
				}
			}
			else if (textBodyItem is WTable)
			{
				WTable wTable = textBodyItem as WTable;
				int j = 0;
				for (int count = wTable.Rows.Count; j < count; j++)
				{
					RemoveEmptyMergeFieldsInTableRow(wTable.Rows[j]);
				}
			}
		}
	}

	private void RemoveEmptyMergeFieldsInBodyItems(BodyItemCollection items)
	{
		TextBodyItem textBodyItem = null;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			textBodyItem = items[num];
			if (textBodyItem is WParagraph)
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				if (RemoveEmptyMergeFieldsInParagraph(wParagraph) && RemoveEmptyParagraphs && items.Count > 1)
				{
					items.Remove(wParagraph);
				}
			}
			else if (textBodyItem is WTable)
			{
				WTable wTable = textBodyItem as WTable;
				int i = 0;
				for (int count = wTable.Rows.Count; i < count; i++)
				{
					RemoveEmptyMergeFieldsInTableRow(wTable.Rows[i]);
				}
			}
		}
	}

	private bool RemoveEmptyMergeFieldsInParagraph(WParagraph para)
	{
		bool result = false;
		WField wField = null;
		int i = 0;
		for (int count = para.Items.Count; i < count; i++)
		{
			if (para.Items[i] is WField)
			{
				bool flag = false;
				wField = para.Items[i] as WField;
				if (wField.FieldType == FieldType.FieldMergeField)
				{
					flag = RemoveField(wField, isMergeStartAndEndPara: false) != -1;
				}
				else if (wField.FieldType == FieldType.FieldNext)
				{
					flag = RemoveField(wField, isMergeStartAndEndPara: false) != -1;
				}
				if (para.Items.Count == 0 && flag)
				{
					result = true;
					break;
				}
				if (flag)
				{
					i--;
					count = para.Items.Count;
				}
			}
			else if (para.Items[i] is WTextBox)
			{
				RemoveEmptyMergeFieldsInBodyItems((para.Items[i] as WTextBox).TextBoxBody.Items);
			}
			else if (para.Items[i] is Shape)
			{
				RemoveEmptyMergeFieldsInBodyItems((para.Items[i] as Shape).TextBody.Items);
			}
		}
		return result;
	}

	private bool HideField(WField field, bool hide)
	{
		bool result = false;
		if (field is WMergeField && (IsBeginGroup(field as WMergeField) || IsEndGroup(field as WMergeField) || hide))
		{
			RemoveField(field, isMergeStartAndEndPara: false);
			result = true;
		}
		return result;
	}

	private string GetMappedColName(string fieldName)
	{
		if (m_mappedFields != null && m_mappedFields.ContainsKey(fieldName))
		{
			return m_mappedFields[fieldName];
		}
		return null;
	}

	private void RemoveSpellChecking()
	{
		if (Document.GrammarSpellingData != null)
		{
			Document.GrammarSpellingData.PlcfgramData = null;
			Document.GrammarSpellingData.PlcfsplData = null;
		}
	}
}
