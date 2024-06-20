using System.Linq;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class IdGenerator
    {
        public static long Generate(int length = 10)
        {
            return long.Parse(string.Join("", new int[10].Select(_ => Random.Range(0, 10))));
        }
    }
}