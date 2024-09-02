using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfStructureElement : PdfTag
{
	private PdfTagType m_tagType = PdfTagType.None;

	private PdfStructureElement m_parent;

	private string m_altText;

	private string m_title;

	private string m_language;

	private string m_abbrevation;

	private string m_actualText;

	internal string m_name;

	private int m_order;

	private bool isAdded;

	private ScopeType m_scope = ScopeType.None;

	internal bool m_isActiveSetTag;

	internal List<PdfStructureElement> m_child = new List<PdfStructureElement>();

	private PdfDictionary m_dictionary;

	private PdfDictionary m_pageDictionary;

	private PdfLoadedDocument m_document;

	private PdfPageBase m_page;

	private RectangleF m_bounds;

	private string m_text = string.Empty;

	internal List<int> m_mcids = new List<int>();

	private List<RectangleF> m_boundsCollection;

	private string m_fontName;

	private float m_fontSize;

	internal bool IsTagSplitParser;

	private PdfDictionary m_attributeDictionary;

	private FontStyle m_fontStyle;

	public string Abbrevation
	{
		get
		{
			if (string.IsNullOrEmpty(m_abbrevation) && m_dictionary != null)
			{
				InitializeTaggedContents();
			}
			return m_abbrevation;
		}
		set
		{
			m_abbrevation = value;
		}
	}

	public string ActualText
	{
		get
		{
			if (m_actualText == null && m_dictionary != null && m_dictionary.ContainsKey("ActualText") && PdfCrossTable.Dereference(m_dictionary["ActualText"]) is PdfString pdfString)
			{
				m_actualText = pdfString.Value;
			}
			return m_actualText;
		}
		set
		{
			m_actualText = value;
		}
	}

	public string AlternateText
	{
		get
		{
			if (m_altText == null && m_dictionary != null && m_dictionary.ContainsKey("Alt") && PdfCrossTable.Dereference(m_dictionary["Alt"]) is PdfString pdfString)
			{
				m_altText = pdfString.Value;
			}
			return m_altText;
		}
		set
		{
			m_altText = value;
		}
	}

	public string Language
	{
		get
		{
			if (m_language == null && m_dictionary != null && m_dictionary.ContainsKey("Lang") && PdfCrossTable.Dereference(m_dictionary["Lang"]) is PdfString pdfString)
			{
				m_language = pdfString.Value;
			}
			return m_language;
		}
		set
		{
			m_language = value;
		}
	}

	public override int Order
	{
		get
		{
			return m_order;
		}
		set
		{
			m_order = value;
		}
	}

	public PdfStructureElement Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			m_parent = value;
		}
	}

	public PdfTagType TagType
	{
		get
		{
			if (m_tagType == PdfTagType.None && m_dictionary != null && m_dictionary.ContainsKey("S"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(m_dictionary["S"]) as PdfName;
				if (pdfName != null)
				{
					m_tagType = GetTagType(pdfName.Value);
				}
			}
			else if (m_tagType == PdfTagType.None && Parent != null && Parent.TagType != PdfTagType.None)
			{
				m_tagType = Parent.TagType;
			}
			return m_tagType;
		}
		set
		{
			m_tagType = value;
		}
	}

	public string Title
	{
		get
		{
			if (m_title == null && m_dictionary != null && m_dictionary.ContainsKey("T") && PdfCrossTable.Dereference(m_dictionary["T"]) is PdfString pdfString)
			{
				m_title = pdfString.Value;
			}
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal bool IsAdded
	{
		get
		{
			return isAdded;
		}
		set
		{
			isAdded = value;
		}
	}

	internal PdfDictionary AttributeDictionary => m_attributeDictionary;

	public ScopeType Scope
	{
		get
		{
			if (m_scope == ScopeType.None && m_dictionary != null && m_dictionary.ContainsKey("A") && PdfCrossTable.Dereference(m_dictionary["A"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Scope"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["Scope"]) as PdfName;
				if (pdfName != null)
				{
					m_scope = GetScopeType(pdfName.Value);
				}
			}
			return m_scope;
		}
		set
		{
			m_scope = value;
		}
	}

	public PdfStructureElement[] ChildElements => m_child.ToArray();

	public PdfPageBase Page
	{
		get
		{
			if (m_page == null && m_dictionary != null)
			{
				m_page = GetPageFromElement(this);
			}
			if (m_page == null && m_dictionary != null && Parent != null && !IsTagSplitParser)
			{
				m_page = GetPageFromElement(Parent);
				if (m_page == null)
				{
					m_page = GetChildPage();
				}
			}
			return m_page;
		}
		internal set
		{
			m_page = value;
		}
	}

	public new RectangleF Bounds
	{
		get
		{
			if (m_bounds == RectangleF.Empty && m_dictionary != null)
			{
				InitializeTaggedContents();
			}
			else if (m_dictionary == null)
			{
				m_bounds = base.Bounds;
			}
			return m_bounds;
		}
		internal set
		{
			base.Bounds = value;
		}
	}

	public string Text
	{
		get
		{
			if (string.IsNullOrEmpty(m_text) && m_dictionary != null)
			{
				InitializeTaggedContents();
			}
			return m_text;
		}
	}

	internal string FontName => m_fontName;

	internal float FontSize => m_fontSize;

	internal FontStyle FontStyle => m_fontStyle;

	internal PdfDictionary Dictionary => m_dictionary;

	public PdfStructureElement()
	{
		m_tagType = PdfTagType.None;
		if (PdfCatalog.StructTreeRoot == null)
		{
			PdfCatalog.m_structTreeRoot = new PdfStructTreeRoot();
		}
	}

	public PdfStructureElement(PdfTagType tag)
		: this()
	{
		m_tagType = tag;
		m_name = Guid.NewGuid().ToString();
	}

	internal PdfStructureElement(PdfLoadedDocument loadedDocument, PdfDictionary dictionary, int order, PdfStructureElement parent)
	{
		m_dictionary = dictionary;
		m_parent = parent;
		m_document = loadedDocument;
		m_order = order;
	}

	private PdfTagType GetTagType(string tagType)
	{
		PdfTagType result = PdfTagType.None;
		switch (tagType)
		{
		case "P":
			result = PdfTagType.Paragraph;
			break;
		case "Figure":
			result = PdfTagType.Figure;
			break;
		case "Art":
			result = PdfTagType.Article;
			break;
		case "Annot":
			result = PdfTagType.Annotation;
			break;
		case "Bibentry":
			result = PdfTagType.BibliographyEntry;
			break;
		case "BlockQuote":
			result = PdfTagType.BlockQuotation;
			break;
		case "Caption":
			result = PdfTagType.Caption;
			break;
		case "Code":
			result = PdfTagType.Code;
			break;
		case "Div":
			result = PdfTagType.Division;
			break;
		case "Document":
			result = PdfTagType.Document;
			break;
		case "Form":
			result = PdfTagType.Form;
			break;
		case "Formula":
			result = PdfTagType.Formula;
			break;
		case "Index":
			result = PdfTagType.Index;
			break;
		case "H":
		case "Title":
			result = PdfTagType.Heading;
			break;
		case "H1":
			result = PdfTagType.HeadingLevel1;
			break;
		case "H2":
			result = PdfTagType.HeadingLevel2;
			break;
		case "H3":
			result = PdfTagType.HeadingLevel3;
			break;
		case "H4":
			result = PdfTagType.HeadingLevel4;
			break;
		case "H5":
			result = PdfTagType.HeadingLevel5;
			break;
		case "H6":
			result = PdfTagType.HeadingLevel6;
			break;
		case "Lbl":
			result = PdfTagType.Label;
			break;
		case "Link":
			result = PdfTagType.Link;
			break;
		case "L":
			result = PdfTagType.List;
			break;
		case "LI":
			result = PdfTagType.ListItem;
			break;
		case "LBody":
			result = PdfTagType.ListBody;
			break;
		case "Note":
			result = PdfTagType.Note;
			break;
		case "Part":
			result = PdfTagType.Part;
			break;
		case "Quote":
			result = PdfTagType.Quotation;
			break;
		case "Reference":
			result = PdfTagType.Reference;
			break;
		case "Sect":
			result = PdfTagType.Section;
			break;
		case "HyphenSpan":
		case "Span":
		case "StyleSpan":
		case "ParagraphSpan":
			result = PdfTagType.Span;
			break;
		case "Table":
			result = PdfTagType.Table;
			break;
		case "TD":
			result = PdfTagType.TableDataCell;
			break;
		case "TH":
			result = PdfTagType.TableHeader;
			break;
		case "TOC":
			result = PdfTagType.TableOfContent;
			break;
		case "TOCI":
			result = PdfTagType.TableOfContentItem;
			break;
		case "TR":
			result = PdfTagType.TableRow;
			break;
		case "THead":
			result = PdfTagType.TableHeaderRowGroup;
			break;
		case "TBody":
			result = PdfTagType.TableBodyRowGroup;
			break;
		case "TFoot":
			result = PdfTagType.TableFooterRowGroup;
			break;
		}
		return result;
	}

	private ScopeType GetScopeType(string scopeType)
	{
		ScopeType result = ScopeType.None;
		switch (scopeType)
		{
		case "Row":
			result = ScopeType.Row;
			break;
		case "Column":
			result = ScopeType.Column;
			break;
		case "Both":
			result = ScopeType.Both;
			break;
		}
		return result;
	}

	private void InitializeTaggedContents()
	{
		if (Page == null)
		{
			return;
		}
		if (TagType == PdfTagType.Paragraph || TagType == PdfTagType.Heading || TagType == PdfTagType.HeadingLevel1 || TagType == PdfTagType.HeadingLevel2 || TagType == PdfTagType.HeadingLevel3 || TagType == PdfTagType.HeadingLevel4 || TagType == PdfTagType.HeadingLevel5 || TagType == PdfTagType.HeadingLevel6 || TagType == PdfTagType.Label || TagType == PdfTagType.ListBody || TagType == PdfTagType.TableHeader || TagType == PdfTagType.TableDataCell || TagType == PdfTagType.TableOfContent || TagType == PdfTagType.TableOfContentItem || TagType == PdfTagType.Span || TagType == PdfTagType.BlockQuotation || TagType == PdfTagType.Caption || TagType == PdfTagType.Note || TagType == PdfTagType.Formula || TagType == PdfTagType.ListItem || (TagType == PdfTagType.Annotation && m_mcids.Count > 0 && string.IsNullOrEmpty(m_text)))
		{
			if (!string.IsNullOrEmpty(m_text) || !(m_bounds == RectangleF.Empty) || !string.IsNullOrEmpty(m_abbrevation))
			{
				return;
			}
			Dictionary<string, object> properties = null;
			m_text = Page.ExtractTaggedText(m_mcids, out properties);
			if (properties != null)
			{
				if (properties.ContainsKey("Bounds"))
				{
					m_bounds = (RectangleF)properties["Bounds"];
				}
				if (properties.ContainsKey("Abbrevation"))
				{
					m_abbrevation = (string)properties["Abbrevation"];
				}
				if (properties.ContainsKey("FontName"))
				{
					m_fontName = (string)properties["FontName"];
				}
				if (properties.ContainsKey("FontSize"))
				{
					m_fontSize = (float)properties["FontSize"];
				}
				if (properties.ContainsKey("FontStyle"))
				{
					m_fontStyle = (FontStyle)properties["FontStyle"];
				}
				properties.Clear();
				properties = null;
			}
			if (m_bounds != RectangleF.Empty)
			{
				m_bounds = CalculateBounds(m_bounds);
			}
			if (Page is PdfLoadedPage pdfLoadedPage)
			{
				_ = pdfLoadedPage.CropBox;
				if (pdfLoadedPage.CropBox != RectangleF.Empty)
				{
					m_bounds = new RectangleF(m_bounds.X - pdfLoadedPage.CropBox.X, m_bounds.Y + pdfLoadedPage.CropBox.Y, m_bounds.Width, m_bounds.Height);
				}
			}
		}
		else if (Parent != null && (Parent.TagType == PdfTagType.Annotation || Parent.TagType == PdfTagType.Link || Parent.TagType == PdfTagType.Form))
		{
			if (m_dictionary == null || (!m_dictionary.ContainsKey("Obj") && !m_dictionary.ContainsKey("obj")))
			{
				return;
			}
			string key = (m_dictionary.ContainsKey("Obj") ? "Obj" : "obj");
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(m_dictionary[key]) as PdfDictionary;
			if (Parent.TagType == PdfTagType.Annotation || Parent.TagType == PdfTagType.Link)
			{
				if (pdfDictionary == null || Page.Annotations.Count <= 0)
				{
					return;
				}
				{
					foreach (PdfAnnotation annotation in Page.Annotations)
					{
						if (annotation.Dictionary == pdfDictionary)
						{
							m_bounds = CalculateBounds(annotation.Bounds);
						}
					}
					return;
				}
			}
			if (pdfDictionary == null || m_document.Form.Fields.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < m_document.Form.Fields.Count; i++)
			{
				PdfLoadedStyledField pdfLoadedStyledField = m_document.Form.Fields[i] as PdfLoadedStyledField;
				if (pdfLoadedStyledField != null && pdfLoadedStyledField.Dictionary == pdfDictionary)
				{
					m_bounds = pdfLoadedStyledField.Bounds;
				}
				else if (pdfLoadedStyledField != null && pdfLoadedStyledField.Kids != null && pdfLoadedStyledField.Kids.Count > 0)
				{
					if (pdfLoadedStyledField is PdfLoadedTextBoxField)
					{
						PdfLoadedTextBoxField pdfLoadedTextBoxField = pdfLoadedStyledField as PdfLoadedTextBoxField;
						for (int j = 0; j < pdfLoadedTextBoxField.Items.Count; j++)
						{
							if (pdfLoadedTextBoxField.Items[j].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedTextBoxField.Items[j].Bounds;
							}
						}
					}
					else if (pdfLoadedStyledField is PdfLoadedComboBoxField)
					{
						PdfLoadedComboBoxField pdfLoadedComboBoxField = pdfLoadedStyledField as PdfLoadedComboBoxField;
						for (int k = 0; k < pdfLoadedComboBoxField.Items.Count; k++)
						{
							if (pdfLoadedComboBoxField.Items[k].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedComboBoxField.Items[k].Bounds;
							}
						}
					}
					else if (pdfLoadedStyledField is PdfLoadedListBoxField)
					{
						PdfLoadedListBoxField pdfLoadedListBoxField = pdfLoadedStyledField as PdfLoadedListBoxField;
						for (int l = 0; l < pdfLoadedListBoxField.Items.Count; l++)
						{
							if (pdfLoadedListBoxField.Items[l].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedListBoxField.Items[l].Bounds;
							}
						}
					}
					else if (pdfLoadedStyledField is PdfLoadedButtonField)
					{
						PdfLoadedButtonField pdfLoadedButtonField = pdfLoadedStyledField as PdfLoadedButtonField;
						for (int m = 0; m < pdfLoadedButtonField.Items.Count; m++)
						{
							if (pdfLoadedButtonField.Items[m].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedButtonField.Items[m].Bounds;
							}
						}
					}
					else if (pdfLoadedStyledField is PdfLoadedCheckBoxField)
					{
						PdfLoadedCheckBoxField pdfLoadedCheckBoxField = pdfLoadedStyledField as PdfLoadedCheckBoxField;
						for (int n = 0; n < pdfLoadedCheckBoxField.Items.Count; n++)
						{
							if (pdfLoadedCheckBoxField.Items[n].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedCheckBoxField.Items[n].Bounds;
							}
						}
					}
					else if (pdfLoadedStyledField is PdfLoadedRadioButtonListField)
					{
						PdfLoadedRadioButtonListField pdfLoadedRadioButtonListField = pdfLoadedStyledField as PdfLoadedRadioButtonListField;
						for (int num = 0; num < pdfLoadedRadioButtonListField.Items.Count; num++)
						{
							if (pdfLoadedRadioButtonListField.Items[num].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedRadioButtonListField.Items[num].Bounds;
							}
						}
					}
					else
					{
						if (!(pdfLoadedStyledField is PdfLoadedSignatureField))
						{
							continue;
						}
						PdfLoadedSignatureField pdfLoadedSignatureField = pdfLoadedStyledField as PdfLoadedSignatureField;
						for (int num2 = 0; num2 < pdfLoadedSignatureField.Items.Count; num2++)
						{
							if (pdfLoadedSignatureField.Items[num2].Dictionary == pdfDictionary)
							{
								m_bounds = pdfLoadedSignatureField.Items[num2].Bounds;
							}
						}
					}
				}
				else if (pdfDictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(pdfDictionary["Parent"]) is PdfDictionary pdfDictionary2 && pdfLoadedStyledField.Dictionary == pdfDictionary2)
				{
					m_bounds = pdfLoadedStyledField.Bounds;
				}
			}
			m_bounds = CalculateBounds(m_bounds);
		}
		else
		{
			if (TagType != PdfTagType.Figure)
			{
				return;
			}
			bool objectType = false;
			if (m_mcids.Count <= 0 || !(m_bounds == RectangleF.Empty))
			{
				return;
			}
			m_bounds = Page.ExtractTaggedContent(out m_abbrevation, m_mcids[0], out objectType);
			if (m_dictionary == null || !m_dictionary.ContainsKey("A") || !(PdfCrossTable.Dereference(m_dictionary["A"]) is PdfDictionary pdfDictionary3) || !(PdfCrossTable.Dereference(pdfDictionary3["BBox"]) is PdfArray pdfArray) || (!objectType && m_bounds.Width != 1f && m_bounds.Height != 1f))
			{
				return;
			}
			RectangleF rectangleF = pdfArray.ToRectangle();
			if (((Page.Rotation == PdfPageRotateAngle.RotateAngle0 || Page.Rotation == PdfPageRotateAngle.RotateAngle180) && Math.Abs(rectangleF.Width - m_bounds.Width) > 1f && Math.Abs(rectangleF.Height - m_bounds.Height) > 1f) || ((Page.Rotation == PdfPageRotateAngle.RotateAngle90 || Page.Rotation == PdfPageRotateAngle.RotateAngle270) && Math.Abs(rectangleF.Width - m_bounds.Height) > 1f && Math.Abs(rectangleF.Height - m_bounds.Width) > 1f))
			{
				if (Page.Rotation == PdfPageRotateAngle.RotateAngle0)
				{
					m_bounds = new RectangleF(m_bounds.X, m_bounds.Y - rectangleF.Height, rectangleF.Width, rectangleF.Height);
				}
				else if (Page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					m_bounds = new RectangleF(m_bounds.X, m_bounds.Y, rectangleF.Height, rectangleF.Width);
				}
				else if (Page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					m_bounds = new RectangleF(m_bounds.X - rectangleF.Width, m_bounds.Y, rectangleF.Width, rectangleF.Height);
				}
				else if (Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					m_bounds = new RectangleF(m_bounds.X - rectangleF.Height, m_bounds.Y - rectangleF.Width, rectangleF.Height, rectangleF.Width);
				}
			}
		}
	}

	private RectangleF CalculateBounds(RectangleF bounds)
	{
		if (Page != null)
		{
			switch (Page.Rotation)
			{
			case PdfPageRotateAngle.RotateAngle90:
				bounds = new RectangleF(Page.Size.Height - bounds.Y - bounds.Height, bounds.X, bounds.Height, bounds.Width);
				break;
			case PdfPageRotateAngle.RotateAngle180:
				bounds = new RectangleF(Page.Size.Width - bounds.X - bounds.Width, Page.Size.Height - bounds.Y - bounds.Height, bounds.Width, bounds.Height);
				break;
			case PdfPageRotateAngle.RotateAngle270:
				bounds = new RectangleF(bounds.Y, Page.Size.Width - bounds.X - bounds.Width, bounds.Height, bounds.Width);
				break;
			}
		}
		return bounds;
	}

	private PdfPageBase GetChildPage()
	{
		PdfPageBase pdfPageBase = null;
		if (ChildElements.Length != 0)
		{
			PdfStructureElement[] childElements = ChildElements;
			foreach (PdfStructureElement pdfStructureElement in childElements)
			{
				pdfPageBase = GetPageFromElement(pdfStructureElement);
				if (pdfPageBase == null)
				{
					pdfPageBase = pdfStructureElement.GetChildPage();
				}
				if (pdfPageBase != null)
				{
					break;
				}
			}
		}
		return pdfPageBase;
	}

	private PdfPageBase GetPageFromElement(PdfStructureElement element)
	{
		if (element.m_page != null)
		{
			return element.m_page;
		}
		if (element.m_pageDictionary != null)
		{
			m_page = m_document.Pages.GetPage(element.m_pageDictionary);
		}
		return m_page;
	}

	internal void InitializePageDictionary(PdfDictionary pageDictionary)
	{
		m_pageDictionary = pageDictionary;
	}

	internal void SetAttributeDictionary(PdfDictionary dictionary)
	{
		m_attributeDictionary = dictionary;
	}
}
