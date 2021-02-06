﻿using UnityEngine;
using UnityEngine.UI;

namespace VirtualChat {
    internal sealed class LoginControl: MonoBehaviour {
        #region Fields

        [SerializeField] private Dropdown dropdown;

        [SerializeField] private GameObject chatCanvas;

        [SerializeField] private GameObject loginButton;

        [SerializeField] private SendTCP sendTCP;

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

            chatCanvas = null;

            loginButton = null;

            sendTCP = null;

            usernameTxtBox = null;
            IPAddressServerTxtBox = null;
            portNumberServerTxtBox = null;

            loginStatusTxt = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(dropdown);

            UnityEngine.Assertions.Assert.IsNotNull(chatCanvas);

            UnityEngine.Assertions.Assert.IsNotNull(loginButton);

            UnityEngine.Assertions.Assert.IsNotNull(sendTCP);

            UnityEngine.Assertions.Assert.IsNotNull(usernameTxtBox);
            UnityEngine.Assertions.Assert.IsNotNull(IPAddressServerTxtBox);
            UnityEngine.Assertions.Assert.IsNotNull(portNumberServerTxtBox);

            UnityEngine.Assertions.Assert.IsNotNull(loginStatusTxt);
        }

        #endregion

        public void OnLoginButtonClicked() {
            if(dropdown.options[dropdown.value].text == "TCP/IP") {
                if(usernameTxtBox.text == string.Empty
                    || IPAddressServerTxtBox.text == string.Empty
                    || portNumberServerTxtBox.text == string.Empty
                ) {
                    loginStatusTxt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                    loginStatusTxt.text = "Login Failed!";
                    return;
                }

                try {
                    sendTCP.DstPortNumber = int.Parse(portNumberServerTxtBox.text);
                } catch(System.Exception) {
                    loginStatusTxt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                    loginStatusTxt.text = "Login Failed!";
                    return;
                }

                sendTCP.DstIPAddress = IPAddressServerTxtBox.text;
                if(sendTCP.InitClient()) {
                    ClientData.Username = usernameTxtBox.text;
                    ClientData.MyColor = Color.HSVToRGB(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f, false);

                    _ = StartCoroutine(nameof(MoveToChat));

                    loginStatusTxt.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
                    loginStatusTxt.text = "Login Successful!";
                } else {
                    loginStatusTxt.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                    loginStatusTxt.text = "Login Failed!";
                }
            } else {
                //??
            }
        }

        private System.Collections.IEnumerator MoveToChat() {
            yield return new WaitForSeconds(4);

            chatCanvas.SetActive(true);

            dropdown.gameObject.SetActive(false);
            usernameTxtBox.gameObject.SetActive(false);
            IPAddressServerTxtBox.gameObject.SetActive(false);
            portNumberServerTxtBox.gameObject.SetActive(false);
            loginButton.SetActive(false);
            loginStatusTxt.gameObject.SetActive(false);

            sendTCP.enabled = true;
            sendTCP.OnEnterChat();
        } 
    }
}