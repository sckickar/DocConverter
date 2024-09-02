using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedSoundAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfSound m_sound;

	private PdfDictionary m_dictionary;

	private PdfSoundIcon m_icon;

	private new PdfAppearance m_appearance;

	public PdfLoadedPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory == null)
			{
				m_reviewHistory = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: true);
			}
			return m_reviewHistory;
		}
	}

	public PdfLoadedPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments == null)
			{
				m_comments = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: false);
			}
			return m_comments;
		}
	}

	public PdfSound Sound
	{
		get
		{
			return ObtainSound();
		}
		set
		{
			m_sound = value;
			base.Dictionary.Remove("Sound");
			PdfReferenceHolder primitive = new PdfReferenceHolder(m_sound);
			base.Dictionary.SetProperty("Sound", primitive);
			base.Dictionary.Modify();
			NotifyPropertyChanged("Sound");
		}
	}

	public string FileName => ObtainFileName();

	public PdfSoundIcon Icon
	{
		get
		{
			return ObtainIcon();
		}
		set
		{
			m_icon = value;
			base.Dictionary.SetName("Name", m_icon.ToString());
			NotifyPropertyChanged("Icon");
		}
	}

	private string ObtainFileName()
	{
		string result = string.Empty;
		if (base.Dictionary.ContainsKey("Sound"))
		{
			result = ((m_crossTable.GetObject(base.Dictionary["Sound"]) as PdfDictionary)["T"] as PdfString).Value.ToString();
		}
		return result;
	}

	private PdfSoundIcon ObtainIcon()
	{
		PdfSoundIcon result = PdfSoundIcon.Speaker;
		if (base.Dictionary.ContainsKey("Name"))
		{
			PdfName pdfName = base.Dictionary["Name"] as PdfName;
			result = GetIconName(pdfName.Value.ToString());
		}
		return result;
	}

	private new PdfSoundIcon GetIconName(string iType)
	{
		PdfSoundIcon result = PdfSoundIcon.Speaker;
		if (!(iType == "Mic"))
		{
			if (iType == "Speaker")
			{
				result = PdfSoundIcon.Speaker;
			}
		}
		else
		{
			result = PdfSoundIcon.Mic;
		}
		return result;
	}

	private PdfSound ObtainSound()
	{
		PdfSound pdfSound = new PdfSound(ObtainFileName());
		if (base.Dictionary.ContainsKey("Sound"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["Sound"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("B"))
			{
				pdfSound.Bits = (pdfDictionary["B"] as PdfNumber).IntValue;
			}
			if (pdfDictionary.ContainsKey("R"))
			{
				pdfSound.Rate = (pdfDictionary["R"] as PdfNumber).IntValue;
			}
			if (pdfDictionary.ContainsKey("C"))
			{
				if ((pdfDictionary["C"] as PdfNumber).IntValue == 1)
				{
					pdfSound.Channels = PdfSoundChannels.Mono;
				}
				else
				{
					pdfSound.Channels = PdfSoundChannels.Stereo;
				}
			}
			if (pdfDictionary.ContainsKey("E"))
			{
				PdfName pdfName = pdfDictionary["E"] as PdfName;
				pdfSound.Encoding = GetEncodigType(pdfName.Value.ToString());
			}
		}
		return pdfSound;
	}

	private PdfSoundEncoding GetEncodigType(string eType)
	{
		PdfSoundEncoding result = PdfSoundEncoding.Raw;
		switch (eType)
		{
		case "Raw":
			result = PdfSoundEncoding.Raw;
			break;
		case "Signed":
			result = PdfSoundEncoding.Signed;
			break;
		case "MuLaw":
			result = PdfSoundEncoding.MuLaw;
			break;
		case "ALaw":
			result = PdfSoundEncoding.ALaw;
			break;
		}
		return result;
	}

	internal PdfLoadedSoundAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle)
		: base(dictionary, crossTable)
	{
		if (PdfCrossTable.Dereference((PdfCrossTable.Dereference(dictionary["Sound"]) as PdfDictionary)["T"]) is PdfString pdfString)
		{
			string value = pdfString.Value;
			PdfReferenceHolder obj = dictionary["Sound"] as PdfReferenceHolder;
			if (obj == null)
			{
				throw new ArgumentNullException();
			}
			_ = (obj.Object as PdfStream).Data;
			m_dictionary = dictionary;
			m_crossTable = crossTable;
			m_sound = new PdfSound(value, test: true);
		}
		else
		{
			if (dictionary["Sound"] as PdfReferenceHolder == null)
			{
				throw new ArgumentNullException();
			}
			m_dictionary = dictionary;
			m_crossTable = crossTable;
			m_sound = new PdfSound();
		}
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

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}
}
