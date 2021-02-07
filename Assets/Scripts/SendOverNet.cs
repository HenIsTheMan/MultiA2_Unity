using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using System;

namespace VirtualChat {
    internal sealed class SendOverNet: MonoBehaviour {
        #region Fields

        private TcpClient clientTCP;
        private UdpClient clientUDP;
        private IPEndPoint remoteEndPt;
        [SerializeField] private GameObject msgListItemPrefab;

        [SerializeField] private ChatCanvasControl chatCanvasControlScript;
        [SerializeField] private InputField publicTextInputBox;
        [SerializeField] private InputField serverTextInputBox;
        [SerializeField] private GameObject publicContent;
        [SerializeField] private GameObject serverContent;

        [SerializeField] private UnreadMsgsTracker publicUnreadMsgsTracker;
        [SerializeField] private UnreadMsgsTracker serverUnreadMsgsTracker;

        #endregion

        #region Properties

        public string Protocol {
            private get;
            set;
        }

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

        public SendOverNet() {
            clientTCP = null;
            clientUDP = null;
            remoteEndPt = null;
            msgListItemPrefab = null;

            chatCanvasControlScript = null;
            publicTextInputBox = null;
            serverTextInputBox = null;
            publicContent = null;
            serverContent = null;

            publicUnreadMsgsTracker = null;
            serverUnreadMsgsTracker = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(msgListItemPrefab);

            UnityEngine.Assertions.Assert.IsNotNull(chatCanvasControlScript);
            UnityEngine.Assertions.Assert.IsNotNull(publicTextInputBox);
            UnityEngine.Assertions.Assert.IsNotNull(serverTextInputBox);
            UnityEngine.Assertions.Assert.IsNotNull(publicContent);
            UnityEngine.Assertions.Assert.IsNotNull(serverContent);

            UnityEngine.Assertions.Assert.IsNotNull(publicUnreadMsgsTracker);
            UnityEngine.Assertions.Assert.IsNotNull(serverUnreadMsgsTracker);
        }

        private void Update() {
            if(Protocol == "UDP") {
                return;
            }

            NetworkStream stream = clientTCP.GetStream();

            if(stream.DataAvailable) {
                byte[] bytes = new byte[clientTCP.ReceiveBufferSize];
                _ = stream.Read(bytes, 0, clientTCP.ReceiveBufferSize); //Returns 0 - clientTCP.ReceiveBufferSize //Blocks calling thread of execution until at least 1 byte is read

                ProcessReceivedStr(Encoding.UTF8.GetString(bytes));
            }
        }

        private void OnDisable() {
            if(Protocol == "TCP") {
                clientTCP.GetStream().Close();
                clientTCP.Close();
            } else {
                clientUDP.Close();
            }
        }

        #endregion

        public bool Init() {
            if(Protocol == "TCP") {
                try {
                    clientTCP = new TcpClient(DstIPAddress, DstPortNumber);
                } catch(SocketException) {
                    return false;
                }
            } else{
                try {
                    //clientUDP = new UdpClient(DstIPAddress, DstPortNumber);
                    clientUDP = new UdpClient();
                } catch(SocketException) {
                    return false;
                }

                remoteEndPt = new IPEndPoint(IPAddress.Parse(DstIPAddress), DstPortNumber);
                clientUDP.EnableBroadcast = true;

                //sendString("Hello World I am??");

                try {
                    clientUDP.BeginReceive(new AsyncCallback(Receive), null);
                } catch(Exception e) {
                    Debug.Log(e.ToString());
                    return false;
                }
            }
            return true;
        }

        private void Receive(IAsyncResult res) {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, DstPortNumber);
            byte[] bytes = clientUDP.EndReceive(res, ref RemoteIpEndPoint);

            ProcessReceivedStr(Encoding.UTF8.GetString(bytes));
            clientUDP.BeginReceive(new AsyncCallback(Receive), null);
        }

        public void OnEnterChat() {
            SendStr(-1 + " /NewClientJoined " + SavedUsername);
        }

