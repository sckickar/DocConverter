using System;
using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation;

internal class IdReserver
{
	public const int SegmentSize = 1024;

	private Dictionary<int, int> m_id = new Dictionary<int, int>();

	private Dictionary<int, int> m_idCount = new Dictionary<int, int>();

	private Dictionary<int, KeyValuePair<int, int>> m_collectionCount = new Dictionary<int, KeyValuePair<int, int>>();

	private int m_iMaximumId;

	private Dictionary<int, int> m_dictAdditionalShapes = new Dictionary<int, int>();

	public int MaximumId => m_iMaximumId;

	public static int GetSegmentStart(int id)
	{
		int num = id % 1024;
		return id - num;
	}

	public bool CheckReserved(int id)
	{
		int segmentStart = GetSegmentStart(id);
		return m_id.ContainsKey(segmentStart);
	}

	public bool CheckFree(int id, int count)
	{
		int num = GetSegmentStart(id);
		bool flag = true;
		int num2 = 0;
		while (num2 < count && flag)
		{
			flag = !m_id.ContainsKey(num);
			num2++;
			num += 1024;
		}
		return flag;
	}

	public int ReservedBy(int id)
	{
		int num = id % 1024;
		int key = id - num;
		m_id.TryGetValue(key, out var value);
		return value;
	}

	public bool TryReserve(int id, int lastId, int collectionId)
	{
		int num = id;
		id = GetSegmentStart(id);
		m_iMaximumId = Math.Max(m_iMaximumId, lastId);
		int segmentStart = GetSegmentStart(lastId);
		bool flag = false;
		int num2 = (segmentStart - id) / 1024 + 1;
		if (CheckFree(id, num2))
		{
			flag = true;
			if (!m_collectionCount.TryGetValue(collectionId, out var value))
			{
				m_collectionCount.Add(collectionId, new KeyValuePair<int, int>(num2, id));
			}
			else
			{
				int key = value.Key + (int)Math.Ceiling((double)(segmentStart - id + 1) / 1024.0);
				KeyValuePair<int, int> value2 = new KeyValuePair<int, int>(key, value.Value);
				m_collectionCount[collectionId] = value2;
			}
			for (int i = id; i <= lastId && flag; i += 1024)
			{
				m_id.Add(i, collectionId);
			}
			for (int j = num; j <= lastId; j++)
			{
				IncreaseCount(j);
			}
		}
		else
		{
			flag = IsReservedBy(id, segmentStart, collectionId);
		}
		return flag;
	}

	private void IncreaseCount(int id)
	{
		int segmentStart = GetSegmentStart(id);
		int value = ((!m_idCount.TryGetValue(segmentStart, out value)) ? 1 : (value + 1));
		m_idCount[segmentStart] = value;
	}

	private bool IsReservedBy(int id, int lastId, int collectionId)
	{
		bool flag = true;
		for (int i = id; i <= lastId && flag; i += 1024)
		{
			flag = ReservedBy(id) == collectionId;
		}
		return flag;
	}

	private void FreeSegment(int id)
	{
		int segmentStart = GetSegmentStart(id);
		m_id.Remove(segmentStart);
	}

	public void FreeSegmentsSequence(int id, int collectionId)
	{
		while (ReservedBy(id) == collectionId)
		{
			FreeSegment(id);
			id += 1024;
		}
	}

	public void FreeSequence(int collectionId)
	{
		if (m_collectionCount.TryGetValue(collectionId, out var value))
		{
			FreeSegmentsSequence(value.Value, collectionId);
		}
	}

	public int Allocate(int idNumber, int collectionId)
	{
		int i = 1024;
		for (int count = (int)Math.Ceiling((double)idNumber / 1024.0); !CheckFree(i, count); i += 1024)
		{
		}
		int result = i;
		if (!TryReserve(i, i + idNumber, collectionId))
		{
			throw new InvalidOperationException();
		}
		return result;
	}

	public int GetReservedCount(int collectionId)
	{
		if (!m_collectionCount.TryGetValue(collectionId, out var value))
		{
			return 0;
		}
		return value.Key * 1024;
	}

	public int ReservedCount(int id)
	{
		id = GetSegmentStart(id);
		m_idCount.TryGetValue(id, out var value);
		return value;
	}

	public void AddAdditionalShapes(int collectionIndex, int shapesNumber)
	{
		if (!m_dictAdditionalShapes.TryGetValue(collectionIndex, out var value))
		{
			value = 0;
		}
		m_dictAdditionalShapes[collectionIndex] = value + shapesNumber;
	}

	public int GetAdditionalShapesNumber(int collectionIndex)
	{
		m_dictAdditionalShapes.TryGetValue(collectionIndex, out var value);
		return value;
	}
}
