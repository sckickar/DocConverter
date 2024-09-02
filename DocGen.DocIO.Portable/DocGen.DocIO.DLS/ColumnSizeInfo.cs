namespace DocGen.DocIO.DLS;

internal class ColumnSizeInfo
{
	private float minimumWordWidth;

	private float maximumWordWidth;

	private float minimumWidth;

	private float maxParaWidth;

	private bool hasMinimumWidth;

	private bool hasMinimumWordWidth;

	private bool hasMaximumWordWidth;

	internal bool HasMinimumWidth
	{
		get
		{
			return hasMinimumWidth;
		}
		set
		{
			hasMinimumWidth = value;
		}
	}

	internal bool HasMinimumMaximumWordWidth
	{
		get
		{
			return hasMinimumWordWidth;
		}
		set
		{
			hasMinimumWordWidth = value;
		}
	}

	internal bool HasMaximumWordWidth
	{
		get
		{
			return hasMaximumWordWidth;
		}
		set
		{
			hasMaximumWordWidth = value;
		}
	}

	internal float MinimumWordWidth
	{
		get
		{
			return minimumWordWidth;
		}
		set
		{
			minimumWordWidth = value;
			HasMinimumMaximumWordWidth = true;
		}
	}

	internal float MaximumWordWidth
	{
		get
		{
			return maximumWordWidth;
		}
		set
		{
			maximumWordWidth = value;
			HasMinimumMaximumWordWidth = true;
		}
	}

	internal float MinimumWidth
	{
		get
		{
			return minimumWidth;
		}
		set
		{
			minimumWidth = value;
			HasMinimumWidth = true;
		}
	}

	internal float MaxParaWidth
	{
		get
		{
			return maxParaWidth;
		}
		set
		{
			maxParaWidth = value;
		}
	}
}
