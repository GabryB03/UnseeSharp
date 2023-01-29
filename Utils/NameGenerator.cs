public class NameGenerator
{
    private static char[] _characters = "ABCDEFGHIJKLMNOQPRSTUVWXYZ0123456789".ToCharArray();
    private static ProtoRandom.ProtoRandom _random = new ProtoRandom.ProtoRandom(1);

    public static string GenerateName()
    {
        return _random.GetRandomString(_characters, 12);
    }
}