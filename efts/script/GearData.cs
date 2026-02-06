using Godot;
using System;

[GlobalClass] // 让该类出现在编辑器创建资源菜单中
public partial class GearData : Resource{
	
	//装备相关属性
	[Export] public string ItemId { get; set; } // ID
	[Export] public int slotColumnNum { get; set; } // 格列数X
	[Export] public int slotRowNum { get; set; } // 格行数Y

}
