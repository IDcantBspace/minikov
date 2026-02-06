using Godot;
using System;

public partial class Equipment : Panel{
	[Export]
	public Inventory inventory{ get; set; }
	[Export]
	public Panel equipmentPanel{ get; set; }
	//背包格
	[Export]
	public AspectRatioContainer bagSlot{ get; set; }
	public String bag;
	public bool IsOpen = false;
	
	[Export]
	public PackedScene genericItem { get; set; }
	[Export]
	public Color CloseColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.5f);
	
	[Export]
	public Color OpenColor { get; set; } = new Color(0.0f, 0.0f, 0.0f, 0.392f);
	
	private StyleBoxFlat styleBox;
	
	public override void _Ready(){
		// 设置鼠标过滤器为Stop，这样节点才能接收鼠标事件
		MouseFilter = MouseFilterEnum.Stop;
		bagSlot.AddToGroup("BagSlot");
		bag = "000000";
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
	
	public (int,int) GetBag(){
		if(bag.Substring(0, 2) == "21"){
			GearData newGearData = GearDatabase.Instance.GetGear(bag);
			return (newGearData.slotColumnNum,newGearData.slotRowNum);
		}
		else{
			if(bag != "000000"){
				bag = "000000";
			}
			return (0,0);
		}
	}
	
	public void DeleteEquipment(AspectRatioContainer tSlot){
		if(tSlot == bagSlot){
			bag = "000000";
		}
		//else if(tSlot == rifleSlot2){
			//rifle2 = "000000";
			//equipmentNeedToUpdate = 2;
		//}
		//else if(tSlot == pistolSlot){
			//GD.Print("delete work");
			//pistol = "000000";
			//equipmentNeedToUpdate = 3;
		//}
		inventory.Initialize();
	}
	
	public String GetEquipment(AspectRatioContainer tSlot){
		if(tSlot == bagSlot){
			return bag;
		}
		return "000000";
	}
	
	public String ChangeEquipment(AspectRatioContainer tSlot, String oItemID){
		String temEquipmentID = "000000";
		if(tSlot == bagSlot){
			temEquipmentID = bag;
			bag = oItemID;
			ItemData newItemData = ItemDatabase.Instance.GetItem(oItemID);
			if (newItemData != null){
				Items newItem = genericItem.Instantiate<Items>();
				bagSlot.AddChild(newItem);
				newItem.Name = oItemID;
				newItem.Texture = newItemData.equipmentTexture;
				}
			}
		inventory.Initialize();
		return temEquipmentID;
	}
	
	public void Initialize(){
		this.Visible = true;
	}
	
	public void Close(){
		this.Visible = false;
	}
	
}
