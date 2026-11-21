using System;
using System.Collections.Generic;
using System.Linq;
using Aotenjo;

namespace ModenjoASM.SevenYaku
{
    /// <summary>
    /// Static (final-hand) 7nnkmj yaku. Situational yaku that need wall/call
    /// context are kept out of this first batch until their host events exist.
    /// </summary>
    public static class SevenYakuRegistry
    {
        private const string Prefix = "7nnkmj:";
        private static bool _registered;
        private static readonly List<YakuEntry> _entries = new List<YakuEntry>();

        private sealed class YakuEntry
        {
            public YakuEntry(YakuType type, int fu, Func<Permutation, Player, bool> predicate,
                bool isHigh = false, int? fixedPoints = null, bool doubleYakuman = false, int pointsBonus = 0)
            {
                Type = type;
                Fu = fu;
                Predicate = predicate;
                IsHigh = isHigh;
                FixedPoints = fixedPoints;
                DoubleYakuman = doubleYakuman;
                PointsBonus = pointsBonus;
            }

            public YakuType Type { get; }
            public int Fu { get; }
            public Func<Permutation, Player, bool> Predicate { get; }
            public bool IsHigh { get; }
            public int? FixedPoints { get; }
            public bool DoubleYakuman { get; }
            public int PointsBonus { get; }
        }

        public sealed class ScoreResult
        {
            public int Points { get; set; }
            public int Fu { get; set; }
            public bool IsHigh { get; set; }
            public bool DoubleYakuman { get; set; }
        }

        public static void RegisterAll()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;

            Add("four_pairs_seven", 7, FourPairsSeven, "11223344556677m", fixedPoints: 7000);
            Add("missing_seven", 1, MissingSeven, "11223344556688m");
            Add("all_number", 1, AllNumber, "123456789m123p45s");
            Add("all_chi", 1, AllChi, "123m456m789p123s11z");
            Add("all_pon", 2, AllPon, "111m222p333s444z11z");
            Add("one_dragon", 4, OneDragon, "123456789m111p22s");
            Add("same_color_old_young", 1, SameColorOldYoung, "123789m111p22s");
            Add("four_same", 2, FourSame, "1111m222p333s44z");
            Add("five_gates", 5, FiveGates, "123m456p789s11z11z");
            Add("missing_suit", 3, MissingSuit, "123m456m789m11p11p");
            Add("two_color_straight", 3, TwoColorStraight, "123m123p456s789s11z");
            Add("three_color_straight", 4, ThreeColorStraight, "123m123p123s456m11z");
            Add("two_color_ko", 4, TwoColorKo, "111m111p234s567s11z");
            Add("three_color_ko", 5, ThreeColorKo, "111m111p111s234m11z");
            Add("one_color_step", 4, OneColorStep, "123m234m345m678p11z");
            Add("one_color_triple", 3, OneColorTriple, "111m222m333m456p11z");
            Add("same_high", 4, SameHigh, "123m123p456s789s11z");
            Add("double_same_high", 7, DoubleSameHigh, "123m123p123s789m11z");
            Add("small_three_dragons", 8, SmallThreeDragons, "555z666z777z11m22m");
            AddHigh("pure_moon_in_water", 70, PureMoonInWater, "11223344556677p", doubleYakuman: true);
            AddHigh("three_seven_flower", 70, ThreeSevenFlower, "777m777p777s111z55z", doubleYakuman: true);
            AddHigh("beauty_seven_pairs", 70, BeautySevenPairs, "111z222z555z666z777z11p", doubleYakuman: true);
            AddHigh("big_three_dragons", 28, BigThreeDragons, "555z666z777z111m22m");
            AddHigh("four_kings", 28, FourKings, "111z222z333z444z11m", pointsBonus: 7000);
            AddHigh("four_ankan", 70, FourAnkan, "1111m2222p3333s4444z11z", doubleYakuman: true);
            AddHigh("four_kan", 28, FourKan, "1111m2222p3333s4444z11z");
            AddHigh("all_honors", 28, AllHonors, "111z222z333z444z55z");
            AddHigh("all_terminals", 28, AllTerminals, "111m999m111p999p11s");
            AddHigh("all_green", 28, AllGreen, "222s333s444s666s88s");
            AddHigh("thirteen_orphans", 28, ThirteenOrphans, "19m19p19s1234567z1m");
            AddHigh("nine_gates", 28, NineGates, "1112345678999m11m");
            Add("pure_fully_terminal", 7, PureFullyTerminal, "123m789m111p999p11s");
            Add("mixed_fully_terminal", 5, MixedFullyTerminal, "123m789p111s555z11z");
            Add("mixed_terminals", 5, MixedTerminals, "111m999p111s555z11z");
            Add("three_kingdoms", 14, ThreeKingdoms, "222m555p888s111z22z", fixedPoints: 7000);
            Add("less_than_seven", 14, LessThanSeven, "123m55p55s111z222z");
            Add("greater_than_seven", 14, GreaterThanSeven, "789m55p55s111z222z");
            Add("north_south_pass", 14, NorthSouthPass, "123456789m22z44z11p");
            Add("flower_in_mirror", 14, FlowerInMirror, "11223344556677p");
            Add("moon_in_water", 14, MoonInWater, "11223344556677p");
            Add("chidaichi", 14, Chidaichi, "123456789m11p33p55p");
            Add("all_simple", 2, AllSimple, "234m345p456s678m22p");
            Add("all_pungs", 3, AllPungs, "111m222p333s444z11z");
            Add("half_flush", 5, HalfFlush, "123m456m789m111z22z");
            Add("fully_flush", 14, FullyFlush, "123m456m789m111m22m");
            Add("two_ankou", 3, TwoAnkou, "111m222p345s678s99m");
            Add("three_ankou", 5, ThreeAnkou, "111m222p333s456m99m");
            Add("two_kan", 3, TwoKan, "1111m2222p345s678s99m");
            Add("three_kan", 5, ThreeKan, "1111m2222p3333s456m99m");
            Add("same_color_double_old_young", 2, SameColorDoubleOldYoung, "123789m123789p11s");
            Add("two_color_double_old_young", 3, TwoColorDoubleOldYoung, "123789m123789p11s");
            Add("all_number_jin", 1, AllNumberJin, "123m456p789s234m55s");
            Add("seven_pairs", 7, SevenPairs, "11223344556677m");

