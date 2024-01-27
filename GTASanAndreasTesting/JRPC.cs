using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XDevkit;

namespace JRPC_Client
{
    public static class JRPC
    {
        public static uint connectionId;
        public static readonly uint jrpcVersion = 2;

        public static bool Connect(this IXboxConsole console, out IXboxConsole Console, string XboxNameOrIP = "default")
        {
            if (XboxNameOrIP == "default")
                XboxNameOrIP = ((IXboxManager)new XboxManager()).DefaultConsole;
            IXboxConsole xboxConsole = (IXboxConsole)((IXboxManager)new XboxManager()).OpenConsole(XboxNameOrIP);
            bool flag = false;
            while (!flag)
            {
                try
                {
                    connectionId = xboxConsole.OpenConnection((string)null);
                    flag = true;
                }
                catch
                {
                    Console = xboxConsole;
                    return false;
                }
            }
            Console = xboxConsole;
            return true;
        }

        public static string ConsoleType(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + jrpcVersion + " type=17 params=\"A\\0\\A\\0\\\"";
            string String = SendCommand(console, Command);
            return String.Substring(String.find(" ") + 1);
        }

        public static int find(this string String, string _Ptr)
        {
            if (_Ptr.Length == 0 || String.Length == 0)
                return -1;
            for (int index1 = 0; index1 < String.Length; ++index1)
            {
                if ((int)String[index1] == (int)_Ptr[0])
                {
                    bool flag = true;
                    for (int index2 = 0; index2 < _Ptr.Length; ++index2)
                    {
                        if ((int)String[index1 + index2] != (int)_Ptr[index2])
                            flag = false;
                    }
                    if (flag)
                        return index1;
                }
            }
            return -1;
        }

        public static string GetConsoleID(this IXboxConsole console)
        {
            string String = SendCommand(console, "getconsoleid");
            return String.Substring(String.find(" ") + 1).Replace("consoleid=", "");
        }

        public static string GetCPUKey(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)jrpcVersion + " type=10 params=\"A\\0\\A\\0\\\"";
            string String = SendCommand(console, Command);
            return String.Substring(String.find(" ") + 1);
        }

        public static string GetDebugVersion(this IXboxConsole console)
        {
            return SendCommand(console, "dmversion").Replace("200- ", string.Empty);
        }

        public static string GetGamertag(this IXboxConsole console, bool IsDevkit)
        {
            uint address;
            uint XAMGamertagWCHARDevkit = 0x81D44475;
            uint XAMGamertagWCHARRetail = 0x81AA28FD;

            if (IsDevkit)
            {
                address = XAMGamertagWCHARDevkit;
            }
            else
            {
                address = XAMGamertagWCHARRetail;
            }
            byte[] memory = GetMemory(console, address, 30);
            return Encoding.Unicode.GetString(memory);
        }

