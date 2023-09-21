using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;

namespace Bagger {
    public class DatabaseManager {

        private IDbConnection _db;

        public DatabaseManager(IDbConnection db) {
            _db = db;

            var sqlCreator = new SqlTableCreator(db, new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable("Players",
                new SqlColumn("Name", MySqlDbType.String) { Primary = true, Unique = true },
                new SqlColumn("BagList", MySqlDbType.Text)));
        }

        /// <exception cref="NullReferenceException"></exception>
        public List<int> GetBagList(string name) {
            using var reader = _db.QueryReader("SELECT * FROM Players WHERE Name = @0", name);
            
            while (reader.Read()) {
                /*
                try {
                */
                    return reader.Get<string>("BagList").Split(',').Select(int.Parse).ToList();
            /*    
                }
                catch (FormatException) {
                    return new() { 0 };
                }
                */
            }
            throw new NullReferenceException();
        }

        public bool InsertPlayer(string name, string firstBagId = "0") {
            return _db.Query("INSERT INTO Players (Name, BagList) VALUES (@0, @1)", name, firstBagId) != 0;
        }

        public bool SavePlayer(string name, string bagList) {
            return _db.Query("UPDATE Players SET BagList = @0 WHERE Name = @1", bagList, name) != 0;
        }

        public bool IsPlayerInDb(string name) {
            return _db.QueryScalar<int>("SELECT COUNT(*) FROM Players WHERE Name = @0", name) > 0;
        }
    }
}