        public void OnSendButtonClicked() {
            InputField textInputBox = chatCanvasControlScript.IsPublicActive ? publicTextInputBox : serverTextInputBox;
            if(textInputBox.text == string.Empty) {
                return;
            }

            int textLen = textInputBox.text.Length;
			char delimiter = ' ';

            int delimiterPos1st = -999;
            for(int i = 0; i < textLen; ++i) {
                if(textInputBox.text[i] == delimiter) {
                    delimiterPos1st = i;
                    break;
                }
            }

			string msg;
            string prefix = chatCanvasControlScript.IsPublicActive ? "0" : "1";

            if(textInputBox.text[0] == '/' && delimiterPos1st != 1) {
                string inputTxt = textInputBox.text;
                while(inputTxt.EndsWith(" ")) {
                    inputTxt = inputTxt.Substring(0, inputTxt.Length - 1);
                }

                int count = 0;
                int inputTxtLen = inputTxt.Length;
                for(int i = 0; i < inputTxtLen; ++i) {
                    if(inputTxt[i] == ' ') {
                         ++count;
                    }
                }

                if(count == 0) {
                    msg = prefix + delimiter + inputTxt + delimiter + delimiter;
                } else {
                    msg = prefix + delimiter + inputTxt;
                }
			} else {
				msg = prefix + " / " + textInputBox.text;
			}

			SendStr(msg);
            textInputBox.text = string.Empty;
        }

        private void SendStr(string msg) {
            if(Protocol == "TCP") {
                NetworkStream stream = clientTCP.GetStream();

                if(stream.CanWrite) {
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    stream.Write(data, 0, data.Length);
                }
            } else {
                try {
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    clientUDP.Send(data, data.Length, remoteEndPt);
                } catch(Exception e) {
                    print(e.ToString());
                    return;
                }
            }

            Debug.Log("[SENT] " + msg);
        }

