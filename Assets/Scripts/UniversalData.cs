using System.Collections.Generic;
using UnityEngine;

namespace VirtualChat {
    internal sealed class UniversalData: MonoBehaviour { //Static class
		#region Fields

		private static List<Client> clients = new List<Client>();

        #endregion

        #region Properties

        public static int MyClientIndex {
            get;
            set;
        }

        #endregion

        #region Ctors and Dtor

        private UniversalData() {
        }

        #endregion

        #region Unity User Callback Event Funcs
        #endregion

        public static void AddClient(Client client) {
            clients.Add(client);
        }

        public static void RemoveClient(int index) {
            clients.Remove(clients[index]);
        }

        public static int CalcAmtOfClients() {
            return clients.Count;
        }

        public static Client GetClient(int index) {
            return clients[index];
        }
    }
}