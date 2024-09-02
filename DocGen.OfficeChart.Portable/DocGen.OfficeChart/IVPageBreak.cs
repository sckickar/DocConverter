namespace DocGen.OfficeChart;

internal interface IVPageBreak
{
	IApplication Application { get; }

	IRange Location { get; set; }

	object Parent { get; }
}
