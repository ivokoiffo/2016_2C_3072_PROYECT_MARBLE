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
    class Farol : Luz
    {
        public Farol()
        {
            VelocidadConsumo = 0.009f;
            setMaximaEnergia();
        }
        public Farol(float velocidadConsumo)
        {
            VelocidadConsumo = velocidadConsumo;
            setMaximaEnergia();
        }
        public override void aplicarEfecto(TgcMesh mesh, Vector3 posicionCamara, Vector3 direccionLuz)
        {
            mesh.Effect = TgcShaders.Instance.TgcMeshSpotLightShader;
            //El Technique depende del tipo RenderType del mesh
            // mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
            mesh.Technique = "DIFFUSE_MAP";
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.FromArgb(0, 255, 207, 46)));
            mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(posicionCamara));
            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(posicionCamara));
            mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(direccionLuz));
            mesh.Effect.SetValue("lightIntensity", Energia);
            mesh.Effect.SetValue("lightAttenuation", 0.005f);
            mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(30f));
            mesh.Effect.SetValue("spotLightExponent", 15f);

            //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
            mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
            mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.FromArgb(0, 255, 207, 46)));
            mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.FromArgb(0, 255, 207, 46)));
            mesh.Effect.SetValue("materialSpecularExp", 10000f);
        }

        public override string getNombreYEnergia()
        {
            return "Energia de Farol es de: " + (Energia + 0.55f) * 100;
        }
        public override float getConversion()
        {
            return 1f;
        }

        public override void setMaximaEnergia()
        {
            Energia = 0.45f;
        }

        public override string descripcion()
        {
            return "Farol";
        }
    }
}
