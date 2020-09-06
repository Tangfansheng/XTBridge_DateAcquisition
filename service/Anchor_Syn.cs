using System;
using System.Collections.Generic;
using System.Text;
using Advantech.Adam;
using ConsoleXTBridge.utils;
using MySql.Data.MySqlClient;
using System.Net.Sockets;
using System.Threading;

namespace ConsoleXTBridge.service
{
    /**
     * 
    0.实体按键 点击之后开始运行
 * 1.读取数据
 * 2.然后组合sql
 * 3.执行sql
 */
    class Anchor_Syn
    {
        private bool m_bStart;//是否开始读取数据
        private AdamSocket adamModbus;
        private Adam6000Type m_Adam6000Type;
        private string m_szIP;
        private int m_iPort;
        private int m_iAiTotal;//使用的通道数量
        private bool[] m_bChEnabled;
        private ushort[] m_byRange;
        private float[] fValue;//存储数据

        private MysqlConnectionPool pool; //连接池
        private String sql = "insert into XTBridge.anchor_force(force1, force2,force3, force4, force5, force6,  datetime) values ({0}, {1}, {2}, {3},{4},{5}, NOW())";



        public Anchor_Syn() {
            Initialize();
        }

        public void Initialize() {
            m_bStart = false;			// the action stops at the beginning
            m_szIP = "192.168.1.3";	// modbus slave IP address
            m_iPort = 502;				// modbus TCP port is 502
            adamModbus = new AdamSocket();
            adamModbus.SetTimeout(1000, 1000, 1000); // set timeout for TCP
            adamModbus.AdamSeriesType = AdamType.Adam6200; // set AdamSeriesType for  ADAM-6217
            m_Adam6000Type = Adam6000Type.Adam6217; // the sample is for ADAM-6217

            m_iAiTotal = AnalogInput.GetChannelTotal(m_Adam6000Type);//channel number
            m_bChEnabled = new bool[m_iAiTotal];
            m_byRange = new ushort[m_iAiTotal];

            fValue = new float[m_iAiTotal];
            pool = new MysqlConnectionPool();


            Connect2Adam();

        }

        private void RefreshChannelRange(int i_iChannel)
        {
            //同步数据范围
            ushort usRange;
            if (adamModbus.AnalogInput().GetInputRange(i_iChannel, out usRange))
                m_byRange[i_iChannel] = usRange;
        }

        private void RefreshChannelEnabled() {
            //同步端口使能
            bool[] bEnabled;
            if (adamModbus.AnalogInput().GetChannelEnabled(m_iAiTotal, out bEnabled))
                Array.Copy(bEnabled, 0, m_bChEnabled, 0, m_iAiTotal);
        }

        private void Connect2Adam() {
            if (adamModbus.Connect(m_szIP, ProtocolType.Tcp, m_iPort))
            {
                m_bStart = true; // starting flag

                RefreshChannelRange(7);
                RefreshChannelRange(6);
                RefreshChannelRange(5);
                RefreshChannelRange(4);
                RefreshChannelRange(3);
                RefreshChannelRange(2);
                RefreshChannelRange(1);
                RefreshChannelRange(0);

                RefreshChannelEnabled();
            }
            else
                Console.WriteLine("Connect to " + m_szIP + " failed", "Error");
        }
    

        private  void readSensorData()
        {
            int iStart = 1;
            int iIdx;
            int[] iData;
         
            if (adamModbus.Modbus().ReadInputRegs(iStart, m_iAiTotal, out iData))
            {
                //读数据
                for (iIdx = 0; iIdx < m_iAiTotal; iIdx++)
                {
                    //NOTE ： 有一个公式换算 需要后面修改
                    fValue[iIdx] = AnalogInput.GetScaledValue(m_Adam6000Type, m_byRange[iIdx], (ushort)iData[iIdx]);
                    //for test
                   // Console.Write(fValue[iIdx] + " ");
                   
                }
               // Console.WriteLine();
            }
        }

       
        public void UpdateData()
        {
            readSensorData();
            //同步数据
            MySqlConnection conn = pool.getConnection();
            sql = String.Format(sql, fValue[0], fValue[1], fValue[2], fValue[3], fValue[4], fValue[5]);
            try {
                MySqlCommand sqlCommand = new MySqlCommand(sql, conn);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex) {
                throw ex;
            }
        }








    }
}
