﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAimbot
{
    public static class Offsets
    {
        //Offsets.cs
        public static int dwViewAngles = 0x1890F30;
        public static int dwLocalPlayerPawn = 0x16D4F48;
        public static int dwEntityList = 0x17CE6A0;

        //client.dll.cs
        public static int m_hPlayerPawn = 0x7EC;
        public static int m_iHealth = 0x32C;
        public static int m_vOldOrigin = 0x1224;
        public static int m_iTeamNum = 0x3BF;
        public static int m_vecViewOffset = 0xC48;
        public static int m_lifeState = 0x330;
        public static int m_modelState = 0x160;
        public static int m_pGameSceneNode = 0x310;
    }
}