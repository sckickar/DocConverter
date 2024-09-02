using System;
using System.Runtime.InteropServices;

namespace DocGen.Pdf.Native;

[StructLayout(LayoutKind.Sequential)]
internal class FORMATRANGE
{
	public nint hdc = IntPtr.Zero;

	public nint hdcTarget = IntPtr.Zero;

	public RECT rc;

	public RECT rcPage;

	public CHARRANGE chrg;
}
