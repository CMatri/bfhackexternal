using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MemLibs;
using SharpDX;
using Matrix3x3 = SharpDX.Matrix3x3;

namespace MemHack
{
    public class Player
    {
        private const float SOLDIER_HEIGHT = 1.7f;
        private const float SOLDIER_WIDTH = 0.35f;

        public string Name;
        public string WeaponName;
        public string VehicleName;
        public float Health;
        public float MaxHealth;
        public float VehicleHealth;
        public float VehicleMaxHealth;
        public float Distance_Crosshair;
        public float Distance_3D;
        public float Pitch;
        public float Yaw;
        public float Ammo;
        public float MaxAmmo;
        public float BulletSpeed;
        public float BulletGravity;
        public float VehicleEntry;
        public float VehicleSpeed;
        public Int32 Pose;
        public Int32 Team;
        public Int64 pClient;
        public Int64 pVehicle;
        public Int64 pRenderView;
        public Int64 pOwnerRenderView;
        public bool IsOccluded;
        public Boolean InVehicle;
        public Vector3D Position = new Vector3D();
        public Vector3D Vehicle = new Vector3D();
        public Vector3D Velocity = new Vector3D();
        public Vector2D Sway = new Vector2D();
        public SkeletonStruct Skeleton = new SkeletonStruct();
        public Matrix4x4 SoldierTransform;

        public struct SkeletonStruct
        {
            public Vector3D BoneHead;
            public Vector3D BoneNeck;
            public Vector3D BoneLeftShoulder;
            public Vector3D BoneRightShoulder;
            public Vector3D BoneLeftElbowRoll;
            public Vector3D BoneRightElbowRoll;
            public Vector3D BoneLeftHand;
            public Vector3D BoneRightHand;
            public Vector3D BoneSpine;
            public Vector3D BoneLeftKnee;
            public Vector3D BoneRightKnee;
            public Vector3D BoneLeftFoot;
            public Vector3D BoneRightFoot;
        }

        public bool IsValid
        {
            get
            {
                return (Health >= 0 && Health <= 100 && Team >= 0 && Team < 5);
            }
        }

        public AxisAlignedBox GetAAB()
        {
            Vector3D vMin, vMax;
            AxisAlignedBox returnObj = new AxisAlignedBox();

            switch (Pose)
            {
                case 2:
                    vMax = new Vector3D(SOLDIER_WIDTH, SOLDIER_HEIGHT / 4, SOLDIER_WIDTH);
                    vMin = new Vector3D(-1.200000f, 0.000000f, -SOLDIER_WIDTH);
                    break;
                case 1:
                    vMax = new Vector3D(SOLDIER_WIDTH, SOLDIER_HEIGHT * 2 / 3, SOLDIER_WIDTH);
                    vMin = new Vector3D(-SOLDIER_WIDTH, 0.000000f, -0.500000f);
                    break;
                case 0:
                    vMax = new Vector3D(SOLDIER_WIDTH, SOLDIER_HEIGHT, SOLDIER_WIDTH);
                    vMin = new Vector3D(-SOLDIER_WIDTH, 0.000000f, -SOLDIER_WIDTH);
                    break;
                default:
                    vMax = new Vector3D(SOLDIER_WIDTH, SOLDIER_HEIGHT, SOLDIER_WIDTH);
                    vMin = new Vector3D(-SOLDIER_WIDTH, 0.000000f, -SOLDIER_WIDTH);
                    break;
            }

            returnObj.Init(vMin, vMax);
            returnObj.Setup(Position, SoldierTransform);

            return returnObj;
        }
    }

    public class AxisAlignedBox
    {
        private Vector3D max;
        private Vector3D min;

        public Vector3D[] corners = new Vector3D[8];

