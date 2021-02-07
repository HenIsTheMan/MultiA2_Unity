using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

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

                    if(txts[1].Length == 1) {
                        string contentTxt = txts[2];
                        int contentTxtLen = contentTxt.Length;
                        char contentDelimiter = ' ';

                        List<int> contentDelimiterPos = new List<int>();
                        for(int j = 0; j < contentTxtLen; ++j) {
                            if(contentTxt[j] == contentDelimiter) {
                                contentDelimiterPos.Add(j);
                            }
                        }

                        int contentDelimiterPosSize = (int)contentDelimiterPos.Count;
                        List<string> valTxts = new List<string>();

                        for(int j = 0; j < contentDelimiterPosSize; ++j) {
                            if(j == 0) {
                                valTxts.Add(contentTxt.Substring(0, contentDelimiterPos[0]));
                            } else {
                                valTxts.Add(contentTxt.Substring(contentDelimiterPos[j - 1] + 1, contentDelimiterPos[j] - (contentDelimiterPos[j - 1] + 1)));

                                if(j == contentDelimiterPosSize - 1 && contentDelimiterPos[j] + 1 < rawStrLen) {
                                    valTxts.Add(contentTxt.Substring(contentDelimiterPos[j] + 1, rawStrLen - 1 - contentDelimiterPos[j]));
                                }
                            }
                        }

                        GameObject msgListItemGO = Instantiate(msgListItemPrefab, GameObject.Find("Content").transform);

                        Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                        textComponent.text = valTxts[0] + ": " + valTxts[4];

                        textComponent.color = new Color(float.Parse(valTxts[1]), float.Parse(valTxts[2]), float.Parse(valTxts[3]), 1.0f);
                    } else {
                        //string commandIdentifier = txts[1].Substring(1);
                    }
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
            SendStr(-1 + " /NewClientJoined " + SavedUsername + '\0');
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
					msg = -1 + delimiter + textInputBox.text + delimiter + '\0';
				} else {
					msg = -1 + delimiter + textInputBox.text + '\0';
				}
			} else {
				msg = -1 + " / " + textInputBox.text + '\0';
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
    }
}