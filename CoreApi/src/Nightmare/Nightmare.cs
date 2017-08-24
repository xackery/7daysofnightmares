using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//animalBear
//animalBoar
//animalChicken
//animalDireWolf
//animalRabbit
//animalSnake
//animalStag
//animalWolf
//animalZombieBear
//animalZombieDog
//animalZombieVulture
//Backpack
//DroppedLootContainer
//EvisceratedRemains
//fallingBlock
//fallingTree
//invisibleAnimal
//item
//minibike
//npcTraderBob
//npcTraderHugh
//npcTraderJimmy
//npcTraderJoel
//npcTraderRekt
//playerFemale
//playerMale
//sc_General
//zombieArlene
//zombieArleneFeral
//zombieBiker
//zombieBoe
//zombieBoeFeral
//zombieBurnt
//zombieBusinessMan
//zombieBusinessManFeral
//zombieCheerleader
//zombieCheerleaderFeral
//zombieDarlene
//zombieDarleneFeral
//zombieFarmer
//zombieFarmerFeral
//zombieFatCop
//zombieFatCopFeral
//zombieFatCopFeralRadiated
//zombieFatHawaiian
//zombieFatHawaiianFeral
//zombieFemaleFat
//zombieFemaleFatFeral
//zombieFootballPlayer
//zombieJoe
//zombieJoeFeral
//zombieMaleHazmat
//zombieMarlene
//zombieMarleneFeral
//zombieMoe
//zombieMoeFeral
//zombieNurse
//zombieNurseFeral
//zombieOldTimer
//zombieOldTimerFeral
//zombieScreamer
//zombieScreamerFeral
//zombieSkateboarder
//zombieSkateboarderFeral
//zombieSnow
//zombieSnowFeral
//zombieSoldier
//zombieSoldierFeral
//zombieSpider
//zombieSpiderFeral
//zombieSpiderFeralRadiated
//zombieSteve
//zombieSteveCrawler
//zombieSteveCrawlerFeral
//zombieSteveFeral
//zombieStripper
//zombieStripperFeral
//zombieUtilityWorker
//zombieUtilityWorkerFeral
//zombieWightFeral
//zombieWightFeralRadiated
//zombieYo
//zombieYoFeral
namespace CoreApi
{
    class NightmareType
    {
        public string Message;
        public string ClassName;
        public int MinimumDay;
        public int Weight;        
    }

    class NightmareInstance
    {
        public EntityEnemy Instance;
        public NightmareType InstanceType;

        public EntityAlive TargetEntity;
        public ClientInfo TargetClient;

        public void PickRandomTarget()
        {
            Random rnd = new Random();
            List<ClientInfo> _cInfoList = ConnectionManager.Instance.GetClients();
            TargetClient = _cInfoList[rnd.Next(0, _cInfoList.Count())];
            TargetEntity = (EntityAlive)GameManager.Instance.World.GetEntity(TargetClient.entityId);
            Nightmare.Log("Targetting " + TargetClient.playerName);
            //Instance.SetInvestigatePosition(TargetEntity.position, 2400);
            Instance.SetAttackTarget(TargetEntity, 2400);
        }
    }

    class Nightmare
    {
        //_cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("{0}{1}[-]", API.ChatColor, _phrase200), "Server", false, "", false));
        public static bool IsRunning = false;
        private static Thread th;

        //Has a new hour happened?
        public static int LastHourCheck = 0;
        //Has a nightmare happened this hour yet?
        public static bool HasANightmareHappenedThisHour;
        //Track how many nightmares have happened in the day so far.
        public static int NightmareDayCounter = 0;
        public static bool AreNightmaresHappening
        {
            get {
                return (NightmareInstances.Count() > 0);
            }
            set { }
        }
        private static List<NightmareInstance> NightmareInstances = new List<NightmareInstance>();
        private static List<NightmareType> NightmareTypes = new List<NightmareType>();

        public static void Load()
        {
            Start();
            IsRunning = true;
        }
        
