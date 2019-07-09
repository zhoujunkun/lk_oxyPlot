using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lk_verify;
namespace lk_protecl
{
        /*
    ,-----+-----+-----+------+------------+- - - -+-------------,
    | SOF | ID  | LEN | TYPE | HEAD_CKSUM | DATA  | DATA_CKSUM  |
    | 0-1 | 1-4 | 1-4 | 1-4  | 0-4        | ...   | 0-4         | <- size (bytes)
    '-----+-----+-----+------+------------+- - - -+-------------'

    SOF ......... start of frame, usually 0x01 (optional, configurable)
    ID  ......... the frame ID (MSb is the peer bit)
    LEN ......... number of data bytes in the frame
    TYPE ........ message type (used to run Type Listeners, pick any values you like)
    HEAD_CKSUM .. header checksum

    DATA ........ LEN bytes of data
    DATA_CKSUM .. data checksum (left out if LEN is 0)
 * */
   public class tinyFrame
    {
        public tinyFrame()
        {
            ifHeadOnly = false;
            Type = 0xff;
            id = 0;
            isRsponse = true;

            //
            state = "sof";
            sofByte = 0x01;   //start of frame, usually 0x01 (optional, configurable)
            idSize = 1;   //
            lenSize = 2;
            typeSize = 1;
        }

        private byte next_id;
        public bool ifHeadOnly{set;get;}
        public byte Type { set; get; }
        public byte id { set; get; }
        public UInt16 len { set; get; }
        public bool isRsponse { set; get; }
        public byte[] dataBuf;
        private Queue<byte> sendBuf = new Queue<byte>();
        CRC crc_checksum = new CRC();
        byte sofbyte = 0x01;   //起始
        /// <summary>
        /// 发送数据帧
        /// </summary>
        /// <param name="z_Serial"></param>
        public void sendFrame(ZSeial.z_serial z_Serial,tinyFrame frame)
        {

            frame.id = frame.isRsponse ? frame.id : getNextID();
            //add listener       
            frame.sendBuf.Enqueue(frame.sofbyte);
            frame.sendBuf.Enqueue(frame.id);
            frame.sendBuf.Enqueue((byte)(frame.len >> 8));
            frame.sendBuf.Enqueue((byte)(frame.len));
            frame.sendBuf.Enqueue(frame.Type);
            crc_checksum.crcReset();
            UInt16 head_crc = crc_checksum.crc16(frame.sendBuf.ToArray());
            byte high_crc = (byte)(head_crc >> 8);
            byte low_crc = (byte)head_crc;
            frame.sendBuf.Enqueue(high_crc);
            frame.sendBuf.Enqueue(low_crc);
            if (!frame.ifHeadOnly)
            {
                byte[] data = new byte[len];
                for (int i = 0; i < len; i++)
                {
                    data[i] = dataBuf[i];
                    frame.sendBuf.Enqueue(dataBuf[i]);
                }
                crc_checksum.crcReset();
                UInt16 data_crc = crc_checksum.crc16(data);
                byte high_data_crc = (byte)(data_crc >> 8);
                byte low_data_crc = (byte)data_crc;

                frame.sendBuf.Enqueue(high_data_crc);
                frame.sendBuf.Enqueue(low_data_crc);
            }
            else
            {
                len = 0;
            }
            byte[] re_buf = sendBuf.ToArray();
            frame.sendBuf.Clear();
            z_Serial.sendBuf(re_buf);
        }
        public byte getNextID()
        {
            if (next_id >= 125)
            {
                next_id = 0;
            }
            return ++next_id;
        }

        //recieve

        static int MAX_TYPE_SIZE = 10;
        static int MAX_GENERAL_SIZE = 10;
        string state;
        int partlen;
        int cksum;
        byte peer;
        _idListenr idListenr = new _idListenr();
        _generalListenr[] generalListenr = new _generalListenr[MAX_GENERAL_SIZE];
        _typeListenr[] typeListener = new _typeListenr[MAX_TYPE_SIZE];
        public delegate void myDelegate(byte[] rxbuf, _message msg);
         _message msg = new _message();
        public Queue<byte> headBuf = new Queue<byte>();
        public Queue<byte> rxDataBuf = new Queue<byte>();
        public Queue<byte> frameBuf = new Queue<byte>();
        public int idSize { set; get; }
        public int lenSize { set; get; }
        public int typeSize { set; get; }
        public byte sofByte { set; get; }
        public void resetParser()
        {
            state = "sof";
            partlen = 0;
            cksum = 0;
            headBuf.Clear();
            rxDataBuf.Clear();
            frameBuf.Clear();
        }

