using Godot;
using System;
using System.Linq;

public partial class Box : Node2D
{
	[Export]
	public AnimationPlayer boxAnimationPlayer{ get; set; }
	public int listLength = 6;
	public String[] itemsList;
	
	public override void _Ready(){
		itemsList = Enumerable.Repeat("000000", listLength).ToArray();
		itemsList[0] = "000007";
		//GD.Print(itemsList[0]);
		AddToGroup("itemslist");
	}
	
	//PS:信号这个硬性要求的参数真逆天啊
	public void OnOpenBox(bool useless1, Node useless2){
		boxAnimationPlayer.Play("open");
	}
	
	public void OnUpdate(String[] newList){
		for(int i=0;i<listLength;i++){
			itemsList[i] = newList[i];
		}
	}
	
	
}
