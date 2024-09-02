using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class Shape : ShapeBase, IEntity, ILeafWidget, IWidget
{
	private AutoShapeType m_AutoShapeType;

	private WTextBody m_TextBody;

	private FillFormat m_FillFormat;

	private LineFormat m_LineFormat;

	private List<string> m_styleProps;

	public string ShapeTypeID;

	private Dictionary<string, DictionaryEntry> m_relations;

	private TextFrame m_TextFrame;

	private string m_Adjustments;

	private double m_arcSize;

	private Color m_fontRefColor = Color.Empty;

	private RectangleF m_textLayoutingBounds;

	private float m_rotation;

	private byte m_bFlags;

	private List<EffectFormat> m_effectList;

	private List<ShapeStyleReference> m_shapeStyleItems;

	private WPicture m_fallbackPic;

	private List<Path2D> m_vmlPathPoints;

	private Dictionary<string, string> m_guideList;

	private Dictionary<string, string> m_avList;

	private List<Path2D> m_path2DList;

	internal bool m_isVMLPathUpdated;

	internal Dictionary<string, Stream> m_docx2007Props;

	private Dictionary<string, ImageRecord> m_imageRelations;

	internal Dictionary<string, string> m_shapeGuide;

	internal List<Path2D> VMLPathPoints
	{
		get
		{
			return m_vmlPathPoints;
		}
		set
		{
			m_vmlPathPoints = value;
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

	internal RectangleF TextLayoutingBounds
	{
		get
		{
			return m_textLayoutingBounds;
		}
		set
		{
			m_textLayoutingBounds = value;
		}
	}

	internal bool UseStandardColorHR
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

	internal bool UseNoShadeHR
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
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

	internal double ArcSize
	{
		get
		{
			return m_arcSize;
		}
		set
		{
			m_arcSize = value;
		}
	}

	internal bool IsHorizontalRule
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

	internal string Adjustments
	{
		get
		{
			return m_Adjustments;
		}
		set
		{
			m_Adjustments = value;
		}
	}

	public TextFrame TextFrame
	{
		get
		{
			if (m_TextFrame == null)
			{
				m_TextFrame = new TextFrame(this);
				m_TextFrame.SetOwner(this);
			}
			return m_TextFrame;
		}
		set
		{
			m_TextFrame = value;
		}
	}

	internal Color FontRefColor
	{
		get
		{
			return m_fontRefColor;
		}
		set
		{
			m_fontRefColor = value;
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

	internal Dictionary<string, string> ShapeGuide
	{
		get
		{
			if (m_shapeGuide == null)
			{
				m_shapeGuide = new Dictionary<string, string>();
			}
			return m_shapeGuide;
		}
	}

	public AutoShapeType AutoShapeType
	{
		get
		{
			return m_AutoShapeType;
		}
		internal set
		{
			m_AutoShapeType = value;
		}
	}

	public WTextBody TextBody
	{
		get
		{
			if (m_TextBody == null)
			{
				m_TextBody = new WTextBody(base.Document, this);
			}
			return m_TextBody;
		}
		set
		{
			m_TextBody = value;
		}
	}

	public LineFormat LineFormat
	{
		get
		{
			if (m_LineFormat == null)
			{
				m_LineFormat = new LineFormat(this);
			}
			return m_LineFormat;
		}
		set
		{
			m_LineFormat = value;
		}
	}

	public FillFormat FillFormat
	{
		get
		{
			if (m_FillFormat == null)
			{
				m_FillFormat = new FillFormat(this);
			}
			return m_FillFormat;
		}
		set
		{
			m_FillFormat = value;
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

	public override EntityType EntityType => EntityType.AutoShape;

	public bool FlipHorizontal
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

	public bool FlipVertical
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

	internal bool IsScenePropertiesInline
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsShapePropertiesInline
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
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

	internal WPicture FallbackPic
	{
		get
		{
			return m_fallbackPic;
		}
		set
		{
			m_fallbackPic = value;
		}
	}

	internal List<Path2D> Path2DList
	{
		get
		{
			return m_path2DList;
		}
		set
		{
			m_path2DList = value;
		}
	}

	internal bool Compare(Shape shape)
	{
		if (base.IsRelativeVerticalPosition != shape.IsRelativeVerticalPosition || base.IsRelativeHorizontalPosition != shape.IsRelativeHorizontalPosition || base.IsRelativeHeight != shape.IsRelativeHeight || base.IsRelativeWidth != shape.IsRelativeWidth || base.IsBelowText != shape.IsBelowText || base.LockAnchor != shape.LockAnchor || base.Visible != shape.Visible || UseStandardColorHR != shape.UseStandardColorHR || UseNoShadeHR != shape.UseNoShadeHR || FlipHorizontal != shape.FlipHorizontal || FlipVertical != shape.FlipVertical || IsLineStyleInline != shape.IsLineStyleInline || IsFillStyleInline != shape.IsFillStyleInline || IsScenePropertiesInline != shape.IsScenePropertiesInline || IsShapePropertiesInline != shape.IsShapePropertiesInline || base.HorizontalOrigin != shape.HorizontalOrigin || base.RelativeWidthHorizontalOrigin != shape.RelativeWidthHorizontalOrigin || base.RelativeHeightVerticalOrigin != shape.RelativeHeightVerticalOrigin || base.RelativeHorizontalOrigin != shape.RelativeHorizontalOrigin || base.RelativeVerticalOrigin != shape.RelativeVerticalOrigin || base.HorizontalAlignment != shape.HorizontalAlignment || base.VerticalOrigin != shape.VerticalOrigin || base.VerticalAlignment != shape.VerticalAlignment || AutoShapeType != shape.AutoShapeType || base.HorizontalPosition != shape.HorizontalPosition || base.RelativeHorizontalPosition != shape.RelativeHorizontalPosition || base.RelativeVerticalPosition != shape.RelativeVerticalPosition || base.RelativeHeight != shape.RelativeHeight || base.RelativeWidth != shape.RelativeWidth || base.VerticalPosition != shape.VerticalPosition || base.ZOrderPosition != shape.ZOrderPosition || base.Height != shape.Height || base.Width != shape.Width || base.HeightScale != shape.HeightScale || base.WidthScale != shape.WidthScale || base.AlternativeText != shape.AlternativeText || base.Title != shape.Title || base.Path != shape.Path || base.CoordinateSize != shape.CoordinateSize || base.CoordinateXOrigin != shape.CoordinateXOrigin || base.CoordinateYOrigin != shape.CoordinateYOrigin || Rotation != shape.Rotation || ArcSize != shape.ArcSize || Adjustments != shape.Adjustments)
		{
			return false;
		}
		if ((TextBody == null && shape.TextBody != null) || (TextBody != null && shape.TextBody == null) || (TextFrame == null && shape.TextFrame != null) || (TextFrame != null && shape.TextFrame == null) || (LineFormat == null && shape.LineFormat != null) || (LineFormat != null && shape.LineFormat == null) || (FillFormat == null && shape.FillFormat != null) || (FillFormat != null && shape.FillFormat == null) || (ShapeStyleReferences == null && shape.ShapeStyleReferences != null) || (ShapeStyleReferences != null && shape.ShapeStyleReferences == null) || (EffectList == null && shape.EffectList != null) || (EffectList != null && shape.EffectList == null) || (Path2DList == null && shape.Path2DList != null) || (Path2DList != null && shape.Path2DList == null) || (ShapeGuide == null && shape.ShapeGuide != null) || (ShapeGuide != null && shape.ShapeGuide == null) || (DocxStyleProps == null && shape.DocxStyleProps != null) || (DocxStyleProps != null && shape.DocxStyleProps == null) || (VMLPathPoints == null && shape.VMLPathPoints != null) || (VMLPathPoints != null && shape.VMLPathPoints == null) || (base.WrapFormat == null && shape.WrapFormat != null) || (base.WrapFormat != null && shape.WrapFormat == null))
		{
			return false;
		}
		if (TextFrame != null && !TextFrame.Compare(shape.TextFrame))
		{
			return false;
		}
		if (LineFormat != null && !LineFormat.Compare(shape.LineFormat))
		{
			return false;
		}
		if (FillFormat != null && !FillFormat.Compare(shape.FillFormat))
		{
			return false;
		}
		if (ShapeStyleReferences != null && shape.ShapeStyleReferences != null)
		{
			if (ShapeStyleReferences.Count != shape.ShapeStyleReferences.Count)
			{
				return false;
			}
			for (int i = 0; i < ShapeStyleReferences.Count; i++)
			{
				if (!ShapeStyleReferences[i].Compare(shape.ShapeStyleReferences[i]))
				{
					return false;
				}
			}
		}
		if (EffectList != null && shape.EffectList != null)
		{
			if (EffectList.Count != shape.EffectList.Count)
			{
				return false;
			}
			for (int j = 0; j < EffectList.Count; j++)
			{
				if (!EffectList[j].Compare(shape.EffectList[j]))
				{
					return false;
				}
			}
		}
		if (Path2DList != null && shape.Path2DList != null)
		{
			if (Path2DList.Count != shape.Path2DList.Count)
			{
				return false;
			}
			for (int k = 0; k < Path2DList.Count; k++)
			{
				if (!Path2DList[k].Compare(shape.Path2DList[k]))
				{
					return false;
				}
			}
		}
		if (ShapeGuide != null && shape.ShapeGuide != null)
		{
			if (ShapeGuide.Count != shape.ShapeGuide.Count)
			{
				return false;
			}
			foreach (string key in ShapeGuide.Keys)
			{
				if (!shape.ShapeGuide.ContainsKey(key) || !(ShapeGuide[key] == shape.ShapeGuide[key]))
				{
					return false;
				}
			}
		}
		if (DocxStyleProps != null && shape.DocxStyleProps != null)
		{
			if (DocxStyleProps.Count != shape.DocxStyleProps.Count)
			{
				return false;
			}
			for (int l = 0; l < DocxStyleProps.Count; l++)
			{
				if (!DocxStyleProps[l].Equals(shape.DocxStyleProps[l]))
				{
					return false;
				}
			}
		}
		if (VMLPathPoints != null && shape.VMLPathPoints != null)
		{
			if (VMLPathPoints.Count != shape.VMLPathPoints.Count)
			{
				return false;
			}
			for (int m = 0; m < VMLPathPoints.Count; m++)
			{
				if (!VMLPathPoints[m].Compare(shape.VMLPathPoints[m]))
				{
					return false;
				}
			}
		}
		if (base.WrapFormat != null && !base.WrapFormat.Compare(shape.WrapFormat))
		{
			return false;
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
		string text = (base.IsRelativeVerticalPosition ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (base.IsRelativeHorizontalPosition ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (base.IsRelativeHeight ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (base.IsRelativeWidth ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (base.IsBelowText ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (base.LockAnchor ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (base.Visible ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UseStandardColorHR ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (UseNoShadeHR ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (FlipHorizontal ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (FlipVertical ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsLineStyleInline ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsFillStyleInline ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsScenePropertiesInline ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsShapePropertiesInline ? "1" : "0");
		stringBuilder.Append(text + ";");
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
		stringBuilder.Append(base.ZOrderPosition + ";");
		stringBuilder.Append(base.Height + ";");
		stringBuilder.Append(base.Width + ";");
		stringBuilder.Append(base.HeightScale + ";");
		stringBuilder.Append(base.WidthScale + ";");
		stringBuilder.Append(base.AlternativeText + ";");
		stringBuilder.Append(base.Title + ";");
		stringBuilder.Append(base.Path + ";");
		stringBuilder.Append(base.CoordinateSize + ";");
		stringBuilder.Append(base.CoordinateXOrigin + ";");
		stringBuilder.Append(base.CoordinateYOrigin + ";");
		stringBuilder.Append(Rotation + ";");
		stringBuilder.Append(ArcSize + ";");
		stringBuilder.Append(Adjustments + ";");
		if (TextBody != null)
		{
			stringBuilder.Append(TextBody.GetAsString());
		}
		if (TextFrame != null)
		{
			stringBuilder.Append(TextFrame.GetAsString());
		}
		if (LineFormat != null)
		{
			stringBuilder.Append(LineFormat.GetAsString());
		}
		if (FillFormat != null)
		{
			stringBuilder.Append(FillFormat.GetAsString());
		}
		if (base.WrapFormat != null)
		{
			stringBuilder.Append(base.WrapFormat.GetAsString());
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
		if (Path2DList != null)
		{
			foreach (Path2D path2D in Path2DList)
			{
				stringBuilder.Append(path2D.GetAsString());
			}
		}
		if (ShapeGuide != null)
		{
			foreach (KeyValuePair<string, string> item in ShapeGuide)
			{
				stringBuilder.Append(item.Value + ";");
			}
		}
		if (DocxStyleProps != null)
		{
			foreach (string docxStyleProp in DocxStyleProps)
			{
				stringBuilder.Append(docxStyleProp + ";");
			}
		}
		if (VMLPathPoints != null)
		{
			foreach (Path2D vMLPathPoint in VMLPathPoints)
			{
				stringBuilder.Append(vMLPathPoint.GetAsString());
			}
		}
		return stringBuilder;
	}

	internal Shape(IWordDocument doc)
		: base((WordDocument)doc)
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
		base.HorizontalOrigin = HorizontalOrigin.Column;
		base.VerticalOrigin = VerticalOrigin.Paragraph;
		base.HorizontalAlignment = ShapeHorizontalAlignment.None;
		base.VerticalAlignment = ShapeVerticalAlignment.None;
		base.WrapFormat.SetTextWrappingStyleValue(TextWrappingStyle.InFrontOfText);
		TextFrame.TextVerticalAlignment = DocGen.DocIO.DLS.VerticalAlignment.Top;
	}

	public Shape(IWordDocument doc, AutoShapeType autoShapeType)
		: this((WordDocument)doc)
	{
		m_AutoShapeType = autoShapeType;
		if (base.Document != null && !base.Document.IsOpening && (!base.Document.IsCloning & !base.Document.IsMailMerge))
		{
			TextFrame.InternalMargin.SetDefaultMargins();
		}
	}

	internal void InitializeVMLDefaultValues()
	{
		base.WrapFormat.TextWrappingStyle = TextWrappingStyle.Inline;
		FillFormat.Color = Color.White;
		LineFormat.ForeColor = Color.Black;
		LineFormat.Color = Color.Empty;
		base.WrapFormat.AllowOverlap = true;
	}

	internal override void Detach()
	{
		base.Detach();
		if (!base.DeepDetached)
		{
			base.Document.AutoShapeCollection.Remove(this);
			base.Document.FloatingItems.Remove(this);
		}
	}

	internal override void AttachToDocument()
	{
		base.Document.AutoShapeCollection.Add(this);
		if (base.WrapFormat.TextWrappingStyle != 0)
		{
			base.Document.FloatingItems.Add(this);
		}
		base.IsCloned = false;
		if (TextBody != null)
		{
			TextBody.AttachToDocument();
		}
	}

	protected override object CloneImpl()
	{
		Shape shape = (Shape)base.CloneImpl();
		shape.IsCloned = true;
		if (m_TextBody != null)
		{
			shape.m_TextBody = m_TextBody.Clone() as WTextBody;
			shape.m_TextBody.SetOwner(shape);
		}
		CloneShapeFormat(shape);
		if (m_docx2007Props != null && m_docx2007Props.Count > 0)
		{
			shape.Document.CloneProperties(Docx2007Props, ref shape.m_docx2007Props);
		}
		shape.ShapeID = 0L;
		return shape;
	}

	internal void CloneShapeFormat(Shape shape)
	{
		bool flag = base.Document != null && base.Document.DocHasThemes;
		if (IsFillStyleInline && FillFormat != null)
		{
			shape.FillFormat = FillFormat.Clone();
			shape.IsFillStyleInline = true;
		}
		else if (flag && ShapeStyleReferences != null && ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex = ShapeStyleReferences[1].StyleRefIndex;
			if (styleRefIndex > 0 && base.Document.Themes.FmtScheme.FillFormats.Count > styleRefIndex - 1)
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
						shape.FillFormat.GradientFill.GradientStops[i].Color = StyleColorTransform(fillFormat.GradientFill.GradientStops[i].FillSchemeColorTransforms, ShapeStyleReferences[1].StyleRefColor, ref opacity);
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
					shape.FillFormat.ForeColor = StyleColorTransform(list, ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
					shape.FillFormat.Color = StyleColorTransform(list2, ShapeStyleReferences[1].StyleRefColor, ref opacity);
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
						shape.FillFormat.GradientFill.GradientStops[k].Color = StyleColorTransform(lineFormat.GradientFill.GradientStops[k].FillSchemeColorTransforms, ShapeStyleReferences[1].StyleRefColor, ref opacity2);
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
					shape.FillFormat.ForeColor = StyleColorTransform(list3, ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
					shape.FillFormat.Color = StyleColorTransform(list4, ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
				}
				shape.IsLineStyleInline = true;
			}
		}
		if (EffectList != null)
		{
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
		if (m_guideList != null)
		{
			base.Document.CloneProperties(m_guideList, ref shape.m_guideList);
		}
		if (m_avList != null)
		{
			base.Document.CloneProperties(m_avList, ref shape.m_avList);
		}
		if (Path2DList != null)
		{
			List<Path2D> list6 = new List<Path2D>();
			for (int n = 0; n < Path2DList.Count; n++)
			{
				Path2D item = Path2DList[n].Clone();
				list6.Add(item);
			}
			shape.Path2DList = list6;
		}
	}

	internal Color StyleColorTransform(List<DictionaryEntry> fillTransformation, Color themeColor, ref uint opacity)
	{
		bool flag = false;
		foreach (DictionaryEntry item in fillTransformation)
		{
			string empty = string.Empty;
			switch ((!StartsWithExt(item.Key.ToString(), "fgClr") && !StartsWithExt(item.Key.ToString(), "bgClr")) ? item.Key.ToString() : item.Key.ToString().Substring(5))
			{
			case "alpha":
			{
				flag = false;
				string value4 = item.Value.ToString();
				if (!string.IsNullOrEmpty(value4))
				{
					double percentage3 = GetPercentage(value4);
					opacity = (uint)(percentage3 * 65536.0 / 100.0);
					if (opacity > 65536)
					{
						opacity = 65536u;
					}
				}
				break;
			}
			case "alphaMod":
			{
				flag = false;
				string value2 = item.Value.ToString();
				if (!string.IsNullOrEmpty(value2))
				{
					double percentage = GetPercentage(value2);
					opacity = (uint)((double)((opacity == uint.MaxValue) ? 65536u : opacity) * (percentage / 100.0));
					if (opacity > 65536)
					{
						opacity = 65536u;
					}
				}
				break;
			}
			case "alphaOff":
			{
				if (flag)
				{
					break;
				}
				string value3 = item.Value.ToString();
				if (!string.IsNullOrEmpty(value3))
				{
					double percentage2 = GetPercentage(value3);
					opacity = (uint)((double)((opacity != uint.MaxValue) ? opacity : 0u) + Math.Round(percentage2 * 65536.0 / 100.0));
					if (opacity > 65536)
					{
						opacity = 65536u;
					}
				}
				break;
			}
			default:
			{
				string value = item.Value.ToString();
				if (string.IsNullOrEmpty(value) && (item.Key.ToString() == "comp" || item.Key.ToString() == "gamma" || item.Key.ToString() == "gray" || item.Key.ToString() == "invGamma" || item.Key.ToString() == "inv"))
				{
					value = string.Empty;
				}
				if (!string.IsNullOrEmpty(value))
				{
					flag = ColorTransform(item.Key.ToString(), value, ref themeColor);
				}
				if (flag)
				{
					opacity = uint.MaxValue;
				}
				break;
			}
			}
		}
		return themeColor;
	}

	private double GetPercentage(string value)
	{
		double result;
		if (value.EndsWith("%"))
		{
			double.TryParse(value.Replace("%", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
			return result;
		}
		double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
		return result / 1000.0;
	}

	private bool ColorTransform(string localName, string value, ref Color themeColor)
	{
		switch (localName)
		{
		case "blue":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage2 = GetPercentage(value);
				byte blue = (byte)Math.Round(255.0 * WordColor.ConvertsLinearRGBtoRGB(percentage2 / 100.0));
				themeColor = Color.FromArgb(themeColor.A, themeColor.R, themeColor.G, blue);
			}
			return true;
		case "blueMod":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage3 = GetPercentage(value);
				themeColor = Color.FromArgb(themeColor.A, themeColor.R, themeColor.G, WordColor.ConvertbyModulation(themeColor.B, percentage3));
			}
			return true;
		case "blueOff":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage13 = GetPercentage(value);
				themeColor = Color.FromArgb(themeColor.A, themeColor.R, themeColor.G, WordColor.ConvertbyOffset(themeColor.B, percentage13));
			}
			return true;
		case "green":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage10 = GetPercentage(value);
				byte green = (byte)Math.Round(255.0 * WordColor.ConvertsLinearRGBtoRGB(percentage10 / 100.0));
				themeColor = Color.FromArgb(themeColor.A, themeColor.R, green, themeColor.B);
			}
			return true;
		case "greenMod":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage6 = GetPercentage(value);
				themeColor = Color.FromArgb(themeColor.A, themeColor.R, WordColor.ConvertbyModulation(themeColor.G, percentage6), themeColor.B);
			}
			return true;
		case "greenOff":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage4 = GetPercentage(value);
				themeColor = Color.FromArgb(themeColor.A, themeColor.R, WordColor.ConvertbyOffset(themeColor.G, percentage4), themeColor.B);
			}
			return true;
		case "red":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage11 = GetPercentage(value);
				byte red = (byte)Math.Round(255.0 * WordColor.ConvertsLinearRGBtoRGB(percentage11 / 100.0));
				themeColor = Color.FromArgb(themeColor.A, red, themeColor.G, themeColor.B);
			}
			return true;
		case "redMod":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage8 = GetPercentage(value);
				themeColor = Color.FromArgb(themeColor.A, WordColor.ConvertbyModulation(themeColor.R, percentage8), themeColor.G, themeColor.B);
			}
			return true;
		case "redOff":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage15 = GetPercentage(value);
				themeColor = Color.FromArgb(themeColor.A, WordColor.ConvertbyOffset(themeColor.R, percentage15), themeColor.G, themeColor.B);
			}
			return true;
		case "hue":
			if (!string.IsNullOrEmpty(value))
			{
				double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result2);
				result2 /= 60000.0;
				WordColor.ConvertbyHue(ref themeColor, result2);
			}
			return true;
		case "hueMod":
			if (!string.IsNullOrEmpty(value))
			{
				double ratio = GetPercentage(value) / 100.0;
				WordColor.ConvertbyHueMod(ref themeColor, ratio);
			}
			return true;
		case "hueOff":
			if (!string.IsNullOrEmpty(value))
			{
				double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result);
				result /= 60000.0;
				WordColor.ConvertbyHueOffset(ref themeColor, result);
			}
			return true;
		case "sat":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage = GetPercentage(value);
				WordColor.ConvertbySat(ref themeColor, percentage);
			}
			return true;
		case "satMod":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage14 = GetPercentage(value);
				WordColor.ConvertbySatMod(ref themeColor, percentage14);
			}
			return true;
		case "satOff":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage12 = GetPercentage(value);
				WordColor.ConvertbySatOffset(ref themeColor, percentage12);
			}
			return true;
		case "lum":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage9 = GetPercentage(value);
				WordColor.ConvertbyLum(ref themeColor, percentage9);
			}
			return true;
		case "lumMod":
			if (!string.IsNullOrEmpty(value))
			{
				double percentage7 = GetPercentage(value);
				WordColor.ConvertbyLumMod(ref themeColor, percentage7);
			}
			return true;
		case "lumOff":
			if (value != null)
			{
				double percentage5 = GetPercentage(value);
				WordColor.ConvertbyLumOffset(ref themeColor, percentage5);
			}
			return true;
		case "comp":
			themeColor = WordColor.ComplementColor(themeColor);
			return true;
		case "gamma":
			themeColor = WordColor.GammaColor(themeColor);
			return true;
		case "gray":
			themeColor = WordColor.GrayColor(themeColor);
			return true;
		case "invGamma":
			themeColor = WordColor.InverseGammaColor(themeColor);
			return true;
		case "inv":
			themeColor = WordColor.InverseColor(themeColor);
			return true;
		case "tint":
			if (!string.IsNullOrEmpty(value))
			{
				double tint = GetPercentage(value) / 100.0;
				themeColor = WordColor.ConvertColorByTint(themeColor, tint);
			}
			return true;
		case "shade":
			if (!string.IsNullOrEmpty(value))
			{
				double shade = GetPercentage(value) / 100.0;
				themeColor = WordColor.ConvertColorByShade(themeColor, shade);
			}
			return true;
		default:
			return false;
		}
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (m_TextBody != null)
		{
			m_TextBody.CloneRelationsTo(doc, nextOwner);
		}
		base.CloneRelationsTo(doc, nextOwner);
	}

	public void ApplyCharacterFormat(WCharacterFormat charFormat)
	{
		if (charFormat != null)
		{
			SetParagraphItemCharacterFormat(charFormat);
		}
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
				base.Width = Layouter.GetLeftMargin(section) * (base.RelativeWidth / 100f);
				break;
			case HorizontalOrigin.RightMargin:
			case HorizontalOrigin.OutsideMargin:
				base.Width = Layouter.GetRightMargin(section) * (base.RelativeWidth / 100f);
				break;
			default:
				base.Width = (section.PageSetup.PageSize.Width - Layouter.GetLeftMargin(section) - Layouter.GetRightMargin(section)) * (base.RelativeWidth / 100f);
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
		if (IsHorizontalRule && base.WidthScale != 0f)
		{
			Entity ownerSection = GetOwnerSection(this);
			if (ownerSection is WSection)
			{
				return new SizeF((ownerSection as WSection).PageSetup.ClientWidth * (base.WidthScale / 100f), base.Height);
			}
		}
		float width = base.Width;
		float height = base.Height;
		if (TextFrame.WidthRelativePercent != 0f)
		{
			width = GetWidthRelativeToPercent(isDocToPdf: true);
		}
		if (TextFrame.HeightRelativePercent != 0f)
		{
			height = GetHeightRelativeToPercent(isDocToPdf: true);
		}
		return new SizeF(width, height);
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
		if (base.Owner is InlineContentControl || base.Owner is XmlParagraphItem)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		if (wParagraph.IsInCell && ((IWidget)wParagraph).LayoutInfo.IsClipped)
		{
			m_layoutInfo.IsClipped = true;
		}
		if (Entity.IsVerticalTextDirection(TextFrame.TextDirection))
		{
			m_layoutInfo.IsVerticalText = true;
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
		if (m_TextBody != null)
		{
			m_TextBody.InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				return;
			}
		}
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal void UpdateShapeBoundsToLayoutTextBody(ref RectangleF layoutRect, InternalMargin internalMargin, LayoutedWidget ltWidget)
	{
		Shape shape = ltWidget.Widget as Shape;
		layoutRect.Height -= layoutRect.Y;
		layoutRect.Y += ltWidget.Bounds.Y;
		layoutRect.Width -= layoutRect.X;
		layoutRect.X += ltWidget.Bounds.X;
		if (shape.IsNoNeedToConsiderLineWidth())
		{
			layoutRect.Y += internalMargin.Top;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom;
		}
		else
		{
			layoutRect.Y += internalMargin.Top + shape.LineFormat.Weight / 2f;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom + shape.LineFormat.Weight;
		}
	}

	internal bool IsNoNeedToConsiderLineWidth()
	{
		if (!LineFormat.Line)
		{
			return Is2007Shape;
		}
		return false;
	}

	internal Dictionary<string, string> GetGuideList()
	{
		return m_guideList ?? (m_guideList = new Dictionary<string, string>());
	}

	internal Dictionary<string, string> GetAvList()
	{
		return m_avList ?? (m_avList = new Dictionary<string, string>());
	}

	internal void SetGuideList(Dictionary<string, string> value)
	{
		m_guideList = value;
	}

	internal void SetAvList(Dictionary<string, string> value)
	{
		m_avList = value;
	}

	internal override void Close()
	{
		if (m_TextBody != null)
		{
			m_TextBody.Close();
			m_TextBody = null;
		}
		if (m_FillFormat != null)
		{
			m_FillFormat.Close();
			m_FillFormat = null;
		}
		if (m_LineFormat != null)
		{
			m_LineFormat.Close();
			m_LineFormat = null;
		}
		if (m_effectList != null)
		{
			m_effectList.Clear();
			m_effectList = null;
		}
		if (m_styleProps != null)
		{
			m_styleProps.Clear();
			m_styleProps = null;
		}
		if (m_relations != null)
		{
			m_relations.Clear();
			m_relations = null;
		}
		if (m_shapeStyleItems != null)
		{
			m_shapeStyleItems.Clear();
			m_shapeStyleItems = null;
		}
		if (m_vmlPathPoints != null)
		{
			foreach (Path2D vmlPathPoint in m_vmlPathPoints)
			{
				vmlPathPoint.Close();
			}
			m_vmlPathPoints.Clear();
			m_vmlPathPoints = null;
		}
		if (m_guideList != null)
		{
			m_guideList.Clear();
			m_guideList = null;
		}
		if (m_avList != null)
		{
			m_avList.Clear();
			m_avList = null;
		}
		if (Path2DList != null)
		{
			foreach (Path2D path2D in m_path2DList)
			{
				path2D.Close();
			}
			m_path2DList.Clear();
			m_path2DList = null;
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
