using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lk_verify;
using System.Windows;
using lk_protecl;
using ZSeial;
using System.Threading;
using System.Windows.Controls;

namespace lk_tool
{
    /**   上位机               传感器
     *    sendAck --->
     * 
     * */
    public class download
    {
        public string firemwar_name { set; get; }
        UInt16 fileSize { set; get; }
        ProgressBar progressBar_dowload;
        public string firemwar_path { set; get; }
        public string firemwar_size { set; get; }
        int packetCnt { set; get; }   //固件包大小
        CRC lk_crc = new CRC();
        tinyFrame lk_tinyframe = new tinyFrame();
         z_serial serial;
        private FileStream fileStream;
        
        int packedSize = 1024; //包大小1024
        UInt16 binCrc;  //binCrc校验码
        public void set_control(z_serial lserial, ProgressBar prosbar)
        {
            serial = lserial;
            progressBar_dowload = prosbar;
        }
        byte[] binBuf;  //bin文件存放
        public bool open_firmware()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".zmc";
            ofd.Filter = "zjk@MaiCe 固件|*.zmc";
            if (ofd.ShowDialog() == true)
            {
                firemwar_path = ofd.FileName;
                firemwar_name = ofd.SafeFileName;
                fileStream  = new FileStream(firemwar_path, FileMode.Open, FileAccess.Read);
                BinaryReader fileRead = new BinaryReader(fileStream);
                byte[] jsonLenBuf = fileRead.ReadBytes(2);
                int jsonLenghts = jsonLenBuf[0] << 8 | jsonLenBuf[1];
                byte[] jsonBuf = fileRead.ReadBytes(jsonLenghts);
                string jsonCfg = Encoding.Default.GetString(jsonBuf);
                versionApp = JsonConvert.DeserializeObject<VersionApp>(jsonCfg);
                binBuf = fileRead.ReadBytes(versionApp.Filesize);
                fileSize = Convert.ToUInt16(versionApp.Filesize);
                firemwar_size = fileSize.ToString();
                packetCnt = (fileSize % packedSize) == 0 ? (int)(fileSize / packedSize) : (int)(fileSize / packedSize) + 1;
                CRC crc_check = new CRC();
                binCrc = crc_check.crc16(binBuf);
                if (binCrc == versionApp.Crc16Modulbus)
                {
                    return true;
                }
                else
                { return false; }
                
            }
            return false;

        }

        

        enum Package_enum_ { firstPackage = 1, dataPackaged };
        Package_enum_ package_statu = Package_enum_.firstPackage;
        /// <summary>
        /// 时间结束时触发的方法
        /// </summary>
        private void endtime(Object sender, EventArgs e)
        {
            MessageBox.Show("time out");
        }
        VersionApp versionApp;
        /// <summary>
        /// 首包帧
        /// </summary>
        /// <returns></returns>
        private byte[] packageBegin()
        {

            byte[] firstPackage = new byte[versionApp.TimeCreate.Length + 5];   //time+size(2)+crc(file 2)
            /*add file name to first package*/
            for (int i = 0; i < firemwar_name.Length; i++)
            {
                firstPackage[i] = (byte)firemwar_name.ToCharArray()[i];
            }
            firstPackage[firemwar_name.Length + 1] = (byte)(fileSize >> 8);
            firstPackage[firemwar_name.Length + 2] = (byte)(fileSize & 0xff);
            firstPackage[versionApp.TimeCreate.Length + 3] = (byte)(binCrc >> 8);
            firstPackage[versionApp.TimeCreate.Length + 4] = (byte)(binCrc & 0xff);
            sendPakagedFrame(firstPackage, 0);
            return firstPackage;
        }
        int packageCntTrans = 0;    //发送包计数
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool packageSend()
        {
            /* data: 1024 bytes */
            byte[] packaedTosend;
            /* send packets with a cycle until we send the last byte */
            int filePackedSize = binBuf.Length - packageCntTrans++ * packedSize;
            if (packageCntTrans == (packetCnt + 1))
                return false;
            if (filePackedSize < packedSize)   //小于packedSize 
            {
                packaedTosend = new byte[filePackedSize];
                Array.Copy(binBuf, packedSize * (packageCntTrans - 1), packaedTosend, 0, filePackedSize);
            }
            else
            {
                packaedTosend = new byte[packedSize];
                Array.Copy(binBuf, packedSize * (packageCntTrans - 1), packaedTosend, 0, packedSize);
            }

            /* calculate packetNumber */
            progressBar(packageCntTrans);
            sendPakagedFrame(packaedTosend, (byte)packageCntTrans);
            return true;
        }

        //进度条
        private void progressBar(int count)
        {
            Thread thread = new Thread(new ThreadStart(() =>   //多线程
            {
                progressBar_dowload.Dispatcher.BeginInvoke((ThreadStart)delegate
                {
                    progressBar_dowload.Value = count;
                });
            }));
            thread.Start();
        }


        byte packetNumber = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="buff_len"></param>
        /// <param name="id"></param>
        public void sendPakagedFrame(byte[] buff, byte id)
        {

        }

        private void downloadAck()
        {

        }
        System.Timers.Timer timerTOA;

        /// <summary>
        /// 
        /// </summary>
        public void upload()
        {
            timerTOA = new System.Timers.Timer();
            timerTOA.Interval = 2000; //超时时间 ms
            timerTOA.AutoReset = false;
            timerTOA.Enabled = false;
            timerTOA.Elapsed += new System.Timers.ElapsedEventHandler(endtime);
            downloadStart();    //此处应该有等待响应转换
            timerTOA.Start(); //定时器用于超时处理
        }

        private void downloadStart()
        {

        }

        public bool ifBeginUpdata { set; get; }     //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ackID"></param>
        public void ackCallback(Protecl_typical_cmd.ack_id ackID)
        {
            Protecl_typical_cmd.ack_id _ack = ackID;
            switch (_ack)
            {
                case Protecl_typical_cmd.ack_id.lk_download_ack:
                    {
                        ifBeginUpdata = true;
                        timerTOA.Stop();
                        switch (package_statu)
                        {
                            case Package_enum_.firstPackage:
                                {
                                    packetNumber = 0;
                                    packageBegin();
                                    package_statu = Package_enum_.dataPackaged;
                                }
                                break;
                            case Package_enum_.dataPackaged:
                                {
                                    if (packageSend() == true)
                                    {
                                        timerTOA.Start();
                                    }
                                    else
                                    {
                                        MessageBox.Show("send succeed");
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

        }

        public class VersionApp
        {
            public string Name { set; get; }
            public string Version { set; get; }
            public int Filesize { set; get; }
            public string TimeCreate { set; get; }
            public int Crc16Modulbus { set; get; }
        }
    }
}
