﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{

	[HarmonyPatch(typeof(Pawn_HealthTracker))]
	[HarmonyPatch("NotifyPlayerOfKilled")]
	internal static class DeadPawnMessageReplacement
	{
		private static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff, Caravan caravan)
		{
			if (ACUtils.ACTracker.stacksIndex.ContainsKey(___pawn.ThingID + ___pawn.Name)
					|| ACUtils.ACTracker.pawnsWithStacks.Contains(___pawn))
			{
				TaggedString taggedString = "";
				taggedString = (dinfo.HasValue ? "AlteredCarbon.SleveOf".Translate() + dinfo.Value.Def.deathMessage
					.Formatted(___pawn.LabelShortCap, ___pawn.Named("PAWN")) : ((hediff == null)
					? "AlteredCarbon.PawnDied".Translate(___pawn.LabelShortCap, ___pawn.Named("PAWN"))
					: "AlteredCarbon.PawnDiedBecauseOf".Translate(___pawn.LabelShortCap, hediff.def.LabelCap,
					___pawn.Named("PAWN"))));
				taggedString = taggedString.AdjustedFor(___pawn);
				TaggedString label = "AlteredCarbon.SleeveDeath".Translate() + ": " + ___pawn.LabelShortCap;
				Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NeutralEvent, ___pawn);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike")]
	public class AppendThoughts_ForHumanlike_Patch
	{
		public static bool DisableKilledEffect = false;

		[HarmonyPrefix]
		public static bool Prefix(ref Pawn victim)
		{
			if (DisableKilledEffect)
			{
				DisableKilledEffect = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_Relations")]
	public class AppendThoughts_Relations_Patch
	{
		public static bool DisableKilledEffect = false;

		[HarmonyPrefix]
		public static bool Prefix(ref Pawn victim)
		{
			if (DisableKilledEffect)
			{
				DisableKilledEffect = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(SocialCardUtility), "GetPawnSituationLabel")]
	public class Dead_Patch
	{
		[HarmonyPrefix]
		public static bool Prefix(Pawn pawn, Pawn fromPOV, ref string __result)
		{
			if (ACUtils.ACTracker.deadPawns.Contains(pawn) && ACUtils.ACTracker.stacksIndex.ContainsKey(pawn.ThingID + pawn.Name))
			{
				__result = "AlteredCarbon.NoSleeve".Translate();
				return false;
			}
			var stackHediff = pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
				x.def == AlteredCarbonDefOf.AC_CorticalStack);
			if (stackHediff != null && pawn.Dead)
			{
				__result = "AlteredCarbon.Sleeve".Translate();
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Faction), "Notify_LeaderDied")]
	public static class Notify_LeaderDied_Patch
	{
		public static bool DisableKilledEffect = false;

		[HarmonyPrefix]
		public static bool Prefix()
		{
			if (DisableKilledEffect)
			{
				DisableKilledEffect = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(StatsRecord), "Notify_ColonistKilled")]
	public static class Notify_ColonistKilled_Patch
	{
		public static bool DisableKilledEffect = false;

		[HarmonyPrefix]
		public static bool Prefix()
		{
			if (DisableKilledEffect)
			{
				DisableKilledEffect = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Pawn_RoyaltyTracker), "Notify_PawnKilled")]
	public static class Notify_PawnKilled_Patch
	{
		public static bool DisableKilledEffect = false;

		[HarmonyPrefix]
		public static bool Prefix()
		{
			if (DisableKilledEffect)
			{
				DisableKilledEffect = false;
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Pawn), "Kill")]
	public class Pawn_Kill_Patch
	{
		public static void Prefix(Pawn __instance)
		{
			try
			{
				if (__instance != null && (ACUtils.ACTracker.stacksIndex.ContainsKey(__instance.ThingID + __instance.Name)
					|| ACUtils.ACTracker.pawnsWithStacks.Contains(__instance)))
				{
					Notify_ColonistKilled_Patch.DisableKilledEffect = true;
					Notify_PawnKilled_Patch.DisableKilledEffect = true;
					Notify_LeaderDied_Patch.DisableKilledEffect = true;
					AppendThoughts_ForHumanlike_Patch.DisableKilledEffect = true;
					AppendThoughts_Relations_Patch.DisableKilledEffect = true;
				}
				var stackHediff = __instance.health.hediffSet.hediffs.FirstOrDefault((Hediff x) =>
					x.def == AlteredCarbonDefOf.AC_CorticalStack);
				if (stackHediff != null)
				{
					ACUtils.ACTracker.deadPawns.Add(__instance);
				}
			}
			catch { };
		}
	}

	[HarmonyPatch(typeof(ColonistBarColonistDrawer), "HandleClicks")]
	public static class HandleClicks_Patch
	{
		[HarmonyPrefix]
		public static bool Prefix(Rect rect, Pawn colonist, int reorderableGroup, out bool reordering)
		{
			reordering = false;
			if (colonist.Dead)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && Mouse.IsOver(rect))
				{
					Event.current.Use();
					if (ACUtils.ACTracker.stacksIndex.ContainsKey(colonist.ThingID + colonist.Name))
					{
						Log.Message(colonist + "TEST 3: " + ACUtils.ACTracker.stacksIndex[colonist.ThingID + colonist.Name]);
						if (ACUtils.ACTracker.stacksIndex[colonist.ThingID + colonist.Name] == null)
						{
							CameraJumper.TryJumpAndSelect(colonist);
						}
						else
						{
							CameraJumper.TryJumpAndSelect(ACUtils.ACTracker.stacksIndex[colonist.ThingID + colonist.Name]);
						}
					}
					else
					{
						Log.Message(colonist + "TEST 4");
						CameraJumper.TryJumpAndSelect(colonist);
					}
				}
				reordering = ReorderableWidget.Reorderable(reorderableGroup, rect, useRightButton: true);
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
				{
					Event.current.Use();
				}
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawColonist")]
	[StaticConstructorOnStartup]
	public static class DrawColonist_Patch
	{
		private static readonly Texture2D Icon_StackDead = ContentFinder<Texture2D>.Get("UI/Icons/StackDead");

		[HarmonyPrefix]
		public static bool Prefix(Rect rect, Pawn colonist, Map pawnMap, bool highlight, bool reordering,
			Dictionary<string, string> ___pawnLabelsCache, Vector2 ___PawnTextureSize,
			Texture2D ___MoodBGTex, Vector2[] ___bracketLocs)
		{
			if (colonist.Dead && (ACUtils.ACTracker.stacksIndex.ContainsKey(colonist.ThingID + colonist.Name)
					|| ACUtils.ACTracker.pawnsWithStacks.Contains(colonist)))
			{
				float alpha = Find.ColonistBar.GetEntryRectAlpha(rect);
				ApplyEntryInAnotherMapAlphaFactor(pawnMap, ref alpha);
				if (reordering)
				{
					alpha *= 0.5f;
				}
				Color color2 = GUI.color = new Color(1f, 1f, 1f, alpha);
				GUI.DrawTexture(rect, ColonistBar.BGTex);
				if (colonist.needs != null && colonist.needs.mood != null)
				{
					Rect position = rect.ContractedBy(2f);
					float num = position.height * colonist.needs.mood.CurLevelPercentage;
					position.yMin = position.yMax - num;
					position.height = num;
					GUI.DrawTexture(position, ___MoodBGTex);
				}
				if (highlight)
				{
					int thickness = (rect.width <= 22f) ? 2 : 3;
					GUI.color = Color.white;
					Widgets.DrawBox(rect, thickness);
					GUI.color = color2;
				}
				Rect rect2 = rect.ContractedBy(-2f * Find.ColonistBar.Scale);
				if ((colonist.Dead ? Find.Selector.SelectedObjects.Contains(colonist.Corpse) : Find.Selector.SelectedObjects.Contains(colonist)) && !WorldRendererUtility.WorldRenderedNow)
				{
					DrawSelectionOverlayOnGUI(colonist, rect2, ___bracketLocs);
				}

				GUI.DrawTexture(GetPawnTextureRect(rect.position, ___PawnTextureSize), PortraitsCache.Get(colonist, ___PawnTextureSize, ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f));
				GUI.color = new Color(1f, 1f, 1f, alpha * 0.8f);

				float num3 = 20f * Find.ColonistBar.Scale;
				Vector2 pos2 = new Vector2(rect.x + 1f, rect.yMax - num3 - 1f);
				DrawIcon(Icon_StackDead, ref pos2, "ActivityIconMedicalRest".Translate());
				GUI.color = color2;

				float num2 = 4f * Find.ColonistBar.Scale;
				Vector2 pos = new Vector2(rect.center.x, rect.yMax - num2);
				GenMapUI.DrawPawnLabel(colonist, pos, alpha, rect.width + Find.ColonistBar.SpaceBetweenColonistsHorizontal - 2f, ___pawnLabelsCache);
				Text.Font = GameFont.Small;
				GUI.color = Color.white;
				return false;
			}
			return true;
		}

		private static void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
		{
			float num = 20f * Find.ColonistBar.Scale;
			Rect rect = new Rect(pos.x, pos.y, num, num);
			GUI.DrawTexture(rect, icon);
			TooltipHandler.TipRegion(rect, tooltip);
			pos.x += num;
		}

		public static Rect GetPawnTextureRect(Vector2 pos, Vector2 ___PawnTextureSize)
		{
			float x = pos.x;
			float y = pos.y;
			Vector2 vector = ___PawnTextureSize * Find.ColonistBar.Scale;
			return new Rect(x + 1f, y - (vector.y - Find.ColonistBar.Size.y) - 1f, vector.x, vector.y).ContractedBy(1f);
		}

		private static void ApplyEntryInAnotherMapAlphaFactor(Map map, ref float alpha)
		{
			if (map == null)
			{
				if (!WorldRendererUtility.WorldRenderedNow)
				{
					alpha = Mathf.Min(alpha, 0.4f);
				}
			}
			else if (map != Find.CurrentMap || WorldRendererUtility.WorldRenderedNow)
			{
				alpha = Mathf.Min(alpha, 0.4f);
			}
		}

		private static void DrawSelectionOverlayOnGUI(Pawn colonist, Rect rect, Vector2[] ___bracketLocs)
		{
			Thing obj = colonist;
			if (colonist.Dead)
			{
				obj = colonist.Corpse;
			}
			float num = 0.4f * Find.ColonistBar.Scale;
			SelectionDrawerUtility.CalculateSelectionBracketPositionsUI<object>(textureSize: new Vector2((float)SelectionDrawerUtility.SelectedTexGUI.width * num, (float)SelectionDrawerUtility.SelectedTexGUI.height * num), bracketLocs: ___bracketLocs, obj: (object)obj, rect: rect, selectTimes: SelectionDrawer.SelectTimes, jumpDistanceFactor: 20f * Find.ColonistBar.Scale);
			DrawSelectionOverlayOnGUI(___bracketLocs, num);
		}

		private static void DrawSelectionOverlayOnGUI(Vector2[] bracketLocs, float selectedTexScale)
		{
			int num = 90;
			for (int i = 0; i < 4; i++)
			{
				Widgets.DrawTextureRotated(bracketLocs[i], SelectionDrawerUtility.SelectedTexGUI, num, selectedTexScale);
				num += 90;
			}
		}
	}
}

