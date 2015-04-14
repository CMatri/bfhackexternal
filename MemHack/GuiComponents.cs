using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MemLibs;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.IO;
using SharpDX.WIC;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using Matrix3x3 = SharpDX.Matrix3x3;

namespace MemHack
{
    class GuiComponents
    {
        private Overlay overlay;

        private bool hackMenuExpanded = true;
        public bool infoExpanded = true;
        public bool infoExpandedBeforeDeath = true;
        private bool playerSearchExpanded = true;
        public bool radarExpanded = true;
        public bool radarExpandedBeforeDeath = true;

        public List<TextBox> textBoxes = new List<TextBox>();
        public List<RadarData> toDrawOnRadar = new List<RadarData>();

        private Rectangle radar;

        [StructLayout(LayoutKind.Sequential)]
        public struct RadarData
        {
            public Vector2D Pos;
            public int Type;
            public float Rotation;
            public int Team;
            public Player Owner;
        }

        public GuiComponents(Overlay overlay)
        {
            this.overlay = overlay;
        }

        public void DrawGuiBox(int X, int Y, int Width, int Height, ref bool expanded, string name = "")
        {
            Color4 color = overlay.solidColorBrush.Color;
            Height = expanded ? (Height + 20) : 20;
            overlay.solidColorBrush.Color = new Color(40, 40, 40, 150);
            overlay.device.FillRectangle(overlay.MakeRectangle((X + 1), (Y - 20), (Width - 1), (Height - 1)), overlay.solidColorBrush);
            overlay.device.FillRectangle(overlay.MakeRectangle((X + 1), (Y - 20), (Width - 1), 19f), overlay.solidColorBrush);
            overlay.solidColorBrush.Color = Color.Gray;
            overlay.device.DrawRectangle(overlay.MakeRectangle(X, (Y - 20), Width, Height), overlay.solidColorBrush);
            overlay.solidColorBrush.Color = new Color4(Color.Gray.R, Color.Gray.G, Color.Gray.B, 0.5f);
            if (expanded)
                overlay.device.DrawLine(new Vector2(X, Y), new Vector2((X + Width), Y), overlay.solidColorBrush);
            DrawText(name, X + 5, Y - 0x15, 200, 20, false, overlay.medSmallText, Color.LightGray);
            DrawGuiButton((X + Width) - 0x10, Y - 0x10, 12, 12, ref expanded, true);
            overlay.solidColorBrush.Color = color;
        }

        public void DrawGuiButton(int X, int Y, int Width, int Height, ref bool clicked, bool colorful = false)
        {
            Color4 color = overlay.solidColorBrush.Color;
            overlay.solidColorBrush.Color = Color.Gray;
            overlay.device.DrawRectangle(overlay.MakeRectangle(X, Y, Width, Height), overlay.solidColorBrush);
            Color color2 = colorful ? (clicked ? Color.Green : Color.Red) : Color.Gray;
            overlay.solidColorBrush.Color = new Color4(color2.R, color2.G, color2.B, 0.25f);
            if (!(!clicked || colorful))
            {
                overlay.device.FillRectangle(overlay.MakeRectangle((X + 2), (Y + 2), (Width - 4), (Height - 4)), overlay.solidColorBrush);
            }
            else if (colorful)
            {
                overlay.device.FillRectangle(overlay.MakeRectangle((X + 2), (Y + 2), (Width - 4), (Height - 4)), overlay.solidColorBrush);
            }
            if (overlay.MakeRectangle(X, Y, Width, Height).Contains(Mouse.MouseDownPos().X, Mouse.MouseDownPos().Y) && (Mouse.timeSinceLastClick > 0))
            {
                Mouse.timeSinceLastClick = 0;
                clicked = !clicked;
            }
            overlay.solidColorBrush.Color = color;
        }

