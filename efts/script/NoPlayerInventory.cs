using Godot;
using System;
using System.Linq;

public partial class NoPlayerInventory : Panel{
	[Export]
	public VBoxContainer vBoxContainer{ get; set; }
	[Export]
	public PackedScene itemPanel{ get; set; }
	
	
	public void AddItem(String itemID){
		ItemData newItem = ItemDatabase.Instance.GetItem(itemID);
		if (newItem != null){
			ItemPanel newItemPanel = itemPanel.Instantiate<ItemPanel>();
			vBoxContainer.AddChild(newItemPanel); // 将新实例添加到滚动条中显示
			newItemPanel.ChangeTexture(newItem.itemTexture);
		}
	}
	
	public void Close(){
		vBoxContainer.GetChildren().ToList().ForEach(child => child.QueueFree());
	}
}
