using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedWatermarkAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private float m_opacity = 1f;

	private PdfTemplate m_template;

	private bool opacityChanged;

	private new PdfAppearance m_appearance;

	private float rotateAngle;

	private SizeF m_size;

	private PointF m_location;

	internal PdfLoadedWatermarkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle)
		: base(dictionary, crossTable)
	{
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_template = new PdfTemplate(Bounds.Width, Bounds.Height);
	}

	internal PdfTemplate CreateAppearance()
	{
		PdfTemplate pdfTemplate = new PdfTemplate(new RectangleF(0f, 0f, Bounds.Width, Bounds.Height));
		if (base.Rotate != 0 || base.RotateAngle != 0f)
		{
			rotateAngle = base.RotateAngle;
			if (rotateAngle == 0f)
			{
				rotateAngle = (int)base.Rotate * 90;
			}
			Bounds = GetRotatedBounds(Bounds, rotateAngle);
		}
		SetMatrix(pdfTemplate.m_content);
		PdfGraphics graphics = pdfTemplate.Graphics;
		if (Opacity < 1f)
		{
			PdfGraphicsState state = graphics.Save();
			graphics.SetTransparency(m_opacity);
			graphics.Restore(state);
		}
		else
		{
			pdfTemplate.Graphics.DrawPdfTemplate(pdfTemplate, PointF.Empty);
		}
		return pdfTemplate;
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, isExternalFlattenPopUps: false);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (base.Flatten || base.Page.Annotations.Flatten || base.SetAppearanceDictionary || isExternalFlatten)
		{
			PdfTemplate appearance = null;
			if (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten)
			{
				FlattenAnnotation(base.Page, appearance);
			}
		}
		if (Popup != null && (base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		if (base.Dictionary.ContainsKey("AP") && appearance == null)
		{
			if (!(PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary) || !(PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2) || !(pdfDictionary2 is PdfStream template))
			{
				return;
			}
			bool flag = base.Page.Rotation == PdfPageRotateAngle.RotateAngle0 && base.Rotate == PdfAnnotationRotateAngle.RotateAngle0;
			if (!flag)
			{
				flag = base.Page.Rotation != 0 && base.Rotate == PdfAnnotationRotateAngle.RotateAngle0;
			}
			bool flag2 = flag;
			appearance = new PdfTemplate(template, flag2);
			if (appearance != null)
			{
				bool isNormalMatrix = ValidateTemplateMatrix(pdfDictionary2);
				if (flag2)
				{
					FlattenAnnotationTemplate(appearance, flag2);
				}
				else
				{
					FlattenAnnotationTemplate(appearance, isNormalMatrix);
				}
			}
		}
		else if (!base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix2 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix2);
		}
		else if (base.Dictionary.ContainsKey("AP") && appearance != null)
		{
			bool isNormalMatrix3 = ValidateTemplateMatrix(appearance.m_content);
			FlattenAnnotationTemplate(appearance, isNormalMatrix3);
		}
	}

	private new void FlattenAnnotationTemplate(PdfTemplate appearance, bool isNormalMatrix)
	{
		PdfGraphics pdfGraphics = ObtainlayerGraphics();
		PdfGraphicsState state = base.Page.Graphics.Save();
		if (!isNormalMatrix && base.Rotate != PdfAnnotationRotateAngle.RotateAngle180)
		{
			appearance.IsAnnotationTemplate = true;
			appearance.NeedScaling = true;
		}
		if (Opacity < 1f)
		{
			base.Page.Graphics.SetTransparency(Opacity);
		}
		RectangleF rectangleF = CalculateTemplateBounds(Bounds, base.Page, appearance, isNormalMatrix);
		if (base.RotateAngle == 0f)
		{
			m_size = rectangleF.Size;
			m_location = rectangleF.Location;
		}
		else
		{
			m_size = appearance.Size;
			m_location = rectangleF.Location;
		}
		bool flag = false;
		if (base.Rotate != 0 && appearance.m_content != null && appearance.m_content.ContainsKey("Matrix") && PdfCrossTable.Dereference(appearance.m_content["Matrix"]) is PdfArray { Count: 6 } pdfArray && (pdfArray[4] as PdfNumber).FloatValue == 0f && (pdfArray[5] as PdfNumber).FloatValue == 0f)
		{
			flag = true;
		}
		float num = ((appearance.Width > 0f) ? (rectangleF.Size.Width / appearance.Width) : 1f);
		float num2 = ((appearance.Height > 0f) ? (rectangleF.Size.Height / appearance.Height) : 1f);
		bool flag2 = num != 1f || num2 != 1f;
		if (base.Rotate != PdfAnnotationRotateAngle.RotateAngle0 && flag)
		{
			if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle90)
			{
				if (base.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					if (flag2 && (Bounds.X != 0f || Bounds.Y != 0f))
					{
						m_location.X += m_size.Width - m_size.Height;
						m_location.Y += m_size.Width;
					}
					else
					{
						m_location.X += m_size.Height;
						m_location.Y += m_size.Width - m_size.Height + (m_size.Width - m_size.Height);
					}
				}
				else if (!flag2)
				{
					m_location.X += m_size.Height;
				}
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270)
			{
				if (base.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					if (flag2)
					{
						m_location.Y += m_size.Width - m_size.Height;
					}
				}
				else if (!flag2 && (Bounds.X != 0f || Bounds.Y != 0f))
				{
					m_location.Y += 0f - m_size.Width;
				}
				else
				{
					m_location.Y += 0f - (m_size.Width - m_size.Height);
				}
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle180)
			{
				m_location.X += m_size.Width;
				m_location.Y += 0f - m_size.Height;
			}
		}
		PdfArray pdfArray2 = null;
		if (base.Page != null && base.Page.Dictionary.ContainsKey("CropBox"))
		{
			pdfArray2 = PdfCrossTable.Dereference(base.Page.Dictionary["CropBox"]) as PdfArray;
		}
		PdfArray pdfArray3 = null;
		if (base.Page != null && base.Page.Dictionary.ContainsKey("MediaBox"))
		{
			pdfArray3 = PdfCrossTable.Dereference(base.Page.Dictionary["MediaBox"]) as PdfArray;
		}
		if (pdfArray3 != null && pdfArray3.Count == 4 && pdfArray2 != null && pdfArray2.Count == 4)
		{
			float num3 = 0f;
			if (PdfCrossTable.Dereference(pdfArray3[2]) is PdfNumber pdfNumber)
			{
				num3 = pdfNumber.FloatValue;
			}
			float num4 = 0f;
			if (PdfCrossTable.Dereference(pdfArray3[3]) is PdfNumber pdfNumber2)
			{
				num4 = pdfNumber2.FloatValue;
			}
			float num5 = 0f;
			PdfNumber pdfNumber3 = PdfCrossTable.Dereference(pdfArray2[2]) as PdfNumber;
			if (pdfNumber3 != null)
			{
				num5 = pdfNumber3.FloatValue;
			}
			float num6 = 0f;
			if (PdfCrossTable.Dereference(pdfArray2[3]) is PdfNumber)
			{
				num6 = pdfNumber3.FloatValue;
			}
			float num7 = 0f;
			if (PdfCrossTable.Dereference(pdfArray2[0]) is PdfNumber pdfNumber4)
			{
				num7 = pdfNumber4.FloatValue;
			}
			float num8 = 0f;
			if (PdfCrossTable.Dereference(pdfArray2[1]) is PdfNumber pdfNumber5)
			{
				num8 = pdfNumber5.FloatValue;
			}
			if (num7 != 0f && num8 != 0f && num3 != num5 && num4 != num6)
			{
				base.Page.Graphics.m_isWatermarkMediabox = true;
			}
		}
		if (pdfGraphics != null && base.Page.Rotation == PdfPageRotateAngle.RotateAngle0)
		{
			pdfGraphics.DrawPdfTemplate(appearance, m_location, rectangleF.Size);
		}
		else
		{
			base.Page.Graphics.DrawPdfTemplate(appearance, m_location, rectangleF.Size);
		}
		base.Page.Graphics.Restore(state);
		RemoveAnnoationFromPage(base.Page, this);
	}
}
