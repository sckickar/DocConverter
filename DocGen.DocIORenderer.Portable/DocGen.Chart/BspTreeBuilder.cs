using System.Collections;
using System.Collections.Generic;

namespace DocGen.Chart;

internal sealed class BspTreeBuilder
{
	private const double c_epsilon = 0.0005;

	private List<Polygon> m_polygons = new List<Polygon>();

	private BspNode m_root;

	public Polygon this[int index] => m_polygons[index];

	public int Count => m_polygons.Count;

	public int Add(Polygon poly)
	{
		m_polygons.Add(poly);
		return m_polygons.Count - 1;
	}

	public BspNode Build()
	{
		return Build(m_polygons);
	}

	public BspNode Build(List<Polygon> arlist)
	{
		BspNode bspNode = new BspNode();
		Polygon polygon2 = (bspNode.Plane = arlist[0]);
		bool clipPolygon = polygon2.ClipPolygon;
		List<Polygon> list = new List<Polygon>(arlist.Count);
		List<Polygon> list2 = new List<Polygon>(arlist.Count);
		int i = 1;
		for (int count = arlist.Count; i < count; i++)
		{
			Polygon polygon3 = arlist[i];
			if (polygon3 == polygon2)
			{
				continue;
			}
			switch (ClassifyPolygon(polygon2, polygon3))
			{
			case ClassifyPolyResult.OnPlane:
			case ClassifyPolyResult.ToRight:
				list2.Add(polygon3);
				break;
			case ClassifyPolyResult.ToLeft:
				if (!clipPolygon)
				{
					list.Add(polygon3);
				}
				break;
			case ClassifyPolyResult.Unknown:
			{
				SplitPolygon(polygon3, polygon2, out var backPoly, out var frontPoly);
				if (!clipPolygon)
				{
					list.AddRange(backPoly);
				}
				list2.AddRange(frontPoly);
				break;
			}
			}
		}
		if (list.Count > 0)
		{
			bspNode.Back = Build(list);
		}
		if (list2.Count > 0)
		{
			bspNode.Front = Build(list2);
		}
		return bspNode;
	}

	public int GetNodeCount(BspNode el)
	{
		if (el != null)
		{
			return 1 + GetNodeCount(el.Back) + GetNodeCount(el.Front);
		}
		return 0;
	}

	public int GetNodeCount()
	{
		return GetNodeCount(m_root);
	}

