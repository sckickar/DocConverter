using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRubberStampAnnotation : PdfAnnotation
{
	private PdfRubberStampAnnotationIcon m_rubberStampAnnotaionIcon = PdfRubberStampAnnotationIcon.Draft;

	private new PdfAppearance m_appearance;

	private string m_icon;

	private float m_stampWidth;

	private bool m_standardStampAppearance;

	private float rotateAngle;

	private SizeF m_size;

	private PointF m_location;

	private bool m_saved;

	private bool m_resetAppearance = true;

	private bool m_alterRotateBounds = true;

	public PdfRubberStampAnnotationIcon Icon
	{
		get
		{
			return m_rubberStampAnnotaionIcon;
		}
		set
		{
			m_rubberStampAnnotaionIcon = value;
			if (m_appearance == null)
			{
				base.Dictionary.SetName("Name", "#23" + m_rubberStampAnnotaionIcon);
			}
			m_resetAppearance = true;
			NotifyPropertyChanged("Icon");
		}
	}

	public new PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
				if (!m_standardStampAppearance)
				{
					base.Dictionary.Remove("Name");
				}
			}
			return m_appearance;
		}
		set
		{
			if (m_appearance != value)
			{
				if (m_appearance == null && !m_standardStampAppearance)
				{
					base.Dictionary.Remove("Name");
				}
				m_appearance = value;
				if (Opacity < 1f)
				{
					m_appearance.Normal.Graphics.SetTransparency(Opacity);
				}
			}
			NotifyPropertyChanged("Appearance");
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

	private string IconName => ObtainIconName(Icon);

	private PdfColor BackGroundColor => ObtainBackGroundColor();

	private PdfColor BorderColor => ObtainBorderColor();

	public PdfRubberStampAnnotation()
	{
	}

	public PdfRubberStampAnnotation(RectangleF rectangle)
		: base(rectangle)
	{
	}

	public PdfRubberStampAnnotation(RectangleF rectangle, string text)
		: base(rectangle)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		Text = text;
	}

	internal PdfRubberStampAnnotation(RectangleF rectangle, bool alterRotateBounds = true)
		: base(rectangle)
	{
		m_alterRotateBounds = alterRotateBounds;
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Stamp"));
	}

	private string ObtainIconName(PdfRubberStampAnnotationIcon icon)
	{
		switch (icon)
		{
		case PdfRubberStampAnnotationIcon.Approved:
			m_icon = "APPROVED";
			m_stampWidth = 126f;
			break;
		case PdfRubberStampAnnotationIcon.AsIs:
			m_icon = "AS IS";
			m_stampWidth = 75f;
			break;
		case PdfRubberStampAnnotationIcon.Confidential:
			m_icon = "CONFIDENTIAL";
			m_stampWidth = 166f;
			break;
		case PdfRubberStampAnnotationIcon.Departmental:
			m_icon = "DEPARTMENTAL";
			m_stampWidth = 186f;
			break;
		case PdfRubberStampAnnotationIcon.Draft:
			m_icon = "DRAFT";
			m_stampWidth = 90f;
			break;
		case PdfRubberStampAnnotationIcon.Experimental:
			m_icon = "EXPERIMENTAL";
			m_stampWidth = 176f;
			break;
		case PdfRubberStampAnnotationIcon.Expired:
			m_icon = "EXPIRED";
			m_stampWidth = 116f;
			break;
		case PdfRubberStampAnnotationIcon.Final:
			m_icon = "FINAL";
			m_stampWidth = 90f;
			break;
		case PdfRubberStampAnnotationIcon.ForComment:
			m_icon = "FOR COMMENT";
			m_stampWidth = 166f;
			break;
		case PdfRubberStampAnnotationIcon.ForPublicRelease:
			m_icon = "FOR PUBLIC RELEASE";
			m_stampWidth = 240f;
			break;
		case PdfRubberStampAnnotationIcon.NotApproved:
			m_icon = "NOT APPROVED";
			m_stampWidth = 186f;
			break;
		case PdfRubberStampAnnotationIcon.NotForPublicRelease:
			m_icon = "NOT FOR PUBLIC RELEASE";
			m_stampWidth = 290f;
			break;
		case PdfRubberStampAnnotationIcon.Sold:
			m_icon = "SOLD";
			m_stampWidth = 75f;
			break;
		case PdfRubberStampAnnotationIcon.TopSecret:
			m_icon = "TOP SECRET";
			m_stampWidth = 146f;
			break;
		case PdfRubberStampAnnotationIcon.Completed:
			m_icon = "COMPLETED";
			m_stampWidth = 136f;
			break;
		case PdfRubberStampAnnotationIcon.Void:
			m_icon = "VOID";
			m_stampWidth = 75f;
			break;
		case PdfRubberStampAnnotationIcon.InformationOnly:
			m_icon = "INFORMATION ONLY";
			m_stampWidth = 230f;
			break;
		case PdfRubberStampAnnotationIcon.PreliminaryResults:
			m_icon = "PRELIMINARY RESULTS";
			m_stampWidth = 260f;
			break;
		}
		return m_icon;
	}

	private PdfColor ObtainBackGroundColor()
	{
		PdfColor pdfColor = default(PdfColor);
		if (Icon == PdfRubberStampAnnotationIcon.NotApproved || Icon == PdfRubberStampAnnotationIcon.Void)
		{
			float red = 0.9843137f;
			float green = 74f / 85f;
			float blue = 13f / 15f;
			pdfColor = new PdfColor(red, green, blue);
		}
		else if (Icon == PdfRubberStampAnnotationIcon.Approved || Icon == PdfRubberStampAnnotationIcon.Final || Icon == PdfRubberStampAnnotationIcon.Completed)
		{
			float red2 = 0.8980392f;
			float green2 = 14f / 15f;
			float blue2 = 74f / 85f;
			pdfColor = new PdfColor(red2, green2, blue2);
		}
		else
		{
			float red3 = 73f / 85f;
			float green3 = 0.8901961f;
			float blue3 = 0.9411765f;
			pdfColor = new PdfColor(red3, green3, blue3);
		}
		return pdfColor;
	}

	private PdfColor ObtainBorderColor()
	{
		PdfColor pdfColor = default(PdfColor);
		if (Icon == PdfRubberStampAnnotationIcon.NotApproved || Icon == PdfRubberStampAnnotationIcon.Void)
		{
			float red = 0.5921569f;
			float green = 0.09019608f;
			float blue = 1f / 17f;
			pdfColor = new PdfColor(red, green, blue);
		}
		else if (Icon == PdfRubberStampAnnotationIcon.Approved || Icon == PdfRubberStampAnnotationIcon.Final || Icon == PdfRubberStampAnnotationIcon.Completed)
		{
			float red2 = 0.28627452f;
			float green2 = 22f / 51f;
			float blue2 = 0.14901961f;
			pdfColor = new PdfColor(red2, green2, blue2);
		}
		else
		{
			float red3 = 8f / 85f;
			float green3 = 0.14509805f;
			float blue3 = 20f / 51f;
			pdfColor = new PdfColor(red3, green3, blue3);
		}
		return pdfColor;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
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
			m_appearance = null;
		}
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenPopup();
		}
		if (!isExternalFlatten && !base.Flatten)
		{
			base.Save();
		}
		if (!isExternalFlatten && m_appearance != null)
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

	protected override void Save()
	{
		if (!m_saved)
		{
			CheckFlatten();
			SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
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
		bool isNormalMatrix = base.Rotate == PdfAnnotationRotateAngle.RotateAngle0;
		page.Graphics.Save();
		RectangleF rectangleF = CalculateTemplateBounds(Bounds, page, appearance, isNormalMatrix);
		if (base.RotateAngle == 0f)
		{
			m_size = rectangleF.Size;
		}
		else
		{
			m_size = appearance.Size;
		}
		m_location = rectangleF.Location;
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

	internal PdfTemplate CreateAppearance()
	{
		PdfTemplate pdfTemplate = null;
		if (m_appearance != null)
		{
			pdfTemplate = CustomAppearance(pdfTemplate);
			SetCustomStampIcon();
			m_resetAppearance = false;
		}
		if (m_appearance == null && !base.Dictionary.ContainsKey("Name"))
		{
			SetDefaultIcon();
		}
		if (m_appearance == null && m_resetAppearance)
		{
			pdfTemplate = CreateStampAppearance(pdfTemplate);
			m_standardStampAppearance = true;
			m_resetAppearance = false;
		}
		return pdfTemplate;
	}

	private PdfTemplate CreateStampAppearance(PdfTemplate template)
	{
		PdfBrush mBackBrush = new PdfSolidBrush(BackGroundColor);
		PdfPen mBorderPen = new PdfPen(BorderColor, Border.Width);
		m_icon = IconName;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Center;
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
		template = new PdfTemplate(new RectangleF(0f, 0f, Bounds.Width, Bounds.Height));
		if (m_alterRotateBounds && (base.Rotate != 0 || base.RotateAngle != 0f))
		{
			rotateAngle = base.RotateAngle;
			if (rotateAngle == 0f)
			{
				rotateAngle = (int)base.Rotate * 90;
			}
			Bounds = GetRotatedBounds(Bounds, rotateAngle);
		}
		SetMatrix(template.m_content);
		PdfGraphics graphics = template.Graphics;
		graphics.ScaleTransform(template.Size.Width / (m_stampWidth + 4f), template.Size.Height / 28f);
		PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20f, PdfFontStyle.Bold | PdfFontStyle.Italic);
		if (new PdfPath() != null)
		{
			if (Opacity < 1f)
			{
				PdfGraphicsState state = graphics.Save();
				graphics.SetTransparency(Opacity);
				DrawRubberStamp(graphics, mBorderPen, mBackBrush, font, pdfStringFormat);
				graphics.Restore(state);
			}
			else
			{
				DrawRubberStamp(graphics, mBorderPen, mBackBrush, font, pdfStringFormat);
			}
		}
		return template;
	}

	private void SetDefaultIcon()
	{
		if (IconName == "DRAFT")
		{
			base.Dictionary.SetName("Name", "#23" + m_rubberStampAnnotaionIcon);
		}
	}

	private PdfTemplate CustomAppearance(PdfTemplate template)
	{
		if (m_appearance != null && m_appearance.Normal != null)
		{
			if (m_alterRotateBounds && (base.Rotate != 0 || base.RotateAngle != 0f))
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

	private void DrawRubberStamp(PdfGraphics graphics, PdfPath path, PdfPen mBorderPen, PdfBrush mBackBrush)
	{
		graphics.DrawRoundedRectangle(new RectangleF(2f, 1f, m_stampWidth, 26f), 3, mBorderPen, mBackBrush);
		graphics.DrawPath(new PdfSolidBrush(BorderColor), path);
	}

	private void DrawRubberStamp(PdfGraphics graphics, PdfPen mBorderPen, PdfBrush mBackBrush, PdfFont font, PdfStringFormat stringFormat)
	{
		graphics.DrawRoundedRectangle(new RectangleF(2f, 1f, m_stampWidth, 26f), 3, mBorderPen, mBackBrush);
		PdfBrush brush = new PdfSolidBrush(BorderColor);
		graphics.DrawString(m_icon, font, brush, new PointF(m_stampWidth / 2f + 1f, 15f), stringFormat);
	}

	private new void SetMatrix(PdfDictionary template)
	{
		PdfArray pdfArray = null;
		_ = new float[0];
		if (template["BBox"] is PdfArray pdfArray2)
		{
			if (base.Rotate != 0 || base.RotateAngle != 0f)
			{
				rotateAngle = base.RotateAngle;
				if (rotateAngle == 0f)
				{
					rotateAngle = (int)base.Rotate * 90;
				}
				pdfArray = new PdfArray(GetRotatedTransformMatrixAngle(pdfArray2, rotateAngle, Bounds).Matrix.Elements);
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

	private void SetCustomStampIcon()
	{
		if (!base.Dictionary.ContainsKey("Name"))
		{
			base.Dictionary.SetName("Name", "#23CustomStamp");
		}
	}
}
