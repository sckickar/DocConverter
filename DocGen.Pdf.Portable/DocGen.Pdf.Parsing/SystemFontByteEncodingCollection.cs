using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontByteEncodingCollection : List<SystemFontByteEncoding>
{
	public SystemFontByteEncoding FindEncoding(byte b0)
	{
		using (List<SystemFontByteEncoding>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SystemFontByteEncoding current = enumerator.Current;
				if (current.IsInRange(b0))
				{
					return current;
				}
			}
		}
		return null;
	}
}
