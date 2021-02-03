using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_RecordsTracker), "AddTo")]
    class Record_AddTo_Patch
    {
        static bool Prefix(Pawn_RecordsTracker __instance, DefMap<RecordDef, float> ___records, RecordDef def, float value)
        {
            if (def.type == RecordType.Time)
            {
                ___records[def] += value;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
