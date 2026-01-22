using Godot;
using System;
using System.Collections.Generic;

public partial class WeaponDatabase : Node{
	public static WeaponDatabase Instance { get; private set; }
	private Dictionary<string, WeaponData> _weaponDictionary = new();

	public override void _Ready()
	{
		Instance = this;
		LoadAllWeapons();
	}

	private void LoadAllWeapons()
	{
		using var dir = DirAccess.Open("res://tres/WeaponData/"); // 请确认这是正确路径
		if (dir == null)
		{
			GD.PrintErr("WeaponDatabase: 无法打开资源目录！请检查路径。");
			return;
		}

		var error = dir.ListDirBegin();
		if (error != Error.Ok)
		{
			GD.PrintErr("WeaponDatabase: 遍历目录失败。");
			return;
		}

		string fileName = dir.GetNext();
		int loadedCount = 0;
		while (fileName != "")
		{
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
			{
				string fullPath = "res://tres/WeaponData/" + fileName; // 确保与上面目录一致
				var weaponRes = GD.Load<WeaponData>(fullPath);
				if (weaponRes == null)
				{
					GD.PrintErr($"WeaponDatabase: 加载资源失败：{fullPath}");
				}
				else if (string.IsNullOrEmpty(weaponRes.ItemId))
				{
					GD.PrintErr($"WeaponDatabase: 资源 ItemId 为空：{fullPath}");
				}
				else
				{
					_weaponDictionary[weaponRes.ItemId] = weaponRes;
					loadedCount++;
				}
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
		GD.Print($"WeaponDatabase: 加载完成，共 {loadedCount} 个物品。");
	}

	public WeaponData GetWeapon(string itemId){
		// 常数时间查找[citation:10]
		if (_weaponDictionary.TryGetValue(itemId, out WeaponData weapon))
		{
			return weapon;
		}
		return null;
	}
}