        private static void Start()
        {
            NightmareType n = new NightmareType();
            n.Message = "A nightmare manifests itself";
            n.MinimumDay = 0;
            n.Weight = 10;
            n.ClassName = "zombieSpiderFeral";
            NightmareTypes.Add(n);

            n = new NightmareType();
            n.Message = "A nightmare manifests itself";
            n.Weight = 50;
            n.MinimumDay = 0;
            n.ClassName = "animalZombieDog";
            NightmareTypes.Add(n);

            n = new NightmareType();
            n.Message = "A nightmare manifests itself";
            n.Weight = 10;
            n.MinimumDay = 0;
            n.ClassName = "animalWolf";
            NightmareTypes.Add(n);


            n = new NightmareType();
            n.Message = "A nightmare manifests itself";
            n.Weight = 10;
            n.MinimumDay = 2;
            n.ClassName = "zombieSkateboarderFeral";
            NightmareTypes.Add(n);


            n = new NightmareType();
            n.Message = "A nightmare manifests itself";
            n.Weight = 10;
            n.MinimumDay = 2;
            n.ClassName = "zombieArleneFeral";
            NightmareTypes.Add(n);
            
            Log("Nightmare: Added " + NightmareTypes.Count + " nightmares.");

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
                Thread.Sleep(6000);
                Log("Tick" + NightmareDayCounter);
                //if (!IsRunning) return;

                if (ConnectionManager.Instance.ClientCount() < 1) continue;

                //AI LOGIC/CLEANUP
                for (int i = NightmareInstances.Count - 1; i > 0; i--)
                {
                    //Remove dead/null instances
                    if (NightmareInstances[i].Instance == null || NightmareInstances[i].Instance.IsDead())
                    {                        
                        NightmareInstances.RemoveAt(i);
                        Log("Removed nightmare instance " + i + ", " + NightmareInstances.Count + " left");
                        continue;
                    }

                    //Ensure we got a target.
                    if (NightmareInstances[i].TargetEntity == null || NightmareInstances[i].TargetEntity.IsDead())
                    {
                        Log(i + " is picking a new random player target");
                        NightmareInstances[i].PickRandomTarget();
                    }
                }

                if (!HasANightmareHappenedThisHour) TrySpawningNightmare();

                // HOURLY CHECK DICE
                if (LastHourCheck == CurrentHour()) continue;

                Log("Hour changed from " + LastHourCheck + " to " + CurrentHour());
                if (LastHourCheck > CurrentHour())
                {
                    Log("New Day, NightmareDayCounter reset, there was " + NightmareDayCounter + " total spawned nightmares yesterday.");
                    NightmareDayCounter = 0;
                }
                LastHourCheck = CurrentHour();
                HasANightmareHappenedThisHour = false;
            }
        }

        public static void TrySpawningNightmare()
        {

            //See if there's already a prominent number of nightmares.
            int playerCount = ConnectionManager.Instance.GetClients().Count;

            int maxNightmares = playerCount + (CurrentDay() / 2);
            if (maxNightmares < playerCount) maxNightmares = playerCount;

            //We've met daily quota
            if (maxNightmares <= NightmareDayCounter)
            {
                Log("Daily quota met: " + maxNightmares);
                return;
            }

            Random rnd = new Random();
            int roll = rnd.Next(0, 31);
            Log("Rolled dice: "+roll);
            if (roll < 2) {
                SpawnNightmare();
            }
        }

        //Spawns a nightmare
        public static void SpawnNightmare()
        {
            Random rnd = new Random();
            int poolSize = 0;

            Dictionary<int, NightmareType> rngPool = new Dictionary<int, NightmareType>();

            Log("Spawning Nightmare...");
            if (NightmareTypes.Count() == 0)
            {
                Log("No nightmare entries found");
                return;
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
                Log("No rngPool found");
                return;
            }


            int result = rnd.Next(0, poolSize);
            Log("Rolled "+result+" of "+poolSize);

            bool isFound = false;

            NightmareInstance ni = new NightmareInstance();
            foreach (KeyValuePair<int, NightmareType> entry in rngPool)
            {
                Log(entry.Key + ", " + result);
                if (entry.Key < result) continue;                
                ni.InstanceType = entry.Value;
                isFound = true;
            }

            if (!isFound)
            {
                Log("Did not find RNG entry, falling back to 0");
                ni.InstanceType = NightmareTypes[0];
            }
            Log("Spawning instance "+ ni.InstanceType.ClassName);

            
            using (Dictionary<int, EntityClass>.KeyCollection.Enumerator enumerator3 = EntityClass.list.Keys.GetEnumerator())
            {
                while (enumerator3.MoveNext())
                {
                    //Find entity class
                    int current3 = enumerator3.Current;
                    if (!EntityClass.list[current3].entityClassName.Equals(ni.InstanceType.ClassName)) continue;
                    //Log("Found entity " + current3 + " to be classname " + CurrentNightmare.ClassName);
                    //Find a place to spawn it
                    int x, y, z;
                    
                    if (!GameManager.Instance.World.FindRandomSpawnPointNearPlayer(ni.TargetEntity, 15, out x, out y, out z, 50)) continue;                    
                    //spawn it
                    UnityEngine.Vector3 pos = new UnityEngine.Vector3(x,y,z);

                    ni.Instance = (EntityEnemy)EntityFactory.CreateEntity(current3, pos);

                    //EntityClass.list.Add()
                    //CurrentNightmare.TargetEntity.PlayOneShot()
                    //CurrentNightmare.NightmareEntity.SetVelocity();
                    ni.Instance.SetEntityName("A nightmare");
                    ni.Instance.MovementRunning = true;
                    ni.Instance.isFeral = true;
                    GameManager.Instance.World.SpawnEntityInWorld(ni.Instance);
                    
                    BroadcastMessage(ni.InstanceType.Message);
                    //EntityClass.list[current3].ExperienceValue
                    NightmareInstances.Add(ni);
                    ni.PickRandomTarget();
                    Log("Added instance " + ni.InstanceType.ClassName+ " at "+ x + ", " + y + ", " + z);
                    return;
                }
            }
            Log("Failed to find class type of nightmare: " + ni.InstanceType.ClassName);
        }

        public static void Log(string message)
        {            
            UnityEngine.Debug.Log("NightmareMod: "+message);
            //BroadcastMessage(message);
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