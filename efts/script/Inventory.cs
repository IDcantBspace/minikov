using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Inventory : Panel{
	
	private Player player;
	private Box itemBox;
	private AspectRatioContainer[] invSlotList;
	[Signal]
	public delegate void UpdateEventHandler(String[] newList);
	[Export]
	public PackedScene BoxListPanel { get; set; }
	[Export]
	public PackedScene genericItem { get; set; }
	[Export]
	public GridContainer invGrid { get; set; }
	public int listLength = 6;
	private String[] invItemsList;
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
	[Export]
	public Panel inventoryPanel{ get; set; }
	[Export]
	public Equipment equipment{ get; set; }

	public bool IsOpen = true;
	
	[Export]
	public Color CloseColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.5f);
	
	[Export]
	public Color OpenColor { get; set; } = new Color(0.0f, 0.0f, 0.0f, 0.392f);
	
	private StyleBoxFlat styleBox;
	
	public override void _Ready(){
		player = GetNode<Player>("/root/world/Player");
		abandonSlot.AddToGroup("AbandonSlot");
		rifleSlot1.AddToGroup("RifleSlot");
		rifleSlot2.AddToGroup("RifleSlot");
		pistolSlot.AddToGroup("PistolSlot");

		invItemsList = Enumerable.Repeat("000000", listLength).ToArray();
		FindItemsInSlots(); //获得枪械数据
		TestReady();  //临时设置背包物品ID列表
		invSlotList = new AspectRatioContainer[6];
		//GetInvSlot(6);  //获得容器格子数组
		UpdateGunDate();
		// 设置鼠标过滤器为Stop，这样节点才能接收鼠标事件
		MouseFilter = MouseFilterEnum.Stop;
		
		// 创建样式
		styleBox = new StyleBoxFlat();
		styleBox.BgColor = OpenColor;
		styleBox.CornerRadiusTopLeft = 5;
		styleBox.CornerRadiusTopRight = 5;
		styleBox.CornerRadiusBottomLeft = 5;
		styleBox.CornerRadiusBottomRight = 5;
		
		AddThemeStyleboxOverride("panel", styleBox);
	}
	
	public override void _GuiInput(InputEvent @event){
		// 处理鼠标点击
		if (@event is InputEventMouseButton mouseButton){
			if (mouseButton.ButtonIndex == MouseButton.Left){
				if (mouseButton.Pressed&&!IsOpen){
					OnSwapped();
				}
			}
		}
	}
	
	public void OnSwapped()
	{
		if(IsOpen){
			styleBox.BgColor = CloseColor;
			inventoryPanel.Visible = false;
			IsOpen = false;
		}
		else{
			equipment.OnSwapped();
			styleBox.BgColor = OpenColor;
			inventoryPanel.Visible = true;
			IsOpen = true;
		}
		QueueRedraw();
		GD.Print("可点击区域被点击了！");
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
		invItemsList[0] = "000000";
		invItemsList[1] = "000000";
		invItemsList[2] = "000000";
		invItemsList[3] = "000000";
		invItemsList[4] = "000000";
		invItemsList[5] = "000000";
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
	
	public void Initialize(){
		
		for(int temNum = 0;temNum<listLength;temNum++){
			// 创建AspectRatioContainer实例
			AspectRatioContainer slot = new AspectRatioContainer();
			// 设置最小尺寸为64x64（关键步骤）
			slot.CustomMinimumSize = new Vector2(64, 64);
			// 给容器命名（可选）
			slot.Name = $"Slot{temNum + 1}";
			// 添加到GridContainer
			invGrid.AddChild(slot);
			invSlotList[temNum] = slot;
			slot.AddToGroup("InvSlot");
			if (invItemsList[temNum] != "000000"){
				AddItemInInventory(invItemsList[temNum], slot);
			}
		}
		this.Visible = true;
	}
	
	public void AddItemInInventory(String itemID, AspectRatioContainer targetSlot){
		ItemData newItemData = ItemDatabase.Instance.GetItem(itemID);
		if (newItemData != null){
			Items newItem = genericItem.Instantiate<Items>();
			targetSlot.AddChild(newItem);// 将新实例添加到滚动条中显示
			newItem.Name = itemID;
			newItem.Texture = newItemData.itemTexture;
		}
	}
	
	public String GetItem(int slotID){
		return invItemsList[slotID];
	}
	
	public int GetSlotID(AspectRatioContainer originSlot){
		int oSlotID = 0;
		for(int i=0; i<invSlotList.Length; i++){
			if(invSlotList[i] == originSlot){
				oSlotID = i;
			}
		}
		return oSlotID;
	}
	
	public void Close(){
		Array.Clear(invSlotList, 0, listLength);
		invGrid.GetChildren().ToList().ForEach(child => child.QueueFree());
		this.Visible = false;
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
		
	public void ChangeItem(int tSlotID, String oItemID){
		invItemsList[tSlotID] = oItemID;
	}
		
		
	public void OnSwapped(int swapType, int oSlotID, int tSlotID){
		GD.Print("swap信号收到了");
		if(swapType == 0){
			GD.Print("背包交换oSlotID="+oSlotID+"  tSlotID="+tSlotID);
			String temStr = invItemsList[oSlotID];
			invItemsList[oSlotID] = invItemsList[tSlotID];
			invItemsList[tSlotID] = temStr;
		}
		else if(swapType == 1){
			//未实现的箱子交换
		}
		else if(swapType == 2){
			invItemsList[oSlotID] = "000000";
		}
		else if(swapType == 3){
			
			//未实现的箱子换进背包
		}
		else if(swapType == 4){
			//未实现的箱子销毁
		}
		else if(swapType == 6){
			GD.Print("delete gun");
		}
	}
	
	//boxListPanel = BoxListPanel.Instantiate<PanelContainer>();
		//itemListPanel.AddChild(boxListPanel);
		//boxListPanel.Position = new Vector2(100, 100);
		//itemListPanel.Visible = true;
		//boxGrid = GetNode<GridContainer>("ListInventory/BoxListPanel/GridContainer");
		//if(boxGrid != null){
			//GetBoxSlot(6);
		//}
		//itemBox = list as Box;
		//Update += itemBox.OnUpdate;
		//for(int i=0;i<itemBox.listLength;i++){
			//GD.Print("循环i="+i);
			//GD.Print(itemBox.itemsList[i]);
			//boxItemsList[i] = itemBox.itemsList[i];
			//if(itemBox.itemsList[i] != "000000"){
				//ItemData newItem = ItemDatabase.Instance.GetItem(itemBox.itemsList[i]);
				//if (newItem != null){
					//Node itemInstance = newItem.itemTscn.Instantiate();
					//boxSlotList[i].AddChild(itemInstance); // 将新实例添加到场景树中显示
				//}
			//}
		//}
}
