using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class ExternNamesCollection : CollectionBaseEx<ExternNameImpl>
{
	private ExternWorkbookImpl m_externBook;

	private List<ExternNameImpl> m_hashNames = new List<ExternNameImpl>();

	private SortedList<int, object> m_lstToRemove = new SortedList<int, object>();

	public new ExternNameImpl this[int index]
	{
		get
		{
			if (index < 0 || index > base.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than Count");
			}
			return base.List[index];
		}
	}

	public ExternNameImpl this[string name]
	{
		get
		{
			int nameIndex = GetNameIndex(name);
			if (nameIndex >= 0 && nameIndex <= base.Count)
			{
				return base.List[nameIndex];
			}
			return null;
		}
	}

	public ExternWorkbookImpl ParentWorkbook => m_externBook;

	public ExternNamesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		base.Removed += ExternNamesCollection_Removed;
		base.Inserted += ExternNamesCollection_Inserted;
	}

	private void SetParents()
	{
		m_externBook = FindParent(typeof(ExternWorkbookImpl)) as ExternWorkbookImpl;
		if (m_externBook == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	[CLSCompliant(false)]
	public int Add(ExternNameRecord name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		_ = m_externBook.Workbook;
		ExternNameImpl item = new ExternNameImpl(base.Application, this, name, base.List.Count);
		base.Add(item);
		if (!m_hashNames.Contains(item))
		{
			m_hashNames.Add(item);
		}
		return base.Count - 1;
	}

	public int Add(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name - string cannot be empty");
		}
		ExternNameRecord externNameRecord = (ExternNameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExternName);
		externNameRecord.Name = name;
		return Add(externNameRecord);
	}

	public int Add(string name, bool isAddIn)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("name - string cannot be empty");
		}
		ExternNameRecord externNameRecord = (ExternNameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ExternName);
		externNameRecord.Name = name;
		if (isAddIn)
		{
			externNameRecord.IsAddIn = true;
		}
		return Add(externNameRecord);
	}

	public bool Contains(string name)
	{
		for (int i = 0; i < m_hashNames.Count; i++)
		{
			if (m_hashNames[i].Name == name)
			{
				return true;
			}
		}
		return false;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			this[i].Serialize(records);
		}
	}

	public int GetNameIndex(string strName)
	{
		for (int i = 0; i < m_hashNames.Count; i++)
		{
			if (m_hashNames[i].Name == strName)
			{
				return m_hashNames[i].Index;
			}
		}
		return -1;
	}

	public int GetNewIndex(int iNameIndex)
	{
		int num = m_lstToRemove.IndexOfKey(iNameIndex);
		if (num != -1)
		{
			return iNameIndex - num - 1;
		}
		return iNameIndex;
	}

	public override object Clone(object parent)
	{
		ExternNamesCollection result = (ExternNamesCollection)base.Clone(parent);
		IList<int> keys = m_lstToRemove.Keys;
		int i = 0;
		for (int count = m_lstToRemove.Count; i < count; i++)
		{
			int key = keys[i];
			m_lstToRemove.Add(key, null);
		}
		return result;
	}

	private new int Add(ExternNameImpl name)
	{
		base.Add(name);
		return base.Count - 1;
	}

	private void ExternNamesCollection_Removed(object sender, CollectionChangeEventArgs<ExternNameImpl> args)
	{
		int i = args.Index;
		for (int count = base.Count; i < count; i++)
		{
			this[i].Index = i;
		}
		ExternNameImpl value = args.Value;
		if (!value.Record.NeedDataArray)
		{
			m_hashNames.Remove(value);
		}
	}

	private void ExternNamesCollection_Inserted(object sender, CollectionChangeEventArgs<ExternNameImpl> args)
	{
		ExternNameImpl value = args.Value;
		if (!value.Record.NeedDataArray)
		{
			m_hashNames.Add(value);
		}
	}
}
