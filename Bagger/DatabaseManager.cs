using MySql.Data.MySqlClient;
using System.Data;
using TShockAPI;
using TShockAPI.DB;

namespace Bagger
{
    public class DatabaseManager
    {

        private IDbConnection _db;

        public DatabaseManager(IDbConnection db)
        {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("Players",
                new SqlColumn("PlayerID", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },
                new SqlColumn("UUID", MySqlDbType.String) { Unique = true },
                new SqlColumn("ClaimedBossesMask", MySqlDbType.Int32)));
        }

        /// <exception cref="NullReferenceException"></exception>
        public int GetClaimedBossMask(string uuid)
        {
            using var reader = _db.QueryReader("SELECT * FROM Players WHERE UUID = @0", uuid);

            while (reader.Read())
            {
                return reader.Get<int>("ClaimedBossesMask");
            }
            throw new NullReferenceException();
        }

        public bool InsertPlayer(string uuid, int mask = 0)
        {
            return _db.Query("INSERT INTO Players (UUID, ClaimedBossesMask) VALUES (@0, @1)", uuid, mask) != 0;
        }

        public bool SavePlayer(string uuid, int mask)
        {
            return _db.Query("UPDATE Players SET ClaimedBossesMask = @0 WHERE UUID = @1", mask, uuid) != 0;
        }

        public bool IsPlayerInDb(string uuid)
        {
            return _db.QueryScalar<int>("SELECT COUNT(*) FROM Players WHERE UUID = @0", uuid) > 0;
        }
    }
}
