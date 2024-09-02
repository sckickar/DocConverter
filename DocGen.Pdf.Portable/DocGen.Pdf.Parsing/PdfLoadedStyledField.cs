using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedStyledField : PdfLoadedField
{
	protected struct GraphicsProperties
	{
		public RectangleF Rect;

		public PdfPen Pen;

		public PdfBorderStyle Style;

		public float BorderWidth;

		public PdfBrush BackBrush;

		public PdfBrush ForeBrush;

		public PdfBrush ShadowBrush;

		public PdfFont Font;

		public PdfStringFormat StringFormat;

		public int RotationAngle;

		public GraphicsProperties(PdfLoadedStyledField field)
		{
			if (field == null)
			{
				throw new ArgumentNullException("field");
			}
			Rect = field.Bounds;
			Pen = field.BorderPen;
			Style = field.BorderStyle;
			BorderWidth = field.BorderWidth;
			BackBrush = field.BackBrush;
			ForeBrush = field.ForeBrush;
			ShadowBrush = field.ShadowBrush;
			Font = field.Font;
			StringFormat = field.StringFormat;
			RotationAngle = field.RotationAngle;
			if (field.Page != null && field.Page.Rotation != 0)
			{
				Rect = RotateTextbox(field.Bounds, field.Page.Size, field.Page.Rotation);
			}
		}

		public GraphicsProperties(PdfLoadedFieldItem item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			Rect = item.Bounds;
			Pen = item.BorderPen;
			Style = item.BorderStyle;
			BorderWidth = item.BorderWidth;
			BackBrush = item.BackBrush;
			ForeBrush = item.ForeBrush;
			ShadowBrush = item.ShadowBrush;
			Font = item.Font;
			StringFormat = item.StringFormat;
			RotationAngle = 0;
			PdfName key = new PdfName("MK");
			if (item.Dictionary.ContainsKey(key))
			{
				PdfDictionary pdfDictionary = null;
				pdfDictionary = ((!(item.Dictionary["MK"] as PdfReferenceHolder != null)) ? (item.Dictionary["MK"] as PdfDictionary) : (PdfCrossTable.Dereference(item.Dictionary["MK"]) as PdfDictionary));
				PdfName key2 = new PdfName("R");
				if (pdfDictionary != null && pdfDictionary.ContainsKey(key2) && pdfDictionary.Items[new PdfName("R")] is PdfNumber pdfNumber)
				{
					RotationAngle = pdfNumber.IntValue;
				}
			}
			if (item.Page != null && item.Page.Rotation != 0)
			{
				Rect = RotateTextbox(item.Bounds, item.Page.Size, item.Page.Rotation);
			}
		}

		private RectangleF RotateTextbox(RectangleF rect, SizeF size, PdfPageRotateAngle angle)
		{
			RectangleF result = rect;
			if (angle == PdfPageRotateAngle.RotateAngle180)
			{
				float x = size.Width - (rect.X + rect.Width);
				float y = size.Height - (rect.Y + rect.Height);
				result = new RectangleF(x, y, rect.Width, rect.Height);
			}
			if (angle == PdfPageRotateAngle.RotateAngle270)
			{
				float y = size.Width - (rect.X + rect.Width);
				float x = rect.Y;
				result = new RectangleF(x, y, rect.Height, rect.Width);
			}
			if (angle == PdfPageRotateAngle.RotateAngle90)
			{
				float x = size.Height - (rect.Y + rect.Height);
				float y = rect.X;
				result = new RectangleF(x, y, rect.Height, rect.Width);
			}
			return result;
		}
	}

	private const byte ShadowShift = 64;

	private PdfFieldActions m_actions;

	private WidgetAnnotation m_widget = new WidgetAnnotation();

	private PdfAction m_mouseEnter;

	private PdfAction m_mouseLeave;

	private PdfAction m_mouseDown;

	private PdfAction m_mouseUp;

	private PdfAction m_gotFocus;

	private PdfAction m_lostFocus;

	private PdfPen m_borderPen;

	internal PdfFont m_font;

	private PdfFormFieldVisibility m_visibility;

	internal PdfArray m_array = new PdfArray();

	internal bool m_isTextChanged;

	internal bool m_isCustomFontSize;

	internal bool m_isFieldPropertyChanged;

	internal int m_angle;

	internal bool isRotationModified;

	internal bool m_isTextModified;

	internal bool m_isFontModified;

	internal bool m_isCropBox;

	private Dictionary<int, PdfFont> m_fontCollection = new Dictionary<int, PdfFont>();

	private bool m_mouseUpAction;

	internal bool m_isImportFields;

	public PdfAction MouseEnter
	{
		get
		{
			if (m_mouseEnter == null)
			{
				m_mouseEnter = GetPdfAction("E");
			}
			return m_mouseEnter;
		}
		set
		{
			if (value != null)
			{
				m_mouseEnter = value;
				m_actions = new PdfFieldActions(Widget.Actions);
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				widgetAnnotation.SetProperty("AA", m_actions);
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["AA"]) as PdfDictionary;
				pdfDictionary.SetProperty("E", m_mouseEnter);
				widgetAnnotation.SetProperty("AA", pdfDictionary);
				base.Changed = true;
				NotifyPropertyChanged("MouseEnter");
			}
		}
	}

	public PdfAction MouseUp
	{
		get
		{
			if (m_mouseUp == null)
			{
				m_mouseUpAction = true;
				m_mouseUp = GetPdfAction("U");
				m_mouseUpAction = false;
			}
			return m_mouseUp;
		}
		set
		{
			if (value != null)
			{
				m_mouseUp = value;
				m_actions = new PdfFieldActions(Widget.Actions);
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				widgetAnnotation.SetProperty("AA", m_actions);
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["AA"]) as PdfDictionary;
				pdfDictionary.SetProperty("U", m_mouseUp);
				widgetAnnotation.SetProperty("AA", pdfDictionary);
				base.Changed = true;
				NotifyPropertyChanged("MouseUp");
			}
		}
	}

	public PdfAction MouseDown
	{
		get
		{
			if (m_mouseDown == null)
			{
				m_mouseDown = GetPdfAction("D");
			}
			return m_mouseDown;
		}
		set
		{
			if (value != null)
			{
				m_mouseDown = value;
				m_actions = new PdfFieldActions(Widget.Actions);
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				widgetAnnotation.SetProperty("AA", m_actions);
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["AA"]) as PdfDictionary;
				pdfDictionary.SetProperty("D", m_mouseDown);
				widgetAnnotation.SetProperty("AA", pdfDictionary);
				base.Changed = true;
				NotifyPropertyChanged("MouseDown");
			}
		}
	}

	public PdfAction MouseLeave
	{
		get
		{
			if (m_mouseLeave == null)
			{
				m_mouseLeave = GetPdfAction("X");
			}
			return m_mouseLeave;
		}
		set
		{
			if (value != null)
			{
				m_mouseLeave = value;
				m_actions = new PdfFieldActions(Widget.Actions);
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				widgetAnnotation.SetProperty("AA", m_actions);
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["AA"]) as PdfDictionary;
				pdfDictionary.SetProperty("X", m_mouseLeave);
				widgetAnnotation.SetProperty("AA", pdfDictionary);
				base.Changed = true;
				NotifyPropertyChanged("MouseLeave");
			}
		}
	}

	public PdfAction GotFocus
	{
		get
		{
			if (m_gotFocus == null)
			{
				m_gotFocus = GetPdfAction("Fo");
			}
			return m_gotFocus;
		}
		set
		{
			if (value != null)
			{
				m_gotFocus = value;
				m_actions = new PdfFieldActions(Widget.Actions);
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				widgetAnnotation.SetProperty("AA", m_actions);
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["AA"]) as PdfDictionary;
				pdfDictionary.SetProperty("Fo", m_gotFocus);
				widgetAnnotation.SetProperty("AA", pdfDictionary);
				base.Changed = true;
				NotifyPropertyChanged("GotFocus");
			}
		}
	}

	internal PdfColor ForeColor
	{
		get
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfColor result = new PdfColor(0, 0, 0);
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfString pdfString = base.CrossTable.GetObject(widgetAnnotation["DA"]) as PdfString;
				return GetForeColour(pdfString.Value);
			}
			if (widgetAnnotation != null && widgetAnnotation.GetValue(base.CrossTable, "DA", "Parent") is PdfString pdfString2)
			{
				return GetForeColour(pdfString2.Value);
			}
			return result;
		}
		set
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			float height = 0f;
			string text = null;
			if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
			{
				PdfString pdfString = widgetAnnotation["DA"] as PdfString;
				text = FontName(pdfString.Value, out height);
			}
			else if (widgetAnnotation != null && base.Dictionary.ContainsKey("DA"))
			{
				PdfString pdfString2 = base.Dictionary["DA"] as PdfString;
				text = FontName(pdfString2.Value, out height);
			}
			if (text != null)
			{
				PdfDefaultAppearance pdfDefaultAppearance = new PdfDefaultAppearance();
				pdfDefaultAppearance.FontName = text;
				pdfDefaultAppearance.FontSize = height;
				pdfDefaultAppearance.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance.ToString());
			}
			else
			{
				PdfDefaultAppearance pdfDefaultAppearance2 = new PdfDefaultAppearance();
				pdfDefaultAppearance2.FontName = Font.Name;
				pdfDefaultAppearance2.FontSize = Font.Size;
				pdfDefaultAppearance2.ForeColor = value;
				widgetAnnotation["DA"] = new PdfString(pdfDefaultAppearance2.ToString());
			}
			((PdfField)this).Form.SetAppearanceDictionary = true;
			NotifyPropertyChanged("ForeColor");
		}
	}

	public PdfAction LostFocus
	{
		get
		{
			if (m_lostFocus == null)
			{
				m_lostFocus = GetPdfAction("Bl");
			}
			return m_lostFocus;
		}
		set
		{
			if (value != null)
			{
				m_lostFocus = value;
				m_actions = new PdfFieldActions(Widget.Actions);
				PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				widgetAnnotation.SetProperty("AA", m_actions);
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["AA"]) as PdfDictionary;
				pdfDictionary.SetProperty("Bl", m_lostFocus);
				widgetAnnotation.SetProperty("AA", pdfDictionary);
				base.Changed = true;
				NotifyPropertyChanged("LostFocus");
			}
		}
	}

	internal WidgetAnnotation Widget => m_widget;

	public RectangleF Bounds
	{
		get
		{
			RectangleF bounds = GetBounds(base.Dictionary, base.CrossTable);
			if (Page != null && Page.Dictionary.ContainsKey("CropBox"))
			{
				m_isCropBox = true;
				PdfArray pdfArray = null;
				pdfArray = ((!(Page.Dictionary["CropBox"] is PdfArray)) ? ((Page.Dictionary["CropBox"] as PdfReferenceHolder).Object as PdfArray) : (Page.Dictionary["CropBox"] as PdfArray));
				if ((pdfArray[0] as PdfNumber).FloatValue != 0f || (pdfArray[1] as PdfNumber).FloatValue != 0f || Page.Size.Width == (pdfArray[2] as PdfNumber).FloatValue || Page.Size.Height == (pdfArray[3] as PdfNumber).FloatValue)
				{
					bounds.X -= (pdfArray[0] as PdfNumber).FloatValue;
					bounds.Y = (pdfArray[3] as PdfNumber).FloatValue - (bounds.Y + bounds.Height);
				}
				else
				{
					bounds.Y = Page.Size.Height - (bounds.Y + bounds.Height);
				}
			}
			else if (Page != null && Page.Dictionary.ContainsKey("MediaBox"))
			{
				PdfArray pdfArray2 = null;
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				float num4 = 0f;
				if (PdfCrossTable.Dereference(Page.Dictionary["MediaBox"]) is PdfArray)
				{
					pdfArray2 = PdfCrossTable.Dereference(Page.Dictionary["MediaBox"]) as PdfArray;
				}
				if (pdfArray2 != null && pdfArray2.Count > 3)
				{
					PdfNumber pdfNumber = pdfArray2[0] as PdfNumber;
					PdfNumber pdfNumber2 = pdfArray2[1] as PdfNumber;
					PdfNumber pdfNumber3 = pdfArray2[2] as PdfNumber;
					PdfNumber pdfNumber4 = pdfArray2[3] as PdfNumber;
					if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && pdfNumber4 != null)
					{
						num = pdfNumber.FloatValue;
						num2 = pdfNumber2.FloatValue;
						num3 = pdfNumber3.FloatValue;
						num4 = pdfNumber4.FloatValue;
					}
				}
				if (num > 0f || num2 > 0f || Page.Size.Width == num3 || Page.Size.Height == num4)
				{
					bounds.X -= num;
					if (num4 > 0f)
					{
						bounds.Y = num4 - (bounds.Y + bounds.Height);
					}
					else
					{
						bounds.Y = Page.Size.Height - (bounds.Y + bounds.Height);
					}
				}
				else
				{
					bounds.Y = Page.Size.Height - (bounds.Y + bounds.Height);
					if (num != 0f || num2 != 0f)
					{
						float x = bounds.X;
						float y = bounds.Y;
						bounds.X = 0f - num - (0f - x);
						bounds.Y = num2 + y;
					}
				}
			}
			else if (Page != null)
			{
				bounds.Y = Page.Size.Height - (bounds.Y + bounds.Height);
			}
			else
			{
				bounds.Y += bounds.Height;
			}
			return bounds;
		}
		set
		{
			RectangleF rectangleF = value;
			if (rectangleF == RectangleF.Empty)
			{
				throw new ArgumentNullException("rectangle");
			}
			float height = Page.Size.Height;
			if (Page != null && Page.Dictionary.ContainsKey("CropBox"))
			{
				PdfArray pdfArray = null;
				pdfArray = ((!(Page.Dictionary["CropBox"] is PdfArray)) ? ((Page.Dictionary["CropBox"] as PdfReferenceHolder).Object as PdfArray) : (Page.Dictionary["CropBox"] as PdfArray));
				if ((pdfArray[0] as PdfNumber).FloatValue != 0f || (pdfArray[1] as PdfNumber).FloatValue != 0f || Page.Size.Width == (pdfArray[2] as PdfNumber).FloatValue || Page.Size.Height == (pdfArray[3] as PdfNumber).FloatValue)
				{
					rectangleF.X += (pdfArray[0] as PdfNumber).FloatValue;
					rectangleF.Y = (pdfArray[3] as PdfNumber).FloatValue - (rectangleF.Y + rectangleF.Height);
				}
				else
				{
					rectangleF.Y = Page.Size.Height - (rectangleF.Y + rectangleF.Height);
				}
			}
			else if (Page != null && Page.Dictionary.ContainsKey("MediaBox"))
			{
				PdfArray pdfArray2 = null;
				if (PdfCrossTable.Dereference(Page.Dictionary["MediaBox"]) is PdfArray)
				{
					pdfArray2 = PdfCrossTable.Dereference(Page.Dictionary["MediaBox"]) as PdfArray;
				}
				if ((pdfArray2[0] as PdfNumber).FloatValue > 0f || (pdfArray2[1] as PdfNumber).FloatValue > 0f || Page.Size.Width == (pdfArray2[2] as PdfNumber).FloatValue || Page.Size.Height == (pdfArray2[3] as PdfNumber).FloatValue)
				{
					rectangleF.X -= (pdfArray2[0] as PdfNumber).FloatValue;
					rectangleF.Y = (pdfArray2[3] as PdfNumber).FloatValue - (rectangleF.Y + rectangleF.Height);
				}
				else
				{
					rectangleF.Y = Page.Size.Height - (rectangleF.Y + rectangleF.Height);
				}
			}
			else
			{
				rectangleF.Y = height - (rectangleF.Y + rectangleF.Height);
			}
			PdfNumber[] array = new PdfNumber[4]
			{
				new PdfNumber(rectangleF.X),
				new PdfNumber(rectangleF.Y),
				new PdfNumber(rectangleF.X + rectangleF.Width),
				new PdfNumber(height - (height - (rectangleF.Y + rectangleF.Height)))
			};
			PdfDictionary pdfDictionary = base.Dictionary;
			if (!pdfDictionary.ContainsKey("Rect"))
			{
				pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			}
			PdfDictionary pdfDictionary2 = pdfDictionary;
			IPdfPrimitive[] list = array;
			pdfDictionary2.SetArray("Rect", list);
			NotifyPropertyChanged("Bounds");
			base.Changed = true;
		}
	}

	public PointF Location
	{
		get
		{
			return Bounds.Location;
		}
		set
		{
			Bounds = new RectangleF(value, Bounds.Size);
			NotifyPropertyChanged("Location");
		}
	}

	public SizeF Size
	{
		get
		{
			return Bounds.Size;
		}
		set
		{
			Bounds = new RectangleF(Bounds.Location, value);
			NotifyPropertyChanged("Size");
		}
	}

	internal PdfPen BorderPen => ObtainBorderPen();

	public PdfBorderStyle BorderStyle
	{
		get
		{
			return ObtainBorderStyle();
		}
		set
		{
			AssignBorderStyle(value);
			CreateBorderPen();
			m_isFieldPropertyChanged = true;
			NotifyPropertyChanged("BorderStyle");
		}
	}

	public PdfColor BorderColor
	{
		get
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfColor result = new PdfColor(Color.Transparent);
			if (widgetAnnotation.ContainsKey("MK"))
			{
				PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
				if (pdfDictionary.ContainsKey("BC"))
				{
					PdfArray array = pdfDictionary["BC"] as PdfArray;
					return CreateColor(array);
				}
			}
			return result;
		}
		set
		{
			((PdfField)this).Form.SetAppearanceDictionary = true;
			m_widget.WidgetAppearance.BorderColor = value;
			AssignBorderColor(value);
			base.FieldChanged = true;
			m_isFieldPropertyChanged = true;
			NotifyPropertyChanged("BorderColor");
		}
	}

	internal float[] DashPatern => ObtainDashPatern();

	public float BorderWidth
	{
		get
		{
			return ObtainBorderWidth();
		}
		set
		{
			m_widget.WidgetBorder.Width = value;
			AssignBorderWidth(value);
			m_isFieldPropertyChanged = true;
			NotifyPropertyChanged("BorderWidth");
		}
	}

	internal PdfStringFormat StringFormat => AssignStringFormat();

	internal PdfBrush BackBrush
	{
		get
		{
			return ObtainBackBrush();
		}
		set
		{
			AssignBackBrush(value);
			m_isFieldPropertyChanged = true;
		}
	}

	internal PdfBrush ForeBrush => ObtainForeBrush();

	internal PdfBrush ShadowBrush => ObtainShadowBrush();

	public PdfFont Font
	{
		get
		{
			PdfArray pdfArray = null;
			if (m_font != null && base.Dictionary.ContainsKey("Kids"))
			{
				pdfArray = PdfCrossTable.Dereference(base.Dictionary["Kids"]) as PdfArray;
				if (pdfArray != null && pdfArray.Count <= 1)
				{
					return m_font;
				}
				if (m_fontCollection.ContainsKey(DefaultIndex))
				{
					return m_fontCollection[DefaultIndex];
				}
			}
			else if (m_font != null)
			{
				return m_font;
			}
			PdfFont pdfFont = null;
			pdfFont = PdfDocument.DefaultFont;
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (widgetAnnotation != null && (widgetAnnotation.ContainsKey("DA") || base.Dictionary.ContainsKey("DA")))
			{
				PdfString pdfString = base.CrossTable.GetObject(widgetAnnotation["DA"]) as PdfString;
				if (pdfString == null)
				{
					pdfString = base.CrossTable.GetObject(base.Dictionary["DA"]) as PdfString;
				}
				string name = null;
				if (pdfString != null)
				{
					pdfFont = GetFont(pdfString.Value, out bool isCorrectFont, out name);
					if (!isCorrectFont && name != null)
					{
						string newValue = "/Helv";
						widgetAnnotation.SetProperty("DA", new PdfString(pdfString.Value.Replace(name, newValue)));
					}
				}
			}
			m_font = pdfFont;
			if (m_font != null && pdfArray != null && pdfArray.Count > 1)
			{
				m_fontCollection[DefaultIndex] = m_font;
			}
			return pdfFont;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Font");
			}
			if (m_font == value)
			{
				return;
			}
			m_font = value;
			if (((PdfField)this).Form != null)
			{
				((PdfField)this).Form.SetAppearanceDictionary = true;
			}
			PdfDefaultAppearance pdfDefaultAppearance = new PdfDefaultAppearance();
			pdfDefaultAppearance.FontSize = m_font.Size;
			pdfDefaultAppearance.ForeColor = ForeColor;
			PdfName name = ((PdfField)this).Form.Resources.GetName(m_font);
			pdfDefaultAppearance.FontName = name.Value;
			m_isFontModified = true;
			PdfDictionary pdfDictionary = null;
			if (base.CrossTable.Document is PdfLoadedDocument { RaisePdfFont: false } && base.Dictionary.ContainsKey("Kids"))
			{
				PdfArray pdfArray = base.Dictionary["Kids"] as PdfArray;
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if ((pdfArray[i] as PdfReferenceHolder).Object is PdfDictionary pdfDictionary2 && (base.Dictionary.ContainsKey("DA") || pdfDictionary2.ContainsKey("DA")))
					{
						pdfDictionary2["DA"] = new PdfString(pdfDefaultAppearance.ToString());
						if (base.Dictionary.ContainsKey("DA"))
						{
							base.Dictionary["DA"] = new PdfString(pdfDefaultAppearance.ToString());
						}
						m_fontCollection[i] = m_font;
					}
				}
			}
			else
			{
				pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
				if (pdfDictionary != null && (base.Dictionary.ContainsKey("DA") || pdfDictionary.ContainsKey("DA")))
				{
					pdfDictionary["DA"] = new PdfString(pdfDefaultAppearance.ToString());
					if (base.Dictionary.ContainsKey("DA"))
					{
						base.Dictionary["DA"] = new PdfString(pdfDefaultAppearance.ToString());
					}
				}
			}
			NotifyPropertyChanged("Font");
		}
	}

	public new int DefaultIndex
	{
		get
		{
			return base.DefaultIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new IndexOutOfRangeException("index");
			}
			base.DefaultIndex = value;
			NotifyPropertyChanged("DefaultIndex");
		}
	}

	internal PdfArray Kids => ObtainKids();

	public bool Visible => ObtainVisible();

	public PdfFormFieldVisibility Visibility
	{
		get
		{
			m_visibility = ObtainVisibility();
			return m_visibility;
		}
		set
		{
			m_visibility = value;
			SetVisibility();
			NotifyPropertyChanged("Visibility");
		}
	}

	public new int RotationAngle
	{
		get
		{
			return ObtainRotationAngle();
		}
		set
		{
			m_angle = value;
			int num = 360;
			if (m_angle >= 360)
			{
				m_angle %= num;
			}
			if (m_angle < 45)
			{
				m_angle = 0;
			}
			else if (m_angle >= 45 && m_angle < 135)
			{
				m_angle = 90;
			}
			else if (m_angle >= 135 && m_angle < 225)
			{
				m_angle = 180;
			}
			else if (m_angle >= 225 && m_angle < 315)
			{
				m_angle = 270;
			}
			int angle = m_angle;
			((PdfField)this).Form.SetAppearanceDictionary = true;
			m_widget.WidgetAppearance.RotationAngle = angle;
			SetRotationAngle(angle);
			NotifyPropertyChanged("RotationAngle");
			isRotationModified = true;
		}
	}

	internal PdfCheckBoxStyle Style
	{
		get
		{
			return ObtainStyle();
		}
		set
		{
			AssignStyle(value);
		}
	}

	internal PdfLoadedStyledField(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
	}

	private PdfFormFieldVisibility ObtainVisibility()
	{
		PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfFormFieldVisibility result = PdfFormFieldVisibility.Visible;
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		if (pdfDictionary != null)
		{
			if (pdfDictionary.ContainsKey("F"))
			{
				PdfNumber flagNumber = base.CrossTable.GetObject(pdfDictionary["F"]) as PdfNumber;
				PdfAnnotationFlags num = ObtainAnnotationFlags(flagNumber);
				int num2 = 3;
				if ((num & PdfAnnotationFlags.Hidden) == PdfAnnotationFlags.Hidden)
				{
					num2 = 0;
				}
				if ((num & PdfAnnotationFlags.NoView) == PdfAnnotationFlags.NoView)
				{
					num2 = 1;
				}
				if ((num & PdfAnnotationFlags.Print) != PdfAnnotationFlags.Print)
				{
					num2 &= 2;
				}
				switch (num2)
				{
				case 0:
					result = PdfFormFieldVisibility.Hidden;
					break;
				case 1:
					result = PdfFormFieldVisibility.HiddenPrintable;
					break;
				case 2:
					result = PdfFormFieldVisibility.VisibleNotPrintable;
					break;
				case 3:
					result = PdfFormFieldVisibility.Visible;
					break;
				}
			}
			else
			{
				result = PdfFormFieldVisibility.VisibleNotPrintable;
			}
		}
		return result;
	}

	private PdfAnnotationFlags ObtainAnnotationFlags(PdfNumber flagNumber)
	{
		PdfAnnotationFlags result = PdfAnnotationFlags.Default;
		if (flagNumber != null)
		{
			result = (PdfAnnotationFlags)flagNumber.IntValue;
		}
		return result;
	}

	private PdfFormFieldVisibility ObtainKidsVisibility(PdfDictionary dictionary)
	{
		PdfDictionary pdfDictionary = dictionary;
		PdfFormFieldVisibility result = PdfFormFieldVisibility.Visible;
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		if (pdfDictionary != null)
		{
			if (pdfDictionary.ContainsKey("F"))
			{
				PdfNumber flagNumber = base.CrossTable.GetObject(pdfDictionary["F"]) as PdfNumber;
				PdfAnnotationFlags num = ObtainAnnotationFlags(flagNumber);
				int num2 = 3;
				if ((num & PdfAnnotationFlags.Hidden) == PdfAnnotationFlags.Hidden)
				{
					num2 = 0;
				}
				if ((num & PdfAnnotationFlags.NoView) == PdfAnnotationFlags.NoView)
				{
					num2 = 1;
				}
				if ((num & PdfAnnotationFlags.Print) != PdfAnnotationFlags.Print)
				{
					num2 &= 2;
				}
				switch (num2)
				{
				case 0:
					result = PdfFormFieldVisibility.Hidden;
					break;
				case 1:
					result = PdfFormFieldVisibility.HiddenPrintable;
					break;
				case 2:
					result = PdfFormFieldVisibility.VisibleNotPrintable;
					break;
				case 3:
					result = PdfFormFieldVisibility.Visible;
					break;
				}
			}
			else
			{
				result = PdfFormFieldVisibility.VisibleNotPrintable;
			}
		}
		return result;
	}

	private void SetVisibility()
	{
		PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		if (base.Dictionary.ContainsKey("Kids"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Kids"]) is PdfArray { Count: >0 } pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					PdfDictionary widget = PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary;
					SetVisiblityToWidget(widget, kids: true);
				}
			}
		}
		else
		{
			SetVisiblityToWidget(pdfDictionary, kids: false);
		}
	}

	private void SetVisiblityToWidget(PdfDictionary widget, bool kids)
	{
		if (widget == null)
		{
			return;
		}
		PdfFormFieldVisibility pdfFormFieldVisibility = PdfFormFieldVisibility.Visible;
		pdfFormFieldVisibility = (kids ? ObtainKidsVisibility(widget) : ObtainVisibility());
		if (pdfFormFieldVisibility == m_visibility)
		{
			return;
		}
		if (widget.ContainsKey("F"))
		{
			widget.Remove("F");
		}
		switch (m_visibility)
		{
		case PdfFormFieldVisibility.Hidden:
			widget.Items.Add(new PdfName("F"), new PdfNumber(2));
			widget.Modify();
			break;
		case PdfFormFieldVisibility.HiddenPrintable:
			widget.Items.Add(new PdfName("F"), new PdfNumber(36));
			widget.Modify();
			break;
		case PdfFormFieldVisibility.Visible:
			widget.Items.Add(new PdfName("F"), new PdfNumber(4));
			if (widget.ContainsKey("DV"))
			{
				widget.Remove("DV");
			}
			if (widget.ContainsKey("MK"))
			{
				PdfReferenceHolder pdfReferenceHolder = widget["MK"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					if (pdfReferenceHolder.Object is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("BG"))
					{
						pdfDictionary.Remove("BG");
					}
				}
				else if (widget["MK"] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("BG"))
				{
					pdfDictionary2.Remove("BG");
				}
			}
			if (pdfFormFieldVisibility == PdfFormFieldVisibility.Hidden)
			{
				base.Changed = true;
			}
			widget.Modify();
			break;
		case PdfFormFieldVisibility.VisibleNotPrintable:
			break;
		}
	}

	protected void GetGraphicsProperties(out GraphicsProperties graphicsProperties, PdfLoadedFieldItem item)
	{
		if (item != null)
		{
			graphicsProperties = new GraphicsProperties(item);
		}
		else
		{
			graphicsProperties = new GraphicsProperties(this);
		}
	}

	private PdfBorderStyle CreateBorderStyle(PdfDictionary bs)
	{
		PdfBorderStyle result = PdfBorderStyle.Solid;
		if (bs != null && bs.ContainsKey("S"))
		{
			PdfName pdfName = base.CrossTable.GetObject(bs["S"]) as PdfName;
			if (pdfName != null)
			{
				switch (pdfName.Value.ToLower())
				{
				case "d":
					result = PdfBorderStyle.Dashed;
					break;
				case "b":
					result = PdfBorderStyle.Beveled;
					break;
				case "i":
					result = PdfBorderStyle.Inset;
					break;
				case "u":
					result = PdfBorderStyle.Underline;
					break;
				}
			}
		}
		return result;
	}

	private void AssignBorderStyle(PdfBorderStyle borderStyle)
	{
		string value = "";
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation.ContainsKey("BS"))
		{
			base.CrossTable.GetObject(widgetAnnotation["BS"]);
			switch (borderStyle)
			{
			case PdfBorderStyle.Dashed:
				value = "D";
				break;
			case PdfBorderStyle.Beveled:
				value = "B";
				break;
			case PdfBorderStyle.Inset:
				value = "I";
				break;
			case PdfBorderStyle.Underline:
				value = "U";
				break;
			case PdfBorderStyle.Solid:
				value = "S";
				break;
			}
			(widgetAnnotation["BS"] as PdfDictionary)["S"] = new PdfName(value);
			Widget.WidgetBorder.Style = borderStyle;
		}
	}

	internal PdfPen AssignBorderColor(PdfColor borderColor)
	{
		PdfPen pdfPen = null;
		if (base.Dictionary.ContainsKey("Kids"))
		{
			if (base.CrossTable.GetObject(base.Dictionary["Kids"]) is PdfArray pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary)
					{
						if (pdfDictionary.ContainsKey("MK"))
						{
							if (base.CrossTable.GetObject(pdfDictionary["MK"]) is PdfDictionary pdfDictionary2)
							{
								PdfArray pdfArray2 = borderColor.ToArray();
								if (borderColor.A == 0)
								{
									pdfDictionary2.Remove("BC");
								}
								else
								{
									pdfDictionary2["BC"] = pdfArray2;
									pdfPen = new PdfPen(CreateColor(pdfArray2));
								}
							}
						}
						else
						{
							PdfDictionary pdfDictionary3 = new PdfDictionary();
							PdfArray pdfArray3 = borderColor.ToArray();
							if (borderColor.A == 0)
							{
								pdfDictionary3["BC"] = new PdfArray(new float[0]);
								pdfPen = new PdfPen(borderColor);
							}
							else
							{
								pdfDictionary3["BC"] = pdfArray3;
								pdfPen = new PdfPen(CreateColor(pdfArray3));
							}
							pdfDictionary["MK"] = pdfDictionary3;
						}
					}
					PdfBorderStyle borderStyle = BorderStyle;
					float borderWidth = BorderWidth;
					if (pdfPen == null)
					{
						continue;
					}
					pdfPen.Width = borderWidth;
					if (borderStyle == PdfBorderStyle.Dashed)
					{
						float[] dashPatern = DashPatern;
						pdfPen.DashStyle = PdfDashStyle.Custom;
						if (dashPatern != null)
						{
							pdfPen.DashPattern = dashPatern;
							continue;
						}
						pdfPen.DashPattern = new float[1] { 3f / borderWidth };
					}
				}
			}
		}
		else
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (widgetAnnotation != null)
			{
				if (widgetAnnotation.ContainsKey("MK"))
				{
					if (base.CrossTable.GetObject(widgetAnnotation["MK"]) is PdfDictionary pdfDictionary4)
					{
						PdfArray pdfArray4 = borderColor.ToArray();
						if (borderColor.A == 0)
						{
							pdfDictionary4["BC"] = new PdfArray(new float[0]);
							pdfPen = new PdfPen(borderColor);
						}
						else
						{
							pdfDictionary4["BC"] = pdfArray4;
							pdfPen = new PdfPen(CreateColor(pdfArray4));
						}
					}
				}
				else
				{
					PdfDictionary pdfDictionary5 = new PdfDictionary();
					PdfArray pdfArray5 = borderColor.ToArray();
					if (borderColor.A == 0)
					{
						pdfDictionary5["BC"] = new PdfArray(new float[0]);
						pdfPen = new PdfPen(borderColor);
					}
					else
					{
						pdfDictionary5["BC"] = pdfArray5;
						pdfPen = new PdfPen(CreateColor(pdfArray5));
					}
					widgetAnnotation["MK"] = pdfDictionary5;
				}
			}
			PdfBorderStyle borderStyle2 = BorderStyle;
			float borderWidth2 = BorderWidth;
			if (pdfPen != null)
			{
				pdfPen.Width = borderWidth2;
				if (borderStyle2 == PdfBorderStyle.Dashed)
				{
					float[] dashPatern2 = DashPatern;
					pdfPen.DashStyle = PdfDashStyle.Custom;
					if (dashPatern2 != null)
					{
						pdfPen.DashPattern = dashPatern2;
					}
					else
					{
						pdfPen.DashPattern = new float[1] { 3f / borderWidth2 };
					}
				}
			}
		}
		return pdfPen;
	}

	internal void AssignBackColor(PdfColor value)
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation == null)
		{
			return;
		}
		if (widgetAnnotation.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
			if (value.A == 0)
			{
				pdfDictionary.Remove("BG");
				widgetAnnotation["MK"] = pdfDictionary;
			}
			else
			{
				PdfArray value2 = value.ToArray();
				pdfDictionary["BG"] = value2;
			}
		}
		else if (value.A != 0)
		{
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			PdfArray value3 = value.ToArray();
			pdfDictionary2["BG"] = value3;
			widgetAnnotation["MK"] = pdfDictionary2;
		}
		((PdfField)this).Form.SetAppearanceDictionary = true;
	}

	private RectangleF GetBounds(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfArray pdfArray = null;
		if (dictionary.ContainsKey("Kids"))
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(dictionary, crossTable);
			if (widgetAnnotation.ContainsKey("Rect"))
			{
				pdfArray = crossTable.GetObject(widgetAnnotation["Rect"]) as PdfArray;
			}
		}
		else
		{
			if (dictionary.ContainsKey("Parent") && PdfCrossTable.Dereference(dictionary["Parent"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Kids"))
			{
				PdfName pdfName = pdfDictionary["FT"] as PdfName;
				if (pdfDictionary.ContainsKey("FT") && pdfName != null && pdfName.Value == "Btn")
				{
					PdfDictionary widgetAnnotation2 = GetWidgetAnnotation(pdfDictionary, crossTable);
					if (widgetAnnotation2 != null && widgetAnnotation2.ContainsKey("Rect"))
					{
						pdfArray = crossTable.GetObject(widgetAnnotation2["Rect"]) as PdfArray;
					}
				}
			}
			if (pdfArray == null && dictionary.ContainsKey("Rect"))
			{
				pdfArray = crossTable.GetObject(dictionary["Rect"]) as PdfArray;
			}
		}
		RectangleF result;
		if (pdfArray != null)
		{
			result = pdfArray.ToRectangle();
			result.Height = (float)Math.Round(result.Height, 3);
			if ((PdfCrossTable.Dereference(pdfArray[1]) as PdfNumber).FloatValue < 0f)
			{
				result.Y = (PdfCrossTable.Dereference(pdfArray[1]) as PdfNumber).FloatValue;
				if ((PdfCrossTable.Dereference(pdfArray[1]) as PdfNumber).FloatValue > (PdfCrossTable.Dereference(pdfArray[3]) as PdfNumber).FloatValue)
				{
					result.Y -= result.Height;
				}
			}
		}
		else
		{
			result = default(RectangleF);
		}
		return result;
	}

	private string GetHighLightString(PdfHighlightMode mode)
	{
		string result = null;
		switch (mode)
		{
		case PdfHighlightMode.NoHighlighting:
			result = "N";
			break;
		case PdfHighlightMode.Invert:
			result = "I";
			break;
		case PdfHighlightMode.Outline:
			result = "O";
			break;
		case PdfHighlightMode.Push:
			result = "P";
			break;
		}
		return result;
	}

	private PdfColor CreateColor(PdfArray array)
	{
		int count = array.Count;
		PdfColor result = PdfColor.Empty;
		float[] array2 = new float[array.Count];
		int i = 0;
		for (int count2 = array.Count; i < count2; i++)
		{
			PdfNumber pdfNumber = base.CrossTable.GetObject(array[i]) as PdfNumber;
			array2[i] = pdfNumber.FloatValue;
		}
		switch (count)
		{
		case 1:
			result = ((!((double)array2[0] > 0.0) || !((double)array2[0] <= 1.0)) ? new PdfColor((int)(byte)array2[0]) : new PdfColor(array2[0]));
			break;
		case 3:
			result = (((!((double)array2[0] > 0.0) || !((double)array2[0] <= 1.0)) && (!((double)array2[1] > 0.0) || !((double)array2[1] <= 1.0)) && (!((double)array2[2] > 0.0) || !((double)array2[2] <= 1.0))) ? new PdfColor((byte)array2[0], (byte)array2[1], (byte)array2[2]) : new PdfColor(array2[0], array2[1], array2[2]));
			break;
		case 4:
			result = (((!((double)array2[0] > 0.0) || !((double)array2[0] <= 1.0)) && (!((double)array2[1] > 0.0) || !((double)array2[1] <= 1.0)) && (!((double)array2[2] > 0.0) || !((double)array2[2] <= 1.0)) && (!((double)array2[3] > 0.0) || !((double)array2[3] <= 1.0))) ? new PdfColor((byte)array2[0], (byte)array2[1], (byte)array2[2], (byte)array2[3]) : new PdfColor(array2[0], array2[1], array2[2], array2[3]));
			break;
		}
		return result;
	}

	internal PdfColor GetForeColour(string defaultAppearance)
	{
		PdfColor result = new PdfColor(0, 0, 0);
		if (defaultAppearance == null || defaultAppearance == string.Empty)
		{
			result = new PdfColor(0, 0, 0);
		}
		else
		{
			PdfReader pdfReader = new PdfReader(new MemoryStream(Encoding.UTF8.GetBytes(defaultAppearance)));
			pdfReader.Position = 0L;
			bool flag = false;
			Stack<string> stack = new Stack<string>();
			string nextToken = pdfReader.GetNextToken();
			if (nextToken == "/")
			{
				flag = true;
			}
			while (nextToken != null && nextToken != string.Empty)
			{
				if (flag)
				{
					nextToken = pdfReader.GetNextToken();
				}
				flag = true;
				switch (nextToken)
				{
				case "g":
				{
					string text3 = stack.Pop();
					float gray = ParseFloatColour(text3);
					result = new PdfColor(gray);
					break;
				}
				case "rg":
				{
					string text2 = stack.Pop();
					byte blue = (byte)(ParseFloatColour(text2) * 255f);
					text2 = stack.Pop();
					byte green = (byte)(ParseFloatColour(text2) * 255f);
					text2 = stack.Pop();
					byte red = (byte)(ParseFloatColour(text2) * 255f);
					result = new PdfColor(red, green, blue);
					break;
				}
				case "k":
				{
					string text = stack.Pop();
					float black = ParseFloatColour(text);
					text = stack.Pop();
					float yellow = ParseFloatColour(text);
					text = stack.Pop();
					float magenta = ParseFloatColour(text);
					text = stack.Pop();
					float cyan = ParseFloatColour(text);
					result = new PdfColor(cyan, magenta, yellow, black);
					break;
				}
				default:
					stack.Push(nextToken);
					break;
				}
			}
		}
		return result;
	}

	private float ParseFloatColour(string text)
	{
		double result;
		try
		{
			result = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
		}
		catch
		{
			if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				result = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
			}
		}
		return (float)result;
	}

	internal string FontName(string fontString, out float height)
	{
		if (fontString.Contains("#2C"))
		{
			StringBuilder stringBuilder = new StringBuilder(fontString);
			stringBuilder.Replace("#2C", ",");
			fontString = stringBuilder.ToString();
		}
		PdfReader pdfReader = new PdfReader(new MemoryStream(Encoding.UTF8.GetBytes(fontString)));
		pdfReader.Position = 0L;
		string text = pdfReader.GetNextToken();
		string nextToken = pdfReader.GetNextToken();
		string result = null;
		height = 0f;
		while (nextToken != null && nextToken != string.Empty)
		{
			result = text;
			text = nextToken;
			nextToken = pdfReader.GetNextToken();
			if (!(nextToken == "Tf"))
			{
				continue;
			}
			try
			{
				height = (float)double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
			}
			catch (Exception)
			{
				if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					height = (float)double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
				}
				else
				{
					height = (float)result2;
				}
			}
			break;
		}
		return result;
	}

	internal bool CheckFieldFlagValue(IPdfPrimitive kid)
	{
		if (kid != null)
		{
			int num = 0;
			if (PdfCrossTable.Dereference(kid) is PdfDictionary pdfDictionary)
			{
				if (PdfCrossTable.Dereference(pdfDictionary["F"]) is PdfNumber pdfNumber)
				{
					num = pdfNumber.IntValue;
				}
				if (num == 6)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private PdfFontMetrics CreateFont(PdfDictionary fontDictionary, float height, PdfName baseFont)
	{
		PdfFontMetrics pdfFontMetrics = new PdfFontMetrics();
		if (fontDictionary.ContainsKey("FontDescriptor"))
		{
			PdfDictionary pdfDictionary = null;
			PdfReferenceHolder pdfReferenceHolder = fontDictionary["FontDescriptor"] as PdfReferenceHolder;
			pdfDictionary = ((!(pdfReferenceHolder != null)) ? (fontDictionary["FontDescriptor"] as PdfDictionary) : (pdfReferenceHolder.Object as PdfDictionary));
			pdfFontMetrics.Ascent = (pdfDictionary["Ascent"] as PdfNumber).IntValue;
			pdfFontMetrics.Descent = (pdfDictionary["Descent"] as PdfNumber).IntValue;
			pdfFontMetrics.Size = height;
			pdfFontMetrics.Height = pdfFontMetrics.Ascent - pdfFontMetrics.Descent;
			pdfFontMetrics.PostScriptName = baseFont.Value;
		}
		PdfArray pdfArray = null;
		if (fontDictionary.ContainsKey("Widths"))
		{
			if (fontDictionary["Widths"] is PdfReferenceHolder)
			{
				pdfArray = (new PdfReferenceHolder(fontDictionary["Widths"]).Object as PdfReferenceHolder).Object as PdfArray;
				float[] array = new float[pdfArray.Count];
				for (int i = 0; i < pdfArray.Count; i++)
				{
					array[i] = (pdfArray[i] as PdfNumber).IntValue;
				}
				pdfFontMetrics.WidthTable = new StandardWidthTable(array);
			}
			else
			{
				pdfArray = fontDictionary["Widths"] as PdfArray;
				float[] array2 = new float[pdfArray.Count];
				for (int j = 0; j < pdfArray.Count; j++)
				{
					array2[j] = (pdfArray[j] as PdfNumber).IntValue;
				}
				pdfFontMetrics.WidthTable = new StandardWidthTable(array2);
			}
		}
		pdfFontMetrics.Name = baseFont.Value;
		return pdfFontMetrics;
	}

	private PdfFont GetFontByName(string name, float height)
	{
		PdfFont result = null;
		switch (name)
		{
		case "CoBO":
			result = new PdfStandardFont(PdfFontFamily.Courier, height, PdfFontStyle.Bold | PdfFontStyle.Italic);
			break;
		case "CoBo":
			result = new PdfStandardFont(PdfFontFamily.Courier, height, PdfFontStyle.Bold);
			break;
		case "CoOb":
			result = new PdfStandardFont(PdfFontFamily.Courier, height, PdfFontStyle.Italic);
			break;
		case "Courier":
		case "Cour":
			result = new PdfStandardFont(PdfFontFamily.Courier, height, PdfFontStyle.Regular);
			break;
		case "HeBO":
			result = new PdfStandardFont(PdfFontFamily.Helvetica, height, PdfFontStyle.Bold | PdfFontStyle.Italic);
			break;
		case "HeBo":
			result = new PdfStandardFont(PdfFontFamily.Helvetica, height, PdfFontStyle.Bold);
			break;
		case "HeOb":
			result = new PdfStandardFont(PdfFontFamily.Helvetica, height, PdfFontStyle.Italic);
			break;
		case "Helv":
			result = new PdfStandardFont(PdfFontFamily.Helvetica, height, PdfFontStyle.Regular);
			break;
		case "Symb":
			result = new PdfStandardFont(PdfFontFamily.Symbol, height);
			break;
		case "TiBI":
			result = new PdfStandardFont(PdfFontFamily.TimesRoman, height, PdfFontStyle.Bold | PdfFontStyle.Italic);
			break;
		case "TiBo":
			result = new PdfStandardFont(PdfFontFamily.TimesRoman, height, PdfFontStyle.Bold);
			break;
		case "TiIt":
			result = new PdfStandardFont(PdfFontFamily.TimesRoman, height, PdfFontStyle.Italic);
			break;
		case "TiRo":
			result = new PdfStandardFont(PdfFontFamily.TimesRoman, height, PdfFontStyle.Regular);
			break;
		case "ZaDb":
			result = new PdfStandardFont(PdfFontFamily.ZapfDingbats, height);
			break;
		}
		return result;
	}

	private bool CheckFontStyle(string fontFamilyString, out PdfFontStyle style)
	{
		bool result = false;
		style = PdfFontStyle.Regular;
		if (fontFamilyString != null)
		{
			if (fontFamilyString.Contains("BoldItalic") || fontFamilyString.Contains("BoldOblique") || fontFamilyString.Contains("BoldItalicMT"))
			{
				style = PdfFontStyle.Bold | PdfFontStyle.Italic;
				result = true;
			}
			else if (fontFamilyString.Contains("Italic") || fontFamilyString.Contains("Oblique") || fontFamilyString.Contains("ItalicMT") || fontFamilyString.Contains("It"))
			{
				style = PdfFontStyle.Italic;
				result = true;
			}
			else if (fontFamilyString.Contains("Bold") || fontFamilyString.Contains("BoldMT"))
			{
				style = PdfFontStyle.Bold;
				result = true;
			}
		}
		return result;
	}

	private PdfFontStyle GetFontStyle(string fontFamilyString)
	{
		int num = fontFamilyString.IndexOf("-");
		if (num < 0)
		{
			num = fontFamilyString.IndexOf(",");
		}
		PdfFontStyle result = PdfFontStyle.Regular;
		if (num >= 0)
		{
			switch (fontFamilyString.Substring(num + 1, fontFamilyString.Length - num - 1))
			{
			case "Italic":
			case "Oblique":
			case "ItalicMT":
			case "It":
				result = PdfFontStyle.Italic;
				break;
			case "BoldMT":
			case "Bold":
				result = PdfFontStyle.Bold;
				break;
			case "BoldItalic":
			case "BoldOblique":
			case "BoldItalicMT":
				result = PdfFontStyle.Bold | PdfFontStyle.Italic;
				break;
			}
		}
		return result;
	}

	internal string GetFontName(string fontFamilyString)
	{
		return fontFamilyString.Split('-')[0];
	}

	private PdfFontFamily GetFontFamily(string fontFamilyString, out string standardName)
	{
		int num = fontFamilyString.IndexOf("-");
		PdfFontFamily result = PdfFontFamily.Helvetica;
		standardName = fontFamilyString;
		if (num >= 0)
		{
			standardName = fontFamilyString.Substring(0, num);
		}
		if (standardName == "Times")
		{
			result = PdfFontFamily.TimesRoman;
			standardName = null;
		}
		else
		{
			string[] names = Enum.GetNames(typeof(PdfFontFamily));
			foreach (string text in names)
			{
				try
				{
					if (text.Contains(standardName))
					{
						result = (PdfFontFamily)Enum.Parse(typeof(PdfFontFamily), standardName, ignoreCase: true);
						standardName = null;
						break;
					}
				}
				catch (ArgumentException)
				{
					return PdfFontFamily.Helvetica;
				}
			}
		}
		return result;
	}

	internal PdfColor GetBackColor()
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfColor result = default(PdfColor);
		if (widgetAnnotation.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("BG"))
			{
				PdfArray array = pdfDictionary["BG"] as PdfArray;
				return CreateColor(array);
			}
		}
		return result;
	}

	private PdfBorderStyle ObtainBorderStyle()
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfBorderStyle result = PdfBorderStyle.Solid;
		if (widgetAnnotation.ContainsKey("BS"))
		{
			PdfDictionary bs = base.CrossTable.GetObject(widgetAnnotation["BS"]) as PdfDictionary;
			result = CreateBorderStyle(bs);
		}
		return result;
	}

	private float[] ObtainDashPatern()
	{
		float[] array = null;
		if (BorderStyle == PdfBorderStyle.Dashed)
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			if (widgetAnnotation.ContainsKey("D"))
			{
				PdfArray pdfArray = base.CrossTable.GetObject(widgetAnnotation["D"]) as PdfArray;
				if (array.Length == 2)
				{
					array = new float[2];
					PdfNumber pdfNumber = pdfArray[0] as PdfNumber;
					array[0] = pdfNumber.IntValue;
					pdfNumber = pdfArray[1] as PdfNumber;
					array[1] = pdfNumber.IntValue;
				}
				else
				{
					array = new float[1];
					PdfNumber pdfNumber2 = pdfArray[0] as PdfNumber;
					array[0] = pdfNumber2.IntValue;
				}
			}
		}
		return array;
	}

	private float ObtainBorderWidth()
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		float result = 0f;
		PdfName pdfName = base.CrossTable.GetObject(widgetAnnotation["FT"]) as PdfName;
		if (pdfName == null && base.Dictionary.ContainsKey("FT"))
		{
			pdfName = base.CrossTable.GetObject(base.Dictionary["FT"]) as PdfName;
		}
		if (widgetAnnotation.ContainsKey("BS"))
		{
			result = 1f;
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["BS"]) as PdfDictionary;
			PdfNumber pdfNumber = null;
			if (pdfDictionary != null)
			{
				pdfNumber = base.CrossTable.GetObject(pdfDictionary["W"]) as PdfNumber;
			}
			if (pdfNumber != null)
			{
				result = pdfNumber.FloatValue;
			}
		}
		else if (pdfName != null && pdfName.Value == "Btn")
		{
			result = 1f;
		}
		return result;
	}

	private void AssignBorderWidth(float width)
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation.ContainsKey("BS"))
		{
			(PdfCrossTable.Dereference(widgetAnnotation["BS"]) as PdfDictionary)["W"] = new PdfNumber(width);
		}
		else
		{
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary["W"] = new PdfNumber(width);
			widgetAnnotation["BS"] = pdfDictionary;
		}
		CreateBorderPen();
	}

	private PdfStringFormat AssignStringFormat()
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
		pdfStringFormat.LineAlignment = (((Flags & FieldFlags.Multiline) <= FieldFlags.Default) ? PdfVerticalAlignment.Middle : PdfVerticalAlignment.Top);
		PdfNumber pdfNumber = null;
		if (widgetAnnotation != null && widgetAnnotation.ContainsKey("Q"))
		{
			pdfNumber = base.CrossTable.GetObject(widgetAnnotation["Q"]) as PdfNumber;
		}
		else if (base.Dictionary.ContainsKey("Q"))
		{
			pdfNumber = base.CrossTable.GetObject(base.Dictionary["Q"]) as PdfNumber;
		}
		if (pdfNumber != null && (pdfNumber.IsInteger || pdfNumber.IsLong))
		{
			if (pdfNumber.IsInteger)
			{
				pdfStringFormat.Alignment = (PdfTextAlignment)pdfNumber.IntValue;
			}
			else
			{
				pdfStringFormat.Alignment = (PdfTextAlignment)pdfNumber.LongValue;
			}
		}
		if (base.ComplexScript || (base.Form != null && base.Form.ComplexScript))
		{
			pdfStringFormat.ComplexScript = true;
		}
		return pdfStringFormat;
	}

	private PdfBrush ObtainBackBrush()
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfBrush result = null;
		if (widgetAnnotation != null && widgetAnnotation.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("BG") && base.CrossTable.GetObject(pdfDictionary["BG"]) is PdfArray array)
			{
				result = new PdfSolidBrush(CreateColor(array));
			}
		}
		return result;
	}

	private void AssignBackBrush(PdfBrush brush)
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation != null && brush is PdfSolidBrush)
		{
			PdfDictionary pdfDictionary = (PdfDictionary)(widgetAnnotation.ContainsKey("MK") ? (base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary) : (widgetAnnotation["MK"] = new PdfDictionary()));
			PdfArray value = (brush as PdfSolidBrush).Color.ToArray();
			pdfDictionary["BG"] = value;
		}
	}

	private PdfFont UpdateFontEncoding(PdfFont font, PdfDictionary fontDictionary)
	{
		PdfDictionary pdfDictionary = font.FontInternal as PdfDictionary;
		if (fontDictionary.Items.ContainsKey(new PdfName("Encoding")))
		{
			PdfName key = new PdfName("Encoding");
			PdfReferenceHolder pdfReferenceHolder = fontDictionary.Items[new PdfName("Encoding")] as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				if (pdfReferenceHolder.Object is PdfDictionary value)
				{
					if (pdfDictionary.Items.ContainsKey(new PdfName("Encoding")))
					{
						pdfDictionary.Items.Remove(new PdfName("Encoding"));
					}
					pdfDictionary.Items.Add(key, value);
				}
			}
			else if (fontDictionary.Items[new PdfName("Encoding")] is PdfDictionary value2)
			{
				if (pdfDictionary.Items.ContainsKey(new PdfName("Encoding")))
				{
					pdfDictionary.Items.Remove(new PdfName("Encoding"));
				}
				pdfDictionary.Items.Add(key, value2);
			}
		}
		return font;
	}

	private PdfBrush ObtainForeBrush()
	{
		PdfBrush result = PdfBrushes.Black;
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA") && base.CrossTable.GetObject(widgetAnnotation["DA"]) is PdfString pdfString)
		{
			result = new PdfSolidBrush(GetForeColour(pdfString.Value));
		}
		return result;
	}

	private PdfBrush ObtainShadowBrush()
	{
		PdfBrush result = PdfBrushes.White;
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation != null && widgetAnnotation.ContainsKey("DA"))
		{
			base.CrossTable.GetObject(widgetAnnotation["DA"]);
			PdfColor color = new PdfColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);
			if (BackBrush is PdfSolidBrush pdfSolidBrush)
			{
				color = pdfSolidBrush.Color;
			}
			color.R = (byte)((color.R - 64 >= 0) ? ((uint)(color.R - 64)) : 0u);
			color.G = (byte)((color.G - 64 >= 0) ? ((uint)(color.G - 64)) : 0u);
			color.B = (byte)((color.B - 64 >= 0) ? ((uint)(color.B - 64)) : 0u);
			result = new PdfSolidBrush(color);
		}
		return result;
	}

	internal override void Draw()
	{
	}

	internal override PdfLoadedFieldItem CreateLoadedItem(PdfDictionary dictionary)
	{
		return null;
	}

	internal override void BeginSave()
	{
		base.BeginSave();
	}

	internal virtual float GetFontHeight(PdfFontFamily family)
	{
		return 0f;
	}

	internal PdfPen ObtainBorderPen()
	{
		PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfPen pdfPen = null;
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary2 = base.CrossTable.GetObject(pdfDictionary["MK"]) as PdfDictionary;
			if (pdfDictionary2.ContainsKey("BC"))
			{
				PdfArray array = base.CrossTable.GetObject(pdfDictionary2["BC"]) as PdfArray;
				pdfPen = new PdfPen(CreateColor(array));
			}
		}
		PdfBorderStyle borderStyle = BorderStyle;
		float borderWidth = BorderWidth;
		if (pdfPen != null)
		{
			pdfPen.Width = borderWidth;
			if (borderStyle == PdfBorderStyle.Dashed)
			{
				float[] dashPatern = DashPatern;
				pdfPen.DashStyle = PdfDashStyle.Custom;
				if (dashPatern != null)
				{
					pdfPen.DashPattern = dashPatern;
				}
				else if (borderWidth > 0f)
				{
					pdfPen.DashPattern = new float[1] { 3f / borderWidth };
				}
			}
		}
		return pdfPen;
	}

	private PdfArray ObtainKids()
	{
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("Kids"))
		{
			pdfArray = base.CrossTable.GetObject(base.Dictionary["Kids"]) as PdfArray;
			if (pdfArray != null && pdfArray.Count > 0)
			{
				return RecursiveCall(pdfArray);
			}
		}
		return pdfArray;
	}

	private PdfArray ObtainKids(PdfDictionary dictionary)
	{
		PdfArray result = null;
		if (dictionary.ContainsKey("Kids"))
		{
			result = base.CrossTable.GetObject(dictionary["Kids"]) as PdfArray;
		}
		return result;
	}

	private PdfArray RecursiveCall(PdfArray kids)
	{
		if (kids != null && kids.Count > 0)
		{
			for (int i = 0; i < kids.Count; i++)
			{
				if (PdfCrossTable.Dereference(kids[i]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("T") && pdfDictionary.ContainsKey("Kids"))
				{
					PdfArray pdfArray = ObtainKids(pdfDictionary);
					if (pdfArray != null)
					{
						return RecursiveCall(pdfArray);
					}
				}
			}
		}
		return kids;
	}

	private bool ObtainVisible()
	{
		PdfDictionary pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (pdfDictionary == null)
		{
			pdfDictionary = base.Dictionary;
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("F") && (base.CrossTable.GetObject(pdfDictionary["F"]) as PdfNumber).IntValue == 2)
		{
			return false;
		}
		return true;
	}

	private void CreateBorderPen()
	{
		float width = m_widget.WidgetBorder.Width;
		m_borderPen = new PdfPen(m_widget.WidgetAppearance.BorderColor, width);
		if (Widget.WidgetBorder.Style == PdfBorderStyle.Dashed)
		{
			m_borderPen.DashStyle = PdfDashStyle.Custom;
			m_borderPen.DashPattern = new float[1] { 3f / width };
		}
	}

	protected override void DefineDefaultAppearance()
	{
		_ = m_widget;
		if (base.Form != null && m_font != null)
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			PdfName name = base.Form.Resources.GetName(m_font);
			base.Form.Resources.Add(m_font, name);
			base.Form.NeedAppearances = true;
			widgetAnnotation["DA"] = new PdfString(new PdfDefaultAppearance
			{
				FontName = name.Value,
				FontSize = m_font.Size,
				ForeColor = ForeColor
			}.ToString());
		}
	}

	private int ObtainRotationAngle()
	{
		int result = 0;
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation.ContainsKey("MK"))
		{
			if (base.CrossTable.GetObject(widgetAnnotation["MK"]) is PdfDictionary pdfDictionary)
			{
				result = (pdfDictionary.ContainsKey("R") ? (pdfDictionary["R"] as PdfNumber).IntValue : 0);
			}
		}
		else if (base.Dictionary.ContainsKey("Kids"))
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(base.Dictionary["Kids"]) as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("MK") && PdfCrossTable.Dereference(pdfDictionary2["MK"]) is PdfDictionary pdfDictionary3)
				{
					return pdfDictionary3.ContainsKey("R") ? (pdfDictionary3["R"] as PdfNumber).IntValue : 0;
				}
			}
		}
		return result;
	}

	internal void SetRotationAngle(int angle)
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation.ContainsKey("MK"))
		{
			PdfDictionary pdfDictionary = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("R"))
			{
				pdfDictionary["R"] = new PdfNumber(angle);
			}
			else if (!pdfDictionary.ContainsKey("R"))
			{
				pdfDictionary.SetProperty("R", new PdfNumber(angle));
			}
		}
	}

	internal PdfField Clone(PdfDictionary dictionary, PdfPage page)
	{
		PdfCrossTable crossTable = page.Section.ParentDocument.CrossTable;
		PdfLoadedStyledField pdfLoadedStyledField = new PdfLoadedStyledField(dictionary, crossTable);
		pdfLoadedStyledField.Page = page;
		pdfLoadedStyledField.Widget.Dictionary = Widget.Dictionary.Clone(crossTable) as PdfDictionary;
		return pdfLoadedStyledField;
	}

	internal PdfCheckBoxStyle ObtainStyle()
	{
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		PdfCheckBoxStyle result = PdfCheckBoxStyle.Check;
		if (widgetAnnotation.ContainsKey("MK"))
		{
			PdfDictionary bs = base.CrossTable.GetObject(widgetAnnotation["MK"]) as PdfDictionary;
			result = CreateStyle(bs);
		}
		return result;
	}

	private PdfCheckBoxStyle CreateStyle(PdfDictionary bs)
	{
		PdfCheckBoxStyle result = PdfCheckBoxStyle.Check;
		if (bs.ContainsKey("CA") && base.CrossTable.GetObject(bs["CA"]) is PdfString pdfString)
		{
			switch (pdfString.Value.ToLower())
			{
			case "4":
				result = PdfCheckBoxStyle.Check;
				break;
			case "l":
				result = PdfCheckBoxStyle.Circle;
				break;
			case "8":
				result = PdfCheckBoxStyle.Cross;
				break;
			case "u":
				result = PdfCheckBoxStyle.Diamond;
				break;
			case "n":
				result = PdfCheckBoxStyle.Square;
				break;
			case "h":
				result = PdfCheckBoxStyle.Star;
				break;
			}
		}
		return result;
	}

	internal void AssignStyle(PdfCheckBoxStyle checkStyle)
	{
		string text = "";
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation.ContainsKey("MK"))
		{
			base.CrossTable.GetObject(widgetAnnotation["MK"]);
			switch (checkStyle)
			{
			case PdfCheckBoxStyle.Check:
				text = "4";
				break;
			case PdfCheckBoxStyle.Circle:
				text = "l";
				break;
			case PdfCheckBoxStyle.Cross:
				text = "8";
				break;
			case PdfCheckBoxStyle.Diamond:
				text = "u";
				break;
			case PdfCheckBoxStyle.Square:
				text = "n";
				break;
			case PdfCheckBoxStyle.Star:
				text = "H";
				break;
			}
			(widgetAnnotation["MK"] as PdfDictionary)["CA"] = new PdfString(text);
			Widget.WidgetAppearance.NormalCaption = text;
		}
	}

	protected string StyleToString(PdfCheckBoxStyle style)
	{
		return style switch
		{
			PdfCheckBoxStyle.Circle => "l", 
			PdfCheckBoxStyle.Cross => "8", 
			PdfCheckBoxStyle.Diamond => "u", 
			PdfCheckBoxStyle.Square => "n", 
			PdfCheckBoxStyle.Star => "H", 
			_ => "4", 
		};
	}

	internal PdfLoadedStyledField Clone()
	{
		PdfLoadedStyledField result = null;
		if (this is PdfLoadedCheckBoxField)
		{
			return (this as PdfLoadedCheckBoxField).Clone();
		}
		if (this is PdfLoadedTextBoxField)
		{
			return (this as PdfLoadedTextBoxField).Clone();
		}
		if (this is PdfLoadedComboBoxField)
		{
			return (this as PdfLoadedComboBoxField).Clone();
		}
		if (this is PdfLoadedRadioButtonListField)
		{
			return (this as PdfLoadedRadioButtonListField).Clone();
		}
		if (this is PdfLoadedListBoxField)
		{
			return (this as PdfLoadedListBoxField).Clone();
		}
		if (this is PdfLoadedButtonField)
		{
			return (this as PdfLoadedButtonField).Clone();
		}
		return result;
	}

	private PdfFont GetFont(string fontString, out bool isCorrectFont, out string name)
	{
		float height = 0f;
		isCorrectFont = true;
		string text = null;
		name = FontName(fontString, out height);
		PdfFont pdfFont = new PdfStandardFont((PdfStandardFont)PdfDocument.DefaultFont, height);
		PdfFontStyle textFontStyle = PdfFontStyle.Regular;
		PdfDictionary pdfDictionary = null;
		if (base.Form.Resources != null && base.Form.Resources.ContainsKey("Font"))
		{
			pdfDictionary = PdfCrossTable.Dereference(base.Form.Resources["Font"]) as PdfDictionary;
		}
		bool isUnicode = false;
		if (base.Dictionary.ContainsKey("V"))
		{
			PdfString pdfString = PdfCrossTable.Dereference(base.Dictionary["V"]) as PdfString;
			if (pdfString != null && pdfString.Value != null)
			{
				isUnicode = PdfString.IsUnicode(pdfString.Value);
			}
			if (base.Dictionary.ContainsKey("FT"))
			{
				PdfName pdfName = base.Dictionary["FT"] as PdfName;
				if (pdfString != null && pdfName != null && pdfName.Value.Equals("Ch") && base.Dictionary.ContainsKey("Opt") && PdfCrossTable.Dereference(base.Dictionary["Opt"]) is PdfArray pdfArray)
				{
					for (int i = 0; i < pdfArray.Count; i++)
					{
						if (PdfCrossTable.Dereference(pdfArray[i]) is PdfArray { Count: >1 } pdfArray2)
						{
							PdfString pdfString2 = PdfCrossTable.Dereference(pdfArray2[0]) as PdfString;
							PdfString pdfString3 = PdfCrossTable.Dereference(pdfArray2[1]) as PdfString;
							if (pdfString2 != null && pdfString3 != null && pdfString2.Value == pdfString.Value && pdfString3.Value != null)
							{
								isUnicode = PdfString.IsUnicode(pdfString3.Value);
								break;
							}
						}
					}
				}
			}
		}
		if (pdfDictionary != null && name != null)
		{
			text = FindFontName(pdfDictionary, name, out textFontStyle);
		}
		if (height == 0f)
		{
			if (pdfFont is PdfStandardFont pdfStandardFont)
			{
				height = GetFontHeight(pdfStandardFont.FontFamily);
				if (float.IsNaN(height) || height == 0f)
				{
					height = 12f;
				}
				pdfStandardFont.Size = height;
			}
			m_isCustomFontSize = true;
		}
		if (base.CrossTable.Document is PdfLoadedDocument { RaisePdfFont: not false } pdfLoadedDocument)
		{
			PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
			pdfFontEventArgs.m_fontName = text;
			pdfFontEventArgs.m_fontStyle = textFontStyle;
			pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
			return Font = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, height, isUnicode, string.Empty, textFontStyle);
		}
		if (pdfDictionary != null && name != null && pdfDictionary.ContainsKey(name))
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary[name]) as PdfDictionary;
			name = text;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Subtype"))
			{
				PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary["Subtype"]) as PdfName;
				if (pdfName2 != null)
				{
					PdfName pdfName3 = null;
					PdfFontStyle pdfFontStyle = PdfFontStyle.Regular;
					string standardName = string.Empty;
					PdfFontFamily pdfFontFamily = PdfFontFamily.Helvetica;
					if (pdfDictionary.ContainsKey("BaseFont"))
					{
						pdfName3 = PdfCrossTable.Dereference(pdfDictionary["BaseFont"]) as PdfName;
						if (pdfName3 != null)
						{
							pdfFontStyle = GetFontStyle(PdfName.DecodeName(pdfName3.Value));
							pdfFontFamily = GetFontFamily(PdfName.DecodeName(pdfName3.Value), out standardName);
						}
					}
					if (pdfName3 != null && pdfName2.Value == "Type1")
					{
						PdfFontMetrics pdfFontMetrics = CreateFont(pdfDictionary, height, pdfName3);
						pdfFont = new PdfStandardFont(pdfFontFamily, height, pdfFontStyle);
						if (!m_isTextChanged)
						{
							pdfFont = UpdateFontEncoding(pdfFont, pdfDictionary);
						}
						if (standardName == null)
						{
							if (m_isCustomFontSize)
							{
								height = GetFontHeight(pdfFontFamily);
								if (height != 0f)
								{
									pdfFont.Size = height;
								}
							}
							return pdfFont;
						}
						bool flag = false;
						if (pdfDictionary.ContainsKey("Encoding") && PdfCrossTable.Dereference(pdfDictionary["Encoding"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("Differences") && !pdfDictionary.ContainsKey("Widths"))
						{
							flag = true;
						}
						if ((!flag && !m_isTextChanged) || standardName != null)
						{
							pdfFont.Metrics = ((pdfFontMetrics != null && pdfFontMetrics.Height > 0f && pdfDictionary.ContainsKey("FontDescriptor")) ? pdfFontMetrics : pdfFont.Metrics);
						}
						pdfFont.FontInternal = pdfDictionary;
					}
					else
					{
						if (pdfName3 != null && pdfName2.Value == "TrueType")
						{
							pdfFont.FontInternal = pdfDictionary;
							pdfFont = CreateFontFromFontStream(pdfFont, pdfDictionary, isUnicode, height, pdfFontStyle);
							if (pdfFont.Name == null)
							{
								pdfFont.Metrics.Name = name;
							}
							if (pdfFont is PdfStandardFont)
							{
								string[] names = Enum.GetNames(typeof(PdfFontFamily));
								foreach (string value in names)
								{
									if (name.Contains(value))
									{
										return pdfFont = new PdfStandardFont((PdfFontFamily)Enum.Parse(typeof(PdfFontFamily), value, ignoreCase: true), height, pdfFontStyle);
									}
								}
							}
							if (!m_isTextChanged && pdfFont is PdfStandardFont)
							{
								WidthTable widthTable = pdfFont.Metrics.WidthTable;
								PdfFontMetrics pdfFontMetrics2 = CreateFont(pdfDictionary, height, pdfName3);
								if (pdfFontMetrics2 != null)
								{
									pdfFont.Metrics = pdfFontMetrics2;
								}
								pdfFont.Metrics.WidthTable = widthTable;
							}
							return pdfFont;
						}
						if (pdfName2.Value == "Type0")
						{
							PdfName pdfName4 = null;
							PdfDictionary pdfDictionary3 = pdfDictionary;
							PdfDictionary pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary["FontDescriptor"]) as PdfDictionary;
							if (pdfDictionary4 == null && pdfDictionary.ContainsKey("DescendantFonts"))
							{
								if (PdfCrossTable.Dereference(pdfDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray3)
								{
									pdfDictionary3 = PdfCrossTable.Dereference(pdfArray3[0]) as PdfDictionary;
									if (pdfDictionary3 != null)
									{
										pdfDictionary4 = PdfCrossTable.Dereference(pdfDictionary3["FontDescriptor"]) as PdfDictionary;
									}
								}
								if (pdfDictionary4 != null && pdfDictionary4.ContainsKey("FontName"))
								{
									pdfName4 = pdfDictionary4["FontName"] as PdfName;
									if (pdfName4 != null)
									{
										string text2 = pdfName4.Value.Substring(pdfName4.Value.IndexOf('+') + 1);
										PdfFontMetrics pdfFontMetrics3 = CreateFont(pdfDictionary3, height, new PdfName(text2));
										if (text2.Contains("-"))
										{
											text2 = text2.Remove(text2.IndexOf("-"));
										}
										if (text2 == "ArialMT")
										{
											text2 = "Arial";
										}
										pdfFont.FontInternal = pdfDictionary;
										pdfFont = CreateFontFromFontStream(pdfFont, pdfDictionary, isUnicode, height, pdfFontStyle);
										if (pdfFont.Name == null)
										{
											pdfFont.Metrics.Name = text2;
										}
										if (pdfFont is PdfStandardFont)
										{
											WidthTable widthTable2 = pdfFont.Metrics.WidthTable;
											if (pdfFontMetrics3 != null)
											{
												pdfFont.Metrics = pdfFontMetrics3;
											}
											pdfFont.Metrics.WidthTable = widthTable2;
										}
										return pdfFont;
									}
								}
							}
						}
					}
				}
			}
		}
		else
		{
			PdfFont fontByName = GetFontByName(name, height);
			if (fontByName != null)
			{
				pdfFont = fontByName;
			}
			else
			{
				isCorrectFont = false;
			}
		}
		return pdfFont;
	}

	private string FindFontName(PdfDictionary fontDictionary, string name, out PdfFontStyle textFontStyle)
	{
		textFontStyle = PdfFontStyle.Regular;
		PdfName pdfName = null;
		fontDictionary = PdfCrossTable.Dereference(fontDictionary[name]) as PdfDictionary;
		if (fontDictionary != null && fontDictionary.ContainsKey("BaseFont"))
		{
			pdfName = PdfCrossTable.Dereference(fontDictionary["BaseFont"]) as PdfName;
			if (pdfName != null)
			{
				name = PdfName.DecodeName(pdfName.Value);
				textFontStyle = GetFontStyle(pdfName.Value);
				if (name.Contains("PSMT"))
				{
					name = name.Remove(name.IndexOf("PSMT"));
				}
				if (name.Contains("PS"))
				{
					name = name.Remove(name.IndexOf("PS"));
				}
				if (name.Contains("-"))
				{
					name = name.Remove(name.IndexOf("-"));
				}
			}
		}
		else if (fontDictionary != null)
		{
			PdfName pdfName2 = null;
			PdfDictionary pdfDictionary = fontDictionary;
			PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) as PdfDictionary;
			if (pdfDictionary2 == null && fontDictionary.ContainsKey("DescendantFonts") && PdfCrossTable.Dereference(fontDictionary["DescendantFonts"]) is PdfArray { Count: >0 } pdfArray)
			{
				if (PdfCrossTable.Dereference(pdfArray[0]) is PdfDictionary pdfDictionary3)
				{
					pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary3["FontDescriptor"]) as PdfDictionary;
				}
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("FontName"))
				{
					pdfName2 = pdfDictionary2["FontName"] as PdfName;
					if (pdfName2 != null)
					{
						string text = pdfName2.Value.Substring(pdfName2.Value.IndexOf('+') + 1);
						textFontStyle = GetFontStyle(text);
						if (text.Contains("PSMT"))
						{
							text = text.Remove(text.IndexOf("PSMT"));
						}
						if (text.Contains("PS"))
						{
							text = text.Remove(text.IndexOf("PS"));
						}
						if (text.Contains("-"))
						{
							text = text.Remove(text.IndexOf("-"));
						}
					}
				}
			}
		}
		else if (name != null)
		{
			textFontStyle = GetFontStyle(name);
		}
		string[] array2;
		if (name.Contains("#"))
		{
			string[] array = name.Split('#');
			string text2 = string.Empty;
			array2 = array;
			foreach (string text3 in array2)
			{
				text2 = (text3.Contains("20") ? (text2 + text3.Substring(2)) : (text2 + text3));
			}
			name = text2;
		}
		string text4 = name;
		string[] array3 = new string[1] { "" };
		int num = 0;
		for (int j = 0; j < text4.Length; j++)
		{
			string text5 = text4.Substring(j, 1);
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(text5) && j > 0 && !"ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".Contains(text4[j - 1].ToString()))
			{
				num++;
				string[] array4 = new string[num + 1];
				Array.Copy(array3, 0, array4, 0, num);
				array3 = array4;
			}
			if (!text5.Contains(" "))
			{
				array3[num] += text5;
			}
		}
		name = string.Empty;
		array2 = array3;
		foreach (string text6 in array2)
		{
			name = name + text6 + " ";
		}
		name = name.Substring(name.IndexOf('+') + 1);
		name = name.Trim();
		name = name.Split(',')[0];
		if (name == "Arial MT")
		{
			name = "Arial";
		}
		return name;
	}

	private PdfFont CreateFontFromFontStream(PdfFont font, PdfDictionary fontDictionary, bool isUnicode, float height, PdfFontStyle fontStyle)
	{
		PdfDictionary pdfDictionary = PdfCrossTable.Dereference(fontDictionary["FontDescriptor"]) as PdfDictionary;
		if (pdfDictionary == null && fontDictionary.ContainsKey("DescendantFonts") && PdfCrossTable.Dereference(fontDictionary["DescendantFonts"]) is PdfArray pdfArray && PdfCrossTable.Dereference(pdfArray[0]) is PdfDictionary pdfDictionary2)
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary2["FontDescriptor"]) as PdfDictionary;
		}
		Encoding encoding = null;
		try
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			encoding = Encoding.GetEncoding(1250);
		}
		catch
		{
			encoding = new Windows1252Encoding();
		}
		PdfStream pdfStream = null;
		if (pdfDictionary != null && pdfDictionary.ContainsKey("FontFile2"))
		{
			pdfStream = PdfCrossTable.Dereference(pdfDictionary["FontFile2"]) as PdfStream;
		}
		if (pdfDictionary != null && pdfDictionary.ContainsKey("FontFile3"))
		{
			pdfStream = PdfCrossTable.Dereference(pdfDictionary["FontFile3"]) as PdfStream;
		}
		if (pdfStream != null)
		{
			pdfStream.Decompress();
			pdfStream.FreezeChanges(pdfStream);
			try
			{
				pdfStream.InternalStream.Position = 0L;
				font = new PdfTrueTypeFont(pdfStream.InternalStream, height, isUnicode, string.Empty, fontStyle);
			}
			catch
			{
				if (isUnicode && font is PdfStandardFont)
				{
					(font as PdfStandardFont).SetTextEncoding(encoding);
				}
			}
		}
		else if (isUnicode && font is PdfStandardFont)
		{
			(font as PdfStandardFont).SetTextEncoding(encoding);
		}
		return font;
	}

	private PdfDestination ObtainDestination(PdfDictionary actionDictionary)
	{
		PdfDestination pdfDestination = null;
		if (actionDictionary != null)
		{
			IPdfPrimitive pdfPrimitive = actionDictionary["D"];
			PdfArray pdfArray = pdfPrimitive as PdfArray;
			PdfName pdfName = pdfPrimitive as PdfName;
			PdfString pdfString = pdfPrimitive as PdfString;
			PdfLoadedDocument pdfLoadedDocument = base.CrossTable.Document as PdfLoadedDocument;
			if (pdfLoadedDocument != null)
			{
				if (pdfName != null)
				{
					pdfArray = pdfLoadedDocument.GetNamedDestination(pdfName);
				}
				else if (pdfString != null)
				{
					pdfArray = pdfLoadedDocument.GetNamedDestination(pdfString);
				}
			}
			if (pdfArray != null)
			{
				PdfReferenceHolder pdfReferenceHolder = pdfArray[0] as PdfReferenceHolder;
				PdfPageBase pdfPageBase = null;
				if (pdfReferenceHolder != null && PdfCrossTable.Dereference(pdfReferenceHolder) is PdfDictionary dic && pdfLoadedDocument != null)
				{
					pdfPageBase = pdfLoadedDocument.Pages.GetPage(dic);
				}
				if (pdfPageBase != null)
				{
					PdfName pdfName2 = pdfArray[1] as PdfName;
					if (pdfName2.Value == "FitBH" || pdfName2.Value == "FitH")
					{
						PdfNumber pdfNumber = pdfArray[2] as PdfNumber;
						float y = ((pdfNumber == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber.FloatValue));
						pdfDestination = new PdfDestination(pdfPageBase, new PointF(0f, y));
						if (pdfNumber == null)
						{
							pdfDestination.SetValidation(valid: false);
						}
						if (pdfName2.Value == "FitH")
						{
							pdfDestination.Mode = PdfDestinationMode.FitH;
						}
						else
						{
							pdfDestination.Mode = PdfDestinationMode.FitBH;
						}
					}
					else if (pdfName2.Value == "XYZ")
					{
						PdfNumber pdfNumber2 = pdfArray[2] as PdfNumber;
						PdfNumber pdfNumber3 = pdfArray[3] as PdfNumber;
						PdfNumber pdfNumber4 = pdfArray[4] as PdfNumber;
						if (pdfPageBase != null)
						{
							float y2 = ((pdfNumber3 == null) ? 0f : (pdfPageBase.Size.Height - pdfNumber3.FloatValue));
							float x = pdfNumber2?.FloatValue ?? 0f;
							pdfDestination = new PdfDestination(pdfPageBase, new PointF(x, y2));
							if (pdfNumber4 != null)
							{
								pdfDestination.Zoom = pdfNumber4.FloatValue;
							}
							if (pdfNumber2 == null || pdfNumber3 == null || pdfNumber4 == null)
							{
								pdfDestination.SetValidation(valid: false);
							}
						}
					}
					else if (pdfName2.Value == "FitR")
					{
						if (pdfArray.Count == 6)
						{
							PdfNumber pdfNumber5 = pdfArray[2] as PdfNumber;
							PdfNumber pdfNumber6 = pdfArray[3] as PdfNumber;
							PdfNumber pdfNumber7 = pdfArray[4] as PdfNumber;
							PdfNumber pdfNumber8 = pdfArray[5] as PdfNumber;
							pdfDestination = new PdfDestination(pdfPageBase, new RectangleF(pdfNumber5.FloatValue, pdfNumber6.FloatValue, pdfNumber7.FloatValue, pdfNumber8.FloatValue));
							pdfDestination.Mode = PdfDestinationMode.FitR;
						}
					}
					else if (pdfPageBase != null && pdfName2.Value == "Fit")
					{
						pdfDestination = new PdfDestination(pdfPageBase);
						pdfDestination.Mode = PdfDestinationMode.FitToPage;
					}
					else if (pdfName2.Value == "FitV" || pdfName2.Value == "FitBV")
					{
						if (pdfArray[2] is PdfNumber pdfNumber9)
						{
							pdfDestination = new PdfDestination(pdfPageBase, new PointF(pdfNumber9.FloatValue, 0f));
						}
						if (pdfName2.Value == "FitV")
						{
							pdfDestination.Mode = PdfDestinationMode.FitV;
						}
						else
						{
							pdfDestination.Mode = PdfDestinationMode.FitBV;
						}
					}
				}
			}
		}
		return pdfDestination;
	}

	private PdfRemoteDestination ObtainRemoteDestination(PdfDictionary actionDictionary)
	{
		PdfRemoteDestination pdfRemoteDestination = null;
		if (actionDictionary != null)
		{
			PdfArray pdfArray = actionDictionary["D"] as PdfArray;
			_ = base.CrossTable.Document;
			if (pdfArray != null)
			{
				pdfRemoteDestination = new PdfRemoteDestination();
				PdfNumber pdfNumber = ((pdfArray.Count > 0) ? (pdfArray[0] as PdfNumber) : new PdfNumber(0));
				if (pdfNumber != null)
				{
					pdfRemoteDestination.RemotePageNumber = pdfNumber.IntValue;
				}
				PdfName pdfName = ((pdfArray.Count > 1) ? (pdfArray[1] as PdfName) : new PdfName(string.Empty));
				if (pdfName != null)
				{
					switch (pdfName.Value)
					{
					case "FitV":
					{
						pdfRemoteDestination.Mode = PdfDestinationMode.FitV;
						PdfNumber pdfNumber8 = ((pdfArray.Count > 2) ? (pdfArray[2] as PdfNumber) : new PdfNumber(0));
						if (pdfRemoteDestination != null && pdfNumber8 != null)
						{
							pdfRemoteDestination.Location = new PointF(pdfNumber8.FloatValue, 0f);
						}
						break;
					}
					case "FitH":
					{
						pdfRemoteDestination.Mode = PdfDestinationMode.FitH;
						PdfNumber pdfNumber3 = ((pdfArray.Count > 2) ? (pdfArray[2] as PdfNumber) : new PdfNumber(0));
						if (pdfRemoteDestination != null && pdfNumber3 != null)
						{
							pdfRemoteDestination.Location = new PointF(pdfNumber3.FloatValue, 0f);
						}
						break;
					}
					case "Fit":
						pdfRemoteDestination.Mode = PdfDestinationMode.FitToPage;
						break;
					case "FitR":
					{
						pdfRemoteDestination.Mode = PdfDestinationMode.FitR;
						PdfNumber pdfNumber4 = ((pdfArray.Count > 2) ? (pdfArray[2] as PdfNumber) : new PdfNumber(0));
						PdfNumber pdfNumber5 = ((pdfArray.Count > 3) ? (pdfArray[3] as PdfNumber) : new PdfNumber(0));
						PdfNumber pdfNumber6 = ((pdfArray.Count > 4) ? (pdfArray[4] as PdfNumber) : new PdfNumber(0));
						PdfNumber pdfNumber7 = ((pdfArray.Count > 5) ? (pdfArray[5] as PdfNumber) : new PdfNumber(0));
						if (pdfRemoteDestination != null)
						{
							pdfRemoteDestination.Location = new PointF(pdfNumber4.FloatValue, pdfNumber5.FloatValue);
							pdfRemoteDestination.Bounds = new RectangleF(0f, 0f, pdfNumber6.FloatValue, pdfNumber7.FloatValue);
						}
						break;
					}
					case "XYZ":
					{
						pdfRemoteDestination.Mode = PdfDestinationMode.Location;
						PdfNumber pdfNumber9 = ((pdfArray.Count > 2) ? (pdfArray[2] as PdfNumber) : new PdfNumber(0));
						PdfNumber pdfNumber10 = ((pdfArray.Count > 3) ? (pdfArray[3] as PdfNumber) : new PdfNumber(0));
						PdfNumber pdfNumber11 = ((pdfArray.Count > 4) ? (pdfArray[4] as PdfNumber) : new PdfNumber(0));
						if (pdfRemoteDestination != null)
						{
							pdfRemoteDestination.Location = new PointF(pdfNumber9.FloatValue, pdfNumber10.FloatValue);
							pdfRemoteDestination.Zoom = pdfNumber11.FloatValue;
						}
						break;
					}
					case "FitBH":
					{
						pdfRemoteDestination.Mode = PdfDestinationMode.FitBH;
						PdfNumber pdfNumber12 = ((pdfArray.Count > 2) ? (pdfArray[2] as PdfNumber) : new PdfNumber(0));
						if (pdfRemoteDestination != null)
						{
							pdfRemoteDestination.Location = new PointF(0f, pdfNumber12.FloatValue);
						}
						break;
					}
					case "FitB":
						pdfRemoteDestination.Mode = PdfDestinationMode.FitB;
						break;
					case "FitBV":
					{
						pdfRemoteDestination.Mode = PdfDestinationMode.FitBV;
						PdfNumber pdfNumber2 = ((pdfArray.Count > 2) ? (pdfArray[2] as PdfNumber) : new PdfNumber(0));
						if (pdfRemoteDestination != null)
						{
							pdfRemoteDestination.Location = new PointF(pdfNumber2.FloatValue, 0f);
						}
						break;
					}
					}
				}
			}
		}
		return pdfRemoteDestination;
	}

	private PdfAction GetPdfAction(string key)
	{
		PdfAction result = null;
		PdfDictionary widgetAnnotation = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
		if (widgetAnnotation != null)
		{
			if (widgetAnnotation.ContainsKey("AA"))
			{
				if (base.CrossTable.GetObject(widgetAnnotation["AA"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey(key))
				{
					PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary[key]) as PdfDictionary;
					if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("JS"))
					{
						if (PdfCrossTable.Dereference(pdfDictionary2["JS"]) is PdfString pdfString)
						{
							result = new PdfJavaScriptAction(pdfString.Value);
						}
					}
					else
					{
						PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary2["S"]) as PdfName;
						if (pdfName != null)
						{
							if (pdfName.Value == "GoTo")
							{
								PdfDestination pdfDestination = null;
								if (pdfDictionary2.ContainsKey("D"))
								{
									pdfDestination = ObtainDestination(pdfDictionary2);
								}
								if (pdfDestination != null)
								{
									result = new PdfGoToAction(pdfDestination);
								}
							}
							if (pdfName.Value == "GoToR")
							{
								PdfRemoteDestination remoteDestination = null;
								if (pdfDictionary2.ContainsKey("D"))
								{
									remoteDestination = ObtainRemoteDestination(pdfDictionary2);
								}
								if (PdfCrossTable.Dereference(pdfDictionary2["F"]) is PdfString { Value: not null } pdfString2)
								{
									PdfRemoteGoToAction pdfRemoteGoToAction = new PdfRemoteGoToAction(pdfString2.Value, remoteDestination);
									if (pdfDictionary2.ContainsKey("NewWindow") && PdfCrossTable.Dereference(pdfDictionary2["NewWindow"]) is PdfBoolean pdfBoolean)
									{
										_ = pdfBoolean.Value;
										pdfRemoteGoToAction.IsNewWindow = pdfBoolean.Value;
									}
									result = pdfRemoteGoToAction;
								}
							}
							if (pdfName.Value == "Named")
							{
								PdfActionDestination destination = PdfActionDestination.FirstPage;
								if (pdfDictionary2.ContainsKey("N"))
								{
									PdfName pdfName2 = PdfCrossTable.Dereference(pdfDictionary2["N"]) as PdfName;
									if (pdfName2 != null && pdfName2.Value != null)
									{
										switch (pdfName2.Value)
										{
										case "NextPage":
											destination = PdfActionDestination.NextPage;
											break;
										case "LastPage":
											destination = PdfActionDestination.LastPage;
											break;
										case "PrevPage":
											destination = PdfActionDestination.PrevPage;
											break;
										}
									}
								}
								result = new PdfNamedAction(destination);
							}
						}
					}
				}
			}
			else if (m_mouseUpAction && widgetAnnotation.ContainsKey("A") && PdfCrossTable.Dereference(widgetAnnotation["A"]) is PdfDictionary pdfDictionary3)
			{
				PdfName pdfName3 = PdfCrossTable.Dereference(pdfDictionary3["S"]) as PdfName;
				if (pdfName3 != null)
				{
					if (pdfName3.Value == "GoTo")
					{
						PdfDestination pdfDestination2 = null;
						if (pdfDictionary3.ContainsKey("D"))
						{
							pdfDestination2 = ObtainDestination(pdfDictionary3);
						}
						if (pdfDestination2 != null)
						{
							result = new PdfGoToAction(pdfDestination2);
						}
					}
					if (pdfName3.Value == "GoToR")
					{
						PdfRemoteDestination remoteDestination2 = null;
						if (pdfDictionary3.ContainsKey("D"))
						{
							remoteDestination2 = ObtainRemoteDestination(pdfDictionary3);
						}
						if (pdfDictionary3.ContainsKey("F") && PdfCrossTable.Dereference(pdfDictionary3["F"]) is PdfString { Value: not null } pdfString3)
						{
							PdfRemoteGoToAction pdfRemoteGoToAction2 = new PdfRemoteGoToAction(pdfString3.Value, remoteDestination2);
							if (pdfDictionary3.ContainsKey("NewWindow") && PdfCrossTable.Dereference(pdfDictionary3["NewWindow"]) is PdfBoolean pdfBoolean2)
							{
								_ = pdfBoolean2.Value;
								pdfRemoteGoToAction2.IsNewWindow = pdfBoolean2.Value;
							}
							result = pdfRemoteGoToAction2;
						}
					}
					if (pdfName3.Value == "Named")
					{
						PdfActionDestination destination2 = PdfActionDestination.FirstPage;
						if (pdfDictionary3.ContainsKey("N"))
						{
							PdfName pdfName4 = PdfCrossTable.Dereference(pdfDictionary3["N"]) as PdfName;
							if (pdfName4 != null && pdfName4.Value != null)
							{
								switch (pdfName4.Value)
								{
								case "NextPage":
									destination2 = PdfActionDestination.NextPage;
									break;
								case "LastPage":
									destination2 = PdfActionDestination.LastPage;
									break;
								case "PrevPage":
									destination2 = PdfActionDestination.PrevPage;
									break;
								}
							}
						}
						result = new PdfNamedAction(destination2);
					}
				}
			}
		}
		return result;
	}
}
