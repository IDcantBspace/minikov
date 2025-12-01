using Godot;
using System;

public partial class Enemy : Creature{
	
	private int speed = 100;
	
	[Export]
	public PackedScene BulletScene { get; set; }
	
	private Vector2 hpScale;
	
	private Timer enemyTimer;
	
	private Player player;
	
	private Vector2 moveDirection;
	
	[Export]
	public Sprite2D HP { get; set; }

	public override void _Ready(){
		player = GetNode<Node2D>("../Player") as Player;
		maxHealthPoint = 100;
		healthPoint = maxHealthPoint;
		hpScale = HP.Scale;
		AddToGroup("enemies");
		GD.Print($"曼波");
		
		// 创建并配置计时器
		enemyTimer = new Timer();
		AddChild(enemyTimer);
		enemyTimer.WaitTime = 0.5f; // 设置1秒间隔
		enemyTimer.Timeout += OnTimeOut; // 连接超时信号
		enemyTimer.Start();
		
		ChooseMoveDirection();
	}
	
	private void OnTimeOut(){
		if (player != null){
			Shoot();
		}
		
		ChooseMoveDirection();
	}
	
	private void Shoot(){
		// 安全检查
		if (BulletScene == null){
			GD.PrintErr("BulletScene is not assigned in the inspector!");
			return;
		}
		if (player.healthPoint == 0){
			return;
		}
		// 计算全球坐标系下的枪口位置
		Vector2 playerPosition = player.GlobalPosition;
		Vector2 enemyPosition = this.GlobalPosition;
		Vector2 shootDirection = (playerPosition - enemyPosition).Normalized();
		//GD.Print(
			//"敌人位置：("+enemyPosition.X+","+enemyPosition.Y+"),玩家位置：("+playerPosition.X+","+playerPosition.Y+")"
			//);
		// 实例化子弹
		Bullet bulletInstance = BulletScene.Instantiate<Bullet>();
		// 获取场景树根节点（或当前场景）并添加子弹实例
		GetTree().CurrentScene.AddChild(bulletInstance);
		// 设置子弹的初始位置和方向
		bulletInstance.GlobalPosition = GlobalPosition + shootDirection*50;
		// 调用子弹的初始化方法
		bulletInstance.Initialize(shootDirection);
	}
	
	//选择AI移动方向
	private void ChooseMoveDirection(){
		// 生成0-4的随机整数，对应5种行为
		int randomChoice = (int)GD.Randi() % 5;
		switch (randomChoice){
			case 0:
				moveDirection = Vector2.Up;
				break;
			case 1:
				moveDirection = Vector2.Down;
				break;
			case 2:
				moveDirection = Vector2.Left;
				break;
			case 3:
				moveDirection = Vector2.Right;
				break;
			case 4:
				moveDirection = Vector2.Zero;
				break;
		}
	}
	
	public override void _PhysicsProcess(double delta){
		EnemyMovement();
	}
	
	private void EnemyMovement(){
		Velocity = moveDirection * speed;
		MoveAndSlide();
	}

	public override void OnGetDamage(float damage){
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
