using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Cms;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace ConsoleXTBridge.utils
{
    class MysqlConnectionPool
    {

        private int _initPoolSize = 10; //连接池初始大小
        private int _incre = 5;//一次性增加连接数量
        private int  _maxPoolSize = 50; //最大连接数量
        
        private List<PoolConnection> _mConnectionPool = null;//连接池
        private String _mConnectionStr = "Server=120.26.187.166;Port=3306;Database=XTBridge;Uid=root;Pwd=123456;";
        private String testConnValidation = "select 1";

   
        public MysqlConnectionPool() {
            if (_mConnectionPool != null)
            {
                return;
            }
            else {
                _mConnectionPool = new List<PoolConnection>(10);
                createConnections(_initPoolSize);
            }
        }


        #region 增加池中的连接
        private void createConnections(int connectionNum) {
            for (int i = 0; i < connectionNum; i++) {
                if (_mConnectionPool.Count >= _maxPoolSize)
                {
                    return;
                }
                try
                {
                    MySqlConnection conn = createNewConnection();
                    _mConnectionPool.Add(new PoolConnection(ref conn));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("增加池中的连接异常" + ex.ToString());
                }
            }
        }

        #endregion

        #region 创建一个连接并返回
        private MySqlConnection createNewConnection() {
            MySqlConnection newConnection = new MySqlConnection(_mConnectionStr);
            newConnection.Open();
            return newConnection;
        }

        #endregion

        #region 获取连接池中的连接
        public MySqlConnection getConnection() {
            if (_mConnectionPool == null) {
                return null;
            }
            MySqlConnection conn = getFreeConnection();
            while (conn == null) {
                //后面需要改成消费者-生产者 wait notify
                Thread.Sleep(250);
                conn = getFreeConnection();
            }
            return conn;
        }
        #endregion


        #region
        private MySqlConnection getFreeConnection()
        {
            // 从连接池中获得一个可用的数据库连接
            MySqlConnection conn = findFreeConnection();
            if (conn == null)
            {
                createConnections(_incre);
                // 重新从池中查找是否有可用连接
                conn = findFreeConnection();
            }
            return conn;
        }
        #endregion


        #region
        private MySqlConnection findFreeConnection() {
            MySqlConnection conn = null;
            //搜索池中的连接
            for (int i = 0; i < _mConnectionPool.Count; i++) {
                if (!_mConnectionPool[i].isBusy()) {
                    _mConnectionPool[i].setBusy(true);
                    conn = _mConnectionPool[i].getConnection();

                    /*会让效率降低 但是可以确保连接可用 
                     * 但这样还不如不用连接池
                     */
                    
                    if (!testConnection(ref conn)) {
                        try
                        {
                            conn = createNewConnection();
                        }
                        catch (Exception ex){
                            Console.WriteLine("寻找可用连接"+ex.ToString());
                        }
                        _mConnectionPool[i].setConnection(ref conn);
                    }
                    
                    break;
                }
            }
            return conn;
        }
        #endregion


        #region 测试连接有效性 true代表可用

        private bool testConnection(ref  MySqlConnection conn) {
            try {
                MySqlCommand testCommand = new MySqlCommand(testConnValidation, conn);
                MySqlDataReader dbReader  = testCommand.ExecuteReader();
                dbReader.Close();
            } 
            catch
            {
                CloseConnection(conn);
                Console.WriteLine("测试连接出现异常...");
                return false;
            }
            return true;
        }

        #endregion



        #region 关闭连接

        private void CloseConnection(MySqlConnection conn) {
            if (conn == null) {
                return;
            }
            try
            {
                conn.Close();
            }
            catch(Exception ex) {
               Console.WriteLine("关闭连接"+ex.ToString());
            }
        }
        #endregion

        #region 归还连接
        public void returnConnection(MySqlConnection conn) {

            for (int i = 0; i < _mConnectionPool.Count; i++) {
                if (conn == _mConnectionPool[i].getConnection()) {
                    _mConnectionPool[i].setBusy(false);
                    break;
                }
            }
        }
        #endregion


        #region 关闭连接池
        //关闭连接池中所有的连接，并清空连接池。
        public void CloseConnectionPool()
        {
            if (_mConnectionPool == null)
                return;

            for (int i = 0; i < _mConnectionPool.Count; ++i)
            {
                // 关闭此连接
                CloseConnection(_mConnectionPool[i].getConnection());
                _mConnectionPool.RemoveAt(i);
            }
            _mConnectionPool.Clear();
            _mConnectionPool = null;
        }

        #endregion



    }
}
