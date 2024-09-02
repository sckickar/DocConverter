using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using DocGen.DocIO.DLS.Convertors;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WPicture : ParagraphItem, ILeafWidget, IWidget, IWPicture, IParagraphItem, IEntity
{
	private float m_rotation;

	internal SizeF m_size;

	private float m_widthScale = 100f;

	private float m_heightScale = 100f;

	private HorizontalOrigin m_horizontalOrigin;

	private ShapePosition m_shapePosition;

	private VerticalOrigin m_verticalOrigin;

	private float m_horizPosition;

	private TileRectangle m_fillRectable;

	private float m_vertPosition;

	private float m_DistanceFromBottom;

	private float m_DistanceFromLeft = 9f;

	private float m_DistanceFromRight = 9f;

	private float m_DistanceFromTop;

	private TextWrappingStyle m_wrappingStyle;

	private TextWrappingType m_wrappingType;

	private ShapeHorizontalAlignment m_horAlignment;

	private ShapeVerticalAlignment m_vertAlignment;

	private int m_spid = -1;

	private InlineShapeObject m_inlinePictureShape;

	internal List<Stream> m_docxProps;

	internal List<Stream> m_docxVisualShapeProps;

	private List<Stream> m_signatureLineElements;

	private string m_altText;

	private string m_name;

	private string m_title;

	private WTextBody m_embedBody;

	private int m_orderIndex = int.MaxValue;

	private ImageRecord m_imageRecord;

	internal short WrapCollectionIndex = -1;

	private WrapPolygon m_wrapPolygon;

	private string m_href;

	private string m_ExternalLinkName;

	private string m_linktype;

	private ushort m_bFlags = 88;

	private FillFormat m_fillFormat;

	private Color m_chromaKeyColor;

	private string m_oPictureHRef;

	private byte[] m_svgImageData;

	private string m_svgExternalLinkName = string.Empty;

	internal TileRectangle FillRectangle
	{
		get
		{
			if (m_fillRectable == null)
			{
				m_fillRectable = new TileRectangle();
			}
			return m_fillRectable;
		}
		set
		{
			m_fillRectable = value;
		}
	}

	internal bool HasBorder
	{
		get
		{
			if (TextWrappingStyle == TextWrappingStyle.Inline && IsShape)
			{
				return !((PictureShape.PictureDescriptor.BorderBottom.IsDefault && PictureShape.PictureDescriptor.BorderLeft.IsDefault && PictureShape.PictureDescriptor.BorderRight.IsDefault && PictureShape.PictureDescriptor.BorderTop.IsDefault) | (PictureShape.PictureDescriptor.BorderBottom.BorderType == 0 && PictureShape.PictureDescriptor.BorderLeft.BorderType == 0 && PictureShape.PictureDescriptor.BorderRight.BorderType == 0 && PictureShape.PictureDescriptor.BorderTop.BorderType == 0));
			}
			if (PictureShape.ShapeContainer != null && PictureShape.ShapeContainer.ShapeOptions != null && PictureShape.ShapeContainer.ShapeOptions.LineProperties.HasDefined && PictureShape.ShapeContainer.ShapeOptions.LineProperties.UsefLine)
			{
				return PictureShape.ShapeContainer.ShapeOptions.LineProperties.Line;
			}
			return false;
		}
	}

	public override EntityType EntityType => EntityType.Picture;

	public float Height
	{
		get
		{
			return Size.Height * m_heightScale / 100f;
		}
		set
		{
			if (base.Document != null && LockAspectRatio && !base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && !base.Document.IsClosing)
			{
				float heightScale = m_heightScale;
				SetHeightScaleValue(value / Size.Height * 100f);
				float num = m_heightScale / heightScale;
				SetWidthScaleValue(m_widthScale * num);
			}
			else
			{
				SetHeightScaleValue(value / Size.Height * 100f);
			}
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

	public float Width
	{
		get
		{
			return Size.Width * m_widthScale / 100f;
		}
		set
		{
			if (base.Document != null && LockAspectRatio && !base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && !base.Document.IsClosing)
			{
				float widthScale = m_widthScale;
				SetWidthScaleValue(value / Size.Width * 100f);
				float num = m_widthScale / widthScale;
				SetHeightScaleValue(m_heightScale * num);
			}
			else
			{
				SetWidthScaleValue(value / Size.Width * 100f);
			}
		}
	}

	public float HeightScale
	{
		get
		{
			return m_heightScale;
		}
		set
		{
			if (value < 0f || value > 10675f)
			{
				throw new ArgumentOutOfRangeException("Scale factor must be between 0 and 10675");
			}
			m_heightScale = value;
		}
	}

	public float WidthScale
	{
		get
		{
			return m_widthScale;
		}
		set
		{
			if (value < 0f || value > 10675f)
			{
				throw new ArgumentOutOfRangeException("Scale factor must be between 0 and 10675");
			}
			m_widthScale = value;
		}
	}

	public bool LockAspectRatio
	{
		get
		{
			return (m_bFlags & 0x100) >> 8 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFEFFu) | ((value ? 1u : 0u) << 8));
		}
	}

	internal DocGen.DocIO.DLS.Entities.Image ImageForPartialTrustMode => GetImageForPartialTrustMode(ImageBytes);

	internal DocGen.DocIO.DLS.Entities.Image Image => GetImage(ImageBytes, base.Document != null && !base.Document.IsOpening);

	public byte[] ImageBytes
	{
		get
		{
			if (m_imageRecord == null)
			{
				return null;
			}
			return m_imageRecord.ImageBytes;
		}
	}

	public byte[] SvgData
	{
		get
		{
			return m_svgImageData;
		}
		internal set
		{
			m_svgImageData = value;
		}
	}

	internal ImageRecord ImageRecord => m_imageRecord;

	internal ShapePosition Position
	{
		get
		{
			return m_shapePosition;
		}
		set
		{
			m_shapePosition = value;
		}
	}

	public HorizontalOrigin HorizontalOrigin
	{
		get
		{
			return m_horizontalOrigin;
		}
		set
		{
			m_horizontalOrigin = value;
		}
	}

	public VerticalOrigin VerticalOrigin
	{
		get
		{
			return m_verticalOrigin;
		}
		set
		{
			m_verticalOrigin = value;
		}
	}

	public float HorizontalPosition
	{
		get
		{
			return m_horizPosition;
		}
		set
		{
			m_horizPosition = value;
		}
	}

	public float VerticalPosition
	{
		get
		{
			return m_vertPosition;
		}
		set
		{
			m_vertPosition = value;
		}
	}

	internal float DistanceFromBottom
	{
		get
		{
			if (m_DistanceFromBottom < 0f || m_DistanceFromBottom > 1584f)
			{
				return 0f;
			}
			return m_DistanceFromBottom;
		}
		set
		{
			m_DistanceFromBottom = value;
		}
	}

	internal float DistanceFromLeft
	{
		get
		{
			if (m_DistanceFromLeft < 0f || m_DistanceFromLeft > 1584f)
			{
				return 0f;
			}
			return m_DistanceFromLeft;
		}
		set
		{
			m_DistanceFromLeft = value;
		}
	}

	internal float DistanceFromRight
	{
		get
		{
			if (m_DistanceFromRight < 0f || m_DistanceFromRight > 1584f)
			{
				return 0f;
			}
			return m_DistanceFromRight;
		}
		set
		{
			m_DistanceFromRight = value;
		}
	}

	internal float DistanceFromTop
	{
		get
		{
			if (m_DistanceFromTop < 0f || m_DistanceFromTop > 1584f)
			{
				return 0f;
			}
			return m_DistanceFromTop;
		}
		set
		{
			m_DistanceFromTop = value;
		}
	}

	public TextWrappingStyle TextWrappingStyle
	{
		get
		{
			return m_wrappingStyle;
		}
		set
		{
			if (HasBorder)
			{
				if (m_wrappingStyle == TextWrappingStyle.Inline && value != 0)
				{
					PictureShape.ConvertToShape();
				}
				else if (m_wrappingStyle != 0 && value == TextWrappingStyle.Inline)
				{
					PictureShape.ConvertToInlineShape();
				}
			}
			m_wrappingStyle = value;
			if (m_wrappingStyle == TextWrappingStyle.Behind)
			{
				m_bFlags = (ushort)((m_bFlags & 0xFFFEu) | 1u);
			}
			else
			{
				m_bFlags = (ushort)((m_bFlags & 0xFFFEu) | 0u);
			}
		}
	}

	public TextWrappingType TextWrappingType
	{
		get
		{
			return m_wrappingType;
		}
		set
		{
			m_wrappingType = value;
		}
	}

	public ShapeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_horAlignment;
		}
		set
		{
			m_horAlignment = value;
		}
	}

	public ShapeVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_vertAlignment;
		}
		set
		{
			m_vertAlignment = value;
		}
	}

	public bool IsBelowText
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFEu) | (value ? 1u : 0u));
			if (value && TextWrappingStyle == TextWrappingStyle.InFrontOfText)
			{
				m_wrappingStyle = TextWrappingStyle.Behind;
			}
			else if (!value && TextWrappingStyle == TextWrappingStyle.Behind)
			{
				m_wrappingStyle = TextWrappingStyle.InFrontOfText;
			}
		}
	}

	public WCharacterFormat CharacterFormat
	{
		get
		{
			return m_charFormat;
		}
		internal set
		{
			m_charFormat = value;
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

	internal bool IsHeaderPicture
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal InlineShapeObject PictureShape
	{
		get
		{
			return m_inlinePictureShape;
		}
		set
		{
			if (m_inlinePictureShape != null)
			{
				m_inlinePictureShape.SetOwner(null, null);
			}
			m_inlinePictureShape = value;
			if (m_inlinePictureShape != null)
			{
				m_inlinePictureShape.SetOwner(this);
			}
		}
	}

	internal SizeF Size
	{
		get
		{
			if (m_size.Width == float.MinValue || m_size.Height == float.MinValue)
			{
				if (WordDocument.EnablePartialTrustCode)
				{
					CheckPicSizeForPartialTrustMode(ImageForPartialTrustMode);
				}
				else
				{
					CheckPicSize(GetImage(ImageBytes, base.Document != null && !base.Document.IsOpening));
				}
			}
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	internal bool IsMetaFile
	{
		get
		{
			if (m_imageRecord == null)
			{
				return false;
			}
			return m_imageRecord.IsMetafile;
		}
	}

	internal List<Stream> DocxProps
	{
		get
		{
			if (m_docxProps == null)
			{
				m_docxProps = new List<Stream>();
			}
			return m_docxProps;
		}
		set
		{
			m_docxProps = value;
		}
	}

	internal List<Stream> DocxVisualShapeProps
	{
		get
		{
			if (m_docxVisualShapeProps == null)
			{
				m_docxVisualShapeProps = new List<Stream>();
			}
			return m_docxVisualShapeProps;
		}
		set
		{
			m_docxVisualShapeProps = value;
		}
	}

	internal List<Stream> SignatureLineElements
	{
		get
		{
			if (m_signatureLineElements == null)
			{
				m_signatureLineElements = new List<Stream>();
			}
			return m_signatureLineElements;
		}
	}

	public string AlternativeText
	{
		get
		{
			return m_altText;
		}
		set
		{
			m_altText = value;
		}
	}

	public string Name
	{
		get
		{
			if (m_name == null && m_imageRecord != null)
			{
				m_name = "Picture " + m_imageRecord.ImageId;
			}
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal Color ChromaKeyColor
	{
		get
		{
			return m_chromaKeyColor;
		}
		set
		{
			m_chromaKeyColor = value;
		}
	}

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal WTextBody EmbedBody
	{
		get
		{
			return m_embedBody;
		}
		set
		{
			m_embedBody = value;
		}
	}

	internal bool IsShape
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFBu) | ((value ? 1u : 0u) << 2));
		}
	}

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

	internal bool LayoutInCell
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool AllowOverlap
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal string Href
	{
		get
		{
			return m_href;
		}
		set
		{
			m_href = value;
		}
	}

	internal string ExternalLink
	{
		get
		{
			return m_ExternalLinkName;
		}
		set
		{
			m_ExternalLinkName = value;
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

	internal bool HasImageRecordReference
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFF7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal string LinkType
	{
		get
		{
			return m_linktype;
		}
		set
		{
			m_linktype = value;
		}
	}

	internal WrapPolygon WrapPolygon
	{
		get
		{
			if (m_wrapPolygon == null)
			{
				m_wrapPolygon = new WrapPolygon();
				m_wrapPolygon.Edited = false;
				m_wrapPolygon.Vertices.Add(new PointF(0f, 0f));
				m_wrapPolygon.Vertices.Add(new PointF(0f, 21600f));
				m_wrapPolygon.Vertices.Add(new PointF(21600f, 21600f));
				m_wrapPolygon.Vertices.Add(new PointF(21600f, 0f));
				m_wrapPolygon.Vertices.Add(new PointF(0f, 0f));
			}
			return m_wrapPolygon;
		}
		set
		{
			m_wrapPolygon = value;
		}
	}

	public bool Visible
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal new bool IsWrappingBoundsAdded
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFDFu) | ((value ? 1u : 0u) << 5));
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

	public bool FlipHorizontal
	{
		get
		{
			return (m_bFlags & 0x200) >> 9 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFDFFu) | ((value ? 1u : 0u) << 9));
		}
	}

	public bool FlipVertical
	{
		get
		{
			return (m_bFlags & 0x400) >> 10 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal bool IsDefaultPicOfContentControl
	{
		get
		{
			return (m_bFlags & 0x800) >> 11 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xF7FFu) | ((value ? 1u : 0u) << 11));
		}
	}

	public WPicture(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(doc, this);
		m_inlinePictureShape = new InlineShapeObject(doc);
		m_inlinePictureShape.SetOwner(this);
		m_size = default(SizeF);
		m_size.Height = float.MinValue;
		m_size.Width = float.MinValue;
	}

	public void LoadImage(byte[] imageBytes)
	{
		if (imageBytes == null)
		{
			throw new ArgumentNullException("Image bytes cannot be null or empty");
		}
		ResetImageData();
		DocGen.DocIO.DLS.Entities.Image image = GetImage(imageBytes, base.Document != null && !base.Document.IsOpening);
		if (image.IsMetafile)
		{
			LoadImage(imageBytes, isMetafile: true);
		}
		else
		{
			LoadImage(imageBytes, isMetafile: false);
		}
		imageBytes = null;
		CheckPicSize(image);
		UpdateBlipImageRecord();
	}

	public void LoadImage(byte[] svgData, byte[] imageBytes)
	{
		EvaluateSVGImageBytes(svgData);
		SvgData = svgData;
		string item = "{96DAC541-7B7A-43D3-8B79-37D633B846F1}";
		FillFormat.FillType = FillType.FillPicture;
		FillFormat.BlipFormat.ExtensionURI.Add(item);
		FillFormat.BlipFormat.ExtensionURI.Add("svgBlip");
		LoadImage(imageBytes);
	}

	internal static void EvaluateSVGImageBytes(byte[] svgData)
	{
		if (svgData == null)
		{
			throw new ArgumentNullException("svgData should not be null");
		}
		XmlReader xmlReader = UtilityMethods.CreateReader(new MemoryStream(svgData));
		string localName = xmlReader.LocalName;
		xmlReader.Dispose();
		if (localName != "svg")
		{
			throw new ArgumentException("SVG data should be *.svg format");
		}
	}

	internal void LoadImage(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		ResetImageData();
		if (image.IsMetafile)
		{
			m_imageRecord = base.Document.Images.LoadMetaFileImage(image.ImageData, isCompressed: false);
		}
		else
		{
			m_imageRecord = base.Document.Images.LoadImage(image.ImageData);
		}
		CheckPicSize(image);
		UpdateBlipImageRecord();
	}

	public void LoadImage(Stream imageStream)
	{
		if (imageStream == null)
		{
			throw new ArgumentNullException("imageStream");
		}
		DocGen.DocIO.DLS.Entities.Image image = new DocGen.DocIO.DLS.Entities.Image(imageStream);
		LoadImage(image);
	}

	private void UpdateBlipImageRecord()
	{
		if (base.Document != null && !base.Document.IsOpening && base.Document.Escher != null && base.Document.Escher.Containers != null && base.Document.Escher.Containers.ContainsKey(ShapeId) && base.Document.Escher.Containers[ShapeId] is MsofbtSpContainer { Bse: not null } msofbtSpContainer && msofbtSpContainer.Bse.Blip != null)
		{
			msofbtSpContainer.Bse.Blip.ImageRecord = m_imageRecord;
		}
	}

	public IWParagraph AddCaption(string name, CaptionNumberingFormat format, CaptionPosition captionPosition)
	{
		WTextBody wTextBody = base.OwnerParagraph.Owner as WTextBody;
		WParagraph wParagraph = null;
		if (wTextBody != null)
		{
			int indexInOwnerCollection = base.OwnerParagraph.GetIndexInOwnerCollection();
			wParagraph = new WParagraph(base.Document);
			wParagraph.AppendText(name + " ");
			name = name.Replace(" ", "_");
			wParagraph.ApplyStyle(BuiltinStyle.Caption, isDomChanges: false);
			((WSeqField)wParagraph.AppendField(name, FieldType.FieldSequence)).NumberFormat = format;
			int num = base.OwnerParagraph.Items.IndexOf(this);
			if (captionPosition == CaptionPosition.AboveImage)
			{
				wParagraph.ParagraphFormat.KeepFollow = true;
				int num2 = ((num == 0) ? indexInOwnerCollection : (indexInOwnerCollection + 1));
				wTextBody.Items.Insert(num2, wParagraph);
				if (num > 0)
				{
					base.OwnerParagraph.Items.RemoveAt(num);
					WParagraph wParagraph2 = new WParagraph(base.Document);
					wParagraph2.Items.Insert(0, this);
					wTextBody.Items.Insert(num2 + 1, wParagraph2);
				}
			}
			else
			{
				base.OwnerParagraph.ParagraphFormat.KeepFollow = true;
				wTextBody.Items.Insert(indexInOwnerCollection + 1, wParagraph);
			}
		}
		return wParagraph;
	}

	internal override void AddSelf()
	{
		if (m_imageRecord != null)
		{
			Size size = m_imageRecord.Size;
			DocGen.DocIO.DLS.Entities.ImageFormat imageFormat = m_imageRecord.ImageFormat;
			int length = m_imageRecord.Length;
			if (m_imageRecord.IsMetafile)
			{
				m_imageRecord = base.Document.Images.LoadMetaFileImage(m_imageRecord.m_imageBytes, isCompressed: true);
			}
			else
			{
				m_imageRecord = base.Document.Images.LoadImage(m_imageRecord.ImageBytes);
			}
			m_imageRecord.Size = size;
			m_imageRecord.ImageFormat = imageFormat;
			m_imageRecord.Length = length;
		}
	}

	protected override object CloneImpl()
	{
		WPicture wPicture = (WPicture)base.CloneImpl();
		if (m_docxProps != null && m_docxProps.Count > 0)
		{
			base.Document.CloneProperties(m_docxProps, ref wPicture.m_docxProps);
		}
		if (m_docxVisualShapeProps != null && m_docxVisualShapeProps.Count > 0)
		{
			base.Document.CloneProperties(m_docxVisualShapeProps, ref wPicture.m_docxVisualShapeProps);
		}
		if (m_signatureLineElements != null && m_signatureLineElements.Count > 0)
		{
			base.Document.CloneProperties(m_signatureLineElements, ref wPicture.m_signatureLineElements);
		}
		wPicture.m_charFormat = new WCharacterFormat(base.Document, wPicture);
		wPicture.m_charFormat.ImportContainer(m_charFormat);
		wPicture.m_inlinePictureShape = (InlineShapeObject)PictureShape.Clone();
		wPicture.m_inlinePictureShape.SetOwner(wPicture);
		if (wPicture.ImageRecord != null)
		{
			ImageRecord imageRecord = new ImageRecord(base.Document, m_imageRecord);
			wPicture.m_imageRecord = imageRecord;
		}
		if (SvgData != null)
		{
			wPicture.SvgData = SvgData;
		}
		if (m_size.Width != float.MinValue && m_size.Height != float.MinValue)
		{
			wPicture.Size = m_size;
		}
		if (EmbedBody != null)
		{
			wPicture.EmbedBody = (WTextBody)EmbedBody.Clone();
		}
		if (WrapPolygon != null)
		{
			wPicture.WrapPolygon = WrapPolygon.Clone();
		}
		wPicture.IsCloned = true;
		wPicture.ShapeId = 0;
		return wPicture;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if ((nextOwner.OwnerBase != null && nextOwner.OwnerBase is HeaderFooter) || nextOwner is HeaderFooter)
		{
			IsHeaderPicture = true;
		}
		if (m_imageRecord != null)
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
		base.Document.CloneShapeEscher(doc, this);
		PictureShape.CloneRelationsTo(doc, nextOwner);
		base.IsCloned = false;
		if (EmbedBody != null)
		{
			EmbedBody.CloneRelationsTo(doc, nextOwner);
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.Picture);
		writer.WriteValue("width", Size.Width);
		writer.WriteValue("height", Size.Height);
		writer.WriteValue("WidthScale", m_widthScale);
		writer.WriteValue("HeightScale", m_heightScale);
		writer.WriteValue("IsMetafile", ImageRecord.IsMetafile);
		if (m_wrappingStyle != 0)
		{
			writer.WriteValue("HorizontalOrigin", m_horizontalOrigin);
			writer.WriteValue("VerticalOrigin", m_verticalOrigin);
			writer.WriteValue("VerticalPosition", m_vertPosition);
			writer.WriteValue("HorizontalPosition", m_horizPosition);
			writer.WriteValue("WrappingStyle", m_wrappingStyle);
			writer.WriteValue("WrappingType", m_wrappingType);
			writer.WriteValue("IsBelowText", IsBelowText);
			writer.WriteValue("HorizontalAlignment", m_horAlignment);
			writer.WriteValue("VerticalAlignment", m_vertAlignment);
			writer.WriteValue("ShapeID", m_spid);
			if (IsHeaderPicture)
			{
				writer.WriteValue("IsHeader", IsHeaderPicture);
			}
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_size.Width = reader.ReadFloat("width");
		m_size.Height = reader.ReadFloat("height");
		m_widthScale = reader.ReadFloat("WidthScale");
		m_heightScale = reader.ReadFloat("HeightScale");
		if (reader.HasAttribute("HorizontalOrigin"))
		{
			m_horizontalOrigin = (HorizontalOrigin)(object)reader.ReadEnum("HorizontalOrigin", typeof(HorizontalOrigin));
		}
		if (reader.HasAttribute("VerticalOrigin"))
		{
			m_verticalOrigin = (VerticalOrigin)(object)reader.ReadEnum("VerticalOrigin", typeof(VerticalOrigin));
		}
		if (reader.HasAttribute("VerticalPosition"))
		{
			m_vertPosition = reader.ReadFloat("VerticalPosition");
		}
		if (reader.HasAttribute("HorizontalPosition"))
		{
			m_horizPosition = reader.ReadFloat("HorizontalPosition");
		}
		if (reader.HasAttribute("WrappingStyle"))
		{
			m_wrappingStyle = (TextWrappingStyle)(object)reader.ReadEnum("WrappingStyle", typeof(TextWrappingStyle));
		}
		if (reader.HasAttribute("WrappingType"))
		{
			m_wrappingType = (TextWrappingType)(object)reader.ReadEnum("WrappingType", typeof(TextWrappingType));
		}
		if (reader.HasAttribute("IsBelowText"))
		{
			IsBelowText = reader.ReadBoolean("IsBelowText");
		}
		if (reader.HasAttribute("HorizontalAlignment"))
		{
			m_horAlignment = (ShapeHorizontalAlignment)(object)reader.ReadEnum("HorizontalAlignment", typeof(ShapeHorizontalAlignment));
		}
		if (reader.HasAttribute("VerticalAlignment"))
		{
			m_vertAlignment = (ShapeVerticalAlignment)(object)reader.ReadEnum("VerticalAlignment", typeof(ShapeVerticalAlignment));
		}
		if (reader.HasAttribute("ShapeID"))
		{
			m_spid = reader.ReadInt("ShapeID");
		}
		if (reader.HasAttribute("IsHeader"))
		{
			IsHeaderPicture = reader.ReadBoolean("IsHeader");
		}
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		if (m_imageRecord != null)
		{
			writer.WriteChildBinaryElement("image", m_imageRecord.ImageBytes);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		base.ReadXmlContent(reader);
		return false;
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("character-format", m_charFormat);
		base.XDLSHolder.AddElement("shape-format", m_inlinePictureShape);
	}

	internal override void Close()
	{
		base.Close();
		if (m_imageRecord != null && !base.DeepDetached)
		{
			m_imageRecord.OccurenceCount--;
			m_imageRecord = null;
		}
		if (m_embedBody != null)
		{
			m_embedBody.Close();
			m_embedBody = null;
		}
		if (m_inlinePictureShape != null)
		{
			m_inlinePictureShape.Close();
			m_inlinePictureShape = null;
		}
		if (m_wrapPolygon != null)
		{
			m_wrapPolygon.Close();
			m_wrapPolygon = null;
		}
		if (m_docxProps != null)
		{
			foreach (Stream docxProp in m_docxProps)
			{
				docxProp.Close();
			}
			m_docxProps.Clear();
			m_docxProps = null;
		}
		if (m_docxVisualShapeProps != null)
		{
			foreach (Stream docxVisualShapeProp in m_docxVisualShapeProps)
			{
				docxVisualShapeProp.Close();
			}
			m_docxVisualShapeProps.Clear();
			m_docxVisualShapeProps = null;
		}
		if (m_signatureLineElements != null)
		{
			foreach (Stream signatureLineElement in m_signatureLineElements)
			{
				signatureLineElement.Close();
			}
			m_signatureLineElements.Clear();
			m_signatureLineElements = null;
		}
		if (m_fillFormat != null)
		{
			m_fillFormat.Close();
			m_fillFormat = null;
		}
		if (m_svgImageData != null)
		{
			m_svgImageData = null;
		}
	}

	internal bool Compare(WPicture picture)
	{
		if (picture.Document.Comparison.IsComparedImages)
		{
			if (string.IsNullOrEmpty(picture.ImageRecord.comparedImageName))
			{
				return false;
			}
		}
		else
		{
			picture.Document.Comparison.CompareImagesInDoc(base.Document);
			if (string.IsNullOrEmpty(picture.ImageRecord.comparedImageName))
			{
				return false;
			}
		}
		if (HasBorder != picture.HasBorder || LockAspectRatio != picture.LockAspectRatio || IsBelowText != picture.IsBelowText || IsHeaderPicture != picture.IsHeaderPicture || IsMetaFile != picture.IsMetaFile || IsShape != picture.IsShape || LayoutInCell != picture.LayoutInCell || AllowOverlap != picture.AllowOverlap || HasImageRecordReference != picture.HasImageRecordReference || Visible != picture.Visible || IsWrappingBoundsAdded != picture.IsWrappingBoundsAdded || FlipHorizontal != picture.FlipHorizontal || FlipVertical != picture.FlipVertical || Math.Round(Height, 2) != Math.Round(picture.Height, 2) || Math.Round(Rotation, 2) != Math.Round(picture.Rotation, 2) || Math.Round(Width, 2) != Math.Round(picture.Width, 2) || Math.Round(WidthScale, 2) != Math.Round(picture.WidthScale, 2) || Position != picture.Position || HorizontalOrigin != picture.HorizontalOrigin || VerticalOrigin != picture.VerticalOrigin || Math.Round(HorizontalPosition, 2) != Math.Round(picture.HorizontalPosition, 2) || Math.Round(VerticalPosition, 2) != Math.Round(picture.VerticalPosition, 2) || Math.Round(DistanceFromBottom, 2) != Math.Round(picture.DistanceFromBottom, 2) || Math.Round(DistanceFromLeft, 2) != Math.Round(picture.DistanceFromLeft, 2) || Math.Round(DistanceFromRight, 2) != Math.Round(picture.DistanceFromRight, 2) || Math.Round(DistanceFromTop, 2) != Math.Round(picture.DistanceFromTop, 2) || TextWrappingStyle != picture.TextWrappingStyle || TextWrappingType != picture.TextWrappingType || HorizontalAlignment != picture.HorizontalAlignment || VerticalAlignment != picture.VerticalAlignment || ShapeId != picture.ShapeId || AlternativeText != picture.AlternativeText || Name != picture.Name || Title != picture.Title || OrderIndex != picture.OrderIndex || Href != picture.Href || ExternalLink != picture.ExternalLink || SvgExternalLink != picture.SvgExternalLink || LinkType != picture.LinkType)
		{
			return false;
		}
		if ((FillRectangle != null && picture.FillRectangle == null) || (FillRectangle == null && picture.FillRectangle != null) || (CharacterFormat != null && picture.CharacterFormat == null) || (CharacterFormat == null && picture.CharacterFormat != null) || (FillFormat != null && picture.FillFormat == null) || (FillFormat == null && picture.FillFormat != null) || (PictureShape != null && picture.PictureShape == null) || (PictureShape == null && picture.PictureShape != null) || (WrapPolygon != null && picture.WrapPolygon == null) || (WrapPolygon == null && picture.WrapPolygon != null) || (SvgData != null && picture.SvgData == null) || (SvgData == null && picture.SvgData != null))
		{
			return false;
		}
		if (FillRectangle != null && picture.FillRectangle != null && !FillRectangle.Compare(picture.FillRectangle))
		{
			return false;
		}
		if (FillFormat != null && picture.FillFormat != null && !FillFormat.Compare(picture.FillFormat))
		{
			return false;
		}
		if (CharacterFormat != null && picture.CharacterFormat != null && !CharacterFormat.Compare(picture.CharacterFormat))
		{
			return false;
		}
		if (PictureShape != null && picture.PictureShape != null && !PictureShape.Compare(picture.PictureShape))
		{
			return false;
		}
		if (WrapPolygon != null && picture.WrapPolygon != null && !WrapPolygon.Compare(picture.WrapPolygon))
		{
			return false;
		}
		if (SvgData != null && picture.SvgData != null && !Comparison.CompareBytes(SvgData, picture.SvgData))
		{
			return false;
		}
		return true;
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0013');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append(ImageRecord.comparedImageName);
		stringBuilder.Append('\u0013');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (HasBorder ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (LockAspectRatio ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsBelowText ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsHeaderPicture ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsMetaFile ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsShape ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (LayoutInCell ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (AllowOverlap ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (HasImageRecordReference ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (Visible ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (IsWrappingBoundsAdded ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (FlipHorizontal ? "1" : "0");
		stringBuilder.Append(text + ";");
		text = (FlipVertical ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(Height + ";");
		stringBuilder.Append(Rotation + ";");
		stringBuilder.Append(Width + ";");
		stringBuilder.Append(WidthScale + ";");
		stringBuilder.Append((int)Position + ";");
		stringBuilder.Append((int)HorizontalOrigin + ";");
		stringBuilder.Append((int)VerticalOrigin + ";");
		stringBuilder.Append(HorizontalPosition + ";");
		stringBuilder.Append(VerticalPosition + ";");
		stringBuilder.Append(DistanceFromBottom + ";");
		stringBuilder.Append(DistanceFromLeft + ";");
		stringBuilder.Append(DistanceFromRight + ";");
		stringBuilder.Append(DistanceFromTop + ";");
		stringBuilder.Append((int)TextWrappingStyle + ";");
		stringBuilder.Append((int)TextWrappingType + ";");
		stringBuilder.Append((int)HorizontalAlignment + ";");
		stringBuilder.Append((int)VerticalAlignment + ";");
		stringBuilder.Append(ShapeId + ";");
		stringBuilder.Append(AlternativeText + ";");
		stringBuilder.Append(Name + ";");
		stringBuilder.Append(Title + ";");
		stringBuilder.Append(OrderIndex + ";");
		stringBuilder.Append(Href + ";");
		stringBuilder.Append(ExternalLink + ";");
		stringBuilder.Append(SvgExternalLink + ";");
		stringBuilder.Append(LinkType + ";");
		if (CharacterFormat != null)
		{
			stringBuilder.Append(CharacterFormat.GetAsString());
		}
		if (FillRectangle != null)
		{
			stringBuilder.Append(FillRectangle.GetAsString());
		}
		if (FillFormat != null)
		{
			stringBuilder.Append(FillFormat.GetAsString());
		}
		if (PictureShape != null)
		{
			stringBuilder.Append(PictureShape.GetAsString());
		}
		if (WrapPolygon != null)
		{
			stringBuilder.Append(WrapPolygon.GetAsString());
		}
		stringBuilder.Append(Size.Height + ";" + Size.Width + ";");
		if (SvgData != null)
		{
			stringBuilder.Append(base.Document.Comparison.ConvertBytesAsHash(SvgData));
		}
		stringBuilder.Append(ChromaKeyColor.ToArgb() + ";");
		return stringBuilder;
	}

	internal Dictionary<float, List<float>> GetBrightnessValueRanges()
	{
		return new Dictionary<float, List<float>>
		{
			{
				0f,
				new List<float> { -32768f, -32441f }
			},
			{
				1f,
				new List<float> { -32440f, -31785f }
			},
			{
				2f,
				new List<float> { -31784f, -31130f }
			},
			{
				3f,
				new List<float> { -31129f, -30475f }
			},
			{
				4f,
				new List<float> { -30474f, -29819f }
			},
			{
				5f,
				new List<float> { -29818f, -29164f }
			},
			{
				6f,
				new List<float> { -29163f, -28509f }
			},
			{
				7f,
				new List<float> { -28508f, -27853f }
			},
			{
				8f,
				new List<float> { -27852f, -27198f }
			},
			{
				9f,
				new List<float> { -27197f, -26543f }
			},
			{
				10f,
				new List<float> { -26542f, -25887f }
			},
			{
				11f,
				new List<float> { -25886f, -25232f }
			},
			{
				12f,
				new List<float> { -25231f, -24576f }
			},
			{
				13f,
				new List<float> { -24575f, -23921f }
			},
			{
				14f,
				new List<float> { -23920f, -23266f }
			},
			{
				15f,
				new List<float> { -23265f, -22610f }
			},
			{
				16f,
				new List<float> { -22609f, -21955f }
			},
			{
				17f,
				new List<float> { -21954f, -21300f }
			},
			{
				18f,
				new List<float> { -21299f, -20644f }
			},
			{
				19f,
				new List<float> { -20643f, -19989f }
			},
			{
				20f,
				new List<float> { -19988f, -19334f }
			},
			{
				21f,
				new List<float> { -19333f, -18678f }
			},
			{
				22f,
				new List<float> { -18677f, -18023f }
			},
			{
				23f,
				new List<float> { -18022f, -17368f }
			},
			{
				24f,
				new List<float> { -17367f, -16712f }
			},
			{
				25f,
				new List<float> { -16711f, -16057f }
			},
			{
				26f,
				new List<float> { -16056f, -15401f }
			},
			{
				27f,
				new List<float> { -15400f, -14746f }
			},
			{
				28f,
				new List<float> { -14745f, -14091f }
			},
			{
				29f,
				new List<float> { -14090f, -13435f }
			},
			{
				30f,
				new List<float> { -13434f, -12780f }
			},
			{
				31f,
				new List<float> { -12779f, -12125f }
			},
			{
				32f,
				new List<float> { -12124f, -11469f }
			},
			{
				33f,
				new List<float> { -11468f, -10814f }
			},
			{
				34f,
				new List<float> { -10813f, -10159f }
			},
			{
				35f,
				new List<float> { -10158f, -9503f }
			},
			{
				36f,
				new List<float> { -9502f, -8848f }
			},
			{
				37f,
				new List<float> { -8847f, -8192f }
			},
			{
				38f,
				new List<float> { -8191f, -7537f }
			},
			{
				39f,
				new List<float> { -7536f, -6882f }
			},
			{
				40f,
				new List<float> { -6881f, -6226f }
			},
			{
				41f,
				new List<float> { -6225f, -5571f }
			},
			{
				42f,
				new List<float> { -5570f, -4916f }
			},
			{
				43f,
				new List<float> { -4915f, -4260f }
			},
			{
				44f,
				new List<float> { -4259f, -3605f }
			},
			{
				45f,
				new List<float> { -3604f, -2950f }
			},
			{
				46f,
				new List<float> { -2949f, -2294f }
			},
			{
				47f,
				new List<float> { -2293f, -1639f }
			},
			{
				48f,
				new List<float> { -1638f, -984f }
			},
			{
				49f,
				new List<float> { -983f, -328f }
			},
			{
				50f,
				new List<float> { -327f, 327f }
			},
			{
				51f,
				new List<float> { 328f, 983f }
			},
			{
				52f,
				new List<float> { 984f, 1638f }
			},
			{
				53f,
				new List<float> { 1639f, 2293f }
			},
			{
				54f,
				new List<float> { 2294f, 2949f }
			},
			{
				55f,
				new List<float> { 2950f, 3604f }
			},
			{
				56f,
				new List<float> { 3605f, 4259f }
			},
			{
				57f,
				new List<float> { 4260f, 4915f }
			},
			{
				58f,
				new List<float> { 4916f, 5570f }
			},
			{
				59f,
				new List<float> { 5571f, 6225f }
			},
			{
				60f,
				new List<float> { 6226f, 6881f }
			},
			{
				61f,
				new List<float> { 6882f, 7536f }
			},
			{
				62f,
				new List<float> { 7537f, 8191f }
			},
			{
				63f,
				new List<float> { 8192f, 8847f }
			},
			{
				64f,
				new List<float> { 8848f, 9502f }
			},
			{
				65f,
				new List<float> { 9503f, 10158f }
			},
			{
				66f,
				new List<float> { 10159f, 10813f }
			},
			{
				67f,
				new List<float> { 10814f, 11468f }
			},
			{
				68f,
				new List<float> { 11469f, 12124f }
			},
			{
				69f,
				new List<float> { 12125f, 12779f }
			},
			{
				70f,
				new List<float> { 12780f, 13434f }
			},
			{
				71f,
				new List<float> { 13435f, 14090f }
			},
			{
				72f,
				new List<float> { 14091f, 14745f }
			},
			{
				73f,
				new List<float> { 14746f, 15400f }
			},
			{
				74f,
				new List<float> { 15401f, 16056f }
			},
			{
				75f,
				new List<float> { 16057f, 16711f }
			},
			{
				76f,
				new List<float> { 16712f, 17367f }
			},
			{
				77f,
				new List<float> { 17368f, 18022f }
			},
			{
				78f,
				new List<float> { 18023f, 18677f }
			},
			{
				79f,
				new List<float> { 18678f, 19333f }
			},
			{
				80f,
				new List<float> { 19334f, 19988f }
			},
			{
				81f,
				new List<float> { 19989f, 20643f }
			},
			{
				82f,
				new List<float> { 20644f, 21299f }
			},
			{
				83f,
				new List<float> { 21300f, 21954f }
			},
			{
				84f,
				new List<float> { 21955f, 22609f }
			},
			{
				85f,
				new List<float> { 22610f, 23265f }
			},
			{
				86f,
				new List<float> { 23266f, 23920f }
			},
			{
				87f,
				new List<float> { 23921f, 24575f }
			},
			{
				88f,
				new List<float> { 24576f, 25231f }
			},
			{
				89f,
				new List<float> { 25232f, 25886f }
			},
			{
				90f,
				new List<float> { 25887f, 26542f }
			},
			{
				91f,
				new List<float> { 26543f, 27197f }
			},
			{
				92f,
				new List<float> { 27198f, 27852f }
			},
			{
				93f,
				new List<float> { 27853f, 28508f }
			},
			{
				94f,
				new List<float> { 28509f, 29163f }
			},
			{
				95f,
				new List<float> { 29164f, 29818f }
			},
			{
				96f,
				new List<float> { 29819f, 30474f }
			},
			{
				97f,
				new List<float> { 30475f, 31129f }
			},
			{
				98f,
				new List<float> { 31130f, 31784f }
			},
			{
				99f,
				new List<float> { 31785f, 32440f }
			},
			{
				100f,
				new List<float> { 32441f, 32768f }
			}
		};
	}

	internal Dictionary<float, List<double>> GetContrastValueRanges()
	{
		return new Dictionary<float, List<double>>
		{
			{
				0f,
				new List<double> { 0.0, 655.0 }
			},
			{
				1f,
				new List<double> { 656.0, 1966.0 }
			},
			{
				2f,
				new List<double> { 1967.0, 3276.0 }
			},
			{
				3f,
				new List<double> { 3277.0, 4587.0 }
			},
			{
				4f,
				new List<double> { 4588.0, 5898.0 }
			},
			{
				5f,
				new List<double> { 5899.0, 7208.0 }
			},
			{
				6f,
				new List<double> { 7209.0, 8519.0 }
			},
			{
				7f,
				new List<double> { 8520.0, 9830.0 }
			},
			{
				8f,
				new List<double> { 9831.0, 11141.0 }
			},
			{
				9f,
				new List<double> { 11142.0, 12451.0 }
			},
			{
				10f,
				new List<double> { 12452.0, 13762.0 }
			},
			{
				11f,
				new List<double> { 13763.0, 15073.0 }
			},
			{
				12f,
				new List<double> { 15074.0, 16383.0 }
			},
			{
				13f,
				new List<double> { 16384.0, 17694.0 }
			},
			{
				14f,
				new List<double> { 17695.0, 19005.0 }
			},
			{
				15f,
				new List<double> { 19006.0, 20316.0 }
			},
			{
				16f,
				new List<double> { 20317.0, 21626.0 }
			},
			{
				17f,
				new List<double> { 21627.0, 22937.0 }
			},
			{
				18f,
				new List<double> { 22938.0, 24248.0 }
			},
			{
				19f,
				new List<double> { 24249.0, 25559.0 }
			},
			{
				20f,
				new List<double> { 25560.0, 26869.0 }
			},
			{
				21f,
				new List<double> { 26870.0, 28180.0 }
			},
			{
				22f,
				new List<double> { 28181.0, 29491.0 }
			},
			{
				23f,
				new List<double> { 29492.0, 30801.0 }
			},
			{
				24f,
				new List<double> { 30802.0, 32112.0 }
			},
			{
				25f,
				new List<double> { 32113.0, 33423.0 }
			},
			{
				26f,
				new List<double> { 33424.0, 34734.0 }
			},
			{
				27f,
				new List<double> { 34735.0, 36044.0 }
			},
			{
				28f,
				new List<double> { 36045.0, 37355.0 }
			},
			{
				29f,
				new List<double> { 37356.0, 38666.0 }
			},
			{
				30f,
				new List<double> { 38667.0, 39976.0 }
			},
			{
				31f,
				new List<double> { 39977.0, 41287.0 }
			},
			{
				32f,
				new List<double> { 41288.0, 42598.0 }
			},
			{
				33f,
				new List<double> { 42599.0, 43909.0 }
			},
			{
				34f,
				new List<double> { 43910.0, 45219.0 }
			},
			{
				35f,
				new List<double> { 45220.0, 46530.0 }
			},
			{
				36f,
				new List<double> { 46531.0, 47841.0 }
			},
			{
				37f,
				new List<double> { 47842.0, 49151.0 }
			},
			{
				38f,
				new List<double> { 49152.0, 50462.0 }
			},
			{
				39f,
				new List<double> { 50463.0, 51773.0 }
			},
			{
				40f,
				new List<double> { 51774.0, 53084.0 }
			},
			{
				41f,
				new List<double> { 53085.0, 54394.0 }
			},
			{
				42f,
				new List<double> { 54395.0, 55705.0 }
			},
			{
				43f,
				new List<double> { 55706.0, 57016.0 }
			},
			{
				44f,
				new List<double> { 57017.0, 58327.0 }
			},
			{
				45f,
				new List<double> { 58328.0, 59637.0 }
			},
			{
				46f,
				new List<double> { 59638.0, 60948.0 }
			},
			{
				47f,
				new List<double> { 60949.0, 62259.0 }
			},
			{
				48f,
				new List<double> { 62260.0, 63569.0 }
			},
			{
				49f,
				new List<double> { 63570.0, 64880.0 }
			},
			{
				50f,
				new List<double> { 64881.0, 66197.0 }
			},
			{
				51f,
				new List<double> { 66198.0, 67562.0 }
			},
			{
				52f,
				new List<double> { 67563.0, 68985.0 }
			},
			{
				53f,
				new List<double> { 68986.0, 70468.0 }
			},
			{
				54f,
				new List<double> { 70469.0, 72017.0 }
			},
			{
				55f,
				new List<double> { 72018.0, 73635.0 }
			},
			{
				56f,
				new List<double> { 73636.0, 75328.0 }
			},
			{
				57f,
				new List<double> { 75329.0, 77101.0 }
			},
			{
				58f,
				new List<double> { 77102.0, 78959.0 }
			},
			{
				59f,
				new List<double> { 78960.0, 80908.0 }
			},
			{
				60f,
				new List<double> { 80909.0, 82956.0 }
			},
			{
				61f,
				new List<double> { 82957.0, 85111.0 }
			},
			{
				62f,
				new List<double> { 85112.0, 87381.0 }
			},
			{
				63f,
				new List<double> { 87382.0, 89775.0 }
			},
			{
				64f,
				new List<double> { 89776.0, 92304.0 }
			},
			{
				65f,
				new List<double> { 92305.0, 94979.0 }
			},
			{
				66f,
				new List<double> { 94980.0, 97814.0 }
			},
			{
				67f,
				new List<double> { 97815.0, 100824.0 }
			},
			{
				68f,
				new List<double> { 100825.0, 104025.0 }
			},
			{
				69f,
				new List<double> { 104026.0, 107436.0 }
			},
			{
				70f,
				new List<double> { 107437.0, 111077.0 }
			},
			{
				71f,
				new List<double> { 111078.0, 114975.0 }
			},
			{
				72f,
				new List<double> { 114976.0, 119156.0 }
			},
			{
				73f,
				new List<double> { 119157.0, 123652.0 }
			},
			{
				74f,
				new List<double> { 123653.0, 128501.0 }
			},
			{
				75f,
				new List<double> { 128502.0, 133746.0 }
			},
			{
				76f,
				new List<double> { 133747.0, 139438.0 }
			},
			{
				77f,
				new List<double> { 139439.0, 145635.0 }
			},
			{
				78f,
				new List<double> { 145636.0, 152409.0 }
			},
			{
				79f,
				new List<double> { 152410.0, 159843.0 }
			},
			{
				80f,
				new List<double> { 159844.0, 168041.0 }
			},
			{
				81f,
				new List<double> { 168042.0, 177124.0 }
			},
			{
				82f,
				new List<double> { 177125.0, 187245.0 }
			},
			{
				83f,
				new List<double> { 187246.0, 198593.0 }
			},
			{
				84f,
				new List<double> { 198594.0, 211406.0 }
			},
			{
				85f,
				new List<double> { 211407.0, 225986.0 }
			},
			{
				86f,
				new List<double> { 225987.0, 242725.0 }
			},
			{
				87f,
				new List<double> { 242726.0, 262144.0 }
			},
			{
				88f,
				new List<double> { 262145.0, 284939.0 }
			},
			{
				89f,
				new List<double> { 284940.0, 312076.0 }
			},
			{
				90f,
				new List<double> { 312077.0, 344926.0 }
			},
			{
				91f,
				new List<double> { 344927.0, 385504.0 }
			},
			{
				92f,
				new List<double> { 385505.0, 436906.0 }
			},
			{
				93f,
				new List<double> { 436907.0, 504123.0 }
			},
			{
				94f,
				new List<double> { 504124.0, 595781.0 }
			},
			{
				95f,
				new List<double> { 595782.0, 728177.0 }
			},
			{
				96f,
				new List<double> { 728178.0, 936228.0 }
			},
			{
				97f,
				new List<double> { 936229.0, 1310720.0 }
			},
			{
				98f,
				new List<double> { 1310721.0, 2184533.0 }
			},
			{
				99f,
				new List<double> { 2184534.0, 6553600.0 }
			},
			{
				100f,
				new List<double> { 6553601.0, 2147483647.0 }
			}
		};
	}

	internal void SetWidthScaleValue(float value)
	{
		m_widthScale = value;
	}

	internal void SetHeightScaleValue(float value)
	{
		m_heightScale = value;
	}

	internal void SetTextWrappingStyleValue(TextWrappingStyle textWrappingStyle)
	{
		m_wrappingStyle = textWrappingStyle;
	}

	internal override void Detach()
	{
		if (m_imageRecord != null)
		{
			m_imageRecord.Detach();
		}
		RemoveImageInCollection();
	}

	internal void RemoveImageInCollection()
	{
		if (base.Document == null)
		{
			return;
		}
		if (base.Document.Escher != null && base.Document.Escher.Containers != null && base.Document.Escher.Containers.ContainsKey(ShapeId))
		{
			if (base.Document.Escher.Containers[ShapeId] is MsofbtSpContainer { Bse: not null } msofbtSpContainer)
			{
				msofbtSpContainer.Bse.Blip = null;
				msofbtSpContainer.Bse = null;
			}
			base.Document.Escher.Containers.Remove(ShapeId);
			foreach (MsofbtDgContainer dgContainer in base.Document.Escher.m_dgContainers)
			{
				for (int i = 0; i < dgContainer.PatriarchGroupContainer.Children.Count; i++)
				{
					if (dgContainer.PatriarchGroupContainer.Children[i] is MsofbtSpContainer msofbtSpContainer2 && msofbtSpContainer2.Shape.ShapeId == ShapeId)
					{
						dgContainer.PatriarchGroupContainer.Children.Remove(msofbtSpContainer2);
						break;
					}
				}
			}
		}
		base.Document.FloatingItems.Remove(this);
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		if (m_imageRecord != null)
		{
			m_imageRecord.Attach();
		}
		base.AttachToParagraph(owner, itemPos);
		if (!IsPreviousItemIsOleObject() && (TextWrappingStyle != 0 || base.Document.ActualFormatType != FormatType.Docx))
		{
			base.Document.FloatingItems.Add(this);
		}
	}

	private bool IsPreviousItemIsOleObject()
	{
		if (base.PreviousSibling is WFieldMark && (base.PreviousSibling as WFieldMark).Type == FieldMarkType.FieldSeparator)
		{
			return base.PreviousSibling.PreviousSibling is WOleObject;
		}
		return false;
	}

	internal void LoadImage(byte[] imageBytes, bool isMetafile)
	{
		if (imageBytes == null)
		{
			throw new ArgumentNullException("image");
		}
		if (isMetafile)
		{
			m_imageRecord = base.Document.Images.LoadMetaFileImage(imageBytes, isCompressed: false);
		}
		else
		{
			m_imageRecord = base.Document.Images.LoadImage(imageBytes);
		}
		if (m_imageRecord.ImageFormat == DocGen.DocIO.DLS.Entities.ImageFormat.Emf || m_imageRecord.ImageFormat == DocGen.DocIO.DLS.Entities.ImageFormat.Wmf)
		{
			m_imageRecord.IsMetafile = true;
		}
		imageBytes = null;
		m_size = new SizeF(float.MinValue, float.MinValue);
	}

	internal void LoadImage(ImageRecord imageRecord)
	{
		m_imageRecord = imageRecord;
		if (WordDocument.EnablePartialTrustCode)
		{
			DocGen.DocIO.DLS.Entities.Image imageForPartialTrustMode = GetImageForPartialTrustMode(imageRecord.ImageBytes);
			m_size = ConvertSizeForPartialTrustMode(imageForPartialTrustMode);
		}
		else
		{
			DocGen.DocIO.DLS.Entities.Image image = GetImage(imageRecord.ImageBytes, base.Document != null && !base.Document.IsOpening);
			m_size = ConvertSize(image);
		}
	}

	internal void CheckTextWrappingStyle()
	{
		if (!IsShape && TextWrappingStyle != 0 && base.OwnerParagraph != null && base.OwnerParagraph.OwnerTextBody != null && base.OwnerParagraph.OwnerTextBody.Owner != null && (base.OwnerParagraph.OwnerTextBody.Owner.EntityType == EntityType.TextBox || base.OwnerParagraph.OwnerTextBody.Owner.EntityType == EntityType.Shape))
		{
			SetTextWrappingStyleValue(TextWrappingStyle.Inline);
		}
	}

	private DocGen.DocIO.DLS.Entities.Image GetImageForPartialTrustMode(byte[] imageBytes)
	{
		DocGen.DocIO.DLS.Entities.Image result = null;
		if (imageBytes != null)
		{
			try
			{
				result = DocGen.DocIO.DLS.Entities.Image.FromStream(new MemoryStream(imageBytes));
				imageBytes = null;
			}
			catch
			{
				throw new ArgumentException("Argument is not image byte array");
			}
		}
		return result;
	}

	internal SizeF ConvertSizeForPartialTrustMode(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		m_imageRecord.Size = image.Size;
		m_imageRecord.ImageFormatForPartialTrustMode = image.RawFormat;
		UnitsConvertor instance = UnitsConvertor.Instance;
		if (image.IsMetafile && image.HorizontalDpi != 0L)
		{
			return instance.ConvertFromPixels(new SizeF(image.Size.Width, image.Size.Height), PrintUnits.Point, image.HorizontalDpi);
		}
		if (image.HorizontalDpi == 0L)
		{
			return instance.ConvertFromPixels(image.Size, PrintUnits.Point);
		}
		return instance.ConvertFromPixels(new SizeF(image.Size.Width, image.Size.Height), PrintUnits.Point, image.HorizontalDpi);
	}

	internal DocGen.DocIO.DLS.Entities.Image GetImage(byte[] imageBytes, bool isImageFromScratch)
	{
		DocGen.DocIO.DLS.Entities.Image image = null;
		if (imageBytes != null)
		{
			try
			{
				image = DocGen.DocIO.DLS.Entities.Image.FromStream(new MemoryStream(imageBytes));
				if (image.Format == DocGen.DocIO.DLS.Entities.ImageFormat.Emf || image.Format == DocGen.DocIO.DLS.Entities.ImageFormat.Wmf)
				{
					m_doc.SetTriggerElement(ref m_doc.m_notSupportedElementFlag, 34);
					Stream manifestResourceStream = GetManifestResourceStream("ImageNotFound.jpg");
					MemoryStream memoryStream = new MemoryStream();
					manifestResourceStream.CopyTo(memoryStream);
					image = DocGen.DocIO.DLS.Entities.Image.FromStream(memoryStream);
				}
				imageBytes = null;
			}
			catch
			{
				if (isImageFromScratch)
				{
					throw new ArgumentException("Argument is not image byte array");
				}
				Stream manifestResourceStream2 = GetManifestResourceStream("ImageNotFound.jpg");
				MemoryStream memoryStream2 = new MemoryStream();
				manifestResourceStream2.CopyTo(memoryStream2);
				image = DocGen.DocIO.DLS.Entities.Image.FromStream(memoryStream2);
				imageBytes = null;
			}
		}
		return image;
	}

	internal static Stream GetManifestResourceStream(string fileName)
	{
		Assembly assembly = typeof(WPicture).GetTypeInfo().Assembly;
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (text.EndsWith("." + fileName))
			{
				fileName = text;
				break;
			}
		}
		return assembly.GetManifestResourceStream(fileName);
	}

	private void ResetImageData()
	{
		if (m_inlinePictureShape.ShapeContainer != null && m_inlinePictureShape.ShapeContainer.Shape != null)
		{
			m_inlinePictureShape.ShapeContainer = null;
		}
		if (m_imageRecord != null)
		{
			m_imageRecord.OccurenceCount--;
			m_imageRecord = null;
		}
		m_size = new SizeF(float.MinValue, float.MinValue);
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
		IsWrappingBoundsAdded = false;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return dc.MeasureImage(this);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		WParagraph wParagraph = base.OwnerParagraph;
		if (base.Owner is InlineContentControl || base.Owner is XmlParagraphItem || base.Owner is GroupShape || base.Owner is ChildGroupShape)
		{
			wParagraph = GetOwnerParagraphValue();
		}
		Entity ownerEntity = wParagraph.GetOwnerEntity();
		if ((wParagraph.IsInCell && ((IWidget)wParagraph).LayoutInfo.IsClipped) || ownerEntity is Shape || ownerEntity is WTextBox || ownerEntity is ChildShape)
		{
			m_layoutInfo.IsClipped = true;
		}
		m_layoutInfo.IsVerticalText = ((IWidget)wParagraph).LayoutInfo.IsVerticalText;
		if (TextWrappingStyle != 0)
		{
			m_layoutInfo.IsSkipBottomAlign = true;
		}
		if (CharacterFormat.Hidden || (!Visible && GetTextWrappingStyle() != 0))
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
		if (m_embedBody != null)
		{
			m_embedBody.InitLayoutInfo(entity, ref isLastTOCEntry);
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

	internal SizeF ConvertSize(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		m_imageRecord.Size = image.Size;
		m_imageRecord.ImageFormat = image.RawFormat;
		UnitsConvertor instance = UnitsConvertor.Instance;
		if (image.HorizontalDpi == 0L)
		{
			return instance.ConvertFromPixels(image.Size, PrintUnits.Point);
		}
		return instance.ConvertFromPixels(new SizeF(image.Size.Width, image.Size.Height), PrintUnits.Point, image.HorizontalDpi);
	}

	private void CheckPicSizeForPartialTrustMode(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image != null)
		{
			m_size = ConvertSizeForPartialTrustMode(image);
		}
	}

	private void CheckPicSize(DocGen.DocIO.DLS.Entities.Image image)
	{
		if (image != null)
		{
			m_size = ConvertSize(image);
		}
	}
}
