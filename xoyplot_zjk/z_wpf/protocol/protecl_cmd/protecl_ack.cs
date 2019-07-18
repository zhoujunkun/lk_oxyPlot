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
        public delegate void addUsrAckMydelege(Protecl_typical_cmd.user_ack_id id, byte[] buf);
        public delegate void addProgramerAckMydelege(Protecl_typical_cmd.programer_ack_id id, byte[] buf);


        addUsrAckMydelege usr_ack_id;
        addProgramerAckMydelege programer_ack_id;
        public void  add_usr_ackId(addUsrAckMydelege dis)
        {
            usr_ack_id = dis;
        }
        public void add_programer_ackId(addProgramerAckMydelege dis)
        {
            programer_ack_id = dis;
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
                        usr_ack_id(ack_id, buf);
                    }
                    break;
                case Protecl_typical_cmd.ctl_type.programer_ack:
                    {
                        Protecl_typical_cmd.programer_ack_id p_ackid=(Protecl_typical_cmd.programer_ack_id)msg.frame_id;
                        programer_ack_id(p_ackid,buf);
                    }
                    break;
                default:
                    break;
            }

        }

        #region tinyfrme 协议应答
        
        
        #endregion
    }
}
