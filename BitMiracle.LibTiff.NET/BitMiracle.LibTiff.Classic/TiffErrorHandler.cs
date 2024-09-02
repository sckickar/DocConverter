using System;
using System.IO;

namespace BitMiracle.LibTiff.Classic;

public class TiffErrorHandler
{
	public virtual void ErrorHandler(Tiff tif, string method, string format, params object[] args)
	{
		TextWriter error = Console.Error;
		if (method != null)
		{
			error.Write("{0}: ", method);
		}
		error.Write(format, args);
		error.Write("\n");
	}

	public virtual void ErrorHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
	{
	}

	public virtual void WarningHandler(Tiff tif, string method, string format, params object[] args)
	{
		TextWriter error = Console.Error;
		if (method != null)
		{
			error.Write("{0}: ", method);
		}
		error.Write("Warning, ");
		error.Write(format, args);
		error.Write("\n");
	}

	public virtual void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
	{
	}
}
