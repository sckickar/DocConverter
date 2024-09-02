namespace DocGen.Chart;

internal class Vector3DWithIndexWithClassification
{
	private Vector3D v;

	private int index;

	private ClassifyPointResult result;

	private bool isCuttingBackPoint;

	private int cuttingBackPairIndex;

	private bool alreadyCuttedBack;

	private bool isCuttingFrontPoint;

	private int cuttingFrontPairIndex;

	private bool alreadyCuttedFront;

	public Vector3D Vector
	{
		get
		{
			return v;
		}
		set
		{
			v = value;
		}
	}

	public int Index
	{
		get
		{
			return index;
		}
		set
		{
			if (index != value)
			{
				index = value;
			}
		}
	}

	internal ClassifyPointResult Result
	{
		get
		{
			return result;
		}
		set
		{
			if (result != value)
			{
				result = value;
			}
		}
	}

	public bool CuttingBackPoint
	{
		get
		{
			return isCuttingBackPoint;
		}
		set
		{
			if (isCuttingBackPoint != value)
			{
				isCuttingBackPoint = value;
			}
		}
	}

	public bool CuttingFrontPoint
	{
		get
		{
			return isCuttingFrontPoint;
		}
		set
		{
			if (isCuttingFrontPoint != value)
			{
				isCuttingFrontPoint = value;
			}
		}
	}

	public int CuttingBackPairIndex
	{
		get
		{
			return cuttingBackPairIndex;
		}
		set
		{
			if (cuttingBackPairIndex != value)
			{
				cuttingBackPairIndex = value;
			}
		}
	}

	public int CuttingFrontPairIndex
	{
		get
		{
			return cuttingFrontPairIndex;
		}
		set
		{
			if (cuttingFrontPairIndex != value)
			{
				cuttingFrontPairIndex = value;
			}
		}
	}

	public bool AlreadyCuttedBack
	{
		get
		{
			return alreadyCuttedBack;
		}
		set
		{
			if (alreadyCuttedBack != value)
			{
				alreadyCuttedBack = value;
			}
		}
	}

	public bool AlreadyCuttedFront
	{
		get
		{
			return alreadyCuttedFront;
		}
		set
		{
			if (alreadyCuttedFront != value)
			{
				alreadyCuttedFront = value;
			}
		}
	}

	public Vector3DWithIndexWithClassification(Vector3D point, int ind, ClassifyPointResult res)
	{
		v = point;
		index = ind;
		result = res;
	}

	public Vector3DWithIndexWithClassification(Vector3DWithIndexWithClassification vectWW)
	{
		v = vectWW.Vector;
		index = vectWW.Index;
		result = vectWW.Result;
		isCuttingBackPoint = vectWW.CuttingBackPoint;
		cuttingBackPairIndex = vectWW.cuttingBackPairIndex;
		alreadyCuttedBack = vectWW.AlreadyCuttedBack;
		isCuttingFrontPoint = vectWW.CuttingFrontPoint;
		cuttingFrontPairIndex = vectWW.cuttingFrontPairIndex;
		alreadyCuttedFront = vectWW.AlreadyCuttedFront;
	}
}
