using System;
using System.Collections;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public class WHeadersFooters : XDLSSerializableBase, IEnumerable
{
	internal class HFEnumerator : IEnumerator
	{
		private int m_index = -1;

		private WHeadersFooters m_hfs;

		public object Current
		{
			get
			{
				if (m_index >= 0)
				{
					return m_hfs[m_index];
				}
				return null;
			}
		}

		internal HFEnumerator(WHeadersFooters hfs)
		{
			m_hfs = hfs;
		}

		public bool MoveNext()
		{
			if (m_index < 5)
			{
				m_index++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			m_index = -1;
		}
	}

	private HeaderFooter m_evenHeader;

	private HeaderFooter m_oddFooter;

	private HeaderFooter m_oddHeader;

	private HeaderFooter m_evenFooter;

	private HeaderFooter m_firstPageHeader;

	private HeaderFooter m_firstPageFooter;

	public HeaderFooter Header => OddHeader;

	public HeaderFooter Footer => OddFooter;

	public HeaderFooter EvenHeader => m_evenHeader;

	public HeaderFooter OddHeader => m_oddHeader;

	public HeaderFooter EvenFooter => m_evenFooter;

	public HeaderFooter OddFooter => m_oddFooter;

	public HeaderFooter FirstPageHeader => m_firstPageHeader;

	public HeaderFooter FirstPageFooter => m_firstPageFooter;

	public bool IsEmpty
	{
		get
		{
			if (m_evenHeader.ChildEntities.Count == 0 && m_evenFooter.ChildEntities.Count == 0 && m_oddFooter.ChildEntities.Count == 0 && m_oddHeader.ChildEntities.Count == 0 && m_firstPageFooter.ChildEntities.Count == 0)
			{
				return m_firstPageHeader.ChildEntities.Count == 0;
			}
			return false;
		}
	}

	public HeaderFooter this[int index]
	{
		get
		{
			if (index < 0 || index > 5)
			{
				throw new ArgumentOutOfRangeException("index", "index can't be less 0 or greater 5");
			}
			return this[(HeaderFooterType)index];
		}
	}

	public HeaderFooter this[HeaderFooterType hfType]
	{
		get
		{
			return hfType switch
			{
				HeaderFooterType.EvenHeader => EvenHeader, 
				HeaderFooterType.OddHeader => OddHeader, 
				HeaderFooterType.EvenFooter => EvenFooter, 
				HeaderFooterType.OddFooter => OddFooter, 
				HeaderFooterType.FirstPageHeader => FirstPageHeader, 
				HeaderFooterType.FirstPageFooter => FirstPageFooter, 
				_ => throw new ArgumentException("Invalid header/footer type", "hfType"), 
			};
		}
		internal set
		{
			switch (hfType)
			{
			case HeaderFooterType.EvenHeader:
				m_evenHeader = value;
				break;
			case HeaderFooterType.OddHeader:
				m_oddHeader = value;
				break;
			case HeaderFooterType.EvenFooter:
				m_evenFooter = value;
				break;
			case HeaderFooterType.OddFooter:
				m_oddFooter = value;
				break;
			case HeaderFooterType.FirstPageHeader:
				m_firstPageHeader = value;
				break;
			case HeaderFooterType.FirstPageFooter:
				m_firstPageFooter = value;
				break;
			default:
				throw new ArgumentException("Invalid header/footer type", "hfType");
			}
		}
	}

	public bool LinkToPrevious
	{
		get
		{
			return GetLinkToPreviousValue();
		}
		set
		{
			if (LinkToPrevious != value)
			{
				UpdateLinkToPrevious(value);
			}
		}
	}

	internal WHeadersFooters(WSection sec)
		: base(sec.Document, sec)
	{
		m_evenHeader = new HeaderFooter(sec, HeaderFooterType.EvenHeader);
		m_oddHeader = new HeaderFooter(sec, HeaderFooterType.OddHeader);
		m_evenFooter = new HeaderFooter(sec, HeaderFooterType.EvenFooter);
		m_oddFooter = new HeaderFooter(sec, HeaderFooterType.OddFooter);
		m_firstPageFooter = new HeaderFooter(sec, HeaderFooterType.FirstPageFooter);
		m_firstPageHeader = new HeaderFooter(sec, HeaderFooterType.FirstPageHeader);
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("even-header", EvenHeader);
		base.XDLSHolder.AddElement("odd-header", OddHeader);
		base.XDLSHolder.AddElement("even-footer", EvenFooter);
		base.XDLSHolder.AddElement("odd-footer", OddFooter);
		base.XDLSHolder.AddElement("first-page-header", FirstPageHeader);
		base.XDLSHolder.AddElement("first-page-footer", FirstPageFooter);
	}

	internal WHeadersFooters Clone()
	{
		return (WHeadersFooters)CloneImpl();
	}

	protected override object CloneImpl()
	{
		WHeadersFooters obj = (WHeadersFooters)base.CloneImpl();
		obj.m_evenHeader = (HeaderFooter)m_evenHeader.Clone();
		obj.m_oddHeader = (HeaderFooter)m_oddHeader.Clone();
		obj.m_evenFooter = (HeaderFooter)m_evenFooter.Clone();
		obj.m_oddFooter = (HeaderFooter)m_oddFooter.Clone();
		obj.m_firstPageFooter = (HeaderFooter)m_firstPageFooter.Clone();
		obj.m_firstPageHeader = (HeaderFooter)m_firstPageHeader.Clone();
		return obj;
	}

	internal new void Close()
	{
		for (int i = 0; i < 6; i++)
		{
			this[i].Close();
		}
		base.Close();
	}

	internal void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		for (int i = 0; i < 6; i++)
		{
			this[i].InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				break;
			}
		}
	}

	public IEnumerator GetEnumerator()
	{
		return new HFEnumerator(this);
	}

	private bool GetLinkToPreviousValue()
	{
		if (!(base.OwnerBase is WSection wSection))
		{
			return false;
		}
		if (wSection.Index > 0)
		{
			bool result = true;
			for (int i = 0; i < 6; i++)
			{
				if (this[i].Items.Count > 0)
				{
					result = false;
					break;
				}
			}
			return result;
		}
		return false;
	}

	private void UpdateLinkToPrevious(bool linkToPrevious)
	{
		if (base.OwnerBase is WSection wSection && wSection.GetIndexInOwnerCollection() > 0)
		{
			for (int i = 0; i < 6; i++)
			{
				this[i].LinkToPrevious = linkToPrevious;
			}
		}
	}
}
