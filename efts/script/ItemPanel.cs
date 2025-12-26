using Godot;
using System;

public partial class ItemPanel : Panel{
	[Export]
	public TextureRect item{ get; set; }
	
	public void ChangeTexture(Texture2D texture){
		if(texture != null && item != null){
			item.Texture = texture;
		}
	}
}
