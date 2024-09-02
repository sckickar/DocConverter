using System.Collections.Generic;

namespace DocGen.PdfViewer.Base;

internal class KeyPrivate : PostScriptObj
{
	private readonly KeyProperty<PostScriptArray> subrs;

	private readonly KeyProperty<PostScriptArray> otherSubrs;

	private readonly Dictionary<int, byte[]> subroutines;

	public PostScriptArray Subrs => subrs.GetValue();

	public PostScriptArray OtherSubrs => otherSubrs.GetValue();

	public KeyPrivate()
	{
		subrs = CreateProperty<PostScriptArray>(new KeyPropertyDescriptor
		{
			Name = "Subrs"
		});
		otherSubrs = CreateProperty<PostScriptArray>(new KeyPropertyDescriptor
		{
			Name = "OtherSubrs"
		});
		subroutines = new Dictionary<int, byte[]>();
	}

	public byte[] GetSubr(int index)
	{
		if (!subroutines.TryGetValue(index, out byte[] value))
		{
			value = Subrs.GetElementAs<PostScriptStrHelper>(index).ToByteArray();
			subroutines[index] = value;
		}
		return value;
	}
}