	private ClassifyPolyResult ClassifyPolygon(Polygon pln, Polygon plg)
	{
		ClassifyPolyResult result = ClassifyPolyResult.Unknown;
		Vector3D[] points = plg.Points;
		if (points == null || plg.ClipPolygon)
		{
			return result;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = !(plg is Path3D) && !(plg is Path3DCollect) && !(plg is Image3D) && !(plg is PathGroup3D);
		Vector3D normal = pln.Normal;
		double d = pln.D;
		int i = 0;
		int num4 = points.Length;
		int num5 = num4 / 2 + 1;
		for (; i < num4; i++)
		{
			double num6 = 0.0 - d - (points[i] & normal);
			if (num6 > 0.0005)
			{
				num++;
			}
			else if (num6 < -0.0005)
			{
				num2++;
			}
			else
			{
				num3++;
			}
			if (flag)
			{
				if (num > 0 && num2 > 0)
				{
					result = ClassifyPolyResult.Unknown;
					break;
				}
			}
			else if (num >= num5 || num2 >= num5)
			{
				break;
			}
		}
		if (!flag)
		{
			if (num2 < num)
			{
				return ClassifyPolyResult.ToLeft;
			}
			return ClassifyPolyResult.ToRight;
		}
		if (num3 == points.Length)
		{
			return ClassifyPolyResult.OnPlane;
		}
		if (num2 + num3 == points.Length)
		{
			return ClassifyPolyResult.ToRight;
		}
		if (num + num3 == points.Length)
		{
			return ClassifyPolyResult.ToLeft;
		}
		return ClassifyPolyResult.Unknown;
	}

	private ClassifyPointResult ClassifyPoint(Vector3D pt, Polygon pln)
	{
		ClassifyPointResult result = ClassifyPointResult.OnPlane;
		double num = 0.0 - pln.D - (pt & pln.Normal);
		if (num > 0.0005)
		{
			result = ClassifyPointResult.OnBack;
		}
		else if (num < -0.0005)
		{
			result = ClassifyPointResult.OnFront;
		}
		return result;
	}

	private void SplitPolygon(Polygon poly, Polygon part, out Polygon[] backPoly, out Polygon[] frontPoly)
	{
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		if (poly.Points != null && !poly.ClipPolygon)
		{
			ArrayList arrayList3 = new ArrayList(poly.Points.Length + 4);
			ArrayList arrayList4 = new ArrayList(4);
			ArrayList arrayList5 = new ArrayList(4);
			ArrayList arrayList6 = new ArrayList();
			ArrayList arrayList7 = new ArrayList();
			int num = poly.Points.Length;
			for (int i = 0; i < num; i++)
			{
				Vector3D vector3D = poly.Points[i];
				Vector3D vector3D2 = poly.Points[GetNext(i + 1, num)];
				ClassifyPointResult classifyPointResult = ClassifyPoint(vector3D, part);
				ClassifyPointResult classifyPointResult2 = ClassifyPoint(vector3D2, part);
				Vector3DWithIndexWithClassification value = new Vector3DWithIndexWithClassification(vector3D, arrayList3.Count, classifyPointResult);
				arrayList3.Add(value);
				if (classifyPointResult != classifyPointResult2 && classifyPointResult != ClassifyPointResult.OnPlane && classifyPointResult2 != ClassifyPointResult.OnPlane)
				{
					Vector3D vector3D3 = vector3D - vector3D2;
					double num2 = ((part.Normal * (0.0 - part.D) - vector3D2) & part.Normal) / (part.Normal & vector3D3);
					Vector3DWithIndexWithClassification value2 = new Vector3DWithIndexWithClassification(vector3D2 + vector3D3 * num2, arrayList3.Count, ClassifyPointResult.OnPlane);
					arrayList3.Add(value2);
					arrayList4.Add(value2);
					arrayList5.Add(value2);
				}
				else
				{
					if (classifyPointResult != ClassifyPointResult.OnPlane)
					{
						continue;
					}
					Vector3D pt = poly.Points[GetNext(i - 1, num)];
					ClassifyPointResult classifyPointResult3 = ClassifyPoint(pt, part);
					if (classifyPointResult3 == classifyPointResult2)
					{
						continue;
					}
					if (classifyPointResult3 != ClassifyPointResult.OnPlane && classifyPointResult2 != ClassifyPointResult.OnPlane)
					{
						arrayList4.Add(value);
						arrayList5.Add(value);
					}
					else if (classifyPointResult3 == ClassifyPointResult.OnPlane)
					{
						switch (classifyPointResult2)
						{
						case ClassifyPointResult.OnBack:
							arrayList4.Add(value);
							break;
						case ClassifyPointResult.OnFront:
							arrayList5.Add(value);
							break;
						}
					}
					else if (classifyPointResult2 == ClassifyPointResult.OnPlane)
					{
						switch (classifyPointResult3)
						{
						case ClassifyPointResult.OnBack:
							arrayList4.Add(value);
							break;
						case ClassifyPointResult.OnFront:
							arrayList5.Add(value);
							break;
						}
					}
				}
			}
			Vector3D direction = poly.Normal * part.Normal;
			if (!double.IsNaN(direction.GetLength()))
			{
				direction *= 1.0 / direction.GetLength();
			}
			if (arrayList5.Count != 0 || arrayList4.Count != 0)
			{
				PointsOnLineComparer comparer = new PointsOnLineComparer(point: (arrayList4.Count == 0) ? (arrayList5[0] as Vector3DWithIndexWithClassification).Vector : (arrayList4[0] as Vector3DWithIndexWithClassification).Vector, direction: direction);
				arrayList4.Sort(comparer);
				arrayList5.Sort(comparer);
				for (int j = 0; j < arrayList4.Count - 1; j += 2)
				{
					Vector3DWithIndexWithClassification vector3DWithIndexWithClassification = arrayList4[j] as Vector3DWithIndexWithClassification;
					Vector3DWithIndexWithClassification vector3DWithIndexWithClassification2 = arrayList4[j + 1] as Vector3DWithIndexWithClassification;
					vector3DWithIndexWithClassification.CuttingBackPoint = true;
					vector3DWithIndexWithClassification2.CuttingBackPoint = true;
					vector3DWithIndexWithClassification.CuttingBackPairIndex = vector3DWithIndexWithClassification2.Index;
					vector3DWithIndexWithClassification2.CuttingBackPairIndex = vector3DWithIndexWithClassification.Index;
				}
				for (int k = 0; k < arrayList5.Count - 1; k += 2)
				{
					Vector3DWithIndexWithClassification vector3DWithIndexWithClassification3 = arrayList5[k] as Vector3DWithIndexWithClassification;
					Vector3DWithIndexWithClassification vector3DWithIndexWithClassification4 = arrayList5[k + 1] as Vector3DWithIndexWithClassification;
					vector3DWithIndexWithClassification3.CuttingFrontPoint = true;
					vector3DWithIndexWithClassification4.CuttingFrontPoint = true;
					vector3DWithIndexWithClassification3.CuttingFrontPairIndex = vector3DWithIndexWithClassification4.Index;
					vector3DWithIndexWithClassification4.CuttingFrontPairIndex = vector3DWithIndexWithClassification3.Index;
				}
				for (int l = 0; l < arrayList4.Count - 1; l++)
				{
					Vector3DWithIndexWithClassification vector3DWithIndexWithClassification5 = arrayList4[l] as Vector3DWithIndexWithClassification;
					if (!vector3DWithIndexWithClassification5.AlreadyCuttedBack)
					{
						CutOutBackPolygon(arrayList3, vector3DWithIndexWithClassification5, arrayList6);
						if (arrayList6.Count > 2)
						{
							arrayList.Add(new Polygon((Vector3D[])arrayList6.ToArray(typeof(Vector3D)), poly));
						}
					}
				}
				for (int m = 0; m < arrayList5.Count - 1; m++)
				{
					Vector3DWithIndexWithClassification vector3DWithIndexWithClassification6 = arrayList5[m] as Vector3DWithIndexWithClassification;
					if (!vector3DWithIndexWithClassification6.AlreadyCuttedFront)
					{
						CutOutFrontPolygon(arrayList3, vector3DWithIndexWithClassification6, arrayList7);
						if (arrayList7.Count > 2)
						{
							arrayList2.Add(new Polygon((Vector3D[])arrayList7.ToArray(typeof(Vector3D)), poly));
						}
					}
				}
			}
		}
		else
		{
			arrayList.Add(poly);
			arrayList2.Add(poly);
		}
		backPoly = (Polygon[])arrayList.ToArray(typeof(Polygon));
		frontPoly = (Polygon[])arrayList2.ToArray(typeof(Polygon));
	}

	private void CutOutBackPolygon(ArrayList polyPoints, Vector3DWithIndexWithClassification vwiwc, ArrayList points)
	{
		points.Clear();
		Vector3DWithIndexWithClassification vector3DWithIndexWithClassification = vwiwc;
		while (true)
		{
			vector3DWithIndexWithClassification.AlreadyCuttedBack = true;
			points.Add(vector3DWithIndexWithClassification.Vector);
			Vector3DWithIndexWithClassification vector3DWithIndexWithClassification2 = (Vector3DWithIndexWithClassification)polyPoints[vector3DWithIndexWithClassification.CuttingBackPairIndex];
			if (vector3DWithIndexWithClassification.CuttingBackPoint)
			{
				if (!vector3DWithIndexWithClassification2.AlreadyCuttedBack)
				{
					vector3DWithIndexWithClassification = vector3DWithIndexWithClassification2;
					continue;
				}
				Vector3DWithIndexWithClassification vector3DWithIndexWithClassification3 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index - 1, polyPoints.Count)];
				Vector3DWithIndexWithClassification vector3DWithIndexWithClassification4 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index + 1, polyPoints.Count)];
				if (vector3DWithIndexWithClassification3.Result == ClassifyPointResult.OnBack && !vector3DWithIndexWithClassification3.AlreadyCuttedBack)
				{
					vector3DWithIndexWithClassification = vector3DWithIndexWithClassification3;
					continue;
				}
				if (vector3DWithIndexWithClassification4.Result == ClassifyPointResult.OnBack && !vector3DWithIndexWithClassification4.AlreadyCuttedBack)
				{
					vector3DWithIndexWithClassification = vector3DWithIndexWithClassification4;
					continue;
				}
				break;
			}
			Vector3DWithIndexWithClassification vector3DWithIndexWithClassification5 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index - 1, polyPoints.Count)];
			Vector3DWithIndexWithClassification vector3DWithIndexWithClassification6 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index + 1, polyPoints.Count)];
			if (vector3DWithIndexWithClassification5.Result != 0 && !vector3DWithIndexWithClassification5.AlreadyCuttedBack)
			{
				vector3DWithIndexWithClassification = vector3DWithIndexWithClassification5;
				continue;
			}
			if (vector3DWithIndexWithClassification6.Result != 0 && !vector3DWithIndexWithClassification6.AlreadyCuttedBack)
			{
				vector3DWithIndexWithClassification = vector3DWithIndexWithClassification6;
				continue;
			}
			break;
		}
	}

	private void CutOutFrontPolygon(ArrayList polyPoints, Vector3DWithIndexWithClassification vwiwc, ArrayList points)
	{
		points.Clear();
		Vector3DWithIndexWithClassification vector3DWithIndexWithClassification = vwiwc;
		while (true)
		{
			vector3DWithIndexWithClassification.AlreadyCuttedFront = true;
			points.Add(vector3DWithIndexWithClassification.Vector);
			Vector3DWithIndexWithClassification vector3DWithIndexWithClassification2 = (Vector3DWithIndexWithClassification)polyPoints[vector3DWithIndexWithClassification.CuttingFrontPairIndex];
			if (vector3DWithIndexWithClassification.CuttingFrontPoint)
			{
				if (!vector3DWithIndexWithClassification2.AlreadyCuttedFront)
				{
					vector3DWithIndexWithClassification = vector3DWithIndexWithClassification2;
					continue;
				}
				Vector3DWithIndexWithClassification vector3DWithIndexWithClassification3 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index - 1, polyPoints.Count)];
				Vector3DWithIndexWithClassification vector3DWithIndexWithClassification4 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index + 1, polyPoints.Count)];
				if (vector3DWithIndexWithClassification3.Result == ClassifyPointResult.OnFront && !vector3DWithIndexWithClassification3.AlreadyCuttedFront)
				{
					vector3DWithIndexWithClassification = vector3DWithIndexWithClassification3;
					continue;
				}
				if (vector3DWithIndexWithClassification4.Result == ClassifyPointResult.OnFront && !vector3DWithIndexWithClassification4.AlreadyCuttedFront)
				{
					vector3DWithIndexWithClassification = vector3DWithIndexWithClassification4;
					continue;
				}
				break;
			}
			Vector3DWithIndexWithClassification vector3DWithIndexWithClassification5 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index - 1, polyPoints.Count)];
			Vector3DWithIndexWithClassification vector3DWithIndexWithClassification6 = (Vector3DWithIndexWithClassification)polyPoints[GetNext(vector3DWithIndexWithClassification.Index + 1, polyPoints.Count)];
			if (vector3DWithIndexWithClassification5.Result != ClassifyPointResult.OnBack && !vector3DWithIndexWithClassification5.AlreadyCuttedFront)
			{
				vector3DWithIndexWithClassification = vector3DWithIndexWithClassification5;
				continue;
			}
			if (vector3DWithIndexWithClassification6.Result != ClassifyPointResult.OnBack && !vector3DWithIndexWithClassification6.AlreadyCuttedFront)
			{
				vector3DWithIndexWithClassification = vector3DWithIndexWithClassification6;
				continue;
			}
			break;
		}
	}

	private int GetNext(int i, int count)
	{
		if (i >= count)
		{
			return i - count;
		}
		if (i < 0)
		{
			return i + count;
		}
		return i;
	}
}
