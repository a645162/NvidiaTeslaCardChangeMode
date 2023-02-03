using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvidiaTeslaCardChangeMode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NVIDIA Tesla 计算卡TTC切换WDDM工具");


            RegistryKey hk_lm = Registry.LocalMachine;
            RegistryKey hk_gpu_list =
                hk_lm.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}",
                    true);

            //Console.WriteLine(hk_gpu_list.SubKeyCount);
            string[] subnameStrings = hk_gpu_list.GetSubKeyNames();

            int max_count = 0;
            List<(string, string)> gpuNameList = new List<(string, string)>();

            for (int i = 0; i < subnameStrings.Length; i++)
            {
                String keyStr = max_count.ToString();
                while (keyStr.Length < 4)
                {
                    keyStr = "0" + keyStr;
                }

                var hk_gpu = hk_gpu_list.OpenSubKey(keyStr);
                if (hk_gpu != null)
                {
                    max_count = i;
                    var gpuName = (string)hk_gpu.GetValue("DriverDesc");
                    if (gpuName != null)
                    {
                        gpuNameList.Add((keyStr, gpuName));
                    }

                    hk_gpu.Close();
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("\n\n显卡列表：");
            foreach (var gpuName in gpuNameList)
            {
                Console.WriteLine(gpuName.Item1 + "\t" + gpuName.Item2);
                if (gpuName.Item2.ToLower().IndexOf("tesla".ToLower(), StringComparison.Ordinal) != -1)
                {
                    Console.WriteLine("\t这是一个NVIDIA Tesla计算卡！");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\n请输入4位的代码：");
            string gpuKey = Console.ReadLine();
            //const string gpuKey = "0001";

            if (gpuKey.Length == 4)
            {
                var hk_gpu = hk_gpu_list.OpenSubKey(gpuKey, true);
                if (hk_gpu != null)
                {
                    var gpuName = (string)hk_gpu.GetValue("DriverDesc");
                    if (gpuName != null)
                    {
                        Console.WriteLine("\n即将开始处理：" + gpuName);
                        handleReg(hk_gpu);
                    }

                    hk_gpu.Close();
                }
            }
            else
            {
                Console.WriteLine("输入非法！");
            }


            //foreach (var item in subnameStrings)
            //{
            //    Console.WriteLine(item);
            //}


            hk_gpu_list.Close();
            hk_lm.Close();


            Console.WriteLine("");
            Console.WriteLine("ok!");
            System.Console.ReadKey();
        }

        private static void handleReg(RegistryKey hk_gpu)
        {
            //Console.WriteLine((uint)(int)hk_gpu.GetValue("AdapterType"));
            if ((uint)(int)hk_gpu.GetValue("AdapterType") != 1
                && (uint)(int)hk_gpu.GetValue("FeatureScore") != 209)
            {
                Console.WriteLine("即将转换为WDDM模式！");
                hk_gpu.SetValue("AdapterType", 1, RegistryValueKind.DWord);
                hk_gpu.SetValue("FeatureScore", 209, RegistryValueKind.DWord);
                hk_gpu.SetValue("GridLicensedFeatures", 7, RegistryValueKind.DWord);
                hk_gpu.SetValue("EnableMsHybrid", 1, RegistryValueKind.DWord);
            }
            else
            {
                Console.WriteLine("貌似不可以转换！");
                //Console.WriteLine((int)hk_gpu.GetValue("AdapterType"));
                //Console.WriteLine((int)hk_gpu.GetValue("FeatureScore"));
                if ((int)hk_gpu.GetValue("FeatureScore") == 209)
                {
                    Console.WriteLine("貌似已经工作在WDDM模式下了！");
                }
            }
            //string str16 = string.Format("{0:x}", 209); //转十六进制数
            //hk_gpu.SetValue("test111", 209, RegistryValueKind.DWord);

            //hk_gpu.SetValue("test111121", str16, RegistryValueKind.String);
        }
    }
}