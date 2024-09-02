using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfStyledField : PdfField
{
	private const byte ShadowShift = 64;

	internal int m_angle;

	internal WidgetAnnotation m_widget;

	private PdfFont m_font;

	private PdfFieldActions m_actions;

	private PdfTemplate m_appearanceTemplate;

	private PdfBrush m_backBrush;

	private PdfColor m_backRectColor = PdfColor.Empty;

	private PdfBrush m_foreBrush;

	private PdfPen m_borderPen;

	private PdfStringFormat m_stringFormat;

	private PdfBrush m_shadowBrush;

	private bool m_visible = true;

	private PdfFormFieldVisibility m_visibility;

	internal PdfArray m_array = new PdfArray();

	internal List<PdfField> fieldItems = new List<PdfField>();

	internal bool m_isBCSet;

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

	public PdfFormFieldVisibility Visibility
	{
		get
		{
			return m_visibility;
		}
		set
		{
			m_visibility = value;
			SetVisibility();
			NotifyPropertyChanged("Visibility");
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
			m_isBCSet = true;
			NotifyPropertyChanged("BackColor");
		}
	}

	internal PdfColor BackRectColor
	{
		get
		{
			return m_backRectColor;
		}
		set
		{
			m_backRectColor = value;
		}
	}

	public PdfColor ForeColor
	{
		get
		{
			return m_widget.DefaultAppearance.ForeColor;
		}
		set
		{
			m_widget.DefaultAppearance.ForeColor = value;
			m_foreBrush = new PdfSolidBrush(value);
			NotifyPropertyChanged("ForeColor");
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
				if (value == 0f)
				{
					m_widget.WidgetAppearance.BorderColor = new PdfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);
				}
				else
				{
					CreateBorderPen();
				}
			}
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

	public PdfFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Font");
			}
			if (m_font != value)
			{
				m_font = value;
				DefineDefaultAppearance();
				NotifyPropertyChanged("Font");
			}
		}
	}

	public PdfTextAlignment TextAlignment
	{
		get
		{
			return Widget.TextAlignment;
		}
		set
		{
			if (Widget.TextAlignment != value)
			{
				Widget.TextAlignment = value;
				m_stringFormat = new PdfStringFormat(value, PdfVerticalAlignment.Middle);
			}
			NotifyPropertyChanged("TextAlignment");
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

	internal PdfBrush ForeBrush => m_foreBrush;

	internal PdfStringFormat StringFormat
	{
		get
		{
			if (m_stringFormat != null && (base.ComplexScript || (Form != null && Form.ComplexScript)))
			{
				m_stringFormat.ComplexScript = true;
			}
			return m_stringFormat;
		}
	}

	public PdfStyledField(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal PdfStyledField()
	{
	}

	private void SetVisibility()
	{
		switch (m_visibility)
		{
		case PdfFormFieldVisibility.Hidden:
			m_widget.AnnotationFlags = PdfAnnotationFlags.Hidden;
			break;
		case PdfFormFieldVisibility.HiddenPrintable:
			m_widget.AnnotationFlags = PdfAnnotationFlags.Print | PdfAnnotationFlags.NoView;
			break;
		case PdfFormFieldVisibility.Visible:
			m_widget.AnnotationFlags = PdfAnnotationFlags.Print;
			break;
		case PdfFormFieldVisibility.VisibleNotPrintable:
			m_widget.AnnotationFlags = PdfAnnotationFlags.Default;
			break;
		}
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
			pdfPage.Annotations.Add(widget);
			return;
		}
		PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
		PdfDictionary dictionary = pdfLoadedPage.Dictionary;
		PdfArray pdfArray = null;
		pdfArray = ((!dictionary.ContainsKey("Annots")) ? new PdfArray() : (pdfLoadedPage.CrossTable.GetObject(dictionary["Annots"]) as PdfArray));
		widget.Dictionary.SetProperty("P", new PdfReferenceHolder(pdfLoadedPage));
		pdfArray.Add(new PdfReferenceHolder(widget));
		page.Dictionary.SetProperty("Annots", pdfArray);
	}

	protected PdfFont ObtainFont()
	{
		if (m_font != null)
		{
			return m_font;
		}
		return PdfDocument.DefaultFont;
	}

	protected override void Initialize()
	{
		base.Initialize();
		m_widget = new WidgetAnnotation();
		m_widget.Parent = this;
		m_foreBrush = new PdfSolidBrush(m_widget.DefaultAppearance.ForeColor);
		m_stringFormat = new PdfStringFormat(Widget.TextAlignment, PdfVerticalAlignment.Middle);
		CreateBorderPen();
		CreateBackBrush();
		PdfArray pdfArray = new PdfArray();
		pdfArray.Add(new PdfReferenceHolder(m_widget));
		base.Dictionary.SetProperty("Kids", new PdfArray(pdfArray));
		Widget.DefaultAppearance.FontName = "TiRo";
	}

	protected override void DefineDefaultAppearance()
	{
		if (Form != null && m_font != null)
		{
			PdfName name = Form.Resources.GetName(m_font);
			m_widget.DefaultAppearance.FontName = name.Value;
			m_widget.DefaultAppearance.FontSize = m_font.Size;
		}
		else if (m_font != null)
		{
			Widget.DefaultAppearance.FontName = m_font.Name;
			Widget.DefaultAppearance.FontSize = m_font.Size;
		}
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
