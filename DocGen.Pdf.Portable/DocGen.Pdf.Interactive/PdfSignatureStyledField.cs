using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfSignatureStyledField : PdfField
{
	private const byte ShadowShift = 64;

	internal int m_angle;

	private WidgetAnnotation m_widget;

	private PdfFieldActions m_actions;

	private PdfTemplate m_appearanceTemplate;

	private PdfBrush m_backBrush;

	private PdfPen m_borderPen;

	private PdfBrush m_shadowBrush;

	private bool m_visible = true;

	private string m_name;

	internal bool m_containsBW;

	internal bool m_containsBG;

	public virtual RectangleF Bounds
	{
		get
		{
			return m_widget.Bounds;
		}
		set
		{
			m_widget.Bounds = value;
			NotifyPropertyChanged("Bounds");
		}
	}

	public new int RotationAngle
	{
		get
		{
			return m_widget.WidgetAppearance.RotationAngle;
		}
		set
		{
			m_angle = value;
			int num = 360;
			if (m_angle >= 360)
			{
				m_angle %= num;
			}
			if (m_angle < 45)
			{
				m_angle = 0;
			}
			else if (m_angle >= 45 && m_angle < 135)
			{
				m_angle = 90;
			}
			else if (m_angle >= 135 && m_angle < 225)
			{
				m_angle = 180;
			}
			else if (m_angle >= 225 && m_angle < 315)
			{
				m_angle = 270;
			}
			int angle = m_angle;
			m_widget.WidgetAppearance.RotationAngle = angle;
			NotifyPropertyChanged("RotationAngle");
		}
	}

	public PointF Location
	{
		get
		{
			return m_widget.Location;
		}
		set
		{
			m_widget.AssignLocation(value);
			NotifyPropertyChanged("Location");
		}
	}

	public SizeF Size
	{
		get
		{
			return m_widget.Size;
		}
		set
		{
			m_widget.AssignSize(value);
			NotifyPropertyChanged("Size");
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return m_widget.WidgetAppearance.BorderColor;
		}
		set
		{
			m_widget.WidgetAppearance.BorderColor = value;
			CreateBorderPen();
			NotifyPropertyChanged("BorderColor");
		}
	}

	public PdfColor BackColor
	{
		get
		{
			return m_widget.WidgetAppearance.BackColor;
		}
		set
		{
			m_widget.WidgetAppearance.BackColor = value;
			CreateBackBrush();
			m_containsBG = true;
			NotifyPropertyChanged("BackColor");
		}
	}

	public float BorderWidth
	{
		get
		{
			return m_widget.WidgetBorder.Width;
		}
		set
		{
			if (m_widget.WidgetBorder.Width != value)
			{
				m_widget.WidgetBorder.Width = value;
				CreateBorderPen();
			}
			m_containsBW = true;
			NotifyPropertyChanged("BorderWidth");
		}
	}

	public PdfHighlightMode HighlightMode
	{
		get
		{
			return m_widget.HighlightMode;
		}
		set
		{
			m_widget.HighlightMode = value;
			NotifyPropertyChanged("HighlightMode");
		}
	}

	public PdfFieldActions Actions
	{
		get
		{
			if (m_actions == null)
			{
				m_actions = new PdfFieldActions(Widget.Actions);
				base.Dictionary.SetProperty("AA", m_actions);
			}
			return m_actions;
		}
	}

	public PdfBorderStyle BorderStyle
	{
		get
		{
			return Widget.WidgetBorder.Style;
		}
		set
		{
			Widget.WidgetBorder.Style = value;
			CreateBorderPen();
			NotifyPropertyChanged("BorderStyle");
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
			if (m_visible != value && !value)
			{
				m_visible = value;
				m_widget.AnnotationFlags = PdfAnnotationFlags.Hidden;
			}
			NotifyPropertyChanged("Visible");
		}
	}

	internal PdfBrush ShadowBrush => m_shadowBrush;

	internal WidgetAnnotation Widget => m_widget;

	internal PdfTemplate AppearanceTemplate => m_appearanceTemplate;

	internal PdfBrush BackBrush => m_backBrush;

	internal PdfPen BorderPen => m_borderPen;

	public PdfSignatureStyledField(PdfPageBase page, string name)
		: base(page, name)
	{
		m_name = name;
		AddAnnotationToPage(page, Widget);
	}

	internal PdfSignatureStyledField()
	{
	}

	internal override void Draw()
	{
		RemoveAnnoationFromPage(Page, Widget);
	}

	internal void RemoveAnnoationFromPage(PdfPageBase page, PdfAnnotation widget)
	{
		if (page is PdfPage pdfPage)
		{
			pdfPage.Annotations.Remove(widget);
			return;
		}
		PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
		PdfDictionary dictionary = pdfLoadedPage.Dictionary;
		PdfArray pdfArray = null;
		pdfArray = ((!dictionary.ContainsKey("Annots")) ? new PdfArray() : (pdfLoadedPage.CrossTable.GetObject(dictionary["Annots"]) as PdfArray));
		widget.Dictionary.SetProperty("P", new PdfReferenceHolder(pdfLoadedPage));
		pdfArray.Remove(new PdfReferenceHolder(widget));
		page.Dictionary.SetProperty("Annots", pdfArray);
	}

	internal void AddAnnotationToPage(PdfPageBase page, PdfAnnotation widget)
	{
		if (page is PdfPage pdfPage)
		{
			widget.Dictionary.SetProperty("T", new PdfString(m_name));
			pdfPage.Annotations.Add(widget);
			return;
		}
		PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
		PdfDictionary dictionary = pdfLoadedPage.Dictionary;
		PdfArray pdfArray = null;
		if (dictionary.ContainsKey("Annots"))
		{
			pdfArray = pdfLoadedPage.CrossTable.GetObject(dictionary["Annots"]) as PdfArray;
			if (pdfArray == null)
			{
				pdfArray = new PdfArray();
			}
		}
		else
		{
			pdfArray = new PdfArray();
		}
		widget.Dictionary.SetProperty("P", new PdfReferenceHolder(pdfLoadedPage));
		if ((this as PdfSignatureField).m_fieldAutoNaming)
		{
			widget.Dictionary.SetProperty("T", new PdfString(m_name));
		}
		else
		{
			base.Dictionary.SetProperty("T", new PdfString(m_name));
		}
		pdfArray.Add(new PdfReferenceHolder(widget));
		page.Dictionary.SetProperty("Annots", pdfArray);
	}

	protected override void Initialize()
	{
		base.Initialize();
		bool fieldAutoNaming = (this as PdfSignatureField).m_fieldAutoNaming;
		m_widget = new WidgetAnnotation();
		if (fieldAutoNaming)
		{
			CreateBorderPen();
			CreateBackBrush();
			base.Dictionary = m_widget.Dictionary;
			m_widget.m_signatureField = this as PdfSignatureField;
		}
		else
		{
			m_widget.Parent = this;
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfReferenceHolder(m_widget));
			base.Dictionary.SetProperty("Kids", new PdfArray(pdfArray));
		}
		Widget.DefaultAppearance.FontName = "TiRo";
	}

	private void CreateBorderPen()
	{
		float width = m_widget.WidgetBorder.Width;
		m_borderPen = new PdfPen(m_widget.WidgetAppearance.BorderColor, width);
		if (Widget.WidgetBorder.Style == PdfBorderStyle.Dashed)
		{
			m_borderPen.DashStyle = PdfDashStyle.Custom;
			m_borderPen.DashPattern = new float[1] { 3f / width };
		}
	}

	private void CreateBackBrush()
	{
		m_backBrush = new PdfSolidBrush(m_widget.WidgetAppearance.BackColor);
		PdfColor color = new PdfColor(m_widget.WidgetAppearance.BackColor);
		color.R = (byte)((color.R - 64 >= 0) ? ((uint)(color.R - 64)) : 0u);
		color.G = (byte)((color.G - 64 >= 0) ? ((uint)(color.G - 64)) : 0u);
		color.B = (byte)((color.B - 64 >= 0) ? ((uint)(color.B - 64)) : 0u);
		m_shadowBrush = new PdfSolidBrush(color);
	}
}
