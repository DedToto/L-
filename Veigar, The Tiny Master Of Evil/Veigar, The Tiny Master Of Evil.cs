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
using KeMinimap;
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
        private static Obj_AI_Hero WimmTarget = null;
        public static int Orb = 0;
        public static int ComboStarted = 0;
        public static float Delay = 0f;
        public static float Delay1 = 0f;
        public static string Ccombo = null;
        public static float Delayy = 0f;
        public static float Delayy1 = 0f;
        public static string Cccombo = null;

        //HUD
        public static List<HUD> HUDlist = new List<HUD>
        {
            new HUD()
            {
                DisplayTextON = "Combo : On", DisplayTextOFF = "Combo : Off", MenuText = "Display Combo Status", MenuComboText = "Combo"
            },
            new HUD()
            {
                DisplayTextON = "Harass : On", DisplayTextOFF = "Harass : Off", MenuText = "Display Harass Status", MenuComboText = "HarassActive"
            },
            new HUD()
            {
                DisplayTextON = "UseAll : On", DisplayTextOFF = "UseAll : Off", MenuText = "Display Use All Status", MenuComboText = "AllInActive"
            },
            new HUD()
            {
                DisplayTextON = "Q LastHit : On", DisplayTextOFF = "Q LastHit : Off", MenuText = "Display Q farm Status", MenuComboText = "LastHitQQ"
            },
            new HUD()
            {
                DisplayTextON = "AutoKS : On", DisplayTextOFF = "AutoKS : Off", MenuText = "Display KS Status", MenuComboText = "AutoKST"
            },
            new HUD()
            {
                DisplayTextON = "StunClosest : On", DisplayTextOFF = "StunClosest : Off", MenuText = "Display Stun Closest Status", MenuComboText = "Stun Closest Enemy"
            },
            new HUD()
            {
                DisplayTextON = "LaneClearW : On", DisplayTextOFF = "LaneClearW : Off", MenuText = "Display LaneClear Status", MenuComboText = "LastHitWW"
            },
        };

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
            new NewBuff()
            {
                MenuName = "Guardian Angel", DisplayName = "Guardian Angel", Name = "willrevive"
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
        public static int NeededCD = 0;

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

            Dfg = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline ||
                  Utility.Map.GetMap().Type == Utility.Map.MapType.CrystalScar
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
            menu.SubMenu("Keys").AddItem(new MenuItem("JungleActive", "Jungle Farm").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Keys").AddItem(new MenuItem("InfoTable", "Show Info Table[FPS]").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));

            //Drawings menu:
            menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("HUDdisplay", "Heads-up Display").SetValue(true));
            menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("HUDX", "X axis").SetValue(new Slider(67, 0, 100)));
            menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("HUDY", "Y axis").SetValue(new Slider(86, 0, 100)));
            foreach (var hud in HUDlist.Where(hud => hud.MenuText != "Display KS Status" && hud.MenuText != "Display Stun Closest Status" && hud.MenuText != "Display LaneClear Status"))
                menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("U" + hud.MenuText, hud.MenuText).SetValue(true));
            foreach (var hud in HUDlist.Where(hud => hud.MenuText == "Display KS Status" || hud.MenuText == "Display Stun Closest Status" || hud.MenuText == "Display LaneClear Status"))
                menu.SubMenu("Drawings").SubMenu("HUD Settings").AddItem(new MenuItem("U" + hud.MenuText, hud.MenuText).SetValue(false));

            menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q,R range").SetValue(new Circle(true, Color.FromArgb(255, 0, 255, 0))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, Color.FromArgb(255, 255, 0, 255))));
            menu.SubMenu("Drawings").AddItem(new MenuItem("MinionMarker", "Mark Q Farm Minions").SetValue(new Circle(true, Color.Green)));
            menu.SubMenu("Drawings").AddItem(new MenuItem("TText", "Mark Targets with Circles").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("LText", "Display Locked Target[HP BAR]").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("manaStatus", "Mana status").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("ExtraNeeded", "Show Extra/Needed Damage").SetValue(true));
            menu.SubMenu("Drawings").AddItem(new MenuItem("OptimalCombo", "Show Best Kill Combo[FPS]").SetValue(false));


            //Misc menu:
            menu.AddSubMenu(new Menu("Other", "Other"));
            menu.SubMenu("Other").AddItem(new MenuItem("StunUnderTower", "Stun Enemies Attacked by Tower").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("UseInt", "Use E to Interrupt").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("UseGap", "Use E against GapClosers").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("Wimm", "Use W on CC'ed targets in range").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("PotOnIGN", "Use HP Pot when ignited").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("buystart", "Buy Starting Items").SetValue(new KeyBind("P".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Other").AddItem(new MenuItem("Reset", "Remove Target Lock").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Press, false)));
            menu.SubMenu("Other").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));
            menu.SubMenu("Other").AddItem(new MenuItem("Show", "Display Sprite").SetValue(true));

            //Farm menu:
            menu.AddSubMenu(new Menu("Farm", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("dontfarm", "Disable Q farm when using any combos").SetValue(true));
            menu.SubMenu("Farm").AddItem(new MenuItem("SaveE", "Save Mana for E while Farming").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("OnlySiege", "Last hit only siege creeps").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("WAmount", "Min Minions To Land W").SetValue(new Slider(3, 1, 7)));
            menu.SubMenu("Farm").AddItem(new MenuItem("FarmMove", "Move To mouse").SetValue(new StringList(new[] { "Never", "Lane Clear", "Q farm", "Lane Clear & Q farm" }, 0)));

            //Jungle Farm menu:
            menu.AddSubMenu(new Menu("Jungle Farm", "Jungle Clear"));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseAAJungle", "Use AA").SetValue(true));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseQJungle", "Use Q").SetValue(true));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseWJungle", "Use W").SetValue(true));
            menu.SubMenu("Jungle Clear").AddItem(new MenuItem("UseEJungle", "Use E").SetValue(true));

            //Auto KS
            menu.AddSubMenu(new Menu("Auto KS", "AutoKS"));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseWKS", "Use W").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseRKS", "Use R").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseDFGKS", "Use DFG").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("UseIGNKS", "Use IGN").SetValue(false));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("RangeKS", "KS only when in shortest needed spell range").SetValue(true));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("DisableKS", "Disable KS when using combos").SetValue(true));
            menu.SubMenu("AutoKS").AddItem(new MenuItem("AutoKST", "AutoKS (toggle)!").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle, true)));

            //Harass menu:
            menu.AddSubMenu(new Menu("Harass", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("HarassMode", "Choose Harass Type").SetValue(new StringList(new[] { "E+W+Q", "E+W", "Q", "None" }, 0)));
            menu.SubMenu("Harass").AddItem(new MenuItem("WaitW", "Cast W before Q").SetValue(false));

            //Combo menu:
            menu.AddSubMenu(new Menu("Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("LockTargets", "Lock Targets with Left Click").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("DontEShields", "Dont use E in spell shields").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("DontWimm", "Don't Use W on CC'ed targets in range misc when comboing").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ToOrb", "OrbWalk when using any combat functions").SetValue(false));
            menu.SubMenu("Combo").AddItem(new MenuItem("CastMode", "E and W settings").SetValue(new StringList(new[] { "Use E before W", "Use W before E", }, 0)));
            menu.SubMenu("Combo").AddItem(new MenuItem("ComboMode", "Combo config for unkillable targets").SetValue(new StringList(new[] { "Choosed Harass Mode", "Q Harass", "None" }, 2)));
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

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Delay != 0f)
            {
                //int I = Environment.TickCount;
                //Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 50, System.Drawing.Color.LightGreen, "Combo:" + Ccombo + "(" + (Environment.TickCount - Delay1) + "/" + Delay + ")");
                if (Environment.TickCount - Delay1 > Delay)
                {
                    Delay = 0f;
                    Delay1 = 0f;
                    Ccombo = null;
                }
            }

            if (Delayy != 0f)
            {
                //int I = Environment.TickCount;
                //Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 50, System.Drawing.Color.LightGreen, "Combo:" + Ccombo + "(" + (Environment.TickCount - Delay1) + "/" + Delay + ")");
                if (Environment.TickCount - Delayy1 > Delayy)
                {
                    Delayy = 0f;
                    Delayy1 = 0f;
                    Cccombo = null;
                }
            }

            #region OnUpdate
            if (ChoosedTarget != null && ChoosedTarget.IsDead || !menu.Item("LockTargets").GetValue<bool>())
                ChoosedTarget = null;

            Target = GetTarget();

            var point = Player.ServerPosition + 300 * (Game.CursorPos.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();

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
                if (menu.Item("JungleActive").GetValue<KeyBind>().Active)
                {
                    JungleFarm();
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active)
                {
                    Harass();
                }

                if (menu.Item("LastHitWW").GetValue<KeyBind>().Active)
                {
                    lastHitW();
                    if (menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 1 || menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 3) if (Player.ServerPosition.Distance(Game.CursorPos) > 55) Player.IssueOrder(GameObjectOrder.MoveTo, point);
                }

                if (menu.Item("AllInActive").GetValue<KeyBind>().Active)
                {
                    AllIn();
                }

                if (menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active)
                {
                    if (Player.ServerPosition.Distance(Game.CursorPos) > 55 && !KeMinimap.Minimap.MouseOver) Player.IssueOrder(GameObjectOrder.MoveTo, point);
                    if (E.IsReady())
                    {
                        castE(GetNearestEnemy(Player));
                    }
                }
            }
            else
            {
                Combo("Combo");
                //if (menu.Item("ToOrb").GetValue<bool>()) if (Orb == 2) xSLx_Orbwalker.xSLxOrbwalker.Orbwalk(Game.CursorPos, Target); else if (Orb == 1) Orbwalking.Orbwalk(Target, Game.CursorPos);
            }

            if (menu.Item("AutoKST").GetValue<KeyBind>().Active)
            {
                AutoKS();
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
                enemyDictionary1 = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).ToDictionary(enemy => enemy, enemy => GetBestCombo(enemy, "Table"));
            }

            if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
            {
                if (menu.Item("AllInActive").GetValue<KeyBind>().Active || menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active || menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("Combo").GetValue<KeyBind>().Active && menu.Item("dontfarm").GetValue<bool>()) return;
                if (menu.Item("OnlySiege").GetValue<bool>())
                {
                    _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.BaseSkinName.Contains("Siege") && m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q) - 20)) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);
                }
                else
                {
                    _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q) - 20)) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);
                }
                lastHit();
                if (menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 2 || menu.Item("FarmMove").GetValue<StringList>().SelectedIndex == 3) if (Player.ServerPosition.Distance(Game.CursorPos) > 55) Player.IssueOrder(GameObjectOrder.MoveTo, point);
            }

            if (menu.Item("Wimm").GetValue<bool>())
            {
                WimmTarget = ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget()).FirstOrDefault(m => m.IsValidTarget(E.Range) && m.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && b.Name != "VeigarStun" && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay);
                if (WimmTarget != null && !WimmTarget.HasBuff("VeigarStun") && !menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active)
                    W.Cast(WimmTarget);
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
                    if (menu.Item("LText").GetValue<bool>()) Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 25, System.Drawing.Color.LightGreen, "Lock:" + ChoosedTarget.ChampionName);
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
                            Drawing.DrawText(enemy.HPBarPosition.X + 7, enemy.HPBarPosition.Y + 40, System.Drawing.Color.White, enemyDictionary1[enemy]);
                        }
                    }
                }
            }

            if (menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
            {
                var MinMar = menu.Item("MinionMarker").GetValue<Circle>();
                if (_m != null)
                    Utility.DrawCircle(_m.Position, 75, MinMar.Color);
            }

            #region Indicators
            if (menu.Item("HUDdisplay").GetValue<bool>())
            {
                float X = (float)menu.Item("HUDX").GetValue<Slider>().Value / 100;
                float Y = (float)menu.Item("HUDY").GetValue<Slider>().Value / 100;
                foreach (var hud in HUDlist.Where(hud => "U" + hud.MenuText == menu.Item("U" + hud.MenuText).Name && menu.Item("U" + hud.MenuText).GetValue<bool>()))
                {
                    if (menu.Item(hud.MenuComboText).GetValue<KeyBind>().Active)
                        Drawing.DrawText(Drawing.Width * X, Drawing.Height * Y, System.Drawing.Color.Yellow, hud.DisplayTextON);
                    else
                        Drawing.DrawText(Drawing.Width * X, Drawing.Height * Y, System.Drawing.Color.DarkRed, hud.DisplayTextOFF);
                    Y = Y + 0.02f;
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
                var y = Drawing.Height * 0.61f;
                Drawing.DrawText(x, y - 20f, System.Drawing.Color.White, "~INFO TABLE~");
                //Applies the function to all enemy heroes
                foreach (Obj_AI_Hero enemy in enemyDictionary1.Keys)
                {
                    if (!enemy.IsDead)
                    {
                        float ENNdamage = 0f;
                        if (GetExtraNeeded(enemy).Item1 >= 1000)
                        {
                            int Integer = GetExtraNeeded(enemy).Item1;
                            float Floater = Integer;
                            ENNdamage = (float)Math.Round((Floater / 1000), 1);
                        }
                        else
                        {
                            ENNdamage = GetExtraNeeded(enemy).Item1;
                        }
                        if (enemyDictionary1[enemy] == "Unkillable")
                        {
                            if (GetExtraNeeded(enemy).Item1 >= 1000)
                            {
                                Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + ENNdamage + "k+)");
                            }
                            else
                            {
                                Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + GetExtraNeeded(enemy).Item1 + ")");
                            }
                        }
                        else
                        {
                            Drawing.DrawText(x, y, System.Drawing.Color.White, enemy.ChampionName + ":" + enemyDictionary1[enemy] + "(" + GetExtraNeeded(enemy).Item1 + ")");
                        }
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
            if (menu.Item("AllInActive").GetValue<KeyBind>().Active || menu.Item("Stun Closest Enemy").GetValue<KeyBind>().Active || menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("Combo").GetValue<KeyBind>().Active && menu.Item("DisableKS").GetValue<bool>()) return;
            if (Target != null && Player.Distance(Target.Position) <= NeededRange(menu.Item("UseQKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseWKS").GetValue<bool>(), menu.Item("UseRKS").GetValue<bool>(), menu.Item("UseDFGKS").GetValue<bool>(), menu.Item("UseIGNKS").GetValue<bool>()) || !menu.Item("RangeKS").GetValue<bool>() && Player.Distance(Target.Position) <= E.Range)
            {
                Combo("KS");
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
            if (menu.Item("ToOrb").GetValue<bool>()) if (Orb == 2) xSLx_Orbwalker.xSLxOrbwalker.Orbwalk(Game.CursorPos, Target); else if (Orb == 1) Orbwalking.Orbwalk(Target, Game.CursorPos);
        }

        //The Main Smart Combo that chooses the most efficient combo that will ensure the kill
        private static void Combo(string Source)
        {
            if (Target != null && Player.Distance(Target.Position) <= E.Range || Source == "KS")
            {
                string TheCombo = null;

                if (Source == "Combo")
                {
                    TheCombo = GetBestCombo(Target, "Comboing");
                }
                else
                {
                    TheCombo = GetBestCombo(Target, "KS");
                }

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
                    if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 2)
                        return;
                    else if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 0)
                        Harass();
                    else if (menu.Item("ComboMode").GetValue<StringList>().SelectedIndex == 1)
                        UseSpells(Target, "N", true, false, false, false, false, false);
            }
        }

        //Uses selected abilities
        private static void UseSpells(Obj_AI_Hero T, string Source, bool QQ, bool WW, bool EE, bool RR, bool DFGG, bool IGNN)
        {

            if (Player.Distance(T, true) < Math.Pow(NeededRange(QQ, WW, EE, RR, DFGG, IGNN), 2) && Player.Distance(T, true) > Math.Pow(NeededRange(QQ, WW, EE, RR, DFGG, IGNN), 2)) ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, T);

            if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
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
                        if (Source == "NE")
                        {
                            if (pred.Hitchance == HitChance.VeryHigh || T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay && W.IsReady())
                                W.Cast(T.ServerPosition, Packets());
                        }
                        else
                        {
                            if (pred.Hitchance == HitChance.Immobile || T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay && W.IsReady())
                                W.Cast(T.ServerPosition, Packets());
                        }
                    }
                }
            }
            else if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 1)
            {
                if (WW && T != null)
                {
                    if (W.IsReady())
                    {
                        var pred = W.GetPrediction(T);
                        if (EE)
                        {
                            if (E.IsReady() && pred.Hitchance == HitChance.VeryHigh) W.Cast(T.ServerPosition, Packets());
                        }
                        else
                        {
                            if (pred.Hitchance == HitChance.VeryHigh) W.Cast(T.ServerPosition, Packets());
                        }
                    }
                }

                if (EE && T != null)
                {
                    if (Player.Distance(T.Position) <= E.Range)
                    {
                        if (E.IsReady() && !IsImmune(T) || !menu.Item("DontEShields").GetValue<bool>())
                        {
                            if (WW)
                            {
                                if (!W.IsReady()) castE((Obj_AI_Hero)T);
                            }
                            else
                            {
                                castE((Obj_AI_Hero)T);
                            }
                        }
                    }
                }
            }

            if (DFGG && T != null)
            {
                if (Dfg.IsReady() && !HasBuffs(T))
                {
                    if (Source == "NE")
                    {
                        if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (!W.IsReady())
                                Items.UseItem(Dfg.Id, T);
                        }
                        else
                        {
                            if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                                Items.UseItem(Dfg.Id, T);
                        }
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
                        if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (!W.IsReady())
                                R.CastOnUnit(T, Packets());
                        }
                        else
                        {
                            if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                                R.CastOnUnit(T, Packets());
                        }
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
                    if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                    {
                        Q.CastOnUnit(T, Packets());
                    }
                    else
                    {
                        if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                            Q.CastOnUnit(T, Packets());
                    }
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
                        if (menu.Item("CastMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            if (!W.IsReady())
                                Player.Spellbook.CastSpell(IgniteSlot, T);
                        }
                        else
                        {
                            if (T.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay)
                                Player.Spellbook.CastSpell(IgniteSlot, T);
                        }
                    }
                    else
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, T);
                    }
                }
            }
        }

        //The Main function that decides which combo to use and what is the needed cooldown
        private static string GetBestCombo(Obj_AI_Hero x, string Source)
        {
            string BestCombo = null;

            if (GetComboDamage(x, Source, true, false, false, false, false, false) > x.Health) //Q
            {
                if (HasMana(true, false, false, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>())
                        {
                            BestCombo = "Q";
                        }
                    }
                }

            }
            else if (GetComboDamage(x, Source, false, true, true, false, false, false) > x.Health) //E+W
            {
                if (HasMana(false, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W";
                    }
                    else
                    {
                        if (menu.Item("UseWKS").GetValue<bool>())
                        {
                            BestCombo = "E+W";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, false, false, false) > x.Health) //E+W+Q
            {
                if (HasMana(true, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W+Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>())
                        {
                            BestCombo = "E+W+Q";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, false, true, false) > x.Health) //DFG+E+W+Q
            {
                if (HasMana(true, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W+Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W+Q";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, true, true, false, true, false) > x.Health) //DFG+E+W
            {
                if (HasMana(false, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W";
                    }
                    else
                    {
                        if (menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, false, false, false, true, false) > x.Health) //DFG+Q
            {
                if (HasMana(true, false, false, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|Q";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, true, true, true, false, false) > x.Health) //E+W+R
            {
                if (HasMana(false, true, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W+R";
                    }
                    else
                    {
                        if (menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>())
                        {
                            BestCombo = "E+W+R";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, true, false, false) > x.Health) //E+W+R+Q
            {
                if (HasMana(true, true, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W+R+Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>())
                        {
                            BestCombo = "E+W+R+Q";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, false, true, true, false, false) > x.Health) //E+R
            {
                if (HasMana(false, false, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+R";
                    }
                    else
                    {
                        if (menu.Item("UseEKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>())
                        {
                            BestCombo = "E+R";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, false, false, true, false, false) > x.Health) //R
            {
                if (HasMana(false, false, false, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "R";
                    }
                    else
                    {
                        if (menu.Item("UseRKS").GetValue<bool>())
                        {
                            BestCombo = "R";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, false, true, true, true, false) > x.Health) //DFG+E+R+Q
            {
                if (HasMana(true, false, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+R+Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+R+Q";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, true, true, true, true, false) > x.Health) //DFG+E+W+R
            {
                if (HasMana(false, true, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W+R";
                    }
                    else
                    {
                        if (menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W+R";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, true, true, false) > x.Health) //DFG+E+W+R+Q
            {
                if (HasMana(true, true, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W+R+Q";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W+R+Q";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, false, false, true, true, false) > x.Health) //DFG+R
            {
                if (HasMana(false, false, false, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|R";
                    }
                    else
                    {
                        if (menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|R";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, false, true, false, false, true) > x.Health) //E+Q+IGN
            {
                if (HasMana(true, false, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+Q+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "E+Q+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, true, true, false, false, true) > x.Health) //E+W+IGN
            {
                if (HasMana(false, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "E+W+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, false, false, true) > x.Health) //E+W+Q+IGN
            {
                if (HasMana(true, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W+Q+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "E+W+Q+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, true, false, true) > x.Health) //E+W+R+Q+IGN
            {
                if (HasMana(true, true, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "E+W+R+Q+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "E+W+R+Q+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, false, true, false, true, true) > x.Health) //DFG+E+Q+IGN
            {
                if (HasMana(true, false, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+Q+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+Q+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, true, true, false, true, true) > x.Health) //DFG+E+W+IGN
            {
                if (HasMana(false, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, false, true, true) > x.Health) //DFG+E+W+Q+IGN
            {
                if (HasMana(true, true, true, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W+Q+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W+Q+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, true, true, true, true, true, true) > x.Health) //DFG+E+W+R+Q+IGN
            {
                if (HasMana(true, true, true, true))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "|DFG|E+W+R+Q+IGN";
                    }
                    else
                    {
                        if (menu.Item("UseQKS").GetValue<bool>() && menu.Item("UseWKS").GetValue<bool>() && menu.Item("UseRKS").GetValue<bool>() && menu.Item("UseDFGKS").GetValue<bool>() && menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "|DFG|E+W+R+Q+IGN";
                        }
                    }
                }
            }
            else if (GetComboDamage(x, Source, false, false, false, false, false, true) > x.Health)  //IGN
            {
                if (HasMana(false, false, false, false))
                {
                    if (Source != "KS")
                    {
                        BestCombo = "IGN";
                    }
                    else
                    {
                        if (menu.Item("UseIGNKS").GetValue<bool>())
                        {
                            BestCombo = "IGN";
                        }
                    }
                }
            }

            if (BestCombo == null) //Not Killable
            {
                BestCombo = "Unkillable";
            }

            if (x.IsDead)
            {
                Ccombo = null;
                Cccombo = null;
            }

            if (Source == "Comboing")
            {
                if (Ccombo == null && BestCombo != "Unkillable")
                {
                    Delay = CastTime(BestCombo);
                    Delay1 = Environment.TickCount;
                    Ccombo = BestCombo;
                }
            }

            if (Source == "KS")
            {
                if (Cccombo == null && BestCombo != "Unkillable")
                {
                    Delayy = CastTime(BestCombo);
                    Delayy1 = Environment.TickCount;
                    Cccombo = BestCombo;
                }
            }

            if (Source != "KS" && Source != "Comboing")
            {
                if (!x.IsVisible)
                    BestCombo = "N/A";
            }

            if (Source == "Comboing" || Source == "KS")
            {
                if (Source == "Comboing")
                {
                    if (Ccombo != null)
                        return Ccombo;
                    else
                        return BestCombo;
                }
                else
                {
                    if (Cccombo != null)
                        return Cccombo;
                    else
                        return BestCombo;
                }
            }
            else
            {
                return BestCombo;
            }
        }

        //Jungle Farm
        public static void JungleFarm()
        {
            if (menu.Item("UseAAJungle").GetValue<bool>())
            {
                var AAminion = MinionManager.GetMinions(525, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (AAminion != null)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, AAminion);
                }
            }

            if (menu.Item("UseEJungle").GetValue<bool>() && E.IsReady())
            {
                var minion = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                if (minion != null)
                    castE(minion);
            }

            if (menu.Item("UseQJungle").GetValue<bool>() && Q.IsReady())
            {
                var targetClear = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();
                var targetFarm = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < (Player.GetSpellDamage(m, SpellSlot.Q)));

                if (targetFarm != null)
                {
                    Q.Cast(targetFarm, Packets());
                }
                else if (targetClear != null)
                {
                    Q.Cast(targetClear, Packets());
                }
            }

            if (menu.Item("UseWJungle").GetValue<bool>() && W.IsReady())
            {
                var JungleWMinions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                List<Vector2> minionerinos2 =
         (from minions in JungleWMinions select minions.Position.To2D()).ToList();
                var ePos2 = MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).Position;

                if (ePos2.Distance(Player.Position.To2D()) < W.Range && MinionManager.GetBestCircularFarmLocation(minionerinos2, W.Width, W.Range).MinionsHit > 0)
                {
                    W.Cast(ePos2, Packets());
                }
            }
        }

        //Q last Hitting
        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;
            if (menu.Item("SaveE").GetValue<bool>() && !HasMana(false, false, true, false) && Exists(false, false, true, false, false, false)) return;
            if (Q.IsReady())
            {
                if (_m != null)
                    Q.Cast(_m, Packets());
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

        //        //Calculates the damage from selected abilities
        private static float GetComboDamage(Obj_AI_Base enemy, string source, bool A, bool B, bool C, bool D, bool EE, bool F)
        {
            double damage = 0d;

            if (Q.IsReady() && A)
            {
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                    else if (E.IsReady())
                        damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                }
                else
                {
                    damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                }
            }

            if (R.IsReady() && D)
            {
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                    else if (E.IsReady())
                        damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                }
                else
                {
                    damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                }
            }

            if (W.IsReady() && B)
            {
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                    else if (E.IsReady())
                        damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                }
                else
                {
                    damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                }
            }
            if (Dfg.IsReady() && EE)
            {
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        damage = (damage * 1.2) + Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
                    else if (E.IsReady())
                        damage = (damage * 1.2) + Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
                }
                else
                {
                    damage = (damage * 1.2) + Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
                }
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && F)
            {
                if (source == "Comboing" || source == "KS" || source == "Table")
                {
                    if (!C)
                        damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                    else if (E.IsReady())
                        damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                }
                else
                {
                    damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
                }
            }
            //}

            //if (Items.HasItem(3155, (Obj_AI_Hero)enemy))
            //{
            //    damage = damage - 250;
            //}

            //if (Items.HasItem(3156, (Obj_AI_Hero)enemy))
            //{
            //    damage = damage - 400;
            //}
            return (float)damage - 20;
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
        public static Tuple<int, int> manaCheck()
        {
            int QMana = qMana[Q.Level];
            int EWQMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level];
            int EWQRMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level];
            int end = 0;
            int EndValue = 0;

            if (Player.Mana < QMana)
            {
                if (Q.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] - Player.Mana;
                    EndValue = (int)Math.Round(NeededMana / ManaRegen);
                    end = 1;
                }
            }

            else if (Player.Mana < EWQMana)
            {
                if (Q.Level != 0 && W.Level != 0 && E.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] - Player.Mana;
                    EndValue = (int)Math.Round(NeededMana / ManaRegen);
                    end = 2;
                }
            }

            else if (Player.Mana < EWQRMana)
            {
                if (Q.Level != 0 && W.Level != 0 && E.Level != 0 && R.Level != 0)
                {
                    double ManaRegen = ObjectManager.Player.PARRegenRate;
                    double NeededMana = qMana[Q.Level] + wMana[W.Level] + eMana[E.Level] + rMana[R.Level] - Player.Mana;
                    EndValue = (int)Math.Round(NeededMana / ManaRegen);
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

        //Returns how much time will you spend on casting spells
        public static float CastTime(Obj_AI_Base enemy, bool A, bool B, bool C, bool D, bool EEE, bool F)
        {
            float time = 0f;
            if (A) time += 0.6f * 1000;
            if (B) time += 1.2f * 1000;
            if (C) time += 0.2f * 1000;
            if (D) time += 0.6f * 1000;
            if (F) time += 1f * 1000;
            return time;
        }

        //Returns how much time will you spend on casting spells
        public static float CastTime(string combo)
        {
            float time = 0f;
            if (combo.Contains("Q")) time += 0.6f * 1000;
            if (combo.Contains("W")) time += 1.2f * 1000;
            if (combo.Contains("E")) time += 0.2f * 1000;
            if (combo.Contains("R")) time += 0.6f * 1000;
            if (combo.Contains("IGN")) time += 3f * 1000;
            return time;
        }

        //Returns how much more damage you need to deal to the target to ensure the kill OR how much you will overdamage to them
        private static Tuple<int, string> GetExtraNeeded(Obj_AI_Hero target)
        {
            var combodamage = GetComboDamage(target, "Cunts", true, true, true, true, true, true);
            float Damage = 0f;
            int OutPut = 0;
            string Text = null;

            if (combodamage > target.Health)
            {
                Damage = (float)Math.Round(combodamage - target.Health);
                OutPut = (int)Damage;
                Text = "Extra:";
            }
            else
            {
                Damage = (float)Math.Round(target.Health - combodamage);
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
                    if (SharpDX.Vector2.Distance(Game.CursorPos.To2D(), enemy.ServerPosition.To2D()) < 200 && !KeMinimap.Minimap.MouseOver)
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

        //Checks if the target will be affected by spell
        public static bool IsImmune(Obj_AI_Hero target)
        {
            foreach (var buff in IgnoreList)
            {
                if (target.HasBuff(buff.DisplayName) || target.HasBuff(buff.Name)) return true;
            }
            return false;
        }

        //Checks if the target will be killed by spell
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

        //E CAST(UNIT)
        public static void castE(Obj_AI_Base target)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;

            if (pred.Hitchance >= HitChance.High && E.IsReady())
            {
                E.Cast(castVec);
            }
        }

        //E CAST(UNIT)
        public static void castE(Obj_AI_Hero target)
        {
            PredictionOutput pred = Prediction.GetPrediction(target, E.Delay);
            Vector2 castVec = pred.UnitPosition.To2D() -
                              Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;

            if (pred.Hitchance >= HitChance.High && E.IsReady())
            {
                E.Cast(castVec, Packets());
            }
        }

        //E CAST(COORDINATES)
        public static void castE(Vector3 pos)
        {
            Vector2 castVec = pos.To2D() -
                              Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width;

            if (E.IsReady())
            {
                E.Cast(castVec, Packets());
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
                Orb = 1;
                Game.PrintChat("Regular Orbwalker Loaded");
            }
            else
            {
                xSLxOrbwalker.AddToMenu(orbwalkerMenu);
                Orb = 2;
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