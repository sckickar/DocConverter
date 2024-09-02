using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

public class PdfXfaForm : PdfXfaField, ICloneable
{
	private PdfXfaFlowDirection m_flowDirection;

	private PdfXfaFieldCollection m_fields;

	internal XmlWriter m_dataSetWriter;

	private int m_fieldCount = 1;

	internal PdfDocument m_document;

	internal PdfArray m_imageArray = new PdfArray();

	private PdfXfaBorder m_border;

	private float m_width;

	private bool m_readOnly;

	private PdfXfaPage m_xfaPage;

	internal PdfPage m_page;

	internal PdfXfaForm m_parent;

	internal PdfXfaType m_formType;

	internal PdfXfaDocument m_xfaDocument;

	internal PointF m_startPoint = PointF.Empty;

	internal PointF m_currentPoint = PointF.Empty;

	internal SizeF m_maxSize = SizeF.Empty;

	internal string m_name = string.Empty;

	internal PdfFieldCollection m_acroFields = new PdfFieldCollection();

	internal List<string> m_subFormNames = new List<string>();

	internal List<string> m_fieldNames = new List<string>();

	internal SizeF m_size = SizeF.Empty;

	private SizeF m_maximumSize = SizeF.Empty;

	private PointF m_currentPosition = PointF.Empty;

	private SizeF m_pageSize = SizeF.Empty;

	private float m_height;

	private List<float> m_borderHeight = new List<float>();

	private int m_borderCount;

	private PdfXfaPage m_tempPage;

	internal bool m_isReadOnly;

	public PdfXfaFlowDirection FlowDirection
	{
		get
		{
			return m_flowDirection;
		}
		set
		{
			m_flowDirection = value;
		}
	}

