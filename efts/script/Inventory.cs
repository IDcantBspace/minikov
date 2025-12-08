using Godot;
using System;

public partial class Inventory : Panel{
	private Box itemBox;
	private AspectRatioContainer[] slotList;
	[Export]
	public GridContainer grid { get; set; }
	[Export]
	public Panel itemListPanel { get; set; }
	
	public void Initialize(){
		this.Visible = true;
	}
	
	public void Initialize(Node list){
		this.Visible = true;
		itemListPanel.Visible = true;
		itemBox = list as Box;
		slotList = new AspectRatioContainer[itemBox.listLength];
		GetSlot(itemBox.listLength);
		for(int i=0;i<itemBox.listLength;i++){
			GD.Print("循环i="+i);
			GD.Print(itemBox.itemsList[i]);
			if(itemBox.itemsList[i] != "000000"){
				ItemData newItem = ItemDatabase.Instance.GetItem(itemBox.itemsList[i]);
				if (newItem != null){
					Node itemInstance = newItem.itemTscn.Instantiate();
					slotList[i].AddChild(itemInstance); // 将新实例添加到场景树中显示
				}
			}
		}
	}
	
	public void Close(){
		itemListPanel.Visible = false;
		this.Visible = false;
	}
	
	public void GetSlot(int length){
		int j = 0;
		//GD.Print("getslot循环开始");
		foreach (AspectRatioContainer slot in grid.GetChildren()){
			//GD.Print("getslot循环j="+j);
			slotList[j] = slot;
			j++;
			if(j>=length) return;
		}
	}
}
