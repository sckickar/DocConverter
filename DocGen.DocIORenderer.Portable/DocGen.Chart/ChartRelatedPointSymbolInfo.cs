using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

[ImmutableObject(true)]
internal class ChartRelatedPointSymbolInfo
{
	private ChartSymbolShape shape;

	private int imageIndex;

	private Color color;

	private Size size;

	public ChartSymbolShape Shape => shape;

	public int ImageIndex => imageIndex;

	public Color Color => color;

	public Size Size => size;

	public ChartRelatedPointSymbolInfo(ChartSymbolShape shape, int imageIndex, Color color, Size size)
	{
		this.shape = shape;
		this.imageIndex = imageIndex;
		this.color = color;
		this.size = size;
	}
}
