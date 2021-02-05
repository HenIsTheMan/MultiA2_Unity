using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

        private TcpClient client;

        [SerializeField] private string IP = "127.0.0.1"; //??
        [SerializeField] private int port;
        [SerializeField] private InputField textInputBox;
        [SerializeField] private Text textOutput;

        #endregion

        #region Properties
        #endregion

        #region Ctors and Dtor

        public SendTCP() {
            client = null;

            IP = "127.0.0.1"; //??
            port = 0;
            textInputBox = null;
            textOutput = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
			UnityEngine.Assertions.Assert.IsNotNull(textInputBox);
            UnityEngine.Assertions.Assert.IsNotNull(textOutput);
        }

        private void Start() {
            client = new TcpClient(IP, port);
            SendStr("Hello World!");
        }

        private void Update() {
            NetworkStream stream = client.GetStream();
            if(stream.DataAvailable) {
                byte[] bytes = new byte[client.ReceiveBufferSize];
                stream.Read(bytes, 0, client.ReceiveBufferSize); //Returns 0 - client.ReceiveBufferSize //Blocks calling thread until at least 1 byte is read
                textOutput.text = Encoding.UTF8.GetString(bytes);
            }
        }

        private void OnDisable() {
            client.GetStream().Close();
            client.Close();
        }

        #endregion

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