	public PdfXfaBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (value != null)
			{
				m_border = value;
			}
		}
	}

	public PdfXfaFieldCollection Fields
	{
		get
		{
			if (m_fields == null)
			{
				m_fields = new PdfXfaFieldCollection();
			}
			return m_fields;
		}
		internal set
		{
			m_fields = value;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return m_readOnly;
		}
		set
		{
			m_readOnly = value;
		}
	}

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public PdfXfaPage Page
	{
		get
		{
			return m_xfaPage;
		}
		set
		{
			if (value != null)
			{
				m_xfaPage = value;
			}
		}
	}

	public PdfXfaForm(float width)
	{
		FlowDirection = PdfXfaFlowDirection.Vertical;
		Border = null;
		Width = width;
	}

	public PdfXfaForm(string name, float width)
	{
		FlowDirection = PdfXfaFlowDirection.Vertical;
		base.Name = name;
		Border = null;
		Width = width;
	}

	public PdfXfaForm(string name, PdfXfaPage xfaPage, float width)
	{
		FlowDirection = PdfXfaFlowDirection.Vertical;
		m_xfaPage = xfaPage;
		base.Name = name;
		Border = null;
		Width = width;
	}

	public PdfXfaForm(string name, PdfXfaFlowDirection flowDirection, float width)
	{
		FlowDirection = flowDirection;
		base.Name = name;
		Border = null;
		Width = width;
	}

	public PdfXfaForm(PdfXfaPage xfaPage, PdfXfaFlowDirection flowDirection, float width)
	{
		FlowDirection = flowDirection;
		m_xfaPage = xfaPage;
		Border = null;
		Width = width;
	}

	public PdfXfaForm(PdfXfaFlowDirection flowDirection, float width)
	{
		FlowDirection = flowDirection;
		Border = null;
		Width = width;
	}

	public PdfXfaForm(string name, PdfXfaPage xfaPage, PdfXfaFlowDirection flowDirection, float width)
	{
		FlowDirection = flowDirection;
		m_xfaPage = xfaPage;
		base.Name = name;
		Border = null;
		Width = width;
	}

	internal PdfXfaForm()
	{
	}

	internal void Save(PdfDocument document, PdfXfaType type)
	{
		m_document = document;
		if (m_formType == PdfXfaType.Dynamic)
		{
			Message();
		}
		m_formType = type;
		if (!m_document.Form.Dictionary.ContainsKey("XFA"))
		{
			XfaWriter xfaWriter = new XfaWriter();
			PdfStream pdfStream = new PdfStream();
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.OmitXmlDeclaration = true;
			xmlWriterSettings.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
			m_dataSetWriter = XmlWriter.Create(pdfStream.InternalStream, xmlWriterSettings);
			xfaWriter.StartDataSets(m_dataSetWriter);
			new PdfResources();
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfString("preamble"));
			pdfArray.Add(new PdfReferenceHolder(xfaWriter.WritePreamble()));
			pdfArray.Add(new PdfString("config"));
			pdfArray.Add(new PdfReferenceHolder(xfaWriter.WriteConfig()));
			pdfArray.Add(new PdfString("template"));
			pdfArray.Add(new PdfReferenceHolder(xfaWriter.WriteDocumentTemplate(this)));
			pdfArray.Add(new PdfString("datasets"));
			xfaWriter.EndDataSets(m_dataSetWriter);
			m_dataSetWriter.Close();
			pdfArray.Add(new PdfReferenceHolder(pdfStream));
			pdfArray.Add(new PdfString("postamble"));
			pdfArray.Add(new PdfReferenceHolder(xfaWriter.WritePostable()));
			m_document.Form.Dictionary.SetProperty("XFA", pdfArray);
			if (m_imageArray != null)
			{
				PdfDictionary pdfDictionary = new PdfDictionary();
				PdfDictionary pdfDictionary2 = new PdfDictionary();
				pdfDictionary2.SetProperty("Names", m_imageArray);
				pdfDictionary.SetProperty("XFAImages", new PdfReferenceHolder(pdfDictionary2));
				m_document.Catalog.SetProperty("Names", new PdfReferenceHolder(pdfDictionary));
			}
		}
	}

	internal void SaveMainForm(XfaWriter xfaWriter)
	{
		xfaWriter.Write.WriteStartElement("subform");
		PdfXfaForm pdfXfaForm = new PdfXfaForm();
		if (m_xfaDocument.FormName != null && m_xfaDocument.FormName != "")
		{
			pdfXfaForm.m_name = m_xfaDocument.FormName;
		}
		else
		{
			pdfXfaForm.m_name = "form1";
		}
		xfaWriter.Write.WriteAttributeString("name", pdfXfaForm.m_name);
		m_dataSetWriter.WriteStartElement(pdfXfaForm.m_name);
		xfaWriter.Write.WriteAttributeString("locale", "en_US");
		xfaWriter.Write.WriteAttributeString("layout", "tb");
		xfaWriter.Write.WriteStartElement("pageSet");
		if (m_xfaDocument.Pages.m_pages.Count > 0)
		{
			foreach (PdfXfaPage page in m_xfaDocument.Pages.m_pages)
			{
				page.Save(xfaWriter);
			}
		}
		else
		{
			m_xfaDocument.Pages.Add().Save(xfaWriter);
		}
		xfaWriter.Write.WriteEndElement();
		if (m_xfaDocument.Pages.m_pages.Count > 0)
		{
			m_xfaPage = m_xfaDocument.Pages.m_pages[0];
		}
		AddForm(xfaWriter);
		if (m_formType == PdfXfaType.Static)
		{
			pdfXfaForm.m_acroFields.Add(this, pdfXfaForm.GetSubFormName(m_name));
			m_document.Form.isXfaForm = true;
			m_document.Form.Fields.Add(pdfXfaForm, pdfXfaForm.GetSubFormName(pdfXfaForm.m_name));
		}
		m_dataSetWriter.WriteEndElement();
		xfaWriter.Write.WriteEndElement();
	}

	internal void AddForm(XfaWriter xfaWriter)
	{
		xfaWriter.Write.WriteStartElement("subform");
		if (base.Name != null && base.Name != "")
		{
			m_name = base.Name;
		}
		else
		{
			m_name = "subform" + xfaWriter.m_subFormFieldCount++;
		}
		xfaWriter.Write.WriteAttributeString("name", m_name);
		m_dataSetWriter.WriteStartElement(m_name);
		if (FlowDirection == PdfXfaFlowDirection.Horizontal)
		{
			xfaWriter.Write.WriteAttributeString("layout", "lr-tb");
		}
		else
		{
			xfaWriter.Write.WriteAttributeString("layout", "tb");
		}
		if (Width != 0f)
		{
			xfaWriter.SetSize(0f, Width, 0f, 0f);
		}
		if (ReadOnly)
		{
			xfaWriter.Write.WriteAttributeString("access", "readOnly");
		}
		if (m_xfaPage != null && m_formType == PdfXfaType.Static)
		{
			m_page = AddPdfPage(m_xfaPage);
		}
		m_tempPage = m_xfaPage;
		if (m_formType == PdfXfaType.Static)
		{
			m_pageSize = m_page.GetClientSize();
			ParseSubForm(this);
			m_size.Height += base.Margins.Bottom + base.Margins.Top + m_maximumSize.Height;
			m_currentPosition = (m_startPoint = (m_currentPoint = new PointF(base.Margins.Left, base.Margins.Top)));
			GetBackgroundHeight(this);
			m_borderHeight.Add(m_currentPoint.Y + m_maxSize.Height);
			m_currentPoint = (m_startPoint = (m_currentPosition = PointF.Empty));
			m_maxSize = SizeF.Empty;
			m_height = 0f;
		}
		xfaWriter.WriteMargins(base.Margins);
		if (Border != null)
		{
			xfaWriter.DrawBorder(Border);
		}
		if (m_formType == PdfXfaType.Static && Border != null)
		{
			PdfPen pen = Border.GetPen();
			RectangleF rectangleF = default(RectangleF);
			rectangleF = new RectangleF(0f, 0f, (Width != 0f) ? Width : m_size.Width, m_borderHeight[0]);
			m_borderCount++;
			if (Border.LeftEdge != null || Border.RightEdge != null || Border.TopEdge != null || Border.BottomEdge != null)
			{
				m_page.Graphics.DrawRectangle(Border.GetBrush(rectangleF), rectangleF);
				if (Border.LeftEdge != null)
				{
					DrawEdge(Border.LeftEdge, rectangleF.Location, new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height), m_page);
				}
				if (Border.RightEdge != null)
				{
					DrawEdge(Border.RightEdge, new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y), new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height), m_page);
				}
				if (Border.TopEdge != null)
				{
					DrawEdge(Border.TopEdge, rectangleF.Location, new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y), m_page);
				}
				if (Border.BottomEdge != null)
				{
					DrawEdge(Border.BottomEdge, new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height), new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height), m_page);
				}
			}
			else
			{
				m_page.Graphics.DrawRectangle(pen, Border.GetBrush(rectangleF), rectangleF);
			}
		}
		m_currentPosition = (m_startPoint = (m_currentPoint = new PointF(base.Margins.Left, base.Margins.Top)));
		AddSubForm(this, xfaWriter);
		m_dataSetWriter.WriteEndElement();
	}

	private void DrawEdge(PdfXfaEdge edge, PointF startPont, PointF endPoint, PdfPage page)
	{
		PdfPen pdfPen = new PdfPen(edge.Color);
		pdfPen.Width = edge.Thickness;
		pdfPen.DashStyle = GetPenDashStyle(edge.BorderStyle);
		page.Graphics.DrawLine(pdfPen, startPont, endPoint);
	}

	private PdfDashStyle GetPenDashStyle(PdfXfaBorderStyle style)
	{
		PdfDashStyle result = PdfDashStyle.Solid;
		switch (style)
		{
		case PdfXfaBorderStyle.DashDot:
			result = PdfDashStyle.DashDot;
			break;
		case PdfXfaBorderStyle.DashDotDot:
			result = PdfDashStyle.DashDotDot;
			break;
		case PdfXfaBorderStyle.Dashed:
			result = PdfDashStyle.Dash;
			break;
		case PdfXfaBorderStyle.Dotted:
			result = PdfDashStyle.Dot;
			break;
		}
		return result;
	}

	internal void AddSubForm(PdfXfaForm subForm, XfaWriter xfaWriter)
	{
		for (int i = 0; i < subForm.Fields.Count; i++)
		{
			object obj = subForm.Fields[i];
			if (obj is PdfXfaForm)
			{
				PdfXfaForm pdfXfaForm = obj as PdfXfaForm;
				pdfXfaForm.m_fieldNames = new List<string>();
				if (pdfXfaForm.Name != null && pdfXfaForm.Name != "" && pdfXfaForm.Name != string.Empty)
				{
					pdfXfaForm.m_name = pdfXfaForm.Name;
				}
				else
				{
					pdfXfaForm.m_name = "subform" + xfaWriter.m_subFormFieldCount++;
				}
				pdfXfaForm.m_formType = m_formType;
				m_dataSetWriter.WriteStartElement(pdfXfaForm.m_name);
				pdfXfaForm.m_parent = subForm;
				xfaWriter.Write.WriteStartElement("subform");
				xfaWriter.Write.WriteAttributeString("name", pdfXfaForm.m_name);
				xfaWriter.SetSize(0f, pdfXfaForm.Width, 0f, 0f);
				if (pdfXfaForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					xfaWriter.Write.WriteAttributeString("layout", "lr-tb");
				}
				else
				{
					xfaWriter.Write.WriteAttributeString("layout", "tb");
				}
				if (pdfXfaForm.ReadOnly)
				{
					xfaWriter.Write.WriteAttributeString("access", "readOnly");
				}
				if (pdfXfaForm.Border != null)
				{
					xfaWriter.DrawBorder(pdfXfaForm.Border);
				}
				xfaWriter.WriteMargins(pdfXfaForm.Margins);
				SizeF sizeF = SizeF.Empty;
				if (m_formType == PdfXfaType.Static)
				{
					if (subForm.ReadOnly)
					{
						pdfXfaForm.m_readOnly = true;
					}
					pdfXfaForm.m_startPoint = new PointF(subForm.m_startPoint.X + pdfXfaForm.Margins.Left, subForm.m_startPoint.Y + pdfXfaForm.Margins.Top);
					pdfXfaForm.m_currentPoint = (pdfXfaForm.m_currentPosition = pdfXfaForm.m_startPoint);
					sizeF = new SizeF((pdfXfaForm.Width != 0f) ? pdfXfaForm.Width : pdfXfaForm.m_size.Width, pdfXfaForm.m_size.Height);
					if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal && subForm.Width != 0f && subForm.m_currentPoint.X + sizeF.Width > subForm.Width)
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					pdfXfaForm.m_currentPoint.X += subForm.m_currentPoint.X - subForm.m_startPoint.X;
					if (pdfXfaForm.m_currentPoint.Y < subForm.m_currentPoint.Y)
					{
						pdfXfaForm.m_currentPoint.Y += subForm.m_currentPoint.Y - subForm.m_startPoint.Y;
					}
					if (pdfXfaForm.m_xfaPage != null)
					{
						pdfXfaForm.BreakPage(xfaWriter, pdfXfaForm.m_xfaPage);
						if (m_formType == PdfXfaType.Static)
						{
							m_page = AddPdfPage(pdfXfaForm.m_xfaPage);
							m_pageSize = m_page.GetClientSize();
							SetCurrentPoint(pdfXfaForm);
							pdfXfaForm.m_startPoint = new PointF(subForm.m_startPoint.X + pdfXfaForm.Margins.Left, subForm.m_startPoint.Y + pdfXfaForm.Margins.Top);
							pdfXfaForm.m_currentPoint = pdfXfaForm.m_startPoint;
						}
					}
					pdfXfaForm.m_page = m_page;
					pdfXfaForm.m_currentPosition = pdfXfaForm.m_currentPoint;
					if (pdfXfaForm.Border != null)
					{
						if (pdfXfaForm.m_xfaPage != null)
						{
							DrawBackground(pdfXfaForm);
						}
						PdfPen pen = pdfXfaForm.Border.GetPen();
						RectangleF rectangleF = default(RectangleF);
						float width = pdfXfaForm.Width;
						if (pdfXfaForm.Width == 0f)
						{
							width = pdfXfaForm.m_size.Width + pdfXfaForm.Margins.Left + pdfXfaForm.Margins.Right;
						}
						rectangleF = new RectangleF(pdfXfaForm.m_currentPoint.X - pdfXfaForm.Margins.Left, pdfXfaForm.m_currentPoint.Y - pdfXfaForm.Margins.Top, width, pdfXfaForm.m_borderHeight[pdfXfaForm.m_borderCount]);
						if (pdfXfaForm.m_startPoint.Y != pdfXfaForm.m_currentPoint.Y)
						{
							rectangleF.Height -= pdfXfaForm.m_currentPoint.Y;
						}
						rectangleF.Height += pdfXfaForm.Margins.Bottom;
						pdfXfaForm.m_borderCount++;
						if (rectangleF.Height > 0f)
						{
							if (pdfXfaForm.Border.LeftEdge != null || pdfXfaForm.Border.RightEdge != null || pdfXfaForm.Border.TopEdge != null || pdfXfaForm.Border.BottomEdge != null)
							{
								m_page.Graphics.DrawRectangle(pdfXfaForm.Border.GetBrush(rectangleF), rectangleF);
								if (pdfXfaForm.Border.LeftEdge != null)
								{
									DrawEdge(pdfXfaForm.Border.LeftEdge, rectangleF.Location, new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height), m_page);
								}
								if (pdfXfaForm.Border.RightEdge != null)
								{
									DrawEdge(pdfXfaForm.Border.RightEdge, new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y), new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height), m_page);
								}
								if (pdfXfaForm.Border.TopEdge != null)
								{
									DrawEdge(pdfXfaForm.Border.TopEdge, rectangleF.Location, new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y), m_page);
								}
								if (pdfXfaForm.Border.BottomEdge != null)
								{
									DrawEdge(pdfXfaForm.Border.BottomEdge, new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height), new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height), m_page);
								}
							}
							else
							{
								m_page.Graphics.DrawRectangle(pen, pdfXfaForm.Border.GetBrush(rectangleF), rectangleF);
							}
						}
					}
					else if (pdfXfaForm.m_xfaPage != null)
					{
						DrawBackground(pdfXfaForm);
					}
				}
				else if (pdfXfaForm.m_xfaPage != null)
				{
					pdfXfaForm.BreakPage(xfaWriter, pdfXfaForm.m_xfaPage);
				}
				AddSubForm(pdfXfaForm, xfaWriter);
				if (m_formType == PdfXfaType.Static)
				{
					string subFormName = subForm.GetSubFormName(pdfXfaForm.m_name);
					if (pdfXfaForm.m_acroFields.Count > 0)
					{
						subForm.m_acroFields.Add(pdfXfaForm, subFormName);
					}
					pdfXfaForm.m_isRendered = true;
					subForm.m_currentPoint.X += sizeF.Width;
					if (subForm.FlowDirection == PdfXfaFlowDirection.Vertical)
					{
						subForm.m_currentPoint.X = subForm.m_startPoint.X;
						if (pdfXfaForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
						{
							pdfXfaForm.m_height += pdfXfaForm.m_maxSize.Height + pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						else
						{
							pdfXfaForm.m_height += pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						if (pdfXfaForm.m_height - pdfXfaForm.Margins.Bottom != 0f)
						{
							subForm.m_currentPoint.Y += pdfXfaForm.m_height;
						}
						else
						{
							subForm.m_currentPoint.Y = pdfXfaForm.m_currentPoint.Y;
						}
						subForm.m_maxSize.Height = 0f;
					}
					else
					{
						if (pdfXfaForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
						{
							pdfXfaForm.m_height += pdfXfaForm.m_maxSize.Height + pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						else
						{
							pdfXfaForm.m_height += pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						if (pdfXfaForm.m_height - pdfXfaForm.Margins.Bottom != 0f)
						{
							if (subForm.m_maxSize.Height < pdfXfaForm.m_height)
							{
								subForm.m_maxSize.Height = pdfXfaForm.m_height;
							}
						}
						else if (subForm.m_maxSize.Height < pdfXfaForm.m_currentPoint.Y)
						{
							subForm.m_maxSize.Height = pdfXfaForm.m_currentPoint.Y;
						}
					}
				}
				xfaWriter.Write.WriteEndElement();
				m_dataSetWriter.WriteEndElement();
				continue;
			}
			if (obj is PdfXfaTextBoxField)
			{
				PdfXfaTextBoxField pdfXfaTextBoxField = obj as PdfXfaTextBoxField;
				pdfXfaTextBoxField.parent = subForm;
				m_dataSetWriter.WriteStartElement(pdfXfaTextBoxField.Name);
				m_dataSetWriter.WriteString(pdfXfaTextBoxField.Text);
				m_dataSetWriter.WriteEndElement();
				pdfXfaTextBoxField.Save(xfaWriter, m_formType);
				SetFont(m_document, pdfXfaTextBoxField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size = pdfXfaTextBoxField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaTextBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size), subForm.GetFieldName(pdfXfaTextBoxField.Name)));
					subForm.m_currentPoint.X += size.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaTextBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size.Height;
					subForm.m_height += size.Height;
				}
				if (subForm.m_maxSize.Width < size.Width)
				{
					subForm.m_maxSize.Width = size.Width;
				}
				if (subForm.m_maxSize.Height < size.Height)
				{
					subForm.m_maxSize.Height = size.Height;
				}
				continue;
			}
			if (obj is PdfXfaNumericField)
			{
				PdfXfaNumericField pdfXfaNumericField = obj as PdfXfaNumericField;
				pdfXfaNumericField.parent = subForm;
				m_dataSetWriter.WriteStartElement(pdfXfaNumericField.Name);
				if (!double.IsNaN(pdfXfaNumericField.NumericValue))
				{
					if (pdfXfaNumericField.FieldType == PdfXfaNumericType.Integer)
					{
						pdfXfaNumericField.NumericValue = (int)pdfXfaNumericField.NumericValue;
					}
					m_dataSetWriter.WriteString(pdfXfaNumericField.NumericValue.ToString());
				}
				m_dataSetWriter.WriteEndElement();
				pdfXfaNumericField.Save(xfaWriter);
				SetFont(m_document, pdfXfaNumericField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size2 = pdfXfaNumericField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size2.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size2.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaNumericField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size2), subForm.GetFieldName(pdfXfaNumericField.Name)));
					subForm.m_currentPoint.X += size2.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size2.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaNumericField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size2), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size2.Height;
					subForm.m_height += size2.Height;
				}
				if (subForm.m_maxSize.Width < size2.Width)
				{
					subForm.m_maxSize.Width = size2.Width;
				}
				if (subForm.m_maxSize.Height < size2.Height)
				{
					subForm.m_maxSize.Height = size2.Height;
				}
				continue;
			}
			if (obj is PdfXfaTextElement)
			{
				PdfXfaTextElement pdfXfaTextElement = obj as PdfXfaTextElement;
				pdfXfaTextElement.parent = subForm;
				pdfXfaTextElement.Save(xfaWriter);
				SetFont(m_document, pdfXfaTextElement.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size3 = pdfXfaTextElement.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size3.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size3.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaTextElement.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size3));
					subForm.m_currentPoint.X += size3.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size3.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaTextElement.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size3));
					subForm.m_currentPoint.Y += size3.Height;
					subForm.m_height += size3.Height;
				}
				if (subForm.m_maxSize.Width < size3.Width)
				{
					subForm.m_maxSize.Width = size3.Width;
				}
				if (subForm.m_maxSize.Height < size3.Height)
				{
					subForm.m_maxSize.Height = size3.Height;
				}
				continue;
			}
			if (obj is PdfXfaImage)
			{
				string text = "ImageReference" + m_fieldCount++;
				PdfXfaImage pdfXfaImage = obj as PdfXfaImage;
				if (!pdfXfaImage.isBase64Type)
				{
					m_imageArray.Add(new PdfString(text));
					m_imageArray.Add(new PdfReferenceHolder((obj as PdfXfaImage).ImageStream));
				}
				pdfXfaImage.parent = subForm;
				pdfXfaImage.Save(m_fieldCount++, text, xfaWriter);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size4 = pdfXfaImage.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size4.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size4.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaImage.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size4));
					subForm.m_currentPoint.X += size4.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size4.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaImage.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size4));
					subForm.m_currentPoint.Y += size4.Height;
					subForm.m_height += size4.Height;
				}
				if (subForm.m_maxSize.Width < size4.Width)
				{
					subForm.m_maxSize.Width = size4.Width;
				}
				if (subForm.m_maxSize.Height < size4.Height)
				{
					subForm.m_maxSize.Height = size4.Height;
				}
				continue;
			}
			if (obj is PdfXfaLine)
			{
				PdfXfaLine pdfXfaLine = obj as PdfXfaLine;
				pdfXfaLine.parent = subForm;
				pdfXfaLine.Save(xfaWriter);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size5 = pdfXfaLine.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size5.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size5.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaLine.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size5));
					subForm.m_currentPoint.X += size5.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size5.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaLine.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size5));
					subForm.m_currentPoint.Y += size5.Height;
					subForm.m_height += size5.Height;
				}
				if (subForm.m_maxSize.Width < size5.Width)
				{
					subForm.m_maxSize.Width = size5.Width;
				}
				if (subForm.m_maxSize.Height < size5.Height)
				{
					subForm.m_maxSize.Height = size5.Height;
				}
				continue;
			}
			if (obj is PdfXfaCheckBoxField)
			{
				PdfXfaCheckBoxField pdfXfaCheckBoxField = obj as PdfXfaCheckBoxField;
				pdfXfaCheckBoxField.parent = subForm;
				m_dataSetWriter.WriteStartElement(pdfXfaCheckBoxField.Name);
				if (pdfXfaCheckBoxField.IsChecked)
				{
					m_dataSetWriter.WriteString("1");
				}
				m_dataSetWriter.WriteEndElement();
				pdfXfaCheckBoxField.Save(xfaWriter);
				SetFont(m_document, pdfXfaCheckBoxField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size6 = pdfXfaCheckBoxField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size6.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size6.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaCheckBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size6), subForm.GetFieldName(pdfXfaCheckBoxField.Name)));
					subForm.m_currentPoint.X += size6.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size6.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaCheckBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size6), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size6.Height;
					subForm.m_height += size6.Height;
				}
				if (subForm.m_maxSize.Width < size6.Width)
				{
					subForm.m_maxSize.Width = size6.Width;
				}
				if (subForm.m_maxSize.Height < size6.Height)
				{
					subForm.m_maxSize.Height = size6.Height;
				}
				continue;
			}
			if (obj is PdfXfaRadioButtonGroup)
			{
				PdfXfaRadioButtonGroup pdfXfaRadioButtonGroup = obj as PdfXfaRadioButtonGroup;
				string text2 = "group1";
				if (pdfXfaRadioButtonGroup.Name != null && pdfXfaRadioButtonGroup.Name != "")
				{
					text2 = pdfXfaRadioButtonGroup.Name;
				}
				pdfXfaRadioButtonGroup.parent = subForm;
				m_dataSetWriter.WriteStartElement(text2);
				pdfXfaRadioButtonGroup.Save(xfaWriter);
				if (pdfXfaRadioButtonGroup.selectedItem > 0)
				{
					m_dataSetWriter.WriteString(pdfXfaRadioButtonGroup.selectedItem.ToString());
				}
				m_dataSetWriter.WriteEndElement();
				foreach (PdfXfaRadioButtonField radio in pdfXfaRadioButtonGroup.m_radioList)
				{
					SetFont(m_document, radio.Font);
				}
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				PdfRadioButtonListField pdfRadioButtonListField = new PdfRadioButtonListField(m_page, subForm.GetFieldName(text2));
				pdfRadioButtonListField.isXfa = true;
				if (pdfXfaRadioButtonGroup.ReadOnly || subForm.ReadOnly)
				{
					pdfRadioButtonListField.ReadOnly = true;
				}
				float num = 0f;
				if (subForm.Width != 0f && subForm.m_currentPoint.X + pdfXfaRadioButtonGroup.Size.Width > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
				{
					subForm.m_currentPoint.X = subForm.m_startPoint.X;
					subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
					subForm.m_height += subForm.m_maxSize.Height;
					subForm.m_maxSize.Height = 0f;
				}
				subForm.m_currentPoint.Y += pdfXfaRadioButtonGroup.Margins.Top;
				subForm.m_currentPoint.X += pdfXfaRadioButtonGroup.Margins.Left;
				foreach (PdfXfaRadioButtonField radio2 in pdfXfaRadioButtonGroup.m_radioList)
				{
					radio2.parent = pdfXfaRadioButtonGroup;
					SizeF size7 = radio2.GetSize();
					if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
					{
						if (subForm.Width != 0f)
						{
							if (subForm.m_currentPoint.X + size7.Width - subForm.m_startPoint.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
							{
								subForm.m_currentPoint.X = subForm.m_startPoint.X;
								subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
								subForm.m_maxSize.Height = 0f;
							}
						}
						else if (subForm.m_currentPoint.X + size7.Width - subForm.m_startPoint.X > subForm.m_size.Width)
						{
							subForm.m_currentPoint.X = subForm.m_startPoint.X;
							subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
							subForm.m_maxSize.Height = 0f;
						}
						if (subForm.m_currentPoint.Y + size7.Height > m_page.GetClientSize().Height - (subForm.Margins.Top + subForm.Margins.Bottom))
						{
							m_page = m_page.Section.Pages.Add();
							subForm.m_currentPoint = subForm.m_startPoint;
							SetCurrentPoint(subForm);
							DrawBackground(subForm);
							subForm.m_height = 0f;
						}
						pdfRadioButtonListField.Items.Add(radio2.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, new SizeF(size7.Width, size7.Height))));
						if (pdfXfaRadioButtonGroup.FlowDirection == PdfXfaFlowDirection.Vertical)
						{
							subForm.m_currentPoint.Y += size7.Height;
						}
						else
						{
							subForm.m_currentPoint.X += size7.Width;
						}
					}
					else
					{
						if (subForm.m_currentPoint.Y + size7.Height > m_page.GetClientSize().Height - (subForm.Margins.Top + subForm.Margins.Bottom))
						{
							m_page = m_page.Section.Pages.Add();
							subForm.m_currentPoint = subForm.m_startPoint;
							SetCurrentPoint(subForm);
							DrawBackground(subForm);
							subForm.m_height = 0f;
						}
						pdfRadioButtonListField.Items.Add(radio2.SaveAcroForm(subForm.m_page, new RectangleF(subForm.m_currentPoint, new SizeF(size7.Width, size7.Height))));
						if (pdfXfaRadioButtonGroup.FlowDirection == PdfXfaFlowDirection.Vertical)
						{
							subForm.m_currentPoint.Y += size7.Height;
						}
						else
						{
							subForm.m_currentPoint.X += size7.Width;
						}
					}
					if (subForm.m_maxSize.Width < size7.Width)
					{
						subForm.m_maxSize.Width = size7.Width;
					}
					if (subForm.m_maxSize.Height < size7.Height)
					{
						subForm.m_maxSize.Height = size7.Height;
					}
					if (num < size7.Width)
					{
						num = size7.Width;
					}
				}
				subForm.m_acroFields.Add(pdfRadioButtonListField);
				if (pdfXfaRadioButtonGroup.FlowDirection == PdfXfaFlowDirection.Vertical)
				{
					subForm.m_currentPoint.X += num;
				}
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					subForm.m_currentPoint.Y -= pdfXfaRadioButtonGroup.Size.Height;
				}
				if (subForm.m_maxSize.Height < pdfXfaRadioButtonGroup.Size.Height)
				{
					subForm.m_maxSize.Height = pdfXfaRadioButtonGroup.Size.Height;
				}
				if (pdfXfaRadioButtonGroup.selectedItem > 0)
				{
					pdfRadioButtonListField.SelectedIndex = pdfXfaRadioButtonGroup.selectedItem - 1;
				}
				continue;
			}
			if (obj is PdfXfaButtonField)
			{
				PdfXfaButtonField pdfXfaButtonField = obj as PdfXfaButtonField;
				pdfXfaButtonField.parent = subForm;
				pdfXfaButtonField.Save(xfaWriter);
				SetFont(m_document, pdfXfaButtonField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size8 = pdfXfaButtonField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size8.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size8.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaButtonField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size8), subForm.GetFieldName(pdfXfaButtonField.Name)));
					subForm.m_currentPoint.X += size8.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size8.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaButtonField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size8), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size8.Height;
					subForm.m_height += size8.Height;
				}
				if (subForm.m_maxSize.Width < size8.Width)
				{
					subForm.m_maxSize.Width = size8.Width;
				}
				if (subForm.m_maxSize.Height < size8.Height)
				{
					subForm.m_maxSize.Height = size8.Height;
				}
				continue;
			}
			if (obj is PdfXfaListBoxField)
			{
				PdfXfaListBoxField pdfXfaListBoxField = obj as PdfXfaListBoxField;
				pdfXfaListBoxField.parent = subForm;
				m_dataSetWriter.WriteStartElement(pdfXfaListBoxField.Name);
				if (pdfXfaListBoxField.SelectedValue != string.Empty)
				{
					m_dataSetWriter.WriteString(pdfXfaListBoxField.SelectedValue);
				}
				else if (pdfXfaListBoxField.SelectedIndex != -1 && pdfXfaListBoxField.SelectedIndex < pdfXfaListBoxField.Items.Count)
				{
					m_dataSetWriter.WriteString(pdfXfaListBoxField.Items[pdfXfaListBoxField.SelectedIndex]);
				}
				m_dataSetWriter.WriteEndElement();
				pdfXfaListBoxField.Save(xfaWriter);
				SetFont(m_document, pdfXfaListBoxField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size9 = pdfXfaListBoxField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size9.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size9.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaListBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size9), subForm.GetFieldName(pdfXfaListBoxField.Name)));
					subForm.m_currentPoint.X += size9.Width;
					subForm.m_height += size9.Height;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size9.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaListBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size9), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size9.Height;
				}
				if (subForm.m_maxSize.Width < size9.Width)
				{
					subForm.m_maxSize.Width = size9.Width;
				}
				if (subForm.m_maxSize.Height < size9.Height)
				{
					subForm.m_maxSize.Height = size9.Height;
				}
				continue;
			}
			if (obj is PdfXfaComboBoxField)
			{
				PdfXfaComboBoxField pdfXfaComboBoxField = obj as PdfXfaComboBoxField;
				pdfXfaComboBoxField.parent = subForm;
				m_dataSetWriter.WriteStartElement(pdfXfaComboBoxField.Name);
				if (pdfXfaComboBoxField.SelectedValue != string.Empty)
				{
					if (pdfXfaComboBoxField.Items.Contains(pdfXfaComboBoxField.SelectedValue) || pdfXfaComboBoxField.AllowTextEntry)
					{
						m_dataSetWriter.WriteString(pdfXfaComboBoxField.SelectedValue);
					}
				}
				else if (pdfXfaComboBoxField.SelectedIndex != -1 && pdfXfaComboBoxField.SelectedIndex < pdfXfaComboBoxField.Items.Count)
				{
					m_dataSetWriter.WriteString(pdfXfaComboBoxField.Items[pdfXfaComboBoxField.SelectedIndex]);
				}
				m_dataSetWriter.WriteEndElement();
				pdfXfaComboBoxField.Save(xfaWriter);
				SetFont(m_document, pdfXfaComboBoxField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size10 = pdfXfaComboBoxField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size10.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size10.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaComboBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size10), subForm.GetFieldName(pdfXfaComboBoxField.Name)));
					subForm.m_currentPoint.X += size10.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size10.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaComboBoxField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size10), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size10.Height;
					subForm.m_height += size10.Height;
				}
				if (subForm.m_maxSize.Width < size10.Width)
				{
					subForm.m_maxSize.Width = size10.Width;
				}
				if (subForm.m_maxSize.Height < size10.Height)
				{
					subForm.m_maxSize.Height = size10.Height;
				}
				continue;
			}
			if (obj is PdfXfaDateTimeField)
			{
				PdfXfaDateTimeField pdfXfaDateTimeField = obj as PdfXfaDateTimeField;
				pdfXfaDateTimeField.parent = subForm;
				_ = pdfXfaDateTimeField.Value;
				m_dataSetWriter.WriteStartElement(pdfXfaDateTimeField.Name);
				if (pdfXfaDateTimeField.isSet)
				{
					if (pdfXfaDateTimeField.Format == PdfXfaDateTimeFormat.Date)
					{
						m_dataSetWriter.WriteString(pdfXfaDateTimeField.Value.ToString(xfaWriter.GetDatePattern(pdfXfaDateTimeField.DatePattern)));
					}
					else if (pdfXfaDateTimeField.Format == PdfXfaDateTimeFormat.DateTime)
					{
						m_dataSetWriter.WriteString(pdfXfaDateTimeField.Value.ToString(xfaWriter.GetDateTimePattern(pdfXfaDateTimeField.DatePattern, pdfXfaDateTimeField.TimePattern)));
					}
					else if (pdfXfaDateTimeField.Format == PdfXfaDateTimeFormat.Time)
					{
						m_dataSetWriter.WriteString(pdfXfaDateTimeField.Value.ToString(xfaWriter.GetTimePattern(pdfXfaDateTimeField.TimePattern)));
					}
				}
				m_dataSetWriter.WriteEndElement();
				pdfXfaDateTimeField.Save(xfaWriter);
				SetFont(m_document, pdfXfaDateTimeField.Font);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size11 = pdfXfaDateTimeField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size11.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size11.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaDateTimeField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size11), subForm.GetFieldName(pdfXfaDateTimeField.Name)));
					subForm.m_currentPoint.X += size11.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size11.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					subForm.m_acroFields.Add(pdfXfaDateTimeField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size11), subForm.GetFieldName(base.Name)));
					subForm.m_currentPoint.Y += size11.Height;
					subForm.m_height += size11.Height;
				}
				if (subForm.m_maxSize.Width < size11.Width)
				{
					subForm.m_maxSize.Width = size11.Width;
				}
				if (subForm.m_maxSize.Height < size11.Height)
				{
					subForm.m_maxSize.Height = size11.Height;
				}
				continue;
			}
			if (obj is PdfXfaCircleField)
			{
				PdfXfaCircleField pdfXfaCircleField = obj as PdfXfaCircleField;
				pdfXfaCircleField.parent = subForm;
				pdfXfaCircleField.Save(xfaWriter);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size12 = pdfXfaCircleField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size12.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size12.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaCircleField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size12));
					subForm.m_currentPoint.X += size12.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size12.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaCircleField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size12));
					subForm.m_currentPoint.Y += size12.Height;
					subForm.m_height += size12.Height;
				}
				if (subForm.m_maxSize.Width < size12.Width)
				{
					subForm.m_maxSize.Width = size12.Width;
				}
				if (subForm.m_maxSize.Height < size12.Height)
				{
					subForm.m_maxSize.Height = size12.Height;
				}
				continue;
			}
			if (obj is PdfXfaRectangleField)
			{
				PdfXfaRectangleField pdfXfaRectangleField = obj as PdfXfaRectangleField;
				pdfXfaRectangleField.parent = subForm;
				pdfXfaRectangleField.Save(xfaWriter);
				if (m_formType != PdfXfaType.Static)
				{
					continue;
				}
				SizeF size13 = pdfXfaRectangleField.GetSize();
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					if (subForm.Width != 0f && subForm.m_currentPoint.X + size13.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
					{
						subForm.m_currentPoint.X = subForm.m_currentPosition.X;
						subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
						subForm.m_height += subForm.m_maxSize.Height;
						subForm.m_maxSize.Height = 0f;
					}
					if (subForm.m_currentPoint.Y + size13.Height > m_pageSize.Height - (subForm.Margins.Top + subForm.Margins.Bottom))
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaRectangleField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size13));
					subForm.m_currentPoint.X += size13.Width;
				}
				else
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					if (subForm.m_currentPoint.Y + size13.Height > m_pageSize.Height - subForm.Margins.Bottom)
					{
						m_page = m_page.Section.Pages.Add();
						m_pageSize = m_page.GetClientSize();
						subForm.m_currentPoint = subForm.m_startPoint;
						SetCurrentPoint(subForm);
						DrawBackground(subForm);
						subForm.m_height = 0f;
					}
					pdfXfaRectangleField.SaveAcroForm(m_page, new RectangleF(subForm.m_currentPoint, size13));
					subForm.m_currentPoint.Y += size13.Height;
					subForm.m_height += size13.Height;
				}
				if (subForm.m_maxSize.Width < size13.Width)
				{
					subForm.m_maxSize.Width = size13.Width;
				}
				if (subForm.m_maxSize.Height < size13.Height)
				{
					subForm.m_maxSize.Height = size13.Height;
				}
				continue;
			}
			throw new NotImplementedException();
		}
	}

	internal string GetFieldName(string name)
	{
		string text = name + "[0]";
		if (m_fieldNames.Count > 0)
		{
			int num = 0;
			while (m_fieldNames.Contains(text))
			{
				num++;
				text = name + "[" + num + "]";
			}
		}
		m_fieldNames.Add(text);
		return text;
	}

	internal string GetSubFormName(string name)
	{
		string text = name + "[0]";
		if (m_subFormNames.Count > 0)
		{
			int num = 0;
			while (m_subFormNames.Contains(text))
			{
				num++;
				text = name + "[" + num + "]";
			}
		}
		m_subFormNames.Add(text);
		return text;
	}

	internal void AddSubForm(XfaWriter xfaWriter)
	{
		string empty = string.Empty;
		empty = ((base.Name == null || !(base.Name != "")) ? ("subform" + xfaWriter.m_subFormFieldCount++) : base.Name);
		m_dataSetWriter.WriteStartElement(empty);
		xfaWriter.Write.WriteStartElement("subform");
		xfaWriter.Write.WriteAttributeString("name", empty);
		xfaWriter.SetSize(0f, Width, 0f, 0f);
		if (FlowDirection == PdfXfaFlowDirection.Horizontal)
		{
			xfaWriter.Write.WriteAttributeString("layout", "lr-tb");
		}
		else
		{
			xfaWriter.Write.WriteAttributeString("layout", "tb");
		}
		if (ReadOnly)
		{
			xfaWriter.Write.WriteAttributeString("access", "readOnly");
		}
		if (m_xfaPage != null && !m_xfaPage.isAdded)
		{
			m_xfaPage.Save(xfaWriter);
		}
		xfaWriter.WriteMargins(base.Margins);
		m_formType = PdfXfaType.Dynamic;
		AddSubForm(this, xfaWriter);
		xfaWriter.Write.WriteEndElement();
		m_dataSetWriter.WriteEndElement();
	}

	internal void BreakPage(XfaWriter writer, PdfXfaPage page)
	{
		writer.Write.WriteStartElement("breakBefore");
		writer.Write.WriteAttributeString("targetType", "pageArea");
		writer.Write.WriteAttributeString("target", "Page" + page.pageId);
		writer.Write.WriteAttributeString("startNew", "1");
		writer.Write.WriteEndElement();
		page.isBreaked = true;
	}

	private void GetBackgroundHeight(PdfXfaForm subForm)
	{
		for (int i = 0; i < subForm.Fields.Count; i++)
		{
			object obj = subForm.Fields[i];
			if (obj is PdfXfaForm)
			{
				PdfXfaForm pdfXfaForm = obj as PdfXfaForm;
				pdfXfaForm.m_startPoint = new PointF(subForm.m_startPoint.X + pdfXfaForm.Margins.Left, subForm.m_startPoint.Y + pdfXfaForm.Margins.Top);
				pdfXfaForm.m_currentPoint = (pdfXfaForm.m_currentPosition = pdfXfaForm.m_startPoint);
				SizeF sizeF = new SizeF((pdfXfaForm.Width != 0f) ? pdfXfaForm.Width : pdfXfaForm.m_size.Width, pdfXfaForm.m_size.Height);
				if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal && subForm.Width != 0f && subForm.m_currentPoint.X + sizeF.Width > subForm.Width)
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
					subForm.m_height += subForm.m_maxSize.Height;
					subForm.m_maxSize.Height = 0f;
				}
				pdfXfaForm.m_currentPoint.X += subForm.m_currentPoint.X - subForm.m_startPoint.X;
				if (pdfXfaForm.m_currentPoint.Y < subForm.m_currentPoint.Y)
				{
					pdfXfaForm.m_currentPoint.Y += subForm.m_currentPoint.Y - subForm.m_startPoint.Y;
				}
				if (pdfXfaForm.m_xfaPage != null && m_formType == PdfXfaType.Static)
				{
					m_tempPage = m_xfaPage;
					pdfXfaForm.m_startPoint = new PointF(subForm.m_startPoint.X + pdfXfaForm.Margins.Left, subForm.m_startPoint.Y + pdfXfaForm.Margins.Top);
					pdfXfaForm.m_currentPoint = pdfXfaForm.m_startPoint;
					subForm.m_borderHeight.Add(subForm.m_currentPoint.Y + subForm.m_maxSize.Height + subForm.Margins.Bottom);
				}
				pdfXfaForm.m_currentPosition = pdfXfaForm.m_currentPoint;
				GetBackgroundHeight(pdfXfaForm);
				if (m_formType == PdfXfaType.Static)
				{
					subForm.m_currentPoint.X += sizeF.Width;
					if (subForm.FlowDirection == PdfXfaFlowDirection.Vertical)
					{
						subForm.m_currentPoint.X = subForm.m_startPoint.X;
						if (pdfXfaForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
						{
							pdfXfaForm.m_height += pdfXfaForm.m_maxSize.Height + pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						else
						{
							pdfXfaForm.m_height += pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						if (pdfXfaForm.m_height - pdfXfaForm.Margins.Bottom != 0f)
						{
							subForm.m_currentPoint.Y += pdfXfaForm.m_height;
						}
						else
						{
							subForm.m_currentPoint.Y = pdfXfaForm.m_currentPoint.Y;
						}
						subForm.m_maxSize.Height = 0f;
					}
					else
					{
						if (pdfXfaForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
						{
							pdfXfaForm.m_height += pdfXfaForm.m_maxSize.Height + pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						else
						{
							pdfXfaForm.m_height += pdfXfaForm.Margins.Bottom + pdfXfaForm.Margins.Top;
						}
						if (pdfXfaForm.m_height - pdfXfaForm.Margins.Bottom != 0f)
						{
							if (subForm.m_maxSize.Height < pdfXfaForm.m_height)
							{
								subForm.m_maxSize.Height = pdfXfaForm.m_height;
							}
						}
						else if (subForm.m_maxSize.Height < pdfXfaForm.m_currentPoint.Y)
						{
							subForm.m_maxSize.Height = pdfXfaForm.m_currentPoint.Y;
						}
					}
				}
				if (pdfXfaForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					pdfXfaForm.m_borderHeight.Add(pdfXfaForm.m_currentPoint.Y + pdfXfaForm.Margins.Bottom + pdfXfaForm.m_maxSize.Height);
				}
				else
				{
					pdfXfaForm.m_borderHeight.Add(pdfXfaForm.m_currentPoint.Y + pdfXfaForm.Margins.Bottom);
				}
				pdfXfaForm.m_currentPoint = (pdfXfaForm.m_startPoint = (pdfXfaForm.m_currentPosition = PointF.Empty));
				pdfXfaForm.m_maxSize = SizeF.Empty;
				pdfXfaForm.m_height = 0f;
				continue;
			}
			SizeF sizeF2 = SizeF.Empty;
			if (obj is PdfXfaTextBoxField)
			{
				sizeF2 = (obj as PdfXfaTextBoxField).GetSize();
			}
			else if (obj is PdfXfaNumericField)
			{
				sizeF2 = (obj as PdfXfaNumericField).GetSize();
			}
			else if (obj is PdfXfaTextElement)
			{
				sizeF2 = (obj as PdfXfaTextElement).GetSize();
			}
			else if (obj is PdfXfaCheckBoxField)
			{
				sizeF2 = (obj as PdfXfaCheckBoxField).GetSize();
			}
			else if (obj is PdfXfaListBoxField)
			{
				sizeF2 = (obj as PdfXfaListBoxField).GetSize();
			}
			else if (obj is PdfXfaComboBoxField)
			{
				sizeF2 = (obj as PdfXfaComboBoxField).GetSize();
			}
			else if (obj is PdfXfaButtonField)
			{
				sizeF2 = (obj as PdfXfaButtonField).GetSize();
			}
			else if (obj is PdfXfaCircleField)
			{
				sizeF2 = (obj as PdfXfaCircleField).GetSize();
			}
			else if (obj is PdfXfaRectangleField)
			{
				sizeF2 = (obj as PdfXfaRectangleField).GetSize();
			}
			else if (obj is PdfXfaLine)
			{
				sizeF2 = (obj as PdfXfaLine).GetSize();
			}
			else if (obj is PdfXfaDateTimeField)
			{
				sizeF2 = (obj as PdfXfaDateTimeField).GetSize();
			}
			else if (obj is PdfXfaImage)
			{
				sizeF2 = (obj as PdfXfaImage).GetSize();
			}
			else if (obj is PdfXfaRadioButtonGroup)
			{
				sizeF2 = (obj as PdfXfaRadioButtonGroup).Size;
			}
			if (subForm.FlowDirection == PdfXfaFlowDirection.Horizontal)
			{
				if (subForm.Width != 0f && subForm.m_currentPoint.X + sizeF2.Width - subForm.m_currentPosition.X > subForm.Width - (subForm.Margins.Left + subForm.Margins.Right))
				{
					subForm.m_currentPoint.X = subForm.m_currentPosition.X;
					subForm.m_currentPoint.Y += subForm.m_maxSize.Height;
					subForm.m_height += subForm.m_maxSize.Height;
					subForm.m_maxSize.Height = 0f;
				}
				if (subForm.m_currentPoint.Y + sizeF2.Height > m_tempPage.GetClientSize().Height - (subForm.Margins.Top + subForm.Margins.Bottom))
				{
					SetBackgroundHeight(subForm, subForm.m_currentPoint.Y);
					subForm.m_currentPoint = subForm.m_startPoint;
					SetCurrentPoint(subForm);
					subForm.m_height = 0f;
				}
				subForm.m_currentPoint.X += sizeF2.Width;
			}
			else
			{
				subForm.m_currentPoint.X = subForm.m_currentPosition.X;
				if (subForm.m_currentPoint.Y + sizeF2.Height > m_tempPage.GetClientSize().Height - subForm.Margins.Bottom)
				{
					SetBackgroundHeight(subForm, subForm.m_currentPoint.Y);
					subForm.m_currentPoint = subForm.m_startPoint;
					SetCurrentPoint(subForm);
					subForm.m_height = 0f;
				}
				subForm.m_currentPoint.Y += sizeF2.Height;
				subForm.m_height += sizeF2.Height;
			}
			if (subForm.m_maxSize.Width < sizeF2.Width)
			{
				subForm.m_maxSize.Width = sizeF2.Width;
			}
			if (subForm.m_maxSize.Height < sizeF2.Height)
			{
				subForm.m_maxSize.Height = sizeF2.Height;
			}
		}
	}

	private void DrawBackground(PdfXfaForm tempForm)
	{
		if (tempForm.m_parent != null)
		{
			DrawBackground(tempForm.m_parent);
		}
		if (tempForm.Border == null)
		{
			return;
		}
		PdfPen pen = tempForm.Border.GetPen();
		RectangleF rectangleF = default(RectangleF);
		float width = tempForm.Width;
		if (tempForm.Width == 0f)
		{
			width = tempForm.m_size.Width + tempForm.Margins.Left + tempForm.Margins.Right;
		}
		rectangleF = new RectangleF(tempForm.m_currentPoint.X - tempForm.Margins.Left, tempForm.m_currentPoint.Y - tempForm.Margins.Top, width, tempForm.m_borderHeight[tempForm.m_borderCount] - tempForm.m_currentPoint.Y);
		tempForm.m_borderCount++;
		if (tempForm.m_parent == null)
		{
			rectangleF.Height += m_currentPoint.Y;
		}
		if (tempForm.m_borderHeight.Count == tempForm.m_borderCount && tempForm.m_parent == null)
		{
			rectangleF.Height += tempForm.Margins.Bottom;
		}
		if (tempForm.Border.LeftEdge != null || tempForm.Border.RightEdge != null || tempForm.Border.TopEdge != null || tempForm.Border.BottomEdge != null)
		{
			m_page.Graphics.DrawRectangle(tempForm.Border.GetBrush(rectangleF), rectangleF);
			if (tempForm.Border.LeftEdge != null)
			{
				DrawEdge(tempForm.Border.LeftEdge, rectangleF.Location, new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height), m_page);
			}
			if (tempForm.Border.RightEdge != null)
			{
				DrawEdge(tempForm.Border.RightEdge, new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y), new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height), m_page);
			}
			if (tempForm.Border.TopEdge != null)
			{
				DrawEdge(tempForm.Border.TopEdge, rectangleF.Location, new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y), m_page);
			}
			if (tempForm.Border.BottomEdge != null)
			{
				DrawEdge(tempForm.Border.BottomEdge, new PointF(rectangleF.X, rectangleF.Y + rectangleF.Height), new PointF(rectangleF.X + rectangleF.Width, rectangleF.Y + rectangleF.Height), m_page);
			}
		}
		else
		{
			m_page.Graphics.DrawRectangle(pen, tempForm.Border.GetBrush(rectangleF), rectangleF);
		}
	}

	private PdfPage AddPdfPage(PdfXfaPage xfaPage)
	{
		PdfSection pdfSection = m_document.Sections.Add();
		pdfSection.PageSettings.Margins.Left = xfaPage.pageSettings.Margins.Left;
		pdfSection.PageSettings.Margins.Right = xfaPage.pageSettings.Margins.Right;
		pdfSection.PageSettings.Margins.Top = xfaPage.pageSettings.Margins.Top;
		pdfSection.PageSettings.Margins.Bottom = xfaPage.pageSettings.Margins.Bottom;
		pdfSection.PageSettings.Size = new SizeF(xfaPage.pageSettings.PageSize.Width, xfaPage.pageSettings.PageSize.Height);
		if (xfaPage.pageSettings.PageOrientation == PdfXfaPageOrientation.Landscape)
		{
			pdfSection.PageSettings.Orientation = PdfPageOrientation.Landscape;
		}
		return pdfSection.Pages.Add();
	}

	private void SetCurrentPoint(PdfXfaForm subForm)
	{
		if (subForm.m_parent != null)
		{
			SetCurrentPoint(subForm.m_parent);
		}
		subForm.m_currentPoint = subForm.m_startPoint;
		subForm.m_height = 0f;
	}

	private void SetBackgroundHeight(PdfXfaForm subForm, float height)
	{
		height += subForm.Margins.Bottom;
		if (subForm.m_parent != null)
		{
			SetBackgroundHeight(subForm.m_parent, height);
		}
		subForm.m_borderHeight.Add(height);
	}

	private void ParseSubForm(PdfXfaForm subform)
	{
		float num = 0f;
		if (subform.Width != 0f)
		{
			num = subform.Width - (subform.Margins.Left + subform.Margins.Right);
		}
		for (int i = 0; i < subform.Fields.Count; i++)
		{
			object obj = subform.Fields[i];
			if (obj is PdfXfaForm)
			{
				PdfXfaForm pdfXfaForm = obj as PdfXfaForm;
				pdfXfaForm.m_parent = subform;
				pdfXfaForm.m_size = SizeF.Empty;
				pdfXfaForm.m_maximumSize = SizeF.Empty;
				ParseSubForm(pdfXfaForm);
				if (subform.FlowDirection == PdfXfaFlowDirection.Vertical)
				{
					pdfXfaForm.m_size.Height += pdfXfaForm.m_maximumSize.Height;
					pdfXfaForm.m_size.Height += pdfXfaForm.Margins.Top + pdfXfaForm.Margins.Bottom;
					subform.m_size.Height += pdfXfaForm.m_size.Height;
					if (pdfXfaForm.Width != 0f)
					{
						if (subform.m_size.Width < pdfXfaForm.Width)
						{
							subform.m_size.Width = pdfXfaForm.Width;
						}
					}
					else if (subform.m_size.Width < pdfXfaForm.m_size.Width)
					{
						subform.m_size.Width = pdfXfaForm.m_size.Width;
					}
					continue;
				}
				pdfXfaForm.m_size.Height += pdfXfaForm.m_maximumSize.Height;
				pdfXfaForm.m_size.Height += pdfXfaForm.Margins.Top + pdfXfaForm.Margins.Bottom;
				if (subform.Width == 0f)
				{
					subform.m_size.Width += pdfXfaForm.Width;
					if (subform.m_maximumSize.Height < pdfXfaForm.m_size.Height)
					{
						subform.m_maximumSize.Height = pdfXfaForm.m_size.Height;
					}
					if (pdfXfaForm.Width == 0f)
					{
						subform.m_size.Width += pdfXfaForm.m_size.Width;
					}
					continue;
				}
				if (pdfXfaForm.Width != 0f)
				{
					if (subform.m_size.Width + pdfXfaForm.Width >= subform.Width)
					{
						subform.m_size.Height += pdfXfaForm.m_size.Height;
						continue;
					}
					subform.m_size.Width += pdfXfaForm.Width;
					if (subform.m_maximumSize.Height < pdfXfaForm.m_size.Height)
					{
						subform.m_maximumSize.Height = pdfXfaForm.m_size.Height;
					}
					continue;
				}
				if (subform.m_maximumSize.Height != 0f)
				{
					subform.m_size.Height += subform.m_maximumSize.Height;
					subform.m_maximumSize.Height = 0f;
				}
				subform.m_size.Height += pdfXfaForm.m_size.Height;
				if (pdfXfaForm.m_size.Width < pdfXfaForm.m_maximumSize.Width)
				{
					pdfXfaForm.m_size.Width = pdfXfaForm.m_maximumSize.Width;
				}
				continue;
			}
			SizeF sizeF = SizeF.Empty;
			if (obj is PdfXfaTextBoxField)
			{
				sizeF = (obj as PdfXfaTextBoxField).GetSize();
			}
			else if (obj is PdfXfaNumericField)
			{
				sizeF = (obj as PdfXfaNumericField).GetSize();
			}
			else if (obj is PdfXfaTextElement)
			{
				sizeF = (obj as PdfXfaTextElement).GetSize();
			}
			else if (obj is PdfXfaCheckBoxField)
			{
				sizeF = (obj as PdfXfaCheckBoxField).GetSize();
			}
			else if (obj is PdfXfaListBoxField)
			{
				sizeF = (obj as PdfXfaListBoxField).GetSize();
			}
			else if (obj is PdfXfaComboBoxField)
			{
				sizeF = (obj as PdfXfaComboBoxField).GetSize();
			}
			else if (obj is PdfXfaButtonField)
			{
				sizeF = (obj as PdfXfaButtonField).GetSize();
			}
			else if (obj is PdfXfaCircleField)
			{
				sizeF = (obj as PdfXfaCircleField).GetSize();
			}
			else if (obj is PdfXfaRectangleField)
			{
				sizeF = (obj as PdfXfaRectangleField).GetSize();
			}
			else if (obj is PdfXfaLine)
			{
				sizeF = (obj as PdfXfaLine).GetSize();
			}
			else if (obj is PdfXfaDateTimeField)
			{
				sizeF = (obj as PdfXfaDateTimeField).GetSize();
			}
			else if (obj is PdfXfaImage)
			{
				sizeF = (obj as PdfXfaImage).GetSize();
			}
			else if (obj is PdfXfaRadioButtonGroup)
			{
				sizeF = (obj as PdfXfaRadioButtonGroup).Size;
			}
			if (obj is PdfXfaRadioButtonGroup)
			{
				PdfXfaRadioButtonGroup pdfXfaRadioButtonGroup = obj as PdfXfaRadioButtonGroup;
				float num2 = 0f;
				float num3 = 0f;
				foreach (PdfXfaRadioButtonField radio in pdfXfaRadioButtonGroup.m_radioList)
				{
					SizeF size = radio.GetSize();
					if (pdfXfaRadioButtonGroup.FlowDirection == PdfXfaFlowDirection.Vertical)
					{
						pdfXfaRadioButtonGroup.Size.Height += size.Height;
						if (pdfXfaRadioButtonGroup.Size.Width < size.Width)
						{
							pdfXfaRadioButtonGroup.Size.Width = size.Width;
						}
					}
					else if (subform.Width == 0f)
					{
						num3 += size.Width;
						if (num2 < size.Height)
						{
							num2 = size.Height;
						}
					}
					else if (num3 + size.Width > subform.Width)
					{
						pdfXfaRadioButtonGroup.Size.Height += num2;
						if (pdfXfaRadioButtonGroup.Size.Width < num3)
						{
							pdfXfaRadioButtonGroup.Size.Width = num3;
						}
						if (pdfXfaRadioButtonGroup.Size.Width < num3)
						{
							pdfXfaRadioButtonGroup.Size.Width = num3;
						}
						num3 = size.Width;
					}
					else
					{
						num3 += size.Width;
						if (num2 < size.Height)
						{
							num2 = size.Height;
						}
					}
				}
				if (pdfXfaRadioButtonGroup.FlowDirection == PdfXfaFlowDirection.Horizontal)
				{
					pdfXfaRadioButtonGroup.Size.Height += num2;
					if (pdfXfaRadioButtonGroup.Size.Width < num3)
					{
						pdfXfaRadioButtonGroup.Size.Width = num3;
					}
					if (subform.Width == 0f)
					{
						subform.m_size.Width += pdfXfaRadioButtonGroup.Size.Width;
						if (subform.m_maximumSize.Height < pdfXfaRadioButtonGroup.Size.Height)
						{
							subform.m_maximumSize.Height = pdfXfaRadioButtonGroup.Size.Height;
						}
					}
					else if (subform.m_size.Width + pdfXfaRadioButtonGroup.Size.Width > subform.Width)
					{
						subform.m_size.Height += subform.m_maximumSize.Height;
						if (subform.m_maximumSize.Width < subform.m_size.Width)
						{
							subform.m_maximumSize.Width = subform.m_size.Width;
						}
						subform.m_size.Width = pdfXfaRadioButtonGroup.Size.Width;
						subform.m_maximumSize.Height = 0f;
						if (subform.m_maximumSize.Height < pdfXfaRadioButtonGroup.Size.Height)
						{
							subform.m_maximumSize.Height = pdfXfaRadioButtonGroup.Size.Height;
						}
					}
					else
					{
						subform.m_size.Width += pdfXfaRadioButtonGroup.Size.Width;
						if (subform.m_maximumSize.Height < pdfXfaRadioButtonGroup.Size.Height)
						{
							subform.m_maximumSize.Height = pdfXfaRadioButtonGroup.Size.Height;
						}
					}
				}
				else
				{
					subform.m_size.Height += pdfXfaRadioButtonGroup.Size.Height;
					if (subform.m_maximumSize.Width < pdfXfaRadioButtonGroup.Size.Width)
					{
						subform.m_maximumSize.Width = pdfXfaRadioButtonGroup.Size.Width;
					}
				}
			}
			else if (subform.FlowDirection == PdfXfaFlowDirection.Vertical)
			{
				subform.m_size.Height += sizeF.Height;
				if (subform.m_maximumSize.Width < sizeF.Width)
				{
					subform.m_maximumSize.Width = sizeF.Width;
				}
			}
			else if (subform.Width == 0f)
			{
				subform.m_size.Width += sizeF.Width;
				if (subform.m_maximumSize.Height < sizeF.Height)
				{
					subform.m_maximumSize.Height = sizeF.Height;
				}
			}
			else if (subform.m_size.Width + sizeF.Width > num)
			{
				subform.m_size.Height += subform.m_maximumSize.Height;
				if (subform.m_maximumSize.Width < subform.m_size.Width)
				{
					subform.m_maximumSize.Width = subform.m_size.Width;
				}
				subform.m_size.Width = sizeF.Width;
				subform.m_maximumSize.Height = 0f;
				if (subform.m_maximumSize.Height < sizeF.Height)
				{
					subform.m_maximumSize.Height = sizeF.Height;
				}
			}
			else
			{
				subform.m_size.Width += sizeF.Width;
				if (subform.m_maximumSize.Height < sizeF.Height)
				{
					subform.m_maximumSize.Height = sizeF.Height;
				}
			}
		}
	}

	private void Message()
	{
		PdfPage pdfPage = m_document.Pages.Add();
		PdfFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 20f, PdfFontStyle.Bold);
		PdfFont font2 = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f, PdfFontStyle.Regular);
		pdfPage.Graphics.DrawString("Please wait...", font, PdfBrushes.Black, new PointF(0f, 0f));
		pdfPage.Graphics.DrawString("If this message is not eventually replaced by the proper contents of the document, your PDF viewer may not be able to display this type of document.", font2, PdfBrushes.Black, new RectangleF(0f, 40f, 515f, 100f));
	}

	public object Clone()
	{
		PdfXfaForm obj = (PdfXfaForm)MemberwiseClone();
		obj.Fields = (PdfXfaFieldCollection)Fields.Clone();
		obj.m_acroFields = new PdfFieldCollection();
		obj.m_borderHeight = new List<float>();
		return obj;
	}
}
