using System.Collections.Generic;

namespace ServerApp
{
    public enum RoomState
    {
        Waiting,
        Playing,
        Finished
    }

    public enum Stone
    {
        Empty = 0,
        Black = 1,
        White = 2
    }

    public class Room
    {
        private readonly Stone[,] _board = new Stone[19, 19];

        public int RoomId { get; }
        public int PlayerAId { get; }
        public int PlayerBId { get; }
        public int CurrentTurnUserId { get; private set; }
        public RoomState State { get; private set; }

        public Room(int roomId, int userA, int userB)
        {
            RoomId = roomId;
            PlayerAId = userA;
            PlayerBId = userB;
            State = RoomState.Waiting;
        }

        public void Start()
        {
            State = RoomState.Playing;
            CurrentTurnUserId = PlayerAId;
        }

        public bool TryPlace(int userId, uint x, uint y, out string rejectReason)
        {
            rejectReason = null;

            if (State != RoomState.Playing)
            {
                rejectReason = "room not ready";
                return false;
            }

            if (x > 18 || y > 18)
            {
                rejectReason = "out of range";
                return false;
            }

            if (userId != CurrentTurnUserId)
            {
                rejectReason = "not your turn";
                return false;
            }

            if (_board[x, y] != Stone.Empty)
            {
                rejectReason = "already occupied";
                return false;
            }

            _board[x, y] = userId == PlayerAId ? Stone.Black : Stone.White;
            CurrentTurnUserId = GetOpponentId(userId);
            return true;
        }

        public int GetOpponentId(int userId)
        {
            return userId == PlayerAId ? PlayerBId : PlayerAId;
        }

        public IEnumerable<int> GetPlayers()
        {
            yield return PlayerAId;
            yield return PlayerBId;
        }
    }
}
