using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Color = System.Drawing.Color;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;

namespace Cassiopeia
{
    class Program
    {
        public static AIHeroClient Player = ObjectManager.Player;
        public static Menu Root;
        public static Menu Combo, Harass, LaneClear, LastHit, Misc, Drawings;
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static List<Spell.Skillshot> Spells = new List<Spell.Skillshot>();
        public static float LastQ = 0;
        public static float LastE = 0;
        public static Item TotG = new Item(3070, 0);


        static void Main(string[] args)
        {
            Bootstrap.Init(null);
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.ChampionName.ToLower() != "cassiopeia")
                return;

            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, 750, int.MaxValue, 150); Spells.Add(Q);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, 250, 2500, 250); Spells.Add(W);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Skillshot(SpellSlot.R, 825, SkillShotType.Cone, (int)0.6f, int.MaxValue, (int)(80*Math.PI/180)); Spells.Add(R);

            Root = MainMenu.AddMenu("Cassiopeia", "cassroot");
            Root.AddLabel("Made by Thorn @ EloBuddy!");
            Root.AddLabel("Report bugs or suggestions on the thread!");
            Combo = Root.AddSubMenu("Combo", "combo");
            Combo.AddLabel("Spell Usage");
            Combo.Add("combo.q", new CheckBox("Use Q", true));
            Combo.Add("combo.w", new CheckBox("Use W", true));
            Combo.Add("combo.e", new CheckBox("Use E", true));
            Combo.Add("combo.r", new CheckBox("Use R", true));
            Combo.AddLabel("Ultimate Settings");
            Combo.Add("combo.r.minfacing", new Slider("Minimum Facing", 1, 1, 5));
            Combo.Add("combo.r.minhit", new Slider("Minimum Hit", 1, 1, 5));
            Combo.Add("combo.r.smart", new CheckBox("Use Smart Mode", true));
            Harass = Root.AddSubMenu("Harass", "harass");
            Harass.AddLabel("Spell Usage");
            Harass.Add("harass.q", new CheckBox("Use Q", true));
            Harass.Add("harass.w", new CheckBox("Use W", true));
            Harass.Add("harass.e", new CheckBox("Use E", true));
            Harass.AddLabel("Extra Settings");
            Harass.Add("harass.e.restriction", new CheckBox("E only if enemy is poisoned", true));
            LastHit = Root.AddSubMenu("Last Hit", "lh");
            LastHit.AddLabel("Last Hitting Options");
            LastHit.Add("lasthit.e", new CheckBox("Use E", true));
            LastHit.Add("lasthit.e.auto", new CheckBox("E Automatically", true));
            /*LaneClear = Root.AddSubMenu("Lane Clear", "lc");
            LaneClear.AddLabel("Spell Usage");
            LaneClear.Add("laneclear.q", new CheckBox("Use Q", true));
            LaneClear.Add("laneclear.w", new CheckBox("Use W", true));
            LaneClear.Add("laneclear.e", new CheckBox("Use E", true));
            LaneClear.AddLabel("Extra Options");
            LaneClear.Add("laneclear.e.restriction", new CheckBox("E only if target poisoned", true));
            LaneClear.Add("laneclear.e.lasthit", new CheckBox("E only if target can be last hit", true));
            LaneClear.Add("laneclear.w.restriction", new Slider("W if it hits", 3, 1, 10));*/
            Misc = Root.AddSubMenu("Misc", "misc");
            Misc.AddLabel("Mana Managing");
            Misc.Add("misc.mm", new CheckBox("Restrict mana usage", true));
            Misc.Add("misc.mm.slider", new Slider("Minimum mana", 35, 0, 95));
            //Misc.AddLabel("Item Stacking");
            //Misc.Add("misc.itemstack", new CheckBox("Stack item", true));
            Misc.AddLabel("Kill Stealing");
            Misc.Add("misc.qks", new CheckBox("KS with Q", true));
            Misc.Add("misc.wks", new CheckBox("KS with W", true));
            Misc.Add("misc.eks", new CheckBox("KS with E", true));
            Misc.AddLabel("Delay");
            Misc.Add("misc.edelay", new Slider("E Delay", 0, 0, 500));
            Misc.AddLabel("Gapcloser");
            Misc.Add("misc.gc", new CheckBox("W on gapcloser", true));
            Misc.Add("misc.gc.hp", new Slider("if HP <", 30, 0, 100));
            Misc.AddLabel("AA");
            Misc.Add("misc.aablock", new CheckBox("Auto Attack blocking in combo", false));
            Drawings = Root.AddSubMenu("Drawings", "drawings");
            Drawings.Add("draw", new CheckBox("Use Drawings", true));
            Drawings.AddLabel("Spells");
            Drawings.Add("draw.q", new CheckBox("Draw Q Range", true));
            Drawings.Add("draw.w", new CheckBox("Draw W Range", true));
            Drawings.Add("draw.e", new CheckBox("Draw E Range", true));
            Drawings.Add("draw.r", new CheckBox("Draw R Range", true));
            Drawings.AddLabel("Others");
            Drawings.Add("draw.tg", new CheckBox("Draw Target", true));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapCloser += Gapcloser_OnGapCloser;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;


        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q)
                LastQ = Environment.TickCount;
            else if (args.Slot == SpellSlot.E)
                LastE = Environment.TickCount;
        }

        private static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapCloserEventArgs e)
        {
            if (Misc["misc.gc"].Cast<CheckBox>().CurrentValue && W.IsReady() && Player.HealthPercent <= Misc["misc.gc.hp"].Cast<Slider>().CurrentValue && sender.IsValidTarget())
                W.Cast(e.End);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Drawings["draw"].Cast<CheckBox>().CurrentValue)
                return;
            var target = TargetSelector.GetTarget(850f, DamageType.Magical);
            if (Drawings["draw.tg"].Cast<CheckBox>().CurrentValue && target.IsValidTarget())
                new Circle { Color = Color.DarkRed, Radius = 75 }.Draw(target.Position);
            foreach (var x in Spells.Where(x => Drawings["draw." + x.Slot.ToString().ToLower()].Cast<CheckBox>().CurrentValue))
            {
                new Circle { Color = x.IsReady() ? Color.Green : Color.Red, Radius = x.Range }.Draw(Player.Position);
            }
            if (Drawings["draw.e"].Cast<CheckBox>().CurrentValue)
                new Circle { Color = E.IsReady() ? Color.Green : Color.Red, Radius = E.Range }.Draw(Player.Position);
        }

        public static void AutoE()
        {
            if (Misc["misc.mm"].Cast<CheckBox>().CurrentValue && Misc["misc.mm.slider"].Cast<Slider>().CurrentValue > Player.ManaPercent)
                return;

            if (!LastHit["lasthit.e.auto"].Cast<CheckBox>().CurrentValue || !E.IsReady())
                return;

            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(x => x.IsEnemy && !x.IsDead && E.IsInRange(x) && GetDamage(SpellSlot.E, x) > x.Health + 5 && x.IsValidTarget()))
            {
                E.Cast(unit);
            }
        }

        public static float GetDamage(SpellSlot spell, Obj_AI_Base target)
        {
            float ap = Player.FlatMagicDamageMod + Player.BaseAbilityDamage;
            if (spell == SpellSlot.Q)
            {
                if (!Q.IsReady())
                    return 0;
                return Player.CalculateDamageOnUnit(target, DamageType.Magical, 75f + 40f * (Q.Level - 1) + 45 / 100 * ap);
            }
            else if (spell == SpellSlot.W)
            {
                if (!W.IsReady())
                    return 0;
                return Player.CalculateDamageOnUnit(target, DamageType.Magical, 90f + 45f * (W.Level - 1) + 90 / 100 * ap);
            }
            else if (spell == SpellSlot.E)
            {
                if (!E.IsReady())
                    return 0;
                return Player.CalculateDamageOnUnit(target, DamageType.Magical, 55f + 25f * (E.Level - 1) + 55 / 100 * ap);
            }
            else if (spell == SpellSlot.R)
            {
                if (!R.IsReady())
                    return 0;
                return Player.CalculateDamageOnUnit(target, DamageType.Magical, 150f + 100f * (R.Level - 1) + 50 / 100 * ap);
            }

            return 0;
        }

        public static float GetManaCost(SpellSlot spell)
        {
            switch (spell)
            {
                case SpellSlot.Q:
                    return 40f + 10f * (Q.Level - 1);
                case SpellSlot.W:
                    return 40f + 10f * (W.Level - 1);
                case SpellSlot.E:
                    return 50f + 10f * (E.Level - 1);
                case SpellSlot.R:
                    return 100f;
            }
            return 0f;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Orbwalker.DisableAttacking = false;
            KS();
            var target = TargetSelector.GetTarget(900f, DamageType.Magical);
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    if (Player.Distance(target.Position) > 440)
                        Orbwalker.DisableAttacking = true;
                    else
                        Orbwalker.DisableAttacking = false;
                    Orbwalker.DisableAttacking = false;
                    DoCombo();
                    AABlock();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    DoHarass();
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    DoLastHit();
                    break;
            }
        }

        public static void DoCombo()
        {
            var target = TargetSelector.GetTarget(900f, DamageType.Magical);

            if (!target.IsValidTarget())
                return;
            if (Combo["combo.q"].Cast<CheckBox>().CurrentValue)
            {
                if ((target.Health + 50 < GetDamage(SpellSlot.Q, target) && Q.IsReady())
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && E.IsInRange(target) && E.IsReady() && Q.IsReady() &&
                     Player.Mana >= GetManaCost(SpellSlot.Q) + 2 * GetManaCost(SpellSlot.E))
                    || (!target.HasBuffOfType(BuffType.Poison) && E.Level < 1 && Q.IsReady() && Q.IsInRange(target))
                    ||
                    (Q.IsReady() && E.IsReady() && E.IsInRange(target) &&
                     target.Health + 25 < GetDamage(SpellSlot.Q, target) + GetDamage(SpellSlot.E, target) &&
                     Player.Mana >= GetManaCost(SpellSlot.Q) + GetManaCost(SpellSlot.E)))
                    Q.Cast(target);
            }
            if (Combo["combo.w"].Cast<CheckBox>().CurrentValue)
            {
                if ((Player.HealthPercent <= 25 && !Player.IsFacing(target) && target.IsFacing(Player) && target.IsValidTarget((W.Range / 3) * 2) && W.IsReady() && target.MoveSpeed >= Player.MoveSpeed)
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && Q.CastDelay * 1000 + LastQ < Environment.TickCount && !Q.IsReady() && W.IsReady() && E.IsReady() && E.IsInRange(target) && Player.Mana >= GetManaCost(SpellSlot.W) + 2 * GetManaCost(SpellSlot.E))
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && Q.CastDelay * 1000 + LastQ < Environment.TickCount && !Q.IsReady() && W.IsReady() && E.IsReady() && E.IsInRange(target) && Player.Mana >= GetManaCost(SpellSlot.W) + GetManaCost(SpellSlot.E) && GetDamage(SpellSlot.W, target) + GetDamage(SpellSlot.E, target) > target.Health + 25)
                    ||
                    (!target.HasBuffOfType(BuffType.Poison) && Q.CastDelay * 1000 + LastQ < Environment.TickCount && (!Q.IsReady() || GetDamage(SpellSlot.Q, target) < target.Health + 25) && W.IsReady() && W.IsInRange(target) && GetDamage(SpellSlot.W, target) > target.Health + 25))
                    W.Cast(target);
            }
            if (Combo["combo.e"].Cast<CheckBox>().CurrentValue)
            {
                if ((target.HasBuffOfType(BuffType.Poison) && E.IsReady() && target.IsValidTarget(E.Range) &&
                     Environment.TickCount > LastE + Misc["misc.edelay"].Cast<Slider>().CurrentValue)
                    || (E.IsReady() && target.IsValidTarget(E.Range) && target.Health + 25 < GetDamage(SpellSlot.E, target)))
                    E.Cast(target);
            }

            EasyRLogic();
            SmartR();

        }

        public static void DoLastHit()
        {
            if (Misc["misc.mm"].Cast<CheckBox>().CurrentValue && Misc["misc.mm.slider"].Cast<Slider>().CurrentValue > Player.ManaPercent)
                return;

            if (!LastHit["lasthit.e"].Cast<CheckBox>().CurrentValue || !E.IsReady())
                return;

            foreach (var min in ObjectManager.Get<Obj_AI_Minion>().Where(x =>
                E.IsInRange(x)
                && !x.IsDead
                && x.IsEnemy
                && x.Health + 5 < GetDamage(SpellSlot.E, x)))
            {
                E.Cast(min);
            }
        }

        private static void SmartR()
        {
            var srSpell = Combo["combo.r.smart"].Cast<CheckBox>().CurrentValue;
            var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);

            if (Player.CountEnemiesInRange(1200) == 1)

                return;

            if (srSpell)
            {
                if (rTarget.IsValidTarget(500))
                {
                    if (rTarget.IsFacing(Player))
                    {
                        if (Player.HealthPercent + 25 <= rTarget.HealthPercent
                            && R.IsReady())
                        {
                            R.Cast(rTarget);
                        }

                        if (rTarget.HasBuffOfType(BuffType.Poison)
                            && E.IsReady() && R.IsReady()
                            && Player.Mana >= (2 * GetManaCost(SpellSlot.E) + GetManaCost(SpellSlot.R))
                            && rTarget.Health < (GetDamage(SpellSlot.E, rTarget) * 4 + GetDamage(SpellSlot.R, rTarget)))
                        {
                            R.Cast(rTarget);
                        }
                        if (!rTarget.HasBuffOfType(BuffType.Poison)
                            && Q.IsReady() && E.IsReady() && R.IsReady()
                            && rTarget.Health < (GetDamage(SpellSlot.Q, rTarget) + GetDamage(SpellSlot.E, rTarget) * 4 + GetDamage(SpellSlot.R, rTarget))
                            && Player.Mana >= (GetManaCost(SpellSlot.Q) + 2 * GetManaCost(SpellSlot.E) + GetManaCost(SpellSlot.R)))
                        {
                            R.Cast(rTarget);
                        }
                    }
                }
            }
        }

        private static void EasyRLogic()
        {

            var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var rSpell = Combo["combo.r"].Cast<CheckBox>().CurrentValue;
            var rminhitSpell = Combo["combo.r.minhit"].Cast<Slider>().CurrentValue;
            var rfaceSpell = Combo["combo.r.minfacing"].Cast<Slider>().CurrentValue;

            List<AIHeroClient> targets = HeroManager.Enemies.Where(o => R.GetPrediction(o).HitChance == HitChance.High
                                                                       && o.Distance(Player.Position) < 600).ToList();

            var facing =
                HeroManager.Enemies.Where(
                    x => R.GetPrediction(x).HitChance == HitChance.High
                         && x.IsFacing(Player)
                         && !x.IsDead
                         && x.IsValidTarget(600));

            if (rSpell)
            {
                if ((targets.Count() >= rminhitSpell
                    || facing.Count() >= rfaceSpell)
                    && R.IsReady()
                    && rTarget.Health >= (GetDamage(SpellSlot.Q, rTarget) + 2 * GetDamage(SpellSlot.E, rTarget) + GetDamage(SpellSlot.R, rTarget)))
                {
                    R.Cast(rTarget);
                }
            }
        }

        public static void DoHarass()
        {
            if (Misc["misc.mm"].Cast<CheckBox>().CurrentValue && Misc["misc.mm.slider"].Cast<Slider>().CurrentValue > Player.ManaPercent)
                return;
            var target = TargetSelector.GetTarget(850f, DamageType.Magical);
            if (Harass["harass.q"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Q.IsInRange(target))
                Q.Cast(target);
            if (Harass["harass.w"].Cast<CheckBox>().CurrentValue && W.IsReady() && W.IsInRange(target))
                W.Cast(target);
            if (Harass["harass.e"].Cast<CheckBox>().CurrentValue && E.IsReady() && E.IsInRange(target) && Environment.TickCount > LastE + Misc["misc.edelay"].Cast<Slider>().CurrentValue
                && ((target.HasBuffOfType(BuffType.Poison) && Harass["harass.e.restriction"].Cast<CheckBox>().CurrentValue)
                || (!target.HasBuffOfType(BuffType.Poison) && !Harass["harass.e.restriction"].Cast<CheckBox>().CurrentValue)))
                E.Cast(target);
        }

        public static void KS()
        {
            if (E.IsReady() && Misc["misc.eks"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var x in HeroManager.Enemies.Where(x => !x.IsDead
                    && x.IsValidTarget(E.Range)
                    && x.Health + 10 < GetDamage(SpellSlot.E, x)))
                {
                    E.Cast(x);
                }
            }

            if (Q.IsReady() && Misc["misc.qks"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var x in HeroManager.Enemies.Where(x => !x.IsDead
                    && x.IsValidTarget(Q.Range)
                    && x.Health + 25 < GetDamage(SpellSlot.Q, x)))
                {
                    Q.Cast(x);
                }
            }

            if (W.IsReady() && Misc["misc.wks"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var x in HeroManager.Enemies.Where(x => !x.IsDead
                    && x.IsValidTarget(W.Range)
                    && x.Health + 25 < GetDamage(SpellSlot.W, x)))
                {
                    if ((!Q.IsReady() && !x.HasBuffOfType(BuffType.Poison) && Q.CastDelay * 1000 + LastQ < Environment.TickCount)
                        || (Q.IsReady() && GetDamage(SpellSlot.Q, x) < x.Health))
                        W.Cast(x);
                }
            }

        }

        private static void AABlock()
        {
            var aaBlock = Misc["misc.aablock"].Cast<CheckBox>().CurrentValue;
            if (aaBlock)
            {
                Orbwalker.DisableAttacking = true;
            }
        }

    }
}
