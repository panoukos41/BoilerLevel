using OpenExtensions.Services;
using SQLite;
using System;
using System.Collections.Generic;

namespace BoilerLevel.Models
{
    [Table("MeasurmentsTable")]
    public class Measurment
    {
        public Measurment() { }

        public Measurment(int boilerId)
        {
            BoilerId = boilerId;
            DateTime = DateTime.Now;
        }

        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [Indexed]
        public int BoilerId { get; set; }

        public DateTime DateTime { get; set; }

        public float Level { get; set; }

        public float Temperature { get; set; }

        [Ignore]
        public int Count => Values?.Count + 2 ?? 2;

        [Ignore]
        public Dictionary<string, float> Values
        {
            get => JsonService.Deserialize<Dictionary<string, float>>(JsonValues);
            set => JsonValues = JsonService.Serialize(value);
        }       

        public string JsonValues { get; set; }        
    }
}