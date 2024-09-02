using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

internal class FopteOptionWrapper : IFopteOptionWrapper
{
	private List<MsofbtOPT.FOPTE> m_list;

	[CLSCompliant(false)]
	public List<MsofbtOPT.FOPTE> OptionList => m_list;

	public FopteOptionWrapper()
	{
		m_list = new List<MsofbtOPT.FOPTE>();
	}

	[CLSCompliant(false)]
	public FopteOptionWrapper(List<MsofbtOPT.FOPTE> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		m_list = list;
	}

	[CLSCompliant(false)]
	public void AddOptionSorted(MsofbtOPT.FOPTE option)
	{
		int i = 0;
		int count = m_list.Count;
		for (int num = count; i < num && m_list[i].Id < option.Id; i++)
		{
		}
		if (i < count)
		{
			if (m_list[i].Id == option.Id)
			{
				m_list[i] = option;
			}
			else
			{
				m_list.Insert(i, option);
			}
		}
		else
		{
			m_list.Add(option);
		}
	}

	public void RemoveOption(int index)
	{
		int i = 0;
		for (int count = m_list.Count; i < count; i++)
		{
			if (m_list[i].Id == (MsoOptions)index)
			{
				m_list.RemoveAt(i);
				break;
			}
		}
	}
}
