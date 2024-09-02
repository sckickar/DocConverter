using System;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class WidgetAnnotation : PdfAnnotation
{
	private PdfField m_parent;

	private PdfExtendedAppearance m_extendedAppearance;

	private WidgetBorder m_border = new WidgetBorder();

	private WidgetAppearance m_widgetAppearance = new WidgetAppearance();

	private PdfHighlightMode m_highlightMode = PdfHighlightMode.Invert;

	private PdfDefaultAppearance m_defaultAppearance;

	private PdfTextAlignment m_alignment;

	private PdfAnnotationActions m_actions;

	private new PdfAppearance m_appearance;

	private string m_appearanceState;

	internal bool isAutoResize;

	internal PdfSignatureField m_signatureField;

	public PdfField Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			if (m_parent != value)
			{
				m_parent = value;
				if (m_parent != null)
				{
					base.Dictionary.SetProperty("Parent", new PdfReferenceHolder(m_parent));
				}
				else
				{
					base.Dictionary.Remove("Parent");
				}
			}
			NotifyPropertyChanged("Parent");
		}
	}

	public PdfExtendedAppearance ExtendedAppearance
	{
		get
		{
			if (m_extendedAppearance == null)
			{
				m_extendedAppearance = new PdfExtendedAppearance();
			}
			return m_extendedAppearance;
		}
		set
		{
			m_extendedAppearance = value;
			NotifyPropertyChanged("ExtendedAppearance");
		}
	}

	public PdfDefaultAppearance DefaultAppearance
	{
		get
		{
			if (m_defaultAppearance == null)
			{
				m_defaultAppearance = new PdfDefaultAppearance();
			}
			return m_defaultAppearance;
		}
	}

	public WidgetBorder WidgetBorder => m_border;

	public WidgetAppearance WidgetAppearance => m_widgetAppearance;

	public PdfHighlightMode HighlightMode
	{
		get
		{
			return m_highlightMode;
		}
		set
		{
			m_highlightMode = value;
			base.Dictionary.SetName("H", HighlightModeToString(m_highlightMode));
			NotifyPropertyChanged("HighlightMode");
		}
	}

	public PdfTextAlignment TextAlignment
	{
		get
		{
			return m_alignment;
		}
		set
		{
			if (m_alignment != value)
			{
				m_alignment = value;
				base.Dictionary.SetProperty("Q", new PdfNumber((int)m_alignment));
			}
			NotifyPropertyChanged("TextAlignment");
		}
	}

	public PdfAnnotationActions Actions
	{
		get
		{
			if (m_actions == null)
			{
				m_actions = new PdfAnnotationActions();
				base.Dictionary.SetProperty("AA", m_actions);
			}
			return m_actions;
		}
	}

	public new PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
			}
			return m_appearance;
		}
		set
		{
			if (m_appearance != value)
			{
				m_appearance = value;
			}
			NotifyPropertyChanged("Appearance");
		}
	}

	internal string AppearanceState
	{
		get
		{
			return m_appearanceState;
		}
		set
		{
			if (m_appearanceState != value)
			{
				m_appearanceState = value;
				base.Dictionary.SetName("AS", value);
			}
			NotifyPropertyChanged("AppearanceState");
		}
	}

	internal event EventHandler BeginSave;

	protected override void Initialize()
	{
		base.Initialize();
		AnnotationFlags |= PdfAnnotationFlags.Print;
		base.Dictionary.SetProperty("Subtype", new PdfName("Widget"));
		base.Dictionary.SetProperty("BS", m_border);
	}

	protected virtual void OnBeginSave(EventArgs args)
	{
		if (this.BeginSave != null)
		{
			this.BeginSave(this, args);
		}
	}

	protected override void Save()
	{
		base.Save();
		if (Parent is PdfCheckBoxField && base.Dictionary.isXfa)
		{
			Parent.Save();
		}
		if (m_signatureField != null && m_signatureField.Signature == null)
		{
			m_signatureField.Save();
			if (m_appearance != null)
			{
				if (!m_signatureField.m_containsBG)
				{
					m_widgetAppearance.BackColor = PdfColor.Empty;
				}
				if (m_signatureField.BorderWidth <= 0f || !m_signatureField.m_containsBW)
				{
					m_widgetAppearance.BorderColor = PdfColor.Empty;
				}
				base.Dictionary.SetProperty("MK", m_widgetAppearance);
			}
		}
		OnBeginSave(new EventArgs());
		if (m_extendedAppearance != null)
		{
			base.Dictionary.SetProperty("AP", m_extendedAppearance);
			base.Dictionary.SetProperty("MK", m_widgetAppearance);
		}
		else
		{
			if (m_appearance != null && m_appearance.GetNormalTemplate() != null)
			{
				base.Dictionary.SetProperty("AP", m_appearance);
			}
			else
			{
				base.Dictionary.SetProperty("AP", (IPdfPrimitive)null);
			}
			bool flag = false;
			if (base.Dictionary.ContainsKey("FT"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(base.Dictionary["FT"]) as PdfName;
				if (pdfName != null && pdfName.Value == "Sig")
				{
					flag = true;
				}
			}
			if (!flag)
			{
				base.Dictionary.SetProperty("MK", m_widgetAppearance);
			}
			base.Dictionary.SetProperty("AS", (IPdfPrimitive)null);
		}
		if (m_defaultAppearance != null && !isAutoResize)
		{
			base.Dictionary.SetProperty("DA", new PdfString(m_defaultAppearance.ToString()));
		}
	}

	private string HighlightModeToString(PdfHighlightMode m_highlightingMode)
	{
		return m_highlightingMode switch
		{
			PdfHighlightMode.NoHighlighting => "N", 
			PdfHighlightMode.Outline => "O", 
			PdfHighlightMode.Push => "P", 
			_ => "I", 
		};
	}

	internal PdfAppearance ObtainAppearance()
	{
		return m_appearance;
	}
}
