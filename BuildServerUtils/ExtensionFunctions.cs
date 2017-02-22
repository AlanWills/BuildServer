using System.Text;

namespace BuildServerUtils
{
    public static class ExtensionFunctions
    {
        public static string ConvertToString(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
    }
}
