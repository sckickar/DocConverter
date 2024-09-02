using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

[TypeConverter(typeof(ExpandableObjectConverter))]
internal class ChartMarker
{
	private LineCap lineCap;

	private ChartLineInfo lineInfo = ChartLineInfo.CreateDefault();

	public LineCap LineCap
	{
		get
		{
			return lineCap;
		}
		set
		{
			lineCap = value;
		}
	}

	public ChartLineInfo LineInfo
	{
		get
		{
			return lineInfo;
		}
		set
		{
			lineInfo = value;
		}
	}

	public ChartMarker()
	{
		lineInfo.Width = 5f;
	}

	public void Dispose()
	{
		lineInfo.Dispose();
		lineInfo = null;
	}
}
