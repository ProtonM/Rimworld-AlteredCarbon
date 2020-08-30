﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

    [StaticConstructorOnStartup]
    internal static class Rotation_Patch
    {

        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[]
        {
            typeof(Vector3),
            typeof(float),
            typeof(bool),
            typeof(Rot4),
            typeof(Rot4),
            typeof(RotDrawMode),
            typeof(bool),
            typeof(bool),
            typeof(bool)
        })]
        public static class PawnRenderer_RenderPawnInternal_Patch
        {
            public static void Prefix(ref PawnRenderer __instance, Pawn ___pawn, ref Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible)
            {
                if (___pawn.CurrentBed() is Building_SleeveCasket bed)
                {
                    if (___pawn.Rotation == Rot4.South)
                    {
                        rootLoc.z -= 0.1f;
                    }
                    else if (___pawn.Rotation == Rot4.North)
                    {
                        rootLoc.z += 0.3f;
                    }
                    else if (___pawn.Rotation == Rot4.East)
                    {
                        rootLoc.x += 0.3f;
                    }
                    else if (___pawn.Rotation == Rot4.West)
                    {
                        rootLoc.x -= 0.3f;
                    }
                }
            }
        }
    }
}

