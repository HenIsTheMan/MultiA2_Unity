using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

        private bool isMyClientActivated;
        private TcpClient client;
        [SerializeField] private InputField textInputBox;
        [SerializeField] private GameObject msgListItemPrefab;

        #endregion

        #region Properties

        public string DstIPAddress {
            private get;
            set;
        }

        public int DstPortNumber {
            private get;
            set;
        }

        public string SavedUsername {
            private get;
            set;
        }

        #endregion

        #region Ctors and Dtor

        public SendTCP() {
            isMyClientActivated = false;
            client = null;
            textInputBox = null;
            msgListItemPrefab = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(textInputBox);
            UnityEngine.Assertions.Assert.IsNotNull(msgListItemPrefab);
        }

        private void Update() {
            NetworkStream stream = client.GetStream();

            if(stream.DataAvailable) {
                byte[] bytes = new byte[client.ReceiveBufferSize];
                _ = stream.Read(bytes, 0, client.ReceiveBufferSize); //Returns 0 - client.ReceiveBufferSize //Blocks calling thread of execution until at least 1 byte is read

                string pureStr = Encoding.UTF8.GetString(bytes);
                int pureStrLen = pureStr.Length;
                char pureDelimiter = '\0';
                List<int> pureDelimiterPos = new List<int>();

                for(int i = 0; i < pureStrLen; ++i) {
                    if(pureStr[i] == pureDelimiter) {
                        pureDelimiterPos.Add(i);
                        if(i < pureStrLen - 1 && pureStr[i + 1] == pureDelimiter) {
                            break;
                        }
                    }
                }

                int pureDelimiterPosSize = pureDelimiterPos.Count;
                List<string> rawStrs = new List<string>();

                for(int i = 0; i < pureDelimiterPosSize; ++i) {
                    if(i == 0) {
                        rawStrs.Add(pureStr.Substring(0, pureDelimiterPos[0]));
                    } else {
                        int startIndex = pureDelimiterPos[i - 1] + 1;
                        rawStrs.Add(pureStr.Substring(startIndex, pureDelimiterPos[i] - startIndex));
                    }
                }

                int rawStrsSize = pureDelimiterPos.Count;
                for(int i = 0; i < rawStrsSize; ++i) {
                    string rawStr = rawStrs[i];
                    int rawStrLen = rawStr.Length;
                    char delimiter = ' ';

                    List<int> delimiterPos = new List<int>();
                    for(int j = 0, count = 0; j < rawStrLen && count <= 2; ++j) {
                        if(rawStr[j] == delimiter) {
                            delimiterPos.Add(j);
                            ++count;
                        }
                    }

                    List<string> txts = new List<string> {
                        rawStr.Substring(0, delimiterPos[0]),
                        rawStr.Substring(delimiterPos[0] + 1, delimiterPos[1] - (delimiterPos[0] + 1)),
                        rawStr.Substring(delimiterPos[1] + 1, rawStrLen - delimiterPos[1] - 1)
                    };

                    /*
                    string commandIdentifier = txts[1].Substring(2);

                    if(commandIdentifier == "UpdateClients") {
                        int txtsCountMinusOne = txts.Count - 1;
                        int membersToUpdateCount = 5;
                        string myUsername = UniversalData.GetClient(UniversalData.MyClientIndex).Username;
                        UniversalData.ClearClients();

                        for(int offset = 0; offset < txtsCountMinusOne / membersToUpdateCount; ++offset) {
                            Client client = new Client {
                                Index = int.Parse(txts[1 + offset]),
                                Username = txts[2 + offset],
                                MyColor = new Color(float.Parse(txts[3 + offset]), float.Parse(txts[4 + offset]), float.Parse(txts[5 + offset]))
                            };
                            UniversalData.AddClient(client);
                        }

                        int clientsSize = UniversalData.CalcAmtOfClients();
                        for(int i = 0; i < clientsSize; ++i) {
                            Client currClient = UniversalData.GetClient(i);
                            if(myUsername == currClient.Username) {
                                UniversalData.MyClientIndex = currClient.Index;
                                break;
                            }
                        }

                        if(!isMyClientActivated) {
                            ActivateClient();
                            isMyClientActivated = true;
                        }
                    }
                    //*/

                    GameObject msgListItemGO = Instantiate(msgListItemPrefab, GameObject.Find("Content").transform);
                    //Client sender = UniversalData.GetClient(rawStr[0] - 48);

                    Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                    //textComponent.text = sender.Username + ": " + rawStr.Substring(3);
                    textComponent.text = "Test";

                    //textComponent.color = new Color(sender.MyColor.r, sender.MyColor.g, sender.MyColor.b, 1.0f);
                }
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

        public void OnEnterChat() {
            SendStr(UniversalData.MyClientIndex + " / Hi! I'm new.\0");
        }

        public void OnSendButtonClicked() {
			int textLen = textInputBox.text.Length;
			char delimiter = ' ';

            int delimiterPos1st = -1;
            for(int i = 0; i < textLen; ++i) {
                if(textInputBox.text[i] == delimiter) {
                    delimiterPos1st = i;
                    break;
                }
            }

			string msg;
			if(textInputBox.text[0] == '/' && delimiterPos1st != 1) {
				if(textInputBox.text[textInputBox.text.Length - 1] == delimiter && textInputBox.text[textInputBox.text.Length - 2] != delimiter) {
					msg = UniversalData.MyClientIndex + ' ' + textInputBox.text + ' ' + '\0';
				} else {
					msg = UniversalData.MyClientIndex + ' ' + textInputBox.text + '\0';
				}
			} else {
				msg = UniversalData.MyClientIndex + " / " + textInputBox.text + '\0';
			}

			SendStr(msg);
            textInputBox.text = string.Empty;
        }

        private void SendStr(string msg) {
            NetworkStream stream = client.GetStream();

            if(stream.CanWrite) {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
            }
        }

        private void ActivateClient() {
            /*Client client = new Client {
                Index = UniversalData.CalcAmtOfClients(),
                Username = SavedUsername,
                MyColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f, false)
            };
            UniversalData.MyClientIndex = client.Index;
            UniversalData.AddClient(client);

            string msg = "~/UpdateClients";
            int clientsSize = UniversalData.CalcAmtOfClients();
            for(int i = 0; i < clientsSize; ++i){
                Client currClient = UniversalData.GetClient(i);
                msg += ' ' + currClient.Index;
                msg += ' ' + currClient.Username;
                msg += ' ' + currClient.MyColor.r;
                msg += ' ' + currClient.MyColor.g;
                msg += ' ' + currClient.MyColor.b;
            }

            SendStr(msg);*/
        }
    }
}