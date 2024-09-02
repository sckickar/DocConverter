using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xfa;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedTextBoxField : PdfLoadedStyledField
{
	private const string m_passwordValue = "*";

	private PdfLoadedTextBoxItemCollection m_items;

	private PdfColor m_foreColor = new PdfColor(0, 0, 0);

	private bool m_autoSize;

	private bool m_applyAppearence = true;

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

	public PdfColor BackColor
	{
		get
		{
			return GetBackColor();
		}
		set
		{
			AssignBackColor(value);
			m_isFieldPropertyChanged = true;
			NotifyPropertyChanged("BackColor");
		}
	}

	public new virtual PdfColor ForeColor
	{
		get
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				string empty = string.Empty;
				empty = ((!(base.CrossTable.GetObject(widgetAnnotation["DA"]) is PdfString pdfString)) ? base.CrossTable.GetObject(widgetAnnotation["DA"]).ToString() : pdfString.Value);
				if (empty != null && empty != string.Empty)
				{
					m_foreColor = GetForeColour(empty);
				}
			}
			else if (widgetAnnotation != null && widgetAnnotation.GetValue(base.CrossTable, "DA", "Parent") is PdfString pdfString2)
			{
				m_foreColor = GetForeColour(pdfString2.Value);
			}
			return m_foreColor;
		}
		set
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			float height = 0f;
			string text = null;
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(base.Form.Resources["Font"]) as PdfDictionary;
				PdfString pdfString = widgetAnnotation["DA"] as PdfString;
				text = FontName(pdfString.Value, out height);
				if (!string.IsNullOrEmpty(text))
				{
					PdfReferenceHolder pdfReferenceHolder = pdfDictionary[text] as PdfReferenceHolder;
					if (pdfReferenceHolder != null)
					{
						_ = pdfReferenceHolder.Object;
					}
				}
			}
			else if (widgetAnnotation != null && base.Dictionary.ContainsKey("DA"))
			{
				PdfDictionary obj = base.CrossTable.GetObject(base.Form.Resources["Font"]) as PdfDictionary;
				PdfString pdfString2 = base.Dictionary["DA"] as PdfString;
				text = FontName(pdfString2.Value, out height);
				_ = (obj[text] as PdfReferenceHolder).Object;
			}
			if (text != null)
			{
				PdfDefaultAppearance pdfDefaultAppearance = new PdfDefaultAppearance();
				pdfDefaultAppearance.FontName = text;
				pdfDefaultAppearance.FontSize = height;
				pdfDefaultAppearance.ForeColor = value;
				PdfArray kids = base.Kids;
				if (kids != null)
				{
					for (int i = 0; i < kids.Count; i++)
					{
						(base.CrossTable.GetObject(kids[i]) as PdfDictionary)["DA"] = new PdfString(pdfDefaultAppearance.ToString());
					}
				}
				else
				{
					widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance.ToString());
				}
			}
			else
			{
				PdfDefaultAppearance pdfDefaultAppearance2 = new PdfDefaultAppearance();
				pdfDefaultAppearance2.FontName = base.Font.Name;
				pdfDefaultAppearance2.FontSize = base.Font.Size;
				pdfDefaultAppearance2.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance2.ToString());
			}
			((PdfField)this).Form.SetAppearanceDictionary = true;
			NotifyPropertyChanged("ForeColor");
		}
	}

	public PdfTextAlignment TextAlignment
	{
		get
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfTextAlignment result = PdfTextAlignment.Left;
			if (widgetAnnotation.ContainsKey("Q"))
			{
				PdfNumber pdfNumber = widgetAnnotation["Q"] as PdfNumber;
				result = (PdfTextAlignment)Enum.ToObject(typeof(PdfTextAlignment), pdfNumber.IntValue);
			}
			else if (widgetAnnotation != null && widgetAnnotation.GetValue(base.CrossTable, "Q", "Parent") is PdfNumber pdfNumber2)
			{
				result = (PdfTextAlignment)Enum.ToObject(typeof(PdfTextAlignment), pdfNumber2.IntValue);
			}
			return result;
		}
		set
		{
			if (base.Dictionary.ContainsKey("Kids"))
			{
				PdfArray pdfArray = base.CrossTable.GetObject(base.Dictionary["Kids"]) as PdfArray;
				for (int i = 0; i < pdfArray.Count; i++)
				{
					(PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary).SetProperty("Q", new PdfNumber((int)value));
				}
			}
			else
			{
				GetWidgetAnnotation(base.Dictionary, base.CrossTable).SetProperty("Q", new PdfNumber((int)value));
			}
			((PdfField)this).Form.SetAppearanceDictionary = true;
			NotifyPropertyChanged("TextAlignment");
		}
	}

	public PdfHighlightMode HighlightMode
	{
		get
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfHighlightMode result = PdfHighlightMode.NoHighlighting;
			if (widgetAnnotation.ContainsKey("H"))
			{
				PdfName hightlightMode = base.CrossTable.GetObject(widgetAnnotation["H"]) as PdfName;
				result = GetHighlightModeFromString(hightlightMode);
			}
			return result;
		}
		set
		{
			GetWidgetAnnotation(base.Dictionary, base.CrossTable)["H"] = new PdfName(HighlightModeToString(value));
			NotifyPropertyChanged("HighlightMode");
		}
	}

	public string Text
	{
		get
		{
			string empty = string.Empty;
			PdfReferenceHolder pdfReferenceHolder = base.Dictionary["V"] as PdfReferenceHolder;
			PdfString pdfString;
			if (pdfReferenceHolder != null)
			{
				IPdfPrimitive pdfPrimitive = PdfCrossTable.Dereference(pdfReferenceHolder);
				if (!(pdfPrimitive is PdfStream))
				{
					pdfString = ((!(pdfPrimitive is PdfString)) ? new PdfString(string.Empty) : (PdfLoadedField.GetValue(base.Dictionary, base.CrossTable, "V", inheritable: true) as PdfString));
				}
				else
				{
					byte[] decompressedData = (pdfReferenceHolder.Object as PdfStream).GetDecompressedData();
					pdfString = new PdfString(Encoding.UTF8.GetString(decompressedData));
				}
			}
			else
			{
				pdfString = PdfLoadedField.GetValue(base.Dictionary, base.CrossTable, "V", inheritable: true) as PdfString;
			}
			if (pdfString == null && Items != null && Items.Count > 0)
			{
				pdfString = PdfLoadedField.GetValue(Items[0].Dictionary, base.CrossTable, "V", inheritable: true) as PdfString;
			}
			if (pdfString != null)
			{
				empty = pdfString.Value;
				if (Regex.IsMatch(empty, "[\\u0080-\\uFFFF]"))
				{
					empty = new PdfEncoding().ConvertUnicodeToString(empty);
				}
			}
			else
			{
				empty = string.Empty;
			}
			return empty;
		}
		set
		{
			if ((FieldFlags.ReadOnly & Flags) == 0 || m_isImportFields)
			{
				if (value == null)
				{
					throw new ArgumentNullException("text");
				}
				m_isFieldPropertyChanged = true;
				m_isTextChanged = true;
				if (!string.IsNullOrEmpty(Text))
				{
					m_isTextModified = true;
				}
				string text = value;
				string finalText = string.Empty;
				bool replace = false;
				if (!base.DisableAutoFormat)
				{
					MapNumberFormat(value, updateDictionary: true, out finalText, out replace);
				}
				text = (replace ? finalText : text);
				if (replace)
				{
					base.Dictionary.SetProperty("V", new PdfString(text));
				}
				else
				{
					base.Dictionary.SetProperty("V", new PdfString(value));
				}
				if (Items != null && Items.Count > 0)
				{
					PdfLoadedTexBoxItem pdfLoadedTexBoxItem = Items[0];
					if (pdfLoadedTexBoxItem != null && pdfLoadedTexBoxItem.Dictionary.ContainsKey("V"))
					{
						if (replace)
						{
							pdfLoadedTexBoxItem.Dictionary.SetProperty("V", new PdfString(text));
						}
						else
						{
							pdfLoadedTexBoxItem.Dictionary.SetProperty("V", new PdfString(value));
						}
					}
				}
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				base.CrossTable.GetObject(widgetAnnotation["MK"]);
				base.Changed = true;
				if (base.Form is PdfLoadedForm { isUR3: not false })
				{
					base.Dictionary.BeginSave += Dictionary_BeginSave;
				}
			}
			else
			{
				base.Changed = false;
			}
			if (base.Form is PdfLoadedForm { EnableXfaFormFill: not false, IsXFAForm: not false } pdfLoadedForm2)
			{
				string fieldName = Name.Replace("\\", string.Empty);
				PdfLoadedXfaField xfaField = pdfLoadedForm2.GetXfaField(fieldName);
				if (xfaField != null)
				{
					UpdateXfaFieldData(xfaField);
				}
			}
			NotifyPropertyChanged("Text");
		}
	}

	public string DefaultValue
	{
		get
		{
			string result = null;
			if (PdfLoadedField.GetValue(base.Dictionary, base.CrossTable, "DV", inheritable: true) is PdfString pdfString)
			{
				result = pdfString.Value;
			}
			return result;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("DefaultValue");
			}
			base.Dictionary.SetString("DV", value);
			NotifyPropertyChanged("DefaultValue");
			base.Changed = true;
		}
	}

	public bool SpellCheck
	{
		get
		{
			return (FieldFlags.DoNotSpellCheck & Flags) == 0;
		}
		set
		{
			if (value)
			{
				Flags &= ~FieldFlags.DoNotSpellCheck;
			}
			else
			{
				Flags |= FieldFlags.DoNotSpellCheck;
			}
			NotifyPropertyChanged("SpellCheck");
		}
	}

	public bool InsertSpaces
	{
		get
		{
			if ((FieldFlags.Comb & Flags) != 0)
			{
				if ((Flags & FieldFlags.Multiline) == 0 && (Flags & FieldFlags.Password) == 0)
				{
					return (Flags & FieldFlags.FileSelect) == 0;
				}
				return false;
			}
			return false;
		}
		set
		{
			if (value)
			{
				Flags |= FieldFlags.Comb;
			}
			else
			{
				Flags &= ~FieldFlags.Comb;
			}
			NotifyPropertyChanged("InsertSpaces");
		}
	}

	public bool Multiline
	{
		get
		{
			return (FieldFlags.Multiline & Flags) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= FieldFlags.Multiline;
			}
			else
			{
				Flags &= ~FieldFlags.Multiline;
			}
			NotifyPropertyChanged("Multiline");
		}
	}

	public bool Password
	{
		get
		{
			return (FieldFlags.Password & Flags) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= FieldFlags.Password;
			}
			else
			{
				Flags &= ~FieldFlags.Password;
			}
			NotifyPropertyChanged("Password");
		}
	}

	public bool Scrollable
	{
		get
		{
			return (FieldFlags.DoNotScroll & Flags) == 0;
		}
		set
		{
			if (value)
			{
				Flags &= ~FieldFlags.DoNotScroll;
			}
			else
			{
				Flags |= FieldFlags.DoNotScroll;
			}
			NotifyPropertyChanged("Scrollable");
		}
	}

	public int MaxLength
	{
		get
		{
			int result = 0;
			if (PdfLoadedField.GetValue(base.Dictionary, base.CrossTable, "MaxLen", inheritable: true) is PdfNumber pdfNumber)
			{
				result = pdfNumber.IntValue;
			}
			return result;
		}
		set
		{
			base.Dictionary.SetNumber("MaxLen", value);
			NotifyPropertyChanged("MaxLength");
			base.Changed = true;
		}
	}

	public bool IsAutoFontSize
	{
		get
		{
			bool result = false;
			if (base.CrossTable.Document is PdfLoadedDocument && (base.CrossTable.Document as PdfLoadedDocument).Form.Dictionary.ContainsKey("DA"))
			{
				PdfString pdfString = (base.CrossTable.Document as PdfLoadedDocument).Form.Dictionary.Items[new PdfName("DA")] as PdfString;
				float height = 0f;
				FontName(pdfString.Value, out height);
				if (height == 0f)
				{
					if (base.Dictionary.ContainsKey("Kids"))
					{
						bool flag = false;
						PdfArray pdfArray = base.Dictionary["Kids"] as PdfArray;
						PdfDictionary pdfDictionary = null;
						if (base.Dictionary.ContainsKey("DA"))
						{
							PdfString pdfString2 = base.Dictionary.Items[new PdfName("DA")] as PdfString;
							height = 0f;
							FontName(pdfString2.Value, out height);
							if (height == 0f)
							{
								flag = true;
							}
						}
						if (flag || !base.Dictionary.ContainsKey("DA"))
						{
							foreach (PdfReferenceHolder element in pdfArray.Elements)
							{
								pdfDictionary = element.Object as PdfDictionary;
								if (!pdfDictionary.ContainsKey("DA"))
								{
									result = true;
									continue;
								}
								PdfString pdfString3 = pdfDictionary.Items[new PdfName("DA")] as PdfString;
								height = 0f;
								FontName(pdfString3.Value, out height);
								if (height == 0f)
								{
									result = true;
								}
							}
						}
					}
					else if (!base.Dictionary.ContainsKey("DA"))
					{
						result = true;
					}
					else
					{
						PdfString pdfString4 = base.Dictionary.Items[new PdfName("DA")] as PdfString;
						height = 0f;
						FontName(pdfString4.Value, out height);
						if (height == 0f)
						{
							result = true;
						}
					}
				}
			}
			return result;
		}
	}

	public bool AutoResizeText
	{
		get
		{
			return m_autoSize;
		}
		set
		{
			m_autoSize = value;
			base.Changed = true;
			NotifyPropertyChanged("AutoResizeText");
		}
	}

	public PdfLoadedTextBoxItemCollection Items
	{
		get
		{
			return m_items;
		}
		internal set
		{
			m_items = value;
			NotifyPropertyChanged("Items");
		}
	}

	internal PdfLoadedTextBoxField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		PdfArray kids = base.Kids;
		m_items = new PdfLoadedTextBoxItemCollection();
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				PdfDictionary dictionary2 = crossTable.GetObject(kids[i]) as PdfDictionary;
				PdfLoadedTexBoxItem item = new PdfLoadedTexBoxItem(this, i, dictionary2);
				m_items.Add(item);
			}
		}
	}

	private void UpdateXfaFieldData(PdfLoadedXfaField field)
	{
		if (field is PdfLoadedXfaTextBoxField)
		{
			(field as PdfLoadedXfaTextBoxField).Text = Text;
		}
		else if (field is PdfLoadedXfaNumericField)
		{
			PdfLoadedXfaNumericField pdfLoadedXfaNumericField = field as PdfLoadedXfaNumericField;
			if (double.TryParse(Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				pdfLoadedXfaNumericField.NumericValue = result;
			}
			else
			{
				pdfLoadedXfaNumericField.NumericValue = double.NaN;
			}
		}
		else if (field is PdfLoadedXfaDateTimeField)
		{
			PdfLoadedXfaDateTimeField pdfLoadedXfaDateTimeField = field as PdfLoadedXfaDateTimeField;
			if (DateTime.TryParse(Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
			{
				pdfLoadedXfaDateTimeField.SetDate(result2);
			}
			else
			{
				pdfLoadedXfaDateTimeField.SetDate(null);
			}
		}
	}

	internal virtual void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		BeginSave();
	}

	private string HighlightModeToString(PdfHighlightMode m_highlightingMode)
	{
		return m_highlightingMode switch
		{
			PdfHighlightMode.NoHighlighting => "N", 
			PdfHighlightMode.Outline => "O", 
			PdfHighlightMode.Push => "P", 
			_ => "I", 
		};
	}

	private PdfHighlightMode GetHighlightModeFromString(PdfName hightlightMode)
	{
		return hightlightMode.Value switch
		{
			"P" => PdfHighlightMode.Push, 
			"N" => PdfHighlightMode.NoHighlighting, 
			"O" => PdfHighlightMode.Outline, 
			_ => PdfHighlightMode.Invert, 
		};
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
		PdfLoadedTextBoxField pdfLoadedTextBoxField = new PdfLoadedTextBoxField(dictionary, crossTable);
		pdfLoadedTextBoxField.Page = page;
		pdfLoadedTextBoxField.SetName(GetFieldName());
		pdfLoadedTextBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedTextBoxField;
	}

	internal new PdfLoadedStyledField Clone()
	{
		PdfLoadedTextBoxField pdfLoadedTextBoxField = MemberwiseClone() as PdfLoadedTextBoxField;
		base.Dictionary.m_clonedObject = null;
		pdfLoadedTextBoxField.Dictionary = base.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedTextBoxField.Widget.Dictionary = base.Widget.Dictionary.Clone(base.CrossTable) as PdfDictionary;
		pdfLoadedTextBoxField.Items = new PdfLoadedTextBoxItemCollection();
		for (int i = 0; i < Items.Count; i++)
		{
			PdfLoadedTexBoxItem item = new PdfLoadedTexBoxItem(pdfLoadedTextBoxField, i, Items[i].Dictionary);
			pdfLoadedTextBoxField.Items.Add(item);
		}
		return pdfLoadedTextBoxField;
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		base.CreateLoadedItem(dictionary);
		PdfLoadedTexBoxItem pdfLoadedTexBoxItem = new PdfLoadedTexBoxItem(this, m_items.Count, dictionary);
		m_items.Add(pdfLoadedTexBoxItem);
		if (base.Kids == null)
		{
			base.Dictionary["Kids"] = new PdfArray();
		}
		base.Kids.Add(new PdfReferenceHolder(dictionary));
		return pdfLoadedTexBoxItem;
	}

	private void ApplyAppearance(PdfDictionary widget, PdfLoadedFieldItem item)
	{
		_ = ((PdfField)this).Form.NeedAppearances;
		if (!((PdfField)this).Form.SetAppearanceDictionary && !m_isTextChanged)
		{
			return;
		}
		if (widget != null)
		{
			if (AutoResizeText && widget.ContainsKey("DA"))
			{
				widget.Remove("DA");
			}
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widget["AP"]) as PdfDictionary;
			pdfDictionary = new PdfDictionary();
			RectangleF rectangleF = item?.Bounds ?? base.Bounds;
			PdfTemplate pdfTemplate = null;
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
			if (!Required)
			{
				pdfTemplate.Graphics.StreamWriter.BeginMarkupSequence("Tx");
				pdfTemplate.Graphics.InitializeCoordinates();
			}
			DrawTextBox(pdfTemplate.Graphics, item);
			if (!Required)
			{
				pdfTemplate.Graphics.StreamWriter.EndMarkupSequence();
			}
			pdfDictionary.SetProperty("N", new PdfReferenceHolder(pdfTemplate));
			widget.SetProperty("AP", pdfDictionary);
		}
		else
		{
			((PdfField)this).Form.NeedAppearances = true;
		}
	}

	internal override void Draw()
	{
		base.Draw();
		PdfArray kids = base.Kids;
		if (kids != null)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (CheckFieldFlagValue(kids[i]))
				{
					continue;
				}
				PdfLoadedFieldItem pdfLoadedFieldItem = Items[i];
				if (pdfLoadedFieldItem.Page != null && pdfLoadedFieldItem.Page is PdfLoadedPage)
				{
					DrawTextBox(pdfLoadedFieldItem.Page.Graphics, pdfLoadedFieldItem);
					continue;
				}
				PdfDictionary pdfDictionary = PdfCrossTable.Dereference(kids[i]) as PdfDictionary;
				if (pdfDictionary != null && pdfDictionary.ContainsKey("P") && pdfDictionary["P"] as PdfReferenceHolder != null && (pdfDictionary["P"] as PdfReferenceHolder).Reference == null)
				{
					PdfPageBase pdfPageBase = null;
					PdfPage pdfPage = null;
					bool flag = false;
					if (pdfLoadedFieldItem != null && pdfLoadedFieldItem.Page != null)
					{
						pdfPage = pdfLoadedFieldItem.Page as PdfPage;
					}
					if (pdfPage != null && pdfPage.Imported)
					{
						pdfPageBase = pdfPage;
					}
					else
					{
						if (Page != null && Page is PdfPage { Section: not null } pdfPage2 && pdfPage2.Section.ParentDocument != null)
						{
							flag = pdfPage2.Section.ParentDocument.EnableMemoryOptimization;
						}
						pdfPageBase = ((!flag || pdfLoadedFieldItem == null || pdfLoadedFieldItem.Page == null) ? Page : pdfLoadedFieldItem.Page);
					}
					if (pdfPageBase != null)
					{
						DrawTextBox(pdfPageBase.Graphics, pdfLoadedFieldItem);
					}
				}
				else
				{
					if (base.Form.m_pageMap.Count <= 0 || pdfDictionary == null || !(PdfCrossTable.Dereference(pdfDictionary["P"]) is PdfDictionary key))
					{
						continue;
					}
					PdfPageBase pdfPageBase2 = base.Form.m_pageMap[key];
					if (pdfPageBase2.Dictionary["Annots"] is PdfArray pdfArray)
					{
						for (int j = 0; j < pdfArray.Count - 1; j++)
						{
							if (pdfArray[j] is PdfReferenceHolder && PdfCrossTable.Dereference(pdfArray[j]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Parent") && PdfCrossTable.Dereference(pdfDictionary2["Parent"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("T") && pdfDictionary3["T"] is PdfString pdfString && pdfString.Value == Name)
							{
								pdfArray.RemoveAt(j);
							}
						}
					}
					DrawTextBox(pdfPageBase2.Graphics, pdfLoadedFieldItem);
				}
			}
		}
		else
		{
			DrawTextBox(Page.Graphics, null);
		}
	}

	private void DrawTextBox(PdfGraphics graphics, PdfLoadedFieldItem item)
	{
		GetGraphicsProperties(out var graphicsProperties, item);
		if (base.Flatten && graphicsProperties.BorderWidth == 0f && graphicsProperties.Pen != null)
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (!widgetAnnotation.ContainsKey("BS"))
			{
				graphicsProperties.BorderWidth = 1f;
				graphicsProperties.Pen.Width = 1f;
			}
			else if (PdfCrossTable.Dereference(widgetAnnotation["BS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("W"))
			{
				PdfNumber pdfNumber = pdfDictionary["W"] as PdfNumber;
				graphicsProperties.BorderWidth = pdfNumber.FloatValue;
				graphicsProperties.Pen.Width = pdfNumber.FloatValue;
			}
		}
		if (graphics.Layer == null)
		{
			graphicsProperties.Rect.Size = graphics.Size;
		}
		if (!base.Flatten)
		{
			graphicsProperties.Rect.Location = new PointF(0f, 0f);
		}
		string text = Text;
		if (Password)
		{
			text = string.Empty;
			for (int i = 0; i < Text.Length; i++)
			{
				text += "*";
			}
		}
		string finalText = string.Empty;
		bool replace = false;
		if (!base.DisableAutoFormat)
		{
			MapNumberFormat(text, updateDictionary: false, out finalText, out replace);
		}
		text = (replace ? finalText : text);
		if (replace)
		{
			graphicsProperties.ForeBrush = base.ForeBrush;
		}
		_ = graphicsProperties.BackBrush;
		graphicsProperties.StringFormat.RightToLeft = isRTL(text);
		char[] array = text.ToCharArray();
		for (int j = 0; j < array.Length; j++)
		{
			if (IsRTLChar(array[j]))
			{
				graphicsProperties.StringFormat.TextDirection = PdfTextDirection.RightToLeft;
				break;
			}
		}
		graphicsProperties.StringFormat.LineLimit = false;
		if (!Multiline)
		{
			graphicsProperties.StringFormat.LineAlignment = PdfVerticalAlignment.Middle;
			if (((PdfField)this).Form.NeedAppearances)
			{
				graphicsProperties.StringFormat.WordWrap = PdfWordWrapType.None;
			}
		}
		if (!Multiline && base.Flatten)
		{
			graphicsProperties.StringFormat.WordWrap = PdfWordWrapType.Character;
			if (graphicsProperties.Font.Height < graphicsProperties.Rect.Height)
			{
				graphicsProperties.StringFormat.LineLimit = true;
			}
		}
		if (graphicsProperties.Font is PdfTrueTypeFont && ((PdfField)this).Form.SetAppearanceDictionary && !base.Flatten && graphicsProperties.RotationAngle == 0 && text.Length < 255 && !base.Flatten)
		{
			SizeF sizeF = graphicsProperties.Font.MeasureString(text);
			if (sizeF.Width > graphicsProperties.Rect.Width && sizeF.Height < graphicsProperties.Rect.Height && !InsertSpaces && !Multiline)
			{
				graphicsProperties.Rect.Width = sizeF.Width;
			}
		}
		PaintParams paintParams = new PaintParams(graphicsProperties.Rect, graphicsProperties.BackBrush, graphicsProperties.ForeBrush, graphicsProperties.Pen, graphicsProperties.Style, graphicsProperties.BorderWidth, graphicsProperties.ShadowBrush, graphicsProperties.RotationAngle);
		paintParams.IsRequired = Required;
		if (Page != null)
		{
			paintParams.PageRotationAngle = Page.Rotation;
		}
		paintParams.isFlatten = base.Flatten;
		paintParams.InsertSpace = InsertSpaces;
		if ((graphicsProperties.Font.Name.Equals("TimesLTStd-Roman") || graphicsProperties.Font.Name.Equals("Arial")) && graphicsProperties.Font is PdfStandardFont pdfStandardFont)
		{
			graphicsProperties.Font = new PdfStandardFont(pdfStandardFont.FontFamily, graphicsProperties.Font.Size, graphicsProperties.Font.Style);
		}
		PdfResources pdfResources = null;
		bool flag = false;
		if (graphicsProperties.Font is PdfStandardFont)
		{
			PdfStandardFont pdfStandardFont2 = graphicsProperties.Font as PdfStandardFont;
			if (!graphicsProperties.Font.Name.Equals(pdfStandardFont2.FontFamily.ToString()) && Page != null)
			{
				pdfResources = Page.GetResources();
				pdfResources.OriginalFontName = pdfStandardFont2.Name;
				flag = true;
				if (PdfString.IsUnicode(text))
				{
					pdfStandardFont2.SetTextEncoding(new Windows1252Encoding());
				}
			}
		}
		if (PdfString.IsUnicode(text) && base.Font is PdfStandardFont)
		{
			graphics.isStandardUnicode = IsUnicodeStandardFont();
		}
		if (text != string.Empty && (base.Flatten || m_isCustomFontSize) && (AutoResizeText || m_isCustomFontSize) && !Multiline)
		{
			SetFittingFontSize(ref graphicsProperties, paintParams, text);
		}
		else if ((AutoResizeText || m_isCustomFontSize) && Multiline)
		{
			if (!((PdfField)this).Form.NeedAppearances && !base.Flatten)
			{
				PdfDictionary pdfDictionary2 = null;
				float height = 0f;
				string text2 = "";
				if (base.Dictionary["DA"] is PdfString pdfString)
				{
					text2 = FontName(pdfString.Value, out height);
				}
				PdfDictionary stream = graphics.StreamWriter.GetStream();
				stream = PdfCrossTable.Dereference(stream["Resources"]) as PdfDictionary;
				if (stream != null && stream.ContainsKey("Font"))
				{
					pdfDictionary2 = PdfCrossTable.Dereference(stream["Font"]) as PdfDictionary;
					if (!pdfDictionary2.ContainsKey(text2))
					{
						pdfDictionary2[new PdfName(text2)] = new PdfReferenceHolder(base.Font);
						stream["Font"] = new PdfReferenceHolder(pdfDictionary2);
					}
				}
				else if (pdfDictionary2 == null)
				{
					pdfDictionary2 = new PdfDictionary();
					pdfDictionary2[new PdfName(text2)] = new PdfReferenceHolder(base.Font);
					stream["Font"] = new PdfReferenceHolder(pdfDictionary2);
				}
				if (graphicsProperties.BorderWidth == 0f && graphicsProperties.Pen != null && !GetWidgetAnnotation(base.Dictionary, base.CrossTable).ContainsKey("BS"))
				{
					graphicsProperties.BorderWidth = 1f;
					graphicsProperties.Pen.Width = 1f;
				}
			}
			if (text != string.Empty)
			{
				SetMultiLineFontSize(ref graphicsProperties, text);
			}
			FieldPainter.isAutoFontSize = true;
		}
		if (graphics != null && graphics.IsTemplateGraphics && base.Visibility != PdfFormFieldVisibility.Hidden)
		{
			graphics.Save();
			if (AutoResizeText)
			{
				SetFittingFontSize(ref graphicsProperties, paintParams, text);
			}
			FieldPainter.DrawTextBox(graphics, paintParams, text, graphicsProperties.Font, graphicsProperties.StringFormat, Multiline, Scrollable, MaxLength);
			graphics.Restore();
			return;
		}
		graphics.Save();
		if (base.Dictionary.ContainsKey("Q"))
		{
			PdfNumber pdfNumber2 = base.Dictionary["Q"] as PdfNumber;
			if (base.Dictionary["T"] is PdfString pdfString2)
			{
				string value = pdfString2.Value;
				if ((pdfNumber2 != null && pdfNumber2.IntValue == 2 && value[value.Length - 1] == 'a') || (pdfNumber2.IntValue == 0 && value[value.Length - 1] == 'b'))
				{
					MaxLength = 0;
				}
			}
		}
		if (base.Dictionary.ContainsKey("Rect") || base.Dictionary.ContainsKey("Kids"))
		{
			if (InsertSpaces && MaxLength != 0)
			{
				FieldPainter.DrawTextBox(graphics, paintParams, text, graphicsProperties.Font, graphicsProperties.StringFormat, Multiline, Scrollable, MaxLength);
			}
			else
			{
				bool flag2 = false;
				if (DefaultValue != null && DefaultValue == Text && PdfString.IsUnicode(DefaultValue) && graphicsProperties.Font is PdfStandardFont && (!base.Dictionary.ContainsKey("DA") || base.Dictionary.ContainsKey("AP")) && !base.Dictionary.ContainsKey("Q"))
				{
					flag2 = true;
				}
				bool flag3 = false;
				if (base.Form.Dictionary.ContainsKey("NeedAppearances") && base.Form.Dictionary["NeedAppearances"] is PdfBoolean pdfBoolean)
				{
					flag3 = pdfBoolean.Value;
				}
				if (!AutoResizeText && base.RotationAngle == 0 && !m_isFieldPropertyChanged && !flag2 && !base.Form.Dictionary.ContainsKey("NeedAppearances") && !base.Dictionary.ContainsKey("DV") && ((PdfString.IsUnicode(Text) && base.Dictionary.ContainsKey("AP")) || (base.Dictionary.ContainsKey("AP") && base.Dictionary.ContainsKey("PMD") && base.Flatten) || (item != null && item.Dictionary != null && item.Dictionary.ContainsKey("AP") && base.Flatten) || (!base.Form.Dictionary.ContainsKey("NeedAppearances") && base.Dictionary.ContainsKey("AP") && base.Flatten && graphics.Page is PdfLoadedPage && (graphics.Page as PdfLoadedPage).CropBox.X == 0f && (graphics.Page as PdfLoadedPage).CropBox.Y == 0f)))
				{
					flag2 = true;
				}
				if (!flag2 && ((m_isCustomFontSize && m_applyAppearence && base.Dictionary.ContainsKey("AP") && !m_isFieldPropertyChanged && graphicsProperties.Font is PdfTrueTypeFont) || (!PdfString.IsUnicode(Text) && graphicsProperties.Font is PdfStandardFont && !m_isFieldPropertyChanged && m_applyAppearence && !flag3 && base.Flatten && base.RotationAngle == 0 && ((base.Dictionary.ContainsKey("AP") && base.Dictionary.ContainsKey("AA")) || (item != null && item.Dictionary != null && item.Dictionary.ContainsKey("AP") && base.Dictionary.ContainsKey("AA"))))))
				{
					flag2 = true;
				}
				if (flag && !AutoResizeText && !flag2 && base.RotationAngle == 0 && !m_isFieldPropertyChanged && base.Dictionary.ContainsKey("AP") && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary3 && pdfDictionary3.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary3["N"]) is PdfStream apperenceStream)
				{
					flag2 = FindAppearanceDrawFromStream(apperenceStream);
				}
				if (!flag && !flag2 && graphicsProperties.Font is PdfStandardFont && base.Dictionary.ContainsKey("Parent") && base.Dictionary.ContainsKey("Kids") && (graphicsProperties.Font as PdfStandardFont).FontInternal is PdfDictionary pdfDictionary4 && pdfDictionary4.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary4["Encoding"]) is PdfDictionary pdfDictionary5 && pdfDictionary5.ContainsKey("Differences") && !pdfDictionary4.ContainsKey("Widths") && !m_isFieldPropertyChanged && base.Dictionary.ContainsKey("Kids"))
				{
					PdfDictionary widgetAnnotation2 = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
					PdfDictionary pdfDictionary6 = PdfCrossTable.Dereference(widgetAnnotation2["MK"]) as PdfDictionary;
					PdfNumber pdfNumber3 = null;
					int num = 0;
					if (pdfDictionary6 != null && pdfDictionary6.ContainsKey("R"))
					{
						pdfNumber3 = PdfCrossTable.Dereference(pdfDictionary6["R"]) as PdfNumber;
						if (pdfNumber3 != null)
						{
							num = pdfNumber3.IntValue;
						}
					}
					if (widgetAnnotation2 != null && ((pdfNumber3 != null && num == 0) || pdfNumber3 == null) && base.RotationAngle == 0 && widgetAnnotation2.ContainsKey("AP") && PdfCrossTable.Dereference(widgetAnnotation2["AP"]) is PdfDictionary pdfDictionary7 && pdfDictionary7.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary7["N"]) is PdfStream pdfStream && pdfStream.Data.Length != 0)
					{
						flag2 = true;
					}
				}
				if (base.CrossTable != null && base.CrossTable.Document != null && base.CrossTable.Document is PdfLoadedDocument)
				{
					m_applyAppearence = !(base.CrossTable.Document as PdfLoadedDocument).RaisePdfFont;
				}
				if (flag2 || (base.Dictionary.ContainsKey("Kids") && !base.Dictionary.ContainsKey("DA")) || (base.Dictionary.ContainsKey("AP") && base.Dictionary.ContainsKey("AA") && !base.Dictionary.ContainsKey("Q") && !base.Dictionary.ContainsKey("BS") && base.Dictionary.ContainsKey("Parent")))
				{
					PdfDictionary pdfDictionary8 = ((item != null && item.Dictionary != null) ? item.Dictionary : GetWidgetAnnotation(base.Dictionary, base.CrossTable));
					if (pdfDictionary8.ContainsKey("AP") && !m_isFieldPropertyChanged && m_applyAppearence && !m_isFontModified)
					{
						if (base.Flatten && graphics.Page != null)
						{
							graphics.Save();
							if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle90)
							{
								graphics.TranslateTransform(graphics.Size.Width, graphics.Size.Height);
								graphics.RotateTransform(90f);
							}
							else if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle180)
							{
								graphics.TranslateTransform(graphics.Size.Width, graphics.Size.Height);
								graphics.RotateTransform(-180f);
							}
							else if (graphics.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
							{
								graphics.TranslateTransform(graphics.Size.Width, graphics.Size.Height);
								graphics.RotateTransform(270f);
							}
						}
						if (PdfCrossTable.Dereference(pdfDictionary8["AP"]) is PdfDictionary pdfDictionary9)
						{
							IPdfPrimitive pdfPrimitive = pdfDictionary9["N"];
							if (pdfPrimitive != null && PdfCrossTable.Dereference(pdfPrimitive) is PdfDictionary pdfDictionary10 && pdfDictionary10 is PdfStream template)
							{
								PdfTemplate pdfTemplate = new PdfTemplate(template);
								if (pdfTemplate != null)
								{
									if (pdfTemplate.m_content != null)
									{
										PdfName pdfName = null;
										PdfDictionary content = pdfTemplate.m_content;
										if (!content.ContainsKey("Subtype"))
										{
											pdfName = content.GetName("Form");
											content["Subtype"] = pdfName;
										}
										if (!content.ContainsKey("Type"))
										{
											pdfName = content.GetName("XObject");
											content["Type"] = pdfName;
										}
									}
									if (item != null)
									{
										graphics.DrawPdfTemplate(pdfTemplate, item.Location);
									}
									else
									{
										graphics.DrawPdfTemplate(pdfTemplate, base.Bounds.Location, base.Bounds.Size);
									}
								}
							}
						}
						graphics.Restore();
					}
					else
					{
						if (AutoResizeText)
						{
							SetFittingFontSize(ref graphicsProperties, paintParams, text);
						}
						FieldPainter.DrawTextBox(graphics, paintParams, text, graphicsProperties.Font, graphicsProperties.StringFormat, Multiline, Scrollable);
					}
				}
				else
				{
					paintParams.Bounds = paintParams.Bounds;
					if (graphicsProperties.Font is PdfStandardFont && graphics.isStandardUnicode && IsChineseString(text))
					{
						graphicsProperties.Font = new PdfCjkStandardFont(PdfCjkFontFamily.HeiseiMinchoW3, graphicsProperties.Font.Size);
					}
					FieldPainter.DrawTextBox(graphics, paintParams, text, graphicsProperties.Font, graphicsProperties.StringFormat, Multiline, Scrollable);
				}
			}
		}
		graphics.Restore();
		if (flag && pdfResources != null)
		{
			pdfResources.OriginalFontName = null;
		}
	}

	internal bool IsRTLChar(char input)
	{
		bool result = false;
		if (input >= '\u0590' && input <= '\u05ff')
		{
			result = true;
		}
		else if ((input >= '\u0600' && input <= 'ۿ') || (input >= 'ݐ' && input <= 'ݿ') || (input >= 'ࢠ' && input <= '\u08ff') || (input >= 'ﭐ' && input <= '\ufeff') || (input >= 126464 && input <= 126719))
		{
			result = true;
		}
		else if (input >= 67648 && input <= 67679)
		{
			result = true;
		}
		else if (input >= 66464 && input <= 66527)
		{
			result = true;
		}
		return result;
	}

	private void SetFittingFontSize(ref GraphicsProperties gp, PaintParams prms, string text)
	{
		float num = 0f;
		float num2 = 0f;
		num2 = ((prms.BorderStyle != PdfBorderStyle.Beveled && prms.BorderStyle != PdfBorderStyle.Inset) ? (gp.Rect.Width - 4f * prms.BorderWidth) : (gp.Rect.Width - 8f * prms.BorderWidth));
		float num3 = gp.Rect.Height - 2f * gp.BorderWidth;
		float num4 = 0.248f;
		_ = gp.Font is PdfStandardFont;
		if (text.EndsWith(" "))
		{
			gp.StringFormat.MeasureTrailingSpaces = true;
		}
		for (float num5 = 0f; num5 <= gp.Rect.Height; num5 += 1f)
		{
			if (gp.Font is PdfStandardFont)
			{
				gp.Font.Size = num5;
			}
			else
			{
				gp.Font.Size = num5;
			}
			SizeF sizeF = gp.Font.MeasureString(text, gp.StringFormat);
			if (!(sizeF.Width > gp.Rect.Width) && !(sizeF.Height > num3))
			{
				continue;
			}
			num = num5;
			do
			{
				num -= 0.001f;
				gp.Font.Size = num;
				float lineWidth = gp.Font.GetLineWidth(text, gp.StringFormat);
				if (num < num4)
				{
					gp.Font.Size = num4;
					break;
				}
				sizeF = gp.Font.MeasureString(text, gp.StringFormat);
				if (lineWidth < num2 && sizeF.Height < num3)
				{
					gp.Font.Size = num;
					break;
				}
			}
			while (num > num4);
			break;
		}
	}

	private void SetMultiLineFontSize(ref GraphicsProperties gp, string text)
	{
		GraphicsProperties graphicsProperties = gp;
		bool flag = false;
		float num = graphicsProperties.Font.Metrics.Size;
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		graphicsProperties.Rect.Width = graphicsProperties.Rect.Width - 2f * graphicsProperties.BorderWidth;
		graphicsProperties.Rect.Height = graphicsProperties.Rect.Height - 6f * graphicsProperties.BorderWidth;
		if (text.EndsWith(" "))
		{
			graphicsProperties.StringFormat.MeasureTrailingSpaces = true;
		}
		SizeF sizeF = graphicsProperties.Font.MeasureString(text, graphicsProperties.StringFormat);
		float num2 = (int)(graphicsProperties.Rect.Height / sizeF.Height);
		for (float num3 = num2 * graphicsProperties.Rect.Width; num3 <= sizeF.Width; num3 = num2 * graphicsProperties.Rect.Width)
		{
			flag = true;
			num -= 0.2f;
			graphicsProperties.Font.Metrics.Size = num;
			sizeF = graphicsProperties.Font.MeasureString(text, graphicsProperties.StringFormat);
			num2 = (int)(graphicsProperties.Rect.Height / sizeF.Height);
		}
		if (flag)
		{
			PdfStringLayoutResult pdfStringLayoutResult = pdfStringLayouter.Layout(text, graphicsProperties.Font, graphicsProperties.StringFormat, graphicsProperties.Rect.Size);
			if (num2 != (float)pdfStringLayoutResult.LineCount && (float)pdfStringLayoutResult.LineCount > num2)
			{
				gp.Font.Metrics.Size = num - 0.4f;
			}
			else
			{
				gp.Font.Metrics.Size = num - 0.2f;
			}
		}
		if (base.Dictionary == null || !base.Dictionary.ContainsKey("DA") || !(base.CrossTable.GetObject(base.Dictionary["DA"]) is PdfString pdfString))
		{
			return;
		}
		float height = 0f;
		_ = string.Empty;
		FontName(pdfString.Value, out height);
		if (height == 0f && base.Dictionary.ContainsKey("AP") && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfStream apperenceStream)
		{
			num = FindAppearanceFontSizeFromStream(apperenceStream);
			if (num != 0f)
			{
				gp.Font.Metrics.Size = num;
			}
			graphicsProperties.Font.Metrics.Size = gp.Font.Metrics.Size;
			if (pdfStringLayouter.Layout(text, graphicsProperties.Font, graphicsProperties.StringFormat, graphicsProperties.Rect.Size).Remainder != null)
			{
				gp.StringFormat.WordWrap = PdfWordWrapType.Character;
			}
		}
	}

	private bool isRTL(string text)
	{
		return new Regex("[\\u0600-\\u06ff]\\?[ ]\\?[0-9]\\?").IsMatch(text);
	}

	private bool IsRTLText(ushort[] characterCodes)
	{
		bool result = false;
		int i = 0;
		for (int num = characterCodes.Length; i < num; i++)
		{
			if (characterCodes[i] == 2 || characterCodes[i] == 6)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal override float GetFontHeight(PdfFontFamily family)
	{
		float num = 12f;
		PdfStandardFont pdfStandardFont = new PdfStandardFont(family, 12f);
		if (!Multiline)
		{
			float width = pdfStandardFont.MeasureString(Text).Width;
			num = 8f * (base.Bounds.Size.Width - 4f * base.BorderWidth) / width;
			return (num > 8f) ? 8f : num;
		}
		return 12.5f;
	}

	private bool IsUnicodeStandardFont()
	{
		if (base.Font.FontInternal is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Encoding"))
		{
			PdfName pdfName = pdfDictionary["Encoding"] as PdfName;
			if (pdfName != null && pdfName.Value == "WinAnsiEncoding")
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsChineseString(string text)
	{
		return Regex.IsMatch(text, "[一-龥]");
	}

	private bool FindAppearanceDrawFromStream(PdfStream apperenceStream)
	{
		bool result = false;
		foreach (PdfRecord item in new ContentParser(apperenceStream.GetDecompressedData()).ReadContent())
		{
			string empty = string.Empty;
			_ = item.Operands;
			switch (item.OperatorName)
			{
			case "Tj":
			case "'":
			case "TJ":
			case "m":
			case "l":
			case "S":
				result = true;
				goto end_IL_008a;
			}
			continue;
			end_IL_008a:
			break;
		}
		return result;
	}

	private float FindAppearanceFontSizeFromStream(PdfStream apperenceStream)
	{
		float result = 0f;
		foreach (PdfRecord item in new ContentParser(apperenceStream.GetDecompressedData()).ReadContent())
		{
			_ = string.Empty;
			string[] operands = item.Operands;
			if (item.OperatorName == "Tf")
			{
				result = float.Parse(operands[1], CultureInfo.InvariantCulture);
				break;
			}
		}
		return result;
	}

	private string FindOperator(int token)
	{
		return new string[79]
		{
			"b", "B", "bx", "Bx", "BDC", "BI", "BMC", "BT", "BX", "c",
			"cm", "CS", "cs", "d", "d0", "d1", "Do", "DP", "EI", "EMC",
			"ET", "EX", "f", "F", "fx", "G", "g", "gs", "h", "i",
			"ID", "j", "J", "K", "k", "l", "m", "M", "MP", "n",
			"q", "Q", "re", "RG", "rg", "ri", "s", "S", "SC", "sc",
			"SCN", "scn", "sh", "f*", "Tx", "Tc", "Td", "TD", "Tf", "Tj",
			"TJ", "TL", "Tm", "Tr", "Ts", "Tw", "Tz", "v", "w", "W",
			"W*", "Wx", "y", "T*", "b*", "B*", "'", "\"", "true"
		}.GetValue(token) as string;
	}

	public void RemoveAt(int index)
	{
		if (Items != null && Items.Count != 0)
		{
			PdfLoadedTexBoxItem item = Items[index];
			Remove(item);
		}
	}

	public void Remove(PdfLoadedTexBoxItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		int index = Items.Remove(item);
		if (!(base.Dictionary["Kids"] is PdfArray pdfArray))
		{
			return;
		}
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(pdfArray[index]) as PdfDictionary;
		PdfReferenceHolder pdfReferenceHolder = pdfArray[index] as PdfReferenceHolder;
		if (pdfDictionary != null)
		{
			if (item.Page != null && pdfReferenceHolder != null && PdfCrossTable.Dereference(item.Page.Dictionary["Annots"]) is PdfArray pdfArray2)
			{
				pdfArray2.Remove(pdfReferenceHolder);
				pdfArray2.MarkChanged();
			}
			if (base.CrossTable.PdfObjects.Contains(pdfDictionary))
			{
				base.CrossTable.PdfObjects.Remove(base.CrossTable.PdfObjects.IndexOf(pdfDictionary));
			}
		}
		pdfArray.RemoveAt(index);
		pdfArray.MarkChanged();
	}

	private void MapNumberFormat(string text, bool updateDictionary, out string finalText, out bool replace)
	{
		finalText = string.Empty;
		replace = false;
		if (!base.Dictionary.ContainsKey("AA") || !(PdfCrossTable.Dereference(base.Dictionary["AA"]) is PdfDictionary pdfDictionary) || !pdfDictionary.ContainsKey("F") || !(PdfCrossTable.Dereference(pdfDictionary["F"]) is PdfDictionary pdfDictionary2) || !pdfDictionary2.ContainsKey("JS") || !pdfDictionary2.ContainsKey("S") || !(PdfCrossTable.Dereference(pdfDictionary2["JS"]) is PdfString { Value: not null } pdfString))
		{
			return;
		}
		string text2 = pdfString.Value;
		string text3 = text2;
		if (!text2.Contains("AFNumber_Format"))
		{
			return;
		}
		if (text2.Contains("="))
		{
			int num = text3.IndexOf("=");
			text3 = text3.Substring(0, num + 1);
			text2 = text2.Replace(text3, "");
		}
		text2 = text2.Replace("AFNumber_Format(", "");
		text2 = text2.Replace(");", "");
		text2 = text2.Replace(")", "");
		string[] array = text2.Split(',');
		bool space = false;
		bool flag = false;
		string text4 = new Regex("[^0-9-,-.]").Replace(text, "");
		flag = text4.StartsWith("-");
		if (flag)
		{
			text4 = text4.Replace("-", "");
		}
		if (string.IsNullOrEmpty(text4))
		{
			if (!string.IsNullOrEmpty(text))
			{
				finalText = text4;
				replace = true;
			}
			return;
		}
		int separatorStyle = int.Parse(array[1], CultureInfo.InvariantCulture);
		if (updateDictionary)
		{
			UpdateNumberFormatTextAlone(separatorStyle, text4, array, flag, out finalText, out replace);
			return;
		}
		int decimalSpace = int.Parse(array[0], CultureInfo.InvariantCulture);
		int negativeNumberStyle = int.Parse(array[2], CultureInfo.InvariantCulture);
		int.Parse(array[3], CultureInfo.InvariantCulture);
		char[] chars = array[4].ToCharArray();
		string currencyText = string.Empty;
		bool preAppend = bool.Parse(array[5]);
		text4 = UpdateTextFromSeparatorStyle(separatorStyle, text4, array, decimalSpace, text);
		ConvertUnicodeToCurrency(chars, preAppend, array, out space, out currencyText);
		UpdateNegativeTextWithCurrency(flag, negativeNumberStyle, preAppend, currencyText, text4, space, out finalText);
		replace = true;
	}

	private void ConvertUnicodeToCurrency(char[] chars, bool preAppend, string[] splitted, out bool space, out string currencyText)
	{
		currencyText = string.Empty;
		space = false;
		if (chars.Length <= 3)
		{
			return;
		}
		if (preAppend)
		{
			space = chars[^2] == ' ';
		}
		else if (splitted[4].Contains("u"))
		{
			int num = splitted[4].IndexOf('u');
			space = chars[num - 2] == ' ';
		}
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>();
		foreach (char c in chars)
		{
			switch (c)
			{
			case 'u':
				if (stringBuilder.ToString() != string.Empty)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
				}
				break;
			default:
				stringBuilder.Append(c);
				break;
			case ' ':
			case '"':
			case '\\':
				break;
			}
		}
		if (stringBuilder.ToString() != string.Empty)
		{
			list.Add(stringBuilder.ToString());
		}
		stringBuilder = null;
		foreach (string item in list)
		{
			if (item.Length > 3)
			{
				currencyText += (char)long.Parse(item.Substring(0, 4), NumberStyles.HexNumber);
			}
			else
			{
				currencyText += item;
			}
		}
		list.Clear();
	}

	private void UpdateNegativeTextWithCurrency(bool negativeText, int negativeNumberStyle, bool preAppend, string currencyText, string number, bool space, out string finalText)
	{
		finalText = string.Empty;
		if (negativeText && negativeNumberStyle == 0)
		{
			finalText += "-";
		}
		else if (negativeText && negativeNumberStyle > 1)
		{
			finalText += "(";
		}
		if (preAppend)
		{
			finalText += currencyText;
			finalText += (space ? " " : "");
			finalText += number;
		}
		else
		{
			finalText += number;
			finalText += (space ? " " : "");
			finalText += currencyText;
		}
		if (negativeText && negativeNumberStyle > 1)
		{
			finalText += ")";
		}
		if (negativeNumberStyle == 1 || negativeNumberStyle == 3)
		{
			ForeColor = new PdfColor(Color.Red);
		}
	}

	private void UpdateNumberFormatTextAlone(int separatorStyle, string number, string[] splitted, bool negativeText, out string finalText, out bool replace)
	{
		finalText = string.Empty;
		replace = false;
		string empty = string.Empty;
		string text = string.Empty;
		string empty2 = string.Empty;
		empty2 = ((separatorStyle > 1) ? "," : ".");
		if (separatorStyle > 1)
		{
			number = number.Replace(".", "");
			splitted = number.Split(',');
		}
		else
		{
			number = number.Replace(",", "");
			splitted = number.Split('.');
		}
		empty = splitted[0];
		for (int i = 1; i < splitted.Length; i++)
		{
			text += splitted[i];
		}
		finalText = (negativeText ? "-" : "");
		finalText += empty;
		if (text.Length > 0)
		{
			finalText = finalText + empty2 + text;
		}
		replace = true;
	}

	private string UpdateTextFromSeparatorStyle(int separatorStyle, string number, string[] splitted, int decimalSpace, string text)
	{
		string empty = string.Empty;
		string text2 = string.Empty;
		_ = string.Empty;
		string empty2 = string.Empty;
		if (separatorStyle > 1)
		{
			number = number.Replace(".", "");
			splitted = number.Split(',');
		}
		else
		{
			number = number.Replace(",", "");
			splitted = number.Split('.');
		}
		empty = splitted[0];
		if (empty.Length > 1)
		{
			empty = empty.TrimStart(new char[1] { '0' });
		}
		empty2 = empty + ".";
		for (int i = 1; i < splitted.Length; i++)
		{
			text2 += splitted[i];
		}
		if (text2.Length < decimalSpace)
		{
			int num = decimalSpace - text2.Length;
			for (int j = 0; j < num; j++)
			{
				text2 += 0;
			}
		}
		empty2 += text2;
		string symbol = string.Empty;
		switch (separatorStyle)
		{
		case 0:
			symbol = ",";
			break;
		case 2:
			symbol = ".";
			break;
		case 4:
			symbol = "'";
			break;
		}
		if (decimalSpace == 0 && text2.Length == 0)
		{
			return number = UpdateTextWithDecimalBreak(number, empty2, text, separatorStyle, symbol);
		}
		decimal d = decimal.Parse(empty2, CultureInfo.InvariantCulture);
		d = Math.Round(d, decimalSpace);
		if (separatorStyle == 1)
		{
			return number = d.ToString();
		}
		number = UpdateDecimalRoundOffText(number, d, decimalSpace, separatorStyle, symbol);
		return number;
	}

	private string UpdateTextWithDecimalBreak(string number, string decimalProcess, string text, int separatorStyle, string symbol)
	{
		number = decimalProcess.Replace(".", "");
		if (!string.IsNullOrEmpty(number))
		{
			if (number.GetType() == typeof(int))
			{
				if (int.Parse(number) <= 0)
				{
					number = "0";
				}
			}
			else if (long.Parse(number) <= 0)
			{
				number = "0";
			}
		}
		else
		{
			number = "0";
		}
		if (separatorStyle == 1 || separatorStyle == 3)
		{
			return number;
		}
		if (number.Length > 3)
		{
			bool flag = false;
			int num = 0;
			string empty = string.Empty;
			string empty2 = string.Empty;
			flag = ((number.Length > 3 && number.Length % 3 != 0) ? true : false);
			num = number.Length % 3;
			empty = number.Substring(0, num);
			empty += (flag ? symbol : "");
			empty2 = number.Substring(num, number.Length - num);
			for (int i = 0; i < empty2.Length; i++)
			{
				if (i != 0 && i % 3 == 0)
				{
					empty += symbol;
				}
				empty += empty2[i];
			}
			number = empty;
		}
		return number;
	}

	private string UpdateDecimalRoundOffText(string number, decimal parsedNumber, int decimalSpace, int separatorStyle, string symbol)
	{
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		string[] array = parsedNumber.ToString().Split('.');
		string text = array[0];
		string empty = string.Empty;
		string empty2 = string.Empty;
		string text2 = ((separatorStyle > 1) ? "," : ".");
		bool flag = ((text.Length > 3 && text.Length % 3 != 0) ? true : false);
		int num = text.Length % 3;
		empty = text.Substring(0, num);
		empty += (flag ? symbol : "");
		empty2 = text.Substring(num, text.Length - num);
		for (int i = 0; i < empty2.Length; i++)
		{
			if (i != 0 && i % 3 == 0)
			{
				empty += symbol;
			}
			empty += empty2[i];
		}
		text = empty;
		if (decimalSpace > 0)
		{
			number = text + text2;
			for (int j = 1; j < array.Length; j++)
			{
				number += array[j];
			}
		}
		else
		{
			number = text;
		}
		Thread.CurrentThread.CurrentCulture = currentCulture;
		return number;
	}
}
