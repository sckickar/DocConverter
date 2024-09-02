using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedRubberStampAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private PdfRubberStampAnnotationIcon m_name;

	private string m_icon;

	private PdfNumber rotation;

	private float m_stampWidth;

	private bool isStampAppearance;

	private RectangleF innerTemplateBounds = RectangleF.Empty;

	private float rotateAngle;

	private SizeF m_size;

	private PointF m_location;

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

	public PdfRubberStampAnnotationIcon Icon
	{
		get
		{
			return ObtainIcon();
		}
		set
		{
			m_name = value;
			base.Dictionary.SetName("Name", "#23" + m_name);
			isStampAppearance = true;
			NotifyPropertyChanged("Icon");
		}
	}

	internal RectangleF InnerTemplateBounds
	{
		get
		{
			innerTemplateBounds = ObtainInnerBounds();
			innerTemplateBounds.X = Bounds.X;
			innerTemplateBounds.Y = Bounds.Y;
			return innerTemplateBounds;
		}
	}

	private PdfColor BackGroundColor => ObtainBackGroundColor();

	private PdfColor BorderColor => ObtainBorderColor();

	internal PdfLoadedRubberStampAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_text = text;
	}

	private RectangleF ObtainInnerBounds()
	{
		RectangleF result = RectangleF.Empty;
		if (base.Dictionary.ContainsKey("AP") && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary != null && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("BBox") && PdfCrossTable.Dereference(pdfDictionary2["BBox"]) is PdfArray { Count: 4 } pdfArray)
		{
			result = pdfArray.ToRectangle();
		}
		return result;
	}

	private PdfRubberStampAnnotationIcon ObtainIcon()
	{
		PdfRubberStampAnnotationIcon result = PdfRubberStampAnnotationIcon.Draft;
		if (base.Dictionary.ContainsKey("Name"))
		{
			PdfName pdfName = base.Dictionary["Name"] as PdfName;
			if (pdfName != null && pdfName.Value != null)
			{
				result = GetIconName(pdfName.Value.ToString());
			}
		}
		return result;
	}

	private new PdfRubberStampAnnotationIcon GetIconName(string name)
	{
		name = name.TrimStart("#23".ToCharArray());
		PdfRubberStampAnnotationIcon result = PdfRubberStampAnnotationIcon.Draft;
		switch (name)
		{
		case "SBApproved":
		case "Approved":
			result = PdfRubberStampAnnotationIcon.Approved;
			m_icon = "APPROVED";
			m_stampWidth = 126f;
			break;
		case "AsIs":
		case "SBAsIs":
			result = PdfRubberStampAnnotationIcon.AsIs;
			m_icon = "AS IS";
			m_stampWidth = 75f;
			break;
		case "Confidential":
		case "SBConfidential":
			result = PdfRubberStampAnnotationIcon.Confidential;
			m_icon = "CONFIDENTIAL";
			m_stampWidth = 166f;
			break;
		case "Departmental":
		case "SBDepartmental":
			result = PdfRubberStampAnnotationIcon.Departmental;
			m_icon = "DEPARTMENTAL";
			m_stampWidth = 186f;
			break;
		case "Draft":
		case "SBDraft":
			result = PdfRubberStampAnnotationIcon.Draft;
			m_icon = "DRAFT";
			m_stampWidth = 90f;
			break;
		case "Experimental":
		case "SBExperimental":
			result = PdfRubberStampAnnotationIcon.Experimental;
			m_icon = "EXPERIMENTAL";
			m_stampWidth = 176f;
			break;
		case "Expired":
		case "SBExpired":
			result = PdfRubberStampAnnotationIcon.Expired;
			m_icon = "EXPIRED";
			m_stampWidth = 116f;
			break;
		case "Final":
		case "SBFinal":
			result = PdfRubberStampAnnotationIcon.Final;
			m_icon = "FINAL";
			m_stampWidth = 90f;
			break;
		case "ForComment":
		case "SBForComment":
			result = PdfRubberStampAnnotationIcon.ForComment;
			m_icon = "FOR COMMENT";
			m_stampWidth = 166f;
			break;
		case "SBForPublicRelease":
		case "ForPublicRelease":
			result = PdfRubberStampAnnotationIcon.ForPublicRelease;
			m_icon = "FOR PUBLIC RELEASE";
			m_stampWidth = 240f;
			break;
		case "NotApproved":
		case "SBNotApproved":
			result = PdfRubberStampAnnotationIcon.NotApproved;
			m_icon = "NOT APPROVED";
			m_stampWidth = 186f;
			break;
		case "NotForPublicRelease":
		case "SBNotForPublicRelease":
			result = PdfRubberStampAnnotationIcon.NotForPublicRelease;
			m_icon = "NOT FOR PUBLIC RELEASE";
			m_stampWidth = 290f;
			break;
		case "Sold":
		case "SBSold":
			result = PdfRubberStampAnnotationIcon.Sold;
			m_icon = "SOLD";
			m_stampWidth = 75f;
			break;
		case "TopSecret":
		case "SBTopSecret":
			result = PdfRubberStampAnnotationIcon.TopSecret;
			m_icon = "TOP SECRET";
			m_stampWidth = 146f;
			break;
		case "Void":
		case "SBVoid":
			result = PdfRubberStampAnnotationIcon.Void;
			m_icon = "VOID";
			m_stampWidth = 75f;
			break;
		case "InformationOnly":
		case "SBInformationOnly":
			result = PdfRubberStampAnnotationIcon.InformationOnly;
			m_icon = "INFORMATION ONLY";
			m_stampWidth = 230f;
			break;
		case "PreliminaryResults":
		case "SBPreliminaryResults":
			result = PdfRubberStampAnnotationIcon.PreliminaryResults;
			m_icon = "PRELIMINARY RESULTS";
			m_stampWidth = 260f;
			break;
		case "Completed":
		case "SBCompleted":
			result = PdfRubberStampAnnotationIcon.Completed;
			m_icon = "COMPLETED";
			m_stampWidth = 136f;
			break;
		}
		return result;
	}

	private PdfColor ObtainBackGroundColor()
	{
		PdfColor pdfColor = default(PdfColor);
		if (m_name == PdfRubberStampAnnotationIcon.NotApproved || m_name == PdfRubberStampAnnotationIcon.Void)
		{
			float red = 0.9843137f;
			float green = 74f / 85f;
			float blue = 13f / 15f;
			pdfColor = new PdfColor(red, green, blue);
		}
		else if (m_name == PdfRubberStampAnnotationIcon.Approved || m_name == PdfRubberStampAnnotationIcon.Final || m_name == PdfRubberStampAnnotationIcon.Completed)
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
		if (m_name == PdfRubberStampAnnotationIcon.NotApproved || m_name == PdfRubberStampAnnotationIcon.Void)
		{
			float red = 0.5921569f;
			float green = 0.09019608f;
			float blue = 1f / 17f;
			pdfColor = new PdfColor(red, green, blue);
		}
		else if (m_name == PdfRubberStampAnnotationIcon.Approved || m_name == PdfRubberStampAnnotationIcon.Final || m_name == PdfRubberStampAnnotationIcon.Completed)
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
		bool flag = true;
		if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270 && base.Page.Rotation == PdfPageRotateAngle.RotateAngle270 && appearance.m_content.ContainsKey("Matrix") && PdfCrossTable.Dereference(appearance.m_content["Matrix"]) is PdfArray { Count: 6 } pdfArray && (pdfArray[4] as PdfNumber).FloatValue == 0f && (pdfArray[5] as PdfNumber).FloatValue != 0f)
		{
			flag = false;
		}
		if (!isNormalMatrix && base.Rotate != PdfAnnotationRotateAngle.RotateAngle180 && flag)
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
		bool flag2 = false;
		if (base.Rotate != 0 && appearance.m_content != null && appearance.m_content.ContainsKey("Matrix") && PdfCrossTable.Dereference(appearance.m_content["Matrix"]) is PdfArray { Count: 6 } pdfArray2 && (pdfArray2[4] as PdfNumber).FloatValue == 0f && (pdfArray2[5] as PdfNumber).FloatValue == 0f)
		{
			flag2 = true;
		}
		float num = ((appearance.Width > 0f) ? (rectangleF.Size.Width / appearance.Width) : 1f);
		float num2 = ((appearance.Height > 0f) ? (rectangleF.Size.Height / appearance.Height) : 1f);
		bool flag3 = num != 1f || num2 != 1f;
		if (base.Rotate != PdfAnnotationRotateAngle.RotateAngle0 && flag2)
		{
			if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle90)
			{
				if (base.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					if (flag3 && (Bounds.X != 0f || Bounds.Y != 0f))
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
				else if (!flag3)
				{
					m_location.X += m_size.Height;
				}
			}
			else if (base.Rotate == PdfAnnotationRotateAngle.RotateAngle270)
			{
				if (base.Page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					if (flag && appearance.IsAnnotationTemplate && base.Page.Size.Width < base.Page.Size.Height)
					{
						m_location.Y = Bounds.Y - Bounds.Width;
					}
					else if (flag3)
					{
						m_location.Y += m_size.Width - m_size.Height;
					}
				}
				else if (!flag3 && (Bounds.X != 0f || Bounds.Y != 0f))
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
		if (base.Dictionary.ContainsKey("Name"))
		{
			PdfName pdfName = base.Dictionary["Name"] as PdfName;
			m_icon = pdfName.Value.ToString().TrimStart("#23".ToCharArray());
		}
		appearance.m_isSignatureAppearance = true;
		if (m_icon == "CustomStamp")
		{
			appearance.isCustomStamp = true;
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

	private PdfTemplate CreateAppearance()
	{
		PdfTemplate result = null;
		if (isStampAppearance && base.SetAppearanceDictionary)
		{
			result = CreateStampAppearance();
		}
		else if (base.SetAppearanceDictionary && base.Dictionary.ContainsKey("AP"))
		{
			ModifyTemplateAppearance(Bounds);
		}
		return result;
	}

	internal PdfTemplate CreateTemplate()
	{
		PdfTemplate result = null;
		if (base.Dictionary.ContainsKey("AP"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N"))
			{
				PdfDictionary pdfDictionary2 = PdfCrossTable.Dereference(pdfDictionary["N"]) as PdfDictionary;
				if (pdfDictionary2 != null && base.CrossTable != null)
				{
					if (pdfDictionary2.Count > 0)
					{
						if (base.CrossTable.Encryptor != null)
						{
							if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary3)
							{
								PdfReferenceHolder pdfReferenceHolder = pdfDictionary3["N"] as PdfReferenceHolder;
								if (pdfReferenceHolder != null && pdfReferenceHolder.Reference != null && pdfReferenceHolder.Reference.ObjNum > -1)
								{
									pdfDictionary2 = pdfDictionary2.Clone(base.CrossTable) as PdfDictionary;
									(pdfDictionary2 as PdfStream).Decrypt(base.CrossTable.Encryptor, pdfReferenceHolder.Reference.ObjNum);
								}
							}
						}
						else
						{
							pdfDictionary2 = pdfDictionary2.Clone(base.CrossTable) as PdfDictionary;
						}
					}
					if (pdfDictionary2 is PdfStream pdfStream)
					{
						if (pdfStream.Count == 0)
						{
							if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary4)
							{
								PdfReferenceHolder pdfReferenceHolder2 = pdfDictionary4["N"] as PdfReferenceHolder;
								if (pdfReferenceHolder2 != null)
								{
									PdfReference reference = pdfReferenceHolder2.Reference;
									if (reference != null)
									{
										reference.IsDisposed = true;
										PdfStream pdfStream2 = base.CrossTable.GetObject(reference) as PdfStream;
										reference.IsDisposed = false;
										if (pdfStream2 != null && pdfStream2.Count > 0)
										{
											result = new PdfTemplate(pdfStream2, isTransformBBox: true);
										}
									}
								}
							}
						}
						else
						{
							result = new PdfTemplate(pdfStream, isTransformBBox: true);
						}
					}
				}
			}
		}
		else
		{
			result = CreateStampAppearance();
		}
		return result;
	}

	private PdfTemplate CreateStampAppearance()
	{
		PdfBrush mBackBrush = new PdfSolidBrush(BackGroundColor);
		PdfPen mBorderPen = new PdfPen(BorderColor, Border.Width);
		m_name = Icon;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Center;
		pdfStringFormat.LineAlignment = PdfVerticalAlignment.Middle;
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
		graphics.ScaleTransform(pdfTemplate.Size.Width / (m_stampWidth + 4f), pdfTemplate.Size.Height / 28f);
		PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20f, PdfFontStyle.Bold | PdfFontStyle.Italic);
		if (new PdfPath() != null)
		{
			if (Opacity < 1f)
			{
				graphics.Save();
				graphics.SetTransparency(Opacity);
				DrawRubberStamp(graphics, mBorderPen, mBackBrush, font, pdfStringFormat);
				graphics.Restore();
			}
			else
			{
				DrawRubberStamp(graphics, mBorderPen, mBackBrush, font, pdfStringFormat);
			}
		}
		return pdfTemplate;
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

	protected void ModifyTemplateAppearance(RectangleF bounds)
	{
		if (PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("N") && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			PdfArray pdfArray = null;
			int num = 0;
			PdfTemplate pdfTemplate = new PdfTemplate(template);
			base.Appearance = new PdfAppearance(this);
			base.Appearance.Normal.Graphics.SetBBox(new RectangleF(0f, 0f, Bounds.Width, Bounds.Height));
			if (PdfCrossTable.Dereference(pdfTemplate.m_content["Matrix"]) is PdfArray { Count: >5 } pdfArray2)
			{
				PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
				pdfTransformationMatrix.Matrix = new Matrix((pdfArray2.Elements[0] as PdfNumber).FloatValue, (pdfArray2.Elements[1] as PdfNumber).FloatValue, (pdfArray2.Elements[2] as PdfNumber).FloatValue, (pdfArray2.Elements[3] as PdfNumber).FloatValue, (pdfArray2.Elements[4] as PdfNumber).FloatValue, (pdfArray2.Elements[5] as PdfNumber).FloatValue);
				num = ObtainGraphicsRotation(pdfTransformationMatrix);
			}
			if (num == 90 || num == 270)
			{
				base.Appearance.Normal.Graphics.ScaleTransform(Bounds.Width / pdfTemplate.Height, Bounds.Height / pdfTemplate.Width);
			}
			else
			{
				base.Appearance.Normal.Graphics.ScaleTransform(Bounds.Width / pdfTemplate.Width, Bounds.Height / pdfTemplate.Height);
			}
			if (rotationModified)
			{
				SetMatrix(base.Appearance.Normal.m_content);
			}
			else
			{
				base.SetMatrix((PdfDictionary)base.Appearance.Normal.m_content);
			}
			base.Appearance.Normal.Graphics.Save();
			if (Opacity < 1f)
			{
				base.Appearance.Normal.Graphics.SetTransparency(Opacity);
			}
			base.Appearance.Normal.Graphics.DrawPdfTemplate(pdfTemplate, new PointF(0f, 0f));
			base.Appearance.Normal.Graphics.Restore();
			base.Dictionary["AP"] = new PdfReferenceHolder(base.Appearance);
		}
	}

	private new void SetMatrix(PdfDictionary template)
	{
		PdfArray pdfArray = null;
		_ = new float[0];
		if (PdfCrossTable.Dereference(template["BBox"]) is PdfArray pdfArray2)
		{
			if (base.Rotate != 0 || base.RotateAngle != 0f)
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
}
