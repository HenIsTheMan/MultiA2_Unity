using UnityEngine;
using UnityEngine.UI;

namespace VirtualChat {
    internal sealed class LoginControl: MonoBehaviour {
        #region Fields

        [SerializeField] private Dropdown dropdown;

        [SerializeField] private GameObject chatCanvasControl;

        [SerializeField] private GameObject loginButton;

        [SerializeField] private SendOverNet sendOverNet;

        [SerializeField] private InputField usernameTxtBox;
        [SerializeField] private InputField IPAddressServerTxtBox;
        [SerializeField] private InputField portNumberServerTxtBox;

        [SerializeField] private Text loginStatusTxt;

        #endregion

        #region Properties
        #endregion

        #region Ctors and Dtor

        public LoginControl() {
            dropdown = null;

            chatCanvasControl = null;

            loginButton = null;

            sendOverNet = null;

            usernameTxtBox = null;
            IPAddressServerTxtBox = null;
            portNumberServerTxtBox = null;

            loginStatusTxt = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(dropdown);

            UnityEngine.Assertions.Assert.IsNotNull(chatCanvasControl);

            UnityEngine.Assertions.Assert.IsNotNull(loginButton);

            UnityEngine.Assertions.Assert.IsNotNull(sendOverNet);

            UnityEngine.Assertions.Assert.IsNotNull(usernameTxtBox);
            UnityEngine.Assertions.Assert.IsNotNull(IPAddressServerTxtBox);
            UnityEngine.Assertions.Assert.IsNotNull(portNumberServerTxtBox);

            UnityEngine.Assertions.Assert.IsNotNull(loginStatusTxt);
        }

        #endregion

        public void OnLoginButtonClicked() {
            sendOverNet.Protocol = dropdown.options[dropdown.value].text == "TCP/IP" ? "TCP" : "UDP";

            if(usernameTxtBox.text == string.Empty
                || IPAddressServerTxtBox.text == string.Empty
                || portNumberServerTxtBox.text == string.Empty
            ) {
                loginStatusTxt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                loginStatusTxt.text = "Login Failed!";
                return;
            }

            try {
                sendOverNet.DstPortNumber = int.Parse(portNumberServerTxtBox.text);
            } catch(System.Exception) {
                loginStatusTxt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                loginStatusTxt.text = "Login Failed!";
                return;
            }

            sendOverNet.DstIPAddress = IPAddressServerTxtBox.text;
            if(sendOverNet.InitClient()) {
                sendOverNet.SavedUsername = usernameTxtBox.text;

                _ = StartCoroutine(nameof(MoveToChat));

                loginStatusTxt.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                loginStatusTxt.text = "Login Successful!";
            } else {
                loginStatusTxt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                loginStatusTxt.text = "Login Failed!";
            }
        }

        private System.Collections.IEnumerator MoveToChat() {
            yield return new WaitForSeconds(4);

            chatCanvasControl.SetActive(true);

            dropdown.gameObject.SetActive(false);
            usernameTxtBox.gameObject.SetActive(false);
            IPAddressServerTxtBox.gameObject.SetActive(false);
            portNumberServerTxtBox.gameObject.SetActive(false);
            loginButton.SetActive(false);
            loginStatusTxt.gameObject.SetActive(false);

            sendOverNet.enabled = true;
            sendOverNet.OnEnterChat();
        }
    }
}