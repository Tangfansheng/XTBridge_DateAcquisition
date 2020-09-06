using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

/*
 * For Test:
 * 性能测试报告： 在1000个连接测试中 平均比不带连接池的版本快2秒，  在20个连接测试中，也比不带连接池的版本快40ms
 * 样本 1000
 * 连接池 cost time: 5034.6829
    不用连接池 cost time :6797.9445ms.
 * 
 * 样本：20
 * 连接池 cost time: 95.8741
    不用连接池 cost time :130.0158ms.

 * 1.手写的连接池在速度上有明显的优势
 * 2.如果在连接池中加入监测testConnection 会使速度变慢 但是还是会比不用连接池快一些
 */
namespace ConsoleXTBridge.utils
{
    class PoolUnitTest
    {
        
        private static String connectStr = "Server=xxxxx;Port=3306;Database=XTBridge;Uid=xxx;Pwd=xxxxx;";
        private static int numTest = 1000; 

        public static void test() {      
            MysqlConnectionPool pool = new MysqlConnectionPool();
            DateTime beforeDt = System.DateTime.Now;
            for (int i = 0; i < numTest; ++i)
            {
                MySqlConnection tmpConnect = pool.getConnection();
                testConnection(tmpConnect);
                pool.returnConnection(tmpConnect);//用完回收 
            }

            DateTime afterDt = System.DateTime.Now;
            TimeSpan ts = afterDt.Subtract(beforeDt);
            Console.WriteLine("连接池 cost time: {0}", ts.TotalMilliseconds);
            pool.CloseConnectionPool();


            beforeDt = System.DateTime.Now;
            for (int i = 0; i < numTest; ++i)
            {
                MySqlConnection mSqlcon = new MySqlConnection(connectStr);
                mSqlcon.Open();
                testConnection(mSqlcon);
                mSqlcon.Close();
            }

            afterDt = System.DateTime.Now;
            TimeSpan ts3 = afterDt.Subtract(beforeDt);
            Console.WriteLine("不用连接池 cost time :{0}ms.", ts3.TotalMilliseconds);

        }


        public static void testConnection(MySqlConnection conn) {
            try
            {
                String testSql = "select 1  from anchor_force";
                MySqlCommand dbCommand = new MySqlCommand(testSql, conn);
                MySqlDataReader reader = dbCommand.ExecuteReader();

                if (reader.Read())
                {
                    int r = reader.GetInt16(0);
                   // Console.WriteLine(r);
                    reader.Close();
                }
            }
            catch(Exception ex) {
               
                Console.WriteLine("测试：", ex.ToString());
            }
        }
 
       

    }
}
