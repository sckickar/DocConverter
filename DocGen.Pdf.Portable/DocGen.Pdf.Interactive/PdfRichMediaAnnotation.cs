using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRichMediaAnnotation : PdfAnnotation
{
	internal PdfRichMediaContent content;

	internal PdfRichMediaPresentationStyle m_presentationStyle;

	internal PdfRichMediaActivationMode m_activationMode = PdfRichMediaActivationMode.Click;

	private bool m_saved;

	private new PdfAppearance m_appearance;

	public new PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
				m_isStandardAppearance = false;
			}
			return m_appearance;
		}
	}

	public PdfRichMediaActivationMode ActivationMode
	{
		get
		{
			return m_activationMode;
		}
		set
		{
			m_activationMode = value;
			NotifyPropertyChanged("ActivationMode");
		}
	}

	public PdfRichMediaContent Content
	{
		get
		{
			return content;
		}
		set
		{
			if (value != null)
			{
				content = value;
			}
			NotifyPropertyChanged("Content");
		}
	}

	public PdfRichMediaPresentationStyle PresentationStyle
	{
		get
		{
			return m_presentationStyle;
		}
		set
		{
			m_presentationStyle = value;
			NotifyPropertyChanged("PresentationStyle");
		}
	}

	public PdfRichMediaAnnotation(RectangleF bounds)
		: base(bounds)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("RichMedia"));
	}

	protected override void Save()
	{
		base.Save();
		if (Content != null && Content.isInternalLoad)
		{
			Content.m_fileSpecification = new PdfEmbeddedFileSpecification(Content.FileName + "." + Content.FileExtension, Content.Data);
		}
		if (Content != null && Content.m_fileSpecification != null && string.IsNullOrEmpty(Text))
		{
			Text = Content.m_fileSpecification.FileName;
		}
		SaveRichMediaDictionary();
		if (!m_saved)
		{
			CheckFlatten();
			SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
		}
	}

	private void SaveRichMediaDictionary()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("F", new PdfString(content.m_fileSpecification.FileName));
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		pdfDictionary2["F"] = new PdfReferenceHolder(content.m_fileSpecification.EmbeddedFile);
		pdfDictionary.SetProperty("EF", pdfDictionary2);
		pdfDictionary.SetProperty("Type", new PdfName("Filespec"));
		pdfDictionary.SetProperty("UF", new PdfString(content.m_fileSpecification.FileName));
		PdfArray pdfArray = new PdfArray();
		pdfArray.Add(new PdfString(content.m_fileSpecification.FileName));
		pdfArray.Add(new PdfReferenceHolder(pdfDictionary));
		PdfDictionary pdfDictionary3 = new PdfDictionary();
		pdfDictionary3.SetProperty("Names", new PdfArray(pdfArray));
		PdfDictionary pdfDictionary4 = new PdfDictionary();
		pdfDictionary4.SetProperty("Assets", pdfDictionary3);
		base.Dictionary.SetProperty("RichMediaContent", pdfDictionary4);
		PdfDictionary pdfDictionary5 = new PdfDictionary();
		pdfDictionary5.SetProperty("Asset", new PdfReferenceHolder(pdfDictionary));
		pdfDictionary5.SetProperty("Subtype", new PdfName(Content.ContentType));
		PdfArray pdfArray2 = new PdfArray();
		pdfArray2.Elements.Add(new PdfReferenceHolder(pdfDictionary5));
		PdfDictionary pdfDictionary6 = new PdfDictionary();
		pdfDictionary6.SetProperty("Instances", new PdfArray(pdfArray2));
		pdfDictionary6.SetProperty("Subtype", new PdfName(Content.ContentType));
		PdfArray pdfArray3 = new PdfArray();
		pdfArray3.Elements.Add(new PdfReferenceHolder(pdfDictionary6));
		pdfDictionary4.SetProperty("Configurations", new PdfArray(pdfArray3));
		PdfDictionary pdfDictionary7 = new PdfDictionary();
		PdfDictionary pdfDictionary8 = new PdfDictionary();
		pdfDictionary8.SetProperty("Condition", new PdfName(GetActiveMode(m_activationMode)));
		PdfDictionary pdfDictionary9 = new PdfDictionary();
		pdfDictionary9.SetProperty("Style", new PdfName(m_presentationStyle));
		pdfDictionary8.SetProperty("Presentation", pdfDictionary9);
		pdfDictionary7.SetProperty("Activation", pdfDictionary8);
		base.Dictionary.SetProperty("RichMediaSettings", pdfDictionary7);
	}

	private string GetActiveMode(PdfRichMediaActivationMode mode)
	{
		string result = "";
		switch (mode)
		{
		case PdfRichMediaActivationMode.Click:
			result = "XA";
			break;
		case PdfRichMediaActivationMode.PageOpen:
			result = "PO";
			break;
		case PdfRichMediaActivationMode.PageVisible:
			result = "PV";
			break;
		}
		return result;
	}

	private PdfTemplate CreateAppearance()
	{
		PdfTemplate pdfTemplate = null;
		if (m_appearance != null)
		{
			pdfTemplate = CustomAppearance(pdfTemplate);
		}
		return pdfTemplate;
	}

	private new void SetMatrix(PdfDictionary template)
	{
		PdfArray pdfArray = null;
		_ = new float[0];
		if (template["BBox"] is PdfArray pdfArray2)
		{
			pdfArray = new PdfArray(new float[6]
			{
				1f,
				0f,
				0f,
				1f,
				0f - (pdfArray2[0] as PdfNumber).FloatValue,
				0f - (pdfArray2[1] as PdfNumber).FloatValue
			});
		}
		if (pdfArray != null)
		{
			template["Matrix"] = pdfArray;
		}
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private PdfTemplate CustomAppearance(PdfTemplate template)
	{
		if (m_appearance != null && m_appearance.Normal != null)
		{
			SetMatrix(Appearance.Normal.m_content);
			template = m_appearance.Normal;
		}
		return template;
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		PdfGraphics layerGraphics = GetLayerGraphics();
		if (layerGraphics != null)
		{
			layerGraphics.DrawPdfTemplate(appearance, Bounds.Location, Bounds.Size);
		}
		else
		{
			page.Graphics.DrawPdfTemplate(appearance, Bounds.Location, Bounds.Size);
		}
		RemoveAnnoationFromPage(page, this);
		page.Graphics.Restore();
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		PdfTemplate pdfTemplate = CreateAppearance();
		if (base.Flatten)
		{
			if (pdfTemplate != null)
			{
				if (base.Page != null)
				{
					FlattenAnnotation(base.Page, pdfTemplate);
				}
				else if (base.LoadedPage != null)
				{
					FlattenAnnotation(base.LoadedPage, pdfTemplate);
				}
			}
			else
			{
				RemoveAnnoationFromPage(base.Page, this);
			}
		}
		else if (pdfTemplate != null)
		{
			Appearance.Normal = pdfTemplate;
			base.Dictionary.SetProperty("AP", new PdfReferenceHolder(Appearance));
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenPopup();
		}
		if (!isExternalFlatten && !base.Flatten)
		{
			base.Save();
		}
		if (!isExternalFlatten)
		{
			m_saved = true;
		}
		if (base.Page != null && base.Popup != null && base.Flatten)
		{
			RemoveAnnoationFromPage(base.Page, base.Popup);
		}
		else if (base.LoadedPage != null && base.Popup != null && base.Flatten)
		{
			RemoveAnnoationFromPage(base.LoadedPage, base.Popup);
		}
	}
}
