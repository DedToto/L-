#region
using System;
using System.Collections;
using System.Linq;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using xSLx_Orbwalker;
using Veigar__The_Tiny_Master_Of_Evil.Properties;
#endregion

namespace Veigar__The_Tiny_Master_Of_Evil
{
    class Program
    {
        public const string ChampionName = "Veigar";
        public static bool boughtItemOne;                   //Var to check if bought starting items
        public static Obj_AI_Hero ChoosedTarget = null;
        private static int TargetLockCD;
        public static Obj_AI_Base _m = null;
        private static Obj_AI_Hero Player;
        private static Obj_AI_Hero Target = null;

        //Buffs
        public static List<NewBuff> buffList = new List<NewBuff>
        {
            
            new NewBuff()
            {
                MenuName = "Sion Passive", DisplayName = "SionPassiveZombie", Name = "sionpassivezombie"
            },
            new NewBuff()
            {
                MenuName = "Alistar Ult", DisplayName = "Trample Buff", Name = "alistartrample"
            },
            new NewBuff()
            {
                MenuName = "Anivia Passive", DisplayName = "rebirth", Name = "Rebirth"
            },
            new NewBuff()
            {
                MenuName = "Aatrox Passive", DisplayName = "aatroxpassivedeath", Name = "AatroxPassiveReady"
            },
            new NewBuff()
            {
                MenuName = "Zac Passive", DisplayName = "zacrebirthready", Name = "ZacRebirthReady"
            },
            new NewBuff()
            {
                MenuName = "Kayle Ult", DisplayName = "JudicatorIntervention", Name = "JudicatorIntervention"
            },
            new NewBuff()
            {
                MenuName = "Lissandra Ult", DisplayName = "lissandrarself", Name = "LissandraRSelf"
            },
            new NewBuff()
            {
                MenuName = "Poppy Ult", DisplayName = "PoppyDiplomaticImmunity", Name = "PoppyDiplomaticImmunity"
            },
            new NewBuff()
            {
                MenuName = "Trynda Ult", DisplayName = "UndyingRage", Name = "Undying Rage"
            },
            new NewBuff()
            {
                MenuName = "Braum Shield", DisplayName = "braumeshieldbuff", Name = "BraumShieldRaise"
            },
        };

        public static List<NewIgnore> IgnoreList = new List<NewIgnore>
        {
            new NewIgnore()
            {
                MenuName = "Banshees Veil", DisplayName = "BansheesVeil", Name = "bansheesveil"
            },
            new NewIgnore()
            {
                MenuName = "Nocturne Shield", DisplayName = "NocturneShroudofDarkness", Name = "NocturneShroudofDarknessShield"
            },
            new NewIgnore()
            {
                MenuName = "Morgana Shield", DisplayName = "Black Shield", Name = "BlackShield"
            },
            new NewIgnore()
            {
                MenuName = "Sivir Shield", DisplayName = "SivirE", Name = "Spell Shield"
            },
            new NewIgnore()
            {
                MenuName = "Fizz E", DisplayName = "fizztrickslamsounddummy", Name = "fizztrickslamsounddummy"
            },
                        new NewIgnore()
            {
                MenuName = "Vladimir W", DisplayName = "VladimirSanguinePool", Name = "VladimirSanguinePool"
            },
        };

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Damage
        private static Dictionary<Obj_AI_Hero, int> enemyDictionary = new Dictionary<Obj_AI_Hero, int>();
        private static Dictionary<Obj_AI_Hero, string> enemyDictionary1 = new Dictionary<Obj_AI_Hero, string>();

        //DFG and Ignite
        public static Items.Item Dfg;
        public static SpellSlot IgniteSlot;
        public static SpellSlot DFGSlot;

        //Items
        public static Items.Item biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        //Mana Manager
        public static int[] qMana = { 0, 60, 65, 70, 75, 80 };
        public static int[] wMana = { 0, 70, 80, 90, 100, 110 };
        public static int[] eMana = { 0, 80, 90, 100, 110, 120 };
        public static int[] rMana = { 0, 125, 175, 225 };

        public static int ManaMode = 0;
        public static double NeededCD = 0d;

        //Orbwalker instance
        private static Orbwalking.Orbwalker Orbwalker;

        //Menu
        public static Menu menu;

        private static Menu orbwalkerMenu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            var sprite = new Render.Sprite(Properties.Resources.Sprite, new Vector2(Drawing.Width * 0.83f, Drawing.Height * 0.33f));
            sprite.VisibleCondition += s => Render.OnScreen(Drawing.WorldToScreen(Player.Position)) && menu.Item("Show").GetValue<bool>();
            sprite.Scale = new Vector2(1f, 1f);
            sprite.Add();
            Game.OnGameUpdate += eventArgs =>
            {
                if (sprite != null && Game.ClockTime >= 40)
                {
                    sprite.Dispose();
                    sprite = null;
                }
            };
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            // Check if the champion is Veigar or not.
            if (Player.BaseSkinName != ChampionName) return;

            //Initializing Spells
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 1005);
            R = new Spell(SpellSlot.R, 650);

