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
    abstract class Luz
    {
        public bool comienzoConShader = true;
        public float tiempoPrimerRender = 0;
        public float tiempoSegundoRender = 0;
        public bool segundoRender = false;
        public bool pasoPrimero = false;
        public float tiempoAcumulado = 0;
        public float duracionInicial;
        public float Energia { get; set; }
        public float Nombre { get;}
        public float VelocidadConsumo { get; set; }
        abstract public string descripcion();
        abstract public void aplicarEfecto(TgcSkeletalMesh mesh, Vector3 posicionCamara, Vector3 direccionLuz);
        abstract public void aplicarEfecto(TgcMesh mesh,Vector3 posicionCamara, Vector3 direccionLuz);
        public void deshabilitarEfecto(TgcMesh mesh) {
            mesh.Effect = TgcShaders.Instance.TgcMeshShader;
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(TgcMesh.MeshRenderType.DIFFUSE_MAP);
        }
        public void deshabilitarEfecto(TgcSkeletalMesh mesh)
        {
            mesh.Effect = TgcShaders.Instance.TgcSkeletalMeshShader;
            mesh.Technique = TgcShaders.Instance.getTgcSkeletalMeshTechnique(mesh.RenderType);
        }
        public void consumir(float tiempo)
        {
            float ener = 0;
            if (pasoPrimero == true)
            {
                if (tiempoPrimerRender == 0)
                {
                    tiempoPrimerRender = tiempo;
                    duracionInicial = Energia;
                }
                else
                {
                    if (tiempoSegundoRender == 0)
                    {
                        tiempoSegundoRender = tiempoPrimerRender + tiempo;
                    }
                }
            }
            else
            {
                pasoPrimero = true;
            }
            if (tiempoPrimerRender != 0 && tiempoSegundoRender != 0)
            {
                tiempoAcumulado += tiempo;
                ener = getEnergiaPorFuncion();
                if (ener <= 0)
                {
                    Energia = 0;
                }
                else
                {
                    Energia = ener;
                }
            }
            
        }

        private float getEnergiaPorFuncion()
        {
            var pendiente = tiempoPrimerRender - tiempoSegundoRender;
            return (tiempoAcumulado * pendiente * VelocidadConsumo) + duracionInicial;
        }

        abstract public string getNombreYEnergia();
        abstract public void setMaximaEnergia();

        abstract public float getConversion();
        public float getConversionEnergiaABarra()
        {
            return Energia / this.getConversion();
        }
    }
}
