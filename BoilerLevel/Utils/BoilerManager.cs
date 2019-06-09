using BoilerLevel.Models;
using SQLite;

namespace BoilerLevel.Utils
{
    public static class BoilerManager
    {
        public static TableQuery<Boiler> Boilers => SQL.Db.Table<Boiler>();

        public static int Count => Boilers?.Count() ?? 0;

        public static void Initialize()
        {
            SQL.Db.CreateTable<Boiler>();
        }

        public static Boiler GetBoiler(int id)
        {
            return SQL.Db.Get<Boiler>(id);
        }

        public static void AddBoiler(Boiler boiler)
        {
            SQL.Db.Insert(boiler);
        }

        public static void DeleteBoiler(Boiler boiler)
        {
            foreach (var measurement in boiler.Measurments)
            {
                MeasurementManager.DeleteMeasurement(measurement);
            }
            SQL.Db.Delete<Boiler>(boiler.Id);
        }

        public static void UpdateBoiler(Boiler boiler)
        {
            SQL.Db.Update(boiler);
        }
    }
}