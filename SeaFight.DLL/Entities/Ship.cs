﻿using CustomORM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaFight.DLL.Entities
{
    public abstract class Ship
    {
        [PK]
        public int ID { get; set; }
        [FK]
        public int FieldID { get; set; }
        public int Length { get; set; }
        public int Speed { get; set; }
        public int AbilityRange { get; set; }
        public string Name { get; set; }
        static readonly int sizeLimit=4;
        static readonly int speedLimit=3;
        static readonly int abilityRangeLimit=3;

        public Ship (int fieldID, int length, int speed, int abilityRange, string name = "")
        {
            this.Length = length;
            this.FieldID = fieldID;
            this.Speed = speed;
            this.AbilityRange = abilityRange;
            this.Name = name;
        }
    }
}
