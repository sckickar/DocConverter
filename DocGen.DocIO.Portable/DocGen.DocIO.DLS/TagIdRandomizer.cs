using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class TagIdRandomizer
{
	[ThreadStatic]
	private static Random m_instance;

	[ThreadStatic]
	private static List<int> m_ids;

	[ThreadStatic]
	private static List<int> m_noneChangeIds;

	[ThreadStatic]
	private static Dictionary<int, int> m_changedIds;

	internal static Random Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new Random(1000);
			}
			return m_instance;
		}
	}

	internal static Dictionary<int, int> ChangedIds
	{
		get
		{
			if (m_changedIds == null)
			{
				m_changedIds = new Dictionary<int, int>();
			}
			return m_changedIds;
		}
	}

	internal static List<int> Identificators
	{
		get
		{
			if (m_ids == null)
			{
				m_ids = new List<int>();
			}
			return m_ids;
		}
	}

	internal static List<int> NoneChangeIds
	{
		get
		{
			if (m_noneChangeIds == null)
			{
				m_noneChangeIds = new List<int>();
			}
			return m_noneChangeIds;
		}
	}

	internal static int GetId(int currentId)
	{
		int num = -1;
		if (!ChangedIds.ContainsKey(currentId))
		{
			num = Instance.Next();
			ChangedIds.Add(currentId, num);
			Identificators.Add(num);
		}
		else
		{
			num = ChangedIds[currentId];
			if (IsValidId(num))
			{
				Identificators.Add(num);
			}
			else
			{
				num = Instance.Next();
				Identificators.Add(num);
			}
		}
		return num;
	}

	private static bool IsValidId(int newId)
	{
		bool result = true;
		if (m_ids != null && m_ids.Count > 0)
		{
			foreach (int id in m_ids)
			{
				if (id == newId)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	internal static int GetMarkerId(int currentId, bool newId)
	{
		if (NoneChangeIds.Contains(currentId))
		{
			return currentId;
		}
		if (!ChangedIds.ContainsKey(currentId) || newId)
		{
			int num = Instance.Next();
			if (!ChangedIds.ContainsKey(currentId))
			{
				ChangedIds.Add(currentId, num);
			}
			else
			{
				ChangedIds[currentId] = num;
			}
			return num;
		}
		return ChangedIds[currentId];
	}
}
