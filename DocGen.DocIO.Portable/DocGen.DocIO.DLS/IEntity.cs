namespace DocGen.DocIO.DLS;

public interface IEntity
{
	WordDocument Document { get; }

	Entity Owner { get; }

	EntityType EntityType { get; }

	IEntity NextSibling { get; }

	IEntity PreviousSibling { get; }

	bool IsComposite { get; }

	Entity Clone();
}
