using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfWatermarkAnnotation : PdfAnnotation
{
	private new PdfAppearance m_appearance;

	private float m_opacity = 1f;

	private float rotateAngle;

	private SizeF m_size;

	private PointF m_location;

	private bool m_saved;

	public new PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
			}
			return m_appearance;
		}
	}

	public override float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			if (value < 0f || value > 1f)
			{
				throw new ArgumentException("Valid value should be between 0 to 1.");
			}
			if (m_opacity != value)
			{
				m_opacity = value;
				m_appearance.Normal.Graphics.SetTransparency(m_opacity);
			}
			NotifyPropertyChanged("Opacity");
		}
	}

	public PdfWatermarkAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
		m_appearance = new PdfAppearance(this);
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Watermark"));
	}

	private new void SetMatrix(PdfDictionary template)
	{
		PdfArray pdfArray = null;
		_ = new float[0];
		if (template["BBox"] is PdfArray pdfArray2)
		{
			if (base.RotateAngle != 0f)
			{
				rotateAngle = base.RotateAngle;
				if (rotateAngle == 0f)
				{
					rotateAngle = (int)base.Rotate * 90;
				}
				pdfArray = new PdfArray(GetRotatedTransformMatrix(pdfArray2, rotateAngle).Matrix.Elements);
			}
			else
			{
				pdfArray = new PdfArray(new float[6]
				{
					1f,
					0f,
					0f,
					1f,
					0f - (pdfArray2[0] as PdfNumber).FloatValue,
					0f - (pdfArray2[1] as PdfNumber).FloatValue
				});
			}
		}
		if (pdfArray != null)
		{
			template["Matrix"] = pdfArray;
		}
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, isExternalFlattenPopUps: false);
	}

	private PdfTemplate CustomAppearance(PdfTemplate template)
	{
		if (m_appearance != null && m_appearance.Normal != null)
		{
			if (base.Rotate != 0 || base.RotateAngle != 0f)
			{
				rotateAngle = base.RotateAngle;
				if (rotateAngle == 0f)
				{
					rotateAngle = (int)base.Rotate * 90;
				}
				Bounds = GetRotatedBounds(Bounds, rotateAngle);
			}
			SetMatrix(Appearance.Normal.m_content);
			template = m_appearance.Normal;
		}
		return template;
	}

	private PdfTemplate CreateAppearance()
	{
		PdfTemplate pdfTemplate = null;
		if (m_appearance != null)
		{
			pdfTemplate = CustomAppearance(pdfTemplate);
		}
		return pdfTemplate;
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
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
			Appearance.Normal = pdfTemplate;
			base.Dictionary.SetProperty("AP", new PdfReferenceHolder(Appearance));
		}
		if (!isExternalFlatten && !base.Flatten)
		{
			base.Save();
		}
		if (!isExternalFlatten)
		{
			m_saved = true;
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

	private void FlattenAnnotation(PdfPageBase page, PdfTemplate appearance)
	{
		PdfGraphics layerGraphics = GetLayerGraphics();
		if (base.Rotate != 0)
		{
			appearance.IsAnnotationTemplate = true;
			appearance.NeedScaling = true;
		}
		bool flag = base.Rotate == PdfAnnotationRotateAngle.RotateAngle0;
		page.Graphics.Save();
		if (!flag)
		{
			flag = page.Rotation != PdfPageRotateAngle.RotateAngle0;
		}
		isAnnotationCreation = true;
		RectangleF rectangleF = CalculateTemplateBounds(Bounds, page, appearance, flag);
		isAnnotationCreation = false;
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
		if (base.Rotate != 0)
		{
			if (base.RotateAngle == 90f)
			{
				if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					m_location.X += m_size.Width - m_size.Height;
					m_location.Y += m_size.Width - m_size.Height;
				}
				if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					m_location.X += m_size.Width;
				}
				else
				{
					m_location.X += m_size.Height;
				}
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270)
			{
				if (page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					m_location.X += m_size.Width - m_size.Height;
					m_location.Y += m_size.Width - m_size.Height;
				}
				if (page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					m_location.X += m_size.Width - m_size.Height;
					m_location.Y += 0f - m_size.Width;
				}
				else
				{
					m_location.Y += 0f - m_size.Width;
				}
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle180)
			{
				m_location.X += m_size.Width;
				m_location.Y += 0f - m_size.Height;
			}
		}
		if (Opacity < 1f)
		{
			page.Graphics.SetTransparency(Opacity);
		}
		if (layerGraphics != null)
		{
			layerGraphics.DrawPdfTemplate(appearance, m_location, m_size);
		}
		else
		{
			page.Graphics.DrawPdfTemplate(appearance, m_location, m_size);
		}
		RemoveAnnoationFromPage(page, this);
		page.Graphics.Restore();
	}
}
