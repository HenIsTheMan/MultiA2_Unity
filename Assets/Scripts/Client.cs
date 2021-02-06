using UnityEngine;

namespace VirtualChat {
    internal sealed class Client: MonoBehaviour {
        #region Fields
        #endregion

        #region Properties

        public int Index {
            get;
            set;
        }

        public string Username {
            get;
            set;
        }

        public Color MyColor {
            get;
            set;
        }

        #endregion

        #region Ctors and Dtor

        public Client() {
        }

        #endregion

        #region Unity User Callback Event Funcs
        #endregion
    }
}