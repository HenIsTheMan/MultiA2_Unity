using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

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

                BinaryFormatter formatter = new BinaryFormatter();
                object myObj = null;
                try {
                    myObj = formatter.Deserialize(stream);
                } catch(SerializationException e) {
                    Debug.Log("Deserialization Failed: " + e.Message);
                }
                string myStr = myObj as string;

                GameObject msgListItemGO = Instantiate(msgListItemPrefab, GameObject.Find("Content").transform);

                Text textComponent = msgListItemGO.transform.Find("Text").GetComponent<Text>();
                textComponent.text = ClientData.Username + ": " + myStr;

                textComponent.color = new Color(ClientData.MyColor.r, ClientData.MyColor.g, ClientData.MyColor.b, 1.0f);
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
            SendObj("Hello World!");
        }

        public void OnSendButtonClicked() {
            SendObj(textInputBox.text);
            textInputBox.text = string.Empty;
        }

        private void SendObj(object obj) {
            NetworkStream stream = client.GetStream();
            if(stream.CanWrite) {
				MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();

                try {
                    formatter.Serialize(memoryStream, obj);
                } catch(SerializationException e) {
                    Debug.Log("Serialization Failed: " + e.Message);
                }

                byte[] data = memoryStream.ToArray();
                stream.Write(data, 0, data.Length);
            }
        }
    }
}