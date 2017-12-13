using Memory;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EAZGlow
{
    internal class Program
    {
        public static int ClientPointer;
        public static int EnginePointer;

        #region Struct

        [StructLayout(LayoutKind.Explicit)]
        public struct GlowStruct
        {
            [FieldOffset(0x00)]
            public int EntityPointer;
            [FieldOffset(0x4)]
            public float r;
            [FieldOffset(0x8)]
            public float g;
            [FieldOffset(0xC)]
            public float b;
            [FieldOffset(0x10)]
            public float a;
            [FieldOffset(0x14)]
            public int jnk1;
            [FieldOffset(0x18)]
            public int jnk2;
            [FieldOffset(0x1C)]
            public float BloomAmount;
            [FieldOffset(0x20)]
            public int jnk3;

            [FieldOffset(0x24)]
            public bool m_bRenderWhenOccluded;
            [FieldOffset(0x25)]
            public bool m_bRenderWhenUnoccluded;
            [FieldOffset(0x2C)]
            public bool m_bFullBloom;
        };

        internal struct Entity
        {
            public int m_iBase;
            public int m_iDormant;
            public int m_iHealth;
            public int m_iTeam;
            public int m_iGlowIndex;
        }

        internal struct LocalPlayer
        {
            public static int m_iBase;
            public static int m_iTeam;
            public static int m_iClientState;
            public static int m_iGlowBase;
        }

        internal class Arrays
        {
            public static Entity[] Entity = new Entity[64];
        }

        #endregion

        #region AddressReader

        static void AddressReader()
        {
            while (true)
            {
                LocalPlayer.m_iBase = MemoryManager.ReadMemory<int>(ClientPointer + Offsets.dwLocalPlayer);
                LocalPlayer.m_iTeam = MemoryManager.ReadMemory<int>(LocalPlayer.m_iBase + Offsets.m_iTeamNum);
                LocalPlayer.m_iClientState = MemoryManager.ReadMemory<int>(EnginePointer + Offsets.dwClientState);
                LocalPlayer.m_iGlowBase = MemoryManager.ReadMemory<int>(ClientPointer + Offsets.dwGlowObjectManager);

                for (var i = 0; i < 64; i++)
                {
                    var Entity = Arrays.Entity[i];

                    Entity.m_iBase = MemoryManager.ReadMemory<int>(ClientPointer + Offsets.dwEntityList + i * 0x10);

                    if (Entity.m_iBase > 0)
                    {
                        Entity.m_iTeam = MemoryManager.ReadMemory<int>(Entity.m_iBase + Offsets.m_iTeamNum);
                        Entity.m_iHealth = MemoryManager.ReadMemory<int>(Entity.m_iBase + Offsets.m_iHealth);
                        Entity.m_iDormant = MemoryManager.ReadMemory<int>(Entity.m_iBase + 0xE9);
                        Entity.m_iGlowIndex = MemoryManager.ReadMemory<int>(Entity.m_iBase + Offsets.m_iGlowIndex);

                        Arrays.Entity[i] = Entity;
                    }

                    Thread.Sleep(5);
                }
            }
        }

        #endregion

        #region DLLImports

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        #endregion

        static bool KeyPressed(Keys key) => GetAsyncKeyState(key) != 0;
        static bool GlowEnabled = false;

        static void Main()
        {
            Console.Title = "EAZ Glow - Unknowncheats.me";

            MemoryManager.Initialize("csgo");
            ClientPointer = MemoryManager.GetModuleAdress("client");
            EnginePointer = MemoryManager.GetModuleAdress("engine");

            Offsets.ScanPatterns();

            var AddressReaderThread = new Thread(AddressReader);
            var KeyProcThread = new Thread(KeyProc);

            AddressReaderThread.Start();
            KeyProcThread.Start();
            
            while (true)
            {
                for (var i = 0; i < 64; i++)
                {
                    if (Arrays.Entity[i].m_iBase == 0)
                        continue;
                    if (Arrays.Entity[i].m_iBase == LocalPlayer.m_iBase)
                        continue;
                    if (Arrays.Entity[i].m_iHealth < 1)
                        continue;
                    if (Arrays.Entity[i].m_iDormant == 1)
                        continue;
                    
                    GlowStruct GlowObject = new GlowStruct();

                    if (GlowEnabled != false)
                    {
                        if (Arrays.Entity[i].m_iTeam != LocalPlayer.m_iTeam)
                        {
                            GlowObject = MemoryManager.ReadMemory<GlowStruct>(LocalPlayer.m_iGlowBase + Arrays.Entity[i].m_iGlowIndex * 0x38);

                            GlowObject.r = 255 / 255;
                            GlowObject.g = 0 / 255;
                            GlowObject.b = 0 / 255;
                            GlowObject.a = 255 / 255;
                            GlowObject.m_bRenderWhenOccluded = true;
                            GlowObject.m_bRenderWhenUnoccluded = false;
                            GlowObject.m_bFullBloom = false;

                            MemoryManager.WriteMemory<GlowStruct>(LocalPlayer.m_iGlowBase + Arrays.Entity[i].m_iGlowIndex * 0x38, GlowObject);
                        }
                        else
                        {
                            GlowObject = MemoryManager.ReadMemory<GlowStruct>(LocalPlayer.m_iGlowBase + Arrays.Entity[i].m_iGlowIndex * 0x38);

                            GlowObject.r = 0 / 255;
                            GlowObject.g = 0 / 255;
                            GlowObject.b = 255 / 255;
                            GlowObject.a = 255 / 255;
                            GlowObject.m_bRenderWhenOccluded = true;
                            GlowObject.m_bRenderWhenUnoccluded = false;
                            GlowObject.m_bFullBloom = false;

                            MemoryManager.WriteMemory<GlowStruct>(LocalPlayer.m_iGlowBase + Arrays.Entity[i].m_iGlowIndex * 0x38, GlowObject);
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        static async void KeyProc()
        {
            while (true)
            {
                if (KeyPressed(Keys.F2))
                {
                    GlowEnabled = !GlowEnabled;
                    Console.WriteLine("Glow - " + GlowEnabled);
                    await Task.Delay(400);
                }

                Thread.Sleep(5);
            }
        }
    }
}