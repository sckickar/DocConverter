using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorkbookObjectsCollection : CollectionBaseEx<object>, ITabSheets
{
	private Dictionary<string, int> m_hashNameToValue = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

	private WorkbookImpl m_book;

	[CLSCompliant(false)]
	public new ISerializableNamedObject this[int index]
	{
		get
		{
			if (index < 0 || index >= base.List.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return base.List[index] as ISerializableNamedObject;
		}
	}

	public INamedObject this[string name]
	{
		get
		{
			if (!m_hashNameToValue.TryGetValue(name, out var value))
			{
				return null;
			}
			return this[value];
		}
	}

	public IWorkbook Workbook => m_book;

	ITabSheet ITabSheets.this[int index]
	{
		get
		{
			if (index < 0 || index > base.Count - 1)
			{
				throw new ArgumentOutOfRangeException("index", "Value cannot be less than 0 and greater than Count - 1.");
			}
			return (ITabSheet)base.InnerList[index];
		}
	}

	internal event TabSheetMovedEventHandler TabSheetMoved;

	public WorkbookObjectsCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	[CLSCompliant(false)]
	public void Add(ISerializableNamedObject namedObject)
	{
		if (namedObject == null)
		{
			throw new ArgumentNullException("workbookObject");
		}
		if (namedObject.Name == null)
		{
			throw new ArgumentNullException("Name can't be NULL.");
		}
		if (m_hashNameToValue.ContainsKey(namedObject.Name))
		{
			throw new ArgumentException("Sheet Name is already existed in workbook");
		}
		int value = (namedObject.RealIndex = base.List.Count);
		m_hashNameToValue.Add(namedObject.Name, value);
		base.InnerList.Add(namedObject);
		namedObject.NameChanged += object_NameChanged;
	}

	public void Move(int iOldIndex, int iNewIndex)
	{
		if (iOldIndex != iNewIndex)
		{
			ISerializableNamedObject item = this[iOldIndex];
			base.InnerList.RemoveAt(iOldIndex);
			base.InnerList.Insert(iNewIndex, item);
			int num = Math.Min(iNewIndex, iOldIndex);
			int num2 = Math.Max(iNewIndex, iOldIndex);
			for (int i = num; i <= num2; i++)
			{
				this[i].RealIndex = i;
			}
			m_book.MoveSheetIndex(iOldIndex, iNewIndex);
			m_book.UpdateActiveSheetAfterMove(iOldIndex, iNewIndex);
			if (this.TabSheetMoved != null)
			{
				TabSheetMovedEventArgs args = new TabSheetMovedEventArgs(iOldIndex, iNewIndex);
				this.TabSheetMoved(this, args);
			}
		}
	}

	public void MoveBefore(ITabSheet sheetToMove, ITabSheet sheetForPlacement)
	{
		ISerializableNamedObject obj = (ISerializableNamedObject)sheetToMove;
		ISerializableNamedObject serializableNamedObject = (ISerializableNamedObject)sheetForPlacement;
		int realIndex = obj.RealIndex;
		int realIndex2 = serializableNamedObject.RealIndex;
		int iNewIndex = ((realIndex > realIndex2) ? realIndex2 : (realIndex2 - 1));
		Move(realIndex, iNewIndex);
	}

	public void MoveAfter(ITabSheet sheetToMove, ITabSheet sheetForPlacement)
	{
		ISerializableNamedObject obj = (ISerializableNamedObject)sheetToMove;
		ISerializableNamedObject serializableNamedObject = (ISerializableNamedObject)sheetForPlacement;
		int realIndex = obj.RealIndex;
		int realIndex2 = serializableNamedObject.RealIndex;
		int iNewIndex = ((realIndex > realIndex2) ? (realIndex2 + 1) : realIndex2);
		Move(realIndex, iNewIndex);
	}

	public void DisposeInternalData()
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			if (base.InnerList[i] is WorksheetBaseImpl worksheetBaseImpl)
			{
				worksheetBaseImpl.Dispose();
			}
		}
	}

	public override object Clone(object parent)
	{
		WorkbookObjectsCollection workbookObjectsCollection = new WorkbookObjectsCollection(base.Application, parent);
		List<object> innerList = base.InnerList;
		IList<object> list = workbookObjectsCollection.List;
		workbookObjectsCollection.m_book.Objects = workbookObjectsCollection;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			object item = (innerList[i] as WorksheetBaseImpl).Clone(workbookObjectsCollection, cloneShapes: false);
			list.Add(item);
		}
		int j = 0;
		for (int count2 = innerList.Count; j < count2; j++)
		{
			WorksheetBaseImpl obj = innerList[j] as WorksheetBaseImpl;
			WorksheetBaseImpl result = list[j] as WorksheetBaseImpl;
			obj.CloneShapes(result);
		}
		return workbookObjectsCollection;
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
	}

	protected override void OnInsertComplete(int index, object value)
	{
		ISerializableNamedObject serializableNamedObject = (ISerializableNamedObject)value;
		serializableNamedObject.NameChanged += object_NameChanged;
		m_hashNameToValue[serializableNamedObject.Name] = index;
		int i = index;
		for (int count = base.List.Count; i < count; i++)
		{
			this[index].RealIndex = i;
		}
		m_book.IncreaseSheetIndex(index);
		base.OnInsertComplete(index, value);
	}

	protected override void OnSetComplete(int index, object oldValue, object newValue)
	{
		WorksheetImpl worksheetImpl = (WorksheetImpl)oldValue;
		WorksheetImpl worksheetImpl2 = (WorksheetImpl)newValue;
		worksheetImpl.NameChanged -= object_NameChanged;
		m_hashNameToValue.Remove(worksheetImpl.Name);
		m_hashNameToValue[worksheetImpl2.Name] = index;
		base.OnSetComplete(index, oldValue, newValue);
	}

	protected override void OnRemoveComplete(int index, object value)
	{
		ISerializableNamedObject serializableNamedObject = (ISerializableNamedObject)value;
		serializableNamedObject.NameChanged -= object_NameChanged;
		m_hashNameToValue.Remove(serializableNamedObject.Name);
		int count = base.List.Count;
		for (int i = index; i < count; i++)
		{
			ISerializableNamedObject serializableNamedObject2 = this[i];
			serializableNamedObject2.RealIndex = i;
			m_hashNameToValue[serializableNamedObject2.Name] = i;
		}
		m_book.DecreaseSheetIndex(index);
		int num = m_book.ActiveSheetIndex;
		if (index < m_book.ActiveSheetIndex || (index == m_book.ActiveSheetIndex && index == count))
		{
			num--;
			FindVisibleWorksheet(num);
		}
		(this[num] as ITabSheet).Activate();
		base.OnRemoveComplete(index, value);
	}

	private void FindVisibleWorksheet(int proposedIndex)
	{
		if ((this[proposedIndex] as ITabSheet).Visibility == OfficeWorksheetVisibility.Visible)
		{
			m_book.ActiveSheetIndex = proposedIndex;
			return;
		}
		int num = -1;
		for (int num2 = proposedIndex - 1; num2 >= 0; num2--)
		{
			if ((this[num2] as ITabSheet).Visibility == OfficeWorksheetVisibility.Visible)
			{
				num = num2;
				break;
			}
		}
		if (num == -1)
		{
			int num3 = proposedIndex + 1;
			int count = base.Count;
			while (num3 < count)
			{
				if ((this[num3] as ITabSheet).Visibility == OfficeWorksheetVisibility.Visible)
				{
					num = num3;
					break;
				}
				num3--;
			}
		}
		if (num == -1)
		{
			throw new Exception("A workbook must contain at least one visible worksheet. To hide, delete, or move the selected sheet(s), you must first insert a new sheet or unhide a sheet that is already hidden.");
		}
		m_book.ActiveSheetIndex = num;
	}

	protected override void OnClearComplete()
	{
		base.OnClearComplete();
		m_hashNameToValue.Clear();
	}

	private void object_NameChanged(object sender, ValueChangedEventArgs e)
	{
		string key = (string)e.newValue;
		if (m_hashNameToValue.ContainsKey(key))
		{
			throw new ArgumentException("Name of worksheet must be unique in a workbook.");
		}
		string key2 = (string)e.oldValue;
		int value = m_hashNameToValue[key2];
		m_hashNameToValue.Remove(key2);
		m_hashNameToValue[key] = value;
	}
}
