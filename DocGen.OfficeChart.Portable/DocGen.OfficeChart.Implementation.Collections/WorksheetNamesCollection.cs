using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorksheetNamesCollection : CollectionBaseEx<IName>, INames, IEnumerable
{
	private Dictionary<string, IName> m_hashNameToIName = new Dictionary<string, IName>();

	private WorkbookImpl m_book;

	private WorksheetImpl m_worksheet;

	public IName this[string name]
	{
		get
		{
			m_hashNameToIName.TryGetValue(name, out var value);
			if (value == null)
			{
				return null;
			}
			if (!(value as NameImpl).IsDeleted)
			{
				return value;
			}
			return null;
		}
	}

	public IWorksheet ParentWorksheet => m_worksheet;

	int INames.Count => GetKnownNamedCount();

	public new IName this[int index]
	{
		get
		{
			if (index < 0 || index >= base.List.Count)
			{
				throw new ArgumentOutOfRangeException($"index is {index}, Count is {base.List.Count}");
			}
			int num = 0;
			for (int i = 0; i < base.List.Count; i++)
			{
				NameImpl nameImpl = base.List[i] as NameImpl;
				if (!nameImpl.IsDeleted)
				{
					if (num == index)
					{
						return nameImpl;
					}
					num++;
				}
			}
			return null;
		}
	}

	public WorksheetNamesCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	public void Rename(IName name, string strOldName)
	{
		if (Contains(strOldName))
		{
			m_hashNameToIName.Remove(strOldName);
			m_hashNameToIName.Add(name.Name, name);
		}
	}

	public IName Add(string name)
	{
		m_hashNameToIName.TryGetValue(name, out var value);
		if (value != null)
		{
			(value as NameImpl).IsDeleted = false;
			return value;
		}
		IName name2 = new NameImpl(base.Application, this, name, base.Count, bIsLocal: true);
		Add(name2);
		return name2;
	}

	public IName Add(string name, IRange namedRange)
	{
		NameImpl nameImpl = new NameImpl(base.Application, this, name, namedRange, base.Count, bIsLocal: true);
		Add(nameImpl);
		return nameImpl;
	}

	public new IName Add(IName name)
	{
		return Add(name, bAddInGlobalNamesHash: true);
	}

	public IName Add(IName name, bool bAddInGlobalNamesHash)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		base.Add(name);
		((WorkbookNamesCollection)m_book.Names).AddLocal(name, bAddInGlobalNamesHash);
		return name;
	}

	public void Remove(string name)
	{
		if (m_hashNameToIName.TryGetValue(name, out var value))
		{
			value.Delete();
		}
	}

	public new void Clear()
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			Remove(this[num].Name);
		}
	}

	public bool Contains(string name)
	{
		if (m_hashNameToIName.ContainsKey(name))
		{
			return !(m_hashNameToIName[name] as NameImpl).IsDeleted;
		}
		return false;
	}

	internal bool Contains(string name, bool isFormulaNamedrange)
	{
		if (isFormulaNamedrange)
		{
			if (m_hashNameToIName.ContainsKey(name))
			{
				return !(m_hashNameToIName[name] as NameImpl).IsDeleted;
			}
			return false;
		}
		if (m_hashNameToIName.ContainsKey(name) && !(m_hashNameToIName[name] as NameImpl).IsDeleted)
		{
			return !(m_hashNameToIName[name] as NameImpl).m_isFormulaNamedRange;
		}
		return false;
	}

	private int GetKnownNamedCount()
	{
		int num = 0;
		foreach (IName item in base.List)
		{
			if (item.RefersToRange != null)
			{
				num++;
			}
		}
		return num;
	}

	public IName AddLocal(IName name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		base.Add(name);
		return name;
	}

	[CLSCompliant(false)]
	public IName Add(NameRecord name)
	{
		return Add(name, bAddInGlobalNamesHash: true);
	}

	[CLSCompliant(false)]
	public IName Add(NameRecord name, bool bAddInGlobalNamesHash)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		NameImpl nameImpl = new NameImpl(base.Application, this, name.Name, base.Count);
		nameImpl.Parse(name);
		((IParseable)nameImpl).Parse();
		Add(nameImpl, bAddInGlobalNamesHash);
		return nameImpl;
	}

	[CLSCompliant(false)]
	public void AddRange(NameRecord[] names)
	{
		if (names == null)
		{
			throw new ArgumentNullException("names");
		}
		string value = "'" + m_worksheet.Name + "'";
		FormulaUtil formulaUtil = m_book.FormulaUtil;
		int i = 0;
		for (int num = names.Length; i < num; i++)
		{
			NameRecord nameRecord = names[i];
			if (formulaUtil.ParsePtgArray(nameRecord.FormulaTokens, 0, 0, bR1C1: false, isForSerialization: false).StartsWith(value))
			{
				Add(nameRecord);
			}
		}
	}

	internal void FillFrom(WorksheetNamesCollection sourceNames, IDictionary hashNewWorksheetNames, Dictionary<int, int> dicNewNameIndexes, ExcelNamesMergeOptions option, Dictionary<int, int> hashExternSheetIndexes)
	{
		if (sourceNames == null)
		{
			throw new ArgumentNullException("sourceNames");
		}
		if (hashExternSheetIndexes == null)
		{
			throw new ArgumentNullException("hashExternSheetIndexes");
		}
		WorkbookImpl book = sourceNames.m_book;
		int i = 0;
		for (int count = sourceNames.Count; i < count; i++)
		{
			NameImpl nameImpl = (NameImpl)sourceNames.InnerList[i];
			NameRecord nameRecord = (NameRecord)nameImpl.Record.Clone();
			nameRecord.IndexOrGlobal = (ushort)(m_worksheet.RealIndex + 1);
			UpdateReferenceIndexes(nameRecord, book, hashNewWorksheetNames, hashExternSheetIndexes, m_book);
			IName name = Add(nameRecord);
			dicNewNameIndexes.Add(nameImpl.Index, (name as NameImpl).Index);
		}
	}

	internal static void UpdateReferenceIndexes(NameRecord name, WorkbookImpl oldBook, IDictionary hashNewWorksheetNames, Dictionary<int, int> hashExternSheetIndexes, WorkbookImpl newBook)
	{
		if (hashExternSheetIndexes == null)
		{
			return;
		}
		Ptg[] formulaTokens = name.FormulaTokens;
		if (formulaTokens == null || formulaTokens.Length == 0)
		{
			return;
		}
		if (oldBook == null)
		{
			throw new ArgumentException("oldBook");
		}
		if (newBook == null)
		{
			throw new ArgumentNullException("newBook");
		}
		int i = 0;
		for (int num = formulaTokens.Length; i < num; i++)
		{
			if (!(formulaTokens[i] is IReference reference))
			{
				continue;
			}
			int refIndex = reference.RefIndex;
			string text = oldBook.GetSheetNameByReference(refIndex);
			if (text == null)
			{
				text = "#REF";
			}
			if (text != null && hashNewWorksheetNames.Contains(text))
			{
				text = (string)hashNewWorksheetNames[text];
			}
			if (text == "#REF")
			{
				if (hashExternSheetIndexes.ContainsKey(refIndex))
				{
					int num2 = hashExternSheetIndexes[refIndex];
					reference.RefIndex = (ushort)num2;
				}
			}
			else
			{
				refIndex = newBook.AddSheetReference(text);
				reference.RefIndex = (ushort)refIndex;
			}
		}
	}

	public void SetSheetIndex(int iSheetIndex)
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			((NameImpl)base.InnerList[num]).SetSheetIndex(iSheetIndex);
		}
	}

	public NameImpl GetOrCreateName(string strName)
	{
		if (strName == null)
		{
			throw new ArgumentNullException("strName");
		}
		if (strName.Length == 0)
		{
			throw new ArgumentException("strName - string cannot be empty.");
		}
		NameImpl nameImpl = this[strName] as NameImpl;
		if (nameImpl == null)
		{
			nameImpl = (NameImpl)Add(strName);
		}
		return nameImpl;
	}

	public void ConvertFullRowColumnNames(OfficeVersion version)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			((NameImpl)GetNameByIndex(i)).ConvertFullRowColumnName(version);
		}
	}

	internal IName GetNameByIndex(int index)
	{
		return base.List[index];
	}

	private void SetParents()
	{
		m_worksheet = FindParent(typeof(WorksheetImpl)) as WorksheetImpl;
		if (m_worksheet == null)
		{
			throw new ArgumentNullException("WorksheetNamesCollection has no parent Worksheet.");
		}
		m_book = m_worksheet.ParentWorkbook;
	}

	protected override void OnInsertComplete(int index, IName value)
	{
		string name = ((NameImpl)value).Name;
		if (!m_book.IsWorkbookOpening || !m_hashNameToIName.ContainsKey(name))
		{
			m_hashNameToIName[name] = value;
		}
		base.OnInsertComplete(index, value);
	}
}
