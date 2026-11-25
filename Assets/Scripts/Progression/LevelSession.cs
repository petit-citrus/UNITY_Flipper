
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

    // Guarda os dados antes de mudar de cena
    public static void Save(int score, int lives, int level)
    {
        Score = score;
        Lives = lives;
        Level = level;

        HasPendingData = true;
    }

    // Indica que os dados foram restaurados
    public static void Clear()
    {
        HasPendingData = false;
    }
}
