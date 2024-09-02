using System.Collections;

namespace DocGen.Chart;

internal class PointsOnLineComparer : IComparer
{
	private Vector3D dir;

	private Vector3D p;

	private const double EPSILON = 0.0001;

	public PointsOnLineComparer(Vector3D direction, Vector3D point)
	{
		dir = direction;
		p = point;
	}

	int IComparer.Compare(object x, object y)
	{
		Vector3D vector3D = ((Vector3DWithIndexWithClassification)x).Vector - p;
		Vector3D vector3D2 = ((Vector3DWithIndexWithClassification)y).Vector - p;
		double num = vector3D & dir;
		double num2 = vector3D2 & dir;
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}
}
