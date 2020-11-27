﻿using Baskin_Kiosk.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Baskin_Kiosk.Network
{
    public class ServerConnection
    {
        private bool isSend = false;
        private const int MAX_LEN = 4096;

        private byte[] sendData = new byte[MAX_LEN];
        private byte[] receiveData = new byte[MAX_LEN];
        private byte[] response = new byte[MAX_LEN];
        private Thread networkThread = null;

        public bool isConnected = false;

        TcpClient client = null;
        NetworkStream networkStream = null;

        private void messageSettings()
        {
            if (this.client == null)
            {
                this.client = new TcpClient(Constants.SERVER_ADDRESS, Constants.SERVER_PORT); // (ip주소 , 포트 번호)
                networkStream = this.client.GetStream();
            }

            networkStream.Write(this.sendData, 0, this.sendData.Length);
        }

        private string getResponse()
        {
            Int32 readData = networkStream.Read(this.response, 0, this.response.Length);
            String response = Encoding.UTF8.GetString(this.response, 0, readData);
            return response;
        }
        
        public string sendMessage()
        {
            MsgPacket packet = new MsgPacket
            {
                MSGType = "0",
                Id = "2205"
            };

            String JsonStr = JsonConvert.SerializeObject(packet);
            this.sendData = Encoding.UTF8.GetBytes(JsonStr);

            try
            {
                messageSettings();
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                this.isConnected = true;
                return getResponse();
            }
            catch (Exception ex)
            {
                this.isConnected = false;
                MessageBoxResult confirm = MessageBox.Show("서버가 꺼진상태로 로그인 하시겠습니까?", "잠시만요", MessageBoxButton.YesNo);

                if (confirm == MessageBoxResult.Yes)
                {
                    return "1";
                } else
                {
                    return "2";
                }
            }
        }

        public async void receiveMessage()
        {
            try
            {
                NetworkStream receiveStream = client.GetStream();
                int readData = 0;
                string response = "";

                while (true)
                {
                    try
                    {
                        isSend = false;
                        readData = await receiveStream.ReadAsync(receiveData, 0, receiveData.Length);
                        response = Encoding.UTF8.GetString(receiveData, 0, readData);

                        if (!isSend)
                        {
                            if (response.Length > 0)
                            {
                                MessageBox.Show(response);
                            } else
                            {
                                this.isConnected = false;
                                MessageBox.Show("서버와 연결이 종료되었습니다.");
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("서버와 연결이 닫혔습니다.");
                        this.isConnected = false;
                        break;
                    }
                }
            } catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void threadStart()
        {
            if (this.isConnected)
            {
                this.networkThread = new Thread(new ThreadStart(receiveMessage));
                networkThread.Start();
            }
        }

        public void threadEnd()
        {
            if (networkThread != null)
            {
                networkThread.Abort();
            }
        }

        public void sendMessage(string message, bool? isGroup)
        {
            MsgPacket packet = new MsgPacket
            {
                MSGType = "1",
                Id = "2205",
                Content = message,
                Group = (isGroup == true)
            };

            string JsonStr = JsonConvert.SerializeObject(packet);
            this.sendData = Encoding.UTF8.GetBytes(JsonStr);

            try
            {
                messageSettings();
                isSend = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public String sendMessage(List<MsgOrderInfo> orderInfo, String orderNum)
        {
            MsgPacket packet = new MsgPacket
            {
                MSGType = "2",
                Id = "2205",
                Group = true,
                ShopName = "배스킨라빈스 구지점",
                Menus = orderInfo,
                OrderNumber = orderNum.ToString(),
            };

            String JsonStr = JsonConvert.SerializeObject(packet);
            this.sendData = Encoding.UTF8.GetBytes(JsonStr);

            try
            {
                messageSettings();
                return getResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return null;
        }
    }
}
