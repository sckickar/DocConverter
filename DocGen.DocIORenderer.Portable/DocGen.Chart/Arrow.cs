using DocGen.OfficeChart;

namespace DocGen.Chart;

internal class Arrow
{
	private OfficeArrowType _type;

	private float _arrowWidth = 7f;

	private float _arrowLength = 7f;

	internal OfficeArrowType Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	internal float ArrowWidth
	{
		get
		{
			return _arrowWidth;
		}
		set
		{
			_arrowWidth = value;
		}
	}

	internal float ArrowLength
	{
		get
		{
			return _arrowLength;
		}
		set
		{
			_arrowLength = value;
		}
	}
}
