using Godot;
using System;

public partial class PM : Weapon{
		//UI相关属性
	[Signal]
	public delegate void SwappedEventHandler(
		int swapType, AspectRatioContainer originSlot, AspectRatioContainer targetSlot
		);//6:丢弃武器
	private bool isDragging = false;
	private Vector2 dragOffsetInLocalSpace = Vector2.Zero; // 改回本地偏移计算
	private Control originalSlot;
	private AspectRatioContainer targetSlot;
	private CanvasLayer dragLayer;
	public Inventory inventory;
	
	public override void _Ready(){
		inventory = GetNode<Inventory>("/root/world/UILayer/Inventory");
		//Swapped += inventory.OnSwapped;
		firingRate = 300f;
		//damage = 20;
		fireModeManual = false;
		fireModeSemi = true;
		fireModeBurst = false;
		fireModeAuto = false;
		magazineSize = 8;
		reloadTime = 2.0f;
		tacReloadTime = 1.4f;
		originalSlot = GetParent() as Control;
		dragLayer = GetNode<CanvasLayer>("/root/world/UILayer");
	}

	public override void _Input(InputEvent @event){
		// 修改点3：增加关键安全检查！
		if (!IsInsideTree() || !inventory.Visible) return;
		if (@event is InputEventMouseButton mbEvent && mbEvent.ButtonIndex == MouseButton.Left){
			Rect2 globalRect = new Rect2(GlobalPosition, Size);
			bool isMouseOverItem = globalRect.HasPoint(mbEvent.GlobalPosition);
			if (mbEvent.Pressed && isMouseOverItem){
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

	private void TryDrop(){
		// 安全：如果不在场景树，直接返回
		if (!IsInsideTree()) return;
		Vector2 mouseScreenPos = GetViewport().GetMousePosition();
		targetSlot = FindSlotAtPosition(dragLayer, mouseScreenPos);
		if (targetSlot != null && targetSlot.IsInGroup("AbandonSlot")){
			PlaceIntoSlot(targetSlot);
		}
		else{
			ReturnToOriginalSlot();
		}
	}

	private AspectRatioContainer FindSlotAtPosition(Node root, Vector2 screenPos){
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

	private void PlaceIntoSlot(AspectRatioContainer targetSlot){
		// 1. 基础安全检查
		if (targetSlot == null || !targetSlot.IsInsideTree()){
			GD.PrintErr("PlaceIntoSlot: 目标槽位无效。");
			ReturnToOriginalSlot();
			return;
		}
		SwapSignal(originalSlot);
	}
	
	private void SwapSignal(Control oSlot){
		Inventory inventory = GetNode<Inventory>("/root/world/UILayer/Inventory");
		//Swapped += inventory.OnSwapped;
		GD.Print("signal");
		EmitSignal(SignalName.Swapped, 6, oSlot, oSlot);
		ProcessMode = ProcessModeEnum.Disabled;
		QueueFree();
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
