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

        public string Username {
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

                GameObject msgListItemGO = Instantiate(msgListItemPrefab, GameObject.Find("Content").transform);

                Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                textComponent.text = Encoding.UTF8.GetString(bytes);

                Color myColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f, false);
                textComponent.color = new Color(myColor.r, myColor.g, myColor.b, 1.0f);
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