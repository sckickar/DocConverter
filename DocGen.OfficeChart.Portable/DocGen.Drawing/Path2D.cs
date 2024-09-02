using System.Collections.Generic;

namespace DocGen.Drawing;

internal class Path2D
{
	internal enum Path2DElements : ushort
	{
		Close = 1,
		MoveTo,
		LineTo,
		ArcTo,
		QuadBezTo,
		CubicBezTo
	}

	private List<double> _pathElementList;

	private double _width;

	private double _height;

	private bool _isStroke = true;

	internal List<double> PathElements => _pathElementList ?? (_pathElementList = new List<double>());

	internal double Width
	{
		get
		{
			return _width;
		}
		set
		{
			_width = value;
		}
	}

	internal double Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	internal bool IsStroke
	{
		get
		{
			return _isStroke;
		}
		set
		{
			_isStroke = value;
		}
	}

	internal void Close()
	{
		if (_pathElementList != null)
		{
			_pathElementList.Clear();
			_pathElementList = null;
		}
	}
}
