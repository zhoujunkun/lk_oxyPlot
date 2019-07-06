using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LK_PROTECL
{
    public class LK_CHECKSUM_PROTEL
    {
        int data_buf_size = 0;
        int dist_size = 6;
        int param_size = 15;
        int ack_size = 6;
        public enum lk_frameRv_type { dist_cmd = 1, ack_cmd, param_cmd };
        enum FRAME_PROTECL { head = 0, cmd, data, checksum };
        byte[] buf_receive = new byte[20];
        byte[] protecl_recieve = new byte[100];
        int data_count = 0;    //实际数据
        int recieve_count = 0;  //接收字节计数
        FRAME_PROTECL protecl = FRAME_PROTECL.head;
          lk_frameRv_type cmd;
         addGeneralFuncDeglete funcComplete;
         public  delegate void addGeneralFuncDeglete(byte[] buf, lk_frameRv_type cmd);

        public void addGeneralFun(addGeneralFuncDeglete func)
        {
            funcComplete = func;
        }
        /// <summary>
        /// 协议解析
        /// </summary>
        /// <param name="buf"></param>
        public void protecl_ansys(byte buf)
        {
            protecl_recieve[recieve_count++] = buf;
            switch (protecl)
            {
                case FRAME_PROTECL.head:
                    {
                        if (buf == 0xff)
                        {
                            protecl = FRAME_PROTECL.cmd;
                        }
                        else
                        {
                            recieve_count = 0;
                        }
                    }
                    break;
                case FRAME_PROTECL.cmd:
                    {
                        cmd = (lk_frameRv_type)buf;
                        if (cmd == lk_frameRv_type.dist_cmd)
                        {
                            data_buf_size = dist_size;
                            protecl = FRAME_PROTECL.data;
                        }
                        else if (cmd == lk_frameRv_type.ack_cmd)
                        {
                            data_buf_size = ack_size;
                            protecl = FRAME_PROTECL.data;
                        }
                        else if (cmd == lk_frameRv_type.param_cmd)
                        {
                            data_buf_size = param_size;
                            protecl = FRAME_PROTECL.data;
                        }
                        else
                        {
                            protecl = FRAME_PROTECL.head;
                            recieve_count = 0;
                        }
                    }
                    break;
                case FRAME_PROTECL.data:
                    {
                        buf_receive[data_count++] = buf;
                        switch (cmd)
                        {
                            case lk_frameRv_type.dist_cmd:
                                {
                                    if (data_count == dist_size)
                                    {
                                        protecl = FRAME_PROTECL.checksum;
                                    }
                                }
                                break;
                            case lk_frameRv_type.ack_cmd:
                                {
                                    if (data_count == ack_size)
                                    {
                                        protecl = FRAME_PROTECL.checksum;
                                    }
                                }
                                break;
                            case lk_frameRv_type.param_cmd:
                                {
                                    if (data_count == param_size)
                                    {
                                        protecl = FRAME_PROTECL.checksum;
                                    }
                                }
                                break;
                        }


                    }
                    break;
                case FRAME_PROTECL.checksum:
                    {
                        if (rx_checkSum(protecl_recieve, recieve_count))
                        {
                            funcComplete(buf_receive, cmd);
                        }
                        Array.Clear(buf_receive, 0, buf_receive.Length);
                        Array.Clear(protecl_recieve, 0, protecl_recieve.Length);
                        protecl = FRAME_PROTECL.head;
                        recieve_count = 0;
                        data_count = 0;
                    }
                    break;
            }



        }
        /// <summary>
        /// 校验和
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="lens"></param>
        /// <returns></returns>
        private bool rx_checkSum(byte[] buf, int lens)
        {
            byte ret = 0;
            for (int i = 0; i < lens; i++)
            {
                ret += buf[i];
            }
            if (ret == 0xff)
            {
                return true;
            }
            else return false;
        }

      


    }
}
