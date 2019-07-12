using lk_protecl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using xoyplot_zjk;
namespace lk_verify
{
    public class protecl_ack: System.Windows.Threading.DispatcherObject
    {
        //consol ack
        private addLogConsle console_showLog;

        public void set_consol_ack(addLogConsle cns)
        { 
            console_showLog = cns;
        }

        private void lk_log(string log)
        {
            console_showLog(log);
        }

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
        enum enum_qc_index_stand { first = 0, second, third };
        public delegate void otherDelegateQcAckrest(byte indes);
        public delegate void otherDelegate();
        public delegate void addLogConsle(string log);
        #region other protecl
        public UInt16 other_sighal { set; get; }
        public UInt16 other_distance { set; get; }
        public UInt16 other_agc { set; get; }
        otherDelegate otherDatedisplay;
        otherDelegateQcAckrest qc_ack_rest;

        public void  add_other_display(otherDelegate dis)
        {
            otherDatedisplay = dis;
        }
        public void add_other_reset_paramAck(otherDelegateQcAckrest restAck)
        {
            qc_ack_rest = restAck;
        }
        /// <summary>
        /// 通用监听协议解析完成函数
        /// </summary>
        /// <param name="lkSensor"></param>
        public void genralListen(byte[] buf, Lk_other_protecl_cmd.type type)
        {
            switch (type)
            {
                case Lk_other_protecl_cmd.type.dist_cmd:
                    {
                        other_sighal = (UInt16)((buf[0] << 8) | buf[1]);
                        other_distance = (UInt16)((buf[2] << 8) | buf[3]);
                        other_agc = (UInt16)((buf[4] << 8) | buf[5]);
                        otherDatedisplay();
                    }
                    break;
                case Lk_other_protecl_cmd.type.ack_cmd:
                    {
                        Protecl_typical_cmd.type frme_type = (Protecl_typical_cmd.type)buf[0];
                        ackId_func(frme_type, buf[1]);
                    }
                    break;
                case Lk_other_protecl_cmd.type.param_cmd:
                    {
                        byte[] qc_param = new byte[15];
                        for (int i = 0; i < 15; i++)
                        {
                            qc_param[i] = buf[i];
                        }
                        // ackId_func(frme_type, buf[3]);
                    }
                    break;
                default:
                    break;
            }

        }

        private void ackId_func(Protecl_typical_cmd.type type, byte id)
        {
            switch (type)
            {
                case Protecl_typical_cmd.type.lk_getData:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_saveParm:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_getParm:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_debug:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_download:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_QC:
                    {
                        Protecl_typical_cmd.qc_id qc_id = (Protecl_typical_cmd.qc_id)id;
                        qc_ack(qc_id);
                    }
                    break;
            }
        }
        /// <summary>
        /// qc ack id
        /// </summary>
        /// <param name="id"></param>
        private void qc_ack(Protecl_typical_cmd.qc_id id)
        {
            switch (id)
            {
                case Protecl_typical_cmd.qc_id.stand_start:
                    {
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standParamFirst:
                    {
                         lk_log("档位1标定保存成功！");
                        lk03_other.ifLkHavedStand[0] = true;    //档位1已经标定标记
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standParamSecond:
                    {
                        lk_log("档位2标定保存成功！");
                        lk03_other.ifLkHavedStand[1] = true;    //档位2已经标定标记
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standParamThird:
                    {
                        lk_log("档位3标定保存成功！");
                        lk03_other.ifLkHavedStand[2] = true;    //档位3已经标定标记
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standParamFirstReset:
                    {
                        qc_ack_rest((byte)enum_qc_index_stand.first);
                        lk_log("档位1复位成功！");
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standParamSecondReset:
                    {
                        qc_ack_rest((byte)enum_qc_index_stand.second);
                        lk_log("档位2复位成功！");
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standParamThirdReset:
                    {
                        qc_ack_rest((byte)enum_qc_index_stand.third);
                         lk_log("档位3复位成功！");
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standFirstSwitch:
                    {
                        lk03_other.curret_stand_statu = 1;
                        lk_log("档位1切换成功！");
                    }
                    break;
                case Protecl_typical_cmd.qc_id.standSecondSwitch:
                    {
                        lk03_other.curret_stand_statu = 2;
                        lk_log("档位2切换成功！");
                    }
                    break;
                case Protecl_typical_cmd.qc_id.sStandThirdSwitch:
                    {
                        lk03_other.curret_stand_statu = 3;
                        lk_log("档位3切换成功！");
                    }
                    break;
                case Protecl_typical_cmd.qc_id.getParam:
                    {
                    }
                    break;


            }

        }

        #endregion


        #region tinyfrme 协议应答
        void genralFunc(byte[] buf, Protecl_typical_cmd.type type)
        {
            switch (type)
            {
                case Protecl_typical_cmd.type.lk_getData:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_getParm:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_QC:
                    {

                    }
                    break;
                case Protecl_typical_cmd.type.lk_debug:
                    {

                    }
                    break;
                    case Protecl_typical_cmd.type.lk_ack:
                    {
                        //Protecl_typical_cmd.ack_id ack_id = (Protecl_typical_cmd.ack_id)(sensor.id);
                        //settingWin.ackCallback(ack_id);
                    }
                    break;
                case Protecl_typical_cmd.type.lk_saveParm:
                    {

                    }
                    break;
                default:
                    break;
            }

        }

        public void tinyFram_ack(Protecl_typical_cmd.ack_id ackID)
        {
            Protecl_typical_cmd.ack_id _ack = ackID;
            switch (_ack)
            {
                case Protecl_typical_cmd.ack_id.lk_download_ack:
                    {
                        //ifBeginUpdata = true;
                        //timerTOA.Stop();
                        //switch (package_statu)
                        //{
                        //    case Package_enum_.firstPackage:
                        //        {
                        //            packageCntTrans = 0;
                        //            packageBegin();
                        //            package_statu = Package_enum_.dataPackaged;
                        //        }
                        //        break;
                        //    case Package_enum_.dataPackaged:
                        //        {
                        //            if (packageSend() == true)
                        //            {

                        //                timerTOA.Start();
                        //            }
                        //            else
                        //            {
                        //                MessageBox.Show("send succeed");
                        //            }
                        //        }
                        //        break;
                        //}
                    }
                    break;
                case Protecl_typical_cmd.ack_id.lk_debug_ack:
                    {

                    }
                    break;
            }
        }
        #endregion
    }
}
