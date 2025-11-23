using Godot;
using System;

public partial class EnemyController : CharacterBody2D{
	
	[Export] 
	public PlayerController _player { get; private set; }
	
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
		//连接信号
		_player.HitEnemy += OnHitEnemy;
	}

	public void OnHitEnemy(){
		healthPoint = healthPoint-20;
		HP.Scale = new Vector2(hpScale.X*(healthPoint/maxHealthPoint),hpScale.Y);
		// 获取精灵纹理缩放后的变化量，目前伤害为20
		//float hpPositionMove = HP.Texture.GetWidth()*hpScale.X*(20/maxHealthPoint)/2;
		//HP.Position -= new Vector2(hpPositionMove,0);
		if(healthPoint==0){
			GD.Print($"呃啊！");
			_player.HitEnemy -= OnHitEnemy;
			ProcessMode = ProcessModeEnum.Disabled;
			QueueFree();
		}
	}
}
