using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedStyledAnnotation : PdfLoadedAnnotation
{
	private PdfDictionary m_dictionary;

	private PdfCrossTable m_crossTable;

	private PdfColor m_color;

	private new string m_text;

	private string m_name = string.Empty;

	private string m_author;

	private string m_subject;

	private DateTime m_modifiedDate;

	private PdfColor m_innerColor;

	private float m_opacity = 1f;

	internal bool isBounds;

	private PdfAnnotationBorder m_border;

	internal bool isOpacity;

	private bool m_isContainBorder;

	internal bool isContentUpdated;

	internal new PdfLoadedPopupAnnotationCollection m_reviewHistory;

	internal new PdfLoadedPopupAnnotationCollection m_comments;

	internal string m_rctext = string.Empty;

	public override PdfColor Color
	{
		get
		{
			return ObtainColor();
		}
		set
		{
			base.Color = value;
			m_color = value;
		}
	}

	public override float Opacity
	{
		get
		{
			return ObtainOpacity();
		}
		set
		{
			if (value < 0f || value > 1f)
			{
				throw new ArgumentException("Valid value should be between 0 to 1.");
			}
			m_opacity = value;
			base.Dictionary.SetNumber("CA", m_opacity);
			NotifyPropertyChanged("Opacity");
			isOpacity = true;
		}
	}

	public override PdfColor InnerColor
	{
		get
		{
			return ObtainInnerColor();
		}
		set
		{
			m_innerColor = value;
			if (m_innerColor.A != 0)
			{
				base.Dictionary.SetProperty("IC", m_innerColor.ToArray());
			}
			else if (base.Dictionary.ContainsKey("IC"))
			{
				base.Dictionary.Remove("IC");
			}
			NotifyPropertyChanged("InnerColor");
		}
	}

	public override string Text
	{
		get
		{
			if (string.IsNullOrEmpty(m_text))
			{
				return ObtainText();
			}
			return m_text;
		}
		set
		{
			base.Text = value;
			m_text = value;
			isContentUpdated = true;
		}
	}

	public override string Author
	{
		get
		{
			m_author = ObtainAuthor();
			return m_author;
		}
		set
		{
			m_author = value;
			base.Dictionary.SetString("T", m_author);
			NotifyPropertyChanged("Author");
		}
	}

	public override string Subject
	{
		get
		{
			m_subject = ObtainSubject();
			return m_subject;
		}
		set
		{
			m_subject = value;
			base.Dictionary.SetString("Subj", m_subject);
			NotifyPropertyChanged("Subject");
		}
	}

	public override DateTime ModifiedDate
	{
		get
		{
			return ObtainModifiedDate();
		}
		set
		{
			m_modifiedDate = value;
			base.Dictionary.SetDateTime("M", m_modifiedDate);
			NotifyPropertyChanged("ModifiedDate");
		}
	}

	public new string Name
	{
		get
		{
			if (m_name == string.Empty && base.Dictionary.ContainsKey("NM"))
			{
				PdfString pdfString = base.Dictionary["NM"] as PdfString;
				m_name = pdfString.Value;
			}
			return m_name;
		}
		set
		{
			if (value == null || value == string.Empty)
			{
				throw new ArgumentNullException("Name value cannot be null or Empty");
			}
			if (m_name != value)
			{
				m_name = value;
				base.Dictionary["NM"] = new PdfString(m_name);
			}
			NotifyPropertyChanged("Name");
		}
	}

	public override RectangleF Bounds
	{
		get
		{
			RectangleF bounds = GetBounds(base.Dictionary, base.CrossTable);
			if (base.Page != null && base.Page.Dictionary.ContainsKey("CropBox"))
			{
				PdfArray pdfArray = null;
				pdfArray = ((!(base.Page.Dictionary["CropBox"] is PdfArray)) ? ((base.Page.Dictionary["CropBox"] as PdfReferenceHolder).Object as PdfArray) : (base.Page.Dictionary["CropBox"] as PdfArray));
				if (((pdfArray[0] as PdfNumber).FloatValue != 0f || (pdfArray[1] as PdfNumber).FloatValue != 0f || base.Page.Size.Width == (pdfArray[2] as PdfNumber).FloatValue || base.Page.Size.Height == (pdfArray[3] as PdfNumber).FloatValue) && bounds.X != (pdfArray[0] as PdfNumber).FloatValue)
				{
					bounds.X -= (pdfArray[0] as PdfNumber).FloatValue;
					bounds.Y = (pdfArray[3] as PdfNumber).FloatValue - (bounds.Y + bounds.Height);
				}
				else
				{
					bounds.Y = base.Page.Size.Height - (bounds.Y + bounds.Height);
				}
			}
			else if (base.Page != null && base.Page.Dictionary.ContainsKey("MediaBox"))
			{
				PdfArray pdfArray2 = null;
				if (PdfCrossTable.Dereference(base.Page.Dictionary["MediaBox"]) is PdfArray)
				{
					pdfArray2 = PdfCrossTable.Dereference(base.Page.Dictionary["MediaBox"]) as PdfArray;
				}
				if ((pdfArray2[0] as PdfNumber).FloatValue > 0f || (pdfArray2[1] as PdfNumber).FloatValue > 0f || base.Page.Size.Width == (pdfArray2[2] as PdfNumber).FloatValue || base.Page.Size.Height == (pdfArray2[3] as PdfNumber).FloatValue)
				{
					bounds.X -= (pdfArray2[0] as PdfNumber).FloatValue;
					if ((pdfArray2[1] as PdfNumber).FloatValue > 0f && (pdfArray2[3] as PdfNumber).FloatValue == 0f)
					{
						bounds.Y = base.Page.Size.Height - (bounds.Y + bounds.Height);
					}
					else
					{
						bounds.Y = (pdfArray2[3] as PdfNumber).FloatValue - (bounds.Y + bounds.Height);
					}
				}
				else
				{
					bounds.Y = base.Page.Size.Height - (bounds.Y + bounds.Height);
				}
			}
			else if (base.Page != null)
			{
				bounds.Y = base.Page.Size.Height - (bounds.Y + bounds.Height);
			}
			else
			{
				bounds.Y += bounds.Height;
			}
			return bounds;
		}
		set
		{
			RectangleF rectangleF = value;
			isBounds = true;
			if (rectangleF == RectangleF.Empty && !(this is PdfLoadedPopupAnnotation))
			{
				throw new ArgumentNullException("rectangle");
			}
			float height = base.Page.Size.Height;
			if (base.Page != null && base.Page.Dictionary.ContainsKey("CropBox"))
			{
				PdfArray pdfArray = null;
				pdfArray = ((!(base.Page.Dictionary["CropBox"] is PdfArray)) ? ((base.Page.Dictionary["CropBox"] as PdfReferenceHolder).Object as PdfArray) : (base.Page.Dictionary["CropBox"] as PdfArray));
				if (((pdfArray[0] as PdfNumber).FloatValue != 0f || (pdfArray[1] as PdfNumber).FloatValue != 0f || base.Page.Size.Width == (pdfArray[2] as PdfNumber).FloatValue || base.Page.Size.Height == (pdfArray[3] as PdfNumber).FloatValue) && (pdfArray[3] as PdfNumber).FloatValue != 0f)
				{
					rectangleF.X += (pdfArray[0] as PdfNumber).FloatValue;
					rectangleF.Y = (pdfArray[3] as PdfNumber).FloatValue - (rectangleF.Y + rectangleF.Height);
				}
				else
				{
					rectangleF.Y = base.Page.Size.Height - (rectangleF.Y + rectangleF.Height);
				}
			}
			else if (base.Page != null && base.Page.Dictionary.ContainsKey("MediaBox"))
			{
				PdfArray pdfArray2 = null;
				if (PdfCrossTable.Dereference(base.Page.Dictionary["MediaBox"]) is PdfArray)
				{
					pdfArray2 = PdfCrossTable.Dereference(base.Page.Dictionary["MediaBox"]) as PdfArray;
				}
				if ((pdfArray2[0] as PdfNumber).FloatValue > 0f || (pdfArray2[1] as PdfNumber).FloatValue > 0f || base.Page.Size.Width == (pdfArray2[2] as PdfNumber).FloatValue || base.Page.Size.Height == (pdfArray2[3] as PdfNumber).FloatValue)
				{
					rectangleF.X -= (pdfArray2[0] as PdfNumber).FloatValue;
					rectangleF.Y = (pdfArray2[3] as PdfNumber).FloatValue - (rectangleF.Y + rectangleF.Height);
				}
				else
				{
					rectangleF.Y = base.Page.Size.Height - (rectangleF.Y + rectangleF.Height);
				}
			}
			else
			{
				rectangleF.Y = height - (rectangleF.Y + rectangleF.Height);
			}
			PdfNumber[] array = new PdfNumber[4]
			{
				new PdfNumber(rectangleF.X),
				new PdfNumber(rectangleF.Y),
				new PdfNumber(rectangleF.X + rectangleF.Width),
				new PdfNumber(height - (height - (rectangleF.Y + rectangleF.Height)))
			};
			PdfDictionary pdfDictionary = base.Dictionary;
			if (!pdfDictionary.ContainsKey("Rect"))
			{
				pdfDictionary = GetWidgetAnnotation(base.Dictionary, base.CrossTable);
			}
			PdfDictionary pdfDictionary2 = pdfDictionary;
			IPdfPrimitive[] list = array;
			pdfDictionary2.SetArray("Rect", list);
			NotifyPropertyChanged("Bounds");
			base.Changed = true;
		}
	}

	public override PdfAnnotationBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = ObtainBorder();
				base.Dictionary.SetProperty("Border", m_border);
			}
			return m_border;
		}
		set
		{
			base.Border = value;
			base.Changed = true;
		}
	}

	public override PointF Location
	{
		get
		{
			return Bounds.Location;
		}
		set
		{
			Bounds = new RectangleF(value, Bounds.Size);
		}
	}

	public override SizeF Size
	{
		get
		{
			return Bounds.Size;
		}
		set
		{
			Bounds = new RectangleF(Bounds.Location, value);
		}
	}

	public override PdfAnnotationFlags AnnotationFlags
	{
		get
		{
			return base.AnnotationFlags = ObtainAnnotationFlags();
		}
		set
		{
			base.AnnotationFlags = value;
			base.Changed = true;
		}
	}

	internal bool IsContainBorder
	{
		get
		{
			if (base.Dictionary.ContainsKey("Border") || base.Dictionary.ContainsKey("BS"))
			{
				m_isContainBorder = true;
			}
			return m_isContainBorder;
		}
	}

	internal PdfAnnotationState ObtainState()
	{
		PdfAnnotationState result = PdfAnnotationState.None;
		if (base.Dictionary.ContainsKey("State") && base.Dictionary["State"] is PdfString pdfString)
		{
			result = pdfString.Value switch
			{
				"None" => PdfAnnotationState.None, 
				"Accepted" => PdfAnnotationState.Accepted, 
				"Cancelled" => PdfAnnotationState.Cancelled, 
				"Completed" => PdfAnnotationState.Completed, 
				"Rejected" => PdfAnnotationState.Rejected, 
				"Marked" => PdfAnnotationState.Marked, 
				"Unmarked" => PdfAnnotationState.Unmarked, 
				_ => PdfAnnotationState.Unknown, 
			};
		}
		return result;
	}

	internal PdfAnnotationStateModel ObtainStateModel()
	{
		PdfAnnotationStateModel result = PdfAnnotationStateModel.None;
		if (base.Dictionary.ContainsKey("StateModel") && base.Dictionary["StateModel"] is PdfString pdfString)
		{
			string value = pdfString.Value;
			if (!(value == "Review"))
			{
				if (value == "Marked")
				{
					result = PdfAnnotationStateModel.Marked;
				}
			}
			else
			{
				result = PdfAnnotationStateModel.Review;
			}
		}
		return result;
	}

	private string ObtainText()
	{
		if (this is PdfLoadedFreeTextAnnotation)
		{
			m_text = (this as PdfLoadedFreeTextAnnotation).MarkUpText;
			return m_text;
		}
		if (base.Dictionary.ContainsKey("Contents"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Contents"]) is PdfString pdfString)
			{
				m_text = pdfString.Value.ToString();
			}
			return m_text;
		}
		if (this is PdfLoadedWidgetAnnotation && base.Dictionary.ContainsKey("V"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["V"]) is PdfString pdfString2)
			{
				m_text = pdfString2.Value.ToString();
			}
			return m_text;
		}
		return string.Empty;
	}

	private string ObtainAuthor()
	{
		string result = null;
		if (base.Dictionary.ContainsKey("Author"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Author"]) is PdfString pdfString)
			{
				result = pdfString.Value;
			}
		}
		else if (base.Dictionary.ContainsKey("T") && PdfCrossTable.Dereference(base.Dictionary["T"]) is PdfString pdfString2)
		{
			result = pdfString2.Value;
		}
		return result;
	}

	private string ObtainSubject()
	{
		string result = null;
		if (base.Dictionary.ContainsKey("Subject"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Subject"]) is PdfString pdfString)
			{
				result = pdfString.Value;
			}
		}
		else if (base.Dictionary.ContainsKey("Subj") && PdfCrossTable.Dereference(base.Dictionary["Subj"]) is PdfString pdfString2)
		{
			result = pdfString2.Value;
		}
		return result;
	}

	private DateTime ObtainModifiedDate()
	{
		if (base.Dictionary.ContainsKey("ModDate") || base.Dictionary.ContainsKey("M"))
		{
			PdfString pdfString = PdfCrossTable.Dereference(base.Dictionary["ModDate"]) as PdfString;
			if (pdfString == null)
			{
				pdfString = PdfCrossTable.Dereference(base.Dictionary["M"]) as PdfString;
			}
			m_modifiedDate = base.Dictionary.GetDateTime(pdfString);
		}
		return m_modifiedDate;
	}

	internal RectangleF GetBounds(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		PdfArray pdfArray = null;
		if (dictionary.ContainsKey("Kids"))
		{
			PdfDictionary widgetAnnotation = GetWidgetAnnotation(dictionary, crossTable);
			if (widgetAnnotation.ContainsKey("Rect"))
			{
				pdfArray = crossTable.GetObject(widgetAnnotation["Rect"]) as PdfArray;
			}
		}
		else if (dictionary.ContainsKey("Rect"))
		{
			pdfArray = crossTable.GetObject(dictionary["Rect"]) as PdfArray;
		}
		return pdfArray.ToRectangle();
	}

	private PdfAnnotationBorder ObtainBorder()
	{
		if (base.Dictionary.ContainsKey("Border"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Border"]) is PdfArray { Count: >0 } pdfArray && pdfArray[0] is PdfNumber && pdfArray[1] is PdfNumber && pdfArray[2] is PdfNumber)
			{
				float floatValue = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue2 = (pdfArray[1] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[2] as PdfNumber).FloatValue;
				m_border = new PdfAnnotationBorder(floatValue3, floatValue, floatValue2);
				m_border.Width = floatValue3;
				m_border.HorizontalRadius = floatValue;
				m_border.VerticalRadius = floatValue2;
			}
		}
		else if (base.Dictionary.ContainsKey("BS"))
		{
			if (base.Dictionary["BS"] as PdfReferenceHolder != null)
			{
				PdfReferenceHolder pdfReferenceHolder = base.Dictionary["BS"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					PdfDictionary pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
					if (pdfDictionary.ContainsKey("W"))
					{
						float floatValue4 = (pdfDictionary["W"] as PdfNumber).FloatValue;
						m_border = new PdfAnnotationBorder();
						m_border.Width = floatValue4;
						m_border.HorizontalRadius = floatValue4;
						m_border.VerticalRadius = floatValue4;
					}
				}
			}
			else if (base.Dictionary["BS"] is PdfDictionary && base.Dictionary["BS"] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("W"))
			{
				float floatValue5 = (pdfDictionary2["W"] as PdfNumber).FloatValue;
				m_border = new PdfAnnotationBorder();
				m_border.Width = floatValue5;
				m_border.HorizontalRadius = floatValue5;
				m_border.VerticalRadius = floatValue5;
			}
		}
		else
		{
			m_border = new PdfAnnotationBorder();
		}
		if (m_border == null)
		{
			m_border = new PdfAnnotationBorder();
		}
		return m_border;
	}

	private PdfColor ObtainColor()
	{
		PdfColor result = new PdfColor(DocGen.Drawing.Color.Empty);
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("C"))
		{
			pdfArray = base.Dictionary["C"] as PdfArray;
		}
		if (pdfArray != null && pdfArray.Elements.Count == 1)
		{
			if (base.CrossTable.GetObject(pdfArray[0]) is PdfNumber pdfNumber)
			{
				result = new PdfColor(pdfNumber.FloatValue);
			}
		}
		else if (pdfArray != null && pdfArray.Elements.Count == 3)
		{
			PdfNumber pdfNumber2 = pdfArray[0] as PdfNumber;
			PdfNumber pdfNumber3 = pdfArray[1] as PdfNumber;
			PdfNumber pdfNumber4 = pdfArray[2] as PdfNumber;
			if (pdfNumber2 != null && pdfNumber3 != null && pdfNumber4 != null)
			{
				byte red = (byte)Math.Round(pdfNumber2.FloatValue * 255f);
				byte green = (byte)Math.Round(pdfNumber3.FloatValue * 255f);
				byte blue = (byte)Math.Round(pdfNumber4.FloatValue * 255f);
				result = new PdfColor(red, green, blue);
			}
		}
		else if (pdfArray != null && pdfArray.Elements.Count == 4)
		{
			PdfNumber pdfNumber5 = base.CrossTable.GetObject(pdfArray[0]) as PdfNumber;
			PdfNumber pdfNumber6 = base.CrossTable.GetObject(pdfArray[1]) as PdfNumber;
			PdfNumber pdfNumber7 = base.CrossTable.GetObject(pdfArray[2]) as PdfNumber;
			PdfNumber pdfNumber8 = base.CrossTable.GetObject(pdfArray[3]) as PdfNumber;
			if (pdfNumber5 != null && pdfNumber6 != null && pdfNumber7 != null && pdfNumber8 != null)
			{
				float floatValue = pdfNumber5.FloatValue;
				float floatValue2 = pdfNumber6.FloatValue;
				float floatValue3 = pdfNumber7.FloatValue;
				float floatValue4 = pdfNumber8.FloatValue;
				result = new PdfColor(floatValue, floatValue2, floatValue3, floatValue4);
			}
		}
		return result;
	}

	private float ObtainOpacity()
	{
		if (base.Dictionary.ContainsKey("CA"))
		{
			m_opacity = GetNumber("CA");
		}
		return m_opacity;
	}

	private float GetNumber(string keyName)
	{
		float result = 0f;
		if (m_dictionary[keyName] is PdfNumber pdfNumber)
		{
			result = pdfNumber.FloatValue;
		}
		return result;
	}

	private PdfColor ObtainInnerColor()
	{
		PdfColor result = PdfColor.Empty;
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("IC") && PdfCrossTable.Dereference(base.Dictionary["IC"]) is PdfArray { Count: 3 } pdfArray2)
		{
			byte red = (byte)Math.Round((pdfArray2[0] as PdfNumber).FloatValue * 255f);
			byte green = (byte)Math.Round((pdfArray2[1] as PdfNumber).FloatValue * 255f);
			byte blue = (byte)Math.Round((pdfArray2[2] as PdfNumber).FloatValue * 255f);
			result = new PdfColor(red, green, blue);
		}
		return result;
	}

	private PdfAnnotationFlags ObtainAnnotationFlags()
	{
		PdfAnnotationFlags result = PdfAnnotationFlags.Default;
		if (base.Dictionary.ContainsKey("F"))
		{
			result = (PdfAnnotationFlags)(PdfLoadedAnnotation.GetValue(base.Dictionary, m_crossTable, "F", inheritable: false) as PdfNumber).IntValue;
		}
		return result;
	}

	private bool IsClockWise(PointF[] points)
	{
		double num = 0.0;
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF = points[i];
			PointF pointF2 = points[(i + 1) % points.Length];
			num += (double)((pointF2.X - pointF.X) * (pointF2.Y + pointF.Y));
		}
		return num > 0.0;
	}

	private PointF GetIntersectionDegrees(PointF point1, PointF point2, float radius)
	{
		float num = point2.X - point1.X;
		float num2 = point2.Y - point1.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		float num4 = (float)(0.5 * (double)num3 / (double)radius);
		if (num4 < -1f)
		{
			num4 = -1f;
		}
		if (num4 > 1f)
		{
			num4 = 1f;
		}
		float num5 = (float)Math.Atan2(num2, num);
		float num6 = (float)Math.Acos(num4);
		return new PointF((float)((double)(num5 - num6) * (180.0 / Math.PI)), (float)((Math.PI + (double)num5 + (double)num6) * (180.0 / Math.PI)));
	}

	internal void DrawCloudStyle(PdfGraphics graphics, PdfBrush brush, PdfPen pen, float radius, float overlap, PointF[] points, bool isAppearance)
	{
		if (IsClockWise(points))
		{
			PointF[] array = new PointF[points.Length];
			int num = points.Length - 1;
			int num2 = 0;
			while (num >= 0)
			{
				array[num2] = points[num];
				num--;
				num2++;
			}
			points = array;
		}
		List<CloudStyleArc> list = new List<CloudStyleArc>();
		float num3 = 2f * radius * overlap;
		PointF pointF = points[^1];
		for (int i = 0; i < points.Length; i++)
		{
			PointF pointF2 = points[i];
			float num4 = pointF2.X - pointF.X;
			float num5 = pointF2.Y - pointF.Y;
			float num6 = (float)Math.Sqrt(num4 * num4 + num5 * num5);
			num4 /= num6;
			num5 /= num6;
			float num7 = num3;
			for (float num8 = 0f; (double)num8 + 0.1 * (double)num7 < (double)num6; num8 += num7)
			{
				CloudStyleArc cloudStyleArc = new CloudStyleArc();
				cloudStyleArc.point = new PointF(pointF.X + num8 * num4, pointF.Y + num8 * num5);
				list.Add(cloudStyleArc);
			}
			pointF = pointF2;
		}
		new PdfPath().AddPolygon(points);
		CloudStyleArc cloudStyleArc2 = list[list.Count - 1];
		for (int j = 0; j < list.Count; j++)
		{
			CloudStyleArc cloudStyleArc3 = list[j];
			PointF intersectionDegrees = GetIntersectionDegrees(cloudStyleArc2.point, cloudStyleArc3.point, radius);
			cloudStyleArc2.endAngle = intersectionDegrees.X;
			cloudStyleArc3.startAngle = intersectionDegrees.Y;
			cloudStyleArc2 = cloudStyleArc3;
		}
		PdfPath pdfPath = new PdfPath();
		for (int k = 0; k < list.Count; k++)
		{
			CloudStyleArc cloudStyleArc4 = list[k];
			float num9 = cloudStyleArc4.startAngle % 360f;
			float num10 = cloudStyleArc4.endAngle % 360f;
			float num11 = 0f;
			if (num9 > 0f && num10 < 0f)
			{
				num11 = 180f - num9 + (180f - ((num10 < 0f) ? (0f - num10) : num10));
			}
			else if (num9 < 0f && num10 > 0f)
			{
				num11 = 0f - num9 + num10;
			}
			else if (num9 > 0f && num10 > 0f)
			{
				float num12 = 0f;
				if (num9 > num10)
				{
					num12 = num9 - num10;
					num11 = 360f - num12;
				}
				else
				{
					num11 = num10 - num9;
				}
			}
			else if (num9 < 0f && num10 < 0f)
			{
				float num13 = 0f;
				if (num9 > num10)
				{
					num13 = num9 - num10;
					num11 = 360f - num13;
				}
				else
				{
					num11 = 0f - (num9 + (0f - num10));
				}
			}
			if (num11 < 0f)
			{
				num11 = 0f - num11;
			}
			cloudStyleArc4.endAngle = num11;
			pdfPath.AddArc(new RectangleF(cloudStyleArc4.point.X - radius, cloudStyleArc4.point.Y - radius, 2f * radius, 2f * radius), num9, num11);
		}
		pdfPath.CloseFigure();
		PointF[] array2 = new PointF[pdfPath.PathPoints.Length];
		if (isAppearance)
		{
			for (int l = 0; l < pdfPath.PathPoints.Length; l++)
			{
				array2[l] = new PointF(pdfPath.PathPoints[l].X, 0f - pdfPath.PathPoints[l].Y);
			}
		}
		PdfPath pdfPath2 = null;
		pdfPath2 = ((!isAppearance) ? new PdfPath(pdfPath.PathPoints, pdfPath.PathTypes) : new PdfPath(array2, pdfPath.PathTypes));
		if (brush != null)
		{
			graphics.DrawPath(brush, pdfPath2);
		}
		float num14 = 60f / (float)Math.PI;
		pdfPath = new PdfPath();
		for (int m = 0; m < list.Count; m++)
		{
			CloudStyleArc cloudStyleArc5 = list[m];
			pdfPath.AddArc(new RectangleF(cloudStyleArc5.point.X - radius, cloudStyleArc5.point.Y - radius, 2f * radius, 2f * radius), cloudStyleArc5.startAngle, cloudStyleArc5.endAngle + num14);
		}
		pdfPath.CloseFigure();
		array2 = new PointF[pdfPath.PathPoints.Length];
		if (isAppearance)
		{
			for (int n = 0; n < pdfPath.PathPoints.Length; n++)
			{
				array2[n] = new PointF(pdfPath.PathPoints[n].X, 0f - pdfPath.PathPoints[n].Y);
			}
		}
		pdfPath2 = ((!isAppearance) ? new PdfPath(pdfPath.PathPoints, pdfPath.PathTypes) : new PdfPath(array2, pdfPath.PathTypes));
		graphics.DrawPath(pen, pdfPath2);
	}

	protected new void CheckFlatten()
	{
		PdfPageBase page = base.Page;
		if (base.Page.Annotations.Count > 0 && page.Annotations.Flatten)
		{
			base.Page.Annotations.Flatten = page.Annotations.Flatten;
		}
	}

	protected void FlattenAnnotationTemplate(PdfTemplate appearance, bool isNormalMatrix)
	{
		PdfGraphics pdfGraphics = base.Page.Graphics;
		PdfGraphics pdfGraphics2 = ObtainlayerGraphics();
		if (pdfGraphics2 != null)
		{
			pdfGraphics = pdfGraphics2;
		}
		PdfGraphicsState state = pdfGraphics.Save();
		if (!isNormalMatrix)
		{
			appearance.IsAnnotationTemplate = true;
			appearance.NeedScaling = true;
		}
		if (Opacity < 1f)
		{
			pdfGraphics.SetTransparency(Opacity);
		}
		RectangleF empty = RectangleF.Empty;
		if (base.Page.Rotation == PdfPageRotateAngle.RotateAngle270 && IsRotatedTemplate(appearance.m_content))
		{
			appearance.m_content["Matrix"] = new PdfArray(new float[6] { 1f, 0f, 0f, 1f, 0f, 0f });
			RectangleF rectangleF = (appearance.m_content["BBox"] as PdfArray).ToRectangle();
			empty = CalculateBounds(Bounds, base.Page);
			empty.X -= rectangleF.X;
			empty.Y += rectangleF.Y;
		}
		else
		{
			empty = CalculateTemplateBounds(Bounds, base.Page, appearance, isNormalMatrix, pdfGraphics);
		}
		pdfGraphics.DrawPdfTemplate(appearance, empty.Location, empty.Size);
		pdfGraphics.Restore(state);
		RemoveAnnoationFromPage(base.Page, this);
	}

	internal PdfGraphics ObtainlayerGraphics()
	{
		PdfGraphics result = null;
		if (base.Layer != null)
		{
			PdfLayer pdfLayer = base.Layer;
			if (pdfLayer.LayerId == null)
			{
				pdfLayer.LayerId = "OCG_" + Guid.NewGuid();
			}
			result = pdfLayer.CreateGraphics(base.Page);
		}
		return result;
	}

	internal bool IsValidTemplateMatrix(PdfDictionary dictionary, PointF bounds, PdfTemplate template)
	{
		bool flag = true;
		PointF location = bounds;
		if (dictionary != null)
		{
			if (dictionary.ContainsKey("Matrix"))
			{
				PdfArray pdfArray = PdfCrossTable.Dereference(dictionary["BBox"]) as PdfArray;
				if (PdfCrossTable.Dereference(dictionary["Matrix"]) is PdfArray pdfArray2 && pdfArray != null && pdfArray2.Count > 3 && pdfArray.Count > 2)
				{
					if ((pdfArray2[0] as PdfNumber).FloatValue == 1f && (pdfArray2[1] as PdfNumber).FloatValue == 0f && (pdfArray2[2] as PdfNumber).FloatValue == 0f && (pdfArray2[3] as PdfNumber).FloatValue == 1f)
					{
						if (((pdfArray[0] as PdfNumber).FloatValue != 0f - (pdfArray2[4] as PdfNumber).FloatValue && (pdfArray[1] as PdfNumber).FloatValue != 0f - (pdfArray2[5] as PdfNumber).FloatValue) || ((pdfArray[0] as PdfNumber).FloatValue == 0f && 0f - (pdfArray2[4] as PdfNumber).FloatValue == 0f))
						{
							PdfGraphics pdfGraphics = base.Page.Graphics;
							PdfGraphics pdfGraphics2 = ObtainlayerGraphics();
							if (pdfGraphics2 != null)
							{
								pdfGraphics = pdfGraphics2;
							}
							PdfGraphicsState state = pdfGraphics.Save();
							if (Opacity < 1f)
							{
								pdfGraphics.SetTransparency(Opacity);
							}
							location.X -= (pdfArray[0] as PdfNumber).FloatValue;
							location.Y += (pdfArray[1] as PdfNumber).FloatValue;
							pdfGraphics.DrawPdfTemplate(template, location);
							pdfGraphics.Restore(state);
							RemoveAnnoationFromPage(base.Page, this);
							flag = false;
						}
					}
					else if ((pdfArray2[0] as PdfNumber).FloatValue == 0f && (pdfArray2[1] as PdfNumber).FloatValue == -1f && (pdfArray2[2] as PdfNumber).FloatValue == 1f && (pdfArray2[3] as PdfNumber).FloatValue == 0f)
					{
						if ((pdfArray[0] as PdfNumber).FloatValue > 0f)
						{
							flag = false;
						}
					}
					else if ((pdfArray[0] as PdfNumber).FloatValue > 0f)
					{
						flag = false;
					}
				}
			}
			if (!flag && base.Page.Dictionary.ContainsKey("MediaBox") && PdfCrossTable.Dereference(base.Page.Dictionary["MediaBox"]) is PdfArray { Count: >3 } pdfArray3)
			{
				PdfNumber pdfNumber = pdfArray3[0] as PdfNumber;
				PdfNumber pdfNumber2 = pdfArray3[1] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue < 0f || pdfNumber2.FloatValue < 0f))
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	internal PdfLineBorderStyle GetLineBorder()
	{
		PdfLineBorderStyle result = PdfLineBorderStyle.Solid;
		if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("S"))
			{
				PdfName pdfName = pdfDictionary["S"] as PdfName;
				result = GetBorderStyle(pdfName.Value.ToString());
			}
		}
		return result;
	}

	internal PdfLineBorderStyle GetBorderStyle(string bstyle)
	{
		PdfLineBorderStyle result = PdfLineBorderStyle.Solid;
		switch (bstyle)
		{
		case "S":
			result = PdfLineBorderStyle.Solid;
			break;
		case "D":
			result = PdfLineBorderStyle.Dashed;
			break;
		case "B":
			result = PdfLineBorderStyle.Beveled;
			break;
		case "I":
			result = PdfLineBorderStyle.Inset;
			break;
		case "U":
			result = PdfLineBorderStyle.Underline;
			break;
		}
		return result;
	}

	internal RectangleF ObtainBounds()
	{
		RectangleF bounds = Bounds;
		PdfArray cropOrMediaBox = null;
		cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3 && !base.LoadedPage.Dictionary.ContainsKey("CropBox"))
		{
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				float x = bounds.X;
				float y = bounds.Y;
				bounds.X = 0f - pdfNumber.FloatValue - (0f - x);
				bounds.Y = pdfNumber2.FloatValue + y;
			}
		}
		return bounds;
	}

	internal PdfLoadedStyledAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable)
		: base(dictionary, crossTable)
	{
		m_dictionary = dictionary;
		m_crossTable = crossTable;
	}

	internal void FlattenLoadedPopup()
	{
		string text = string.Empty;
		if (base.Dictionary.ContainsKey("Contents"))
		{
			text = (base.Dictionary["Contents"] as PdfString).Value;
		}
		string text2 = ObtainAuthor();
		string text3 = ObtainSubject();
		if (!base.Dictionary.ContainsKey("Popup"))
		{
			FlattenPopup(base.Page, Color, Bounds, Border, text2, text3, text);
			RemoveAnnoationFromPage(base.Page, this);
			return;
		}
		RectangleF boundsValue = GetBoundsValue();
		PdfBrush pdfBrush = new PdfSolidBrush(Color);
		float num = Border.Width / 2f;
		float trackingHeight = 0f;
		PdfBrush aBrush = new PdfSolidBrush(GetForeColor(Color));
		if (text2 != null && text2 != string.Empty)
		{
			DrawAuthor(text2, text3, boundsValue, pdfBrush, aBrush, base.Page, out trackingHeight, Border);
		}
		else if (text3 != null && text3 != string.Empty)
		{
			RectangleF rectangle = new RectangleF(boundsValue.X + num, boundsValue.Y + num, boundsValue.Width - Border.Width, 40f);
			SaveGraphics(base.Page, PdfBlendMode.HardLight);
			base.Page.Graphics.DrawRectangle(PdfPens.Black, pdfBrush, rectangle);
			base.Page.Graphics.Restore();
			RectangleF rectangleF = new RectangleF(rectangle.X + 11f, rectangle.Y, rectangle.Width, rectangle.Height / 2f);
			rectangleF = new RectangleF(rectangleF.X, rectangleF.Y + rectangleF.Height - 2f, rectangleF.Width, rectangle.Height / 2f);
			SaveGraphics(base.Page, PdfBlendMode.Normal);
			DrawSubject(text3, rectangleF, base.Page);
			trackingHeight = 40f;
			base.Page.Graphics.Restore();
		}
		else
		{
			SaveGraphics(base.Page, PdfBlendMode.HardLight);
			RectangleF rectangle2 = new RectangleF(boundsValue.X + num, boundsValue.Y + num, boundsValue.Width - Border.Width, 20f);
			base.Page.Graphics.DrawRectangle(PdfPens.Black, pdfBrush, rectangle2);
			trackingHeight = 20f;
			base.Page.Graphics.Restore();
		}
		SaveGraphics(base.Page, PdfBlendMode.HardLight);
		RectangleF rectangleF2 = new RectangleF(boundsValue.X + num, boundsValue.Y + num + trackingHeight, boundsValue.Width - Border.Width, boundsValue.Height - (trackingHeight + Border.Width));
		base.Page.Graphics.DrawRectangle(PdfPens.Black, PdfBrushes.White, rectangleF2);
		rectangleF2.X += 11f;
		rectangleF2.Y += 5f;
		rectangleF2.Width -= 22f;
		base.Page.Graphics.Restore();
		SaveGraphics(base.Page, PdfBlendMode.Normal);
		List<object> list = null;
		base.Dictionary.ContainsKey("RC");
		if (list != null)
		{
			float num2 = 0f;
			for (int i = 0; i < list.Count; i += 4)
			{
				PdfFont pdfFont = list[i] as PdfFont;
				string text4 = list[i + 3] as string;
				PdfBrush pdfBrush2 = list[i + 2] as PdfBrush;
				if (pdfBrush2 == null)
				{
					pdfBrush2 = PdfBrushes.Black;
				}
				PdfStringFormat format = new PdfStringFormat((PdfTextAlignment)list[i + 1]);
				if (text4 != null)
				{
					SizeF sizeF = pdfFont.MeasureString(text4, rectangleF2.Width);
					int count = Regex.Matches(text4, "\r\r").Count;
					float num3 = sizeF.Height;
					if (count > 0)
					{
						num3 += pdfFont.Height * (float)count;
					}
					if (rectangleF2.Height < num3)
					{
						num3 = rectangleF2.Height;
					}
					if (num2 + num3 > rectangleF2.Height)
					{
						float num4 = num2 + num3 - rectangleF2.Height;
						num3 -= num4;
					}
					base.Page.Graphics.DrawString(text4, pdfFont, pdfBrush2, new RectangleF(rectangleF2.X, rectangleF2.Y + num2, sizeF.Width, num3), format);
					num2 += num3;
				}
			}
		}
		else
		{
			base.Page.Graphics.DrawString(text, new PdfStandardFont(PdfFontFamily.Helvetica, 10.5f), PdfBrushes.Black, rectangleF2);
		}
		base.Page.Graphics.Restore();
		RemoveAnnoationFromPage(base.Page, this);
	}

	private RectangleF CalculateBounds(RectangleF bounds, PdfPageBase Page)
	{
		if (Page != null)
		{
			switch (Page.Rotation)
			{
			case PdfPageRotateAngle.RotateAngle90:
				bounds = new RectangleF(Page.Size.Height - bounds.Y - bounds.Height, bounds.X, bounds.Height, bounds.Width);
				break;
			case PdfPageRotateAngle.RotateAngle180:
				bounds = new RectangleF(Page.Size.Width - bounds.X - bounds.Width, Page.Size.Height - bounds.Y - bounds.Height, bounds.Width, bounds.Height);
				break;
			case PdfPageRotateAngle.RotateAngle270:
				bounds = new RectangleF(bounds.Y, Page.Size.Width - bounds.X - bounds.Width, bounds.Height, bounds.Width);
				break;
			}
		}
		return bounds;
	}

	private bool IsRotatedTemplate(PdfDictionary dictionary)
	{
		bool result = true;
		if (dictionary.ContainsKey("Matrix") && PdfCrossTable.Dereference(dictionary["Matrix"]) is PdfArray { Count: >3 } pdfArray && (pdfArray[0] as PdfNumber).FloatValue == 1f && (pdfArray[1] as PdfNumber).FloatValue == 0f && (pdfArray[2] as PdfNumber).FloatValue == 0f && (pdfArray[3] as PdfNumber).FloatValue == 1f)
		{
			result = false;
		}
		return result;
	}

	internal List<object> ParseXMLData()
	{
		PdfFont item = null;
		m_rctext = string.Empty;
		PdfLoadedDocument pdfLoadedDocument = m_crossTable.Document as PdfLoadedDocument;
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
		{
			item = new PdfStandardFont(PdfFontFamily.Helvetica, 10.5f, PdfFontStyle.Regular);
		}
		else if (pdfLoadedDocument.RaisePdfFont)
		{
			PdfFontEventArgs pdfFontEventArgs = new PdfFontEventArgs();
			pdfFontEventArgs.m_fontName = "Arial";
			pdfFontEventArgs.m_fontStyle = PdfFontStyle.Regular;
			pdfLoadedDocument.PdfFontStream(pdfFontEventArgs);
			item = new PdfTrueTypeFont(pdfFontEventArgs.FontStream, pdfFontEventArgs.m_fontStyle, 10.5f, string.Empty, useTrueType: true, isEnableEmbedding: true);
		}
		List<XElement> list = (from p in XDocument.Parse((base.Dictionary["RC"] as PdfString).Value).Descendants()
			where p.Name.LocalName == "span"
			select p).ToList();
		if (list.Count > 0)
		{
			List<object> list2 = new List<object>();
			{
				foreach (XElement item3 in list)
				{
					float result = 10.5f;
					PdfTextAlignment pdfTextAlignment = PdfTextAlignment.Left;
					PdfFontStyle pdfFontStyle = PdfFontStyle.Regular;
					PdfBrush item2 = null;
					if (item3.Value != null)
					{
						m_rctext += item3.Value;
					}
					if (item3.Attribute("style") != null)
					{
						string[] array = item3.Attribute("style").Value.Split(';');
						if (array.Length == 0 && item3.Parent.Elements() != null)
						{
							array = item3.Parent.Elements().ToString().Split(';');
						}
						string[] array2 = array;
						for (int i = 0; i < array2.Length; i++)
						{
							string[] array3 = array2[i].Split(':');
							switch (array3[0])
							{
							case "font-size":
								float.TryParse(array3[1].TrimEnd('p', 't').TrimStart('-'), out result);
								break;
							case "font-weight":
								if (array3[1].Contains("bold"))
								{
									pdfFontStyle |= PdfFontStyle.Bold;
								}
								break;
							case "font-style":
								if (array3[1].Contains("normal"))
								{
									pdfFontStyle |= PdfFontStyle.Regular;
								}
								if (array3[1].Contains("underline"))
								{
									pdfFontStyle |= PdfFontStyle.Underline;
								}
								if (array3[1].Contains("strikeout"))
								{
									pdfFontStyle |= PdfFontStyle.Strikeout;
								}
								if (array3[1].Contains("italic"))
								{
									pdfFontStyle |= PdfFontStyle.Italic;
								}
								if (array3[1].Contains("bold"))
								{
									pdfFontStyle |= PdfFontStyle.Bold;
								}
								break;
							case "text-align":
								switch (array3[1])
								{
								case "left":
									pdfTextAlignment = PdfTextAlignment.Left;
									break;
								case "right":
									pdfTextAlignment = PdfTextAlignment.Right;
									break;
								case "center":
									pdfTextAlignment = PdfTextAlignment.Center;
									break;
								case "justify":
									pdfTextAlignment = PdfTextAlignment.Justify;
									break;
								}
								break;
							case "color":
								item2 = new PdfSolidBrush(new PdfColor(FromHtml(array3[1])));
								break;
							case "xfa-spacerun":
								if (array3.Length > 1 && array3[1].Contains("yes"))
								{
									m_rctext += " ";
								}
								break;
							}
						}
					}
					if (PdfDocument.ConformanceLevel == PdfConformanceLevel.None)
					{
						list2.Add(new PdfStandardFont(PdfFontFamily.Helvetica, result, pdfFontStyle));
					}
					else
					{
						if (pdfLoadedDocument.RaisePdfFont)
						{
							PdfFontEventArgs pdfFontEventArgs2 = new PdfFontEventArgs();
							pdfFontEventArgs2.m_fontName = "Arial";
							pdfFontEventArgs2.m_fontStyle = PdfFontStyle.Regular;
							pdfLoadedDocument.PdfFontStream(pdfFontEventArgs2);
							item = new PdfTrueTypeFont(pdfFontEventArgs2.FontStream, pdfFontEventArgs2.m_fontStyle, result, string.Empty, useTrueType: true, isEnableEmbedding: true);
						}
						list2.Add(item);
					}
					list2.Add(pdfTextAlignment);
					list2.Add(item2);
					list2.Add((item3.FirstAttribute != null) ? item3.FirstAttribute.Value : string.Empty);
				}
				return list2;
			}
		}
		return null;
	}

	internal Color FromHtml(string colorString)
	{
		return FromHex(colorString);
	}

	private Color FromHex(string hex)
	{
		int num = int.Parse(hex.Replace("#", ""), NumberStyles.HexNumber);
		Color result = DocGen.Drawing.Color.FromArgb((byte)((num & -16777216) >> 24), (byte)((num & 0xFF0000) >> 16), (byte)((num & 0xFF00) >> 8), (byte)(num & 0xFF));
		hex = hex.Replace("#", string.Empty);
		if (hex.Contains(" "))
		{
			hex = hex.Replace(" ", string.Empty);
		}
		if (result.A == 0 && hex.Length == 6)
		{
			result = DocGen.Drawing.Color.FromArgb(255, result.R, result.G, result.B);
		}
		return result;
	}

	private RectangleF GetBoundsValue()
	{
		if (base.Dictionary.ContainsKey("Popup"))
		{
			if (PdfCrossTable.Dereference(((base.Dictionary["Popup"] as PdfReferenceHolder).Object as PdfDictionary)["Rect"]) is PdfArray pdfArray)
			{
				RectangleF result = pdfArray.ToRectangle();
				new PdfUnitConvertor();
				if (base.Page != null)
				{
					if (result.Y == 0f && result.Height == 0f)
					{
						result.Y += result.Height;
					}
					else
					{
						result.Y = base.Page.Size.Height - (result.Y + result.Height);
					}
				}
				else
				{
					result.Y -= result.Height;
				}
				return result;
			}
			return RectangleF.Empty;
		}
		return RectangleF.Empty;
	}

	private void RemoveAnnoation(PdfPageBase page, PdfAnnotation annot)
	{
		if (page is PdfPage pdfPage)
		{
			pdfPage.Annotations.Remove(annot);
			return;
		}
		PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
		PdfDictionary dictionary = pdfLoadedPage.Dictionary;
		PdfArray pdfArray = null;
		pdfArray = ((!dictionary.ContainsKey("Annots")) ? new PdfArray() : (pdfLoadedPage.CrossTable.GetObject(dictionary["Annots"]) as PdfArray));
		annot.Dictionary.SetProperty("P", new PdfReferenceHolder(pdfLoadedPage));
		int num = -1;
		List<IPdfPrimitive> list = new List<IPdfPrimitive>();
		for (int i = 0; i < pdfArray.Count; i++)
		{
			if (annot.Dictionary.ContainsKey("Popup") && (pdfArray[i] as PdfReferenceHolder).Reference == (annot.Dictionary["Popup"] as PdfReferenceHolder).Reference)
			{
				list.Add(pdfArray[i]);
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			pdfArray.RemoveAt(num);
		}
		pdfArray.Remove(new PdfReferenceHolder(annot));
		page.Dictionary.SetProperty("Annots", pdfArray);
	}

	internal void SaveAnnotationBorder(float borderWidth)
	{
		if (!base.Dictionary.ContainsKey("BS"))
		{
			return;
		}
		if (base.Dictionary["BS"] as PdfReferenceHolder != null)
		{
			PdfReferenceHolder pdfReferenceHolder = base.Dictionary["BS"] as PdfReferenceHolder;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("W"))
			{
				pdfDictionary.SetProperty("W", new PdfNumber(borderWidth));
			}
		}
		else if (base.Dictionary["BS"] is PdfDictionary && base.Dictionary["BS"] is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("W"))
		{
			pdfDictionary2.SetProperty("W", new PdfNumber(borderWidth));
		}
	}
}
