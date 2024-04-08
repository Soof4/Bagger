using Terraria.ID;
using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace Bagger
{

    public class Config
    {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "BaggerConfig.json");
        
        public DropInfo KingSlimeDrop = new DropInfo(ItemID.KingSlimeBossBag, 1);
        public DropInfo EyeOFCthulhuDrop = new DropInfo(ItemID.EyeOfCthulhuBossBag, 1);
        public DropInfo EaterOfWorldsDrop = new DropInfo(ItemID.EaterOfWorldsBossBag, 1);
        public DropInfo BrainOfCthulhuDrop = new DropInfo(ItemID.BrainOfCthulhuBossBag, 1);
        public DropInfo QueenBeeDrop = new DropInfo(ItemID.QueenBeeBossBag, 1);
        public DropInfo SkeletronDrop = new DropInfo(ItemID.SkeletronBossBag, 1);
        public DropInfo Deerclops = new DropInfo(ItemID.DeerclopsBossBag, 1);
        public DropInfo WallOfFleshDrop = new DropInfo(ItemID.WallOfFleshBossBag, 1);
        public DropInfo QueenSlimeDrop = new DropInfo(ItemID.QueenSlimeBossBag, 1);
        public DropInfo TheDestroyerDrop = new DropInfo(ItemID.DestroyerBossBag, 1);
        public DropInfo TheTwinsDrop = new DropInfo(ItemID.TwinsBossBag, 1);
        public DropInfo SkeletronPrimeDrop = new DropInfo(ItemID.SkeletronPrimeBossBag, 1);
        public DropInfo PlanteraDrop = new DropInfo(ItemID.PlanteraBossBag, 1);
        public DropInfo GolemDrop = new DropInfo(ItemID.GolemBossBag, 1);
        public DropInfo DukeFishronDrop = new DropInfo(ItemID.FishronBossBag, 1);
        public DropInfo EmpressOfLight = new DropInfo(ItemID.FairyQueenBossBag, 1);
        public DropInfo LunaticCultistDrop = new DropInfo(ItemID.CultistBossBag, -1);
        public DropInfo MoonlordDrop = new DropInfo(ItemID.MoonLordBossBag, 1);

        public static Config Reload()
        {
            Config? c = null;

            if (File.Exists(ConfigPath))
            {
                c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            }

            if (c == null)
            {
                c = new Config();
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));
            }

            return c;
        }
    }
}