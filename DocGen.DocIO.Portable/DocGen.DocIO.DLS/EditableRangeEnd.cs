using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

internal class EditableRangeEnd : ParagraphItem
{
	private string m_id = string.Empty;

	private byte m_bFlags;

	public override EntityType EntityType => EntityType.EditableRangeEnd;

	internal string Id => m_id;

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

	internal EditableRangeEnd(WordDocument doc)
		: this(doc, string.Empty)
	{
	}

	internal EditableRangeEnd(IWordDocument document, string id)
		: base((WordDocument)document)
	{
		SetId(id);
	}

	internal void SetId(string id)
	{
		m_id = id;
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		if (!base.DeepDetached)
		{
			base.Document.EditableRanges.AttacheEditableRangeEnd(this);
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
			base.Document.EditableRanges.FindById(Id)?.SetEnd(null);
		}
	}

	internal override void AttachToDocument()
	{
		if (IsDetached)
		{
			base.Document.EditableRanges.AttacheEditableRangeEnd(this);
			IsDetached = false;
		}
	}

	protected override object CloneImpl()
	{
		EditableRangeEnd obj = (EditableRangeEnd)base.CloneImpl();
		obj.IsDetached = true;
		return obj;
	}

	internal override void Close()
	{
		if (!string.IsNullOrEmpty(m_id))
		{
			m_id = string.Empty;
		}
		base.Close();
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new ParagraphLayoutInfo(ChildrenLayoutDirection.Horizontal);
		m_layoutInfo.IsSkip = true;
	}
}
