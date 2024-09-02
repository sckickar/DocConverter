using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class PdfEncryption
{
	internal static long Sequence = DateTime.Now.Ticks + Environment.TickCount;

	internal static byte[] CreateDocumentId()
	{
		long num = DateTime.Now.Ticks + Environment.TickCount;
		long totalMemory = GC.GetTotalMemory(forceFullCollection: false);
		string s = num + "+" + totalMemory + "+" + Sequence++;
		byte[] array = null;
		array = Encoding.UTF8.GetBytes(s);
		return new MessageDigestAlgorithms().Digest("MD5", array);
	}
}
