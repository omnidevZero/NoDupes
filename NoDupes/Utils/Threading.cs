using System.Threading;

namespace NoDupes.Utils
{
    public class Threading
    {
        public static CancellationTokenSource CancellationTokenSource { get; set; }
        public static CancellationToken CancellationToken { get; set; }
    }
}
