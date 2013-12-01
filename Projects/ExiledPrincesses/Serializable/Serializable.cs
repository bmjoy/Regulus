﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Regulus.Project.ExiledPrincesses
{
    [Serializable]
    public enum Strategy
    {        
        Sword,
        Ax,
        Staff,        
        Shield,
        Count,
        None = Count
    };
    
	[Serializable]
	public class AccountInfomation
	{
		public string Name { get; set; }
		public string Password { get; set; }
		public Guid Id { get; set; }
        public GameRecord Record { get; set; }
	}

    [Serializable]
    public class Contingent
    {
        public enum FormationType
        {
            Auxiliary,
            Defensive,
        }
        public FormationType Formation { get; set; }
        public Guid[] Members { get; set; }
    }

    [Serializable]
    public class GameRecord
    {
        public GameRecord()
        {
            Actors = new ActorInfomation[] { };
        }
        public ActorInfomation[] Actors { get; set; }
        public string Tone { get; set; }
        public Contingent Contingent { get; set; }


        public ActorInfomation[] GetContingentActors()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class Ability
    {
        public Ability(int hp, int _int, int dex, int exp, int[] skills)
        {
            this.Hp = hp;
            this.Int = _int;
            this.Dex = dex;
            this.Exp = exp;
            Skills = skills;
        }
        public int Hp { get; set; }
        public int Int { get; set; }
        public int Dex { get; set; }
        public int Exp { get; set; }
        public int[] Skills { get; set; }
    }
    

    [Serializable]
    public class ActorPrototype
    {        
        public Strategy Specializes { get; set; }
        public Strategy Weakness { get; set; } 
        public Ability[] Abilitys { get; set; }

        public Ability FindAbility(int exp)
        {
            return (from a in Abilitys where a.Exp < exp orderby a.Exp descending select a).FirstOrDefault();
        }
    }

    [Serializable]
    public class ActorInfomation
    {        
        public Guid Id { get; set; }
        public int Prototype { get; set; }        
        public int Exp { get; set; }
    }

    [Serializable]
    public enum UserStatus
    {
        None,
        Verify,
        Tone,
        Adventure,        
    }

	[Serializable]
	public enum LoginResult
	{
        Success,
        Fail,
        Repeat
	}
    [Serializable]
    public class Station
    {
        public Guid Id;
        public enum Kind
        {
            None,
            Combat,
            Choice
        }
        virtual public Kind GetKind()
        {
            return Kind.None;
        }
        public float Position { get; set; }
    }

    [Serializable]
    public class MapPrototype
    {
        public string Tone { get; set; }
        public Station[] Stations { get; set; }
    }
    [Serializable]
    public class BattlefieldPrototype
    {
        public Contingent.FormationType Formation { get; set; }
        public int[] Enemys { get; set; }
        public int Money { get; set; }
    }


    [Serializable]
    public class TonePrototype
    {
        public string[] Maps { get; set; } 
    }
}