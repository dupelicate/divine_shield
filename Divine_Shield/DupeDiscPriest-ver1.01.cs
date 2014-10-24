using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Geometry;
using ReBot.API;


namespace ReBot
{
    [Rotation("Disc Priest", "Dupe", WoWClass.Priest, Specialization.PriestDiscipline, 30)]
    public class PriestDiscipline2 : CombatRotation
    {
//
//Hello, and welcome to the Aphla for my first Discipline Priest, Utility Support healing rotation.
//name pending by Duplicate aka dupe
//
//Special thanks too, and credit reserved for:
//Vecelinus:- The script structure and allowing me to use his work as a template.
//Dalajin:- The Tank healing script.
//
//Target friendly to heal or target hostile to Dps.
//Switching between damage and healing pops archangel.
//May act funny/pause while mesh is loading, restart bot and reselect target if this happens.
//
//Will not Mind Sear Training Dummies as they class as neutral.
//
//
//
//
//
//------Settings Start
        [JsonProperty("SwiftmendTank")]
		public bool	SwiftmendTank = true;
//------Settings Finish
    
        public bool Debug = true;
        
        //Writes to log
        private void DebugWrite(string text)
        {
            if (Debug) API.Print(text);
        }
//--------------Talent Start --------------      
        void DesperatePrayer()
        {
            if (CastPreventDouble("Desperate Prayer", () => Me.HealthFraction < 0.5 && !Me.IsCasting)) return;
        }
        void AngelicFeather()
        {
            if (CastOnTerrain("Angelic Feather", Me.PositionPredicted, () => Me.MovementSpeed > 0 && !HasAura("Angelic Feather"))) return; 
        }
        void MindBender()
        {
            if (Cast("Mindbender", () => Me.ManaFraction < 0.5))
            {
                DebugWrite("Low Mana Casting Mindbender");
            }
        }
        void PowerWordSolace()
        {
            List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquared < SpellMaxRangeSq("Power Word: Solace"));
            if (mobs.Count > 0)
            {
                //Solace CD Check
                if (SpellCooldown("Power Word: Solace") < 0)
                {

                    //Cast solace on add
                    var Solmob = mobs.FirstOrDefault();
                    CastPreventDouble("Power Word: Solace", () => !Solmob.IsDead && Solmob.IsInCombatRangeAndLoS && SpellCooldown("Power Word: Solace") <= -2000, Solmob);
                    DebugWrite("Casting Solace on " + Solmob + SpellCooldown("Power Word: Solace"));
                    return;
                }
            }

                if (Cast("Power Word: Solace")) return;
        }
        void VoidTendrils()
        {
            List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquaredTo(Target) < 10 * 10);
            if (CastPreventDouble("Void Tendrils", () => mobs.Count > 2)) return;
        }
        void PsychicScream()
        {
            List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquaredTo(Me) < 8 * 8);
            if (CastPreventDouble("Psychic Scream", () => mobs.Count > 2)) return;
        }
        void DominateMind()
        {
            //No idea how to get this working at the min :D
        }
        void SpiritShell()
        {
            if (CastPreventDouble("Spirit Shell", () => !Target.IsEnemy && Target.HealthFraction < 0.9 && Target.IsInCombatRangeAndLoS && !Me.HasAura("Spirit Shell"))) return;
        }
        void Cascade()
        {
            if (CastPreventDouble("Cascade", () => Target.HealthFraction < 0.9 && SpellCooldown("Cascade") < 0)) return;
        }
        void DivineStar()
        {
             List<PlayerObject> members = Group.GetGroupMemberObjects();
             if (members.Count > 0)
             {
                    // Finding Tank
                 List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRangeAndLoS);
                 PlayerObject Tank = Tanks.FirstOrDefault();
                 if (Tank != null)
                 {
                     Cast("Divine Start", () => !Tank.IsDead && Tank.HealthFraction < 0.8,Tank);
                     return;
                 }
             }
        }
        void Halo()
        {
            if (SpellCooldown("Halo") <= 0)
            {
                CastPreventDouble("Halo");
            }
            DebugWrite("Casting Halo");
        }
