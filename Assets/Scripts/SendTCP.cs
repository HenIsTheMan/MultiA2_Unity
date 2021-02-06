using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

        private bool canWrite;
        private Queue<string> msgQueue;
        private TcpClient client;
        [SerializeField] private InputField textInputBox;
        [SerializeField] private GameObject msgListItemPrefab;

        #endregion

        #region Properties

        public string DstIPAddress {
            get;
            set;
        }

        public int DstPortNumber {
            get;
            set;
        }

        #endregion

        #region Ctors and Dtor

        public SendTCP() {
            canWrite = true;
            msgQueue = null;
            client = null;
            textInputBox = null;
            msgListItemPrefab = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            msgQueue = new Queue<string>();
            UnityEngine.Assertions.Assert.IsNotNull(textInputBox);
            UnityEngine.Assertions.Assert.IsNotNull(msgListItemPrefab);
        }

        private void Update() {
            NetworkStream stream = client.GetStream();

            if(canWrite && stream.CanWrite && msgQueue.Count > 0) {
                string msg = msgQueue.Dequeue();
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
                canWrite = false;
            }

            if(stream.DataAvailable) {
                byte[] bytes = new byte[client.ReceiveBufferSize];
                _ = stream.Read(bytes, 0, client.ReceiveBufferSize); //Returns 0 - client.ReceiveBufferSize //Blocks calling thread of execution until at least 1 byte is read

                string rawStr = Encoding.UTF8.GetString(bytes);
                if(rawStr[0] == '~' && rawStr[1] == '/') {
                    int rawStrLen = rawStr.Length;
                    List<int> spacePosIndices = new List<int>();
                    for(int i = 0; i < rawStrLen; ++i) {
                        if(rawStr[i] == ' ') {
                            spacePosIndices.Add(i);
                        }
                    }

                    int spacePosIndicesSize = spacePosIndices.Count;
                    if(spacePosIndicesSize > 0) {
                        List<string> txts = new List<string>();
                        for(int i = 0; i < spacePosIndicesSize; ++i) {
                            if(i == 0) {
                                txts.Add(rawStr.Substring(0, spacePosIndices[0]));
                            } else {
                                txts.Add(rawStr.Substring(spacePosIndices[i - 1] + 1, spacePosIndices[i] - (spacePosIndices[i - 1] + 1)));

                                if(i == spacePosIndicesSize - 1 && spacePosIndices[i] + 1 < rawStrLen) {
                                    txts.Add(rawStr.Substring(spacePosIndices[i] + 1, rawStrLen - 1 - spacePosIndices[i]));
                                }
                            }
                        }

                        string commandIdentifier = txts[0].Substring(2);
                        if(commandIdentifier == "AddClient") {
                            Client client = new Client {
                                Index = int.Parse(txts[1]),
                                Username = txts[2],
                                MyColor = new Color(float.Parse(txts[3]), float.Parse(txts[4]), float.Parse(txts[5]))
                            };

                            UniversalData.AddClient(client);
                        } else if(commandIdentifier == "RemoveClient") {
                            UniversalData.RemoveClient(int.Parse(txts[1]));
                        }
                    }
                } else {
                    GameObject msgListItemGO = Instantiate(msgListItemPrefab, GameObject.Find("Content").transform);
                    Client sender = UniversalData.GetClient(rawStr[0] - 48);

                    Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                    textComponent.text = sender.Username + ": " + rawStr.Substring(3);

                    textComponent.color = new Color(sender.MyColor.r, sender.MyColor.g, sender.MyColor.b, 1.0f);
                }

                canWrite = true;
            }
        }

        private void OnDisable() {
            client.GetStream().Close();
            client.Close();
        }

        #endregion
        public bool InitClient() {
            try {
                client = new TcpClient(DstIPAddress, DstPortNumber);
            } catch(SocketException) {
                return false;
            }
            return true;
        }

        public void OnClientJoin(Client client) {
            SendStr("~/AddClient " + client.Index + ' ' + client.Username + ' ' + client.MyColor.r + ' ' + client.MyColor.g + ' ' + client.MyColor.b);
        }

        public void OnClientLeave(Client client) {
            SendStr("~/RemoveClient " + client.Index);
        }

        public void OnEnterChat() {
            SendStr(UniversalData.MyClientIndex + "/ Welcome!");
        }

        public void OnSendButtonClicked() {
            SendStr(UniversalData.MyClientIndex + "/ " + textInputBox.text);
            textInputBox.text = string.Empty;
        }

        private void SendStr(string msg) {
            msgQueue.Enqueue(msg);
        }
    }
}