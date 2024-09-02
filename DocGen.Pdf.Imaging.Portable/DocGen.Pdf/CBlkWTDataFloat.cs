namespace DocGen.Pdf;

internal class CBlkWTDataFloat : CBlkWTData
{
	private float[] data;

	public override int DataType => 4;

	public override object Data
	{
		get
		{
			return data;
		}
		set
		{
			data = (float[])value;
		}
	}

	public virtual float[] DataFloat
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}
}
