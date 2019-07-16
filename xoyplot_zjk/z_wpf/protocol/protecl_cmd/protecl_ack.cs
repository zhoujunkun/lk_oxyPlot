using lk_protecl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using xoyplot_zjk;
using static lk_protecl.tinyFrame;

namespace lk_verify
{
    public class protecl_ack: System.Windows.Threading.DispatcherObject
    {
        //QC
        public LK03QC lk03_other = new LK03QC();
        public class LK03QC
        {
            public byte first_index=0;
            public byte second_index = 1;
            public byte third_index = 2;
            public byte curret_stand_statu;    //当前标定的档位，按照档位切换对应来确定
            public bool[] ifStandSwitchComplete = new bool[3];    //档位切换完成应答
            public bool[] ifLkHavedStand = new bool[3];   //是否已经标定过
        }

        public double sensor_distance { set; get; }


        #region other protecl
        enum enum_qc_index_stand { first = 0, second, third };
        public delegate void otherDelegateQcAckrest(byte indes);
        public delegate void addLogConsle(string log);
        public delegate void addUsrDisplayMydelege();
        public UInt16 other_sighal { set; get; }
        public UInt16 other_distance { set; get; }
        public UInt16 other_agc { set; get; }
        private static addLogConsle user_console;  //显示字符
        otherDelegateQcAckrest qc_ack_rest;
        addUsrDisplayMydelege usr_display;
        public void  add_usr_console(addLogConsle dis)
        {
            user_console = dis;
        }

        public void add_usr_display(addUsrDisplayMydelege ds)
        {
            usr_display = ds;
        }
        public void add_other_reset_paramAck(otherDelegateQcAckrest restAck)
        {
            qc_ack_rest = restAck;
        }
        /// <summary>
        /// 通用监听协议解析完成函数
        /// </summary>
        /// <param name="lkSensor"></param>
        public void genralListen(byte[] buf, _message msg)
        {
            Protecl_typical_cmd.ctl_type type_sel = (Protecl_typical_cmd.ctl_type)msg.type;
            switch (type_sel)
            {
                case Protecl_typical_cmd.ctl_type.usr_ack:
                    {
                        Protecl_typical_cmd.user_ack_id ack_id = (Protecl_typical_cmd.user_ack_id)msg.frame_id;
                        user_ack_id(ack_id, buf);
                    }
                    break;
                case Protecl_typical_cmd.ctl_type.programer_ack:
                    {

                    }
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// usr_ack 通用应答
        /// </summary>
        /// <param name="id"> </param>
        private void user_ack_id(Protecl_typical_cmd.user_ack_id id,byte[] buf  )
        {
            switch (id)
            {
                case Protecl_typical_cmd.user_ack_id.dist_base://
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_continue_ack:
                    {
                        sensor_distance = (UInt16)(buf[0] << 8 | buf[1]);
                        //user_console(sensor_distance.ToString());
                        usr_display();
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_once_ack:
                    {
                  
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_stop_ack:
                    {
                     
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.get_paramAll_base: //
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_baudRate_ack:
                    {
      
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_frontSwich_ack:
                    {
             
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_backSwich_ack:
                    {
   
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_disBase_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.getParam_powerOn_mode_ack:
                    {

                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_all:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_baudRate_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_frontSwich_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_backSwich_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_distBase_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_powerOn_mode_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_outData_freq_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.system_boot_paramReset_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.system_boot_firmware_ctl_ack:
                    {
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.system_boot_firmware_pakage_ack:
                    {
                    }
                    break;


            }

        }

        #endregion


        #region tinyfrme 协议应答
        
        
        #endregion
    }
}
