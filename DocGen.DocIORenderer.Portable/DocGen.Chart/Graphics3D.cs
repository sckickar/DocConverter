using System;
using System.Collections;
using System.ComponentModel;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class Graphics3D
{
	private bool m_light;

	private float m_lightCoef = 16f;

	private Vector3D m_lightPosition = new Vector3D(0.0, 0.0, 1.0);

	private Transform3D m_transform;

	private BspTreeBuilder m_treeBuilder = new BspTreeBuilder();

	private BspNode m_tree;

	private IList m_regions;

	private Graphics m_graph;

	private static StringFormat m_defSFormat;

	internal BspNode RootNode
	{
		get
		{
			return m_tree;
		}
		set
		{
			m_tree = value;
		}
	}

	public Polygon this[int index] => m_treeBuilder[index];

	public int Count => m_treeBuilder.Count;

	public Vector3D LightPosition
	{
		get
		{
			return m_lightPosition;
		}
		set
		{
			m_lightPosition = value;
		}
	}

	public float LightCoeficient
	{
		get
		{
			return m_lightCoef;
		}
		set
		{
			m_lightCoef = value;
		}
	}

	public bool Light
	{
		get
		{
			return m_light;
		}
		set
		{
			m_light = value;
		}
	}

	public Graphics Graphics => m_graph;

	public int CountPolygons => m_treeBuilder.GetNodeCount(m_tree);

	public IList Regions
	{
		get
		{
			return m_regions;
		}
		set
		{
			m_regions = value;
		}
	}

	public static StringFormat DefaultStrinfFormat
	{
		get
		{
			if (m_defSFormat == null)
			{
				m_defSFormat = new StringFormat();
				m_defSFormat.Alignment = StringAlignment.Near;
			}
			return m_defSFormat;
		}
	}

	public Transform3D Transform
	{
		get
		{
			if (m_transform == null)
			{
				m_transform = new Transform3D();
			}
			return m_transform;
		}
		set
		{
			if (m_transform != value)
			{
				m_transform = value;
			}
		}
	}

	public Graphics3D(Graphics g)
	{
		m_graph = g;
	}

	public int AddPolygon(Polygon polygon)
	{
		if (polygon == null || polygon.Test())
		{
			return -1;
		}
		return m_treeBuilder.Add(polygon);
	}

	[Obsolete("Use PrepareView()")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void PrepairView()
	{
		m_tree = m_treeBuilder.Build();
	}

	public void PrepareView()
	{
		m_tree = m_treeBuilder.Build();
	}

	public void View3D()
	{
		Vector3D eye = new Vector3D(0.0, 0.0, 32767.0);
		_ = Matrix3D.GetInterval(m_transform.Centered) * m_transform.View * m_transform.Centered;
		DrawBspNode3D(m_tree, eye);
	}

	public Graphics3DState SaveState()
	{
		return new Graphics3DState
		{
			Light = m_light,
			LightCoeficient = m_lightCoef,
			LightPosition = m_lightPosition
		};
	}

	public void LoadState(Graphics3DState state)
	{
		m_light = state.Light;
		m_lightCoef = state.LightCoeficient;
		m_lightPosition = state.LightPosition;
	}

	public Polygon[] CreateBox(Vector3D v1, Vector3D v2, Pen p, Brush b)
	{
		Polygon[] result = new Polygon[0];
		Vector3D vector3D = v1 - v2;
		if (vector3D.X != 0.0 && vector3D.Y != 0.0 && vector3D.Z != 0.0)
		{
			Vector3D[] points = new Vector3D[4]
			{
				new Vector3D(v1.X, v1.Y, v1.Z),
				new Vector3D(v2.X, v1.Y, v1.Z),
				new Vector3D(v2.X, v2.Y, v1.Z),
				new Vector3D(v1.X, v2.Y, v1.Z)
			};
			Vector3D[] points2 = new Vector3D[4]
			{
				new Vector3D(v1.X, v1.Y, v2.Z),
				new Vector3D(v2.X, v1.Y, v2.Z),
				new Vector3D(v2.X, v2.Y, v2.Z),
				new Vector3D(v1.X, v2.Y, v2.Z)
			};
			Vector3D[] points3 = new Vector3D[4]
			{
				new Vector3D(v1.X, v1.Y, v2.Z),
				new Vector3D(v2.X, v1.Y, v2.Z),
				new Vector3D(v2.X, v1.Y, v1.Z),
				new Vector3D(v1.X, v1.Y, v1.Z)
			};
			Vector3D[] points4 = new Vector3D[4]
			{
				new Vector3D(v1.X, v2.Y, v2.Z),
				new Vector3D(v2.X, v2.Y, v2.Z),
				new Vector3D(v2.X, v2.Y, v1.Z),
				new Vector3D(v1.X, v2.Y, v1.Z)
			};
			Vector3D[] points5 = new Vector3D[4]
			{
				new Vector3D(v1.X, v1.Y, v1.Z),
				new Vector3D(v1.X, v1.Y, v2.Z),
				new Vector3D(v1.X, v2.Y, v2.Z),
				new Vector3D(v1.X, v2.Y, v1.Z)
			};
			Vector3D[] points6 = new Vector3D[4]
			{
				new Vector3D(v2.X, v1.Y, v1.Z),
				new Vector3D(v2.X, v1.Y, v2.Z),
				new Vector3D(v2.X, v2.Y, v2.Z),
				new Vector3D(v2.X, v2.Y, v1.Z)
			};
			Polygon polygon = new Polygon(points, b, null);
			Polygon polygon2 = new Polygon(points2, b, null);
			Polygon polygon3 = new Polygon(points3, b, p);
			Polygon polygon4 = new Polygon(points4, b, p);
			Polygon polygon5 = new Polygon(points5, b, p);
			Polygon polygon6 = new Polygon(points6, b, p);
			AddPolygon(polygon);
			AddPolygon(polygon2);
			AddPolygon(polygon3);
			AddPolygon(polygon4);
			AddPolygon(polygon5);
			AddPolygon(polygon6);
			result = new Polygon[6] { polygon, polygon2, polygon3, polygon4, polygon5, polygon6 };
		}
		return result;
	}

	public Polygon[] CreateBox(Vector3D v1, Vector3D v2, Pen p, BrushInfo b)
	{
		_ = new Polygon[0];
		_ = v1 - v2;
		Vector3D[] points = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v1.Z),
			new Vector3D(v2.X, v1.Y, v1.Z),
			new Vector3D(v2.X, v2.Y, v1.Z),
			new Vector3D(v1.X, v2.Y, v1.Z)
		};
		Vector3D[] points2 = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v2.Z),
			new Vector3D(v1.X, v2.Y, v2.Z)
		};
		Vector3D[] points3 = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v1.Y, v1.Z),
			new Vector3D(v1.X, v1.Y, v1.Z)
		};
		Vector3D[] points4 = new Vector3D[4]
		{
			new Vector3D(v1.X, v2.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v1.Z),
			new Vector3D(v1.X, v2.Y, v1.Z)
		};
		Vector3D[] points5 = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v1.Z),
			new Vector3D(v1.X, v1.Y, v2.Z),
			new Vector3D(v1.X, v2.Y, v2.Z),
			new Vector3D(v1.X, v2.Y, v1.Z)
		};
		Vector3D[] points6 = new Vector3D[4]
		{
			new Vector3D(v2.X, v1.Y, v1.Z),
			new Vector3D(v2.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v1.Z)
		};
		Polygon polygon = new Polygon(points, b, null);
		Polygon polygon2 = new Polygon(points2, b, null);
		Polygon polygon3 = new Polygon(points3, b, p);
		Polygon polygon4 = new Polygon(points4, b, p);
		Polygon polygon5 = new Polygon(points5, b, p);
		Polygon polygon6 = new Polygon(points6, b, p);
		AddPolygon(polygon);
		AddPolygon(polygon2);
		AddPolygon(polygon3);
		AddPolygon(polygon4);
		AddPolygon(polygon5);
		AddPolygon(polygon6);
		return new Polygon[6] { polygon, polygon2, polygon3, polygon4, polygon5, polygon6 };
	}

	public Polygon[] CreateBoxV(Vector3D v1, Vector3D v2, Pen p, BrushInfo b)
	{
		_ = new Polygon[0];
		_ = v1 - v2;
		Vector3D[] points = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v1.Z),
			new Vector3D(v2.X, v1.Y, v1.Z),
			new Vector3D(v2.X, v2.Y, v1.Z),
			new Vector3D(v1.X, v2.Y, v1.Z)
		};
		Vector3D[] points2 = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v2.Z),
			new Vector3D(v1.X, v2.Y, v2.Z)
		};
		Vector3D[] points3 = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v1.Y, v1.Z),
			new Vector3D(v1.X, v1.Y, v1.Z)
		};
		Vector3D[] points4 = new Vector3D[4]
		{
			new Vector3D(v1.X, v2.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v1.Z),
			new Vector3D(v1.X, v2.Y, v1.Z)
		};
		Vector3D[] points5 = new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v1.Z),
			new Vector3D(v1.X, v1.Y, v2.Z),
			new Vector3D(v1.X, v2.Y, v2.Z),
			new Vector3D(v1.X, v2.Y, v1.Z)
		};
		Vector3D[] points6 = new Vector3D[4]
		{
			new Vector3D(v2.X, v1.Y, v1.Z),
			new Vector3D(v2.X, v1.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v2.Z),
			new Vector3D(v2.X, v2.Y, v1.Z)
		};
		Polygon polygon = new Polygon(points, b, p);
		Polygon polygon2 = new Polygon(points2, b, p);
		Polygon polygon3 = new Polygon(points3, b, p);
		Polygon polygon4 = new Polygon(points4, b, p);
		Polygon polygon5 = new Polygon(points5, b, null);
		Polygon polygon6 = new Polygon(points6, b, null);
		AddPolygon(polygon6);
		AddPolygon(polygon5);
		AddPolygon(polygon);
		AddPolygon(polygon2);
		AddPolygon(polygon3);
		AddPolygon(polygon4);
		return new Polygon[6] { polygon, polygon2, polygon3, polygon4, polygon5, polygon6 };
	}

	public Polygon[] CreateEllipse(Vector3D v1, SizeF sz, int dsc, Pen p, BrushInfo br)
	{
		Vector3D[] array = new Vector3D[dsc];
		double num = Math.PI * 2.0 / (double)dsc;
		float num2 = (float)(v1.X + (double)(sz.Width / 2f));
		float num3 = (float)(v1.Y + (double)(sz.Height / 2f));
		for (int i = 0; i < dsc; i++)
		{
			float num4 = (float)((double)num2 + (double)(sz.Width / 2f) * Math.Cos((double)i * num));
			float num5 = (float)((double)num3 + (double)(sz.Height / 2f) * Math.Sin((double)i * num));
			array[i] = new Vector3D(num4, num5, v1.Z);
		}
		Polygon polygon = new Polygon(array, br, p);
		AddPolygon(polygon);
		return new Polygon[1] { polygon };
	}

	public Polygon[] CreateRectangle(Vector3D v1, SizeF sz, Pen p, BrushInfo br)
	{
		Polygon polygon = new Polygon(new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v1.Z),
			new Vector3D(v1.X + (double)sz.Width, v1.Y, v1.Z),
			new Vector3D(v1.X + (double)sz.Width, v1.Y + (double)sz.Height, v1.Z),
			new Vector3D(v1.X, v1.Y + (double)sz.Height, v1.Z)
		}, br, p);
		AddPolygon(polygon);
		return new Polygon[1] { polygon };
	}

	public Polygon[] CreateRectangle(Vector3D v1, SizeF sz, Pen p, BrushInfo br, bool IsPNF)
	{
		Polygon polygon = new Polygon(new Vector3D[4]
		{
			new Vector3D(v1.X, v1.Y, v1.Z),
			new Vector3D(v1.X + (double)sz.Width, v1.Y, v1.Z),
			new Vector3D(v1.X + (double)sz.Width, v1.Y + (double)sz.Height, v1.Z),
			new Vector3D(v1.X, v1.Y + (double)sz.Height, v1.Z)
		}, br, p, IsPNF);
		AddPolygon(polygon);
		return new Polygon[1] { polygon };
	}

	[Obsolete("Use CreateSphere")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Polygon[] CreateShpeare(Vector3D v1, Vector3D r, int dsc, Pen p, BrushInfo br)
	{
		ArrayList arrayList = new ArrayList();
		double num = Math.PI * 2.0 / (double)dsc;
		for (int i = 0; i < dsc - 1; i++)
		{
			double z = r.Z * Math.Cos((double)i * num);
			double num2 = r.X * Math.Sin((double)i * num);
			double num3 = r.Y * Math.Sin((double)i * num);
			double z2 = r.Z * Math.Cos((double)(i + 1) * num);
			double num4 = r.X * Math.Sin((double)(i + 1) * num);
			double num5 = r.Y * Math.Sin((double)(i + 1) * num);
			for (int j = 0; j < dsc - 1; j++)
			{
				double x = num2 * Math.Cos((double)j * num);
				double y = num3 * Math.Sin((double)j * num);
				double x2 = num4 * Math.Cos((double)j * num);
				double y2 = num5 * Math.Sin((double)j * num);
				double x3 = num2 * Math.Cos((double)(j + 1) * num);
				double y3 = num3 * Math.Sin((double)(j + 1) * num);
				double x4 = num4 * Math.Cos((double)(j + 1) * num);
				double y4 = num5 * Math.Sin((double)(j + 1) * num);
				Polygon polygon = new Polygon(new Vector3D[4]
				{
					new Vector3D(x, y, z) + v1,
					new Vector3D(x2, y2, z2) + v1,
					new Vector3D(x4, y4, z2) + v1,
					new Vector3D(x3, y3, z) + v1
				}, br);
				arrayList.Add(polygon);
				AddPolygon(polygon);
			}
		}
		return (Polygon[])arrayList.ToArray(typeof(Polygon));
	}

	public Polygon[] CreateSphere(Vector3D v1, Vector3D r, int dsc, Pen p, BrushInfo br)
	{
		ArrayList arrayList = new ArrayList();
		double num = Math.PI * 2.0 / (double)dsc;
		for (int i = 0; i < dsc - 1; i++)
		{
			double z = r.Z * Math.Cos((double)i * num);
			double num2 = r.X * Math.Sin((double)i * num);
			double num3 = r.Y * Math.Sin((double)i * num);
			double z2 = r.Z * Math.Cos((double)(i + 1) * num);
			double num4 = r.X * Math.Sin((double)(i + 1) * num);
			double num5 = r.Y * Math.Sin((double)(i + 1) * num);
			for (int j = 0; j < dsc - 1; j++)
			{
				double x = num2 * Math.Cos((double)j * num);
				double y = num3 * Math.Sin((double)j * num);
				double x2 = num4 * Math.Cos((double)j * num);
				double y2 = num5 * Math.Sin((double)j * num);
				double x3 = num2 * Math.Cos((double)(j + 1) * num);
				double y3 = num3 * Math.Sin((double)(j + 1) * num);
				double x4 = num4 * Math.Cos((double)(j + 1) * num);
				double y4 = num5 * Math.Sin((double)(j + 1) * num);
				Polygon polygon = new Polygon(new Vector3D[4]
				{
					new Vector3D(x, y, z) + v1,
					new Vector3D(x2, y2, z2) + v1,
					new Vector3D(x4, y4, z2) + v1,
					new Vector3D(x3, y3, z) + v1
				}, br);
				arrayList.Add(polygon);
				AddPolygon(polygon);
			}
		}
		return (Polygon[])arrayList.ToArray(typeof(Polygon));
	}

	public Polygon[] CreateCylinderV(Vector3D v1, Vector3D v2, int dsc, Pen p, BrushInfo br)
	{
		ArrayList arrayList = new ArrayList();
		Vector3D[] array = new Vector3D[dsc];
		Vector3D[] array2 = new Vector3D[dsc];
		double num = Math.PI * 2.0 / (double)dsc;
		Vector3D vector3D = v2 - v1;
		float num2 = (float)(v1.X + vector3D.X / 2.0);
		float num3 = (float)(v1.Z + vector3D.Z / 2.0);
		for (int i = 0; i < dsc; i++)
		{
			float num4 = (float)((double)num2 + vector3D.X / 2.0 * Math.Cos((double)i * num));
			float num5 = (float)((double)num3 + vector3D.Z / 2.0 * Math.Sin((double)i * num));
			float num6 = (float)((double)num2 + vector3D.X / 2.0 * Math.Cos((double)(i + 1) * num));
			float num7 = (float)((double)num3 + vector3D.Z / 2.0 * Math.Sin((double)(i + 1) * num));
			Polygon polygon = new Polygon(new Vector3D[4]
			{
				new Vector3D(num4, v1.Y, num5),
				new Vector3D(num4, v2.Y, num5),
				new Vector3D(num6, v2.Y, num7),
				new Vector3D(num6, v1.Y, num7)
			}, br);
			array[i] = new Vector3D(num4, v1.Y, num5);
			array2[i] = new Vector3D(num4, v2.Y, num5);
			arrayList.Add(polygon);
			AddPolygon(polygon);
		}
		Polygon polygon2 = new Polygon(array, br, p);
		Polygon polygon3 = new Polygon(array2, br, p);
		AddPolygon(polygon2);
		AddPolygon(polygon3);
		arrayList.Add(polygon2);
		arrayList.Add(polygon3);
		return (Polygon[])arrayList.ToArray(typeof(Polygon));
	}

	public Polygon[] CreateCylinderH(Vector3D v1, Vector3D v2, int dsc, Pen p, BrushInfo br)
	{
		ArrayList arrayList = new ArrayList();
		Vector3D[] array = new Vector3D[dsc];
		Vector3D[] array2 = new Vector3D[dsc];
		double num = Math.PI * 2.0 / (double)dsc;
		Vector3D vector3D = v2 - v1;
		float num2 = (float)(v1.Y + vector3D.Y / 2.0);
		float num3 = (float)(v1.Z + vector3D.Z / 2.0);
		for (int i = 0; i < dsc; i++)
		{
			float num4 = (float)((double)num2 + vector3D.Y / 2.0 * Math.Cos((double)i * num));
			float num5 = (float)((double)num3 + vector3D.Z / 2.0 * Math.Sin((double)i * num));
			float num6 = (float)((double)num2 + vector3D.Y / 2.0 * Math.Cos((double)(i + 1) * num));
			float num7 = (float)((double)num3 + vector3D.Z / 2.0 * Math.Sin((double)(i + 1) * num));
			Polygon polygon = new Polygon(new Vector3D[4]
			{
				new Vector3D(v1.X, num4, num5),
				new Vector3D(v2.X, num4, num5),
				new Vector3D(v2.X, num6, num7),
				new Vector3D(v1.X, num6, num7)
			}, br);
			array[i] = new Vector3D(v1.X, num4, num5);
			array2[i] = new Vector3D(v2.X, num4, num5);
			arrayList.Add(polygon);
			AddPolygon(polygon);
		}
		Polygon polygon2 = new Polygon(array, br, p);
		Polygon polygon3 = new Polygon(array2, br, p);
		AddPolygon(polygon2);
		AddPolygon(polygon3);
		arrayList.Add(polygon2);
		arrayList.Add(polygon3);
		return (Polygon[])arrayList.ToArray(typeof(Polygon));
	}

	private void DrawBspNode3D(BspNode tr, Vector3D eye)
	{
		if ((tr.Plane.GetNormal(m_transform.Result) & eye) > tr.Plane.D)
		{
			if (tr.Front != null)
			{
				DrawBspNode3D(tr.Front, eye);
			}
			tr.Plane.Draw(this);
			if (tr.Back != null)
			{
				DrawBspNode3D(tr.Back, eye);
			}
		}
		else
		{
			if (tr.Back != null)
			{
				DrawBspNode3D(tr.Back, eye);
			}
			tr.Plane.Draw(this);
			if (tr.Front != null)
			{
				DrawBspNode3D(tr.Front, eye);
			}
		}
	}
}
