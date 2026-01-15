using Godot;
using System;

public partial class ItemPanel : Panel{
	[Export]
	public TextureRect item{ get; set; }
	[Export]
	public AspectRatioContainer slot{ get; set; }
	
	public override void _Ready(){
		slot.AddToGroup("BoxSlot");
	}
	
	public void ChangeTexture(String name, Texture2D texture){
		item.Name = name;
		if(texture != null && item != null){
			item.Texture = texture;
		}
	}
}
