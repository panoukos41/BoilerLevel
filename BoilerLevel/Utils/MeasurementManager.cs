using BoilerLevel.Models;

namespace BoilerLevel.Utils
{
    public static class MeasurementManager
    {
        public static int Count => SQL.Db.Table<Measurment>()?.Count() ?? 0;
        public static void Initialize()
        {
            SQL.Db.CreateTable<Measurment>();
        }

        public static Measurment GetMeasurement(int id)
        {
            return SQL.Db.Get<Measurment>(id);
        }

        public static void AddMeasurement(Measurment measurement)
        {
            SQL.Db.Insert(measurement);
        }

        public static void DeleteMeasurement(Measurment measurement)
        {
            SQL.Db.Delete<Measurment>(measurement.Id);
        }

        public static void UpdateMeasurement(Measurment measurement)
        {
            SQL.Db.Update(measurement);
        }
    }
}