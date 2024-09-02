using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class GroupShape : ShapeBase, IEntity, ILeafWidget, IWidget
{
	private FillFormat m_fillFormat;

	private LineFormat m_lineFormat;

	private float m_rotation;

	internal bool? flipH;

	internal bool? flipV;

	private List<EffectFormat> m_effectList;

	private ChildShapeCollection m_childShapes;

	private byte m_bFlags;

	private List<ShapeStyleReference> m_shapeStyleItems;

	private AutoShapeType m_autoShapeType;

	private float m_xValue;

	private float m_yValue;

	private float m_extentXValue;

	private float m_extentYValue;

	internal Dictionary<string, Stream> m_docx2007Props;

	private float m_leftPosition;

	private float m_topPosition;

	private Dictionary<string, DictionaryEntry> m_relations;

	private Dictionary<string, ImageRecord> m_imageRelations;

	private List<string> m_styleProps;

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

	internal Dictionary<string, DictionaryEntry> Relations
	{
		get
		{
			if (m_relations == null)
			{
				m_relations = new Dictionary<string, DictionaryEntry>();
			}
			return m_relations;
		}
	}

	internal Dictionary<string, ImageRecord> ImageRelations
	{
		get
		{
			if (m_imageRelations == null)
			{
				m_imageRelations = new Dictionary<string, ImageRecord>();
			}
			return m_imageRelations;
		}
	}

	public float Rotation
	{
		get
		{
			return m_rotation;
		}
		set
		{
			m_rotation = value;
		}
	}

	public bool FlipHorizontal
	{
		get
		{
			if (!flipH.HasValue)
			{
				return false;
			}
			return flipH.Value;
		}
		set
		{
			flipH = value;
		}
	}

	public bool FlipVertical
	{
		get
		{
			if (!flipV.HasValue)
			{
				return false;
			}
			return flipV.Value;
		}
		set
		{
			flipV = value;
		}
	}

	public override EntityType EntityType => EntityType.GroupShape;

	internal AutoShapeType AutoShapeType
	{
		get
		{
			return m_autoShapeType;
		}
		set
		{
			m_autoShapeType = value;
		}
	}

	internal LineFormat LineFormat
	{
		get
		{
			if (m_lineFormat == null)
			{
				m_lineFormat = new LineFormat(this);
			}
			return m_lineFormat;
		}
		set
		{
			m_lineFormat = value;
		}
	}

	internal bool IsScenePropertiesInline
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

	internal bool IsLineStyleInline
	{
		get
		{
			return (m_bFlags1 & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal List<ShapeStyleReference> ShapeStyleReferences
	{
		get
		{
			if (m_shapeStyleItems == null)
			{
				m_shapeStyleItems = new List<ShapeStyleReference>();
			}
			return m_shapeStyleItems;
		}
		set
		{
			m_shapeStyleItems = value;
		}
	}

	internal bool IsEffectStyleInline
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal List<EffectFormat> EffectList
	{
		get
		{
			if (m_effectList == null)
			{
				m_effectList = new List<EffectFormat>();
			}
			return m_effectList;
		}
		set
		{
			m_effectList = value;
		}
	}

	internal bool Is2007Shape
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

	internal bool IsShapePropertiesInline
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

	internal FillFormat FillFormat
	{
		get
		{
			if (m_fillFormat == null)
			{
				m_fillFormat = new FillFormat(this);
			}
			return m_fillFormat;
		}
		set
		{
			m_fillFormat = value;
		}
	}

	internal bool IsFillStyleInline
	{
		get
		{
			return (m_bFlags1 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal float X
	{
		get
		{
			return m_xValue;
		}
		set
		{
			m_xValue = value;
		}
	}

	internal Dictionary<string, Stream> Docx2007Props
	{
		get
		{
			if (m_docx2007Props == null)
			{
				m_docx2007Props = new Dictionary<string, Stream>();
			}
			return m_docx2007Props;
		}
		set
		{
			m_docx2007Props = value;
		}
	}

	internal float Y
	{
		get
		{
			return m_yValue;
		}
		set
		{
			m_yValue = value;
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

	internal float LeftMargin
	{
		get
		{
			return m_leftPosition;
		}
		set
		{
			m_leftPosition = value;
		}
	}

	internal float TopMargin
	{
		get
		{
			return m_topPosition;
		}
		set
		{
			m_topPosition = value;
		}
	}

	internal List<string> DocxStyleProps
	{
		get
		{
			if (m_styleProps == null)
			{
				m_styleProps = new List<string>();
			}
			return m_styleProps;
		}
	}

	internal void Add(ParagraphItem[] childShapes)
	{
		foreach (ParagraphItem childShape in childShapes)
		{
			Add(childShape);
		}
	}

	public void Add(ParagraphItem childShape)
	{
		if (!base.Document.IsOpening && childShape.EntityType != EntityType.ChildShape && childShape.GetTextWrappingStyle() == TextWrappingStyle.Inline)
		{
			throw new Exception("Inline objects cannot be grouped");
		}
		ChildShape childShape2 = null;
		switch (childShape.EntityType)
		{
		case EntityType.AutoShape:
			childShape2 = ConvertShapeToChildShape(childShape as Shape);
			break;
		case EntityType.TextBox:
			childShape2 = ConvertTextboxToChildShape(childShape as WTextBox);
			break;
		case EntityType.Chart:
			childShape2 = ConvertChartToChildShape(childShape as WChart);
			break;
		case EntityType.Picture:
			childShape2 = ConvertPictureToChildShape(childShape as WPicture);
			break;
		case EntityType.GroupShape:
		{
			ChildGroupShape childGroupShape = ConvertGroupShapeToChildGroupShape(childShape as GroupShape);
			childGroupShape.SetOwner(this);
			ChildShapes.Add(childGroupShape);
			break;
		}
		case EntityType.ChildShape:
			childShape2 = childShape as ChildShape;
			break;
		case EntityType.ChildGroupShape:
			childShape2 = childShape as ChildGroupShape;
			break;
		default:
			throw new InvalidOperationException($"Cannot add object of type {childShape.EntityType} to Group Shape");
		}
		if (childShape2 != null)
		{
			childShape2.SetOwner(this);
			ChildShapes.Add(childShape2);
		}
	}

	internal ChildShape ConvertShapeToChildShape(Shape shape)
	{
		ChildShape childShape = new ChildShape(base.Document);
		childShape.ElementType = EntityType.AutoShape;
		childShape.AlternativeText = shape.AlternativeText;
		childShape.ArcSize = shape.ArcSize;
		childShape.AutoShapeType = shape.AutoShapeType;
		childShape.Title = shape.Title;
		childShape.Width = shape.Width;
		childShape.WidthScale = shape.WidthScale;
		childShape.Height = shape.Height;
		childShape.HeightScale = shape.HeightScale;
		childShape.Rotation = shape.Rotation;
		childShape.Name = shape.Name;
		childShape.Adjustments = shape.Adjustments;
		childShape.FlipHorizantal = shape.FlipHorizontal;
		childShape.FlipVertical = shape.FlipVertical;
		childShape.FontRefColor = shape.FontRefColor;
		childShape.Is2007Shape = shape.Is2007Shape;
		childShape.LayoutInCell = shape.LayoutInCell;
		childShape.UseNoShadeHR = shape.UseNoShadeHR;
		childShape.UseStandardColorHR = shape.UseStandardColorHR;
		childShape.TextFrame.HasInternalMargin = shape.TextFrame.HasInternalMargin;
		childShape.TextFrame.HeightOrigin = shape.TextFrame.HeightOrigin;
		childShape.TextFrame.HeightRelativePercent = shape.TextFrame.HeightRelativePercent;
		childShape.TextFrame.HorizontalRelativePercent = shape.TextFrame.HorizontalRelativePercent;
		childShape.TextFrame.NoAutoFit = shape.TextFrame.NoAutoFit;
		childShape.TextFrame.NormalAutoFit = shape.TextFrame.NormalAutoFit;
		childShape.TextFrame.NoWrap = shape.TextFrame.NoWrap;
		childShape.TextFrame.ShapeAutoFit = shape.TextFrame.ShapeAutoFit;
		childShape.TextFrame.TextDirection = shape.TextFrame.TextDirection;
		childShape.TextFrame.TextVerticalAlignment = shape.TextFrame.TextVerticalAlignment;
		childShape.TextFrame.VerticalRelativePercent = shape.TextFrame.VerticalRelativePercent;
		childShape.TextFrame.WidthOrigin = shape.TextFrame.WidthOrigin;
		childShape.TextFrame.WidthRelativePercent = shape.TextFrame.WidthRelativePercent;
		childShape.TextFrame.InternalMargin.Bottom = shape.TextFrame.InternalMargin.Bottom;
		childShape.TextFrame.InternalMargin.Left = shape.TextFrame.InternalMargin.Left;
		childShape.TextFrame.InternalMargin.Right = shape.TextFrame.InternalMargin.Right;
		childShape.TextFrame.InternalMargin.Top = shape.TextFrame.InternalMargin.Top;
		childShape.LineFormat = shape.LineFormat.Clone();
		float x = (childShape.HorizontalPosition = shape.HorizontalPosition);
		childShape.X = x;
		x = (childShape.VerticalPosition = shape.VerticalPosition);
		childShape.Y = x;
		childShape.CloneShapeFormat(shape);
		if (shape.TextBody != null)
		{
			childShape.TextBody = shape.TextBody.Clone() as WTextBody;
			childShape.TextBody.SetOwner(childShape);
		}
		if (shape.m_docxProps != null && shape.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(shape.DocxProps, ref childShape.m_docxProps);
		}
		if (shape.m_docx2007Props != null && shape.m_docx2007Props.Count > 0)
		{
			base.Document.CloneProperties(shape.Docx2007Props, ref childShape.m_docx2007Props);
		}
		if (shape.m_shapeGuide != null && shape.m_shapeGuide.Count > 0)
		{
			base.Document.CloneProperties(shape.ShapeGuide, ref childShape.m_shapeGuide);
		}
		return childShape;
	}

	internal ChildShape ConvertTextboxToChildShape(WTextBox textBox)
	{
		ChildShape childShape = new ChildShape(base.Document);
		childShape.ElementType = EntityType.TextBox;
		childShape.SetOwner(this);
		childShape.IsTextBoxShape = true;
		childShape.Name = textBox.Name;
		childShape.Visible = textBox.Visible;
		childShape.Height = textBox.TextBoxFormat.Height;
		childShape.Width = textBox.TextBoxFormat.Width;
		childShape.AutoShapeType = AutoShapeType.Rectangle;
		float x = (childShape.HorizontalPosition = textBox.TextBoxFormat.HorizontalPosition);
		childShape.X = x;
		x = (childShape.VerticalPosition = textBox.TextBoxFormat.VerticalPosition);
		childShape.Y = x;
		childShape.TextFrame.HeightOrigin = textBox.TextBoxFormat.HeightOrigin;
		childShape.TextFrame.HeightRelativePercent = textBox.TextBoxFormat.HeightRelativePercent;
		childShape.TextFrame.HorizontalRelativePercent = textBox.TextBoxFormat.HorizontalRelativePercent;
		childShape.TextFrame.InternalMargin.Bottom = textBox.TextBoxFormat.InternalMargin.Bottom;
		childShape.TextFrame.InternalMargin.Left = textBox.TextBoxFormat.InternalMargin.Left;
		childShape.TextFrame.InternalMargin.Right = textBox.TextBoxFormat.InternalMargin.Right;
		childShape.TextFrame.InternalMargin.Top = textBox.TextBoxFormat.InternalMargin.Top;
		childShape.TextFrame.TextDirection = textBox.TextBoxFormat.TextDirection;
		childShape.TextFrame.TextVerticalAlignment = textBox.TextBoxFormat.TextVerticalAlignment;
		childShape.TextFrame.VerticalRelativePercent = textBox.TextBoxFormat.VerticalRelativePercent;
		childShape.TextFrame.WidthOrigin = textBox.TextBoxFormat.WidthOrigin;
		childShape.TextFrame.WidthRelativePercent = textBox.TextBoxFormat.WidthRelativePercent;
		childShape.TextFrame.ShapeAutoFit = textBox.TextBoxFormat.AutoFit;
		childShape.FontRefColor = textBox.TextBoxFormat.TextThemeColor;
		childShape.Rotation = textBox.TextBoxFormat.Rotation;
		childShape.FlipVertical = textBox.TextBoxFormat.FlipVertical;
		childShape.FlipHorizantal = textBox.TextBoxFormat.FlipHorizontal;
		if (textBox.TextBoxFormat.FillEfects.Type == BackgroundType.Color)
		{
			childShape.FillFormat.Fill = true;
			childShape.FillFormat.Color = textBox.TextBoxFormat.FillColor;
			childShape.FillFormat.IsDefaultFill = false;
			childShape.IsFillStyleInline = true;
		}
		else if (textBox.TextBoxFormat.FillEfects.Type == BackgroundType.Gradient)
		{
			childShape.FillFormat.FillType = FillType.FillGradient;
			childShape.FillFormat.IsDefaultFill = false;
			childShape.IsFillStyleInline = true;
			childShape.FillFormat.ForeColor = textBox.TextBoxFormat.FillEfects.Color;
			childShape.FillFormat.Color = textBox.TextBoxFormat.FillEfects.Gradient.Color2;
		}
		else if (textBox.TextBoxFormat.FillEfects.Type == BackgroundType.Picture)
		{
			childShape.FillFormat.FillType = FillType.FillPicture;
			childShape.FillFormat.ImageRecord = new ImageRecord(base.Document, textBox.TextBoxFormat.FillEfects.ImageBytes);
			childShape.FillFormat.IsDefaultFill = false;
			childShape.IsFillStyleInline = true;
		}
		else
		{
			childShape.FillFormat.Fill = false;
			childShape.FillFormat.Color = Color.Empty;
			childShape.FillFormat.IsDefaultFill = false;
		}
		childShape.LineFormat.Line = !textBox.TextBoxFormat.NoLine;
		childShape.LineFormat.Color = textBox.TextBoxFormat.LineColor;
		childShape.LineFormat.Weight = textBox.TextBoxFormat.LineWidth;
		childShape.LineFormat.DashStyle = textBox.TextBoxFormat.LineDashing;
		if (textBox.TextBoxBody != null)
		{
			childShape.TextBody = textBox.TextBoxBody.Clone() as WTextBody;
			childShape.TextBody.SetOwner(childShape);
		}
		if (textBox.DocxProps != null && textBox.DocxProps.Count > 0)
		{
			base.Document.CloneProperties(textBox.DocxProps, ref childShape.m_docxProps);
		}
		return childShape;
	}

	internal ChildShape ConvertChartToChildShape(WChart chart)
	{
		ChildShape childShape = new ChildShape(base.Document);
		childShape.ElementType = EntityType.Chart;
		childShape.SetOwner(this);
		childShape.Chart = chart;
		childShape.Width = chart.Width;
		childShape.Height = chart.Height;
		childShape.WidthScale = chart.WidthScale;
		childShape.HeightScale = chart.HeightScale;
		childShape.Title = chart.Title;
		childShape.AlternativeText = chart.AlternativeText;
		childShape.Name = chart.Name;
		childShape.LayoutInCell = chart.LayoutInCell;
		float x = (childShape.HorizontalPosition = chart.HorizontalPosition);
		childShape.X = x;
		x = (childShape.VerticalPosition = chart.VerticalPosition);
		childShape.Y = x;
		if (chart.m_docxProps != null && chart.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(chart.DocxProps, ref childShape.m_docxProps);
		}
		return childShape;
	}

	internal ChildShape ConvertPictureToChildShape(WPicture picture)
	{
		ChildShape childShape = new ChildShape(base.Document);
		childShape.ElementType = EntityType.Picture;
		childShape.AlternativeText = picture.AlternativeText;
		childShape.FillFormat = picture.FillFormat.Clone();
		childShape.FillFormat.SourceRectangle = picture.FillRectangle.Clone();
		childShape.Height = picture.Height;
		if (picture.HeightScale > 0f)
		{
			childShape.HeightScale = picture.HeightScale;
		}
		childShape.Width = picture.Width;
		if (picture.WidthScale > 0f)
		{
			childShape.WidthScale = picture.WidthScale;
		}
		float x = (childShape.HorizontalPosition = picture.HorizontalPosition);
		childShape.X = x;
		x = (childShape.VerticalPosition = picture.VerticalPosition);
		childShape.Y = x;
		childShape.Title = picture.Title;
		childShape.Rotation = (int)picture.Rotation;
		childShape.Name = picture.Name;
		childShape.LayoutInCell = picture.LayoutInCell;
		ImageRecord imageRecord = new ImageRecord(base.Document, picture.ImageRecord);
		childShape.m_imageRecord = imageRecord;
		if (picture.ImageRecord != null)
		{
			childShape.m_imageRecord.ImageId = picture.ImageRecord.ImageId;
		}
		childShape.FlipHorizantal = picture.FlipHorizontal;
		childShape.FlipVertical = picture.FlipVertical;
		childShape.SvgData = picture.SvgData;
		childShape.OPictureHRef = picture.OPictureHRef;
		childShape.SvgExternalLink = picture.SvgExternalLink;
		if (picture.m_docxVisualShapeProps != null && picture.m_docxVisualShapeProps.Count > 0)
		{
			base.Document.CloneProperties(picture.DocxVisualShapeProps, ref childShape.m_pictureProps);
		}
		base.Document.HasPicture = true;
		return childShape;
	}

	internal ChildGroupShape ConvertGroupShapeToChildGroupShape(GroupShape groupShape)
	{
		ChildGroupShape childGroupShape = new ChildGroupShape(base.Document);
		childGroupShape.ElementType = EntityType.ChildGroupShape;
		childGroupShape.AlternativeText = groupShape.AlternativeText;
		childGroupShape.AutoShapeType = groupShape.AutoShapeType;
		if (groupShape.m_docxProps != null && groupShape.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(groupShape.DocxProps, ref childGroupShape.m_docxProps);
		}
		if (groupShape.m_docx2007Props != null && groupShape.m_docx2007Props.Count > 0)
		{
			base.Document.CloneProperties(groupShape.Docx2007Props, ref childGroupShape.m_docx2007Props);
		}
		for (int i = 0; i < groupShape.DocxStyleProps.Count; i++)
		{
			childGroupShape.DocxStyleProps.Add(groupShape.DocxStyleProps[i]);
		}
		childGroupShape.EffectList = groupShape.EffectList;
		childGroupShape.ExtentXValue = groupShape.ExtentXValue;
		childGroupShape.ExtentYValue = groupShape.ExtentYValue;
		childGroupShape.FillFormat = groupShape.FillFormat.Clone();
		childGroupShape.Height = groupShape.Height;
		childGroupShape.HeightScale = groupShape.HeightScale;
		childGroupShape.Is2007Shape = groupShape.Is2007Shape;
		childGroupShape.IsChangedCFormat = groupShape.IsChangedCFormat;
		childGroupShape.IsDetachedTextChanged = groupShape.IsDetachedTextChanged;
		childGroupShape.IsEffectStyleInline = groupShape.IsEffectStyleInline;
		childGroupShape.IsFillStyleInline = groupShape.IsFillStyleInline;
		childGroupShape.IsLineStyleInline = groupShape.IsLineStyleInline;
		childGroupShape.IsMappedItem = groupShape.IsMappedItem;
		childGroupShape.IsScenePropertiesInline = groupShape.IsScenePropertiesInline;
		childGroupShape.IsShapePropertiesInline = groupShape.IsShapePropertiesInline;
		childGroupShape.LayoutInCell = groupShape.LayoutInCell;
		childGroupShape.LeftMargin = groupShape.LeftMargin;
		childGroupShape.LineFormat = groupShape.LineFormat.Clone();
		childGroupShape.Name = groupShape.Name;
		childGroupShape.OffsetXValue = groupShape.X;
		childGroupShape.OffsetYValue = groupShape.Y;
		childGroupShape.ParaItemCharFormat.ApplyBase(groupShape.ParaItemCharFormat);
		childGroupShape.Rotation = groupShape.Rotation;
		childGroupShape.ShapeID = groupShape.ShapeID;
		childGroupShape.SkipDocxItem = groupShape.SkipDocxItem;
		childGroupShape.Title = groupShape.Title;
		childGroupShape.TopMargin = groupShape.TopMargin;
		childGroupShape.Visible = groupShape.Visible;
		childGroupShape.Width = groupShape.Width;
		childGroupShape.WidthScale = groupShape.WidthScale;
		childGroupShape.HorizontalPosition = groupShape.HorizontalPosition;
		childGroupShape.VerticalPosition = groupShape.VerticalPosition;
		foreach (ShapeStyleReference shapeStyleReference in groupShape.ShapeStyleReferences)
		{
			childGroupShape.ShapeStyleReferences.Add(shapeStyleReference);
		}
		foreach (KeyValuePair<string, DictionaryEntry> relation in groupShape.Relations)
		{
			childGroupShape.Relations.Add(relation.Key, relation.Value);
		}
		foreach (KeyValuePair<int, object> item in groupShape.PropertiesHash)
		{
			childGroupShape.PropertiesHash.Add(item.Key, item.Value);
		}
		for (int j = 0; j < groupShape.ChildShapes.Count; j++)
		{
			ChildShape childShape = groupShape.ChildShapes[j].Clone() as ChildShape;
			childShape.SetOwner(childGroupShape);
			childGroupShape.ChildShapes.Add(childShape);
		}
		return childGroupShape;
	}

	internal override void Detach()
	{
		base.Detach();
		if (!base.DeepDetached)
		{
			base.Document.FloatingItems.Remove(this);
		}
	}

	internal override void AttachToDocument()
	{
		if (base.WrapFormat.TextWrappingStyle != 0)
		{
			base.Document.FloatingItems.Add(this);
		}
		foreach (ChildShape childShape in ChildShapes)
		{
			childShape.AttachToDocument();
		}
		base.IsCloned = false;
	}

	internal void UpdatePositionForGroupShapeAndChildShape()
	{
		CalculateGroupShapeBounds();
		foreach (ChildShape childShape in ChildShapes)
		{
			childShape.X = childShape.HorizontalPosition - base.HorizontalPosition;
			childShape.Y = childShape.VerticalPosition - base.VerticalPosition;
		}
	}

	internal void CalculateGroupShapeBounds()
	{
		float num = 0f;
		for (int i = 0; i < ChildShapes.Count; i++)
		{
			float horizontalPosition = ChildShapes[i].HorizontalPosition;
			if (i == 0)
			{
				num = horizontalPosition;
			}
			else if (horizontalPosition < num)
			{
				num = horizontalPosition;
			}
		}
		float num2 = 0f;
		for (int j = 0; j < ChildShapes.Count; j++)
		{
			float verticalPosition = ChildShapes[j].VerticalPosition;
			if (j == 0)
			{
				num2 = verticalPosition;
			}
			else if (verticalPosition < num2)
			{
				num2 = verticalPosition;
			}
		}
		float num3 = 0f;
		for (int k = 0; k < ChildShapes.Count; k++)
		{
			float num4 = ChildShapes[k].VerticalPosition + ChildShapes[k].Height;
			if (k == 0)
			{
				num3 = num4;
			}
			else if (num4 > num3)
			{
				num3 = num4;
			}
		}
		float num5 = 0f;
		for (int l = 0; l < ChildShapes.Count; l++)
		{
			float num6 = ChildShapes[l].HorizontalPosition + ChildShapes[l].Width;
			if (l == 0)
			{
				num5 = num6;
			}
			else if (num6 > num5)
			{
				num5 = num6;
			}
		}
		base.HorizontalPosition = num;
		base.VerticalPosition = num2;
		base.Width = num5 - num;
		base.Height = num3 - num2;
		ExtentXValue = base.Width;
		ExtentYValue = base.Height;
	}

	public ParagraphItem[] Ungroup()
	{
		ParagraphItem[] array = Ungroup(ChildShapes, new PointF(base.HorizontalPosition, base.VerticalPosition));
		int num = GetIndexInOwnerCollection();
		WParagraph ownerParagraph = base.OwnerParagraph;
		RemoveSelf();
		for (int i = 0; i < array.Length; i++)
		{
			if (num > ownerParagraph.ChildEntities.Count)
			{
				break;
			}
			ownerParagraph.ChildEntities.Insert(num, array[i]);
			num++;
		}
		return array;
	}

	internal ParagraphItem[] Ungroup(ChildShapeCollection childShapes, PointF positionOfGroupShape)
	{
		List<ParagraphItem> list = new List<ParagraphItem>();
		_ = base.Document.LastSection.PageSetup.Margins;
		for (int i = 0; i < childShapes.Count; i++)
		{
			switch (childShapes[i].ElementType)
			{
			case EntityType.Picture:
			{
				WPicture wPicture = ConvertChildShapeToPicture(childShapes[i]);
				wPicture.HorizontalOrigin = base.HorizontalOrigin;
				wPicture.VerticalOrigin = base.VerticalOrigin;
				wPicture.HorizontalPosition = base.Width / ExtentXValue * childShapes[i].X - base.Width / ExtentXValue * X + positionOfGroupShape.X;
				wPicture.VerticalPosition = base.Height / ExtentYValue * childShapes[i].Y - base.Height / ExtentYValue * Y + positionOfGroupShape.Y;
				list.Add(wPicture);
				break;
			}
			case EntityType.AutoShape:
			{
				Shape shape = ConvertChildShapeToShape(childShapes[i]);
				shape.HorizontalOrigin = base.HorizontalOrigin;
				shape.VerticalOrigin = base.VerticalOrigin;
				shape.HorizontalPosition = base.Width / ExtentXValue * childShapes[i].X - base.Width / ExtentXValue * X + positionOfGroupShape.X;
				shape.VerticalPosition = base.Height / ExtentYValue * childShapes[i].Y - base.Height / ExtentYValue * Y + positionOfGroupShape.Y;
				list.Add(shape);
				break;
			}
			case EntityType.TextBox:
			{
				WTextBox wTextBox = ConvertChildShapeToTextbox(childShapes[i]);
				wTextBox.TextBoxFormat.HorizontalOrigin = base.HorizontalOrigin;
				wTextBox.TextBoxFormat.VerticalOrigin = base.VerticalOrigin;
				wTextBox.TextBoxFormat.HorizontalPosition = base.Width / ExtentXValue * childShapes[i].X - base.Width / ExtentXValue * X + positionOfGroupShape.X;
				wTextBox.TextBoxFormat.VerticalPosition = base.Height / ExtentYValue * childShapes[i].Y - base.Height / ExtentYValue * Y + positionOfGroupShape.Y;
				list.Add(wTextBox);
				break;
			}
			case EntityType.Chart:
			{
				WChart wChart = ConvertChildShapeToChart(childShapes[i]);
				wChart.HorizontalOrigin = base.HorizontalOrigin;
				wChart.VerticalOrigin = base.VerticalOrigin;
				wChart.HorizontalPosition = base.Width / ExtentXValue * childShapes[i].X - base.Width / ExtentXValue * X + positionOfGroupShape.X;
				wChart.VerticalPosition = base.Height / ExtentYValue * childShapes[i].Y - base.Height / ExtentYValue * Y + positionOfGroupShape.Y;
				list.Add(wChart);
				break;
			}
			case EntityType.ChildGroupShape:
			{
				ChildGroupShape childGroupShape = ChildShapes[i] as ChildGroupShape;
				GroupShape groupShape = ConvertChildGroupShapeToGroupShape(childGroupShape);
				groupShape.HorizontalPosition = base.Width / ExtentXValue * childShapes[i].X - base.Width / ExtentXValue * X + positionOfGroupShape.X;
				groupShape.VerticalPosition = base.Height / ExtentYValue * childShapes[i].Y - base.Height / ExtentYValue * Y + positionOfGroupShape.Y;
				list.Add(groupShape);
				break;
			}
			}
		}
		return list.ToArray();
	}

	internal Shape ConvertChildShapeToShape(ChildShape childShape)
	{
		Shape shape = new Shape(base.Document);
		shape.AlternativeText = childShape.AlternativeText;
		shape.ArcSize = childShape.ArcSize;
		shape.WrapFormat.AllowOverlap = true;
		shape.AutoShapeType = childShape.AutoShapeType;
		shape.Title = childShape.Title;
		shape.Width = base.Width / ExtentXValue * childShape.Width;
		shape.WidthScale = childShape.WidthScale;
		shape.Height = base.Height / ExtentYValue * childShape.Height;
		shape.HeightScale = childShape.HeightScale;
		shape.Rotation = (int)childShape.Rotation;
		shape.Name = childShape.Name;
		shape.Adjustments = childShape.Adjustments;
		shape.FlipHorizontal = childShape.FlipHorizantal;
		shape.FlipVertical = childShape.FlipVertical;
		shape.FontRefColor = childShape.FontRefColor;
		shape.Is2007Shape = childShape.Is2007Shape;
		shape.LayoutInCell = childShape.LayoutInCell;
		shape.UseNoShadeHR = childShape.UseNoShadeHR;
		shape.UseStandardColorHR = childShape.UseStandardColorHR;
		shape.TextFrame.HasInternalMargin = childShape.TextFrame.HasInternalMargin;
		shape.TextFrame.HeightOrigin = childShape.TextFrame.HeightOrigin;
		shape.TextFrame.HeightRelativePercent = childShape.TextFrame.HeightRelativePercent;
		shape.TextFrame.HorizontalRelativePercent = childShape.TextFrame.HorizontalRelativePercent;
		shape.TextFrame.NoAutoFit = childShape.TextFrame.NoAutoFit;
		shape.TextFrame.NormalAutoFit = childShape.TextFrame.NormalAutoFit;
		shape.TextFrame.NoWrap = childShape.TextFrame.NoWrap;
		shape.TextFrame.ShapeAutoFit = childShape.TextFrame.ShapeAutoFit;
		shape.TextFrame.TextDirection = childShape.TextFrame.TextDirection;
		shape.TextFrame.TextVerticalAlignment = childShape.TextFrame.TextVerticalAlignment;
		shape.TextFrame.VerticalRelativePercent = childShape.TextFrame.VerticalRelativePercent;
		shape.TextFrame.WidthOrigin = childShape.TextFrame.WidthOrigin;
		shape.TextFrame.WidthRelativePercent = childShape.TextFrame.WidthRelativePercent;
		shape.TextFrame.InternalMargin.m_intBottomMarg = childShape.TextFrame.InternalMargin.Bottom;
		shape.TextFrame.InternalMargin.m_intLeftMarg = childShape.TextFrame.InternalMargin.Left;
		shape.TextFrame.InternalMargin.m_intRightMarg = childShape.TextFrame.InternalMargin.Right;
		shape.TextFrame.InternalMargin.m_intTopMarg = childShape.TextFrame.InternalMargin.Top;
		childShape.CloneChildShapeFormatToShape(shape);
		if (!childShape.FillFormat.IsGrpFill)
		{
			shape.FillFormat.Fill = childShape.FillFormat.Fill;
		}
		shape.HorizontalOrigin = HorizontalOrigin.Margin;
		shape.VerticalOrigin = VerticalOrigin.Margin;
		shape.LineFormat.Line = childShape.LineFormat.Line;
		shape.LineFormat.Weight = childShape.LineFormat.Weight;
		if (childShape.Is2007Shape && childShape.m_isVMLPathUpdated)
		{
			shape.VMLPathPoints = childShape.VMLPathPoints;
		}
		if (childShape.Path2DList != null && childShape.Path2DList.Count > 0)
		{
			shape.Path2DList = childShape.Path2DList;
		}
		if (childShape.GetAvList().Count > 0)
		{
			shape.SetAvList(childShape.GetAvList());
		}
		if (childShape.GetGuideList().Count > 0)
		{
			shape.SetGuideList(childShape.GetGuideList());
		}
		if (childShape.TextBody != null)
		{
			shape.TextBody = childShape.TextBody.Clone() as WTextBody;
			shape.TextBody.SetOwner(shape);
		}
		if (childShape.m_docxProps != null && childShape.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(childShape.DocxProps, ref shape.m_docxProps);
		}
		if (childShape.m_docx2007Props != null && childShape.m_docx2007Props.Count > 0)
		{
			base.Document.CloneProperties(childShape.Docx2007Props, ref shape.m_docx2007Props);
		}
		if (childShape.m_shapeGuide != null && childShape.m_shapeGuide.Count > 0)
		{
			base.Document.CloneProperties(childShape.ShapeGuide, ref shape.m_shapeGuide);
		}
		return shape;
	}

	internal WPicture ConvertChildShapeToPicture(ChildShape childShape)
	{
		WPicture wPicture = new WPicture(base.Document);
		ImageRecord imageRecord = new ImageRecord(base.Document, childShape.m_imageRecord);
		wPicture.LoadImage(imageRecord);
		wPicture.TextWrappingStyle = childShape.GetOwnerGroupShape().GetTextWrappingStyle();
		wPicture.LayoutInCell = childShape.LayoutInCell;
		wPicture.AlternativeText = childShape.AlternativeText;
		wPicture.FillFormat = childShape.FillFormat.Clone();
		if (childShape.DocxProps.ContainsKey("grpFill"))
		{
			childShape.ApplyOwnerGroupShapeFill(wPicture);
		}
		wPicture.FillRectangle = childShape.FillFormat.SourceRectangle.Clone();
		wPicture.AllowOverlap = true;
		wPicture.HeightScale = childShape.HeightScale;
		wPicture.Height = base.Height / ExtentYValue * childShape.Height;
		wPicture.WidthScale = childShape.WidthScale;
		wPicture.Width = base.Width / ExtentXValue * childShape.Width;
		wPicture.HorizontalPosition = childShape.X;
		wPicture.VerticalPosition = childShape.Y;
		childShape.Title = wPicture.Title;
		wPicture.Rotation = childShape.Rotation;
		wPicture.Name = childShape.Name;
		wPicture.HorizontalOrigin = HorizontalOrigin.Margin;
		wPicture.VerticalOrigin = VerticalOrigin.Margin;
		if (childShape.m_pictureProps != null && childShape.m_pictureProps.Count > 0)
		{
			base.Document.CloneProperties(childShape.DocxPictureVisualProps, ref wPicture.m_docxVisualShapeProps);
		}
		base.Document.HasPicture = true;
		return wPicture;
	}

	internal GroupShape ConvertChildGroupShapeToGroupShape(ChildGroupShape childGroupShape)
	{
		GroupShape groupShape = new GroupShape(base.Document);
		groupShape.AlternativeText = childGroupShape.AlternativeText;
		groupShape.AutoShapeType = childGroupShape.AutoShapeType;
		groupShape.WrapFormat.AllowOverlap = true;
		if (childGroupShape.m_docxProps != null && childGroupShape.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(childGroupShape.DocxProps, ref groupShape.m_docxProps);
		}
		if (childGroupShape.m_docx2007Props != null && childGroupShape.m_docx2007Props.Count > 0)
		{
			base.Document.CloneProperties(childGroupShape.Docx2007Props, ref groupShape.m_docx2007Props);
		}
		for (int i = 0; i < childGroupShape.DocxStyleProps.Count; i++)
		{
			groupShape.DocxStyleProps.Add(childGroupShape.DocxStyleProps[i]);
		}
		groupShape.EffectList = childGroupShape.EffectList;
		groupShape.X = childGroupShape.X;
		groupShape.Y = childGroupShape.Y;
		groupShape.ExtentXValue = childGroupShape.ExtentXValue;
		groupShape.ExtentYValue = childGroupShape.ExtentYValue;
		groupShape.FillFormat = childGroupShape.FillFormat.Clone();
		groupShape.Height = base.Height / ExtentYValue * childGroupShape.Height;
		groupShape.HeightScale = childGroupShape.HeightScale;
		groupShape.Is2007Shape = childGroupShape.Is2007Shape;
		groupShape.IsChangedCFormat = childGroupShape.IsChangedCFormat;
		groupShape.IsDetachedTextChanged = childGroupShape.IsDetachedTextChanged;
		groupShape.IsEffectStyleInline = childGroupShape.IsEffectStyleInline;
		groupShape.IsFillStyleInline = childGroupShape.IsFillStyleInline;
		groupShape.IsLineStyleInline = childGroupShape.IsLineStyleInline;
		groupShape.IsMappedItem = childGroupShape.IsMappedItem;
		groupShape.IsScenePropertiesInline = childGroupShape.IsScenePropertiesInline;
		groupShape.IsShapePropertiesInline = childGroupShape.IsShapePropertiesInline;
		groupShape.LayoutInCell = childGroupShape.LayoutInCell;
		groupShape.LeftMargin = childGroupShape.LeftMargin;
		groupShape.LineFormat = childGroupShape.LineFormat.Clone();
		groupShape.Name = childGroupShape.Name;
		groupShape.ParaItemCharFormat.ApplyBase(childGroupShape.ParaItemCharFormat);
		groupShape.Rotation = childGroupShape.Rotation;
		groupShape.ShapeID = childGroupShape.ShapeID;
		groupShape.SkipDocxItem = childGroupShape.SkipDocxItem;
		groupShape.Title = childGroupShape.Title;
		groupShape.TopMargin = childGroupShape.TopMargin;
		groupShape.Visible = childGroupShape.Visible;
		groupShape.Width = base.Width / ExtentXValue * childGroupShape.Width;
		groupShape.WidthScale = childGroupShape.WidthScale;
		foreach (ShapeStyleReference shapeStyleReference in childGroupShape.ShapeStyleReferences)
		{
			groupShape.ShapeStyleReferences.Add(shapeStyleReference);
		}
		foreach (KeyValuePair<string, DictionaryEntry> relation in childGroupShape.Relations)
		{
			groupShape.Relations.Add(relation.Key, relation.Value);
		}
		foreach (KeyValuePair<int, object> item in childGroupShape.PropertiesHash)
		{
			groupShape.PropertiesHash.Add(item.Key, item.Value);
		}
		for (int j = 0; j < childGroupShape.ChildShapes.Count; j++)
		{
			ChildShape childShape = childGroupShape.ChildShapes[j].Clone() as ChildShape;
			childShape.SetOwner(groupShape);
			childShape.skipPositionUpdate = true;
			groupShape.ChildShapes.Add(childShape);
			childShape.skipPositionUpdate = false;
		}
		return groupShape;
	}

	internal WTextBox ConvertChildShapeToTextbox(ChildShape childShape)
	{
		WTextBox wTextBox = new WTextBox(base.Document);
		wTextBox.Name = childShape.Name;
		wTextBox.Visible = childShape.Visible;
		wTextBox.TextBoxFormat.AllowOverlap = true;
		wTextBox.TextBoxFormat.Height = base.Height / ExtentYValue * childShape.Height;
		wTextBox.TextBoxFormat.Width = base.Width / ExtentXValue * childShape.Width;
		wTextBox.TextBoxFormat.HorizontalPosition = childShape.X;
		wTextBox.TextBoxFormat.VerticalPosition = childShape.Y;
		wTextBox.TextBoxFormat.HeightOrigin = childShape.TextFrame.HeightOrigin;
		wTextBox.TextBoxFormat.HeightRelativePercent = childShape.TextFrame.HeightRelativePercent;
		wTextBox.TextBoxFormat.InternalMargin.Bottom = childShape.TextFrame.InternalMargin.Bottom;
		wTextBox.TextBoxFormat.InternalMargin.Left = childShape.TextFrame.InternalMargin.Left;
		wTextBox.TextBoxFormat.InternalMargin.Right = childShape.TextFrame.InternalMargin.Right;
		wTextBox.TextBoxFormat.InternalMargin.Top = childShape.TextFrame.InternalMargin.Top;
		wTextBox.TextBoxFormat.TextDirection = childShape.TextFrame.TextDirection;
		wTextBox.TextBoxFormat.TextVerticalAlignment = childShape.TextFrame.TextVerticalAlignment;
		wTextBox.TextBoxFormat.VerticalRelativePercent = childShape.TextFrame.VerticalRelativePercent;
		wTextBox.TextBoxFormat.WidthOrigin = childShape.TextFrame.WidthOrigin;
		wTextBox.TextBoxFormat.WidthRelativePercent = childShape.TextFrame.WidthRelativePercent;
		wTextBox.TextBoxFormat.AutoFit = childShape.TextFrame.ShapeAutoFit;
		wTextBox.TextBoxFormat.Rotation = childShape.Rotation;
		wTextBox.TextBoxFormat.FlipHorizontal = childShape.FlipHorizantal;
		wTextBox.TextBoxFormat.FlipVertical = childShape.FlipVertical;
		if (!childShape.FillFormat.IsDefaultFill)
		{
			if (childShape.DocxProps.ContainsKey("grpFill"))
			{
				childShape.ApplyOwnerGroupShapeFill(wTextBox);
			}
			else if (!childShape.FillFormat.Fill)
			{
				if (wTextBox.Shape != null)
				{
					wTextBox.Shape.FillFormat.Fill = childShape.FillFormat.Fill;
					wTextBox.Shape.IsFillStyleInline = childShape.IsFillStyleInline;
					wTextBox.Shape.FillFormat.IsDefaultFill = childShape.FillFormat.IsDefaultFill;
				}
				wTextBox.TextBoxFormat.FillColor = Color.Empty;
			}
			else if (childShape.FillFormat.FillType == FillType.FillGradient)
			{
				wTextBox.TextBoxFormat.FillEfects.Type = BackgroundType.Gradient;
				wTextBox.TextBoxFormat.FillEfects.Gradient.Color2 = childShape.FillFormat.Color;
			}
			else if (childShape.FillFormat.FillType == FillType.FillPicture)
			{
				wTextBox.TextBoxFormat.FillEfects.Type = BackgroundType.Picture;
				wTextBox.TextBoxFormat.FillEfects.ImageRecord = new ImageRecord(base.Document, childShape.FillFormat.ImageRecord.ImageBytes);
			}
			else if (childShape.FillFormat.FillType == FillType.FillPatterned)
			{
				wTextBox.TextBoxFormat.FillEfects.Type = BackgroundType.Color;
				wTextBox.TextBoxFormat.FillColor = childShape.FillFormat.ForeColor;
			}
			else
			{
				wTextBox.TextBoxFormat.FillColor = childShape.FillFormat.Color;
			}
		}
		else if (!childShape.Is2007Shape)
		{
			wTextBox.TextBoxFormat.FillColor = Color.Empty;
		}
		wTextBox.TextBoxFormat.NoLine = !childShape.LineFormat.Line;
		if (childShape.LineFormat.HasKey(12))
		{
			wTextBox.TextBoxFormat.LineColor = childShape.LineFormat.Color;
		}
		if (childShape.LineFormat.HasKey(11))
		{
			wTextBox.TextBoxFormat.LineWidth = childShape.LineFormat.Weight;
		}
		if (childShape.LineFormat.HasKey(5))
		{
			wTextBox.TextBoxFormat.LineDashing = childShape.LineFormat.DashStyle;
		}
		wTextBox.TextBoxFormat.TextThemeColor = childShape.FontRefColor;
		wTextBox.TextBoxFormat.HorizontalOrigin = HorizontalOrigin.Margin;
		wTextBox.TextBoxFormat.VerticalOrigin = VerticalOrigin.Margin;
		if (wTextBox.IsShape)
		{
			wTextBox.Shape.FillFormat.Transparency = childShape.FillFormat.Transparency;
		}
		if (childShape.TextBody != null)
		{
			wTextBox.m_textBody = childShape.TextBody.Clone() as WTextBody;
			wTextBox.m_textBody.SetOwner(wTextBox);
		}
		if (childShape.m_docxProps != null && childShape.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(childShape.DocxProps, ref wTextBox.m_docxProps);
		}
		return wTextBox;
	}

	internal WChart ConvertChildShapeToChart(ChildShape childShape)
	{
		WChart wChart = new WChart(base.Document);
		wChart = childShape.Chart;
		wChart.Width = base.Width / ExtentXValue * childShape.Width;
		wChart.Height = base.Height / ExtentYValue * childShape.Height;
		wChart.WidthScale = childShape.WidthScale;
		wChart.HeightScale = childShape.HeightScale;
		wChart.Title = childShape.Title;
		wChart.AlternativeText = childShape.AlternativeText;
		wChart.Name = childShape.Name;
		wChart.LayoutInCell = childShape.LayoutInCell;
		wChart.WrapFormat.AllowOverlap = true;
		wChart.HorizontalOrigin = HorizontalOrigin.Margin;
		wChart.VerticalOrigin = VerticalOrigin.Margin;
		if (childShape.m_docxProps != null && childShape.m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(childShape.DocxProps, ref wChart.m_docxProps);
		}
		return wChart;
	}

	public GroupShape(IWordDocument document)
		: base((WordDocument)document)
	{
		m_charFormat = new WCharacterFormat(base.Document, this);
		FillFormat.Color = Color.White;
		FillFormat.Fill = true;
		FillFormat.FillType = FillType.FillSolid;
		LineFormat.Color = Color.Black;
		LineFormat.DashStyle = LineDashing.Solid;
		LineFormat.Line = true;
		LineFormat.Style = LineStyle.Single;
		LineFormat.Transparency = 0f;
		LineFormat.m_Weight = 1f;
		if (!base.Document.IsOpening)
		{
			base.HorizontalOrigin = HorizontalOrigin.Page;
			base.VerticalOrigin = VerticalOrigin.Page;
		}
		else
		{
			base.HorizontalOrigin = HorizontalOrigin.Column;
			base.VerticalOrigin = VerticalOrigin.Paragraph;
		}
		base.HorizontalAlignment = ShapeHorizontalAlignment.None;
		base.VerticalAlignment = ShapeVerticalAlignment.None;
		base.WrapFormat.TextWrappingStyle = TextWrappingStyle.InFrontOfText;
	}

	public GroupShape(IWordDocument document, ParagraphItem[] childShapes)
		: base((WordDocument)document)
	{
		m_charFormat = new WCharacterFormat(base.Document, this);
		FillFormat.Color = Color.White;
		FillFormat.Fill = true;
		FillFormat.FillType = FillType.FillSolid;
		LineFormat.Color = Color.Black;
		LineFormat.DashStyle = LineDashing.Solid;
		LineFormat.Line = true;
		LineFormat.Style = LineStyle.Single;
		LineFormat.Transparency = 0f;
		LineFormat.Weight = 1f;
		if (!base.Document.IsOpening)
		{
			base.HorizontalOrigin = HorizontalOrigin.Page;
			base.VerticalOrigin = VerticalOrigin.Page;
		}
		else
		{
			base.HorizontalOrigin = HorizontalOrigin.Column;
			base.VerticalOrigin = VerticalOrigin.Paragraph;
		}
		base.HorizontalAlignment = ShapeHorizontalAlignment.None;
		base.VerticalAlignment = ShapeVerticalAlignment.None;
		base.WrapFormat.TextWrappingStyle = TextWrappingStyle.InFrontOfText;
		Add(childShapes);
	}

	internal bool Compare(GroupShape groupShape)
	{
		if (base.IsRelativeVerticalPosition != groupShape.IsRelativeVerticalPosition || base.IsRelativeHorizontalPosition != groupShape.IsRelativeHorizontalPosition || base.IsRelativeHeight != groupShape.IsRelativeHeight || base.IsRelativeWidth != groupShape.IsRelativeWidth || base.IsBelowText != groupShape.IsBelowText || base.LockAnchor != groupShape.LockAnchor || base.Visible != groupShape.Visible || FlipHorizontal != groupShape.FlipHorizontal || FlipVertical != groupShape.FlipVertical || IsScenePropertiesInline != groupShape.IsScenePropertiesInline || IsLineStyleInline != groupShape.IsLineStyleInline || IsShapePropertiesInline != groupShape.IsShapePropertiesInline || IsFillStyleInline != groupShape.IsFillStyleInline || base.HorizontalOrigin != groupShape.HorizontalOrigin || base.RelativeWidthHorizontalOrigin != groupShape.RelativeWidthHorizontalOrigin || base.RelativeHeightVerticalOrigin != groupShape.RelativeHeightVerticalOrigin || base.RelativeHorizontalOrigin != groupShape.RelativeHorizontalOrigin || base.RelativeVerticalOrigin != groupShape.RelativeVerticalOrigin || base.HorizontalAlignment != groupShape.HorizontalAlignment || base.VerticalOrigin != groupShape.VerticalOrigin || base.VerticalAlignment != groupShape.VerticalAlignment || AutoShapeType != groupShape.AutoShapeType || base.HorizontalPosition != groupShape.HorizontalPosition || base.RelativeHorizontalPosition != groupShape.RelativeHorizontalPosition || base.RelativeVerticalPosition != groupShape.RelativeVerticalPosition || base.RelativeHeight != groupShape.RelativeHeight || base.RelativeWidth != groupShape.RelativeWidth || base.VerticalPosition != groupShape.VerticalPosition || base.ZOrderPosition != groupShape.ZOrderPosition || base.Height != groupShape.Height || base.Width != groupShape.Width || base.HeightScale != groupShape.HeightScale || base.WidthScale != groupShape.WidthScale || base.AlternativeText != groupShape.AlternativeText || base.Title != groupShape.Title || base.CoordinateSize != groupShape.CoordinateSize || base.CoordinateXOrigin != groupShape.CoordinateXOrigin || base.CoordinateYOrigin != groupShape.CoordinateYOrigin || Rotation != groupShape.Rotation || X != groupShape.X || Y != groupShape.Y || ExtentXValue != groupShape.ExtentXValue || ExtentYValue != groupShape.ExtentYValue || LeftMargin != groupShape.LeftMargin || TopMargin != groupShape.TopMargin)
		{
			return false;
		}
		if ((ChildShapes == null && groupShape.ChildShapes != null) || (ChildShapes != null && groupShape.ChildShapes == null) || (LineFormat == null && groupShape.LineFormat != null) || (LineFormat != null && groupShape.LineFormat == null) || (FillFormat == null && groupShape.FillFormat != null) || (FillFormat != null && groupShape.FillFormat == null) || (ShapeStyleReferences == null && groupShape.ShapeStyleReferences != null) || (ShapeStyleReferences != null && groupShape.ShapeStyleReferences == null) || (EffectList == null && groupShape.EffectList != null) || (EffectList != null && groupShape.EffectList == null) || (base.WrapFormat == null && groupShape.WrapFormat != null) || (base.WrapFormat != null && groupShape.WrapFormat == null))
		{
			return false;
		}
		if (ChildShapes != null && groupShape.ChildShapes != null)
		{
			if (ChildShapes.Count != groupShape.ChildShapes.Count)
			{
				return false;
			}
			for (int i = 0; i < ChildShapes.Count; i++)
			{
				if (ChildShapes[i] is ChildGroupShape && groupShape.ChildShapes[i] is ChildGroupShape && !(ChildShapes[i] as ChildGroupShape).Compare(groupShape.ChildShapes[i] as ChildGroupShape))
				{
					return false;
				}
				if (ChildShapes[i] != null && groupShape.ChildShapes[i] != null && !ChildShapes[i].Compare(groupShape.ChildShapes[i]))
				{
					return false;
				}
			}
		}
		if (LineFormat != null && !LineFormat.Compare(groupShape.LineFormat))
		{
			return false;
		}
		if (FillFormat != null && !FillFormat.Compare(groupShape.FillFormat))
		{
			return false;
		}
		if (base.WrapFormat != null && !base.WrapFormat.Compare(groupShape.WrapFormat))
		{
			return false;
		}
		if (ShapeStyleReferences != null && groupShape.ShapeStyleReferences != null)
		{
			if (ShapeStyleReferences.Count != groupShape.ShapeStyleReferences.Count)
			{
				return false;
			}
			for (int j = 0; j < ShapeStyleReferences.Count; j++)
			{
				if (!ShapeStyleReferences[j].Compare(groupShape.ShapeStyleReferences[j]))
				{
					return false;
				}
			}
		}
		if (EffectList != null && groupShape.EffectList != null)
		{
			if (EffectList.Count != groupShape.EffectList.Count)
			{
				return false;
			}
			for (int k = 0; k < EffectList.Count; k++)
			{
				if (!EffectList[k].Compare(groupShape.EffectList[k]))
				{
					return false;
				}
			}
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0014');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0014');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.IsRelativeVerticalPosition ? "1" : "0;");
		stringBuilder.Append(base.IsRelativeHorizontalPosition ? "1" : "0;");
		stringBuilder.Append(base.IsRelativeHeight ? "1" : "0;");
		stringBuilder.Append(base.IsRelativeWidth ? "1" : "0;");
		stringBuilder.Append(base.IsBelowText ? "1" : "0;");
		stringBuilder.Append(base.LockAnchor ? "1" : "0;");
		stringBuilder.Append(base.Visible ? "1" : "0;");
		stringBuilder.Append(FlipHorizontal ? "1" : "0;");
		stringBuilder.Append(FlipVertical ? "1" : "0;");
		stringBuilder.Append(IsScenePropertiesInline ? "1" : "0;");
		stringBuilder.Append(IsLineStyleInline ? "1" : "0;");
		stringBuilder.Append(IsShapePropertiesInline ? "1" : "0;");
		stringBuilder.Append(IsFillStyleInline ? "1" : "0;");
		stringBuilder.Append((int)base.HorizontalOrigin + ";");
		stringBuilder.Append((int)base.RelativeWidthHorizontalOrigin + ";");
		stringBuilder.Append((int)base.RelativeHeightVerticalOrigin + ";");
		stringBuilder.Append((int)base.RelativeHorizontalOrigin + ";");
		stringBuilder.Append((int)base.RelativeVerticalOrigin + ";");
		stringBuilder.Append((int)base.HorizontalAlignment + ";");
		stringBuilder.Append((int)base.VerticalOrigin + ";");
		stringBuilder.Append((int)base.VerticalAlignment + ";");
		stringBuilder.Append((int)AutoShapeType + ";");
		stringBuilder.Append(base.HorizontalPosition + ";");
		stringBuilder.Append(base.RelativeHorizontalPosition + ";");
		stringBuilder.Append(base.RelativeVerticalPosition + ";");
		stringBuilder.Append(base.RelativeHeight + ";");
		stringBuilder.Append(base.RelativeWidth + ";");
		stringBuilder.Append(base.VerticalPosition + ";");
		stringBuilder.Append(base.WrapFormat?.ToString() + ";");
		stringBuilder.Append(base.ZOrderPosition + ";");
		stringBuilder.Append(base.Height + ";");
		stringBuilder.Append(base.Width + ";");
		stringBuilder.Append(base.HeightScale + ";");
		stringBuilder.Append(base.WidthScale + ";");
		stringBuilder.Append(base.AlternativeText + ";");
		stringBuilder.Append(base.Title + ";");
		stringBuilder.Append(base.CoordinateSize + ";");
		stringBuilder.Append(base.CoordinateXOrigin + ";");
		stringBuilder.Append(base.CoordinateYOrigin + ";");
		stringBuilder.Append(Rotation + ";");
		stringBuilder.Append(X + ";");
		stringBuilder.Append(Y + ";");
		stringBuilder.Append(ExtentXValue + ";");
		stringBuilder.Append(ExtentYValue + ";");
		stringBuilder.Append(LeftMargin + ";");
		stringBuilder.Append(TopMargin + ";");
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
		if (LineFormat != null)
		{
			stringBuilder.Append(LineFormat.GetAsString());
		}
		if (FillFormat != null)
		{
			stringBuilder.Append(FillFormat.GetAsString());
		}
		if (ShapeStyleReferences != null)
		{
			foreach (ShapeStyleReference shapeStyleReference in ShapeStyleReferences)
			{
				stringBuilder.Append(shapeStyleReference.GetAsString());
			}
		}
		if (EffectList != null)
		{
			foreach (EffectFormat effect in EffectList)
			{
				stringBuilder.Append(effect.GetAsString());
			}
		}
		return stringBuilder;
	}

	internal void InitializeVMLDefaultValues()
	{
		FillFormat.Color = Color.White;
		LineFormat.ForeColor = Color.Black;
		LineFormat.Color = Color.Empty;
		base.WrapFormat.AllowOverlap = true;
	}

	protected override object CloneImpl()
	{
		GroupShape groupShape = (GroupShape)base.CloneImpl();
		groupShape.IsCloned = true;
		groupShape.ChildShapes = new ChildShapeCollection(base.Document);
		for (int i = 0; i < ChildShapes.Count; i++)
		{
			Entity entity = ChildShapes[i].Clone();
			entity.SetOwner(groupShape);
			bool flag = false;
			if (entity is ChildGroupShape)
			{
				flag = (entity as ChildGroupShape).skipPositionUpdate;
				(entity as ChildGroupShape).skipPositionUpdate = true;
				groupShape.ChildShapes.Add(entity as ChildGroupShape);
				(entity as ChildGroupShape).skipPositionUpdate = flag;
			}
			else
			{
				flag = (entity as ChildShape).skipPositionUpdate;
				(entity as ChildShape).skipPositionUpdate = true;
				groupShape.ChildShapes.Add(entity as ChildShape);
				(entity as ChildShape).skipPositionUpdate = flag;
			}
		}
		CloneShapeFormat(groupShape);
		groupShape.ShapeID = 0L;
		return groupShape;
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

	internal bool HasChildGroupShape()
	{
		foreach (ChildShape childShape in ChildShapes)
		{
			if (childShape is ChildGroupShape)
			{
				return true;
			}
		}
		return false;
	}

	internal void ApplyCharacterFormat(WCharacterFormat charFormat)
	{
		if (charFormat != null)
		{
			SetParagraphItemCharacterFormat(charFormat);
		}
	}

	internal void CloneShapeFormat(GroupShape shape)
	{
		ChildShape childShape = new ChildShape(base.Document);
		bool flag = base.Document != null && base.Document.DocHasThemes;
		if (IsFillStyleInline && FillFormat != null)
		{
			shape.FillFormat = FillFormat.Clone();
			shape.IsFillStyleInline = true;
		}
		else if (flag && ShapeStyleReferences != null && ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex = ShapeStyleReferences[1].StyleRefIndex;
			if (styleRefIndex > 0 && base.Document.Themes.FmtScheme.FillFormats.Count > styleRefIndex)
			{
				uint opacity = uint.MaxValue;
				FillFormat fillFormat = shape.Document.Themes.FmtScheme.FillFormats[styleRefIndex - 1];
				shape.FillFormat = fillFormat.Clone();
				if (fillFormat.FillType == FillType.FillSolid)
				{
					shape.FillFormat.Color = ShapeStyleReferences[1].StyleRefColor;
					shape.FillFormat.Transparency = ShapeStyleReferences[1].StyleRefOpacity;
				}
				else if (fillFormat.FillType == FillType.FillGradient)
				{
					for (int i = 0; i < fillFormat.GradientFill.GradientStops.Count; i++)
					{
						shape.FillFormat.GradientFill.GradientStops[i].Color = childShape.StyleColorTransform(fillFormat.GradientFill.GradientStops[i].FillSchemeColorTransforms, ShapeStyleReferences[1].StyleRefColor, ref opacity);
						opacity = uint.MaxValue;
					}
				}
				else if (fillFormat.FillType == FillType.FillPatterned)
				{
					List<DictionaryEntry> list = new List<DictionaryEntry>();
					List<DictionaryEntry> list2 = new List<DictionaryEntry>();
					if (fillFormat.FillSchemeColorTransforms.Count > 0)
					{
						for (int j = 0; j < fillFormat.FillSchemeColorTransforms.Count; j++)
						{
							if (StartsWithExt(fillFormat.FillSchemeColorTransforms[j].Key.ToString(), "fgClr"))
							{
								list.Add(fillFormat.FillSchemeColorTransforms[j]);
							}
							if (StartsWithExt(fillFormat.FillSchemeColorTransforms[j].Key.ToString(), "bgClr"))
							{
								list2.Add(fillFormat.FillSchemeColorTransforms[j]);
							}
						}
					}
					shape.FillFormat.ForeColor = childShape.StyleColorTransform(list, ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
					shape.FillFormat.Color = childShape.StyleColorTransform(list2, ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
				}
				shape.IsFillStyleInline = true;
			}
		}
		if (IsLineStyleInline && LineFormat != null)
		{
			shape.LineFormat = LineFormat.Clone();
			shape.IsLineStyleInline = true;
		}
		else if (flag && ShapeStyleReferences != null && ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex2 = ShapeStyleReferences[0].StyleRefIndex;
			if (styleRefIndex2 > 0 && base.Document.Themes.FmtScheme.LnStyleScheme.Count > styleRefIndex2)
			{
				uint opacity2 = uint.MaxValue;
				LineFormat lineFormat = shape.Document.Themes.FmtScheme.LnStyleScheme[styleRefIndex2 - 1];
				shape.LineFormat = shape.Document.Themes.FmtScheme.LnStyleScheme[styleRefIndex2 - 1].Clone();
				if (lineFormat.LineFormatType == LineFormatType.Solid)
				{
					shape.LineFormat.Color = ShapeStyleReferences[0].StyleRefColor;
					shape.LineFormat.Transparency = ShapeStyleReferences[0].StyleRefOpacity;
				}
				else if (lineFormat.LineFormatType == LineFormatType.Gradient)
				{
					for (int k = 0; k < lineFormat.GradientFill.GradientStops.Count; k++)
					{
						shape.FillFormat.GradientFill.GradientStops[k].Color = childShape.StyleColorTransform(lineFormat.GradientFill.GradientStops[k].FillSchemeColorTransforms, ShapeStyleReferences[1].StyleRefColor, ref opacity2);
						opacity2 = uint.MaxValue;
					}
				}
				else if (lineFormat.LineFormatType == LineFormatType.Patterned)
				{
					List<DictionaryEntry> list3 = new List<DictionaryEntry>();
					List<DictionaryEntry> list4 = new List<DictionaryEntry>();
					if (lineFormat.LineSchemeColorTransforms.Count > 0)
					{
						for (int l = 0; l < lineFormat.LineSchemeColorTransforms.Count; l++)
						{
							if (StartsWithExt(lineFormat.LineSchemeColorTransforms[l].Key.ToString(), "fgClr"))
							{
								list3.Add(lineFormat.LineSchemeColorTransforms[l]);
							}
							if (StartsWithExt(lineFormat.LineSchemeColorTransforms[l].Key.ToString(), "bgClr"))
							{
								list4.Add(lineFormat.LineSchemeColorTransforms[l]);
							}
						}
					}
					shape.FillFormat.ForeColor = childShape.StyleColorTransform(list3, ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
					shape.FillFormat.Color = childShape.StyleColorTransform(list4, ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
				}
				shape.IsLineStyleInline = true;
			}
		}
		if (EffectList == null)
		{
			return;
		}
		List<EffectFormat> list5 = new List<EffectFormat>();
		for (int m = 0; m < EffectList.Count; m++)
		{
			EffectFormat effectFormat = new EffectFormat(this);
			if (EffectList[m].IsEffectListItem)
			{
				if (EffectList[m].IsShadowEffect && EffectList[m].ShadowFormat != null)
				{
					effectFormat.ShadowFormat = EffectList[m].ShadowFormat.Clone();
					effectFormat.IsShadowEffect = true;
				}
				else if (EffectList[m].IsGlowEffect && EffectList[m].GlowFormat != null)
				{
					effectFormat.GlowFormat = EffectList[m].GlowFormat.Clone();
					effectFormat.IsGlowEffect = true;
				}
				else if (EffectList[m].IsReflection && EffectList[m].ReflectionFormat != null)
				{
					effectFormat.ReflectionFormat = EffectList[m].ReflectionFormat.Clone();
					effectFormat.IsReflection = true;
				}
				else if (EffectList[m].IsSoftEdge)
				{
					effectFormat.NoSoftEdges = EffectList[m].NoSoftEdges;
					effectFormat.SoftEdgeRadius = EffectList[m].SoftEdgeRadius;
				}
				list5.Add(effectFormat);
				list5[m].IsEffectListItem = true;
			}
			else if ((EffectList[m].IsShapeProperties || EffectList[m].IsSceneProperties) && EffectList[m].ThreeDFormat != null)
			{
				effectFormat.ThreeDFormat = EffectList[m].ThreeDFormat.Clone();
				effectFormat.IsSceneProperties = EffectList[m].IsSceneProperties;
				effectFormat.IsShapeProperties = EffectList[m].IsShapeProperties;
				list5.Add(effectFormat);
			}
		}
		shape.IsEffectStyleInline = true;
		shape.EffectList.Clear();
		shape.EffectList = list5;
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
		base.WrapFormat.IsWrappingBoundsAdded = false;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	private void SetShapeWidth(WSection section)
	{
		if (base.RelativeWidth != 0f)
		{
			switch (base.RelativeWidthHorizontalOrigin)
			{
			case HorizontalOrigin.Page:
				base.Width = section.PageSetup.PageSize.Width * (base.RelativeWidth / 100f);
				break;
			case HorizontalOrigin.LeftMargin:
			case HorizontalOrigin.InsideMargin:
				base.Width = (section.PageSetup.Margins.Left + (section.Document.DOP.GutterAtTop ? 0f : section.PageSetup.Margins.Gutter)) * (base.RelativeWidth / 100f);
				break;
			case HorizontalOrigin.RightMargin:
			case HorizontalOrigin.OutsideMargin:
				base.Width = section.PageSetup.Margins.Right * (base.RelativeWidth / 100f);
				break;
			default:
				base.Width = (section.PageSetup.PageSize.Width - section.PageSetup.Margins.Left - (section.Document.DOP.GutterAtTop ? section.PageSetup.Margins.Gutter : 0f) - section.PageSetup.Margins.Right) * (base.RelativeWidth / 100f);
				break;
			}
		}
	}

	private void SetShapeHeight(WSection section)
	{
		if (base.RelativeHeight != 0f)
		{
			switch (base.RelativeHeightVerticalOrigin)
			{
			case VerticalOrigin.Page:
				base.Height = section.PageSetup.PageSize.Height * (base.RelativeHeight / 100f);
				break;
			case VerticalOrigin.TopMargin:
			case VerticalOrigin.InsideMargin:
				base.Height = (section.PageSetup.Margins.Top + (section.Document.DOP.GutterAtTop ? section.PageSetup.Margins.Gutter : 0f)) * (base.RelativeHeight / 100f);
				break;
			case VerticalOrigin.BottomMargin:
			case VerticalOrigin.OutsideMargin:
				base.Height = section.PageSetup.Margins.Bottom * (base.RelativeHeight / 100f);
				break;
			default:
				base.Height = (section.PageSetup.PageSize.Height - section.PageSetup.Margins.Top - (section.Document.DOP.GutterAtTop ? section.PageSetup.Margins.Gutter : 0f) - section.PageSetup.Margins.Bottom) * (base.RelativeHeight / 100f);
				break;
			}
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return new SizeF(base.Width, base.Height);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		Entity ownerSection = GetOwnerSection(this);
		if (ownerSection is WSection)
		{
			if (base.IsRelativeWidth)
			{
				SetShapeWidth(ownerSection as WSection);
			}
			if (base.IsRelativeHeight)
			{
				SetShapeHeight(ownerSection as WSection);
			}
		}
		WParagraph wParagraph = base.OwnerParagraph;
		if (base.Owner is InlineContentControl)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		if (wParagraph.IsInCell && ((IWidget)wParagraph).LayoutInfo.IsClipped)
		{
			m_layoutInfo.IsClipped = true;
		}
		if (base.WrapFormat.TextWrappingStyle != 0)
		{
			m_layoutInfo.IsSkipBottomAlign = true;
		}
		if (base.ParaItemCharFormat.Hidden)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (!base.Visible && GetTextWrappingStyle() != 0)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (base.IsDeleteRevision && !base.Document.RevisionOptions.ShowDeletedText)
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal override void Close()
	{
		if (m_childShapes != null)
		{
			foreach (ParagraphItem childShape in m_childShapes)
			{
				childShape.Close();
			}
			m_childShapes.Clear();
			m_childShapes = null;
		}
		if (m_fillFormat != null)
		{
			m_fillFormat.Close();
			m_fillFormat = null;
		}
		if (m_lineFormat != null)
		{
			m_lineFormat.Close();
			m_lineFormat = null;
		}
		if (m_effectList != null)
		{
			foreach (EffectFormat effect in m_effectList)
			{
				effect.Close();
			}
			m_effectList.Clear();
			m_effectList = null;
		}
		if (m_styleProps != null)
		{
			m_styleProps.Clear();
			m_styleProps = null;
		}
		if (m_docx2007Props != null)
		{
			foreach (Stream value in m_docx2007Props.Values)
			{
				value.Close();
			}
			m_docx2007Props.Clear();
			m_docx2007Props = null;
		}
		if (m_relations != null)
		{
			m_relations.Clear();
			m_relations = null;
		}
		if (m_imageRelations != null)
		{
			foreach (ImageRecord value2 in m_imageRelations.Values)
			{
				value2.Close();
			}
			m_imageRelations.Clear();
			m_imageRelations = null;
		}
		if (m_shapeStyleItems != null)
		{
			m_shapeStyleItems.Clear();
			m_shapeStyleItems = null;
		}
		base.Close();
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	internal byte[] GetAsImage()
	{
		try
		{
			DocumentLayouter documentLayouter = new DocumentLayouter();
			byte[] result = documentLayouter.ConvertAsImage(this);
			documentLayouter.Close();
			return result;
		}
		catch
		{
			return null;
		}
	}
}
