using Godot;
using System;

public partial class World : Node2D{
	

	
	public override void _Ready(){
		
	}
	
	public override void _Process(double delta){
		if (Input.IsActionJustPressed("ui_cancel")){
			GetTree().Quit();
		}
	}
}
