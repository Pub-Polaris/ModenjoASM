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
            public YakuEntry(YakuType type, int fu, Func<Permutation, Player, bool> predicate)
            {
                Type = type;
                Fu = fu;
                Predicate = predicate;
            }

            public YakuType Type { get; }
            public int Fu { get; }
            public Func<Permutation, Player, bool> Predicate { get; }
        }

        public static void RegisterAll()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;

            Add("four_pairs_seven", 7, FourPairsSeven, "11223344556677m");
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
            Add("big_three_dragons", 28, BigThreeDragons, "555z666z777z111m22m");
            Add("four_kings", 28, FourKings, "111z222z333z444z11m");
            Add("four_kan", 28, FourKan, "1111m2222p3333s4444z11z");
            Add("all_honors", 28, AllHonors, "111z222z333z444z55z");
            Add("all_terminals", 28, AllTerminals, "111m999m111p999p11s");
            Add("all_green", 28, AllGreen, "222s333s444s666s88s");
            Add("thirteen_orphans", 28, ThirteenOrphans, "19m19p19s1234567z1m");
            Add("nine_gates", 28, NineGates, "1112345678999m11m");
            Add("pure_fully_terminal", 7, PureFullyTerminal, "123m789m111p999p11s");
            Add("mixed_fully_terminal", 5, MixedFullyTerminal, "123m789p111s555z11z");
            Add("mixed_terminals", 5, MixedTerminals, "111m999p111s555z11z");
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
        public static int ScoreFu(Permutation perm, Player player)
        {
            if (perm == null || player == null)
            {
                return 0;
            }

            int fu = 0;
            foreach (var entry in _entries)
            {
                try
                {
                    if (entry.Predicate(perm, player))
                    {
                        fu += entry.Fu;
                    }
                }
                catch
                {
                    // A single custom predicate must not break settlement.
                }
            }

            return fu;
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

        private static void Add(string id, int fu, Func<Permutation, Player, bool> predicate, string example)
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
            _entries.Add(new YakuEntry(yaku, fu, predicate));
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
