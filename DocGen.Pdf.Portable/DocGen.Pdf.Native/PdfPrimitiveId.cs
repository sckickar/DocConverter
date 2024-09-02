using System.Runtime.InteropServices;

namespace DocGen.Pdf.Native;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct PdfPrimitiveId
{
	internal byte[] Null => new byte[1];

	internal byte[] Integer => new byte[1] { 1 };

	internal byte[] Real => new byte[1] { 2 };

	internal byte[] Boolean => new byte[1] { 3 };

	internal byte[] Name => new byte[1] { 4 };

	internal byte[] String => new byte[1] { 5 };

	internal byte[] Dictionary => new byte[1] { 6 };

	internal byte[] Array => new byte[1] { 7 };

	internal byte[] Stream => new byte[1] { 8 };

	internal byte[] True => new byte[1] { 1 };

	internal byte[] False => new byte[1];

	internal byte[] Visited => new byte[4] { 255, 255, 255, 255 };
}
