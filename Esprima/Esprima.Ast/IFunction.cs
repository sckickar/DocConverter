namespace Esprima.Ast;

public interface IFunction : INode
{
	Identifier Id { get; }

	ref readonly NodeList<INode> Params { get; }

	INode Body { get; }

	bool Generator { get; }

	bool Expression { get; }

	bool Strict { get; }

	bool Async { get; }

	HoistingScope HoistingScope { get; }
}
