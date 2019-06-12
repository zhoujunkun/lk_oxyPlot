using System;
using System.Runtime.InteropServices;
using System.Text;
namespace zLkControl
{
    public class SensorDataItem
    {
        public byte type { get; set; }
        public byte id { get; set; }
        public UInt16 len { get; set; }
        public string Frame;   //完整接收帧字符串
        public bool isCMDReceived { get; set; }
        public bool isReceveSucceed { get; set; }
        public byte[] buf;
        // public byte[] dataBuff;

        public StructHelper structHelper = new StructHelper();

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //  stringBuilder.AppendLine(string.Format("dist={0,5:0.000}", this.dist));
            return stringBuilder.ToString();
        }
        public string fromHexToString(byte[] hex)
        {
            string hexStr = null;
            foreach (byte str in hex)
            {
                hexStr += string.Format("{0:X2} ", str);
            }
            return hexStr;
        }
        public string stringByteToString(byte byt)
        {
            return string.Format("{0:X2} ", byt);
        }
    }

    public  class StructHelper
    {
        /// <summary>
        /// byte数组转换目标结构体       
        /// </summary>
        /// <typeparam name="bytes"></typeparam>
        /// <param name="type">目标结构体类型</param>
        /// <returns>目标结构体</returns>
        public  T ByteToStruct<T>(byte[] DataBuff_) where T : struct
        {
            Type t = typeof(T);
            //得到结构体大小
            int size = Marshal.SizeOf(t);
            //数组长度小于结构体大小
            if (size > DataBuff_.Length)
            {
                return default(T);
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte 数组copy到分配好的内存空间内
            Marshal.Copy(DataBuff_, 0, structPtr, size);
            //将内存空间转换为目标结构体
            T obj = (T)Marshal.PtrToStructure(structPtr, t);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        /// <summary>
        /// 结构体转byte数组
        /// </summary>
        /// <param name="objestuct">结构体</param>
        /// <returns>byte 数组</returns>
        public  byte[] StructToByte(object objestuct)
        {
            //得到结构体大小
            int size = Marshal.SizeOf(objestuct);
            //创建byte 数组
            byte[] bytes = new byte[size];
            //分配结构体大小的空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体copy到分配好的内存空间内
            Marshal.StructureToPtr(objestuct, structPtr, false);
            //从内存空间 copy到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }


        public class LKSensorCmd
        {
            //id 类型命令分配
            /*
            ,---------+-----------+------------+-----------+------------+- - - -+-------------,
            | PROUDCT | BAUD_RATE | LIMIR_TRIG | RED_LIGHT | FRONT_BASE |
            |    1    |     4     |      2     |     1     |      1     | ...   |          | <- size (bytes)
            '---------+-----------+------------+-----------+------------+- - - -+-------------'         
            */
            public enum FRAME_TYPE { DataGet = 1, ParmsSave, ParamGet, Upload, ACK, QC, Erro };
            public enum FRAME_GetDataID { DistOnce = 1, DistContinue, DistStop };
            public enum FRAME_AckID { downLoadBegin = 1, upload };
            public enum FRAME_GetParamID { ParamAll = 1 };
            public enum FRAME_SpeeCtlID { START = 1, STOP };
            public enum FRAME_QCcmdID { stand_start = 1, StandParamFirst, StandParamSecond, StandParamThird, StandParamFirstReset, StandParamSecondReset, StandParamThirdReset, GetParam };  //标定开始
            public enum FRAME_ParmSaveID { BarudSave = 1, RedLightSave, FrontOrBase, AutoMel };

            public enum FRAME_FRONT_BASE { BASE = 0, FRONT };
            public enum FRAME_RED_LIGHT { OFF = 0, ON };
            public enum FRAME_AUTO_MEL { OFF = 0, ON };

            public bool isbase;  //后基准标记
            public bool isRedLedOn; //红外激光是否打开
            public int parmBarudIndex;   //参数波特率选择
            public bool isSensosAutoCel;   //是否自动开机测量

            public byte parmBarudRateByteSize = 4;
            public byte parmProductByteSize = 1;
            public byte parmLimitTrigByteSize = 1;
            public byte parmRedLightByteSize = 1;
            public byte parmFrontBaseByteSize = 1;

            public byte parmStandDistByteSize = 1;

            public byte stand_distance { set; get; }   //b标定距离

        }
    }

}
