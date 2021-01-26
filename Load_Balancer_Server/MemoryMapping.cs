using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Threading;

namespace Load_Balancer_Server
{
    public class MemoryMapping
    {
        long capacity = 1024 * 1024 * 10;
        MemoryMappedFile file;
        private Semaphore m_Write;  //可写的信号
        private Semaphore m_Read;  //可读的信号
        public MemoryMapping(string fileName)
        {
            m_Write = new Semaphore(1, 1, "WriteMap");
            m_Read = new Semaphore(0, 1, "ReadMap");
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
            m_Write.Release();
            m_Read.Release();
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
                    List<byte> bytes = new List<byte>();
                    byte[] temp = new byte[1024];
                    while (true)
                    {
                        int readCount = reader.Read(temp, 0, temp.Length);
                        if (readCount == 0)
                        {
                            break;
                        }
                        for (int i = 0; i < readCount; i++)
                        {
                            bytes.Add(temp[i]);
                        }
                    }
                    if (bytes.Count > 0)
                    {
                        m_Write.Release();
                        //将“\0”去掉
                        return Encoding.Default.GetString(bytes.ToArray()).Replace("\0", "");
                    }
                    else
                    {
                        m_Write.Release();
                        return null;
                    }
                }
            }
        }
    }
}
