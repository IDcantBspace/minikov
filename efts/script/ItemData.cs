using Godot;
using System;

[GlobalClass] // 让该类出现在编辑器创建资源菜单中
public partial class ItemData : Resource{
	[Export] public string ItemId { get; set; } // ID
	[Export] public PackedScene itemTscn { get; set; } //预设场景
}
