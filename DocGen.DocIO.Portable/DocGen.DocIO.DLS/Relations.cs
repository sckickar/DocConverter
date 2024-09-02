using System.IO;
using DocGen.Compression.Zip;

namespace DocGen.DocIO.DLS;

internal class Relations : Part
{
	public Relations(ZipArchiveItem item)
		: base(item.DataStream)
	{
		m_name = item.ItemName;
	}

	internal Relations(Stream dataStream, string name)
		: base(dataStream)
	{
		m_name = name;
	}

	internal new Part Clone()
	{
		return new Relations(m_dataStream, m_name);
	}

	internal new void Close()
	{
		base.Close();
	}
}
