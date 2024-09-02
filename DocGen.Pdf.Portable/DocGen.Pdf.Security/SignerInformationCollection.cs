using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class SignerInformationCollection
{
	private readonly ICollection all;

	private readonly IDictionary table = new Dictionary<SignerId, List<CmsSignerDetails>>();

	internal SignerInformationCollection(ICollection signerInfos)
	{
		foreach (CmsSignerDetails signerInfo in signerInfos)
		{
			SignerId iD = signerInfo.ID;
			IList list = (IList)table[iD];
			if (list == null)
			{
				list = (IList)(table[iD] = new List<CmsSignerDetails>(1));
			}
			list.Add(signerInfo);
		}
		all = signerInfos;
	}

	internal ICollection GetSigners()
	{
		return all;
	}
}
