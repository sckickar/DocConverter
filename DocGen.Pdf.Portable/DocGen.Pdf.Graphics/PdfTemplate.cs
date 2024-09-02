using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public class PdfTemplate : PdfShapeElement, IPdfWrapper
{
	private PdfGraphics m_graphics;

	internal PdfStream m_content;

	internal PdfResources m_resources;

	private SizeF m_size;

	private bool m_bIsReadonly;

	internal bool m_writeTransformation = true;

	internal bool isLoadedPageTemplate;

	internal bool isCustomStamp;

	internal bool isContainPageRotation;

	private string m_customPdfTemplateName;

	private bool m_isSignatureAppearanceValidation;

	private bool m_isAnnotationTemplate;

	private bool m_isScaleAnnotation;

	internal PointF m_origin = new PointF(0f, 0f);

	internal bool isCropBox;

	internal bool m_isSignatureAppearance;

	private SizeF m_originalSize;

	public PdfGraphics Graphics
	{
		get
		{
			if (m_bIsReadonly)
			{
				m_graphics = null;
			}
			else if (m_graphics == null)
			{
				PdfGraphics.GetResources resources = GetResources;
				m_graphics = new PdfGraphics(Size, resources, m_content);
				if (m_writeTransformation)
				{
					m_graphics.InitializeCoordinates();
				}
				if (base.PdfTag != null)
				{
					m_graphics.Tag = base.PdfTag;
				}
				m_graphics.IsTemplateGraphics = true;
			}
			return m_graphics;
		}
	}

	public SizeF Size => m_size;

	public float Width => Size.Width;

	public float Height => Size.Height;

	public bool ReadOnly => m_bIsReadonly;

	internal string CustomPdfTemplateName
	{
		get
		{
			return m_customPdfTemplateName;
		}
		set
		{
			if (value != null)
			{
				m_customPdfTemplateName = value;
			}
		}
	}

	internal bool IsSignatureAppearanceValidation
	{
		get
		{
			return m_isSignatureAppearanceValidation;
		}
		set
		{
			m_isSignatureAppearanceValidation = value;
		}
	}

	internal SizeF OriginalSize => m_originalSize;

	IPdfPrimitive IPdfWrapper.Element => m_content;

	internal bool IsAnnotationTemplate
	{
		get
		{
			return m_isAnnotationTemplate;
		}
		set
		{
			m_isAnnotationTemplate = value;
		}
	}

	internal bool NeedScaling
	{
		get
		{
			return m_isScaleAnnotation;
		}
		set
		{
			m_isScaleAnnotation = value;
		}
	}

	public PdfTemplate(SizeF size)
		: this(size.Width, size.Height)
	{
	}

	public PdfTemplate(RectangleF rect)
		: this(rect.X, rect.Y, rect.Width, rect.Height)
	{
	}

	internal PdfTemplate(SizeF size, bool writeTransformation)
		: this(size.Width, size.Height)
	{
		m_writeTransformation = writeTransformation;
	}

	public PdfTemplate(float width, float height)
	{
		m_content = new PdfStream();
		SetSize(new SizeF(width, height));
		Initialize();
	}

	public PdfTemplate(float x, float y, float width, float height)
	{
		m_content = new PdfStream();
		RectangleF bounds = new RectangleF(x, y, width, height);
		SetBounds(bounds);
		Initialize();
	}

	internal PdfTemplate(PointF origin, SizeF size, MemoryStream stream, PdfDictionary resources, bool isLoadedPage, PdfPageBase page)
	{
		if (size == SizeF.Empty)
		{
			throw new ArgumentException("The size of the new PdfTemplate can't be empty.");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_content = new PdfStream();
		PdfLoadedPage pdfLoadedPage = null;
		if (isLoadedPage)
		{
			pdfLoadedPage = page as PdfLoadedPage;
		}
		if (pdfLoadedPage != null && (pdfLoadedPage.CropBox.X > 0f || pdfLoadedPage.CropBox.Y > 0f))
		{
			RectangleF cropBox = pdfLoadedPage.GetCropBox();
			PdfArray value = PdfArray.FromRectangle(cropBox);
			m_content["BBox"] = value;
			isCropBox = true;
			if (pdfLoadedPage.CropBox.X > 0f && pdfLoadedPage.CropBox.Y > 0f)
			{
				m_origin = new PointF(cropBox.X, cropBox.Y);
			}
			SetSize(m_origin, cropBox.Size);
		}
		else if (pdfLoadedPage != null && (pdfLoadedPage.MediaBox.X > 0f || pdfLoadedPage.MediaBox.Y > 0f))
		{
			if (pdfLoadedPage.MediaBox.X < pdfLoadedPage.MediaBox.Y)
			{
				RectangleF mediaBox = pdfLoadedPage.MediaBox;
				PdfArray value2 = PdfArray.FromRectangle(mediaBox);
				m_content["BBox"] = value2;
				isCropBox = true;
				if (pdfLoadedPage.MediaBox.X > 0f || pdfLoadedPage.MediaBox.Y > 0f)
				{
					m_origin = new PointF(mediaBox.X, mediaBox.Y);
				}
			}
			else
			{
				SetSize(size);
			}
		}
		else if (origin.X < 0f || origin.Y < 0f)
		{
			SetSize(origin, size);
		}
		else
		{
			SetSize(size);
		}
		Initialize();
		stream.WriteTo(m_content.InternalStream);
		if (resources != null)
		{
			m_content["Resources"] = new PdfDictionary(resources);
			m_resources = new PdfResources(resources);
		}
		isLoadedPageTemplate = isLoadedPage;
		m_bIsReadonly = true;
	}

	internal PdfTemplate(SizeF size, MemoryStream stream, PdfDictionary resources)
	{
		if (size == SizeF.Empty)
		{
			throw new ArgumentException("The size of the new PdfTemplate can't be empty.");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_content = new PdfStream();
		SetSize(size);
		Initialize();
		stream.WriteTo(m_content.InternalStream);
		if (resources != null)
		{
			m_content["Resources"] = new PdfDictionary(resources);
			m_resources = new PdfResources(resources);
		}
		m_bIsReadonly = true;
	}

	internal PdfTemplate(PdfStream template)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template");
		}
		m_content = template;
		if (!template.ContainsKey("Type") || !template.ContainsKey("Subtype"))
		{
			Initialize();
		}
		IPdfPrimitive pdfPrimitive = PdfCrossTable.Dereference(m_content["BBox"]);
		RectangleF rectangleF = default(RectangleF);
		if (pdfPrimitive != null && pdfPrimitive is PdfArray { Count: >3 } pdfArray)
		{
			rectangleF = pdfArray.ToRectangle();
		}
		m_size = rectangleF.Size;
		m_bIsReadonly = true;
	}

	internal PdfTemplate(PdfStream template, bool isTransformBBox)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template not to be null value");
		}
		m_content = template;
		RectangleF bBoxValue = RectangleF.Empty;
		if (PdfCrossTable.Dereference(template["BBox"]) is PdfArray pdfArray)
		{
			bBoxValue = pdfArray.ToRectangle();
			m_originalSize = bBoxValue.Size;
		}
		if (isTransformBBox)
		{
			if (PdfCrossTable.Dereference(template["Matrix"]) is PdfArray pdfArray2)
			{
				float[] array = new float[pdfArray2.Count];
				for (int i = 0; i < pdfArray2.Count; i++)
				{
					if (pdfArray2.Elements[i] is PdfNumber pdfNumber)
					{
						array[i] = pdfNumber.FloatValue;
					}
				}
				m_size = TransformBBoxByMatrix(bBoxValue, array).Size;
			}
			else
			{
				m_size = bBoxValue.Size;
			}
		}
		else
		{
			m_size = bBoxValue.Size;
		}
		m_bIsReadonly = true;
	}

	private PdfTemplate()
	{
		m_content = new PdfStream();
	}

	public void Reset(SizeF size)
	{
		SetSize(size);
		Reset();
	}

	public void Reset()
	{
		if (m_resources != null)
		{
			m_resources = null;
			m_content.Remove("Resources");
		}
		if (m_graphics != null)
		{
			m_graphics.Reset(Size);
		}
	}

	internal static PdfTemplate FromJson(string jsonData)
	{
		IPdfPrimitive appearanceStreamFromJson = new JsonParser(null, null).GetAppearanceStreamFromJson(jsonData);
		if (appearanceStreamFromJson == null || !(appearanceStreamFromJson is PdfStream))
		{
			return null;
		}
		return new PdfTemplate(appearanceStreamFromJson as PdfStream, isTransformBBox: true);
	}

	protected override RectangleF GetBoundsInternal()
	{
		return new RectangleF(PointF.Empty, Size);
	}

	protected override void DrawInternal(PdfGraphics graphics)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		graphics.DrawPdfTemplate(this, PointF.Empty);
	}

	private void Initialize()
	{
		AddType();
		AddSubType();
	}

	private PdfResources GetResources()
	{
		if (m_resources == null)
		{
			m_resources = new PdfResources();
			m_content["Resources"] = m_resources;
		}
		return m_resources;
	}

	private void AddType()
	{
		PdfName name = m_content.GetName("XObject");
		m_content["Type"] = name;
	}

	private void AddSubType()
	{
		PdfName name = m_content.GetName("Form");
		m_content["Subtype"] = name;
	}

	private void SetSize(SizeF size)
	{
		PdfArray value = PdfArray.FromRectangle(new RectangleF(PointF.Empty, size));
		m_content["BBox"] = value;
		m_size = size;
	}

	private void SetBounds(RectangleF bounds)
	{
		PdfArray value = PdfArray.FromRectangle(new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height));
		m_content["BBox"] = value;
		m_size = bounds.Size;
	}

	private void SetSize(PointF origin, SizeF size)
	{
		PdfArray value = new PdfArray(new float[4] { origin.X, origin.Y, size.Width, size.Height });
		m_content["BBox"] = value;
		m_size = size;
	}

	internal void CloneResources(PdfCrossTable crossTable)
	{
		if (m_resources != null && crossTable != null)
		{
			List<PdfReference> prevReference = crossTable.PrevReference;
			crossTable.PrevReference = new List<PdfReference>();
			PdfDictionary pdfDictionary = m_resources.Clone(crossTable) as PdfDictionary;
			crossTable.PrevReference.AddRange(prevReference);
			m_resources = new PdfResources(pdfDictionary);
			m_content["Resources"] = pdfDictionary;
		}
	}

	internal void Clear(string Data)
	{
		m_content.Clear();
		m_content.Write(Data);
	}

	private RectangleF TransformBBoxByMatrix(RectangleF bBoxValue, float[] matrix)
	{
		float[] array = new float[4];
		float[] array2 = new float[4];
		PointF pointF = TransformPoint(bBoxValue.Left, bBoxValue.Bottom, matrix);
		array[0] = pointF.X;
		array2[0] = pointF.Y;
		PointF pointF2 = TransformPoint(bBoxValue.Right, bBoxValue.Top, matrix);
		array[1] = pointF2.X;
		array2[1] = pointF2.Y;
		PointF pointF3 = TransformPoint(bBoxValue.Left, bBoxValue.Top, matrix);
		array[2] = pointF3.X;
		array2[2] = pointF3.Y;
		PointF pointF4 = TransformPoint(bBoxValue.Right, bBoxValue.Bottom, matrix);
		array[3] = pointF4.X;
		array2[3] = pointF4.Y;
		return new RectangleF(MinValue(array), MinValue(array2), MaxValue(array), MaxValue(array2));
	}

	private float MaxValue(float[] value)
	{
		float num = value[0];
		for (int i = 1; i < value.Length; i++)
		{
			if (value[i] > num)
			{
				num = value[i];
			}
		}
		return num;
	}

	private float MinValue(float[] value)
	{
		float num = value[0];
		for (int i = 1; i < value.Length; i++)
		{
			if (value[i] < num)
			{
				num = value[i];
			}
		}
		return num;
	}

	private PointF TransformPoint(float x, float y, float[] matrix)
	{
		PointF result = default(PointF);
		result.X = matrix[0] * x + matrix[2] * y + matrix[4];
		result.Y = matrix[1] * x + matrix[3] * y + matrix[5];
		return result;
	}

	internal string GetAppearanceAsJson()
	{
		return new JsonDocument(string.Empty).GetJsonAppearanceString(m_content);
	}
}
