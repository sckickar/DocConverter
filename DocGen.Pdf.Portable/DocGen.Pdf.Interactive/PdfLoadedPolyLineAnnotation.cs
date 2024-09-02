using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedPolyLineAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private LineBorder m_lineborder;

	private int[] m_points;

	private float[] m_point;

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

	internal int[] PolylinePoints
	{
		get
		{
			if (base.Dictionary.ContainsKey("Vertices") && PdfCrossTable.Dereference(base.Dictionary["Vertices"]) is PdfArray pdfArray)
			{
				m_points = new int[pdfArray.Count];
				int num = 0;
				foreach (PdfNumber item in pdfArray)
				{
					m_points[num] = item.IntValue;
					num++;
				}
			}
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			_ = base.LoadedPage.Size.Width;
			_ = base.LoadedPage.Size.Height;
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
			{
				PdfNumber pdfNumber2 = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber4 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber2 != null && pdfNumber3 != null && pdfNumber4 != null && (pdfNumber2.FloatValue != 0f || pdfNumber3.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					int[] points = m_points;
					float floatValue = pdfNumber2.FloatValue;
					float floatValue2 = pdfNumber3.FloatValue;
					for (int i = 0; i < m_points.Length; i += 2)
					{
						float num2 = points[i];
						float num3 = points[i + 1];
						m_points[i] = (int)(0f - floatValue + num2 + pdfMargins.Left);
						if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && cropOrMediaBox[3] is PdfNumber && pdfNumber4.FloatValue == 0f && pdfNumber3.FloatValue > 0f)
						{
							m_points[i + 1] = (int)(num3 - pdfNumber4.FloatValue - pdfMargins.Top);
						}
						else
						{
							m_points[i + 1] = (int)(num3 - floatValue2 - pdfMargins.Top);
						}
					}
				}
			}
			return m_points;
		}
		set
		{
			m_points = value;
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			_ = base.LoadedPage.Size.Width;
			_ = base.LoadedPage.Size.Height;
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten && base.Page != null && !base.Page.Annotations.Flatten)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					int[] points = m_points;
					for (int i = 0; i < m_points.Length; i += 2)
					{
						float num = points[i];
						float num2 = points[i + 1];
						m_points[i] = (int)(num + pdfNumber.FloatValue - pdfMargins.Left);
						if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber3.FloatValue == 0f && pdfNumber2.FloatValue > 0f)
						{
							m_points[i + 1] = (int)(num2 + pdfNumber3.FloatValue + pdfMargins.Top);
						}
						else
						{
							m_points[i + 1] = (int)(num2 + pdfNumber2.FloatValue + pdfMargins.Top);
						}
					}
				}
			}
			PdfArray primitive = new PdfArray(m_points);
			base.Dictionary.SetProperty("Vertices", primitive);
			NotifyPropertyChanged("PolyLinePoints");
		}
	}

	internal float[] PolylinePoint
	{
		get
		{
			if (base.Dictionary.ContainsKey("Vertices") && PdfCrossTable.Dereference(base.Dictionary["Vertices"]) is PdfArray pdfArray)
			{
				m_point = new float[pdfArray.Count];
				int num = 0;
				foreach (PdfNumber item in pdfArray)
				{
					m_point[num] = item.FloatValue;
					num++;
				}
			}
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			_ = base.LoadedPage.Size.Width;
			_ = base.LoadedPage.Size.Height;
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
			{
				PdfNumber pdfNumber2 = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber4 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber2 != null && pdfNumber3 != null && pdfNumber4 != null && (pdfNumber2.FloatValue != 0f || pdfNumber3.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					float[] point = m_point;
					float floatValue = pdfNumber2.FloatValue;
					float floatValue2 = pdfNumber3.FloatValue;
					for (int i = 0; i < m_point.Length; i += 2)
					{
						float num2 = point[i];
						float num3 = point[i + 1];
						m_point[i] = 0f - floatValue + num2 + pdfMargins.Left;
						if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber4.FloatValue == 0f && pdfNumber3.FloatValue > 0f)
						{
							m_point[i + 1] = num3 - pdfNumber4.FloatValue - pdfMargins.Top;
						}
						else
						{
							m_point[i + 1] = num3 - floatValue2 - pdfMargins.Top;
						}
					}
				}
			}
			return m_point;
		}
	}

	public LineBorder LineBorder
	{
		get
		{
			return AssignLineBorder();
		}
		set
		{
			m_lineborder = value;
			base.Dictionary.SetProperty("BS", m_lineborder);
			NotifyPropertyChanged("LineBorder");
		}
	}

	public PdfLineEndingStyle BeginLineStyle
	{
		get
		{
			return GetLineStyle(0);
		}
		set
		{
			PdfArray lineStyle = GetLineStyle();
			if (lineStyle == null)
			{
				lineStyle.Insert(1, new PdfName(PdfLineEndingStyle.Square));
			}
			else
			{
				lineStyle.RemoveAt(0);
			}
			lineStyle.Insert(0, new PdfName(GetLineStyle(value.ToString())));
			base.Dictionary.SetProperty("LE", lineStyle);
			NotifyPropertyChanged("BeginLineStyle");
		}
	}

	public PdfLineEndingStyle EndLineStyle
	{
		get
		{
			return GetLineStyle(1);
		}
		set
		{
			PdfArray lineStyle = GetLineStyle();
			if (lineStyle == null)
			{
				lineStyle.Insert(0, new PdfName(PdfLineEndingStyle.Square));
			}
			else
			{
				lineStyle.RemoveAt(1);
			}
			lineStyle.Insert(1, new PdfName(GetLineStyle(value.ToString())));
			base.Dictionary.SetProperty("LE", lineStyle);
			NotifyPropertyChanged("EndLineStyle");
		}
	}

	internal PdfLoadedPolyLineAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("Text must be not null");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		Text = text;
	}

	private PdfLineEndingStyle GetLineStyle(int Ch)
	{
		PdfLineEndingStyle result = PdfLineEndingStyle.None;
		PdfArray lineStyle = GetLineStyle();
		if (lineStyle != null)
		{
			PdfName pdfName = lineStyle[Ch] as PdfName;
			result = GetLineStyle(pdfName.Value);
		}
		return result;
	}

	private PdfLineEndingStyle GetLineStyle(string style)
	{
		PdfLineEndingStyle result = PdfLineEndingStyle.None;
		switch (style)
		{
		case "Square":
			result = PdfLineEndingStyle.Square;
			break;
		case "Circle":
			result = PdfLineEndingStyle.Circle;
			break;
		case "Diamond":
			result = PdfLineEndingStyle.Diamond;
			break;
		case "OpenArrow":
			result = PdfLineEndingStyle.OpenArrow;
			break;
		case "ClosedArrow":
			result = PdfLineEndingStyle.ClosedArrow;
			break;
		case "None":
			result = PdfLineEndingStyle.None;
			break;
		case "ROpenArrow":
			result = PdfLineEndingStyle.ROpenArrow;
			break;
		case "Butt":
			result = PdfLineEndingStyle.Butt;
			break;
		case "RClosedArrow":
			result = PdfLineEndingStyle.RClosedArrow;
			break;
		case "Slash":
			result = PdfLineEndingStyle.Slash;
			break;
		}
		return result;
	}

	private PdfArray GetLineStyle()
	{
		PdfArray result = null;
		if (base.Dictionary.ContainsKey("LE"))
		{
			result = m_crossTable.GetObject(base.Dictionary["LE"]) as PdfArray;
		}
		return result;
	}

	private PointF[] GetLinePoints()
	{
		PdfPageBase page = base.Page;
		if (base.Page.Annotations.Count > 0 && page.Annotations.Flatten)
		{
			base.Page.Annotations.Flatten = page.Annotations.Flatten;
		}
		PointF[] array = null;
		if (base.Dictionary.ContainsKey("Vertices"))
		{
			PdfCrossTable.Dereference(base.Dictionary["Vertices"]);
			if (PolylinePoint != null)
			{
				float[] polylinePoint = PolylinePoint;
				array = new PointF[polylinePoint.Length / 2];
				int num = 0;
				for (int i = 0; i < polylinePoint.Length; i += 2)
				{
					float height = base.Page.Size.Height;
					if (base.Flatten || base.Page.Annotations.Flatten)
					{
						array[num] = new PointF(polylinePoint[i], height - polylinePoint[i + 1]);
					}
					else
					{
						array[num] = new PointF(polylinePoint[i], 0f - polylinePoint[i + 1]);
					}
					num++;
				}
			}
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			_ = base.LoadedPage.Size.Width;
			_ = base.LoadedPage.Size.Height;
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten && base.Page != null && !base.Page.Annotations.Flatten)
			{
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					PointF[] array2 = array;
					for (int j = 0; j < array2.Length; j++)
					{
						float x = array2[j].X;
						float y = array2[j].Y;
						array[j].X = x + pdfNumber.FloatValue - pdfMargins.Left;
						if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber3.FloatValue == 0f && pdfNumber2.FloatValue > 0f)
						{
							array[j].Y = y - pdfNumber3.FloatValue - pdfMargins.Top;
						}
						else
						{
							array[j].Y = y - pdfNumber2.FloatValue - pdfMargins.Top;
						}
					}
				}
			}
		}
		return array;
	}

	private void GetBoundsValue()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (base.Dictionary.ContainsKey("Vertices") && PdfCrossTable.Dereference(base.Dictionary["Vertices"]) is PdfArray pdfArray)
		{
			int[] array = new int[pdfArray.Count];
			int num = 0;
			foreach (PdfNumber item in pdfArray)
			{
				array[num] = item.IntValue;
				num++;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (i % 2 == 0)
				{
					list.Add(array[i]);
				}
				else
				{
					list2.Add(array[i]);
				}
			}
		}
		list.Sort();
		list2.Sort();
		Bounds = new RectangleF(list[0], list2[0], list[list.Count - 1] - list[0], list2[list2.Count - 1] - list2[0]);
	}

	private PdfColor GetBackColor()
	{
		PdfColorSpace colorSpace = PdfColorSpace.RGB;
		PdfColor empty = PdfColor.Empty;
		PdfArray pdfArray = null;
		pdfArray = ((!base.Dictionary.ContainsKey("C")) ? empty.ToArray(colorSpace) : (PdfCrossTable.Dereference(base.Dictionary["C"]) as PdfArray));
		float floatValue = (pdfArray[0] as PdfNumber).FloatValue;
		float floatValue2 = (pdfArray[1] as PdfNumber).FloatValue;
		float floatValue3 = (pdfArray[2] as PdfNumber).FloatValue;
		return new PdfColor(floatValue, floatValue2, floatValue3);
	}

	private LineBorder AssignLineBorder()
	{
		LineBorder lineBorder = new LineBorder();
		if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("W"))
			{
				int intValue = (pdfDictionary["W"] as PdfNumber).IntValue;
				float floatValue = (pdfDictionary["W"] as PdfNumber).FloatValue;
				lineBorder.BorderWidth = intValue;
				lineBorder.BorderLineWidth = floatValue;
			}
			if (pdfDictionary.ContainsKey("S"))
			{
				PdfName pdfName = pdfDictionary["S"] as PdfName;
				lineBorder.BorderStyle = GetBorderStyle(pdfName.Value.ToString());
			}
			if (pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfArray { Count: >0 } pdfArray && pdfArray[0] is PdfNumber { IntValue: var intValue2 })
			{
				pdfArray.Clear();
				pdfArray.Insert(0, new PdfNumber(intValue2));
				pdfArray.Insert(1, new PdfNumber(intValue2));
				lineBorder.DashArray = intValue2;
			}
		}
		return lineBorder;
	}

	private new PdfBorderStyle GetBorderStyle(string bstyle)
	{
		PdfBorderStyle result = PdfBorderStyle.Solid;
		switch (bstyle)
		{
		case "S":
			result = PdfBorderStyle.Solid;
			break;
		case "D":
			result = PdfBorderStyle.Dashed;
			break;
		case "B":
			result = PdfBorderStyle.Beveled;
			break;
		case "I":
			result = PdfBorderStyle.Inset;
			break;
		case "U":
			result = PdfBorderStyle.Underline;
			break;
		}
		return result;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlattenPopUps)
	{
		int num = 0;
		if (base.Dictionary.ContainsKey("F"))
		{
			num = (base.Dictionary["F"] as PdfNumber).IntValue;
		}
		PdfPageBase page = base.Page;
		PdfGraphicsState pdfGraphicsState = null;
		PointF[] linePoints = GetLinePoints();
		byte[] array = new byte[linePoints.Length];
		array[0] = 0;
		for (int i = 1; i < linePoints.Length; i++)
		{
			array[i] = 1;
		}
		PdfPath path = new PdfPath(linePoints, array);
		RectangleF rectangleF = RectangleF.Empty;
		PdfGraphics pdfGraphics = ObtainlayerGraphics();
		if (base.SetAppearanceDictionary)
		{
			GetBoundsValue();
			rectangleF = new RectangleF(Bounds.X - Border.Width, Bounds.Y - Border.Width, Bounds.Width + 2f * Border.Width, Bounds.Height + 2f * Border.Width);
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Dictionary["AP"] != null)
			{
				base.Appearance.Normal = new PdfTemplate(rectangleF);
				base.Appearance.Normal.m_writeTransformation = false;
				PdfGraphics graphics = base.Appearance.Normal.Graphics;
				PdfPen pdfPen = null;
				if (Border.Width > 0f)
				{
					pdfPen = new PdfPen(Color, Border.Width);
				}
				if (base.Dictionary.ContainsKey("BS"))
				{
					PdfDictionary pdfDictionary = null;
					pdfDictionary = ((!(base.Dictionary.Items[new PdfName("BS")] is PdfReferenceHolder)) ? (base.Dictionary.Items[new PdfName("BS")] as PdfDictionary) : ((base.Dictionary.Items[new PdfName("BS")] as PdfReferenceHolder).Object as PdfDictionary));
					if (pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary.Items[new PdfName("D")]) is PdfArray pdfArray)
					{
						float[] array2 = new float[pdfArray.Count];
						for (int j = 0; j < pdfArray.Count; j++)
						{
							if (pdfArray.Elements[j] is PdfNumber)
							{
								array2[j] = (pdfArray.Elements[j] as PdfNumber).FloatValue;
							}
						}
						pdfPen.DashStyle = PdfDashStyle.Dash;
						pdfPen.isSkipPatternWidth = true;
						pdfPen.DashPattern = array2;
					}
				}
				if (base.Flatten || base.Page.Annotations.Flatten)
				{
					RemoveAnnoationFromPage(base.Page, this);
					if (Opacity < 1f)
					{
						pdfGraphicsState = page.Graphics.Save();
						page.Graphics.SetTransparency(Opacity);
						if (pdfGraphics != null)
						{
							pdfGraphics.DrawPath(pdfPen, path);
						}
						else
						{
							page.Graphics.DrawPath(pdfPen, path);
						}
						page.Graphics.Restore(pdfGraphicsState);
					}
					else if (pdfGraphics != null)
					{
						pdfGraphics.DrawPath(pdfPen, path);
					}
					else
					{
						base.Page.Graphics.DrawPath(pdfPen, path);
					}
				}
				else
				{
					PdfArray pdfArray2 = PdfCrossTable.Dereference(base.Dictionary["Vertices"]) as PdfArray;
					int num2 = 0;
					if (pdfArray2 != null)
					{
						int num3;
						for (num3 = 0; num3 < linePoints.Length; num3++)
						{
							linePoints[num2] = new PointF((pdfArray2.Elements[num3] as PdfNumber).FloatValue, 0f - (pdfArray2.Elements[num3 + 1] as PdfNumber).FloatValue);
							num3++;
							num2++;
						}
					}
					path = new PdfPath(linePoints, array);
					if (Opacity < 1f)
					{
						pdfGraphicsState = graphics.Save();
						graphics.SetTransparency(Opacity);
						graphics.DrawPath(pdfPen, path);
						graphics.Restore(pdfGraphicsState);
					}
					else
					{
						graphics.DrawPath(pdfPen, path);
					}
				}
				base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangleF));
			}
		}
		if (num != 2)
		{
			if ((base.Flatten && !base.SetAppearanceDictionary) || (base.Page.Annotations.Flatten && !base.SetAppearanceDictionary))
			{
				if (base.Dictionary["AP"] != null)
				{
					if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["N"]) is PdfDictionary pdfDictionary3 && pdfDictionary3 is PdfStream template)
					{
						PdfTemplate pdfTemplate = new PdfTemplate(template);
						if (pdfTemplate != null)
						{
							pdfGraphicsState = page.Graphics.Save();
							if (Opacity < 1f)
							{
								page.Graphics.SetTransparency(Opacity);
							}
							bool flag = ValidateTemplateMatrix(pdfDictionary3);
							if ((flag && base.Page.Rotation != 0) || IsValidTemplateMatrix(pdfDictionary3, Bounds.Location, pdfTemplate))
							{
								RectangleF rectangleF2 = CalculateTemplateBounds(Bounds, page, pdfTemplate, flag);
								if (!pdfTemplate.m_content.ContainsKey("Matrix") && PdfCrossTable.Dereference(pdfTemplate.m_content["BBox"]) is PdfArray pdfArray3)
								{
									float[] array3 = new float[6]
									{
										1f,
										0f,
										0f,
										1f,
										0f - (pdfArray3[0] as PdfNumber).FloatValue,
										0f - (pdfArray3[1] as PdfNumber).FloatValue
									};
									pdfTemplate.m_content["Matrix"] = new PdfArray(array3);
								}
								if (pdfGraphics != null)
								{
									pdfGraphics.DrawPdfTemplate(pdfTemplate, rectangleF2.Location, rectangleF2.Size);
								}
								else
								{
									base.Page.Graphics.DrawPdfTemplate(pdfTemplate, rectangleF2.Location, rectangleF2.Size);
								}
								page.Graphics.Restore(pdfGraphicsState);
								RemoveAnnoationFromPage(base.Page, this);
							}
							else
							{
								page.Graphics.Restore(pdfGraphicsState);
							}
							RemoveAnnoationFromPage(base.Page, this);
						}
					}
				}
				else
				{
					RemoveAnnoationFromPage(base.Page, this);
					PdfPen pen = new PdfPen(Color, Border.Width);
					if (Opacity < 1f)
					{
						pdfGraphicsState = page.Graphics.Save();
						page.Graphics.SetTransparency(Opacity);
						if (pdfGraphics != null)
						{
							pdfGraphics.DrawPath(pen, path);
						}
						else
						{
							page.Graphics.DrawPath(pen, path);
						}
						page.Graphics.Restore(pdfGraphicsState);
					}
					else if (pdfGraphics != null)
					{
						pdfGraphics.DrawPath(pen, path);
					}
					else
					{
						base.Page.Graphics.DrawPath(pen, path);
					}
				}
			}
		}
		else
		{
			RemoveAnnoationFromPage(base.Page, this);
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenLoadedPopup();
		}
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		PdfPageBase page = base.Page;
		if (base.Page.Annotations.Count > 0 && page.Annotations.Flatten)
		{
			base.Page.Annotations.Flatten = page.Annotations.Flatten;
		}
		SaveAndFlatten(isExternalFlattenPopUps: false);
	}
}
