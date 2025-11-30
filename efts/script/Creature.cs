using Godot;
using System;

//这是个虚拟父类，不要用
public partial class Creature : CharacterBody2D{
	
	protected float maxHealthPoint;
	protected float healthPoint;

	public virtual void OnGetDamage(float damage){
		GD.Print($"你把父类的虚拟方法给搞出来了笨蛋");
	}
}
