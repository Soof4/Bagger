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
                new SqlColumn("Name", MySqlDbType.String) { Primary = true, Unique = true },
                new SqlColumn("ClaimedBossesMask", MySqlDbType.Int32)));
        }

        /// <exception cref="NullReferenceException"></exception>
        public int GetClaimedBossMask(string name)
        {
            using var reader = _db.QueryReader("SELECT * FROM Players WHERE Name = @0", name);

            while (reader.Read())
            {
                return reader.Get<int>("ClaimedBossesMask");
            }
            throw new NullReferenceException();
        }

        public bool InsertPlayer(string name, int mask = 0)
        {
            return _db.Query("INSERT INTO Players (Name, ClaimedBossesMask) VALUES (@0, @1)", name, mask) != 0;
        }

        public bool SavePlayer(string name, int mask)
        {
            return _db.Query("UPDATE Players SET ClaimedBossesMask = @0 WHERE Name = @1", mask, name) != 0;
        }

        public bool IsPlayerInDb(string name)
        {
            return _db.QueryScalar<int>("SELECT COUNT(*) FROM Players WHERE Name = @0", name) > 0;
        }
    }
}
