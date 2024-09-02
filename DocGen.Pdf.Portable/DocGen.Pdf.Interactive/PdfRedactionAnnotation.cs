using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRedactionAnnotation : PdfAnnotation
{
	private PdfColor borderColor;

	private PdfColor textColor;

	private PdfFont font;

	private PdfTextAlignment alignment;

	private LineBorder border = new LineBorder();

	private string overlayText;

	private bool repeat;

	private bool flatten;

	private float m_borderWidth;

	public PdfColor TextColor
	{
		get
		{
			return textColor;
		}
		set
		{
			textColor = value;
			base.Dictionary.SetProperty("C", textColor.ToArray());
			NotifyPropertyChanged("TextColor");
		}
	}

	public PdfTextAlignment TextAlignment
	{
		get
		{
			return alignment;
		}
		set
		{
			if (alignment != value)
			{
				alignment = value;
				base.Dictionary.SetProperty("Q", new PdfNumber((int)alignment));
			}
			NotifyPropertyChanged("TextAlignment");
		}
	}

	public string OverlayText
	{
		get
		{
			return overlayText;
		}
		set
		{
			overlayText = value;
			base.Dictionary.SetString("OverlayText", overlayText);
			NotifyPropertyChanged("OverlayText");
		}
	}

	public PdfFont Font
	{
		get
		{
			return font;
		}
		set
		{
			font = value;
			NotifyPropertyChanged("Font");
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			base.Dictionary.SetProperty("OC", borderColor.ToArray());
			NotifyPropertyChanged("BorderColor");
		}
	}

	public new LineBorder Border
	{
		get
		{
			return border;
		}
		set
		{
			border = value;
			NotifyPropertyChanged("Border");
		}
	}

	public bool RepeatText
	{
		get
		{
			return repeat;
		}
		set
		{
			repeat = value;
			base.Dictionary.SetBoolean("Repeat", repeat);
			NotifyPropertyChanged("RepeatText");
		}
	}

	public new bool Flatten
	{
		get
		{
			return flatten;
		}
		set
		{
			flatten = value;
			NotifyPropertyChanged("Flatten");
		}
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Redact"));
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		flatten = true;
		SaveAndFlatten(flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlattenPopUps)
	{
		if (!Flatten && !base.SetAppearanceDictionary)
		{
			return;
		}
		PdfTemplate mouseHover = CreateNormalAppearance(OverlayText, Font, RepeatText, TextColor, TextAlignment, Border);
		if (Flatten)
		{
			if (base.LoadedPage != null)
			{
				RemoveAnnoationFromPage(base.LoadedPage, this);
			}
		}
		else
		{
			PdfTemplate normal = CreateBorderAppearance(BorderColor, Border);
			base.Appearance.Normal = normal;
			base.Appearance.MouseHover = mouseHover;
			base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
		}
	}

	protected override void Save()
	{
		base.Save();
		CheckFlatten();
		SaveAndFlatten(isExternalFlattenPopUps: false);
	}

	internal void ApplyRedaction(PdfLoadedPage page)
	{
		throw new PdfException("Flattening of redaction annotation is currently not supported on this platform");
	}
}
