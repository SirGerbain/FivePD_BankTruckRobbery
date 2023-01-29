using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;

namespace BankTruckRobbery
{
    [CalloutProperties("Bank Truck Robbery", "SirGerbain", "1.1")]
    public class BankTruckRobbery : Callout
    {
        private Ped guard1;
        private Ped guard2;
        private Vehicle bankTruck;
        private Ped robber1;
        private Ped robber2;
        private Ped robber3;
        private Ped robber4;
        private Vehicle robberVehicle;
        private Vector3 robberyLocation;
        private List<PedHash> guardHashList = new List<PedHash>();
        private List<PedHash> robberHashList = new List<PedHash>();
        private List<VehicleHash> vehicleHashList = new List<VehicleHash>();
        private float tickTimer = 0f;
        private float tickInterval = 1f;
        private bool isSceneDone = false;
        private bool isChaseStarted = false;

        public BankTruckRobbery()
        {
            Random rnd = new Random();
            float offsetX = rnd.Next(100, 700);
            float offsetY = rnd.Next(100, 700);
            robberyLocation = World.GetNextPositionOnStreet(Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0)));

            vehicleHashList.Add(VehicleHash.Baller4);

            robberHashList.Add(PedHash.Blackops01SMY);
            robberHashList.Add(PedHash.Blackops02SMY);
            robberHashList.Add(PedHash.Blackops03SMY);
            robberHashList.Add(PedHash.Robber01SMY);

            guardHashList.Add(PedHash.Prisguard01SMM);
            guardHashList.Add(PedHash.Sheriff01SMY);
            guardHashList.Add(PedHash.Sheriff01SFY);

            InitInfo(robberyLocation);

