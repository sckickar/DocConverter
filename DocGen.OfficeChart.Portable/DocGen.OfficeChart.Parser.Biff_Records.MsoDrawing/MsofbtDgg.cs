using System;
using System.Collections.Generic;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtDgg)]
[CLSCompliant(false)]
internal class MsofbtDgg : MsoBase
{
	public class ClusterID : ICloneable
	{
		private const int DEF_SIZE = 8;

		private uint m_uiGroupId;

		private uint m_uiNumber;

		public uint GroupId
		{
			get
			{
				return m_uiGroupId;
			}
			set
			{
				m_uiGroupId = value;
			}
		}

		public uint Number
		{
			get
			{
				return m_uiNumber;
			}
			set
			{
				m_uiNumber = value;
			}
		}

		public static int Size => 8;

		public ClusterID(uint groupId, uint number)
		{
			m_uiGroupId = groupId;
			m_uiNumber = number;
		}

		public ClusterID(byte[] data, int iOffset)
		{
			GroupId = BitConverter.ToUInt32(data, iOffset);
			iOffset += 4;
			Number = BitConverter.ToUInt32(data, iOffset);
		}

		public ClusterID(Stream stream)
		{
			GroupId = MsoBase.ReadUInt32(stream);
			Number = MsoBase.ReadUInt32(stream);
		}

		public byte[] GetBytes()
		{
			byte[] array = new byte[Size];
			BitConverter.GetBytes(m_uiGroupId).CopyTo(array, 0);
			BitConverter.GetBytes(m_uiNumber).CopyTo(array, 4);
			return array;
		}

		public void Write(Stream stream)
		{
			MsoBase.WriteUInt32(stream, m_uiGroupId);
			MsoBase.WriteUInt32(stream, m_uiNumber);
		}

		public object Clone()
		{
			return (ClusterID)MemberwiseClone();
		}
	}

	private const int DEF_ARRAY_OFFSET = 16;

	[BiffRecordPos(0, 4)]
	private uint m_uiIdMax;

	[BiffRecordPos(4, 4)]
	private uint m_uiNumberOfIdClus;

	[BiffRecordPos(8, 4)]
	private uint m_uiTotalShapes;

	[BiffRecordPos(12, 4)]
	private uint m_uiTotalDrawings;

	private List<ClusterID> m_arrClusters = new List<ClusterID>();

	public uint IdMax
	{
		get
		{
			return m_uiIdMax;
		}
		set
		{
			m_uiIdMax = value;
		}
	}

	public uint NumberOfIdClus => m_uiNumberOfIdClus;

	public uint TotalShapes
	{
		get
		{
			return m_uiTotalShapes;
		}
		set
		{
			m_uiTotalShapes = value;
		}
	}

	public uint TotalDrawings
	{
		get
		{
			return m_uiTotalDrawings;
		}
		set
		{
			m_uiTotalDrawings = value;
		}
	}

	public ClusterID[] ClusterIDs => m_arrClusters.ToArray();

	public MsofbtDgg(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtDgg(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public override void ParseStructure(Stream stream)
	{
		m_uiIdMax = MsoBase.ReadUInt32(stream);
		m_uiNumberOfIdClus = MsoBase.ReadUInt32(stream);
		m_uiTotalShapes = MsoBase.ReadUInt32(stream);
		m_uiTotalDrawings = MsoBase.ReadUInt32(stream);
		int num = 16;
		if (m_uiNumberOfIdClus != 0)
		{
			int num2 = 0;
			while (num2 < m_uiNumberOfIdClus - 1)
			{
				ClusterID item = new ClusterID(stream);
				m_arrClusters.Add(item);
				num2++;
				num += ClusterID.Size;
			}
		}
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		MsoBase.WriteUInt32(stream, m_uiIdMax);
		MsoBase.WriteUInt32(stream, m_uiNumberOfIdClus);
		MsoBase.WriteUInt32(stream, m_uiTotalShapes);
		MsoBase.WriteUInt32(stream, m_uiTotalDrawings);
		m_iLength = 16;
		int num = 0;
		int count = m_arrClusters.Count;
		while (num < count)
		{
			m_arrClusters[num].Write(stream);
			num++;
			m_iLength += ClusterID.Size;
		}
	}

	protected override object InternalClone()
	{
		MsofbtDgg msofbtDgg = (MsofbtDgg)base.InternalClone();
		if (m_arrClusters != null)
		{
			int count = m_arrClusters.Count;
			List<ClusterID> list = new List<ClusterID>(count);
			for (int i = 0; i < count; i++)
			{
				ClusterID item = (ClusterID)m_arrClusters[i].Clone();
				list.Add(item);
			}
			msofbtDgg.m_arrClusters = list;
		}
		return msofbtDgg;
	}

	public void AddCluster(uint uiGroupId, uint uiNumber)
	{
		ClusterID item = new ClusterID(uiGroupId, uiNumber);
		m_arrClusters.Add(item);
		m_uiNumberOfIdClus = (uint)(m_arrClusters.Count + 1);
		m_uiTotalDrawings = (uint)m_arrClusters.Count;
	}
}
