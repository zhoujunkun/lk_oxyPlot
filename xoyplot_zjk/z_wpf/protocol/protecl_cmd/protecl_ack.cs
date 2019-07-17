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
        public LK03QC lk03_qc = new LK03QC();
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
        public delegate void addUsrMydelege();
        public delegate void addUsrDataMydelege(byte[]buf);
        public UInt16 other_sighal { set; get; }
        public UInt16 other_distance { set; get; }
        public UInt16 other_agc { set; get; }
        private static addLogConsle user_console;  //显示字符
        otherDelegateQcAckrest qc_ack_rest;
        addUsrMydelege usr_display;
        addUsrDataMydelege sensor_parma_refresh;
        public void  add_usr_console(addLogConsle dis)
        {
            user_console = dis;
        }
        public void add_usr_param_refresh(addUsrDataMydelege dis)
        {
            sensor_parma_refresh = dis;
        }
        public void add_usr_display(addUsrMydelege ds)
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
                        Protecl_typical_cmd.programer_ack_id p_ackid=(Protecl_typical_cmd.programer_ack_id)msg.frame_id;
                        proramer_ack_id(p_ackid,buf);
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
                        UInt16 distance = (UInt16)(buf[0] << 8 | buf[1]);
                        user_console(distance.ToString());
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.dist_stop_ack:
                    {
                        user_console("停止测量成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.get_paramAll_base: //
                    {
                        sensor_parma_refresh(buf);
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
                        user_console("波特率设置成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_frontSwich_ack:
                    {
                        user_console("设置前开关量成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_backSwich_ack:
                    {
                        user_console("设置后开关量成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_distBase_ack:
                    {
                        user_console("设置基准成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_powerOn_mode_ack:
                    {
                        user_console("开机自动运行成功！");
                    }
                    break;
                case Protecl_typical_cmd.user_ack_id.cfgParam_outData_freq_ack:
                    {
                        user_console("输出频率设置成功");
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

        /// <summary>
        /// 开发人员应答
        /// </summary>
        /// <param name="id"></param>
        /// <param name="buf"></param>
        public void proramer_ack_id(Protecl_typical_cmd.programer_ack_id id, byte[] buf)
        {
            switch(id)
            {
                case Protecl_typical_cmd.programer_ack_id.qc_get_param_ack:
                    {

                    }break;
                case Protecl_typical_cmd.programer_ack_id.qc_standFirst_switch_ack:
                    {
                        lk03_qc.curret_stand_statu = 1;
                        user_console("档位1切换成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standSecond_switch_ack:
                    {
                        lk03_qc.curret_stand_statu = 2;
                        user_console("档位2切换成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standthird_switch_ack:
                    {
                        lk03_qc.curret_stand_statu = 3;
                        user_console("档位3切换成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standFirst_reset_ack:
                    {
                        user_console("档位1复位成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standSecond_reset_ack:
                    {
                        user_console("档位2复位成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standthird_reset_ack:
                    {
                        user_console("档位3复位成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standFirst_save_ack:
                    {
                        user_console("档位1保存成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standSecond_save_ack:
                    {
                        user_console("档位2保存成功");
                    }
                    break;
                case Protecl_typical_cmd.programer_ack_id.qc_standthird_save_ack:
                    {
                        user_console("档位3保存成功");
                    }
                    break;
            }

        }

        #endregion


        #region tinyfrme 协议应答
        
        
        #endregion
    }
}
