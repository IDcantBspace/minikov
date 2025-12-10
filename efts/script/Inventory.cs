using Godot;
using System;
using System.Linq;

public partial class Inventory : Panel{
	private Box itemBox;
	private AspectRatioContainer[] invSlotList;
	private AspectRatioContainer[] boxSlotList;
	[Signal]
	public delegate void UpdateEventHandler(String[] newList);
	[Export]
	public PackedScene BoxListPanel { get; set; }
	[Export]
	public GridContainer invGrid { get; set; }
	public GridContainer boxGrid;
	[Export]
	public Panel itemListPanel { get; set; }
	public int listLength = 6;
	public String[] invItemsList;
	public String[] boxItemsList;
	public PanelContainer boxListPanel;
	
	public override void _Ready(){
		invItemsList = Enumerable.Repeat("000000", listLength).ToArray();
		boxItemsList = Enumerable.Repeat("000000", listLength).ToArray();
		TestReady();
		invSlotList = new AspectRatioContainer[6];
		GetInvSlot(6);
		boxSlotList = new AspectRatioContainer[6];
	}
	
	public void TestReady(){
		invItemsList[0] = "000001";
		invItemsList[1] = "000002";
		invItemsList[2] = "000003";
		invItemsList[3] = "000004";
		invItemsList[4] = "000005";
		invItemsList[5] = "000006";
	}
	
	//信号响应，调用不同方法
	public void OnOpenBox(bool isOpen, Node list){
		if(isOpen){
			Initialize(list);
		}
		else{
			Close();
		}
	}
	
	public void Initialize(Node list){
		this.Visible = true;
		if(list == null){
			return;
		}
		boxListPanel = BoxListPanel.Instantiate<PanelContainer>();
		itemListPanel.AddChild(boxListPanel);
		boxListPanel.Position = new Vector2(100, 100);
		itemListPanel.Visible = true;
		boxGrid = GetNode<GridContainer>("ListInventory/BoxListPanel/GridContainer");
		if(boxGrid != null){
			GetBoxSlot(6);
		}
		itemBox = list as Box;
		Update += itemBox.OnUpdate;
		for(int i=0;i<itemBox.listLength;i++){
			GD.Print("循环i="+i);
			GD.Print(itemBox.itemsList[i]);
			boxItemsList[i] = itemBox.itemsList[i];
			if(itemBox.itemsList[i] != "000000"){
				ItemData newItem = ItemDatabase.Instance.GetItem(itemBox.itemsList[i]);
				if (newItem != null){
					Node itemInstance = newItem.itemTscn.Instantiate();
					boxSlotList[i].AddChild(itemInstance); // 将新实例添加到场景树中显示
				}
			}
		}
	}
	
	public void Close(){
		EmitSignal(SignalName.Update, boxItemsList);
		//Update -= itemBox.OnUpdate;
		itemListPanel.Visible = false;
		this.Visible = false;
		if(boxListPanel != null){
			boxListPanel.QueueFree();
			boxListPanel = null;
		}
	}
	
	public void GetInvSlot(int length){
		int j = 0;
		//GD.Print("getslot循环开始");
		foreach (AspectRatioContainer slot in invGrid.GetChildren()){
			//GD.Print("getslot循环j="+j);
			invSlotList[j] = slot;
			slot.AddToGroup("InvSlot");
			j++;
			if(j>=length) return;
		}
	}
	
	public void GetBoxSlot(int length){
		int j = 0;
		//GD.Print("getslot循环开始");
		foreach (AspectRatioContainer slot in boxGrid.GetChildren()){
			//GD.Print("getslot循环j="+j);
			boxSlotList[j] = slot;
			slot.AddToGroup("BoxSlot");
			j++;
			if(j>=length) return;
		}
	}
	
	public void OnSwapped(int swapType, AspectRatioContainer originSlot, AspectRatioContainer targetSlot){
		GD.Print("swap信号收到了");
		int originNum = 0;
		int targetNum = 0;
		if(swapType == 0){
			for(int i=0; i<invSlotList.Length; i++){
				if(invSlotList[i] == originSlot){
					originNum = i;
				}
				if(invSlotList[i] == targetSlot){
					targetNum = i;
				}
			}
			GD.Print("背包交换originNum="+originNum+"  targetNum="+targetNum);
			String temStr = invItemsList[originNum];
			invItemsList[originNum] = invItemsList[targetNum];
			invItemsList[targetNum] = temStr;
		}
		else if(swapType == 1){
			for(int i=0; i<boxSlotList.Length; i++){
				if(boxSlotList[i] == originSlot){
					originNum = i;
				}
				if(boxSlotList[i] == targetSlot){
					targetNum = i;
				}
			}
			GD.Print("箱子交换originNum="+originNum+"  targetNum="+targetNum);
			String temStr = boxItemsList[originNum];
			boxItemsList[originNum] = boxItemsList[targetNum];
			boxItemsList[targetNum] = temStr;
		}
		else if(swapType == 2){
			for(int i=0; i<invSlotList.Length; i++){
				if(invSlotList[i] == originSlot){
					originNum = i;
				}
			}
			for(int i=0; i<boxSlotList.Length; i++){
				if(boxSlotList[i] == targetSlot){
					targetNum = i;
				}
			}
			GD.Print("箱子交换originNum="+originNum+"  targetNum="+targetNum);
			String temStr = invItemsList[originNum];
			invItemsList[originNum] = boxItemsList[targetNum];
			boxItemsList[targetNum] = temStr;
		}
		else if(swapType == 3){
			for(int i=0; i<boxSlotList.Length; i++){
				if(boxSlotList[i] == originSlot){
					originNum = i;
				}
			}
			for(int i=0; i<invSlotList.Length; i++){
				if(invSlotList[i] == targetSlot){
					targetNum = i;
				}
			}
			GD.Print("箱子交换originNum="+originNum+"  targetNum="+targetNum);
			String temStr = boxItemsList[originNum];
			boxItemsList[originNum] = invItemsList[targetNum];
			invItemsList[targetNum] = temStr;
		}
	}
}
