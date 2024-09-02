namespace DocGen.Pdf.Barcode;

public sealed class PdfBarcodeQuietZones
{
	private const float DEF_MARGIN = 0f;

	private float right;

	private float top;

	private float left;

	private float bottom;

	public float Right
	{
		get
		{
			return right;
		}
		set
		{
			right = value;
		}
	}

	public float Top
	{
		get
		{
			return top;
		}
		set
		{
			top = value;
		}
	}

	public float Left
	{
		get
		{
			return left;
		}
		set
		{
			left = value;
		}
	}

	public float Bottom
	{
		get
		{
			return bottom;
		}
		set
		{
			bottom = value;
		}
	}

	public float All
	{
		get
		{
			return right;
		}
		set
		{
			right = (top = (left = (bottom = value)));
		}
	}

	public bool IsAll
	{
		get
		{
			if (left == top && left == right)
			{
				return left == bottom;
			}
			return false;
		}
	}
}
