using Godot;
using System;

public partial class Items : TextureRect{
	private bool isDragging = false;
	private Vector2 dragOffsetInLocalSpace = Vector2.Zero; // 改回本地偏移计算
	private Control originalSlot;
	private AspectRatioContainer targetSlot;
	private CanvasLayer dragLayer;
	public Inventory inventory;
	public BoxList boxList;
	public Rect2 boxListGlobalRect;
	private int oSlotID;
	private String oItemID;
	
	public override void _Ready(){
		boxList = GetNode<BoxList>("/root/world/UILayer/BoxList");
		inventory = GetNode<Inventory>("/root/world/UILayer/Inventory");
		originalSlot = GetParent() as Control;
		dragLayer = GetNode<CanvasLayer>("/root/world/UILayer");
		boxListGlobalRect = new Rect2(boxList.GlobalPosition, boxList.Size);
	}

	

	public override void _Input(InputEvent @event){
		// 修改点3：增加关键安全检查！
		if (!IsInsideTree() || !inventory.Visible) return; // 如果本节点不在场景树中，直接返回，避免崩溃
		if (@event is InputEventMouseButton mbEvent && mbEvent.ButtonIndex == MouseButton.Left){
			Rect2 globalRect = new Rect2(GlobalPosition, Size);
			bool isMouseOverItem = globalRect.HasPoint(mbEvent.GlobalPosition);
			if (mbEvent.Pressed && isMouseOverItem){
				GetSlotID();
				// 开始拖拽
				isDragging = true;
				// 计算偏移量（使用全局坐标和物品自身全局坐标的差值）
				dragOffsetInLocalSpace = mbEvent.GlobalPosition - GlobalPosition;
				ZIndex = 100;
				// 关键：检查是否有有效的拖拽层
				if (dragLayer != null && originalSlot != null){
					// 记录原父节点和位置（局部坐标）
					Vector2 originalLocalPos = Position;
					originalSlot.RemoveChild(this);
					dragLayer.AddChild(this);
					// 设置新父节点下的全局位置，使其看起来在原地
					GlobalPosition = mbEvent.GlobalPosition - dragOffsetInLocalSpace;
				}
				else{
					GD.PrintErr("拖拽层或原始槽位未找到！");
					isDragging = false; // 无法拖拽
					return;
				}
				// 可选：标记事件已处理
				GetViewport().SetInputAsHandled();
			}
			else if (!mbEvent.Pressed && isDragging){
				isDragging = false;
				ZIndex = 0;
				DropMainController();
			}
		}
	}

	public override void _Process(double delta){
		if (isDragging){
			// 直接使用鼠标全局坐标更新位置
			GlobalPosition = GetGlobalMousePosition() - dragOffsetInLocalSpace;
		}
	}

	private bool GetSlotIDInBox(Node root){
		if (root is Items items){
			if (items == this){
				GD.Print("找到了");
				oSlotID++;
				return true;
			} 
			oSlotID++;
		}
		foreach (Node child in root.GetChildren()){
			bool found = GetSlotIDInBox(child);
			if (found){
				return true; // 在子节点分支中找到，立即返回
			}
		}
		return false;
	}

	private void GetSlotID(){
		if (originalSlot.IsInGroup("InvSlot")){
			oSlotID = inventory.GetSlotID(originalSlot as AspectRatioContainer);
		}
		else if (originalSlot.IsInGroup("BoxSlot")){
			oSlotID = -1;
			this.GetSlotIDInBox(boxList);
			GD.Print("箱子中的序号找到了，是"+oSlotID);
		}
	}

	private void DropMainController(){
		String slotType = FindSlotType();
		if(slotType == "NotFound"){
			ReturnToOriginalSlot();
			return;
		}
		GetItemID();
		DropJudge(slotType);
	}