//------------Talents End-----------
//------------Combat Start----------
        public override bool OutOfCombat()
        {

            if (CastSelf("Power Word: Fortitude", () => !HasAura("Power Word: Fortitude"))) return true;
            if (CastSelf("Fear Ward", () => CurrentBotName == "PvP" && !HasAura("Fear Ward"))) return true;
            AngelicFeather();


            if (CurrentBotName == "Combat")
            {
                List<PlayerObject> members = Group.GetGroupMemberObjects();
                if (members.Count > 0)
                {
                    PlayerObject deadPlayer = members.FirstOrDefault(x => x.IsDead);
                    if (Cast("Resurrection", () => deadPlayer != null, deadPlayer)) return true;
                }

            }
            if (CastSelf("Levitate", () => Me.FallingTime > 2 && !HasAura("Levitate"))) return true;

            // Only use OnWaterMove Spell, if Navi target is not in water. Cancel buff if we have to dive
            if (API.GetNaviTarget() != Vector3.Zero && HasSpell("Levitate"))
            {
                if (!API.IsNaviTargetInWater())
                {
                    if (CastSelf("Levitate", () => Me.Race != WoWRace.Tauren && Me.IsSwimming && !HasAura("Levitate"))) return true;
                }
                else if (HasAura("Levitate"))
                    CancelAura("Levitate");
            }

            return false;
        }



        void Healer()
        {
            // Popping Archangel if possible
            if (CastSelf("Archangel", () => HasAura("Evangelism"))) return;

            // setting group
            List<PlayerObject> members = Group.GetGroupMemberObjects();
            if (members.Count > 0)
            {

                // Finding Tank
                List<PlayerObject> Tanks = members.FindAll(x => x.IsTank && x.IsInCombatRangeAndLoS);
                PlayerObject Tank1 = Tanks.FirstOrDefault();
                
            
            // Group Healing
            List<PlayerObject> GrpHeal1 = members.FindAll(x => x.HealthFraction <= 0.85 && x.IsInCombatRangeAndLoS);
            if (GrpHeal1.Count > 3 && SpellCooldown("Halo") <= 0)
            {
                Halo();
            }

            List<PlayerObject> GrpHeal2 = members.FindAll(x => x.HealthFraction <= 0.7 && x.IsInCombatRangeAndLoS);
            if (GrpHeal2.Count > 3)
            {
                Cast("Prayer of Healing", GrpHeal2.FirstOrDefault());
                DebugWrite("Casting Payer of Healing");
            }
            
            
            
               


                // Shield Tank
                if (Tank1 != null)
                {
                    if (CastPreventDouble("Power Word: Shield", () => !Tank1.HasAura("Weakened Soul") && Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1, 1000))
                    {
                        DebugWrite("Shielding " + Tank1);
                        return;
                    }
                    if (CastPreventDouble("Prayer of Mending", () => !Tank1.HasAura("Prayer of Mending") && Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1, 1000))
                    {
                        DebugWrite("POM on " + Tank1);
                        return;
                    }
                }

                // Tank 1
                if (Tank1 != null)
                {
                    if (Tank1.HasAura("Weakened Soul", true) && Tank1.IsInCombatRangeAndLoS)
                    {
                        if (Tank1.HealthFraction <= 0.3)
                        {
                            CastPreventDouble("Flash Heal", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                            DebugWrite("Casting Flash Heal on " + Tank1);
                            return;
                        }
                        else if (Tank1.HealthFraction <= 0.7)
                        {
                            CastPreventDouble("Pain Supression", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                            DebugWrite("Casting Pain Supression on " + Tank1);
                            return;
                        }
                        else if (Tank1.HealthFraction <= 0.8)
                        {
                            CastPreventDouble("Heal", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                            DebugWrite("Casting Heal on " + Tank1);
                            return;
                           
                        }
                        else if (Tank1.HealthFraction <= 0.9)
                        {
                            CastPreventDouble("Penance", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                            DebugWrite("Casting Penance on " + Tank1);
                            return;
                            
                        }

                    }
                }

                // Tank 2
                if (Tanks.Count > 1)
                {
                    PlayerObject Tank2 = Tanks.Last();
                
                if (Tank2 != null)
                {

               
                    if (CastPreventDouble("Power Word: Shield", () => !Tank2.HasAura("Weakened Soul") && Tank2.IsInCombatRangeAndLoS && !Tank2.IsDead, Tank2, 1000))
                    {
                        DebugWrite("Shielding " + Tank2);
                        return;
                    }
                    if (CastPreventDouble("Prayer of Mending", () => !Tank2.HasAura("Prayer of Mending") && Tank2.IsInCombatRangeAndLoS && !Tank2.IsDead, Tank2, 1000))
                    {
                        DebugWrite("POM on " + Tank2);
                        return;
                    }
                    if (Tank2.HealthFraction <= 0.3)
                    {
                        CastPreventDouble("Flash Heal", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                        DebugWrite("Casting Flash Heal on " + Tank1);
                        return;
                    }
                    else if (Tank1.HealthFraction <= 0.7)
                    {
                        CastPreventDouble("Pain Supression", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                        DebugWrite("Casting Pain Supression on " + Tank1);
                        return;

                    }
                    else if (Tank2.HealthFraction <= 0.8)
                    {
                        CastPreventDouble("Heal", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                        DebugWrite("Casting Heal on " + Tank1);
                        return;

                    }
                    else if (Tank2.HealthFraction <= 0.9)
                    {
                        CastPreventDouble("Penance", () => Tank1.IsInCombatRangeAndLoS && !Tank1.IsDead, Tank1);
                        DebugWrite("Casting Penance on " + Tank1);
                        return;

                    }
                } else {
                    return;
                }
                }
                //Healing me
                if (CastSelfPreventDouble("Heal", () => Me.HealthFraction <= 0.9)) return;
                //Regen mana if low
                MindBender();
                if (Cast("Shadowfiend", () => Me.ManaFraction < 0.5))
                {
                    DebugWrite("Low Mana Releasing Shadowfiend");
                    return;
                }

                // Purify
                List<PlayerObject> GrpCleanse = members.FindAll(m => m.Auras.Any(a => a.IsDebuff && "Magic,Disease,Curse".Contains(a.DebuffType)));
                if (GrpCleanse.Count > 3)
                {
                    CastOnTerrain("Mass Dispel", GrpCleanse.First().Position);
                }
                var emd = members.FirstOrDefault(m => m.Auras.Any(a => a.IsDebuff && "Magic,Disease".Contains(a.DebuffType)));
                if (emd != null)
                    if (Cast("Purify", emd))
                    {
                        DebugWrite("Dispelling" + emd);
                        return;
                    }

               
                //Second part of group healing, single target healing
                foreach (var player in Group.GetGroupMemberObjects())
                {
                    if (Cast("Flash Heal", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.6)) return;
                    if (Cast("Heal", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.95 && player.HealthFraction > 0.4)) return;
                    if (Cast("Penance", player, () => !player.IsTank && !player.IsDead && player.HealthFraction < 0.8)) return;
                    
                }
               
               
               

               
            }
        }

        void DPS()
        {
            //Global cooldown check
            if (HasGlobalCooldown())
                return;
            //finding adds for Dots
            List<UnitObject> mobs = Adds.FindAll(x => x.DistanceSquared < SpellMaxRangeSq("Power Word: Solace"));
            if (mobs.Count > 0)
            {
                
                //Finds adds for SWpain dot
                foreach (var SWpain in mobs.Where(x => !x.HasAura("Shadow Word: Pain")))
                {
                    //Dots mobs in range
                    UnitObject SWP = SWpain;
                   CastPreventDouble("Shadow Word: Pain", () => !SWP.HasAura("Sahdow Word: Pain") && !SWP.IsDead, SWP);
                   return;
                }
            }
            //selfheal
            if (CastSelf("Desperate Prayer", () => Me.HealthFraction < 0.4)) return;
            if (CastSelf("Penance", () => Me.HealthFraction < 0.5)) return;
            if (CastSelf("Power Word: Shield", () => Me.HealthFraction < 0.75 && !HasAura("Weakened Soul"))) return;
            if (CastSelf("Renew", () => Me.HealthFraction < 0.9 && !HasAura("Renew"))) return;

            //mana regen
            if (Cast("Mindbender", () => Me.ManaFraction < 0.5)) return;
            if (Cast("Shadowfiend", () => Me.ManaFraction < 0.5)) return;

            // attack rotation
            if (Cast("Penance")) return;
            PowerWordSolace();
            if (Cast("Shadow Word: Pain", () => !Target.HasAura("Shadow Word: Pain"))) return;
            List<UnitObject> MS = Adds.FindAll(x => x.DistanceSquaredTo(Target) < 10 * 10);
            if (MS.Count > 2)
            {
                Cast("Mind Sear");
            }

            Cast("Smite");


            



        }

        // Gruppen Heal - Ende

        public override void Combat()
        {
          
            //Could never get if (CombatMode == CombatModus.Healer) to work correctly or at all most of the time.
            //This is my work around, anything other than a friendly target is dpsed
            if (!Target.IsEnemy)
            {                  
                Healer();
            }
            else
            {
                DPS();
            }
            //Dummy zapping
            if (Target.DisplayId == 28048 || Target.DisplayId == 27510)
            {
                DPS();
            }
            return;

        }

    }
}

