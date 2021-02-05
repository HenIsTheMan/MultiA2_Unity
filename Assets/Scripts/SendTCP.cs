using UnityEngine;
using System.Text;
using System.Net.Sockets;
using UnityEngine.UI;

namespace VirtualChat {
    internal sealed class SendTCP: MonoBehaviour {
        #region Fields

        private TcpClient client;

        [SerializeField] string IP = "127.0.0.1"; //??
        [SerializeField] int port;
        [SerializeField] Text textInput;
        [SerializeField] Text textOutput;

        #endregion

        #region Properties
        #endregion

        #region Ctors and Dtor

        public SendTCP() {
            client = null;

            IP = "127.0.0.1"; //??
            port = 0;
            textInput = null;
            textOutput = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
			UnityEngine.Assertions.Assert.IsNotNull(textInput);
            UnityEngine.Assertions.Assert.IsNotNull(textOutput);
        }

        private void Start() {
            client = new TcpClient(IP, port);
            //SendStr("Hello World!"); //??
        }

        #endregion
    }
}