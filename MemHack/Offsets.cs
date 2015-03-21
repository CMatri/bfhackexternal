using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemLibs;

namespace MemHack
{
    public struct sAddresses
    {
        public static Int64 ClientGameContext = 0x14247e940; // MAIN ADDRESS
        public static Int64 WorldRenderSettings = 0x14247ffe8;
        public static Int64 GameRenderer = 0x14247fea0; // MAIN ADDRESS
        public static Int64 DXRenderer = 0x1425459d0; // MAIN ADDRESS
        public static Int64 ViewAngles = 0x1421c1950; // MAIN ADDRESS
        public static Int64 StrangeJD62 = 0x1424871e0; // MAIN ADDRESS
        public static Int64 StrangePlayer = 0x01E0; // StrangePtr->ClientPlayer
        public static Int64 TypeInfoClientVehicleEntity = 0x1427FAE60; // MAIN TYPE INFO ADDRESS
    }

    public struct sOffsets
    {
        public static Int64 ClassClientPlayerManager = 0x60; // PTR

        public struct ClientPlayerManger
        {
            public static Int64 SpecArray = 0x308; // PTR
            public static Int64 LocalPlayer = 0x540; // PTR
            public static Int64 PlayerArray = 0x548; // PTR
        }

        public struct Player
        {
            public static Int64 Name = 0x40; // STRING 20 CHARS
            public static Int64 Team = 0x13CC; // INT32
            public static Int64 ClassSoldier = 0x14d0; // PTR

            public struct ClientVehicleEntity
            {
                public static Int64 RenderFlags = 0x443; // INT32
                public static Int64 ComponentList = 0x38; // PTR
                public static Int64 Comp1 = 0x0A; // BYTE
                public static Int64 Comp2 = 0x09; // BYTE
            }

            public struct VehicleEntityData
            {
                public static Int64 ClassVehicleHudData = 0x00D0;

                public static Int64 ClassFrontHealthZone = 0x0140;
                public static Int64 ClassRearHealthZone = 0x0160;
                public static Int64 ClassLeftHealthZone = 0x0180;
                public static Int64 ClassRightHealthZone = 0x01A0;
                public static Int64 ClassTopHealthZone = 0x01C0;

                public struct VehicleHudData
                {

                }

                public struct VehicleHealthZone
                {
                    public static Int64 MaxHealth = 0x0000; // FLOAT
                    public static Int64 MaxShieldHealth = 0x0004; // FLOAT
                    public static Int64 MinDamageAngle = 0x0008; // FLOAT
                    public static Int64 DamageAngleMultiplier = 0x000C; // FLOAT
                    public static Int64 DamageAngleMultiplierCurve = 0x0010; // INT64
                    public static Int64 VerticalFactor = 0x0018; // FLOAT
                    public static Int64 UseDampingAngleCalculation = 0x001C; // INT64
                }
            }

            public struct Soldier
            {
                public static Int64 ClassHealth = 0x140; // PTR
                public static Int64 ClassPlayerPosition = 0x490; // PTR
                public static Int64 ClassBoneCollisionComponent = 0x100; // PTR
                public static Int64 ClassClientRagDollComponent = 0x560; // PTR
                public static Int64 ClassClientSoldierWeaponsComponent = 0x550; // PTR
                public static Int64 ClassClientAntAnimatableComponent = 0x1E0; // PTR
                public static Int64 SoldierPose = 0x4F0; // INT32
                public static Int64 RenderFlags = 0x4F4; // INT32
                public static Int64 Sprinting = 0x590; // BYTE
                public static Int64 IsOccluded = 0x591; // BYTE
                public static Int64 Yaw = 0x4D8; // FLOAT
                public static Int64 Pitch = 0x4DC; // FLOAT
                public static Int64 WeaponTransform = 0xD0; // MATRIX
                public static Int64 ActiveSlot = 0x9A8; // INT32
                public static Int64 LastActiveSlot = 0x9AC; // INT32
                public static Int64 LastActiveWeaponSlot = 0x9B0; // INT32
                public static Int64 CurrentZoomLevel = 0x9D0; // INT32
                public static Int64 ZoomLevelMax = 0x9D4; // INT32
                public static Int64 ZeroingDistanceLevel = 0x9D8; // INT32
                public static Int64 NoMagnifierAccessory = 0x9DC; // BYTE
                public static Int64 TimeSinceLastShot = 0xCE8; // FLOAT
                public static Int64 InVehicle = 0x1548; // BYTE

