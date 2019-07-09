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
        public enum type { lk_getData = 1, lk_saveParm, lk_getParm, lk_QC = 6,lk_debug = 7,lk_download = 0xfe ,lk_ack};
        public enum getData_id { onceDist = 1, continueDist, stopDist };
        public enum ack_id { lk_getData_ack = 1, lk_saveParm_ack, lk_getParm_ack, lk_QC_ack = 6, lk_debug_ack = 7, lk_download_ack = 0xfe };
        public enum getParm_id { all = 1 };
        public enum saveParm_id { saveBaud = 1, saveRedLight, frontOrBase, autoMel };
        public enum qc_id { stand_start = 1, standParamFirst, standParamSecond, standParamThird, standParamFirstReset, standParamSecondReset, standParamThirdReset, standFirstSwitch, standSecondSwitch, sStandThirdSwitch, getParam };  //标定开始
        public enum download_id { start = 1 };
    }
}
