public class GameStringProvider : ChvjUnityInfra.IStringProvider
{
    public string GetString(int stringID) => CHMString.Instance.GetString(stringID);
}
