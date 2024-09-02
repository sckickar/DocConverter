using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedInkAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private List<float> m_inkList;

	private List<List<float>> m_inkListcollection;

	private int[] m_dashArray;

	private int m_borderWidth = 1;

	private float m_borderLineWidth = 1f;

	private PdfDictionary m_borderDic = new PdfDictionary();

	private PdfLineBorderStyle m_borderStyle;

	private bool m_isConatainsCropOrMediaBoxValue;

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

	public List<float> InkList
	{
		get
		{
			return ObtainInkList();
		}
		set
		{
			m_inkList = value;
			PdfArray obj = new PdfArray(m_inkList.ToArray());
			PdfArray pdfArray = new PdfArray();
			pdfArray.Add(new PdfReferenceHolder(obj));
			base.Dictionary.SetProperty("InkList", new PdfArray(pdfArray));
			InkPointsCollection = null;
			NotifyPropertyChanged("InkList");
		}
	}

	public List<List<float>> InkPointsCollection
	{
		get
		{
			if (m_inkListcollection == null)
			{
				m_inkListcollection = ObtainInkListCollection();
			}
			return m_inkListcollection;
		}
		set
		{
			m_inkListcollection = value;
			NotifyPropertyChanged("InkPointsCollection");
		}
	}

	public int BorderWidth
	{
		get
		{
			return (int)ObtainBorderWidth();
		}
		set
		{
			m_borderWidth = value;
			m_borderDic.SetProperty("W", new PdfNumber(m_borderWidth));
			NotifyPropertyChanged("BorderWidth");
		}
	}

	internal float BorderLineWidth
	{
		get
		{
			return ObtainBorderWidth();
		}
		set
		{
			m_borderLineWidth = value;
			m_borderDic.SetNumber("W", m_borderLineWidth);
			NotifyPropertyChanged("BorderLIneWidth");
		}
	}

	public PdfLineBorderStyle BorderStyle
	{
		get
		{
			return GetLineBorder();
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
			return ObtainDashArray();
		}
		set
		{
			m_dashArray = value;
			PdfArray primitive = new PdfArray(m_dashArray);
			m_borderDic.SetProperty("D", primitive);
			NotifyPropertyChanged("DashArray");
		}
	}

	internal PdfLoadedInkAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle)
		: base(dictionary, crossTable)
	{
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		if (base.Dictionary.ContainsKey("BS"))
		{
			m_borderDic = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
		}
	}

	private List<float> ObtainInkList()
	{
		List<float> list = new List<float>();
		bool flag = false;
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("InkList"))
		{
			pdfArray = m_crossTable.GetObject(base.Dictionary["InkList"]) as PdfArray;
			PdfArray pdfArray2 = m_crossTable.GetObject(pdfArray[0]) as PdfArray;
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
				PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
				PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
				PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
				if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
				{
					flag = true;
					PdfMargins pdfMargins = new PdfMargins();
					pdfMargins = ObtainMargin();
					for (int i = 0; i < pdfArray2.Count; i += 2)
					{
						float floatValue = (pdfArray2[i] as PdfNumber).FloatValue;
						float floatValue2 = (pdfArray2[i + 1] as PdfNumber).FloatValue;
						float floatValue3 = pdfNumber.FloatValue;
						float floatValue4 = pdfNumber2.FloatValue;
						floatValue = 0f - floatValue3 + floatValue + pdfMargins.Left;
						floatValue2 = ((m_loadedPage == null || !m_loadedPage.Dictionary.ContainsKey("MediaBox") || m_loadedPage.Dictionary.ContainsKey("CropBox") || pdfNumber3.FloatValue != 0f || !(pdfNumber2.FloatValue > 0f)) ? (floatValue2 - floatValue4 - pdfMargins.Top) : (floatValue2 - pdfNumber3.FloatValue - pdfMargins.Top));
						list.Add(floatValue);
						list.Add(floatValue2);
					}
				}
			}
			if (!flag && pdfArray2 != null && pdfArray2.Elements.Count > 0)
			{
				foreach (PdfNumber item in pdfArray2)
				{
					list.Add(item.FloatValue);
				}
			}
		}
		return list;
	}

	private List<List<float>> ObtainInkListCollection()
	{
		List<List<float>> list = new List<List<float>>();
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("InkList"))
		{
			bool flag = false;
			pdfArray = m_crossTable.GetObject(base.Dictionary["InkList"]) as PdfArray;
			for (int i = 0; i < pdfArray.Count; i++)
			{
				PdfArray pdfArray2 = m_crossTable.GetObject(pdfArray[i]) as PdfArray;
				List<float> list2 = new List<float>();
				PdfArray pdfArray3 = null;
				PdfArray cropOrMediaBox = null;
				cropOrMediaBox = GetCropOrMediaBox(base.LoadedPage, cropOrMediaBox);
				if (cropOrMediaBox != null && cropOrMediaBox.Count > 3)
				{
					PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
					PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
					PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
					if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
					{
						flag = true;
						PdfMargins pdfMargins = new PdfMargins();
						pdfMargins = ObtainMargin();
						pdfArray3 = pdfArray2;
						for (int j = 0; j < pdfArray2.Count; j += 2)
						{
							float floatValue = (pdfArray2[j] as PdfNumber).FloatValue;
							float floatValue2 = (pdfArray2[j + 1] as PdfNumber).FloatValue;
							float floatValue3 = pdfNumber.FloatValue;
							float floatValue4 = pdfNumber2.FloatValue;
							floatValue = 0f - floatValue3 + floatValue + pdfMargins.Left;
							floatValue2 = ((m_loadedPage == null || !m_loadedPage.Dictionary.ContainsKey("MediaBox") || m_loadedPage.Dictionary.ContainsKey("CropBox") || pdfNumber3.FloatValue != 0f || !(pdfNumber2.FloatValue > 0f)) ? (floatValue2 - floatValue4 - pdfMargins.Top) : (floatValue2 - pdfNumber3.FloatValue - pdfMargins.Top));
							(pdfArray3[j] as PdfNumber).FloatValue = floatValue;
							(pdfArray3[j + 1] as PdfNumber).FloatValue = floatValue2;
						}
						if (pdfArray3 != null && pdfArray3.Elements.Count > 0)
						{
							foreach (PdfNumber item in pdfArray3)
							{
								list2.Add(item.FloatValue);
							}
						}
					}
				}
				if (!flag)
				{
					foreach (PdfNumber item2 in m_crossTable.GetObject(pdfArray2) as PdfArray)
					{
						list2.Add(item2.FloatValue);
					}
				}
				list.Add(list2);
			}
		}
		return list;
	}

	private float ObtainBorderWidth()
	{
		float result = 1f;
		if (base.Dictionary.ContainsKey("Border"))
		{
			if (PdfCrossTable.Dereference(base.Dictionary["Border"]) is PdfArray { Count: >=2 } pdfArray && pdfArray[2] is PdfNumber)
			{
				result = (pdfArray[2] as PdfNumber).FloatValue;
			}
		}
		else if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("W"))
			{
				result = (pdfDictionary["W"] as PdfNumber).FloatValue;
			}
		}
		return result;
	}

	private int[] ObtainDashArray()
	{
		List<int> list = new List<int>();
		PdfArray pdfArray = null;
		if (base.Dictionary.ContainsKey("BS"))
		{
			PdfDictionary pdfDictionary = m_crossTable.GetObject(base.Dictionary["BS"]) as PdfDictionary;
			if (pdfDictionary.ContainsKey("D"))
			{
				pdfArray = m_crossTable.GetObject(pdfDictionary["D"]) as PdfArray;
				for (int i = 0; i < pdfArray.Count; i++)
				{
					list.Add((pdfArray[i] as PdfNumber).IntValue);
				}
			}
		}
		return list.ToArray();
	}

	private void GetBoundsValue()
	{
		if (InkPointsCollection == null || m_inkListcollection.Count <= 0)
		{
			return;
		}
		List<float> list = new List<float>();
		for (int i = 0; i < m_inkListcollection.Count; i++)
		{
			if (m_inkListcollection[i] != null && m_inkListcollection[i].Count % 2 == 0)
			{
				for (int j = 0; j < m_inkListcollection[i].Count; j++)
				{
					list.Add(m_inkListcollection[i][j]);
				}
			}
		}
		if (list.Count == 2)
		{
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
		if (array.Length == 0)
		{
			return;
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		bool flag = true;
		PointF[] array2 = array;
		for (int l = 0; l < array2.Length; l++)
		{
			PointF pointF = array2[l];
			if (flag)
			{
				num2 = pointF.X;
				num3 = pointF.Y;
				flag = false;
				continue;
			}
			if (pointF.X < num2)
			{
				num2 = pointF.X;
			}
			if (pointF.X > num4)
			{
				num4 = pointF.X;
			}
			if (pointF.Y < num3)
			{
				num3 = pointF.Y;
			}
			if (pointF.Y > num5)
			{
				num5 = pointF.Y;
			}
		}
		Bounds = new RectangleF(num2, num3, num4 - num2, num5 - num3);
		if (base.Flatten || base.SetAppearanceDictionary)
		{
			Bounds = new RectangleF(Bounds.X - (float)BorderWidth, Bounds.Y - (float)BorderWidth, Bounds.Width + (float)(2 * BorderWidth), Bounds.Height + (float)(2 * BorderWidth));
		}
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
		AddInkPoints();
		SaveAnnotationBorder(BorderLineWidth);
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

	private PdfTemplate CreateAppearance()
	{
		if (base.SetAppearanceDictionary)
		{
			RectangleF bounds = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			if (!base.Flatten || bounds.X < 0f || bounds.Y < 0f || !m_isConatainsCropOrMediaBoxValue)
			{
				GetBoundsValue();
			}
			RectangleF rectangleF = new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			if (base.Flatten && (bounds.X < 0f || bounds.Y < 0f))
			{
				Bounds = bounds;
			}
			PdfTemplate pdfTemplate = new PdfTemplate(rectangleF);
			SetMatrix(pdfTemplate.m_content);
			pdfTemplate.m_writeTransformation = false;
			PdfGraphics graphics = pdfTemplate.Graphics;
			if (m_inkListcollection != null && m_inkListcollection.Count > 0)
			{
				for (int i = 0; i < m_inkListcollection.Count; i++)
				{
					if (m_inkListcollection[i].Count % 2 != 0)
					{
						continue;
					}
					float[] array = m_inkListcollection[i].ToArray();
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
					if (num5 == 0)
					{
						return pdfTemplate;
					}
					int num6 = num5 + num5 * 2 - 2;
					PointF[] array3 = new PointF[num6];
					GetControlPoints(array2, out PointF[] controlPointOne, out PointF[] controlPointTwo);
					int num7 = 0;
					for (int k = 0; k < num6 - 1; k += 3)
					{
						array3[k] = array2[num7];
						array3[k + 1] = controlPointOne[num7];
						array3[k + 2] = controlPointTwo[num7];
						num7++;
					}
					array3[num6 - 1] = array2[^1];
					if (array3 != null)
					{
						PointF[] array4 = array3;
						for (int l = 0; l < array4.Length; l++)
						{
							PointF pointF = array4[l];
							array4[l] = new PointF(pointF.X, 0f - pointF.Y);
						}
						PdfPath pdfPath = new PdfPath();
						PdfPath pdfPath2 = null;
						if (array2.Length == 2 && array2[1].X > array2[0].X)
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
							PdfPen pdfPen = new PdfPen(Color, BorderLineWidth);
							pdfPen.StartCap = PdfLineCap.Round;
							pdfPen.EndCap = PdfLineCap.Round;
							graphics.DrawPath(pdfPen, pdfPath2);
							graphics.Restore(state);
						}
						else
						{
							graphics.DrawPath(new PdfPen(Color, BorderLineWidth), pdfPath2);
						}
					}
				}
				if (!base.Flatten && m_isConatainsCropOrMediaBoxValue)
				{
					base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangleF));
				}
				else if (!m_isConatainsCropOrMediaBoxValue)
				{
					base.Dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangleF));
				}
			}
			return pdfTemplate;
		}
		return null;
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
					bool isNormalMatrix = ValidateTemplateMatrix(pdfDictionary2);
					if (!base.Dictionary.ContainsKey("Matrix"))
					{
						SetMatrix(appearance.m_content);
					}
					FlattenAnnotationTemplate(appearance, isNormalMatrix);
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

	private void AddInkPoints()
	{
		if (m_inkList == null && InkPointsCollection == null)
		{
			return;
		}
		if (m_inkList != null && m_inkListcollection != null)
		{
			if (InkPointsCollection[0] != m_inkList)
			{
				InkPointsCollection.Insert(0, m_inkList);
			}
		}
		else if (m_inkList != null && m_inkListcollection == null)
		{
			List<List<float>> list = new List<List<float>>();
			list.Add(m_inkList);
			InkPointsCollection = list;
		}
		List<PdfArray> list2 = new List<PdfArray>();
		for (int i = 0; i < m_inkListcollection.Count; i++)
		{
			PdfArray item = new PdfArray(m_inkListcollection[i].ToArray());
			list2.Add(item);
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
			PdfNumber pdfNumber = cropOrMediaBox[0] as PdfNumber;
			PdfNumber pdfNumber2 = cropOrMediaBox[1] as PdfNumber;
			PdfNumber pdfNumber3 = cropOrMediaBox[3] as PdfNumber;
			if (pdfNumber != null && pdfNumber2 != null && pdfNumber3 != null && (pdfNumber.FloatValue != 0f || pdfNumber2.FloatValue != 0f))
			{
				m_isConatainsCropOrMediaBoxValue = true;
				PdfMargins pdfMargins = new PdfMargins();
				pdfMargins = ObtainMargin();
				for (int j = 0; j < list2.Count; j++)
				{
					PdfArray pdfArray = list2[j];
					PdfArray pdfArray2 = pdfArray;
					for (int k = 0; k < pdfArray.Count; k += 2)
					{
						float floatValue = (pdfArray[k] as PdfNumber).FloatValue;
						float floatValue2 = (pdfArray[k + 1] as PdfNumber).FloatValue;
						floatValue = floatValue + pdfNumber.FloatValue - pdfMargins.Left;
						floatValue2 = ((m_loadedPage == null || !m_loadedPage.Dictionary.ContainsKey("MediaBox") || m_loadedPage.Dictionary.ContainsKey("CropBox") || pdfNumber3.FloatValue != 0f || !(pdfNumber2.FloatValue > 0f)) ? (floatValue2 + pdfNumber2.FloatValue + pdfMargins.Top) : (floatValue2 + pdfNumber3.FloatValue + pdfMargins.Top));
						(pdfArray2[k] as PdfNumber).FloatValue = floatValue;
						(pdfArray2[k + 1] as PdfNumber).FloatValue = floatValue2;
					}
					list2[j] = pdfArray2;
				}
			}
		}
		if (!base.Flatten)
		{
			for (int l = 0; l < list2.Count; l++)
			{
				List<float> list3 = new List<float>();
				for (int m = 0; m < list2[l].Count; m++)
				{
					list3.Add((list2[l][m] as PdfNumber).FloatValue);
				}
				m_inkListcollection[l] = list3;
			}
		}
		base.Dictionary.SetProperty("InkList", new PdfArray(list2));
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
}
