using DAL;
using System;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        async static Task Main(string[] args)
        {
            await HTTPHelper.CrazyDowmload("http://ftp.pconline.com.cn/b79d0cbdbfe166ad9337b855b3dc208f/pub/download/201010/Adobe_Reader_XI_zh_CN.exe");
            Console.WriteLine("Hello World!");
        }
    }
}
