using Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAZGlow
{
    public static class Offsets
    {
        public static List<string> ScanPatterns()
        {
            List<string> outdatedSignatures = new List<string> { };

            dwClientState = MemoryManager.ScanPattern(Program.EnginePointer, "A1????????33D26A006A0033C989B0", 1, 0, true);
            {
                if (dwClientState == 0) outdatedSignatures.Add("dwClientState");
            }

            dwLocalPlayer = MemoryManager.ScanPattern(Program.ClientPointer, "A3????????C705????????????????E8????????59C36A??", 1, 16, true);
            {
                if (dwLocalPlayer == 0) outdatedSignatures.Add("dwLocalPlayer");
            }

            dwGlowObjectManager = MemoryManager.ScanPattern(Program.ClientPointer, "A1????????A801754B", 1, 4, true);
            {
                if (dwGlowObjectManager == 0) outdatedSignatures.Add("dwGlowObjectManager");
            }

            dwEntityList = MemoryManager.ScanPattern(Program.ClientPointer, "BB????????83FF010F8C????????3BF8", 1, 0, true);
            {
                if (dwEntityList == 0) outdatedSignatures.Add("dwEntityList");
            }

            return outdatedSignatures;
        }

        public static Int32 dwClientState = 0x0;
        public static Int32 dwLocalPlayer = 0x0;
        public static Int32 dwGlowObjectManager = 0x0;
        public static Int32 dwEntityList = 0x0;

        public static Int32 m_iHealth = 0xFC;
        public static Int32 m_iGlowIndex = 0xA310;
        public const Int32 m_iTeamNum = 0xF0;
    }
}