using System.Text;

namespace Scheduler.Common.Helpers
{
    public static class StringHelper
    {
        public static string ToBase64(this string str)
        {
            byte[] base64 = Encoding.Default.GetBytes(str);
            return Convert.ToBase64String(base64);
        }
    }
}
