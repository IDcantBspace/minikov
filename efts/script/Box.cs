using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class Box : Node2D
{
	[Signal]
	public delegate void OpenBoxEventHandler(Box box);
	[Export]
	public AnimationPlayer boxAnimationPlayer{ get; set; }
	//交互距离
	[Export]
	public float InteractionRange { get; set; } = 64.0f;
	public int listLength = 10;
	public List<String> itemsList;
	[Export]
	public Area2D area2d{ get; set; }
	private Player player;
	
	public override void _Ready(){
		player = GetNodeOrNull<Player>("/root/world/Player");
		itemsList = new List<String>();
		itemsList.Add("000001");
		itemsList.Add("000002");
		itemsList.Add("000003");
		itemsList.Add("000004");
		itemsList.Add("000005");
		itemsList.Add("000006");
		itemsList.Add("000007");
		itemsList.Add("110001");
		//GD.Print(itemsList[0]);
		AddToGroup("itemslist");
		area2d.InputEvent += OnInputEvent; 
	}
	
	public String GetItemInBox(int i){
		if(i>=itemsList.Count){
			return null;
		}
		else{
			return itemsList[i];
		}
	}
	
	public void AddItemInBox(String itemID){
		itemsList.Add(itemID);
	}
	
	public void DeleteItemInBox(int itemNum){
		GD.Print("box删除的位置是"+itemNum);
		itemsList.RemoveAt(itemNum);
	}
	
	public void ChangeItemInBox(int itemNum, String itemID){
		itemsList[itemNum] = itemID;
	}
	
	private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx){
		// 鼠标右键判定
		if (@event is InputEventMouseButton mouseEvent && 
			mouseEvent.ButtonIndex == MouseButton.Right &&
			mouseEvent.Pressed)
		{
			// 距离判定
			if (IsPlayerWithinRange())
			{
				GetViewport().SetInputAsHandled();
				boxAnimationPlayer.Play("open");
				OpenBox += player.OnOpenBox;
				EmitSignal(SignalName.OpenBox, this);
				OpenBox -= player.OnOpenBox;
			}
			else
			{
				GD.Print($"箱子 {Name}：玩家距离太远，无法打开。");
			}
		}
	}

	/// <summary>
	/// 判断玩家是否在可交互范围内。
	/// </summary>
	private bool IsPlayerWithinRange()
	{
		if (player == null)
		{
			GD.PrintErr($"{Name}: 未找到玩家节点，无法进行距离判定。");
			return false; // 找不到玩家则不允许交互
		}
		
		// 计算箱子与玩家之间的实际距离
		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
		// 如果距离小于等于交互范围，则返回true
		bool isInRange = distance <= InteractionRange;
		
		return isInRange;
	}
	
	public void OnUpdate(String[] newList){
		for(int i=0;i<listLength;i++){
			itemsList[i] = newList[i];
		}
	}
	
	
}
