using Esprima.Ast;

namespace Esprima;

internal static class ArrayListExtensions
{
	public static void AddRange<T>(this ref ArrayList<T> destination, in NodeList<T> source) where T : class, INode
	{
		foreach (T item in source)
		{
			destination.Add(item);
		}
	}
}