                public struct ClientAntAnimatableComponent
                {
                    public static Int64 ClassGameAnimatabe = 0x68; // PTR

                    public struct GameAnimatable
                    {
                        public static Int64 HadVisualUpdate = 0xD4; // BOOL
                    }
                }

                public struct BoneCollisionComponent
                {
                    public static Int64 Skeleton = 0x50; // INT64
                    public static Int64 ClassBoneTransformInfo = 0x58; // PTR

                    public struct BoneTransformInfo
                    {
                        public static Int64 Matrix_Transform = 0x0; // MATRIX4x4
                        public static Int64 Vec4_Position = 0x40; // VECTOR4D
                    } // Size: 0x50
                }

                public struct ClientRagDollComponent
                {
                    public static Int64 ClassUpdatePoseResultData = 0x88; // Int64 - Offset
                    public static Int64 Transform = 0x5D0; // MATRIX4x4

                    public struct UpdatePoseResultData
                    {
                        public static Int64 LocalTransform = 0x0; // QUATTRANSFORM
                        public static Int64 WorldTransform = 0x8; // QUATTRANSFORM
                        public static Int64 ActiveWorldTransforms = 0x28; // QUATTRANSFORM
                        public static Int64 ActiveLocalTransforms = 0x30; // QUATTRANSFORM
                        public static Int64 Slot = 0x38; // INT32
                        public static Int64 ReaderIndex = 0x3C; // INT32
                        public static Int64 ValidTransforms = 0x40; // BYTE
                        public static Int64 PoseUpdateEnabled = 0x41; // BYTE
                        public static Int64 PoseNeeded = 0x42; // BYTE
                    }
                }

                public struct PlayerHealth
                {
                    public static Int64 CurPlayerHealth = 0x20; // FLOAT
                    public static Int64 MaxPlayerHealth = 0x24; // FLOAT
                    public static Int64 CurVehicleHealth = 0x0038; // FLOAT
                }

                public struct PlayerPosition
                {
                    public static Int64 XPlayer = 0x30; // FLOAT
                    public static Int64 YPlayer = 0x34; // FLOAT
                    public static Int64 ZPlayer = 0x38; // FLOAT
                }

                public struct ClientSoldierWeaponsComponent
                {
                    public static Int64 ClassClientAnimatedSoldierWeaponHandler = 0x0890; // PTR
                    public static Int64 ActiveSlot = 0x0A98; // INT32

                    public struct ClientAnimatedSoldierWeaponHandler
                    {
                        public static Int64 ClassClientSoldierWeapon = 0x0; // PTR

                        public struct ClientSoldierWeapon
                        {
                            public static Int64 ClassClientSoldierAimingSimulation = 0x4988; // PTR
                            public static Int64 ClassWeaponFiring = 0x49C0; // PTR
                            public static Int64 ClassClientWeapon = 0x49A8; // PTR

                            public struct ClientSoldierAimingSimulation
                            {
                                public static Int64 ClassAimAssist = 0x10; // PTR
                                public static Int64 ClassAimingSimulationData = 0x0; // PTR
                                public static Int64 ClassAimingSimulationEnvironment = 0x8; // PTR
                                public static Int64 Yaw = 0x18; // FLOAT
                                public static Int64 Pitch = 0x1C; // FLOAT
                                public static Int64 AimYawTimer = 0x20; // FLOAT
                                public static Int64 AimPitchTimer = 0x24; // FLOAT
                                public static Int64 Sway = 0x28; // VECTOR2D
                                public static Int64 ZoomLevel = 0x68; // FLOAT

                                public struct AimAssist
                                {
                                    public static Int64 Data = 0x0; // INT64
                                    public static Int64 Yaw = 0x14; // FLOAT
                                    public static Int64 Pitch = 0x18; // FLOAT
                                }

