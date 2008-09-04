/*
 Party Shaman is the complete work of Pontus - I just maintain it since he has retired.
Latest version can always be found at: http://vforums.mmoglider.com/showthread.php?t=162461 
Developers: TheUltimateParadox - acepimprace- miceiken
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text;
using System.IO;

using Glider.Common.Objects;

namespace Glider.Common.Objects
{
    public class PShaman : GGameClass
    {
	string version = "2.0.9";

         static Mover mover;
        public static Random random = new Random();
        static float PI = (float)Math.PI;
	#region PShaman props

        enum ItemUse_e { NoUse, MyHealth50, MyHealth25, MobHealth75, MobHealth50, SaveForAdds, Feared };
	enum PullMethod_e { Bolt, Shock, WalkUp };
 	enum ChaseStyle_e { Chase, ChaseSafe, NoChase };
	enum PvPStyle_e { Active, FightBack, Die };
        enum BuffUse_e { NoUse, Food, Potion }
        enum HealType_e { None, Wave, Lesser, Chain, Panic }
        enum Shield_e { None, Lightning, Water }
        enum WhenCast_e { Never, Resting, AfterCast, Always }
        enum Shock_e { None, Earth, Frost, Flame }

	enum Totem_e {
	    None, 
	    Flametongue,
	    FireNova,
	    FrostResistance,
	    Magma,
	    Searing,
	    FireElemental,
	    Wrath, 

	    Earthbind,
	    Stoneskin,
	    Stoneclaw,
	    Strength,
	    Tremor,
	    EarthElemental,

	    FireResistance,
	    HealingStream,
	    ManaSpring,
	    ManaTide,
	    PoisonCleansing ,
	    DiseaseCleansing,

	    GraceOfAir,
	    Grounding,
	    NatureResistance,
	    Sentry,
	    Tranquil,
	    Windfury,
	    Windwall,
	    WrathOfAir	    
	};

	enum SkillCondition_e 
	{
	    Never, 
	    Always, 
	    Mob50HP, 
	    Add, 
	    CloseAdd,
	    Silence	    
	};


	// Configurable parameters
	

        Shield_e Shield;
        WhenCast_e WhenShield;

	bool RunFromAddsInCombat      = true;  // Move away if mobs are getting to close while in combat
	int AvoidAddDistance    = 23;    // Avoid adds when the get this close to me
      
	int MyPullDistance ;
	PullMethod_e PullMethod;


        WhenCast_e WhenPoison;
        WhenCast_e WhenDisease;

        bool TotemsBeforePull ;
        int  WaitTime ;
        

        int   BoltDistance;
        bool  BoltOnFocused;
        bool  BoltSpam ;

	double HealMana;
	double HealPanicLife;
        double HealLesserWaveLife;
        double HealWaveLife;
        double HealChainLife;
        double RestHealWaveLife;
        double RestHealChainLife;

	bool UseStormstrike;

	bool UseGrounding;
	bool UseTremor;
	bool UsePoisonTotem;
	bool UseDiseaseTotem;

	bool UsePurge;
        bool PurgeOnGain;

	bool UseRage;
        double RageMinHealth;
        double RageMaxMana;


	double DPSLife;
	double WhiteDPSLife;
	bool StopDPSOnAggro;

        Shock_e DPSShock; 
	double DPSShockMana;
        bool DPSShockFocus; 
        bool DPSShockStormstrike; 
	bool UseInterruptShock;
        bool ShockRunners       = true; 

	Totem_e CombatFire; 
	Totem_e CombatEarth; 
	Totem_e CombatWater; 
	Totem_e CombatAir; 

	Totem_e AddFire; 
	Totem_e AddEarth; 
	Totem_e AddWater; 
	Totem_e AddAir; 

	Totem_e RestWater; 

	bool UseRecallRange = false;

        ChaseStyle_e ChaseStyle = ChaseStyle_e.ChaseSafe;

	//SkillCondition_e UseThunder; 
	
	bool SpamAlot = true;
	bool NoJumpFromAoE = false;

	bool UseMount = true;
        int MountDistance = 50; 

        ItemUse_e UseItem1 = ItemUse_e.NoUse;
	int UseItem1CD = 60;
	GSpellTimer UseItem1Timer;

        ItemUse_e UseItem2 = ItemUse_e.NoUse;
	int UseItem2CD = 60;
	GSpellTimer UseItem2Timer;

        ItemUse_e UseItem3 = ItemUse_e.NoUse;
	int UseItem3CD = 60;
	GSpellTimer UseItem3Timer;

        ItemUse_e UseItem4 = ItemUse_e.NoUse;
	int UseItem4CD = 60;
	GSpellTimer UseItem4Timer;

        BuffUse_e BuffItem1 = BuffUse_e.NoUse;
        string BuffName1 = "";

        BuffUse_e BuffItem2 = BuffUse_e.NoUse;
        string BuffName2 = "";

        BuffUse_e BuffItem3 = BuffUse_e.NoUse;
        string BuffName3 = "";

	
	bool UseRepair = false;
	bool UseSell   = false;
	string VendorName = "Kaja";
	string[] ProtItems;
	
	bool SellPoor;
	bool SellCommon;
	bool SellUncommon;
	bool SellRare;
		
	int  RestHealth;
	bool UseBandage;
	int  BandageHealth;
	int  HarvestRange;
	bool PickupJunk;

        string PartyLeaderName;
        int PartyFollowerStart;           
        int PartyFollowerStop;

	PvPStyle_e PvPStyle = PvPStyle_e.Active;


        

	////
	// no configuration dialog for the ones below

	double MaxDistanceFromStart = 35; // Prevent to get too far of track in PvP
        bool SkipWaypoints = false; // Enable my special waypoint skipper, run post-combat

        // End configuration

	GSpellTimer AddBackup = new GSpellTimer(4 * 1000);
	GSpellTimer ForceNoMount   = new GSpellTimer(6 * 1000);
	GSpellTimer Feared = new GSpellTimer(30000);
	GSpellTimer CombatTimer = new GSpellTimer(0);	
	GSpellTimer LastKillTimer = new GSpellTimer(0);	
	GSpellTimer SellTimer = new GSpellTimer(10* 60 * 1000, true);	
	GSpellTimer HealCooldown = new GSpellTimer(700, true);	
	GSpellTimer PoisonCooldown = new GSpellTimer(2000, true);	
	GSpellTimer DiseaseCooldown = new GSpellTimer(2000, true);	
	GSpellTimer ShieldCooldown = new GSpellTimer(2000, true);	
        GSpellTimer Enchants = new GSpellTimer(29 * 60 * 1000, true);
        GSpellTimer FSR = new GSpellTimer(5000, true); // mana regen tick start timer
	GSpellTimer PurgeTimer = new GSpellTimer(4 * 1000);
        bool TryPurge = false;

	GSpellTimer PotionTimer = new GSpellTimer(120 * 1000);

	GSpellTimer SeenLeader = new GSpellTimer(3 * 60 * 1000);

	double CombatStartHealth = 0;
	GLocation MyCombatStartLocation;

	double ZMax = 10;

        bool forceMount = false;
	int evades = 0;
        bool sawAnEvade = false;

	bool StandingInAoE = false; // Combat log parser set this flag


        TendencyManager MobTendencies = new TendencyManager();

        SpellCostTracker SpellCost = new SpellCostTracker();

        bool oom = false;
	#endregion



	#region GGameClass overrides
            public override string DisplayName
	{
	    get { return "PShaman " + version ; }
	}

        public override void Startup()
	{
	    Context.Log(DisplayName + " loaded");


            MobTendencies.LoadFromFile();
            Context.Log("Loaded tendency info on " + MobTendencies.GetMobCount() + " mobs");
            
            mover = new Mover(GContext.Main);
            base.Startup();
	}

        public override void Shutdown()
	{
	    Context.Log(DisplayName + " unloaded");
            base.Shutdown();
	}


	public override void OnStartGlide()
	{
	    Context.CombatLog += new GContext.GCombatLogHandler(Context_CombatLog);
	    Context.ChatLog   += new GContext.GChatLogHandler(Context_ChatLog);


            MobTendencies.LoadFromFile();
            Context.Log("Loaded tendency info on " + MobTendencies.GetMobCount() + " mobs");

	    base.OnStartGlide();
            SpellCost.Clear();
	    mover.Stop();


	    Feared.ForceReady();
	    SellTimer.ForceReady();
            oom = false;

	    if(UseSell)
	    {
		Context.Log("Checking bags for what to sell when reaching vendor '" +VendorName+ "'");
		CheckBags(true);
	    }	    
	}


	public override void OnStopGlide()
	{
	    Context.CombatLog -= new GContext.GCombatLogHandler(Context_CombatLog);
	    Context.ChatLog   -= new GContext.GChatLogHandler(Context_ChatLog);
	    mover.Stop();
            MobTendencies.SaveToFile();

	    base.OnStopGlide();
	}


        private void SetConfigValueFromContext(object configDialog, string vKey)
	{
	    PropertyInfo pKey;
	    PropertyInfo pValue;
	    string vValue = Context.GetConfigString(vKey);
	    Type type = configDialog.GetType();
	    //Setup Entry Points to pass Values
	    pKey = type.GetProperty("ConfigKey");
	    pValue = type.GetProperty("ConfigValue");
	    if (pKey != null && pValue != null)
	    {
		pKey.SetValue(configDialog, vKey, null);
		pValue.SetValue(configDialog, vValue, null);
	    }
	}

        //Custom GetConfigValue
        private string GetConfigValue(object configDialog, string vKey)
	{
	    PropertyInfo pKey;
	    PropertyInfo pValue;
	    Type type = configDialog.GetType();
	    //Setup Entry Points to pass Values
	    pKey = type.GetProperty("ConfigKey");
	    pValue = type.GetProperty("ConfigValue");
	    if (pKey != null && pValue != null)
	    {
		pKey.SetValue(configDialog, vKey, null);
		return (pValue.GetValue(configDialog, null)).ToString();
	    }
	    return "";
	}

	private void SetConfigValueFromDialog(object configDialog, string vKey)
	{
	    string val = GetConfigValue(configDialog, vKey);
	    Context.SetConfigValue(vKey, val, true);
	}


        public override void ResetBuffs()
        {
            Enchants.ForceReady();
        }



        WhenCast_e DecodeWhen(String s)
	{
	    switch(s)
	    {
	    case "Never":             return WhenCast_e.Never;
	    case "Resting":           return WhenCast_e.Resting;
	    case "After other cast":  return WhenCast_e.AfterCast;
	    case "Always":            return WhenCast_e.Always;
            }
            return WhenCast_e.Never;
        }

        Totem_e DecodeTotem(String s)
	{
	    switch(s)
	    {
	    case "-":       return Totem_e.None;
	    case "Flametongue Totem": return Totem_e.Flametongue;
	    case "Fire Nova Totem": return Totem_e.FireNova;
	    case "Frost Resistance Totem": return Totem_e.FrostResistance;
	    case "Magma Totem": return Totem_e.Magma;
	    case "Searing Totem": return Totem_e.Searing;
	    case "Fire Elemental Totem": return Totem_e.FireElemental;
	    case "Totem of Wrath": return Totem_e.Wrath;

	    case "Earthbind Totem": return Totem_e.Earthbind;
	    case "Stoneskin Totem": return Totem_e.Stoneskin;
	    case "Stoneclaw Totem": return Totem_e.Stoneclaw;
	    case "Strength of Earth Totem": return Totem_e.Strength;
	    case "Tremor Totem": return Totem_e.Tremor;
	    case "Earth Elemental Totem ": return Totem_e.EarthElemental;

	    case "Fire Resistance Totem": return Totem_e.FireResistance;
	    case "Healing Stream Totem": return Totem_e.HealingStream;
	    case "Mana Spring Totem": return Totem_e.ManaSpring;
	    case "Mana Tide Totem": return Totem_e.ManaTide;
	    case "Poison Cleansing Totem": return Totem_e.PoisonCleansing;
	    case "Disease Cleansing Totem": return Totem_e.DiseaseCleansing;

	    case "Grace of Air Totem": return Totem_e.GraceOfAir;
	    case "Grounding Totem": return Totem_e.Grounding;
	    case "Nature Resistance Totem": return Totem_e.NatureResistance;
	    case "Sentry Totem": return Totem_e.Sentry;
	    case "Tranquil Air Totem": return Totem_e.Tranquil;
	    case "Windfury Totem": return Totem_e.Windfury;
	    case "Windwall Totem": return Totem_e.Windwall;
	    case "Wrath of Air Totem": return Totem_e.WrathOfAir;
	    }
	    Context.Log("Unknown totem '"  + s + "'");
	    return Totem_e.None;
	}

	string TotemKey(Totem_e totem)
	{
	    switch(totem)
	    {
	    case Totem_e.Flametongue: return "PShaman.TotemFlametongue";
	    case Totem_e.FireNova: return "PShaman.TotemFireNova";
	    case Totem_e.FrostResistance: return "PShaman.TotemFrostResistance";
	    case Totem_e.Magma: return "PShaman.TotemMagma";
	    case Totem_e.Searing: return "PShaman.TotemSearing";
	    case Totem_e.FireElemental: return "PShaman.TotemFireElemental";
	    case Totem_e.Wrath:  return "PShaman.TotemWrath";
	    case Totem_e.Earthbind: return "PShaman.TotemEarthbind";
	    case Totem_e.Stoneskin: return "PShaman.TotemStoneskin";
	    case Totem_e.Stoneclaw: return "PShaman.TotemStoneclaw";
	    case Totem_e.Strength: return "PShaman.TotemStrength";
	    case Totem_e.Tremor: return "PShaman.TotemTremor";
	    case Totem_e.EarthElemental: return "PShaman.TotemEarthElemental";
	    case Totem_e.FireResistance: return "PShaman.TotemFireResistance";
	    case Totem_e.HealingStream: return "PShaman.TotemHealingStream";
	    case Totem_e.ManaSpring: return "PShaman.TotemManaSpring";
	    case Totem_e.ManaTide: return "PShaman.TotemManaTide";
	    case Totem_e.PoisonCleansing : return "PShaman.TotemPoisonCleansing ";
	    case Totem_e.DiseaseCleansing: return "PShaman.TotemDiseaseCleansing";
	    case Totem_e.GraceOfAir: return "PShaman.TotemGraceOfAir";
	    case Totem_e.Grounding: return "PShaman.TotemGrounding";
	    case Totem_e.NatureResistance: return "PShaman.TotemNatureResistance";
	    case Totem_e.Sentry: return "PShaman.TotemSentry";
	    case Totem_e.Tranquil: return "PShaman.TotemTranquil";
	    case Totem_e.Windfury: return "PShaman.TotemWindfury";
	    case Totem_e.Windwall: return "PShaman.TotemWindwall";
	    case Totem_e.WrathOfAir: return "PShaman.TotemWrathOfAir";
	    }
	    Context.Log("Asking for key of unknown totem " + totem);
	    return "Common.Jump";
	}



        public override GConfigResult ShowConfiguration()
	{
	    Assembly asm = System.Reflection.Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + "\\Classes\\GConfig2.dll");

	    foreach (Type loadedType in asm.GetTypes())
	    {
		if (loadedType.Name == "GConfig")              
		{
		    PropertyInfo pi;
		    object configDialog = loadedType.GetConstructor(new Type[] { }).Invoke(new object[] { });
		    MethodInfo showDialogMethod = loadedType.GetMethod("ShowDialog", new Type[] { });
		    Type type = configDialog.GetType();

		    // ### ADD MORE
		    SetConfigValueFromContext(configDialog, "PShaman.PullDistance");
		    SetConfigValueFromContext(configDialog, "PShaman.PullMethod");

		    SetConfigValueFromContext(configDialog, "PShaman.WaitTime");
		    SetConfigValueFromContext(configDialog, "PShaman.TotemsBeforePull");

		    SetConfigValueFromContext(configDialog, "PShaman.BoltDistance");
		    SetConfigValueFromContext(configDialog, "PShaman.BoltOnFocused");
		    SetConfigValueFromContext(configDialog, "PShaman.BoltSpam");

		    SetConfigValueFromContext(configDialog, "PShaman.HealMana");
		    SetConfigValueFromContext(configDialog, "PShaman.HealPanicLife");
		    SetConfigValueFromContext(configDialog, "PShaman.HealLesserWaveLife");
		    SetConfigValueFromContext(configDialog, "PShaman.HealWaveLife");
		    SetConfigValueFromContext(configDialog, "PShaman.HealChainLife");
		    SetConfigValueFromContext(configDialog, "PShaman.RestHealWaveLife");
		    SetConfigValueFromContext(configDialog, "PShaman.RestHealChainLife");

		    SetConfigValueFromContext(configDialog, "PShaman.Shield");
		    SetConfigValueFromContext(configDialog, "PShaman.WhenShield");

		    SetConfigValueFromContext(configDialog, "PShaman.WhenPoison");
		    SetConfigValueFromContext(configDialog, "PShaman.WhenDisease");

		    SetConfigValueFromContext(configDialog, "PShaman.WhiteDPSLife");
		    SetConfigValueFromContext(configDialog, "PShaman.DPSLife");
		    SetConfigValueFromContext(configDialog, "PShaman.StopDPSOnAggro");

		    SetConfigValueFromContext(configDialog, "PShaman.DPSShock");
		    SetConfigValueFromContext(configDialog, "PShaman.DPSShockMana");
		    SetConfigValueFromContext(configDialog, "PShaman.DPSShockFocus");
		    SetConfigValueFromContext(configDialog, "PShaman.DPSShockStormstrike");
		    SetConfigValueFromContext(configDialog, "PShaman.UseInterruptShock");
		    SetConfigValueFromContext(configDialog, "PShaman.ShockRunners");

		    SetConfigValueFromContext(configDialog, "PShaman.UseMount");
		    SetConfigValueFromContext(configDialog, "PShaman.MountDistance");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem1");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem2");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem3");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem4");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem1CD");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem2CD");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem3CD");
		    SetConfigValueFromContext(configDialog, "PShaman.UseItem4CD");

                    SetConfigValueFromContext(configDialog, "PShaman.BuffItem1");
                    SetConfigValueFromContext(configDialog, "PShaman.BuffName1");
                    SetConfigValueFromContext(configDialog, "PShaman.BuffItem2");
                    SetConfigValueFromContext(configDialog, "PShaman.BuffName2");
                    SetConfigValueFromContext(configDialog, "PShaman.BuffItem3");
                    SetConfigValueFromContext(configDialog, "PShaman.BuffName3");

		    SetConfigValueFromContext(configDialog, "PShaman.RunnerAction");

		    SetConfigValueFromContext(configDialog, "PShaman.UseStormstrike");

		    SetConfigValueFromContext(configDialog, "PShaman.UseGrounding");
		    SetConfigValueFromContext(configDialog, "PShaman.UseTremor");
		    SetConfigValueFromContext(configDialog, "PShaman.UsePoisonTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.UseDiseaseTotem");

		    SetConfigValueFromContext(configDialog, "PShaman.UsePurge");
		    SetConfigValueFromContext(configDialog, "PShaman.PurgeOnGain");

		    SetConfigValueFromContext(configDialog, "PShaman.UseRage");
		    SetConfigValueFromContext(configDialog, "PShaman.RageMinHealth");
		    SetConfigValueFromContext(configDialog, "PShaman.RageMaxMana");

		    SetConfigValueFromContext(configDialog, "PShaman.AvoidAddDist");

		    SetConfigValueFromContext(configDialog, "PShaman.SpamAlot");
		    SetConfigValueFromContext(configDialog, "PShaman.NoJumpFromAoE");

		    SetConfigValueFromContext(configDialog, "PShaman.CombatFireTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.CombatEarthTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.CombatAirTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.CombatWaterTotem");

		    SetConfigValueFromContext(configDialog, "PShaman.AddFireTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.AddEarthTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.AddAirTotem");
		    SetConfigValueFromContext(configDialog, "PShaman.AddWaterTotem");

		    SetConfigValueFromContext(configDialog, "PShaman.RestWaterTotem");

		    SetConfigValueFromContext(configDialog, "PShaman.UseRecallRange");

		    SetConfigValueFromContext(configDialog, "PShaman.UseRepair");
		    SetConfigValueFromContext(configDialog, "PShaman.UseSell");
		    SetConfigValueFromContext(configDialog, "PShaman.VendorName");

		    SetConfigValueFromContext(configDialog, "PShaman.PvPStyle");
		    
		    SetConfigValueFromContext(configDialog, "PShaman.SellPoor");
		    SetConfigValueFromContext(configDialog, "PShaman.SellCommon");
		    SetConfigValueFromContext(configDialog, "PShaman.SellUncommon");
		    SetConfigValueFromContext(configDialog, "PShaman.SellRare");

		    for(int i = 0; i<20; i++)
		    {
			string name = String.Format("PShaman.Protected{0}", i);
			SetConfigValueFromContext(configDialog, name);
		    }

		    //Set Config File
		    pi = type.GetProperty("ConfigXML");
		    if (pi != null) pi.SetValue(configDialog, "PShaman.XML", null);

		    //Popup Dialog
		    object modalResult = showDialogMethod.Invoke(configDialog, new object[] { });
		    if ((int)modalResult == 1)
		    {
			//Get Current Values

			SetConfigValueFromDialog(configDialog, "PShaman.PullDistance");
			SetConfigValueFromDialog(configDialog, "PShaman.PullMethod");

			SetConfigValueFromDialog(configDialog, "PShaman.WaitTime");
			SetConfigValueFromDialog(configDialog, "PShaman.TotemsBeforePull");

			SetConfigValueFromDialog(configDialog, "PShaman.BoltDistance");
			SetConfigValueFromDialog(configDialog, "PShaman.BoltOnFocused");
			SetConfigValueFromDialog(configDialog, "PShaman.BoltSpam");

			SetConfigValueFromDialog(configDialog, "PShaman.HealMana");
			SetConfigValueFromDialog(configDialog, "PShaman.HealPanicLife");
			SetConfigValueFromDialog(configDialog, "PShaman.HealLesserWaveLife");
			SetConfigValueFromDialog(configDialog, "PShaman.HealWaveLife");
			SetConfigValueFromDialog(configDialog, "PShaman.HealChainLife");
			SetConfigValueFromDialog(configDialog, "PShaman.RestHealWaveLife");
			SetConfigValueFromDialog(configDialog, "PShaman.RestHealChainLife");

			SetConfigValueFromDialog(configDialog, "PShaman.Shield");
			SetConfigValueFromDialog(configDialog, "PShaman.WhenShield");

			SetConfigValueFromDialog(configDialog, "PShaman.WhenPoison");
			SetConfigValueFromDialog(configDialog, "PShaman.WhenDisease");

			SetConfigValueFromDialog(configDialog, "PShaman.WhiteDPSLife");
			SetConfigValueFromDialog(configDialog, "PShaman.DPSLife");
			SetConfigValueFromDialog(configDialog, "PShaman.StopDPSOnAggro");

                        SetConfigValueFromDialog(configDialog, "PShaman.DPSShock");
			SetConfigValueFromDialog(configDialog, "PShaman.DPSShockMana");
                        SetConfigValueFromDialog(configDialog, "PShaman.DPSShockFocus");
                        SetConfigValueFromDialog(configDialog, "PShaman.DPSShockStormstrike");
                        SetConfigValueFromDialog(configDialog, "PShaman.UseInterruptShock");
                        SetConfigValueFromDialog(configDialog, "PShaman.ShockRunners");


			SetConfigValueFromDialog(configDialog, "PShaman.UseMount");
			SetConfigValueFromDialog(configDialog, "PShaman.MountDistance");

			SetConfigValueFromDialog(configDialog, "PShaman.UseItem1");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem2");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem3");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem4");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem1CD");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem2CD");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem3CD");
			SetConfigValueFromDialog(configDialog, "PShaman.UseItem4CD");

                        SetConfigValueFromDialog(configDialog, "PShaman.BuffItem1");
                        SetConfigValueFromDialog(configDialog, "PShaman.BuffName1");
                        SetConfigValueFromDialog(configDialog, "PShaman.BuffItem2");
                        SetConfigValueFromDialog(configDialog, "PShaman.BuffName2");
                        SetConfigValueFromDialog(configDialog, "PShaman.BuffItem3");
                        SetConfigValueFromDialog(configDialog, "PShaman.BuffName3");


			SetConfigValueFromDialog(configDialog, "PShaman.RunnerAction");

			SetConfigValueFromDialog(configDialog, "PShaman.UseStormstrike");

			SetConfigValueFromDialog(configDialog, "PShaman.UseGrounding");
			SetConfigValueFromDialog(configDialog, "PShaman.UseTremor");
			SetConfigValueFromDialog(configDialog, "PShaman.UsePoisonTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.UseDiseaseTotem");

			SetConfigValueFromDialog(configDialog, "PShaman.UsePurge");
			SetConfigValueFromDialog(configDialog, "PShaman.PurgeOnGain");

			SetConfigValueFromDialog(configDialog, "PShaman.UseRage");
			SetConfigValueFromDialog(configDialog, "PShaman.RageMinHealth");
			SetConfigValueFromDialog(configDialog, "PShaman.RageMaxMana");

			SetConfigValueFromDialog(configDialog, "PShaman.AvoidAddDist");

			SetConfigValueFromDialog(configDialog, "PShaman.SpamAlot");
			SetConfigValueFromDialog(configDialog, "PShaman.NoJumpFromAoE");



			SetConfigValueFromDialog(configDialog, "PShaman.CombatFireTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.CombatEarthTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.CombatAirTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.CombatWaterTotem");


			SetConfigValueFromDialog(configDialog, "PShaman.AddFireTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.AddEarthTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.AddAirTotem");
			SetConfigValueFromDialog(configDialog, "PShaman.AddWaterTotem");

			SetConfigValueFromDialog(configDialog, "PShaman.RestWaterTotem");


			SetConfigValueFromDialog(configDialog, "PShaman.PvPStyle");



			SetConfigValueFromDialog(configDialog, "PShaman.UseRecallRange");

			SetConfigValueFromDialog(configDialog, "PShaman.UseRepair");
			SetConfigValueFromDialog(configDialog, "PShaman.UseSell");
			SetConfigValueFromDialog(configDialog, "PShaman.VendorName");

			SetConfigValueFromDialog(configDialog, "PShaman.SellPoor");
			SetConfigValueFromDialog(configDialog, "PShaman.SellCommon");
			SetConfigValueFromDialog(configDialog, "PShaman.SellUncommon");
			SetConfigValueFromDialog(configDialog, "PShaman.SellRare");

			for(int i = 0; i<20; i++)
			{
			    string name = String.Format("PShaman.Protected{0}", i);
			    SetConfigValueFromDialog(configDialog, name);
			}
			
			return GConfigResult.Accept;
		    }
		    return GConfigResult.Cancel;
		}
	    }
	    return GConfigResult.Cancel;

	    //return Context.ShowStockConfigDialog(GPlayerClass.PShaman);
	}


        public override int PullDistance { get { return MyPullDistance; } }

#region UpdateKeys method
        public override void UpdateKeys(GKey[] UpdatableKeys)
        {
            foreach (GKey One in UpdatableKeys)
            {
                GShortcut button = null;
                switch (One.KeyName)
                {
                    case "PShaman.CureDisease":
                        button = GShortcut.FindMatchingSpellGroup("0xB36");
                        break;
                    case "PShaman.CurePoison":
                        button = GShortcut.FindMatchingSpellGroup("0x20e");
                        break;
                    case "PShaman.ChainHeal":
                    case "PShaman.ChainHealSelf":
                        button = GShortcut.FindMatchingSpellGroup("0x428");
                        break;
                    case "PShaman.TotemFlametongue":
                        button = GShortcut.FindMatchingSpellGroup("0x2023");
                        break;
                    case "PShaman.TotemFrostResistance":
                        button = GShortcut.FindMatchingSpellGroup("0x1FF5");
                        break;
                    case "PShaman.TotemMagma":
                        button = GShortcut.FindMatchingSpellGroup("0x1FFE");
                        break;
                    case "PShaman.TotemFireElemental":
                        button = GShortcut.FindMatchingSpellGroup("0xB4E");
                        break;
                    case "PShaman.TotemWrath":
                        button = GShortcut.FindMatchingSpellGroup("0x77F2");
                        break;
                    case "PShaman.TotemStoneskin":
                        button = GShortcut.FindMatchingSpellGroup("0x1F87");
                        break;
                    case "PShaman.TotemEarthElemental":
                        button = GShortcut.FindMatchingSpellGroup("0x80E");
                        break;
                    case "PShaman.TotemFireResistance":
                        button = GShortcut.FindMatchingSpellGroup("0x1FF8");
                        break;
                    case "PShaman.TotemManaSpring":
                        button = GShortcut.FindMatchingSpellGroup("0x162B");
                        break;
                    case "PShaman.TotemManaTide":
                        button = GShortcut.FindMatchingSpellGroup("0x3F3E");
                        break;
                    case "PShaman.TotemDiseaseCleansing":
                        button = GShortcut.FindMatchingSpellGroup("0x1FEA");
                        break;
                    case "PShaman.TotemGraceOfAir":
                        button = GShortcut.FindMatchingSpellGroup("0x2283");
                        break;
                    case "PShaman.TotemGrounding":
                        button = GShortcut.FindMatchingSpellGroup("0x1FF1");
                        break;
                    case "PShaman.TotemNatureResistance":
                        button = GShortcut.FindMatchingSpellGroup("0x2963");
                        break;
                    case "PShaman.TotemSentry":
                        button = GShortcut.FindMatchingSpellGroup("0x195F");
                        break;
                    case "PShaman.TotemTranquil":
                        button = GShortcut.FindMatchingSpellGroup("0x6534");
                        break;
                    case "PShaman.TotemWindfury":
                        button = GShortcut.FindMatchingSpellGroup("0x2140");
                        break;
                    case "PShaman.TotemWindwall":
                        button = GShortcut.FindMatchingSpellGroup("0x3B03");
                        break;
                    case "PShaman.TotemWrathOfAir":
                        button = GShortcut.FindMatchingSpellGroup("0xE9A");
                        break;
                    case "PShaman.DPSShock":
                        button = GShortcut.FindMatchingSpellGroup("0x28ae");
                        break;
                    case "PShaman.EarthShock":
                        button = GShortcut.FindMatchingSpellGroup("0x28ae");
                        break;
                    case "PShaman.FlameShock":
                        button = GShortcut.FindMatchingSpellGroup("0x722c");
                        break;
                    case "PShaman.FrostShock":
                        button = GShortcut.FindMatchingSpellGroup("0x28e9");
                        break;
                    case "PShaman.HealingWave":
                        button = GShortcut.FindMatchingSpellGroup("0x632f");
                        break;
                    case "PShaman.HealingWaveSelf":
                        button = GShortcut.FindMatchingSpellGroup("0x632f");
                        break;
                    case "PShaman.InterruptShock":
                        button = GShortcut.FindMatchingSpellGroup("0x1f6a");
                        break;
                    case "PShaman.LesserHealingWave":
                        button = GShortcut.FindMatchingSpellGroup("0x28e4");
                        break;
                    case "PShaman.LesserHealingWaveSelf":
                        button = GShortcut.FindMatchingSpellGroup("0x28e4");
                        break;
                    case "PShaman.LightningBolt":
                        button = GShortcut.FindMatchingSpellGroup("0x193");
                        break;
					case "PShaman.TotemicCall":
                        button = GShortcut.FindMatchingSpellGroup("0x9048");
                        break;
                    case "PShaman.LightningShield":
                        button = GShortcut.FindMatchingSpellGroup("0x637d");
                        break;
                    case "PShaman.MainWeaponEnchant":
                        button = GShortcut.FindMatchingSpellGroup("0x2028 0x1f61 0x1f58 0x1f51");
                        break;
					case "PShaman.OffWeaponEnchant":
                        button = GShortcut.FindMatchingSpellGroup("0x2028 0x1f61 0x1f58 0x1f51");
                        break;
					case "PShaman.Mount":
                    string mountId = "21176 33809 32458 38576 29465 18243 13328 18247 29466 29467 18244 18241 29468 32319 33999 13335 30480 13329 29745 18794 18795 29746 29747 18793 15292 32314 13334 12351 18245 29469 19029 12330 18796 18798 18797 13327 34092 12354 32316 18791 32317 29470 18248 32858 35906 29471 18242 32859 31829 31830 29102 29227 28915 29228 12302 12303 32857 32860 32768 31831 31832 29229 29104 18766 18767 33225 18902 31833 31834 29230 29105 32861 32862 31835 31836 29231 29103 13086 32318 19030 25473 18788 33977 18786 18777 33182 18787 25528 29223 18772 25531 33184 30609 18789 18790 18776 28936 25529 29224 25533 19872 25527 25477 34129 35513 18773 18785 18778 18774 25532 19902 15293 37012 37676 29472 18246 13317 8586 13326 12353 35226 29221 2411 29220 8595 21218 13332 25475 33976 28481 5656 15290 5872 13333 5655 25471 33176 25470 29744 15277 5864 13321 21323 25476 5668 5665 1132 37011 33183 2414 29743 29222 28927 8563 21321 13331 33224 8632 8631 8629 25472 25474 13322 8588 8591 8592 5873 35225 21324 8589 37719";
                    button = GShortcut.FindMatchingShortcut(GShortcutType.Item, mountId);
                    break;
                    case "PShaman.NS":
                        button = GShortcut.FindMatchingSpellGroup("0x3F3C");
                        break;
                    case "PShaman.Purge":
                        button = GShortcut.FindMatchingSpellGroup("0x1f4c");
                        break;
                    case "PShaman.Rage":
                        button = GShortcut.FindMatchingSpellGroup("0x7867");
                        break;
                    case "PShaman.RunnerShock":
                        button = GShortcut.FindMatchingSpellGroup("0x28e9");
                        break;
                    case "PShaman.Stormstrike":
                        button = GShortcut.FindMatchingSpellGroup("0x43d4");
                        break;
                    case "PShaman.TotemEarthbind":
                        button = GShortcut.FindMatchingSpellGroup("0x9b4");
                        break;
                    case "PShaman.TotemFireNova":
                        button = GShortcut.FindMatchingSpellGroup("0x63ca");
                        break;
                    case "PShaman.TotemHealingStream":
                        button = GShortcut.FindMatchingSpellGroup("0x28df");
                        break;
                    case "PShaman.TotemPoisonCleansing":
                        button = GShortcut.FindMatchingSpellGroup("0x1fe6");
                        break;
                    case "PShaman.TotemSearing":
                        button = GShortcut.FindMatchingSpellGroup("0x28c6");
                        break;
                    case "PShaman.TotemStoneclaw":
                        button = GShortcut.FindMatchingSpellGroup("0x28bc");
                        break;
                    case "PShaman.TotemStrength":
                        button = GShortcut.FindMatchingSpellGroup("0x6311");
                        break;
                    case "PShaman.TotemTremor":
                        button = GShortcut.FindMatchingSpellGroup("0x1fcf");
                        break;
                    case "PShaman.WaterShield":
                        button = GShortcut.FindMatchingSpellGroup("0x5f4e");
                        break;
					case "PShaman.EarthShield":
                        button = GShortcut.FindMatchingSpellGroup("0x7f52");
                        break;
                    default:
                        continue;  // Don't try to handle this, we didn't do anything smart.
                }
                if (button != null)
                {
                    if (One.KeyName.ToUpper().Contains("SELF"))
                    {
                        One.ShiftState = GKey.SS_ALT;
                    }
                    Context.Debug("Mapped " + One.KeyName + " -> " + button.ToString());
                    One.SIM = button.ShortcutValue;
                }
                else
                    Context.Log("Unable to find suitable button for " + One.KeyName + ", see help section \"Re-Assigning Keys\" under Key Mappings.");
            }
        }
        #endregion
        public override void CreateDefaultConfig()
	{
        Context.AddAutoKey("PShaman.LightningBolt");
        Context.AddAutoKey("PShaman.ChainHeal");
        Context.AddAutoKey("PShaman.ChainHealSelf");
        Context.AddAutoKey("PShaman.TotemFlametongue");
        Context.AddAutoKey("PShaman.TotemFrostResistance");
        Context.AddAutoKey("PShaman.TotemMagma");
        Context.AddAutoKey("PShaman.TotemFireElemental");
        Context.AddAutoKey("PShaman.TotemWrath");
        Context.AddAutoKey("PShaman.TotemStoneskin");
        Context.AddAutoKey("PShaman.TotemEarthElemental");
        Context.AddAutoKey("PShaman.TotemFireResistance");
        Context.AddAutoKey("PShaman.TotemManaSpring");
        Context.AddAutoKey("PShaman.TotemManaTide");
        Context.AddAutoKey("PShaman.TotemDiseaseCleansing");
        Context.AddAutoKey("PShaman.TotemGraceOfAir");
        Context.AddAutoKey("PShaman.TotemGrounding");
        Context.AddAutoKey("PShaman.TotemNatureResistance");
        Context.AddAutoKey("PShaman.TotemSentry");
        Context.AddAutoKey("PShaman.TotemTranquil");
        Context.AddAutoKey("PShaman.TotemWindfury");
        Context.AddAutoKey("PShaman.TotemWindwall");
        Context.AddAutoKey("PShaman.TotemWrathOfAir");
        Context.AddAutoKey("PShaman.InterruptShock");
        Context.AddAutoKey("PShaman.FrostShock");
        Context.AddAutoKey("PShaman.RunnerShock");
        Context.AddAutoKey("PShaman.DPSShock");
        Context.AddAutoKey("PShaman.EarthShock");
        Context.AddAutoKey("PShaman.FlameShock");
        Context.AddAutoKey("PShaman.Purge");
        Context.AddAutoKey("PShaman.MainWeaponEnchant");
        Context.AddAutoKey("PShaman.OffWeaponEnchant");
        Context.AddAutoKey("PShaman.HealingWave");
        Context.AddAutoKey("PShaman.HealingWaveSelf");
        Context.AddAutoKey("PShaman.LesserHealingWave");
        Context.AddAutoKey("PShaman.LesserHealingWaveSelf");
        Context.AddAutoKey("PShaman.LightningShield");
        Context.AddAutoKey("PShaman.NS");
        Context.AddAutoKey("PShaman.UseItem1");
        Context.AddAutoKey("PShaman.UseItem2");
        Context.AddAutoKey("PShaman.UseItem3");
        Context.AddAutoKey("PShaman.UseItem4");
        Context.AddAutoKey("PShaman.BuffUsable1");
        Context.AddAutoKey("PShaman.BuffUsable2");
        Context.AddAutoKey("PShaman.TotemFireNova");
        Context.AddAutoKey("PShaman.TotemSearing");
        Context.AddAutoKey("PShaman.TotemEarthbind");
        Context.AddAutoKey("PShaman.TotemStoneclaw");
        Context.AddAutoKey("PShaman.TotemStrength");
        Context.AddAutoKey("PShaman.TotemTremor");
        Context.AddAutoKey("PShaman.TotemHealingStream");
        Context.AddAutoKey("PShaman.CurePoison");
        Context.AddAutoKey("PShaman.TotemPoisonCleansing ");
        Context.AddAutoKey("PShaman.CureDisease");
        Context.AddAutoKey("PShaman.TotemicCall");
        Context.AddAutoKey("PShaman.Stormstrike");
        Context.AddAutoKey("PShaman.Rage");
        Context.AddAutoKey("PShaman.Mount");
        Context.AddAutoKey("PShaman.WaterShield");
		Context.AddAutoKey("PShaman.EarthShield");
	    Context.SetConfigValue("PShaman.PullDistance", "30", false);
	    Context.SetConfigValue("PShaman.PullMethod", "Lightning Bolt", false);


	    Context.SetConfigValue("PShaman.WaitTime", "6", false);
	    Context.SetConfigValue("PShaman.TotemsBeforePull", "false", false);

	    Context.SetConfigValue("PShaman.BoltDistance", "15", false);
	    Context.SetConfigValue("PShaman.BoltOnFocused", "True", false);
	    Context.SetConfigValue("PShaman.BoltSpam", "False", false);

	    Context.SetConfigValue("PShaman.HealMana", "30", false);
	    Context.SetConfigValue("PShaman.HealPanicLife", "25", false);
	    Context.SetConfigValue("PShaman.HealLesserWaveLife", "35", false);
	    Context.SetConfigValue("PShaman.HealWaveLife", "60", false);
	    Context.SetConfigValue("PShaman.HealChainLife", "75", false);

	    Context.SetConfigValue("PShaman.RestHealWaveLife", "70", false);
	    Context.SetConfigValue("PShaman.RestHealChainLife", "75", false);


	    Context.SetConfigValue("PShaman.Shield", "None", false);
	    Context.SetConfigValue("PShaman.WhenShield", "After other cast", false);

	    Context.SetConfigValue("PShaman.WhenPoison", "After other cast", false);
	    Context.SetConfigValue("PShaman.WhenDisease", "After other cast", false);

	    Context.SetConfigValue("PShaman.WhiteDPSLife", "90", false);
	    Context.SetConfigValue("PShaman.DPSLife", "80", false);
	    Context.SetConfigValue("PShaman.StopDPSOnAggro", "True", false);

	    Context.SetConfigValue("PShaman.DPSShock", "Earth", false);
	    Context.SetConfigValue("PShaman.DPSShockMana", "80", false);
	    Context.SetConfigValue("PShaman.DPSShockFocus", "True", false);
	    Context.SetConfigValue("PShaman.DPSShockStormstrike", "True", false);
	    Context.SetConfigValue("PShaman.UseInterruptShock", "True", false);
	    Context.SetConfigValue("PShaman.ShockRunners", "True", false);



	    Context.SetConfigValue("PShaman.UseMount", "False", false);
	    Context.SetConfigValue("PShaman.MountDistance", "45", false);
	    Context.SetConfigValue("PShaman.UseItem1", "Do not use", false);
	    Context.SetConfigValue("PShaman.UseItem2", "Do not use", false);
	    Context.SetConfigValue("PShaman.UseItem3", "Do not use", false);
	    Context.SetConfigValue("PShaman.UseItem4", "Do not use", false);
	    Context.SetConfigValue("PShaman.UseItem1CD", "120", false);
	    Context.SetConfigValue("PShaman.UseItem2CD", "120", false);
	    Context.SetConfigValue("PShaman.UseItem3CD", "120", false);
	    Context.SetConfigValue("PShaman.UseItem4CD", "120", false);	   

            Context.SetConfigValue("PShaman.BuffItem1", "Do not use", false);
            Context.SetConfigValue("PShaman.BuffName1", "", false);
            Context.SetConfigValue("PShaman.BuffItem2", "Do not use", false);
            Context.SetConfigValue("PShaman.BuffName2", "", false);
            Context.SetConfigValue("PShaman.BuffItem3", "Do not use", false);
            Context.SetConfigValue("PShaman.BuffName3", "", false);

	    Context.SetConfigValue("PShaman.RunnerAction", "Chase while it is safe", false);

	    Context.SetConfigValue("PShaman.UseStormstrike", "False", false);

	    Context.SetConfigValue("PShaman.UseGrounding", "False", false);
	    Context.SetConfigValue("PShaman.UseTremor", "False", false);

	    Context.SetConfigValue("PShaman.UsePoisonTotem", "False", false);
	    Context.SetConfigValue("PShaman.UseDiseaseTotem", "False", false);

	    Context.SetConfigValue("PShaman.UsePurge", "False", false);
	    Context.SetConfigValue("PShaman.PurgeOnGain", "True", false);

	    Context.SetConfigValue("PShaman.UseRage", "False", false);
	    Context.SetConfigValue("PShaman.RageMinHealth", "75", false);
	    Context.SetConfigValue("PShaman.RageMaxMana", "55", false);


	    Context.SetConfigValue("PShaman.AvoidAddDist", "19", false);

	    Context.SetConfigValue("PShaman.SpamAlot", "True", false);
	    Context.SetConfigValue("PShaman.NoJumpFromAoE", "False", false);

	    Context.SetConfigValue("PShaman.UseRecallRange", "False", false);

	    Context.SetConfigValue("PShaman.UseSell", "False", false);
	    Context.SetConfigValue("PShaman.UseRepair", "False", false);

	    Context.SetConfigValue("PShaman.VendorName", "Kaja", false);

	    Context.SetConfigValue("PShaman.SellPoor", "True", false);
	    Context.SetConfigValue("PShaman.SellCommon", "False", false);
	    Context.SetConfigValue("PShaman.SellUncommon", "False", false);
	    Context.SetConfigValue("PShaman.SellRare", "False", false);


	    Context.SetConfigValue("PShaman.CombatFireTotem", "-", false);
	    Context.SetConfigValue("PShaman.CombatEarthTotem", "-", false);
	    Context.SetConfigValue("PShaman.CombatAirTotem", "-", false);
	    Context.SetConfigValue("PShaman.CombatWaterTotem", "-", false);


	    Context.SetConfigValue("PShaman.AddFireTotem", "-", false);
	    Context.SetConfigValue("PShaman.AddEarthTotem", "-", false);
	    Context.SetConfigValue("PShaman.AddAirTotem", "-", false);
	    Context.SetConfigValue("PShaman.AddWaterTotem", "-", false);

	    Context.SetConfigValue("PShaman.RestWaterTotem", "-", false);

	    Context.SetConfigValue("PShaman.PvPStyle", "Fight back", false);


	    Context.SetConfigValue("PShaman.Protected0", "bandage", false);
	    Context.SetConfigValue("PShaman.Protected1", "healing potion", false);
	    Context.SetConfigValue("PShaman.Protected2", "skinning knife", false);

	    for(int i = 3; i<20; i++)
	    {
		string name = String.Format("PShaman.Protected{0}", i);
		Context.SetConfigValue(name, "", false);
	    }

	}

        private bool SkillConditionEval(SkillCondition_e cond)
	{
	    if(cond == SkillCondition_e.Never) return false;
	    if(cond == SkillCondition_e.Always) return true;
	    if(cond == SkillCondition_e.Mob50HP && Target != null) return Target.Health >= 0.5;
	    if(cond == SkillCondition_e.CloseAdd) return HaveCloseAdd;
	    if(cond == SkillCondition_e.Add) return HaveAdd;
	    if(cond == SkillCondition_e.Silence && Target != null) return Target.IsCasting;
	    return false;
	}

        private bool EvalWhenCast(WhenCast_e cond)
        {
            if(cond == WhenCast_e.Never) return false;
            if(cond == WhenCast_e.Always) return true;
            if(cond == WhenCast_e.Resting && Resting) return true;
            if(cond == WhenCast_e.AfterCast && FSR.TicksLeft > 2000) return true;
            return false;
        }

        private SkillCondition_e DecodeConditionString(String s)
	{
	    switch(s)
	    {
            case "True":       return SkillCondition_e.Always;
            case "Always":     return SkillCondition_e.Always;
            case "Yes":        return SkillCondition_e.Always;

            case "False":      return SkillCondition_e.Never;
            case "-":          return SkillCondition_e.Never;
            case "Never":      return SkillCondition_e.Never;
            case "No":         return SkillCondition_e.Never;

            case "When close add":   return SkillCondition_e.CloseAdd;
            case "Close add":        return SkillCondition_e.CloseAdd;

            case "Save for adds":        return SkillCondition_e.Add;

            case "Target has high HP":   return SkillCondition_e.Mob50HP;

            case "Interrupt spellcast":   return SkillCondition_e.Silence;
		   
	    }	   
	    return SkillCondition_e.Never;
	}

        PullMethod_e DecodePullMethod(String s)
	{
	    switch(s)
	    {
	    case "Lightning Bolt":   return PullMethod_e.Bolt; 
	    case "Shock":            return PullMethod_e.Shock; 
	    case "Walk up":          return PullMethod_e.WalkUp; 
	    }
	    return PullMethod_e.Bolt;
	}

        public override void LoadConfig()
	{
	    // Some glider settings
	    RestHealth = Context.GetConfigInt("RestHealth");
	    UseBandage = Context.GetConfigBool("UseBandages");
	    BandageHealth = Context.GetConfigInt("BandageHealth");
	    HarvestRange = Context.GetConfigInt("HarvestRange");

	    PickupJunk = Context.GetConfigBool("PickupJunk");
	    
            PartyLeaderName    = Context.GetConfigString("PartyLeaderName");
            PartyFollowerStart = Context.GetConfigInt("PartyFollowerStart");
            PartyFollowerStop  = Context.GetConfigInt("PartyFollowerStop");
            

	    MyPullDistance     = Context.GetConfigInt("PShaman.PullDistance");
	    PullMethod         = DecodePullMethod(Context.GetConfigString("PShaman.PullMethod"));

	    WaitTime           = Context.GetConfigInt("PShaman.WaitTime");
	    TotemsBeforePull   = Context.GetConfigBool("PShaman.TotemsBeforePull");

	    BoltDistance    = Context.GetConfigInt("PShaman.BoltDistance");
	    BoltOnFocused   = Context.GetConfigBool("PShaman.BoltOnFocused");
	    BoltSpam        = Context.GetConfigBool("PShaman.BoltSpam");


	    HealMana           = Context.GetConfigInt("PShaman.HealMana") / 100.0;
	    HealPanicLife      = Context.GetConfigInt("PShaman.HealPanicLife") / 100.0;
	    HealLesserWaveLife = Context.GetConfigInt("PShaman.HealLesserWaveLife") / 100.0;
	    HealWaveLife       = Context.GetConfigInt("PShaman.HealWaveLife") / 100.0;
	    HealChainLife      = Context.GetConfigInt("PShaman.HealChainLife") / 100.0;
	    RestHealWaveLife       = Context.GetConfigInt("PShaman.RestHealWaveLife") / 100.0;
	    RestHealChainLife      = Context.GetConfigInt("PShaman.RestHealChainLife") / 100.0;


	    string chs     = Context.GetConfigString("PShaman.Shield");
	    switch(chs)
	    {
	    case "None":        Shield = Shield_e.None;break; 
	    case "Lightning":   Shield = Shield_e.Lightning;break; 
	    case "Water":       Shield = Shield_e.Water; break; 
            }            

            string wsh = Context.GetConfigString("PShaman.WhenShield");
            WhenShield = DecodeWhen(wsh);

            string wcp = Context.GetConfigString("PShaman.WhenPoison");
            WhenPoison = DecodeWhen(wcp);

            string wcd = Context.GetConfigString("PShaman.WhenDisease");
            WhenDisease = DecodeWhen(wcd);

	    WhiteDPSLife    = Context.GetConfigInt("PShaman.WhiteDPSLife") / 100.0;
	    DPSLife         = Context.GetConfigInt("PShaman.DPSLife") / 100.0;
	    StopDPSOnAggro  = Context.GetConfigBool("PShaman.StopDPSOnAggro");

            String shocks = Context.GetConfigString("PShaman.DPSShock");
            DPSShock = Shock_e.None;
            if(shocks == "Earth") DPSShock = Shock_e.Earth;
            if(shocks == "Frost") DPSShock = Shock_e.Frost;
            if(shocks == "Flame")  DPSShock = Shock_e.Flame;
	    DPSShockMana    = Context.GetConfigInt("PShaman.DPSShockMana") / 100.0;

	    DPSShockFocus        = Context.GetConfigBool("PShaman.DPSShockFocus");
	    DPSShockStormstrike  = Context.GetConfigBool("PShaman.DPSShockStormstrike");            
	    UseInterruptShock  = Context.GetConfigBool("PShaman.UseInterruptShock");
	    ShockRunners       = Context.GetConfigBool("PShaman.ShockRunners");
	    

	    UseMount        = Context.GetConfigBool("PShaman.UseMount");
	    MountDistance   = Context.GetConfigInt("PShaman.MountDistance");


	    string cs = Context.GetConfigString("PShaman.RunnerAction");
	    switch(cs)
	    {
	    case "Chase":                   ChaseStyle = ChaseStyle_e.Chase; break; 
	    case "Chase while it is safe":  ChaseStyle = ChaseStyle_e.ChaseSafe; break; 
	    case "Do not chase":            ChaseStyle = ChaseStyle_e.NoChase; break; 
	    }


	    UseStormstrike  = Context.GetConfigBool("PShaman.UseStormstrike");

	    UseGrounding  = Context.GetConfigBool("PShaman.UseGrounding");
	    UseTremor     = Context.GetConfigBool("PShaman.UseTremor");

	    UsePoisonTotem     = Context.GetConfigBool("PShaman.UsePoisonTotem");
	    UseDiseaseTotem     = Context.GetConfigBool("PShaman.UseDiseaseTotem");

	    UsePurge     = Context.GetConfigBool("PShaman.UsePurge");
	    PurgeOnGain  = Context.GetConfigBool("PShaman.PurgeOnGain");

	    UseRage        = Context.GetConfigBool("PShaman.UseRage");
	    RageMinHealth  = Context.GetConfigInt("PShaman.RageMinHealth") / 100.0;
	    RageMaxMana    = Context.GetConfigInt("PShaman.RageMaxMana") / 100.0;

	    AvoidAddDistance   = Context.GetConfigInt("PShaman.AvoidAddDist");
		

	    UseRecallRange  = Context.GetConfigBool("PShaman.UseRecallRange");

	    SpamAlot   = Context.GetConfigBool("PShaman.SpamAlot");
   	    NoJumpFromAoE = Context.GetConfigBool("PShaman.NoJumpFromAoE");

	    UseRepair  = Context.GetConfigBool("PShaman.UseRepair");
	    UseSell    = Context.GetConfigBool("PShaman.UseSell");
	    VendorName = Context.GetConfigString("PShaman.VendorName");

	    SellPoor     = Context.GetConfigBool("PShaman.SellPoor");
	    SellCommon   = Context.GetConfigBool("PShaman.SellCommon");
	    SellUncommon = Context.GetConfigBool("PShaman.SellUncommon");
	    SellRare     = Context.GetConfigBool("PShaman.SellRare");

	    ProtItems = new string[20];
	    for(int i = 0; i<20; i++)
	    {
		string name = String.Format("PShaman.Protected{0}", i);
		ProtItems[i] = Context.GetConfigString(name).ToLower();
	    }


	    string ui1 = Context.GetConfigString("PShaman.UseItem1");
	    switch(ui1)
	    {
	    case "Do not use":          UseItem1 = ItemUse_e.NoUse; break; 
	    case "My health <50%":      UseItem1 = ItemUse_e.MyHealth50;break; 
	    case "My health <25%":      UseItem1 = ItemUse_e.MyHealth25;break; 
	    case "Monster health >75%": UseItem1 = ItemUse_e.MobHealth75;break; 
	    case "Monster health >50%": UseItem1 = ItemUse_e.MobHealth50;break; 
	    case "We got adds":       UseItem1 = ItemUse_e.SaveForAdds;break; 
	    case "Feared/Charmed/Sleeped":       UseItem1 = ItemUse_e.Feared;break; 
	    }
	    UseItem1CD = Context.GetConfigInt("PShaman.UseItem1CD");
	    UseItem1Timer = new GSpellTimer(UseItem1CD * 1000);
	    UseItem1Timer.ForceReady();


	    string ui2 = Context.GetConfigString("PShaman.UseItem2");
	    switch(ui2)
	    {
	    case "Do not use":          UseItem2 = ItemUse_e.NoUse; break; 
	    case "My health <50%":      UseItem2 = ItemUse_e.MyHealth50;break; 
	    case "My health <25%":      UseItem2 = ItemUse_e.MyHealth25;break; 
	    case "Monster health >75%": UseItem2 = ItemUse_e.MobHealth75;break; 
	    case "Monster health >50%": UseItem2 = ItemUse_e.MobHealth50;break; 
	    case "We got adds":       UseItem2 = ItemUse_e.SaveForAdds;break; 
	    case "Feared/Charmed/Sleeped":       UseItem2 = ItemUse_e.Feared;break; 
	    }
	    UseItem2CD = Context.GetConfigInt("PShaman.UseItem2CD");
	    UseItem2Timer = new GSpellTimer(UseItem2CD * 1000);
	    UseItem2Timer.ForceReady();


	    string ui3 = Context.GetConfigString("PShaman.UseItem3");
	    switch(ui3)
	    {
	    case "Do not use":          UseItem3 = ItemUse_e.NoUse; break; 
	    case "My health <50%":      UseItem3 = ItemUse_e.MyHealth50;break; 
	    case "My health <25%":      UseItem3 = ItemUse_e.MyHealth25;break; 
	    case "Monster health >75%": UseItem3 = ItemUse_e.MobHealth75;break; 
	    case "Monster health >50%": UseItem3 = ItemUse_e.MobHealth50;break; 
	    case "We got adds":       UseItem3 = ItemUse_e.SaveForAdds;break; 
	    case "Feared/Charmed/Sleeped":       UseItem3 = ItemUse_e.Feared;break; 
	    }
	    UseItem3CD = Context.GetConfigInt("PShaman.UseItem3CD");
	    UseItem3Timer = new GSpellTimer(UseItem3CD * 1000);
	    UseItem3Timer.ForceReady();


	    string ui4 = Context.GetConfigString("PShaman.UseItem4");
	    switch(ui4)
	    {
	    case "Do not use":          UseItem4 = ItemUse_e.NoUse; break; 
	    case "My health <50%":      UseItem4 = ItemUse_e.MyHealth50;break; 
	    case "My health <25%":      UseItem4 = ItemUse_e.MyHealth25;break; 
	    case "Monster health >75%": UseItem4 = ItemUse_e.MobHealth75;break; 
	    case "Monster health >50%": UseItem4 = ItemUse_e.MobHealth50;break; 
	    case "We got adds":       UseItem4 = ItemUse_e.SaveForAdds;break; 
	    case "Feared/Charmed/Sleeped":       UseItem4 = ItemUse_e.Feared;break; 
	    }
	    UseItem4CD = Context.GetConfigInt("PShaman.UseItem4CD");
	    UseItem4Timer = new GSpellTimer(UseItem4CD * 1000);
	    UseItem4Timer.ForceReady();



            string bi1 = Context.GetConfigString("PShaman.BuffItem1");
            switch (bi1)
            {
            case "Do not use":          BuffItem1 = BuffUse_e.NoUse; break;
            case "Food":                BuffItem1 = BuffUse_e.Food; break;
            case "Potion/Usable item":  BuffItem1 = BuffUse_e.Potion; break;
            }
            BuffName1 = Context.GetConfigString("PShaman.BuffName1").ToLower();

            string bi2 = Context.GetConfigString("PShaman.BuffItem2");
            switch (bi2)
            {
            case "Do not use":          BuffItem2 = BuffUse_e.NoUse; break;
            case "Food":                BuffItem2 = BuffUse_e.Food; break;
            case "Potion/Usable item":  BuffItem2 = BuffUse_e.Potion; break;
            }
            BuffName2 = Context.GetConfigString("PShaman.BuffName2").ToLower();

            string bi3 = Context.GetConfigString("PShaman.BuffItem3");
            switch (bi3)
            {
            case "Do not use":          BuffItem3 = BuffUse_e.NoUse; break;
            case "Food":                BuffItem3 = BuffUse_e.Food; break;
            case "Potion/Usable item":  BuffItem3 = BuffUse_e.Potion; break;
            }
            BuffName3 = Context.GetConfigString("PShaman.BuffName3").ToLower();



	    string pvps = Context.GetConfigString("PShaman.PvPStyle");
	    switch(pvps)
	    {
	    case "Fight back":      PvPStyle = PvPStyle_e.FightBack; break; 
	    case "Active":          PvPStyle = PvPStyle_e.Active; break; 
	    case "Just die":        PvPStyle = PvPStyle_e.Die; break; 
	    }

	    CombatFire  = DecodeTotem(Context.GetConfigString("PShaman.CombatFireTotem"));
	    CombatEarth = DecodeTotem(Context.GetConfigString("PShaman.CombatEarthTotem"));
	    CombatWater = DecodeTotem(Context.GetConfigString("PShaman.CombatWaterTotem"));
	    CombatAir   = DecodeTotem(Context.GetConfigString("PShaman.CombatAirTotem"));

	    AddFire  = DecodeTotem(Context.GetConfigString("PShaman.AddFireTotem"));
	    AddEarth = DecodeTotem(Context.GetConfigString("PShaman.AddEarthTotem"));
	    AddWater = DecodeTotem(Context.GetConfigString("PShaman.AddWaterTotem"));
	    AddAir   = DecodeTotem(Context.GetConfigString("PShaman.AddAirTotem"));
	    
	    RestWater = DecodeTotem(Context.GetConfigString("PShaman.RestWaterTotem"));

	}
	#endregion 


            private bool BandageIfNeeded()
	{
	    if(!UseBandage) return false;

	    if(Me.Health * 100 > BandageHealth) return false;
	    
	    return CheckBandageApply(false);
	}

	private bool IsEating()
	{
	    GBuff [] buffs = Me.GetBuffSnapshot();
	    for(int i = 0; i<buffs.Length; i++)
	    {
		if(buffs[i].SpellName == "Food") return true;
	    }
	    return false;
	}

        GBuff FindBuff(string name)
        {
            GBuff[] buffs = Me.GetBuffSnapshot();
            for (int i = 0; i < buffs.Length; i++)
            {
                GBuff b = buffs[i];
                if(b.SpellName == name)
                {
                    return b;
                }
            }
            return null;
            
        }

        string SimilarBuff(string name)
        {
            GBuff[] buffs = Me.GetBuffSnapshot();
            string name_low = name.ToLower();
            for (int i = 0; i < buffs.Length; i++)
            {
                GBuff b = buffs[i];
                String n = b.SpellName.ToLower();
                if(n.Contains(name_low))
                {
                    return b.SpellName;
                }
            }
            return null;
        }

        bool CheckBuff(BuffUse_e use, string name, out string new_name, 
                       string key, string ConfigKey)
        {
            new_name = name;
            if(use != BuffUse_e.NoUse)
            {
                if(name != "" && SimilarBuff(name) != null)
                    return false; // got it
                Context.Log("Missing buff '"+name+"'");
               

                // Check that there are any items left

                int Left = GContext.Main.Interface.GetActionInventory(key);                
                if (Left < 1)
                {
                    Context.Log("  But there are no buff items left!");
                    return false;                    
                }

                int Timeout;
                if(use == BuffUse_e.Food)
                {
                    WaitForCombatToEnd(5000);
                    if(Me.IsInCombat) return false;
                    Context.Log("  Eating buff food");
                    Timeout = 15000;
                }
                else
                {
                    Timeout = 2000;
                }

                BuffSnapshot();
                CastSpell(key); // use
                if(use == BuffUse_e.Food)                 
                {
                    Thread.Sleep(1000); // Avoid the "Food" buff
                    BuffSnapshot();                    
                }
                
                
                GSpellTimer Futile = new GSpellTimer(Timeout, false); // max 15s wait for buff
                do
                {      
                    GBuff buff = FindNewBuff();
                    if(buff != null) 
                    {
                        Context.Log("  Got the buff " + buff.SpellName);
                        
                        if(name == "")
                        {
                            Context.Log("  No buff name configured. Setting it to \""+buff.SpellName+"\"");
                            name = buff.SpellName;
                            new_name = name;
                            Context.SetConfigValue(ConfigKey, buff.SpellName, true);
                        }
                    }
                    if(name != "")
                    {
                        string s = SimilarBuff(name);
                        if(s != null)
                        {
                            return true;
                        }
                    }
                } while(!Me.IsInCombat && !Me.IsDead && !Futile.IsReadySlow);
                if(Futile.IsReady)
                {
                    Context.Log("  Timed out waiting for buff '"+name+"'");
                }
            }
            return false;
        }
        
        private bool WaitForCombatToEnd(int maxTime)
        {
            if (Me.IsInCombat)
            {
                if (GObjectList.GetNearestAttacker(0) == null)
                {
                    mover.Stop();
                    GUnit Attacker = null;
                    GSpellTimer Futile = new GSpellTimer(maxTime, false);
                    do
                    {
                        Attacker = GObjectList.GetNearestAttacker(0);
                    } while (!Me.IsDead && Me.IsInCombat && Attacker == null && !Futile.IsReadySlow);
                    //if(Attacker != null)
                    //    Spam(" Got attack " + Attacker.Name);
                    if (Futile.IsReady)
                        Context.Log("Timeout waiting for combat flag to be able to eat");
                    //else
                    //Context.Log(" waited " + (5000 - Futile.TicksLeft) + "ms");			
                }
            }
            return !Me.IsInCombat;
        }

        public override bool CheckPartyHeal(GUnit OriginalTarget)
        {
            Target = OriginalTarget;
            if(WantHeal()) { DoHeal(); return true; };
            return false;
        }

        public override bool Rest()
	{
	    mover.Stop();
	    if(Me.IsDead) return false;
	    while(StandingInAoE) // we are inside some AoE effect (posions cloud thunderstom etc)
	    {
		Context.Log("Standing on an AoE effect!");
		StepOutOfAoE();
		Thread.Sleep(800); // let him land first
	    }
            Resting = true;
	    
            DoRestHeal();
            if(WantCurePoison())  DoCurePoison();
            if(WantCureDisease()) DoCureDisease();
            if(WantShield()) DoShield();
	    
	    bool didSomething = false;

	    didSomething |= BandageIfNeeded(); 

	    if(Me.Health * 100 <= RestHealth)
	    {
		if(Me.IsInCombat)
		{
		    if(GObjectList.GetNearestAttacker(0) == null)
		    {
			mover.Stop();
			GUnit Attacker = null;
			GSpellTimer Futile = new GSpellTimer(10000, false);
			do
			{
			    Attacker = GObjectList.GetNearestAttacker(0);
			} while(Me.IsInCombat && Attacker == null && !Futile.IsReadySlow);
			//if(Attacker != null)
			//    Spam(" Got attack " + Attacker.Name);
			if(Futile.IsReady)
			    Context.Log("Timeout waiting for combat flag to be able to eat");
			//else
			//Context.Log(" waited " + (5000 - Futile.TicksLeft) + "ms");			
		    }
		}
		
	    }
	    didSomething |= base.Rest();

	    if(Me.IsSitting) 
		SendKey("Common.Sit");

            didSomething |= CheckBuff(BuffItem1, BuffName1, out BuffName1, 
                                      "PShaman.BuffUsable1", "PShaman.BuffName1");
            didSomething |= CheckBuff(BuffItem2, BuffName2, out BuffName2,
                                      "PShaman.BuffUsable2", "PShaman.BuffName2");
            didSomething |= CheckBuff(BuffItem3, BuffName3, out BuffName3,
                                      "PShaman.BuffUsable3", "PShaman.BuffName3");

		
	    if(Me.IsSitting) 
		SendKey("Common.Sit");

            if(WantShield()) DoShield();

            oom = false; 

            if (Enchants.IsReady)
            {
                if(Me.IsSitting) 
                    SendKey("Common.Sit");
                CastSpellMana("PShaman.MainWeaponEnchant");
                CastSpellMana("PShaman.OffWeaponEnchant");
                Enchants.Reset();
		SendKey("Common.Escape"); // Just to be on the safe side
                
            }
            Resting = false;
	    return didSomething;
	}

        
        int mountBuffID = 0;

        private bool IsMounted()
        {
            if (mountBuffID == 0) return false;
            GBuff[] buffs = Me.GetBuffSnapshot();
            for (int i = 0; i < buffs.Length; i++)
            {
                if (buffs[i].SpellID == mountBuffID) return true;
            }
            return false;
        }

        private GBuff[] BuffSnap;
        private void BuffSnapshot()
        {
            BuffSnap = Me.GetBuffSnapshot();
            //Context.Log("Snapshot");
            //DumpBuffs();
        }

        // Find a buff not present in last BuffSnapshot
        private GBuff FindNewBuff()
        {
            GBuff[] buffs = Me.GetBuffSnapshot();
            //Context.Log("Search for new");
            //DumpBuffs();
            for (int i = 0; i < buffs.Length; i++)
            {
                GBuff b = buffs[i];
                // O(n^2) FTW
                GBuff old = null;
                for (int j = 0; j < BuffSnap.Length && old == null; j++)
                {
                    GBuff b2 = BuffSnap[j];
                    if (b2.SpellID == b.SpellID)
                    {
                        old = b;
                    }
                }
                if (old == null)
                {
                    //Context.Log("New buff: " + b.SpellName);
                    return b;
                }
            }
            return null;
        }


        /////////////////////////////////////////////////////////////////
        // Mounting

        private bool IsMounted(GUnit unit)
        {
            // Mount test based on spell ID - 5 June 2008
            int[] mountBuffsID = {
                35022, //	Black Hawkstrider
                6896, //	Black Ram
                17461, //	Black Ram
                22718, //	Black War Kodo
                22720, //	Black War Ram
                22721, //	Black War Raptor
                22717, //	Black War Steed
                22723, //	Black War Tiger
                22724, //	Black War Wolf
                578, //	Black Wolf
                35020, //	Blue Hawkstrider
                10969, //	Blue Mechanostrider
                33630, //	Blue Mechanostrider
                6897, //	Blue Ram
                17463, //	Blue Skeletal Horse
                43899, //	Brewfest Ram
                34406, //	Brown Elekk
                458, //	Brown Horse
                18990, //	Brown Kodo
                6899, //	Brown Ram
                17464, //	Brown Skeletal Horse
                6654, //	Brown Wolf
                39315, //	Cobalt Riding Talbuk
                34896, //	Cobalt War Talbuk
                39316, //	Dark Riding Talbuk
                34790, //	Dark War Talbuk
                17481, //	Deathcharger
                6653, //	Dire Wolf
                8395, //	Emerald Raptor
                36702, //	Fiery Warhorse
                23509, //	Frostwolf Howler
                16060, //	Golden Sabercat
                35710, //	Gray Elekk
                18989, //	Gray Kodo
                6777, //	Gray Ram
                459, //	Gray Wolf
                35713, //	Great Blue Elekk
                23249, //	Great Brown Kodo
                34407, //	Great Elite Elekk
                23248, //	Great Gray Kodo
                35712, //	Great Green Elekk
                35714, //	Great Purple Elekk
                23247, //	Great White Kodo
                18991, //	Green Kodo
                15780, //	Green Mechanostrider
                17453, //	Green Mechanostrider
                17465, //	Green Skeletal Warhorse
                824, //	Horse Riding
                6743, //	Horse Riding
                17459, //	Icy Blue Mechanostrider
                16262, //	Improved Ghost Wolf
                16287, //	Improved Ghost Wolf
                10795, //	Ivory Raptor
                17450, //	Ivory Raptor
                18995, //	Kodo Riding
                18996, //	Kodo Riding
                16084, //	Mottled Red Raptor
                16055, //	Nightsaber
                10798, //	Obsidian Raptor
                42936, //	Ornery Ram
                44370, //	Pink Elekk Call
                472, //	Pinto Horse
                35711, //	Purple Elekk
                35018, //	Purple Hawkstrider
                17455, //	Purple Mechanostrider
                23246, //	Purple Skeletal Warhorse
                826, //	Ram Riding
                6744, //	Ram Riding
                17456, //	Red & Blue Mechanostrider
                34795, //	Red Hawkstrider
                10873, //	Red Mechanostrider
                17462, //	Red Skeletal Horse
                22722, //	Red Skeletal Warhorse
                43883, //	Rental Racing Ram
                18363, //	Riding Kodo
                30174, //	Riding Turtle
                39317, //	Silver Riding Talbuk
                34898, //	Silver War Talbuk
                8980, //	Skeletal Horse
                10921, //	Skeletal Horse Riding
                29059, //	Skeletal Steed
                42776, //	Spectral Tiger
                10789, //	Spotted Frostsaber
                15781, //	Steel Mechanostrider
                23510, //	Stormpike Battle Charger
                8394, //	Striped Frostsaber
                10793, //	Striped Nightsaber
                897, //	Summon Angry Programmer
                44369, //	Summon Baby Pink Elekk
                31700, //	Summon Black Qiraji Battle Tank
                26655, //	Summon Black Qiraji Battle Tank
                26656, //	Summon Black Qiraji Battle Tank
                25863, //	Summon Black Qiraji Battle Tank
                25953, //	Summon Blue Qiraji Battle Tank
                32723, //	Summon Bonechewer Riding Wolf
                34767, //	Summon Charger
                23214, //	Summon Charger
                23215, //	Summon Charger
                34766, //	Summon Charger
                23261, //	Summon Darkreaver's Fallen Charger
                31331, //	Summon Dire Wolf
                23161, //	Summon Dreadsteed
                38311, //	Summon Eclipsion Hawkstrider
                1710, //	Summon Felsteed
                5784, //	Summon Felsteed
                5968, //	Summon Ghost Saber
                6084, //	Summon Ghost Saber
                26056, //	Summon Green Qiraji Battle Tank
                30829, //	Summon Kessel's Elekk
                30837, //	Summon Kessel's Elekk
                30840, //	Summon Kessel's Elekk Trigger
                39782, //	Summon Lightsworn Elekk
                18166, //	Summon Magram Ravager
                26054, //	Summon Red Qiraji Battle Tank
                41543, //	Summon Reins (Jorus)
                41544, //	Summon Reins (Malfas)
                41546, //	Summon Reins (Onyxien)
                41547, //	Summon Reins (Suraku)
                41548, //	Summon Reins (Voranaku)
                41549, //	Summon Reins (Zoya)
                39783, //	Summon Scryer Hawkstrider
                7910, //	Summon Tamed Raptor
                7915, //	Summon Tamed Turtle
                4946, //	Summon Tamed Wolf
                13819, //	Summon Warhorse
                13820, //	Summon Warhorse
                34768, //	Summon Warhorse
                34769, //	Summon Warhorse
                23241, //	Swift Blue Raptor
                43900, //	Swift Brewfest Ram
                23238, //	Swift Brown Ram
                23229, //	Swift Brown Steed
                23250, //	Swift Brown Wolf
                23220, //	Swift Dawnsaber
                23221, //	Swift Frostsaber
                23239, //	Swift Gray Ram
                23252, //	Swift Gray Wolf
                35025, //	Swift Green Hawkstrider
                23225, //	Swift Green Mechanostrider
                23219, //	Swift Mistsaber
                23242, //	Swift Olive Raptor
                23243, //	Swift Orange Raptor
                33660, //	Swift Pink Hawkstrider
                35027, //	Swift Purple Hawkstrider
                24242, //	Swift Razzashi Raptor
                42777, //	Swift Spectral Tiger
                23338, //	Swift Stormsaber
                23251, //	Swift Timber Wolf
                35028, //	Swift War Hawkstrider
                23223, //	Swift White Mechanostrider
                23240, //	Swift White Ram
                23228, //	Swift White Steed
                23222, //	Swift Yellow Mechanostrider
                24252, //	Swift Zulian Tiger
				49322, //	Swift Zhevra
                39318, //	Tan Riding Talbuk
                34899, //	Tan War Talbuk
                16059, //	Tawny Sabercat
                18992, //	Teal Kodo
                22480, //	Tender Wolf Steak
                10790, //	Tiger
                10796, //	Turquoise Raptor
                77, //	Turtle's Shell
                17454, //	Unpainted Mechanostrider
                10799, //	Violet Raptor
                15779, //	White Mechanostrider
                6898, //	White Ram
                39319, //	White Riding Talbuk
                34897, //	White War Talbuk
                17229, //	Winterspring Frostsaber
                 };

            Me.Refresh(true);
            if (Me.HasBuff(mountBuffsID))
            {
                return true;
            }

            // Pontus and Birt code
            GBuff[] buffs = unit.GetBuffSnapshot();

            foreach (GBuff b in buffs)
            {
                string s = b.SpellName;

                if (
                   s.Contains("Horse") ||
                   s.Contains("Stallion") ||
                   s.Contains("Warhorse") ||
                   s.Contains("Raptor") ||
                   s.Contains("Kodo") ||
                   s.Contains("Wolf") ||
                   s.Contains("Saber") ||
                   s.Contains("Ram") ||
                   s.Contains("Mechanostrider") ||
                   s.Contains("Hawkstrider") ||
                   s.Contains("Elekk") ||
                   s.Contains("Steed") ||
                   s.Contains("Tiger") ||
                   s.Contains("Talbuk") ||
                   s.Contains("Frostsaber") ||
                   s.Contains("Nightsaber") ||
                   s.Contains("Battle Tank") ||
                   s.Contains("Charger") ||
                   s.Contains("Dreadsteed") ||
                   s.Contains("Frostwolf Howler") ||
                   s.Contains("Cheetah") ||
                   s.Contains("Travel form") ||
                   s.Contains("Reins") || // yeah right
                   s.Contains("Turtle") || // lol....Mitch <3's Turtles xD
                   s.Contains("Mistsaber") ||
                   s.Contains("Battlestrider") ||
                   s.Contains("steed") ||
                   s.Contains("Palomino") ||
                   s.Contains("Amani") ||
                   s.Contains("Stormsaber") ||
                   s.Contains("Windrider") ||
                   s.Contains("Gryphon") ||
                   s.Contains("Nether Ray") ||
                   s.Contains("Netherdragon") ||
                   s.Contains("Raven Lord")
                )
                {
                    if (s != "Ghost Wolf") return true;
                }
            }

            return false;
        }
#if !PPATHERENABLED
        private bool WantMount()
        {
            if (Context.IsCorpseNearby)
            {
                return false;
            }

            GUnit[] Monsters = GObjectList.GetMonsters();
            double minDist = 1E100; // Far far away
            string mobName = "Monster";
            string oldMobName = mobName;

            foreach (GMonster Add in Monsters)
            {
                double d = Add.GetDistanceTo(Me);
                if (!Add.IsDead && d < minDist)
                {
                    if (Add.IsValidProfileTarget)
                    {
                        mobName = Add.Name;
                        minDist = d;
                    }
                }
            }
            if (minDist < MountDistance) return false;
            return true;
        }
        private void MountUp()
        {
            int MIN_MOUNT_LEVEL = 30;
            if (GPlayerSelf.Me.Level < MIN_MOUNT_LEVEL) return;
            if (IsMounted()) return;
            if (GPlayerSelf.Me.IsInCombat) return;
            if (!ForceNoMount.IsReady) return;

            bool HaveMount = new GInterfaceHelper().IsKeyPopulated("Common.Mount");
            if (!HaveMount) return;

            // if (!WantMount()) return;
            mover.Stop();
            Context.Log("Mount up");
            BuffSnapshot();
            CastSpell("PShaman.Mount");

            Thread.Sleep(100);

            string badMount = null; // from ppather mount.cs - June 7 2008

            if (GContext.Main.RedMessage.Contains("while swimming"))
            {
                badMount = "Trying to mount while swimming";
            }
            else if (GContext.Main.RedMessage.Contains("can't mount here"))
            {
                badMount = "Trying to mount inside";
            }

            if (null != badMount)
            {
                Context.Log(badMount);
                ForceNoMount.Reset();
                return;
            }
        }

        private void Dismount()
        {
            if (!IsMounted()) return;
            //mover.Stop(); // glider waypoint followe gets really sad
            Context.Log("Dismount");
            SendKey("PShaman.Mount");
        }
        private void RecallTotems()
	{
	    if(FireTotem != null ||
	       EarthTotem != null ||
	       WaterTotem != null ||
	       AirTotem != null)
	    {
		Spam("Totemic Call");
		CastSpell("PShaman.TotemicCall", true, true);
		FireTotem = null; FireTotemType = Totem_e.None;
		EarthTotem = null; EarthTotemType = Totem_e.None;
		WaterTotem = null; WaterTotemType = Totem_e.None;
		AirTotem = null; AirTotemType = Totem_e.None;
	    }
	}
#endif
        private void RecallTotemsIfNeeded()
	{
	    bool recall = false;
	    if(FireTotem != null && !IsTotemStillUseful(FireTotem))  recall = true; 
	    if(EarthTotem != null && !IsTotemStillUseful(EarthTotem))  recall = true; 
	    if(WaterTotem != null && !IsTotemStillUseful(WaterTotem))  recall = true; 
	    if(AirTotem != null && !IsTotemStillUseful(AirTotem))  recall = true; 
	    if(recall && Interface.IsKeyReady("PShaman.TotemicCall"))
	    {
		RecallTotems();
	    }
	}

        private void DoRestHeal()
        {
            bool want = false;
            do
            {
                want = WantHeal();
                if(want) 
                {
                    mover.Stop();
                    DoHeal();
                    while(!HealCooldown.IsReadySlow && !Me.IsInCombat); // want more perhaps...
                }
                if(Me.IsDead || Me.IsInCombat) return; 
            } while(want && !oom);
        }


        public  void RunningAction2()
	{
            if(Me.IsDead) return; 
	    if(Me.IsSitting) 
		SendKey("Common.Sit");

	    if(UseRecallRange)
	    {
		//RecallTotems();
		RecallTotemsIfNeeded();
	    }
	    if(UseSell || UseRepair)
	    {
		if(SellTimer.IsReady)
		{
		    GUnit vendor = GObjectList.FindUnit(VendorName);
		    if(vendor != null && vendor.DistanceToSelf < 15)
		    {
			Context.Log("I am close to vendor " + VendorName);
			mover.Stop();
			SellAndRepair(vendor, false);
			SellTimer.Reset();
		    }
		}
	    }

            DoRestHeal();
            if(WantShield()) DoShield();
            
	    // Check if glider is running around too damaged
	    if(Me.Health * 100 < RestHealth)
	    {
		// *Gasp*
		if(DistanceToClosestMonsterFrom(Me) > AvoidAddDistance)
		{
		    mover.Stop();		    
		    Rest();
		}
		
	    }
            //if(Context.IsCorpseNearby)
            if(Context.Party.Mode == GPartyMode.Follower)
                LootStuff();

	    if(UseMount)
	    {
		double d = DistanceToClosestMonsterFrom(Me);
		if(DistanceToClosestMine() < HarvestRange+10)
		{
		    if(IsMounted())
		    {
			Context.Log("Close to a mine, dismount");
			Dismount();
		    }
		}
		else if((ForceNoMount.IsReady && !Context.IsCorpseNearby))
		{
		    if(d > MountDistance || forceMount)
		    {		      
                        if(forceMount)
                            Context.Log("I obey");
			//mover.Stop();
			ForceNoMount.Reset(); // Do not try to mount the next 5 seconds, we might be in water or something
			MountUp();
                        forceMount = false;
		    }
		}
	    }	    

        }
        

        private List<long> looted = new List<long>();
        private void LootStuff()
        {
            if(FindUnitsAttackingParty().Count > 0) return; 
            bool Looted = false;
            bool Moved = false;
	    GUnit[] Monsters = GObjectList.GetMonsters();
	    foreach (GMonster Add in Monsters)
	    {
                if(Me.IsInCombat) return;
                if(Add.IsLootable && Add.DistanceToSelf < 15)
                {
                    if(!looted.Contains(Add.GUID))
                    {
                        if(FindUnitsAttackingParty().Count > 0) return; 
                        mover.Stop();
                        Moved = true;
                        Context.Log("Loot " + Add.Name);
                        looted.Add(Add.GUID);
                        if(Approach(Add, true, 4000))
                            //if(Add.ApproachSafe(Context.MeleeDistance, false))
                        {
                            Add.Interact();
                            Looted = true;
                            CastSpell("Common.PostLoot");
                        }
                    }
                }
            }

            if(PickupJunk)
            {
                GNode[] Nodes = GObjectList.GetNodes();
                foreach (GNode Node in Nodes)
                {
                    if(Me.IsInCombat) return;
                    if(Node.IsValid && Node.DistanceToSelf < 15)
                    {
                        if(!looted.Contains(Node.GUID))
                        {
                            if(FindUnitsAttackingParty().Count > 0) return; 
                            mover.Stop();
                            Moved = true;
                            Context.Log("Loot " + Node.Name);
                            looted.Add(Node.GUID);
                            if(Movement.MoveToLocation(Node.Location, Context.MeleeDistance, false))
                            {
                                Node.Interact();
                                WaitForCasting();
                                Looted = true;
                            }
                        }
                    }
                }
            }
            if(Looted || Moved)
            {
                mover.Stop();
                Thread.Sleep(500);
            }
            
        }

        public override void RunningAction()
	{

                RunningAction2(); // default
        }


        public override void ApproachingTarget(GUnit Target)
	{
	    //Context.Log("ApproachingTarget " + Target.Name + " distance " + (int)Target.DistanceToSelf);
	    Dismount();
	}

	#region Totems
	

        // Make sure this totem is up and close.  If it's not, cast it and return it.
        bool IsTotemStillUseful(GUnit LastKnown)
        {
            if (LastKnown == null || !LastKnown.IsValid || LastKnown.DistanceToSelf > 20.0)
            {
		return false;
            }
            else
                return true;
        }

        // Return a list of all my totems.
        private GUnit[] GetMyTotems()
        {
            GUnit[] All = GObjectList.GetUnits();
            List<GUnit> MyTotems = new List<GUnit>();

            foreach (GUnit one in All)
                if (one.CreatedBy == Me.GUID)
                    MyTotems.Add(one);

            return MyTotems.ToArray();
        }


        private GUnit CastTotem(Totem_e totem)
        {
            // Cast it:
	    //Context.Log("Before CastSpell "+ SpellName + " is ready " + Interface.IsKeyReady(SpellName));
            string SpellName = TotemKey(totem);
            CastSpellMana(SpellName, true, true);

            // Wait for a new totem.
            GSpellTimer Futility = new GSpellTimer(2000, false);

            while (!Futility.IsReadySlow)
            {
                GUnit[] NewTotems = GetMyTotems();

                foreach (GUnit totemUnit in NewTotems)
                {
                    if (totemUnit.Age < 2000)
                    {
                        Context.Debug("New totem is: " + totemUnit.ToString());
			Spam("  Popped " + totemUnit.Name);
                        return totemUnit;
                    }
                }
            }

            // Never found it, damn.
            Context.Log("Never found new totem when casting, damn!");
            return null;
        }

        // Totem cooldown timers
        // TODO: there are talests for some of them

        // Fire Nova 15s
        // Grounding 15s
        // Earth Elemental 20 min
        // Fire Elemental 20 min
        // Earthbind 15s
        // Stoneclaw 30s
        
        GSpellTimer FireNovaCD   = new GSpellTimer(15*1000);
        GSpellTimer GroundingCD  = new GSpellTimer(15*1000);
        GSpellTimer EarthElementalCD = new GSpellTimer(20*60*1000);
        GSpellTimer FireElementalCD  = new GSpellTimer(20*60*1000);
        GSpellTimer EarthbindCD  = new GSpellTimer(15*1000);
        GSpellTimer StoneclawCD  = new GSpellTimer(30*1000);
        GSpellTimer ManatideCD   = new GSpellTimer(30*1000);

	bool TotemOnCoolDown(Totem_e totem)
	{
	    switch(totem)
	    {
	    case Totem_e.FireNova: return !FireNovaCD.IsReady;
	    case Totem_e.FireElemental: return !FireElementalCD.IsReady;
	    case Totem_e.Earthbind: return !EarthbindCD.IsReady;
	    case Totem_e.Stoneclaw: return !StoneclawCD.IsReady;
	    case Totem_e.EarthElemental: return !EarthElementalCD.IsReady;
            case Totem_e.Grounding: return  !GroundingCD.IsReady;
	    case Totem_e.ManaTide: return !ManatideCD.IsReady;                
	    }
            return false; // all other have no CD
	}
        
        void ResetTotemCooldown(Totem_e totem)
        {
            Context.Log("reset totem cooldown: " + totem);
	    switch(totem)
	    {
	    case Totem_e.FireNova:  FireNovaCD.Reset(); break;
	    case Totem_e.FireElemental:  FireElementalCD.Reset(); break;
	    case Totem_e.Earthbind:  EarthbindCD.Reset(); break;
	    case Totem_e.Stoneclaw:  StoneclawCD.Reset(); break;
	    case Totem_e.EarthElemental:  EarthElementalCD.Reset(); break;
            case Totem_e.Grounding:   GroundingCD.Reset(); break;
	    case Totem_e.ManaTide:  ManatideCD.Reset();                 break;
	    }          
        }

	#endregion

        #region KillTarget


            // Some combat state
            Int64 LastTarget = 0;
	int no_retries = 0;
	bool IsRunning = false;

        bool HaveAdd; // we got an add?
        long AddedGUID;
	bool HaveCloseAdd; // we got an add in melee range?
	GUnit CloseAdd = null;
	GUnit Target = null;
        bool Resting = false;
	bool  GCD; // is GCD active
        bool ForceBolt = false;

        GSpellTimer WaitForMobT = new GSpellTimer(5000);

	Totem_e FireTotemType = Totem_e.None;
	Totem_e WantFireTotemType = Totem_e.None;
	GUnit FireTotem;

	Totem_e EarthTotemType = Totem_e.None;
	Totem_e WantEarthTotemType = Totem_e.None;
	GUnit EarthTotem;

	Totem_e WaterTotemType = Totem_e.None;
	Totem_e WantWaterTotemType = Totem_e.None;
	GUnit WaterTotem;

	Totem_e AirTotemType = Totem_e.None;
	Totem_e WantAirTotemType = Totem_e.None;
	GUnit AirTotem;

        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////


	private bool ShouldWhiteDPS()
	{
	    if(Context.Party.Mode == GPartyMode.Solo) return true;
	    if(Context.Party.Mode == GPartyMode.Leader) return true;
	    if(Context.Party.HealMode == GHealDisposition.Dedicated) return false;

	    if(Target.IsTargetingMe && StopDPSOnAggro) return false;
	    return Target.Health < WhiteDPSLife;
	}


	private bool ShouldDPS()
	{
	    if(Context.Party.Mode == GPartyMode.Solo) return true;
	    if(Context.Party.Mode == GPartyMode.Leader) return true;
	    if(Context.Party.HealMode == GHealDisposition.Dedicated) return false;

	    if(Target.IsTargetingMe && StopDPSOnAggro) return false;
	    return Target.Health < DPSLife;
	}


	/*
	  Each skill has 2 function
	  bool WantSkill() and bool DoSkill;
	  
	  WantSkill should check all contition for performing a skill and see if they are met

        */


        private bool HaveShield(Shield_e sh)
        {
            string name = "xxx";
            if(sh == Shield_e.Lightning) name = "Lightning Shield";
            if(sh == Shield_e.Water) name = "Water Shield";
            GBuff buff = FindBuff(name);
            if(buff != null) return true;
            return false;                
        }

	private bool WantShield()
	{
            if(Shield == Shield_e.None) return false;
	    if(!ShieldCooldown.IsReady) return false;
            if(IsMounted()) return false;
            if(HaveShield(Shield)) return false; // me happy campe
            if(!EvalWhenCast(WhenShield)) return false; // no renews in combat
            
            if(Me.Mana > HealMana || Shield == Shield_e.Water)
            {
                return true; 
            }
            return false;
        }

	private bool DoShield()
        {
            string key = "xxx";
            if(Shield == Shield_e.Lightning) key = "PShaman.LightningShield";
            if(Shield == Shield_e.Water)     key = "PShaman.WaterShield";

            Spam(Shield + "Shield"); 
	    CastSpell(key, true, true);
            ShieldCooldown.Reset();
            return true;            
        }

        // Heal logic
	GPlayer HealTarget = null;
        HealType_e HealType = HealType_e.None;
	private bool WantHeal()
	{
	    if(!HealCooldown.IsReady) return false;

            List<GPlayer> targets = new List<GPlayer>(); // potential guys to heal
            targets.Add(Me);
            //Context.Log(Me.Name + " is a viable heal target");


            if (Context.Party.HealParty)  // Check party too
            {
                long[] PartyMembers = Context.Party.GetPartyMembers();
                foreach (long OneGuy in PartyMembers)
                {
                    GObject TargetObject = GObjectList.FindObject(OneGuy);
                    
                    if (TargetObject == null)  // Party member is not around, no big deal.
                        continue;
                    
                    GPlayer Member = (GPlayer)TargetObject;
                    targets.Add(Member);
                    //Context.Log(Member.Name + " is a viable heal target");
                }
            }

            // Analyze the situation
            int NumberForChain = 0;
            GPlayer targetForPanic = null;
            GPlayer targetForLesser = null;
            GPlayer targetForWave = null;
            GPlayer targetForChain = null;
            
            foreach(GPlayer player in targets)
            {
                double life = player.Health;
                if(Resting)
                {
                    if(life < RestHealWaveLife)
                    {
                        targetForWave = player;
                        //Context.Log(player.Name + " need a healing wave");
                    }
                    
                    if(life < RestHealChainLife)
                    {
                        if(targetForChain == null || life < targetForChain.Health)
                            targetForChain = player;
                        //Context.Log(player.Name + " need a chain heal");
                        NumberForChain ++;
                    }
                } 
                else
                {
                    if(life < HealPanicLife)
                    {
                        targetForPanic = player;
                        //Context.Log(player.Name + " need a panic heal");
                    }
                    
                    if(life < HealLesserWaveLife)
                    {
                        targetForLesser = player;
                        //Context.Log(player.Name + " need a lesser heal");
                    }
                    
                    if(life < HealWaveLife)
                    {
                        targetForWave = player;
                        //Context.Log(player.Name + " need a healing wave");
                    }
                    
                    if(life < HealChainLife)
                    {
                        if(targetForChain == null || life < targetForChain.Health)
                            targetForChain = player;
                        //Context.Log(player.Name + " need a chain heal");
                        NumberForChain ++;
                    }
                }
            }

            // Choose target and heal type
            HealType = HealType_e.None;
            if(targetForPanic != null)
            {
                HealTarget = targetForPanic;
                HealType = HealType_e.Panic;
            }
            else if(targetForLesser != null)
            {
                HealTarget = targetForLesser;
                HealType = HealType_e.Lesser;
            }
            else if(targetForWave != null)
            {
                HealTarget = targetForWave;
                HealType = HealType_e.Wave;
            }
            else if(targetForChain != null && NumberForChain > 1)
            {
                HealTarget = targetForChain;
                HealType = HealType_e.Chain;
            }
            //if(HealType != HealType_e.None)          Context.Log(HealTarget.Name + " needs a " + HealType);
	    return HealType != HealType_e.None;
	}

	private bool DoHeal()
	{
	    if(HealTarget != null)
	    {
                string healkey = "";
                if(HealType == HealType_e.None) return false; // doh!
                if(HealType == HealType_e.Wave)   healkey = "PShaman.HealingWave";
                if(HealType == HealType_e.Lesser) healkey = "PShaman.LesserHealingWave";
                if(HealType == HealType_e.Chain)  healkey = "PShaman.ChainHeal";                

                if(HealTarget == Me) healkey += "Self";

                if(!HasEnoughManaFor(healkey))
                    return false;

                if(HealType == HealType_e.Panic)  
                {
                    CastSpell("PShaman.NS");
                    healkey = "PShaman.HealingWave";
                    if(HealTarget == Me) healkey += "Self";
                }

		Spam("Heal " + HealTarget.Name + " with " + HealType+ "  HP " + (int)(HealTarget.Health*100));

                if(HealTarget == Me)
                    CastSpellMana(healkey);
                else
                    CastOnMemberMana(HealTarget, healkey, Target);
		HealCooldown.Reset();
	    }
	    return true;
	}


	// Interrupt shock
	private bool WantManaPotion()
	{
	    if(Me.Mana < 0.10 && PotionTimer.IsReady)
            {
                int NumOfPotion = Interface.GetActionInventory("Common.Potion");
                if(NumOfPotion > 0 && Interface.IsKeyReady("Common.Potion"))
                {
                    return true;
                }
            }
	    return false;
	}

	private bool DoManaPotion()
	{
            CastSpell("Common.Potion");
            oom = false;
            PotionTimer.Reset();
	    return true;
	}

	// Interrupt shock
	private bool WantInterruptShock()
	{
	    if(Target.IsCasting && 
	       Interface.IsKeyReady("PShaman.InterruptShock"))
	    {
		return true;
	    }
	    return false;
	}

	private bool DoInterruptShock()
	{
	    Spam("Interrupt Shock "); //  + Me.Mana + " > " + DPSShockMana);
	    CastSpellMana("PShaman.InterruptShock", true, true);
	    return true;
	}

	// Lighting bolt
	private bool WantBolt()
	{
            bool want = false;
            if(!ShouldDPS()) return false;
            if(Target.Health < 0.05) return false; 
            if(Me.Mana <= HealMana) return false;
            
            if(IsNatureImmune(Target.Name)) return false; // doh

            if(Target.DistanceToSelf > BoltDistance) want = true;
            if(ForceBolt || BoltSpam) want = true;
            if(BoltOnFocused && FindBuff("Focused Casting") != null) want = true;
            
            
	    if( want && Interface.IsKeyReady("PShaman.LightningBolt"))
	    {
		return true;
	    }
	    return false;
	}

	private bool DoBolt()
	{
	    Spam("Lightning Bolt "); //  + Me.Mana + " > " + DPSShockMana);
            CastSpellMana("PShaman.LightningBolt");
            ForceBolt = false;
	    return true;
	}


        private Shock_e SelectDPSShock()
        {
            Shock_e s = DPSShock;
            if(s == Shock_e.None) return s;

            // immunity checking is not perfect if the target has multiple immunities, but thats just silly
            if(s == Shock_e.Earth && IsNatureImmune(Target.Name))
                s = Shock_e.Frost; 

            if(s == Shock_e.Frost && IsFrostImmune(Target.Name))
                s = Shock_e.Earth; 

            if(s == Shock_e.Flame && IsFireImmune(Target.Name))
                s = Shock_e.Earth; 

            return s;
        }

	// DPS shock
	private bool WantDPSShock()
	{
            if(!Interface.IsKeyReady("PShaman.EarthShock")) return false;

            if(!ShouldDPS()) return false;
            if(Me.Mana <= HealMana) return false;
            if(Target.DistanceToSelf > 20.0) return false;
                   
            // Start off by selecting shock type
            Shock_e s = SelectDPSShock();
            if(s == Shock_e.None) return false;

            bool want = false;
            if(DPSShockFocus)
            {
            bool Clearcast = FindBuff("Clearcasting") != null;
            bool Focused   = FindBuff("Focus") != null;
                if(Clearcast ||Focused) want = true;
            }

            if(DPSShockStormstrike)
            {
                bool Stormstrike = false;
                GBuff [] buffs = Target.GetBuffSnapshot();
                foreach(GBuff b in buffs)
                {
                    if(b.SpellName == "Stormstrike") Stormstrike = true;
                }
                if(Stormstrike) want = true;
            }
            if(Me.Mana > DPSShockMana) want = true;
            
            if(s == Shock_e.Flame)
            {
                // check that target does not have flame shock DOT
                GBuff [] buffs = Target.GetBuffSnapshot();
                foreach(GBuff b in buffs)
                {
                    if(b.SpellName == "Flame Shock") return false;
                }                    
            }

	    if(want)
	    {
		return true;
	    }
	    return false;
	}

	private bool DoDPSShock()
	{
            Shock_e s = SelectDPSShock();
            string key = "xxx";
	    Spam(s + "Shock ");
            if(s == Shock_e.Earth) key = "PShaman.EarthShock"; 
            if(s == Shock_e.Frost) key = "PShaman.FrostShock"; 
            if(s == Shock_e.Flame) key = "PShaman.FlameShock"; 
	    if(CastSpellMana(key, true, true))
            {
            }
	    return true;
	}

	// Stormstrike
	private bool WantStormstrike()
	{
	    if(ShouldDPS() &&
               Target.IsInMeleeRange && 
               Me.Mana > HealMana &&               
               UseStormstrike &&
	       Interface.IsKeyReady("PShaman.Stormstrike"))
	    {
		return true;
	    }
	    return false;
	}

	private bool DoStormstrike()
	{
	    Spam("Stormstrike");
	    CastSpellMana("PShaman.Stormstrike", true, true);
	    return true;
	}

	// Purge
	private bool WantPurge()
	{
            if(UsePurge && TryPurge)
            {
                GBuff [] buffs = Target.GetBuffSnapshot();
                foreach(GBuff b in buffs)
                {
                    if(!b.IsHarmful)
                        return true; // there is a buff there
                }
            }
            return false;
        }

	private bool DoPurge()
	{
	    Spam("Purge");
	    CastSpellMana("PShaman.Purge", true, true);
            TryPurge = false;
	    return true;
        }

	// Shamanistic Rage
	private bool WantRage()
	{
	    if(UseRage && 
               ShouldWhiteDPS() &&
               Me.Mana <= RageMaxMana &&
               (Target.Health >= RageMinHealth || HaveAdd) && 
	       Interface.IsKeyReady("PShaman.Rage"))
	    {
		return true;
	    }
	    return false;
	}

	private bool DoRage()
	{
	    Spam("Shamanistic Rage");
	    CastSpellMana("PShaman.Rage", true, true);
	    return true;
	}


	// Fire totem
	private bool WantFireTotem()
	{

	    WantFireTotemType = Totem_e.None;

            if(Me.Mana <= HealMana) return false;
	    if(HaveAdd) WantFireTotemType = AddFire;
	    else WantFireTotemType = CombatFire;
	    if(WantFireTotemType == Totem_e.None) return false;

	    if(TotemOnCoolDown(WantFireTotemType)) return false;
	    if(FireTotemType == WantFireTotemType && IsTotemStillUseful(FireTotem)) return false; 

	    if(!Interface.IsKeyReady(TotemKey(WantFireTotemType))) return false; 
	    return true;
	}

	private bool DoFireTotem()
	{
	    Spam("Pop Fire Totem");
	    FireTotem = CastTotem(WantFireTotemType);
	    if(FireTotem != null) 
            {
		FireTotemType = WantFireTotemType;
                ResetTotemCooldown(FireTotemType);
            }
	    return FireTotem != null;
	}


	// Earth totem
	private bool WantEarthTotem()
	{

	    WantEarthTotemType = Totem_e.None;
            if(Me.Mana <= HealMana) return false;

            if(UseTremor && AnyAttackerHasTendency("fearer"))
                WantEarthTotemType = Totem_e.Tremor;
	    else if(HaveAdd) 
                WantEarthTotemType = AddEarth;
	    else 
                WantEarthTotemType = CombatEarth;
	    if(WantEarthTotemType == Totem_e.None) return false;

            //Context.Log("Want earth totem: " + WantEarthTotemType + " had " + EarthTotemType + " useful " + IsTotemStillUseful(EarthTotem)); 
	    if(TotemOnCoolDown(WantEarthTotemType)) 
            {
                //  Context.Log(" totem " + WantEarthTotemType + " is on cooldown");
                return false;
            }
	    if(WantEarthTotemType == EarthTotemType && IsTotemStillUseful(EarthTotem)) return false; 

	    if(!Interface.IsKeyReady(TotemKey(WantEarthTotemType))) return false; 
	    return true;
	}

	private bool DoEarthTotem()
	{
	    Spam("Pop Earth Totem");
	    EarthTotem = CastTotem(WantEarthTotemType);
	    if(EarthTotem != null)
            {
		EarthTotemType = WantEarthTotemType;
                ResetTotemCooldown(EarthTotemType);
            }
	    return EarthTotem != null;
	}


	// Water totem
	private bool WantWaterTotem()
	{
	    WantWaterTotemType = Totem_e.None;
            if(Me.Mana <= HealMana) return false;
            if(UsePoisonTotem && AnyAttackerHasTendency("poisoner"))
                WantWaterTotemType = Totem_e.PoisonCleansing;
            else if(UseDiseaseTotem && AnyAttackerHasTendency("diseaser"))
                WantWaterTotemType = Totem_e.DiseaseCleansing;
            else if(HaveAdd) 
                WantWaterTotemType = AddWater;
	    else 
                WantWaterTotemType = CombatWater;
	    if(WantWaterTotemType == Totem_e.None) return false;

	    if(TotemOnCoolDown(WantWaterTotemType)) return false;
	    if(WantWaterTotemType == WaterTotemType && IsTotemStillUseful(WaterTotem)) return false; 

	    if(!Interface.IsKeyReady(TotemKey(WantWaterTotemType))) return false; 
	    return true;
	}

	private bool DoWaterTotem()
	{
	    Spam("Pop Water Totem");
	    WaterTotem = CastTotem(WantWaterTotemType);
	    if(WaterTotem != null)
            {
		WaterTotemType = WantWaterTotemType;
                ResetTotemCooldown(WaterTotemType);
            }
	    return WaterTotem != null;
	}


	// Air totem
	private bool WantAirTotem()
	{
	    WantAirTotemType = Totem_e.None;
            if(Me.Mana <= HealMana) return false;

            if(UseGrounding && AnyAttackerHasTendency("caster"))
            {
                WantAirTotemType = Totem_e.Grounding;
            }
	    else if(HaveAdd) 
                WantAirTotemType = AddAir;
	    else 
                WantAirTotemType = CombatAir;

	    if(WantAirTotemType == Totem_e.None) return false;


	    if(TotemOnCoolDown(WantAirTotemType)) return false;
	    if(WantAirTotemType == AirTotemType && IsTotemStillUseful(AirTotem)) return false; 

	    if(!Interface.IsKeyReady(TotemKey(WantAirTotemType))) return false; 
	    return true;
	}

	private bool DoAirTotem()
	{
	    Spam("Pop Air Totem");
	    AirTotem = CastTotem(WantAirTotemType);
	    if(AirTotem != null)
            {
		AirTotemType = WantAirTotemType;
                ResetTotemCooldown(AirTotemType);
            }
	    return AirTotem != null;
	}



	// Cure disease
	private bool WantCureDisease()
	{
	    if(!DiseaseCooldown.IsReady) return false;
            if(!EvalWhenCast(WhenDisease)) return false;
	    GBuff [] buffs = Me.GetBuffSnapshot();
            foreach(GBuff b in buffs)
            {
                if(b.BuffType == GBuffType.Disease) return true;
            }
            return false;
	}

	private bool DoCureDisease()
	{
            Spam("Cure Disease");
            bool ok = CastSpellMana("PShaman.CureDisease");
            DiseaseCooldown.Reset();
            return ok;
	}
	

	// Cure poison
	private bool WantCurePoison()
	{
	    if(!PoisonCooldown.IsReady) return false;
            if(!EvalWhenCast(WhenPoison)) return false;
	    GBuff [] buffs = Me.GetBuffSnapshot();
            foreach(GBuff b in buffs)
            {
                if(b.BuffType == GBuffType.Poison) return true;
            }
            return false;
	}

	private bool DoCurePoison()
	{
            Spam("Cure Poison");
            bool ok = CastSpellMana("PShaman.CurePoison");
            PoisonCooldown.Reset();
            return ok;
	}
	

        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////



        void ConsiderSkipWP()
        {
            const int maxSkip  = 8;
            if (Context.CurrentMode != GGlideMode.Glide) return;
            if (!SkipWaypoints) return;

            GProfile profile = Context.Profile;

            int index = profile.CurrentIndex;
            int i = 0;
            List<GLocation> wps = profile.Waypoints;
            GLocation currentWP = wps[index];
            int newIndex = index;
            foreach (GLocation wp in wps)
            {
                if (i >= index - maxSkip && i <= index + maxSkip)
                {
                    // consider if this is better to go to
                    if (Me.GetDistanceTo(wp) < Me.GetDistanceTo(currentWP))
                    {
                        newIndex = i;
                    }
                }
                i++;
            }
            if (newIndex < index)
            {
                for (i = index; i > newIndex; i--)
                {
                    if (profile.ReverseWaypoints)
                        profile.ConsumeCurrentWaypoint();
                    else
                        profile.SetPreviousWaypoint();
                    Context.Log("Skip waypoint backwards WP #" + i);
                }
            }
            else if (newIndex > index)
            {
                for (i = index; i < newIndex; i++)
                {
                    if (profile.ReverseWaypoints)
                        profile.SetPreviousWaypoint();
                    else
                        profile.ConsumeCurrentWaypoint();
                    Context.Log("Skip waypoint forwards WP #" + i);
                }
            }
        }



	public GCombatResult EndCombat(GCombatResult res, GUnit target)
	{
	    mover.Stop();	

	    if(res == GCombatResult.Retry && LastTarget == target.GUID)
	    {
		no_retries++;
		if(no_retries > 5) // Max 5 retries on the the same mob
		    res =  GCombatResult.Bugged;
	    }
	    else
		no_retries = 0;
	    LastTarget = target.GUID;


	    if((HaveAdd) && res == GCombatResult.Success)
	    {
		GUnit Add = GObjectList.FindUnit(AddedGUID);

		if (Add == null)
		{
		    Context.Log("*** Could not find add after combat, id = " + AddedGUID.ToString("x"));
		} 
		else if (!Add.SetAsTarget(false))
		{
		    Context.Log("*** Could not target add after combat, name = '" + Add.Name + "', id = " + Add.GUID.ToString("x"));
		}
		else
		{
		    // Tell Glider to immediately begin wasting this guy and not rest:
		    res = GCombatResult.SuccessWithAdd;
		}
	    }
	    else
	    {
                if(Me.Target != null) 
                    Context.ClearTarget();
	    }

	    Spam("Combat done. lost " + (int)((CombatStartHealth - Me.Health)*100) + "% health. " + res);
	    ForceNoMount.Reset(); // Do not try to mount the next 5 seconds, lootable issues
            ConsiderSkipWP();

	    if(Me.IsDead)
		return GCombatResult.Died;

	    LastKillTimer.Reset();
	    /*
              if(res != GCombatResult.Died)
              {
              if(!HaveAdd)
              RecallTotems();
              }*/
	    return res;
	}

        private bool CheckUseItem(ItemUse_e UseItem, GSpellTimer cd, GUnit Monster, string key, bool CloseAdds)
        {
	    if(!cd.IsReady) return false;
	    if(UseItem == ItemUse_e.NoUse) return  false;
	    // Check conditions
	    if(UseItem == ItemUse_e.MyHealth50)
	    {
		if(Me.Health > 0.50) return  false;
	    }
	    else if(UseItem == ItemUse_e.MyHealth25)
	    {
		if(Me.Health > 0.25) return  false;
	    }
	    else if(UseItem == ItemUse_e.MobHealth75)
	    {
		if(Monster.Health < 0.75) return  false;
	    }
	    else if(UseItem == ItemUse_e.MobHealth50)
	    {
		if(Monster.Health < 0.50) return  false;
	    }
	    else if(UseItem == ItemUse_e.SaveForAdds)
	    {
		if(!CloseAdds) return  false;
	    }
	    else if(UseItem == ItemUse_e.Feared)
	    {
		if(Feared.IsReady) return false;
	    }
	    if(Interface.IsKeyReady(key))
	    {		    
		// Use it   
		cd.Reset(); 
		//SendKey(key);
		Context.Log("### USE ITEM " + key );
		CastSpell(key);
		//Thread.Sleep(300);
		return true;
	    }
	    return false;
        }
										   
	
	private GCombatResult PvPCommonResult(GUnit Unit)
	{
	    if(Me.IsDead) return GCombatResult.Died;
	    if(Unit.IsDead) return GCombatResult.Success;
	    if(Unit.DistanceToSelf > 40) return GCombatResult.Retry; // The slag ran off
        if (!Unit.IsValid) return GCombatResult.Bugged;

        GPlayer p = GetClosestPvPPlayer();
        if (p != null && p.DistanceToSelf + 15 < Unit.DistanceToSelf) return GCombatResult.Retry;
        return GCombatResult.Unknown;
	}

        
        private GCombatResult DoOpener(GUnit Monster)
        {

            if(TotemsBeforePull)
            {
                mover.Stop();
                if(WantEarthTotem()) if(DoEarthTotem()) WaitForGCD("PShaman.LightningBolt", 1500); 
                if(WantFireTotem())  if(DoFireTotem()) WaitForGCD("PShaman.LightningBolt", 1500);
                if(WantWaterTotem()) if(DoWaterTotem()) WaitForGCD("PShaman.LightningBolt", 1500);
                if(WantAirTotem())   if(DoAirTotem()) WaitForGCD("PShaman.LightningBolt", 1500); 
            }
            if(WantShield()) if(DoShield()) WaitForGCD("PShaman.LightningBolt", 1500); 

            
            if(!Me.IsMeleeing)
            {
                if(ShouldWhiteDPS())
                {
                    Spam("Start White DPS");
                    SendKey("Common.ToggleCombat"); // Start DPS
                }
            }
            
            if(PullMethod == PullMethod_e.WalkUp)
            {
                // cool
            }
            else
            { 
                mover.Stop();
                bool spellOK = true;
                WaitForMobT.ForceReady();
                if(PullMethod == PullMethod_e.Shock)
                {
                    // TODO: run up to shock distance
                    spellOK = DoDPSShock(); 
                }
                else if(PullMethod == PullMethod_e.Bolt)
                {
                    Spam("Lightning Bolt ");
                    spellOK = CastSpellMana("PShaman.LightningBolt");
                }
                // wait for approach
                    
                if(!spellOK && !WaitForEngage((GMonster)Monster))
                {
                    Spam("  Range pull failed for some reason");
                    if (Monster.DistanceToSelf > PullDistance)  // Wandered out, no biggie.
                    {
                        Spam("  Wandered out of range during pull, will try again later");
                        return GCombatResult.Retry;
                    }
                    
                }
                else
                {
                    WaitForMobT = new GSpellTimer(WaitTime * 1000, false);
                    /*
                    WaitForApproach((GMonster)Monster);
                    if (Monster.DistanceToSelf > PullDistance && Monster.Target != Me )  // Wandered out, no biggie.
                    {
                        Spam("  Wandered out of range during pull, will try again later");
                        return GCombatResult.Retry;
                    }*/
                }
                
            }
            // Do opener moves

            return GCombatResult.Unknown; 
        }


	public override GCombatResult KillTarget(GUnit Target, bool IsAmbush)
	{
	    int ApproachTimeout = 10000;
	//          bool Engaged = false;
	    MyCombatStartLocation = Me.Location;

	    Dismount();
	    this.Target = Target;
	    CloseAdd = null;

            TryPurge = UsePurge; // Do one initial purge

		if (PvPStyle == PvPStyle_e.Active)

            {
                Context.Log("Actively searching for PVP targets");
            }
            if (Target.Name == "Wild Sparrowhawk") return GCombatResult.Bugged;
            if (Target.Name == "Stormpike Bowman") return GCombatResult.Bugged;
	    CombatTimer.Reset();
	    CombatStartHealth = Me.Health;
	    sawAnEvade = false;
	    IsRunning = false;
	    evades = 0;
	    HaveAdd = false;

	    GSpellTimer PullAway = new GSpellTimer(2000, true); // if we try to avoid adds,...
	    GSpellTimer IgnoreBugged = new GSpellTimer(10*1000);

	    PullAway.ForceReady(); // we are trying to move the mob.

	    Context.Log("--- Kill '" + Target.Name + "' lvl " + Target.Level+ " distance "+ (int)Target.DistanceToSelf + (IsAmbush?" Ambush":"") + " ---");

	    if(IsCrybaby(Target.Name))
 		Context.Log("  this monster cries for help, we have to be extra careful"); 

	    bool isUnmovable = IsUnmovable(Target.Name);

	    if(Me.IsInCombat)
	    {
		// hmm i am in combat
		IsAmbush = true; 
	    }

	    if(Context.Party.Mode == GPartyMode.Follower && !Target.IsInCombat && ! IsAmbush) // IsTargetingParty(Target))
	    {
 		//Context.Log("  looks like glider is sending me on a solo mission. No way!"); 
		//return GCombatResult.Retry;
	    }

	    GUnit Monster = (GUnit)Target;
	    if (Target.IsMonster)
	    { 
                if(!IsAmbush)
                {

                  GCombatResult res = DoOpener(Monster); 
                  if(res != GCombatResult.Unknown) return EndCombat(res, Monster);
                }
	    }
	    else
	    {
		if(Target.IsPlayer)
		{
		    Context.Log("Attack player!");
		}
	    }

	    IgnoreBugged.Reset();


	    // Ok, combat is on, have at it:
            GSpellTimer t = new GSpellTimer(0);
	    while (true)            
	    {
                //Context.Log("dt: " + -t.TicksLeft);
                t.Reset();
                if (Monster.IsPlayer
                    && (Monster.DistanceToSelf > 50 || MyCombatStartLocation.GetDistanceTo(Me.Location) > MaxDistanceFromStart))
                {
                    Context.Log("Target ran off ");
                    return EndCombat(GCombatResult.Retry, Monster);
                }

                GUnit changetargetto = null;
                if (Monster.IsPlayer && !Monster.IsInMeleeRange && PvPStyle == PvPStyle_e.Active)
                {
                    GPlayer topwn = GetClosestPvPPlayer();
                    if (Monster != topwn)
                    {
                        Context.Log("Another player is closer '" + topwn.Name + "'");
                        changetargetto = topwn;
                    }
#if PPather
                }
                if (!Monster.IsPlayer)
                {
                    GUnit closestAgressivePlayer = GetClosestPvPPlayerAttackingMe();
                    if (closestAgressivePlayer != null)
                    {
                        // someone is targetting me

                        if ((PvPStyle == PvPStyle_e.Active) || // some slag targetting me
                           (PvPStyle == PvPStyle_e.FightBack && IsPlayerFaction(Monster))) // ..while I attack a pet
                        {
                            Context.Log("Attacking a mob while some jackass is kicking my ass. Thats stupid");
                            changetargetto = closestAgressivePlayer;
                        }
                    }
                }
#endif
 //               if (changetargetto != null && changetargetto != Target)
 //               {
 //                   ChangeToTarget(changetargetto);
 //               }
//
                if (Monster.IsDead) return EndCombat(GCombatResult.Success, Target);
                if (Me.IsDead) return EndCombat(GCombatResult.Died, Target);
                if (Me.Target == null || Me.Target != Target)
                {
                    Context.Log("Lost my target"); // Hmm, this is usually because the mob is dead
                    return EndCombat(GCombatResult.Success, Target);
                }

                GCombatResult CommonResult;
                if (Monster.IsMonster)
                    CommonResult = Context.CheckCommonCombatResult((GMonster)Monster, IsAmbush);
                else
                    CommonResult = PvPCommonResult(Monster);

                if (CommonResult == GCombatResult.Bugged)
                {
                    if (IgnoreBugged.IsReady)
                        return EndCombat(CommonResult, Target);
                }
                else if (CommonResult != GCombatResult.Unknown)
                {
                    return EndCombat(CommonResult, Target);
                }

                if (evades > 5)
                {
                    // evaded 5 attacks, must be bugged
                    Context.Log("Evaded many times now. Must be bugged");
                    return EndCombat(GCombatResult.Bugged, Target);
                }

		isUnmovable = IsUnmovable(Monster.Name);
		if(Monster.IsCasting) AddCaster(Target.Name);
		

		/////////////////////////////////////////////////////////////////
		//
		// first of all check heals

		if(WantManaPotion())  DoManaPotion();

		if(WantHeal())  if(DoHeal()  || Monster.IsDead) continue;


		/////////////////////////////////////////////////////////////////
		//
		// running time

		if(RunFromAddsInCombat && !isUnmovable &&
		   !(ChaseStyle == ChaseStyle_e.Chase && IsRunning))
		{
		    bool crybaby = IsCrybaby(Monster.Name);
		    double avoidDistance = AvoidAddDistance + (crybaby?8:0); 
		    GUnit add = KeepSafeSleep(Monster, 200, 1000,avoidDistance);
		    if(add != null)
		    {
			//Monster.Face();
			PullAway.Reset(); 
			Spam("avoided an add '" + add.Name + "' d " + (int)add.DistanceToSelf + " level " + add.Level);
			GProfile profile = Context.Profile;
			if(profile != null)
			    profile.PlaceBreadcrumb();
			//GUnit addd = FindClosestPotentialAddSmart(Monster, avoidDistance);	    
			//if(addd != null) continue; // we need to move more

                        if(!Me.IsInCombat)
                        {
                            // hmm, avoiding add during approach
                            ForceBolt = true;
                        }
		    }
		    else
		    {
			if(StandingInAoE) // we are inside some AoE effect (posions cloud thunderstom etc)
			{
			    Spam("avoid AoE effect");
			    StepOutOfAoE();
			    PullAway.Reset(); 
			}
		    }
		}
		else
		{
		    Thread.Sleep(200);
		}

		if(Monster.IsDead) return  EndCombat(GCombatResult.Success, Target);

		if(sawAnEvade)
		{
		    Context.Log("Saw an evade!!! Jump");
		    // this is no good
		    mover.MoveRandom();
		    mover.Jump();
		    Thread.Sleep(100);
		    mover.Stop();
		    sawAnEvade = false;
		}
		if(Monster.IsDead) return  EndCombat(GCombatResult.Success, Target);

		HaveCloseAdd = CheckAdditional(Monster); 

		/////////////////
		// Stay alive moves
		    

		/////////////////
		// Chase and Approach logic


                // Check wait for mob status
                if(!WaitForMobT.IsReady &&
                   (Monster.IsInMeleeRange || Monster.IsCasting || IsRanged(Monster.Name) || IsUnmovable(Monster.Name)))
                {
                    WaitForMobT.ForceReady();
                }

		double Distance = Monster.DistanceToSelf;

                Face(Monster, 1000);
		if(IsRunning && Distance <= 20)
		{		
		    if(ShockRunners && Interface.IsKeyReady("PShaman.FrostShock"))
		    {
			Spam("Shock runner");
			CastSpellMana("PShaman.FrostShock", true, true);
		    }
		}
		
                if(IsRunning)
                {		
                    if(ChaseStyle == ChaseStyle_e.Chase)
                    {
                        if(Approach(Monster, false, ApproachTimeout))
                            TweakMelee(Monster);
                    }
                    else if(ChaseStyle == ChaseStyle_e.ChaseSafe)
                    {
                        // Chase if it is safe
                        GUnit add = FindPotentialAdd(Monster);
                        if(add == null)
                        {
                            if(Approach(Monster, true, ApproachTimeout))
                                TweakMelee(Monster);
                        }
                    }
                }
                else
                {
                    if(PullAway.IsReady && WaitForMobT.IsReady)
                    {
                        if(Approach(Monster, false, ApproachTimeout))
                            TweakMelee(Monster);
                        else
                        {
                            Context.Log("Failed approach!");
                            // FUCK! I cant reach the monster
                        }
		    }
		}

		if(Monster.IsDead) return EndCombat(GCombatResult.Success, Target);

		///////////// offensive moves ////// 


		////////
		// Handle runners


		if(IsRunning && true)
		{
		    // Shoot
		    //Spam("Ranged"); 
		    //CastSpell("PShaman.Ranged");x1
		    //continue;
		}


		/////////////////
		// Usable items
		if(!CheckUseItem(UseItem1, UseItem1Timer, Monster, "PShaman.UseItem1", HaveCloseAdd))
		    if(!CheckUseItem(UseItem2, UseItem2Timer, Monster, "PShaman.UseItem2", HaveCloseAdd))
			if(!CheckUseItem(UseItem3, UseItem3Timer, Monster, "PShaman.UseItem3", HaveCloseAdd))
			    CheckUseItem(UseItem4, UseItem4Timer, Monster, "PShaman.UseItem4", HaveCloseAdd);


		/////////////////////////////////////////////////////////////////
		// Special combat moves
		
		if(!Me.IsMeleeing)
		{
		    if(ShouldWhiteDPS())
		    {
			Spam("Start White DPS");
			SendKey("Common.ToggleCombat"); // Start DPS
		    }
		}
		else
		{
		    if(!ShouldWhiteDPS())
		    {
			Spam("Stop White DPS");
			SendKey("Common.ToggleCombat"); // Stop DPS			    
		    }
		}

		GCD = !Interface.IsKeyReady("PShaman.LightningBolt"); // Just to avoid a lot of bar flipping
		if(!GCD)
		{
                    // Heal is important
		    if(WantHeal()) if(DoHeal() || Monster.IsDead) continue;

		    // interrupt casters
		    if(WantInterruptShock()) if(DoInterruptShock() || Monster.IsDead) continue;

                    // Get me some mana!
		    if(WantRage()) if(DoRage() || Monster.IsDead) continue;

		    // Totem popping
		    if(WantEarthTotem()) if(DoEarthTotem() || Monster.IsDead) continue;
		    if(WantFireTotem())  if(DoFireTotem() || Monster.IsDead) continue;
		    if(WantWaterTotem()) if(DoWaterTotem() || Monster.IsDead) continue;
		    if(WantAirTotem())   if(DoAirTotem() || Monster.IsDead) continue;

		    // DPS
		    if(WantStormstrike()) if(DoStormstrike() || Monster.IsDead) continue;
		    if(WantBolt()) if(DoBolt() || Monster.IsDead) continue;
		    if(WantDPSShock()) if(DoDPSShock() || Monster.IsDead) continue;

		    if(WantPurge()) if(DoPurge() || Monster.IsDead) continue;
		    if(WantCurePoison()) if(DoCurePoison() || Monster.IsDead) continue;
		    if(WantCureDisease()) if(DoCureDisease() || Monster.IsDead) continue;
		    if(WantShield()) if(DoShield() || Monster.IsDead) continue;

		    
		}
		else
		{
		    // dont do anything during GCD
		}

	       

	    }
	}
}
	#endregion


            #region PShaman combat helpers
	
            void Spam(string s)
	{
	    if(SpamAlot)
	    {
		Me.Refresh(true);
		if(s == "")
		{
		    return;
		}		
		double t = (double)(-CombatTimer.TicksLeft) / 1000.0; 
		string prefix = String.Format("t {0,4:#0.0} ", t);
		GUnit target = Me.Target;
		if(target != null)
		{		    
		    int mobHealth = (int)(target.Health * 100);
		    prefix += String.Format("{0,3:##0} ", mobHealth);
		    
		    if(target.IsCasting) 
			prefix += "C " + target.CastingID + " " + target.ChannelingSpellID + " ";
		}

		Context.Log(prefix + s);
	    }
	}

        void WaitForCasting()
        {
            GSpellTimer FutileStart = new GSpellTimer(1000, false);
            while (!FutileStart.IsReadySlow && !Me.IsCasting && !Me.IsInCombat);

            GSpellTimer FutileStone = new GSpellTimer(9000, false);
            while (!FutileStone.IsReadySlow && Me.IsCasting && !Me.IsInCombat);
        }

        bool WaitForEngage(GMonster Monster)
	{
            Spam("Wait for engage");
	    GSpellTimer Futile = new GSpellTimer(3000, false);

	    while (!Futile.IsReadySlow) 
	    {
		if (Monster.IsMine || Monster.IsTagged || Monster.TargetGUID == Me.GUID) 
		    return true;

		if(Monster != null)
		{
		    GUnit unit = GObjectList.GetNearestAttacker(Monster.GUID);
		    if(unit != null && unit != Monster)
		    {
			Context.Log("got attacked by a different mob! " + unit.Name);
			return false;
		    }
		    if(IsRanged(Monster.Name) || IsUnmovable(Monster.Name))
		    {
			// What are we doing waiting for him to get here!!!
			return false; 
		    }
		}
	    }
	    return false;
	}

	/*
	 */
	// return value from -PI to PI
	// 
	double BearingToMe(GUnit unit)
	{
	    GLocation MyLocation = Me.Location;
	    float bearing = (float)unit.GetHeadingDelta(MyLocation);
	    return  bearing;
	}




	double DistanceToClosestMine()
	{
	    GNode[] nodes = GObjectList.GetNodes();
	    GNode closest = null;
	    foreach(GNode node in nodes)
	    {
		if(node.IsMineral)
		{
		    if(closest == null || node.DistanceToSelf < closest.DistanceToSelf)
			closest = node;
		}
	    }
	    if(closest == null) return 1E100; 
	    return closest.DistanceToSelf;
	}

	double DistanceToClosestMonsterFrom(GUnit target)
	{
	    GUnit[] Monsters = GObjectList.GetMonsters();
	    double minDist = 1E100; // Far far away

	    foreach (GMonster Add in Monsters)
	    {
		double d = Add.GetDistanceTo(target); 
		if(Add != target && 
		   !Add.IsDead && 
		   d < minDist)
		{
		    minDist = d;
		}		
	    }	 
	    return minDist;
	}


        public bool CheckAdditional(GUnit Target)
	{
	    List<GUnit> adds = FindUnitsAttackingParty();

	    GUnit Extra = null;
	    foreach(GUnit Add in adds)
	    {
		if(Add != Target && 
		   (Extra == null || Add.DistanceToSelf < Extra.DistanceToSelf))
		{
		    Extra = Add;
		}
	    }
	    //GUnit Extra = GObjectList.GetNearestAttacker(Target.GUID);

	    if (Extra == null)   // No extra.
	    {
		return false;
	    }
	    HaveAdd = true;
	    long add = Extra.GUID;
	    if(AddedGUID != add)
		Context.Log("*** New add '" + Extra.Name + "'");
	    AddedGUID = add;

	    if (Extra.DistanceToSelf < Context.MeleeDistance+2)
	    {
		if(CloseAdd == null)
		    Context.Log("*** New close add '" + Extra.Name + "' d: " + Extra.DistanceToSelf);
		    
		CloseAdd = Extra;
		return true;
	    }

	    return false;
	}

        

        public override void Disengage(GUnit Target)
	{
	    base.Disengage(Target);
	    CastSpell("PShaman.IAmNoCoward");
	}


	bool WaitForGCD(string name, int time)
	{
	    //name = "Common.CooldownProbe"; 
	    GSpellTimer timeout = new GSpellTimer(time, false);
	    while(!Interface.IsKeyReady(name) && !timeout.IsReadySlow);
	    return Interface.IsKeyReady(name);
	}


	bool CastSpell(string name)
	{
	    //Context.Log("Spell: " + name);	
            return CastSpell(name, true, false);
	}


        int WaitForManaLoss(int time)
        {
            GSpellTimer timeout = new GSpellTimer(time, false);
            int OldMana = Me.ManaPoints;
            do
            {
                int e = Me.ManaPoints;

                if (e > OldMana)
                {
                    //Spam("reset rage old : " + OldMana + " new " + e);
                    OldMana = e; // Got some rage
                }
                if (e != OldMana)
                {
                    //if((timeout.Duration - timeout.TicksLeft) > 1000)
                    return OldMana - e;
                }
            }
            while (!timeout.IsReadySlow);
            //Spam("  wait for rage loss timed out '"+Context.RedMessage+"'");
            return -1; // no loss
            
        }

	bool CastSpell( String KeyName,   Boolean WaitGCD,   Boolean FastReturn)
        {
            bool ok=  Context.CastSpell(KeyName, WaitGCD, FastReturn);
            if(true) { FSR.Reset();  }
            return ok;
        }

        bool CastOnMemberMana(GPlayer target, string KeyName, GUnit oldTarget)
        {
          bool knownMana = SpellCost.HasKnownCost(KeyName);
            //Spam("CastOther: " + KeyName + " Me mana " + Me.ManaPoints + " cost " + SpellCost.GetCostOfSpell(KeyName));
            if(knownMana  && SpellCost.GetCostOfSpell(KeyName) > Me.ManaPoints)
            {
                Spam("  OOM!!!!");
                oom = true;
                return false;
            }
	    Context.Party.CastOnMember(target, KeyName, oldTarget);
            bool res = true;
            if(!knownMana)
            {
                int cost = WaitForManaLoss(1000);
                if(cost != -1)
                {
                    SpellCost.SetCostOfSpell(KeyName, cost);
                    //Spam(KeyName + " cost " + cost + " Mana");
                }
            }
            return res;            
        }

        bool HasEnoughManaFor(String KeyName)
        {
            if( SpellCost.HasKnownCost(KeyName) && SpellCost.GetCostOfSpell(KeyName) > Me.ManaPoints)
            {
                return false;
            }
            return true;
        }

	bool CastSpellMana(string name)
	{
	    //Context.Log("Spell: " + name);	
            return CastSpellMana(name, true, false);
	}

	bool CastSpellMana( String KeyName,   Boolean WaitGCD,   Boolean FastReturn)
	{
          bool knownMana = SpellCost.HasKnownCost(KeyName);
            //Spam("CastOther: " + KeyName + " Me mana " + Me.ManaPoints + " cost " + SpellCost.GetCostOfSpell(KeyName));
            if(knownMana  && SpellCost.GetCostOfSpell(KeyName) > Me.ManaPoints)
            {
                Spam("  OOM!!!!");
                return false;
            }

	    bool res= Context.CastSpell(KeyName, WaitGCD, FastReturn);
            FSR.Reset();
            
            if(!knownMana)
            {
                int cost = WaitForManaLoss(1000);
                if(cost != -1)
                {
                    SpellCost.SetCostOfSpell(KeyName, cost);
                    //Spam(KeyName + " cost " + cost + " Mana");
                }
            }
            return res;
	}


	void SendKey(string name)
	{	    
	    //Context.Log("Send: " + name);
	    Context.SendKey(name);
	}

        void PressKey(string name)
	{	    
	    //Context.Log("Press : " + name);
	    Context.PressKey(name);
	}

	void ReleaseKey(string name)
	{	    
	    //Context.Log("Release: " + name);
	    Context.ReleaseKey(name);
	}

	#endregion

        #region Keep safe


 

	GUnit FindCloserMonster(GUnit monster)
	{
	    GUnit hostile = FindClosestPotentialAdd(monster, 1E100, monster.Location);
	    //GUnit hostile = GObjectList.GetNearestHostile();
	    if(hostile != null && hostile != monster)
	    {
		double old_d = monster.DistanceToSelf;
		double new_d = hostile.DistanceToSelf;
		if(new_d + 4 < old_d) return hostile; // 4 is some margin to avoid target-swapping
	    }
	    return null; 
	}


	bool IsAnyNeutralNonaggoredClose(double distance)
	{
	    GUnit[] adds = GObjectList.GetMonsters();
	    if(adds.Length == 0) return false;
	    
	    foreach (GUnit Add in adds)
	    {
		if(Add.IsMonster)
		{
		    GMonster gm = (GMonster)Add; 
		    if(gm.DistanceToSelf <= distance &&
		       !gm.IsDead && 
		       gm.Level > 1 && 
		       !gm.IsInCombat && 
		       gm.Reaction != GReaction.Friendly &&
		       !IsStupidItem(gm))
		    {
			//Context.Log(gm.Name + " will aggro if we AoE ("+gm.DistanceToSelf+" < "+distance+")");
			return true; // this one will aggro if we AoE
		    }
		}
	    }
	    return false;
	}

	GUnit FindPotentialAdd(GUnit target)
	{
	    GUnit h = FindClosestPotentialAdd(target, AvoidAddDistance, target.Location);
	    return h;
	}

	bool HasAddPotential(GUnit target)
	{
	    return FindPotentialAdd(target) != null;
	}


	GUnit FindClosestPotentialAdd(GUnit target, double distance, GLocation here)
	{
	    GUnit[] adds = GObjectList.GetMonsters();
	    if(adds.Length == 0) return null;
	    
	    GUnit closestgm = null;

	    foreach (GUnit Add in adds)
	    {
		if(Add.IsMonster)
		{
		    GMonster gm = (GMonster)Add; 
		    GLocation loc = PredictedLocation(gm);
		    if(gm != target &&
		       loc.GetDistanceTo(here) < distance && 
		       !gm.IsDead && 
		       !gm.IsTargetingMe && !IsTargetingParty(gm) && 
		       gm.Level > (Me.Level-20) &&
		       gm.Level > 1 && 
		       gm.Reaction == GReaction.Hostile &&
		       !gm.IsTagged &&
		       Math.Abs(gm.Location.Z- target.Location.Z) < ZMax &&
		       !IsStupidItem(gm))
		    {
			if(closestgm == null || loc.GetDistanceTo(here) < closestgm.GetDistanceTo(here))
			{
			    closestgm = gm;
			}
		    }
		}
	    }
	    return closestgm;
	}

	/*
	  1 - location is front
	  2 - location is right
	  3 - location is back
	  4 - location is left
        */
	int GetLocationDirection(GLocation loc)
	{
	    int dir = 0;
	    double b = loc.Bearing;
	    if(b > -PI/4  && b <= PI/4)  // Front
	    {
		dir= 1;
	    }
	    if(b > -3*PI/4 && b <= -PI/4) // Left
	    {
		dir =  4; 
	    }
	    if(b <= -3*PI/4 || b > 3*PI/4) //  Back   
	    {
		dir =  3;
	    }
	    if(b > PI/4  && b <= 3*PI/4) //  Right  
	    {
		dir =  2; 
	    }
	    if(dir == 0)
		Context.Log("Odd, no known direction");
	    
	    return dir;
	}


	bool IsTargetingParty(GUnit unit)
	{
	    if(unit.IsTargetingMe) return true;
	    long [] party = Context.Party.GetPartyMembers();

            // Check totems
            GUnit target = unit.Target;
            if(target != null)                          
            {
                long creator = target.CreatedBy;
                if(creator == Me.GUID) return true;
                if(Array.IndexOf(party, creator) >= 0)
                    return true;
            }
            
	    foreach(long player in party)
	    {
		if(unit.TargetGUID == player) return true;
	    }
	    return false;
	}


        bool AnyAttackerHasTendency(string tendency)
        {
            List<GUnit> attackers = FindUnitsAttackingParty();
            foreach(GUnit u in attackers)
            {
                if(MobTendencies.MobHasTendency(u.Name, tendency))
                    return true;
            }
            return false;            
        }

	List<GUnit> FindUnitsAttackingParty()
	{
	    GUnit[] adds = GObjectList.GetMonsters();

	    List<GUnit> mobs = new List<GUnit>();
	    foreach (GUnit Add in adds)
	    {
		if(Add.IsMonster)
		{
		    if(IsTargetingParty(Add))
		    {
			//Context.Log("Targeting party: " + Add);
			mobs.Add(Add);
		    }
		}
	    }	    
	    //if(mobs.Size == 0) mobs = null;
	    return mobs;
	}

	GUnit FindClosestPotentialAddSmart(GUnit target, double distance)
	{
	    GUnit closestAdd = FindClosestPotentialAdd(target, distance, Me.Location); 
	    return closestAdd;
	}

	/*
	  returns 
	  0 - no add
	  1 - potential add front
	  2 - potential add right
	  3 - potential add back
	  4 - potential add left
	*/
	int AddDirection(GUnit closestAdd, double distance)
	{
	    if(closestAdd == null) return 0;

	    double b = closestAdd.Bearing;

	    //  Front  b > -PI/4  && b < PI/4 
	    //  Left   b > -3PI/4 && b < -PI/4
	    //  Back   b < -3PI/4 || b > 3PI/4
	    //  Right  b > PI/4   && b < 3PI/4

	    string[] dirName = { "-", "in front of me", "right of me", "behind me", "left of me" };

	    int dir = 0;

	    if (true) //closestAdd.DistanceToSelf < distance)
	    {

		dir =GetLocationDirection(closestAdd.Location);
		if(dir == 0)
		    Context.Log("Odd, no known direction");
		//Context.Log("    '" + closestAdd.Name + "' is " + dirName[dir] + " distance " + (int)closestAdd.DistanceToSelf);
	    }

	    return dir;
	}

	GUnit KeepSafeSleep(int time)
	{
	    return KeepSafeSleep(null, time, time);
	}

	GUnit KeepSafeSleep(int safeTime, int unsafeTime)
	{
	    return KeepSafeSleep(null, safeTime, unsafeTime);
	}

	GUnit KeepSafeSleep(GUnit ignore, int safeTime, int unsafeTime)
	{
	    return KeepSafeSleep(ignore, safeTime, unsafeTime, AvoidAddDistance);
	}
        GUnit KeepSafeSleep(GUnit ignore, int safeTime, int unsafeTime, double distance)
        {
            GUnit add = FindClosestPotentialAddSmart(ignore==null?Me:ignore, distance);
            if (add != null)
            {
                if (ignore == null) ignore = add;
                GSpellTimer t = new GSpellTimer(unsafeTime);
                while (!t.IsReady)
                {
                    double heading = Me.Location.GetHeadingTo(add.Location);
                    heading += Math.PI; if (heading > PI * 2) heading -= 2 * PI;
                    GLocation dst = InFrontOf(Me.Location, heading, 50.0);
                    bool moved = mover.moveTowardsFacing(Me, dst, 0, ignore.Location);

                    Thread.Sleep(50);
                }
                mover.Stop();
            }
            else
                Thread.Sleep(safeTime);

            return add;
        }
        

	#endregion 


            #region Dont stand/run  in campfires


            void avoidRunInto(GLocation loc)
	{
            // something damgerous is close
            // where is it
            double heading = Me.Location.GetHeadingTo(loc);
            double bearing = Me.Heading - heading;
            if(bearing > PI) bearing -= 2*PI;
            if(bearing < -PI) bearing += 2*PI;
            if(bearing <=0 && bearing >-PI/2) 
            {  // left of me
                mover.StrafeRight(true);
            }
            if(bearing  > 0 && bearing < PI/2) 
            {  // right of me
                mover.StrafeLeft(true);
            }
            // lets hope some other move-code stops our strafing!	    
	}


	private GNode LastAvoidedNode = null;
	void avoidRunIntoCampfires()
	{
	    GNode[] nodes = GObjectList.GetNodes();
	    GNode fireNode = null;
	    foreach(GNode node in nodes)
	    {
		if(node.DistanceToSelf < 5 && IsBurner(node.Name))
		{
		    fireNode = node;
		}
	    }	    
	    if(fireNode != null)
	    {
		GLocation loc =fireNode.Location;
		avoidRunInto(loc);
		if(LastAvoidedNode == null || LastAvoidedNode != fireNode)
		    Context.Log("Avoid run into fire " + fireNode.Name);
		LastAvoidedNode = fireNode;
	    }
	}

	int lastRandomJump; 
	bool chooseNewRandom = true;
	void RandomJump()
	{
	    if(chooseNewRandom)
	    {
		lastRandomJump = random.Next(2);
	    }
	    chooseNewRandom = !chooseNewRandom; // jump same direction twice
	    switch(lastRandomJump)
	    {
	    case 0:
		mover.StrafeRight(true);
		mover.Jump();
		Thread.Sleep(100);
		mover.StrafeRight(false);
		break; 
	    case 1:
		mover.StrafeLeft(true);
		mover.Jump();
		Thread.Sleep(100);
		mover.StrafeLeft(false);
		break; 
	    }	    
	}

	void StepOutOfAoE()
	{
	    if(NoJumpFromAoE) 
	    {
		StandingInAoE = false; // Clear
		return;
	    }
	    RandomJump();
	    StandingInAoE = false; // Clear
	}

        void StepOutOfFire()
	{
	    bool found = false;
	    GNode[] nodes = GObjectList.GetNodes();
	    GNode fireNode = null;
	    foreach(GNode node in nodes)
	    {
		if(node.DistanceToSelf < 8 && (node.Name.Contains("fire")))
		{
		    fireNode = node;
		    found = true;
		}
	    }
	    if(found)
	    {
		Context.Log("Ouch! I stepped in '" +fireNode.Name+ "'");
		AddBurner(fireNode.Name);

		RandomJump();
	    }
            else
            {
                
            }
	      
	} 

	#endregion

	    // A DOT on me   20:14 You suffer 16 Nature damage from Highperch Wyvern's Poison.
	    // Throw on me   19:37 Vilebranch Axe Thrower's Throw hits you for 85.
	    // Campfire      16:38 You suffer 13 points of fire damage.
	    // Poison        23:36 Cursed Ooze is afflicted by Deadly Poison IV (2).
	    //               15:35 Northspring Slayer is afflicted by Crippling Poison.
	    // 

	    //Combat Log watcher


            void Context_CombatLog(string RawTextOriginal)
	{
	    if(Context.CurrentMode != GGlideMode.Glide && Context.CurrentMode != GGlideMode.OneKill) return; 
	    //Context.Log(" CB: " + RawTextOriginal);
	    string RawText = RawTextOriginal.ToLower();

	    if (RawText.Contains("you are drowning"))
	    {
                // !!!!!!  This is bad !!!!!
		Context.Log("I AM DROWNING!!!!");
		mover.SwimUp(true);
		Thread.Sleep(1000);
		mover.SwimUp(false);
		
	    }
	    if(RawText.Contains("you are afflicted") || RawText.Contains("you suffer"))
	    {
		if(RawText.Contains("spore cloud") || RawText.Contains("chemical flames") || RawText.Contains("flames wave"))
		{
		    StandingInAoE = true; // Someone should move us!!
		}
	    }

            if(RawText.Contains("is immune"))
            {
                // something is immune to something
//11:54 Your Lightning Bolt failed. Charlga Razorflank is immune.
//09:45 Your Lightning Bolt failed. Swirling Vortex is immune.
//15:23 Your Fiery Weapon failed. Searing Infernal is immune.
//11:35 Your Rend failed. Molten Elemental is immune.
                int start = RawText.IndexOf("failed. ");
                int end   = RawText.IndexOf(" is immune.");
                if(start != -1 && end != -1)
                {
                    start += 8; // skip "failed. "
                    string monster = RawTextOriginal.Substring(start, end-start);
                    if(RawText.Contains("your lightning bolt") ||
                       RawText.Contains("your earth shock") ||
                       RawText.Contains("your lightning shield"))
                    {
                        // nature
                        Context.Log("'" + monster + "' is immune to nature");
                        AddNatureImmune(monster);
                    }
                    if(RawText.Contains("your flame shock"))
                    {
                        // fire
                        Context.Log("'" + monster + "' is immune to fire");
                        AddFireImmune(monster);
                    }
                    if(RawText.Contains("your frost shock"))
                    {
                        // frost
                        Context.Log("'" + monster + "' is immune to frost");
                        AddFrostImmune(monster);
                    }
                }

                   
              
            }
	    if (RawText.Contains("you suffer"))
	    {
		if(!RawText.Contains("from") && RawText.Contains("fire"))
		{
		    // hmm, stading in a fire?!
		    StepOutOfFire();
		}

	    } 
	    else if (RawText.Contains("fissure's consumption hits you"))
	    {
		RandomJump(); // Very very bad!
            }
	    else if(RawText.Contains("evade"))
	    {
		if(!RawText.Contains("was evaded.")) // hmmm, I evade stuff sometimes
		{
		    sawAnEvade = true;
		    evades ++;
		    Context.Log("Evade #" + evades);
		}
	    }
	    else if(RawText.Contains("hits you") ||
		    RawText.Contains("crits you"))
	    {
		
		int i = RawTextOriginal.IndexOf("\'s");
		if(i != -1)
		{
                    string MonsterName = RawTextOriginal.Substring(0, i);

                    if(RawText.Contains("throw") ||
                       RawText.Contains("shoot"))
                    {
                        // Ranged mob...
                        AddRanged(MonsterName);
                    }
                    if(Me.Target != null && Me.Target.Name == MonsterName)
                    {
                        // my target
                        if(Me.Target.DistanceToSelf > Context.MeleeDistance + 2)
                        {
                            Spam(Me.Target.Name + " hit me from distance " + Me.Target.DistanceToSelf);
                            //if(IsCaster(Me.Target.Name))
                            if(IsCaster(Me.Target.Name) ||
                               IsRanged(Me.Target.Name))
                                AddUnmovable(Me.Target.Name);
                        }
                    }
		}
	    }
	    else if(RawText.Contains("gains"))
            {
                // Someone gained something, lets see if it is my target
                if(Me.Target != null)
                {
                    if(UsePurge && PurgeOnGain)
                    {
                        int i = RawTextOriginal.IndexOf(" gains ");
                        if(i != -1)
                        {
                            string MonsterName = RawTextOriginal.Substring(0, i);
                            if(MonsterName == Me.Target.Name)
                            {
                                //Spam("  " + 
                                Spam("  "+ RawTextOriginal);
                                //Context.Log("My target, '" + MonsterName + "' gained an effect. PurgeIt");
                                TryPurge = true;
                            }
                        }
                    }                    
                }
            }
	    else if(RawText.Contains("removed by"))
            {
                if(RawText.Contains("purge"))
                {
                    Spam("  "+ RawTextOriginal);                    
                }
            }


            if (RawText.Contains("you are afflicted by") && 
		(RawText.Contains("terror") ||
		 RawText.Contains("fear") ||
		 RawText.Contains("terrify") ||
		 RawText.Contains("psychic scream") ||
		 RawText.Contains("shriek") ||
		 RawText.Contains("charm") ||
		 RawText.Contains("sleep") ||
		 RawText.Contains("charm") ||
		 RawText.Contains("sleep") ||
		 RawText.Contains("seduce") ||
		 RawText.Contains("slumber")))
	    {
		if(Me.Target != null) AddFearer(Me.Target.Name);
		Spam("Got feared/charmed/sleeped.");
		Feared.Reset(); 
	    }

            if (RawText.Contains("fades from you"))
            {
                if (RawText.Contains("terror") ||
                    RawText.Contains("fear") ||
                    RawText.Contains("terrify") ||
                    RawText.Contains("psychic scream") ||
                    RawText.Contains("shriek"))
                {
		    Spam("Fear Ran out.");
		    Feared.ForceReady(); 
                }
            }

	    if(RawText.Contains("your"))
            {
                if(RawText.Contains("shock") || RawText.Contains("stormstrike") || RawText.Contains("lightning bolt"))
                {
                    if(!RawText.Contains("suffers") && !RawText.Contains("heals"))
                        Spam("  "+ RawTextOriginal);
                }
                if(RawText.Contains("healing wave") ||RawText.Contains("chain heal"))
                {
                    Spam("  "+ RawTextOriginal);                    
                }
            }
	}

        void Context_ChatLog(string RawText, string ParsedText)
	{
	    if(Context.CurrentMode != GGlideMode.Glide && Context.CurrentMode != GGlideMode.OneKill) return; 
	    if(ParsedText.Contains("attempts to run away") ||
	       ParsedText.Contains("senses danger and flees"))
	    {
		int i = ParsedText.IndexOf(" attempts"); 
		if(i != -1)
		{
		    string MonsterName = ParsedText.Substring(0, i);
		    AddRunner(MonsterName);			
		    Context.Log(MonsterName + " attempts to run away");
		    IsRunning = true;
		}
	    }
	    else if(ParsedText.Contains("calls for help") ||
		    ParsedText.Contains("lets out a shriek, calling for help"))
	    {
		int i = ParsedText.IndexOf(" calls"); 
		if(i == -1) i = ParsedText.IndexOf(" lets out");
		if(i != -1)
		{
		    string MonsterName = ParsedText.Substring(0, i);
		    AddCrybaby(MonsterName);			
		    Context.Log(MonsterName + " calls for help");
		}
	    }
	    else if(ParsedText.Contains("mount up"))
            {
                if(!IsMounted())
                    forceMount = true;
            }
	}


	#region Mob tendencies
        
            bool IsRanged(string Name) { return MobTendencies.MobHasTendency(Name, "ranged");	}
        void AddRanged(string Name){ MobTendencies.AddTendency(Name, "ranged");}

        bool IsCaster(string Name) { return MobTendencies.MobHasTendency(Name, "caster");	}
        void AddCaster(string Name){ MobTendencies.AddTendency(Name, "caster");}

        bool IsUnmovable(string Name) { return MobTendencies.MobHasTendency(Name, "unmovable");	}
        void AddUnmovable(string Name){ MobTendencies.AddTendency(Name, "unmovable");}


        bool IsPoisoner(string Name) { return MobTendencies.MobHasTendency(Name, "poisoner");	}
        void AddPoisoner(string Name){ MobTendencies.AddTendency(Name, "posioner");}

        bool IsDiseaseer(string Name) { return MobTendencies.MobHasTendency(Name, "diseaser");	}
        void AddDiseaseer(string Name){ MobTendencies.AddTendency(Name, "diseaser");}

        bool IsFearer(string Name) { return MobTendencies.MobHasTendency(Name, "fearer");	}
        void AddFearer(string Name){ MobTendencies.AddTendency(Name, "fearer");}

        bool IsCrybaby(string Name) { return MobTendencies.MobHasTendency(Name, "crybaby");	}
        void AddCrybaby(string Name){ MobTendencies.AddTendency(Name, "crybaby");}

        bool IsRunner(string Name) { return MobTendencies.MobHasTendency(Name, "runner");	}
        void AddRunner(string Name){ MobTendencies.AddTendency(Name, "runner");}

        bool IsNatureImmune(string Name) { return MobTendencies.MobHasTendency(Name, "natureimmune");	}
        void AddNatureImmune(string Name){ MobTendencies.AddTendency(Name, "natureimmune"); }

        bool IsFireImmune(string Name) { return MobTendencies.MobHasTendency(Name, "fireimmune");	}
        void AddFireImmune(string Name){ MobTendencies.AddTendency(Name, "fireimmune"); }

        bool IsFrostImmune(string Name) { return MobTendencies.MobHasTendency(Name, "frostimmune");	}
        void AddFrostImmune(string Name){ MobTendencies.AddTendency(Name, "frostimmune"); }


        #endregion

            #region Things that burns
        
            List<string> BurningThings = new List<string>(); // Gief set class plx

        bool IsBurner(string Name)
	{
	    return BurningThings.Contains(Name);
	}


        void AddBurner(string Name)
	{
	    if(IsBurner(Name)) return; 
	    BurningThings.Add(Name); 
	}
	#endregion


            #region RepairAndSell


            /*
              This will not work if you have "holes" in your bag slots. All bags must be packed to the right 
            */
            bool SellStuff(int bagNr, bool JustCheck)
	{
	    // cb=0 default bag
	    // cb=1 bar #1 ...
	    long[] AllBags = GPlayerSelf.Me.Bags;
	    long[] Contents;
	    int SlotCount;
	    bool SellAnything = false;
	    if(bagNr == 0)
	    {
		Contents = Me.BagContents;
		SlotCount = 16;
	    }
	    else
	    {
		GContainer bag = (GContainer) GObjectList.FindObject(AllBags[bagNr-1]);
		if (bag != null)
		{
		    Contents = bag.BagContents;
		    SlotCount = bag.SlotCount;
		}
		else
		    return false;
	    }

	    for (int i = 0; i < Contents.Length; i++)
	    {
		bool Skip = false;
		if (Contents[i] == 0)
		    continue;
		
		GItem CurItem = (GItem)GObjectList.FindObject(Contents[i]);
		//Context.Log("Checking: " + CurItem.Name);
		string ItemName = CurItem.Name.ToLower();
		foreach ( string ProtItem in ProtItems )
		{
		    if (ProtItem != "" && ItemName.Contains(ProtItem) )
		    {
			//Context.Log("Not Selling Item: " + CurItem.Name + " Reason=\"Is on safe list\"");
			Skip = true;
			break;
		    }
		}
		if ((CurItem.Definition.Quality == GItemQuality.Poor && !SellPoor) ||
		    (CurItem.Definition.Quality == GItemQuality.Common && !SellCommon) ||
		    (CurItem.Definition.Quality == GItemQuality.Uncommon && !SellUncommon) ||
		    (CurItem.Definition.Quality == GItemQuality.Rare && !SellRare) ||
		    (CurItem.Definition.Quality == GItemQuality.Epic) ||
		    (CurItem.Definition.Quality == GItemQuality.Legendary) ||
		    (CurItem.Definition.Quality == GItemQuality.Artifact ))
		{
		    //Context.Log("Not Selling Item: " + CurItem.Name + " Reason=\"Quality "+CurItem.Definition.Quality+" is not to be sold\"");
		    Skip = true;
		}
		
		//If we got here, we plan on selling the item
		if (!Skip)
		{
		    Context.Log("  Will sell item: " + CurItem.Name);
		    SellAnything = true;
		    if(!JustCheck)
		    {
			GInterfaceObject CurItemObj = Context.Interface.GetByName("ContainerFrame" + (bagNr+1) + "Item" + (SlotCount - i));
			CurItemObj.ClickMouse(true);
			Thread.Sleep(500);
		    }
		    //else
		    //    Context.Log("Not selling Item: " + CurItem.Name);
		    
		}
	    }	    
	    return SellAnything; 
	}
	bool CheckBags(bool JustCheck)
	{
	    bool sell = false;
	    for(int i = 0 ; i<5; i++)
	    {
		if(SellStuff(i, JustCheck))
		    sell = true;
	    }
	    if(!sell)
		Context.Log("  nothing to sell");
	    return sell;
	}
	
	void SellAndRepair(GUnit guy, bool JustCheck)
        {
	    
            // Only for Glider v1.5
            guy.Approach(3.0);   // Get extra close to make sure.  
            guy.Interact();

            if (GPlayerSelf.Me.Target != guy)
            {
                GContext.Main.Log("Never managed to click on vendor");
                return;
            }

            GMerchant Merchant = new GMerchant();

            if ( Merchant.IsVisible )
            {
		if(UseRepair)
		{
		    if ( Merchant.IsRepairEnabled )   // Might as well fix it up while we're here.  
		    {
			Context.Log("  Reparing");
			Merchant.ClickRepairButton();
		    }
		}
		if(UseSell)
		{
                
		    for (int b = 0; b < 4; b++)
		    {
			GInterfaceObject CurBag = Context.Interface.GetByName("CharacterBag" + b + "Slot");
			if (CurBag != null && !JustCheck)
			{
			    CurBag.ClickMouse(false);
			    Thread.Sleep(100);
			}
		    }
            
		    CheckBags(JustCheck);
		}
                Merchant.Close();}
	     
        }
	
	#endregion

            #region SpellCost

            private class SpellCostTracker
            {
                private Dictionary<string, int> Costs = new Dictionary<string, int>();
                private Dictionary<string, int> SeenTimes = new Dictionary<string, int>();

                public void Clear()
                    {
                        Costs = new Dictionary<string, int>();
                        SeenTimes = new Dictionary<string, int>();
                    }

                public bool HasKnownCost(string key)
                    {
                        int cost = GetCostOfSpell(key);
                        if(cost == -1) return false;
                        return true;
                    }
            
                // returns -1 if unknown
                public int GetCostOfSpell(string key)
                    {
                        int times = 0;
                        if (SeenTimes.TryGetValue(key, out times))
                        {
                            int cost = -1;
                            if (times >= 5 && Costs.TryGetValue(key, out cost))
                            {
                                return cost;
                            }
                        }
                        return -1;                
                    }
            
                private int GetCostInternal(string key)
                    {
                        int cost = -1;
                        if (Costs.TryGetValue(key, out cost))
                        {
                            return cost;
                        }
                        return -1;
                    }

                private void SetCostInternal(string key, int cost)
                    {
                        int old_cost = -1;
                        if(Costs.TryGetValue(key, out old_cost))
                        {
                            Costs.Remove(key);
                        }
                        Costs.Add(key, cost);
                    }

                public void SetCostOfSpell(string key, int cost)
                    {
                        int times = 0;
                        if (SeenTimes.TryGetValue(key, out times))
                        {
                            int old_cost = GetCostInternal(key);
                            int new_cost;
                            new_cost = old_cost;
                            if(cost > old_cost) new_cost = cost; 

                            /*if(old_cost == -1)
                                new_cost = cost;
                            else
                              new_cost = cost + old_cost)/2;
                            */

                            GContext.Main.Log("Spell " + key + " is set for the " + (times+1) +" time. cost: " + cost  + " old: " + old_cost + " new: " + cost);

                            SetCostInternal(key, new_cost);                        
                            if (SeenTimes.TryGetValue(key, out times))
                            {
                                SeenTimes.Remove(key);
                                SeenTimes.Add(key, times+1);
                            }
                            else
                                SeenTimes.Add(key, 1); // should never be here...
                        }
                        else
                        {
                            GContext.Main.Log("Spell " + key + " is set for the " + (1) +" time. new: " + cost);
                            SeenTimes.Add(key, 1);
                            Costs.Add(key, cost);
                        }
                    }
            
            }

	#endregion

            #region TendencyManager

            private class TendencyManager
            {

                // Class to manage tendency of one mob
                private class MobTendency
                {
                    private string Name;
                    private List<string> TendencyList = new List<string>();
                    private bool locked = false;

                    // create and decode a line from the tendency file
                    // Syntax is "mobname:tendeny(,tendency)*"
                    public MobTendency(string Line)
                        {
                            //split it
                            char[] splitter = { ':', ',' };
                            string[] fields = Line.Split(splitter);

                            if (fields != null && fields.Length > 1)
                            {
                                Name = fields[0];

                                for (int x = 1; x < fields.Length; x++)
                                {
                                    AddTendency(fields[x]);
                                }
                            }
                        }

                    public MobTendency(string MobName, string InitialTendency)
                        {
                            Name = MobName;
                            AddTendency(InitialTendency);
                        }

                    public string MobName
                        {
                            get { return Name; }
                        }

                    public bool HasTendency(string Tendency)
                        {
                            return TendencyList.Contains(Tendency);
                        }

                    public bool AddTendency(string Tendency)
                        {
                            if (locked) return false;
                            if (Tendency == "") return false;
                            if (Tendency == "locked")
                            {
                                locked = true;
                                //GContext.Main.Log(MobName + " is locked");
                            }
                            bool retval;
                            if (!HasTendency(Tendency))
                            {
                                TendencyList.Add(Tendency);
                                //GContext.Main.Log(MobName + " is '" + Tendency + "'");
                                retval = true;
                            }
                            else
                            {
                                retval = false;
                            }
                            return retval;
                        }

                    public override string ToString()
                        {
                            StringBuilder x = new StringBuilder();

                            x.Append(Name + ":");

                            for (int idx = 0; idx < TendencyList.Count; idx++)
                            {
                                x.Append(TendencyList[idx]);
                                if (idx < TendencyList.Count - 1)
                                    x.Append(",");
                            }
                            if (locked)
                                x.Append(",locked");

                            return x.ToString();
                        }
                }
                private Dictionary<string, MobTendency> MobList = new Dictionary<string, MobTendency>();

                public bool SaveToFile()
                    {
                        return SaveToFile("Classes/MobTendencies.txt");
                    }

                public bool SaveToFile(string FileName)
                    {
                        bool retval = false;

                        try {
                            if (MobList != null)
                            {
                                System.IO.StreamWriter fileout = System.IO.File.CreateText(FileName);
                        
                                if (fileout != null)
                                {
                                    retval = true;
                            
                                    foreach (KeyValuePair<string, MobTendency> kvp in MobList)
                                    {
                                        MobTendency x = kvp.Value;
                                        fileout.WriteLine(x.ToString());
                                    }
                            
                                    fileout.Flush();
                                    fileout.Close();
                            
                                }
                                else
                                {
                                    retval = false;
                                }
                        
                            }
                            else
                            {
                                retval = false;
                            }
                        } catch(Exception e)
                        {
                            GContext.Main.Log("Failed to save '" + FileName + "'"); 
                            GContext.Main.Log(""+e);
                        }

                        return retval;


                    }


                public bool LoadFromFile()
                    {
                        return LoadFromFile("Classes/MobTendencies.txt");
                    }

                public bool LoadFromFile(string FileName)
                    {

                        //System.IO.StreamWriter fileout = System.IO.File.CreateText(FileName);
                        bool retval = true;
                        MobList.Clear();

                        try 
                        {
                            if (FileName != "" && System.IO.File.Exists(FileName))
                            {
                                System.IO.StreamReader filein = System.IO.File.OpenText(FileName);
                        
                                if (filein != null)
                                {
                                    //read the lines of the file....
                            
                                    while (!filein.EndOfStream)
                                    {
                                        try
                                        {
                                            MobTendency t = new MobTendency(filein.ReadLine());
                                            MobList.Add(t.MobName, t);
                                        }
                                        catch
                                        {
                                            //error parsing the line...
                                        }
                                    }
                            
                                    filein.Close();
                                }
                                else
                                {
                                    retval = false;
                                }
                        
                        
                            }
                            else
                            {
                                retval = false;
                            }
                        } catch(Exception e)
                        {
                            GContext.Main.Log("Failed to load '" + FileName + "'"); 
                            GContext.Main.Log(""+e);
                        }
                        //Context.Log("Loaded info for " + MobList.Count + " mobs");

                        return retval;
                    }

                public bool KnownMob(string MobName)
                    {
                        return MobList.ContainsKey(MobName);
                    }

                public int GetMobCount()
                    {
                        return MobList.Count;
                    }

                public bool MobHasTendency(string MobName, string Tendency)
                    {
                        MobTendency x;
                        if (MobList.TryGetValue(MobName, out x))
                        {
                            return x.HasTendency(Tendency);
                        }
                        return false;
                    }

                public bool AddTendency(string MobName, string Tendency)
                    {
                        MobTendency x;
                        if (MobList.TryGetValue(MobName, out x))
                        {
                            if (x.HasTendency(Tendency) || x.AddTendency(Tendency))
                                return true;
                        }
                        else
                        {
                            MobList.Add(MobName, new MobTendency(MobName, Tendency));
                        }
                        return false;
                    }
            }
        #endregion


        public GLocation InFrontOf(GLocation loc, double heading, double d)
        {
            double x = loc.X;
            double y = loc.Y;
            double z = loc.Z;

            x += Math.Cos(heading) * d;
            y += Math.Sin(heading) * d;

            return new GLocation((float)x, (float)y, (float)z);
        }

	GPlayer GetClosestPvPPlayer()
	{
	    GPlayer[] plys = GObjectList.GetPlayers();
	    GPlayer ClosestPlayer = null;

	    foreach(GPlayer p in plys)
	    {
		if(!p.IsSameFaction)
		{
		    if(ClosestPlayer == null || p.GetDistanceTo(Me) < ClosestPlayer.GetDistanceTo(Me))
			ClosestPlayer = p;
		}
	    }
	    return ClosestPlayer;
	}

	GPlayer GetClosestFriendlyPlayer()
	{
	    GPlayer[] plys = GObjectList.GetPlayers();
	    GPlayer ClosestPlayer = null;

	    foreach(GPlayer p in plys)
	    {
		if(p.IsSameFaction && p!=Me)
		{
		    if(ClosestPlayer == null || p.GetDistanceTo(Me) < ClosestPlayer.GetDistanceTo(Me))
			ClosestPlayer = p;
		}
	    }
	    return ClosestPlayer;
	}

	void TweakMelee(GUnit Monster)
	{
	    double Distance = Monster.DistanceToSelf;
	    double sensitivity = 2.5; // default melee distance is 4.8 - 2.5 = 2.3, no monster will chase us at 2.3
	    double min = Context.MeleeDistance - sensitivity;
	    if(min < 1.0) min = 1.0;

	    if(Distance > Context.MeleeDistance)
	    {
		// Too far
		//Spam("Tweak forwards. "+ Distance + " > " + Context.MeleeDistance);
		mover.Forwards(true);
		Thread.Sleep(100); 
		mover.Forwards(false);
	    }
	    else if(Distance < min)
	    {
		// Too close
		//Spam("Tweak backwards. "+ Distance + " < " + min);
		mover.Backwards(true);
		Thread.Sleep(200); 
		mover.Backwards(false);
	    }
	}

       GLocation PredictedLocation(GUnit mob)
	{
	    GLocation currentLocation = mob.Location;
	    double x = currentLocation.X; 
	    double y = currentLocation.Y; 
	    double z = currentLocation.Z; 
	    double heading = mob.Heading;
	    double dist = 4;

	    x += Math.Cos(heading) * dist;
	    y += Math.Sin(heading) * dist;
	   
	    GLocation predictedLocation = new GLocation((float)x, (float)y, (float)z);

	    GLocation closestLocatition = currentLocation;
	    if(predictedLocation.DistanceToSelf < closestLocatition.DistanceToSelf)
		closestLocatition = predictedLocation;
	    return closestLocatition;
	}

