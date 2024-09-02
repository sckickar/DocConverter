using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class ChildShape : ShapeCommon, IEntity, ILeafWidget, IWidget
{
	private List<string> m_styleProps;

	internal List<Stream> m_pictureProps;

	private double m_arcSize;

	private string m_Adjustments;

	internal byte m_bFlags1 = 16;

	private float m_rotation;

	internal bool? flipH;

	internal bool? flipV;

	private float m_rotationToRender;

	private List<EffectFormat> m_effectList;

	private byte m_bFlags;

	private Dictionary<string, DictionaryEntry> m_relations;

	private Dictionary<string, ImageRecord> m_imageRelations;

	private Dictionary<int, object> m_propertiesHash;

	internal const byte LineFromXPositionKey = 0;

	internal const byte LineFromYPositionKey = 1;

	internal const byte LineToXPositionKey = 2;

	internal const byte LineToYPositionKey = 3;

	private float x_value;

	private float y_value;

	private float m_horzPos;

	private float m_verPos;

	private string m_type;

	private WTextBody m_textBody;

	private TextFrame m_textFrame;

	private LineFormat m_lineFormat;

	private FillFormat m_fillFormat;

	private WChart m_chart;

	private XmlParagraphItem m_xmlParagraphItem;

	private float m_leftPosition;

	private float m_topPosition;

	private AutoShapeType m_autoShapeType;

	internal Dictionary<string, string> m_shapeGuide;

	private List<ShapeStyleReference> m_shapeStyleItems;

	private Color m_fontRefColor = Color.Empty;

	internal Dictionary<string, Stream> m_docx2007Props;

	private float m_lineFromXPosition;

	private float m_lineFromYPosition;

	private float m_lineToXPosition;

	private float m_lineToYPosition;

	internal ImageRecord m_imageRecord;

	private EntityType m_elementType;

	private bool m_visible = true;

	internal bool skipPositionUpdate;

	private RectangleF m_textLayoutingBounds;

	private List<Path2D> m_vmlPathPoints;

	internal bool m_isVMLPathUpdated;

	private Dictionary<string, string> m_guideList;

	private Dictionary<string, string> m_avList;

	private List<Path2D> m_path2DList;

	private string m_oPictureHRef;

	private byte[] m_svgImageData;

	private string m_svgExternalLinkName = string.Empty;

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

	internal string OPictureHRef
	{
		get
		{
			return m_oPictureHRef;
		}
		set
		{
			m_oPictureHRef = value;
		}
	}

	internal byte[] SvgData
	{
		get
		{
			return m_svgImageData;
		}
		set
		{
			m_svgImageData = value;
		}
	}

	internal string SvgExternalLink
	{
		get
		{
			return m_svgExternalLinkName;
		}
		set
		{
			m_svgExternalLinkName = value;
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

	internal bool FlipHorizantal
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

	internal bool FlipVertical
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

	internal float X
	{
		get
		{
			return x_value;
		}
		set
		{
			x_value = value;
		}
	}

	internal float Y
	{
		get
		{
			return y_value;
		}
		set
		{
			y_value = value;
		}
	}

	internal float HorizontalPosition
	{
		get
		{
			return m_horzPos;
		}
		set
		{
			m_horzPos = value;
		}
	}

	internal float VerticalPosition
	{
		get
		{
			return m_verPos;
		}
		set
		{
			m_verPos = value;
		}
	}

	internal string Type
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

	internal EntityType ElementType
	{
		get
		{
			return m_elementType;
		}
		set
		{
			m_elementType = value;
		}
	}

	internal bool HasTextBody
	{
		get
		{
			if (m_elementType == EntityType.AutoShape || m_elementType == EntityType.Shape || m_elementType == EntityType.TextBox)
			{
				return m_textBody != null;
			}
			return false;
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

	internal new Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	internal float LineFromXPosition
	{
		get
		{
			if (HasKey(0))
			{
				return (float)PropertiesHash[0];
			}
			return m_lineFromXPosition;
		}
		set
		{
			m_lineFromXPosition = value;
			SetKeyValue(0, value);
		}
	}

	internal float LineFromYPosition
	{
		get
		{
			if (HasKey(1))
			{
				return (float)PropertiesHash[1];
			}
			return m_lineFromYPosition;
		}
		set
		{
			m_lineFromYPosition = value;
			SetKeyValue(1, value);
		}
	}

	internal float LineToXPosition
	{
		get
		{
			if (HasKey(2))
			{
				return (float)PropertiesHash[2];
			}
			return m_lineToXPosition;
		}
		set
		{
			m_lineToXPosition = value;
			SetKeyValue(2, value);
		}
	}

	internal float LineToYPosition
	{
		get
		{
			if (HasKey(3))
			{
				return (float)PropertiesHash[3];
			}
			return m_lineToYPosition;
		}
		set
		{
			m_lineToYPosition = value;
			SetKeyValue(3, value);
		}
	}

	internal WTextBody TextBody
	{
		get
		{
			if (m_textBody == null)
			{
				m_textBody = new WTextBody(Document, this);
			}
			return m_textBody;
		}
		set
		{
			m_textBody = value;
		}
	}

	internal TextFrame TextFrame
	{
		get
		{
			if (m_textFrame == null)
			{
				m_textFrame = new TextFrame(this);
			}
			return m_textFrame;
		}
		set
		{
			m_textFrame = value;
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

	internal WChart Chart
	{
		get
		{
			if (Document.IsOpening)
			{
				return m_chart = new WChart(m_doc);
			}
			return m_chart;
		}
		set
		{
			m_chart = value;
		}
	}

	internal XmlParagraphItem XmlParagraphItem
	{
		get
		{
			return m_xmlParagraphItem;
		}
		set
		{
			m_xmlParagraphItem = value;
		}
	}

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

	internal bool IsPicture
	{
		get
		{
			if (m_imageRecord != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			m_visible = value;
		}
	}

	internal byte[] ImageBytes => m_imageRecord.ImageBytes;

	internal ImageRecord ImageRecord => m_imageRecord;

	internal new WordDocument Document => m_doc;

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

	internal bool IsTextBoxShape
	{
		get
		{
			return (m_bFlags1 & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xBFu) | ((value ? 1u : 0u) << 6));
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

	internal float Rotation
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

	internal List<Stream> DocxPictureVisualProps
	{
		get
		{
			if (m_pictureProps == null)
			{
				m_pictureProps = new List<Stream>();
			}
			return m_pictureProps;
		}
		set
		{
			m_pictureProps = value;
		}
	}

	internal bool LayoutInCell
	{
		get
		{
			return (m_bFlags1 & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool Is2007Shape
	{
		get
		{
			return (m_bFlags1 & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xDFu) | ((value ? 1u : 0u) << 5));
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

	public override EntityType EntityType => EntityType.ChildShape;

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

	internal float RotationToRender
	{
		get
		{
			return m_rotationToRender;
		}
		set
		{
			m_rotationToRender = value;
		}
	}

	internal bool FlipHorizantalToRender
	{
		get
		{
			return (m_bFlags1 & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool FlipVerticalToRender
	{
		get
		{
			return (m_bFlags1 & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	protected new object this[int key]
	{
		get
		{
			return key;
		}
		set
		{
			PropertiesHash[key] = value;
		}
	}

	internal new void SetKeyValue(int propKey, object value)
	{
		this[propKey] = value;
	}

	internal new bool HasKey(int Key)
	{
		if (m_propertiesHash != null && m_propertiesHash.ContainsKey(Key))
		{
			return true;
		}
		return false;
	}

	internal ChildShape(IWordDocument doc, AutoShapeType autoShapeType)
		: this((WordDocument)doc)
	{
		m_autoShapeType = autoShapeType;
	}

	internal ChildShape(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(Document, this);
		FillFormat.Color = Color.White;
		FillFormat.Fill = true;
		FillFormat.FillType = FillType.FillSolid;
		LineFormat.Color = Color.Black;
		LineFormat.DashStyle = LineDashing.Solid;
		LineFormat.Line = true;
		LineFormat.Style = LineStyle.Single;
		LineFormat.Transparency = 0f;
		LineFormat.m_Weight = 1f;
		TextFrame.TextVerticalAlignment = VerticalAlignment.Top;
	}

	internal override void Detach()
	{
		base.Detach();
		if (!base.DeepDetached)
		{
			Document.FloatingItems.Remove(this);
		}
	}

	internal override void AttachToDocument()
	{
		Document.FloatingItems.Add(this);
		base.IsCloned = false;
		if (m_textBody != null)
		{
			m_textBody.AttachToDocument();
		}
	}

	protected override object CloneImpl()
	{
		ChildShape childShape = (ChildShape)base.CloneImpl();
		childShape.IsCloned = true;
		if (ElementType == EntityType.Shape || ElementType == EntityType.AutoShape)
		{
			if (m_textBody != null)
			{
				childShape.m_textBody = m_textBody.Clone() as WTextBody;
				childShape.m_textBody.SetOwner(childShape);
			}
			if (m_shapeGuide != null && m_shapeGuide.Count > 0)
			{
				Document.CloneProperties(m_shapeGuide, ref childShape.m_shapeGuide);
			}
		}
		else if (ElementType == EntityType.TextBox)
		{
			if (m_textBody != null)
			{
				childShape.m_textBody = m_textBody.Clone() as WTextBody;
				childShape.m_textBody.SetOwner(childShape);
			}
		}
		else if (ElementType == EntityType.Chart && Chart != null)
		{
			childShape.Chart = Chart.Clone() as WChart;
		}
		else if (IsPicture)
		{
			if (m_textBody != null)
			{
				childShape.m_textBody = m_textBody.Clone() as WTextBody;
				childShape.m_textBody.SetOwner(childShape);
			}
			childShape.m_imageRecord = new ImageRecord(Document, childShape.ImageRecord);
			if (DocxPictureVisualProps != null && DocxPictureVisualProps.Count > 0)
			{
				childShape.DocxPictureVisualProps = new List<Stream>();
				foreach (Stream docxPictureVisualProp in DocxPictureVisualProps)
				{
					childShape.DocxPictureVisualProps.Add(Document.CloneStream(docxPictureVisualProp));
				}
			}
			Document.HasPicture = true;
		}
		else if (ElementType == EntityType.XmlParaItem && XmlParagraphItem != null)
		{
			childShape.XmlParagraphItem = XmlParagraphItem.Clone() as XmlParagraphItem;
		}
		childShape.CloneShapeFormat(this);
		if (TextFrame != null && TextFrame.InternalMargin != null)
		{
			childShape.TextFrame.m_intMargin = TextFrame.InternalMargin.Clone();
		}
		if (m_docx2007Props != null && m_docx2007Props.Count > 0)
		{
			childShape.Document.CloneProperties(Docx2007Props, ref childShape.m_docx2007Props);
		}
		childShape.ShapeID = 0L;
		return childShape;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (m_textBody != null)
		{
			m_textBody.CloneRelationsTo(doc, nextOwner);
		}
		base.CloneRelationsTo(doc, nextOwner);
		if (IsPicture && m_imageRecord != null)
		{
			Size size = m_imageRecord.Size;
			DocGen.DocIO.DLS.Entities.ImageFormat imageFormat = m_imageRecord.ImageFormat;
			int length = m_imageRecord.Length;
			if (m_imageRecord.IsMetafile)
			{
				m_imageRecord = doc.Images.LoadMetaFileImage(m_imageRecord.m_imageBytes, isCompressed: true);
			}
			else
			{
				m_imageRecord = doc.Images.LoadImage(m_imageRecord.ImageBytes);
			}
			m_imageRecord.Size = size;
			m_imageRecord.ImageFormat = imageFormat;
			m_imageRecord.Length = length;
		}
		else if (ElementType == EntityType.XmlParaItem && XmlParagraphItem != null)
		{
			XmlParagraphItem.CloneRelationsTo(doc, nextOwner);
		}
		base.IsCloned = false;
	}

	internal Dictionary<string, string> GetGuideList()
	{
		return m_guideList ?? (m_guideList = new Dictionary<string, string>());
	}

	internal Dictionary<string, string> GetAvList()
	{
		return m_avList ?? (m_avList = new Dictionary<string, string>());
	}

	internal void CloneShapeFormat(ChildShape childShape)
	{
		bool flag = Document != null && Document.DocHasThemes;
		if (childShape.FillFormat != null && (childShape.IsFillStyleInline || (childShape.Is2007Shape && !childShape.FillFormat.IsDefaultFill)))
		{
			FillFormat = childShape.FillFormat.Clone();
			IsFillStyleInline = true;
		}
		else if (flag && childShape.ShapeStyleReferences != null && childShape.ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex = childShape.ShapeStyleReferences[1].StyleRefIndex;
			if (styleRefIndex > 0 && Document.Themes.FmtScheme.FillFormats.Count > styleRefIndex)
			{
				uint opacity = uint.MaxValue;
				FillFormat fillFormat = childShape.Document.Themes.FmtScheme.FillFormats[styleRefIndex - 1];
				FillFormat = fillFormat.Clone();
				if (fillFormat.FillType == FillType.FillSolid)
				{
					FillFormat.Color = childShape.ShapeStyleReferences[1].StyleRefColor;
					FillFormat.Transparency = childShape.ShapeStyleReferences[1].StyleRefOpacity;
				}
				else if (fillFormat.FillType == FillType.FillGradient)
				{
					for (int i = 0; i < fillFormat.GradientFill.GradientStops.Count; i++)
					{
						FillFormat.GradientFill.GradientStops[i].Color = StyleColorTransform(fillFormat.GradientFill.GradientStops[i].FillSchemeColorTransforms, childShape.ShapeStyleReferences[1].StyleRefColor, ref opacity);
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
					FillFormat.ForeColor = StyleColorTransform(list, childShape.ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
					FillFormat.Color = StyleColorTransform(list2, childShape.ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
				}
				IsFillStyleInline = true;
			}
		}
		if (childShape.IsLineStyleInline && childShape.LineFormat != null)
		{
			LineFormat = childShape.LineFormat.Clone();
			IsLineStyleInline = true;
		}
		else if (flag && childShape.ShapeStyleReferences != null && childShape.ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex2 = childShape.ShapeStyleReferences[0].StyleRefIndex;
			if (styleRefIndex2 > 0 && Document.Themes.FmtScheme.LnStyleScheme.Count > styleRefIndex2)
			{
				uint opacity2 = uint.MaxValue;
				LineFormat lineFormat = childShape.Document.Themes.FmtScheme.LnStyleScheme[styleRefIndex2 - 1];
				LineFormat = childShape.Document.Themes.FmtScheme.LnStyleScheme[styleRefIndex2 - 1].Clone();
				if (lineFormat.LineFormatType == LineFormatType.Solid)
				{
					LineFormat.Color = childShape.ShapeStyleReferences[0].StyleRefColor;
					LineFormat.Transparency = childShape.ShapeStyleReferences[0].StyleRefOpacity;
				}
				else if (lineFormat.LineFormatType == LineFormatType.Gradient)
				{
					for (int k = 0; k < lineFormat.GradientFill.GradientStops.Count; k++)
					{
						FillFormat.GradientFill.GradientStops[k].Color = StyleColorTransform(lineFormat.GradientFill.GradientStops[k].FillSchemeColorTransforms, childShape.ShapeStyleReferences[1].StyleRefColor, ref opacity2);
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
					FillFormat.ForeColor = StyleColorTransform(list3, childShape.ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
					FillFormat.Color = StyleColorTransform(list4, childShape.ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
				}
				IsLineStyleInline = true;
			}
		}
		if (childShape.EffectList == null)
		{
			return;
		}
		List<EffectFormat> list5 = new List<EffectFormat>();
		for (int m = 0; m < childShape.EffectList.Count; m++)
		{
			EffectFormat effectFormat = new EffectFormat(this);
			if (childShape.EffectList[m].IsEffectListItem)
			{
				if (childShape.EffectList[m].IsShadowEffect && childShape.EffectList[m].ShadowFormat != null)
				{
					effectFormat.ShadowFormat = childShape.EffectList[m].ShadowFormat.Clone();
					effectFormat.IsShadowEffect = true;
				}
				else if (childShape.EffectList[m].IsGlowEffect && childShape.EffectList[m].GlowFormat != null)
				{
					effectFormat.GlowFormat = childShape.EffectList[m].GlowFormat.Clone();
					effectFormat.IsGlowEffect = true;
				}
				else if (childShape.EffectList[m].IsReflection && childShape.EffectList[m].ReflectionFormat != null)
				{
					effectFormat.ReflectionFormat = childShape.EffectList[m].ReflectionFormat.Clone();
					effectFormat.IsReflection = true;
				}
				else if (childShape.EffectList[m].IsSoftEdge)
				{
					effectFormat.NoSoftEdges = childShape.EffectList[m].NoSoftEdges;
					effectFormat.SoftEdgeRadius = childShape.EffectList[m].SoftEdgeRadius;
				}
				list5.Add(effectFormat);
				list5[m].IsEffectListItem = true;
			}
			else if ((childShape.EffectList[m].IsShapeProperties || childShape.EffectList[m].IsSceneProperties) && childShape.EffectList[m].ThreeDFormat != null)
			{
				effectFormat.ThreeDFormat = childShape.EffectList[m].ThreeDFormat.Clone();
				effectFormat.IsSceneProperties = childShape.EffectList[m].IsSceneProperties;
				effectFormat.IsShapeProperties = childShape.EffectList[m].IsShapeProperties;
				list5.Add(effectFormat);
			}
		}
		IsEffectStyleInline = true;
		EffectList.Clear();
		EffectList = list5;
		if (m_guideList != null)
		{
			Document.CloneProperties(m_guideList, ref childShape.m_guideList);
		}
		if (m_avList != null)
		{
			Document.CloneProperties(m_avList, ref childShape.m_avList);
		}
		if (Path2DList != null)
		{
			List<Path2D> list6 = new List<Path2D>();
			for (int n = 0; n < Path2DList.Count; n++)
			{
				Path2D item = Path2DList[n].Clone();
				list6.Add(item);
			}
			childShape.Path2DList = list6;
		}
	}

	internal void CloneShapeFormat(Shape shape)
	{
		bool flag = Document != null && Document.DocHasThemes;
		if (shape.FillFormat != null && (shape.IsFillStyleInline || (shape.Is2007Shape && !shape.FillFormat.IsDefaultFill)))
		{
			FillFormat = shape.FillFormat.Clone();
			IsFillStyleInline = true;
		}
		else if (flag && shape.ShapeStyleReferences != null && shape.ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex = shape.ShapeStyleReferences[1].StyleRefIndex;
			if (styleRefIndex > 0 && Document.Themes.FmtScheme.FillFormats.Count > styleRefIndex)
			{
				uint opacity = uint.MaxValue;
				FillFormat fillFormat = shape.Document.Themes.FmtScheme.FillFormats[styleRefIndex - 1];
				FillFormat = fillFormat.Clone();
				if (fillFormat.FillType == FillType.FillSolid)
				{
					FillFormat.Color = shape.ShapeStyleReferences[1].StyleRefColor;
					FillFormat.Transparency = shape.ShapeStyleReferences[1].StyleRefOpacity;
				}
				else if (fillFormat.FillType == FillType.FillGradient)
				{
					for (int i = 0; i < fillFormat.GradientFill.GradientStops.Count; i++)
					{
						FillFormat.GradientFill.GradientStops[i].Color = StyleColorTransform(fillFormat.GradientFill.GradientStops[i].FillSchemeColorTransforms, shape.ShapeStyleReferences[1].StyleRefColor, ref opacity);
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
					FillFormat.ForeColor = StyleColorTransform(list, shape.ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
					FillFormat.Color = StyleColorTransform(list2, shape.ShapeStyleReferences[1].StyleRefColor, ref opacity);
					opacity = uint.MaxValue;
				}
				IsFillStyleInline = true;
			}
		}
		if (shape.IsLineStyleInline && shape.LineFormat != null)
		{
			LineFormat = shape.LineFormat.Clone();
			IsLineStyleInline = true;
		}
		else if (flag && shape.ShapeStyleReferences != null && shape.ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex2 = shape.ShapeStyleReferences[0].StyleRefIndex;
			if (styleRefIndex2 > 0 && Document.Themes.FmtScheme.LnStyleScheme.Count > styleRefIndex2)
			{
				uint opacity2 = uint.MaxValue;
				LineFormat lineFormat = shape.Document.Themes.FmtScheme.LnStyleScheme[styleRefIndex2 - 1];
				LineFormat = shape.Document.Themes.FmtScheme.LnStyleScheme[styleRefIndex2 - 1].Clone();
				if (lineFormat.LineFormatType == LineFormatType.Solid)
				{
					LineFormat.Color = shape.ShapeStyleReferences[0].StyleRefColor;
					LineFormat.Transparency = shape.ShapeStyleReferences[0].StyleRefOpacity;
				}
				else if (lineFormat.LineFormatType == LineFormatType.Gradient)
				{
					for (int k = 0; k < lineFormat.GradientFill.GradientStops.Count; k++)
					{
						FillFormat.GradientFill.GradientStops[k].Color = StyleColorTransform(lineFormat.GradientFill.GradientStops[k].FillSchemeColorTransforms, shape.ShapeStyleReferences[1].StyleRefColor, ref opacity2);
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
					FillFormat.ForeColor = StyleColorTransform(list3, shape.ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
					FillFormat.Color = StyleColorTransform(list4, shape.ShapeStyleReferences[1].StyleRefColor, ref opacity2);
					opacity2 = uint.MaxValue;
				}
				IsLineStyleInline = true;
			}
		}
		if (shape.EffectList == null)
		{
			return;
		}
		List<EffectFormat> list5 = new List<EffectFormat>();
		for (int m = 0; m < shape.EffectList.Count; m++)
		{
			EffectFormat effectFormat = new EffectFormat(this);
			if (shape.EffectList[m].IsEffectListItem)
			{
				if (shape.EffectList[m].IsShadowEffect && shape.EffectList[m].ShadowFormat != null)
				{
					effectFormat.ShadowFormat = shape.EffectList[m].ShadowFormat.Clone();
					effectFormat.IsShadowEffect = true;
				}
				else if (shape.EffectList[m].IsGlowEffect && shape.EffectList[m].GlowFormat != null)
				{
					effectFormat.GlowFormat = shape.EffectList[m].GlowFormat.Clone();
					effectFormat.IsGlowEffect = true;
				}
				else if (shape.EffectList[m].IsReflection && shape.EffectList[m].ReflectionFormat != null)
				{
					effectFormat.ReflectionFormat = shape.EffectList[m].ReflectionFormat.Clone();
					effectFormat.IsReflection = true;
				}
				else if (shape.EffectList[m].IsSoftEdge)
				{
					effectFormat.NoSoftEdges = shape.EffectList[m].NoSoftEdges;
					effectFormat.SoftEdgeRadius = shape.EffectList[m].SoftEdgeRadius;
				}
				list5.Add(effectFormat);
				list5[m].IsEffectListItem = true;
			}
			else if ((shape.EffectList[m].IsShapeProperties || shape.EffectList[m].IsSceneProperties) && shape.EffectList[m].ThreeDFormat != null)
			{
				effectFormat.ThreeDFormat = shape.EffectList[m].ThreeDFormat.Clone();
				effectFormat.IsSceneProperties = shape.EffectList[m].IsSceneProperties;
				effectFormat.IsShapeProperties = shape.EffectList[m].IsShapeProperties;
				list5.Add(effectFormat);
			}
		}
		IsEffectStyleInline = true;
		EffectList.Clear();
		EffectList = list5;
	}

	internal GroupShape GetOwnerGroupShape()
	{
		Entity owner = base.Owner;
		while (!(owner is GroupShape))
		{
			owner = owner.Owner;
		}
		return owner as GroupShape;
	}

	internal void CloneChildShapeFormatToShape(Shape shape)
	{
		bool flag = Document != null && Document.DocHasThemes;
		if (FillFormat != null && (IsFillStyleInline || (Is2007Shape && !FillFormat.IsDefaultFill)))
		{
			shape.FillFormat = FillFormat.Clone();
			shape.IsFillStyleInline = true;
		}
		else if ((base.Owner is ChildGroupShape || base.Owner is GroupShape) && FillFormat.IsGrpFill)
		{
			Entity owner = base.Owner;
			while (!(owner is GroupShape) && !IsNeedToGetFillStyleFromChildGroupShape(owner))
			{
				if (owner is ChildGroupShape)
				{
					owner = owner.Owner;
				}
			}
			if (owner is GroupShape && (owner as GroupShape).FillFormat != null)
			{
				shape.FillFormat = (owner as GroupShape).FillFormat.Clone();
				shape.IsFillStyleInline = true;
			}
			else if (owner is ChildGroupShape && (owner as ChildGroupShape).FillFormat != null)
			{
				shape.FillFormat = (owner as ChildGroupShape).FillFormat.Clone();
				shape.IsFillStyleInline = true;
			}
		}
		else if (flag && ShapeStyleReferences != null && ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex = ShapeStyleReferences[1].StyleRefIndex;
			if (styleRefIndex > 0 && Document.Themes.FmtScheme.FillFormats.Count > styleRefIndex)
			{
				uint opacity = uint.MaxValue;
				FillFormat fillFormat = shape.Document.Themes.FmtScheme.FillFormats[styleRefIndex - 1];
				shape.FillFormat = fillFormat.Clone();
				if (base.DocxProps.ContainsKey("grpFill"))
				{
					ApplyOwnerGroupShapeFill(shape);
				}
				else if (fillFormat.FillType == FillType.FillSolid)
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
		if (LineFormat != null)
		{
			shape.LineFormat = LineFormat.Clone();
			shape.IsLineStyleInline = true;
		}
		else if (flag && ShapeStyleReferences != null && ShapeStyleReferences.Count > 0)
		{
			int styleRefIndex2 = ShapeStyleReferences[0].StyleRefIndex;
			if (styleRefIndex2 > 0 && Document.Themes.FmtScheme.LnStyleScheme.Count > styleRefIndex2)
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

	internal bool IsNeedToGetFillStyleFromChildGroupShape(Entity entity)
	{
		if (entity is ChildGroupShape && (entity as ChildGroupShape).FillFormat != null && !(entity as ChildGroupShape).FillFormat.IsGrpFill)
		{
			return (entity as ChildGroupShape).IsFillStyleInline;
		}
		return false;
	}

	internal void ApplyOwnerGroupShapeFill(Entity entity)
	{
		GroupShape groupShape = base.Owner as GroupShape;
		ChildGroupShape childGroupShape = base.Owner as ChildGroupShape;
		byte[] array = ((groupShape != null && groupShape.FillFormat.FillType == FillType.FillPicture) ? groupShape.FillFormat.ImageRecord.ImageBytes : ((childGroupShape != null && childGroupShape.FillFormat.FillType == FillType.FillPicture) ? childGroupShape.FillFormat.ImageRecord.ImageBytes : null));
		float transparency = groupShape?.FillFormat.BlipFormat.Transparency ?? childGroupShape?.FillFormat.BlipFormat.Transparency ?? 0f;
		if (array != null)
		{
			if (entity is WTextBox)
			{
				(entity as WTextBox).TextBoxFormat.FillEfects.Type = BackgroundType.Picture;
				(entity as WTextBox).TextBoxFormat.FillEfects.ImageRecord = new ImageRecord(Document, array);
				(entity as WTextBox).Shape.FillFormat.BlipFormat.Transparency = transparency;
			}
			else if (entity is Shape)
			{
				(entity as Shape).FillFormat.FillType = FillType.FillPicture;
				(entity as Shape).FillFormat.ImageRecord = new ImageRecord(Document, array);
				(entity as Shape).FillFormat.BlipFormat.Transparency = transparency;
			}
			else if (entity is WPicture)
			{
				(entity as WPicture).PictureShape.FillFormat.FillType = FillType.FillPicture;
				(entity as WPicture).PictureShape.FillFormat.ImageRecord = new ImageRecord(Document, array);
				(entity as WPicture).PictureShape.FillFormat.BlipFormat.Transparency = transparency;
			}
		}
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	internal Color StyleColorTransform(List<DictionaryEntry> fillTransformation, Color themeColor, ref uint opacity)
	{
		bool flag = false;
		foreach (DictionaryEntry item in fillTransformation)
		{
			string empty = string.Empty;
			switch ((!item.Key.ToString().StartsWith("fgClr") && !item.Key.ToString().StartsWith("bgClr")) ? item.Key.ToString() : item.Key.ToString().Substring(5))
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

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return new SizeF(base.Width, base.Height);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		if (Entity.IsVerticalTextDirection(TextFrame.TextDirection))
		{
			m_layoutInfo.IsVerticalText = true;
		}
		if (!Visible)
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_textBody != null)
		{
			m_textBody.InitLayoutInfo(entity, ref isLastTOCEntry);
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

	internal override void Close()
	{
		if (m_textBody != null)
		{
			m_textBody.Close();
			m_textBody = null;
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
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_effectList != null)
		{
			for (int i = 0; i < m_effectList.Count; i++)
			{
				EffectFormat effectFormat = m_effectList[i];
				if (effectFormat != null)
				{
					effectFormat.Close();
					effectFormat = null;
				}
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
		if (m_shapeGuide != null)
		{
			m_shapeGuide.Clear();
			m_shapeGuide = null;
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

	internal bool Compare(ChildShape childShape)
	{
		if (base.IsInsertRevision != childShape.IsInsertRevision || base.IsDeleteRevision != childShape.IsDeleteRevision || IsFillStyleInline != childShape.IsFillStyleInline || FlipHorizantal != childShape.FlipHorizantal || FlipVertical != childShape.FlipVertical || IsScenePropertiesInline != childShape.IsScenePropertiesInline || IsShapePropertiesInline != childShape.IsShapePropertiesInline || IsLineStyleInline != childShape.IsLineStyleInline || IsPicture != childShape.IsPicture || IsHorizontalRule != childShape.IsHorizontalRule || LayoutInCell != childShape.LayoutInCell || AutoShapeType != childShape.AutoShapeType || base.ShapeID != childShape.ShapeID || base.Height != childShape.Height || base.Width != childShape.Width || base.HeightScale != childShape.HeightScale || base.WidthScale != childShape.WidthScale || base.AlternativeText != childShape.AlternativeText || base.Name != childShape.Name || base.Title != childShape.Title || base.Path != childShape.Path || base.CoordinateSize != childShape.CoordinateSize || base.CoordinateXOrigin != childShape.CoordinateXOrigin || base.CoordinateYOrigin != childShape.CoordinateYOrigin || OPictureHRef != childShape.OPictureHRef || SvgExternalLink != childShape.SvgExternalLink || X != childShape.X || Y != childShape.Y || Type != childShape.Type || LeftMargin != childShape.LeftMargin || TopMargin != childShape.TopMargin || LineFromXPosition != childShape.LineFromXPosition || LineFromYPosition != childShape.LineFromYPosition || LineToXPosition != childShape.LineToXPosition || LineToYPosition != childShape.LineToYPosition || Adjustments != childShape.Adjustments || Rotation != childShape.Rotation || ArcSize != childShape.ArcSize)
		{
			return false;
		}
		if ((Path2DList == null && childShape.Path2DList != null) || (Path2DList != null && childShape.Path2DList == null) || (VMLPathPoints == null && childShape.VMLPathPoints != null) || (VMLPathPoints != null && childShape.VMLPathPoints == null) || (ShapeGuide == null && childShape.ShapeGuide != null) || (ShapeGuide != null && childShape.ShapeGuide == null) || (ShapeStyleReferences == null && childShape.ShapeStyleReferences != null) || (ShapeStyleReferences != null && childShape.ShapeStyleReferences == null) || (EffectList == null && childShape.EffectList != null) || (EffectList != null && childShape.EffectList == null) || (Docx2007Props == null && childShape.Docx2007Props != null) || (Docx2007Props != null && childShape.Docx2007Props == null) || (DocxStyleProps == null && childShape.DocxStyleProps != null) || (DocxStyleProps != null && childShape.DocxStyleProps == null) || (DocxPictureVisualProps == null && childShape.DocxPictureVisualProps != null) || (DocxPictureVisualProps != null && childShape.DocxPictureVisualProps == null) || (SvgData == null && childShape.SvgData != null) || (SvgData != null && childShape.SvgData == null) || (TextBody == null && childShape.TextBody != null) || (TextBody != null && childShape.TextBody == null) || (TextFrame == null && childShape.TextFrame != null) || (TextFrame != null && childShape.TextFrame == null) || (LineFormat == null && childShape.LineFormat != null) || (LineFormat != null && childShape.LineFormat == null) || (FillFormat == null && childShape.FillFormat != null) || (FillFormat != null && childShape.FillFormat == null) || (Chart == null && childShape.Chart != null) || (Chart != null && childShape.Chart == null) || (ImageRecord == null && childShape.ImageRecord != null) || (ImageRecord != null && childShape.ImageRecord == null) || ElementType != childShape.ElementType)
		{
			return false;
		}
		if (Path2DList != null && childShape.Path2DList != null)
		{
			if (Path2DList.Count != childShape.Path2DList.Count)
			{
				return false;
			}
			for (int i = 0; i < Path2DList.Count; i++)
			{
				if (!Path2DList[i].Compare(childShape.Path2DList[i]))
				{
					return false;
				}
			}
		}
		if (VMLPathPoints != null && childShape.VMLPathPoints != null)
		{
			if (VMLPathPoints.Count != childShape.VMLPathPoints.Count)
			{
				return false;
			}
			for (int j = 0; j < VMLPathPoints.Count; j++)
			{
				if (!VMLPathPoints[j].Compare(childShape.VMLPathPoints[j]))
				{
					return false;
				}
			}
		}
		if (ShapeGuide != null && childShape.ShapeGuide != null)
		{
			if (ShapeGuide.Count != childShape.ShapeGuide.Count)
			{
				return false;
			}
			foreach (string key in ShapeGuide.Keys)
			{
				if (!childShape.ShapeGuide.ContainsKey(key) || !(ShapeGuide[key] == childShape.ShapeGuide[key]))
				{
					return false;
				}
			}
		}
		if (ShapeStyleReferences != null && childShape.ShapeStyleReferences != null)
		{
			if (ShapeStyleReferences.Count != childShape.ShapeStyleReferences.Count)
			{
				return false;
			}
			for (int k = 0; k < ShapeStyleReferences.Count; k++)
			{
				if (!ShapeStyleReferences[k].Compare(childShape.ShapeStyleReferences[k]))
				{
					return false;
				}
			}
		}
		if (EffectList != null && childShape.EffectList != null)
		{
			if (EffectList.Count != childShape.EffectList.Count)
			{
				return false;
			}
			for (int l = 0; l < EffectList.Count; l++)
			{
				if (!EffectList[l].Compare(childShape.EffectList[l]))
				{
					return false;
				}
			}
		}
		if (!Document.Comparison.CompareDocxProps(Docx2007Props, childShape.Docx2007Props))
		{
			return false;
		}
		if (DocxStyleProps != null && childShape.DocxStyleProps != null)
		{
			if (DocxStyleProps.Count != childShape.DocxStyleProps.Count)
			{
				return false;
			}
			for (int m = 0; m < DocxStyleProps.Count; m++)
			{
				if (!DocxStyleProps[m].Equals(childShape.DocxStyleProps[m]))
				{
					return false;
				}
			}
		}
		if (DocxPictureVisualProps != null && childShape.DocxPictureVisualProps != null)
		{
			if (DocxPictureVisualProps.Count != childShape.DocxPictureVisualProps.Count)
			{
				return false;
			}
			for (int n = 0; n < DocxPictureVisualProps.Count; n++)
			{
				if (DocxPictureVisualProps[n].Length != childShape.DocxPictureVisualProps[n].Length)
				{
					return false;
				}
			}
		}
		if (SvgData != null && childShape.SvgData != null && !Comparison.CompareBytes(SvgData, childShape.SvgData))
		{
			return false;
		}
		if (TextFrame != null && !TextFrame.Compare(childShape.TextFrame))
		{
			return false;
		}
		if (LineFormat != null && !LineFormat.Compare(childShape.LineFormat))
		{
			return false;
		}
		if (FillFormat != null && !FillFormat.Compare(childShape.FillFormat))
		{
			return false;
		}
		if (Chart != null && !Chart.Compare(childShape.Chart))
		{
			return false;
		}
		if (ImageRecord != null && childShape.ImageRecord != null)
		{
			if (ImageRecord.comparedImageName != childShape.ImageRecord.comparedImageName)
			{
				return false;
			}
			if (ImageBytes != null && childShape.ImageBytes != null && !Comparison.CompareBytes(ImageBytes, childShape.ImageBytes))
			{
				return false;
			}
		}
		if (ElementType != childShape.ElementType)
		{
			return false;
		}
		switch (ElementType)
		{
		case EntityType.Picture:
		{
			WPicture wPicture = GetOwnerGroupShape().ConvertChildShapeToPicture(this);
			WPicture wPicture2 = GetOwnerGroupShape().ConvertChildShapeToPicture(childShape);
			if (wPicture != null && wPicture2 != null && !wPicture.Compare(wPicture2))
			{
				return false;
			}
			break;
		}
		case EntityType.TextBox:
		{
			WTextBox wTextBox = GetOwnerGroupShape().ConvertChildShapeToTextbox(this);
			WTextBox wTextBox2 = GetOwnerGroupShape().ConvertChildShapeToTextbox(childShape);
			if (wTextBox != null && wTextBox2 != null && !wTextBox.Compare(wTextBox2))
			{
				return false;
			}
			break;
		}
		case EntityType.AutoShape:
		{
			Shape shape = GetOwnerGroupShape().ConvertChildShapeToShape(this);
			Shape shape2 = GetOwnerGroupShape().ConvertChildShapeToShape(childShape);
			if (shape != null && shape2 != null && !shape.Compare(shape2))
			{
				return false;
			}
			break;
		}
		case EntityType.Chart:
			if (Chart != null && childShape.Chart != null && !Chart.Compare(childShape.Chart))
			{
				return false;
			}
			break;
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
		stringBuilder.Append(base.IsInsertRevision ? "1" : "0;");
		stringBuilder.Append(base.IsDeleteRevision ? "1" : "0;");
		stringBuilder.Append(IsFillStyleInline ? "1" : "0;");
		stringBuilder.Append(FlipHorizantal ? "1" : "0;");
		stringBuilder.Append(FlipVertical ? "1" : "0;");
		stringBuilder.Append(IsScenePropertiesInline ? "1" : "0;");
		stringBuilder.Append(IsShapePropertiesInline ? "1" : "0;");
		stringBuilder.Append(IsLineStyleInline ? "1" : "0;");
		stringBuilder.Append(IsPicture ? "1" : "0;");
		stringBuilder.Append(IsHorizontalRule ? "1" : "0;");
		stringBuilder.Append(LayoutInCell ? "1" : "0;");
		stringBuilder.Append((int)AutoShapeType + ";");
		stringBuilder.Append(base.ShapeID + ";");
		stringBuilder.Append(base.Height + ";");
		stringBuilder.Append(base.Width + ";");
		stringBuilder.Append(base.HeightScale + ";");
		stringBuilder.Append(base.WidthScale + ";");
		stringBuilder.Append(base.AlternativeText + ";");
		stringBuilder.Append(base.Name + ";");
		stringBuilder.Append(base.Title + ";");
		stringBuilder.Append(base.Path + ";");
		stringBuilder.Append(base.CoordinateSize + ";");
		stringBuilder.Append(base.CoordinateXOrigin + ";");
		stringBuilder.Append(base.CoordinateYOrigin + ";");
		stringBuilder.Append(OPictureHRef + ";");
		stringBuilder.Append(SvgExternalLink + ";");
		stringBuilder.Append(X + ";");
		stringBuilder.Append(Y + ";");
		stringBuilder.Append(Type + ";");
		stringBuilder.Append(LeftMargin + ";");
		stringBuilder.Append(TopMargin + ";");
		stringBuilder.Append(LineFromXPosition + ";");
		stringBuilder.Append(LineFromYPosition + ";");
		stringBuilder.Append(LineToXPosition + ";");
		stringBuilder.Append(LineToYPosition + ";");
		stringBuilder.Append(Adjustments + ";");
		stringBuilder.Append(Rotation + ";");
		stringBuilder.Append(ArcSize + ";");
		if (Path2DList != null)
		{
			foreach (Path2D path2D in Path2DList)
			{
				stringBuilder.Append(path2D.GetAsString());
			}
		}
		if (VMLPathPoints != null)
		{
			foreach (Path2D vMLPathPoint in VMLPathPoints)
			{
				stringBuilder.Append(vMLPathPoint.GetAsString());
			}
		}
		if (ShapeGuide != null)
		{
			foreach (KeyValuePair<string, string> item in ShapeGuide)
			{
				stringBuilder.Append(item.Value + ";");
			}
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
		if (Docx2007Props != null)
		{
			foreach (KeyValuePair<string, Stream> docx2007Prop in Docx2007Props)
			{
				stringBuilder.Append(Document.Comparison.ConvertBytesAsHash(Comparison.ConvertStreamToBytes(docx2007Prop.Value)));
			}
		}
		if (DocxStyleProps != null)
		{
			foreach (string docxStyleProp in DocxStyleProps)
			{
				stringBuilder.Append(docxStyleProp + ";");
			}
		}
		if (DocxPictureVisualProps != null)
		{
			foreach (Stream docxPictureVisualProp in DocxPictureVisualProps)
			{
				stringBuilder.Append(Document.Comparison.ConvertBytesAsHash(Comparison.ConvertStreamToBytes(docxPictureVisualProp)));
			}
		}
		if (SvgData != null)
		{
			stringBuilder.Append(Document.Comparison.ConvertBytesAsHash(SvgData));
		}
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
		if (Chart != null)
		{
			stringBuilder.Append(Chart.GetAsString());
		}
		if (ImageRecord != null)
		{
			stringBuilder.Append(ImageRecord.comparedImageName + ";");
			if (ImageBytes != null)
			{
				stringBuilder.Append(Document.Comparison.ConvertBytesAsHash(ImageBytes));
			}
		}
		switch (ElementType)
		{
		case EntityType.Picture:
		{
			WPicture wPicture = GetOwnerGroupShape().ConvertChildShapeToPicture(this);
			stringBuilder.Append(wPicture.GetAsString());
			break;
		}
		case EntityType.TextBox:
		{
			WTextBox wTextBox = GetOwnerGroupShape().ConvertChildShapeToTextbox(this);
			stringBuilder.Append(wTextBox.GetAsString());
			break;
		}
		case EntityType.AutoShape:
		{
			Shape shape = GetOwnerGroupShape().ConvertChildShapeToShape(this);
			stringBuilder.Append(shape.GetAsString());
			break;
		}
		case EntityType.Chart:
			stringBuilder.Append(Chart.GetAsString());
			break;
		}
		return stringBuilder;
	}
}
