using Godot;
using System;

public partial class Weapon : TextureRect{
	
	//射击相关属性
	public float firingRate;
	//public float damage;
	public bool fireModeManual;
	public bool fireModeSemi;
	public bool fireModeBurst;
	public bool fireModeAuto;
	public int magazineSize;
	public float reloadTime;
	public float tacReloadTime;
	[Export] 
	public AudioStream gunshotSound { get; set; }
	[Export] 
	public AudioStream reloadSound { get; set; }
}
