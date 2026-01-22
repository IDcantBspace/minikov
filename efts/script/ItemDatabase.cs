using Godot;
using System;
using System.Collections.Generic;

public partial class ItemDatabase : Node{
	public static ItemDatabase Instance { get; private set; }
	private Dictionary<string, ItemData> _itemDictionary = new();

	public override void _Ready()
	{
		Instance = this;
		LoadAllItems();
	}

	private void LoadAllItems()
	{
		using var dir = DirAccess.Open("res://tres/ItemData/"); // 请确认这是正确路径
		if (dir == null)
		{
			GD.PrintErr("ItemDatabase: 无法打开资源目录！请检查路径。");
			return;
		}

		var error = dir.ListDirBegin();
		if (error != Error.Ok)
		{
			GD.PrintErr("ItemDatabase: 遍历目录失败。");
			return;
		}

		string fileName = dir.GetNext();
		int loadedCount = 0;
		while (fileName != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
			{
				string fullPath = "res://tres/ItemData/" + fileName; // 确保与上面目录一致
				var itemRes = GD.Load<ItemData>(fullPath);
				if (itemRes == null)
				{
					GD.PrintErr($"ItemDatabase: 加载资源失败：{fullPath}");
				}
				else if (string.IsNullOrEmpty(itemRes.ItemId))
				{
					GD.PrintErr($"ItemDatabase: 资源 ItemId 为空：{fullPath}");
				}
				else
				{
					_itemDictionary[itemRes.ItemId] = itemRes;
					loadedCount++;
				}
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
		GD.Print($"ItemDatabase: 加载完成，共 {loadedCount} 个物品。");
	}

	public ItemData GetItem(string itemId){
		// 常数时间查找[citation:10]
		if (_itemDictionary.TryGetValue(itemId, out ItemData item))
		{
			return item;
		}
		return null;
	}
}
