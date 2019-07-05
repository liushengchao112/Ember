using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using Logic;
using Utils;
using Data;

using PowerUps = Data.PowerUpsProto.PowerUps;

public enum PowerUpType
{
    None = 0,
    Power = 1,
    Armor = 2,
    MovementSpeed = 3,
    Heal = 4
}

public enum CalculateType
{
    None = 0,
    NaturalNumber = 1,
    Percent = 2
}

public class PowerUp : LogicUnit
{
    public PowerUpType PowerUpType;

    public PowerUps proto;
    public int calculateType = 0;
    public int lifeTime = 0;

    private float mainValue = 0;
    public float MainValue
    {
        get
        {
            return mainValue;
        }
    }

    private Town ownerTown;
    private List<Soldier> soldiers;

    public void Initialize( int powerUpId, PowerUpType powerUpType, FixVector3 pos )
    {
        id = powerUpId;
        position = pos;
        PowerUpType = powerUpType;

        proto = DataManager.GetInstance().powerUpsProtoData.Find( p => p.ID == (int)powerUpType );
        modelId = proto.Model;
        mainValue = proto.Value;
        calculateType = proto.CalculateType;
        lifeTime = proto.LifeTime;

        if ( proto == null )
        {
            DebugUtils.LogError( DebugUtils.Type.Battle, string.Format( "Can't find the proto data by PowerUpType , type = {0}", powerUpType ) );
        }

    }

    public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
    {

    }

    public override void AddCoin( int coin )
    {
        
    }

    public override void LogicUpdate( int deltaTime )
    {

    }

    public override bool Alive()
    {
        return true;
    }

    public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
    {
        DebugUtils.Assert( false, "OnKillEnemy() in PowerUp is not implemented!" );
    }

    public override void Reset()
    {

    }

    public void PickedUp( Town town = null )
    {
        if( town != null )
        {
            mark = town.mark;
            ownerTown = town;

            soldiers = ownerTown.GetAliveSoldiers();

            for ( int i = 0; i < soldiers.Count; i++ )
            {
                AddBuff( soldiers[i] );
            }
        }

        // Post rm
        RenderMessage rm = new RenderMessage();
        rm.ownerId = id;
        rm.type = RenderMessage.Type.PowerUpPickedUp;

        PostDestroy( this );
        PostRenderMessage(rm);
    }

    public void StartRelease()
    {
        soldiers = ownerTown.GetAliveSoldiers();
    }

    private void AddBuff( Soldier soldier )
    {
        //if ( PowerUpType == PowerUpType.MovementSpeed )
        //{
        //    MoveSpeedBuff ms = new MoveSpeedBuff();
        //    ms.Init( -1, -1, lifeTime, mainValue, calculateType );
        //    ms.Attach( this, soldier );
        //}
        //else if ( PowerUpType == PowerUpType.Armor )
        //{
        //    ArmorBuff db = new ArmorBuff();
        //    db.Init( -1, -1, lifeTime, mainValue, calculateType );
        //    db.Attach( this, soldier );
        //}
        //else if ( PowerUpType == PowerUpType.Heal )
        //{
        //    HealBuff heal = new HealBuff();
        //    heal.Init( -1, -1, lifeTime, mainValue, calculateType );
        //    heal.Attach( this, soldier );
        //}
        //else if ( PowerUpType == PowerUpType.Power )
        //{
        //    PhysicalAttackBuff power = new Logic.PhysicalAttackBuff();
        //    power.Init( -1, -1, lifeTime, mainValue, calculateType );
        //    power.Attach( this, soldier );
        //}
    }

}
