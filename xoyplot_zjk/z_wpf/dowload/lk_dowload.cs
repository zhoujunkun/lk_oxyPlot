using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xoyplot_zjk.z_wpf.dowload
{
    class lk_dowload
    {
        string filePath, fileName;
        UInt16 fileSize;
        int packedSize = 1024; //包大小1024
        private void openFloder_firmware()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".zmc";
            ofd.Filter = "zjk@MaiCe 固件|*.zmc";
            if (ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
                fileName = texbockFileName.Text = ofd.SafeFileName;
                fileStream = new FileStream(@filePath, FileMode.Open, FileAccess.Read);
                BinaryReader fileRead = new BinaryReader(fileStream);
                byte[] jsonLenBuf = fileRead.ReadBytes(2);
                int jsonLenghts = jsonLenBuf[0] << 8 | jsonLenBuf[1];
                byte[] jsonBuf = fileRead.ReadBytes(jsonLenghts);
                string jsonCfg = Encoding.Default.GetString(jsonBuf);
                VersionApp versionApp = JsonConvert.DeserializeObject<VersionApp>(jsonCfg);
                byte[] binBuf = fileRead.ReadBytes(versionApp.Filesize);
                crc16Module crc_check = new crc16Module();
                UInt16 crc = crc_check.crc16(binBuf);
                if (crc == versionApp.Crc16Modulbus)
                {
                    MessageBox.Show("文件完整，开始升级。。。。")
                }
                fileSize = (UInt16)fileStream.Length;
                textBlockFileSize.Text = fileSize.ToString();
                //此处做你想做的事 ...=ofd.FileName; 

            }

        }
    }
}
