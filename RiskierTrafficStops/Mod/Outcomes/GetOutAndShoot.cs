﻿using static RiskierTrafficStops.Engine.Helpers.Extensions.PedExtensions;

// ReSharper disable HeapView.BoxingAllocation

namespace RiskierTrafficStops.Mod.Outcomes;

internal class GetOutAndShoot : Outcome
{
    private static GetOutAndShootOutcomes _chosenOutcome;

    private static GetOutAndShootOutcomes[] _allGoasOutcomes =
        (GetOutAndShootOutcomes[])Enum.GetValues(typeof(GetOutAndShootOutcomes));
    
    private static List<Ped> _pedsInVehicle = SuspectVehicle.Occupants.ToList();
    
    private enum GetOutAndShootOutcomes
    {
        Flee,
        KeepShooting
    }

    // RTSGetOutAndShootSuspects
    public GetOutAndShoot(LHandle handle) : base(handle)
    {
        try
        {
            if (MeetsRequirements(TrafficStopLHandle))
            {
                SuspectRelateGroup = new RelationshipGroup("RTSGetOutAndShootSuspects");
                GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(StartOutcome));
            }
        }
        catch (Exception e)
        {
            if (e is ThreadAbortException) return;
            Error(e, nameof(StartOutcome));
            CleanupOutcome(true);
        }
    }

    internal override void StartOutcome()
    {
        InvokeEvent(RTSEventType.Start);

        Normal("Adding all suspect in the vehicle to a list");
        
        if (_pedsInVehicle.Count < 1) throw new ArgumentNullException(nameof(_pedsInVehicle));

        foreach (var ped in _pedsInVehicle.Where(ped => ped.IsAvailable() && PedsToIgnore.Contains(ped)))
        {
            _pedsInVehicle.Remove(ped);
        }
        
        SetRelationshipGroups(SuspectRelateGroup);

        foreach (var ped in _pedsInVehicle)
        {
            ped.GiveWeapon();
            GameFiberHandling.OutcomeGameFibers.Add(GameFiber.StartNew(() => GetPedOutOfVehicle(ped)));
        }

        GameFiber.Wait(7010);

        Normal("Choosing outcome from GetOutAndShootOutcomes");
        _chosenOutcome = _allGoasOutcomes.PickRandom();
        Normal($"Chosen Outcome: {_chosenOutcome}");

        switch (_chosenOutcome)
        {
            case GetOutAndShootOutcomes.Flee:
                if (Functions.GetCurrentPullover() == null)
                {
                    CleanupOutcome(false);
                    return;
                }

                PursuitLHandle = SetupPursuitWithList(true, _pedsInVehicle);
                break;
            case GetOutAndShootOutcomes.KeepShooting:
                foreach (var i in _pedsInVehicle.Where(i => i.IsAvailable()))
                {
                    Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
                    i.Tasks.FightAgainstClosestHatedTarget(40f, -1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        InvokeEvent(RTSEventType.End);
    }

    private static void GetPedOutOfVehicle(Ped ped)
    {
        ped.RelationshipGroup = SuspectRelateGroup;
        if (ped.IsInVehicle(ped.LastVehicle, false) && ped.LastVehicle.IsAvailable())
        {
            Normal("Making Suspect leave vehicle");
            ped.Tasks.LeaveVehicle(ped.LastVehicle, LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();
        }
        Normal("Giving Suspect FightAgainstClosestHatedTarget Task");
        ped.Tasks.FightAgainstClosestHatedTarget(40f, 7000).WaitForCompletion(7001);
    }
}