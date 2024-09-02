using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Shapes;

internal class GroupShapeImpl : ShapeImpl, IGroupShape, IShape, IParentApplication
{
	private IShape[] m_items;

	private bool m_flipVertical;

	private bool m_flipHorizontal;

	private new Dictionary<string, Stream> m_preservedElements;

	public IShape[] Items
	{
		get
		{
			return m_items;
		}
		internal set
		{
			m_items = value;
		}
	}

	internal bool FlipVertical
	{
		get
		{
			return m_flipVertical;
		}
		set
		{
			m_flipVertical = value;
		}
	}

	internal bool FlipHorizontal
	{
		get
		{
			return m_flipHorizontal;
		}
		set
		{
			m_flipHorizontal = value;
		}
	}

	internal new Dictionary<string, Stream> PreservedElements
	{
		get
		{
			if (m_preservedElements == null)
			{
				m_preservedElements = new Dictionary<string, Stream>();
			}
			return m_preservedElements;
		}
	}

	internal GroupShapeImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_bSupportOptions = true;
		base.ShapeType = OfficeShapeType.Group;
	}

	public override IShape Clone(object parent, Dictionary<string, string> hashNewNames, Dictionary<int, int> dicFontIndexes, bool addToCollections)
	{
		GroupShapeImpl groupShapeImpl = (GroupShapeImpl)base.Clone(parent, hashNewNames, dicFontIndexes, addToCollections);
		if (m_items != null && m_items.Length != 0)
		{
			List<IShape> list = new List<IShape>();
			for (int i = 0; i < m_items.Length; i++)
			{
				IShape item = (m_items[i] as ShapeImpl).Clone(groupShapeImpl, hashNewNames, dicFontIndexes, addToCollections: false);
				list.Add(item);
			}
			groupShapeImpl.m_items = list.ToArray();
		}
		groupShapeImpl.m_preservedElements = CloneUtils.CloneHash(PreservedElements);
		return groupShapeImpl;
	}

	internal void LayoutGroupShape(bool isAll)
	{
		UpdateGroupFrame(isAll);
		if (Items == null)
		{
			return;
		}
		IShape[] items = Items;
		for (int i = 0; i < items.Length; i++)
		{
			ShapeImpl shapeImpl = (ShapeImpl)items[i];
			if (shapeImpl.IsGroup)
			{
				(shapeImpl as GroupShapeImpl).LayoutGroupShape(isAll);
				continue;
			}
			shapeImpl.UpdateGroupFrame(isAll);
			if (shapeImpl.GroupFrame != null)
			{
				RectangleF rect = new RectangleF(shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
				rect = UpdateShapeBounds(rect, shapeImpl.GroupFrame.Rotation / 60000);
				shapeImpl.GroupFrame.SetAnchor(shapeImpl.GroupFrame.Rotation, (long)rect.X, (long)rect.Y, (long)rect.Width, (long)rect.Height);
			}
		}
	}

	internal void LayoutGroupShape()
	{
		if (Items == null)
		{
			return;
		}
		IShape[] items = Items;
		for (int i = 0; i < items.Length; i++)
		{
			ShapeImpl shapeImpl = (ShapeImpl)items[i];
			if (shapeImpl.IsGroup)
			{
				shapeImpl.UpdateGroupFrame();
				(shapeImpl as GroupShapeImpl).LayoutGroupShape();
			}
			else
			{
				shapeImpl.UpdateGroupFrame();
			}
		}
	}

	internal void SetUpdatedChildOffset()
	{
		base.ShapeFrame.SetChildAnchor(base.ShapeFrame.OffsetX, base.ShapeFrame.OffsetY, base.ShapeFrame.OffsetCX, base.ShapeFrame.OffsetCY);
		if (Items == null)
		{
			return;
		}
		IShape[] items = Items;
		for (int i = 0; i < items.Length; i++)
		{
			ShapeImpl shapeImpl = (ShapeImpl)items[i];
			if (shapeImpl.IsGroup)
			{
				if (shapeImpl.GroupFrame != null)
				{
					shapeImpl.SetPostion(shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
					shapeImpl.ShapeFrame.SetAnchor(shapeImpl.GroupFrame.Rotation, shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
				}
				(shapeImpl as GroupShapeImpl).SetUpdatedChildOffset();
			}
			else if (shapeImpl.GroupFrame != null)
			{
				shapeImpl.SetPostion(shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
				shapeImpl.ShapeFrame.SetAnchor(shapeImpl.GroupFrame.Rotation, shapeImpl.GroupFrame.OffsetX, shapeImpl.GroupFrame.OffsetY, shapeImpl.GroupFrame.OffsetCX, shapeImpl.GroupFrame.OffsetCY);
				if (shapeImpl is AutoShapeImpl)
				{
					(shapeImpl as AutoShapeImpl).ShapeExt.Coordinates = new Rectangle((int)shapeImpl.GroupFrame.OffsetX, (int)shapeImpl.GroupFrame.OffsetY, (int)shapeImpl.GroupFrame.OffsetCX, (int)shapeImpl.GroupFrame.OffsetCY);
				}
			}
		}
	}

	internal bool RemoveGroupShapeItem(IShape shape)
	{
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i].ShapeType == OfficeShapeType.Group && ((Items[i] as IGroupShape) as GroupShapeImpl).RemoveGroupShapeItem(shape))
			{
				return true;
			}
			if (Items[i] != shape)
			{
				continue;
			}
			for (int j = i; j < Items.Length - 1; j++)
			{
				Items[j] = Items[j + 1];
			}
			IShape[] array = Items;
			Array.Resize(ref array, array.Length - 1);
			if (FindParent(typeof(ShapesCollection)) is ShapesCollection shapesCollection)
			{
				shapesCollection.Group(this, array, isRemove: false);
				if (array.Length <= 1)
				{
					shapesCollection.Ungroup(this);
				}
			}
			return true;
		}
		return false;
	}
}
