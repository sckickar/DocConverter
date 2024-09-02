using System.Text;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class InlineContentControl : ParagraphItem, IInlineContentControl, IParagraphItem, IEntity
{
	private ContentControlProperties m_controlProperties;

	private ParagraphItemCollection m_paragraphItemCollection;

	private ParagraphItem m_mappedItem;

	internal new bool IsMappedItem;

	private byte m_bFlags;

	private string m_comparisonText;

	internal ParagraphItem MappedItem
	{
		get
		{
			return m_mappedItem;
		}
		set
		{
			m_mappedItem = value;
		}
	}

	public ParagraphItemCollection ParagraphItems
	{
		get
		{
			if (m_paragraphItemCollection == null)
			{
				m_paragraphItemCollection = new ParagraphItemCollection(m_doc);
			}
			return m_paragraphItemCollection;
		}
	}

	public ContentControlProperties ContentControlProperties => m_controlProperties;

	public WCharacterFormat BreakCharacterFormat => base.ParaItemCharFormat;

	public override EntityType EntityType => EntityType.InlineContentControl;

	internal override int EndPos
	{
		get
		{
			if (ParagraphItems.Count > 0)
			{
				return ParagraphItems[ParagraphItems.Count - 1].EndPos;
			}
			return base.StartPos;
		}
	}

	internal bool IsFirstNestedParaParsed
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

	internal string ComparisonText
	{
		get
		{
			if (m_comparisonText == null)
			{
				string s = GetAsString().ToString();
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				m_comparisonText = base.Document.Comparison.ConvertBytesAsHash(bytes);
			}
			return m_comparisonText;
		}
	}

	internal InlineContentControl(WordDocument doc)
		: base(doc)
	{
		m_controlProperties = new ContentControlProperties(doc, this);
		m_paragraphItemCollection = new ParagraphItemCollection(doc);
		m_paragraphItemCollection.SetOwner(this);
		m_charFormat = new WCharacterFormat(doc, this);
	}

	public InlineContentControl(WordDocument doc, ContentControlType controlType)
		: base(doc)
	{
		m_controlProperties = new ContentControlProperties(doc, this);
		m_paragraphItemCollection = new ParagraphItemCollection(doc);
		m_paragraphItemCollection.SetOwner(this);
		m_charFormat = new WCharacterFormat(doc, this);
		m_controlProperties.Type = controlType;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Vertical);
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_paragraphItemCollection != null)
		{
			foreach (Entity item in m_paragraphItemCollection)
			{
				item.InitLayoutInfo(entity, ref isLastTOCEntry);
				if (isLastTOCEntry)
				{
					return;
				}
			}
		}
		_ = isLastTOCEntry;
	}

	internal bool IsHidden()
	{
		for (int i = 0; i < ParagraphItems.Count; i++)
		{
			if (!ParagraphItems[i].ParaItemCharFormat.Hidden)
			{
				return false;
			}
		}
		return ParagraphItems.Count > 0;
	}

	internal bool IsDeletion()
	{
		for (int i = 0; i < ParagraphItems.Count; i++)
		{
			if (!ParagraphItems[i].ParaItemCharFormat.IsDeleteRevision)
			{
				return false;
			}
		}
		return ParagraphItems.Count > 0;
	}

	internal bool IsAllItemSameRevision()
	{
		ParagraphItemCollection paragraphItems = ParagraphItems;
		string authorName = paragraphItems[0].AuthorName;
		bool isInsertRevision = paragraphItems[0].IsInsertRevision;
		bool isDeleteRevision = paragraphItems[0].IsDeleteRevision;
		for (int i = 1; i < paragraphItems.Count; i++)
		{
			if (paragraphItems[i].AuthorName != authorName || paragraphItems[i].IsInsertRevision != isInsertRevision || paragraphItems[i].IsDeleteRevision != isDeleteRevision)
			{
				return false;
			}
		}
		return true;
	}

	internal new InlineContentControl Clone()
	{
		return (InlineContentControl)CloneImpl();
	}

	internal override void AddSelf()
	{
		foreach (ParagraphItem item in m_paragraphItemCollection)
		{
			item.AddSelf();
		}
	}

	protected override object CloneImpl()
	{
		InlineContentControl inlineContentControl = (InlineContentControl)base.CloneImpl();
		inlineContentControl.m_paragraphItemCollection = new ParagraphItemCollection(inlineContentControl);
		m_paragraphItemCollection.CloneItemsTo(inlineContentControl.m_paragraphItemCollection);
		if (m_mappedItem != null)
		{
			inlineContentControl.m_mappedItem = m_mappedItem.Clone() as ParagraphItem;
		}
		inlineContentControl.m_controlProperties = m_controlProperties.Clone();
		inlineContentControl.m_controlProperties.SetOwnerContentControl(inlineContentControl);
		inlineContentControl.m_charFormat = new WCharacterFormat(base.Document, inlineContentControl);
		inlineContentControl.m_charFormat.ImportContainer(BreakCharacterFormat);
		inlineContentControl.m_charFormat.CopyProperties(BreakCharacterFormat);
		return inlineContentControl;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		int i = 0;
		for (int count = ParagraphItems.Count; i < count; i++)
		{
			ParagraphItems[i].CloneRelationsTo(doc, nextOwner);
		}
		ContentControlProperties.CloneRelationsTo(doc, nextOwner);
		ContentControlProperties.SetOwner(this);
	}

	internal void ApplyBaseFormat()
	{
		IWParagraphStyle style = base.OwnerParagraph.GetStyle();
		if (style != null)
		{
			base.ParaItemCharFormat.ApplyBase(style.CharacterFormat);
			for (int i = 0; i < ParagraphItems.Count; i++)
			{
				ParagraphItems[i].ParaItemCharFormat.ApplyBase(style.CharacterFormat);
			}
			if (MappedItem != null)
			{
				MappedItem.ParaItemCharFormat.ApplyBase(style.CharacterFormat);
			}
		}
	}

	internal void ApplyBaseFormatForCharacterStyle(IWCharacterStyle style)
	{
		for (int i = 0; i < ParagraphItems.Count; i++)
		{
			ParagraphItems[i].ParaItemCharFormat.CharStyleName = style.Name;
		}
	}

	internal override void Close()
	{
		if (m_paragraphItemCollection != null)
		{
			m_paragraphItemCollection.Close();
			m_paragraphItemCollection = null;
		}
		if (m_controlProperties != null)
		{
			m_controlProperties.Close();
			m_controlProperties = null;
		}
		if (m_mappedItem != null)
		{
			m_mappedItem.Close();
			m_mappedItem = null;
		}
		base.Close();
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0012');
		stringBuilder.Append(ContentControlProperties.GetAsString());
		stringBuilder.Append(base.OwnerParagraph.GetAsString(ParagraphItems));
		stringBuilder.Append('\u0012');
		return stringBuilder;
	}

	internal void CopyItemsTo(ParagraphItemCollection paraItems)
	{
		for (int i = 0; i < ParagraphItems.Count; i++)
		{
			XmlParagraphItem xmlParagraphItem = ParagraphItems[i] as XmlParagraphItem;
			if (ParagraphItems[i] is InlineContentControl)
			{
				((InlineContentControl)ParagraphItems[i]).CopyItemsTo(paraItems);
			}
			else if (xmlParagraphItem != null && xmlParagraphItem.MathParaItemsCollection != null)
			{
				for (int j = 0; j < xmlParagraphItem.MathParaItemsCollection.Count; j++)
				{
					if (xmlParagraphItem.MathParaItemsCollection[j] is InlineContentControl)
					{
						((InlineContentControl)xmlParagraphItem.MathParaItemsCollection[j]).CopyItemsTo(paraItems);
					}
					else
					{
						paraItems.InnerList.Add(xmlParagraphItem.MathParaItemsCollection[j]);
					}
				}
			}
			else
			{
				paraItems.InnerList.Add(ParagraphItems[i]);
			}
		}
	}
}