                                public struct AimingSimulationData
                                {
                                    public static Int64 ClassZoomLevelData = 0x10; // PTR-PTR
                                    public static Int64 ClassSoldierAimAssistData = 0x18; // PTR
                                    public static Int64 ClassAimingPoseStand = 0x20; // PTR
                                    public static Int64 ClassAimingPoseCrouch = 0x38; // PTR
                                    public static Int64 ClassAimingPoseProne = 0x50; // PTR
                                    public static Int64 ReturnToZoomAfterReload = 0x90; // PTR
                                    public static Int64 GameDataContainer = 0x0; // 17 CHAR
                                    public static Int64 ZoomTransitionTime = 0x68; // FLOAT
                                    public static Int64 ZoomTransitionTimeArray = 0x70; // FLOAT
                                    public static Int64 FovDelayTime = 0x78; // FLOAT
                                    public static Int64 FovTransitionTime = 0x7C; // FLOAT
                                    public static Int64 AimingRange = 0x80; // FLOAT
                                    public static Int64 LockAimToTargetSpeed = 0x84; // FLOAT
                                    public static Int64 Modifiers = 0x88; // FLOAT

                                    public struct ZoomLevelData
                                    {
                                        public static Int64 ClassFOVTransitionData = 0x18; // PTR
                                        public static Int64 ZoomLevelActivateEventType = 0x78; // INT32
                                        public static Int64 FieldOfView = 0x10; // FLOAT
                                        public static Int64 FieldOfViewSP = 0x14; // FLOAT
                                        public static Int64 LookSpeedMultiplier = 0x20; // FLOAT
                                        public static Int64 SprintLookSpeedMultiplier = 0x24; // FLOAT
                                        public static Int64 MoveSpeedMultiplier = 0x28; // FLOAT
                                        public static Int64 SwayPitchMagnitude = 0x2C; // FLOAT
                                        public static Int64 SwayYawMagnitude = 0x30; // FLOAT
                                        public static Int64 SupportedSwayPitchMagnitude = 0x34; // FLOAT
                                        public static Int64 SupportedSwayYawMagnitude = 0x38; // FLOAT
                                        public static Int64 SuppressedSwayPitchMagnitude = 0x3C; // FLOAT
                                        public static Int64 SuppressedSwayYawMagnitude = 0x40; // FLOAT
                                        public static Int64 SuppressedSwayMinFactor = 0x44; // FLOAT
                                        public static Int64 TimePitchMultiplier = 0x48; // FLOAT
                                        public static Int64 TimeYawMultiplier = 0x4C; // FLOAT
                                        public static Int64 DispersionMultiplier = 0x50; // FLOAT
                                        public static Int64 DispersionRotation = 0x54; // FLOAT
                                        public static Int64 RecoilMultiplier = 0x58; // FLOAT
                                        public static Int64 RecoilFOVMultiplier = 0x5C; // FLOAT
                                        public static Int64 CameraImpulseMultiplier = 0x60; // FLOAT
                                        public static Int64 StartFadeToBlackAtTime = 0x64; // FLOAT
                                        public static Int64 FadeToBlackDuration = 0x68; // FLOAT
                                        public static Int64 StartFadeFromBlackAtTime = 0x6C; // FLOAT
                                        public static Int64 FadeFromBlackDuration = 0x70; // FLOAT
                                        public static Int64 ScreenExposureAreaScale = 0x74; // FLOAT
                                        public static Int64 AttractYawStrength = 0x7C; // FLOAT
                                        public static Int64 AttractPitchStrength = 0x80; // FLOAT
                                        public static Int64 AllowFieldOfViewScaling = 0x84; // BOOL
                                        public static Int64 FadeToBlackInZoomTransition = 0x85; // BOOL
                                        public static Int64 UseFovSpecialisation = 0x86; // BOOL
                                        public static Int64 UseWeaponMeshZoom1P = 0x87; // BOOL

