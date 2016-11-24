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
using TGC.Core.SkeletalAnimation;
using TGC.Core.Utils;

namespace TGC.Group.Model
{
    class Linterna : Luz
    {
        public Linterna(float velocidadConsumo) {
            VelocidadConsumo = velocidadConsumo;
            setMaximaEnergia();
        }
        public Linterna()
        {
            VelocidadConsumo = 100f;
            setMaximaEnergia();
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
            mesh.Effect.SetValue("lightIntensity", Energia);
            mesh.Effect.SetValue("lightAttenuation", 0.5750f);
            mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(45f));
            mesh.Effect.SetValue("spotLightExponent", 20f);

            //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
            mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
            mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularExp", 1000000000000000f);
        }
        public override void aplicarEfecto(TgcSkeletalMesh mesh, Vector3 posicionCamara, Vector3 direccionLuz)
        {
            mesh.Effect = TgcShaders.Instance.TgcSkeletalMeshPointLightShader;
            //El Technique depende del tipo RenderType del mesh
            mesh.Technique = "DIFFUSE_MAP";
            mesh.Effect.SetValue("lightColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("lightPosition", TgcParserUtils.vector3ToFloat4Array(posicionCamara));
            mesh.Effect.SetValue("eyePosition", TgcParserUtils.vector3ToFloat4Array(posicionCamara));
            mesh.Effect.SetValue("spotLightDir", TgcParserUtils.vector3ToFloat3Array(direccionLuz));
            mesh.Effect.SetValue("spotLightAngleCos", FastMath.ToRad(45f));
            mesh.Effect.SetValue("spotLightExponent", 20f);
            mesh.Effect.SetValue("lightIntensity", Energia);
            mesh.Effect.SetValue("lightAttenuation", 0.5750f);
            

            //Cargar variables de shader de Material. El Material en realidad deberia ser propio de cada mesh. Pero en este ejemplo se simplifica con uno comun para todos
            mesh.Effect.SetValue("materialEmissiveColor", ColorValue.FromColor(Color.Black));
            mesh.Effect.SetValue("materialAmbientColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialDiffuseColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularColor", ColorValue.FromColor(Color.White));
            mesh.Effect.SetValue("materialSpecularExp", 1000000000000000f);
        }
        public override float getConversion()
        {
            return 222.2f;
        }

        public override string getNombreYEnergia()
        {
            return "Energia de Linterna es de: "+ Energia;
        }

        public override void setMaximaEnergia()
        {
            Energia = 100;
        }

        public override string descripcion()
        {
            return "Linterna";
        }
    }
}
