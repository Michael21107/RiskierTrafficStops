﻿using System;
using System.Threading;
using LSPD_First_Response.Mod.API;
using Rage;
using RiskierTrafficStops.API;
using RiskierTrafficStops.Engine.InternalSystems;
using static RiskierTrafficStops.Engine.Helpers.Helper;
using static RiskierTrafficStops.Engine.InternalSystems.Logger;
using static RiskierTrafficStops.Engine.Helpers.Extensions;

namespace RiskierTrafficStops.Mod.Outcomes;

internal class Spitting : Outcome
{
    internal Spitting(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, nameof(StartOutcome));
            CleanupOutcome();
        }
    }

    private static readonly string[] SpittingText =
    {
        "~y~Suspect: ~s~*spits at you* Fuck you pig",
        "~y~Suspect: ~s~*spits at you* Bitch",
        "~y~Suspect: ~s~*spits at you* Come on lets fight!",
        "~y~Suspect: ~s~*spits at you* Motherfucker",
        "~y~Suspect: ~s~*spits at you* Shit I didn't mean to hit you officer",
        "~y~Suspect: ~s~*spits at you* Damnit I didn't see you there",
        "~y~Suspect: ~s~*spits at you* ACAB!",
        "~y~Suspect: ~s~*spits at you and misses* You little bitch",
        "~y~Suspect: ~s~*spits at you and misses* Agh what did I do now",
        "~y~Suspect: ~s~*spits at you and misses* Ope sorry!",
        "~y~Suspect: ~s~*spits at you and hits badge* Haha little bitch",
        "~y~Suspect: ~s~*spits at you and hits badge* I should shoot you for pulling me over",
        "~y~Suspect: ~s~*spits at you and hits badge* I AM SO SORRY OFFICER",
        "~y~Suspect: ~s~*spits at you and hits badge* Oh fuck off",
        "~y~Suspect: ~s~*spits at you and hits shoe* ACAB Bitch!",
        "~y~Suspect: ~s~*spits at you and hits shoe* Fuckin pig",
        "~y~Suspect: ~s~*spits at you and hits shoe* Fucking pig",
        "~y~Suspect: ~s~*spits at you and hits shoe* Your a bitch you know that?",
        "~y~Suspect: ~s~*spits at you and hits shoe* Screw you pig",
        "~y~Suspect: ~s~*spits at you and hits shoe* What are you gonna do now, huh?",
        "~y~Suspect: ~s~*spits at you and hits shoe* Whatcha gonna do you little bitch?",
        "~y~Suspect: ~s~*spits at you and hits shoe* Where's your little squad of bitches?",
        "~y~Suspect: ~s~*spits at you and hits gun* Oh look the bitch patrol!",
        "~y~Suspect: ~s~*spits at you and hits taser* Oh look the bitch patrol!",
    };

    internal override void StartOutcome()
    {
        APIs.InvokeEvent(RTSEventType.Start);

        GameFiber.WaitWhile(
            () => Suspect.IsAvailable() && MainPlayer.DistanceTo(Suspect) >= 3f && Suspect.IsInAnyVehicle(true),
            120000);
        if (Functions.IsPlayerPerformingPullover() && Suspect.IsAvailable() &&
            MainPlayer.DistanceTo(Suspect) <= 2.5f && Suspect.IsInAnyVehicle(true))
        {
            Game.DisplaySubtitle(SpittingText[Rndm.Next(SpittingText.Length)], 6000);
            Suspect.PlayAmbientSpeech(VoiceLines[Rndm.Next(VoiceLines.Length)]);
        }

        GameFiberHandling.CleanupFibers();
        APIs.InvokeEvent(RTSEventType.End);
    }
}