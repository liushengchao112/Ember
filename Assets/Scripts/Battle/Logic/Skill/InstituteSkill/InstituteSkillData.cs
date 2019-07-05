using System.Collections;
using System.Collections.Generic;

public class InstituteSkillData 
{
	public int skillID;
	public int skillIconID;
	public int skillNameID;
	public int skillDescriptionID;
	public int skillCost;
	public string BuffsID;
	public int nextSkillID;
	public int skillRequiredLevel;
	public int skillLevel;

	public void SetSkillData( int skillID, int skillLv, int skillRequiredLevel, int skillIconID, int skillNameID, int skillDescription, int skillCost, string buffsID, int nextSkillID )
	{
		this.skillID = skillID;
		this.skillLevel = skillLv;
		this.skillRequiredLevel = skillRequiredLevel;
		this.skillIconID = skillIconID;
		this.skillNameID = skillNameID;
		this.skillDescriptionID = skillDescription;
		this.skillCost = skillCost;
		this.nextSkillID = nextSkillID;
		this.BuffsID = buffsID;
	}
}
