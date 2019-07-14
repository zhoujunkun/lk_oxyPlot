using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lk_verify
{
   public class Protecl_typical_cmd
    {
        //id 类型命令分配
        /*
        ,---------+-----------+------------+-----------+------------+- - - -+-------------,
        | PROUDCT | BAUD_RATE | LIMIR_TRIG | RED_LIGHT | FRONT_BASE |
        |    1    |     4     |      2     |     1     |      1     | ...   |          | <- size (bytes)
        '---------+-----------+------------+-----------+------------+- - - -+-------------'         
        */
         public enum sensor_func {lk_base=0, baudRate, frontSwich, backSwich, disBase ,powerOn_mode, outData_freq };
        int param_get_base = 0x00;
        int param_set_base = 0x00;
        int param_get_ack_base = 0x10;
        int param_set_base_base = 0x40;



        public enum ctl_type { user_dist_ctl = 0x01,     usr_paramCfg_get=0x02, user_paramCfg_set=0x03,
                               usr_ack = 0x10,
                               system_boot_firmware_ctl=0x20, system_boot_firmware_pakage,
                                system_boot_param = 0x30,
                               programer_ctl = 0xe0,
                               programer_ack = 0xf0,
        };
        public enum dist_ctl_id { dist_continue = 0x01, dist_once=0x02, dist_stop=0x03};
        public enum user_ack_id { dist_base=0,          dist_continue_ack, dist_once_ack, dist_stop_ack,
                                  get_paramAll_base= 0x10, getParam_baudRate_ack, getParam_frontSwich_ack, getParam_backSwich_ack, getParam_disBase_ack, getParam_powerOn_mode_ack,
                                  cfgParam_all= 0x40, cfgParam_baudRate_ack, cfgParam_frontSwich_ack, cfgParam_backSwich_ack, cfgParam_distBase_ack, cfgParam_powerOn_mode_ack, cfgParam_outData_freq_ack,
                                  system_boot_param_ack = 0xf0, system_boot_firmware_ctl_ack, system_boot_firmware_pakage_ack,
        };
        public enum paramCfg_get_id { lk_all = 0x00, baudRate, frontSwich, backSwich, disBase, powerOn_mode, outData_freq };
        public enum paramCfg_set_id { lk_all = 0x00, baudRate, frontSwich, backSwich, disBase, powerOn_mode, outData_freq };
        public enum usr_boot_ctl_id { usr_reset=0x01,usr_download };

        public enum programer_id { qc_get_param = 0x00, qc_standFirst_switch, qc_standSecond_switch, qc_standthird_switch, qc_standFirst_reset, qc_standSecond_reset, qc_standthird_reset, qc_standFirst_save, qc_standSecond_save, qc_standthird_save };  
        public enum firmware_ctl_id { firmware_begin = 1 };      


    }
}
