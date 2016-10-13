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
    class Linterna : Luz
    {
        public Linterna(float duracion) {
            Duracion = duracion;
        }
        public override void aplicarEfecto(TgcMesh mesh, Vector3 posicionCamara, Vector3 direccionLuz)
        {
            mesh.Effect = TgcShaders.Instance.TgcMeshSpotLightShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(posicionCamara));
            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(posicionCamara));
            mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(direccionLuz));
            mesh.Effect.SetValue("lightIntensity", 75f);
            mesh.Effect.SetValue("lightAttenuation", 0.5750f);
            mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(36f));
            mesh.Effect.SetValue("spotLightExponent", 7f);

            //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
            mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
            mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularExp", 0f);

            //BAJAR DURACION Y VERIFICAR
        }
    }
}
