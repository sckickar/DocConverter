namespace DocGen.DocIO.DLS;

public interface ICompositeEntity : IEntity
{
	EntityCollection ChildEntities { get; }
}
