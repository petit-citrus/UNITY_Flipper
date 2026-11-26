
public static class LevelSession
{
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetForNewPlaySession()
    {
        HasPendingData = false;
        Score = 0;
        Lives = 0;
        Level = 0;
    }
    public static bool HasPendingData { get; private set; }

    public static int Score { get; private set; }
    public static int Lives { get; private set; }
    public static int Level { get; private set; }

    // Enregistre les données avant de changer de scène.
    public static void Save(int score, int lives, int level)
    {
        Score = score;
        Lives = lives;
        Level = level;

        HasPendingData = true;
    }

    // Indique que les données ont été restaurées.
    public static void Clear()
    {
        HasPendingData = false;
    }
}
