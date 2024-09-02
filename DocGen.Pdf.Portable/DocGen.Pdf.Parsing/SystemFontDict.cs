using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Parsing;

internal class SystemFontDict : SystemFontCFFTable
{
	private readonly int length;

	protected Dictionary<SystemFontOperatorDescriptor, SystemFontOperandsCollection> Data { get; private set; }

	public long SkipOffset => base.Offset + length;

	private static bool IsOperator(byte b)
	{
		if (0 <= b)
		{
			return b <= 21;
		}
		return false;
	}

	private static bool IsTwoByteOperator(byte b)
	{
		return b == 12;
	}

	public SystemFontDict(SystemFontCFFFontFile file, long offset, int length)
		: base(file, offset)
	{
		this.length = length;
	}

	protected int GetInt(SystemFontOperatorDescriptor op)
	{
		if (Data.TryGetValue(op, out SystemFontOperandsCollection value))
		{
			return value.GetLastAsInt();
		}
		if (op.DefaultValue != null)
		{
			return (int)op.DefaultValue;
		}
		throw new ArgumentException("Operator not found");
	}

	protected SystemFontOperandsCollection GetOperands(SystemFontOperatorDescriptor op)
	{
		if (!Data.TryGetValue(op, out SystemFontOperandsCollection value))
		{
			throw new ArgumentException("Operator not found");
		}
		return value;
	}

	protected double GetNumber(SystemFontOperatorDescriptor op)
	{
		if (Data.TryGetValue(op, out SystemFontOperandsCollection value))
		{
			return value.GetLastAsReal();
		}
		if (op.DefaultValue != null)
		{
			return (double)op.DefaultValue;
		}
		throw new ArgumentException("Operator not found");
	}

	protected SystemFontPostScriptArray GetArray(SystemFontOperatorDescriptor op)
	{
		if (Data.TryGetValue(op, out SystemFontOperandsCollection value))
		{
			SystemFontPostScriptArray systemFontPostScriptArray = new SystemFontPostScriptArray();
			while (value.Count > 0)
			{
				systemFontPostScriptArray.Add(value.GetFirst());
			}
			return systemFontPostScriptArray;
		}
		if (op.DefaultValue != null)
		{
			return (SystemFontPostScriptArray)op.DefaultValue;
		}
		throw new ArgumentException("Operator not found");
	}

	public override void Read(SystemFontCFFFontReader reader)
	{
		byte[] array = new byte[length];
		reader.Read(array, length);
		SystemFontEncodedDataReader systemFontEncodedDataReader = new SystemFontEncodedDataReader(array, SystemFontByteEncoding.DictByteEncodings);
		Data = new Dictionary<SystemFontOperatorDescriptor, SystemFontOperandsCollection>();
		SystemFontOperandsCollection systemFontOperandsCollection = new SystemFontOperandsCollection();
		while (!systemFontEncodedDataReader.EndOfFile)
		{
			byte b = systemFontEncodedDataReader.Peek(0);
			if (IsOperator(b))
			{
				SystemFontOperatorDescriptor key = ((!IsTwoByteOperator(b)) ? new SystemFontOperatorDescriptor(systemFontEncodedDataReader.Read()) : new SystemFontOperatorDescriptor(SystemFontHelper.CreateByteArray(systemFontEncodedDataReader.Read(), systemFontEncodedDataReader.Read())));
				Data[key] = systemFontOperandsCollection;
				systemFontOperandsCollection = new SystemFontOperandsCollection();
			}
			else
			{
				systemFontOperandsCollection.AddLast(systemFontEncodedDataReader.ReadOperand());
			}
		}
	}
}
