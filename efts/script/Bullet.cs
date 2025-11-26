using Godot;
using System;

public partial class Bullet : CharacterBody2D{
	[Signal]
	public delegate void BulletHitEventHandler(float damage);
	
	private float damage = 20;
	
	[Export] // 允许在编辑器中设置子弹速度
	public int Speed { get; set; } = 800;
	
	[Export] // 设置子弹存在时间，避免飞出屏幕后一直存在
	public float Lifetime { get; set; } = 2.0f;
	
	private Vector2 _direction = Vector2.Right;
	private float _lifeTimer = 0f;

	// 提供一个方法，由发射者设置方向
	public void Initialize(Vector2 direction){
		_direction = direction.Normalized();
		// 可选：让子弹一出生就面向移动方向，视觉效果更好
		Rotation = _direction.Angle();
	}

	public override void _PhysicsProcess(double delta){
		// 更新存在时间
		_lifeTimer += (float)delta;
		if (_lifeTimer >= Lifetime){
			ProcessMode = ProcessModeEnum.Disabled;
			QueueFree(); // 超过存在时间，销毁自身
			return;
		}
		// 设置速度并移动
		Velocity = _direction * Speed;
		MoveAndSlide();
		
		if (GetSlideCollisionCount() > 0){
			KinematicCollision2D collision = GetSlideCollision(0);
			Enemy collider = collision.GetCollider() as Enemy;

			if (collider != null){
				// 使用分组进行碰撞类型判断
				if (collider.IsInGroup("enemies")){
					BulletHit += collider.OnHitEnemy;
					EmitSignal(SignalName.BulletHit,damage);
					BulletHit -= collider.OnHitEnemy;
				}
				/*else if (collider.IsInGroup("player")){
					BulletHit += collider.OnHitPlayer;
					EmitSignal(SignalName.BulletHit,damage);
					BulletHit -= collider.OnHitPlayer;
				}*/
			}
			// 可以在这里播放爆炸特效、音效
			ProcessMode = ProcessModeEnum.Disabled;
			QueueFree();
		}
	}
}
