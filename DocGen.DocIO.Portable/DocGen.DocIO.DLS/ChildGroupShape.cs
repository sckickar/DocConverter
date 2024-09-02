using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class ChildGroupShape : ChildShape, IEntity, ILeafWidget, IWidget
{
	private ChildShapeCollection m_childShapes;

	private float m_extentXValue;

	private float m_extentYValue;

	private float m_offsetXValue;

	private float m_offsetYValue;

	private long shapeId;

	private bool m_hasPictureItem;

	internal float OffsetXValue
	{
		get
		{
			return m_offsetXValue;
		}
		set
		{
			m_offsetXValue = value;
		}
	}

	internal float OffsetYValue
	{
		get
		{
			return m_offsetYValue;
		}
		set
		{
			m_offsetYValue = value;
		}
	}

	internal float ExtentXValue
	{
		get
		{
			return m_extentXValue;
		}
		set
		{
			m_extentXValue = value;
		}
	}

	internal float ExtentYValue
	{
		get
		{
			return m_extentYValue;
		}
		set
		{
			m_extentYValue = value;
		}
	}

	internal bool HasPictureItem
	{
		get
		{
			return m_hasPictureItem;
		}
		set
		{
			m_hasPictureItem = value;
		}
	}

	internal ChildShapeCollection ChildShapes
	{
		get
		{
			if (m_childShapes == null)
			{
				m_childShapes = new ChildShapeCollection(base.Document);
			}
			return m_childShapes;
		}
		set
		{
			m_childShapes = value;
		}
	}

	public override EntityType EntityType => EntityType.ChildGroupShape;

	internal long GenerateShapeID()
	{
		return shapeId++;
	}

	internal void Add(ParagraphItem childShape)
	{
		if (!base.Document.IsOpening && childShape.EntityType != EntityType.ChildShape && childShape.GetTextWrappingStyle() == TextWrappingStyle.Inline)
		{
			throw new Exception("Inline objects cannot be grouped");
		}
		GroupShape groupShape = new GroupShape(base.Document);
		switch (childShape.EntityType)
		{
		case EntityType.AutoShape:
		{
			ChildShape childShape2 = groupShape.ConvertShapeToChildShape(childShape as Shape);
			childShape2.SetOwner(this);
			ChildShapes.Add(childShape2);
			break;
		}
		case EntityType.TextBox:
		{
			ChildShape childShape2 = groupShape.ConvertTextboxToChildShape(childShape as WTextBox);
			childShape2.SetOwner(this);
			ChildShapes.Add(childShape2);
			break;
		}
		case EntityType.Chart:
		{
			ChildShape childShape2 = groupShape.ConvertChartToChildShape(childShape as WChart);
			childShape2.SetOwner(this);
			ChildShapes.Add(childShape2);
			break;
		}
		case EntityType.Picture:
		{
			ChildShape childShape2 = groupShape.ConvertPictureToChildShape(childShape as WPicture);
			childShape2.SetOwner(this);
			ChildShapes.Add(childShape2);
			break;
		}
		case EntityType.GroupShape:
		{
			ChildShape childShape2 = groupShape.ConvertGroupShapeToChildGroupShape(childShape as GroupShape);
			childShape2.SetOwner(this);
			ChildShapes.Add(childShape2);
			break;
		}
		case EntityType.ChildShape:
			childShape.SetOwner(this);
			ChildShapes.Add(childShape as ChildShape);
			break;
		case EntityType.ChildGroupShape:
			childShape.SetOwner(this);
			ChildShapes.Add(childShape as ChildGroupShape);
			break;
		default:
			throw new InvalidOperationException($"Cannot add object of type {childShape.EntityType} to Group Shape");
		}
	}

	internal ChildGroupShape(IWordDocument doc)
		: base((WordDocument)doc)
	{
	}

	protected override object CloneImpl()
	{
		ChildGroupShape childGroupShape = (ChildGroupShape)base.CloneImpl();
		childGroupShape.ChildShapes = new ChildShapeCollection(m_doc);
		foreach (ChildShape childShape in ChildShapes)
		{
			Entity entity = childShape.Clone();
			entity.SetOwner(childGroupShape);
			if (entity is ChildGroupShape)
			{
				childGroupShape.ChildShapes.Add(entity as ChildGroupShape);
			}
			else
			{
				childGroupShape.ChildShapes.Add(entity as ChildShape);
			}
		}
		childGroupShape.shapeId = 0L;
		return childGroupShape;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		int i = 0;
		for (int count = ChildShapes.Count; i < count; i++)
		{
			ChildShapes[i].CloneRelationsTo(doc, nextOwner);
		}
	}

	internal bool Compare(ChildGroupShape childGroupShape)
	{
		if (base.IsFillStyleInline != childGroupShape.IsFillStyleInline || base.FlipHorizantal != childGroupShape.FlipHorizantal || base.FlipVertical != childGroupShape.FlipVertical || base.IsScenePropertiesInline != childGroupShape.IsScenePropertiesInline || base.IsShapePropertiesInline != childGroupShape.IsShapePropertiesInline || base.IsLineStyleInline != childGroupShape.IsLineStyleInline || base.Visible != childGroupShape.Visible || base.LayoutInCell != childGroupShape.LayoutInCell || base.IsInsertRevision != childGroupShape.IsInsertRevision || base.IsDeleteRevision != childGroupShape.IsDeleteRevision || base.HorizontalPosition != childGroupShape.HorizontalPosition || base.X != childGroupShape.X || base.Y != childGroupShape.Y || base.VerticalPosition != childGroupShape.VerticalPosition || base.LeftMargin != childGroupShape.LeftMargin || base.TopMargin != childGroupShape.TopMargin || base.Rotation != childGroupShape.Rotation || base.ArcSize != childGroupShape.ArcSize || base.ShapeID != childGroupShape.ShapeID || base.Height != childGroupShape.Height || base.Width != childGroupShape.Width || base.HeightScale != childGroupShape.HeightScale || base.WidthScale != childGroupShape.WidthScale || base.AlternativeText != childGroupShape.AlternativeText || base.Name != childGroupShape.Name || base.Title != childGroupShape.Title || base.CoordinateSize != childGroupShape.CoordinateSize || base.CoordinateXOrigin != childGroupShape.CoordinateXOrigin || base.CoordinateYOrigin != childGroupShape.CoordinateYOrigin || OffsetXValue != childGroupShape.OffsetXValue || OffsetYValue != childGroupShape.OffsetYValue || ExtentXValue != childGroupShape.ExtentXValue || ExtentYValue != childGroupShape.ExtentYValue)
		{
			return false;
		}
		if ((base.Path2DList == null && childGroupShape.Path2DList != null) || (base.Path2DList != null && childGroupShape.Path2DList == null) || (base.VMLPathPoints == null && childGroupShape.VMLPathPoints != null) || (base.VMLPathPoints != null && childGroupShape.VMLPathPoints == null) || (base.ShapeGuide == null && childGroupShape.ShapeGuide != null) || (base.ShapeGuide != null && childGroupShape.ShapeGuide == null) || (base.ShapeStyleReferences == null && childGroupShape.ShapeStyleReferences != null) || (base.ShapeStyleReferences != null && childGroupShape.ShapeStyleReferences == null) || (base.TextFrame == null && childGroupShape.TextFrame != null) || (base.TextFrame != null && childGroupShape.TextFrame == null) || (base.LineFormat == null && childGroupShape.LineFormat != null) || (base.LineFormat != null && childGroupShape.LineFormat == null) || (base.FillFormat == null && childGroupShape.FillFormat != null) || (base.FillFormat != null && childGroupShape.FillFormat == null) || (base.EffectList == null && childGroupShape.EffectList != null) || (base.EffectList != null && childGroupShape.EffectList == null) || (base.DocxProps == null && childGroupShape.DocxProps != null) || (base.DocxProps != null && childGroupShape.DocxProps == null) || (ChildShapes == null && childGroupShape.ChildShapes != null) || (ChildShapes != null && childGroupShape.ChildShapes == null))
		{
			return false;
		}
		if (base.Path2DList != null && childGroupShape.Path2DList != null)
		{
			if (base.Path2DList.Count != childGroupShape.Path2DList.Count)
			{
				return false;
			}
			for (int i = 0; i < base.Path2DList.Count; i++)
			{
				if (!base.Path2DList[i].Compare(childGroupShape.Path2DList[i]))
				{
					return false;
				}
			}
		}
		if (base.VMLPathPoints != null && childGroupShape.VMLPathPoints != null)
		{
			if (base.VMLPathPoints.Count != childGroupShape.VMLPathPoints.Count)
			{
				return false;
			}
			for (int j = 0; j < base.VMLPathPoints.Count; j++)
			{
				if (!base.VMLPathPoints[j].Compare(childGroupShape.VMLPathPoints[j]))
				{
					return false;
				}
			}
		}
		if (base.ShapeGuide != null && childGroupShape.ShapeGuide != null)
		{
			if (base.ShapeGuide.Count != childGroupShape.ShapeGuide.Count)
			{
				return false;
			}
			foreach (string key in base.ShapeGuide.Keys)
			{
				if (!childGroupShape.ShapeGuide.ContainsKey(key) || !(base.ShapeGuide[key] == childGroupShape.ShapeGuide[key]))
				{
					return false;
				}
			}
		}
		if (base.ShapeStyleReferences != null && childGroupShape.ShapeStyleReferences != null)
		{
			if (base.ShapeStyleReferences.Count != childGroupShape.ShapeStyleReferences.Count)
			{
				return false;
			}
			for (int k = 0; k < base.ShapeStyleReferences.Count; k++)
			{
				if (!base.ShapeStyleReferences[k].Compare(childGroupShape.ShapeStyleReferences[k]))
				{
					return false;
				}
			}
		}
		if (base.TextFrame != null && !base.TextFrame.Compare(childGroupShape.TextFrame))
		{
			return false;
		}
		if (base.LineFormat != null && !base.LineFormat.Compare(childGroupShape.LineFormat))
		{
			return false;
		}
		if (base.FillFormat != null && !base.FillFormat.Compare(childGroupShape.FillFormat))
		{
			return false;
		}
		if (base.EffectList != null && childGroupShape.EffectList != null)
		{
			if (base.EffectList.Count != childGroupShape.EffectList.Count)
			{
				return false;
			}
			for (int l = 0; l < base.EffectList.Count; l++)
			{
				if (!base.EffectList[l].Compare(childGroupShape.EffectList[l]))
				{
					return false;
				}
			}
		}
		if (!base.Document.Comparison.CompareDocxProps(base.DocxProps, childGroupShape.DocxProps))
		{
			return false;
		}
		if (ChildShapes != null && childGroupShape.ChildShapes != null)
		{
			if (ChildShapes.Count != childGroupShape.ChildShapes.Count)
			{
				return false;
			}
			int num = 0;
			if (num < ChildShapes.Count)
			{
				if (ChildShapes[num] is ChildGroupShape && childGroupShape.ChildShapes[num] is ChildGroupShape && !(ChildShapes[num] as ChildGroupShape).Compare(childGroupShape.ChildShapes[num] as ChildGroupShape))
				{
					return false;
				}
				if (ChildShapes[num] != null && childGroupShape.ChildShapes[num] != null)
				{
					ChildShapes[num].Compare(childGroupShape.ChildShapes[num]);
					return false;
				}
				return false;
			}
		}
		return true;
	}

	internal new StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0014');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0014');
		return stringBuilder;
	}

	internal new StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.IsFillStyleInline ? "1" : "0;");
		stringBuilder.Append(base.FlipHorizantal ? "1" : "0;");
		stringBuilder.Append(base.FlipVertical ? "1" : "0;");
		stringBuilder.Append(base.IsScenePropertiesInline ? "1" : "0;");
		stringBuilder.Append(base.IsShapePropertiesInline ? "1" : "0;");
		stringBuilder.Append(base.IsLineStyleInline ? "1" : "0;");
		stringBuilder.Append(base.Visible ? "1" : "0;");
		stringBuilder.Append(base.LayoutInCell ? "1" : "0;");
		stringBuilder.Append(base.IsInsertRevision ? "1" : "0;");
		stringBuilder.Append(base.IsDeleteRevision ? "1" : "0;");
		stringBuilder.Append(base.X + ";");
		stringBuilder.Append(base.Y + ";");
		stringBuilder.Append(base.HorizontalPosition + ";");
		stringBuilder.Append(base.VerticalPosition + ";");
		stringBuilder.Append(base.LeftMargin + ";");
		stringBuilder.Append(base.TopMargin + ";");
		stringBuilder.Append(base.Rotation + ";");
		stringBuilder.Append(base.ArcSize + ";");
		stringBuilder.Append(base.ShapeID + ";");
		stringBuilder.Append(base.Height + ";");
		stringBuilder.Append(base.Width + ";");
		stringBuilder.Append(base.HeightScale + ";");
		stringBuilder.Append(base.WidthScale + ";");
		stringBuilder.Append(base.AlternativeText + ";");
		stringBuilder.Append(base.Name + ";");
		stringBuilder.Append(base.Title + ";");
		stringBuilder.Append(base.CoordinateSize + ";");
		stringBuilder.Append(base.CoordinateXOrigin + ";");
		stringBuilder.Append(base.CoordinateYOrigin + ";");
		stringBuilder.Append(OffsetXValue + ";");
		stringBuilder.Append(OffsetYValue + ";");
		stringBuilder.Append(ExtentXValue + ";");
		stringBuilder.Append(ExtentYValue + ";");
		if (base.Path2DList != null)
		{
			foreach (Path2D path2D in base.Path2DList)
			{
				stringBuilder.Append(path2D.GetAsString());
			}
		}
		if (base.VMLPathPoints != null)
		{
			foreach (Path2D vMLPathPoint in base.VMLPathPoints)
			{
				stringBuilder.Append(vMLPathPoint.GetAsString());
			}
		}
		if (base.ShapeGuide != null)
		{
			foreach (KeyValuePair<string, string> item in base.ShapeGuide)
			{
				stringBuilder.Append(item.Value + ";");
			}
		}
		if (base.ShapeStyleReferences != null)
		{
			foreach (ShapeStyleReference shapeStyleReference in base.ShapeStyleReferences)
			{
				stringBuilder.Append(shapeStyleReference.GetAsString());
			}
		}
		if (base.TextFrame != null)
		{
			stringBuilder.Append(base.TextFrame.GetAsString());
		}
		if (base.LineFormat != null)
		{
			stringBuilder.Append(base.LineFormat.GetAsString());
		}
		if (base.FillFormat != null)
		{
			stringBuilder.Append(base.FillFormat.GetAsString());
		}
		if (base.EffectList != null)
		{
			foreach (EffectFormat effect in base.EffectList)
			{
				stringBuilder.Append(effect.GetAsString());
			}
		}
		if (base.DocxProps != null)
		{
			foreach (KeyValuePair<string, Stream> docxProp in base.DocxProps)
			{
				stringBuilder.Append(base.Document.Comparison.ConvertBytesAsHash(Comparison.ConvertStreamToBytes(docxProp.Value)));
			}
		}
		if (ChildShapes != null)
		{
			foreach (ParagraphItem childShape in ChildShapes)
			{
				if (childShape is ChildGroupShape)
				{
					stringBuilder.Append((childShape as ChildGroupShape).GetAsString());
				}
				else if (childShape is ChildShape)
				{
					stringBuilder.Append((childShape as ChildShape).GetAsString());
				}
			}
		}
		return stringBuilder;
	}
}
