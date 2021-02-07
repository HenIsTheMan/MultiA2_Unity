using UnityEngine;

namespace VirtualChat {
    internal sealed class ChatCanvasControl: MonoBehaviour {
        #region Fields

        [SerializeField] private UnreadMsgsTracker publicUnreadMsgsTracker;
        [SerializeField] private UnreadMsgsTracker serverUnreadMsgsTracker;
        [SerializeField] private GameObject publicChatCanvas;
        [SerializeField] private GameObject serverChatCanvas;

        #endregion

        #region Properties

        public bool IsPublicActive {
            get;
            private set;
        }

        #endregion

        #region Ctors and Dtor

        public ChatCanvasControl() {
            IsPublicActive = true;
            publicUnreadMsgsTracker = null;
            serverUnreadMsgsTracker = null;
            publicChatCanvas = null;
            serverChatCanvas = null;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(publicUnreadMsgsTracker);
            UnityEngine.Assertions.Assert.IsNotNull(serverUnreadMsgsTracker);
            UnityEngine.Assertions.Assert.IsNotNull(publicChatCanvas);
            UnityEngine.Assertions.Assert.IsNotNull(serverChatCanvas);
        }

        private void Update() {
            if(IsPublicActive) {
                publicChatCanvas.SetActive(true);
                serverChatCanvas.SetActive(false);
            } else {
                serverChatCanvas.SetActive(true);
                publicChatCanvas.SetActive(false);
            }
        }

        #endregion

        public void OnPublicChatButtonClick() {
            IsPublicActive = true;
            publicUnreadMsgsTracker.Qty = 0;
        }

        public void OnServerChatButtonClick() {
            IsPublicActive = false;
            serverUnreadMsgsTracker.Qty = 0;
        }
    }
}