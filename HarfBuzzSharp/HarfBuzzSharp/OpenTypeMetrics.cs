using System;

namespace HarfBuzzSharp;

public class OpenTypeMetrics
{
	private readonly Font font;

	public OpenTypeMetrics(Font font)
	{
		this.font = font ?? throw new ArgumentNullException("font");
	}

	public unsafe bool TryGetPosition(OpenTypeMetricsTag metricsTag, out int position)
	{
		fixed (int* position2 = &position)
		{
			return HarfBuzzApi.hb_ot_metrics_get_position(font.Handle, metricsTag, position2);
		}
	}

	public float GetVariation(OpenTypeMetricsTag metricsTag)
	{
		return HarfBuzzApi.hb_ot_metrics_get_variation(font.Handle, metricsTag);
	}

	public int GetXVariation(OpenTypeMetricsTag metricsTag)
	{
		return HarfBuzzApi.hb_ot_metrics_get_x_variation(font.Handle, metricsTag);
	}

	public int GetYVariation(OpenTypeMetricsTag metricsTag)
	{
		return HarfBuzzApi.hb_ot_metrics_get_y_variation(font.Handle, metricsTag);
	}
}
