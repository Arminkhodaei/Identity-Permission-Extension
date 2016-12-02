using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;

namespace IdentityPermissionExtension
{
    /// <summary>Extension methods for OwinContext</summary>
    public static class OwinContextExtensions
    {
        private static readonly string IdentityPermissionKeyPrefix = "IdentityPermissionExtension:";

        private static IDictionary<Type, object> _permissionEnvironment = new ConcurrentDictionary<Type, object>();

        private static string GetKey(Type t)
        {
            return OwinContextExtensions.IdentityPermissionKeyPrefix + t.AssemblyQualifiedName;
        }

        private static IDictionary<Type, object> GetEnvirenment()
        {
            return _permissionEnvironment;
        }

        private static IDictionary<Type, object> SetEnvirenment(Type type, object value)
        {
            if (_permissionEnvironment.ContainsKey(type))
                return _permissionEnvironment;
            _permissionEnvironment.Add(type, value);
            return _permissionEnvironment;
        }

        /// <summary>
        ///     Stores an object in the OwinContext using a key based on the AssemblyQualified type name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetPermissionObject<T>(this IOwinContext context, T value)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            SetPermissionEnvironment(context, value);
        }

        public static void SetPermissionEnvironment<T>(this IOwinContext context, T value)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (value == null || GetEnvirenment().ContainsKey(typeof(T)))
                return;
            SetEnvirenment(typeof(T), value);
        }

        /// <summary>
        ///     Retrieves an object from the OwinContext using a key based on the AssemblyQualified type name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetPermissionObject<T>(this IOwinContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            var dicEntry = GetEnvirenment().FirstOrDefault(x => x.Key == typeof(T));
            if (dicEntry.Value != null)
                return (T)dicEntry.Value;
            dicEntry =
                GetEnvirenment()
                    .FirstOrDefault(
                        x => typeof(T).IsAssignableFrom(x.Key) || x.Key.GetInterfaces().Any(i => i == typeof(T)));
            if (dicEntry.Value != null)
                return (T)dicEntry.Value;

            return default(T);
        }

        /// <summary>Get the permission manager from the context</summary>
        /// <typeparam name="TManager"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TManager GetPermissionManager<TManager>(this IOwinContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return context.GetPermissionObject<TManager>();
        }
    }
}
