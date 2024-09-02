namespace DocGen.DocIO.DLS;

public interface IBlockContentControl : ICompositeEntity, IEntity
{
	ContentControlProperties ContentControlProperties { get; }

	WCharacterFormat BreakCharacterFormat { get; }

	WTextBody TextBody { get; }
}
