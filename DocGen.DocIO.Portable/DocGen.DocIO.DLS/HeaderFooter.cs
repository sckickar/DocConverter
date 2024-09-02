namespace DocGen.DocIO.DLS;

public class HeaderFooter : WTextBody
{
	private HeaderFooterType m_type;

	private byte m_bFlags;

	private Watermark m_watermark;

	public override EntityType EntityType => EntityType.HeaderFooter;

	internal HeaderFooterType Type
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

	internal bool WriteWatermark
	{
		get
		{
			if (LinkToPrevious)
			{
				return false;
			}
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal Watermark Watermark
	{
		get
		{
			if (m_watermark == null)
			{
				m_watermark = new Watermark(WatermarkType.NoWatermark);
			}
			return m_watermark;
		}
		set
		{
			m_watermark = value;
			if (m_watermark != null)
			{
				WriteWatermark = true;
				m_watermark.SetOwner(this);
				if (m_watermark is PictureWatermark)
				{
					(m_watermark as PictureWatermark).UpdateImage();
				}
				else if (m_watermark is TextWatermark)
				{
					(m_watermark as TextWatermark).SetDefaultSize();
				}
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

	internal Watermark InsertWatermark(WatermarkType type)
	{
		switch (type)
		{
		case WatermarkType.PictureWatermark:
			m_watermark = new PictureWatermark(base.Document);
			break;
		case WatermarkType.TextWatermark:
			m_watermark = new TextWatermark(base.Document);
			break;
		default:
			m_watermark = new Watermark(type);
			break;
		}
		return m_watermark;
	}

	internal bool CheckWriteWatermark()
	{
		return WriteWatermark;
	}

	internal HeaderFooter(WSection sec, HeaderFooterType type)
		: base(sec)
	{
		m_type = type;
	}

	private bool GetLinkToPreviousValue()
	{
		if (!(base.OwnerBase is WSection wSection))
		{
			return false;
		}
		if (wSection.Index > 0)
		{
			return base.Items.Count == 0;
		}
		return false;
	}

	private void UpdateLinkToPrevious(bool linkToPrevious)
	{
		if (linkToPrevious)
		{
			base.ChildEntities.Clear();
			(base.OwnerBase as WSection).HeadersFooters[m_type] = new HeaderFooter(base.OwnerBase as WSection, m_type);
			return;
		}
		WSection wSection = FindSourceSection();
		if (wSection != null)
		{
			foreach (HeaderFooter headersFooter in wSection.HeadersFooters)
			{
				if (m_type == headersFooter.m_type && CheckShapes(headersFooter))
				{
					headersFooter.m_bodyItems.CloneTo(m_bodyItems);
				}
			}
		}
		if (base.ChildEntities.Count == 0)
		{
			AddParagraph();
		}
	}

	private WSection FindSourceSection()
	{
		WSection wSection;
		for (wSection = (base.OwnerBase as WSection).PreviousSibling as WSection; wSection != null; wSection = wSection.PreviousSibling as WSection)
		{
			if (!wSection.HeadersFooters.LinkToPrevious)
			{
				return wSection;
			}
		}
		return wSection;
	}

	private bool CheckShapes(HeaderFooter hf)
	{
		foreach (WParagraph paragraph in hf.Paragraphs)
		{
			foreach (ParagraphItem item in paragraph.Items)
			{
				if (item is WPicture || item is WTextBox)
				{
					item.IsCloned = true;
				}
				else if (item is ShapeObject)
				{
					return false;
				}
			}
		}
		return true;
	}
}
