using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class ByteEncodingCollectionBase : List<ByteEncodingBase>
{
	public ByteEncodingBase FindEncoding(byte b0)
	{
		using (List<ByteEncodingBase>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ByteEncodingBase current = enumerator.Current;
				if (current.IsInRange(b0))
				{
					return current;
				}
			}
		}
		return null;
	}
}