	private void DropJudge(String slotType){
		if(oItemID.Substring(0, 2) == "00"){
			if(slotType == "Box"&&originalSlot.IsInGroup("InvSlot")){
				boxList.AddItem(oItemID);
				inventory.DeleteItem(oSlotID);
				ProcessMode = ProcessModeEnum.Disabled;
				QueueFree();
				return;
			}
			else if(targetSlot.IsInGroup("AbandonSlot")&&originalSlot.IsInGroup("InvSlot")){
				inventory.DeleteItem(oSlotID);
				ProcessMode = ProcessModeEnum.Disabled;
				QueueFree();
				return;
			}
			else if(targetSlot.IsInGroup("InvSlot")&&originalSlot.IsInGroup("InvSlot")){
				int tSlotID = inventory.GetSlotID(targetSlot);
				String tItemID = inventory.GetItem(tSlotID);
				inventory.DeleteItem(tSlotID);
				inventory.ChangeItem(tSlotID, oItemID);
				inventory.ChangeItem(oSlotID, tItemID);
				ProcessMode = ProcessModeEnum.Disabled;
				QueueFree();
				return;
			}
			else if(targetSlot.IsInGroup("InvSlot")&&originalSlot.IsInGroup("BoxSlot")){
				int tSlotID = inventory.GetSlotID(targetSlot);
				String tItemID = inventory.GetItem(tSlotID);
				GD.Print("原格子序号为"+oSlotID+" 原物品ID为"+oItemID+" 目标格子序号为"+tSlotID+" 目标物品ID为"+tItemID);
				inventory.ChangeItem(tSlotID, oItemID);
				if(tItemID != "000000"){
					boxList.ChangeItem(oSlotID, tItemID);
				}
				else{
					boxList.DeleteItem(oSlotID);
				}
				ProcessMode = ProcessModeEnum.Disabled;
				QueueFree();
				return;
			}
			else if(targetSlot.IsInGroup("AbandonSlot")&&originalSlot.IsInGroup("BoxSlot")){
				boxList.DeleteItem(oSlotID);
				ProcessMode = ProcessModeEnum.Disabled;
				QueueFree();
				return;
			}
		}
		if(oItemID.Substring(0, 2) == "11"){
			if(targetSlot.IsInGroup("RifleSlot")&&originalSlot.IsInGroup("BoxSlot")){
				String tItemID = inventory.ChangeEquipment(targetSlot, oItemID);
				if(tItemID != "000000"){
					boxList.ChangeItem(oSlotID, tItemID);
				}
				else{
					boxList.DeleteItem(oSlotID);
				}
				ProcessMode = ProcessModeEnum.Disabled;
				QueueFree();
				return;
			}
		}
		ReturnToOriginalSlot();
	}

	private void GetItemID(){
		if (originalSlot.IsInGroup("InvSlot")){
			oItemID = inventory.GetItem(oSlotID);
		}
		else if (originalSlot.IsInGroup("BoxSlot")){
			oItemID = boxList.GetItem(oSlotID);
		}
		
	}

	private String FindSlotType(){
		Vector2 mouseScreenPos = GetViewport().GetMousePosition();
		Rect2 boxListGlobalRect = new Rect2(boxList.GlobalPosition, boxList.Size);
		if (boxListGlobalRect.HasPoint(mouseScreenPos)&&boxList.Visible==true){
			return "Box";
		}
		targetSlot=FindARC(dragLayer, mouseScreenPos);
		if (targetSlot != null){
			return "Inv";
		}
		else{
			return "NotFound";
		}
	}

	private AspectRatioContainer FindARC(Node root, Vector2 screenPos){	
		// 遍历所有槽位（AspectRatioContainer）
		if (root is AspectRatioContainer aspectRatioContainer){
			Rect2 slotGlobalRect = new Rect2(aspectRatioContainer.GlobalPosition, aspectRatioContainer.Size);
			if (slotGlobalRect.HasPoint(screenPos)) return aspectRatioContainer;
		}
		foreach (Node child in root.GetChildren()){
			AspectRatioContainer found = FindARC(child, screenPos);
			if (found != null){
				return found; // 在子节点分支中找到，立即返回
			}
		}
		return null;
	}

	// 为了支持交换，ReturnToOriginalSlot 可以保持原样，但建议增加一点日志
	private void ReturnToOriginalSlot(){
		if (originalSlot != null && originalSlot.IsInsideTree()){
			GetParent()?.RemoveChild(this);
			originalSlot.AddChild(this);
			Position = Vector2.Zero;
			GD.Print("物品已返回原始槽位。");
		}
		else{
			GD.PrintErr("无法返回原始槽位，槽位无效或不在场景树中。");
			// 作为最后手段，可以销毁自己，但请谨慎
			// QueueFree();
		}
	}
}
