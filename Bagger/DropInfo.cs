
namespace Bagger
{
    public class DropInfo
    {
        public int ItemID { get; set; }
        public int Stack { get; set; }

        public DropInfo(int itemID, int stack)
        {
            ItemID = itemID;
            Stack = stack;
        }
    }
}