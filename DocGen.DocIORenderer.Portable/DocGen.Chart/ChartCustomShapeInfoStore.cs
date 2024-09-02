using DocGen.Drawing;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartCustomShapeInfoStore : StyleInfoStore
{
	private static StaticData sd = new StaticData(typeof(ChartCustomShapeInfoStore), typeof(ChartCustomShapeInfo), sortProperties: true);

	public static StyleInfoProperty ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");

	public static StyleInfoProperty ShapeTypeProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "ShapeType");

	public static StyleInfoProperty BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "Border");

	public static StyleInfoProperty BorderWidthProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "BorderWidth");

	public static StyleInfoProperty BorderColorProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "BorderColor");

	protected override StaticData StaticDataStore => sd;

	internal static ChartCustomShapeInfoStore InitializeStaticVariables()
	{
		if (sd == null)
		{
			sd = new StaticData(typeof(ChartCustomShapeInfoStore), typeof(ChartCustomShapeInfo), sortProperties: true);
			ColorProperty = sd.CreateStyleInfoProperty(typeof(Color), "Color");
			ShapeTypeProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "ShapeType");
			BorderProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "Border");
			BorderColorProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "BorderColor");
			BorderWidthProperty = sd.CreateStyleInfoProperty(typeof(ChartCustomShape), "BorderWidth");
		}
		return new ChartCustomShapeInfoStore();
	}

	public new void Dispose()
	{
		if (sd != null)
		{
			sd.Dispose();
			sd = null;
		}
		if (ColorProperty != null)
		{
			ColorProperty.Dispose();
			ColorProperty = null;
		}
		if (ShapeTypeProperty != null)
		{
			ShapeTypeProperty.Dispose();
			ShapeTypeProperty = null;
		}
		if (BorderProperty != null)
		{
			BorderProperty.Dispose();
			BorderProperty = null;
		}
		if (BorderWidthProperty != null)
		{
			BorderWidthProperty.Dispose();
			BorderWidthProperty = null;
		}
		if (BorderColorProperty != null)
		{
			BorderColorProperty.Dispose();
			BorderColorProperty = null;
		}
		base.Dispose();
	}
}
