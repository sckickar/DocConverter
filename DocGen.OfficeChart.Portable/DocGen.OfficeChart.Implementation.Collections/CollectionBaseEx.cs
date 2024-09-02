using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class CollectionBaseEx<T> : CollectionBase<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IParentApplication, ICloneParent
{
	public delegate void CollectionClear();

	public delegate void CollectionChange(object sender, CollectionChangeEventArgs<T> args);

	public delegate void CollectionSet(int index, object old, object value);

	private IApplication m_appl;

	private object m_parent;

	private bool m_bSkipEvents;

	private static Dictionary<string, int> m_dictCollectionsMaxValues = new Dictionary<string, int>();

	private static readonly object m_threadLocker = new object();

	public IApplication Application
	{
		[DebuggerStepThrough]
		get
		{
			return m_appl;
		}
	}

	public object Parent
	{
		[DebuggerStepThrough]
		get
		{
			return m_parent;
		}
	}

	public bool QuietMode
	{
		get
		{
			return m_bSkipEvents;
		}
		set
		{
			if (value != m_bSkipEvents)
			{
				m_bSkipEvents = value;
			}
		}
	}

	protected ApplicationImpl AppImplementation
	{
		[DebuggerStepThrough]
		get
		{
			return (ApplicationImpl)Application;
		}
	}

	public event EventHandler Changed;

	public event CollectionClear Clearing;

	public event CollectionClear Cleared;

	public event CollectionChange Inserting;

	public event CollectionChange Inserted;

	public event CollectionChange Removing;

	public event CollectionChange Removed;

	public event CollectionSet Setting;

	public event CollectionSet Set;

	private CollectionBaseEx()
	{
	}

	public CollectionBaseEx(IApplication application, object parent)
		: this()
	{
		m_appl = application;
		m_parent = parent;
	}

	private void RaiseChangedEvent()
	{
		if (this.Changed != null && !m_bSkipEvents)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}

	protected override void OnClear()
	{
		if (this.Clearing != null && !m_bSkipEvents)
		{
			this.Clearing();
		}
		m_dictCollectionsMaxValues.Clear();
		base.OnClear();
	}

	protected override void OnClearComplete()
	{
		if (this.Cleared != null && !m_bSkipEvents)
		{
			this.Cleared();
		}
		base.OnClearComplete();
		RaiseChangedEvent();
	}

	protected override void OnInsert(int index, T value)
	{
		if (this.Inserting != null && !m_bSkipEvents)
		{
			this.Inserting(this, new CollectionChangeEventArgs<T>(index, value));
		}
		base.OnInsert(index, value);
	}

	protected override void OnInsertComplete(int index, T value)
	{
		if (this.Inserted != null && !m_bSkipEvents)
		{
			this.Inserted(this, new CollectionChangeEventArgs<T>(index, value));
		}
		base.OnInsertComplete(index, value);
		RaiseChangedEvent();
	}

	protected override void OnRemove(int index, T value)
	{
		if (this.Removing != null && !m_bSkipEvents)
		{
			this.Removing(this, new CollectionChangeEventArgs<T>(index, value));
		}
		base.OnRemove(index, value);
	}

	protected override void OnRemoveComplete(int index, T value)
	{
		if (this.Removed != null && !m_bSkipEvents)
		{
			this.Removed(this, new CollectionChangeEventArgs<T>(index, value));
		}
		base.OnRemoveComplete(index, value);
		RaiseChangedEvent();
	}

	protected override void OnSet(int index, T oldValue, T newValue)
	{
		if (this.Setting != null && !m_bSkipEvents)
		{
			this.Setting(index, oldValue, newValue);
		}
		base.OnSet(index, oldValue, newValue);
	}

	protected override void OnSetComplete(int index, T oldValue, T newValue)
	{
		if (this.Set != null && !m_bSkipEvents)
		{
			this.Set(index, oldValue, newValue);
		}
		base.OnSetComplete(index, oldValue, newValue);
		RaiseChangedEvent();
	}

	public object FindParent(Type parentType)
	{
		return FindParent(parentType, bCheckSubclasses: false);
	}

	public object FindParent(Type parentType, bool bCheckSubclasses)
	{
		int num = 0;
		IParentApplication parentApplication = (IParentApplication)Parent;
		bool isInterface = parentType.GetTypeInfo().IsInterface;
		do
		{
			if (num > 100)
			{
				throw new ArgumentException("links Cycle in object tree detected!");
			}
			if (parentApplication == null || parentApplication.Parent == null)
			{
				break;
			}
			Type type = parentApplication.GetType();
			if (!isInterface)
			{
				if (type.Equals(parentType))
				{
					break;
				}
				if (bCheckSubclasses)
				{
					type.GetTypeInfo().IsSubclassOf(parentType);
					break;
				}
			}
			parentApplication = (IParentApplication)parentApplication.Parent;
			num++;
		}
		while (parentApplication != null);
		return parentApplication;
	}

	public void SetParent(object parent)
	{
		m_parent = parent;
	}

	public virtual object Clone(object parent)
	{
		ConstructorInfo? constructor = GetType().GetConstructor(new Type[2]
		{
			typeof(IApplication),
			typeof(object)
		});
		if (constructor == null)
		{
			throw new ApplicationException("Cannot find required constructor.");
		}
		CollectionBaseEx<T> collectionBaseEx = constructor.Invoke(new object[2] { Application, parent }) as CollectionBaseEx<T>;
		List<T> innerList = base.InnerList;
		int i = 0;
		for (int count = innerList.Count; i < count; i++)
		{
			T val = innerList[i];
			if (val is ICloneParent)
			{
				val = (T)((ICloneParent)(object)val).Clone(collectionBaseEx);
			}
			else if (val is ICloneable)
			{
				val = (T)((ICloneable)(object)val).Clone();
			}
			collectionBaseEx.Add(val);
		}
		return collectionBaseEx;
	}

	public void EnsureCapacity(int size)
	{
		if (base.InnerList.Capacity < size)
		{
			base.InnerList.Capacity = size;
		}
	}

	public static string GenerateDefaultName(ICollection<T> namesCollection, string strStart)
	{
		int num = 1;
		int length = strStart.Length;
		lock (m_threadLocker)
		{
			if (m_dictCollectionsMaxValues.ContainsKey(strStart))
			{
				num = m_dictCollectionsMaxValues[strStart];
				num++;
				m_dictCollectionsMaxValues[strStart] = num;
			}
			else
			{
				foreach (T item in namesCollection)
				{
					string name = ((INamedObject)(object)item).Name;
					if (name != null && name.StartsWith(strStart) && double.TryParse(name.Substring(length, name.Length - length), NumberStyles.Integer, null, out var result))
					{
						num = Math.Max((int)result + 1, num);
					}
				}
				m_dictCollectionsMaxValues.Add(strStart, num);
			}
		}
		return strStart + num;
	}

	public static string GenerateDefaultName(ICollection namesCollection, string strStart)
	{
		int val = 1;
		int length = strStart.Length;
		foreach (INamedObject item in namesCollection)
		{
			string name = item.Name;
			if (name != null && name.StartsWith(strStart) && double.TryParse(name.Substring(length, name.Length - length), NumberStyles.Integer, null, out var result))
			{
				val = Math.Max((int)result + 1, val);
			}
		}
		return strStart + val;
	}

	public static string GenerateDefaultName(string strStart, params ICollection[] arrCollections)
	{
		int val = 1;
		int length = strStart.Length;
		int i = 0;
		for (int num = arrCollections.Length; i < num; i++)
		{
			foreach (object item in arrCollections[i])
			{
				string text = ((!(item is INamedObject)) ? item.ToString() : (item as INamedObject).Name);
				if (text.StartsWith(strStart) && double.TryParse(text.Substring(length, text.Length - length), NumberStyles.Integer, null, out var result))
				{
					val = Math.Max((int)result + 1, val);
				}
			}
		}
		return strStart + val;
	}

	internal static int GenerateID(ICollection<T> shapeCollection)
	{
		int num = 1;
		foreach (T item in shapeCollection)
		{
			IShape shape = (IShape)(object)item;
			if (num < shape.Id)
			{
				num = shape.Id;
			}
		}
		return num + 1;
	}

	public static void ChangeName(IDictionary hashNames, ValueChangedEventArgs e)
	{
		string text = (string)e.oldValue;
		string text2 = (string)e.newValue;
		if (!hashNames.Contains(text))
		{
			throw new ArgumentOutOfRangeException("Collection does not contain object named " + text);
		}
		if (hashNames.Contains(text2))
		{
			throw new ArgumentOutOfRangeException("Collection already contains object named " + text2);
		}
		object value = hashNames[text];
		hashNames.Remove(text);
		hashNames.Add(text2, value);
	}
}
