using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedPolygonAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private LineBorder m_border = new LineBorder();

	private float[] m_dashPattern;

	private PdfBorderEffect m_borderEffect = new PdfBorderEffect();

	private float m_borderWidth;

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

	public PdfBorderEffect BorderEffect
	{
		get
		{
			return m_borderEffect;
		}
		set
		{
			m_borderEffect = value;
			base.Dictionary.SetProperty("BE", m_borderEffect);
			NotifyPropertyChanged("BorderEffect");
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

	public new LineBorder Border
	{
		get
		{
			m_border = GetLineBorder();
			return m_border;
		}
		set
		{
			m_border = value;
			base.Dictionary.SetProperty("BS", m_border);
			NotifyPropertyChanged("Border");
		}
	}

	public int[] PolygonPoints
	{
		get
		{
			if (base.Dictionary.ContainsKey("Vertices") && base.Dictionary["Vertices"] is PdfArray pdfArray)
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
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten && base.Page != null && !base.Page.Annotations.Flatten)
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
						if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber4.FloatValue == 0f && pdfNumber3.FloatValue > 0f)
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
		internal set
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
			NotifyPropertyChanged("PolygonPoints");
		}
	}

	internal float[] PolygonPoint
	{
		get
		{
			if (base.Dictionary.ContainsKey("Vertices") && base.Dictionary["Vertices"] is PdfArray pdfArray)
			{
				m_point = new float[pdfArray.Count];
				int num = 0;
				foreach (PdfNumber item in pdfArray)
				{
					m_point[num] = item.FloatValue;
					num++;
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
			}
			return m_point;
		}
	}

	internal PdfLoadedPolygonAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("Text must be not null");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_text = text;
		m_borderEffect = new PdfBorderEffect(dictionary);
	}

	private new LineBorder GetLineBorder()
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
			if (pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfArray { Count: >0 } pdfArray)
			{
				if (m_dashPattern == null)
				{
					m_dashPattern = new float[pdfArray.Count];
					for (int i = 0; i < pdfArray.Count; i++)
					{
						if (pdfArray.Elements[i] is PdfNumber)
						{
							m_dashPattern[i] = (pdfArray.Elements[i] as PdfNumber).FloatValue;
						}
					}
				}
				int intValue2 = (pdfArray[0] as PdfNumber).IntValue;
				pdfArray.Clear();
				pdfArray.Insert(0, new PdfNumber(intValue2));
				pdfArray.Insert(1, new PdfNumber(intValue2));
				lineBorder.DashArray = intValue2;
			}
		}
		else
		{
			base.Dictionary.SetProperty("BS", lineBorder);
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

	private PointF[] GetLinePoints()
	{
		PdfPageBase page = base.Page;
		if (base.Page.Annotations.Count > 0 && page.Annotations.Flatten)
		{
			base.Page.Annotations.Flatten = page.Annotations.Flatten;
		}
		PointF[] array = null;
		PdfNumber pdfNumber = null;
		if (base.Dictionary.ContainsKey("Vertices"))
		{
			float height = base.Page.Size.Height;
			float width = base.Page.Size.Width;
			if (page != null && page.Dictionary.ContainsKey("Rotate"))
			{
				pdfNumber = page.Dictionary["Rotate"] as PdfNumber;
			}
			_ = page.Rotation;
			if (page.Rotation != 0)
			{
				if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					pdfNumber = new PdfNumber(90);
				}
				else if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					pdfNumber = new PdfNumber(180);
				}
				else if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					pdfNumber = new PdfNumber(270);
				}
			}
			_ = base.Dictionary["Vertices"];
			if (PolygonPoints != null)
			{
				int[] polygonPoints = PolygonPoints;
				array = new PointF[polygonPoints.Length / 2];
				int num = 0;
				for (int i = 0; i < polygonPoints.Length; i += 2)
				{
					if (base.Flatten || base.Page.Annotations.Flatten)
					{
						array[num] = new PointF(polygonPoints[i], height - (float)polygonPoints[i + 1]);
					}
					else
					{
						array[num] = new PointF(polygonPoints[i], -polygonPoints[i + 1]);
					}
					num++;
				}
				if (pdfNumber != null && (base.Flatten || base.Page.Annotations.Flatten))
				{
					if (pdfNumber.IntValue == 270)
					{
						for (int j = 0; j < array.Length; j++)
						{
							float x = array[j].X;
							array[j].X = array[j].Y;
							array[j].Y = width - x;
						}
					}
					else if (pdfNumber.IntValue == 90)
					{
						for (int k = 0; k < array.Length; k++)
						{
							float x2 = array[k].X;
							if ((base.Flatten || base.Page.Annotations.Flatten) & (base.Page.Origin.Y != 0f))
							{
								array[k].X = height - (array[k].Y - height);
							}
							else
							{
								array[k].X = height - array[k].Y;
							}
							array[k].Y = x2;
						}
					}
					else if (pdfNumber.IntValue == 180)
					{
						for (int l = 0; l < array.Length; l++)
						{
							float x3 = array[l].X;
							array[l].X = width - x3;
							array[l].Y = height - array[l].Y;
						}
					}
				}
			}
			PdfArray cropOrMediaBox = null;
			cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
			_ = base.LoadedPage.Size.Width;
			_ = base.LoadedPage.Size.Height;
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten && base.Page != null && !base.Page.Annotations.Flatten)
			{
				PdfNumber pdfNumber2 = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber4 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber2 != null && pdfNumber3 != null && pdfNumber4 != null && (pdfNumber2.FloatValue != 0f || pdfNumber3.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					PointF[] array2 = array;
					for (int m = 0; m < array2.Length; m++)
					{
						float x4 = array2[m].X;
						float y = array2[m].Y;
						array[m].X = x4 + pdfNumber2.FloatValue - pdfMargins.Left;
						if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber4.FloatValue == 0f && pdfNumber3.FloatValue > 0f)
						{
							array[m].Y = y - pdfNumber4.FloatValue - pdfMargins.Top;
						}
						else
						{
							array[m].Y = y - pdfNumber3.FloatValue - pdfMargins.Top;
						}
					}
				}
			}
		}
		return array;
	}

	private void GetBoundsValue()
	{
		PdfArray pdfArray = base.Dictionary["Rect"] as PdfArray;
		Bounds = pdfArray.ToRectangle();
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		if (base.Dictionary.ContainsKey("Vertices"))
		{
			PdfArray pdfArray2 = PdfCrossTable.Dereference(base.Dictionary["Vertices"]) as PdfArray;
			_ = pdfArray2.Count;
			if (pdfArray2.Count > 0)
			{
				float[] array = new float[pdfArray2.Count];
				int num = 0;
				foreach (PdfNumber item in pdfArray2)
				{
					array[num] = item.FloatValue;
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
		}
		list.Sort();
		list2.Sort();
		Bounds = new RectangleF(list[0], list2[0], list[list.Count - 1] - list[0], list2[list2.Count - 1] - list2[0]);
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
		if (Border.BorderWidth == 1 || Border.BorderLineWidth != 0f)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		PdfPageBase page = base.Page;
		PdfGraphicsState state = null;
		RectangleF rectangleF = RectangleF.Empty;
		PdfGraphics pdfGraphics = null;
		PdfGraphics pdfGraphics2 = null;
		if (base.SetAppearanceDictionary)
		{
			pdfGraphics = base.Page.Graphics;
			pdfGraphics2 = ObtainlayerGraphics();
			if (pdfGraphics2 != null)
			{
				pdfGraphics = pdfGraphics2;
			}
			GetBoundsValue();
			rectangleF = ((BorderEffect.Intensity == 0f || BorderEffect.Style != PdfBorderEffectStyle.Cloudy) ? new RectangleF(Bounds.X - m_borderWidth, Bounds.Y - m_borderWidth, Bounds.Width + 2f * m_borderWidth, Bounds.Height + 2f * m_borderWidth) : new RectangleF(Bounds.X - BorderEffect.Intensity * 5f - m_borderWidth, Bounds.Y - BorderEffect.Intensity * 5f - m_borderWidth, Bounds.Width + BorderEffect.Intensity * 10f + 2f * m_borderWidth, Bounds.Height + BorderEffect.Intensity * 10f + 2f * m_borderWidth));
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Dictionary["AP"] != null)
			{
				base.Appearance.Normal = new PdfTemplate(rectangleF);
				PaintParams paintParams = new PaintParams();
				base.Appearance.Normal.m_writeTransformation = false;
				PdfGraphics graphics = base.Appearance.Normal.Graphics;
				PdfBrush pdfBrush = null;
				if (InnerColor.A != 0)
				{
					pdfBrush = new PdfSolidBrush(InnerColor);
				}
				paintParams.BackBrush = pdfBrush;
				PdfPen pdfPen = null;
				if (m_borderWidth > 0f)
				{
					pdfPen = new PdfPen(Color, m_borderWidth);
				}
				paintParams.BorderPen = pdfPen;
				if (base.Dictionary.ContainsKey("BS"))
				{
					PdfDictionary pdfDictionary = null;
					pdfDictionary = ((!(base.Dictionary.Items[new PdfName("BS")] is PdfReferenceHolder)) ? (base.Dictionary.Items[new PdfName("BS")] as PdfDictionary) : ((base.Dictionary.Items[new PdfName("BS")] as PdfReferenceHolder).Object as PdfDictionary));
					if (pdfDictionary.ContainsKey("D"))
					{
						PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary.Items[new PdfName("D")]) as PdfArray;
						float[] array = new float[pdfArray.Count];
						for (int i = 0; i < pdfArray.Count; i++)
						{
							array[i] = (pdfArray.Elements[i] as PdfNumber).FloatValue;
						}
						pdfPen.DashStyle = PdfDashStyle.Dash;
						pdfPen.isSkipPatternWidth = true;
						pdfPen.DashPattern = array;
						pdfPen.DashPattern = m_dashPattern;
					}
				}
				if (base.Flatten || base.Page.Annotations.Flatten)
				{
					RemoveAnnoationFromPage(base.Page, this);
					if (Opacity < 1f)
					{
						state = pdfGraphics.Save();
						pdfGraphics.SetTransparency(Opacity);
					}
					if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
					{
						CalculateBounds(Bounds, null, base.Page);
						float num2 = BorderEffect.Intensity * 4f + 0.5f * m_borderWidth;
						if (num2 > 0f)
						{
							PdfPath pdfPath = new PdfPath();
							pdfPath.AddPolygon(GetLinePoints());
							if (pdfPath.PathPoints[0].Y > pdfPath.PathPoints[pdfPath.PathPoints.Length - 1].Y)
							{
								DrawCloudStyle(graphics, pdfBrush, pdfPen, num2, 0.833f, pdfPath.PathPoints, isAppearance: false);
							}
							if (pdfGraphics2 != null)
							{
								DrawCloudStyle(pdfGraphics2, pdfBrush, pdfPen, num2, 0.833f, pdfPath.PathPoints, isAppearance: false);
							}
							else
							{
								DrawCloudStyle(page.Graphics, pdfBrush, pdfPen, num2, 0.833f, pdfPath.PathPoints, isAppearance: false);
							}
						}
						else if (pdfGraphics2 != null)
						{
							pdfGraphics2.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
						}
						else
						{
							base.Page.Graphics.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
						}
					}
					else if (pdfGraphics2 != null)
					{
						pdfGraphics2.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
					}
					else
					{
						base.Page.Graphics.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
					}
					if (Opacity < 1f)
					{
						page.Graphics.Restore(state);
					}
				}
				else
				{
					if (Opacity < 1f)
					{
						state = graphics.Save();
						graphics.SetTransparency(Opacity);
					}
					if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
					{
						CalculateBounds(Bounds, null, base.Page);
						float radius = BorderEffect.Intensity * 4f + 0.5f * m_borderWidth;
						_ = new PointF[(base.Dictionary["Vertices"] as PdfArray).Count / 2];
						PdfPath pdfPath2 = new PdfPath();
						pdfPath2.AddPolygon(GetLinePoints());
						if (pdfPath2.PathPoints[0].Y > pdfPath2.PathPoints[pdfPath2.PathPoints.Length - 1].Y)
						{
							PointF[] array2 = new PointF[pdfPath2.PathPoints.Length];
							for (int j = 0; j < pdfPath2.PathPoints.Length; j++)
							{
								array2[j] = new PointF(pdfPath2.PathPoints[j].X, pdfPath2.PathPoints[j].Y);
							}
							pdfPath2 = new PdfPath();
							pdfPath2.AddPolygon(array2);
							DrawCloudStyle(graphics, pdfBrush, pdfPen, radius, 0.833f, pdfPath2.PathPoints, isAppearance: false);
						}
						else
						{
							DrawCloudStyle(graphics, pdfBrush, pdfPen, radius, 0.833f, pdfPath2.PathPoints, isAppearance: false);
						}
					}
					else
					{
						graphics.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
					}
					if (Opacity < 1f)
					{
						graphics.Restore(state);
					}
				}
				base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangleF));
			}
		}
		if (num != 2)
		{
			if ((base.Flatten && !base.SetAppearanceDictionary) || (base.Page.Annotations.Flatten && !base.SetAppearanceDictionary))
			{
				pdfGraphics = base.Page.Graphics;
				pdfGraphics2 = ObtainlayerGraphics();
				if (pdfGraphics2 != null)
				{
					pdfGraphics = pdfGraphics2;
				}
				if (base.Dictionary["AP"] != null)
				{
					if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary2 && PdfCrossTable.Dereference(pdfDictionary2["N"]) is PdfDictionary pdfDictionary3 && pdfDictionary3 is PdfStream template)
					{
						PdfTemplate pdfTemplate = new PdfTemplate(template);
						if (pdfTemplate != null)
						{
							state = pdfGraphics.Save();
							if (Opacity < 1f)
							{
								pdfGraphics.SetTransparency(Opacity);
							}
							bool flag = ValidateTemplateMatrix(pdfDictionary3);
							RectangleF rectangleF2 = CalculateTemplateBounds(Bounds, page, pdfTemplate, flag, pdfGraphics);
							if (!pdfTemplate.m_content.ContainsKey("Matrix") && PdfCrossTable.Dereference(pdfTemplate.m_content["BBox"]) is PdfArray pdfArray2)
							{
								float[] array3 = new float[6]
								{
									1f,
									0f,
									0f,
									1f,
									0f - (pdfArray2[0] as PdfNumber).FloatValue,
									0f - (pdfArray2[1] as PdfNumber).FloatValue
								};
								pdfTemplate.m_content["Matrix"] = new PdfArray(array3);
							}
							bool flag2 = true;
							if (pdfTemplate.m_content.ContainsKey("Matrix") && ((PdfCrossTable.Dereference(pdfTemplate.m_content["Matrix"]) as PdfArray).Elements[3] as PdfNumber).IntValue != 1 && base.Rotate == PdfAnnotationRotateAngle.RotateAngle0)
							{
								flag2 = false;
							}
							if (!flag && flag2)
							{
								pdfTemplate.IsAnnotationTemplate = true;
								pdfTemplate.NeedScaling = true;
							}
							pdfGraphics.DrawPdfTemplate(pdfTemplate, rectangleF2.Location, rectangleF2.Size);
							pdfGraphics.Restore(state);
							RemoveAnnoationFromPage(base.Page, this);
						}
					}
				}
				else
				{
					RemoveAnnoationFromPage(base.Page, this);
					PdfPen pen = new PdfPen(Color, m_borderWidth);
					PdfColor innerColor = InnerColor;
					PdfBrush brush = (innerColor.IsEmpty ? null : new PdfSolidBrush(innerColor));
					if (Opacity < 1f)
					{
						state = pdfGraphics.Save();
						pdfGraphics.SetTransparency(Opacity);
					}
					if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
					{
						CalculateBounds(Bounds, null, base.Page);
						float radius2 = BorderEffect.Intensity * 4f + 0.5f * m_borderWidth;
						PdfPath pdfPath3 = new PdfPath();
						pdfPath3.AddPolygon(GetLinePoints());
						if (pdfGraphics2 != null)
						{
							DrawCloudStyle(pdfGraphics2, brush, pen, radius2, 0.833f, pdfPath3.PathPoints, isAppearance: false);
						}
						else
						{
							DrawCloudStyle(page.Graphics, brush, pen, radius2, 0.833f, pdfPath3.PathPoints, isAppearance: false);
						}
					}
					else if (pdfGraphics2 != null)
					{
						pdfGraphics2.DrawPolygon(pen, brush, GetLinePoints());
					}
					else
					{
						base.Page.Graphics.DrawPolygon(pen, brush, GetLinePoints());
					}
					if (Opacity < 1f)
					{
						pdfGraphics.Restore(state);
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
