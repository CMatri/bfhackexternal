using System.Collections;
using System.IO;
using System.IO.Packaging;
using SharpDX.IO;
using SharpDX.WIC;

namespace MemHack
{
    using MemLibs;
    using SharpDX;
    using SharpDX.Direct2D1;
    using SharpDX.DirectWrite;
    using SharpDX.DXGI;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public static class Settings
    {
        public static bool aimbot;
        public static bool smoothBot = false;

        public static bool ESPAlways2D = true;
        public static bool ESPBox = true;
        public static bool ESPDistance = true;
        public static bool ESPEntities = true;
        public static bool ESPHead = true;
        public static bool ESPHealth = true;
        public static bool ESPLines = true;
        public static bool ESPSkeleton = true;
        public static bool ESPVehicle = true;
        public static bool UltraHax = true;

        public static bool crossHair = true;
        public static bool guiDefinedCrosshair = true;

        public static float minimapScaleRatio = 0.75f;
        public static int minimapPlayerRad = 3;

        public static bool noRecoil = true;
        public static bool noSway = true;

        public static bool proxWarning = true;

        public static bool playerSearch;
        public static bool playerSearchTeleport;
    }

    public partial class Overlay
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);

        [DllImport("user32.dll")]
        public static extern int GetKeyState(int KeyStates);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int SetWindowsHookEx(int idHook, Overlay.HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]

        public static extern bool UnhookWindowsHookEx(int idHook);

        private bool first;
        public bool clearPlayerSearch;
        public static bool IsRunning;

        private float AimLevel;
        private float lastTap;

        public const int WH_MOUSE = 7;
        private static int hHook = 0;

        public WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private IntPtr HWND_TOPMOST = new IntPtr(-1);
        private Overlay.HookProc MouseHookProcedure;
        private Thread keyBoardThread;
        private Thread mouseThread;

        public Memory Mem = new Memory();

        private List<Player> enemys = new List<Player>();
        private List<Player> players = new List<Player>();
        public List<Player> spectators = new List<Player>();
        public Player LocalPlayer = new Player();
        public Player LockedOnPlayer;
        public Player ToTeleport;

        private SharpDX.Direct2D1.Factory factory;
        private SharpDX.DirectWrite.Factory fontFactory;
        public TextFormat largeText;
        public TextFormat medSmallText;
        public TextFormat medSmallText2;
        public TextFormat medText;
        public TextFormat smallText;
        public SolidColorBrush solidColorBrush;

        private TextBox selectedBox;

        private GuiComponents guiComponents;

        private Update update;
        private Matrix4x4 viewMatrix;

        public SharpDX.Direct2D1.Bitmap crosshairLocked;
        public SharpDX.Direct2D1.Bitmap crosshairUnlocked;
        public SharpDX.Direct2D1.Bitmap playerFriendly;
        public SharpDX.Direct2D1.Bitmap playerEnemy;
        public SharpDX.Direct2D1.Bitmap entity;
        public SharpDX.Direct2D1.Bitmap genericVehicle;
        public SharpDX.Direct2D1.Bitmap genericVehicleFriendly;
        public Dictionary<String, SharpDX.Direct2D1.Bitmap> vehicleTextures;

        public bool isAimbotting;

        public Overlay()
        {
            Console.WriteLine("Battlefield open: " + Mem.AttackProcess("bf4"));
            InitializeComponent();

            guiComponents = new GuiComponents(this);
            mouseThread = new Thread(Mouse.HookMouse);
            keyBoardThread = new Thread(Keyboard.HookKeyboard);
            mouseThread.Start();
            keyBoardThread.Start();
            guiComponents.RegisterTextBox((Width - 0xaf) - 12, 0x2c3, 0x9b, "Search", "Type here");
        }

        public bool UpdateStats()
        {
           // Console.WriteLine(Mem.ReadByte(0x14285B3E8));

            if (!Mem.AttackProcess("bf4"))
            {
                Console.WriteLine("Battlefield closed!");
                Keyboard.UnHookKeyBoard();
                Mouse.UnHookMouse();
                mouseThread.Abort();
                keyBoardThread.Abort();
                mouseThread.Join();
                keyBoardThread.Join();
                return false;
            }

            Mouse.Update();
            players.Clear();
            enemys.Clear();
            spectators.Clear();
            guiComponents.toDrawOnRadar.Clear();

            if (Keyboard.IsKeyDown(Keys.C) && (lastTap == 0f))
            {
                Settings.aimbot = !Settings.aimbot;
                lastTap = 10f;
            }
            else if (lastTap > 0f)
            {
                lastTap--;
            }

            long num = Mem.ReadInt64(sAddresses.ClientGameContext);
            if (!IsValidPtr(num)) return true;
            long num2 = Mem.ReadInt64(num + sOffsets.ClassClientPlayerManager);
            if (!IsValidPtr(num2)) return true;
            long pClient = Mem.ReadInt64(num2 + sOffsets.ClientPlayerManger.LocalPlayer);
            if (!IsValidPtr(pClient)) return true;
            long num4 = Mem.ReadInt64(Mem.ReadInt64(pClient + 0x14b0L)) - 8L;
            if (!IsValidPtr(num4)) return true;
            long num5 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassHealth);
            if (!IsValidPtr(num5)) return true;
            long num6 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassPlayerPosition);
            if (!IsValidPtr(num6)) return true;
            long num7 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassClientRagDollComponent);
            if (!IsValidPtr(num7)) return true;

            viewMatrix = getViewMatrix();
            LocalPlayer.Health = Mem.ReadFloat(num5 + sOffsets.Player.Soldier.PlayerHealth.CurPlayerHealth);
            LocalPlayer.Team = Mem.ReadInt32(pClient + sOffsets.Player.Team);
            LocalPlayer.Name = Mem.ReadString(pClient + sOffsets.Player.Name, 10);
            foreach (char c in LocalPlayer.Name)
            {
                if ((int)c > 127) LocalPlayer.Name.Replace(Char.ToString(c), "");
            }
            LocalPlayer.Pose = Mem.ReadInt32(num4 + sOffsets.Player.Soldier.SoldierPose);
            LocalPlayer.InVehicle = IsValidPtr(Mem.ReadInt64(pClient + 0x14c0L));
            LocalPlayer.SoldierTransform = Mem.ReadMatrix4x4(num7 + sOffsets.Player.Soldier.ClientRagDollComponent.Transform);
            LocalPlayer.Yaw = LocalPlayer.InVehicle ? AxisAlignedBox.MatrixToAngles(LocalPlayer.SoldierTransform).Y : Mem.ReadFloat(num4 + sOffsets.Player.Soldier.Yaw);
            LocalPlayer.Pitch = Mem.ReadFloat(num4 + sOffsets.Player.Soldier.Pitch);
            LocalPlayer.MaxHealth = Mem.ReadFloat(num5 + sOffsets.Player.Soldier.PlayerHealth.MaxPlayerHealth);
            LocalPlayer.pClient = pClient;
            LocalPlayer.pVehicle = GetClientSoldier(pClient, LocalPlayer);
            LocalPlayer.VehicleEntry = Mem.ReadByte(pClient + 0x14c8L);
            long num8 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassClientSoldierWeaponsComponent);
            long pWeaponHandler = Mem.ReadInt64(num8 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClassClientAnimatedSoldierWeaponHandler);
            int num10 = Mem.ReadInt32(num8 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ActiveSlot);
            long pSoldierWeapon = Mem.ReadInt64(pWeaponHandler + (num10 * 8));
            long num12 = Mem.ReadInt64(pSoldierWeapon + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.ClassWeaponFiring);
            long num13 = Mem.ReadInt64(pSoldierWeapon + 0x4850L);
            long num14 = Mem.ReadInt64(num13 + 0x20L);
            long num15 = Mem.ReadInt64(pSoldierWeapon + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.ClassClientSoldierAimingSimulation);
            LocalPlayer.Pitch = Mem.ReadFloat(num4 + sOffsets.Player.Soldier.Pitch);
            LocalPlayer.Ammo = Mem.ReadInt32(num12 + 0x1a0L);
            LocalPlayer.MaxAmmo = Mem.ReadInt32(num12 + 420L);
            LocalPlayer.WeaponName = Mem.ReadString(num14, 7L);
            LocalPlayer.Skeleton.BoneHead = GetBonyById(Mem.ReadInt64(Mem.ReadInt64(pClient + 0x14b0L)) - 8L, 0x68);
            LocalPlayer.Position.X = Mem.ReadFloat(num6 + sOffsets.Player.Soldier.PlayerPosition.XPlayer);
            LocalPlayer.Position.Y = Mem.ReadFloat(num6 + sOffsets.Player.Soldier.PlayerPosition.YPlayer);
            LocalPlayer.Position.Z = Mem.ReadFloat(num6 + sOffsets.Player.Soldier.PlayerPosition.ZPlayer);
            long pLocalRenderer = Mem.ReadInt64(sAddresses.LocalRenderer);
            long pLocalRenderView = Mem.ReadInt64(pLocalRenderer + (long)96);
            LocalPlayer.Fov.X = Mem.ReadFloat(pLocalRenderView + 0x250);
            LocalPlayer.Fov.Y = Mem.ReadFloat(pLocalRenderView + (long)180);

            if (!LocalPlayer.InVehicle)
            {
                LocalPlayer.Velocity.X = Mem.ReadFloat(num6 + 80L);
                LocalPlayer.Velocity.Y = Mem.ReadFloat(num6 + 0x54L);
                LocalPlayer.Velocity.Z = Mem.ReadFloat(num6 + 0x58L);

                Int64 SuperClimbOffset = Mem.ReadInt64(0x14284f860L);
                if (IsValidPtr(SuperClimbOffset))
                {
                    Int64 SuperClimbOffset1 = Mem.ReadInt64(SuperClimbOffset + 0x770);
                    if (IsValidPtr(SuperClimbOffset1))
                    {
                        Int64 SuperClimbOffset2 = Mem.ReadInt64(SuperClimbOffset1 + 0x10);
                        if (IsValidPtr(SuperClimbOffset2))
                        {
                            Mem.WriteFloat(SuperClimbOffset2 + 0x250, Settings.UltraHax ? 200.0f : 1.5f);
                            //Console.WriteLine((SuperClimbOffset2 + 0x250).ToString("X"));
                        }
                    }
                }

                if (num10 < 2)
                {
                    //Console.WriteLine(num12.ToString("X"));
                    long num16 = Mem.ReadInt64(num12 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.WeaponFiring.WeaponSway);
                    long num17 = Mem.ReadInt64(num16 + 8L);
                    AimLevel = Mem.ReadFloat(num15 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.ClientSoldierAimingSimulation.ZoomLevel);

                    Settings.crossHair = !((AimLevel < 1f) && LocalPlayer.WeaponName.Contains("M200"));

                    Mem.WriteFloat(num17 + 0x374L, 1f);
                    Mem.WriteFloat(num17 + 880L, 100f);
                    long num19 = Mem.ReadInt64(num4 + sOffsets.WeaponModifier.BreathControl);
                    byte num20 = 0;
                    Mem.WriteInt32(num19 + 0x58L, num20);

                    if (Settings.UltraHax && LocalPlayer.IsValid && !IsOnDeployScreen() && IsValidPtr(pSoldierWeapon))
                    {
                        Int64 pClientWeaponn = Mem.ReadInt64(pSoldierWeapon + 0x49A8);
                        if (IsValidPtr(pClientWeaponn + 0x0020))
                        {
                            Mem.WriteInt32(pClientWeaponn + 0x0020, 0); //  WeaponModifier* m_pModifier; //0x0020 
                        }
                    }
                }
            }
            else
            {
                Settings.crossHair = false;
                double num21 = Math.Pow(Mem.ReadFloat(LocalPlayer.pVehicle + 640L), 2.0);
                double num22 = Math.Pow(Mem.ReadFloat(LocalPlayer.pVehicle + 0x284L), 2.0);
                double num23 = Math.Pow(Mem.ReadFloat(LocalPlayer.pVehicle + 0x288L), 2.0);
                LocalPlayer.VehicleSpeed = 0f;
                LocalPlayer.VehicleSpeed = ((float)Math.Sqrt((num21 + num22) + num23)) * 3.6f;
                if (LocalPlayer.VehicleName != null)
                {
                    if (((LocalPlayer.VehicleSpeed <= 299f) && (LocalPlayer.VehicleSpeed > 20f)) && LocalPlayer.VehicleName.Contains("jet"))
                    {
                        MemLib.SendInputWithAPI(MemLib.ScanCodeShort.KEY_S, MemLib.KEYEVENTF.KEYUP);
                        MemLib.SendInputWithAPI(MemLib.ScanCodeShort.KEY_W, MemLib.KEYEVENTF.SCANCODE);
                    }
                    else if ((LocalPlayer.VehicleSpeed >= 302f) && LocalPlayer.VehicleName.Contains("jet"))
                    {
                        MemLib.SendInputWithAPI(MemLib.ScanCodeShort.KEY_W, MemLib.KEYEVENTF.KEYUP);
                        MemLib.SendInputWithAPI(MemLib.ScanCodeShort.KEY_S, MemLib.KEYEVENTF.SCANCODE);
                    }
                }
            }
            long pVehicle = LocalPlayer.pVehicle;
            long num25 = Mem.ReadInt64(pVehicle + sOffsets.Player.ClientVehicleEntity.ComponentList);
            byte num26 = Mem.ReadByte(num25 + sOffsets.Player.ClientVehicleEntity.Comp1);
            int num28 = Mem.ReadByte(num25 + sOffsets.Player.ClientVehicleEntity.Comp2) + (num26 * 2);
            num28 = num28 << 5;
            Matrix4x4 matrixx = Mem.ReadMatrix4x4((num28 + num25) + 0x10L);
            LocalPlayer.Vehicle.X = matrixx.M41;
            LocalPlayer.Vehicle.Y = matrixx.M42;
            LocalPlayer.Vehicle.Z = matrixx.M43;
            LocalPlayer.SoldierTransform = matrixx;
            long num29 = Mem.ReadInt64(pVehicle + 0x30L);
            LocalPlayer.VehicleName = Mem.ReadString(Mem.ReadInt64(num29 + 240L), 29L);
            long num30 = Mem.ReadInt64(num12 + sOffsets.WeaponFiring.ClassPrimaryFire);
            long num31 = Mem.ReadInt64(num30 + sOffsets.WeaponFiring.PrimaryFire.ClassShotConfigData);
            if (num10 == 0)
            {
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShot, Settings.UltraHax ? 60 : 2);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShell, Settings.UltraHax ? 60 : 0x4b0);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerBurst, Settings.UltraHax ? 60 : 1);
            }
            else if (num10 == 1)
            {
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShot, Settings.UltraHax ? 60 : 2);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShell, Settings.UltraHax ? 60 : 2);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerBurst, Settings.UltraHax ? 60 : 2);
            }
            else if (num10 == 2)
            {
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShot, 1);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShell, 1);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerBurst, 1);
            }
            else
            {
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShot, 1);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShell, 1);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerBurst, 1);
            }

            if (num10 < 2)
            {
                long address = Mem.ReadInt64(num12 + 0x128L);
                if (IsValidPtr(address))
                {
                    long num33 = Mem.ReadInt64(address + 0x10L);
                    if (IsValidPtr(num33))
                    {
                        if (Settings.UltraHax)
                        {
                            Mem.WriteFloat(num33 + 0x168 + 0x5C, 0.15f); // m_TriggerPullWeight --> 0.15 FAMAS
                            Mem.WriteFloat(num33 + 0x168 + 0x60, 1000.000f); // m_RateOfFire
                            Mem.WriteFloat(num33 + 0x168 + 0x64, 1000.000f); // m_RateOfFireForBurst
                        }

                        long num34 = Mem.ReadInt64(num33 + 0xb0L);
                        if (IsValidPtr(address) && (num15 != 0L))
                        {
                            LocalPlayer.BulletGravity = 0f;
                            LocalPlayer.BulletSpeed = 0f;
                            LocalPlayer.BulletGravity = Mem.ReadFloat(num34 + 0x130L);
                            LocalPlayer.BulletSpeed = Mem.ReadFloat(num33 + 0x88L);
                            LocalPlayer.Sway.X = Mem.ReadFloat(num15 + 40L);
                            LocalPlayer.Sway.Y = Mem.ReadFloat(num15 + 0x2cL);

                            if (Settings.UltraHax)
                            {
                                Mem.WriteFloat(num34 + 0x130, 0.0f);
                                Mem.WriteFloat(num33 + 0x88, 5000.0f);
                            }
                        }
                    }
                }
            }

            long num35 = Mem.ReadInt64(num2 + sOffsets.ClientPlayerManger.PlayerArray);
            for (int i = 0; i < 70; i++)
            {
                long num37 = Mem.ReadInt64(num35 + (i * 8));
                if (pClient != num37)
                {
                    Player player = new Player
                    {
                        pClient = num37,
                        Name = Mem.ReadString(num37 + sOffsets.Player.Name, 20L)
                    };
                    foreach (char c in player.Name)
                    {
                        if ((int)c > 127) player.Name.Replace(Char.ToString(c), "");
                    }
                    long clientSoldier = GetClientSoldier(num37, player);
                    long pSoldier = Mem.ReadInt64(Mem.ReadInt64(num37 + 0x14b0L)) - 8L;
                    long num40 = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassHealth);
                    long num41 = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassPlayerPosition);
                    long num42 = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassClientRagDollComponent);
                    long num43 = Mem.ReadInt64(num42 + 0xb0L);
                    long num45 = Mem.ReadInt64(pClient + 0x14);
                    long num46 = Mem.ReadInt64(pClient + 0x1510);
                    player.pRenderView = num45;
                    player.pOwnerRenderView = num46;

                    player.IsSpectator = Convert.ToBoolean(Mem.ReadByte(num37 + 0x13C9));
                    if (player.IsSpectator)
                        spectators.Add(player);

                    if (((num40 != 0L) && (num41 != 0L)) && (num42 != 0L))
                    {
                        Mem.WriteByte(pSoldier + 0x1aL, new byte[] { 0x8f });
                        if (player.InVehicle)
                        {
                            byte num44 = 1;
                            Mem.WriteByte(clientSoldier + 0x443L, new byte[] { num44 });
                            num29 = Mem.ReadInt64(clientSoldier + 0x30L);
                            player.VehicleName = Mem.ReadString(Mem.ReadInt64(num29 + 240L), 13L);
                        }
                        player.Yaw = Mem.ReadFloat(pSoldier + sOffsets.Player.Soldier.Yaw);
                        player.Pitch = Mem.ReadFloat(pSoldier + sOffsets.Player.Soldier.Pitch);
                        player.Health = Mem.ReadFloat(num40 + sOffsets.Player.Soldier.PlayerHealth.CurPlayerHealth);
                        player.MaxHealth = Mem.ReadFloat(num40 + sOffsets.Player.Soldier.PlayerHealth.MaxPlayerHealth);
                        player.Team = Mem.ReadInt32(num37 + sOffsets.Player.Team);
                        player.Pose = Mem.ReadInt32(pSoldier + sOffsets.Player.Soldier.SoldierPose);
                        player.Position.X = Mem.ReadFloat(num41 + sOffsets.Player.Soldier.PlayerPosition.XPlayer);
                        player.Position.Y = Mem.ReadFloat(num41 + sOffsets.Player.Soldier.PlayerPosition.YPlayer);
                        player.Position.Z = Mem.ReadFloat(num41 + sOffsets.Player.Soldier.PlayerPosition.ZPlayer);
                        player.Skeleton.BoneHead = GetBonyById(pSoldier, 0x68);
                        player.Skeleton.BoneNeck = GetBonyById(pSoldier, 0x8e);
                        player.Skeleton.BoneLeftShoulder = GetBonyById(pSoldier, 9);
                        player.Skeleton.BoneRightShoulder = GetBonyById(pSoldier, 0x6d);
                        player.Skeleton.BoneLeftElbowRoll = GetBonyById(pSoldier, 11);
                        player.Skeleton.BoneRightElbowRoll = GetBonyById(pSoldier, 0x6f);
                        player.Skeleton.BoneLeftHand = GetBonyById(pSoldier, 15);
                        player.Skeleton.BoneRightHand = GetBonyById(pSoldier, 0x73);
                        player.Skeleton.BoneSpine = GetBonyById(pSoldier, 5);
                        player.Skeleton.BoneLeftKnee = GetBonyById(pSoldier, 0xbc);
                        player.Skeleton.BoneRightKnee = GetBonyById(pSoldier, 0xc5);
                        player.Skeleton.BoneLeftFoot = GetBonyById(pSoldier, 0xb8);
                        player.Skeleton.BoneRightFoot = GetBonyById(pSoldier, 0xc6);
                        player.VehicleEntry = Mem.ReadByte(num37 + 0x14c8L);
                        player.SoldierTransform = Mem.ReadMatrix4x4(num42 + sOffsets.Player.Soldier.ClientRagDollComponent.Transform);
                        player.VehicleHealth = Mem.ReadFloat(pSoldier + sOffsets.Player.Soldier.PlayerHealth.CurVehicleHealth);
                        player.InVehicle = IsValidPtr(Mem.ReadInt64(num37 + 0x14c0L));
                        pVehicle = Mem.ReadInt64(num37 + sOffsets.Player.ClassSoldier);
                        num25 = Mem.ReadInt64(pVehicle + sOffsets.Player.ClientVehicleEntity.ComponentList);
                        num26 = Mem.ReadByte(num25 + sOffsets.Player.ClientVehicleEntity.Comp1);
                        num28 = Mem.ReadByte(num25 + sOffsets.Player.ClientVehicleEntity.Comp2) + (num26 * 2);
                        num28 = num28 << 5;
                        matrixx = Mem.ReadMatrix4x4((num28 + num25) + 0x10L);
                        player.SoldierTransform = matrixx;
                        player.Vehicle.X = matrixx.M41;
                        player.Vehicle.Y = matrixx.M42;
                        player.Vehicle.Z = matrixx.M43;
                        player.Velocity.X = Mem.ReadFloat(num41 + 80L);
                        player.Velocity.Y = Mem.ReadFloat(num41 + 0x54L);
                        player.Velocity.Z = Mem.ReadFloat(num41 + 0x58L);
                        if (player.InVehicle)
                        {
                            player.Velocity.X = Mem.ReadFloat(clientSoldier + 640L);
                            player.Velocity.Y = Mem.ReadFloat(clientSoldier + 0x284L);
                            player.Velocity.Z = Mem.ReadFloat(clientSoldier + 0x288L);
                        }
                        byte num47 = Mem.ReadByte(pSoldier + sOffsets.Player.Soldier.IsOccluded);
                        player.IsOccluded = num47 != 0;
                        player.Distance_3D = GetLength(LocalPlayer.Position, player.Position);
                        player.Distance_Crosshair = Distance_Crosshair(player);
                        players.Add(player);
                        if (player.Team != LocalPlayer.Team)
                        {
                            enemys.Add(player);
                        }
                    }
                }
            }

            if (IsValidPtr(0x1421c1950) && IsValidPtr(Mem.ReadInt64(0x1421c1950)) && IsValidPtr(Mem.ReadInt64(0x1421c1950) + 0x49c0) && IsValidPtr(Mem.ReadInt64(Mem.ReadInt64(0x1421c1950) + 0x49c0) + 0x78) && IsValidPtr(Mem.ReadInt64(Mem.ReadInt64(Mem.ReadInt64(0x1421c1950) + 0x49c0) + 0x78) + 0x8))
            {
                Int64 noSpread = Mem.ReadInt64(Mem.ReadInt64(Mem.ReadInt64(Mem.ReadInt64(0x1421c1950) + 0x49c0) + 0x78) + 0x8);
                if (IsValidPtr(noSpread) && Settings.UltraHax)
                {
                    Mem.WriteFloat(noSpread + 0x360, 0.0f);
                    Mem.WriteFloat(noSpread + 0x364, 0.0f);
                    Mem.WriteFloat(noSpread + 0x368, 0.0f);
                    Mem.WriteFloat(noSpread + 0x36C, 0.0f);
                }
            }

            long num48 = Mem.ReadInt64(sAddresses.ViewAngles);
            long num49 = Mem.ReadInt64(num48 + 0x4988L);
            long num50 = Mem.ReadInt64(num49 + 0x10L);
            float a = Mem.ReadFloat(num50 + 20L);
            float num52 = Mem.ReadFloat(num50 + 0x18L);
            if (Keyboard.IsKeyDown(Keys.LShiftKey))
            {
                Player player2 = DistanceCrosshairSortPlayers(enemys);
                Vector2 onScreen = new Vector2();
                ViewAngle aimHead = GetAimHead(player2, true, ref onScreen);

                LockedOnPlayer = player2;
                if (player2 == null) return true;
                aimTo = new Vector3(onScreen, player2.IsOccluded ? 0 : 1);

                //Console.WriteLine(Distance_Crosshair(new Vector2D(onScreen.X, onScreen.Y)));
                if (Settings.aimbot && !LocalPlayer.InVehicle && (AimLevel < 1f) && (player2 != null) && (((!player2.IsOccluded || (player2.InVehicle)) && (aimHead != null)) && (player2.Distance_Crosshair < (player2.InVehicle ? (160) : (Settings.UltraHax ? 280 : 90)))))
                {
                    isAimbotting = true;
                    if (Math.Abs(Distance_Crosshair(new Vector2D(onScreen.X, onScreen.Y))) < 15 || player2.InVehicle && Settings.smoothBot)
                    {
                        Vector2 vector = new Vector2(LerpRadians(a, aimHead.Yaw, 0.855f), LerpRadians(num52, aimHead.Pitch, 0.855f));
                        Mem.WriteAngle(vector.X, vector.Y);
                    }
                    else
                    {
                        Vector2 vector = new Vector2(LerpRadians(a, aimHead.Yaw, 0.25f), LerpRadians(num52, aimHead.Pitch, 0.25f));
                        Mem.WriteAngle(vector.X, vector.Y);
                    }

                    if (Settings.UltraHax && !Settings.smoothBot)
                        Mem.WriteAngle(aimHead.Yaw, aimHead.Pitch);

                }
                else
                    isAimbotting = false;
            }
            else
            {
                aimTo = new Vector3(-200, -200, 0);
                isAimbotting = false;
            }

            if (!LocalPlayer.InVehicle/* && Settings.playerSearchTeleport && ToTeleport != null && ToTeleport.IsValid*/ && Keyboard.IsKeyDown(Keys.Q))
            {
                TeleportToPlayer(new Vector3D(LocalPlayer.Position.X, LocalPlayer.Position.Y + 50, LocalPlayer.Position.Z));
            }
            //TeleportToPlayer(new Vector3D(LocalPlayer.Position.X, 175, LocalPlayer.Position.Z));

            return true;
        }

        private Vector3 aimTo;

        public void Render(bool focus)
        {
            long num2;
            Vector3D vectord;
            Vector3D vectord2;
            AxisAlignedBox box;
            SharpDX.Color color2;
            device.BeginDraw();
            device.Clear(new Color4(0f, 0f, 0f, 0f));

            if (!focus)
            {
                device.EndDraw();
                return;
            }
            //guiComponents.DrawBitmap(test, MakeRectangle(200, 200, 26, 32), (float)(LocalPlayer.Yaw));

            if (Settings.ESPHead)
                guiComponents.DrawCrosshair(aimTo.X, aimTo.Y, 15, 15, aimTo.Z == 1 ? SharpDX.Color.LightGreen : SharpDX.Color.OrangeRed, false);

            if (LocalPlayer.InVehicle)
            {
                guiComponents.infoExpanded = false;
                if (Settings.ESPVehicle)
                {
                    num2 = Mem.ReadInt64(LocalPlayer.pClient + sOffsets.Player.ClassSoldier) + 0x250L;
                    vectord = new Vector3D(Mem.ReadFloat(num2), Mem.ReadFloat(num2 + 4L), Mem.ReadFloat(num2 + 8L));
                    vectord2 = new Vector3D(Mem.ReadFloat(num2 + 0x10L), Mem.ReadFloat(num2 + 20L), Mem.ReadFloat(num2 + 0x18L));
                    box = new AxisAlignedBox();
                    box.Init(vectord, vectord2);
                    box.Setup(LocalPlayer.Vehicle, LocalPlayer.SoldierTransform, true, 0f, 0f, 0f);
                    color2 = new SharpDX.Color();
                    guiComponents.DrawAxisAlignedBoundingBox(LocalPlayer, box, false, color2, false);

                    if (LocalPlayer.VehicleName.Contains("heli"))
                    {
                        color2 = new SharpDX.Color();
                        guiComponents.DrawAxisAlignedBoundingBox(LocalPlayer, null, false, color2, false);
                    }
                }
            }
            if (!IsOnDeployScreen() && LocalPlayer.IsValid && LocalPlayer.Position.X != 0.0f && LocalPlayer.Position.Y != 0.0f && LocalPlayer.Position.Z != 0.0f && LocalPlayer.Health > 0 && IsValidPtr(LocalPlayer.pClient))
            {
                SharpDX.Rectangle rectangle = new SharpDX.Rectangle();
                if (Settings.ESPEntities)
                {
                    guiComponents.DrawEntities();
                }
                List<long> list = new List<long>();
                bool flag = false;
                float num3 = 0f;

                foreach (Player player in spectators)
                {
                    if (Mem.ReadInt64(player.pOwnerRenderView + 0x00F8) == LocalPlayer.pClient)
                    {
                        solidColorBrush.Color = new SharpDX.Color(40, 40, 40, 150);
                        device.FillRectangle(MakeRectangle((WindowWidth / 2) - 121.5f, 76.5f, 254f, 29f), solidColorBrush);
                        solidColorBrush.Color = SharpDX.Color.Gray;
                        device.DrawRectangle(MakeRectangle((WindowWidth / 2) - 121.5f, 76.5f, 255f, 30f), solidColorBrush);
                        guiComponents.DrawText("SPECTATOR WARNING", (WindowWidth / 2) - 15, 72, 0x1a2, 30, true, largeText, SharpDX.Color.Red);
                    }

                }

                foreach (Player player in players)
                {

                    if (player.IsValid && IsValidPtr(player.pClient))
                    {
                        long num;
                        Color4 color;
                        GuiComponents.RadarData data;
                        Vector3D vectord3 = WorldToScreen_Head(player.Position, player.Pose);
                        Vector3D vectord4 = WorldToScreen(player.Position);
                        float num4 = vectord4.Y - vectord3.Y;
                        float num5 = num4 / 2f;
                        float num6 = vectord3.X - (num5 / 2f);
                        if (player.Team == LocalPlayer.Team)
                        {
                            solidColorBrush.Color = SharpDX.Color.LightGreen;
                        }
                        else if (!player.IsOccluded)
                        {
                            solidColorBrush.Color = SharpDX.Color.Yellow;
                        }
                        else
                        {
                            solidColorBrush.Color = SharpDX.Color.OrangeRed;
                        }
                        rectangle.X = (int)num6;
                        rectangle.Y = (int)vectord3.Y;
                        rectangle.Width = (int)num5;
                        rectangle.Height = (int)num4;

                        if (Settings.playerSearch && (player.Name.Contains(guiComponents.GetTextBox("Search").Text) && (guiComponents.GetTextBox("Search").Text.Length != 0)))
                        {
                            color = solidColorBrush.Color;
                            solidColorBrush.Color = SharpDX.Color.Pink;
                            device.DrawLine(new Vector2((Width / 2), 0f), new Vector2(vectord3.X, vectord3.Y), solidColorBrush);
                            guiComponents.DrawText("Player Found!", Width / 2 - 170 / 2, Height - 20, 300, 20, false, medSmallText2, SharpDX.Color.LightGray);
                            ToTeleport = player;
                            solidColorBrush.Color = color;
                        }

                        if (!player.InVehicle)
                        {
                            if ((((player.pClient != LocalPlayer.pClient)) && (Settings.ESPSkeleton && player.IsValid)) && LocalPlayer.IsValid)
                            {
                                color = solidColorBrush.Color;
                                guiComponents.DrawSkeleton(player, player.Team == LocalPlayer.Team ? SharpDX.Color.LightBlue : SharpDX.Color.Orange);
                                solidColorBrush.Color = color;
                            }
                            if (Settings.ESPHealth)
                            {
                                guiComponents.DrawHealthBar((int)num6, (int)vectord3.Y - 5, (int)num5, 5, (int)player.Health, (int)player.MaxHealth, false);
                            }
                            if (Settings.ESPBox)
                            {
                                if (!((player.Distance_3D >= 100f) || Settings.ESPAlways2D))
                                {
                                    guiComponents.DrawAxisAlignedBoundingBox(player, null, false);
                                }
                                else
                                {
                                    guiComponents.DrawEsp(rectangle);
                                }
                            }
                            num = Mem.ReadInt64(player.pClient + sOffsets.Player.ClassSoldier);
                            if (!list.Contains(num))
                            {
                                data.Pos = new Vector2D(LocalPlayer.Position.X - player.Position.X, LocalPlayer.Position.Z - player.Position.Z);
                                data.Rotation = player.Yaw;
                                data.Team = player.Team;
                                data.Type = 0;
                                data.Owner = player;
                                guiComponents.toDrawOnRadar.Add(data);
                            }
                        }
                        else if (Settings.ESPVehicle)
                        {
                            num = Mem.ReadInt64(player.pClient + sOffsets.Player.ClassSoldier);
                            num2 = num + 0x250L;
                            vectord = new Vector3D(Mem.ReadFloat(num2), Mem.ReadFloat(num2 + 4L), Mem.ReadFloat(num2 + 8L));
                            vectord2 = new Vector3D(Mem.ReadFloat(num2 + 0x10L), Mem.ReadFloat(num2 + 20L), Mem.ReadFloat(num2 + 0x18L));
                            if ((!list.Contains(num) && (player.VehicleEntry == 0f)) && IsValidPtr(num2))
                            {
                                data.Pos = new Vector2D(LocalPlayer.Position.X - player.Position.X, LocalPlayer.Position.Z - player.Position.Z);
                                data.Rotation = player.Yaw;
                                data.Team = player.Team;
                                data.Type = 1;
                                data.Owner = player;
                                guiComponents.toDrawOnRadar.Add(data);
                                list.Add(num);
                                box = new AxisAlignedBox();
                                box.Init(vectord, vectord2);
                                box.Setup(player.Vehicle, player.SoldierTransform, true, 0f, 0f, 0f);
                                guiComponents.DrawAxisAlignedBoundingBox(player, box, true);
                            }

                            if (Settings.ESPSkeleton)
                            {
                                guiComponents.DrawSkeleton(player, player.Team == LocalPlayer.Team ? SharpDX.Color.LightBlue : SharpDX.Color.Orange);
                            }

                            if (player.Team != LocalPlayer.Team)
                            {
                                if (Settings.ESPBox)
                                {
                                    guiComponents.DrawEsp(rectangle);
                                }
                                if (Settings.ESPHealth)
                                {
                                    guiComponents.DrawHealthBar((int)num6, (int)vectord3.Y - 5, (int)num5, 5, (int)player.Health, (int)player.MaxHealth, false);
                                }
                            }
                        }

                        if (player.Team != LocalPlayer.Team)
                        {
                            if ((!player.IsOccluded && Settings.ESPHead) && (player.Distance_Crosshair < 110f))
                            {
                                //guiComponents.DrawCrosshair(vectord5.X, vectord5.Y, 10, 10, new SharpDX.Color(solidColorBrush.Color), false);
                            }
                            if (player.Distance_3D <= 150f)
                            {
                                if ((((vectord4.X + vectord4.Y) <= 5f) && (player.Distance_3D <= 50f)) && Settings.proxWarning)
                                {
                                    num3++;
                                    flag = true;
                                }
                                else if (((vectord4.X + vectord4.Y) > 5f) && Settings.ESPLines)
                                {
                                    setBrushAlpha(0.5f);
                                    if (!player.IsOccluded)
                                    {
                                        device.DrawLine(new Vector2(vectord4.X, vectord4.Y), new Vector2((Width / 2), Height), solidColorBrush, 1f);
                                    }
                                    else if (player.IsOccluded && (player.Distance_3D <= 50f))
                                    {
                                        device.DrawLine(new Vector2(vectord4.X, vectord4.Y), new Vector2((Width / 2), Height), solidColorBrush, 1f);
                                    }
                                }
                            }
                        }
                        if (Settings.ESPDistance)
                        {
                            guiComponents.DrawText(((int)player.Distance_3D) + "m", (int)vectord4.X, ((int)vectord3.Y) + ((int)num4), 200, 20, true, smallText, SharpDX.Color.Wheat);
                        }
                    }
                }
                if (flag && Settings.proxWarning)
                {
                    solidColorBrush.Color = new SharpDX.Color(40, 40, 40, 150);
                    device.FillRectangle(MakeRectangle((WindowWidth / 2) - 130.5f, WindowHeight / 8 + 1, 274f, 29f), solidColorBrush);
                    solidColorBrush.Color = SharpDX.Color.Gray;
                    device.DrawRectangle(MakeRectangle((WindowWidth / 2) - 131.5f, WindowHeight / 8 + 1, 275f, 30f), solidColorBrush);
                    guiComponents.DrawText("PROXIMITY WARNING", (WindowWidth / 2) - 25, WindowHeight / 8 - 4, 0x198, 30, true, largeText, SharpDX.Color.Red);
                    guiComponents.DrawText(((num3 > 0f) ? ("x" + num3) : ("x1")), (WindowWidth / 2) + 130, WindowHeight / 8 - 4, 0x198, 30, true, largeText, SharpDX.Color.Red);
                }
                if (Settings.crossHair && Settings.guiDefinedCrosshair && (!IsOnDeployScreen() || LocalPlayer.Health > 0))
                {
                    guiComponents.DrawCrosshair((Width / 2), (Height / 2), 20, 20, SharpDX.Color.White, true);
                }
                if (guiComponents.infoExpandedBeforeDeath)
                {
                    guiComponents.infoExpanded = true;
                    guiComponents.infoExpandedBeforeDeath = false;
                }
                if (guiComponents.radarExpandedBeforeDeath)
                {
                    guiComponents.radarExpanded = true;
                    guiComponents.radarExpandedBeforeDeath = false;
                }
            }
            else
            {
                if (guiComponents.infoExpanded)
                {
                    guiComponents.infoExpandedBeforeDeath = true;
                }
                if (guiComponents.radarExpanded)
                {
                    guiComponents.radarExpanded = true;
                }
                guiComponents.infoExpanded = false;
                guiComponents.radarExpanded = false;
            }

            guiComponents.DrawInfo();
            guiComponents.DrawRadar();
            guiComponents.DrawHackMenu();
            guiComponents.DrawPlayerSearch();
            guiComponents.DrawTextBoxes();
            if (LockedOnPlayer != null && Keyboard.IsKeyDown(Keys.LShiftKey)) guiComponents.DrawLockedOnPlayerInfo(LockedOnPlayer);
            guiComponents.DrawText("External Multihack by Slyth", Width - 167, Height - 20, 300, 20, false, medSmallText2, SharpDX.Color.LightGray);

            if (LocalPlayer.Health > 0f)
            {
                int num7 = 0x3e8;
                for (int i = 0; i < players.Count; i++)
                {
                    if ((players[i].Distance_3D < num7) && (players[i].Team != LocalPlayer.Team))
                    {
                        num7 = (int)players[i].Distance_3D;
                    }
                }
                solidColorBrush.Color = new SharpDX.Color(40, 40, 40, 150);
                device.FillRectangle(MakeRectangle((WindowWidth / 2) - 102.5f, 16.5f, 214f, 29f), solidColorBrush);
                solidColorBrush.Color = SharpDX.Color.Gray;
                device.DrawRectangle(MakeRectangle((WindowWidth / 2) - 102.5f, 16.5f, 215f, 30f), solidColorBrush);
                guiComponents.DrawText("Closest enemy is " + num7 + " meters away", (WindowWidth / 2) + 0x5f, 18, 0x1a2, 30, true, medSmallText2, SharpDX.Color.Red);
            }

            device.EndDraw();
        }

        private void TeleportToPlayer(Player p) { TeleportToPlayer(p.Position); }

        private void TeleportToPlayer(Vector3D p)
        {
            Int64 pVault1 = Mem.ReadInt64(LocalPlayer.pVehicle + 0x0CD0);
            if (IsValidPtr(pVault1))
            {
                Int64 pVault2 = Mem.ReadInt64(pVault1 + 0x0110);
                if (IsValidPtr(pVault2))
                {
                    //Console.WriteLine(Mem.ReadFloat(pVault2 + 0x0030) + " : " +
                    //                  Mem.ReadFloat(pVault2 + 0x0034) + " : " +
                    //                  Mem.ReadFloat(pVault2 + 0x0038));

                    Mem.WriteFloat(pVault2 + 0x0030, p.X);
                    Mem.WriteFloat(pVault2 + 0x0034, p.Y);
                    Mem.WriteFloat(pVault2 + 0x0038, p.Z);
                    Console.WriteLine((pVault2 + 0x0030).ToString("X"));
                }
            }
        }

        private Vector3 AimCorrection(Vector3 enemyPos, Vector3 myPos, Vector3 Velocity, Vector3 EnemyVelocity, Vector3 InVec, float Distance, float Speed, float Gravity)
        {
            InVec += (EnemyVelocity * (Distance / Math.Abs(Speed)));
            InVec -= (Velocity * (Distance / Math.Abs(Speed)));
            float num = Math.Abs(Gravity);
            float num2 = Distance / Math.Abs(Speed);
            InVec.Y += ((0.5f * num) * num2) * num2;

            return InVec;

            /*Vector3 predictedAimingPosition = enemyPos;
            Matrix4x4 viewMatrixInverse = GetViewMatrixInverse();

            // trans target position relate to local player's view position for simplifying equations
            Vector3 p1 = enemyPos;
            p1.X -= viewMatrixInverse.M41;
            p1.Y -= viewMatrixInverse.M42;
            p1.Z -= viewMatrixInverse.M43;

            double a = Gravity * Gravity * 0.25;
            double b = -Gravity * EnemyVelocity.Y;
            double c = EnemyVelocity.X * EnemyVelocity.X + EnemyVelocity.Y * EnemyVelocity.Y + EnemyVelocity.Z * EnemyVelocity.Z - Gravity * p1.Y - Speed * Speed;
            double d = 2.0 * (p1.X * EnemyVelocity.X + p1.Y * EnemyVelocity.Y + p1.Z * EnemyVelocity.Z);
            double e = p1.X * p1.X + p1.Y * p1.Y + p1.Z * p1.Z;

            // some unix guys will not afraid these two lines
            double[] roots = new double[4];
            uint num_roots = SolveQuartic(a, b, c, d, e, ref roots);

            if (num_roots > 0)
            {
                // find the best predict hit time
                // smallest 't' for guns, largest 't' for something like mortar with beautiful arcs
                double hitTime = 0.0;
                for (int i = 0; i < num_roots; ++i)
                {
                    if (roots[i] > 0.0 && (hitTime == 0.0 || roots[i] < hitTime))
                        hitTime = roots[i];
                }

                if (hitTime > 0.0)
                {
                    // get predict bullet velocity vector at aiming direction
                    double hitVelX = p1.X / hitTime + EnemyVelocity.X;
                    double hitVelY = p1.Y / hitTime + EnemyVelocity.Y - 0.5 * Gravity * hitTime;
                    double hitVelZ = p1.Z / hitTime + EnemyVelocity.Z;

                    // finally, the predict aiming position in world space
                    predictedAimingPosition.X = (float)(viewMatrixInverse.M41 + hitVelX * hitTime);
                    predictedAimingPosition.Y = (float)(viewMatrixInverse.M42 + hitVelY * hitTime);
                    predictedAimingPosition.Z = (float)(viewMatrixInverse.M43 + hitVelZ * hitTime);
                }
            }

            return predictedAimingPosition;*/
        }

        public bool CheckVectorIsZero(params Vector3D[] v)
        {
            foreach (Vector3D vectord in v)
            {
                if ((vectord.X == 0f) || (vectord.Y == 0f))
                {
                    return false;
                }
            }
            return true;
        }

        private float Distance_Crosshair(Player player)
        {
            Vector3D vectord = new Vector3D();
            vectord = WorldToScreen(player.Skeleton.BoneHead);
            float num = (vectord.X > CrosshairX) ? (vectord.X - CrosshairX) : (CrosshairX - vectord.X);
            float num2 = (vectord.Y > CrosshairX) ? (vectord.Y - CrosshairX) : (CrosshairY - vectord.Y);
            return (float)Math.Sqrt((double)((num * num) + (num2 * num2)));
            Vector3 Origin = new Vector3();
            Vector3 ShootSpace = new Vector3();

            Origin = player.Skeleton.BoneHead.ToVector3();

            Matrix4x4 mTmp = GetViewMatrixInverse();

            ShootSpace.X = Origin.X - mTmp.M41;
            ShootSpace.Y = Origin.Y - mTmp.M42;
            ShootSpace.Z = Origin.Z - mTmp.M43;

            ShootSpace = VectorNormalize(ShootSpace);

            Vector3 vLeft = new Vector3(mTmp.M11, mTmp.M12, mTmp.M13);
            float Yaw = (float)-Math.Asin(Vector3.Dot(vLeft, ShootSpace));
            float YawDifference = LocalPlayer.Fov.Y / 4.0f - Yaw;

            float RealDistance = (float)Math.Abs(Math.Sin(YawDifference) * player.Distance_3D);

            // Console.WriteLine("Player: {0} Real distance: {1} Enemy distance: {2}", player.Name, RealDistance, player.Distance);
            // Thread.Sleep(1000);

            return RealDistance;
        }

        private float Distance_Crosshair(Vector2D vec)
        {
            float num = (vec.X > CrosshairX) ? (vec.X - CrosshairX) : (CrosshairX - vec.X);
            float num2 = (vec.Y > CrosshairX) ? (vec.Y - CrosshairX) : (CrosshairY - vec.Y);
            return (float)Math.Sqrt((double)((num * num) + (num2 * num2)));
        }

        private Player DistanceCrosshairSortPlayers(List<Player> _Players)
        {
            List<Player> list = (from a in _Players
                                 orderby a.Distance_Crosshair
                                 select a).ToList<Player>();
            if (list.Count == 0)
            {
                return null;
            }
            return list[0];
        }


        private ViewAngle GetAimHead(Player _EnemyPlayer, bool onHead, ref Vector2 onScreen)
        {
            if (_EnemyPlayer == null) return new ViewAngle();
            if (_EnemyPlayer.Skeleton.BoneHead.X != 0 &&
                _EnemyPlayer.Skeleton.BoneHead.Y != 0 &&
                _EnemyPlayer.Skeleton.BoneHead.Z != 0)
            {
                Vector3 Space = new Vector3();
                Vector3 boneAim = onHead ? new Vector3(_EnemyPlayer.Skeleton.BoneHead.X, _EnemyPlayer.Skeleton.BoneHead.Y, _EnemyPlayer.Skeleton.BoneHead.Z) : new Vector3(_EnemyPlayer.Skeleton.BoneSpine.X, _EnemyPlayer.Skeleton.BoneSpine.Y, _EnemyPlayer.Skeleton.BoneSpine.Z);
                Vector3 Origin = AimCorrection(
                    new Vector3(_EnemyPlayer.Position.X, _EnemyPlayer.Position.Y, _EnemyPlayer.Position.Z),
                    new Vector3(LocalPlayer.Position.X, LocalPlayer.Position.Y, LocalPlayer.Position.Z),
                    new Vector3(LocalPlayer.Velocity.X, LocalPlayer.Velocity.Y, LocalPlayer.Velocity.Z),
                    new Vector3(_EnemyPlayer.Velocity.X, _EnemyPlayer.Velocity.Y, _EnemyPlayer.Velocity.Z),
                   boneAim,
                    _EnemyPlayer.Distance_3D,
                    LocalPlayer.BulletSpeed,
                    LocalPlayer.BulletGravity
                    );

                Vector3D posOnScreen = WorldToScreen(new Vector3D(Origin.X, Origin.Y, Origin.Z));
                onScreen = new Vector2(posOnScreen.X, posOnScreen.Y);

                Matrix4x4 mTmp = GetViewMatrixInverse();
                Space.X = Origin.X - mTmp.M41;
                Space.Y = Origin.Y - mTmp.M42;
                Space.Z = Origin.Z - mTmp.M43;
                Space = VectorNormalize(Space);

                ViewAngle Angles = new ViewAngle();
                Angles.Yaw = (float)-Math.Atan2(Space.X, Space.Z);
                Angles.Pitch = (float)Math.Atan2(Space.Y, Math.Sqrt((Space.X * Space.X) + (Space.Z * Space.Z)));
                Angles.Yaw -= LocalPlayer.Sway.X;
                Angles.Pitch -= LocalPlayer.Sway.Y;
                return Angles;
            }
            return null;
        }

        private Vector3D GetBonyById(long pSoldier, int Id)
        {
            Vector3D vectord = new Vector3D();
            long num = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassClientRagDollComponent);
            long num2 = Mem.ReadInt64(num + 0xb0L);
            vectord.X = Mem.ReadFloat(num2 + (Id * 0x20));
            vectord.Y = Mem.ReadFloat((num2 + (Id * 0x20)) + 4L);
            vectord.Z = Mem.ReadFloat((num2 + (Id * 0x20)) + 8L);
            return vectord;
        }

        private long GetClientSoldier(long pClient, Player player)
        {
            return Mem.ReadInt64(pClient + sOffsets.Player.ClassSoldier);
        }

        private float GetLength(Vector3D from, Vector3D to)
        {
            Vector3D vectord = new Vector3D
            {
                X = to.X - from.X,
                Y = to.Y - from.Y,
                Z = to.Z - from.Z
            };
            return (float)Math.Sqrt((((vectord.X * vectord.X) + (vectord.Y * vectord.Y)) + (vectord.Z * vectord.Z)));
        }

        private bool GetSpectators(out Player spectator)
        {
            spectator = null;
            foreach (Player player in players)
            {
                long num = Mem.ReadInt64(player.pOwnerRenderView + 0xf8L);
                if ((player.pClient != LocalPlayer.pClient) && ((player.pRenderView == LocalPlayer.pRenderView) && (num == LocalPlayer.pClient)))
                {
                    spectator = player;
                    return true;
                }
            }
            return false;
        }

        private Matrix4x4 getViewMatrix()
        {
            long num = Mem.ReadInt64(sAddresses.GameRenderer);
            long num2 = Mem.ReadInt64(num + sOffsets.GameRenderer.ClassRenderView);
            return Mem.ReadMatrix4x4(num2 + sOffsets.GameRenderer.RenderView.ViewProjectionMatrix);
        }

        private Matrix4x4 GetViewMatrixInverse()
        {
            long num = Mem.ReadInt64(sAddresses.GameRenderer);
            long num2 = Mem.ReadInt64(num + sOffsets.GameRenderer.ClassRenderView);
            return Mem.ReadMatrix4x4(num2 + sOffsets.GameRenderer.RenderView.ViewMatrixInverse);
        }

        public bool IsKeyDown(int key)
        {
            return Convert.ToBoolean((int)(GetKeyState(key) & 0x8000));
        }

        public bool IsOnDeployScreen()
        {
            return (Mem.ReadByte(0x1421c1468L) == 1);
        }

        public bool IsValidPtr(long Address)
        {
            return ((Address >= 0x10000L) && (Address < 0xf000000000000L));
        }

        private float Lerp(float a, float b, float s)
        {
            return (((1f - s) * a) + (s * b));
        }

        private float LerpRadians(float a, float b, float s)
        {
            if (Math.Abs((b - a)) > (float)Math.PI)
            {
                if (b > a)
                {
                    a += (float)Math.PI * 2f;
                }
                else
                {
                    b += (float)Math.PI * 2f;
                }
            }
            float num2 = a + ((b - a) * s);
            float num3 = (float)Math.PI * 2f;
            if ((num2 >= 0f) && (num2 <= ((float)Math.PI * 2f)))
            {
                return num2;
            }
            return (num2 % num3);
        }

        private static uint SolveCubic(double[] coeff, ref double[] x)
        {
            /* Adjust coefficients */

            double a1 = coeff[2] / coeff[3];
            double a2 = coeff[1] / coeff[3];
            double a3 = coeff[0] / coeff[3];

            double Q = (a1 * a1 - 3 * a2) / 9;
            double R = (2 * a1 * a1 * a1 - 9 * a1 * a2 + 27 * a3) / 54;
            double Qcubed = Q * Q * Q;
            double d = Qcubed - R * R;

            /* Three real roots */

            if (d >= 0)
            {
                double theta = Math.Acos(R / Math.Sqrt(Qcubed));
                double sqrtQ = Math.Sqrt(Q);

                x[0] = -2 * sqrtQ * Math.Cos(theta / 3) - a1 / 3;
                x[1] = -2 * sqrtQ * Math.Cos((theta + 2 * Math.PI) / 3) - a1 / 3;
                x[2] = -2 * sqrtQ * Math.Cos((theta + 4 * Math.PI) / 3) - a1 / 3;

                return (3);
            }

            /* One real root */

            else
            {
                double e = Math.Pow(Math.Sqrt(-d) + Math.Abs(R), 1.0 / 3.0);

                if (R > 0)
                {
                    e = -e;
                }

                x[0] = (e + Q / e) - a1 / 3.0;

                return (1);
            }
        }

        public static uint SolveQuartic(double a, double b, double c, double d, double e, ref double[] x)
        {
            /* Adjust coefficients */

            double a1 = d / e;
            double a2 = c / e;
            double a3 = b / e;
            double a4 = a / e;

            /* Reduce to solving cubic equation */

            double q = a2 - a1 * a1 * 3 / 8;
            double r = a3 - a1 * a2 / 2 + a1 * a1 * a1 / 8;
            double s = a4 - a1 * a3 / 4 + a1 * a1 * a2 / 16 - 3 * a1 * a1 * a1 * a1 / 256;

            double[] coeff_cubic = new double[4];
            double[] roots_cubic = new double[3];
            double positive_root = 0;

            coeff_cubic[3] = 1;
            coeff_cubic[2] = q / 2;
            coeff_cubic[1] = (q * q - 4 * s) / 16;
            coeff_cubic[0] = -r * r / 64;

            uint nRoots = SolveCubic(coeff_cubic, ref roots_cubic);

            for (int i = 0; i < nRoots; i++)
            {
                if (roots_cubic[i] > 0)
                {
                    positive_root = roots_cubic[i];
                }
            }

            /* Reduce to solving two quadratic equations */

            double k = Math.Sqrt(positive_root);
            double l = 2 * k * k + q / 2 - r / (4 * k);
            double m = 2 * k * k + q / 2 + r / (4 * k);

            nRoots = 0;

            if (k * k - l > 0)
            {
                x[nRoots + 0] = -k - Math.Sqrt(k * k - l) - a1 / 4;
                x[nRoots + 1] = -k + Math.Sqrt(k * k - l) - a1 / 4;

                nRoots += 2;
            }

            if (k * k - m > 0)
            {
                x[nRoots + 0] = +k - Math.Sqrt(k * k - m) - a1 / 4;
                x[nRoots + 1] = +k + Math.Sqrt(k * k - m) - a1 / 4;

                nRoots += 2;
            }

            return nRoots;
        }

        public SharpDX.RectangleF MakeRectangle(float X, float Y, float Width, float Height)
        {
            return new SharpDX.RectangleF { X = X, Y = Y, Width = Width, Height = Height };
        }

        protected override void OnResize(EventArgs e)
        {
            int[] numArray2 = new int[4];
            numArray2[2] = base.Width;
            numArray2[3] = base.Height;
            int[] pMargins = numArray2;
            DwmExtendFrameIntoClientArea(base.Handle, ref pMargins);
        }

        private void onUpdate(object sender, EventArgs e)
        {
            if (UpdateStats())
            {
                int activeProcId;
                GetWindowThreadProcessId(GetForegroundWindow(), out activeProcId);
                Render(activeProcId == Mem.ProcessID);
            }
            else
            {
                Application.Exit();
            }
        }

        private void Overlay_FormClosing(object sender, FormClosedEventArgs formClosedEventArgs)
        {
            IsRunning = false;
            if (update != null)
            {
                update.Stop();
                update.Dispose();
            }
        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            base.TopMost = true;
            base.Visible = true;
            base.Name = "OverlayView";
            Text = "OverlayView";
            base.FormBorderStyle = FormBorderStyle.None;
            base.WindowState = FormWindowState.Maximized;
            base.MinimizeBox = base.MaximizeBox = false;
            MinimumSize = MaximumSize = base.Size;
            int windowLong = GetWindowLong(base.Handle, -20);
            SetWindowLong(base.Handle, -20, (windowLong | 0x80000) | 0x20);
            SetWindowPos(base.Handle, HWND_TOPMOST, 0, 0, 0, 0, 3);
            OnResize(null);
            factory = new SharpDX.Direct2D1.Factory();
            HwndRenderTargetProperties properties = new HwndRenderTargetProperties
            {
                Hwnd = base.Handle,
                PixelSize = new Size2(ClientSize.Width, ClientSize.Height),
                PresentOptions = PresentOptions.None
            };
            renderProperties = properties;
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);
            solidColorBrush = new SolidColorBrush(device, SharpDX.Color.Red);
            fontFactory = new SharpDX.DirectWrite.Factory();
            smallText = new TextFormat(fontFactory, "Segoe UI", 12f);
            medSmallText = new TextFormat(fontFactory, "Segoe UI", 15f);
            medSmallText2 = new TextFormat(fontFactory, "Segoe UI", 13.5f);
            medText = new TextFormat(fontFactory, "Segoe UI", 18f);
            largeText = new TextFormat(fontFactory, "Segoe UI", 24f);

            crosshairUnlocked = guiComponents.GetBitmap("crosshairUnlocked.png");
            crosshairLocked = guiComponents.GetBitmap("crosshairLocked.png");
            playerFriendly = guiComponents.GetBitmap("player.png");
            playerEnemy = guiComponents.GetBitmap("enemy.png");
            entity = guiComponents.GetBitmap("entity.png");
            genericVehicle = guiComponents.GetBitmap("vehicle.png");
            genericVehicleFriendly = guiComponents.GetBitmap("vehicleFriendly.png");

            vehicleTextures = new Dictionary<string, SharpDX.Direct2D1.Bitmap>();

            vehicleTextures.Add("heli.scout", guiComponents.GetBitmap("scoutHeli.png"));
            vehicleTextures.Add("heli.attack", guiComponents.GetBitmap("attackHeli.png"));
            vehicleTextures.Add("heli.transport", guiComponents.GetBitmap("supportHeli.png"));
            vehicleTextures.Add("jeep.fast", guiComponents.GetBitmap("fastJeep.png"));
            vehicleTextures.Add("quadbike", guiComponents.GetBitmap("fastBike.png"));
            vehicleTextures.Add("tank.ifv", guiComponents.GetBitmap("tank.png"));
            vehicleTextures.Add("tank", guiComponents.GetBitmap("tank.png"));
            vehicleTextures.Add("himars", guiComponents.GetBitmap("ifv.png"));
            vehicleTextures.Add("jeep.armored", guiComponents.GetBitmap("armoredJeep.png"));
            vehicleTextures.Add("vehicles.jet", guiComponents.GetBitmap("attackJet.png"));
            vehicleTextures.Add("jet.bomber", guiComponents.GetBitmap("bomberJet.png"));

            update = new Update(new EventHandler(onUpdate));
            update.FPS = 1000;
            update.Start();
        }

        public void setBrushAlpha(float v)
        {
            solidColorBrush.Color = new Color4(solidColorBrush.Color.Red, solidColorBrush.Color.Green, solidColorBrush.Color.Blue, v);
        }

        public float VectorDistance(Vector2D a, Vector2D b)
        {
            return (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private Vector3 VectorNormalize(Vector3 _Space)
        {
            Vector3 vector = new Vector3();
            float num = (float)Math.Sqrt((double)(((_Space.X * _Space.X) + (_Space.Y * _Space.Y)) + (_Space.Z * _Space.Z)));
            vector.X = _Space.X / num;
            vector.Y = _Space.Y / num;
            vector.Z = _Space.Z / num;
            return vector;
        }

        public Vector3D WorldToScreen(Vector3D pos)
        {
            Vector3D vectord = new Vector3D();
            float num = ((viewMatrix.M14 * pos.X) + (viewMatrix.M24 * pos.Y)) + ((viewMatrix.M34 * pos.Z) + viewMatrix.M44);
            if (num >= 0.0001f)
            {
                float num2 = ((viewMatrix.M11 * pos.X) + (viewMatrix.M21 * pos.Y)) + ((viewMatrix.M31 * pos.Z) + viewMatrix.M41);
                float num3 = ((viewMatrix.M12 * pos.X) + (viewMatrix.M22 * pos.Y)) + ((viewMatrix.M32 * pos.Z) + viewMatrix.M42);
                vectord.X = CrosshairX + ((CrosshairX * num2) / num);
                vectord.Y = CrosshairY - ((CrosshairY * num3) / num);
                vectord.Z = num;
            }
            return vectord;
        }

        private Vector3D WorldToScreen_Head(Vector3D pos, int _Pose)
        {
            Vector3D vectord = new Vector3D();
            float y = pos.Y;
            if (_Pose == 0)
            {
                y += 1.8f;
            }
            if (_Pose == 1)
            {
                y += 1.1f;
            }
            if (_Pose == 2)
            {
                y += 0.7f;
            }
            float num2 = ((viewMatrix.M14 * pos.X) + (viewMatrix.M24 * y)) + ((viewMatrix.M34 * pos.Z) + viewMatrix.M44);
            if (num2 >= 0.0001f)
            {
                float num3 = ((viewMatrix.M11 * pos.X) + (viewMatrix.M21 * y)) + ((viewMatrix.M31 * pos.Z) + viewMatrix.M41);
                float num4 = ((viewMatrix.M12 * pos.X) + (viewMatrix.M22 * y)) + ((viewMatrix.M32 * pos.Z) + viewMatrix.M42);
                vectord.X = CrosshairX + ((CrosshairX * num3) / num2);
                vectord.Y = CrosshairY - ((CrosshairY * num4) / num2);
                vectord.Z = num2;
            }
            return vectord;
        }

        public int CrosshairX
        {
            get
            {
                return (WindowWidth / 2);
            }
        }

        public int CrosshairY
        {
            get
            {
                return (WindowHeight / 2);
            }
        }

        public long pScreen
        {
            get
            {
                long num = Mem.ReadInt64(sAddresses.DXRenderer);
                if (num != 0L)
                {
                    return Mem.ReadInt64(num + 0x38L);
                }
                return 0L;
            }
        }

        public int WindowHeight
        {
            get
            {
                if (pScreen != 0L)
                {
                    return Mem.ReadInt32(pScreen + sOffsets.DXRenderer.Screen.Height);
                }
                return 0;
            }
        }

        public int WindowWidth
        {
            get
            {
                if (pScreen != 0L)
                {
                    return Mem.ReadInt32(pScreen + sOffsets.DXRenderer.Screen.Width);
                }
                return 0;
            }
        }

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public Overlay.POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Overlay
            // 
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(284, 261);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "Overlay";
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            TransparencyKey = System.Drawing.Color.Black;
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ResumeLayout(false);
            Load += Overlay_Load;
        }
    }
}