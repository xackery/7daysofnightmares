using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoreApi
{

    //This is a lookup table of possible nightmares with weights
    class NightmareType
    {
        public string Message;
        public string ClassName;
        public int MinimumDay;
        public int Weight;        

        public NightmareType(string message, string className, int minimumDay, int weight)
        {
            Message = message;
            ClassName = className;
            MinimumDay = minimumDay;
            Weight = weight;
        }
    }

    //This is a holder for nightmare types with scheduled time information
    class NightmareSeed
    {
        public List<NightmareType> NightmareTypes = new List<NightmareType>();
        public ulong SeedTime;
        public NightmareSeed(NightmareType nt, ulong seedTime)
        {
            NightmareTypes.Add(nt);
            SeedTime = seedTime;
        }
        public void AddType(NightmareType nt)
        {
            NightmareTypes.Add(nt);
        }
    }

    class NightmareInstance
    {
        public EntityEnemy Instance;
        public NightmareType InstanceType;

        public EntityAlive TargetEntity;
                
        public void PickRandomTarget()
        {
            API.Log("Picking target...");
            Random rnd = new Random();
            List<EntityPlayer> players = GameManager.Instance.World.GetPlayers();
            int roll = rnd.Next(0, players.Count);
            API.Log("Rolled: " + roll + " Size of players: " + players.Count);
            TargetEntity = (EntityAlive)players[roll];
            
            API.Log("Targetting " + TargetEntity.belongsPlayerId);

            //Instance.SetInvestigatePosition(TargetEntity.position, 2400);
        }
    }

    class Nightmare
    {
        //_cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", API.ChatColor, _phrase200), "Server", false, "", false));
        public static bool IsRunning = false;
        private static Thread th;

        //next time a nightmare will happen
        private static ulong NextNightmare;
        //Next time to reseed nightmares
        private static ulong NextNightmareReseed;
        
        private static List<NightmareInstance> NightmareInstances = new List<NightmareInstance>();
        private static List<NightmareType> NightmareTypes = new List<NightmareType>();
        private static List<NightmareSeed> NightmareSeeds = new List<NightmareSeed>();

        public static void Load()
        {
            Start();
            IsRunning = true;
        }
        
        private static void Start()
        {
            NightmareType n;
            n = new NightmareType("A nightmare manifests itself", "zombieSpider", 0, 10);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "animalZombieDog", 0, 100);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "animalWolf", 0, 80);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "animalBear", 0, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieFatCop", 0, 10);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieArlene", 0, 20);
            NightmareTypes.Add(n);

            n = new NightmareType("A nightmare manifests itself", "animalBear", 1, 50);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieSoldier", 1, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieSoldier", 1, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieSkateboarderFeral", 1, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieCheerleaderFeral", 1, 20);
            NightmareTypes.Add(n);
            

            n = new NightmareType("A nightmare manifests itself", "zombieOldTimer", 2, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieArleneFeral", 2, 20);
            NightmareTypes.Add(n);
            //n = new NightmareType("A nightmare manifests itself", "animalZombieVulture", 2, 20);
            //NightmareTypes.Add(n);

            n = new NightmareType("A nightmare manifests itself", "zombieSoldierFeral", 3, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieFatHawaiianFeral", 3, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieSnowFeral", 3, 20);
            NightmareTypes.Add(n);

            n = new NightmareType("A nightmare manifests itself", "zombieScreamer", 4, 100);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieSpiderFeral", 4, 100);
            NightmareTypes.Add(n);

            n = new NightmareType("A nightmare manifests itself", "animalZombieBear", 5, 20);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieStripperFeral", 5, 20);
            NightmareTypes.Add(n);
            
            n = new NightmareType("A nightmare manifests itself", "zombieScreamerFeral", 6, 150);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieFatCopFeral", 6, 150);
            NightmareTypes.Add(n);
            n = new NightmareType("A nightmare manifests itself", "zombieFatCopFeralRadiated", 6, 100);
            NightmareTypes.Add(n);


            n = new NightmareType("A nightmare manifests itself", "zombieFatCopFeralRadiated", 7, 350);
            NightmareTypes.Add(n);


            API.Log("*** Initialized Nightmare Mod, Added " + NightmareTypes.Count + " nightmare types ***");

            th = new Thread(new ThreadStart(StatusCheck));
            th.IsBackground = true;
            th.Start();
        }

        public static void Unload()
        {
            th.Abort();
            IsRunning = false;
        }   

        private static void StatusCheck()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(6000);
                    if (NextNightmareReseed <= GameManager.Instance.World.GetWorldTime()) SeedNightmares();
                    API.Log("Time: " + GameManager.Instance.World.GetWorldTime()+", nextSeed: "+NextNightmareReseed+", nextEvent: "+NextNightmare);
                    

                    if (ConnectionManager.Instance.ClientCount() < 1 || GameManager.Instance.World.GetPlayers().Count < 1) continue;

                    //AI LOGIC/CLEANUP
                    for (int i = NightmareInstances.Count - 1; i > 0; i--)
                    {
                        //Remove dead/null instances
                        if (NightmareInstances[i].Instance == null || NightmareInstances[i].Instance.IsDead())
                        {
                            NightmareInstances.RemoveAt(i);
                            API.Log("Removed nightmare instance " + i + ", " + NightmareInstances.Count + " left");
                            continue;
                        }

                        //Ensure we got a target.
                        if (NightmareInstances[i].TargetEntity == null || NightmareInstances[i].TargetEntity.IsDead())
                        {
                            API.Log(i + " is picking a new random player target");
                            NightmareInstances[i].PickRandomTarget();
                        }
                    }

                    if (NextNightmare <= GameManager.Instance.World.GetWorldTime() && NightmareSeeds.Count > 0)
                    {
                        foreach (NightmareType nt in NightmareSeeds[0].NightmareTypes) SpawnNightmare(nt);
                        
                        NightmareSeeds.RemoveAt(0);
                        NextNightmare = NightmareSeeds[0].SeedTime;
                    }
                } catch (Exception e)
                {
                    API.Log("Exception during CheckStatus: " + e.Message);
                }
            }
        }

        public static void SeedNightmares()
        {
            try
            {
                NightmareSeeds.Clear();
                ulong currentTime = GameManager.Instance.World.GetWorldTime();

                API.Log("Seeding nightmares with current time: " + currentTime);

                Random rnd = new Random();
                int playerCount = GameManager.Instance.World.Players.Count;
                if (playerCount < 2) playerCount = 2;

                int maxNumberofNightmares = playerCount + GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
                if (maxNumberofNightmares < 2) maxNumberofNightmares = 2;

                int numberOfNightmares = rnd.Next(1, maxNumberofNightmares);

                int batchNumberofNightmares = playerCount + (GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) / 2);
                if (batchNumberofNightmares < 2) batchNumberofNightmares = 2;

                API.Log("Preparing " + numberOfNightmares + " total nightmares for the next 24 hours");
                
                ulong lastTime = currentTime;
                int timeGap = 24000 / numberOfNightmares;
                API.Log("Timegap: " + timeGap);
                if (timeGap < 2) timeGap = 200;

                for (int i = 0; i < numberOfNightmares; i++)
                {
                    string nightmareGroup = "";
                    NightmareType nt = GetRandomNightmareType();
                    //1440 minutes in a day.
                    NightmareSeed ns = new NightmareSeed(nt, lastTime + (ulong)rnd.Next(1, timeGap));
                    nightmareGroup += nt.ClassName;

                    //add additional nightmares based on batch
                    int batchCount = rnd.Next(1, batchNumberofNightmares);
                    for (int j = 1; j < batchCount; j++) {
                        nt = GetRandomNightmareType();
                        ns.AddType(nt);
                        nightmareGroup += ", "+nt.ClassName;
                    }

                    lastTime = ns.SeedTime;
                    API.Log(NightmareSeeds.Count + " contains: " + nightmareGroup+ " and will spawn at " + ns.SeedTime);
                    if (i == 0) NextNightmare = ns.SeedTime;
                    NightmareSeeds.Add(ns);
                }

                NextNightmareReseed = currentTime + 24000;
            } catch (Exception e)
            {
                API.Log("Exception while seeding nightmares: " + e.Message);
            }
        }

        public static NightmareType GetRandomNightmareType()
        {
            try
            {
                Random rnd = new Random();
                int poolSize = 0;

                Dictionary<int, NightmareType> rngPool = new Dictionary<int, NightmareType>();

                if (NightmareTypes.Count() == 0)
                {
                    API.Log("No nightmare entries found");
                    return null;
                }

                foreach (NightmareType nt in NightmareTypes)
                {
                    if (nt.MinimumDay > CurrentDay()) continue;
                    int poolWeight = nt.Weight;
                    if (nt.MinimumDay < CurrentDay())
                    {
                        int negativeWeight = CurrentDay() - nt.MinimumDay * 5;
                        poolWeight -= negativeWeight;
                    }

                    if (poolWeight < 1) poolWeight = 1;
                    poolSize += poolWeight;
                    rngPool.Add(poolSize, nt);
                }

                if (rngPool.Count() == 0)
                {
                    API.Log("No rngPool found");
                    return NightmareTypes[0];
                }


                int result = rnd.Next(0, poolSize);

                bool isFound = false;

                foreach (KeyValuePair<int, NightmareType> entry in rngPool)
                {
                    if (entry.Key <= result) continue;

                    API.Log("Rolled " + result + " of " + poolSize + ", which is a " + entry.Value.ClassName);
                    return entry.Value;
                }

                if (!isFound)
                {
                    API.Log("Did not find RNG entry, falling back to first entry");
                    return NightmareTypes[0];
                }
            } catch (Exception e)
            {
                API.Log("Exception while picking random nightmare: " + e.Message);
            }

            return NightmareTypes[0];
        }
        

        //Spawns a nightmare
        public static void SpawnNightmare(NightmareType nt)
        {

            NightmareInstance ni = new NightmareInstance();

            ni.InstanceType = nt;

            try
            {
                API.Log("Spawning instance " + ni.InstanceType.ClassName);


                using (Dictionary<int, EntityClass>.KeyCollection.Enumerator enumerator3 = EntityClass.list.Keys.GetEnumerator())
                {

                    while (enumerator3.MoveNext())
                    {

                        //Find entity class
                        int current3 = enumerator3.Current;
                        //API.Log("Iterating enum, on " + current3+ " ("+EntityClass.list[current3].entityClassName+")");
                        if (!EntityClass.list[current3].entityClassName.Equals(ni.InstanceType.ClassName)) continue;
                        //API.Log("Found entity " + current3 + " to be classname " + ni.InstanceType.ClassName);
                        //Find a place to spawn it
                        int x = 0, y = 0, z = 0;

                        API.Log("Finding target");

                        ni.PickRandomTarget();
                        if (ni.TargetEntity == null)
                        {
                            API.Log("Failed to find a target entity");
                            return;
                        }
                        API.Log("Finding location to spawn");

                        bool isSpawnLocFound = false;
                        for (int i = 0; i < 10; i++)
                        {
                            if (GameManager.Instance.World.FindRandomSpawnPointNearPlayer(ni.TargetEntity, 30, out x, out y, out z, 500))
                            {
                                isSpawnLocFound = true;
                                break;
                            }
                        }

                        if (!isSpawnLocFound)
                        {
                            API.Log("Failed to find spawn location near player" + ni.TargetEntity);
                            return;
                        }

                        UnityEngine.Vector3 pos = new UnityEngine.Vector3(x, y, z);

                        API.Log("Creating entity.." + current3);
                        ni.Instance = (EntityEnemy)EntityFactory.CreateEntity(current3, pos);

                        //EntityClass.list.Add()
                        //CurrentNightmare.TargetEntity.PlayOneShot()
                        //CurrentNightmare.NightmareEntity.SetVelocity();
                        API.Log("Setting entity name");
                        ni.Instance.SetEntityName("A nightmare");
                        //ni.Instance.MovementRunning = true;
                        
                        //ni.Instance.lootContainer.AddItem()
                        GameManager.Instance.World.SpawnEntityInWorld(ni.Instance);
                        API.Log("Broadcasting spawn alert");
                        BroadcastMessage(ni.InstanceType.Message);
                        //EntityClass.list[current3].ExperienceValue
                        NightmareInstances.Add(ni);
                        API.Log("Added instance " + ni.InstanceType.ClassName + " at " + x + ", " + y + ", " + z);

                        ni.Instance.SetAttackTarget(ni.TargetEntity, 2400);
                        return;
                    }
                }
                API.Log("Failed to find class type of nightmare: " + ni.InstanceType.ClassName);
            } 
            catch (Exception e)
            {
                API.Log("Failed to spawn nightmare: " + e.Message);
                return;
            }
        }
        

        public static void BroadcastMessage(string message, string color = "[FF0000]")
        {
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            foreach (ClientInfo _cInfo in _cInfoList) WhisperMessage(_cInfo, message);
        }

        public static void WhisperMessage(ClientInfo _cInfo, string message, string color = "[FF0000]")
        {
            if (_cInfo == null) return;
            if (color == "") color = "[FF0000]";

            string Nickname = "";
            _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", color, message), Nickname, false, "", false));
        }

        public static int CurrentHour()
        {
                       
            return GameUtils.WorldTimeToHours(GameManager.Instance.World.GetWorldTime());
        }

        private static int CurrentDay()
        {            
            return GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        }       
    }
}