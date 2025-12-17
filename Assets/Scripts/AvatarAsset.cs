using System;

[Serializable]
public class AvatarAsset
{
	public string id;
	public int slot;
	public string[] hides;
	public string group;
	public string morphTarget;
	public string type;
	public string variant;
	public string mesh;
	public string meshGlb;
	public string texture;
	public string icon;
	public int priority;
	public bool unlocked;
	public bool isEliteExclusive;
	/*public Requirement[] requirements;*/
	public string name;
	public string description;
	public int rarity;
}