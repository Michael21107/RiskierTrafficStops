﻿using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.PedExtensions;

namespace RiskierTrafficStops.Mod.Outcomes
{
    internal static class Yelling
    {
        private enum YellingScenarioOutcomes
        {
            GetBackInVehicle,
            ContinueYelling,
            PullOutKnife
        }

        private static Ped _suspect;
        private static Vehicle _suspectVehicle;
        private static RelationshipGroup _suspectRelationshipGroup = new("RTSYellingSuspects");
        private static YellingScenarioOutcomes _chosenOutcome;
        private static bool _isSuspectInVehicle;

        internal static void YellingOutcome(LHandle handle)
        {
            try
            {
                APIs.InvokeEvent(RTSEventType.Start);
                if (!GetSuspectAndSuspectVehicle(handle, out _suspect, out _suspectVehicle))
                {
                    Normal("Failed to get suspect and vehicle, cleaning up RTS event...");
                    CleanupEvent();
                    return;
                }

                Normal("Making Suspect Leave Vehicle");
                _suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion(30000);
                Normal("Making Suspect Face Player");
                NativeFunction.Natives.x5AD23D40115353AC(_suspect, MainPlayer, -1);

                Normal("Making suspect Yell at Player");
                const int timesToSpeak = 2;

                for (var i = 0; i < timesToSpeak; i++)
                {
                    if (_suspect.IsAvailable())
                    {
                        Normal($"Making Suspect Yell, time: {i}");
                        _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                        GameFiber.WaitWhile(() => _suspect.IsAvailable() && _suspect.IsAnySpeechPlaying, 30000);
                    }
                }

                Normal("Choosing outcome from possible Yelling outcomes");
                var scenarioList = (YellingScenarioOutcomes[])Enum.GetValues(typeof(YellingScenarioOutcomes));
                _chosenOutcome = scenarioList[Rndm.Next(scenarioList.Length)];
                Normal($"Chosen Outcome: {_chosenOutcome}");

                switch (_chosenOutcome)
                {
                    case YellingScenarioOutcomes.GetBackInVehicle:
                        if (_suspect.IsAvailable() && !Functions.IsPedArrested(_suspect)) //Double checking if suspect exists
                        {
                            _suspect.Tasks.EnterVehicle(_suspectVehicle, -1);
                        }
                        break;
                    case YellingScenarioOutcomes.PullOutKnife:
                        OutcomePullKnife();
                        break;
                    case YellingScenarioOutcomes.ContinueYelling:
                        GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(KeyPressed));
                        while (!_isSuspectInVehicle && _suspect.IsAvailable() && (!Functions.IsPedArrested(_suspect) || Functions.IsPedGettingArrested(_suspect)))
                        {
                            GameFiber.Yield();
                            _suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
                            GameFiber.WaitWhile(() => _suspect.IsAvailable() && _suspect.IsAnySpeechPlaying);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException) return;
                Error(e, nameof(YellingOutcome));
                CleanupEvent();
            }
            
            GameFiberHandling.CleanupFibers();
            APIs.InvokeEvent(RTSEventType.End);
        }

        private static void KeyPressed()
        {
            Game.DisplayHelp($"~BLIP_INFO_ICON~ Press ~{Settings.GetBackInKey.GetInstructionalId()}~ to have the suspect get back in their vehicle", 10000);
            while (_suspect.IsAvailable() && !_isSuspectInVehicle)
            {
                GameFiber.Yield();
                if (Game.IsKeyDown(Settings.GetBackInKey))
                {
                    _isSuspectInVehicle = true;
                    _suspect.Tasks.EnterVehicle(_suspectVehicle, -1).WaitForCompletion();
                    break;
                }
            }
        }

        private static void OutcomePullKnife()
        {
            if (!_suspect.IsAvailable() || Functions.IsPedArrested(_suspect) ||
                Functions.IsPedGettingArrested(_suspect)) return;
            
            _suspect.BlockPermanentEvents = true;
            _suspect.Inventory.GiveNewWeapon(MeleeWeapons[Rndm.Next(MeleeWeapons.Length)], -1, true);

            SetRelationshipGroups(_suspectRelationshipGroup);
            _suspect.RelationshipGroup = _suspectRelationshipGroup;

            Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
            _suspect.Tasks.FightAgainstClosestHatedTarget(40f, -1);
        }
    }
}