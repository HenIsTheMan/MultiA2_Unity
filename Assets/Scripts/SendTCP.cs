using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

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

/*                string txt = Encoding.UTF8.GetString(bytes);
                if(txt[0] == '~' && txt[1] == '/') {
                } else {
                    GameObject msgListItemGO = Instantiate(msgListItemPrefab, GameObject.Find("Content").transform);

                    Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                    textComponent.text = ClientData.Username + ": " + txt;

                    textComponent.color = new Color(ClientData.MyColor.r, ClientData.MyColor.g, ClientData.MyColor.b, 1.0f);
                }*/
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
            SendStr("~/AddClient " + client.Username + client.MyColor);
        }

        public void OnClientLeave(Client client) {
            SendStr("~/RemoveClient " + client.Username + client.MyColor);
        }

        public void OnEnterChat() {
            SendStr("Hello World!");
        }

        public void OnSendButtonClicked() {
            SendStr(textInputBox.text);
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