using System;
using System.Collections.Generic;

namespace Esprima.Ast;

public class UnaryExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly UnaryOperator Operator;

	public readonly Expression Argument;

	public bool Prefix { get; protected set; }

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Argument);

	public static UnaryOperator ParseUnaryOperator(string op)
	{
		return op switch
		{
			"+" => UnaryOperator.Plus, 
			"-" => UnaryOperator.Minus, 
			"++" => UnaryOperator.Increment, 
			"--" => UnaryOperator.Decrement, 
			"~" => UnaryOperator.BitwiseNot, 
			"!" => UnaryOperator.LogicalNot, 
			"delete" => UnaryOperator.Delete, 
			"void" => UnaryOperator.Void, 
			"typeof" => UnaryOperator.TypeOf, 
			_ => throw new ArgumentOutOfRangeException("Invalid unary operator: " + op), 
		};
	}

	public UnaryExpression(string op, Expression arg)
		: this(Nodes.UnaryExpression, op, arg)
	{
	}

	protected UnaryExpression(Nodes type, string op, Expression arg)
		: base(type)
	{
		Operator = ParseUnaryOperator(op);
		Argument = arg;
		Prefix = true;
	}
}
