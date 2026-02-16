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

        // 역할: 새 유저를 등록하고 사용자 ID를 반환한다.
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

        // 역할: 사용자 정보를 조회한다.
        public bool TryGetUser(int userId, out SessionInfo info)
        {
            lock (_lock)
            {
                return _users.TryGetValue(userId, out info);
            }
        }

        // 역할: 사용자 세션을 조회한다.
        public bool TryGetSession(int userId, out ClientSession session)
        {
            session = null;
            lock (_lock)
            {
                if (_users.TryGetValue(userId, out SessionInfo sessionInfo))
                {
                    session = sessionInfo.Session;
                    return true;
                }
            }

            return false;
        }

        public List<SessionInfo> GetSessionSnapshot()
        {
            lock (_lock)
            {
                return new List<SessionInfo>(_users.Values);
            }
        }

        // 역할: 사용자 상태를 변경한다.
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

        // 역할: 사용자의 룸 정보를 갱신한다.
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

        // 역할: 연결 종료된 사용자를 제거한다.
        public void OnDisconnect(int userId)
        {
            lock (_lock)
            {
                _users.Remove(userId);
            }
        }

        // 역할: 모든 세션을 정리하고 스레드를 종료 대기한다.
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

        // 역할: 스레드를 제한 시간 내에 종료 대기한다.
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

        // 역할: 세션 소켓을 안전하게 종료한다.
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
