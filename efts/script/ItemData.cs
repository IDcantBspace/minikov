using Godot;
using System;

[GlobalClass] // 让该类出现在编辑器创建资源菜单中
public partial class ItemData : Resource{
	[Export] public string ItemId { get; set; } // ID
	[Export] public string ItemType { get; set; } // 类型
	[Export] public Texture2D itemTexture { get; set; } //标准材质
	[Export] public Texture2D equipmentTexture { get; set; } //标准材质
}
