using System;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedUriAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private string m_uri;

	public string Uri
	{
		get
		{
			return GetUriText();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("Uri can not be an empty string");
			}
			if (m_uri != value)
			{
				m_uri = value;
				PdfDictionary dictionary = base.Dictionary;
				if (base.Dictionary.ContainsKey("A"))
				{
					if (PdfCrossTable.Dereference(base.Dictionary["A"]) is PdfDictionary pdfDictionary)
					{
						pdfDictionary.SetString("URI", m_uri);
					}
					base.Dictionary.Modify();
				}
			}
			NotifyPropertyChanged("Uri");
		}
	}

	internal PdfLoadedUriAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		Text = text;
	}

	private string GetUriText()
	{
		string result = string.Empty;
		if (base.Dictionary.ContainsKey("A") && PdfCrossTable.Dereference(base.Dictionary["A"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("URI") && PdfCrossTable.Dereference(pdfDictionary["URI"]) is PdfString pdfString)
		{
			result = Encoding.UTF8.GetString(pdfString.Bytes, 0, pdfString.Bytes.Length);
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
