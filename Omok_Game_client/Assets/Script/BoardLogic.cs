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
            // 승리 조건 로직 구현 (예: 5목 체크)
            return PlaceSucces.Continue; // 임시 반환값
        }
        public bool IsInBounds(int x, int y) // 좌표가 보드 범위 내에 있는지 확인
        {
            return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
        }
        public bool PlaceStone(int x, int y, Stone stone, out PlaceSucces Check) // 돌을 놓을 수 있는지 확인
        {
            if (!IsInBounds(x, y)) // 좌표가 보드 범위 밖인 경우
            {
                Check = PlaceSucces.None;
                return false;
            }

            else if (GetStone(x, y) != Stone.None) // 이미 돌이 놓여있는 경우
            {
                Check = PlaceSucces.None;
                return false;
            }

            else // 돌을 놓을 수 있는 경우
            {
                SetStone(x, y, stone);
                Check = CheckWin(x, y, stone);
                return true;
            }
        }
    }
}
