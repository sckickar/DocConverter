using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedAttachmentAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfAttachmentIcon m_icon;

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

	public PdfAttachmentIcon Icon
	{
		get
		{
			return ObtainIcon();
		}
		set
		{
			m_icon = value;
			base.Dictionary.SetName("Name", m_icon.ToString());
		}
	}

	public string FileName
	{
		get
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["FS"]) as PdfDictionary;
			string result = " ";
			if (pdfDictionary.ContainsKey("F"))
			{
				result = (pdfDictionary["F"] as PdfString).Value;
			}
			else if (pdfDictionary.ContainsKey("Desc"))
			{
				result = (pdfDictionary["Desc"] as PdfString).Value;
			}
			else if (pdfDictionary.ContainsKey("UF"))
			{
				result = (pdfDictionary["UF"] as PdfString).Value;
			}
			return result;
		}
	}

	public byte[] Data
	{
		get
		{
			byte[] result = null;
			if (m_crossTable.GetObject(base.Dictionary["FS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("EF"))
			{
				PdfDictionary pdfDictionary2 = ((!(pdfDictionary["EF"] is PdfDictionary)) ? ((pdfDictionary["EF"] as PdfReferenceHolder).Object as PdfDictionary) : (pdfDictionary["EF"] as PdfDictionary));
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("F"))
				{
					PdfReferenceHolder pdfReferenceHolder = pdfDictionary2["F"] as PdfReferenceHolder;
					if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfStream pdfStream)
					{
						result = pdfStream.GetDecompressedData();
					}
				}
			}
			return result;
		}
	}

	internal PdfLoadedAttachmentAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
	}

	private PdfAttachmentIcon ObtainIcon()
	{
		PdfAttachmentIcon result = PdfAttachmentIcon.PushPin;
		if (base.Dictionary.ContainsKey("Name"))
		{
			switch ((base.Dictionary["Name"] as PdfName).Value.ToString())
			{
			case "Graph":
				result = PdfAttachmentIcon.Graph;
				break;
			case "Paperclip":
				result = PdfAttachmentIcon.Paperclip;
				break;
			case "PushPin":
				result = PdfAttachmentIcon.PushPin;
				break;
			case "Tag":
				result = PdfAttachmentIcon.Tag;
				break;
			}
		}
		return result;
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
