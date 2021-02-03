using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public static class ModCompatibility
	{
		public static void SetSkinColor(Pawn pawn, Color color)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
            {
				alienComp.GetChannel("skin").first = color;
            }
		}

		public static void SetAlienHead(Pawn pawn, string head)
		{
			var alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.crownType = head;
			}
		}
		public static List<string> GetAlienHeadPaths(Pawn pawn)
        {
			var alienDef = pawn.def as AlienRace.ThingDef_AlienRace;
			return alienDef.alienRace.generalSettings.alienPartGenerator.aliencrowntypes;
		}
		public static int GetSyrTraitsSexuality(Pawn pawn)
        {
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				return (int)comp.sexuality;
			}
			return -1;
		}
		public static float GetSyrTraitsRomanceFactor(Pawn pawn)
		{
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				return comp.RomanceFactor;
			}
			return -1f;
		}

		public static void SetSyrTraitsSexuality(Pawn pawn, int sexuality)
		{
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				comp.sexuality = (SyrTraits.CompIndividuality.Sexuality)sexuality;
			}
		}
		public static void SetSyrTraitsRomanceFactor(Pawn pawn, float romanceFactor)
		{
			var comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				comp.RomanceFactor = romanceFactor;
			}
		}

		public static PsychologyData GetPsychologyData(Pawn pawn)
		{
			var comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
			if (comp != null)
			{
				var psychologyData = new PsychologyData();
				var sexualityTracker = comp.Sexuality;
				psychologyData.sexDrive = sexualityTracker.sexDrive;
				psychologyData.romanticDrive = sexualityTracker.romanticDrive;
				psychologyData.kinseyRating = sexualityTracker.kinseyRating;
				psychologyData.knownSexualities = Traverse.Create(sexualityTracker).Field<Dictionary<Pawn, int>>("knownSexualities").Value;
				psychologyData.tickSinceLastSeenLover = comp.LDRTick;
				psychologyData.lastDateTick = comp.Psyche.lastDateTick;
				psychologyData.beenBuried = comp.AlreadyBuried;
				psychologyData.records = new Dictionary<string, float>();
				psychologyData.upbringing = comp.Psyche.upbringing;
				List<RecordDef> defsListForReading = DefDatabase<RecordDef>.AllDefsListForReading;
				for (int index = 0; index < defsListForReading.Count; ++index)
                {
					psychologyData.records[defsListForReading[index].defName] = pawn.records.GetValue(defsListForReading[index]);
				}

				psychologyData.personalityNodes = new Dictionary<string, float>();
				foreach (Psychology.PersonalityNode node in comp.Psyche.PersonalityNodes)
                {
					psychologyData.personalityNodes.Add(node.def.defName, node.rawRating);
				}
				
				return psychologyData;
			}
			return null;
		}

		public static void SetPsychologyData(Pawn pawn, PsychologyData psychologyData)
		{
			var comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
			if (comp != null)
			{
				var sexualityTracker = new Psychology.Pawn_SexualityTracker(pawn);
				sexualityTracker.sexDrive = psychologyData.sexDrive;
				sexualityTracker.romanticDrive = psychologyData.romanticDrive;
				sexualityTracker.kinseyRating = psychologyData.kinseyRating;
				comp.LDRTick = psychologyData.tickSinceLastSeenLover;
				comp.Psyche.lastDateTick = psychologyData.lastDateTick;
				comp.AlreadyBuried = psychologyData.beenBuried;
				comp.Psyche.upbringing = psychologyData.upbringing;
				foreach (KeyValuePair<string, float> entry in psychologyData.records)
				{
					
					pawn.records.AddTo(DefDatabase<RecordDef>.GetNamed(entry.Key), entry.Value);
				}
				Traverse.Create(sexualityTracker).Field<Dictionary<Pawn, int>>("knownSexualities").Value = psychologyData.knownSexualities;
				comp.Sexuality = sexualityTracker;
				foreach (Psychology.PersonalityNode node in comp.Psyche.PersonalityNodes)
				{
					node.pawn = pawn;
					node.rawRating = psychologyData.personalityNodes[node.def.defName];
				}
			}
		}

		public static List<ThingDef> GetAllAlienRaces(ExcludeRacesModExtension raceOptions)
        {
			return DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where(x => !raceOptions.racesToExclude.Contains(x.defName)).Cast<ThingDef>().ToList();
		}

		public static bool AlienRacesIsActive => ModLister.HasActiveModWithName("Humanoid Alien Races 2.0");
		public static bool IndividualityIsActive => ModLister.HasActiveModWithName("[SYR] Individuality");
		public static bool PsychologyIsActive => ModLister.AllInstalledMods.Where(x => x.Active && (x.PackageId.ToLower() == "community.psychology.unofficialupdate")).Any();
	}

}
