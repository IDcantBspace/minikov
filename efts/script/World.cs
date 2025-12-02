using Godot;
using System;

public partial class World : Node2D{
	
	//private Panel inventory;
	
	public override void _Ready(){
		//ProcessMode = ProcessModeEnum.Always;
		//inventory = GetNode<Panel>("UILayer/Inventory");
	}
	
	public override void _Process(double delta){
		//if (Input.IsActionJustPressed("openInventory")){
			//if(inventory != null && inventory.Visible == true){
				//inventory.Visible = false;
			//}
			//else if(inventory != null && inventory.Visible == false){
				//inventory.Visible = true;
			//}
		//}
		
		if (Input.IsActionJustPressed("ui_cancel")){
			GetTree().Quit();
		}
	}
}
