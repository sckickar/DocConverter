using System;
using System.Runtime.InteropServices;
using System.Security;

namespace DocGen.OfficeChart.Parser;

internal class MemoryConverter
{
	private const int DEF_MEMORY_BLOCK_SIZE = 8228;

	private const int DEF_MIN_BLOCK_SIZE = 1024;

	private const int DEF_MAX_BLOCK_SIZE = int.MaxValue;

	private const string DEF_OUT_OF_MEMORY_MSG = "Application was unable to allocate memory block";

	private nint m_memoryBlock;

	private int m_iMemoryBlockSize;

	[SecurityCritical]
	public MemoryConverter()
		: this(8228)
	{
	}

	[SecurityCritical]
	public MemoryConverter(int iMemoryBlockSize)
	{
		if (iMemoryBlockSize < 1024)
		{
			iMemoryBlockSize = 1024;
		}
		if (iMemoryBlockSize > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("iMemoryBlock", "Value cannot be greater " + int.MaxValue);
		}
		m_memoryBlock = Marshal.AllocCoTaskMem(iMemoryBlockSize);
		m_iMemoryBlockSize = iMemoryBlockSize;
		if (((IntPtr)m_memoryBlock).ToInt64() == 0L)
		{
			throw new OutOfMemoryException("Application was unable to allocate memory block");
		}
	}

	[SecurityCritical]
	public void EnsureMemoryBlockSize(int iDesiredSize)
	{
		if (iDesiredSize > m_iMemoryBlockSize)
		{
			if (iDesiredSize > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("iDesiredSize", "Value cannot be greater than " + int.MaxValue);
			}
			m_memoryBlock = Marshal.ReAllocCoTaskMem(m_memoryBlock, iDesiredSize);
			m_iMemoryBlockSize = iDesiredSize;
			if (((IntPtr)m_memoryBlock).ToInt64() == 0L)
			{
				throw new OutOfMemoryException("Application was unable to allocate memory block");
			}
		}
	}

	[SecurityCritical]
	public void CopyFrom(byte[] arrData)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		int num = arrData.Length;
		if (num != 0)
		{
			EnsureMemoryBlockSize(num);
			Marshal.Copy(arrData, 0, m_memoryBlock, num);
		}
	}

	[SecurityCritical]
	public void CopyFrom(byte[] arrData, int iStartIndex)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iStartIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iStartIndex");
		}
		int num = arrData.Length - iStartIndex;
		if (num != 0)
		{
			EnsureMemoryBlockSize(num);
			Marshal.Copy(arrData, iStartIndex, m_memoryBlock, num);
		}
	}

	[SecurityCritical]
	public void CopyFrom(byte[] arrData, int iStartIndex, int iCount)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iStartIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iStartIndex");
		}
		if (iCount > 0)
		{
			if (arrData.Length - iStartIndex < iCount)
			{
				throw new ArgumentOutOfRangeException("iCount is too large");
			}
			EnsureMemoryBlockSize(iCount);
			Marshal.Copy(arrData, iStartIndex, m_memoryBlock, iCount);
		}
	}

	[SecurityCritical]
	public object CopyTo(Type destType)
	{
		if (destType == null)
		{
			throw new ArgumentNullException("destination");
		}
		return Marshal.PtrToStructure(m_memoryBlock, destType);
	}

	[SecurityCritical]
	public void CopyTo(object destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		Marshal.PtrToStructure(m_memoryBlock, destination);
	}

	[SecurityCritical]
	public void Copy(byte[] arrData, object destination)
	{
		CopyFrom(arrData);
		CopyTo(destination);
	}

	[SecurityCritical]
	public void Copy(byte[] arrData, int iStartIndex, object destination)
	{
		CopyFrom(arrData, iStartIndex);
		CopyTo(destination);
	}

	[SecurityCritical]
	public void Copy(byte[] arrData, int iStartIndex, int iCount, object destination)
	{
		CopyFrom(arrData, iStartIndex, iCount);
		CopyTo(destination);
	}

	[SecurityCritical]
	public void CopyObject(object source, byte[] arrDestination, int iCount)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (arrDestination == null)
		{
			throw new ArgumentNullException("arrDestination");
		}
		if (arrDestination.Length < iCount)
		{
			throw new ArgumentException("arrDestination", "Array too short");
		}
		EnsureMemoryBlockSize(iCount);
		Marshal.StructureToPtr(source, m_memoryBlock, fDeleteOld: false);
		Marshal.Copy(m_memoryBlock, arrDestination, 0, iCount);
		Marshal.DestroyStructure(m_memoryBlock, source.GetType());
	}
}
