using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfPolygonAnnotation : PdfAnnotation
{
	private LineBorder m_border = new LineBorder();

	internal PdfArray m_linePoints;

	private int m_lineExtension;

	private PdfBorderEffect m_borderEffect = new PdfBorderEffect();

	private float m_borderWidth;

	private int[] m_points;

	private float[] m_point;

	public PdfPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory != null)
			{
				return m_reviewHistory;
			}
			return m_reviewHistory = new PdfPopupAnnotationCollection(this, isReview: true);
		}
	}

	public PdfPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments != null)
			{
				return m_comments;
			}
			return m_comments = new PdfPopupAnnotationCollection(this, isReview: false);
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
			NotifyPropertyChanged("BorderEffect");
		}
	}

	public new LineBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			m_border = value;
			NotifyPropertyChanged("Border");
		}
	}

	public int LineExtension
	{
		get
		{
			return m_lineExtension;
		}
		set
		{
			m_lineExtension = value;
			NotifyPropertyChanged("LineExtension");
		}
	}

	internal int[] PolygonPoints
	{
		get
		{
			return m_points;
		}
		set
		{
			m_points = value;
			m_linePoints = new PdfArray(m_points);
			NotifyPropertyChanged("PolygonPoints");
		}
	}

	internal float[] PolygonPoint
	{
		get
		{
			return m_point;
		}
		set
		{
			m_point = value;
			m_linePoints = new PdfArray(m_point);
			NotifyPropertyChanged("PolygonPoint");
		}
	}

	public PdfPolygonAnnotation(int[] points, string text)
	{
		m_linePoints = new PdfArray(points);
		m_points = points;
		Text = text;
	}

	internal PdfPolygonAnnotation(float[] linePoints, string text)
	{
		m_linePoints = new PdfArray(linePoints);
		Text = text;
		m_point = linePoints;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Polygon"));
	}

	private PointF[] GetLinePoints()
	{
		PointF[] array = null;
		if (m_linePoints != null)
		{
			int[] array2 = new int[m_linePoints.Count];
			int num = 0;
			foreach (PdfNumber linePoint in m_linePoints)
			{
				array2[num] = linePoint.IntValue;
				num++;
			}
			array = new PointF[array2.Length / 2];
			int num2 = 0;
			for (int i = 0; i < array2.Length; i += 2)
			{
				float num3 = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
				if (base.Flatten)
				{
					if (base.Page != null)
					{
						array[num2] = new PointF((float)array2[i] - base.Page.m_section.PageSettings.Margins.Left, num3 - (float)array2[i + 1] - base.Page.m_section.PageSettings.Margins.Right);
					}
					else
					{
						array[num2] = new PointF(array2[i], num3 - (float)array2[i + 1]);
					}
				}
				else
				{
					array[num2] = new PointF(array2[i], -array2[i + 1]);
				}
				num2++;
			}
		}
		return array;
	}

	private void GetBoundsValue()
	{
		_ = m_linePoints.Count;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (m_linePoints.Count > 0)
		{
			int[] array = new int[m_linePoints.Count];
			int num = 0;
			foreach (PdfNumber linePoint in m_linePoints)
			{
				array[num] = linePoint.IntValue;
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
		if (!base.Flatten)
		{
			Bounds = new RectangleF(list[0], list2[0], list[list.Count - 1] - list[0], list2[list2.Count - 1] - list2[0]);
		}
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		PdfPageBase pdfPageBase = ((base.Page == null) ? ((PdfPageBase)base.LoadedPage) : ((PdfPageBase)base.Page));
		RectangleF rectangleF = RectangleF.Empty;
		PdfGraphics layerGraphics = GetLayerGraphics();
		List<float> list = new List<float>();
		foreach (PdfNumber linePoint in m_linePoints)
		{
			list.Add(linePoint.FloatValue);
		}
		list.ToArray();
		PdfArray cropOrMediaBox = null;
		if (m_linePoints != null)
		{
			float[] array = new float[m_linePoints.Count];
			int num = 0;
			foreach (PdfNumber linePoint2 in m_linePoints)
			{
				array[num] = linePoint2.IntValue;
				num++;
			}
			cropOrMediaBox = GetCropOrMediaBox(pdfPageBase, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
			{
				if ((cropOrMediaBox[3] as PdfNumber).FloatValue < 0f)
				{
					float floatValue = (cropOrMediaBox[1] as PdfNumber).FloatValue;
					float floatValue2 = (cropOrMediaBox[3] as PdfNumber).FloatValue;
					(cropOrMediaBox[1] as PdfNumber).FloatValue = floatValue2;
					(cropOrMediaBox[3] as PdfNumber).FloatValue = floatValue;
				}
				PdfNumber pdfNumber3 = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber4 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber5 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber3 != null && pdfNumber4 != null && pdfNumber5 != null && (pdfNumber3.FloatValue != 0f || pdfNumber4.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					float[] array2 = array;
					for (int i = 0; i < array2.Length; i += 2)
					{
						float num2 = array2[i];
						float num3 = array2[i + 1];
						if (cropOrMediaBox != null)
						{
							array[i] = num2 + pdfNumber3.FloatValue - pdfMargins.Left;
							if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber5.FloatValue == 0f && pdfNumber4.FloatValue > 0f)
							{
								array[i + 1] = num3 + pdfNumber5.FloatValue + pdfMargins.Top;
							}
							else
							{
								array[i + 1] = num3 + pdfNumber4.FloatValue + pdfMargins.Top;
							}
						}
					}
					m_linePoints = new PdfArray(array);
				}
			}
		}
		if (list[0] != list[list.Count - 2] || list[1] != list[list.Count - 1])
		{
			m_linePoints.Add(m_linePoints[0]);
			m_linePoints.Add(m_linePoints[1]);
		}
		if (!m_isStandardAppearance && !base.SetAppearanceDictionary)
		{
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Flatten)
			{
				PdfTemplate normal = base.Appearance.Normal;
				if (normal != null)
				{
					FlattenAnnotation(pdfPageBase, normal);
				}
			}
		}
		if (base.SetAppearanceDictionary)
		{
			isPropertyChanged = false;
			GetBoundsValue();
			isPropertyChanged = true;
			rectangleF = ((BorderEffect.Intensity == 0f || BorderEffect.Style != PdfBorderEffectStyle.Cloudy) ? new RectangleF(Bounds.X - m_borderWidth, Bounds.Y - m_borderWidth, Bounds.Width + 2f * m_borderWidth, Bounds.Height + 2f * m_borderWidth) : new RectangleF(Bounds.X - BorderEffect.Intensity * 5f - m_borderWidth, Bounds.Y - BorderEffect.Intensity * 5f - m_borderWidth, Bounds.Width + BorderEffect.Intensity * 10f + 2f * m_borderWidth, Bounds.Height + BorderEffect.Intensity * 10f + 2f * m_borderWidth));
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Dictionary["AP"] != null)
			{
				base.Appearance.Normal = new PdfTemplate(rectangleF);
				PdfTemplate normal2 = base.Appearance.Normal;
				PaintParams paintParams = new PaintParams();
				normal2.m_writeTransformation = false;
				PdfGraphics graphics = base.Appearance.Normal.Graphics;
				PdfBrush pdfBrush = (InnerColor.IsEmpty ? null : new PdfSolidBrush(InnerColor));
				PdfPen pdfPen = null;
				if (m_borderWidth > 0f && Color.A != 0)
				{
					pdfPen = new PdfPen(Color, m_borderWidth);
				}
				paintParams.BackBrush = pdfBrush;
				paintParams.BorderPen = pdfPen;
				if (base.Flatten)
				{
					if (Opacity < 1f)
					{
						pdfPageBase.Graphics.SetTransparency(Opacity);
					}
					RemoveAnnoationFromPage(pdfPageBase, this);
					if (layerGraphics != null)
					{
						layerGraphics.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
					}
					else if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
					{
						FieldPainter.DrawPolygonCloud(pdfPageBase.Graphics, paintParams.BorderPen, paintParams.BackBrush, BorderEffect.Intensity, GetLinePoints(), m_borderWidth);
					}
					else
					{
						pdfPageBase.Graphics.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
					}
					if (Opacity < 1f)
					{
						pdfPageBase.Graphics.Restore();
					}
				}
				else
				{
					if (Opacity < 1f)
					{
						graphics.Save();
						graphics.SetTransparency(Opacity);
					}
					if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
					{
						FieldPainter.DrawPolygonCloud(graphics, paintParams.BorderPen, paintParams.BackBrush, BorderEffect.Intensity, GetLinePoints(), m_borderWidth);
					}
					else
					{
						graphics.DrawPolygon(pdfPen, pdfBrush, GetLinePoints());
					}
					if (Opacity < 1f)
					{
						graphics.Restore();
					}
				}
			}
		}
		if (base.Flatten && !base.SetAppearanceDictionary && m_isStandardAppearance)
		{
			RemoveAnnoationFromPage(pdfPageBase, this);
			PdfPen pdfPen2 = null;
			if (m_borderWidth > 0f && Color.A != 0)
			{
				pdfPen2 = new PdfPen(Color, m_borderWidth);
			}
			PdfBrush pdfBrush2 = (InnerColor.IsEmpty ? null : new PdfSolidBrush(InnerColor));
			if (Opacity < 1f)
			{
				pdfPageBase.Graphics.SetTransparency(Opacity);
			}
			if (layerGraphics != null)
			{
				layerGraphics.DrawPolygon(pdfPen2, pdfBrush2, GetLinePoints());
			}
			else if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
			{
				FieldPainter.DrawPolygonCloud(pdfPageBase.Graphics, pdfPen2, pdfBrush2, BorderEffect.Intensity, GetLinePoints(), m_borderWidth);
			}
			else
			{
				pdfPageBase.Graphics.DrawPolygon(pdfPen2, pdfBrush2, GetLinePoints());
			}
			if (Opacity < 1f)
			{
				pdfPageBase.Graphics.Restore();
			}
		}
		else if (!isExternalFlatten && !base.Flatten)
		{
			base.Save();
			base.Dictionary.SetProperty("Vertices", new PdfArray(m_linePoints));
			if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
			{
				base.Dictionary.SetProperty("BE", m_borderEffect);
				base.Dictionary.SetProperty("BS", m_border);
				if (base.Dictionary["BS"] is PdfDictionary pdfDictionary)
				{
					if (pdfDictionary.ContainsKey("S"))
					{
						pdfDictionary.Remove("S");
					}
					if (pdfDictionary.ContainsKey("D"))
					{
						pdfDictionary.Remove("D");
					}
				}
			}
			else
			{
				base.Dictionary.SetProperty("BS", m_border);
			}
			isPropertyChanged = false;
			GetBoundsValue();
			isPropertyChanged = true;
			base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(Bounds));
			base.Dictionary.SetProperty("LLE", new PdfNumber(m_lineExtension));
			if (base.SetAppearanceDictionary)
			{
				base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangleF));
			}
		}
		if (base.Page != null && base.Popup != null && base.Flatten)
		{
			RemoveAnnoationFromPage(base.Page, base.Popup);
		}
		else if (base.LoadedPage != null && base.Popup != null && base.Flatten)
		{
			RemoveAnnoationFromPage(base.LoadedPage, base.Popup);
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		RectangleF bounds = RectangleF.Empty;
		if (appearance.m_content.ContainsKey("BBox"))
		{
			bounds = (appearance.m_content["BBox"] as PdfArray).ToRectangle();
			bounds.Y = base.Page.Size.Height - (bounds.Y + bounds.Height);
		}
		PdfGraphics pdfGraphics = page.Graphics;
		PdfGraphics layerGraphics = GetLayerGraphics();
		if (layerGraphics != null)
		{
			pdfGraphics = layerGraphics;
		}
		PdfGraphicsState state = pdfGraphics.Save();
		if (Opacity < 1f)
		{
			pdfGraphics.SetTransparency(Opacity);
		}
		bool isNormalMatrix = ValidateTemplateMatrix(appearance.m_content);
		RectangleF rectangleF = CalculateTemplateBounds(bounds, page, appearance, isNormalMatrix, pdfGraphics);
		pdfGraphics.DrawPdfTemplate(appearance, rectangleF.Location, rectangleF.Size);
		pdfGraphics.Restore(state);
		RemoveAnnoationFromPage(base.Page, this);
	}
}
