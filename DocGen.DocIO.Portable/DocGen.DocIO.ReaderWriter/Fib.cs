using System;
using System.IO;
using System.Text;

namespace DocGen.DocIO.ReaderWriter;

internal class Fib
{
	private byte[] m_fibBase;

	private ushort m_csw;

	private byte[] m_fibRgW;

	private ushort m_cslw;

	private byte[] m_fibRgLw;

	private ushort m_cbRgFcLcb;

	private byte[] m_fibRgFcLcbBlob;

	private ushort m_cswNew;

	private byte[] m_fibRgCswNew;

	private Encoding m_encoding = Encoding.Unicode;

	internal ushort FibVersion
	{
		get
		{
			if (CswNew > 0)
			{
				return NFibNew;
			}
			return NFib;
		}
	}

	internal ushort WIdent
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 0);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 0, 2);
		}
	}

	internal ushort NFib
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 2);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 2, 2);
		}
	}

	internal ushort BaseUnused
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 4);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 4, 2);
		}
	}

	internal ushort Lid
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 6);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 6, 2);
		}
	}

	internal ushort PnNext
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 8);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 8, 2);
		}
	}

	internal bool FDot
	{
		get
		{
			return (m_fibBase[10] & 1) != 0;
		}
		set
		{
			byte b = m_fibBase[10];
			b = (byte)((b & 0xFEu) | (value ? 1u : 0u));
			m_fibBase[10] = b;
		}
	}

	internal bool FGlsy
	{
		get
		{
			return (m_fibBase[10] & 2) >> 1 != 0;
		}
		set
		{
			byte b = m_fibBase[10];
			b = (byte)((b & 0xFDu) | ((value ? 1u : 0u) << 1));
			m_fibBase[10] = b;
		}
	}

	internal bool FComplex
	{
		get
		{
			return (m_fibBase[10] & 4) >> 2 != 0;
		}
		set
		{
			byte b = m_fibBase[10];
			b = (byte)((b & 0xFBu) | ((value ? 1u : 0u) << 2));
			m_fibBase[10] = b;
		}
	}

	internal bool FHasPic
	{
		get
		{
			return (m_fibBase[10] & 8) >> 3 != 0;
		}
		set
		{
			byte b = m_fibBase[10];
			b = (byte)((b & 0xF7u) | ((value ? 1u : 0u) << 3));
			m_fibBase[10] = b;
		}
	}

	internal byte CQuickSaves
	{
		get
		{
			return (byte)(m_fibBase[10] >> 4);
		}
		set
		{
			byte b = m_fibBase[10];
			b = (byte)((b & 0xFu) | (uint)(((value > 15) ? 15 : value) << 4));
			m_fibBase[10] = b;
		}
	}

	internal bool FEncrypted
	{
		get
		{
			return (m_fibBase[11] & 1) != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xFEu) | (value ? 1u : 0u));
			m_fibBase[11] = b;
		}
	}

	internal bool FWhichTblStm
	{
		get
		{
			return (m_fibBase[11] & 2) >> 1 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xFDu) | ((value ? 1u : 0u) << 1));
			m_fibBase[11] = b;
		}
	}

	internal bool FReadOnlyRecommended
	{
		get
		{
			return (m_fibBase[11] & 4) >> 2 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xFBu) | ((value ? 1u : 0u) << 2));
			m_fibBase[11] = b;
		}
	}

	internal bool FWriteReservation
	{
		get
		{
			return (m_fibBase[11] & 8) >> 3 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xF7u) | ((value ? 1u : 0u) << 3));
			m_fibBase[11] = b;
		}
	}

	internal bool FExtChar
	{
		get
		{
			return (m_fibBase[11] & 0x10) >> 4 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xEFu) | ((value ? 1u : 0u) << 4));
			m_fibBase[11] = b;
		}
	}

	internal bool FLoadOverride
	{
		get
		{
			return (m_fibBase[11] & 0x20) >> 5 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xDFu) | ((value ? 1u : 0u) << 5));
			m_fibBase[11] = b;
		}
	}

	internal bool FFarEast
	{
		get
		{
			return (m_fibBase[11] & 0x40) >> 6 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0xBFu) | ((value ? 1u : 0u) << 6));
			m_fibBase[11] = b;
		}
	}

	internal bool FObfuscated
	{
		get
		{
			return (m_fibBase[11] & 0x80) >> 7 != 0;
		}
		set
		{
			byte b = m_fibBase[11];
			b = (byte)((b & 0x7Fu) | ((value ? 1u : 0u) << 7));
			m_fibBase[11] = b;
		}
	}

	internal ushort NFibBack
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 12);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 12, 2);
		}
	}

	internal int LKey
	{
		get
		{
			return BitConverter.ToInt32(m_fibBase, 14);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 14, 4);
		}
	}

	internal byte Envr
	{
		get
		{
			return m_fibBase[18];
		}
		set
		{
			m_fibBase[18] = value;
		}
	}

	internal bool FMac
	{
		get
		{
			return (m_fibBase[19] & 1) != 0;
		}
		set
		{
			byte b = m_fibBase[19];
			b = (byte)((b & 0xFEu) | (value ? 1u : 0u));
			m_fibBase[19] = b;
		}
	}

	internal bool FEmptySpecial
	{
		get
		{
			return (m_fibBase[19] & 2) >> 1 != 0;
		}
		set
		{
			byte b = m_fibBase[19];
			b = (byte)((b & 0xFDu) | ((value ? 1u : 0u) << 1));
			m_fibBase[19] = b;
		}
	}

	internal bool FLoadOverridePage
	{
		get
		{
			return (m_fibBase[19] & 4) >> 2 != 0;
		}
		set
		{
			byte b = m_fibBase[19];
			b = (byte)((b & 0xFBu) | ((value ? 1u : 0u) << 2));
			m_fibBase[19] = b;
		}
	}

	internal bool BaseReserved1
	{
		get
		{
			return (m_fibBase[19] & 8) >> 3 != 0;
		}
		set
		{
			byte b = m_fibBase[19];
			b = (byte)((b & 0xF7u) | ((value ? 1u : 0u) << 3));
			m_fibBase[19] = b;
		}
	}

	internal bool BaseReserved2
	{
		get
		{
			return (m_fibBase[19] & 0x10) >> 4 != 0;
		}
		set
		{
			byte b = m_fibBase[19];
			b = (byte)((b & 0xEFu) | ((value ? 1u : 0u) << 4));
			m_fibBase[19] = b;
		}
	}

	internal byte FSpare0
	{
		get
		{
			return (byte)(m_fibBase[19] >> 5);
		}
		set
		{
			byte b = m_fibBase[19];
			b = (byte)((b & 0x1Fu) | (uint)(((value > 7) ? 7 : value) << 5));
			m_fibBase[19] = b;
		}
	}

	internal ushort BaseReserved3
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 20);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 20, 2);
		}
	}

	internal ushort BaseReserved4
	{
		get
		{
			return BitConverter.ToUInt16(m_fibBase, 22);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 22, 2);
		}
	}

	internal uint BaseReserved5
	{
		get
		{
			return BitConverter.ToUInt32(m_fibBase, 24);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 24, 4);
		}
	}

	internal uint BaseReserved6
	{
		get
		{
			return BitConverter.ToUInt32(m_fibBase, 28);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 28, 4);
		}
	}

	internal ushort Csw
	{
		get
		{
			return m_csw;
		}
		set
		{
			m_csw = value;
		}
	}

	internal ushort FibRgWReserved1
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 0);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 0, 2);
		}
	}

	internal ushort FibRgWReserved2
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 2);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 2, 2);
		}
	}

	internal ushort FibRgWReserved3
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 4);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 4, 2);
		}
	}

	internal ushort FibRgWReserved4
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 6);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 6, 2);
		}
	}

	internal ushort FibRgWReserved5
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 8);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 8, 2);
		}
	}

	internal ushort FibRgWReserved6
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 10);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 10, 2);
		}
	}

	internal ushort FibRgWReserved7
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 12);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 12, 2);
		}
	}

	internal ushort FibRgWReserved8
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 14);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 14, 2);
		}
	}

	internal ushort FibRgWReserved9
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 16);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 16, 2);
		}
	}

	internal ushort FibRgWReserved10
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 18);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 18, 2);
		}
	}

	internal ushort FibRgWReserved11
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 20);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 20, 2);
		}
	}

	internal ushort FibRgWReserved12
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 22);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 22, 2);
		}
	}

	internal ushort FibRgWReserved13
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 24);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 24, 2);
		}
	}

	internal ushort FibRgWLidFE
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgW, 26);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgW, 26, 2);
		}
	}

	internal ushort Cslw
	{
		get
		{
			return m_cslw;
		}
		set
		{
			m_cslw = value;
		}
	}

	internal int CbMac
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 0);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 0, 4);
		}
	}

	internal int RgLwReserved1
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 4);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 4, 4);
		}
	}

	internal int RgLwReserved2
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 8);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 8, 4);
		}
	}

	internal int CcpText
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 12);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 12, 4);
		}
	}

	internal int CcpFtn
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 16);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 16, 4);
		}
	}

	internal int CcpHdd
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 20);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 20, 4);
		}
	}

	internal int RgLwReserved3
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 24);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 24, 4);
		}
	}

	internal int CcpAtn
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 28);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 28, 4);
		}
	}

	internal int CcpEdn
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 32);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 32, 4);
		}
	}

	internal int CcpTxbx
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 36);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 36, 4);
		}
	}

	internal int CcpHdrTxbx
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 40);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 40, 4);
		}
	}

	internal int RgLwReserved4
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 44);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 44, 4);
		}
	}

	internal int RgLwReserved5
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 48);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 48, 4);
		}
	}

	internal int RgLwReserved6
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 52);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 52, 4);
		}
	}

	internal int RgLwReserved7
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 56);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 56, 4);
		}
	}

	internal int RgLwReserved8
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 60);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 60, 4);
		}
	}

	internal int RgLwReserved9
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 64);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 64, 4);
		}
	}

	internal int RgLwReserved10
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 68);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 68, 4);
		}
	}

	internal int RgLwReserved11
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 72);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 72, 4);
		}
	}

	internal int RgLwReserved12
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 76);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 76, 4);
		}
	}

	internal int RgLwReserved13
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 80);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 80, 4);
		}
	}

	internal int RgLwReserved14
	{
		get
		{
			return BitConverter.ToInt32(m_fibRgLw, 84);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgLw, 84, 4);
		}
	}

	internal ushort CbRgFcLcb
	{
		get
		{
			return m_cbRgFcLcb;
		}
		set
		{
			m_cbRgFcLcb = value;
		}
	}

	internal uint FibRgFcLcb97FcStshfOrig
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 0);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 0, 4);
		}
	}

	internal uint FibRgFcLcb97LcbStshfOrig
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 4);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 4, 4);
		}
	}

	internal uint FibRgFcLcb97FcStshf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 8);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 8, 4);
		}
	}

	internal uint FibRgFcLcb97LcbStshf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 12);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 12, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcffndRef
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 16);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 16, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcffndRef
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 20);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 20, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcffndTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 24);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 24, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcffndTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 28);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 28, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfandRef
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 32);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 32, 4);
		}
	}

	internal uint FibRgFcLcb97lcbPlcfandRef
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 36);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 36, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfandTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 40);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 40, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfandTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 44);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 44, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfSed
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 48);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 48, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfSed
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 52);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 52, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcPad
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 56);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 56, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcPad
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 60);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 60, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfPhe
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 64);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 64, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfPhe
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 68);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 68, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfGlsy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 72);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 72, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfGlsy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 76);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 76, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfGlsy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 80);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 80, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfGlsy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 84);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 84, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfHdd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 88);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 88, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfHdd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 92);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 92, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfBteChpx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 96);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 96, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfBteChpx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 100);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 100, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfBtePapx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 104);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 104, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfBtePapx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 108);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 108, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfSea
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 112);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 112, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfSea
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 116);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 116, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfFfn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 120);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 120, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfFfn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 124);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 124, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldMom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 128);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 128, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldMom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 132);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 132, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldHdr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 136);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 136, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldHdr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 140);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 140, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 144);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 144, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 148);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 148, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldAtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 152);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 152, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldAtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 156);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 156, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldMcr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 160);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 160, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldMcr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 164);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 164, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfBkmk
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 168);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 168, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfBkmk
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 172);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 172, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfBkf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 176);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 176, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfBkf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 180);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 180, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfBkl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 184);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 184, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfBkl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 188);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 188, 4);
		}
	}

	internal uint FibRgFcLcb97FcCmds
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 192);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 192, 4);
		}
	}

	internal uint FibRgFcLcb97LcbCmds
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 196);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 196, 4);
		}
	}

	internal uint FibRgFcLcb97FcUnused1
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 200);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 200, 4);
		}
	}

	internal uint FibRgFcLcb97LcbUnused1
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 204);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 204, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfMcr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 208);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 208, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfMcr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 212);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 212, 4);
		}
	}

	internal uint FibRgFcLcb97FcPrDrvr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 216);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 216, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPrDrvr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 220);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 220, 4);
		}
	}

	internal uint FibRgFcLcb97FcPrEnvPort
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 224);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 224, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPrEnvPort
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 228);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 228, 4);
		}
	}

	internal uint FibRgFcLcb97FcPrEnvLand
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 232);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 232, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPrEnvLand
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 236);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 236, 4);
		}
	}

	internal uint FibRgFcLcb97FcWss
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 240);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 240, 4);
		}
	}

	internal uint FibRgFcLcb97LcbWss
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 244);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 244, 4);
		}
	}

	internal uint FibRgFcLcb97FcDop
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 248);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 248, 4);
		}
	}

	internal uint FibRgFcLcb97LcbDop
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 252);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 252, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfAssoc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 256);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 256, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfAssoc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 260);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 260, 4);
		}
	}

	internal uint FibRgFcLcb97FcClx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 264);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 264, 4);
		}
	}

	internal uint FibRgFcLcb97LcbClx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 268);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 268, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfPgdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 272);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 272, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfPgdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 276);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 276, 4);
		}
	}

	internal uint FibRgFcLcb97FcAutosaveSource
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 280);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 280, 4);
		}
	}

	internal uint FibRgFcLcb97LcbAutosaveSource
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 284);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 284, 4);
		}
	}

	internal uint FibRgFcLcb97FcGrpXstAtnOwners
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 288);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 288, 4);
		}
	}

	internal uint FibRgFcLcb97LcbGrpXstAtnOwners
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 292);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 292, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfAtnBkmk
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 296);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 296, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfAtnBkmk
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 300);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 300, 4);
		}
	}

	internal uint FibRgFcLcb97FcUnused2
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 304);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 304, 4);
		}
	}

	internal uint FibRgFcLcb97LcbUnused2
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 308);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 308, 4);
		}
	}

	internal uint FibRgFcLcb97FcUnused3
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 312);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 312, 4);
		}
	}

	internal uint FibRgFcLcb97LcbUnused3
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 316);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 316, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcSpaMom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 320);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 320, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcSpaMom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 324);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 324, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcSpaHdr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 328);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 328, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcSpaHdr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 332);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 332, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfAtnBkf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 336);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 336, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfAtnBkf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 340);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 340, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfAtnBkl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 344);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 344, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfAtnBkl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 348);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 348, 4);
		}
	}

	internal uint FibRgFcLcb97FcPms
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 352);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 352, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPms
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 356);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 356, 4);
		}
	}

	internal uint FibRgFcLcb97FcFormFldSttbs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 360);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 360, 4);
		}
	}

	internal uint FibRgFcLcb97LcbFormFldSttbs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 364);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 364, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfendRef
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 368);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 368, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfendRef
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 372);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 372, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfendTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 376);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 376, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfendTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 380);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 380, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 384);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 384, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 388);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 388, 4);
		}
	}

	internal uint FibRgFcLcb97FcUnused4
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 392);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 392, 4);
		}
	}

	internal uint FibRgFcLcb97LcbUnused4
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 396);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 396, 4);
		}
	}

	internal uint FibRgFcLcb97FcDggInfo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 400);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 400, 4);
		}
	}

	internal uint FibRgFcLcb97LcbDggInfo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 404);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 404, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfRMark
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 408);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 408, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfRMark
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 412);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 412, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfCaption
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 416);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 416, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfCaption
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 420);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 420, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfAutoCaption
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 424);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 424, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfAutoCaption
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 428);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 428, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfWkb
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 432);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 432, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfWkb
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 436);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 436, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfSpl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 440);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 440, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfSpl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 444);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 444, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcftxbxTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 448);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 448, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcftxbxTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 452);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 452, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfFldTxbx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 456);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 456, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfFldTxbx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 460);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 460, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfHdrtxbxTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 464);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 464, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfHdrtxbxTxt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 468);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 468, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcffldHdrTxbx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 472);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 472, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcffldHdrTxbx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 476);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 476, 4);
		}
	}

	internal uint FibRgFcLcb97FcStwUser
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 480);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 480, 4);
		}
	}

	internal uint FibRgFcLcb97LcbStwUser
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 484);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 484, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbTtmbd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 488);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 488, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbTtmbd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 492);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 492, 4);
		}
	}

	internal uint FibRgFcLcb97FcCookieData
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 496);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 496, 4);
		}
	}

	internal uint FibRgFcLcb97LcbCookieData
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 500);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 500, 4);
		}
	}

	internal uint FibRgFcLcb97FcPgdMotherOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 504);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 504, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPgdMotherOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 508);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 508, 4);
		}
	}

	internal uint FibRgFcLcb97FcBkdMotherOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 512);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 512, 4);
		}
	}

	internal uint FibRgFcLcb97LcbBkdMotherOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 516);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 516, 4);
		}
	}

	internal uint FibRgFcLcb97FcPgdFtnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 520);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 520, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPgdFtnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 524);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 524, 4);
		}
	}

	internal uint FibRgFcLcb97FcBkdFtnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 528);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 528, 4);
		}
	}

	internal uint FibRgFcLcb97LcbBkdFtnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 532);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 532, 4);
		}
	}

	internal uint FibRgFcLcb97FcPgdEdnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 536);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 536, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPgdEdnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 540);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 540, 4);
		}
	}

	internal uint FibRgFcLcb97FcBkdEdnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 544);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 544, 4);
		}
	}

	internal uint FibRgFcLcb97LcbBkdEdnOldOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 548);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 548, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbfIntlFld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 552);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 552, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfIntlFld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 556);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 556, 4);
		}
	}

	internal uint FibRgFcLcb97FcRouteSlip
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 560);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 560, 4);
		}
	}

	internal uint FibRgFcLcb97LcbRouteSlip
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 564);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 564, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbSavedBy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 568);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 568, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbSavedBy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 572);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 572, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbFnm
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 576);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 576, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbFnm
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 580);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 580, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlfLst
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 584);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 584, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlfLst
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 588);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 588, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlfLfo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 592);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 592, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlfLfo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 596);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 596, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfTxbxBkd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 600);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 600, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfTxbxBkd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 604);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 604, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfTxbxHdrBkd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 608);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 608, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfTxbxHdrBkd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 612);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 612, 4);
		}
	}

	internal uint FibRgFcLcb97FcDocUndoWord9
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 616);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 616, 4);
		}
	}

	internal uint FibRgFcLcb97LcbDocUndoWord9
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 620);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 620, 4);
		}
	}

	internal uint FibRgFcLcb97FcRgbUse
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 624);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 624, 4);
		}
	}

	internal uint FibRgFcLcb97LcbRgbUse
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 628);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 628, 4);
		}
	}

	internal uint FibRgFcLcb97FcUsp
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 632);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 632, 4);
		}
	}

	internal uint FibRgFcLcb97LcbUsp
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 636);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 636, 4);
		}
	}

	internal uint FibRgFcLcb97FcUskf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 640);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 640, 4);
		}
	}

	internal uint FibRgFcLcb97LcbUskf
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 644);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 644, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcupcRgbUse
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 648);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 648, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcupcRgbUse
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 652);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 652, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcupcUsp
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 656);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 656, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcupcUsp
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 660);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 660, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbGlsyStyle
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 664);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 664, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbGlsyStyle
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 668);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 668, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlgosl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 672);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 672, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlgosl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 676);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 676, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcocx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 680);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 680, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcocx
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 684);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 684, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfBteLvc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 688);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 688, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfBteLvc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 692);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 692, 4);
		}
	}

	internal uint FibRgFcLcb97DwLowDateTime
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 696);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 696, 4);
		}
	}

	internal uint FibRgFcLcb97DwHighDateTime
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 700);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 700, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfLvcPre10
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 704);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 704, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfLvcPre10
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 708);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 708, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfAsumy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 712);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 712, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfAsumy
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 716);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 716, 4);
		}
	}

	internal uint FibRgFcLcb97FcPlcfGram
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 720);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 720, 4);
		}
	}

	internal uint FibRgFcLcb97LcbPlcfGram
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 724);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 724, 4);
		}
	}

	internal uint FibRgFcLcb97FcSttbListNames
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 728);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 728, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbListNames
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 732);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 732, 4);
		}
	}

	internal uint FibRgFcLcb97fcSttbfUssr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 736);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 736, 4);
		}
	}

	internal uint FibRgFcLcb97LcbSttbfUssr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 740);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 740, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPlcfTch
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 744);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 744, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPlcfTch
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 748);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 748, 4);
		}
	}

	internal uint FibRgFcLcb2000FcRmdThreading
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 752);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 752, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbRmdThreading
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 756);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 756, 4);
		}
	}

	internal uint FibRgFcLcb2000FcMid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 760);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 760, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbMid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 764);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 764, 4);
		}
	}

	internal uint FibRgFcLcb2000FcSttbRgtplc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 768);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 768, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbSttbRgtplc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 772);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 772, 4);
		}
	}

	internal uint FibRgFcLcb2000FcMsoEnvelope
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 776);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 776, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbMsoEnvelope
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 780);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 780, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPlcfLad
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 784);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 784, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPlcfLad
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 788);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 788, 4);
		}
	}

	internal uint FibRgFcLcb2000FcRgDofr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 792);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 792, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbRgDofr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 796);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 796, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPlcosl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 800);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 800, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPlcosl
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 804);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 804, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPlcfCookieOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 808);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 808, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPlcfCookieOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 812);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 812, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPgdMotherOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 816);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 816, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPgdMotherOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 820);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 820, 4);
		}
	}

	internal uint FibRgFcLcb2000FcBkdMotherOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 824);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 824, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbBkdMotherOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 828);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 828, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPgdFtnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 832);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 832, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPgdFtnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 836);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 836, 4);
		}
	}

	internal uint FibRgFcLcb2000FcBkdFtnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 840);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 840, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbBkdFtnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 844);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 844, 4);
		}
	}

	internal uint FibRgFcLcb2000FcPgdEdnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 848);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 848, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbPgdEdnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 852);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 852, 4);
		}
	}

	internal uint FibRgFcLcb2000FcBkdEdnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 856);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 856, 4);
		}
	}

	internal uint FibRgFcLcb2000LcbBkdEdnOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 860);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 860, 4);
		}
	}

	internal uint FibRgFcLcb2002FcUnused1
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 864);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 864, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbUnused1
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 868);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 868, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfPgp
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 872);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 872, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfPgp
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 876);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 876, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfuim
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 880);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 880, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfuim
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 884);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 884, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlfguidUim
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 888);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 888, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlfguidUim
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 892);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 892, 4);
		}
	}

	internal uint FibRgFcLcb2002FcAtrdExtra
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 896);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 896, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbAtrdExtra
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 900);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 900, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlrsid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 904);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 904, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlrsid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 908);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 908, 4);
		}
	}

	internal uint FibRgFcLcb2002FcSttbfBkmkFactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 912);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 912, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbSttbfBkmkFactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 916);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 916, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfBkfFactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 920);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 920, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfBkfFactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 924);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 924, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfcookie
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 928);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 928, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfcookie
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 932);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 932, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfBklFactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 936);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 936, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfBklFactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 940);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 940, 4);
		}
	}

	internal uint FibRgFcLcb2002FcFactoidData
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 944);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 944, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbFactoidData
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 948);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 948, 4);
		}
	}

	internal uint FibRgFcLcb2002FcDocUndo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 952);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 952, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbDocUndo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 956);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 956, 4);
		}
	}

	internal uint FibRgFcLcb2002FcSttbfBkmkFcc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 960);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 960, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbSttbfBkmkFcc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 964);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 964, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfBkfFcc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 968);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 968, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfBkfFcc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 972);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 972, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfBklFcc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 976);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 976, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfBklFcc
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 980);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 980, 4);
		}
	}

	internal uint FibRgFcLcb2002FcSttbfbkmkBPRepairs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 984);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 984, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbSttbfbkmkBPRepairs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 988);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 988, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfbkfBPRepairs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 992);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 992, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfbkfBPRepairs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 996);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 996, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfbklBPRepairs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1000);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1000, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfbklBPRepairs
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1004);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1004, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPmsNew
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1008);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1008, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPmsNew
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1012);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1012, 4);
		}
	}

	internal uint FibRgFcLcb2002FcODSO
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1016);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1016, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbODSO
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1020);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1020, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfpmiOldXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1024);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1024, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfpmiOldXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1028);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1028, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfpmiNewXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1032);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1032, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfpmiNewXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1036);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1036, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcfpmiMixedXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1040);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1040, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcfpmiMixedXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1044);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1044, 4);
		}
	}

	internal uint FibRgFcLcb2002FcUnused2
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1048);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibBase, 1048, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbUnused2
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1052);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1052, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcffactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1056);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1056, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcffactoid
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1060);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1060, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcflvcOldXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1064);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1064, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcflvcOldXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1068);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1068, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcflvcNewXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1072);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1072, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcflvcNewXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1076);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1076, 4);
		}
	}

	internal uint FibRgFcLcb2002FcPlcflvcMixedXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1080);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1080, 4);
		}
	}

	internal uint FibRgFcLcb2002LcbPlcflvcMixedXP
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1084);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1084, 4);
		}
	}

	internal uint FibRgFcLcb2003FcHplxsdr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1088);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1088, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbHplxsdr
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1092);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1092, 4);
		}
	}

	internal uint FibRgFcLcb2003FcSttbfBkmkSdt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1096);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1096, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbSttbfBkmkSdt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1100);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1100, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfBkfSdt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1104);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1104, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfBkfSdt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1108);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1108, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfBklSdt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1112);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1112, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfBklSdt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1116);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1116, 4);
		}
	}

	internal uint FibRgFcLcb2003FcCustomXForm
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1120);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1120, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbCustomXForm
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1124);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1124, 4);
		}
	}

	internal uint FibRgFcLcb2003FcSttbfBkmkProt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1128);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1128, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbSttbfBkmkProt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1132);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1132, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfBkfProt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1136);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1136, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfBkfProt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1140);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1140, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfBklProt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1144);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1144, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfBklProt
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1148);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1148, 4);
		}
	}

	internal uint FibRgFcLcb2003FcSttbProtUser
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1152);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1152, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbSttbProtUser
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1156);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1156, 4);
		}
	}

	internal uint FibRgFcLcb2003FcUnused
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1160);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1160, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbUnused
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1164);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1164, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfpmiOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1168);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1168, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfpmiOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1172);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1172, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfpmiOldInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1176);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1176, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfpmiOldInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1180);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1180, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfpmiNew
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1184);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1184, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfpmiNew
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1188);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1188, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcfpmiNewInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1192);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1192, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcfpmiNewInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1196);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1196, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcflvcOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1200);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1200, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcflvcOld
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1204);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1204, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcflvcOldInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1208);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1208, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcflvcOldInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1212);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1212, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcflvcNew
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1216);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1216, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcflvcNew
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1220);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1220, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPlcflvcNewInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1224);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1224, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPlcflvcNewInline
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1228);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1228, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPgdMother
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1232);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1232, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPgdMother
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1236);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1236, 4);
		}
	}

	internal uint FibRgFcLcb2003FcBkdMother
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1240);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1240, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbBkdMother
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1244);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1244, 4);
		}
	}

	internal uint FibRgFcLcb2003FcAfdMother
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1248);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1248, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbAfdMother
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1252);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1252, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPgdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1256);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1256, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPgdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1260);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1260, 4);
		}
	}

	internal uint FibRgFcLcb2003FcBkdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1264);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1264, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbBkdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1268);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1268, 4);
		}
	}

	internal uint FibRgFcLcb2003FcAfdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1272);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1272, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbAfdFtn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1276);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1276, 4);
		}
	}

	internal uint FibRgFcLcb2003FcPgdEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1280);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1280, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbPgdEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1284);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1284, 4);
		}
	}

	internal uint FibRgFcLcb2003FcBkdEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1288);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1288, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbBkdEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1292);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1292, 4);
		}
	}

	internal uint FibRgFcLcb2003FcAfdEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1296);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1296, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbAfdEdn
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1300);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1300, 4);
		}
	}

	internal uint FibRgFcLcb2003FcAfd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1304);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1304, 4);
		}
	}

	internal uint FibRgFcLcb2003LcbAfd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1308);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1308, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfmthd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1312);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1312, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfmthd
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1316);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1316, 4);
		}
	}

	internal uint FibRgFcLcb2007FcSttbfBkmkMoveFrom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1320);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1320, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbSttbfBkmkMoveFrom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1324);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1324, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfBkfMoveFrom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1328);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1328, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfBkfMoveFrom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1332);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1332, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfBklMoveFrom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1336);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1336, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfBklMoveFrom
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1340);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1340, 4);
		}
	}

	internal uint FibRgFcLcb2007FcSttbfBkmkMoveTo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1344);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1344, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbSttbfBkmkMoveTo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1348);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1348, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfBkfMoveTo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1352);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1352, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfBkfMoveTo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1356);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1356, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfBklMoveTo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1360);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1360, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfBklMoveTo
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1364);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1364, 4);
		}
	}

	internal uint FibRgFcLcb2007FcUnused1
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1368);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1368, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbUnused1
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1372);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1372, 4);
		}
	}

	internal uint FibRgFcLcb2007FcUnused2
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1376);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1376, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbUnused2
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1380);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1380, 4);
		}
	}

	internal uint FibRgFcLcb2007FcUnused3
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1384);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1384, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbUnused3
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1388);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1388, 4);
		}
	}

	internal uint FibRgFcLcb2007FcSttbfBkmkArto
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1392);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1392, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbSttbfBkmkArto
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1396);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1396, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfBkfArto
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1400);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1400, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfBkfArto
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1404);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1404, 4);
		}
	}

	internal uint FibRgFcLcb2007FcPlcfBklArto
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1408);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1408, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbPlcfBklArto
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1412);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1412, 4);
		}
	}

	internal uint FibRgFcLcb2007FcArtoData
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1416);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1416, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbArtoData
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1420);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1420, 4);
		}
	}

	internal uint FibRgFcLcb2007FcUnused4
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1424);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1424, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbUnused4
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1428);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1428, 4);
		}
	}

	internal uint FibRgFcLcb2007FcUnused5
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1432);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1432, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbUnused5
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1436);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1436, 4);
		}
	}

	internal uint FibRgFcLcb2007FcUnused6
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1440);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1440, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbUnused6
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1444);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1444, 4);
		}
	}

	internal uint FibRgFcLcb2007FcOssTheme
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1448);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1448, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbOssTheme
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1452);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1452, 4);
		}
	}

	internal uint FibRgFcLcb2007FcColorSchemeMapping
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1456);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1456, 4);
		}
	}

	internal uint FibRgFcLcb2007LcbColorSchemeMapping
	{
		get
		{
			return BitConverter.ToUInt32(m_fibRgFcLcbBlob, 1460);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgFcLcbBlob, 1460, 4);
		}
	}

	internal ushort CswNew
	{
		get
		{
			return m_cswNew;
		}
		set
		{
			m_cswNew = value;
		}
	}

	internal ushort NFibNew
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgCswNew, 0);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgCswNew, 0, 2);
		}
	}

	internal ushort CQuickSavesNew
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgCswNew, 2);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgCswNew, 2, 2);
		}
	}

	internal ushort LidThemeOther
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgCswNew, 4);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgCswNew, 4, 2);
		}
	}

	internal ushort LidThemeFE
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgCswNew, 6);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgCswNew, 6, 2);
		}
	}

	internal ushort LidThemeCS
	{
		get
		{
			return BitConverter.ToUInt16(m_fibRgCswNew, 8);
		}
		set
		{
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_fibRgCswNew, 8, 2);
		}
	}

	internal int EncodingCharSize
	{
		get
		{
			if (m_encoding != Encoding.UTF8)
			{
				return 2;
			}
			return 1;
		}
	}

	internal Encoding Encoding
	{
		get
		{
			return m_encoding;
		}
		set
		{
			m_encoding = value;
		}
	}

	internal Fib()
	{
		Initialize();
	}

	private void Initialize()
	{
		m_fibBase = new byte[32];
		WIdent = 42476;
		NFib = 193;
		BaseUnused = 57437;
		Lid = 1033;
		PnNext = 0;
		FDot = false;
		FGlsy = false;
		FComplex = false;
		FHasPic = false;
		CQuickSaves = 15;
		FEncrypted = false;
		FWhichTblStm = true;
		FReadOnlyRecommended = false;
		FWriteReservation = false;
		FExtChar = true;
		FLoadOverride = false;
		FFarEast = false;
		FObfuscated = false;
		NFibBack = 191;
		LKey = 0;
		Envr = 0;
		FMac = false;
		FEmptySpecial = false;
		FLoadOverridePage = false;
		BaseReserved1 = false;
		BaseReserved2 = true;
		FSpare0 = 0;
		BaseReserved3 = 0;
		BaseReserved4 = 0;
		BaseReserved5 = 2048u;
		BaseReserved6 = 2048u;
		m_csw = 14;
		m_fibRgW = new byte[28];
		FibRgWReserved1 = 27234;
		FibRgWReserved2 = 27234;
		FibRgWReserved3 = 14842;
		FibRgWReserved4 = 14842;
		FibRgWLidFE = 1033;
		m_cslw = 22;
		m_fibRgLw = new byte[88];
		RgLwReserved1 = 1546671000;
		RgLwReserved2 = 1546671000;
		RgLwReserved4 = 1048575;
		RgLwReserved7 = 1048575;
		RgLwReserved10 = 1048575;
		m_cbRgFcLcb = 183;
		m_fibRgFcLcbBlob = new byte[1464];
		m_cswNew = 5;
		m_fibRgCswNew = new byte[10];
		NFibNew = 274;
	}

	private void InitializeBeforeRead()
	{
		m_fibBase = new byte[32];
		m_csw = 0;
		m_fibRgW = new byte[28];
		m_cslw = 0;
		m_fibRgLw = new byte[88];
		m_cbRgFcLcb = 0;
		m_fibRgFcLcbBlob = new byte[1464];
		m_cswNew = 0;
		m_fibRgCswNew = new byte[10];
	}

	internal void Read(Stream stream)
	{
		InitializeBeforeRead();
		stream.Position = 0L;
		stream.Read(m_fibBase, 0, 32);
		if (NFib >= 101 && NFib <= 105)
		{
			throw new Exception("This file format is not supported");
		}
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		m_csw = BitConverter.ToUInt16(array, 0);
		stream.Read(m_fibRgW, 0, 28);
		array = new byte[2];
		stream.Read(array, 0, 2);
		m_cslw = BitConverter.ToUInt16(array, 0);
		if (!FEncrypted)
		{
			ReadInternal(stream);
		}
	}

	internal void ReadAfterDecryption(Stream stream)
	{
		stream.Position = 64L;
		ReadInternal(stream);
	}

	private void ReadInternal(Stream stream)
	{
		stream.Read(m_fibRgLw, 0, 88);
		byte[] array = new byte[2];
		stream.Read(array, 0, 2);
		m_cbRgFcLcb = BitConverter.ToUInt16(array, 0);
		if (m_cbRgFcLcb > 183)
		{
			m_cbRgFcLcb = 183;
		}
		int count = m_cbRgFcLcb * 8;
		stream.Read(m_fibRgFcLcbBlob, 0, count);
		array = new byte[2];
		stream.Read(array, 0, 2);
		m_cswNew = BitConverter.ToUInt16(array, 0);
		if (m_cswNew > 5)
		{
			m_cswNew = 5;
		}
		if (m_cswNew != 0)
		{
			count = m_cswNew * 2;
			stream.Read(m_fibRgCswNew, 0, count);
		}
	}

	private void ValidateCbRgFcLcb()
	{
		switch (NFib)
		{
		case 193:
			if (m_cbRgFcLcb != 93)
			{
				m_cbRgFcLcb = 93;
			}
			break;
		case 217:
			if (m_cbRgFcLcb != 108)
			{
				m_cbRgFcLcb = 108;
			}
			break;
		case 257:
			if (m_cbRgFcLcb != 136)
			{
				m_cbRgFcLcb = 136;
			}
			break;
		case 268:
			if (m_cbRgFcLcb != 164)
			{
				m_cbRgFcLcb = 164;
			}
			break;
		case 274:
			if (m_cbRgFcLcb != 183)
			{
				m_cbRgFcLcb = 183;
			}
			break;
		}
	}

	private void ValidateCswNew()
	{
		switch (NFib)
		{
		case 193:
			if (m_cswNew != 0)
			{
				m_cswNew = 0;
			}
			break;
		case 217:
			if (m_cswNew != 2)
			{
				m_cswNew = 2;
			}
			break;
		case 257:
			if (m_cswNew != 2)
			{
				m_cswNew = 2;
			}
			break;
		case 268:
			if (m_cswNew != 2)
			{
				m_cswNew = 2;
			}
			break;
		case 274:
			if (m_cswNew != 5)
			{
				m_cswNew = 5;
			}
			break;
		}
	}

	private void CorrectFib()
	{
		if (CcpHdrTxbx > 0)
		{
			CcpHdrTxbx--;
		}
		else if (CcpTxbx > 0)
		{
			CcpTxbx--;
		}
		else if (CcpEdn > 0)
		{
			CcpEdn--;
		}
		else if (CcpAtn > 0)
		{
			CcpAtn--;
		}
		else if (CcpHdd > 0)
		{
			CcpHdd--;
		}
		else if (CcpFtn > 0)
		{
			CcpFtn--;
		}
	}

	internal void Write(Stream stream, ushort fibVersion)
	{
		CorrectFib();
		WriteInternal(stream);
		stream.Write(m_fibRgLw, 0, m_fibRgLw.Length);
		byte[] bytes = BitConverter.GetBytes(m_cbRgFcLcb);
		stream.Write(bytes, 0, bytes.Length);
		stream.Write(m_fibRgFcLcbBlob, 0, m_cbRgFcLcb * 8);
		bytes = BitConverter.GetBytes(m_cswNew);
		stream.Write(bytes, 0, bytes.Length);
		if (m_cswNew > 0)
		{
			stream.Write(m_fibRgCswNew, 0, m_cswNew * 2);
		}
	}

	private void WriteInternal(Stream stream)
	{
		stream.Position = 0L;
		stream.Write(m_fibBase, 0, m_fibBase.Length);
		byte[] bytes = BitConverter.GetBytes(m_csw);
		stream.Write(bytes, 0, bytes.Length);
		stream.Write(m_fibRgW, 0, m_fibRgW.Length);
		bytes = BitConverter.GetBytes(m_cslw);
		stream.Write(bytes, 0, bytes.Length);
	}

	internal void WriteAfterEncryption(Stream stream)
	{
		WriteInternal(stream);
	}

	internal void UpdateFcMac()
	{
		BaseReserved6 = (uint)(BaseReserved5 + (CcpText + CcpFtn + CcpHdd + CcpAtn + CcpEdn + CcpTxbx + CcpHdrTxbx) * EncodingCharSize);
	}

	internal virtual void Close()
	{
		m_fibBase = null;
		m_fibRgW = null;
		m_fibRgLw = null;
		m_fibRgFcLcbBlob = null;
		m_fibRgCswNew = null;
	}
}
