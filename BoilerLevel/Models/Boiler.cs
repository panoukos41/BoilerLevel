using OpenExtensions.Services;
using SQLite;
using System;
using System.Collections.Generic;

namespace BoilerLevel.Models
{
    [Table("BoilerTable")]
    public class Boiler
    {
        public Boiler() { }

        public Boiler(string name)
        {
            Name = name;
            DateCreated = DateTime.Now;
        }

        /// <summary>
        /// Primary key.
        /// </summary>
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateCreated { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        [Ignore]
        public int Count => Measurments.Count;

        [Ignore]
        public List<Measurment> Measurments => SQL.Db.Table<Measurment>()?.Where(x => x.BoilerId == Id).ToList() ?? new List<Measurment>();

        [Ignore]
        public List<string> Template
        {
            get => JsonService.Deserialize<List<string>>(JsonTemplate);
            set => JsonTemplate = JsonService.Serialize(value);
        }

        public string JsonTemplate { get; set; }
    }
}