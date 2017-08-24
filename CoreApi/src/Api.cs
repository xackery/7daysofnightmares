using System.IO;

namespace CoreApi
{
    public class API : ModApiAbstract
    {
        public static string ChatColor = "[00FF00]";

        //When game wakes up
        public override void GameAwake()
        {
            Nightmare.Load();
        }

        //When a player logs into game
        public override void PlayerLogin(ClientInfo _cInfo, string _compatibilityVersion)
        {
                        
        }

        //When a player spawning into game
        public override void PlayerSpawning(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
        }

        //Player is now spawned into world
        public override void PlayerSpawnedInWorld(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos)
        {            
        }

        //Chat hook system
        /*public override bool ChatMessage(ClientInfo _cInfo, EnumGameMessages _type, string _message, string _playerName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
        {
            if (_message.ToLower().Equals("nightmare")) {
                Nightmare.TrySpawningNightmare();
                return false;
            }
            return true;
        }*/

        //When a player disconnects
        public override void PlayerDisconnected(ClientInfo _cInfo, bool _bShutdown)
        {
            if (Nightmare.AreNightmaresHappening)
            {
                //Todo: Figure out a way to punish or switch targets on D/c ?
            }
        }

        //When game shuts down
        public override void GameShutdown()
        {
            Nightmare.Unload();
        }
    }
}