        public void Init(Vector3D _min, Vector3D _max)
        {
            min = _min;
            max = _max;

            corners[0] = new Vector3D(min.X, min.Y, min.Z);
            corners[1] = new Vector3D(max.X, min.Y, min.Z);
            corners[2] = new Vector3D(max.X, min.Y, max.Z);
            corners[3] = new Vector3D(min.X, min.Y, max.Z);
            corners[4] = new Vector3D(min.X, max.Y, max.Z);
            corners[5] = new Vector3D(min.X, max.Y, min.Z);
            corners[6] = new Vector3D(max.X, max.Y, min.Z);
            corners[7] = new Vector3D(max.X, max.Y, max.Z);
        }

        public void Setup(Vector3D origin, Matrix4x4 m, bool change = true, float yaw = 0, float pitch = 0, float roll = 0)
        {
            Matrix3x3 yawMat = new Matrix3x3();
            Matrix3x3 pitchMat = new Matrix3x3();
            Matrix3x3 rollMat = new Matrix3x3();

            if (yaw != 0)
            {
                float cosY = (float)Math.Cos(yaw);
                float sinY = (float)Math.Sin(yaw);

                float cosX = (float)Math.Cos(pitch);
                float sinX = (float)Math.Sin(pitch);

                float cosZ = (float)Math.Cos(roll);
                float sinZ = (float)Math.Sin(roll);

                pitchMat = new Matrix3x3(new[]
                {
                    cosX, -sinX, 0,
                    sinX, cosX, 0,
                    0, 0, 1
                });

                yawMat = new Matrix3x3(new[]
                {
                    cosY, 0, sinY,
                    0, 1, 0,
                    -sinY, 0, cosY
                });

                rollMat = new Matrix3x3(new[]
                {
                    1, 0, 0,
                    0, cosZ, -sinZ,
                    0, sinZ, cosZ
                });

                pitchMat.Transpose();
                yawMat.Transpose();
                rollMat.Transpose();
            }

            for (int i = 0; i < 8; ++i)
            {
                Vector3D n = corners[i];

                if (change)
                {
                    if (yaw == 0)
                    {
                        Matrix3x3 m2 = new Matrix3x3(new[]
                        {
                            m.M11, m.M12, m.M13,
                            m.M21, m.M22, m.M23,
                            m.M31, m.M32, m.M33,
                        });

                        m2.Transpose();
                        n *= new MemLibs.Matrix3x3(m2);
                    }
                    else
                    {
                        n *= new MemLibs.Matrix3x3(yawMat);
                        n *= new MemLibs.Matrix3x3(pitchMat);
                        n *= new MemLibs.Matrix3x3(rollMat);
                    }
                }

                corners[i] = new Vector3D(n.X, n.Y, n.Z);
                corners[i] += origin;
            }
        }

        public Vector3D GetCenter()
        {
            return new Vector3D((max.X + min.X) * 0.5f, (max.Y + min.Y) * 0.5f, (max.Z + min.Z) * 0.5f);
        }

        public static Vector3D MatrixToAngles(Matrix4x4 matrix)
        {
            float flYaw, flPitch, flRoll;
            if (matrix.M11 == 1.0f)
            {
                flYaw = (float)Math.Atan2(matrix.M13, matrix.M34);
                flPitch = 0; flRoll = 0;
            }
            else if (matrix.M11 == -1.0f)
            {
                flYaw = (float)Math.Atan2(matrix.M13, matrix.M34);
                flPitch = 0;
                flRoll = 0;
            }
            else
            {
                flYaw = (float)Math.Atan2(-matrix.M31, matrix.M11);
                flPitch = (float)Math.Asin(matrix.M21);
                flRoll = (float)Math.Atan2(-matrix.M23, matrix.M22);
            }

            if (flYaw < 0) flYaw = flYaw + ((float)Math.PI * 2);
            if (flPitch < 0) flPitch = flPitch + ((float)Math.PI * 2);
            if (flRoll < 0) flRoll = flRoll + ((float)Math.PI * 2);

            return new Vector3D(flPitch, flYaw, flRoll);
        }
    }
}
