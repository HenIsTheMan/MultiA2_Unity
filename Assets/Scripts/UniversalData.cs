using System.Collections.Generic;
using UnityEngine;

namespace VirtualChat {
    internal sealed class UniversalData: MonoBehaviour { //Static class
		#region Fields

		private static List<Client> clients = new List<Client>();

        [SerializeField] private static SendTCP sendTCP;

        #endregion

        #region Properties
        #endregion

        #region Ctors and Dtor

        private UniversalData() {
        }

        #endregion

        #region Unity User Callback Event Funcs

        private void Awake() {
            UnityEngine.Assertions.Assert.IsNotNull(sendTCP);
        }

        #endregion

        public static void AddClient(Client client) {
            clients.Add(client);
            sendTCP.OnClientJoin(client);
        }

        public static void RemoveClient(Client client) {
            clients.Remove(client);
            sendTCP.OnClientLeave(client);
        }
    }
}