using lk_protecl;
using lk_verify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZSeial;

namespace xoyplot_zjk
{
    class CmdFuncLists
    {
        z_serial lk_serial;
        public void set_serial_control(z_serial serial)
        {
            lk_serial = serial;
        }

        #region  测量
        /// <summary>
        /// 停止测量
        /// </summary>
        public void dist_stop()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_getData);
            frame.id = (byte)(Protecl_typical_cmd.getData_id.stopDist);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }

        /// <summary>
        /// 连续测量
        /// </summary>
        public void dist_continue()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_getData);
            frame.id = (byte)(Protecl_typical_cmd.getData_id.continueDist);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 单次测量
        /// </summary>
        public void dist_once()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_getData);
            frame.id = (byte)(Protecl_typical_cmd.getData_id.onceDist);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }
        #endregion

        #region  QC 标定
        /// <summary>
        /// 第一档切换
        /// </summary>
        /// <param name="buf"></param>
        public void qc_first_switch()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standFirstSwitch);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);

        }
        /// <summary>
        /// 第2档切换
        /// </summary>
        /// <param name="buf"></param>
        public void qc_second_switch()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standSecondSwitch);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);

        }
        /// <summary>
        /// 第三档切换
        /// </summary>
        /// <param name="buf"></param>
        public void qc_third_switch()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.sStandThirdSwitch);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);

        }
        /// <summary>
        /// 获取标定参数
        /// </summary>
        public void qc_get_param()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.getParam);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }

        /// <summary>
        /// 保存第一档标定参数
        /// </summary>
        /// <param name="buf">数据</param>
        public void qc_save_first_param(byte[] buf)
        {
            //发送保存参数
            //发送消息 前基准
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.ifHeadOnly = false;  //含有数据帧
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standParamFirst);  //校准参数
            frame.dataBuf = buf;    //数据帧缓存
            frame.len = (UInt16)buf.Length; //数据帧字节长度
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 保存第2档标定参数
        /// </summary>
        /// <param name="buf">数据</param>
        public void qc_save_second_param(byte[] buf)
        {
            //发送保存参数
            //发送消息 前基准
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.ifHeadOnly = false;  //含有数据帧
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standParamSecond);  //校准参数
            frame.dataBuf = buf;    //数据帧缓存
            frame.len = (UInt16)buf.Length; //数据帧字节长度
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 保存第3档标定参数
        /// </summary>
        /// <param name="buf">数据</param>
        public void qc_save_third_param(byte[] buf)
        {
            //发送保存参数
            //发送消息 前基准
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.ifHeadOnly = false;  //含有数据帧
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standParamThird);  //校准参数
            frame.dataBuf = buf;    //数据帧缓存
            frame.len = (UInt16)buf.Length; //数据帧字节长度
            frame.sendFrame(lk_serial, frame);
        }

        /// <summary>
        /// 复位第一档
        /// </summary>
        public void qc_reset_first_parm()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standParamFirstReset);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 复位第2档
        /// </summary>
        public void qc_reset_second_parm()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standParamSecondReset);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 复位第3档
        /// </summary>
        public void qc_reset_third_parm()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_QC);
            frame.id = (byte)(Protecl_typical_cmd.qc_id.standParamThirdReset);
            frame.ifHeadOnly = true;
            frame.sendFrame(lk_serial, frame);
        }
        #endregion


        #region 固件升级
        /// <summary>
        /// 固件升级命令
        /// </summary>
        public void lk_download_start()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_download);
            frame.id = (byte)(Protecl_typical_cmd.download_id.start);
            frame.ifHeadOnly = true;  //含有数据帧
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 应答
        /// </summary>
        public void lk_download_ack()
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_ack);
            frame.id = (byte)(Protecl_typical_cmd.download_id.start);
            frame.ifHeadOnly = true;  //不含数据帧
            frame.sendFrame(lk_serial, frame);
        }
        /// <summary>
        /// 包发送命令
        /// </summary>
        /// <param name="buf">数据</param>
        /// <param name="packge_cnt">发送包id</param>
        public void lk_firmware_send(byte[] buf,byte packge_cnt)
        {
            tinyFrame frame = new tinyFrame();
            frame.Type = (byte)(Protecl_typical_cmd.type.lk_download);
            frame.id = packge_cnt;
            frame.ifHeadOnly = false;  //含有数据帧
            frame.dataBuf = buf;
            frame.len = (UInt16)buf.Length; //数据帧字节长度
            frame.sendFrame(lk_serial, frame);
        }
        enum Package_enum_ { firstPackage = 1, dataPackaged };
        Package_enum_ package_statu = Package_enum_.firstPackage;
        /// <summary>
        /// firmawre download 应答回调函数
        /// </summary>
        /// <param name="ackID"></param>
        public void lk_firmware_Callback(Protecl_typical_cmd.ack_id ackID)
        {
            Protecl_typical_cmd.ack_id _ack = ackID;
            switch (_ack)
            {
                case Protecl_typical_cmd.ack_id.lk_download_ack:
                    {
                        switch (package_statu)
                        {
                            case Package_enum_.firstPackage:
                                {

                                }
                                break;
                            case Package_enum_.dataPackaged:
                                {

                                }
                                break;
                        }
                    }
                    break;
            }
        }
            #endregion


        }
}
