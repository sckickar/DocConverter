using System;

namespace DocGen.Layouting;

internal class UIDispatcher
{
	internal static void Execute(Action action)
	{
		action();
	}
}