                                        public struct FOVTransitionData
                                        {
                                            public static Int64 TransitionType = 0x10; // ENUM
                                            public static Int64 Shape = 0x14; // FLOAT
                                            public static Int64 StartDelay = 0x18; // FLOAT
                                            public static Int64 StartJump = 0x1C; // FLOAT
                                            public static Int64 EndEarly = 0x20; // FLOAT
                                            public static Int64 Invert = 0x24; // BOOL
                                        }
                                    }

                                    public struct SoldierAimAssistData
                                    {
                                        public static Int64 InputPolynomial = 0x10; // INT64
                                        public static Int64 ZoomedInputPolynomial = 0x18; // INT64
                                        public static Int64 StickyBoxScale = 0x20; // VECTOR3D
                                        public static Int64 StickyDistanceScale = 0x30; // VECTOR3D
                                        public static Int64 SnapBoxScale = 0x40; // VECTOR3D
                                        public static Int64 SnapDistanceScale = 0x50; // VECTOR3D
                                        public static Int64 EyePosOffset = 0x60; // VECTOR3D
                                        public static Int64 AccelerationInputThreshold = 0x70; // FLOAT
                                        public static Int64 AccelerationMultiplier = 0x74; // FLOAT
                                        public static Int64 AccelerationDamping = 0x78; // FLOAT
                                        public static Int64 AccelerationTimeThreshold = 0x7C; // FLOAT
                                        public static Int64 SquaredAcceleration = 0x80; // FLOAT
                                        public static Int64 MaxAcceleration = 0x84; // VECTOR2D
                                        public static Int64 YawSpeedStrength = 0x8C; // FLOAT
                                        public static Int64 PitchSpeedStrength = 0x90; // FLOAT
                                        public static Int64 AttractDistanceFallOffs = 0x94; // VECTOR2D
                                        public static Int64 AttractSoftZone = 0x9C; // FLOAT
                                        public static Int64 AttractUserInputMultiplier = 0xA0; // FLOAT
                                        public static Int64 AttractUserInputMultiplier_NoZoom = 0xA4; // FLOAT
                                        public static Int64 AttractOwnSpeedInfluence = 0xA8; // FLOAT
                                        public static Int64 AttractTargetSpeedInfluence = 0xAC; // FLOAT
                                        public static Int64 AttractOwnRequiredMovementForMaximumAttract = 0xB0; // FLOAT
                                        public static Int64 AttractStartInputThreshold = 0xB4; // FLOAT
                                        public static Int64 AttractMoveInputCap = 0xB8; // FLOAT
                                        public static Int64 AttractYawStrength = 0xBC; // FLOAT
                                        public static Int64 AttractPitchStrength = 0xC0; // FLOAT
                                        public static Int64 MaxToTargetAngle = 0xC4; // FLOAT
                                        public static Int64 MaxToTargetXZAngle = 0xC8; // FLOAT
                                        public static Int64 ViewObstructedKeepTime = 0xCC; // FLOAT
                                        public static Int64 SnapZoomLateralSpeedLimit = 0xD0; // FLOAT
                                        public static Int64 SnapZoomTime = 0xD4; // FLOAT
                                        public static Int64 SnapZoomPostTimeNoInput = 0xD8; // FLOAT
                                        public static Int64 SnapZoomPostTime = 0xDC; // FLOAT
                                        public static Int64 SnapZoomReticlePointPriority = 0xE0; // UINT32
                                        public static Int64 SnapZoomAutoEngageTime = 0xE4; // FLOAT
                                        public static Int64 SnapZoomBreakTimeAtMaxInput = 0xE8; // FLOAT
                                        public static Int64 SnapZoomBreakMaxInput = 0xEC; // FLOAT
                                        public static Int64 SnapZoomBreakMinAngle = 0xF0; // FLOAT
                                        public static Int64 SnapZoomSpamGuardTime = 0xF4; // FLOAT
                                        public static Int64 SoldierBackupSkeletonCollisionData = 0xF8; // INT64
                                        public static Int64 CheckBoneCenterOnlyDistance = 0x100; // FLOAT
                                        public static Int64 DisableForcedTargetRecalcDistance = 0x104; // FLOAT
                                        public static Int64 OverrideAimingRange = 0x108; // FLOAT
                                        public static Int64 OverrideAimingRangeCrouch = 0x10C; // FLOAT
                                        public static Int64 OverrideAimingRangeProne = 0x110; // FLOAT
                                        public static Int64 UseYawAcceleration = 0x114; // BOOL
                                        public static Int64 UsePitchAcceleration = 0x115; // BOOL
                                        public static Int64 SnapZoomUserShorterWeaponTime = 0x116; // BOOL
                                        public static Int64 SnapZoomPostTimeDynamicPoint = 0x117; // BOOL
                                        public static Int64 ForceSoldierBackupSkeletonCollisionUse = 0x118; // BOOL
                                    }

