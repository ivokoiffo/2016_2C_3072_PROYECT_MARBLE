using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Utils;
namespace TGC.Group.Model
{
    abstract class  Luz
    {
        public float Duracion { get; set; }
        abstract public void aplicarEfecto(TgcMesh mesh,Vector3 posicionCamara, Vector3 direccionLuz);
        public void deshabilitarEfecto(TgcMesh mesh) {
            mesh.Effect = TgcShaders.Instance.TgcMeshShader;
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
        }
    }
}
