using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedButtonField : PdfLoadedStyledField
{
	private PdfLoadedButtonItemCollection m_items;

	public new bool ComplexScript
	{
		get
		{
			return base.ComplexScript;
		}
		set
		{
			base.ComplexScript = value;
			NotifyPropertyChanged("ComplexScript");
		}
	}

	public string Text
	{
		get
		{
			return ObtainText();
		}
		set
		{
			if ((FieldFlags.ReadOnly & Flags) == 0)
			{
				((PdfField)this).Form.SetAppearanceDictionary = true;
				AssignText(value);
				NotifyPropertyChanged("Text");
			}
		}
	}

	public PdfLoadedButtonItemCollection Items
	{
		get
		{
			return m_items;
		}
		internal set
		{
			m_items = value;
		}
	}

	internal PdfLoadedButtonField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		PdfArray kids = base.Kids;
		m_items = new PdfLoadedButtonItemCollection();
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfDictionary dictionary2 = crossTable.GetObject(kids[i]) as PdfDictionary;
				PdfLoadedButtonItem item = new PdfLoadedButtonItem(this, i, dictionary2);
				m_items.Add(item);
			}
		}
	}

	private string ObtainText()
	{
		PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		string text = null;
		if (pdfDictionary.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary2 = base.CrossTable.GetObject(pdfDictionary["MK"]) as PdfDictionary;
			if (pdfDictionary2.ContainsKey("CA"))
			{
				text = (base.CrossTable.GetObject(pdfDictionary2["CA"]) as PdfString).Value;
			}
		}
		if (text == null)
		{
			PdfString pdfString = base.CrossTable.GetObject(base.Dictionary["V"]) as PdfString;
			if (pdfString == null)
			{
				pdfString = PdfLoadedField.GetValue(base.Dictionary, base.CrossTable, "V", inheritable: true) as PdfString;
			}
			text = ((pdfString == null) ? "" : pdfString.Value);
		}
		return text;
	}

	private void AssignText(string value)
	{
		PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		if (pdfDictionary.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary2 = base.CrossTable.GetObject(pdfDictionary["MK"]) as PdfDictionary;
			pdfDictionary2.SetString("CA", value);
			pdfDictionary.SetProperty("MK", new PdfReferenceHolder(pdfDictionary2));
		}
		else
		{
			PdfDictionary pdfDictionary3 = new PdfDictionary();
			pdfDictionary3.SetString("CA", value);
			pdfDictionary.SetProperty("MK", new PdfReferenceHolder(pdfDictionary3));
		}
		base.Changed = true;
	}

	internal override void Draw()
	{
		base.Draw();
		PdfArray kids = base.Kids;
		if (kids != null && kids.Count > 0)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (!CheckFieldFlagValue(kids[i]))
				{
					PdfLoadedFieldItem pdfLoadedFieldItem = Items[i];
					if (pdfLoadedFieldItem.Page != null)
					{
						DrawButton(pdfLoadedFieldItem.Page.Graphics, pdfLoadedFieldItem);
					}
				}
			}
		}
		else
		{
			DrawButton(Page.Graphics, null);
		}
	}

	internal override void BeginSave()
	{
		base.BeginSave();
		PdfArray kids = base.Kids;
		if (kids != null && kids.Count == Items.Count)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfDictionary widget = base.CrossTable.GetObject(kids[i]) as PdfDictionary;
				ApplyAppearance(widget, Items[i]);
			}
		}
		else
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			ApplyAppearance(widgetAnnotation, null);
		}
	}

	internal new PdfField Clone(PdfDictionary dictionary, PdfPage page)
	{
		PdfCrossTable crossTable = page.Section.ParentDocument.CrossTable;
		PdfLoadedButtonField pdfLoadedButtonField = new PdfLoadedButtonField(dictionary, crossTable);
		pdfLoadedButtonField.Page = page;
		pdfLoadedButtonField.SetName(GetFieldName());
		pdfLoadedButtonField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedButtonField;
	}

	internal new PdfLoadedStyledField Clone()
	{
		PdfLoadedButtonField pdfLoadedButtonField = MemberwiseClone() as PdfLoadedButtonField;
		pdfLoadedButtonField.Dictionary = base.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedButtonField.Widget.Dictionary = base.Widget.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedButtonField.Items = new PdfLoadedButtonItemCollection();
		for (int i = 0; i < Items.Count; i++)
		{
			PdfLoadedButtonItem item = new PdfLoadedButtonItem(pdfLoadedButtonField, i, Items[i].Dictionary);
			pdfLoadedButtonField.Items.Add(item);
		}
		return pdfLoadedButtonField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		base.CreateLoadedItem(dictionary);
		PdfLoadedButtonItem pdfLoadedButtonItem = new PdfLoadedButtonItem(this, m_items.Count, dictionary);
		m_items.Add(pdfLoadedButtonItem);
		if (base.Kids == null)
		{
			base.Dictionary["Kids"] = new PdfArray();
		}
		base.Kids.Add(new PdfReferenceHolder(dictionary));
		return pdfLoadedButtonItem;
	}

	private void ApplyAppearance(PdfDictionary widget, PdfLoadedFieldItem item)
	{
		if (widget != null && widget.ContainsKey("AP"))
		{
			if (!(base.CrossTable.GetObject(widget["AP"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("N"))
			{
				return;
			}
			RectangleF rectangleF = item?.Bounds ?? base.Bounds;
			PdfTemplate pdfTemplate = new PdfTemplate(rectangleF.Size);
			PdfTemplate pdfTemplate2 = new PdfTemplate(rectangleF.Size);
			if (widget.ContainsKey("MK") && widget["MK"] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("R") && pdfDictionary2["R"] is PdfNumber pdfNumber)
			{
				if (pdfNumber.FloatValue == 90f)
				{
					pdfTemplate = new PdfTemplate(new SizeF(rectangleF.Size.Height, rectangleF.Size.Width), writeTransformation: false);
					pdfTemplate.m_content["Matrix"] = new PdfArray(new float[6]
					{
						0f,
						1f,
						-1f,
						0f,
						rectangleF.Size.Width,
						0f
					});
				}
				else if (pdfNumber.FloatValue == 180f)
				{
					pdfTemplate = new PdfTemplate(rectangleF.Size, writeTransformation: false);
					pdfTemplate.m_content["Matrix"] = new PdfArray(new float[6]
					{
						-1f,
						0f,
						0f,
						-1f,
						rectangleF.Size.Width,
						rectangleF.Size.Height
					});
				}
				else if (pdfNumber.FloatValue == 270f)
				{
					pdfTemplate = new PdfTemplate(new SizeF(rectangleF.Size.Height, rectangleF.Size.Width), writeTransformation: false);
					pdfTemplate.m_content["Matrix"] = new PdfArray(new float[6]
					{
						0f,
						-1f,
						1f,
						0f,
						0f,
						rectangleF.Size.Height
					});
				}
			}
			if (pdfTemplate == null)
			{
				pdfTemplate = new PdfTemplate(rectangleF.Size, writeTransformation: false);
				pdfTemplate.m_content["Matrix"] = new PdfArray(new float[6] { 1f, 0f, 0f, 1f, 0f, 0f });
			}
			DrawButton(pdfTemplate.Graphics, item);
			DrawButton(pdfTemplate2.Graphics, item);
			pdfDictionary.SetProperty("N", new PdfReferenceHolder(pdfTemplate));
			pdfDictionary.SetProperty("D", new PdfReferenceHolder(pdfTemplate2));
			widget.SetProperty("AP", pdfDictionary);
		}
		else if (((PdfField)this).Form.SetAppearanceDictionary)
		{
			((PdfField)this).Form.NeedAppearances = true;
		}
	}

	private void DrawButton(PdfGraphics graphics, PdfLoadedFieldItem item)
	{
		GetGraphicsProperties(out var graphicsProperties, item);
		if (!base.Flatten)
		{
			graphicsProperties.Rect.Location = new PointF(0f, 0f);
		}
		PaintParams paintParams = new PaintParams(graphicsProperties.Rect, graphicsProperties.BackBrush, graphicsProperties.ForeBrush, graphicsProperties.Pen, graphicsProperties.Style, graphicsProperties.BorderWidth, graphicsProperties.ShadowBrush, graphicsProperties.RotationAngle);
		if (base.Flatten || (base.Changed && ComplexScript) || (graphics != null && graphics.IsTemplateGraphics))
		{
			graphicsProperties.StringFormat.Alignment = PdfTextAlignment.Center;
		}
		if (base.Dictionary.ContainsKey("AP") && !ComplexScript && (graphics.Layer == null || graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle0) && paintParams.RotationAngle <= 0)
		{
			if (!(PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary) || !(PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2) || !(pdfDictionary2 is PdfStream template))
			{
				return;
			}
			PdfTemplate pdfTemplate = new PdfTemplate(template);
			if (pdfTemplate == null)
			{
				return;
			}
			if (graphics != null && graphics.IsTemplateGraphics)
			{
				if (base.Visibility != PdfFormFieldVisibility.Hidden)
				{
					graphics.Save();
					graphics.DrawPdfTemplate(pdfTemplate, new PointF(0f, 0f));
					graphics.Restore();
				}
			}
			else
			{
				Page.Graphics.DrawPdfTemplate(pdfTemplate, base.Bounds.Location);
			}
		}
		else if (base.Dictionary.ContainsKey("Kids") && item != null && !ComplexScript && (graphics.Layer == null || graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle0) && paintParams.RotationAngle <= 0)
		{
			if (PdfCrossTable.Dereference(item.Dictionary["AP"]) is PdfDictionary pdfDictionary3)
			{
				if (!(PdfCrossTable.Dereference(pdfDictionary3["N"]) is PdfDictionary pdfDictionary4) || !(pdfDictionary4 is PdfStream template2))
				{
					return;
				}
				PdfTemplate pdfTemplate2 = new PdfTemplate(template2);
				if (pdfTemplate2 == null)
				{
					return;
				}
				if (graphics != null && graphics.IsTemplateGraphics)
				{
					if (base.Visibility != PdfFormFieldVisibility.Hidden)
					{
						graphics.Save();
						graphics.DrawPdfTemplate(pdfTemplate2, new PointF(0f, 0f));
						graphics.Restore();
					}
				}
				else if (item.Page != null)
				{
					PointF location = item.Bounds.Location;
					item.Page.Graphics.DrawPdfTemplate(pdfTemplate2, location);
				}
				else
				{
					Page.Graphics.DrawPdfTemplate(pdfTemplate2, base.Bounds.Location);
				}
			}
			else
			{
				FieldPainter.DrawButton(graphics, paintParams, Text, item.Font, graphicsProperties.StringFormat);
			}
		}
		else
		{
			FieldPainter.DrawButton(graphics, paintParams, Text, graphicsProperties.Font, graphicsProperties.StringFormat);
		}
	}

	internal override float GetFontHeight(PdfFontFamily family)
	{
		float width = new PdfStandardFont(family, 12f).MeasureString(Text).Width;
		float num = 12f * (base.Bounds.Size.Width - 4f * base.BorderWidth) / width;
		return (num > 12f) ? 12f : num;
	}

	public void AddPrintAction()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		pdfDictionary.SetProperty("N", new PdfName("Print"));
		pdfDictionary.SetProperty("S", new PdfName("Named"));
		if (base.Dictionary["Kids"] is PdfArray pdfArray)
		{
			((pdfArray[0] as PdfReferenceHolder).Object as PdfDictionary).SetProperty("A", pdfDictionary);
		}
		else
		{
			base.Dictionary.SetProperty("A", pdfDictionary);
		}
	}

	public void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedButtonItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedButtonItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		int index = Items.Remove(item);
		if (base.Dictionary["Kids"] is PdfArray pdfArray)
		{
			pdfArray.RemoveAt(index);
			pdfArray.MarkChanged();
		}
	}
}
