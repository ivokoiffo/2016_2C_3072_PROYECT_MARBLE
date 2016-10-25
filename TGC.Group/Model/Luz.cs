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
    abstract class Luz
    {
        public bool comienzoConShader = true;
        public float tiempoPrimerRender = 0;
        public float tiempoSegundoRender = 0;
        public bool segundoRender = false;
        public bool pasoPrimero = false;
        public float tiempoAcumulado = 0;
        public float duracionInicial;
        public float Duracion { get; set; }
        public float Nombre { get;}
        public float VelocidadConsumo { get; set; } 
        abstract public void aplicarEfecto(TgcMesh mesh,Vector3 posicionCamara, Vector3 direccionLuz);
        public void deshabilitarEfecto(TgcMesh mesh) {
            mesh.Effect = TgcShaders.Instance.TgcMeshShader;
            mesh.Technique = TgcShaders.Instance.getTgcMeshTechnique(mesh.RenderType);
        }/*
        public void getPrimerUpdate(float tiempo) {
            if (comienzoConShader) {
                if (segundoRender == true) {  segundoRender = false; }
                if (primerRender == false) { segundoRender = true; primerRender = true; }
            }
        }
        */
        public void consumir(float tiempo)
        {
            if (pasoPrimero == true)
            {
                if (tiempoPrimerRender == 0)
                {
                    tiempoPrimerRender = tiempo;
                    duracionInicial = Duracion;
                }else{
                    if (tiempoSegundoRender == 0)
                    {
                            tiempoSegundoRender = tiempoPrimerRender + tiempo;
                    }
                }
            } else {
                pasoPrimero = true;
            }
            if (tiempoPrimerRender != 0 && tiempoSegundoRender != 0)
            {
                tiempoAcumulado += tiempo;
                Duracion = getDuracionPorFuncion();
            }
        }

        private float getDuracionPorFuncion()
        {
            var pendiente = tiempoPrimerRender - tiempoSegundoRender;
            return (tiempoAcumulado * pendiente * VelocidadConsumo) + duracionInicial;
        }

        abstract public string getNombreYDuracion();
    }
}
