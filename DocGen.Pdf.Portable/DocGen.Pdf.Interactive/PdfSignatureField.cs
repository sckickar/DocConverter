using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Interactive;

public class PdfSignatureField : PdfSignatureAppearanceField
{
	private PdfSignature m_signature;

	internal bool m_fieldAutoNaming = true;

	internal bool m_SkipKidsCertificate;

	public new PdfAppearance Appearance => base.Widget.Appearance;

	public PdfSignature Signature
	{
		get
		{
			return m_signature;
		}
		set
		{
			m_signature = value;
			NotifyPropertyChanged("Signature");
		}
	}

	public PdfSignatureField(PdfPageBase page, string name)
		: base(page, name)
	{
		PdfDocumentBase pdfDocumentBase = null;
		if (page is PdfPage)
		{
			pdfDocumentBase = (page as PdfPage).Document;
		}
		else if (page is PdfLoadedPage)
		{
			pdfDocumentBase = (page as PdfLoadedPage).Document;
		}
		if (pdfDocumentBase != null)
		{
			pdfDocumentBase.ObtainForm().SignatureFlags = SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly;
		}
	}

	internal PdfSignatureField()
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		if (m_fieldAutoNaming)
		{
			base.Widget.Dictionary.SetProperty("FT", new PdfName("Sig"));
		}
		else
		{
			base.Dictionary.SetProperty("FT", new PdfName("Sig"));
		}
	}

	internal override void Save()
	{
		base.Save();
		if (m_signature == null)
		{
			return;
		}
		PdfSignatureDictionary pdfSignatureDictionary = null;
		if (Page is PdfPage)
		{
			pdfSignatureDictionary = new PdfSignatureDictionary(((PdfPage)Page).Document, m_signature, m_signature.Certificate);
			base.Widget.Dictionary.SetProperty("V", new PdfReferenceHolder(pdfSignatureDictionary));
		}
		else
		{
			if (!(Page is PdfLoadedPage))
			{
				return;
			}
			if (m_signature.Certificate == null)
			{
				pdfSignatureDictionary = new PdfSignatureDictionary(((PdfLoadedPage)Page).Document, m_signature);
				return;
			}
			((PdfLoadedPage)Page).Document.Catalog.BeginSave += Catalog_BeginSave;
			base.Dictionary.BeginSave += Dictionary_BeginSave;
			pdfSignatureDictionary = new PdfSignatureDictionary(((PdfLoadedPage)Page).Document, m_signature, m_signature.Certificate);
			((PdfLoadedPage)Page).Document.PdfObjects.Add(((IPdfWrapper)pdfSignatureDictionary).Element);
			if (!((PdfLoadedPage)Page).Document.CrossTable.IsMerging)
			{
				((IPdfWrapper)pdfSignatureDictionary).Element.Position = -1;
			}
			base.Widget.Dictionary.SetProperty("V", new PdfReferenceHolder(pdfSignatureDictionary));
			base.Widget.Dictionary.SetProperty("Ff", new PdfNumber(0));
			pdfSignatureDictionary.Archive = false;
		}
	}

	private void Catalog_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (m_signature.Certificated)
		{
			if (!(PdfCrossTable.Dereference(((PdfLoadedPage)Page).Document.Catalog["Perms"]) is PdfDictionary pdfDictionary))
			{
				PdfDictionary pdfDictionary2 = new PdfDictionary();
				pdfDictionary2["DocMDP"] = new PdfReferenceHolder(m_signature.m_signatureDictionary);
				((PdfLoadedPage)Page).Document.Catalog["Perms"] = pdfDictionary2;
			}
			else if (!pdfDictionary.ContainsKey("DocMDP"))
			{
				pdfDictionary.SetProperty("DocMDP", new PdfReferenceHolder(m_signature.m_signatureDictionary));
			}
		}
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (this != null && Page is PdfPage)
		{
			base.Dictionary.Encrypt = ((PdfPage)Page).Document.Security.Enabled;
		}
		else if (this != null && Page is PdfLoadedPage)
		{
			base.Dictionary.Encrypt = ((PdfLoadedPage)Page).Document.Security.Enabled;
		}
	}

	internal override void Draw()
	{
		base.Draw();
		if (base.Widget.ObtainAppearance() != null)
		{
			RectangleF bounds = Bounds;
			Page.Graphics.DrawPdfTemplate(Appearance.Normal, bounds.Location);
		}
	}

	protected override void DrawAppearance(PdfTemplate template)
	{
		base.DrawAppearance(template);
		PaintParams paintParams = new PaintParams(new RectangleF(PointF.Empty, base.Size), base.BackBrush, null, base.BorderPen, base.BorderStyle, m_containsBW ? base.BorderWidth : 0f, base.ShadowBrush, base.RotationAngle);
		FieldPainter.DrawSignature(template.Graphics, paintParams);
	}
}
