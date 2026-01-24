using System;

namespace Protocol
{
    public struct PacketHeader
    {
        public uint type; // network order
        public uint len;  // network order
    }

    public struct IP_Port
    {
        public string ip;
        public int port;
    }

    public enum PacketType : uint
    {
        None = 0,                           // (디버그/초기값)

        // -----------------------------
        // Client -> Server (C2S) : 1000~
        // -----------------------------
        C2S_Hello = 1000,                   // 최초 접속 핸드셰이크 요청 ("HELLO" 등)
        C2S_PlaceStoneRequest = 1001,       // 돌 두기 요청 (x,y)
        C2S_Ping = 1002,                    // 통신 확인 요청(핑) (옵션: clientTick)
        C2S_ChatMessage = 1003,             // 채팅 메시지 전송 (len + utf8 bytes)
        C2S_MatchRequest = 1010,            // 매칭 요청
        C2S_EndGame = 1011,                 // 게임 종료 알림

        //C2S_MatchRequest = 1010,            // 매칭 시작(대기열 들어가기)
        //C2S_MatchCancel = 1011,             // 매칭 취소(대기열 나오기)
        //C2S_Ready = 1012,                   // 준비 완료(룸 입장 후)
        //C2S_ResyncRequest = 1013,           // 상태 재동기화 요청(누락/재접속 대비)
        //C2S_Surrender = 1014,               // 기권
        //C2S_Pong = 1015,                    // (옵션) 서버 Ping에 대한 응답을 분리하고 싶을 때

        // -----------------------------
        // Server -> Client (S2C) : 2000~
        // -----------------------------
        S2C_Welcome = 2000,                 // 핸드셰이크 성공 응답 ("WELCOME" 등)
        //S2C_HandshakeNack = 2001,           // (옵션) 핸드셰이크 실패/거부 (reasonCode)
        S2C_PlaceStoneAck = 2002,           // 돌 두기 승인 (x,y,stone)
        //S2C_PlaceStoneNack = 2003,          // 돌 두기 거부 (reasonCode)
        S2C_ChatMessage = 2004,             // 채팅 전달 (senderId + len + utf8 bytes)
        S2C_Error = 2005,                   // 오류 응답 (msg)
        S2C_MatchFound = 2010,              // 매칭 완료 (roomId + myColor + isMyTurn)
        //S2C_StateSnapshot = 2005,           // 상태 스냅샷 (state + roomId + myColor + turnColor + ...)

        //S2C_MatchFound = 2010,              // 상대 찾음/룸 생성됨 (roomId + myColor 등)
        //S2C_GameStart = 2011,               // 게임 시작 (선공/색/초기턴 등)
        //S2C_BoardUpdate = 2012,             // 보드 업데이트 (lastMove or delta or fullSync)
        //S2C_TurnChange = 2013,              // 턴 변경 통지 (옵션: BoardUpdate에 합칠 수 있음)
        //S2C_GameOver = 2014,                // 게임 종료 (winner + reason)
        //S2C_OpponentDisconnected = 2015,    // 상대 연결 끊김 알림
        //S2C_Ping = 2016,                    // 서버 keepalive 핑 (옵션: serverTick)
        //S2C_Error = 2017,                   // 공통 오류 응답 (errorCode + msg(optional))
    }

    public static class ProtocolHelper
    {
        public const uint MAX_PAYLOAD = 1024u * 1024u; // 1MB
    }
}
