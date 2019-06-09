using SQLite;
using System;
using System.IO;

namespace BoilerLevel
{
    public static class SQL
    {
        private static SQLiteConnection _Db;
        public static SQLiteConnection Db
        {
            get => _Db ?? (_Db = new SQLiteConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "database.db3")));
        }
    }
}