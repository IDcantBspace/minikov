using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Inventory : Panel{
	
	private Player player;
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
	[Export]
	public Control abandonSlot { get; set; }
	[Export]
	public Control rifleSlot1 { get; set; }
	public Weapon rifle1;
	[Export]
	public Control rifleSlot2 { get; set; }
	public Weapon rifle2;
	[Export]
	public Control pistolSlot { get; set; }
	public Weapon pistol;
	
	public override void _Ready(){
		player = GetNode<Player>("/root/world/Player");
		abandonSlot.AddToGroup("AbandonSlot");
		rifleSlot1.AddToGroup("RifleSlot");
		rifleSlot2.AddToGroup("RifleSlot");
		pistolSlot.AddToGroup("PistolSlot");
		//临时设置枪
		rifle1 = GetNode<Weapon>("/root/world/UILayer/Inventory/RifleSlot1/AK74");
		invItemsList = Enumerable.Repeat("000000", listLength).ToArray();
		boxItemsList = Enumerable.Repeat("000000", listLength).ToArray();
		FindItemsInSlots();
		TestReady();
		invSlotList = new AspectRatioContainer[6];
		GetInvSlot(6);
		boxSlotList = new AspectRatioContainer[6];
		UpdateGunDate();
	}
	
	private void FindItemsInSlots()
	{
		rifle1 = FindTextureRectInSlot(rifleSlot1);
		rifle2 = FindTextureRectInSlot(rifleSlot2);
		pistol = FindTextureRectInSlot(pistolSlot);
	}
	
	private Weapon FindTextureRectInSlot(Control slot)
	{
		// 获取容器节点。这里使用GetNode，假设这些节点与当前脚本挂载的节点是同级或子级。
		// 你需要根据实际的节点路径进行调整，例如可能为 $"../{slotNodeName}" 或 $"{slotNodeName}"
		if (slot == null){
			GD.PrintErr($"未找到容器节点！");
			return null;
		}
		// 遍历容器的所有直接子节点
		foreach (Node child in slot.GetChildren()){
			// 检查子节点是否为TextureRect类型
			if (child is Weapon textureRect){
				// 找到TextureRect，直接返回
				return textureRect;
			}
		}
		// 循环结束，未找到任何TextureRect子节点
		return null;
	}
	
	public void TestReady(){
		invItemsList[0] = "000001";
		invItemsList[1] = "000002";
		invItemsList[2] = "000003";
		invItemsList[3] = "000004";
		invItemsList[4] = "000005";
		invItemsList[5] = "000006";
	}
	
	public void UpdateGunDate(){
		if(rifle1 != null){
			player.UpdateGunDate(
				0, "Semi", rifle1.firingRate, rifle1.fireModeManual, rifle1.fireModeSemi,
				rifle1.fireModeBurst, rifle1.fireModeAuto, rifle1.magazineSize,
				rifle1.reloadTime, rifle1.tacReloadTime, rifle1.gunshotSound, rifle1.reloadSound
				);
		}
		if(rifle2 != null){
			player.UpdateGunDate(
				1, "Semi", rifle2.firingRate, rifle2.fireModeManual, rifle2.fireModeSemi,
				rifle2.fireModeBurst, rifle2.fireModeAuto, rifle2.magazineSize,
				rifle2.reloadTime, rifle2.tacReloadTime, rifle2.gunshotSound, rifle2.reloadSound
				);
		}
		if(pistol != null){
			player.UpdateGunDate(
				1, "Semi", pistol.firingRate, pistol.fireModeManual, pistol.fireModeSemi,
				pistol.fireModeBurst, pistol.fireModeAuto, pistol.magazineSize,
				pistol.reloadTime, pistol.tacReloadTime, pistol.gunshotSound, pistol.reloadSound
				);
		}

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
		else if(swapType == 4){
			for(int i=0; i<invSlotList.Length; i++){
				if(invSlotList[i] == originSlot){
					originNum = i;
				}
			}
			invItemsList[originNum] = "000000";
		}
		else if(swapType == 5){
			for(int i=0; i<boxItemsList.Length; i++){
				if(boxSlotList[i] == originSlot){
					originNum = i;
				}
			}
			boxItemsList[originNum] = "000000";
		}
		else if(swapType == 6){
			GD.Print("delete gun");
		}
	}
}
