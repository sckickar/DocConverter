using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

internal abstract class JBIG2BaseFlags
{
	protected internal int flagsAsInt;

	protected internal IDictionary flags = new Dictionary<string, int>();

	public int GetFlagValue(string key)
	{
		return ((int?)flags[key]).Value;
	}

	internal abstract void setFlags(int flagsAsInt);
}
