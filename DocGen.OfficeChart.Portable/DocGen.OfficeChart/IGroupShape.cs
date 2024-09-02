namespace DocGen.OfficeChart;

internal interface IGroupShape : IShape, IParentApplication
{
	IShape[] Items { get; }
}
