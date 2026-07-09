using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SportsStore.Infrastructure {

    /// <summary>
    /// Extension Method ها برای کار با Session
    /// امکان ذخیره و بازیابی اشیاء JSON در Session را فراهم می‌کند.
    /// </summary>
    public static class SessionExtensions {

        /// <summary>
        /// ذخیره یک شیء به صورت JSON در Session
        /// </summary>
        public static void SetJson(this ISession session, string key, object value) {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        /// <summary>
        /// بازیابی یک شیء از Session و Deserialize آن
        /// </summary>
        public static T? GetJson<T>(this ISession session, string key) {
            string? sessionData = session.GetString(key);
            return sessionData == null
                ? default(T)
                : JsonSerializer.Deserialize<T>(sessionData);
        }
    }
}
