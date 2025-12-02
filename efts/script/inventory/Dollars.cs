using Godot;
using System;

public partial class Dollars : TextureRect{
	private bool _isDragging = false;
	private Vector2 _dragOffset = Vector2.Zero; // 记录鼠标点击位置与物品自身的偏移[citation:10]

	public override void _Input(InputEvent @event){
		// 处理鼠标左键按下事件
		if (@event is InputEventMouseButton mbEvent && mbEvent.ButtonIndex == MouseButton.Left){
			// 获取此物品在全局坐标系下的矩形区域[citation:10]
			Rect2 globalRect = new Rect2(GlobalPosition, Size);

			if (mbEvent.Pressed){
				// 如果按下时鼠标在物品范围内，开始拖拽[citation:10]
				if (globalRect.HasPoint(mbEvent.GlobalPosition)){
					_isDragging = true;
					_dragOffset = mbEvent.GlobalPosition - GlobalPosition;
					// 可以在此处将物品设为所有节点的顶层，避免被遮挡[citation:9]
				}
			}
			else{
				// 鼠标释放，结束拖拽
				_isDragging = false;
				// 在此处触发“放置物品”的逻辑，例如与格子交换位置
				HandleDrop();
			}
		}
	}

	public override void _Process(double delta){
		// 如果正在拖拽，让物品位置跟随鼠标（减去偏移量）[citation:10]
		if (_isDragging){
			GlobalPosition = GetGlobalMousePosition() - _dragOffset;
		}
	}

	private void HandleDrop(){
		// 核心：处理物品放置逻辑
		// 1. 使用射线检测等方法，获取鼠标下方是哪个“物品格”(Slot)。
		// 2. 通知物品栏管理器（如 Inventory.cs），进行物品位置交换或堆叠的逻辑判断。
		// 3. 根据管理器的结果，将此物品节点的父节点设置为新的格子，并重置其位置。
	}
}
