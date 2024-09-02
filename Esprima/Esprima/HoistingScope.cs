using Esprima.Ast;

namespace Esprima;

public class HoistingScope
{
	private readonly NodeList<IFunctionDeclaration> _functionDeclarations;

	private readonly NodeList<VariableDeclaration> _variableDeclarations;

	public ref readonly NodeList<IFunctionDeclaration> FunctionDeclarations => ref _functionDeclarations;

	public ref readonly NodeList<VariableDeclaration> VariableDeclarations => ref _variableDeclarations;

	public HoistingScope()
	{
		NodeList<IFunctionDeclaration> functionDeclarations = default(NodeList<IFunctionDeclaration>);
		NodeList<VariableDeclaration> variableDeclarations = default(NodeList<VariableDeclaration>);
		this._002Ector(in functionDeclarations, in variableDeclarations);
	}

	public HoistingScope(in NodeList<IFunctionDeclaration> functionDeclarations, in NodeList<VariableDeclaration> variableDeclarations)
	{
		_functionDeclarations = functionDeclarations;
		_variableDeclarations = variableDeclarations;
	}
}
