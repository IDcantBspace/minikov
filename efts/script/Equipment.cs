using Godot;
using System;

public partial class Equipment : Panel{
	[Export]
	public Inventory inventory{ get; set; }
	[Export]
	public Panel equipmentPanel{ get; set; }
	
	public bool IsOpen = false;
	
	[Export]
	public Color CloseColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.5f);
	
	[Export]
	public Color OpenColor { get; set; } = new Color(0.0f, 0.0f, 0.0f, 0.392f);
	
	private StyleBoxFlat styleBox;
	
	public override void _Ready(){
		// 设置鼠标过滤器为Stop，这样节点才能接收鼠标事件
		MouseFilter = MouseFilterEnum.Stop;
		
		// 创建样式
		styleBox = new StyleBoxFlat();
		styleBox.BgColor = CloseColor;
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
			equipmentPanel.Visible = false;
			IsOpen = false;
		}
		else{
			inventory.OnSwapped();
			styleBox.BgColor = OpenColor;
			equipmentPanel.Visible = true;
			IsOpen = true;
		}
		QueueRedraw();
		GD.Print("可点击区域被点击了！");
		// 在这里执行你的函数
	}
	
	public void Initialize(){
		this.Visible = true;
	}
	
	public void Close(){
		this.Visible = false;
	}
	
}
