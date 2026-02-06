using Godot;
using System;
using System.Collections.Generic;

public partial class GearDatabase : Node{
	public static GearDatabase Instance { get; private set; }
	private Dictionary<string, GearData> _gearDictionary = new();

	public override void _Ready()
	{
		Instance = this;
		LoadAllGears();
	}

	private void LoadAllGears()
	{
		using var dir = DirAccess.Open("res://tres/GearData/"); // 请确认这是正确路径
		if (dir == null)
		{
			GD.PrintErr("GearDatabase: 无法打开资源目录！请检查路径。");
			return;
		}

		var error = dir.ListDirBegin();
		if (error != Error.Ok)
		{
			GD.PrintErr("GearDatabase: 遍历目录失败。");
			return;
		}

		string fileName = dir.GetNext();
		int loadedCount = 0;
		while (fileName != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
			{
				string fullPath = "res://tres/GearData/" + fileName; // 确保与上面目录一致
				var gearRes = GD.Load<GearData>(fullPath);
				if (gearRes == null)
				{
					GD.PrintErr($"GearDatabase: 加载资源失败：{fullPath}");
				}
				else if (string.IsNullOrEmpty(gearRes.ItemId))
				{
					GD.PrintErr($"GearDatabase: 资源 ItemId 为空：{fullPath}");
				}
				else
				{
					_gearDictionary[gearRes.ItemId] = gearRes;
					loadedCount++;
				}
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
		GD.Print($"GearDatabase: 加载完成，共 {loadedCount} 个物品。");
	}

	public GearData GetGear(string itemId){
		// 常数时间查找[citation:10]
		if (_gearDictionary.TryGetValue(itemId, out GearData gear))
		{
			return gear;
		}
		return null;
	}
}