            W.SetSkillshot(1.25f, 230f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(.2f, 330f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            DFGSlot = Player.GetSpellSlot("Deathfire_Grasp");

            Dfg = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ||
                  Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar
                ? new Items.Item(3188, 750)
                : new Items.Item(3128, 750);

            //Menu
            menu = new Menu(ChampionName, ChampionName, true);
            orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");

            //Target selector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            orbwalkerMenu.AddItem(new MenuItem("Orbwalker_Mode", "Regular Orbwalker").SetValue(false));
            menu.AddSubMenu(orbwalkerMenu);
            chooseOrbwalker(menu.Item("Orbwalker_Mode").GetValue<bool>());

            //Keys & Combo Related
            menu.AddSubMenu(new Menu("Keys", "Keys"));
            menu.SubMenu("Keys");
            menu.SubMenu("Keys").AddItem(new MenuItem("Combo", "Smart Combo").SetValue(new KeyBind(32, KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("AllInActive", "Use All Spells").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "Harass").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            menu.SubMenu("Keys").AddItem(new MenuItem("Stun Closest Enemy", "Stun Closest Enemy").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitQQ", "Last hit with Q").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Keys").AddItem(new MenuItem("LastHitWW", "Lane Clear with W").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("InfoTable", "Show Info Table[FPS]").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            //Drawings menu:
            menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q,R range").SetValue(new Circle(true, Color.FromArgb(255, 0, 255, 0))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, Color.FromArgb(255, 255, 0, 255))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("MinionMarker", "Mark Q Farm Minions").SetValue(new Circle(true, Color.Green)));
            menu.SubMenu("Drawings").AddItem(new MenuItem("TText", "Mark Targets with Circles").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("LText", "Display Locked Target[HP BAR]").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("manaStatus", "Mana status").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("HUD", "Heads-up Display").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ExtraNeeded", "Show Extra/Needed Damage").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("OptimalCombo", "Show Best Kill Combo[FPS]").SetValue(false));


            //Misc menu:
            menu.AddSubMenu(new Menu("Other", "Other"));
            menu.SubMenu("Other").AddItem(new MenuItem("StunUnderTower", "Stun Enemies Attacked by Tower").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("UseInt", "Use E to Interrupt").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("UseGap", "Use E against GapClosers").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("PotOnIGN", "Use HP Pot when ignited").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("buystart", "Buy Starting Items").SetValue(new KeyBind("P".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Other").AddItem(new MenuItem("Reset", "Remove Target Lock").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Other").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("Show", "Display Sprite").SetValue(true));

            //Farm menu:
            menu.AddSubMenu(new Menu("Farm", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("WAmount", "Min Minions To Land W").SetValue(new Slider(3, 1, 7)));
            menu.SubMenu("Farm").AddItem(new MenuItem("SaveE", "Save Mana for E while Farming").SetValue(false));

            //Auto KS
            menu.AddSubMenu(new Menu("Auto KS", "AutoKS"));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseWKS", "Use W").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseRKS", "Use R").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseDFGKS", "Use DFG").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseIGNKS", "Use IGN").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("AutoKST", "AutoKS (toggle)!").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle, true)));

            //Harass menu:
            menu.AddSubMenu(new Menu("Harass", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("HarassMode", "Choose Harass Type").SetValue(new StringList(new[] { "E+W+Q", "E+W", "Q" }, 0)));
            menu.SubMenu("Harass").AddItem(new MenuItem("WaitW", "Cast W before Q").SetValue(false));

            //Combo menu:
            menu.AddSubMenu(new Menu("Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("LockTargets", "Lock Targets with Left Click").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("DontEShields", "Dont use E in spell shields").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("Harassifnk", "Harass with Q if not killable").SetValue(false));
            menu.SubMenu("Combo").AddSubMenu(new Menu("Dont use R,IGN,DFG if target has", "DontRIGN"));
            foreach (var buff in buffList)
                menu.SubMenu("Combo").SubMenu("DontRIGN").AddItem(new MenuItem("dont" + buff.Name, buff.MenuName).SetValue(true));
            foreach (var buff in IgnoreList.Where(buff => buff.MenuName != "Sivir Shield" && buff.MenuName != "Fizz E" && buff.MenuName != "Vladimir W"))
                menu.SubMenu("Combo").SubMenu("DontRIGN").AddItem(new MenuItem("dont" + buff.Name, buff.MenuName).SetValue(true));

            menu.AddToMainMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            GameObject.OnCreate += TowerAttackOnCreate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.PrintChat("Veigar, The Tiny Master Of Evil Loaded! Made by DedToto");
        }
        
        public static bool IsImmune(Obj_AI_Hero target)
        {
            foreach (var buff in IgnoreList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name)) return true;
            }
            return false;
        }

        public static bool HasBuffs(Obj_AI_Hero target)
        {
            foreach (var buff in buffList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name))
                {
                    if (menu.Item("dont" + buff.Name).GetValue<bool>())
                        return true;
                }
            }
            return false;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            #region OnUpdate
            if (ChoosedTarget != null && ChoosedTarget.IsDead || !menu.Item("LockTargets").GetValue<bool>())
                ChoosedTarget = null;

            Target = GetTarget();

            //check if player is dead
            if (Player.IsDead) return;

            if (menu.Item("Reset").GetValue<KeyBind>().Active)
            {
                ChoosedTarget = null;
            }

            if (menu.Item("PotOnIGN").GetValue<bool>())
            {
                AutoHP();
            }

            if (menu.Item("buystart").GetValue<KeyBind>().Active)
            {
                BuyItems();
            }

            if (!menu.Item("Combo").GetValue<KeyBind>().Active)
            {

                if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active && !menu.Item("AllInActive").GetValue<KeyBind>().Active && !menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active && !menu.Item("HarassActive").GetValue<KeyBind>().Active)
                {
                    lastHit();
                }

                if (menu.Item("AutoKST").GetValue<KeyBind>().Active)
                {
                    AutoKS();
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                {
                    Harass();
                }

                if (menu.Item("LastHitWW").GetValue<KeyBind>().Active)
                {
                    lastHitW();
                }

                if (menu.Item("AllInActive").GetValue<KeyBind>().Active)
                {
                    AllIn();
                }

                if (menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active)
                {
                    if (E.IsReady())
                        castE(GetNearestEnemy(Player));
                }
            }
            else
            {
                Combo();
            }

            if (menu.Item("manaStatus").GetValue<bool>())
            {
                ManaMode = manaCheck().Item1;
                NeededCD = manaCheck().Item2;
            }

            if (menu.Item("ExtraNeeded").GetValue<bool>())
            {
                enemyDictionary = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).ToDictionary(enemy => enemy, enemy => GetExtraNeeded(enemy).Item1);
            }

            if (menu.Item("InfoTable").GetValue<KeyBind>().Active || menu.Item("OptimalCombo").GetValue<bool>())
            {
                enemyDictionary1 = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).ToDictionary(enemy => enemy, enemy => GetBestCombo(enemy, "Comboing").Item1);
            }
            #endregion
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            if (Target != null && Target.IsVisible)
            {
                if (ChoosedTarget != null && menu.Item("LockTargets").GetValue<bool>())
                {
                    if (menu.Item("LText").GetValue<bool>()) Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 25, System.Drawing.Color.LightGreen, "Lock:" + ChoosedTarget.Name);
                    if (menu.Item("TText").GetValue<bool>()) Utility.DrawCircle(ChoosedTarget.Position, 75, Color.LightGreen);
                }
                else
                {
                    if (menu.Item("TText").GetValue<bool>()) Utility.DrawCircle(Target.Position, 75, Color.Red);
                }
            }

            //Draw Extra or Needed for kill damage
            if (menu.Item("ExtraNeeded").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in enemyDictionary.Keys)
                {
                    if (enemy.IsVisible && !enemy.IsDead)
                    {
                        string Text = GetExtraNeeded(enemy).Item2;
                        float ENdamage = 0f;
                        if (enemyDictionary[enemy] >= 1000)
                        {
                            int Integer = enemyDictionary[enemy];
                            float Floater = Integer;
                            ENdamage = (float)Math.Round((Floater / 1000), 1);
                        }
                        else
                        {
                            ENdamage = enemyDictionary[enemy];
                        }
                        if (Text == "Extra:")
                        {
                            if (enemyDictionary[enemy] >= 1000)
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.Red, "Killable(" + ENdamage + "k+)");
                            else
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.Red, "Killable(" + ENdamage + ")");
                        }
                        else
                        {
                            if (enemyDictionary[enemy] >= 1000)
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.White, "Unkillable(" + ENdamage + "k+)");
                            else
                                Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 1, Color.White, "Unkillable(" + ENdamage + ")");
                        }
                    }
                }
            }

            if (menu.Item("OptimalCombo").GetValue<bool>())
            {
                foreach (Obj_AI_Hero enemy in enemyDictionary1.Keys)
                {
                    if (enemy.IsVisible)
                    {
                        if (!enemy.IsDead)
                        {
                            Drawing.DrawText(enemy.HPBarPosition.X + 9, enemy.HPBarPosition.Y + 2, System.Drawing.Color.White, enemyDictionary1[enemy]);
                        }
                    }
                }
            }

            #region Indicators
            if (menu.Item("HUD").GetValue<bool>())
            {
                if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.92f, System.Drawing.Color.Yellow, "Q LastHit : On");
                else
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.92f, System.Drawing.Color.DarkRed, "Q LastHit : Off");

                if (menu.Item("AllInActive").GetValue<KeyBind>().Active)
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.90f, System.Drawing.Color.Yellow, "Use All Spells : On");
                else
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.90f, System.Drawing.Color.DarkRed, "Use All Spells : Off");

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.88f, System.Drawing.Color.Yellow, "Harass : On");
                else
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.88f, System.Drawing.Color.DarkRed, "Harass : Off");

                if (menu.Item("Combo").GetValue<KeyBind>().Active)
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.86f, System.Drawing.Color.Yellow, "Combo : On");
                else
                    Drawing.DrawText(Drawing.Width * 0.67f, Drawing.Height * 0.86f, System.Drawing.Color.DarkRed, "Combo : Off");
            }
            #endregion
            #region Minion Marker
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var MinMar = menu.Item("MinionMarker").GetValue<Circle>();
            if (MinMar.Active)
            {
                if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                {
                    foreach (Obj_AI_Base minion in allMinions)
                    {
                        if (minion.IsValidTarget() &&
                            HealthPrediction.GetHealthPrediction(minion, (int)((minion.Distance(Player.Position) / 1500) * 1000 + .25f * 1000), 100) <
                            Player.GetSpellDamage(minion, SpellSlot.Q))
                        {
                            _m = minion;
                            if (_m.IsDead)
                                _m = null;
                            if (_m == minion)
                                Utility.DrawCircle(minion.Position, 75, MinMar.Color);
                        }
                    }
                }
            }
            #endregion
            #region Mana Status
            if (ManaMode != 0)
            {
                Vector2 wts = Drawing.WorldToScreen(Player.Position);
                if (ManaMode == 1)
                {
                    Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ("Q(" + NeededCD + ")s"));
                }
                else if (ManaMode == 2)
                {
                    Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ("E+W+Q(" + NeededCD + ")s"));
                }
                else if (ManaMode == 3)
                {
                    Drawing.DrawText(wts[0] - 30, wts[1], Color.White, ("E+W+Q+R(" + NeededCD + ")s"));
                }
            }
            #endregion
            #region InfoTable
            if (menu.Item("InfoTable").GetValue<KeyBind>().Active)
            {
                var x = Drawing.Width * 0.85f;
                var y = Drawing.Height * 0.62f;
                Drawing.DrawText(x, y - 20f, System.Drawing.Color.White, "~INFO TABLE~");
                //Applies the function to all enemy heroes
                foreach (Obj_AI_Hero enemy in enemyDictionary1.Keys)
                {
                    if (!enemy.IsDead)
                    {
                        if (enemyDictionary1[enemy] == "Unkillable")
                            Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + GetExtraNeeded(enemy).Item1 + ")");
                        else
                            Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy]);
                    }
                    else
                    {
                        Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + "Dead"); //Enemy is dead
                    }
                    y = y + 20f;
                }
            }
            #endregion
            foreach (Spell spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }
        }

        //Automatic Kill Steal
        private static void AutoKS()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team && Player.Distance(enemy.Position) <= NeededRange(menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseDFGKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>())))
            {
                //if (ChoosedTarget != null && enemy != ChoosedTarget) return;
                double damage = 0d;
                string LocalSource = null;
                if (menu.Item("UseWKS").GetValue<bool>()) LocalSource = "NE"; else LocalSource = "N";
                damage = GetComboDamage(enemy, LocalSource, menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseDFGKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>());
                if (damage > enemy.Health && HasMana(menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseRKS").GetValue<bool>()))
                    UseSpells(enemy, LocalSource, menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseDFGKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>());
            }
        }

        private static int NeededRange(bool A, bool B, bool C, bool D, bool EEE, bool F)
        {
            bool AR = Exists(true, false, false, false, false, false);
            bool BR = Exists(false, true, false, false, false, false);
            bool CR = Exists(false, false, true, false, false, false);
            bool DR = Exists(false, false, false, true, false, false);
            bool ER = Exists(false, false, false, true, true, false);
            bool FR = Exists(false, false, false, false, false, true);

            int NeededRangee = 0;
            if (F && FR)
                NeededRangee = 600;
            else if (D && DR)
                NeededRangee = 650;
            else if (A && AR)
                NeededRangee = 650;
            else if (EEE && ER)
                NeededRangee = 750;
            else if (B && BR)
                NeededRangee = 900;
            else if (C && CR)
                NeededRangee = 1050;
            return NeededRangee;
        }

        //Harass Combo(Independent of CD and target HP)
        private static void Harass()
        {
            if (menu.Item("HarassMode").GetValue<StringList>().SelectedIndex == 0) UseSpells(Target, "EWQHarass", true, true, true, false, false, false);
            else if (menu.Item("HarassMode").GetValue<StringList>().SelectedIndex == 1) UseSpells(Target, "EWHarass", false, true, true, false, false, false);
            else if (menu.Item("HarassMode").GetValue<StringList>().SelectedIndex == 2) UseSpells(Target, "QHarass", true, false, false, false, false, false);
        }

        //Use All Available Spells Combo(Independent of CD and target HP)
        private static void AllIn()
        {
            UseSpells(Target, "AllIn", true, true, true, true, true, true);
        }

        //The Main Smart Combo that chooses the most efficient combo that will ensure the kill
        private static void Combo()
        {
            if (Player.Distance(Target.Position) <= E.Range)
            {
                string TheCombo = GetBestCombo(Target, "Comboing").Item1;

                if (TheCombo == "Q" && HasMana(true, false, false, false)) //Q
                    UseSpells(Target, "N", true, false, false, false, false, false);
                else if (TheCombo == "E+W" && HasMana(false, true, true, false)) //E+W
                    UseSpells(Target, "NE", false, true, true, false, false, false);
                else if (TheCombo == "E+W+Q" && HasMana(true, true, true, false)) //E+W+Q
                    UseSpells(Target, "NE", true, true, true, false, false, false);
                else if (TheCombo == "|DFG|E+W+Q" && HasMana(true, true, true, false)) //DFG+E+W+Q
                    UseSpells(Target, "NE", true, true, true, false, true, false);
                else if (TheCombo == "|DFG|E+W" && HasMana(false, true, true, false)) //DFG+E+W
                    UseSpells(Target, "NE", false, true, true, false, true, false);
                else if (TheCombo == "|DFG|Q" && HasMana(true, false, false, false)) //DFG+Q
                    UseSpells(Target, "N", true, false, false, false, true, false);
                else if (TheCombo == "E+W+R" && HasMana(false, true, true, true)) //E+W+R
                    UseSpells(Target, "NE", false, true, true, true, false, false);
                else if (TheCombo == "E+W+R+Q" && HasMana(true, true, true, true)) //E+W+R+Q
                    UseSpells(Target, "NE", true, true, true, true, false, false);
                else if (TheCombo == "E+R" && HasMana(false, false, true, true)) //E+R
                    UseSpells(Target, "N", false, false, true, true, false, false);
                else if (TheCombo == "R" && HasMana(false, false, false, true)) //R
                    UseSpells(Target, "N", false, false, false, true, false, false);
                else if (TheCombo == "|DFG|E+R+Q" && HasMana(true, false, true, true)) //DFG+E+R+Q
                    UseSpells(Target, "N", true, false, true, true, true, false);
                else if (TheCombo == "|DFG|E+W+R" && HasMana(false, true, true, true)) //DFG+E+W+R
                    UseSpells(Target, "NE", false, true, true, true, true, false);
                else if (TheCombo == "|DFG|E+W+R+Q" && HasMana(true, true, true, true)) //DFG+E+W+R+Q
                    UseSpells(Target, "NE", true, true, true, true, true, false);
                else if (TheCombo == "|DFG|R" && HasMana(false, false, false, true)) //DFG+R
                    UseSpells(Target, "N", false, false, false, true, true, false);
                else if (TheCombo == "E+Q+IGN" && HasMana(true, false, true, false)) //E+Q+IGN
                    UseSpells(Target, "N", true, false, true, false, false, true);
                else if (TheCombo == "E+W+IGN" && HasMana(false, true, true, false)) //E+W+IGN
                    UseSpells(Target, "NE", false, true, true, false, false, true);
                else if (TheCombo == "E+W+Q+IGN" && HasMana(true, true, true, false)) //E+W+Q+IGN
                    UseSpells(Target, "NE", true, true, true, false, false, true);
                else if (TheCombo == "E+W+R+Q+IGN" && HasMana(true, true, true, true)) //E+W+R+Q+IGN
                    UseSpells(Target, "NE", true, true, true, true, false, true);
                else if (TheCombo == "|DFG|E+Q+IGN" && HasMana(true, false, true, false)) //DFG+E+Q+IGN
                    UseSpells(Target, "N", true, false, true, false, true, true);
                else if (TheCombo == "|DFG|E+W+IGN" && HasMana(false, true, true, false)) //DFG+E+W+IGN
                    UseSpells(Target, "NE", false, true, true, false, true, true);
                else if (TheCombo == "|DFG|E+W+Q+IGN" && HasMana(true, true, true, false)) //DFG+E+W+Q+IGN
                    UseSpells(Target, "NE", true, true, true, false, true, true);
                else if (TheCombo == "|DFG|E+W+R+Q+IGN" && HasMana(true, true, true, true)) //DFG+E+W+R+Q+IGN
                    UseSpells(Target, "NE", true, true, true, true, true, true);
                else if (TheCombo == "IGN" && HasMana(false, false, false, false)) //IGN
                    UseSpells(Target, "N", false, false, false, false, false, true);
                else if (TheCombo == "Unkillable" && HasMana(false, false, false, false)) //Unkillable
                    if (menu.Item("Harassifnk").GetValue<bool>())
                        UseSpells(Target, "N", true, false, false, false, false, false);
                    else
                        UseSpells(Target, "N", false, false, false, false, false, false);
            }
        }

        //Uses selected abilities
        private static void UseSpells(Obj_AI_Hero T, string Source, bool QQ, bool WW, bool EE, bool RR, bool DFGG, bool IGNN)
        {
            if (EE && T != null)
            {
                if (Player.Distance(T.Position) <= E.Range)
                {
                    if (E.IsReady() && !IsImmune(T) || !menu.Item("DontEShields").GetValue<bool>())
                    {
                        castE((Obj_AI_Hero)T);
                    }
                }
            }

            if (WW && T != null)
            {
                if (W.IsReady())
                {
                    var pred = W.GetPrediction(T);
                    if (pred.Hitchance == HitChance.Immobile && W.IsReady())
                        W.Cast(T.ServerPosition, Packets());
                }
            }

            if (DFGG && T != null)
            {
                if (Dfg.IsReady() && !HasBuffs(T))
                {
                    if (Source == "NE")
                    {
                        if (!W.IsReady())
                            Items.UseItem(Dfg.Id, T);
                    }
                    else
                    {
                        Items.UseItem(Dfg.Id, T);
                    }

                }
            }

            if (RR && T != null)
            {
                if (R.IsReady() && !HasBuffs(T))
                {
                    if (Source == "NE")
                    {
                        if (!W.IsReady())
                            R.CastOnUnit(T, Packets());
                    }
                    else
                    {
                        R.CastOnUnit(T, Packets());
                    }

                }
            }

            if (QQ && T != null && !HasBuffs(T))
            {
                if (Source == "NE")
                {
                    if (!W.IsReady())
                        Q.CastOnUnit(T, Packets());
                }
                else if (Source != "EWQHarass" && Source != "QHarass")
                {
                    Q.CastOnUnit(T, Packets());
                }
                else if (Source == "EWQHarass")
                {
                    if (!menu.Item("WaitW").GetValue<bool>() || !W.IsReady())
                        Q.CastOnUnit(T, Packets());
                }
                else if (Source == "QHarass")
                {
                    if (Player.Distance(T.Position) <= Q.Range)
                        Q.CastOnUnit(T, Packets());
                }

            }

            if (IGNN && T != null && IgniteSlot != SpellSlot.Unknown)
            {
                if (Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && !HasBuffs(T))
                {
                    if (Source == "NE")
                    {
                        if (!W.IsReady())
                            Player.Spellbook.CastSpell(IgniteSlot, T);
                    }
                    else
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, T);
                    }
                }
            }
        }

        //The Main function that decides which combo to use and what is the needed cooldown
        private static Tuple<string, double> GetBestCombo(Obj_AI_Hero x, string Source)
        {
            string BestCombo = null;
            double NeededCooldown = 0d;

            if (GetComboDamage(x, Source, true, false, false, false, false, false) > x.Health && Exists(true, false, false, false, false, false)) //Q
            {
                if (!UpCD(true, false, false, false, false, false))
                {
                    NeededCooldown = UpCDD(true, false, false, false, false, false);
                }
                BestCombo = "Q";
            }
            else if (GetComboDamage(x, Source, false, true, true, false, false, false) > x.Health && Exists(false, true, true, false, false, false)) //E+W
            {
                if (!UpCD(false, true, true, false, false, false))
                {
                    NeededCooldown = UpCDD(false, true, true, false, false, false);
                }
                BestCombo = "E+W";
            }
            else if (GetComboDamage(x, Source, true, true, true, false, false, false) > x.Health && Exists(true, true, true, false, false, false)) //E+W+Q
            {
                if (!UpCD(true, true, true, false, false, false))
                {
                    NeededCooldown = UpCDD(true, true, true, false, false, false);
                }
                BestCombo = "E+W+Q";
            }
            else if (GetComboDamage(x, Source, true, true, true, false, true, false) > x.Health && Exists(true, true, true, false, true, false)) //DFG+E+W+Q
            {
                if (!UpCD(true, true, true, false, true, false))
                {
                    NeededCooldown = UpCDD(true, true, true, false, true, false);
                }
                BestCombo = "|DFG|E+W+Q";
            }
            else if (GetComboDamage(x, Source, false, true, true, false, true, false) > x.Health && Exists(false, true, true, false, true, false)) //DFG+E+W
            {
                if (!UpCD(false, true, true, false, true, false))
                {
                    NeededCooldown = UpCDD(false, true, true, false, true, false);
                }
                BestCombo = "|DFG|E+W";
            }
            else if (GetComboDamage(x, Source, true, false, false, false, true, false) > x.Health && Exists(true, false, false, false, true, false)) //DFG+Q
            {
                if (!UpCD(true, false, false, false, true, false))
                {
                    NeededCooldown = UpCDD(true, false, false, false, true, false);
                }
                BestCombo = "|DFG|Q";
            }
            else if (GetComboDamage(x, Source, false, true, true, true, false, false) > x.Health && Exists(false, true, true, true, false, false)) //E+W+R
            {
                if (!UpCD(false, true, true, true, false, false))
                {
                    NeededCooldown = UpCDD(false, true, true, true, false, false);
                }
                BestCombo = "E+W+R";
            }
            else if (GetComboDamage(x, Source, true, true, true, true, false, false) > x.Health && Exists(true, true, true, true, false, false)) //E+W+R+Q
            {
                if (!UpCD(true, true, true, true, false, false))
                {
                    NeededCooldown = UpCDD(true, true, true, true, false, false);
                }
                BestCombo = "E+W+R+Q";
            }
            else if (GetComboDamage(x, Source, false, false, true, true, false, false) > x.Health && Exists(false, false, true, true, false, false)) //E+R
            {
                if (!UpCD(false, false, true, true, false, false))
                {
                    NeededCooldown = UpCDD(false, false, true, true, false, false);
                }
                BestCombo = "E+R";
            }
            else if (GetComboDamage(x, Source, false, false, false, true, false, false) > x.Health && Exists(false, false, false, true, false, false)) //R
            {
                if (!UpCD(false, false, false, true, false, false))
                {
                    NeededCooldown = UpCDD(false, false, false, true, false, false);
                }
                BestCombo = "R";
            }
            else if (GetComboDamage(x, Source, true, false, true, true, true, false) > x.Health && Exists(true, false, true, true, true, false)) //DFG+E+R+Q
            {
                if (!UpCD(true, false, true, true, true, false))
                {
                    NeededCooldown = UpCDD(true, false, true, true, true, false);
                }
                BestCombo = "|DFG|E+R+Q";
            }
            else if (GetComboDamage(x, Source, false, true, true, true, true, false) > x.Health && Exists(false, true, true, true, true, false)) //DFG+E+W+R
            {
                if (!UpCD(false, true, true, true, true, false))
                {
                    NeededCooldown = UpCDD(false, true, true, true, true, false);
                }
                BestCombo = "|DFG|E+W+R";
            }
            else if (GetComboDamage(x, Source, true, true, true, true, true, false) > x.Health && Exists(true, true, true, true, true, false)) //DFG+E+W+R+Q
            {
                if (!UpCD(true, true, true, true, true, false))
                {
                    NeededCooldown = UpCDD(true, true, true, true, true, false);
                }
                BestCombo = "|DFG|E+W+R+Q";
            }
            else if (GetComboDamage(x, Source, false, false, false, true, true, false) > x.Health && Exists(false, false, false, true, true, false)) //DFG+R
            {
                if (!UpCD(false, false, false, true, true, false))
                {
                    NeededCooldown = UpCDD(false, false, false, true, true, false);
                }
                BestCombo = "|DFG|R";
            }
            else if (GetComboDamage(x, Source, true, false, true, false, false, true) > x.Health && Exists(true, false, true, false, false, true)) //E+Q+IGN
            {
                if (!UpCD(true, false, true, false, false, true))
                {
                    NeededCooldown = UpCDD(true, false, true, false, false, true);
                }
                BestCombo = "E+Q+IGN";
            }
            else if (GetComboDamage(x, Source, false, true, true, false, false, true) > x.Health && Exists(false, true, true, false, false, true)) //E+W+IGN
            {
                if (!UpCD(false, true, true, false, false, true))
                {
                    NeededCooldown = UpCDD(false, true, true, false, false, true);
                }
                BestCombo = "E+W+IGN";
            }
            else if (GetComboDamage(x, Source, true, true, true, false, false, true) > x.Health && Exists(true, true, true, false, false, true)) //E+W+Q+IGN
            {
                if (!UpCD(true, true, true, false, false, true))
                {
                    NeededCooldown = UpCDD(true, true, true, false, false, true);
                }
                BestCombo = "E+W+Q+IGN";
            }
            else if (GetComboDamage(x, Source, true, true, true, true, false, true) > x.Health && Exists(true, true, true, true, false, true)) //E+W+R+Q+IGN
            {
                if (!UpCD(true, true, true, true, false, true))
                {
                    NeededCooldown = UpCDD(true, true, true, true, false, true);
                }
                BestCombo = "E+W+R+Q+IGN";
            }
            else if (GetComboDamage(x, Source, true, false, true, false, true, true) > x.Health && Exists(true, false, true, false, true, true)) //DFG+E+Q+IGN
            {
                if (!UpCD(true, false, true, false, true, true))
                {
                    NeededCooldown = UpCDD(true, false, true, false, true, true);
                }
                BestCombo = "|DFG|E+Q+IGN";
            }
            else if (GetComboDamage(x, Source, false, true, true, false, true, true) > x.Health && Exists(false, true, true, false, true, true)) //DFG+E+W+IGN
            {
                if (!UpCD(false, true, true, false, true, true))
                {
                    NeededCooldown = UpCDD(false, true, true, false, true, true);
                }
                BestCombo = "|DFG|E+W+IGN";
            }
            else if (GetComboDamage(x, Source, true, true, true, false, true, true) > x.Health && Exists(true, true, true, false, true, true)) //DFG+E+W+Q+IGN
            {
                if (!UpCD(true, true, true, false, true, true))
                {
                    NeededCooldown = UpCDD(true, true, true, false, true, true);
                }
                BestCombo = "|DFG|E+W+Q+IGN";
            }
            else if (GetComboDamage(x, Source, true, true, true, true, true, true) > x.Health && Exists(true, true, true, true, true, true)) //DFG+E+W+R+Q+IGN
            {
                if (!UpCD(true, true, true, true, true, true))
                {
                    NeededCooldown = UpCDD(true, true, true, true, true, true);
                }
                BestCombo = "|DFG|E+W+R+Q+IGN";
            }
            else if (GetComboDamage(x, Source, false, false, false, false, false, true) > x.Health && Exists(false, false, false, false, false, true))  //IGN
            {
                if (!UpCD(false, false, false, false, false, true))
                {
                    NeededCooldown = UpCDD(false, false, false, false, false, true);
                }
                BestCombo = "IGN";
            }
            if (BestCombo == null) //Not Killable
            {
                BestCombo = "Unkillable";
            }

            if (!x.IsVisible)
                BestCombo = "N/A";

            return Tuple.Create(BestCombo, NeededCooldown);

        }

        //Q last Hitting
        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;
            if (menu.Item("SaveE").GetValue<bool>() && !HasMana(false, false, true, false) && Exists(false, false, true, false, false, false)) return;
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget() && !minion.IsDead && HealthPrediction.GetHealthPrediction(minion, (int)((minion.Distance(Player.Position) / 1500) * 1000 + .25f * 1000), 100) <= Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        if (Q.IsReady())
                        {
                            Q.Cast(minion, Packets());
                            return;
                        }
                    }
                }
            }
        }

        //W Lane Crearing
        public static void lastHitW()
        {
            var laneMinions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
            List<Vector2> minionerinos2 =
     (from minions in laneMinions select minions.Position.To2D()).ToList();
            var ePos2 = MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).Position;

            if (ePos2.Distance(Player.Position.To2D()) < W.Range && MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).MinionsHit >= menu.Item("WAmount").GetValue<Slider>().Value)
            {
                W.Cast(ePos2, Packets());
            }
        }

        //Calculates the damage from selected abilities
        private static float GetComboDamage(Obj_AI_Base enemy, string source, bool A, bool B, bool C, bool D, bool EE, bool F)
        {
            double damage = 0d;
            if (source == "Draw")
            {
                if (EE)
                    damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

                if (A)
                    damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

                if (D)
                    damage += Player.GetSpellDamage(enemy, SpellSlot.R);

                if (B)
                    damage += Player.GetSpellDamage(enemy, SpellSlot.W);

                if (EE)
                    damage = damage * 1.2;

                if (IgniteSlot != SpellSlot.Unknown && F)
                    damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }
            else
            {
                if (Dfg.IsReady() && EE)
                    damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

                if (Q.IsReady() && A)
                    damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

                if (R.IsReady() && D)
                    damage += Player.GetSpellDamage(enemy, SpellSlot.R);

                if (W.IsReady() && B)
                    damage += Player.GetSpellDamage(enemy, SpellSlot.W);

                if (Dfg.IsReady() && EE)
                    damage = damage * 1.2;

                if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && F)
                    damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            if (Items.HasItem(3155, (Obj_AI_Hero)enemy))
            {
                damage = damage - 250;
            }

            if (Items.HasItem(3156, (Obj_AI_Hero)enemy))
            {
                damage = damage - 400;
            }
            return (float)damage;
        }

        //Checks if you have enough mana for casting selected abilities
        public static bool HasMana(bool A, bool B, bool C, bool D)
        {
            int QMana = 0;
            if (A) QMana = qMana[Q.Level];
            int WMana = 0;
            if (B) WMana = wMana[W.Level];
            int EMana = 0;
            if (C) EMana = eMana[E.Level];
            int RMana = 0;
            if (D) RMana = rMana[R.Level];
            int together = QMana + WMana + EMana + RMana;
            if (Player.Mana >= together)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Returns the time in seconds needed to regen mana for Q,E+W+Q,E+W+Q+R combos.
        public static Tuple<int, double> manaCheck()
        {
            int QMana = qMana[Q.Level];
            int EWQMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level];
            int EWQRMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level];
            int end = 0;
            double EndValue = 0d;

            if (Player.Mana < QMana)
            {
                if (Q.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] - Player.Mana;
                    EndValue = Math.Round(NeededMana / ManaRegen);
                    end = 1;
                }
            }

            else if (Player.Mana < EWQMana)
            {
                if (Q.Level != 0 && W.Level != 0 && E.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] - Player.Mana;
                    EndValue = Math.Round(NeededMana / ManaRegen);
                    end = 2;
                }
            }

            else if (Player.Mana < EWQRMana)
            {
                if (Q.Level != 0 && W.Level != 0 && E.Level != 0 && R.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level] - Player.Mana;
                    EndValue = Math.Round(NeededMana / ManaRegen);
                    end = 3;
                }
            }

            return Tuple.Create(end, EndValue);
        }

        //Check if needed abilities are ready
        public static bool UpCD(bool A, bool B, bool C, bool D, bool EEE, bool F)
        {
            bool AR = Q.IsReady();
            bool BR = W.IsReady();
            bool CR = E.IsReady();
            bool DR = R.IsReady();
            bool ER = Dfg.IsReady();
            bool FR = IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;

            if (A && !AR && Q.Level > 0)
            {
                return false;
            }
            else if (B && !BR && W.Level > 0)
            {
                return false;
            }
            else if (C && !CR && E.Level > 0)
            {
                return false;
            }
            else if (D && !DR && R.Level > 0)
            {
                return false;
            }
            else if (EEE && !ER && IgniteSlot == SpellSlot.Unknown)
            {
                return false;
            }
            else if (F && !FR && DFGSlot == SpellSlot.Unknown)
            {
                return false;
            }
            return true;
        }

        //Calculates the biggest needed cooldown for performing combo
        public static double UpCDD(bool A, bool B, bool C, bool D, bool EEE, bool F)
        {
            bool AR = Q.IsReady();
            bool BR = W.IsReady();
            bool CR = E.IsReady();
            bool DR = R.IsReady();
            bool ER = Dfg.IsReady();
            bool FR = IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready;

            double timeleft = 0f;
            if (A && !AR)
                if ((Math.Round(Q.Instance.CooldownExpires - Game.Time)) > timeleft)
                    timeleft = Math.Round(Q.Instance.CooldownExpires - Game.Time);
            if (B && !BR)
                if ((Math.Round(W.Instance.CooldownExpires - Game.Time)) > timeleft)
                    timeleft = Math.Round(W.Instance.CooldownExpires - Game.Time);
            if (C && !CR)
                if ((Math.Round(E.Instance.CooldownExpires - Game.Time)) > timeleft)
                    timeleft = Math.Round(E.Instance.CooldownExpires - Game.Time);
            if (D && !DR)
                if ((Math.Round(R.Instance.CooldownExpires - Game.Time)) > timeleft)
                    timeleft = (Math.Round(R.Instance.CooldownExpires - Game.Time));
            if (EEE && !ER)
                if (Math.Round((ObjectManager.Player.Spellbook.GetSpell(DFGSlot).CooldownExpires - Game.Time)) > timeleft)
                    timeleft = Math.Round((ObjectManager.Player.Spellbook.GetSpell(DFGSlot).CooldownExpires - Game.Time));
            if (F && !FR)
                if (Math.Round((ObjectManager.Player.Spellbook.GetSpell(IgniteSlot).CooldownExpires - Game.Time)) > timeleft)
                    timeleft = Math.Round((ObjectManager.Player.Spellbook.GetSpell(IgniteSlot).CooldownExpires - Game.Time));
            return Math.Round(timeleft);
        }

        //Checks if needed abilities are leveled up/exist
        public static bool Exists(bool A, bool B, bool C, bool D, bool EE, bool F)
        {
            int x = 0;

            if (A && Q.Level == 0)
                x++;
            if (B && W.Level == 0)
                x++;
            if (C && E.Level == 0)
                x++;
            if (D && R.Level == 0)
                x++;
            if (EE && DFGSlot == null)
                x++;
            if (F && IgniteSlot == null)
                x++;

            if (x > 0) return false;
            else return true;
        }

        //Returns how much more damage you need to deal to the target to ensure the kill OR how much you will overdamage to them
        private static Tuple<int, string> GetExtraNeeded(Obj_AI_Hero target)
        {
            var combodamage = GetComboDamage(target, "calc", true, true, true, true, true, true);
            double Damage = 0d;
            int OutPut = 0;
            string Text = null;

            if (combodamage > target.Health)
            {
                Damage = Math.Round(combodamage - target.Health);
                OutPut = (int)Damage;
                Text = "Extra:";
            }
            else
            {
                Damage = Math.Round(target.Health - combodamage);
                OutPut = (int)Damage;
                Text = "Need:";
            }
            if (!target.IsVisible)
            {
                Text = "N/A";
            }
            if (target.IsDead)
            {
                Text = "Dead";
            }

            return Tuple.Create(OutPut, Text);
        }

        //Left Click Target Locker
        public static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam == 1 && (menu.Item("LockTargets").GetValue<bool>())) // Left-Mouse
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    if (SharpDX.Vector2.Distance(Game.CursorPos.To2D(), enemy.ServerPosition.To2D()) < 200)
                    {
                        if (enemy.IsValidTarget())
                        {
                            if (Environment.TickCount + Game.Ping - TargetLockCD > 400)
                            {
                                if (ChoosedTarget == null)
                                {
                                    ChoosedTarget = enemy;
                                }
                                else
                                {
                                    if (ChoosedTarget.Name == enemy.Name)
                                    {
                                        ChoosedTarget = null;
                                    }
                                    else
                                    {
                                        ChoosedTarget = enemy;
                                    }
                                }
                                TargetLockCD = Environment.TickCount;
                            }
                        }
                    }
                }
            }
        }

        //Check if packet casting is turned ON/OFF
        public static bool Packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        //Gets Current Target
        private static Obj_AI_Hero GetTarget()
        {
            Obj_AI_Hero Target = null;
            if (ChoosedTarget == null)
            {
                Target = TargetSelector.GetTarget(1050, TargetSelector.DamageType.Magical);
            }
            else
            {
                Target = ChoosedTarget;
            }
            return Target;
        }

        //E CAST(UNIT)
        public static void castE(Obj_AI_Hero target)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;

            if (pred.Hitchance >= HitChance.High && E.IsReady())
            {
                E.Cast(castVec);
            }
        }

        //E CAST(COORDINATES)
        public static void castE(Vector3 pos)
        {
            Vector2 castVec = pos.To2D() -
                              Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width;

            if (E.IsReady())
            {
                E.Cast(castVec);
            }
        }

        //Get Nearest Enemy Hero around "unit"
        public static Obj_AI_Hero GetNearestEnemy(Obj_AI_Hero unit)
        {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid && Player.Distance(x.Position) <= E.Range)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        //Uses Items
        public static void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        //Loads standart or xSLx Orbwalker
        private static void chooseOrbwalker(bool mode)
        {
            if (mode)
            {
                Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                Game.PrintChat("Regular Orbwalker Loaded");
            }
            else
            {
                xSLxOrbwalker.AddToMenu(orbwalkerMenu);
                Game.PrintChat("xSLx Orbwalker Loaded");
            }
        }

        //Automatically uses Health Pots when ignite/morde buff is on you
        public static void AutoHP()
        {
            if (Player.HasBuff("summonerdot") || Player.HasBuff("MordekaiserChildrenOfTheGrave"))
            {
                if (!Utility.InFountain())

                    if (Items.HasItem(biscuit.Id) && Items.CanUseItem(biscuit.Id) && !Player.HasBuff("ItemMiniRegenPotion"))
                    {
                        biscuit.Cast(Player);
                    }
                    else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) && !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("Health Potion"))
                    {
                        HPpot.Cast(Player);
                    }
                    else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) && !Player.HasBuff("ItemCrystalFlask"))
                    {
                        Flask.Cast(Player);
                    }
            }
        }

        //Buys listed items(Gets called once in a game in fountain when you have 475 gold)
        public static void BuyItems()
        {
            if (Utility.InFountain() && ObjectManager.Player.Gold == 475 && !boughtItemOne)
            {
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Warding_Totem_Trinket);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Faerie_Charm);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Health_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Mana_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Mana_Potion);
                ObjectManager.Player.BuyItem(LeagueSharp.ItemId.Mana_Potion);
                boughtItemOne = true;
            }
        }

        //Uses E on the end point of enemy Gapclosers
        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("UseGap").GetValue<bool>()) return;
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                castE((Vector3)gapcloser.End);
        }

        //Interrupts Dangerous enemy spells with E
        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit.Position) < E.Range && unit != null && E.IsReady())
            {
                castE((Obj_AI_Hero)unit);
            }
        }

        private static void TowerAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!E.IsReady() || !menu.Item("StunUnderTower").GetValue<bool>())
            {
                return;
            }

            if (sender.IsValid<Obj_SpellMissile>())
            {
                var missile = (Obj_SpellMissile)sender;

                // Ally Turret -> Enemy Hero
                if (missile.SpellCaster.IsValid<Obj_AI_Turret>() && missile.SpellCaster.IsAlly &&
                    missile.Target.IsValid<Obj_AI_Hero>() && missile.Target.IsEnemy)
                {
                    var turret = (Obj_AI_Turret)missile.SpellCaster;
                    if (Player.Distance(missile.Target.Position) < 1050)
                        castE((Obj_AI_Hero)missile.Target);
                }
            }
        }
    }
}