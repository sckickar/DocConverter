using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Esprima.Ast;

public static class NodeExtensions
{
	[DebuggerStepThrough]
	public static T As<T>(this object node) where T : class
	{
		return (T)node;
	}

	public static IEnumerable<INode> DescendantNodesAndSelf(this INode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return DescendantNodes(new List<INode> { node });
	}

	public static IEnumerable<INode> DescendantNodes(this INode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return DescendantNodes(new List<INode>(node.ChildNodes));
	}

	private static IEnumerable<INode> DescendantNodes(List<INode> nodes)
	{
		while (nodes.Count > 0)
		{
			INode node = nodes[0];
			nodes.RemoveAt(0);
			yield return node;
			nodes.InsertRange(0, node.ChildNodes);
		}
	}

	public static IEnumerable<INode> AncestorNodesAndSelf(this INode node, INode rootNode)
	{
		using IEnumerator<INode> ancestor = node.AncestorNodes(rootNode).GetEnumerator();
		if (ancestor.MoveNext())
		{
			yield return node;
			do
			{
				yield return ancestor.Current;
			}
			while (ancestor.MoveNext());
		}
	}

	public static IEnumerable<INode> AncestorNodes(this INode node, INode rootNode)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (rootNode == null)
		{
			throw new ArgumentNullException("rootNode");
		}
		Stack<INode> parents = new Stack<INode>();
		Search(rootNode);
		return parents;
		bool Search(INode aNode)
		{
			parents.Push(aNode);
			foreach (INode childNode in aNode.ChildNodes)
			{
				if (childNode == node)
				{
					return true;
				}
				if (Search(childNode))
				{
					return true;
				}
			}
			parents.Pop();
			return false;
		}
	}
}
