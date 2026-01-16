using Godot;
using System;

public partial class Items : TextureRect{
	[Signal]
	public delegate void SwappedEventHandler(int swapType, int originSlotID, int targetSlotID);
	//0:背包内交换  1:箱子内交换  2:背包换去箱子或销毁  3:箱子换去背包  4:箱子销毁
	private bool isDragging = false;
	private Vector2 dragOffsetInLocalSpace = Vector2.Zero; // 改回本地偏移计算
	private Control originalSlot;
	private AspectRatioContainer targetSlot;
	private CanvasLayer dragLayer;
	public Inventory inventory;
	public BoxList boxList;
	public Rect2 boxListGlobalRect;
	private int oSlotID;
	
	public override void _Ready(){
		boxList = GetNode<BoxList>("/root/world/UILayer/BoxList");
		inventory = GetNode<Inventory>("/root/world/UILayer/Inventory");
		Swapped += inventory.OnSwapped;
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
				if (originalSlot.IsInGroup("InvSlot")){
					oSlotID = inventory.GetSlotID(originalSlot as AspectRatioContainer);
				}
				else if (originalSlot.IsInGroup("BoxSlot")){
					oSlotID = -1;
					this.GetSlotID(boxList);
					GD.Print("箱子中的序号找到了，是"+oSlotID);
				}
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
				TryDrop();
			}
		}
	}

	public override void _Process(double delta){
		if (isDragging){
			// 直接使用鼠标全局坐标更新位置
			GlobalPosition = GetGlobalMousePosition() - dragOffsetInLocalSpace;
		}
	}

	private bool GetSlotID(Node root){
		if (root is Items items){
			if (items == this){
				GD.Print("找到了");
				oSlotID++;
				return true;
			} 
			oSlotID++;
		}
		foreach (Node child in root.GetChildren()){
			bool found = GetSlotID(child);
			if (found){
				return true; // 在子节点分支中找到，立即返回
			}
		}
		return false;
	}


	private void TryDrop(){
		// 安全：如果不在场景树，直接返回
		if (!IsInsideTree()) return;
		
		Vector2 mouseScreenPos = GetViewport().GetMousePosition();
		targetSlot = FindSlotAtPosition(dragLayer ,mouseScreenPos);
		if (targetSlot != null){
			PlaceIntoSlot();
		}
		else{
			ReturnToOriginalSlot();
		}
	}

	private AspectRatioContainer FindSlotAtPosition(Node root, Vector2 screenPos){	
		if (boxListGlobalRect.HasPoint(screenPos)&&boxList.Visible==true){
			String itemID = inventory.GetItem(oSlotID);
			boxList.AddItem(itemID);
			SwapSignal(originalSlot);
			return null;
		}
		
		// 遍历所有槽位（AspectRatioContainer）
		if (root is AspectRatioContainer aspectRatioContainer){
			Rect2 slotGlobalRect = new Rect2(aspectRatioContainer.GlobalPosition, aspectRatioContainer.Size);
			if (slotGlobalRect.HasPoint(screenPos)) return aspectRatioContainer;
		}
		foreach (Node child in root.GetChildren()){
			AspectRatioContainer found = FindSlotAtPosition(child, screenPos);
			if (found != null){
				return found; // 在子节点分支中找到，立即返回
			}
		}
		return null;
	}

	private void PlaceIntoSlot(){
		// 1. 基础安全检查
		if (targetSlot == null || !targetSlot.IsInsideTree()){
			GD.PrintErr("PlaceIntoSlot: 目标槽位无效。");
			ReturnToOriginalSlot();
			return;
		}
		//拖动到销毁槽
		if (targetSlot.IsInGroup("AbandonSlot")){
			SwapSignal(originalSlot);
		}
		// 2. 查找目标槽位中已存在的物品
		Items targetItem = null;
		foreach (Node child in targetSlot.GetChildren()){
			// 注意：这里假设你的物品脚本名为`Items`
			if (child is Items item) {
				targetItem = item;
				break;
			}
		}
		// 3. 执行放置或交换
		if (targetItem == null){
			// 情况A：目标槽位为空，直接放入
			GetParent()?.RemoveChild(this);
			targetSlot.AddChild(this);
			Position = Vector2.Zero; // 重置在新槽位内的位置
			GD.Print("准备signal");
			SwapSignal(originalSlot,targetSlot);
			// 更新自己的原始槽位记录
			originalSlot = targetSlot;
			//物品已放置到空槽位
		}
		else{
			// 情况B：目标槽位有物品，执行交换
			// 3.1 安全检查：确保两个物品不是同一个，且有原始槽位
			if (targetItem == this || originalSlot == null){
				GD.PrintErr("无法与自己交换或原始槽位丢失。");
				ReturnToOriginalSlot();
				return;
			}
			// 3.2 交换核心逻辑
			// a) 将目标物品移动到我原来的槽位
			targetItem.GetParent()?.RemoveChild(targetItem);
			originalSlot.AddChild(targetItem);
			targetItem.Position = Vector2.Zero; // 目标物品在原始槽位中复位
			// **关键**：更新目标物品记录的“原始槽位”，现在对它而言，它的新家就是我的原槽位
			targetItem.originalSlot = originalSlot;
			// b) 将我移动到目标槽位
			GetParent()?.RemoveChild(this);
			targetSlot.AddChild(this);
			Position = Vector2.Zero; // 我在新槽位中复位
			// 3.3 发出交换完成的信号，方便其他系统更新数据
			GD.Print("准备signal");
			SwapSignal(originalSlot,targetSlot);
			// 更新我自己的原始槽位记录，现在我的新家就是目标槽位
			originalSlot = targetSlot;
			GD.Print($"物品交换成功！与槽位中的物品互换了位置。");
		}
	}
	
	private void SwapSignal(Control oSlot){
		GD.Print("signal");
		if (oSlot.IsInGroup("InvSlot")){
			EmitSignal(SignalName.Swapped, 2, oSlotID, 0);
		}
		else if (oSlot.IsInGroup("BoxSlot")){ //4
			boxList.DeleteItem(oSlotID);
		}
		ProcessMode = ProcessModeEnum.Disabled;
		QueueFree();
	}
	
	//信号发送筛选
	private void SwapSignal(Control oSlot, Control tSlot){
		int tSlotID = inventory.GetSlotID(tSlot as AspectRatioContainer);
		GD.Print("signal");
		if (oSlot.IsInGroup("InvSlot") && tSlot.IsInGroup("InvSlot")){
			EmitSignal(SignalName.Swapped, 0, oSlotID, tSlotID);
		}
		else if (oSlot.IsInGroup("BoxSlot") && tSlot.IsInGroup("BoxSlot")){
			EmitSignal(SignalName.Swapped, 1, oSlotID, tSlotID);
		}
		else if (oSlot.IsInGroup("BoxSlot") && tSlot.IsInGroup("InvSlot")){ //3
			String oItemID = boxList.GetItem(oSlotID);
			String tItemID = inventory.GetItem(tSlotID);
			GD.Print("原格子序号为"+oSlotID+" 原物品ID为"+oItemID+" 目标格子序号为"+tSlotID+" 目标物品ID为"+tItemID);
			inventory.ChangeItem(tSlotID, oItemID);
			if(tItemID != "000000"){
				boxList.ChangeItem(oSlotID, tItemID);
			}
			else{
				boxList.DeleteItem(oSlotID);
			}
		}
		
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
