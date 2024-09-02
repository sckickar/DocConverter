using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.DocIO.DLS;
using DocGen.Drawing;
using DocGen.Drawing.DocIOHelper;

namespace DocGen.DocIO.Rendering;

internal class ShapePath
{
	private RectangleF _rectBounds;

	private Dictionary<string, string> _shapeGuide;

	internal ShapePath(RectangleF bounds, Dictionary<string, string> shapeGuide)
	{
		_rectBounds = bounds;
		_shapeGuide = shapeGuide;
	}

	internal IGraphicsPath GetCurvedConnectorPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedConnector);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddBeziers(new PointF[7]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Bottom),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedConnector2Path()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddBeziers(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedConnector4Path()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedConnector4);
		graphicsPath.AddBeziers(new PointF[10]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedConnector5Path()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedConnector5);
		graphicsPath.AddBeziers(new PointF[13]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Bottom),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetBentConnectorPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.ElbowConnector);
		PointF[] array = new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		};
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.CloseFigure();
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[1], array[2]);
		graphicsPath.CloseFigure();
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[2], array[3]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetBendConnector2Path()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[3]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetBentConnector4Path()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.BentConnector4);
		graphicsPath.AddLines(new PointF[5]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetBentConnector5Path()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.BentConnector5);
		graphicsPath.AddLines(new PointF[6]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Bottom),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetRoundedRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RoundedRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		float num = dictionary["x1"] * 2f;
		if (num > 0f)
		{
			graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, num, num, 180f, 90f);
			graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Y, num, num, 270f, 90f);
			graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Bottom - num, num, num, 0f, 90f);
			graphicsPath.AddArc(_rectBounds.X, _rectBounds.Bottom - num, num, num, 90f, 90f);
			graphicsPath.CloseFigure();
		}
		return graphicsPath;
	}

	internal IGraphicsPath GetSnipSingleCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.SnipSingleCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[5]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["dx1"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetSnipSameSideCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.SnipSameSideCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(_rectBounds.X + dictionary["tx1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["tx2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["tx1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["by1"]),
			new PointF(_rectBounds.X + dictionary["bx2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["bx1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["by1"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["tx1"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetSnipDiagonalCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.SnipDiagonalCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(_rectBounds.X + dictionary["lx1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["rx2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["rx1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["ly1"]),
			new PointF(_rectBounds.X + dictionary["lx2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["rx1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["ry1"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["lx1"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetSnipAndRoundSingleCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.SnipAndRoundSingleCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[6];
		float num = dictionary["x1"] * 2f;
		array[0] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y);
		array[1] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y);
		array[2] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["dx2"]);
		array[3] = new PointF(_rectBounds.Right, _rectBounds.Bottom);
		array[4] = new PointF(_rectBounds.X, _rectBounds.Bottom);
		array[5] = new PointF(_rectBounds.X, _rectBounds.Y + dictionary["x1"]);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, num, num, 180f, 90f);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRoundSingleCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RoundSingleCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[2];
		float num = dictionary["dx1"] * 2f;
		array[0] = new PointF(_rectBounds.X, _rectBounds.Y);
		array[1] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y);
		graphicsPath.AddLines(array);
		graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Y, num, num, 270f, 90f);
		array[0] = new PointF(_rectBounds.Right, _rectBounds.Bottom);
		array[1] = new PointF(_rectBounds.X, _rectBounds.Bottom);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRoundSameSideCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RoundSameSideCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[2];
		float num = dictionary["tx1"] * 2f;
		float num2 = dictionary["bx1"] * 2f;
		array[0] = new PointF(_rectBounds.X + dictionary["tx1"], _rectBounds.Y);
		array[1] = new PointF(_rectBounds.X + dictionary["tx2"], _rectBounds.Y);
		graphicsPath.AddLines(array);
		graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Y, num, num, 270f, 90f);
		if (num2 == 0f)
		{
			array[0] = new PointF(_rectBounds.Right, _rectBounds.Bottom);
			array[1] = new PointF(_rectBounds.X, _rectBounds.Bottom);
			graphicsPath.AddLines(array);
		}
		else
		{
			graphicsPath.AddArc(_rectBounds.Right - num2, _rectBounds.Bottom - num2, num2, num2, 0f, 90f);
			graphicsPath.AddArc(_rectBounds.X, _rectBounds.Bottom - num2, num2, num2, 90f, 90f);
		}
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, num, num, 180f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRoundDiagonalCornerRectanglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RoundDiagonalCornerRectangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[2];
		float num = dictionary["x1"] * 2f;
		float num2 = dictionary["a"] * 2f;
		array[0] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y);
		array[1] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y);
		graphicsPath.AddLines(array);
		if (num2 != 0f)
		{
			graphicsPath.AddArc(_rectBounds.Right - num2, _rectBounds.Y, num2, num2, 270f, 90f);
		}
		graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Bottom - num, num, num, 0f, 90f);
		if (num2 == 0f)
		{
			array[0] = new PointF(_rectBounds.Right - dictionary["x1"], _rectBounds.Bottom);
			array[1] = new PointF(_rectBounds.X, _rectBounds.Bottom);
			graphicsPath.AddLines(array);
		}
		else
		{
			graphicsPath.AddArc(_rectBounds.X, _rectBounds.Bottom - num2, num2, num2, 90f, 90f);
		}
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, num, num, 180f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetTrianglePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.IsoscelesTriangle);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[3]
		{
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetParallelogramPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Parallelogram);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetTrapezoidPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Trapezoid);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRegularPentagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RegularPentagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[5]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetHexagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Hexagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[6]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetHeptagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Heptagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[7]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetOctagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Octagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDecagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Decagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[10]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDodecagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Dodecagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[12]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y3"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetPiePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Pie);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddPie(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, dictionary["stAng"], dictionary["swAng"]);
		return graphicsPath;
	}

	internal IGraphicsPath GetChordPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Chord);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, dictionary["stAng"], dictionary["swAng"]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetTearDropPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Teardrop);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, 180f, 90f);
		graphicsPath.AddBeziers(new PointF[7]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f)
		});
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, 0f, 90f);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFramePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Frame);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		};
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["x1"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["x1"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetHalfFramePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.HalfFrame);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[6]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetL_ShapePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.L_Shape);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[6]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDiagonalStripePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DiagonalStripe);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Left, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCrossPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Cross);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[12]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetPlaquePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Plaque);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		float num = dictionary["x1"] * 2f;
		graphicsPath.AddArc(_rectBounds.X - dictionary["x1"], _rectBounds.Y - dictionary["x1"], num, num, 90f, -90f);
		graphicsPath.AddArc(_rectBounds.Right - dictionary["x1"], _rectBounds.Y - dictionary["x1"], num, num, 180f, -90f);
		graphicsPath.AddArc(_rectBounds.Right - dictionary["x1"], _rectBounds.Bottom - dictionary["x1"], num, num, 270f, -90f);
		graphicsPath.AddArc(_rectBounds.X - dictionary["x1"], _rectBounds.Bottom - dictionary["x1"], num, num, 0f, -90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCanPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Can);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, dictionary["y1"] * 2f, 0f, 180f);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, dictionary["y1"] * 2f, 180f, 180f);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y + dictionary["y3"] - dictionary["y1"], _rectBounds.Width, dictionary["y1"] * 2f, 0f, 180f);
		graphicsPath.AddLine(_rectBounds.X, _rectBounds.Y + dictionary["y3"] - dictionary["y1"], _rectBounds.X, _rectBounds.Y + dictionary["y1"]);
		return graphicsPath;
	}

	internal IGraphicsPath GetCubePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Cube);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[6]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["y1"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLines(new PointF[5]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Bottom)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetBevelPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Bevel);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.Left, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(_rectBounds.X, _rectBounds.Y, _rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["x1"]);
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(_rectBounds.X, _rectBounds.Bottom, _rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(_rectBounds.Right, _rectBounds.Y, _rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["x1"]);
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(_rectBounds.Right, _rectBounds.Bottom, _rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDonutPath(double lineWidth)
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Donut);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		RectangleF rect = new RectangleF(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height);
		if (rect.Width > 0f && rect.Height > 0f)
		{
			graphicsPath.AddArc(rect, 180f, 90f);
			graphicsPath.AddArc(rect, 270f, 90f);
			graphicsPath.AddArc(rect, 0f, 90f);
			graphicsPath.AddArc(rect, 90f, 90f);
		}
		graphicsPath.CloseFigure();
		rect = new RectangleF(_rectBounds.X + dictionary["dr"], _rectBounds.Y + dictionary["dr"], dictionary["iwd2"] * 2f, dictionary["ihd2"] * 2f);
		if (rect.Width > 0f && rect.Height > 0f)
		{
			graphicsPath.AddArc(rect, 180f, -90f);
			graphicsPath.AddArc(rect, 90f, -90f);
			graphicsPath.AddArc(rect, 0f, -90f);
			graphicsPath.AddArc(rect, 270f, -90f);
		}
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetNoSymbolPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.NoSymbol);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		RectangleF rect = new RectangleF(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height);
		graphicsPath.AddArc(rect, 180f, 90f);
		graphicsPath.AddArc(rect, 270f, 90f);
		graphicsPath.AddArc(rect, 0f, 90f);
		graphicsPath.AddArc(rect, 90f, 90f);
		if (dictionary["iwd2"] == 0f)
		{
			dictionary["iwd2"] = 1f;
		}
		if (dictionary["ihd2"] == 0f)
		{
			dictionary["ihd2"] = 1f;
		}
		graphicsPath.CloseFigure();
		graphicsPath.AddArc(_rectBounds.X + dictionary["dr"], _rectBounds.Y + dictionary["dr"], dictionary["iwd2"] * 2f, dictionary["ihd2"] * 2f, dictionary["stAng1"], dictionary["swAng"]);
		graphicsPath.CloseFigure();
		graphicsPath.AddArc(_rectBounds.X + dictionary["dr"], _rectBounds.Y + dictionary["dr"], dictionary["iwd2"] * 2f, dictionary["ihd2"] * 2f, dictionary["stAng2"], dictionary["swAng"]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetBlockArcPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.BlockArc);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		RectangleF rect = new RectangleF(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height);
		if (dictionary["iwd2"] == 0f)
		{
			dictionary["iwd2"] = 1f;
		}
		if (dictionary["ihd2"] == 0f)
		{
			dictionary["ihd2"] = 1f;
		}
		graphicsPath.AddArc(rect, dictionary["stAng"] / 60000f, dictionary["swAng"] / 60000f);
		graphicsPath.AddArc(_rectBounds.Right - dictionary["x2"], _rectBounds.Bottom - dictionary["y2"], dictionary["iwd2"] * 2f, dictionary["ihd2"] * 2f, dictionary["istAng"] / 60000f, dictionary["iswAng"] / 60000f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFoldedCornerPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.FoldedCorner);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Top + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Top + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.Left, _rectBounds.Bottom),
			new PointF(_rectBounds.Left, _rectBounds.Top),
			new PointF(_rectBounds.Right, _rectBounds.Top),
			new PointF(_rectBounds.Right, _rectBounds.Top + dictionary["y2"])
		});
		return graphicsPath;
	}

	internal IGraphicsPath[] GetSmileyFacePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.SmileyFace);
		IGraphicsPath[] array = new IGraphicsPath[3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetGraphicsPath();
		}
		PointF[] array2 = new PointF[4];
		array2[0] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]);
		array2[1] = array2[0];
		array2[2] = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["y5"]);
		array2[3] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]);
		array[1].AddBeziers(array2);
		array[0].AddEllipse(_rectBounds);
		array[2].AddEllipse(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"] - dictionary["hR"], dictionary["wR"] * 2f, dictionary["hR"] * 2f);
		array[2].AddEllipse(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"] - dictionary["hR"], dictionary["wR"] * 2f, dictionary["hR"] * 2f);
		return array;
	}

	internal IGraphicsPath GetHeartPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Heart);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddBeziers(new PointF[7]
		{
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + _rectBounds.Height / 4f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLightningBoltPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[11]
		{
			GetXYPosition(8472f, 0f, 21600f),
			GetXYPosition(12860f, 6080f, 21600f),
			GetXYPosition(11050f, 6797f, 21600f),
			GetXYPosition(16577f, 12007f, 21600f),
			GetXYPosition(14767f, 12877f, 21600f),
			GetXYPosition(21600f, 21600f, 21600f),
			GetXYPosition(10012f, 14915f, 21600f),
			GetXYPosition(12222f, 13987f, 21600f),
			GetXYPosition(5022f, 9705f, 21600f),
			GetXYPosition(7602f, 8382f, 21600f),
			GetXYPosition(0f, 3890f, 21600f)
		});
		graphicsPath.CloseAllFigures();
		return graphicsPath;
	}

	internal IGraphicsPath GetSunPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Sun);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[3]
		{
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x15"], _rectBounds.Y + dictionary["y18"]),
			new PointF(_rectBounds.X + dictionary["x15"], _rectBounds.Y + dictionary["y14"])
		};
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + dictionary["ox1"], _rectBounds.Y + dictionary["oy1"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x16"], _rectBounds.Y + dictionary["y13"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x17"], _rectBounds.Y + dictionary["y12"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y);
		array[1] = new PointF(_rectBounds.X + dictionary["x18"], _rectBounds.Y + dictionary["y10"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x14"], _rectBounds.Y + dictionary["y10"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + dictionary["ox2"], _rectBounds.Y + dictionary["oy1"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x13"], _rectBounds.Y + dictionary["y12"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x12"], _rectBounds.Y + dictionary["y13"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f);
		array[1] = new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y14"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y18"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + dictionary["ox2"], _rectBounds.Y + dictionary["oy2"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x12"], _rectBounds.Y + dictionary["y17"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x13"], _rectBounds.Y + dictionary["y16"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom);
		array[1] = new PointF(_rectBounds.X + dictionary["x14"], _rectBounds.Y + dictionary["y15"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x18"], _rectBounds.Y + dictionary["y15"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + dictionary["ox1"], _rectBounds.Y + dictionary["oy2"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x17"], _rectBounds.Y + dictionary["y16"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x16"], _rectBounds.Y + dictionary["y17"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		graphicsPath.AddEllipse(_rectBounds.X + dictionary["x19"], _rectBounds.Y + _rectBounds.Height / 2f - dictionary["hR"], dictionary["wR"] * 2f, dictionary["hR"] * 2f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMoonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Moon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width * 2f, _rectBounds.Height, 90f, 180f);
		float num = dictionary["stAng1"];
		if (num < 180f)
		{
			num += 180f;
		}
		graphicsPath.AddArc(_rectBounds.X + dictionary["g0w"], _rectBounds.Y + _rectBounds.Height / 2f - dictionary["dy1"], dictionary["g18w"] * 2f, dictionary["dy1"] * 2f, num, dictionary["swAng1"] % 360f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCloudPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Cloud);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF xYPosition = GetXYPosition(3900f, 14370f, 43200f);
		xYPosition.X += dictionary["g27"];
		xYPosition.Y -= dictionary["g30"];
		SizeF sizeF = new SizeF(_rectBounds.Width * 6753f / 43200f * 2f, _rectBounds.Height * 9190f / 43200f * 2f);
		SizeF sizeF2 = new SizeF(_rectBounds.Width * 5333f / 43200f * 2f, _rectBounds.Height * 7267f / 43200f * 2f);
		SizeF sizeF3 = new SizeF(_rectBounds.Width * 4365f / 43200f * 2f, _rectBounds.Height * 5945f / 43200f * 2f);
		SizeF sizeF4 = new SizeF(_rectBounds.Width * 4857f / 43200f * 2f, _rectBounds.Height * 6595f / 43200f * 2f);
		SizeF sizeF5 = new SizeF(_rectBounds.Width * 5333f / 43200f * 2f, _rectBounds.Height * 7273f / 43200f * 2f);
		SizeF sizeF6 = new SizeF(_rectBounds.Width * 6775f / 43200f * 2f, _rectBounds.Height * 9220f / 43200f * 2f);
		SizeF sizeF7 = new SizeF(_rectBounds.Width * 5785f / 43200f * 2f, _rectBounds.Height * 7867f / 43200f * 2f);
		SizeF sizeF8 = new SizeF(_rectBounds.Width * 6752f / 43200f * 2f, _rectBounds.Height * 9215f / 43200f * 2f);
		SizeF sizeF9 = new SizeF(_rectBounds.Width * 7720f / 43200f * 2f, _rectBounds.Height * 10543f / 43200f * 2f);
		SizeF sizeF10 = new SizeF(_rectBounds.Width * 4360f / 43200f * 2f, _rectBounds.Height * 5918f / 43200f * 2f);
		SizeF sizeF11 = new SizeF(_rectBounds.Width * 4345f / 43200f * 2f, _rectBounds.Height * 5945f / 43200f * 2f);
		graphicsPath.AddArc(_rectBounds.X + 4076f * _rectBounds.Width / 43200f, _rectBounds.Y + 3912f * _rectBounds.Height / 43200f, sizeF.Width, sizeF.Height, -190f, 123f);
		graphicsPath.AddArc(_rectBounds.X + 13469f * _rectBounds.Width / 43200f, _rectBounds.Y + 1304f * _rectBounds.Height / 43200f, sizeF2.Width, sizeF2.Height, -144f, 89f);
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f + 531f * _rectBounds.Width / 43200f, _rectBounds.Y + 1f, sizeF3.Width, sizeF3.Height, -145f, 99f);
		graphicsPath.AddArc(xYPosition.X + _rectBounds.Width / 2f + 3013f * _rectBounds.Width / 43200f, _rectBounds.Y + 1f, sizeF4.Width, sizeF4.Height, -130f, 117f);
		graphicsPath.AddArc(_rectBounds.Right - sizeF5.Width - 708f * _rectBounds.Width / 43200f, _rectBounds.Y + sizeF4.Height / 2f - 1127f * _rectBounds.Height / 43200f, sizeF5.Width, sizeF5.Height, -78f, 109f);
		graphicsPath.AddArc(_rectBounds.Right - sizeF6.Width + 354f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f - 9129f * _rectBounds.Height / 43200f, sizeF6.Width, sizeF6.Height, -46f, 130f);
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f + 4608f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f + 869f * _rectBounds.Height / 43200f, sizeF7.Width, sizeF7.Height, 0f, 114f);
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f - sizeF8.Width / 2f + 886f * _rectBounds.Width / 43200f, _rectBounds.Bottom - sizeF8.Height, sizeF8.Width, sizeF8.Height, 22f, 115f);
		graphicsPath.AddArc(_rectBounds.X + 4962f * _rectBounds.Width / 43200f, _rectBounds.Bottom - sizeF9.Height - 2173f * _rectBounds.Height / 43200f, sizeF9.Width, sizeF9.Height, 66f, 75f);
		graphicsPath.AddArc(_rectBounds.X + 1063f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f + 2608f * _rectBounds.Height / 43200f, sizeF10.Width, sizeF10.Height, -274f, 146f);
		graphicsPath.AddArc(_rectBounds.X + 1f, _rectBounds.Y + _rectBounds.Height / 2f - sizeF11.Height / 2f - 1304f * _rectBounds.Height / 43200f, sizeF11.Width, sizeF11.Height, -246f, 152f);
		graphicsPath.CloseFigure();
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f + 2658f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f, _rectBounds.Width * 6753f / 43200f * 2f, _rectBounds.Height * 9190f / 43200f * 2f, -58f, 59f);
		return graphicsPath;
	}

	internal IGraphicsPath[] GetArcPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Arc);
		IGraphicsPath[] array = new IGraphicsPath[2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetGraphicsPath();
		}
		array[0].AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, dictionary["stAng"] / 60000f, dictionary["swAng"] / 60000f);
		array[1].AddPie(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, dictionary["stAng"] / 60000f, dictionary["swAng"] / 60000f);
		return array;
	}

	internal IGraphicsPath GetDoubleBracketPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DoubleBracket);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Bottom - dictionary["x1"] * 2f, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 90f, 90f);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 180f, 90f);
		graphicsPath.StartFigure();
		graphicsPath.AddArc(_rectBounds.X + dictionary["x2"] - dictionary["x1"], _rectBounds.Y, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 270f, 90f);
		graphicsPath.AddArc(_rectBounds.X + dictionary["x2"] - dictionary["x1"], _rectBounds.Y + dictionary["y2"] - dictionary["x1"], dictionary["x1"] * 2f, dictionary["x1"] * 2f, 0f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetDoubleBracePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DoubleBrace);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X - dictionary["x1"] + dictionary["x2"], _rectBounds.Bottom - dictionary["x1"] * 2f, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 90f, 90f);
		graphicsPath.AddArc(_rectBounds.X - dictionary["x1"], _rectBounds.Y + dictionary["y3"] - dictionary["x1"], dictionary["x1"] * 2f, dictionary["x1"] * 2f, 0f, -90f);
		graphicsPath.AddArc(_rectBounds.X - dictionary["x1"], _rectBounds.Y + dictionary["y3"] - dictionary["x1"] * 3f, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 90f, -90f);
		graphicsPath.AddArc(_rectBounds.X - dictionary["x1"] + dictionary["x2"], _rectBounds.Y, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 180f, 90f);
		graphicsPath.StartFigure();
		graphicsPath.AddArc(_rectBounds.X + dictionary["x3"] - dictionary["x1"], _rectBounds.Top, dictionary["x1"] * 2f, dictionary["x1"] * 2f, 270f, 90f);
		graphicsPath.AddArc(_rectBounds.Right - dictionary["x1"], _rectBounds.Y + dictionary["y2"] - dictionary["x1"], dictionary["x1"] * 2f, dictionary["x1"] * 2f, 180f, -90f);
		graphicsPath.AddArc(_rectBounds.Right - dictionary["x1"], _rectBounds.Y + dictionary["y2"] + dictionary["x1"], dictionary["x1"] * 2f, dictionary["x1"] * 2f, 270f, -90f);
		graphicsPath.AddArc(_rectBounds.X + dictionary["x3"] - dictionary["x1"], _rectBounds.Y + dictionary["y4"], dictionary["x1"] * 2f, dictionary["x1"], 0f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftBracketPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftBracket);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.Right - _rectBounds.Width, _rectBounds.Bottom - dictionary["y1"] * 2f, _rectBounds.Width * 2f, dictionary["y1"] * 2f, 90f, 90f);
		graphicsPath.AddArc(_rectBounds.Right - _rectBounds.Width, _rectBounds.Y, _rectBounds.Width * 2f, dictionary["y1"] * 2f, 180f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetRightBracketPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RightBracket);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width, _rectBounds.Y, _rectBounds.Width * 2f, dictionary["y1"] * 2f, 270f, 90f);
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width, _rectBounds.Y + dictionary["y2"] - dictionary["y1"], _rectBounds.Width * 2f, dictionary["y1"] * 2f, 0f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftBracePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftBrace);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.Right - _rectBounds.Width / 2f, _rectBounds.Bottom - dictionary["y1"] * 2f, _rectBounds.Width, dictionary["y1"] * 2f, 90f, 90f);
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width / 2f, _rectBounds.Y + dictionary["y4"] - dictionary["y1"], _rectBounds.Width, dictionary["y1"] * 2f, 0f, -90f);
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width / 2f, _rectBounds.Y + dictionary["y4"] - dictionary["y1"] * 3f, _rectBounds.Width, dictionary["y1"] * 2f, 90f, -90f);
		graphicsPath.AddArc(_rectBounds.Right - _rectBounds.Width / 2f, _rectBounds.Y, _rectBounds.Width, dictionary["y1"] * 2f, 180f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetRightBracePath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RightBrace);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width / 2f, _rectBounds.Top, _rectBounds.Width, dictionary["y1"] * 2f, 270f, 90f);
		PointF pointF = new PointF(_rectBounds.X + _rectBounds.Width, _rectBounds.Y + dictionary["y2"] - dictionary["y1"]);
		graphicsPath.AddArc(pointF.X - _rectBounds.Width / 2f, pointF.Y, _rectBounds.Width, dictionary["y1"] * 2f, 180f, -90f);
		graphicsPath.AddArc(pointF.X - _rectBounds.Width / 2f, pointF.Y + dictionary["y1"] * 2f, _rectBounds.Width, dictionary["y1"] * 2f, 270f, -90f);
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width / 2f, _rectBounds.Y + dictionary["y4"] - dictionary["y1"], _rectBounds.Width, dictionary["y1"] * 2f, 0f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetRightArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RightArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[7]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[7]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetUpArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.UpArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[7]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDownArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DownArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[7]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftRightArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftRightArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[10]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedRightArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedRightArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[7]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["hR"]),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddArc(array[0].X, array[0].Y - dictionary["hR"], _rectBounds.Width * 2f, dictionary["hR"] * 2f, 180f, dictionary["mswAng"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]);
		array[2] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y6"]);
		graphicsPath.AddLine(array[1], array[2]);
		array[3] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y8"]);
		graphicsPath.AddLine(array[2], array[3]);
		array[4] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y7"]);
		graphicsPath.AddArc(array[4].X - dictionary["x1"], array[4].Y - dictionary["hR"] * 2f, _rectBounds.Width * 2f, dictionary["hR"] * 2f, dictionary["stAng"], dictionary["swAng"]);
		graphicsPath.CloseFigure();
		array[5] = new PointF(_rectBounds.X, _rectBounds.Y + dictionary["hR"]);
		graphicsPath.AddArc(array[5].X, array[5].Y - dictionary["hR"], _rectBounds.Width * 2f, dictionary["hR"] * 2f, 180f, 90f);
		array[6] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["th"]);
		graphicsPath.AddLine(array[5].X + _rectBounds.Width, array[5].Y - dictionary["hR"], array[6].X, array[6].Y);
		graphicsPath.AddArc(array[6].X - _rectBounds.Width, array[6].Y, _rectBounds.Width * 2f, dictionary["hR"] * 2f, 270f, dictionary["swAng2"]);
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedLeftArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedLeftArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[7]
		{
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y3"]),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddArc(_rectBounds.Right - _rectBounds.Width * 2f, array[0].Y - dictionary["hR"], _rectBounds.Width * 2f, dictionary["hR"] * 2f, 0f, -90f);
		array[1] = new PointF(_rectBounds.X, _rectBounds.Y);
		graphicsPath.AddArc(array[1].X - _rectBounds.Width, array[1].Y, _rectBounds.Width * 2f, dictionary["hR"] * 2f, 270f, 90f);
		array[2] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddArc(array[2].X - _rectBounds.Width * 2f, array[2].Y - dictionary["hR"], _rectBounds.Width * 2f, dictionary["hR"] * 2f, 0f, dictionary["swAng"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y8"]);
		array[4] = new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y6"]);
		graphicsPath.AddLine(array[3], array[4]);
		array[5] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[4], array[5]);
		array[6] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y5"]);
		graphicsPath.AddLine(array[5], array[6]);
		graphicsPath.AddArc(_rectBounds.X - _rectBounds.Width, array[1].Y, _rectBounds.Width * 2f, dictionary["hR"] * 2f, dictionary["swAng"], dictionary["swAng2"]);
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedUpArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedUpArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[7];
		SizeF sizeF = new SizeF(dictionary["wR"], _rectBounds.Height);
		array[0] = new PointF(_rectBounds.X + dictionary["ix"], _rectBounds.Y + dictionary["iy"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y);
		array[3] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y1"]);
		array[4] = new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y1"]);
		array[5] = new PointF(_rectBounds.X + dictionary["wR"], _rectBounds.Bottom);
		array[6] = new PointF(_rectBounds.X + dictionary["th"], _rectBounds.Y);
		graphicsPath.AddArc(array[5].X - sizeF.Width, array[0].Y - sizeF.Height - dictionary["iy"], sizeF.Width * 2f, sizeF.Height * 2f, dictionary["stAng2"], dictionary["swAng2"]);
		graphicsPath.AddLine(array[1], array[2]);
		graphicsPath.AddLine(array[2], array[3]);
		graphicsPath.AddLine(array[3], array[4]);
		graphicsPath.AddArc(array[6].X, _rectBounds.Y - sizeF.Height, sizeF.Width * 2f, sizeF.Height * 2f, dictionary["stAng3"], dictionary["swAng"]);
		graphicsPath.AddArc(array[5].X - sizeF.Width, array[5].Y - sizeF.Height * 2f, sizeF.Width * 2f, sizeF.Height * 2f, 90f, 90f);
		graphicsPath.AddArc(array[6].X, array[6].Y - sizeF.Height, sizeF.Width * 2f, sizeF.Height * 2f, 180f, -90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedDownArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedDownArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[7];
		SizeF sizeF = new SizeF(dictionary["wR"], _rectBounds.Height);
		array[0] = new PointF(_rectBounds.X + dictionary["ix"], _rectBounds.Y + dictionary["iy"]);
		array[1] = new PointF(_rectBounds.X, _rectBounds.Bottom);
		array[2] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y);
		array[3] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y1"]);
		array[4] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Bottom);
		array[5] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]);
		array[6] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddArc(array[2].X - sizeF.Width, _rectBounds.Y, sizeF.Width * 2f, sizeF.Height * 2f, dictionary["stAng2"], dictionary["swAng2"]);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, sizeF.Width * 2f, sizeF.Height * 2f, 180f, 90f);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, sizeF.Width * 2f, sizeF.Height * 2f, 270f, dictionary["swAng"]);
		graphicsPath.AddLine(array[6], array[5]);
		graphicsPath.AddLine(array[5], array[4]);
		graphicsPath.AddLine(array[4], array[3]);
		graphicsPath.AddLine(array[3], new PointF(array[3].X - dictionary["x5"] + dictionary["x4"], array[3].Y));
		graphicsPath.AddArc(array[2].X - sizeF.Width, _rectBounds.Y, sizeF.Width * 2f, sizeF.Height * 2f, dictionary["stAng"], dictionary["mswAng"]);
		graphicsPath.AddLines(new PointF[1]
		{
			new PointF(_rectBounds.X + sizeF.Width, _rectBounds.Y)
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetUpDownArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.UpDownArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[10]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetQuadArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.QuadArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[24]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y5"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftRightUpArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftRightUpArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[17]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetBentArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.BentArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[6];
		graphicsPath.AddLine(_rectBounds.X, _rectBounds.Bottom, _rectBounds.X, _rectBounds.Y + dictionary["y5"]);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y + dictionary["y5"] - dictionary["bd"], dictionary["bd"] * 2f, dictionary["bd"] * 2f, 180f, 90f);
		array[0] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["dh2"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y);
		array[2] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["aw2"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]);
		array[4] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]);
		array[5] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLines(array);
		if (dictionary["bd2"] == 0f)
		{
			dictionary["bd2"] = 1f;
		}
		graphicsPath.AddArc(array[5].X - dictionary["bd2"], array[5].Y, dictionary["bd2"] * 2f, dictionary["bd2"] * 2f, 270f, -90f);
		graphicsPath.AddLine(array[5].X - dictionary["bd2"], array[5].Y + dictionary["bd2"], _rectBounds.X + dictionary["th"], _rectBounds.Bottom);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetUTrunArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.UTurnArrow);
		if (dictionary["bd2"] == 0f)
		{
			dictionary["bd2"] = 1f;
		}
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[11]
		{
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["bd"]),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.AddArc(array[1].X, array[1].Y - dictionary["bd"], dictionary["bd"] * 2f, dictionary["bd"] * 2f, 180f, 90f);
		array[2] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y);
		graphicsPath.AddLine(array[1].X + dictionary["bd"], array[1].Y - dictionary["bd"], array[2].X, array[2].Y);
		graphicsPath.AddArc(array[2].X - dictionary["bd"], array[2].Y, dictionary["bd"] * 2f, dictionary["bd"] * 2f, 270f, 90f);
		array[3] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[2].X + dictionary["bd"], array[2].Y + dictionary["bd"], array[3].X, array[3].Y);
		array[4] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[3], array[4]);
		array[5] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y5"]);
		graphicsPath.AddLine(array[4], array[5]);
		array[6] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[5], array[6]);
		array[7] = new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[6], array[7]);
		array[8] = new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["x3"]);
		graphicsPath.AddLine(array[7], array[8]);
		graphicsPath.AddArc(array[8].X - dictionary["bd2"] * 2f, array[8].Y - dictionary["bd2"], dictionary["bd2"] * 2f, dictionary["bd2"] * 2f, 0f, -90f);
		array[9] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["th"]);
		graphicsPath.AddLine(array[8].X - dictionary["bd2"], array[8].Y - dictionary["bd2"], array[9].X, array[9].Y);
		graphicsPath.AddArc(array[9].X - dictionary["bd2"], array[9].Y, dictionary["bd2"] * 2f, dictionary["bd2"] * 2f, 270f, -90f);
		array[10] = new PointF(_rectBounds.X + dictionary["th"], _rectBounds.Bottom);
		graphicsPath.AddLine(array[9].X - dictionary["bd2"], array[9].Y + dictionary["bd2"], array[10].X, array[10].Y);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftUpArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftUpArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[12]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["x1"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetBentUpArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.BentUpArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[9]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStripedRightArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.StripedRightArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + Math.Min(_rectBounds.Width, _rectBounds.Height) / 32f, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + Math.Min(_rectBounds.Width, _rectBounds.Height) / 32f, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X + Math.Min(_rectBounds.Width, _rectBounds.Height) / 16f, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + Math.Min(_rectBounds.Width, _rectBounds.Height) / 8f, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + Math.Min(_rectBounds.Width, _rectBounds.Height) / 8f, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + Math.Min(_rectBounds.Width, _rectBounds.Height) / 16f, _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLines(new PointF[7]
		{
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetNotchedRightArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.NotchedRightArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + _rectBounds.Height / 2f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetPentagonPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Pentagon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[5]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetChevronPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Chevron);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[6]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + _rectBounds.Height / 2f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRightArrowCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RightArrowCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[11]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDownArrowCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DownArrowCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[11]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftArrowCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftArrowCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[11]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetUpArrowCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.UpArrowCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[11]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLeftRightArrowCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LeftRightArrowCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[18]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetQuadArrowCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.QuadArrowCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[32]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["ah"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["ah"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["ah"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["ah"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["ah"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["ah"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["ah"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["ah"], _rectBounds.Y + dictionary["y6"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCircularArrowPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CircularArrow);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[5];
		SizeF sizeF = new SizeF(dictionary["rw1"], dictionary["rh1"]);
		array[0] = new PointF(_rectBounds.X + dictionary["xE"], _rectBounds.Y + dictionary["yE"]);
		graphicsPath.AddArc(array[0].X - sizeF.Width * 2f, array[0].Y - sizeF.Height, sizeF.Width * 2f, sizeF.Height * 2f, dictionary["stAng"], dictionary["swAng"]);
		array[1] = new PointF(_rectBounds.X + dictionary["xGp"], _rectBounds.Y + dictionary["yGp"]);
		array[2] = new PointF(_rectBounds.X + dictionary["xA"], _rectBounds.Y + dictionary["yA"]);
		array[3] = new PointF(_rectBounds.X + dictionary["xBp"], _rectBounds.Y + dictionary["yBp"]);
		array[4] = new PointF(_rectBounds.X + dictionary["xC"], _rectBounds.Y + dictionary["yC"]);
		graphicsPath.AddLine(new PointF(array[1].X - (array[4].X - array[3].X), array[1].Y), array[1]);
		graphicsPath.AddLine(array[1], array[2]);
		graphicsPath.AddLine(array[2], array[3]);
		graphicsPath.AddLine(array[3], array[4]);
		SizeF sizeF2 = new SizeF(dictionary["rw2"], dictionary["rh2"]);
		graphicsPath.AddArc(array[0].X - sizeF.Width - sizeF2.Width, array[0].Y - sizeF2.Height, sizeF2.Width * 2f, sizeF2.Height * 2f, dictionary["istAng"], dictionary["iswAng"]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMathPlusPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.MathPlus);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[12]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMathMinusPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.MathMinus);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMathMultiplyPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.MathMultiply);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[12]
		{
			new PointF(_rectBounds.X + dictionary["xA"], _rectBounds.Y + dictionary["yA"]),
			new PointF(_rectBounds.X + dictionary["xB"], _rectBounds.Y + dictionary["yB"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["yC"]),
			new PointF(_rectBounds.X + dictionary["xD"], _rectBounds.Y + dictionary["yB"]),
			new PointF(_rectBounds.X + dictionary["xE"], _rectBounds.Y + dictionary["yA"]),
			new PointF(_rectBounds.X + dictionary["xF"], _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["xE"], _rectBounds.Y + dictionary["yG"]),
			new PointF(_rectBounds.X + dictionary["xD"], _rectBounds.Y + dictionary["yH"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["yI"]),
			new PointF(_rectBounds.X + dictionary["xB"], _rectBounds.Y + dictionary["yH"]),
			new PointF(_rectBounds.X + dictionary["xA"], _rectBounds.Y + dictionary["yG"]),
			new PointF(_rectBounds.X + dictionary["xL"], _rectBounds.Y + _rectBounds.Height / 2f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMathDivisionPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.MathDivision);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[4];
		graphicsPath.AddEllipse(_rectBounds.X + _rectBounds.Width / 2f - dictionary["rad"], _rectBounds.Y + dictionary["y1"], dictionary["rad"] * 2f, dictionary["rad"] * 2f);
		graphicsPath.CloseFigure();
		graphicsPath.AddEllipse(_rectBounds.X + _rectBounds.Width / 2f - dictionary["rad"], _rectBounds.Y + dictionary["y5"] - dictionary["rad"] * 2f, dictionary["rad"] * 2f, dictionary["rad"] * 2f);
		graphicsPath.CloseFigure();
		array[0] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMathEqualPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.MathEqual);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetMathNotEqualPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.MathNotEqual);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[20]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["lx"], _rectBounds.Y + dictionary["ly"]),
			new PointF(_rectBounds.X + dictionary["rx"], _rectBounds.Y + dictionary["ry"]),
			new PointF(_rectBounds.X + dictionary["rx6"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["rx5"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["rx4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["rx3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["drx"], _rectBounds.Y + dictionary["dry"]),
			new PointF(_rectBounds.X + dictionary["dlx"], _rectBounds.Y + dictionary["dly"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartAlternateProcessPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		float num = GetPresetOperandValue("ssd6") * 2f;
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(_rectBounds.Right - num, _rectBounds.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartPredefinedProcessPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddRectangle(_rectBounds);
		graphicsPath.AddLine(_rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Y, _rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Bottom);
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(_rectBounds.Right - _rectBounds.Width / 8f, _rectBounds.Y, _rectBounds.Right - _rectBounds.Width / 8f, _rectBounds.Bottom);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartInternalStoragePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddRectangle(_rectBounds);
		graphicsPath.AddLine(_rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Y, _rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Bottom);
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 8f, _rectBounds.Right, _rectBounds.Top + _rectBounds.Height / 8f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartDocumentPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(_rectBounds.X, _rectBounds.Y, _rectBounds.Right, _rectBounds.Y);
		graphicsPath.AddLine(_rectBounds.Right, _rectBounds.Y, _rectBounds.Right, _rectBounds.Y + _rectBounds.Height * 17322f / 21600f);
		PointF xYPosition = GetXYPosition(21600f, 17322f, 21600f);
		PointF xYPosition2 = GetXYPosition(10800f, 17322f, 21600f);
		PointF xYPosition3 = GetXYPosition(10800f, 23922f, 21600f);
		PointF xYPosition4 = GetXYPosition(0f, 20172f, 21600f);
		graphicsPath.AddBezier(xYPosition, xYPosition2, xYPosition3, xYPosition4);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartMultiDocumentPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 3675f, 21600f), GetXYPosition(18595f, 3675f, 21600f));
		graphicsPath.AddLine(GetXYPosition(18595f, 3675f, 21600f), GetXYPosition(18595f, 18022f, 21600f));
		graphicsPath.AddBezier(GetXYPosition(18595f, 18022f, 21600f), GetXYPosition(9298f, 18022f, 21600f), GetXYPosition(9298f, 23542f, 21600f), GetXYPosition(0f, 20782f, 21600f));
		graphicsPath.CloseFigure();
		graphicsPath.AddLine(GetXYPosition(1532f, 3675f, 21600f), GetXYPosition(1532f, 1815f, 21600f));
		graphicsPath.AddLine(GetXYPosition(1532f, 1815f, 21600f), GetXYPosition(20000f, 1815f, 21600f));
		graphicsPath.AddLine(GetXYPosition(20000f, 1815f, 21600f), GetXYPosition(20000f, 16252f, 21600f));
		graphicsPath.AddBezier(GetXYPosition(20000f, 16252f, 21600f), GetXYPosition(19298f, 16252f, 21600f), GetXYPosition(18595f, 16352f, 21600f), GetXYPosition(18595f, 16352f, 21600f));
		graphicsPath.StartFigure();
		graphicsPath.AddLine(GetXYPosition(2972f, 1815f, 21600f), GetXYPosition(2972f, 0f, 21600f));
		graphicsPath.AddLine(GetXYPosition(2972f, 0f, 21600f), GetXYPosition(21600f, 0f, 21600f));
		graphicsPath.AddLine(GetXYPosition(21600f, 0f, 21600f), GetXYPosition(21600f, 14392f, 21600f));
		graphicsPath.AddBezier(GetXYPosition(21600f, 14392f, 21600f), GetXYPosition(20800f, 14392f, 21600f), GetXYPosition(20000f, 14467f, 21600f), GetXYPosition(20000f, 14467f, 21600f));
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartTerminatorPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		SizeF sizeF = new SizeF(_rectBounds.Width * 3475f / 21600f * 2f, _rectBounds.Height * 10800f / 21600f * 2f);
		graphicsPath.AddLine(GetXYPosition(3475f, 0f, 21600f), GetXYPosition(18125f, 0f, 21600f));
		graphicsPath.StartFigure();
		PointF xYPosition = GetXYPosition(18125f, 0f, 21600f);
		RectangleF rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 270f, 180f);
		graphicsPath.AddLine(new PointF(rect.X, rect.Y + rect.Height), GetXYPosition(3475f, 21600f, 21600f));
		xYPosition = GetXYPosition(3475f, 0f, 21600f);
		rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 90f, 180f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartPreparationPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 5f, 10f), GetXYPosition(2f, 0f, 10f));
		graphicsPath.AddLine(GetXYPosition(2f, 0f, 10f), GetXYPosition(8f, 0f, 10f));
		graphicsPath.AddLine(GetXYPosition(8f, 0f, 10f), GetXYPosition(10f, 5f, 10f));
		graphicsPath.AddLine(GetXYPosition(10f, 5f, 10f), GetXYPosition(8f, 10f, 10f));
		graphicsPath.AddLine(GetXYPosition(8f, 10f, 10f), GetXYPosition(2f, 10f, 10f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartManualInputPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 1f, 5f), GetXYPosition(5f, 0f, 5f));
		graphicsPath.AddLine(GetXYPosition(5f, 0f, 5f), GetXYPosition(5f, 5f, 5f));
		graphicsPath.AddLine(GetXYPosition(5f, 5f, 5f), GetXYPosition(0f, 5f, 5f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartManualOperationPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 0f, 5f), GetXYPosition(5f, 0f, 5f));
		graphicsPath.AddLine(GetXYPosition(5f, 0f, 5f), GetXYPosition(4f, 5f, 5f));
		graphicsPath.AddLine(GetXYPosition(4f, 5f, 5f), GetXYPosition(1f, 5f, 5f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartConnectorPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		RectangleF rect = new RectangleF(new PointF(_rectBounds.X, _rectBounds.Y), new SizeF(_rectBounds.Width, _rectBounds.Height));
		graphicsPath.AddArc(rect, 180f, 90f);
		graphicsPath.AddArc(rect, 270f, 90f);
		graphicsPath.AddArc(rect, 0f, 90f);
		graphicsPath.AddArc(rect, 90f, 90f);
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartOffPageConnectorPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 0f, 10f), GetXYPosition(10f, 0f, 10f));
		graphicsPath.AddLine(GetXYPosition(10f, 0f, 10f), GetXYPosition(10f, 8f, 10f));
		graphicsPath.AddLine(GetXYPosition(10f, 8f, 10f), GetXYPosition(5f, 10f, 10f));
		graphicsPath.AddLine(GetXYPosition(5f, 10f, 10f), GetXYPosition(0f, 8f, 10f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartCardPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 1f, 5f), GetXYPosition(1f, 0f, 5f));
		graphicsPath.AddLine(GetXYPosition(1f, 0f, 5f), GetXYPosition(5f, 0f, 5f));
		graphicsPath.AddLine(GetXYPosition(5f, 0f, 5f), GetXYPosition(5f, 5f, 5f));
		graphicsPath.AddLine(GetXYPosition(5f, 5f, 5f), GetXYPosition(0f, 5f, 5f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartPunchedTapePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		RectangleF rect = new RectangleF(GetXYPosition(0f, 2f, 20f), new SizeF(_rectBounds.Width * 5f / 20f * 2f, _rectBounds.Height * 2f / 20f * 2f));
		graphicsPath.AddArc(rect, 180f, -180f);
		PointF location = new PointF(rect.X + rect.Width, rect.Y);
		rect = new RectangleF(location, new SizeF(_rectBounds.Width * 5f / 20f * 2f, _rectBounds.Height * 2f / 20f * 2f));
		graphicsPath.AddArc(rect, 180f, 180f);
		graphicsPath.AddLine(new PointF(rect.X + rect.Width, rect.Y + rect.Height), GetXYPosition(20f, 18f, 20f));
		rect = new RectangleF(new PointF(GetXYPosition(20f, 18f, 20f).X - rect.Width, GetXYPosition(20f, 18f, 20f).Y), new SizeF(_rectBounds.Width * 5f / 20f * 2f, _rectBounds.Height * 2f / 20f * 2f));
		graphicsPath.AddArc(rect, 0f, -180f);
		location = new PointF(rect.X - rect.Width, rect.Y);
		rect = new RectangleF(location, new SizeF(_rectBounds.Width * 5f / 20f * 2f, _rectBounds.Height * 2f / 20f * 2f));
		graphicsPath.AddArc(rect, 0f, 180f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartSummingJunctionPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.FlowChartSummingJunction);
		graphicsPath.AddLine(new PointF(_rectBounds.X + dictionary["il"], _rectBounds.Y + dictionary["it"]), new PointF(_rectBounds.X + dictionary["ir"], _rectBounds.Y + dictionary["ib"]));
		graphicsPath.StartFigure();
		graphicsPath.AddLine(new PointF(_rectBounds.X + dictionary["ir"], _rectBounds.Y + dictionary["it"]), new PointF(_rectBounds.X + dictionary["il"], _rectBounds.Y + dictionary["ib"]));
		graphicsPath.StartFigure();
		graphicsPath.AddArc(_rectBounds, 180f, 90f);
		graphicsPath.AddArc(_rectBounds, 270f, 90f);
		graphicsPath.AddArc(_rectBounds, 0f, 90f);
		graphicsPath.AddArc(_rectBounds, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartOrPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y), new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom));
		graphicsPath.StartFigure();
		graphicsPath.AddLine(new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f), new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f));
		graphicsPath.StartFigure();
		graphicsPath.AddArc(_rectBounds, 180f, 90f);
		graphicsPath.AddArc(_rectBounds, 270f, 90f);
		graphicsPath.AddArc(_rectBounds, 0f, 90f);
		graphicsPath.AddArc(_rectBounds, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartCollatePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 0f, 2f), GetXYPosition(2f, 0f, 2f));
		graphicsPath.AddLine(GetXYPosition(2f, 0f, 2f), GetXYPosition(1f, 1f, 2f));
		graphicsPath.AddLine(GetXYPosition(1f, 1f, 2f), GetXYPosition(2f, 2f, 2f));
		graphicsPath.AddLine(GetXYPosition(2f, 2f, 2f), GetXYPosition(0f, 2f, 2f));
		graphicsPath.AddLine(GetXYPosition(0f, 2f, 2f), GetXYPosition(1f, 1f, 2f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartSortPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 1f, 2f), GetXYPosition(2f, 1f, 2f));
		graphicsPath.AddLine(GetXYPosition(2f, 1f, 2f), GetXYPosition(0f, 1f, 2f));
		graphicsPath.AddLine(GetXYPosition(0f, 1f, 2f), GetXYPosition(1f, 0f, 2f));
		graphicsPath.AddLine(GetXYPosition(1f, 0f, 2f), GetXYPosition(2f, 1f, 2f));
		graphicsPath.AddLine(GetXYPosition(2f, 1f, 2f), GetXYPosition(1f, 2f, 2f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartExtractPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 2f, 2f), GetXYPosition(1f, 0f, 2f));
		graphicsPath.AddLine(GetXYPosition(1f, 0f, 2f), GetXYPosition(2f, 2f, 2f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartMergePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 0f, 2f), GetXYPosition(2f, 0f, 2f));
		graphicsPath.AddLine(GetXYPosition(2f, 0f, 2f), GetXYPosition(1f, 2f, 2f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartOnlineStoragePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(1f, 0f, 6f), GetXYPosition(6f, 0f, 6f));
		SizeF sizeF = new SizeF(_rectBounds.Width / 6f * 2f, _rectBounds.Height / 2f * 2f);
		PointF xYPosition = GetXYPosition(6f, 0f, 6f);
		RectangleF rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 270f, -180f);
		graphicsPath.AddLine(new PointF(xYPosition.X, xYPosition.Y + sizeF.Height), GetXYPosition(1f, 6f, 6f));
		xYPosition = GetXYPosition(1f, 0f, 6f);
		rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 90f, 180f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartDelayPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(new PointF(_rectBounds.X, _rectBounds.Y), new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y));
		SizeF sizeF = new SizeF(_rectBounds.Width, _rectBounds.Height);
		PointF pointF = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y);
		RectangleF rect = new RectangleF(pointF.X - sizeF.Width / 2f, pointF.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 270f, 180f);
		graphicsPath.AddLine(new PointF(pointF.X, pointF.Y + sizeF.Height), new PointF(_rectBounds.X, _rectBounds.Bottom));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartSequentialAccessStoragePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.FlowChartSequentialAccessStorage);
		SizeF sizeF = new SizeF(_rectBounds.Width, _rectBounds.Height);
		RectangleF rect = new RectangleF(_rectBounds.X, _rectBounds.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 90f, 90f);
		graphicsPath.AddArc(rect, 180f, 90f);
		graphicsPath.AddArc(rect, 270f, 90f);
		graphicsPath.AddArc(rect, 0f, dictionary["ang1"]);
		graphicsPath.AddLine(new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["ib"]), new PointF(_rectBounds.Right, _rectBounds.Bottom));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartMagneticDiskPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		SizeF sizeF = new SizeF(_rectBounds.Width, _rectBounds.Height / 3f);
		PointF xYPosition = GetXYPosition(6f, 1f, 6f);
		RectangleF rect = new RectangleF(xYPosition.X - sizeF.Width, xYPosition.Y - sizeF.Height / 2f, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 0f, 180f);
		graphicsPath.StartFigure();
		xYPosition = GetXYPosition(0f, 1f, 6f);
		rect = new RectangleF(xYPosition.X, xYPosition.Y - sizeF.Height / 2f, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 180f, 180f);
		graphicsPath.AddLine(new PointF(rect.X + rect.Width, rect.Y + sizeF.Height), GetXYPosition(6f, 5f, 6f));
		xYPosition = GetXYPosition(6f, 5f, 6f);
		rect = new RectangleF(xYPosition.X - sizeF.Width, xYPosition.Y - sizeF.Height / 2f, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 0f, 180f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartDirectAccessStoragePath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		SizeF sizeF = new SizeF(_rectBounds.Width / 3f, _rectBounds.Height);
		PointF xYPosition = GetXYPosition(5f, 6f, 6f);
		RectangleF rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y - sizeF.Height, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 90f, 180f);
		graphicsPath.StartFigure();
		graphicsPath.AddLine(GetXYPosition(1f, 0f, 6f), GetXYPosition(5f, 0f, 6f));
		xYPosition = GetXYPosition(5f, 0f, 6f);
		rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y, sizeF.Width, sizeF.Height);
		graphicsPath.AddArc(rect, 270f, 180f);
		graphicsPath.AddLine(new PointF(rect.X, rect.Y + sizeF.Height), GetXYPosition(1f, 6f, 6f));
		xYPosition = GetXYPosition(1f, 6f, 6f);
		rect = new RectangleF(xYPosition.X - sizeF.Width / 2f, xYPosition.Y - sizeF.Height, sizeF.Width, sizeF.Height);
		graphicsPath.StartFigure();
		graphicsPath.AddArc(rect, 90f, 180f);
		return graphicsPath;
	}

	internal IGraphicsPath GetFlowChartDisplayPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLine(GetXYPosition(0f, 3f, 6f), GetXYPosition(1f, 0f, 6f));
		graphicsPath.AddLine(GetXYPosition(1f, 0f, 6f), GetXYPosition(5f, 0f, 6f));
		graphicsPath.AddArc(GetXYPosition(5f, 0f, 6f).X - _rectBounds.Width / 6f, GetXYPosition(5f, 0f, 6f).Y, _rectBounds.Width / 3f, _rectBounds.Height, 270f, 180f);
		graphicsPath.AddLine(new PointF(GetXYPosition(5f, 0f, 6f).X - _rectBounds.Width / 6f, GetXYPosition(5f, 0f, 6f).Y + _rectBounds.Height), GetXYPosition(1f, 6f, 6f));
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetExplosion1()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[24]
		{
			GetXYPosition(10800f, 5800f, 21600f),
			GetXYPosition(14522f, 0f, 21600f),
			GetXYPosition(14155f, 5325f, 21600f),
			GetXYPosition(18380f, 4457f, 21600f),
			GetXYPosition(16702f, 7315f, 21600f),
			GetXYPosition(21097f, 8137f, 21600f),
			GetXYPosition(17607f, 10475f, 21600f),
			GetXYPosition(21600f, 13290f, 21600f),
			GetXYPosition(16837f, 12942f, 21600f),
			GetXYPosition(18145f, 18095f, 21600f),
			GetXYPosition(14020f, 14457f, 21600f),
			GetXYPosition(13247f, 19737f, 21600f),
			GetXYPosition(10532f, 14935f, 21600f),
			GetXYPosition(8485f, 21600f, 21600f),
			GetXYPosition(7715f, 15627f, 21600f),
			GetXYPosition(4762f, 17617f, 21600f),
			GetXYPosition(5667f, 13937f, 21600f),
			GetXYPosition(135f, 14587f, 21600f),
			GetXYPosition(3722f, 11775f, 21600f),
			GetXYPosition(0f, 8615f, 21600f),
			GetXYPosition(4627f, 7617f, 21600f),
			GetXYPosition(370f, 2295f, 21600f),
			GetXYPosition(7312f, 6320f, 21600f),
			GetXYPosition(8352f, 2295f, 21600f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetExplosion2()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[28]
		{
			GetXYPosition(11462f, 4342f, 21600f),
			GetXYPosition(14790f, 0f, 21600f),
			GetXYPosition(14525f, 5777f, 21600f),
			GetXYPosition(18007f, 3172f, 21600f),
			GetXYPosition(16380f, 6532f, 21600f),
			GetXYPosition(21600f, 6645f, 21600f),
			GetXYPosition(16985f, 9402f, 21600f),
			GetXYPosition(18270f, 11290f, 21600f),
			GetXYPosition(16380f, 12310f, 21600f),
			GetXYPosition(18877f, 15632f, 21600f),
			GetXYPosition(14640f, 14350f, 21600f),
			GetXYPosition(14942f, 17370f, 21600f),
			GetXYPosition(12180f, 15935f, 21600f),
			GetXYPosition(11612f, 18842f, 21600f),
			GetXYPosition(9872f, 17370f, 21600f),
			GetXYPosition(8700f, 19712f, 21600f),
			GetXYPosition(7527f, 18125f, 21600f),
			GetXYPosition(4917f, 21600f, 21600f),
			GetXYPosition(4805f, 18240f, 21600f),
			GetXYPosition(1285f, 17825f, 21600f),
			GetXYPosition(3330f, 15370f, 21600f),
			GetXYPosition(0f, 12877f, 21600f),
			GetXYPosition(3935f, 11592f, 21600f),
			GetXYPosition(1172f, 8270f, 21600f),
			GetXYPosition(5372f, 7817f, 21600f),
			GetXYPosition(4502f, 3625f, 21600f),
			GetXYPosition(8550f, 6382f, 21600f),
			GetXYPosition(9722f, 1887f, 21600f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar4Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star4Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar5Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star5Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[10]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy2"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar6Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star6Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[12]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + _rectBounds.Height / 2f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar7Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star7Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[14]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy3"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar8Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star8Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[16]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy3"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar10Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star10Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[20]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + _rectBounds.Height / 2f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar12Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star12Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[24]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 4f, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + _rectBounds.Height / 4f),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 4f, _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy4"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar16Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star16Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[32]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx7"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx8"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx8"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx7"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy7"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy8"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy8"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy7"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy5"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar24Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star24Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[48]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx7"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx8"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx9"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx10"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx11"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["sx12"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx12"], _rectBounds.Y + dictionary["sy7"]),
			new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["sx11"], _rectBounds.Y + dictionary["sy8"]),
			new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["sx10"], _rectBounds.Y + dictionary["sy9"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["sx9"], _rectBounds.Y + dictionary["sy10"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y9"]),
			new PointF(_rectBounds.X + dictionary["sx8"], _rectBounds.Y + dictionary["sy11"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y10"]),
			new PointF(_rectBounds.X + dictionary["sx7"], _rectBounds.Y + dictionary["sy12"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy12"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y10"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy11"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y9"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy10"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy9"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy8"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy7"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetStar32Point()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Star32Point);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[64]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy8"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy7"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx7"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx8"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["sx9"], _rectBounds.Y + dictionary["sy1"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["sx10"], _rectBounds.Y + dictionary["sy2"]),
			new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["sx11"], _rectBounds.Y + dictionary["sy3"]),
			new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["sx12"], _rectBounds.Y + dictionary["sy4"]),
			new PointF(_rectBounds.X + dictionary["x11"], _rectBounds.Y + dictionary["y4"]),
			new PointF(_rectBounds.X + dictionary["sx13"], _rectBounds.Y + dictionary["sy5"]),
			new PointF(_rectBounds.X + dictionary["x12"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["sx14"], _rectBounds.Y + dictionary["sy6"]),
			new PointF(_rectBounds.X + dictionary["x13"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + dictionary["sx15"], _rectBounds.Y + dictionary["sy7"]),
			new PointF(_rectBounds.X + dictionary["x14"], _rectBounds.Y + dictionary["y7"]),
			new PointF(_rectBounds.X + dictionary["sx16"], _rectBounds.Y + dictionary["sy8"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + _rectBounds.Height / 2f),
			new PointF(_rectBounds.X + dictionary["sx16"], _rectBounds.Y + dictionary["sy9"]),
			new PointF(_rectBounds.X + dictionary["x14"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["sx15"], _rectBounds.Y + dictionary["sy10"]),
			new PointF(_rectBounds.X + dictionary["x13"], _rectBounds.Y + dictionary["y9"]),
			new PointF(_rectBounds.X + dictionary["sx14"], _rectBounds.Y + dictionary["sy11"]),
			new PointF(_rectBounds.X + dictionary["x12"], _rectBounds.Y + dictionary["y10"]),
			new PointF(_rectBounds.X + dictionary["sx13"], _rectBounds.Y + dictionary["sy12"]),
			new PointF(_rectBounds.X + dictionary["x11"], _rectBounds.Y + dictionary["y11"]),
			new PointF(_rectBounds.X + dictionary["sx12"], _rectBounds.Y + dictionary["sy13"]),
			new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y12"]),
			new PointF(_rectBounds.X + dictionary["sx11"], _rectBounds.Y + dictionary["sy14"]),
			new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y13"]),
			new PointF(_rectBounds.X + dictionary["sx10"], _rectBounds.Y + dictionary["sy15"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y14"]),
			new PointF(_rectBounds.X + dictionary["sx9"], _rectBounds.Y + dictionary["sy16"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["sx8"], _rectBounds.Y + dictionary["sy16"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y14"]),
			new PointF(_rectBounds.X + dictionary["sx7"], _rectBounds.Y + dictionary["sy15"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y13"]),
			new PointF(_rectBounds.X + dictionary["sx6"], _rectBounds.Y + dictionary["sy14"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y12"]),
			new PointF(_rectBounds.X + dictionary["sx5"], _rectBounds.Y + dictionary["sy13"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y11"]),
			new PointF(_rectBounds.X + dictionary["sx4"], _rectBounds.Y + dictionary["sy12"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y10"]),
			new PointF(_rectBounds.X + dictionary["sx3"], _rectBounds.Y + dictionary["sy11"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y9"]),
			new PointF(_rectBounds.X + dictionary["sx2"], _rectBounds.Y + dictionary["sy10"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y8"]),
			new PointF(_rectBounds.X + dictionary["sx1"], _rectBounds.Y + dictionary["sy9"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetUpRibbon()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.UpRibbon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[25];
		array[0] = new PointF(_rectBounds.X, _rectBounds.Bottom);
		array[1] = new PointF(_rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLine(array[0], array[1]);
		array[2] = new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[1], array[2]);
		array[3] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[2], array[3]);
		array[4] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["hR"]);
		graphicsPath.AddLine(array[3], array[4]);
		graphicsPath.AddArc(array[4].X, array[4].Y - dictionary["hR"], _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 180f, 90f);
		array[5] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y);
		graphicsPath.AddLine(new PointF(array[4].X + _rectBounds.Width / 32f, array[4].Y - dictionary["hR"]), new PointF(array[5].X - _rectBounds.Width / 32f, array[5].Y));
		graphicsPath.AddArc(array[5].X - _rectBounds.Width / 16f, array[5].Y, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 270f, 90f);
		array[6] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(new PointF(array[5].X, array[5].Y + dictionary["hR"]), array[6]);
		array[7] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[6], array[7]);
		array[8] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[7], array[8]);
		array[9] = new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLine(array[8], array[9]);
		array[10] = new PointF(_rectBounds.Right, _rectBounds.Bottom);
		graphicsPath.AddLine(array[9], array[10]);
		array[11] = new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Bottom);
		graphicsPath.AddLine(array[10], new PointF(array[11].X + _rectBounds.Width / 32f, array[11].Y));
		graphicsPath.AddArc(array[11].X, array[11].Y - dictionary["hR"] * 2f, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 90f, 180f);
		array[12] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddLine(new PointF(array[11].X + _rectBounds.Width / 32f, array[11].Y - dictionary["hR"] * 2f), new PointF(array[12].X - _rectBounds.Width / 32f, array[12].Y));
		graphicsPath.AddArc(array[12].X - _rectBounds.Width / 32f * 2f, array[12].Y - dictionary["hR"] * 2f, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 90f, -180f);
		array[13] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(new PointF(array[12].X - _rectBounds.Width / 32f * 2f, array[12].Y - dictionary["hR"] * 2f), new PointF(array[13].X + _rectBounds.Width / 32f, array[13].Y));
		graphicsPath.AddArc(array[13].X, array[13].Y, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 270f, -180f);
		array[14] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddLine(new PointF(array[13].X + _rectBounds.Width / 32f, array[13].Y + dictionary["hR"] * 2f), new PointF(array[14].X - _rectBounds.Width / 32f, array[14].Y));
		graphicsPath.AddArc(array[14].X - _rectBounds.Width / 32f * 2f, array[14].Y, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 270f, 180f);
		graphicsPath.CloseFigure();
		graphicsPath.StartFigure();
		array[15] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y2"]);
		array[16] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y6"]);
		graphicsPath.AddLine(array[15], array[16]);
		graphicsPath.StartFigure();
		array[17] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y6"]);
		array[18] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(array[17], array[18]);
		graphicsPath.StartFigure();
		array[19] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y7"]);
		array[20] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[19], array[20]);
		graphicsPath.StartFigure();
		array[21] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		array[22] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y7"]);
		graphicsPath.AddLine(array[21], array[22]);
		return graphicsPath;
	}

	internal IGraphicsPath GetDownRibbon()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DownRibbon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[23]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddLine(array[0], new PointF(array[1].X - _rectBounds.Width / 32f, array[1].Y));
		graphicsPath.AddArc(array[1].X - _rectBounds.Width / 32f * 2f, array[1].Y, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 270f, 180f);
		array[2] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddLine(new PointF(array[1].X - _rectBounds.Width / 32f, array[1].Y + dictionary["hR"] * 2f), new PointF(array[2].X + _rectBounds.Width / 32f, array[2].Y));
		graphicsPath.AddArc(array[2].X, array[2].Y, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 270f, -180f);
		array[3] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(new PointF(array[2].X + _rectBounds.Width / 32f, array[2].Y + dictionary["hR"] * 2f), new PointF(array[3].X - _rectBounds.Width / 32f, array[3].Y));
		graphicsPath.AddArc(array[3].X - _rectBounds.Width / 32f * 2f, array[3].Y - dictionary["hR"] * 2f, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 90f, -180f);
		array[4] = new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddLine(new PointF(array[3].X - _rectBounds.Width / 32f, array[3].Y - dictionary["hR"] * 2f), new PointF(array[4].X + _rectBounds.Width / 32f, array[4].Y));
		graphicsPath.AddArc(array[4].X, array[4].Y - dictionary["hR"] * 2f, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 90f, 180f);
		array[5] = new PointF(_rectBounds.Right, _rectBounds.Y);
		graphicsPath.AddLine(new PointF(array[4].X + _rectBounds.Width / 32f, array[4].Y - dictionary["hR"] * 2f), array[5]);
		array[6] = new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLine(array[5], array[6]);
		array[7] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[6], array[7]);
		array[8] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[7], array[8]);
		array[9] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y5"]);
		graphicsPath.AddLine(array[8], array[9]);
		graphicsPath.AddArc(array[9].X - _rectBounds.Width / 32f * 2f, array[9].Y - dictionary["hR"], _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 0f, 90f);
		array[10] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Bottom);
		graphicsPath.AddLine(new PointF(array[9].X - _rectBounds.Width / 32f, array[9].Y + dictionary["hR"]), new PointF(array[10].X + _rectBounds.Width / 32f, array[10].Y));
		graphicsPath.AddArc(array[10].X, array[10].Y - dictionary["hR"] * 2f, _rectBounds.Width / 32f * 2f, dictionary["hR"] * 2f, 90f, 90f);
		array[11] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(new PointF(array[10].X, array[10].Y - dictionary["hR"]), array[11]);
		array[12] = new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[11], array[12]);
		array[13] = new PointF(_rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLine(array[12], array[13]);
		graphicsPath.CloseFigure();
		array[14] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["hR"]);
		array[15] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(array[14], array[15]);
		graphicsPath.StartFigure();
		array[16] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]);
		array[17] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["hR"]);
		graphicsPath.AddLine(array[16], array[17]);
		graphicsPath.StartFigure();
		array[18] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y4"]);
		array[19] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y6"]);
		graphicsPath.AddLine(array[18], array[19]);
		graphicsPath.StartFigure();
		array[21] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y6"]);
		array[22] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[21], array[22]);
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedUpRibbon()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedUpRibbon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[5]
		{
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["q1"]),
			new PointF(_rectBounds.X + dictionary["cx4"], _rectBounds.Y + dictionary["cy4"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y5"])
		};
		graphicsPath.AddLines(new PointF[2]
		{
			array[0],
			array[1]
		});
		graphicsPath.AddBezier(array[2], array[2], array[3], array[4]);
		PointF[] array2 = new PointF[3]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y6"]),
			new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["cy6"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y6"])
		};
		graphicsPath.AddBezier(array2[0], array2[0], array2[1], array2[2]);
		PointF[] array3 = new PointF[3]
		{
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["cx5"], _rectBounds.Y + dictionary["cy4"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["q1"])
		};
		graphicsPath.AddBezier(array3[0], array3[0], array3[1], array3[2]);
		PointF[] array4 = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["cx2"], _rectBounds.Y + dictionary["cy1"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"])
		};
		graphicsPath.AddLines(new PointF[1] { array4[0] });
		graphicsPath.AddBezier(array4[1], array4[1], array4[2], array4[3]);
		array3[0] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y3"]);
		array3[1] = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["cy3"]);
		array3[2] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddBezier(array3[0], array3[0], array3[1], array3[2]);
		PointF[] array5 = new PointF[3]
		{
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["cx1"], _rectBounds.Y + dictionary["cy1"]),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		};
		graphicsPath.AddBezier(array5[0], array5[0], array5[1], array5[2]);
		graphicsPath.CloseFigure();
		PointF[] array6 = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y5"])
		};
		graphicsPath.AddLines(array6);
		graphicsPath.CloseFigure();
		array6[0] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y5"]);
		array6[1] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLines(array6);
		graphicsPath.CloseFigure();
		array6[0] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y7"]);
		array6[1] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddLines(array6);
		graphicsPath.CloseFigure();
		array6[0] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]);
		array6[1] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y7"]);
		graphicsPath.AddLines(array6);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCurvedDownRibbon()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CurvedDownRibbon);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[3]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["cx1"], _rectBounds.Y + dictionary["cy1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"])
		};
		graphicsPath.AddBezier(array[0], array[0], array[1], array[2]);
		array[0] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"]);
		array[1] = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["cy3"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddBezier(array[0], array[0], array[1], array[2]);
		array[0] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]);
		array[1] = new PointF(_rectBounds.X + dictionary["cx2"], _rectBounds.Y + dictionary["cy1"]);
		array[2] = new PointF(_rectBounds.Right, _rectBounds.Y);
		graphicsPath.AddBezier(array[0], array[0], array[1], array[2]);
		PointF[] array2 = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["rh"]),
			new PointF(_rectBounds.X + dictionary["cx5"], _rectBounds.Y + dictionary["cy4"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y5"])
		};
		PointF[] array3 = new PointF[1] { array2[0] };
		graphicsPath.AddLines(array3);
		graphicsPath.AddBezier(array2[1], array2[1], array2[2], array2[3]);
		array[0] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y6"]);
		array[1] = new PointF(_rectBounds.X + _rectBounds.Width / 2f, _rectBounds.Y + dictionary["cy6"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y6"]);
		graphicsPath.AddBezier(array[0], array[0], array[1], array[2]);
		array[0] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y5"]);
		array[1] = new PointF(_rectBounds.X + dictionary["cx4"], _rectBounds.Y + dictionary["cy4"]);
		array[2] = new PointF(_rectBounds.X, _rectBounds.Y + dictionary["rh"]);
		graphicsPath.AddBezier(array[0], array[0], array[1], array[2]);
		array[0] = new PointF(_rectBounds.X + _rectBounds.Width / 8f, _rectBounds.Y + dictionary["y2"]);
		array3[0] = array[0];
		graphicsPath.AddLines(array3);
		graphicsPath.CloseFigure();
		PointF[] array4 = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y5"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y3"])
		};
		graphicsPath.AddLines(array4);
		graphicsPath.CloseFigure();
		array4[0] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y3"]);
		array4[1] = new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y5"]);
		graphicsPath.AddLines(array4);
		graphicsPath.CloseFigure();
		array4[0] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y1"]);
		array4[1] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y7"]);
		graphicsPath.AddLines(array4);
		graphicsPath.CloseFigure();
		array4[0] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y7"]);
		array4[1] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y1"]);
		graphicsPath.AddLines(array4);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetVerticalScroll()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.VerticalScroll);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y)
		};
		graphicsPath.AddLines(new PointF[1] { array[0] });
		graphicsPath.AddArc(array[1].X, array[1].Y, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 180f, 90f);
		PointF[] array2 = new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y)
		};
		graphicsPath.AddArc(array2[0].X - dictionary["ch2"], array2[0].Y, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 270f, 180f);
		PointF[] array3 = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["ch"]),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y4"])
		};
		graphicsPath.AddLines(new PointF[1] { array3[0] });
		graphicsPath.AddArc(array3[1].X - dictionary["ch2"] * 2f, array3[1].Y - dictionary["ch2"], dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 0f, 90f);
		PointF[] array4 = new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["ch2"], _rectBounds.Bottom)
		};
		graphicsPath.AddArc(array4[0].X - dictionary["ch2"], array4[0].Y - dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 90f, 180f);
		graphicsPath.CloseFigure();
		PointF[] array5 = new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y)
		};
		graphicsPath.StartFigure();
		graphicsPath.AddArc(array5[0].X - dictionary["ch2"], array5[0].Y, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 270f, 180f);
		graphicsPath.StartFigure();
		graphicsPath.AddArc(array5[0].X - dictionary["ch2"] / 2f, array5[0].Y + dictionary["ch2"], dictionary["ch4"] * 2f, dictionary["ch4"] * 2f, 90f, 180f);
		graphicsPath.AddLines(new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["ch2"])
		});
		PointF[] array6 = new PointF[2];
		graphicsPath.StartFigure();
		array6[0] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["ch"]);
		array6[1] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["ch"]);
		graphicsPath.AddLines(array6);
		graphicsPath.CloseFigure();
		PointF[] array7 = new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["ch2"], _rectBounds.Y + dictionary["y3"])
		};
		graphicsPath.StartFigure();
		graphicsPath.AddArc(array7[0].X - dictionary["ch2"] / 2f, array7[0].Y, dictionary["ch4"] * 2f, dictionary["ch4"] * 2f, 270f, 180f);
		graphicsPath.AddLines(new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y + dictionary["y4"])
		});
		PointF[] array8 = new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["ch2"], _rectBounds.Bottom)
		};
		graphicsPath.StartFigure();
		graphicsPath.AddArc(array8[0].X - dictionary["ch2"], array8[0].Y - dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 90f, -90f);
		graphicsPath.AddLines(new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y + dictionary["y3"])
		});
		return graphicsPath;
	}

	internal IGraphicsPath[] GetHorizontalScroll()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.HorizontalScroll);
		IGraphicsPath[] array = new IGraphicsPath[7]
		{
			GetGraphicsPath(),
			GetGraphicsPath(),
			GetGraphicsPath(),
			GetGraphicsPath(),
			GetGraphicsPath(),
			GetGraphicsPath(),
			GetGraphicsPath()
		};
		PointF[] array2 = new PointF[1]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y3"])
		};
		if (dictionary["ch2"] > 0f)
		{
			array[0].AddArc(array2[0].X, array2[0].Y - dictionary["ch2"], dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 180f, 90f);
		}
		PointF[] array3 = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["ch"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["ch2"])
		};
		PointF[] array4 = new PointF[1] { array3[0] };
		array[0].AddLines(array4);
		if (dictionary["ch2"] > 0f)
		{
			array[0].AddArc(array3[1].X, array3[1].Y - dictionary["ch2"], dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 180f, 180f);
		}
		array2[0] = new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y5"]);
		if (dictionary["ch2"] > 0f)
		{
			array[0].AddArc(array2[0].X - dictionary["ch2"] * 2f, array2[0].Y - dictionary["ch2"], dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 0f, 90f);
		}
		array3[0] = new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y + dictionary["y6"]);
		array3[1] = new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y + dictionary["y7"]);
		array4[0] = array3[0];
		array[0].AddLines(array4);
		if (dictionary["ch2"] > 0f)
		{
			array[0].AddArc(array3[0].X - dictionary["ch2"] * 2f, array3[0].Y, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 0f, 180f);
		}
		array[0].CloseFigure();
		array3[0] = new PointF(_rectBounds.X + dictionary["x3"] + dictionary["ch2"], _rectBounds.Y + dictionary["ch"]);
		array3[1] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["ch"]);
		PointF[] array5 = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["ch"]),
			array3[0]
		};
		array[1].AddLines(array5);
		if (dictionary["ch2"] > 0f)
		{
			array[2].AddArc(array3[1].X - dictionary["ch2"], array3[1].Y - dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 90f, -90f);
		}
		array[1].CloseFigure();
		array3[0] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["ch"] - dictionary["ch2"]);
		array3[1] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["ch2"]);
		array5[0] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["ch"]);
		array5[1] = array3[0];
		array[3].AddLines(array5);
		if (dictionary["ch4"] > 0f)
		{
			array[3].AddArc(array3[1].X - dictionary["ch2"], array3[1].Y - dictionary["ch2"] / 2f, dictionary["ch4"] * 2f, dictionary["ch4"] * 2f, 0f, 180f);
		}
		array3[0] = new PointF(_rectBounds.X + dictionary["ch2"] * 2f, _rectBounds.Y - dictionary["ch2"] + dictionary["y4"]);
		array3[1] = new PointF(_rectBounds.X + dictionary["ch2"], _rectBounds.Y + dictionary["y3"]);
		array5[0] = new PointF(_rectBounds.X + dictionary["ch"], _rectBounds.Y + dictionary["y6"]);
		array5[1] = array3[0];
		array[4].AddLines(array5);
		if (dictionary["ch4"] > 0f)
		{
			array[5].AddArc(array3[1].X, array3[1].Y - dictionary["ch2"] / 2f, dictionary["ch4"] * 2f, dictionary["ch4"] * 2f, 180f, 180f);
		}
		if (dictionary["ch2"] > 0f)
		{
			array[5].AddArc(array3[1].X - dictionary["ch2"], array3[1].Y - dictionary["ch2"], dictionary["ch2"] * 2f, dictionary["ch2"] * 2f, 0f, 180f);
		}
		array3[0] = new PointF(_rectBounds.X + dictionary["ch"] - dictionary["ch2"], _rectBounds.Y + dictionary["y3"]);
		array3[1] = new PointF(_rectBounds.X + dictionary["ch"] - dictionary["ch2"], _rectBounds.Y + dictionary["y3"] + dictionary["ch2"]);
		array[6].AddLines(array3);
		return array;
	}

	internal IGraphicsPath GetWave()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.Wave);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"])
		};
		graphicsPath.AddBezier(array[0], array[1], array[2], array[3]);
		array[0] = new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y4"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y6"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y5"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddBezier(array[0], array[1], array[2], array[3]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetDoubleWave()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.DoubleWave);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x5"], _rectBounds.Y + dictionary["y1"])
		};
		graphicsPath.AddBezier(array[0], array[1], array[2], array[3]);
		PointF[] array2 = new PointF[4]
		{
			default(PointF),
			new PointF(_rectBounds.X + dictionary["x6"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x7"], _rectBounds.Y + dictionary["y3"]),
			new PointF(_rectBounds.X + dictionary["x8"], _rectBounds.Y + dictionary["y1"])
		};
		graphicsPath.AddBezier(array[3], array2[1], array2[2], array2[3]);
		array[0] = new PointF(_rectBounds.X + dictionary["x15"], _rectBounds.Y + dictionary["y4"]);
		array[1] = new PointF(_rectBounds.X + dictionary["x14"], _rectBounds.Y + dictionary["y6"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x13"], _rectBounds.Y + dictionary["y5"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x12"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddBezier(array[0], array[1], array[2], array[3]);
		array2[1] = new PointF(_rectBounds.X + dictionary["x11"], _rectBounds.Y + dictionary["y6"]);
		array2[2] = new PointF(_rectBounds.X + dictionary["x10"], _rectBounds.Y + dictionary["y5"]);
		array2[3] = new PointF(_rectBounds.X + dictionary["x9"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddBezier(array[3], array2[1], array2[2], array2[3]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRectangularCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RectangularCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[16]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["xt"], _rectBounds.Y + dictionary["yt"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["xr"], _rectBounds.Y + dictionary["yr"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["xb"], _rectBounds.Y + dictionary["yb"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["xl"], _rectBounds.Y + dictionary["yl"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetRoundedRectangularCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.RoundedRectangularCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[4];
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, dictionary["u1"] * 2f, dictionary["u1"] * 2f, 180f, 90f);
		array[0] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y);
		array[1] = new PointF(_rectBounds.X + dictionary["xt"], _rectBounds.Y + dictionary["yt"]);
		array[2] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y);
		array[3] = new PointF(_rectBounds.X + dictionary["u2"], _rectBounds.Y);
		graphicsPath.AddLines(array);
		graphicsPath.AddArc(array[3].X - dictionary["u1"], array[3].Y, dictionary["u1"] * 2f, dictionary["u1"] * 2f, 270f, 90f);
		array = new PointF[4]
		{
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["xr"], _rectBounds.Y + dictionary["yr"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.Right, _rectBounds.Y + dictionary["v2"])
		};
		graphicsPath.AddLines(array);
		graphicsPath.AddArc(array[3].X - dictionary["u1"] * 2f, array[3].Y - dictionary["u1"], dictionary["u1"] * 2f, dictionary["u1"] * 2f, 0f, 90f);
		array = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["xb"], _rectBounds.Y + dictionary["yb"]),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			new PointF(_rectBounds.X + dictionary["u1"], _rectBounds.Bottom)
		};
		graphicsPath.AddLines(array);
		graphicsPath.AddArc(array[3].X - dictionary["u1"], array[3].Y - dictionary["u1"] * 2f, dictionary["u1"] * 2f, dictionary["u1"] * 2f, 90f, 90f);
		graphicsPath.AddLines(new PointF[3]
		{
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["xl"], _rectBounds.Y + dictionary["yl"]),
			new PointF(_rectBounds.X, _rectBounds.Y + dictionary["y1"])
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetOvalCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.OvalCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] array = new PointF[1]
		{
			new PointF(_rectBounds.X + dictionary["xPos"], _rectBounds.Y + dictionary["yPos"])
		};
		float num = dictionary["stAng1"];
		float num2 = dictionary["swAng"];
		if ((num < 180f && array[0].X < _rectBounds.X + _rectBounds.Width / 2f) || (num < 0f && array[0].Y > _rectBounds.Y))
		{
			num += 180f;
		}
		if (num2 < 180f)
		{
			num2 += 180f;
		}
		graphicsPath.AddArc(_rectBounds.X, _rectBounds.Y, _rectBounds.Width, _rectBounds.Height, num, num2);
		graphicsPath.AddLines(array);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetCloudCalloutPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.CloudCallout);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF xYPosition = GetXYPosition(3900f, 14370f, 43200f);
		xYPosition.X += dictionary["g27"];
		xYPosition.Y -= dictionary["g30"];
		SizeF sizeF = new SizeF(_rectBounds.Width * 6753f / 43200f * 2f, _rectBounds.Height * 9190f / 43200f * 2f);
		SizeF sizeF2 = new SizeF(_rectBounds.Width * 5333f / 43200f * 2f, _rectBounds.Height * 7267f / 43200f * 2f);
		SizeF sizeF3 = new SizeF(_rectBounds.Width * 4365f / 43200f * 2f, _rectBounds.Height * 5945f / 43200f * 2f);
		SizeF sizeF4 = new SizeF(_rectBounds.Width * 4857f / 43200f * 2f, _rectBounds.Height * 6595f / 43200f * 2f);
		SizeF sizeF5 = new SizeF(_rectBounds.Width * 5333f / 43200f * 2f, _rectBounds.Height * 7273f / 43200f * 2f);
		SizeF sizeF6 = new SizeF(_rectBounds.Width * 6775f / 43200f * 2f, _rectBounds.Height * 9220f / 43200f * 2f);
		SizeF sizeF7 = new SizeF(_rectBounds.Width * 5785f / 43200f * 2f, _rectBounds.Height * 7867f / 43200f * 2f);
		SizeF sizeF8 = new SizeF(_rectBounds.Width * 6752f / 43200f * 2f, _rectBounds.Height * 9215f / 43200f * 2f);
		SizeF sizeF9 = new SizeF(_rectBounds.Width * 7720f / 43200f * 2f, _rectBounds.Height * 10543f / 43200f * 2f);
		SizeF sizeF10 = new SizeF(_rectBounds.Width * 4360f / 43200f * 2f, _rectBounds.Height * 5918f / 43200f * 2f);
		SizeF sizeF11 = new SizeF(_rectBounds.Width * 4345f / 43200f * 2f, _rectBounds.Height * 5945f / 43200f * 2f);
		graphicsPath.AddArc(_rectBounds.X + 4076f * _rectBounds.Width / 43200f, _rectBounds.Y + 3912f * _rectBounds.Height / 43200f, sizeF.Width, sizeF.Height, -190f, 123f);
		graphicsPath.AddArc(_rectBounds.X + 13469f * _rectBounds.Width / 43200f, _rectBounds.Y + 1304f * _rectBounds.Height / 43200f, sizeF2.Width, sizeF2.Height, -144f, 89f);
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f + 531f * _rectBounds.Width / 43200f, _rectBounds.Y + 1f, sizeF3.Width, sizeF3.Height, -145f, 99f);
		graphicsPath.AddArc(xYPosition.X + _rectBounds.Width / 2f + 3013f * _rectBounds.Width / 43200f, _rectBounds.Y + 1f, sizeF4.Width, sizeF4.Height, -130f, 117f);
		graphicsPath.AddArc(_rectBounds.Right - sizeF5.Width - 708f * _rectBounds.Width / 43200f, _rectBounds.Y + sizeF4.Height / 2f - 1127f * _rectBounds.Height / 43200f, sizeF5.Width, sizeF5.Height, -78f, 109f);
		graphicsPath.AddArc(_rectBounds.Right - sizeF6.Width + 354f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f - 9129f * _rectBounds.Height / 43200f, sizeF6.Width, sizeF6.Height, -46f, 130f);
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f + 4608f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f + 869f * _rectBounds.Height / 43200f, sizeF7.Width, sizeF7.Height, 0f, 114f);
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f - sizeF8.Width / 2f + 886f * _rectBounds.Width / 43200f, _rectBounds.Bottom - sizeF8.Height, sizeF8.Width, sizeF8.Height, 22f, 115f);
		graphicsPath.AddArc(_rectBounds.X + 4962f * _rectBounds.Width / 43200f, _rectBounds.Bottom - sizeF9.Height - 2173f * _rectBounds.Height / 43200f, sizeF9.Width, sizeF9.Height, 66f, 75f);
		graphicsPath.AddArc(_rectBounds.X + 1063f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f + 2608f * _rectBounds.Height / 43200f, sizeF10.Width, sizeF10.Height, -274f, 146f);
		graphicsPath.AddArc(_rectBounds.X + 1f, _rectBounds.Y + _rectBounds.Height / 2f - sizeF11.Height / 2f - 1304f * _rectBounds.Height / 43200f, sizeF11.Width, sizeF11.Height, -246f, 152f);
		graphicsPath.CloseFigure();
		graphicsPath.AddArc(_rectBounds.X + _rectBounds.Width / 2f + 2658f * _rectBounds.Width / 43200f, _rectBounds.Y + _rectBounds.Height / 2f, _rectBounds.Width * 6753f / 43200f * 2f, _rectBounds.Height * 9190f / 43200f * 2f, -58f, 59f);
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout1Path()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout1);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		PointF[] array = new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"])
		};
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout2Path()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout2);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		graphicsPath.StartFigure();
		PointF[] array = new PointF[3]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"])
		};
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[1], array[2]);
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout3Path()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout3);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		PointF[] array = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]),
			default(PointF),
			default(PointF)
		};
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[0], array[1]);
		array[2] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[1], array[2]);
		array[3] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.StartFigure();
		graphicsPath.AddLine(array[2], array[3]);
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout1AccentBarPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout1AccentBar);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		PointF[] array = new PointF[4]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.StartFigure();
		array[2] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(array[2], array[3]);
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout2AccentBarPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout2AccentBar);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		PointF[] array = new PointF[5]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			default(PointF),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.StartFigure();
		array[2] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(array[2], array[3]);
		graphicsPath.StartFigure();
		array[4] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLine(array[3], array[4]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout3AccentBarPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout3AccentBar);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		PointF[] array = new PointF[6]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y),
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Bottom),
			default(PointF),
			default(PointF),
			default(PointF),
			default(PointF)
		};
		graphicsPath.AddLine(array[0], array[1]);
		graphicsPath.StartFigure();
		array[2] = new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]);
		array[3] = new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"]);
		graphicsPath.AddLine(array[2], array[3]);
		graphicsPath.StartFigure();
		array[4] = new PointF(_rectBounds.X + dictionary["x3"], _rectBounds.Y + dictionary["y3"]);
		graphicsPath.AddLine(array[3], array[4]);
		graphicsPath.StartFigure();
		array[5] = new PointF(_rectBounds.X + dictionary["x4"], _rectBounds.Y + dictionary["y4"]);
		graphicsPath.AddLine(array[4], array[5]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout1NoBorderPath()
	{
		Dictionary<string, float> dictionary = ParseShapeFormula(AutoShapeType.LineCallout1NoBorder);
		IGraphicsPath graphicsPath = GetGraphicsPath();
		graphicsPath.AddLines(new PointF[4]
		{
			new PointF(_rectBounds.X, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Y),
			new PointF(_rectBounds.Right, _rectBounds.Bottom),
			new PointF(_rectBounds.X, _rectBounds.Bottom)
		});
		graphicsPath.CloseFigure();
		graphicsPath.AddLines(new PointF[2]
		{
			new PointF(_rectBounds.X + dictionary["x1"], _rectBounds.Y + dictionary["y1"]),
			new PointF(_rectBounds.X + dictionary["x2"], _rectBounds.Y + dictionary["y2"])
		});
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout2NoBorderPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] linePoints = new PointF[2];
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout3NoBorderPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] linePoints = new PointF[2];
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout1BorderAndAccentBarPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] linePoints = new PointF[2];
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout2BorderAndAccentBarPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] linePoints = new PointF[2];
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetLineCallout3BorderAndAccentBarPath()
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF[] linePoints = new PointF[2];
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	internal IGraphicsPath GetVMLCustomShapePath(List<DocGen.DocIO.DLS.Path2D> path2DPoints)
	{
		IGraphicsPath graphicsPath = GetGraphicsPath();
		PointF pointF = Point.Empty;
		for (int i = 0; i < path2DPoints.Count; i++)
		{
			DocGen.DocIO.DLS.Path2D path2D = path2DPoints[i];
			switch (path2D.PathCommandType)
			{
			case "l":
			case "r":
				if (path2D.PathPoints.Count > 0)
				{
					PointF[] array = new PointF[path2D.PathPoints.Count + 1];
					array[0] = pointF;
					for (int j = 0; j < path2D.PathPoints.Count; j++)
					{
						array[j + 1] = path2D.PathPoints[j];
					}
					graphicsPath.AddLines(array);
					pointF = array[^1];
				}
				break;
			case "m":
			case "t":
				if (path2D.PathPoints.Count > 0)
				{
					graphicsPath.CloseFigure();
					pointF = path2D.PathPoints[path2D.PathPoints.Count - 1];
				}
				break;
			case "x":
				graphicsPath.CloseFigure();
				pointF = Point.Empty;
				break;
			case "e":
				graphicsPath.CloseFigure();
				pointF = Point.Empty;
				break;
			}
		}
		return graphicsPath;
	}

	internal IGraphicsPath GetCustomGeomentryPath(RectangleF bounds, IGraphicsPath path, Shape shape)
	{
		Dictionary<string, string> guideList = shape.GetGuideList();
		Dictionary<string, string> avList = shape.GetAvList();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, float> dictionary2 = new Dictionary<string, float>();
		foreach (KeyValuePair<string, string> item in guideList)
		{
			dictionary.Add(item.Key, item.Value);
		}
		foreach (KeyValuePair<string, string> item2 in avList)
		{
			if (!guideList.ContainsKey(item2.Key))
			{
				dictionary.Add(item2.Key, item2.Value);
			}
		}
		foreach (DocGen.DocIO.DLS.Path2D path2D in shape.Path2DList)
		{
			List<double> list = new List<double>();
			foreach (string pathElement in path2D.PathElements)
			{
				ConvertPathElement(pathElement, list, dictionary, path2D, dictionary2);
			}
			double width = path2D.Width;
			double height = path2D.Height;
			GetGeomentryPath(path, list, width, height, bounds);
			list.Clear();
			list = null;
		}
		dictionary.Clear();
		dictionary = null;
		dictionary2.Clear();
		dictionary2 = null;
		return path;
	}

	private void GetGeomentryPath(IGraphicsPath path, List<double> pathElements, double pathWidth, double pathHeight, RectangleF bounds)
	{
		PointF pointF = Point.Empty;
		double num = 0.0;
		int num2;
		for (num2 = 0; num2 < pathElements.Count; num2++)
		{
			switch ((DocGen.DocIO.DLS.Path2D.Path2DElements)(ushort)pathElements[num2])
			{
			case DocGen.DocIO.DLS.Path2D.Path2DElements.LineTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				PointF pointF2 = new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds));
				path.AddLine(pointF, pointF2);
				pointF = pointF2;
				break;
			}
			case DocGen.DocIO.DLS.Path2D.Path2DElements.MoveTo:
				path.StartFigure();
				num = pathElements[num2 + 1] * 2.0;
				pointF = new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds));
				break;
			case DocGen.DocIO.DLS.Path2D.Path2DElements.QuadBezTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				PointF[] array2 = new PointF[3]
				{
					pointF,
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds)),
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 4], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 5], bounds))
				};
				path.AddBeziers(array2);
				pointF = array2[2];
				break;
			}
			case DocGen.DocIO.DLS.Path2D.Path2DElements.CubicBezTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				PointF[] array = new PointF[4]
				{
					pointF,
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 2], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 3], bounds)),
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 4], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 5], bounds)),
					new PointF(GetGeomentryPathXValue(pathWidth, pathElements[num2 + 6], bounds), GetGeomentryPathYValue(pathHeight, pathElements[num2 + 7], bounds))
				};
				path.AddBeziers(array);
				pointF = array[3];
				break;
			}
			case DocGen.DocIO.DLS.Path2D.Path2DElements.ArcTo:
			{
				num = pathElements[num2 + 1] * 2.0;
				RectangleF rect = default(RectangleF);
				rect.X = bounds.X;
				rect.Y = bounds.Y;
				rect.Width = (float)(pathElements[num2 + 2] / 12700.0) * 2f;
				rect.Height = (float)(pathElements[num2 + 3] / 12700.0) * 2f;
				float startAngle = (float)pathElements[num2 + 4] / 60000f;
				float sweepAngle = (float)pathElements[num2 + 5] / 60000f;
				path.AddArc(rect, startAngle, sweepAngle);
				pointF = path.PathPoints[path.PathPoints.Length - 1];
				break;
			}
			case DocGen.DocIO.DLS.Path2D.Path2DElements.Close:
				path.CloseFigure();
				pointF = Point.Empty;
				num = 0.0;
				break;
			}
			num2 += (int)num + 1;
		}
	}

	private float GetGeomentryPathXValue(double pathWidth, double x, RectangleF bounds)
	{
		if (pathWidth != 0.0)
		{
			double num = x * 100.0 / pathWidth;
			return (float)((double)bounds.Width * num / 100.0) + bounds.X;
		}
		return (float)((double)bounds.X + x / 12700.0);
	}

	private float GetGeomentryPathYValue(double pathHeight, double y, RectangleF bounds)
	{
		if (pathHeight != 0.0)
		{
			double num = y * 100.0 / pathHeight;
			return (float)((double)bounds.Height * num / 100.0) + bounds.Y;
		}
		return (float)((double)bounds.Y + y / 12700.0);
	}

	private void ConvertPathElement(string pathElement, List<double> pathElements, Dictionary<string, string> combinedValues, DocGen.DocIO.DLS.Path2D path, Dictionary<string, float> calculatedValues)
	{
		int result = 0;
		if (int.TryParse(pathElement, out result))
		{
			pathElements.Add(double.Parse(pathElement, CultureInfo.InvariantCulture));
		}
		else if (combinedValues != null && combinedValues.Count > 0 && calculatedValues.Count == 0)
		{
			ShapePath shapePath = new ShapePath(new RectangleF(0f, 0f, (float)path.Width, (float)path.Height), new Dictionary<string, string>());
			Dictionary<string, float> formulaValues = shapePath.GetFormulaValues(AutoShapeType.Unknown, combinedValues, isAdjValue: false);
			foreach (KeyValuePair<string, float> item in formulaValues)
			{
				calculatedValues.Add(item.Key, item.Value);
			}
			shapePath.Close();
			formulaValues.Clear();
			if (!string.IsNullOrEmpty(pathElement) && calculatedValues.ContainsKey(pathElement))
			{
				pathElements.Add(calculatedValues[pathElement]);
			}
		}
		else if (calculatedValues != null && !string.IsNullOrEmpty(pathElement) && calculatedValues.ContainsKey(pathElement))
		{
			pathElements.Add(calculatedValues[pathElement]);
		}
	}

	private IGraphicsPath GetGraphicsPath()
	{
		return WordDocument.RenderHelper.GetGraphicsPath();
	}

	private float GetDegreeValue(float value)
	{
		return value / 60000f;
	}

	private PointF GetXYPosition(float xDifference, float yDifference, float positionRatio)
	{
		float x = _rectBounds.X + _rectBounds.Width * xDifference / positionRatio;
		float y = _rectBounds.Y + _rectBounds.Height * yDifference / positionRatio;
		return new PointF(x, y);
	}

	private Dictionary<string, float> GetPathAdjustValue(AutoShapeType shapeType)
	{
		Dictionary<string, float> formulaValues = GetFormulaValues(AutoShapeType.Unknown, _shapeGuide, isAdjValue: true);
		List<string> list = new List<string>(formulaValues.Keys);
		if (shapeType == AutoShapeType.CircularArrow)
		{
			foreach (string item in list)
			{
				if (item != "adj1" && item != "adj5")
				{
					formulaValues[item] /= 60000f;
				}
			}
		}
		return formulaValues;
	}

	public Dictionary<string, float> ParseShapeFormula(AutoShapeType shapeType)
	{
		return GetFormulaValues(shapeType, GetShapeFormula(shapeType), isAdjValue: false);
	}

	private Dictionary<string, float> GetFormulaValues(AutoShapeType shapeType, Dictionary<string, string> formulaColl, bool isAdjValue)
	{
		if (formulaColl.Count == 0)
		{
			return null;
		}
		Dictionary<string, float> formulaValues = new Dictionary<string, float>();
		foreach (KeyValuePair<string, string> item in formulaColl)
		{
			string[] array = item.Value.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length > 1)
			{
				float[] operandValues = GetOperandValues(shapeType, ref formulaValues, array, isAdjValue);
				float resultValue = GetResultValue(array[0], operandValues);
				formulaValues.Add(item.Key, resultValue);
			}
		}
		return formulaValues;
	}

	private float[] GetOperandValues(AutoShapeType shapeType, ref Dictionary<string, float> formulaValues, string[] splitFormula, bool isAdjValue)
	{
		string[] array = new string[34]
		{
			"3cd4", "3cd8", "5cd8", "7cd8", "b", "cd2", "cd4", "cd8", "h", "hc",
			"hd2", "hd3", "hd4", "hd5", "hd6", "hd8", "l", "ls", "r", "ss",
			"ssd2", "ssd4", "ssd6", "ssd8", "t", "vc", "w", "wd2", "wd3", "wd4",
			"wd5", "wd6", "wd8", "wd10"
		};
		Dictionary<string, float> dictionary = ((_shapeGuide.Count > 0 && !isAdjValue) ? GetPathAdjustValue(shapeType) : GetDefaultPathAdjValues(shapeType));
		float[] array2 = new float[splitFormula.Length - 1];
		int num = 0;
		for (int i = 1; i < splitFormula.Length; i++)
		{
			if (!float.TryParse(splitFormula[i], out array2[num]))
			{
				if (Array.IndexOf(array, splitFormula[i]) > -1)
				{
					array2[num] = GetPresetOperandValue(splitFormula[i]);
				}
				else if (!isAdjValue && dictionary.ContainsKey(splitFormula[i]))
				{
					array2[num] = dictionary[splitFormula[i]];
				}
				else if (formulaValues.ContainsKey(splitFormula[i]))
				{
					array2[num] = formulaValues[splitFormula[i]];
				}
			}
			num++;
		}
		return array2;
	}

	private float GetPresetOperandValue(string operand)
	{
		return operand switch
		{
			"3cd4" => 270f, 
			"3cd8" => 135f, 
			"5cd8" => 225f, 
			"7cd8" => 315f, 
			"b" => _rectBounds.Height, 
			"cd2" => 180f, 
			"cd4" => 90f, 
			"cd8" => 45f, 
			"h" => _rectBounds.Height, 
			"hc" => _rectBounds.Width / 2f, 
			"hd2" => _rectBounds.Height / 2f, 
			"hd3" => _rectBounds.Height / 3f, 
			"hd4" => _rectBounds.Height / 4f, 
			"hd5" => _rectBounds.Height / 5f, 
			"hd6" => _rectBounds.Height / 6f, 
			"hd8" => _rectBounds.Height / 8f, 
			"l" => 0f, 
			"ls" => Math.Max(_rectBounds.Width, _rectBounds.Height), 
			"r" => _rectBounds.Width, 
			"ss" => Math.Min(_rectBounds.Width, _rectBounds.Height), 
			"ssd2" => Math.Min(_rectBounds.Width, _rectBounds.Height) / 2f, 
			"ssd4" => Math.Min(_rectBounds.Width, _rectBounds.Height) / 4f, 
			"ssd6" => Math.Min(_rectBounds.Width, _rectBounds.Height) / 6f, 
			"ssd8" => Math.Min(_rectBounds.Width, _rectBounds.Height) / 8f, 
			"t" => 0f, 
			"vc" => _rectBounds.Height / 2f, 
			"w" => _rectBounds.Width, 
			"wd2" => _rectBounds.Width / 2f, 
			"wd3" => _rectBounds.Width / 3f, 
			"wd4" => _rectBounds.Width / 4f, 
			"wd5" => _rectBounds.Width / 5f, 
			"wd6" => _rectBounds.Width / 6f, 
			"wd8" => _rectBounds.Width / 8f, 
			"wd10" => _rectBounds.Width / 10f, 
			_ => 0f, 
		};
	}

	private float GetResultValue(string formula, float[] operandValues)
	{
		char[] array = new char[4] { '*', '/', '+', '-' };
		float num = operandValues[0];
		if (formula.Length > 1 && Array.IndexOf(array, formula[0]) > -1)
		{
			int num2 = 0;
			for (int i = 0; i < formula.Length; i++)
			{
				num2++;
				switch (formula[i])
				{
				case '*':
					if (operandValues[num2] != 0f)
					{
						num *= operandValues[num2];
					}
					break;
				case '/':
					if (operandValues[num2] != 0f)
					{
						num /= operandValues[num2];
					}
					break;
				case '+':
					num += operandValues[num2];
					break;
				case '-':
					num -= operandValues[num2];
					break;
				}
			}
		}
		else
		{
			switch (formula)
			{
			case "?:":
				num = ((operandValues[0] > 0f) ? operandValues[1] : operandValues[2]);
				break;
			case "abs":
				num = Math.Abs(operandValues[0]);
				break;
			case "at2":
				num = (float)Math.Atan(operandValues[1] / operandValues[0]);
				break;
			case "cat2":
			{
				float num4 = (float)Math.Atan(operandValues[2] / operandValues[1]);
				num = (float)((double)operandValues[0] * Math.Cos(num4));
				break;
			}
			case "cos":
			{
				double num3 = Math.Cos((double)operandValues[1] * Math.PI / 180.0);
				num = (float)((double)operandValues[0] * num3);
				break;
			}
			case "max":
				num = Math.Max(operandValues[0], operandValues[1]);
				break;
			case "min":
				num = Math.Min(operandValues[0], operandValues[1]);
				break;
			case "mod":
				num = (float)Math.Sqrt(Math.Pow(operandValues[0], 2.0) + Math.Pow(operandValues[1], 2.0) + Math.Pow(operandValues[2], 2.0));
				break;
			case "pin":
				num = ((!(operandValues[1] < operandValues[0])) ? ((!(operandValues[1] > operandValues[2])) ? operandValues[1] : operandValues[2]) : operandValues[0]);
				break;
			case "sat2":
			{
				float num4 = (float)Math.Atan(operandValues[2] / operandValues[1]);
				num = (float)((double)operandValues[0] * Math.Sin(num4));
				break;
			}
			case "sin":
			{
				double num3 = Math.Sin((double)operandValues[1] * Math.PI / 180.0);
				num = (float)((double)operandValues[0] * num3);
				break;
			}
			case "sqrt":
				num = (float)Math.Sqrt(operandValues[0]);
				break;
			case "tan":
			{
				double num3 = Math.Tan((double)operandValues[1] * Math.PI / 180.0);
				num = (float)((double)operandValues[0] * num3);
				break;
			}
			case "val":
				num = operandValues[0];
				break;
			}
		}
		return num;
	}

	private Dictionary<string, string> GetShapeFormula(AutoShapeType shapeType)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		switch (shapeType)
		{
		case AutoShapeType.CurvedConnector:
			dictionary.Add("x2", "*/ w adj1 100000");
			dictionary.Add("x1", "+/ l x2 2");
			dictionary.Add("x3", "+/ r x2 2");
			dictionary.Add("y3", "*/ h 3 4");
			break;
		case AutoShapeType.CurvedConnector4:
			dictionary.Add("x2", "*/ w adj1 100000");
			dictionary.Add("x1", "+/ l x2 2");
			dictionary.Add("x3", "+/ r x2 2");
			dictionary.Add("x4", "+/ x2 x3 2");
			dictionary.Add("x5", "+/ x3 r 2");
			dictionary.Add("y4", "*/ h adj2 100000");
			dictionary.Add("y1", "+/ t y4 2");
			dictionary.Add("y2", "+/ t y1 2");
			dictionary.Add("y3", "+/ y1 y4 2");
			dictionary.Add("y5", "+/ b y4 2");
			break;
		case AutoShapeType.CurvedConnector5:
			dictionary.Add("x3", "*/ w adj1 100000");
			dictionary.Add("x6", "*/ w adj3 100000");
			dictionary.Add("x1", "+/ x3 x6 2");
			dictionary.Add("x2", "+/ l x3 2");
			dictionary.Add("x4", "+/ x3 x1 2");
			dictionary.Add("x5", "+/ x6 x1 2");
			dictionary.Add("x7", "+/ x6 r 2");
			dictionary.Add("y4", "*/ h adj2 100000");
			dictionary.Add("y1", "+/ t y4 2");
			dictionary.Add("y2", "+/ t y1 2");
			dictionary.Add("y3", "+/ y1 y4 2");
			dictionary.Add("y5", "+/ b y4 2");
			dictionary.Add("y6", "+/ y5 y4 2");
			dictionary.Add("y7", "+/ y5 b 2");
			break;
		case AutoShapeType.ElbowConnector:
			dictionary.Add("x1", "*/ w adj1 100000");
			break;
		case AutoShapeType.BentConnector4:
			dictionary.Add("x1", "*/ w adj1 100000");
			dictionary.Add("x2", "+/ x1 r 2");
			dictionary.Add("y2", "*/ h adj2 100000");
			dictionary.Add("y1", "+/ t y2 2");
			break;
		case AutoShapeType.BentConnector5:
			dictionary.Add("x1", "*/ w adj1 100000");
			dictionary.Add("x3", "*/ w adj3 100000");
			dictionary.Add("x2", "+/ x1 x3 2");
			dictionary.Add("y2", "*/ h adj2 100000");
			dictionary.Add("y1", "+/ t y2 2");
			dictionary.Add("y3", "+/ b y2 2");
			break;
		case AutoShapeType.RoundedRectangle:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("y2", "+- b 0 x1");
			dictionary.Add("il", "*/ x1 29289 100000");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.SnipSingleCornerRectangle:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "*/ ss a 100000");
			dictionary.Add("x1", "+- r 0 dx1");
			dictionary.Add("it", "*/ dx1 1 2");
			dictionary.Add("ir", "+/ x1 r 2");
			break;
		case AutoShapeType.SnipSameSideCornerRectangle:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("tx1", "*/ ss a1 100000");
			dictionary.Add("tx2", "+- r 0 tx1");
			dictionary.Add("bx1", "*/ ss a2 100000");
			dictionary.Add("bx2", "+- r 0 bx1");
			dictionary.Add("by1", "+- b 0 bx1");
			dictionary.Add("d", "+- tx1 0 bx1");
			dictionary.Add("dx", "?: d tx1 bx1");
			dictionary.Add("il", "*/ dx 1 2");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("it", "*/ tx1 1 2");
			dictionary.Add("ib", "+/ by1 b 2");
			break;
		case AutoShapeType.SnipDiagonalCornerRectangle:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("lx1", "*/ ss a1 100000");
			dictionary.Add("lx2", "+- r 0 lx1");
			dictionary.Add("ly1", "+- b 0 lx1");
			dictionary.Add("rx1", "*/ ss a2 100000");
			dictionary.Add("rx2", "+- r 0 rx1");
			dictionary.Add("ry1", "+- b 0 rx1");
			dictionary.Add("d", "+- lx1 0 rx1");
			dictionary.Add("dx", "?: d lx1 rx1");
			dictionary.Add("il", "*/ dx 1 2");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.SnipAndRoundSingleCornerRectangle:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("x1", "*/ ss a1 100000");
			dictionary.Add("dx2", "*/ ss a2 100000");
			dictionary.Add("x2", "+- r 0 dx2");
			dictionary.Add("il", "*/ x1 29289 100000");
			dictionary.Add("ir", "+/ x2 r 2");
			break;
		case AutoShapeType.RoundSingleCornerRectangle:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "*/ ss a 100000");
			dictionary.Add("x1", "+- r 0 dx1");
			dictionary.Add("idx", "*/ dx1 29289 100000");
			dictionary.Add("ir", "+- r 0 idx");
			break;
		case AutoShapeType.RoundSameSideCornerRectangle:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("tx1", "*/ ss a1 100000");
			dictionary.Add("tx2", "+- r 0 tx1");
			dictionary.Add("bx1", "*/ ss a2 100000");
			dictionary.Add("bx2", "+- r 0 bx1");
			dictionary.Add("by1", "+- b 0 bx1");
			dictionary.Add("d", "+- tx1 0 bx1");
			dictionary.Add("tdx", "*/ tx1 29289 100000");
			dictionary.Add("bdx", "*/ bx1 29289 100000");
			dictionary.Add("il", "?: d tdx bdx");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 bdx");
			break;
		case AutoShapeType.RoundDiagonalCornerRectangle:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("x1", "*/ ss a1 100000");
			dictionary.Add("y1", "+- b 0 x1");
			dictionary.Add("a", "*/ ss a2 100000");
			dictionary.Add("x2", "+- r 0 a");
			dictionary.Add("y2", "+- b 0 a");
			dictionary.Add("dx1", "*/ x1 29289 100000");
			dictionary.Add("dx2", "*/ a 29289 100000");
			dictionary.Add("d", "+- dx1 0 dx2");
			dictionary.Add("dx", "?: d dx1 dx2");
			dictionary.Add("ir", "+- r 0 dx");
			dictionary.Add("ib", "+- b 0 dx");
			break;
		case AutoShapeType.Oval:
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.RightTriangle:
			dictionary.Add("it", "*/ h 7 12");
			dictionary.Add("ir", "*/ w 7 12");
			dictionary.Add("ib", "*/ h 11 12");
			break;
		case AutoShapeType.Diamond:
			dictionary.Add("ir", "*/ w 3 4");
			dictionary.Add("ib", "*/ h 3 4");
			break;
		case AutoShapeType.IsoscelesTriangle:
			dictionary.Add("a", "pin 0 adj 100000");
			dictionary.Add("x1", "*/ w a 200000");
			dictionary.Add("x2", "*/ w a 100000");
			dictionary.Add("x3", "+- x1 wd2 0");
			break;
		case AutoShapeType.Parallelogram:
			dictionary.Add("maxAdj", "*/ 100000 w ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("x1", "*/ ss a 200000");
			dictionary.Add("x2", "*/ ss a 100000");
			dictionary.Add("x6", "+- r 0 x1");
			dictionary.Add("x5", "+- r 0 x2");
			dictionary.Add("x3", "*/ x5 1 2");
			dictionary.Add("x4", "+- r 0 x3");
			dictionary.Add("il1", "*/ wd2 a maxAdj");
			dictionary.Add("q1", "*/ 5 a maxAdj");
			dictionary.Add("q2", "+/ 1 q1 12");
			dictionary.Add("il", "*/ q2 w 1");
			dictionary.Add("it", "*/ q2 h 1");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 it");
			dictionary.Add("q3", "*/ h hc x2");
			dictionary.Add("y1", "pin 0 q3 h");
			dictionary.Add("y2", "+- b 0 y1");
			break;
		case AutoShapeType.Trapezoid:
			dictionary.Add("maxAdj", "*/ 50000 w ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("x1", "*/ ss a 200000");
			dictionary.Add("x2", "*/ ss a 100000");
			dictionary.Add("x3", "+- r 0 x2");
			dictionary.Add("x4", "+- r 0 x1");
			dictionary.Add("il", "*/ wd3 a maxAdj");
			dictionary.Add("it", "*/ hd3 a maxAdj");
			dictionary.Add("ir", "+- r 0 il");
			break;
		case AutoShapeType.RegularPentagon:
			dictionary.Add("swd2", "*/ wd2 hf 100000");
			dictionary.Add("shd2", "*/ hd2 vf 100000");
			dictionary.Add("svc", "*/ vc  vf 100000");
			dictionary.Add("dx1", "cos swd2 18");
			dictionary.Add("dx2", "cos swd2 306");
			dictionary.Add("dy1", "sin shd2 18");
			dictionary.Add("dy2", "sin shd2 306");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "+- svc 0 dy1");
			dictionary.Add("y2", "+- svc 0 dy2");
			dictionary.Add("it", "*/ y1 dx2 dx1");
			break;
		case AutoShapeType.Hexagon:
			dictionary.Add("maxAdj", "*/ 50000 w ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("shd2", "*/ hd2 vf 100000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("dy1", "sin shd2 60");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("q1", "*/ maxAdj -1 2");
			dictionary.Add("q2", "+- a q1 0");
			dictionary.Add("q3", "?: q2 4 2");
			dictionary.Add("q4", "?: q2 3 2");
			dictionary.Add("q5", "?: q2 q1 0");
			dictionary.Add("q6", "+/ a q5 q1");
			dictionary.Add("q7", "*/ q6 q4 -1");
			dictionary.Add("q8", "+- q3 q7 0");
			dictionary.Add("il", "*/ w q8 24");
			dictionary.Add("it", "*/ h q8 24");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 it");
			break;
		case AutoShapeType.Heptagon:
			dictionary.Add("swd2", "*/ wd2 hf 100000");
			dictionary.Add("shd2", "*/ hd2 vf 100000");
			dictionary.Add("svc", "*/ vc  vf 100000");
			dictionary.Add("dx1", "*/ swd2 97493 100000");
			dictionary.Add("dx2", "*/ swd2 78183 100000");
			dictionary.Add("dx3", "*/ swd2 43388 100000");
			dictionary.Add("dy1", "*/ shd2 62349 100000");
			dictionary.Add("dy2", "*/ shd2 22252 100000");
			dictionary.Add("dy3", "*/ shd2 90097 100000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc dx3 0");
			dictionary.Add("x5", "+- hc dx2 0");
			dictionary.Add("x6", "+- hc dx1 0");
			dictionary.Add("y1", "+- svc 0 dy1");
			dictionary.Add("y2", "+- svc dy2 0");
			dictionary.Add("y3", "+- svc dy3 0");
			dictionary.Add("ib", "+- b 0 y1");
			break;
		case AutoShapeType.Octagon:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("y2", "+- b 0 x1");
			dictionary.Add("il", "*/ x1 1 2");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.Decagon:
			dictionary.Add("shd2", "*/ hd2 vf 100000");
			dictionary.Add("dx1", "cos wd2 36");
			dictionary.Add("dx2", "cos wd2 72");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("dy1", "sin shd2 72");
			dictionary.Add("dy2", "sin shd2 36");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y4", "+- vc dy1 0");
			break;
		case AutoShapeType.Dodecagon:
			dictionary.Add("x1", "*/ w 2894 21600");
			dictionary.Add("x2", "*/ w 7906 21600");
			dictionary.Add("x3", "*/ w 13694 21600");
			dictionary.Add("x4", "*/ w 18706 21600");
			dictionary.Add("y1", "*/ h 2894 21600");
			dictionary.Add("y2", "*/ h 7906 21600");
			dictionary.Add("y3", "*/ h 13694 21600");
			dictionary.Add("y4", "*/ h 18706 21600");
			break;
		case AutoShapeType.Pie:
			dictionary.Add("stAng", "pin 0 adj1 360");
			dictionary.Add("enAng", "pin 0 adj2 360");
			dictionary.Add("sw1", "+- enAng 0 stAng");
			dictionary.Add("sw2", "+- sw1 360 0");
			dictionary.Add("swAng", "?: sw1 sw1 sw2");
			dictionary.Add("wt1", "sin wd2 stAng");
			dictionary.Add("ht1", "cos hd2 stAng");
			dictionary.Add("dx1", "cat2 wd2 ht1 wt1");
			dictionary.Add("dy1", "sat2 hd2 ht1 wt1");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc dy1 0");
			dictionary.Add("wt2", "sin wd2 enAng");
			dictionary.Add("ht2", "cos hd2 enAng");
			dictionary.Add("dx2", "cat2 wd2 ht2 wt2");
			dictionary.Add("dy2", "sat2 hd2 ht2 wt2");
			dictionary.Add("x2", "+- hc dx2 0");
			dictionary.Add("y2", "+- vc dy2 0");
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.Chord:
			dictionary.Add("stAng", "pin 0 adj1 360");
			dictionary.Add("enAng", "pin 0 adj2 360");
			dictionary.Add("sw1", "+- enAng 0 stAng");
			dictionary.Add("sw2", "+- sw1 360 0");
			dictionary.Add("swAng", "?: sw1 sw1 sw2");
			dictionary.Add("wt1", "sin wd2 stAng");
			dictionary.Add("ht1", "cos hd2 stAng");
			dictionary.Add("dx1", "cat2 wd2 ht1 wt1");
			dictionary.Add("dy1", "sat2 hd2 ht1 wt1");
			dictionary.Add("wt2", "sin wd2 enAng");
			dictionary.Add("ht2", "cos hd2 enAng");
			dictionary.Add("dx2", "cat2 wd2 ht2 wt2");
			dictionary.Add("dy2", "sat2 hd2 ht2 wt2");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc dy1 0");
			dictionary.Add("x2", "+- hc dx2 0");
			dictionary.Add("y2", "+- vc dy2 0");
			dictionary.Add("x3", "+/ x1 x2 2");
			dictionary.Add("y3", "+/ y1 y2 2");
			dictionary.Add("midAng0", "*/ swAng 1 2");
			dictionary.Add("midAng", "+- stAng midAng0 cd2");
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.Teardrop:
			dictionary.Add("a", "pin 0 adj 200000");
			dictionary.Add("r2", "sqrt 2");
			dictionary.Add("tw", "*/ wd2 r2 1");
			dictionary.Add("th", "*/ hd2 r2 1");
			dictionary.Add("sw", "*/ tw a 100000");
			dictionary.Add("sh", "*/ th a 100000");
			dictionary.Add("dx1", "cos sw 45");
			dictionary.Add("dy1", "sin sh 45");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("x2", "+/ hc x1 2");
			dictionary.Add("y2", "+/ vc y1 2");
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.Frame:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("x1", "*/ ss a1 100000");
			dictionary.Add("x4", "+- r 0 x1");
			dictionary.Add("y4", "+- b 0 x1");
			break;
		case AutoShapeType.HalfFrame:
			dictionary.Add("maxAdj2", "*/ 100000 w ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("x1", "*/ ss a2 100000");
			dictionary.Add("g1", "*/ h x1 w");
			dictionary.Add("g2", "+- h 0 g1");
			dictionary.Add("maxAdj1", "*/ 100000 g2 ss");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("y1", "*/ ss a1 100000");
			dictionary.Add("dx2", "*/ y1 w h");
			dictionary.Add("x2", "+- r 0 dx2");
			dictionary.Add("dy2", "*/ x1 h w");
			dictionary.Add("y2", "+- b 0 dy2");
			dictionary.Add("cx1", "*/ x1 1 2");
			dictionary.Add("cy1", "+/ y2 b 2");
			dictionary.Add("cx2", "+/ x2 r 2");
			dictionary.Add("cy2", "*/ y1 1 2");
			break;
		case AutoShapeType.L_Shape:
			dictionary.Add("maxAdj1", "*/ 100000 h ss");
			dictionary.Add("maxAdj2", "*/ 100000 w ss");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("x1", "*/ ss a2 100000");
			dictionary.Add("dy1", "*/ ss a1 100000");
			dictionary.Add("y1", "+- b 0 dy1");
			dictionary.Add("cx1", "*/ x1 1 2");
			dictionary.Add("cy1", "+/ y1 b 2");
			dictionary.Add("d", "+- w 0 h");
			dictionary.Add("it", "?: d y1 t");
			dictionary.Add("ir", "?: d r x1");
			break;
		case AutoShapeType.DiagonalStripe:
			dictionary.Add("a", "pin 0 adj 100000");
			dictionary.Add("x2", "*/ w a 100000");
			dictionary.Add("x1", "*/ x2 1 2");
			dictionary.Add("x3", "+/ x2 r 2");
			dictionary.Add("y2", "*/ h a 100000");
			dictionary.Add("y1", "*/ y2 1 2");
			dictionary.Add("y3", "+/ y2 b 2");
			break;
		case AutoShapeType.Cross:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("y2", "+- b 0 x1");
			dictionary.Add("d", "+- w 0 h");
			dictionary.Add("il", "?: d l x1");
			dictionary.Add("ir", "?: d r x2");
			dictionary.Add("it", "?: d x1 t");
			dictionary.Add("ib", "?: d y2 b");
			break;
		case AutoShapeType.Plaque:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("y2", "+- b 0 x1");
			dictionary.Add("il", "*/ x1 70711 100000");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.Can:
			dictionary.Add("maxAdj", "*/ 50000 h ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("y1", "*/ ss a 200000");
			dictionary.Add("y2", "+- y1 y1 0");
			dictionary.Add("y3", "+- b 0 y1");
			break;
		case AutoShapeType.Cube:
			dictionary.Add("a", "pin 0 adj 100000");
			dictionary.Add("y1", "*/ ss a 100000");
			dictionary.Add("y4", "+- b 0 y1");
			dictionary.Add("y2", "*/ y4 1 2");
			dictionary.Add("y3", "+/ y1 b 2");
			dictionary.Add("x4", "+- r 0 y1");
			dictionary.Add("x2", "*/ x4 1 2");
			dictionary.Add("x3", "+/ y1 r 2");
			break;
		case AutoShapeType.Bevel:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("y2", "+- b 0 x1");
			break;
		case AutoShapeType.Donut:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dr", "*/ ss a 100000");
			dictionary.Add("iwd2", "+- wd2 0 dr");
			dictionary.Add("ihd2", "+- hd2 0 dr");
			dictionary.Add("idx", "cos wd2 2700000");
			dictionary.Add("idy", "sin hd2 2700000");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.NoSymbol:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dr", "*/ ss a 100000");
			dictionary.Add("iwd2", "+- wd2 0 dr");
			dictionary.Add("ihd2", "+- hd2 0 dr");
			dictionary.Add("ang3", "at2 w h");
			dictionary.Add("ang", "*/ ang3 180 " + Math.PI);
			dictionary.Add("ct", "cos ihd2 ang");
			dictionary.Add("st", "sin iwd2 ang");
			dictionary.Add("m", "mod ct st 0");
			dictionary.Add("n", "*/ iwd2 ihd2 m");
			dictionary.Add("drd2", "*/ dr 1 2");
			dictionary.Add("dang3", "at2 n drd2");
			dictionary.Add("dang", "*/ dang3 180 " + Math.PI);
			dictionary.Add("2dang", "*/ dang 2 1");
			dictionary.Add("swAng", "+- -180 2dang 0");
			dictionary.Add("t4", "at2 w h");
			dictionary.Add("t3", "*/ t4 180 " + Math.PI);
			dictionary.Add("stAng1", "+- t3 0 dang");
			dictionary.Add("stAng2", "+- stAng1 0 cd2");
			dictionary.Add("ct1", "cos ihd2 stAng1");
			dictionary.Add("st1", "sin iwd2 stAng1");
			dictionary.Add("m1", "mod ct1 st1 0");
			dictionary.Add("n1", "*/ iwd2 ihd2 m1");
			dictionary.Add("dx1", "cos n1 stAng1");
			dictionary.Add("dy1", "sin n1 stAng1");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc dy1 0");
			dictionary.Add("x2", "+- hc 0 dx1");
			dictionary.Add("y2", "+- vc 0 dy1");
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.BlockArc:
			dictionary.Add("stAng", "pin 0 adj1 21599999");
			dictionary.Add("istAng", "pin 0 adj2 21599999");
			dictionary.Add("a3", "pin 0 adj3 50000");
			dictionary.Add("sw11", "+- istAng 0 stAng");
			dictionary.Add("sw12", "+- sw11 21600000 0");
			dictionary.Add("swAng", "?: sw11 sw11 sw12");
			dictionary.Add("iswAng", "+- 0 0 swAng");
			dictionary.Add("wt1", "sin wd2 stAng");
			dictionary.Add("ht1", "cos hd2 stAng");
			dictionary.Add("wt3", "sin wd2 istAng");
			dictionary.Add("ht3", "cos hd2 istAng");
			dictionary.Add("dx1", "cat2 wd2 ht1 wt1");
			dictionary.Add("dy1", "sat2 hd2 ht1 wt1");
			dictionary.Add("dx3", "cat2 wd2 ht3 wt3");
			dictionary.Add("dy3", "sat2 hd2 ht3 wt3");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc dy1 0");
			dictionary.Add("x3", "+- hc dx3 0");
			dictionary.Add("y3", "+- vc dy3 0");
			dictionary.Add("dr", "*/ ss a3 100000");
			dictionary.Add("iwd2", "+- wd2 0 dr");
			dictionary.Add("ihd2", "+- hd2 0 dr");
			dictionary.Add("wt2", "sin iwd2 istAng");
			dictionary.Add("ht2", "cos ihd2 istAng");
			dictionary.Add("wt4", "sin iwd2 stAng");
			dictionary.Add("ht4", "cos ihd2 stAng");
			dictionary.Add("dx2", "cat2 iwd2 ht2 wt2");
			dictionary.Add("dy2", "sat2 ihd2 ht2 wt2");
			dictionary.Add("dx4", "cat2 iwd2 ht4 wt4");
			dictionary.Add("dy4", "sat2 ihd2 ht4 wt4");
			dictionary.Add("x2", "+- hc dx2 0");
			dictionary.Add("y2", "+- vc dy2 0");
			dictionary.Add("x4", "+- hc dx4 0");
			dictionary.Add("y4", "+- vc dy4 0");
			dictionary.Add("sw0", "+- 21600000 0 stAng");
			dictionary.Add("da1", "+- swAng 0 sw0");
			dictionary.Add("g1", "max x1 x2");
			dictionary.Add("g2", "max x3 x4");
			dictionary.Add("g3", "max g1 g2");
			dictionary.Add("ir", "?: da1 r g3");
			dictionary.Add("sw1", "+- cd4 0 stAng");
			dictionary.Add("sw2", "+- 27000000 0 stAng");
			dictionary.Add("sw3", "?: sw1 sw1 sw2");
			dictionary.Add("da2", "+- swAng 0 sw3");
			dictionary.Add("g5", "max y1 y2");
			dictionary.Add("g6", "max y3 y4");
			dictionary.Add("g7", "max g5 g6");
			dictionary.Add("ib", "?: da2 b g7");
			dictionary.Add("sw4", "+- cd2 0 stAng");
			dictionary.Add("sw5", "+- 32400000 0 stAng");
			dictionary.Add("sw6", "?: sw4 sw4 sw5");
			dictionary.Add("da3", "+- swAng 0 sw6");
			dictionary.Add("g9", "min x1 x2");
			dictionary.Add("g10", "min x3 x4");
			dictionary.Add("g11", "min g9 g10");
			dictionary.Add("il", "?: da3 l g11");
			dictionary.Add("sw7", "+- 3cd4 0 stAng");
			dictionary.Add("sw8", "+- 37800000 0 stAng");
			dictionary.Add("sw9", "?: sw7 sw7 sw8");
			dictionary.Add("da4", "+- swAng 0 sw9");
			dictionary.Add("g13", "min y1 y2");
			dictionary.Add("g14", "min y3 y4");
			dictionary.Add("g15", "min g13 g14");
			dictionary.Add("it", "?: da4 t g15");
			dictionary.Add("x5", "+/ x1 x4 2");
			dictionary.Add("y5", "+/ y1 y4 2");
			dictionary.Add("x6", "+/ x3 x2 2");
			dictionary.Add("y6", "+/ y3 y2 2");
			dictionary.Add("cang1", "+- stAng 0 cd4");
			dictionary.Add("cang2", "+- istAng cd4 0");
			dictionary.Add("cang3", "+/ cang1 cang2 2");
			break;
		case AutoShapeType.FoldedCorner:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dy2", "*/ ss a 100000");
			dictionary.Add("dy1", "*/ dy2 1 5");
			dictionary.Add("x1", "+- r 0 dy2");
			dictionary.Add("x2", "+- x1 dy1 0");
			dictionary.Add("y2", "+- b 0 dy2");
			dictionary.Add("y1", "+- y2 dy1 0");
			break;
		case AutoShapeType.SmileyFace:
			dictionary.Add("a", "pin -4653 adj 4653");
			dictionary.Add("x1", "*/ w 4969 21699");
			dictionary.Add("x2", "*/ w 6215 21600");
			dictionary.Add("x3", "*/ w 13135 21600");
			dictionary.Add("x4", "*/ w 16640 21600");
			dictionary.Add("y1", "*/ h 7570 21600");
			dictionary.Add("y3", "*/ h 16515 21600");
			dictionary.Add("dy2", "*/ h a 100000");
			dictionary.Add("y2", "+- y3 0 dy2");
			dictionary.Add("y4", "+- y3 dy2 0");
			dictionary.Add("dy3", "*/ h a 50000");
			dictionary.Add("y5", "+- y4 dy3 0");
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			dictionary.Add("wR", "*/ w 1125 21600");
			dictionary.Add("hR", "*/ h 1125 21600");
			break;
		case AutoShapeType.Heart:
			dictionary.Add("dx1", "*/ w 49 48");
			dictionary.Add("dx2", "*/ w 10 48");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "+- t 0 hd3");
			dictionary.Add("il", "*/ w 1 6");
			dictionary.Add("ir", "*/ w 5 6");
			dictionary.Add("ib", "*/ h 2 3");
			break;
		case AutoShapeType.LightningBolt:
			dictionary.Add("x1", "*/ w 5022 21600");
			dictionary.Add("x3", "*/ w 8472 21600");
			dictionary.Add("x4", "*/ w 8757 21600");
			dictionary.Add("x5", "*/ w 10012 21600");
			dictionary.Add("x8", "*/ w 12860 21600");
			dictionary.Add("x9", "*/ w 13917 21600");
			dictionary.Add("x11", "*/ w 16577 21600");
			dictionary.Add("y1", "*/ h 3890 21600");
			dictionary.Add("y2", "*/ h 6080 21600");
			dictionary.Add("y4", "*/ h 7437 21600");
			dictionary.Add("y6", "*/ h 9705 21600");
			dictionary.Add("y7", "*/ h 12007 21600");
			dictionary.Add("y10", "*/ h 14277 21600");
			dictionary.Add("y11", "*/ h 14915 21600");
			break;
		case AutoShapeType.Sun:
			dictionary.Add("a", "pin 12500 adj 46875");
			dictionary.Add("g0", "+- 50000 0 a");
			dictionary.Add("g1", "*/ g0 30274 32768");
			dictionary.Add("g2", "*/ g0 12540 32768");
			dictionary.Add("g3", "+- g1 50000 0");
			dictionary.Add("g4", "+- g2 50000 0");
			dictionary.Add("g5", "+- 50000 0 g1");
			dictionary.Add("g6", "+- 50000 0 g2");
			dictionary.Add("g7", "*/ g0 23170 32768");
			dictionary.Add("g8", "+- 50000 g7 0");
			dictionary.Add("g9", "+- 50000 0 g7");
			dictionary.Add("g10", "*/ g5 3 4");
			dictionary.Add("g11", "*/ g6 3 4");
			dictionary.Add("g12", "+- g10 3662 0");
			dictionary.Add("g13", "+- g11 3662 0");
			dictionary.Add("g14", "+- g11 12500 0");
			dictionary.Add("g15", "+- 100000 0 g10");
			dictionary.Add("g16", "+- 100000 0 g12");
			dictionary.Add("g17", "+- 100000 0 g13");
			dictionary.Add("g18", "+- 100000 0 g14");
			dictionary.Add("ox1", "*/ w 18436 21600");
			dictionary.Add("oy1", "*/ h 3163 21600");
			dictionary.Add("ox2", "*/ w 3163 21600");
			dictionary.Add("oy2", "*/ h 18436 21600");
			dictionary.Add("x8", "*/ w g8 100000");
			dictionary.Add("x9", "*/ w g9 100000");
			dictionary.Add("x10", "*/ w g10 100000");
			dictionary.Add("x12", "*/ w g12 100000");
			dictionary.Add("x13", "*/ w g13 100000");
			dictionary.Add("x14", "*/ w g14 100000");
			dictionary.Add("x15", "*/ w g15 100000");
			dictionary.Add("x16", "*/ w g16 100000");
			dictionary.Add("x17", "*/ w g17 100000");
			dictionary.Add("x18", "*/ w g18 100000");
			dictionary.Add("x19", "*/ w a 100000");
			dictionary.Add("wR", "*/ w g0 100000");
			dictionary.Add("hR", "*/ h g0 100000");
			dictionary.Add("y8", "*/ h g8 100000");
			dictionary.Add("y9", "*/ h g9 100000");
			dictionary.Add("y10", "*/ h g10 100000");
			dictionary.Add("y12", "*/ h g12 100000");
			dictionary.Add("y13", "*/ h g13 100000");
			dictionary.Add("y14", "*/ h g14 100000");
			dictionary.Add("y15", "*/ h g15 100000");
			dictionary.Add("y16", "*/ h g16 100000");
			dictionary.Add("y17", "*/ h g17 100000");
			dictionary.Add("y18", "*/ h g18 100000");
			break;
		case AutoShapeType.Moon:
			dictionary.Add("a", "pin 0 adj 87500");
			dictionary.Add("g0", "*/ ss a 100000");
			dictionary.Add("g0w", "*/ g0 w ss");
			dictionary.Add("g1", "+- ss 0 g0");
			dictionary.Add("g2", "*/ g0 g0 g1");
			dictionary.Add("g3", "*/ ss ss g1");
			dictionary.Add("g4", "*/ g3 2 1");
			dictionary.Add("g5", "+- g4 0 g2");
			dictionary.Add("g6", "+- g5 0 g0");
			dictionary.Add("g6w", "*/ g6 w ss");
			dictionary.Add("g7", "*/ g5 1 2");
			dictionary.Add("g8", "+- g7 0 g0");
			dictionary.Add("dy1", "*/ g8 hd2 ss");
			dictionary.Add("g10h", "+- vc 0 dy1");
			dictionary.Add("g11h", "+- vc dy1 0");
			dictionary.Add("g12", "*/ g0 9598 32768");
			dictionary.Add("g12w", "*/ g12 w ss");
			dictionary.Add("g13", "+- ss 0 g12");
			dictionary.Add("q1", "*/ ss ss 1");
			dictionary.Add("q2", "*/ g13 g13 1");
			dictionary.Add("q3", "+- q1 0 q2");
			dictionary.Add("q4", "sqrt q3");
			dictionary.Add("dy4", "*/ q4 hd2 ss");
			dictionary.Add("g15h", "+- vc 0 dy4");
			dictionary.Add("g16h", "+- vc dy4 0");
			dictionary.Add("g17w", "+- g6w 0 g0w");
			dictionary.Add("g18w", "*/ g17w 1 2");
			dictionary.Add("dx2p", "+- g0w g18w w");
			dictionary.Add("dx2", "*/ dx2p -1 1");
			dictionary.Add("dy2", "*/ hd2 -1 1");
			dictionary.Add("stAng", "at2 dx2 dy2");
			dictionary.Add("stAng1", "*/ stAng 180 " + Math.PI);
			dictionary.Add("enAngp", "at2 dx2 hd2");
			dictionary.Add("enAngp1", "*/ enAngp 180 " + Math.PI);
			dictionary.Add("enAng1", "+- enAngp1 0 360");
			dictionary.Add("swAng1", "+- enAng1 0 stAng1");
			break;
		case AutoShapeType.Cloud:
			dictionary.Add("il", "*/ w 2977 21600");
			dictionary.Add("it", "*/ h 3262 21600");
			dictionary.Add("ir", "*/ w 17087 21600");
			dictionary.Add("ib", "*/ h 17337 21600");
			dictionary.Add("g27", "*/ w 67 21600");
			dictionary.Add("g28", "*/ h 21577 21600");
			dictionary.Add("g29", "*/ w 21582 21600");
			dictionary.Add("g30", "*/ h 1235 21600");
			break;
		case AutoShapeType.Arc:
			dictionary.Add("stAng", "pin 0 adj1 21599999");
			dictionary.Add("enAng", "pin 0 adj2 21599999");
			dictionary.Add("sw11", "+- enAng 0 stAng");
			dictionary.Add("sw12", "+- sw11 21600000 0");
			dictionary.Add("swAng", "?: sw11 sw11 sw12");
			dictionary.Add("wt1", "sin wd2 stAng");
			dictionary.Add("ht1", "cos hd2 stAng");
			dictionary.Add("dx1", "cat2 wd2 ht1 wt1");
			dictionary.Add("dy1", "sat2 hd2 ht1 wt1");
			dictionary.Add("wt2", "sin wd2 enAng");
			dictionary.Add("ht2", "cos hd2 enAng");
			dictionary.Add("dx2", "cat2 wd2 ht2 wt2");
			dictionary.Add("dy2", "sat2 hd2 ht2 wt2");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc dy1 0");
			dictionary.Add("x2", "+- hc dx2 0");
			dictionary.Add("y2", "+- vc dy2 0");
			dictionary.Add("sw0", "+- 21600000 0 stAng");
			dictionary.Add("da1", "+- swAng 0 sw0");
			dictionary.Add("g1", "max x1 x2");
			dictionary.Add("ir", "?: da1 r g1");
			dictionary.Add("sw1", "+- cd4 0 stAng");
			dictionary.Add("sw2", "+- 27000000 0 stAng");
			dictionary.Add("sw3", "?: sw1 sw1 sw2");
			dictionary.Add("da2", "+- swAng 0 sw3");
			dictionary.Add("g5", "max y1 y2");
			dictionary.Add("ib", "?: da2 b g5");
			dictionary.Add("sw4", "+- cd2 0 stAng");
			dictionary.Add("sw5", "+- 32400000 0 stAng");
			dictionary.Add("sw6", "?: sw4 sw4 sw5");
			dictionary.Add("da3", "+- swAng 0 sw6");
			dictionary.Add("g9", "min x1 x2");
			dictionary.Add("il", "?: da3 l g9");
			dictionary.Add("sw7", "+- 3cd4 0 stAng");
			dictionary.Add("sw8", "+- 37800000 0 stAng");
			dictionary.Add("sw9", "?: sw7 sw7 sw8");
			dictionary.Add("da4", "+- swAng 0 sw9");
			dictionary.Add("g13", "min y1 y2");
			dictionary.Add("it", "?: da4 t g13");
			dictionary.Add("cang1", "+- stAng 0 cd4");
			dictionary.Add("cang2", "+- enAng cd4 0");
			dictionary.Add("cang3", "+/ cang1 cang2 2");
			break;
		case AutoShapeType.DoubleBracket:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("y2", "+- b 0 x1");
			dictionary.Add("il", "*/ x1 29289 100000");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.DoubleBrace:
			dictionary.Add("a", "pin 0 adj 25000");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "*/ ss a 50000");
			dictionary.Add("x3", "+- r 0 x2");
			dictionary.Add("x4", "+- r 0 x1");
			dictionary.Add("y2", "+- vc 0 x1");
			dictionary.Add("y3", "+- vc x1 0");
			dictionary.Add("y4", "+- b 0 x1");
			dictionary.Add("it", "*/ x1 29289 100000");
			dictionary.Add("il", "+- x1 it 0");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 it");
			break;
		case AutoShapeType.LeftBracket:
			dictionary.Add("maxAdj", "*/ 50000 h ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("y1", "*/ ss a 100000");
			dictionary.Add("y2", "+- b 0 y1");
			dictionary.Add("dx1", "cos w 2700000");
			dictionary.Add("dy1", "sin y1 2700000");
			dictionary.Add("il", "+- r 0 dx1");
			dictionary.Add("it", "+- y1 0 dy1");
			dictionary.Add("ib", "+- b dy1 y1");
			break;
		case AutoShapeType.RightBracket:
			dictionary.Add("maxAdj", "*/ 50000 h ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("y1", "*/ ss a 100000");
			dictionary.Add("y2", "+- b 0 y1");
			dictionary.Add("dx1", "cos w 2700000");
			dictionary.Add("dy1", "sin y1 2700000");
			dictionary.Add("ir", "+- l dx1 0");
			dictionary.Add("it", "+- y1 0 dy1");
			dictionary.Add("ib", "+- b dy1 y1");
			break;
		case AutoShapeType.LeftBrace:
			dictionary.Add("a2", "pin 0 adj2 100000");
			dictionary.Add("q1", "+- 100000 0 a2");
			dictionary.Add("q2", "min q1 a2");
			dictionary.Add("q3", "*/ q2 1 2");
			dictionary.Add("maxAdj1", "*/ q3 h ss");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("y1", "*/ ss a1 100000");
			dictionary.Add("y3", "*/ h a2 100000");
			dictionary.Add("y4", "+- y3 y1 0");
			dictionary.Add("dx1", "cos wd2 2700000");
			dictionary.Add("dy1", "sin y1 2700000");
			dictionary.Add("il", "+- r 0 dx1");
			dictionary.Add("it", "+- y1 0 dy1");
			dictionary.Add("ib", "+- b dy1 y1");
			break;
		case AutoShapeType.RightBrace:
			dictionary.Add("a2", "pin 0 adj2 100000");
			dictionary.Add("q1", "+- 100000 0 a2");
			dictionary.Add("q2", "min q1 a2");
			dictionary.Add("q3", "*/ q2 1 2");
			dictionary.Add("maxAdj1", "*/ q3 h ss");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("y1", "*/ ss a1 100000");
			dictionary.Add("y3", "*/ h a2 100000");
			dictionary.Add("y2", "+- y3 0 y1");
			dictionary.Add("y4", "+- b 0 y1");
			dictionary.Add("dx1", "cos wd2 2700000");
			dictionary.Add("dy1", "sin y1 2700000");
			dictionary.Add("ir", "+- l dx1 0");
			dictionary.Add("it", "+- y1 0 dy1");
			dictionary.Add("ib", "+- b dy1 y1");
			break;
		case AutoShapeType.RightArrow:
			dictionary.Add("maxAdj2", "*/ 100000 w ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("dx1", "*/ ss a2 100000");
			dictionary.Add("x1", "+- r 0 dx1");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("dx2", "*/ y1 dx1 hd2");
			dictionary.Add("x2", "+- x1 dx2 0");
			break;
		case AutoShapeType.LeftArrow:
			dictionary.Add("maxAdj2", "*/ 100000 w ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("dx2", "*/ ss a2 100000");
			dictionary.Add("x2", "+- l dx2 0");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("dx1", "*/ y1 dx2 hd2");
			dictionary.Add("x1", "+- x2  0 dx1");
			break;
		case AutoShapeType.UpArrow:
			dictionary.Add("maxAdj2", "*/ 100000 h ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("dy2", "*/ ss a2 100000");
			dictionary.Add("y2", "+- t dy2 0");
			dictionary.Add("dx1", "*/ w a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			dictionary.Add("dy1", "*/ x1 dy2 wd2");
			dictionary.Add("y1", "+- y2  0 dy1");
			break;
		case AutoShapeType.DownArrow:
			dictionary.Add("maxAdj2", "*/ 100000 h ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("dy1", "*/ ss a2 100000");
			dictionary.Add("y1", "+- b 0 dy1");
			dictionary.Add("dx1", "*/ w a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			dictionary.Add("dy2", "*/ x1 dy1 wd2");
			dictionary.Add("y2", "+- y1 dy2 0");
			break;
		case AutoShapeType.LeftRightArrow:
			dictionary.Add("maxAdj2", "*/ 50000 w ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("x2", "*/ ss a2 100000");
			dictionary.Add("x3", "+- r 0 x2");
			dictionary.Add("dy", "*/ h a1 200000");
			dictionary.Add("y1", "+- vc 0 dy");
			dictionary.Add("y2", "+- vc dy 0");
			dictionary.Add("dx1", "*/ y1 x2 hd2");
			dictionary.Add("x1", "+- x2 0 dx1");
			dictionary.Add("x4", "+- x3 dx1 0");
			break;
		case AutoShapeType.UpDownArrow:
			dictionary.Add("maxAdj2", "*/ 50000 h ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("y2", "*/ ss a2 100000");
			dictionary.Add("y3", "+- b 0 y2");
			dictionary.Add("dx1", "*/ w a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			dictionary.Add("dy1", "*/ x1 y2 wd2");
			dictionary.Add("y1", "+- y2 0 dy1");
			dictionary.Add("y4", "+- y3 dy1 0");
			break;
		case AutoShapeType.QuadArrow:
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("q1", "+- 100000 0 maxAdj1");
			dictionary.Add("maxAdj3", "*/ q1 1 2");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("x1", "*/ ss a3 100000");
			dictionary.Add("dx2", "*/ ss a2 100000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x5", "+- hc dx2 0");
			dictionary.Add("dx3", "*/ ss a1 200000");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc dx3 0");
			dictionary.Add("x6", "+- r 0 x1");
			dictionary.Add("y2", "+- vc 0 dx2");
			dictionary.Add("y5", "+- vc dx2 0");
			dictionary.Add("y3", "+- vc 0 dx3");
			dictionary.Add("y4", "+- vc dx3 0");
			dictionary.Add("y6", "+- b 0 x1");
			dictionary.Add("il", "*/ dx3 x1 dx2");
			dictionary.Add("ir", "+- r 0 il");
			break;
		case AutoShapeType.LeftRightUpArrow:
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("q1", "+- 100000 0 maxAdj1");
			dictionary.Add("maxAdj3", "*/ q1 1 2");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("x1", "*/ ss a3 100000");
			dictionary.Add("dx2", "*/ ss a2 100000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x5", "+- hc dx2 0");
			dictionary.Add("dx3", "*/ ss a1 200000");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc dx3 0");
			dictionary.Add("x6", "+- r 0 x1");
			dictionary.Add("dy2", "*/ ss a2 50000");
			dictionary.Add("y2", "+- b 0 dy2");
			dictionary.Add("y4", "+- b 0 dx2");
			dictionary.Add("y3", "+- y4 0 dx3");
			dictionary.Add("y5", "+- y4 dx3 0");
			dictionary.Add("il", "*/ dx3 x1 dx2");
			dictionary.Add("ir", "+- r 0 il");
			break;
		case AutoShapeType.BentArrow:
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("a3", "pin 0 adj3 50000");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("aw2", "*/ ss a2 100000");
			dictionary.Add("th2", "*/ th 1 2");
			dictionary.Add("dh2", "+- aw2 0 th2");
			dictionary.Add("ah", "*/ ss a3 100000");
			dictionary.Add("bw", "+- r 0 ah");
			dictionary.Add("bh", "+- b 0 dh2");
			dictionary.Add("bs", "min bw bh");
			dictionary.Add("maxAdj4", "*/ 100000 bs ss");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("bd", "*/ ss a4 100000");
			dictionary.Add("bd3", "+- bd 0 th");
			dictionary.Add("bd2", "max bd3 0");
			dictionary.Add("x3", "+- th bd2 0");
			dictionary.Add("x4", "+- r 0 ah");
			dictionary.Add("y3", "+- dh2 th 0");
			dictionary.Add("y4", "+- y3 dh2 0");
			dictionary.Add("y5", "+- dh2 bd 0");
			dictionary.Add("y6", "+- y3 bd2 0");
			break;
		case AutoShapeType.UTurnArrow:
			dictionary.Add("a2", "pin 0 adj2 25000");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("q2", "*/ a1 ss h");
			dictionary.Add("q3", "+- 100000 0 q2");
			dictionary.Add("maxAdj3", "*/ q3 h ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q1", "+- a3 a1 0");
			dictionary.Add("minAdj5", "*/ q1 ss h");
			dictionary.Add("a5", "pin minAdj5 adj5 100000");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("aw2", "*/ ss a2 100000");
			dictionary.Add("th2", "*/ th 1 2");
			dictionary.Add("dh2", "+- aw2 0 th2");
			dictionary.Add("y5", "*/ h a5 100000");
			dictionary.Add("ah", "*/ ss a3 100000");
			dictionary.Add("y4", "+- y5 0 ah");
			dictionary.Add("x9", "+- r 0 dh2");
			dictionary.Add("bw", "*/ x9 1 2");
			dictionary.Add("bs", "min bw y4");
			dictionary.Add("maxAdj4", "*/ bs 100000 ss");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("bd", "*/ ss a4 100000");
			dictionary.Add("bd3", "+- bd 0 th");
			dictionary.Add("bd2", "max bd3 0");
			dictionary.Add("x3", "+- th bd2 0");
			dictionary.Add("x8", "+- r 0 aw2");
			dictionary.Add("x6", "+- x8 0 aw2");
			dictionary.Add("x7", "+- x6 dh2 0");
			dictionary.Add("x4", "+- x9 0 bd");
			dictionary.Add("x5", "+- x7 0 bd2");
			dictionary.Add("cx", "+/ th x7 2");
			break;
		case AutoShapeType.LeftUpArrow:
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "+- 100000 0 maxAdj1");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("x1", "*/ ss a3 100000");
			dictionary.Add("dx2", "*/ ss a2 50000");
			dictionary.Add("x2", "+- r 0 dx2");
			dictionary.Add("y2", "+- b 0 dx2");
			dictionary.Add("dx4", "*/ ss a2 100000");
			dictionary.Add("x4", "+- r 0 dx4");
			dictionary.Add("y4", "+- b 0 dx4");
			dictionary.Add("dx3", "*/ ss a1 200000");
			dictionary.Add("x3", "+- x4 0 dx3");
			dictionary.Add("x5", "+- x4 dx3 0");
			dictionary.Add("y3", "+- y4 0 dx3");
			dictionary.Add("y5", "+- y4 dx3 0");
			dictionary.Add("il", "*/ dx3 x1 dx4");
			dictionary.Add("cx1", "+/ x1 x5 2");
			dictionary.Add("cy1", "+/ x1 y5 2");
			break;
		case AutoShapeType.BentUpArrow:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("a3", "pin 0 adj3 50000");
			dictionary.Add("y1", "*/ ss a3 100000");
			dictionary.Add("dx1", "*/ ss a2 50000");
			dictionary.Add("x1", "+- r 0 dx1");
			dictionary.Add("dx3", "*/ ss a2 100000");
			dictionary.Add("x3", "+- r 0 dx3");
			dictionary.Add("dx2", "*/ ss a1 200000");
			dictionary.Add("x2", "+- x3 0 dx2");
			dictionary.Add("x4", "+- x3 dx2 0");
			dictionary.Add("dy2", "*/ ss a1 100000");
			dictionary.Add("y2", "+- b 0 dy2");
			dictionary.Add("x0", "*/ x4 1 2");
			dictionary.Add("y3", "+/ y2 b 2");
			dictionary.Add("y15", "+/ y1 b 2");
			break;
		case AutoShapeType.CurvedRightArrow:
			dictionary.Add("maxAdj2", "*/ 50000 h ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("a1", "pin 0 adj1 a2");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("aw", "*/ ss a2 100000");
			dictionary.Add("q1", "+/ th aw 4");
			dictionary.Add("hR", "+- hd2 0 q1");
			dictionary.Add("q7", "*/ hR 2 1");
			dictionary.Add("q8", "*/ q7 q7 1");
			dictionary.Add("q9", "*/ th th 1");
			dictionary.Add("q10", "+- q8 0 q9");
			dictionary.Add("q11", "sqrt q10");
			dictionary.Add("idx", "*/ q11 w q7");
			dictionary.Add("maxAdj3", "*/ 100000 idx ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("ah", "*/ ss a3 100000");
			dictionary.Add("y3", "+- hR th 0");
			dictionary.Add("q2", "*/ w w 1");
			dictionary.Add("q3", "*/ ah ah 1");
			dictionary.Add("q4", "+- q2 0 q3");
			dictionary.Add("q5", "sqrt q4");
			dictionary.Add("dy", "*/ q5 hR w");
			dictionary.Add("y5", "+- hR dy 0");
			dictionary.Add("y7", "+- y3 dy 0");
			dictionary.Add("q6", "+- aw 0 th");
			dictionary.Add("dh", "*/ q6 1 2");
			dictionary.Add("y4", "+- y5 0 dh");
			dictionary.Add("y8", "+- y7 dh 0");
			dictionary.Add("aw2", "*/ aw 1 2");
			dictionary.Add("y6", "+- b 0 aw2");
			dictionary.Add("x1", "+- r 0 ah");
			dictionary.Add("swAng0", "at2 ah dy");
			dictionary.Add("swAng", "*/ swAng0 180 " + Math.PI);
			dictionary.Add("stAng", "+- cd2 0 swAng");
			dictionary.Add("mswAng", "+- 0 0 swAng");
			dictionary.Add("ix", "+- r 0 idx");
			dictionary.Add("iy", "+/ hR y3 2");
			dictionary.Add("q12", "*/ th 1 2");
			dictionary.Add("dang0", "at2 idx q12");
			dictionary.Add("dang2", "*/ dang0 180 " + Math.PI);
			dictionary.Add("swAng2", "+- dang2 0 cd4");
			dictionary.Add("swAng3", "+- cd4 dang2 0");
			dictionary.Add("stAng3", "+- cd2 0 dang2");
			break;
		case AutoShapeType.CurvedLeftArrow:
			dictionary.Add("maxAdj2", "*/ 50000 h ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("a1", "pin 0 adj1 a2");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("aw", "*/ ss a2 100000");
			dictionary.Add("q1", "+/ th aw 4");
			dictionary.Add("hR", "+- hd2 0 q1");
			dictionary.Add("q7", "*/ hR 2 1");
			dictionary.Add("q8", "*/ q7 q7 1");
			dictionary.Add("q9", "*/ th th 1");
			dictionary.Add("q10", "+- q8 0 q9");
			dictionary.Add("q11", "sqrt q10");
			dictionary.Add("idx", "*/ q11 w q7");
			dictionary.Add("maxAdj3", "*/ 100000 idx ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("ah", "*/ ss a3 100000");
			dictionary.Add("y3", "+- hR th 0");
			dictionary.Add("q2", "*/ w w 1");
			dictionary.Add("q3", "*/ ah ah 1");
			dictionary.Add("q4", "+- q2 0 q3");
			dictionary.Add("q5", "sqrt q4");
			dictionary.Add("dy", "*/ q5 hR w");
			dictionary.Add("y5", "+- hR dy 0");
			dictionary.Add("y7", "+- y3 dy 0");
			dictionary.Add("q6", "+- aw 0 th");
			dictionary.Add("dh", "*/ q6 1 2");
			dictionary.Add("y4", "+- y5 0 dh");
			dictionary.Add("y8", "+- y7 dh 0");
			dictionary.Add("aw2", "*/ aw 1 2");
			dictionary.Add("y6", "+- b 0 aw2");
			dictionary.Add("x1", "+- l ah 0");
			dictionary.Add("swAng1", "at2 ah dy");
			dictionary.Add("swAng", "*/ swAng1 180 " + Math.PI);
			dictionary.Add("mswAng", "+- 0 0 swAng");
			dictionary.Add("ix", "+- l idx 0");
			dictionary.Add("iy", "+/ hR y3 2");
			dictionary.Add("q12", "*/ th 1 2");
			dictionary.Add("dang3", "at2 idx q12");
			dictionary.Add("dang2", "*/ dang3 180 " + Math.PI);
			dictionary.Add("swAng2", "+- dang2 0 swAng");
			dictionary.Add("swAng3", "+- swAng dang2 0");
			dictionary.Add("stAng3", "+- 0 0 dang2");
			break;
		case AutoShapeType.CurvedUpArrow:
			dictionary.Add("maxAdj2", "*/ 50000 w ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("aw", "*/ ss a2 100000");
			dictionary.Add("q1", "+/ th aw 4");
			dictionary.Add("wR", "+- wd2 0 q1");
			dictionary.Add("q7", "*/ wR 2 1");
			dictionary.Add("q8", "*/ q7 q7 1");
			dictionary.Add("q9", "*/ th th 1");
			dictionary.Add("q10", "+- q8 0 q9");
			dictionary.Add("q11", "sqrt q10");
			dictionary.Add("idy", "*/ q11 h q7");
			dictionary.Add("maxAdj3", "*/ 100000 idy ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("ah", "*/ ss adj3 100000");
			dictionary.Add("x3", "+- wR th 0");
			dictionary.Add("q2", "*/ h h 1");
			dictionary.Add("q3", "*/ ah ah 1");
			dictionary.Add("q4", "+- q2 0 q3");
			dictionary.Add("q5", "sqrt q4");
			dictionary.Add("dx", "*/ q5 wR h");
			dictionary.Add("x5", "+- wR dx 0");
			dictionary.Add("x7", "+- x3 dx 0");
			dictionary.Add("q6", "+- aw 0 th");
			dictionary.Add("dh", "*/ q6 1 2");
			dictionary.Add("x4", "+- x5 0 dh");
			dictionary.Add("x8", "+- x7 dh 0");
			dictionary.Add("aw2", "*/ aw 1 2");
			dictionary.Add("x6", "+- r 0 aw2");
			dictionary.Add("y1", "+- t ah 0");
			dictionary.Add("swAng0", "at2 ah dx");
			dictionary.Add("swAng", "*/ swAng0 180 " + Math.PI);
			dictionary.Add("mswAng", "+- 0 0 swAng");
			dictionary.Add("iy", "+- t idy 0");
			dictionary.Add("ix", "+/ wR x3 2");
			dictionary.Add("q12", "*/ th 1 2");
			dictionary.Add("dang0", "at2 idy q12");
			dictionary.Add("dang2", "*/ dang0 180 " + Math.PI);
			dictionary.Add("swAng2", "+- dang2 0 swAng");
			dictionary.Add("mswAng2", "+- 0 0 swAng2");
			dictionary.Add("stAng3", "+- cd4 0 swAng");
			dictionary.Add("swAng3", "+- swAng dang2 0");
			dictionary.Add("stAng2", "+- cd4 0 dang2");
			break;
		case AutoShapeType.CurvedDownArrow:
			dictionary.Add("maxAdj2", "*/ 50000 w ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("aw", "*/ ss a2 100000");
			dictionary.Add("q1", "+/ th aw 4");
			dictionary.Add("wR", "+- wd2 0 q1");
			dictionary.Add("q7", "*/ wR 2 1");
			dictionary.Add("q8", "*/ q7 q7 1");
			dictionary.Add("q9", "*/ th th 1");
			dictionary.Add("q10", "+- q8 0 q9");
			dictionary.Add("q11", "sqrt q10");
			dictionary.Add("idy", "*/ q11 h q7");
			dictionary.Add("maxAdj3", "*/ 100000 idy ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("ah", "*/ ss adj3 100000");
			dictionary.Add("x3", "+- wR th 0");
			dictionary.Add("q2", "*/ h h 1");
			dictionary.Add("q3", "*/ ah ah 1");
			dictionary.Add("q4", "+- q2 0 q3");
			dictionary.Add("q5", "sqrt q4");
			dictionary.Add("dx", "*/ q5 wR h");
			dictionary.Add("x5", "+- wR dx 0");
			dictionary.Add("x7", "+- x3 dx 0");
			dictionary.Add("q6", "+- aw 0 th");
			dictionary.Add("dh", "*/ q6 1 2");
			dictionary.Add("x4", "+- x5 0 dh");
			dictionary.Add("x8", "+- x7 dh 0");
			dictionary.Add("aw2", "*/ aw 1 2");
			dictionary.Add("x6", "+- r 0 aw2");
			dictionary.Add("y1", "+- b 0 ah");
			dictionary.Add("swAng0", "at2 ah dx");
			dictionary.Add("swAng", "*/ swAng0 180 " + Math.PI);
			dictionary.Add("mswAng", "+- 0 0 swAng");
			dictionary.Add("iy", "+- b 0 idy");
			dictionary.Add("ix", "+/ wR x3 2");
			dictionary.Add("q12", "*/ th 1 2");
			dictionary.Add("dang0", "at2 idy q12");
			dictionary.Add("dang2", "*/ dang0 180 " + Math.PI);
			dictionary.Add("stAng", "+- 3cd4 swAng 0");
			dictionary.Add("stAng2", "+- 3cd4 0 dang2");
			dictionary.Add("swAng2", "+- dang2 0 cd4");
			dictionary.Add("swAng3", "+- cd4 dang2 0");
			break;
		case AutoShapeType.StripedRightArrow:
			dictionary.Add("maxAdj2", "*/ 84375 w ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("x4", "*/ ss 5 32");
			dictionary.Add("dx5", "*/ ss a2 100000");
			dictionary.Add("x5", "+- r 0 dx5");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("dx6", "*/ dy1 dx5 hd2");
			dictionary.Add("x6", "+- r 0 dx6");
			break;
		case AutoShapeType.NotchedRightArrow:
			dictionary.Add("maxAdj2", "*/ 100000 w ss");
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("dx2", "*/ ss a2 100000");
			dictionary.Add("x2", "+- r 0 dx2");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("x1", "*/ dy1 dx2 hd2");
			dictionary.Add("x3", "+- r 0 x1");
			break;
		case AutoShapeType.Pentagon:
			dictionary.Add("maxAdj", "*/ 100000 w ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("dx1", "*/ ss a 100000");
			dictionary.Add("x1", "+- r 0 dx1");
			dictionary.Add("ir", "+/ x1 r 2");
			dictionary.Add("x2", "*/ x1 1 2");
			break;
		case AutoShapeType.Chevron:
			dictionary.Add("maxAdj", "*/ 100000 w ss");
			dictionary.Add("a", "pin 0 adj maxAdj");
			dictionary.Add("x1", "*/ ss a 100000");
			dictionary.Add("x2", "+- r 0 x1");
			dictionary.Add("x3", "*/ x2 1 2");
			dictionary.Add("dx", "+- x2 0 x1");
			dictionary.Add("il", "?: dx x1 l");
			dictionary.Add("ir", "?: dx x2 r");
			break;
		case AutoShapeType.LeftArrowCallout:
			dictionary.Add("maxAdj2", "*/ 50000 h ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "*/ 100000 w ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 ss w");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("dy1", "*/ ss a2 100000");
			dictionary.Add("dy2", "*/ ss a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y4", "+- vc dy1 0");
			dictionary.Add("x1", "*/ ss a3 100000");
			dictionary.Add("dx2", "*/ w a4 100000");
			dictionary.Add("x2", "+- r 0 dx2");
			dictionary.Add("x3", "+/ x2 r 2");
			break;
		case AutoShapeType.RightArrowCallout:
			dictionary.Add("maxAdj2", "*/ 50000 h ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "*/ 100000 w ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 ss w");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("dy1", "*/ ss a2 100000");
			dictionary.Add("dy2", "*/ ss a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y4", "+- vc dy1 0");
			dictionary.Add("dx3", "*/ ss a3 100000");
			dictionary.Add("x3", "+- r 0 dx3");
			dictionary.Add("x2", "*/ w a4 100000");
			dictionary.Add("x1", "*/ x2 1 2");
			break;
		case AutoShapeType.DownArrowCallout:
			dictionary.Add("maxAdj2", "*/ 50000 w ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "*/ 100000 h ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 ss h");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("dx1", "*/ ss a2 100000");
			dictionary.Add("dx2", "*/ ss a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("dy3", "*/ ss a3 100000");
			dictionary.Add("y3", "+- b 0 dy3");
			dictionary.Add("y2", "*/ h a4 100000");
			dictionary.Add("y1", "*/ y2 1 2");
			break;
		case AutoShapeType.UpArrowCallout:
			dictionary.Add("maxAdj2", "*/ 50000 w ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "*/ 100000 h ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 ss h");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("dx1", "*/ ss a2 100000");
			dictionary.Add("dx2", "*/ ss a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "*/ ss a3 100000");
			dictionary.Add("dy2", "*/ h a4 100000");
			dictionary.Add("y2", "+- b 0 dy2");
			dictionary.Add("y3", "+/ y2 b 2");
			break;
		case AutoShapeType.LeftRightArrowCallout:
			dictionary.Add("maxAdj2", "*/ 50000 h ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "*/ 50000 w ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 ss wd2");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("dy1", "*/ ss a2 100000");
			dictionary.Add("dy2", "*/ ss a1 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y4", "+- vc dy1 0");
			dictionary.Add("x1", "*/ ss a3 100000");
			dictionary.Add("x4", "+- r 0 x1");
			dictionary.Add("dx2", "*/ w a4 200000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			break;
		case AutoShapeType.UpDownArrowCallout:
			dictionary.Add("maxAdj2", "*/ 50000 w ss");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "*/ 50000 h ss");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 ss hd2");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin 0 adj4 maxAdj4");
			dictionary.Add("dx1", "*/ ss a2 100000");
			dictionary.Add("dx2", "*/ ss a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "*/ ss a3 100000");
			dictionary.Add("y4", "+- b 0 y1");
			dictionary.Add("dy2", "*/ h a4 200000");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			break;
		case AutoShapeType.QuadArrowCallout:
			dictionary.Add("a2", "pin 0 adj2 50000");
			dictionary.Add("maxAdj1", "*/ a2 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("maxAdj3", "+- 50000 0 a2");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("q2", "*/ a3 2 1");
			dictionary.Add("maxAdj4", "+- 100000 0 q2");
			dictionary.Add("a4", "pin a1 adj4 maxAdj4");
			dictionary.Add("dx2", "*/ ss a2 100000");
			dictionary.Add("dx3", "*/ ss a1 200000");
			dictionary.Add("ah", "*/ ss a3 100000");
			dictionary.Add("dx1", "*/ w a4 200000");
			dictionary.Add("dy1", "*/ h a4 200000");
			dictionary.Add("x8", "+- r 0 ah");
			dictionary.Add("x2", "+- hc 0 dx1");
			dictionary.Add("x7", "+- hc dx1 0");
			dictionary.Add("x3", "+- hc 0 dx2");
			dictionary.Add("x6", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc 0 dx3");
			dictionary.Add("x5", "+- hc dx3 0");
			dictionary.Add("y8", "+- b 0 ah");
			dictionary.Add("y2", "+- vc 0 dy1");
			dictionary.Add("y7", "+- vc dy1 0");
			dictionary.Add("y3", "+- vc 0 dx2");
			dictionary.Add("y6", "+- vc dx2 0");
			dictionary.Add("y4", "+- vc 0 dx3");
			dictionary.Add("y5", "+- vc dx3 0");
			break;
		case AutoShapeType.CircularArrow:
			dictionary.Add("a5", "pin 0 adj5 25000");
			dictionary.Add("maxAdj1", "*/ a5 2 1");
			dictionary.Add("a1", "pin 0 adj1 maxAdj1");
			dictionary.Add("enAng", "pin 1 adj3 360");
			dictionary.Add("stAng", "pin 0 adj4 360");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("thh", "*/ ss a5 100000");
			dictionary.Add("th2", "*/ th 1 2");
			dictionary.Add("rw1", "+- wd2 th2 thh");
			dictionary.Add("rh1", "+- hd2 th2 thh");
			dictionary.Add("rw2", "+- rw1 0 th");
			dictionary.Add("rh2", "+- rh1 0 th");
			dictionary.Add("rw3", "+- rw2 th2 0");
			dictionary.Add("rh3", "+- rh2 th2 0");
			dictionary.Add("wtH", "sin rw3 enAng");
			dictionary.Add("htH", "cos rh3 enAng");
			dictionary.Add("dxH", "cat2 rw3 htH wtH");
			dictionary.Add("dyH", "sat2 rh3 htH wtH");
			dictionary.Add("xH", "+- hc dxH 0");
			dictionary.Add("yH", "+- vc dyH 0");
			dictionary.Add("rI", "min rw2 rh2");
			dictionary.Add("u1", "*/ dxH dxH 1");
			dictionary.Add("u2", "*/ dyH dyH 1");
			dictionary.Add("u3", "*/ rI rI 1");
			dictionary.Add("u4", "+- u1 0 u3");
			dictionary.Add("u5", "+- u2 0 u3");
			dictionary.Add("u6", "*/ u4 u5 u1");
			dictionary.Add("u7", "*/ u6 1 u2");
			dictionary.Add("u8", "+- 1 0 u7");
			dictionary.Add("u9", "sqrt u8");
			dictionary.Add("u10", "*/ u4 1 dxH");
			dictionary.Add("u11", "*/ u10 1 dyH");
			dictionary.Add("u12", "+/ 1 u9 u11");
			dictionary.Add("u0", "at2 1 u12");
			dictionary.Add("u13", "*/ u0 180 " + Math.PI);
			dictionary.Add("u14", "+- u13 360 0");
			dictionary.Add("u15", "?: u13 u13 u14");
			dictionary.Add("u16", "+- u15 0 enAng");
			dictionary.Add("u17", "+- u16 360 0");
			dictionary.Add("u18", "?: u16 u16 u17");
			dictionary.Add("u19", "+- u18 0 cd2");
			dictionary.Add("u20", "+- u18 0 360");
			dictionary.Add("u21", "?: u19 u20 u18");
			dictionary.Add("maxAng", "abs u21");
			dictionary.Add("aAng", "pin 0 adj2 maxAng");
			dictionary.Add("ptAng", "+- enAng aAng 0");
			dictionary.Add("wtA", "sin rw3 ptAng");
			dictionary.Add("htA", "cos rh3 ptAng");
			dictionary.Add("dxA", "cat2 rw3 htA wtA");
			dictionary.Add("dyA", "sat2 rh3 htA wtA");
			dictionary.Add("xA", "+- hc dxA 0");
			dictionary.Add("yA", "+- vc dyA 0");
			dictionary.Add("wtE", "sin rw1 stAng");
			dictionary.Add("htE", "cos rh1 stAng");
			dictionary.Add("dxE", "cat2 rw1 htE wtE");
			dictionary.Add("dyE", "sat2 rh1 htE wtE");
			dictionary.Add("xE", "+- hc dxE 0");
			dictionary.Add("yE", "+- vc dyE 0");
			dictionary.Add("dxG", "cos thh ptAng");
			dictionary.Add("dyG", "sin thh ptAng");
			dictionary.Add("xG", "+- xH dxG 0");
			dictionary.Add("yG", "+- yH dyG 0");
			dictionary.Add("dxB", "cos thh ptAng");
			dictionary.Add("dyB", "sin thh ptAng");
			dictionary.Add("xB", "+- xH 0 dxB 0");
			dictionary.Add("yB", "+- yH 0 dyB 0");
			dictionary.Add("sx1", "+- xB 0 hc");
			dictionary.Add("sy1", "+- yB 0 vc");
			dictionary.Add("sx2", "+- xG 0 hc");
			dictionary.Add("sy2", "+- yG 0 vc");
			dictionary.Add("rO", "min rw1 rh1");
			dictionary.Add("x1O", "*/ sx1 rO rw1");
			dictionary.Add("y1O", "*/ sy1 rO rh1");
			dictionary.Add("x2O", "*/ sx2 rO rw1");
			dictionary.Add("y2O", "*/ sy2 rO rh1");
			dictionary.Add("dxO", "+- x2O 0 x1O");
			dictionary.Add("dyO", "+- y2O 0 y1O");
			dictionary.Add("dO", "mod dxO dyO 0");
			dictionary.Add("q1", "*/ x1O y2O 1");
			dictionary.Add("q2", "*/ x2O y1O 1");
			dictionary.Add("DO", "+- q1 0 q2");
			dictionary.Add("q3", "*/ rO rO 1");
			dictionary.Add("q4", "*/ dO dO 1");
			dictionary.Add("q5", "*/ q3 q4 1");
			dictionary.Add("q6", "*/ DO DO 1");
			dictionary.Add("q7", "+- q5 0 q6");
			dictionary.Add("q8", "max q7 0");
			dictionary.Add("sdelO", "sqrt q8");
			dictionary.Add("ndyO", "*/ dyO -1 1");
			dictionary.Add("sdyO", "?: ndyO -1 1");
			dictionary.Add("q9", "*/ sdyO dxO 1");
			dictionary.Add("q10", "*/ q9 sdelO 1");
			dictionary.Add("q11", "*/ DO dyO 1");
			dictionary.Add("dxF1", "+/ q11 q10 q4");
			dictionary.Add("q12", "+- q11 0 q10");
			dictionary.Add("dxF2", "*/ q12 1 q4");
			dictionary.Add("adyO", "abs dyO");
			dictionary.Add("q13", "*/ adyO sdelO 1");
			dictionary.Add("q14", "*/ DO dxO -1");
			dictionary.Add("dyF1", "+/ q14 q13 q4");
			dictionary.Add("q15", "+- q14 0 q13");
			dictionary.Add("dyF2", "*/ q15 1 q4");
			dictionary.Add("q16", "+- x2O 0 dxF1");
			dictionary.Add("q17", "+- x2O 0 dxF2");
			dictionary.Add("q18", "+- y2O 0 dyF1");
			dictionary.Add("q19", "+- y2O 0 dyF2");
			dictionary.Add("q20", "mod q16 q18 0");
			dictionary.Add("q21", "mod q17 q19 0");
			dictionary.Add("q22", "+- q21 0 q20");
			dictionary.Add("dxF", "?: q22 dxF1 dxF2");
			dictionary.Add("dyF", "?: q22 dyF1 dyF2");
			dictionary.Add("sdxF", "*/ dxF rw1 rO");
			dictionary.Add("sdyF", "*/ dyF rh1 rO");
			dictionary.Add("xF", "+- hc sdxF 0");
			dictionary.Add("yF", "+- vc sdyF 0");
			dictionary.Add("x1I", "*/ sx1 rI rw2");
			dictionary.Add("y1I", "*/ sy1 rI rh2");
			dictionary.Add("x2I", "*/ sx2 rI rw2");
			dictionary.Add("y2I", "*/ sy2 rI rh2");
			dictionary.Add("dxI1", "+- x2I 0 x1I");
			dictionary.Add("dyI1", "+- y2I 0 y1I");
			dictionary.Add("dI", "mod dxI1 dyI1 0");
			dictionary.Add("v1", "*/ x1I y2I 1");
			dictionary.Add("v2", "*/ x2I y1I 1");
			dictionary.Add("DI", "+- v1 0 v2");
			dictionary.Add("v3", "*/ rI rI 1");
			dictionary.Add("v4", "*/ dI dI 1");
			dictionary.Add("v5", "*/ v3 v4 1");
			dictionary.Add("v6", "*/ DI DI 1");
			dictionary.Add("v7", "+- v5 0 v6");
			dictionary.Add("v8", "max v7 0");
			dictionary.Add("sdelI", "sqrt v8");
			dictionary.Add("v9", "*/ sdyO dxI1 1");
			dictionary.Add("v10", "*/ v9 sdelI 1");
			dictionary.Add("v11", "*/ DI dyI1 1");
			dictionary.Add("dxC1", "+/ v11 v10 v4");
			dictionary.Add("v12", "+- v11 0 v10");
			dictionary.Add("dxC2", "*/ v12 1 v4");
			dictionary.Add("adyI", "abs dyI1");
			dictionary.Add("v13", "*/ adyI sdelI 1");
			dictionary.Add("v14", "*/ DI dxI1 -1");
			dictionary.Add("dyC1", "+/ v14 v13 v4");
			dictionary.Add("v15", "+- v14 0 v13");
			dictionary.Add("dyC2", "*/ v15 1 v4");
			dictionary.Add("v16", "+- x1I 0 dxC1");
			dictionary.Add("v17", "+- x1I 0 dxC2");
			dictionary.Add("v18", "+- y1I 0 dyC1");
			dictionary.Add("v19", "+- y1I 0 dyC2");
			dictionary.Add("v20", "mod v16 v18 0");
			dictionary.Add("v21", "mod v17 v19 0");
			dictionary.Add("v22", "+- v21 0 v20");
			dictionary.Add("dxC", "?: v22 dxC1 dxC2");
			dictionary.Add("dyC", "?: v22 dyC1 dyC2");
			dictionary.Add("sdxC", "*/ dxC rw2 rI");
			dictionary.Add("sdyC", "*/ dyC rh2 rI");
			dictionary.Add("xC", "+- hc sdxC 0");
			dictionary.Add("yC", "+- vc sdyC 0");
			dictionary.Add("ist00", "at2 sdxC sdyC");
			dictionary.Add("ist0", "*/ ist00 180 " + Math.PI);
			dictionary.Add("ist1", "+- ist0 360 0");
			dictionary.Add("istAng", "?: ist0 ist0 ist1");
			dictionary.Add("isw1", "+- stAng 0 istAng");
			dictionary.Add("isw2", "+- isw1 0 360");
			dictionary.Add("iswAng", "?: isw1 isw2 isw1");
			dictionary.Add("p1", "+- xF 0 xC");
			dictionary.Add("p2", "+- yF 0 yC");
			dictionary.Add("p3", "mod p1 p2 0");
			dictionary.Add("p4", "*/ p3 1 2");
			dictionary.Add("p5", "+- p4 0 thh");
			dictionary.Add("xGp", "?: p5 xF xG");
			dictionary.Add("yGp", "?: p5 yF yG");
			dictionary.Add("xBp", "?: p5 xC xB");
			dictionary.Add("yBp", "?: p5 yC yB");
			dictionary.Add("en00", "at2 sdxF sdyF");
			dictionary.Add("en0", "*/ en00 180 " + Math.PI);
			dictionary.Add("en1", "+- en0 360 0");
			dictionary.Add("en2", "?: en0 en0 en1");
			dictionary.Add("sw0", "+- en2 0 stAng");
			dictionary.Add("sw1", "+- sw0 360 0");
			dictionary.Add("swAng", "?: sw0 sw0 sw1");
			dictionary.Add("wtI", "sin rw3 stAng");
			dictionary.Add("htI", "cos rh3 stAng");
			dictionary.Add("dxI", "cat2 rw3 htI wtI");
			dictionary.Add("dyI", "sat2 rh3 htI wtI");
			dictionary.Add("xI", "+- hc dxI 0");
			dictionary.Add("yI", "+- vc dyI 0");
			dictionary.Add("aI", "+- stAng 0 cd4");
			dictionary.Add("aA", "+- ptAng cd4 0");
			dictionary.Add("aB", "+- ptAng cd2 0");
			dictionary.Add("idx", "cos rw1 45");
			dictionary.Add("idy", "sin rh1 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.MathPlus:
			dictionary.Add("a1", "pin 0 adj1 73490");
			dictionary.Add("dx1", "*/ w 73490 200000");
			dictionary.Add("dy1", "*/ h 73490 200000");
			dictionary.Add("dx2", "*/ ss a1 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dx2");
			dictionary.Add("y3", "+- vc dx2 0");
			dictionary.Add("y4", "+- vc dy1 0");
			break;
		case AutoShapeType.MathMinus:
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("dx1", "*/ w 73490 200000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			break;
		case AutoShapeType.MathMultiply:
			dictionary.Add("a1", "pin 0 adj1 51965");
			dictionary.Add("th", "*/ ss a1 100000");
			dictionary.Add("a0", "at2 w h");
			dictionary.Add("a", "*/ a0 180 " + Math.PI);
			dictionary.Add("sa", "sin 1 a");
			dictionary.Add("ca", "cos 1 a");
			dictionary.Add("ta", "tan 1 a");
			dictionary.Add("dl", "mod w h 0");
			dictionary.Add("rw", "*/ dl 51965 100000");
			dictionary.Add("lM", "+- dl 0 rw");
			dictionary.Add("xM", "*/ ca lM 2");
			dictionary.Add("yM", "*/ sa lM 2");
			dictionary.Add("dxAM", "*/ sa th 2");
			dictionary.Add("dyAM", "*/ ca th 2");
			dictionary.Add("xA", "+- xM 0 dxAM");
			dictionary.Add("yA", "+- yM dyAM 0");
			dictionary.Add("xB", "+- xM dxAM 0");
			dictionary.Add("yB", "+- yM 0 dyAM");
			dictionary.Add("xBC", "+- hc 0 xB");
			dictionary.Add("yBC", "*/ xBC ta 1");
			dictionary.Add("yC", "+- yBC yB 0");
			dictionary.Add("xD", "+- r 0 xB");
			dictionary.Add("xE", "+- r 0 xA");
			dictionary.Add("yFE", "+- vc 0 yA");
			dictionary.Add("xFE", "*/ yFE 1 ta");
			dictionary.Add("xF", "+- xE 0 xFE");
			dictionary.Add("xL", "+- xA xFE 0");
			dictionary.Add("yG", "+- b 0 yA");
			dictionary.Add("yH", "+- b 0 yB");
			dictionary.Add("yI", "+- b 0 yC");
			dictionary.Add("xC2", "+- r 0 xM");
			dictionary.Add("yC3", "+- b 0 yM");
			break;
		case AutoShapeType.MathDivision:
			dictionary.Add("a1", "pin 1000 adj1 36745");
			dictionary.Add("ma1", "+- 0 0 a1");
			dictionary.Add("ma3h", "+/ 73490 ma1 4");
			dictionary.Add("ma3w", "*/ 36745 w h");
			dictionary.Add("maxAdj3", "min ma3h ma3w");
			dictionary.Add("a3", "pin 1000 adj3 maxAdj3");
			dictionary.Add("m4a3", "*/ -4 a3 1");
			dictionary.Add("maxAdj2", "+- 73490 m4a3 a1");
			dictionary.Add("a2", "pin 0 adj2 maxAdj2");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("yg", "*/ h a2 100000");
			dictionary.Add("rad", "*/ h a3 100000");
			dictionary.Add("dx1", "*/ w 73490 200000");
			dictionary.Add("y3", "+- vc 0 dy1");
			dictionary.Add("y4", "+- vc dy1 0");
			dictionary.Add("a", "+- yg rad 0");
			dictionary.Add("y2", "+- y3 0 a");
			dictionary.Add("y1", "+- y2 0 rad");
			dictionary.Add("y5", "+- b 0 y1");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x3", "+- hc dx1 0");
			dictionary.Add("x2", "+- hc 0 rad");
			break;
		case AutoShapeType.MathEqual:
			dictionary.Add("a1", "pin 0 adj1 36745");
			dictionary.Add("2a1", "*/ a1 2 1");
			dictionary.Add("mAdj2", "+- 100000 0 2a1");
			dictionary.Add("a2", "pin 0 adj2 mAdj2");
			dictionary.Add("dy1", "*/ h a1 100000");
			dictionary.Add("dy2", "*/ h a2 200000");
			dictionary.Add("dx1", "*/ w 73490 200000");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y1", "+- y2 0 dy1");
			dictionary.Add("y4", "+- y3 dy1 0");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			dictionary.Add("yC1", "+/ y1 y2 2");
			dictionary.Add("yC2", "+/ y3 y4 2");
			break;
		case AutoShapeType.MathNotEqual:
			dictionary.Add("a1", "pin 0 adj1 50000");
			dictionary.Add("crAng", "pin 4200000 adj2 6600000");
			dictionary.Add("2a1", "*/ a1 2 1");
			dictionary.Add("maxAdj3", "+- 100000 0 2a1");
			dictionary.Add("a3", "pin 0 adj3 maxAdj3");
			dictionary.Add("dy1", "*/ h a1 100000");
			dictionary.Add("dy2", "*/ h a3 200000");
			dictionary.Add("dx1", "*/ w 73490 200000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x8", "+- hc dx1 0");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y1", "+- y2 0 dy1");
			dictionary.Add("y4", "+- y3 dy1 0");
			dictionary.Add("cadj2", "+- crAng 0 cd4");
			dictionary.Add("xadj2", "tan hd2 cadj2");
			dictionary.Add("len", "mod xadj2 hd2 0");
			dictionary.Add("bhw", "*/ len dy1 hd2");
			dictionary.Add("bhw2", "*/ bhw 1 2");
			dictionary.Add("x7", "+- hc xadj2 bhw2");
			dictionary.Add("dx67", "*/ xadj2 y1 hd2");
			dictionary.Add("x6", "+- x7 0 dx67");
			dictionary.Add("dx57", "*/ xadj2 y2 hd2");
			dictionary.Add("x5", "+- x7 0 dx57");
			dictionary.Add("dx47", "*/ xadj2 y3 hd2");
			dictionary.Add("x4", "+- x7 0 dx47");
			dictionary.Add("dx37", "*/ xadj2 y4 hd2");
			dictionary.Add("x3", "+- x7 0 dx37");
			dictionary.Add("dx27", "*/ xadj2 2 1");
			dictionary.Add("x2", "+- x7 0 dx27");
			dictionary.Add("rx7", "+- x7 bhw 0");
			dictionary.Add("rx6", "+- x6 bhw 0");
			dictionary.Add("rx5", "+- x5 bhw 0");
			dictionary.Add("rx4", "+- x4 bhw 0");
			dictionary.Add("rx3", "+- x3 bhw 0");
			dictionary.Add("rx2", "+- x2 bhw 0");
			dictionary.Add("dx7", "*/ dy1 hd2 len");
			dictionary.Add("rxt", "+- x7 dx7 0");
			dictionary.Add("lxt", "+- rx7 0 dx7");
			dictionary.Add("rx", "?: cadj2 rxt rx7");
			dictionary.Add("lx", "?: cadj2 x7 lxt");
			dictionary.Add("dy3", "*/ dy1 xadj2 len");
			dictionary.Add("dy4", "+- 0 0 dy3");
			dictionary.Add("ry", "?: cadj2 dy3 t");
			dictionary.Add("ly", "?: cadj2 t dy4");
			dictionary.Add("dlx", "+- w 0 rx");
			dictionary.Add("drx", "+- w 0 lx");
			dictionary.Add("dly", "+- h 0 ry");
			dictionary.Add("dry", "+- h 0 ly");
			dictionary.Add("xC1", "+/ rx lx 2");
			dictionary.Add("xC2", "+/ drx dlx 2");
			dictionary.Add("yC1", "+/ ry ly 2");
			dictionary.Add("yC2", "+/ y1 y2 2");
			dictionary.Add("yC3", "+/ y3 y4 2");
			dictionary.Add("yC4", "+/ dry dly 2");
			break;
		case AutoShapeType.FlowChartAlternateProcess:
			dictionary.Add("x2", "+- r 0 ssd6");
			dictionary.Add("y2", "+- b 0 ssd6");
			dictionary.Add("il", "*/ ssd6 29289 100000");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.FlowChartDecision:
			dictionary.Add("ir", "*/ w 3 4");
			dictionary.Add("ib", "*/ h 3 4");
			break;
		case AutoShapeType.FlowChartData:
			dictionary.Add("x3", "*/ w 2 5");
			dictionary.Add("x4", "*/ w 3 5");
			dictionary.Add("x5", "*/ w 4 5");
			dictionary.Add("x6", "*/ w 9 10");
			break;
		case AutoShapeType.FlowChartPredefinedProcess:
			dictionary.Add("x2", "*/ w 7 8");
			break;
		case AutoShapeType.FlowChartDocument:
			dictionary.Add("y1", "*/ h 17322 21600");
			dictionary.Add("y2", "*/ h 20172 21600");
			break;
		case AutoShapeType.FlowChartMultiDocument:
			dictionary.Add("y2", "*/ h 3675 21600");
			dictionary.Add("y8", "*/ h 20782 21600");
			dictionary.Add("x3", "*/ w 9298 21600");
			dictionary.Add("x4", "*/ w 12286 21600");
			dictionary.Add("x5", "*/ w 18595 21600");
			break;
		case AutoShapeType.FlowChartTerminator:
			dictionary.Add("il", "*/ w 1018 21600");
			dictionary.Add("ir", "*/ w 20582 21600");
			dictionary.Add("it", "*/ h 3163 21600");
			dictionary.Add("ib", "*/ h 18437 21600");
			break;
		case AutoShapeType.FlowChartPreparation:
			dictionary.Add("x2", "*/ w 4 5");
			break;
		case AutoShapeType.FlowChartManualOperation:
			dictionary.Add("x3", "*/ w 4 5");
			dictionary.Add("x4", "*/ w 9 10");
			break;
		case AutoShapeType.FlowChartConnector:
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.FlowChartOffPageConnector:
			dictionary.Add("y1", "*/ h 4 5");
			break;
		case AutoShapeType.FlowChartPunchedTape:
			dictionary.Add("y2", "*/ h 9 10");
			dictionary.Add("ib", "*/ h 4 5");
			break;
		case AutoShapeType.FlowChartSummingJunction:
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.FlowChartOr:
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.FlowChartCollate:
			dictionary.Add("ir", "*/ w 3 4");
			dictionary.Add("ib", "*/ h 3 4");
			break;
		case AutoShapeType.FlowChartSort:
			dictionary.Add("ir", "*/ w 3 4");
			dictionary.Add("ib", "*/ h 3 4");
			break;
		case AutoShapeType.FlowChartExtract:
			dictionary.Add("x2", "*/ w 3 4");
			break;
		case AutoShapeType.FlowChartMerge:
			dictionary.Add("x2", "*/ w 3 4");
			break;
		case AutoShapeType.FlowChartStoredData:
			dictionary.Add("x2", "*/ w 5 6");
			break;
		case AutoShapeType.FlowChartDelay:
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.FlowChartSequentialAccessStorage:
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			dictionary.Add("ang", "at2 w h");
			dictionary.Add("ang1", "*/ ang 180 " + Math.PI);
			break;
		case AutoShapeType.FlowChartMagneticDisk:
			dictionary.Add("y3", "*/ h 5 6");
			break;
		case AutoShapeType.FlowChartDirectAccessStorage:
			dictionary.Add("x2", "*/ w 2 3");
			break;
		case AutoShapeType.FlowChartDisplay:
			dictionary.Add("x2", "*/ w 5 6");
			break;
		case AutoShapeType.Explosion1:
			dictionary.Add("x5", "*/ w 4627 21600");
			dictionary.Add("x12", "*/ w 8485 21600");
			dictionary.Add("x21", "*/ w 16702 21600");
			dictionary.Add("x24", "*/ w 14522 21600");
			dictionary.Add("y3", "*/ h 6320 21600");
			dictionary.Add("y6", "*/ h 8615 21600");
			dictionary.Add("y9", "*/ h 13937 21600");
			dictionary.Add("y18", "*/ h 13290 21600");
			break;
		case AutoShapeType.Explosion2:
			dictionary.Add("x2", "*/ w 9722 21600");
			dictionary.Add("x5", "*/ w 5372 21600");
			dictionary.Add("x16", "*/ w 11612 21600");
			dictionary.Add("x19", "*/ w 14640 21600");
			dictionary.Add("y2", "*/ h 1887 21600");
			dictionary.Add("y3", "*/ h 6382 21600");
			dictionary.Add("y8", "*/ h 12877 21600");
			dictionary.Add("y14", "*/ h 19712 21600");
			dictionary.Add("y16", "*/ h 18842 21600");
			dictionary.Add("y17", "*/ h 15935 21600");
			dictionary.Add("y24", "*/ h 6645 21600");
			break;
		case AutoShapeType.Star4Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("iwd2", "*/ wd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx", "cos iwd2 45");
			dictionary.Add("sdy", "sin ihd2 45");
			dictionary.Add("sx1", "+- hc 0 sdx");
			dictionary.Add("sx2", "+- hc sdx 0");
			dictionary.Add("sy1", "+- vc 0 sdy");
			dictionary.Add("sy2", "+- vc sdy 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star5Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("swd2", "*/ wd2 hf 100000");
			dictionary.Add("shd2", "*/ hd2 vf 100000");
			dictionary.Add("svc", "*/ vc  vf 100000");
			dictionary.Add("dx1", "cos swd2 18");
			dictionary.Add("dx2", "cos swd2 306");
			dictionary.Add("dy1", "sin shd2 18");
			dictionary.Add("dy2", "sin shd2 306");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "+- svc 0 dy1");
			dictionary.Add("y2", "+- svc 0 dy2");
			dictionary.Add("iwd2", "*/ swd2 a 50000");
			dictionary.Add("ihd2", "*/ shd2 a 50000");
			dictionary.Add("sdx1", "cos iwd2 342");
			dictionary.Add("sdx2", "cos iwd2 54");
			dictionary.Add("sdy1", "sin ihd2 54");
			dictionary.Add("sdy2", "sin ihd2 342");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc sdx2 0");
			dictionary.Add("sx4", "+- hc sdx1 0");
			dictionary.Add("sy1", "+- svc 0 sdy1");
			dictionary.Add("sy2", "+- svc 0 sdy2");
			dictionary.Add("sy3", "+- svc ihd2 0");
			dictionary.Add("yAdj", "+- svc 0 ihd2");
			break;
		case AutoShapeType.Star6Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("swd2", "*/ wd2 hf 100000");
			dictionary.Add("dx1", "cos swd2 30");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			dictionary.Add("y2", "+- vc hd4 0");
			dictionary.Add("iwd2", "*/ swd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx2", "*/ iwd2 1 2");
			dictionary.Add("sx1", "+- hc 0 iwd2");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc sdx2 0");
			dictionary.Add("sx4", "+- hc iwd2 0");
			dictionary.Add("sdy1", "sin ihd2 60");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc sdy1 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star7Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("swd2", "*/ wd2 hf 100000");
			dictionary.Add("shd2", "*/ hd2 vf 100000");
			dictionary.Add("svc", "*/ vc  vf 100000");
			dictionary.Add("dx1", "*/ swd2 97493 100000");
			dictionary.Add("dx2", "*/ swd2 78183 100000");
			dictionary.Add("dx3", "*/ swd2 43388 100000");
			dictionary.Add("dy1", "*/ shd2 62349 100000");
			dictionary.Add("dy2", "*/ shd2 22252 100000");
			dictionary.Add("dy3", "*/ shd2 90097 100000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc dx3 0");
			dictionary.Add("x5", "+- hc dx2 0");
			dictionary.Add("x6", "+- hc dx1 0");
			dictionary.Add("y1", "+- svc 0 dy1");
			dictionary.Add("y2", "+- svc dy2 0");
			dictionary.Add("y3", "+- svc dy3 0");
			dictionary.Add("iwd2", "*/ swd2 a 50000");
			dictionary.Add("ihd2", "*/ shd2 a 50000");
			dictionary.Add("sdx1", "*/ iwd2 97493 100000");
			dictionary.Add("sdx2", "*/ iwd2 78183 100000");
			dictionary.Add("sdx3", "*/ iwd2 43388 100000");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc 0 sdx3");
			dictionary.Add("sx4", "+- hc sdx3 0");
			dictionary.Add("sx5", "+- hc sdx2 0");
			dictionary.Add("sx6", "+- hc sdx1 0");
			dictionary.Add("sdy1", "*/ ihd2 90097 100000");
			dictionary.Add("sdy2", "*/ ihd2 22252 100000");
			dictionary.Add("sdy3", "*/ ihd2 62349 100000");
			dictionary.Add("sy1", "+- svc 0 sdy1");
			dictionary.Add("sy2", "+- svc 0 sdy2");
			dictionary.Add("sy3", "+- svc sdy3 0");
			dictionary.Add("sy4", "+- svc ihd2 0");
			dictionary.Add("yAdj", "+- svc 0 ihd2");
			break;
		case AutoShapeType.Star8Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "cos wd2 45");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc dx1 0");
			dictionary.Add("dy1", "sin hd2 45");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc dy1 0");
			dictionary.Add("iwd2", "*/ wd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx1", "*/ iwd2 92388 100000");
			dictionary.Add("sdx2", "*/ iwd2 38268 100000");
			dictionary.Add("sdy1", "*/ ihd2 92388 100000");
			dictionary.Add("sdy2", "*/ ihd2 38268 100000");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc sdx2 0");
			dictionary.Add("sx4", "+- hc sdx1 0");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc 0 sdy2");
			dictionary.Add("sy3", "+- vc sdy2 0");
			dictionary.Add("sy4", "+- vc sdy1 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star10Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("swd2", "*/ wd2 hf 100000");
			dictionary.Add("dx1", "*/ swd2 95106 100000");
			dictionary.Add("dx2", "*/ swd2 58779 100000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc dx2 0");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("dy1", "*/ hd2 80902 100000");
			dictionary.Add("dy2", "*/ hd2 30902 100000");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc dy2 0");
			dictionary.Add("y4", "+- vc dy1 0");
			dictionary.Add("iwd2", "*/ swd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx1", "*/ iwd2 80902 100000");
			dictionary.Add("sdx2", "*/ iwd2 30902 100000");
			dictionary.Add("sdy1", "*/ ihd2 95106 100000");
			dictionary.Add("sdy2", "*/ ihd2 58779 100000");
			dictionary.Add("sx1", "+- hc 0 iwd2");
			dictionary.Add("sx2", "+- hc 0 sdx1");
			dictionary.Add("sx3", "+- hc 0 sdx2");
			dictionary.Add("sx4", "+- hc sdx2 0");
			dictionary.Add("sx5", "+- hc sdx1 0");
			dictionary.Add("sx6", "+- hc iwd2 0");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc 0 sdy2");
			dictionary.Add("sy3", "+- vc sdy2 0");
			dictionary.Add("sy4", "+- vc sdy1 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star12Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "cos wd2 30");
			dictionary.Add("dy1", "sin hd2 60");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x3", "*/ w 3 4");
			dictionary.Add("x4", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y3", "*/ h 3 4");
			dictionary.Add("y4", "+- vc dy1 0");
			dictionary.Add("iwd2", "*/ wd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx1", "cos iwd2 15");
			dictionary.Add("sdx2", "cos iwd2 45");
			dictionary.Add("sdx3", "cos iwd2 75");
			dictionary.Add("sdy1", "sin ihd2 75");
			dictionary.Add("sdy2", "sin ihd2 45");
			dictionary.Add("sdy3", "sin ihd2 15");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc 0 sdx3");
			dictionary.Add("sx4", "+- hc sdx3 0");
			dictionary.Add("sx5", "+- hc sdx2 0");
			dictionary.Add("sx6", "+- hc sdx1 0");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc 0 sdy2");
			dictionary.Add("sy3", "+- vc 0 sdy3");
			dictionary.Add("sy4", "+- vc sdy3 0");
			dictionary.Add("sy5", "+- vc sdy2 0");
			dictionary.Add("sy6", "+- vc sdy1 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star16Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "*/ wd2 92388 100000");
			dictionary.Add("dx2", "*/ wd2 70711 100000");
			dictionary.Add("dx3", "*/ wd2 38268 100000");
			dictionary.Add("dy1", "*/ hd2 92388 100000");
			dictionary.Add("dy2", "*/ hd2 70711 100000");
			dictionary.Add("dy3", "*/ hd2 38268 100000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc dx3 0");
			dictionary.Add("x5", "+- hc dx2 0");
			dictionary.Add("x6", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc 0 dy3");
			dictionary.Add("y4", "+- vc dy3 0");
			dictionary.Add("y5", "+- vc dy2 0");
			dictionary.Add("y6", "+- vc dy1 0");
			dictionary.Add("iwd2", "*/ wd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx1", "*/ iwd2 98079 100000");
			dictionary.Add("sdx2", "*/ iwd2 83147 100000");
			dictionary.Add("sdx3", "*/ iwd2 55557 100000");
			dictionary.Add("sdx4", "*/ iwd2 19509 100000");
			dictionary.Add("sdy1", "*/ ihd2 98079 100000");
			dictionary.Add("sdy2", "*/ ihd2 83147 100000");
			dictionary.Add("sdy3", "*/ ihd2 55557 100000");
			dictionary.Add("sdy4", "*/ ihd2 19509 100000");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc 0 sdx3");
			dictionary.Add("sx4", "+- hc 0 sdx4");
			dictionary.Add("sx5", "+- hc sdx4 0");
			dictionary.Add("sx6", "+- hc sdx3 0");
			dictionary.Add("sx7", "+- hc sdx2 0");
			dictionary.Add("sx8", "+- hc sdx1 0");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc 0 sdy2");
			dictionary.Add("sy3", "+- vc 0 sdy3");
			dictionary.Add("sy4", "+- vc 0 sdy4");
			dictionary.Add("sy5", "+- vc sdy4 0");
			dictionary.Add("sy6", "+- vc sdy3 0");
			dictionary.Add("sy7", "+- vc sdy2 0");
			dictionary.Add("sy8", "+- vc sdy1 0");
			dictionary.Add("idx", "cos iwd2 45");
			dictionary.Add("idy", "sin ihd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("ib", "+- vc idy 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star24Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "cos wd2 15");
			dictionary.Add("dx2", "cos wd2 30");
			dictionary.Add("dx3", "cos wd2 45");
			dictionary.Add("dx4", "val wd4");
			dictionary.Add("dx5", "cos wd2 75");
			dictionary.Add("dy1", "sin hd2 75");
			dictionary.Add("dy2", "sin hd2 60");
			dictionary.Add("dy3", "sin hd2 45");
			dictionary.Add("dy4", "val hd4");
			dictionary.Add("dy5", "sin hd2 15");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc 0 dx4");
			dictionary.Add("x5", "+- hc 0 dx5");
			dictionary.Add("x6", "+- hc dx5 0");
			dictionary.Add("x7", "+- hc dx4 0");
			dictionary.Add("x8", "+- hc dx3 0");
			dictionary.Add("x9", "+- hc dx2 0");
			dictionary.Add("x10", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc 0 dy3");
			dictionary.Add("y4", "+- vc 0 dy4");
			dictionary.Add("y5", "+- vc 0 dy5");
			dictionary.Add("y6", "+- vc dy5 0");
			dictionary.Add("y7", "+- vc dy4 0");
			dictionary.Add("y8", "+- vc dy3 0");
			dictionary.Add("y9", "+- vc dy2 0");
			dictionary.Add("y10", "+- vc dy1 0");
			dictionary.Add("iwd2", "*/ wd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx1", "*/ iwd2 99144 100000");
			dictionary.Add("sdx2", "*/ iwd2 92388 100000");
			dictionary.Add("sdx3", "*/ iwd2 79335 100000");
			dictionary.Add("sdx4", "*/ iwd2 60876 100000");
			dictionary.Add("sdx5", "*/ iwd2 38268 100000");
			dictionary.Add("sdx6", "*/ iwd2 13053 100000");
			dictionary.Add("sdy1", "*/ ihd2 99144 100000");
			dictionary.Add("sdy2", "*/ ihd2 92388 100000");
			dictionary.Add("sdy3", "*/ ihd2 79335 100000");
			dictionary.Add("sdy4", "*/ ihd2 60876 100000");
			dictionary.Add("sdy5", "*/ ihd2 38268 100000");
			dictionary.Add("sdy6", "*/ ihd2 13053 100000");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc 0 sdx3");
			dictionary.Add("sx4", "+- hc 0 sdx4");
			dictionary.Add("sx5", "+- hc 0 sdx5");
			dictionary.Add("sx6", "+- hc 0 sdx6");
			dictionary.Add("sx7", "+- hc sdx6 0");
			dictionary.Add("sx8", "+- hc sdx5 0");
			dictionary.Add("sx9", "+- hc sdx4 0");
			dictionary.Add("sx10", "+- hc sdx3 0");
			dictionary.Add("sx11", "+- hc sdx2 0");
			dictionary.Add("sx12", "+- hc sdx1 0");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc 0 sdy2");
			dictionary.Add("sy3", "+- vc 0 sdy3");
			dictionary.Add("sy4", "+- vc 0 sdy4");
			dictionary.Add("sy5", "+- vc 0 sdy5");
			dictionary.Add("sy6", "+- vc 0 sdy6");
			dictionary.Add("sy7", "+- vc sdy6 0");
			dictionary.Add("sy8", "+- vc sdy5 0");
			dictionary.Add("sy9", "+- vc sdy4 0");
			dictionary.Add("sy10", "+- vc sdy3 0");
			dictionary.Add("sy11", "+- vc sdy2 0");
			dictionary.Add("sy12", "+- vc sdy1 0");
			dictionary.Add("idx", "cos iwd2 45");
			dictionary.Add("idy", "sin ihd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("ib", "+- vc idy 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.Star32Point:
			dictionary.Add("a", "pin 0 adj 50000");
			dictionary.Add("dx1", "*/ wd2 98079 100000");
			dictionary.Add("dx2", "*/ wd2 92388 100000");
			dictionary.Add("dx3", "*/ wd2 83147 100000");
			dictionary.Add("dx4", "cos wd2 45");
			dictionary.Add("dx5", "*/ wd2 55557 100000");
			dictionary.Add("dx6", "*/ wd2 38268 100000");
			dictionary.Add("dx7", "*/ wd2 19509 100000");
			dictionary.Add("dy1", "*/ hd2 98079 100000");
			dictionary.Add("dy2", "*/ hd2 92388 100000");
			dictionary.Add("dy3", "*/ hd2 83147 100000");
			dictionary.Add("dy4", "sin hd2 45");
			dictionary.Add("dy5", "*/ hd2 55557 100000");
			dictionary.Add("dy6", "*/ hd2 38268 100000");
			dictionary.Add("dy7", "*/ hd2 19509 100000");
			dictionary.Add("x1", "+- hc 0 dx1");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- hc 0 dx3");
			dictionary.Add("x4", "+- hc 0 dx4");
			dictionary.Add("x5", "+- hc 0 dx5");
			dictionary.Add("x6", "+- hc 0 dx6");
			dictionary.Add("x7", "+- hc 0 dx7");
			dictionary.Add("x8", "+- hc dx7 0");
			dictionary.Add("x9", "+- hc dx6 0");
			dictionary.Add("x10", "+- hc dx5 0");
			dictionary.Add("x11", "+- hc dx4 0");
			dictionary.Add("x12", "+- hc dx3 0");
			dictionary.Add("x13", "+- hc dx2 0");
			dictionary.Add("x14", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc 0 dy1");
			dictionary.Add("y2", "+- vc 0 dy2");
			dictionary.Add("y3", "+- vc 0 dy3");
			dictionary.Add("y4", "+- vc 0 dy4");
			dictionary.Add("y5", "+- vc 0 dy5");
			dictionary.Add("y6", "+- vc 0 dy6");
			dictionary.Add("y7", "+- vc 0 dy7");
			dictionary.Add("y8", "+- vc dy7 0");
			dictionary.Add("y9", "+- vc dy6 0");
			dictionary.Add("y10", "+- vc dy5 0");
			dictionary.Add("y11", "+- vc dy4 0");
			dictionary.Add("y12", "+- vc dy3 0");
			dictionary.Add("y13", "+- vc dy2 0");
			dictionary.Add("y14", "+- vc dy1 0");
			dictionary.Add("iwd2", "*/ wd2 a 50000");
			dictionary.Add("ihd2", "*/ hd2 a 50000");
			dictionary.Add("sdx1", "*/ iwd2 99518 100000");
			dictionary.Add("sdx2", "*/ iwd2 95694 100000");
			dictionary.Add("sdx3", "*/ iwd2 88192 100000");
			dictionary.Add("sdx4", "*/ iwd2 77301 100000");
			dictionary.Add("sdx5", "*/ iwd2 63439 100000");
			dictionary.Add("sdx6", "*/ iwd2 47140 100000");
			dictionary.Add("sdx7", "*/ iwd2 29028 100000");
			dictionary.Add("sdx8", "*/ iwd2 9802 100000");
			dictionary.Add("sdy1", "*/ ihd2 99518 100000");
			dictionary.Add("sdy2", "*/ ihd2 95694 100000");
			dictionary.Add("sdy3", "*/ ihd2 88192 100000");
			dictionary.Add("sdy4", "*/ ihd2 77301 100000");
			dictionary.Add("sdy5", "*/ ihd2 63439 100000");
			dictionary.Add("sdy6", "*/ ihd2 47140 100000");
			dictionary.Add("sdy7", "*/ ihd2 29028 100000");
			dictionary.Add("sdy8", "*/ ihd2 9802 100000");
			dictionary.Add("sx1", "+- hc 0 sdx1");
			dictionary.Add("sx2", "+- hc 0 sdx2");
			dictionary.Add("sx3", "+- hc 0 sdx3");
			dictionary.Add("sx4", "+- hc 0 sdx4");
			dictionary.Add("sx5", "+- hc 0 sdx5");
			dictionary.Add("sx6", "+- hc 0 sdx6");
			dictionary.Add("sx7", "+- hc 0 sdx7");
			dictionary.Add("sx8", "+- hc 0 sdx8");
			dictionary.Add("sx9", "+- hc sdx8 0");
			dictionary.Add("sx10", "+- hc sdx7 0");
			dictionary.Add("sx11", "+- hc sdx6 0");
			dictionary.Add("sx12", "+- hc sdx5 0");
			dictionary.Add("sx13", "+- hc sdx4 0");
			dictionary.Add("sx14", "+- hc sdx3 0");
			dictionary.Add("sx15", "+- hc sdx2 0");
			dictionary.Add("sx16", "+- hc sdx1 0");
			dictionary.Add("sy1", "+- vc 0 sdy1");
			dictionary.Add("sy2", "+- vc 0 sdy2");
			dictionary.Add("sy3", "+- vc 0 sdy3");
			dictionary.Add("sy4", "+- vc 0 sdy4");
			dictionary.Add("sy5", "+- vc 0 sdy5");
			dictionary.Add("sy6", "+- vc 0 sdy6");
			dictionary.Add("sy7", "+- vc 0 sdy7");
			dictionary.Add("sy8", "+- vc 0 sdy8");
			dictionary.Add("sy9", "+- vc sdy8 0");
			dictionary.Add("sy10", "+- vc sdy7 0");
			dictionary.Add("sy11", "+- vc sdy6 0");
			dictionary.Add("sy12", "+- vc sdy5 0");
			dictionary.Add("sy13", "+- vc sdy4 0");
			dictionary.Add("sy14", "+- vc sdy3 0");
			dictionary.Add("sy15", "+- vc sdy2 0");
			dictionary.Add("sy16", "+- vc sdy1 0");
			dictionary.Add("idx", "cos iwd2 45");
			dictionary.Add("idy", "sin ihd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("ib", "+- vc idy 0");
			dictionary.Add("yAdj", "+- vc 0 ihd2");
			break;
		case AutoShapeType.UpRibbon:
			dictionary.Add("a1", "pin 0 adj1 33333");
			dictionary.Add("a2", "pin 25000 adj2 75000");
			dictionary.Add("x10", "+- r 0 wd8");
			dictionary.Add("dx2", "*/ w a2 200000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x9", "+- hc dx2 0");
			dictionary.Add("x3", "+- x2 wd32 0");
			dictionary.Add("x8", "+- x9 0 wd32");
			dictionary.Add("x5", "+- x2 wd8 0");
			dictionary.Add("x6", "+- x9 0 wd8");
			dictionary.Add("x4", "+- x5 0 wd32");
			dictionary.Add("x7", "+- x6 wd32 0");
			dictionary.Add("dy1", "*/ h a1 200000");
			dictionary.Add("y1", "+- b 0 dy1");
			dictionary.Add("dy2", "*/ h a1 100000");
			dictionary.Add("y2", "+- b 0 dy2");
			dictionary.Add("y4", "+- t dy2 0");
			dictionary.Add("y3", "+/ y4 b 2");
			dictionary.Add("hR", "*/ h a1 400000");
			dictionary.Add("y6", "+- b 0 hR");
			dictionary.Add("y7", "+- y1 0 hR");
			break;
		case AutoShapeType.DownRibbon:
			dictionary.Add("a1", "pin 0 adj1 33333");
			dictionary.Add("a2", "pin 25000 adj2 75000");
			dictionary.Add("x10", "+- r 0 wd8");
			dictionary.Add("dx2", "*/ w a2 200000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x9", "+- hc dx2 0");
			dictionary.Add("x3", "+- x2 wd32 0");
			dictionary.Add("x8", "+- x9 0 wd32");
			dictionary.Add("x5", "+- x2 wd8 0");
			dictionary.Add("x6", "+- x9 0 wd8");
			dictionary.Add("x4", "+- x5 0 wd32");
			dictionary.Add("x7", "+- x6 wd32 0");
			dictionary.Add("y1", "*/ h a1 200000");
			dictionary.Add("y2", "*/ h a1 100000");
			dictionary.Add("y4", "+- b 0 y2");
			dictionary.Add("y3", "*/ y4 1 2");
			dictionary.Add("hR", "*/ h a1 400000");
			dictionary.Add("y5", "+- b 0 hR");
			dictionary.Add("y6", "+- y2 0 hR");
			break;
		case AutoShapeType.CurvedUpRibbon:
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 25000 adj2 75000");
			dictionary.Add("q10", "+- 100000 0 a1");
			dictionary.Add("q11", "*/ q10 1 2");
			dictionary.Add("q12", "+- a1 0 q11");
			dictionary.Add("minAdj3", "max 0 q12");
			dictionary.Add("a3", "pin minAdj3 adj3 a1");
			dictionary.Add("dx2", "*/ w a2 200000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- x2 wd8 0");
			dictionary.Add("x4", "+- r 0 x3");
			dictionary.Add("x5", "+- r 0 x2");
			dictionary.Add("x6", "+- r 0 wd8");
			dictionary.Add("dy1", "*/ h a3 100000");
			dictionary.Add("f1", "*/ 4 dy1 w");
			dictionary.Add("q111", "*/ x3 x3 w");
			dictionary.Add("q2", "+- x3 0 q111");
			dictionary.Add("u1", "*/ f1 q2 1");
			dictionary.Add("y1", "+- b 0 u1");
			dictionary.Add("cx1", "*/ x3 1 2");
			dictionary.Add("cu1", "*/ f1 cx1 1");
			dictionary.Add("cy1", "+- b 0 cu1");
			dictionary.Add("cx2", "+- r 0 cx1");
			dictionary.Add("q1", "*/ h a1 100000");
			dictionary.Add("dy3", "+- q1 0 dy1");
			dictionary.Add("q3", "*/ x2 x2 w");
			dictionary.Add("q4", "+- x2 0 q3");
			dictionary.Add("q5", "*/ f1 q4 1");
			dictionary.Add("u3", "+- q5 dy3 0");
			dictionary.Add("y3", "+- b 0 u3");
			dictionary.Add("q6", "+- dy1 dy3 u3");
			dictionary.Add("q7", "+- q6 dy1 0");
			dictionary.Add("cu3", "+- q7 dy3 0");
			dictionary.Add("cy3", "+- b 0 cu3");
			dictionary.Add("rh", "+- b 0 q1");
			dictionary.Add("q8", "*/ dy1 14 16");
			dictionary.Add("u2", "+/ q8 rh 2");
			dictionary.Add("y2", "+- b 0 u2");
			dictionary.Add("u5", "+- q5 rh 0");
			dictionary.Add("y5", "+- b 0 u5");
			dictionary.Add("u6", "+- u3 rh 0");
			dictionary.Add("y6", "+- b 0 u6");
			dictionary.Add("cx4", "*/ x2 1 2");
			dictionary.Add("q9", "*/ f1 cx4 1");
			dictionary.Add("cu4", "+- q9 rh 0");
			dictionary.Add("cy4", "+- b 0 cu4");
			dictionary.Add("cx5", "+- r 0 cx4");
			dictionary.Add("cu6", "+- cu3 rh 0");
			dictionary.Add("cy6", "+- b 0 cu6");
			dictionary.Add("u7", "+- u1 dy3 0");
			dictionary.Add("y7", "+- b 0 u7");
			dictionary.Add("cu7", "+- q1 q1 u7");
			dictionary.Add("cy7", "+- b 0 cu7");
			break;
		case AutoShapeType.CurvedDownRibbon:
			dictionary.Add("a1", "pin 0 adj1 100000");
			dictionary.Add("a2", "pin 25000 adj2 75000");
			dictionary.Add("q10", "+- 100000 0 a1");
			dictionary.Add("q11", "*/ q10 1 2");
			dictionary.Add("q12", "+- a1 0 q11");
			dictionary.Add("minAdj3", "max 0 q12");
			dictionary.Add("a3", "pin minAdj3 adj3 a1");
			dictionary.Add("dx2", "*/ w a2 200000");
			dictionary.Add("x2", "+- hc 0 dx2");
			dictionary.Add("x3", "+- x2 wd8 0");
			dictionary.Add("x4", "+- r 0 x3");
			dictionary.Add("x5", "+- r 0 x2");
			dictionary.Add("x6", "+- r 0 wd8");
			dictionary.Add("dy1", "*/ h a3 100000");
			dictionary.Add("f1", "*/ 4 dy1 w");
			dictionary.Add("q111", "*/ x3 x3 w");
			dictionary.Add("q2", "+- x3 0 q111");
			dictionary.Add("y1", "*/ f1 q2 1");
			dictionary.Add("cx1", "*/ x3 1 2");
			dictionary.Add("cy1", "*/ f1 cx1 1");
			dictionary.Add("cx2", "+- r 0 cx1");
			dictionary.Add("q1", "*/ h a1 100000");
			dictionary.Add("dy3", "+- q1 0 dy1");
			dictionary.Add("q3", "*/ x2 x2 w");
			dictionary.Add("q4", "+- x2 0 q3");
			dictionary.Add("q5", "*/ f1 q4 1");
			dictionary.Add("y3", "+- q5 dy3 0");
			dictionary.Add("q6", "+- dy1 dy3 y3");
			dictionary.Add("q7", "+- q6 dy1 0");
			dictionary.Add("cy3", "+- q7 dy3 0");
			dictionary.Add("rh", "+- b 0 q1");
			dictionary.Add("q8", "*/ dy1 14 16");
			dictionary.Add("y2", "+/ q8 rh 2");
			dictionary.Add("y5", "+- q5 rh 0");
			dictionary.Add("y6", "+- y3 rh 0");
			dictionary.Add("cx4", "*/ x2 1 2");
			dictionary.Add("q9", "*/ f1 cx4 1");
			dictionary.Add("cy4", "+- q9 rh 0");
			dictionary.Add("cx5", "+- r 0 cx4");
			dictionary.Add("cy6", "+- cy3 rh 0");
			dictionary.Add("y7", "+- y1 dy3 0");
			dictionary.Add("cy7", "+- q1 q1 y7");
			dictionary.Add("y8", "+- b 0 dy1");
			break;
		case AutoShapeType.VerticalScroll:
			dictionary.Add("a", "pin 0 adj 25000");
			dictionary.Add("ch", "*/ ss a 100000");
			dictionary.Add("ch2", "*/ ch 1 2");
			dictionary.Add("ch4", "*/ ch 1 4");
			dictionary.Add("x3", "+- ch ch2 0");
			dictionary.Add("x4", "+- ch ch 0");
			dictionary.Add("x6", "+- r 0 ch");
			dictionary.Add("x7", "+- r 0 ch2");
			dictionary.Add("x5", "+- x6 0 ch2");
			dictionary.Add("y3", "+- b 0 ch");
			dictionary.Add("y4", "+- b 0 ch2");
			break;
		case AutoShapeType.HorizontalScroll:
			dictionary.Add("a", "pin 0 adj 25000");
			dictionary.Add("ch", "*/ ss a 100000");
			dictionary.Add("ch2", "*/ ch 1 2");
			dictionary.Add("ch4", "*/ ch 1 4");
			dictionary.Add("y3", "+- ch ch2 0");
			dictionary.Add("y4", "+- ch ch 0");
			dictionary.Add("y6", "+- b 0 ch");
			dictionary.Add("y7", "+- b 0 ch2");
			dictionary.Add("y5", "+- y6 0 ch2");
			dictionary.Add("x3", "+- r 0 ch");
			dictionary.Add("x4", "+- r 0 ch2");
			break;
		case AutoShapeType.Wave:
			dictionary.Add("a1", "pin 0 adj1 20000");
			dictionary.Add("a2", "pin -10000 adj2 10000");
			dictionary.Add("y1", "*/ h a1 100000");
			dictionary.Add("dy2", "*/ y1 10 3");
			dictionary.Add("y2", "+- y1 0 dy2");
			dictionary.Add("y3", "+- y1 dy2 0");
			dictionary.Add("y4", "+- b 0 y1");
			dictionary.Add("y5", "+- y4 0 dy2");
			dictionary.Add("y6", "+- y4 dy2 0");
			dictionary.Add("dx1", "*/ w a2 100000");
			dictionary.Add("of2", "*/ w a2 50000");
			dictionary.Add("x1", "abs dx1");
			dictionary.Add("dx2", "?: of2 0 of2");
			dictionary.Add("x2", "+- l 0 dx2");
			dictionary.Add("dx5", "?: of2 of2 0");
			dictionary.Add("x5", "+- r 0 dx5");
			dictionary.Add("dx3", "+/ dx2 x5 3");
			dictionary.Add("x3", "+- x2 dx3 0");
			dictionary.Add("x4", "+/ x3 x5 2");
			dictionary.Add("x6", "+- l dx5 0");
			dictionary.Add("x10", "+- r dx2 0");
			dictionary.Add("x7", "+- x6 dx3 0");
			dictionary.Add("x8", "+/ x7 x10 2");
			dictionary.Add("x9", "+- r 0 x1");
			dictionary.Add("xAdj", "+- hc dx1 0");
			dictionary.Add("xAdj2", "+- hc 0 dx1");
			dictionary.Add("il", "max x2 x6");
			dictionary.Add("ir", "min x5 x10");
			dictionary.Add("it", "*/ h a1 50000");
			dictionary.Add("ib", "+- b 0 it");
			break;
		case AutoShapeType.DoubleWave:
			dictionary.Add("a1", "pin 0 adj1 12500");
			dictionary.Add("a2", "pin -10000 adj2 10000");
			dictionary.Add("y1", "*/ h a1 100000");
			dictionary.Add("dy2", "*/ y1 10 3");
			dictionary.Add("y2", "+- y1 0 dy2");
			dictionary.Add("y3", "+- y1 dy2 0");
			dictionary.Add("y4", "+- b 0 y1");
			dictionary.Add("y5", "+- y4 0 dy2");
			dictionary.Add("y6", "+- y4 dy2 0");
			dictionary.Add("dx1", "*/ w a2 100000");
			dictionary.Add("of2", "*/ w a2 50000");
			dictionary.Add("x1", "abs dx1");
			dictionary.Add("dx2", "?: of2 0 of2");
			dictionary.Add("x2", "+- l 0 dx2");
			dictionary.Add("dx8", "?: of2 of2 0");
			dictionary.Add("x8", "+- r 0 dx8");
			dictionary.Add("dx3", "+/ dx2 x8 6");
			dictionary.Add("x3", "+- x2 dx3 0");
			dictionary.Add("dx4", "+/ dx2 x8 3");
			dictionary.Add("x4", "+- x2 dx4 0");
			dictionary.Add("x5", "+/ x2 x8 2");
			dictionary.Add("x6", "+- x5 dx3 0");
			dictionary.Add("x7", "+/ x6 x8 2");
			dictionary.Add("x9", "+- l dx8 0");
			dictionary.Add("x15", "+- r dx2 0");
			dictionary.Add("x10", "+- x9 dx3 0");
			dictionary.Add("x11", "+- x9 dx4 0");
			dictionary.Add("x12", "+/ x9 x15 2");
			dictionary.Add("x13", "+- x12 dx3 0");
			dictionary.Add("x14", "+/ x13 x15 2");
			dictionary.Add("x16", "+- r 0 x1");
			dictionary.Add("xAdj", "+- hc dx1 0");
			dictionary.Add("il", "max x2 x9");
			dictionary.Add("ir", "min x8 x15");
			dictionary.Add("it", "*/ h a1 50000");
			dictionary.Add("ib", "+- b 0 it");
			break;
		case AutoShapeType.RectangularCallout:
			dictionary.Add("dxPos", "*/ w adj1 100000");
			dictionary.Add("dyPos", "*/ h adj2 100000");
			dictionary.Add("xPos", "+- hc dxPos 0");
			dictionary.Add("yPos", "+- vc dyPos 0");
			dictionary.Add("dx", "+- xPos 0 hc");
			dictionary.Add("dy", "+- yPos 0 vc");
			dictionary.Add("dq", "*/ dxPos h w");
			dictionary.Add("ady", "abs dyPos");
			dictionary.Add("adq", "abs dq");
			dictionary.Add("dz", "+- ady 0 adq");
			dictionary.Add("xg1", "?: dxPos 7 2");
			dictionary.Add("xg2", "?: dxPos 10 5");
			dictionary.Add("x1", "*/ w xg1 12");
			dictionary.Add("x2", "*/ w xg2 12");
			dictionary.Add("yg1", "?: dyPos 7 2");
			dictionary.Add("yg2", "?: dyPos 10 5");
			dictionary.Add("y1", "*/ h yg1 12");
			dictionary.Add("y2", "*/ h yg2 12");
			dictionary.Add("t1", "?: dxPos l xPos");
			dictionary.Add("xl", "?: dz l t1");
			dictionary.Add("t2", "?: dyPos x1 xPos");
			dictionary.Add("xt", "?: dz t2 x1");
			dictionary.Add("t3", "?: dxPos xPos r");
			dictionary.Add("xr", "?: dz r t3");
			dictionary.Add("t4", "?: dyPos xPos x1");
			dictionary.Add("xb", "?: dz t4 x1");
			dictionary.Add("t5", "?: dxPos y1 yPos");
			dictionary.Add("yl", "?: dz y1 t5");
			dictionary.Add("t6", "?: dyPos t yPos");
			dictionary.Add("yt", "?: dz t6 t");
			dictionary.Add("t7", "?: dxPos yPos y1");
			dictionary.Add("yr", "?: dz y1 t7");
			dictionary.Add("t8", "?: dyPos yPos b");
			dictionary.Add("yb", "?: dz t8 b");
			break;
		case AutoShapeType.RoundedRectangularCallout:
			dictionary.Add("dxPos", "*/ w adj1 100000");
			dictionary.Add("dyPos", "*/ h adj2 100000");
			dictionary.Add("xPos", "+- hc dxPos 0");
			dictionary.Add("yPos", "+- vc dyPos 0");
			dictionary.Add("dq", "*/ dxPos h w");
			dictionary.Add("ady", "abs dyPos");
			dictionary.Add("adq", "abs dq");
			dictionary.Add("dz", "+- ady 0 adq");
			dictionary.Add("xg1", "?: dxPos 7 2");
			dictionary.Add("xg2", "?: dxPos 10 5");
			dictionary.Add("x1", "*/ w xg1 12");
			dictionary.Add("x2", "*/ w xg2 12");
			dictionary.Add("yg1", "?: dyPos 7 2");
			dictionary.Add("yg2", "?: dyPos 10 5");
			dictionary.Add("y1", "*/ h yg1 12");
			dictionary.Add("y2", "*/ h yg2 12");
			dictionary.Add("t1", "?: dxPos l xPos");
			dictionary.Add("xl", "?: dz l t1");
			dictionary.Add("t2", "?: dyPos x1 xPos");
			dictionary.Add("xt", "?: dz t2 x1");
			dictionary.Add("t3", "?: dxPos xPos r");
			dictionary.Add("xr", "?: dz r t3");
			dictionary.Add("t4", "?: dyPos xPos x1");
			dictionary.Add("xb", "?: dz t4 x1");
			dictionary.Add("t5", "?: dxPos y1 yPos");
			dictionary.Add("yl", "?: dz y1 t5");
			dictionary.Add("t6", "?: dyPos t yPos");
			dictionary.Add("yt", "?: dz t6 t");
			dictionary.Add("t7", "?: dxPos yPos y1");
			dictionary.Add("yr", "?: dz y1 t7");
			dictionary.Add("t8", "?: dyPos yPos b");
			dictionary.Add("yb", "?: dz t8 b");
			dictionary.Add("u1", "*/ ss adj3 100000");
			dictionary.Add("u2", "+- r 0 u1");
			dictionary.Add("v2", "+- b 0 u1");
			dictionary.Add("il", "*/ u1 29289 100000");
			dictionary.Add("ir", "+- r 0 il");
			dictionary.Add("ib", "+- b 0 il");
			break;
		case AutoShapeType.OvalCallout:
			dictionary.Add("dxPos", "*/ w adj1 100000");
			dictionary.Add("dyPos", "*/ h adj2 100000");
			dictionary.Add("xPos", "+- hc dxPos 0");
			dictionary.Add("yPos", "+- vc dyPos 0");
			dictionary.Add("sdx", "*/ dxPos h 1");
			dictionary.Add("sdy", "*/ dyPos w 1");
			dictionary.Add("pang1", "at2 sdx sdy");
			dictionary.Add("pang", "*/ pang1 180 " + Math.PI);
			dictionary.Add("stAng", "+- pang 11 0");
			dictionary.Add("enAng", "+- pang 0 11");
			dictionary.Add("dx1", "cos wd2 stAng");
			dictionary.Add("dy1", "sin hd2 stAng");
			dictionary.Add("x1", "+- hc dx1 0");
			dictionary.Add("y1", "+- vc dy1 0");
			dictionary.Add("dx2", "cos wd2 enAng");
			dictionary.Add("dy2", "sin hd2 enAng");
			dictionary.Add("x2", "+- hc dx2 0");
			dictionary.Add("y2", "+- vc dy2 0");
			dictionary.Add("stAng2", "at2 dx1 dy1");
			dictionary.Add("stAng1", "*/ stAng2 180 " + Math.PI);
			dictionary.Add("enAng2", "at2 dx2 dy2");
			dictionary.Add("enAng1", "*/ enAng2 180 " + Math.PI);
			dictionary.Add("swAng1", "+- enAng1 0 stAng1");
			dictionary.Add("swAng2", "+- swAng1 360 0");
			dictionary.Add("swAng", "?: swAng1 swAng1 swAng2");
			dictionary.Add("idx", "cos wd2 45");
			dictionary.Add("idy", "sin hd2 45");
			dictionary.Add("il", "+- hc 0 idx");
			dictionary.Add("ir", "+- hc idx 0");
			dictionary.Add("it", "+- vc 0 idy");
			dictionary.Add("ib", "+- vc idy 0");
			break;
		case AutoShapeType.CloudCallout:
			dictionary.Add("dxPos", "*/ w adj1 100000");
			dictionary.Add("dyPos", "*/ h adj2 100000");
			dictionary.Add("xPos", "+- hc dxPos 0");
			dictionary.Add("yPos", "+- vc dyPos 0");
			dictionary.Add("ht", "cat2 hd2 dxPos dyPos");
			dictionary.Add("wt", "sat2 wd2 dxPos dyPos");
			dictionary.Add("g2", "cat2 wd2 ht wt");
			dictionary.Add("g3", "sat2 hd2 ht wt");
			dictionary.Add("g4", "+- hc g2 0");
			dictionary.Add("g5", "+- vc g3 0");
			dictionary.Add("g6", "+- g4 0 xPos");
			dictionary.Add("g7", "+- g5 0 yPos");
			dictionary.Add("g8", "mod g6 g7 0");
			dictionary.Add("g9", "*/ ss 6600 21600");
			dictionary.Add("g10", "+- g8 0 g9");
			dictionary.Add("g11", "*/ g10 1 3");
			dictionary.Add("g12", "*/ ss 1800 21600");
			dictionary.Add("g13", "+- g11 g12 0");
			dictionary.Add("g14", "*/ g13 g6 g8");
			dictionary.Add("g15", "*/ g13 g7 g8");
			dictionary.Add("g16", "+- g14 xPos 0");
			dictionary.Add("g17", "+- g15 yPos 0");
			dictionary.Add("g18", "*/ ss 4800 21600");
			dictionary.Add("g19", "*/ g11 2 1");
			dictionary.Add("g20", "+- g18 g19 0");
			dictionary.Add("g21", "*/ g20 g6 g8");
			dictionary.Add("g22", "*/ g20 g7 g8");
			dictionary.Add("g23", "+- g21 xPos 0");
			dictionary.Add("g24", "+- g22 yPos 0");
			dictionary.Add("g25", "*/ ss 1200 21600");
			dictionary.Add("g26", "*/ ss 600 21600");
			dictionary.Add("x23", "+- xPos g26 0");
			dictionary.Add("x24", "+- g16 g25 0");
			dictionary.Add("x25", "+- g23 g12 0");
			dictionary.Add("il", "*/ w 2977 21600");
			dictionary.Add("it", "*/ h 3262 21600");
			dictionary.Add("ir", "*/ w 17087 21600");
			dictionary.Add("ib", "*/ h 17337 21600");
			dictionary.Add("g27", "*/ w 67 21600");
			dictionary.Add("g28", "*/ h 21577 21600");
			dictionary.Add("g29", "*/ w 21582 21600");
			dictionary.Add("g30", "*/ h 1235 21600");
			dictionary.Add("pang", "at2 dxPos dyPos");
			break;
		case AutoShapeType.LineCallout1:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			break;
		case AutoShapeType.LineCallout2:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			break;
		case AutoShapeType.LineCallout3:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			dictionary.Add("y4", "*/ h adj7 100000");
			dictionary.Add("x4", "*/ w adj8 100000");
			break;
		case AutoShapeType.LineCallout1AccentBar:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			break;
		case AutoShapeType.LineCallout2AccentBar:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			break;
		case AutoShapeType.LineCallout3AccentBar:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			dictionary.Add("y4", "*/ h adj7 100000");
			dictionary.Add("x4", "*/ w adj8 100000");
			break;
		case AutoShapeType.LineCallout1NoBorder:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			break;
		case AutoShapeType.LineCallout2NoBorder:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			break;
		case AutoShapeType.LineCallout3NoBorder:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			dictionary.Add("y4", "*/ h adj7 100000");
			dictionary.Add("x4", "*/ w adj8 100000");
			break;
		case AutoShapeType.LineCallout1BorderAndAccentBar:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			break;
		case AutoShapeType.LineCallout2BorderAndAccentBar:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			break;
		case AutoShapeType.LineCallout3BorderAndAccentBar:
			dictionary.Add("y1", "*/ h adj1 100000");
			dictionary.Add("x1", "*/ w adj2 100000");
			dictionary.Add("y2", "*/ h adj3 100000");
			dictionary.Add("x2", "*/ w adj4 100000");
			dictionary.Add("y3", "*/ h adj5 100000");
			dictionary.Add("x3", "*/ w adj6 100000");
			dictionary.Add("y4", "*/ h adj7 100000");
			dictionary.Add("x4", "*/ w adj8 100000");
			break;
		}
		return dictionary;
	}

	private Dictionary<string, float> GetDefaultPathAdjValues(AutoShapeType shapeType)
	{
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		switch (shapeType)
		{
		case AutoShapeType.ElbowConnector:
		case AutoShapeType.CurvedConnector:
			dictionary.Add("adj1", 50000f);
			break;
		case AutoShapeType.BentConnector4:
		case AutoShapeType.CurvedConnector4:
			dictionary.Add("adj1", 50000f);
			dictionary.Add("adj2", 50000f);
			break;
		case AutoShapeType.BentConnector5:
		case AutoShapeType.CurvedConnector5:
			dictionary.Add("adj1", 50000f);
			dictionary.Add("adj2", 50000f);
			dictionary.Add("adj3", 50000f);
			break;
		case AutoShapeType.RoundedRectangle:
		case AutoShapeType.FoldedCorner:
		case AutoShapeType.DoubleBracket:
		case AutoShapeType.Plaque:
		case AutoShapeType.RoundSingleCornerRectangle:
		case AutoShapeType.SnipSingleCornerRectangle:
			dictionary.Add("adj", 16667f);
			break;
		case AutoShapeType.IsoscelesTriangle:
		case AutoShapeType.Moon:
		case AutoShapeType.Pentagon:
		case AutoShapeType.Chevron:
		case AutoShapeType.DiagonalStripe:
			dictionary.Add("adj", 50000f);
			break;
		case AutoShapeType.RoundSameSideCornerRectangle:
		case AutoShapeType.RoundDiagonalCornerRectangle:
		case AutoShapeType.SnipSameSideCornerRectangle:
			dictionary.Add("adj1", 16667f);
			dictionary.Add("adj2", 0f);
			break;
		case AutoShapeType.SnipDiagonalCornerRectangle:
			dictionary.Add("adj2", 16667f);
			dictionary.Add("adj1", 0f);
			break;
		case AutoShapeType.SnipAndRoundSingleCornerRectangle:
			dictionary.Add("adj1", 16667f);
			dictionary.Add("adj2", 16667f);
			break;
		case AutoShapeType.Parallelogram:
		case AutoShapeType.Trapezoid:
			dictionary.Add("adj", 25000f);
			break;
		case AutoShapeType.RegularPentagon:
			dictionary.Add("hf", 105146f);
			dictionary.Add("vf", 110557f);
			break;
		case AutoShapeType.Hexagon:
			dictionary.Add("adj", 25000f);
			dictionary.Add("vf", 115470f);
			break;
		case AutoShapeType.Heptagon:
			dictionary.Add("hf", 102572f);
			dictionary.Add("vf", 105210f);
			break;
		case AutoShapeType.Octagon:
			dictionary.Add("adj", 29289f);
			break;
		case AutoShapeType.Decagon:
			dictionary.Add("vf", 105146f);
			break;
		case AutoShapeType.Pie:
			dictionary.Add("adj1", 0f);
			dictionary.Add("adj2", 270f);
			break;
		case AutoShapeType.Chord:
			dictionary.Add("adj1", 45f);
			dictionary.Add("adj2", 270f);
			break;
		case AutoShapeType.Teardrop:
			dictionary.Add("adj", 100000f);
			break;
		case AutoShapeType.Frame:
			dictionary.Add("adj1", 12500f);
			break;
		case AutoShapeType.HalfFrame:
			dictionary.Add("adj1", 33333f);
			dictionary.Add("adj2", 33333f);
			break;
		case AutoShapeType.RightArrow:
		case AutoShapeType.LeftArrow:
		case AutoShapeType.UpArrow:
		case AutoShapeType.DownArrow:
		case AutoShapeType.LeftRightArrow:
		case AutoShapeType.UpDownArrow:
		case AutoShapeType.StripedRightArrow:
		case AutoShapeType.NotchedRightArrow:
		case AutoShapeType.L_Shape:
			dictionary.Add("adj1", 50000f);
			dictionary.Add("adj2", 50000f);
			break;
		case AutoShapeType.Cross:
		case AutoShapeType.Can:
		case AutoShapeType.Cube:
		case AutoShapeType.Donut:
		case AutoShapeType.Sun:
			dictionary.Add("adj", 25000f);
			break;
		case AutoShapeType.Bevel:
		case AutoShapeType.Star4Point:
		case AutoShapeType.VerticalScroll:
		case AutoShapeType.HorizontalScroll:
			dictionary.Add("adj", 12500f);
			break;
		case AutoShapeType.NoSymbol:
			dictionary.Add("adj", 18750f);
			break;
		case AutoShapeType.BlockArc:
			dictionary.Add("adj1", 10800000f);
			dictionary.Add("adj2", 0f);
			dictionary.Add("adj3", 25000f);
			break;
		case AutoShapeType.SmileyFace:
			dictionary.Add("adj", 4653f);
			break;
		case AutoShapeType.Arc:
			dictionary.Add("adj1", 16200000f);
			dictionary.Add("adj2", 0f);
			break;
		case AutoShapeType.DoubleBrace:
		case AutoShapeType.LeftBracket:
		case AutoShapeType.RightBracket:
			dictionary.Add("adj", 8333f);
			break;
		case AutoShapeType.LeftBrace:
		case AutoShapeType.RightBrace:
			dictionary.Add("adj1", 8333f);
			dictionary.Add("adj2", 50000f);
			break;
		case AutoShapeType.QuadArrow:
		case AutoShapeType.LeftRightUpArrow:
			dictionary.Add("adj1", 22500f);
			dictionary.Add("adj2", 22500f);
			dictionary.Add("adj3", 22500f);
			break;
		case AutoShapeType.BentArrow:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 25000f);
			dictionary.Add("adj3", 25000f);
			dictionary.Add("adj4", 43750f);
			break;
		case AutoShapeType.UTurnArrow:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 25000f);
			dictionary.Add("adj3", 25000f);
			dictionary.Add("adj4", 43750f);
			dictionary.Add("adj5", 75000f);
			break;
		case AutoShapeType.LeftUpArrow:
		case AutoShapeType.BentUpArrow:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 25000f);
			dictionary.Add("adj3", 25000f);
			break;
		case AutoShapeType.CurvedRightArrow:
		case AutoShapeType.CurvedLeftArrow:
		case AutoShapeType.CurvedUpArrow:
		case AutoShapeType.CurvedDownArrow:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 50000f);
			dictionary.Add("adj3", 25000f);
			break;
		case AutoShapeType.RightArrowCallout:
		case AutoShapeType.LeftArrowCallout:
		case AutoShapeType.UpArrowCallout:
		case AutoShapeType.DownArrowCallout:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 25000f);
			dictionary.Add("adj3", 25000f);
			dictionary.Add("adj4", 64977f);
			break;
		case AutoShapeType.LeftRightArrowCallout:
		case AutoShapeType.UpDownArrowCallout:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 25000f);
			dictionary.Add("adj3", 25000f);
			dictionary.Add("adj4", 48123f);
			break;
		case AutoShapeType.QuadArrowCallout:
			dictionary.Add("adj1", 18515f);
			dictionary.Add("adj2", 18515f);
			dictionary.Add("adj3", 18515f);
			dictionary.Add("adj4", 48123f);
			break;
		case AutoShapeType.CircularArrow:
			dictionary.Add("adj1", 12500f);
			dictionary.Add("adj2", 19f);
			dictionary.Add("adj3", 341f);
			dictionary.Add("adj4", 180f);
			dictionary.Add("adj5", 12500f);
			break;
		case AutoShapeType.MathPlus:
		case AutoShapeType.MathMinus:
		case AutoShapeType.MathMultiply:
			dictionary.Add("adj1", 23520f);
			break;
		case AutoShapeType.MathDivision:
			dictionary.Add("adj1", 23520f);
			dictionary.Add("adj2", 5880f);
			dictionary.Add("adj3", 11760f);
			break;
		case AutoShapeType.MathEqual:
			dictionary.Add("adj1", 23520f);
			dictionary.Add("adj2", 11760f);
			break;
		case AutoShapeType.MathNotEqual:
			dictionary.Add("adj1", 23520f);
			dictionary.Add("adj2", 6600000f);
			dictionary.Add("adj3", 11760f);
			break;
		case AutoShapeType.Star5Point:
			dictionary.Add("adj", 19098f);
			dictionary.Add("hf", 105146f);
			dictionary.Add("vf", 110557f);
			break;
		case AutoShapeType.Star6Point:
			dictionary.Add("adj", 28868f);
			dictionary.Add("hf", 115470f);
			break;
		case AutoShapeType.Star7Point:
			dictionary.Add("adj", 34601f);
			dictionary.Add("hf", 102572f);
			dictionary.Add("vf", 105210f);
			break;
		case AutoShapeType.Star8Point:
		case AutoShapeType.Star16Point:
		case AutoShapeType.Star24Point:
		case AutoShapeType.Star32Point:
		case AutoShapeType.Star12Point:
			dictionary.Add("adj", 37500f);
			break;
		case AutoShapeType.Star10Point:
			dictionary.Add("adj", 42533f);
			dictionary.Add("hf", 105146f);
			break;
		case AutoShapeType.UpRibbon:
		case AutoShapeType.DownRibbon:
			dictionary.Add("adj1", 16667f);
			dictionary.Add("adj2", 50000f);
			break;
		case AutoShapeType.CurvedUpRibbon:
		case AutoShapeType.CurvedDownRibbon:
			dictionary.Add("adj1", 25000f);
			dictionary.Add("adj2", 50000f);
			dictionary.Add("adj3", 12500f);
			break;
		case AutoShapeType.Wave:
			dictionary.Add("adj1", 12500f);
			dictionary.Add("adj2", 0f);
			break;
		case AutoShapeType.DoubleWave:
			dictionary.Add("adj1", 6250f);
			dictionary.Add("adj2", 0f);
			break;
		case AutoShapeType.RectangularCallout:
		case AutoShapeType.OvalCallout:
		case AutoShapeType.CloudCallout:
			dictionary.Add("adj1", -20833f);
			dictionary.Add("adj2", 62500f);
			break;
		case AutoShapeType.RoundedRectangularCallout:
			dictionary.Add("adj1", -20833f);
			dictionary.Add("adj2", 62500f);
			dictionary.Add("adj3", 16667f);
			break;
		case AutoShapeType.LineCallout1:
		case AutoShapeType.LineCallout1NoBorder:
		case AutoShapeType.LineCallout1AccentBar:
		case AutoShapeType.LineCallout1BorderAndAccentBar:
			dictionary.Add("adj1", 18750f);
			dictionary.Add("adj2", -8333f);
			dictionary.Add("adj3", 112500f);
			dictionary.Add("adj4", -38333f);
			break;
		case AutoShapeType.LineCallout2:
		case AutoShapeType.LineCallout2AccentBar:
		case AutoShapeType.LineCallout2NoBorder:
		case AutoShapeType.LineCallout2BorderAndAccentBar:
			dictionary.Add("adj1", 18750f);
			dictionary.Add("adj2", -8333f);
			dictionary.Add("adj3", 18750f);
			dictionary.Add("adj4", -16667f);
			dictionary.Add("adj5", 112500f);
			dictionary.Add("adj6", -46667f);
			break;
		case AutoShapeType.LineCallout3:
		case AutoShapeType.LineCallout3AccentBar:
		case AutoShapeType.LineCallout3NoBorder:
		case AutoShapeType.LineCallout3BorderAndAccentBar:
			dictionary.Add("adj1", 18750f);
			dictionary.Add("adj2", -8333f);
			dictionary.Add("adj3", 18750f);
			dictionary.Add("adj4", -16667f);
			dictionary.Add("adj5", 100000f);
			dictionary.Add("adj6", -16667f);
			dictionary.Add("adj7", 112963f);
			dictionary.Add("adj8", -8333f);
			break;
		}
		return dictionary;
	}

	internal void Close()
	{
		if (_shapeGuide != null)
		{
			_shapeGuide.Clear();
			_shapeGuide = null;
		}
	}
}
