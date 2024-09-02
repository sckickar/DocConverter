using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfPolyLineAnnotation : PdfAnnotation
{
	private LineBorder m_border;

	internal PdfArray m_linePoints;

	private int m_lineExtension;

	internal PdfArray m_lineStyle;

	private PdfLineEndingStyle m_beginLine;

	private PdfLineEndingStyle m_endLine;

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

	public new LineBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = new LineBorder();
			}
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

	public PdfLineEndingStyle BeginLineStyle
	{
		get
		{
			return m_beginLine;
		}
		set
		{
			if (m_beginLine != value)
			{
				m_beginLine = value;
			}
			NotifyPropertyChanged("BeginLineStyle");
		}
	}

	public PdfLineEndingStyle EndLineStyle
	{
		get
		{
			return m_endLine;
		}
		set
		{
			if (m_endLine != value)
			{
				m_endLine = value;
			}
			NotifyPropertyChanged("EndLineStyle");
		}
	}

	internal int[] PolylinePoints
	{
		get
		{
			return m_points;
		}
		set
		{
			m_points = value;
			m_linePoints = new PdfArray(m_points);
			NotifyPropertyChanged("LinePoints");
		}
	}

	internal float[] PolylinePoint
	{
		get
		{
			return m_point;
		}
		set
		{
			m_point = value;
			m_linePoints = new PdfArray(m_point);
		}
	}

	public PdfPolyLineAnnotation(int[] points, string text)
	{
		m_linePoints = new PdfArray(points);
		m_points = points;
		Text = text;
	}

	internal PdfPolyLineAnnotation(float[] points, string text)
	{
		m_linePoints = new PdfArray(points);
		m_point = points;
		Text = text;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("PolyLine"));
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
			PdfArray cropOrMediaBox = null;
			PdfPageBase page = ((base.Page == null) ? ((PdfPageBase)base.LoadedPage) : ((PdfPageBase)base.Page));
			cropOrMediaBox = GetCropOrMediaBox(page, cropOrMediaBox);
			if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
			{
				if ((cropOrMediaBox[3] as PdfNumber).FloatValue < 0f)
				{
					float floatValue = (cropOrMediaBox[1] as PdfNumber).FloatValue;
					float floatValue2 = (cropOrMediaBox[3] as PdfNumber).FloatValue;
					(cropOrMediaBox[1] as PdfNumber).FloatValue = floatValue2;
					(cropOrMediaBox[3] as PdfNumber).FloatValue = floatValue;
				}
				PdfNumber pdfNumber2 = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber4 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber2 != null && pdfNumber3 != null && pdfNumber4 != null && (pdfNumber2.FloatValue != 0f || pdfNumber3.FloatValue != 0f))
				{
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					int[] array3 = array2;
					for (int i = 0; i < array3.Length; i += 2)
					{
						float num3 = array3[i];
						float num4 = array3[i + 1];
						if (cropOrMediaBox != null)
						{
							array2[i] = (int)(num3 + pdfNumber2.FloatValue - pdfMargins.Left);
							if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfNumber4.FloatValue == 0f && pdfNumber3.FloatValue > 0f)
							{
								array2[i + 1] = (int)(num4 + pdfNumber4.FloatValue + pdfMargins.Top);
							}
							else
							{
								array2[i + 1] = (int)(num4 + pdfNumber3.FloatValue + pdfMargins.Top);
							}
						}
					}
					m_linePoints = new PdfArray(array2);
				}
			}
			for (int j = 0; j < array2.Length; j += 2)
			{
				float num5 = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
				if (base.Flatten)
				{
					if (base.Page != null)
					{
						array[num2] = new PointF((float)array2[j] - base.Page.m_section.PageSettings.Margins.Left, num5 - (float)array2[j + 1] - base.Page.m_section.PageSettings.Margins.Right);
					}
					else
					{
						array[num2] = new PointF(array2[j], num5 - (float)array2[j + 1]);
					}
				}
				else
				{
					array[num2] = new PointF(array2[j], -array2[j + 1]);
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
		PdfPageBase pdfPageBase = ((base.Page == null) ? ((PdfPageBase)base.LoadedPage) : ((PdfPageBase)base.Page));
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		PdfGraphics layerGraphics = GetLayerGraphics();
		RectangleF rectangleF = RectangleF.Empty;
		PointF[] linePoints = GetLinePoints();
		byte[] array = new byte[linePoints.Length];
		array[0] = 0;
		for (int i = 1; i < linePoints.Length; i++)
		{
			array[i] = 1;
		}
		PdfPath path = new PdfPath(linePoints, array);
		if (!isExternalFlatten)
		{
			base.Save();
			base.Dictionary.SetProperty("Vertices", new PdfArray(m_linePoints));
			m_lineStyle = new PdfArray();
			m_lineStyle.Insert(0, new PdfName(BeginLineStyle));
			m_lineStyle.Insert(1, new PdfName(EndLineStyle));
			base.Dictionary.SetProperty("LE", m_lineStyle);
			base.Dictionary.SetProperty("BS", m_border);
			base.Dictionary.SetProperty("LLE", new PdfNumber(m_lineExtension));
		}
		if (base.SetAppearanceDictionary)
		{
			isPropertyChanged = false;
			GetBoundsValue();
			isPropertyChanged = true;
			rectangleF = new RectangleF(Bounds.X - m_borderWidth, Bounds.Y - m_borderWidth, Bounds.Width + 2f * m_borderWidth, Bounds.Height + 2f * m_borderWidth);
			base.Dictionary.SetProperty("AP", base.Appearance);
			if (base.Dictionary["AP"] != null)
			{
				base.Appearance.Normal = new PdfTemplate(rectangleF);
				PdfTemplate normal = base.Appearance.Normal;
				PaintParams paintParams = new PaintParams();
				normal.m_writeTransformation = false;
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
						pdfPageBase.Graphics.Save();
						pdfPageBase.Graphics.SetTransparency(Opacity);
					}
					RemoveAnnoationFromPage(pdfPageBase, this);
					if (layerGraphics != null)
					{
						layerGraphics.DrawPath(pdfPen, pdfBrush, path);
					}
					else
					{
						pdfPageBase.Graphics.DrawPath(pdfPen, pdfBrush, path);
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
					graphics.DrawPath(pdfPen, pdfBrush, path);
					if (Opacity < 1f)
					{
						graphics.Restore();
					}
				}
			}
		}
		if (base.Flatten && !base.SetAppearanceDictionary)
		{
			RemoveAnnoationFromPage(pdfPageBase, this);
			PdfPen pen = null;
			if (m_borderWidth > 0f && Color.A != 0)
			{
				pen = new PdfPen(Color, m_borderWidth);
			}
			PdfBrush brush = (InnerColor.IsEmpty ? null : new PdfSolidBrush(InnerColor));
			if (Opacity < 1f)
			{
				pdfPageBase.Graphics.Save();
				pdfPageBase.Graphics.SetTransparency(Opacity);
			}
			if (layerGraphics != null)
			{
				layerGraphics.DrawPath(pen, brush, path);
			}
			else
			{
				pdfPageBase.Graphics.DrawPath(pen, brush, path);
			}
			if (Opacity < 1f)
			{
				pdfPageBase.Graphics.Restore();
			}
		}
		else if (!isExternalFlatten && !base.Flatten)
		{
			base.Save();
			isPropertyChanged = false;
			GetBoundsValue();
			isPropertyChanged = true;
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
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}
}
