using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;

namespace Nautilus
{
    class Program
    {
        
        private static Menu Menu;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Spell Q, W, E, R;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Nautilus") 
                return;
            //q = new spell(spellslot, range)
            Q = new Spell(SpellSlot.Q, 1100);
            //setskillshot(delay, width, speed, bool collison, skillshot type)
            Q.SetSkillshot(1100f, 80f, 1200, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 1500);

            Menu = new Menu("SynNautilus", "synnaut", true);

            Menu Orbwalkermenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Orbwalkermenu);

            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));
            TargetSelector.AddToMenu(ts);

            Menu harass = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Qharass", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("Eharass", "Use E").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("manapercent", "Harass till % mana")).SetValue(new Slider(50, 1, 100));

            Menu combo = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Qcombo", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Wcombo", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("Ecombo", "Use E").SetValue(true));

            Menu ks = Menu.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            Menu.SubMenu("Killsteal").AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            Menu.SubMenu("Killsteal").AddItem(new MenuItem("UseRKS", "Use R").SetValue(true));

            Menu misc = Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("QInterr", "Use Q to interrupt").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Egapclose", "Use E gapclose").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Wgapclose", "Use W gapclose").SetValue(true));

            Menu drawings = Menu.AddSubMenu(new Menu("Drawing", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawEnable", "Enable Drawing")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Draw Q")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawE", "Draw E")).SetValue(true);
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawR", "Draw R")).SetValue(true);

            Menu.AddToMainMenu();

            

            /*foreach (var spellData in ObjectManager.Player.Spellbook.Spells)
            {
                Game.PrintChat("Spell name: " + spellData.SData.Name);
                Game.PrintChat("Spell width: " + spellData.SData.LineWidth);
                Game.PrintChat("Spell speed: " + spellData.SData.MissileSpeed);
                Game.PrintChat("Spell range: " + spellData.SData.CastRangeDisplayOverride);
                Game.PrintChat("Spell delay: " + spellData.SData.SpellCastTime);
            }*/
            Game.OnUpdate += OnGameUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Game.PrintChat("Thank you for using Syntax Nautilus! GLHF");
            
        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
            }
            //KS();
            
        }

        static void Orbwalking_BeforeAttack(LeagueSharp.Common.Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                if (((Obj_AI_Base)Orbwalker.GetTarget()).IsMinion) args.Process = false;
            }
        }
        

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range - 100, TargetSelector.DamageType.Magical);
            

            if (target == null || !target.IsValid) return;

            if (Player.ManaPercent < Menu.Item("manapercent").GetValue<Slider>().Value) return;

            if (Menu.Item("Qharass").GetValue<bool>() && Q.IsReady()){
                Q.CastIfHitchanceEquals(target, HitChance.High);

            }

            if (Menu.Item("Eharass").GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast();

            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range - 100, TargetSelector.DamageType.Magical);


            if (target == null || !target.IsValid) return;

            if (Menu.Item("Qcombo").GetValue<bool>() && Q.IsReady())
            {
                Q.CastIfHitchanceEquals(target, HitChance.High);

            }

            if (Menu.Item("Wcombo").GetValue<bool>() && W.IsReady() && target.IsValidTarget(Player.AttackRange + 100))
            {
                W.Cast();

            }

            if (Menu.Item("Ecombo").GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast();

            }

        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (Menu.Item("QInterrupt").GetValue<bool>() && Q.IsReady() && unit.IsValidTarget(Q.Range)) 
            {
                Q.Cast(unit);   
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("Wgapclose").GetValue<bool>() && !Menu.Item("Egapclose").GetValue<bool>())return;

            if (gapcloser.Sender.IsValidTarget(E.Range) && Menu.Item("Egapclose").GetValue<bool>())
            {
                E.Cast();
            }
            if (Menu.Item("Wgapclose").GetValue<bool>())
            {
                W.Cast();
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Q.IsReady() && Menu.Item("drawQ").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.White, 1, 1);  
              
            }
            if (E.IsReady() && Menu.Item("drawE").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Aqua, 1, 1); 
            }
            if (R.IsReady() && Menu.Item("drawR").GetValue<bool>())
            {
                Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Cyan, 1, 1);
            }
        }
    }
}
