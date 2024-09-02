using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class BookmarkEnd : ParagraphItem, ILeafWidget, IWidget
{
	private string m_strName = string.Empty;

	private byte m_bFlags;

	private string m_displacedByCustomXml;

	public override EntityType EntityType => EntityType.BookmarkEnd;

	public string Name => m_strName;

	internal bool IsCellGroupBkmk
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

	internal bool IsDetached
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

	internal bool IsAfterParagraphMark
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

	internal bool IsAfterCellMark
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

	internal bool IsAfterRowMark
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

	internal bool IsAfterTableMark
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

	internal bool IsToAddInNextPara
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

	internal string DisplacedByCustomXml
	{
		get
		{
			return m_displacedByCustomXml;
		}
		set
		{
			m_displacedByCustomXml = value;
		}
	}

	internal BookmarkEnd(WordDocument doc)
		: this(doc, string.Empty)
	{
	}

	public BookmarkEnd(IWordDocument document, string name)
		: base((WordDocument)document)
	{
		SetName(name);
	}

	internal override void Close()
	{
		if (!string.IsNullOrEmpty(m_strName))
		{
			m_strName = string.Empty;
		}
		base.Close();
	}

	internal void SetName(string name)
	{
		m_strName = name.Replace('-', '_');
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		if (!base.DeepDetached)
		{
			base.Document.Bookmarks.AttachBookmarkEnd(this);
			IsDetached = false;
		}
		else
		{
			IsDetached = true;
		}
	}

	internal override void Detach()
	{
		base.Detach();
		if (!base.DeepDetached)
		{
			base.Document.Bookmarks.FindByName(Name)?.SetEnd(null);
		}
	}

	internal override void AttachToDocument()
	{
		if (IsDetached)
		{
			base.Document.Bookmarks.AttachBookmarkEnd(this);
			IsDetached = false;
		}
	}

	protected override object CloneImpl()
	{
		BookmarkEnd obj = (BookmarkEnd)base.CloneImpl();
		obj.IsDetached = true;
		return obj;
	}

	internal bool HasRenderableItemBefore()
	{
		WParagraph ownerParagraph = base.OwnerParagraph;
		int num = 0;
		while (ownerParagraph != null && Index != -1 && num < Index)
		{
			Entity entity = ownerParagraph.ChildEntities[num];
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
			{
				return true;
			}
			num++;
		}
		return false;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.BookmarkEnd);
		writer.WriteValue("BookmarkName", Name);
		if (IsCellGroupBkmk)
		{
			writer.WriteValue("IsCellGroupBkmk", IsCellGroupBkmk);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_strName = reader.ReadString("BookmarkName");
		base.Document.Bookmarks.AttachBookmarkEnd(this);
		if (reader.HasAttribute("IsCellGroupBkmk"))
		{
			IsCellGroupBkmk = reader.ReadBoolean("IsCellGroupBkmk");
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		m_layoutInfo.IsClipped = ((IWidget)GetOwnerParagraphValue()).LayoutInfo.IsClipped;
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		SizeF result = default(SizeF);
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue.IsNeedToMeasureBookMarkSize)
		{
			result.Height = ((IWidget)ownerParagraphValue).LayoutInfo.Size.Height;
		}
		return result;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}
}
