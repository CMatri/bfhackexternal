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

        public static bool ESPAlways2D = true;
        public static bool ESPBox = true;
        public static bool ESPDistance = true;
        public static bool ESPEntities = true;
        public static bool ESPHead = true;
        public static bool ESPHealth = true;
        public static bool ESPLines = true;
        public static bool ESPSkeleton = true;
        public static bool ESPVehicle = true;

        public static bool crossHair = true;
        public static bool guiDefinedCrosshair = true;

        public static float minimapScaleRatio = 0.75f;
        public static int minimapPlayerRad = 3;

        public static bool noRecoil = true;
        public static bool noSway = true;

        public static bool proxWarning = true;

        public static bool playerSearch;
    }

    public partial class Overlay
    {
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
        private HookProc MouseHookProcedure;
        private Thread keyBoardThread;
        private Thread mouseThread;

        public Memory Mem = new Memory();

        private List<Player> enemys = new List<Player>();
        private List<Player> players = new List<Player>();
        private List<Player> spectators = new List<Player>();
        public Player LocalPlayer = new Player();

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

        private SharpDX.Direct2D1.Bitmap test;
        public SharpDX.Direct2D1.Bitmap crosshair;

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
            long num2 = Mem.ReadInt64(num + sOffsets.ClassClientPlayerManager);
            long pClient = Mem.ReadInt64(num2 + sOffsets.ClientPlayerManger.LocalPlayer);
            long num4 = Mem.ReadInt64(Mem.ReadInt64(pClient + 0x14b0L)) - 8L;
            long num5 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassHealth);
            long num6 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassPlayerPosition);
            long num7 = Mem.ReadInt64(num4 + sOffsets.Player.Soldier.ClassClientRagDollComponent);
            viewMatrix = getViewMatrix();
            LocalPlayer.Health = Mem.ReadFloat(num5 + sOffsets.Player.Soldier.PlayerHealth.CurPlayerHealth);
            LocalPlayer.Team = Mem.ReadInt32(pClient + sOffsets.Player.Team);
            LocalPlayer.Name = Mem.ReadString(pClient + sOffsets.Player.Name, 0x10L);
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
            long num9 = Mem.ReadInt64(num8 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClassClientAnimatedSoldierWeaponHandler);
            int num10 = Mem.ReadInt32(num8 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ActiveSlot);
            long num11 = Mem.ReadInt64(num9 + (num10 * 8));
            long num12 = Mem.ReadInt64(num11 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.ClassWeaponFiring);
            long num13 = Mem.ReadInt64(num11 + 0x4850L);
            long num14 = Mem.ReadInt64(num13 + 0x20L);
            long num15 = Mem.ReadInt64(num11 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.ClassClientSoldierAimingSimulation);
            LocalPlayer.Pitch = Mem.ReadFloat(num4 + sOffsets.Player.Soldier.Pitch);
            LocalPlayer.Ammo = Mem.ReadInt32(num12 + 0x1a0L);
            LocalPlayer.MaxAmmo = Mem.ReadInt32(num12 + 420L);
            LocalPlayer.WeaponName = Mem.ReadString(num14, 7L);
            LocalPlayer.Skeleton.BoneHead = GetBonyById(Mem.ReadInt64(Mem.ReadInt64(pClient + 0x14b0L)) - 8L, 0x68);
            LocalPlayer.Position.X = Mem.ReadFloat(num6 + sOffsets.Player.Soldier.PlayerPosition.XPlayer);
            LocalPlayer.Position.Y = Mem.ReadFloat(num6 + sOffsets.Player.Soldier.PlayerPosition.YPlayer);
            LocalPlayer.Position.Z = Mem.ReadFloat(num6 + sOffsets.Player.Soldier.PlayerPosition.ZPlayer);

            if (!LocalPlayer.InVehicle)
            {
                LocalPlayer.Velocity.X = Mem.ReadFloat(num6 + 80L);
                LocalPlayer.Velocity.Y = Mem.ReadFloat(num6 + 0x54L);
                LocalPlayer.Velocity.Z = Mem.ReadFloat(num6 + 0x58L);
                if (num10 < 2)
                {
                    long num16 = Mem.ReadInt64(num12 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.WeaponFiring.WeaponSway);
                    long num17 = Mem.ReadInt64(num16 + 8L);
                    AimLevel = Mem.ReadFloat(num15 + sOffsets.Player.Soldier.ClientSoldierWeaponsComponent.ClientAnimatedSoldierWeaponHandler.ClientSoldierWeapon.ClientSoldierAimingSimulation.ZoomLevel);

                    Settings.crossHair = !((AimLevel < 1f) && LocalPlayer.WeaponName.Contains("M200"));

                    Mem.WriteFloat(num17 + 0x374L, 1f);
                    Mem.WriteFloat(num17 + 880L, 100f);
                    long num19 = Mem.ReadInt64(num4 + sOffsets.WeaponModifier.BreathControl);
                    byte num20 = 0;
                    Mem.WriteInt32(num19 + 0x58L, num20);
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
            LocalPlayer.VehicleName = Mem.ReadString(Mem.ReadInt64(num29 + 240L), 13L);
            long num30 = Mem.ReadInt64(num12 + sOffsets.WeaponFiring.ClassPrimaryFire);
            long num31 = Mem.ReadInt64(num30 + sOffsets.WeaponFiring.PrimaryFire.ClassShotConfigData);
            if (num10 == 0)
            {
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShot, 2);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShell, 0x4b0);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerBurst, 1);
            }
            else if (num10 == 1)
            {
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShot, 2);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerShell, 2);
                Mem.WriteInt32(num31 + sOffsets.WeaponFiring.PrimaryFire.ShotConfigData.NumberOfBulletsPerBurst, 2);
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
            long address = Mem.ReadInt64(num12 + 0x128L);
            if (IsValidPtr(address))
            {
                long num33 = Mem.ReadInt64(address + 0x10L);
                if (IsValidPtr(num33))
                {
                    long num34 = Mem.ReadInt64(num33 + 0xb0L);
                    if (IsValidPtr(address) && (num15 != 0L))
                    {
                        LocalPlayer.BulletGravity = 0f;
                        LocalPlayer.BulletSpeed = 0f;
                        LocalPlayer.BulletGravity = Mem.ReadFloat(num34 + 0x130L);
                        LocalPlayer.BulletSpeed = Mem.ReadFloat(num33 + 0x88L);
                        LocalPlayer.Sway.X = Mem.ReadFloat(num15 + 40L);
                        LocalPlayer.Sway.Y = Mem.ReadFloat(num15 + 0x2cL);
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
                    long clientSoldier = GetClientSoldier(num37, player);
                    long pSoldier = Mem.ReadInt64(Mem.ReadInt64(num37 + 0x14b0L)) - 8L;
                    long num40 = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassHealth);
                    long num41 = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassPlayerPosition);
                    long num42 = Mem.ReadInt64(pSoldier + sOffsets.Player.Soldier.ClassClientRagDollComponent);
                    long num43 = Mem.ReadInt64(num42 + 0xb0L);

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
                        long num45 = Mem.ReadInt64(num37 + 0x1520L);
                        long num46 = Mem.ReadInt64(num37 + 0x1510L);
                        player.pRenderView = num45;
                        player.pOwnerRenderView = num46;
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
            long num48 = Mem.ReadInt64(sAddresses.ViewAngles);
            long num49 = Mem.ReadInt64(num48 + 0x4988L);
            long num50 = Mem.ReadInt64(num49 + 0x10L);
            float a = Mem.ReadFloat(num50 + 20L);
            float num52 = Mem.ReadFloat(num50 + 0x18L);
            if (((Keyboard.IsKeyDown(Keys.LShiftKey) && (AimLevel < 1f))) && !LocalPlayer.InVehicle)
            {
                Player player2 = DistanceCrosshairSortPlayers(enemys);
                Vector2 onScreen = new Vector2();
                ViewAngle aimHead = GetAimHead(player2, ref onScreen);
                if (player2 == null) return true;
                aimTo = new Vector3(onScreen, player2.IsOccluded ? 0 : 1);
                //Console.WriteLine(Distance_Crosshair(new Vector2D(onScreen.X, onScreen.Y)));
                if (Settings.aimbot && (player2 != null) && (((!player2.IsOccluded || (player2.InVehicle && (player2.VehicleEntry == 0f))) && (aimHead != null)) && (player2.Distance_Crosshair < (player2.InVehicle ? (160) : (70)))))
                {
                    if (Math.Abs(Distance_Crosshair(new Vector2D(onScreen.X, onScreen.Y))) < 10 || player2.InVehicle)
                    {
                        Vector2 vector = new Vector2(LerpRadians(a, aimHead.Yaw, 0.855f), LerpRadians(num52, aimHead.Pitch, 0.855f));
                        Mem.WriteAngle(vector.X, vector.Y);
                    }
                    else
                    {
                        Vector2 vector = new Vector2(LerpRadians(a, aimHead.Yaw, 0.275f), LerpRadians(num52, aimHead.Pitch, 0.275f));
                        Mem.WriteAngle(vector.X, vector.Y);
                    }

                }
            }
            else
                aimTo = new Vector3(-200, -200, 0);
            return true;
        }

        private Vector3 aimTo;

        public void Render()
        {
            long num2;
            Vector3D vectord;
            Vector3D vectord2;
            AxisAlignedBox box;
            SharpDX.Color color2;
            device.BeginDraw();
            device.Clear(new Color4(0f, 0f, 0f, 0f));

            //guiComponents.DrawBitmap(test, MakeRectangle(200, 200, 26, 32), (float)(LocalPlayer.Yaw));

            guiComponents.DrawCrosshair(aimTo.X, aimTo.Y, 20, 20, aimTo.Z == 1 ? SharpDX.Color.LightGreen : SharpDX.Color.OrangeRed, false);

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
                    box.Setup(LocalPlayer.Position, LocalPlayer.SoldierTransform, true, 0f, 0f, 0f);
                    color2 = new SharpDX.Color();
                    guiComponents.DrawAxisAlignedBoundingBox(LocalPlayer, box, color2, false);
                    if (LocalPlayer.VehicleName.Contains("heli"))
                    {
                        color2 = new SharpDX.Color();
                        guiComponents.DrawAxisAlignedBoundingBox(LocalPlayer, null, color2, false);
                    }
                }
            }
            if (((LocalPlayer.Position.X + LocalPlayer.Position.Y) + LocalPlayer.Position.Z) != 0f)
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
                    //if (player.IsSpectator) Console.WriteLine(player.Name);
                    if (player.pOwnerRenderView == LocalPlayer.pOwnerRenderView && player.IsSpectator)
                    {
                        solidColorBrush.Color = new SharpDX.Color(40, 40, 40, 150);
                        device.FillRectangle(MakeRectangle((WindowWidth / 2) - 102.5f, 68.5f, 214f, 29f), solidColorBrush);
                        solidColorBrush.Color = SharpDX.Color.Gray;
                        device.DrawRectangle(MakeRectangle((WindowWidth / 2) - 102.5f, 68.5f, 215f, 30f), solidColorBrush);
                        guiComponents.DrawText("SPECTATOR WARNING", (WindowWidth / 2) + 42, 72, 0x1a2, 30, true, medSmallText2, SharpDX.Color.Red);
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
                            solidColorBrush.Color = SharpDX.Color.Green;
                        }
                        else if (!player.IsOccluded)
                        {
                            solidColorBrush.Color = SharpDX.Color.Yellow;
                        }
                        else
                        {
                            solidColorBrush.Color = SharpDX.Color.Red;
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
                            guiComponents.DrawText("Player Found!", 100, 0, 300, 20, false, medSmallText2, SharpDX.Color.LightGray);
                            solidColorBrush.Color = color;
                        }
                        if (!player.InVehicle)
                        {
                            if ((((player.Team != LocalPlayer.Team) && (player.pClient != LocalPlayer.pClient)) && (Settings.ESPSkeleton && player.IsValid)) && LocalPlayer.IsValid)
                            {
                                color = solidColorBrush.Color;
                                guiComponents.DrawSkeleton(player);
                                solidColorBrush.Color = color;
                            }
                            if (Settings.ESPHealth)
                            {
                                guiComponents.DrawHealthBar((((int)vectord4.X) - ((int)(num5 / 2f))) - 8, ((int)vectord4.Y) - ((int)num4), 5, (int)num4, (int)player.Health, (int)player.MaxHealth, true);
                            }
                            if (Settings.ESPBox)
                            {
                                if (!((player.Distance_3D >= 100f) || Settings.ESPAlways2D))
                                {
                                    color2 = new SharpDX.Color();
                                    guiComponents.DrawAxisAlignedBoundingBox(player, null, color2, false);
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
                                data.Color = new SharpDX.Color(solidColorBrush.Color);
                                data.Type = 0;
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
                                data.Color = new SharpDX.Color(solidColorBrush.Color);
                                data.Type = 1;
                                guiComponents.toDrawOnRadar.Add(data);
                                list.Add(num);
                                box = new AxisAlignedBox();
                                box.Init(vectord, vectord2);
                                box.Setup(player.Vehicle, player.SoldierTransform, true, 0f, 0f, 0f);
                                color2 = new SharpDX.Color();
                                guiComponents.DrawAxisAlignedBoundingBox(player, box, color2, false);
                            }
                            if (player.Team != LocalPlayer.Team)
                            {
                                if (Settings.ESPBox)
                                {
                                    guiComponents.DrawEsp(rectangle);
                                }
                                if (Settings.ESPSkeleton)
                                {
                                    guiComponents.DrawSkeleton(player);
                                }
                                if (Settings.ESPHealth)
                                {
                                    guiComponents.DrawHealthBar((((int)vectord4.X) - ((int)(num5 / 2f))) - 8, ((int)vectord4.Y) - ((int)num4), 5, (int)num4, (int)player.Health, (int)player.MaxHealth, true);
                                }
                            }
                        }
                        Vector3D vectord5 = WorldToScreen(player.Skeleton.BoneHead);
                        if (player.Team != LocalPlayer.Team)
                        {
                            if ((!player.IsOccluded && Settings.ESPHead) && (player.Distance_Crosshair < 110f))
                            {
                                guiComponents.DrawCrosshair(vectord5.X, vectord5.Y, 10, 10, new SharpDX.Color(solidColorBrush.Color), false);
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
                                        device.DrawLine(new Vector2(vectord4.X, vectord4.Y), new Vector2((Width / 2), Height), solidColorBrush, 2f);
                                    }
                                    else if (player.IsOccluded && (player.Distance_3D <= 50f))
                                    {
                                        device.DrawLine(new Vector2(vectord4.X, vectord4.Y), new Vector2((Width / 2), Height), solidColorBrush, 2f);
                                    }
                                }
                            }
                        }
                        if (Settings.ESPDistance)
                        {
                            guiComponents.DrawText(((int)player.Distance_3D) + "m", (int)vectord4.X, ((int)vectord3.Y) + ((int)num4), 200, 20, true, smallText, SharpDX.Color.White);
                        }
                    }
                }
                if (flag && Settings.proxWarning)
                {
                    guiComponents.DrawText("PROXIMITY WARNING", (WindowWidth / 2) - 0x19, WindowHeight / 8, 0x198, 30, true, largeText, SharpDX.Color.Red);
                    guiComponents.DrawText(((num3 > 0f) ? ("x" + num3) : ("")), (WindowWidth / 2) + 130, WindowHeight / 8, 0x198, 30, true, largeText, SharpDX.Color.Red);
                }
                if (Settings.crossHair && Settings.guiDefinedCrosshair)
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
            guiComponents.DrawText("External Multihack by Slyth", 0x17, 0, 300, 20, false, medSmallText2, SharpDX.Color.LightGray);

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
                device.FillRectangle(MakeRectangle((WindowWidth / 2) - 102.5f, 38.5f, 214f, 29f), solidColorBrush);
                solidColorBrush.Color = SharpDX.Color.Gray;
                device.DrawRectangle(MakeRectangle((WindowWidth / 2) - 102.5f, 38.5f, 215f, 30f), solidColorBrush);
                guiComponents.DrawText("Closest enemy is " + num7 + " meters away", (WindowWidth / 2) + 0x5f, 40, 0x1a2, 30, true, medSmallText2, SharpDX.Color.Red);
            }
            device.EndDraw();
        }

        private Vector3 AimCorrection(Vector3 Velocity, Vector3 EnemyVelocity, Vector3 InVec, float Distance, float Speed, float Gravity)
        {
            InVec += (EnemyVelocity * (Distance / Math.Abs(Speed)));
            InVec -= (Velocity * (Distance / Math.Abs(Speed)));
            float num = Math.Abs(Gravity);
            float num2 = Distance / Math.Abs(Speed);
            InVec.Y += ((0.5f * num) * num2) * num2;
            return InVec;
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


        private ViewAngle GetAimHead(Player _EnemyPlayer, ref Vector2 onScreen)
        {
            if (_EnemyPlayer == null) return new ViewAngle();
            if (_EnemyPlayer.Skeleton.BoneHead.X != 0 &&
                _EnemyPlayer.Skeleton.BoneHead.Y != 0 &&
                _EnemyPlayer.Skeleton.BoneHead.Z != 0)
            {
                Vector3 Space = new Vector3();
                Vector3 Origin = AimCorrection(
                    new Vector3(LocalPlayer.Velocity.X, LocalPlayer.Velocity.Y, LocalPlayer.Velocity.Z),
                    new Vector3(_EnemyPlayer.Velocity.X, _EnemyPlayer.Velocity.Y, _EnemyPlayer.Velocity.Z),
                    new Vector3(_EnemyPlayer.Skeleton.BoneHead.X, _EnemyPlayer.Skeleton.BoneHead.Y, _EnemyPlayer.Skeleton.BoneHead.Z),
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

        private bool IsOnDeployScreen()
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
                Render();
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

            test = guiComponents.GetBitmap("player.png");
            crosshair = guiComponents.GetBitmap("crosshair.png");

            update = new Update(new EventHandler(onUpdate));
            update.FPS = 100;
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
