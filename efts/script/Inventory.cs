using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Inventory : Panel{
	
	private Player player;
	private AspectRatioContainer[] invSlotList;
	[Export]
	public PackedScene genericItem { get; set; }
	[Export]
	public GridContainer invGrid { get; set; }
	public int listLength = 6;
	private String[] invItemsList;
	[Export]
	public Control abandonSlot { get; set; }
	[Export]
	public Control rifleSlot1 { get; set; }
	public String rifle1;
	[Export]
	public Control rifleSlot2 { get; set; }
	public String rifle2;
	[Export]
	public Control pistolSlot { get; set; }
	public String pistol;
	private int equipmentNeedToUpdate = 0;
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
		TestReady();  //临时设置背包物品ID列表
		invSlotList = new AspectRatioContainer[6];
		//UpdateGunDate();
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
	
	private TextureRect FindTextureRectInSlot(Control slot)
	{
		// 获取容器节点。这里使用GetNode，假设这些节点与当前脚本挂载的节点是同级或子级。
		if (slot == null){
			GD.PrintErr($"未找到容器节点！");
			return null;
		}
		// 遍历容器的所有直接子节点
		foreach (Node child in slot.GetChildren()){
			// 检查子节点是否为TextureRect类型
			if (child is TextureRect textureRect){
				// 找到TextureRect，直接返回
				return textureRect;
			}
		}
		// 循环结束，未找到任何TextureRect子节点
		return null;
	}
	
	public void TestReady(){
		rifle1 = "000000";
		rifle2 = "000000";
		pistol = "000000";
		invItemsList[0] = "000000";
		invItemsList[1] = "000000";
		invItemsList[2] = "000000";
		invItemsList[3] = "000000";
		invItemsList[4] = "000000";
		invItemsList[5] = "000000";
	}
	
	public void UpdateGunDate(){
		if(equipmentNeedToUpdate == 1){
			if(rifle1 != "000000"){
			WeaponData newWeaponData = WeaponDatabase.Instance.GetWeapon(rifle1);
			player.UpdateGunDate(
				0, "Semi", newWeaponData.firingRate, newWeaponData.fireModeManual, newWeaponData.fireModeSemi,
				newWeaponData.fireModeBurst, newWeaponData.fireModeAuto, newWeaponData.magazineSize,
				newWeaponData.reloadTime, newWeaponData.tacReloadTime, newWeaponData.gunshotSound, newWeaponData.reloadSound
				);
			}
			else{
				player.UpdateGunDate(0);
			}
		}
		if(equipmentNeedToUpdate == 2){
			if(rifle2 != "000000"){
				WeaponData newWeaponData = WeaponDatabase.Instance.GetWeapon(rifle2);
				player.UpdateGunDate(
					1, "Semi", newWeaponData.firingRate, newWeaponData.fireModeManual, newWeaponData.fireModeSemi,
					newWeaponData.fireModeBurst, newWeaponData.fireModeAuto, newWeaponData.magazineSize,
					newWeaponData.reloadTime, newWeaponData.tacReloadTime, newWeaponData.gunshotSound, newWeaponData.reloadSound
					);
			}
			else{
				player.UpdateGunDate(1);
			}
		}
		if(equipmentNeedToUpdate == 3){
			GD.Print("update work");
			if(pistol != "000000"){
				WeaponData newWeaponData = WeaponDatabase.Instance.GetWeapon(pistol);
				player.UpdateGunDate(
					2, "Semi", newWeaponData.firingRate, newWeaponData.fireModeManual, newWeaponData.fireModeSemi,
					newWeaponData.fireModeBurst, newWeaponData.fireModeAuto, newWeaponData.magazineSize,
					newWeaponData.reloadTime, newWeaponData.tacReloadTime, newWeaponData.gunshotSound, newWeaponData.reloadSound
					);
			}
			else{
				player.UpdateGunDate(2);
				
			}
		}
		equipmentNeedToUpdate = 0;
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
			targetSlot.AddChild(newItem);// 新实例添加
			newItem.Name = itemID;
			newItem.Texture = newItemData.itemTexture;
		}
	}
	
	public void AddItemInInventoryWithID(String itemID, int slotID){
		ItemData newItemData = ItemDatabase.Instance.GetItem(itemID);
		if (newItemData != null){
			Items newItem = genericItem.Instantiate<Items>();
			invSlotList[slotID].AddChild(newItem);
			newItem.Name = itemID;
			newItem.Texture = newItemData.itemTexture;
		}
	}
	
	public void DeleteItem(int tSlotID){
		invItemsList[tSlotID] = "000000";
		if(invSlotList[tSlotID].GetChildCount() > 0){
			Node targetItem = invSlotList[tSlotID].GetChild(0);
			// 从父节点移除
			invSlotList[tSlotID].RemoveChild(targetItem);
			// 安全删除节点
			targetItem.QueueFree();
		}
	}
	
	public String GetItem(int slotID){
		return invItemsList[slotID];
	}

	public void ChangeItem(int tSlotID, String oItemID){
		if(invItemsList[tSlotID] != "000000"){
			DeleteItem(tSlotID);
		}
		invItemsList[tSlotID] = oItemID;
		AddItemInInventoryWithID(oItemID, tSlotID);
	}
	
	public String ChangeEquipment(AspectRatioContainer tSlot, String oItemID){
		String temWeaponID = "000000";
		if(tSlot == rifleSlot1){
			temWeaponID = rifle1;
			rifle1 = oItemID;
			ItemData newItemData = ItemDatabase.Instance.GetItem(oItemID);
			if (newItemData != null){
				Items newItem = genericItem.Instantiate<Items>();
				rifleSlot1.AddChild(newItem);
				newItem.Name = oItemID;
				newItem.Texture = newItemData.equipmentTexture;
			}
			equipmentNeedToUpdate = 1;
		}
		else if(tSlot == rifleSlot2){
			temWeaponID = rifle2;
			rifle2 = oItemID;
			ItemData newItemData = ItemDatabase.Instance.GetItem(oItemID);
			if (newItemData != null){
				Items newItem = genericItem.Instantiate<Items>();
				rifleSlot2.AddChild(newItem);
				newItem.Name = oItemID;
				newItem.Texture = newItemData.equipmentTexture;
			}
			equipmentNeedToUpdate = 2;
		}
		else if(tSlot == pistolSlot){
			temWeaponID = pistol;
			pistol = oItemID;
			ItemData newItemData = ItemDatabase.Instance.GetItem(oItemID);
			if (newItemData != null){
				Items newItem = genericItem.Instantiate<Items>();
				pistolSlot.AddChild(newItem);
				newItem.Name = oItemID;
				newItem.Texture = newItemData.equipmentTexture;
			}
			equipmentNeedToUpdate = 3;
		}
		UpdateGunDate();
		return temWeaponID;
	}
	
	public void DeleteEquipment(AspectRatioContainer tSlot){
		if(tSlot == rifleSlot1){
			rifle1 = "000000";
			equipmentNeedToUpdate = 1;
		}
		else if(tSlot == rifleSlot2){
			rifle2 = "000000";
			equipmentNeedToUpdate = 2;
		}
		else if(tSlot == pistolSlot){
			GD.Print("delete work");
			pistol = "000000";
			equipmentNeedToUpdate = 3;
		}
		UpdateGunDate();
	}
	
	public String GetEquipment(AspectRatioContainer tSlot){
		if(tSlot == rifleSlot1){
			return rifle1;
		}
		else if(tSlot == rifleSlot2){
			return rifle2;
		}
		else if(tSlot == pistolSlot){
			return pistol;
		}
		return "000000";
	}

	//wrong
	public void SwapItem(int oSlotID, int tSlotID){
		GD.Print("背包交换oSlotID="+oSlotID+"  tSlotID="+tSlotID);
		String temStr = invItemsList[oSlotID];
		invItemsList[oSlotID] = invItemsList[tSlotID];
		invItemsList[tSlotID] = temStr;
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
	
}