/*
  GUID=f0013c1800010c68,BaseAddr=1b0e9b70,Type=Monster,Name="OLDWorld Trigger (DO NOT DELETE)", Culled=False
*/
	bool IsStupidItem(GUnit unit)
	{
	    // Filter out all stupid sting found in outland
	    string name = unit.Name.ToLower();
	    if(name.Contains("target") || name.Contains("trigger") || name.Contains("infernal rain")  || name.Contains("anilia") ||
               name.Contains("earthbind") || name.Contains("moonflare"))
		return true;
	    return false;
	}


	bool IsInFrontOfMe(GUnit unit)
	{
	    double bearing = unit.Bearing;
	    return bearing < PI/2.0 && bearing > -PI/2.0;	    
	}

        bool Face(GUnit monster, int timeout)
        {
            double tolerance = PI/8;

            GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
            if(Math.Abs(monster.Bearing)< tolerance)  return true;            
            do
            {
                if (Me.IsDead || monster.IsDead) { mover.Stop(); return false; }

                double b = monster.Bearing;
                if(b < -tolerance)
                {
                    // to the left
                    mover.RotateLeft(true);
                }
                else if(b > tolerance)
                {
                    // to the rigth
                    mover.RotateRight(true);
                }
                else
                {
                    // ahead
                    mover.Stop(); 
                    return true;
                }
                
            } while (!approachTimeout.IsReadySlow);

            mover.Stop();
            Context.Log("Face timed out");
            return false;

        }

	bool Approach(GUnit monster, bool AbortIfUnsafe)
	{
	    return Approach(monster, AbortIfUnsafe, 10000);
	}

	bool Approach(GUnit monster, bool AbortIfUnsafe, int timeout)
	{
	    if(monster.DistanceToSelf < Context.MeleeDistance &&
               Math.Abs(monster.Bearing)< PI/8)  return true;
	    //Context.Log("Approach " + monster.Name + " d: " + monster.DistanceToSelf);

	    GSpellTimer approachTimeout = new GSpellTimer(timeout, false);
	    StuckDetecter sd = new StuckDetecter(Me, Context, 1, 5);
	    GSpellTimer t = new GSpellTimer(0);
	    bool doJump = random.Next(4) == 0;
            bool WasInCombat = GContext.Main.Me.IsInCombat;
	    do 
	    {		
		if(Me.IsDead )  { mover.Stop(); return false; } 
		    
		// Check for stuck
		if(sd.checkStuck())
		{
		    Context.Log("Major stuck on approach. Giving up");
		    mover.Stop();
		    return false;
		}
                if(WasInCombat !=  GContext.Main.Me.IsInCombat)
                {
                    Context.Log("Combat status changed");
                    mover.Stop();
                    return false;                    
                }


		if(AbortIfUnsafe)
		{
		    // Check for adds		
		    GUnit PotentialAdd = FindPotentialAdd(monster);
		    if(PotentialAdd != null) 
		    {
			mover.Stop();
			Context.Log("Approach saw an add");
			return false;
		    }
		}

                GLocation mloc = monster.Location; 
                bool moved = mover.moveTowardsFacing(Me, mloc, Context.MeleeDistance, mloc); 
		 
                if(!moved)
                {
                    mover.Stop();
                    return true; 
                }
		
	    } while(!approachTimeout.IsReadySlow && !Me.IsDead); 
	    mover.Stop();
	    Context.Log("Approach timed out"); 
	    return false;
	}


	double DistanceToClosestHostileFrom(GUnit target)
	{

	    GMonster m = GObjectList.GetNearestHostile(target.Location, target.GUID, false);
	    if(m != null)
		return m.GetDistanceTo(target);
	    else
		return 1E100;
	}


            private class Mover
         {
             GSpellTimer BooringT = new GSpellTimer(2000);

             private bool runForwards  = false;
             private bool runBackwards = false;
             private bool strafeLeft   = false;
             private bool strafeRight  = false;
             private bool rotateLeft   = false;
             private bool rotateRight  = false;
             private GContext Context;

             public Mover(GContext Context)	   
                 {
                     this.Context = Context;
                 }
	  
             void PressKey(string name)
                 {	    
                     //Context.Log("Press : " + name);
                     Context.PressKey(name);
                 }
	    
             void ReleaseKey(string name)
                 {	    
                     //Context.Log("Release: " + name);
                     Context.ReleaseKey(name);
                 }
	    
             void SendKey(string name)
                 {	    
                     //Context.Log("Send: " + name);
                     Context.SendKey(name);
                 }


             public void Jump()
                 {
                     SendKey("Common.Jump");
                 }

             public void SwimUp(bool go)
                 {
                     if(go)
                         PressKey("Common.Jump");
                     else 
                         ReleaseKey("Common.Jump");
                 }


             public void MoveRandom()
                 {
                     int d = random.Next(4); 
                     if(d == 0) Forwards(true);
                     if(d == 1) StrafeRight(true);
                     if(d == 2) Backwards(true);
                     if(d == 3) StrafeLeft(true);
                 }

             public void StrafeLeft(bool go)
                 {
                     BooringT.Reset();

                     if(go && strafeRight) StrafeRight(false);

                     if(!go && strafeLeft) ReleaseKey("Common.StrafeLeft"); // stop
                     if(go && !strafeLeft) PressKey("Common.StrafeLeft"); // go
                     strafeLeft = go;
                 }

             public void StrafeRight(bool go)
                 {
                     BooringT.Reset();
                     if(go && strafeLeft) StrafeLeft(false);

                     if(!go && strafeRight) ReleaseKey("Common.StrafeRight"); // stop
                     if(go && !strafeRight) PressKey("Common.StrafeRight"); // go
                     strafeRight = go;
                 }

             public void RotateLeft(bool go)
                 {
                     BooringT.Reset();
                     if(go && rotateRight) RotateRight(false);

                     if(!go && rotateLeft) ReleaseKey("Common.RotateLeft"); // stop
                     if(go && !rotateLeft) PressKey("Common.RotateLeft"); // go
                     rotateLeft = go;
                 }


             public void RotateRight(bool go)
                 {
                     BooringT.Reset();
                     if(go && rotateLeft) RotateLeft(false);

                     if(!go && rotateRight) ReleaseKey("Common.RotateRight"); // stop
                     if(go && !rotateRight) PressKey("Common.RotateRight"); // go
                     rotateRight = go;
                 }


             public void Forwards(bool go)
                 {
                     BooringT.Reset();
                     if(go && runBackwards) Backwards(false);

                     if(!go && runForwards) ReleaseKey("Common.Forward"); // stop
                     if(go && !runForwards) PressKey("Common.Forward"); // go
                     runForwards = go;
                 }

             public void Backwards(bool go)
                 {
                     BooringT.Reset();
                     if(go && runForwards) Forwards(false);

                     if(!go && runBackwards) ReleaseKey("Common.Back"); // stop
                     if(go && !runBackwards) PressKey("Common.Back"); // go
                     runBackwards = go;
                 }

             public void StopRotate()
                 {
                     BooringT.Reset();
                     RotateLeft(false); 
                     RotateRight(false); 
                 }

             public void StopMove()
                 {
                     BooringT.Reset();
                     Context.ReleaseSpinRun();
                     StrafeLeft(false);
                     StrafeRight(false);
                     Forwards(false);
                     Backwards(false);
                     SwimUp(false);
                 }


             public void Stop()
                 {
                     BooringT.Reset();
                     StopMove(); 
                     StopRotate();
                 }

             public bool IsMoving()
                 {
                     return runForwards || runBackwards ||strafeLeft || strafeRight;
                 }


             public bool IsRotating()
                 {
                     return rotateLeft || rotateRight;
                 }

             public bool IsRotatingLeft()
                 {
                     return rotateLeft;
                 }
             public bool IsRotatingRight()
                 {
                     return rotateLeft;
                 }


             /*
               1 - location is front
               2 - location is right
               3 - location is back
               4 - location is left
             */
             int GetLocationDirection(GLocation loc)
                 {
                     int dir = 0;
                     double b = loc.Bearing;
                     if(b > -PI/4  && b <= PI/4)  // Front
                     {
                         dir= 1;
                     }
                     if(b > -3*PI/4 && b <= -PI/4) // Left
                     {
                         dir =  4; 
                     }
                     if(b <= -3*PI/4 || b > 3*PI/4) //  Back   
                     {
                         dir =  3;
                     }
                     if(b > PI/4  && b <= 3*PI/4) //  Right  
                     {
                         dir =  2; 
                     }
                     if(dir == 0)
                         Context.Log("Odd, no known direction");
	    
                     return dir;
                 }

             public bool moveTowardsFacing(GPlayerSelf Me, GLocation to, 
                                           double distance, GLocation facing)
                 {
                     bool moving = false;
                     double d = Me.Location.GetDistanceTo(to);
                     if(d > distance)
                     {
                         int dir = GetLocationDirection(to);
                         if(dir != 0) moving |= true;
                         if(dir == 1 || dir == 3 || dir == 0) {StrafeLeft(false); StrafeRight(false); };
                         if(dir == 2 || dir == 4 || dir == 0) {Forwards(false);   Backwards(false); };
                         if(dir == 1) Forwards(true);
                         if(dir == 2) StrafeRight(true);
                         if(dir == 3) Backwards(true); 
                         if(dir == 4) StrafeLeft(true);                     
                     }
                     else
                     {
                         StrafeLeft(false);
                         StrafeRight(false);
                         Forwards(false);
                         Backwards(false);                    
                     }
                     double bearing = Me.GetHeadingDelta(facing);
                     if(bearing < -PI/8)
                     {
                         moving |= true; 
                         RotateLeft(true);
                     }
                     else if (bearing > PI/8)
                     {
                         moving |= true; 
                         RotateRight(true);
                     }
                     else
                         StopRotate();

                     return moving;                
                 }
            
             public void Booring()
                 {
                     if(BooringT.IsReady)
                     {
                         MoveRandom();
                         Jump();
                         Stop();
                         BooringT.Reset();
                     }
                 }
	}

	class StuckDetecter
	{
	    GLocation oldLocation = null;
            double oldHeading; 
	    GSpellTimer StuckTimeout = new GSpellTimer(500); // Check every 500ms
	    int stuck = 0;
	    GPlayerSelf Me;
	    GContext Context;
	    int stuckSensitivity ; 
	    int abortSensitivity ; 

            public StuckDetecter(GPlayerSelf Me, GContext Context, 
                                 int stuckSensitivity, int abortSensitivity)
                {
                    this.Me = Me; 
                    this.Context = Context;
                    this.stuckSensitivity = stuckSensitivity;
                    this.abortSensitivity = abortSensitivity;
                }
	    public bool checkStuck()
                {
                    if(StuckTimeout.IsReady)
                    {
                        if(oldLocation != null)
                        {
                            if(mover.IsMoving())
                            {
                                float d = Me.GetDistanceTo(oldLocation);
                                if(d < 0.5)
                                {
                                    stuck++;
                                }
                                else
                                    stuck = 0;
                                if(stuck > stuckSensitivity)
                                {
                                    // Jump a bit to get loose...
                                    int dir = random.Next(3);
                                    if(dir == 0) mover.StrafeRight(true); 
                                    if(dir == 1) mover.StrafeLeft(true);
                                    if(random.Next(3) == 0) mover.Backwards(true); else mover.Forwards(true);
                                    Context.Log("I am stuck. Jumping to get free");
                                    mover.Jump();
                                    mover.StrafeLeft(false); mover.StrafeRight(false);
                                    mover.Forwards(false);  // just to sync the keys
                                    mover.Backwards(false); 
                                }
                                if(stuck > abortSensitivity)
                                {
                                    return true;
                                }
                            }
                            if(mover.IsRotating())
                            {
                                double diff = Math.Abs(oldHeading - Me.Heading);
                                if(diff < PI/16)
                                {
                                    //Context.Log("Rotate futility. resync keys");
                                    if(mover.IsRotatingLeft())
                                    {
                                        mover.RotateLeft(false);
                                        mover.RotateLeft(true);
                                    }
                                    if(mover.IsRotatingRight())
                                    {
                                        mover.RotateRight(false);
                                        mover.RotateRight(true);
                                    }
                                }
                            }
                        }
                        oldLocation = Me.Location;
                        oldHeading  = Me.Heading;
                        StuckTimeout.Reset();
                    }				
                    return false;
                }
	}


            }




}
