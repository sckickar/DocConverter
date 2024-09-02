using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfInkAnnotation : PdfAnnotation
{
	private List<float> m_inkList;

	private bool m_isListAdded;

	private List<List<float>> m_inkPointsCollection;

	private int[] m_dashArray;

	private int m_borderWidth = 1;

	private float m_borderLineWidth = 1f;

	private PdfDictionary m_borderDic = new PdfDictionary();

	private PdfLineBorderStyle m_borderStyle;

	internal bool EnableControlPoints = true;

	private bool isContainsCropOrMediaBox;

	public List<float> InkList
	{
		get
		{
			return m_inkList;
		}
		set
		{
			m_inkList = value;
			NotifyPropertyChanged("InkList");
		}
	}

	public List<List<float>> InkPointsCollection
	{
		get
		{
			if (m_inkPointsCollection == null)
			{
				m_inkPointsCollection = new List<List<float>>();
			}
			return m_inkPointsCollection;
		}
		set
		{
			m_inkPointsCollection = value;
			NotifyPropertyChanged("InkPointsCollection");
		}
	}

	public int BorderWidth
	{
		get
		{
			return m_borderWidth;
		}
		set
		{
			m_borderWidth = value;
			m_borderLineWidth = value;
			m_borderDic.SetProperty("W", new PdfNumber(m_borderWidth));
			NotifyPropertyChanged("BorderWidth");
		}
	}

	internal float BorderLineWidth
	{
		get
		{
			return m_borderLineWidth;
		}
		set
		{
			m_borderLineWidth = value;
			m_borderWidth = (int)value;
			m_borderDic.SetNumber("W", m_borderLineWidth);
		}
	}

	public PdfLineBorderStyle BorderStyle
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

	public int[] DashArray
	{
		get
		{
			return m_dashArray;
		}
		set
		{
			m_dashArray = value;
			PdfArray primitive = new PdfArray(m_dashArray);
			m_borderDic.SetProperty("D", primitive);
			NotifyPropertyChanged("DashArray");
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

	public PdfInkAnnotation(RectangleF rectangle, List<float> linePoints)
		: base(rectangle)
	{
		InkList = linePoints;
	}

	internal PdfInkAnnotation()
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		base.Dictionary.SetProperty("Subtype", new PdfName("Ink"));
	}

	private void GetBoundsValue()
	{
		if (m_inkPointsCollection == null || m_inkPointsCollection.Count <= 0)
		{
			return;
		}
		List<float> list = new List<float>();
		for (int i = 0; i < m_inkPointsCollection.Count; i++)
		{
			if (m_inkPointsCollection[i] != null && m_inkPointsCollection[i].Count % 2 == 0)
			{
				for (int j = 0; j < m_inkPointsCollection[i].Count; j++)
				{
					list.Add(m_inkPointsCollection[i][j]);
				}
			}
		}
		bool isTwoPoints = false;
		if (list.Count == 2)
		{
			isTwoPoints = true;
			list.Add(list[0] + 1f);
			list.Add(list[1] + 1f);
		}
		PointF[] array = new PointF[list.Count / 2];
		int num = 0;
		for (int k = 0; k < list.Count; k += 2)
		{
			array[num] = new PointF(list[k], list[k + 1]);
			num++;
		}
		CalculateInkBounds(array, isTwoPoints);
	}

	private void GetControlPoints(PointF[] pointCollection, out PointF[] controlPointOne, out PointF[] controlPointTwo)
	{
		if (pointCollection == null)
		{
			throw new ArgumentNullException("pointCollection");
		}
		int num = pointCollection.Length - 1;
		if (num < 1)
		{
			throw new ArgumentException("At least two knot PointFs required", "pointCollection");
		}
		if (num == 1)
		{
			controlPointOne = new PointF[1];
			controlPointOne[0].X = (2f * pointCollection[0].X + pointCollection[1].X) / 3f;
			controlPointOne[0].Y = (2f * pointCollection[0].Y + pointCollection[1].Y) / 3f;
			controlPointTwo = new PointF[1];
			controlPointTwo[0].X = 2f * controlPointOne[0].X - pointCollection[0].X;
			controlPointTwo[0].Y = 2f * controlPointOne[0].Y - pointCollection[0].Y;
			return;
		}
		double[] array = new double[num];
		for (int i = 1; i < num - 1; i++)
		{
			array[i] = 4f * pointCollection[i].X + 2f * pointCollection[i + 1].X;
		}
		array[0] = pointCollection[0].X + 2f * pointCollection[1].X;
		array[num - 1] = (double)(8f * pointCollection[num - 1].X + pointCollection[num].X) / 2.0;
		double[] singleControlPoint = GetSingleControlPoint(array);
		for (int j = 1; j < num - 1; j++)
		{
			array[j] = 4f * pointCollection[j].Y + 2f * pointCollection[j + 1].Y;
		}
		array[0] = pointCollection[0].Y + 2f * pointCollection[1].Y;
		array[num - 1] = (double)(8f * pointCollection[num - 1].Y + pointCollection[num].Y) / 2.0;
		double[] singleControlPoint2 = GetSingleControlPoint(array);
		controlPointOne = new PointF[num];
		controlPointTwo = new PointF[num];
		for (int k = 0; k < num; k++)
		{
			controlPointOne[k] = new PointF(Convert.ToSingle(singleControlPoint[k]), Convert.ToSingle(singleControlPoint2[k]));
			if (k < num - 1)
			{
				controlPointTwo[k] = new PointF(Convert.ToSingle((double)(2f * pointCollection[k + 1].X) - singleControlPoint[k + 1]), Convert.ToSingle((double)(2f * pointCollection[k + 1].Y) - singleControlPoint2[k + 1]));
			}
			else
			{
				controlPointTwo[k] = new PointF(Convert.ToSingle(((double)pointCollection[num].X + singleControlPoint[num - 1]) / 2.0), Convert.ToSingle(((double)pointCollection[num].Y + singleControlPoint2[num - 1]) / 2.0));
			}
		}
	}

	private double[] GetSingleControlPoint(double[] rightVector)
	{
		int num = rightVector.Length;
		double[] array = new double[num];
		double[] array2 = new double[num];
		double num2 = 2.0;
		array[0] = rightVector[0] / num2;
		for (int i = 1; i < num; i++)
		{
			array2[i] = 1.0 / num2;
			num2 = ((i < num - 1) ? 4.0 : 3.5) - array2[i];
			array[i] = (rightVector[i] - array[i - 1]) / num2;
		}
		for (int j = 1; j < num; j++)
		{
			array[num - j - 1] -= array2[num - j] * array[num - j];
		}
		return array;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		base.Flatten = true;
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		AddInkPoints();
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
			if (Color.IsEmpty)
			{
				base.Dictionary.SetProperty("C", Color.ToArray());
			}
			SaveInkDictionary();
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
	}

	protected override void Save()
	{
		CheckFlatten();
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
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

	internal PdfTemplate CreateAppearance()
	{
		PdfTemplate pdfTemplate = new PdfTemplate(new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height));
		SetMatrix(pdfTemplate.m_content);
		pdfTemplate.m_writeTransformation = false;
		PdfGraphics graphics = pdfTemplate.Graphics;
		if (m_inkPointsCollection != null && m_inkPointsCollection.Count > 0)
		{
			for (int i = 0; i < m_inkPointsCollection.Count; i++)
			{
				if (m_inkPointsCollection[i].Count % 2 != 0)
				{
					continue;
				}
				float[] array = m_inkPointsCollection[i].ToArray();
				if (array.Length == 2)
				{
					float num = array[0] - 0.5f;
					float num2 = array[1] - 0.5f;
					float num3 = array[0] + 0.5f;
					float num4 = array[1] + 0.5f;
					array = new float[4] { num, num2, num3, num4 };
				}
				PointF[] array2 = new PointF[array.Length / 2];
				int num5 = 0;
				for (int j = 0; j < array.Length; j += 2)
				{
					array2[num5] = new PointF(array[j], array[j + 1]);
					num5++;
				}
				int num6 = num5 + num5 * 2 - 2;
				PointF[] array3 = new PointF[num6];
				if (EnableControlPoints)
				{
					GetControlPoints(array2, out PointF[] controlPointOne, out PointF[] controlPointTwo);
					int num7 = 0;
					for (int k = 0; k < num6 - 1; k += 3)
					{
						array3[k] = array2[num7];
						array3[k + 1] = controlPointOne[num7];
						array3[k + 2] = controlPointTwo[num7];
						num7++;
					}
				}
				else if (num5 % 3 == 1)
				{
					num6 = num5;
					array3 = new PointF[num6];
					array3 = array2;
				}
				else if (num5 % 3 == 0)
				{
					num6 = num5 + 1;
					array3 = new PointF[num6];
					for (int l = 0; l < array2.Length; l++)
					{
						array3[l] = array2[l];
					}
				}
				else
				{
					num6 = num5 + 2;
					array3 = new PointF[num6];
					for (int m = 0; m < array2.Length; m++)
					{
						array3[m] = array2[m];
					}
					array3[num6 - 2] = array2[^2];
				}
				array3[num6 - 1] = array2[^1];
				if (array3 != null)
				{
					PointF[] array4 = array3;
					for (int n = 0; n < array4.Length; n++)
					{
						PointF pointF = array4[n];
						array4[n] = new PointF(pointF.X, 0f - pointF.Y);
					}
					PdfPath pdfPath = new PdfPath();
					PdfPath pdfPath2 = null;
					if (array2.Length == 2)
					{
						float width = array2[1].X - array2[0].X;
						float num8 = array2[1].Y - array2[0].Y;
						pdfPath.AddEllipse(array2[0].X + 0.5f, 0f - (array2[0].Y + num8 + 0.5f), width, num8);
						pdfPath2 = new PdfPath(pdfPath.PathPoints, pdfPath.PathTypes);
					}
					else
					{
						pdfPath.AddBeziers(array4);
						pdfPath2 = new PdfPath(array4, pdfPath.PathTypes);
					}
					if (Opacity < 1f)
					{
						PdfGraphicsState state = graphics.Save();
						graphics.SetTransparency(Opacity);
						graphics.DrawPath(new PdfPen(Color, BorderLineWidth), pdfPath2);
						graphics.Restore(state);
					}
					else
					{
						graphics.DrawPath(new PdfPen(Color, BorderLineWidth), pdfPath2);
					}
				}
			}
			if (base.Flatten)
			{
				PdfMargins pdfMargins = new PdfMargins();
				pdfMargins = ObtainMargin();
				float num9 = ((base.Page != null) ? base.Page.Size.Height : base.LoadedPage.Size.Height);
				Bounds = new RectangleF(Bounds.X - pdfMargins.Left, num9 - (Bounds.Y + Bounds.Height) - pdfMargins.Top, Bounds.Width, Bounds.Height);
			}
		}
		return pdfTemplate;
	}

	internal void AddInkPoints()
	{
		List<PdfArray> list = new List<PdfArray>();
		if (m_inkList != null && !m_isListAdded)
		{
			InkPointsCollection.Insert(0, m_inkList);
			m_isListAdded = true;
		}
		if (m_inkPointsCollection != null)
		{
			for (int i = 0; i < m_inkPointsCollection.Count; i++)
			{
				PdfArray item = new PdfArray(m_inkPointsCollection[i].ToArray());
				list.Add(item);
			}
		}
		PdfArray cropOrMediaBox = null;
		if (base.Page == null)
		{
			_ = base.LoadedPage.Size.Height;
		}
		else
		{
			_ = base.Page.Size.Height;
		}
		cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
		{
			if ((cropOrMediaBox[3] as PdfNumber).FloatValue < 0f)
			{
				float floatValue = (cropOrMediaBox[1] as PdfNumber).FloatValue;
				float floatValue2 = (cropOrMediaBox[3] as PdfNumber).FloatValue;
				(cropOrMediaBox[1] as PdfNumber).FloatValue = floatValue2;
				(cropOrMediaBox[3] as PdfNumber).FloatValue = floatValue;
			}
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				isContainsCropOrMediaBox = true;
				PdfMargins pdfMargins = new PdfMargins();
				pdfMargins = ObtainMargin();
				for (int j = 0; j < list.Count; j++)
				{
					PdfArray pdfArray = list[j];
					PdfArray pdfArray2 = pdfArray;
					for (int k = 0; k < pdfArray.Count; k += 2)
					{
						float floatValue3 = (pdfArray[k] as PdfNumber).FloatValue;
						float floatValue4 = (pdfArray[k + 1] as PdfNumber).FloatValue;
						floatValue3 = floatValue3 + pdfNumber.FloatValue - pdfMargins.Left;
						floatValue4 = ((m_loadedPage == null || !m_loadedPage.Dictionary.ContainsKey("MediaBox") || m_loadedPage.Dictionary.ContainsKey("CropBox") || pdfNumber3.FloatValue != 0f || !(pdfNumber2.FloatValue > 0f)) ? (floatValue4 + pdfNumber2.FloatValue + pdfMargins.Top) : (floatValue4 + pdfNumber3.FloatValue + pdfMargins.Top));
						(pdfArray2[k] as PdfNumber).FloatValue = floatValue3;
						(pdfArray2[k + 1] as PdfNumber).FloatValue = floatValue4;
					}
					list[j] = pdfArray2;
				}
			}
		}
		base.Dictionary.SetProperty("InkList", new PdfArray(list));
		if (EnableControlPoints || isContainsCropOrMediaBox)
		{
			GetBoundsValue();
		}
		if (base.Flatten)
		{
			return;
		}
		for (int l = 0; l < list.Count; l++)
		{
			List<float> list2 = new List<float>();
			for (int m = 0; m < list[l].Count; m++)
			{
				list2.Add((list[l][m] as PdfNumber).FloatValue);
			}
			m_inkPointsCollection[l] = list2;
		}
	}

	internal void AddInkPointCollection()
	{
		List<PdfArray> list = new List<PdfArray>();
		if (m_inkList != null && !m_isListAdded)
		{
			InkPointsCollection.Insert(0, m_inkList);
			m_isListAdded = true;
		}
		if (m_inkPointsCollection != null)
		{
			for (int i = 0; i < m_inkPointsCollection.Count; i++)
			{
				PdfArray item = new PdfArray(m_inkPointsCollection[i].ToArray());
				list.Add(item);
			}
		}
		if (EnableControlPoints || isContainsCropOrMediaBox)
		{
			GetBoundsValue();
		}
	}

	private void SaveInkDictionary()
	{
		base.Save();
		base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(Bounds));
		m_borderDic.SetProperty("Type", new PdfName("Border"));
		base.Dictionary.SetProperty("BS", new PdfReferenceHolder(m_borderDic));
	}

	private void CalculateInkBounds(PointF[] pointCollection, bool isTwoPoints)
	{
		if (pointCollection.Length <= 5)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		bool flag = true;
		for (int i = 0; i < pointCollection.Length; i++)
		{
			PointF pointF = pointCollection[i];
			if (flag)
			{
				num = pointF.X;
				num2 = pointF.Y;
				num3 = pointF.X;
				num4 = pointF.Y;
				flag = false;
				continue;
			}
			if (pointF.X < num)
			{
				num = pointF.X;
			}
			else if (pointF.X > num3)
			{
				num3 = pointF.X;
			}
			if (pointF.Y < num2)
			{
				num2 = pointF.Y;
			}
			else if (pointF.Y > num4)
			{
				num4 = pointF.Y;
			}
		}
		if (Bounds.Width != 0f && Bounds.Width < num3)
		{
			num3 = Bounds.Width;
		}
		else if (isContainsCropOrMediaBox)
		{
			num3 -= num;
		}
		if (Bounds.Height != 0f && Bounds.Height < num4)
		{
			num4 = Bounds.Height;
		}
		else if (isContainsCropOrMediaBox)
		{
			num4 -= num2;
		}
		PdfArray cropOrMediaBox = null;
		cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
		if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
		{
			float num5 = num;
			float num6 = num2;
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				num = pdfNumber.FloatValue + num5;
				num2 = pdfNumber2.FloatValue + num6;
			}
		}
		Bounds = new RectangleF(num, num2, num3, num4);
		if (base.Flatten || base.SetAppearanceDictionary)
		{
			int num7 = (isTwoPoints ? 2 : 3);
			Bounds = new RectangleF(Bounds.X - (float)BorderWidth, Bounds.Y - (float)BorderWidth, Bounds.Width + (float)(num7 * BorderWidth), Bounds.Height + (float)(num7 * BorderWidth));
		}
	}
}
