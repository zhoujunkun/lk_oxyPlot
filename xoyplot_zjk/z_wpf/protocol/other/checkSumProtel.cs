using lk_verify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lk_protecl
{


   public class Lk_other_protecl_cmd
    {
        public enum type { dist_cmd = 1, ack_cmd, param_cmd };
        public enum statu { head = 0, cmd, data, checksum };
        
    }
    public class Lk_other_protecl
    {
        int data_buf_size = 0;
        int dist_size = 6;
        int param_size = 15;
        int ack_size = 6;
        byte[] buf_receive = new byte[20];
        byte[] protecl_recieve = new byte[100];
        int data_count = 0;    //实际数据
        int recieve_count = 0;  //接收字节计数
        Lk_other_protecl_cmd.statu protecl = Lk_other_protecl_cmd.statu.head;
        Lk_other_protecl_cmd.type cmd;
         addGeneralFuncDeglete funcComplete;
         public  delegate void addGeneralFuncDeglete(byte[] buf, Lk_other_protecl_cmd.type cmd);
        public protecl_ack sensor_ack = new protecl_ack();

        public void  other_protecl_init()
        {
            funcComplete = sensor_ack.genralListen;
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
                case Lk_other_protecl_cmd.statu.head:
                    {
                        if (buf == 0xff)
                        {
                            protecl = Lk_other_protecl_cmd.statu.cmd;
                        }
                        else
                        {
                            recieve_count = 0;
                        }
                    }
                    break;
                case Lk_other_protecl_cmd.statu.cmd:
                    {
                        cmd = (Lk_other_protecl_cmd.type)buf;
                        if (cmd == Lk_other_protecl_cmd.type.dist_cmd)
                        {
                            data_buf_size = dist_size;
                            protecl = Lk_other_protecl_cmd.statu.data;
                        }
                        else if (cmd == Lk_other_protecl_cmd.type.ack_cmd)
                        {
                            data_buf_size = ack_size;
                            protecl = Lk_other_protecl_cmd.statu.data;
                        }
                        else if (cmd == Lk_other_protecl_cmd.type.param_cmd)
                        {
                            data_buf_size = param_size;
                            protecl = Lk_other_protecl_cmd.statu.data;
                        }
                        else
                        {
                            protecl = Lk_other_protecl_cmd.statu.head;
                            recieve_count = 0;
                        }
                    }
                    break;
                case Lk_other_protecl_cmd.statu.data:
                    {
                        buf_receive[data_count++] = buf;
                        switch (cmd)
                        {
                            case Lk_other_protecl_cmd.type.dist_cmd:
                                {
                                    if (data_count == dist_size)
                                    {
                                        protecl = Lk_other_protecl_cmd.statu.checksum;
                                    }
                                }
                                break;
                            case Lk_other_protecl_cmd.type.ack_cmd:
                                {
                                    if (data_count == ack_size)
                                    {
                                        protecl = Lk_other_protecl_cmd.statu.checksum;
                                    }
                                }
                                break;
                            case Lk_other_protecl_cmd.type.param_cmd:
                                {
                                    if (data_count == param_size)
                                    {
                                        protecl = Lk_other_protecl_cmd.statu.checksum;
                                    }
                                }
                                break;
                        }


                    }
                    break;
                case Lk_other_protecl_cmd.statu.checksum:
                    {
                        if (rx_checkSum(protecl_recieve, recieve_count))
                        {
                            funcComplete(buf_receive, cmd);
                        }
                        Array.Clear(buf_receive, 0, buf_receive.Length);
                        Array.Clear(protecl_recieve, 0, protecl_recieve.Length);
                        protecl = Lk_other_protecl_cmd.statu.head;
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
