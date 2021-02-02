using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Threading;

namespace SetVMStatus
{
    public class MemoryMapping
    {
        long capacity = 1024 * 1024 * 10;
        MemoryMappedFile file;
        private Semaphore m_Write;  //可写的信号
        private Semaphore m_Read;  //可读的信号
        public Semaphore m_Received; //是否收到并处理完的信号
        public MemoryMapping(string fileName)
        {
            m_Write = new Semaphore(1, 1, "WriteMap");
            m_Read = new Semaphore(0, 1, "ReadMap");
            m_Received = new Semaphore(0, 1, "ReceivedMap");
            file = MemoryMappedFile.CreateOrOpen(fileName, capacity);
        }


        public void WriteString(string msg)
        {
            m_Write = Semaphore.OpenExisting("WriteMap");
            m_Read = Semaphore.OpenExisting("ReadMap");
            m_Write.WaitOne();
            using (var stream = file.CreateViewStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(msg);
                }
            }
            // 先允许读
            m_Read.Release();
            // 等待操作完后，释放写锁退出
            m_Write.Release();
            m_Received.WaitOne();
        }
        public string ReadString()
        {
            m_Write = Semaphore.OpenExisting("WriteMap");
            m_Read = Semaphore.OpenExisting("ReadMap");
            m_Read.WaitOne();
            
            m_Write.WaitOne();
            using (var stream = file.CreateViewStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    string str = reader.ReadString();
                    m_Write.Release();
                    return str;
                }
            }
        }
    }
}
