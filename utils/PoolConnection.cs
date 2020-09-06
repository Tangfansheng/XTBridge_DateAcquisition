using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace ConsoleXTBridge.utils
{

    /**
     * 包装一个Connection
     */
    class PoolConnection
    {
        private MySqlConnection _mConnection;
        private bool _isBusy = false;  //connection status

        public PoolConnection(ref MySqlConnection conn){
            this._mConnection = conn;
        }

        public MySqlConnection getConnection() {
            return this._mConnection;
        }

        public void setConnection(ref MySqlConnection conn) {
            _mConnection = conn;
        }

        public void setBusy(bool status) {
            _isBusy = status;
        }

        public bool isBusy() {
            return _isBusy;
        }

    }
}
