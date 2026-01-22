using Godot;
using System;
using System.Linq;

public partial class BoxList : Panel{
	[Export]
	public VBoxContainer vBoxContainer{ get; set; }
	[Export]
	public PackedScene itemPanel{ get; set; }
	public Box openingBox;
	
	public void AddItem(String itemID){
		openingBox.AddItemInBox(itemID);
		GD.Print("BoxList");
		this.AddItemOnUI(itemID);
	}
	
	public void AddItemOnUI(String itemID){
		ItemData newItem = ItemDatabase.Instance.GetItem(itemID);
		if (newItem != null){
			ItemPanel newItemPanel = itemPanel.Instantiate<ItemPanel>();
			vBoxContainer.AddChild(newItemPanel);// 将新实例添加到滚动条中显示
			newItemPanel.ChangeTexture(itemID, newItem.itemTexture);
		}
	}
	
	public void DeleteItem(int slotID){
		openingBox.DeleteItemInBox(slotID);
		GD.Print("删除的位置是"+slotID);
		Node targetItem = vBoxContainer.GetChild(slotID);
		// 从父节点移除
		vBoxContainer.RemoveChild(targetItem);
		// 安全删除节点
		targetItem.QueueFree();
	}
	
	public void DeleteItemOnUI(int slotID){
		GD.Print("删除的位置是"+slotID);
		Node targetItem = vBoxContainer.GetChild(slotID);
		// 从父节点移除
		vBoxContainer.RemoveChild(targetItem);
		// 安全删除节点
		targetItem.QueueFree();
	}
	
	public void ChangeItem(int slotID, String itemID){
		DeleteItem(slotID);
		AddItem(itemID);
	}
	
	public String GetItem(int slotID){
		return openingBox.GetItemInBox(slotID);
	}
	
	public void Close(){
		if(this.Visible == false){
			return;
		}
		this.Visible = false;
		vBoxContainer.GetChildren().ToList().ForEach(child => child.QueueFree());
		openingBox = null;
	}
	
	public void Initialize(Box box){
		openingBox = box;
		this.Visible = true;
		int i = 0;
		while(box.GetItemInBox(i)!=null){
			this.AddItemOnUI(openingBox.GetItemInBox(i));
			i++;
		}
	}
	
}
