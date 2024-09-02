using System;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation;

internal class AddInFunctionImpl : CommonObject, IAddInFunction, IParentApplication, ICloneParent
{
	private int m_iBookIndex;

	private int m_iNameIndex;

	private WorkbookImpl m_book;

	public int BookIndex
	{
		get
		{
			return m_iBookIndex;
		}
		set
		{
			m_iBookIndex = value;
		}
	}

	public int NameIndex
	{
		get
		{
			return m_iNameIndex;
		}
		set
		{
			m_iNameIndex = value;
		}
	}

	public string Name => null;

	public AddInFunctionImpl(IApplication application, object parent, int iBookIndex, int iNameIndex)
		: base(application, parent)
	{
		m_iBookIndex = iBookIndex;
		m_iNameIndex = iNameIndex;
		SetParents();
	}

	private void SetParents()
	{
		m_book = FindParent(typeof(WorkbookImpl)) as WorkbookImpl;
		if (m_book == null)
		{
			throw new ArgumentNullException("m_book");
		}
	}

	public object Clone(object parent)
	{
		AddInFunctionImpl obj = (AddInFunctionImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.SetParents();
		return obj;
	}
}
