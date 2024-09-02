using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class SPContainer : BaseWordRecord
{
	private const int DEF_MSOFBH_LENGTH = 8;

	private const int DEF_SP_LENGTH = 8;

	private const int DEF_FOPTE_LENGTH = 6;

	private const int DEF_RECT_LENGTH = 4;

	private FSP m_sp;

	private List<FOPTE> m_opt;

	private Rect m_anchor;

	private List<MSOFBH> m_msofbhArray;

	public new int Length => 0 + 16 + GetFoptesLength() + 12;

	public SPContainer()
	{
		m_opt = new List<FOPTE>();
	}

	public void Read(Stream stream, uint lenght)
	{
		m_msofbhArray = new List<MSOFBH>();
		uint num = (uint)(int)stream.Position + lenght;
		while (stream.Position < num)
		{
			MSOFBH mSOFBH = new MSOFBH();
			mSOFBH.Read(stream);
			m_msofbhArray.Add(mSOFBH);
			switch (mSOFBH.Msofbt)
			{
			case MSOFBT.msofbtSp:
				m_sp = new FSP();
				m_sp.Read(stream);
				break;
			case MSOFBT.msofbtOPT:
				ReadFoptes(stream, mSOFBH);
				break;
			case MSOFBT.msofbtAnchor:
			case MSOFBT.msofbtChildAnchor:
			case MSOFBT.msofbtClientAnchor:
				m_anchor = new Rect();
				m_anchor.Read(stream);
				break;
			default:
				stream.Position += mSOFBH.Length;
				break;
			}
		}
	}

	public void Write(Stream stream)
	{
		GenerateDefaultOPT();
		MSOFBH mSOFBH = new MSOFBH();
		mSOFBH.Msofbt = MSOFBT.msofbtSpContainer;
		mSOFBH.Inst = 0u;
		mSOFBH.Length = (uint)Length;
		mSOFBH.Version = 15u;
		mSOFBH.Write(stream);
		WriteFSP(stream);
		WriteFoptes(stream);
		m_anchor = new Rect();
		m_anchor.Bottom = 120L;
		MSOFBH mSOFBH2 = new MSOFBH();
		mSOFBH2.Msofbt = MSOFBT.msofbtClientAnchor;
		mSOFBH2.Length = 4u;
		mSOFBH2.Inst = 0u;
		mSOFBH2.Version = 0u;
		mSOFBH2.Write(stream);
		m_anchor.Write(stream);
	}

	private void ReadFoptes(Stream stream, MSOFBH msofbh)
	{
		int num = (int)msofbh.Length / 6;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (num2 >= msofbh.Length)
			{
				break;
			}
			FOPTE fOPTE = new FOPTE();
			num2 += fOPTE.Read(stream);
			m_opt.Add(fOPTE);
		}
		for (int j = 0; j < m_opt.Count; j++)
		{
			if (m_opt[j].IsComplex)
			{
				stream.Read(m_opt[j].NameBytes, 0, m_opt[j].NameBytes.Length);
			}
		}
	}

	private void WriteFoptes(Stream stream)
	{
		MSOFBH mSOFBH = new MSOFBH();
		mSOFBH.Msofbt = MSOFBT.msofbtOPT;
		mSOFBH.Length = (uint)GetFoptesLength();
		mSOFBH.Inst = 4u;
		mSOFBH.Version = 3u;
		mSOFBH.Write(stream);
		for (int i = 0; i < m_opt.Count; i++)
		{
			m_opt[i].Write(stream);
		}
		for (int j = 0; j < m_opt.Count; j++)
		{
			if (m_opt[j].IsComplex)
			{
				stream.Write(m_opt[j].NameBytes, 0, m_opt[j].NameBytes.Length);
			}
		}
	}

	private int GetFoptesLength()
	{
		int num = 0;
		if (m_opt != null)
		{
			num += 8;
			for (int i = 0; i < m_opt.Count; i++)
			{
				num = ((!m_opt[i].IsComplex) ? (num + 6) : (num + (int)(m_opt[i].Op + 6)));
			}
		}
		return num;
	}

	private void GenerateDefaultOPT()
	{
		m_opt = new List<FOPTE>();
		FOPTE fOPTE = new FOPTE();
		fOPTE.IsBid = true;
		fOPTE.IsComplex = false;
		fOPTE.Pid = 260;
		fOPTE.Op = 1u;
		m_opt.Add(fOPTE);
		fOPTE = new FOPTE();
		fOPTE.IsBid = true;
		fOPTE.IsComplex = true;
		fOPTE.Pid = 261;
		fOPTE.NameBytes = Encoding.Unicode.GetBytes("autowalls_ru_17\0");
		fOPTE.Op = (uint)fOPTE.NameBytes.Length;
		m_opt.Add(fOPTE);
		fOPTE = new FOPTE();
		fOPTE.IsBid = false;
		fOPTE.IsComplex = false;
		fOPTE.Pid = 262;
		fOPTE.Op = 2u;
		m_opt.Add(fOPTE);
		fOPTE = new FOPTE();
		fOPTE.IsBid = false;
		fOPTE.IsComplex = false;
		fOPTE.Pid = 511;
		fOPTE.Op = 524288u;
		m_opt.Add(fOPTE);
	}

	private void WriteFSP(Stream stream)
	{
		MSOFBH mSOFBH = new MSOFBH();
		mSOFBH.Msofbt = MSOFBT.msofbtSp;
		mSOFBH.Inst = 75u;
		mSOFBH.Version = 2u;
		mSOFBH.Length = 8u;
		mSOFBH.Write(stream);
		FSP fSP = new FSP();
		fSP.Spid = 1026u;
		fSP.GzfPersistent = 2560u;
		fSP.Write(stream);
	}
}
