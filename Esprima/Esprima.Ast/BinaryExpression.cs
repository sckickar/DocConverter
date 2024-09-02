using System;
using System.Collections.Generic;

namespace Esprima.Ast;

public class BinaryExpression : Node, Expression, INode, PropertyValue, IDeclaration, IStatementListItem, ArgumentListElement, ArrayExpressionElement
{
	public readonly BinaryOperator Operator;

	public readonly Expression Left;

	public readonly Expression Right;

	public override IEnumerable<INode> ChildNodes => ChildNodeYielder.Yield(Left, Right);

	public BinaryExpression(string op, Expression left, Expression right)
		: this(ParseBinaryOperator(op), left, right)
	{
	}

	private BinaryExpression(BinaryOperator op, Expression left, Expression right)
		: base((op == BinaryOperator.LogicalAnd || op == BinaryOperator.LogicalOr) ? Nodes.LogicalExpression : Nodes.BinaryExpression)
	{
		Operator = op;
		Left = left;
		Right = right;
	}

	public static BinaryOperator ParseBinaryOperator(string op)
	{
		return op switch
		{
			"+" => BinaryOperator.Plus, 
			"-" => BinaryOperator.Minus, 
			"*" => BinaryOperator.Times, 
			"/" => BinaryOperator.Divide, 
			"%" => BinaryOperator.Modulo, 
			"==" => BinaryOperator.Equal, 
			"!=" => BinaryOperator.NotEqual, 
			">" => BinaryOperator.Greater, 
			">=" => BinaryOperator.GreaterOrEqual, 
			"<" => BinaryOperator.Less, 
			"<=" => BinaryOperator.LessOrEqual, 
			"===" => BinaryOperator.StrictlyEqual, 
			"!==" => BinaryOperator.StricltyNotEqual, 
			"&" => BinaryOperator.BitwiseAnd, 
			"|" => BinaryOperator.BitwiseOr, 
			"^" => BinaryOperator.BitwiseXOr, 
			"<<" => BinaryOperator.LeftShift, 
			">>" => BinaryOperator.RightShift, 
			">>>" => BinaryOperator.UnsignedRightShift, 
			"instanceof" => BinaryOperator.InstanceOf, 
			"in" => BinaryOperator.In, 
			"&&" => BinaryOperator.LogicalAnd, 
			"||" => BinaryOperator.LogicalOr, 
			"**" => BinaryOperator.Exponentiation, 
			_ => throw new ArgumentOutOfRangeException("Invalid binary operator: " + op), 
		};
	}
}
