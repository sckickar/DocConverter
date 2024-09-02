using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedTextMarkupAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfDictionary m_dictionary;

	private PdfTextMarkupAnnotationType m_TextMarkupAnnotationType;

	private PdfColor m_color;

	private List<RectangleF> m_boundscollection = new List<RectangleF>();

	private PdfDictionary m_borderDic = new PdfDictionary();

	private PdfLineBorderStyle m_borderStyle;

	public PdfLoadedPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory == null)
			{
				m_reviewHistory = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: true);
			}
			return m_reviewHistory;
		}
	}

	public PdfLoadedPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments == null)
			{
				m_comments = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: false);
			}
			return m_comments;
		}
	}

	public PdfTextMarkupAnnotationType TextMarkupAnnotationType
	{
		get
		{
			return ObtainTextMarkupAnnotationType();
		}
		set
		{
			m_TextMarkupAnnotationType = value;
			base.Dictionary.SetName("Subtype", m_TextMarkupAnnotationType.ToString());
			NotifyPropertyChanged("TextMarkupAnnotationType");
		}
	}

	public PdfColor TextMarkupColor
	{
		get
		{
			return ObtainTextMarkupColor();
		}
		set
		{
			PdfArray pdfArray = new PdfArray();
			m_color = value;
			pdfArray.Insert(0, new PdfNumber((float)(int)m_color.R / 255f));
			pdfArray.Insert(1, new PdfNumber((float)(int)m_color.G / 255f));
			pdfArray.Insert(2, new PdfNumber((float)(int)m_color.B / 255f));
			base.Dictionary.SetProperty("C", pdfArray);
			NotifyPropertyChanged("TextMarkupColor");
		}
	}

	public List<RectangleF> BoundsCollection
	{
		get
		{
			m_boundscollection = ObtainBoundsValue();
			return m_boundscollection;
		}
		set
		{
			m_boundscollection = value;
			SetQuadPoints(base.Page.Size);
			NotifyPropertyChanged("BoundsCollection");
		}
	}

	internal PdfLineBorderStyle BorderStyle
	{
		get
		{
			return GetLineBorder();
		}
		set
		{
			m_borderStyle = value;
			if (m_borderStyle == PdfLineBorderStyle.Solid)
			{
				m_borderDic.SetProperty("S", new PdfName("S"));
			}
			else if (m_borderStyle == PdfLineBorderStyle.Inset)
			{
				m_borderDic.SetProperty("S", new PdfName("I"));
			}
			else if (m_borderStyle == PdfLineBorderStyle.Dashed)
			{
				m_borderDic.SetProperty("S", new PdfName("D"));
			}
			else if (m_borderStyle == PdfLineBorderStyle.Beveled)
			{
				m_borderDic.SetProperty("S", new PdfName("B"));
			}
			else if (m_borderStyle == PdfLineBorderStyle.Underline)
			{
				m_borderDic.SetProperty("S", new PdfName("U"));
			}
			NotifyPropertyChanged("BorderStyle");
		}
	}

	internal PdfLoadedTextMarkupAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectagle)
		: base(dictionary, crossTable)
	{
		m_dictionary = dictionary;
		m_crossTable = crossTable;
	}

	public void SetTitleText(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text == string.Empty)
		{
			throw new ArgumentException("The text can't be empty");
		}
		if (Text != text)
		{
			new PdfString(text);
			base.Dictionary.SetString("T", text);
			base.Changed = true;
		}
	}

	private List<RectangleF> ObtainBoundsValue()
	{
		List<RectangleF> list = new List<RectangleF>();
		if (base.Dictionary.ContainsKey("QuadPoints") && PdfCrossTable.Dereference(base.Dictionary["QuadPoints"]) is PdfArray pdfArray)
		{
			int num = pdfArray.Count / 8;
			for (int i = 0; i < num; i++)
			{
				PdfNumber pdfNumber = pdfArray[4 + i * 8] as PdfNumber;
				PdfNumber pdfNumber2 = pdfArray[i * 8] as PdfNumber;
				PdfNumber pdfNumber3 = pdfArray[5 + i * 8] as PdfNumber;
				PdfNumber pdfNumber4 = pdfArray[1 + i * 8] as PdfNumber;
				PdfNumber pdfNumber5 = pdfArray[6 + i * 8] as PdfNumber;
				PdfNumber pdfNumber6 = pdfArray[7 + i * 8] as PdfNumber;
				PdfNumber pdfNumber7 = pdfArray[2 + i * 8] as PdfNumber;
				PdfNumber pdfNumber8 = pdfArray[3 + i * 8] as PdfNumber;
				if (pdfNumber == null || pdfNumber2 == null || pdfNumber3 == null || pdfNumber4 == null || pdfNumber5 == null || pdfNumber6 == null || pdfNumber7 == null || pdfNumber8 == null)
				{
					continue;
				}
				float num2 = pdfNumber.FloatValue - pdfNumber2.FloatValue;
				float num3 = pdfNumber3.FloatValue - pdfNumber4.FloatValue;
				double num4 = Math.Sqrt(num2 * num2 + num3 * num3);
				float num5 = pdfNumber5.FloatValue - pdfNumber.FloatValue;
				num3 = pdfNumber6.FloatValue - pdfNumber3.FloatValue;
				double num6 = Math.Sqrt(num5 * num5 + num3 * num3);
				float floatValue = pdfNumber2.FloatValue;
				float floatValue2 = pdfNumber7.FloatValue;
				floatValue = Math.Min(floatValue, floatValue2);
				float val = base.Page.Size.Height - pdfNumber4.FloatValue;
				float val2 = base.Page.Size.Height - pdfNumber8.FloatValue;
				val = Math.Min(val, val2);
				if (floatValue != 0f || val != 0f)
				{
					PdfArray cropOrMediaBox = null;
					cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
					if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
					{
						PdfNumber pdfNumber9 = cropOrMediaBox[0] as PdfNumber;
						PdfNumber pdfNumber10 = cropOrMediaBox[1] as PdfNumber;
						if (pdfNumber9 != null && pdfNumber10 != null && (pdfNumber9.FloatValue != 0f || pdfNumber10.FloatValue != 0f))
						{
							float num7 = floatValue;
							float num8 = val;
							floatValue = 0f - pdfNumber9.FloatValue - (0f - num7);
							val = pdfNumber10.FloatValue + num8;
						}
					}
				}
				RectangleF item = new RectangleF(floatValue, val, (float)num6, (float)num4);
				list.Add(item);
			}
		}
		return list;
	}

	private void SetQuadPoints(SizeF pageSize)
	{
		float[] array = new float[m_boundscollection.Count * 8];
		_ = pageSize.Width;
		float height = pageSize.Height;
		PdfArray cropOrMediaBox = null;
		cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
		bool flag = false;
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
		{
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				for (int i = 0; i < m_boundscollection.Count; i++)
				{
					float num = m_boundscollection[i].X + pdfNumber.FloatValue;
					float floatValue = pdfNumber2.FloatValue;
					array[i * 8] = num;
					array[1 + i * 8] = height - (0f - floatValue) - m_boundscollection[i].Y;
					array[2 + i * 8] = num + m_boundscollection[i].Width;
					array[3 + i * 8] = height - (0f - floatValue) - m_boundscollection[i].Y;
					array[4 + i * 8] = num;
					array[5 + i * 8] = array[1 + i * 8] - m_boundscollection[i].Height;
					array[6 + i * 8] = num + m_boundscollection[i].Width;
					array[7 + i * 8] = array[5 + i * 8];
				}
				flag = true;
			}
		}
		if (!flag)
		{
			for (int j = 0; j < m_boundscollection.Count; j++)
			{
				float x = m_boundscollection[j].X;
				float y = m_boundscollection[j].Y;
				array[j * 8] = x;
				array[1 + j * 8] = height - y;
				array[2 + j * 8] = x + m_boundscollection[j].Width;
				array[3 + j * 8] = height - y;
				array[4 + j * 8] = x;
				array[5 + j * 8] = array[1 + j * 8] - m_boundscollection[j].Height;
				array[6 + j * 8] = x + m_boundscollection[j].Width;
				array[7 + j * 8] = array[5 + j * 8];
			}
		}
		base.Dictionary.SetProperty("QuadPoints", new PdfArray(array));
	}

	private PdfTextMarkupAnnotationType ObtainTextMarkupAnnotationType()
	{
		string aType = (base.Dictionary["Subtype"] as PdfName).Value.ToString();
		return GetTextMarkupAnnotation(aType);
	}

	private PdfTextMarkupAnnotationType GetTextMarkupAnnotation(string aType)
	{
		PdfTextMarkupAnnotationType result = PdfTextMarkupAnnotationType.Highlight;
		switch (aType)
		{
		case "Highlight":
			result = PdfTextMarkupAnnotationType.Highlight;
			break;
		case "Squiggly":
			result = PdfTextMarkupAnnotationType.Squiggly;
			break;
		case "StrikeOut":
			result = PdfTextMarkupAnnotationType.StrikeOut;
			break;
		case "Underline":
			result = PdfTextMarkupAnnotationType.Underline;
			break;
		}
		return result;
	}

	private PdfColor ObtainTextMarkupColor()
	{
		PdfColorSpace colorSpace = PdfColorSpace.RGB;
		PdfColor result = PdfColor.Empty;
		PdfArray pdfArray = null;
		pdfArray = ((!base.Dictionary.ContainsKey("C")) ? result.ToArray(colorSpace) : (PdfCrossTable.Dereference(base.Dictionary["C"]) as PdfArray));
		if (pdfArray != null && pdfArray[0] is PdfNumber && pdfArray[1] is PdfNumber && pdfArray[2] is PdfNumber)
		{
			float floatValue = (pdfArray[0] as PdfNumber).FloatValue;
			float floatValue2 = (pdfArray[1] as PdfNumber).FloatValue;
			float floatValue3 = (pdfArray[2] as PdfNumber).FloatValue;
			result = new PdfColor(floatValue, floatValue2, floatValue3);
		}
		return result;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (base.Flatten || base.Page.Annotations.Flatten || base.SetAppearanceDictionary || isExternalFlatten)
		{
			PdfName pdfName = PdfCrossTable.Dereference(base.Dictionary["Subtype"]) as PdfName;
			if (pdfName != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten) && !(pdfName.Value == "Highlight") && !(pdfName.Value == "Squiggly") && !(pdfName.Value == "Underline") && !(pdfName.Value == "StrikeOut"))
			{
				FlattenNonSupportAnnotation();
			}
			else
			{
				PdfTemplate pdfTemplate = CreateAppearance();
				if (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten)
				{
					FlattenAnnotation(base.Page, pdfTemplate);
				}
				else if (pdfTemplate != null)
				{
					base.Appearance.Normal = pdfTemplate;
					base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
				}
			}
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenLoadedPopup();
		}
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		if (base.Dictionary.ContainsKey("AP") && appearance == null)
		{
			if (!(PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary))
			{
				return;
			}
			if (PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2)
			{
				if (!(pdfDictionary2 is PdfStream template))
				{
					return;
				}
				appearance = new PdfTemplate(template);
				if (appearance != null)
				{
					bool flag = ValidateTemplateMatrix(pdfDictionary2);
					if (flag && base.Page.Rotation != 0)
					{
						FlattenAnnotationTemplate(appearance, flag);
					}
					else if (flag && IsValidTemplateMatrix(pdfDictionary2, Bounds.Location, appearance))
					{
						FlattenAnnotationTemplate(appearance, flag);
					}
				}
			}
			else
			{
				base.SetAppearanceDictionary = true;
				appearance = CreateAppearance();
				if (appearance != null)
				{
					bool isNormalMatrix = ValidateTemplateMatrix(appearance.m_content);
					FlattenAnnotationTemplate(appearance, isNormalMatrix);
				}
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance == null)
		{
			base.SetAppearanceDictionary = true;
			appearance = CreateAppearance();
			if (appearance != null)
			{
				bool isNormalMatrix2 = ValidateTemplateMatrix(appearance.m_content);
				FlattenAnnotationTemplate(appearance, isNormalMatrix2);
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix3 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix3);
		}
		else if (base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix4 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix4);
		}
	}

	private PdfTemplate CreateAppearance()
	{
		if (base.SetAppearanceDictionary)
		{
			RectangleF rectangle = RectangleF.Empty;
			if (BoundsCollection.Count > 1)
			{
				PdfPath pdfPath = new PdfPath();
				for (int i = 0; i < BoundsCollection.Count; i++)
				{
					pdfPath.AddRectangle(BoundsCollection[i]);
				}
				rectangle = (Bounds = pdfPath.GetBounds());
			}
			else if (base.Dictionary.ContainsKey("QuadPoints") && PdfCrossTable.Dereference(base.Dictionary["QuadPoints"]) is PdfArray pdfArray)
			{
				for (int j = 0; j < pdfArray.Count / 8; j++)
				{
					PointF[] array = new PointF[pdfArray.Count / 2];
					int num = 0;
					int num2 = 0;
					while (num2 < pdfArray.Count)
					{
						float floatValue = (pdfArray[num2] as PdfNumber).FloatValue;
						float floatValue2 = (pdfArray[num2 + 1] as PdfNumber).FloatValue;
						array[num] = new PointF(floatValue, floatValue2);
						num2 += 2;
						num++;
					}
					PdfPath pdfPath2 = new PdfPath();
					pdfPath2.AddLines(array);
					rectangle = pdfPath2.GetBounds();
				}
			}
			PdfTemplate pdfTemplate = new PdfTemplate(new RectangleF(0f, 0f, rectangle.Width, rectangle.Height));
			SetMatrix(pdfTemplate.m_content);
			PdfGraphics graphics = pdfTemplate.Graphics;
			graphics.SetTransparency(Opacity, Opacity, PdfBlendMode.Multiply);
			if (BoundsCollection.Count > 1)
			{
				for (int k = 0; k < BoundsCollection.Count; k++)
				{
					if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Highlight)
					{
						graphics.DrawRectangle(new PdfSolidBrush(TextMarkupColor), BoundsCollection[k].X - rectangle.X, BoundsCollection[k].Y - rectangle.Y, BoundsCollection[k].Width, BoundsCollection[k].Height);
					}
					else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Underline)
					{
						graphics.DrawLine(new PdfPen(TextMarkupColor), BoundsCollection[k].X - rectangle.X, BoundsCollection[k].Y - rectangle.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f / 3f), BoundsCollection[k].Width + (BoundsCollection[k].X - rectangle.X), BoundsCollection[k].Y - rectangle.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f / 3f));
					}
					else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.StrikeOut)
					{
						graphics.DrawLine(new PdfPen(TextMarkupColor), BoundsCollection[k].X - rectangle.X, BoundsCollection[k].Y - rectangle.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f), BoundsCollection[k].Width + (BoundsCollection[k].X - rectangle.X), BoundsCollection[k].Y - rectangle.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f));
					}
					else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Squiggly)
					{
						PdfPen pdfPen = new PdfPen(TextMarkupColor);
						pdfPen.Width = BoundsCollection[k].Height * 0.02f;
						graphics.Save();
						graphics.TranslateTransform(BoundsCollection[k].X - rectangle.X, BoundsCollection[k].Y - rectangle.Y);
						graphics.SetClip(new RectangleF(0f, 0f, BoundsCollection[k].Width, BoundsCollection[k].Height));
						graphics.DrawPath(pdfPen, DrawSquiggly(BoundsCollection[k].Width, BoundsCollection[k].Height));
						graphics.Restore();
					}
				}
			}
			else
			{
				if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Highlight)
				{
					graphics.DrawRectangle(new PdfSolidBrush(TextMarkupColor), 0f, 0f, rectangle.Width, rectangle.Height);
				}
				else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Underline)
				{
					graphics.DrawLine(new PdfPen(TextMarkupColor), 0f, rectangle.Height - rectangle.Height / 2f / 3f, rectangle.Width, rectangle.Height - rectangle.Height / 2f / 3f);
				}
				else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.StrikeOut)
				{
					graphics.DrawLine(new PdfPen(TextMarkupColor), 0f, rectangle.Height / 2f, rectangle.Width, rectangle.Height / 2f);
				}
				else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Squiggly)
				{
					PdfPen pdfPen2 = new PdfPen(TextMarkupColor);
					pdfPen2.Width = rectangle.Height * 0.02f;
					graphics.DrawPath(pdfPen2, DrawSquiggly(rectangle.Width, rectangle.Height));
				}
				base.Dictionary["Rect"] = PdfArray.FromRectangle(rectangle);
			}
			return pdfTemplate;
		}
		return null;
	}

	private PdfPath DrawSquiggly(float width, float height)
	{
		if ((int)width % 2 != 0 || Math.Round(width) > (double)width)
		{
			width = (int)width + 1;
		}
		PdfPath pdfPath = new PdfPath();
		PointF[] array = new PointF[(int)Math.Ceiling(width / height * 16f)];
		float num = width / (float)(array.Length / 2);
		float num2 = (num + num) * 0.6f;
		float num3 = num2;
		float num4 = 0f;
		int num5 = 0;
		while (num5 < array.Length)
		{
			array[num5] = new PointF(num4, height - num2 + num3 - height * 0.02f);
			num3 = ((num3 != 0f) ? 0f : num2);
			num5++;
			num4 += num;
		}
		pdfPath.AddLines(array);
		return pdfPath;
	}

	internal void FlattenNonSupportAnnotation()
	{
		PdfTemplate pdfTemplate = null;
		if (base.Dictionary.ContainsKey("AP") && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			pdfTemplate = new PdfTemplate(template);
			if (pdfTemplate != null)
			{
				bool isNormalMatrix = ValidateTemplateMatrix(pdfDictionary2);
				FlattenAnnotationTemplate(pdfTemplate, isNormalMatrix);
			}
		}
		RemoveAnnoationFromPage(base.Page, this);
	}
}
