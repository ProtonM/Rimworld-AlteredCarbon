﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class CorticalStack : ThingWithComps
    {
        public Name name;
        public Pawn origPawn;
        public int hostilityMode;
        public Area areaRestriction;
        public MedicalCareCategory medicalCareCategory;
        public bool selfTend;
        public long ageChronologicalTicks;
        public List<TimeAssignmentDef> times;
        public FoodRestriction foodRestriction;
        public Outfit outfit;
        public DrugPolicy drugPolicy;
        public Faction faction;
        public bool isFactionLeader;
        public List<Thought_Memory> thoughts;
        public List<Trait> traits;
        public List<DirectPawnRelation> relations;
        public HashSet<Pawn> relatedPawns;
        public List<SkillRecord> skills;
        public string childhood;
        public string adulthood;
        public Dictionary<WorkTypeDef, int> priorities;
        public bool hasPawn = false;

        public Gender gender;
        public string pawnID;

        public List<RoyalTitle> royalTitles;
        public Dictionary<Faction, int> favor = new Dictionary<Faction, int>();
        public Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();
        public List<Thing> bondedThings = new List<Thing>();

        public bool isCopied = false;
        public int stackGroupID;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad && !hasPawn && this.def.defName == "AC_FilledCorticalStack")
            {
                var pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                var faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
                this.SavePawnToCorticalStack(pawn);
                this.gender = pawn.gender;
                if (ACUtils.ACTracker.stacksRelationships != null)
                {
                    this.stackGroupID = ACUtils.ACTracker.stacksRelationships.Count + 1;
                }
                else
                {
                    this.stackGroupID = 0;
                }
                ACUtils.ACTracker.RegisterStack(this);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, 0))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level >= 5)
            {
                string label = "AlteredCarbon.WipeStack".Translate();
                Action action = delegate ()
                {
                    Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_WipeStack, this);
                    job.count = 1;
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
                        (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
                        this, "ReservedBy");
            }
            else if (this.def == AlteredCarbonDefOf.AC_FilledCorticalStack && myPawn.skills.GetSkill(SkillDefOf.Intellectual).Level < 5)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption("AlteredCarbon.CantWipeStackTooDumb".Translate(), null,
                    MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            if (this.hasPawn)
            {
                if (this.Map.listerThings.ThingsOfDef(AlteredCarbonDefOf.AC_EmptyCorticalStack)
                        .Where(x => myPawn.CanReserveAndReach(x, PathEndMode.ClosestTouch, Danger.Deadly)).Any())
                {
                    if (myPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt >= 10)
                    {
                        string label = "AlteredCarbon.DuplicateStack".Translate();
                        Action action = delegate ()
                        {
                            Job job = JobMaker.MakeJob(AlteredCarbonDefOf.AC_CopyStack, this);
                            job.count = 1;
                            myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption
                                (label, action, MenuOptionPriority.Default, null, null, 0f, null, null), myPawn,
                                this, "ReservedBy");
                    }
                    else if (myPawn.skills.GetSkill(SkillDefOf.Intellectual).levelInt < 10)
                    {
                        FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate("AlteredCarbon.CannotCopyNoIntellectual"), null,
                            MenuOptionPriority.Default, null, null, 0f, null, null);
                        yield return floatMenuOption;
                    }
                }
                else
                {
                    FloatMenuOption floatMenuOption = new FloatMenuOption(Translator.Translate(
                        "AlteredCarbon.CannotCopyNoOtherEmptyStacks"), null,
                            MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption;
                }
            }

            yield break;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.hasPawn)
            {
                stringBuilder.Append("AlteredCarbon.Name".Translate() + ": " + this.name + "\n");
                stringBuilder.Append("AlteredCarbon.faction".Translate() + ": " + this.faction.Name + "\n");
                Backstory newChildhood = null;
                BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
                stringBuilder.Append("AlteredCarbon.childhood".Translate() + ": " + newChildhood.title.CapitalizeFirst() + "\n");

                if (this.adulthood?.Length > 0)
                {
                    Backstory newAdulthood = null;
                    BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
                    stringBuilder.Append("AlteredCarbon.adulthood".Translate() + ": " + newAdulthood.title.CapitalizeFirst() + "\n");
                }
                stringBuilder.Append("AlteredCarbon.ageChronologicalTicks".Translate() + ": " + (int)(this.ageChronologicalTicks / 3600000) + "\n");
                stringBuilder.Append("Gender".Translate() + ": " + this.gender.GetLabel().CapitalizeFirst() + "\n");

            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (this.Destroyed)
            {
                this.KillInnerPawn();
            }
        }
        public void KillInnerPawn(bool affectFactionRelationship = false, Pawn affecter = null)
        {
            if (this.hasPawn)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                this.OverwritePawn(pawn);
                if (affectFactionRelationship)
                {
                    this.faction.TryAffectGoodwillWith(affecter.Faction, -70, canSendMessage: true, canSendHostilityLetter: true,
                        "AlteredCarbon.GoodwillChangedReason_ErasedPawn".Translate(pawn.Named("PAWN")), affecter);
                    QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
                }
                if (isFactionLeader)
                {
                    pawn.Faction.leader = pawn;
                }
                pawn.Kill(null);
            }
        }
        public void EmptyStack(Pawn affecter, bool affectFactionRelationship = false)
        {
            Find.WindowStack.Add(new Dialog_MessageBox("AlteredCarbon.EmptyStackConfirmation".Translate(),
                "No".Translate(), null,
                "Yes".Translate(), delegate ()
                {
                    float damageChance = Mathf.Abs((affecter.skills.GetSkill(SkillDefOf.Intellectual).levelInt / 2f) - 11f) / 10f;
                    if (Rand.Chance(damageChance))
                    {
                        Find.LetterStack.ReceiveLetter("AlteredCarbon.DestroyedStack".Translate(),
                        "AlteredCarbon.DestroyedWipingStackDesc".Translate(affecter.Named("PAWN")),
                        LetterDefOf.NegativeEvent, affecter);
                        ACUtils.ACTracker.stacksIndex.Remove(this.pawnID + this.name);
                        this.KillInnerPawn(affectFactionRelationship, affecter);
                        this.Destroy();
                    }
                    else
                    {
                        var newStack = ThingMaker.MakeThing(AlteredCarbonDefOf.AC_EmptyCorticalStack);
                        GenSpawn.Spawn(newStack, this.Position, this.Map);
                        Find.Selector.Select(newStack);
                        ACUtils.ACTracker.stacksIndex.Remove(this.pawnID + this.name);
                        this.KillInnerPawn(affectFactionRelationship, affecter);
                        this.Destroy();
                    }
                }, null, false, null, null));
        }
        public void SavePawnFromHediff(Hediff_CorticalStack hediff)
        {
            this.name = hediff.name;
            this.origPawn = hediff.origPawn;
            this.hostilityMode = hediff.hostilityMode;
            this.areaRestriction = hediff.areaRestriction;
            this.ageChronologicalTicks = hediff.ageChronologicalTicks;
            this.medicalCareCategory = hediff.medicalCareCategory;
            this.selfTend = hediff.selfTend;
            this.foodRestriction = hediff.foodRestriction;
            this.outfit = hediff.outfit;
            this.drugPolicy = hediff.drugPolicy;
            this.times = hediff.times;
            this.thoughts = hediff.thoughts;
            this.faction = hediff.faction;
            this.isFactionLeader = hediff.isFactionLeader;
            this.traits = hediff.traits;
            this.relations = hediff.relations;
            this.relatedPawns = hediff.relatedPawns;
            this.skills = hediff.skills;
            this.childhood = hediff.childhood;
            this.adulthood = hediff.adulthood;
            this.priorities = hediff.priorities;
            this.hasPawn = true;

            if (this.gender == Gender.None)
            {
                this.gender = hediff.gender;
            }


            this.pawnID = hediff.pawnID;

            if (ModLister.RoyaltyInstalled)
            {
                this.royalTitles = hediff.royalTitles;
                this.favor = hediff.favor;
                this.heirs = hediff.heirs;
                this.bondedThings = hediff.bondedThings;
            }
            this.isCopied = hediff.isCopied;
            this.stackGroupID = hediff.stackGroupID;
        }

        public void SavePawnToCorticalStack(Pawn pawn)
        {
            this.name = pawn.Name;
            this.origPawn = pawn;
            if (pawn.playerSettings != null)
            {
                this.hostilityMode = (int)pawn.playerSettings.hostilityResponse;
                this.areaRestriction = pawn.playerSettings.AreaRestriction;
                this.medicalCareCategory = pawn.playerSettings.medCare;
                this.selfTend = pawn.playerSettings.selfTend;
            }
            this.ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
            this.foodRestriction = pawn.foodRestriction?.CurrentFoodRestriction;

            this.outfit = pawn.outfits?.CurrentOutfit;
            this.drugPolicy = pawn.drugs?.CurrentPolicy;
            this.times = pawn.timetable?.times;
            this.thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories;
            this.faction = pawn.Faction;
            if (pawn.Faction.leader == pawn)
            {
                this.isFactionLeader = true;
            }
            this.traits = pawn.story?.traits?.allTraits;
            this.relations = pawn.relations?.DirectRelations;
            this.relatedPawns = pawn.relations.RelatedPawns.ToHashSet();

            this.skills = pawn.skills?.skills;
            this.childhood = pawn.story?.childhood?.identifier;
            if (pawn.story?.adulthood != null)
            {
                this.adulthood = pawn.story.adulthood.identifier;
            }
            this.priorities = new Dictionary<WorkTypeDef, int>();
            if (pawn.workSettings != null && Traverse.Create(pawn.workSettings).Field("priorities").GetValue<DefMap<WorkTypeDef, int>>() != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    this.priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }
            this.hasPawn = true;
            this.pawnID = pawn.ThingID;
            if (ModLister.RoyaltyInstalled)
            {
                this.royalTitles = pawn.royalty?.AllTitlesForReading;
                this.favor = Traverse.Create(pawn.royalty).Field("favor").GetValue<Dictionary<Faction, int>>();
                this.heirs = Traverse.Create(pawn.royalty).Field("heirs").GetValue<Dictionary<Faction, Pawn>>();
                foreach (var map in Find.Maps)
                {
                    foreach (var thing in map.listerThings.AllThings)
                    {
                        var comp = thing.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(thing);
                        }
                    }
                    foreach (var gear in pawn.apparel.WornApparel)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.equipment.AllEquipmentListForReading)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                    foreach (var gear in pawn.inventory.innerContainer)
                    {
                        var comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                        }
                        if (comp != null && comp.bondedPawn == pawn)
                        {
                            this.bondedThings.Add(gear);
                        }
                    }
                }
            }
        }

        public void CopyFromOtherStack(CorticalStack otherStack)
        {
            this.name = otherStack.name;
            this.origPawn = otherStack.origPawn;
            this.hostilityMode = otherStack.hostilityMode;
            this.areaRestriction = otherStack.areaRestriction;
            this.ageChronologicalTicks = otherStack.ageChronologicalTicks;
            this.medicalCareCategory = otherStack.medicalCareCategory;
            this.selfTend = otherStack.selfTend;
            this.foodRestriction = otherStack.foodRestriction;

            this.outfit = otherStack.outfit;
            this.drugPolicy = otherStack.drugPolicy;
            this.times = otherStack.times;
            this.thoughts = otherStack.thoughts;
            this.faction = otherStack.faction;
            this.isFactionLeader = otherStack.isFactionLeader;
            this.traits = otherStack.traits;
            this.relations = otherStack.relations;
            this.relatedPawns = otherStack.relatedPawns;

            this.skills = otherStack.skills;
            this.childhood = otherStack.childhood;
            this.adulthood = otherStack.adulthood;
            this.priorities = otherStack.priorities;
            this.hasPawn = true;
            if (this.gender == Gender.None)
            {
                this.gender = otherStack.gender;
            }

            this.pawnID = otherStack.pawnID;

            if (ModLister.RoyaltyInstalled)
            {
                this.royalTitles = otherStack.royalTitles;
                this.favor = otherStack.favor;
                this.heirs = otherStack.heirs;
                this.bondedThings = otherStack.bondedThings;
            }
            this.isCopied = true;
            this.stackGroupID = otherStack.stackGroupID;
        }

        public Pawn GetOriginalPawn(Pawn pawn)
        {
            if (this.origPawn != null)
            {
                return this.origPawn;
            }
            if (this.relatedPawns != null)
            {
                foreach (var otherPawn in this.relatedPawns)
                {
                    if (otherPawn != null)
                    {
                        foreach (var rel in otherPawn.relations.DirectRelations)
                        {
                            if (rel.otherPawn.Name == pawn.Name && rel.otherPawn != pawn)
                            {
                                return rel.otherPawn;
                            }
                        }
                    }
                }
            }
            foreach (var otherPawn in PawnsFinder.AllMaps)
            {
                if (otherPawn.Name == pawn.Name && otherPawn != pawn)
                {
                    return otherPawn;
                }
            }

            return null;
        }

        public void OverwritePawn(Pawn pawn)
        {
            var extension = this.def.GetModExtension<StackSavingOptionsModExtension>();
            if (pawn.Faction != this.faction)
            {
                pawn.SetFaction(this.faction);
            }
            if (this.isFactionLeader)
            {
                pawn.Faction.leader = pawn;
            }

            pawn.Name = this.name;
            if (pawn.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
                }
            }

            if (this.thoughts != null)
            {
                if (this.gender == pawn.gender)
                {
                    this.thoughts.RemoveAll(x => x.def == AlteredCarbonDefOf.AC_WrongGender);
                    this.thoughts.RemoveAll(x => x.def == AlteredCarbonDefOf.AC_WrongGenderDouble);

                }
                foreach (var thought in this.thoughts)
                {
                    if (thought is Thought_MemorySocial && thought.otherPawn == null)
                    {
                        continue;
                    }
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                }
            }
            pawn.story.traits.allTraits.RemoveAll(x => !extension.ignoresTraits.Contains(x.def.defName));
            if (this.traits != null)
            {
                foreach (var trait in this.traits)
                {
                    if (extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                    {
                        continue;
                    }
                    pawn.story.traits.GainTrait(trait);
                }
            }

            pawn.relations.ClearAllRelations();

            var origPawn = GetOriginalPawn(pawn);

            foreach (var otherPawn in this.relatedPawns)
            {
                if (otherPawn != null)
                {
                    foreach (var rel in otherPawn.relations.DirectRelations)
                    {
                        if (this.name == rel.otherPawn?.Name)
                        {
                            //Log.Message("1 Changing Rel: " + pawn.Name + " - " + rel.def + " - " + otherPawn.Name + " - " + rel.otherPawn.Name, true);
                            rel.otherPawn = pawn;
                        }
                    }
                }
            }

            foreach (var otherPawn in this.relatedPawns)
            {
                if (otherPawn != null)
                {
                    foreach (var rel in this.relations)
                    {
                        foreach (var rel2 in otherPawn.relations.DirectRelations)
                        {
                            if (rel.def == rel2.def && rel2.otherPawn?.Name == pawn.Name)
                            {
                                //Log.Message("2 Changing Rel: " + pawn.Name + " - " + rel.def + " - " + otherPawn.Name + " - " + rel.otherPawn.Name, true);
                                rel2.otherPawn = pawn;
                            }
                        }
                    }
                }

            }

            foreach (var rel in this.relations)
            {
                //Log.Message("Adding Rel: " + pawn.Name + " - " + rel.def + " - " + rel.otherPawn.Name, true);
                if (rel.otherPawn != null)
                {
                    var oldRelation = rel.otherPawn.relations.DirectRelations.Where(r => r.def == rel.def && r.otherPawn.Name == pawn.Name).FirstOrDefault();
                    if (oldRelation != null)
                    {
                        //Log.Message("3 Changing Rel: " + pawn.Name + " - " + rel.def + " - " + oldRelation.otherPawn.Name + " - " + rel.otherPawn.Name, true);
                        oldRelation.otherPawn = pawn;
                    }
                }

                //foreach (var child in rel.otherPawn.relations.Children)
                //{
                //    if (child != null)
                //    {
                //        if (child.Name == pawn.Name)
                //        {
                //            var oldRelation2 = child.relations.DirectRelations.Where(r => r.def == rel.def && r.otherPawn == rel.otherPawn).FirstOrDefault();
                //            if (oldRelation2 != null)
                //            {
                //                var otherRelation = oldRelation2.otherPawn.relations.GetDirectRelation(oldRelation2.def, child);
                //                Log.Message("4 Changing Rel: " + pawn.Name + " - " + rel.def + " - " + otherRelation.otherPawn.Name + " - " + rel.otherPawn.Name, true);
                //                otherRelation.otherPawn = pawn;
                //            }
                //        }
                //    }
                //}

                //foreach (var rel2 in rel.otherPawn.relations.DirectRelations)
                //{
                //    if (rel2.def == rel.def)
                //    {
                //        Log.Message("Check rel: " + rel.otherPawn.Name + " - " + rel2.def + " - " + rel2.otherPawn.Name, true);
                //    }
                //}

                pawn.relations.AddDirectRelation(rel.def, rel.otherPawn);

                //foreach (var children in rel.otherPawn.relations.Children)
                //{
                //    //Log.Message("1.5: " + rel.otherPawn.Name + " - child: " + children.Name, true);
                //}
                //Log.Message("-------------");
            }

            //foreach (var otherPawn in pawn.relations.RelatedPawns)
            //{
            //    for (int num = otherPawn.relations.DirectRelations.Count - 1; num >= 0; num--)
            //    {
            //        if (pawn.Name == otherPawn.relations.DirectRelations[num].otherPawn.Name)
            //        {
            //            if (pawn != otherPawn.relations.DirectRelations[num].otherPawn)
            //            {
            //                Log.Message("5 Rel: " + pawn.Name + " - " + otherPawn.relations.DirectRelations[num].def + " - " + otherPawn.Name + " - " + otherPawn.relations.DirectRelations[num].otherPawn.Name, true);
            //                Log.Message("6 pawn != otherPawn: " + pawn + " - " + otherPawn.relations.DirectRelations[num].otherPawn, true);
            //            }
            //        }
            //    }
            //}
            //
            //foreach (var children in pawn.relations.Children)
            //{
            //    Log.Message("7: " + pawn.Name + " - child: " + children.Name, true);
            //}

            if (origPawn != null)
            {
                origPawn.relations = new Pawn_RelationsTracker(origPawn);
            }

            pawn.skills.skills.Clear();
            if (this.skills != null)
            {
                foreach (var skill in this.skills)
                {
                    var newSkill = new SkillRecord(pawn, skill.def);
                    newSkill.passion = skill.passion;
                    newSkill.levelInt = skill.levelInt;
                    newSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                    newSkill.xpSinceMidnight = skill.xpSinceMidnight;
                    pawn.skills.skills.Add(newSkill);
                }
            }
            if (pawn.playerSettings == null) pawn.playerSettings = new Pawn_PlayerSettings(pawn);
            pawn.playerSettings.hostilityResponse = (HostilityResponseMode)this.hostilityMode;

            Backstory newChildhood = null;
            BackstoryDatabase.TryGetWithIdentifier(this.childhood, out newChildhood, true);
            pawn.story.childhood = newChildhood;
            if (this.adulthood?.Length > 0)
            {
                Backstory newAdulthood = null;
                BackstoryDatabase.TryGetWithIdentifier(this.adulthood, out newAdulthood, true);
                pawn.story.adulthood = newAdulthood;
            }
            else
            {
                pawn.story.adulthood = null;
            }
            if (pawn.workSettings == null) pawn.workSettings = new Pawn_WorkSettings();
            pawn.Notify_DisabledWorkTypesChanged();
            if (priorities != null)
            {
                foreach (var priority in priorities)
                {
                    pawn.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }
            pawn.playerSettings.AreaRestriction = this.areaRestriction;
            pawn.playerSettings.medCare = this.medicalCareCategory;
            pawn.playerSettings.selfTend = this.selfTend;
            if (pawn.foodRestriction == null) pawn.foodRestriction = new Pawn_FoodRestrictionTracker();
            pawn.foodRestriction.CurrentFoodRestriction = this.foodRestriction;
            if (pawn.outfits == null) pawn.outfits = new Pawn_OutfitTracker();
            pawn.outfits.CurrentOutfit = this.outfit;
            if (pawn.drugs == null) pawn.drugs = new Pawn_DrugPolicyTracker();
            pawn.drugs.CurrentPolicy = this.drugPolicy;
            pawn.ageTracker.AgeChronologicalTicks = this.ageChronologicalTicks;
            if (pawn.timetable == null) pawn.timetable = new Pawn_TimetableTracker(pawn);
            if (this.times != null) pawn.timetable.times = this.times;
            if (pawn.gender != this.gender)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.BodyPurist))
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGenderDouble);
                }
                else
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AlteredCarbonDefOf.AC_WrongGender);
                }
            }
            if (ModLister.RoyaltyInstalled)
            {
                if (pawn.royalty == null) pawn.royalty = new Pawn_RoyaltyTracker(pawn);
                if (this.royalTitles != null)
                {
                    foreach (var title in this.royalTitles)
                    {
                        pawn.royalty.SetTitle(title.faction, title.def, false, false, false);
                    }
                }
                if (this.heirs != null)
                {
                    foreach (var heir in this.heirs)
                    {
                        pawn.royalty.SetHeir(heir.Value, heir.Key);
                    }
                }

                if (this.favor != null)
                {
                    foreach (var fav in this.favor)
                    {
                        pawn.royalty.SetFavor(fav.Key, fav.Value);
                    }
                }

                if (this.bondedThings != null)
                {
                    foreach (var bonded in this.bondedThings)
                    {
                        var comp = bonded.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            comp.bondedPawn = pawn;
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.stackGroupID, "stackGroupID", 0);
            Scribe_Values.Look<bool>(ref this.isCopied, "isCopied", false, false);
            Scribe_Deep.Look<Name>(ref this.name, "name", new object[0]);
            Scribe_References.Look<Pawn>(ref this.origPawn, "origPawn", true);
            Scribe_Values.Look<int>(ref this.hostilityMode, "hostilityMode");
            Scribe_References.Look<Area>(ref this.areaRestriction, "areaRestriction", false);
            Scribe_Values.Look<MedicalCareCategory>(ref this.medicalCareCategory, "medicalCareCategory", 0, false);
            Scribe_Values.Look<bool>(ref this.selfTend, "selfTend", false, false);
            Scribe_Values.Look<long>(ref this.ageChronologicalTicks, "ageChronologicalTicks", 0, false);

            Scribe_References.Look<Outfit>(ref this.outfit, "outfit", false);
            Scribe_References.Look<FoodRestriction>(ref this.foodRestriction, "foodPolicy", false);
            Scribe_References.Look<DrugPolicy>(ref this.drugPolicy, "drugPolicy", false);

            Scribe_Collections.Look<TimeAssignmentDef>(ref this.times, "times");
            Scribe_Collections.Look<Thought_Memory>(ref this.thoughts, "thoughts");
            Scribe_References.Look<Faction>(ref this.faction, "faction", true);
            Scribe_Values.Look<bool>(ref this.isFactionLeader, "isFactionLeader", false, false);

            Scribe_Values.Look<string>(ref this.childhood, "childhood", null, false);
            Scribe_Values.Look<string>(ref this.adulthood, "adulthood", null, false);

            Scribe_Values.Look<string>(ref this.pawnID, "pawnID", null, false);
            Scribe_Collections.Look<Trait>(ref this.traits, "traits");
            Scribe_Collections.Look<SkillRecord>(ref this.skills, "skills");
            Scribe_Collections.Look<DirectPawnRelation>(ref this.relations, "relations");
            Scribe_Collections.Look<Pawn>(ref this.relatedPawns, "relatedPawns", LookMode.Reference);

            Scribe_Collections.Look<WorkTypeDef, int>(ref this.priorities, "priorities");
            Scribe_Values.Look<bool>(ref this.hasPawn, "hasPawn", false, false);

            Scribe_Values.Look<Gender>(ref this.gender, "gender", 0, false);
            if (ModLister.RoyaltyInstalled)
            {
                Scribe_Collections.Look<Faction, int>(ref this.favor, "favor",
                    LookMode.Reference, LookMode.Value,
                    ref this.favorKeys, ref this.favorValues);

                Scribe_Collections.Look<Faction, Pawn>(ref this.heirs, "heirs",
                    LookMode.Reference, LookMode.Reference,
                    ref this.heirsKeys, ref this.heirsValues);

                Scribe_Collections.Look<Thing>(ref this.bondedThings, "bondedThings", LookMode.Reference);
                Scribe_Collections.Look<RoyalTitle>(ref this.royalTitles, "royalTitles", LookMode.Deep);
            }
        }

        private List<Faction> favorKeys = new List<Faction>();
        private List<int> favorValues = new List<int>();

        private List<Faction> heirsKeys = new List<Faction>();
        private List<Pawn> heirsValues = new List<Pawn>();
    }
}