using System.Collections.Generic;

namespace Esprima.Ast;

internal static class ChildNodeYielder
{
	public static IEnumerable<INode> Yield(INode first, INode second = null, INode third = null, INode fourth = null)
	{
		if (first != null)
		{
			yield return first;
		}
		if (second != null)
		{
			yield return second;
		}
		if (third != null)
		{
			yield return third;
		}
		if (fourth != null)
		{
			yield return fourth;
		}
	}

	public static IEnumerable<INode> Yield<T>(NodeList<T> first, INode second = null) where T : class, INode
	{
		foreach (T item in first)
		{
			yield return item;
		}
		if (second != null)
		{
			yield return second;
		}
	}

	public static IEnumerable<INode> Yield<T1, T2>(NodeList<T1> first, NodeList<T2> second) where T1 : class, INode where T2 : class, INode
	{
		foreach (T1 item in first)
		{
			yield return item;
		}
		foreach (T2 item2 in second)
		{
			yield return item2;
		}
	}

	public static IEnumerable<INode> Yield<T>(INode first, NodeList<T> second, INode third = null) where T : class, INode
	{
		if (first != null)
		{
			yield return first;
		}
		foreach (T item in second)
		{
			yield return item;
		}
		if (third != null)
		{
			yield return third;
		}
	}
}
