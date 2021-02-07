using UnityEngine;
using UnityEngine.UI;

namespace VirtualChat {
    internal sealed class UnreadMsgsTracker: MonoBehaviour {
        #region Fields

        [SerializeField] private Text unreadMsgsCountTxt;

        #endregion

        #region Properties

        public int Qty {
            get;
            set;
        }

        #endregion

        #region Ctors and Dtor

        public UnreadMsgsTracker() {
            unreadMsgsCountTxt = null;
            Qty = 0;
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(unreadMsgsCountTxt);
        }

        private void Update() {
            unreadMsgsCountTxt.text = Qty.ToString();
        }

        #endregion
    }
}