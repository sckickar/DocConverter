using System;
using System.IO;
using System.Text;
using System.Xml;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;
using DocGen.OfficeChart.Implementation.XmlSerialization.Shapes;

namespace DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

internal class UnknownVmlShapeParser : ShapeParser
{
	public override ShapeImpl ParseShapeType(XmlReader reader, ShapeCollectionBase shapes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		if (!reader.MoveToAttribute("spt", "urn:schemas-microsoft-com:office:office"))
		{
			throw new XmlException();
		}
		int shapeInstance = int.Parse(reader.Value);
		reader.MoveToElement();
		AddNewSerializator(shapeInstance, reader, shapes);
		return new ShapeImpl(shapes.Application, shapes);
	}

	public override bool ParseShape(XmlReader reader, ShapeImpl defaultShape, RelationCollection relations, string parentItemPath)
	{
		ShapeImpl shapeImpl = (ShapeImpl)defaultShape.Clone(defaultShape.Parent);
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, defattr: false);
		xmlWriter.Flush();
		shapeImpl.XmlDataStream = memoryStream;
		memoryStream.Position = 0L;
		reader = UtilityMethods.CreateReader(memoryStream);
		reader.Read();
		while (reader.NodeType != 0)
		{
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "imagedata")
			{
				if (reader.MoveToAttribute("relid", "urn:schemas-microsoft-com:office:office"))
				{
					string value = reader.Value;
					shapeImpl.ImageRelation = (Relation)relations[value].Clone();
					shapeImpl.ImageRelationId = value;
				}
				break;
			}
			reader.Read();
		}
		memoryStream.Position = 0L;
		return true;
	}

	private void AddNewSerializator(int shapeInstance, XmlReader reader, ShapeCollectionBase shapes)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shapes == null)
		{
			throw new ArgumentNullException("shapes");
		}
		MemoryStream memoryStream = new MemoryStream();
		XmlWriter xmlWriter = UtilityMethods.CreateWriter(memoryStream, Encoding.UTF8);
		xmlWriter.WriteNode(reader, defattr: false);
		xmlWriter.Flush();
		WorksheetBaseImpl worksheetBase = shapes.WorksheetBase;
		UnknownShapeSerializator value = new UnknownShapeSerializator(memoryStream);
		worksheetBase.DataHolder.ParentHolder.Serializator.VmlSerializators[shapeInstance] = value;
		worksheetBase.UnknownVmlShapes = true;
	}
}
