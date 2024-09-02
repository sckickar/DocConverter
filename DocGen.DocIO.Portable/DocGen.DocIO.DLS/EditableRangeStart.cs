using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

internal class EditableRangeStart : ParagraphItem
{
	private string m_id = string.Empty;

	private short m_colFirst = -1;

	private short m_colLast = -1;

	private string m_ed = string.Empty;

	private string m_edGrp = string.Empty;

	private byte m_bFlags;

	public override EntityType EntityType => EntityType.EditableRangeStart;

	internal string Id => m_id;

	internal short ColumnFirst
	{
		get
		{
			return m_colFirst;
		}
		set
		{
			m_colFirst = value;
		}
	}

	internal short ColumnLast
	{
		get
		{
			return m_colLast;
		}
		set
		{
			m_colLast = value;
		}
	}

	internal string Ed
	{
		get
		{
			return m_ed;
		}
		set
		{
			m_ed = value;
		}
	}

	internal string EdGrp
	{
		get
		{
			return m_edGrp;
		}
		set
		{
			m_edGrp = value;
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

	internal EditableRangeStart(WordDocument doc)
		: this(doc, string.Empty)
	{
	}

	internal EditableRangeStart(IWordDocument doc, string id)
		: base((WordDocument)doc)
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
			base.Document.EditableRanges.AttachEditableRangeStart(this);
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
			EditableRangeCollection editableRanges = base.Document.EditableRanges;
			EditableRange editableRange = editableRanges.FindById(Id);
			if (editableRange != null)
			{
				editableRange.SetStart(null);
				editableRanges.Remove(editableRange);
			}
		}
	}

	internal override void AttachToDocument()
	{
		if (IsDetached)
		{
			base.Document.EditableRanges.AttachEditableRangeStart(this);
			IsDetached = false;
		}
	}

	protected override object CloneImpl()
	{
		EditableRangeStart obj = (EditableRangeStart)base.CloneImpl();
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
