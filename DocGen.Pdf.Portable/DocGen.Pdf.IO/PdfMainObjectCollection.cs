using System;
using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.IO;

internal class PdfMainObjectCollection
{
	private List<ObjectInfo> m_objectCollection = new List<ObjectInfo>();

	internal Dictionary<long, ObjectInfo> mainObjectCollection = new Dictionary<long, ObjectInfo>();

	private Dictionary<IPdfPrimitive, long> m_primitiveObjectCollection = new Dictionary<IPdfPrimitive, long>();

	private HashSet<IPdfPrimitive> m_primitiveObjects = new HashSet<IPdfPrimitive>();

	private int m_index;

	internal int m_maximumReferenceObjNumber;

	protected static object s_syncObject = new object();

	internal ObjectInfo this[int index]
	{
		get
		{
			if (index < 0 || index > m_objectCollection.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_objectCollection[index];
		}
	}

	internal int Count => m_objectCollection.Count;

	internal PdfMainObjectCollection()
	{
	}

	internal void Add(IPdfPrimitive element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (element is IPdfWrapper)
		{
			element = (element as IPdfWrapper).Element;
		}
		ObjectInfo item = new ObjectInfo(element);
		m_objectCollection.Add(item);
		m_primitiveObjects.Add(element);
		if (!m_primitiveObjectCollection.ContainsKey(element))
		{
			m_primitiveObjectCollection.Add(element, m_objectCollection.Count - 1);
		}
		element.Position = (m_index = m_objectCollection.Count - 1);
		element.Status = ObjectStatus.Registered;
	}

	internal void Add(IPdfPrimitive obj, PdfReference reference)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("element");
		}
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		if (obj is IPdfWrapper)
		{
			obj = (obj as IPdfWrapper).Element;
		}
		lock (s_syncObject)
		{
			ObjectInfo objectInfo = new ObjectInfo(obj, reference);
			if (m_maximumReferenceObjNumber < reference.ObjNum)
			{
				m_maximumReferenceObjNumber = (int)reference.ObjNum;
			}
			m_objectCollection.Add(objectInfo);
			m_primitiveObjects.Add(objectInfo.Object);
			if (!m_primitiveObjectCollection.ContainsKey(objectInfo.Object))
			{
				m_primitiveObjectCollection.Add(objectInfo.Object, m_objectCollection.Count - 1);
			}
			mainObjectCollection.Add(reference.ObjNum, objectInfo);
		}
		IPdfPrimitive pdfPrimitive = obj;
		int position = (reference.Position = m_objectCollection.Count - 1);
		pdfPrimitive.Position = position;
	}

	internal void Remove(int index)
	{
		lock (s_syncObject)
		{
			if (m_objectCollection.Count > index)
			{
				IPdfPrimitive @object = m_objectCollection[index].Object;
				if (m_primitiveObjects.Contains(@object))
				{
					m_primitiveObjects.Remove(@object);
				}
			}
			if (m_objectCollection.Count > index)
			{
				m_objectCollection.RemoveAt(index);
			}
		}
	}

	internal bool Contains(IPdfPrimitive element)
	{
		return LookFor(element) >= 0;
	}

	internal bool ContainsReference(PdfReference reference)
	{
		return mainObjectCollection.ContainsKey(reference.ObjNum);
	}

	internal PdfReference GetReference(int index)
	{
		return m_objectCollection[index].Reference;
	}

	internal PdfReference GetReference(IPdfPrimitive obj, out bool isNew)
	{
		m_index = LookFor(obj);
		if (m_index < 0 || m_index > Count)
		{
			isNew = true;
			return null;
		}
		isNew = false;
		return m_objectCollection[m_index].Reference;
	}

	internal IPdfPrimitive GetObject(int index)
	{
		return m_objectCollection[index].Object;
	}

	internal IPdfPrimitive GetObject(PdfReference reference)
	{
		try
		{
			return mainObjectCollection[reference.ObjNum].Object;
		}
		catch
		{
			return null;
		}
	}

	internal int GetObjectIndex(PdfReference reference)
	{
		int result = -1;
		if (reference.Position != -1)
		{
			return reference.Position;
		}
		if (mainObjectCollection.Count == 0)
		{
			if (m_objectCollection.Count == 0)
			{
				return result;
			}
			for (int i = 0; i < m_objectCollection.Count - 1; i++)
			{
				mainObjectCollection.Add(m_objectCollection[i].Reference.ObjNum, m_objectCollection[i]);
			}
			if (!mainObjectCollection.ContainsKey(reference.ObjNum))
			{
				return result;
			}
			return 0;
		}
		if (!mainObjectCollection.ContainsKey(reference.ObjNum))
		{
			return result;
		}
		return 0;
	}

	internal bool TrySetReference(IPdfPrimitive obj, PdfReference reference, out bool found)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		bool result = true;
		found = true;
		m_index = LookFor(obj);
		if (m_index < 0 || m_index >= m_objectCollection.Count)
		{
			result = false;
			found = false;
		}
		else
		{
			ObjectInfo objectInfo = m_objectCollection[m_index];
			if (objectInfo.Reference != null)
			{
				result = false;
			}
			else
			{
				objectInfo.SetReference(reference);
			}
		}
		return result;
	}

	internal int IndexOf(IPdfPrimitive element)
	{
		return LookFor(element);
	}

	internal void ReregisterReference(int oldObjIndex, IPdfPrimitive newObj)
	{
		if (newObj == null)
		{
			throw new ArgumentNullException("newObj");
		}
		if (oldObjIndex < 0 || oldObjIndex > Count)
		{
			throw new ArgumentOutOfRangeException("oldObjectIndex");
		}
		ObjectInfo objectInfo = m_objectCollection[oldObjIndex];
		if (objectInfo.Object != newObj)
		{
			m_primitiveObjectCollection.Remove(objectInfo.Object);
			m_primitiveObjectCollection.Add(newObj, oldObjIndex);
			m_primitiveObjects.Remove(objectInfo.Object);
			m_primitiveObjects.Add(newObj);
		}
		objectInfo.Object = newObj;
		newObj.Position = oldObjIndex;
	}

	internal void ReregisterReference(IPdfPrimitive oldObj, IPdfPrimitive newObj)
	{
		if (oldObj == null)
		{
			throw new ArgumentNullException("oldObj");
		}
		if (newObj == null)
		{
			throw new ArgumentNullException("newObj");
		}
		int num = IndexOf(oldObj);
		if (num < 0)
		{
			throw new ArgumentException("Can't reregister an object.", "oldObj");
		}
		ReregisterReference(num, newObj);
	}

	private int LookFor(IPdfPrimitive obj)
	{
		lock (s_syncObject)
		{
			int result = -1;
			if (obj.Position != -1)
			{
				return obj.Position;
			}
			if (Count == m_primitiveObjectCollection.Count)
			{
				if (m_primitiveObjectCollection.ContainsKey(obj))
				{
					result = GetObjectIndex(obj);
				}
				else if (obj.ClonedObject != null && m_primitiveObjectCollection.ContainsKey(obj.ClonedObject))
				{
					result = (int)m_primitiveObjectCollection[obj.ClonedObject];
				}
			}
			else if (m_primitiveObjects.Contains(obj))
			{
				result = GetObjectIndex(obj);
			}
			return result;
		}
	}

	private int GetObjectIndex(IPdfPrimitive obj)
	{
		int num = (int)m_primitiveObjectCollection[obj];
		if (Count > num)
		{
			ObjectInfo objectInfo = m_objectCollection[num];
			if (objectInfo.Object != obj)
			{
				if (Count > num - 1)
				{
					objectInfo = m_objectCollection[num - 1];
				}
				if (objectInfo.Object == obj)
				{
					return num - 1;
				}
				for (int num2 = Count - 1; num2 >= 0; num2--)
				{
					if (m_objectCollection[num2].Object == obj)
					{
						num = num2;
						break;
					}
				}
			}
		}
		else if (Count <= num)
		{
			num = -1;
			for (int num3 = Count - 1; num3 >= 0; num3--)
			{
				if (m_objectCollection[num3].Object == obj)
				{
					num = num3;
					break;
				}
			}
		}
		return num;
	}

	private int LookForReference(PdfReference reference)
	{
		int result = -1;
		if (reference.Position != -1)
		{
			return reference.Position;
		}
		for (int num = Count - 1; num >= 0; num--)
		{
			if (m_objectCollection[num].Reference == reference)
			{
				result = num;
				break;
			}
		}
		return result;
	}
}
