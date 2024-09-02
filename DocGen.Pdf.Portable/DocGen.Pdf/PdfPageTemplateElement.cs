using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public class PdfPageTemplateElement
{
	private bool m_foreground;

	private PdfDockStyle m_dockStyle;

	private PdfAlignmentStyle m_alignmentStyle;

	private PdfTemplate m_template;

	private TemplateType m_type;

	private PointF m_location;

	private PdfTag m_tag;

	public PdfDockStyle Dock
	{
		get
		{
			return m_dockStyle;
		}
		set
		{
			if (m_dockStyle != value && Type == TemplateType.None)
			{
				m_dockStyle = value;
				ResetAlignment();
			}
		}
	}

	public PdfAlignmentStyle Alignment
	{
		get
		{
			return m_alignmentStyle;
		}
		set
		{
			if (m_alignmentStyle != value)
			{
				AssignAlignment(value);
			}
		}
	}

	public bool Foreground
	{
		get
		{
			return m_foreground;
		}
		set
		{
			if (m_foreground != value)
			{
				m_foreground = value;
			}
		}
	}

	public bool Background
	{
		get
		{
			return !m_foreground;
		}
		set
		{
			m_foreground = !value;
		}
	}

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			if (Type == TemplateType.None)
			{
				m_location = value;
			}
		}
	}

	public float X
	{
		get
		{
			return m_location.X;
		}
		set
		{
			if (Type == TemplateType.None)
			{
				m_location.X = value;
			}
		}
	}

	public float Y
	{
		get
		{
			return m_location.Y;
		}
		set
		{
			if (Type == TemplateType.None)
			{
				m_location.Y = value;
			}
		}
	}

	public SizeF Size
	{
		get
		{
			return m_template.Size;
		}
		set
		{
			if (m_template.Size != value && Type == TemplateType.None)
			{
				m_template.Reset(value);
			}
		}
	}

	public float Width
	{
		get
		{
			return m_template.Width;
		}
		set
		{
			if (m_template.Width != value && Type == TemplateType.None)
			{
				SizeF size = m_template.Size;
				size.Width = value;
				m_template.Reset(size);
			}
		}
	}

	public float Height
	{
		get
		{
			return m_template.Height;
		}
		set
		{
			if (m_template.Height != value && Type == TemplateType.None)
			{
				SizeF size = m_template.Size;
				size.Height = value;
				m_template.Reset(size);
			}
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return new RectangleF(Location, Size);
		}
		set
		{
			if (Type == TemplateType.None)
			{
				Location = value.Location;
				Size = value.Size;
			}
		}
	}

	public PdfGraphics Graphics => Template.Graphics;

	internal PdfTemplate Template
	{
		get
		{
			if (m_template == null)
			{
				m_template = new PdfTemplate(Size);
			}
			return m_template;
		}
	}

	internal TemplateType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			if (m_type != value)
			{
				UpdateDocking(value);
				m_type = value;
			}
		}
	}

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
			m_template.Graphics.Tag = m_tag;
		}
	}

	public PdfPageTemplateElement(RectangleF bounds)
		: this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
	{
	}

	public PdfPageTemplateElement(RectangleF bounds, PdfPage page)
		: this(bounds.X, bounds.Y, bounds.Width, bounds.Height, page)
	{
	}

	public PdfPageTemplateElement(PointF location, SizeF size)
		: this(location.X, location.Y, size.Width, size.Height)
	{
	}

	public PdfPageTemplateElement(PointF location, SizeF size, PdfPage page)
		: this(location.X, location.Y, size.Width, size.Height, page)
	{
	}

	public PdfPageTemplateElement(SizeF size)
		: this(size.Width, size.Height)
	{
	}

	public PdfPageTemplateElement(float width, float height)
		: this(0f, 0f, width, height)
	{
	}

	public PdfPageTemplateElement(float width, float height, PdfPage page)
		: this(0f, 0f, width, height, page)
	{
	}

	public PdfPageTemplateElement(float x, float y, float width, float height)
	{
		X = x;
		Y = Y;
		m_template = new PdfTemplate(width, height);
	}

	public PdfPageTemplateElement(float x, float y, float width, float height, PdfPage page)
	{
		X = x;
		Y = Y;
		m_template = new PdfTemplate(width, height);
		Graphics.ColorSpace = page.Document.ColorSpace;
	}

	internal void Draw(PdfPageLayer layer, PdfDocument document)
	{
		if (layer == null)
		{
			throw new ArgumentNullException("layer");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		PdfPage page = layer.Page as PdfPage;
		RectangleF rectangleF = CalculateBounds(page, document);
		if (PdfTag != null && Template.Graphics.Tag == null)
		{
			Template.Graphics.Tag = PdfTag;
		}
		else if (document.AutoTag && PdfTag == null && Template.Graphics.Tag == null)
		{
			Template.Graphics.Tag = new PdfStructureElement(PdfTagType.Figure);
		}
		if (document.Template.blinkMargin == null)
		{
			layer.Graphics.DrawPdfTemplate(Template, rectangleF.Location, rectangleF.Size);
			return;
		}
		PdfMargins blinkMargin = document.Template.blinkMargin;
		float x = rectangleF.X + blinkMargin.Left;
		float y = rectangleF.Y;
		float width = rectangleF.Size.Width - blinkMargin.Left - blinkMargin.Right;
		float height = rectangleF.Size.Height;
		y = ((Dock != PdfDockStyle.Top) ? (y - blinkMargin.Bottom) : (y + blinkMargin.Top));
		layer.Graphics.DrawPdfTemplate(Template, new PointF(x, y), new SizeF(width, height));
	}

	private void UpdateDocking(TemplateType type)
	{
		switch (type)
		{
		case TemplateType.Top:
			Dock = PdfDockStyle.Top;
			break;
		case TemplateType.Bottom:
			Dock = PdfDockStyle.Bottom;
			break;
		case TemplateType.Left:
			Dock = PdfDockStyle.Left;
			break;
		case TemplateType.Right:
			Dock = PdfDockStyle.Right;
			break;
		case TemplateType.None:
			return;
		}
		ResetAlignment();
	}

	private void ResetAlignment()
	{
		Alignment = PdfAlignmentStyle.None;
	}

	private void AssignAlignment(PdfAlignmentStyle alignment)
	{
		if (Dock == PdfDockStyle.None)
		{
			m_alignmentStyle = alignment;
			return;
		}
		bool flag = false;
		switch (Dock)
		{
		case PdfDockStyle.Left:
			flag = alignment == PdfAlignmentStyle.TopLeft || alignment == PdfAlignmentStyle.MiddleLeft || alignment == PdfAlignmentStyle.BottomLeft || alignment == PdfAlignmentStyle.None;
			break;
		case PdfDockStyle.Top:
			flag = alignment == PdfAlignmentStyle.TopLeft || alignment == PdfAlignmentStyle.TopCenter || alignment == PdfAlignmentStyle.TopRight || alignment == PdfAlignmentStyle.None;
			break;
		case PdfDockStyle.Right:
			flag = alignment == PdfAlignmentStyle.TopRight || alignment == PdfAlignmentStyle.MiddleRight || alignment == PdfAlignmentStyle.BottomRight || alignment == PdfAlignmentStyle.None;
			break;
		case PdfDockStyle.Bottom:
			flag = alignment == PdfAlignmentStyle.BottomLeft || alignment == PdfAlignmentStyle.BottomCenter || alignment == PdfAlignmentStyle.BottomRight || alignment == PdfAlignmentStyle.None;
			break;
		case PdfDockStyle.Fill:
			flag = alignment == PdfAlignmentStyle.MiddleCenter || alignment == PdfAlignmentStyle.None;
			break;
		}
		if (flag)
		{
			m_alignmentStyle = alignment;
		}
	}

	private RectangleF CalculateBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		RectangleF result = Bounds;
		if (m_alignmentStyle != 0)
		{
			result = GetAlignmentBounds(page, document);
		}
		else if (m_dockStyle != 0)
		{
			result = GetDockBounds(page, document);
		}
		return result;
	}

	private RectangleF GetAlignmentBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		RectangleF bounds = Bounds;
		if (Type == TemplateType.None)
		{
			return GetSimpleAlignmentBounds(page, document);
		}
		return GetTemplateAlignmentBounds(page, document);
	}

	private RectangleF GetSimpleAlignmentBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		RectangleF bounds = Bounds;
		RectangleF actualBounds = page.Section.GetActualBounds(document, page, includeMargins: false);
		float x = X;
		float y = Y;
		switch (m_alignmentStyle)
		{
		case PdfAlignmentStyle.TopLeft:
			x = 0f;
			y = 0f;
			break;
		case PdfAlignmentStyle.TopCenter:
			x = (actualBounds.Width - Width) / 2f;
			y = 0f;
			break;
		case PdfAlignmentStyle.TopRight:
			x = actualBounds.Width - Width;
			y = 0f;
			break;
		case PdfAlignmentStyle.MiddleLeft:
			x = 0f;
			y = (actualBounds.Height - Height) / 2f;
			break;
		case PdfAlignmentStyle.MiddleCenter:
			x = (actualBounds.Width - Width) / 2f;
			y = (actualBounds.Height - Height) / 2f;
			break;
		case PdfAlignmentStyle.MiddleRight:
			x = actualBounds.Width - Width;
			y = (actualBounds.Height - Height) / 2f;
			break;
		case PdfAlignmentStyle.BottomLeft:
			x = 0f;
			y = actualBounds.Height - Height;
			break;
		case PdfAlignmentStyle.BottomCenter:
			x = (actualBounds.Width - Width) / 2f;
			y = actualBounds.Height - Height;
			break;
		case PdfAlignmentStyle.BottomRight:
			x = actualBounds.Width - Width;
			y = actualBounds.Height - Height;
			break;
		}
		bounds.X = x;
		bounds.Y = y;
		return bounds;
	}

	private RectangleF GetTemplateAlignmentBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		RectangleF bounds = Bounds;
		PdfSection section = page.Section;
		RectangleF actualBounds = section.GetActualBounds(document, page, includeMargins: false);
		float x = X;
		float y = Y;
		switch (m_alignmentStyle)
		{
		case PdfAlignmentStyle.TopLeft:
			if (Type == TemplateType.Left)
			{
				x = 0f - actualBounds.X;
				y = 0f;
			}
			else if (Type == TemplateType.Top)
			{
				x = 0f - actualBounds.X;
				y = 0f - actualBounds.Y;
			}
			break;
		case PdfAlignmentStyle.TopCenter:
			x = (actualBounds.Width - Width) / 2f;
			y = 0f - actualBounds.Y;
			break;
		case PdfAlignmentStyle.TopRight:
			if (Type == TemplateType.Right)
			{
				x = actualBounds.Width + section.GetRightIndentWidth(document, page, includeMargins: false) - Width;
				y = 0f;
			}
			else if (Type == TemplateType.Top)
			{
				x = actualBounds.Width + section.GetRightIndentWidth(document, page, includeMargins: false) - Width;
				y = 0f - actualBounds.Y;
			}
			break;
		case PdfAlignmentStyle.MiddleLeft:
			x = 0f - actualBounds.X;
			y = (actualBounds.Height - Height) / 2f;
			break;
		case PdfAlignmentStyle.MiddleCenter:
			x = (actualBounds.Width - Width) / 2f;
			y = (actualBounds.Height - Height) / 2f;
			break;
		case PdfAlignmentStyle.MiddleRight:
			x = actualBounds.Width + section.GetRightIndentWidth(document, page, includeMargins: false) - Width;
			y = (actualBounds.Height - Height) / 2f;
			break;
		case PdfAlignmentStyle.BottomLeft:
			if (Type == TemplateType.Left)
			{
				x = 0f - actualBounds.X;
				y = actualBounds.Height - Height;
			}
			else if (Type == TemplateType.Bottom)
			{
				x = 0f - actualBounds.X;
				y = actualBounds.Height + section.GetBottomIndentHeight(document, page, includeMargins: false) - Height;
			}
			break;
		case PdfAlignmentStyle.BottomCenter:
			x = (actualBounds.Width - Width) / 2f;
			y = actualBounds.Height + section.GetBottomIndentHeight(document, page, includeMargins: false) - Height;
			break;
		case PdfAlignmentStyle.BottomRight:
			if (Type == TemplateType.Right)
			{
				x = actualBounds.Width + section.GetRightIndentWidth(document, page, includeMargins: false) - Width;
				y = actualBounds.Height - Height;
			}
			else if (Type == TemplateType.Bottom)
			{
				x = actualBounds.Width + section.GetRightIndentWidth(document, page, includeMargins: false) - Width;
				y = actualBounds.Height + section.GetBottomIndentHeight(document, page, includeMargins: false) - Height;
			}
			break;
		}
		bounds.X = x;
		bounds.Y = y;
		return bounds;
	}

	private RectangleF GetDockBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		RectangleF bounds = Bounds;
		if (Type == TemplateType.None)
		{
			return GetSimpleDockBounds(page, document);
		}
		return GetTemplateDockBounds(page, document);
	}

	private RectangleF GetSimpleDockBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		_ = Bounds;
		RectangleF actualBounds = page.Section.GetActualBounds(document, page, includeMargins: false);
		float x = X;
		float y = Y;
		float width = Width;
		float height = Height;
		switch (m_dockStyle)
		{
		case PdfDockStyle.Left:
			x = 0f;
			y = 0f;
			width = Width;
			height = actualBounds.Height;
			break;
		case PdfDockStyle.Top:
			x = 0f;
			y = 0f;
			width = actualBounds.Width;
			height = Height;
			break;
		case PdfDockStyle.Right:
			x = actualBounds.Width - Width;
			y = 0f;
			width = Width;
			height = actualBounds.Height;
			break;
		case PdfDockStyle.Bottom:
			x = 0f;
			y = actualBounds.Height - Height;
			width = actualBounds.Width;
			height = Height;
			break;
		case PdfDockStyle.Fill:
			x = 0f;
			x = 0f;
			width = actualBounds.Width;
			height = actualBounds.Height;
			break;
		}
		return new RectangleF(x, y, width, height);
	}

	private RectangleF GetTemplateDockBounds(PdfPage page, PdfDocument document)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		_ = Bounds;
		PdfSection section = page.Section;
		RectangleF actualBounds = section.GetActualBounds(document, page, includeMargins: false);
		SizeF actualSize = section.PageSettings.GetActualSize();
		float x = X;
		float num = Y;
		float width = Width;
		float height = Height;
		switch (m_dockStyle)
		{
		case PdfDockStyle.Left:
			x = 0f - actualBounds.X;
			num = 0f;
			width = Width;
			height = actualBounds.Height;
			break;
		case PdfDockStyle.Top:
			x = 0f - actualBounds.X;
			num = 0f - actualBounds.Y;
			width = actualSize.Width;
			height = Height;
			if (actualBounds.Height < 0f)
			{
				num = 0f - actualBounds.Y + actualSize.Height;
			}
			break;
		case PdfDockStyle.Right:
			x = actualBounds.Width + section.GetRightIndentWidth(document, page, includeMargins: false) - Width;
			num = 0f;
			width = Width;
			height = actualBounds.Height;
			break;
		case PdfDockStyle.Bottom:
			x = 0f - actualBounds.X;
			num = actualBounds.Height + section.GetBottomIndentHeight(document, page, includeMargins: false) - Height;
			width = actualSize.Width;
			height = Height;
			if (actualBounds.Height < 0f)
			{
				num -= actualSize.Height;
			}
			break;
		case PdfDockStyle.Fill:
			x = 0f;
			x = 0f;
			width = actualBounds.Width;
			height = actualBounds.Height;
			break;
		}
		return new RectangleF(x, num, width, height);
	}
}
