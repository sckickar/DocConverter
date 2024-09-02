using System;
using DocGen.Drawing;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPage : PdfPageBase
{
	internal PdfSection m_section;

	private PdfAnnotationCollection m_annotations;

	internal bool IsNewPage;

	internal bool isMergingPage;

	public PdfSection Section
	{
		get
		{
			if (m_section == null)
			{
				throw new PdfException("Page must be added to some section before using.");
			}
			return m_section;
		}
		internal set
		{
			m_section = value;
		}
	}

	public override SizeF Size => Section.PageSettings.Size;

	internal override PointF Origin => Section.PageSettings.Origin;

	public new PdfAnnotationCollection Annotations
	{
		get
		{
			if (m_annotations == null)
			{
				m_annotations = new PdfAnnotationCollection(this);
				if (!base.Dictionary.ContainsKey("Annots"))
				{
					base.Dictionary["Annots"] = ((IPdfWrapper)m_annotations).Element;
				}
				m_annotations.Annotations = base.Dictionary["Annots"] as PdfArray;
			}
			return m_annotations;
		}
	}

	internal PdfDocument Document
	{
		get
		{
			if (m_section != null && m_section.Parent != null)
			{
				return m_section.Parent.Document;
			}
			return null;
		}
	}

	internal PdfCrossTable CrossTable
	{
		get
		{
			if (m_section == null)
			{
				throw new PdfDocumentException("Page is not created");
			}
			if (m_section.Parent != null)
			{
				return m_section.Parent.Document.CrossTable;
			}
			return m_section.ParentDocument.CrossTable;
		}
	}

	public event EventHandler BeginSave;

	public PdfPage()
		: base(new PdfDictionary())
	{
		Initialize();
	}

	public SizeF GetClientSize()
	{
		return Section.GetActualBounds(this, includeMargins: true).Size;
	}

	protected virtual void OnBeginSave(EventArgs e)
	{
		if (this.BeginSave != null)
		{
			this.BeginSave(this, e);
		}
	}

	internal override void Clear()
	{
		base.Clear();
		if (m_annotations != null)
		{
			m_annotations.Clear();
		}
		m_section = null;
	}

	internal void AssignSection(PdfSection section)
	{
		if (m_section != null)
		{
			throw new PdfException("The page already exists in some section, it can't be contained by several sections");
		}
		m_section = section;
		base.Dictionary["Parent"] = new PdfReferenceHolder(section);
	}

	private void Initialize()
	{
		base.Dictionary["Type"] = new PdfName("Page");
		base.Dictionary.BeginSave += PageBeginSave;
		base.Dictionary.EndSave += PageEndSave;
	}

	private void DrawPageTemplates(PdfDocument document)
	{
		if (document == null)
		{
			return;
		}
		if (Section.ContainsTemplates(document, this, foreground: false))
		{
			PdfPageLayer layer = new PdfPageLayer(this, clipPageTemplates: false);
			if (!base.Imported)
			{
				base.Layers.Insert(0, layer);
			}
			else
			{
				base.Layers.Add(layer);
			}
			Section.DrawTemplates(this, layer, document, foreground: false);
		}
		if (Section.ContainsTemplates(document, this, foreground: true))
		{
			PdfPageLayer layer2 = new PdfPageLayer(this, clipPageTemplates: false);
			base.Layers.Add(layer2);
			Section.DrawTemplates(this, layer2, document, foreground: true);
		}
	}

	private void RemoveTemplateLayers(PdfDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		bool num = Section.ContainsTemplates(document, this, foreground: false);
		bool flag = Section.ContainsTemplates(document, this, foreground: true);
		if (num)
		{
			base.Layers.RemoveAt(0);
		}
		if (flag)
		{
			base.Layers.RemoveAt(base.Layers.Count - 1);
		}
	}

	private void PageBeginSave(object sender, SavePdfPrimitiveEventArgs args)
	{
		if (args.Writer.Document is PdfDocument document && Document != null)
		{
			DrawPageTemplates(document);
			if (m_isProgressOn)
			{
				Section.OnPageSaving(this);
			}
			if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001)
			{
				base.Dictionary["MediaBox"] = PdfArray.FromRectangle(new RectangleF(PointF.Empty, Size));
				base.Dictionary["TrimBox"] = PdfArray.FromRectangle(new RectangleF(PointF.Empty, Size));
			}
			PdfPageTransition transitionSettings = Section.GetTransitionSettings();
			if (transitionSettings != null)
			{
				base.Dictionary.SetProperty("Dur", new PdfNumber(transitionSettings.PageDuration));
				base.Dictionary.SetProperty("Trans", ((IPdfWrapper)transitionSettings).Element);
			}
		}
		else if (args.Writer.Document is PdfLoadedDocument { progressDelegate: not null } pdfLoadedDocument)
		{
			pdfLoadedDocument.OnPageSave(this);
		}
		OnBeginSave(new EventArgs());
	}

	private void PageEndSave(object sender, SavePdfPrimitiveEventArgs args)
	{
		if (args.Writer.Document is PdfDocument document && Document != null)
		{
			RemoveTemplateLayers(document);
		}
	}
}
