using DocGen.DocIO.DLS.XML;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class Watermark : ParagraphItem
{
	private WatermarkType m_type;

	private int m_orderIndex = int.MaxValue;

	private int m_spid = -1;

	public override EntityType EntityType => EntityType.Undefined;

	public WatermarkType Type => m_type;

	internal int OrderIndex
	{
		get
		{
			if (m_orderIndex == int.MaxValue && base.Document != null && !base.Document.IsOpening && base.Document.Escher != null)
			{
				int shapeOrderIndex = base.Document.Escher.GetShapeOrderIndex(ShapeId);
				if (shapeOrderIndex != -1)
				{
					m_orderIndex = shapeOrderIndex;
				}
			}
			return m_orderIndex;
		}
		set
		{
			m_orderIndex = value;
		}
	}

	internal int ShapeId
	{
		get
		{
			return m_spid;
		}
		set
		{
			m_spid = value;
		}
	}

	internal Watermark(WatermarkType type)
		: base(null)
	{
		m_type = type;
	}

	internal Watermark(WordDocument doc, WatermarkType type)
		: base(doc)
	{
		m_type = type;
	}

	internal override void RemoveSelf()
	{
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("WatermarkType", m_type);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("WatermarkType"))
		{
			m_type = (WatermarkType)(object)reader.ReadEnum("WatermarkType", typeof(WatermarkType));
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
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
