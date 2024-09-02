using System.IO;
using System.Text;

namespace DocGen.Office;

internal abstract class ReferenceRecord
{
	internal abstract string Name { get; set; }

	internal abstract Encoding EncodingType { get; set; }

	internal abstract void ParseRecord(Stream dirData);

	internal abstract void SerializeRecord(Stream dirData);

	internal virtual ReferenceRecord Clone()
	{
		return (ReferenceRecord)MemberwiseClone();
	}
}
