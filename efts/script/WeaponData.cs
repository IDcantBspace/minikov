using Godot;
using System;

[GlobalClass] // 让该类出现在编辑器创建资源菜单中
public partial class WeaponData : Resource{
	
	//射击相关属性
	[Export] public string ItemId { get; set; } // ID
	[Export] public float firingRate { get; set; }
	[Export] public bool fireModeManual { get; set; }
	[Export] public bool fireModeSemi { get; set; }
	[Export] public bool fireModeBurst { get; set; }
	[Export] public bool fireModeAuto { get; set; }
	[Export] public int magazineSize { get; set; }
	[Export] public float reloadTime { get; set; }
	[Export] public float tacReloadTime { get; set; }
	[Export] public AudioStream gunshotSound { get; set; }
	[Export] public AudioStream reloadSound { get; set; }
}