                                    public struct AimingPoseStand
                                    {
                                        public static Int64 MinimumPitch = 0x0; // FLOAT
                                        public static Int64 MaximumPitch = 0x4; // FLOAT
                                        public static Int64 TargetingFOV = 0x8; // FLOAT
                                        public static Int64 AimSteadiness = 0xC; // FLOAT
                                        public static Int64 SpeedMultiplier = 0x10; // FLOAT
                                        public static Int64 RecoilMultiplier = 0x14; // FLOAT
                                    }

                                    public struct AimingPoseCrouch
                                    {
                                        public static Int64 MinimumPitch = 0x0; // FLOAT
                                        public static Int64 MaximumPitch = 0x4; // FLOAT
                                        public static Int64 TargetingFOV = 0x8; // FLOAT
                                        public static Int64 AimSteadiness = 0xC; // FLOAT
                                        public static Int64 SpeedMultiplier = 0x10; // FLOAT
                                        public static Int64 RecoilMultiplier = 0x14; // FLOAT
                                    }

                                    public struct AimingPoseProne
                                    {
                                        public static Int64 MinimumPitch = 0x0; // FLOAT
                                        public static Int64 MaximumPitch = 0x4; // FLOAT
                                        public static Int64 TargetingFOV = 0x8; // FLOAT
                                        public static Int64 AimSteadiness = 0xC; // FLOAT
                                        public static Int64 SpeedMultiplier = 0x10; // FLOAT
                                        public static Int64 RecoilMultiplier = 0x14; // FLOAT
                                    }
                                }

                                public struct AimingSimulationEnvironment
                                {

                                }
                            }

                            public struct WeaponFiring
                            {
                                public static Int64 WeaponSway = 0x78; // PTR
                            }

                            public struct ClientWeapon
                            {

                            }
                        }
                    }
                }
            }
        }

        public struct GameRenderer
        {
            public static Int64 ClassRenderView = 0x60;

            public struct RenderView
            {
                public static Int64 Aspect = 0xC4;
                public static Int64 FovY = 0xB4;
                public static Int64 FovX = 0x250;
                public static Int64 ViewProjectionMatrix = 0x420; // Foot Position
                public static Int64 ViewMatrixInverse = 0x2E0; // Camera
            }
        }

        public struct ViewAngles
        {
            public static Int64 Offset1 = 0x4988;
            public static Int64 Offset2 = 0x10;
            public static Int64 Yaw = 0x14;
            public static Int64 Pitch = 0x18;
        }

        public struct DXRenderer
        {
            public static Int64 ClassScreen = 0x38;

            public struct Screen
            {
                public static Int64 Width = 0x58;
                public static Int64 Height = 0x5C;
            }
        }

