using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedCircleAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private LineBorder m_border = new LineBorder();

	private PdfArray m_DashedArray;

	private bool m_isDashArrayReset;

	private float m_borderWidth;

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

	internal PdfLoadedCircleAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("Text must be not null");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_text = text;
	}

	private new LineBorder GetLineBorder()
	{
		if (base.Dictionary.ContainsKey("Border"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Border"]) is PdfArray { Count: >=2 } pdfArray && pdfArray[2] is PdfNumber)
			{
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				m_border.BorderWidth = (int)floatValue;
				m_border.BorderLineWidth = floatValue;
			}
		}
		else if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("W"))
			{
				int intValue = (pdfDictionary["W"] as PdfNumber).IntValue;
				float floatValue2 = (pdfDictionary["W"] as PdfNumber).FloatValue;
				m_border.BorderWidth = intValue;
				m_border.BorderLineWidth = floatValue2;
			}
			if (pdfDictionary.ContainsKey("S"))
			{
				PdfName pdfName = pdfDictionary["S"] as PdfName;
				m_border.BorderStyle = GetBorderStyle(pdfName.Value.ToString());
			}
			if (pdfDictionary.ContainsKey("D") && PdfCrossTable.Dereference(pdfDictionary["D"]) is PdfArray { Count: >0 } pdfArray2)
			{
				if (!m_isDashArrayReset)
				{
					m_DashedArray = pdfArray2.Clone(base.CrossTable) as PdfArray;
					m_isDashArrayReset = true;
				}
				if (pdfArray2[0] is PdfNumber { IntValue: var intValue2 })
				{
					pdfArray2.Clear();
					pdfArray2.Insert(0, new PdfNumber(intValue2));
					pdfArray2.Insert(1, new PdfNumber(intValue2));
					m_border.DashArray = intValue2;
				}
			}
		}
		return m_border;
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
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
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
		SaveAnnotationBorder(m_borderWidth);
		if (num != 2)
		{
			if (base.Flatten || base.Page.Annotations.Flatten || base.SetAppearanceDictionary || isExternalFlatten)
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
		else
		{
			RemoveAnnoationFromPage(base.Page, this);
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenLoadedPopup();
		}
		if (!isExternalFlatten)
		{
			m_isDashArrayReset = false;
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
				if (pdfDictionary2 is PdfStream template)
				{
					appearance = new PdfTemplate(template);
					if (appearance != null)
					{
						bool isNormalMatrix = ValidateTemplateMatrix(pdfDictionary2);
						FlattenAnnotationTemplate(appearance, isNormalMatrix);
					}
				}
			}
			else
			{
				base.SetAppearanceDictionary = true;
				appearance = CreateAppearance();
				if (appearance != null)
				{
					bool isNormalMatrix2 = ValidateTemplateMatrix(appearance.m_content);
					FlattenAnnotationTemplate(appearance, isNormalMatrix2);
				}
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance == null)
		{
			base.SetAppearanceDictionary = true;
			appearance = CreateAppearance();
			if (appearance != null)
			{
				bool isNormalMatrix3 = ValidateTemplateMatrix(appearance.m_content);
				FlattenAnnotationTemplate(appearance, isNormalMatrix3);
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix4 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix4);
		}
		else if (base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix5 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix5);
		}
	}

	private PdfTemplate CreateAppearance()
	{
		if (base.SetAppearanceDictionary)
		{
			PaintParams paintParams = new PaintParams();
			float num = m_borderWidth / 2f;
			PdfPen pdfPen = new PdfPen(Color, m_borderWidth);
			PdfBrush backBrush = null;
			RectangleF rectangleF = new RectangleF(0f, 0f, Bounds.Width, Bounds.Height);
			PdfTemplate pdfTemplate = new PdfTemplate(rectangleF);
			SetMatrix(pdfTemplate.m_content);
			if (base.Dictionary.ContainsKey("BE"))
			{
				pdfTemplate.m_writeTransformation = false;
			}
			PdfGraphics graphics = pdfTemplate.Graphics;
			if (InnerColor.A != 0)
			{
				backBrush = new PdfSolidBrush(InnerColor);
			}
			if (m_borderWidth > 0f)
			{
				paintParams.BorderPen = pdfPen;
			}
			paintParams.ForeBrush = new PdfSolidBrush(Color);
			paintParams.BackBrush = backBrush;
			paintParams.BorderWidth = (int)m_borderWidth;
			RectangleF rectangle = ObtainStyle(pdfPen, rectangleF, num);
			if (Opacity < 1f)
			{
				graphics.Save();
				graphics.SetTransparency(Opacity);
			}
			if (base.Dictionary.ContainsKey("BE"))
			{
				DrawAppearance(rectangle, num, graphics, paintParams);
			}
			else
			{
				FieldPainter.DrawEllipseAnnotation(graphics, paintParams, rectangle.X + num, rectangle.Y, rectangle.Width - m_borderWidth, rectangle.Height);
			}
			if (Opacity < 1f)
			{
				graphics.Restore();
			}
			return pdfTemplate;
		}
		return null;
	}

	private void DrawAppearance(RectangleF rectangle, float borderWidth, PdfGraphics graphics, PaintParams paintParams)
	{
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		new PdfPath().AddEllipse(new RectangleF(rectangle.X + borderWidth, 0f - rectangle.Y - rectangle.Height, rectangle.Width - m_borderWidth, rectangle.Height));
		float num = 0f;
		if (base.Dictionary.ContainsKey("RD") && PdfCrossTable.Dereference(base.Dictionary.Items[new PdfName("RD")]) is PdfArray pdfArray)
		{
			num = (pdfArray.Elements[0] as PdfNumber).FloatValue;
		}
		if (num > 0f)
		{
			RectangleF rectangleF = new RectangleF(rectangle.X + borderWidth, 0f - rectangle.Y - rectangle.Height, rectangle.Width - m_borderWidth, rectangle.Height);
			List<PointF> list = new List<PointF>();
			List<PointF> list2 = new List<PointF>();
			List<PointF> list3 = new List<PointF>();
			List<PointF> list4 = new List<PointF>();
			list2.Add(new PointF(rectangleF.Right, rectangleF.Bottom));
			list2.Add(new PointF(rectangleF.Left, rectangleF.Bottom));
			list2.Add(new PointF(rectangleF.Left, rectangleF.Top));
			list2.Add(new PointF(rectangleF.Right, rectangleF.Top));
			list.Add(new PointF(rectangleF.Right, rectangleF.Top + rectangleF.Height / 2f));
			list.Add(new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Bottom));
			list.Add(new PointF(rectangleF.Left, rectangleF.Top + rectangleF.Height / 2f));
			list.Add(new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top));
			list3.Add(new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Bottom));
			list3.Add(new PointF(rectangleF.Left, rectangleF.Top + rectangleF.Height / 2f));
			list3.Add(new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top));
			list3.Add(new PointF(rectangleF.Right, rectangleF.Top + rectangleF.Height / 2f));
			for (int i = 0; i < list2.Count; i++)
			{
				CreateBezier(list[i], list2[i], list3[i], list4);
			}
			DrawCloudStyle(graphics, paintParams.BackBrush, paintParams.BorderPen, num, 0.833f, list4.ToArray(), isAppearance: false);
			list.Clear();
			list2.Clear();
			list3.Clear();
			list4.Clear();
		}
		else
		{
			FieldPainter.DrawEllipseAnnotation(graphics, paintParams, rectangle.X + borderWidth, 0f - rectangle.Y, rectangle.Width - m_borderWidth, 0f - rectangle.Height);
		}
	}

	private RectangleF ObtainStyle(PdfPen mBorderPen, RectangleF rectangle, float borderWidth)
	{
		if (base.Dictionary.ContainsKey("BS") && PdfCrossTable.Dereference(base.Dictionary["BS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("D"))
		{
			PdfArray dashedArray = m_DashedArray;
			float[] array = new float[dashedArray.Count];
			bool flag = false;
			for (int i = 0; i < dashedArray.Count; i++)
			{
				array[i] = (dashedArray.Elements[i] as PdfNumber).FloatValue;
				if (array[i] > 0f)
				{
					flag = true;
				}
			}
			if (flag && Border.BorderStyle == PdfBorderStyle.Dashed)
			{
				mBorderPen.DashStyle = PdfDashStyle.Dash;
				mBorderPen.DashPattern = array;
			}
		}
		if (!isBounds && base.Dictionary["RD"] != null)
		{
			if (PdfCrossTable.Dereference(base.Dictionary["RD"]) is PdfArray pdfArray)
			{
				PdfNumber pdfNumber = pdfArray.Elements[0] as PdfNumber;
				PdfNumber pdfNumber2 = pdfArray.Elements[1] as PdfNumber;
				PdfNumber pdfNumber3 = pdfArray.Elements[2] as PdfNumber;
				PdfNumber pdfNumber4 = pdfArray.Elements[3] as PdfNumber;
				rectangle.X += pdfNumber.FloatValue;
				rectangle.Y = rectangle.Y + borderWidth + pdfNumber2.FloatValue;
				rectangle.Width -= 2f * pdfNumber3.FloatValue;
				rectangle.Height -= m_borderWidth;
				rectangle.Height -= 2f * pdfNumber4.FloatValue;
			}
		}
		else
		{
			rectangle.Y += borderWidth;
			rectangle.Height = Bounds.Height - m_borderWidth;
		}
		return rectangle;
	}

	private void CreateBezier(PointF ctrl1, PointF ctrl2, PointF ctrl3, List<PointF> bezierPoints)
	{
		bezierPoints.Add(ctrl1);
		PopulateBezierPoints(ctrl1, ctrl2, ctrl3, 0, bezierPoints);
		bezierPoints.Add(ctrl3);
	}

	private void PopulateBezierPoints(PointF ctrl1, PointF ctrl2, PointF ctrl3, int currentIteration, List<PointF> bezierPoints)
	{
		if (currentIteration < 2)
		{
			PointF pointF = MidPoint(ctrl1, ctrl2);
			PointF pointF2 = MidPoint(ctrl2, ctrl3);
			PointF pointF3 = MidPoint(pointF, pointF2);
			currentIteration++;
			PopulateBezierPoints(ctrl1, pointF, pointF3, currentIteration, bezierPoints);
			bezierPoints.Add(pointF3);
			PopulateBezierPoints(pointF3, pointF2, ctrl3, currentIteration, bezierPoints);
		}
	}

	private PointF MidPoint(PointF controlPoint1, PointF controlPoint2)
	{
		return new PointF((controlPoint1.X + controlPoint2.X) / 2f, (controlPoint1.Y + controlPoint2.Y) / 2f);
	}
}
