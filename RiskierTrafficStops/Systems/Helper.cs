﻿using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiskierTrafficStops.Systems.Logger;

namespace RiskierTrafficStops.Systems
{
    internal static class Helper
    {
        internal static Ped MainPlayer => Game.LocalPlayer.Character;
        internal static readonly Random Rndm = new(DateTime.Now.Millisecond);

        /// <summary>
        /// Setup a Pursuit with an Array of suspects
        /// </summary>
        /// <param name="isSuspectsPulledOver"></param>
        /// <param name="suspects"></param>
        /// <returns>PursuitLHandle</returns>

        internal static LHandle SetupPursuit(bool isSuspectsPulledOver, params Ped[] suspects)
        {
            if (isSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            var pursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

            for (var i = suspects.Length - 1; i >= 0; i--)
            {
                if (!suspects[i].Exists()) { continue; }
                Functions.AddPedToPursuit(pursuitLHandle, suspects[i]);
            }

            return pursuitLHandle;
        }

        /*internal static Vehicle GetVehicleBehindPlayerVehicle()
        {
            Vehicle[] i = World.GetAllVehicles().Where(i => (i.DistanceTo2D(MainPlayer.Position) < 35f) && i.HasDriver && !i.IsPersistent && !i.IsBicycle && (i.Class != VehicleClass.Motorcycle) && !i.IsBoat && !i.IsPlane && !i.Model.IsEmergencyVehicle && (i.GetPositionOffset(MainPlayer.LastVehicle.Position).Y <= 3f) && CheckIfHeadingIsWithinRange(MainPlayer.LastVehicle.Heading, i.Heading, 20f) && CheckZDistance(MainPlayer.LastVehicle.Position.Z, i.Position.Z, 5f)).ToArray();

            return i[Rndm.Next(i.Length)];
        }*/

        internal static Vector3 GetRearOffset(Vehicle vehicle, float offset)
        {
            var backwardDirection = vehicle.RearPosition - vehicle.FrontPosition;
            backwardDirection.Normalize();
            return (backwardDirection * offset) + vehicle.RearPosition;
        }

        /// <summary>
        /// Returns the nearest vehicle to a position
        /// </summary>

        internal static Vehicle GetNearestVehicle(Vector3 position, float maxDistance = 40f)
        {
            var vehicles = MainPlayer.GetNearbyVehicles(16).ToList();
            if (vehicles.Count < 1)
                throw new ArgumentOutOfRangeException();

            var nearestVehicles = vehicles.OrderBy(vehicles1 => vehicles1.DistanceTo(position)).ToList();
            var vehicle = nearestVehicles[0];

            return vehicle;
        }

        /*/// <summary>
        /// Checks if the given heading is within a range of headingToCheckAgainst, the range is in both directions, for example 10f as a range would translate to if its within a range of 10f to the left or 10f to the right
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool CheckIfHeadingIsWithinRange(float referenceHeading, float headingToCheck, float range)
        {
            float absoluteDifference = Math.Abs(referenceHeading - headingToCheck);

            if (absoluteDifference > 180f)
            {
                absoluteDifference = 360f - absoluteDifference;
            }

            return absoluteDifference <= range;
        }*/

        /// <summary>
        /// Returns the Driver and its vehicle
        /// </summary>
        /// <returns>Ped, Vehicle</returns>

        internal static bool GetSuspectAndVehicle(LHandle handle, out Ped suspect, out Vehicle suspectVehicle)
        {
            Ped driver = null;
            Vehicle driverVehicle = null;
            if ((handle != null) && Functions.IsPlayerPerformingPullover())
            {
                Debug("Setting up Suspect");
                driver = Functions.GetPulloverSuspect(handle);
                Debug("Setting driver as persistent and Blocking permanent events");
                driver.IsPersistent = true;
                driver.BlockPermanentEvents = true;
            }
            if (driver != null && driver.Exists() && driver.IsInAnyVehicle(false) && !driver.IsInAnyPoliceVehicle)
            {
                Debug("Setting up Suspect Vehicle");
                driverVehicle = driver.LastVehicle;
                Debug("Setting driver vehicle as Persistent");
                driverVehicle.IsPersistent = true;
            }
            Debug($"Returning Driver: {driver} & Driver Vehicle: {driverVehicle}");
            suspect = driver;
            suspectVehicle = driverVehicle;
            return suspect.Exists() && suspectVehicle.Exists();
        }

        internal static void CleanupEvent(List<Ped> peds, Vehicle vehicle)
        {
            for (var i = peds.Count - 1; i >= 0; i--)
            {
                if (peds[i].Exists())
                {
                    peds[i].IsPersistent = false;
                }
            }
            if (vehicle.Exists())
            {
                vehicle.IsPersistent = false;
            }

            PulloverEventHandler.HasEventHappened = false;
        }

        internal static void CleanupEvent(Ped suspect, Vehicle vehicle)
        {
            if (suspect.Exists())
            {
                suspect.IsPersistent = false;
            }
            else if (vehicle.Exists())
            {
                vehicle.IsPersistent = false;
            }

            PulloverEventHandler.HasEventHappened = false;
        }

        internal static void CleanupEvent(Ped suspect)
        {
            if (suspect.Exists())
            {
                suspect.IsPersistent = false;
            }
        }

        /// <summary>
        /// Same as SetupPursuit but with a suspect list
        /// </summary>
        /// <param name="isSuspectsPulledOver">If the suspects are in a traffic stop</param>
        /// <param name="suspectList">The list of Suspects, Type=Ped</param>
        /// <returns>PursuitLHandle</returns>

        internal static LHandle SetupPursuitWithList(bool isSuspectsPulledOver, List<Ped> suspectList)
        {
            if (isSuspectsPulledOver)
            {
                Functions.ForceEndCurrentPullover();
            }
            var pursuitLHandle = Functions.CreatePursuit();

            Functions.SetPursuitIsActiveForPlayer(pursuitLHandle, true);

            for (var i = suspectList.Count - 1; i >= 0; i--)
            {
                GameFiber.Yield();
                if (suspectList[i].Exists())
                {
                    Functions.AddPedToPursuit(pursuitLHandle, suspectList[i]);
                }
            }
            return pursuitLHandle;
        }

        /// <summary>
        /// Converts MPH to meters per second which is what all tasks use, returns meters per second
        /// </summary>
        internal static float MphToMps(float speed)
        {
            var newSpeed = MathHelper.ConvertMilesPerHourToMetersPerSecond(speed);
            return newSpeed;
        }

        /// <summary>
        /// List of (Almost) every weapon
        /// </summary>

        internal static readonly string[] WeaponList = {
            "weapon_pistol",
            "weapon_pistol_mk2",
            "weapon_combatpistol",
            "weapon_appistol",
            "weapon_pistol50",
            "weapon_snspistol",
            "weapon_snspistol_mk2",
            "weapon_heavypistol",
            "weapon_vintagepistol",
            "weapon_microsmg",
            "weapon_smg",
            "weapon_smg_mk2",
            "weapon_assaultsmg",
            "weapon_combatpdw",
            "weapon_machinepistol",
            "weapon_minismg",
            "weapon_pumpshotgun",
            "weapon_pumpshotgun_mk2",
            "weapon_sawnoffshotgun",
            "weapon_assaultshotgun",
            "weapon_bullpupshotgun",
            "weapon_combatshotgun",
            "weapon_carbinerifle",
            "weapon_carbinerifle_mk2",
            "weapon_advancedrifle",
            "weapon_specialcarbine",
            "weapon_specialcarbine_mk2",
            "weapon_bullpuprifle",
            "weapon_bullpuprifle_mk2",
            "weapon_compactrifle",
            "weapon_militaryrifle",
            "weapon_tacticalrifle",
        };

        /// <summary>
        /// List of all Weapons that can be fired from inside of a vehicle
        /// </summary>

        internal static readonly string[] PistolList =
        {
            "weapon_pistol",
            "weapon_pistol_mk2",
            "weapon_combatpistol",
            "weapon_appistol",
            "weapon_pistol50",
            "weapon_snspistol",
            "weapon_snspistol_mk2",
            "weapon_heavypistol",
            "weapon_microsmg",
        };

        /// <summary>
        /// List of all viable melee Weapons
        /// </summary>

        internal static readonly string[] MeleeWeapons = {
            "weapon_dagger",
            "weapon_bat",
            "weapon_bottle",
            "weapon_crowbar",
            "weapon_hammer",
            "weapon_hatchet",
            "weapon_knife",
            "weapon_switchblade",
            "weapon_machete",
            "weapon_wrench",
};

        /// <summary>
        /// Makes a ped rev their vehicles engine, the int list parameters each need a minimum and maximum value
        /// </summary>
        internal static void RevEngine(Ped driver, Vehicle suspectVehicle, int[] timeBetweenRevs, int[] timeForRevsToLast, int totalNumberOfRevs)
        {
            Logger.Debug("Starting Rev Engine method");
            for (var i = 0; i < totalNumberOfRevs; i++)
            {
                GameFiber.Yield();
                var time = Rndm.Next(timeForRevsToLast[0], timeForRevsToLast[1]) * 1000;
                driver.Tasks.PerformDrivingManeuver(suspectVehicle, VehicleManeuver.RevEngine, time);
                GameFiber.Wait(time);
                var time2 = Rndm.Next(timeBetweenRevs[0], timeBetweenRevs[1]) * 1000;
                GameFiber.Wait(time2);
            }
        }

        internal static List<Ped> GetAllVehicleOccupants(Vehicle vehicle)
        {
            var seatCount = vehicle.PassengerCount; //Testing rph method instead of NativeFunction.Natives.GET_VEHICLE_NUMBER_OF_PASSENGERS<int>(vehicle, true, false);
            List<Ped> occupantList = new() { vehicle.GetPedOnSeat(-1) //vehicle.PassengerCount does not include the driver, so driver is being added here
            };
            for (var i = 0; i < seatCount; i++)
            {
                if (vehicle.IsSeatFree(i)) { continue; }
                var ped = vehicle.GetPedOnSeat(i);
                if (ped.Exists())
                {
                    occupantList.Add(ped);
                }
            }
            Logger.Debug($"Peds In Vehicle: {occupantList.Count}");
            return occupantList;
        }

        internal static bool CheckZDistance(float z1, float z2, float range)
        {
            var difference = Math.Abs(z1 - z2);
            return difference <= range;
        }

        /// <summary>
        /// Array of Used curse voice-lines
        /// </summary>

        internal static readonly string[] VoiceLines = {
            "FIGHT",
            "GENERIC_INSULT_HIGH",
            "GENERIC_CURSE_MED",
            "CHALLENGE_THREATEN",
            "GENERIC_CURSE_HIGH",
            "GENERIC_INSULT_HIGH_01",
        };
    }
}