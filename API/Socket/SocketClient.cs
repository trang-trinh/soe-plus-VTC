using Helper;
using Newtonsoft.Json;
using SocketIOClient;
using System;
using System.Collections.Generic;

namespace Client
{
    public class SocketClient
    {
        public static async void onConnect(string url = "https://socket2.soe.vn/")
        {
            SocketIO _socket = new SocketIO(url, new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = 10,
                ReconnectionDelay = 1000,
                Auth = new Dictionary<string, string>
                {
                    { "user_id", "chatbot" },
                    { "full_name", "Chat Bot hệ thống" },
                }
            });
            try
            {
                await _socket.ConnectAsync();
                helper.socketClient = _socket;
            }
            catch(Exception e) {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog("chatbot", "chatbot", JsonConvert.SerializeObject(new { contents, _socket }), "", "", "", "Lỗi connect socket", 0, "socket");
            };
        }
    }
}