            UnityEngine.Debug.Log($"[7nnkmj] registered {_entries.Count} custom yaku");
        }

        public static void GrantAll(Player player)
        {
            if (player == null || _entries.Count == 0)
            {
                return;
            }

            var skills = player.GetSkillSet();
            foreach (var entry in _entries)
            {
                if (skills.GetLevel(entry.Type) <= 0)
                {
                    skills.SetLevel(entry.Type, 1);
                }
            }
        }

        /// <summary>
        /// Original 7nnkmj fu for the current permutation. The artifact uses this
        /// to replace Aotenjo's Fan x Fu result with the 7nnkmj point table.
        /// </summary>
        public static ScoreResult Evaluate(Permutation perm, Player player)
        {
            var result = new ScoreResult();
            if (perm == null || player == null)
            {
                return result;
            }

            // High yaku replace the normal table entirely (first match wins).
            foreach (var entry in _entries)
            {
                if (!entry.IsHigh || !Matches(entry, perm, player))
                {
                    continue;
                }

                result.IsHigh = true;
                result.DoubleYakuman = entry.DoubleYakuman;
                result.Fu = entry.Fu;
                result.Points = entry.DoubleYakuman ? 70000 : entry.Fu >= 28 ? 35000 : CalcPoints(entry.Fu);
                result.Points += entry.PointsBonus;
                return result;
            }

            int fu = 0;
            int bonus = 0;
            int? fixedPoints = null;
            foreach (var entry in _entries)
            {
                if (entry.IsHigh || !Matches(entry, perm, player))
                {
                    continue;
                }

                fu += entry.Fu;
                bonus += entry.PointsBonus;
                if (entry.FixedPoints.HasValue && !fixedPoints.HasValue)
                {
                    fixedPoints = entry.FixedPoints;
                }
            }

            result.Fu = Math.Max(7, fu);
            result.Points = fixedPoints ?? CalcPoints(result.Fu);
            result.Points += bonus;
            return result;
        }