       public  struct _message
        {
            public byte frame_id;   // the frame ID (MSb is the peer bit)
            public UInt16 len;  //number of data bytes in the frame
            public byte type; //message type (used to run Type Listeners, pick any values you like)

        }

        //添加id listen
        struct _idListenr
        {
            public int id;
            public myDelegate Func;
        };

        struct _generalListenr
        {
            public myDelegate Func;
        };
        struct _typeListenr
        {
            public int type;
            public myDelegate Func;
        };
        //数据解析
        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="data"></param>
        public void AcceptByte(byte data)
        {
            frameBuf.Enqueue(data);
            switch (state)
            {
                case "sof":
                    {
                        if (data == sofByte)
                        {
                            beginFrame();
                            headBuf.Enqueue(data);
                        }

                    }
                    break;
                case "id":
                    {
                        headBuf.Enqueue(data);
                        msg.frame_id = (byte)((msg.frame_id << 8) | data);
                        if (++partlen == idSize)
                        {
                            state = "len";
                            partlen = 0;
                        }
                    }
                    break;
                case "len":
                    {

                        headBuf.Enqueue(data);
                        msg.len = (UInt16)((msg.len << 8) | data);
                        if (++partlen == lenSize)
                        {

                            state = "type";
                            partlen = 0;
                        }
                    }
                    break;
                case "type":
                    {
                        headBuf.Enqueue(data);
                        msg.type = (byte)((msg.type << 8) | data);
                        if (++partlen == typeSize)
                        {
                            state = "headcksum";
                            partlen = 0;
                        }
                    }
                    break;
                case "headcksum":
                    {
                        cksum = (cksum << 8) | data;
                        if (++partlen == crc_checksum.size)
                        {
                            byte[] buf = headBuf.ToArray();
                            UInt32 crc = 0;
                            crc_checksum.crcReset();
                            crc = crc_checksum.crc16(buf);
                            if (crc == cksum)
                            {
                                if (msg.len == 0)  //无数据包
                                {
                                    resetParser();
                                    //lkSensor.Frame = lkSensor.fromHexToString(headBuf.ToArray());
                                    //lkSensor.buf = dataBuf.ToArray();
                                    //handleReceived(lkSensor);
                                }
                                else
                                {
                                    state = "data";
                                }
                            }
                            else
                            {
                                resetParser();
                                break;
                            }
                        }

                    }
                    break;
                case "data":
                    {
                        rxDataBuf.Enqueue(data);
                        if (++partlen == msg.len)
                        {
                            state = "datachksum";
                            partlen = 0;
                            cksum = 0;
                        }
                    }
                    break;
                case "datachksum":
                    {
                        cksum = (cksum << 8) | data;
                        if (++partlen == crc_checksum.size)
                        {
                            byte[] buf = dataBuf.ToArray();
                            UInt16 crc = 0;
                            crc_checksum.crcReset();
                            crc = crc_checksum.crc16(buf);
                            if (crc == cksum)
                            {
                                //lkSensor.isReceveSucceed = true;
                                //lkSensor.Frame = lkSensor.fromHexToString(frameBuf.ToArray());
                                //lkSensor.buf = dataBuf.ToArray();
                                //handleReceived(lkSensor);
                            }
                            else
                            {
                             //   lkSensor.isReceveSucceed = false;
                            }
                            cksum = 0;
                            partlen = 0;
                            rxDataBuf.Clear();
                            resetParser();
                        }
                    }
                    break;
            }
        }
        public void beginFrame()
        {
            this.state = "id";
            partlen = 0;
            msg.frame_id = 0;
            msg.len = 0;
            msg.type = 0;
            this.cksum = 0;
        }
        //添加回调函数
        public void handleReceived(byte[]rxbuf, _message msg)
        {
                if (msg.frame_id == idListenr.id)
                {
                    idListenr.Func.Invoke(rxbuf,msg);     //回调函数运行
                }
                for (int i = 0; i < MAX_TYPE_SIZE; i++)
                {
                    if (msg.type == typeListener[i].type)
                    {
                        typeListener[i].Func.Invoke(rxbuf, msg);
                    }
                }
                for (int i = 0; i < MAX_GENERAL_SIZE; i++)
                {
                    if (generalListenr[i].Func == null)
                    {
                        return;
                    }
                    else
                    {
                        generalListenr[i].Func.Invoke(rxbuf, msg);
                    }
                }
            


        }


    }
}
