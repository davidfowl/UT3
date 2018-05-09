using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UTT
{
    public class User
    {
        public string Name { get; set; }

        private static ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();

        public static int Count;

        public static void AddUser(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return;

            }
            _users.TryAdd(user, new User
            {
                Name = user
            });
        }

        public static void Remove(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return;
            }

            _users.TryRemove(user, out _);
        }

        public static IEnumerable<User> GetUsers() => _users.Values;
    }
}