        private void ProcessReceivedStr(string msg) {
            string pureStr = msg;
            Debug.Log("[RECEIVED] " + pureStr);

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

            int rawStrsSize = rawStrs.Count;
            for(int i = 0; i < rawStrsSize; ++i) {
                string rawStr = rawStrs[i];
                int rawStrLen = rawStr.Length;
                char delimiter = ' ';

                List<int> delimiterPos = new List<int>();
                for(int j = 0, count = 0; j < rawStrLen && count < 2; ++j) {
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
                    for(int j = 0, count = 0; j < contentTxtLen && count < 4; ++j) {
                        if(contentTxt[j] == contentDelimiter) {
                            contentDelimiterPos.Add(j);
                            ++count;
                        }
                    }

                    int contentDelimiterPosSize = contentDelimiterPos.Count;
                    List<string> valTxts = new List<string>();

                    for(int j = 0; j < contentDelimiterPosSize; ++j) {
                        if(j == 0) {
                            valTxts.Add(contentTxt.Substring(0, contentDelimiterPos[0]));
                        } else {
                            valTxts.Add(contentTxt.Substring(contentDelimiterPos[j - 1] + 1, contentDelimiterPos[j] - (contentDelimiterPos[j - 1] + 1)));
                        }

                        if(j == contentDelimiterPosSize - 1 && contentDelimiterPos[j] + 1 < contentTxtLen) {
                            valTxts.Add(contentTxt.Substring(contentDelimiterPos[j] + 1, contentTxtLen - 1 - contentDelimiterPos[j]));
                        }
                    }

                    if(txts[0] == "0") {
                        ++publicUnreadMsgsTracker.Qty;
                    } else {
                        ++serverUnreadMsgsTracker.Qty;
                    }

                    GameObject msgListItemGO = Instantiate(msgListItemPrefab, txts[0] == "0" ? publicContent.transform : serverContent.transform);

                    Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                    textComponent.text = valTxts[0] + ": " + valTxts[4];

                    textComponent.color = new Color(float.Parse(valTxts[1]), float.Parse(valTxts[2]), float.Parse(valTxts[3]), 1.0f);
                } else {
                    string commandIdentifier = txts[1].Substring(1);
                    commandIdentifier = commandIdentifier.ToLower();

                    if(commandIdentifier == "clear") {
                        GameObject content = chatCanvasControlScript.IsPublicActive ? publicContent : serverContent;
                        foreach(Transform child in content.transform) {
                            Destroy(child.gameObject);
                        }

                        if(chatCanvasControlScript.IsPublicActive) {
                            publicUnreadMsgsTracker.Qty = 0;
                        } else {
                            serverUnreadMsgsTracker.Qty = 0;
                        }
                    } else if(commandIdentifier == "wipe") {
                        foreach(Transform child in publicContent.transform) {
                            Destroy(child.gameObject);
                        }

                        publicUnreadMsgsTracker.Qty = 0;
                    } else if(commandIdentifier == "datame") {
                        string contentTxt = txts[2];
                        int contentTxtLen = contentTxt.Length;
                        char contentDelimiter = ' ';

                        List<int> contentDelimiterPos = new List<int>();
                        for(int j = 0, count = 0; j < contentTxtLen && count < 4; ++j) {
                            if(contentTxt[j] == contentDelimiter) {
                                contentDelimiterPos.Add(j);
                                ++count;
                            }
                        }

                        int contentDelimiterPosSize = contentDelimiterPos.Count;
                        List<string> valTxts = new List<string>();

                        for(int j = 0; j < contentDelimiterPosSize; ++j) {
                            if(j == 0) {
                                valTxts.Add(contentTxt.Substring(0, contentDelimiterPos[0]));
                            } else {
                                valTxts.Add(contentTxt.Substring(contentDelimiterPos[j - 1] + 1, contentDelimiterPos[j] - (contentDelimiterPos[j - 1] + 1)));
                            }

                            if(j == contentDelimiterPosSize - 1 && contentDelimiterPos[j] + 1 < contentTxtLen) {
                                valTxts.Add(contentTxt.Substring(contentDelimiterPos[j] + 1, contentTxtLen - 1 - contentDelimiterPos[j]));
                            }
                        }

                        ++serverUnreadMsgsTracker.Qty;

                        GameObject msgListItemGO = Instantiate(msgListItemPrefab, serverContent.transform);

                        RectTransform rectTransform = (RectTransform)msgListItemGO.transform;
                        rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height * 2.0f);

                        Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                        textComponent.text = "About Me\n"
                            + "Username: " + valTxts[0] + '\n'
                            + "Color: " + "RGB(" + valTxts[1] + ", " + valTxts[2] + ", " + valTxts[3] + ")\n"
                            + "isAfk: " + valTxts[4];

                        textComponent.color = new Color(0.0f, 0.0f, 0.0f);
                    } else if(commandIdentifier == "datawho") {
                        string contentTxt = txts[2];
                        int contentTxtLen = contentTxt.Length;
                        char contentDelimiter = ' ';

                        List<int> contentDelimiterPos = new List<int>();
                        for(int j = 0; j < contentTxtLen; ++j) {
                            if(contentTxt[j] == contentDelimiter) {
                                contentDelimiterPos.Add(j);
                            }
                        }

                        int contentDelimiterPosSize = contentDelimiterPos.Count;
                        List<string> valTxts = new List<string>();

                        for(int j = 0; j < contentDelimiterPosSize; ++j) {
                            if(j == 0) {
                                valTxts.Add(contentTxt.Substring(0, contentDelimiterPos[0]));
                            } else {
                                valTxts.Add(contentTxt.Substring(contentDelimiterPos[j - 1] + 1, contentDelimiterPos[j] - (contentDelimiterPos[j - 1] + 1)));
                            }

                            if(j == contentDelimiterPosSize - 1 && contentDelimiterPos[j] + 1 < contentTxtLen) {
                                valTxts.Add(contentTxt.Substring(contentDelimiterPos[j] + 1, contentTxtLen - 1 - contentDelimiterPos[j]));
                            }
                        }

                        ++serverUnreadMsgsTracker.Qty;

                        int onlineCount = valTxts.Count / 2;

                        GameObject msgListItemGO = Instantiate(msgListItemPrefab, serverContent.transform);

                        RectTransform rectTransform = (RectTransform)msgListItemGO.transform;
                        rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height * (1.0f + onlineCount * 0.25f));

                        Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();

                        string myTxt = "Users in server\n";
                        for(int j = 0; j < onlineCount; ++j) {
                            myTxt += "Username: " + valTxts[2 * j] + (valTxts[2 * j + 1] == "true" ? " (afk)\n" : "\n");
                        }
                        textComponent.text = myTxt;

                        textComponent.color = new Color(0.0f, 0.0f, 0.0f);
                    }
                }
            }
        }
    }
}