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

    public enum RoomCloseReason
    {
        Win = 1,
        Disconnect = 2,
        Requested = 3
    }

    public class Room
    {
        private readonly Stone[,] _board = new Stone[19, 19];
        private const int BoardSize = 19;
        private const int WinCount = 5;

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
        public bool TryPlace(int userId, uint x, uint y, out Stone placedStone, out bool isWin, out string? rejectReason)
        {
            placedStone = Stone.Empty;
            isWin = false;
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

            int boardX = (int)x;
            int boardY = (int)y;

            if (_board[boardX, boardY] != Stone.Empty)
            {
                rejectReason = "already occupied";
                return false;
            }

            placedStone = userId == PlayerAId ? Stone.Black : Stone.White;
            _board[boardX, boardY] = placedStone;
            isWin = HasFiveInRow(boardX, boardY, placedStone);
            if (isWin)
            {
                State = RoomState.Finished;
            }
            CurrentTurnUserId = GetOpponentId(userId);
            return true;
        }

        private bool HasFiveInRow(int x, int y, Stone stone)
        {
            return CountLine(x, y, 1, 0, stone) >= WinCount
                || CountLine(x, y, 0, 1, stone) >= WinCount
                || CountLine(x, y, 1, 1, stone) >= WinCount
                || CountLine(x, y, 1, -1, stone) >= WinCount;
        }

        private int CountLine(int x, int y, int dx, int dy, Stone stone)
        {
            return 1 + CountDirection(x, y, dx, dy, stone) + CountDirection(x, y, -dx, -dy, stone);
        }

        private int CountDirection(int x, int y, int dx, int dy, Stone stone)
        {
            int count = 0;
            int currentX = x + dx;
            int currentY = y + dy;

            while (currentX >= 0 && currentX < BoardSize && currentY >= 0 && currentY < BoardSize && _board[currentX, currentY] == stone)
            {
                count++;
                currentX += dx;
                currentY += dy;
            }

            return count;
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
