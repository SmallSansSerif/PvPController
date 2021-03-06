﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using PvPController.StorageTypes;

namespace PvPController
{
    public class Database
    {
        private MongoClient client;
        private IMongoDatabase db;
        private IMongoCollection<BsonDocument> weaponCollection;
        private IMongoCollection<BsonDocument> projectileCollection;
        private IMongoCollection<BsonDocument> weaponBuffCollection;

        public Database(Config config)
        {
            var host = config.Database.Hostname;
            var port = config.Database.Port;
            var dbName = config.Database.DBName;

            client = new MongoClient($"mongodb://{host}:{port}");
            db = client.GetDatabase(dbName);
            weaponCollection = db.GetCollection<BsonDocument>("weapons");
            projectileCollection = db.GetCollection<BsonDocument>("projectiles");
            weaponBuffCollection = db.GetCollection<BsonDocument>("weaponbuffs");
        }

        public List<Weapon> GetWeapons()
        {
            var weaponList = new List<Weapon>();
            var cursor = weaponCollection.Find(new BsonDocument()).ToCursor();
            foreach (var item in cursor.ToEnumerable())
            {
                var weapon = new Weapon(item["NetID"].AsInt32,
                                        Convert.ToSingle(item["CurrentDamage"]) / Convert.ToSingle(item["BaseDamage"]),
                                        Convert.ToSingle(item["CurrentVelocity"]) / Convert.ToSingle(item["BaseVelocity"]),
                                        Convert.ToBoolean(item["Banned"]), item["MinDamage"].AsInt32, item["MaxDamage"].AsInt32);
                weaponList.Add(weapon);
            }

            return weaponList;
        }

        public List<Projectile> GetProjectiles()
        {
            var projectileList = new List<Projectile>();
            var cursor = projectileCollection.Find(new BsonDocument()).ToCursor();
            foreach (var item in cursor.ToEnumerable())
            {
                var projectile = new Projectile(item["NetID"].AsInt32,
                                        Convert.ToSingle(item["DamageRatio"]),
                                        Convert.ToSingle(item["VelocityRatio"]),
                                        Convert.ToBoolean(item["Banned"]));
                projectileList.Add(projectile);
            }

            return projectileList;
        }


        /// <summary>
        /// Gets the weapon buffs and adds them to the appropriate weapons in the weapons list
        /// </summary>
        /// <param name="weapons"></param>
        public void addWeaponBuffs(List<Weapon> weapons)
        {
            var cursor = weaponBuffCollection.Find(new BsonDocument()).ToCursor();
            foreach (var item in cursor.ToEnumerable())
            {
                var buff = new Buff(Convert.ToInt32(item["NetID"]), Convert.ToInt32(item["Milliseconds"]));
                var weapon = weapons.FirstOrDefault(p => p.netID == Convert.ToInt32(item["WeaponNetID"]));
                if (weapon != null)
                {
                    weapon.buffs.Add(buff);
                }
            }
        }
    }
}