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
            List<EntityPlayer> players = new List<EntityPlayer>();

            foreach (var player in GameManager.Instance.World.GetPlayers())
            {
                if (player.IsSpawned() && !player.IsDead()) players.Add(player);
            }

            if (players.Count < 1)
            {
                API.Log("No alive/spawned players found to target, returning");
                return;
            }

            if (Instance != null) //entity is already spawned, find closest target
            {
                API.Log("Picking new target...");                
                float distance = 99999999;
                float newDistance;
                EntityPlayer closestPlayer = null;
                foreach (var player in players)
                {
                    newDistance = player.GetDistance(Instance);
                    if (newDistance > 500) continue; //don't bother chasing players really far away.
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        closestPlayer = player;
                    }
                }
                if (closestPlayer != null)
                {
                    API.Log("Closest player "+ TargetEntity.belongsPlayerId + " is " + distance + " away");
                    TargetEntity = (EntityAlive)closestPlayer;
                    return;
                }
            }
            

            API.Log("Picking random target...");
            Random rnd = new Random();
            
            int roll = rnd.Next(0, players.Count-1);
            API.Log("Rolled: " + roll + " Size of players: " + players.Count);
            TargetEntity = (EntityAlive)players[roll];
            
            API.Log("Targetting " + TargetEntity.belongsPlayerId);
        }
    }

    class Nightmare
    {
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

            th = new Thread(new ThreadStart(MainLoop));
            th.IsBackground = true;
            th.Start();
        }

        public static void Unload()
        {
            th.Abort();
            IsRunning = false;
        }   

        private static void MainLoop()
        {

            //Lasttime only triggers when server is offline, and saves effort.
            ulong lastTime = 0;
            ulong currentTime = 0;
            while (true)
            {
                try
                {
                    Thread.Sleep(6000);
                    currentTime = GameManager.Instance.World.GetWorldTime();
                    if (NextNightmareReseed <= currentTime) SeedNightmares();                    
                    if (currentTime == lastTime) continue;
                    lastTime = currentTime;
                    string strNextNightmare = "";
                    if (NextNightmare < currentTime) strNextNightmare = "None";
                    if (NextNightmare >= currentTime) strNextNightmare = string.Format("{1} ({2:0}s)", NextNightmare, ((NextNightmare - currentTime) / 5.8f));
                    API.Log(string.Format("Time: {0}, NextNightmare: {1} ({2:0}s), NextReseed: {3} ({4:0}s [{5} Nightmares Currently Spawned])", currentTime, strNextNightmare, NextNightmareReseed, ((NextNightmareReseed - currentTime) / 5.8f), NightmareInstances.Count));

                    if (ConnectionManager.Instance.ClientCount() < 1 || GameManager.Instance.World.GetPlayers().Count < 1) continue;

                    //AI LOGIC/CLEANUP
                    for (int i = NightmareInstances.Count - 1; i > -1; i--)
                    {
                        int index = i;
                        NightmareInstance ni = NightmareInstances[index];

                        //Remove dead/null instances
                        if (ni == null || ni.Instance == null || ni.Instance.IsDead())
                        {
                            NightmareInstances.RemoveAt(index);
                            API.Log(string.Format("Removed nightmare instance {0}, {1} nightmares remain",  index , NightmareInstances.Count));
                            continue;
                        }
                        //Ensure entity is behaving like a nightmare
                        if (ni.Instance.GetAttackTarget() == null && ni.TargetEntity != null && !ni.TargetEntity.IsDead() && ni.TargetEntity.IsSpawned())
                        {
                            API.Log(string.Format("{0} {1} was evaded by target player {2}, re-engaging (it's a nightmare afterall)...", ni.InstanceType.ClassName, index, ni.TargetEntity.belongsPlayerId));
                            ni.Instance.SetAttackTarget(ni.TargetEntity, 1200);
                            continue;
                        }


                        //Ensure target is still legit
                        if (ni.TargetEntity == null || (ni.TargetEntity.IsDead() && ni.TargetEntity.IsSpawned()))
                        {
                            API.Log(string.Format("{0} {1} thinks target is dead/disconnected, picking new target...", ni.InstanceType.ClassName, index));
                            ni.PickRandomTarget();
                            if (ni.TargetEntity == null)
                            {
                                NightmareInstances.RemoveAt(index);
                                API.Log("Removed nightmare instance " + index + " due to no target found, " + NightmareInstances.Count + " left");
                                continue;
                            }
                            //engage target
                            ni.Instance.SetAttackTarget(ni.TargetEntity, 1200);
                        }
                    }

                    if (NightmareSeeds.Count > 0 && NextNightmare <= GameManager.Instance.World.GetWorldTime())
                    {
                        foreach (NightmareType nt in NightmareSeeds[0].NightmareTypes) SpawnNightmare(nt);
                        
                        NightmareSeeds.RemoveAt(0);
                        NextNightmare = 0;
                        if (NightmareSeeds.Count > 0) NextNightmare = NightmareSeeds[0].SeedTime;
                    }
                } catch (Exception e)
                {
                    API.Log("Exception during MainLoop: " + e.Message);
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
                int timeGap = 6000 / numberOfNightmares;
                API.Log("Timegap: " + timeGap + " (" + (timeGap / 5.8f) + "s)");
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

                NextNightmareReseed = currentTime + 6000;
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
        
        //Chat-triggered override spawner
        public static void SpawnNightmareOnPlayer(ClientInfo _cInfo)
        {
            if (NightmareSeeds.Count < 1)
            {
                WhisperMessage(_cInfo, "No nightmares are ready to spawn...");
                return;
            }
            try
            {
                var ns = NightmareSeeds[0];
                foreach (NightmareType nt in ns.NightmareTypes)
                {
                    SpawnNightmare(nt);
                }
                NightmareSeeds.RemoveAt(0);
                NextNightmare = 0;
                if (NightmareSeeds.Count > 0) NextNightmare = NightmareSeeds[0].SeedTime;
            } catch (Exception e)
            {
                API.Log("Failed to SpawnNightmareOnPlayer:" + e.Message);
            }
        }

        //Spawns a nightmare
        public static void SpawnNightmare(NightmareType nt, EntityAlive target = null)
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


                        if (target == null)
                        {
                            API.Log("Finding target");
                            ni.PickRandomTarget();
                        } else
                        {
                            ni.TargetEntity = target;
                        }

                        if (ni.TargetEntity == null)
                        {
                            API.Log("Failed to find a target entity");
                            return;
                        }
                        API.Log("Finding location to spawn");                        


                        bool isSpawnLocFound = false;
                        for (int i = 0; i < 10; i++)
                        {
                            if (GameManager.Instance.World.FindRandomSpawnPointNearPlayer(ni.TargetEntity, 30, out x, out y, out z, 200))
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
                        //This is found in AIDirectoryBloodMoonParty
                        ni.Instance = (EntityEnemy)EntityFactory.CreateEntity(current3, pos);
                        ni.Instance.isFeral = true;
                        
                        //ni.Instance.lootContainer.AddItem(new ItemStack(new ItemValue(114), 1));
                        //string loot = "Loot: ";
                        //foreach (var item in ni.Instance.equipment.GetItems())
                        //{
                         //   loot += string.Format("{0} [{1}] x{2}", item.ItemClass, item.Quality);
                        //}
                        //API.Log(string.Format("Loot: {0}", loot));

                        GameManager.Instance.World.SpawnEntityInWorld(ni.Instance);


                        //ni.Instance.SetEntityName("A nightmare");
                        //ni.Instance.SetSpawnerSource(EnumSpawnerSource.Dynamic);
                        NightmareInstances.Add(ni);
                        ni.Instance.SetAttackTarget(ni.TargetEntity, 1200);
                        API.Log("Added instance " + ni.InstanceType.ClassName + " at " + x + ", " + y + ", " + z);

                        WhisperMessage(ConnectionManager.Instance.GetClientInfoForPlayerId(ni.TargetEntity.belongsPlayerId.ToString()), ni.InstanceType.Message);                        
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
            try
            {
                if (_cInfo == null)
                {
                    API.Log("Failed to send whisper message: _cInfo is null (message: " + message);
                    return;
                }
                if (color == "") color = "[FF0000]";

                string Nickname = "";
                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", color, message), Nickname, false, "", false));
            }
            catch (Exception e)
            {
                API.Log("Failed to WhisperMessage: " + e.Message);
            }
        }
        
        private static int CurrentDay()
        {            
            return GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        }       
    }
}