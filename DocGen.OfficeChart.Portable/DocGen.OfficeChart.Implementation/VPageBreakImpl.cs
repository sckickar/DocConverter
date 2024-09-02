using System;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class VPageBreakImpl : CommonObject, IVPageBreak
{
	private VerticalPageBreaksRecord.TVPageBreak m_vPageBreak;

	private ExcelPageBreak m_type = ExcelPageBreak.PageBreakManual;

	private WorksheetImpl m_sheet;

	public IRange Location
	{
		get
		{
			return m_sheet.Range[(int)(m_vPageBreak.StartRow + 1), m_vPageBreak.Column + 1, (int)(m_vPageBreak.EndRow + 1), m_vPageBreak.Column + 1];
		}
		set
		{
			m_vPageBreak.Column = (ushort)(value.Column - 1);
			m_vPageBreak.StartRow = (ushort)(value.Row - 1);
			m_vPageBreak.EndRow = (ushort)(value.LastRow - 1);
		}
	}

	public ExcelPageBreak Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	[CLSCompliant(false)]
	public VerticalPageBreaksRecord.TVPageBreak VPageBreak
	{
		get
		{
			if (m_vPageBreak == null)
			{
				throw new ArgumentNullException("VPageBreak");
			}
			return m_vPageBreak;
		}
		set
		{
			m_vPageBreak = value;
		}
	}

	public int Column
	{
		get
		{
			return m_vPageBreak.Column;
		}
		internal set
		{
			m_vPageBreak.Column = (ushort)value;
		}
	}

	public VPageBreakImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	private VPageBreakImpl(IApplication application, object parent, BiffReader reader)
		: this(application, parent)
	{
	}

	[CLSCompliant(false)]
	public VPageBreakImpl(IApplication application, object parent, VerticalPageBreaksRecord.TVPageBreak pagebreak)
		: this(application, parent)
	{
		m_vPageBreak = pagebreak;
		m_type = ExcelPageBreak.PageBreakManual;
	}

	public VPageBreakImpl(IApplication application, object parent, IRange location)
		: this(application, parent)
	{
		m_vPageBreak = new VerticalPageBreaksRecord.TVPageBreak();
		m_vPageBreak.Column = (ushort)(location.Column - 1);
		m_vPageBreak.StartRow = (ushort)(location.Row - 1);
		m_vPageBreak.EndRow = (ushort)(location.LastRow - 1);
	}

	private void FindParents()
	{
		object obj = FindParent(typeof(WorksheetImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent worksheet");
		}
		m_sheet = (WorksheetImpl)obj;
	}

	public VPageBreakImpl Clone(object parent)
	{
		VPageBreakImpl obj = (VPageBreakImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.FindParents();
		m_vPageBreak = (VerticalPageBreaksRecord.TVPageBreak)m_vPageBreak.Clone();
		return obj;
	}
}
