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

        // 역할: 룸 기본 상태를 초기화한다.
        public Room(int roomId, int userA, int userB)
        {
            RoomId = roomId;
            PlayerAId = userA;
            PlayerBId = userB;
            State = RoomState.Waiting;
        }

        // 역할: 룸을 시작 상태로 전환한다.
        public void Start()
        {
            State = RoomState.Playing;
            CurrentTurnUserId = PlayerAId;
        }

        // 역할: 착수 가능 여부를 검증하고 돌을 놓는다.
        public bool TryPlace(int userId, uint x, uint y, out Stone placedStone, out string? rejectReason)
        {
            placedStone = Stone.Empty;
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

            placedStone = userId == PlayerAId ? Stone.Black : Stone.White;
            _board[x, y] = placedStone;
            CurrentTurnUserId = GetOpponentId(userId);
            return true;
        }

        // 역할: 상대 플레이어 ID를 반환한다.
        public int GetOpponentId(int userId)
        {
            return userId == PlayerAId ? PlayerBId : PlayerAId;
        }

        // 역할: 룸 참여자 ID를 열거한다.
        public IEnumerable<int> GetPlayers()
        {
            yield return PlayerAId;
            yield return PlayerBId;
        }
    }
}
