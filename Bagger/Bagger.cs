using Microsoft.Data.Sqlite;
using System.Data;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Bagger
{
    [ApiVersion(2, 1)]
    public class Bagger : TerrariaPlugin
    {
        public Bagger(Main game) : base(game)
        {
        }
        public override string Name => "Bagger";
        public override Version Version => new Version(1, 2, 0);
        public override string Author => "Soofa";
        public override string Description => "Gives people boss bags if they missed the fight.";

        private static IDbConnection db = new SqliteConnection("Data Source=" + Path.Combine(TShock.SavePath, "Bagger.sqlite"));
        public static DatabaseManager dbManager = new DatabaseManager(db);
        public static Config Config = Config.Reload();
        private List<int> DownedBosses = new List<int>();


        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            GeneralHooks.ReloadEvent += OnReload;

            Commands.ChatCommands.Add(new Command("bagger.getbags", GetBagsCmd, "getbags", "gb")
            {
                AllowServer = false,
                HelpText = "Get bags for the bosses that you've missed."
            });
        }

        private void OnGamePostInitialize(EventArgs args)
        {
            if (IsDefeated(NPCID.KingSlime)) { DownedBosses.Add(NPCID.KingSlime); }
            if (IsDefeated(NPCID.EyeofCthulhu)) { DownedBosses.Add(NPCID.EyeofCthulhu); }
            if (IsDefeated(NPCID.EaterofWorldsHead)) { DownedBosses.Add(NPCID.EaterofWorldsHead); }
            if (IsDefeated(NPCID.BrainofCthulhu)) { DownedBosses.Add(NPCID.BrainofCthulhu); }
            if (IsDefeated(NPCID.QueenBee)) { DownedBosses.Add(NPCID.QueenBee); }
            if (IsDefeated(NPCID.SkeletronHead)) { DownedBosses.Add(NPCID.SkeletronHead); }
            if (IsDefeated(NPCID.Deerclops)) { DownedBosses.Add(NPCID.Deerclops); }
            if (IsDefeated(NPCID.WallofFlesh)) { DownedBosses.Add(NPCID.WallofFlesh); }
            if (IsDefeated(NPCID.QueenSlimeBoss)) { DownedBosses.Add(NPCID.QueenSlimeBoss); }
            if (IsDefeated(NPCID.TheDestroyer)) { DownedBosses.Add(NPCID.TheDestroyer); }
            if (IsDefeated(NPCID.Spazmatism)) { DownedBosses.Add(NPCID.Spazmatism); }
            if (IsDefeated(NPCID.SkeletronPrime)) { DownedBosses.Add(NPCID.SkeletronPrime); }
            if (IsDefeated(NPCID.Plantera)) { DownedBosses.Add(NPCID.Plantera); }
            if (IsDefeated(NPCID.Golem)) { DownedBosses.Add(NPCID.Golem); }
            if (IsDefeated(NPCID.DukeFishron)) { DownedBosses.Add(NPCID.DukeFishron); }
            if (IsDefeated(NPCID.HallowBoss)) { DownedBosses.Add(NPCID.HallowBoss); }
            if (IsDefeated(NPCID.CultistBoss)) { DownedBosses.Add(NPCID.CultistBoss); }
            if (IsDefeated(NPCID.MoonLordCore)) { DownedBosses.Add(NPCID.MoonLordCore); }
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (!args.npc.boss || DownedBosses.Contains(args.npc.type) || !IsDefeated(args.npc.type))
            {
                return;
            }

            DownedBosses.Add(args.npc.type);

            foreach (TSPlayer plr in TShock.Players)
            {
                if (plr != null && plr.Active)
                {
                    if (dbManager.IsPlayerInDb(plr.UUID))
                    {
                        int claimedMask = dbManager.GetClaimedBossMask(plr.UUID);
                        claimedMask = AddToTheMask(claimedMask, args.npc.type);

                        dbManager.SavePlayer(plr.UUID, claimedMask);
                    }
                    else
                    {
                        dbManager.InsertPlayer(plr.UUID, AddToTheMask(0, args.npc.type));
                    }
                }
            }
        }

        private bool IsDefeated(int type)
        {
            var unlockState = Main.BestiaryDB.FindEntryByNPCID(type).UIInfoProvider.GetEntryUICollectionInfo().UnlockState;
            return unlockState == Terraria.GameContent.Bestiary.BestiaryEntryUnlockState.CanShowDropsWithDropRates_4;
        }

        private int AddToTheMask(int mask, int type)
        {
            return type switch
            {
                NPCID.KingSlime => mask | 1,
                NPCID.EyeofCthulhu => mask | 2,
                NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail => mask | 4,
                NPCID.BrainofCthulhu => mask | 8,
                NPCID.QueenBee => mask | 16,
                NPCID.SkeletronHead => mask | 32,
                NPCID.Deerclops => mask | 64,
                NPCID.WallofFlesh => mask | 128,
                NPCID.QueenSlimeBoss => mask | 256,
                NPCID.TheDestroyer => mask | 512,
                NPCID.Spazmatism or NPCID.Retinazer => mask | 1024,
                NPCID.SkeletronPrime => mask | 2048,
                NPCID.Plantera => mask | 4096,
                NPCID.Golem => mask | 8192,
                NPCID.DukeFishron => mask | 16384,
                NPCID.HallowBoss => mask | 32768,
                NPCID.CultistBoss => mask | 65536,
                NPCID.MoonLordCore => mask | 131072,
                _ => mask,
            };
        }

        private void GetBagsCmd(CommandArgs args)
        {
            int claimMask = 0;
            if (dbManager.IsPlayerInDb(args.Player.UUID))
            {
                claimMask = dbManager.GetClaimedBossMask(args.Player.UUID);
            }
            else
            {
                dbManager.InsertPlayer(args.Player.UUID);
            }

            if ((claimMask & 1) != 1 && DownedBosses.Contains(NPCID.KingSlime))
            {
                claimMask |= 1;
                args.Player.GiveItem(Config.KingSlimeDrop.ItemID, Config.KingSlimeDrop.Stack);
            }

            if ((claimMask & 2) != 2 && DownedBosses.Contains(NPCID.EyeofCthulhu))
            {
                claimMask |= 2;
                args.Player.GiveItem(Config.EyeOFCthulhuDrop.ItemID, Config.EyeOFCthulhuDrop.Stack);
            }

            if ((claimMask & 4) != 4 && DownedBosses.Contains(NPCID.EaterofWorldsHead))
            {
                claimMask |= 4;
                args.Player.GiveItem(Config.EaterOfWorldsDrop.ItemID, Config.EaterOfWorldsDrop.Stack);
            }

            if ((claimMask & 8) != 8 && DownedBosses.Contains(NPCID.BrainofCthulhu))
            {
                claimMask |= 8;
                args.Player.GiveItem(Config.BrainOfCthulhuDrop.ItemID, Config.BrainOfCthulhuDrop.Stack);
            }

            if ((claimMask & 16) != 16 && DownedBosses.Contains(NPCID.QueenBee))
            {
                claimMask |= 16;
                args.Player.GiveItem(Config.QueenBeeDrop.ItemID, Config.QueenBeeDrop.Stack);
            }

            if ((claimMask & 32) != 32 && DownedBosses.Contains(NPCID.SkeletronHead))
            {
                claimMask |= 32;
                args.Player.GiveItem(Config.SkeletronDrop.ItemID, Config.SkeletronDrop.Stack);
            }

            if ((claimMask & 64) != 64 && DownedBosses.Contains(NPCID.Deerclops))
            {
                claimMask |= 64;
                args.Player.GiveItem(Config.Deerclops.ItemID, Config.Deerclops.Stack);
            }

            if ((claimMask & 128) != 128 && DownedBosses.Contains(NPCID.WallofFlesh))
            {
                claimMask |= 128;
                args.Player.GiveItem(Config.WallOfFleshDrop.ItemID, Config.WallOfFleshDrop.Stack);
            }

            if ((claimMask & 256) != 256 && DownedBosses.Contains(NPCID.QueenSlimeBoss))
            {
                claimMask |= 256;
                args.Player.GiveItem(Config.QueenSlimeDrop.ItemID, Config.QueenSlimeDrop.Stack);
            }

            if ((claimMask & 512) != 512 && DownedBosses.Contains(NPCID.TheDestroyer))
            {
                claimMask |= 512;
                args.Player.GiveItem(Config.TheDestroyerDrop.ItemID, Config.TheDestroyerDrop.Stack);
            }

            if ((claimMask & 1024) != 1024 && DownedBosses.Contains(NPCID.Spazmatism))
            {
                claimMask |= 1024;
                args.Player.GiveItem(Config.TheTwinsDrop.ItemID, Config.TheTwinsDrop.Stack);
            }

            if ((claimMask & 2048) != 2048 && DownedBosses.Contains(NPCID.SkeletronPrime))
            {
                claimMask |= 2048;
                args.Player.GiveItem(Config.SkeletronPrimeDrop.ItemID, Config.SkeletronPrimeDrop.Stack);
            }

            if ((claimMask & 4096) != 4096 && DownedBosses.Contains(NPCID.Plantera))
            {
                claimMask |= 4096;
                args.Player.GiveItem(Config.PlanteraDrop.ItemID, Config.PlanteraDrop.Stack);
            }

            if ((claimMask & 8192) != 8192 && DownedBosses.Contains(NPCID.Golem))
            {
                claimMask |= 8192;
                args.Player.GiveItem(Config.GolemDrop.ItemID, Config.GolemDrop.Stack);
            }

            if ((claimMask & 16384) != 16384 && DownedBosses.Contains(NPCID.DukeFishron))
            {
                claimMask |= 16384;
                args.Player.GiveItem(Config.DukeFishronDrop.ItemID, Config.DukeFishronDrop.Stack);
            }

            if ((claimMask & 32768) != 32768 && DownedBosses.Contains(NPCID.HallowBoss))
            {
                claimMask |= 32768;
                args.Player.GiveItem(Config.EmpressOfLight.ItemID, Config.EmpressOfLight.Stack);
            }

            if ((claimMask & 65536) != 65536 && DownedBosses.Contains(NPCID.CultistBoss))
            {
                claimMask |= 65536;
                args.Player.GiveItem(Config.LunaticCultistDrop.ItemID, Config.LunaticCultistDrop.Stack);
            }

            if ((claimMask & 131072) != 131072 && DownedBosses.Contains(NPCID.MoonLordCore))
            {
                claimMask |= 131072;
                args.Player.GiveItem(Config.MoonlordDrop.ItemID, Config.MoonlordDrop.Stack);
            }

            dbManager.SavePlayer(args.Player.UUID, claimMask);
        }

        private static void OnReload(ReloadEventArgs args)
        {
            Config = Config.Reload();
            args.Player.SendSuccessMessage("Bagger has been reloaded.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }
    }
}
