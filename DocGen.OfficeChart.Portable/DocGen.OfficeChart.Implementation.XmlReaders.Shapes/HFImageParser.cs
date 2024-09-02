using System;
using System.Collections.Generic;
using System.Xml;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Implementation.XmlSerialization;

namespace DocGen.OfficeChart.Implementation.XmlReaders.Shapes;

internal class HFImageParser : ShapeParser
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
		reader.Skip();
		return new BitmapShapeImpl(shapes.Application, shapes)
		{
			VmlShape = true
		};
	}

	public override bool ParseShape(XmlReader reader, ShapeImpl defaultShape, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (defaultShape == null)
		{
			throw new ArgumentNullException("defaultShape");
		}
		if (reader.LocalName != "shape")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		string text = null;
		int num = -1;
		if (reader.MoveToAttribute("spid", "urn:schemas-microsoft-com:office:office"))
		{
			text = reader.Value;
			num = ParseShapeId(text);
		}
		if (reader.MoveToAttribute("id"))
		{
			text = reader.Value;
			if (num == -1)
			{
				num = ParseShapeId(text);
			}
		}
		BitmapShapeImpl bitmapShapeImpl = (BitmapShapeImpl)defaultShape.Clone(defaultShape.Parent, null, null, addToCollections: false);
		if (reader.MoveToAttribute("stroked"))
		{
			bitmapShapeImpl.HasBorder = true;
		}
		else
		{
			bitmapShapeImpl.HasBorder = false;
		}
		if (reader.MoveToAttribute("style"))
		{
			ParseStyle(reader, bitmapShapeImpl);
		}
		reader.Read();
		if (num != -1)
		{
			bitmapShapeImpl.ShapeId = num;
			bitmapShapeImpl.Name = text;
		}
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string localName = reader.LocalName;
				if (!(localName == "imagedata"))
				{
					if (localName == "ClientData")
					{
						ParseClientData(reader, bitmapShapeImpl);
					}
					else
					{
						reader.Skip();
					}
				}
				else
				{
					ParseImageData(reader, text, bitmapShapeImpl, relations, parentItemPath);
				}
			}
			else
			{
				reader.Read();
			}
		}
		reader.Read();
		return true;
	}

	private void ParseStyle(XmlReader reader, BitmapShapeImpl result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (result == null)
		{
			throw new ArgumentNullException("textBox");
		}
		result.PreserveStyleString = reader.Value;
	}

	protected virtual void ParseStyle(BitmapShapeImpl result, Dictionary<string, string> styleProperties)
	{
		if (styleProperties.TryGetValue("height", out var value))
		{
			result.Height = (int)result.AppImplementation.ConvertUnits(Convert.ToDouble(value.Substring(0, value.Length - 2)), MeasureUnits.Point, MeasureUnits.Pixel);
		}
		if (styleProperties.TryGetValue("width", out var value2))
		{
			result.Width = (int)result.AppImplementation.ConvertUnits(Convert.ToDouble(value2.Substring(0, value2.Length - 2)), MeasureUnits.Point, MeasureUnits.Pixel);
		}
	}

	private int ParseShapeId(string shapeId)
	{
		int result = shapeId.IndexOf("_s");
		int result2 = -1;
		if (result >= 0 && int.TryParse(shapeId.Substring(result + 2), out result))
		{
			result2 = result;
		}
		return result2;
	}

	private void ParseClientData(XmlReader reader, BitmapShapeImpl shape)
	{
		if (reader.LocalName != "ClientData")
		{
			throw new XmlException("Unexpected xml token");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		reader.Read();
		shape.IsMoveWithCell = true;
		shape.IsSizeWithCell = true;
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
				case "MoveWithCells":
					shape.IsMoveWithCell = VmlTextBoxBaseParser.ParseBoolOrEmpty(reader, defaultValue: true);
					break;
				case "SizeWithCells":
					shape.IsSizeWithCell = VmlTextBoxBaseParser.ParseBoolOrEmpty(reader, defaultValue: true);
					break;
				case "Anchor":
					ParseAnchor(reader, shape);
					break;
				case "DDE":
					shape.IsDDE = true;
					reader.Read();
					break;
				case "Camera":
					shape.IsCamera = true;
					reader.Read();
					break;
				default:
					reader.Skip();
					break;
				}
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Read();
	}

	private void ParseImageData(XmlReader reader, string shapeName, BitmapShapeImpl shape, RelationCollection relations, string parentItemPath)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (shape == null)
		{
			throw new ArgumentNullException("shape");
		}
		if (shapeName == null || shapeName.Length == 0)
		{
			throw new ArgumentOutOfRangeException("shapeName");
		}
		if (parentItemPath == null || parentItemPath.Length == 0)
		{
			throw new ArgumentOutOfRangeException("parentItemPath");
		}
		if (relations == null)
		{
			throw new ArgumentNullException("relations");
		}
		if (reader.LocalName != "imagedata")
		{
			throw new XmlException("Unexpected xml tag.");
		}
		if (reader.MoveToAttribute("relid", "urn:schemas-microsoft-com:office:office"))
		{
			string value = reader.Value;
			Relation relation = relations[value];
			if (relation == null)
			{
				throw new XmlException("Cannot find required relation.");
			}
			WorksheetBaseImpl worksheet = shape.Worksheet;
			string strFullPath = FileDataHolder.CombinePath(parentItemPath, relation.Target);
			Image image = worksheet.DataHolder.ParentHolder.GetImage(strFullPath);
			if (shape.ParentShapes is HeaderFooterShapeCollection)
			{
				worksheet.HeaderFooterShapes.SetPicture(shapeName, image, -1, bIncludeOptions: false, shape.PreserveStyleString);
			}
			else
			{
				shape.Picture = image;
				worksheet.InnerShapes.Add(shape);
			}
			reader.Skip();
			return;
		}
		throw new XmlException("Wrong xml format.");
	}
}
