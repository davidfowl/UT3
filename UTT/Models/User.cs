using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace UTT;

public class User
{
    public string Name { get; set; }

    private static ConcurrentDictionary<string, User> _users = new ConcurrentDictionary<string, User>();

    private static int _count;

    public static int Count => _count;

    public static void AddUser(string user)
    {
        if (string.IsNullOrEmpty(user))
        {
            return;

        }

        if (_users.TryAdd(user, new User
        {
            Name = user
        }))
        {
            Interlocked.Increment(ref _count);
        }
    }

    public static void Remove(string user)
    {
        if (string.IsNullOrEmpty(user))
        {
            return;
        }

        if (_users.TryRemove(user, out _))
        {
            Interlocked.Decrement(ref _count);
        }
    }

    public static IEnumerable<User> GetUsers() => _users.Values;
}
