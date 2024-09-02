using System;
using System.IO;
using System.Xml;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

internal class UnknownShapeSerializator : ShapeSerializator
{
	private Stream m_shapeTypeStream;

	public UnknownShapeSerializator(Stream shapeTypeStream)
	{
		if (shapeTypeStream == null || shapeTypeStream.Length == 0L)
		{
			throw new ArgumentOutOfRangeException("shapeTypeStream");
		}
		m_shapeTypeStream = shapeTypeStream;
	}

	public override void Serialize(XmlWriter writer, ShapeImpl shape, WorksheetDataHolder holder, RelationCollection vmlRelations)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (holder == null)
		{
			throw new ArgumentNullException("holder");
		}
		Stream xmlDataStream = shape.XmlDataStream;
		xmlDataStream.Position = 0L;
		XmlReader reader = UtilityMethods.CreateReader(xmlDataStream);
		writer.WriteNode(reader, defattr: false);
		writer.Flush();
	}

	public override void SerializeShapeType(XmlWriter writer, Type shapeType)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		m_shapeTypeStream.Position = 0L;
		XmlReader reader = UtilityMethods.CreateReader(m_shapeTypeStream);
		writer.WriteNode(reader, defattr: false);
		writer.Flush();
	}
}
