namespace CoreApi
{
    public class Mods
    {
        public static void Load()
        {
            if (Nightmare.IsRunning)
            {
                Nightmare.Unload();
            }
            if (!Nightmare.IsRunning)
            {
                Nightmare.Load();
            }
        }
    }
}