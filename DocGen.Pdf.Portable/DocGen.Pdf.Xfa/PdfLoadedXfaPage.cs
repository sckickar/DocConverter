using System;
using System.Globalization;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaPage
{
	private PdfPage m_currentPage;

	internal PdfXfaPageSettings pageSettings = new PdfXfaPageSettings();

	internal PdfLoadedXfaForm m_loadedXfaForm = new PdfLoadedXfaForm();

	internal PdfTemplate pageSetTemplate;

	internal string Name = string.Empty;

	internal string Id = string.Empty;

	internal PdfDocument document;

	internal bool isSet;

	private float m_headerTemplateHeight;

	internal PdfPage CurrentPage
	{
		get
		{
			if (m_currentPage == null)
			{
				isSet = true;
				AddPdfPage();
			}
			else
			{
				isSet = false;
			}
			return m_currentPage;
		}
	}

	internal SizeF GetClientSize()
	{
		if (pageSettings.PageOrientation == PdfXfaPageOrientation.Landscape)
		{
			return new SizeF(pageSettings.PageSize.Height - (pageSettings.Margins.Left + pageSettings.Margins.Right), pageSettings.PageSize.Width - (pageSettings.Margins.Top + pageSettings.Margins.Bottom));
		}
		return new SizeF(pageSettings.PageSize.Width - (pageSettings.Margins.Left + pageSettings.Margins.Right), pageSettings.PageSize.Height - (pageSettings.Margins.Top + pageSettings.Margins.Bottom));
	}

	internal void ReadPage(XmlNode node, PdfLoadedXfaForm lForm)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		_ = string.Empty;
		if (node.Attributes["name"] != null)
		{
			Name = node.Attributes["name"].Value;
		}
		if (node.Attributes["id"] != null)
		{
			Id = node.Attributes["id"].Value;
		}
		if (node["contentArea"] != null)
		{
			XmlNode xmlNode = node["contentArea"];
			if (xmlNode.Attributes["x"] != null)
			{
				num = ConvertToPoint(xmlNode.Attributes["x"].Value);
			}
			if (xmlNode.Attributes["y"] != null)
			{
				num2 = ConvertToPoint(xmlNode.Attributes["y"].Value);
			}
			if (xmlNode.Attributes["w"] != null)
			{
				num3 = ConvertToPoint(xmlNode.Attributes["w"].Value);
			}
			if (xmlNode.Attributes["h"] != null)
			{
				num4 = ConvertToPoint(xmlNode.Attributes["h"].Value);
			}
		}
		if (node["medium"] != null)
		{
			num5 = ConvertToPoint(node["medium"].Attributes["short"].Value);
			num6 = ConvertToPoint(node["medium"].Attributes["long"].Value);
			if (node["medium"].Attributes["orientation"] != null && node["medium"].Attributes["orientation"].Value == "landscape")
			{
				pageSettings.PageOrientation = PdfXfaPageOrientation.Landscape;
			}
		}
		if (num5 > 0f && num6 > 0f)
		{
			pageSettings.PageSize = new SizeF(num5, num6);
			pageSettings.Margins.Left = num;
			pageSettings.Margins.Top = num2;
			if (pageSettings.PageOrientation == PdfXfaPageOrientation.Landscape)
			{
				pageSettings.Margins.Bottom = num5 - (num4 + num2);
				pageSettings.Margins.Right = num6 - (num3 + num);
			}
			else
			{
				pageSettings.Margins.Bottom = num6 - (num4 + num2);
				pageSettings.Margins.Right = num5 - (num3 + num);
			}
		}
		m_loadedXfaForm.parent = lForm;
		m_loadedXfaForm.Name = lForm.Name;
		m_loadedXfaForm.nodeName = lForm.nodeName;
		m_loadedXfaForm.dataSetDoc = lForm.dataSetDoc;
		m_loadedXfaForm.currentNode = node;
		m_loadedXfaForm.ReadSubForm(node, m_loadedXfaForm, m_loadedXfaForm.m_fieldNames, m_loadedXfaForm.m_subFormNames);
		SizeF size = pageSettings.PageSize;
		if ((pageSettings.PageOrientation == PdfXfaPageOrientation.Landscape && size.Height > size.Width) || (pageSettings.PageOrientation == PdfXfaPageOrientation.Portrait && size.Width > size.Height))
		{
			size = new SizeF(size.Height, size.Width);
		}
		pageSetTemplate = new PdfTemplate(size);
	}

	internal void AddPdfPage()
	{
		PdfSection pdfSection = document.Sections.Add();
		pdfSection.PageSettings.Margins.Left = pageSettings.Margins.Left;
		pdfSection.PageSettings.Margins.Right = pageSettings.Margins.Right;
		pdfSection.PageSettings.Margins.Top = 0f;
		pdfSection.PageSettings.Margins.Bottom = 0f;
		SizeF pageSize = pageSettings.PageSize;
		if (pageSettings.PageOrientation == PdfXfaPageOrientation.Landscape)
		{
			pdfSection.PageSettings.Orientation = PdfPageOrientation.Landscape;
			if (pageSize.Height > pageSize.Width)
			{
				pdfSection.PageSettings.Size = new SizeF(pageSize.Height, pageSize.Width);
			}
			else
			{
				pdfSection.PageSettings.Size = pageSize;
			}
		}
		else if (pageSize.Width > pageSize.Height)
		{
			pdfSection.PageSettings.Size = new SizeF(pageSize.Height, pageSize.Width);
		}
		else
		{
			pdfSection.PageSettings.Size = pageSize;
		}
		m_currentPage = pdfSection.Pages.Add();
		if (m_loadedXfaForm != null)
		{
			PdfPageTemplateElement pdfPageTemplateElement = new PdfPageTemplateElement(new SizeF(m_currentPage.GetClientSize().Width, pageSettings.Margins.Top));
			pdfPageTemplateElement.Graphics.DrawPdfTemplate(pageSetTemplate, new PointF(0f - pageSettings.Margins.Left, 0f));
			pdfSection.Template.Top = pdfPageTemplateElement;
			m_headerTemplateHeight = pdfPageTemplateElement.Height;
			PdfPageTemplateElement pdfPageTemplateElement2 = new PdfPageTemplateElement(pageSetTemplate.Size.Width, pageSettings.Margins.Bottom);
			pdfPageTemplateElement2.Graphics.Save();
			pdfPageTemplateElement2.Graphics.SetClip(new RectangleF(0f, 0f, pageSetTemplate.Width, pageSetTemplate.Height));
			pageSetTemplate.Draw(pdfPageTemplateElement2.Graphics, new PointF(0f - pageSettings.Margins.Left, 0f - (pdfSection.PageSettings.Size.Height - pageSettings.Margins.Bottom)));
			pdfPageTemplateElement2.Graphics.Restore();
			pdfSection.Template.Bottom = pdfPageTemplateElement2;
		}
	}

	internal void DrawPageBackgroundTemplate(PdfPage currentPage)
	{
		currentPage.Graphics.Save();
		currentPage.Graphics.SetClip(new RectangleF(new PointF(0f, 0f), m_currentPage.GetClientSize()));
		currentPage.Graphics.DrawPdfTemplate(pageSetTemplate, new PointF(0f - pageSettings.Margins.Left, 0f - m_headerTemplateHeight));
		currentPage.Graphics.Restore();
	}

	internal float ConvertToPoint(string value)
	{
		float result = 0f;
		if (value.Contains("pt"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
		}
		else if (value.Contains("m"))
		{
			result = Convert.ToSingle(value.Trim('p', 't', 'm'), CultureInfo.InvariantCulture);
			result *= 2.8346457f;
		}
		else if (value.Contains("in"))
		{
			result = Convert.ToSingle(value.Trim('i', 'n'), CultureInfo.InvariantCulture);
			result *= 72f;
		}
		return result;
	}
}
