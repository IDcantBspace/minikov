using Godot;
using System;

public partial class Player : Creature{
	
	private int speed = 200;
	
	[Export]
	public PackedScene BulletScene { get; set; }
	
	[Export]
	public PackedScene GameOverScene { get; set; }
	
	//暂时！ 敌人预制场景
	[Export]
	public PackedScene EnemyScene { get; set; }
	
	[Export] // 子弹生成位置偏移
	public Vector2 MuzzleOffset { get; set; } = new Vector2(50, 0);
	
	[Export] // 射击间隔（秒），控制射速
	public float FireRate { get; set; } = 0.1f;
	
	// 引用Sprite2D节点
	private Sprite2D _sprite;
	
	
	private float _fireTimer = 0f;
	private bool _isFiring = false;
	
	//private RandomNumberGenerator randomNum = new RandomNumberGenerator();
	
	// 初始化函数
	public override void _Ready(){
		// 以时间为种子生成随机数(弃用，改用GD生成随机数)
		//randomNum.Randomize();
		
		maxHealthPoint = 100;
		healthPoint = maxHealthPoint;
		AddToGroup("player");
		
		// 获取Sprite2D节点
		_sprite = GetNode<Sprite2D>("Sprite2D");
		// 如果场景中没有Sprite2D节点，可以在这里创建一个
		if (_sprite == null){
			GD.Print("警告：未找到Sprite2D节点，请确保在场景中添加了Sprite2D子节点");
		}
	}
	
	// 在编辑器和游戏中绘制枪口位置
	public override void _Draw(){
		Vector2 rotatedOffset = MuzzleOffset.Rotated(_sprite.Rotation);
		DrawCircle(rotatedOffset, 5, Colors.Red); // 红色圆点标记枪口位置
		DrawLine(Vector2.Zero, rotatedOffset, Colors.Yellow, 2); // 黄线连接角色中心和枪口
	}

	// 每帧更新绘制
	public override void _Process(double delta){
		QueueRedraw(); 
		HandleFiring((float)delta);
		if (Input.IsActionJustPressed("testDeployEnemy"))
		{
			deployEnemy();
		}
	}

	// 射击
	private void HandleFiring(float delta){
		// 1. 检测鼠标左键按下状态
		_isFiring = Input.IsMouseButtonPressed(MouseButton.Left);
		if (_isFiring){
			// 2. 射击间隔计时
			_fireTimer -= delta;
			if (_fireTimer <= 0){
				// 3. 重置计时器并执行射击
				_fireTimer = FireRate;
				Shoot();
			}
		}
		else{
			// 松开鼠标时重置计时器，可以立即射击
			_fireTimer = 0;
		}
	}

	public bool checkCollision(Vector2 position, float radius = 10f){
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = new PhysicsShapeQueryParameters2D();
		// 使用圆形检测区域
		var shape = new CircleShape2D();
		shape.Radius = radius;
		query.Shape = shape;
		query.Transform = new Transform2D(0, position);
	
		// 设置碰撞层掩码（根据你的项目设置调整）
		query.CollisionMask = 1; // 检测第一层碰撞
	
		var results = spaceState.IntersectShape(query);
		return results.Count == 0; // 如果没有碰撞结果，则可以生成
	}

	// 生成敌人
	private void deployEnemy(){
		// 安全检查
		if (EnemyScene == null){
			GD.PrintErr("EnemyScene is not assigned in the inspector!");
			return;
		}
		// 设置初始位置
		Vector2 spawnPosition = GlobalPosition + new Vector2(
			(float)GD.RandRange(-500.0f, 500.0f),(float)GD.RandRange(-500.0f, 500.0f)
			);
		if (checkCollision(spawnPosition, 50f)){
			// 1. 实例化
			Enemy enemyInstance = EnemyScene.Instantiate<Enemy>();
			// 2. 获取场景树根节点（或当前场景）并添加实例
			GetTree().CurrentScene.AddChild(enemyInstance);
			enemyInstance.GlobalPosition = spawnPosition;
			GD.Print("敌人生成成功，坐标：" + spawnPosition.X + " " + spawnPosition.Y);
		}
		else{
			GD.Print("生成位置被占用，尝试其他位置");
			// 可以在这里实现重试逻辑
		}
	}

	// 生成子弹
	private void Shoot(){
		// 安全检查
		if (BulletScene == null){
			GD.PrintErr("BulletScene is not assigned in the inspector!");
			return;
		}
		// 1. 实例化子弹
		Bullet bulletInstance = BulletScene.Instantiate<Bullet>();
		// 2. 获取场景树根节点（或当前场景）并添加子弹实例
		GetTree().CurrentScene.AddChild(bulletInstance);
		// 3. 设置子弹的初始位置和方向
		// 计算全球坐标系下的枪口位置
		Vector2 muzzleGlobalPosition = GlobalPosition + MuzzleOffset.Rotated(_sprite.Rotation);
		bulletInstance.GlobalPosition = muzzleGlobalPosition;
		// 计算朝向鼠标的方向
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 shootDirection = (mousePos - muzzleGlobalPosition).Normalized();
		// 调用子弹的初始化方法
		bulletInstance.Initialize(shootDirection);
	}

	// 每帧处理
	public override void _PhysicsProcess(double delta){
		// 移动逻辑
		HandleMovement();
		// 旋转逻辑
		HandleRotation();
	}

	// 移动逻辑
	private void HandleMovement(){
		Vector2 inputVector = Input.GetVector("move_W", "move_E", "move_N", "move_S");
		Velocity = inputVector * speed;
		MoveAndSlide();
	}

	// 旋转逻辑
	private void HandleRotation(){
		if (_sprite == null) return;
		// 获取鼠标在全局坐标系中的位置
		Vector2 mousePosition = GetGlobalMousePosition();
		// 计算角色位置到鼠标位置的方向向量
		Vector2 direction = mousePosition - GlobalPosition;
		// 计算旋转角度（弧度）
		float angle = direction.Angle();
		// 将弧度转换为角度并设置Sprite的旋转
		_sprite.Rotation = angle;
	}
	
	public override void OnGetDamage(float damage){
		healthPoint = healthPoint - damage;
		GD.Print($"我血流满地啊 HP:"+healthPoint+"/"+maxHealthPoint);
		if(healthPoint==0){
			GD.Print($"呃啊！");
			Node2D gameOverInstance = GameOverScene.Instantiate<Node2D>();
			// 2. 获取场景树根节点（或当前场景）并添加实例
			GetTree().CurrentScene.AddChild(gameOverInstance);
			gameOverInstance.GlobalPosition = GlobalPosition;
			GetTree().Paused = true;
			ProcessMode = ProcessModeEnum.Disabled;
			CallDeferred("queue_free");
		}
	}
}