        public static int ScoreFu(Permutation perm, Player player) => Evaluate(perm, player).Fu;

        private static bool Matches(YakuEntry entry, Permutation perm, Player player)
        {
            try
            {
                return entry.Predicate(perm, player);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ike.json points table: thresholds 7/14/21/28, baselines
        /// 0/7000/14000/28000/35000, 700 per extra fu.
        /// </summary>
        public static int CalcPoints(int fu)
        {
            if (fu >= 28)
            {
                return 35000;
            }

            int[] thresholds = { 7, 14, 21, 28 };
            int[] baselines = { 0, 7000, 14000, 28000, 35000 };
            int baseline = baselines[0];
            int threshold = 0;
            for (int i = thresholds.Length - 1; i >= 0; i--)
            {
                if (fu >= thresholds[i])
                {
                    baseline = baselines[i + 1];
                    threshold = thresholds[i];
                    break;
                }
            }

            return baseline + (fu - threshold) * 700;
        }

        private static void Add(string id, int fu, Func<Permutation, Player, bool> predicate,
            string example, int? fixedPoints = null)
        {
            AddInternal(id, fu, predicate, example, isHigh: false, fixedPoints, doubleYakuman: false, pointsBonus: 0);
        }

        private static void AddHigh(string id, int fu, Func<Permutation, Player, bool> predicate,
            string example, bool doubleYakuman = false, int pointsBonus = 0)
        {
            AddInternal(id, fu, predicate, example, isHigh: true, fixedPoints: null, doubleYakuman, pointsBonus);
        }

        private static void AddInternal(string id, int fu, Func<Permutation, Player, bool> predicate,
            string example, bool isHigh, int? fixedPoints, bool doubleYakuman, int pointsBonus)
        {
            var yaku = CustomYakuBuilder.RegisterCustomYaku(
                Prefix + id,
                fu,
                1.0,
                0.0,
                predicate,
                Array.Empty<string>(),
                new[] { "standard", "galaxy" },
                new[] { 0, 2, 6 },
                Rarity.COMMON,
                example);
            _entries.Add(new YakuEntry(yaku, fu, predicate, isHigh, fixedPoints, doubleYakuman, pointsBonus));
        }

        private static bool SevenPairs(Permutation perm, Player player) =>
            perm.GetPermType() == PermutationType.SEVEN_PAIRS;

        private static bool FourPairsSeven(Permutation perm, Player player)
        {
            if (perm.GetPermType() != PermutationType.SEVEN_PAIRS)
            {
                return false;
            }

            return CountByTile(perm).Any(v => v >= 4);
        }

        private static bool MissingSeven(Permutation perm, Player player)
        {
            var tiles = perm.ToTiles();
            bool hasNumber = false;
            foreach (var tile in tiles)
            {
                if (!IsNumbered(tile))
                {
                    continue;
                }

                hasNumber = true;
                if (tile.GetOrder() == 7)
                {
                    return false;
                }
            }

            return hasNumber;
        }

        private static bool AllNumber(Permutation perm, Player player) =>
            perm.ToTiles().All(t => !IsHonor(t));

        private static bool AllChi(Permutation perm, Player player) =>
            perm.blocks.Length > 0 && perm.blocks.All(b => b.IsABC());

        private static bool AllPon(Permutation perm, Player player) =>
            perm.blocks.Length > 0 && perm.blocks.All(b => b.IsAAA() || b.IsAAAA());

        private static bool OneDragon(Permutation perm, Player player) =>
            HasRuns(perm, 1, 4, 7) || HasRuns(perm, 1, 4, 7, sameSuit: true);

        private static bool SameColorOldYoung(Permutation perm, Player player)
        {
            var runs = Runs(perm);
            return runs.Any(a => a.Order == 1 && runs.Any(b => b.Order == 7 && b.Category == a.Category));
        }

        private static bool FourSame(Permutation perm, Player player) =>
            CountByTile(perm).Any(v => v >= 4);

        private static bool FiveGates(Permutation perm, Player player)
        {
            var cats = new HashSet<Tile.Category>(perm.ToTiles().Select(t => t.GetCategory()));
            return cats.Contains(Tile.Category.Wan)
                && cats.Contains(Tile.Category.Suo)
                && cats.Contains(Tile.Category.Bing)
                && cats.Contains(Tile.Category.Feng)
                && cats.Contains(Tile.Category.Jian);
        }

        private static bool MissingSuit(Permutation perm, Player player)
        {
            var cats = new HashSet<Tile.Category>(perm.ToTiles().Select(t => t.GetCategory()));
            int suits = (cats.Contains(Tile.Category.Wan) ? 1 : 0)
                + (cats.Contains(Tile.Category.Suo) ? 1 : 0)
                + (cats.Contains(Tile.Category.Bing) ? 1 : 0);
            return suits == 2;
        }

        private static bool TwoColorStraight(Permutation perm, Player player)
        {
            var runs = Runs(perm);
            return runs.GroupBy(r => r.Order).Any(g => g.Select(r => r.Category).Distinct().Count() >= 2);
        }

        private static bool ThreeColorStraight(Permutation perm, Player player)
        {
            var runs = Runs(perm);
            return runs.GroupBy(r => r.Order).Any(g => g.Select(r => r.Category).Distinct().Count() >= 3);
        }

        private static bool TwoColorKo(Permutation perm, Player player)
        {
            var kos = Kos(perm);
            return kos.GroupBy(k => k.Order).Any(g => g.Select(k => k.Category).Distinct().Count() >= 2);
        }

        private static bool ThreeColorKo(Permutation perm, Player player)
        {
            var kos = Kos(perm);
            return kos.GroupBy(k => k.Order).Any(g => g.Select(k => k.Category).Distinct().Count() >= 3);
        }

        private static bool OneColorStep(Permutation perm, Player player)
        {
            var runs = Runs(perm).Where(r => !r.Category.Equals(Tile.Category.Feng)
                && !r.Category.Equals(Tile.Category.Jian)).ToList();
            for (int a = 0; a < runs.Count; a++)
            for (int b = a + 1; b < runs.Count; b++)
            for (int c = b + 1; c < runs.Count; c++)
            {
                if (runs[a].Category == runs[b].Category && runs[b].Category == runs[c].Category
                    && runs[b].Order - runs[a].Order == 1
                    && runs[c].Order - runs[b].Order == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool OneColorTriple(Permutation perm, Player player)
        {
            var kos = Kos(perm).Where(k => !IsHonor(k.Category)).ToList();
            for (int a = 0; a < kos.Count; a++)
            for (int b = a + 1; b < kos.Count; b++)
            for (int c = b + 1; c < kos.Count; c++)
            {
                if (kos[a].Category == kos[b].Category && kos[b].Category == kos[c].Category
                    && kos[b].Order - kos[a].Order == 1
                    && kos[c].Order - kos[b].Order == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SameHigh(Permutation perm, Player player)
        {
            var runs = Runs(perm);
            return runs.GroupBy(r => (r.Category, r.Order)).Any(g => g.Count() >= 2);
        }

        private static bool DoubleSameHigh(Permutation perm, Player player)
        {
            var runs = Runs(perm);
            var pairs = runs.GroupBy(r => (r.Category, r.Order)).Count(g => g.Count() >= 2);
            return pairs >= 2;
        }

        private static bool SmallThreeDragons(Permutation perm, Player player)
        {
            var dragons = new[] { 5, 6, 7 };
            var kos = Kos(perm).Where(k => IsDragon(k.Category)).Select(k => k.Order).ToHashSet();
            int tripletCount = dragons.Count(kos.Contains);
            return tripletCount == 2
                && perm.jiang != null
                && IsDragon(perm.jiang.tile1.GetCategory())
                && perm.jiang.tile1.GetOrder() == perm.jiang.tile2.GetOrder();
        }

        private static bool BigThreeDragons(Permutation perm, Player player)
        {
            var kos = Kos(perm).Where(k => IsDragon(k.Category)).Select(k => k.Order).ToHashSet();
            return kos.SetEquals(new[] { 5, 6, 7 });
        }

        private static bool FourKings(Permutation perm, Player player)
        {
            var kos = Kos(perm).Where(k => k.Category.Equals(Tile.Category.Feng)).Select(k => k.Order).ToHashSet();
            return kos.SetEquals(new[] { 1, 2, 3, 4 });
        }

        private static bool FourKan(Permutation perm, Player player) =>
            perm.blocks.Count(b => b.IsAAAA()) >= 4;

        private static bool AllHonors(Permutation perm, Player player) =>
            perm.ToTiles().All(IsHonor);

        private static bool AllTerminals(Permutation perm, Player player) =>
            perm.ToTiles().All(t => !IsHonor(t) && (t.GetOrder() == 1 || t.GetOrder() == 9));

        private static bool AllGreen(Permutation perm, Player player)
        {
            if (player == null)
            {
                return false;
            }

            var tiles = perm.ToTiles();
            return tiles.Count > 0 && tiles.All(t => t.FulfillAllGreen(player));
        }

        private static bool ThirteenOrphans(Permutation perm, Player player) =>
            perm.GetPermType() == PermutationType.THIRTEEN_ORPHANS
            || perm.GetPermType() == PermutationType.EXTENDED_THIRTEEN_ORPHANS;

        private static bool NineGates(Permutation perm, Player player)
        {
            var tiles = perm.ToTiles().Where(t => !IsHonor(t)).ToList();
            if (tiles.Count == 0 || tiles.Select(t => t.GetCategory()).Distinct().Count() != 1)
            {
                return false;
            }

            var counts = CountByTile(perm);
            if (counts.Length <= 26 || counts.Take(9).Count(v => v > 0) < 9)
            {
                return false;
            }

            for (int i = 0; i < 9; i++)
            {
                int need = i == 0 || i == 8 ? 3 : 1;
                if (counts[i] < need)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool PureFullyTerminal(Permutation perm, Player player)
        {
            var tiles = perm.ToTiles();
            return tiles.All(t => !IsHonor(t))
                && perm.blocks.Where(b => !b.IsAAAA()).All(b => b.Any(t => t.GetOrder() == 1 || t.GetOrder() == 9))
                && perm.jiang.tile1.GetOrder() is 1 or 9;
        }

        private static bool MixedFullyTerminal(Permutation perm, Player player) =>
            perm.blocks.Where(b => !b.IsAAAA()).All(b => b.Any(t => IsTerminalOrHonor(t)))
            && IsTerminalOrHonor(perm.jiang.tile1);

        private static bool MixedTerminals(Permutation perm, Player player) =>
            perm.ToTiles().All(t => IsHonor(t) || t.GetOrder() is 1 or 9);

        private static bool AllSimple(Permutation perm, Player player) =>
            perm.ToTiles().All(t => IsNumbered(t) && t.GetOrder() >= 2 && t.GetOrder() <= 8);

        private static bool AllPungs(Permutation perm, Player player) =>
            perm.blocks.Length > 0 && perm.blocks.All(b => b.IsAAA() || b.IsAAAA());

        private static bool HalfFlush(Permutation perm, Player player)
        {
            var cats = perm.ToTiles().Select(t => t.GetCategory()).Distinct().ToList();
            bool hasHonor = cats.Any(IsHonor);
            int suits = cats.Count(c => !IsHonor(c));
            return hasHonor && suits == 1;
        }

        private static bool FullyFlush(Permutation perm, Player player)
        {
            var cats = perm.ToTiles().Select(t => t.GetCategory()).Distinct().ToList();
            return cats.Count == 1 && !IsHonor(cats[0]);
        }

        private static bool TwoAnkou(Permutation perm, Player player) => Kos(perm).Count >= 2;

        private static bool ThreeAnkou(Permutation perm, Player player) => Kos(perm).Count >= 3;

        private static bool TwoKan(Permutation perm, Player player) =>
            perm.blocks.Count(b => b.IsAAAA()) >= 2;

        private static bool ThreeKan(Permutation perm, Player player) =>
            perm.blocks.Count(b => b.IsAAAA()) >= 3;

        private static bool FourAnkan(Permutation perm, Player player) =>
            perm.blocks.Count(b => b.IsAAAA()) >= 4;

        private static bool SameColorDoubleOldYoung(Permutation perm, Player player)
        {
            foreach (var category in new[] { Tile.Category.Wan, Tile.Category.Suo, Tile.Category.Bing })
            {
                var runs = Runs(perm).Where(r => r.Category == category).ToList();
                if (runs.Count(r => r.Order == 1) >= 2 && runs.Count(r => r.Order == 7) >= 2)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TwoColorDoubleOldYoung(Permutation perm, Player player)
        {
            int matched = 0;
            foreach (var category in new[] { Tile.Category.Wan, Tile.Category.Suo, Tile.Category.Bing })
            {
                var runs = Runs(perm).Where(r => r.Category == category).ToList();
                if (runs.Any(r => r.Order == 1) && runs.Any(r => r.Order == 7))
                {
                    matched++;
                }
            }

            return matched >= 2;
        }

        private static bool AllNumberJin(Permutation perm, Player player) =>
            AllNumber(perm, player) && perm.blocks.Length > 0 && perm.blocks.All(b => b.IsABC());

        private static bool PureMoonInWater(Permutation perm, Player player) =>
            perm.GetPermType() == PermutationType.SEVEN_PAIRS && CountByTile(perm)[9] == 2;

        private static bool ThreeSevenFlower(Permutation perm, Player player)
        {
            var counts = CountByTile(perm);
            if (counts[6] < 3 || counts[15] < 3 || counts[24] < 3)
            {
                return false;
            }

            int rest = perm.ToTiles().Count(t =>
                IsHonor(t) || t.GetOrder() == 1 || t.GetOrder() == 5 || t.GetOrder() == 9);
            return rest == 5 && !perm.ToTiles().Any(t => !IsHonor(t) && t.GetOrder() == 7);
        }

        private static bool BeautySevenPairs(Permutation perm, Player player)
        {
            if (perm.GetPermType() != PermutationType.SEVEN_PAIRS)
            {
                return false;
            }

            var counts = CountByTile(perm);
            return counts[31] == 4 && counts[32] == 4 && counts[33] == 4 && counts[9] == 2;
        }

        private static bool ThreeKingdoms(Permutation perm, Player player)
        {
            var kos = Kos(perm);
            if (kos.Count != 3)
            {
                return false;
            }

            var cats = kos.Select(k => k.Category).Distinct().ToList();
            var orders = kos.Select(k => k.Order).ToList();
            return cats.Count == 3
                && cats.All(c => c is Tile.Category.Wan or Tile.Category.Suo or Tile.Category.Bing)
                && orders.Contains(2) && orders.Contains(5) && orders.Contains(8);
        }

        private static bool LessThanSeven(Permutation perm, Player player) =>
            XyShape(perm, 1);

        private static bool GreaterThanSeven(Permutation perm, Player player) =>
            XyShape(perm, 7);

        private static bool XyShape(Permutation perm, int start)
        {
            var tiles = perm.ToTiles();
            if (tiles.Count != 14)
            {
                return false;
            }

            int honors = tiles.Count(IsHonor);
            if (honors != 8 || tiles.Count - honors != 6)
            {
                return false;
            }

            var honorCounts = new int[8];
            foreach (var t in tiles.Where(IsHonor))
            {
                honorCounts[t.GetOrder() - 1]++;
            }

            if (honorCounts.Count(c => c > 0) != 4 || honorCounts.Any(c => c is not (0 or 2)))
            {
                return false;
            }

            return perm.blocks.Any(b => b.IsABC() && b.tiles[0].GetOrder() == start);
        }

        private static bool NorthSouthPass(Permutation perm, Player player)
        {
            var counts = CountByTile(perm);
            bool hasDragon = false;
            for (int suit = 0; suit < 3; suit++)
            {
                bool all = true;
                for (int n = 0; n < 9; n++)
                {
                    if (counts[suit * 9 + n] < 1)
                    {
                        all = false;
                        break;
                    }
                }

                hasDragon |= all;
            }

            if (!hasDragon)
            {
                return false;
            }

            int south = counts[28];
            int north = counts[30];
            return (south >= 2 && north >= 2) || (south >= 3 && north >= 1) || (north >= 3 && south >= 1);
        }

        private static bool FlowerInMirror(Permutation perm, Player player) =>
            perm.GetPermType() == PermutationType.SEVEN_PAIRS && CountByTile(perm)[13] == 2;

        private static bool MoonInWater(Permutation perm, Player player) =>
            perm.GetPermType() == PermutationType.SEVEN_PAIRS && CountByTile(perm)[9] == 2;

        private static bool Chidaichi(Permutation perm, Player player)
        {
            if (player == null || perm.ToTiles().Count != 14)
            {
                return false;
            }

            return perm.ToTiles().All(t => t.ContainsRed(player));
        }

        // ---- helpers ----

        private static int[] CountByTile(Permutation perm)
        {
            var counts = new int[34];
            foreach (var tile in perm.ToTiles())
            {
                int idx = Index(tile);
                if (idx >= 0)
                {
                    counts[idx]++;
                }
            }

            return counts;
        }

        private static int Index(Tile tile)
        {
            if (tile == null)
            {
                return -1;
            }

            int category = tile.GetCategory() switch
            {
                Tile.Category.Wan => 0,
                Tile.Category.Suo => 1,
                Tile.Category.Bing => 2,
                Tile.Category.Feng => 3,
                Tile.Category.Jian => 3,
                _ => -1,
            };
            if (category < 0)
            {
                return -1;
            }

            int order = tile.GetOrder();
            if (category == 3)
            {
                return 27 + order - 1;
            }

            return category * 9 + order - 1;
        }

        private static bool HasRuns(Permutation perm, int first, int second, int third, bool sameSuit = false)
        {
            var runs = Runs(perm);
            bool a = runs.Any(r => r.Order == first);
            bool b = runs.Any(r => r.Order == second);
            bool c = runs.Any(r => r.Order == third);
            if (!(a && b && c))
            {
                return false;
            }

            if (!sameSuit)
            {
                return true;
            }

            var categories = runs.Where(r => r.Order == first || r.Order == second || r.Order == third)
                .Select(r => r.Category)
                .Distinct()
                .ToList();
            return categories.Count == 1;
        }

        private static List<(Tile.Category Category, int Order)> Runs(Permutation perm) =>
            perm.blocks.Where(b => b.IsABC())
                .Select(b => (b.tiles[0].GetCategory(), b.tiles[0].GetOrder()))
                .ToList();

        private static List<(Tile.Category Category, int Order)> Kos(Permutation perm) =>
            perm.blocks.Where(b => b.IsAAA())
                .Select(b => (b.tiles[0].GetCategory(), b.tiles[0].GetOrder()))
                .ToList();

        private static bool IsNumbered(Tile tile)
        {
            var c = tile.GetCategory();
            return c == Tile.Category.Wan || c == Tile.Category.Suo || c == Tile.Category.Bing;
        }

        private static bool IsHonor(Tile tile) => IsHonor(tile.GetCategory());

        private static bool IsHonor(Tile.Category c) =>
            c == Tile.Category.Feng || c == Tile.Category.Jian;

        private static bool IsDragon(Tile.Category c) => c == Tile.Category.Jian;

        private static bool IsTerminalOrHonor(Tile tile)
        {
            if (IsHonor(tile))
            {
                return true;
            }

            return tile.GetOrder() is 1 or 9;
        }
    }
}
