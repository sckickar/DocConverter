using System;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class ExternNameImpl : CommonObject, INameIndexChangedEventProvider, ICloneParent
{
	private ExternNameRecord m_name;

	private int m_iIndex;

	private ExternWorkbookImpl m_externBook;

	private string m_refersTo;

	public int sheetId;

	public int Index
	{
		get
		{
			return m_iIndex;
		}
		set
		{
			if (value != m_iIndex)
			{
				int iIndex = m_iIndex;
				m_iIndex = value;
				NameIndexChangedEventArgs args = new NameIndexChangedEventArgs(iIndex, m_iIndex);
				RaiseIndexChangedEvent(args);
			}
		}
	}

	public string Name => m_name.Name;

	public int BookIndex => m_externBook.Index;

	internal ExternNameRecord Record => m_name;

	internal string RefersTo
	{
		get
		{
			return m_refersTo;
		}
		set
		{
			m_refersTo = value;
		}
	}

	public event NameImpl.NameIndexChangedEventHandler NameIndexChanged;

	[CLSCompliant(false)]
	public ExternNameImpl(IApplication application, object parent, ExternNameRecord name, int index)
		: base(application, parent)
	{
		m_name = name;
		m_iIndex = index;
		SetParents();
	}

	private void SetParents()
	{
		m_externBook = FindParent(typeof(ExternWorkbookImpl)) as ExternWorkbookImpl;
		if (m_externBook == null)
		{
			throw new ArgumentNullException("Can't find parent extern workbook");
		}
	}

	private void RaiseIndexChangedEvent(NameIndexChangedEventArgs args)
	{
		if (this.NameIndexChanged != null)
		{
			this.NameIndexChanged(this, args);
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_name);
	}

	public object Clone(object parent)
	{
		ExternNameImpl obj = (ExternNameImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.SetParents();
		m_name = (ExternNameRecord)CloneUtils.CloneCloneable(m_name);
		return obj;
	}
}
