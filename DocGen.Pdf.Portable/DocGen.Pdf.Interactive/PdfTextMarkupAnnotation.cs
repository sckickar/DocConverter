using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfTextMarkupAnnotation : PdfAnnotation
{
	private PdfTextMarkupAnnotationType m_textMarkupAnnotationType;

	private int[] m_quadPoints = new int[8];

	private PdfArray m_points;

	private PdfColor m_textMarkupColor;

	private new string m_text;

	internal SizeF m_textSize;

	private PointF m_textPoint;

	internal PdfFont m_font;

	private List<RectangleF> m_boundscollection = new List<RectangleF>();

	internal PdfDictionary m_borderDic = new PdfDictionary();

	private PdfLineBorderStyle m_borderStyle;

	public PdfTextMarkupAnnotationType TextMarkupAnnotationType
	{
		get
		{
			return m_textMarkupAnnotationType;
		}
		set
		{
			m_textMarkupAnnotationType = value;
			NotifyPropertyChanged("TextMarkupAnnotationType");
		}
	}

	public PdfColor TextMarkupColor
	{
		get
		{
			return m_textMarkupColor;
		}
		set
		{
			m_textMarkupColor = value;
			NotifyPropertyChanged("TextMarkupColor");
		}
	}

	internal PdfLineBorderStyle BorderStyle
	{
		get
		{
			return m_borderStyle;
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

	public List<RectangleF> BoundsCollection
	{
		get
		{
			if (m_boundscollection == null)
			{
				m_boundscollection = new List<RectangleF>();
			}
			return m_boundscollection;
		}
		set
		{
			if (value != null && value.Count > 0)
			{
				m_quadPoints = new int[value.Count * 8];
			}
			else
			{
				m_quadPoints = new int[8];
			}
			m_boundscollection = value;
			NotifyPropertyChanged("BoundsCollection");
		}
	}

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

	public PdfTextMarkupAnnotation()
	{
	}

	public PdfTextMarkupAnnotation(string markupTitle, string text, string markupText, PointF point, PdfFont pdfFont)
	{
		Text = text;
		m_text = markupTitle;
		m_font = pdfFont;
		Location = point;
		m_textSize = m_font.MeasureString(markupText);
		m_textPoint = point;
		m_textPoint.X += 25f;
		m_textPoint.Y = 800f - m_textPoint.Y;
	}

	public PdfTextMarkupAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName(m_textMarkupAnnotationType));
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		PdfArray pdfArray = new PdfArray();
		if (!TextMarkupColor.IsEmpty)
		{
			float value = (float)(int)TextMarkupColor.R / 255f;
			float value2 = (float)(int)TextMarkupColor.G / 255f;
			float value3 = (float)(int)TextMarkupColor.B / 255f;
			pdfArray.Insert(0, new PdfNumber(value));
			pdfArray.Insert(1, new PdfNumber(value2));
			pdfArray.Insert(2, new PdfNumber(value3));
			base.Dictionary.SetProperty("C", pdfArray);
			if (base.Flatten || base.SetAppearanceDictionary)
			{
				PdfTemplate pdfTemplate = CreateAppearance();
				if (base.Flatten)
				{
					if (pdfTemplate != null)
					{
						if (base.Page != null)
						{
							FlattenAnnotation(base.Page, pdfTemplate);
						}
						else if (base.LoadedPage != null)
						{
							FlattenAnnotation(base.LoadedPage, pdfTemplate);
						}
					}
				}
				else if (pdfTemplate != null)
				{
					base.Appearance.Normal = pdfTemplate;
					base.Dictionary.SetProperty("AP", new PdfReferenceHolder(base.Appearance));
				}
			}
			if (!isExternalFlatten && !base.Flatten)
			{
				base.Save();
				SaveTextMarkUpDictionary();
			}
			if (base.FlattenPopUps || isExternalFlattenPopUps)
			{
				FlattenPopup();
			}
			if (base.Page != null && base.Popup != null && base.Flatten)
			{
				RemoveAnnoationFromPage(base.Page, base.Popup);
			}
			else if (base.LoadedPage != null && base.Popup != null && base.Flatten)
			{
				RemoveAnnoationFromPage(base.LoadedPage, base.Popup);
			}
			return;
		}
		throw new Exception("TextMarkupColor is not null");
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	private void SaveTextMarkUpDictionary()
	{
		base.Dictionary.SetProperty("Subtype", new PdfName(m_textMarkupAnnotationType));
		if (!isAuthorExplicitSet && m_text != null)
		{
			base.Dictionary.SetString("T", m_text);
		}
		m_borderDic.SetProperty("Type", new PdfName("Border"));
		m_borderDic.SetNumber("W", Border.Width);
		base.Dictionary.SetProperty("BS", new PdfReferenceHolder(m_borderDic));
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		PdfGraphics layerGraphics = GetLayerGraphics();
		page.Graphics.Save();
		isAnnotationCreation = true;
		RectangleF rectangleF = CalculateTemplateBounds(Bounds, page, appearance, isNormalMatrix: true);
		isAnnotationCreation = false;
		if (Opacity < 1f)
		{
			page.Graphics.SetTransparency(Opacity);
		}
		if (layerGraphics != null)
		{
			layerGraphics.DrawPdfTemplate(appearance, rectangleF.Location, rectangleF.Size);
		}
		else
		{
			page.Graphics.DrawPdfTemplate(appearance, rectangleF.Location, rectangleF.Size);
		}
		RemoveAnnoationFromPage(page, this);
		page.Graphics.Restore();
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

	private PdfTemplate CreateAppearance()
	{
		double num = 0.0;
		double num2 = 0.0;
		RectangleF rectangleF = RectangleF.Empty;
		if (BoundsCollection.Count > 1)
		{
			PdfPath pdfPath = new PdfPath();
			for (int i = 0; i < BoundsCollection.Count; i++)
			{
				pdfPath.AddRectangle(BoundsCollection[i]);
			}
			rectangleF = (Bounds = pdfPath.GetBounds());
			num = rectangleF.Width;
			num2 = rectangleF.Height;
		}
		else if (base.Dictionary.ContainsKey("QuadPoints"))
		{
			PdfArray pdfArray = base.Dictionary["QuadPoints"] as PdfArray;
			if (m_quadPoints != null)
			{
				for (int j = 0; j < pdfArray.Count / 8; j++)
				{
					float num3 = (pdfArray[4 + j * 8] as PdfNumber).IntValue - (pdfArray[j * 8] as PdfNumber).IntValue;
					float num4 = (pdfArray[5 + j * 8] as PdfNumber).IntValue - (pdfArray[1 + j * 8] as PdfNumber).IntValue;
					num2 = Math.Sqrt(num3 * num3 + num4 * num4);
					float num5 = (pdfArray[6 + j * 8] as PdfNumber).IntValue - (pdfArray[4 + j * 8] as PdfNumber).IntValue;
					num4 = (pdfArray[7 + j * 8] as PdfNumber).IntValue - (pdfArray[5 + j * 8] as PdfNumber).IntValue;
					num = Math.Sqrt(num5 * num5 + num4 * num4);
					Bounds = new RectangleF(Bounds.X, Bounds.Y, (float)num, (float)num2);
				}
			}
		}
		PdfTemplate pdfTemplate = new PdfTemplate(new RectangleF(0f, 0f, (float)num, (float)num2));
		SetMatrix(pdfTemplate.m_content);
		PdfGraphics graphics = pdfTemplate.Graphics;
		graphics.SetTransparency(Opacity, Opacity, PdfBlendMode.Multiply);
		if (BoundsCollection.Count > 1)
		{
			for (int k = 0; k < BoundsCollection.Count; k++)
			{
				if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Highlight)
				{
					graphics.DrawRectangle(new PdfSolidBrush(TextMarkupColor), BoundsCollection[k].X - rectangleF.X, BoundsCollection[k].Y - rectangleF.Y, BoundsCollection[k].Width, BoundsCollection[k].Height);
				}
				else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Underline)
				{
					graphics.DrawLine(new PdfPen(TextMarkupColor), BoundsCollection[k].X - rectangleF.X, BoundsCollection[k].Y - rectangleF.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f / 3f), BoundsCollection[k].Width + (BoundsCollection[k].X - rectangleF.X), BoundsCollection[k].Y - rectangleF.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f / 3f));
				}
				else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.StrikeOut)
				{
					graphics.DrawLine(new PdfPen(TextMarkupColor), BoundsCollection[k].X - rectangleF.X, BoundsCollection[k].Y - rectangleF.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f), BoundsCollection[k].Width + (BoundsCollection[k].X - rectangleF.X), BoundsCollection[k].Y - rectangleF.Y + (BoundsCollection[k].Height - BoundsCollection[k].Height / 2f));
				}
				else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Squiggly)
				{
					PdfPen pdfPen = new PdfPen(TextMarkupColor);
					pdfPen.Width = BoundsCollection[k].Height * 0.02f;
					graphics.Save();
					graphics.TranslateTransform(BoundsCollection[k].X - rectangleF.X, BoundsCollection[k].Y - rectangleF.Y);
					graphics.SetClip(new RectangleF(0f, 0f, BoundsCollection[k].Width, BoundsCollection[k].Height));
					graphics.DrawPath(pdfPen, DrawSquiggly(BoundsCollection[k].Width, BoundsCollection[k].Height));
					graphics.Restore();
				}
			}
		}
		else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Highlight)
		{
			graphics.DrawRectangle(new PdfSolidBrush(TextMarkupColor), 0f, 0f, (float)num, (float)num2);
		}
		else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Underline)
		{
			graphics.DrawLine(new PdfPen(TextMarkupColor), 0f, (float)num2 - (float)num2 / 2f / 3f, (float)num, (float)num2 - (float)num2 / 2f / 3f);
		}
		else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.StrikeOut)
		{
			graphics.DrawLine(new PdfPen(TextMarkupColor), 0f, (float)num2 / 2f, (float)num, (float)num2 / 2f);
		}
		else if (TextMarkupAnnotationType == PdfTextMarkupAnnotationType.Squiggly)
		{
			PdfPen pdfPen2 = new PdfPen(TextMarkupColor);
			pdfPen2.Width = (float)num2 * 0.02f;
			graphics.DrawPath(pdfPen2, DrawSquiggly((float)num, (float)num2));
		}
		return pdfTemplate;
	}

	internal void SetQuadPoints(SizeF pageSize)
	{
		float[] array = new float[m_quadPoints.Length];
		float height = pageSize.Height;
		PdfMargins pdfMargins = ObtainMargin();
		if (m_textSize == new SizeF(0f, 0f))
		{
			m_textSize = Size;
		}
		if (BoundsCollection.Count == 0)
		{
			m_boundscollection.Add(new RectangleF(Location, m_textSize));
		}
		int num = m_quadPoints.Length / 8;
		float num2 = 0f;
		float num3 = 0f;
		PdfArray cropOrMediaBox = null;
		cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
		bool flag = false;
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.Flatten)
		{
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				for (int i = 0; i < num; i++)
				{
					num2 = m_boundscollection[i].X + pdfMargins.Left + pdfNumber.FloatValue;
					num3 = pdfNumber2.FloatValue + pdfMargins.Top;
					array[i * 8] = num2 + pdfMargins.Left;
					array[1 + i * 8] = height - (0f - num3) - pdfMargins.Top - m_boundscollection[i].Y;
					array[2 + i * 8] = num2 + m_boundscollection[i].Width + pdfMargins.Left;
					array[3 + i * 8] = height - (0f - num3) - pdfMargins.Top - m_boundscollection[i].Y;
					array[4 + i * 8] = num2 + pdfMargins.Left;
					array[5 + i * 8] = array[1 + i * 8] - m_boundscollection[i].Height;
					array[6 + i * 8] = num2 + m_boundscollection[i].Width + pdfMargins.Left;
					array[7 + i * 8] = array[5 + i * 8];
				}
				flag = true;
			}
		}
		if (!flag)
		{
			for (int j = 0; j < num; j++)
			{
				num2 = m_boundscollection[j].X;
				num3 = m_boundscollection[j].Y;
				array[j * 8] = num2 + pdfMargins.Left;
				array[1 + j * 8] = height - num3 - pdfMargins.Top;
				array[2 + j * 8] = num2 + m_boundscollection[j].Width + pdfMargins.Left;
				array[3 + j * 8] = height - num3 - pdfMargins.Top;
				array[4 + j * 8] = num2 + pdfMargins.Left;
				array[5 + j * 8] = array[1 + j * 8] - m_boundscollection[j].Height;
				array[6 + j * 8] = num2 + m_boundscollection[j].Width + pdfMargins.Left;
				array[7 + j * 8] = array[5 + j * 8];
			}
		}
		m_points = new PdfArray(array);
		base.Dictionary.SetProperty("QuadPoints", m_points);
	}
}
