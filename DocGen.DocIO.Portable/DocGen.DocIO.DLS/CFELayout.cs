using System;

namespace DocGen.DocIO.DLS;

internal class CFELayout
{
	private ushort m_flag;

	private int m_id;

	private CombineBracketsType m_combineBrackets;

	internal bool Combine
	{
		get
		{
			return (m_flag & 2) != 0;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool Vertical
	{
		get
		{
			return (m_flag & 1) != 0;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool VerticalCompress
	{
		get
		{
			return (m_flag & 0x1000) != 0;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xEFFFu) | ((value ? 1u : 0u) << 12));
		}
	}

	internal int ID
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	internal CombineBracketsType CombineBracketsType
	{
		get
		{
			return m_combineBrackets;
		}
		set
		{
			m_flag = (ushort)((m_flag & 0xFEFFu) | 0x100u);
			m_combineBrackets = value;
		}
	}

	internal bool HasCombineBracketsType()
	{
		return (m_flag & 0x100) != 0;
	}

	internal void UpdateCFELayout(ushort ufel, int iFELayoutId)
	{
		Vertical = (ufel & 1) != 0;
		Combine = (ufel & 2) != 0;
		VerticalCompress = (ufel & 0x1000) != 0;
		ID = iFELayoutId;
		CombineBracketsType = (CombineBracketsType)((ufel >> 8) & 7);
	}

	internal byte[] GetCFELayoutBytes()
	{
		byte[] array = new byte[6];
		if ((m_flag & 0x100u) != 0)
		{
			m_flag = (ushort)((m_flag & 0xFEFFu) | 0u);
			m_flag = (ushort)((m_flag & 0xF8FFu) | (uint)((int)CombineBracketsType << 8));
		}
		Buffer.BlockCopy(BitConverter.GetBytes(m_flag), 0, array, 0, 2);
		Buffer.BlockCopy(BitConverter.GetBytes(ID), 0, array, 2, 4);
		return array;
	}
}
