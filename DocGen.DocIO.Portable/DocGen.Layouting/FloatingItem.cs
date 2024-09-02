using System;
using System.Collections.Generic;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class FloatingItem
{
	private RectangleF m_textWrappingBounds;

	private Entity m_FloatingEntity;

	private List<Entity> m_frameEntities;

	private int m_wrapcollectionindex = -1;

	private byte m_bFlags;

	internal RectangleF TextWrappingBounds
	{
		get
		{
			return m_textWrappingBounds;
		}
		set
		{
			m_textWrappingBounds = value;
		}
	}

	internal Entity FloatingEntity
	{
		get
		{
			return m_FloatingEntity;
		}
		set
		{
			m_FloatingEntity = value;
		}
	}

	internal List<Entity> FrameEntities
	{
		get
		{
			return m_frameEntities;
		}
		set
		{
			m_frameEntities = value;
		}
	}

	internal TextWrappingStyle TextWrappingStyle
	{
		get
		{
			if (m_FloatingEntity is WTable)
			{
				WTable wTable = m_FloatingEntity as WTable;
				if (wTable.IsFrame && (wTable.Rows.Count <= 0 || wTable.Rows[0].Cells.Count <= 0 || wTable.Rows[0].Cells[0].Paragraphs.Count <= 0 || wTable.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.WrapFrameAround == FrameWrapMode.NotBeside || wTable.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.WrapFrameAround == FrameWrapMode.None))
				{
					return TextWrappingStyle.TopAndBottom;
				}
			}
			else if (m_FloatingEntity is WParagraph)
			{
				WParagraph wParagraph = m_FloatingEntity as WParagraph;
				if (wParagraph.ParagraphFormat.WrapFrameAround == FrameWrapMode.None || wParagraph.ParagraphFormat.WrapFrameAround == FrameWrapMode.NotBeside)
				{
					return TextWrappingStyle.TopAndBottom;
				}
			}
			else
			{
				if (m_FloatingEntity is WPicture)
				{
					return (m_FloatingEntity as WPicture).TextWrappingStyle;
				}
				if (m_FloatingEntity is WTextBox)
				{
					return (m_FloatingEntity as WTextBox).TextBoxFormat.TextWrappingStyle;
				}
				if (m_FloatingEntity is Shape)
				{
					return (m_FloatingEntity as Shape).WrapFormat.TextWrappingStyle;
				}
				if (m_FloatingEntity is WChart)
				{
					return (m_FloatingEntity as WChart).WrapFormat.TextWrappingStyle;
				}
				if (m_FloatingEntity is GroupShape)
				{
					return (m_FloatingEntity as GroupShape).WrapFormat.TextWrappingStyle;
				}
			}
			return TextWrappingStyle.Square;
		}
	}

	internal TextWrappingType TextWrappingType
	{
		get
		{
			if (m_FloatingEntity is WPicture)
			{
				return (m_FloatingEntity as WPicture).TextWrappingType;
			}
			if (m_FloatingEntity is Shape)
			{
				return (m_FloatingEntity as Shape).WrapFormat.TextWrappingType;
			}
			if (m_FloatingEntity is WChart)
			{
				return (m_FloatingEntity as WChart).WrapFormat.TextWrappingType;
			}
			if (m_FloatingEntity is WTextBox)
			{
				return (m_FloatingEntity as WTextBox).TextBoxFormat.TextWrappingType;
			}
			if (m_FloatingEntity is GroupShape)
			{
				return (m_FloatingEntity as GroupShape).WrapFormat.TextWrappingType;
			}
			return TextWrappingType.Both;
		}
	}

	internal WParagraph OwnerParagraph
	{
		get
		{
			if (m_FloatingEntity is ParagraphItem)
			{
				return (m_FloatingEntity as ParagraphItem).GetOwnerParagraphValue();
			}
			return null;
		}
	}

	internal bool AllowOverlap
	{
		get
		{
			if (m_FloatingEntity is WTable)
			{
				WTable wTable = m_FloatingEntity as WTable;
				if (wTable.TableFormat != null && wTable.TableFormat.WrapTextAround)
				{
					return wTable.TableFormat.Positioning.AllowOverlap;
				}
				return true;
			}
			if (m_FloatingEntity is WTextBox && (m_FloatingEntity as WTextBox).TextBoxFormat.TextWrappingStyle != 0)
			{
				return (m_FloatingEntity as WTextBox).TextBoxFormat.AllowOverlap;
			}
			if (m_FloatingEntity is WPicture && (m_FloatingEntity as WPicture).TextWrappingStyle != 0)
			{
				return (m_FloatingEntity as WPicture).AllowOverlap;
			}
			if (m_FloatingEntity is Shape && (m_FloatingEntity as Shape).WrapFormat.TextWrappingStyle != 0)
			{
				return (m_FloatingEntity as Shape).WrapFormat.AllowOverlap;
			}
			if (m_FloatingEntity is WChart && (m_FloatingEntity as WChart).WrapFormat.TextWrappingStyle != 0)
			{
				return (m_FloatingEntity as WChart).WrapFormat.AllowOverlap;
			}
			if (m_FloatingEntity is GroupShape && (m_FloatingEntity as GroupShape).WrapFormat.TextWrappingStyle != 0)
			{
				return (m_FloatingEntity as GroupShape).WrapFormat.AllowOverlap;
			}
			if (m_FloatingEntity is WTextBox && (m_FloatingEntity as WTextBox).TextBoxFormat.TextWrappingStyle != 0)
			{
				return (m_FloatingEntity as WTextBox).TextBoxFormat.AllowOverlap;
			}
			return false;
		}
	}

	internal bool LayoutInCell
	{
		get
		{
			if (m_FloatingEntity is WPicture)
			{
				return (m_FloatingEntity as WPicture).LayoutInCell;
			}
			if (m_FloatingEntity is Shape)
			{
				return (m_FloatingEntity as Shape).LayoutInCell;
			}
			if (m_FloatingEntity is GroupShape)
			{
				return (m_FloatingEntity as GroupShape).LayoutInCell;
			}
			if (m_FloatingEntity is WTextBox)
			{
				return (m_FloatingEntity as WTextBox).Shape?.LayoutInCell ?? (m_FloatingEntity as WTextBox).TextBoxFormat.AllowInCell;
			}
			if (m_FloatingEntity is WChart)
			{
				return (m_FloatingEntity as WChart).LayoutInCell;
			}
			return false;
		}
	}

	internal int WrapCollectionIndex
	{
		get
		{
			return m_wrapcollectionindex;
		}
		set
		{
			m_wrapcollectionindex = value;
		}
	}

	internal bool IsFloatingItemFit
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsDoesNotDenotesRectangle
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal static void SortFloatingItems(List<FloatingItem> floatingItems, SortPosition sortPosition, bool isNeedToUpdateWrapCollectionIndex)
	{
		for (int i = 0; i < floatingItems.Count; i++)
		{
			FloatingItem floatingItem = floatingItems[i];
			int num = int.MinValue;
			for (int j = i; j < floatingItems.Count; j++)
			{
				if (IsNeedTobeChangeSortedItem(sortPosition, floatingItem.TextWrappingBounds, floatingItems[j].TextWrappingBounds))
				{
					num = j;
					floatingItem = floatingItems[j];
				}
			}
			if (num != int.MinValue)
			{
				FloatingItem item = floatingItems[num];
				floatingItems.RemoveAt(num);
				floatingItems.Insert(i, item);
				if (isNeedToUpdateWrapCollectionIndex)
				{
					UpdateWrapCollectionIndex(floatingItems, num, i);
				}
				num = int.MinValue;
			}
		}
	}

	internal static bool IsYPositionIntersect(RectangleF floatingItemBounds, RectangleF currentItemBounds)
	{
		if ((Math.Round(currentItemBounds.Y, 2) >= Math.Round(floatingItemBounds.Y, 2) && Math.Round(currentItemBounds.Y, 2) <= Math.Round(floatingItemBounds.Bottom, 2)) || (Math.Round(currentItemBounds.Bottom, 2) >= Math.Round(floatingItemBounds.Y, 2) && Math.Round(currentItemBounds.Bottom, 2) <= Math.Round(floatingItemBounds.Bottom, 2)) || (Math.Round(currentItemBounds.Y, 2) <= Math.Round(floatingItemBounds.Bottom, 2) && Math.Round(currentItemBounds.Y, 2) >= Math.Round(floatingItemBounds.Y, 2)))
		{
			return true;
		}
		return false;
	}

	internal static void SortSameYPostionFloatingItems(List<FloatingItem> floatingItems, SortPosition sortPosition)
	{
		for (int i = 0; i < floatingItems.Count; i++)
		{
			FloatingItem floatingItem = floatingItems[i];
			int num = int.MinValue;
			for (int j = i; j < floatingItems.Count; j++)
			{
				if (((floatingItem.TextWrappingStyle != TextWrappingStyle.Tight && floatingItem.TextWrappingStyle != TextWrappingStyle.Through) || !floatingItem.IsDoesNotDenotesRectangle) && Math.Round(floatingItem.TextWrappingBounds.Y) == Math.Round(floatingItems[j].TextWrappingBounds.Y) && IsNeedTobeChangeSortedItem(sortPosition, floatingItem.TextWrappingBounds, floatingItems[j].TextWrappingBounds))
				{
					num = j;
					floatingItem = floatingItems[j];
				}
			}
			if (num != int.MinValue)
			{
				FloatingItem item = floatingItems[num];
				floatingItems.RemoveAt(num);
				floatingItems.Insert(i, item);
				UpdateWrapCollectionIndex(floatingItems, num, i);
				num = int.MinValue;
			}
		}
	}

	internal static void SortIntersectedYPostionFloatingItems(List<FloatingItem> floatingItems, SortPosition sortPosition)
	{
		for (int i = 0; i < floatingItems.Count; i++)
		{
			FloatingItem floatingItem = floatingItems[i];
			int num = int.MinValue;
			for (int j = i; j < floatingItems.Count; j++)
			{
				if (((floatingItem.TextWrappingStyle != TextWrappingStyle.Tight && floatingItem.TextWrappingStyle != TextWrappingStyle.Through) || !floatingItem.IsDoesNotDenotesRectangle) && IsYPositionIntersect(floatingItem.TextWrappingBounds, floatingItems[j].TextWrappingBounds) && IsNeedTobeChangeSortedItem(sortPosition, floatingItem.TextWrappingBounds, floatingItems[j].TextWrappingBounds))
				{
					num = j;
					floatingItem = floatingItems[j];
				}
			}
			if (num != int.MinValue)
			{
				FloatingItem item = floatingItems[num];
				floatingItems.RemoveAt(num);
				floatingItems.Insert(i, item);
				UpdateWrapCollectionIndex(floatingItems, num, i);
				num = int.MinValue;
			}
		}
	}

	internal static void UpdateWrapCollectionIndex(List<FloatingItem> floatingItems, int sortItemIndex, int indexToBeInserted)
	{
		for (int i = indexToBeInserted; i <= sortItemIndex; i++)
		{
			switch (floatingItems[i].FloatingEntity.EntityType)
			{
			case EntityType.TextBox:
				(floatingItems[i].FloatingEntity as WTextBox).TextBoxFormat.WrapCollectionIndex = (short)i;
				break;
			case EntityType.Shape:
				(floatingItems[i].FloatingEntity as Shape).WrapFormat.WrapCollectionIndex = (short)i;
				break;
			case EntityType.GroupShape:
				(floatingItems[i].FloatingEntity as GroupShape).WrapFormat.WrapCollectionIndex = (short)i;
				break;
			case EntityType.Chart:
				(floatingItems[i].FloatingEntity as WChart).WrapFormat.WrapCollectionIndex = (short)i;
				break;
			case EntityType.Picture:
				(floatingItems[i].FloatingEntity as WPicture).WrapCollectionIndex = (short)i;
				break;
			}
			floatingItems[i].WrapCollectionIndex = (short)i;
		}
	}

	internal static void SortXYPostionFloatingItems(List<FloatingItem> floatingItems, RectangleF rect, SizeF size)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < floatingItems.Count; i++)
		{
			if (IsYPositionIntersect(floatingItems[i].TextWrappingBounds, rect, size.Height))
			{
				list.Add(i);
			}
		}
		for (int j = 0; j < list.Count - 1; j++)
		{
			FloatingItem floatingItem = floatingItems[list[j]];
			FloatingItem floatingItem2 = floatingItems[list[j] + 1];
			if (floatingItem.TextWrappingBounds.X > floatingItem2.TextWrappingBounds.X && floatingItem.TextWrappingBounds.Y < floatingItem2.TextWrappingBounds.Y)
			{
				FloatingItem value = floatingItem;
				floatingItems[list[j]] = floatingItem2;
				floatingItems[list[j] + 1] = value;
			}
		}
	}

	internal static bool IsYPositionIntersect(RectangleF floatingItemBounds, RectangleF currentItemBounds, float height)
	{
		if ((Math.Round(currentItemBounds.Y, 2) > Math.Round(floatingItemBounds.Y, 2) && Math.Round(currentItemBounds.Y, 2) < Math.Round(floatingItemBounds.Bottom, 2)) || (Math.Round(currentItemBounds.Y + height, 2) > Math.Round(floatingItemBounds.Y, 2) && Math.Round(currentItemBounds.Y + height, 2) < Math.Round(floatingItemBounds.Bottom, 2)) || (Math.Round(currentItemBounds.Y, 2) < Math.Round(floatingItemBounds.Bottom, 2) && Math.Round(currentItemBounds.Y, 2) > Math.Round(floatingItemBounds.Y, 2)))
		{
			return true;
		}
		return false;
	}

	private static bool IsNeedTobeChangeSortedItem(SortPosition SortPosition, RectangleF firstItem, RectangleF secondItem)
	{
		return SortPosition switch
		{
			SortPosition.X => firstItem.X > secondItem.X, 
			SortPosition.Y => firstItem.Y > secondItem.Y, 
			SortPosition.Bottom => firstItem.Bottom > secondItem.Bottom, 
			_ => false, 
		};
	}

	internal static RectangleF GetIntersectingItemBounds(Layouter m_lcOperator, FloatingItem intersectedFloatingItem, float yPosition)
	{
		return GetMinBottomFloatingItem(GetIntersectingFloatingItems(m_lcOperator, intersectedFloatingItem, yPosition))?.TextWrappingBounds ?? RectangleF.Empty;
	}

	internal static List<FloatingItem> GetIntersectingFloatingItems(Layouter m_lcOperator, FloatingItem intersectedFloatingItem, float yPosition)
	{
		List<FloatingItem> list = new List<FloatingItem>();
		foreach (FloatingItem floatingItem in m_lcOperator.FloatingItems)
		{
			if (yPosition <= floatingItem.TextWrappingBounds.Bottom && intersectedFloatingItem.TextWrappingBounds.Bottom >= floatingItem.TextWrappingBounds.Bottom && floatingItem.TextWrappingBounds.Right > m_lcOperator.ClientLayoutArea.X && floatingItem.TextWrappingBounds.X < intersectedFloatingItem.TextWrappingBounds.X)
			{
				list.Add(floatingItem);
			}
		}
		return list;
	}

	internal static FloatingItem GetMinBottomFloatingItem(List<FloatingItem> fItems)
	{
		int num = -1;
		float num2 = float.MaxValue;
		int num3 = 0;
		SortFloatingItems(fItems, SortPosition.X, isNeedToUpdateWrapCollectionIndex: false);
		foreach (FloatingItem fItem in fItems)
		{
			if (num2 > fItem.TextWrappingBounds.Bottom)
			{
				if (fItem.FloatingEntity is WParagraph && fItems.IndexOf(fItem) + 1 < fItems.Count && fItems[fItems.IndexOf(fItem) + 1].FloatingEntity is WParagraph && (fItem.FloatingEntity as WParagraph).ParagraphFormat.IsInSameFrame((fItems[fItems.IndexOf(fItem) + 1].FloatingEntity as WParagraph).ParagraphFormat))
				{
					num3++;
					continue;
				}
				num2 = fItem.TextWrappingBounds.Bottom;
				num = fItems.IndexOf(fItem);
			}
		}
		if (num - num3 != 0)
		{
			return null;
		}
		return fItems[num];
	}
}