            ShortName = "Bank Truck Robbery";
            CalloutDescription = "A bank truck is being robbed on the street.";
            ResponseCode = 3;
            StartDistance = 200f;
        }
        public async override Task OnAccept()
        {
            InitBlip();
            UpdateData();

            PlayerData playerData = Utilities.GetPlayerData();
            string displayName = playerData.DisplayName;
            Notify("~r~[PDM 911] ~y~Officer ~b~" + displayName + ",~y~ Gruppe 6 reported an emergency!");

        }

        public async override void OnStart(Ped player)
        {
            base.OnStart(player);
            await setupCallout();
            Tick += OnTick;
        }

        public async Task setupCallout()
        {
            Random random = new Random();

            bankTruck = await SpawnVehicle(VehicleHash.Stockade, robberyLocation);
      
            guard1 = await SpawnPed(guardHashList[random.Next(guardHashList.Count)], robberyLocation + new Vector3(0, 2, 0));
            guard1.AlwaysKeepTask = true;
            guard1.BlockPermanentEvents = true;
            guard1.Weapons.Give(WeaponHash.Pistol, 250, true, true);

            guard2 = await SpawnPed(guardHashList[random.Next(guardHashList.Count)], robberyLocation + new Vector3(0, 3, 0));
            guard2.AlwaysKeepTask = true;
            guard2.BlockPermanentEvents = true;
            guard2.Weapons.Give(WeaponHash.Pistol, 250, true, true);

            robber1 = await SpawnPed(robberHashList[random.Next(robberHashList.Count)], robberyLocation + new Vector3(10, 10, 0));
            robber1.ArmorFloat = 100f;
            robber2 = await SpawnPed(robberHashList[random.Next(robberHashList.Count)], robberyLocation + new Vector3(10, 11, 0));
            robber2.ArmorFloat = 100f;
            robber3 = await SpawnPed(robberHashList[random.Next(robberHashList.Count)], robberyLocation + new Vector3(10, 10, 0));
            robber3.ArmorFloat = 100f;
            robber4 = await SpawnPed(robberHashList[random.Next(robberHashList.Count)], robberyLocation + new Vector3(10, 11, 0));
            robber4.ArmorFloat = 100f;

            robberVehicle = await SpawnVehicle(vehicleHashList[random.Next(vehicleHashList.Count)], robberyLocation + new Vector3(15, 15, 0));
            robberVehicle.Mods.PrimaryColor = VehicleColor.MetallicBlack;
            robberVehicle.Mods.SecondaryColor = VehicleColor.MetallicBlack;
            robberVehicle.Mods.PearlescentColor = VehicleColor.MetallicBlack;

            robber1.AlwaysKeepTask = true;
            robber1.BlockPermanentEvents = true;
            robber1.Weapons.Give(WeaponHash.MicroSMG, 250, true, true);
            
            robber2.AlwaysKeepTask = true;
            robber2.BlockPermanentEvents = true;
            robber2.Weapons.Give(WeaponHash.MicroSMG, 250, true, true);

            robber3.AlwaysKeepTask = true;
            robber3.BlockPermanentEvents = true;
            robber3.Weapons.Give(WeaponHash.Pistol, 250, true, true);
            robber3.SetIntoVehicle(robberVehicle, VehicleSeat.Driver);

            robber4.AlwaysKeepTask = true;
            robber4.BlockPermanentEvents = true;
            robber4.Weapons.Give(WeaponHash.MicroSMG, 250, true, true);
            robber4.SetIntoVehicle(robberVehicle, VehicleSeat.Passenger);

        }

        public async Task OnTick()
        {
            
            tickTimer += Game.LastFrameTime;
            if (tickTimer >= tickInterval)
            {
                if (!isSceneDone)
                {
                    await ShootOnScene();
                }

                if(isSceneDone)
                { 
                    await InitiateChase();
                }

                if(isSceneDone && !isChaseStarted)
                {

                }

                tickTimer = 0f;
            }

        }
        public async Task InitiateChase()
        {
            if (robber1.IsAlive)
            {
                robber1.Task.EnterVehicle(robberVehicle, VehicleSeat.LeftRear);
            }

            if (robber2.IsAlive)
            {
                robber2.Task.EnterVehicle(robberVehicle, VehicleSeat.RightRear);
            }

            API.SetDriveTaskMaxCruiseSpeed(robber3.GetHashCode(), 250f);
            API.SetDriveTaskDrivingStyle(robber3.GetHashCode(), 524852);
            await BaseScript.Delay(1500);
            robber3.Task.FleeFrom(Game.PlayerPed);


            /*robber1.Task.EnterVehicle(robberVehicle, VehicleSeat.Driver);
            if (robber1.IsDead)
            {
                robber2.Task.EnterVehicle(robberVehicle, VehicleSeat.Driver);
                API.SetDriveTaskMaxCruiseSpeed(robber2.GetHashCode(), 250f);
                API.SetDriveTaskDrivingStyle(robber2.GetHashCode(), 524852);
                await BaseScript.Delay(1500);
                robber2.Task.FleeFrom(Game.PlayerPed);
            }
            else
            {
                robber2.Task.EnterVehicle(robberVehicle, VehicleSeat.Passenger);
                API.SetDriveTaskMaxCruiseSpeed(robber1.GetHashCode(), 250f);
                API.SetDriveTaskDrivingStyle(robber1.GetHashCode(), 524852);
                await BaseScript.Delay(1500);
                robber1.Task.FleeFrom(Game.PlayerPed);
            }*/
        }
        public async Task ShootOnScene()
        {
            Random rnd = new Random();

            if((robber1.IsDead && robber2.IsDead && robber3.IsDead && robber4.IsDead) || (guard1.IsDead && guard1.IsDead))
            {
                robber1.Task.ClearAll();
                robber2.Task.ClearAll();
                robber3.Task.ClearAll();
                robber4.Task.ClearAll();
                guard1.Task.ClearAll();
                guard2.Task.ClearAll();
                isSceneDone= true;
            }
            else
            {

                if (robber1.IsAlive)
                {
                    if (guard1.IsAlive)
                    {
                        robber1.Task.FightAgainst(guard1);
                    }
                    else
                    {
                        robber1.Task.FightAgainst(guard2);
                    }
                    await BaseScript.Delay(rnd.Next(1000, 5000));
                }

                if (robber2.IsAlive)
                {
                    if (guard2.IsAlive)
                    {
                        robber2.Task.FightAgainst(guard2);
                    }
                    else
                    {
                        robber2.Task.FightAgainst(guard1);
                    }
                    await BaseScript.Delay(rnd.Next(1000, 5000));
                }

                if (guard1.IsAlive)
                {
                    if (robber1.IsAlive)
                    {
                        guard1.Task.FightAgainst(robber1);
                    }
                    else
                    {
                        guard1.Task.FightAgainst(robber2);
                    }
                    await BaseScript.Delay(rnd.Next(1000, 5000));
                }

                if (guard2.IsAlive)
                {
                    if (robber2.IsAlive)
                    {
                        guard2.Task.FightAgainst(robber2);
                    }
                    else
                    {
                        guard2.Task.FightAgainst(robber1);
                    }
                    await BaseScript.Delay(rnd.Next(1000, 5000));
                }
            }

        }

        private void Notify(string message)
        {
            ShowNetworkedNotification(message, "CHAR_CALL911", "CHAR_CALL911", "Dispatch", "AIR-1", 15f);
        }
        private void DrawSubtitle(string message, int duration)
        {
            API.BeginTextCommandPrint("STRING");
            API.AddTextComponentSubstringPlayerName(message);
            API.EndTextCommandPrint(duration, false);
        }
    }
}

