using Godot;
using System;
using System.Linq;

public partial class Box : Node2D
{
	public int listLength = 6;
	public String[] itemsList;
	
	public override void _Ready(){
		itemsList = Enumerable.Repeat("000000", listLength).ToArray();
		itemsList[0] = "000007";
		//GD.Print(itemsList[0]);
		AddToGroup("itemslist");
	}
}
