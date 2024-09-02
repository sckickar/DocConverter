using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class ShapeObject : ParagraphItem, ILeafWidget, IWidget
{
	private FileShapeAddress m_fspa;

	private WTextBoxCollection m_textBoxColl;

	private byte m_bFlags;

	public override EntityType EntityType => EntityType.Shape;

	internal FileShapeAddress FSPA
	{
		get
		{
			return m_fspa;
		}
		set
		{
			m_fspa = value;
		}
	}

	internal WTextBoxCollection AutoShapeTextCollection => m_textBoxColl;

	internal bool IsHeaderAutoShape
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

	internal bool AllowInCell
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

	internal WCharacterFormat CharacterFormat => m_charFormat;

	internal ShapeObject(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_fspa = new FileShapeAddress();
		m_textBoxColl = new WTextBoxCollection(doc);
		m_charFormat = new WCharacterFormat(doc, this);
	}

	internal override void AddSelf()
	{
		foreach (WTextBox item in AutoShapeTextCollection)
		{
			item.AddSelf();
		}
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		foreach (WTextBox item in AutoShapeTextCollection)
		{
			foreach (Entity childEntity in item.ChildEntities)
			{
				childEntity.CloneRelationsTo(doc, nextOwner);
				childEntity.SetOwner(doc);
			}
		}
		base.Document.CloneShapeEscher(doc, this);
		base.IsCloned = false;
	}

	protected override object CloneImpl()
	{
		ShapeObject shapeObject = (ShapeObject)base.CloneImpl();
		shapeObject.m_textBoxColl = new WTextBoxCollection(base.Document);
		m_textBoxColl.CloneTo(shapeObject.m_textBoxColl);
		if (FSPA != null)
		{
			shapeObject.m_fspa = FSPA.Clone();
		}
		shapeObject.IsCloned = true;
		return shapeObject;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		if (!(this is InlineShapeObject))
		{
			m_layoutInfo.IsSkipBottomAlign = true;
		}
		m_layoutInfo.IsClipped = true;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_textBoxColl == null || m_textBoxColl.Count <= 0)
		{
			return;
		}
		foreach (WTextBox item in m_textBoxColl)
		{
			item.InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				break;
			}
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.ShapeObject);
		writer.WriteValue("ShapeID", m_fspa.Spid);
		writer.WriteValue("IsBelowText", m_fspa.IsBelowText);
		writer.WriteValue("HorizontalOrigin", m_fspa.RelHrzPos);
		writer.WriteValue("VerticalOrigin", m_fspa.RelVrtPos);
		writer.WriteValue("WrappingStyle", m_fspa.TextWrappingStyle);
		writer.WriteValue("WrappingType", m_fspa.TextWrappingType);
		writer.WriteValue("HorizontalPosition", m_fspa.XaLeft);
		writer.WriteValue("VerticalPosition", m_fspa.YaTop);
		writer.WriteValue("TxbxCount", m_fspa.TxbxCount);
		writer.WriteValue("Height", m_fspa.Height);
		writer.WriteValue("Width", m_fspa.Width);
		writer.WriteValue("IsHeader", IsHeaderAutoShape);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("ShapeID"))
		{
			m_fspa.Spid = reader.ReadInt("ShapeID");
		}
		if (reader.HasAttribute("IsBelowText"))
		{
			m_fspa.IsBelowText = reader.ReadBoolean("IsBelowText");
		}
		if (reader.HasAttribute("HorizontalOrigin"))
		{
			m_fspa.RelHrzPos = (HorizontalOrigin)(object)reader.ReadEnum("HorizontalOrigin", typeof(HorizontalOrigin));
		}
		if (reader.HasAttribute("VerticalOrigin"))
		{
			m_fspa.RelVrtPos = (VerticalOrigin)(object)reader.ReadEnum("VerticalOrigin", typeof(VerticalOrigin));
		}
		if (reader.HasAttribute("WrappingStyle"))
		{
			m_fspa.TextWrappingStyle = (TextWrappingStyle)(object)reader.ReadEnum("WrappingStyle", typeof(TextWrappingStyle));
		}
		if (reader.HasAttribute("WrappingType"))
		{
			m_fspa.TextWrappingType = (TextWrappingType)(object)reader.ReadEnum("WrappingType", typeof(TextWrappingType));
		}
		if (reader.HasAttribute("HorizontalPosition"))
		{
			m_fspa.XaLeft = reader.ReadInt("HorizontalPosition");
		}
		if (reader.HasAttribute("VerticalPosition"))
		{
			m_fspa.YaTop = reader.ReadInt("VerticalPosition");
		}
		if (reader.HasAttribute("TxbxCount"))
		{
			m_fspa.TxbxCount = reader.ReadInt("TxbxCount");
		}
		if (reader.HasAttribute("Height"))
		{
			m_fspa.Height = reader.ReadInt("Height");
		}
		if (reader.HasAttribute("Width"))
		{
			m_fspa.Width = reader.ReadInt("Width");
		}
		if (reader.HasAttribute("IsHeader"))
		{
			IsHeaderAutoShape = reader.ReadBoolean("IsHeader");
		}
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("textboxes", m_textBoxColl);
		base.XDLSHolder.AddElement("character-format", m_charFormat);
	}

	internal override void Close()
	{
		base.Close();
		m_fspa = null;
		if (m_textBoxColl != null)
		{
			m_textBoxColl.Close();
			m_textBoxColl = null;
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		float height = 0f;
		if (base.OwnerParagraph.ChildEntities.Count == 1 && !((IWidget)this).LayoutInfo.IsSkip)
		{
			m_layoutInfo.IsClipped = false;
			height = base.OwnerParagraph.m_layoutInfo.Size.Height;
		}
		return new SizeF(0f, height);
	}
}
