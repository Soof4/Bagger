using Microsoft.Data.Sqlite;
using System.Data;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace Bagger {
    [ApiVersion(2, 1)]
    public class Bagger : TerrariaPlugin {
        public Bagger(Main game) : base(game) {
        }
        public override string Name => "Bagger";
        public override Version Version => new Version(1, 0, 0);
        public override string Author => "Soofa";
        public override string Description => "Gives people boss bags if they missed the fight.";

        private IDbConnection db;
        public static DatabaseManager dbManager;

        private List<int> downedBosses = new();
        public override void Initialize() {
            db = new SqliteConnection("Data Source=" + Path.Combine(TShock.SavePath, "Bagger.sqlite"));
            dbManager = new(db);

            Commands.ChatCommands.Add(new Command("bagger.getbags", GetBagsCmd, "getbags", "gb") { 
                AllowServer = false,
                HelpText = "Get bags for the bosses that you've missed."
            });
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
        }

        private void OnGamePostInitialize(EventArgs args) {
            if (NPC.downedSlimeKing) { downedBosses.Add(ItemID.KingSlimeBossBag); }
            if (NPC.downedBoss1) { downedBosses.Add(ItemID.EyeOfCthulhuBossBag); }
            if (NPC.downedBoss2 && WorldGen.crimson) { downedBosses.Add(ItemID.BrainOfCthulhuBossBag); }
            if (NPC.downedBoss2 && !WorldGen.crimson) { downedBosses.Add(ItemID.EaterOfWorldsBossBag); }
            if (NPC.downedBoss3) { downedBosses.Add(ItemID.SkeletronBossBag); }
            if (Main.hardMode) { downedBosses.Add(ItemID.WallOfFleshBossBag); }
            if (NPC.downedMechBoss1) { downedBosses.Add(ItemID.DestroyerBossBag); }
            if (NPC.downedMechBoss2) { downedBosses.Add(ItemID.TwinsBossBag); }
            if (NPC.downedMechBoss3) { downedBosses.Add(ItemID.SkeletronPrimeBossBag); }
            if (NPC.downedPlantBoss) { downedBosses.Add(ItemID.PlanteraBossBag); }
            if (NPC.downedGolemBoss) { downedBosses.Add(ItemID.GolemBossBag); }
            if (NPC.downedMoonlord) { downedBosses.Add(ItemID.MoonLordBossBag); }
        }

        private void OnNpcKilled(NpcKilledEventArgs args) {
            int bagId = -1;
            if (!TryGetBossBag(args.npc.netID, ref bagId) || downedBosses.Contains(bagId)) {
                return;
            }

            downedBosses.Add(bagId);
            foreach (TSPlayer plr in TShock.Players) {
                if (plr != null && plr.Active) {
                    if (dbManager.IsPlayerInDb(plr.Name)) {
                        string plrBagList = string.Join(",", dbManager.GetBagList(plr.Name)) + $",{bagId}";
                        dbManager.SavePlayer(plr.Name, plrBagList);
                    }
                    else {
                        dbManager.InsertPlayer(plr.Name, bagId.ToString());
                    }
                }
            }
        }

        private void GetBagsCmd(CommandArgs args) {
            List<int> plrBagList = new();
            try {
                plrBagList = dbManager.GetBagList(args.Player.Name);
            }
            catch (NullReferenceException) {
                dbManager.InsertPlayer(args.Player.Name);
            }

            foreach (int bagId in downedBosses) {
                if (!plrBagList.Contains(bagId)) {
                    args.Player.GiveItem(bagId, 1);
                }
            }

            dbManager.SavePlayer(args.Player.Name, string.Join(",", downedBosses));
        }

        private bool TryGetBossBag(int npcNetID, ref int bagId) {
            switch (npcNetID) {
                case NPCID.KingSlime: bagId = ItemID.KingSlimeBossBag; return NPC.downedSlimeKing;
                case NPCID.EyeofCthulhu: bagId = ItemID.EyeOfCthulhuBossBag; return NPC.downedBoss1;
                case NPCID.BrainofCthulhu: bagId = ItemID.BrainOfCthulhuBossBag; return NPC.downedBoss2;
                case NPCID.EaterofWorldsHead: bagId = ItemID.EaterOfWorldsBossBag; return NPC.downedBoss2 && !WorldGen.crimson;
                case NPCID.SkeletronHead: bagId = ItemID.SkeletronBossBag; return NPC.downedBoss3;
                case NPCID.WallofFlesh: bagId = ItemID.WallOfFleshBossBag; return Main.hardMode;
                case NPCID.TheDestroyer: bagId = ItemID.DestroyerBossBag; return NPC.downedMechBoss1;
                case NPCID.Spazmatism: bagId = ItemID.TwinsBossBag; return NPC.downedMechBoss2;
                case NPCID.Retinazer: bagId = ItemID.TwinsBossBag; return NPC.downedMechBoss2;
                case NPCID.SkeletronPrime: bagId = ItemID.SkeletronPrimeBossBag; return NPC.downedMechBoss3;
                case NPCID.Plantera: bagId = ItemID.PlanteraBossBag; return NPC.downedPlantBoss;
                case NPCID.Golem: bagId = ItemID.GolemBossBag; return NPC.downedGolemBoss;
                case NPCID.MoonLordCore: bagId = ItemID.MoonLordBossBag; return NPC.downedMoonlord;
                default: return false;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
            }
            base.Dispose(disposing);
        }
    }
}