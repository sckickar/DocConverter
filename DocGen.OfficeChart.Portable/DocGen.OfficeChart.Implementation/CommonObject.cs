using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace DocGen.OfficeChart.Implementation;

internal class CommonObject : IParentApplication, IDisposable
{
	private IApplication m_appl;

	private object m_parent;

	private int m_iReferenceCount;

	protected bool m_bIsDisposed;

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

	public ApplicationImpl AppImplementation => (ApplicationImpl)m_appl;

	public int ReferenceCount
	{
		[DebuggerStepThrough]
		get
		{
			return m_iReferenceCount;
		}
	}

	protected CommonObject()
	{
	}

	public CommonObject(IApplication application, object parent)
		: this()
	{
		if (application == null)
		{
			throw new ArgumentNullException("application");
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_appl = application;
		m_parent = parent;
	}

	~CommonObject()
	{
		Dispose();
	}

	public object FindParent(Type parentType)
	{
		return FindParent(m_parent, parentType);
	}

	public object FindParent(Type parentType, bool bSubTypes)
	{
		return FindParent(m_parent, parentType, bSubTypes);
	}

	public static object FindParent(object parentStart, Type parentType)
	{
		return FindParent(parentStart, parentType, bSubTypes: false);
	}

	public static object FindParent(object parentStart, Type parentType, bool bSubTypes)
	{
		if (parentType == null)
		{
			throw new ArgumentNullException("parentType");
		}
		int num = 0;
		IParentApplication parentApplication = (IParentApplication)parentStart;
		bool isInterface = parentType.GetTypeInfo().IsInterface;
		do
		{
			if (num > 100)
			{
				throw new ArgumentException("links Cycle in object tree detected!");
			}
			if (parentApplication == null)
			{
				break;
			}
			Type type = parentApplication.GetType();
			if (!isInterface)
			{
				if (type.Equals(parentType) || (bSubTypes && type.GetTypeInfo().IsSubclassOf(parentType)))
				{
					break;
				}
			}
			else if (type.GetInterface(parentType.Name, ignoreCase: false) != null)
			{
				break;
			}
			parentApplication = (IParentApplication)parentApplication.Parent;
			num++;
		}
		while (parentApplication != null);
		return parentApplication;
	}

	public object[] FindParents(Type[] arrTypes)
	{
		int num = 0;
		IParentApplication parentApplication = (IParentApplication)m_parent;
		object[] array = new object[arrTypes.Length];
		do
		{
			if (num > 100)
			{
				throw new ArgumentException("links Cycle in object tree detected!");
			}
			if (parentApplication == null)
			{
				break;
			}
			int num2 = Array.IndexOf(arrTypes, parentApplication.GetType());
			if (num2 != -1)
			{
				array[num2] = parentApplication;
			}
			parentApplication = (IParentApplication)parentApplication.Parent;
			num++;
		}
		while (parentApplication != null);
		return array;
	}

	public object FindParent(Type[] arrTypes)
	{
		int num = 0;
		IParentApplication parentApplication = (IParentApplication)Parent;
		do
		{
			if (num > 100)
			{
				throw new ArgumentException("links Cycle in object tree detected!");
			}
			if (parentApplication == null)
			{
				break;
			}
			if (Array.IndexOf(arrTypes, parentApplication.GetType()) != -1)
			{
				return parentApplication;
			}
			parentApplication = (IParentApplication)parentApplication.Parent;
			num++;
		}
		while (parentApplication != null);
		return parentApplication;
	}

	protected internal void SetParent(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_parent = parent;
	}

	protected void CheckDisposed()
	{
		if (m_bIsDisposed)
		{
			throw new ApplicationException("Object was disposed.");
		}
	}

	[DebuggerStepThrough]
	public virtual int AddReference()
	{
		return Interlocked.Increment(ref m_iReferenceCount);
	}

	[DebuggerStepThrough]
	public virtual int ReleaseReference()
	{
		return Interlocked.Decrement(ref m_iReferenceCount);
	}

	public virtual void Dispose()
	{
		if (!m_bIsDisposed)
		{
			OnDispose();
			m_parent = null;
			m_appl = null;
			m_bIsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}

	protected virtual void OnDispose()
	{
	}

	protected void SetParent(IApplication application, object parent)
	{
		if (application == null)
		{
			throw new ArgumentNullException("application");
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		m_appl = application;
		m_parent = parent;
	}
}
