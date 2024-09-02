using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedListBoxField : PdfLoadedChoiceField
{
	private PdfLoadedListFieldItemCollection m_items;

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

	public bool MultiSelect
	{
		get
		{
			return (FieldFlags.MultiSelect & Flags) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= FieldFlags.MultiSelect;
			}
			else
			{
				Flags &= ~FieldFlags.MultiSelect;
			}
			NotifyPropertyChanged("MultiSelect");
		}
	}

	public PdfLoadedListFieldItemCollection Items
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

	internal PdfLoadedListBoxField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		PdfArray kids = base.Kids;
		m_items = new PdfLoadedListFieldItemCollection();
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfDictionary dictionary2 = crossTable.GetObject(kids[i]) as PdfDictionary;
				PdfLoadedListFieldItem item = new PdfLoadedListFieldItem(this, i, dictionary2);
				m_items.Add(item);
			}
		}
	}

	internal override void Draw()
	{
		base.Draw();
		PdfGraphics graphics = Page.Graphics;
		PdfTemplate pdfTemplate = new PdfTemplate(base.Bounds.Size);
		if (base.Flatten && graphics.Page != null)
		{
			graphics.Save();
			if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle90)
			{
				Page.Graphics.TranslateTransform(Page.Graphics.Size.Width, Page.Graphics.Size.Height);
				Page.Graphics.RotateTransform(90f);
			}
			else if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle180)
			{
				Page.Graphics.TranslateTransform(Page.Graphics.Size.Width, Page.Graphics.Size.Height);
				Page.Graphics.RotateTransform(-180f);
			}
			else if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
			{
				Page.Graphics.TranslateTransform(Page.Graphics.Size.Width, Page.Graphics.Size.Height);
				Page.Graphics.RotateTransform(270f);
			}
		}
		PdfArray kids = base.Kids;
		if (kids != null && kids.Count > 1)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (!CheckFieldFlagValue(kids[i]))
				{
					PdfLoadedFieldItem pdfLoadedFieldItem = Items[i];
					pdfTemplate = new PdfTemplate(pdfLoadedFieldItem.Size);
					DrawListBox(pdfTemplate.Graphics, pdfLoadedFieldItem);
					if (pdfLoadedFieldItem.Page != null && pdfLoadedFieldItem.Page.Graphics != null)
					{
						pdfLoadedFieldItem.Page.Graphics.DrawPdfTemplate(pdfTemplate, base.Bounds.Location);
					}
				}
			}
		}
		else
		{
			DrawListBox(pdfTemplate.Graphics, null);
			Page.Graphics.DrawPdfTemplate(pdfTemplate, base.Bounds.Location);
		}
		graphics.Restore();
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
		PdfLoadedListBoxField pdfLoadedListBoxField = new PdfLoadedListBoxField(dictionary, crossTable);
		pdfLoadedListBoxField.Page = page;
		pdfLoadedListBoxField.SetName(GetFieldName());
		pdfLoadedListBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedListBoxField;
	}

	internal new PdfLoadedStyledField Clone()
	{
		PdfLoadedListBoxField pdfLoadedListBoxField = MemberwiseClone() as PdfLoadedListBoxField;
		pdfLoadedListBoxField.Dictionary = base.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedListBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedListBoxField.Items = new PdfLoadedListFieldItemCollection();
		for (int i = 0; i < Items.Count; i++)
		{
			PdfLoadedListFieldItem item = new PdfLoadedListFieldItem(pdfLoadedListBoxField, i, Items[i].Dictionary);
			pdfLoadedListBoxField.Items.Add(item);
		}
		return pdfLoadedListBoxField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		base.CreateLoadedItem(dictionary);
		PdfLoadedListFieldItem pdfLoadedListFieldItem = new PdfLoadedListFieldItem(this, m_items.Count, dictionary);
		m_items.Add(pdfLoadedListFieldItem);
		if (base.Kids == null)
		{
			base.Dictionary["Kids"] = new PdfArray();
		}
		base.Kids.Add(new PdfReferenceHolder(dictionary));
		return pdfLoadedListFieldItem;
	}

	private void ApplyAppearance(PdfDictionary widget, PdfLoadedFieldItem item)
	{
		if (widget != null && widget.ContainsKey("AP"))
		{
			if (base.CrossTable.GetObject(widget["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N"))
			{
				PdfTemplate pdfTemplate = new PdfTemplate((item?.Bounds ?? base.Bounds).Size, writeTransformation: false);
				if (!Required)
				{
					pdfTemplate.Graphics.StreamWriter.BeginMarkupSequence("Tx");
					pdfTemplate.Graphics.InitializeCoordinates();
				}
				DrawListBox(pdfTemplate.Graphics, item);
				if (!Required)
				{
					pdfTemplate.Graphics.StreamWriter.EndMarkupSequence();
				}
				pdfDictionary.Remove("N");
				pdfDictionary.SetProperty("N", new PdfReferenceHolder(pdfTemplate));
				widget.SetProperty("AP", pdfDictionary);
			}
		}
		else if (((PdfField)this).Form.SetAppearanceDictionary)
		{
			((PdfField)this).Form.NeedAppearances = true;
		}
	}

	private void DrawListBox(PdfGraphics graphics, PdfLoadedFieldItem item)
	{
		GetGraphicsProperties(out var graphicsProperties, item);
		graphicsProperties.Rect.Location = PointF.Empty;
		PaintParams paintParams = new PaintParams(graphicsProperties.Rect, graphicsProperties.BackBrush, graphicsProperties.ForeBrush, graphicsProperties.Pen, graphicsProperties.Style, graphicsProperties.BorderWidth, graphicsProperties.ShadowBrush, graphicsProperties.RotationAngle);
		paintParams.IsRequired = Required;
		if (!base.Form.SetAppearanceDictionary && !base.Form.Flatten)
		{
			paintParams.BackBrush = null;
		}
		PdfListFieldItemCollection items = ConvertToListItems(base.Values);
		FieldPainter.DrawListBox(graphics, paintParams, items, base.SelectedIndex, graphicsProperties.Font, graphicsProperties.StringFormat);
	}

	private PdfListFieldItemCollection ConvertToListItems(PdfLoadedListItemCollection items)
	{
		PdfListFieldItemCollection pdfListFieldItemCollection = new PdfListFieldItemCollection();
		foreach (PdfLoadedListItem item in items)
		{
			pdfListFieldItemCollection.Add(new PdfListFieldItem(item.Text, item.Value));
		}
		return pdfListFieldItemCollection;
	}

	internal override float GetFontHeight(PdfFontFamily family)
	{
		PdfLoadedListItemCollection values = base.Values;
		float result = 0f;
		if (values.Count > 0)
		{
			PdfFont pdfFont = new PdfStandardFont(family, 12f);
			float num = pdfFont.MeasureString(values[0].Text).Width;
			int i = 1;
			for (int count = values.Count; i < count; i++)
			{
				float width = pdfFont.MeasureString(values[i].Text).Width;
				num = ((num > width) ? num : width);
			}
			result = 12f * (base.Bounds.Size.Width - 4f * base.BorderWidth) / num;
			result = ((result > 12f) ? 12f : result);
		}
		return result;
	}

	public void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedListFieldItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedListFieldItem item)
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
