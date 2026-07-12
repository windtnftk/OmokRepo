using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script
{
    public enum PlaceSucces { None, Continue , Win, Draw }
    public enum Stone { None, Black, White }
    public class BoardLogic
    {
        private const int BOARD_SIZE = 19;
        private const int Win_Count = 5;
        (int dx, int dy)[] dirs = { (1, 0), (0, 1), (1, 1), (1, -1) };

        private Stone[] board = new Stone[BOARD_SIZE * BOARD_SIZE];
        
        private Stone GetStone(int x, int y) // 함수 호출시에는 inRange 체크 필요
        {
            return board[x + y * BOARD_SIZE];
        }
        private void SetStone(int x, int y, Stone stone) // 함수 호출시에는 inRange 체크 필요
        {
            board[x + y * BOARD_SIZE] = stone;
        }
        private PlaceSucces CheckWin(int x, int y, Stone stone) // 승리 조건 체크
        {
            foreach(var (dx,dy) in dirs)
            { 
                int count = 1 + Run(x, y, dx, dy, stone) + Run(x, y, -dx, -dy, stone);
                if (count >= Win_Count) return PlaceSucces.Win;
            }
            return PlaceSucces.Continue; // 임시 반환값
        }
        private int Run(int x, int y, int dx, int dy, Stone stone)
        {
            int count = 0;
            int Cx = x + dx;
            int Cy = y + dy;
            while(IsInBounds(Cx,Cy)&& GetStone(Cx,Cy) == stone && count < Win_Count - 1) 
            {
                Cx += dx;
                Cy += dy;
                ++count;
            }
            return count;
        }
        public bool IsInBounds(int x, int y) // 좌표가 보드 범위 내에 있는지 확인
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }
        public PlaceSucces PlaceStone(int x, int y, Stone stone) // 돌을 놓을 수 있는지 확인
        {
            // 좌표가 보드 범위 밖인 경우 && 이미 돌이 놓여있는 경우
            if (!IsInBounds(x, y) || GetStone(x, y) != Stone.None) 
            {
                return PlaceSucces.None;
            }
           // 돌을 놓을 수 있는 경우
            
                SetStone(x, y, stone);
                return CheckWin(x, y, stone);
            
        }

        public void Reset()
        {
            board = new Stone[BOARD_SIZE * BOARD_SIZE];
        }
    }
}
