using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfCheckFieldBase : PdfStyledField
{
	private PdfCheckBoxStyle m_style;

	private PdfTemplate m_checkedTemplate;

	private PdfTemplate m_uncheckedTemplate;

	private PdfTemplate m_pressedCheckedTemplate;

	private PdfTemplate m_pressedUncheckedTemplate;

	public PdfCheckBoxStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			if (m_style != value)
			{
				m_style = value;
				base.Widget.WidgetAppearance.NormalCaption = StyleToString(m_style);
			}
			NotifyPropertyChanged("Style");
		}
	}

	internal PdfTemplate CheckedTemplate
	{
		get
		{
			return m_checkedTemplate;
		}
		set
		{
			m_checkedTemplate = value;
		}
	}

	internal PdfTemplate UncheckedTemplate
	{
		get
		{
			return m_uncheckedTemplate;
		}
		set
		{
			m_uncheckedTemplate = value;
		}
	}

	internal PdfTemplate PressedCheckedTemplate
	{
		get
		{
			return m_pressedCheckedTemplate;
		}
		set
		{
			m_pressedCheckedTemplate = value;
		}
	}

	internal PdfTemplate PressedUncheckedTemplate
	{
		get
		{
			return m_pressedUncheckedTemplate;
		}
		set
		{
			m_pressedUncheckedTemplate = value;
		}
	}

	public PdfCheckFieldBase(PdfPageBase page, string name)
		: base(page, name)
	{
	}

	internal PdfCheckFieldBase()
	{
	}

	protected string StyleToString(PdfCheckBoxStyle style)
	{
		return style switch
		{
			PdfCheckBoxStyle.Circle => "l", 
			PdfCheckBoxStyle.Cross => "8", 
			PdfCheckBoxStyle.Diamond => "u", 
			PdfCheckBoxStyle.Square => "n", 
			PdfCheckBoxStyle.Star => "H", 
			_ => "4", 
		};
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("FT", new PdfName("Btn"));
	}

	internal override void Save()
	{
		base.Save();
		if (Form != null || isXfa)
		{
			CreateTemplate(ref m_checkedTemplate);
			CreateTemplate(ref m_uncheckedTemplate);
			CreateTemplate(ref m_pressedCheckedTemplate);
			CreateTemplate(ref m_pressedUncheckedTemplate);
			base.Widget.ExtendedAppearance.Normal.On = m_checkedTemplate;
			base.Widget.ExtendedAppearance.Normal.Off = m_uncheckedTemplate;
			base.Widget.ExtendedAppearance.Pressed.On = m_pressedCheckedTemplate;
			base.Widget.ExtendedAppearance.Pressed.Off = m_pressedUncheckedTemplate;
			DrawAppearance();
		}
		else
		{
			ReleaseTemplate(m_checkedTemplate);
			ReleaseTemplate(m_uncheckedTemplate);
			ReleaseTemplate(m_pressedCheckedTemplate);
			ReleaseTemplate(m_pressedUncheckedTemplate);
		}
	}

	protected virtual void DrawAppearance()
	{
	}

	private void CreateTemplate(ref PdfTemplate template)
	{
		if (template == null)
		{
			template = new PdfTemplate(base.Size);
		}
		else
		{
			template.Reset(base.Size);
		}
	}

	private void ReleaseTemplate(PdfTemplate template)
	{
		if (template != null)
		{
			template.Reset();
			base.Widget.ExtendedAppearance = null;
		}
	}

	internal override void Draw()
	{
		base.Draw();
	}
}
