using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace ServerApp
{
    public enum UserState
    {
        Connected,
        Matching,
        InRoom,
        Disconnected
    }

    public class SessionInfo
    {
        public int UserId;
        public ClientSession Session;
        public Thread WorkerThread;
        public UserState State;
        public int? RoomId;
    }

    public class UserManager
    {
        private readonly Dictionary<int, SessionInfo> _users = new();
        private readonly object _lock = new();
        private int _nextUserId = 1;

        public int RegisterUser(ClientSession session, Thread workerThread)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (workerThread == null) throw new ArgumentNullException(nameof(workerThread));

            lock (_lock)
            {
                int userId = _nextUserId++;
                _users[userId] = new SessionInfo
                {
                    UserId = userId,
                    Session = session,
                    WorkerThread = workerThread,
                    State = UserState.Connected,
                    RoomId = null
                };
                return userId;
            }
        }

        public bool TryGetUser(int userId, out SessionInfo info)
        {
            lock (_lock)
            {
                return _users.TryGetValue(userId, out info);
            }
        }

        public bool TryGetSession(int userId, out ClientSession session)
        {
            session = null;
            lock (_lock)
            {
                if (_users.TryGetValue(userId, out SessionInfo info))
                {
                    session = info.Session;
                    return true;
                }
            }

            return false;
        }

        public void SetState(int userId, UserState state)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(userId, out SessionInfo info))
                {
                    info.State = state;
                }
            }
        }

        public void SetRoom(int userId, int? roomId)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(userId, out SessionInfo info))
                {
                    info.RoomId = roomId;
                }
            }
        }

        public void OnDisconnect(int userId)
        {
            lock (_lock)
            {
                _users.Remove(userId);
            }
        }

        public void CloseAll()
        {
            List<SessionInfo> sessions;
            lock (_lock)
            {
                sessions = new List<SessionInfo>(_users.Values);
                _users.Clear();
            }

            foreach (var info in sessions)
            {
                SafeCloseSession(info.Session);
            }

            foreach (var info in sessions)
            {
                JoinThread(info.WorkerThread);
            }
        }

        private static void JoinThread(Thread thread)
        {
            if (thread == null)
            {
                return;
            }

            if (thread.IsAlive)
            {
                thread.Join(500);
            }
        }

        private static void SafeCloseSession(ClientSession session)
        {
            if (session?.Socket == null)
            {
                return;
            }

            try
            {
                session.Socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                session.Socket.Close();
            }
            catch
            {
            }

            try
            {
                session.Socket.Dispose();
            }
            catch
            {
            }
        }
    }
}
