using Godot;
using System;

public partial class Enemy : CharacterBody2D{
	
	private float maxHealthPoint = 100;
	private float healthPoint;
	private Vector2 hpScale;
	
	
	[Export]
	public Sprite2D HP { get; set; }

	public override void _Ready(){
		healthPoint = maxHealthPoint;
		hpScale = HP.Scale;
		AddToGroup("enemies");
		GD.Print($"曼波");
	}

	public void OnHitEnemy(float damage){
		healthPoint = healthPoint-damage;
		HP.Scale = new Vector2(hpScale.X*(healthPoint/maxHealthPoint),hpScale.Y);
		// 获取精灵纹理缩放后的变化量
		//float hpPositionMove = HP.Texture.GetWidth()*hpScale.X*(damage/maxHealthPoint)/2;
		//HP.Position -= new Vector2(hpPositionMove,0);
		if(healthPoint==0){
			GD.Print($"呃啊！");
			ProcessMode = ProcessModeEnum.Disabled;
			QueueFree();
		}
	}
}