        public void DrawEsp(RectangleF rect)
        {
            overlay.setBrushAlpha(0.5f);
            overlay.device.DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + (rect.Height / 4f)), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X + (rect.Width / 4f), rect.Y), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X + rect.Width, rect.Y), new Vector2((rect.X + rect.Width) - (rect.Width / 4f), rect.Y), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + (rect.Height / 4f)), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X, (rect.Y + rect.Height) - (rect.Height / 4f)), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + (rect.Width / 4f), rect.Y + rect.Height), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X + rect.Width, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, (rect.Y + rect.Height) - (rect.Height / 4f)), overlay.solidColorBrush, 2f);
            overlay.device.DrawLine(new Vector2(rect.X + rect.Width, rect.Y + rect.Height), new Vector2((rect.X + rect.Width) - (rect.Width / 4f), rect.Y + rect.Height), overlay.solidColorBrush, 2f);
        }

        public void DrawHealthBar(int X, int Y, int Width, int Height, int Health, int MaxHealth, bool vertical)
        {
            Color4 color = overlay.solidColorBrush.Color;
            if (MaxHealth < Health)
            {
                MaxHealth = 100;
            }
            Color red = Color.Red;
            int num = (int)((Health) / ((MaxHealth) / 100f));
            if (num >= 0)
            {
                red = Color.Red;
            }
            if (num >= 10)
            {
                red = Color.OrangeRed;
            }
            if (num >= 20)
            {
                red = Color.Orange;
            }
            if (num >= 40)
            {
                red = Color.Yellow;
            }
            if (num >= 60)
            {
                red = Color.YellowGreen;
            }
            if (num >= 80)
            {
                red = Color.Green;
            }
            if (vertical)
            {
                int num2 = (int)(((Height) / 100f) * num);
                if ((num < 1) || (Health < 1))
                {
                    DrawFillRect(X, Y - 1, Width + 1, Height + 2, Color.Black);
                }
                else
                {
                    DrawFillRect(X, Y - 1, Width + 1, Height + 2, Color.Black);
                    DrawFillRect(X + 1, Y + Height, Width - 1, -num2 - 1, red);
                }
            }
            else
            {
                int num3 = (int)(((Width) / 100f) * num);
                if ((num < 1) || (Health < 1))
                {
                    DrawFillRect(X, Y - 1, Width + 1, Height + 2, Color.Black);
                }
                else
                {
                    DrawFillRect(X, Y - 1, Width + 1, Height + 2, Color.Black);
                    DrawFillRect(X + 1, Y, num3 - 1, Height, red);
                }
            }
            overlay.solidColorBrush.Color = color;
        }

        public void DrawCrosshair(float X, float Y, int Width, int Height, Color color, bool complex)
        {
            if (complex)
            {
                //overlay.solidColorBrush.Color = new Color(0, 0, 0, 170);
                //overlay.device.DrawRectangle(new RectangleF((X - (Width / 2)) - 7f, Y - 2f, 14f, 4f), overlay.solidColorBrush);
                //overlay.device.DrawRectangle(new RectangleF((X + (Width / 2)) - 7f, Y - 2f, 14f, 4f), overlay.solidColorBrush);
                //overlay.device.DrawRectangle(new RectangleF(X - 2f, (Y - (Width / 2)) - 7f, 4f, 14f), overlay.solidColorBrush);
                //overlay.device.DrawRectangle(new RectangleF(X - 2f, (Y + (Width / 2)) - 7f, 4f, 14f), overlay.solidColorBrush);
                //overlay.solidColorBrush.Color = color;
                //overlay.device.DrawLine(new Vector2((X - (Width / 2)) - 6f, Y), new Vector2(X - 4f, Y), overlay.solidColorBrush);
                //overlay.device.DrawLine(new Vector2((X + (Width / 2)) + 6f, Y), new Vector2(X + 4f, Y), overlay.solidColorBrush);
                //overlay.device.DrawLine(new Vector2(X, (Y - (Height / 2)) - 6f), new Vector2(X, Y - 4f), overlay.solidColorBrush);
                //overlay.device.DrawLine(new Vector2(X, (Y + (Height / 2)) + 6f), new Vector2(X, Y + 4f), overlay.solidColorBrush);

                if (overlay.isAimbotting) DrawBitmap(overlay.crosshairLocked, overlay.MakeRectangle(overlay.Width / 2 - 32 / 2, overlay.Height / 2 - 32 / 2, 32, 32), 0);
                else DrawBitmap(overlay.crosshairUnlocked, overlay.MakeRectangle(overlay.Width / 2 - 32 / 2, overlay.Height / 2 - 32 / 2, 32, 32), 0);
            }
            else
            {
                overlay.solidColorBrush.Color = color;
                overlay.device.DrawLine(new Vector2(X - (Width / 2), Y), new Vector2(X + (Width / 2), Y), overlay.solidColorBrush);
                overlay.device.DrawLine(new Vector2(X, Y - (Width / 2)), new Vector2(X, Y + (Width / 2)), overlay.solidColorBrush);
            }
        }

        public void DrawPlayerSearch()
        {
            Rectangle rectangle;
            rectangle = new Rectangle();
            rectangle.Width = 175;
            rectangle.Height = 73;
            rectangle.X = (overlay.Width - 7) - rectangle.Width;
            rectangle.Y = ((730 - (!infoExpanded ? 90 : 0)) - (!radarExpanded ? 250 : 0)) - (!hackMenuExpanded ? 310 : 0);
            DrawGuiBox(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, ref playerSearchExpanded, "Player Search");
            if (playerSearchExpanded)
            {
                GetTextBox("Search").Render = true;
                DrawText("Enabled", rectangle.X + 10, rectangle.Y + 2, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 8, 12, 12, ref Settings.playerSearch, false);
                DrawText("Clear", rectangle.X + 10, rectangle.Y + 20, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x1a, 12, 12, ref overlay.clearPlayerSearch, false);
                GetTextBox("Search").X = rectangle.X + 10;
                GetTextBox("Search").Y = rectangle.Y + 0x2c;
                if (overlay.clearPlayerSearch)
                {
                    GetTextBox("Search").Text = "";
                }
                overlay.clearPlayerSearch = false;
            }
            else
            {
                GetTextBox("Search").Render = false;
            }
        }

        public void DrawHackMenu()
        {
            Rectangle rectangle;
            rectangle = new Rectangle();
            rectangle.Width = 175;
            rectangle.Height = 280;
            rectangle.X = (overlay.Width - 7) - rectangle.Width;
            rectangle.Y = (425 - (!infoExpanded ? 90 : 0)) - (!radarExpanded ? 250 : 0);
            DrawGuiBox(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, ref hackMenuExpanded, "Hack Menu");
            if (hackMenuExpanded)
            {
                DrawText("Aimbot (C)", rectangle.X + 10, rectangle.Y + 2, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 8, 12, 12, ref Settings.aimbot, false);
                DrawText("Crosshair", rectangle.X + 10, rectangle.Y + 20, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x1a, 12, 12, ref Settings.guiDefinedCrosshair, false);
                DrawText("No Recoil", rectangle.X + 10, rectangle.Y + 0x26, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x2c, 12, 12, ref Settings.noRecoil, false);
                DrawText("No Breath", rectangle.X + 10, rectangle.Y + 0x38, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x3e, 12, 12, ref Settings.noSway, false);
                DrawText("Proximity Warnings", rectangle.X + 10, rectangle.Y + 0x4a, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 80, 12, 12, ref Settings.proxWarning, false);
                DrawText("Box ESP", rectangle.X + 10, rectangle.Y + 0x5c, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x62, 12, 12, ref Settings.ESPBox, false);
                DrawText("Box ESP Always 2D", rectangle.X + 10, rectangle.Y + 110, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x74, 12, 12, ref Settings.ESPAlways2D, false);
                DrawText("Skeleton ESP", rectangle.X + 10, rectangle.Y + 0x80, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x86, 12, 12, ref Settings.ESPSkeleton, false);
                DrawText("Head ESP", rectangle.X + 10, rectangle.Y + 0x92, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0x98, 12, 12, ref Settings.ESPHead, false);
                DrawText("Distance ESP", rectangle.X + 10, rectangle.Y + 0xa4, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 170, 12, 12, ref Settings.ESPDistance, false);
                DrawText("Health ESP", rectangle.X + 10, rectangle.Y + 0xb6, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0xbc, 12, 12, ref Settings.ESPHealth, false);
                DrawText("Lines ESP", rectangle.X + 10, rectangle.Y + 200, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0xce, 12, 12, ref Settings.ESPLines, false);
                DrawText("Vehicle ESP", rectangle.X + 10, rectangle.Y + 0xda, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0xe0, 12, 12, ref Settings.ESPVehicle, false);
                DrawText("Entity ESP", rectangle.X + 10, rectangle.Y + 0xec, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 0xf2, 12, 12, ref Settings.ESPEntities, false);
                DrawText("Smooth Bot", rectangle.X + 10, rectangle.Y + 0xfe, 200, 20, false, overlay.medSmallText2, Color.LightGray);
                DrawGuiButton((rectangle.X + rectangle.Width) - 0x16, rectangle.Y + 260, 12, 12, ref Settings.smoothBot, false);
            }
        }

        public void DrawInfo()
        {
            Rectangle rectangle;
            rectangle = new Rectangle();
            rectangle.Width = 250;
            rectangle.Height = 90;
            rectangle.X = (overlay.Width - 7) - rectangle.Width;
            rectangle.Y = 35;
            DrawGuiBox(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, ref infoExpanded, "Info");
            if (infoExpanded)
            {
                infoExpandedBeforeDeath = true;
                int num = (int)(overlay.LocalPlayer.Health / (overlay.LocalPlayer.MaxHealth / 100f));
                if (num == 0xa6)
                {
                    num = (int)(overlay.LocalPlayer.Health / (overlay.LocalPlayer.MaxHealth / 60f));
                }
                DrawText(overlay.LocalPlayer.Ammo + "/" + overlay.LocalPlayer.MaxAmmo, rectangle.X + 10, rectangle.Y + 10, 100, 20, false, overlay.largeText, Color.LightGray);
                DrawText("+" + num, rectangle.X + ((num < 100) ? ((num < 10) ? 200 : 0xc3) : 0xb9), rectangle.Y + 10, 100, 20, false, overlay.largeText, Color.LightGray);
                DrawText(overlay.spectators.Count + " Spectators on server.", rectangle.X + 35, rectangle.Y + 50, 400, 20, false, overlay.medText, Color.LightGray);
                // DrawHealthBar(rectangle.X + 10, rectangle.Y + 60, rectangle.Width - 20, 10, (int)overlay.LocalPlayer.Health, (int)overlay.LocalPlayer.MaxHealth, false);
            }

            DrawHealthBar(0, 0, overlay.Width, 10, (int)overlay.LocalPlayer.Health, (int)overlay.LocalPlayer.MaxHealth, false);
        }

        public void DrawRadar()
        {
            Color4 color = overlay.solidColorBrush.Color;
            radar = new Rectangle();
            radar.Width = 250;
            radar.Height = 250;
            radar.X = (overlay.Width - 7) - radar.Width;
            radar.Y = 150 - (!infoExpanded ? 90 : 0);
            DrawGuiBox(radar.X, radar.Y, radar.Width, radar.Height, ref radarExpanded, "Radar");
            if (radarExpanded)
            {
                radarExpandedBeforeDeath = true;
                overlay.solidColorBrush.Color = Color.Gray;
                overlay.device.DrawLine(new Vector2((radar.X + radar.Width), radar.Y + ((radar.Height) / 2f)), new Vector2((radar.X + ((radar.Width) / 2f)) + 5f, radar.Y + ((radar.Height) / 2f)), overlay.solidColorBrush);
                overlay.device.DrawLine(new Vector2(radar.X, radar.Y + ((radar.Height) / 2f)), new Vector2((radar.X + ((radar.Width) / 2f)) - 5f, radar.Y + ((radar.Height) / 2f)), overlay.solidColorBrush);
                overlay.device.DrawLine(new Vector2(radar.X + ((radar.Width) / 2f), (radar.Y + radar.Height)), new Vector2(radar.X + ((radar.Width) / 2f), (radar.Y + ((radar.Height) / 2f)) + 5f), overlay.solidColorBrush);
                overlay.device.DrawLine(new Vector2(radar.X + ((radar.Width) / 2f), radar.Y), new Vector2(radar.X + ((radar.Width) / 2f), (radar.Y + ((radar.Height) / 2f)) - 5f), overlay.solidColorBrush);
                overlay.device.DrawEllipse(new Ellipse(radar.Center, 4f, 4f), overlay.solidColorBrush);
                foreach (RadarData data in toDrawOnRadar)
                {
                    DrawOnRadar(data.Pos, radar, data.Team, data.Rotation, data.Type, data.Owner);
                }
            }
            overlay.solidColorBrush.Color = color;
        }

        public void DrawLockedOnPlayerInfo(Player p)
        {
            if (p.Skeleton.BoneHead.X + p.Skeleton.BoneHead.Y + p.Skeleton.BoneHead.Z > 0.1f)
            {
                Vector3D headPos = overlay.WorldToScreen(p.Skeleton.BoneHead);
                Vector2D drawPos = new Vector2D(headPos.X + 20, headPos.Y);
                Color drawColor = p.IsOccluded ? Color.Orange : Color.LimeGreen;

                DrawText(p.Name, (int)drawPos.X, (int)drawPos.Y - 20, 400, 20, false, overlay.medSmallText2, drawColor);
                DrawText("Distance: " + (int)p.Distance_3D, (int)drawPos.X, (int)drawPos.Y, 400, 20, false, overlay.medSmallText2, drawColor);
                //DrawText("Health: " + (int)p.Health + "/" + (int)p.MaxHealth, (int)drawPos.X, (int)drawPos.Y + 20, 400, 20, false, overlay.medSmallText2, Color.Wheat);
                DrawHealthBar((int)drawPos.X, (int)drawPos.Y + 25, 80, 5, (int)p.Health, (int)p.MaxHealth, false);
            }
        }

        public void DrawEntities()
        {
            DrawClientEntity(0x142851810L, 0x220L, new Vector3D(-0.125f, -0.045f, -0.125f), new Vector3D(0.125f, 0.045f, 0.125f), Color.Purple);
            DrawClientEntity(0x1428516d0L, 0x220L, new Vector3D(-0.05f, -0.05f, -0.05f), new Vector3D(0.05f, 0.05f, 0.05f), Color.Purple);
            DrawClientEntity(0x142851450L, 0x220L, new Vector3D(-0.3f, -0.1f, -0.3f), new Vector3D(0.3f, 0.5f, 0.3f), Color.Plum);
            DrawClientEntity(0x142788790L, 0x220L, new Vector3D(-0.25f, -0.25f, -0.25f), new Vector3D(0.25f, 0.25f, 0.25f), Color.Purple);
        }

        public void DrawClientEntity(long basePtr, long transOff, Vector3D min, Vector3D max, Color drawColor)
        {
            long num = basePtr + 0x60L;
            long address = overlay.Mem.ReadInt64(num);
            if (overlay.IsValidPtr(address))
            {
                do
                {
                    RadarData data;
                    Vector3D origin = new Vector3D();
                    Matrix4x4 m = overlay.Mem.ReadMatrix4x4(address + transOff);
                    origin.X = m.M41;
                    origin.Y = m.M42;
                    origin.Z = m.M43;
                    if (((origin.X == 0f) || (origin.Y == 0f)) || (origin.Z == 0f))
                    {
                        break;
                    }
                    AxisAlignedBox aabb = new AxisAlignedBox();
                    aabb.Init(min, max);
                    aabb.Setup(origin, m);
                    DrawAxisAlignedBoundingBox(new Player(), aabb, false, drawColor, true);
                    data.Pos = new Vector2D(overlay.LocalPlayer.Position.X - origin.X, overlay.LocalPlayer.Position.Z - origin.Z);
                    data.Team = 3;
                    data.Type = 2;
                    data.Rotation = 0;
                    data.Owner = null;
                    toDrawOnRadar.Add(data);
                    address = overlay.Mem.ReadInt64(address);
                }
                while (overlay.IsValidPtr(address));
            }
        }

        public void DrawSkeleton(Player p)
        {
            Vector3D headBone = overlay.WorldToScreen(p.Skeleton.BoneHead);
            Vector3D neckBone = overlay.WorldToScreen(p.Skeleton.BoneNeck);
            Vector3D leftShoulderBone = overlay.WorldToScreen(p.Skeleton.BoneLeftShoulder);
            Vector3D rightShoulderBone = overlay.WorldToScreen(p.Skeleton.BoneRightShoulder);
            Vector3D leftElbowRollBone = overlay.WorldToScreen(p.Skeleton.BoneLeftElbowRoll);
            Vector3D rightElbowRollBone = overlay.WorldToScreen(p.Skeleton.BoneRightElbowRoll);
            Vector3D leftHandBone = overlay.WorldToScreen(p.Skeleton.BoneLeftHand);
            Vector3D rightHandBone = overlay.WorldToScreen(p.Skeleton.BoneRightHand);
            Vector3D spineBone = overlay.WorldToScreen(p.Skeleton.BoneSpine);
            Vector3D leftKneeBone = overlay.WorldToScreen(p.Skeleton.BoneLeftKnee);
            Vector3D rightKneeBone = overlay.WorldToScreen(p.Skeleton.BoneRightKnee);
            Vector3D leftFootBone = overlay.WorldToScreen(p.Skeleton.BoneLeftFoot);
            Vector3D rightFootBone = overlay.WorldToScreen(p.Skeleton.BoneRightFoot);
            if (overlay.CheckVectorIsZero(headBone, neckBone, leftShoulderBone, rightShoulderBone, leftElbowRollBone, rightElbowRollBone, leftHandBone, rightHandBone, spineBone, leftKneeBone, rightKneeBone, leftFootBone, rightFootBone) && p.IsValid)
            {
                overlay.solidColorBrush.Color = Color.Orange;
                overlay.setBrushAlpha(0.5f);
                overlay.device.DrawLine(new Vector2(headBone.X, headBone.Y), new Vector2(leftShoulderBone.X, leftShoulderBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(neckBone.X, neckBone.Y), new Vector2(leftShoulderBone.X, leftShoulderBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(leftShoulderBone.X, leftShoulderBone.Y), new Vector2(leftElbowRollBone.X, leftElbowRollBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(leftElbowRollBone.X, leftElbowRollBone.Y), new Vector2(leftHandBone.X, leftHandBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(neckBone.X, neckBone.Y), new Vector2(rightShoulderBone.X, rightShoulderBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(rightShoulderBone.X, rightShoulderBone.Y), new Vector2(rightElbowRollBone.X, rightElbowRollBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(rightElbowRollBone.X, rightElbowRollBone.Y), new Vector2(rightHandBone.X, rightHandBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(neckBone.X, neckBone.Y), new Vector2(spineBone.X, spineBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(spineBone.X, spineBone.Y), new Vector2(leftKneeBone.X, leftKneeBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(spineBone.X, spineBone.Y), new Vector2(rightKneeBone.X, rightKneeBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(leftKneeBone.X, leftKneeBone.Y), new Vector2(leftFootBone.X, leftFootBone.Y), overlay.solidColorBrush, 3f);
                overlay.device.DrawLine(new Vector2(rightKneeBone.X, rightKneeBone.Y), new Vector2(rightFootBone.X, rightFootBone.Y), overlay.solidColorBrush, 3f);
            }
        }

        public void RegisterTextBox(int X, int Y, int Width, string Name, string Text = "")
        {
            textBoxes.Add(new TextBox(X, Y, Width, Name, Text));
        }

        public void DrawOnRadar(Vector2D pos, Rectangle radar, int team, float rotation, int type = 0, Player player = null)
        {
            pos = new Vector2D(pos.X / 0.75f, pos.Y / 0.75f);
            Vector2 center = new Vector2(radar.Center.X + pos.X, radar.Center.Y + pos.Y);
            float num = -overlay.LocalPlayer.Yaw;
            double num2 = Math.Cos((double)num);
            double num3 = Math.Sin((double)num);
            double num4 = center.X - radar.Center.X;
            double num5 = center.Y - radar.Center.Y;
            center.X = radar.Center.X + ((float)((num4 * num2) - (num5 * num3)));
            center.Y = radar.Center.Y + ((float)((num4 * num3) + (num5 * num2)));
            if (((Math.Abs(center.X) < (radar.Width + radar.X)) && (Math.Abs(center.X) > radar.X)) && ((Math.Abs(center.Y) < (radar.Height + radar.Y)) && (Math.Abs(center.Y) > radar.Y)))
            {
                if (type == 0)
                {
                    //overlay.device.DrawEllipse(new Ellipse(new Vector2(center.X, center.Y), Settings.minimapPlayerRad, Settings.minimapPlayerRad), overlay.solidColorBrush);
                    if (team == overlay.LocalPlayer.Team) DrawBitmap(overlay.playerFriendly, overlay.MakeRectangle(center.X - 6, center.Y - 6, 12, 12), rotation);
                    else DrawBitmap(overlay.playerEnemy, overlay.MakeRectangle(center.X - 6, center.Y - 6, 12, 12), rotation);
                }
                else if (type == 1)
                {
                    //overlay.device.DrawRectangle(new RectangleF(center.X - ((Settings.minimapPlayerRad * 3) / 2), center.Y - ((Settings.minimapPlayerRad * 3) / 2), (Settings.minimapPlayerRad * 3), (Settings.minimapPlayerRad * 3)), overlay.solidColorBrush);
                    if (player.Team != overlay.LocalPlayer.Team)
                        DrawBitmap(overlay.genericVehicle, overlay.MakeRectangle(center.X - 4, center.Y - 4, 8, 8), 0);
                    else
                        DrawBitmap(overlay.genericVehicleFriendly, overlay.MakeRectangle(center.X - 4, center.Y - 4, 8, 8), 0);
                }
                else if (type == 2)
                {
                    //overlay.device.DrawEllipse(new Ellipse(new Vector2(center.X - (Settings.minimapPlayerRad / 4), center.Y - (Settings.minimapPlayerRad / 4)), (Settings.minimapPlayerRad / 2), (Settings.minimapPlayerRad / 2)), overlay.solidColorBrush);
                    DrawBitmap(overlay.entity, overlay.MakeRectangle(center.X - 4, center.Y - 4, 8, 8), 0);
                }
            }
        }

        public void DrawText(string message, int x, int y, int W, int H, bool center, TextFormat format, Color color)
        {
            DrawText(message, x, y, W, H, center, format, color, Color.Black);
        }

        public void DrawText(string message, int x, int y, int W, int H, bool center, TextFormat format, Color color, Color backColor)
        {
            int num = x;
            int num2 = y;
            if (center)
            {
                num = (int)((x - ((message.Length * 12f) / 2f)) + 0.5f);
                num2 = y + ((int)3f);
            }
            RectangleF layoutRect = new RectangleF
            {
                Height = H,
                Width = W
            };
            overlay.solidColorBrush.Color = backColor;
            layoutRect.X = num + 1;
            layoutRect.Y = num2;
            overlay.device.DrawText(message, format, layoutRect, overlay.solidColorBrush);
            layoutRect.X = num;
            layoutRect.Y = num2 + 1;
            overlay.device.DrawText(message, format, layoutRect, overlay.solidColorBrush);
            layoutRect.X = num - 1;
            layoutRect.Y = num2;
            overlay.device.DrawText(message, format, layoutRect, overlay.solidColorBrush);
            layoutRect.X = num;
            layoutRect.Y = num2 - 1;
            overlay.device.DrawText(message, format, layoutRect, overlay.solidColorBrush);
            layoutRect.X = num;
            layoutRect.Y = num2;
            overlay.solidColorBrush.Color = color;
            overlay.device.DrawText(message, format, layoutRect, overlay.solidColorBrush);
        }

        public void DrawTextBoxes()
        {
            Color4 color = overlay.solidColorBrush.Color;
            foreach (TextBox box in textBoxes)
            {
                if (box.Render)
                {
                    overlay.solidColorBrush.Color = Color.Gray;
                    overlay.device.DrawRectangle(overlay.MakeRectangle(box.X, box.Y, box.Width, 20f), overlay.solidColorBrush);
                    DrawText(box.Text, box.X + 4, box.Y + 1, box.Width, 20, false, overlay.smallText, Color.LightGray);
                    if (overlay.MakeRectangle(box.X, box.Y, box.Width, 20f).Contains(Mouse.MouseDownPos().X, Mouse.MouseDownPos().Y) && (Mouse.timeSinceLastClick > 0))
                    {
                        Mouse.timeSinceLastClick = 0;
                        box.Selected = true;
                    }
                    else if (!(overlay.MakeRectangle(box.X, box.Y, box.Width, 20f).Contains(Mouse.MouseDownPos().X, Mouse.MouseDownPos().Y) || (Mouse.MouseDownPos().X == 0f)))
                    {
                        box.Selected = false;
                    }
                    Color lightGray = Color.LightGray;
                    lightGray.A = 60;
                    overlay.solidColorBrush.Color = (Color4)lightGray;
                    if (box.Selected)
                    {
                        overlay.device.FillRectangle(overlay.MakeRectangle((box.X + 1), (box.Y + 1), (box.Width - 2), 18f), overlay.solidColorBrush);
                        if ((overlay.IsKeyDown(Keyboard.lastKeyCode) && (Keyboard.lastKeyCode != 160)) && ((box.Text.Count<char>() == 0) || (Keyboard.lastKey != box.Text.Last<char>())))
                        {
                            box.Text = box.Text + Keyboard.lastKey;
                        }
                    }
                }
            }
            overlay.solidColorBrush.Color = color;
        }

        public void DrawWarn(int X, int Y, int Width, int Height, string PlayerName)
        {
            Rectangle rectangle = new Rectangle
            {
                X = X - 1,
                Y = Y - 1,
                Width = Width + 1,
                Height = Height + 1
            };
            overlay.solidColorBrush.Color = Color.Gray;
            overlay.device.DrawRectangle(rectangle, overlay.solidColorBrush);
            overlay.solidColorBrush.Color = new Color(50, 50, 50, 200);
            overlay.device.FillRectangle(overlay.MakeRectangle((rectangle.X + 1), (rectangle.Y + 1), (rectangle.Width - 2), (rectangle.Height - 2)), overlay.solidColorBrush);
            DrawText(string.Format("Spectator: {0}", PlayerName), X + 5, Y + 5, 150, 20, false, overlay.smallText, Color.White);
            DrawText("Watching You!", X + 5, Y + 0x23, 150, 20, false, overlay.smallText, Color.White);
        }

        public void DrawAxisAlignedBoundingBox(Player p, AxisAlignedBox aabb = null, bool isVehicle = false, Color drawColor = new Color(), bool c = false)
        {
            if (aabb == null)
            {
                aabb = p.GetAAB();
                if (p.SoldierTransform.M44 == 100f)
                {
                    Matrix3x3 matrixx = new Matrix3x3(new[] { p.SoldierTransform.M11, p.SoldierTransform.M12, p.SoldierTransform.M13, p.SoldierTransform.M21, p.SoldierTransform.M22, p.SoldierTransform.M23, p.SoldierTransform.M31, p.SoldierTransform.M32, p.SoldierTransform.M33 });
                    matrixx.Transpose();
                    float[] vals = new float[0x10];
                    vals[0] = matrixx.M11;
                    vals[1] = matrixx.M12;
                    vals[2] = matrixx.M13;
                    vals[3] = p.SoldierTransform.M41;
                    vals[4] = matrixx.M21;
                    vals[5] = matrixx.M22;
                    vals[6] = matrixx.M23;
                    vals[7] = p.SoldierTransform.M42;
                    vals[8] = matrixx.M31;
                    vals[9] = matrixx.M32;
                    vals[10] = matrixx.M33;
                    vals[11] = p.SoldierTransform.M43;
                    vals[15] = 1f;
                    p.SoldierTransform = new Matrix4x4(vals);
                }
            }
            if (p.Team == overlay.LocalPlayer.Team)
            {
                overlay.solidColorBrush.Color = isVehicle ? Color.CadetBlue : Color.LimeGreen;
            }
            else if (!p.IsOccluded)
            {
                overlay.solidColorBrush.Color = Color.Yellow;
            }
            else
            {
                overlay.solidColorBrush.Color = isVehicle ? Color.OrangeRed : Color.Red;
            }

            if (c)
                overlay.solidColorBrush.Color = drawColor;

            for (int i = 0; i < 8; i++)
            {
                aabb.corners[i] = overlay.WorldToScreen(aabb.corners[i]);
                if (aabb.corners[i].Z < 0.5)
                {
                    return;
                }
            }

            overlay.setBrushAlpha(0.5f);
            overlay.device.DrawLine(new Vector2(aabb.corners[0].X, aabb.corners[0].Y), new Vector2(aabb.corners[1].X, aabb.corners[1].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[5].X, aabb.corners[5].Y), new Vector2(aabb.corners[6].X, aabb.corners[6].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[0].X, aabb.corners[0].Y), new Vector2(aabb.corners[5].X, aabb.corners[5].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[1].X, aabb.corners[1].Y), new Vector2(aabb.corners[6].X, aabb.corners[6].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[3].X, aabb.corners[3].Y), new Vector2(aabb.corners[2].X, aabb.corners[2].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[4].X, aabb.corners[4].Y), new Vector2(aabb.corners[7].X, aabb.corners[7].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[3].X, aabb.corners[3].Y), new Vector2(aabb.corners[4].X, aabb.corners[4].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[2].X, aabb.corners[2].Y), new Vector2(aabb.corners[7].X, aabb.corners[7].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[3].X, aabb.corners[3].Y), new Vector2(aabb.corners[0].X, aabb.corners[0].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[4].X, aabb.corners[4].Y), new Vector2(aabb.corners[5].X, aabb.corners[5].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[1].X, aabb.corners[1].Y), new Vector2(aabb.corners[2].X, aabb.corners[2].Y), overlay.solidColorBrush);
            overlay.device.DrawLine(new Vector2(aabb.corners[6].X, aabb.corners[6].Y), new Vector2(aabb.corners[7].X, aabb.corners[7].Y), overlay.solidColorBrush);
        }

        public void DrawBitmap(Bitmap bitmap, RectangleF rec, float rotationDeg)
        {
            Matrix3x2 trans = overlay.device.Transform;
            overlay.device.Transform = Matrix.Transformation2D(Vector2.Zero, 0f, new Vector2(1f, 1f), rec.Center, rotationDeg, Vector2.Zero);
            overlay.device.DrawBitmap(bitmap, rec, 255, BitmapInterpolationMode.Linear);
            overlay.device.Transform = trans;
        }

        public Bitmap GetBitmap(String name)
        {
            ImagingFactory imagingFactory = new ImagingFactory();
            NativeFileStream fileStream = new NativeFileStream(Directory.GetCurrentDirectory() + "\\" + name, NativeFileMode.Open, NativeFileAccess.Read);
            BitmapDecoder bitmapDecoder = new BitmapDecoder(imagingFactory, fileStream, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode frame = bitmapDecoder.GetFrame(0);
            FormatConverter converter = new FormatConverter(imagingFactory);
            converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);
            return SharpDX.Direct2D1.Bitmap.FromWicBitmap(overlay.device, converter);
        }

        public TextBox GetTextBox(string Name)
        {
            foreach (TextBox box in textBoxes)
            {
                if (box.Name == Name)
                {
                    return box;
                }
            }
            return null;
        }

        public void DrawFillRect(int X, int Y, int Width, int Height, Color color)
        {
            RectangleF rect = new RectangleF
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height
            };
            overlay.solidColorBrush.Color = (Color4)color;
            overlay.setBrushAlpha(0.5f);
            overlay.device.FillRectangle(rect, overlay.solidColorBrush);
        }
    }
}
