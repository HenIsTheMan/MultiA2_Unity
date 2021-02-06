using UnityEngine;

namespace VirtualChat {
    internal sealed class ClientData: MonoBehaviour { //Static class
        #region Fields
        #endregion

        #region Properties

        public static Color MyColor {
            get;
            set;
        }

        public static string Username {
            get;
            set;
        }

        #endregion

        #region Ctors and Dtor

        private ClientData() {
        }

        #endregion

        #region Unity User Callback Event Funcs
        #endregion
    }
}