        public struct WeaponFiring
        {
            public static Int64 ClassWeaponSway = 0x78; // PTR
            public static Int64 ClassPrimaryFire = 0x128; // PTR
            public static Int64 FiringHolderData = 0x1C8; // PTR
            public static Int64 WeaponState = 0x148; // INT32
            public static Int64 LastWeaponState = 0x14C; // INT32
            public static Int64 NextWeaponState = 0x150; // INT32
            public static Int64 TimeToWait = 0x15C; // FLOAT
            public static Int64 ReloadTimer = 0x160; // FLOAT
            public static Int64 HoldReleaseMinDelay = 0x164; // FLOAT
            public static Int64 RecoilTimer = 0x168; // FLOAT
            public static Int64 RecoilAngleX = 0x16C; // FLOAT
            public static Int64 RecoilAngleY = 0x170; // FLOAT
            public static Int64 RecoilAngleZ = 0x174; // FLOAT
            public static Int64 RecoilFOVAngle = 0x178; // FLOAT
            public static Int64 RecoilTimeMultiplier = 0x17C; // FLOAT
            public static Int64 OverheadDropMultiplier = 0x180; // FLOAT
            public static Int64 PrimaryAmmoToFill = 0x184; // INT32
            public static Int64 ReloadStage = 0x188; // INT32
            public static Int64 ExternalPrimaryMagazinCapacity = 0x194; // INT32
            public static Int64 ProjectilesLoaded = 0x1A0; // INT32
            public static Int64 ProjectilesInMagazines = 0x1A4; // INT32
            public static Int64 NumberOfProjectilesToFire = 0x1A8; // INT32
            public static Int64 HasStoppedFiring = 0x1AC; // BYTE
            public static Int64 PrimaryFireTriggeredLastFrame = 0x1AD; // BYTE
            public static Int64 IsOverheated = 0x1AE; // BYTE
            public static Int64 TickCounter = 0x1B8; // INT32
            public static Int64 FireMode = 0x1BC; // INT32


            public struct WeaponSway
            {

            }

            public struct PrimaryFire
            {
                public static Int64 ClassShotConfigData = 0x10; // PTR

                public struct ShotConfigData
                {
                    public static Int64 ClassPrimaryProjectileData = 0xB0; // PTR
                    public static Int64 ClassSecondaryProjectileData = 0xB8; // PTR
                    public static Int64 ClassSoundAsset = 0x120; // PTR
                    public static Int64 InitialSpeed = 0x88; // FLOAT
                    public static Int64 PrimaryProjectile = 0xC0; // INT64
                    public static Int64 SecondaryProjectile = 0xC8; // INT64
                    public static Int64 SpawnDelay = 0xD0; // FLOAT
                    public static Int64 NumberOfBulletsPerShell = 0xD4; // INT32
                    public static Int64 NumberOfBulletsPerShot = 0xD8; // INT32
                    public static Int64 NumberOfBulletsPerBurst = 0xDC; // INT32
                    public static Int64 RelativeTargetAiming = 0xE0; // BYTE
                    public static Int64 ForceSpawnToCamera = 0xE1; // BYTE
                    public static Int64 SpawnVisualAtWeaponBone = 0xE2; // BYTE
                    public static Int64 ActiveForceSpawnToCamera = 0xE3; // BYTE
                    public static Int64 RateOfFire = 0xE8; // FLOAT

                    public struct PrimaryProjectileData
                    {
                        // MESH
                        public static Int64 InitialAngularVelocity = 0xE0; // VECTOR4D
                        public static Int64 InstantAttachableTestDistance = 0x100; // FLOAT
                        public static Int64 InstantAttachableVisualConvergenceDelay = 0x104; // FLOAT
                        public static Int64 InstantAttachableVisualConvergenceDuration = 0x108; // FLOAT
                        public static Int64 MaxAttachableInclination = 0x10C; // FLOAT
                        public static Int64 UnspawnAfterDetontationDelay = 0x110; // FLOAT
                        public static Int64 IsAttachable = 0x114; // BYTE
                        public static Int64 InstantAttachableTestUnderReticule = 0x115; // BYTE
                        public static Int64 ExtraDamping = 0x116; // BYTE
                        // --