        public static uint GetKernalVersion(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + (object)jrpcVersion + " type=13 params=\"A\\0\\A\\0\\\"";
            string String = SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1));
        }

        public static byte[] GetMemory(this IXboxConsole console, uint Address, uint Length)
        {
            byte[] Data = new byte[(int)(IntPtr)Length];
            console.DebugTarget.GetMemory(Address, Length, Data, out uint BytesRead);
            console.DebugTarget.InvalidateMemoryCache(true, Address, Length);
            return Data;
        }

        private static  byte[] GetMemory1(this IXboxConsole console, uint Address, uint Length)
        {
            byte[] Data = new byte[(int)(IntPtr)Length];
            console.DebugTarget.GetMemory(Address, Length, Data, out uint BytesRead);
            return Data;
        }

        public static byte[] GetMemory2(this IXboxConsole console, uint address, uint length, uint ChunkSize=0x10000)
        {
            byte[] result = new byte[length];

            for (uint offset = 0; offset < length; offset += ChunkSize)
            {
                uint chunkLength = Math.Min(ChunkSize, length - offset);
                byte[] chunk = console.GetMemory1(address + offset, chunkLength);
                Buffer.BlockCopy(chunk, 0, result, (int)offset, (int)chunkLength);
            }
            console.DebugTarget.InvalidateMemoryCache(true, address, length);
            return result;
        }

        public static byte[] GetMemoryTest(this IXboxConsole console, uint address, uint length, uint ChunkSize = 0x10000) // you can adjust this based on search length....i think
        {
            byte[] result = new byte[length];
            object lockObject = new object();

            Parallel.For(0, (int)Math.Ceiling((double)length / ChunkSize), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                i =>
                {
                    uint offset = (uint)(i * ChunkSize);
                    uint chunkLength = Math.Min(ChunkSize, length - offset);
                    byte[] chunk = console.GetMemory1(address + offset, chunkLength);

                    lock (lockObject)
                    {
                        Buffer.BlockCopy(chunk, 0, result, (int)offset, (int)chunkLength);
                    }
                });

            console.DebugTarget.InvalidateMemoryCache(true, address, length);
            return result;
        }

        public static ulong GetOfflineXuidDevKit(this IXboxConsole console, bool IsDevkit)
        {
            uint address;
            uint XAMOfflineXuidDevKit = 0x81D44460;
            uint XAMProfileIDRetail = 0x81AA28E8;

            if (IsDevkit)
            {
                address = XAMOfflineXuidDevKit;
            }
            else address = XAMProfileIDRetail;
            return ReadUInt64(console, address);
        }

        public static void Push(this byte[] InArray, out byte[] OutArray, byte Value)
        {
            OutArray = new byte[InArray.Length + 1];
            InArray.CopyTo((Array)OutArray, 0);
            OutArray[InArray.Length] = Value;
        }

        public static float ReadFloat(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory2(Address, 4U);
            ReverseBytes(memory, 4);
            return BitConverter.ToSingle(memory, 0);
        }

        public static byte ReadByte(this IXboxConsole console, uint Address)
        {
            return console.GetMemory2(Address, 1U)[0];
        }

        public static string ReadString(this IXboxConsole console, uint Address, uint size)
        {
            return Encoding.UTF8.GetString(console.GetMemory2(Address, size));
        }

        public static ushort ReadUInt16(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory2(Address, 2U);
            ReverseBytes(memory, 2);
            return BitConverter.ToUInt16(memory, 0);
        }

        public static uint ReadUInt32(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory2(Address, 4U);
            ReverseBytes(memory, 4);
            return BitConverter.ToUInt32(memory, 0);
        }

        public static ulong ReadUInt64(this IXboxConsole console, uint Address)
        {
            byte[] memory = console.GetMemory2(Address, 8U);
            ReverseBytes(memory, 8);
            return BitConverter.ToUInt64(memory, 0);
        }

        private static void ReverseBytes(byte[] buffer, int groupSize)
        {
            if (buffer.Length % groupSize != 0)
                throw new ArgumentException("Group size must be a multiple of the buffer length");
            int num1 = 0;
            while (num1 < buffer.Length)
            {
                int index1 = num1;
                for (int index2 = num1 + groupSize - 1; index1 < index2; --index2)
                {
                    byte num2 = buffer[index1];
                    buffer[index1] = buffer[index2];
                    buffer[index2] = num2;
                    ++index1;
                }
                num1 += groupSize;
            }
        }

        public static string SendCommand(this IXboxConsole console, string Command)
        {
            string Response;
            try
            {
                console.SendTextCommand(connectionId, Command, out Response);
                if (Response.Contains("error="))
                    throw new Exception(Response.Substring(11));
                if (Response.Contains("DEBUG"))
                    throw new Exception("JRPC2 is not installed on the current console");
            }
            catch (COMException ex)
            {
                if (ex.ErrorCode == UIntToInt(2195324935U))
                    throw new Exception("JRPC2 is not installed on the current console");
                throw ex;
            }
            return Response;
        }

        public static void SetMemory(this IXboxConsole console, uint Address, byte[] Data)
        {
            uint BytesWritten;
            console.DebugTarget.SetMemory(Address, (uint)Data.Length, Data, out BytesWritten);
        }

        public static void ShutDownConsole(this IXboxConsole console)
        {
             string Command = "consolefeatures ver=" + (object)jrpcVersion + " type=11 params=\"A\\0\\A\\0\\\"";
             SendCommand(console, Command);
        }

        public static int UIntToInt(uint Value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(Value), 0);
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte Value)
        {
            console.SetMemory(Address, new byte[1] { Value });
        }

        public static void WriteByte(this IXboxConsole console, uint Address, byte[] Value)
        {
            console.SetMemory(Address, Value);
        }

        public static void WriteFloat(this IXboxConsole console, uint Address, float Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            Array.Reverse((Array)bytes);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt16(this IXboxConsole console, uint Address, ushort Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 2);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt32(this IXboxConsole console, uint Address, uint Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 4);
            console.SetMemory(Address, bytes);
        }

        public static void WriteUInt64(this IXboxConsole console, uint Address, ulong Value)
        {
            byte[] bytes = BitConverter.GetBytes(Value);
            ReverseBytes(bytes, 8);
            console.SetMemory(Address, bytes);
        }

        public static void WriteString(this IXboxConsole console, uint Address, string String)
        {
            byte[] OutArray = new byte[0];
            foreach (byte num in String)
                OutArray.Push(out OutArray, num);
            OutArray.Push(out OutArray, (byte)0);
            console.SetMemory(Address, OutArray);
        }

        public static uint XamGetCurrentTitleId(this IXboxConsole console)
        {
            string Command = "consolefeatures ver=" + jrpcVersion + " type=16 params=\"A\\0\\A\\0\\\"";
            string String = SendCommand(console, Command);
            return uint.Parse(String.Substring(String.find(" ") + 1), NumberStyles.HexNumber);
        }

        public static string XboxIP(this IXboxConsole console)
        {
            byte[] bytes = BitConverter.GetBytes(console.IPAddress);
            Array.Reverse((Array)bytes);
            return new IPAddress(bytes).ToString();
        }
    }
}
