using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedRectangleAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private LineBorder m_lineborder = new LineBorder();

	private PdfArray m_dashedArray;

	private bool m_isDashArrayReset;

	private PdfBorderEffect m_borderEffect = new PdfBorderEffect();

	private float m_borderWidth;

	private bool ismodified;

	internal bool IsModified
	{
		get
		{
			return ismodified;
		}
		set
		{
			ismodified = value;
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

	public LineBorder LineBorder
	{
		get
		{
			m_lineborder = ObtainLineBorder();
			return m_lineborder;
		}
		set
		{
			m_lineborder = value;
			base.Dictionary.SetProperty("BS", m_lineborder);
			NotifyPropertyChanged("LineBorder");
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
			if (base.Dictionary.ContainsKey("AP"))
			{
				ismodified = true;
			}
			NotifyPropertyChanged("BorderEffect");
		}
	}

	internal PdfLoadedRectangleAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
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

	private LineBorder ObtainLineBorder()
	{
		if (base.Dictionary.ContainsKey("Border"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Border"]) is PdfArray { Count: >2 } pdfArray && pdfArray[2] is PdfNumber)
			{
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				m_lineborder.BorderWidth = (int)floatValue;
				m_lineborder.BorderLineWidth = floatValue;
			}
		}
		else if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("W") && PdfCrossTable.Dereference(pdfDictionary["W"]) is PdfNumber { IntValue: var intValue, FloatValue: var floatValue2 })
			{
				m_lineborder.BorderWidth = intValue;
				m_lineborder.BorderLineWidth = floatValue2;
			}
			if (pdfDictionary.ContainsKey("S"))
			{
				PdfName pdfName = PdfCrossTable.Dereference(pdfDictionary["S"]) as PdfName;
				if (pdfName != null)
				{
					m_lineborder.BorderStyle = GetBorderStyle(pdfName.Value.ToString());
				}
			}
			if (pdfDictionary.ContainsKey("D"))
			{
				PdfArray pdfArray2 = pdfDictionary["D"] as PdfArray;
				if (!m_isDashArrayReset)
				{
					m_dashedArray = pdfArray2.Clone(base.CrossTable) as PdfArray;
					m_isDashArrayReset = true;
				}
				if (pdfArray2.Count != 0)
				{
					int intValue2 = (pdfArray2[0] as PdfNumber).IntValue;
					pdfArray2.Clear();
					pdfArray2.Insert(0, new PdfNumber(intValue2));
					pdfArray2.Insert(1, new PdfNumber(intValue2));
					m_lineborder.DashArray = intValue2;
				}
			}
		}
		base.Dictionary.SetProperty("BS", m_lineborder);
		return m_lineborder;
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
		if (num != 2)
		{
			if (base.Flatten || base.Page.Annotations.Flatten || base.SetAppearanceDictionary || IsModified || isExternalFlatten)
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
				if (!(pdfDictionary2 is PdfStream template))
				{
					return;
				}
				appearance = new PdfTemplate(template);
				if (appearance != null)
				{
					bool flag = ValidateTemplateMatrix(pdfDictionary2);
					if ((flag && base.Page.Rotation != 0) || IsValidTemplateMatrix(pdfDictionary2, Bounds.Location, appearance))
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
		if (LineBorder.BorderWidth == 1 || LineBorder.BorderLineWidth != 0f)
		{
			m_borderWidth = LineBorder.BorderLineWidth;
		}
		else
		{
			m_borderWidth = LineBorder.BorderWidth;
		}
		if (base.SetAppearanceDictionary || IsModified)
		{
			PaintParams paintParams = new PaintParams();
			float borderWidth = m_borderWidth / 2f;
			PdfPen pdfPen = new PdfPen(Color, m_borderWidth);
			PdfBrush backBrush = null;
			RectangleF rectangleF = RectangleF.Empty;
			if (base.Dictionary["RD"] == null && BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
			{
				RectangleF bounds = new RectangleF(Bounds.X - BorderEffect.Intensity * 5f - m_borderWidth / 2f, Bounds.Y - BorderEffect.Intensity * 5f - m_borderWidth / 2f, Bounds.Width + BorderEffect.Intensity * 10f + m_borderWidth, Bounds.Height + BorderEffect.Intensity * 10f + m_borderWidth);
				float num = BorderEffect.Intensity * 5f;
				float[] array = new float[4]
				{
					num + m_borderWidth / 2f,
					num + m_borderWidth / 2f,
					num + m_borderWidth / 2f,
					num + m_borderWidth / 2f
				};
				base.Dictionary.SetProperty("RD", new PdfArray(array));
				Bounds = bounds;
			}
			if (!isBounds && base.Dictionary["RD"] != null)
			{
				PdfArray obj = base.Dictionary["RD"] as PdfArray;
				PdfNumber pdfNumber = obj.Elements[0] as PdfNumber;
				PdfNumber pdfNumber2 = obj.Elements[1] as PdfNumber;
				PdfNumber pdfNumber3 = obj.Elements[2] as PdfNumber;
				PdfNumber pdfNumber4 = obj.Elements[3] as PdfNumber;
				RectangleF bounds2 = new RectangleF(Bounds.X + pdfNumber.FloatValue, Bounds.Y + pdfNumber2.FloatValue, Bounds.Width - pdfNumber3.FloatValue * 2f, Bounds.Height - pdfNumber4.FloatValue * 2f);
				if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
				{
					bounds2.X = bounds2.X - BorderEffect.Intensity * 5f - m_borderWidth / 2f;
					bounds2.Y = bounds2.Y - BorderEffect.Intensity * 5f - m_borderWidth / 2f;
					bounds2.Width = bounds2.Width + BorderEffect.Intensity * 10f + m_borderWidth;
					bounds2.Height = bounds2.Height + BorderEffect.Intensity * 10f + m_borderWidth;
					float num2 = BorderEffect.Intensity * 5f;
					float[] array2 = new float[4]
					{
						num2 + m_borderWidth / 2f,
						num2 + m_borderWidth / 2f,
						num2 + m_borderWidth / 2f,
						num2 + m_borderWidth / 2f
					};
					base.Dictionary.SetProperty("RD", new PdfArray(array2));
				}
				else
				{
					base.Dictionary.Remove("RD");
				}
				Bounds = bounds2;
			}
			rectangleF = new RectangleF(0f, 0f, Bounds.Width, Bounds.Height);
			PdfTemplate pdfTemplate = new PdfTemplate(rectangleF);
			SetMatrix(pdfTemplate.m_content);
			if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
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
			RectangleF rectangle = ObtainStyle(pdfPen, rectangleF, borderWidth);
			if (Opacity < 1f)
			{
				graphics.Save();
				graphics.SetTransparency(Opacity);
			}
			if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
			{
				DrawAppearance(rectangle, borderWidth, graphics, paintParams);
			}
			else
			{
				FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
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
		PdfPath pdfPath = new PdfPath();
		pdfPath.AddRectangle(rectangle);
		float num = BorderEffect.Intensity * 4.25f;
		if (num > 0f)
		{
			PointF[] array = new PointF[pdfPath.PathPoints.Length];
			for (int i = 0; i < pdfPath.PathPoints.Length; i++)
			{
				array[i] = new PointF(pdfPath.PathPoints[i].X, 0f - pdfPath.PathPoints[i].Y);
			}
			pdfPath = new PdfPath();
			pdfPath.AddPolygon(array);
			DrawCloudStyle(graphics, paintParams.BackBrush, paintParams.BorderPen, num, 0.833f, pdfPath.PathPoints, isAppearance: false);
		}
		else
		{
			FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}
	}

	private RectangleF ObtainStyle(PdfPen mBorderPen, RectangleF rectangle, float borderWidth)
	{
		if (LineBorder.BorderWidth == 1 || LineBorder.BorderLineWidth != 0f)
		{
			m_borderWidth = LineBorder.BorderLineWidth;
		}
		else
		{
			m_borderWidth = LineBorder.BorderWidth;
		}
		if (base.Dictionary.ContainsKey("BS") && PdfCrossTable.Dereference(base.Dictionary["BS"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("D"))
		{
			PdfArray dashedArray = m_dashedArray;
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
			if (flag && LineBorder.BorderStyle == PdfBorderStyle.Dashed)
			{
				mBorderPen.DashStyle = PdfDashStyle.Dash;
				mBorderPen.DashPattern = array;
			}
		}
		if (BorderEffect.Intensity != 0f && BorderEffect.Style == PdfBorderEffectStyle.Cloudy)
		{
			float num = BorderEffect.Intensity * 5f;
			rectangle.X = rectangle.X + num + borderWidth;
			rectangle.Y = rectangle.Y + num + borderWidth;
			rectangle.Width = rectangle.Width - 2f * num - 2f * borderWidth;
			rectangle.Height = rectangle.Height - 2f * num - 2f * borderWidth;
		}
		else
		{
			rectangle.X += borderWidth;
			rectangle.Y += borderWidth;
			rectangle.Width -= m_borderWidth;
			rectangle.Height = Bounds.Height - m_borderWidth;
		}
		return rectangle;
	}
}
