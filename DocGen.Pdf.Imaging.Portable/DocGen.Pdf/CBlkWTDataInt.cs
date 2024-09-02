namespace DocGen.Pdf;

internal class CBlkWTDataInt : CBlkWTData
{
	public int[] data_array;

	public override int DataType => 3;

	public override object Data
	{
		get
		{
			return data_array;
		}
		set
		{
			data_array = (int[])value;
		}
	}

	public virtual int[] DataInt
	{
		get
		{
			return data_array;
		}
		set
		{
			data_array = value;
		}
	}
}
