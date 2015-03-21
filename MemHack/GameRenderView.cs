using System;
using MemLibs;

namespace MemHack
{
    public class GameRenderView
    {
        Memory Memory = new Memory();

        public GameRenderView()
        {
            Memory.AttackProcess("bf4");
        }

        private Int64 pRenderView
        {
            get
            {
                Int64 pRenderer = Memory.ReadInt64(sAddresses.GameRenderer);

                return Memory.ReadInt64(pRenderer + sOffsets.GameRenderer.ClassRenderView);
            }
        }


        public float Aspect
        {
            get
            {
                return Memory.ReadFloat(pRenderView + sOffsets.GameRenderer.RenderView.Aspect);
            }
        }


        public float FovY
        {
            get
            {
                return Memory.ReadFloat(pRenderView + sOffsets.GameRenderer.RenderView.FovY);
            }
        }


        public float FovX
        {
            get
            {
                return Memory.ReadFloat(pRenderView + sOffsets.GameRenderer.RenderView.FovX);
            }
        }


        public Matrix4x4 GetViewMatrix()
        {
            return Memory.ReadMatrix4x4(pRenderView + sOffsets.GameRenderer.RenderView.ViewProjectionMatrix);
        }

        public Matrix4x4 GetViewMatrixInverse()
        {
            return Memory.ReadMatrix4x4(pRenderView + sOffsets.GameRenderer.RenderView.ViewMatrixInverse);
        }


    }
}
