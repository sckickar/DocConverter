namespace DocGen.Chart.Renderers;

internal class PolygonWithTangent
{
	private Polygon poly;

	private double tangent;

	public Polygon Polygon
	{
		get
		{
			return poly;
		}
		set
		{
			poly = value;
		}
	}

	public double Tangent
	{
		get
		{
			return tangent;
		}
		set
		{
			if (tangent != value)
			{
				tangent = value;
			}
		}
	}

	public PolygonWithTangent(Polygon p, double tan)
	{
		poly = p;
		tangent = tan;
	}
}
