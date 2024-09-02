using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedRichMediaAnnotation : PdfLoadedStyledAnnotation
{
	internal PdfRichMediaContent m_content;

	private PdfRichMediaPresentationStyle m_presentationStyle;

	private PdfRichMediaActivationMode m_activationMode;

	private PdfCrossTable m_crossTable;

	private PdfDictionary m_contentDictionary;

	private PdfDictionary m_settingsDictionary;

	private PdfDictionary m_activationDictionary;

	private PdfDictionary m_configurations;

	private PdfArray m_configurationsArray;

	private PdfArray m_instancesArray;

	private PdfDictionary m_instances;

	private PdfDictionary m_assets;

	private PdfDictionary ContentDictionary
	{
		get
		{
			if (m_contentDictionary == null && base.Dictionary != null && base.Dictionary.ContainsKey("RichMediaContent"))
			{
				m_contentDictionary = PdfCrossTable.Dereference(base.Dictionary["RichMediaContent"]) as PdfDictionary;
			}
			return m_contentDictionary;
		}
	}

	private PdfArray ConfigurationsArray
	{
		get
		{
			if (m_configurationsArray == null && ContentDictionary != null && ContentDictionary.ContainsKey("Configurations"))
			{
				m_configurationsArray = PdfCrossTable.Dereference(ContentDictionary["Configurations"]) as PdfArray;
			}
			return m_configurationsArray;
		}
	}

	private PdfDictionary Configurations
	{
		get
		{
			if (m_configurations == null && ConfigurationsArray != null)
			{
				m_configurations = PdfCrossTable.Dereference(ConfigurationsArray[0]) as PdfDictionary;
			}
			return m_configurations;
		}
	}

	private PdfArray InstancesArray
	{
		get
		{
			if (m_instancesArray == null && Configurations != null && Configurations.ContainsKey("Instances"))
			{
				m_instancesArray = PdfCrossTable.Dereference(Configurations["Instances"]) as PdfArray;
			}
			return m_instancesArray;
		}
	}

	private PdfDictionary Instances
	{
		get
		{
			if (m_instances == null && InstancesArray != null)
			{
				m_instances = PdfCrossTable.Dereference(InstancesArray[0]) as PdfDictionary;
			}
			return m_instances;
		}
	}

	private PdfDictionary Asset
	{
		get
		{
			if (m_assets == null && Instances != null && Instances.ContainsKey("Asset"))
			{
				m_assets = PdfCrossTable.Dereference(Instances["Asset"]) as PdfDictionary;
			}
			return m_assets;
		}
	}

	private PdfDictionary SettingsDictionary
	{
		get
		{
			if (m_settingsDictionary == null && base.Dictionary != null && base.Dictionary.ContainsKey("RichMediaSettings"))
			{
				m_settingsDictionary = PdfCrossTable.Dereference(base.Dictionary["RichMediaSettings"]) as PdfDictionary;
			}
			return m_settingsDictionary;
		}
	}

	private PdfDictionary ActivationDictionary
	{
		get
		{
			if (m_activationDictionary == null && SettingsDictionary != null && SettingsDictionary.ContainsKey("Activation"))
			{
				m_activationDictionary = PdfCrossTable.Dereference(SettingsDictionary["Activation"]) as PdfDictionary;
			}
			return m_activationDictionary;
		}
	}

	public PdfRichMediaActivationMode ActivationMode
	{
		get
		{
			return ObtainActivateMode();
		}
		set
		{
			m_activationMode = value;
			string value2 = ActivateMode(m_activationMode);
			if (ActivationDictionary != null && ActivationDictionary.ContainsKey("Condition"))
			{
				if (ActivationDictionary != null)
				{
					ActivationDictionary.SetProperty("Condition", new PdfName(value2));
				}
				base.Dictionary.Modify();
			}
		}
	}

	public PdfRichMediaContent Content
	{
		get
		{
			if (m_content == null)
			{
				m_content = new PdfRichMediaContent(FileName, new MemoryStream(Data), isInternal: true);
			}
			return m_content;
		}
	}

	internal byte[] Data
	{
		get
		{
			byte[] result = null;
			if (Asset != null && Asset.ContainsKey("EF"))
			{
				PdfDictionary pdfDictionary = ((!(Asset["EF"] is PdfDictionary)) ? ((Asset["EF"] as PdfReferenceHolder).Object as PdfDictionary) : (Asset["EF"] as PdfDictionary));
				if (pdfDictionary != null && pdfDictionary.ContainsKey("F"))
				{
					PdfReferenceHolder pdfReferenceHolder = pdfDictionary["F"] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfStream pdfStream)
					{
						result = pdfStream.GetDecompressedData();
					}
				}
			}
			return result;
		}
	}

	internal string FileName
	{
		get
		{
			string result = " ";
			if (ContentDictionary != null)
			{
				if (Asset != null && Asset.ContainsKey("F"))
				{
					result = (Asset["F"] as PdfString).Value;
				}
				else if (Asset.ContainsKey("UF"))
				{
					result = (Asset["UF"] as PdfString).Value;
				}
			}
			return result;
		}
	}

	public PdfRichMediaPresentationStyle PresentationStyle
	{
		get
		{
			return ObtainPresentationStyle();
		}
		set
		{
			m_presentationStyle = value;
			string style = GetStyle(m_presentationStyle);
			if (ActivationDictionary != null && ActivationDictionary.ContainsKey("Presentation"))
			{
				if (PdfCrossTable.Dereference(ActivationDictionary["Presentation"]) is PdfDictionary pdfDictionary)
				{
					pdfDictionary.SetProperty("Style", new PdfName(style));
				}
				base.Dictionary.Modify();
			}
		}
	}

	internal PdfLoadedRichMediaAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle)
		: base(dictionary, crossTable)
	{
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
	}

	private PdfRichMediaActivationMode ObtainActivateMode()
	{
		PdfRichMediaActivationMode result = PdfRichMediaActivationMode.Click;
		if (ActivationDictionary != null && ActivationDictionary.ContainsKey("Condition"))
		{
			switch ((ActivationDictionary["Condition"] as PdfName).Value.ToString())
			{
			case "XA":
				result = PdfRichMediaActivationMode.Click;
				break;
			case "PO":
				result = PdfRichMediaActivationMode.PageOpen;
				break;
			case "PV":
				result = PdfRichMediaActivationMode.PageVisible;
				break;
			}
		}
		return result;
	}

	private string ActivateMode(PdfRichMediaActivationMode mode)
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

	private new string Type(PdfRichMediaContentType mode)
	{
		string result = "";
		switch (mode)
		{
		case PdfRichMediaContentType.Video:
			result = "Video";
			break;
		case PdfRichMediaContentType.Sound:
			result = "Sound";
			break;
		}
		return result;
	}

	private PdfRichMediaPresentationStyle ObtainPresentationStyle()
	{
		PdfRichMediaPresentationStyle result = PdfRichMediaPresentationStyle.Embedded;
		if (ActivationDictionary != null && ActivationDictionary.ContainsKey("Presentation"))
		{
			string text = ((m_crossTable.GetObject(ActivationDictionary["Presentation"]) as PdfDictionary)["Style"] as PdfName).Value.ToString();
			if (!(text == "Embedded"))
			{
				if (text == "Windowed")
				{
					result = PdfRichMediaPresentationStyle.Windowed;
				}
			}
			else
			{
				result = PdfRichMediaPresentationStyle.Embedded;
			}
		}
		return result;
	}

	private string GetStyle(PdfRichMediaPresentationStyle pstyle)
	{
		string result = "";
		switch (pstyle)
		{
		case PdfRichMediaPresentationStyle.Embedded:
			result = "Embedded";
			break;
		case PdfRichMediaPresentationStyle.Windowed:
			result = "Windowed";
			break;
		}
		return result;
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (!(base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			return;
		}
		if (base.Dictionary["AP"] != null && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			PdfTemplate pdfTemplate = new PdfTemplate(template);
			if (pdfTemplate != null)
			{
				PdfGraphics pdfGraphics = ObtainlayerGraphics();
				PdfGraphicsState state = base.Page.Graphics.Save();
				if (Opacity < 1f)
				{
					base.Page.Graphics.SetTransparency(Opacity);
				}
				if (pdfGraphics != null)
				{
					pdfGraphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
				}
				else
				{
					base.Page.Graphics.DrawPdfTemplate(pdfTemplate, Bounds.Location, Bounds.Size);
				}
				base.Page.Graphics.Restore(state);
			}
		}
		RemoveAnnoationFromPage(base.Page, this);
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}
}
