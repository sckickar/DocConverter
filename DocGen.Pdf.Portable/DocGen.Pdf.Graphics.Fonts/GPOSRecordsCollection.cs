using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class GPOSRecordsCollection
{
	internal IDictionary<int, GPOSRecord> Records;

	internal IDictionary<int, GPOSValueRecord[]> Collection;

	internal IDictionary<int, IList<GPOSValueRecord[]>> Ligatures;

	internal GPOSRecordsCollection()
	{
		Records = new Dictionary<int, GPOSRecord>();
		Collection = new Dictionary<int, GPOSValueRecord[]>();
		Ligatures = new Dictionary<int, IList<GPOSValueRecord[]>>();
	}
}