                        public static Int64 Gravity = 0x130; // FLOAT
                        public static Int64 ImpactImpulse = 0x134; // FLOAT
                        public static Int64 DetonationTimeVariantion = 0x138; // FLOAT
                        public static Int64 VehicleDetonationRadius = 0x13C; // FLOAT
                        public static Int64 VehicleDetonationActivationDelay = 0x140; // FLOAT
                        public static Int64 FlyBySoundRadius = 0x144; // FLOAT
                        public static Int64 FlyBySoundSpeed = 0x148; // FLOAT
                        public static Int64 Stamina = 0x14C; // FLOAT
                        public static Int64 DistributeDamageOverTime = 0x150; // FLOAT
                        public static Int64 StartDamage = 0x154; // FLOAT
                        public static Int64 EndDamage = 0x158; // FLOAT
                        public static Int64 DamageFalloffStartDistance = 0x15C; // FLOAT
                        public static Int64 DamageFalloffEndDistance = 0x160; // FLOAT
                        public static Int64 TimeToArmExplosion = 0x16C; // FLOAT
                        public static Int64 FirstFrameTravelDistance = 0x168; // FLOAT
                        public static Int64 HasVehicleDetonation = 0x16C; // BYTE
                        public static Int64 InstantHit = 0x16D; // BYTE
                        public static Int64 StopTrailEffectOnUnspawn = 0x16E; // BYTE
                    }

                    public struct SecondaryProjectileData
                    {
                        public static Int64 Gravity = 0x130; // FLOAT
                        public static Int64 ImpactImpulse = 0x134; // FLOAT
                        public static Int64 DetonationTimeVariantion = 0x138; // FLOAT
                        public static Int64 VehicleDetonationRadius = 0x13C; // FLOAT
                        public static Int64 VehicleDetonationActivationDelay = 0x140; // FLOAT
                        public static Int64 FlyBySoundRadius = 0x144; // FLOAT
                        public static Int64 FlyBySoundSpeed = 0x148; // FLOAT
                        public static Int64 Stamina = 0x14C; // FLOAT
                        public static Int64 DistributeDamageOverTime = 0x150; // FLOAT
                        public static Int64 StartDamage = 0x154; // FLOAT
                        public static Int64 EndDamage = 0x158; // FLOAT
                        public static Int64 DamageFalloffStartDistance = 0x15C; // FLOAT
                        public static Int64 DamageFalloffEndDistance = 0x160; // FLOAT
                        public static Int64 TimeToArmExplosion = 0x16C; // FLOAT
                        public static Int64 FirstFrameTravelDistance = 0x168; // FLOAT
                        public static Int64 HasVehicleDetonation = 0x16C; // BYTE
                        public static Int64 InstantHit = 0x16D; // BYTE
                        public static Int64 StopTrailEffectOnUnspawn = 0x16E; // BYTE
                    }

                    public struct SoundAsset
                    {
                        public static Int64 ClassScopeData = 0x18; // PTR 2 BYTE
                        public static Int64 ClassReferencedData = 0x20; // PTR 2 BYTE

                        public struct ScopeData
                        {
                            public static Int64 ClassSoundScopeStrategyData = 0x18; // PTR
                            public static Int64 Name = 0x10; // STRING 20 CHARS

                            public struct SoundScopeStrategyData
                            {
                                public static Int64 Name = 0x10; // STRING 20 CHARS
                                public static Int64 Stages = 0x18; // STRING 20 CHARS
                            }
                        }

                        public struct ReferenceData
                        {
                            public static Int64 ClassSoundScopeStrategyData = 0x18; // PTR
                            public static Int64 Name = 0x10; // STRING 20 CHARS

                            public struct SoundScopeStrategyData
                            {
                                public static Int64 Name = 0x10; // STRING 20 CHARS
                                public static Int64 Stages = 0x18; // STRING 20 CHARS
                            }
                        }
                    }
                }
            }
        }

        public struct WeaponModifier
        {
            public static Int64 BreathControl = 0x568; // BYTE
            public static Int64 SupportedShooting = 0x41; // BYTE
            public static Int64 UnZoomOnBoltAction = 0x42; // BYTE
            public static Int64 HoldBoltActionUntilZoomRelease = 0x43; // BYTE
        }

        public struct ClientWeapon
        {


            struct ControllableFinder
            {
                public static Int64 ClassControllableResult = 0xA28; // PTR
                public static Int64 VTable = 0x950; // INT64
                public static Int64 LastRayBegin = 0x9B0; // VECTOR4D
                public static Int64 LastRayEnd = 0x9C0; // VECTOR4D

                struct ControllableResult
                {
                    public static Int64 Collision = 0x70; // VECTOR3D
                }
            }
        }

    }
}
