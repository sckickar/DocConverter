using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

internal class FOPTEBid : FOPTEBase
{
	private uint m_value;

	internal uint Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	internal FOPTEBid(int id, bool isBid, uint value)
		: base(id, isBid)
	{
		m_value = value;
	}

	internal override void Write(Stream stream)
	{
		int id = base.Id;
		id |= (base.IsBid ? 16384 : 0);
		BaseWordRecord.WriteInt16(stream, (short)id);
		BaseWordRecord.WriteUInt32(stream, m_value);
	}

	internal override FOPTEBase Clone()
	{
		return new FOPTEBid(base.Id, base.IsBid, Value);
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = (base.IsBid ? "1" : "0");
		stringBuilder.Append(text + ";");
		stringBuilder.Append(Value + ";");
		stringBuilder.Append(base.Id + ";");
		return stringBuilder;
	}
}
