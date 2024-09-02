using System;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter;

[CLSCompliant(false)]
internal class GrammarSpelling
{
	private byte[] m_plcfsplData;

	private byte[] m_plcfgramData;

	private List<int> m_gramPositions;

	private List<int> m_spellPositions;

	internal byte[] PlcfsplData
	{
		get
		{
			return m_plcfsplData;
		}
		set
		{
			m_plcfsplData = value;
		}
	}

	internal byte[] PlcfgramData
	{
		get
		{
			return m_plcfgramData;
		}
		set
		{
			m_plcfgramData = value;
		}
	}

	internal GrammarSpelling()
	{
	}

	internal GrammarSpelling(Fib fib, Stream stream, CharPosTableRecord hfCharPosTable)
	{
		int fibRgFcLcb97LcbPlcfSpl = (int)fib.FibRgFcLcb97LcbPlcfSpl;
		int fibRgFcLcb97LcbPlcfGram = (int)fib.FibRgFcLcb97LcbPlcfGram;
		m_plcfsplData = new byte[fibRgFcLcb97LcbPlcfSpl];
		m_plcfgramData = new byte[fibRgFcLcb97LcbPlcfGram];
		MakeCorrection(hfCharPosTable, fib, stream);
		stream.Position = fib.FibRgFcLcb97FcPlcfSpl;
		stream.Read(m_plcfsplData, 0, fibRgFcLcb97LcbPlcfSpl);
		stream.Position = fib.FibRgFcLcb97FcPlcfGram;
		stream.Read(m_plcfgramData, 0, fibRgFcLcb97LcbPlcfGram);
	}

	internal void Write(Fib fib, Stream stream)
	{
		if (m_plcfgramData != null && m_plcfsplData != null)
		{
			fib.FibRgFcLcb97FcPlcfSpl = (uint)stream.Position;
			stream.Write(m_plcfsplData, 0, m_plcfsplData.Length);
			fib.FibRgFcLcb97LcbPlcfSpl = (uint)m_plcfsplData.Length;
			fib.FibRgFcLcb97FcPlcfGram = (uint)stream.Position;
			stream.Write(m_plcfgramData, 0, m_plcfgramData.Length);
			fib.FibRgFcLcb97LcbPlcfGram = (uint)m_plcfgramData.Length;
		}
	}

	internal void GetPositions(Fib fib, Stream stream)
	{
		BinaryReader binaryReader = new BinaryReader(stream);
		if (m_plcfsplData.Length != 0)
		{
			m_spellPositions = new List<int>();
			int num = (m_plcfsplData.Length + 2) / 6;
			binaryReader.BaseStream.Position = fib.FibRgFcLcb97FcPlcfSpl;
			for (int i = 0; i < num; i++)
			{
				m_spellPositions.Add(binaryReader.ReadInt32());
			}
		}
		if (m_plcfgramData.Length != 0)
		{
			m_gramPositions = new List<int>();
			int num2 = (m_plcfgramData.Length + 2) / 6;
			binaryReader.BaseStream.Position = fib.FibRgFcLcb97FcPlcfGram;
			for (int j = 0; j < num2; j++)
			{
				m_gramPositions.Add(binaryReader.ReadInt32());
			}
		}
	}

	private void MakeCorrection(CharPosTableRecord hfCharPosTable, Fib fib, Stream stream)
	{
		if (fib.CcpHdd > 0 && hfCharPosTable != null)
		{
			GetPositions(fib, stream);
			if (MakeHeaderCorrection(hfCharPosTable, fib))
			{
				UpdateGramSpellData(stream, fib);
			}
		}
	}

	private bool MakeHeaderCorrection(CharPosTableRecord hfCharPosTable, Fib fib)
	{
		bool flag = false;
		if (fib.CcpHdd > 0 && hfCharPosTable != null)
		{
			int num = fib.CcpText + fib.CcpFtn;
			int num2 = hfCharPosTable.Positions[6];
			int startShiftCP = num + num2;
			if (m_gramPositions != null)
			{
				flag = ShiftHFPos(isGrammar: true, num, startShiftCP, num2);
			}
			if (m_spellPositions != null && flag)
			{
				flag = ShiftHFPos(isGrammar: false, num, startShiftCP, num2);
			}
		}
		return flag;
	}

	private bool ShiftHFPos(bool isGrammar, int startHeaderCP, int startShiftCP, int shiftValue)
	{
		int posIndex = GetPosIndex(isGrammar, startHeaderCP);
		int posIndex2 = GetPosIndex(isGrammar, startShiftCP);
		if (posIndex == int.MaxValue || posIndex2 == int.MaxValue)
		{
			return false;
		}
		posIndex2++;
		SetHFSeparatorsPos(startHeaderCP, posIndex, posIndex2, isGrammar);
		ShiftPositions(posIndex2, shiftValue, isGrammar);
		return true;
	}

	private void SetHFSeparatorsPos(int value, int startIndex, int endIndex, bool isGrammar)
	{
		List<int> list = (isGrammar ? m_gramPositions : m_spellPositions);
		for (int i = startIndex; i < endIndex; i++)
		{
			list[i] = value;
		}
	}

	private int GetPosIndex(bool isGrammarArray, int charPos)
	{
		List<int> list = (isGrammarArray ? m_gramPositions : m_spellPositions);
		int result = int.MaxValue;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (list[i] >= charPos)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	private void ShiftPositions(int startIndex, int shiftValue, bool isGrammarArray)
	{
		List<int> list = (isGrammarArray ? m_gramPositions : m_spellPositions);
		int i = startIndex;
		for (int count = list.Count; i < count; i++)
		{
			list[i] -= shiftValue;
		}
	}

	private void UpdateGramSpellData(Stream stream, Fib fib)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		if (m_spellPositions != null)
		{
			binaryWriter.BaseStream.Position = fib.FibRgFcLcb97FcPlcfSpl;
			int i = 0;
			for (int count = m_spellPositions.Count; i < count; i++)
			{
				binaryWriter.Write(m_spellPositions[i]);
			}
		}
		if (m_gramPositions != null)
		{
			binaryWriter.BaseStream.Position = fib.FibRgFcLcb97FcPlcfGram;
			int j = 0;
			for (int count2 = m_gramPositions.Count; j < count2; j++)
			{
				binaryWriter.Write(m_gramPositions[j]);
			}
		}
	}

	internal void Close()
	{
		if (m_plcfsplData != null)
		{
			m_plcfsplData = null;
		}
		if (m_plcfgramData != null)
		{
			m_plcfgramData = null;
		}
		if (m_gramPositions != null)
		{
			m_gramPositions.Clear();
			m_gramPositions = null;
		}
		if (m_spellPositions != null)
		{
			m_spellPositions.Clear();
			m_spellPositions = null;
		}
	}
}
