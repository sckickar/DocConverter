using System;
using System.IO;
using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaImage : PdfLoadedXfaStyledField
{
	internal string m_base64ImageData = string.Empty;

	internal XfaImageAspectRadio aspectRadio;

	internal string m_imageReference = string.Empty;

	internal void ReadField(XmlNode node, XmlDocument dataSetDoc)
	{
		currentNode = node;
		ReadCommonProperties(node);
		if (node["value"] != null && node["value"]["image"] != null)
		{
			XmlNode xmlNode = node["value"]["image"];
			if (xmlNode.Attributes["aspect"] != null)
			{
				string text = xmlNode.Attributes["aspect"].Value.ToLower();
				if (!(text == "actual"))
				{
					if (text == "none")
					{
						aspectRadio = XfaImageAspectRadio.None;
					}
					else
					{
						aspectRadio = XfaImageAspectRadio.Proportionally;
					}
				}
				else
				{
					aspectRadio = XfaImageAspectRadio.Actual;
				}
			}
			if (xmlNode.Attributes["href"] != null)
			{
				m_imageReference = xmlNode.Attributes["href"].Value;
			}
			string innerText = xmlNode.InnerText;
			try
			{
				Convert.FromBase64String(innerText);
				m_base64ImageData = innerText;
			}
			catch (Exception)
			{
			}
		}
		char[] separator = new char[1] { '.' };
		string text2 = string.Empty;
		if (node["bind"] != null)
		{
			XmlNode xmlNode2 = node["bind"];
			if (xmlNode2.Attributes["ref"] != null)
			{
				text2 = xmlNode2.Attributes["ref"].Value.Replace("$record.", "");
			}
		}
		if (!(text2 != string.Empty))
		{
			return;
		}
		if (text2.StartsWith("$"))
		{
			text2 = text2.Remove(0, 1);
		}
		text2 = text2.Replace(".", "/");
		string text3 = parent.nodeName.Split(separator)[0];
		text3 = text3.Replace("[0]", "");
		text3 = text3 + "/" + text2;
		text3 = "//" + text3;
		if (dataSetDoc != null)
		{
			XmlNode xmlNode3 = dataSetDoc.SelectSingleNode(text3);
			if (xmlNode3 != null && xmlNode3.FirstChild != null)
			{
				m_base64ImageData = xmlNode3.FirstChild.InnerText;
			}
		}
	}

	internal void DrawImage(PdfGraphics graphics, RectangleF bounds, PdfLoadedDocument ldoc)
	{
		PdfBitmap pdfBitmap = null;
		PdfDictionary pdfDictionary = null;
		if (m_imageReference != string.Empty)
		{
			PdfReferenceHolder pdfReferenceHolder = null;
			if (ldoc.Catalog != null)
			{
				pdfReferenceHolder = ldoc.Catalog["Names"] as PdfReferenceHolder;
			}
			if (pdfReferenceHolder != null)
			{
				if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary2 && pdfDictionary2.Items.ContainsKey(new PdfName("XFAImages")))
				{
					pdfReferenceHolder = pdfDictionary2.Items[new PdfName("XFAImages")] as PdfReferenceHolder;
				}
				if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary3 && pdfDictionary3.Items.ContainsKey(new PdfName("Names")))
				{
					if (pdfDictionary3.Items[new PdfName("Names")] is PdfArray pdfArray)
					{
						for (int i = 0; i < pdfArray.Count; i += 2)
						{
							if ((pdfArray.Elements[i] as PdfString).Value == m_imageReference)
							{
								pdfDictionary = (pdfArray.Elements[i + 1] as PdfReferenceHolder).Object as PdfDictionary;
								break;
							}
						}
					}
					else
					{
						try
						{
							(pdfDictionary as PdfStream).Decompress();
							Image.FromStream(new MemoryStream((pdfDictionary as PdfStream).Data));
							pdfBitmap = new PdfBitmap(new MemoryStream((pdfDictionary as PdfStream).Data));
						}
						catch
						{
						}
					}
				}
			}
		}
		if (m_base64ImageData != string.Empty)
		{
			try
			{
				pdfBitmap = new PdfBitmap(new MemoryStream(Convert.FromBase64String(m_base64ImageData)));
			}
			catch (Exception)
			{
			}
		}
		if (pdfBitmap != null)
		{
			SizeF size = GetSize();
			RectangleF tempBounds = default(RectangleF);
			tempBounds.Location = new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top);
			tempBounds.Size = new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom));
			graphics.Save();
			graphics.TranslateTransform(tempBounds.X, tempBounds.Y);
			graphics.RotateTransform(-GetRotationAngle());
			PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor();
			pdfUnitConvertor.ConvertFromPixels(pdfBitmap.Width, PdfGraphicsUnit.Point);
			pdfUnitConvertor.ConvertFromPixels(pdfBitmap.Height, PdfGraphicsUnit.Point);
			if (aspectRadio == XfaImageAspectRadio.Proportionally)
			{
				float val = tempBounds.Width / (float)pdfBitmap.Width;
				float val2 = tempBounds.Height / (float)pdfBitmap.Height;
				float num = Math.Min(val, val2);
				tempBounds.Width = (float)pdfBitmap.Width * num;
				tempBounds.Height = (float)pdfBitmap.Height * num;
			}
			RectangleF renderingRect = GetRenderingRect(tempBounds);
			graphics.DrawImage(pdfBitmap, renderingRect);
			graphics.Restore();
		}
	}
}
