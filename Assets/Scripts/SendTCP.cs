using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

        private TcpClient client;
        [SerializeField] private InputField textInputBox;
        [SerializeField] private Text textOutput;

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
            textOutput = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
			UnityEngine.Assertions.Assert.IsNotNull(textInputBox);
            UnityEngine.Assertions.Assert.IsNotNull(textOutput);
        }

        private void Update() {
            NetworkStream stream = client.GetStream();
            if(stream.DataAvailable) {
                byte[] bytes = new byte[client.ReceiveBufferSize];
                _ = stream.Read(bytes, 0, client.ReceiveBufferSize); //Returns 0 - client.ReceiveBufferSize //Blocks calling thread of execution until at least 1 byte is read

                BinaryFormatter formatter = new BinaryFormatter();
                string myStr = string.Empty;
                try {
                    myStr = (string)formatter.Deserialize(stream);
                } catch(SerializationException e) {
                    Debug.Log("Deserialization Failed: " + e.Message);
                }

                textOutput.text = myStr;
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
                MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();

                try {
                    formatter.Serialize(stream, msg);
                } catch(SerializationException e) {
                    Debug.Log("Serialization Failed: " + e.Message);
                }

                byte[] data = memoryStream.ToArray();
                stream.Write(data, 0, data.Length);
            }
        }


/*        private void SendStr(object obj) {
            NetworkStream stream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();

            if(stream.CanWrite) {
                try {
                    formatter.Serialize(stream, obj);
                } catch(System.Runtime.Serialization.SerializationException e) {
                    Debug.Log("Serialization Failed: " + e.Message);
                }


                byte[] data = stream.ToArray();
                stream.Write(data, 0, data.Length);
                //stream.Seek(0, SeekOrigin.Begin);

                //stream.Close();
            }
        }*/

        /* BinaryFormatter formatter = new BinaryFormatter();
try {
    objectThatWasDeserialized = (SomeObject)formatter.Deserialize(stream);
} catch(SerializationException e) {
    Debug.Log("Deserialization Failed : " + e.Message);
}*/

        /*                        using(var memoryStream = new MemoryStream()) {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }*/


    }
}